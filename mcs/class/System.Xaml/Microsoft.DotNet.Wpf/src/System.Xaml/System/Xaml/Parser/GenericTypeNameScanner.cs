// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using System.Xaml.MS.Impl;

namespace MS.Internal.Xaml.Parser
{
    internal enum GenericTypeNameScannerToken { NONE, ERROR, OPEN, CLOSE, COLON, COMMA, SUBSCRIPT, NAME }

    internal class GenericTypeNameScanner : Sample_StringParserBase
    {
        internal enum State { START, INNAME, INSUBSCRIPT }

        public const char Space = ' ';
        public const char OpenParen = '(';
        public const char CloseParen = ')';
        public const char Comma = ',';
        public const char OpenBracket = '[';
        public const char CloseBracket = ']';
        public const char Colon = ':';

        private GenericTypeNameScannerToken _token;
        private string _tokenText;
        private State _state;
        private GenericTypeNameScannerToken _pushedBackSymbol;
        private int _multiCharTokenStartIdx;
        private int _multiCharTokenLength;
        private char _lastChar;

        public GenericTypeNameScanner(string text)
            :base(text)
        {
            _state = State.START;
            _pushedBackSymbol = GenericTypeNameScannerToken.NONE;
        }

        public GenericTypeNameScannerToken Token { get { return _token; } }

        public string MultiCharTokenText { get { return _tokenText; } }

        public char ErrorCurrentChar { get { return _lastChar; } }

        public void Read()
        {
            if (_pushedBackSymbol != GenericTypeNameScannerToken.NONE)
            {
                _token = _pushedBackSymbol;
                _pushedBackSymbol = GenericTypeNameScannerToken.NONE;
                return;
            }

            _token = GenericTypeNameScannerToken.NONE;
            _tokenText = String.Empty;
            _multiCharTokenStartIdx = -1;
            _multiCharTokenLength = 0;

            while (_token == GenericTypeNameScannerToken.NONE)
            {
                if(IsAtEndOfInput)
                {
                    if (_state == State.INNAME)
                    {
                        _token = GenericTypeNameScannerToken.NAME;
                        _state = State.START;
                    }
                    if (_state == State.INSUBSCRIPT)
                    {
                        _token = GenericTypeNameScannerToken.ERROR;
                        _state = State.START;
                    }
                    break;
                }

                switch (_state)
                {
                    case State.START:
                        State_Start();
                        break;

                    case State.INNAME:
                        State_InName();
                        break;

                    case State.INSUBSCRIPT:
                        State_InSubscript();
                        break;
                }
            }
            if (_token == GenericTypeNameScannerToken.NAME || _token == GenericTypeNameScannerToken.SUBSCRIPT)
            {
                _tokenText = CollectMultiCharToken();
            }
        }

        // Parse a single subscript (e.g. [] or [,]) at the given position, returning its rank
        // Returns 0 if the parse failed
        internal static int ParseSubscriptSegment(string subscript, ref int pos)
        {
            bool openBracketFound = false;
            int rank = 1;
            do
            {
                switch (subscript[pos])
                {
                    case OpenBracket:
                        if (openBracketFound)
                        {
                            return 0;
                        }
                        openBracketFound = true;
                        break;
                    case Comma:
                        if (!openBracketFound)
                        {
                            return 0;
                        }
                        rank++;
                        break;
                    case CloseBracket:
                        if (!openBracketFound)
                        {
                            return 0;
                        }
                        pos++;
                        return rank;
                    default:
                        // Whitespace is allowed inside subscripts
                        if (!IsWhitespaceChar(subscript[pos]))
                        {
                            return 0;
                        }
                        break;
                }
                pos++;
            }
            while (pos < subscript.Length);
            //unterminated string
            return 0; 
        }

        // strips the subscript off the end of typeName, and returns it
        internal static string StripSubscript(string typeName, out string subscript)
        {
            int openBracketNdx = typeName.IndexOf(GenericTypeNameScanner.OpenBracket);
            if (openBracketNdx < 0)
            {
                subscript = null;
                return typeName;
            }
            subscript = typeName.Substring(openBracketNdx);
            return typeName.Substring(0, openBracketNdx);
        }

        private void State_Start()
        {
            AdvanceOverWhitespace();
            if (IsAtEndOfInput)
            {
                _token = GenericTypeNameScannerToken.NONE;
                return;
            }

            switch (CurrentChar)
            {
                case OpenParen:
                    _token = GenericTypeNameScannerToken.OPEN;
                    break;

                case CloseParen:
                    _token = GenericTypeNameScannerToken.CLOSE;
                    break;

                case Comma:
                    _token = GenericTypeNameScannerToken.COMMA;
                    break;

                case Colon:
                    _token = GenericTypeNameScannerToken.COLON;
                    break;

                case OpenBracket:
                    StartMultiCharToken();
                    _state = State.INSUBSCRIPT;
                    // No _token set so continue to scan.
                    break;

                default:
                    if(XamlName.IsValidNameStartChar(CurrentChar))
                    {
                        StartMultiCharToken();
                        _state = State.INNAME;
                        // No _token set so continue to scan.
                    }
                    else
                    {
                        _token = GenericTypeNameScannerToken.ERROR;
                    }
                    break;
            }
            _lastChar = CurrentChar;
            Advance();
        }

        private void State_InName()
        {
            if(IsAtEndOfInput || IsWhitespaceChar(CurrentChar) || CurrentChar == OpenBracket)
            {
                _token = GenericTypeNameScannerToken.NAME;
                _state = State.START;
                return;
            }

            switch(CurrentChar)
            {
                case OpenParen:
                    _pushedBackSymbol = GenericTypeNameScannerToken.OPEN;
                    _token = GenericTypeNameScannerToken.NAME;
                    _state = State.START;
                    break;

                case CloseParen:
                    _pushedBackSymbol = GenericTypeNameScannerToken.CLOSE;
                    _token = GenericTypeNameScannerToken.NAME;
                    _state = State.START;
                    break;

                case Comma:
                    _pushedBackSymbol = GenericTypeNameScannerToken.COMMA;
                    _token = GenericTypeNameScannerToken.NAME;
                    _state = State.START;
                    break;

                case Colon:
                    _pushedBackSymbol = GenericTypeNameScannerToken.COLON;
                    _token = GenericTypeNameScannerToken.NAME;
                    _state = State.START;
                    break;

                default:
                    if (XamlName.IsValidQualifiedNameChar(CurrentChar))
                    {
                        AddToMultiCharToken();
                        // No _token set so continue to scan.
                    }
                    else
                    {
                        _token = GenericTypeNameScannerToken.ERROR;
                    }
                    break;
            }
            _lastChar = CurrentChar;
            Advance();
        }

        private void State_InSubscript()
        {
            if (IsAtEndOfInput)
            {
                _token = GenericTypeNameScannerToken.ERROR;
                _state = State.START;
                return;
            }

            switch (CurrentChar)
            {
                case Comma:
                    AddToMultiCharToken();
                    break;

                case CloseBracket:
                    AddToMultiCharToken();
                    _token = GenericTypeNameScannerToken.SUBSCRIPT;
                    _state = State.START;
                    break;

                default:
                    if (IsWhitespaceChar(CurrentChar))
                    {
                        AddToMultiCharToken();
                    }
                    else
                    {
                        _token = GenericTypeNameScannerToken.ERROR;
                    }
                    break;
            }
            _lastChar = CurrentChar;
            Advance();
        }

        private void StartMultiCharToken()
        {
            Debug.Assert(_multiCharTokenStartIdx == -1 && _multiCharTokenLength == 0);

            _multiCharTokenStartIdx = _idx;
            _multiCharTokenLength = 1;
        }

        private void AddToMultiCharToken()
        {
            Debug.Assert(_multiCharTokenStartIdx != -1 && _multiCharTokenLength > 0);

            _multiCharTokenLength += 1;
        }

        private string CollectMultiCharToken()
        {
            if (_multiCharTokenStartIdx == 0 && _multiCharTokenLength == _inputText.Length)
            {
                return _inputText;
            }
            string result = _inputText.Substring(_multiCharTokenStartIdx, _multiCharTokenLength);
            return result;
        }
    }


    internal class Sample_StringParserBase
    {
        protected const char NullChar = '\0';

        protected String _inputText;
        protected int _idx;

        public Sample_StringParserBase(string text)
        {
            _inputText = text;
            _idx = 0;
        }

        protected char CurrentChar
        {
            get { return _inputText[_idx]; }
        }

        public bool IsAtEndOfInput
        {
            get { return (_idx >= _inputText.Length); }
        }

        protected bool Advance()
        {
            ++_idx;
            if (IsAtEndOfInput)
            {
                _idx = _inputText.Length;
                return false;
            }
            return true;
        }

        protected static bool IsWhitespaceChar(char ch)
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

        protected bool AdvanceOverWhitespace()
        {
            bool sawWhitespace = true;

            while (!IsAtEndOfInput && IsWhitespaceChar(CurrentChar))
            {
                sawWhitespace = true;
                Advance();
            }
            return sawWhitespace;
        }
    }
}
