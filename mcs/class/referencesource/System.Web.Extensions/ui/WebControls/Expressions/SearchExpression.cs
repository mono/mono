namespace System.Web.UI.WebControls.Expressions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class SearchExpression : ParameterDataSourceExpression {        
        public string DataFields {
            get {
                return (string)ViewState["DataFields"] ?? String.Empty;
            }
            set {
                ViewState["DataFields"] = value;
            }
        }

        public SearchType SearchType {
            get {
                object o = ViewState["SearchType"];
                return o != null ? (SearchType)o : SearchType.StartsWith;
            }
            set {
                ViewState["SearchType"] = value;
            }
        }

        public StringComparison ComparisonType {
            get {
                object o = ViewState["ComparisonType"];
                return o != null ? (StringComparison)o : StringComparison.OrdinalIgnoreCase;
            }
            set {
                ViewState["ComparisonType"] = value;
            }
        }

        public override IQueryable GetQueryable(IQueryable source) {
            if (source == null) {
                return null;
            }

            if ((DataFields == null) || String.IsNullOrEmpty(DataFields.Trim())) {
                throw new InvalidOperationException(AtlasWeb.Expressions_DataFieldRequired);
            }

            IDictionary<string, object> values = GetValues();

            if (values.Count == 0) {
                throw new InvalidOperationException(AtlasWeb.SearchExpression_ParameterRequired);
            }

            string query = Convert.ToString(values.First().Value, CultureInfo.CurrentCulture);

            if (String.IsNullOrEmpty(query)) {
                return null;
            }

            string[] properties = DataFields.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Use the or expression to or the fields together
            List<Expression> searchExpressions = new List<Expression>();

            ParameterExpression parameterExpression = Expression.Parameter(source.ElementType, String.Empty);

            foreach (string p in properties) {
                Expression property = ExpressionHelper.CreatePropertyExpression(parameterExpression, p.Trim());
                searchExpressions.Add(CreateCallExpression(property, query));
            }

            return ExpressionHelper.Where(source,
                                        Expression.Lambda(ExpressionHelper.Or(searchExpressions),
                                        parameterExpression));
        }

        private Expression CreateCallExpression(Expression property, string query) {
            // LINQ to SQL does not support the overloads StartsWith(string, StringComparer) or EndsWith(string, StringComparer) 
            // and Contains has not overload that takes a StringComparer
            if (SearchType == SearchType.Contains || (ViewState["ComparisonType"] == null)) {
                return Expression.Call(property, SearchType.ToString(), Type.EmptyTypes, Expression.Constant(query, property.Type));
            }
            return Expression.Call(property, SearchType.ToString(), Type.EmptyTypes, Expression.Constant(query, property.Type), Expression.Constant(ComparisonType));
        }
    }

}
