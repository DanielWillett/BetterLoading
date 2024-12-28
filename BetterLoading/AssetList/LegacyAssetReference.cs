using SDG.Unturned;
using System;
using System.Globalization;

namespace BetterLoading.AssetList;

public struct LegacyAssetReference : IEquatable<LegacyAssetReference>
{
    public ushort Id;
    public EAssetType Category;
    public Guid Guid;

    public Asset? Resolve()
    {
        if (Id != 0)
            return Assets.find(Category, Id);

        return Assets.find(Guid);
    }

    public override int GetHashCode()
    {
        return Id != 0 ? Id << 16 | (int)Category : Guid.GetHashCode();
    }

    public readonly override bool Equals(object? other)
    {
        return other is LegacyAssetReference r && Equals(in r);
    }

    public readonly bool Equals(LegacyAssetReference other)
    {
        return Equals(in other);
    }
    
    public readonly bool Equals(in LegacyAssetReference other)
    {
        if (Id != 0)
        {
            return other.Id == Id && other.Category == Category && other.Guid == Guid.Empty;
        }

        if (Guid != Guid.Empty)
        {
            return other.Guid == Guid && other.Id == 0;
        }

        return other.Id == 0 && other.Guid == Guid.Empty;
    }

    public override string ToString()
    {
        if (Id != 0)
            return Category + " " + Id.ToString(CultureInfo.InvariantCulture);

        return Guid.ToString("N");
    }
}
