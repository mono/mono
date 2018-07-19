using System.Security.Permissions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.DynamicData.ModelProviders;
using System.Collections;
using System.Web.Routing;

namespace System.Web.DynamicData {
    /// <summary>
    /// A special column representing 1-many relationships
    /// </summary>
    public class MetaChildrenColumn : MetaColumn, IMetaChildrenColumn {

        public MetaChildrenColumn(MetaTable table, ColumnProvider entityMember)
            : base(table, entityMember) {
        }

        /// <summary>
        /// Perform initialization logic for this column
        /// </summary>
        internal protected override void Initialize() {
            base.Initialize();

            AssociationProvider a = this.Provider.Association;
            ChildTable = Model.GetTable(a.ToTable.Name, Table.DataContextType);

            if (a.ToColumn != null) {
                ColumnInOtherTable = ChildTable.GetColumn(a.ToColumn.Name);
            }
        }

        /// <summary>
        /// Returns whether this entity set column is in a Many To Many relationship
        /// </summary>
        public bool IsManyToMany {
            get {
                return Provider.Association != null &&
                    Provider.Association.Direction == AssociationDirection.ManyToMany;
            }
        }

        /// <summary>
        /// The child table (e.g. Products in Categories&lt;-Products)
        /// </summary>
        public MetaTable ChildTable { get; private set; }

        /// <summary>
        /// A pointer to the MetaColumn in the other table
        /// </summary>
        public MetaColumn ColumnInOtherTable { get; private set; }

        /// <summary>
        /// Override disabling sorting
        /// </summary>
        internal override string SortExpressionInternal {
            get {
                // children columns are not sortable
                return String.Empty;
            }
        }

        /*protected*/ internal override bool ScaffoldNoCache {
            get {
                // always display 1-many associations
                return true;
            }
        }

        /// <summary>
        /// Shortcut for getting the path to the list action for all entities in the child table that have the given row as a parent.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public string GetChildrenListPath(object row) {
            return GetChildrenPath(PageAction.List, row);
        }

        public string GetChildrenPath(string action, object row) {

            // If there is no row, we can't get a path
            if (row == null)
                return String.Empty;

            return ChildTable.GetActionPath(action, GetRouteValues(row));
        }

        public string GetChildrenPath(string action, object row, string path) {

            // If there is no row, we can't get a path
            if (row == null)
                return String.Empty;

            if (String.IsNullOrEmpty(path)) {
                return GetChildrenPath(action, row);
            }

            // Build a query string param with our primary key

            RouteValueDictionary routeValues = GetRouteValues(row);

            // Add it to the path
            return QueryStringHandler.AddFiltersToPath(path, routeValues);
        }

        private RouteValueDictionary GetRouteValues(object row) {
            var routeValues = new RouteValueDictionary();
            IList<object> pkValues = Table.GetPrimaryKeyValues(row);

            var fkColumn = ColumnInOtherTable as MetaForeignKeyColumn;

            if (fkColumn != null) {
                Debug.Assert(fkColumn.ForeignKeyNames.Count == pkValues.Count);
                for (int i = 0; i < fkColumn.ForeignKeyNames.Count; i++) {
                    routeValues.Add(fkColumn.ForeignKeyNames[i], Misc.SanitizeQueryStringValue(pkValues[i]));
                }
            }

            return routeValues;
        }


        IMetaTable IMetaChildrenColumn.ChildTable {
            get { return ChildTable; }
        }

        IMetaColumn IMetaChildrenColumn.ColumnInOtherTable {
            get { return ColumnInOtherTable; }
        }
    }
}
