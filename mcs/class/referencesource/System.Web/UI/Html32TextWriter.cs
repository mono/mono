//------------------------------------------------------------------------------
// <copyright file="Html32TextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using System.Web.UI.WebControls;  // 
    using System.Security.Permissions;
    using System.Web.Util;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class Html32TextWriter : HtmlTextWriter {

        private const int NOTHING = 0x0000;
        private const int FONT_AROUND_CONTENT = 0x0001;
        private const int FONT_AROUND_TAG = 0x0002;
        private const int TABLE_ATTRIBUTES = 0x0004;
        private const int TABLE_AROUND_CONTENT = 0x0008;
        private const int FONT_PROPAGATE = 0x0010;
        private const int FONT_CONSUME = 0x0020;
        private const int SUPPORTS_HEIGHT_WIDTH = 0x0040;
        private const int SUPPORTS_BORDER = 0x0080;
        private const int SUPPORTS_NOWRAP = 0x0100;

        private StringBuilder _afterContent;
        private StringBuilder _afterTag;
        private StringBuilder _beforeContent;
        private StringBuilder _beforeTag;
        private string _fontColor;
        private string _fontFace;
        private string _fontSize;
        //

        private Stack _fontStack;
        private bool _shouldPerformDivTableSubstitution;
        private bool _renderFontTag;
        private bool _supportsBold = true;
        private bool _supportsItalic = true;
        private int _tagSupports;


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Html32TextWriter(TextWriter writer) : this(writer, DefaultTabString) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Html32TextWriter(TextWriter writer, string tabString) : base(writer, tabString) {
            // The initial capacities should be set up such that they are at least twice what
            // we expect for the maximum content we're going to stuff in into the builder.
            // This gives the best perf when using the Length property to reset the builder.

            _beforeTag = new StringBuilder(256);
            _beforeContent = new StringBuilder(256);
            _afterContent = new StringBuilder(128);
            _afterTag = new StringBuilder(128);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected Stack FontStack {
            get {
                if (_fontStack == null) {
                    _fontStack = new Stack(3);
                }
                return _fontStack;
            }
        }

        internal override bool RenderDivAroundHiddenInputs {
            get {
                return false;
            }
        }

        public bool ShouldPerformDivTableSubstitution {
            get {
                return _shouldPerformDivTableSubstitution;
            }
            set {
                _shouldPerformDivTableSubstitution = value;
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public bool SupportsBold {
            get {
                return _supportsBold;
            }
            set {
                _supportsBold = value;
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public bool SupportsItalic {
            get {
                return _supportsItalic;
            }
            set {
                _supportsItalic = value;
            }
        }

        private void AppendFontTag(StringBuilder sbBegin,StringBuilder sbEnd) {
            AppendFontTag(_fontFace, _fontColor, _fontSize, sbBegin, sbEnd);
        }

        private void AppendFontTag(string fontFace, string fontColor, string fontSize, StringBuilder sbBegin,StringBuilder sbEnd) {
            // append font begin tag
            sbBegin.Append(TagLeftChar);
            sbBegin.Append("font");
            if (fontFace != null) {
                sbBegin.Append(" face");
                sbBegin.Append(EqualsDoubleQuoteString);
                sbBegin.Append(fontFace);
                sbBegin.Append(DoubleQuoteChar);
            }
            if (fontColor != null) {
                sbBegin.Append(" color=");
                sbBegin.Append(DoubleQuoteChar);
                sbBegin.Append(fontColor);
                sbBegin.Append(DoubleQuoteChar);
            }
            if (fontSize != null) {
                sbBegin.Append(" size=");
                sbBegin.Append(DoubleQuoteChar);
                sbBegin.Append(fontSize);
                sbBegin.Append(DoubleQuoteChar);
            }
            sbBegin.Append(TagRightChar);

            // insert font end tag
            sbEnd.Insert(0,EndTagLeftChars + "font" + TagRightChar);
        }

        private void AppendOtherTag(string tag) {
            if (Supports(FONT_AROUND_CONTENT))
                AppendOtherTag(tag,_beforeContent,_afterContent);
            else
                AppendOtherTag(tag,_beforeTag,_afterTag);
        }

        private void AppendOtherTag(string tag,StringBuilder sbBegin,StringBuilder sbEnd) {
            // append begin tag
            sbBegin.Append(TagLeftChar);
            sbBegin.Append(tag);
            sbBegin.Append(TagRightChar);

            // insert end tag
            sbEnd.Insert(0,EndTagLeftChars + tag + TagRightChar);
        }

        private void AppendOtherTag(string tag, object[] attribs, StringBuilder sbBegin, StringBuilder sbEnd) {
            // append begin tag
            sbBegin.Append(TagLeftChar);
            sbBegin.Append(tag);
            for (int i = 0; i < attribs.Length; i++) {
                sbBegin.Append(SpaceChar);
                sbBegin.Append(((string[])attribs[i])[0]);
                sbBegin.Append(EqualsDoubleQuoteString);
                sbBegin.Append(((string[])attribs[i])[1]);
                sbBegin.Append(DoubleQuoteChar);
            }
            sbBegin.Append(TagRightChar);

            // insert end tag
            sbEnd.Insert(0,EndTagLeftChars + tag + TagRightChar);
        }

        private void ConsumeFont(StringBuilder sbBegin, StringBuilder sbEnd) {
            
            if (FontStack.Count > 0) {

                string fontFace = null;
                string fontColor = null;
                string fontSize = null;
                bool underline = false;
                bool italic = false;
                bool bold = false;
                bool strikeout = false;

                IEnumerator e = FontStack.GetEnumerator();
                while(e.MoveNext()) {
                    FontStackItem fontInfo = (FontStackItem) e.Current;
                    if (fontFace == null) {
                        fontFace = fontInfo.name;
                    }
                    if (fontColor == null) {
                        fontColor = fontInfo.color;
                    }
                    if (fontSize == null) {
                        fontSize = fontInfo.size;
                    }
                    if (!underline) {
                        underline = fontInfo.underline;
                    }
                    if (!italic) {
                        italic = fontInfo.italic;
                    }
                    if (!bold) {
                        bold = fontInfo.bold;
                    }
                    if (!strikeout) {
                        strikeout = fontInfo.strikeout;
                    }
                }
                
                if ((fontFace != null) || (fontColor != null) || (fontSize != null)) {
                    AppendFontTag(fontFace, fontColor, fontSize, sbBegin, sbEnd);
                }
                if (underline) {
                    AppendOtherTag("u", sbBegin, sbEnd);
                }
                if (italic && SupportsItalic) {
                    AppendOtherTag("i", sbBegin, sbEnd);
                }
                if (bold && SupportsBold) {
                    AppendOtherTag("b", sbBegin, sbEnd);
                }
                if (strikeout) {
                    AppendOtherTag("strike", sbBegin, sbEnd);                    
                }

            }
        }

        private string ConvertToHtmlFontSize(string value) {
            FontUnit fu = new FontUnit(value, CultureInfo.InvariantCulture);
            if ((int)(fu.Type) > 3)
                return((int)(fu.Type)-3).ToString(CultureInfo.InvariantCulture);

            if (fu.Type == FontSize.AsUnit) {
                if (fu.Unit.Type == UnitType.Point) {
                    if (fu.Unit.Value <= 8)
                        return "1";
                    else if (fu.Unit.Value <= 10)
                        return "2";
                    else if (fu.Unit.Value <= 12)
                        return "3";
                    else if (fu.Unit.Value <= 14)
                        return "4";
                    else if (fu.Unit.Value <= 18)
                        return "5";
                    else if (fu.Unit.Value <= 24)
                        return "6";
                    else
                        return "7";
                }
            }

            return null;
        }

        private string ConvertToHtmlSize(string value) {
            Unit u = new Unit(value, CultureInfo.InvariantCulture);
            if (u.Type == UnitType.Pixel) {
                return u.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (u.Type == UnitType.Percentage) {
                return value;
            }
            return null;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override bool OnStyleAttributeRender(string name,string value, HtmlTextWriterStyle key) {

            string s;
            if (Supports(FONT_AROUND_CONTENT)) {
                // tag supports downlevel fonts
                switch (key) {
                    case HtmlTextWriterStyle.FontFamily:
                        _fontFace = value;
                        _renderFontTag = true;
                        break;
                    case HtmlTextWriterStyle.Color:
                        _fontColor = value;
                        _renderFontTag = true;
                        break;
                    case HtmlTextWriterStyle.FontSize:
                        _fontSize = ConvertToHtmlFontSize(value);
                        if (_fontSize != null)
                            _renderFontTag = true;
                        break;
                    case HtmlTextWriterStyle.FontWeight:
                        if (StringUtil.EqualsIgnoreCase(value, "bold") && SupportsBold) {
                            AppendOtherTag("b");
                        }
                        break;
                    case HtmlTextWriterStyle.FontStyle:
                        if (!StringUtil.EqualsIgnoreCase(value, "normal") && SupportsItalic) {
                            AppendOtherTag("i");
                        }
                        break;
                    case HtmlTextWriterStyle.TextDecoration:
                        s = value.ToLower(CultureInfo.InvariantCulture);
                        if (s.IndexOf("underline", StringComparison.Ordinal) != -1) {
                            AppendOtherTag("u");
                        }
                        if (s.IndexOf("line-through", StringComparison.Ordinal) != -1) {
                            AppendOtherTag("strike");
                        }
                        break;
                }
            }
            else if (Supports(FONT_PROPAGATE)) {
                FontStackItem font = (FontStackItem)FontStack.Peek();

                switch (key) {
                    case HtmlTextWriterStyle.FontFamily:
                        font.name = value;
                        break;
                    case HtmlTextWriterStyle.Color:
                        font.color = value;
                        break;
                    case HtmlTextWriterStyle.FontSize:
                        font.size = ConvertToHtmlFontSize(value);
                        break;
                    case HtmlTextWriterStyle.FontWeight:
                        if (StringUtil.EqualsIgnoreCase(value, "bold")) {
                            font.bold = true;
                        }
                        break;
                    case HtmlTextWriterStyle.FontStyle:
                        if (!StringUtil.EqualsIgnoreCase(value, "normal")) {
                            font.italic = true;
                        }
                        break;
                    case HtmlTextWriterStyle.TextDecoration:
                        s = value.ToLower(CultureInfo.InvariantCulture);
                        if (s.IndexOf("underline", StringComparison.Ordinal) != -1) {
                            font.underline = true;
                        }
                        if (s.IndexOf("line-through", StringComparison.Ordinal) != -1) {
                            font.strikeout = true;
                        }
                        break;
                }
            }

            if (Supports(SUPPORTS_BORDER) && key == HtmlTextWriterStyle.BorderWidth) {
                s = ConvertToHtmlSize(value);
                if (s != null)
                    AddAttribute(HtmlTextWriterAttribute.Border,s);
            }

            if (Supports(SUPPORTS_NOWRAP) && key == HtmlTextWriterStyle.WhiteSpace) {
                AddAttribute(HtmlTextWriterAttribute.Nowrap, value);
            }

            if (Supports(SUPPORTS_HEIGHT_WIDTH)) {
                switch (key) {
                    case HtmlTextWriterStyle.Height :
                        s = ConvertToHtmlSize(value);
                        if (s != null)
                            AddAttribute(HtmlTextWriterAttribute.Height,s);
                        break;
                    case HtmlTextWriterStyle.Width :
                        s = ConvertToHtmlSize(value);
                        if (s != null)
                            AddAttribute(HtmlTextWriterAttribute.Width,s);
                        break;
                }
            }

            if (Supports(TABLE_ATTRIBUTES) || Supports(TABLE_AROUND_CONTENT)) {
                // tag supports downlevel table attributes
                switch (key) {
                    case HtmlTextWriterStyle.BorderColor:
                        switch (TagKey) {
                            case HtmlTextWriterTag.Div:
                                if (ShouldPerformDivTableSubstitution) {
                                    AddAttribute(HtmlTextWriterAttribute.Bordercolor, value);
                                }
                                break;
                        }
                        break;
                    case HtmlTextWriterStyle.BackgroundColor :
                        switch (TagKey) {
                            case HtmlTextWriterTag.Table :
                            case HtmlTextWriterTag.Tr :
                            case HtmlTextWriterTag.Td :
                            case HtmlTextWriterTag.Th :
                            case HtmlTextWriterTag.Body :
                                AddAttribute(HtmlTextWriterAttribute.Bgcolor,value);
                                break;
                                // div->table substitution.
                            case HtmlTextWriterTag.Div:
                                if (ShouldPerformDivTableSubstitution) {
                                    AddAttribute(HtmlTextWriterAttribute.Bgcolor,value);
                                }
                                break;
                        }
                        break;
                    case HtmlTextWriterStyle.BackgroundImage :
                        switch (TagKey) {
                            case HtmlTextWriterTag.Table :
                            case HtmlTextWriterTag.Td :
                            case HtmlTextWriterTag.Th :
                            case HtmlTextWriterTag.Body :
                                // strip url(...) from value
                                if (StringUtil.StringStartsWith(value, "url("))
                                    value = value.Substring(4,value.Length-5);
                                AddAttribute(HtmlTextWriterAttribute.Background,value);
                                break;
                                // div->table substitution.
                            case HtmlTextWriterTag.Div:
                                if (ShouldPerformDivTableSubstitution) {
                                    if (StringUtil.StringStartsWith(value, "url("))
                                        value = value.Substring(4,value.Length-5);
                                    AddAttribute(HtmlTextWriterAttribute.Background,value);
                                }
                                break;
                        }
                        break;
                }
            }

            switch (key) {
                case HtmlTextWriterStyle.ListStyleType:
                    switch (value) {
                        case "decimal":
                            AddAttribute(HtmlTextWriterAttribute.Type, "1");
                            break;
                        case "lower-alpha":
                            AddAttribute(HtmlTextWriterAttribute.Type, "a");
                            break;
                        case "upper-alpha":
                            AddAttribute(HtmlTextWriterAttribute.Type, "A");
                            break;
                        case "lower-roman":
                            AddAttribute(HtmlTextWriterAttribute.Type, "i");
                            break;
                        case "upper-roman":
                            AddAttribute(HtmlTextWriterAttribute.Type, "I");
                            break;
                        case "disc":
                        case "circle":
                        case "square":
                            AddAttribute(HtmlTextWriterAttribute.Type, value);
                            break;
                        default:
                            AddAttribute(HtmlTextWriterAttribute.Type, "disc");
                            Debug.Assert(false, "Invalid BulletStyle for HTML32.");
                            break;
                    }
                    break;
                case HtmlTextWriterStyle.TextAlign:
                    AddAttribute(HtmlTextWriterAttribute.Align, value);
                    break;
                case HtmlTextWriterStyle.VerticalAlign:
                    AddAttribute(HtmlTextWriterAttribute.Valign, value);
                    break;
                // Netscape 4.72 can properly handle the Display style attribute, so allow it
                // to be rendered.
                case HtmlTextWriterStyle.Display:
                    return true;
            }

            return false;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override bool OnTagRender(string name, HtmlTextWriterTag key) {
            // handle any tags that do not work downlevel

            SetTagSupports();
            if (Supports(FONT_PROPAGATE)) {
                FontStack.Push(new FontStackItem());
            }

            // div->table substitution.
            // Make tag look like a table. This must be done after we establish tag support.
            if (key == HtmlTextWriterTag.Div && ShouldPerformDivTableSubstitution) {
                TagKey = HtmlTextWriterTag.Table;
            }

            return base.OnTagRender(name,key);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetTagName(HtmlTextWriterTag tagKey) {
            // div->table substitution.
            if (tagKey == HtmlTextWriterTag.Div && ShouldPerformDivTableSubstitution) {
                return "table";
            }
            return base.GetTagName(tagKey);
        }



        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void RenderBeginTag(HtmlTextWriterTag tagKey) {
            // flush string buffers to build new tag
            _beforeTag.Length = 0;
            _beforeContent.Length = 0;
            _afterContent.Length = 0;
            _afterTag.Length = 0;

            _renderFontTag = false;
            _fontFace = null;
            _fontColor = null;
            _fontSize = null;

            // div->table substitution.
            if (ShouldPerformDivTableSubstitution) {
                if (tagKey == HtmlTextWriterTag.Div) {
                    AppendOtherTag("tr", _beforeContent, _afterContent);

                    string alignment;
                    if (IsAttributeDefined(HtmlTextWriterAttribute.Align, out alignment)) {
                        string[] attribs = new string[] { GetAttributeName(HtmlTextWriterAttribute.Align), alignment};

                        AppendOtherTag("td", new object[]{ attribs}, _beforeContent, _afterContent);
                    }
                    else {
                        AppendOtherTag("td", _beforeContent, _afterContent);
                    }
                    if (!IsAttributeDefined(HtmlTextWriterAttribute.Cellpadding)) {
                        AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                    }
                    if (!IsAttributeDefined(HtmlTextWriterAttribute.Cellspacing)) {
                        AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    }
                    if (!IsStyleAttributeDefined(HtmlTextWriterStyle.BorderWidth)) {
                        AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    }
                    if (!IsStyleAttributeDefined(HtmlTextWriterStyle.Width)) {
                        AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                    }
                }
            }

            base.RenderBeginTag(tagKey);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string RenderBeforeTag() {
            if (_renderFontTag && Supports(FONT_AROUND_TAG))
                AppendFontTag(_beforeTag,_afterTag);

            if (_beforeTag.Length > 0)
                return(_beforeTag.ToString());

            return base.RenderBeforeTag();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string RenderBeforeContent() {

            if (Supports(FONT_CONSUME)) {
                ConsumeFont(_beforeContent, _afterContent);
            }
            else if (_renderFontTag && Supports(FONT_AROUND_CONTENT)) {
                AppendFontTag(_beforeContent,_afterContent);
            }

            if (_beforeContent.Length > 0)
                return(_beforeContent.ToString());

            return base.RenderBeforeContent();

        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string RenderAfterContent() {
            if (_afterContent.Length > 0)
                return(_afterContent.ToString());

            return base.RenderAfterContent();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string RenderAfterTag() {
            if (_afterTag.Length > 0)
                return(_afterTag.ToString());

            return base.RenderAfterTag();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void RenderEndTag() {
            base.RenderEndTag();

            SetTagSupports();
            if (Supports(FONT_PROPAGATE)) {
                FontStack.Pop();
            }
        }

        private void SetTagSupports() {
            // determine what downlevel tag supports
            _tagSupports = NOTHING;
            switch (TagKey) {
                case HtmlTextWriterTag.A :
                case HtmlTextWriterTag.Label :
                case HtmlTextWriterTag.P :
                case HtmlTextWriterTag.Span :
                    _tagSupports |= FONT_AROUND_CONTENT;
                    break;
                case HtmlTextWriterTag.Li:
                    _tagSupports |= FONT_AROUND_CONTENT | FONT_CONSUME;
                    break;
                case HtmlTextWriterTag.Div :
                    _tagSupports |= FONT_AROUND_CONTENT | FONT_PROPAGATE;
                    break;
                case HtmlTextWriterTag.Ul:
                case HtmlTextWriterTag.Ol:
                case HtmlTextWriterTag.Table:
                case HtmlTextWriterTag.Tr:
                    _tagSupports |= FONT_PROPAGATE;
                    break;
                case HtmlTextWriterTag.Td :
                case HtmlTextWriterTag.Th :
                    _tagSupports |= FONT_PROPAGATE | FONT_CONSUME;
                    break;
                case HtmlTextWriterTag.Input :
                    _tagSupports |= SUPPORTS_BORDER;
                    break;
            }

            switch (TagKey) {
                // div->table substitution.
                case HtmlTextWriterTag.Div:
                    if (ShouldPerformDivTableSubstitution) {
                        _tagSupports |= SUPPORTS_HEIGHT_WIDTH | SUPPORTS_BORDER;
                    }
                    _tagSupports |= SUPPORTS_NOWRAP;
                    break;
                case HtmlTextWriterTag.Img:
                    _tagSupports |= SUPPORTS_HEIGHT_WIDTH | SUPPORTS_BORDER;
                    break;
                case HtmlTextWriterTag.Table:
                    _tagSupports |= SUPPORTS_HEIGHT_WIDTH;
                    break;
                case HtmlTextWriterTag.Th:
                case HtmlTextWriterTag.Td:
                    _tagSupports |= SUPPORTS_NOWRAP | SUPPORTS_HEIGHT_WIDTH;
                    break;
            }

            //switch (TagKey) {
            //    case HtmlTextWriterTag.INPUT :
            //        _tagSupports |= FONT_AROUND_TAG;
            //        break;
            //}

            switch (TagKey) {
                case HtmlTextWriterTag.Table :
                case HtmlTextWriterTag.Tr :
                case HtmlTextWriterTag.Td :
                case HtmlTextWriterTag.Th :
                case HtmlTextWriterTag.Body :
                    _tagSupports |= TABLE_ATTRIBUTES;
                    break;
            }
            switch (TagKey) {
                // div->table substitution.
                case HtmlTextWriterTag.Div :
                    if (ShouldPerformDivTableSubstitution) {
                        _tagSupports |= TABLE_AROUND_CONTENT;
                    }
                    break;
            }
        }

        private bool Supports(int flag) {
            return(_tagSupports & flag) == flag;
        }



        /// <devdoc>
        ///   Contains information about a font placed on the stack of font information.
        /// </devdoc>
        private sealed class FontStackItem {
            public string name;
            public string color;
            public string size;
            public bool bold;
            public bool italic;
            public bool underline;
            public bool strikeout;
        }
    }
}
