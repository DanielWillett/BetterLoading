using System.Collections.Generic;

namespace BetterLoading.AssetList;

public interface ILoadableAssetList : IEnumerable<LegacyAssetReference>
{
    string FileName { get; }
}