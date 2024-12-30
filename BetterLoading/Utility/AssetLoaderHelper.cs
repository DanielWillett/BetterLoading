using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BetterLoading.Utility;
internal static class AssetLoaderHelper
{
    public static void SyncAssetsFromOrigin(AssetOrigin origin) => SyncAssetsFromOriginMethod?.Invoke(origin);
    public static void LoadAsset(string filePath, AssetOrigin origin)
    {
        LoadFile(filePath, origin);
    }

    private static readonly DatParser _parser = new DatParser();
    private static void GetData(string filePath, out DatDictionary assetData, out string? assetError, out byte[] hash, out DatDictionary? translationData, out DatDictionary? fallbackTranslationData)
    {
        string directoryName = Path.GetDirectoryName(filePath)!;
        using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using SHA1Stream sha1Fs = new SHA1Stream(fs);
        using StreamReader input = new StreamReader(sha1Fs);

        assetData = _parser.Parse(input);
        assetError = _parser.ErrorMessage;
        hash = sha1Fs.Hash;
        string localLang = Path.Combine(directoryName, Provider.language + ".dat");
        string englishLang = Path.Combine(directoryName, "English.dat");
        translationData = null;
        fallbackTranslationData = null;
        if (File.Exists(localLang))
        {
            translationData = ReadFileWithoutHash(localLang);
            if (!Provider.language.Equals("English", StringComparison.Ordinal) && File.Exists(englishLang))
                fallbackTranslationData = ReadFileWithoutHash(englishLang);
        }
        else if (File.Exists(englishLang))
            translationData = ReadFileWithoutHash(englishLang);
    }
    public static DatDictionary ReadFileWithoutHash(string path)
    {
        using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader inputReader = new StreamReader(fileStream);
        return _parser.Parse(inputReader);
    }

    private static readonly Action<string, AssetOrigin> LoadFile;
    private static readonly Action<AssetOrigin> SyncAssetsFromOriginMethod;
    static AssetLoaderHelper()
    {
        SyncAssetsFromOriginMethod = (Action<AssetOrigin>)typeof(Assets)
            .GetMethod("AddAssetsFromOriginToCurrentMapping", BindingFlags.Static | BindingFlags.NonPublic)?
            .CreateDelegate(typeof(Action<AssetOrigin>))!;

        if (SyncAssetsFromOriginMethod == null)
        {
            CommandWindow.LogError("[AssetLoaderHelper] Assets.AddAssetsFromOriginToCurrentMapping not found or arguments changed.");
            return;
        }

        MethodInfo method = typeof(AssetLoaderHelper).GetMethod(nameof(GetData), BindingFlags.Static | BindingFlags.NonPublic)!;
        Type? assetInfo = typeof(Assets).Assembly.GetType("SDG.Unturned.AssetsWorker+AssetDefinition", false, false);
        if (assetInfo == null)
        {
            CommandWindow.LogError("[AssetLoaderHelper] AssetsWorker.AssetDefinition not found.");
            return;
        }

        MethodInfo? loadFileMethod = typeof(Assets).GetMethod("LoadFile", BindingFlags.NonPublic | BindingFlags.Static);
        if (loadFileMethod == null)
        {
            CommandWindow.LogError("[AssetLoaderHelper] Assets.LoadFile not found.");
            return;
        }

        FieldInfo? pathField = assetInfo.GetField("path", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? hashField = assetInfo.GetField("hash", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? assetDataField = assetInfo.GetField("assetData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? translationDataField = assetInfo.GetField("translationData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? fallbackTranslationDataField = assetInfo.GetField("fallbackTranslationData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? assetErrorField = assetInfo.GetField("assetError", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? originField = assetInfo.GetField("origin", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pathField == null || hashField == null || assetDataField == null || translationDataField == null || fallbackTranslationDataField == null || assetErrorField == null || originField == null)
        {
            CommandWindow.LogError("[AssetLoaderHelper] Missing field in AssetsWorker.AssetDefinition.");
            return;
        }

        DynamicMethod dm = new DynamicMethod("LoadAsset", typeof(void), new Type[] { typeof(string), typeof(AssetOrigin) }, typeof(AssetLoaderHelper).Module, true);
        ILGenerator generator = dm.GetILGenerator();
        dm.DefineParameter(0, ParameterAttributes.None, "path");
        dm.DefineParameter(1, ParameterAttributes.None, "assetOrigin");
        generator.DeclareLocal(typeof(DatDictionary));

        generator.DeclareLocal(typeof(string));
        generator.DeclareLocal(typeof(byte[]));
        generator.DeclareLocal(typeof(DatDictionary));
        generator.DeclareLocal(typeof(DatDictionary));
        generator.DeclareLocal(assetInfo);

        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldloca_S, 0);
        generator.Emit(OpCodes.Ldloca_S, 1);
        generator.Emit(OpCodes.Ldloca_S, 2);
        generator.Emit(OpCodes.Ldloca_S, 3);
        generator.Emit(OpCodes.Ldloca_S, 4);
        generator.Emit(OpCodes.Call, method);

        generator.Emit(OpCodes.Ldloca_S, 5);
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Stfld, pathField);

        generator.Emit(OpCodes.Ldloca_S, 5);
        generator.Emit(OpCodes.Ldloc_2);
        generator.Emit(OpCodes.Stfld, hashField);

        generator.Emit(OpCodes.Ldloca_S, 5);
        generator.Emit(OpCodes.Ldloc_0);
        generator.Emit(OpCodes.Stfld, assetDataField);

        generator.Emit(OpCodes.Ldloca_S, 5);
        generator.Emit(OpCodes.Ldloc_3);
        generator.Emit(OpCodes.Stfld, translationDataField);

        generator.Emit(OpCodes.Ldloca_S, 5);
        generator.Emit(OpCodes.Ldloc_S, 4);
        generator.Emit(OpCodes.Stfld, fallbackTranslationDataField);

        generator.Emit(OpCodes.Ldloca_S, 5);
        generator.Emit(OpCodes.Ldloc_1);
        generator.Emit(OpCodes.Stfld, assetErrorField);

        generator.Emit(OpCodes.Ldloca_S, 5);
        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Stfld, originField);

        generator.Emit(OpCodes.Ldloc_S, 5);
        generator.Emit(OpCodes.Call, loadFileMethod);

        generator.Emit(OpCodes.Ret);

        LoadFile = (Action<string, AssetOrigin>)dm.CreateDelegate(typeof(Action<string, AssetOrigin>));
    }
}
