/*
 * This class was originally written by Nelson Sexton of SDG.
 *
 * It was copied over and modified to easily parse out GUID/ID without as much
 * unnecessary allocation for just scanning asset files.
 */

using SDG.Unturned;
using System;
using System.IO;
using System.Text;

namespace BetterLoading.AssetList;

public class DatQuickParser
{
    private readonly StringBuilder stringBuilder = new StringBuilder();

    private TextReader? _inputReader;
    private int _currentLineNumber;
    private int _currentReadResult;
    private char _currentChar;
    private bool _hasChar;
    private bool _hasError;
    private string? _errorMessage;

    private string? _id;
    private string? _type;
    private bool _typeInMetadata;
    private string? _guid;

    public QuickAssetInfo Parse(TextReader inputReader)
    {
        _id = null;
        _type = null;
        _guid = null;
        _typeInMetadata = false;

        _inputReader = inputReader;
        ErrorMessage = null;
        _hasError = false;
        _hasChar = false;
        _currentLineNumber = 1;
        ReadChar();
        SkipUtf8Bom();
        SkipWhitespaceAndComments();
        while (_hasChar)
        {
            if (_currentChar == '/')
            {
                SkipWhitespaceAndComments();
            }
            else
            {
                KnownKey key = ReadDictionaryKey();
                SkipSpacesAndTabs();
                string? value = ReadDictionaryValue(key);
                if (key == KnownKey.Id)
                {
                    _id = value;
                }
                else if (key == KnownKey.Guid)
                {
                    _guid = value;
                }
                else if (key == KnownKey.Type && _type == null)
                {
                    _type = value;
                    _typeInMetadata = false;
                }
            }
        }


        Guid.TryParse(_guid, out Guid guid);
        ushort.TryParse(_id, out ushort id);
        Type? type = null;
        string? typeName = _type;
        if (!string.IsNullOrEmpty(typeName))
        {
            Type? t = _typeInMetadata ? null : Assets.assetTypes.getType(typeName);
            t ??= Type.GetType(typeName, false, true);

            if (t != null && typeof(Asset).IsAssignableFrom(t))
            {
                type = t;
            }
        }

        return new QuickAssetInfo(type, guid, id);
    }

    public QuickAssetInfo Parse(string input)
    {
        using StringReader inputReader = new StringReader(input);
        return Parse(inputReader);
    }

    public QuickAssetInfo Parse(byte[] input)
    {
        using MemoryStream memoryStream = new MemoryStream(input);
        using StreamReader inputReader = new StreamReader(memoryStream);
        return Parse(inputReader);
    }

    public bool HasError => _hasError;

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            _errorMessage = value;
            _hasError = !string.IsNullOrEmpty(_errorMessage);
        }
    }

    private void ReadChar()
    {
        bool flag = _hasChar && _currentChar == '\r';
        _currentReadResult = _inputReader!.Read();
        _hasChar = _currentReadResult >= 0;
        _currentChar = _hasChar ? (char)_currentReadResult : char.MinValue;
        _currentLineNumber += !_hasChar || _currentChar != '\r' && (_currentChar != '\n' || flag) ? 0 : 1;
    }

    private void SkipSpacesAndTabs()
    {
        while (_hasChar && (_currentChar == ' ' || _currentChar == '\t'))
            ReadChar();
    }

    private void SkipUtf8Bom()
    {
        if (!_hasChar || _currentChar != 'ï')
            return;
        ReadChar();
        if (!_hasChar || _currentChar != '»')
            return;
        ReadChar();
        if (!_hasChar || _currentChar != '¿')
            return;
        ReadChar();
    }

    private void SkipWhitespaceAndComments()
    {
    restart:
        while (_hasChar)
        {
            if (_currentChar == '/')
            {
                ReadChar();
                while (true)
                {
                    if (_hasChar && _currentChar != '\n' && _currentChar != '\r')
                        ReadChar();
                    else
                        goto restart;
                }
            }

            if (!char.IsWhiteSpace(_currentChar) && _currentChar != ',')
                break;
            ReadChar();
        }
    }

    private string? ReadDictionaryValue(KnownKey key)
    {
        string? str;
        if (key is KnownKey.Id or KnownKey.Type or KnownKey.Guid)
        {
            str = ReadString();
        }
        else
        {
            SkipString();
            str = null;
        }
        SkipWhitespaceAndComments();
        if (_hasChar)
        {
            if (_currentChar == '{')
            {
                ReadDictionary(key);
                return null;
            }

            if (_currentChar == '[')
            {
                SkipList();
                return null;
            }
        }
        return str;
    }

    private void ReadDictionary(KnownKey knownKey)
    {
        int currentLineNumber = _currentLineNumber;
        ReadChar();
        SkipWhitespaceAndComments();

        bool ended = false;
        while (_hasChar)
        {
            if (_currentChar == '/')
            {
                SkipWhitespaceAndComments();
            }
            else
            {
                if (_currentChar == '}')
                {
                    ReadChar();
                    ended = true;
                    break;
                }
                KnownKey key = ReadDictionaryKey();
                SkipSpacesAndTabs();
                string? value = ReadDictionaryValue(key);
                if (knownKey == KnownKey.Asset)
                {
                    if (key == KnownKey.Id)
                        _id = value;
                    else if (key == KnownKey.Type && _type == null)
                    {
                        _type = value;
                        _typeInMetadata = false;
                    }

                }
                else if (knownKey == KnownKey.Metadata)
                {
                    if (key == KnownKey.Type)
                    {
                        _type = value;
                        _typeInMetadata = true;
                    }
                    else if (key == KnownKey.Guid)
                        _guid = value;
                }
            }
        }
        if (!ended && !_hasError)
            ErrorMessage = $"missing closing curly bracket '}}' for dictionary opened on line {currentLineNumber}";
        
        SkipWhitespaceAndComments();
    }

    private void SkipList()
    {
        int currentLineNumber = _currentLineNumber;
        ReadChar();
        SkipWhitespaceAndComments();

        bool flag = false;
        while (_hasChar)
        {
            if (_currentChar == '/')
            {
                SkipWhitespaceAndComments();
            }
            else
            {
                if (_currentChar == ']')
                {
                    ReadChar();
                    flag = true;
                    break;
                }
                if (_currentChar == '{')
                    ReadDictionary(KnownKey.None);
                else if (_currentChar == '[')
                {
                    SkipList();
                }
                else
                {
                    SkipString();
                    SkipWhitespaceAndComments();
                }
            }
        }
        if (!flag && !_hasError)
            ErrorMessage = $"missing closing bracket ']' for list opened on line {currentLineNumber}";
        SkipWhitespaceAndComments();
    }


    private string ReadString()
    {
        if (_currentChar == '"')
            return ReadQuotedString();
        bool flag = false;
        stringBuilder.Clear();
        while (_hasChar)
        {
            if (flag)
            {
                if (_currentChar == 'n')
                    _currentChar = '\n';
                else if (_currentChar == 't')
                    _currentChar = '\t';
                else if (_currentChar != '\\')
                {
                    stringBuilder.Append('\\');
                    if (!_hasError)
                        ErrorMessage = $"unrecognized escape sequence (\\{_currentChar}) on line {_currentLineNumber} - if this is a file path please use forward slash (/)";
                }
            }
            else if (_currentChar != '\r' && _currentChar != '\n')
            {
                if (_currentChar == '\\')
                {
                    flag = true;
                    ReadChar();
                    continue;
                }
            }
            else
                break;
            flag = false;
            stringBuilder.Append(_currentChar);
            ReadChar();
        }
        return stringBuilder.ToString();
    }

    private void SkipString()
    {
        if (_currentChar == '"')
        {
            SkipQuotedString();
            return;
        }

        bool flag = false;
        while (_hasChar)
        {
            if (flag)
            {
                if (_currentChar == 'n')
                    _currentChar = '\n';
                else if (_currentChar == 't')
                    _currentChar = '\t';
                else if (_currentChar != '\\')
                {
                    if (!_hasError)
                        ErrorMessage = $"unrecognized escape sequence (\\{_currentChar}) on line {_currentLineNumber} - if this is a file path please use forward slash (/)";
                }
            }
            else if (_currentChar != '\r' && _currentChar != '\n')
            {
                if (_currentChar == '\\')
                {
                    flag = true;
                    ReadChar();
                    continue;
                }
            }
            else
                break;

            flag = false;
            ReadChar();
        }
    }

    private KnownKey ReadDictionaryKey()
    {
        if (_currentChar == '"')
            return ReadQuotedKey();

        KnownKey knownKey = 0;
        int knownKeyIndex = 0;

        while (_hasChar && !char.IsWhiteSpace(_currentChar))
        {
            ReadKnownKey(ref knownKeyIndex, ref knownKey);
            ReadChar();
        }

        return knownKeyIndex > 0 ? knownKey : KnownKey.None;
    }

    private string ReadQuotedString()
    {
        int currentLineNumber = _currentLineNumber;
        ReadChar();
        bool isNextEscapeChar = false;
        bool hasClosedQuotes = false;

        stringBuilder.Clear();
        while (_hasChar)
        {
            if (isNextEscapeChar)
            {
                if (_currentChar == 'n')
                    _currentChar = '\n';
                else if (_currentChar == 't')
                    _currentChar = '\t';
                else if (_currentChar != '\\' && _currentChar != '"')
                {
                    stringBuilder.Append('\\');
                    if (!_hasError)
                        ErrorMessage = $"unrecognized escape sequence (\\{_currentChar}) on line {_currentLineNumber} - if this is a file path please use forward slash (/)";
                }
            }
            else
            {
                if (_currentChar == '"')
                {
                    ReadChar();
                    hasClosedQuotes = true;
                    break;
                }
                if (_currentChar == '\\')
                {
                    isNextEscapeChar = true;
                    ReadChar();
                    continue;
                }
            }
            isNextEscapeChar = false;

            stringBuilder.Append(_currentChar);
            ReadChar();
        }
        if (!hasClosedQuotes && !_hasError)
            ErrorMessage = $"missing closing quotation mark (\") for string opened on line {currentLineNumber}";
        return stringBuilder.ToString();
    }

    private void SkipQuotedString()
    {
        int currentLineNumber = _currentLineNumber;
        ReadChar();
        bool isNextEscapeChar = false;
        bool hasClosedQuotes = false;

        while (_hasChar)
        {
            if (isNextEscapeChar)
            {
                if (_currentChar == 'n')
                    _currentChar = '\n';
                else if (_currentChar == 't')
                    _currentChar = '\t';
                else if (_currentChar != '\\' && _currentChar != '"')
                {
                    if (!_hasError)
                        ErrorMessage = $"unrecognized escape sequence (\\{_currentChar}) on line {_currentLineNumber} - if this is a file path please use forward slash (/)";
                }
            }
            else
            {
                if (_currentChar == '"')
                {
                    ReadChar();
                    hasClosedQuotes = true;
                    break;
                }
                if (_currentChar == '\\')
                {
                    isNextEscapeChar = true;
                    ReadChar();
                    continue;
                }
            }
            isNextEscapeChar = false;

            ReadChar();
        }
        if (!hasClosedQuotes && !_hasError)
            ErrorMessage = $"missing closing quotation mark (\") for string opened on line {currentLineNumber}";
    }

    private KnownKey ReadQuotedKey()
    {
        int currentLineNumber = _currentLineNumber;
        ReadChar();
        bool isNextEscapeChar = false;
        bool hasClosedQuotes = false;

        KnownKey knownKey = 0;
        int knownKeyIndex = 0;

        while (_hasChar)
        {
            if (isNextEscapeChar)
            {
                if (_currentChar == 'n')
                    _currentChar = '\n';
                else if (_currentChar == 't')
                    _currentChar = '\t';
                else if (_currentChar != '\\' && _currentChar != '"')
                {
                    if (!_hasError)
                        ErrorMessage = $"unrecognized escape sequence (\\{_currentChar}) on line {_currentLineNumber} - if this is a file path please use forward slash (/)";
                }
            }
            else
            {
                if (_currentChar == '"')
                {
                    ReadChar();
                    hasClosedQuotes = true;
                    break;
                }
                if (_currentChar == '\\')
                {
                    isNextEscapeChar = true;
                    ReadChar();
                    continue;
                }
            }
            isNextEscapeChar = false;

            ReadKnownKey(ref knownKeyIndex, ref knownKey);
            ReadChar();
        }
        if (!hasClosedQuotes && !_hasError)
            ErrorMessage = $"missing closing quotation mark (\") for string opened on line {currentLineNumber}";
        return knownKeyIndex > 0 ? knownKey : KnownKey.None;
    }

    private void ReadKnownKey(ref int knownKeyIndex, ref KnownKey knownKey)
    {
        if (knownKeyIndex == 0)
        {
            for (KnownKey key = KnownKey.Metadata; (int)key < _knownKeys.Length; ++key)
            {
                if (!EqualsCaseInsensitive(_knownKeys[(int)key][0], _currentChar))
                    continue;

                knownKey = key;
                knownKeyIndex = 1;
                break;
            }
        }
        else if (knownKeyIndex > 0 && knownKeyIndex < _knownKeys[(int)knownKey].Length)
        {
            if (!EqualsCaseInsensitive(_knownKeys[(int)knownKey][knownKeyIndex], _currentChar))
            {
                knownKeyIndex = -1;
            }
            else ++knownKeyIndex;
        }
        else if (knownKeyIndex == _knownKeys[(int)knownKey].Length)
        {
            knownKeyIndex = -1;
        }
    }

    private static bool EqualsCaseInsensitive(char a, char b)
    {
        if (a == b) return true;
        if (a is >= 'a' and <= 'z')
            return b == a - 32;
        else if (a is >= 'A' and <= 'Z')
            return b == a + 32;
        return false;
    }

    private static readonly char[][] _knownKeys =
    [
        null!,
        [ 'm', 'e', 't', 'a', 'd', 'a', 't', 'a' ],
        [ 'a', 's', 's', 'e', 't' ],
        [ 'g', 'u', 'i', 'd' ],
        [ 'i', 'd' ],
        [ 't', 'y', 'p', 'e' ]
    ];

    private enum KnownKey
    {
        None,
        Metadata,
        Asset,
        Guid,
        Id,
        Type
    }
}
public readonly struct QuickAssetInfo
{
    public readonly Type? AssetType;
    public readonly Guid Guid;
    public readonly ushort Id;
    public QuickAssetInfo(Type? assetType, Guid guid, ushort id)
    {
        AssetType = assetType;
        Guid = guid;
        Id = id;
    }
}