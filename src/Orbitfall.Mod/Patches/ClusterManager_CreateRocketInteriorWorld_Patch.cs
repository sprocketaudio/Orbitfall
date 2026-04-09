using HarmonyLib;

namespace Orbitfall.Patches;

[HarmonyPatch(typeof(ClusterManager), nameof(ClusterManager.CreateRocketInteriorWorld))]
internal static class ClusterManager_CreateRocketInteriorWorld_Patch
{
	private static void Prefix()
	{
		RocketInteriorSizing.Apply("ClusterManager.CreateRocketInteriorWorld");
	}
}
