using System.Collections.Generic;
using UnityEngine;

namespace Orbitfall;

internal static class StartupRocketBootstrap
{
	private const string StartupCraftName = "OrbitfallStartupCraft";
	private const string StartupInteriorTemplate = "expansion1::interiors/orbitfall_start";

	private static bool attempted;
	private static bool finalized;
	private static GameObject startupCraft;

	internal static bool SuppressTelepadEvents { get; private set; } = true;

	internal static void TryRun()
	{
		if (attempted)
		{
			return;
		}
		attempted = true;
		finalized = false;

		if (!DlcManager.FeatureClusterSpaceEnabled())
		{
			Debug.LogWarning("[Orbitfall] Startup rocket bootstrap skipped because Spaced Out is not active.");
			return;
		}

		ClusterManager clusterManager = ClusterManager.Instance;
		WorldContainer startWorld = clusterManager?.GetStartWorld();
		if (clusterManager == null || startWorld == null)
		{
			Debug.LogWarning("[Orbitfall] Startup rocket bootstrap skipped because start-world data is unavailable.");
			return;
		}

		GameObject telepad = GameUtil.GetTelepad(startWorld.id);
		if (telepad == null)
		{
			Debug.LogWarning("[Orbitfall] Startup rocket bootstrap skipped because no starting telepad was found.");
			return;
		}
		startupCraft = CreateStartupCraft(startWorld);
		if (startupCraft == null)
		{
			Debug.LogError("[Orbitfall] Startup rocket bootstrap failed: could not create startup clustercraft.");
			return;
		}

		CraftModuleInterface craftModuleInterface = startupCraft.GetComponent<CraftModuleInterface>();
		if (craftModuleInterface == null || !EnsurePassengerModule(craftModuleInterface, startWorld))
		{
			Debug.LogError("[Orbitfall] Startup rocket bootstrap failed: could not create/attach a valid passenger module.");
			Object.Destroy(startupCraft);
			return;
		}

		RocketInteriorSizing.Apply("StartupRocketBootstrap");

		GameScheduler.Instance.ScheduleNextFrame("Orbitfall.StartupBootstrap", delegate
		{
			FinalizeBootstrap();
		});
	}

	private static void FinalizeBootstrap()
	{
		if (finalized)
		{
			return;
		}

		ClusterManager clusterManager = ClusterManager.Instance;
		CraftModuleInterface craftModuleInterface = startupCraft != null ? startupCraft.GetComponent<CraftModuleInterface>() : null;
		WorldContainer interiorWorld = craftModuleInterface?.GetInteriorWorld();
		bool doorPairingReady = EnsureDoorPairing(craftModuleInterface, interiorWorld);
		List<int> safeCells = (interiorWorld != null) ? GetSafeSpawnCells(interiorWorld) : null;
		if (clusterManager == null || interiorWorld == null || !doorPairingReady || safeCells == null || safeCells.Count == 0 || Components.LiveMinionIdentities.Count == 0)
		{
			GameScheduler.Instance.ScheduleNextFrame("Orbitfall.StartupBootstrapRetry", delegate
			{
				FinalizeBootstrap();
			});
			return;
		}

		List<MinionIdentity> liveMinions = new List<MinionIdentity>(Components.LiveMinionIdentities.Items);
		int movedDupes = 0;
		int dupeIndex = 0;
		foreach (MinionIdentity minion in liveMinions)
		{
			if (minion == null || minion.gameObject == null)
			{
				continue;
			}

			int targetCell = GetSafeSpawnCell(safeCells, dupeIndex);
			if (!Grid.IsValidCell(targetCell))
			{
				Vector2I fallback = interiorWorld.WorldOffset + new Vector2I(interiorWorld.WorldSize.x / 2, interiorWorld.WorldSize.y / 2);
				targetCell = Grid.XYToCell(fallback.x, fallback.y);
			}

			if (Grid.IsValidCell(targetCell))
			{
				clusterManager.MigrateMinion(minion, interiorWorld.id);
				minion.transform.SetPosition(Grid.CellToPosCBC(targetCell, Grid.SceneLayer.Move));
				movedDupes++;
			}
			dupeIndex++;
		}

		clusterManager.SetActiveWorld(interiorWorld.id);
		FocusCamera(interiorWorld);
		DisablePrintingPodFlow();
		RemoveStartWorldStarterObjects(clusterManager.GetStartWorld()?.id ?? -1);
		SetStartupCraftInOrbit();
		RemoveStartupOrbitalBackdrop();
		finalized = true;
		Debug.Log($"[Orbitfall] Startup rocket bootstrap complete. Moved {movedDupes} dupes to interior world {interiorWorld.id}.");
	}

	private static bool EnsureDoorPairing(CraftModuleInterface craftModuleInterface, WorldContainer interiorWorld)
	{
		if (craftModuleInterface == null || interiorWorld == null)
		{
			return false;
		}

		PassengerRocketModule passengerModule = craftModuleInterface.GetPassengerModule();
		if (passengerModule == null)
		{
			return false;
		}

		ClustercraftExteriorDoor exteriorDoor = passengerModule.GetComponent<ClustercraftExteriorDoor>();
		if (exteriorDoor == null)
		{
			return false;
		}

		ClustercraftInteriorDoor interiorDoor = exteriorDoor.GetInteriorDoor();
		if (interiorDoor == null || interiorDoor.GetMyWorldId() != interiorWorld.id)
		{
			foreach (ClustercraftInteriorDoor candidate in Components.ClusterCraftInteriorDoors.Items)
			{
				if (candidate != null && candidate.GetMyWorldId() == interiorWorld.id)
				{
					interiorDoor = candidate;
					break;
				}
			}
			if (interiorDoor == null)
			{
				return false;
			}
		}

		AssignmentGroupController exteriorGroup = exteriorDoor.GetComponent<AssignmentGroupController>();
		if (exteriorGroup != null && string.IsNullOrEmpty(exteriorGroup.AssignmentGroupID))
		{
			_ = exteriorGroup.GetMembers();
		}

		exteriorDoor.SetTarget(interiorDoor);
		return true;
	}

	private static List<int> GetSafeSpawnCells(WorldContainer interiorWorld)
	{
		List<int> result = new List<int>();
		Vector2I offset = interiorWorld.WorldOffset;
		Vector2I size = interiorWorld.WorldSize;
		int minX = offset.x;
		int maxX = offset.x + size.x - 1;
		int minY = offset.y;
		int maxY = offset.y + size.y - 1;
		int centerX = offset.x + (size.x / 2);
		int maxSpread = Mathf.Max(centerX - minX, maxX - centerX);

		// Prefer lower, central standable cells so dupes spawn inside on floor space, not on the roof.
		for (int y = minY + 1; y <= maxY; y++)
		{
			for (int spread = 0; spread <= maxSpread; spread++)
			{
				int leftX = centerX - spread;
				if (leftX >= minX)
				{
					int leftCell = Grid.XYToCell(leftX, y);
					if (IsStandableCell(leftCell) && Grid.WorldIdx[leftCell] == interiorWorld.id)
					{
						result.Add(leftCell);
					}
				}

				int rightX = centerX + spread;
				if (spread > 0 && rightX <= maxX)
				{
					int rightCell = Grid.XYToCell(rightX, y);
					if (IsStandableCell(rightCell) && Grid.WorldIdx[rightCell] == interiorWorld.id)
					{
						result.Add(rightCell);
					}
				}
			}
		}

		return result;
	}

	private static int GetSafeSpawnCell(List<int> safeCells, int dupeIndex)
	{
		if (safeCells == null || safeCells.Count == 0)
		{
			return Grid.InvalidCell;
		}

		int idx = dupeIndex % safeCells.Count;
		return safeCells[idx];
	}

	private static bool IsStandableCell(int cell)
	{
		if (!Grid.IsValidCell(cell))
		{
			return false;
		}

		int below = Grid.CellBelow(cell);
		if (!Grid.IsValidCell(below))
		{
			return false;
		}

		if (Grid.Solid[cell])
		{
			return false;
		}

		return Grid.Solid[below];
	}

	private static GameObject CreateStartupCraft(WorldContainer startWorld)
	{
		GameObject clustercraftPrefab = Assets.GetPrefab(ClustercraftConfig.ID);
		if (clustercraftPrefab == null)
		{
			return null;
		}

		GameObject craft = Util.KInstantiate(clustercraftPrefab);
		craft.name = StartupCraftName;
		craft.SetActive(value: true);

		Clustercraft clustercraft = craft.GetComponent<Clustercraft>();
		ClusterGridEntity startWorldEntity = startWorld.GetComponent<ClusterGridEntity>();
		if (clustercraft != null && startWorldEntity != null)
		{
			clustercraft.Init(startWorldEntity.Location, null);
		}
		return craft;
	}

	private static bool EnsurePassengerModule(CraftModuleInterface craftModuleInterface, WorldContainer startWorld)
	{
		if (craftModuleInterface.GetPassengerModule() != null)
		{
			return true;
		}

		BuildingDef habitatDef = Assets.GetBuildingDef(HabitatModuleMediumConfig.ID);
		if (habitatDef == null)
		{
			return false;
		}

		int cell = FindModulePlacementCell(startWorld, habitatDef);
		if (!Grid.IsValidCell(cell))
		{
			Debug.LogWarning("[Orbitfall] No valid placement cell found for startup habitat module.");
			return false;
		}

		GameObject module = habitatDef.Build(cell, Orientation.Neutral, null, habitatDef.DefaultElements(), 293.15f, playsound: false);
		if (module == null)
		{
			return false;
		}

		module.name = "OrbitfallStartupPassengerModule";
		RocketModuleCluster moduleCluster = module.GetComponent<RocketModuleCluster>();
		PassengerRocketModule passengerModule = module.GetComponent<PassengerRocketModule>();
		ClustercraftExteriorDoor exteriorDoor = module.GetComponent<ClustercraftExteriorDoor>();
		if (moduleCluster == null || passengerModule == null)
		{
			return false;
		}

		if (exteriorDoor != null)
		{
			exteriorDoor.interiorTemplateName = StartupInteriorTemplate;
		}

		AssignmentGroupController assignmentGroup = module.GetComponent<AssignmentGroupController>();
		if (assignmentGroup != null)
		{
			_ = assignmentGroup.GetMembers();
		}

		craftModuleInterface.AddModule(moduleCluster);
		return craftModuleInterface.GetPassengerModule() != null;
	}

	private static int FindModulePlacementCell(WorldContainer world, BuildingDef moduleDef)
	{
		if (world == null || moduleDef == null)
		{
			return Grid.InvalidCell;
		}

		Vector2I offset = world.WorldOffset;
		Vector2I size = world.WorldSize;
		int minX = offset.x;
		int maxX = offset.x + size.x - 1;
		int minY = offset.y;
		int maxY = offset.y + size.y - 1;
		int targetX = minX + Mathf.Max(6, size.x / 12);

		// Prefer placing the exterior module as high as possible and away from the normal starting area.
		for (int y = maxY; y >= minY; y--)
		{
			int bestCellForRow = Grid.InvalidCell;
			int bestDistanceForRow = int.MaxValue;
			for (int x = minX; x <= maxX; x++)
			{
				int cell = Grid.XYToCell(x, y);
				if (!Grid.IsValidCell(cell) || Grid.WorldIdx[cell] != world.id)
				{
					continue;
				}

				if (!moduleDef.IsValidPlaceLocation(null, cell, Orientation.Neutral, out _))
				{
					continue;
				}
				if (!HasOpenPlacementClearance(moduleDef, cell, world.id))
				{
					continue;
				}

				int distance = Mathf.Abs(x - targetX);
				if (distance < bestDistanceForRow)
				{
					bestDistanceForRow = distance;
					bestCellForRow = cell;
				}
			}

			if (Grid.IsValidCell(bestCellForRow))
			{
				return bestCellForRow;
			}
		}

		// Fallback: center-out radial search.
		Vector2I center = world.WorldOffset + new Vector2I(world.WorldSize.x / 2, world.WorldSize.y / 2);
		int maxRadius = Mathf.Max(world.WorldSize.x, world.WorldSize.y);
		for (int radius = 0; radius <= maxRadius; radius++)
		{
			for (int dy = -radius; dy <= radius; dy++)
			{
				for (int dx = -radius; dx <= radius; dx++)
				{
					if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
					{
						continue;
					}

					int x = center.x + dx;
					int y = center.y + dy;
					int cell = Grid.XYToCell(x, y);
					if (!Grid.IsValidCell(cell) || Grid.WorldIdx[cell] != world.id)
					{
						continue;
					}

					if (moduleDef.IsValidPlaceLocation(null, cell, Orientation.Neutral, out _))
					{
						if (!HasOpenPlacementClearance(moduleDef, cell, world.id))
						{
							continue;
						}
						return cell;
					}
				}
			}
		}

		return Grid.InvalidCell;
	}

	private static bool HasOpenPlacementClearance(BuildingDef moduleDef, int cell, int worldId)
	{
		if (moduleDef?.PlacementOffsets == null || moduleDef.PlacementOffsets.Length == 0)
		{
			return false;
		}

		int minX = int.MaxValue;
		int maxX = int.MinValue;
		int minY = int.MaxValue;
		int maxY = int.MinValue;
		foreach (CellOffset offset in moduleDef.PlacementOffsets)
		{
			int occupiedCell = Grid.OffsetCell(cell, offset);
			if (!Grid.IsValidCell(occupiedCell) || Grid.WorldIdx[occupiedCell] != worldId || Grid.Solid[occupiedCell])
			{
				return false;
			}

			Vector2I xy = Grid.CellToXY(occupiedCell);
			minX = Mathf.Min(minX, xy.x);
			maxX = Mathf.Max(maxX, xy.x);
			minY = Mathf.Min(minY, xy.y);
			maxY = Mathf.Max(maxY, xy.y);
		}

		for (int y = minY - 1; y <= maxY + 1; y++)
		{
			for (int x = minX - 1; x <= maxX + 1; x++)
			{
				if (x >= minX && x <= maxX && y >= minY && y <= maxY)
				{
					continue;
				}

				int c = Grid.XYToCell(x, y);
				if (!Grid.IsValidCell(c) || Grid.WorldIdx[c] != worldId || Grid.Solid[c])
				{
					return false;
				}
			}
		}

		return true;
	}

	private static void SetStartupCraftInOrbit()
	{
		if (startupCraft == null)
		{
			return;
		}

		Clustercraft clustercraft = startupCraft.GetComponent<Clustercraft>();
		if (clustercraft == null)
		{
			return;
		}

		if (clustercraft.Status == Clustercraft.CraftStatus.Grounded)
		{
			clustercraft.SetCraftStatus(Clustercraft.CraftStatus.InFlight);
			clustercraft.UpdateStatusItem();
		}
	}

	private static void RemoveStartupOrbitalBackdrop()
	{
		if (startupCraft == null)
		{
			return;
		}

		OrbitalObject[] orbitalObjects = startupCraft.GetComponentsInChildren<OrbitalObject>(true);
		int removed = 0;
		foreach (OrbitalObject orbitalObject in orbitalObjects)
		{
			if (orbitalObject == null || orbitalObject.gameObject == null)
			{
				continue;
			}

			Object.Destroy(orbitalObject.gameObject);
			removed++;
		}

		if (removed > 0)
		{
			Debug.Log($"[Orbitfall] Removed {removed} startup orbital backdrop object(s).");
		}
	}

	private static void FocusCamera(WorldContainer interiorWorld)
	{
		if (CameraController.Instance == null)
		{
			return;
		}

		Vector2I center = interiorWorld.WorldOffset + new Vector2I(interiorWorld.WorldSize.x / 2, interiorWorld.WorldSize.y / 2);
		int centerCell = Grid.XYToCell(center.x, center.y);
		if (!Grid.IsValidCell(centerCell))
		{
			return;
		}

		Vector3 pos = Grid.CellToPosCCC(centerCell, Grid.SceneLayer.Background);
		CameraController.Instance.SnapTo(pos);
		CameraController.Instance.SetTargetPos(pos, 5f, playSound: false);
	}

	private static void DisablePrintingPodFlow()
	{
		if (Immigration.Instance != null)
		{
			Immigration.Instance.EndImmigration();
			Immigration.Instance.timeBeforeSpawn = float.PositiveInfinity;
		}
	}

	private static void RemoveStartWorldStarterObjects(int startWorldId)
	{
		if (startWorldId < 0)
		{
			return;
		}

		RemoveObjectsByTag(startWorldId, GameTags.Telepad, "Telepad-tagged object");
		RemoveComponentsInWorld<RationBox>(startWorldId, "RationBox");
	}

	private static void RemoveObjectsByTag(int worldId, Tag tag, string label)
	{
		KPrefabID[] prefabs = Object.FindObjectsOfType<KPrefabID>();
		int removed = 0;
		foreach (KPrefabID prefab in prefabs)
		{
			if (prefab == null || prefab.gameObject == null)
			{
				continue;
			}
			if (!prefab.HasTag(tag))
			{
				continue;
			}
			if (prefab.GetMyWorldId() != worldId)
			{
				continue;
			}

			Object.Destroy(prefab.gameObject);
			removed++;
		}

		if (removed > 0)
		{
			Debug.Log($"[Orbitfall] Removed {removed} {label}(s) from start world {worldId}.");
		}
	}

	private static void RemoveComponentsInWorld<T>(int worldId, string label) where T : KMonoBehaviour
	{
		T[] components = Object.FindObjectsOfType<T>();
		int removed = 0;
		foreach (T component in components)
		{
			if (component == null || component.gameObject == null)
			{
				continue;
			}
			if (component.GetMyWorldId() != worldId)
			{
				continue;
			}

			Object.Destroy(component.gameObject);
			removed++;
		}

		if (removed > 0)
		{
			Debug.Log($"[Orbitfall] Removed {removed} {label} objects from start world {worldId}.");
		}
	}
}
