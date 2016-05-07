namespace System.Web.UI.WebControls.Expressions {    
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Resources;
    using System.Web.UI;

    [
    PersistChildren(false),
    ParseChildren(true, "ThenByExpressions")
    ]
    public class OrderByExpression : DataSourceExpression {
        private const string OrderByMethod = "OrderBy";
        private const string ThenByMethod = "ThenBy";
        private const string OrderDescendingByMethod = "OrderByDescending";
        private const string ThenDescendingByMethod = "ThenByDescending";

        private Collection<ThenBy> _thenByExpressions;

        public string DataField {
            get {
                return (string)ViewState["DataField"] ?? String.Empty;
            }
            set {
                ViewState["DataField"] = value;
            }
        }

        public SortDirection Direction {
            get {
                object o = ViewState["Direction"];
                return o != null ? (SortDirection)o : SortDirection.Ascending;
            }
            set {
                ViewState["Direction"] = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public Collection<ThenBy> ThenByExpressions {
            get {
                if (_thenByExpressions == null) {
                    // 
                    _thenByExpressions = new Collection<ThenBy>();
                }
                return _thenByExpressions;
            }
        }

        public override IQueryable GetQueryable(IQueryable source) {
            if (source == null) {
                return null;
            }

            if (String.IsNullOrEmpty(DataField)) {
                throw new InvalidOperationException(AtlasWeb.Expressions_DataFieldRequired);
            }

            ParameterExpression pe = Expression.Parameter(source.ElementType, String.Empty);
            source = CreateSortQueryable(source, pe, Direction, DataField, false /* isThenBy */);

            foreach (ThenBy thenBy in ThenByExpressions) {
                source = CreateSortQueryable(source, pe, thenBy.Direction, thenBy.DataField, true /* isThenBy */);
            }

            return source;
        }

        private static IQueryable CreateSortQueryable(IQueryable source, ParameterExpression parameterExpression, SortDirection direction, string dataField, bool isThenBy) {
            string methodName = isThenBy ? GetThenBySortMethod(direction) : GetSortMethod(direction);
            Expression propertyExpression = ExpressionHelper.CreatePropertyExpression(parameterExpression, dataField);

            return source.Call(methodName,
                               Expression.Lambda(propertyExpression, parameterExpression),
                               source.ElementType,
                               propertyExpression.Type);
        }

        private static string GetSortMethod(SortDirection direction) {
            switch (direction) {
                case SortDirection.Ascending:
                    return OrderByMethod;
                case SortDirection.Descending:
                    return OrderDescendingByMethod;
                default:
                    Debug.Fail("shouldn't get here!");
                    return OrderByMethod;
            }
        }

        private static string GetThenBySortMethod(SortDirection direction) {
            switch (direction) {
                case SortDirection.Ascending:
                    return ThenByMethod;
                case SortDirection.Descending:
                    return ThenDescendingByMethod;
                default:
                    Debug.Fail("shouldn't get here!");
                    return null;
            }
        }
    }
}
