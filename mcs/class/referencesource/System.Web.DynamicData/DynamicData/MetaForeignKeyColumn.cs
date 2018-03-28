using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Permissions;
using System.Web.DynamicData.ModelProviders;
using System.Linq;
using System.Web.UI;

namespace System.Web.DynamicData {
    /// <summary>
    /// A special column representing many-1 relationships
    /// </summary>
    public class MetaForeignKeyColumn : MetaColumn, IMetaForeignKeyColumn {
        // Maps a foreign key name to the name that should be used in a Linq expression for filtering
        // i.e. the foreignkey name might be surfaced through a custom type descriptor e.g. CategoryID but we might really want to use
        // Category.CategoryId in the expression
        private Dictionary<string, string> _foreignKeyFilterMapping;

        public MetaForeignKeyColumn(MetaTable table, ColumnProvider entityMember)
            : base(table, entityMember) {
        }

        /// <summary>
        /// Perform initialization logic for this column
        /// </summary>
        internal protected override void Initialize() {
            base.Initialize();

            ParentTable = Model.GetTable(Provider.Association.ToTable.Name, Table.DataContextType);

            CreateForeignKeyFilterMapping(ForeignKeyNames, ParentTable.PrimaryKeyNames, (foreignKey) => Table.EntityType.GetProperty(foreignKey) != null);
        }

        internal void CreateForeignKeyFilterMapping(IList<string> foreignKeyNames, IList<string> primaryKeyNames, Func<string, bool> propertyExists) {
            // HACK: Some tests don't mock foreign key names, but this should never be the case at runtime
            if (foreignKeyNames == null) {
                return;
            }

            int pKIndex = 0;
            foreach (string fkName in foreignKeyNames) {
                if (!propertyExists(fkName)) {
                    if (_foreignKeyFilterMapping == null) {
                        _foreignKeyFilterMapping = new Dictionary<string, string>();
                    }
                    _foreignKeyFilterMapping[fkName] = Name + "." + primaryKeyNames[pKIndex];
                }
                pKIndex++;
            }
        }

        /// <summary>
        /// The parent table of the relationship (e.g. Categories in Products-&gt;Categories)
        /// </summary>
        public MetaTable ParentTable {
            get;
            // internal for unit testing
            internal set;
        }

        /// <summary>
        /// Returns true if this foriegn key column is part of the primary key of its table
        /// e.g. Order and Product are PKs in the Order_Details table
        /// </summary>
        public bool IsPrimaryKeyInThisTable {
            get {
                return Provider.Association.IsPrimaryKeyInThisTable;
            }
        }

        /// <summary>
        /// This is used when saving the value of a foreign key, e.g. when selected from a drop down.
        /// </summary>
        public void ExtractForeignKey(IDictionary dictionary, string value) {
            if (String.IsNullOrEmpty(value)) {
                // If the value is null, set all the FKs to null
                foreach (string fkName in ForeignKeyNames) {
                    dictionary[fkName] = null;
                }
            }
            else {
                string[] fkValues = Misc.ParseCommaSeparatedString(value);
                Debug.Assert(fkValues.Length == ForeignKeyNames.Count);
                for (int i = 0; i < fkValues.Length; i++) {
                    dictionary[ForeignKeyNames[i]] = fkValues[i];
                }
            }
        }

        /// <summary>
        /// Return the value of all the foreign keys components for the passed in row
        /// </summary>
        public IList<object> GetForeignKeyValues(object row) {
            object[] values = new object[ForeignKeyNames.Count];

            int index = 0;
            bool hasNonNullKey = false;
            foreach (string fkMemberName in ForeignKeyNames) {
                object keyValue = Table.Provider.EvaluateForeignKey(row, fkMemberName);

                // Set a flag if at least one non-null key is found
                if (keyValue != null)
                    hasNonNullKey = true;

                values[index++] = keyValue;
            }

            // If all the foreign keys are null, return null
            if (!hasNonNullKey)
                return null;

            return values;
        }

        /// <summary>
        /// Get a comma separated list of values representing the foreign key 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public string GetForeignKeyString(object row) {
            // Don't do anything if the row is null
            if (row == null) {
                return String.Empty;
            }
            return Misc.PersistListToCommaSeparatedString(GetForeignKeyValues(row));
        }

        /// <summary>
        /// Override allowing for sorting by the display column of the parent table (e.g. in the Products table, the Category column
        /// will be sorted by the Category.Name column order)
        /// </summary>
        internal override string SortExpressionInternal {
            get {
                var displayColumn = ParentTable.DisplayColumn;
                var sortExpression = Provider.Association.GetSortExpression(displayColumn.Provider);
                return sortExpression ?? String.Empty;
            }
        }

        /*protected*/ internal override bool ScaffoldNoCache {
            get {
                // always display many-1 associations
                return true;
            }
        }

        public string GetFilterExpression(string foreignKeyName) {
            string mappedforeignKey;
            // If the mapping doesn't exists for this property then we return the actual FK
            if (_foreignKeyFilterMapping == null || !_foreignKeyFilterMapping.TryGetValue(foreignKeyName, out mappedforeignKey)) {
                return foreignKeyName;
            }

            return mappedforeignKey;
        }

        /// <summary>
        /// Shortcut for getting the path to the details action for the given row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public string GetForeignKeyDetailsPath(object row) {
            return GetForeignKeyPath(PageAction.Details, row);
        }

        public string GetForeignKeyPath(string action, object row) {
            return GetForeignKeyPath(action, row, null);
        }

        public string GetForeignKeyPath(string action, object row, string path) {

            // If there is no row, we can't get a path
            if (row == null)
                return String.Empty;

            // Get the value of all the FKs
            IList<object> fkValues = GetForeignKeyValues(row);

            // If null, there is no associated object to go to
            if (fkValues == null)
                return String.Empty;

            return GetForeignKeyMetaTable(row).GetActionPath(action, fkValues, path);
        }

        internal MetaTable GetForeignKeyMetaTable(object row) {
            // Get the foreign key reference
            object foreignKeyReference = DataBinder.GetPropertyValue(row, Name);
            // if the type is different to the parent table type then proceed to get the correct table
            if (foreignKeyReference != null) {
                // Get the correct MetaTable based on the live object. This is used for inheritance scenarios where the type of the navigation
                // property's parent table is some base type but the instance is pointing to a derived type.
                Type rowType = foreignKeyReference.GetType();
                MetaTable rowTable = Misc.GetTableFromTypeHierarchy(rowType);
                if (rowTable != null) {
                    return rowTable;
                }
            }
            return ParentTable;
        }

        /// <summary>
        /// The names of the underlying foreign keys that make up this association
        /// </summary>
        public ReadOnlyCollection<string> ForeignKeyNames { get { return Provider.Association.ForeignKeyNames; } }

        IMetaTable IMetaForeignKeyColumn.ParentTable {
            get {
                return ParentTable;
            }
        }
    }
}
