//------------------------------------------------------------------------------
// <copyright file="ProxyGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Web.Resources;

    public static class ProxyGenerator {

        const string WCFProxyTypeName = "System.ServiceModel.Description.WCFServiceClientProxyGenerator";
        const string WCFProxyMethodName = "GetClientProxyScript";

        public static string GetClientProxyScript(Type type, string path, bool debug) {
            return GetClientProxyScript(type, path, debug, null);
        }
    
        public static string GetClientProxyScript(Type type, string path, bool debug, ServiceEndpoint serviceEndpoint) {
            if (type ==  null) {
                throw new ArgumentNullException("type");
            }
            if (path == null) {
                throw new ArgumentNullException("path");
            }
            WebServiceData webServiceData = null; 
            ClientProxyGenerator proxyGenerator = null;
            if (IsWebServiceType(type)) {
                proxyGenerator = new WebServiceClientProxyGenerator(path, debug);
                webServiceData = new WebServiceData(type, false);
            }
            else if (IsPageType(type)) {
                proxyGenerator = new PageClientProxyGenerator(path, debug);
                webServiceData = new WebServiceData(type, true);
            }
            else if(IsWCFServiceType(type)) {
                // invoke the WCFServiceClientProxyGenerator.GetClientProxyScript method using reflection
                Assembly wcfWebAssembly = Assembly.Load(AssemblyRef.SystemServiceModelWeb);
                if (wcfWebAssembly != null) {
                    Type wcfProxyType = wcfWebAssembly.GetType(WCFProxyTypeName);
                    if (wcfProxyType != null) {
                        MethodInfo getClientProxyMethod = wcfProxyType.GetMethod(WCFProxyMethodName, BindingFlags.Static | BindingFlags.NonPublic);
                        if (getClientProxyMethod != null) {     
                            return getClientProxyMethod.Invoke(null, new object[] { type, path, debug, serviceEndpoint }) as string;
                        }
                    }
                }

                // in case the reflection fails, we should throw unsupported exception
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                    AtlasWeb.ProxyGenerator_UnsupportedType,
                    type.FullName));
            }
            else {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                    AtlasWeb.ProxyGenerator_UnsupportedType, 
                    type.FullName));
            }
            return proxyGenerator.GetClientProxyScript(webServiceData);
        }

        private static bool IsPageType(Type type) {
            return typeof(System.Web.UI.Page).IsAssignableFrom(type);
        }

        private static bool IsWCFServiceType(Type type) {
            object[] attribs = type.GetCustomAttributes(typeof(ServiceContractAttribute), true);
            return (attribs.Length != 0);
        }

        private static bool IsWebServiceType(Type type) {
            object[] attribs = type.GetCustomAttributes(typeof(ScriptServiceAttribute), true);
            return (attribs.Length != 0);
        }

    }
}
