using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;

internal static class Program
{
    private const string StreamingAssetsPath = @"E:\Games\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\StreamingAssets";
    private const string SearchToken = "starmap_landed_surface";

    private static readonly string[] PreferredBundles =
    {
        "expansion1_bundle",
        "base_bundle",
        "hires_expansion1_bundle",
        "hires_base_bundle"
    };

    private static int Main()
    {
        string outputDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "out"));
        Directory.CreateDirectory(outputDir);

        if (!Directory.Exists(StreamingAssetsPath))
        {
            Console.Error.WriteLine($"StreamingAssets path not found: {StreamingAssetsPath}");
            return 2;
        }

        var bundlePaths = ResolveBundles(StreamingAssetsPath);
        if (bundlePaths.Count == 0)
        {
            Console.Error.WriteLine("No bundle files found.");
            return 3;
        }

        Console.WriteLine($"Scanning {bundlePaths.Count} bundle files...");
        var am = new AssetsManager();

        foreach (string bundlePath in bundlePaths)
        {
            Console.WriteLine($"Bundle: {Path.GetFileName(bundlePath)}");
            try
            {
                var bundle = am.LoadBundleFile(bundlePath, unpackIfPacked: true);
                if (TryExtractFromBundle(am, bundle, outputDir, out string outputPath))
                {
                    Console.WriteLine($"Extracted: {outputPath}");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Skipped ({ex.GetType().Name}): {ex.Message}");
            }
        }

        Console.Error.WriteLine($"Could not find texture matching '{SearchToken}'.");
        return 1;
    }

    private static List<string> ResolveBundles(string streamingAssetsPath)
    {
        var allBundles = Directory.GetFiles(streamingAssetsPath, "*_bundle", SearchOption.TopDirectoryOnly).ToList();
        var ordered = new List<string>();

        foreach (string preferred in PreferredBundles)
        {
            string? match = allBundles.FirstOrDefault(p => string.Equals(Path.GetFileName(p), preferred, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                ordered.Add(match);
            }
        }

        foreach (string path in allBundles.OrderBy(Path.GetFileName))
        {
            if (!ordered.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                ordered.Add(path);
            }
        }

        return ordered;
    }

    private static bool TryExtractFromBundle(AssetsManager am, BundleFileInstance bundle, string outputDir, out string outputPath)
    {
        outputPath = string.Empty;

        List<string> names = bundle.file.GetAllFileNames();
        for (int i = 0; i < names.Count; i++)
        {
            if (!bundle.file.IsAssetsFile(i))
            {
                continue;
            }

            AssetsFileInstance assetsFile;
            try
            {
                assetsFile = am.LoadAssetsFileFromBundle(bundle, i, loadDeps: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Asset file {i}: load failed ({ex.GetType().Name})");
                continue;
            }

            if (TryExtractTexture(am, assetsFile, outputDir, out outputPath))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryExtractTexture(AssetsManager am, AssetsFileInstance assetsFile, string outputDir, out string outputPath)
    {
        outputPath = string.Empty;

        List<AssetFileInfo> textureInfos = assetsFile.file.GetAssetsOfType(AssetClassID.Texture2D);
        foreach (AssetFileInfo info in textureInfos)
        {
            AssetTypeValueField baseField;
            try
            {
                baseField = am.GetBaseField(assetsFile, info, AssetReadFlags.None);
            }
            catch
            {
                continue;
            }

            string name = SafeString(baseField.Get("m_Name")?.AsString);
            if (!name.Contains(SearchToken, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                var texture = TextureFile.ReadTextureFile(baseField);
                byte[] data = texture.FillPictureData(assetsFile);

                string safeName = SanitizeFileName(name);
                string outputName = $"{safeName}_{info.PathId}.png";
                outputPath = Path.Combine(outputDir, outputName);

                bool ok = texture.DecodeTextureImage(data, outputPath, ImageExportType.Png, quality: 100);
                if (ok && File.Exists(outputPath))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Found '{name}' but decode failed: {ex.GetType().Name} {ex.Message}");
            }
        }

        return false;
    }

    private static string SafeString(string? s) => s ?? string.Empty;

    private static string SanitizeFileName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        char[] chars = input.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (invalid.Contains(chars[i]))
            {
                chars[i] = '_';
            }
        }
        return new string(chars);
    }
}
