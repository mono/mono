//------------------------------------------------------------------------------
// <copyright file="ObjectDataSource.cs" company="Microsoft">
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
    using System.Text;
    using System.Web.Caching;
    using System.Web.UI;


    /// <devdoc>
    /// Represents a data source that calls methods on a business object in order to
    /// perform the Delete, Insert, Select, and Update operations. The business object
    /// is specified in the TypeName property.
    /// </devdoc>
    [
    DefaultEvent("Selecting"),
    DefaultProperty("TypeName"),
    Designer("System.Web.UI.Design.WebControls.ObjectDataSourceDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxBitmap(typeof(ObjectDataSource)),
    WebSysDescription(SR.ObjectDataSource_Description),
    WebSysDisplayName(SR.ObjectDataSource_DisplayName)
    ]
    public class ObjectDataSource : DataSourceControl {

        private const string DefaultViewName = "DefaultView";

        private SqlDataSourceCache _cache;
        private ObjectDataSourceView _view;
        private ICollection _viewNames;



        /// <devdoc>
        /// Creates a new instance of ObjectDataSource.
        /// </devdoc>
        public ObjectDataSource() {
        }


        /// <devdoc>
        /// Creates a new instance of ObjectDataSource with a specified type name and select method.
        /// </devdoc>
        public ObjectDataSource(string typeName, string selectMethod) {
            TypeName = typeName;
            SelectMethod = selectMethod;
        }



        /// <devdoc>
        /// Specifies the cache settings for this data source. For the cache to
        /// work, the SelectMethod must return a DataSet.
        /// </devdoc>
        internal SqlDataSourceCache Cache {
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
        /// Whether the commands pass old values in the parameter collection.
        /// </devdoc>
        [
        DefaultValue(ConflictOptions.OverwriteChanges),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_ConflictDetection),
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
        /// Whether null values passed into insert/update/delete operations
        /// will be converted to System.DbNull.
        /// </devdoc>
        [
        DefaultValue(false),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_ConvertNullToDBNull),
        ]
        public bool ConvertNullToDBNull {
            get {
                return GetView().ConvertNullToDBNull;
            }
            set {
                GetView().ConvertNullToDBNull = value;
            }
        }


        /// <devdoc>
        /// An optional type that is used for update, insert, and delete
        /// scenarios where the object's methods take in an aggregate object
        /// rather than one parameter for each property in the selected data.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_DataObjectTypeName),
        ]
        public string DataObjectTypeName {
            get {
                return GetView().DataObjectTypeName;
            }
            set {
                GetView().DataObjectTypeName = value;
            }
        }


        /// <devdoc>
        /// The method to execute when Delete() is called.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_DeleteMethod),
        ]
        public string DeleteMethod {
            get {
                return GetView().DeleteMethod;
            }
            set {
                GetView().DeleteMethod = value;
            }
        }

        /// <devdoc>
        // Collection of parameters used when calling the DeleteMethod. These parameters are merged with the parameters provided by data-bound controls.

        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_DeleteParameters),
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
        /// Indicates whether the Select method supports paging. If this is set to true, the
        /// StartRowIndexParameterName and MaximumRowsParameterName properties must be set to the
        /// names of the parameters of the Select method that accept the values for the starting
        /// record to retrieve and the number of records to retrieve.
        /// </devdoc>
        [
        DefaultValue(false),
        WebCategory("Paging"),
        WebSysDescription(SR.ObjectDataSource_EnablePaging),
        ]
        public bool EnablePaging {
            get {
                return GetView().EnablePaging;
            }
            set {
                GetView().EnablePaging = value;
            }
        }


        /// <devdoc>
        /// Filter expression used when Select() is called. Filtering is only available when the SelectMethod returns a DataSet.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_FilterExpression),
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
        /// Collection of parameters used in the FilterExpression property. Filtering is only available when the SelectMethod returns a DataSet.
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_FilterParameters),
        ]
        public ParameterCollection FilterParameters {
            get {
                return GetView().FilterParameters;
            }
        }


        /// <devdoc>
        /// The method to execute when Insert() is called.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_InsertMethod),
        ]
        public string InsertMethod {
            get {
                return GetView().InsertMethod;
            }
            set {
                GetView().InsertMethod = value;
            }
        }


        /// <devdoc>
        /// Collection of values used when calling the InsertMethod. These parameters are merged with the parameters provided by data-bound controls.
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_InsertParameters),
        ]
        public ParameterCollection InsertParameters {
            get {
                return GetView().InsertParameters;
            }
        }

        /// <devdoc>
        /// When EnablePaging is set to true, this property indicates the parameter of the Select
        /// method that accepts the value for the number of records to retrieve.
        /// </devdoc>
        [
        DefaultValue("maximumRows"),
        WebCategory("Paging"),
        WebSysDescription(SR.ObjectDataSource_MaximumRowsParameterName),
        ]
        public string MaximumRowsParameterName {
            get {
                return GetView().MaximumRowsParameterName;
            }
            set {
                GetView().MaximumRowsParameterName = value;
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
        /// The command to execute when Select is called on the ObjectDataSourceView, requesting the total number of rows.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Paging"),
        WebSysDescription(SR.ObjectDataSource_SelectCountMethod),
        ]
        public string SelectCountMethod {
            get {
                return GetView().SelectCountMethod;
            }
            set {
                GetView().SelectCountMethod = value;
            }
        }


        /// <devdoc>
        /// The method to execute when Select() is called.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_SelectMethod),
        ]
        public string SelectMethod {
            get {
                return GetView().SelectMethod;
            }
            set {
                GetView().SelectMethod = value;
            }
        }


        /// <devdoc>
        /// Collection of parameters used when calling the SelectMethod.
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_SelectParameters),
        ]
        public ParameterCollection SelectParameters {
            get {
                return GetView().SelectParameters;
            }
        }


        /// <devdoc>
        /// The name of the parameter in the SelectMethod that specifies the
        /// sort expression. This parameter's value will be automatically set
        /// at runtime with the appropriate sort expression.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_SortParameterName),
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
        /// A semi-colon delimited string indicating which databases to use for the dependency in the format "database1:table1;database2:table2".
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Cache"),
        WebSysDescription(SR.SqlDataSourceCache_SqlCacheDependency),
        ]
        public virtual string SqlCacheDependency {
            get {
                return Cache.SqlCacheDependency;
                
            }
            set {
                Cache.SqlCacheDependency = value;
            }
        }

        /// <devdoc>
        /// When EnablePaging is set to true, this property indicates the parameter of the Select
        /// method that accepts the value for the number of first record to retrieve.
        /// </devdoc>
        [
        DefaultValue("startRowIndex"),
        WebCategory("Paging"),
        WebSysDescription(SR.ObjectDataSource_StartRowIndexParameterName),
        ]
        public string StartRowIndexParameterName {
            get {
                return GetView().StartRowIndexParameterName;
            }
            set {
                GetView().StartRowIndexParameterName = value;
            }
        }


        /// <devdoc>
        /// The type that contains the methods specified in this control.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_TypeName),
        ]
        public string TypeName {
            get {
                return GetView().TypeName;
            }
            set {
                GetView().TypeName = value;
            }
        }
        

        /// <devdoc>
        /// The method to execute when Update() is called.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_UpdateMethod),
        ]
        public string UpdateMethod {
            get {
                return GetView().UpdateMethod;
            }
            set {
                GetView().UpdateMethod = value;
            }
        }
        

        /// <devdoc>
        /// Collection of parameters and values used when calling the UpdateMethod. These parameters are merged with the parameters provided by data-bound controls.
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_UpdateParameters),
        ]
        public ParameterCollection UpdateParameters {
            get {
                return GetView().UpdateParameters;
            }
        }

        /// <summary>
        /// Indicates which <see cref='System.Globalization.CultureInfo'/> is used by ObjectDataSource
        /// when converting string values to actual types of properties while constructing an object of type 
        /// <see cref='System.Web.UI.WebControls.ObjectDataSource.DataObjectTypeName'/>.
        /// </summary>
        [
        DefaultValue(ParsingCulture.Invariant),
        WebCategory("Behavior"),
        WebSysDescription(SR.ObjectDataSource_ParsingCulture)
        ]
        public ParsingCulture ParsingCulture {
            get {
                return GetView().ParsingCulture;
            }
            set {
                GetView().ParsingCulture = value;
            }
        }



        /// <devdoc>
        /// This event is raised after the Delete operation has completed.
        /// Handle this event if you need to examine the return values of
        /// the method call, or examine an exception that may have been thrown.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Deleted),
        ]
        public event ObjectDataSourceStatusEventHandler Deleted {
            add {
                GetView().Deleted += value;
            }
            remove {
                GetView().Deleted -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the Delete operation has been executed.
        /// Handle this event if you need to validate the values of parameters or
        /// change their values.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Deleting),
        ]
        public event ObjectDataSourceMethodEventHandler Deleting {
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
        public event ObjectDataSourceFilteringEventHandler Filtering {
            add {
                GetView().Filtering += value;
            }
            remove {
                GetView().Filtering -= value;
            }
        }


        /// <devdoc>
        /// This event is raised after the Insert operation has completed.
        /// Handle this event if you need to examine the return values of
        /// the method call, or examine an exception that may have been thrown.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Inserted),
        ]
        public event ObjectDataSourceStatusEventHandler Inserted {
            add {
                GetView().Inserted += value;
            }
            remove {
                GetView().Inserted -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the Insert operation has been executed.
        /// Handle this event if you need to validate the values of parameters or
        /// change their values.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Inserting),
        ]
        public event ObjectDataSourceMethodEventHandler Inserting {
            add {
                GetView().Inserting += value;
            }
            remove {
                GetView().Inserting -= value;
            }
        }


        /// <devdoc>
        /// This event is raised after the instance of the object has been created.
        /// Handle this event if you need to set additional properties on the
        /// object before any other methods are called. This event will not be
        /// raised if a custom instance was provided in the ObjectCreating event.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_ObjectCreated),
        ]
        public event ObjectDataSourceObjectEventHandler ObjectCreated {
            add {
                GetView().ObjectCreated += value;
            }
            remove {
                GetView().ObjectCreated -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the instance of the object has been created.
        /// Handle this event if you need to call a non-default constructor on the
        /// object. Set the ObjectInstance property of the EventArgs with the
        /// custom instance. If this is set, the ObjectCreated event will not be
        /// raised.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_ObjectCreating),
        ]
        public event ObjectDataSourceObjectEventHandler ObjectCreating {
            add {
                GetView().ObjectCreating += value;
            }
            remove {
                GetView().ObjectCreating -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the instance of the object is disposed.
        /// Handle this event if you need to retrieve properties on the
        /// object before it is disposed. If the object implements the IDispoable
        /// interface, then the Dispose() method will be called automatically.
        /// Set the Cancel property of the event args to true if you do not want
        /// IDisposable.Dispose() to be called automatically.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_ObjectDisposing),
        ]
        public event ObjectDataSourceDisposingEventHandler ObjectDisposing {
            add {
                GetView().ObjectDisposing += value;
            }
            remove {
                GetView().ObjectDisposing -= value;
            }
        }


        /// <devdoc>
        /// This event is raised after the Select operation has completed.
        /// Handle this event if you need to examine the return values of
        /// the method call, or examine an exception that may have been thrown.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_Selected),
        ]
        public event ObjectDataSourceStatusEventHandler Selected {
            add {
                GetView().Selected += value;
            }
            remove {
                GetView().Selected -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the Select operation has been executed.
        /// Handle this event if you need to validate the values of parameters or
        /// change their values.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.ObjectDataSource_Selecting),
        ]
        public event ObjectDataSourceSelectingEventHandler Selecting {
            add {
                GetView().Selecting += value;
            }
            remove {
                GetView().Selecting -= value;
            }
        }


        /// <devdoc>
        /// This event is raised after the Update operation has completed.
        /// Handle this event if you need to examine the return values of
        /// the method call, or examine an exception that may have been thrown.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Updated),
        ]
        public event ObjectDataSourceStatusEventHandler Updated {
            add {
                GetView().Updated += value;
            }
            remove {
                GetView().Updated -= value;
            }
        }


        /// <devdoc>
        /// This event is raised before the Update operation has been executed.
        /// Handle this event if you need to validate the values of parameters or
        /// change their values.
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_Updating),
        ]
        public event ObjectDataSourceMethodEventHandler Updating {
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

            sb.Append(':');
            sb.Append(startRowIndex.ToString(CultureInfo.InvariantCulture));
            sb.Append(':');
            sb.Append(maximumRows.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        /// <devdoc>
        /// Creates the cache key for the master (parent) cache entry, which holds the total row count.
        /// </devdoc>
        internal string CreateMasterCacheKey() {
            return CreateRawCacheKey().ToString();
        }

        /// <devdoc>
        /// Returns the string for the raw (unhashed) master cache key.
        /// </devdoc>
        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "This is specifically on ObjectDataSource type which is not a com interop type.")]
        private StringBuilder CreateRawCacheKey() {
            // Note: The cache key will contain information such as type names and
            // method names, however it will be stored in the internal cache, which is
            // not accessible to page developers, so it is secure.
            StringBuilder sb = new StringBuilder(CacheInternal.PrefixDataSourceControl, 1024);
            sb.Append(GetType().GetHashCode().ToString(CultureInfo.InvariantCulture));
            sb.Append(":");
            sb.Append(CacheDuration.ToString(CultureInfo.InvariantCulture));
            sb.Append(':');
            sb.Append(((int)CacheExpirationPolicy).ToString(CultureInfo.InvariantCulture));
            sb.Append(":");
            sb.Append(SqlCacheDependency);
            sb.Append(":");
            sb.Append(TypeName);
            sb.Append(":");
            sb.Append(SelectMethod);

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
        /// Dynamically creates the default (and only) ObjectDataSourceView on demand.
        /// </devdoc>
        private ObjectDataSourceView GetView() {
            if (_view == null) {
                _view = new ObjectDataSourceView(this, DefaultViewName, Context);

                if (IsTrackingViewState) {
                    ((IStateManager)_view).TrackViewState();
                }
            }

            return _view;
        }


        /// <devdoc>
        /// Gets the view associated with this data source.
        /// </devdoc>
        protected override DataSourceView GetView(string viewName) {
            if (viewName == null || (viewName.Length != 0 && !String.Equals(viewName, DefaultViewName, StringComparison.OrdinalIgnoreCase))) {
                throw new ArgumentException(SR.GetString(SR.DataSource_InvalidViewName, ID, DefaultViewName), "viewName");
            }

            return GetView();
        }


        /// <devdoc>
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
            Cache.Invalidate(key);
        }

        /// <devdoc>
        /// Event handler for the Page's LoadComplete event.
        /// Updates the parameters' values to possibly raise a DataSourceViewChanged event, causing bound controls to re-databind.
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
        internal void SaveDataToCache(int startRowIndex, int maximumRows, object data) {
            string key = CreateCacheKey(startRowIndex, maximumRows);
            string parentKey = CreateMasterCacheKey();
            if (Cache.LoadDataFromCache(parentKey) == null) {
                Cache.SaveDataToCache(parentKey, -1);
            }
            CacheDependency cacheDependency = new CacheDependency(0, new string[0], new string[] { parentKey });
            Cache.SaveDataToCache(key, data, cacheDependency);
        }

        /// <devdoc>
        /// Saves the total row count to cache.
        /// </devdoc>
        internal void SaveTotalRowCountToCache(int totalRowCount) {
            string key = CreateMasterCacheKey();
            Cache.SaveDataToCache(key, totalRowCount);
        }


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
        /// Parameters are taken from the SelectParameters property collection.
        /// </devdoc>
        public IEnumerable Select() {
            return GetView().Select(DataSourceSelectArguments.Empty);
        }
        

        /// <devdoc>
        /// Starts tracking view state.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_view != null) {
                ((IStateManager)_view).TrackViewState();
            }
        }


        /// <devdoc>
        /// Updates rows in the data source indicated by the parameters in the UpdateParameters collection.
        /// </devdoc>
        public int Update() {
            return GetView().Update(null, null, null);
        }
    }
}

