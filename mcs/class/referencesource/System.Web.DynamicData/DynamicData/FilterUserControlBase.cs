using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Web.Compilation;
using System.Web.Resources;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Routing;

namespace System.Web.DynamicData {
    /// <summary>
    /// Base class for the filter user control
    /// </summary>
    public class FilterUserControlBase : UserControl, IControlParameterTarget {

        private MetaColumn _column;
        private FilterDelegateBase _filterDelegate;
        private DataKey _selectedDataKey;

        private bool? _isPostBackMock;
        private RouteValueDictionary _routeValues;

        /// <summary>
        /// Name of the table that the filter applies to
        /// </summary>
        [Category("Data")]
        [DefaultValue((string)null)]
        public string TableName { get; set; }
        
        /// <summary>
        /// Name of the column that the filter applies to
        /// </summary>
        [Category("Data")]
        [DefaultValue((string)null)]
        public string DataField { get; set; }

        /// <summary>
        /// The type of the context that the table/column is part of
        /// </summary>
        [Category("Data")]
        [DefaultValue((string)null)]
        public string ContextTypeName { get; set; }

        /// <summary>
        /// The value selcted in the drop down
        /// </summary>
        public virtual string SelectedValue {
            get { return null; }
        }

        /// <summary>
        /// The DataKey of the selected item
        /// </summary>
        public virtual DataKey SelectedDataKey {
            get {
                if (_selectedDataKey == null) {
                    // Build a DataKey for the primary key of the selected item

                    var fkColumn = Column as MetaForeignKeyColumn;
                    if (fkColumn == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                            DynamicDataResources.FilterUserControlBase_SelectedDataKeyNotSupportedForThisField,
                            ID,
                            DataField));
                    }

                    if (String.IsNullOrEmpty(SelectedValue))
                        return null;

                    MetaTable parentTable = fkColumn.ParentTable;
                    // 
                    string[] keyNames = parentTable.PrimaryKeyNames;
                    string[] keyStrings = Misc.ParseCommaSeparatedString(SelectedValue);
                    Debug.Assert(keyStrings.Length == keyNames.Length);

                    var keyTable = new OrderedDictionary(keyNames.Length);
                    for (int i = 0; i < keyNames.Length; i++) {
                        keyTable.Add(keyNames[i], keyStrings[i]);
                    }
                    _selectedDataKey = new DataKey(keyTable, keyNames);

                }
                return _selectedDataKey;
            }
        }

        private FilterDelegateBase FilterDelegate {
            get {
                EnsureInit();
                return _filterDelegate;
            }
        }

        /// <summary>
        /// The initial value of the filter when it gets populated (i.e. the current value)
        /// </summary>
        public string InitialValue {
            get {
                return FilterDelegate.InitialValue;
            }
        }

        /// <summary>
        /// Populate a ListControl with all the items in the foreign table (or true/false for boolean fields)
        /// </summary>
        /// <param name="listControl"></param>
        public void PopulateListControl(ListControl listControl) {
            FilterDelegate.PopulateListControl(listControl);
        }

        /// <summary>
        /// The column this filter applies to
        /// </summary>
        public MetaColumn Column {
            get {
                EnsureInit();
                return _column;
            }
        }

        private void EnsureInit() {
            if (_column != null)
                return;

            // make sure we have a DataField
            if (String.IsNullOrEmpty(DataField)) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.FilterUserControlBase_MissingDataField,
                    ID));
            }

            MetaTable table = null;
            
            if (!String.IsNullOrEmpty(ContextTypeName) || !String.IsNullOrEmpty(TableName)) {
                // make sure both ContextTypeName and TableName are specified together
                if (String.IsNullOrEmpty(ContextTypeName)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterUserControlBase_MissingContextTypeName,
                        ID));
                }
                if (String.IsNullOrEmpty(TableName)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterUserControlBase_MissingTableName,
                        ID));
                }

                Type contextType = GetContextType(ContextTypeName);
                MetaModel model = null;
                try {
                    model = MetaModel.GetModel(contextType);
                } catch (InvalidOperationException e) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterUserControlBase_UnknownContextType,
                        ID,
                        contextType.FullName), e);
                }

                string tableName = TableName;
                try {
                    table = model.GetTable(tableName);
                } catch (ArgumentException e) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterUserControlBase_InvalidTableName,
                        ID,
                        tableName), e);
                }
            } else {
                // get context information from request context
                table = DynamicDataRouteHandler.GetRequestMetaTable(HttpContext.Current);
                if (table == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterUserControlBase_CantInferInformationFromRequestUrl,
                        ID));
                }
            }

            try {
                _column = table.GetColumn(DataField);
            } catch (InvalidOperationException e) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.FilterUserControlBase_InvalidDataField,
                    ID,
                    DataField), e);
            }

            // create appropriate filter implementation based on column type
            if (_column is MetaForeignKeyColumn) {
                _filterDelegate = new ForeignKeyFilterDelegate(this);
            } else if (_column.ColumnType == typeof(bool) && !_column.IsCustomProperty) {
                _filterDelegate = new BooleanPropertyFilterDelegate(this);
            } else {
                _filterDelegate = new DefaultPropertyFilterDelegate(this);
            }
        }

        private Type GetContextType(string contextTypeName) {
            Type contextType = null;
            if (!String.IsNullOrEmpty(contextTypeName)) {
                try {
                    contextType = BuildManager.GetType(contextTypeName, /*throwOnError*/ true, /*ignoreCase*/ true);
                } catch (Exception e) {
                    throw new InvalidOperationException(String.Format(
                        CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterUserControlBase_InvalidContextTypeName,
                        ID, contextTypeName), e);
                }
            }
            return contextType;
        }

        internal bool IsPostBackInternal {
            get {
                return _isPostBackMock ?? base.IsPostBack;
            }
            set {
                _isPostBackMock = value;
            }
        }

        internal RouteValueDictionary RouteValues {
            get {
                if (_routeValues == null) {
                    RequestContext requestContext = DynamicDataRouteHandler.GetRequestContext(Context);
                    _routeValues = requestContext.RouteData.Values;
                }

                return _routeValues;
            }
            set {
                _routeValues = value;
            }
        }

        #region IDynamicControlParameterProvider Members

        MetaTable IControlParameterTarget.Table {
            get {
                EnsureInit();
                ForeignKeyFilterDelegate foreignKeyDelegate = FilterDelegate as ForeignKeyFilterDelegate;
                if (foreignKeyDelegate != null) {
                    return foreignKeyDelegate.FilterTable;
                } else {
                    return null;
                }
            }
        }

        MetaColumn IControlParameterTarget.FilteredColumn {
            get {
                EnsureInit();
                return _column;
            }
        }

        string IControlParameterTarget.GetPropertyNameExpression(string columnName) {
            return FilterDelegate.GetPropertyNameExpression(columnName);
        }

        #endregion

        private abstract class FilterDelegateBase {
            private string _initialValue;
            private bool _initialValueObtained = false;

            public FilterDelegateBase(FilterUserControlBase filterControl) {
                FilterUserControl = filterControl;
            }

            public abstract void PopulateListControl(ListControl listControl);

            public abstract string GetPropertyNameExpression(string columnName);

            protected FilterUserControlBase FilterUserControl {
                get;
                private set;
            }

            public string InitialValue {
                get {
                    // Ignore the query string param on postbacks
                    if (!_initialValueObtained && !FilterUserControl.IsPostBackInternal) {
                        _initialValue = GetInitialValueFromQueryString();
                        _initialValueObtained = true;
                    }
                    return _initialValue;
                }
            }

            protected abstract string GetInitialValueFromQueryString();
        }

        private class DefaultPropertyFilterDelegate : FilterDelegateBase {
            public DefaultPropertyFilterDelegate(FilterUserControlBase filterControl)
                : base(filterControl) {
            }

            public override void PopulateListControl(ListControl listControl) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.FilterUserControlBase_UnsupportedDataField,
                    FilterUserControl.ID,
                    FilterUserControl.DataField));
            }

            public override string GetPropertyNameExpression(string columnName) {
                return "SelectedValue";
            }

            protected override string GetInitialValueFromQueryString() {
                object value;
                if (FilterUserControl.RouteValues.TryGetValue(FilterUserControl.Column.Name, out value)) {
                    return value as string;
                }

                return null;
            }
        }

        private class BooleanPropertyFilterDelegate : DefaultPropertyFilterDelegate {
            public BooleanPropertyFilterDelegate(FilterUserControlBase filterControl)
                : base(filterControl) {
            }

            public override void PopulateListControl(ListControl listControl) {
                listControl.Items.Add(new ListItem(DynamicDataResources.FilterUserControlBase_BooleanFilter_TrueString, bool.TrueString));
                listControl.Items.Add(new ListItem(DynamicDataResources.FilterUserControlBase_BooleanFilter_FalseString, bool.FalseString));
            }
        }

        private class ForeignKeyFilterDelegate : FilterDelegateBase {

            public ForeignKeyFilterDelegate(FilterUserControlBase filterControl)
                : base(filterControl) {
                FilterTable = ((MetaForeignKeyColumn)filterControl.Column).ParentTable;
            }

            internal MetaTable FilterTable { get; private set; }

            public override void PopulateListControl(ListControl listControl) {
                Misc.FillListItemCollection(FilterTable, listControl.Items);
            }

            public override string GetPropertyNameExpression(string columnName) {
                return String.Format(CultureInfo.InvariantCulture, "SelectedDataKey['{0}']", columnName);
            }

            protected override string GetInitialValueFromQueryString() {
                var fkColumn = (MetaForeignKeyColumn)FilterUserControl.Column;

                var builder = new StringBuilder();

                for (int i = 0; i < fkColumn.ForeignKeyNames.Count; i++) {
                    // The query string parameter looks like CategoryID=5
                    string queryStringParamName = fkColumn.ForeignKeyNames[i];

                    object value;
                    // If any of the fk components are missing, we don't have a value for the filter
                    if (!FilterUserControl.RouteValues.TryGetValue(queryStringParamName, out value)) {
                        return String.Empty;
                    }

                    string pkValue = (string)value;

                    // For the ListControl value, we use a comma separated list of primary keys, instead of using
                    // named keys.  This is simpler since the ListControl value must be a single string.

                    if (i > 0) {
                        builder.Append(",");
                    }

                    builder.Append(pkValue);
                }

                return builder.ToString();
            }
        }
    }
}
