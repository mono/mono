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


    [ExpressionPrefix("AppSettings")]
    [ExpressionEditor("System.Web.UI.Design.AppSettingsExpressionEditor, " + AssemblyRef.SystemDesign)]
    public class AppSettingsExpressionBuilder : ExpressionBuilder {
        public override bool SupportsEvaluate {
            get {
                return true;
            }
        }


        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            if (entry.DeclaringType == null || entry.PropertyInfo == null) {
                return new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(this.GetType()),
                    "GetAppSetting",
                    new CodePrimitiveExpression(entry.Expression.Trim()));
            }
            else {
                return new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(this.GetType()),
                    "GetAppSetting",
                    new CodePrimitiveExpression(entry.Expression.Trim()),
                    new CodeTypeOfExpression(entry.DeclaringType),
                    new CodePrimitiveExpression(entry.PropertyInfo.Name));
            }
        }

        public override object EvaluateExpression(object target, BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            return GetAppSetting(entry.Expression, target.GetType(), entry.PropertyInfo.Name);
        }

        public static object GetAppSetting(string key) {
            string value = ConfigurationManager.AppSettings[key];

            if (value == null) {
                throw new InvalidOperationException(SR.GetString(SR.AppSetting_not_found, key));
            }
            return value;
        }

        public static object GetAppSetting(string key, Type targetType, string propertyName) {
            string value = ConfigurationManager.AppSettings[key];

            if (targetType != null) {
                PropertyDescriptor propDesc = TypeDescriptor.GetProperties(targetType)[propertyName];
                if (propDesc != null) {
                    if (propDesc.PropertyType != typeof(string)) {
                        TypeConverter converter = propDesc.Converter;
                        if (converter.CanConvertFrom(typeof(string))) {
                            return converter.ConvertFrom(value);
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.AppSetting_not_convertible, value, propDesc.PropertyType.Name, propDesc.Name));
                        }
                    }
                }
            }

            if (value == null) {
                throw new InvalidOperationException(SR.GetString(SR.AppSetting_not_found, key));
            }
            return value;
        }
    }
}
