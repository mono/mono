namespace System.Web.UI.WebControls.Expressions {
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Linq.Expressions;
    using System.Linq;

    public class PropertyExpression : ParameterDataSourceExpression {
        public override IQueryable GetQueryable(IQueryable source) {
            if (source == null) {
                return null;
            }

            IDictionary<string, object> values = GetValues();
            List<Expression> equalsExpressions = new List<Expression>();
            ParameterExpression parameterExpression = Expression.Parameter(source.ElementType, String.Empty);

            foreach (KeyValuePair<string, object> pair in values) {
                if (!String.IsNullOrEmpty(pair.Key)) {
                    // Create the property expression
                    Expression property = ExpressionHelper.CreatePropertyExpression(parameterExpression, pair.Key);
                    // Get the value
                    object value = ExpressionHelper.BuildObjectValue(pair.Value, property.Type);
                    // Create Property == Value and '&&' the expressions together
                    if (value != null) {
                        Expression valueExpression = Expression.Constant(value, property.Type);
                        Expression equalsExpression = Expression.Equal(property, valueExpression);
                        equalsExpressions.Add(equalsExpression);
                    }
                }
            }

            if (equalsExpressions.Any()) {
                Expression body = ExpressionHelper.And(equalsExpressions);
                return ExpressionHelper.Where(source, Expression.Lambda(body, parameterExpression));
            }

            return source;
        }
    }
}
