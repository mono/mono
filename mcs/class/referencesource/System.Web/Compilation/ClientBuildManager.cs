//------------------------------------------------------------------------------
// <copyright file="ClientBuildManager.cs" company="Microsoft">
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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Util;
using Debug = System.Web.Util.Debug;


// Flags that drive the behavior of precompilation
[Flags]
public enum PrecompilationFlags {

    Default = 0x00000000,

    // determines whether the deployed app will be updatable
    Updatable = 0x00000001,

    // determines whether the target directory can be overwritten
    OverwriteTarget = 0x00000002,

    // determines whether the compiler will emit debug information
    ForceDebug = 0x00000004,

    // determines whether the application is built clean
    Clean = 0x00000008,

    // determines whether the /define:CodeAnalysis flag needs to be added
    // as compilation symbol
    CodeAnalysis = 0x00000010,

    // determines whether to generate APTCA attribute.
    AllowPartiallyTrustedCallers = 0x00000020,

    // determines whether to delaySign the generate assemblies.
    DelaySign = 0x00000040,

    // determines whether to use fixed assembly names
    FixedNames = 0x00000080,

    // determines whether to skip BadImageFormatException
    IgnoreBadImageFormatException = 0x00000100,
}

[Serializable]
public class ClientBuildManagerParameter {
    private string _strongNameKeyFile;
    private string _strongNameKeyContainer;
    private PrecompilationFlags _precompilationFlags = PrecompilationFlags.Default;
    private List<string> _excludedVirtualPaths;

    public List<string> ExcludedVirtualPaths {
        get {
            if (_excludedVirtualPaths == null) {
                _excludedVirtualPaths = new List<string>();
            }
            return _excludedVirtualPaths;
        }
    }

    // Determines the behavior of the precompilation
    public PrecompilationFlags PrecompilationFlags {
        get { return _precompilationFlags; }
        set { _precompilationFlags = value; }
    }

    public string StrongNameKeyFile {
        get { return _strongNameKeyFile; }
        set { _strongNameKeyFile = value; }
    }

    public string StrongNameKeyContainer {
        get { return _strongNameKeyContainer; }
        set { _strongNameKeyContainer = value; }
    }
}

//
// This class provide access to the BuildManager outside of an IIS environment
// Instances of this class are created in the caller's App Domain.
//
// It creates and configures the new App Domain for handling BuildManager calls
// using System.Web.Hosting.ApplicationHost.CreateApplicationHost()
//

[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public sealed class ClientBuildManager : MarshalByRefObject, IDisposable {

    private VirtualPath _virtualPath;
    private string _physicalPath;
    private string _installPath;
    private string _appId;
    private IApplicationHost _appHost;
    private string _codeGenDir;

    private HostingEnvironmentParameters _hostingParameters;
    private ClientBuildManagerTypeDescriptionProviderBridge _cbmTdpBridge;

    private WaitCallback _onAppDomainUnloadedCallback;
    private WaitCallback _onAppDomainShutdown;
    private ApplicationShutdownReason _reason;

    private BuildManagerHost _host;
    private Exception _hostCreationException;
    private bool _hostCreationPending;

    public event BuildManagerHostUnloadEventHandler AppDomainUnloaded;

    public event EventHandler AppDomainStarted;

    public event BuildManagerHostUnloadEventHandler AppDomainShutdown;
    // internal lock used for host creation.
    private object _lock = new object();

    // Whether to wait for the call back from the previous host unloading before creating a new one
    private bool _waitForCallBack; 

    private const string IISExpressPrefix = "/IISExpress/";

    /*
     * Creates an instance of the ClientBuildManager.
     * appPhysicalSourceDir points to the physical root of the application (e.g "c:\myapp")
     * virtualPath is the virtual path to the app root. It can be anything (e.g. "/dummy"),
     *      but ideally it should match the path later given to Cassini, in order for
     *      compilation that happens here to be reused there.
     */

    public ClientBuildManager(string appVirtualDir, string appPhysicalSourceDir) : 
        this(appVirtualDir, appPhysicalSourceDir,
        appPhysicalTargetDir: null, parameter: null) {
    }

    /*
     * Creates an instance of the PrecompilationManager.
     * appPhysicalSourceDir points to the physical root of the application (e.g "c:\myapp")
     * appVirtualDir is the virtual path to the app root. It can be anything (e.g. "/dummy"),
     *      but ideally it should match the path later given to Cassini, in order for
     *      compilation that happens here to be reused there.
     * appPhysicalTargetDir is the directory where the precompiled site is placed
     */
    public ClientBuildManager(string appVirtualDir, string appPhysicalSourceDir,
        string appPhysicalTargetDir) : this(appVirtualDir, appPhysicalSourceDir,
            appPhysicalTargetDir, parameter: null) {
    }

    /*
     * Creates an instance of the PrecompilationManager.
     * appPhysicalSourceDir points to the physical root of the application (e.g "c:\myapp")
     * appVirtualDir is the virtual path to the app root. It can be anything (e.g. "/dummy"),
     *      but ideally it should match the path later given to Cassini, in order for
     *      compilation that happens here to be reused there.
     * appPhysicalTargetDir is the directory where the precompiled site is placed
     * flags determines the behavior of the precompilation
     */
    public ClientBuildManager(string appVirtualDir, string appPhysicalSourceDir,
        string appPhysicalTargetDir, ClientBuildManagerParameter parameter) :
        this(appVirtualDir, appPhysicalSourceDir,
            appPhysicalTargetDir, parameter, typeDescriptionProvider: null) {
    }

    /*
     * Creates an instance of the PrecompilationManager.
     * appPhysicalSourceDir points to the physical root of the application (e.g "c:\myapp")
     * appVirtualDir is the virtual path to the app root. It can be anything (e.g. "/dummy"),
     *      but ideally it should match the path later given to Cassini, in order for
     *      compilation that happens here to be reused there.
     * appPhysicalTargetDir is the directory where the precompiled site is placed
     * typeDescriptionProvider is the provider used for retrieving type 
     * information for multi-targeting 
     */
    public ClientBuildManager(string appVirtualDir, string appPhysicalSourceDir,
        string appPhysicalTargetDir, ClientBuildManagerParameter parameter,
        TypeDescriptionProvider typeDescriptionProvider) {

        if (parameter == null) {
            parameter = new ClientBuildManagerParameter();
        }

        InitializeCBMTDPBridge(typeDescriptionProvider);

        // Always build clean in precompilation for deployment mode, 
        // since building incrementally raises all kind of issues (VSWhidbey 382954).
        if (!String.IsNullOrEmpty(appPhysicalTargetDir)) {
            parameter.PrecompilationFlags |= PrecompilationFlags.Clean;
        }

        _hostingParameters = new HostingEnvironmentParameters();
        _hostingParameters.HostingFlags = HostingEnvironmentFlags.DontCallAppInitialize |
                                          HostingEnvironmentFlags.ClientBuildManager;
        _hostingParameters.ClientBuildManagerParameter = parameter;
        _hostingParameters.PrecompilationTargetPhysicalDirectory = appPhysicalTargetDir;
        if (typeDescriptionProvider != null) {
            _hostingParameters.HostingFlags |= HostingEnvironmentFlags.SupportsMultiTargeting;
        }

        // Make sure the app virtual dir starts with /
        if (appVirtualDir[0] != '/')
            appVirtualDir = "/" + appVirtualDir;

        if (appPhysicalSourceDir == null
            && appVirtualDir.StartsWith(IISExpressPrefix, StringComparison.OrdinalIgnoreCase)
            && appVirtualDir.Length > IISExpressPrefix.Length) {
            // appVirtualDir should have the form "/IISExpress/<version>/LM/W3SVC/",
            // and we will try to extract the version.  The version will be validated
            // when it is passed to IISVersionHelper..ctor.
            int endSlash = appVirtualDir.IndexOf('/', IISExpressPrefix.Length);
            if (endSlash > 0) {
                _hostingParameters.IISExpressVersion = appVirtualDir.Substring(IISExpressPrefix.Length, endSlash - IISExpressPrefix.Length);
                appVirtualDir = appVirtualDir.Substring(endSlash);
            }
        }

        Initialize(VirtualPath.CreateNonRelative(appVirtualDir), appPhysicalSourceDir);
    }

    /*
     * returns the codegendir used by runtime appdomain
     */
    public string CodeGenDir {
        get {
            if (_codeGenDir == null) {
                EnsureHostCreated();
                _codeGenDir = _host.CodeGenDir;
            }

            return _codeGenDir;
        }
    }

    /*
     * Indicates whether the host is created.
     */

    public bool IsHostCreated {
        get {
            return _host != null;
        }
    }

    /*
     * Create an object in the runtime appdomain
     */

    public IRegisteredObject CreateObject(Type type, bool failIfExists) {
        if (type == null) {
            throw new ArgumentNullException("type");
        }

        EnsureHostCreated();
        Debug.Assert(_appId != null);
        Debug.Assert(_appHost != null);

        _host.RegisterAssembly(type.Assembly.FullName, type.Assembly.Location);

        ApplicationManager appManager = ApplicationManager.GetApplicationManager();
        return appManager.CreateObjectInternal(_appId, type, _appHost, failIfExists, _hostingParameters);
    }

    /*
     * Return the list of directories that would cause appdomain shutdown.
     */
    public string[] GetAppDomainShutdownDirectories() {
        Debug.Trace("CBM", "GetAppDomainShutdownDirectories");

        return FileChangesMonitor.s_dirsToMonitor;
    }

    /*
     * Makes sure that all the top level files are compiled (code, global.asax, ...)
     */

    public void CompileApplicationDependencies() {
        Debug.Trace("CBM", "CompileApplicationDependencies");

        EnsureHostCreated();

        _host.CompileApplicationDependencies();
    }


    public IDictionary GetBrowserDefinitions() {
        Debug.Trace("CBM", "GetBrowserDefinitions");

        EnsureHostCreated();

        return _host.GetBrowserDefinitions();
    }

    /*
     * Returns the physical path of the generated file corresponding to the virtual directory.
     * Note the virtualPath needs to use this format:
     * "/[appname]/App_WebReferences/{[subDir]/}"
     */
    public string GetGeneratedSourceFile(string virtualPath) {
        Debug.Trace("CBM", "GetGeneratedSourceFile " + virtualPath);

        if (virtualPath == null) {
            throw new ArgumentNullException("virtualPath");
        }

        EnsureHostCreated();

        return _host.GetGeneratedSourceFile(VirtualPath.CreateTrailingSlash(virtualPath));
    }

    /*
    * Returns the virtual path of the corresponding generated file.
    * Note the filepath needs to be a full path.
    */
    public string GetGeneratedFileVirtualPath(string filePath) {
        Debug.Trace("CBM", "GetGeneratedFileVirtualPath " + filePath);

        if (filePath == null) {
            throw new ArgumentNullException("filePath");
        }

        EnsureHostCreated();

        return _host.GetGeneratedFileVirtualPath(filePath);
    }
    /*
     * Returns an array of the virtual paths to all the code directories in the app thru the hosted appdomain
     */

    public string[] GetVirtualCodeDirectories() {
        Debug.Trace("CBM", "GetHostedVirtualCodeDirectories");

        EnsureHostCreated();

        return _host.GetVirtualCodeDirectories();
    }

    /*
     * Returns an array of the assemblies defined in the bin and assembly reference config section
     */

    public String[] GetTopLevelAssemblyReferences(string virtualPath) {
        Debug.Trace("CBM", "GetHostedVirtualCodeDirectories");

        if (virtualPath == null) {
            throw new ArgumentNullException("virtualPath");
        }

        EnsureHostCreated();

        return _host.GetTopLevelAssemblyReferences(VirtualPath.Create(virtualPath));
    }

    /*
     * Returns the compiler type and parameters that need to be used to build
     * a given code directory.  Also, returns the directory containing all the code
     * files generated from non-code files in the code directory (e.g. wsdl files)
     */

    public void GetCodeDirectoryInformation(string virtualCodeDir,
        out Type codeDomProviderType, out CompilerParameters compilerParameters,
        out string generatedFilesDir) {
        Debug.Trace("CBM", "GetCodeDirectoryInformation " + virtualCodeDir);

        if (virtualCodeDir == null) {
            throw new ArgumentNullException("virtualCodeDir");
        }

        EnsureHostCreated();

        _host.GetCodeDirectoryInformation(VirtualPath.CreateTrailingSlash(virtualCodeDir),
            out codeDomProviderType, out compilerParameters, out generatedFilesDir);

        Debug.Trace("CBM", "GetCodeDirectoryInformation " + virtualCodeDir + " end");
    }

    /*
     * Returns the compiler type and parameters that need to be used to build
     * a given file.
     */

    public void GetCompilerParameters(string virtualPath,
        out Type codeDomProviderType, out CompilerParameters compilerParameters) {
        Debug.Trace("CBM", "GetCompilerParameters " + virtualPath);

        if (virtualPath == null) {
            throw new ArgumentNullException("virtualPath");
        }

        EnsureHostCreated();

        _host.GetCompilerParams(VirtualPath.Create(virtualPath), out codeDomProviderType, out compilerParameters);
    }

    /*
     * Returns the codedom tree and the compiler type/param for a given file.
     */

    public CodeCompileUnit GenerateCodeCompileUnit(
        string virtualPath, out Type codeDomProviderType,
        out CompilerParameters compilerParameters, out IDictionary linePragmasTable) {
        Debug.Trace("CBM", "GenerateCodeCompileUnit " + virtualPath);

        return GenerateCodeCompileUnit(virtualPath, null, 
            out codeDomProviderType, out compilerParameters, out linePragmasTable);
    }


    public CodeCompileUnit GenerateCodeCompileUnit(
        string virtualPath, String virtualFileString, out Type codeDomProviderType,
        out CompilerParameters compilerParameters, out IDictionary linePragmasTable) {
        Debug.Trace("CBM", "GenerateCodeCompileUnit " + virtualPath);

        if (virtualPath == null) {
            throw new ArgumentNullException("virtualPath");
        }

        EnsureHostCreated();

        return _host.GenerateCodeCompileUnit(VirtualPath.Create(virtualPath), virtualFileString,
            out codeDomProviderType, out compilerParameters, out linePragmasTable);
    }

    public string GenerateCode(
        string virtualPath, String virtualFileString, out IDictionary linePragmasTable) {
        Debug.Trace("CBM", "GenerateCode " + virtualPath);

        if (virtualPath == null) {
            throw new ArgumentNullException("virtualPath");
        }

        EnsureHostCreated();

        return _host.GenerateCode(VirtualPath.Create(virtualPath), virtualFileString, out linePragmasTable);
    }

    /*
     * Returns the compiled type for an input file
     */

    public Type GetCompiledType(string virtualPath) {
        Debug.Trace("CBM", "GetCompiledType " + virtualPath);

        if (virtualPath == null) {
            throw new ArgumentNullException("virtualPath");
        }

        EnsureHostCreated();

        string[] typeAndAsemblyName = _host.GetCompiledTypeAndAssemblyName(VirtualPath.Create(virtualPath), null);
        if (typeAndAsemblyName == null)
            return null;

        Assembly a = Assembly.LoadFrom(typeAndAsemblyName[1]);
        Type t = a.GetType(typeAndAsemblyName[0]);
        return t;
    }

    /*
     * Compile a file
     */
    public void CompileFile(string virtualPath) {
        CompileFile(virtualPath, null);
    }

    public void CompileFile(string virtualPath, ClientBuildManagerCallback callback) {
        Debug.Trace("CBM", "CompileFile " + virtualPath);

        if (virtualPath == null) {
            throw new ArgumentNullException("virtualPath");
        }

        try {
            EnsureHostCreated();
            _host.GetCompiledTypeAndAssemblyName(VirtualPath.Create(virtualPath), callback);
        }
        finally {
            // DevDiv 180798. We are returning null in ClientBuildManagerCallback.InitializeLifetimeService,
            // so we need to manually disconnect the instance so that it will be released.
            if (callback != null) {
                RemotingServices.Disconnect(callback);
            }
        }
    }

    /*
     * Indicates whether an assembly is a code assembly.
     */
    public bool IsCodeAssembly(string assemblyName) {
        Debug.Trace("CBM", "IsCodeAssembly " + assemblyName);

        if (assemblyName == null) {
            throw new ArgumentNullException("assemblyName");
        }

        // 

        EnsureHostCreated();
        bool result = _host.IsCodeAssembly(assemblyName);

        Debug.Trace("CBM", "IsCodeAssembly " + result.ToString());
        return result;
    }


    public bool Unload() {
        Debug.Trace("CBM", "Unload");

        BuildManagerHost host = _host;
        if (host != null) {
            _host = null;
            return host.UnloadAppDomain();
        }

        return false;
    }

    /*
     * Precompile an application 
     */
    public void PrecompileApplication() {
        PrecompileApplication(null);
    }

    /*
     * Precompile an application with callback support
     */
    public void PrecompileApplication(ClientBuildManagerCallback callback) {
        PrecompileApplication(callback, false);
    }

    public void PrecompileApplication(ClientBuildManagerCallback callback, bool forceCleanBuild) {
        Debug.Trace("CBM", "PrecompileApplication");

        PrecompilationFlags savedFlags = _hostingParameters.ClientBuildManagerParameter.PrecompilationFlags;

        if (forceCleanBuild) {

            // If there was a previous host, it will be unloaded by CBM and we will wait for the callback.
            // If there was no previous host, we don't do any waiting.
            // DevDiv 46290
            _waitForCallBack = _host != null;

            Debug.Trace("CBM", "Started Unload");
            // Unload the existing appdomain so the new one will be created with the clean flag
            Unload();

            _hostingParameters.ClientBuildManagerParameter.PrecompilationFlags = 
                savedFlags | PrecompilationFlags.Clean;

            WaitForCallBack();
        }

        try {
            EnsureHostCreated();
            _host.PrecompileApp(callback, _hostingParameters.ClientBuildManagerParameter.ExcludedVirtualPaths);
        }
        finally {
            if (forceCleanBuild) {
                // Revert precompilationFlags
                _hostingParameters.ClientBuildManagerParameter.PrecompilationFlags = savedFlags;
            }
            // DevDiv 180798. We are returning null in ClientBuildManagerCallback.InitializeLifetimeService,
            // so we need to manually disconnect the instance so that it will be released.
            if (callback != null) {
                RemotingServices.Disconnect(callback);
            }
        }
    }

    // _waitForCallBack is set to false in OnAppDomainUnloaded.
    // This method waits until it is set to false before continuing, so that
    // we do not run into a concurrency issue where _host could be set to null.
    // DevDiv 46290
    private void WaitForCallBack() {
        Debug.Trace("CBM", "WaitForCallBack");
        int waited = 0;
        while (_waitForCallBack && waited <= 50) {
            Thread.Sleep(200);
            waited++;
        }
        if (_waitForCallBack) {
            Debug.Trace("CBM", "timeout while waiting for callback");
        }
        else {
            Debug.Trace("CBM", "callback received before timeout");
        }
    }

    public override Object InitializeLifetimeService() {
        return null; // never expire lease
    }

    internal void Initialize(VirtualPath virtualPath, string physicalPath) {
        Debug.Trace("CBM", "Initialize");

        _virtualPath = virtualPath;

        _physicalPath = FileUtil.FixUpPhysicalDirectory(physicalPath);

        _onAppDomainUnloadedCallback = new WaitCallback(OnAppDomainUnloadedCallback);
        _onAppDomainShutdown = new WaitCallback(OnAppDomainShutdownCallback);

        _installPath = RuntimeEnvironment.GetRuntimeDirectory();

        // Do not create host during intialization. It will be done on demand.
        //CreateHost();
    }

    private void EnsureHostCreated() {

        if (_host == null) {
            lock (_lock) {
                // Create the host if necessary
                if (_host == null) {
                    CreateHost();
                    Debug.Trace("CBM", "EnsureHostCreated: after CreateHost()");
                }
            }
        }

        // If an exception happened during host creation, rethrow it
        if (_hostCreationException != null) {
            Debug.Trace("CBM", "EnsureHostCreated: failed. " + _hostCreationException);

            // We need to wrap it in a new exception, otherwise we lose the original stack.
            throw new HttpException(_hostCreationException.Message,
                _hostCreationException);
        }
    }

    private void CreateHost() {
        Debug.Trace("CBM", "CreateHost");
        Debug.Assert(_host == null);

        Debug.Assert(!_hostCreationPending, "CreateHost: creation already pending");

        _hostCreationPending = true;

        // Use a local to avoid having a partially created _host
        BuildManagerHost host = null;

        try {
            string appId;
            IApplicationHost appHost;

            ApplicationManager appManager = ApplicationManager.GetApplicationManager();

            host = (BuildManagerHost) appManager.CreateObjectWithDefaultAppHostAndAppId(
                _physicalPath, _virtualPath,
                typeof(BuildManagerHost), false /*failIfExists*/, 
                _hostingParameters, out appId, out appHost);

            // host appdomain cannot be unloaded during creation.
            host.AddPendingCall();

            host.Configure(this);

            _host = host;
            _appId = appId;
            _appHost = appHost;

            _hostCreationException = _host.InitializationException;
        }
        catch (Exception e) {
            // If an exception happens, keep track of it
            _hostCreationException = e;

            // Even though the host initialization failed, keep track of it so subsequent
            // request will see the error
            _host = host;
        }
        finally {
            _hostCreationPending = false;

            if (host != null) {
                // Notify the client that the host is ready
                if (AppDomainStarted != null) {
                    AppDomainStarted(this, EventArgs.Empty);
                }

                // The host can be unloaded safely now.
                host.RemovePendingCall();
            }
        }

        Debug.Trace("CBM", "CreateHost LEAVE");
    }

    // Called by BuildManagerHost when the ASP appdomain is unloaded
    internal void OnAppDomainUnloaded(ApplicationShutdownReason reason) {
        Debug.Trace("CBM", "OnAppDomainUnloaded " + reason.ToString());

        _reason = reason;
        _waitForCallBack = false;

        // Don't do anything that can be slow here.  Instead queue in a worker thread
        ThreadPool.QueueUserWorkItem(_onAppDomainUnloadedCallback);
    }

    internal void ResetHost() {
        lock (_lock) {
            // Though _appId and _appHost are created along with _host,
            // we need not reset those here as they always correspond to 
            // default app id and app host.
            _host = null;
            _hostCreationException = null;
        }
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    private void OnAppDomainUnloadedCallback(Object unused) {
        Debug.Trace("CBM", "OnAppDomainUnloadedCallback");

        // Notify the client that the appdomain is unloaded
        if (AppDomainUnloaded != null) {
            AppDomainUnloaded(this, new BuildManagerHostUnloadEventArgs(_reason));
        }
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    private void OnAppDomainShutdownCallback(Object o) {
        if (AppDomainShutdown != null) {
            AppDomainShutdown(this, new BuildManagerHostUnloadEventArgs((ApplicationShutdownReason)o));
        }
    }

    internal void OnAppDomainShutdown(ApplicationShutdownReason reason) {
        // Don't do anything that can be slow here. Instead queue in a worker thread
        ThreadPool.QueueUserWorkItem(_onAppDomainShutdown, reason);
    }

    private void InitializeCBMTDPBridge(TypeDescriptionProvider typeDescriptionProvider) {
        if (typeDescriptionProvider == null){
            return;
        }
        _cbmTdpBridge = new ClientBuildManagerTypeDescriptionProviderBridge(typeDescriptionProvider);
    }

    internal ClientBuildManagerTypeDescriptionProviderBridge CBMTypeDescriptionProviderBridge {
        get {
            return _cbmTdpBridge;
        }
    }

    #region IDisposable
    //Dispose the runtime appdomain properly when CBM is disposed
    void IDisposable.Dispose() {
        Unload();
    }
    #endregion

}


[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
public class BuildManagerHostUnloadEventArgs : EventArgs {
    ApplicationShutdownReason _reason;

    public BuildManagerHostUnloadEventArgs(ApplicationShutdownReason reason) {
        _reason = reason;
    }

    // Get the reason for the hosted appdomain shutdown

    public ApplicationShutdownReason Reason { get { return _reason; } }
}


public delegate void BuildManagerHostUnloadEventHandler(object sender, BuildManagerHostUnloadEventArgs e);

/*
 * Type of the entries in the table returned by GenerateCodeCompileUnit
 */

[Serializable]
public sealed class LinePragmaCodeInfo {

    public LinePragmaCodeInfo() {
    }

    public LinePragmaCodeInfo(int startLine, int startColumn, int startGeneratedColumn, int codeLength, bool isCodeNugget) {
        this._startLine = startLine;
        this._startColumn = startColumn;
        this._startGeneratedColumn = startGeneratedColumn;
        this._codeLength = codeLength;
        this._isCodeNugget = isCodeNugget;
    }

    // Starting line in ASPX file
    internal int _startLine;

    public int StartLine { get { return _startLine; } }

    // Starting column in the ASPX file
    internal int _startColumn;

    public int StartColumn { get { return _startColumn; } }

    // Starting column in the generated source file (assuming no indentations are used)
    internal int _startGeneratedColumn;

    public int StartGeneratedColumn { get { return _startGeneratedColumn; } }

    // Length of the code snippet
    internal int _codeLength;

    public int CodeLength { get { return _codeLength; } }

    // Whether the script block is a nugget.
    internal bool _isCodeNugget;

    public bool IsCodeNugget { get { return _isCodeNugget; } }
}

}


