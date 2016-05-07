//------------------------------------------------------------------------------
// <copyright file="XhtmlMobileTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Security.Permissions;
using System.Web;
using System.Web.Caching;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.SessionState;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{
    /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlMobileTextWriter : MobileTextWriter {

        private int _styleCount = 0;
        private static readonly Style _defaultStyle = new Style ();
        private static readonly XhtmlStyleClass _defaultStyleClass = new XhtmlStyleClass(_defaultStyle, XhtmlConstants.All);
        // For certain WML 2.0 devices, we have to render a WML onevent element.  
        // Therefore we need two markup builders -a preWmlOnEventLocation and a postWmlOnEventLocation.
        // Calling MarkWmlOnEventLocation switches the markup string builder from the pre to the post
        // builder.  Everything is concatenated in order in EndCachedRendering / WriteCachedMarkup.
        private XhtmlStyleClass _bodyStyle = null;
        private TextWriter _cachedInnerWriter = null;
        private String _cacheKey;
        private String _cachedMarkup;
        private string _cachedEndTag = null;
        private bool _cacheKeyValid = false;
        private bool _cachingRendering;
        private string _customBodyStyles = null;
        private IDictionary _doctypeDeclarations = new Hashtable ();
        private bool _isStyleSheetEmpty = true;
        private ArrayList _orderedStyleClassKeys = new ArrayList ();
        private bool _pendingBreak = false;
        private Stack _physicalCssClasses = new Stack();
        private StringBuilder _preWmlOnEventMarkupBuilder = new StringBuilder();
        private StringBuilder _postWmlOnEventMarkupBuilder = new StringBuilder();
        private string _sessionKey;
        private bool _sessionKeyValid = false;
        private IDictionary _styleHash = new Hashtable ();
        private bool _supportsNoWrapStyle = true;
        private bool _suppressNewLines = false;
        private ArrayList _wmlOnEnterForwardVarNames = new ArrayList();
        private bool _useDivsForBreaks = false;

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.XhtmlMobileTextWriter"]/*' />
        public XhtmlMobileTextWriter (TextWriter writer, MobileCapabilities device) : base(writer, device) {
            _doctypeDeclarations[Doctype.XhtmlBasic] = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML Basic 1.0//EN\" \"http://www.w3.org/TR/xhtml-basic/xhtml-basic10.dtd\">";
            _doctypeDeclarations[Doctype.XhtmlMobileProfile] = "<!DOCTYPE html PUBLIC \"-//WAPFORUM//DTD XHTML Mobile 1.0//EN\" \"http://www.wapforum.org/DTD/xhtml-mobile10.dtd\">"; 
            _doctypeDeclarations[Doctype.Wml20] = "<!DOCTYPE html PUBLIC \"-//WAPFORUM//DTD WML 2.0//EN\" \"http://www.wapforum.org/dtd/wml20.dtd\" >";
        }

        internal String CachedEndTag {
            get {
                return _cachedEndTag;
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.CacheKey"]/*' />
        public virtual String CacheKey {
            get {
                // Exception is for extensibility scenarios.
                if (!_cacheKeyValid) {
                    throw new Exception (SR.GetString(
                        SR.XhtmlMobileTextWriter_CacheKeyNotSet));
                }
                return _cacheKey;
            }
        }

#if UNUSED_CODE
        internal bool CssClassOnStack {
            get {
                return _physicalCssClasses.Count > 0;
            }
        }
#endif

        // Saves a couple of lines of code in most cases. 
        internal XhtmlStyleClass CurrentStyleClass {
            get {
                if (_styleStack.Count > 0) {
                    StylePair pair = (StylePair) _styleStack.Peek ();
                    return pair.Class;
                }
                else {
                    // If the style stack is empty, the current style is default.
                    return _defaultStyleClass;
                }
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.CustomBodyStyles"]/*' />
        public String CustomBodyStyles {
            get {
                return _customBodyStyles;
            }
            set {
                _isStyleSheetEmpty = false;
                _customBodyStyles = value;
            }
        }
        
        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.SessionKey"]/*' />
        public virtual String SessionKey {
            // Exception is for extensibility scenarios.
            get {
                if (!_sessionKeyValid) {
                    throw new Exception (SR.GetString(
                        SR.XhtmlMobileTextWriter_SessionKeyNotSet));
                }
                return _sessionKey;
            }
        }

        public virtual bool SupportsNoWrapStyle {
            get {
                return _supportsNoWrapStyle;
            }
            set {
                _supportsNoWrapStyle = value;
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.SuppressNewLine"]/*' />
        public bool SuppressNewLine {
            get {
                return _suppressNewLines;
            }
            set {
                _suppressNewLines = value;
            }

        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.UseDivsForBreaks"]/*' />
        public bool UseDivsForBreaks {
            get {
                return _useDivsForBreaks;
            }
            set {
                _useDivsForBreaks = value;
            }
        }

        // Add a variable name to clear.
        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.AddOnEnterForwardSetVar"]/*' />
        public virtual void AddOnEnterForwardSetVar(String var) {
            AddOnEnterForwardSetVar(var, String.Empty);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.AddOnEnterForwardSetVar1"]/*' />
        public virtual void AddOnEnterForwardSetVar(String var, String value) {
            _wmlOnEnterForwardVarNames.Add(new String[2]{var, value});
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.BeginCachedRendering"]/*' />
        public virtual void BeginCachedRendering () {
            _cachedInnerWriter = InnerWriter;
            InnerWriter = new StringWriter (_preWmlOnEventMarkupBuilder, CultureInfo.InvariantCulture);
            _cachingRendering = true;
        }

        internal void ClearCachedEndTag() {
            _cachedEndTag = null;
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.ClearPendingBreak"]/*' />
        public virtual void ClearPendingBreak() {
            _pendingBreak = false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Optimization for physical stylesheets.  We only write the cssClass attribute if it differs
        // (case-sensitive) from the current cssClass.  The current cssClass is kept in a stack.
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal bool DiffersFromCurrentPhysicalCssClass(String cssClass) {
            // Case sensitive comparison.
            return _physicalCssClasses.Count == 0 || 
                String.Compare(cssClass, (String)_physicalCssClasses.Peek(), StringComparison.Ordinal) != 0;
        }

        private void EncodeAttributeValue(String value, StringBuilder encodedValue) {
            StringWriter writer = new StringWriter(encodedValue, CultureInfo.InvariantCulture);
            HttpUtility.HtmlEncode(value, writer);
        }

        // Expose base method to xhtml control adapter.
        internal string EncodeUrlInternal(String url) {
            return base.EncodeUrl(url);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.EndCachedRendering"]/*' />
        public virtual void EndCachedRendering () {
            StringBuilder cachedMarkupBuilder = new StringBuilder();
            cachedMarkupBuilder.Append(_preWmlOnEventMarkupBuilder.ToString());
            cachedMarkupBuilder.Append(GetWmlOnEventSubtree());
            cachedMarkupBuilder.Append(_postWmlOnEventMarkupBuilder.ToString());

            _cachedMarkup = cachedMarkupBuilder.ToString ();
            InnerWriter = _cachedInnerWriter;
            _cachingRendering = false;
        }

        private XhtmlStyleClass EnsureXhtmlStyleClassInHashtable (XhtmlStyleClass styleClass) {
            if (styleClass.Filter == StyleFilter.None) {
                return CurrentStyleClass;
            }
            // We hash the style classes by the class definition.
            String classKey = styleClass.GetClassDefinition ();
            XhtmlStyleClass existingClass = (XhtmlStyleClass) _styleHash [classKey];            
            string className = existingClass == null ?
                "s" + _styleCount++ : existingClass.StyleClassName;

            if (existingClass == null) {
                // Used to retrieve style classes in order from the hash table.
                _orderedStyleClassKeys.Add (classKey);
                styleClass.StyleClassName = className;
                _styleHash [classKey] = styleClass;
                _isStyleSheetEmpty = false;
            }
            return existingClass == null ? styleClass : existingClass;
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.EnterFormat"]/*' />
        public override void EnterFormat(Style style) {
            StyleFilter filter = CurrentStyleClass.GetFilter(style);
            EnterStyleInternal(new XhtmlFormatStyleClass(style, filter), "span", null /* no additional attributes */);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.EnterLayout"]/*' />
        public override void EnterLayout (Style style) {
            StyleFilter filter = CurrentStyleClass.GetFilter(style);   
            if (!SupportsNoWrapStyle) {
                filter &= ~StyleFilter.Wrapping;
            }
            EnterStyleInternal (new XhtmlLayoutStyleClass(style, filter), "div", null /* no additional attributes */, true /* force tag to be written */);
        }

        // Hiding inherited member works because dynamic type is same as static type.
        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.EnterStyle"]/*' />
        public new void EnterStyle (Style style) {
            StyleFilter filter = CurrentStyleClass.GetFilter(style);
            if (!SupportsNoWrapStyle) {
                filter &= ~StyleFilter.Wrapping;
            }
            // We prefer span to div because span is inline.
            if ((filter & XhtmlConstants.Layout) == 0) {
                EnterStyleInternal (style, "span", filter);
            }
            else {
                EnterStyleInternal (new XhtmlStyleClass(style, filter), "div", null /*additional attributes */, true /* force tag to be written */);
            }
        }

        // This internal overload can be used to enter style by setting the class element
        // on a caller-specified tag, such as <body> or <table>.
        internal void EnterStyle(Style style, String styleTag) {
            StyleFilter filter = CurrentStyleClass.GetFilter(style);
            if (filter == StyleFilter.None) {
                WriteFullBeginTag(styleTag);
                _styleStack.Push(new StylePair(styleTag, style, StyleFilter.None));
                return;
            }
            EnterStyleInternal (style, styleTag, filter);
        }

        // This internal overload can be used to enter style by setting the class element
        // on a caller-specified tag, such as <body> or <table>.
        internal void EnterStyle(XhtmlStyleClass styleClass, String styleTag) {
            if (styleClass.Filter == StyleFilter.None) {
                WriteFullBeginTag(styleTag);
                _styleStack.Push(new StylePair(styleTag, styleClass));
                return;
            }
            EnterStyleInternal (styleClass, styleTag, null);
        }

        private Stack _styleStack = new Stack ();
        // StyleTag is the tag used for the class attribute.  Possible values are div, span,
        // p, or any elt.  Div and span are preferred over p, since p can only contain inline elements.
        internal void EnterStyleInternal (Style style, String styleTag, StyleFilter filter) {
            EnterStyleInternal(style, styleTag, filter, null);
        }

        internal void EnterStyleInternal (XhtmlStyleClass styleClass, String styleTag, NameValueCollection additionalAttributes) {
            EnterStyleInternal(styleClass, styleTag, additionalAttributes, false /* force tag to be written */);
        }

        internal void EnterStyleInternal (XhtmlStyleClass styleClass, String styleTag, NameValueCollection additionalAttributes, bool forceTag) {
            // EnterStyle is only expected to be called when _cachingRendering is true.
            // Specifically, the active form is rendered to the markup cache, then the cached
            // markup is later rendered to the page.  This allows efficient use of CSS.
            // The if clause exits gracefully if the expected precondition is not met.
            if (!_cachingRendering) {
                Debug.Fail ("Only call EnterStyleInternal when caching rendering");
                _styleStack.Push (new StylePair (String.Empty, styleClass));
                return;
            }
            if (styleClass.Filter == StyleFilter.None && !forceTag) {  // value comparison
                WritePendingBreak();
                // Push a placeholder for this style with Tag == "", indicating no
                // tag was written.
                _styleStack.Push (new StylePair (String.Empty, styleClass));
                return;
            }

            // Swap styleClass for an equivalent style class already in the hashtable, if there is one.
            styleClass = EnsureXhtmlStyleClassInHashtable (styleClass);
            WriteBeginTag (styleTag);
            if (styleClass != null && styleClass != CurrentStyleClass && styleClass.Filter != StyleFilter.None) {
                WriteAttribute ("class", styleClass.StyleClassName);
            }
            if (additionalAttributes != null) {
                foreach (String key in additionalAttributes.Keys) {
                    WriteAttribute (key, additionalAttributes[key], true /* encode */);
                }
            }
            Write (">");
            _styleStack.Push (new StylePair (styleTag, styleClass));
        }

        internal void EnterStyleInternal (Style style, String styleTag, StyleFilter filter, NameValueCollection additionalAttributes) {
            EnterStyleInternal(new XhtmlStyleClass(style, filter), styleTag, additionalAttributes, false /* force tag to be written */);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.ExitFormat"]/*' />
        public override void ExitFormat (Style style) {
            ExitStyle (style);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.ExitFormat"]/*' />
        public override void ExitFormat (Style style, bool breakafter) {
            ExitStyle (style);
            if (breakafter) {
                SetPendingBreak();
            }
        }

        
        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.ExitLayout"]/*' />
        public override void ExitLayout (Style style) {
            ExitStyle (style);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.ExitLayout1"]/*' />
        public override void ExitLayout (Style style, bool breakafter) {
            ExitStyle (style);
            if (breakafter) {
                SetPendingBreak();
            }
        }


        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.ExitStyle"]/*' />
        public new void ExitStyle (Style style) {
            StylePair pair = (StylePair) _styleStack.Pop ();
            if (pair.Tag != null && pair.Tag.Length > 0) WriteEndTag (pair.Tag);
            if (IsBlockElement(pair.Tag)) {
                ClearPendingBreak();
            }
            WriteLine ();
        }

        // Please see ASURT 144034 for this device-specific issue.
        // Internal-only utility to return name of an XhtmlFormatStyleClass that represents the (format) diff of the 
        // parameter from the current style. This does not push the style stack -it is used only for tags with 
        // small content models such as <a>.  
        internal String GetCssFormatClassName(Style style) {
            if (!_cachingRendering) {
                Debug.Fail ("Only call when caching rendering");
                return null;
            }
            if (style ==  null) {
                return null; // caller should check for this return value.
            }
            // We need to write all non-default properties, so get the filter from the _defaultStyleClass.
            StyleFilter filter = _defaultStyleClass.GetFilter(style);
            filter &= XhtmlConstants.Format;
            if (filter == StyleFilter.None) {
                return null;
            }
            XhtmlFormatStyleClass styleClass = new XhtmlFormatStyleClass(style, filter);
            // Value returned is a valid styleClass which can be added to the attributes of, e.g., an <a> element
            // to cause character formatting.  Please see 144034 for the device specific issue.
            XhtmlStyleClass hashStyleClass = EnsureXhtmlStyleClassInHashtable(styleClass);
            if (hashStyleClass == null) {
                return null;
            }
            return hashStyleClass.StyleClassName;
        }

        internal string GetStyles () {
            StringBuilder styleBuilder = new StringBuilder ();
            string bodyStyleClassDefinition = _bodyStyle == null ? null : _bodyStyle.GetClassDefinition();
            if (bodyStyleClassDefinition != null && bodyStyleClassDefinition.Trim().Length > 0 
                || _customBodyStyles != null) {
                styleBuilder.Append("body{\r\n");
                styleBuilder.Append(bodyStyleClassDefinition); // null ok.
                styleBuilder.Append(_customBodyStyles); // null ok.
                styleBuilder.Append("}\r\n");
            }
            foreach (String key in _orderedStyleClassKeys) {
                styleBuilder.Append (((XhtmlStyleClass) _styleHash [key]).ToString());
            }            
            return styleBuilder.ToString ();
        }


        private String GetWmlOnEventSubtree() {
            if (_wmlOnEnterForwardVarNames.Count == 0) {
                return String.Empty;
            }
            StringBuilder wmlOnEventBuilder = new StringBuilder ();
            wmlOnEventBuilder.Append("<wml:onevent type=\"onenterforward\">\r\n");
            wmlOnEventBuilder.Append("<wml:refresh>\r\n");
            foreach (String[] varInfo in _wmlOnEnterForwardVarNames) {
                String varName = varInfo[0];
                String varValue = varInfo[1];
                // Clear each client variable by rendering a setvar.
                wmlOnEventBuilder.Append("<wml:setvar name=\"");
                wmlOnEventBuilder.Append(varName);
                wmlOnEventBuilder.Append("\" value=\"");
                EncodeAttributeValue(varValue, wmlOnEventBuilder);
                wmlOnEventBuilder.Append("\"/>\r\n");            
            }
            wmlOnEventBuilder.Append("</wml:refresh>\r\n");
            wmlOnEventBuilder.Append("</wml:onevent>\r\n");
            return wmlOnEventBuilder.ToString();
        }

        private void HandleBreakForTag(String tag) {
            if (IsBlockElement(tag)) {
                ClearPendingBreak();
            }
            else {
                WritePendingBreak();
            }
        }

        private bool IsBlockElement(String tag){
            tag = tag.ToLower(CultureInfo.InvariantCulture);
            return 
            // From xhtml 1.0 transitional dtd, definition of %block; entity.
                tag == "p" || 

            // %heading;
                tag == "h1" || 
                tag == "h2" ||
                tag == "h3" ||
                tag == "h4" ||
                tag == "h5" ||
                tag == "h6" ||

                tag == "div" ||

            // %lists;
                tag == "ul" || 
                tag == "ol" ||
                tag == "dl" ||
                tag == "menu" ||
                tag == "dir" ||

            // %blocktext;
                tag == "pre" ||
                tag == "hr" ||
                tag == "blockquote" ||
                tag == "center" ||
                tag == "noframes" || 

                tag == "isindex" ||                                

                tag == "fieldset" ||

                tag == "table";
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.IsStyleSheetEmpty"]/*' />
        public virtual bool IsStyleSheetEmpty () {
            return _isStyleSheetEmpty;
        }

        // Call to switch from markup above the wml:onevent (if any) to below.
        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.MarkWmlOnEventLocation"]/*' />
        public virtual void MarkWmlOnEventLocation() {
            InnerWriter = new StringWriter(_postWmlOnEventMarkupBuilder, CultureInfo.InvariantCulture);
        }

        internal String PopPhysicalCssClass() {
            return(String)_physicalCssClasses.Pop();
        }

        internal void PushPhysicalCssClass(String cssClass) {
            _physicalCssClasses.Push(cssClass);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.SetBodyStyle"]/*' />
        public virtual void SetBodyStyle (Style style) {
            // Filter is not strictly needed here, since default property values are not written anyway
            // but it is a good practice to provide a meaningful filter.
            StyleFilter filter = _defaultStyleClass.GetFilter(style);
            _bodyStyle = new XhtmlStyleClass (style, filter);
            _isStyleSheetEmpty = filter == 0;
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.SetCacheKey"]/*' />
        public virtual void SetCacheKey (Cache cache) {
            String styleText = GetStyles();
            _cacheKey = styleText.GetHashCode ().ToString (CultureInfo.InvariantCulture);

            // Avoid hash collisions and app developer values in cache by finding a new string.
            while (cache [_cacheKey] != null && 
                (cache [_cacheKey].GetType () != typeof (String) ||
                styleText != (String) cache [_cacheKey])) {
                _cacheKey += "x";
            }            
            _cacheKeyValid = true;
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.SetPendingBreak"]/*' />
        public virtual void SetPendingBreak() {
            _pendingBreak = true;
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.SetSessionKey"]/*' />
        public virtual void SetSessionKey (HttpSessionState session) {
            String styleText = GetStyles();
            _sessionKey = XhtmlConstants.SessionKeyPrefix + styleText.GetHashCode ().ToString (CultureInfo.InvariantCulture);

            // Avoid hash collisions and app developer values in session by finding a new string.
            while (session [_sessionKey] != null && 
                (session [_sessionKey].GetType () != typeof (String) ||
                styleText != (String) session [_sessionKey])) {
                _sessionKey += "x";
            }            
            _sessionKeyValid = true;
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteAttribute"]/*' />
        public override void WriteAttribute(String attribute, String value, bool encode) {

            // If the value is null, we return without writing anything.  This is different
            // from HtmlTextWriter, which writes the name of the attribute, but no value at all.
            // A name with no value is illegal in Xml.
            if (value == null) {
                return;
            }

            if (encode) {
                // Unlike HTML encoding, we need to replace <> with &lt; and &gt. 
                // For performance reasons we duplicate some code,
                // rather than piggybacking on the inherited method.

                Write(' ');
                Write(attribute);
                Write("=\"");
                WriteEncodedAttributeValue(value);

                Write('\"');
            }
            else {
                base.WriteAttribute(attribute, value, encode);
            }
        }


        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteBeginTag"]/*' />
        public override void WriteBeginTag(String tag) {
            HandleBreakForTag(tag);
            base.WriteBeginTag(tag);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteBreak"]/*' />
        public new virtual void WriteBreak() {
            if (UseDivsForBreaks) {
                if((string)Device["usePOverDiv"] == "true")
                    WriteLine("<br/>");
                else
                    WriteLine("</div><div>");
            }
            else {
                WriteLine ("<br/>");
            }
        }

        // CachedEndTag used for devices that cannot render <select> directly after <table>
        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteCachedMarkup"]/*' />
        public virtual void WriteCachedMarkup () {
            Write (_cachedMarkup);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteDoctypeDeclaration"]/*' />
        public virtual void WriteDoctypeDeclaration (Doctype type){
            WriteLine((String)_doctypeDeclarations[type]);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteEncodedAttributeValue"]/*' />
        public virtual void WriteEncodedAttributeValue(String value) {
            StringBuilder builder = new StringBuilder();
            EncodeAttributeValue(value, builder);
            Write(builder.ToString());
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteEndTag"]/*' />
        public override void WriteEndTag(String tag) {
            if (IsBlockElement(tag)) {
                ClearPendingBreak();
            }
            base.WriteEndTag(tag);
            _cachedEndTag = tag;
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteFullBeginTag"]/*' />
        public override void WriteFullBeginTag(String tag) {
            HandleBreakForTag(tag);
            base.WriteFullBeginTag(tag);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteHiddenField"]/*' />
        public virtual void WriteHiddenField(String name, String value) {
            WriteBeginTag ("input");
            WriteAttribute ("type", "hidden");
            WriteAttribute ("name", name);
            WriteAttribute ("value", value, true);
            WriteLine ("/>");
        }

        // Write a hidden field with no value attribute (useful for __ET hidden field).
        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteHiddenField1"]/*' />
        public virtual void WriteHiddenField(String name) {
            WriteBeginTag ("input");
            WriteAttribute ("type", "hidden");
            WriteAttribute ("name", name);
            WriteLine ("/>");
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine"]/*' />
        public override void WriteLine() {
            if (!_suppressNewLines) base.WriteLine();
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine1"]/*' />
        public override void WriteLine(String format, Object[] arg) {
            if (_suppressNewLines) {
                Write(format, arg);
            }
            else {
                base.WriteLine(format, arg);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine2"]/*' />
        public override void WriteLine(String format, Object arg) {
            if (_suppressNewLines) {
                Write(format, arg);
            }
            else {
                base.WriteLine(format, arg);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine3"]/*' />
        public override void WriteLine(String format, Object arg0, Object arg1) {
            if (_suppressNewLines) {
                Write(format, arg0, arg1);
            }
            else {
                base.WriteLine(format, arg0, arg1);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine4"]/*' />
        public override void WriteLine(Object v) {
            if (_suppressNewLines) {
                Write(v);
            }
            else {
                base.WriteLine(v);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine5"]/*' />
        public override void WriteLine(String s) {
            if (_suppressNewLines) {
                Write(s);
            }
            else {
                base.WriteLine(s);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine6"]/*' />
        public override void WriteLine(Double v) {
            if (_suppressNewLines) {
                Write(v);
            }
            else {
                base.WriteLine(v);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine7"]/*' />
        public override void WriteLine(Single v) {

            if (_suppressNewLines) {
                Write(v);
            }
            else {
                base.WriteLine(v);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine8"]/*' />
        public override void WriteLine(Int64 v) {
            if (_suppressNewLines) {
                Write(v);
            }
            else {
                base.WriteLine(v);
            }
        }

        // Note: type UInt32 is not CLS compliant, hence no override for UInt32
        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine9"]/*' />
        public override void WriteLine(Int32 v) {
            if (_suppressNewLines) {
                Write(v);
            }
            else {
                base.WriteLine(v);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine10"]/*' />
        public override void WriteLine(Boolean v) {
            if (_suppressNewLines) {
                Write(v);
            }
            else {
                base.WriteLine(v);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine11"]/*' />
        public override void WriteLine(Char[] buffer, Int32 index, Int32 count) {
            if (_suppressNewLines) {
                Write(buffer, index, count);
            }
            else {
                base.WriteLine(buffer, index, count);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine12"]/*' />
        public override void WriteLine(Char[] v) {
            if (_suppressNewLines) {
                Write(v);
            }
            else {
                base.WriteLine(v);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteLine13"]/*' />
        public override void WriteLine(Char v) {
            if (_suppressNewLines) {
                Write(v);
            }
            else {
                base.WriteLine(v);
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WritePendingBreak"]/*' />
        public virtual void WritePendingBreak(){
            if (_pendingBreak) {
                WriteBreak();
                _pendingBreak = false;
            }
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteUrlParameter"]/*' />
        public virtual void WriteUrlParameter (String name, String value) {
            WriteEncodedUrlParameter (name);
            Write ("=");
            WriteEncodedUrlParameter (value);
        }

        /// <include file='doc\XhtmlMobileTextWriter.uex' path='docs/doc[@for="XhtmlMobileTextWriter.WriteXmlDeclaration"]/*' />
        public virtual void WriteXmlDeclaration (){
            Write ("<?xml version=\"1.0\" ");
            WriteAttribute ("encoding", HttpContext.Current.Response.ContentEncoding.WebName);
            WriteLine ("?>");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Style stack elements are of type StylePair.
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private struct StylePair {

            internal StylePair (String styleTag, XhtmlStyleClass styleClass) {
                Tag = styleTag;
                Class = styleClass;
            }

            internal StylePair (String styleTag, Style style, StyleFilter filter) {
                Tag = styleTag;
                Class = new XhtmlStyleClass (style, filter);
            }

            // Review: public fields
            internal String Tag;
            internal XhtmlStyleClass Class;
        }
    }
}
