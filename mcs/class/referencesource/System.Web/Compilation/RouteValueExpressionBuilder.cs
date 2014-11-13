//------------------------------------------------------------------------------
// <copyright file="AppSettingsExpressionBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {
    using System;
    using System.Security.Permissions;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Web.UI;
    using System.Web.Routing;
    using System.Collections.Generic;

    [ExpressionPrefix("Routes")]
    [ExpressionEditor("System.Web.UI.Design.RouteValueExpressionEditor, " + AssemblyRef.SystemDesign)]
    public class RouteValueExpressionBuilder : ExpressionBuilder {
        public override bool SupportsEvaluate {
            get {
                return true;
            }
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            return new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(this.GetType()),
                "GetRouteValue",
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Page"),
                new CodePrimitiveExpression(entry.Expression.Trim()),
                new CodeTypeOfExpression(new CodeTypeReference(entry.ControlType)),
                new CodePrimitiveExpression(entry.Name)
                );
        }

        public override object EvaluateExpression(object target, BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            // Target should always be a control
            Control control = target as Control;
            if (control == null) 
                return null;

            return GetRouteValue(context.TemplateControl.Page, entry.Expression.Trim(), entry.ControlType, entry.Name);
        }

        internal static object ConvertRouteValue(object value, Type controlType, string propertyName) {
            // Try to do any type converting necessary for the property set
            if (controlType != null && !String.IsNullOrEmpty(propertyName)) {
                PropertyDescriptor propDesc = TypeDescriptor.GetProperties(controlType)[propertyName];
                if (propDesc != null) {
                    if (propDesc.PropertyType != typeof(string)) {
                        TypeConverter converter = propDesc.Converter;
                        if (converter.CanConvertFrom(typeof(string))) {
                            return converter.ConvertFrom(value);
                        }
                    }
                }
            }
            return value;
        }

        // Format will be <%$ RouteValue: Key %>, controlType,propertyName are used to figure out what typeconverter to use
        public static object GetRouteValue(Page page, string key, Type controlType, string propertyName) {
            if (page == null || String.IsNullOrEmpty(key) || page.RouteData == null) {
                return null;
            }

            return ConvertRouteValue(page.RouteData.Values[key], controlType, propertyName);
        }
    }
}
