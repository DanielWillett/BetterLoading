using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace BetterLoading.AssetList;

public class DatLoadableAssetList : ILoadableAssetList
{
    public string FileName { get; }
    public DatLoadableAssetList(string fileName)
    {
        FileName = fileName;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<LegacyAssetReference> IEnumerable<LegacyAssetReference>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    public struct Enumerator : IEnumerator<LegacyAssetReference>
    {
        private readonly string _fileName;
        private StreamReader? _streamReader;
        private LegacyAssetReference _current;
        private int _lineNum = 0;

        public readonly LegacyAssetReference Current => _current;

        public Enumerator(DatLoadableAssetList list)
        {
            _fileName = list.FileName;
            Reset();
        }

        public bool MoveNext()
        {
            while (_streamReader?.ReadLine() is { } line)
            {
                ++_lineNum;

                ReadOnlySpan<char> lineSpan = line;

                if (lineSpan.IsWhiteSpace())
                    continue;

                int commentIndex = lineSpan.IndexOf('#');
                while (commentIndex > 0 && char.IsWhiteSpace(lineSpan[commentIndex - 1]))
                    --commentIndex;

                if (commentIndex != -1)
                    lineSpan = lineSpan[..commentIndex];

                if (lineSpan.IsEmpty)
                    continue;
                
                if (Guid.TryParse(lineSpan, out Guid guid))
                {
                    _current.Id = 0;
                    _current.Category = 0;
                    _current.Guid = guid;
                    return true;
                }

                int firstSpace = -1;
                bool hasHitNonSpace = false;
                int firstNonSpace = -1;
                for (int i = 0; i < lineSpan.Length; ++i)
                {
                    if (!char.IsWhiteSpace(lineSpan[i]))
                    {
                        if (firstNonSpace == -1)
                            firstNonSpace = i;
                        hasHitNonSpace = true;
                        continue;
                    }

                    firstSpace = i;
                    if (hasHitNonSpace)
                        break;
                }

                if (firstSpace == -1 || firstSpace == lineSpan.Length - 1)
                {
                    Log($"[BetterLoading | DatLoadableAssetList] Invalid GUID on line #{_lineNum}.");
                    continue;
                }
                
                int endOfFirstSpace = -1;
                for (int i = firstSpace + 1; i < lineSpan.Length; ++i)
                {
                    if (char.IsWhiteSpace(lineSpan[i]))
                        continue;

                    endOfFirstSpace = i;
                    break;
                }

                if (endOfFirstSpace == -1)
                    continue;

                if (!Enum.TryParse(line.Substring(firstNonSpace, firstSpace - firstNonSpace),
                                   true,
                                   out EAssetType assetCategory)
                    || assetCategory is > EAssetType.NPC or <= EAssetType.NONE)
                {
                    Log($"[BetterLoading | DatLoadableAssetList] Invalid asset category on line #{_lineNum}.");
                    continue;
                }

                if (!ushort.TryParse(lineSpan.Slice(endOfFirstSpace), NumberStyles.Number, CultureInfo.InvariantCulture, out ushort id))
                {
                    Log($"[BetterLoading | DatLoadableAssetList] Invalid ID on line #{_lineNum}.");
                    continue;
                }

                _current.Guid = default;
                _current.Category = assetCategory;
                _current.Id = id;
                return true;
            }

            Dispose();
            return false;
        }

        private static void Log(string message)
        {
            if (BetterLoadingModule.Instance == null)
                Console.WriteLine(message);
            else if (Dedicator.isStandaloneDedicatedServer)
                CommandWindow.LogWarning(message);
            else
                UnturnedLog.warn(message);
        }


        object IEnumerator.Current => Current;

        public void Reset()
        {
            _streamReader?.Dispose();
            if (File.Exists(_fileName))
                _streamReader = new StreamReader(_fileName);
            _lineNum = 0;
        }

        public void Dispose()
        {
            if (_streamReader == null)
                return;

            _streamReader.Dispose();
            _streamReader = null;
        }
    }
}
