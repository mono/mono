//------------------------------------------------------------------------------
// <copyright file="ResourceExpressionBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {
    using System;
    using System.Security.Permissions;
    using System.Collections;
    using System.Collections.Specialized;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Web.UI;


    [ExpressionPrefix("Resources")]
    [ExpressionEditor("System.Web.UI.Design.ResourceExpressionEditor, " + AssemblyRef.SystemDesign)]
    public class ResourceExpressionBuilder : ExpressionBuilder {

        private static ResourceProviderFactory s_resourceProviderFactory;

        public override bool SupportsEvaluate {
            get {
                return true;
            }
        }

        public static ResourceExpressionFields ParseExpression(string expression) {
            return ParseExpressionInternal(expression);
        }

        public override object ParseExpression(string expression, Type propertyType, ExpressionBuilderContext context) {
            ResourceExpressionFields fields = null;

            try {
                fields = ParseExpressionInternal(expression);
            }
            catch {
            }

            // If the parsing failed for any reason throw an error
            if (fields == null) {
                throw new HttpException(
                    SR.GetString(SR.Invalid_res_expr, expression));
            }

            // The resource expression was successfully parsed. We now need to check whether
            // the resource object actually exists in the neutral culture

            // If we don't have a virtual path, we can't check that the resource exists.
            // This happens in the designer.  
            if (context.VirtualPathObject != null) {

                IResourceProvider resourceProvider = GetResourceProvider(fields, VirtualPath.Create(context.VirtualPath));
                object o = null;

                if (resourceProvider != null) {
                    try {
                        o = resourceProvider.GetObject(fields.ResourceKey, CultureInfo.InvariantCulture);
                    }
                    catch {}
                }

                // If it doesn't throw an exception
                if (o == null) {
                    throw new HttpException(
                        SR.GetString(SR.Res_not_found, fields.ResourceKey));
                }
            }

            return fields;
        }


        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            // Retrieve our parsed data
            ResourceExpressionFields fields = (ResourceExpressionFields) parsedData;

            // If there is no classKey, it's a page-level resource
            if (fields.ClassKey.Length == 0) {
                return GetPageResCodeExpression(fields.ResourceKey, entry);
            }

            // Otherwise, it's a global resource
            return GetAppResCodeExpression(fields.ClassKey, fields.ResourceKey, entry);
        }


        public override object EvaluateExpression(object target, BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {

            // Retrieve our parsed data
            ResourceExpressionFields fields = (ResourceExpressionFields) parsedData;

            IResourceProvider resourceProvider = GetResourceProvider(fields, context.VirtualPathObject);

            if (entry.Type == typeof(string))
                return GetResourceObject(resourceProvider, fields.ResourceKey, null /*culture*/);

            // If the property is not of type string, pass in extra information
            // so that the resource value will be converted to the right type
            return GetResourceObject(resourceProvider, fields.ResourceKey, null /*culture*/,
                entry.DeclaringType, entry.PropertyInfo.Name);
        }

        private CodeExpression GetAppResCodeExpression(string classKey, string resourceKey, BoundPropertyEntry entry) {
            // We generate the following
            //      this.GetGlobalResourceObject(classKey)
            CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression();
            expr.Method.TargetObject = new CodeThisReferenceExpression();

            expr.Method.MethodName = "GetGlobalResourceObject";
            expr.Parameters.Add(new CodePrimitiveExpression(classKey));
            expr.Parameters.Add(new CodePrimitiveExpression(resourceKey));

            // If the property is not of type string, it will need to be converted
            if (entry.Type != typeof(string) && entry.Type != null) {
                expr.Parameters.Add(new CodeTypeOfExpression(entry.DeclaringType));
                expr.Parameters.Add(new CodePrimitiveExpression(entry.PropertyInfo.Name));
            }

            return expr;
        }

        private CodeExpression GetPageResCodeExpression(string resourceKey, BoundPropertyEntry entry) {
            // We generate of the following
            //      this.GetLocalResourceObject(resourceKey)
            CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression();
            expr.Method.TargetObject = new CodeThisReferenceExpression();

            expr.Method.MethodName = "GetLocalResourceObject";
            expr.Parameters.Add(new CodePrimitiveExpression(resourceKey));

            // If the property is not of type string, it will need to be converted
            if (entry.Type != typeof(string) && entry.Type != null) {
                expr.Parameters.Add(new CodeTypeOfExpression(entry.DeclaringType));
                expr.Parameters.Add(new CodePrimitiveExpression(entry.PropertyInfo.Name));
            }

            return expr;
        }

        internal static object GetGlobalResourceObject(string classKey, string resourceKey) {
            return GetGlobalResourceObject(classKey, resourceKey, null /*objType*/, null /*propName*/, null /*culture*/);
        }

        internal static object GetGlobalResourceObject(string classKey,
            string resourceKey, Type objType, string propName, CultureInfo culture) {

            IResourceProvider resourceProvider = GetGlobalResourceProvider(classKey);
            return GetResourceObject(resourceProvider, resourceKey, culture,
                objType, propName);
        }

        internal static object GetResourceObject(IResourceProvider resourceProvider,
            string resourceKey, CultureInfo culture) {
            return GetResourceObject(resourceProvider, resourceKey, culture,
                null /*objType*/, null /*propName*/);
        }

        internal static object GetResourceObject(IResourceProvider resourceProvider,
            string resourceKey, CultureInfo culture, Type objType, string propName) {

            if (resourceProvider == null)
                return null;

            object o = resourceProvider.GetObject(resourceKey, culture);

            // If no objType/propName was provided, return the object as is
            if (objType == null)
                return o;

            // Also, if the object from the resource is not a string, return it as is
            string s = o as String;
            if (s == null)
                return o;

            // If they were provided, perform the appropriate conversion
            return ObjectFromString(s, objType, propName);
        }

        private static object ObjectFromString(string value, Type objType, string propName) {

            // Get the PropertyDescriptor for the property
            PropertyDescriptor pd = TypeDescriptor.GetProperties(objType)[propName];
            Debug.Assert(pd != null);
            if (pd == null) return null;

            // Get its type descriptor
            TypeConverter converter = pd.Converter;
            Debug.Assert(converter != null);
            if (converter == null) return null;

            // Perform the conversion
            return converter.ConvertFromInvariantString(value);
        }

        // The following syntaxes are accepted for the expression
        //      resourceKey
        //      classKey, resourceKey
        //
        private static ResourceExpressionFields ParseExpressionInternal(string expression) {
            string classKey = null;
            string resourceKey = null;

            int len = expression.Length;
            if (len == 0) {
                return new ResourceExpressionFields(classKey, resourceKey);
            }

            // Split the comma separated string 
            string[] parts = expression.Split(',');

            int numOfParts = parts.Length;

            if (numOfParts > 2) return null;

            if (numOfParts == 1) {
                resourceKey = parts[0].Trim();
            }
            else {
                classKey = parts[0].Trim();
                resourceKey = parts[1].Trim();
            }

            return new ResourceExpressionFields(classKey, resourceKey);
        }

        private static IResourceProvider GetResourceProvider(ResourceExpressionFields fields,
            VirtualPath virtualPath) {
            // If there is no classKey, it's a page-level resource
            if (fields.ClassKey.Length == 0) {
                return GetLocalResourceProvider(virtualPath);
            }

            // Otherwise, it's a global resource
            return GetGlobalResourceProvider(fields.ClassKey);
        }

        private static void EnsureResourceProviderFactory() {
            if (s_resourceProviderFactory != null)
                return;

            Type t = null;
            GlobalizationSection globConfig = RuntimeConfig.GetAppConfig().Globalization;
            t = globConfig.ResourceProviderFactoryTypeInternal;

            // If we got a type from config, use it.  Otherwise, use default factory
            if (t == null) {
                s_resourceProviderFactory = new ResXResourceProviderFactory();
            }
            else {
                s_resourceProviderFactory = (ResourceProviderFactory) HttpRuntime.CreatePublicInstance(t);
            }
        }

        private static IResourceProvider GetGlobalResourceProvider(string classKey) {
            string fullClassName = BaseResourcesBuildProvider.DefaultResourcesNamespace +
                "." + classKey;

            // If we have it cached, return it
            CacheInternal cacheInternal = System.Web.HttpRuntime.CacheInternal;
            string cacheKey = CacheInternal.PrefixResourceProvider + fullClassName;
            IResourceProvider resourceProvider = cacheInternal[cacheKey] as IResourceProvider;
            if (resourceProvider != null) {
                return resourceProvider;
            }

            EnsureResourceProviderFactory();
            resourceProvider = s_resourceProviderFactory.CreateGlobalResourceProvider(classKey);

            // Cache it
            cacheInternal.UtcInsert(cacheKey, resourceProvider);

            return resourceProvider;
        }

        // Get the page-level IResourceProvider
        internal static IResourceProvider GetLocalResourceProvider(TemplateControl templateControl) {
            return GetLocalResourceProvider(templateControl.VirtualPath);
        }

        // Get the page-level IResourceProvider
        internal static IResourceProvider GetLocalResourceProvider(VirtualPath virtualPath) {

            // If we have it cached, return it (it may be null if there are no local resources)
            CacheInternal cacheInternal = System.Web.HttpRuntime.CacheInternal;
            string cacheKey = CacheInternal.PrefixResourceProvider + virtualPath.VirtualPathString;
            IResourceProvider resourceProvider = cacheInternal[cacheKey] as IResourceProvider;
            if (resourceProvider != null) {
                return resourceProvider;
            }

            EnsureResourceProviderFactory();
            resourceProvider = s_resourceProviderFactory.CreateLocalResourceProvider(virtualPath.VirtualPathString);

            // Cache it
            cacheInternal.UtcInsert(cacheKey, resourceProvider);

            return resourceProvider;
        }

        // Create a ResourceExpressionFields without actually parsing any string.  This
        // is used for 'automatic' resources, where we already have the pieces of
        // relevant data.
        internal static object GetParsedData(string resourceKey) {
            return new ResourceExpressionFields(String.Empty, resourceKey);
        }
    }

    // Holds the fields parsed from a resource expression (e.g. classKey, resourceKey)
    public sealed class ResourceExpressionFields {
        private string _classKey;
        private string _resourceKey;

        internal ResourceExpressionFields(string classKey, string resourceKey) {
            _classKey = classKey;
            _resourceKey = resourceKey;
        }

        public string ClassKey {
            get {
                if (_classKey == null) {
                    return String.Empty;
                }

                return _classKey;
            }
        }

        public string ResourceKey {
            get {
                if (_resourceKey == null) {
                    return String.Empty;
                }

                return _resourceKey;
            }
        }
    }
}

