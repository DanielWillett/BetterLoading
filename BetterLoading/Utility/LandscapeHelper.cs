using DanielWillett.ReflectionTools;
using SDG.Framework.Landscapes;
using System.Collections.Generic;

namespace BetterLoading.Utility;

public static class LandscapeHelper
{
    private static readonly StaticGetter<Dictionary<LandscapeCoord, LandscapeTile>> GetTiles =
        Accessor.GenerateStaticGetter<Landscape, Dictionary<LandscapeCoord, LandscapeTile>>("tiles", throwOnError: true)!;

    public static IReadOnlyCollection<LandscapeTile> Tiles => GetTiles().Values;

    internal static Dictionary<LandscapeCoord, LandscapeTile> GetTileDictionary() => GetTiles();
}