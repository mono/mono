//------------------------------------------------------------------------------
// <copyright file="BuildResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



/*********************************

BuildResult
    BuildResultCompileError
    BuildResultCompiledAssemblyBase
        BuildResultCompiledAssembly
            BuildResultCustomString
            BuildResultMainCodeAssembly
            BuildResultResourceAssembly
        BuildResultCompiledType
            BuildResultCompiledTemplateType
            BuildResultCompiledGlobalAsaxType
            ImageGeneratorBuildResultCompiledType
    BuildResultNoCompileTemplateControl
        BuildResultNoCompilePage
        BuildResultNoCompileUserControl
            BuildResultNoCompileMasterPage
    BuildResultCodeCompileUnit

**********************************/

namespace System.Web.Compilation {

using System;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Util;
using System.Web.UI;
using System.Web.Configuration;

internal enum BuildResultTypeCode {
    Invalid=-1,
    BuildResultCompiledAssembly = 1,
    BuildResultCompiledType = 2,
    BuildResultCompiledTemplateType = 3,
    BuildResultCustomString = 5,
    BuildResultMainCodeAssembly = 6,
    BuildResultCodeCompileUnit = 7,
    BuildResultCompiledGlobalAsaxType = 8,
    BuildResultResourceAssembly = 9,
}

internal abstract class BuildResult {

    // const masks into the BitVector32
    // The 16 lower bits come from the BuildProviderResultFlags enumeration
    // and should not be used here.  They are set from calling
    // BuildProvider.GetResultFlags.
    protected const int usesCacheDependency         = 0x00010000;
    protected const int usesExistingAssembly        = 0x00020000;
    private const int noMemoryCache                 = 0x00040000;
    protected const int hasAppOrSessionObjects      = 0x00080000;
    protected const int dependenciesHashComputed    = 0x00100000;
    #pragma warning disable 0649
    protected SimpleBitVector32 _flags;
    #pragma warning restore 0649

    internal static BuildResult CreateBuildResultFromCode(BuildResultTypeCode code,
        VirtualPath virtualPath) {

        BuildResult ret = null;

        switch (code) {
            case BuildResultTypeCode.BuildResultCompiledAssembly:
                ret = new BuildResultCompiledAssembly();
                break;

            case BuildResultTypeCode.BuildResultCompiledType:
                ret = new BuildResultCompiledType();
                break;

            case BuildResultTypeCode.BuildResultCompiledTemplateType:
                ret = new BuildResultCompiledTemplateType();
                break;

            case BuildResultTypeCode.BuildResultCompiledGlobalAsaxType:
                ret = new BuildResultCompiledGlobalAsaxType();
                break;

            case BuildResultTypeCode.BuildResultCustomString:
                ret = new BuildResultCustomString();
                break;

            case BuildResultTypeCode.BuildResultMainCodeAssembly:
                ret = new BuildResultMainCodeAssembly();
                break;

            case BuildResultTypeCode.BuildResultResourceAssembly:
                ret = new BuildResultResourceAssembly();
                break;

            case BuildResultTypeCode.BuildResultCodeCompileUnit:
                ret = new BuildResultCodeCompileUnit();
                break;

            default:
                Debug.Assert(false, "code=" + code);
                return null;
        }

        ret.VirtualPath = virtualPath;

        // Set _nextUpToDateCheck to MinValue, to make sure the next call to IsUpToDate()
        // actually makes the check
        ret._nextUpToDateCheck = DateTime.MinValue;

        return ret;
    }

    internal virtual BuildResultTypeCode GetCode() { return BuildResultTypeCode.Invalid; }

    internal int Flags {
        get { return _flags.IntegerValue; }
        set { _flags.IntegerValue = value; }
    }

    private VirtualPath _virtualPath;
    internal VirtualPath VirtualPath {
        get { return _virtualPath; }
        set { _virtualPath = value; }
    }

    // Are the BuildResult's VirtualPathDependencies being monitored by a CacheDependency.
    // If so, then we don't need to check validity after finding the BuildResult in the
    // memory cache (since it would have been kicked out if it was invalid).
    internal bool UsesCacheDependency {
        get { return _flags[usesCacheDependency]; }
        set { _flags[usesCacheDependency] = value; }
    }

    // Does the appdomain need to be shut down when this item becomes invalid?
    internal bool ShutdownAppDomainOnChange {
        get { return _flags[(int)BuildProviderResultFlags.ShutdownAppDomainOnChange]; }
    }

    // The list of files (virtual paths) it depends on (for caching purpose)
    private ArrayList _virtualPathDependencies;
    internal ICollection VirtualPathDependencies {
        get { return _virtualPathDependencies; }
    }

    // Hash code based on all the source file dependencies
    private string _virtualPathDependenciesHash;
    internal string VirtualPathDependenciesHash {
        get {
            EnsureVirtualPathDependenciesHashComputed();

            return _virtualPathDependenciesHash;
        }

        set {
            Debug.Assert(_virtualPathDependenciesHash == null);
            _virtualPathDependenciesHash = value;
        }
    }

    internal bool DependenciesHashComputed {
        get { return _flags[dependenciesHashComputed]; }
    }

    internal void EnsureVirtualPathDependenciesHashComputed() {

        if (!DependenciesHashComputed) {

            // We shouldn't already have a hash
            Debug.Assert(_virtualPathDependenciesHash == null);

            // Sort the source dependencies to make the hash code predictable
            if (_virtualPathDependencies != null)
                _virtualPathDependencies.Sort(InvariantComparer.Default);

            _virtualPathDependenciesHash = ComputeSourceDependenciesHashCode(null /*virtualPath*/);

            // It's computed, but it could be null
            _flags[dependenciesHashComputed] = true;
        }
    }

    // These fields are used to make sure we only check the UpToDate status
    // of the build result once every few seconds (since it's expensive)
    private DateTime _nextUpToDateCheck = DateTime.Now.AddSeconds(UpdateInterval);
    private int _lock;
    private const int UpdateInterval = 2;   // 2 seconds


    internal void SetVirtualPathDependencies(ArrayList sourceDependencies) {

        Debug.Assert(_virtualPathDependencies == null);
        Debug.Assert(sourceDependencies != null);

        _virtualPathDependencies = sourceDependencies;
    }

    internal void AddVirtualPathDependencies(ICollection sourceDependencies) {

        if (sourceDependencies == null)
            return;

        if (_virtualPathDependencies == null) {
            _virtualPathDependencies = new ArrayList(sourceDependencies);
        }
        else {
            _virtualPathDependencies.AddRange(sourceDependencies);
        }
    }

    /*
     * Can the result be unloaded from memory.  Most objects can, but things like
     * Assemblies and Types can't.  This is used to determine the caching behavior.
     */
    internal virtual bool IsUnloadable { get { return true; } }

    /*
     * Should the result be cached to disk.  Usually yes, but for things like compile
     * errors, we only cache them to memory.
     */
    internal virtual bool CacheToDisk { get { return true; } }

    /*
     * Should the result be cached to memory.  Usually yes, but for things like top level
     * assemblies, we only cache them to disk.
     */
    internal bool CacheToMemory {
        get { return !_flags[noMemoryCache]; }
        set { _flags[noMemoryCache] = !value; }
    }

    /*
     * Time the build result should expire from the memory cache
     */
    internal virtual DateTime MemoryCacheExpiration {
        get {
            return Cache.NoAbsoluteExpiration;
        }
    }

    /*
     * Sliding expiration for the build result
     */
    internal virtual TimeSpan MemoryCacheSlidingExpiration {
        get {
            return Cache.NoSlidingExpiration;
        }
    }

    protected void ReadPreservedFlags(PreservationFileReader pfr) {
        string s = pfr.GetAttribute("flags");
        if ((s != null) && (s.Length != 0)) {
            Flags = Int32.Parse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }
    }

    internal virtual void GetPreservedAttributes(PreservationFileReader pfr) {
        ReadPreservedFlags(pfr);
    }

    internal virtual void SetPreservedAttributes(PreservationFileWriter pfw) {
        if (Flags != 0) {
            pfw.SetAttribute("flags", Flags.ToString("x", CultureInfo.InvariantCulture));
        }
    }

    /*
     * Tell the BuildResult that its dependencies are not up to date, in order
     * to give it a chance to do some cleanup.
     */
    internal virtual void RemoveOutOfDateResources(PreservationFileReader pfw) {}

    // Compute the current hash code of the preserved data.  Return 0 if the
    // hash code is not valid.
    internal long ComputeHashCode(long hashCode) {
        return ComputeHashCode(hashCode, 0);
    }

    internal long ComputeHashCode(long hashCode1, long hashCode2) {
        HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();

        // If a hashcode was passed in, start with it
        if (hashCode1 != 0)
            hashCodeCombiner.AddObject(hashCode1);
        if (hashCode2 != 0)
            hashCodeCombiner.AddObject(hashCode2);

        ComputeHashCode(hashCodeCombiner);

        return hashCodeCombiner.CombinedHash;
    }

    /*
     * Compute the hash code of what this buid result depends on, excluding
     * the virtual path dependencies (which are handled separately by
     * VirtualPathDependenciesHash).
     */
    protected virtual void ComputeHashCode(HashCodeCombiner hashCodeCombiner) {

    }

    internal virtual string ComputeSourceDependenciesHashCode(VirtualPath virtualPath) {
        // Return an empty string if there are no dependencies.  This is different from
        // null, which means 'don't cache'
        if (VirtualPathDependencies == null)
            return String.Empty;

        // If no virtual path was passed in, use the one from the BuildResult
        if (virtualPath == null)
            virtualPath = VirtualPath;

        return virtualPath.GetFileHash(VirtualPathDependencies);
    }

    internal bool IsUpToDate(VirtualPath virtualPath, bool ensureIsUpToDate) {

        if (!ensureIsUpToDate) {
            return true;
        }

        // This should never be called on a BuildResult that has already been
        // determined to be out of date.
        Debug.Assert(_lock >= 0);
        if (_lock < 0)
            return false;

        // Don't check more than every two seconds
        DateTime now = DateTime.Now;
        // Due to bug 214038, CBM can be called multiple times in a very short time.
        if (now < _nextUpToDateCheck && !BuildManagerHost.InClientBuildManager) {
            Debug.Trace("BuildResult", "IsUpToDate: true since called less than 2 seconds ago. "
                + _nextUpToDateCheck + "," + now);
            return true;
        }

        // If we don't get the lock, just say it's up to date without checking
        if (Interlocked.CompareExchange(ref _lock, 1, 0) != 0) {
            Debug.Trace("BuildResult", "IsUpToDate returning true because it didn't get the lock");
            return true;
        }

        string newHashCode;

        try {
            newHashCode = ComputeSourceDependenciesHashCode(virtualPath);
        }
        catch {
            // Make sure to release the lock if something throws.
            Interlocked.Exchange(ref _lock, 0);
            throw;
        }

        // Check if we're up to date.  A null hash code means the cache should not be used.
        if (newHashCode == null || newHashCode != _virtualPathDependenciesHash) {
            Debug.Trace("BuildResult", "IsUpToDate: '" + VirtualPath + "' is out of date");

            // Set the lock to -1 to mark that we're not up to date
            _lock = -1;
            return false;
        }

        Debug.Trace("BuildResult", "IsUpToDate: '" + VirtualPath + "' is up to date");

        // We're up to date.  Remember the time we checked, and reset the lock
        _nextUpToDateCheck = now.AddSeconds(UpdateInterval);
        Interlocked.Exchange(ref _lock, 0);

        return true;
    }

}

internal class BuildResultCompileError: BuildResult {

    // The exception in case we cached the result of a failed compilation
    private HttpCompileException _compileException;
    internal HttpCompileException CompileException { get { return _compileException; } }

    internal BuildResultCompileError(VirtualPath virtualPath, HttpCompileException compileException) {
        VirtualPath = virtualPath;
        _compileException = compileException;
    }

    /*
     * Don't cache compile errors to disk
     */
    internal override bool CacheToDisk { get { return false; } }

    internal override DateTime MemoryCacheExpiration {
        get {
            // Only cache compile errors for 10 seconds.  This is to get us out of trouble
            // if the compilation fails due to some strange timing issue, and might succeed
            // on retry (VSWhidbey 483169)
            return DateTime.UtcNow.AddSeconds(10);
        }
    }
}

internal class BuildResultCustomString: BuildResultCompiledAssembly {

    private string _customString;

    internal BuildResultCustomString() {}

    internal BuildResultCustomString(Assembly a, string customString) : base(a) {
        Debug.Assert(customString != null);
        _customString = customString;
    }

    internal override BuildResultTypeCode GetCode() {
        return BuildResultTypeCode.BuildResultCustomString; }

    internal override void GetPreservedAttributes(PreservationFileReader pfr) {
        base.GetPreservedAttributes(pfr);

        // Retrieve the custom string
        _customString = pfr.GetAttribute("customString");
        Debug.Assert(_customString != null);
    }

    internal override void SetPreservedAttributes(PreservationFileWriter pfw) {
        base.SetPreservedAttributes(pfw);

        // Preserve the custom string
        pfw.SetAttribute("customString", _customString);
    }

    internal string CustomString {
        get { return _customString; }
    }

}

internal abstract class BuildResultCompiledAssemblyBase: BuildResult {

    internal bool UsesExistingAssembly {
        get { return _flags[usesExistingAssembly]; }
        set { _flags[usesExistingAssembly] = value; }
    }

    // Assemblies are *not* unloadable, so only allow the build result to be unloaded
    // if there is no assembly
    internal override bool IsUnloadable { get { return (ResultAssembly == null); } }

    internal abstract Assembly ResultAssembly { get; set; }
    internal virtual bool HasResultAssembly { get { return ResultAssembly != null; } }
    protected virtual bool IsGacAssembly { get { return ResultAssembly.GlobalAssemblyCache; } }
    protected virtual string ShortAssemblyName { get { return ResultAssembly.GetName().Name; } }

    static private string s_codegenDir = null;

    internal static Assembly GetPreservedAssembly(PreservationFileReader pfr) {
        string assemblyName = pfr.GetAttribute("assembly");

        if (assemblyName == null)
            return null;

        // Try to load the assembly
        try {
            Assembly a = Assembly.Load(assemblyName);

            // VSWhidbey 564168
            // Do not load assemblies or assemblies with references that 
            // do not exist or are marked for deletion

            // It is possible that Assembly.Load succeeds, even though the
            // underlying DLL was renamed (to .delete).  In that case, we should
            // not return the assembly, as we would be unable to compile with
            // a reference to it.
            
            if (AssemblyIsInvalid(a)) {
                // Throw some exception, since the caller doesn't expect null
                throw new InvalidOperationException();
            }

            // Check references of the assembly, and make sure they exists, 
            // otherwise throw an exception.
            CheckAssemblyIsValid(a, new Hashtable());

            return a;
        }
        catch {
            Debug.Trace("BuildResult", "GetPreservedAssembly: couldn't load assembly '" + assemblyName + "'; deleting associated files.");

            // Remove the assembly and all the associated files
            pfr.DiskCache.RemoveAssemblyAndRelatedFiles(assemblyName);
            throw;
        }
    }

    // DevDiv Bug 98735
    // Go through the assembly and all references (including deeper levels) to make sure that
    // each referenced assembly exists and does not have a dot delete.
    // If any referenced assembly is removed or marked for deletion,
    // we invalidate the base assembly by throwing an InvalidOperationException
    private static void CheckAssemblyIsValid(Assembly a, Hashtable checkedAssemblies) {

        // Keep track of which assemblies we already checked so we can skip them
        checkedAssemblies.Add(a, null);

        foreach (AssemblyName aName in a.GetReferencedAssemblies()) {
            Assembly referencedAssembly = Assembly.Load(aName);

            // If it is in the GAC, skip checking it
            if (referencedAssembly.GlobalAssemblyCache)
                continue;

            // Do not validate assemblies other than those we generate.
            // If the assembly is NOT in the codegen folder, skip it
            if (!AssemblyIsInCodegenDir(referencedAssembly))
                continue;

            // If we have already checked an assembly, don't check it again
            if (!checkedAssemblies.Contains(referencedAssembly)) {
                if (AssemblyIsInvalid(referencedAssembly))
                    throw new InvalidOperationException();

                // Visit nested referenced assemblies
                CheckAssemblyIsValid(referencedAssembly, checkedAssemblies);
            }
        }
    }

    internal static bool AssemblyIsInCodegenDir(Assembly a) {
        string path = Util.GetAssemblyCodeBase(a);
        FileInfo f = new FileInfo(path);
        string assemblyDir = FileUtil.RemoveTrailingDirectoryBackSlash(f.Directory.FullName);
        if (s_codegenDir == null) {
            s_codegenDir = FileUtil.RemoveTrailingDirectoryBackSlash(HttpRuntime.CodegenDir);
        }

        // check if the assembly is directly under codegen
        // Shadow-copied assemblies are in a deeper directory (eg myapp\zzz\yyy\assembly\dl3\xxxx)
        if (string.Equals(assemblyDir, s_codegenDir, StringComparison.OrdinalIgnoreCase))
            return true;
        
        return false;
    }

    private static bool AssemblyIsInvalid(Assembly a) {
        // If the file does not exist, or if it has a .delete file,
        // then it should not be used
        string path = Util.GetAssemblyCodeBase(a);
        return (!FileUtil.FileExists(path) || DiskBuildResultCache.HasDotDeleteFile(path));
    }


    internal override void SetPreservedAttributes(PreservationFileWriter pfw) {
        base.SetPreservedAttributes(pfw);

        if (HasResultAssembly) {
            string assemblyName;
            if (IsGacAssembly) {
                // If it's in the GAC, store the full name (VSWhidbey 384416)
                assemblyName = ResultAssembly.FullName;
            }
            else {
                // Otherwise, store the short name, to avoid uselessly growing the preservation file
                assemblyName = ShortAssemblyName;
            }
            pfw.SetAttribute("assembly", assemblyName);
        }
    }

    /*
     * Tell the BuildResult that its dependencies are not up to date, in order
     * to give it a chance to do some cleanup.
     */
    internal override void RemoveOutOfDateResources(PreservationFileReader pfr) {

        // If the preservation file is pointing to an assembly that was not built
        // for this result, do not attempt to clean it up (see VSWhidbey 74094)
        ReadPreservedFlags(pfr);
        if (UsesExistingAssembly)
            return;

        // Remove the assembly and all the associated files
        string assemblyName = pfr.GetAttribute("assembly");
        if (assemblyName != null) {
            pfr.DiskCache.RemoveAssemblyAndRelatedFiles(assemblyName);
        }
    }

    protected override void ComputeHashCode(HashCodeCombiner hashCodeCombiner) {

        base.ComputeHashCode(hashCodeCombiner);

        // Make the hash code depend on the relevant contents of the <compilation> config section

        CompilationSection compConfig = MTConfigUtil.GetCompilationConfig(VirtualPath);

        hashCodeCombiner.AddObject(compConfig.RecompilationHash);
    }
}

internal class BuildResultCompiledAssembly: BuildResultCompiledAssemblyBase {

    private Assembly _assembly;

    internal BuildResultCompiledAssembly() {}

    internal BuildResultCompiledAssembly(Assembly a) {
        _assembly = a;
    }

    internal override BuildResultTypeCode GetCode() { return BuildResultTypeCode.BuildResultCompiledAssembly; }

    internal override Assembly ResultAssembly {
        get { return _assembly; }
        set { _assembly = value; }
    }

    internal override void GetPreservedAttributes(PreservationFileReader pfr) {
        base.GetPreservedAttributes(pfr);

        ResultAssembly = GetPreservedAssembly(pfr);
    }
}

/*
 * Same as BuildResultCompiledAssembly, but with some special behavior specific to
 * the main code assembly.  Specifically, it adds support for the AppInitialize method
 * and for VB's My.*
 */
internal class BuildResultMainCodeAssembly: BuildResultCompiledAssembly {

    private const string appInitializeMethodName = "AppInitialize";

    private MethodInfo _appInitializeMethod;

    internal BuildResultMainCodeAssembly() {}

    internal BuildResultMainCodeAssembly(Assembly a) : base(a) {

        // Look for an AppInitialize static method in the assembly
        FindAppInitializeMethod();
    }

    internal override BuildResultTypeCode GetCode() { return BuildResultTypeCode.BuildResultMainCodeAssembly; }

    internal override void GetPreservedAttributes(PreservationFileReader pfr) {
        base.GetPreservedAttributes(pfr);

        // Does the assembly have an AppInitialize method?
        string appInitializeClass = pfr.GetAttribute("appInitializeClass");
        if (appInitializeClass != null) {

            // Get the Type that contains the method
            Type appInitializeType = ResultAssembly.GetType(appInitializeClass);
            Debug.Assert(appInitializeType != null);

            // Find the method
            _appInitializeMethod = FindAppInitializeMethod(appInitializeType);
            Debug.Assert(_appInitializeMethod != null);
        }
    }

    internal override void SetPreservedAttributes(PreservationFileWriter pfw) {

        base.SetPreservedAttributes(pfw);

        // If there is an AppInitialize method, save the class name that it's in
        if (_appInitializeMethod != null) {
            pfw.SetAttribute("appInitializeClass", _appInitializeMethod.ReflectedType.FullName);
        }
    }

    private void FindAppInitializeMethod() {

        Debug.Assert(_appInitializeMethod == null);

        // Look in all the public types in the assembly
        foreach (Type t in ResultAssembly.GetExportedTypes()) {

            // Look for an AppInitialize method
            MethodInfo tmpAppInitializeMethod = FindAppInitializeMethod(t);

            if (tmpAppInitializeMethod != null) {

                // Make sure we didn't already have one
                if (_appInitializeMethod != null) {
                    throw new HttpException(SR.GetString(SR.Duplicate_appinitialize, _appInitializeMethod.ReflectedType.FullName, t.FullName));
                }

                // Keep track of the method
                _appInitializeMethod = tmpAppInitializeMethod;
            }
        }
    }

    private MethodInfo FindAppInitializeMethod(Type t) {

        return t.GetMethod(appInitializeMethodName,
            BindingFlags.Public | BindingFlags.Static| BindingFlags.IgnoreCase,
            null /*Binder*/,
            new Type[0], // Method with no parameters
            null
            );
    }

    // Call the AppInitialize method if there is one
    internal void CallAppInitializeMethod() {
        if (_appInitializeMethod != null) {
            using (new ApplicationImpersonationContext()) {
                using (HostingEnvironment.SetCultures()) {
                    _appInitializeMethod.Invoke(null, null);
                }
            }
        }
    }
}

/*
 * Same as BuildResultCompiledAssembly, but with some special behavior specific to
 * resources directory (both global and local)
 */
internal class BuildResultResourceAssembly : BuildResultCompiledAssembly {
    internal BuildResultResourceAssembly() { }

    internal BuildResultResourceAssembly(Assembly a) : base(a) { }

    internal override BuildResultTypeCode GetCode() { return BuildResultTypeCode.BuildResultResourceAssembly; }

    internal override string ComputeSourceDependenciesHashCode(VirtualPath virtualPath) {

        // If no virtual path was passed in, use the one from the BuildResult
        if (virtualPath == null)
            virtualPath = VirtualPath;

        // We don't want to use the default ComputeSourceDependenciesHashCode imnplementation,
        // as it would use all files in the resources dir to calculate the hash.  Instead,
        // we only want the hash the be based on the culture neutral resources, so that
        // changes to culture specific files don't cause a rebuild of the main res assembly
        HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
        hashCodeCombiner.AddResourcesDirectory(virtualPath.MapPathInternal());
        return hashCodeCombiner.CombinedHashString;
    }

    // In addition to the standard BuildResult hash code (which drives recompilation of the main
    // resources assembly), we need an additional one so we know when to rebuild satellites.
    private string _resourcesDependenciesHash;
    internal string ResourcesDependenciesHash {
        get {
            EnsureResourcesDependenciesHashComputed();

            return _resourcesDependenciesHash;
        }

        set {
            Debug.Assert(_resourcesDependenciesHash == null);
            _resourcesDependenciesHash = value;
            Debug.Assert(_resourcesDependenciesHash != null);
        }
    }

    private void EnsureResourcesDependenciesHashComputed() {
        if (_resourcesDependenciesHash != null)
            return;

        // Even though we make it dependent on all res files, if we get here we know the neutral
        // ones are up to date, so effectively it's look the culture specific that matter.
        _resourcesDependenciesHash = HashCodeCombiner.GetDirectoryHash(VirtualPath);
    }

    internal override void GetPreservedAttributes(PreservationFileReader pfr) {
        base.GetPreservedAttributes(pfr);

        ResourcesDependenciesHash = pfr.GetAttribute("resHash");
    }

    internal override void SetPreservedAttributes(PreservationFileWriter pfw) {
        base.SetPreservedAttributes(pfw);

        pfw.SetAttribute("resHash", ResourcesDependenciesHash);
    }

}

internal class BuildResultCompiledType : BuildResultCompiledAssemblyBase, ITypedWebObjectFactory {

    // The delegate for fast object instantiation
    private InstantiateObject _instObj;
    private bool _triedToGetInstObj;

    internal BuildResultCompiledType() {}

    internal BuildResultCompiledType(Type t) {
        _builtType = t;
    }

    internal override BuildResultTypeCode GetCode() { return BuildResultTypeCode.BuildResultCompiledType; }

    internal override Assembly ResultAssembly {
        get { return _builtType.Assembly; }
        set { Debug.Assert(false); }
    }

    internal override bool HasResultAssembly { get { return _builtType != null; } }

    protected override bool IsGacAssembly {
        get {
            if (IsDelayLoadType) {
                return false;
            }
            else {
                return base.IsGacAssembly;
            }
        }
    }

    protected override string ShortAssemblyName {
        get {
            var delayLoadType = ResultType as DelayLoadType;
            if (delayLoadType != null) {
                return delayLoadType.AssemblyName;
            }
            else {
                return base.ShortAssemblyName;
            }
        }
    }

    private Type _builtType;
    internal Type ResultType {
        get { return _builtType; }
        set { _builtType = value; }
    }

    private string FullResultTypeName {
        get {
            var delayLoadType = ResultType as DelayLoadType;
            if (delayLoadType != null) {
                return delayLoadType.TypeName;
            }
            else {
                return ResultType.FullName;
            }
        }
    }

    internal bool IsDelayLoadType { get { return ResultType is DelayLoadType; } }

    static internal bool UsesDelayLoadType(BuildResult result) {
        BuildResultCompiledType buildResultCompiledType = result as BuildResultCompiledType;
        if (buildResultCompiledType != null) {
            return buildResultCompiledType.IsDelayLoadType;
        }
        else {
            return false;
        }
    }

    // IWebObjectFactory.CreateInstance
    public object CreateInstance() {

        // Get the fast object creation delegate on demand
        if (!_triedToGetInstObj) {
            _instObj = ObjectFactoryCodeDomTreeGenerator.GetFastObjectCreationDelegate(ResultType);
            _triedToGetInstObj = true;
        }

        // If the fast factory is not available, just call CreateInstance
        // 
        if (_instObj == null) {
            return HttpRuntime.CreatePublicInstance(ResultType);
        }

        // Call it to instantiate the object
        return _instObj();
    }

    // ITypedWebObjectFactory.CreateInstance
    public virtual Type InstantiatedType {
        get { return ResultType; }
    }

    protected override void ComputeHashCode(HashCodeCombiner hashCodeCombiner) {

        base.ComputeHashCode(hashCodeCombiner);

        // Make pages have a dependency on the main local resources assembly, so that they
        // get recompiled when it changes (but not when satellites change). VSWhidbey 277357
        if (VirtualPath != null) {

            // Remove the file name to get its directory
            VirtualPath virtualDir = VirtualPath.Parent;

            Assembly localResAssembly = BuildManager.GetLocalResourcesAssembly(virtualDir);

            if (localResAssembly != null) {
                hashCodeCombiner.AddFile(localResAssembly.Location);
            }
        }
    }

    internal override void GetPreservedAttributes(PreservationFileReader pfr) {
        base.GetPreservedAttributes(pfr);

        // Get the assembly and type
        Assembly a = GetPreservedAssembly(pfr);
        Debug.Assert(a != null);
        string typeName = pfr.GetAttribute("type");
        ResultType = a.GetType(typeName, true /*throwOnError*/);
    }

    internal override void SetPreservedAttributes(PreservationFileWriter pfw) {
        base.SetPreservedAttributes(pfw);
        pfw.SetAttribute("type", FullResultTypeName);
    }
}

/*
 * Used for pages, user controls, and master pages
 */
internal class BuildResultCompiledTemplateType: BuildResultCompiledType {

    public BuildResultCompiledTemplateType() {}

    public BuildResultCompiledTemplateType(Type t) : base(t) {}

    internal override BuildResultTypeCode GetCode() { return BuildResultTypeCode.BuildResultCompiledTemplateType; }

    protected override void ComputeHashCode(HashCodeCombiner hashCodeCombiner) {

        base.ComputeHashCode(hashCodeCombiner);

        // Make the hash code depend on the relevant contents of the <pages> config section

        PagesSection pagesConfig = MTConfigUtil.GetPagesConfig(VirtualPath);
        hashCodeCombiner.AddObject(Util.GetRecompilationHash(pagesConfig));
    }
}

/*
 * Used for global.asax
 */
internal class BuildResultCompiledGlobalAsaxType : BuildResultCompiledType {

    public BuildResultCompiledGlobalAsaxType() { }

    public BuildResultCompiledGlobalAsaxType(Type t) : base(t) { }

    internal override BuildResultTypeCode GetCode() { return BuildResultTypeCode.BuildResultCompiledGlobalAsaxType; }

    // Does global.asax contain <object> tags with application or session scope
    internal bool HasAppOrSessionObjects {
        get { return _flags[hasAppOrSessionObjects]; }
        set { _flags[hasAppOrSessionObjects] = value; }
    }
}

internal abstract class BuildResultNoCompileTemplateControl : BuildResult, ITypedWebObjectFactory {

    protected Type _baseType;
    protected RootBuilder _rootBuilder;
    protected bool _initialized;

    internal BuildResultNoCompileTemplateControl(Type baseType, TemplateParser parser) {
        _baseType = baseType;
        _rootBuilder = parser.RootBuilder;

        // Cleanup anything that's no longer needed in the ControlBuilder
        _rootBuilder.PrepareNoCompilePageSupport();
    }

    internal override BuildResultTypeCode GetCode() {
        Debug.Assert(false, "BuildResultNoCompileTemplateControl");
        return BuildResultTypeCode.Invalid;
    }

    /*
     * Don't cache the result of no-compile pages to disk (they are reparsed in each appdomain)
     */
    internal override bool CacheToDisk { get { return false; } }

    /*
     * Give a 5 minute sliding expiration to no-compile pages
     */
    internal override TimeSpan MemoryCacheSlidingExpiration {
        get {
            return TimeSpan.FromMinutes(5);
        }
    }

    // Note that since this is a no-compile control, this is not really the 'base' type,
    // but is in fact the Type we directly instantiate.
    internal Type BaseType {
        get { return _baseType; }
    }

    // IWebObjectFactory.CreateInstance
    public virtual object CreateInstance() {

        // Create the object that the aspx/ascx 'inherits' from
        TemplateControl templateControl = (TemplateControl) HttpRuntime.FastCreatePublicInstance(_baseType);

        // Set the virtual path and TemplateSourceDirectory in the control
        templateControl.TemplateControlVirtualPath = VirtualPath;
        templateControl.TemplateControlVirtualDirectory = VirtualPath.Parent;

        // Give the TemplateControl a pointer to us, so it can call us back during FrameworkInitialize
        templateControl.SetNoCompileBuildResult(this);

        return templateControl;
    }

    // ITypedWebObjectFactory.CreateInstance
    public virtual Type InstantiatedType {
        get { return _baseType; }
    }

    internal virtual void FrameworkInitialize(TemplateControl templateControl) {

        HttpContext context = HttpContext.Current;

        // Storing the filter resolution service and template control into the context
        // since each thread needs to set them differently.
        TemplateControl savedTemplateControl = context.TemplateControl;
        context.TemplateControl = templateControl;

        try {
            // Create the control tree

            // DevDiv Bug 59351
            // Lock during the first time we initialize the control builder with the object,
            // to prevent concurrency issues.
            if (!_initialized) {
                lock (this) {
                    _rootBuilder.InitObject(templateControl);
                }
                _initialized = true;
            }
            else {
                _rootBuilder.InitObject(templateControl);
            }
        }
        finally {
            // Restore the previous template control
            if (savedTemplateControl != null)
                context.TemplateControl = savedTemplateControl;
        }
    }
}

internal class BuildResultNoCompilePage: BuildResultNoCompileTemplateControl {

    private TraceEnable _traceEnabled;
    private TraceMode _traceMode;

    private OutputCacheParameters _outputCacheData;
    private string[] _fileDependencies;

    private bool _validateRequest;
    private string _stylesheetTheme;

    internal BuildResultNoCompilePage(Type baseType, TemplateParser parser)
        : base(baseType, parser) {

        PageParser pageParser = (PageParser) parser;

        //
        // Keep track of relevant info from the parser
        //

        _traceEnabled = pageParser.TraceEnabled;
        _traceMode = pageParser.TraceMode;

        if (pageParser.OutputCacheParameters != null) {
            _outputCacheData = pageParser.OutputCacheParameters;

            // If we're not supposed to cache it, clear out the field
            if (_outputCacheData.Duration == 0 || _outputCacheData.Location == OutputCacheLocation.None) {
                _outputCacheData = null;
            }
            else {
                // Since we're going to be output caching, remember all the dependencies
                _fileDependencies = new string[pageParser.SourceDependencies.Count];
                int i = 0;
                foreach (string dependency in pageParser.SourceDependencies) {
                    _fileDependencies[i++] = dependency;
                }
                Debug.Assert(i == pageParser.SourceDependencies.Count);
            }
        }

        _validateRequest = pageParser.ValidateRequest;
        _stylesheetTheme = pageParser.StyleSheetTheme;
    }

    internal override void FrameworkInitialize(TemplateControl templateControl) {
        Page page = (Page)templateControl;
        page.StyleSheetTheme = _stylesheetTheme;

        page.InitializeStyleSheet();

        base.FrameworkInitialize(templateControl);

        if (_traceEnabled != TraceEnable.Default)
            page.TraceEnabled = (_traceEnabled == TraceEnable.Enable);
        if (_traceMode != TraceMode.Default)
            page.TraceModeValue = _traceMode;

        if (_outputCacheData != null) {
            page.AddWrappedFileDependencies(_fileDependencies);
            page.InitOutputCache(_outputCacheData);
        }

        if (_validateRequest) {
            page.Request.ValidateInput();
        }
        else if(MultiTargetingUtil.TargetFrameworkVersion >= VersionUtil.Framework45) {
            // Only set the ValidateRequestMode property if we are targetting 4.5 or higher
            // as earlier versions did not have it.
            page.ValidateRequestMode = ValidateRequestMode.Disabled;
        }
    }
}

internal class BuildResultNoCompileUserControl: BuildResultNoCompileTemplateControl {

    private PartialCachingAttribute _cachingAttribute;

    internal BuildResultNoCompileUserControl(Type baseType, TemplateParser parser)
        : base(baseType, parser) {

        UserControlParser ucParser = (UserControlParser) parser;
        OutputCacheParameters cacheSettings = ucParser.OutputCacheParameters;

        // If the user control has an OutputCache directive, create
        // a PartialCachingAttribute with the information about it.
        if (cacheSettings != null && cacheSettings.Duration > 0) {
            _cachingAttribute = new PartialCachingAttribute(
                cacheSettings.Duration,
                cacheSettings.VaryByParam,
                cacheSettings.VaryByControl,
                cacheSettings.VaryByCustom,
                cacheSettings.SqlDependency,
                ucParser.FSharedPartialCaching);
            _cachingAttribute.ProviderName = ucParser.Provider;
        }
    }

    internal PartialCachingAttribute CachingAttribute {
        get { return _cachingAttribute; }
    }
}

internal class BuildResultNoCompileMasterPage: BuildResultNoCompileUserControl {

    private ICollection _placeHolderList;

    internal BuildResultNoCompileMasterPage(Type baseType, TemplateParser parser)
        : base(baseType, parser) {
        _placeHolderList = ((MasterPageParser)parser).PlaceHolderList;
    }

    // IWebObjectFactory.CreateInstance
    public override object CreateInstance() {

        // Create the master page object that the master 'inherits' from
        MasterPage masterPage = (MasterPage) base.CreateInstance();

        foreach(string placeHolderID in _placeHolderList) {
            masterPage.ContentPlaceHolders.Add(placeHolderID.ToLower(CultureInfo.InvariantCulture));
        }

        return masterPage;
    }
}

/*
* This class is used to cache the generated codecompileunit for CBM scenarios,
* when cached on disk, it uses BinaryFormatter to serialize the codecompileunit
* and other compile params.
*/
internal class BuildResultCodeCompileUnit : BuildResult {
    private Type _codeDomProviderType;
    private CodeCompileUnit _codeCompileUnit;
    private CompilerParameters _compilerParameters;
    private IDictionary _linePragmasTable;
    private string _cacheKey;

    private const string fileNameAttribute = "CCUpreservationFileName";

    internal BuildResultCodeCompileUnit() {
    }

    internal BuildResultCodeCompileUnit(
        Type codeDomProviderType, CodeCompileUnit codeCompileUnit,
        CompilerParameters compilerParameters, IDictionary linePragmasTable) {

        _codeDomProviderType = codeDomProviderType;
        _codeCompileUnit = codeCompileUnit;
        _compilerParameters = compilerParameters;
        _linePragmasTable = linePragmasTable;
    }

    internal Type CodeDomProviderType {
        get { return _codeDomProviderType; }
    }

    internal CodeCompileUnit CodeCompileUnit {
        get { return _codeCompileUnit; }
    }

    internal CompilerParameters CompilerParameters {
        get { return _compilerParameters; }
    }

    internal IDictionary LinePragmasTable {
        get { return _linePragmasTable; }
    }

    internal override bool CacheToDisk { get { return true; } }

    internal override BuildResultTypeCode GetCode() {
        return BuildResultTypeCode.BuildResultCodeCompileUnit;
    }

    private string GetPreservationFileName() {
        return _cacheKey + ".ccu";
    }

    protected override void ComputeHashCode(HashCodeCombiner hashCodeCombiner) {

        base.ComputeHashCode(hashCodeCombiner);

        // Make the hash code depend on the relevant contents of the <page> and <compilation> config sections
        CompilationSection compConfig = MTConfigUtil.GetCompilationConfig(VirtualPath);

        hashCodeCombiner.AddObject(compConfig.RecompilationHash);

        PagesSection pagesConfig = MTConfigUtil.GetPagesConfig(VirtualPath);
        hashCodeCombiner.AddObject(Util.GetRecompilationHash(pagesConfig));
    }

    internal override void GetPreservedAttributes(PreservationFileReader pfr) {
        base.GetPreservedAttributes(pfr);

        String _ccuPreservationFileName = pfr.GetAttribute(fileNameAttribute);
        _ccuPreservationFileName = Path.Combine(HttpRuntime.CodegenDirInternal, _ccuPreservationFileName);

        Debug.Assert(FileUtil.FileExists(_ccuPreservationFileName), _ccuPreservationFileName);

        using (FileStream stream = File.Open(_ccuPreservationFileName, FileMode.Open)) {
            BinaryFormatter formatter = new BinaryFormatter();

            _codeCompileUnit = formatter.Deserialize(stream) as CodeCompileUnit;
            _codeDomProviderType = (Type)formatter.Deserialize(stream);
            _compilerParameters = (CompilerParameters)formatter.Deserialize(stream);
            _linePragmasTable = formatter.Deserialize(stream) as IDictionary;
        }
    }

    internal void SetCacheKey(string cacheKey) {
        _cacheKey = cacheKey;
    }

    internal override void SetPreservedAttributes(PreservationFileWriter pfw) {
        base.SetPreservedAttributes(pfw);
        string preservationFileName = GetPreservationFileName();

        pfw.SetAttribute(fileNameAttribute, preservationFileName);
        preservationFileName = Path.Combine(HttpRuntime.CodegenDirInternal, preservationFileName);

        using (FileStream stream = File.Open(preservationFileName, FileMode.Create)) {
            BinaryFormatter formatter = new BinaryFormatter();

            if (_codeCompileUnit != null) {
                formatter.Serialize(stream, _codeCompileUnit);
            }
            else {
                formatter.Serialize(stream, new object());
            }

            formatter.Serialize(stream, _codeDomProviderType);
            formatter.Serialize(stream, _compilerParameters);

            if (_linePragmasTable != null) {
                formatter.Serialize(stream, _linePragmasTable);
            }
            else {
                formatter.Serialize(stream, new object());
            }
        }
    }

    internal override void RemoveOutOfDateResources(PreservationFileReader pfr) {
        // Remove the out-of-date .ccu file
        String ccuPreservationFileName = pfr.GetAttribute(fileNameAttribute);
        ccuPreservationFileName = Path.Combine(HttpRuntime.CodegenDirInternal, ccuPreservationFileName);

        File.Delete(ccuPreservationFileName);
    }
}

}
