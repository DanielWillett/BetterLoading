using BetterLoading.AssetList;

namespace BetterLoading.Tests;

internal class Program
{
    private static void Main(string[] args)
    {
        const string assetList =
            @"C:\SteamCMD\steamapps\common\U3DS\Servers\BetterLoading\Better Loading\Asset Whitelist.dat";

        DatLoadableAssetList list = new DatLoadableAssetList(assetList);

        foreach (LegacyAssetReference reference in list)
        {
            Console.WriteLine(reference);
        }

        const string file =
            @"C:\SteamCMD\steamapps\common\U3DS\Bundles\Items\Outfits\SwashbucklerOutfit.asset";

        DatQuickParser parser = new DatQuickParser();

        using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader sr = new StreamReader(fs);

        QuickAssetInfo info = parser.Parse(sr);

        if (parser.HasError)
        {
            Console.WriteLine(parser.ErrorMessage);
        }

        Console.WriteLine($"Info: {info.Guid}, {info.Id}, {info.AssetType}.");
    }
}