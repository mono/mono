//------------------------------------------------------------------------------
// <copyright file="xml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.Util;
    using System.Web.UI;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Security.Policy;
    using System.Xml;
    using System.Xml.Xsl;
    using System.Xml.XPath;
    using System.Security.Permissions;

    public class XmlBuilder : ControlBuilder {


        public override void AppendLiteralString(string s) {}


        public override Type GetChildControlType(string tagName, IDictionary attribs) {
            return null;
        }


        public override bool NeedsTagInnerText() { return true; }


        [SuppressMessage("Microsoft.Security", "MSEC1220:ReviewDtdProcessingAssignment", Justification = "Dtd processing is needed for back-compat, but is being done as safely as possible.")]
        [SuppressMessage("Microsoft.Security.Xml", "CA3069:ReviewDtdProcessingAssignment", Justification = "Dtd processing is needed for back-compat, but is being done as safely as possible.")]
        public override void SetTagInnerText(string text) {
            if (!Util.IsWhiteSpaceString(text)) {

                // Trim the initial whitespaces since XML is very picky (ASURT 58100)
                int iFirstNonWhiteSpace = Util.FirstNonWhiteSpaceIndex(text);
                string textNoWS = text.Substring(iFirstNonWhiteSpace);

                // Update the line number to point to the correct line in the xml
                // block (ASURT 58233).
                Line += Util.LineCount(text, 0, iFirstNonWhiteSpace);

                // Parse the XML here just to cause a parse error in case it is
                // malformed.  It will be parsed again at runtime.
                XmlDocument document = new XmlDocument();
                XmlReaderSettings readerSettings = XmlUtils.CreateXmlReaderSettings();
                readerSettings.LineNumberOffset = Line - 1;

                // VSWhidbey 546662: XmlReader has different default settings than XmlTextReader which was used in Everett
                readerSettings.DtdProcessing = DtdProcessing.Parse;
                readerSettings.CheckCharacters = false;
                
                XmlReader dataReader = XmlUtils.CreateXmlReader(new StringReader(textNoWS), string.Empty, readerSettings);

                try {
                    document.Load(dataReader);
                }
                catch (XmlException e) {
                    // Xml exception sometimes returns -1 for the line, in which case we ignore it.
                    if (e.LineNumber >= 0) {
                        Line = e.LineNumber;
                    }

                    throw;
                }

                base.AppendLiteralString(text);
            }
        }
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [
    DefaultProperty("DocumentSource"),
    PersistChildren(false, true),
    ControlBuilderAttribute(typeof(XmlBuilder)),
    Designer("System.Web.UI.Design.WebControls.XmlDesigner, " + AssemblyRef.SystemDesign)
    ]
    public class Xml : Control {

        private XPathNavigator _xpathNavigator;
        private XmlDocument _document;
        private XPathDocument _xpathDocument;
#pragma warning disable 0618    // To avoid deprecation warning
        private XslTransform _transform;
#pragma warning restore 0618
        private XslCompiledTransform _compiledTransform;
        private XsltArgumentList _transformArgumentList;
        private string _documentContent;
        private string _documentSource;
        private string _transformSource;

        const string identityXslStr =
            "<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
            "<xsl:template match=\"/\"> <xsl:copy-of select=\".\"/> </xsl:template> </xsl:stylesheet>";

#pragma warning disable 0618    // To avoid deprecation warning
        static XslTransform _identityTransform;
#pragma warning restore 0618

        [SuppressMessage("Microsoft.Security", "MSEC1201:DoNotUseXslTransform", Justification = "_identityTransform contents are trusted hard-coded string.")]
        [SuppressMessage("Microsoft.Security.Xml", "CA3050:DoNotUseXslTransform", Justification = "_identityTransform contents are trusted hard-coded string.")]
        [SuppressMessage("Microsoft.Security", "MSEC1205:DoNotAllowDtdOnXmlTextReader", Justification = "_identityTransform contents are trusted hard-coded string.")]
        [SuppressMessage("Microsoft.Security.Xml", "CA3054:DoNotAllowDtdOnXmlTextReader", Justification = "_identityTransform contents are trusted hard-coded string.")]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        static Xml() {

            // Instantiate an identity transform, to be used whenever we need to output XML
            XmlTextReader reader = new XmlTextReader(new StringReader(identityXslStr));
#pragma warning disable 0618    // To avoid deprecation warning
            _identityTransform = new XslTransform();
#pragma warning restore 0618

            _identityTransform.Load(reader, null /*resolver*/, null /*evidence*/);
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override string ClientID {
            get {
                return base.ClientID;
            }
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override ControlCollection Controls {
            get {
                return base.Controls;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false),
        WebSysDescription(SR.Xml_Document),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Obsolete("The recommended alternative is the XPathNavigator property. Create a System.Xml.XPath.XPathDocument and call CreateNavigator() to create an XPathNavigator. http://go.microsoft.com/fwlink/?linkid=14202"),
        ]
        public XmlDocument Document {
            get {
                if (_document == null) {
                    LoadXmlDocument();
                }
                return _document;
            }
            set {
                DocumentSource = null;
                _xpathDocument = null;
                _documentContent = null;
                _document = value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.Xml_DocumentContent)
        ]
        public String DocumentContent {
            get {
                return _documentContent != null ? _documentContent : String.Empty;
            }
            set {
                _document = null;
                _xpathDocument = null;
                _xpathNavigator = null;
                _documentContent = value;

                if (DesignMode) {
                    ViewState["OriginalContent"] = null;
                }
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.XmlUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.Xml_DocumentSource)
        ]
        public String DocumentSource {
            get {
                return (_documentSource == null) ? String.Empty : _documentSource;
            }
            set {
                _document = null;
                _xpathDocument = null;
                _documentContent = null;
                _xpathNavigator = null;
                _documentSource = value;
            }
        }

        [
        Browsable(false),
        DefaultValue(false),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool EnableTheming {
            get {
                return false;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name));
            }
        }

        [
        Browsable(false),
        DefaultValue(""),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override string SkinID {
            get {
                return String.Empty;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name));
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false),
        WebSysDescription(SR.Xml_Transform),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
#pragma warning disable 0618    // To avoid deprecation warning
        public XslTransform Transform {
#pragma warning restore 0618
            get {
                return XmlUtils.GetXslTransform(_transform);
            }
            set {
                if (XmlUtils.GetXslTransform(value) != null) {
                    // Setting this property will nullify _transform, so do it first.
                    TransformSource = null;
                    _transform = value;
                }
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        Browsable(false),
        WebSysDescription(SR.Xml_TransformArgumentList),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public XsltArgumentList TransformArgumentList {
            get {
                return _transformArgumentList;
            }
            set {
                _transformArgumentList = value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.XslUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.Xml_TransformSource),
        ]
        public String TransformSource {
            get {
                return (_transformSource == null) ? String.Empty : _transformSource;
            }
            set {
                _transform = null;
                _transformSource = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        WebSysDescription(SR.Xml_XPathNavigator),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public XPathNavigator XPathNavigator {
            get {
                return _xpathNavigator;
            }
            set {
                DocumentSource = null;
                _xpathDocument = null;
                _documentContent = null;
                _document = null;
                _xpathNavigator = value;
            }
        }


        [SuppressMessage("Microsoft.Security", "MSEC1218:ReviewWebControlForSet_DocumentContent", Justification = "Legacy code that trusts our developer input.  Optional safer codepath available via appSettings/aspnet:RestrictXmlControls configuration.")]
        [SuppressMessage("Microsoft.Security.Xml", "CA3067:ReviewWebControlForSet_DocumentContent", Justification = "Legacy code that trusts our developer input.  Optional safer codepath available via appSettings/aspnet:RestrictXmlControls configuration.")]
        protected override void AddParsedSubObject(object obj) {
            if (obj is LiteralControl) {
                // Trim the initial whitespaces since XML is very picky (related to ASURT 58100)
                string text = ((LiteralControl)obj).Text;

                int iFirstNonWhiteSpace = Util.FirstNonWhiteSpaceIndex(text);
                DocumentContent = text.Substring(iFirstNonWhiteSpace);

                if (DesignMode) {
                    ViewState["OriginalContent"] = text;
                }
            }
            else {
                throw new HttpException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "Xml", obj.GetType().Name.ToString(CultureInfo.InvariantCulture)));
            }
        }

        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override Control FindControl(string id) {
            return base.FindControl(id);
        }


        /// <devdoc>
        /// </devdoc>
        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void Focus() {
            throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override IDictionary GetDesignModeState() {
            IDictionary designModeState = new HybridDictionary();
            designModeState["OriginalContent"] = ViewState["OriginalContent"];

            return designModeState;
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool HasControls() {
            return base.HasControls();
        }

        private void LoadTransformFromSource() {

            // We're done if we already have a transform
            if (_transform != null)
                return;

            if (String.IsNullOrEmpty(_transformSource) || _transformSource.Trim().Length == 0)
                return;

            // First, figure out if it's a physical or virtual path
            VirtualPath virtualPath;
            string physicalPath;
            ResolvePhysicalOrVirtualPath(_transformSource, out virtualPath, out physicalPath);

            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            string key = CacheInternal.PrefixLoadXPath + ((physicalPath != null) ?
                physicalPath : virtualPath.VirtualPathString);

            object xform = cacheInternal.Get(key);

            if (xform == null) {
                Debug.Trace("XmlControl", "Xsl Transform not found in cache (" + _transformSource + ")");

                // Get the stream, and open the doc
                CacheDependency dependency;
                using (Stream stream = OpenFileAndGetDependency(virtualPath, physicalPath, out dependency)) {

                    // If we don't have a physical path, call MapPath to get one, in order to pass it as
                    // the baseUri to XmlTextReader.  In pure VirtualPathProvider scenarios, it won't
                    // help much, but it allows the default case to have relative references (VSWhidbey 545322)
                    if (physicalPath == null)
                        physicalPath = virtualPath.MapPath();

                    // XslCompiledTransform for some reason wants to completely re-create an internal XmlReader
                    // from scratch.  In doing so, it does not respect all the settings of XmlTextReader.  So we want
                    // to be sure to specifically use XmlReader here so our settings on it will be respected by the
                    // XslCompiledTransform.
                    XmlReader xmlReader = XmlUtils.CreateXmlReader(stream, physicalPath);
                    _transform = XmlUtils.CreateXslTransform(xmlReader);
                    if (_transform == null) {
                        _compiledTransform = XmlUtils.CreateXslCompiledTransform(xmlReader);
                    }
                }

                // Cache it, but only if we got a dependency
                if (dependency != null) {
                    using (dependency) {
                        cacheInternal.UtcInsert(key, ((_compiledTransform == null) ? (object)_transform : (object)_compiledTransform), dependency);
                    }
                }
            }
            else {
                Debug.Trace("XmlControl", "XslTransform found in cache (" + _transformSource + ")");

                _compiledTransform = xform as XslCompiledTransform;
                if (_compiledTransform == null) {
#pragma warning disable 0618    // To avoid deprecation warning
                    _transform = (XslTransform)xform;
#pragma warning restore 0618
		}
            }
        }

        private void LoadXmlDocument() {

            Debug.Assert(_xpathDocument == null && _document == null && _xpathNavigator == null);

            if (!String.IsNullOrEmpty(_documentContent)) {
                _document = XmlUtils.CreateXmlDocumentFromContent(_documentContent);
                return;
            }

            if (String.IsNullOrEmpty(_documentSource))
                return;

            // Make it absolute and check security
            string physicalPath = MapPathSecure(_documentSource);

            CacheInternal cacheInternal = System.Web.HttpRuntime.CacheInternal;
            string key = CacheInternal.PrefixLoadXml + physicalPath;

            _document = (XmlDocument) cacheInternal.Get(key);

            if (_document == null) {
                Debug.Trace("XmlControl", "XmlDocument not found in cache (" + _documentSource + ")");

                CacheDependency dependency;
                using (Stream stream = OpenFileAndGetDependency(null, physicalPath, out dependency)) {

                    _document = new XmlDocument();
                    _document.Load(XmlUtils.CreateXmlReader(stream, physicalPath));
                    cacheInternal.UtcInsert(key, _document, dependency);
                }
            }
            else {
                Debug.Trace("XmlControl", "XmlDocument found in cache (" + _documentSource + ")");
            }

            // 
            lock (_document) {
                // Always return a clone of the cached copy
                _document = (XmlDocument)_document.CloneNode(true/*deep*/);
            }
        }

        private void LoadXPathDocument() {

            Debug.Assert(_xpathDocument == null && _document == null && _xpathNavigator == null);

            if (!String.IsNullOrEmpty(_documentContent)) {
                _xpathDocument = XmlUtils.CreateXPathDocumentFromContent(_documentContent);
                return;
            }

            if (String.IsNullOrEmpty(_documentSource))
                return;

            // First, figure out if it's a physical or virtual path
            VirtualPath virtualPath;
            string physicalPath;
            ResolvePhysicalOrVirtualPath(_documentSource, out virtualPath, out physicalPath);

            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            string key = CacheInternal.PrefixLoadXPath + ((physicalPath != null) ?
                physicalPath : virtualPath.VirtualPathString);

            _xpathDocument = (XPathDocument)cacheInternal.Get(key);
            if (_xpathDocument == null) {
                Debug.Trace("XmlControl", "XPathDocument not found in cache (" + _documentSource + ")");

                // Get the stream, and open the doc
                CacheDependency dependency;
                using (Stream stream = OpenFileAndGetDependency(virtualPath, physicalPath, out dependency)) {
                    // The same comments as in LoadTransformFromSource() (VSWhidbey 545322, 546662)
                    if (physicalPath == null) {
                        physicalPath = virtualPath.MapPath();
                    }
  
                    _xpathDocument = new XPathDocument(XmlUtils.CreateXmlReader(stream, physicalPath));
                }

                // Cache it, but only if we got a dependency
                if (dependency != null) {
                    using (dependency) {
                        cacheInternal.UtcInsert(key, _xpathDocument, dependency);
                    }
                }
            }
            else {
                Debug.Trace("XmlControl", "XPathDocument found in cache (" + _documentSource + ")");
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "MSEC1204:UseSecureXmlResolver", Justification = "Legacy code that trusts our developer input.  Optional safer codepath available via appSettings/aspnet:RestrictXmlControls configuration.")]
        protected internal override void Render(HtmlTextWriter output)
        {

            // If we don't already have an XmlDocument or an XPathNavigator, load am XPathDocument (which is faster)
            if ((_document == null) && (_xpathNavigator == null)) {
                LoadXPathDocument();
            }

            LoadTransformFromSource();

            // Abort if nothing has been loaded
            if (_document == null && _xpathDocument == null && _xpathNavigator == null) {
                return;
            }

            // If we don't have a transform, use the identity transform, which
            // simply renders the XML.
            if (_transform == null)
                _transform = _identityTransform;

            // Pass a resolver in full trust, to support certain XSL scenarios (ASURT 141427)
            XmlUrlResolver xr = null;
            if (HttpRuntime.HasUnmanagedPermission()) {
                xr = new XmlUrlResolver();
            }

            IXPathNavigable doc = null;
            if (_document != null) {
                doc = _document;
            } else if (_xpathNavigator != null) {
                doc = _xpathNavigator;
            } else {
                doc = _xpathDocument;
            }

            if (_compiledTransform != null) {
                XmlWriter writer = XmlWriter.Create(output);
                _compiledTransform.Transform(doc, _transformArgumentList, writer, null);
            } else {
                _transform.Transform(doc, _transformArgumentList, output, xr);
            }
        }
    }
}

