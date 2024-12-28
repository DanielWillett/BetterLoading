using BetterLoading.AssetList;
using DanielWillett.ReflectionTools;
using HarmonyLib;
using SDG.Framework.Modules;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;

namespace BetterLoading;

public class BetterLoadingModule : IModuleNexus
{
    [ThreadStatic]
    private static DatQuickParser? _datQuickParser;

    private const string DefaultAssetList =
        """
        # Whitelist of assets to load
        # Text after a '#' is considered as a comment.
        #
        # Examples (remove '#' to apply):
        # 92b49222958d4c6fbeca1bd00987b0fd # Ace
        # Item 4 # Eaglefire (by ID)
        """;

    public static BetterLoadingModule Instance;

    public Harmony Patcher { get; } = new Harmony("danielwillett.betterloading");

    private string? _homeDir;

    public ICollection<LegacyAssetReference> LoadedAssets { get; private set; } = Array.Empty<LegacyAssetReference>();

    public bool ShouldLoadFile(string fileName)
    {
        if (LoadedAssets.Count == 0)
            return true;

        QuickAssetInfo assetInfo;
        _datQuickParser ??= new DatQuickParser();

        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader sr = new StreamReader(fs))
        {
            try
            {
                assetInfo = _datQuickParser.Parse(sr);
            }
            catch (Exception ex)
            {
                CommandWindow.LogError($"Error parsing asset file {fileName} - {ex.GetType()}.");
                CommandWindow.LogError(ex);
                return false;
            }
        }

        if (_datQuickParser.HasError)
        {
            CommandWindow.LogError($"Error parsing asset file {fileName} - {_datQuickParser.ErrorMessage}.");
            return false;
        }

        if (assetInfo.Guid != Guid.Empty)
        {
            if (LoadedAssets.Contains(new LegacyAssetReference { Guid = assetInfo.Guid }))
            {
                CommandWindow.Log($"Loading file {fileName} by GUID");
                return true;
            }
        }

        if (assetInfo.AssetType is null || assetInfo.Id == 0)
            return false;

        EAssetType category = AssetHelper.GetAssetCategory(assetInfo.AssetType);
        if (!LoadedAssets.Contains(new LegacyAssetReference { Id = assetInfo.Id, Category = category }))
        {
            return false;
        }

        CommandWindow.Log($"Loading file {fileName} by ID");
        return true;
    }

    void IModuleNexus.initialize()
    {
        Instance = this;

        _datQuickParser = new DatQuickParser();

        HarmonyLog.Reset(Path.Combine(UnturnedPaths.RootDirectory.FullName, "Logs", "harmony_betterloading.log"));
        
        Patcher.PatchAll(typeof(BetterLoadingModule).Assembly);

        if (Dedicator.isStandaloneDedicatedServer)
        {
            _homeDir = Path.Combine(UnturnedPaths.RootDirectory.FullName, "Servers", Provider.serverID,
                                    "Better Loading");
        }
        else
        {
            _homeDir = Path.Combine(UnturnedPaths.RootDirectory.FullName, "Better Loading");
        }

        Directory.CreateDirectory(_homeDir);

        DatLoadableAssetList assetList = new DatLoadableAssetList(Path.Combine(_homeDir, "Asset Whitelist.dat"));

        if (!File.Exists(assetList.FileName))
        {
            File.WriteAllText(assetList.FileName, DefaultAssetList);
        }
        else
        {
            foreach (LegacyAssetReference asset in assetList)
            {
                if (LoadedAssets.Count == 0)
                {
                    LoadedAssets = new HashSet<LegacyAssetReference>(64);
                }

                LoadedAssets.Add(asset);
                CommandWindow.Log($"Found filter {asset} in asset list.");
            }
        }

        Level.onLevelLoaded += LevelLoaded;
    }

    private void LevelLoaded(int level)
    {
        if (level != Level.BUILD_INDEX_GAME)
            return;

        List<Asset> assets = new List<Asset>();
        Assets.find(assets);
        assets.ForEach(a =>
        {
            CommandWindow.Log(a.getTypeNameAndIdDisplayString());
        });
    }

    void IModuleNexus.shutdown()
    {
        _datQuickParser = null;

        LoadedAssets = Array.Empty<LegacyAssetReference>();
        Patcher.UnpatchAll();
    }
}