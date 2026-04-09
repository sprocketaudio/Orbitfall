using HarmonyLib;
using Klei;
using ProcGen;
using System;
using System.Reflection;
using UnityEngine;

namespace Orbitfall.Patches;

[HarmonyPatch(typeof(TemplateCache), nameof(TemplateCache.RewriteTemplateYaml))]
internal static class TemplateCache_RewriteTemplateYaml_Patch
{
	private const string StartupInteriorTemplate = "expansion1::interiors/orbitfall_start";
	private const string RelativeTemplatePath = "templates/interiors/orbitfall_start.yaml";

	private static bool loggedMissingFile;

	private static void Postfix(string scopePath, ref string __result)
	{
		if (!string.Equals(scopePath, StartupInteriorTemplate, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		string modRoot = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if (string.IsNullOrEmpty(modRoot))
		{
			return;
		}

		string customTemplatePath = FileSystem.Normalize(System.IO.Path.Combine(modRoot, RelativeTemplatePath));
		if (System.IO.File.Exists(customTemplatePath))
		{
			__result = customTemplatePath;
			return;
		}

		if (!loggedMissingFile)
		{
			Debug.LogWarning($"[Orbitfall] Custom startup template not found at '{customTemplatePath}'. Falling back to default path '{__result}'.");
			loggedMissingFile = true;
		}
	}
}
