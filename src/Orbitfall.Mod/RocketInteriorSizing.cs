using UnityEngine;

namespace Orbitfall;

internal static class RocketInteriorSizing
{
	internal const int TargetWidth = 48;
	internal const int TargetHeight = 48;

	private static bool logged;

	internal static void Apply(string source)
	{
		Vector2I current = TUNING.ROCKETRY.ROCKET_INTERIOR_SIZE;
		if (current.x == TargetWidth && current.y == TargetHeight)
		{
			if (!logged)
			{
				Debug.Log($"[Orbitfall] Rocket interior size already set to {TargetWidth}x{TargetHeight} ({source}).");
				logged = true;
			}
			return;
		}

		TUNING.ROCKETRY.ROCKET_INTERIOR_SIZE = new Vector2I(TargetWidth, TargetHeight);
		Debug.Log($"[Orbitfall] Rocket interior size changed from {current.x}x{current.y} to {TargetWidth}x{TargetHeight} ({source}).");
		logged = true;
	}
}
