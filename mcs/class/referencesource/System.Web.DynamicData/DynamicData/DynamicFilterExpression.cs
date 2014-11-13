using System.Linq.Expressions;
using System.Web.UI;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.UI.WebControls.Expressions;
using System.Web.UI.WebControls;    

namespace System.Web.DynamicData {
    /// <summary>
    /// This is a Dynamic Data-specific extension of DataSourceExpression that works by forwarding the processing of an IQueryable to
    /// a specialized control such as QueryableFilterRepeater or DynamicFilter.
    /// </summary>
    public class DynamicFilterExpression : DataSourceExpression {
        /// <summary>
        /// References the ID of a QueryableFilterRepeater or DynamicFilter control on the page.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = "This refers to a Control ID")]
        public string ControlID { get; set; }
        private IFilterExpressionProvider FilterExpressionProvider { get; set; }

        public override void SetContext(Control owner, HttpContext context, IQueryableDataSource dataSource) {
            base.SetContext(owner, context, dataSource);            

            FilterExpressionProvider = FindControl(Owner);            
            FilterExpressionProvider.Initialize(dataSource);
        }

        private IFilterExpressionProvider FindControl(Control control) {
            var result = Misc.FindControl(control, ControlID) as IFilterExpressionProvider;
            if (result == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "The control '{0}' could not be found.", ControlID));
            }
            return result;
        }

        /// <summary>
        /// Delegates the processing of the source queryable to the control referenced by ControlID.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override IQueryable GetQueryable(IQueryable source) {
            IQueryable result = FilterExpressionProvider.GetQueryable(source);
            return result;
        }
    }
}
