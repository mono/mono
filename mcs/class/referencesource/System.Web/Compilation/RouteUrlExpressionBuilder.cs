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
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.Routing;
    using System.Collections.Generic;

    [ExpressionPrefix("Routes")]
    [ExpressionEditor("System.Web.UI.Design.RouteUrlExpressionEditor, " + AssemblyRef.SystemDesign)]
    public class RouteUrlExpressionBuilder : ExpressionBuilder {
        public override bool SupportsEvaluate {
            get {
                return true;
            }
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            return new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(this.GetType()),
                "GetRouteUrl",
                new CodeThisReferenceExpression(),
                new CodePrimitiveExpression(entry.Expression.Trim()));
        }

        public override object EvaluateExpression(object target, BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            return GetRouteUrl(context.TemplateControl, entry.Expression.Trim());
        }

        public static bool TryParseRouteExpression(string expression, RouteValueDictionary routeValues, out string routeName) {
            routeName = null;

            if (String.IsNullOrEmpty(expression))
                return false;

            string[] pieces = expression.Split(new char[] { ',' });
            foreach (string piece in pieces) {
                string[] subs = piece.Split(new char[] { '=' });
                // Make sure we have exactly <key> = <value>
                if (subs.Length != 2) {
                    return false;
                }

                string key = subs[0].Trim();
                string value = subs[1].Trim();

                if (string.IsNullOrEmpty(key)) {
                    return false;
                }

                if (key.Equals("RouteName", StringComparison.OrdinalIgnoreCase)) {
                    routeName = value;
                }
                else {
                    routeValues[key] = value;
                }
            }

            return true;
        }

        [SuppressMessage("Microsoft.Design", "CA1055", Justification = "Consistent with other URL properties in ASP.NET.")]
        // Format will be <%$ ExpPrefix: RouteName = <name>, Key=Value, Key2=Value2 %>
        public static string GetRouteUrl(Control control, string expression) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }

            string routeName = null;
            RouteValueDictionary routeParams = new RouteValueDictionary();
            if (TryParseRouteExpression(expression, routeParams, out routeName)) {
                return control.GetRouteUrl(routeName, routeParams);
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.RouteUrlExpression_InvalidExpression));
            }
        }
    }
}
