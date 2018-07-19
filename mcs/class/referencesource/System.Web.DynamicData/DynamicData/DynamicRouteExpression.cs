using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.DynamicData.Util;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Web.UI.WebControls.Expressions;

namespace System.Web.DynamicData {
    /// <summary>
    /// This class is an QueryExtender-based equivalent of DynamicQueryStringParameters.
    /// When applied to a data source it will filter the data by the value of a primary key encoded
    /// in the requests query string (or potentially routing if complex routes are used). If ColumnName
    /// is specified it will retrieve the values
    /// </summary>
    public class DynamicRouteExpression : DataSourceExpression {
        private PropertyExpression _expression = new PropertyExpression();

        /// <summary>
        /// An optional column name that can be used to change the filtering mode (i.e. filter by foreign key instead
        /// of primary key).
        /// </summary>
        [DefaultValue("")]
        public string ColumnName { get; set; }

        /// <summary>
        /// See base class.
        /// </summary>
        public override void SetContext(Control owner, HttpContext context, IQueryableDataSource dataSource) {
            base.SetContext(owner, context, dataSource);

            owner.Page.InitComplete += new EventHandler(Page_InitComplete);
        }

        void Page_InitComplete(object sender, EventArgs e) {
            Debug.Assert(DataSource != null);
            var table = DataSource.GetMetaTable();

            IEnumerable<Parameter> parameters = RouteParametersHelper.GetColumnParameters(table, ColumnName);
            parameters.ToList().ForEach(p => _expression.Parameters.Add(p));
        }

        /// <summary>
        /// See base class.
        /// </summary>
        public override IQueryable GetQueryable(IQueryable source) {
            return _expression.GetQueryable(source);
        }
    }
}
