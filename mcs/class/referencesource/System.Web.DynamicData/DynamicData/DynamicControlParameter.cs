using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Web.DynamicData.Util;
using System.Web.Resources;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {

    /// <summary>
    /// DynamicControlParameter is similar to ControlParameter, but understainds higher level concepts.  e.g. in a 
    /// master-details scenario using a GridView and DetailsView, you only need to point the DetailsView's datasource
    /// to the GridView (using a DynamicControlParameter), and it does the right thing.  This works even for
    /// multi-part primary keys
    /// </summary>
    public class DynamicControlParameter : Parameter, IWhereParametersProvider {

        /// <summary>
        /// </summary>
        public DynamicControlParameter() { }

        /// <summary>
        /// </summary>
        public DynamicControlParameter(string controlId) { ControlId = controlId; }

        /// <summary>
        /// The ID of the control from which the parameter gets its data
        /// </summary>
        public string ControlId { get; set; }

        /// <summary>
        /// See IWhereParametersProvider.GetWhereParameters
        /// </summary>
        public virtual IEnumerable<Parameter> GetWhereParameters(IDynamicDataSource dataSource) {
            Debug.Assert(dataSource != null);

            // Find the control that the ControlParameter uses
            Control control = Misc.FindControl((Control)dataSource, ControlId);

            if (control == null) {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture, DynamicDataResources.DynamicControlParameter_DynamicDataSourceControlNotFound, ControlId));
            }

            // If the control is itself a parameter provider, delegate to it
            var whereParametersProvider = control as IWhereParametersProvider;
            if (whereParametersProvider != null) {
                return whereParametersProvider.GetWhereParameters(dataSource);
            }

            IControlParameterTarget paramTarget = DynamicDataManager.GetControlParameterTarget(control);

            if (paramTarget == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.DynamicControlParameter_DynamicDataSourceControlCannotBeUsedAsParent, ControlId));
            }

            string columnName = Name;
            MetaColumn column = null;
            MetaTable table = MetaTableHelper.GetTableWithFullFallback(dataSource, HttpContext.Current.ToWrapper());
            if (!String.IsNullOrEmpty(columnName)) {
                column = table.GetColumn(columnName);
            }
            else {
                // There was no Name attribute telling us what field to filter, but maybe
                // the control given us data has that info
                column = paramTarget.FilteredColumn;
            }

            if (column == null) {
                // If there is no specific column, we're setting the primary key

                if (paramTarget.Table != table) {
                    throw new Exception(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.DynamicControlParameter_InvalidPK,
                        ControlId, paramTarget.Table, table.Name));
                }

                return GetPrimaryKeyControlWhereParameters(control, paramTarget);
            }
            else if (column is MetaForeignKeyColumn) {
                return GetForeignKeyControlWhereParameters(control, paramTarget, (MetaForeignKeyColumn)column);
            }
            return GetPropertyControlWhereParameters(control, paramTarget, column);
        }

        private IEnumerable<Parameter> GetPropertyControlWhereParameters(Control control,
            IControlParameterTarget paramTarget, MetaColumn column) {
            ControlParameter controlParameter = new ControlParameter() {
                Name = column.Name,
                ControlID = control.UniqueID,
                PropertyName = paramTarget.GetPropertyNameExpression(column.Name)
            };
            
            DataSourceUtil.SetParameterTypeCodeAndDbType(controlParameter, column);

            yield return controlParameter;
        }

        private IEnumerable<Parameter> GetPrimaryKeyControlWhereParameters(Control control,
            IControlParameterTarget paramTarget) {

            MetaTable parentTable = paramTarget.Table;
            if (parentTable != null) {
                // For each PK column in the table, we need to create a ControlParameter
                foreach (var keyColumn in parentTable.PrimaryKeyColumns) {
                    var controlParameter = new ControlParameter() {
                        Name = keyColumn.Name,
                        ControlID = control.UniqueID,
                        PropertyName = paramTarget.GetPropertyNameExpression(keyColumn.Name)
                    };

                    DataSourceUtil.SetParameterTypeCodeAndDbType(controlParameter, keyColumn);

                    yield return controlParameter;
                }
            }
        }

        private IEnumerable<Parameter> GetForeignKeyControlWhereParameters(Control control,
            IControlParameterTarget paramTarget, MetaForeignKeyColumn column) {

            MetaTable parentTable = paramTarget.Table;
            if (parentTable != null) {
                string namePrefix = String.Empty;
                // Make sure the data types match
                if (column.ColumnType != parentTable.EntityType) {
                    throw new Exception(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.DynamicControlParameter_DynamicDataSourceColumnNotCompatibleWithTable,
                        column.DisplayName, parentTable.Name));
                }

                // For each underlying FK, we need to create a ControlParameter
                Debug.Assert(column.ForeignKeyNames.Count == parentTable.PrimaryKeyColumns.Count);
                int index = 0;
                foreach (var fkName in column.ForeignKeyNames) {
                    MetaColumn parentTablePKColumn = parentTable.PrimaryKeyColumns[index++];

                    var controlParameter = new ControlParameter() {
                        Name = fkName,
                        ControlID = control.UniqueID,
                        PropertyName = paramTarget.GetPropertyNameExpression(parentTablePKColumn.Name)
                    };

                    DataSourceUtil.SetParameterTypeCodeAndDbType(controlParameter, parentTablePKColumn);

                    yield return controlParameter;
                }
            }
        }

        /// <summary>
        /// same as base
        /// </summary>
        /// <param name="context"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        protected override object Evaluate(HttpContext context, Control control) {
            // If this gets called, it means we never had a chance to expand the parameter. Give an error
            // telling the user to use a DynamicDataManager
            throw new InvalidOperationException(String.Format(
                CultureInfo.CurrentCulture, DynamicDataResources.DynamicParameter_NeedExpansion, typeof(DynamicControlParameter).Name));
        }
    }
}
