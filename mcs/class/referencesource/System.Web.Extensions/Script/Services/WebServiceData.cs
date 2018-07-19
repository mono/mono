//------------------------------------------------------------------------------
// <copyright file="WebServiceData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Web.Services;

    internal class WebServiceData : JavaScriptTypeResolver {
        private WebServiceTypeData _typeData;
        private bool _pageMethods; // True for page methods(which only look at static methods)
        private Dictionary<string, WebServiceMethodData> _methods;

        // this is used to map __type ids in the JSON string to something other than type.FullName
        private Dictionary<string, string> _typeResolverSpecials = new Dictionary<string, string>();
        private Dictionary<string, WebServiceTypeData> _clientTypesDictionary;
        private Dictionary<Type, string> _clientTypeNameDictionary;
        private Dictionary<string, WebServiceEnumData> _enumTypesDictionary;
        private Hashtable _processedTypes;
        private bool _clientTypesProcessed;

        private JavaScriptSerializer _serializer;
        internal JavaScriptSerializer Serializer {
            get {
                return _serializer;
            }
        }


        internal const string _profileServiceFileName = "Profile_JSON_AppService.axd";
        internal const string _authenticationServiceFileName = "Authentication_JSON_AppService.axd";
        internal const string _roleServiceFileName = "Role_JSON_AppService.axd";

        private static WebServiceData GetApplicationService(string appRelativePath) {
            // we only support the application services being accessed at the root level, so that url authorization can be used to control their access.
            // In other words, "~/Profile_JSON_AppService.axd" should work but not "~/SomeSubDir/Profile_JSON_AppService.axd".
            // AppRelativeCurrentExecutionFilePath looks like "~/path/filename.ext".
            // So we can easily detect if the file requested is in the root by ensuring that the last index of "/" is == 1,
            // as in the path "~/rootfile.ext", where "/" is the second character.
            // Note that the WebServiceData object is cached higher in the stack once calculated
            int slashIndex = appRelativePath.LastIndexOf('/');
            if (slashIndex == 1) {
                // it is a root file. Now see if its one of the two built in services
                string name = Path.GetFileName(appRelativePath);

                if (name.Equals(_profileServiceFileName, StringComparison.OrdinalIgnoreCase)) {
                    return new WebServiceData(typeof(System.Web.Profile.ProfileService), false);
                }
                else if (name.Equals(_authenticationServiceFileName, StringComparison.OrdinalIgnoreCase)) {
                    return new WebServiceData(typeof(System.Web.Security.AuthenticationService), false);
                }
                else if (name.Equals(_roleServiceFileName, StringComparison.OrdinalIgnoreCase)) {
                    return new WebServiceData(typeof(System.Web.Security.RoleService), false);
                }
            }

            return null;
        }

        internal static WebServiceData GetWebServiceData(HttpContext context, string virtualPath) {
            return GetWebServiceData(context, virtualPath, true /*failIfNoData*/, false /*pageMethods*/, false/*inlineScript*/);
        }

        private static string GetCacheKey(string virtualPath) {
            return "System.Web.Script.Services.WebServiceData:" + virtualPath;
        }

        internal static WebServiceData GetWebServiceData(HttpContext context, string virtualPath, bool failIfNoData, bool pageMethods) {
            return GetWebServiceData(context, virtualPath, failIfNoData, pageMethods, false /*inlineScript*/);
        }

        [SecuritySafeCritical]
        internal static WebServiceData GetWebServiceData(HttpContext context, string virtualPath, bool failIfNoData, bool pageMethods, bool inlineScript) {
            // Make sure the path is cannonical to avoid doing more work than necessary
            virtualPath = VirtualPathUtility.ToAbsolute(virtualPath);

            string cacheKey = GetCacheKey(virtualPath);
            WebServiceData data = context.Cache[cacheKey] as WebServiceData;

            // Handle the case where the virtualPath exists, for example a real asmx page.
            if (data == null) {
                if (HostingEnvironment.VirtualPathProvider.FileExists(virtualPath)) {
                    Type compiledType = null;
                    try {
                        compiledType = BuildManager.GetCompiledType(virtualPath);

                        // If we can't get the compiled type, try creating an instance (i.e. for no compile pages)
                        if (compiledType == null) {
                            object page = BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(System.Web.UI.Page));
                            if (page != null) {
                                compiledType = page.GetType();
                            }
                        }

                    }
                    catch (SecurityException) {
                        // DevDiv 33708: BuildManager requires Medium trust, so we need to no-op rather than
                        // destroying the page.
                    }
                   
                    if (compiledType != null) {
                        data = new WebServiceData(compiledType, pageMethods);
                        BuildDependencySet deps = BuildManager.GetCachedBuildDependencySet(context, virtualPath);
                        if (deps != null) {
                            // Dev10 718863: It's possible 'deps' is null if the service is modified between GetCompiledType and here.
                            // in that case simply do not cache the result so it is re-established next time it is required.
                            CacheDependency cd = HostingEnvironment.VirtualPathProvider.GetCacheDependency(virtualPath, deps.VirtualPaths, DateTime.Now);
                            context.Cache.Insert(cacheKey, data, cd);
                        }
                    }
                }
                else if (virtualPath.EndsWith("_AppService.axd", StringComparison.OrdinalIgnoreCase)) {
                    // File does not exist, but the url may be a request for one of the three built-in services: ProfileService, AuthenticationService, RoleService
                    data = WebServiceData.GetApplicationService(context.Request.AppRelativeCurrentExecutionFilePath);
                    if (data != null) {
                        context.Cache.Insert(cacheKey, data);
                    }
                }
            }

            if (data == null) {
                if (failIfNoData) {
                    if (inlineScript) {
                        //DevDiv 74432: InlineScript = true fails, for WCF serviceReferences: Need an appropriate error message
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.WebService_NoWebServiceDataInlineScript, virtualPath));
                    }
                    else {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.WebService_NoWebServiceData, virtualPath));
                    }
                }
                else {
                    return null;
                }
            }

            return data;
        }

        internal WebServiceData() {
        }

        private WebServiceData(WebServiceTypeData typeData) {
            _typeData = typeData;
            _serializer = new JavaScriptSerializer(this);
#pragma warning disable 0436
            ScriptingJsonSerializationSection.ApplicationSettings settings = new ScriptingJsonSerializationSection.ApplicationSettings();
#pragma warning restore 0436
            _serializer.MaxJsonLength = settings.MaxJsonLimit;
            _serializer.RecursionLimit = settings.RecursionLimit;
            _serializer.RegisterConverters(settings.Converters);
        }

        // Normal ASMX Atlas codepath for creating webservice data
        internal WebServiceData(Type type, bool pageMethods)
            : this(new WebServiceTypeData(type.Name, type.Namespace, type)) {
            _pageMethods = pageMethods;
            // Pages don't need to have script service attribute
            if (!_pageMethods) {
                object[] attribs = type.GetCustomAttributes(typeof(ScriptServiceAttribute), true);
                if (attribs.Length == 0) {
                    throw new InvalidOperationException(AtlasWeb.WebService_NoScriptServiceAttribute);
                }
            }
        }

        // Indigo entry point for creating WebServiceData
        internal WebServiceData(WebServiceTypeData typeData, Dictionary<string, WebServiceMethodData> methods)
            : this(typeData) {
            _methods = methods;
        }

        private void AddMethod(Dictionary<string, WebServiceMethodData> methods, MethodInfo method) {
            object[] wmAttribs = method.GetCustomAttributes(typeof(WebMethodAttribute), true);

            // Skip it if it doesn't have the WebMethod attribute
            if (wmAttribs.Length == 0)
                return;

            ScriptMethodAttribute sm = null;
            object[] responseAttribs = method.GetCustomAttributes(typeof(ScriptMethodAttribute), true);
            if (responseAttribs.Length > 0) {
                sm = (ScriptMethodAttribute)responseAttribs[0];
            }

            // Create an object to keep track of this method's data
            WebServiceMethodData wmd = new WebServiceMethodData(this, method, (WebMethodAttribute)wmAttribs[0], sm);
            methods[wmd.MethodName] = wmd;
        }

        private void EnsureMethods() {
            // Type will only be null for the Indigo code path
            if (_methods != null || _typeData.Type == null)
                return;

            // Build the method collection on demand
            lock (this) {

                // Need to add the methods of each type in reverse order
                List<Type> typeList = new List<Type>();
                Type current = _typeData.Type;
                typeList.Add(current);
                while (current.BaseType != null) {
                    current = current.BaseType;
                    typeList.Add(current);
                }
                Dictionary<string, WebServiceMethodData> methods = new Dictionary<string, WebServiceMethodData>(StringComparer.OrdinalIgnoreCase);
                BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly;
                if (_pageMethods) flags |= BindingFlags.Static;
                else flags |= BindingFlags.Instance;

                // Add the methods in reverse order from base to derived
                for (int i = typeList.Count - 1; i >= 0; --i) {
                    MethodInfo[] methodInfos = typeList[i].GetMethods(flags);
                    foreach (MethodInfo method in methodInfos) {
                        AddMethod(methods, method);
                    }
                }
                _methods = methods;
            }
        }

        internal WebServiceTypeData TypeData {
            get { return _typeData; }
        }

        internal ICollection<WebServiceMethodData> MethodDatas {
            get {
                EnsureMethods();
                return _methods.Values;
            }
        }

        internal void ClearProcessedTypes() {
            _processedTypes = null;
        }

        internal void Initialize(WebServiceTypeData typeData, Dictionary<string, WebServiceMethodData> methods) {
            Dictionary<string, WebServiceTypeData> clientTypeDictionary = new Dictionary<string, WebServiceTypeData>();
            _clientTypesDictionary = clientTypeDictionary;
            Dictionary<string, WebServiceEnumData> enumTypeDictionary = new Dictionary<string, WebServiceEnumData>();
            _enumTypesDictionary = enumTypeDictionary;
            _processedTypes = new Hashtable();
            _clientTypesProcessed = true;
            _clientTypeNameDictionary = new Dictionary<Type, string>();
            _typeData = typeData;
            _methods = methods;
        }

        internal WebServiceMethodData GetMethodData(string methodName) {
            EnsureMethods();

            // Fail if the web method doesn't exist
            WebServiceMethodData methodData = null;
            if (!_methods.TryGetValue(methodName, out methodData)) {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, AtlasWeb.WebService_UnknownWebMethod, methodName), "methodName");
            }

            EnsureClientTypesProcessed();
            return methodData;
        }

        private void EnsureClientTypesProcessed() {
            if (_clientTypesProcessed)
                return;

            lock (this) {
                if (_clientTypesProcessed)
                    return;
                ProcessClientTypes();
            }
        }

        private void ProcessClientTypes() {
            Debug.Assert(!_clientTypesProcessed, "ProcessClientTypes shouldn't be called after it has already been successfully run.");

            // List of types that can be instantiated on the client
            _clientTypesDictionary = new Dictionary<string, WebServiceTypeData>();
            _enumTypesDictionary = new Dictionary<string, WebServiceEnumData>();

            _clientTypeNameDictionary = new Dictionary<Type, string>();

            try {
                // _processedTypes is used to avoid processing a Type more than once
                _processedTypes = new Hashtable();

                // Process any GenerateScriptTypes on the Service type
                ProcessIncludeAttributes((GenerateScriptTypeAttribute[])_typeData.Type.GetCustomAttributes(typeof(GenerateScriptTypeAttribute), true));

                foreach (WebServiceMethodData methodData in MethodDatas) {

                    // Process any GenerateScriptTypes on the method
                    ProcessIncludeAttributes((GenerateScriptTypeAttribute[])methodData.MethodInfo.GetCustomAttributes(typeof(GenerateScriptTypeAttribute), true));

                    // Also add any input parameters
                    foreach (WebServiceParameterData paramData in methodData.ParameterDatas) {
                        ProcessClientType(paramData.ParameterInfo.ParameterType);
                    }

                    // Ignore return type if it uses XML instead of JSON
                    if (methodData.UseXmlResponse) continue;
                    ProcessClientType(methodData.ReturnType);
                }

                // DevDiv 60672: Only set to true if the proxies were SUCCESSFULLY processed
                // Only setting _clientTypesProcessed=true on success will cause us to retry creating the proxies each
                // request when there is an exception, and so the same exception will be thrown each time.
                _clientTypesProcessed = true;
            }
            catch {
                // If we have any exception we have to null out our caches
                _clientTypesDictionary = null;
                _enumTypesDictionary = null;
                _clientTypeNameDictionary = null;
                throw;
            }
            finally {
                _processedTypes = null;
            }
        }

        private void ProcessIncludeAttributes(GenerateScriptTypeAttribute[] attributes) {
            foreach (GenerateScriptTypeAttribute attribute in attributes) {
                if (!String.IsNullOrEmpty(attribute.ScriptTypeId))
                    _typeResolverSpecials[attribute.Type.FullName] = attribute.ScriptTypeId;

                Type t = attribute.Type;
                if (t.IsPrimitive || t == typeof(object) || t == typeof(string) ||
                    t == typeof(DateTime) || t == typeof(Guid) ||
                    typeof(IEnumerable).IsAssignableFrom(t) || typeof(IDictionary).IsAssignableFrom(t) ||
                    (t.IsGenericType && t.GetGenericArguments().Length > 1) ||
                    !ObjectConverter.IsClientInstantiatableType(t, _serializer))
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        AtlasWeb.WebService_InvalidGenerateScriptType, t.FullName));

                ProcessClientType(t, true);
            }
        }

        private void ProcessClientType(Type t) {
            ProcessClientType(t, false, false);
        }


        private void ProcessClientType(Type t, bool force) {
            ProcessClientType(t, force, false);
        }

        // Force = true is required for when we detect a GenerateScriptType which may supply a new ScriptTypeID
        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "_clientTypeNameDictionary", Justification = "This is used by ASP.Net web services which is a legacy technology.")]
        internal void ProcessClientType(Type t, bool force, bool isWCF)
        {
            if (!force && _processedTypes.Contains(t))
                return;
            _processedTypes[t] = null;

            // Keep track of all enum Types
            if (t.IsEnum) {
                WebServiceEnumData enumData = null;
                if (isWCF) {
                    enumData = (WebServiceEnumData)WebServiceTypeData.GetWebServiceTypeData(t);
                }
                else {
                    enumData = new WebServiceEnumData(t.Name, t.Namespace, t, Enum.GetNames(t), Enum.GetValues(t), Enum.GetUnderlyingType(t) == typeof(ulong));
                }
                _enumTypesDictionary[GetTypeStringRepresentation(enumData.TypeName, false)] = enumData;
                return;
            }

            // For generics, we only allow generic types with one parameter, which we will try to process
            if (t.IsGenericType) {
                if (isWCF) {
                     ProcessKnownTypes(t);
                }
                else {
                    Type[] genericArgs = t.GetGenericArguments();
                    if (genericArgs.Length > 1) {
                        return;
                    }
                    ProcessClientType(genericArgs[0], false, isWCF);
                }
            }
            // Support arrays explicitly
            else if (t.IsArray) {
                ProcessClientType(t.GetElementType(), false, isWCF);
            }
            else {
                // Ignore primitive types
                // Ignore DateTime, since we have special serialization handling for it in the JavaScriptSerializer
                // Ignore IDctionary and IEnumerables as well
                if (t.IsPrimitive || t == typeof(object) || t == typeof(string) || t == typeof(DateTime) ||
                    t == typeof(void) || t == typeof(System.Decimal) || t == typeof(Guid) ||
                    typeof(IEnumerable).IsAssignableFrom(t) || typeof(IDictionary).IsAssignableFrom(t) ||
                    (!isWCF && !ObjectConverter.IsClientInstantiatableType(t, _serializer)))
                    return;

                // Only add it to the list of client types if it can be instantiated.
                // pass false to skip the lock
                if (isWCF) {
                    ProcessKnownTypes(t);
                }
                else {
                    string typeStringRepresentation = GetTypeStringRepresentation(t.FullName, false);
                    _clientTypesDictionary[typeStringRepresentation] = new WebServiceTypeData(t.Name, t.Namespace, t);
                    _clientTypeNameDictionary[t] = typeStringRepresentation;

                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "_clientTypeNameDictionary", Justification = "This is used by ASP.Net web services which is a legacy technology. Fun fact : The current code path suggests that this method is never hit!!")]
        private void ProcessKnownTypes(Type t)
        {
            WebServiceTypeData typeData = WebServiceTypeData.GetWebServiceTypeData(t);
            bool alreadyProcessed = false;
            if (typeData == null) {
                // indicates a type was used that is a built-in type
                return;
            }

            // if T implments IEnumerable or IDictionary, do not include type proxy for it
            // but still continue to get known types. I.e List<MyType> should ignore List<MyType> 
            // but process MyType
            if (!(typeof(IEnumerable).IsAssignableFrom(t) || typeof(IDictionary).IsAssignableFrom(t))) {
                 _clientTypeNameDictionary[t] = GetTypeStringRepresentation(typeData.TypeName);
                 alreadyProcessed = ProcessTypeData(typeData);
            }
            
            if (!alreadyProcessed) {
                IList<WebServiceTypeData> knownTypes = WebServiceTypeData.GetKnownTypes(t, typeData);
                foreach (WebServiceTypeData knownType in knownTypes) {
                    ProcessTypeData(knownType);
                }
            }

        }

        // returns true if typeData already exists in typeDictionary
        private bool ProcessTypeData(WebServiceTypeData typeData) {
            string typeString = GetTypeStringRepresentation(typeData.TypeName);
            bool retval = true;
            if (typeData is WebServiceEnumData) {
                if (!_enumTypesDictionary.ContainsKey(typeString)) {
                    _enumTypesDictionary[typeString] = (WebServiceEnumData)typeData;
                    retval = false;
                }
            }
            else {
                if (!_clientTypesDictionary.ContainsKey(typeString)) {
                    _clientTypesDictionary[typeString] = typeData;
                    retval = false;
                }
            }
            return retval;
        }


        internal IEnumerable<WebServiceTypeData> ClientTypes {
            get {
                return ClientTypeDictionary.Values;
            }
        }

        internal Dictionary<string, WebServiceTypeData> ClientTypeDictionary {
            get {
                EnsureClientTypesProcessed();
                return _clientTypesDictionary;
            }
            set {
                _clientTypesDictionary = value;
            }
        }

        internal Dictionary<Type, string> ClientTypeNameDictionary {
            get {
                EnsureClientTypesProcessed();
                return _clientTypeNameDictionary;
            }
        }

        internal IEnumerable<WebServiceEnumData> EnumTypes {
            get {
                EnsureClientTypesProcessed();
                return _enumTypesDictionary.Values;
            }
        }

        internal Dictionary<string, WebServiceEnumData> EnumTypeDictionary {
            get {
                EnsureClientTypesProcessed();
                return _enumTypesDictionary;
            }
            set {
                _enumTypesDictionary = value;
            }
        }

        public override Type ResolveType(string id) {
            WebServiceTypeData type = null;
            if (ClientTypeDictionary.TryGetValue(id, out type)) {
                if (type != null) {
                    return type.Type;
                }
            }
            return null;
        }

        public override string ResolveTypeId(Type type) {
            string typeString = GetTypeStringRepresentation(type.FullName);

            // If this type is not in the dictionary
            if (!ClientTypeDictionary.ContainsKey(typeString))
                return null;

            return typeString;
        }

        internal string GetTypeStringRepresentation(string typeName) {
            return GetTypeStringRepresentation(typeName, true);
        }

        internal string GetTypeStringRepresentation(string typeName, bool ensure) {
            if (ensure) {
                EnsureClientTypesProcessed();
            }

            // Handle special cases from GenerateScriptType first
            string typeString;
            if (_typeResolverSpecials.TryGetValue(typeName, out typeString)) {
                return typeString;
            }
            return typeName;
        }

        internal string GetTypeStringRepresentation(WebServiceTypeData typeData) {
            //First check if typeData provides its string representaiton ( for WCF case)
            string typeString = typeData.StringRepresentation;
            if (typeString == null) {
                typeString = GetTypeStringRepresentation(typeData.TypeName, true);
            }
            return typeString;
        }
    }
}
