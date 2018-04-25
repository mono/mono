//------------------------------------------------------------------------------
// <copyright file="SqlDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.UI;
    using System.Web.Util;
    using ConflictOptions = System.Web.UI.ConflictOptions;


    /// <devdoc>
    /// This class represents a datasource that uses an ADO.net connection to get
    /// its data.
    /// ADO.net's provider factory model is used to support all managed providers
    /// registered in machine.config.
    /// </devdoc>
    [
    DefaultEvent("Selecting"),
    DefaultProperty("SelectQuery"),
    Designer("System.Web.UI.Design.WebControls.SqlDataSourceDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxBitmap(typeof(SqlDataSource)),
    WebSysDescription(SR.SqlDataSource_Description),
    WebSysDisplayName(SR.SqlDataSource_DisplayName)
    ]
    public class SqlDataSource : DataSourceControl {

        private const string DefaultProviderName = "System.Data.SqlClient";
        private const string DefaultViewName = "DefaultView";

        private DataSourceCache _cache;
        private string _cachedSelectCommand;
        private string _connectionString;
        private SqlDataSourceMode _dataSourceMode = SqlDataSourceMode.DataSet;
        private string _providerName;
        private DbProviderFactory _providerFactory;
        private SqlDataSourceView _view;
        private ICollection _viewNames;


        /// <devdoc>
        /// Creates a new instance of SqlDataSource.
        /// </devdoc>
        public SqlDataSource() {
        }

        /// <devdoc>
        /// Creates a new instance of SqlDataSource with a specified connection string and select command.
        /// </devdoc>
        public SqlDataSource(string connectionString, string selectCommand) {
            _connectionString = connectionString;

            // Store the select command until the default view is created
            _cachedSelectCommand = selectCommand;
        }

        /// <devdoc>
        /// Creates a new instance of SqlDataSource with a specified provider name, connection string, and select command.
        /// </devdoc>
        public SqlDataSource(string providerName, string connectionString, string selectCommand) : this(connectionString, selectCommand) {
            _providerName = providerName;
        }


        /// <devdoc>
        /// Specifies the cache settings for this data source. For the cache to
        /// work, the DataSourceMode must be set to DataSet.
        /// </devdoc>
        internal virtual DataSourceCache Cache {
            get {
                if (_cache == null) {
                    _cache = new SqlDataSourceCache();
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

        /// <devdoc>
        /// Indicates whether the Select operation will be cancelled if the value of any of the SelectParameters is null.
        /// </devdoc>
        [
        DefaultValue(true),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_CancelSelectOnNullParameter),
        ]
        public virtual bool CancelSelectOnNullParameter {
            get {
                return GetView().CancelSelectOnNullParameter;
            }
            set {
                GetView().CancelSelectOnNullParameter = value;
            }
        }

        /// <devdoc>
        /// Whether commands pass old values in the parameter collection.
        /// </devdoc>
        [
        DefaultValue(ConflictOptions.OverwriteChanges),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_ConflictDetection),
        ]
        public ConflictOptions ConflictDetection {
            get {
                return GetView().ConflictDetection;
            }
            set {
                GetView().ConflictDetection = value;
            }
        }

        /// <devdoc>
        /// Gets/sets the connection string for the control. This property is not stored in ViewState.
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.WebControls.SqlDataSourceConnectionStringEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebCategory("Data"),
        MergableProperty(false),
        WebSysDescription(SR.SqlDataSource_ConnectionString),
        ]
        public virtual string ConnectionString {
            get {
                return (_connectionString == null ? String.Empty : _connectionString);
            }
            set {
                if (ConnectionString != value) {
                    _connectionString = value;
                    RaiseDataSourceChangedEvent(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Gets/sets the data source mode for the control.
        /// Only certain operations are supported depending on the data mode.
        /// </devdoc>
        [
        DefaultValue(SqlDataSourceMode.DataSet),
        WebCategory("Behavior"),
        WebSysDescription(SR.SqlDataSource_DataSourceMode),
        ]
        public SqlDataSourceMode DataSourceMode {
            get {
                return _dataSourceMode;
            }
            set {
                if (value < SqlDataSourceMode.DataReader || value > SqlDataSourceMode.DataSet) {
                    throw new ArgumentOutOfRangeException(SR.GetString(SR.SqlDataSource_InvalidMode, ID));
                }

                if (DataSourceMode != value) {
                    _dataSourceMode = value;
                    RaiseDataSourceChangedEvent(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        /// The command to execute when Delete() is called on the SqlDataSourceView.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_DeleteCommand),
        ]
        public string DeleteCommand {
            get {
                return GetView().DeleteCommand;
            }
            set {
                GetView().DeleteCommand = value;
            }
        }

        /// <devdoc>
        /// The type of the delete command (command text or stored procedure).
        /// </devdoc>
        [
        DefaultValue(SqlDataSourceCommandType.Text),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_DeleteCommandType),
        ]
        public SqlDataSourceCommandType DeleteCommandType {
            get {
                return GetView().DeleteCommandType;
            }
            set {
                GetView().DeleteCommandType = value;
            }
        }


        /// <devdoc>
        /// Collection of parameters used in Delete().
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_DeleteParameters),
        ]
        public ParameterCollection DeleteParameters {
            get {
                return GetView().DeleteParameters;
            }
        }


        /// <devdoc>
        /// Whether caching is enabled for this data source.
        /// </devdoc>
        [
        DefaultValue(false),
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
        /// The filter to apply when Select() is called on the SqlDataSourceView.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_FilterExpression),
        ]
        public string FilterExpression {
            get {
                return GetView().FilterExpression;
            }
            set {
                GetView().FilterExpression = value;
            }
        }


        /// <devdoc>
        /// Collection of parameters used in the FilterExpression property.
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_FilterParameters),
        ]
        public ParameterCollection FilterParameters {
            get {
                return GetView().FilterParameters;
            }
        }


        /// <devdoc>
        /// The command to execute when Insert() is called on the SqlDataSourceView.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_InsertCommand),
        ]
        public string InsertCommand {
            get {
                return GetView().InsertCommand;
            }
            set {
                GetView().InsertCommand = value;
            }
        }

        /// <devdoc>
        /// The type of the insert command (command text or stored procedure).
        /// </devdoc>
        [
        DefaultValue(SqlDataSourceCommandType.Text),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_InsertCommandType),
        ]
        public SqlDataSourceCommandType InsertCommandType {
            get {
                return GetView().InsertCommandType;
            }
            set {
                GetView().InsertCommandType = value;
            }
        }


        /// <devdoc>
        /// Collection of values used in Insert().
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_InsertParameters),
        ]
        public ParameterCollection InsertParameters {
            get {
                return GetView().InsertParameters;
            }
        }

        /// <devdoc>
        /// The format string applied to the names of the old values parameters
        /// </devdoc>
        [
        DefaultValue("{0}"),
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_OldValuesParameterFormatString),
        ]
        public string OldValuesParameterFormatString {
            get {
                return GetView().OldValuesParameterFormatString;
            }
            set {
                GetView().OldValuesParameterFormatString = value;
            }
        }


        /// <devdoc>
        /// Gets/sets the ADO.net managed provider name.
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.WebControls.DataProviderNameConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_ProviderName),
        ]
        public virtual string ProviderName {
            get {
                return (_providerName == null ? String.Empty : _providerName);
            }
            set {
                if (ProviderName != value) {
                    _providerFactory = null;
                    _providerName = value;
                    RaiseDataSourceChangedEvent(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        /// The command to execute when Select() is called on the SqlDataSourceView.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_SelectCommand),
        ]
        public string SelectCommand {
            get {
                return GetView().SelectCommand;
            }
            set {
                GetView().SelectCommand = value;
            }
        }

        /// <devdoc>
        /// The type of the select command (command text or stored procedure).
        /// </devdoc>
        [
        DefaultValue(SqlDataSourceCommandType.Text),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_SelectCommandType),
        ]
        public SqlDataSourceCommandType SelectCommandType {
            get {
                return GetView().SelectCommandType;
            }
            set {
                GetView().SelectCommandType = value;
            }
        }


        /// <devdoc>
        /// The command to execute when Select is called on the SqlDataSourceView, requesting the total number of rows.
        /// </devdoc>
        /* Commented out until we add paging support back into SqlDataSource
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_SelectCountCommand),
        ]
        public string SelectCountCommand {
            get {
                return GetView().SelectCountCommand;
            }
            set {
                GetView().SelectCountCommand = value;
            }
        }*/


        /// <devdoc>
        /// Collection of parameters used in Select().
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_SelectParameters),
        ]
        public ParameterCollection SelectParameters {
            get {
                return GetView().SelectParameters;
            }
        }


        /// <devdoc>
        /// The name of the parameter in the SelectCommand that specifies the
        /// sort expression. This parameter's value will be automatically set
        /// at runtime with the appropriate sort expression. This is only
        /// supported for stored procedure commands.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_SortParameterName),
        ]
        public string SortParameterName {
            get {
                return GetView().SortParameterName;
            }
            set {
                GetView().SortParameterName = value;
            }
        }

        /// <devdoc>
        /// Gets the SQL data source cache object.
        /// </devdoc>
        private SqlDataSourceCache SqlDataSourceCache {
            get {
                SqlDataSourceCache sqlCache = Cache as SqlDataSourceCache;
                if (sqlCache == null) {
                    throw new NotSupportedException(SR.GetString(SR.SqlDataSource_SqlCacheDependencyNotSupported, ID));
                }
                return sqlCache;
            }
        }

        /// <devdoc>
        /// A semi-colon delimited string indicating which databases to use for the dependency in the format "database1:table1;database2:table2".
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Cache"),
        WebSysDescription(SR.SqlDataSourceCache_SqlCacheDependency),
        ]
        public virtual string SqlCacheDependency {
            get {
                return SqlDataSourceCache.SqlCacheDependency;

            }
            set {
                SqlDataSourceCache.SqlCacheDependency = value;
            }
        }


        /// <devdoc>
        /// The command to execute when Update() is called on the SqlDataSourceView.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_UpdateCommand),
        ]
        public string UpdateCommand {
            get {
                return GetView().UpdateCommand;
            }
            set {
                GetView().UpdateCommand = value;
            }
        }

        /// <devdoc>
        /// The type of the update command (command text or stored procedure).
        /// </devdoc>
        [
        DefaultValue(SqlDataSourceCommandType.Text),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_UpdateCommandType),
        ]
        public SqlDataSourceCommandType UpdateCommandType {
            get {
                return GetView().UpdateCommandType;
            }
            set {
                GetView().UpdateCommandType = value;
            }
        }


        /// <devdoc>
        /// Collection of parameters used in Update().
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_UpdateParameters),
        ]
        public ParameterCollection UpdateParameters {
            get {
                return GetView().UpdateParameters;
            }
        }



        /// <devdoc>
        /// This event is raised after the Delete operation has completed.
        /// Handle this event if you need to examine the values of output parameters.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Deleted),
        ]
        public event SqlDataSourceStatusEventHandler Deleted {
            add {
                GetView().Deleted += value;
            }
            remove {
                GetView().Deleted -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the Delete operation has been executed.
        /// Handle this event if you want to perform additional initialization operations
        /// that are specific to your application. You can also handle this event if you
        /// need to validate the values of parameters or change their values.
        /// When this event is raised, the database connection is not open yet, and you
        /// can cancel the event by setting the Cancel property of the DataCommandEventArgs
        /// to true.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Deleting),
        ]
        public event SqlDataSourceCommandEventHandler Deleting {
            add {
                GetView().Deleting += value;
            }
            remove {
                GetView().Deleting -= value;
            }
        }

        /// <devdoc>
        /// This event is raised before the Filter operation takes place.
        /// Handle this event if you want to perform validation operations on
        /// the parameter values. This event is only raised if the FilterExpression
        /// is set. If the Cancel property of the event arguments is set to true,
        /// the Select operation is aborted and the operation will return null.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Filtering),
        ]
        public event SqlDataSourceFilteringEventHandler Filtering {
            add {
                GetView().Filtering += value;
            }
            remove {
                GetView().Filtering -= value;
            }
        }


        /// <devdoc>
        /// This event is raised after the Insert operation has completed.
        /// Handle this event if you need to examine the values of output parameters.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Inserted),
        ]
        public event SqlDataSourceStatusEventHandler Inserted {
            add {
                GetView().Inserted += value;
            }
            remove {
                GetView().Inserted -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the Insert operation has been executed.
        /// Handle this event if you want to perform additional initialization operations
        /// that are specific to your application. You can also handle this event if you
        /// need to validate the values of parameters or change their values.
        /// When this event is raised, the database connection is not open yet, and you
        /// can cancel the event by setting the Cancel property of the DataCommandEventArgs
        /// to true.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Inserting),
        ]
        public event SqlDataSourceCommandEventHandler Inserting {
            add {
                GetView().Inserting += value;
            }
            remove {
                GetView().Inserting -= value;
            }
        }


        /// <devdoc>
        /// This event is raised after the Select operation has completed.
        /// Handle this event if you need to examine the values of output parameters.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_Selected),
        ]
        public event SqlDataSourceStatusEventHandler Selected {
            add {
                GetView().Selected += value;
            }
            remove {
                GetView().Selected -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the Select operation has been executed.
        /// Handle this event if you want to perform additional initialization operations
        /// that are specific to your application. You can also handle this event if you
        /// need to validate the values of parameters or change their values.
        /// When this event is raised, the database connection is not open yet, and you
        /// can cancel the event by setting the Cancel property of the DataCommandEventArgs
        /// to true.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.SqlDataSource_Selecting),
        ]
        public event SqlDataSourceSelectingEventHandler Selecting {
            add {
                GetView().Selecting += value;
            }
            remove {
                GetView().Selecting -= value;
            }
        }


        /// <devdoc>
        /// This event is raised after the Update operation has completed.
        /// Handle this event if you need to examine the values of output parameters.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Updated),
        ]
        public event SqlDataSourceStatusEventHandler Updated {
            add {
                GetView().Updated += value;
            }
            remove {
                GetView().Updated -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the Update operation has been executed.
        /// Handle this event if you want to perform additional initialization operations
        /// that are specific to your application. You can also handle this event if you
        /// need to validate the values of parameters or change their values.
        /// When this event is raised, the database connection is not open yet, and you
        /// can cancel the event by setting the Cancel property of the DataCommandEventArgs
        /// to true.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Updating),
        ]
        public event SqlDataSourceCommandEventHandler Updating {
            add {
                GetView().Updating += value;
            }
            remove {
                GetView().Updating -= value;
            }
        }


        /// <devdoc>
        /// Creates a unique cache key for this data source's data.
        /// </devdoc>
        internal string CreateCacheKey(int startRowIndex, int maximumRows) {
            StringBuilder sb = CreateRawCacheKey();

            sb.Append(startRowIndex.ToString(CultureInfo.InvariantCulture));
            sb.Append(':');
            sb.Append(maximumRows.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        /// <devdoc>
        /// Creates a DbConnection to the database based on the ProviderName.
        /// </devdoc>
        internal DbConnection CreateConnection(string connectionString) {
            DbProviderFactory factory = GetDbProviderFactorySecure();
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        /// <devdoc>
        /// Creates a DbCommand based on the ProviderName.
        /// </devdoc>
        internal DbCommand CreateCommand(string commandText, DbConnection connection) {
            DbProviderFactory factory = GetDbProviderFactorySecure();
            DbCommand command = factory.CreateCommand();
            command.CommandText = commandText;
            command.Connection = connection;
            return command;
        }

        /// <devdoc>
        /// Creates an DbDataAdapter based on the ProviderName.
        /// </devdoc>
        internal DbDataAdapter CreateDataAdapter(DbCommand command) {
            DbProviderFactory factory = GetDbProviderFactorySecure();
            DbDataAdapter dataAdapter = factory.CreateDataAdapter();
            dataAdapter.SelectCommand = command;
            return dataAdapter;
        }

        /// <devdoc>
        /// Creates a SqlDataSourceView. Derived classes should override this if they need to return
        /// custom view types.
        /// </devdoc>
        protected virtual SqlDataSourceView CreateDataSourceView(string viewName) {
            return new SqlDataSourceView(this, viewName, Context);
        }

        /// <devdoc>
        /// Creates the cache key for the master (parent) cache entry, which holds the total row count.
        /// </devdoc>
        internal string CreateMasterCacheKey() {
            return CreateRawCacheKey().ToString();
        }

        /// <devdoc>
        /// Creates an IDataParameter based on the ProviderName.
        /// </devdoc>
        internal DbParameter CreateParameter(string parameterName, object parameterValue) {
            DbProviderFactory factory = GetDbProviderFactorySecure();
            DbParameter parameter = factory.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            return parameter;
        }

        /// <devdoc>
        /// Returns the string for the raw master cache key.
        /// </devdoc>
        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "This is specifically on SqlDataSource type which is not a com interop type.")]
        private StringBuilder CreateRawCacheKey()
        {
            // Note: The cache key will contain information such as server names and
            // passwords, however it will be stored in the internal cache, which is
            // not accessible to page developers, so it is secure.
            StringBuilder sb = new StringBuilder(CacheInternal.PrefixDataSourceControl, 1024);
            sb.Append(GetType().GetHashCode().ToString(CultureInfo.InvariantCulture));

            sb.Append(CacheDuration.ToString(CultureInfo.InvariantCulture));
            sb.Append(':');
            sb.Append(((int)CacheExpirationPolicy).ToString(CultureInfo.InvariantCulture));

            SqlDataSourceCache sqlCache = Cache as SqlDataSourceCache;
            if (sqlCache != null) {
                sb.Append(":");
                sb.Append(sqlCache.SqlCacheDependency);
            }

            sb.Append(":");
            sb.Append(ConnectionString);
            sb.Append(":");
            sb.Append(SelectCommand);
            //sb.Append(SelectCountCommand);

            // Append parameter names and values
            if (SelectParameters.Count > 0) {
                sb.Append("?");
                IDictionary parameters = SelectParameters.GetValues(Context, this);
                foreach (DictionaryEntry entry in parameters) {
                    sb.Append(entry.Key.ToString());
                    if ((entry.Value != null) && (entry.Value != DBNull.Value)) {
                        sb.Append("=");
                        sb.Append(entry.Value.ToString());
                    }
                    else {
                        if (entry.Value == DBNull.Value) {
                            sb.Append("(dbnull)");
                        }
                        else {
                            sb.Append("(null)");
                        }
                    }
                    sb.Append("&");
                }
            }
            return sb;
        }

        /// <devdoc>
        /// Deletes rows from the data source using the parameters specified in the DeleteParameters collection.
        /// </devdoc>
        public int Delete() {
            return GetView().Delete(null, null);
        }

        /// <devdoc>
        /// Gets the DbProviderFactory associated with the provider type specified in the ProviderName property.
        /// If no provider is specified, the System.Data.SqlClient factory is used.
        /// </devdoc>
        protected virtual DbProviderFactory GetDbProviderFactory() {
            string providerName = ProviderName;

            // Default to SQL provider
            if (String.IsNullOrEmpty(providerName)) {
                // use DefaultProviderName instance
                return SqlClientFactory.Instance;
            }
            else {
                return DbProviderFactories.GetFactory(providerName);
            }
        }

        /// <devdoc>
        /// Gets the DbProviderFactory and performs a security check.
        /// </devdoc>
        private DbProviderFactory GetDbProviderFactorySecure() {
            if (_providerFactory == null) {
                _providerFactory = GetDbProviderFactory();

                Debug.Assert(_providerFactory != null);

                if (!HttpRuntime.DisableProcessRequestInApplicationTrust) {
                    // Perform security check if we're not running in application trust
                    if (!HttpRuntime.ProcessRequestInApplicationTrust && !HttpRuntime.HasDbPermission(_providerFactory)) {
                        throw new HttpException(SR.GetString(SR.SqlDataSource_NoDbPermission, _providerFactory.GetType().Name, ID));
                    }
                }
            }
            return _providerFactory;
        }

        /// <devdoc>
        /// Dynamically creates the default (and only) SqlDataSourceView on demand.
        /// </devdoc>
        private SqlDataSourceView GetView() {
            if (_view == null) {
                _view = CreateDataSourceView(DefaultViewName);
                if (_cachedSelectCommand != null) {
                    // If there was a cached select command from the constructor, set it
                    _view.SelectCommand = _cachedSelectCommand;
                }

                if (IsTrackingViewState) {
                    ((IStateManager)_view).TrackViewState();
                }
            }

            return _view;
        }

        /// <devdoc>
        /// Gets the view associated with this connection.
        /// SqlDataSource only supports a single view, so the viewName parameter is ignored.
        /// </devdoc>
        protected override DataSourceView GetView(string viewName) {
            if (viewName == null || (viewName.Length != 0 && !String.Equals(viewName, DefaultViewName, StringComparison.OrdinalIgnoreCase))) {
                throw new ArgumentException(SR.GetString(SR.DataSource_InvalidViewName, ID, DefaultViewName), "viewName");
            }

            return GetView();
        }

        /// <devdoc>
        /// Returns an ICollection of the names of all the views. In this case there is only one view called "Table".
        /// </devdoc>
        protected override ICollection GetViewNames() {
            if (_viewNames == null) {
                _viewNames = new string[1] { DefaultViewName };
            }
            return _viewNames;
        }

        /// <devdoc>
        /// Inserts a new row with names and values specified the InsertValues collection.
        /// </devdoc>
        public int Insert() {
            return GetView().Insert(null);
        }
        
        /// <devdoc>
        /// Invalidates a cache entry.
        /// </devdoc>
        internal void InvalidateCacheEntry() {
            string key = CreateMasterCacheKey();
            DataSourceCache cache = Cache;
            Debug.Assert(cache != null);
            cache.Invalidate(key);
        }
        
        /// <devdoc>
        /// Event handler for the Page's LoadComplete event.
        /// Updates the parameters' values to possibly raise a DataSourceChanged event, causing bound controls to re-databind.
        /// </devdoc>
        private void LoadCompleteEventHandler(object sender, EventArgs e) {
            SelectParameters.UpdateValues(Context, this);
            FilterParameters.UpdateValues(Context, this);
        }

        /// <devdoc>
        /// Loads data from the cache.
        /// </devdoc>
        internal object LoadDataFromCache(int startRowIndex, int maximumRows) {
            string key = CreateCacheKey(startRowIndex, maximumRows);
            return Cache.LoadDataFromCache(key);
        }
        
        /// <devdoc>
        /// Loads data from the cache.
        /// </devdoc>
        internal int LoadTotalRowCountFromCache() {
            string key = CreateMasterCacheKey();
            object data = Cache.LoadDataFromCache(key);
            if (data is int)
                return (int)data;
            return -1;
        }

        /// <devdoc>
        /// Loads view state.
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            Pair myState = (Pair)savedState;

            if (savedState == null) {
                base.LoadViewState(null);
            }
            else {
                base.LoadViewState(myState.First);

                if (myState.Second != null) {
                    ((IStateManager)GetView()).LoadViewState(myState.Second);
                }
            }
        }

        /// <devdoc>
        /// Adds LoadComplete event handler to the page.
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (Page != null) {
                Page.LoadComplete += new EventHandler(LoadCompleteEventHandler);
            }
        }

        /// <devdoc>
        /// Saves paged data to cache, creating a dependency on the updated row count
        /// </devdoc>
        internal virtual void SaveDataToCache(int startRowIndex, int maximumRows, object data, CacheDependency dependency) {
            string key = CreateCacheKey(startRowIndex, maximumRows);
            string parentKey = CreateMasterCacheKey();
            if (Cache.LoadDataFromCache(parentKey) == null) {
                Cache.SaveDataToCache(parentKey, -1, dependency);
            }
            CacheDependency cacheDependency = new CacheDependency(0, new string[0], new string[] {parentKey});
            Cache.SaveDataToCache(key, data, cacheDependency);
        }

        /// <devdoc>
        /// Saves the total row count to cache.
        /// </devdoc>
        /*internal virtual void SaveTotalRowCountToCache(int totalRowCount) {
            string key = CreateMasterCacheKey();
            Cache.SaveDataToCache(key, totalRowCount);
        }*/

        /// <devdoc>
        /// Saves view state.
        /// </devdoc>
        protected override object SaveViewState() {
            Pair myState = new Pair();

            myState.First = base.SaveViewState();

            if (_view != null) {
                myState.Second = ((IStateManager)_view).SaveViewState();
            }

            if ((myState.First == null) &&
                (myState.Second == null)) {
                return null;
            }

            return myState;
        }

        /// <devdoc>
        /// Returns all the rows of the datasource.
        /// Parameters are taken from the Parameters property collection.
        /// If SqlDataSourceMode is set to DataSet then a DataView is returned.
        /// If SqlDataSourceMode is set to DataReader then a DataReader is returned, and it must be closed when done.
        /// </devdoc>
        public IEnumerable Select(DataSourceSelectArguments arguments) {
            return GetView().Select(arguments);
        }

        /// <devdoc>
        /// Starts tracking of view state.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_view != null) {
                ((IStateManager)_view).TrackViewState();
            }
        }

        /// <devdoc>
        /// Updates rows matching the parameters specified in the UpdateParameters collection with new values specified the UpdateValues collection.
        /// </devdoc>
        public int Update() {
            return GetView().Update(null, null, null);
        }
    }
}

