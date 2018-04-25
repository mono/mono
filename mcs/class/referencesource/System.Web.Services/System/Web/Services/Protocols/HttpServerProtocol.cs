//------------------------------------------------------------------------------
// <copyright file="HttpServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;
    using System.Web.Services.Description;
    using System.Web.Services.Configuration;
    using System.Net;
    using System.Globalization;
    
    internal class HttpServerType : ServerType {
        Hashtable methods = new Hashtable();

        internal HttpServerType(Type type) : base(type) {
            WebServicesSection config = WebServicesSection.Current;
            Type[] returnWriterTypes = config.ReturnWriterTypes;
            Type[] parameterReaderTypes = config.ParameterReaderTypes;

            LogicalMethodInfo[] methodInfos = WebMethodReflector.GetMethods(type);
            HttpServerMethod[] methods = new HttpServerMethod[methodInfos.Length];

            object[] initializersByType = new object[returnWriterTypes.Length];
            for (int i = 0; i < initializersByType.Length; i++) {
                initializersByType[i] = MimeFormatter.GetInitializers(returnWriterTypes[i], methodInfos);
            }

            for (int i = 0; i < methodInfos.Length; i++) {
                LogicalMethodInfo methodInfo = methodInfos[i];
                HttpServerMethod method = null;
                if (methodInfo.ReturnType == typeof(void)) {
                    method = new HttpServerMethod();
                }
                else {
                    for (int j = 0; j < returnWriterTypes.Length; j++) {
                        object[] initializers = (object[])initializersByType[j];
                        if (initializers[i] != null) {
                            method = new HttpServerMethod();
                            method.writerInitializer = initializers[i];
                            method.writerType = returnWriterTypes[j];
                            break;
                        }
                    }
                }
                if (method != null) {
                    method.methodInfo = methodInfo;
                    methods[i] = method;
                }
            }

            initializersByType = new object[parameterReaderTypes.Length];
            for (int i = 0; i < initializersByType.Length; i++) {
                initializersByType[i] = MimeFormatter.GetInitializers(parameterReaderTypes[i], methodInfos);
            }

            for (int i = 0; i < methodInfos.Length; i++) {
                HttpServerMethod method = methods[i];
                if (method == null) continue;
                LogicalMethodInfo methodInfo = methodInfos[i];
                if (methodInfo.InParameters.Length > 0) {

                    int count = 0;
                    for (int j = 0; j < parameterReaderTypes.Length; j++) {
                        object[] initializers = (object[])initializersByType[j];
                        if (initializers[i] != null) {
                            count++;
                        }
                    }
                    if (count == 0) {
                        methods[i] = null;
                    }
                    else {
                        method.readerTypes = new Type[count];
                        method.readerInitializers = new object[count];
                        count = 0;
                        for (int j = 0; j < parameterReaderTypes.Length; j++) {
                            object[] initializers = (object[])initializersByType[j];
                            if (initializers[i] != null) {
                                method.readerTypes[count] = parameterReaderTypes[j];
                                method.readerInitializers[count] = initializers[i];
                                count++;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < methods.Length; i++) {
                HttpServerMethod method = methods[i];
                if (method != null) {
                    WebMethodAttribute methodAttribute = method.methodInfo.MethodAttribute;
                    method.name = methodAttribute.MessageName;
                    if (method.name.Length == 0) method.name = method.methodInfo.Name;
                    this.methods.Add(method.name, method);
                }
            }
        }

        internal HttpServerMethod GetMethod(string name) {
            return (HttpServerMethod)methods[name];
        }

        internal HttpServerMethod GetMethodIgnoreCase(string name) {
            foreach (DictionaryEntry entry in methods) {
                HttpServerMethod method = (HttpServerMethod)entry.Value;
                if (String.Compare(method.name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return method;
            }
            return null;
        }
    }

    internal class HttpServerMethod {
        internal string name;
        internal LogicalMethodInfo methodInfo;
        internal Type[] readerTypes;
        internal object[] readerInitializers;
        internal Type writerType;
        internal object writerInitializer;
    }

    internal abstract class HttpServerProtocol : ServerProtocol {
        HttpServerMethod serverMethod;
        HttpServerType serverType;
        bool hasInputPayload;

        protected HttpServerProtocol(bool hasInputPayload) { 
            this.hasInputPayload = hasInputPayload;
        }

        internal override bool Initialize() {
            // The derived class better check the verb!

            string methodName = Request.PathInfo.Substring(1);   // Skip leading '/'

            if (null == (serverType = (HttpServerType)GetFromCache(typeof(HttpServerProtocol), Type))
                && null == (serverType = (HttpServerType)GetFromCache(typeof(HttpServerProtocol), Type, true)))
            {
                lock (InternalSyncObject)
                {
                    if (null == (serverType = (HttpServerType)GetFromCache(typeof(HttpServerProtocol), Type))
                        && null == (serverType = (HttpServerType)GetFromCache(typeof(HttpServerProtocol), Type, true)))
                    {
                        bool excludeSchemeHostPortFromCachingKey = this.IsCacheUnderPressure(typeof(HttpServerProtocol), Type);
                        serverType = new HttpServerType(Type);
                        AddToCache(typeof(HttpServerProtocol), Type, serverType, excludeSchemeHostPortFromCachingKey);
                    }
                }
            }

            serverMethod = serverType.GetMethod(methodName);
            if (serverMethod == null) {
                serverMethod = serverType.GetMethodIgnoreCase(methodName);
                if (serverMethod != null) 
                    throw new ArgumentException(Res.GetString(Res.WebInvalidMethodNameCase, methodName, serverMethod.name), "methodName");
                else {
                    // it's possible that the method name came in as UTF-8 but was mangled by IIS so we try it
                    // again as UTF8...
                    string utf8MethodName = Encoding.UTF8.GetString(Encoding.Default.GetBytes(methodName));
                    serverMethod = serverType.GetMethod(utf8MethodName);
                    if (serverMethod == null)
                        throw new InvalidOperationException(Res.GetString(Res.WebInvalidMethodName, methodName));
                }
            }

            return true;
        }

        internal override bool IsOneWay {
            get { return false; }            
        }                                                                           
                                                                     
        internal override LogicalMethodInfo MethodInfo {
            get { return serverMethod.methodInfo; }
        }

        internal override ServerType ServerType {
            get { return serverType; }
        }
        
        internal override object[] ReadParameters() {
            if (serverMethod.readerTypes == null) return new object[0];
            for (int i = 0; i < serverMethod.readerTypes.Length; i++) {
                if (!hasInputPayload) {
                    // only allow URL parameters if doesn't have payload
                    if (serverMethod.readerTypes[i] != typeof(UrlParameterReader)) continue;
                }
                else {
                    // don't allow URL params if has payload
                    if (serverMethod.readerTypes[i] == typeof(UrlParameterReader)) continue;
                }
                MimeParameterReader reader = (MimeParameterReader)MimeFormatter.CreateInstance(serverMethod.readerTypes[i], 
                                                                                               serverMethod.readerInitializers[i]);
                
                object[] parameters = reader.Read(Request);
                if (parameters != null) return parameters;                                                                                    
            }
            if (!hasInputPayload)
                throw new InvalidOperationException(Res.GetString(Res.WebInvalidRequestFormat));
            else
                throw new InvalidOperationException(Res.GetString(Res.WebInvalidRequestFormatDetails, Request.ContentType));
        }

        internal override void WriteReturns(object[] returnValues, Stream outputStream) {
            if (serverMethod.writerType == null) return;
            MimeReturnWriter writer = (MimeReturnWriter)MimeFormatter.CreateInstance(serverMethod.writerType,
                                                                                     serverMethod.writerInitializer);
            writer.Write(Response, outputStream, returnValues[0]);
        }

        internal override bool WriteException(Exception e, Stream outputStream) {
            Response.Clear();
            Response.ClearHeaders();
            Response.ContentType = ContentType.Compose("text/plain", Encoding.UTF8);
            SetHttpResponseStatusCode(Response, (int)HttpStatusCode.InternalServerError);
            Response.StatusDescription = HttpWorkerRequest.GetStatusDescription(Response.StatusCode);
            StreamWriter writer = new StreamWriter(outputStream, new UTF8Encoding(false));
            if (System.Web.Services.Configuration.WebServicesSection.Current.Diagnostics.SuppressReturningExceptions) {
                writer.WriteLine(Res.GetString(Res.WebSuppressedExceptionMessage));
            }
            else {
                writer.WriteLine(GenerateFaultString(e, true));
            }
            writer.Flush();
            return true;
        }

        internal static bool AreUrlParametersSupported(LogicalMethodInfo methodInfo) {
            if (methodInfo.OutParameters.Length > 0) return false;
            ParameterInfo[] parameters = methodInfo.InParameters;
            for (int i = 0; i < parameters.Length; i++) {
                ParameterInfo parameter = parameters[i];
                Type parameterType = parameter.ParameterType;
                if (parameterType.IsArray) {
                    if (!ScalarFormatter.IsTypeSupported(parameterType.GetElementType())) 
                        return false;
                }
                else {
                    if (!ScalarFormatter.IsTypeSupported(parameterType))
                        return false;
                }
            }
            return true;
        }
    }

}
