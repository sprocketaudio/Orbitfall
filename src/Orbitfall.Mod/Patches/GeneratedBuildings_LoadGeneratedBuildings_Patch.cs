using HarmonyLib;

namespace Orbitfall.Patches;

[HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
internal static class GeneratedBuildings_LoadGeneratedBuildings_Patch
{
	private static void Postfix()
	{
		RocketInteriorSizing.Apply("GeneratedBuildings.LoadGeneratedBuildings");
	}
}
