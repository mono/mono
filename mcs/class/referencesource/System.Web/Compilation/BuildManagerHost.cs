//------------------------------------------------------------------------------
// <copyright file="BuildManagerHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/************************************************************************************************************/


namespace System.Web.Compilation {

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.Util;
using Debug = System.Web.Util.Debug;


//
// Instances of this class are created in the ASP.NET app domain.  The class
// methods are called by ClientBuildManager (cross app domain)
//
internal class BuildManagerHost : MarshalByRefObject, IRegisteredObject {

    private ClientBuildManager _client;
    private BuildManager _buildManager;

    private int _pendingCallsCount;

    private EventHandler _onAppDomainUnload;

    private bool _ignorePendingCalls;
    private IDictionary _assemblyCollection;

    private object _lock = new object();

    private static bool _inClientBuildManager;

    internal static bool InClientBuildManager { 
        get { return _inClientBuildManager; }
        set { _inClientBuildManager = true; }
    }

    /// <summary>
    /// Returns whether there is support for multitargeting. This is usually only true in VS as they will supply a 
    /// TypeDescriptionProvider that is required for correctly reflecting over types in the target framework.
    /// When running in the 4.0 runtime application pool, or when running from aspnet_compiler, this value
    /// will be false.
    /// </summary>
    internal static bool SupportsMultiTargeting {
        get; set;
    }

    private ClientVirtualPathProvider _virtualPathProvider;

    public BuildManagerHost() {
        HostingEnvironment.RegisterObject(this);
        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.ResolveAssembly);
    }

    void IRegisteredObject.Stop(bool immediate) {

        // Make sure all the pending calls complete
        WaitForPendingCallsToFinish();

        HostingEnvironment.UnregisterObject(this);

        if (_client != null) {
            _client.ResetHost();
        }
    }

    internal IApplicationHost ApplicationHost {
        get {
            return HostingEnvironment.ApplicationHostInternal;
        }
    }

    internal string CodeGenDir {
        get {
            // Add a pending call to make sure our thread doesn't get killed
            AddPendingCall();

            try {
                return HttpRuntime.CodegenDirInternal;
            }
            finally {
                RemovePendingCall();
            }
        }
    }

    internal void RegisterAssembly(String assemblyName, String assemblyLocation) {
        Debug.Trace("BuildManagerHost", "RegisterAssembly '" + assemblyName + "','" + assemblyLocation + "'");

        if (_assemblyCollection == null) {
            lock (_lock) {
                if (_assemblyCollection == null) {
                    _assemblyCollection = Hashtable.Synchronized(new Hashtable());
                }
            }
        }

        AssemblyName asmName = new AssemblyName(assemblyName);
        _assemblyCollection[asmName.FullName] = assemblyLocation;
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    private Assembly ResolveAssembly(object sender, ResolveEventArgs e) {
        Debug.Trace("BuildManagerHost", "ResolveAssembly '" + e.Name + "'");
        if (_assemblyCollection == null)
            return null;

        String assemblyLocation = (String)_assemblyCollection[e.Name];
        if (assemblyLocation == null)
            return null;

        Debug.Trace("BuildManagerHost", "ResolveAssembly: found");

        return Assembly.LoadFrom(assemblyLocation);
    }

    /*
     * Make sure all the (non-request based) pending calls complete
     */
    private void WaitForPendingCallsToFinish() {
        for (;;) {
            if (_pendingCallsCount <= 0 || _ignorePendingCalls)
                break;

            Thread.Sleep(250);
        }
    }

    internal void AddPendingCall() {
        Interlocked.Increment(ref _pendingCallsCount);
    }

    internal void RemovePendingCall() {
        Interlocked.Decrement(ref _pendingCallsCount);
    }

    private void OnAppDomainShutdown(object o, BuildManagerHostUnloadEventArgs args) {
        _client.OnAppDomainShutdown(args.Reason);
    }

    internal void CompileApplicationDependencies() {
        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            _buildManager.EnsureTopLevelFilesCompiled();
        }
        finally {
            RemovePendingCall();
        }
    }

    internal void PrecompileApp(ClientBuildManagerCallback callback, List<string> excludedVirtualPaths) {
        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            _buildManager.PrecompileApp(callback, excludedVirtualPaths);
        }
        finally {
            RemovePendingCall();
        }
    }

    internal IDictionary GetBrowserDefinitions() {
        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            return BrowserCapabilitiesCompiler.BrowserCapabilitiesFactory.InternalGetBrowserElements();
        }
        finally {
            RemovePendingCall();
        }
    }

    internal string[] GetVirtualCodeDirectories() {
        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            return _buildManager.GetCodeDirectories();
        }
        finally {
            RemovePendingCall();
        }
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal void GetCodeDirectoryInformation(VirtualPath virtualCodeDir,
        out Type codeDomProviderType, out CompilerParameters compParams,
        out string generatedFilesDir) {

        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            BuildManager.SkipTopLevelCompilationExceptions = true;
            _buildManager.EnsureTopLevelFilesCompiled();

            // Treat it as relative to the app root
            virtualCodeDir = virtualCodeDir.CombineWithAppRoot();

            _buildManager.GetCodeDirectoryInformation(virtualCodeDir,
                out codeDomProviderType, out compParams, out generatedFilesDir);
        }
        finally {
            BuildManager.SkipTopLevelCompilationExceptions = false;
            RemovePendingCall();
        }
    }

    internal void GetCompilerParams(VirtualPath virtualPath, out Type codeDomProviderType,
        out CompilerParameters compParams) {

        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            BuildManager.SkipTopLevelCompilationExceptions = true;
            _buildManager.EnsureTopLevelFilesCompiled();

            // Ignore the BuildProvider return value
            GetCompilerParamsAndBuildProvider(virtualPath, out codeDomProviderType, out compParams);

            // This is the no-compile case
            if (compParams == null)
                return;

            FixupReferencedAssemblies(virtualPath, compParams);
        }
        finally {
            BuildManager.SkipTopLevelCompilationExceptions = false;
            RemovePendingCall();
        }
    }

    internal string[] GetCompiledTypeAndAssemblyName(VirtualPath virtualPath, ClientBuildManagerCallback callback) {

        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            // Treat it as relative to the app root
            virtualPath.CombineWithAppRoot();

            Type t = BuildManager.GetCompiledType(virtualPath, callback);

            if (t == null) return null;

            string assemblyPath = Util.GetAssemblyPathFromType(t);
            return new string[] { t.FullName, assemblyPath };
        }
        finally {
            RemovePendingCall();
        }
    }

    internal string GetGeneratedSourceFile(VirtualPath virtualPath) {
        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        Type codeDomProviderType;
        CompilerParameters compilerParameters;
        string generatedFilesDir;

        try {
            if (!virtualPath.DirectoryExists()) {
                throw new ArgumentException(SR.GetString(SR.GetGeneratedSourceFile_Directory_Only, 
                    virtualPath.VirtualPathString), "virtualPath");
            }

            // Calls GetCodeDirectoryInformation to ensure the source files are created for the
            // directory specified by virtualPath
            GetCodeDirectoryInformation(virtualPath,
                out codeDomProviderType, out compilerParameters,
                out generatedFilesDir);

            return BuildManager.GenerateFileTable[virtualPath.VirtualPathStringNoTrailingSlash];
        }
        finally {
            RemovePendingCall();
        }
    }

    internal string GetGeneratedFileVirtualPath(string filePath) {
        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            // Performs reverse hashtable lookup to find the filePath in the Value collection.
            Dictionary<String, String>.Enumerator e = BuildManager.GenerateFileTable.GetEnumerator();
            while (e.MoveNext()) {
                KeyValuePair<String, String> pair = e.Current;
                if (filePath.Equals(pair.Value, StringComparison.Ordinal)) {
                    return pair.Key;
                }
            }

            return null;
        }
        finally {
            RemovePendingCall();
        }
    }

    /*
     * Returns an array of the assemblies defined in the bin and assembly reference config section
     */
    internal String[] GetTopLevelAssemblyReferences(VirtualPath virtualPath) {
        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        List<Assembly> assemblyList = new List<Assembly>();
        try {
            // Treat it as relative to the app root
            virtualPath.CombineWithAppRoot();

            CompilationSection compConfig = MTConfigUtil.GetCompilationConfig(virtualPath);

            // Add all the config assemblies to the list
            foreach (AssemblyInfo assemblyInfo in compConfig.Assemblies) {
                Assembly[] assemblies = assemblyInfo.AssemblyInternal;
                for (int i = 0; i < assemblies.Length; i++) {
                    if (assemblies[i] != null) {
                        assemblyList.Add(assemblies[i]);
                    }
                }
            }
        } finally {
            RemovePendingCall();
        }
        StringCollection paths = new StringCollection();
        Util.AddAssembliesToStringCollection(assemblyList, paths);
        string[] references = new string[paths.Count];
        paths.CopyTo(references, 0);
        return references;
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal string GenerateCode(
        VirtualPath virtualPath, string virtualFileString, out IDictionary linePragmasTable) {

        AddPendingCall();
        try {
            string code = null;
            Type codeDomProviderType;
            CompilerParameters compilerParameters;

            CodeCompileUnit ccu = GenerateCodeCompileUnit(virtualPath, virtualFileString, out codeDomProviderType,
                out compilerParameters, out linePragmasTable);

            if (ccu != null && codeDomProviderType != null) {
                CodeDomProvider codeProvider = CompilationUtil.CreateCodeDomProvider(codeDomProviderType);                    

                CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
                codeGeneratorOptions.BlankLinesBetweenMembers = false;
                codeGeneratorOptions.IndentString = string.Empty;

                StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
                codeProvider.GenerateCodeFromCompileUnit(ccu, sw, codeGeneratorOptions);

                code = sw.ToString();
            }

            return code;
        }
        finally {
            RemovePendingCall();
        }
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal CodeCompileUnit GenerateCodeCompileUnit(
        VirtualPath virtualPath, string virtualFileString, out Type codeDomProviderType,
        out CompilerParameters compilerParameters, out IDictionary linePragmasTable) {

        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            BuildManager.SkipTopLevelCompilationExceptions = true;
            _buildManager.EnsureTopLevelFilesCompiled();

            // Get the virtual file content so that we can use the correct hash code.
            if (virtualFileString == null) {
                using (Stream stream = virtualPath.OpenFile()) {
                    TextReader reader = Util.ReaderFromStream(stream, virtualPath);
                    virtualFileString = reader.ReadToEnd();
                }
            }

            _virtualPathProvider.RegisterVirtualFile(virtualPath, virtualFileString);

            string cacheKey = BuildManager.GetCacheKeyFromVirtualPath(virtualPath) + "_CBMResult";
            BuildResultCodeCompileUnit result = (BuildResultCodeCompileUnit)BuildManager.GetBuildResultFromCache(cacheKey, virtualPath);

            if (result == null) {
                lock (_lock) {
                    // Don't need to check the result again since it's very unlikely in CBM scenarios.
                    DateTime utcStart = DateTime.UtcNow;

                    BuildProvider internalBuildProvider = GetCompilerParamsAndBuildProvider(
                        virtualPath, out codeDomProviderType, out compilerParameters);

                    // This is the no-compile case
                    if (internalBuildProvider == null) {
                        linePragmasTable = null;
                        return null;
                    }

                    CodeCompileUnit ccu = internalBuildProvider.GetCodeCompileUnit(out linePragmasTable);

                    result = new BuildResultCodeCompileUnit(codeDomProviderType, ccu, compilerParameters, linePragmasTable);
                    result.VirtualPath = virtualPath;
                    result.SetCacheKey(cacheKey);

                    FixupReferencedAssemblies(virtualPath, compilerParameters);

                    // CodeCompileUnit could be null, do not try to fix referenced assemblies.
                    // This happens for example when an .asmx file does not contain any code.
                    if (ccu != null) {
                        // VSWhidbey 501260 Add all the referenced assemblies to the CodeCompileUnit
                        // in case the CodeDom provider needs them for code generation
                        foreach (String assemblyString in compilerParameters.ReferencedAssemblies) {
                            ccu.ReferencedAssemblies.Add(assemblyString);
                        }
                    }

                    // Add all the dependencies, so that the ccu gets cached correctly (VSWhidbey 275091)
                    ICollection dependencies = internalBuildProvider.VirtualPathDependencies;
                    if (dependencies != null)
                        result.AddVirtualPathDependencies(dependencies);

                    BuildManager.CacheBuildResult(cacheKey, result, utcStart);

                    return ccu;
                }
            }

            codeDomProviderType = result.CodeDomProviderType;
            compilerParameters = result.CompilerParameters;
            linePragmasTable = result.LinePragmasTable;

            FixupReferencedAssemblies(virtualPath, compilerParameters);

            return result.CodeCompileUnit;
        } 
        finally {
            if (virtualFileString != null) {
                _virtualPathProvider.RevertVirtualFile(virtualPath);
            }
            BuildManager.SkipTopLevelCompilationExceptions = false;

            RemovePendingCall();
        }
    }

    internal bool IsCodeAssembly(string assemblyName) {
        return BuildManager.GetNormalizedCodeAssemblyName(assemblyName) != null;
    }

    // Add the referenced assemblies into the compileParameters. Notice that buildProviders do not have
    // the correct referenced assemblies and we don't cache them since the assemblies could change
    // between appdomains. (removing assemblies from bin, etc)
    private void FixupReferencedAssemblies(VirtualPath virtualPath, CompilerParameters compilerParameters) {
        CompilationSection compConfig = MTConfigUtil.GetCompilationConfig(virtualPath);

        ICollection referencedAssemblies = BuildManager.GetReferencedAssemblies(compConfig);
        Util.AddAssembliesToStringCollection(referencedAssemblies, compilerParameters.ReferencedAssemblies);
    }

    private BuildProvider GetCompilerParamsAndBuildProvider(VirtualPath virtualPath,
        out Type codeDomProviderType, out CompilerParameters compilerParameters) {

        virtualPath.CombineWithAppRoot();

        CompilationSection compConfig = MTConfigUtil.GetCompilationConfig(virtualPath);

        ICollection referencedAssemblies = BuildManager.GetReferencedAssemblies(compConfig);

        // Create the buildprovider for the passed in virtualPath
        BuildProvider buildProvider = null;

        // Special case global asax build provider here since we do not want to compile every files with ".asax" extension.
        if (StringUtil.EqualsIgnoreCase(virtualPath.VirtualPathString, BuildManager.GlobalAsaxVirtualPath.VirtualPathString)) {
            ApplicationBuildProvider provider = new ApplicationBuildProvider();
            provider.SetVirtualPath(virtualPath);
            provider.SetReferencedAssemblies(referencedAssemblies);
            buildProvider = provider;
        }
        else {
            buildProvider = BuildManager.CreateBuildProvider(virtualPath, compConfig,
            referencedAssemblies, true /*failIfUnknown*/);
        }

        // DevDiv 69017
        // The methods restricted to internalBuildProvider have been moved up to BuildProvider
        // to allow WCFBuildProvider to support .svc syntax highlighting.
        
        // Ignore parse errors, since they should not break the designer
        buildProvider.IgnoreParseErrors = true;

        // Ignore all control properties, since we do not generate code for the properties
        buildProvider.IgnoreControlProperties = true;

        // Process as many errors as possible, do not rethrow on first error
        buildProvider.ThrowOnFirstParseError = false;

        // Get the language (causes the file to be parsed)
        CompilerType compilerType = buildProvider.CodeCompilerType;

        // compilerType could be null in the no-compile case (VSWhidbey 221749)
        if (compilerType == null) {
            codeDomProviderType = null;
            compilerParameters = null;
            return null;
        }

        // Return the provider type and compiler params
        codeDomProviderType = compilerType.CodeDomProviderType;
        compilerParameters = compilerType.CompilerParameters;

        IAssemblyDependencyParser parser = buildProvider.AssemblyDependencyParser;

        // Add all the assemblies that the page depends on (e.g. user controls)
        if (parser != null && parser.AssemblyDependencies != null) {
            Util.AddAssembliesToStringCollection(parser.AssemblyDependencies,
                compilerParameters.ReferencedAssemblies);
        }

        // Make any fix up adjustments to the CompilerParameters to work around some issues
        AssemblyBuilder.FixUpCompilerParameters(compConfig, codeDomProviderType, compilerParameters);

        return buildProvider;
    }

    public override Object InitializeLifetimeService() {
        return null; // never expire lease
    }

    internal void Configure(ClientBuildManager client) {

        // Add a pending call to make sure our thread doesn't get killed
        AddPendingCall();

        try {
            _virtualPathProvider = new ClientVirtualPathProvider();
            HostingEnvironment.RegisterVirtualPathProviderInternal(_virtualPathProvider);

            _client = client;

            // Type description provider required for multitargeting compilation support in VS.
            if (_client.CBMTypeDescriptionProviderBridge != null) {
                TargetFrameworkUtil.CBMTypeDescriptionProviderBridge = _client.CBMTypeDescriptionProviderBridge;
            }

            // start watching for app domain unloading
            _onAppDomainUnload = new EventHandler(OnAppDomainUnload);
            Thread.GetDomain().DomainUnload += _onAppDomainUnload;

            _buildManager = BuildManager.TheBuildManager;

            // Listen to appdomain shutdown.
            HttpRuntime.AppDomainShutdown += new BuildManagerHostUnloadEventHandler(this.OnAppDomainShutdown);
        }
        finally {
            RemovePendingCall();
        }
    }

    internal Exception InitializationException {
        get {
            return HostingEnvironment.InitializationException;
        }
    }

    private void OnAppDomainUnload(Object unusedObject, EventArgs unusedEventArgs) {
        Thread.GetDomain().DomainUnload -= _onAppDomainUnload;

        if (_client != null) {
            _client.OnAppDomainUnloaded(HttpRuntime.ShutdownReason);
            _client = null;
        }
    }

    internal bool UnloadAppDomain() {
        _ignorePendingCalls = true;

        // Make sure HttpRuntime does not ignore the appdomain shutdown.
        HttpRuntime.SetUserForcedShutdown();

        // Force unload the appdomain when called from client
        return HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.UnloadAppDomainCalled, "CBM called UnloadAppDomain");
    }

    // This provider is created in the hosted appdomain for faster access of the virtual file content.
    // Note this is used both in CBM and the aspnet precompilation tool
    internal class ClientVirtualPathProvider : VirtualPathProvider {
        private IDictionary _stringDictionary;

        internal ClientVirtualPathProvider() {
            _stringDictionary = new HybridDictionary(true);
        }

        public override bool FileExists(string virtualPath) {
            if (_stringDictionary.Contains(virtualPath)) {
                return true;
            }

            return base.FileExists(virtualPath);
        }

        public override CacheDependency GetCacheDependency(string virtualPath,
            IEnumerable virtualPathDependencies, DateTime utcStart) {

            if (virtualPath != null) {
                virtualPath = UrlPath.MakeVirtualPathAppAbsolute(virtualPath);
                // Return now so the build result will be invalidated based on hashcode.
                // This is for the case that Venus passed in the file content so we don't
                // get file change notification
                if (_stringDictionary.Contains(virtualPath)) {
                    return null;
                }
            }

            // otherwise creates a cachedependency using MapPathBasedVirtualPathProvider
            return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        public override VirtualFile GetFile(string virtualPath) {
            String _virtualFileString = (String)_stringDictionary[virtualPath];

            return _virtualFileString != null? new ClientVirtualFile(virtualPath, _virtualFileString) : base.GetFile(virtualPath);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies) {
            HashCodeCombiner hashCodeCombiner = null;

            ArrayList clonedPaths = new ArrayList();
            foreach (string virtualDependency in virtualPathDependencies) {
                // if the virtual path is previously cached, add the actual content to
                // the hash code combiner.
                if (_stringDictionary.Contains(virtualDependency)) {
                    if (hashCodeCombiner == null) {
                        hashCodeCombiner = new HashCodeCombiner();
                    }

                    hashCodeCombiner.AddInt(_stringDictionary[virtualDependency].GetHashCode());
                    continue;
                }

                // Otherwise move it to the cloned collection and use the base class (previous provider)
                // to get the hash code.
                clonedPaths.Add(virtualDependency);
            }

            if (hashCodeCombiner == null) {
                return base.GetFileHash(virtualPath, virtualPathDependencies);
            }
            hashCodeCombiner.AddObject(base.GetFileHash(virtualPath, clonedPaths));

            return hashCodeCombiner.CombinedHashString;
        }

        internal void RegisterVirtualFile(VirtualPath virtualPath, String virtualFileString) {
            _stringDictionary[virtualPath.VirtualPathString] = virtualFileString;
        }

        internal void RevertVirtualFile(VirtualPath virtualPath) {
            _stringDictionary.Remove(virtualPath.VirtualPathString);
        }

        internal class ClientVirtualFile : VirtualFile {
            String _virtualFileString;

            internal ClientVirtualFile(string virtualPath, String virtualFileString) : base(virtualPath) {
                _virtualFileString = virtualFileString;
            }

            public override Stream Open() {
                Stream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream, Encoding.Unicode);

                writer.Write(_virtualFileString);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }
        }
    }
}
}
