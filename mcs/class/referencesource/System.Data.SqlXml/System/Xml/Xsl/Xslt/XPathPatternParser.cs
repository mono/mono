//------------------------------------------------------------------------------
// <copyright file="XPathPatternParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Xsl.Qil;
using System.Xml.Xsl.XPath;

namespace System.Xml.Xsl.Xslt {
    using XPathParser   = XPathParser<QilNode>;
    using XPathNodeType = System.Xml.XPath.XPathNodeType;
    using Res           = System.Xml.Utils.Res;

    internal class XPathPatternParser {
        public interface IPatternBuilder : IXPathBuilder<QilNode> {
            IXPathBuilder<QilNode> GetPredicateBuilder(QilNode context);
        }

        XPathScanner    scanner;
        IPatternBuilder ptrnBuilder;
        XPathParser     predicateParser = new XPathParser();

        public QilNode Parse(XPathScanner scanner, IPatternBuilder ptrnBuilder) {
            Debug.Assert(this.scanner == null && this.ptrnBuilder == null);
            Debug.Assert(scanner != null && ptrnBuilder != null);
            QilNode result = null;
            ptrnBuilder.StartBuild();
            try {
                this.scanner     = scanner;
                this.ptrnBuilder = ptrnBuilder;
                result = this.ParsePattern();
                this.scanner.CheckToken(LexKind.Eof);
            } finally {
                result = ptrnBuilder.EndBuild(result);
#if DEBUG
                this.ptrnBuilder = null;
                this.scanner = null;
#endif
            }
            return result;
        }

        /*
        *   Pattern ::= LocationPathPattern ('|' LocationPathPattern)*
        */
        private QilNode ParsePattern() {
            QilNode opnd = ParseLocationPathPattern();

            while (scanner.Kind == LexKind.Union) {
                scanner.NextLex();
                opnd = ptrnBuilder.Operator(XPathOperator.Union, opnd, ParseLocationPathPattern());
            }
            return opnd;
        }

        /*
        *   LocationPathPattern ::= '/' RelativePathPattern? | '//'? RelativePathPattern | IdKeyPattern (('/' | '//') RelativePathPattern)?
        */
        private QilNode ParseLocationPathPattern() {
            QilNode opnd;

            switch (scanner.Kind) {
            case LexKind.Slash :
                scanner.NextLex();
                opnd = ptrnBuilder.Axis(XPathAxis.Root, XPathNodeType.All, null, null);

                if (XPathParser.IsStep(scanner.Kind)) {
                    opnd = ptrnBuilder.JoinStep(opnd, ParseRelativePathPattern());
                }
                return opnd;
            case LexKind.SlashSlash :
                scanner.NextLex();
                return ptrnBuilder.JoinStep(
                    ptrnBuilder.Axis(XPathAxis.Root, XPathNodeType.All, null, null),
                    ptrnBuilder.JoinStep(
                        ptrnBuilder.Axis(XPathAxis.DescendantOrSelf, XPathNodeType.All, null, null),
                        ParseRelativePathPattern()
                    )
                );
            case LexKind.Name :
                if (scanner.CanBeFunction && scanner.Prefix.Length == 0 && (scanner.Name == "id" || scanner.Name == "key")) {
                    opnd = ParseIdKeyPattern();
                    switch (scanner.Kind) {
                    case LexKind.Slash :
                        scanner.NextLex();
                        opnd = ptrnBuilder.JoinStep(opnd, ParseRelativePathPattern());
                        break;
                    case LexKind.SlashSlash :
                        scanner.NextLex();
                        opnd = ptrnBuilder.JoinStep(opnd,
                            ptrnBuilder.JoinStep(
                                ptrnBuilder.Axis(XPathAxis.DescendantOrSelf, XPathNodeType.All, null, null),
                                ParseRelativePathPattern()
                            )
                        );
                        break;
                    }
                    return opnd;
                }
                break;
            }
            opnd = ParseRelativePathPattern();
            return opnd;
        }

        /*
        *   IdKeyPattern ::= 'id' '(' Literal ')' | 'key' '(' Literal ',' Literal ')'
        */
        private QilNode ParseIdKeyPattern() {
            Debug.Assert(scanner.CanBeFunction);
            Debug.Assert(scanner.Prefix.Length == 0);
            Debug.Assert(scanner.Name == "id" || scanner.Name == "key");
            List<QilNode> args = new List<QilNode>(2);

            if (scanner.Name == "id") {
                scanner.NextLex();
                scanner.PassToken(LexKind.LParens);
                scanner.CheckToken(LexKind.String);
                args.Add(ptrnBuilder.String(scanner.StringValue));
                scanner.NextLex();
                scanner.PassToken(LexKind.RParens);
                return ptrnBuilder.Function("", "id", args);
            } else {
                scanner.NextLex();
                scanner.PassToken(LexKind.LParens);
                scanner.CheckToken(LexKind.String);
                args.Add(ptrnBuilder.String(scanner.StringValue));
                scanner.NextLex();
                scanner.PassToken(LexKind.Comma);
                scanner.CheckToken(LexKind.String);
                args.Add(ptrnBuilder.String(scanner.StringValue));
                scanner.NextLex();
                scanner.PassToken(LexKind.RParens);
                return ptrnBuilder.Function("", "key", args);
            }
        }

        /*
        *   RelativePathPattern ::= StepPattern (('/' | '//') StepPattern)*
        */
        //Max depth to avoid StackOverflow
        const int MaxParseRelativePathDepth = 1024;
        private int parseRelativePath = 0;
        private QilNode ParseRelativePathPattern() {
            if (++parseRelativePath > MaxParseRelativePathDepth) {
                if (System.Xml.XmlConfiguration.XsltConfigSection.LimitXPathComplexity) {
                    throw scanner.CreateException(System.Xml.Utils.Res.Xslt_InputTooComplex);
                }
            }
            QilNode opnd = ParseStepPattern();
            if (scanner.Kind == LexKind.Slash) {
                scanner.NextLex();
                opnd = ptrnBuilder.JoinStep(opnd, ParseRelativePathPattern());
            } else if (scanner.Kind == LexKind.SlashSlash) {
                scanner.NextLex();
                opnd = ptrnBuilder.JoinStep(opnd,
                    ptrnBuilder.JoinStep(
                        ptrnBuilder.Axis(XPathAxis.DescendantOrSelf, XPathNodeType.All, null, null),
                        ParseRelativePathPattern()
                    )
                );
            }
            --parseRelativePath;
            return opnd;
        }

        /*
        *   StepPattern ::= ChildOrAttributeAxisSpecifier NodeTest Predicate*
        *   ChildOrAttributeAxisSpecifier ::= @ ? | ('child' | 'attribute') '::'
        */
        private QilNode ParseStepPattern() {
            QilNode     opnd;
            XPathAxis   axis;

            switch (scanner.Kind) {
            case LexKind.Dot:
            case LexKind.DotDot:
                throw scanner.CreateException(Res.XPath_InvalidAxisInPattern);
            case LexKind.At:
                axis = XPathAxis.Attribute;
                scanner.NextLex();
                break;
            case LexKind.Axis:
                axis = scanner.Axis;
                if (axis != XPathAxis.Child && axis != XPathAxis.Attribute) {
                    throw scanner.CreateException(Res.XPath_InvalidAxisInPattern);
                }
                scanner.NextLex();  // Skip '::'
                scanner.NextLex();
                break;
            case LexKind.Name:
            case LexKind.Star:
                // NodeTest must start with Name or '*'
                axis = XPathAxis.Child;
                break;
            default:
                throw scanner.CreateException(Res.XPath_UnexpectedToken, scanner.RawValue);
            }

            XPathNodeType  nodeType;
            string         nodePrefix, nodeName;
            XPathParser.InternalParseNodeTest(scanner, axis, out nodeType, out nodePrefix, out nodeName);
            opnd = ptrnBuilder.Axis(axis, nodeType, nodePrefix, nodeName);

            XPathPatternBuilder xpathPatternBuilder = ptrnBuilder as XPathPatternBuilder;
            if (xpathPatternBuilder != null) {
                //for XPathPatternBuilder, get all predicates and then build them
                List<QilNode> predicates = new List<QilNode>();
                while (scanner.Kind == LexKind.LBracket) {
                    predicates.Add(ParsePredicate(opnd));
                }
                if (predicates.Count > 0)
                    opnd = xpathPatternBuilder.BuildPredicates(opnd, predicates);
            }
            else {
                while (scanner.Kind == LexKind.LBracket) {
                    opnd = ptrnBuilder.Predicate(opnd, ParsePredicate(opnd), /*reverseStep:*/false);
                }
            }
            return opnd;
        }

        /*
        *   Predicate ::= '[' Expr ']'
        */
        private QilNode ParsePredicate(QilNode context) {
            Debug.Assert(scanner.Kind == LexKind.LBracket);
            scanner.NextLex();
            QilNode result = predicateParser.Parse(scanner, ptrnBuilder.GetPredicateBuilder(context), LexKind.RBracket);
            Debug.Assert(scanner.Kind == LexKind.RBracket);
            scanner.NextLex();
            return result;
        }
    }
}
