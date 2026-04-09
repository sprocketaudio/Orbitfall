using HarmonyLib;

namespace Orbitfall.Patches;

[HarmonyPatch(typeof(WattsonMessage), "OnActivate")]
internal static class WattsonMessage_OnActivate_Patch
{
	private static bool Prefix(WattsonMessage __instance)
	{
		if (!StartupRocketBootstrap.SuppressTelepadEvents)
		{
			return true;
		}

		// Orbitfall bypasses printing-pod intro flow, so skip Wattson telepad animation sequence.
		if (__instance != null && __instance.isActiveAndEnabled)
		{
			__instance.Deactivate();
		}

		return false;
	}
}
