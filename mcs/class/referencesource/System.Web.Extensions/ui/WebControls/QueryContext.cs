namespace System.Web.UI.WebControls {
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Security.Permissions;
    using System.Web.UI;
    
    public class QueryContext {
        public IDictionary<string, object> SelectParameters { get; private set; }
        public IOrderedDictionary OrderByParameters { get; private set; }
        public IDictionary<string, object> GroupByParameters { get; private set; }
        public IDictionary<string, object> OrderGroupsByParameters { get; private set; }
        public IDictionary<string, object> WhereParameters { get; private set; }
        public DataSourceSelectArguments Arguments { get; private set; }

        public QueryContext(IDictionary<string, object> whereParameters,
            IDictionary<string, object> orderGroupsByParameters,
            IOrderedDictionary orderByParameters,
            IDictionary<string, object> groupByParameters,
            IDictionary<string, object> selectParameters, 
            DataSourceSelectArguments arguments) {
            WhereParameters = whereParameters;
            OrderByParameters = orderByParameters;
            OrderGroupsByParameters = orderGroupsByParameters;
            SelectParameters = selectParameters;
            GroupByParameters = groupByParameters;
            Arguments = arguments;
        }
    }
}
