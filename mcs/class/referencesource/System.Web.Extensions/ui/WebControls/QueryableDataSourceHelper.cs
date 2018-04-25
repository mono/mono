namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    internal static class QueryableDataSourceHelper {
        // This regular expression verifies that parameter names are set to valid identifiers.  This validation
        // needs to match the parser's identifier validation as done in the default block of NextToken().
        private static readonly string IdentifierPattern =
            @"^\s*[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}_]" +                           // first character
            @"[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Nd}\p{Pc}\p{Mn}\p{Mc}\p{Cf}_]*"; // remaining characters

        private static readonly Regex IdentifierRegex = new Regex(IdentifierPattern + @"\s*$");

        private static readonly Regex AutoGenerateOrderByRegex = new Regex(IdentifierPattern +
            @"(\s+(asc|ascending|desc|descending))?\s*$", RegexOptions.IgnoreCase);     // order operators

        internal static IQueryable AsQueryable(object o) {
            IQueryable oQueryable = o as IQueryable;
            if (oQueryable != null) {
                return oQueryable;
            }

            // Wrap strings in IEnumerable<string> instead of treating as IEnumerable<char>.
            string oString = o as string;
            if (oString != null) {
                return Queryable.AsQueryable(new string[] { oString });
            }

            IEnumerable oEnumerable = o as IEnumerable;
            if (oEnumerable != null) {
                // IEnumerable<T> can be directly converted to an IQueryable<T>.
                Type genericType = FindGenericEnumerableType(o.GetType());
                if (genericType != null) {
                    // The non-generic Queryable.AsQueryable gets called for array types, executing
                    // the FindGenericType logic again.  Might want to investigate way to avoid this.
                    return Queryable.AsQueryable(oEnumerable);
                }
                // Wrap non-generic IEnumerables in IEnumerable<object>.
                List<object> genericList = new List<object>();
                foreach (object item in oEnumerable) {
                    genericList.Add(item);
                }
                return Queryable.AsQueryable(genericList);
            }

            // Wrap non-IEnumerable types in IEnumerable<T>.
            Type listType = typeof(List<>).MakeGenericType(o.GetType());
            IList list = (IList)DataSourceHelper.CreateObjectInstance(listType);
            list.Add(o);
            return Queryable.AsQueryable(list);
        }

        public static IList ToList(this IQueryable query, Type dataObjectType) {
            MethodInfo toListMethod = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(dataObjectType);
            return (IList)toListMethod.Invoke(null, new object[] { query });
        }

        public static bool EnumerableContentEquals(IEnumerable enumerableA, IEnumerable enumerableB) {
            IEnumerator enumeratorA = enumerableA.GetEnumerator();
            IEnumerator enumeratorB = enumerableB.GetEnumerator();
            while (enumeratorA.MoveNext()) {
                if (!enumeratorB.MoveNext())
                    return false;
                object itemA = enumeratorA.Current;
                object itemB = enumeratorB.Current;
                if (itemA == null) {
                    if (itemB != null)
                        return false;
                }
                else if (!itemA.Equals(itemB))
                    return false;
            }
            if (enumeratorB.MoveNext())
                return false;
            return true;
        }

        public static Type FindGenericEnumerableType(Type type) {
            // Logic taken from Queryable.AsQueryable which accounts for Array types which are not
            // generic but implement the generic IEnumerable interface.
            while ((type != null) && (type != typeof(object)) && (type != typeof(string))) {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))) {
                    return type;
                }
                foreach (Type interfaceType in type.GetInterfaces()) {
                    Type genericInterface = FindGenericEnumerableType(interfaceType);
                    if (genericInterface != null) {
                        return genericInterface;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        internal static IDictionary<string, object> ToEscapedParameterKeys(this ParameterCollection parameters, HttpContext context, Control control) {
            if (parameters != null) {
                return parameters.GetValues(context, control).ToEscapedParameterKeys(control);
            }
            return null;
        }

        internal static IDictionary<string, object> ToEscapedParameterKeys(this IDictionary parameters, Control owner) {
            Dictionary<string, object> escapedParameters = new Dictionary<string, object>(parameters.Count,
                StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry de in parameters) {
                string key = (string)de.Key;
                if (String.IsNullOrEmpty(key)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        AtlasWeb.LinqDataSourceView_ParametersMustBeNamed, owner.ID));

                }
                ValidateParameterName(key, owner);
                escapedParameters.Add('@' + key, de.Value);
            }
            return escapedParameters;
        }

        internal static IDictionary<string, object> ToEscapedParameterKeys(this IDictionary<string, object> parameters, Control owner) {
            Dictionary<string, object> escapedParameters = new Dictionary<string, object>(parameters.Count,
                StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, object> parameter in parameters) {
                string key = parameter.Key;
                if (String.IsNullOrEmpty(key)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        AtlasWeb.LinqDataSourceView_ParametersMustBeNamed, owner.ID));

                }
                ValidateParameterName(key, owner);
                escapedParameters.Add('@' + key, parameter.Value);
            }
            return escapedParameters;
        }

        internal static IQueryable CreateOrderByExpression(IOrderedDictionary parameters, IQueryable source, IDynamicQueryable queryable) {
            if (parameters != null && parameters.Count > 0) {
                //extract parameter values
                //extract the order by expression and apply it to the queryable
                string orderByExpression = GetOrderByClause(parameters.ToDictionary());
                if (!String.IsNullOrEmpty(orderByExpression)) {
                    return queryable.OrderBy(source, orderByExpression);
                }
            }
            return source;
        }

        internal static IQueryable CreateWhereExpression(IDictionary<string, object> parameters, IQueryable source, IDynamicQueryable queryable) {
            if (parameters != null && parameters.Count > 0) {
                //extract the where clause
                WhereClause clause = GetWhereClause(parameters);
                if (!String.IsNullOrEmpty(clause.Expression)) {
                    //transform the current query with the where clause
                    return queryable.Where(source, clause.Expression, clause.Parameters);
                }
            }
            return source;
        }

        private static WhereClause GetWhereClause(IDictionary<string, object> whereParameters) {
            Debug.Assert((whereParameters != null) && (whereParameters.Count > 0));

            WhereClause whereClause = new WhereClause();

            whereClause.Parameters = new Dictionary<string, object>(whereParameters.Count);
            StringBuilder where = new StringBuilder();
            int index = 0;
            foreach (KeyValuePair<string, object> parameter in whereParameters) {
                string key = parameter.Key;
                string value = (parameter.Value == null) ? null : parameter.Value.ToString();
                // exclude null and empty values.
                if (!(String.IsNullOrEmpty(key) || String.IsNullOrEmpty(value))) {
                    string newKey = "@p" + index++;
                    if (where.Length > 0) {
                        where.Append(" AND ");
                    }
                    where.Append(key);
                    where.Append(" == ");
                    where.Append(newKey);
                    whereClause.Parameters.Add(newKey, parameter.Value);
                }
            }

            whereClause.Expression = where.ToString();
            return whereClause;
        }

        private static string GetOrderByClause(IDictionary<string, object> orderByParameters) {
            Debug.Assert((orderByParameters != null) && (orderByParameters.Count > 0));

            StringBuilder orderBy = new StringBuilder();
            foreach (KeyValuePair<string, object> parameter in orderByParameters) {
                string value = (string)parameter.Value;
                // exclude null and empty values.
                if (!String.IsNullOrEmpty(value)) {
                    string name = parameter.Key;
                    //validate parameter name
                    ValidateOrderByParameter(name, value);

                    if (orderBy.Length > 0) {
                        orderBy.Append(", ");
                    }
                    orderBy.Append(value);
                }
            }

            return orderBy.ToString();
        }

        internal static void ValidateOrderByParameter(string name, string value) {
            if (!AutoGenerateOrderByRegex.IsMatch(value)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSourceView_InvalidOrderByFieldName, value, name));
            }
        }

        internal static void ValidateParameterName(string name, Control owner) {
            if (!IdentifierRegex.IsMatch(name)) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.LinqDataSourceView_InvalidParameterName, name, owner.ID));
            }
        }

        private class WhereClause {
            public string Expression { get; set; }
            public IDictionary<string, object> Parameters { get; set; }
        }
    }
}
