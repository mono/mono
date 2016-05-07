//------------------------------------------------------------------------------
// <copyright file="HtmlHead.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Security.Permissions;

    public class HtmlHeadBuilder : ControlBuilder {


        public override Type GetChildControlType(string tagName, IDictionary attribs) {
            if (String.Equals(tagName, "title", StringComparison.OrdinalIgnoreCase))
                return typeof(HtmlTitle);

            if (String.Equals(tagName, "link", StringComparison.OrdinalIgnoreCase))
                return typeof(HtmlLink);

            if (String.Equals(tagName, "meta", StringComparison.OrdinalIgnoreCase))
                return typeof(HtmlMeta);

            return null;
        }


        public override bool AllowWhitespaceLiterals() {
            return false;
        }
    }

    /// <devdoc>
    /// Represents the HEAD element.
    /// </devdoc>
    [
    ControlBuilderAttribute(typeof(HtmlHeadBuilder))
    ]
    public sealed class HtmlHead : HtmlGenericControl {

        private StyleSheetInternal _styleSheet;
        private HtmlTitle _title;
        private String _cachedTitleText;
        private HtmlMeta _description;
        private String _cachedDescription;
        private HtmlMeta _keywords;
        private String _cachedKeywords;

        /// <devdoc>
        /// Initializes an instance of an HtmlHead class.
        /// </devdoc>
        public HtmlHead() : base("head") {
        }

        public HtmlHead(string tag) : base(tag) {
            if (tag == null) {
                tag = String.Empty;
            }
            _tagName = tag;
        }

        public IStyleSheet StyleSheet {
            get {
                if (_styleSheet == null) {
                    _styleSheet = new StyleSheetInternal(this);
                }

                return _styleSheet;
            }
        }

        public String Title {
            get {
                if (_title == null) {
                    return _cachedTitleText;
                }

                return _title.Text;
            }
            set {
                if (_title == null) {
                    // Side effect of adding a title to the control assigns _title
                    _cachedTitleText = value;
                }
                else {
                    _title.Text = value;
                }
            }
        }

        public String Description {
            get {
                if (_description == null) {
                    return _cachedDescription;
                }

                return _description.Content;
            }
            set {
                if (_description == null) {
                    // Side effect of adding a description to the control assigns _description
                    _cachedDescription = value;
                }
                else {
                    _description.Content = value;
                }
            }
        }

        public String Keywords {
            get {
                if (_keywords == null) {
                    return _cachedKeywords;
                }

                return _keywords.Content;
            }
            set {
                if (_keywords == null) {
                    // Side effect of adding a title to the control assigns _title
                    _cachedKeywords = value;
                }
                else {
                    _keywords.Content = value;
                }
            }
        }

        protected internal override void AddedControl(Control control, int index) {
            base.AddedControl(control, index);

            if (control is HtmlTitle) {
                if (_title != null) {
                    throw new HttpException(SR.GetString(SR.HtmlHead_OnlyOneTitleAllowed));
                }

                _title = (HtmlTitle)control;
            }
            else if (control is HtmlMeta) {
                // We will only use the first matching meta tag per, and ignore any others
                HtmlMeta meta = (HtmlMeta)control;
                if (_description == null && string.Equals(meta.Name, "description", StringComparison.OrdinalIgnoreCase)) {
                    _description = meta;
                }
                else if (_keywords == null && string.Equals(meta.Name, "keywords", StringComparison.OrdinalIgnoreCase)) {
                    _keywords = meta;
                }
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Allows the HEAD element to register itself with the page.
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            Page p = Page;
            if (p == null) {
                throw new HttpException(SR.GetString(SR.Head_Needs_Page));
            }
            if (p.Header != null) {
                throw new HttpException(SR.GetString(SR.HtmlHead_OnlyOneHeadAllowed));
            }
            p.SetHeader(this);
        }

        internal void RegisterCssStyleString(string outputString) {
            ((StyleSheetInternal)StyleSheet).CSSStyleString = outputString;
        }

        protected internal override void RemovedControl(Control control) {
            base.RemovedControl(control);

            if (control is HtmlTitle) {
                _title = null;
            }
            // There can be many meta tags, so we only clear it if its the correct meta
            else if (control == _description) {
                _description = null;
            }
            else if (control == _keywords) {
                _keywords = null;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Notifies the Page when the HEAD is being rendered.
        /// </devdoc>
        protected internal override void RenderChildren(HtmlTextWriter writer) {
            base.RenderChildren(writer);

            if (_title == null) {
                // Always render out a <title> tag since it is required for xhtml 1.1 compliance
                writer.RenderBeginTag(HtmlTextWriterTag.Title);
                if (_cachedTitleText != null) {
                    writer.Write(_cachedTitleText);
                }
                writer.RenderEndTag();
            }

            if (_description == null && !String.IsNullOrEmpty(_cachedDescription)) {
                // Go ahead and render out a meta tag if they set description but don't have a meta tag
                writer.AddAttribute(HtmlTextWriterAttribute.Name, "description");
                writer.AddAttribute(HtmlTextWriterAttribute.Content, _cachedDescription);
                writer.RenderBeginTag(HtmlTextWriterTag.Meta);
                writer.RenderEndTag();
            }

            if (_keywords == null && !String.IsNullOrEmpty(_cachedKeywords)) {
                // Go ahead and render out a meta tag if they set keywords but don't have a meta tag
                writer.AddAttribute(HtmlTextWriterAttribute.Name, "keywords");
                writer.AddAttribute(HtmlTextWriterAttribute.Content, _cachedKeywords);
                writer.RenderBeginTag(HtmlTextWriterTag.Meta);
                writer.RenderEndTag();
            }

            if ((string)Page.Request.Browser["requiresXhtmlCssSuppression"] != "true") {
                RenderStyleSheet(writer);
            }
        }

        internal void RenderStyleSheet(HtmlTextWriter writer) {
            if(_styleSheet != null) {
                _styleSheet.Render(writer);
            }
        }

        internal static void RenderCssRule(CssTextWriter cssWriter, string selector,
            Style style, IUrlResolutionService urlResolver) {

            cssWriter.WriteBeginCssRule(selector);

            CssStyleCollection attrs = style.GetStyleAttributes(urlResolver);
            attrs.Render(cssWriter);

            cssWriter.WriteEndCssRule();
        }

        /// <devdoc>
        /// Implements the IStyleSheet interface to represent an embedded
        /// style sheet within the HEAD element.
        /// </devdoc>
        private sealed class StyleSheetInternal : IStyleSheet, IUrlResolutionService {

            private HtmlHead _owner;
            private ArrayList _styles;
            private ArrayList _selectorStyles;

            private int _autoGenCount;

            public StyleSheetInternal(HtmlHead owner) {
                _owner = owner;
            }

            // CssStyleString registered by the PartialCachingControl
            private string _cssStyleString;
            internal string CSSStyleString {
                get {
                    return _cssStyleString;
                }
                set {
                    _cssStyleString = value;
                }
            }

            public void Render(HtmlTextWriter writer) {
                if ((_styles == null) && (_selectorStyles == null) && CSSStyleString == null) {
                    return;
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.RenderBeginTag(HtmlTextWriterTag.Style);

                CssTextWriter cssWriter = new CssTextWriter(writer);
                if (_styles != null) {
                    for (int i = 0; i < _styles.Count; i++) {
                        StyleInfo si = (StyleInfo)_styles[i];

                        string cssClass = si.style.RegisteredCssClass;
                        if (cssClass.Length != 0) {
                            RenderCssRule(cssWriter, "." + cssClass, si.style, si.urlResolver);
                        }
                    }
                }

                if (_selectorStyles != null) {
                    for (int i = 0; i < _selectorStyles.Count; i++) {
                        SelectorStyleInfo si = (SelectorStyleInfo)_selectorStyles[i];
                        RenderCssRule(cssWriter, si.selector, si.style, si.urlResolver);
                    }
                }

                if (CSSStyleString != null) {
                    writer.Write(CSSStyleString);
                }

                writer.RenderEndTag();
            }

            #region Implementation of IStyleSheet
            void IStyleSheet.CreateStyleRule(Style style, IUrlResolutionService urlResolver, string selector) {
                if (style == null) {
                    throw new ArgumentNullException("style");
                }

                if (selector.Length == 0) {
                    throw new ArgumentNullException("selector");
                }

                if (_selectorStyles == null) {
                    _selectorStyles = new ArrayList();
                }

                if (urlResolver == null) {
                    urlResolver = this;
                }

                SelectorStyleInfo styleInfo = new SelectorStyleInfo();
                styleInfo.selector = selector;
                styleInfo.style = style;
                styleInfo.urlResolver = urlResolver;

                _selectorStyles.Add(styleInfo);

                Page page = _owner.Page;

                // If there are any partial caching controls on the stack, forward the styleInfo to them
                if (page.PartialCachingControlStack != null) {
                    foreach (BasePartialCachingControl c in page.PartialCachingControlStack) {
                        c.RegisterStyleInfo(styleInfo);
                    }
                }
            }

            void IStyleSheet.RegisterStyle(Style style, IUrlResolutionService urlResolver) {
                if (style == null) {
                    throw new ArgumentNullException("style");
                }

                if (_styles == null) {
                    _styles = new ArrayList();
                }
                else if (style.RegisteredCssClass.Length != 0) {
                    // if it's already registered, throw an exception
                    throw new InvalidOperationException(SR.GetString(SR.HtmlHead_StyleAlreadyRegistered));
                }

                if (urlResolver == null) {
                    urlResolver = this;
                }

                StyleInfo styleInfo = new StyleInfo();
                styleInfo.style = style;
                styleInfo.urlResolver = urlResolver;

                int index = _autoGenCount++;
                string name = "aspnet_s" + index.ToString(NumberFormatInfo.InvariantInfo);

                style.SetRegisteredCssClass(name);
                _styles.Add(styleInfo);
            }
            #endregion

            #region Implementation of IUrlResolutionService
            string IUrlResolutionService.ResolveClientUrl(string relativeUrl) {
                return _owner.ResolveClientUrl(relativeUrl);
            }
            #endregion

            private sealed class StyleInfo {
                public Style style;
                public IUrlResolutionService urlResolver;
            }
        }
    }

    internal sealed class SelectorStyleInfo {
        public string selector;
        public Style style;
        public IUrlResolutionService urlResolver;
    }
}
