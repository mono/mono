//---------------------------------------------------------------------
// <copyright file="CqlLexerHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql
{
    using System;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Diagnostics;
    using System.Text;
    using System.Data.Entity;

    /// <summary>
    /// Represents eSQL error context.
    /// </summary>
    internal class ErrorContext
    {
        /// <summary>
        /// Represents the position of the error in the input stream.
        /// </summary>
        internal int InputPosition = -1;

        /// <summary>
        /// Represents the additional/contextual information related to the error position/cause.
        /// </summary>
        internal string ErrorContextInfo;

        /// <summary>
        /// Defines how ErrorContextInfo should be interpreted.
        /// </summary>
        internal bool UseContextInfoAsResourceIdentifier = true;

        /// <summary>
        /// Represents a referece to the original command text.
        /// </summary>
        internal string CommandText;
    }

    /// <summary>
    /// Represents Cql scanner and helper functions.
    /// </summary>
    internal sealed partial class CqlLexer
    {
        static readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;
        static Dictionary<string, short> _keywords;
        static HashSet<string> _invalidAliasNames;
        static HashSet<string> _invalidInlineFunctionNames;
        static Dictionary<string, short> _operators;
        static Dictionary<string, short> _punctuators;
        static HashSet<string> _canonicalFunctionNames;
        static Regex _reDateTimeValue;
        static Regex _reTimeValue;
        static Regex _reDateTimeOffsetValue;
        private const string _datetimeValueRegularExpression = @"^[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}([ ])+[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\.[0-9]{1,7})?)?$";
        private const string _timeValueRegularExpression = @"^[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\.[0-9]{1,7})?)?$";
        private const string _datetimeOffsetValueRegularExpression = @"^[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}([ ])+[0-9]{1,2}:[0-9]{1,2}(:[0-9]{1,2}(\.[0-9]{1,7})?)?([ ])*[\+-][0-9]{1,2}:[0-9]{1,2}$";

        private int _iPos;
        private int _lineNumber;
        ParserOptions _parserOptions;
        private string _query;
        /// <summary>
        /// set for DOT expressions
        /// </summary>
        private bool _symbolAsIdentifierState = false;
        /// <summary>
        /// set for AS expressions
        /// </summary>
        private bool _symbolAsAliasIdentifierState = false;
        /// <summary>
        /// set for function definitions
        /// </summary>
        private bool _symbolAsInlineFunctionNameState = false;

        /// Defines the set of characters to be interpreted as mandatory line breaks
        /// according to UNICODE 5.0, section 5.8 Newline Guidelines.These are 'mandatory' 
        /// line breaks. We do not handle other 'line breaking opportunities'as defined by 
        /// UNICODE 5.0 since they are intended for presentation. The mandatory line break 
        /// defines breaking opportunities that must not be ignored. For all practical purposes
        /// the interpretation of mandatory breaks determines the end of one line and consequently
        /// the start of the next line of query text. 
        /// NOTE that CR and CRLF is treated as a composite 'character' and was obviously and intentionaly 
        /// omitted in the character set bellow.
        static readonly Char[] _newLineCharacters = { '\u000A' , // LF - line feed
                                                      '\u0085' , // NEL - next line
                                                      '\u000B' , // VT - vertical tab
                                                      '\u2028' , // LS - line separator
                                                      '\u2029'   // PS - paragraph separator
                                                    };

        /// <summary>
        /// Intializes scanner
        /// </summary>
        /// <param name="query">input query</param>
        /// <param name="parserOptions">parser options</param>
        internal CqlLexer(string query, ParserOptions parserOptions)
            : this()
        {
            Debug.Assert(query != null, "query must not be null");
            Debug.Assert(parserOptions != null, "parserOptions must not be null");

            _query = query;
            _parserOptions = parserOptions;
            yy_reader = new System.IO.StringReader(_query);
        }

        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name="tokenId">tokenid</param>
        /// <param name="tokenvalue">ast node</param>
        /// <returns></returns>
        static internal Token NewToken(short tokenId, AST.Node tokenvalue)
        {
            return new Token(tokenId, tokenvalue);
        }

        /// <summary>
        /// Creates a new token representing a terminal.
        /// </summary>
        /// <param name="tokenId">tokenid</param>
        /// <param name="termToken">lexical value</param>
        /// <returns></returns>
        static internal Token NewToken(short tokenId, TerminalToken termToken)
        {
            return new Token(tokenId, termToken);
        }

        /// <summary>
        /// Represents a token to be used in parser stack.
        /// </summary>
        internal class Token
        {
            private short _tokenId;
            private object _tokenValue;

            internal Token(short tokenId, AST.Node tokenValue)
            {
                _tokenId = tokenId;
                _tokenValue = tokenValue;
            }

            internal Token(short tokenId, TerminalToken terminal)
            {
                _tokenId = tokenId;
                _tokenValue = terminal;
            }

            internal short TokenId
            {
                get { return _tokenId; }
            }

            internal object Value
            {
                get { return _tokenValue; }
            }
        }

        /// <summary>
        /// Represents a terminal token
        /// </summary>
        internal class TerminalToken
        {
            string _token;
            int _iPos;

            internal TerminalToken(string token, int iPos)
            {
                _token = token;
                _iPos = iPos;
            }

            internal int IPos
            {
                get { return _iPos; }
            }

            internal string Token
            {
                get { return _token; }
            }
        }

        internal static class yy_translate
        {
            internal static char translate(char c)
            #region TRANSLATE
            {
                if (Char.IsWhiteSpace(c) || Char.IsControl(c))
                {
                    if (IsNewLine(c))
                    {
                        return '\n';
                    }
                    return ' ';
                }

                if (c < 0x007F)
                {
                    return c;
                }

                if (Char.IsLetter(c) || Char.IsSymbol(c) || Char.IsNumber(c))
                {
                    return 'a';
                }

                //
                // otherwise pass dummy 'marker' char so as we can continue 'extracting' tokens.
                // 
                return '`';
            }
            #endregion
        }


        /// <summary>
        /// Returns current lexeme
        /// </summary>
        internal string YYText
        {
            get { return yytext(); }
        }

        /// <summary>
        /// Returns current input position
        /// </summary>
        internal int IPos
        {
            get { return _iPos; }
        }

        /// <summary>
        /// Advances input position.
        /// </summary>
        /// <returns>updated input position</returns>
        internal int AdvanceIPos()
        {
            _iPos += YYText.Length;
            return _iPos;
        }

        /// <summary>
        /// returns true if given term is a eSQL keyword
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        internal static bool IsReservedKeyword(string term)
        {
            return CqlLexer.InternalKeywordDictionary.ContainsKey(term);
        }

        /// <summary>
        /// Map lexical symbol to a keyword or an identifier.
        /// </summary>
        /// <param name="symbol">lexeme</param>
        /// <returns>Token</returns>
        internal Token MapIdentifierOrKeyword(string symbol)
        {
            /*
            The purpose of this method is to separate symbols into keywords and identifiers.
            This separation then leads parser into applying different productions 
            to the same eSQL expression. For example if 'key' symbol is mapped to a keyword then
            the expression 'KEY(x)' will satisfy 'keyExpr ::= KEY parenExpr', else if 'key' is mapped
            to an identifier then the expression satisfies
            'methodExpr :: = identifier L_PAREN optAllOrDistinct exprList R_PAREN optWithRelationship'

            Escaped symbols are always assumed to be identifiers.

            For unescaped symbols the naive implementation would check the symbol against 
            the collection of keywords and map the symbol to a keyword in case of match, 
            otherwise map to an identifier.
            This would result in a strong restriction on unescaped identifiers - they must not
            match keywords.

            In the long run this strategy has a potential of invalidating user queries with addition 
            of new keywords to the language. This is an undesired effect and the current implementation
            tries to mitigate it.

            The general mitigation pattern is to separate the collection of keywords and the collection of
            invalid aliases (identifiers), making invalid identifiers a subset of keywords.
            This allows in certain language constructs using unescaped references 'common' identifiers 
            that may be defined in the query or in the model (such as Key in Customer.Key). 
            Although it adds usability for common cases, it does not solve the general problem:
            select c.id as Key from Customers as c -- works
            select Key from (select c.id from Customers as c) as Key -- does not work for the first occurence of Key
                                                                     -- it is mapped to a keyword which results in 
                                                                     -- invalid syntax
            select [Key] from (select c.id from Customers as c) as Key -- works again

            The first two major places in syntax where restrictions are relaxed:
            1. DOT expressions where a symbol before DOT or after DOT is expected to be an identifier.
            2. AS expressions where a symbol after AS is expected to be an identifier.
            In both places identifiers are checked against the invalid aliases collection instead of
            the keywords collection. If an unescaped identifier appears outside of these two places 
            (like the Key in the second query above) it must be escaped or it must not match a keyword.

            The third special case is related to method expressions (function calls). Normally method identifier
            in a method expression must not match a keyword or must be escaped, except the two cases: LEFT and RIGHT.
            LEFT and RIGHT are canonical functions and their usage in a method expression is not ambiguos with 
            LEFT OUTER JOIN and RIGHT OUT JOIN constructs.
            Note that if method identifier is a DOT expression (multipart identifier) such as 'MyNameSpace.Key.Ref(x)' 
            then every part of the identifier follows the relaxed check described for DOT expressions (see above).
            This would help with LEFT and RIGHT functions, 'Edm.Left(x)' would work without the third specialcase,
            but most common use of these function is likely to be without 'Edm.'

            The fourth special case is function names in query inline definition section. These names are checked
            against both
            - the invalid aliases collection and
            - the collection invalid inline function names.
            The second collection contains certain keywords that are not in the first collection and that may be followed
            by the L_PAREN, which makes them look like method expression. The reason for this stronger restriction is to
            disallow the following kind of ambiguos queries:
            Function Key(c Customer) AS (Key(c))
            select Key(cust) from Customsers as cust
            */

            Token token;

            // Handle the escaped identifiers coming from HandleEscapedIdentifiers()
            if (IsEscapedIdentifier(symbol, out token))
            {
                Debug.Assert(token != null, "IsEscapedIdentifier must not return null token");
                return token;
            }

            // Handle keywords
            if (IsKeyword(symbol, out token))
            {
                Debug.Assert(token != null, "IsKeyword must not return null token");
                return token;
            }

            // Handle unescaped identifiers
            return MapUnescapedIdentifier(symbol);
        }

        #region MapIdentifierOrKeyword implementation details
        private bool IsEscapedIdentifier(string symbol, out Token identifierToken)
        {
            if (symbol.Length > 1 && symbol[0] == '[')
            {
                if (symbol[symbol.Length - 1] == ']')
                {
                    string name = symbol.Substring(1, symbol.Length - 2);
                    AST.Identifier id = new AST.Identifier(name, true, _query, _iPos);
                    id.ErrCtx.ErrorContextInfo = EntityRes.CtxEscapedIdentifier;
                    identifierToken = NewToken(CqlParser.ESCAPED_IDENTIFIER, id);
                    return true;
                }
                else
                {
                    throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidEscapedIdentifier(symbol), _iPos);
                }
            }
            else
            {
                identifierToken = null;
                return false;
            }
        }

        private bool IsKeyword(string symbol, out Token terminalToken)
        {
            Char lookAheadChar = GetLookAheadChar();

            if (!IsInSymbolAsIdentifierState(lookAheadChar) &&
                !IsCanonicalFunctionCall(symbol, lookAheadChar) &&
                CqlLexer.InternalKeywordDictionary.ContainsKey(symbol))
            {
                ResetSymbolAsIdentifierState(true);

                short keywordID = CqlLexer.InternalKeywordDictionary[symbol];

                if (keywordID == CqlParser.AS)
                {
                    // Treat the symbol following AS keyword as an identifier.
                    // Note that this state will be turned off by a punctuator, so in case of function definitions:
                    // FUNCTION identifier(...) AS (generalExpr) 
                    // the generalExpr will not be affected by the state.
                    _symbolAsAliasIdentifierState = true;
                }
                else if (keywordID == CqlParser.FUNCTION)
                {
                    // Treat the symbol following FUNCTION keyword as an identifier.
                    // Inline function names in definition section have stronger restrictions than normal identifiers
                    _symbolAsInlineFunctionNameState = true;
                }

                terminalToken = NewToken(keywordID, new TerminalToken(symbol, _iPos));
                return true;
            }
            else
            {
                terminalToken = null;
                return false;
            }
        }

        /// <summary>
        /// Returns true when current symbol looks like a caninical function name in a function call.
        /// Method only treats canonical functions with names ovelapping eSQL keywords. 
        /// This check allows calling these canonical functions without escaping their names.
        /// Check lookAheadChar for a left paren to see if looks like a function call, check symbol against the list of
        /// canonical functions with names overlapping keywords.
        /// </summary>
        private bool IsCanonicalFunctionCall(string symbol, Char lookAheadChar)
        {
            return lookAheadChar == '(' && CqlLexer.InternalCanonicalFunctionNames.Contains(symbol);
        }

        private Token MapUnescapedIdentifier(string symbol)
        {
            // Validate before calling ResetSymbolAsIdentifierState(...) because it will reset _symbolAsInlineFunctionNameState
            bool invalidIdentifier = CqlLexer.InternalInvalidAliasNames.Contains(symbol);
            if (_symbolAsInlineFunctionNameState)
            {
                invalidIdentifier |= CqlLexer.InternalInvalidInlineFunctionNames.Contains(symbol);
            }

            ResetSymbolAsIdentifierState(true);

            if (invalidIdentifier)
            {
                throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidAliasName(symbol), _iPos);
            }
            else
            {
                AST.Identifier id = new AST.Identifier(symbol, false, _query, _iPos);
                id.ErrCtx.ErrorContextInfo = EntityRes.CtxIdentifier;
                return NewToken(CqlParser.IDENTIFIER, id);
            }
        }

        /// <summary>
        /// Skip insignificant whitespace to reach the first potentially significant char.
        /// </summary>
        private Char GetLookAheadChar()
        {
            yy_mark_end();
            Char lookAheadChar = yy_advance();
            while (lookAheadChar != YY_EOF && (Char.IsWhiteSpace(lookAheadChar) || IsNewLine(lookAheadChar)))
            {
                lookAheadChar = yy_advance();
            }
            yy_to_mark();
            return lookAheadChar;
        }

        private bool IsInSymbolAsIdentifierState(char lookAheadChar)
        {
            return _symbolAsIdentifierState ||
                   _symbolAsAliasIdentifierState ||
                   _symbolAsInlineFunctionNameState ||
                   lookAheadChar == '.' /*treat symbols followed by DOT as identifiers*/;
        }

        /// <summary>
        /// Resets "symbol as identifier" state.
        /// </summary>
        /// <param name="significant">see function callers for more info</param>
        private void ResetSymbolAsIdentifierState(bool significant)
        {
            _symbolAsIdentifierState = false;

            // Do not reset the following states if going over {NONNEWLINE_SPACE} or {NEWLINE} or {LINE_COMMENT}
            if (significant)
            {
                _symbolAsAliasIdentifierState = false;
                _symbolAsInlineFunctionNameState = false;
            }
        }
        #endregion

        /// <summary>
        /// Maps operator to respective token
        /// </summary>
        /// <param name="oper">operator lexeme</param>
        /// <returns>Token</returns>
        internal Token MapOperator(string oper)
        {
            if (InternalOperatorDictionary.ContainsKey(oper))
            {
                return NewToken(InternalOperatorDictionary[oper], new TerminalToken(oper, _iPos));
            }
            else
            {
                throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidOperatorSymbol, _iPos);
            }
        }

        /// <summary>
        /// Maps punctuator to respective token
        /// </summary>
        /// <param name="punct">punctuator</param>
        /// <returns>Token</returns>
        internal Token MapPunctuator(string punct)
        {
            if (InternalPunctuatorDictionary.ContainsKey(punct))
            {
                ResetSymbolAsIdentifierState(true);

                if (punct.Equals(".", StringComparison.OrdinalIgnoreCase))
                {
                    _symbolAsIdentifierState = true;
                }

                return NewToken(InternalPunctuatorDictionary[punct], new TerminalToken(punct, _iPos));
            }
            else
            {
                throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidPunctuatorSymbol, _iPos);
            }
        }

        /// <summary>
        /// Maps double quoted string to a literal or an idendifier
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>Token</returns>
        internal Token MapDoubleQuotedString(string symbol)
        {
            // If there is a mode that makes eSQL parser to follow the SQL-92 rules regarding quotation mark
            // delimiting identifiers then this method may decide to map to identifiers.
            // In this case identifiers delimited by double quotation marks can be either eSQL reserved keywords
            // or can contain characters not usually allowed by the eSQL syntax rules for identifiers, 
            // so identifiers mapped here should be treated as escaped identifiers.
            return NewLiteralToken(symbol, AST.LiteralKind.String);
        }

        /// <summary>
        /// Creates literal token
        /// </summary>
        /// <param name="literal">literal</param>
        /// <param name="literalKind">literal kind</param>
        /// <returns>Literal Token</returns>
        internal Token NewLiteralToken(string literal, AST.LiteralKind literalKind)
        {
            Debug.Assert(!String.IsNullOrEmpty(literal), "literal must not be null or empty");
            Debug.Assert(literalKind != AST.LiteralKind.Null, "literalKind must not be LiteralKind.Null");

            string literalValue = literal;
            switch (literalKind)
            {
                case AST.LiteralKind.Binary:
                    literalValue = GetLiteralSingleQuotePayload(literal);
                    if (!IsValidBinaryValue(literalValue))
                    {
                        throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidLiteralFormat("binary", literalValue), _iPos);
                    }
                    break;

                case AST.LiteralKind.String:
                    if ('N' == literal[0])
                    {
                        literalKind = AST.LiteralKind.UnicodeString;
                    }
                    break;

                case AST.LiteralKind.DateTime:
                    literalValue = GetLiteralSingleQuotePayload(literal);
                    if (!IsValidDateTimeValue(literalValue))
                    {
                        throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidLiteralFormat("datetime", literalValue), _iPos);
                    }
                    break;

                case AST.LiteralKind.Time:
                    literalValue = GetLiteralSingleQuotePayload(literal);
                    if (!IsValidTimeValue(literalValue))
                    {
                        throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidLiteralFormat("time", literalValue), _iPos);
                    }
                    break;
                case AST.LiteralKind.DateTimeOffset:
                    literalValue = GetLiteralSingleQuotePayload(literal);
                    if (!IsValidDateTimeOffsetValue(literalValue))
                    {
                        throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidLiteralFormat("datetimeoffset", literalValue), _iPos);
                    }
                    break;

                case AST.LiteralKind.Guid:
                    literalValue = GetLiteralSingleQuotePayload(literal);
                    if (!IsValidGuidValue(literalValue))
                    {
                        throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidLiteralFormat("guid", literalValue), _iPos);
                    }
                    break;
            }

            return NewToken(CqlParser.LITERAL, new AST.Literal(literalValue, literalKind, _query, _iPos));
        }

        /// <summary>
        /// Creates parameter token
        /// </summary>
        /// <param name="param">param</param>
        /// <returns>Parameter Token</returns>
        internal Token NewParameterToken(string param)
        {
            return NewToken(CqlParser.PARAMETER, new AST.QueryParameter(param, _query, _iPos));
        }

        /// <summary>
        /// handles escaped identifiers
        /// ch will always be translated i.e. normalized.
        /// </summary>
        internal Token HandleEscapedIdentifiers()
        {
            char ch = YYText[0];
            while (ch != YY_EOF)
            {
                if (ch == ']')
                {
                    yy_mark_end();
                    ch = yy_advance();
                    if (ch != ']')
                    {
                        yy_to_mark();
                        ResetSymbolAsIdentifierState(true);
                        return MapIdentifierOrKeyword(YYText.Replace("]]", "]"));
                    }
                }
                ch = yy_advance();
            }
            Debug.Assert(ch == YY_EOF, "ch == YY_EOF");
            throw EntityUtil.EntitySqlError(_query, System.Data.Entity.Strings.InvalidEscapedIdentifierUnbalanced(YYText), _iPos);
        }

        internal static bool IsLetterOrDigitOrUnderscore(string symbol, out bool isIdentifierASCII)
        {
            isIdentifierASCII = true;
            for (int i = 0; i < symbol.Length; i++)
            {
                isIdentifierASCII = isIdentifierASCII && symbol[i] < 0x80;
                if (!isIdentifierASCII && !IsLetter(symbol[i]) && !IsDigit(symbol[i]) && (symbol[i] != '_'))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        private static bool IsDigit(char c)
        {
            return (c >= '0' && c <= '9');
        }

        private static bool isHexDigit(char c)
        {
            return (IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }

        /// <summary>
        /// Returns true if given char is a new line character defined by
        /// UNICODE 5.0, section 5.8 Newline Guidelines.
        /// These are 'mandatory' line breaks. NOTE that CRLF is treated as a 
        /// composite 'character' and was intentionaly omitted in the character set bellow.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static bool IsNewLine(Char c)
        {
            for (int i = 0; i < _newLineCharacters.Length; i++)
            {
                if (c == _newLineCharacters[i])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// extracts single quoted literal 'payload'. literal MUST BE normalized.
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        private static string GetLiteralSingleQuotePayload(string literal)
        {
            Debug.Assert(-1 != literal.IndexOf('\''), "quoted literal value must have single quotes");
            Debug.Assert(-1 != literal.LastIndexOf('\''), "quoted literal value must have single quotes");
            Debug.Assert(literal.IndexOf('\'') != literal.LastIndexOf('\''), "quoted literal value must have 2 single quotes");
            Debug.Assert(literal.Split(new char[] { '\'' }).Length == 3, "quoted literal value must have 2 single quotes");

            // NOTE: this is not a precondition validation. This validation is for security purposes based on the 
            // paranoid assumption that all input is evil. we should not see this exception under normal 
            // conditions.
            if ((literal.Split(new char[] { '\'' }).Length != 3) || (-1 == literal.IndexOf('\'')) || (-1 == literal.LastIndexOf('\'')))
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.MalformedSingleQuotePayload);
            }

            int startIndex = literal.IndexOf('\'');

            string literalPayload = literal.Substring(startIndex + 1, literal.Length - (startIndex + 2));

            Debug.Assert(literalPayload.IndexOf('\'') == -1, "quoted literal payload must not have single quotes");
            Debug.Assert(literalPayload.LastIndexOf('\'') == -1, "quoted literal payload must not have single quotes");

            // NOTE: this is not a precondition validation. This validation is for security purposes based on the 
            // paranoid assumption that all input is evil. we should not see this exception under normal 
            // conditions.
            if (literalPayload.Split(new char[] { '\'' }).Length != 1)
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.MalformedSingleQuotePayload);
            }

            return literalPayload;
        }

        /// <summary>
        /// returns true if guid literal value format is valid
        /// </summary>
        /// <param name="guidValue"></param>
        /// <returns></returns>
        private static bool IsValidGuidValue(string guidValue)
        {
            int startIndex = 0;
            int endIndex = guidValue.Length - 1;
            if ((endIndex - startIndex) + 1 != 36)
            {
                return false;
            }

            int i = 0;
            bool bValid = true;
            while (bValid && i < 36)
            {
                if ((i == 8) || (i == 13) || (i == 18) || (i == 23))
                {
                    bValid = (guidValue[startIndex + i] == '-');
                }
                else
                {
                    bValid = isHexDigit(guidValue[startIndex + i]);
                }
                i++;
            }
            return bValid;
        }

        /// <summary>
        /// returns true if binary literal value format is valid
        /// </summary>
        /// <param name="binaryValue"></param>
        /// <returns></returns>
        private static bool IsValidBinaryValue(string binaryValue)
        {
            Debug.Assert(null != binaryValue, "binaryValue must not be null");

            if (String.IsNullOrEmpty(binaryValue))
            {
                return true;
            }

            int i = 0;
            bool bValid = binaryValue.Length > 0;
            while (bValid && i < binaryValue.Length)
            {
                bValid = isHexDigit(binaryValue[i++]);
            }

            return bValid;
        }

        /// <summary>
        /// Returns true if datetime literal value format is valid
        /// allowed format is: dddd-d?d-d?d{space}+d?d:d?d(:d?d(.d?d?d)?)?
        /// where d is any decimal digit.
        /// </summary>
        /// <param name="datetimeValue"></param>
        /// <returns></returns>
        private static bool IsValidDateTimeValue(string datetimeValue)
        {
            if (null == _reDateTimeValue)
            {
                _reDateTimeValue = new Regex(_datetimeValueRegularExpression, RegexOptions.Singleline | RegexOptions.CultureInvariant);
            }
            return _reDateTimeValue.IsMatch(datetimeValue);
        }

        /// <summary>
        /// Returns true if time literal value format is valid
        /// allowed format is: +d?d:d?d(:d?d(.d?d?d)?)?
        /// where d is any decimal digit.
        /// </summary>
        /// <param name="timeValue"></param>
        /// <returns></returns>
        private static bool IsValidTimeValue(string timeValue)
        {
            if (null == _reTimeValue)
            {
                _reTimeValue = new Regex(_timeValueRegularExpression, RegexOptions.Singleline | RegexOptions.CultureInvariant);
            }
            return _reTimeValue.IsMatch(timeValue);
        }

        /// <summary>
        /// Returns true if datetimeoffset literal value format is valid
        /// allowed format is: dddd-d?d-d?d{space}+d?d:d?d(:d?d(.d?d?d)?)?([+-]d?d:d?d)?
        /// where d is any decimal digit.
        /// </summary>
        /// <param name="datetimeOffsetValue"></param>
        /// <returns></returns>
        private static bool IsValidDateTimeOffsetValue(string datetimeOffsetValue)
        {
            if (null == _reDateTimeOffsetValue)
            {
                _reDateTimeOffsetValue = new Regex(_datetimeOffsetValueRegularExpression, RegexOptions.Singleline | RegexOptions.CultureInvariant);
            }
            return _reDateTimeOffsetValue.IsMatch(datetimeOffsetValue);
        }

        private static Dictionary<string, short> InternalKeywordDictionary
        {
            get
            {
                if (null == _keywords)
                {
                    #region Initializes eSQL keywords
                    Dictionary<string, short> keywords = new Dictionary<string, short>(60, _stringComparer);
                    keywords.Add("all", CqlParser.ALL);
                    keywords.Add("and", CqlParser.AND);
                    keywords.Add("anyelement", CqlParser.ANYELEMENT);
                    keywords.Add("apply", CqlParser.APPLY);
                    keywords.Add("as", CqlParser.AS);
                    keywords.Add("asc", CqlParser.ASC);
                    keywords.Add("between", CqlParser.BETWEEN);
                    keywords.Add("by", CqlParser.BY);
                    keywords.Add("case", CqlParser.CASE);
                    keywords.Add("cast", CqlParser.CAST);
                    keywords.Add("collate", CqlParser.COLLATE);
                    keywords.Add("collection", CqlParser.COLLECTION);
                    keywords.Add("createref", CqlParser.CREATEREF);
                    keywords.Add("cross", CqlParser.CROSS);
                    keywords.Add("deref", CqlParser.DEREF);
                    keywords.Add("desc", CqlParser.DESC);
                    keywords.Add("distinct", CqlParser.DISTINCT);
                    keywords.Add("element", CqlParser.ELEMENT);
                    keywords.Add("else", CqlParser.ELSE);
                    keywords.Add("end", CqlParser.END);
                    keywords.Add("escape", CqlParser.ESCAPE);
                    keywords.Add("except", CqlParser.EXCEPT);
                    keywords.Add("exists", CqlParser.EXISTS);
                    keywords.Add("false", CqlParser.LITERAL);
                    keywords.Add("flatten", CqlParser.FLATTEN);
                    keywords.Add("from", CqlParser.FROM);
                    keywords.Add("full", CqlParser.FULL);
                    keywords.Add("function", CqlParser.FUNCTION);
                    keywords.Add("group", CqlParser.GROUP);
                    keywords.Add("grouppartition", CqlParser.GROUPPARTITION);
                    keywords.Add("having", CqlParser.HAVING);
                    keywords.Add("in", CqlParser.IN);
                    keywords.Add("inner", CqlParser.INNER);
                    keywords.Add("intersect", CqlParser.INTERSECT);
                    keywords.Add("is", CqlParser.IS);
                    keywords.Add("join", CqlParser.JOIN);
                    keywords.Add("key", CqlParser.KEY);
                    keywords.Add("left", CqlParser.LEFT);
                    keywords.Add("like", CqlParser.LIKE);
                    keywords.Add("limit", CqlParser.LIMIT);
                    keywords.Add("multiset", CqlParser.MULTISET);
                    keywords.Add("navigate", CqlParser.NAVIGATE);
                    keywords.Add("not", CqlParser.NOT);
                    keywords.Add("null", CqlParser.NULL);
                    keywords.Add("of", CqlParser.OF);
                    keywords.Add("oftype", CqlParser.OFTYPE);
                    keywords.Add("on", CqlParser.ON);
                    keywords.Add("only", CqlParser.ONLY);
                    keywords.Add("or", CqlParser.OR);
                    keywords.Add("order", CqlParser.ORDER);
                    keywords.Add("outer", CqlParser.OUTER);
                    keywords.Add("overlaps", CqlParser.OVERLAPS);
                    keywords.Add("ref", CqlParser.REF);
                    keywords.Add("relationship", CqlParser.RELATIONSHIP);
                    keywords.Add("right", CqlParser.RIGHT);
                    keywords.Add("row", CqlParser.ROW);
                    keywords.Add("select", CqlParser.SELECT);
                    keywords.Add("set", CqlParser.SET);
                    keywords.Add("skip", CqlParser.SKIP);
                    keywords.Add("then", CqlParser.THEN);
                    keywords.Add("top", CqlParser.TOP);
                    keywords.Add("treat", CqlParser.TREAT);
                    keywords.Add("true", CqlParser.LITERAL);
                    keywords.Add("union", CqlParser.UNION);
                    keywords.Add("using", CqlParser.USING);
                    keywords.Add("value", CqlParser.VALUE);
                    keywords.Add("when", CqlParser.WHEN);
                    keywords.Add("where", CqlParser.WHERE);
                    keywords.Add("with", CqlParser.WITH);
                    _keywords = keywords;
                    #endregion
                }
                return _keywords;
            }

        }

        private static HashSet<string> InternalInvalidAliasNames
        {
            get
            {
                if (null == _invalidAliasNames)
                {
                    #region Initializes invalid aliases
                    HashSet<string> invalidAliasName = new HashSet<string>(_stringComparer);
                    invalidAliasName.Add("all");
                    invalidAliasName.Add("and");
                    invalidAliasName.Add("apply");
                    invalidAliasName.Add("as");
                    invalidAliasName.Add("asc");
                    invalidAliasName.Add("between");
                    invalidAliasName.Add("by");
                    invalidAliasName.Add("case");
                    invalidAliasName.Add("cast");
                    invalidAliasName.Add("collate");
                    invalidAliasName.Add("createref");
                    invalidAliasName.Add("deref");
                    invalidAliasName.Add("desc");
                    invalidAliasName.Add("distinct");
                    invalidAliasName.Add("element");
                    invalidAliasName.Add("else");
                    invalidAliasName.Add("end");
                    invalidAliasName.Add("escape");
                    invalidAliasName.Add("except");
                    invalidAliasName.Add("exists");
                    invalidAliasName.Add("flatten");
                    invalidAliasName.Add("from");
                    invalidAliasName.Add("group");
                    invalidAliasName.Add("having");
                    invalidAliasName.Add("in");
                    invalidAliasName.Add("inner");
                    invalidAliasName.Add("intersect");
                    invalidAliasName.Add("is");
                    invalidAliasName.Add("join");
                    invalidAliasName.Add("like");
                    invalidAliasName.Add("multiset");
                    invalidAliasName.Add("navigate");
                    invalidAliasName.Add("not");
                    invalidAliasName.Add("null");
                    invalidAliasName.Add("of");
                    invalidAliasName.Add("oftype");
                    invalidAliasName.Add("on");
                    invalidAliasName.Add("only");
                    invalidAliasName.Add("or");
                    invalidAliasName.Add("overlaps");
                    invalidAliasName.Add("ref");
                    invalidAliasName.Add("relationship");
                    invalidAliasName.Add("select");
                    invalidAliasName.Add("set");
                    invalidAliasName.Add("then");
                    invalidAliasName.Add("treat");
                    invalidAliasName.Add("union");
                    invalidAliasName.Add("using");
                    invalidAliasName.Add("when");
                    invalidAliasName.Add("where");
                    invalidAliasName.Add("with");
                    _invalidAliasNames = invalidAliasName;
                    #endregion
                }
                return _invalidAliasNames;
            }
        }

        private static HashSet<string> InternalInvalidInlineFunctionNames
        {
            get
            {
                if (null == _invalidInlineFunctionNames)
                {
                    #region Initializes invalid inline function names
                    HashSet<string> invalidInlineFunctionNames = new HashSet<string>(_stringComparer);
                    invalidInlineFunctionNames.Add("anyelement");
                    invalidInlineFunctionNames.Add("element");
                    invalidInlineFunctionNames.Add("function");
                    invalidInlineFunctionNames.Add("grouppartition");
                    invalidInlineFunctionNames.Add("key");
                    invalidInlineFunctionNames.Add("ref");
                    invalidInlineFunctionNames.Add("row");
                    invalidInlineFunctionNames.Add("skip");
                    invalidInlineFunctionNames.Add("top");
                    invalidInlineFunctionNames.Add("value");
                    _invalidInlineFunctionNames = invalidInlineFunctionNames;
                    #endregion
                }
                return _invalidInlineFunctionNames;
            }
        }

        private static Dictionary<string, short> InternalOperatorDictionary
        {
            get
            {
                if (null == _operators)
                {
                    #region Initializes operator dictionary
                    Dictionary<string, short> operators = new Dictionary<string, short>(16, _stringComparer);
                    operators.Add("==", CqlParser.OP_EQ);
                    operators.Add("!=", CqlParser.OP_NEQ);
                    operators.Add("<>", CqlParser.OP_NEQ);
                    operators.Add("<", CqlParser.OP_LT);
                    operators.Add("<=", CqlParser.OP_LE);
                    operators.Add(">", CqlParser.OP_GT);
                    operators.Add(">=", CqlParser.OP_GE);
                    operators.Add("&&", CqlParser.AND);
                    operators.Add("||", CqlParser.OR);
                    operators.Add("!", CqlParser.NOT);
                    operators.Add("+", CqlParser.PLUS);
                    operators.Add("-", CqlParser.MINUS);
                    operators.Add("*", CqlParser.STAR);
                    operators.Add("/", CqlParser.FSLASH);
                    operators.Add("%", CqlParser.PERCENT);
                    _operators = operators;
                    #endregion
                }
                return _operators;
            }
        }

        private static Dictionary<string, short> InternalPunctuatorDictionary
        {
            get
            {
                if (null == _punctuators)
                {
                    #region Initializes punctuators dictionary
                    Dictionary<string, short> punctuators = new Dictionary<string, short>(16, _stringComparer);
                    punctuators.Add(",", CqlParser.COMMA);
                    punctuators.Add(":", CqlParser.COLON);
                    punctuators.Add(".", CqlParser.DOT);
                    punctuators.Add("?", CqlParser.QMARK);
                    punctuators.Add("(", CqlParser.L_PAREN);
                    punctuators.Add(")", CqlParser.R_PAREN);
                    punctuators.Add("[", CqlParser.L_BRACE);
                    punctuators.Add("]", CqlParser.R_BRACE);
                    punctuators.Add("{", CqlParser.L_CURLY);
                    punctuators.Add("}", CqlParser.R_CURLY);
                    punctuators.Add(";", CqlParser.SCOLON);
                    punctuators.Add("=", CqlParser.EQUAL);
                    _punctuators = punctuators;
                    #endregion
                }
                return _punctuators;
            }
        }

        private static HashSet<string> InternalCanonicalFunctionNames
        {
            get
            {
                if (null == _canonicalFunctionNames)
                {
                    HashSet<string> canonicalFunctionNames = new HashSet<string>(_stringComparer);
                    canonicalFunctionNames.Add("left");
                    canonicalFunctionNames.Add("right");
                    _canonicalFunctionNames = canonicalFunctionNames;
                }
                return _canonicalFunctionNames;
            }
        }
    }
}
