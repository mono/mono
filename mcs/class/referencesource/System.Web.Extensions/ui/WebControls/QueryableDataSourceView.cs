namespace System.Web.UI.WebControls {
    using System.Web.UI.WebControls.Expressions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;


    public abstract class QueryableDataSourceView : DataSourceView, IStateManager {
        //basic query parameters for any Queryable source
        private ParameterCollection _whereParameters;
        private ParameterCollection _orderByParameters;
        private ParameterCollection _orderGroupsByParameters;
        private ParameterCollection _selectNewParameters;
        private ParameterCollection _groupByParameters;

        // CUD operations
        private ParameterCollection _deleteParameters;
        private ParameterCollection _updateParameters;
        private ParameterCollection _insertParameters;

        private HttpContext _context;
        private DataSourceControl _owner;
        private IDynamicQueryable _queryable;

        private string _groupBy;
        private string _orderBy;
        private string _orderGroupsBy;
        private string _selectNew;
        private string _where;

        private bool _autoGenerateOrderByClause;
        private bool _autoGenerateWhereClause;
        private bool _autoPage = true;
        private bool _autoSort = true;
        private bool _isTracking;

        protected static readonly object EventSelected = new object();
        protected static readonly object EventSelecting = new object();
        private static readonly object EventQueryCreated = new object();

        // using Hashtable for original values so that ObjectStateFormatter will serialize it properly in ViewState.
        private Hashtable _originalValues;


        protected QueryableDataSourceView(DataSourceControl owner, string viewName, HttpContext context)
            : this(owner, viewName, context, new DynamicQueryableWrapper()) {
            _context = context;
            _owner = owner;
        }

        internal QueryableDataSourceView(DataSourceControl owner, string viewName, HttpContext context, IDynamicQueryable queryable)
            : base(owner, viewName) {
            _context = context;
            _queryable = queryable;
            _owner = owner;
        }

        public bool AutoGenerateOrderByClause {
            get {
                return _autoGenerateOrderByClause;
            }
            set {
                if (_autoGenerateOrderByClause != value) {
                    _autoGenerateOrderByClause = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public bool AutoGenerateWhereClause {
            get {
                return _autoGenerateWhereClause;
            }
            set {
                if (_autoGenerateWhereClause != value) {
                    _autoGenerateWhereClause = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public virtual bool AutoPage {
            get {
                return _autoPage;
            }
            set {
                if (_autoPage != value) {
                    _autoPage = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public virtual bool AutoSort {
            get {
                return _autoSort;
            }
            set {
                if (_autoSort != value) {
                    _autoSort = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public override bool CanDelete {
            get {
                return false;
            }
        }

        public override bool CanInsert {
            get {
                return false;
            }
        }

        // When AutoPage is false the user should manually page in the Selecting event.
        public override bool CanPage {
            get {
                return true;
            }
        }

        // When AutoPage is false the user must set the total row count in the Selecting event.
        public override bool CanRetrieveTotalRowCount {
            get {
                return true;
            }
        }

        // When AutoSort is false the user should manually sort in the Selecting event.
        public override bool CanSort {
            get {
                return true;
            }
        }

        public override bool CanUpdate {
            get {
                return false;
            }
        }

        public virtual ParameterCollection DeleteParameters {
            get {
                if (_deleteParameters == null) {
                    _deleteParameters = new ParameterCollection();
                }
                return _deleteParameters;
            }
        }

        protected abstract Type EntityType {
            get;            
        }

        public virtual ParameterCollection GroupByParameters {
            get {
                if (_groupByParameters == null) {
                    _groupByParameters = new ParameterCollection();
                    _groupByParameters.ParametersChanged += new EventHandler(OnQueryParametersChanged);
                    if (_isTracking) {
                        DataSourceHelper.TrackViewState(_groupByParameters);
                    }
                }
                return _groupByParameters;
            }
        }

        protected bool IsTrackingViewState {
            get {
                return _isTracking;
            }
        }

        public virtual ParameterCollection InsertParameters {
            get {
                if (_insertParameters == null) {
                    _insertParameters = new ParameterCollection();
                }
                return _insertParameters;
            }
        }

        public virtual ParameterCollection OrderByParameters {
            get {
                if (_orderByParameters == null) {
                    _orderByParameters = new ParameterCollection();
                    _orderByParameters.ParametersChanged += new EventHandler(OnQueryParametersChanged);
                    if (_isTracking) {
                        DataSourceHelper.TrackViewState(_orderByParameters);
                    }
                }
                return _orderByParameters;
            }
        }

        public virtual ParameterCollection OrderGroupsByParameters {
            get {
                if (_orderGroupsByParameters == null) {
                    _orderGroupsByParameters = new ParameterCollection();
                    _orderGroupsByParameters.ParametersChanged += new EventHandler(OnQueryParametersChanged);
                    if (_isTracking) {
                        DataSourceHelper.TrackViewState(_orderGroupsByParameters);
                    }
                }
                return _orderGroupsByParameters;
            }
        }


        public virtual string OrderBy {
            get {
                return _orderBy ?? String.Empty;
            }
            set {
                if (_orderBy != value) {
                    _orderBy = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }


        public virtual string OrderGroupsBy {
            get {
                return _orderGroupsBy ?? String.Empty;
            }
            set {
                if (_orderGroupsBy != value) {
                    _orderGroupsBy = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public virtual string GroupBy {
            get {
                return _groupBy ?? String.Empty;
            }
            set {
                if (_groupBy != value) {
                    _groupBy = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public virtual ParameterCollection SelectNewParameters {
            get {
                if (_selectNewParameters == null) {
                    _selectNewParameters = new ParameterCollection();
                    _selectNewParameters.ParametersChanged += new EventHandler(OnQueryParametersChanged);
                    if (_isTracking) {
                        DataSourceHelper.TrackViewState(_selectNewParameters);
                    }
                }
                return _selectNewParameters;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
            Justification = "SelectNew refers to a projection and 'New' is not used as a suffix in this case")]
        public virtual string SelectNew {
            get {
                return _selectNew ?? String.Empty;
            }
            set {
                if (_selectNew != value) {
                    _selectNew = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public virtual ParameterCollection WhereParameters {
            get {
                if (_whereParameters == null) {
                    _whereParameters = new ParameterCollection();
                    _whereParameters.ParametersChanged += new EventHandler(OnQueryParametersChanged);
                    if (_isTracking) {
                        DataSourceHelper.TrackViewState(_whereParameters);
                    }
                }
                return _whereParameters;
            }
        }

        public virtual string Where {
            get {
                return _where ?? String.Empty;
            }
            set {
                if (_where != value) {
                    _where = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }


        public virtual ParameterCollection UpdateParameters {
            get {
                if (_updateParameters == null) {
                    _updateParameters = new ParameterCollection();
                }
                return _updateParameters;
            }
        }


        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", Justification = "Derived classes will use this to as the ParametersChanged EventHandler")]
        protected void OnQueryParametersChanged(object sender, EventArgs e) {            
            RaiseViewChanged();
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "An event exists already and it it protected")]
        public void RaiseViewChanged() {
            OnDataSourceViewChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Gets the result to apply query
        /// </summary>
        /// <returns></returns>
        protected abstract object GetSource(QueryContext context);

        protected QueryContext CreateQueryContext(DataSourceSelectArguments arguments) {
            IDictionary<string, object> whereParameters = WhereParameters.ToDictionary(_context, _owner);
            IOrderedDictionary orderByParameters = OrderByParameters.GetValues(_context, _owner).ToCaseInsensitiveDictionary();
            IDictionary<string, object> orderGroupsByParameters = OrderGroupsByParameters.ToDictionary(_context, _owner);
            IDictionary<string, object> selectNewParameters = SelectNewParameters.ToDictionary(_context, _owner);
            IDictionary<string, object> groupByParameters = GroupByParameters.ToDictionary(_context, _owner);

            return new QueryContext(
                whereParameters,
                orderGroupsByParameters,
                orderByParameters,
                groupByParameters,
                selectNewParameters,
                arguments);
        }

        /// <summary>
        /// Creates a select expression based on parameters and properties
        /// </summary>       
        protected virtual IQueryable BuildQuery(DataSourceSelectArguments arguments) {
            if (arguments == null) {
                throw new ArgumentNullException("arguments");
            }

            // Create the query context
            QueryContext context = CreateQueryContext(arguments);

            // Clear out old values before selecting new data
            _originalValues = null;

            // Get the source of the query(root IQueryable)
            object result = GetSource(context);

            if (result != null) {
                IQueryable source = QueryableDataSourceHelper.AsQueryable(result);
                // Apply additional filterting
                return ExecuteQuery(source, context);
            }
            return null;
        }

        protected virtual IQueryable ExecuteQuery(IQueryable source, QueryContext context) {
            // Execute Query
            source = ExecuteQueryExpressions(source, context);

            // Execute Sorting
            source = ExecuteSorting(source, context);

            // Execute Paging
            source = ExecutePaging(source, context);

            return source;
        }

        protected IQueryable ExecuteQueryExpressions(IQueryable source, QueryContext context) {
            if (source != null) {                
                QueryCreatedEventArgs queryArgs = new QueryCreatedEventArgs(source);
                OnQueryCreated(queryArgs);
                source = queryArgs.Query ?? source;

                // Support the Dynamic Expression language used by LinqDataSource
                if (AutoGenerateWhereClause) {
                    if (!String.IsNullOrEmpty(Where)) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        AtlasWeb.LinqDataSourceView_WhereAlreadySpecified, _owner.ID));
                    }
                    source = QueryableDataSourceHelper.CreateWhereExpression(context.WhereParameters, source, _queryable);
                }
                else if (!String.IsNullOrEmpty(Where)) {
                    source = _queryable.Where(source, Where, context.WhereParameters.ToEscapedParameterKeys(_owner));
                }

                if (AutoGenerateOrderByClause) {
                    if (!String.IsNullOrEmpty(OrderBy)) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        AtlasWeb.LinqDataSourceView_OrderByAlreadySpecified, _owner.ID));
                    }
                    source = QueryableDataSourceHelper.CreateOrderByExpression(context.OrderByParameters, source, _queryable);
                }
                else if (!String.IsNullOrEmpty(OrderBy)) {
                    source = _queryable.OrderBy(source, OrderBy, context.OrderByParameters.ToEscapedParameterKeys(_owner));
                }


                string groupBy = GroupBy;
                if (String.IsNullOrEmpty(groupBy)) {
                    if (!String.IsNullOrEmpty(OrderGroupsBy)) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            AtlasWeb.LinqDataSourceView_OrderGroupsByRequiresGroupBy, _owner.ID));
                    }
                }
                else {
                    source = _queryable.GroupBy(source, groupBy, "it", context.GroupByParameters.ToEscapedParameterKeys(_owner));
                    if (!String.IsNullOrEmpty(OrderGroupsBy)) {
                        source = _queryable.OrderBy(source, OrderGroupsBy, context.OrderGroupsByParameters.ToEscapedParameterKeys(_owner));
                    }
                }

                if (!String.IsNullOrEmpty(SelectNew)) {
                    source = _queryable.Select(source, SelectNew, context.SelectParameters.ToEscapedParameterKeys(_owner));
                }

                return source;
            }

            return source;
        }

        protected IQueryable ExecuteSorting(IQueryable source, QueryContext context) {
            string sortExpression = context.Arguments.SortExpression;

            if (CanSort && AutoSort && !String.IsNullOrEmpty(sortExpression)) {
                source = _queryable.OrderBy(source, sortExpression);
            }
            return source;
        }

        protected IQueryable ExecutePaging(IQueryable source, QueryContext context) {
            if (CanPage && AutoPage) {
                if (CanRetrieveTotalRowCount && context.Arguments.RetrieveTotalRowCount) {
                    context.Arguments.TotalRowCount = _queryable.Count(source);
                }

                if ((context.Arguments.MaximumRows > 0) && (context.Arguments.StartRowIndex >= 0)) {
                    source = _queryable.Skip(source, context.Arguments.StartRowIndex);
                    source = _queryable.Take(source, context.Arguments.MaximumRows);
                }
            }
            else if (context.Arguments.RetrieveTotalRowCount && (context.Arguments.TotalRowCount == -1)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                    AtlasWeb.LinqDataSourceView_PagingNotHandled, _owner.ID));
            }
            return source;
        }

        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] myState = (object[])savedState;

                if (myState[0] != null) {
                    ((IStateManager)WhereParameters).LoadViewState(myState[0]);
                }
                if (myState[1] != null) {
                    ((IStateManager)OrderByParameters).LoadViewState(myState[1]);
                }
                if (myState[2] != null) {
                    ((IStateManager)GroupByParameters).LoadViewState(myState[2]);
                }
                if (myState[3] != null) {
                    ((IStateManager)OrderGroupsByParameters).LoadViewState(myState[3]);
                }
                if (myState[4] != null) {
                    ((IStateManager)SelectNewParameters).LoadViewState(myState[4]);
                }
                if (myState[5] != null) {
                    _originalValues = (Hashtable)myState[5];
                }
            }
        }

        protected virtual object SaveViewState() {
            object[] myState = new object[6];
            myState[0] = DataSourceHelper.SaveViewState(_whereParameters);
            myState[1] = DataSourceHelper.SaveViewState(_orderByParameters);
            myState[2] = DataSourceHelper.SaveViewState(_groupByParameters);
            myState[3] = DataSourceHelper.SaveViewState(_orderGroupsByParameters);
            myState[4] = DataSourceHelper.SaveViewState(_selectNewParameters);
            if ((_originalValues != null) && (_originalValues.Count > 0)) {
                myState[5] = _originalValues;
            }

            return myState;
        }


        protected virtual void TrackViewState() {
            _isTracking = true;
            DataSourceHelper.TrackViewState(_whereParameters);
            DataSourceHelper.TrackViewState(_orderByParameters);
            DataSourceHelper.TrackViewState(_groupByParameters);
            DataSourceHelper.TrackViewState(_orderGroupsByParameters);
            DataSourceHelper.TrackViewState(_selectNewParameters);
        }

        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            ClearOriginalValues();
            
            IQueryable source = BuildQuery(arguments);

            IList results = source.ToList(source.ElementType);

            // Store original values for concurrency
            StoreOriginalValues(results);

            return results;
        }

        protected void ClearOriginalValues() {
            _originalValues = null;
        }        

        protected virtual IDictionary GetOriginalValues(IDictionary keys) {
            // Table data is stored in a hashtable with column names for keys and an ArrayList of row data for values.
            // i.e, Hashtable { ID = ArrayList { 0, 1, 2 }, Name = ArrayList { "A", "B", "C" } }
            if (_originalValues != null) {
                // matches list keeps track of row indexes which match.
                List<bool> matches = new List<bool>();
                foreach (DictionaryEntry entry in keys) {
                    string propertyName = (String)entry.Key;
                    if (_originalValues.ContainsKey(propertyName)) {
                        object propertyValue = entry.Value;
                        // get the row values for the current column.
                        ArrayList values = (ArrayList)_originalValues[propertyName];
                        for (int i = 0; i < values.Count; i++) {
                            if (matches.Count <= i) { // first column
                                matches.Add(OriginalValueMatches(values[i], propertyValue));
                            }
                            else if (matches[i] == true) { // subsequent columns
                                matches[i] = OriginalValueMatches(values[i], propertyValue);
                            }
                        }
                    }
                }

                int rowIndex = matches.IndexOf(true);
                // no rows match or too many rows match.
                if ((rowIndex < 0) || (matches.IndexOf(true, rowIndex + 1) >= 0)) {
                    throw new InvalidOperationException(AtlasWeb.LinqDataSourceView_OriginalValuesNotFound);
                }
                // get original values for the matching row.
                Dictionary<string, object> rowValues = new Dictionary<string, object>(_originalValues.Count,
                    StringComparer.OrdinalIgnoreCase);
                foreach (DictionaryEntry entry in _originalValues) {
                    ArrayList value = (ArrayList)entry.Value;
                    rowValues.Add((string)entry.Key, value[rowIndex]);
                }
                return rowValues;
            }

            return null;
        }

        protected virtual void StoreOriginalValues(IList results) {
            // Derived classes can override this function and call the overload
            // to determine which columns it should store for optimistic concurrency
        }

        protected void StoreOriginalValues(IList results, Func<PropertyDescriptor, bool> include) {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(EntityType);
            int numRows = results.Count;
            int maxColumns = props.Count;
            _originalValues = new Hashtable(maxColumns, StringComparer.OrdinalIgnoreCase);
            foreach (PropertyDescriptor p in props) {
                if (include(p) && p.PropertyType.IsSerializable) {
                    ArrayList values = new ArrayList(numRows);
                    _originalValues[p.Name] = values;
                    foreach (object currentRow in results) {
                        values.Add(p.GetValue(currentRow));
                    }
                }
            }
        }


        public int Update(IDictionary keys, IDictionary values, IDictionary oldValues) {
            return ExecuteUpdate(keys, values, oldValues);
        }

        public int Delete(IDictionary keys, IDictionary oldValues) {
            return ExecuteDelete(keys, oldValues);
        }

        public int Insert(IDictionary values) {
            return ExecuteInsert(values);
        }

        protected QueryableDataSourceEditData BuildDeleteObject(IDictionary keys, IDictionary oldValues, IDictionary<string, Exception> validationErrors) {
            QueryableDataSourceEditData editData = new QueryableDataSourceEditData();
            Type dataObjectType = EntityType;
            IDictionary caseInsensitiveOldValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            IDictionary originalValues = GetOriginalValues(keys);

            ParameterCollection deleteParameters = DeleteParameters;
            if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                    deleteParameters,
                                                    keys,
                                                    caseInsensitiveOldValues,
                                                    validationErrors)) {
                return editData;
            }

            if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                    deleteParameters,
                                                    oldValues,
                                                    caseInsensitiveOldValues,
                                                    validationErrors)) {
                return editData;

            }

            if (originalValues != null) {
                if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                        deleteParameters,
                                                        originalValues,
                                                        caseInsensitiveOldValues,
                                                        validationErrors)) {
                    return editData;
                }
            }

            editData.OriginalDataObject = DataSourceHelper.BuildDataObject(dataObjectType, caseInsensitiveOldValues, validationErrors);
            return editData;
        }

        protected QueryableDataSourceEditData BuildInsertObject(IDictionary values, IDictionary<string, Exception> validationErrors) {
            QueryableDataSourceEditData editData = new QueryableDataSourceEditData();
            Type dataObjectType = EntityType;
            IDictionary caseInsensitiveNewValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);

            if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                    InsertParameters,
                                                    InsertParameters.GetValues(_context, _owner),
                                                    caseInsensitiveNewValues,
                                                    validationErrors)) {
                return editData;
            }

            if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                    InsertParameters,
                                                    values,
                                                    caseInsensitiveNewValues, validationErrors)) {
                return editData;
            }

            editData.NewDataObject = DataSourceHelper.BuildDataObject(dataObjectType, caseInsensitiveNewValues, validationErrors);
            return editData;
        }

        protected QueryableDataSourceEditData BuildUpdateObjects(IDictionary keys, IDictionary values, IDictionary oldValues, IDictionary<string, Exception> validationErrors) {
            QueryableDataSourceEditData editData = new QueryableDataSourceEditData();
            Type dataObjectType = EntityType;
            IDictionary caseInsensitiveNewValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            IDictionary caseInsensitiveOldValues = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            IDictionary originalValues = GetOriginalValues(keys);

            // We start out with the old values, just to pre-populate the list with items
            // that might not have corresponding new values. For example if a GridView has
            // a read-only field, there will be an old value, but no new value. The data object
            // still has to have *some* value for a given field, so we just use the old value.
            ParameterCollection updateParameters = UpdateParameters;

            // If we have validation errors bail out while merging bailout

            if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                    updateParameters,
                                                    oldValues,
                                                    caseInsensitiveOldValues,
                                                    caseInsensitiveNewValues,
                                                    validationErrors)) {
                return editData;
            }

            if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                    updateParameters,
                                                    keys,
                                                    caseInsensitiveOldValues,
                                                    caseInsensitiveNewValues,
                                                    validationErrors)) {
                return editData;
            }

            if (originalValues != null) {
                if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                        updateParameters,
                                                        originalValues,
                                                        caseInsensitiveOldValues,
                                                        caseInsensitiveNewValues,
                                                        validationErrors)) {
                    return editData;
                }
            }

            if (!DataSourceHelper.MergeDictionaries(dataObjectType,
                                                    updateParameters,
                                                    values,
                                                    caseInsensitiveNewValues,
                                                    validationErrors)) {
                return editData;
            }

            editData.NewDataObject = DataSourceHelper.BuildDataObject(dataObjectType, caseInsensitiveNewValues, validationErrors);

            if (editData.NewDataObject != null) {
                editData.OriginalDataObject = DataSourceHelper.BuildDataObject(dataObjectType, caseInsensitiveOldValues, validationErrors);
            }
            return editData;
        }

        protected virtual int DeleteObject(object oldEntity) {
            return 0;
        }

        protected virtual int UpdateObject(object oldEntity, object newEntity) {
            return 0;
        }

        protected virtual int InsertObject(object newEntity) {
            return 0;
        }

        protected abstract void HandleValidationErrors(IDictionary<string, Exception> errors, DataSourceOperation operation);

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
            IDictionary<string, Exception> errors = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);
            QueryableDataSourceEditData editData = BuildDeleteObject(keys, oldValues, errors);

            if (errors.Any()) {
                HandleValidationErrors(errors, DataSourceOperation.Delete);
            }
            else {
                return DeleteObject(editData.OriginalDataObject);
            }

            return -1;
        }

        protected override int ExecuteInsert(IDictionary values) {
            IDictionary<string, Exception> errors = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);
            QueryableDataSourceEditData editData = BuildInsertObject(values, errors);
            if (errors.Any()) {
                HandleValidationErrors(errors, DataSourceOperation.Insert);
            }
            else {
                return InsertObject(editData.NewDataObject);
            }
            return -1;
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
            IDictionary<string, Exception> errors = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);
            QueryableDataSourceEditData editData = BuildUpdateObjects(keys, values, oldValues, errors);
            if (errors.Any()) {
                HandleValidationErrors(errors, DataSourceOperation.Update);
            }
            else {
                return UpdateObject(editData.OriginalDataObject, editData.NewDataObject);
            }
            return -1;
        }

        public event EventHandler<QueryCreatedEventArgs> QueryCreated {
            add {
                Events.AddHandler(EventQueryCreated, value);
            }
            remove {
                Events.RemoveHandler(EventQueryCreated, value);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnQueryCreated(QueryCreatedEventArgs e) {
            EventHandler<QueryCreatedEventArgs> handler = (EventHandler<QueryCreatedEventArgs>)Events[EventQueryCreated];
            if (handler != null) {
                handler(this, e);
            }
        }

        private bool OriginalValueMatches(object originalValue, object value) {
            // NOTE: Comparing IEnumerable contents instead of instances to ensure that
            // timestamp columns (of type byte[]) can be matched appropriately.
            IEnumerable originalValueEnumerable = originalValue as IEnumerable;
            IEnumerable valueEnumerable = value as IEnumerable;
            if ((originalValueEnumerable != null) && (valueEnumerable != null)) {
                return QueryableDataSourceHelper.EnumerableContentEquals(originalValueEnumerable, valueEnumerable);
            }
            return originalValue.Equals(value);
        }

        #region IStateManager Members

        bool IStateManager.IsTrackingViewState {
            get { return IsTrackingViewState; }
        }

        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        #endregion
    }
}
