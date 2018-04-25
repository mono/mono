using System.Linq;
using System.Web.Util;

namespace System.Web.UI.WebControls {

    internal static class QueryableHelpers {

        //Methods in this class are found using GetMethod - so avoid defining overloads of the methods. 

        //This method is a helper to be able to use reflection on only this method instead of each SortyBy, Skip and Take methods.
        public static IQueryable<T> SortandPageHelper<T>(IQueryable<T> queryable, int? startRowIndex, int? maxRowIndex, string sortExpression) where T : class {
            if (queryable == null)
                throw new ArgumentNullException("queryable");

            if (!String.IsNullOrEmpty(sortExpression)) {
                queryable = queryable.SortBy<T>(sortExpression);
            }

            if (startRowIndex != null && maxRowIndex != null) {
                queryable = queryable.Skip<T>((int)startRowIndex).Take<T>((int)maxRowIndex);
            }

            return queryable;
        }

        //This method is a helper to be able to use GetMethod to get this one method and execute instead of finding the Count method on Queryable class.
        public static int CountHelper<T>(IQueryable<T> queryable) where T : class {
            if (queryable == null)
                throw new ArgumentNullException("queryable");

            return queryable.Count<T>();
        }

        public static bool IsOrderingMethodFound<T>(IQueryable<T> queryable) where T : class {
            return OrderingMethodFinder.OrderMethodExists(queryable.Expression);
        }
    }
}
