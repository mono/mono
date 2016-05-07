//------------------------------------------------------------------------------
// <copyright file="ConnectionStringsExpressionBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {
    using System;
    using System.Security.Permissions;
    using System.CodeDom;
    using System.Configuration;
    using System.Globalization;
    using System.Web.UI;


    [ExpressionPrefix("ConnectionStrings")]
    [ExpressionEditor("System.Web.UI.Design.ConnectionStringsExpressionEditor, " + AssemblyRef.SystemDesign)]
    public class ConnectionStringsExpressionBuilder : ExpressionBuilder {
        public override bool SupportsEvaluate {
            get {
                return true;
            }
        }

        public override object ParseExpression(string expression, Type propertyType, ExpressionBuilderContext context) {
            // This code is also copied in ConnectionStringsExpressionEditor.ParseExpression()
            const string connectionStringSuffix = ".connectionstring";
            const string providerNameSuffix = ".providername";
            string name = String.Empty;
            bool connectionString = true;
            if (expression != null) {
                if (expression.EndsWith(connectionStringSuffix, StringComparison.OrdinalIgnoreCase)) {
                    name = expression.Substring(0, expression.Length - connectionStringSuffix.Length);
                }
                else {
                    if (expression.EndsWith(providerNameSuffix, StringComparison.OrdinalIgnoreCase)) {
                        connectionString = false;
                        name = expression.Substring(0, expression.Length - providerNameSuffix.Length);
                    }
                    else {
                        name = expression;
                    }
                }
            }

            return new Pair(name, connectionString);
        }


        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            Pair p = (Pair)parsedData;
            string name = (string)p.First;
            bool connectionString = (bool)p.Second;

            if (connectionString) {
                return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.GetType()), "GetConnectionString", new CodePrimitiveExpression(name));
            }
            else {
                return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.GetType()), "GetConnectionStringProviderName", new CodePrimitiveExpression(name));
            }
        }

        public override object EvaluateExpression(object target, BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            Pair p = (Pair)parsedData;
            string name = (string)p.First;
            bool connectionString = (bool)p.Second;

            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[name];
            if (connectionString) {
                return GetConnectionString(name);
            }
            else {
                return GetConnectionStringProviderName(name);
            }
        }

        public static string GetConnectionStringProviderName(string connectionStringName) {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (setting == null) {
                throw new InvalidOperationException(SR.GetString(SR.Connection_string_not_found, connectionStringName));
            }
            return setting.ProviderName;
        }

        public static string GetConnectionString(string connectionStringName) {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (setting == null) {
                throw new InvalidOperationException(SR.GetString(SR.Connection_string_not_found, connectionStringName));
            }
            return setting.ConnectionString;
        }
    }
}
