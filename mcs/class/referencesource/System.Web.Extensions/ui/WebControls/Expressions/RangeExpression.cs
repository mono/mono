namespace System.Web.UI.WebControls.Expressions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    
    public class RangeExpression : ParameterDataSourceExpression {
        public string DataField {
            get {
                return (string)ViewState["DataField"] ?? String.Empty;
            }
            set {
                ViewState["DataField"] = value;
            }
        }

        public RangeType MinType {
            get {
                object o = ViewState["MinType"];
                return o != null ? (RangeType)o : RangeType.None;
            }
            set {
                ViewState["MinType"] = value;
            }
        }

        public RangeType MaxType {
            get {
                object o = ViewState["MaxType"];
                return o != null ? (RangeType)o : RangeType.None;
            }
            set {
                ViewState["MaxType"] = value;
            }
        }

        internal virtual new IOrderedDictionary GetValues() {
            return Parameters.GetValues(Context, Owner);
        }

        public override IQueryable GetQueryable(IQueryable source) {
            if (source == null) {
                return null;
            }

            if (String.IsNullOrEmpty(DataField)) {
                throw new InvalidOperationException(AtlasWeb.Expressions_DataFieldRequired);
            }

            IOrderedDictionary values = GetValues();
            ParameterExpression parameterExpression = Expression.Parameter(source.ElementType, String.Empty);
            Expression properyExpression = ExpressionHelper.GetValue(ExpressionHelper.CreatePropertyExpression(parameterExpression, DataField));

            if (MinType == RangeType.None && MaxType == RangeType.None) {
                throw new InvalidOperationException(AtlasWeb.RangeExpression_RangeTypeMustBeSpecified);
            }

            Expression minExpression = null;
            Expression maxExpression = null;

            if (MinType != RangeType.None) {
                if (values.Count == 0) {
                    throw new InvalidOperationException(AtlasWeb.RangeExpression_MinimumValueRequired);
                }

                if (values[0] != null) {
                    minExpression = GetMinRangeExpression(properyExpression, values[0], MinType);
                }
            }

            if (MaxType != RangeType.None) {
                if (values.Count == 0  || ((minExpression != null) && (values.Count == 1))) {
                    throw new InvalidOperationException(AtlasWeb.RangeExpression_MaximumValueRequired);
                }

                object maxValue = minExpression == null ? values[0] : values[1];
                if (maxValue != null) {
                    maxExpression = GetMaxRangeExpression(properyExpression, maxValue, MaxType);
                }
            }

            if ((maxExpression == null) && (minExpression == null)) {
                return null;
            }

            Expression rangeExpression = CreateRangeExpressionBody(minExpression, maxExpression);

            return ExpressionHelper.Where(source, Expression.Lambda(rangeExpression, parameterExpression));
        }

        private static Expression GetMinRangeExpression(Expression propertyExpression, object value, RangeType rangeType) {
            ConstantExpression constantValue = Expression.Constant(ExpressionHelper.BuildObjectValue(value, propertyExpression.Type));
            switch (rangeType) {
                case RangeType.Exclusive:
                    return Expression.GreaterThan(propertyExpression, constantValue);
                case RangeType.None:
                    return null;
                case RangeType.Inclusive:
                    return Expression.GreaterThanOrEqual(propertyExpression, constantValue);
                default:
                    Debug.Fail("shouldn't get here!");
                    return null;
            }
        }

        private static Expression GetMaxRangeExpression(Expression propertyExpression, object value, RangeType rangeType) {
            ConstantExpression constantValue = Expression.Constant(ExpressionHelper.BuildObjectValue(value, propertyExpression.Type));
            switch (rangeType) {
                case RangeType.Exclusive:
                    return Expression.LessThan(propertyExpression, constantValue);
                case RangeType.None:
                    return null;
                case RangeType.Inclusive:
                    return Expression.LessThanOrEqual(propertyExpression, constantValue);
                default:
                    Debug.Fail("shouldn't get here!");
                    return null;
            }
        }

        private static Expression CreateRangeExpressionBody(Expression minExpression, Expression maxExpression) {
            if (minExpression == null && maxExpression == null) {
                return null;
            }

            if (minExpression == null) {
                return maxExpression;
            }

            if (maxExpression == null) {
                return minExpression;
            }

            return Expression.AndAlso(minExpression, maxExpression);
        }
    }
}
