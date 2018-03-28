namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>
    /// Repeater Control that enumerates over all the filterable columns in a table
    /// </summary>
    [ToolboxItem(false)]
    [ParseChildren(true)]
    public class FilterRepeater : Repeater, IWhereParametersProvider {
        private string _contextTypeName;
        private string _dynamicFilterContainerId;
        private List<FilterUserControlBase> _filters = new List<FilterUserControlBase>();
        private MetaTable _table;
        private string _tableName;

        /// <summary>
        /// The context that the filtered table belongs to
        /// </summary>
        [Category("Data"),
        DefaultValue((string)null),
        Themeable(false)]
        // 
        public string ContextTypeName {
            get {
                return _contextTypeName ?? String.Empty;
            }
            set {
                if (!ContextTypeName.Equals(value)) {
                    _contextTypeName = value;
                    _table = null;
                }
            }
        }

        /// <summary>
        /// ID of the filter control in the ItemTemplate. By default, it's "DynamicFilter".
        /// </summary>
        [Category("Behavior"),
        DefaultValue("DynamicFilter"),
        Themeable(false),
        IDReferenceProperty(typeof(FilterUserControlBase)),
        ResourceDescription("DynamicFilterRepeater_DynamicFilterContainerId")]
        public string DynamicFilterContainerId {
            get {
                if (String.IsNullOrEmpty(_dynamicFilterContainerId)) {
                    _dynamicFilterContainerId = "DynamicFilter";
                }
                return _dynamicFilterContainerId;
            }
            set {
                _dynamicFilterContainerId = value;
            }
        }

        /// <summary>
        /// Returns the table associated with this filter repeater.
        /// </summary>
        public MetaTable Table {
            get {
                if (_table == null) {
                    _table = GetTable();
                }
                return _table;
            }
        }

        /// <summary>
        /// Name of the table being filtered
        /// </summary>
        [Category("Data"),
        DefaultValue((string)null),
        Themeable(false),
        ResourceDescription("FilterRepeater_TableName")]
        public string TableName {
            get {
                return _tableName ?? String.Empty;
            }
            set {
                if (!TableName.Equals(value)) {
                    _tableName = value;
                    _table = null;
                }
            }
        }

        public override bool Visible {
            get {
                // 


                return base.Visible && _filters.Count > 0;
            }
            set {
                base.Visible = value;
            }
        }

        /// <summary>
        /// same as base.
        /// </summary>
        public override void DataBind() {
            // Start with an empty filters list when DataBinding. This is needed when DataBind()
            // gets called multiple times.
            _filters.Clear();

            base.DataBind();
        }

        /// <summary>
        /// Returns an enumeration of the columns belonging to the table associated with
        /// this filter repeater that are sortable (by default, foreign key and boolean columns)
        /// that are scaffoldable
        /// </summary>
        /// <returns></returns>
        protected internal virtual IEnumerable<MetaColumn> GetFilteredColumns() {
            return Table.Columns.Where(c => IsFilterableColumn(c));
        }

        internal IEnumerable<FilterUserControlBase> GetFilterControls() {
            return _filters;
        }

        private MetaTable GetTable() {
            if (!String.IsNullOrEmpty(ContextTypeName) || !String.IsNullOrEmpty(TableName)) {
                // get table from control properties

                string contextTypeName = ContextTypeName;
                string tableName = TableName;

                if (String.IsNullOrEmpty(ContextTypeName)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterRepeater_MissingContextTypeName,
                        ID));
                } else if (String.IsNullOrEmpty(tableName)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterRepeater_MissingTableName,
                        ID));
                }

                Type contextType = null;
                if (!String.IsNullOrEmpty(ContextTypeName)) {
                    try {
                        contextType = BuildManager.GetType(contextTypeName, /*throwOnError*/ true, /*ignoreCase*/ true);
                    } catch (Exception e) {
                        throw new InvalidOperationException(String.Format(
                            CultureInfo.CurrentCulture,
                            DynamicDataResources.FilterRepeater_InvalidContextTypeName,
                            ID,
                            contextTypeName), e);
                    }
                }
                MetaModel model;
                try {
                     model = MetaModel.GetModel(contextType);
                } catch (InvalidOperationException e) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterRepeater_UnknownContextTypeName,
                        ID,
                        contextType.FullName), e);
                }
                try {
                    return model.GetTable(tableName);
                } catch (ArgumentException e) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterRepeater_InvalidTableName,
                        ID,
                        tableName), e);
                }
            } else {
                MetaTable table = DynamicDataRouteHandler.GetRequestMetaTable(HttpContext.Current);
                if (table == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterRepeater_CantInferInformationFromUrl,
                        ID));
                }
                return table;
            }
        }

        internal static bool IsFilterableColumn(MetaColumn column) {
            if (!column.Scaffold) return false;

            if (column.IsCustomProperty) return false;

            if (column is MetaForeignKeyColumn) return true;

            if (column.ColumnType == typeof(bool)) return true;

            return false;
        }


        /// <summary>
        /// gets called for every item and alternating item template being instantiated by this
        /// repeater during databinding
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnFilterItemCreated(RepeaterItem item) {
            var filter = item.FindControl(DynamicFilterContainerId) as FilterUserControlBase;
            if (filter == null) {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FilterRepeater_CouldNotFindControlInTemplate,
                        ID,
                        typeof(FilterUserControlBase).FullName,
                        DynamicFilterContainerId));
            }

            var column = (MetaColumn)item.DataItem;
            filter.TableName = column.Table.Name;
            filter.DataField = column.Name;
            filter.ContextTypeName = column.Table.DataContextType.AssemblyQualifiedName;

            // Keep track of all the filters we create
            _filters.Add(filter);
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            // Don't do anything in Design mode
            if (DesignMode) {
                return;
            }

            Page.InitComplete += new EventHandler(Page_InitComplete);
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected override void OnItemCreated(RepeaterItemEventArgs e) {
            base.OnItemCreated(e);

            if (DesignMode) {
                return;
            }

            ListItemType listItemType = e.Item.ItemType;
            if (listItemType == ListItemType.Item || listItemType == ListItemType.AlternatingItem) {
                OnFilterItemCreated(e.Item);
            }
        }

        private void Page_InitComplete(object sender, EventArgs e) {
            // We need to do this in InitComplete rather than Init to allow the user to set the
            // TableName in Page_Init
            DataSource = GetFilteredColumns();
            DataBind();
        }

        #region IWhereParametersProvider Members

        /// <summary>
        /// See IWhereParametersProvider
        /// </summary>
        public virtual IEnumerable<Parameter> GetWhereParameters(IDynamicDataSource dataSource) {
            // Add all the specific filters as where parameters
            return GetFilterControls().Select(filter => (Parameter)new DynamicControlParameter(filter.UniqueID) {
                Name = filter.DataField,
            });
        }

        #endregion
    }
}
