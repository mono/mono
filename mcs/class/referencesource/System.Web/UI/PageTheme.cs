//------------------------------------------------------------------------------
// <copyright file="PageTheme.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections; 
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI.HtmlControls;
    using System.Web.Util;
    using System.Xml;
    using System.Security.Permissions;

    internal class FileLevelPageThemeBuilder : RootBuilder {

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void AppendLiteralString(string s) {
            // Don't allow any literal contents at theme top level
            if (s != null) {
                if (!Util.IsWhiteSpaceString(s)) {
                    throw new HttpException(SR.GetString(SR.Literal_content_not_allowed, SR.GetString(SR.Page_theme_skin_file), s.Trim()));
                }
            }

            base.AppendLiteralString(s);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void AppendSubBuilder(ControlBuilder subBuilder) {
            // Only allow controls at theme top level
            Type ctrlType = subBuilder.ControlType;
            if (!typeof(Control).IsAssignableFrom(ctrlType)) {
                throw new HttpException(SR.GetString(SR.Page_theme_only_controls_allowed, ctrlType == null ? 
                    String.Empty : ctrlType.ToString()));
            }

            // Check if the control theme type is themeable.
            if (InPageTheme && !ThemeableAttribute.IsTypeThemeable(subBuilder.ControlType)) {
                throw new HttpParseException(SR.GetString(SR.Type_theme_disabled, subBuilder.ControlType.FullName),
                    null, subBuilder.VirtualPath, null, subBuilder.Line);
            }

            base.AppendSubBuilder(subBuilder);
        }    
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public abstract class PageTheme {

        private Page _page;
        private bool _styleSheetTheme;

        protected abstract String[] LinkedStyleSheets { get; }

        protected abstract IDictionary ControlSkins { get; }

        protected abstract String AppRelativeTemplateSourceDirectory { get; }

        protected Page Page {
            get {
                return _page;
            }
        }

        internal void Initialize(Page page, bool styleSheetTheme) {
            Debug.Assert(page != null);
            _page = page;
            _styleSheetTheme = styleSheetTheme;
        }

        protected object Eval(string expression) {
            return Page.Eval(expression);
        }

        protected string Eval(string expression, string format) {
            return Page.Eval(expression, format);
        }

        public static object CreateSkinKey(Type controlType, String skinID) {
            if (controlType == null) {
                throw new ArgumentNullException("controlType");
            }

            return new SkinKey(controlType.ToString(), skinID);
        }

        internal void ApplyControlSkin(Control control) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }

            ControlSkin skin = null;
            String skinId = control.SkinID;
            skin = (ControlSkin)ControlSkins[CreateSkinKey(control.GetType(), skinId)];

            // Don't throw if ControlSkin corresponds to the skinID does not exist.
            Debug.Assert(skin == null || skin.ControlType == control.GetType());

            if (skin != null) {
                skin.ApplySkin(control);
            }
        }

        internal void SetStyleSheet() {
            if (LinkedStyleSheets != null && LinkedStyleSheets.Length > 0) {
                if (Page.Header == null)
                    throw new InvalidOperationException(SR.GetString(SR.Page_theme_requires_page_header));

                int index = 0;
                foreach(string styleSheetPath in LinkedStyleSheets) {
                    HtmlLink link = new HtmlLink();
                    link.Href = styleSheetPath;
                    link.Attributes["type"] = "text/css";
                    link.Attributes["rel"] = "stylesheet";

                    if (_styleSheetTheme) {
                        Page.Header.Controls.AddAt(index ++, link);
                    }
                    else {
                        Page.Header.Controls.Add(link);
                    }
                }
            }
        }

        public bool TestDeviceFilter(string deviceFilterName) {
            return Page.TestDeviceFilter(deviceFilterName);
        }

        protected object XPath(string xPathExpression) {
            return Page.XPath(xPathExpression);
        }

        protected object XPath(string xPathExpression, IXmlNamespaceResolver resolver) {
            return Page.XPath(xPathExpression, resolver);
        }

        protected string XPath(string xPathExpression, string format) {
            return Page.XPath(xPathExpression, format);
        }

        protected string XPath(string xPathExpression, string format, IXmlNamespaceResolver resolver) {
            return Page.XPath(xPathExpression, format, resolver);
        }

        protected IEnumerable XPathSelect(string xPathExpression) {
            return Page.XPathSelect(xPathExpression);
        }

        protected IEnumerable XPathSelect(string xPathExpression, IXmlNamespaceResolver resolver) {
            return Page.XPathSelect(xPathExpression, resolver);
        }
        
        private class SkinKey {
            private string _skinID;
            private string _typeName;

            internal SkinKey(string typeName, string skinID) {
                _typeName = typeName;

                if (String.IsNullOrEmpty(skinID)) {
                    _skinID = null;
                }
                else {
                    _skinID = skinID.ToLower(CultureInfo.InvariantCulture);
                }
            }

            public override int GetHashCode() {
                if (_skinID == null) {
                   return _typeName.GetHashCode();
                }

                return HashCodeCombiner.CombineHashCodes(_typeName.GetHashCode(), _skinID.GetHashCode());
            }

            public override bool Equals(object o) {
                SkinKey key = (SkinKey)o;

                return (_typeName == key._typeName) &&
                    (_skinID == key._skinID);
            }
        }
    }
}
