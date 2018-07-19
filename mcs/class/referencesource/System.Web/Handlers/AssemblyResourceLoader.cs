//------------------------------------------------------------------------------
// <copyright file="AssemblyResourceLoader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.Handlers {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.RegularExpressions;
    using System.Web.Security.Cryptography;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    /// Provides a way to load client-side resources from assemblies
    /// </devdoc>
    public sealed class AssemblyResourceLoader : IHttpHandler {
        private const string _webResourceUrl = "WebResource.axd";

        private readonly static Regex webResourceRegex = new WebResourceRegex();

        private static IDictionary _urlCache = Hashtable.Synchronized(new Hashtable());
        private static IDictionary _assemblyInfoCache = Hashtable.Synchronized(new Hashtable());
        private static IDictionary _webResourceCache = Hashtable.Synchronized(new Hashtable());
        private static IDictionary _typeAssemblyCache = Hashtable.Synchronized(new Hashtable());

        // This group of fields is used for backwards compatibility. In v1.x you could
        // technically customize the files in the /aspnet_client/ folder whereas in v2.x
        // we serve those files using WebResource.axd. These fields are used to check
        // if there is a customized version of the file and use that instead of the resource.
        private static bool _webFormsScriptChecked;
        private static VirtualPath _webFormsScriptLocation;
        private static bool _webUIValidationScriptChecked;
        private static VirtualPath _webUIValidationScriptLocation;
        private static bool _smartNavScriptChecked;
        private static VirtualPath _smartNavScriptLocation;
        private static bool _smartNavPageChecked;
        private static VirtualPath _smartNavPageLocation;

        private static bool _handlerExistenceChecked;
        private static bool _handlerExists;
        // set by unit tests to avoid dependency on httpruntime.
        internal static string _applicationRootPath;

        private static bool DebugMode {
            get {
                return HttpContext.Current.IsDebuggingEnabled;
            }
        }

        /// <devdoc>
        ///     Create a cache key for the UrlCache.  
        ///
        ///     requirement:  If assembly1 and assembly2 represent the same assembly, 
        ///     then they must be the same object; otherwise this method will fail to generate 
        ///     a unique cache key.
        /// </devdoc>
        private static int CreateWebResourceUrlCacheKey(Assembly assembly, string resourceName,
            bool htmlEncoded, bool forSubstitution, bool enableCdn, bool debuggingEnabled, bool secureConnection) {
            int hashCode = HashCodeCombiner.CombineHashCodes(
                assembly.GetHashCode(),
                resourceName.GetHashCode(),
                htmlEncoded.GetHashCode(),
                forSubstitution.GetHashCode(),
                enableCdn.GetHashCode());
            return HashCodeCombiner.CombineHashCodes(hashCode,
                debuggingEnabled.GetHashCode(),
                secureConnection.GetHashCode());
        }

        /// <devdoc>
        /// Validates that the WebResource.axd handler is registered in config and actually
        /// points to the correct handler type.
        /// </devdoc>
        private static void EnsureHandlerExistenceChecked() {
            // First we have to check that the handler is registered:
            // <add path="WebResource.axd" verb="GET" type="System.Web.Handlers.AssemblyResourceLoader" validate="True" />
            if (!_handlerExistenceChecked) { 
                HttpContext context = HttpContext.Current;
                IIS7WorkerRequest iis7WorkerRequest = (context != null) ? context.WorkerRequest as IIS7WorkerRequest : null;
                string webResourcePath = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, _webResourceUrl);
                if (iis7WorkerRequest != null) {
                    // check the IIS <handlers> section by mapping the handler
                    string handlerTypeString = iis7WorkerRequest.MapHandlerAndGetHandlerTypeString(method: "GET",
                                               path: UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, _webResourceUrl),
                                               convertNativeStaticFileModule: false, ignoreWildcardMappings: true);
                    if (!String.IsNullOrEmpty(handlerTypeString)) {
                        _handlerExists = (typeof(AssemblyResourceLoader) == BuildManager.GetType(handlerTypeString, true /*throwOnFail*/, false /*ignoreCase*/));
                    }
                }
                else {
                    // check the <httpHandlers> section
                    HttpHandlerAction httpHandler = RuntimeConfig.GetConfig(VirtualPath.Create(webResourcePath)).HttpHandlers.FindMapping("GET", VirtualPath.Create(_webResourceUrl));
                    _handlerExists = (httpHandler != null) && (httpHandler.TypeInternal == typeof(AssemblyResourceLoader));
                }                
                _handlerExistenceChecked = true;
            }
        }

        /// <devdoc>
        ///     Performs the actual putting together of the resource reference URL.
        /// </devdoc>
        private static string FormatWebResourceUrl(string assemblyName, string resourceName, long assemblyDate, bool htmlEncoded) {
            string encryptedData = Page.EncryptString(assemblyName + "|" + resourceName, Purpose.AssemblyResourceLoader_WebResourceUrl);
            if (htmlEncoded) {
                return String.Format(CultureInfo.InvariantCulture, _webResourceUrl + "?d={0}&amp;t={1}",
                                    encryptedData,
                                    assemblyDate);
            }
            else {
                return String.Format(CultureInfo.InvariantCulture, _webResourceUrl + "?d={0}&t={1}",
                                    encryptedData,
                                    assemblyDate);
            }
        }

        internal static Assembly GetAssemblyFromType(Type type) {
            Assembly assembly = (Assembly)_typeAssemblyCache[type];
            if (assembly == null) {
                assembly = type.Assembly;
                _typeAssemblyCache[type] = assembly;
            }
            return assembly;
        }

        private static Pair GetAssemblyInfo(Assembly assembly) {
            Pair assemblyInfo = _assemblyInfoCache[assembly] as Pair;
            if (assemblyInfo == null) {
                assemblyInfo = GetAssemblyInfoWithAssertInternal(assembly);
                _assemblyInfoCache[assembly] = assemblyInfo;
            }
            Debug.Assert(assemblyInfo != null, "Assembly info should not be null");
            return assemblyInfo;
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        private static Pair GetAssemblyInfoWithAssertInternal(Assembly assembly) {
            AssemblyName assemblyName = assembly.GetName();
            long assemblyDate = File.GetLastWriteTime(new Uri(assemblyName.CodeBase).LocalPath).Ticks;
            Pair assemblyInfo = new Pair(assemblyName, assemblyDate);
            return assemblyInfo;
        }

        /// <devdoc>
        /// Gets the virtual path of a physical resource file. Null is
        /// returned if the resource does not exist.
        /// We assert full FileIOPermission so that we can map paths.
        /// </devdoc>
        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        private static VirtualPath GetDiskResourcePath(string resourceName) {
            VirtualPath clientScriptsLocation = Util.GetScriptLocation();
            VirtualPath resourceVirtualPath = clientScriptsLocation.SimpleCombine(resourceName);
            string resourcePhysicalPath = resourceVirtualPath.MapPath();
            if (File.Exists(resourcePhysicalPath)) {
                return resourceVirtualPath;
            }
            else {
                return null;
            }
        }

        internal static string GetWebResourceUrl(Type type, string resourceName) {
            return GetWebResourceUrl(type, resourceName, false, null);
        }

        internal static string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded) {
            return GetWebResourceUrl(type, resourceName, htmlEncoded, null);
        }

        internal static string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded, IScriptManager scriptManager) {
            bool enableCdn = (scriptManager != null && scriptManager.EnableCdn);
            return GetWebResourceUrl(type, resourceName, htmlEncoded, scriptManager, enableCdn: enableCdn);
        }

        /// <devdoc>
        ///     Gets a URL resource reference to a client-side resource
        /// </devdoc>
        internal static string GetWebResourceUrl(Type type, string resourceName, bool htmlEncoded, IScriptManager scriptManager, bool enableCdn) {
            Assembly assembly = GetAssemblyFromType(type);
            Debug.Assert(assembly != null, "Type.Assembly should never be null.");

            // If the resource request is for System.Web.dll and more specifically
            // it is for a file that we shipped in v1.x, we have to check if a
            // customized copy of the file exists. See notes at the top of the file
            // regarding this.
            if (assembly == typeof(AssemblyResourceLoader).Assembly) {
                if (String.Equals(resourceName, "WebForms.js", StringComparison.Ordinal)) {
                    if (!_webFormsScriptChecked) {
                        _webFormsScriptLocation = GetDiskResourcePath(resourceName);
                        _webFormsScriptChecked = true;
                    }
                    if (_webFormsScriptLocation != null) {
                        return _webFormsScriptLocation.VirtualPathString;
                    }
                }
                else if (String.Equals(resourceName, "WebUIValidation.js", StringComparison.Ordinal)) {
                    if (!_webUIValidationScriptChecked) {
                        _webUIValidationScriptLocation = GetDiskResourcePath(resourceName);
                        _webUIValidationScriptChecked = true;
                    }
                    if (_webUIValidationScriptLocation != null) {
                        return _webUIValidationScriptLocation.VirtualPathString;
                    }
                }
                else if (String.Equals(resourceName, "SmartNav.htm", StringComparison.Ordinal)) {
                    if (!_smartNavPageChecked) {
                        _smartNavPageLocation = GetDiskResourcePath(resourceName);
                        _smartNavPageChecked = true;
                    }
                    if (_smartNavPageLocation != null) {
                        return _smartNavPageLocation.VirtualPathString;
                    }
                }
                else if (String.Equals(resourceName, "SmartNav.js", StringComparison.Ordinal)) {
                    if (!_smartNavScriptChecked) {
                        _smartNavScriptLocation = GetDiskResourcePath(resourceName);
                        _smartNavScriptChecked = true;
                    }
                    if (_smartNavScriptLocation != null) {
                        return _smartNavScriptLocation.VirtualPathString;
                    }
                }
            }

            return GetWebResourceUrlInternal(assembly, resourceName, htmlEncoded, false, scriptManager, enableCdn);
        }

        private static WebResourceAttribute FindWebResourceAttribute(Assembly assembly, string resourceName) {
            object[] attrs = assembly.GetCustomAttributes(false);
            for (int i = 0; i < attrs.Length; i++) {
                WebResourceAttribute wra = attrs[i] as WebResourceAttribute;
                if ((wra != null) && String.Equals(wra.WebResource, resourceName, StringComparison.Ordinal)) {
                    return wra;
                }
            }
            return null;
        }

        internal static string FormatCdnUrl(Assembly assembly, string cdnPath) {
            // {0} = Short Assembly Name
            // {1} = Assembly Version
            // {2} = Assembly File Version
            // use a new AssemblyName because assembly.GetName() doesn't work in medium trust.
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            return String.Format(CultureInfo.InvariantCulture,
                cdnPath,
                HttpUtility.UrlEncode(assemblyName.Name),
                HttpUtility.UrlEncode(assemblyName.Version.ToString(4)),
                HttpUtility.UrlEncode(AssemblyUtil.GetAssemblyFileVersion(assembly)));
        }

        private static string GetCdnPath(string resourceName, Assembly assembly, bool secureConnection) {
            string cdnPath = null;
            WebResourceAttribute wra = FindWebResourceAttribute(assembly, resourceName);
            if (wra != null) {
                cdnPath = secureConnection ? wra.CdnPathSecureConnection : wra.CdnPath;
                if (!String.IsNullOrEmpty(cdnPath)) {
                    cdnPath = FormatCdnUrl(assembly, cdnPath);
                }
            }
            return cdnPath;
        }

        internal static string GetWebResourceUrlInternal(Assembly assembly, string resourceName,
                bool htmlEncoded, bool forSubstitution, IScriptManager scriptManager) {

            bool enableCdn = (scriptManager != null && scriptManager.EnableCdn);
            return GetWebResourceUrlInternal(assembly, resourceName, htmlEncoded, forSubstitution, scriptManager, enableCdn: enableCdn);
        }

        internal static string GetWebResourceUrlInternal(Assembly assembly, string resourceName,
            bool htmlEncoded, bool forSubstitution, IScriptManager scriptManager, bool enableCdn) {
            // When this url is being inserted as a substitution in another resource,
            // it should just be "WebResource.axd?d=..." since the resource is already coming
            // from the app root (i.e. no need for a full absolute /app/WebResource.axd).
            // Otherwise we must return a path that is absolute (starts with '/') or
            // a full absolute uri (http://..) as in the case of a CDN Path.
            
            EnsureHandlerExistenceChecked();
            if (!_handlerExists) {
                throw new InvalidOperationException(SR.GetString(SR.AssemblyResourceLoader_HandlerNotRegistered));
            }
            Assembly effectiveAssembly = assembly;
            string effectiveResourceName = resourceName;

            bool debuggingEnabled = false;
            bool secureConnection;
            if (scriptManager != null) {
                debuggingEnabled = scriptManager.IsDebuggingEnabled;
                secureConnection = scriptManager.IsSecureConnection;
            }
            else {
                secureConnection = ((HttpContext.Current != null) && (HttpContext.Current.Request != null) &&
                    HttpContext.Current.Request.IsSecureConnection);
                debuggingEnabled = (HttpContext.Current != null) && HttpContext.Current.IsDebuggingEnabled;
            }
            int urlCacheKey = CreateWebResourceUrlCacheKey(assembly, resourceName, htmlEncoded,
                forSubstitution, enableCdn, debuggingEnabled, secureConnection);

            string url = (string)_urlCache[urlCacheKey];

            if (url == null) {
                IScriptResourceDefinition definition = null;
                if (ClientScriptManager._scriptResourceMapping != null) {
                    definition = ClientScriptManager._scriptResourceMapping.GetDefinition(resourceName, assembly);
                    if (definition != null) {
                        if (!String.IsNullOrEmpty(definition.ResourceName)) {
                            effectiveResourceName = definition.ResourceName;
                        }
                        if (definition.ResourceAssembly != null) {
                            effectiveAssembly = definition.ResourceAssembly;
                        }
                    }
                } 
                string path = null;
                // if a resource mapping exists, take it's settings into consideration
                // it might supply a path or a cdnpath.
                if (definition != null) {
                    if (enableCdn) {
                        // Winner is first path defined, falling back on the effectiveResourceName/Assembly
                        // Debug Mode  : d.CdnDebugPath, d.DebugPath, *wra.CdnPath, d.Path
                        // Release Mode: d.CdnPath                  , *wra.CdnPath, d.Path
                        // * the WebResourceAttribute corresponding to the resource defined in the definition, not the
                        //  the original resource.
                        // Also, if the definition has a CdnPath but it cannot be converted to a secure one during https,
                        // the WRA's CdnPath is not considered.
                        if (debuggingEnabled) {
                            path = secureConnection ? definition.CdnDebugPathSecureConnection : definition.CdnDebugPath;
                            if (String.IsNullOrEmpty(path)) {
                                path = definition.DebugPath;
                                if (String.IsNullOrEmpty(path)) {
                                    // Get CDN Path from the redirected resource name/assembly, not the original one,
                                    // but not if this is a secure connection and the only reason we didn't use the definition
                                    // cdn path is because it doesnt support secure connections.
                                    if (!secureConnection || String.IsNullOrEmpty(definition.CdnDebugPath)) {
                                        path = GetCdnPath(effectiveResourceName, effectiveAssembly, secureConnection);
                                    }
                                    if (String.IsNullOrEmpty(path)) {
                                        path = definition.Path;
                                    }
                                }
                            }
                        }
                        else {
                            path = secureConnection ? definition.CdnPathSecureConnection : definition.CdnPath;
                            if (String.IsNullOrEmpty(path)) {
                                // Get CDN Path from the redirected resource name/assembly, not the original one
                                // but not if this is a secure connection and the only reason we didn't use the definition
                                // cdn path is because it doesnt support secure connections.
                                if (!secureConnection || String.IsNullOrEmpty(definition.CdnPath)) {
                                    path = GetCdnPath(effectiveResourceName, effectiveAssembly, secureConnection);
                                }
                                if (String.IsNullOrEmpty(path)) {
                                    path = definition.Path;
                                }
                            }
                        }
                    } // cdn
                    else {
                        // Winner is first path defined, falling back on the effectiveResourceName/Assembly
                        // Debug Mode  : d.DebugPath, d.Path
                        // Release Mode: d.Path
                        if (debuggingEnabled) {
                            path = definition.DebugPath;
                            if (String.IsNullOrEmpty(path)) {
                                path = definition.Path;
                            }
                        }
                        else {
                            path = definition.Path;
                        }
                    }
                } // does not have definition
                else if (enableCdn) {
                    path = GetCdnPath(effectiveResourceName, effectiveAssembly, secureConnection);
                }

                if (!String.IsNullOrEmpty(path)) {
                    // assembly based resource has been overridden by a path,
                    // whether that be a CDN Path or a definition.Path or DebugPath.
                    // We must return a path that is absolute (starts with '/') or
                    // a full absolute uri (http://..) as in the case of a CDN Path.
                    // An overridden Path that is not a CDN Path is required to be absolute
                    // or app relative.
                    if (UrlPath.IsAppRelativePath(path)) {
                        // expand ~/. If it is rooted (/) or an absolute uri, no conversion needed
                        if (_applicationRootPath == null) {
                            url = VirtualPathUtility.ToAbsolute(path);
                        }
                        else {
                            url = VirtualPathUtility.ToAbsolute(path, _applicationRootPath);
                        }
                    }
                    else {
                        // must be a full uri or already rooted.
                        url = path;
                    }
                    if (htmlEncoded) {
                        url = HttpUtility.HtmlEncode(url);
                    }
                }
                else {
                    string urlAssemblyName;
                    Pair assemblyInfo = GetAssemblyInfo(effectiveAssembly);
                    AssemblyName assemblyName = (AssemblyName)assemblyInfo.First;
                    long assemblyDate = (long)assemblyInfo.Second;
                    string assemblyVersion = assemblyName.Version.ToString();

                    if (effectiveAssembly.GlobalAssemblyCache) {
                        // If the assembly is in the GAC, we need to store a full name to load the assembly later
                        if (effectiveAssembly == HttpContext.SystemWebAssembly) {
                            urlAssemblyName = "s";
                        }
                        else {
                            // Pack the necessary values into a more compact format than FullName
                            StringBuilder builder = new StringBuilder();
                            builder.Append('f');
                            builder.Append(assemblyName.Name);
                            builder.Append(',');
                            builder.Append(assemblyVersion);
                            builder.Append(',');
                            if (assemblyName.CultureInfo != null) {
                                builder.Append(assemblyName.CultureInfo.ToString());
                            }
                            builder.Append(',');
                            byte[] token = assemblyName.GetPublicKeyToken();
                            for (int i = 0; i < token.Length; i++) {
                                builder.Append(token[i].ToString("x2", CultureInfo.InvariantCulture));
                            }
                            urlAssemblyName = builder.ToString();
                        }
                    }
                    else {
                        // Otherwise, we can just use a partial name
                        urlAssemblyName = "p" + assemblyName.Name;
                    }
                    url = FormatWebResourceUrl(urlAssemblyName, effectiveResourceName, assemblyDate, htmlEncoded);
                    if (!forSubstitution && (HttpRuntime.AppDomainAppVirtualPathString != null)) {
                        // When this url is being inserted as a substitution in another resource,
                        // it should just be "WebResource.axd?d=..." since the resource is already coming
                        // from the app root (i.e. no need for a full absolute /app/WebResource.axd).
                        url = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, url);
                    }
                }
                _urlCache[urlCacheKey] = url;
            }
            return url;
        }

        internal static bool IsValidWebResourceRequest(HttpContext context) {
            EnsureHandlerExistenceChecked();
            if (!_handlerExists) {
                // If the handler isn't properly registered, it can't
                // possibly be a valid web resource request.
                return false;
            }

            string webResourceHandlerUrl = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, _webResourceUrl);
            string requestPath = context.Request.Path;
            if (String.Equals(requestPath, webResourceHandlerUrl, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }

            return false;
        }

        internal static void LogWebResourceFailure(string decryptedData, Exception exception) {
            string errorMessage = null;
            if (decryptedData != null) {
                errorMessage = SR.GetString(SR.Webevent_msg_RuntimeErrorWebResourceFailure_ResourceMissing, decryptedData);
            }
            else {
                errorMessage = SR.GetString(SR.Webevent_msg_RuntimeErrorWebResourceFailure_DecryptionError);
            }
            WebBaseEvent.RaiseSystemEvent(message: errorMessage,
                source: null,
                eventCode: WebEventCodes.RuntimeErrorWebResourceFailure,
                eventDetailCode: WebEventCodes.UndefinedEventDetailCode,
                exception: exception);
        }

        /// <internalonly/>
        bool IHttpHandler.IsReusable {
            get {
                return true;
            }
        }


        /// <internalonly/>
        void IHttpHandler.ProcessRequest(HttpContext context) {
            // Make sure we don't get any extra content in this handler (like Application.BeginRequest stuff);
            context.Response.Clear();

            Stream resourceStream = null;
            string decryptedData = null;
            bool resourceIdentifierPresent = false;

            Exception exception = null;
            try {
                NameValueCollection queryString = context.Request.QueryString;

                string encryptedData = queryString["d"];
                if (String.IsNullOrEmpty(encryptedData)) {
                    throw new HttpException(404, SR.GetString(SR.AssemblyResourceLoader_InvalidRequest));
                }
                resourceIdentifierPresent = true;

                decryptedData = Page.DecryptString(encryptedData, Purpose.AssemblyResourceLoader_WebResourceUrl);

                int separatorIndex = decryptedData.IndexOf('|');
                Debug.Assert(separatorIndex != -1, "The decrypted data must contain a separator.");

                string assemblyName = decryptedData.Substring(0, separatorIndex);
                if (String.IsNullOrEmpty(assemblyName)) {
                    throw new HttpException(404, SR.GetString(SR.AssemblyResourceLoader_AssemblyNotFound, assemblyName));
                }

                string resourceName = decryptedData.Substring(separatorIndex + 1);
                if (String.IsNullOrEmpty(resourceName)) {
                    throw new HttpException(404, SR.GetString(SR.AssemblyResourceLoader_ResourceNotFound, resourceName));
                }

                char nameType = assemblyName[0];
                assemblyName = assemblyName.Substring(1);

                Assembly assembly = null;

                // If it was a full name, create an AssemblyName and load from that
                if (nameType == 'f') {
                    string[] parts = assemblyName.Split(',');

                    if (parts.Length != 4) {
                        throw new HttpException(404, SR.GetString(SR.AssemblyResourceLoader_InvalidRequest));
                    }

                    AssemblyName realName = new AssemblyName();
                    realName.Name = parts[0];
                    realName.Version = new Version(parts[1]);
                    string cultureString = parts[2];

                    // Try to determine the culture, using the invariant culture if there wasn't one (doesn't work without it)
                    if (cultureString.Length > 0) {
                        realName.CultureInfo = new CultureInfo(cultureString);
                    }
                    else {
                        realName.CultureInfo = CultureInfo.InvariantCulture;
                    }

                    // Parse up the public key token which is represented as hex bytes in a string
                    string token = parts[3];
                    byte[] tokenBytes = new byte[token.Length / 2];
                    for (int i = 0; i < tokenBytes.Length; i++) {
                        tokenBytes[i] = Byte.Parse(token.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }
                    realName.SetPublicKeyToken(tokenBytes);

                    assembly = Assembly.Load(realName);
                }
                // System.Web special case
                else if (nameType == 's') {
                    assembly = typeof(AssemblyResourceLoader).Assembly;
                }
                // If was a partial name, just try to load it
                else if (nameType == 'p') {
                    assembly = Assembly.Load(assemblyName);
                }
                else {
                    throw new HttpException(404, SR.GetString(SR.AssemblyResourceLoader_InvalidRequest));
                }

                // Dev10 Bugs 602949: Throw 404 if resource not found rather than do nothing.
                // This is done before creating the cache entry, since it could be that the assembly is loaded
                // later on without the app restarting.
                if (assembly == null) {
                    throw new HttpException(404, SR.GetString(SR.AssemblyResourceLoader_InvalidRequest));
                }

                bool performSubstitution = false;
                bool validResource = false;
                string contentType = String.Empty;

                // Check the validation cache to see if the resource has already been validated
                int cacheKey = HashCodeCombiner.CombineHashCodes(assembly.GetHashCode(), resourceName.GetHashCode());
                Triplet resourceTriplet = (Triplet)_webResourceCache[cacheKey];
                if (resourceTriplet != null) {
                    validResource = (bool)resourceTriplet.First;
                    contentType = (string)resourceTriplet.Second;
                    performSubstitution = (bool)resourceTriplet.Third;
                }
                else {
                    // Validation cache is empty, find out if it's valid and add it to the cache
                    WebResourceAttribute wra = FindWebResourceAttribute(assembly, resourceName);
                    if (wra != null) {
                        resourceName = wra.WebResource;
                        validResource = true;
                        contentType = wra.ContentType;
                        performSubstitution = wra.PerformSubstitution;
                    }

                    // Cache the result so we don't have to do this again
                    try {
                        if (validResource) {
                            // a WebResourceAttribute was found, but does the resource really exist?
                            validResource = false;
                            resourceStream = assembly.GetManifestResourceStream(resourceName);
                            validResource = (resourceStream != null);
                        }
                    }
                    finally {
                        // Cache the results, even if there was an exception getting the stream,
                        // so we don't have to do this again
                        Triplet triplet = new Triplet();
                        triplet.First = validResource;
                        triplet.Second = contentType;
                        triplet.Third = performSubstitution;
                        _webResourceCache[cacheKey] = triplet;
                    }
                }

                if (validResource) {
                    // Cache the resource so we don't keep processing the same requests
                    HttpCachePolicy cachePolicy = context.Response.Cache;
                    cachePolicy.SetCacheability(HttpCacheability.Public);
                    cachePolicy.VaryByParams["d"] = true;
                    cachePolicy.SetOmitVaryStar(true);
                    cachePolicy.SetExpires(DateTime.Now + TimeSpan.FromDays(365));
                    cachePolicy.SetValidUntilExpires(true);
                    Pair assemblyInfo = GetAssemblyInfo(assembly);
                    cachePolicy.SetLastModified(new DateTime((long)assemblyInfo.Second));

                    StreamReader reader = null;
                    try {
                        if (resourceStream == null) {
                            // null in the case that _webResourceCache had the item
                            resourceStream = assembly.GetManifestResourceStream(resourceName);
                        }
                        if (resourceStream != null) {
                            context.Response.ContentType = contentType;

                            if (performSubstitution) {
                                // 
                                reader = new StreamReader(resourceStream, true);
                            
                                string content = reader.ReadToEnd();
                            
                                // Looking for something of the form: WebResource("resourcename")
                                MatchCollection matches = webResourceRegex.Matches(content);
                                int startIndex = 0;
                                StringBuilder newContent = new StringBuilder();
                                foreach (Match match in matches) {
                                    newContent.Append(content.Substring(startIndex, match.Index - startIndex));
                                
                                    Group group = match.Groups["resourceName"];
                                    if (group != null) {
                                        string embeddedResourceName = group.ToString();
                                        if (embeddedResourceName.Length > 0) {
                                            // 
                                            if (String.Equals(embeddedResourceName, resourceName, StringComparison.Ordinal)) {
                                                throw new HttpException(404, SR.GetString(SR.AssemblyResourceLoader_NoCircularReferences, resourceName));
                                            }
                                            newContent.Append(GetWebResourceUrlInternal(assembly, embeddedResourceName, htmlEncoded: false, forSubstitution: true, scriptManager: null));
                                        }
                                    }
                                
                                    startIndex = match.Index + match.Length;
                                }

                                newContent.Append(content.Substring(startIndex, content.Length - startIndex));
                            
                                StreamWriter writer = new StreamWriter(context.Response.OutputStream, reader.CurrentEncoding);
                                writer.Write(newContent.ToString());
                                writer.Flush();
                            }
                            else {
                                byte[] buffer = new byte[1024];
                                Stream outputStream = context.Response.OutputStream;
                                int count = 1;
                                while (count > 0) {
                                    count = resourceStream.Read(buffer, 0, 1024);
                                    outputStream.Write(buffer, 0, count);
                                }
                                outputStream.Flush();
                            }
                        }
                    }
                    finally {
                        if (reader != null)
                            reader.Close();
                        if (resourceStream != null)
                            resourceStream.Close();
                    }
                }
            }
            catch(Exception e) {
                exception = e;
                // MSRC 10405: ---- all errors in the event of failure. In particular, we don't want to
                // bubble the inner exceptions up in the YSOD, as they might contain sensitive cryptographic
                // information. Setting 'resourceStream' to null will cause an appropriate exception to
                // be thrown.
                resourceStream = null;
            }

            // Dev10 Bugs 602949: 404 if the assembly is not found or if the resource does not exist
            if (resourceStream == null) {
                if (resourceIdentifierPresent) {
                    LogWebResourceFailure(decryptedData, exception);
                }
                throw new HttpException(404, SR.GetString(SR.AssemblyResourceLoader_InvalidRequest));
            }

            context.Response.IgnoreFurtherWrites();
        }
    }
}
