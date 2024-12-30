using BetterLoading.AssetList;
using BetterLoading.Utility;
using DanielWillett.ReflectionTools;
using HarmonyLib;
using SDG.Framework.Devkit;
using SDG.Framework.Landscapes;
using SDG.Framework.Modules;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace BetterLoading;

public class BetterLoadingModule : IModuleNexus
{
    [ThreadStatic]
    private static DatQuickParser? _datQuickParser;

    public const string OriginName = "BetterLoading Auto-Load";

    private Dictionary<Guid, string> _lateLoadableAssets = new Dictionary<Guid, string>(2048);

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
                CommandWindow.LogError($"Error parsing asset file {RelativePath(fileName)} - {ex.GetType()}.");
                CommandWindow.LogError(ex);
                return false;
            }
        }

        if (_datQuickParser.HasError)
        {
            CommandWindow.LogError($"Error parsing asset file {RelativePath(fileName)} - {_datQuickParser.ErrorMessage}.");
            return false;
        }

        if (assetInfo.Guid != Guid.Empty)
        {
            if (LoadedAssets.Contains(new LegacyAssetReference { Guid = assetInfo.Guid }))
            {
                CommandWindow.Log($"Loading file {RelativePath(fileName)} by GUID.");
                return true;
            }

            if (assetInfo.AssetType is not null && typeof(PhysicsMaterialAssetBase).IsAssignableFrom(assetInfo.AssetType))
            {
                LoadedAssets.Add(new LegacyAssetReference { Guid = assetInfo.Guid });
                CommandWindow.Log($"Loading physics material {RelativePath(fileName)}.");
                return true;
            }

            _lateLoadableAssets[assetInfo.Guid] = fileName;
        }

        if (assetInfo.AssetType is null || assetInfo.Id == 0)
            return false;

        EAssetType category = AssetHelper.GetAssetCategory(assetInfo.AssetType);
        if (!LoadedAssets.Contains(new LegacyAssetReference { Id = assetInfo.Id, Category = category }))
        {
            return false;
        }

        CommandWindow.Log($"Loading file {RelativePath(fileName)} by ID.");
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

        if (LoadedAssets.Count > 0)
        {
            LevelInfo level = Level.getLevel(Provider.map);
            if (level != null)
            {
                if (level.configData.Asset.isValid)
                    LoadedAssets.Add(new LegacyAssetReference { Guid = level.configData.Asset.GUID });
            }
        }

        Level.onLevelLoaded += LevelLoaded;
        LevelHierarchy.loaded += LevelHierarchyReady;
    }

    private void LevelHierarchyReady()
    {
        foreach (AssetReference<LandscapeMaterialAsset> mat in LandscapeHelper.Tiles.SelectMany(x => x.materials).Where(x => x.isValid))
        {
            TryLoadLateAsset(mat.GUID, out _);
        }
    }

    public bool TryLoadLateAsset(Guid guid, [MaybeNullWhen(false)] out Asset asset)
    {
        LegacyAssetReference aref = new LegacyAssetReference { Guid = guid };
        if (LoadedAssets.Contains(aref) || !_lateLoadableAssets.TryGetValue(aref.Guid, out string path))
        {
            CommandWindow.LogWarning($"Failed to find asset at runtime: {aref.Guid}.");
            asset = null;
            return false;
        }

        LoadedAssets.Add(aref);
        AssetOrigin assetOrigin = new AssetOrigin { name = OriginName };
        AssetLoaderHelper.LoadAsset(path, assetOrigin);
        AssetLoaderHelper.SyncAssetsFromOrigin(assetOrigin);
        _lateLoadableAssets.Remove(guid);
        asset = Assets.find(guid);
        if (asset == null)
        {
            CommandWindow.LogWarning($"Failed to load asset at runtime: {aref.Guid} {RelativePath(path)}.");
            return false;
        }

        CommandWindow.Log($"Loaded asset at runtime: {aref.Guid} {RelativePath(path)}.");
        return true;
    }


    private void LevelLoaded(int level)
    {
        if (level != Level.BUILD_INDEX_GAME)
            return;

        Level.onLevelLoaded -= LevelLoaded;

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

    private static readonly string _workshopRoot = Path.GetFullPath(Path.Combine(UnturnedPaths.RootDirectory.FullName, "..", "..", "workshop", "content", "304930"));
    private string RelativePath(string path)
    {
        string relative = Path.GetRelativePath(UnturnedPaths.RootDirectory.FullName, path);
        if (!Dedicator.isStandaloneDedicatedServer && relative.Equals(path, StringComparison.Ordinal))
        {
            relative = Path.GetRelativePath(_workshopRoot, relative);
        }

        return relative;
    }
}