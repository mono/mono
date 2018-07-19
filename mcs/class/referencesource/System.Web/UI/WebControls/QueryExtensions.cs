using System.Linq;
using System.Linq.Expressions;

namespace System.Web.UI.WebControls {
    public static class QueryExtensions {

        private const string SORT_DIRECTION_DESC = " DESC";

        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string sortExpression) where T : class {

            if (source == null) {
                throw new ArgumentNullException("source");
            }

            if (String.IsNullOrWhiteSpace(sortExpression)) {
                return source;
            }

            sortExpression = sortExpression.Trim();
            bool isDescending = false;

            // DataSource control passes the sort parameter with a direction
            // if the direction is descending
            if (sortExpression.EndsWith(SORT_DIRECTION_DESC, StringComparison.OrdinalIgnoreCase)) {
                isDescending = true;
                int descIndex = sortExpression.Length - SORT_DIRECTION_DESC.Length;
                sortExpression = sortExpression.Substring(0, descIndex).Trim();
            }

            if (String.IsNullOrEmpty(sortExpression)) {
                return source;
            }

            ParameterExpression parameter = Expression.Parameter(source.ElementType, String.Empty);
            //VSO bug 173528-- Add support for sorting by nested property names
            MemberExpression property = null;
            string[] sortExpressionFields = sortExpression.Split('.');
            foreach (string sortExpressionField in sortExpressionFields) {
                if (property == null) {
                    property = Expression.Property(parameter, sortExpressionField);
                }
                else {
                    property = Expression.Property(property, sortExpressionField);
                }
            }
            LambdaExpression lambda = Expression.Lambda(property, parameter);

            string methodName = (isDescending) ? "OrderByDescending" : "OrderBy" ;

            Expression methodCallExpression = Expression.Call(typeof(Queryable), methodName,
                                                new Type[] { source.ElementType, property.Type },
                                                source.Expression, Expression.Quote(lambda));

            return (IQueryable<T>)source.Provider.CreateQuery(methodCallExpression);
        }
    }
}
