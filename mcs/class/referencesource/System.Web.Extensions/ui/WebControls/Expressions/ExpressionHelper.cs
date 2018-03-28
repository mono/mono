namespace System.Web.UI.WebControls.Expressions {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class ExpressionHelper {
        public static Expression GetValue(Expression exp) {
            Type realType = GetUnderlyingType(exp.Type);
            if (realType == exp.Type) {
                return exp;
            }
            return Expression.Convert(exp, realType);
        }

        public static Type GetUnderlyingType(Type type) {
            // Get the type from Nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        public static object BuildObjectValue(object value, Type type) {
            return System.Web.UI.WebControls.DataSourceHelper.BuildObjectValue(value, type, String.Empty);
        }

        public static Expression CreatePropertyExpression(Expression parameterExpression, string propertyName) {
            if (parameterExpression == null) {
                return null;
            }
            
            if (String.IsNullOrEmpty(propertyName)) {
                return null;
            }

            Expression propExpression = null;
            string[] props = propertyName.Split('.');
            foreach (var p in props) {
                if (propExpression == null) {
                    propExpression = Expression.PropertyOrField(parameterExpression, p);
                }
                else {
                    propExpression = Expression.PropertyOrField(propExpression, p);
                }
            }
            return propExpression;
        }


        public static IQueryable Where(this IQueryable source, LambdaExpression lambda) {
            return Call(source, "Where", lambda, source.ElementType);
        }

        public static IQueryable Call(this IQueryable source, string queryMethod, Type[] genericArgs, params Expression[] arguments) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), queryMethod,
                    genericArgs,
                    arguments));
        }

        public static IQueryable Call(this IQueryable source, string queryableMethod, LambdaExpression lambda, params Type[] genericArgs) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), queryableMethod,
                    genericArgs,
                    source.Expression, Expression.Quote(lambda)));
        }

        public static Expression Or(IEnumerable<Expression> expressions) {
            Expression orExpression = null;
            foreach (Expression e in expressions) {
                if (e == null) {
                    continue;
                }
                if (orExpression == null) {
                    orExpression = e;
                }
                else {
                    orExpression = Expression.OrElse(orExpression, e);
                }
            }
            return orExpression;
        }

        public static Expression And(IEnumerable<Expression> expressions) {
            Expression andExpression = null;
            foreach (Expression e in expressions) {
                if (e == null) {
                    continue;
                }
                if (andExpression == null) {
                    andExpression = e;
                }
                else {
                    andExpression = Expression.AndAlso(andExpression, e);
                }
            }
            return andExpression;
        }
    }
}
