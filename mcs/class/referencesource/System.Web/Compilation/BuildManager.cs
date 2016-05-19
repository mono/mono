//------------------------------------------------------------------------------
// <copyright file="BuildManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/************************************************************************************************************/



namespace System.Web.Compilation {

    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Profile;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;


    /// <devdoc>
    ///    <para>
    ///       IProvider compilation related services
    ///    </para>
    /// </devdoc>
    public sealed class BuildManager {

        /// Contants relating to generated assembly names

        // All generated assemblies start with this prefix
        internal const string AssemblyNamePrefix = "App_";

        // Web assemblies are the assemblies generated from web files (aspx, ascx, ...)
        internal const string WebAssemblyNamePrefix = AssemblyNamePrefix + "Web_";

        internal const string AppThemeAssemblyNamePrefix = AssemblyNamePrefix + "Theme_";
        internal const string GlobalThemeAssemblyNamePrefix = AssemblyNamePrefix + "GlobalTheme_";
        internal const string AppBrowserCapAssemblyNamePrefix = AssemblyNamePrefix + "Browsers";

        private const string CodeDirectoryAssemblyName = AssemblyNamePrefix + "Code";
        internal const string SubCodeDirectoryAssemblyNamePrefix = AssemblyNamePrefix + "SubCode_";
        private const string ResourcesDirectoryAssemblyName = AssemblyNamePrefix + "GlobalResources";
        private const string LocalResourcesDirectoryAssemblyName = AssemblyNamePrefix + "LocalResources";
        private const string WebRefDirectoryAssemblyName = AssemblyNamePrefix + "WebReferences";
        internal const string GlobalAsaxAssemblyName = AssemblyNamePrefix + HttpApplicationFactory.applicationFileName;

        private const string LicensesAssemblyName = AssemblyNamePrefix + "Licenses";

        internal const string UpdatableInheritReplacementToken = "__ASPNET_INHERITS";

        // Name of the temporary subdirectory under the codegen folder for buildproviders to generate embedded resource files.
        private const string CodegenResourceDirectoryName = "ResX";

        private static System.Security.Cryptography.RNGCryptoServiceProvider _rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        private static bool _theBuildManagerInitialized;
        private static Exception _initializeException;
        private static BuildManager _theBuildManager = new BuildManager();  // single instance of the class
        private static long s_topLevelHash;
        private readonly HashCodeCombiner _preAppStartHashCodeCombiner = new HashCodeCombiner();
        internal static BuildManager TheBuildManager { get { return _theBuildManager; } }

        // Precompilation related fields
        private const string precompMarkerFileName = "PrecompiledApp.config";
        private string _precompTargetPhysicalDir;
        private PrecompilationFlags _precompilationFlags;
        private bool _isPrecompiledApp;
        private bool _isPrecompiledAppComputed;
        private bool _isUpdatablePrecompiledApp;
        private bool _precompilingApp;  // we're in the process of precompiling an app

        private string _strongNameKeyFile;
        private string _strongNameKeyContainer;

        private string _codegenResourceDir;

        private bool _optimizeCompilations;
        internal static bool OptimizeCompilations {
            get { return _theBuildManager._optimizeCompilations; }
        }

        // filepath to the generated web.hash file, This file should only be re-created when
        // the appdomain is restarted and the top-level generated assemblies need to be recompiled.
        private string _webHashFilePath;
        internal static String WebHashFilePath {
            get { return _theBuildManager._webHashFilePath; }
        }

        private BuildResultCache[] _caches;
        private StandardDiskBuildResultCache _codeGenCache;
        private MemoryBuildResultCache _memoryCache;

        private bool _topLevelFilesCompiledStarted;
        private bool _topLevelFilesCompiledCompleted;
        private Exception _topLevelFileCompilationException;

        private BuildResultCompiledGlobalAsaxType _globalAsaxBuildResult;
        private Type _profileType;

        // Special top level directories that are treated differently from regular web directories
        // during precompilation (e.g. App_Code)
        private StringSet _excludedTopLevelDirectories;

        // Directories that are not requestable
        private StringSet _forbiddenTopLevelDirectories;

        private StringSet _excludedCodeSubdirectories;

        private List<VirtualPath> _excludedCompilationPaths;

        private CompilationStage _compilationStage = CompilationStage.PreTopLevelFiles;
        internal static CompilationStage CompilationStage { get { return _theBuildManager._compilationStage; } }

        private VirtualPath _scriptVirtualDir;
        private VirtualPath _globalAsaxVirtualPath;
        internal static VirtualPath ScriptVirtualDir { get { return _theBuildManager._scriptVirtualDir; } }
        internal static VirtualPath GlobalAsaxVirtualPath { get { return _theBuildManager._globalAsaxVirtualPath; } }

        private BuildManager() { }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal static bool InitializeBuildManager() {

            // If we already tried and got an exception, just rethrow it
            if (_initializeException != null) {
                // We need to wrap it in a new exception, otherwise we lose the original stack.
                throw new HttpException(_initializeException.Message, _initializeException);
            }

            if (!_theBuildManagerInitialized) {

                // If Fusion was not yet initialized, skip the init.
                // This can happen when there is a very early failure (e.g. see VSWhidbey 137366)
                Debug.Trace("BuildManager", "InitializeBuildManager " + HttpRuntime.FusionInited);
                if (!HttpRuntime.FusionInited)
                    return false;

                // Likewise, if the trust level has not yet been determined, skip the init (VSWhidbey 422311)
                if (HttpRuntime.TrustLevel == null)
                    return false;

                _theBuildManagerInitialized = true;
                try {
                    _theBuildManager.Initialize();
                }
                catch (Exception e) {
                    _theBuildManagerInitialized = false;
                    _initializeException = e;
                    throw;
                }
            }

            return true;
        }

        private ClientBuildManagerCallback _cbmCallback;
        internal static ClientBuildManagerCallback CBMCallback { get { return _theBuildManager._cbmCallback; } }

        private static bool _parseErrorReported;
        internal static void ReportParseError(ParserError parseError) {
            // If there is a CBM callback, inform it of the error
            if (BuildManager.CBMCallback != null) {
                _parseErrorReported = true;
                BuildManager.CBMCallback.ReportParseError(parseError);
            }
        }

        private void ReportTopLevelCompilationException() {
            Debug.Assert(_topLevelFileCompilationException != null);

            // Try to report the cached error to the CBM callback
            ReportErrorsFromException(_topLevelFileCompilationException);

            // We need to wrap it in a new exception, otherwise we lose the original stack.
            throw new HttpException(_topLevelFileCompilationException.Message,
                _topLevelFileCompilationException);
        }

        // Given an exception, attempt to turn it into calls to the CBM callback
        private void ReportErrorsFromException(Exception e) {
            // If there is no CBM callback, nothing to do
            if (BuildManager.CBMCallback == null)
                return;

            // Call the CBM callback as appropriate, based on the type of exception

            if (e is HttpCompileException) {
                CompilerResults results = ((HttpCompileException)e).Results;
                foreach (CompilerError error in results.Errors) {
                    BuildManager.CBMCallback.ReportCompilerError(error);
                }
            }
            else if (e is HttpParseException) {
                foreach (ParserError parseError in ((HttpParseException)e).ParserErrors) {
                    ReportParseError(parseError);
                }
            }
        }

        // The assemblies produced from the code directories and global.asax, which
        // every other compilation will linked with.
        private List<Assembly> _topLevelReferencedAssemblies = new List<Assembly>() {
            typeof(HttpRuntime).Assembly,
            typeof(System.ComponentModel.Component).Assembly,
        };

        private List<Assembly> TopLevelReferencedAssemblies { get { return _topLevelReferencedAssemblies; } }

        private Dictionary<String, AssemblyReferenceInfo> _topLevelAssembliesIndexTable;
        private IDictionary<String, AssemblyReferenceInfo> TopLevelAssembliesIndexTable { get { return _topLevelAssembliesIndexTable; } }

        private Dictionary<String, String> _generatedFileTable;
        internal static Dictionary<String, String> GenerateFileTable {
            get {
                if (_theBuildManager._generatedFileTable == null) {
                    _theBuildManager._generatedFileTable = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
                }

                return _theBuildManager._generatedFileTable;
            }
        }

        private ArrayList _codeAssemblies;
        public static IList CodeAssemblies {
            get {
                _theBuildManager.EnsureTopLevelFilesCompiled();
                return _theBuildManager._codeAssemblies;
            }
        }

        private IDictionary _assemblyResolveMapping;

        private Assembly _appResourcesAssembly;
        internal static Assembly AppResourcesAssembly { get { return _theBuildManager._appResourcesAssembly; } }

        // Indicates whether the parsers should coninue processing for more errors.
        // This is used in both CBM precompile-web, precompile-page and aspnet_compiler tool.
        private bool _throwOnFirstParseError = true;
        internal static bool ThrowOnFirstParseError {
            get { return _theBuildManager._throwOnFirstParseError; }
            set { _theBuildManager._throwOnFirstParseError = value; }
        }

        // Marks whether we are in the middle of performing precompilation, which affects how
        // we deal with error handling and batching
        private bool _performingPrecompilation = false;
        internal static bool PerformingPrecompilation {
            get { return _theBuildManager._performingPrecompilation; }
            set { _theBuildManager._performingPrecompilation = value; }
        }

        private bool _skipTopLevelCompilationExceptions;
        internal static bool SkipTopLevelCompilationExceptions {
            get { return _theBuildManager._skipTopLevelCompilationExceptions; }
            set { _theBuildManager._skipTopLevelCompilationExceptions = value; }
        }

        private static HashSet<Assembly> s_dynamicallyAddedReferencedAssembly = new HashSet<Assembly>();

        public static void AddReferencedAssembly(Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }
            ThrowIfPreAppStartNotRunning();

            s_dynamicallyAddedReferencedAssembly.Add(assembly);
        }

        /*
         * Return the list of assemblies that a compilation needs to reference for a given
         * config minus the top-level assemblies indexed later than removeIndex
         */
        internal static ICollection GetReferencedAssemblies(CompilationSection compConfig, int removeIndex) {
            AssemblySet referencedAssemblies = new AssemblySet();

            // Add all the config assemblies to the list
            foreach (AssemblyInfo a in compConfig.Assemblies) {
                Assembly[] assemblies = a.AssemblyInternal;
                if (assemblies == null) {
                    lock (compConfig) {
                        assemblies = a.AssemblyInternal;
                        if (assemblies == null)
                            // 
                            assemblies = a.AssemblyInternal = compConfig.LoadAssembly(a);
                    }
                }

                for (int i = 0; i < assemblies.Length; i++) {
                    if (assemblies[i] != null) {
                        referencedAssemblies.Add(assemblies[i]);
                    }
                }
            }

            // Clone the top level referenced assemblies (code + global.asax + etc...), up to the removeIndex
            for (int i = 0; i < removeIndex; i++) {
                referencedAssemblies.Add(TheBuildManager.TopLevelReferencedAssemblies[i]);
            }

            // 

            foreach (Assembly assembly in s_dynamicallyAddedReferencedAssembly) {
                referencedAssemblies.Add(assembly);
            }

            return referencedAssemblies;
        }

        internal static ICollection GetReferencedAssemblies(CompilationSection compConfig) {

            // Start by cloning the top level referenced assemblies (code + global.asax + etc...)
            AssemblySet referencedAssemblies = AssemblySet.Create(
                TheBuildManager.TopLevelReferencedAssemblies);

            // Add all the config assemblies to the list
            foreach (AssemblyInfo a in compConfig.Assemblies) {
                Assembly[] assemblies = a.AssemblyInternal;
                if (assemblies == null) {
                    lock (compConfig) {
                        assemblies = a.AssemblyInternal;
                        if (assemblies == null)
                            // 
                            assemblies = a.AssemblyInternal = compConfig.LoadAssembly(a);
                    }
                }

                for (int i = 0; i < assemblies.Length; i++) {
                    if (assemblies[i] != null) {
                        referencedAssemblies.Add(assemblies[i]);
                    }
                }
            }

            // 

            foreach (Assembly assembly in s_dynamicallyAddedReferencedAssembly) {
                referencedAssemblies.Add(assembly);
            }

            return referencedAssemblies;
        }


        /*
         * Return the list of assemblies that all page compilations need to reference. This includes
         * config assemblies (<assemblies> section), bin assemblies and assemblies built from the
         * app App_Code and other top level folders.
         */

        /// <devdoc>
        /// Returns the assemblies referenced at the root application level of the current appF
        /// </devdoc>
        public static ICollection GetReferencedAssemblies() {
            CompilationSection compConfig = MTConfigUtil.GetCompilationAppConfig();

            _theBuildManager.EnsureTopLevelFilesCompiled();

            return GetReferencedAssemblies(compConfig);
        }

        /// <summary>
        /// Specifies a string representing a dependency that the BuildManager factors when determining if a clean build is required.
        /// </summary>
        /// <param name="dependency">String representation of a dependency.</param>
        public static void AddCompilationDependency(string dependency) {
            if (String.IsNullOrEmpty(dependency)) {
                throw new ArgumentException(SR.GetString(SR.Parameter_can_not_be_empty), "dependency");
            }
            BuildManager.ThrowIfPreAppStartNotRunning();
            _theBuildManager._preAppStartHashCodeCombiner.AddObject(dependency);
        }

        /*
         * Perform initialization work that should only be done once (per app domain).
         */
        private void Initialize() {

            Debug.Assert(_caches == null);

            // Register an AssemblyResolve event
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.ResolveAssembly);

            _globalAsaxVirtualPath = HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombine(
                HttpApplicationFactory.applicationFileName);

            _webHashFilePath = Path.Combine(HttpRuntime.CodegenDirInternal, "hash\\hash.web");

            // Indicate whether we should ignore the top level compilation exceptions.
            // In CBM case, we want to continue processing the page and return partial info even
            // if the code files fail to compile.
            _skipTopLevelCompilationExceptions = BuildManagerHost.InClientBuildManager;

            // Deal with precompilation if we're in that mode
            SetPrecompilationInfo(HostingEnvironment.HostingParameters);

            MultiTargetingUtil.EnsureFrameworkNamesInitialized();

            // The init code depends on whether we're precompiling or running an app
            if (_precompTargetPhysicalDir != null) {

                // If the app is already precompiled, fail
                FailIfPrecompiledApp();

                PrecompilationModeInitialize();
            }
            else {
                // Check if this application has been precompiled by aspnet_compiler.exe
                if (IsPrecompiledApp) {
                    PrecompiledAppRuntimeModeInitialize();
                }
                else {
                    RegularAppRuntimeModeInitialize();
                }
            }

            _scriptVirtualDir = Util.GetScriptLocation();

            // Top level directories that have a special semantic
            _excludedTopLevelDirectories = new CaseInsensitiveStringSet();
            _excludedTopLevelDirectories.Add(HttpRuntime.BinDirectoryName);
            _excludedTopLevelDirectories.Add(HttpRuntime.CodeDirectoryName);
            _excludedTopLevelDirectories.Add(HttpRuntime.ResourcesDirectoryName);
            _excludedTopLevelDirectories.Add(HttpRuntime.LocalResourcesDirectoryName);
            _excludedTopLevelDirectories.Add(HttpRuntime.WebRefDirectoryName);
            _excludedTopLevelDirectories.Add(HttpRuntime.ThemesDirectoryName);

            // Top level directories that are not requestable
            // It's the same as _excludedTopLevelDirectories, except that we allow
            // the bin directory to avoid a v1 breaking change (VSWhidbey 465018)
            _forbiddenTopLevelDirectories = new CaseInsensitiveStringSet();
            _forbiddenTopLevelDirectories.Add(HttpRuntime.CodeDirectoryName);
            _forbiddenTopLevelDirectories.Add(HttpRuntime.ResourcesDirectoryName);
            _forbiddenTopLevelDirectories.Add(HttpRuntime.LocalResourcesDirectoryName);
            _forbiddenTopLevelDirectories.Add(HttpRuntime.WebRefDirectoryName);
            _forbiddenTopLevelDirectories.Add(HttpRuntime.ThemesDirectoryName);

            LoadLicensesAssemblyIfExists();
        }

        /*
         * Init code used when we are running a non-precompiled app
         */
        private void RegularAppRuntimeModeInitialize() {

            //
            // Initialize the caches
            //

            // Always try the memory cache first
            _memoryCache = new MemoryBuildResultCache(HttpRuntime.CacheInternal);

            // Use the standard disk cache for regular apps
            _codeGenCache = new StandardDiskBuildResultCache(HttpRuntime.CodegenDirInternal);

            _caches = new BuildResultCache[] { _memoryCache, _codeGenCache };
        }

        /*
         * Init code used when we are running a precompiled app
         */
        private void PrecompiledAppRuntimeModeInitialize() {

            //
            // Initialize the caches
            //

            // Always try the memory cache first
            _memoryCache = new MemoryBuildResultCache(HttpRuntime.CacheInternal);

            // Used the precomp cache for precompiled apps
            BuildResultCache preCompCache = new PrecompiledSiteDiskBuildResultCache(
                HttpRuntime.BinDirectoryInternal);

            // Also create a regular disk cache so that we can compile and cache additional things.
            // This is useful even in non-updatable precomp, to cache DefaultWsdlHelpGenerator.aspx.

            _codeGenCache = new StandardDiskBuildResultCache(HttpRuntime.CodegenDirInternal);

            _caches = new BuildResultCache[] { _memoryCache, preCompCache, _codeGenCache };
        }

        /*
         * Init code used when we are precompiling an app
         */
        private void PrecompilationModeInitialize() {

            // We are precompiling an app

            // Always try the memory cache first
            _memoryCache = new MemoryBuildResultCache(HttpRuntime.CacheInternal);

            // Create a regular disk cache, to take advantage of the fact that the app
            // may already have been compiled (and to cause it to be if it wasn't)
            _codeGenCache = new StandardDiskBuildResultCache(HttpRuntime.CodegenDirInternal);

            // Create a special disk cache in the target's bin directory.  Use a slightly different
            // implementation for the updatable case.
            string targetBinDir = Path.Combine(_precompTargetPhysicalDir, HttpRuntime.BinDirectoryName);
            BuildResultCache preCompilationCache;
            if (PrecompilingForUpdatableDeployment) {
                preCompilationCache = new UpdatablePrecompilerDiskBuildResultCache(targetBinDir);
            }
            else {
                preCompilationCache = new PrecompilerDiskBuildResultCache(targetBinDir);
            }

            _caches = new BuildResultCache[] { _memoryCache, preCompilationCache, _codeGenCache };
        }

        // Load the licenses assembly from the bin dir if it exists (DevDiv 42149)
        private void LoadLicensesAssemblyIfExists() {
            string licAssemblyPath = Path.Combine(HttpRuntime.BinDirectoryInternal, LicensesAssemblyName + ".dll");
            if (File.Exists(licAssemblyPath)) {
                Assembly.Load(LicensesAssemblyName);
            }
        }

        // DevDiv #520869: Signal the PortableCompilationOutputSnapshotType to
        // restore a snapshot of portable compilation output
        private static void RestorePortableCompilationOutputSnapshot() {
            if (BuildManagerHost.InClientBuildManager || 
                !AppSettings.PortableCompilationOutput || 
                String.IsNullOrEmpty(AppSettings.PortableCompilationOutputSnapshotType)) {
                return;
            }

            // If a PortableCompilationOutputSnapshotsType has been configured but failed to be loaded, let it throw
            Type t = Type.GetType(AppSettings.PortableCompilationOutputSnapshotType, true);
            object[] args = new Object[] { AppSettings.PortableCompilationOutputSnapshotTypeOptions };
            t.InvokeMember("RestoreSnapshot", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, t, args, CultureInfo.InvariantCulture);
        }

        private long CheckTopLevelFilesUpToDate(long cachedHash) {
            bool gotLock = false;
            try {
                // Grab the compilation mutex, since this method accesses the codegen files
                CompilationLock.GetLock(ref gotLock);

                return CheckTopLevelFilesUpToDateInternal(cachedHash);
            }
            finally {
                // Always release the mutex if we had taken it
                if (gotLock) {
                    CompilationLock.ReleaseLock();
                }
            }
        }

        /*
         * Check if the top level files are up to date, and cleanup the codegendir
         * if they are not.
         */
        private long CheckTopLevelFilesUpToDateInternal(long cachedHash) {
            Debug.Trace("BuildManager", "specialFilesCombinedHash=" + cachedHash);
            var specialFilesHashCodeCombiner = new HashCodeCombiner();

            // Delete all the non essential files left over in the codegen dir, unless
            // specialFilesCombinedHash is 0, in which case we delete *everything* further down
            if (cachedHash != 0) {
                _codeGenCache.RemoveOldTempFiles();
            }

            // Use a HashCodeCombiner object to handle the time stamps of all the 'special'
            // files and directories that all compilations depend on:
            // - System.Web.dll (in case there is a newer version of ASP.NET)
            // - ~\Bin directory
            // - ~\Resource directory
            // - ~\WebReferences directory
            // - ~\Code directory
            // - global.asax

            // Add a check for the app's physical path, in case it changes (ASURT 12975)
            specialFilesHashCodeCombiner.AddObject(HttpRuntime.AppDomainAppPathInternal);

            // Process System.Web.dll
            string aspBinaryFileName = typeof(HttpRuntime).Module.FullyQualifiedName;
            if (!AppSettings.PortableCompilationOutput) {
                specialFilesHashCodeCombiner.AddFile(aspBinaryFileName);
            }
            else {
                specialFilesHashCodeCombiner.AddExistingFileVersion(aspBinaryFileName);
            }

            // Process machine.config
            string machineConfigFileName = HttpConfigurationSystem.MachineConfigurationFilePath;
            if (!AppSettings.PortableCompilationOutput) {
                specialFilesHashCodeCombiner.AddFile(machineConfigFileName);
            }
            else {
                specialFilesHashCodeCombiner.AddFileContentHash(machineConfigFileName);
            }

            // Process root web.config
            string rootWebConfigFileName = HttpConfigurationSystem.RootWebConfigurationFilePath;
            if (!AppSettings.PortableCompilationOutput) {
                specialFilesHashCodeCombiner.AddFile(rootWebConfigFileName);
            }
            else {
                specialFilesHashCodeCombiner.AddFileContentHash(rootWebConfigFileName);
            }

            RuntimeConfig appConfig = RuntimeConfig.GetAppConfig();
            CompilationSection compConfig = appConfig.Compilation;

            // Ignore the OptimizeCompilations flag in ClientBuildManager mode
            if (!BuildManagerHost.InClientBuildManager) {
                _optimizeCompilations = compConfig.OptimizeCompilations;
            }

            // In optimized compilation mode, we don't clean out all the compilations just because a top level
            // file changes.  Instead, we let already compiled pages run against the newer top level binaries.
            // In can be incorrect in some cases (e.g. return type of method changes from int to short), which is
            // why the optimization is optional
            if (!OptimizeCompilations) {
                // Add a dependency of the bin, resources, webresources and code directories
                string binPhysicalDir = HttpRuntime.BinDirectoryInternal;
                specialFilesHashCodeCombiner.AddDirectory(binPhysicalDir);

                // Note that we call AddResourcesDirectory instead of AddDirectory, since we only want
                // culture neutral files to be taken into account (VSWhidbey 359029)
                specialFilesHashCodeCombiner.AddResourcesDirectory(HttpRuntime.ResourcesDirectoryVirtualPath.MapPathInternal());

                specialFilesHashCodeCombiner.AddDirectory(HttpRuntime.WebRefDirectoryVirtualPath.MapPathInternal());

                specialFilesHashCodeCombiner.AddDirectory(HttpRuntime.CodeDirectoryVirtualPath.MapPathInternal());

                // Add a dependency on the global asax file.
                specialFilesHashCodeCombiner.AddFile(GlobalAsaxVirtualPath.MapPathInternal());
            }

            // Add a dependency on the hash of the app level <compilation> section, since it
            // affects all compilations, including the code directory.  It it changes,
            // we may as well, start all over.
            specialFilesHashCodeCombiner.AddObject(compConfig.RecompilationHash);

            ProfileSection profileSection = appConfig.Profile;
            specialFilesHashCodeCombiner.AddObject(profileSection.RecompilationHash);

            // Add a dependency on file encoding (DevDiv 4560)
            specialFilesHashCodeCombiner.AddObject(appConfig.Globalization.FileEncoding);

            // Also add a dependency on the <trust> config section
            TrustSection casConfig = appConfig.Trust;
            specialFilesHashCodeCombiner.AddObject(casConfig.Level);
            specialFilesHashCodeCombiner.AddObject(casConfig.OriginUrl);

            // Add a dependency on whether profile is enabled
            specialFilesHashCodeCombiner.AddObject(ProfileManager.Enabled);

            // Add a dependency to the force debug flag.
            specialFilesHashCodeCombiner.AddObject(PrecompilingWithDebugInfo);

            CheckCodeGenFiles(specialFilesHashCodeCombiner.CombinedHash, cachedHash);
            return specialFilesHashCodeCombiner.CombinedHash;
        }

        private void AfterPreAppStartExecute(Tuple<long, long> currentHash, Tuple<long, long> cachedTopLevelFilesHash) {
            bool gotLock = false;
            try {
                // Grab the compilation mutex, since this method accesses the codegen files
                CompilationLock.GetLock(ref gotLock);

                // After pre app start methods have executed, the second hash value should match the current value in the hash code combiner.
                CheckCodeGenFiles(currentHash.Item2, cachedTopLevelFilesHash.Item2);

                if (!cachedTopLevelFilesHash.Equals(currentHash)) {
                    // Hash has changed. Persist it to disk
                    _codeGenCache.SavePreservedSpecialFilesCombinedHash(currentHash);
                }

                // VSWhidbey 537929 : Setup a filechange monitor for the web.hash file. If this file is modified,
                // we will need to shutdown the appdomain so we don't use the obsolete assemblies. The new appdomain
                // will use the up-to-date assemblies.
                HttpRuntime.FileChangesMonitor.StartMonitoringFile(_webHashFilePath,
                    new FileChangeEventHandler(this.OnWebHashFileChange));
                Debug.Assert(File.Exists(_webHashFilePath), _webHashFilePath);
            }
            finally {
                // Always release the mutex if we had taken it
                if (gotLock) {
                    CompilationLock.ReleaseLock();
                }
            }
        }

        private void CheckCodeGenFiles(long currentHash, long cachedTopLevelFilesHash) {
            // Store the top level hash
            s_topLevelHash = currentHash;

            if (PrecompilingForCleanBuild || currentHash != cachedTopLevelFilesHash) {
                if (PrecompilingForCleanBuild) {
                    Debug.Trace("BuildManager", "Precompiling for clean build.");
                }
                else {
                    Debug.Trace("BuildManager", "EnsureFirstTimeInit: hash codes don't match.  Old=" +
                        cachedTopLevelFilesHash + " New=" + currentHash);
                }

                _codeGenCache.RemoveAllCodegenFiles();
            }
            else {
                Debug.Trace("BuildManager", "BuildManager: the special files are up to date");
            }
        }

        private void OnWebHashFileChange(Object sender, FileChangeEvent e) {
            // Shutdown the app domain
            Debug.Trace("BuildManager", _webHashFilePath + " changed - shutting down the app domain");
            Debug.Trace("AppDomainFactory", "Shutting down appdomain because " + _webHashFilePath + " file changed");
            string message = FileChangesMonitor.GenerateErrorMessage(e.Action, _webHashFilePath);
            if (message == null) {
                message = "Change in " + _webHashFilePath;
            }
            HttpRuntime.ShutdownAppDomain(ApplicationShutdownReason.BuildManagerChange, message);
        }

        /*
         * Check if an assembly name is reserved for a special purpose
         */
        internal static bool IsReservedAssemblyName(string assemblyName) {

            if (String.Compare(assemblyName, CodeDirectoryAssemblyName,
                    StringComparison.OrdinalIgnoreCase) == 0 ||
                String.Compare(assemblyName, ResourcesDirectoryAssemblyName,
                    StringComparison.OrdinalIgnoreCase) == 0 ||
                String.Compare(assemblyName, WebRefDirectoryAssemblyName,
                    StringComparison.OrdinalIgnoreCase) == 0 ||
                String.Compare(assemblyName, GlobalAsaxAssemblyName,
                    StringComparison.OrdinalIgnoreCase) == 0) {

                return true;
            }

            return false;
        }

        internal static void ThrowIfPreAppStartNotRunning() {
            if (PreStartInitStage != PreStartInitStage.DuringPreStartInit) {
                throw new InvalidOperationException(SR.GetString(SR.Method_can_only_be_called_during_pre_start_init));
            }
        }

        internal static PreStartInitStage PreStartInitStage { get; private set; }

        internal static void ExecutePreAppStart() {
            // Restore a snapshot of compilation output when the AppDomain just starts and 
            // before any web site code runs
            BuildManager.RestorePortableCompilationOutputSnapshot();
        
            string preStartInitListPath = Path.Combine(HttpRuntime.CodegenDirInternal, "preStartInitList.web");
            Tuple<long, long> specialFilesCombinedHash = _theBuildManager._codeGenCache.GetPreservedSpecialFilesCombinedHash();
            // Check top level files have changed
            long topLevelFilesHash = _theBuildManager.CheckTopLevelFilesUpToDate(specialFilesCombinedHash.Item1);

            bool hasUpdated = false; 
            ISet<string> preApplicationStartAssemblyNames = CallPreStartInitMethods(preStartInitListPath, out hasUpdated);

            // Check if pre application start code hashes have changed since.
            var currentHash = Tuple.Create(topLevelFilesHash, _theBuildManager._preAppStartHashCodeCombiner.CombinedHash);
            _theBuildManager.AfterPreAppStartExecute(currentHash, specialFilesCombinedHash);

            // Save the cache file only if needed
            if (hasUpdated) {
                SavePreStartInitAssembliesToFile(preStartInitListPath, preApplicationStartAssemblyNames);
            }
        }

        // this method requires global lock as the part of the fix of DevDiv bug 501777
        private static ISet<string> CallPreStartInitMethods(string preStartInitListPath, out bool isRefAssemblyLoaded) {
            Debug.Assert(PreStartInitStage == Compilation.PreStartInitStage.BeforePreStartInit);
            isRefAssemblyLoaded = false;
            using (new ApplicationImpersonationContext()) {
                ICollection<MethodInfo> methods = null;
                ICollection<Assembly> cachedPreStartAssemblies = LoadCachedPreAppStartAssemblies(preStartInitListPath);
                if (cachedPreStartAssemblies != null) {
                    methods = GetPreStartInitMethodsFromAssemblyCollection(cachedPreStartAssemblies, buildingFromCache: true);
                }

                if (methods == null) {
                    // In case of ctlr-f5 scenario, two processes (VS and IisExpress) will start compilation simultaneously.
                    // GetPreStartInitMethodsFromReferencedAssemblies() will load all referenced assemblies
                    // If shallow copy is enabled, one process may fail due race condition in copying assemblies (DevDiv bug 501777) 
                    // to fix it, put GetPreStartInitMethodsFromReferencedAssemblies() under the global lock 
                    bool gotLock = false;
                    try {
                        CompilationLock.GetLock(ref gotLock);
                        methods = GetPreStartInitMethodsFromReferencedAssemblies();
                        isRefAssemblyLoaded = true;
                    }
                    finally {
                        if (gotLock) {
                            CompilationLock.ReleaseLock();
                        }
                    }
                }

                InvokePreStartInitMethods(methods);

                Debug.Assert(PreStartInitStage == Compilation.PreStartInitStage.AfterPreStartInit);

                return new HashSet<string>(methods.Select(m => m.DeclaringType.Assembly.FullName), StringComparer.OrdinalIgnoreCase);
            }
        }

        internal static ISet<string> GetPreStartInitAssembliesFromFile(string path) {
            if (FileUtil.FileExists(path)) {
                try {
                    return new HashSet<string>(File.ReadAllLines(path), StringComparer.OrdinalIgnoreCase);
                }
                catch {
                    // If there are issues delete the bad file. The list will be created from scratch.
                    try {
                        File.Delete(path);
                    }
                    catch { }
                }
            }
            return null;
        }

        // this method requires global lock as the part of the fix of DevDiv bug 501777
        internal static void SavePreStartInitAssembliesToFile(string path, ISet<string> assemblies) {
            Debug.Assert(assemblies != null);
            Debug.Assert(!String.IsNullOrEmpty(path));
            Debug.Assert(!assemblies.Any(String.IsNullOrEmpty));
            bool gotLock = false;
            try {
                //put write under the global lock to avoid race condition
                CompilationLock.GetLock(ref gotLock);
                File.WriteAllLines(path, assemblies);
            }
            catch {
                try {
                    File.Delete(path);
                }
                catch { }
            }
            finally {
                if (gotLock) {
                    CompilationLock.ReleaseLock();
                }
            }
        }

        /// <summary>
        /// Load the cached list of assemblies containing pre app start methods. Since this is a cache we never throw from it.
        /// </summary>
        internal static ICollection<Assembly> LoadCachedPreAppStartAssemblies(string preStartInitListPath) {
            try {
                // Force the enumerable to be saved to a list so that any issues with loading assemblies get caught here.
                ISet<string> assemblyList = GetPreStartInitAssembliesFromFile(preStartInitListPath);
                if (assemblyList == null) {
                    return null;
                }
                return assemblyList.Select(Assembly.Load)
                                   .Distinct()
                                   .ToList();
            }
            catch {
                return null;
            }
        }

        private static void InvokePreStartInitMethods(ICollection<MethodInfo> methods) {
            PreStartInitStage = Compilation.PreStartInitStage.DuringPreStartInit;

            try {
                InvokePreStartInitMethodsCore(methods, HostingEnvironment.SetCultures);
                PreStartInitStage = Compilation.PreStartInitStage.AfterPreStartInit;
            }
            catch {
                PreStartInitStage = Compilation.PreStartInitStage.BeforePreStartInit;
                throw;
            }
        }

        internal static void InvokePreStartInitMethodsCore(ICollection<MethodInfo> methods, Func<IDisposable> setHostingEnvironmentCultures) {
            // Remove dupes 
            var methodsToExecute = methods.Distinct();
            // We want to execute PreApplicationStartmethods in a deterministic order. We'll use a sorted sequence of fully qualified type names and method names. 
            methodsToExecute = methodsToExecute.OrderBy(m => m.DeclaringType.AssemblyQualifiedName, StringComparer.OrdinalIgnoreCase)
                                               .ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var method in methodsToExecute) {
                try {
                    // 
                    using (setHostingEnvironmentCultures()) {
                        method.Invoke(null, null);
                    }
                }
                catch (TargetInvocationException e) {
                    string message = (e.InnerException != null ? e.InnerException.Message : String.Empty);
                    throw new InvalidOperationException(
                        SR.GetString(SR.Pre_application_start_init_method_threw_exception,
                            method.Name,
                            method.DeclaringType.FullName,
                            message),
                        e.InnerException);
                }
            }
        }

        private static ICollection<MethodInfo> GetPreStartInitMethodsFromReferencedAssemblies() {
            CompilationSection compConfig = MTConfigUtil.GetCompilationConfig(HttpRuntime.AppDomainAppVirtualPath);
            var referencedAssemblies = BuildManager.GetReferencedAssemblies(compConfig).Cast<Assembly>();
            return GetPreStartInitMethodsFromAssemblyCollection(referencedAssemblies, buildingFromCache: false);
        }

        /// <summary>
        /// Resolves pre application start methods from the assemblies specified.
        /// </summary>
        /// <param name="assemblies">The list of assemblies to look for methods in.</param>
        /// <param name="buildingFromCache">Flag that determines if we are rebuilding methods from cache.</param>
        internal static ICollection<MethodInfo> GetPreStartInitMethodsFromAssemblyCollection(IEnumerable<Assembly> assemblies, bool buildingFromCache) {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (Assembly assembly in assemblies) {
                PreApplicationStartMethodAttribute[] attributes = null;
                try {
                    attributes = (PreApplicationStartMethodAttribute[])assembly.GetCustomAttributes(typeof(PreApplicationStartMethodAttribute), inherit: true);
                }
                catch {
                    // GetCustomAttributes invokes the constructors of the attributes, so it is possible that they might throw unexpected exceptions.
                    // (Dev10 bug 831981)
                }

                if (attributes == null || !attributes.Any()) {
                    // When rebuilding methods from cache every assembly specified must have one or more PreApplicationStartMethod attributes. 
                    // If one of them doesn't, the cache might be stale. We'll force it to retry it with the list of assemblies currently loaded into the AppDomain.
                     if (buildingFromCache) {
                         return null;
                     }
                }
                else {
                    foreach (PreApplicationStartMethodAttribute attribute in attributes) {
                        MethodInfo method = null;
                        // Ensure the Type on the attribute is in the same assembly as the attribute itself
                        if (attribute.Type != null && !String.IsNullOrEmpty(attribute.MethodName) && attribute.Type.Assembly == assembly) {
                            method = FindPreStartInitMethod(attribute.Type, attribute.MethodName);
                        }

                        if (method != null) {
                            methods.Add(method);
                        }
                        else {
                            throw new HttpException(SR.GetString(SR.Invalid_PreApplicationStartMethodAttribute_value,
                                assembly.FullName,
                                (attribute.Type != null ? attribute.Type.FullName : String.Empty),
                                attribute.MethodName));
                        }
                    }
                }
            }
            return methods;
        }

        internal static MethodInfo FindPreStartInitMethod(Type type, string methodName) {
            Debug.Assert(type != null);
            Debug.Assert(!String.IsNullOrEmpty(methodName));
            MethodInfo method = null;
            if (type.IsPublic) {
                // Verify that type is public to avoid allowing internal code execution. This implementation will not match
                // nested public types.
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase,
                                binder: null,
                                types: Type.EmptyTypes,
                                modifiers: null);
            }
            return method;
        }

        // excludedSubdirectories contains a list of subdirectory names that should not be
        // recursively included in the compilation (they'll instead be compiled into their
        // own assemblies).
        private Assembly CompileCodeDirectory(VirtualPath virtualDir, CodeDirectoryType dirType,
            string assemblyName, StringSet excludedSubdirectories) {

            Debug.Trace("BuildManager", "CompileCodeDirectory(" + virtualDir.VirtualPathString + ")");

            bool isDirectoryAllowed = true;
            if (IsPrecompiledApp) {
                // Most special dirs are not allowed in precompiled apps.  App_LocalResources is
                // an exception, as it is allowed in updatable precompiled apps.
                if (IsUpdatablePrecompiledAppInternal && dirType == CodeDirectoryType.LocalResources)
                    isDirectoryAllowed = true;
                else
                    isDirectoryAllowed = false;
            }

            // Remember the referenced assemblies based on the current count.
            AssemblyReferenceInfo info = new AssemblyReferenceInfo(_topLevelReferencedAssemblies.Count);
            _topLevelAssembliesIndexTable[virtualDir.VirtualPathString] = info;

            Assembly codeAssembly = CodeDirectoryCompiler.GetCodeDirectoryAssembly(
                    virtualDir, dirType, assemblyName, excludedSubdirectories,
                    isDirectoryAllowed);

            if (codeAssembly != null) {

                // Remember the generated assembly
                info.Assembly = codeAssembly;

                // Page resource assemblies are not added to the top level list
                if (dirType != CodeDirectoryType.LocalResources) {
                    _topLevelReferencedAssemblies.Add(codeAssembly);

                    if (dirType == CodeDirectoryType.MainCode || dirType == CodeDirectoryType.SubCode) {
                        if (_codeAssemblies == null) {
                            _codeAssemblies = new ArrayList();
                        }

                        _codeAssemblies.Add(codeAssembly);
                    }

                    // Add it to the list of assembly name that we resolve, so that users can
                    // refer to the assemblies by their fixed name (even though they
                    // random names).  (VSWhidbey 276776)
                    if (_assemblyResolveMapping == null) {
                        _assemblyResolveMapping = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    }
                    _assemblyResolveMapping[assemblyName] = codeAssembly;

                    if (dirType == CodeDirectoryType.MainCode) {
                        // Profile gets built in the same assembly as the main code dir, so
                        // see whether we can get its type from the assembly.
                        _profileType = ProfileBuildProvider.GetProfileTypeFromAssembly(
                            codeAssembly, IsPrecompiledApp);

                        // To avoid breaking earlier Whidbey apps, allows the name "__code"
                        // to be used for the main code assembly.
                        // 
                        _assemblyResolveMapping["__code"] = codeAssembly;
                    }
                }
            }

            Debug.Trace("BuildManager", "CompileCodeDirectory generated assembly: " +
                (codeAssembly == null ? "None" : codeAssembly.ToString()));

            return codeAssembly;
        }


        private void CompileResourcesDirectory() {

            VirtualPath virtualDir = HttpRuntime.ResourcesDirectoryVirtualPath;

            Debug.Assert(_appResourcesAssembly == null);
            _appResourcesAssembly = CompileCodeDirectory(virtualDir, CodeDirectoryType.AppResources,
                ResourcesDirectoryAssemblyName, null /*excludedSubdirectories*/);
        }

        private void CompileWebRefDirectory() {

            CompileCodeDirectory(HttpRuntime.WebRefDirectoryVirtualPath, CodeDirectoryType.WebReferences,
                WebRefDirectoryAssemblyName, null /*excludedSubdirectories*/);
        }

        // Compute the list of subdirectories that should not be compiled with
        // the top level Code
        private void EnsureExcludedCodeSubDirectoriesComputed() {

            if (_excludedCodeSubdirectories != null)
                return;

            _excludedCodeSubdirectories = new CaseInsensitiveStringSet();

            // Get the list of sub directories that will be compiled separately
            CodeSubDirectoriesCollection codeSubDirectories = CompilationUtil.GetCodeSubDirectories();

            // Add them to the exclusion list of the top level code directory
            if (codeSubDirectories != null) {
                foreach (CodeSubDirectory entry in codeSubDirectories) {
                    _excludedCodeSubdirectories.Add(entry.DirectoryName);
                }
            }
        }

        private void CompileCodeDirectories() {

            VirtualPath virtualDir = HttpRuntime.CodeDirectoryVirtualPath;

            // Get the list of sub directories that will be compiled separately
            CodeSubDirectoriesCollection codeSubDirectories = CompilationUtil.GetCodeSubDirectories();

            if (codeSubDirectories != null) {

                // Compile all the subdirectory that are listed in config.

                foreach (CodeSubDirectory entry in codeSubDirectories) {

                    // 



                    VirtualPath virtualSubDir = virtualDir.SimpleCombineWithDir(entry.DirectoryName);

                    string assemblyName = SubCodeDirectoryAssemblyNamePrefix + entry.AssemblyName;

                    // Compile the subdirectory tree (no exclusions)
                    CompileCodeDirectory(virtualSubDir, CodeDirectoryType.SubCode, assemblyName,
                        null /*excludedSubdirectories*/);
                }
            }

            EnsureExcludedCodeSubDirectoriesComputed();

            // Compile the top level Code directory tree, minus the excluded subdirectories
            CompileCodeDirectory(virtualDir, CodeDirectoryType.MainCode,
                CodeDirectoryAssemblyName, _excludedCodeSubdirectories);
        }

        private void CompileGlobalAsax() {
            _globalAsaxBuildResult = ApplicationBuildProvider.GetGlobalAsaxBuildResult(IsPrecompiledApp);

            // Make sure that global.asax notifications are set up (VSWhidbey 267245)
            HttpApplicationFactory.SetupFileChangeNotifications();

            if (_globalAsaxBuildResult != null) {

                // We need to add not only the global.asax type, but also its parent types to
                // the top level assembly list.  This can happen when global.asax has a 'src'
                // attribute pointing to a source file containing its base type.
                Type type = _globalAsaxBuildResult.ResultType;
                while (type.Assembly != typeof(HttpRuntime).Assembly) {
                    _topLevelReferencedAssemblies.Add(type.Assembly);
                    type = type.BaseType;
                }
            }
        }

        // Call the AppInitialize method in the Code assembly if there is one
        internal static void CallAppInitializeMethod() {

            // Make sure the code directory has been processed
            _theBuildManager.EnsureTopLevelFilesCompiled();

            CodeDirectoryCompiler.CallAppInitializeMethod();
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void EnsureTopLevelFilesCompiled() {
            if (PreStartInitStage != Compilation.PreStartInitStage.AfterPreStartInit) {
                throw new InvalidOperationException(SR.GetString(SR.Method_cannot_be_called_during_pre_start_init));
            }

            // This should never get executed in non-hosted appdomains
            Debug.Assert(HostingEnvironment.IsHosted);

            // If we already tried and got an exception, just rethrow it
            if (_topLevelFileCompilationException != null && !SkipTopLevelCompilationExceptions) {
                ReportTopLevelCompilationException();
            }

            if (_topLevelFilesCompiledStarted)
                return;

            // Set impersonation to hosting identity (process or UNC)
            using (new ApplicationImpersonationContext()) {
                bool gotLock = false;
                _parseErrorReported = false;

                try {
                    // Grab the compilation mutex, since this method accesses the codegen files
                    CompilationLock.GetLock(ref gotLock);

                    // Check again if there is an exception
                    if (_topLevelFileCompilationException != null && !SkipTopLevelCompilationExceptions) {
                        ReportTopLevelCompilationException();
                    }

                    // Check again if we're done
                    if (_topLevelFilesCompiledStarted)
                        return;

                    _topLevelFilesCompiledStarted = true;
                    _topLevelAssembliesIndexTable =
                        new Dictionary<String, AssemblyReferenceInfo>(StringComparer.OrdinalIgnoreCase);

                    _compilationStage = CompilationStage.TopLevelFiles;

                    CompileResourcesDirectory();
                    CompileWebRefDirectory();
                    CompileCodeDirectories();

                    _compilationStage = CompilationStage.GlobalAsax;

                    CompileGlobalAsax();

                    _compilationStage = CompilationStage.BrowserCapabilities;

                    // Call GetBrowserCapabilitiesType() to make sure browserCap directory is compiled
                    // early on.  This avoids getting into potential deadlock situations later (VSWhidbey 530732).
                    // For the same reason, get the EmptyHttpCapabilitiesBase.
                    BrowserCapabilitiesCompiler.GetBrowserCapabilitiesType();
                    IFilterResolutionService dummy = HttpCapabilitiesBase.EmptyHttpCapabilitiesBase;

                    _compilationStage = CompilationStage.AfterTopLevelFiles;
                }
                catch (Exception e) {
                    // Remember the exception, and rethrow it
                    _topLevelFileCompilationException = e;

                    // Do not rethrow the exception since so CBM can still provide partial support
                    if (!SkipTopLevelCompilationExceptions) {

                        if (!_parseErrorReported) {
                            // Report the error if this is not a CompileException. CompileExceptions are handled
                            // directly by the AssemblyBuilder already.
                            if (!(e is HttpCompileException)) {
                                ReportTopLevelCompilationException();
                            }
                        }

                        throw;
                    }
                }
                finally {
                    _topLevelFilesCompiledCompleted = true;

                    // Always release the mutex if we had taken it
                    if (gotLock) {
                        CompilationLock.ReleaseLock();
                    }
                }
            }
        }

        // Generate a random file name with 8 characters
        private static string GenerateRandomFileName() {
            // Generate random bytes
            byte[] data = new byte[6];

            lock (_rng) {
                _rng.GetBytes(data);
            }

            // Turn them into a string containing only characters valid in file names/url
            string s = Convert.ToBase64String(data).ToLower(CultureInfo.InvariantCulture);
            s = s.Replace('/', '-');
            s = s.Replace('+', '_');

            return s;
        }

        internal static string GenerateRandomAssemblyName(string baseName) {
            return GenerateRandomAssemblyName(baseName, true /*topLevel*/);
        }

        // Generate a random name for an assembly, starting with the passed in prefix
        internal static string GenerateRandomAssemblyName(string baseName, bool topLevel) {

            // Start with the passed in base name
            string assemblyName = baseName;

            // Append a random token to it.

            // However, don't do this when precompiling for deployment since, we want the name to be more predictable (DevDiv 36625)
            if (PrecompilingForDeployment)
                return baseName;

            // Also, don't use random names for top level files in OptimizeCompilations mode so that pages
            // can more easily bind against rebuilt top level assemblies
            if (OptimizeCompilations && topLevel)
                return baseName;

            return baseName += "." + GenerateRandomFileName();
        }

        private static string GetGeneratedAssemblyBaseName(VirtualPath virtualPath) {

            // Name the assembly using the same scheme as cache keys
            return GetCacheKeyFromVirtualPath(virtualPath);
        }

        /*
         * Look for a type by name in the top level and config assemblies
         */
        public static Type GetType(string typeName, bool throwOnError) {
            return GetType(typeName, throwOnError, false);
        }

        /*
         * Look for a type by name in the top level and config assemblies
         */
        public static Type GetType(string typeName, bool throwOnError, bool ignoreCase) {
            // If it contains an assembly name, just call Type.GetType().  Do this before even trying
            // to initialize the BuildManager, so that if InitializeBuildManager has errors, it doesn't
            // affect us when the type string can be resolved via Type.GetType().
            Type type = null;
            if (Util.TypeNameContainsAssembly(typeName)) {
                type = Type.GetType(typeName, throwOnError, ignoreCase);

                if (type != null) {
                    return type;
                }
            }

            // Make sure the build manager is initialized.  If it fails to initialize for any reason,
            // don't attempt to use the fancy GetType logic.  Just call Type.GetType instead (VSWhidbey 284498)
            if (!InitializeBuildManager()) {
                return Type.GetType(typeName, throwOnError, ignoreCase);
            }

            // First, always try System.Web.dll
            try {
                type = typeof(BuildManager).Assembly.GetType(typeName,
                    false /*throwOnError*/, ignoreCase);
            }
            catch (ArgumentException e) {
                // Even though we pass false to throwOnError, GetType can throw if the
                // assembly name is malformed.  In that case, throw our own error instead
                // of the cryptic ArgumentException (VSWhidbey 275586)
                throw new HttpException(
                    SR.GetString(SR.Invalid_type, typeName), e);
            }

            if (type != null) return type;

            _theBuildManager.EnsureTopLevelFilesCompiled();

            // Otherwise, look for the type in the top level assemblies
            type = Util.GetTypeFromAssemblies(TheBuildManager.TopLevelReferencedAssemblies,
                typeName, ignoreCase);
            if (type != null) return type;

            // Otherwise, look for the type in the config assemblies
            IEnumerable<Assembly> configAssemblies = GetAssembliesForAppLevel();
            type = Util.GetTypeFromAssemblies(configAssemblies, typeName, ignoreCase);

            if (type == null && throwOnError) {
                throw new HttpException(
                    SR.GetString(SR.Invalid_type, typeName));
            }

            return type;
        }

        /*
        * Simple wrapper to get the Assemblies
        */
        private static IEnumerable<Assembly> GetAssembliesForAppLevel() {
            CompilationSection compilationConfiguration = MTConfigUtil.GetCompilationAppConfig();
            AssemblyCollection assemblyInfoCollection = compilationConfiguration.Assemblies;

            Debug.Assert(s_dynamicallyAddedReferencedAssembly != null);

            if (assemblyInfoCollection == null) {
                return s_dynamicallyAddedReferencedAssembly.OfType<Assembly>();
            }

            return assemblyInfoCollection.Cast<AssemblyInfo>()
                .SelectMany(ai => ai.AssemblyInternal)
                .Union(s_dynamicallyAddedReferencedAssembly)
                .Distinct();
        }


        /*
         * Gets a type from one of the code assemblies
         */
        internal static Type GetTypeFromCodeAssembly(string typeName, bool ignoreCase) {

            // No code assembly: return
            if (CodeAssemblies == null)
                return null;

            return Util.GetTypeFromAssemblies(CodeAssemblies, typeName, ignoreCase);
        }

        internal static BuildProvider CreateBuildProvider(VirtualPath virtualPath,
            CompilationSection compConfig, ICollection referencedAssemblies,
            bool failIfUnknown) {

            return CreateBuildProvider(virtualPath, BuildProviderAppliesTo.Web,
                compConfig, referencedAssemblies, failIfUnknown);
        }

        internal static BuildProvider CreateBuildProvider(VirtualPath virtualPath,
            BuildProviderAppliesTo neededFor,
            CompilationSection compConfig, ICollection referencedAssemblies,
            bool failIfUnknown) {

            string extension = virtualPath.Extension;

            Type buildProviderType = CompilationUtil.GetBuildProviderTypeFromExtension(compConfig,
                extension, neededFor, failIfUnknown);
            if (buildProviderType == null)
                return null;

            object o = HttpRuntime.CreatePublicInstance(buildProviderType);

            BuildProvider buildProvider = (BuildProvider)o;

            buildProvider.SetVirtualPath(virtualPath);
            buildProvider.SetReferencedAssemblies(referencedAssemblies);

            return buildProvider;
        }

        internal static void AddFolderLevelBuildProviders(BuildProviderSet buildProviders, VirtualPath virtualPath,
            FolderLevelBuildProviderAppliesTo appliesTo, CompilationSection compConfig, ICollection referencedAssemblies) {

            if (buildProviders == null) {
                return;
            }

            List<Type> buildProviderTypes = CompilationUtil.GetFolderLevelBuildProviderTypes(compConfig, appliesTo);
            if (buildProviderTypes != null) {
                foreach (Type buildProviderType in buildProviderTypes) {
                    object o = HttpRuntime.CreatePublicInstance(buildProviderType);

                    BuildProvider buildProvider = (BuildProvider)o;

                    buildProvider.SetVirtualPath(virtualPath);
                    buildProvider.SetReferencedAssemblies(referencedAssemblies);

                    buildProviders.Add(buildProvider);

                }
            }
        }

        internal static void ValidateCodeFileVirtualPath(VirtualPath virtualPath) {
            _theBuildManager.ValidateVirtualPathInternal(virtualPath, false /*allowCrossApp*/, true /*codeFile*/);
        }

        private void ValidateVirtualPathInternal(VirtualPath virtualPath, bool allowCrossApp, bool codeFile) {

            if (!allowCrossApp) {
                virtualPath.FailIfNotWithinAppRoot();
            }
            else {
                // If cross app is allowed, and the path is in a different app, nothing more to check
                if (!virtualPath.IsWithinAppRoot)
                    return;
            }

            //
            // Now, detect if it's under a special directory (e.g. 'code', 'resources', 'themes')
            //

            // If it's exactly the app root, it's fine
            if (HttpRuntime.AppDomainAppVirtualPathObject == virtualPath)
                return;

            int appPathLen = HttpRuntime.AppDomainAppVirtualPathString.Length;

            string virtualPathString = virtualPath.VirtualPathString;

            // This could happen if the vpath is "/app" (while the app vpath is "/app/")
            if (virtualPathString.Length < appPathLen)
                return;

            // If no slash after the approot (e.g. "/app/foo.aspx"), it's valid
            int slashIndex = virtualPathString.IndexOf('/', appPathLen);
            if (slashIndex < 0)
                return;

            // Get the name of the first directory under the app root (e.g. "/app/aaa/bbb/foo.aspx" -> "aaa")
            string dir = virtualPathString.Substring(appPathLen, slashIndex - appPathLen);

            // If it's a forbidden directory, fail
            if (_forbiddenTopLevelDirectories.Contains(dir)) {
                throw new HttpException(SR.GetString(SR.Illegal_special_dir, virtualPathString, dir));
            }
        }

        /*
         * Returns a single hash code that represents the state of the built object for
         * the passed in virtualPath.  If it isn't already built, don't build it, but just
         * return 0.  This can be used to determine the validity of output cache that
         * has been persisted to disk.
         */
        internal static long GetBuildResultHashCodeIfCached(
            HttpContext context, string virtualPath) {

            BuildResult result = GetVPathBuildResult(context, VirtualPath.Create(virtualPath),
                true /*noBuild*/, false /*allowCrossApp*/);

            // If it's not cached, return 0
            if (result == null)
                return 0;

            // Return a single hash code based on both of the BuildResult's hash codes
            string dependenciesHash = result.VirtualPathDependenciesHash;
            Debug.Assert(result.DependenciesHashComputed);
            return result.ComputeHashCode(s_topLevelHash, StringUtil.GetStringHashCode(dependenciesHash));
        }

        internal static BuildResult GetVPathBuildResult(VirtualPath virtualPath) {

            return GetVPathBuildResult(null /*context*/, virtualPath,
                false /*noBuild*/, false /*allowCrossApp*/, false /*allowBuiltInPrecompile*/);
        }

        internal static BuildResult GetVPathBuildResult(HttpContext context, VirtualPath virtualPath) {

            return GetVPathBuildResult(context, virtualPath, false /*noBuild*/, false /*allowCrossApp*/, false /*allowBuiltInPrecompile*/);
        }

        internal static BuildResult GetVPathBuildResult(HttpContext context, VirtualPath virtualPath,
            bool noBuild, bool allowCrossApp) {

            return GetVPathBuildResult(context, virtualPath, noBuild, allowCrossApp, false /*allowBuiltInPrecompile*/);
        }

        /*
         * Calls either GetVPathBuildResultWithNoAssert or GetVPathBuildResultWithAssert,
         * depending on whether there is any point in asserting.
         */
        internal static BuildResult GetVPathBuildResult(HttpContext context, VirtualPath virtualPath,
            bool noBuild, bool allowCrossApp, bool allowBuildInPrecompile, bool ensureIsUpToDate = true) {

            // Could be called with user code on the stack, so need to assert here (VSWhidbey 85026)
            // e.g. This can happen during a Server.Transfer, or a LoadControl.
            // But if we're running in full trust, skip the assert for perf reasons (VSWhidbey 146871)
            if (HttpRuntime.IsFullTrust) {
                return GetVPathBuildResultWithNoAssert(context, virtualPath, noBuild, allowCrossApp, allowBuildInPrecompile, throwIfNotFound: true, ensureIsUpToDate: ensureIsUpToDate);
            }
            else {
                return GetVPathBuildResultWithAssert(context, virtualPath, noBuild, allowCrossApp, allowBuildInPrecompile, throwIfNotFound: true, ensureIsUpToDate: ensureIsUpToDate);
            }
        }


        internal static BuildResult GetVPathBuildResultWithAssert(
            HttpContext context, VirtualPath virtualPath, bool noBuild, bool allowCrossApp, bool allowBuildInPrecompile) {
            return GetVPathBuildResultWithAssert(context, virtualPath, noBuild, allowCrossApp, allowBuildInPrecompile, true/*throwIfNotFound*/);
        }

        /*
         * Same as GetVPathBuildResultWithNoAssert, but with an Unrestricted Assert.
         */
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal static BuildResult GetVPathBuildResultWithAssert(
            HttpContext context, VirtualPath virtualPath, bool noBuild, bool allowCrossApp, bool allowBuildInPrecompile, bool throwIfNotFound, bool ensureIsUpToDate = true) {

            return GetVPathBuildResultWithNoAssert(context, virtualPath, noBuild, allowCrossApp, allowBuildInPrecompile, throwIfNotFound, ensureIsUpToDate);
        }

        internal static BuildResult GetVPathBuildResultWithNoAssert(
            HttpContext context, VirtualPath virtualPath, bool noBuild, bool allowCrossApp, bool allowBuildInPrecompile) {
            return GetVPathBuildResultWithNoAssert(context, virtualPath, noBuild, allowCrossApp, allowBuildInPrecompile, true/*throwIfNotFound*/);
        }

        internal static BuildResult GetVPathBuildResultWithNoAssert(
            HttpContext context, VirtualPath virtualPath, bool noBuild, bool allowCrossApp, bool allowBuildInPrecompile, bool throwIfNotFound, bool ensureIsUpToDate = true) {

            using (new ApplicationImpersonationContext()) {
                return _theBuildManager.GetVPathBuildResultInternal(virtualPath, noBuild, allowCrossApp, allowBuildInPrecompile, throwIfNotFound, ensureIsUpToDate);
            }
        }

        // name of the slot in call context
        private const String CircularReferenceCheckerSlotName = "CircRefChk";

        private BuildResult GetVPathBuildResultInternal(VirtualPath virtualPath, bool noBuild, bool allowCrossApp, bool allowBuildInPrecompile, bool throwIfNotFound, bool ensureIsUpToDate = true) {

            Debug.Trace("BuildManager", "GetBuildResult(" + virtualPath + ")");

            // This should never be called while building top level files (VSWhidbey 480256)
            if (_compilationStage == CompilationStage.TopLevelFiles) {
                throw new HttpException(SR.GetString(SR.Too_early_for_webfile, virtualPath));
            }

            // Make sure the path is not relative
            Debug.Assert(!virtualPath.IsRelative);

            // Try the cache first before getting the mutex
            BuildResult result = GetVPathBuildResultFromCacheInternal(virtualPath, ensureIsUpToDate);
            if (result != null)
                return result;

            // If we were only checking the cache and it wasn't there, return null.
            if (noBuild)
                return null;

            // Check if it's trying to go cross app, or points to a special directory.
            // It's important to do this before checkin existence, to avoid revealing information
            // about other apps (VSWhidbey 442632)
            ValidateVirtualPathInternal(virtualPath, allowCrossApp, false /*codeFile*/);

            if (throwIfNotFound) {
                // Before grabbing the lock, make sure the file at least exists (ASURT 46465)
                Util.CheckVirtualFileExists(virtualPath);
            }
            else if (!virtualPath.FileExists()) {
                return null;
            }

            // If this is a precompiled app, complain if we couldn't find it in the cache
            if (IsNonUpdatablePrecompiledApp && !allowBuildInPrecompile) {
                throw new HttpException(
                    SR.GetString(SR.Cant_update_precompiled_app, virtualPath));
            }

            bool gotLock = false;

            try {
                // Grab the compilation mutex
                CompilationLock.GetLock(ref gotLock);

                // Check the cache a second time after getting the mutex
                result = GetVPathBuildResultFromCacheInternal(virtualPath, ensureIsUpToDate);
                if (result != null)
                    return result;

                // Get the circular reference checker (create it if needed)
                VirtualPathSet circularReferenceChecker;
                circularReferenceChecker = CallContext.GetData(CircularReferenceCheckerSlotName)
                    as VirtualPathSet;
                if (circularReferenceChecker == null) {
                    circularReferenceChecker = new VirtualPathSet();

                    // Create it and save it in the CallContext
                    CallContext.SetData(CircularReferenceCheckerSlotName, circularReferenceChecker);
                }

                // If a circular reference is detected, throw an error
                if (circularReferenceChecker.Contains(virtualPath)) {
                    throw new HttpException(
                        SR.GetString(SR.Circular_include));
                }

                // Add the current virtualPath to the circular reference checker
                circularReferenceChecker.Add(virtualPath);

                try {
                    // 
                    EnsureTopLevelFilesCompiled();
                    result = CompileWebFile(virtualPath);
                }
                finally {
                    // Remove the current virtualPath from the circular reference checker
                    Debug.Assert(circularReferenceChecker.Contains(virtualPath));
                    circularReferenceChecker.Remove(virtualPath);
                }
            }
            finally {
                // Always release the mutex if we had taken it
                if (gotLock) {
                    CompilationLock.ReleaseLock();
                }
            }

            return result;
        }

        private BuildResult CompileWebFile(VirtualPath virtualPath) {

            BuildResult result = null;
            string cacheKey = null;

            if (_topLevelFilesCompiledCompleted) {

                VirtualPath parentPath = virtualPath.Parent;

                // First, try to batch the directory if enabled
                if (IsBatchEnabledForDirectory(parentPath)) {
                    BatchCompileWebDirectory(null, parentPath, true /*ignoreErrors*/);

                    // If successful, it would have been cached to memory
                    cacheKey = GetCacheKeyFromVirtualPath(virtualPath);
                    result = _memoryCache.GetBuildResult(cacheKey);

                    if (result == null && DelayLoadType.Enabled) {
                        // We might not have cached the result in the memory cache
                        // if we are trying to delay loading the assembly.
                        result = GetBuildResultFromCache(cacheKey);
                    }

                    if (result != null) {
                        // If what we found in the cache is a CompileError, rethrow the exception
                        if (result is BuildResultCompileError) {
                            throw ((BuildResultCompileError)result).CompileException;
                        }

                        return result;
                    }
                }
            }


            DateTime utcStart = DateTime.UtcNow;

            // Name the assembly based on the virtual path, in order to get a recognizable name
            string outputAssemblyName = BuildManager.WebAssemblyNamePrefix +
                BuildManager.GenerateRandomAssemblyName(
                GetGeneratedAssemblyBaseName(virtualPath), false /*topLevel*/);


            BuildProvidersCompiler bpc = new BuildProvidersCompiler(virtualPath /*configPath*/, outputAssemblyName);

            // Create a BuildProvider based on the virtual path
            BuildProvider buildProvider = CreateBuildProvider(virtualPath, bpc.CompConfig,
                bpc.ReferencedAssemblies, true /*failIfUnknown*/);

            // Set the BuildProvider using a single item collection
            bpc.SetBuildProviders(new SingleObjectCollection(buildProvider));

            // Compile it
            CompilerResults results;

            try {
                results = bpc.PerformBuild();
                result = buildProvider.GetBuildResult(results);
            }
            catch (HttpCompileException e) {

                // If we're not supposed to cache the exception, just rethrow it
                if (e.DontCache)
                    throw;

                result = new BuildResultCompileError(virtualPath, e);

                // Add the dependencies to the compile error build provider, so that
                // we will retry compilation when a dependency changes
                buildProvider.SetBuildResultDependencies(result);

                // Remember the virtualpath dependencies, so that we will correctly
                // invalidate buildresult when depdency changes.
                e.VirtualPathDependencies = buildProvider.VirtualPathDependencies;

                // Cache it for next time
                CacheVPathBuildResultInternal(virtualPath, result, utcStart);

                // Set the DontCache flag, so that the exception will not be incorrectly
                // cached again lower down the stack (VSWhidbey 128234)
                e.DontCache = true;

                throw;
            }

            if (result == null)
                return null;

            // Cache it for next time
            CacheVPathBuildResultInternal(virtualPath, result, utcStart);

            if (!_precompilingApp && BuildResultCompiledType.UsesDelayLoadType(result)) {
                // The result uses DelayLoadType, which should not get exposed.
                // If we are not performing precompilation, then we should
                // get the actual result from cache and return that instead.
                if (cacheKey == null) {
                    cacheKey = GetCacheKeyFromVirtualPath(virtualPath);
                }
                result = BuildManager.GetBuildResultFromCache(cacheKey);
            }

            return result;
        }

        // Hashtbale to remember the local resources assembly for each directory (or null
        // if there isn't one). Hashtable<VirtualPath,Assembly>
        private Hashtable _localResourcesAssemblies = new Hashtable();

        private void EnsureFirstTimeDirectoryInit(VirtualPath virtualDir) {

            // Don't process local resources when precompiling for updatable deployment.
            // Instead, we deploy the App_LocalResources folder as is.
            if (PrecompilingForUpdatableDeployment)
                return;

            if (virtualDir == null)
                return;

            // Only do this once per directory
            if (_localResourcesAssemblies.Contains(virtualDir))
                return;

            // Don't do anything if it's outside the app root
            if (!virtualDir.IsWithinAppRoot)
                return;

            Debug.Trace("BuildManager", "EnsureFirstTimeDirectoryInit(" + virtualDir + ")");

            // Get the virtual path to the LocalResources subdirectory for this directory
            VirtualPath localResDir = virtualDir.SimpleCombineWithDir(HttpRuntime.LocalResourcesDirectoryName);

            bool dirExists;
            try {
                dirExists = localResDir.DirectoryExists();
            }
            catch {
                // If an exception happens, the directory may be outside the application,
                // in which case we should skip this logic, and act is if there are no
                // local resources (VSWhidbey 258776);

                _localResourcesAssemblies[virtualDir] = null;
                return;
            }

            Debug.Trace("BuildManager", "EnsureFirstTimeDirectoryInit: dirExists=" + dirExists);

            try {
                // Monitor changes to it so the appdomain can shut down when it changes
                HttpRuntime.StartListeningToLocalResourcesDirectory(localResDir);
            }
            catch {
                // could fail for long directory names
                if (dirExists) {
                    throw;
                }
            }

            Assembly resourceAssembly = null;

            // If it exists, build it
            if (dirExists) {

                string localResAssemblyName = GetLocalResourcesAssemblyName(virtualDir);

                bool gotLock = false;

                try {
                    // Grab the compilation mutex, since this method accesses the codegen files
                    CompilationLock.GetLock(ref gotLock);

                    resourceAssembly = CompileCodeDirectory(localResDir, CodeDirectoryType.LocalResources,
                        localResAssemblyName, null /*excludedSubdirectories*/);
                }
                finally {
                    // Always release the mutex if we had taken it
                    if (gotLock) {
                        CompilationLock.ReleaseLock();
                    }
                }
            }

            // Cache it whether it's null or not
            _localResourcesAssemblies[virtualDir] = resourceAssembly;
        }

        // VSWhidbey Bug 560521
        private void EnsureFirstTimeDirectoryInitForDependencies(ICollection dependencies) {
            foreach (String dependency in dependencies) {
                VirtualPath dependencyPath = VirtualPath.Create(dependency);
                VirtualPath dir = dependencyPath.Parent;
                EnsureFirstTimeDirectoryInit(dir);
            }
        }


        // Retrieve a cached local resources assembly (could be null)
        internal static Assembly GetLocalResourcesAssembly(VirtualPath virtualDir) {
            return (Assembly)_theBuildManager._localResourcesAssemblies[virtualDir];
        }

        internal static string GetLocalResourcesAssemblyName(VirtualPath virtualDir) {
            return LocalResourcesDirectoryAssemblyName + "." + GetGeneratedAssemblyBaseName(virtualDir);
        }

        // name of the slot in call context
        private const String BatchCompilationSlotName = "BatchCompileChk";

        // The semantics are
        //   true - always batch-compile
        //   false - never batch-compile
        //   null - determine from config
        private static bool? s_batchCompilationEnabled;

        public static Nullable<bool> BatchCompilationEnabled {
            get {
                return s_batchCompilationEnabled;
            }
            set {
                ThrowIfPreAppStartNotRunning();
                s_batchCompilationEnabled = value;
            }
        }

        // Check if batching is enabled for directory specified by virtualDir
        private bool IsBatchEnabledForDirectory(VirtualPath virtualDir) {
            // False if compile for fixed name
            if (CompileWithFixedAssemblyNames) {
                return false;
            }

            // Always enable batching for deployement
            if (PrecompilingForDeployment) {
                return true;
            }

            // If it's called by other non-precompile CBM methods, always disable batching
            if (BuildManagerHost.InClientBuildManager && !PerformingPrecompilation) {
                return false;
            }

            // If batch compilation was set through code use that setting
            if (BatchCompilationEnabled.HasValue) {
                return BatchCompilationEnabled.Value;
            }

            // Check the config
            return CompilationUtil.IsBatchingEnabled(virtualDir.VirtualPathString);
        }

        private bool BatchCompileWebDirectory(VirtualDirectory vdir, VirtualPath virtualDir, bool ignoreErrors) {

            // Exactly one of vdir and virtualDir should be non-null.  The idea is to avoid calling
            // VirtualPathProvider.GetDirectory if batching is disabled (VSWhidbey 437549).

            if (virtualDir == null)
                virtualDir = vdir.VirtualPathObject;

            if (vdir == null)
                vdir = HostingEnvironment.VirtualPathProvider.GetDirectory(virtualDir);

            // Then, check if we're already tried batch compiling this directory on this same request

            CaseInsensitiveStringSet directoryBatchCompilerChecker;
            directoryBatchCompilerChecker = CallContext.GetData(BatchCompilationSlotName)
                as CaseInsensitiveStringSet;

            if (directoryBatchCompilerChecker == null) {
                directoryBatchCompilerChecker = new CaseInsensitiveStringSet();

                // Create it and save it in the CallContext
                CallContext.SetData(BatchCompilationSlotName, directoryBatchCompilerChecker);
            }

            // If we've already tried batch compiling this directory, don't do anything
            if (directoryBatchCompilerChecker.Contains(vdir.VirtualPath))
                return false;

            // Add the current virtualDir to the batch compiler checker
            directoryBatchCompilerChecker.Add(vdir.VirtualPath);

            // If we're in the process of precompiling an app, never ignore errors.
            if (_precompilingApp)
                ignoreErrors = false;

            return BatchCompileWebDirectoryInternal(vdir, ignoreErrors);
        }

        private bool BatchCompileWebDirectoryInternal(VirtualDirectory vdir, bool ignoreErrors) {

            WebDirectoryBatchCompiler sdc = new WebDirectoryBatchCompiler(vdir);

            // Just ignore build providers that have errors
            if (ignoreErrors) {
                sdc.SetIgnoreErrors();

                // Don't propagate errors that happen during batch compilation
                try {
                    sdc.Process();
                }
                catch {
                    return false;
                }
            }
            else {
                sdc.Process();
            }

            return true;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Global Asax is a well-known concept")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "This might cick off top-level compilation so it's too big for a property")]
        public static Type GetGlobalAsaxType() {
            return _theBuildManager.GetGlobalAsaxTypeInternal();
        }

        private Type GetGlobalAsaxTypeInternal() {
            EnsureTopLevelFilesCompiled();

            if (_globalAsaxBuildResult == null)
                return PageParser.DefaultApplicationBaseType ?? typeof(HttpApplication);

            return _globalAsaxBuildResult.ResultType;
        }

        internal static BuildResultCompiledGlobalAsaxType GetGlobalAsaxBuildResult() {
            return _theBuildManager.GetGlobalAsaxBuildResultInternal();
        }

        private BuildResultCompiledGlobalAsaxType GetGlobalAsaxBuildResultInternal() {
            EnsureTopLevelFilesCompiled();

            return _globalAsaxBuildResult;
        }

        internal string[] GetCodeDirectories() {

            VirtualPath virtualDir = HttpRuntime.CodeDirectoryVirtualPath;

            // If there is no Code directory, return an empty array
            if (!virtualDir.DirectoryExists())
                return new string[0];

            // Get the list of code sub directories that will be compiled separately
            CodeSubDirectoriesCollection codeSubDirectories = CompilationUtil.GetCodeSubDirectories();

            // Compute the number of code dirs, including the root one
            int numOfCodeDirs = 1;
            if (codeSubDirectories != null)
                numOfCodeDirs += codeSubDirectories.Count;

            string[] codeDirs = new string[numOfCodeDirs];
            int current = 0;

            if (codeSubDirectories != null) {
                foreach (CodeSubDirectory entry in codeSubDirectories) {

                    VirtualPath virtualSubDir = virtualDir.SimpleCombineWithDir(entry.DirectoryName);
                    codeDirs[current++] = virtualSubDir.VirtualPathString;
                }
            }

            // Add the root code dir at the end of the list (since it's compiled last)
            codeDirs[current++] = virtualDir.VirtualPathString;

            return codeDirs;
        }

        internal void GetCodeDirectoryInformation(VirtualPath virtualCodeDir,
            out Type codeDomProviderType, out CompilerParameters compilerParameters,
            out string generatedFilesDir) {

            // Backup the compilation stage, since the call will modify it
            CompilationStage savedCompilationStage = _compilationStage;

            try {
                GetCodeDirectoryInformationInternal(virtualCodeDir, out codeDomProviderType,
                    out compilerParameters, out generatedFilesDir);
            }
            finally {
                // Restore the compilation stage
                _compilationStage = savedCompilationStage;
            }
        }

        private void GetCodeDirectoryInformationInternal(VirtualPath virtualCodeDir,
            out Type codeDomProviderType, out CompilerParameters compilerParameters,
            out string generatedFilesDir) {

            StringSet excludedSubdirectories = null;

            CodeDirectoryType dirType;

            // Get the DirectoryType based on the path
            if (virtualCodeDir == HttpRuntime.CodeDirectoryVirtualPath) {

                // If it's the top level code directory, make sure we exclude any
                // subdirectories that are compiled separately
                EnsureExcludedCodeSubDirectoriesComputed();

                excludedSubdirectories = _excludedCodeSubdirectories;

                dirType = CodeDirectoryType.MainCode;

                _compilationStage = CompilationStage.TopLevelFiles;
            }
            else if (virtualCodeDir == HttpRuntime.ResourcesDirectoryVirtualPath) {

                dirType = CodeDirectoryType.AppResources;

                _compilationStage = CompilationStage.TopLevelFiles;
            }
            // If virtualCodeDir is a subdir of WebReference virtual path.
            else if (String.Compare(virtualCodeDir.VirtualPathString, 0,
                HttpRuntime.WebRefDirectoryVirtualPath.VirtualPathString, 0, HttpRuntime.WebRefDirectoryVirtualPath.VirtualPathString.Length,
                StringComparison.OrdinalIgnoreCase) == 0) {

                // Use the top WebReference directory info for its sub directories.
                virtualCodeDir = HttpRuntime.WebRefDirectoryVirtualPath;
                dirType = CodeDirectoryType.WebReferences;

                _compilationStage = CompilationStage.TopLevelFiles;
            }
            else if (String.Compare(virtualCodeDir.FileName, HttpRuntime.LocalResourcesDirectoryName,
                StringComparison.OrdinalIgnoreCase) == 0) {

                dirType = CodeDirectoryType.LocalResources;

                // LocalResources are compiled *after* top level files
                _compilationStage = CompilationStage.AfterTopLevelFiles;
            }
            else {
                // If all else fails, treat it as a sub directory
                // 
                dirType = CodeDirectoryType.SubCode;

                // Sub-code dirs are compiled *before* the main code dir
                _compilationStage = CompilationStage.TopLevelFiles;
            }

            Debug.Assert(virtualCodeDir.HasTrailingSlash);
            AssemblyReferenceInfo info = TheBuildManager.TopLevelAssembliesIndexTable[virtualCodeDir.VirtualPathString];
            if (info == null) {
                throw new InvalidOperationException(
                    SR.GetString(SR.Invalid_CodeSubDirectory_Not_Exist, virtualCodeDir));
            }

            // Get the info we need for this code directory
            CodeDirectoryCompiler.GetCodeDirectoryInformation(
                virtualCodeDir, dirType, excludedSubdirectories, info.ReferenceIndex,
                out codeDomProviderType, out compilerParameters,
                out generatedFilesDir);

            Assembly resultAssembly = info.Assembly;

            if (resultAssembly != null) {
                // Use the runtime generated assembly location. VSWhidbey 400335
                compilerParameters.OutputAssembly = resultAssembly.Location;
            }
        }

        internal static Type GetProfileType() {
            return _theBuildManager.GetProfileTypeInternal();
        }

        private Type GetProfileTypeInternal() {
            EnsureTopLevelFilesCompiled();
            return _profileType;
        }


        //
        // Caching related code
        //


        public static ICollection GetVirtualPathDependencies(string virtualPath) {

            CompilationSection compConfig = RuntimeConfig.GetRootWebConfig().Compilation;

            // Create a BuildProvider based on the virtual path
            BuildProvider buildProvider = CreateBuildProvider(VirtualPath.Create(virtualPath), compConfig,
                null, false /*failIfUnknown*/);

            if (buildProvider == null)
                return null;

            // Get its dependencies
            // 
            return buildProvider.GetBuildResultVirtualPathDependencies();
        }

#if OLD
    /*
     * Rewrite the virtualPath if appropriate, in order to support ghosting
     */
    private static void GetGhostedVirtualPath(ref string virtualPath) {

        VirtualPathProvider virtualPathProvider = HostingEnvironment.VirtualPathProvider;

        string ghostedVirtualPath = virtualPathProvider.GetGhostedVirtualPath(virtualPath);

        // If the file is not ghosted, don't change the path
        if (ghostedVirtualPath == null)
            return;

        // 


        // Get the list of virtual paths that it depends on (e.g. user controls)
        ICollection virtualPathDependencies = GetVirtualPathDependencies(virtualPath);

        // If there aren't any, return the ghosted path
        if (virtualPathDependencies == null) {
            virtualPath = ghostedVirtualPath;
            return;
        }

        // Go through all the dependencies, and if we find any that is *not* ghosted
        // (i.e. for which GetGhostedVirtualPath returns null), we treat the whole request
        // as unghosted (and hence we return without modifying the virtualPath).

        foreach (string virtualDependency in virtualPathDependencies) {
            string ghostedVirtualDependencyPath = virtualPathProvider.GetGhostedVirtualPath(
                virtualDependency);
            if (ghostedVirtualDependencyPath == null)
                return;
        }

        // All the dependencies are ghosted, so we can safely use the ghosted path,
        // which can then be shared for all fully ghosted requests.
        virtualPath = ghostedVirtualPath;
    }
#endif

        internal static string GetCacheKeyFromVirtualPath(VirtualPath virtualPath) {
            bool keyFromVPP;
            return GetCacheKeyFromVirtualPath(virtualPath, out keyFromVPP);
        }

        /*
         * Same as GetCacheKeyFromVirtualPathInternal, but caches the cache keys
         * for performance, since creating them is expensive (VSWhidbey 146540)
         */
        static SimpleRecyclingCache _keyCache = new SimpleRecyclingCache();
        private static string GetCacheKeyFromVirtualPath(VirtualPath virtualPath, out bool keyFromVPP) {

            // Check if the VirtualPathProvider needs to use a non-default cache key
            string key = virtualPath.GetCacheKey();

            // If so, just return it
            if (key != null) {
                keyFromVPP = true;
                return key.ToLowerInvariant();
            }

            // The VPP didn't return a key, so use our standard key algorithm
            keyFromVPP = false;

            // Check if the key for this virtual path is already cached
            key = _keyCache[virtualPath.VirtualPathString] as string;
            if (key != null) return key;

            // Compute the key
            key = GetCacheKeyFromVirtualPathInternal(virtualPath);

            // The key should always be lower case
            Debug.Assert(key == key.ToLowerInvariant());

            // Cache it for next time
            _keyCache[virtualPath.VirtualPathString] = key;

            return key;
        }

        /*
         * Generate a unique cache key from a virtual path.  e.g. for "/approot/sub1/sub2/foo.aspx"
         * the key could be "foo.aspx.ccdf220e", where ccdf220e is a hash code from
         * the dir "sub1/sub2".
         */
        private static string GetCacheKeyFromVirtualPathInternal(VirtualPath virtualPath) {

            // We want the key to be app independent (for precompilation), so we
            // change the virtual path to be app relative

            /* Disable assertion since global theme needs to compile theme files in different vroot.
            Debug.Assert(StringUtil.VirtualPathStartsWithAppPath(virtualPath),
                String.Format("VPath {0} is outside the application: {1}", virtualPath, HttpRuntime.AppDomainAppVirtualPath));
            */
            string virtualPathString = virtualPath.AppRelativeVirtualPathString.ToLowerInvariant();
            virtualPathString = UrlPath.RemoveSlashFromPathIfNeeded(virtualPathString);

            // Split the path into the directory and the name
            int slashIndex = virtualPathString.LastIndexOf('/');
            Debug.Assert(slashIndex >= 0 || virtualPathString == "~");

            if (virtualPathString == "~")
                return "root";

            Debug.Assert(slashIndex != virtualPathString.Length - 1);
            string name = virtualPathString.Substring(slashIndex + 1);
            string dir;
            if (slashIndex <= 0)
                dir = "/";
            else {
                dir = virtualPathString.Substring(0, slashIndex);
            }

            return name + "." + StringUtil.GetStringHashCode(dir).ToString("x", CultureInfo.InvariantCulture);
        }

        internal static BuildResult GetVPathBuildResultFromCache(VirtualPath virtualPath) {

            return TheBuildManager.GetVPathBuildResultFromCacheInternal(virtualPath);
        }

        private BuildResult GetVPathBuildResultFromCacheInternal(VirtualPath virtualPath, bool ensureIsUpToDate = true) {
            bool keyFromVPP;
            string cacheKey = GetCacheKeyFromVirtualPath(virtualPath, out keyFromVPP);
            return GetBuildResultFromCacheInternal(cacheKey, keyFromVPP, virtualPath, 0 /*hashCode*/, ensureIsUpToDate);
        }

        internal static BuildResult GetBuildResultFromCache(string cacheKey) {
            return _theBuildManager.GetBuildResultFromCacheInternal(cacheKey, false /*keyFromVPP*/, null /*virtualPath*/,
                0 /*hashCode*/);
        }

        internal static BuildResult GetBuildResultFromCache(string cacheKey, VirtualPath virtualPath) {
            return _theBuildManager.GetBuildResultFromCacheInternal(cacheKey, false /*keyFromVPP*/, virtualPath,
                0 /*hashCode*/);
        }

        private BuildResult GetBuildResultFromCacheInternal(string cacheKey, bool keyFromVPP,
            VirtualPath virtualPath, long hashCode, bool ensureIsUpToDate = true) {

            BuildResult result = null;

            // Allow the possibility that BuildManager was not initialized due to
            // a very early failure (e.g. see VSWhidbey 137366)
            //Debug.Trace("BuildManager", "GetBuildResultFromCacheInternal " + _theBuildManagerInitialized);
            if (!_theBuildManagerInitialized)
                return null;

            // The first cache should always be memory
            Debug.Assert(_caches[0] == _memoryCache);

            // Try to get it from the memeory cache before taking any locks (for perf reasons)
            result = _memoryCache.GetBuildResult(cacheKey, virtualPath, hashCode, ensureIsUpToDate);
            if (result != null) {
                return PostProcessFoundBuildResult(result, keyFromVPP, virtualPath);
            }

            Debug.Trace("BuildManager", "Didn't find '" + virtualPath + "' in memory cache before lock");

            lock (this) {
                // Try to get the BuildResult from the cheapest to most expensive cache
                int i;
                for (i = 0; i < _caches.Length; i++) {
                    result = _caches[i].GetBuildResult(cacheKey, virtualPath, hashCode, ensureIsUpToDate);

                    // There might be changes in local resources for dependencies,
                    // so we need to make sure EnsureFirstTimeDirectoryInit gets called
                    // for them even when we already have a cache result.
                    // VSWhidbey Bug 560521

                    if (result != null) {
                        // We should only process the local resources folder after the top level files have been compiled,
                        // so that any custom VPP can be registered first. (Dev10 bug 890796)
                        if (_compilationStage == CompilationStage.AfterTopLevelFiles && result.VirtualPathDependencies != null) {
                            EnsureFirstTimeDirectoryInitForDependencies(result.VirtualPathDependencies);
                        }

                        break;
                    }

                    // If we didn't find it in the memory cache, perform the per directory
                    // initialization.  This is a good place to do this, because we don't
                    // affect the memory cache code path, but we do the init as soon as
                    // something is not found in the memory cache.
                    if (i == 0 && virtualPath != null) {
                        VirtualPath virtualDir = virtualPath.Parent;
                        EnsureFirstTimeDirectoryInit(virtualDir);
                    }
                }


                if (result == null)
                    return null;

                result = PostProcessFoundBuildResult(result, keyFromVPP, virtualPath);
                if (result == null)
                    return null;

                Debug.Assert(_memoryCache != null);

                // If we found it in a cache, cache it in all the caches that come before
                // the one where we found it.  If we found it in the memory cache, this is a no op.
                for (int j = 0; j < i; j++)
                    _caches[j].CacheBuildResult(cacheKey, result, DateTime.UtcNow);

                Debug.Trace("BuildManager", "Found '" + virtualPath + "' in " + _caches[i]);

                return result;
            }
        }

        private BuildResult PostProcessFoundBuildResult(BuildResult result, bool keyFromVPP, VirtualPath virtualPath) {

            // Check that the virtual path in the result matches the passed in
            // virtualPath (VSWhidbey 516641).  But skip this check in case the key came from
            // calling VirtualPathProvider.GetCacheKey, as it may legitimately not match.
            if (!keyFromVPP) {
                if (virtualPath != null && virtualPath != result.VirtualPath) {
                    Debug.Assert(false);
                    return null;
                }
            }

            // If what we found in the cache is a CompileError, rethrow the exception
            if (result is BuildResultCompileError) {
                // Report the cached error from Callback interface.
                HttpCompileException compileException = ((BuildResultCompileError)result).CompileException;

                // But don't report it if we're doing precompilation, as that would cause it to be
                // reported twice because we always try to compile everything that has failed
                // before (VSWhidbey 525414)
                if (!PerformingPrecompilation) {
                    ReportErrorsFromException(compileException);
                }

                throw compileException;
            }

            return result;
        }

        internal static bool CacheVPathBuildResult(VirtualPath virtualPath,
            BuildResult result, DateTime utcStart) {

            return _theBuildManager.CacheVPathBuildResultInternal(virtualPath, result, utcStart);
        }

        private bool CacheVPathBuildResultInternal(VirtualPath virtualPath,
            BuildResult result, DateTime utcStart) {

            string cacheKey = GetCacheKeyFromVirtualPath(virtualPath);
            return CacheBuildResult(cacheKey, result, utcStart);
        }

        internal static bool CacheBuildResult(string cacheKey, BuildResult result, DateTime utcStart) {
            return _theBuildManager.CacheBuildResultInternal(cacheKey, result, 0 /*hashCode*/, utcStart);
        }

        private bool CacheBuildResultInternal(string cacheKey, BuildResult result,
            long hashCode, DateTime utcStart) {

            // Before caching it, make sure the hash has been computed
            result.EnsureVirtualPathDependenciesHashComputed();

            for (int i = 0; i < _caches.Length; i++) {
                _caches[i].CacheBuildResult(cacheKey, result, hashCode, utcStart);
            }

            // If we find that it's no longer valid after caching it, remove it from the cache (VSWhidbey 578372)
            if (!TimeStampChecker.CheckFilesStillValid(cacheKey, result.VirtualPathDependencies)) {
                _memoryCache.RemoveAssemblyAndCleanupDependencies(result as BuildResultCompiledAssemblyBase);
                return false;
            }

            return true;
        }


        //
        // Precompilation related code
        //

        internal void SetPrecompilationInfo(HostingEnvironmentParameters hostingParameters) {

            if (hostingParameters == null || hostingParameters.ClientBuildManagerParameter == null)
                return;

            _precompilationFlags = hostingParameters.ClientBuildManagerParameter.PrecompilationFlags;

            _strongNameKeyFile = hostingParameters.ClientBuildManagerParameter.StrongNameKeyFile;
            _strongNameKeyContainer = hostingParameters.ClientBuildManagerParameter.StrongNameKeyContainer;

            // Check if we're precompiling to a target directory
            _precompTargetPhysicalDir = hostingParameters.PrecompilationTargetPhysicalDirectory;
            if (_precompTargetPhysicalDir == null)
                return;

            // Check if the target dir already exists and is not empty
            if (Util.IsNonEmptyDirectory(_precompTargetPhysicalDir)) {

                // If it's not empty and OverwriteTarget is off, fail
                if ((_precompilationFlags & PrecompilationFlags.OverwriteTarget) == 0) {
                    throw new HttpException(SR.GetString(SR.Dir_not_empty));
                }

                // Does it contain the precomp marker file
                bool updatable;
                bool precompiled = ReadPrecompMarkerFile(_precompTargetPhysicalDir, out updatable);

                // If not, refuse to delete the directory, even if OverwriteTarget is on (VSWhidbey 425095)
                if (!precompiled) {
                    throw new HttpException(SR.GetString(SR.Dir_not_empty_not_precomp));
                }

                // The OverwriteTarget flag was specified, so delete the directory
                if (!DeletePrecompTargetDirectory()) {
                    // If we failed to delete it, sleep 250 ms and try again, in case there is
                    // an appdomain in the process of shutting down (the shut down would
                    // have been triggered by the first delete attempt)
                    Debug.Trace("BuildManager", "Failed to delete " + _precompTargetPhysicalDir + ".  Sleeping and trying once more...");
                    Thread.Sleep(250);

                    if (!DeletePrecompTargetDirectory()) {
                        Debug.Trace("BuildManager", "Failed to delete " + _precompTargetPhysicalDir + ".  Sleeping and trying once more...");
                        // Try again after 1 second.
                        Thread.Sleep(1000);

                        // If we still couldn't delete it, fail
                        if (!DeletePrecompTargetDirectory()) {
                            throw new HttpException(SR.GetString(SR.Cant_delete_dir));
                        }
                    }
                }
            }

            // Create a marker file to mark the fact that this is a precompiled app
            CreatePrecompMarkerFile();
        }

        private bool DeletePrecompTargetDirectory() {
            try {
                if (_precompTargetPhysicalDir != null) {
                    // Go through all the files in the directory and delete them.
                    foreach (FileData fileData in FileEnumerator.Create(_precompTargetPhysicalDir)) {

                        if (fileData.IsDirectory) {
                            Directory.Delete(fileData.FullName, true /*recursive*/);
                        }
                        else {
                            Util.DeleteFileNoException(fileData.FullName);
                        }
                    }
                }
            }
#if DEBUG
            catch (Exception e) {
                Debug.Trace("BuildManager", "DeletePrecompTargetDirectory failed: " + e.Message);
            }
#else
            catch { }
#endif
            return !Util.IsNonEmptyDirectory(_precompTargetPhysicalDir);
        }

        private void FailIfPrecompiledApp() {

            if (IsPrecompiledApp) {
                throw new HttpException(SR.GetString(SR.Already_precomp));
            }
        }

        internal void PrecompileApp(ClientBuildManagerCallback callback, IEnumerable<string> excludedVirtualPaths) {

            // Remember the original setting
            bool skipTopLevelExceptions = SkipTopLevelCompilationExceptions;

            try {
                _cbmCallback = callback;

                // Don't stop on the first parse errors, process as many errors as possible.
                ThrowOnFirstParseError = false;

                // Don't skip top level compilation exceptions even called by CBM.
                SkipTopLevelCompilationExceptions = false;

                PrecompileApp(HttpRuntime.AppDomainAppVirtualPathObject, excludedVirtualPaths);
            }
            finally {
                // Revert to original setting
                SkipTopLevelCompilationExceptions = skipTopLevelExceptions;
                ThrowOnFirstParseError = true;

                _cbmCallback = null;
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private void PrecompileApp(VirtualPath startingVirtualDir, IEnumerable<string> excludedVirtualPaths) {
            using (new ApplicationImpersonationContext()) {
                try {
                    PerformingPrecompilation = true;

                    PrecompileAppInternal(startingVirtualDir, excludedVirtualPaths);
                }
                catch {
                    // If anything fails during precompilation, wipe out the target to avoid
                    // leaving it in a random state (VSWhidbey 447338)
                    DeletePrecompTargetDirectory();

                    throw;
                }
                finally {
                    PerformingPrecompilation = false;
                }
            }
        }

        private void PrecompileAppInternal(VirtualPath startingVirtualDir, IEnumerable<string> excludedVirtualPaths) {

            // If the app is already precompiled, fail
            FailIfPrecompiledApp();

            excludedVirtualPaths = excludedVirtualPaths ?? Enumerable.Empty<string>();
            _excludedCompilationPaths = excludedVirtualPaths.Select(path => VirtualPath.Create(UrlPath.Combine("~", path))).ToList();

            VirtualDirectory appVdir = startingVirtualDir.GetDirectory();

            EnsureTopLevelFilesCompiled();

            try {
                // Clear the parseError flag first
                _parseErrorReported = false;

                PrecompileWebDirectoriesRecursive(appVdir, topLevel: true);
                PrecompileThemeDirectories();
            }
            catch (HttpParseException parseException) {
                // if nothing calls callback.reportparseerror yet, report the parse error.
                if (!_parseErrorReported) {
                    ReportErrorsFromException(parseException);
                }

                throw;
            }

            // Copy all the DLL's we compiled into the destination's bin directory (if any)
            if (_precompTargetPhysicalDir != null) {
                string targetBinDir = Path.Combine(_precompTargetPhysicalDir, HttpRuntime.BinDirectoryName);
                CopyCompiledAssembliesToDestinationBin(HttpRuntime.CodegenDirInternal, targetBinDir);
            }

            // Copy all the static files to the destination directory (if any).  We treat anything we
            // don't compile as a static file.  It's better to do this at the end of the precompilation,
            // this way if any pages has errors (parse or compile), we never get to this step.
            if (_precompTargetPhysicalDir != null) {
                CopyStaticFilesRecursive(appVdir, _precompTargetPhysicalDir, topLevel: true);
            }
        }

        // Create a small file that marks that app as being precompiled
        private void CreatePrecompMarkerFile() {

            Debug.Assert(PrecompilingForDeployment);

            Directory.CreateDirectory(_precompTargetPhysicalDir);
            string precompMarkerFile = Path.Combine(_precompTargetPhysicalDir, precompMarkerFileName);

            using (StreamWriter writer = new StreamWriter(precompMarkerFile, false /*append*/, Encoding.UTF8)) {
                writer.Write("<precompiledApp version=\"2\" updatable=\"");

                // Write out a flag that determines if the precompiled app is updatable
                if (PrecompilingForUpdatableDeployment)
                    writer.Write("true");
                else
                    writer.Write("false");
                writer.Write("\"/>");
            }
        }

        [SuppressMessage("Microsoft.Security", "MSEC1207:UseXmlReaderForLoad", Justification = "Xml file is created by us and only accessible to admins.")]
        private static bool ReadPrecompMarkerFile(string appRoot, out bool updatable) {

            updatable = false;

            // Get the full physical path to the precompilation market file
            string precompMarkerFile = Path.Combine(appRoot, precompMarkerFileName);

            // If the file doesn't exist at all, it's not a precompiled app
            if (!File.Exists(precompMarkerFile))
                return false;

            XmlDocument doc = new XmlDocument();
            try {
                doc.Load(precompMarkerFile);
            }
            catch {
                // If we fail to read it for any reason, ignore it.
                return false;
            }

            // Get the root element, and make sure it's what we expect
            XmlNode root = doc.DocumentElement;
            Debug.Assert(root != null && root.Name == "precompiledApp");
            if (root == null || root.Name != "precompiledApp")
                return false;

            // Check the updatable flag
            HandlerBase.GetAndRemoveBooleanAttribute(root, "updatable", ref updatable);

            return true;
        }

        /*
         * Are we precompiling the app for deployment (as opposed to in-place)
         */
        internal static bool PrecompilingForDeployment {
            get {
                return (_theBuildManager._precompTargetPhysicalDir != null);
            }
        }

        internal static bool PrecompilingForUpdatableDeployment {
            get {
                // The updatebale mode only applies in deployment precompilation mode
                if (!PrecompilingForDeployment)
                    return false;

                return (_theBuildManager._precompilationFlags & PrecompilationFlags.Updatable) != 0;
            }
        }

        private static bool PrecompilingForCleanBuild {
            get {
                return (_theBuildManager._precompilationFlags & PrecompilationFlags.Clean) != 0;
            }
        }

        internal static bool PrecompilingWithDebugInfo {
            get {
                // The ForceDebug flag only applies in deployment precompilation mode
                if (!PrecompilingForDeployment)
                    return false;

                return (_theBuildManager._precompilationFlags & PrecompilationFlags.ForceDebug) != 0;
            }
        }

        internal static bool PrecompilingWithCodeAnalysisSymbol {
            get {
                return (_theBuildManager._precompilationFlags & PrecompilationFlags.CodeAnalysis) != 0;
            }
        }

        private static bool CompileWithFixedAssemblyNames {
            get {
                return (_theBuildManager._precompilationFlags & PrecompilationFlags.FixedNames) != 0;
            }
        }

        internal static bool CompileWithAllowPartiallyTrustedCallersAttribute {
            get {
                return (_theBuildManager._precompilationFlags & PrecompilationFlags.AllowPartiallyTrustedCallers) != 0;
            }
        }

        internal static bool CompileWithDelaySignAttribute {
            get {
                return (_theBuildManager._precompilationFlags & PrecompilationFlags.DelaySign) != 0;
            }
        }

        internal static bool IgnoreBadImageFormatException {
            get {
                return (_theBuildManager._precompilationFlags & PrecompilationFlags.IgnoreBadImageFormatException) != 0;
            }
        }

        internal static string StrongNameKeyFile {
            get {
                return _theBuildManager._strongNameKeyFile;
            }
        }

        internal static string StrongNameKeyContainer {
            get {
                return _theBuildManager._strongNameKeyContainer;
            }
        }

        // If we're in the process of precompiling for updatable deployment, this returns
        // a writer to the target file specified by the virtual path.  This is used when the
        // deployed file needs to be different from the original (as is the case for aspx files).
        internal static TextWriter GetUpdatableDeploymentTargetWriter(VirtualPath virtualPath, Encoding fileEncoding) {

            Debug.Assert(fileEncoding != null);

            if (!PrecompilingForUpdatableDeployment)
                return null;

            Debug.Assert(!virtualPath.IsRelative);

            string path = virtualPath.AppRelativeVirtualPathString;

            // Skip the "~/" to be left with the relative path
            path = path.Substring(2);

            // Combine it with the precomp target dir to get the full path
            string physicalPath = Path.Combine(_theBuildManager._precompTargetPhysicalDir, path);

            // Before trying to create the file, make sure the directory exists
            string physicalDir = Path.GetDirectoryName(physicalPath);
            Directory.CreateDirectory(physicalDir);

            return new StreamWriter(physicalPath, false /*append*/, fileEncoding);
        }

        private bool IsPrecompiledAppInternal {
            get {
                if (!_isPrecompiledAppComputed) {
                    _isPrecompiledApp = ReadPrecompMarkerFile(HttpRuntime.AppDomainAppPathInternal,
                        out _isUpdatablePrecompiledApp);

                    _isPrecompiledAppComputed = true;
                }

                return _isPrecompiledApp;
            }
        }

        public static bool IsPrecompiledApp {
            get {
                return _theBuildManager.IsPrecompiledAppInternal;
            }
        }

        private bool IsUpdatablePrecompiledAppInternal {
            get {
                return IsPrecompiledApp && _isUpdatablePrecompiledApp;
            }
        }

        public static bool IsUpdatablePrecompiledApp {
            get {
                return _theBuildManager.IsUpdatablePrecompiledAppInternal;
            }
        }

        private bool IsNonUpdatablePrecompiledApp {
            get {
                return IsPrecompiledApp && !_isUpdatablePrecompiledApp;
            }
        }

        private bool IsExcludedFromPrecompilation(VirtualDirectory dir) {
            Debug.Assert(dir != null);
            return _excludedCompilationPaths.Any(path => UrlPath.IsEqualOrSubpath(path.VirtualPathString, dir.VirtualPath));
        }

        private void PrecompileWebDirectoriesRecursive(VirtualDirectory vdir, bool topLevel) {

            // Precompile the children directory

            foreach (VirtualDirectory childVdir in vdir.Directories) {

                if (topLevel && _excludedTopLevelDirectories.Contains(childVdir.Name))
                    continue;

                // Exclude the special FrontPage directory (VSWhidbey 116727, 518602)
                if (childVdir.Name == "_vti_cnf")
                    continue;

                // Exclude target directory in precompilation scenarios
                if (SourceDirectoryIsInPrecompilationDestination(childVdir)) {
                    continue;
                }

                if (IsExcludedFromPrecompilation(childVdir)) {
                    continue;
                }

                PrecompileWebDirectoriesRecursive(childVdir, topLevel: false);
            }

            // Precompile this directory
            try {
                // Set a flag to remember that we're in the process of precompiling.  This
                // way, if BatchCompileWebDirectory ends up getting called again recursively
                // via CompileWebFile, we know that we cannot ignore errors.
                _precompilingApp = true;

                if (IsBatchEnabledForDirectory(vdir.VirtualPathObject)) {
                    // batch everything if enabled
                    BatchCompileWebDirectory(vdir, virtualDir: null, ignoreErrors: false);
                }
                else {
                    // if batching is disabled, compile each web file individually.
                    NonBatchDirectoryCompiler dirCompiler = new NonBatchDirectoryCompiler(vdir);
                    dirCompiler.Process();
                }
            }
            finally {
                // Always restore the flag to false when we're done.
                _precompilingApp = false;
            }
        }

        private void PrecompileThemeDirectories() {
            string appPhysicalDir = Path.Combine(HttpRuntime.AppDomainAppPathInternal, HttpRuntime.ThemesDirectoryName);

            if (Directory.Exists(appPhysicalDir)) {
                string[] themeDirs = Directory.GetDirectories(appPhysicalDir);

                foreach (string themeDirPath in themeDirs) {
                    string themeDirName = Path.GetFileName(themeDirPath);
                    ThemeDirectoryCompiler.GetThemeBuildResultType(null /*context*/, themeDirName);
                }
            }
        }

        /*
         * Recursively copy all the static files from the source directory to the
         * target directory of the precompilation
         */
        private void CopyStaticFilesRecursive(VirtualDirectory sourceVdir, string destPhysicalDir,
            bool topLevel) {

            // Make sure the target physical dir has no relation with the source.  It's important to
            // check at every new directory, because IIS apps can have disconnected virtual sub dirs,
            // making an app root check insufficient (VSWhidbey 426251)
            if (SourceDirectoryIsInPrecompilationDestination(sourceVdir)) {
                return;
            }

            if (IsExcludedFromPrecompilation(sourceVdir)) {
                return;
            }

            bool directoryCreationAttempted = false;

            foreach (VirtualFileBase child in sourceVdir.Children) {

                string destPhysicalSubDir = Path.Combine(destPhysicalDir, child.Name);

                if (child.IsDirectory) {

                    // Skip the special top level directories, since they never contain relevant
                    // static files.  Note that we don't skip Themes, which does contain static files.
                    if (topLevel &&
                        (StringUtil.EqualsIgnoreCase(child.Name, HttpRuntime.CodeDirectoryName) ||
                        StringUtil.EqualsIgnoreCase(child.Name, HttpRuntime.ResourcesDirectoryName) ||
                        StringUtil.EqualsIgnoreCase(child.Name, HttpRuntime.WebRefDirectoryName))) {

                        continue;
                    }

                    // Also, skip the LocalResources directory at any level, except when precompiling
                    // for updatable deployment (in which case, we deploy the local resources file)
                    if (!PrecompilingForUpdatableDeployment && StringUtil.EqualsIgnoreCase(child.Name,
                        HttpRuntime.LocalResourcesDirectoryName)) {
                        continue;
                    }

                    CopyStaticFilesRecursive(child as VirtualDirectory, destPhysicalSubDir, topLevel: false);
                    continue;
                }

                // Create the destination directory if needed
                if (!directoryCreationAttempted) {
                    directoryCreationAttempted = true;
                    Directory.CreateDirectory(destPhysicalDir);
                }

                // Copy the file as appropriate based on its extension
                CopyPrecompiledFile(child as VirtualFile, destPhysicalSubDir);
            }
        }

        /*
         * Copy all the assemblies from the codegen dir into the bin directory of the
         * target precompiled app.
         */
        private void CopyCompiledAssembliesToDestinationBin(string fromDir, string toDir) {

            bool createdDirectory = false;

            foreach (FileData fileData in FileEnumerator.Create(fromDir)) {
                // Windows OS Bug 1981578
                // Create a new directory only if there is something in the directory.
                if (!createdDirectory)
                    Directory.CreateDirectory(toDir);
                createdDirectory = true;

                // Recurse on subdirectories.if they contain culture files
                if (fileData.IsDirectory) {

                    if (Util.IsCultureName(fileData.Name)) {
                        string fromSubDir = Path.Combine(fromDir, fileData.Name);
                        string toSubDir = Path.Combine(toDir, fileData.Name);
                        CopyCompiledAssembliesToDestinationBin(fromSubDir, toSubDir);
                    }

                    continue;
                }

                // Only process DLL's and PDB's
                string extension = Path.GetExtension(fileData.Name);
                if (extension != ".dll" && extension != ".pdb")
                    continue;

                // Do not copy the file to the target folder if it has been already
                // marked for deletion - Dev10 bug 676794
                if (DiskBuildResultCache.HasDotDeleteFile(fileData.FullName)) {
                    continue;
                }

                string sourcePhysicalPath = Path.Combine(fromDir, fileData.Name);
                string destPhysicalPath = Path.Combine(toDir, fileData.Name);

                // Copy the file to the destination
                // 
                File.Copy(sourcePhysicalPath, destPhysicalPath, true /*overwrite*/);
            }
        }

        // Copy one file from the source app to the precompiled app
        private void CopyPrecompiledFile(VirtualFile vfile, string destPhysicalPath) {

            bool createStub;

            if (CompilationUtil.NeedToCopyFile(vfile.VirtualPathObject, PrecompilingForUpdatableDeployment,
                out createStub)) {

                // 
                string sourcePhysicalPath = HostingEnvironment.MapPathInternal(vfile.VirtualPath);

                // The file could already exist with updatable precompilation, since we would create the modified file
                // earlier during processing of a code beside page.
                if (File.Exists(destPhysicalPath)) {

                    // In that case, we still need to fix it up to insert the correct type string in the
                    // inherits attribute (VSWhidbey 467936)

                    // First, get the just-compiled BuildResult.  It should always exist
                    BuildResultCompiledType result = GetVPathBuildResult(null, vfile.VirtualPathObject,
                        true /*noBuild*/, false /*allowCrossApp*/) as BuildResultCompiledType;
                    Debug.Assert(result != null);

                    // VSWhidbey 527299. Need to use the same encoding of the original file to
                    // read and write to the new file.
                    Encoding encoding = Util.GetEncodingFromConfigPath(vfile.VirtualPathObject);

                    // Read in the file
                    string newAspxFile = Util.StringFromFile(destPhysicalPath, ref encoding);

                    // Replace the placeholder token by the true type with the assembly
                    newAspxFile = newAspxFile.Replace(UpdatableInheritReplacementToken,
                        Util.GetAssemblyQualifiedTypeName(result.ResultType));

                    // Write the modified file back with the correct inherits type string
                    StreamWriter writer = new StreamWriter(destPhysicalPath, false /* append */, encoding);
                    writer.Write(newAspxFile);
                    writer.Close();
                }
                else {
                    // Just copy the file to the destination
                    File.Copy(sourcePhysicalPath, destPhysicalPath, false /*overwrite*/);
                }

                // If it has a readonly attribute, clear it on the destination (VSWhidbey 122359)
                Util.ClearReadOnlyAttribute(destPhysicalPath);
            }
            else {
                if (createStub) {
                    // Create the stub file, with a helpful static message
                    StreamWriter writer = new StreamWriter(destPhysicalPath);
                    writer.Write(SR.GetString(SR.Precomp_stub_file));
                    writer.Close();
                }
            }
        }

        // Make sure the target physical dir has no relation with the source. Return true if it does.
        private bool SourceDirectoryIsInPrecompilationDestination(VirtualDirectory sourceDir) {
            // Alwasy return false for in-place precompilations or non-precompilation scenarios.
            if (_precompTargetPhysicalDir == null) {
                return false;
            }

            string sourcePhysicalDir = HostingEnvironment.MapPathInternal(sourceDir.VirtualPath);

            // Make sure they're normalized and end with a '\' before comparing (VSWhidbey 452554)
            sourcePhysicalDir = FileUtil.FixUpPhysicalDirectory(sourcePhysicalDir);
            string destPhysicalDir = FileUtil.FixUpPhysicalDirectory(_precompTargetPhysicalDir);

            return StringUtil.StringStartsWithIgnoreCase(sourcePhysicalDir, destPhysicalDir);
        }

        internal static void ReportDirectoryCompilationProgress(VirtualPath virtualDir) {

            // Nothing to do if there is no CBM callback
            if (CBMCallback == null)
                return;

            // Don't report anything if the directory doesn't exist
            if (!virtualDir.DirectoryExists())
                return;

            string message = SR.GetString(SR.Directory_progress, virtualDir.VirtualPathString);
            CBMCallback.ReportProgress(message);
        }


        //
        // Public methods
        //


        /// <devdoc>
        ///     Compiles a file given its virtual path, using the appropriate BuildProvider (based
        ///     on the file's extension).  The compiled type is returned.
        ///     This methods performs both memory and disk caching of the compiled Type.
        /// </devdoc>
        public static Type GetCompiledType(string virtualPath) {
            if (virtualPath == null) {
                throw new ArgumentNullException("virtualPath");
            }

            return GetCompiledType(VirtualPath.Create(virtualPath));
        }

        // This method is called by BuildManagerHost thru CBM
        internal static Type GetCompiledType(VirtualPath virtualPath, ClientBuildManagerCallback callback) {
            // Remember the original setting
            bool skipTopLevelExceptions = SkipTopLevelCompilationExceptions;
            bool throwOnFirstParseError = ThrowOnFirstParseError;

            try {
                // Don't skip top level compilation exceptions even called by CBM.
                SkipTopLevelCompilationExceptions = false;

                // Don't stop on the first parse error, process as many errors as possible.
                ThrowOnFirstParseError = false;

                _theBuildManager._cbmCallback = callback;
                return GetCompiledType(virtualPath);
            }
            finally {
                _theBuildManager._cbmCallback = null;

                // Revert to original setting
                SkipTopLevelCompilationExceptions = skipTopLevelExceptions;

                ThrowOnFirstParseError = throwOnFirstParseError;
            }
        }

        internal static Type GetCompiledType(VirtualPath virtualPath) {
            ITypedWebObjectFactory factory = GetVirtualPathObjectFactory(virtualPath,
                null /*context*/, false /*allowCrossApp*/);

            BuildResultCompiledType resultType = factory as BuildResultCompiledType;
            if (resultType == null) return null;

            return resultType.ResultType;
        }

        /// Process a file based on its virtual path, and instantiate the result.  This API works for both
        /// compiled and no compile pages.  requiredBaseType specifies a type from which the resulting
        /// object must derive.  If it doesn't, the API fails without instantiating the object.
        public static object CreateInstanceFromVirtualPath(string virtualPath, Type requiredBaseType) {
            VirtualPath virtualPathObject = VirtualPath.CreateNonRelative(virtualPath);
            return CreateInstanceFromVirtualPath(virtualPathObject, requiredBaseType,
                null /*context*/, false /*allowCrossApp*/);
        }

        /// <devdoc>
        ///     Process a file given its virtual path, using the appropriate BuildProvider (based
        ///     on the file's extension).  The result is then instantiated and returned.
        /// </devdoc>
        internal static object CreateInstanceFromVirtualPath(VirtualPath virtualPath,
            Type requiredBaseType, HttpContext context, bool allowCrossApp) {

            ITypedWebObjectFactory objectFactory = GetVirtualPathObjectFactory(virtualPath, context, allowCrossApp);
            if (objectFactory == null) return null;

            // Make sure it has the required base type (VSWhidbey 516771)
            Util.CheckAssignableType(requiredBaseType, objectFactory.InstantiatedType);

            // impersonate client while executing page ctor (see ASURT 89712)
            // (compilation is done while not impersonating client)

            Object instance;
            using (new ClientImpersonationContext(context)) {
                instance = objectFactory.CreateInstance();
            }

            return instance;
        }

        public static IWebObjectFactory GetObjectFactory(string virtualPath, bool throwIfNotFound) {
            ITypedWebObjectFactory factory = GetVirtualPathObjectFactory(VirtualPath.Create(virtualPath),
                null /*context*/, false /*allowCrossApp*/, throwIfNotFound);
            return factory;
        }

        private static ITypedWebObjectFactory GetVirtualPathObjectFactory(VirtualPath virtualPath,
            HttpContext context, bool allowCrossApp) {
            return GetVirtualPathObjectFactory(virtualPath, context, allowCrossApp, true /*throwIfNotFound*/);
        }

        /// <devdoc>
        ///     Process a file given its virtual path, using the appropriate BuildProvider (based
        ///     on the file's extension).  The ITypedWebObjectFactory is returned.
        ///     This methods performs both memory and disk caching of the compiled Type.
        /// </devdoc>
        private static ITypedWebObjectFactory GetVirtualPathObjectFactory(VirtualPath virtualPath,
            HttpContext context, bool allowCrossApp, bool throwIfNotFound) {

            if (virtualPath == null)
                throw new ArgumentNullException("virtualPath");

            // Throw here immediately if top level exception exists.
            // This is because EnsureTopLevelFilesCompiled (where the exception is thrown)
            // might not be called.
            if (_theBuildManager._topLevelFileCompilationException != null) {
                _theBuildManager.ReportTopLevelCompilationException();
            }

            ITypedWebObjectFactory objectFactory;
            BuildResult buildResult;

            // We need to assert here since there may be user code on the stack,
            // and code may demand UnmanagedCode permission.  But if we're in full trust,
            // or noAssert is true, skip the assert for perf reasons (VSWhidbey 146871, 500699)
            //
            // In regard to previous comment, in v2/3.5 we only needed to assert when we were
            // running in partial trust and user code was on the stack.  In v4, we need to assert
            // whenever we are running in partial turst, because the AppDomain is homogenous.
            if (HttpRuntime.IsFullTrust) {
                buildResult = GetVPathBuildResultWithNoAssert(
                    context, virtualPath, false /*noBuild*/, allowCrossApp, false /*allowBuildInPrecompile*/, throwIfNotFound);
            }
            else {
                buildResult = GetVPathBuildResultWithAssert(
                    context, virtualPath, false /*noBuild*/, allowCrossApp, false /*allowBuildInPrecompile*/, throwIfNotFound);
            }

            // DevDiv 67952
            // The returned build result may not always be castable to ITypedWebObjectFactory.
            objectFactory = buildResult as ITypedWebObjectFactory;

            return objectFactory;
        }

        /// <devdoc>
        ///     Compiles a file given its virtual path, using the appropriate BuildProvider (based
        ///     on the file's extension).  The compiled assembly is returned.
        ///     This methods performs both memory and disk caching of the compiled assembly.
        /// </devdoc>
        public static Assembly GetCompiledAssembly(string virtualPath) {

            BuildResult result = GetVPathBuildResult(VirtualPath.Create(virtualPath));
            if (result == null) return null;

            BuildResultCompiledAssemblyBase resultAssembly = result as BuildResultCompiledAssemblyBase;
            if (resultAssembly == null) return null;

            return resultAssembly.ResultAssembly;
        }


        /// <devdoc>
        ///     Compiles a file given its virtual path, using the appropriate BuildProvider (based
        ///     on the file's extension).  If the BuildProvider chose to persist a custom
        ///     string, the string is returned.
        ///     This methods performs both memory and disk caching.
        /// </devdoc>
        public static string GetCompiledCustomString(string virtualPath) {

            BuildResult result = GetVPathBuildResult(VirtualPath.Create(virtualPath));
            if (result == null) return null;

            BuildResultCustomString resultCustomString = result as BuildResultCustomString;
            if (resultCustomString == null) return null;

            return resultCustomString.CustomString;
        }

        /// <devdoc>
        ///     Returns the BuildDependencySet for the passed in virtualPath, assuming
        ///     that information is cached.  Otherwise, return null.
        /// </devdoc>
        public static BuildDependencySet GetCachedBuildDependencySet(
            HttpContext context, string virtualPath) {
            return GetCachedBuildDependencySet(context, virtualPath, ensureIsUpToDate: true);
        }

        public static BuildDependencySet GetCachedBuildDependencySet(
            HttpContext context, string virtualPath, bool ensureIsUpToDate) {

            BuildResult result = GetVPathBuildResult(context, VirtualPath.Create(virtualPath),
                true /*noBuild*/, false /*allowCrossApp*/, allowBuildInPrecompile: false, ensureIsUpToDate: ensureIsUpToDate);

            // If it's not cached, return null
            if (result == null)
                return null;

            // We found it in the cache.  Wrap it with a BuildDependencySet object.
            return new BuildDependencySet(result);
        }

        /// <summary>
        /// Returns the target framework moniker for the current web site. For framework versions less than
        /// 4.0, it will either be 3.0 or 3.5. 2.0 and 3.0 have similar web.config, so we use 3.0 to allow 3.0
        /// web sites to reference 3.0 assemblies.
        /// </summary>
        public static FrameworkName TargetFramework {
            get {
                return MultiTargetingUtil.TargetFrameworkName;
            }
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs e) {

            if (_assemblyResolveMapping == null)
                return null;

            string name = e.Name;
            Assembly assembly = (Assembly)_assemblyResolveMapping[name];

            // Return the assembly if we have it in our mapping (VSWhidbey 276776)
            if (assembly != null) {
                return assembly;
            }

            // Get the normalized assembly name from random name (VSWhidbey 380793)
            String normalizedName = GetNormalizedCodeAssemblyName(name);
            if (normalizedName != null) {
                return (Assembly)_assemblyResolveMapping[normalizedName];
            }

            return null;
        }

        internal static string GetNormalizedCodeAssemblyName(string assemblyName) {
            // Return the main code assembly.
            if (assemblyName.StartsWith(CodeDirectoryAssemblyName, StringComparison.Ordinal)) {
                return CodeDirectoryAssemblyName;
            }

            // Check the sub code directories.
            CodeSubDirectoriesCollection codeSubDirectories = CompilationUtil.GetCodeSubDirectories();
            foreach (CodeSubDirectory directory in codeSubDirectories) {
                if (assemblyName.StartsWith(SubCodeDirectoryAssemblyNamePrefix + directory.AssemblyName + ".", StringComparison.Ordinal)) {
                    return directory.AssemblyName;
                }
            }

            return null;
        }

        internal static string GetNormalizedTypeName(Type t) {
            string assemblyFullName = t.Assembly.FullName;
            string normalizedCodeAssemblyName = GetNormalizedCodeAssemblyName(assemblyFullName);
            if (normalizedCodeAssemblyName == null) {
                return t.AssemblyQualifiedName;
            }

            string normalizedTypeName = t.FullName + ", " + normalizedCodeAssemblyName;
            return normalizedTypeName;
        }

        /// <summary>
        /// Temporary subdirectory under the codegen folder for buildproviders to generate embedded resource files.
        /// </summary>
        internal static string CodegenResourceDir {
            get {
                string resxDir = _theBuildManager._codegenResourceDir;
                if (resxDir == null) {
                    resxDir = Path.Combine(HttpRuntime.CodegenDirInternal, CodegenResourceDirectoryName);
                    _theBuildManager._codegenResourceDir = resxDir;
                }
                return resxDir;
            }
        }

        // The Use Cache lives under the codegen folder
        private static string _userCachePath;
        private static string UserCachePath {
            get {
                if (_userCachePath == null) {
                    // Build the full path to the User Cache folder
                    string userCachePath = Path.Combine(HttpRuntime.CodegenDirInternal, "UserCache");

                    // Create it if it doesn't exist
                    if (!Directory.Exists(userCachePath)) {
                        Directory.CreateDirectory(userCachePath);
                    }

                    _userCachePath = userCachePath;
                }

                return _userCachePath;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly",
            Justification = "Too late in the loc process to add an exception message.")]
        private static string GetUserCacheFilePath(string fileName) {
            string path = Path.Combine(UserCachePath, fileName);

            // Make sure that the full path's directory is exactly the User Cache folder. This prevents creating files in any other folders
            if (Path.GetDirectoryName(path) != UserCachePath) {
                throw new ArgumentException();
            }

            return path;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity",
            Justification = "This is the correct Assert for the situation.")]
        public static Stream CreateCachedFile(string fileName) {
            new FileIOPermission(FileIOPermissionAccess.AllAccess, HttpRuntime.CodegenDirInternal).Assert();

            // Get the path to the file in the User Cache folder
            string path = GetUserCacheFilePath(fileName);

            return File.Create(path);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity",
            Justification = "This is the correct Assert for the situation.")]
        public static Stream ReadCachedFile(string fileName) {
            new FileIOPermission(FileIOPermissionAccess.AllAccess, HttpRuntime.CodegenDirInternal).Assert();

            // Get the path to the file in the User Cache folder
            string path = GetUserCacheFilePath(fileName);

            // If the file doesn't exist, just return null, to convey a cache miss
            if (!File.Exists(path))
                return null;

            return File.OpenRead(path);
        }
    }

    internal enum CompilationStage {
        PreTopLevelFiles = 0,       // Before EnsureTopLevelFilesCompiled() is called
        TopLevelFiles = 1,          // In EnsureTopLevelFilesCompiled() but before building global.asax
        GlobalAsax = 2,             // While building global.asax
        BrowserCapabilities = 3,    // While building browserCap
        AfterTopLevelFiles = 4      // After EnsureTopLevelFilesCompiled() is called
    }

    internal enum PreStartInitStage {
        BeforePreStartInit,
        DuringPreStartInit,
        AfterPreStartInit,
    }

    internal class AssemblyReferenceInfo {
        internal Assembly Assembly;
        internal int ReferenceIndex;

        internal AssemblyReferenceInfo(int referenceIndex) {
            ReferenceIndex = referenceIndex;
        }
    }

}
