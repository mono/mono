using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Web.DynamicData.ModelProviders;
using System.Web.DynamicData.Util;
using System.Web.Resources;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using AttributeCollection = System.ComponentModel.AttributeCollection;

namespace System.Web.DynamicData {
    /// <summary>
    /// Represents a database table for use by dynamic data pages
    /// </summary>
    public class MetaTable : IMetaTable {
        private const int DefaultColumnOrder = 10000;
        private Dictionary<string, MetaColumn> _columnsByName;
        private HttpContextBase _context;
        private MetaColumn _displayColumn;
        private string _foreignKeyColumnsNames;
        private bool? _hasToStringOverride;
        private MetaTableMetadata _metadata;
        private ReadOnlyCollection<MetaColumn> _primaryKeyColumns;
        private string[] _primaryKeyColumnNames;
        private bool _scaffoldDefaultValue;
        private MetaColumn _sortColumn;
        private bool _sortColumnProcessed;
        private TableProvider _tableProvider;
        private string _listActionPath;

        /// <summary>
        /// A collection of attributes declared on this entity type (i.e. class-level attributes).
        /// </summary>
        public AttributeCollection Attributes {
            get {
                return Metadata.Attributes;
            }
        }

        /// <summary>
        /// All columns
        /// </summary>
        public ReadOnlyCollection<MetaColumn> Columns {
            get;
            // internal for unit testing
            internal set;
        }

        // for unit testing
        internal HttpContextBase Context {
            private get {
                return _context ?? new HttpContextWrapper(HttpContext.Current);
            }
            set {
                _context = value;
            }
        }

        /// <summary>
        /// Name of table coming from the property on the data context. E.g. the value is "Products" for a table that is part of
        /// the NorthwindDataContext.Products collection.
        /// </summary>
        public string DataContextPropertyName {
            get {
                return _tableProvider.DataContextPropertyName;
            }
        }

        /// <summary>
        /// The type of the data context this table belongs to.
        /// </summary>
        public Type DataContextType {
            get {
                return Provider.DataModel.ContextType;
            }
        }

        /// <summary>
        /// Returns the column being used for display values when entries in this table are used as parents in foreign key relationships.
        /// Which column to use can be specified using DisplayColumnAttribute. If the attribute is not present, the following heuristic is used:
        /// 1. First non-PK string column
        /// 2. First PK string column
        /// 3. First PK non-string column
        /// 4. First column
        /// </summary>
        public virtual MetaColumn DisplayColumn {
            get {
                // use a local to avoid a null value if ResetMetadata gets called
                var displayColumn = _displayColumn;
                if (displayColumn == null) {
                    displayColumn = GetDisplayColumnFromMetadata() ?? GetDisplayColumnFromHeuristic();
                    _displayColumn = displayColumn;
                }

                return displayColumn;
            }
        }

        /// <summary>
        /// Gets the string to be user-friendly string representing this table. Defaults to the value of the Name property.
        /// Can be customized using DisplayNameAttribute.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")]
        public virtual string DisplayName {
            get {
                return Metadata.DisplayName ?? Name;
            }
        }

        /// <summary>
        /// Return the type of the Entity represented by this table (e.g. Product)
        /// </summary>
        public Type EntityType {
            get {
                return Provider.EntityType;
            }
        }

        /// <summary>
        /// Get a comma separated list of foreign key names.  This is useful to set the IncludePaths on an EntityDataSource
        /// </summary>
        public string ForeignKeyColumnsNames {
            get {
                if (_foreignKeyColumnsNames == null) {
                    var fkColumnNamesArray = Columns.OfType<MetaForeignKeyColumn>().Select(column => column.Name).ToArray();
                    _foreignKeyColumnsNames = String.Join(",", fkColumnNamesArray);
                }

                return _foreignKeyColumnsNames;
            }
        }

        /// <summary>
        /// Returns true if the table has a primary key
        /// </summary>
        public bool HasPrimaryKey {
            get {
                // Some of the columns may be primary keys, but if this is a view, it doesn't "have"
                // any primary keys, so PrimaryKey is null.
                return PrimaryKeyColumns.Count > 0;
            }
        }

        private bool HasToStringOverride {
            get {
                // Check if the entity type overrides ToString()
                // 
                if (!_hasToStringOverride.HasValue) {
                    MethodInfo toStringMethod = EntityType.GetMethod("ToString");
                    _hasToStringOverride = (toStringMethod.DeclaringType != typeof(object));
                }

                return _hasToStringOverride.Value;
            }
        }

        /// <summary>
        /// Returns true if this is a read-only table or view(has not PK).
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")]
        public virtual bool IsReadOnly {
            get {
                return Metadata.IsReadOnly || !HasPrimaryKey;
            }
        }

        /// <summary>
        /// Gets the action path to the list action for this table
        /// </summary>
        public string ListActionPath {
            get {
                return _listActionPath ?? GetActionPath(PageAction.List);
            }
            internal set {
                _listActionPath = value;
            }
        }

        private MetaTableMetadata Metadata {
            get {
                // use a local to avoid returning null if ResetMetadata gets called
                var metadata = _metadata;
                if (metadata == null) {
                    metadata = new MetaTableMetadata(this);
                    _metadata = metadata;
                }
                return metadata;
            }
        }

        /// <summary>
        /// The model this table belongs to.
        /// </summary>
        public MetaModel Model { get; private set; }

        /// <summary>
        /// Unique name of table. This name is unique within a given data context. (e.g. "MyCustomName_Products")
        /// </summary>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Columns that constitute the primary key of this table
        /// </summary>
        public ReadOnlyCollection<MetaColumn> PrimaryKeyColumns {
            get {
                if (_primaryKeyColumns == null) {
                    _primaryKeyColumns = Columns.Where(c => c.IsPrimaryKey).ToList().AsReadOnly();
                }
                return _primaryKeyColumns;
            }
        }

        internal string[] PrimaryKeyNames {
            get {
                if (_primaryKeyColumnNames == null) {
                    _primaryKeyColumnNames = PrimaryKeyColumns.Select(c => c.Name).ToArray();
                }
                return _primaryKeyColumnNames;
            }
        }

        /// <summary>
        /// The underlying provider for this column
        /// </summary>
        public TableProvider Provider { get { return _tableProvider; } }

        /// <summary>
        /// Return the root type of this entity's inheritance hierarchy; if the type is at the top
        /// of an inheritance hierarchy or does not have any inheritance, will return EntityType.
        /// </summary>
        public Type RootEntityType {
            get {
                return Provider.RootEntityType;
            }
        }

        /// <summary>
        /// Whether or not to scaffold. This can be customized using ScaffoldAttribute
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")]
        public virtual bool Scaffold {
            get {
                return Metadata.ScaffoldTable ?? _scaffoldDefaultValue;
            }
        }

        /// <summary>
        /// Gets the column used as the sorting column when used FK relationships. Defaults to the same column that is returned by DisplayColumn.
        /// Can be customized using options on DisplayColumnAttribute.
        /// </summary>
        public virtual MetaColumn SortColumn {
            get {
                if (!_sortColumnProcessed) {
                    var displayColumnAttribute = Metadata.DisplayColumnAttribute;
                    if (displayColumnAttribute != null && !String.IsNullOrEmpty(displayColumnAttribute.SortColumn)) {
                        if (!TryGetColumn(displayColumnAttribute.SortColumn, out _sortColumn)) {
                            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                DynamicDataResources.MetaTable_CantFindSortColumn,
                                displayColumnAttribute.SortColumn,
                                Name));
                        }

                        if (_sortColumn is MetaChildrenColumn) {
                            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                DynamicDataResources.MetaTable_CantUseChildrenColumnAsSortColumn,
                                _sortColumn.Name,
                                Name));
                        }
                    }
                    _sortColumnProcessed = true;
                }
                return _sortColumn;
            }
        }

        /// <summary>
        /// Returns true if the entries in this column are meant to be sorted in a descending order when used as parents in a FK relationship.
        /// Can be declared using options on DisplayColumnAttribute
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")]
        public virtual bool SortDescending {
            get {
                return Metadata.SortDescending;
            }
        }

        public MetaTable(MetaModel metaModel, TableProvider tableProvider) {
            _tableProvider = tableProvider;
            Model = metaModel;
        }

        /// <summary>
        /// Build the attribute collection, made publicly available through the Attributes property
        /// </summary>
        protected virtual AttributeCollection BuildAttributeCollection() {
            return Provider.Attributes;
        }

        /// <summary>
        /// Returns whether the passed in user is allowed to delete items from the table
        /// </summary>
        public virtual bool CanDelete(IPrincipal principal) {
            return Provider.CanDelete(principal);
        }

        /// <summary>
        /// Returns whether the passed in user is allowed to insert into the table
        /// </summary>
        public virtual bool CanInsert(IPrincipal principal) {
            return Provider.CanInsert(principal);
        }

        /// <summary>
        /// Returns whether the passed in user is allowed to read from the table
        /// </summary>
        public virtual bool CanRead(IPrincipal principal) {
            return Provider.CanRead(principal);
        }

        /// <summary>
        /// Returns whether the passed in user is allowed to make changes tothe table
        /// </summary>
        public virtual bool CanUpdate(IPrincipal principal) {
            return Provider.CanUpdate(principal);
        }

        public static MetaTable CreateTable(Type entityType) {
            return MetaModel.CreateSimpleModel(entityType).Tables.First();
        }

        public static MetaTable CreateTable(ICustomTypeDescriptor typeDescriptor) {
            return MetaModel.CreateSimpleModel(typeDescriptor).Tables.First();
        }

        /// <summary>
        /// Instantiate a MetaChildrenColumn object. Can be overridden to instantiate a derived type 
        /// </summary>
        /// <returns></returns>
        protected virtual MetaChildrenColumn CreateChildrenColumn(ColumnProvider columnProvider) {
            return new MetaChildrenColumn(this, columnProvider);
        }

        /// <summary>
        /// Instantiate a MetaColumn object. Can be overridden to instantiate a derived type 
        /// </summary>
        /// <returns></returns>
        protected virtual MetaColumn CreateColumn(ColumnProvider columnProvider) {
            return new MetaColumn(this, columnProvider);
        }

        private MetaColumn CreateColumnInternal(ColumnProvider columnProvider) {
            if (columnProvider.Association != null) {
                switch (columnProvider.Association.Direction) {
                    case AssociationDirection.OneToOne:
                    case AssociationDirection.ManyToOne:
                        return CreateForeignKeyColumn(columnProvider);
                    case AssociationDirection.ManyToMany:
                    case AssociationDirection.OneToMany:
                        return CreateChildrenColumn(columnProvider);
                }
                Debug.Assert(false);
            }

            return CreateColumn(columnProvider);
        }

        internal void CreateColumns() {
            var columns = new List<MetaColumn>();

            _columnsByName = new Dictionary<string, MetaColumn>(StringComparer.OrdinalIgnoreCase);
            foreach (ColumnProvider columnProvider in Provider.Columns) {
                MetaColumn column = CreateColumnInternal(columnProvider);
                columns.Add(column);

                if (_columnsByName.ContainsKey(column.Name)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.MetaTable_ColumnNameConflict,
                        column.Name, Provider.Name));
                }
                _columnsByName.Add(column.Name, column);
            }

            Columns = new ReadOnlyCollection<MetaColumn>(columns);
        }

        /// <summary>
        /// Instantiate a data context that this table belongs to. Uses the instatiotion method specified when the context was registered.
        /// </summary>
        /// <returns></returns>
        public virtual object CreateContext() {
            return Provider.DataModel.CreateContext();
        }

        /// <summary>
        /// Instantiate a MetaForeignKeyColumn object. Can be overridden to instantiate a derived type 
        /// </summary>
        /// <returns></returns>
        protected virtual MetaForeignKeyColumn CreateForeignKeyColumn(ColumnProvider columnProvider) {
            return new MetaForeignKeyColumn(this, columnProvider);
        }

        /// <summary>
        /// Gets the action path for the given row (to get primary key values for query string filters, etc.)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="row">the instance of the row</param>
        /// <returns></returns>
        public string GetActionPath(string action, object row) {
            // Delegate to the overload that takes an array of primary key values
            return GetActionPath(action, GetPrimaryKeyValues(row));
        }

        /// <summary>
        /// Gets the action path for the given row (to get primary key values for query string filters, etc.)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="row">the instance of the row</param>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetActionPath(string action, object row, string path) {
            // Delegate to the overload that takes an array of primary key values
            return GetActionPath(action, GetPrimaryKeyValues(row), path);
        }

        /// <summary>
        /// Gets the action path for the current table and the passed in action
        /// </summary>
        public string GetActionPath(string action) {
            return GetActionPath(action, (IList<object>)null);
        }

        /// <summary>
        /// Gets the action path for the current table and the passed in action. Also, include all the passed in
        /// route values in the path
        /// </summary>
        /// <returns></returns>
        public string GetActionPath(string action, RouteValueDictionary routeValues) {
            routeValues.Add(DynamicDataRoute.TableToken, Name);
            routeValues.Add(DynamicDataRoute.ActionToken, action);

            // Try to get the path from the route
            return GetActionPathFromRoutes(routeValues);
        }

        /// <summary>
        /// Gets the action path for the current table and the passed in action. Also, include the passed in
        /// primary key as part of the route.
        /// </summary>
        /// <returns></returns>
        public string GetActionPath(string action, IList<object> primaryKeyValues) {
            var routeValues = new RouteValueDictionary();
            routeValues.Add(DynamicDataRoute.TableToken, Name);
            routeValues.Add(DynamicDataRoute.ActionToken, action);

            GetRouteValuesFromPK(routeValues, primaryKeyValues);

            // Try to get the path from the route
            return GetActionPathFromRoutes(routeValues);
        }

        /// <summary>
        /// Use the passed in path and append to it query string parameters for the passed in primary key values
        /// </summary>
        public string GetActionPath(string action, IList<object> primaryKeyValues, string path) {

            // If there is no path, use standard routing
            if (String.IsNullOrEmpty(path)) {
                return GetActionPath(action, primaryKeyValues);
            }

            // Get all the PK values in a dictionary
            var routeValues = new RouteValueDictionary();
            GetRouteValuesFromPK(routeValues, primaryKeyValues);

            // Create a query string from it and Add it to the path
            return QueryStringHandler.AddFiltersToPath(path, routeValues);
        }

        private string GetActionPathFromRoutes(RouteValueDictionary routeValues) {
            RequestContext requestContext = DynamicDataRouteHandler.GetRequestContext(Context);
            string path = null;

            if (requestContext != null) {
                // Add the model to the route values so that the route can make sure it only
                // gets matched if it is meant to work with that model
                routeValues.Add(DynamicDataRoute.ModelToken, Model);

                VirtualPathData vpd = RouteTable.Routes.GetVirtualPath(requestContext, routeValues);
                if (vpd != null) {
                    path = vpd.VirtualPath;
                }
            }

            // If the virtual path is null, then there is no page to link to
            return path ?? String.Empty;
        }

        /// <summary>
        /// Looks up a column by the given name. If no column is found, an exception is thrown.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public MetaColumn GetColumn(string columnName) {
            MetaColumn column;
            if (!TryGetColumn(columnName, out column)) {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    DynamicDataResources.MetaTable_NoSuchColumn,
                    Name,
                    columnName));
            }
            return column;
        }

        private static int GetColumnOrder(MetaColumn column) {
            var displayAttribute = column.Metadata.DisplayAttribute;
            if (displayAttribute != null && displayAttribute.GetOrder() != null) {
                return displayAttribute.GetOrder().Value;
            }

            return DefaultColumnOrder;
        }

        private static int GetColumnOrder(MetaColumn column, IDictionary<string, int> groupings) {
            var displayAttribute = column.Metadata.DisplayAttribute;
            int order;
            if (displayAttribute != null) {
                string groupName = displayAttribute.GetGroupName();
                if (!String.IsNullOrEmpty(groupName) && groupings.TryGetValue(groupName, out order)) {
                    return order;
                }
            }

            return GetColumnOrder(column);
        }

        /// <summary>
        /// Look for this table's primary key in the route values (i.e. typically the query string).
        /// If they're all found, return a DataKey containing the primary key values. Otherwise return null.
        /// </summary>
        public DataKey GetDataKeyFromRoute() {
            var queryStringKeys = new OrderedDictionary(PrimaryKeyNames.Length);
            foreach (MetaColumn key in PrimaryKeyColumns) {
                // Try to find the PK in the route values. If any PK is not found, return null
                string value = Misc.GetRouteValue(key.Name);
                if (string.IsNullOrEmpty(value))
                    return null;

                queryStringKeys[key.Name] = Misc.ChangeType(value, key.ColumnType);
            }

            return new DataKey(queryStringKeys, PrimaryKeyNames);
        }

        private MetaColumn GetDisplayColumnFromHeuristic() {
            // Pick best available option (except for columns based on custom properties)
            // 1. First non-PK string column
            // 2. First PK string column
            // 3. First PK non-string column
            // 4. First column (from all columns)
            var serverSideColumns = Columns.Where(c => !c.IsCustomProperty).ToList();

            return serverSideColumns.FirstOrDefault(c => c.IsString && !c.IsPrimaryKey) ??
                serverSideColumns.FirstOrDefault(c => c.IsString) ??
                serverSideColumns.FirstOrDefault(c => c.IsPrimaryKey) ??
                Columns.First();
        }

        private MetaColumn GetDisplayColumnFromMetadata() {
            var displayColumnAttribute = Metadata.DisplayColumnAttribute;
            if (displayColumnAttribute == null) {
                return null;
            }

            MetaColumn displayColumn = null;
            if (!TryGetColumn(displayColumnAttribute.DisplayColumn, out displayColumn)) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.MetaTable_CantFindDisplayColumn,
                    displayColumnAttribute.DisplayColumn,
                    Name));
            }

            return displayColumn;
        }

        /// <summary>
        /// Gets the value to be used as the display string for an instance of a row of this table when used in FK relationships.
        /// If the row is null, returns an empty string. If the entity class has an overidden ToString() method, returns the result
        /// of that method. Otherwise, returns the ToString representation of the value of the display column (as returned by the DisplayColumn
        /// property) for the given row.
        /// </summary>
        /// <param name="row">the instance of the row</param>
        /// <returns></returns>
        public virtual string GetDisplayString(object row) {
            if (row == null)
                return String.Empty;

            // Make sure it's of the right type, and handle collections
            row = PreprocessRowObject(row);

            // If there is a ToString() override, use it
            if (HasToStringOverride) {
                return row.ToString();
            }

            // Otherwise, use the 'display column'
            object displayObject = DataBinder.GetPropertyValue(row, DisplayColumn.Name);
            return displayObject == null ? String.Empty : displayObject.ToString();
        }

        /// <summary>
        /// Returns an enumeration of columns that are filterable by default. A column is filterable if it
        /// <ul>
        /// <li>is decorated with FilterAttribte with Enabled=true</li>
        /// <li>is scaffold, is not a custom property, and is either a FK column or a Bool column</li>
        /// </ul>
        /// The enumeration is ordered by the value of the FilterAttribute.Order property. If a column
        /// does not have that attribute, the value 0 is used.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<MetaColumn> GetFilteredColumns() {
            IDictionary<string, int> columnGroupOrder = GetColumnGroupingOrder();
            return Columns.Where(c => IsFilterableColumn(c, Context.User))
                          .OrderBy(c => GetColumnOrder(c, columnGroupOrder))
                          .ThenBy(c => GetColumnOrder(c));
        }

        private IDictionary<string, int> GetColumnGroupingOrder() {
            // Group columns that have groups by group names. Then put them into a dictionary from group name -> 
            // minimum column order so that groups are "stick" close together.
            return Columns.Where(c => c.Metadata.DisplayAttribute != null && !String.IsNullOrEmpty(c.Metadata.DisplayAttribute.GetGroupName()))
                          .GroupBy(c => c.Metadata.DisplayAttribute.GetGroupName())
                          .ToDictionary(cg => cg.Key,
                                        cg => cg.Min(c => GetColumnOrder(c)));
        }

        /// <summary>
        /// Get a dictionary of primary key names and their values for the given row instance
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public IDictionary<string, object> GetPrimaryKeyDictionary(object row) {
            row = PreprocessRowObject(row);

            Dictionary<string, object> result = new Dictionary<string, object>();

            foreach (MetaColumn pkMember in PrimaryKeyColumns) {
                result.Add(pkMember.Name, DataBinder.GetPropertyValue(row, pkMember.Name));
            }

            return result;
        }

        /// <summary>
        /// Get a comma separated list of values representing the primary key for the given row instance
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public string GetPrimaryKeyString(object row) {
            // Make sure it's of the right type, and handle collections
            row = PreprocessRowObject(row);

            return GetPrimaryKeyString(GetPrimaryKeyValues(row));
        }

        /// <summary>
        /// Get a comma separated list of values representing the primary key 
        /// </summary>
        /// <param name="primaryKeyValues"></param>
        /// <returns></returns>
        public string GetPrimaryKeyString(IList<object> primaryKeyValues) {
            return Misc.PersistListToCommaSeparatedString(primaryKeyValues);
        }

        /// <summary>
        /// Get the value of the primary key components for a given row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public IList<object> GetPrimaryKeyValues(object row) {
            if (row == null)
                return null;

            // Make sure it's of the right type, and handle collections
            row = PreprocessRowObject(row);

            return Misc.GetKeyValues(PrimaryKeyColumns, row);
        }

        /// <summary>
        /// Get the IQueryable for the entity type represented by this table (i.e. IQueryable of Product). Retrieves it from a new context 
        /// instantiated using the CreateContext().
        /// </summary>
        /// <returns></returns>
        public IQueryable GetQuery() {
            return GetQuery(null);
        }

        /// <summary>
        /// Get the IQueryable for the entity type represented by this table (i.e. IQueryable of Product). Retrieves it from the provided
        /// context instance, or instantiates a new context using the CreateContext().
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual IQueryable GetQuery(object context) {
            if (context == null) {
                context = CreateContext();
            }

            IQueryable query = Provider.GetQuery(context);

            if (EntityType != RootEntityType) {
                Expression ofTypeExpression = Expression.Call(typeof(Queryable), "OfType", new[] { EntityType }, query.Expression);
                query = query.Provider.CreateQuery(ofTypeExpression);
            }

            // Return the sorted query if there is a sort column
            if (SortColumn != null) {
                return Misc.BuildSortQueryable(query, this);
            }
            return query;
        }

        private void GetRouteValuesFromPK(RouteValueDictionary routeValues, IList<object> primaryKeyValues) {
            if (primaryKeyValues != null) {
                for (int i = 0; i < PrimaryKeyNames.Length; i++) {
                    routeValues.Add(PrimaryKeyNames[i], Misc.SanitizeQueryStringValue(primaryKeyValues[i]));
                }
            }
        }

        /// <summary>
        /// Returns an enumeration of columns that are to be displayed in a scaffolded context. By default all columns with the Scaffold
        /// property set to true are included, with the exception of:
        /// <ul>
        /// <li>Long-string columns (IsLongString property set to true) when the inListControl flag is true</li>
        /// <li>Children columns when mode is equal to Insert</li>
        /// </ul>
        /// </summary>
        /// <param name="mode">The mode, such as ReadOnly, Edit, or Insert.</param>
        /// <param name="inListControl">A flag indicating if the table is being displayed as an individual entity or as part of list-grid.</param>
        /// <returns></returns>
        public virtual IEnumerable<MetaColumn> GetScaffoldColumns(DataBoundControlMode mode, ContainerType containerType) {
            IDictionary<string, int> columnGroupOrder = GetColumnGroupingOrder();
            return Columns.Where(c => IsScaffoldColumn(c, mode, containerType))
                          .OrderBy(c => GetColumnOrder(c, columnGroupOrder))
                          .ThenBy(c => GetColumnOrder(c));
        }

        /// <summary>
        /// Gets the table associated with the given type, regardless of which model it belongs to.
        /// </summary>
        public static MetaTable GetTable(Type entityType) {
            MetaTable table;
            if (!TryGetTable(entityType, out table)) {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    DynamicDataResources.MetaModel_EntityTypeDoesNotBelongToModel,
                    entityType.FullName));
            }

            return table;
        }

        /// <summary>
        /// Perform initialization logic for this table
        /// </summary>
        internal protected virtual void Initialize() {
            foreach (MetaColumn column in Columns) {
                column.Initialize();
            }
        }

        internal static bool IsFilterableColumn(IMetaColumn column, IPrincipal user) {
            Debug.Assert(column != null);

            var displayAttribute = column.Attributes.FirstOrDefault<DisplayAttribute>();
            if (displayAttribute != null && displayAttribute.GetAutoGenerateFilter().HasValue) {
                return displayAttribute.GetAutoGenerateFilter().Value;
            }

            if (!String.IsNullOrEmpty(column.FilterUIHint)) {
                return true;
            }

            // non-scaffolded columns should not be displayed by default
            if (!column.Scaffold) {
                return false;
            }

            // custom properties won't be queryable by the server
            if (column.IsCustomProperty) {
                return false;
            }

            var fkColumn = column as IMetaForeignKeyColumn;
            if (fkColumn != null) {
                // Only allow if the user has access to the parent table
                return fkColumn.ParentTable.CanRead(user);
            }

            if (column.ColumnType == typeof(bool)) {
                return true;
            }

            if (column.GetEnumType() != null) {
                return true;
            }

            return false;
        }

        private bool IsScaffoldColumn(IMetaColumn column, DataBoundControlMode mode, ContainerType containerType) {
            if (!column.Scaffold) {
                return false;
            }

            // 1:Many children columns don't make sense for new rows, so ignore them in Insert mode
            if (mode == DataBoundControlMode.Insert) {
                var childrenColumn = column as IMetaChildrenColumn;
                if (childrenColumn != null && !childrenColumn.IsManyToMany) {
                    return false;
                }
            }

            var fkColumn = column as IMetaForeignKeyColumn;
            if (fkColumn != null) {
                // Ignore the FK column if the user doesn't have access to the parent table
                if (!fkColumn.ParentTable.CanRead(Context.User)) {
                    return false;
                }
            }

            return true;
        }

        public IDictionary<string, object> GetColumnValuesFromRoute(HttpContext context) {
            return GetColumnValuesFromRoute(context.ToWrapper());
        }

        internal IDictionary<string, object> GetColumnValuesFromRoute(HttpContextBase context) {
            RouteValueDictionary routeValues = DynamicDataRouteHandler.GetRequestContext(context).RouteData.Values;
            Dictionary<string, object> columnValues = new Dictionary<string, object>();
            foreach (var column in Columns) {
                if (Misc.IsColumnInDictionary(column, routeValues)) {
                    MetaForeignKeyColumn foreignKeyColumn = column as MetaForeignKeyColumn;
                    if (foreignKeyColumn != null) {
                        // Add all the foreign keys to the column values.
                        foreach (var fkName in foreignKeyColumn.ForeignKeyNames) {
                            columnValues[fkName] = routeValues[fkName];
                        }
                    }
                    else {
                        // Convert the value to the correct type.
                        columnValues[column.Name] = Misc.ChangeType(routeValues[column.Name], column.ColumnType);
                    }
                }
            }
            return columnValues;
        }

        private object PreprocessRowObject(object row) {
            // If null, nothing to do
            if (row == null)
                return null;

            // If it's of the correct entity type, we're done
            if (EntityType.IsAssignableFrom(row.GetType())) {
                return row;
            }

            // If it's a list, try using the first item
            var rowCollection = row as IList;
            if (rowCollection != null) {
                if (rowCollection.Count >= 1) {
                    Debug.Assert(rowCollection.Count == 1);
                    return PreprocessRowObject(rowCollection[0]);
                }
            }

            // We didn't recoginze the object, so return it unchanged
            return row;
        }

        /// <summary>
        /// Resets cached table metadata (i.e. information coming from attributes) as well as metadata of all columns.
        /// The metadata cache will be rebuilt the next time any metadata-derived information gets requested.
        /// </summary>
        public void ResetMetadata() {
            _metadata = null;
            _displayColumn = null;
            _sortColumnProcessed = false;
            foreach (var column in Columns) {
                column.ResetMetadata();
            }
        }

        internal void SetScaffoldAndName(bool scaffoldDefaultValue, string nameOverride) {
            if (!String.IsNullOrEmpty(nameOverride)) {
                Name = nameOverride;
            }
            else if (Provider != null) {
                Name = Provider.Name;
            }

            _scaffoldDefaultValue = scaffoldDefaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString() {
            return Name;
        }

        /// <summary>
        /// Tries to find a column by the given name. If a column is found, it is assigned to the column
        /// variable and the method returns true. Otherwise, it returns false and column is null.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool TryGetColumn(string columnName, out MetaColumn column) {
            if (columnName == null) {
                throw new ArgumentNullException("columnName");
            }
            return _columnsByName.TryGetValue(columnName, out column);
        }

        /// <summary>
        /// Gets the table associated with the given type, regardless of which model it belongs to.
        /// </summary>
        public static bool TryGetTable(Type entityType, out MetaTable table) {
            MetaModel.CheckForRegistrationException();
            if (entityType == null) {
                throw new ArgumentNullException("entityType");
            }

            return System.Web.DynamicData.MetaModel.MetaModelManager.TryGetTable(entityType, out table);
        }

        #region IMetaTable Members

        string[] IMetaTable.PrimaryKeyNames {
            get {
                return PrimaryKeyNames;
            }
        }

        object IMetaTable.CreateContext() {
            return CreateContext();
        }

        string IMetaTable.GetDisplayString(object row) {
            return GetDisplayString(row);
        }

        IQueryable IMetaTable.GetQuery(object context) {
            return GetQuery(context);
        }

        #endregion

        private class MetaTableMetadata {
            private DisplayNameAttribute _displayNameAttribute;
            private ReadOnlyAttribute _readOnlyAttribute;

            public MetaTableMetadata(MetaTable table) {
                Debug.Assert(table != null);

                Attributes = table.BuildAttributeCollection();

                _readOnlyAttribute = Attributes.FirstOrDefault<ReadOnlyAttribute>();
                _displayNameAttribute = Attributes.FirstOrDefault<DisplayNameAttribute>();
                DisplayColumnAttribute = Attributes.FirstOrDefault<DisplayColumnAttribute>();
                ScaffoldTable = Attributes.GetAttributePropertyValue<ScaffoldTableAttribute, bool?>(a => a.Scaffold, null);
            }

            public AttributeCollection Attributes { get; private set; }

            public DisplayColumnAttribute DisplayColumnAttribute {
                get;
                private set;
            }

            public string DisplayName {
                get {
                    return _displayNameAttribute.GetPropertyValue(a => a.DisplayName, null);
                }
            }

            public bool? ScaffoldTable {
                get;
                private set;
            }

            public bool SortDescending {
                get {
                    return DisplayColumnAttribute.GetPropertyValue(a => a.SortDescending, false);
                }
            }

            public bool IsReadOnly {
                get {
                    return _readOnlyAttribute.GetPropertyValue(a => a.IsReadOnly, false);
                }
            }
        }

        ReadOnlyCollection<IMetaColumn> IMetaTable.Columns {
            get {
                // Covariance only supported on interfaces
                return Columns.OfType<IMetaColumn>().ToList().AsReadOnly();
            }
        }

        IMetaModel IMetaTable.Model {
            get {
                return Model;
            }
        }

        IMetaColumn IMetaTable.DisplayColumn {
            get { return DisplayColumn; }
        }

        IMetaColumn IMetaTable.GetColumn(string columnName) {
            return GetColumn(columnName);
        }

        IEnumerable<IMetaColumn> IMetaTable.GetFilteredColumns() {
            // We can remove the of type when we get rid of the Vnext solution since interface covariance support
            // was only added in 4.0
            return GetFilteredColumns().OfType<IMetaColumn>();
        }

        IEnumerable<IMetaColumn> IMetaTable.GetScaffoldColumns(DataBoundControlMode mode, ContainerType containerType) {
            // We can remove the of type when we get rid of the Vnext solution since interface covariance support
            // was only added in 4.0
            return GetScaffoldColumns(mode, containerType).OfType<IMetaColumn>();
        }

        ReadOnlyCollection<IMetaColumn> IMetaTable.PrimaryKeyColumns {
            get {
                return PrimaryKeyColumns.OfType<IMetaColumn>().ToList().AsReadOnly();
            }
        }

        IMetaColumn IMetaTable.SortColumn {
            get {
                return SortColumn;
            }
        }

        bool IMetaTable.TryGetColumn(string columnName, out IMetaColumn column) {
            MetaColumn metaColumn;
            column = null;
            if (TryGetColumn(columnName, out metaColumn)) {
                column = metaColumn;
                return true;
            }
            return false;
        }

        bool IMetaTable.CanDelete(IPrincipal principal) {
            return CanDelete(principal);
        }

        bool IMetaTable.CanInsert(IPrincipal principal) {
            return CanInsert(principal);
        }

        bool IMetaTable.CanRead(IPrincipal principal) {
            return CanRead(principal);
        }

        bool IMetaTable.CanUpdate(IPrincipal principal) {
            return CanUpdate(principal);
        }
    }
}
