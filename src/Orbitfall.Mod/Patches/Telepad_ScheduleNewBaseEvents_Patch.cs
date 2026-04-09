using HarmonyLib;

namespace Orbitfall.Patches;

[HarmonyPatch(typeof(Telepad), nameof(Telepad.ScheduleNewBaseEvents))]
internal static class Telepad_ScheduleNewBaseEvents_Patch
{
	private static bool Prefix()
	{
		return !StartupRocketBootstrap.SuppressTelepadEvents;
	}
}
