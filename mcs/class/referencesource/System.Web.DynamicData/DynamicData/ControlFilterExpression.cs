using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Resources;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Expressions;

namespace System.Web.DynamicData {
    /// <summary>
    /// A Dynamic Data-specific implementation of DataSourceExpression that modifies an IQueryable based on a data key (selected row)
    /// in a data bound controls such as GridView, ListView, DetailsView, or FormView.
    /// If the Column property is left empty, the control treats the data key as the primary key of current table (this is useful 
    /// in a List-Details scenario where the databound control and the datasource are displaying items of the same type). If the
    /// Column property is not empty, this control treats the data key as a foreign key (this is useful in a Parent-Children scenario,
    /// where the databound control is displaying a list of Categories, and the data source is to be filtered to only display the
    /// Products that belong to the selected Category).
    /// </summary>
    public class ControlFilterExpression : DataSourceExpression {
        private PropertyExpression _propertyExpression;
        /// <summary>
        /// The ID of a data-bound control such as a GridView, ListView, DetailsView, or FormView whose data key will be used to build
        /// the expression that gets used in a QueryExtender. 
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = "The property refers to the ID property of a Control")]
        public string ControlID { get; set; }

        /// <summary>
        /// Optional property which when set indicates that the data key should be treated as a foreign key.
        /// </summary>
        public string Column { get; set; }

        private PropertyExpression Expression {
            get {
                if (_propertyExpression == null) {
                    _propertyExpression = new PropertyExpression();
                }
                return _propertyExpression;
            }
        }

        public override void SetContext(Control owner, HttpContext context, IQueryableDataSource dataSource) {
            base.SetContext(owner, context, dataSource);

            Owner.Page.InitComplete += new EventHandler(Page_InitComplete);
            Owner.Page.LoadComplete += new EventHandler(Page_LoadComplete);
        }

        private void Page_InitComplete(object sender, EventArgs e) {
            if (!Owner.Page.IsPostBack) {
                // Do not reconfigure the Expression on postback. It's values should be preserved via ViewState.
                Control control = FindTargetControl();
                MetaTable table = DataSource.GetMetaTable();

                if (String.IsNullOrEmpty(Column)) {
                    foreach (var param in GetPrimaryKeyControlParameters(control, table)) {
                        Expression.Parameters.Add(param);
                    }
                } else {
                    MetaForeignKeyColumn column = (MetaForeignKeyColumn)table.GetColumn(Column);
                    foreach (var param in GetForeignKeyControlParameters(control, column)) {
                        Expression.Parameters.Add(param);
                    }
                }
            }

            Expression.SetContext(Owner, Context, DataSource);
        }

        private Control FindTargetControl() {
            Control control = Misc.FindControl(Owner, ControlID);
            if (control == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.ControlFilterExpression_CouldNotFindControlID,
                    Owner.ID,
                    ControlID));
            }
            return control;
        }

        private void Page_LoadComplete(object sender, EventArgs e) {
            Expression.Parameters.UpdateValues(Context, Owner);
        }

        protected override object SaveViewState() {
            Pair p = new Pair();
            p.First = base.SaveViewState();
            p.Second = ((IStateManager)Expression.Parameters).SaveViewState();
            return p;
        }

        protected override void LoadViewState(object savedState) {
            Pair p = (Pair)savedState;
            base.LoadViewState(p.First);
            if (p.Second != null) {
                ((IStateManager)Expression.Parameters).LoadViewState(p.Second);
            }
        }

        protected override void TrackViewState() {
            base.TrackViewState();
            ((IStateManager)Expression.Parameters).TrackViewState();
        }

        private IEnumerable<Parameter> GetPrimaryKeyControlParameters(Control control, MetaTable table) {
            // For each PK column in the table, we need to create a ControlParameter
            var nameColumnMapping = table.PrimaryKeyColumns.ToDictionary(c => c.Name);
            return GetControlParameters(control, nameColumnMapping);
        }

        private IEnumerable<Parameter> GetForeignKeyControlParameters(Control control, MetaForeignKeyColumn column) {
            // For each underlying FK, we need to create a ControlParameter
            MetaTable otherTable = column.ParentTable;
            Dictionary<string, MetaColumn> nameColumnMapping = CreateColumnMapping(column, otherTable.PrimaryKeyColumns);
            return GetControlParameters(control, nameColumnMapping);
        }

        private static Dictionary<string, MetaColumn> CreateColumnMapping(MetaForeignKeyColumn column, IList<MetaColumn> columns) {
            var names = column.ForeignKeyNames;
            Debug.Assert(names.Count == columns.Count);
            Dictionary<string, MetaColumn> nameColumnMapping = new Dictionary<string, MetaColumn>();
            for (int i = 0; i < names.Count; i++) {
                // Get the filter expression for this foreign key name
                string filterExpression = column.GetFilterExpression(names[i]);
                nameColumnMapping[filterExpression] = columns[i];
            }
            return nameColumnMapping;
        }

        internal static IEnumerable<Parameter> GetControlParameters(Control control, IDictionary<string, MetaColumn> nameColumnMapping) {
            IControlParameterTarget target = null;            

            target = DynamicDataManager.GetControlParameterTarget(control);
            Debug.Assert(target != null);            

            foreach (var entry in nameColumnMapping) {
                string parameterName = entry.Key;
                MetaColumn column = entry.Value;

                ControlParameter controlParameter = new ControlParameter() {
                    Name = parameterName,
                    ControlID = control.UniqueID
                };
                if (target != null) {
                    // this means the relationship consists of more than one key and we need to expand the property name
                    controlParameter.PropertyName = target.GetPropertyNameExpression(column.Name);
                }
                DataSourceUtil.SetParameterTypeCodeAndDbType(controlParameter, column);
                yield return controlParameter;
            }
        }

        public override IQueryable GetQueryable(IQueryable source) {
            return Expression.GetQueryable(source);
        }
    }
}
