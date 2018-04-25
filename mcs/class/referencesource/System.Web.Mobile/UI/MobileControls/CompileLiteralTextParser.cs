//------------------------------------------------------------------------------
// <copyright file="CompileLiteralTextParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web.UI;

namespace System.Web.UI.MobileControls
{
    /*
     * CompileLiteralTextParser class.
     *
     * This is a specialized version of the LiteralTextParser class.
     * It creates a set of control builders from the parsed literal text.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class CompileLiteralTextParser : LiteralTextParser
    {
        TemplateParser _parser;
        ControlBuilder _parentBuilder;
        String _fileName;
        int _lineNumber;
        IList _tagInnerTextElements = null;

        internal CompileLiteralTextParser(TemplateParser parser, 
                                        ControlBuilder parentBuilder, 
                                        String fileName, 
                                        int lineNumber)
        {
            _parser = parser;
            _parentBuilder = parentBuilder;
            _fileName = fileName;
            _lineNumber = lineNumber;
        }

        protected override void ProcessElement(LiteralElement element)
        {
            ControlBuilder subBuilder;

            switch (element.Type)
            {
                case LiteralElementType.Text:
                    Debug.Assert(_tagInnerTextElements == null);
                    subBuilder = ControlBuilder.CreateBuilderFromType(
                                        _parser, _parentBuilder,
                                        typeof(LiteralText), typeof(LiteralText).Name,
                                        null, 
                                        GetPropertyDictionary(element.Format, element.BreakAfter, null),
                                        _lineNumber, _fileName);
                    break;

                case LiteralElementType.Anchor:
                {
                    String linkUrl = (String)element.GetAttribute("href");
                    subBuilder = ControlBuilder.CreateBuilderFromType(
                                        _parser, _parentBuilder,
                                        typeof(LiteralLink), typeof(LiteralLink).Name,
                                        null, 
                                        GetPropertyDictionary(element.Format, element.BreakAfter, linkUrl),
                                        _lineNumber, _fileName);
                    AddTagInnerTextElements(subBuilder);
                    break;
                }

                default:
                    return;
            }

            _parentBuilder.AppendSubBuilder(subBuilder);

            if (element.Text == null || element.Text.Length != 0)
            {
                subBuilder.AppendLiteralString(element.Text);
            }
        }

        private IList TagInnerTextElements
        {
            get
            {
                if (_tagInnerTextElements == null)
                {
                    _tagInnerTextElements = new ArrayList();
                }
                return _tagInnerTextElements;
            }
        }

        private void AddTagInnerTextElements(ControlBuilder builder)
        {
            if (_tagInnerTextElements != null)
            {
                foreach(Object o in _tagInnerTextElements)
                {
                    if (o is String)
                    {
                        builder.AppendLiteralString((String)o);
                    }
                    else
                    {
                        builder.AppendSubBuilder((ControlBuilder)o);
                    }
                }
                _tagInnerTextElements = null;
            }
        }

        protected override void ProcessTagInnerText(String text)
        {
            // Called to add an inner text segment of a multi-segment tag, e.g.
            //      <a ...>some text <%# a databinding %> some more text</a>

            TagInnerTextElements.Add(text);
        }

        internal /*public*/ void AddDataBinding(ControlBuilder builder)
        {
            if (IsInTag)
            {
                TagInnerTextElements.Add(builder);
            }
            else
            {
                ControlBuilder newBuilder = ControlBuilder.CreateBuilderFromType(
                                              _parser, _parentBuilder,
                                              typeof(LiteralText), typeof(LiteralText).Name,
                                            null, 
                                            GetPropertyDictionary(CurrentFormat, false, null),
                                            _lineNumber, _fileName);
                _parentBuilder.AppendSubBuilder(newBuilder);
                newBuilder.AppendSubBuilder(builder);
                OnAfterDataBoundLiteral();
            }
        }

        // Convert formatting and other options into a set of properties, just as if they had been
        // specified in persistence format.

        private ListDictionary GetPropertyDictionary(LiteralFormat format, bool breakAfter, String linkUrl)
        {
            ListDictionary dictionary = null;
            if (format != LiteralFormat.None || !breakAfter || linkUrl != null)
            {
                dictionary = new ListDictionary();
                if ((format & LiteralFormat.Bold) == LiteralFormat.Bold)
                {
                    dictionary.Add("Font-Bold", "True");
                }
                if ((format & LiteralFormat.Italic) == LiteralFormat.Italic)
                {
                    dictionary.Add("Font-Italic", "True");
                }
                if(!breakAfter)
                {
                    dictionary.Add("BreakAfter", "False");
                }
                if (linkUrl != null)
                {
                    dictionary.Add("NavigateUrl", linkUrl);
                }
            }

            return dictionary;
        }
    }

}

