using System.Collections;
using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class Telepad : StateMachineComponent<Telepad.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, Telepad, object>.GameInstance
	{
		public StatesInstance(Telepad master)
			: base(master)
		{
		}

		public bool IsColonyLost()
		{
			if (GameFlowManager.Instance != null)
			{
				return GameFlowManager.Instance.IsGameOver();
			}
			return false;
		}

		public void UpdateMeter()
		{
			float timeRemaining = Immigration.Instance.GetTimeRemaining();
			float totalWaitTime = Immigration.Instance.GetTotalWaitTime();
			float positionPercent = Mathf.Clamp01(1f - timeRemaining / totalWaitTime);
			base.master.meter.SetPositionPercent(positionPercent);
		}

		public IEnumerator SpawnExtraPowerBanks()
		{
			int cellTarget = Grid.OffsetCell(Grid.PosToCell(base.gameObject), 1, 2);
			int count = 5;
			for (int i = 0; i < count; i++)
			{
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, MISC.POPFX.EXTRA_POWERBANKS_BIONIC, base.gameObject.transform, new Vector3(0f, 0.5f, 0f));
				KMonoBehaviour.PlaySound(GlobalAssets.GetSound("SandboxTool_Spawner"));
				GameObject gameObject = Util.KInstantiate(Assets.GetPrefab("DisposableElectrobank_RawMetal"), Grid.CellToPosCBC(cellTarget, Grid.SceneLayer.Front) - Vector3.right / 2f);
				gameObject.SetActive(value: true);
				Vector2 initial_velocity = new Vector2((-2.5f + 5f * ((float)i / 5f)) / 2f, 2f);
				if (GameComps.Fallers.Has(gameObject))
				{
					GameComps.Fallers.Remove(gameObject);
				}
				GameComps.Fallers.Add(gameObject, initial_velocity);
				yield return new WaitForSeconds(0.25f);
			}
			yield return new WaitForSeconds(0.35f);
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, ITEMS.LUBRICATIONSTICK.NAME, base.gameObject.transform, new Vector3(0f, 0.5f, 0f));
			KMonoBehaviour.PlaySound(GlobalAssets.GetSound("SandboxTool_Spawner"));
			GameObject gameObject2 = Util.KInstantiate(Assets.GetPrefab("LubricationStick"), Grid.CellToPosCBC(cellTarget, Grid.SceneLayer.Front) - Vector3.right / 2f);
			gameObject2.SetActive(value: true);
			Vector2 initial_velocity2 = new Vector2(3.75f, 2.5f);
			if (GameComps.Fallers.Has(gameObject2))
			{
				GameComps.Fallers.Remove(gameObject2);
			}
			GameComps.Fallers.Add(gameObject2, initial_velocity2);
			yield return 0;
		}
	}

	public class States : GameStateMachine<States, StatesInstance, Telepad>
	{
		public class BonusDeliveryStates : State
		{
			public State pre;

			public State loop;

			public State pst;
		}

		public Signal openPortal;

		public Signal closePortal;

		public Signal idlePortal;

		public State idle;

		public State resetToIdle;

		public State opening;

		public State open;

		public State close;

		public State unoperational;

		public BonusDeliveryStates bonusDelivery;

		private static readonly HashedString[] workingAnims = new HashedString[2] { "working_loop", "working_pst" };

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = idle;
			base.serializable = SerializeType.Both_DEPRECATED;
			root.OnSignal(idlePortal, resetToIdle).EventTransition(GameHashes.BonusTelepadDelivery, bonusDelivery.pre);
			resetToIdle.GoTo(idle);
			idle.Enter(delegate(StatesInstance smi)
			{
				smi.UpdateMeter();
			}).Update("TelepadMeter", delegate(StatesInstance smi, float dt)
			{
				smi.UpdateMeter();
			}, UpdateRate.SIM_4000ms).EventTransition(GameHashes.OperationalChanged, unoperational, (StatesInstance smi) => !smi.GetComponent<Operational>().IsOperational)
				.PlayAnim("idle")
				.OnSignal(openPortal, opening);
			unoperational.PlayAnim("idle").Enter("StopImmigration", delegate(StatesInstance smi)
			{
				smi.master.meter.SetPositionPercent(0f);
			}).EventTransition(GameHashes.OperationalChanged, idle, (StatesInstance smi) => smi.GetComponent<Operational>().IsOperational);
			opening.Enter(delegate(StatesInstance smi)
			{
				smi.master.meter.SetPositionPercent(1f);
			}).PlayAnim("working_pre").OnAnimQueueComplete(open);
			open.OnSignal(closePortal, close).Enter(delegate(StatesInstance smi)
			{
				smi.master.meter.SetPositionPercent(1f);
			}).PlayAnim("working_loop", KAnim.PlayMode.Loop)
				.Transition(close, (StatesInstance smi) => smi.IsColonyLost())
				.EventTransition(GameHashes.OperationalChanged, close, (StatesInstance smi) => !smi.GetComponent<Operational>().IsOperational);
			close.Enter(delegate(StatesInstance smi)
			{
				smi.master.meter.SetPositionPercent(0f);
			}).PlayAnims((StatesInstance smi) => workingAnims).OnAnimQueueComplete(idle);
			bonusDelivery.pre.PlayAnim("bionic_working_pre").OnAnimQueueComplete(bonusDelivery.loop);
			bonusDelivery.loop.PlayAnim("bionic_working_loop", KAnim.PlayMode.Loop).ScheduleAction("SpawnBonusDelivery", 1f, delegate(StatesInstance smi)
			{
				smi.master.StartCoroutine(smi.SpawnExtraPowerBanks());
			}).ScheduleGoTo(3f, bonusDelivery.pst);
			bonusDelivery.pst.PlayAnim("bionic_working_pst").OnAnimQueueComplete(idle);
		}
	}

	[MyCmpReq]
	private KSelectable selectable;

	private MeterController meter;

	private const float MAX_IMMIGRATION_TIME = 120f;

	private const int NUM_METER_NOTCHES = 8;

	private List<MinionStartingStats> minionStats;

	public float startingSkillPoints;

	[Serialize]
	private List<Ref<MinionIdentity>> aNewHopeEvents = new List<Ref<MinionIdentity>>();

	[Serialize]
	private List<Ref<MinionIdentity>> extraPowerBanksEvents = new List<Ref<MinionIdentity>>();

	public static readonly HashedString[] PortalBirthAnim = new HashedString[1] { "portalbirth" };

	public void AddNewBaseMinion(GameObject minion, bool extra_power_banks)
	{
		Ref<MinionIdentity> item = new Ref<MinionIdentity>(minion.GetComponent<MinionIdentity>());
		aNewHopeEvents.Add(item);
		if (extra_power_banks)
		{
			extraPowerBanksEvents.Add(item);
		}
	}

	public void ScheduleNewBaseEvents()
	{
		aNewHopeEvents.RemoveAll((Ref<MinionIdentity> entry) => entry == null || entry.Get() == null);
		extraPowerBanksEvents.RemoveAll((Ref<MinionIdentity> entry) => entry == null || entry.Get() == null);
		Effect a_new_hope = Db.Get().effects.Get("AnewHope");
		for (int num = 0; num < aNewHopeEvents.Count; num++)
		{
			GameObject callback_data = aNewHopeEvents[num].Get().gameObject;
			GameScheduler.Instance.Schedule("ANewHope", 3f + 0.5f * (float)num, delegate(object m)
			{
				GameObject gameObject = m as GameObject;
				if (!(gameObject == null))
				{
					RemoveFromEvents(aNewHopeEvents, gameObject);
					gameObject.GetComponent<Effects>().Add(a_new_hope, should_save: true);
				}
			}, callback_data);
		}
		for (int num2 = 0; num2 < extraPowerBanksEvents.Count; num2++)
		{
			GameObject callback_data2 = extraPowerBanksEvents[num2].Get().gameObject;
			GameScheduler.Instance.Schedule("ExtraPowerBanks", 3f + 4.5f * (float)num2, delegate(object m)
			{
				GameObject gameObject = m as GameObject;
				if (!(gameObject == null))
				{
					RemoveFromEvents(extraPowerBanksEvents, gameObject);
					GameUtil.GetTelepad(ClusterManager.Instance.GetStartWorld().id).Trigger(1982288670);
				}
			}, callback_data2);
		}
	}

	private void RemoveFromEvents(List<Ref<MinionIdentity>> listToRemove, GameObject go)
	{
		for (int num = listToRemove.Count - 1; num >= 0; num--)
		{
			if (listToRemove[num].Get() != null && listToRemove[num].Get() == go.GetComponent<MinionIdentity>())
			{
				listToRemove.RemoveAt(num);
				break;
			}
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		GetComponent<Deconstructable>().allowDeconstruction = false;
		int x = 0;
		int y = 0;
		Grid.CellToXY(Grid.PosToCell(this), out x, out y);
		if (x == 0)
		{
			Debug.LogError("Headquarters spawned at: (" + x + "," + y + ")");
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Components.Telepads.Add(this);
		meter = new MeterController(GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Behind, Grid.SceneLayer.NoLayer, "meter_target", "meter_fill", "meter_frame", "meter_OL");
		meter.gameObject.GetComponent<KBatchedAnimController>().SetDirty();
		base.smi.StartSM();
		ScheduleNewBaseEvents();
	}

	protected override void OnCleanUp()
	{
		Components.Telepads.Remove(this);
		base.OnCleanUp();
	}

	public void Update()
	{
		if (!base.smi.IsColonyLost())
		{
			if (Immigration.Instance.ImmigrantsAvailable && GetComponent<Operational>().IsOperational)
			{
				base.smi.sm.openPortal.Trigger(base.smi);
				selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.NewDuplicantsAvailable, this);
			}
			else
			{
				base.smi.sm.closePortal.Trigger(base.smi);
				selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.Wattson, this);
			}
			if (GetTimeRemaining() < -120f)
			{
				Messenger.Instance.QueueMessage(new DuplicantsLeftMessage());
				Immigration.Instance.EndImmigration();
			}
		}
	}

	public void RejectAll()
	{
		Immigration.Instance.EndImmigration();
		base.smi.sm.closePortal.Trigger(base.smi);
	}

	public void OnAcceptDelivery(ITelepadDeliverable delivery)
	{
		int cell = Grid.PosToCell(this);
		Immigration.Instance.EndImmigration();
		GameObject gameObject = delivery.Deliver(Grid.CellToPosCBC(cell, Grid.SceneLayer.Move));
		MinionIdentity component = gameObject.GetComponent<MinionIdentity>();
		if (component != null)
		{
			ReportManager.Instance.ReportValueWithGameObjectContext(ReportManager.ReportType.PersonalTime, GameClock.Instance.GetTimeSinceStartOfReport(), gameObject, string.Format(UI.ENDOFDAYREPORT.NOTES.PERSONAL_TIME, DUPLICANTS.CHORES.NOT_EXISTING_TASK));
			foreach (MinionIdentity worldItem in Components.LiveMinionIdentities.GetWorldItems(base.gameObject.GetComponent<KSelectable>().GetMyWorldId()))
			{
				worldItem.GetComponent<Effects>().Add("NewCrewArrival", should_save: true);
			}
			MinionResume component2 = component.GetComponent<MinionResume>();
			for (int i = 0; (float)i < startingSkillPoints; i++)
			{
				component2.ForceAddSkillPoint();
			}
			if (component.HasTag(GameTags.Minions.Models.Bionic))
			{
				GameScheduler.Instance.Schedule("BonusBatteryDelivery", 5f, delegate
				{
					Trigger(1982288670);
				});
			}
		}
		base.smi.sm.closePortal.Trigger(base.smi);
	}

	public float GetTimeRemaining()
	{
		return Immigration.Instance.GetTimeRemaining();
	}
}
