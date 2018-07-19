//------------------------------------------------------------------------------
// <copyright file="ScriptResourceHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Handlers {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using System.Resources;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Policy;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Resources;
    using System.Web.Security.Cryptography;
    using System.Web.UI;
    using System.Web.Util;
    using Debug = System.Diagnostics.Debug;

    public class ScriptResourceHandler : IHttpHandler {

        private const string _scriptResourceUrl = "~/ScriptResource.axd";
        private static readonly IDictionary _assemblyInfoCache = Hashtable.Synchronized(new Hashtable());
        private static readonly IDictionary _cultureCache = Hashtable.Synchronized(new Hashtable());
        private static readonly Object _getMethodLock = new Object();
        private static IScriptResourceHandler _scriptResourceHandler = new RuntimeScriptResourceHandler();
        private static string _scriptResourceAbsolutePath;
        // _bypassVirtualPathResolution set by unit tests to avoid resolving ~/ paths from unit tests.
        private static bool _bypassVirtualPathResolution = false;
        private static int _maximumResourceUrlLength = 2048;

        private static string ScriptResourceAbsolutePath {
            get {
                if (_scriptResourceAbsolutePath == null) {
                    _scriptResourceAbsolutePath = VirtualPathUtility.ToAbsolute(_scriptResourceUrl);
                }
                return _scriptResourceAbsolutePath;
            }
        }

        private static Exception Create404(Exception innerException) {
            return new HttpException(404, AtlasWeb.ScriptResourceHandler_InvalidRequest, innerException);
        }

        internal static CultureInfo DetermineNearestAvailableCulture(
            Assembly assembly,
            string scriptResourceName,
            CultureInfo culture) {

            if (String.IsNullOrEmpty(scriptResourceName)) return CultureInfo.InvariantCulture;

            Tuple<Assembly, string, CultureInfo> cacheKey = Tuple.Create(assembly, scriptResourceName, culture);
            CultureInfo cachedCulture = (CultureInfo)_cultureCache[cacheKey];
            if (cachedCulture == null) {

                string releaseResourceName =
                    scriptResourceName.EndsWith(".debug.js", StringComparison.OrdinalIgnoreCase) ?
                    scriptResourceName.Substring(0, scriptResourceName.Length - 9) + ".js" :
                    null;

                ScriptResourceInfo resourceInfo = ScriptResourceInfo.GetInstance(assembly, scriptResourceName);
                ScriptResourceInfo releaseResourceInfo = (releaseResourceName != null) ?
                    ScriptResourceInfo.GetInstance(assembly, releaseResourceName) : null;

                if (!String.IsNullOrEmpty(resourceInfo.ScriptResourceName) ||
                    ((releaseResourceInfo != null) && !String.IsNullOrEmpty(releaseResourceInfo.ScriptResourceName))) {

                    ResourceManager resourceManager =
                        ScriptResourceAttribute.GetResourceManager(resourceInfo.ScriptResourceName, assembly);
                    ResourceManager releaseResourceManager = (releaseResourceInfo != null) ?
                        ScriptResourceAttribute.GetResourceManager(releaseResourceInfo.ScriptResourceName, assembly) : null;

                    ResourceSet localizedSet = null;
                    ResourceSet releaseSet = null;
                    if (resourceManager != null) {
                        resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
                        // Look for the explicitly localized version of the resources that is nearest the culture.
                        localizedSet = resourceManager.GetResourceSet(culture, true, false);
                    }
                    if (releaseResourceManager != null) {
                        releaseResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
                        // Look for the explicitly localized version of the resources that is nearest the culture.
                        releaseSet = releaseResourceManager.GetResourceSet(culture, true, false);
                    }
                    if ((resourceManager != null) || (releaseResourceManager != null)) {
                        while ((localizedSet == null) && (releaseSet == null)) {
                            culture = culture.Parent;
                            if (culture.Equals(CultureInfo.InvariantCulture)) break;
                            localizedSet = resourceManager.GetResourceSet(culture, true, false);
                            releaseSet = (releaseResourceManager != null) ?
                                releaseResourceManager.GetResourceSet(culture, true, false) : null;
                        }
                    }
                    else {
                        culture = CultureInfo.InvariantCulture;
                    }
                }
                else {
                    culture = CultureInfo.InvariantCulture;
                }
                // Neutral assembly culture falls back on invariant
                CultureInfo neutralCulture = GetAssemblyNeutralCulture(assembly);
                if ((neutralCulture != null) && neutralCulture.Equals(culture)) {
                    culture = CultureInfo.InvariantCulture;
                }
                cachedCulture = culture;
                _cultureCache[cacheKey] = cachedCulture;
            }
            return cachedCulture;
        }

        private static void EnsureScriptResourceRequest(string path) {
            if (!IsScriptResourceRequest(path)) {
                Throw404();
            }
        }

        private static Assembly GetAssembly(string assemblyName) {
            Debug.Assert(!String.IsNullOrEmpty(assemblyName));
            string[] parts = assemblyName.Split(',');

            if ((parts.Length != 1) && (parts.Length != 4)) {
                Throw404();
            }

            AssemblyName realName = new AssemblyName();
            realName.Name = parts[0];
            if (parts.Length == 4) {
                realName.Version = new Version(parts[1]);
                string cultureString = parts[2];
                realName.CultureInfo = (cultureString.Length > 0) ?
                    new CultureInfo(cultureString) :
                    CultureInfo.InvariantCulture;
                realName.SetPublicKeyToken(HexParser.Parse(parts[3]));
            }
            Assembly assembly = null;
            try {
                assembly = Assembly.Load(realName);
            }
            catch (FileNotFoundException fnf) {
                Throw404(fnf);
            }
            catch (FileLoadException fl) {
                Throw404(fl);
            }
            catch (BadImageFormatException badImage) {
                Throw404(badImage);
            }

            return assembly;
        }

        private static Tuple<AssemblyName, String> GetAssemblyInfo(Assembly assembly) {
            Tuple<AssemblyName, String> assemblyInfo =
                (Tuple<AssemblyName, String>)_assemblyInfoCache[assembly];
            if (assemblyInfo == null) {
                assemblyInfo = GetAssemblyInfoInternal(assembly);
                _assemblyInfoCache[assembly] = assemblyInfo;
            }
            Debug.Assert(assemblyInfo != null, "Assembly info should not be null");
            return assemblyInfo;
        }

        private static Tuple<AssemblyName, String> GetAssemblyInfoInternal(Assembly assembly) {
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            string hash = Convert.ToBase64String(assembly.ManifestModule.ModuleVersionId.ToByteArray());
            return new Tuple<AssemblyName, String>(assemblyName, hash);
        }

        private static CultureInfo GetAssemblyNeutralCulture(Assembly assembly) {
            CultureInfo neutralCulture = (CultureInfo)_cultureCache[assembly];
            if (neutralCulture == null) {
                object[] nrlas = assembly.GetCustomAttributes(typeof(NeutralResourcesLanguageAttribute), false);
                if ((nrlas != null) && (nrlas.Length != 0)) {
                    neutralCulture = CultureInfo.GetCultureInfo(
                        ((NeutralResourcesLanguageAttribute)nrlas[0]).CultureName);
                    _cultureCache[assembly] = neutralCulture;
                }
            }
            return neutralCulture;
        }

        internal static string GetEmptyPageUrl(string title) {
            return GetScriptResourceHandler().GetEmptyPageUrl(title);
        }

        private static IScriptResourceHandler GetScriptResourceHandler() {
            if (_scriptResourceHandler == null) {
                _scriptResourceHandler = new RuntimeScriptResourceHandler();
            }
            return _scriptResourceHandler;
        }

        internal static string GetScriptResourceUrl(
            Assembly assembly,
            string resourceName,
            CultureInfo culture,
            bool zip) {

            return GetScriptResourceHandler()
                .GetScriptResourceUrl(assembly, resourceName, culture, zip);
        }

        internal static string GetScriptResourceUrl(
            List<Tuple<Assembly, List<Tuple<string, CultureInfo>>>> assemblyResourceLists,
            bool zip) {

            return GetScriptResourceHandler().GetScriptResourceUrl(assemblyResourceLists, zip);
        }

        protected virtual bool IsReusable {
            get {
                return true;
            }
        }

        internal delegate string VirtualFileReader(string virtualPath, out Encoding encoding);

        private static bool IsCompressionEnabled(HttpContext context) {
            return ScriptingScriptResourceHandlerSection.ApplicationSettings.EnableCompression &&
                ((context == null) ||
                !context.Request.Browser.IsBrowser("IE") ||
                (context.Request.Browser.MajorVersion > 6));
        }

        internal static bool IsScriptResourceRequest(string path) {
            return !String.IsNullOrEmpty(path) &&
                String.Equals(path, ScriptResourceAbsolutePath, StringComparison.OrdinalIgnoreCase);
        }

        private static void OutputEmptyPage(HttpResponseBase response, string title) {
            PrepareResponseCache(response);
            response.Write(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml""><head><script type=""text/javascript"">parent.Sys.Application._onIFrameLoad();</script><title>" +
                           HttpUtility.HtmlEncode(title) +
                           @"</title></head><body></body></html>");
        }

        private static void PrepareResponseCache(HttpResponseBase response) {
            HttpCachePolicyBase cachePolicy = response.Cache;
            DateTime now = DateTime.Now;
            cachePolicy.SetCacheability(HttpCacheability.Public);
            cachePolicy.VaryByParams["d"] = true;
            cachePolicy.SetOmitVaryStar(true);
            cachePolicy.SetExpires(now + TimeSpan.FromDays(365));
            cachePolicy.SetValidUntilExpires(true);
            cachePolicy.SetLastModified(now);
        }

        private static void PrepareResponseNoCache(HttpResponseBase response) {
            HttpCachePolicyBase cachePolicy = response.Cache;
            DateTime now = DateTime.Now;
            cachePolicy.SetCacheability(HttpCacheability.Public);
            cachePolicy.SetExpires(now + TimeSpan.FromDays(365));
            cachePolicy.SetValidUntilExpires(true);
            cachePolicy.SetLastModified(now);
            cachePolicy.SetNoServerCaching();
        }

        [SecuritySafeCritical]
        protected virtual void ProcessRequest(HttpContext context) {
            ProcessRequest(new HttpContextWrapper(context));
        }

        internal static void ProcessRequest(HttpContextBase context, VirtualFileReader fileReader = null, Action<string, Exception> logAction = null, bool validatePath = true) {
            string decryptedString = null;
            bool resourceIdentifierPresent = false;
            try {
                HttpResponseBase response = context.Response;
                response.Clear();

                if (validatePath) {
                    // Checking that the handler is not being called from a different path.
                    EnsureScriptResourceRequest(context.Request.Path);
                }

                string encryptedData = context.Request.QueryString["d"];
                if (String.IsNullOrEmpty(encryptedData)) {
                    Throw404();
                }

                resourceIdentifierPresent = true;
                try {
                    decryptedString = Page.DecryptString(encryptedData, Purpose.ScriptResourceHandler_ScriptResourceUrl);
                }
                catch (CryptographicException ex) {
                    Throw404(ex);
                }

                fileReader = fileReader ?? new VirtualFileReader(delegate(string virtualPath, out Encoding encoding) {
                    VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;
                    if (!vpp.FileExists(virtualPath)) {
                        Throw404();
                    }
                    VirtualFile file = vpp.GetFile(virtualPath);
                    if (!AppSettings.ScriptResourceAllowNonJsFiles && !file.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) {
                        // MSRC 10405: Disallow all extensions other than *.js
                        Throw404();
                    }
                    using (Stream stream = file.Open()) {
                        using (StreamReader reader = new StreamReader(stream, true)) {
                            encoding = reader.CurrentEncoding;
                            return reader.ReadToEnd();
                        }
                    }
                });
                ProcessRequestInternal(response, decryptedString, fileReader);
            } catch(Exception e) {
                if (resourceIdentifierPresent) {
                    logAction = logAction ?? AssemblyResourceLoader.LogWebResourceFailure;
                    logAction(decryptedString, e);
                }
                // MSRC 10405: There's no reason for this to return anything other than a 404 if something
                // goes wrong. We shouldn't propagate the inner exception inside the YSOD, as it might
                // contain sensitive cryptographic information.
                Throw404();
            }
        }

        private static void ProcessRequestInternal(
            HttpResponseBase response,
            string decryptedString,
            VirtualFileReader fileReader) {

            if (String.IsNullOrEmpty(decryptedString)) {
                Throw404();
            }
            bool zip;
            bool singleAssemblyReference;
            // See GetScriptResourceUrl comment below for first character meanings.
            switch (decryptedString[0]) {
                case 'Z':
                case 'z':
                    singleAssemblyReference = true;
                    zip = true;
                    break;
                case 'U':
                case 'u':
                    singleAssemblyReference = true;
                    zip = false;
                    break;
                case 'Q':
                case 'q':
                    singleAssemblyReference = false;
                    zip = true;
                    break;
                case 'R':
                case 'r':
                    singleAssemblyReference = false;
                    zip = false;
                    break;
                case 'T':
                    OutputEmptyPage(response, decryptedString.Substring(1));
                    return;
                default:
                    Throw404();
                    return;
            }

            decryptedString = decryptedString.Substring(1);
            if (String.IsNullOrEmpty(decryptedString)) {
                Throw404();
            }
            string[] decryptedData = decryptedString.Split('|');

            if (singleAssemblyReference) {
                // expected: <assembly>|<resource>|<culture>[|#|<hash>]
                if (decryptedData.Length != 3 && decryptedData.Length != 5) {
                    // The decrypted data must have 3 parts plus an optional 2 part hash code separated by pipes.
                    Throw404();
                }
            }
            else {
                // expected: <assembly1>|<resource1a>,<culture1a>,<resource1b>,<culture1b>,...|<assembly2>|<resource2a>,<culture2a>,<resource2b>,<culture2b>,...|#|<hash>
                if (decryptedData.Length % 2 != 0) {
                    // The decrypted data must have an even number of parts separated by pipes.
                    Throw404();
                }
            }

            StringBuilder script = new StringBuilder();

            string firstContentType = null;

            if (singleAssemblyReference) {
                // single assembly reference, format is
                // <assembly>|<resource>|<culture>
                string assemblyName = decryptedData[0];
                string resourceName = decryptedData[1];
                string cultureName = decryptedData[2];

                Assembly assembly = GetAssembly(assemblyName);
                if (assembly == null) {
                    Throw404();
                }

                script.Append(ScriptResourceAttribute.GetScriptFromWebResourceInternal(
                    assembly,
                    resourceName,
                    String.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : new CultureInfo(cultureName),
                    zip,
                    out firstContentType
                ));
            }
            else {
                // composite script reference, format is:
                // <assembly1>|<resource1a>,<culture1a>,<resource1b>,<culture1b>,...|<assembly2>|<resource2a>,<culture2a>,<resource2b>,<culture2b>,...
                // Assembly is empty for path based scripts, and their resource/culture list is <path1>,<path2>,...
                
                // If an assembly starts with "#", the segment is ignored (expected that this includes a hash to ensure
                // url uniqueness when resources are changed). Also, for forward compatibility '#' segments may contain
                // other data. 

                bool needsNewline = false;

                for (int i = 0; i < decryptedData.Length; i += 2) {
                    string assemblyName = decryptedData[i];
                    bool hasAssembly = !String.IsNullOrEmpty(assemblyName);
                    if (hasAssembly && assemblyName[0] == '#') {
                        // hash segments are ignored, it contains a hash code for url uniqueness
                        continue;
                    }
                    Debug.Assert(!String.IsNullOrEmpty(decryptedData[i + 1]));
                    string[] resourcesAndCultures = decryptedData[i + 1].Split(',');

                    if (resourcesAndCultures.Length == 0) {
                        Throw404();
                    }

                    Assembly assembly = hasAssembly ? GetAssembly(assemblyName) : null;

                    if (assembly == null) {
                        // The scripts are path-based
                        if (firstContentType == null) {
                            firstContentType = "text/javascript";
                        }
                        for (int j = 0; j < resourcesAndCultures.Length; j++) {
                            Encoding encoding;
                            // DevDiv Bugs 197242
                            // path will either be absolute, as in "/app/foo/bar.js" or app relative, as in "~/foo/bar.js"
                            // ToAbsolute() ensures it is in the form /app/foo/bar.js
                            // This conversion was not done when the url was created to conserve url length.
                            string path = _bypassVirtualPathResolution ?
                                resourcesAndCultures[j] :
                                VirtualPathUtility.ToAbsolute(resourcesAndCultures[j]);
                            string fileContents = fileReader(path, out encoding);

                            if (needsNewline) {
                                // Output an additional newline between resources but not for the last one
                                script.Append('\n');
                            }
                            needsNewline = true;

                            script.Append(fileContents);
                        }
                    }
                    else {
                        Debug.Assert(resourcesAndCultures.Length % 2 == 0, "The list of resource names and cultures must have an even number of parts separated by commas.");

                        for (int j = 0; j < resourcesAndCultures.Length; j += 2) {
                            try {
                                string contentType;
                                string resourceName = resourcesAndCultures[j];
                                string cultureName = resourcesAndCultures[j + 1];

                                if (needsNewline) {
                                    // Output an additional newline between resources but not for the last one
                                    script.Append('\n');
                                }
                                needsNewline = true;

                                script.Append(ScriptResourceAttribute.GetScriptFromWebResourceInternal(
                                    assembly,
                                    resourceName,
                                    String.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : new CultureInfo(cultureName),
                                    zip,
                                    out contentType
                                ));

                                if (firstContentType == null) {
                                    firstContentType = contentType;
                                }
                            }
                            catch (MissingManifestResourceException ex) {
                                throw Create404(ex);
                            }
                            catch (HttpException ex) {
                                throw Create404(ex);
                            }
                        }
                    }
                }
            }

            if (ScriptingScriptResourceHandlerSection.ApplicationSettings.EnableCaching) {
                PrepareResponseCache(response);
            }
            else {
                PrepareResponseNoCache(response);
            }

            response.ContentType = firstContentType;

            if (zip) {
                using (MemoryStream zipped = new MemoryStream()) {
                    using (Stream outputStream = new GZipStream(zipped, CompressionMode.Compress)) {
                        // The choice of an encoding matters little here.
                        // Input streams being of potentially different encodings, UTF-8 is the better
                        // choice as it's the natural encoding for JavaScript.
                        using (StreamWriter writer = new StreamWriter(outputStream, Encoding.UTF8)) {
                            writer.Write(script.ToString());
                        }
                    }
                    byte[] zippedBytes = zipped.ToArray();
                    response.AddHeader("Content-encoding", "gzip");
                    response.OutputStream.Write(zippedBytes, 0, zippedBytes.Length);
                }
            }
            else {
                // Bug DevDiv #175061, we don't want to force any encoding here and let the default
                // encoding apply no matter what the incoming scripts might have been encoded with.
                response.Write(script.ToString());
            }
        }

        internal static void SetScriptResourceHandler(IScriptResourceHandler scriptResourceHandler) {
            _scriptResourceHandler = scriptResourceHandler;
        }

        private static void Throw404() {
            throw Create404(null);
        }

        private static void Throw404(Exception innerException) {
            throw Create404(innerException);
        }

        #region IHttpHandler implementation
        void IHttpHandler.ProcessRequest(HttpContext context) {
            ProcessRequest(context);
        }

        bool IHttpHandler.IsReusable {
            get {
                return IsReusable;
            }
        }
        #endregion

        private class RuntimeScriptResourceHandler : IScriptResourceHandler {

            // Keys in the URL cache will be IList objects, so use our custom list comparer.
            private static readonly IDictionary _urlCache = Hashtable.Synchronized(new Hashtable(ListEqualityComparer.Instance));
            private static readonly IDictionary _cultureCache = Hashtable.Synchronized(new Hashtable());
            private static string _absoluteScriptResourceUrl;

            string IScriptResourceHandler.GetScriptResourceUrl(
                Assembly assembly, string resourceName, CultureInfo culture, bool zip) {

                return ((IScriptResourceHandler)this).GetScriptResourceUrl(
                    new List<Tuple<Assembly, List<Tuple<string, CultureInfo>>>>() {
                        new Tuple<Assembly, List<Tuple<string, CultureInfo>>>(
                            assembly,
                            new List<Tuple<string,CultureInfo>>() {
                                new Tuple<string, CultureInfo>(resourceName, culture)
                            }
                        )
                    }, zip);
            }

            string IScriptResourceHandler.GetScriptResourceUrl(
                List<Tuple<Assembly, List<Tuple<string, CultureInfo>>>> assemblyResourceLists,
                bool zip) {

                if (!IsCompressionEnabled(HttpContext.Current)) {
                    zip = false;
                }

                bool allAssemblyResources = true;
                foreach (Tuple<Assembly, List<Tuple<string, CultureInfo>>> assemblyData in assemblyResourceLists) {
                    if (assemblyData.Item1 == null) {
                        allAssemblyResources = false;
                        break;
                    }
                }
                
                // If all the scripts are assembly resources, we can cache the generated ScriptResource URL, since
                // the appdomain will reset if any of the assemblies are changed.  We cannot cache the URL if any
                // scripts are path-based, since the cache entry will not be removed if a path-based script is changed.
                if (allAssemblyResources) {
                    List<object> cacheKeys = new List<object>();
                    
                    foreach (Tuple<Assembly, List<Tuple<string, CultureInfo>>> assemblyData in assemblyResourceLists) {
                        cacheKeys.Add(assemblyData.Item1);
                        foreach (Tuple<string, CultureInfo> resourceAndCulture in assemblyData.Item2) {
                            cacheKeys.Add(resourceAndCulture.Item1);
                            cacheKeys.Add(resourceAndCulture.Item2);
                        }
                    }

                    cacheKeys.Add(zip);

                    string url = (string)_urlCache[cacheKeys];

                    if (url == null) {
                        url = GetScriptResourceUrlImpl(assemblyResourceLists, zip);
                        _urlCache[cacheKeys] = url;
                    }

                    return url;
                }
                else {
                    return GetScriptResourceUrlImpl(assemblyResourceLists, zip);
                }
            }

            [SecuritySafeCritical]
            private static string GetScriptResourceUrlImpl(
                List<Tuple<Assembly, List<Tuple<string, CultureInfo>>>> assemblyResourceLists,
                bool zip) {

                EnsureAbsoluteScriptResourceUrl();

                // If there's only a single assembly resource, format is
                //      [Z|U|z|u]<assembly>|<resource>|<culture>
                // If there are multiple resources, or a single resource that is path based, format is
                //      [Q|R|q|r]<assembly1>|<resource1a>,<culture1a>,<resource1b>,<culture1b>...|<assembly2>|<resource2a>,<culture2a>,<resource2b>,<culture2b>...
                // A path based reference has no assembly (empty).
                // (the Q/R indicators used in place of Z/U give the handler indiciation that the url is a composite
                // reference, and allows for System.Web.Extensions SP1 to maintain compatibility with RTM, should a
                // single resource be encrypted with SP1 and decrypted with RTM).

                bool singleAssemblyResource = false;
                if (assemblyResourceLists.Count == 1) {
                    // only one assembly to pull from...
                    var reference = assemblyResourceLists[0];
                    if ((reference.Item1 != null) && (reference.Item2.Count == 1)) {
                        // resource is assembly not path, and there's only one resource within it to load
                        singleAssemblyResource = true;
                    }
                }

                // Next character of the encoded string is:
                // Format: S = Single Assembly Reference, C = Composite Reference or Single Path Reference
                // Zip: compress or not (true or false)
                // First    Format  Zip?
                // =====================
                // Z        S       T   
                // U        S       F   
                // Q        C       T   
                // R        C       F   

                string indicator;
                if (singleAssemblyResource) {
                    indicator = (zip ? "Z" : "U");
                }
                else {
                    indicator = (zip ? "Q" : "R");
                }

                StringBuilder url = new StringBuilder(indicator);

                HashCodeCombiner hashCombiner = new HashCodeCombiner();

                bool firstAssembly = true;
                foreach (Tuple<Assembly, List<Tuple<string, CultureInfo>>> assemblyData in assemblyResourceLists) {

                    if (!firstAssembly) {
                        url.Append('|');
                    }
                    else {
                        firstAssembly = false;
                    }
                    if (assemblyData.Item1 != null) {
                        Tuple<AssemblyName, String> assemblyInfo = GetAssemblyInfo(assemblyData.Item1);

                        AssemblyName assemblyName = (AssemblyName)assemblyInfo.Item1;
                        string assemblyHash = (String)assemblyInfo.Item2;
                        hashCombiner.AddObject(assemblyHash);

                        if (assemblyData.Item1.GlobalAssemblyCache) {
                            // If the assembly is in the GAC, we need to store a full name to load the assembly later
                            // Pack the necessary values into a more compact format than FullName
                            url.Append(assemblyName.Name);
                            url.Append(',');
                            url.Append(assemblyName.Version);
                            url.Append(',');
                            if (assemblyName.CultureInfo != null) {
                                url.Append(assemblyName.CultureInfo);
                            }
                            url.Append(',');
                            url.Append(HexParser.ToString(assemblyName.GetPublicKeyToken()));
                        }
                        else {
                            // Otherwise, we can just use a partial name
                            url.Append(assemblyName.Name);
                        }
                    }
                    url.Append('|');

                    bool firstResource = true;
                    foreach (Tuple<string, CultureInfo> resourceAndCulture in assemblyData.Item2) {

                        if (!firstResource) {
                            url.Append(',');
                        }

                        if (assemblyData.Item1 != null) {
                            url.Append(resourceAndCulture.Item1);
                            Tuple<Assembly, string, CultureInfo> cacheKey = Tuple.Create(
                                assemblyData.Item1,
                                resourceAndCulture.Item1,
                                resourceAndCulture.Item2
                            );
                            string cultureName = (string)_cultureCache[cacheKey];
                            if (cultureName == null) {
                                // Check if the resources exist
                                ScriptResourceInfo resourceInfo =
                                    ScriptResourceInfo.GetInstance(assemblyData.Item1, resourceAndCulture.Item1);
                                if (resourceInfo == ScriptResourceInfo.Empty) {
                                    ThrowUnknownResource(resourceAndCulture.Item1);
                                }
                                Stream scriptStream = assemblyData.Item1.GetManifestResourceStream(resourceInfo.ScriptName);
                                if (scriptStream == null) {
                                    ThrowUnknownResource(resourceAndCulture.Item1);
                                }
                                cultureName = DetermineNearestAvailableCulture(
                                    assemblyData.Item1, resourceAndCulture.Item1, resourceAndCulture.Item2).Name;
                                _cultureCache[cacheKey] = cultureName;
                            }
                            url.Append(singleAssemblyResource ? "|" : ",");
                            url.Append(cultureName);
                        }
                        else {
                            Debug.Assert(!singleAssemblyResource, "This should never happen since this is a path reference.");

                            if (!_bypassVirtualPathResolution) {
                                VirtualPathProvider vpp = HostingEnvironment.VirtualPathProvider;
                                if (!vpp.FileExists(resourceAndCulture.Item1)) {
                                    ThrowUnknownResource(resourceAndCulture.Item1);
                                }
                                string hash = vpp.GetFileHash(resourceAndCulture.Item1, new string[] { resourceAndCulture.Item1 });
                                hashCombiner.AddObject(hash);
                            }
                            url.Append(resourceAndCulture.Item1);
                        }
                        firstResource = false;
                    }
                }

                // DevDiv Bugs 186624: The hash code needs to be part of the encrypted blob for composite scripts
                // because we cache the composite script on the server using a VaryByParam["d"]. Otherwise, if a
                // path based script in the composite changes, only the 't' parameter would change, which would
                // cause a new request to the server, but it would be served via cache since 'd' would be the same.
                // This isn't a problem for assembly based resources since changing them also restarts the app and
                // clears the cache. We do not vary by 't' because that makes it possible to flood the server cache
                // with cache entries, since anything could be used for 't'. Putting the hash in 'd' ensures a different
                // url and different cache entry when a script changes, but without the possibility of flooding
                // the server cache.

                // However, we continue to use the 't' parameter for single assembly references for compatibility.

                string resourceUrl;
                if (singleAssemblyResource) {
                    resourceUrl = _absoluteScriptResourceUrl +
                            Page.EncryptString(url.ToString(), Purpose.ScriptResourceHandler_ScriptResourceUrl) +
                            "&t=" + hashCombiner.CombinedHashString;
                }
                else {
                    // note that CombinedHashString is hex, it will never include a '|' that would confuse the handler.
                    url.Append("|#|");
                    url.Append(hashCombiner.CombinedHashString);
                    resourceUrl = _absoluteScriptResourceUrl +
                            Page.EncryptString(url.ToString(), Purpose.ScriptResourceHandler_ScriptResourceUrl);
                }

                if (resourceUrl.Length > _maximumResourceUrlLength) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ScriptResourceHandler_ResourceUrlTooLong, _maximumResourceUrlLength));
                }
                return resourceUrl;
            }

            private static void EnsureAbsoluteScriptResourceUrl() {
                if (_absoluteScriptResourceUrl == null) {
                    _absoluteScriptResourceUrl = _bypassVirtualPathResolution ?
                        _scriptResourceUrl + "?d=" :
                        VirtualPathUtility.ToAbsolute(_scriptResourceUrl) + "?d=";
                }
            }

            string IScriptResourceHandler.GetEmptyPageUrl(string title) {
                EnsureAbsoluteScriptResourceUrl();
                return _absoluteScriptResourceUrl +
                        Page.EncryptString('T' + title, Purpose.ScriptResourceHandler_ScriptResourceUrl);
            }

            private static void ThrowUnknownResource(string resourceName) {
                throw new HttpException(String.Format(CultureInfo.CurrentCulture,
                    AtlasWeb.ScriptResourceHandler_UnknownResource, resourceName));
            }

        }
    }
}
