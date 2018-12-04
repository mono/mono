// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Xaml;
using System.Diagnostics;
using MS.Internal.Xaml.Context;
using System.Xaml.Schema;
using System.Xaml.MS.Impl;

namespace MS.Internal.Xaml.Parser
{
    internal class MePullParser
    {
        XamlParserContext _context;
        string _originalText;
        MeScanner _tokenizer;
        string _brokenRule;

        [DebuggerDisplay("{found}")]
        private class Found
        {
            public bool found;
        }

        public MePullParser(XamlParserContext stack)
        {
            _context = stack;
        }

        // MarkupExtension ::= '{' TYPENAME Arguments? '}'
        //    Arguments    ::= (PositionalArgs ( ',' NamedArgs)?) | NamedArgs 
        //    NamedArgs    ::= NamedArg ( ',' NamedArg )*
        //    NamedArg     ::= PROPERTYNAME '=' (STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
        //  PositionalArgs ::= (Value (',' PositionalArgs)?) | NamedArg
        //       Value     ::= STRING | QUOTEDMARKUPEXTENSION |MarkupExtension

        public IEnumerable<XamlNode> Parse(string text, int lineNumber, int linePosition)
        {
            _tokenizer = new MeScanner(_context, text, lineNumber, linePosition);
            _originalText = text;
            Found f = new Found();
            NextToken();
            foreach (XamlNode node in P_MarkupExtension(f))
            {
                yield return node;
            }
            if (!f.found)
            {
                string brokenRule = _brokenRule;
                _brokenRule = null;
                throw new XamlParseException(_tokenizer, brokenRule);
            }
            if (_tokenizer.Token != MeTokenType.None)
            {
                throw new XamlParseException(_tokenizer, SR.Get(SRID.UnexpectedTokenAfterME));
            }
            if (_tokenizer.HasTrailingWhitespace)
            {
                throw new XamlParseException(_tokenizer, SR.Get(SRID.WhitespaceAfterME));
            }
        }

        private void SetBrokenRuleString(string ruleString)
        {
            if (String.IsNullOrEmpty(_brokenRule))
            {
                _brokenRule = SR.Get(SRID.UnexpectedToken,
                                            _tokenizer.Token, ruleString, _originalText);
            }
        }

        private bool Expect(MeTokenType token, string ruleString)
        {
            if(_tokenizer.Token != token)
            {
                SetBrokenRuleString(ruleString);
                return false;
            }
            return true;
        }

        ////////////////////////////////
        // MarkupExtension ::= '{' TYPENAME Arguments? '}'
        //
        private IEnumerable<XamlNode> P_MarkupExtension(Found f)
        {
            // MarkupExtension ::= @'{' TYPENAME Arguments? '}'
            if (Expect(MeTokenType.Open, "MarkupExtension ::= @'{' Expr '}'"))
            {
                NextToken();

                // MarkupExtension ::= '{' @TYPENAME Arguments? '}'
                if (_tokenizer.Token == MeTokenType.TypeName)
                {
                    XamlType xamlType = _tokenizer.TokenType;

                    yield return Logic_StartElement(xamlType, _tokenizer.Namespace);

                    NextToken();

                    // MarkupExtension ::= '{' TYPENAME @(Arguments)? '}'
                    Found f2 = new Found();
                    switch (_tokenizer.Token)
                    {
                    // MarkupExtension ::= '{' TYPENAME (Arguments)? @'}'
                    case MeTokenType.Close:  // legal, Arguments is optional
                        yield return Logic_EndObject();
                        NextToken();
                        f.found = true;
                        break;

                    case MeTokenType.String:
                    case MeTokenType.QuotedMarkupExtension:
                    case MeTokenType.PropertyName:
                    case MeTokenType.Open:
                        // MarkupExtension ::= '{' TYPENAME (@Arguments)? '}'
                        foreach (XamlNode node in P_Arguments(f2))
                        {
                            yield return node;
                        }
                        break;

                    default:
                        SetBrokenRuleString("MarkupExtension ::= '{' TYPENAME @(Arguments)? '}'");
                        break;
                    }

                    if (f2.found)
                    {
                        if (Expect(MeTokenType.Close, "MarkupExtension ::= '{' TYPENAME (Arguments)? @'}'"))
                        {
                            yield return Logic_EndObject();
                            f.found = true;
                            NextToken();
                        }
                    }
                }
                else
                {
                    SetBrokenRuleString("MarkupExtension ::= '{' @TYPENAME (Arguments)? '}'");
                }
            }
        }

        ////////////////////////////////
        // Arguments ::= (PositionalArgs ( ',' NamedArgs)?) | NamedArgs 
        //
        private IEnumerable<XamlNode> P_Arguments(Found f)
        {
            Found f2 = new Found();
            // Arguments ::= @ (PositionalArgs ( ',' NamedArgs)?) | NamedArgs 
            switch (_tokenizer.Token)
            {
            case MeTokenType.Close:  // not found
                break;

            // Arguments ::= (@ PositionalArgs ( ',' NamedArgs)?) | NamedArgs 
            case MeTokenType.String:
            case MeTokenType.QuotedMarkupExtension:
            case MeTokenType.Open:
                foreach (XamlNode node in P_PositionalArgs(f2))
                {
                    yield return node;
                }
                f.found = f2.found;
                if (f.found)
                {
                    if (_context.CurrentArgCount > 0)
                    {
                        yield return Logic_EndPositionalParameters();
                    }
                }

                // Arguments ::= (PositionalArgs @ ( ',' NamedArgs)?) | NamedArgs 
                while (_tokenizer.Token == MeTokenType.Comma)
                {
                    // Arguments ::= (PositionalArgs ( @ ',' NamedArgs)?) | NamedArgs 
                    NextToken();

                    // Arguments ::= (PositionalArgs ( ',' @ NamedArgs)?) | NamedArgs 
                    foreach (XamlNode node in P_NamedArgs(f2))
                    {
                        yield return node;
                    }
                }
                break;

            // Arguments ::= (PositionalArgs ( ',' NamedArgs)?) | @ NamedArgs 
            case MeTokenType.PropertyName:
                foreach (XamlNode node in P_NamedArgs(f2))
                {
                    yield return node;
                }
                f.found = f2.found;
                break;

            default:
                SetBrokenRuleString("Arguments ::= @ (PositionalArgs ( ',' NamedArgs)?) | NamedArgs");
                break;
            }
        }
    
        ////////////////////////////////
        //  PositionalArgs ::= (Value (',' PositionalArgs)?) | NamedArg
        //
        private IEnumerable<XamlNode> P_PositionalArgs(Found f)
        {
            Found f2 = new Found();

            //  PositionalArgs ::= @ (Value (',' PositionalArgs)?) | NamedArg
            switch (_tokenizer.Token)
            {
            //  PositionalArgs ::= ( @ Value (',' PositionalArgs)?) | NamedArg
            case MeTokenType.String:
            case MeTokenType.QuotedMarkupExtension:
            case MeTokenType.Open:
                if (_context.CurrentArgCount++ == 0)
                {
                    yield return Logic_StartPositionalParameters();
                }

                foreach (XamlNode node in P_Value(f2))
                {
                    yield return node;
                }
                if (!f2.found)
                {
                    SetBrokenRuleString("PositionalArgs ::= (NamedArg | (@Value (',' PositionalArgs)?)");
                    break;
                }
                f.found = f2.found;

                //  PositionalArgs ::= (Value @ (',' PositionalArgs)?) | NamedArg
                if (_tokenizer.Token == MeTokenType.Comma)
                {
                    Found f3 = new Found();

                    //  PositionalArgs ::= (Value ( @ ',' PositionalArgs)?) | NamedArg
                    NextToken();

                    //  PositionalArgs ::= (Value (',' @ PositionalArgs)?) | NamedArg
                    foreach (XamlNode node in P_PositionalArgs(f3))
                    {
                        yield return node;
                    }
                    if (!f3.found)
                    {
                        SetBrokenRuleString("PositionalArgs ::= (Value (',' @ PositionalArgs)?) | NamedArg");
                        break;
                    }
                    // no f.found this is optional
                }
                break;

            //  PositionalArgs ::= (Value (',' PositionalArgs)?) | @ NamedArg
            case MeTokenType.PropertyName:
                if (_context.CurrentArgCount > 0)
                {
                    yield return Logic_EndPositionalParameters();
                }
                foreach (XamlNode node in P_NamedArg(f2))
                {
                    yield return node;
                }
                if (!f2.found)
                {
                    SetBrokenRuleString("PositionalArgs ::= (Value (',' PositionalArgs)?) | @ NamedArg");
                }
                f.found = f2.found;
                break;

            default:
                SetBrokenRuleString("PositionalArgs ::= @ (Value (',' PositionalArgs)?) | NamedArg");
                break;
            }
        }

        ////////////////////////////////
        // NamedArgs ::= NamedArg ( ',' NamedArg )*
        //
        private IEnumerable<XamlNode> P_NamedArgs(Found f)
        {
            Found f2 = new Found();

            // NamedArgs ::= @NamedArg ( ',' NamedArg )*
            switch (_tokenizer.Token)
            {
            case MeTokenType.PropertyName:
                foreach (XamlNode node in P_NamedArg(f2))
                {
                    yield return node;
                }
                f.found = f2.found;

                // NamedArgs ::= NamedArg @( ',' NamedArg )*
                while (_tokenizer.Token == MeTokenType.Comma)
                {
                    // NamedArgs ::= NamedArg ( @',' NamedArg )*
                    NextToken();

                    // NamedArgs ::= NamedArg ( ',' @NamedArg )*
                    foreach (XamlNode node in P_NamedArg(f2))
                    {
                        yield return node;
                    }
                }
                break;

            default:
                SetBrokenRuleString("NamedArgs ::= @NamedArg ( ',' NamedArg )*");
                break;
            }
        }

        ////////////////////////////////
        //   Value   ::= (STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
        //
        private IEnumerable<XamlNode> P_Value(Found f)
        {
            Found f2 = new Found();

            //   Value   ::= @(STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
            switch (_tokenizer.Token)
            {
            //   Value   ::= (@STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
            case MeTokenType.String:
                yield return Logic_Text();
                f.found = true;
                NextToken();
                break;

            //   Value   ::= (STRING | @QUOTEDMARKUPEXTENSION | MarkupExtension)
            case MeTokenType.QuotedMarkupExtension:
                MePullParser nestedParser = new MePullParser(_context);
                foreach (XamlNode node in nestedParser.Parse(_tokenizer.TokenText, LineNumber, LinePosition))
                {
                    yield return node;
                }
                f.found = true;
                NextToken();
                break;

            //   Value   ::= (STRING | QUOTEDMARKUPEXTENSION | @MarkupExtension)
            case MeTokenType.Open:
                foreach (XamlNode node in P_MarkupExtension(f2))
                {
                    yield return node;
                }
                f.found = f2.found;
                break;

            default:
                break;
            }
        }

        ////////////////////////////////
        // NamedArg ::= PROPERTYNAME '=' (STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
        //
        private IEnumerable<XamlNode> P_NamedArg(Found f)
        {
            Found f2 = new Found();

            // NamedArg ::= @PROPERTYNAME '=' (STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
            if (_tokenizer.Token == MeTokenType.PropertyName)
            {
                XamlMember property = _tokenizer.TokenProperty;
                yield return Logic_StartMember();
                NextToken();

                // NamedArg ::= PROPERTYNAME @'=' (STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
                Expect(MeTokenType.EqualSign, "NamedArg ::= PROPERTYNAME @'=' Value");
                NextToken();

                // NamedArg ::= PROPERTYNAME '=' @(STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
                switch (_tokenizer.Token)
                {
                // NamedArg ::= PROPERTYNAME '=' (@STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)
                case MeTokenType.String:
                    yield return Logic_Text();
                    f.found = true;
                    NextToken();
                    break;

                // NamedArg ::= PROPERTYNAME '=' (STRING | @QUOTEDMARKUPEXTENSION | MarkupExtension)
                case MeTokenType.QuotedMarkupExtension:
                    MePullParser nestedParser = new MePullParser(_context);
                    foreach (XamlNode node in nestedParser.Parse(_tokenizer.TokenText, LineNumber, LinePosition))
                    {
                        yield return node;
                    }
                    f.found = true;
                    NextToken();
                    break;

                // NamedArg ::= PROPERTYNAME '=' (STRING | QUOTEDMARKUPEXTENSION | @MarkupExtension)
                case MeTokenType.Open:
                    foreach (XamlNode node in P_Value(f2))
                    {
                        yield return node;
                    }
                    f.found = f2.found;
                    break;

                case MeTokenType.PropertyName:
                    {
                        string error;
                        if (_context.CurrentMember == null)
                        {
                            error = SR.Get(SRID.MissingComma1,  _tokenizer.TokenText);
                        }
                        else
                        {
                            error = SR.Get(SRID.MissingComma2, _context.CurrentMember.Name, _tokenizer.TokenText);
                        }
                        throw new XamlParseException(_tokenizer, error);
                    }

                default:
                    SetBrokenRuleString("NamedArg ::= PROPERTYNAME '=' @(STRING | QUOTEDMARKUPEXTENSION | MarkupExtension)");
                    break;
                }
                yield return Logic_EndMember();
            }
        }


        // ================================================

        private void NextToken()
        {
            _tokenizer.Read();
        }

        private int LineNumber
        {
            get { return _tokenizer.LineNumber; }
        }


        private int LinePosition
        {
            get { return _tokenizer.LinePosition; }
        }

        // ================================================

        private XamlNode Logic_StartElement(XamlType xamlType, string xamlNamespace)
        {
            _context.PushScope();
            _context.CurrentType = xamlType;
            _context.CurrentTypeNamespace = xamlNamespace;

            _context.InitLongestConstructor(xamlType);
            _context.InitBracketCharacterCacheForType(xamlType);
            _context.CurrentBracketModeParseParameters = new BracketModeParseParameters(_context);

            var startObj = new XamlNode(XamlNodeType.StartObject, xamlType);
            return startObj;
        }

        private XamlNode Logic_EndObject()
        {
            _context.PopScope();
            return new XamlNode(XamlNodeType.EndObject);
        }

        private XamlNode Logic_StartMember()
        {
            XamlMember member = _tokenizer.TokenProperty;
            _context.CurrentMember = member;

            XamlNode startMember = new XamlNode(XamlNodeType.StartMember, member);
            return startMember;
        }

        private XamlNode Logic_EndMember()
        {
            _context.CurrentMember = null;
            return new XamlNode(XamlNodeType.EndMember);
        }

        private XamlNode Logic_StartPositionalParameters()
        {
            _context.CurrentMember = XamlLanguage.PositionalParameters;

            XamlNode startProperty = new XamlNode(XamlNodeType.StartMember, XamlLanguage.PositionalParameters);
            return startProperty;
        }

        private XamlNode Logic_EndPositionalParameters()
        {
            // the Ctor args were pushed onto the Builder (XamlWriter)
            // stack, but were not pushed onto the parser stack, so the
            // ME is still the CurrentType for us.

            XamlType xamlType = _context.CurrentType;

            _context.CurrentArgCount = 0;
            _context.CurrentMember = null;
            return new XamlNode(XamlNodeType.EndMember);
        }

        private XamlNode Logic_Text()
        {
            string text = _tokenizer.TokenText;
            XamlNode textNode = new XamlNode(XamlNodeType.Value, text);
            return textNode;
        }
    }
}
