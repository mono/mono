//------------------------------------------------------------------------------
// <copyright file="XhtmlTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Globalization;

    public class XhtmlTextWriter : HtmlTextWriter {

        private Hashtable _commonAttributes = new Hashtable();
        // For _elementSpecificAttributes, each hashtable value is a hashtable.  If an attribute name appears as a key
        // in the hashtable corresponding to an element, the attribute is passed through.
        private Hashtable _elementSpecificAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        private Hashtable _suppressCommonAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        private XhtmlMobileDocType _docType;

        internal override bool RenderDivAroundHiddenInputs {
            get {
                return false;
            }
        }


        public XhtmlTextWriter(TextWriter writer) : this(writer, DefaultTabString) {
        }


        public XhtmlTextWriter(TextWriter writer, string tabString) : base(writer, tabString) {

            // Common attributes are defined in the xhtml modularization spec.  They are allowed by the writer unless
            // _suppressCommonAttribues["elementName"] is nonnull.
            _commonAttributes.Add("class", true);
            _commonAttributes.Add("id", true);
            _commonAttributes.Add("title", true);
            _commonAttributes.Add("xml:lang", true);

            // Note: "Dir" attribute is included in I18N if the bidirectional text module is included.  Since
            // this is not the case in XHTML-MP, omit the dir attribute.
            // See http://www.wapforum.org/tech/documents/WAP-277-XHTMLMP-20011029-a.pdf and
            // http://www.w3.org/TR/xhtml-modularization/abstract_modules.html.
            // See also VSWhidbey 228858.
            // _commonAttributes.Add("dir", true);

            // Note: style attribute is added in SetDocType in case doctype is XHTML-MP or WML20.  In XHTML Basic, it is disallowed.

            // Initialize dictionary lookup of attributes by element.

            // Structure Module
            // body elt has only common attributes.
            AddRecognizedAttributes("head", "xml:lang");
            _suppressCommonAttributes["head"] = true; // common attributes are disallowed for this elt.
            AddRecognizedAttributes("html", "xml:lang", "version", "xmlns");
            _suppressCommonAttributes["html"] = true; // common attributes are disallowed for this elt.
            AddRecognizedAttributes("title", "xml:lang");
            _suppressCommonAttributes["title"] = true; // common attributes are disallowed for this elt.

            // Text module
            // abbr, acronym, address have only common attributes.
            AddRecognizedAttributes("blockquote", "cite");
            AddRecognizedAttributes("br", "class", "id", "title"); // br allows only core attributes.
            _suppressCommonAttributes["br"] = true;
            // cite, code, dfn, div, em, h1-h6, kbd, p have only common attributes.
            AddRecognizedAttributes("pre", "xml:space");
            AddRecognizedAttributes("q", "cite");
            // samp, span, strong, var have only common attributes.

            // Hypertext module
            AddRecognizedAttributes("a", "accesskey", "charset", "href", "hreflang", "rel", "rev", "tabindex", "type", "title");
            
            // List module
            // dl, dt, dd, ol, ul, li have only common attributes.

            // Basic Forms module
            AddRecognizedAttributes("form", "action", "method", "enctype");
            AddRecognizedAttributes("input", "accesskey", "checked", "maxlength", "name", "size", "src", "tabindex", "type", "value", "title", "disabled");
            AddRecognizedAttributes("label", "accesskey");
            AddRecognizedAttributes("label", "for");
            AddRecognizedAttributes("select", "multiple", "name", "size", "tabindex", "disabled");
            AddRecognizedAttributes("option", "selected", "value");
            AddRecognizedAttributes("textarea", "accesskey", "cols", "name", "rows", "tabindex");

            // Basic Tables module
            // caption has only common attributes.
            AddRecognizedAttributes("table", "summary", "width");
            AddRecognizedAttributes("td", "abbr", "align", "axis", "colspan", "headers", "rowspan", "scope", "valign");
            AddRecognizedAttributes("th", "abbr", "align", "axis", "colspan", "headers", "rowspan", "scope", "valign");
            AddRecognizedAttributes("tr", "align", "valign");

            // Image module
            AddRecognizedAttributes("img", "alt", "height", "longdesc", "src", "width");

            // Object module
            AddRecognizedAttributes("object", "archive", "classid", "codebase", "codetype", "data", "declare", "height", "name",
                                    "standby", "tabindex", "type", "width");
            AddRecognizedAttributes("param", "id", "name", "type", "value", "valuetype");

            // Metainformation module
            AddRecognizedAttributes("meta", "xml:lang", "content", "http-equiv", "name", "scheme");
            _suppressCommonAttributes["meta"] = true; // common attributes are disallowed for this elt.

            // Link module
            AddRecognizedAttributes("link", "charset", "href", "hreflang", "media", "rel", "rev", "type");


            // Base module
            AddRecognizedAttributes("base", "href");
            _suppressCommonAttributes["base"] = true; // common attributes are disallowed for this elt.

            // Partial Forms module
            // fieldset has only common attributes.
            AddRecognizedAttributes("optgroup", "disabled", "label");

            // Partial Legacy module
            AddRecognizedAttributes("ol", "start");
            AddRecognizedAttributes("li", "value");

            // Partial Presentation module
            // b, big, hr, i, small have only common attributes

            // Style module
            AddRecognizedAttributes("style", "xml:lang", "media", "title", "type", "xml:space");
            _suppressCommonAttributes["style"] = true;  // common attributes are disallowed for this elt.

        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void AddRecognizedAttribute(string elementName, string attributeName) {
            AddRecognizedAttributes(elementName, attributeName);
        }

        private void AddRecognizedAttributes(string elementName, params string[] attributes) {
            Hashtable eltAttributes = (Hashtable) _elementSpecificAttributes[elementName];
            if (eltAttributes == null) {
                eltAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                _elementSpecificAttributes[elementName] = eltAttributes;
            }
            foreach(string attribute in attributes) {
                eltAttributes.Add(attribute, true);
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool IsValidFormAttribute(string attributeName) {
            Hashtable formAttributes = (Hashtable)_elementSpecificAttributes["form"];
            return (formAttributes != null) && (formAttributes[attributeName] != null);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected override bool OnAttributeRender(string name, string value, HtmlTextWriterAttribute key) {
            if (_commonAttributes[name] != null && _suppressCommonAttributes[TagName] == null) {
                return true;
            }

            // TagName is valid when OnAttributeRender is called.
            return _elementSpecificAttributes[TagName] != null && ((Hashtable)_elementSpecificAttributes[TagName])[name] != null;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected override bool OnStyleAttributeRender(string name,string value, HtmlTextWriterStyle key) {
            if (_docType == XhtmlMobileDocType.XhtmlBasic) {
                return false;
            }

            if (TagName.ToLower(CultureInfo.InvariantCulture).Equals("div") && name.ToLower(CultureInfo.InvariantCulture).Equals("border-collapse")) {
                return false;
            }
            return true;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void RemoveRecognizedAttribute(string elementName, string attributeName) {
            Hashtable eltAttributes = (Hashtable) _elementSpecificAttributes[elementName];
            if (eltAttributes == null) {
                eltAttributes = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                _elementSpecificAttributes[elementName] = eltAttributes;
            }

            if (_commonAttributes[attributeName] == null || _suppressCommonAttributes[elementName] != null) {
                // Note: Hashtable::Remove silently continues if the key does not exist.
                eltAttributes.Remove(attributeName);
                return;
            }

            // (...else) This is an edge case.  The call removes a common attribute, so we need to add each common attribute and remove the
            // except the specified one.
            _suppressCommonAttributes[elementName] = true;
            foreach(string key in _commonAttributes.Keys) {
                if (key != attributeName) {
                    eltAttributes.Add(attributeName, true);
                }
            }
        }



        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void SetDocType(XhtmlMobileDocType docType) {
            _docType = docType;

            if (docType != XhtmlMobileDocType.XhtmlBasic && _commonAttributes["style"] == null) {
                _commonAttributes.Add("style", true);
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteBreak()  {
            WriteFullBeginTag("br/");
        }

        protected Hashtable CommonAttributes {
            get {
                return _commonAttributes;
            }
        }

        protected Hashtable ElementSpecificAttributes {
            get {
                return _elementSpecificAttributes;
            }
        }

        protected Hashtable SuppressCommonAttributes {
            get {
                return _suppressCommonAttributes;
            }
        }

    }


    /// <devdoc>
    /// <para>[To be supplied.]</para>
    /// </devdoc>
    public enum XhtmlMobileDocType {

        XhtmlBasic,

        XhtmlMobileProfile,

        Wml20
    }

}
