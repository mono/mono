namespace System.Web.UI.WebControls.Expressions {
    using System.Web.Query.Dynamic;
    using System;
    using System.Linq.Expressions;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;    
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Collections.Generic;
    using System.Linq;

    [
    PersistChildren(false),
    ParseChildren(true, "Parameters")
    ]
    public class CustomExpression : ParameterDataSourceExpression {
        private EventHandler<CustomExpressionEventArgs> _querying;
       
        public event EventHandler<CustomExpressionEventArgs> Querying {
            add {
                _querying += value;
            }
            remove {
                _querying -= value;
            }
        }

        public override IQueryable GetQueryable(IQueryable source) {
            CustomExpressionEventArgs e = new CustomExpressionEventArgs(source, GetValues());
            OnQuerying(e);
            return e.Query;
        }

        private void OnQuerying(CustomExpressionEventArgs e) {
            if (_querying != null) {
                _querying(this, e);
            }
        }
    }
}
