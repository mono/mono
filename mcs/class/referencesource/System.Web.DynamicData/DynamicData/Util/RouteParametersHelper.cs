using System.Collections.Generic;
using System.Diagnostics;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData.Util {
    internal class RouteParametersHelper {
        private static Parameter CreateParameter(string name, string value, MetaColumn configurationColumn) {
            var param = new Parameter() {
                Name = name,
                DefaultValue = value
            };
            DataSourceUtil.SetParameterTypeCodeAndDbType(param, configurationColumn);
            return param;
        }

        internal static IEnumerable<Parameter> GetColumnParameters(MetaTable table, string columnName) {
            if (String.IsNullOrEmpty(columnName)) {
                // If no column is specified, we're setting the primary key from the query string
                return GetPrimaryKeyParameters(table);
            } else {
                var column = table.GetColumn(columnName);
                var fkColumn = column as MetaForeignKeyColumn;
                if (fkColumn != null) {
                    // Handle the case where we're setting one of our foreign keys from the query string
                    return GetForeignKeyParameters(fkColumn);
                } else {
                    // Handle other columns (e.g. booleans)
                    return GetRegularColumnParameters(column);
                }
            }
        }

        internal static IEnumerable<Parameter> GetForeignKeyParameters(MetaForeignKeyColumn fkColumn) {
            Debug.Assert(fkColumn.ForeignKeyNames.Count == fkColumn.ParentTable.PrimaryKeyColumns.Count);
            var result = new List<Parameter>();
            for (int i = 0; i < fkColumn.ForeignKeyNames.Count; i++) {
                string name = fkColumn.ForeignKeyNames[i];
                string value = Misc.GetRouteValue(name);

                MetaColumn parentTablePKColumn = fkColumn.ParentTable.PrimaryKeyColumns[i];

                var param = CreateParameter(name, value, parentTablePKColumn);
            
                result.Add(param);
            }
            return result;
        }

        internal static IEnumerable<Parameter> GetPrimaryKeyParameters(MetaTable table) {
            var result = new List<Parameter>();
            foreach (var primaryKeyColumn in table.PrimaryKeyColumns) {
                string name = primaryKeyColumn.Name;
                string value = Misc.GetRouteValue(name);

                var param = CreateParameter(name, value, primaryKeyColumn);
                result.Add(param);
            }
            return result;
        }

        internal static IEnumerable<Parameter> GetRegularColumnParameters(MetaColumn column) {
            // Handle other columns (e.g. booleans)
            string name = column.Name;
            string value = Misc.GetRouteValue(name);

            var param = CreateParameter(name, value, column);
            
            return new List<Parameter>() { param };
        }
    }
}
