//------------------------------------------------------------------------------
// <copyright file="AccessDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.OleDb;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.IO;
    using System.Text;
    using System.Web.Caching;
    using System.Web.UI;


    /// <devdoc>
    /// Allows a user to create a declarative connection to an Access database in a .aspx page.
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.AccessDataSourceDesigner, " + AssemblyRef.SystemDesign),
    ToolboxBitmap(typeof(AccessDataSource)),
    WebSysDescription(SR.AccessDataSource_Description),
    WebSysDisplayName(SR.AccessDataSource_DisplayName)
    ]
    public class AccessDataSource : SqlDataSource {

        private const string OleDbProviderName = "System.Data.OleDb";
        private const string JetProvider = "Microsoft.Jet.OLEDB.4.0";
        private const string Access2007Provider = "Microsoft.ACE.OLEDB.12.0";
        private const string Access2007FileExtension = ".accdb";

        private FileDataSourceCache _cache;
        private string _connectionString;
        private string _dataFile;
        private string _physicalDataFile;



        /// <devdoc>
        /// Creates a new instance of AccessDataSource.
        /// </devdoc>
        public AccessDataSource() : base() {
        }


        /// <devdoc>
        /// Creates a new instance of AccessDataSource with a specified connection string and select command.
        /// </devdoc>
        public AccessDataSource(string dataFile, string selectCommand) : base() {
            if (String.IsNullOrEmpty(dataFile)) {
                throw new ArgumentNullException("dataFile");
            }
            DataFile = dataFile;
            SelectCommand = selectCommand;
        }

        /// <devdoc>
        /// Specifies the cache settings for this data source. For the cache to
        /// work, the DataSourceMode must be set to DataSet.
        /// </devdoc>
        internal override DataSourceCache Cache {
            get {
                if (_cache == null) {
                    _cache = new FileDataSourceCache();
                }
                return _cache;
            }
        }

        /// <devdoc>
        /// Gets the connection string for the AccessDataSource. This property is auto-generated and cannot be set.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override string ConnectionString {
            get {
                if (_connectionString == null) {
                    _connectionString = CreateConnectionString();
                }
                return _connectionString;
            }
            set {
                throw new InvalidOperationException(SR.GetString(SR.AccessDataSource_CannotSetConnectionString));
            }
        }

        /// <devdoc>
        /// The name of an Access database file.
        /// This property is not stored in ViewState.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.MdbDataFileEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebCategory("Data"),
        WebSysDescription(SR.AccessDataSource_DataFile),
        ]
        public string DataFile {
            get {
                return (_dataFile == null) ? String.Empty : _dataFile;
            }
            set {
                if (DataFile != value) {
                    _dataFile = value;
                    _connectionString = null;
                    _physicalDataFile = null;
                    RaiseDataSourceChangedEvent(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Gets the file data source cache object.
        /// </devdoc>
        private FileDataSourceCache FileDataSourceCache {
            get {
                FileDataSourceCache fileCache = Cache as FileDataSourceCache;
                Debug.Assert(fileCache != null, "Cache object should be a FileDataSourceCache");
                return fileCache;
            }
        }

        /// <devdoc>
        /// Gets the Physical path of the data file.
        /// </devdoc>
        private string PhysicalDataFile {
            get {
                if (_physicalDataFile == null) {
                    _physicalDataFile = GetPhysicalDataFilePath();
                }
                return _physicalDataFile;
            }
        }

        internal string NativeProvider {
            get {
                if (IsAccess2007) {
                    return Access2007Provider;
                }
                return JetProvider;
            }
        }

        internal virtual bool IsAccess2007 {
            get {
                // Access 2007 changed the file format so we have to pick the right provider based on extension.
                return Path.GetExtension(PhysicalDataFile) == Access2007FileExtension;
            }
        }

        /// <devdoc>
        /// Gets/sets the ADO.net managed provider name. This property is restricted to the OLE DB provider and cannot be set.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override string ProviderName {
            get {
                return OleDbProviderName;
            }
            set {
                throw new InvalidOperationException(SR.GetString(SR.AccessDataSource_CannotSetProvider, ID));
            }
        }

        /// <devdoc>
        /// A semi-colon delimited string indicating which databases to use for the dependency in the format "database1:table1;database2:table2".
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override string SqlCacheDependency {
            get {
                throw new NotSupportedException(SR.GetString(SR.AccessDataSource_SqlCacheDependencyNotSupported, ID));
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.AccessDataSource_SqlCacheDependencyNotSupported, ID));
            }
        }

        private void AddCacheFileDependency() {
            FileDataSourceCache.FileDependencies.Clear();
            string filename = PhysicalDataFile;
            if (filename.Length > 0) {
                FileDataSourceCache.FileDependencies.Add(filename);
            }
        }

        /// <devdoc>
        /// Creates a connection string for an Access database connection.
        /// The JET or ACE provider is used based on the DataFile extension, and the filename, username, password, and
        /// share mode are are set.
        /// </devdoc>
        private string CreateConnectionString() {
            return "Provider=" + NativeProvider + "; Data Source=" + PhysicalDataFile;
        }

        /// <devdoc>
        /// Creates a new AccessDataSourceView.
        /// </devdoc>
        protected override SqlDataSourceView CreateDataSourceView(string viewName) {
            return new AccessDataSourceView(this, viewName, Context);
        }

        protected override DbProviderFactory GetDbProviderFactory() {
            return OleDbFactory.Instance;
        }

        /// <devdoc>
        /// Gets the appropriate mapped path for the DataFile.
        /// </devdoc>
        private string GetPhysicalDataFilePath() {
            string filename = DataFile;
            if (filename.Length == 0) {
                return null;
            }
            if (!System.Web.Util.UrlPath.IsAbsolutePhysicalPath(filename)) {
                // Root relative path
                if (DesignMode) {
                    // This exception should never be thrown - the designer always maps paths
                    // before using the runtime control.
                    throw new NotSupportedException(SR.GetString(SR.AccessDataSource_DesignTimeRelativePathsNotSupported, ID));
                }
                filename = Context.Request.MapPath(filename, AppRelativeTemplateSourceDirectory, true);
            }

            HttpRuntime.CheckFilePermission(filename, true);

            // We also need to check for path discovery permissions for the
            // file since the page developer will be able to see the physical
            // path in the ConnectionString property.
            if (!HttpRuntime.HasPathDiscoveryPermission(filename)) {
                throw new HttpException(SR.GetString(SR.AccessDataSource_NoPathDiscoveryPermission, HttpRuntime.GetSafePath(filename), ID));
            }
            return filename;
        }

        /// <devdoc>
        /// Saves data to the cache.
        /// </devdoc>
        internal override void SaveDataToCache(int startRowIndex, int maximumRows, object data, CacheDependency dependency) {
            AddCacheFileDependency();
            base.SaveDataToCache(startRowIndex, maximumRows, data, dependency);
        }

        /*internal override void SaveTotalRowCountToCache(int totalRowCount) {
            AddCacheFileDependency();
            base.SaveTotalRowCountToCache(totalRowCount);
        }*/
    }
}

