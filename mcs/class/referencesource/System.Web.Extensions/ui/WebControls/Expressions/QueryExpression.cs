namespace System.Web.UI.WebControls.Expressions {
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.UI;

    [
    ParseChildren(true, "Expressions"),
    PersistChildren(false)
    ]
    public class QueryExpression {
        private HttpContext _context;
        private Control _owner;
        private IQueryableDataSource _dataSource;
        private DataSourceExpressionCollection _expressions;

        [
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public DataSourceExpressionCollection Expressions {
            get {
                if (_expressions == null) {
                    _expressions = new DataSourceExpressionCollection();
                }
                return _expressions;
            }
        }

        public void Initialize(Control owner, HttpContext context, IQueryableDataSource dataSource) {
            _owner = owner;
            _context = context;
            _dataSource = dataSource;

            Expressions.SetContext(owner, context, dataSource);
        }

        public virtual IQueryable GetQueryable(IQueryable source) {
            if (source == null) {
                return null;
            }

            foreach (DataSourceExpression e in Expressions) {
                source = e.GetQueryable(source) ?? source;
            }

            return source;
        }
    }
}
