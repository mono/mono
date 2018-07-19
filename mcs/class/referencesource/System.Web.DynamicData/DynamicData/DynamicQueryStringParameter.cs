using System.Collections.Generic;
using System.Globalization;
using System.Web.DynamicData.Util;
using System.Web.Resources;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {

    /// <summary>
    /// DynamicQueryStringParameter allows a datasource to have its primary key easily fed from the query string.
    /// It does not require any attributes, and works even for multi-part primary keys.
    /// </summary>
    public class DynamicQueryStringParameter : Parameter, IWhereParametersProvider {
        /// <summary>
        /// See IWhereParametersProvider.GetWhereParameters
        /// </summary>
        public virtual IEnumerable<Parameter> GetWhereParameters(IDynamicDataSource dataSource) {
            var table = MetaTableHelper.GetTableWithFullFallback(dataSource, HttpContext.Current.ToWrapper());
            
            // check makes no sense as the above call will throw
            //if (table == null) {
            //    return new Parameter[0];
            //}

            return RouteParametersHelper.GetColumnParameters(table, Name);
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
                CultureInfo.CurrentCulture, DynamicDataResources.DynamicParameter_NeedExpansion, typeof(DynamicQueryStringParameter).Name));
        }
    }
}
