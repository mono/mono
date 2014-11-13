//------------------------------------------------------------------------------
// <copyright file="XPathScanner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <spec>http://www.w3.org/TR/xpath#exprlex</spec>
//------------------------------------------------------------------------------

using System.Diagnostics;

namespace System.Xml.Xsl.XPath {
    using Res = System.Xml.Utils.Res;

    // Extends XPathOperator enumeration
    internal enum LexKind {
        Unknown,        // Unknown lexeme
        Or,             // Operator 'or'
        And,            // Operator 'and'
        Eq,             // Operator '='
        Ne,             // Operator '!='
        Lt,             // Operator '<'
        Le,             // Operator '<='
        Gt,             // Operator '>'
        Ge,             // Operator '>='
        Plus,           // Operator '+'
        Minus,          // Operator '-'
        Multiply,       // Operator '*'
        Divide,         // Operator 'div'
        Modulo,         // Operator 'mod'
        UnaryMinus,     // Not used
        Union,          // Operator '|'
        LastOperator    = Union,

        DotDot,         // '..'
        ColonColon,     // '::'
        SlashSlash,     // Operator '//'
        Number,         // Number (numeric literal)
        Axis,           // AxisName

        Name,           // NameTest, NodeType, FunctionName, AxisName, second part of VariableReference
        String,         // Literal (string literal)
        Eof,            // End of the expression

        FirstStringable = Name,
        LastNonChar     = Eof,

        LParens     = '(',
        RParens     = ')',
        LBracket    = '[',
        RBracket    = ']',
        Dot         = '.',
        At          = '@',
        Comma       = ',',

        Star        = '*',      // NameTest
        Slash       = '/',      // Operator '/'
        Dollar      = '$',      // First part of VariableReference
        RBrace      = '}',      // Used for AVTs
    };

    internal sealed class XPathScanner {
        private string  xpathExpr;
        private int     curIndex;
        private char    curChar;
        private LexKind kind;
        private string  name;
        private string  prefix;
        private string  stringValue;
        private bool    canBeFunction;
        private int     lexStart;
        private int     prevLexEnd;
        private LexKind prevKind;
        private XPathAxis axis;

        private XmlCharType xmlCharType = XmlCharType.Instance;

        public XPathScanner(string xpathExpr) : this(xpathExpr, 0) {}

        public XPathScanner(string xpathExpr, int startFrom) {
            Debug.Assert(xpathExpr != null);
            this.xpathExpr = xpathExpr;
            this.kind = LexKind.Unknown;
            SetSourceIndex(startFrom);
            NextLex();
        }

        public string   Source      { get { return xpathExpr;   } }
        public LexKind  Kind        { get { return kind;        } }
        public int      LexStart    { get { return lexStart;    } }
        public int      LexSize     { get { return curIndex - lexStart; } }
        public int      PrevLexEnd  { get { return prevLexEnd;  } }

        private void SetSourceIndex(int index) {
            Debug.Assert(0 <= index && index <= xpathExpr.Length);
            curIndex = index - 1;
            NextChar();
        }

        private void NextChar() {
            Debug.Assert(-1 <= curIndex && curIndex < xpathExpr.Length);
            curIndex++;
            if (curIndex < xpathExpr.Length) {
                curChar = xpathExpr[curIndex];
            } else {
                Debug.Assert(curIndex == xpathExpr.Length);
                curChar = '\0';
            }
        }

#if XML10_FIFTH_EDITION
        private char PeekNextChar() {
            Debug.Assert(-1 <= curIndex && curIndex <= xpathExpr.Length);
            if (curIndex + 1 < xpathExpr.Length) {
                return xpathExpr[curIndex + 1];
            }
            else {
                return '\0';
            }
        }
#endif

        public string Name {
            get {
                Debug.Assert(kind == LexKind.Name);
                Debug.Assert(name != null);
                return name;
            }
        }

        public string Prefix {
            get {
                Debug.Assert(kind == LexKind.Name);
                Debug.Assert(prefix != null);
                return prefix;
            }
        }

        public string RawValue {
            get {
                if (kind == LexKind.Eof) {
                    return LexKindToString(kind);
                } else {
                    return xpathExpr.Substring(lexStart, curIndex - lexStart);
                }
            }
        }

        public string StringValue {
            get {
                Debug.Assert(kind == LexKind.String);
                Debug.Assert(stringValue != null);
                return stringValue;
            }
        }

        // Returns true if the character following an QName (possibly after intervening
        // ExprWhitespace) is '('. In this case the token must be recognized as a NodeType
        // or a FunctionName unless it is an OperatorName. This distinction cannot be done
        // without knowing the previous lexeme. For example, "or" in "... or (1 != 0)" may
        // be an OperatorName or a FunctionName.
        public bool CanBeFunction {
            get {
                Debug.Assert(kind == LexKind.Name);
                return canBeFunction;
            }
        }

        public XPathAxis Axis {
            get {
                Debug.Assert(kind == LexKind.Axis);
                Debug.Assert(axis != XPathAxis.Unknown);
                return axis;
            }
        }

        private void SkipSpace() {
            while (xmlCharType.IsWhiteSpace(curChar)) {
                NextChar();
            }
        }

        private static bool IsAsciiDigit(char ch) {
            return (uint)(ch - '0') <= 9;
        }

        public void NextLex() {
            prevLexEnd = curIndex;
            prevKind = kind;
            SkipSpace();
            lexStart = curIndex;

            switch (curChar) {
                case '\0':
                    kind = LexKind.Eof;
                    return;
                case '(': case ')': case '[': case ']':
                case '@': case ',': case '$': case '}':
                    kind = (LexKind)curChar;
                    NextChar();
                    break;
                case '.':
                    NextChar();
                    if (curChar == '.') {
                        kind = LexKind.DotDot;
                        NextChar();
                    } else if (IsAsciiDigit(curChar)) {
                        SetSourceIndex(lexStart);
                        goto case '0';
                    } else {
                        kind = LexKind.Dot;
                    }
                    break;
                case ':':
                    NextChar();
                    if (curChar == ':') {
                        kind = LexKind.ColonColon;
                        NextChar();
                    } else {
                        kind = LexKind.Unknown;
                    }
                    break;
                case '*':
                    kind = LexKind.Star;
                    NextChar();
                    CheckOperator(true);
                    break;
                case '/':
                    NextChar();
                    if (curChar == '/') {
                        kind = LexKind.SlashSlash;
                        NextChar();
                    } else {
                        kind = LexKind.Slash;
                    }
                    break;
                case '|':
                    kind = LexKind.Union;
                    NextChar();
                    break;
                case '+':
                    kind = LexKind.Plus;
                    NextChar();
                    break;
                case '-':
                    kind = LexKind.Minus;
                    NextChar();
                    break;
                case '=':
                    kind = LexKind.Eq;
                    NextChar();
                    break;
                case '!':
                    NextChar();
                    if (curChar == '=') {
                        kind = LexKind.Ne;
                        NextChar();
                    } else {
                        kind = LexKind.Unknown;
                    }
                    break;
                case '<':
                    NextChar();
                    if (curChar == '=') {
                        kind = LexKind.Le;
                        NextChar();
                    } else {
                        kind = LexKind.Lt;
                    }
                    break;
                case '>':
                    NextChar();
                    if (curChar == '=') {
                        kind = LexKind.Ge;
                        NextChar();
                    } else {
                        kind = LexKind.Gt;
                    }
                    break;
                case '"':
                case '\'':
                    kind = LexKind.String;
                    ScanString();
                    break;
                case '0': case '1': case '2': case '3':
                case '4': case '5': case '6': case '7':
                case '8': case '9':
                    kind = LexKind.Number;
                    ScanNumber();
                    break;
                default:
                    if (xmlCharType.IsStartNCNameSingleChar(curChar) 
#if XML10_FIFTH_EDITION
                        || xmlCharType.IsNCNameHighSurrogateChar(curChar)
#endif
                        ) {
                        kind = LexKind.Name;
                        this.name   = ScanNCName();
                        this.prefix = string.Empty;
                        this.canBeFunction = false;
                        this.axis = XPathAxis.Unknown;
                        bool colonColon = false;
                        int saveSourceIndex = curIndex;

                        // "foo:bar" or "foo:*" -- one lexeme (no spaces allowed)
                        // "foo::" or "foo ::"  -- two lexemes, reported as one (AxisName)
                        // "foo:?" or "foo :?"  -- lexeme "foo" reported
                        if (curChar == ':') {
                            NextChar();
                            if (curChar == ':') {   // "foo::" -> OperatorName, AxisName
                                NextChar();
                                colonColon = true;
                                SetSourceIndex(saveSourceIndex);
                            } else {                // "foo:bar", "foo:*" or "foo:?"
                                if (curChar == '*') {
                                    NextChar();
                                    this.prefix = this.name;
                                    this.name = "*";
                                } else if (xmlCharType.IsStartNCNameSingleChar(curChar) 
#if XML10_FIFTH_EDITION
                                    || xmlCharType.IsNCNameHighSurrogateChar(curChar)
#endif
                                    ) {
                                    this.prefix = this.name;
                                    this.name = ScanNCName();
                                    // Look ahead for '(' to determine whether QName can be a FunctionName
                                    saveSourceIndex = curIndex;
                                    SkipSpace();
                                    this.canBeFunction = (curChar == '(');
                                    SetSourceIndex(saveSourceIndex);
                                } else {            // "foo:?" -> OperatorName, NameTest
                                    // Return "foo" and leave ":" to be reported later as an unknown lexeme
                                    SetSourceIndex(saveSourceIndex);
                                }
                            }
                        } else {
                            SkipSpace();
                            if (curChar == ':') {   // "foo ::" or "foo :?"
                                NextChar();
                                if (curChar == ':') {
                                    NextChar();
                                    colonColon = true;
                                }
                                SetSourceIndex(saveSourceIndex);
                            } else {
                                this.canBeFunction = (curChar == '(');
                            }
                        }
                        if (!CheckOperator(false) && colonColon) {
                            this.axis = CheckAxis();
                        }
                    } else {
                        kind = LexKind.Unknown;
                        NextChar();
                    }
                    break;
            }
        }

        private bool CheckOperator(bool star) {
            LexKind opKind;

            if (star) {
                opKind = LexKind.Multiply;
            } else {
                if (prefix.Length != 0 || name.Length > 3)
                    return false;

                switch (name) {
                    case "or" : opKind = LexKind.Or;      break;
                    case "and": opKind = LexKind.And;     break;
                    case "div": opKind = LexKind.Divide;  break;
                    case "mod": opKind = LexKind.Modulo;  break;
                    default   : return false;
                }
            }

            // If there is a preceding token and the preceding token is not one of '@', '::', '(', '[', ',' or an Operator,
            // then a '*' must be recognized as a MultiplyOperator and an NCName must be recognized as an OperatorName.
            if (prevKind <= LexKind.LastOperator)
                return false;

            switch (prevKind) {
                case LexKind.Slash:
                case LexKind.SlashSlash:
                case LexKind.At:
                case LexKind.ColonColon:
                case LexKind.LParens:
                case LexKind.LBracket:
                case LexKind.Comma:
                case LexKind.Dollar:
                    return false;
            }

            this.kind = opKind;
            return true;
        }

        private XPathAxis CheckAxis() {
            this.kind = LexKind.Axis;
            switch (name) {
                case "ancestor"           : return XPathAxis.Ancestor;
                case "ancestor-or-self"   : return XPathAxis.AncestorOrSelf;
                case "attribute"          : return XPathAxis.Attribute;
                case "child"              : return XPathAxis.Child;
                case "descendant"         : return XPathAxis.Descendant;
                case "descendant-or-self" : return XPathAxis.DescendantOrSelf;
                case "following"          : return XPathAxis.Following;
                case "following-sibling"  : return XPathAxis.FollowingSibling;
                case "namespace"          : return XPathAxis.Namespace;
                case "parent"             : return XPathAxis.Parent;
                case "preceding"          : return XPathAxis.Preceding;
                case "preceding-sibling"  : return XPathAxis.PrecedingSibling;
                case "self"               : return XPathAxis.Self;
                default                   : this.kind = LexKind.Name; return XPathAxis.Unknown;
            }
        }

        private void ScanNumber() {
            Debug.Assert(IsAsciiDigit(curChar) || curChar == '.');
            while (IsAsciiDigit(curChar)) {
                NextChar();
            }
            if (curChar == '.') {
                NextChar();
                while (IsAsciiDigit(curChar)) {
                    NextChar();
                }
            }
            if ((curChar & (~0x20)) == 'E') {
                NextChar();
                if (curChar == '+' || curChar == '-') {
                    NextChar();
                }
                while (IsAsciiDigit(curChar)) {
                    NextChar();
                }
                throw CreateException(Res.XPath_ScientificNotation);
            }
        }

        private void ScanString() {
            int startIdx = curIndex + 1;
            int endIdx = xpathExpr.IndexOf(curChar, startIdx);

            if (endIdx < 0) {
                SetSourceIndex(xpathExpr.Length);
                throw CreateException(Res.XPath_UnclosedString);
            }

            this.stringValue = xpathExpr.Substring(startIdx, endIdx - startIdx);
            SetSourceIndex(endIdx + 1);
        }

        private string ScanNCName() {
            Debug.Assert(xmlCharType.IsStartNCNameSingleChar(curChar) 
#if XML10_FIFTH_EDITION
                || xmlCharType.IsNCNameHighSurrogateChar(curChar)
#endif
                );
            int start = curIndex;
            for (;;) {
                if (xmlCharType.IsNCNameSingleChar(curChar)) {
                    NextChar();
                }
#if XML10_FIFTH_EDITION
                else if (xmlCharType.IsNCNameSurrogateChar(PeekNextChar(), curChar)) {
                    NextChar();
                    NextChar();
                }
#endif
                else {
                    break;
                }
            }
            return xpathExpr.Substring(start, curIndex - start);
        }

        public void PassToken(LexKind t) {
            CheckToken(t);
            NextLex();
        }

        public void CheckToken(LexKind t) {
            Debug.Assert(LexKind.FirstStringable <= t);
            if (kind != t) {
                if (t == LexKind.Eof) {
                    throw CreateException(Res.XPath_EofExpected, RawValue);
                } else {
                    throw CreateException(Res.XPath_TokenExpected, LexKindToString(t), RawValue);
                }
            }
        }

        // May be called for the following tokens: Name, String, Eof, Comma, LParens, RParens, LBracket, RBracket, RBrace
        private string LexKindToString(LexKind t) {
            Debug.Assert(LexKind.FirstStringable <= t);

            if (LexKind.LastNonChar < t) {
                Debug.Assert("()[].@,*/$}".IndexOf((char)t) >= 0);
                return new String((char)t, 1);
            }

            switch (t) {
                case LexKind.Name   : return "<name>";
                case LexKind.String : return "<string literal>";
                case LexKind.Eof    : return "<eof>";
                default:
                    Debug.Fail("Unexpected LexKind: " + t.ToString());
                    return string.Empty;
            }
        }

        public XPathCompileException CreateException(string resId, params string[] args) {
            return new XPathCompileException(xpathExpr, lexStart, curIndex, resId, args);
        }
    }
}
