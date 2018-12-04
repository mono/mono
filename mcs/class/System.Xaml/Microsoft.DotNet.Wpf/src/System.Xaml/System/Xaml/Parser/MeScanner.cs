// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Diagnostics;
using System.Xaml;
using MS.Internal.Xaml.Context;
using System.Xaml.Schema;
using System.Xaml.MS.Impl;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MS.Internal.Xaml.Parser
{
    // Markup Extension Tokenizer AKA Scanner.

    enum MeTokenType
    {
        None,
        Open         = '{',
        Close        = '}',
        EqualSign    = '=',
        Comma        = ',',
        TypeName,      // String - Follows a '{' space delimited
        PropertyName,  // String - Preceeds a '='.  {},= delimited, can (but shouldn't) contain spaces.
        String,        // String - all other strings, {},= delimited can contain spaces.
        QuotedMarkupExtension // String - must be recursivly parsed as a MarkupExtension.
    };

    // 1) Value and (propertynames for compatibility with WPF 3.0) can also have
    // escaped character with '\' to include '{' '}' ',' '=', and '\'.
    // 2) Value strings can also be quoted (w/ ' or ") in their entirity to escape all
    // uses of the above characters.
    // 3) All strings are trimmed of whitespace front and back unless they were quoted.
    // 4) Quote characters can only appear at the start and end of strings.
    // 5) TypeNames cannot be quoted.
    
    internal class MeScanner
    {
        public const char Space = ' ';
        public const char OpenCurlie = '{';
        public const char CloseCurlie = '}';
        public const char Comma = ',';
        public const char EqualSign = '=';
        public const char Quote1 = '\'';
        public const char Quote2 = '\"';
        public const char Backslash = '\\';
        public const char NullChar = '\0';

        enum StringState { Value, Type, Property };

        XamlParserContext _context;
        string _inputText;
        int _idx;
        MeTokenType _token;
        XamlType _tokenXamlType;
        XamlMember _tokenProperty;
        string _tokenNamespace;
        string _tokenText;
        StringState _state;
        bool _hasTrailingWhitespace;
        int _lineNumber;
        int _startPosition;
        private string _currentParameterName;
        private SpecialBracketCharacters _currentSpecialBracketCharacters;

        public MeScanner(XamlParserContext context, string text, int lineNumber, int linePosition)
        {
            _context = context;
            _inputText = text;
            _lineNumber = lineNumber;
            _startPosition = linePosition;
            _idx = -1;
            _state = StringState.Value;
            _currentParameterName = null;
            _currentSpecialBracketCharacters = null;
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public int LinePosition
        {
            get
            {
                int offset = (_idx < 0) ? 0 : _idx;
                return _startPosition + offset;
            }
        }

        public string Namespace
        {
            get { return _tokenNamespace; }
        }

        public MeTokenType Token
        {
            get { return _token; }
        }

        public XamlType TokenType
        {
            get { return _tokenXamlType; }
        }

        public XamlMember TokenProperty
        {
            get { return _tokenProperty; }
        }

        // FxCop says this is never called
        //  (but _tokenNamespace is used internally in the Scanner)
        //public XamlNamespace TokenNamespace
        //{
        //    get { return _tokenNamespace; }
        //}

        public string TokenText
        {
            get { return _tokenText; }
        }

        public bool IsAtEndOfInput
        {
            get { return (_idx >= _inputText.Length); }
        }

        public bool HasTrailingWhitespace
        {
            get { return _hasTrailingWhitespace; }
        }

        public void Read()
        {
            bool isQuotedMarkupExtension = false;
            bool readString = false;

            _tokenText = String.Empty;
            _tokenXamlType = null;
            _tokenProperty = null;
            _tokenNamespace = null;

            Advance();
            AdvanceOverWhitespace();

            if (IsAtEndOfInput)
            {
                _token = MeTokenType.None;
                return;
            }

            switch (CurrentChar)
            {
            case OpenCurlie:
                if (NextChar == CloseCurlie)    // the {} escapes the ME.  return the string.
                {
                    _token = MeTokenType.String;
                    _state = StringState.Value;
                    readString = true;          // ReadString() will strip the leading {}
                }
                else
                {
                    _token = MeTokenType.Open;
                    _state = StringState.Type;  // types follow '{'
                }
                break;

            case Quote1:
            case Quote2:
                if (NextChar == OpenCurlie)
                {
                    Advance();                    // read ahead one character
                    if (NextChar != CloseCurlie)  // check for the '}' of a {}
                    {
                        isQuotedMarkupExtension = true;
                    }
                    PushBack();                   // put back the read-ahead.
                }
                readString = true;  // read substring"
                break;

            case CloseCurlie:
                _token = MeTokenType.Close;
                _state = StringState.Value;
                break;

            case EqualSign:
                _token = MeTokenType.EqualSign;
                _state = StringState.Value;
                _context.CurrentBracketModeParseParameters.IsConstructorParsingMode = false;
                break;

            case Comma:
                _token = MeTokenType.Comma;
                _state = StringState.Value;
                if (_context.CurrentBracketModeParseParameters.IsConstructorParsingMode)
                {
                    _context.CurrentBracketModeParseParameters.IsConstructorParsingMode =
                        ++_context.CurrentBracketModeParseParameters.CurrentConstructorParam <
                        _context.CurrentBracketModeParseParameters.MaxConstructorParams;
                }
                break;

            default:
                readString = true;
                break;
            }

            if(readString)
            {
                if (_context.CurrentType.IsMarkupExtension 
                    && _context.CurrentBracketModeParseParameters != null 
                    && _context.CurrentBracketModeParseParameters.IsConstructorParsingMode)
                {
                    int currentCtrParam = _context.CurrentBracketModeParseParameters.CurrentConstructorParam;
                    _currentParameterName = _context.CurrentLongestConstructorOfMarkupExtension[currentCtrParam].Name;
                    _currentSpecialBracketCharacters = GetBracketCharacterForProperty(_currentParameterName);
                }

                string str = ReadString();
                _token = (isQuotedMarkupExtension) ? MeTokenType.QuotedMarkupExtension : MeTokenType.String;

                switch (_state)
                {
                case StringState.Value:
                    break;

                case StringState.Type:
                    _token = MeTokenType.TypeName;
                    ResolveTypeName(str);
                    break;

                case StringState.Property:
                    _token = MeTokenType.PropertyName;
                    ResolvePropertyName(str);
                    break;
                }
                _state = StringState.Value;
                _tokenText = RemoveEscapes(str);
            }
        }

        private static string RemoveEscapes(string value)
        {
            if (value.StartsWith("{}", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2);
            } 
            
            if (!value.Contains("\\"))
            {
                return value;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            int start = 0;
            int idx;
            do
            {
                idx = value.IndexOf(Backslash, start);
                if (idx < 0)
                {
                    builder.Append(value.Substring(start));
                    break;
                }
                else
                {
                    int clearTextLength = idx - start;

                    // Copy Clear Text
                    builder.Append(value.Substring(start, clearTextLength));

                    // Add the character after the backslash
                    if (idx + 1 < value.Length)
                    {
                        builder.Append(value[idx + 1]);
                    }

                    // pick up again after that
                    start = idx + 2;
                }
            } while (start < value.Length);
            string result = builder.ToString();
            return result;
        }

        private void ResolveTypeName(string longName)
        {
            string error;
            XamlTypeName typeName = XamlTypeName.ParseInternal(longName, _context.FindNamespaceByPrefix, out error);
            if (typeName == null)
            {
                throw new XamlParseException(this, error);
            }
            
            // In curly form, we search for TypeName + 'Extension' before TypeName
            string bareTypeName = typeName.Name;
            typeName.Name = typeName.Name + KnownStrings.Extension;
            XamlType xamlType = _context.GetXamlType(typeName, false);
            // This would be cleaner if we moved the Extension fallback logic out of XSC
            if (xamlType == null || 
                // Guard against Extension getting added twice
                (xamlType.UnderlyingType != null && 
                 KS.Eq(xamlType.UnderlyingType.Name, typeName.Name + KnownStrings.Extension)))
            {
                typeName.Name = bareTypeName;
                xamlType = _context.GetXamlType(typeName, true);
            }

            _tokenXamlType = xamlType;
            _tokenNamespace = typeName.Namespace;
        }

        private void ResolvePropertyName(string longName)
        {
            XamlPropertyName propName = XamlPropertyName.Parse(longName);
            if (propName == null)
            {
                throw new ArgumentException(SR.Get(SRID.MalformedPropertyName));
            }

            XamlMember prop = null;
            XamlType declaringType;
            XamlType tagType = _context.CurrentType;
            string tagNamespace = _context.CurrentTypeNamespace;

            if (propName.IsDotted)
            {
                prop = _context.GetDottedProperty(tagType, tagNamespace, propName, false /*tagIsRoot*/);
            }
            // Regular property p
            else
            {
                // _tokenNamespace is always null here
                string ns = _context.GetAttributeNamespace(propName, _tokenNamespace);
                declaringType = _context.CurrentType;

                // _tokenNamespace is always null here
                prop = _context.GetNoDotAttributeProperty(declaringType, propName, _tokenNamespace, ns, false /*tagIsRoot*/);
            }
            _tokenProperty = prop;
        }

        private string ReadString()
        {
            bool escaped = false;
            char quoteChar = NullChar;
            bool atStart = true;
            bool wasQuoted = false;
            uint braceCount = 0;    // To be compat with v3 which allowed balanced {} inside of strings

            StringBuilder sb = new StringBuilder();
            char ch;

            while(!IsAtEndOfInput)
            {
                ch = CurrentChar;

                // handle escaping and quoting first.
                if(escaped)
                {
                    sb.Append('\\');
                    sb.Append(ch);
                    escaped = false;
                }
                else if (quoteChar != NullChar)
                {
                    if (ch == Backslash)
                    {
                        escaped = true;
                    }
                    else if (ch != quoteChar)
                    {
                        sb.Append(ch);
                    }
                    else
                    {
                        ch = CurrentChar;
                        quoteChar = NullChar;
                        break;  // we are done.
                    }
                }
                // If we are inside of MarkupExtensionBracketCharacters for a particular property or position parameter,
                // scoop up everything inside one by one, and keep track of nested Bracket Characters in the stack. 
                else if (_context.CurrentBracketModeParseParameters != null && _context.CurrentBracketModeParseParameters.IsBracketEscapeMode)
                {
                    Stack<char> bracketCharacterStack = _context.CurrentBracketModeParseParameters.BracketCharacterStack;
                    if (_currentSpecialBracketCharacters.StartsEscapeSequence(ch))
                    {
                        bracketCharacterStack.Push(ch);
                    }
                    else if (_currentSpecialBracketCharacters.EndsEscapeSequence(ch))
                    {
                        if (_currentSpecialBracketCharacters.Match(bracketCharacterStack.Peek(), ch))
                        {
                            bracketCharacterStack.Pop();
                        }
                        else
                        {
                            throw new XamlParseException(this, SR.Get(SRID.InvalidClosingBracketCharacers, ch.ToString()));
                        }
                    }
                    else if (ch == MeScanner.Backslash)
                    {
                        escaped = true;
                    }

                    if (bracketCharacterStack.Count == 0)
                    {
                        _context.CurrentBracketModeParseParameters.IsBracketEscapeMode = false;
                    }

                    if (!escaped)
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    bool done = false;
                    switch (ch)
                    {
                    case Space:
                        if (_state == StringState.Type)
                        {
                            done = true;  // we are done.
                            break;
                        }
                        sb.Append(ch);
                        break;

                    case OpenCurlie:
                        braceCount++;
                        sb.Append(ch);
                        break;
                    case CloseCurlie:
                        if (braceCount == 0)
                        {
                            done = true;
                        }
                        else
                        {
                            braceCount--;
                            sb.Append(ch);
                        }
                        break;
                    case Comma:
                        done = true;  // we are done.
                        break;

                    case EqualSign:
                        _state = StringState.Property;
                        done = true;  // we are done.
                        break;

                    case Backslash:
                        escaped = true;
                        break;

                    case Quote1:
                    case Quote2:
                        if (!atStart)
                        {
                            throw new XamlParseException(this, SR.Get(SRID.QuoteCharactersOutOfPlace));
                        }
                        quoteChar = ch;
                        wasQuoted = true;
                        break;

                    default:  // All other character (including whitespace)
                        if (_currentSpecialBracketCharacters != null && _currentSpecialBracketCharacters.StartsEscapeSequence(ch))
                        {
                            Stack<char> bracketCharacterStack =
                                _context.CurrentBracketModeParseParameters.BracketCharacterStack;
                            bracketCharacterStack.Clear();
                            bracketCharacterStack.Push(ch);
                            _context.CurrentBracketModeParseParameters.IsBracketEscapeMode = true;
                        }

                        sb.Append(ch);
                        break;
                    }

                    if (done)
                    {
                        if (braceCount > 0)
                        {
                            throw new XamlParseException(this, SR.Get(SRID.UnexpectedTokenAfterME));
                        }
                        else
                        {
                            if (_context.CurrentBracketModeParseParameters?.BracketCharacterStack.Count > 0)
                            {
                                throw new XamlParseException(this, SR.Get(SRID.MalformedBracketCharacters, ch.ToString()));
                            }
                        }

                        PushBack();
                        break;  // we are done.
                    }
                }
                atStart = false;
                Advance();
            }

            if (quoteChar != NullChar)
            {
                throw new XamlParseException(this, SR.Get(SRID.UnclosedQuote));
            }

            string result = sb.ToString();
            if (!wasQuoted)
            {
                result = result.TrimEnd(KnownStrings.WhitespaceChars);
                result = result.TrimStart(KnownStrings.WhitespaceChars);
            }

            if (_state == StringState.Property)
            {
                _currentParameterName = result;
                _currentSpecialBracketCharacters = GetBracketCharacterForProperty(_currentParameterName);
            }

            return result;
        }

        private char CurrentChar
        {
            get { return _inputText[_idx]; }
        }

        private char NextChar
        {
            get
            {
                if (_idx + 1 < _inputText.Length)
                {
                    return _inputText[_idx + 1];
                }
                return NullChar;
            }
        }

        private bool Advance()
        {
            ++_idx;
            if (IsAtEndOfInput)
            {
                _idx = _inputText.Length;
                return false;
            }
            return true;
        }

        private static bool IsWhitespaceChar(char ch)
        {
            Debug.Assert(KnownStrings.WhitespaceChars.Length == 5);

            if (ch == KnownStrings.WhitespaceChars[0] ||
                ch == KnownStrings.WhitespaceChars[1] ||
                ch == KnownStrings.WhitespaceChars[2] || 
                ch == KnownStrings.WhitespaceChars[3] ||
                ch == KnownStrings.WhitespaceChars[4])
            {
                return true;
            }
            return false;
        }

        private void AdvanceOverWhitespace()
        {
            bool sawWhitespace = false;

            while (!IsAtEndOfInput && IsWhitespaceChar(CurrentChar))
            {
                sawWhitespace = true;
                Advance();
            }

            // WFP 3.0 errors on trailing whitespace.
            // [note: very first compat workaround in the new XAML parser]
            // Noticing trailing whitespace is not very natural in this parser.
            // so this extra code is here to implement this error.
            if (IsAtEndOfInput && sawWhitespace)
            {
                _hasTrailingWhitespace = true;
            }
        }

        private void PushBack()
        {
            _idx -= 1;
        }

        private SpecialBracketCharacters GetBracketCharacterForProperty(string propertyName)
        {
            SpecialBracketCharacters bracketCharacters = null;
            if (_context.CurrentEscapeCharacterMapForMarkupExtension != null && 
                _context.CurrentEscapeCharacterMapForMarkupExtension.ContainsKey(propertyName))
            {
                bracketCharacters = _context.CurrentEscapeCharacterMapForMarkupExtension[propertyName];
            }

            return bracketCharacters;
        }
    }

    internal class BracketModeParseParameters
    {
        internal BracketModeParseParameters(XamlParserContext context)
        {
            CurrentConstructorParam = 0;
            IsBracketEscapeMode = false;
            BracketCharacterStack = new Stack<char>();
            if (context.CurrentLongestConstructorOfMarkupExtension != null)
            {
                IsConstructorParsingMode = context.CurrentLongestConstructorOfMarkupExtension.Length > 0;
                MaxConstructorParams = context.CurrentLongestConstructorOfMarkupExtension.Length;
            }
            else
            {
                IsConstructorParsingMode = false;
                MaxConstructorParams = 0;
            }
        }

        internal int CurrentConstructorParam { get; set; }
        internal int MaxConstructorParams { get; set; }
        internal bool IsConstructorParsingMode { get; set; }
        internal bool IsBracketEscapeMode { get; set; }
        internal Stack<char> BracketCharacterStack { get; set; }
    }
}
