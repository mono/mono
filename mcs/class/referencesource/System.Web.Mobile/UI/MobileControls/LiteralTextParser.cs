//------------------------------------------------------------------------------
// <copyright file="LiteralTextParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Web;

namespace System.Web.UI.MobileControls
{
    /*
     * LiteralTextParser class.
     *
     * The LiteralTextParser class parses a string of literal text, 
     * containing certain recognizable tags, and creates a set of controls
     * from them. Any unrecognized tags are ignored.
     *
     * This is an abstract base class. RuntimeLiteralTextParser and 
     * CompileTimeLiteralTextParser inherit from this class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal abstract class LiteralTextParser
    {
        // The parsing methods (Parse, ParseTag, ParseTagAttributes, ParseText) 
        // build up LiteralElement objects, which can either be text or tags. 
        // ProcessElementInternal is then called. It combines some other data
        // and calls ProcessElement, a method which is overridable by inherited
        // classes.

        // Literal Element type - includes recognized tags.

        protected enum LiteralElementType
        {
            Unrecognized,
            Text,
            Bold,
            Italic,
            Break,
            Paragraph,
            Anchor,
        }

        // Available formatting options for literal elements. This enum can 
        // be combined with the | operator.

        protected enum LiteralFormat
        {
            None = 0,
            Bold = 1,
            Italic = 2,
        }

        // Literal Element.

        protected class LiteralElement
        {
            public LiteralElementType Type;
            public IDictionary Attributes;
            public String Text;
            public LiteralFormat Format = LiteralFormat.None;
            public bool BreakAfter = false;
            public bool ForceBreakTag = false;

            public LiteralElement(String text)
            {
                Type = LiteralElementType.Text;
                Attributes = null;
                Text = text;
            }

            public LiteralElement(LiteralElementType type, IDictionary attributes)
            {
                Type = type;
                Attributes = attributes;
                Text = String.Empty;
            }

            public bool IsText
            {
                get
                {
                    return Type == LiteralElementType.Text;
                }
            }

            public bool IsEmptyText
            {
                get
                {
                    return IsText && !LiteralTextParser.IsValidText(Text);
                }
            }

            public String GetAttribute(String attributeName)
            {
                Object o = (Attributes != null) ? Attributes[attributeName] : null;
                return (o != null) ? (String)o : String.Empty;
            }
        }

        // Methods overriden by inherited classes.

        protected abstract void ProcessElement(LiteralElement element);
        protected abstract void ProcessTagInnerText(String text);

        private bool           _isBreakingReset   = true;
        private LiteralElement _lastQueuedElement = null;
        private LiteralElement _currentTag        = null;
        private bool           _beginNewParagraph = true;
        private FormatStack    _formatStack       = new FormatStack();
        private bool           _elementsProcessed = false;

        // Static constructor that builds a lookup table of recognized tags.

        private static IDictionary _recognizedTags = new Hashtable();
        static LiteralTextParser()
        {
            // PERF: Add both lowercase and uppercase.

            _recognizedTags.Add("b", LiteralElementType.Bold);
            _recognizedTags.Add("B", LiteralElementType.Bold);

            _recognizedTags.Add("i", LiteralElementType.Italic);
            _recognizedTags.Add("I", LiteralElementType.Italic);

            _recognizedTags.Add("br", LiteralElementType.Break);
            _recognizedTags.Add("BR", LiteralElementType.Break);

            _recognizedTags.Add("p", LiteralElementType.Paragraph);
            _recognizedTags.Add("P", LiteralElementType.Paragraph);

            _recognizedTags.Add("a", LiteralElementType.Anchor);
            _recognizedTags.Add("A", LiteralElementType.Anchor);
        }

        // Convert a tag name to a type.

        private static LiteralElementType TagNameToType(String tagName)
        {
            Object o = _recognizedTags[tagName];
            if (o == null)
            {
                o = _recognizedTags[tagName.ToLower(CultureInfo.InvariantCulture)];
            }
            return (o != null) ? (LiteralElementType)o : LiteralElementType.Unrecognized;
        }

        // Returns true if any valid controls could be generated from the given text.

        internal /*public*/ static bool IsValidText(String validText)
        {
            // 

            if (validText.Length == 0)
            {
                return false;
            }

            foreach (char c in validText)
            {
                if (!Char.IsWhiteSpace(c) &&
                        c != '\t' &&
                        c != '\r' && 
                        c != '\n')
                {
                    return true;
                }
            }

            return false;
        }

        // Main parse routine. Called with a block of text to parse.

        internal /*public*/ void Parse(String literalText)
        {
            int length = literalText.Length;
            int currentPosition = 0;

            while (currentPosition < length)
            {
                // Find start of next tag.

                int nextTag = literalText.IndexOf('<', currentPosition);
                if (nextTag == -1)
                {
                    ParseText(literalText.Substring(currentPosition));
                    break;
                }

                if (nextTag > currentPosition)
                {
                    ParseText(literalText.Substring(currentPosition, nextTag - currentPosition));
                }

                // Find end of tag.

                char quoteChar = '\0';
                int endOfTag;
                for (endOfTag = nextTag + 1; endOfTag < length; endOfTag++)
                {
                    char c = literalText[endOfTag];
                    if (quoteChar == '\0')
                    {
                        if (c == '\'' || c == '\"')
                        {
                            quoteChar = c;
                        }
                        else if (c == '>')
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (c == quoteChar)
                        {
                            quoteChar = '\0';
                        }
                    }
                }

                if (endOfTag == length)
                {
                    // 
                    break;
                }

                ParseTag(literalText, nextTag + 1, endOfTag);
                currentPosition = endOfTag + 1;
            }

            Flush();
        }

        internal /*public*/ void ResetBreaking()
        {
            _isBreakingReset = true;
            ElementsProcessed = false;
        }

        internal /*public*/ void ResetNewParagraph()
        {
            _beginNewParagraph = false;
        }

        internal /*public*/ void UnResetBreaking()
        {
            _isBreakingReset = false;
        }

        protected bool ElementsProcessed
        {
            get
            {
                return _elementsProcessed;
            }

            set
            {
                _elementsProcessed = value;
            }
        }

        protected void OnAfterDataBoundLiteral()
        {
            ElementsProcessed = true;
            UnResetBreaking();
        }

        // Parse a single tag.

        private void ParseTag(String literalText, int tagStart, int tagFinish)
        {
            bool isClosingTag = (literalText[tagStart] == '/');
            if (isClosingTag) 
            {
                tagStart++;
            }

            // Empty tag?

            if (tagStart == tagFinish)
            {
                return;
            }

            // Look for end of tag name.

            int tagNameFinish = tagStart;
            while (tagNameFinish < tagFinish && 
                   !Char.IsWhiteSpace(literalText[tagNameFinish]) && literalText[tagNameFinish] != '/')
            {
                tagNameFinish++;
            }

            // Extract tag name, and compare to recognized tags.

            String tagName = literalText.Substring(tagStart, tagNameFinish - tagStart);
            LiteralElementType tagType = TagNameToType(tagName);
            if (tagType == LiteralElementType.Unrecognized)
            {
                return;
            }

            // Are we already in a complex tag?

            if (_currentTag != null)
            {
                // Ignore any inner tags, except the closing tag.

                if (_currentTag.Type == tagType && isClosingTag)
                {
                    ProcessElementInternal(_currentTag);
                    _currentTag = null;
                }
                else
                {
                    // 
                }
                return;
            }

            switch (tagType)
            {
                case LiteralElementType.Paragraph:
                    
                    // Do not create two breaks for </p><p> pairs.

                    if (!_isBreakingReset)
                    {
                        _isBreakingReset = true;
                        goto case LiteralElementType.Break;
                    }

                    break;

                case LiteralElementType.Break:

                    // If a break is already pending, insert an empty one.

                    if (_beginNewParagraph)
                    {
                        ParseText(String.Empty);
                    }
                    if (_lastQueuedElement != null && 
                        _lastQueuedElement.Text.Length == 0)
                    {
                        _lastQueuedElement.ForceBreakTag = true;
                    }
                    _beginNewParagraph = true;
                    break;

                case LiteralElementType.Bold:

                    if (isClosingTag)
                    {
                        _formatStack.Pop(FormatStack.Bold);
                    }
                    else
                    {
                        _formatStack.Push(FormatStack.Bold);
                    }
                    break;

                case LiteralElementType.Italic:

                    if (isClosingTag)
                    {
                        _formatStack.Pop(FormatStack.Italic);
                    }
                    else
                    {
                        _formatStack.Push(FormatStack.Italic);
                    }
                    break;

                default:
                {
                    if (!isClosingTag)
                    {
                        IDictionary attribs = ParseTagAttributes(literalText, tagNameFinish, tagFinish, tagName);
                        _currentTag = new LiteralElement(tagType, attribs);
                    }
                    break;
                }
            }

            if (_isBreakingReset && tagType != LiteralElementType.Paragraph)
            {
                _isBreakingReset = false;
            }
        }

        protected bool IsInTag
        {
            get
            {
                return _currentTag != null;
            }
        }

        protected LiteralFormat CurrentFormat
        {
            get
            {
                return _formatStack.CurrentFormat;
            }
        }

        // Parse attributes of a tag.

        private enum AttributeParseState
        {
            StartingAttributeName,
            ReadingAttributeName,
            ReadingEqualSign,
            StartingAttributeValue,
            ReadingAttributeValue,
            Error,
        }

        private IDictionary ParseTagAttributes(String literalText, int attrStart, int attrFinish, String tagName)
        {
            if (attrFinish > attrStart && literalText[attrFinish - 1] == '/')
            {
                attrFinish--;
            }

            IDictionary dictionary = null;
            int attrPos = attrStart;
            bool skipWhiteSpaces = true;
            int attrNameStart = 0;
            int attrNameFinish = 0; 
            int attrValueStart = 0;
            char quoteChar = '\0';
            AttributeParseState state = AttributeParseState.StartingAttributeName;

            while (attrPos <= attrFinish && state != AttributeParseState.Error)
            {
                char c = attrPos == attrFinish ? '\0' : literalText[attrPos];

                if (skipWhiteSpaces)
                {
                    if (Char.IsWhiteSpace(c))
                    {
                        attrPos++;
                        continue;
                    }
                    else
                    {
                        skipWhiteSpaces = false;
                    }
                }

                switch (state)
                {
                    case AttributeParseState.StartingAttributeName:
                        if (c == '\0')
                        {
                            attrPos = attrFinish + 1;
                        }
                        else
                        {
                            attrNameStart = attrPos;
                            state = AttributeParseState.ReadingAttributeName;
                        }
                        break;

                    case AttributeParseState.ReadingAttributeName:
                        if (c == '=' || Char.IsWhiteSpace(c))
                        {
                            attrNameFinish = attrPos;
                            skipWhiteSpaces = true;
                            state = AttributeParseState.ReadingEqualSign;
                        }
                        else if (c == '\0')
                        {
                            state = AttributeParseState.Error;
                        }
                        else
                        {
                            attrPos++;
                        }
                        break;

                    case AttributeParseState.ReadingEqualSign:
                        if (c == '=')
                        {
                            skipWhiteSpaces = true;
                            state = AttributeParseState.StartingAttributeValue;
                            attrPos++;
                        }
                        else
                        {
                            state = AttributeParseState.Error;
                        }
                        break;

                    case AttributeParseState.StartingAttributeValue:
                        attrValueStart = attrPos;
                        if (c == '\0')
                        {
                            state = AttributeParseState.Error;
                            break;
                        } 
                        else if (c == '\"' || c == '\'')
                        {
                            quoteChar = c;
                            attrValueStart++;
                            attrPos++;
                        }
                        else
                        {
                            quoteChar = '\0';
                        }
                        state = AttributeParseState.ReadingAttributeValue;
                        break;

                    case AttributeParseState.ReadingAttributeValue:
                        if (c == quoteChar || 
                            ((Char.IsWhiteSpace(c) || c == '\0') && quoteChar == '\0'))
                        {
                            if (attrNameFinish == attrNameStart)
                            {
                                state = AttributeParseState.Error;
                                break;
                            }

                            if (dictionary == null)
                            {
                                dictionary = new HybridDictionary(true);
                            }

                            dictionary.Add(
                                literalText.Substring(attrNameStart, attrNameFinish - attrNameStart),
                                literalText.Substring(attrValueStart, attrPos - attrValueStart));

                            skipWhiteSpaces = true;
                            state = AttributeParseState.StartingAttributeName;
                            if (c == quoteChar)
                            {
                                attrPos++;
                            }
                        }
                        else
                        {
                            attrPos++;
                        }
                        break;
                }
            }

            if (state == AttributeParseState.Error)
            {
                throw new Exception(SR.GetString(SR.LiteralTextParser_InvalidTagFormat));
            }

            return dictionary;
        }

        // Parse a plain text literal.

        private void ParseText(String text)
        {
            if (_currentTag != null)
            {
                // Add to inner text of tag.
                _currentTag.Text += text;
            }
            else
            {
                if (_isBreakingReset && IsValidText(text))
                {
                    _isBreakingReset = false;
                }
                ProcessElementInternal(new LiteralElement(text));
            }
        }

        private void ProcessElementInternal(LiteralElement element)
        {
            // This method needs to fill in an element with formatting and
            // breaking information, and calls ProcessElement. However,
            // each element needs to know whether there will be a break
            // AFTER the element, so elements are processed lazily, keeping
            // the last one in a single-element queue.

            LiteralFormat currentFormat = _formatStack.CurrentFormat;

            if (_lastQueuedElement != null)
            {
                // If both the last and current element are text elements, and 
                // the formatting hasn't changed, then just combine the two into
                // a single element.

                if (_lastQueuedElement.IsText && element.IsText && 
                        (_lastQueuedElement.Format == currentFormat) && 
                        !_beginNewParagraph)
                {
                    _lastQueuedElement.Text += element.Text;
                    return;
                }
                else if (_lastQueuedElement.IsEmptyText && 
                         !_beginNewParagraph &&
                         IgnoreWhiteSpaceElement(_lastQueuedElement))
                {
                    // Empty text element with no breaks - so just ignore.
                }
                else
                {
                    _lastQueuedElement.BreakAfter = _beginNewParagraph;
                    ProcessElement(_lastQueuedElement);
                    ElementsProcessed = true;
                }
            }

            _lastQueuedElement = element;
            _lastQueuedElement.Format = currentFormat;
            _beginNewParagraph = false;
        }

        private void Flush()
        {
            if (_currentTag != null)
            {
                // In the middle of a tag. There may be multiple inner text elements inside
                // a tag, e.g.
                //      <a ...>some text <%# a databinding %> some more text</a>
                // and we're being flushed just at the start of the databinding.

                if (!_currentTag.IsEmptyText)
                {
                    ProcessTagInnerText(_currentTag.Text);
                }
                _currentTag.Text = String.Empty;
                return;
            }

            if (_lastQueuedElement == null)
            {
                return;
            }

            // Ignore orphaned whitespace.
                    
            if (!_lastQueuedElement.ForceBreakTag && _lastQueuedElement.IsEmptyText)
            {
                if (!ElementsProcessed)
                {
                    return;
                }
                if (_lastQueuedElement.Text.Length == 0 || _lastQueuedElement.Text[0] != ' ')
                {
                    return;
                }

                _lastQueuedElement.Text = " ";
            }

            _lastQueuedElement.BreakAfter = _beginNewParagraph;
            ProcessElement(_lastQueuedElement);
            _lastQueuedElement = null;
        }

        protected virtual bool IgnoreWhiteSpaceElement(LiteralElement element)
        {
            return true;
        }

        /*
         * FormatStack private class
         *
         * This class maintains a simple stack of formatting directives. As tags and 
         * closing tags are processed, they are pushed on and popped off this stack.
         * The CurrentFormat property returns the current state.
         */

        private class FormatStack
        {
            internal const char Bold = 'b';
            internal const char Italic = 'i';
            
            private StringBuilder _stringBuilder = new StringBuilder(16);

            public void Push(char option)
            {
                _stringBuilder.Append(option);
            }

            public void Pop(char option)
            {
                // Only pop a matching directive - non-matching directives are ignored!

                int length = _stringBuilder.Length;
                if (length > 0 && _stringBuilder[length - 1] == option)
                {
                    _stringBuilder.Remove(length - 1, 1);
                }
            }

            public LiteralFormat CurrentFormat
            {
                get
                {
                    LiteralFormat format = LiteralFormat.None;
                    for (int i = _stringBuilder.Length - 1; i >= 0; i--)
                    {
                        switch (_stringBuilder[i])
                        {
                            case Bold:
                                format |= LiteralFormat.Bold;
                                break;
                            case Italic:
                                format |= LiteralFormat.Italic;
                                break;
                        }
                    }
                    return format;
                }
            }
        }

    }

}

