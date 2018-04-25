//---------------------------------------------------------------------
// <copyright file="DbConnectionOptions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner  Microsoft
//---------------------------------------------------------------------

namespace System.Data.EntityClient
{
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Copied from System.Data.dll
    /// </summary>
    internal class DbConnectionOptions
    {
        // instances of this class are intended to be immutable, i.e readonly
        // used by pooling classes so it is much easier to verify correctness
        // when not worried about the class being modified during execution

#if DEBUG
        private const string ConnectionStringPattern =                  // may not contain embedded null except trailing last value
            "([\\s;]*"                                                  // leading whitespace and extra semicolons
            + "(?![\\s;])"                                              // key does not start with space or semicolon
            + "(?<key>([^=\\s\\p{Cc}]|\\s+[^=\\s\\p{Cc}]|\\s+==|==)+)"  // allow any visible character for keyname except '=' which must quoted as '=='
            + "\\s*=(?!=)\\s*"                                          // the equal sign divides the key and value parts
            + "(?<value>"
            + "(\"([^\"\u0000]|\"\")*\")"                              // double quoted string, " must be quoted as ""
            + "|"
            + "('([^'\u0000]|'')*')"                                   // single quoted string, ' must be quoted as ''
            + "|"
            + "((?![\"'\\s])"                                          // unquoted value must not start with " or ' or space, would also like = but too late to change
            + "([^;\\s\\p{Cc}]|\\s+[^;\\s\\p{Cc}])*"                  // control characters must be quoted
            + "(?<![\"']))"                                            // unquoted value must not stop with " or '
            + ")(\\s*)(;|\u0000|$)"                                     // whitespace after value up to semicolon or end-of-line
            + ")*"                                                      // repeat the key-value pair
            + "[\\s;\u0000]*"                                           // traling whitespace/semicolons and embedded nulls (DataSourceLocator)
        ;

        private static readonly Regex ConnectionStringRegex = new Regex(ConnectionStringPattern, RegexOptions.ExplicitCapture | RegexOptions.Compiled);
#endif
        internal const string DataDirectory = "|datadirectory|";

#if DEBUG 
        private const string ConnectionStringValidKeyPattern = "^(?![;\\s])[^\\p{Cc}]+(?<!\\s)$"; // key not allowed to start with semi-colon or space or contain non-visible characters or end with space
        private const string ConnectionStringValidValuePattern = "^[^\u0000]*$";                    // value not allowed to contain embedded null   
        private static readonly Regex ConnectionStringValidKeyRegex = new Regex(ConnectionStringValidKeyPattern, RegexOptions.Compiled);
        private static readonly Regex ConnectionStringValidValueRegex = new Regex(ConnectionStringValidValuePattern, RegexOptions.Compiled);
#endif

        private readonly string _usersConnectionString;
        private readonly Hashtable _parsetable;
        internal readonly NameValuePair KeyChain;

        // synonyms hashtable is meant to be read-only translation of parsed string
        // keywords/synonyms to a known keyword string
        internal DbConnectionOptions(string connectionString, Hashtable synonyms)
        {
            _parsetable = new Hashtable();
            _usersConnectionString = ((null != connectionString) ? connectionString : "");

            // first pass on parsing, initial syntax check
            if (0 < _usersConnectionString.Length)
            {
                KeyChain = ParseInternal(_parsetable, _usersConnectionString, synonyms);
            }
        }

        internal string UsersConnectionString
        {
            get
            {
                return _usersConnectionString ?? string.Empty;
            }
        }

        internal bool IsEmpty
        {
            get { return (null == KeyChain); }
        }

        internal Hashtable Parsetable
        {
            get { return _parsetable; }
        }

        internal string this[string keyword]
        {
            get { return (string)_parsetable[keyword]; }
        }

        // SxS notes:
        // * this method queries "DataDirectory" value from the current AppDomain.
        //   This string is used for to replace "!DataDirectory!" values in the connection string, it is not considered as an "exposed resource".
        // * This method uses GetFullPath to validate that root path is valid, the result is not exposed out.
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal static string ExpandDataDirectory(string keyword, string value)
        {
            string fullPath = null;
            if ((null != value) && value.StartsWith(DataDirectory, StringComparison.OrdinalIgnoreCase))
            {
                // find the replacement path
                object rootFolderObject = AppDomain.CurrentDomain.GetData("DataDirectory");
                string rootFolderPath = (rootFolderObject as string);
                if ((null != rootFolderObject) && (null == rootFolderPath))
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.ADP_InvalidDataDirectory);
                }
                else if (rootFolderPath == string.Empty)
                {
                    rootFolderPath = AppDomain.CurrentDomain.BaseDirectory;
                }
                if (null == rootFolderPath)
                {
                    rootFolderPath = "";                    
                }

                // We don't know if rootFolderpath ends with '\', and we don't know if the given name starts with onw
                int fileNamePosition = DataDirectory.Length;    // filename starts right after the '|datadirectory|' keyword
                bool rootFolderEndsWith = (0 < rootFolderPath.Length) && rootFolderPath[rootFolderPath.Length - 1] == '\\';
                bool fileNameStartsWith = (fileNamePosition < value.Length) && value[fileNamePosition] == '\\';

                // replace |datadirectory| with root folder path
                if (!rootFolderEndsWith && !fileNameStartsWith)
                {
                    // need to insert '\'
                    fullPath = rootFolderPath + '\\' + value.Substring(fileNamePosition);
                }
                else if (rootFolderEndsWith && fileNameStartsWith)
                {
                    // need to strip one out
                    fullPath = rootFolderPath + value.Substring(fileNamePosition + 1);
                }
                else
                {
                    // simply concatenate the strings
                    fullPath = rootFolderPath + value.Substring(fileNamePosition);
                }

                // verify root folder path is a real path without unexpected "..\"
                if (!EntityUtil.GetFullPath(fullPath).StartsWith(rootFolderPath, StringComparison.Ordinal))
                {
                    throw EntityUtil.InvalidConnectionOptionValue(keyword);
                }
            }
            return fullPath;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        static private string GetKeyName(StringBuilder buffer)
        {
            int count = buffer.Length;
            while ((0 < count) && Char.IsWhiteSpace(buffer[count - 1]))
            {
                count--; // trailing whitespace
            }
            return buffer.ToString(0, count).ToLowerInvariant();
        }

        static private string GetKeyValue(StringBuilder buffer, bool trimWhitespace)
        {
            int count = buffer.Length;
            int index = 0;
            if (trimWhitespace)
            {
                while ((index < count) && Char.IsWhiteSpace(buffer[index]))
                {
                    index++; // leading whitespace
                }
                while ((0 < count) && Char.IsWhiteSpace(buffer[count - 1]))
                {
                    count--; // trailing whitespace
                }
            }
            return buffer.ToString(index, count - index);
        }

        // transistion states used for parsing
        private enum ParserState
        {
            NothingYet = 1,   //start point
            Key,
            KeyEqual,
            KeyEnd,
            UnquotedValue,
            DoubleQuoteValue,
            DoubleQuoteValueQuote,
            SingleQuoteValue,
            SingleQuoteValueQuote,
            QuotedValueEnd,
            NullTermination,
        };
        
        static private int GetKeyValuePair(string connectionString, int currentPosition, StringBuilder buffer, out string keyname, out string keyvalue)
        {
            int startposition = currentPosition;

            buffer.Length = 0;
            keyname = null;
            keyvalue = null;

            char currentChar = '\0';

            ParserState parserState = ParserState.NothingYet;
            int length = connectionString.Length;
            for (; currentPosition < length; ++currentPosition)
            {
                currentChar = connectionString[currentPosition];

                switch (parserState)
                {
                    case ParserState.NothingYet: // [\\s;]*
                        if ((';' == currentChar) || Char.IsWhiteSpace(currentChar))
                        {
                            continue;
                        }
                        if ('\0' == currentChar) { parserState = ParserState.NullTermination; continue; } // MDAC 83540
                        if (Char.IsControl(currentChar)) { throw EntityUtil.ConnectionStringSyntax(startposition); }
                        startposition = currentPosition;
                        if ('=' != currentChar)
                        { // MDAC 86902
                            parserState = ParserState.Key;
                            break;
                        }
                        else
                        {
                            parserState = ParserState.KeyEqual;
                            continue;
                        }

                    case ParserState.Key: // (?<key>([^=\\s\\p{Cc}]|\\s+[^=\\s\\p{Cc}]|\\s+==|==)+)
                        if ('=' == currentChar) { parserState = ParserState.KeyEqual; continue; }
                        if (Char.IsWhiteSpace(currentChar)) { break; }
                        if (Char.IsControl(currentChar)) { throw EntityUtil.ConnectionStringSyntax(startposition); }
                        break;

                    case ParserState.KeyEqual: // \\s*=(?!=)\\s*
                        if ('=' == currentChar) { parserState = ParserState.Key; break; }
                        keyname = GetKeyName(buffer);
                        if (string.IsNullOrEmpty(keyname)) { throw EntityUtil.ConnectionStringSyntax(startposition); }
                        buffer.Length = 0;
                        parserState = ParserState.KeyEnd;
                        goto case ParserState.KeyEnd;

                    case ParserState.KeyEnd:
                        if (Char.IsWhiteSpace(currentChar)) { continue; }
                        if ('\'' == currentChar) { parserState = ParserState.SingleQuoteValue; continue; }
                        if ('"' == currentChar) { parserState = ParserState.DoubleQuoteValue; continue; }

                        if (';' == currentChar) { goto ParserExit; }
                        if ('\0' == currentChar) { goto ParserExit; }
                        if (Char.IsControl(currentChar)) { throw EntityUtil.ConnectionStringSyntax(startposition); }
                        parserState = ParserState.UnquotedValue;
                        break;

                    case ParserState.UnquotedValue: // "((?![\"'\\s])" + "([^;\\s\\p{Cc}]|\\s+[^;\\s\\p{Cc}])*" + "(?<![\"']))"
                        if (Char.IsWhiteSpace(currentChar)) { break; }
                        if (Char.IsControl(currentChar) || ';' == currentChar) { goto ParserExit; }
                        break;

                    case ParserState.DoubleQuoteValue: // "(\"([^\"\u0000]|\"\")*\")"
                        if ('"' == currentChar) { parserState = ParserState.DoubleQuoteValueQuote; continue; }
                        if ('\0' == currentChar) { throw EntityUtil.ConnectionStringSyntax(startposition); }
                        break;

                    case ParserState.DoubleQuoteValueQuote:
                        if ('"' == currentChar) { parserState = ParserState.DoubleQuoteValue; break; }
                        keyvalue = GetKeyValue(buffer, false);
                        parserState = ParserState.QuotedValueEnd;
                        goto case ParserState.QuotedValueEnd;

                    case ParserState.SingleQuoteValue: // "('([^'\u0000]|'')*')"
                        if ('\'' == currentChar) { parserState = ParserState.SingleQuoteValueQuote; continue; }
                        if ('\0' == currentChar) { throw EntityUtil.ConnectionStringSyntax(startposition); }
                        break;

                    case ParserState.SingleQuoteValueQuote:
                        if ('\'' == currentChar) { parserState = ParserState.SingleQuoteValue; break; }
                        keyvalue = GetKeyValue(buffer, false);
                        parserState = ParserState.QuotedValueEnd;
                        goto case ParserState.QuotedValueEnd;

                    case ParserState.QuotedValueEnd:
                        if (Char.IsWhiteSpace(currentChar)) { continue; }
                        if (';' == currentChar) { goto ParserExit; }
                        if ('\0' == currentChar) { parserState = ParserState.NullTermination; continue; } // MDAC 83540
                        throw EntityUtil.ConnectionStringSyntax(startposition);  // unbalanced single quote

                    case ParserState.NullTermination: // [\\s;\u0000]*
                        if ('\0' == currentChar) { continue; }
                        if (Char.IsWhiteSpace(currentChar)) { continue; } // MDAC 83540
                        throw EntityUtil.ConnectionStringSyntax(currentPosition);

                    default:
                        throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidParserState1);
                }
                buffer.Append(currentChar);
            }
        ParserExit:
            switch (parserState)
            {
                case ParserState.Key:
                case ParserState.DoubleQuoteValue:
                case ParserState.SingleQuoteValue:
                    // keyword not found/unbalanced double/single quote
                    throw EntityUtil.ConnectionStringSyntax(startposition);

                case ParserState.KeyEqual:
                    // equal sign at end of line
                    keyname = GetKeyName(buffer);
                    if (string.IsNullOrEmpty(keyname)) { throw EntityUtil.ConnectionStringSyntax(startposition); }
                    break;

                case ParserState.UnquotedValue:
                    // unquoted value at end of line
                    keyvalue = GetKeyValue(buffer, true);

                    char tmpChar = keyvalue[keyvalue.Length - 1];
                    if (('\'' == tmpChar) || ('"' == tmpChar))
                    {
                        throw EntityUtil.ConnectionStringSyntax(startposition);    // unquoted value must not end in quote
                    }
                    break;

                case ParserState.DoubleQuoteValueQuote:
                case ParserState.SingleQuoteValueQuote:
                case ParserState.QuotedValueEnd:
                    // quoted value at end of line
                    keyvalue = GetKeyValue(buffer, false);
                    break;

                case ParserState.NothingYet:
                case ParserState.KeyEnd:
                case ParserState.NullTermination:
                    // do nothing
                    break;

                default:
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidParserState2);
            }
            if ((';' == currentChar) && (currentPosition < connectionString.Length))
            {
                currentPosition++;
            }
            return currentPosition;
        }

#if DEBUG
        static private bool IsValueValidInternal(string keyvalue)
        {
            if (null != keyvalue)
            {

                bool compValue = ConnectionStringValidValueRegex.IsMatch(keyvalue);
                Debug.Assert((-1 == keyvalue.IndexOf('\u0000')) == compValue, "IsValueValid mismatch with regex");
                return (-1 == keyvalue.IndexOf('\u0000'));
            }
            return true;
        }
#endif

        static private bool IsKeyNameValid(string keyname)
        {
            if (null != keyname)
            {
#if DEBUG
                bool compValue = ConnectionStringValidKeyRegex.IsMatch(keyname);
                Debug.Assert(((0 < keyname.Length) && (';' != keyname[0]) && !Char.IsWhiteSpace(keyname[0]) && (-1 == keyname.IndexOf('\u0000'))) == compValue, "IsValueValid mismatch with regex");
#endif
                return ((0 < keyname.Length) && (';' != keyname[0]) && !Char.IsWhiteSpace(keyname[0]) && (-1 == keyname.IndexOf('\u0000')));
            }
            return false;
        }

#if DEBUG
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static Hashtable SplitConnectionString(string connectionString, Hashtable synonyms)
        {
            Hashtable parsetable = new Hashtable();
            Regex parser = ConnectionStringRegex;

            const int KeyIndex = 1, ValueIndex = 2;
            Debug.Assert(KeyIndex == parser.GroupNumberFromName("key"), "wrong key index");
            Debug.Assert(ValueIndex == parser.GroupNumberFromName("value"), "wrong value index");

            if (null != connectionString)
            {
                Match match = parser.Match(connectionString);
                if (!match.Success || (match.Length != connectionString.Length))
                {
                    throw EntityUtil.ConnectionStringSyntax(match.Length);
                }
                int indexValue = 0;
                CaptureCollection keyvalues = match.Groups[ValueIndex].Captures;
                foreach (Capture keypair in match.Groups[KeyIndex].Captures)
                {
                    string keyname = keypair.Value.Replace("==", "=").ToLowerInvariant();
                    string keyvalue = keyvalues[indexValue++].Value;
                    if (0 < keyvalue.Length)
                    {
                        switch (keyvalue[0])
                        {
                            case '\"':
                                keyvalue = keyvalue.Substring(1, keyvalue.Length - 2).Replace("\"\"", "\"");
                                break;
                            case '\'':
                                keyvalue = keyvalue.Substring(1, keyvalue.Length - 2).Replace("\'\'", "\'");
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        keyvalue = null;
                    }

                    string realkeyname = ((null != synonyms) ? (string)synonyms[keyname] : keyname);
                    if (!IsKeyNameValid(realkeyname))
                    {
                        throw EntityUtil.ADP_KeywordNotSupported(keyname);
                    }
                    parsetable[realkeyname] = keyvalue; // last key-value pair wins (or first)
                }
            }
            return parsetable;
        }

        private static void ParseComparision(Hashtable parsetable, string connectionString, Hashtable synonyms, Exception e)
        {
            try
            {
                Hashtable parsedvalues = SplitConnectionString(connectionString, synonyms);
                foreach (DictionaryEntry entry in parsedvalues)
                {
                    string keyname = (string)entry.Key;
                    string value1 = (string)entry.Value;
                    string value2 = (string)parsetable[keyname];
                    Debug.Assert(parsetable.Contains(keyname), "ParseInternal code vs. regex mismatch keyname <" + keyname + ">");
                    Debug.Assert(value1 == value2, "ParseInternal code vs. regex mismatch keyvalue <" + value1 + "> <" + value2 + ">");
                }

            }
            catch (ArgumentException f)
            {
                if (null != e)
                {
                    string msg1 = e.Message;
                    string msg2 = f.Message;
                    if (msg1.StartsWith("Keyword not supported:", StringComparison.Ordinal) && msg2.StartsWith("Format of the initialization string", StringComparison.Ordinal))
                    {
                    }
                    else
                    {
                        // Does not always hold.
                       Debug.Assert(msg1 == msg2, "ParseInternal code vs regex message mismatch: <" + msg1 + "> <" + msg2 + ">");
                    }
                }
                else
                {
                    Debug.Assert(false, "ParseInternal code vs regex throw mismatch " + f.Message);
                }
                e = null;
            }
            if (null != e)
            {
                Debug.Assert(false, "ParseInternal code threw exception vs regex mismatch");
            }
        }
#endif
        private static NameValuePair ParseInternal(Hashtable parsetable, string connectionString, Hashtable synonyms)
        {
            Debug.Assert(null != connectionString, "null connectionstring");
            StringBuilder buffer = new StringBuilder();
            NameValuePair localKeychain = null, keychain = null;
#if DEBUG
            try
            {
#endif
                int nextStartPosition = 0;
                int endPosition = connectionString.Length;
                while (nextStartPosition < endPosition)
                {
                    int startPosition = nextStartPosition;

                    string keyname, keyvalue;
                    nextStartPosition = GetKeyValuePair(connectionString, startPosition, buffer, out keyname, out keyvalue);
                    if (string.IsNullOrEmpty(keyname))
                    {
                        // if (nextStartPosition != endPosition) { throw; }
                        break;
                    }

#if DEBUG
                    Debug.Assert(IsKeyNameValid(keyname), "ParseFailure, invalid keyname");
                    Debug.Assert(IsValueValidInternal(keyvalue), "parse failure, invalid keyvalue");
#endif
                    string realkeyname = ((null != synonyms) ? (string)synonyms[keyname] : keyname);
                    if (!IsKeyNameValid(realkeyname))
                    {
                        throw EntityUtil.ADP_KeywordNotSupported(keyname);
                    }
                    parsetable[realkeyname] = keyvalue; // last key-value pair wins (or first)

                    if (null != localKeychain)
                    {
                        localKeychain = localKeychain.Next = new NameValuePair(realkeyname, keyvalue, nextStartPosition - startPosition);
                    }
                    else 
                    { // first time only - don't contain modified chain from UDL file
                        keychain = localKeychain = new NameValuePair(realkeyname, keyvalue, nextStartPosition - startPosition);
                    }
                }
#if DEBUG
            }
            catch (ArgumentException e)
            {
                ParseComparision(parsetable, connectionString, synonyms, e);
                throw;
            }
            ParseComparision(parsetable, connectionString, synonyms, null);
#endif
            return keychain;
        } 
    }
}
