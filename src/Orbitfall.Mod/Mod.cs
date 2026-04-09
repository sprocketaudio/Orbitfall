using HarmonyLib;
using KMod;
using System.Reflection;

namespace Orbitfall;

public sealed class OrbitfallMod : UserMod2
{
	public override void OnLoad(Harmony harmony)
	{
		base.OnLoad(harmony);
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		RocketInteriorSizing.Apply("OnLoad");
	}
}
