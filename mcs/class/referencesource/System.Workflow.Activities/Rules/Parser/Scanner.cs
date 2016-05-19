// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Workflow.ComponentModel;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    #region IntellisenseKeyword
    internal class IntellisenseKeyword
    {
        private string name;

        internal IntellisenseKeyword(string name)
        {
            this.name = name;
        }

        internal string Name
        {
            get { return name; }
        }
    }
    #endregion

    internal class Scanner
    {
        #region Keywords

        private class KeywordInfo
        {
            internal TokenID Token;
            internal object TokenValue;

            internal KeywordInfo(TokenID token, object tokenValue)
            {
                this.Token = token;
                this.TokenValue = tokenValue;
            }

            internal KeywordInfo(TokenID token)
                : this(token, null)
            { }
        }

        private static Dictionary<string, KeywordInfo> keywordMap = CreateKeywordMap();

        private static Dictionary<string, KeywordInfo> CreateKeywordMap()
        {
            Dictionary<string, KeywordInfo> map = new Dictionary<string, KeywordInfo>(27);
            map.Add("mod", new KeywordInfo(TokenID.Modulus));
            map.Add("and", new KeywordInfo(TokenID.And));
            map.Add("or", new KeywordInfo(TokenID.Or));
            map.Add("not", new KeywordInfo(TokenID.Not));
            map.Add("true", new KeywordInfo(TokenID.True, true));
            map.Add("false", new KeywordInfo(TokenID.False, false));
            map.Add("null", new KeywordInfo(TokenID.Null, null));
            map.Add("nothing", new KeywordInfo(TokenID.Null, null));
            map.Add("this", new KeywordInfo(TokenID.This));
            map.Add("me", new KeywordInfo(TokenID.This));
            map.Add("in", new KeywordInfo(TokenID.In));
            map.Add("out", new KeywordInfo(TokenID.Out));
            map.Add("ref", new KeywordInfo(TokenID.Ref));
            map.Add("halt", new KeywordInfo(TokenID.Halt));
            map.Add("update", new KeywordInfo(TokenID.Update));
            map.Add("new", new KeywordInfo(TokenID.New));
            map.Add("char", new KeywordInfo(TokenID.TypeName, typeof(char)));
            map.Add("byte", new KeywordInfo(TokenID.TypeName, typeof(byte)));
            map.Add("sbyte", new KeywordInfo(TokenID.TypeName, typeof(sbyte)));
            map.Add("short", new KeywordInfo(TokenID.TypeName, typeof(short)));
            map.Add("ushort", new KeywordInfo(TokenID.TypeName, typeof(ushort)));
            map.Add("int", new KeywordInfo(TokenID.TypeName, typeof(int)));
            map.Add("uint", new KeywordInfo(TokenID.TypeName, typeof(uint)));
            map.Add("long", new KeywordInfo(TokenID.TypeName, typeof(long)));
            map.Add("ulong", new KeywordInfo(TokenID.TypeName, typeof(ulong)));
            map.Add("float", new KeywordInfo(TokenID.TypeName, typeof(float)));
            map.Add("double", new KeywordInfo(TokenID.TypeName, typeof(double)));
            map.Add("decimal", new KeywordInfo(TokenID.TypeName, typeof(decimal)));
            map.Add("bool", new KeywordInfo(TokenID.TypeName, typeof(bool)));
            map.Add("string", new KeywordInfo(TokenID.TypeName, typeof(string)));
            map.Add("object", new KeywordInfo(TokenID.TypeName, typeof(object)));
            return map;
        }

        internal static void AddKeywordsStartingWith(char upperFirstCharacter, ArrayList list)
        {
            foreach (KeyValuePair<string, KeywordInfo> kvp in keywordMap)
            {
                if (char.ToUpper(kvp.Key[0], CultureInfo.InvariantCulture) == upperFirstCharacter)
                    list.Add(new IntellisenseKeyword(kvp.Key));
            }
        }

        #endregion

        #region Number scanning
        [Flags]
        private enum NumberKind
        {
            UnsuffixedInteger = 0x01,
            Long = 0x02,
            Unsigned = 0x04,
            Double = 0x08,
            Float = 0xc,
            Decimal = 0x10
        }
        #endregion

        private string inputString;
        private int inputStringLength;
        private object tokenValue;
        private TokenID currentToken = TokenID.Unknown;
        private int currentPosition;
        private int tokenStartPosition;

        internal Scanner(string inputString)
        {
            this.inputString = inputString;
            this.inputStringLength = inputString.Length;
        }

        internal void Tokenize(List<Token> tokenList)
        {
            Token token = null;
            do
            {
                token = NextToken();
                tokenList.Add(token);
            } while (token.TokenID != TokenID.EndOfInput);
        }

        internal void TokenizeForIntellisense(List<Token> tokenList)
        {
            Token token = null;
            do
            {
                try
                {
                    token = NextToken();
                    tokenList.Add(token);
                }
                catch (RuleSyntaxException)
                {
                    // Instead of the invalid token, insert a "placeholder" illegal
                    // token.  This will prevent accidentally legal expressions.
                    token = new Token(TokenID.Illegal, 0, null);
                    tokenList.Add(token);
                }
            } while (token != null && token.TokenID != TokenID.EndOfInput);
        }

        private char NextChar()
        {
            if (currentPosition == inputStringLength - 1)
            {
                ++currentPosition; // Point one past the last character, equal to the length
                return '\0';
            }

            ++currentPosition;
            return CurrentChar();
        }

        private char CurrentChar()
        {
            return (currentPosition < inputStringLength) ? inputString[currentPosition] : '\0';
        }

        private char PeekNextChar()
        {
            if (currentPosition == inputStringLength - 1)
                return '\0';

            int peekPosition = currentPosition + 1;
            return (peekPosition < inputStringLength) ? inputString[peekPosition] : '\0';
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private Token NextToken()
        {
            string message = null;  // for any error messages.

            TokenID tokenID = TokenID.Unknown;

            char ch = CurrentChar();
            ch = SkipWhitespace(ch);

            if (ch == '\0')
                return new Token(TokenID.EndOfInput, currentPosition, null);

            tokenStartPosition = currentPosition;
            tokenValue = null;

            if (char.IsDigit(ch))
            {
                tokenID = ScanNumber();
            }
            else if (char.IsLetter(ch))
            {
                tokenID = ScanKeywordOrIdentifier();
            }
            else
            {
                switch (ch)
                {
                    case '_':
                        tokenID = ScanKeywordOrIdentifier();
                        break;
                    case '+':
                        tokenID = TokenID.Plus;
                        NextChar();
                        break;
                    case '-':
                        tokenID = TokenID.Minus;
                        NextChar();
                        break;
                    case '*':
                        tokenID = TokenID.Multiply;
                        NextChar();
                        break;
                    case '/':
                        tokenID = TokenID.Divide;
                        NextChar();
                        break;
                    case '%':
                        tokenID = TokenID.Modulus;
                        NextChar();
                        break;
                    case '&':
                        tokenID = TokenID.BitAnd;
                        if (NextChar() == '&')
                        {
                            NextChar();
                            tokenID = TokenID.And;
                        }
                        break;
                    case '|':
                        tokenID = TokenID.BitOr;
                        if (NextChar() == '|')
                        {
                            NextChar();
                            tokenID = TokenID.Or;
                        }
                        break;
                    case '=':
                        tokenID = TokenID.Assign;
                        if (NextChar() == '=')
                        {
                            // It's "==", so the token is Equal
                            NextChar();
                            tokenID = TokenID.Equal;
                        }
                        break;
                    case '!':
                        tokenID = TokenID.Not;
                        if (NextChar() == '=')
                        {
                            NextChar();
                            tokenID = TokenID.NotEqual;
                        }
                        break;
                    case '<':
                        tokenID = TokenID.Less;
                        ch = NextChar();
                        if (ch == '=')
                        {
                            NextChar();
                            tokenID = TokenID.LessEqual;
                        }
                        else if (ch == '>')
                        {
                            NextChar();
                            tokenID = TokenID.NotEqual;
                        }
                        break;
                    case '>':
                        tokenID = TokenID.Greater;
                        if (NextChar() == '=')
                        {
                            NextChar();
                            tokenID = TokenID.GreaterEqual;
                        }
                        break;
                    case '(':
                        tokenID = TokenID.LParen;
                        NextChar();
                        break;
                    case ')':
                        tokenID = TokenID.RParen;
                        NextChar();
                        break;
                    case '.':
                        tokenID = TokenID.Dot;
                        if (char.IsDigit(PeekNextChar()))
                            tokenID = ScanDecimal();
                        else
                            NextChar(); // consume the '.'
                        break;
                    case ',':
                        tokenID = TokenID.Comma;
                        NextChar();
                        break;
                    case ';':
                        tokenID = TokenID.Semicolon;
                        NextChar();
                        break;
                    case '[':
                        tokenID = TokenID.LBracket;
                        NextChar();
                        break;
                    case ']':
                        tokenID = TokenID.RBracket;
                        NextChar();
                        break;
                    case '{':
                        tokenID = TokenID.LCurlyBrace;
                        NextChar();
                        break;
                    case '}':
                        tokenID = TokenID.RCurlyBrace;
                        NextChar();
                        break;
                    case '@':
                        ch = NextChar();
                        if (ch == '"')
                        {
                            tokenID = ScanVerbatimStringLiteral();
                        }
                        else
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidCharacter, ch);
                            throw new RuleSyntaxException(ErrorNumbers.Error_InvalidCharacter, message, tokenStartPosition);
                        }
                        NextChar();
                        break;
                    case '"':
                        tokenID = ScanStringLiteral();
                        NextChar();
                        break;
                    case '\'':
                        tokenID = ScanCharacterLiteral();
                        NextChar();
                        break;
                    default:
                        NextChar();
                        message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidCharacter, ch);
                        throw new RuleSyntaxException(ErrorNumbers.Error_InvalidCharacter, message, tokenStartPosition);
                }
            }

            Token token = new Token(tokenID, tokenStartPosition, tokenValue);
            currentToken = tokenID;
            return token;
        }

        // Scan a string that starts with '"' and may contain escaped characters
        private TokenID ScanStringLiteral()
        {
            // The current character is the initiating '"'

            StringBuilder sb = new StringBuilder();

            bool isEscaped = false;
            char ch = ScanCharacter(out isEscaped);
            for (;;)
            {
                if (ch == '\0' && !isEscaped)
                    throw new RuleSyntaxException(ErrorNumbers.Error_UnterminatedStringLiteral, Messages.Parser_UnterminatedStringLiteral, tokenStartPosition);

                if (ch == '"' && !isEscaped)
                    break;
                sb.Append(ch);
                ch = ScanCharacter(out isEscaped);
            }

            tokenValue = sb.ToString();

            return TokenID.StringLiteral;
        }

        // Scan a string that starts with '@' and contains no escaped characters
        private TokenID ScanVerbatimStringLiteral()
        {
            // We've already eaten the initiating '@', and the current character is '"'

            StringBuilder sb = new StringBuilder();

            char ch = NextChar(); // eat the opening '"'
            for (;;)
            {
                if (ch == '\0')
                    throw new RuleSyntaxException(ErrorNumbers.Error_UnterminatedStringLiteral, Messages.Parser_UnterminatedStringLiteral, tokenStartPosition);

                if (ch == '"')
                {
                    if (PeekNextChar() == '"')
                    {
                        // It's a doubled-double-quote:  ""
                        NextChar(); // consume the first '"'
                        sb.Append('"');
                    }
                    else
                    {
                        // It's the end of the string as we know it.  (... and I feel fine.)
                        break;
                    }
                }
                else
                {
                    sb.Append(ch);
                }

                ch = NextChar();
            }

            tokenValue = sb.ToString();
            return TokenID.StringLiteral;
        }

        private char ScanCharacter(out bool isEscaped)
        {
            isEscaped = false;

            char ch = NextChar();
            if (ch == '\\')
            {
                // It's an escape code
                isEscaped = true;
                ch = NextChar();
                switch (ch)
                {
                    case '\\':
                    case '\'':
                    case '"':
                        break;

                    case '0':
                        ch = '\0';
                        break;

                    case 'n':
                        ch = '\n';
                        break;

                    case 'r':
                        ch = '\r';
                        break;

                    case 'b':
                        ch = '\b';
                        break;

                    case 'a':
                        ch = '\a';
                        break;

                    case 't':
                        ch = '\t';
                        break;

                    case 'f':
                        ch = '\f';
                        break;

                    case 'v':
                        ch = '\v';
                        break;

                    case 'u':
                        ch = ScanUnicodeEscapeSequence();
                        break;

                    default:
                        string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidEscapeSequence, ch);
                        throw new RuleSyntaxException(ErrorNumbers.Error_InvalidEscapeSequence, message, currentPosition - 1);
                }
            }

            return ch;
        }

        private char ScanUnicodeEscapeSequence()
        {
            char ch;

            // Scan 4 hex digits.
            uint value = 0;
            for (int i = 0; i < 4; ++i)
            {
                ch = NextChar();

                int hDigit = HexValue(ch);
                value = (16 * value) + (uint)hDigit;
            }

            return (char)value;
        }

        private TokenID ScanCharacterLiteral()
        {
            // The current character is the initiating '
            bool isEscaped = false;
            char ch = ScanCharacter(out isEscaped);
            tokenValue = ch;

            if (NextChar() != '\'')
                throw new RuleSyntaxException(ErrorNumbers.Error_UnterminatedCharacterLiteral, Messages.Parser_UnterminatedCharacterLiteral, currentPosition);

            return TokenID.CharacterLiteral;
        }

        private TokenID ScanNumber()
        {
            char ch = CurrentChar();
            if (ch == '0')
            {
                ch = PeekNextChar();
                if (ch == 'x')
                {
                    NextChar(); // Eat the '0'
                    NextChar(); // eat the 'x'
                    return ScanHexNumber();
                }
            }

            // We get here if it wasn't a hex number.  Try
            // scanning again as a decimal number.
            return ScanDecimal();
        }

        private TokenID ScanDecimal()
        {
            NumberKind numberKind = NumberKind.UnsuffixedInteger; // Start by assuming it's an "int" constant.

            StringBuilder buffer = new StringBuilder();

            char ch = CurrentChar();
            while (char.IsDigit(ch))
            {
                buffer.Append(ch);
                ch = NextChar();
            }

            switch (ch)
            {
                case '.':
                    numberKind = NumberKind.Double; // It's a double or float.
                    buffer.Append('.');
                    NextChar(); // eat the '.'
                    numberKind = ScanFraction(buffer);
                    break;

                case 'e':
                case 'E':
                    buffer.Append('e');
                    NextChar(); // eat the 'e'
                    numberKind = ScanExponent(buffer);
                    break;

                case 'f':
                case 'F':
                    numberKind = NumberKind.Float;
                    NextChar(); // eat the 'f'
                    break;

                case 'd':
                case 'D':
                    numberKind = NumberKind.Double;
                    NextChar(); // eat the 'd'
                    break;

                case 'm':
                case 'M':
                    numberKind = NumberKind.Decimal;
                    NextChar(); // eat the 'm'
                    break;

                default:
                    numberKind = ScanOptionalIntegerSuffix();
                    break;
            }

            string message;
            TokenID token;
            string numberString = buffer.ToString();
            if (numberKind == NumberKind.Float)
            {
                token = TokenID.FloatLiteral;
                try
                {
                    tokenValue = float.Parse(numberString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidFloatingPointConstant, exception.Message);
                    throw new RuleSyntaxException(ErrorNumbers.Error_InvalidRealLiteral, message, tokenStartPosition);
                }
            }
            else if (numberKind == NumberKind.Double)
            {
                token = TokenID.FloatLiteral;
                try
                {
                    tokenValue = double.Parse(numberString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidFloatingPointConstant, exception.Message);
                    throw new RuleSyntaxException(ErrorNumbers.Error_InvalidRealLiteral, message, tokenStartPosition);
                }
            }
            else if (numberKind == NumberKind.Decimal)
            {
                token = TokenID.DecimalLiteral;
                try
                {
                    tokenValue = decimal.Parse(numberString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidDecimalConstant, exception.Message);
                    throw new RuleSyntaxException(ErrorNumbers.Error_InvalidRealLiteral, message, tokenStartPosition);
                }
            }
            else
            {
                token = TokenID.IntegerLiteral;

                ulong value = 0;
                try
                {
                    value = ulong.Parse(numberString, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidIntegerConstant, exception.Message);
                    throw new RuleSyntaxException(ErrorNumbers.Error_InvalidIntegerLiteral, message, tokenStartPosition);
                }


                switch (numberKind)
                {
                    case NumberKind.UnsuffixedInteger:
                        // It's an "int" if it fits, else it's a "long" if it fits, else it's a "ulong".
                        if (value > long.MaxValue) // too big for long, keep it ulong
                            tokenValue = value;
                        else if (value <= int.MaxValue) // fits into an int
                            tokenValue = (int)value;
                        else
                            tokenValue = (long)value; // it's a long
                        break;

                    case NumberKind.Long:
                        tokenValue = (long)value;
                        break;

                    case NumberKind.Unsigned:
                        // It's a "uint" if it fits, else its a "ulong"
                        if (value <= uint.MaxValue)
                            tokenValue = (uint)value;
                        else
                            tokenValue = value;
                        break;

                    case NumberKind.Unsigned | NumberKind.Long:
                        tokenValue = value;
                        break;
                }
            }

            return token;
        }

        private NumberKind ScanFraction(StringBuilder buffer)
        {
            char ch = CurrentChar();
            while (char.IsDigit(ch))
            {
                buffer.Append(ch);
                ch = NextChar();
            }

            NumberKind numberKind = NumberKind.Double;
            switch (ch)
            {
                case 'e':
                case 'E':
                    buffer.Append('E');
                    NextChar();
                    numberKind = ScanExponent(buffer);
                    break;

                case 'd':
                case 'D':
                    numberKind = NumberKind.Double;
                    NextChar();
                    break;

                case 'f':
                case 'F':
                    numberKind = NumberKind.Float;
                    NextChar();
                    break;

                case 'm':
                case 'M':
                    numberKind = NumberKind.Decimal;
                    NextChar();
                    break;
            }

            return numberKind;
        }

        private NumberKind ScanExponent(StringBuilder buffer)
        {
            char ch = CurrentChar();

            if (ch == '-' || ch == '+')
            {
                buffer.Append(ch);
                ch = NextChar();
            }

            if (!char.IsDigit(ch))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidExponentDigit, ch);
                throw new RuleSyntaxException(ErrorNumbers.Error_InvalidExponentDigit, message, currentPosition);
            }

            do
            {
                buffer.Append(ch);
                ch = NextChar();
            } while (char.IsDigit(ch));

            NumberKind numberKind = NumberKind.Double;
            switch (ch)
            {
                case 'd':
                case 'D':
                    numberKind = NumberKind.Double;
                    NextChar();
                    break;

                case 'f':
                case 'F':
                    numberKind = NumberKind.Float;
                    NextChar();
                    break;

                case 'm':
                case 'M':
                    numberKind = NumberKind.Decimal;
                    NextChar();
                    break;
            }

            return numberKind;
        }

        private NumberKind ScanOptionalIntegerSuffix()
        {
            NumberKind numberKind = NumberKind.UnsuffixedInteger;

            char ch = CurrentChar();
            switch (ch)
            {
                case 'l':
                case 'L':
                    ch = NextChar(); // eat the 'L'
                    if (ch == 'u' || ch == 'U')
                    {
                        // "LU" is a ulong.
                        numberKind = NumberKind.Long | NumberKind.Unsigned;
                        NextChar(); // eat the 'U'
                    }
                    else
                    {
                        // "L" is a long
                        numberKind = NumberKind.Long;
                    }
                    break;

                case 'u':
                case 'U':
                    ch = NextChar(); // Eat the 'U'
                    if (ch == 'l' || ch == 'L')
                    {
                        // "UL" is a ulong.
                        numberKind = NumberKind.Long | NumberKind.Unsigned;
                        NextChar(); // eat the 'L'
                    }
                    else
                    {
                        numberKind = NumberKind.Unsigned;
                    }
                    break;
            }

            return numberKind;
        }


        private TokenID ScanHexNumber()
        {
            char ch = CurrentChar();
            int hValue = HexValue(ch);
            if (hValue < 0)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidHexDigit, ch);
                throw new RuleSyntaxException(ErrorNumbers.Error_InvalidHexDigit, message, currentPosition);
            }

            int length = 1;
            ulong value = (ulong)hValue;

            ch = NextChar();
            hValue = HexValue(ch);
            while (hValue >= 0)
            {
                ++length;

                value = (value * 16) + (ulong)hValue;

                ch = NextChar();
                hValue = HexValue(ch);
            }

            if (length > sizeof(ulong) * 2)
            {
                // We had overflow.
                string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidIntegerConstant, string.Empty);
                throw new RuleSyntaxException(ErrorNumbers.Error_InvalidIntegerLiteral, message, tokenStartPosition);
            }

            TokenID token = TokenID.IntegerLiteral;

            NumberKind numberKind = ScanOptionalIntegerSuffix();
            switch (numberKind)
            {
                case NumberKind.UnsuffixedInteger:
                    // It's an "int" if it fits, else it's a "long" if it fits, else it's a "ulong".
                    if (value > long.MaxValue) // too big for long, keep it ulong
                        tokenValue = value;
                    else if (value <= int.MaxValue) // fits into an int
                        tokenValue = (int)value;
                    else
                        tokenValue = (long)value; // it's a long
                    break;

                case NumberKind.Long:
                    tokenValue = (long)value;
                    break;

                case NumberKind.Unsigned:
                    // It's a "uint" if it fits, else its a "ulong"
                    if (value <= uint.MaxValue)
                        tokenValue = (uint)value;
                    else
                        tokenValue = value;
                    break;

                case NumberKind.Unsigned | NumberKind.Long:
                    tokenValue = value;
                    break;
            }

            return token;
        }

        private static int HexValue(char ch)
        {
            int value = -1;

            if (char.IsDigit(ch))
            {
                value = (int)ch - (int)'0';
            }
            else
            {
                if (ch >= 'a' && ch <= 'f')
                    value = (int)ch - (int)'a' + 10;
                else if (ch >= 'A' && ch <= 'F')
                    value = (int)ch - (int)'A' + 10;
            }

            return value;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private TokenID ScanKeywordOrIdentifier()
        {
            StringBuilder sb = new StringBuilder();

            bool hasLettersOnly;
            ScanIdentifier(sb, out hasLettersOnly);

            string strValue = sb.ToString();

            TokenID token = TokenID.Unknown;

            if (hasLettersOnly && currentToken != TokenID.Dot)
            {
                // It might be a keyword.
                KeywordInfo keyword = null;
                if (keywordMap.TryGetValue(strValue.ToLowerInvariant(), out keyword))
                {
                    token = keyword.Token;
                    tokenValue = keyword.TokenValue;
                    return token;
                }
            }

            // Otherwise, it's an identifier
            token = TokenID.Identifier;
            tokenValue = strValue;

            return token;
        }

        private void ScanIdentifier(StringBuilder sb, out bool hasLettersOnly)
        {
            char ch = CurrentChar();
            hasLettersOnly = char.IsLetter(ch);

            sb.Append(ch);

            for (ch = NextChar(); ch != '\0'; ch = NextChar())
            {
                bool isValid = false;
                if (char.IsLetter(ch))
                {
                    isValid = true;
                }
                else if (char.IsDigit(ch))
                {
                    isValid = true;
                    hasLettersOnly = false;
                }
                else if (ch == '_')
                {
                    isValid = true;
                    hasLettersOnly = false;
                }

                if (!isValid)
                    break;

                sb.Append(ch);
            }
        }

        private char SkipWhitespace(char ch)
        {
            while (char.IsWhiteSpace(ch))
                ch = NextChar();

            return ch;
        }
    }
}
