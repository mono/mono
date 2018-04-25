//------------------------------------------------------------------------------
// <copyright file="XmlDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Xml.Xsl;



    /// <devdoc>
    /// Represents an XML file as both an IDataSource and an IHierarchicalDataSource.
    /// The XML data is retrieved either from a file specified by the DataFile property
    /// or by inline XML content in the Data property.
    /// </devdoc>
    [
    DefaultEvent("Transforming"),
    DefaultProperty("DataFile"),
    Designer("System.Web.UI.Design.WebControls.XmlDataSourceDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxBitmap(typeof(XmlDataSource)),
    WebSysDescription(SR.XmlDataSource_Description),
    WebSysDisplayName(SR.XmlDataSource_DisplayName)
    ]
    public class XmlDataSource : HierarchicalDataSourceControl, IDataSource, IListSource {

        private static readonly object EventTransforming = new object();
        private const string DefaultViewName = "DefaultView";

        private DataSourceCache _cache;
        private bool _cacheLookupDone;
        private bool _disallowChanges;
        private XsltArgumentList _transformArgumentList;
        private ICollection _viewNames;
        private XmlDocument _xmlDocument;
        private string _writeableDataFile;

        private string _data;
        private string _dataFile;
        private string _transform;
        private string _transformFile;
        private string _xPath;


        /// <devdoc>
        /// Specifies the cache settings for this data source.
        /// </devdoc>
        private DataSourceCache Cache {
            get {
                if (_cache == null) {
                    _cache = new DataSourceCache();
                    _cache.Enabled = true;
                }
                return _cache;
            }
        }


        /// <devdoc>
        /// The duration, in seconds, of the expiration. The expiration policy is specified by the CacheExpirationPolicy property.
        /// </devdoc>
        [
        DefaultValue(DataSourceCache.Infinite),
        TypeConverterAttribute(typeof(DataSourceCacheDurationConverter)),
        WebCategory("Cache"),
        WebSysDescription(SR.DataSourceCache_Duration),
        ]
        public virtual int CacheDuration {
            get {
                return Cache.Duration;
            }
            set {
                Cache.Duration = value;
            }
        }

        /// <devdoc>
        /// The expiration policy of the cache. The duration for the expiration is specified by the CacheDuration property.
        /// </devdoc>
        [
        DefaultValue(DataSourceCacheExpiry.Absolute),
        WebCategory("Cache"),
        WebSysDescription(SR.DataSourceCache_ExpirationPolicy),
        ]
        public virtual DataSourceCacheExpiry CacheExpirationPolicy {
            get {
                return Cache.ExpirationPolicy;
            }
            set {
                Cache.ExpirationPolicy = value;
            }
        }

        /// <devdoc>
        /// Indicates an arbitrary cache key to make this cache entry depend on. This allows
        /// the user to further customize when this cache entry will expire.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Cache"),
        WebSysDescription(SR.DataSourceCache_KeyDependency),
        ]
        public virtual string CacheKeyDependency {
            get {
                return Cache.KeyDependency;
            }
            set {
                Cache.KeyDependency = value;
            }
        }

        [
        DefaultValue(""),
        WebCategory("Cache"),
        WebSysDescription(SR.XmlDataSource_CacheKeyContext),
        ]
        public virtual string CacheKeyContext {
            get {
                return (string)ViewState["CacheKeyContext "] ?? String.Empty;
            }
            set {
                ViewState["CacheKeyContext "] = value;
            }
        }

        /// <devdoc>
        /// Inline XML content.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.ComponentModel.Design.MultilineStringEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        TypeConverter("System.ComponentModel.MultilineStringConverter," + AssemblyRef.System),
        WebCategory("Data"),
        WebSysDescription(SR.XmlDataSource_Data),
        ]
        public virtual string Data {
            get {
                if (_data == null) {
                    return String.Empty;
                }
                return _data;
            }
            set {
                if (value != null) {
                    value = value.Trim();
                }
                if (Data != value) {
                    if (_disallowChanges) {
                        throw new InvalidOperationException(SR.GetString(SR.XmlDataSource_CannotChangeWhileLoading, "Data", ID));
                    }
                    _data = value;
                    _xmlDocument = null;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Path to an XML file.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.XmlDataFileEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Data"),
        WebSysDescription(SR.XmlDataSource_DataFile),
        ]
        public virtual string DataFile {
            get {
                if (_dataFile == null) {
                    return String.Empty;
                }
                return _dataFile;
            }
            set {
                if (DataFile != value) {
                    if (_disallowChanges) {
                        throw new InvalidOperationException(SR.GetString(SR.XmlDataSource_CannotChangeWhileLoading, "DataFile", ID));
                    }
                    _dataFile = value;
                    _xmlDocument = null;
                    _writeableDataFile = null;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Whether caching is enabled for this data source.
        /// </devdoc>
        [
        DefaultValue(true),
        WebCategory("Cache"),
        WebSysDescription(SR.DataSourceCache_Enabled),
        ]
        public virtual bool EnableCaching {
            get {
                return Cache.Enabled;
            }
            set {
                Cache.Enabled = value;
            }
        }

        /// <devdoc>
        /// Indicates whether the XML data can be modified.
        /// This is also used by XmlDataSourceView to determine whether CanDelete/Insert/Update are true.
        /// </devdoc>
        internal bool IsModifiable {
            get {
                return (String.IsNullOrEmpty(TransformFile) &&
                        String.IsNullOrEmpty(Transform) &&
                        !String.IsNullOrEmpty(WriteableDataFile));
            }
        }

        /// <devdoc>
        /// Inline XSL transform.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.ComponentModel.Design.MultilineStringEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        TypeConverter("System.ComponentModel.MultilineStringConverter," + AssemblyRef.System),
        WebCategory("Data"),
        WebSysDescription(SR.XmlDataSource_Transform),
        ]
        public virtual string Transform {
            get {
                if (_transform == null) {
                    return String.Empty;
                }
                return _transform;
            }
            set {
                if (value != null) {
                    value = value.Trim();
                }
                if (Transform != value) {
                    if (_disallowChanges) {
                        throw new InvalidOperationException(SR.GetString(SR.XmlDataSource_CannotChangeWhileLoading, "Transform", ID));
                    }
                    _transform = value;
                    _xmlDocument = null;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Arguments for the XSL transform.
        /// This should be populated in the Transforming event.
        /// </devdoc>
        [
        Browsable(false),
        ]
        public virtual XsltArgumentList TransformArgumentList {
            get {
                return _transformArgumentList;
            }
            set {
                _transformArgumentList = value;
            }
        }

        /// <devdoc>
        /// Path to an XSL transform file.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.XslTransformFileEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Data"),
        WebSysDescription(SR.XmlDataSource_TransformFile),
        ]
        public virtual string TransformFile {
            get {
                if (_transformFile == null) {
                    return String.Empty;
                }
                return _transformFile;
            }
            set {
                if (TransformFile != value) {
                    if (_disallowChanges) {
                        throw new InvalidOperationException(SR.GetString(SR.XmlDataSource_CannotChangeWhileLoading, "TransformFile", ID));
                    }
                    _transformFile = value;
                    _xmlDocument = null;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Gets a physical path of the data file that can be written to.
        /// The value is null if the path is not a writable path.
        /// </devdoc>
        private string WriteableDataFile {
            get {
                if (_writeableDataFile == null) {
                    _writeableDataFile = GetWriteableDataFile();
                }
                return _writeableDataFile;
            }
        }

        /// <devdoc>
        /// Specifies an initial XPath that is applied to the XML data.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.XmlDataSource_XPath),
        ]
        public virtual string XPath {
            get {
                if (_xPath == null) {
                    return String.Empty;
                }
                return _xPath;
            }
            set {
                if (XPath != value) {
                    if (_disallowChanges) {
                        throw new InvalidOperationException(SR.GetString(SR.XmlDataSource_CannotChangeWhileLoading, "XPath", ID));
                    }
                    _xPath = value;
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        /// Raised before the XSL transform is applied.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.XmlDataSource_Transforming),
        ]
        public event EventHandler Transforming {
            add {
                Events.AddHandler(EventTransforming, value);
            }
            remove {
                Events.RemoveHandler(EventTransforming, value);
            }
        }


        /// <devdoc>
        /// Creates a unique cache key for this data source's data.
        /// </devdoc>
        // Made internal for unit testing
        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "This is specifically on XmlDataSource type which is not a com interop type.")]
        internal string CreateCacheKey()
        {
            StringBuilder sb = new StringBuilder(CacheInternal.PrefixDataSourceControl, 1024);
            sb.Append(GetType().GetHashCode().ToString(CultureInfo.InvariantCulture));

            sb.Append(CacheDuration.ToString(CultureInfo.InvariantCulture));
            sb.Append(':');
            sb.Append(((int)CacheExpirationPolicy).ToString(CultureInfo.InvariantCulture));

            bool includeUniqueID = false;

            if (!String.IsNullOrEmpty(CacheKeyContext)) {
                sb.Append(':');
                sb.Append(CacheKeyContext);
            }

            if (DataFile.Length > 0) {
                sb.Append(':');
                sb.Append(DataFile);
            }
            else {
                if (Data.Length > 0) {
                    includeUniqueID = true;
                }
            }

            if (TransformFile.Length > 0) {
                sb.Append(':');
                sb.Append(TransformFile);
            }
            else {
                if (Transform.Length > 0) {
                    includeUniqueID = true;
                }
            }

            if (includeUniqueID) {
                // If we don't have any paths, use the Page
                if (Page != null) {
                    sb.Append(':');
                    sb.Append(Page.GetType().AssemblyQualifiedName);
                }
                sb.Append(':');
                string uniqueID = UniqueID;
                if (String.IsNullOrEmpty(uniqueID)) {
                    throw new InvalidOperationException(SR.GetString(SR.XmlDataSource_NeedUniqueIDForCache));
                }
                sb.Append(uniqueID);
            }

            return sb.ToString();
        }

        /// <devdoc>
        /// Returns a HierarchicalDataSourceView based on an XPath specified by viewPath.
        /// </devdoc>
        protected override HierarchicalDataSourceView GetHierarchicalView(string viewPath) {
            return new XmlHierarchicalDataSourceView(this, viewPath);
        }

        /// <devdoc>
        /// Gets an XmlReader representing XML or XSL content, and optionally a cache
        /// dependency for that content.
        /// Supported paths are: Relative paths, physical paths, UNC paths, and HTTP URLs
        /// If a path is not provided, the content parameter is assumed to contain the
        /// actual content.
        /// If there is no data, null is returned.
        /// This method is fully compatible with Virtual Path Providers.
        /// </devdoc>
        private XmlReader GetReader(string path, string content, out CacheDependency cacheDependency) {
            // If a filename is specified, load from file. Otherwise load from inner content.
            if (path.Length != 0) {
                // First try to detect if it is an HTTP URL
                Uri uri;
                bool success = Uri.TryCreate(path, UriKind.Absolute, out uri);
                if (success) {
                    if (uri.Scheme == Uri.UriSchemeHttp) {
                        // Check for Web permissions for the URL we want
                        if (!HttpRuntime.HasWebPermission(uri)) {
                            throw new InvalidOperationException(SR.GetString(SR.XmlDataSource_NoWebPermission, uri.PathAndQuery, ID));
                        }
                        // Dependencies are not supported with HTTP URLs
                        cacheDependency = null;
                        // If it is an HTTP URL and we have permissions, get a reader
                        return XmlUtils.CreateXmlReader(path);
                    }
                }

                // Now see what kind of file-based path it is
                VirtualPath virtualPath;
                string physicalPath;
                ResolvePhysicalOrVirtualPath(path, out virtualPath, out physicalPath);

                if (virtualPath != null && DesignMode) {
                    // This exception should never be thrown - the designer always maps paths
                    // before using the runtime control.
                    throw new NotSupportedException(SR.GetString(SR.XmlDataSource_DesignTimeRelativePathsNotSupported, ID));
                }

                Stream dataStream = OpenFileAndGetDependency(virtualPath, physicalPath, out cacheDependency);
                return XmlUtils.CreateXmlReader(dataStream);
            }
            else {
                // Dependencies are not supported with inline content
                cacheDependency = null;
                content = content.Trim();
                if (content.Length == 0) {
                    return null;
                }
                else {
                    return XmlUtils.CreateXmlReader(new StringReader(content));
                }
            }
        }

        /// <devdoc>
        /// Gets a path to a writeable file where we can save data to.
        /// The return value is null if a writeable path cannot be found.
        /// </devdoc>
        private string GetWriteableDataFile() {
            if (DataFile.Length != 0) {
                // First try to detect if it is an HTTP URL
                Uri uri;
                bool success = Uri.TryCreate(DataFile, UriKind.Absolute, out uri);
                if (success) {
                    if (uri.Scheme == Uri.UriSchemeHttp) {
                        // Cannot write to HTTP URLs
                        return null;
                    }
                }

                if (HostingEnvironment.UsingMapPathBasedVirtualPathProvider) {
                    // Now see what kind of file-based path it is
                    VirtualPath virtualPath;
                    string physicalPath;
                    ResolvePhysicalOrVirtualPath(DataFile, out virtualPath, out physicalPath);
                    if (physicalPath == null) {
                        physicalPath = virtualPath.MapPathInternal(this.TemplateControlVirtualDirectory, true /*allowCrossAppMapping*/);
                    }
                    return physicalPath;
                }
                else {
                    // File is coming from a custom virtual path provider, and there is no support for writing
                    return null;
                }
            }
            else {
                // Data is specified using Data property, so it is not writeable
                return null;
            }
        }

        /// <devdoc>
        /// Returns the XmlDocument representing the XML data.
        /// If necessary, the XML data will be reloaded along with the transform, if available.
        /// </devdoc>
        public XmlDocument GetXmlDocument() {

            string cacheKey = null;

            if (!_cacheLookupDone && Cache.Enabled) {
                // If caching is enabled, attempt to load from cache.
                cacheKey = CreateCacheKey();
                _xmlDocument = Cache.LoadDataFromCache(cacheKey) as XmlDocument;

                _cacheLookupDone = true;
            }

            if (_xmlDocument == null) {

                // Load up the data
                _xmlDocument = new XmlDocument();
                CacheDependency transformCacheDependency;
                CacheDependency dataCacheDependency;

                PopulateXmlDocument(_xmlDocument, out dataCacheDependency, out transformCacheDependency);

                if (cacheKey != null) {
                    Debug.Assert(Cache.Enabled);

                    // If caching is enabled, save the XmlDocument to cache.
                    CacheDependency fileDependency;
                    if (dataCacheDependency != null) {
                        if (transformCacheDependency != null) {
                            // We have both a data file as well as a transform file dependency
                            AggregateCacheDependency aggregateDependency = new AggregateCacheDependency();
                            aggregateDependency.Add(dataCacheDependency, transformCacheDependency);
                            fileDependency = aggregateDependency;
                        }
                        else {
                            // We only have a data file dependency
                            fileDependency = dataCacheDependency;
                        }
                    }
                    else {
                        // We have at most only a transform file dependency (or no dependency at all)
                        fileDependency = transformCacheDependency;
                    }

                    Cache.SaveDataToCache(cacheKey, _xmlDocument, fileDependency);
                }
            }

            return _xmlDocument;
        }

        /// <devdoc>
        /// Populates an XmlDocument with the appropriate XML data, including applying transforms.
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "MSEC1204:UseSecureXmlResolver", Justification = "Legacy code that trusts our developer input.  Optional safer codepath available via appSettings/aspnet:RestrictXmlControls configuration.")]
        private void PopulateXmlDocument(XmlDocument document, out CacheDependency dataCacheDependency, out CacheDependency transformCacheDependency) {
            XmlReader transformReader = null;
            XmlReader dataReader = null;
            XmlReader tempDataReader = null;

            try {
                // Don't allow changes to the XmlDataSource while we are loading the document
                _disallowChanges = true;

                // Check if transform is specified.
                // If there is a transform, load the data, then the transform, and get an XmlReader from the transformation.
                transformReader = GetReader(TransformFile, Transform, out transformCacheDependency);
                if (transformReader != null) {
                    tempDataReader = GetReader(DataFile, Data, out dataCacheDependency);

                    // Now load the transform and transform the document data
#pragma warning disable 0618    // To avoid deprecation warning
                    XslTransform transform = XmlUtils.CreateXslTransform(transformReader, null);
#pragma warning restore 0618
                    if (transform != null) {
                        OnTransforming(EventArgs.Empty);

                        XmlDocument tempDocument = new XmlDocument();
                        tempDocument.Load(tempDataReader);
                        // The XmlResolver cast on the third parameter is required to eliminate an ambiguity
                        // from the compiler.
                        dataReader = transform.Transform(tempDocument, _transformArgumentList, (XmlResolver)null);
                        document.Load(dataReader);
                    }
                    else {
                        // XslCompiledTransform for some reason wants to completely re-create an internal XmlReader
                        // from scratch.  In doing so, it does not respect all the settings of XmlTextReader.  Be 100%
                        // sure that this XmlReader we are using here uses settings of XmlReader and not those
                        // introduced by XmlTextReader.
                        XslCompiledTransform compiledTransform = XmlUtils.CreateXslCompiledTransform(transformReader);

                        OnTransforming(EventArgs.Empty);

                        using (MemoryStream ms = new MemoryStream()) {
                            XmlWriter writer = XmlWriter.Create(ms);
                            compiledTransform.Transform(tempDataReader, _transformArgumentList, writer, null);
                            document.Load(XmlUtils.CreateXmlReader(ms));
                        }
                    }
                }
                else {
                    dataReader = GetReader(DataFile, Data, out dataCacheDependency);
                    document.Load(dataReader);
                }
            }
            finally {
                _disallowChanges = false;

                if (dataReader != null) {
                    dataReader.Close();
                }
                if (tempDataReader != null) {
                    tempDataReader.Close();
                }
                if (transformReader != null) {
                    transformReader.Close();
                }
            }
        }

        /// <devdoc>
        /// Called right before the XSLT transform is applied.
        /// This allows a developer to supply an XsltArgumentList in the TransformArgumentList property.
        /// </devdoc>
        protected virtual void OnTransforming(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventTransforming];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// Saves the XML data to disk.
        /// </devdoc>
        public void Save() {
            if (!IsModifiable) {
                throw new InvalidOperationException(SR.GetString(SR.XmlDataSource_SaveNotAllowed, ID));
            }

            string writeableDataFile = WriteableDataFile;
            Debug.Assert(!String.IsNullOrEmpty(writeableDataFile), "Did not expect WriteableDataFile to be empty in Save()");

            // Check for write permissions
            HttpRuntime.CheckFilePermission(writeableDataFile, true);

            // Save the document
            GetXmlDocument().Save(writeableDataFile);
        }


        #region Implementation of IDataSource
        event EventHandler IDataSource.DataSourceChanged {
            add {
                ((IHierarchicalDataSource)this).DataSourceChanged += value;
            }
            remove {
                ((IHierarchicalDataSource)this).DataSourceChanged -= value;
            }
        }


        /// <internalonly/>
        DataSourceView IDataSource.GetView(string viewName) {
            if (viewName.Length == 0) {
                viewName = DefaultViewName;
            }
            return new XmlDataSourceView(this, viewName);
        }


        /// <internalonly/>
        ICollection IDataSource.GetViewNames() {
            if (_viewNames == null) {
                _viewNames = new string[1] { DefaultViewName };
            }
            return _viewNames;
        }
        #endregion

        #region Implementation of IListSource
        /// <internalonly/>
        bool IListSource.ContainsListCollection {
            get {
                if (DesignMode) {
                    return false;
                }
                return ListSourceHelper.ContainsListCollection(this);
            }
        }


        /// <internalonly/>
        IList IListSource.GetList() {
            if (DesignMode) {
                return null;
            }
            return ListSourceHelper.GetList(this);
        }
        #endregion
    }
}

