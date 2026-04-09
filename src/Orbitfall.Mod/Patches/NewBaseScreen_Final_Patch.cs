using HarmonyLib;

namespace Orbitfall.Patches;

[HarmonyPatch(typeof(NewBaseScreen), "Final")]
internal static class NewBaseScreen_Final_Patch
{
	private static void Postfix()
	{
		StartupRocketBootstrap.TryRun();
	}
}
