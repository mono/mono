//------------------------------------------------------------------------------
// <copyright file="TemplateParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Implements the ASP.NET template parser
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

// Turn this on to do regex profiling
//#define PROFILE_REGEX

namespace System.Web.UI {
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Reflection;
using System.Globalization;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.Caching;
using System.Web.Util;
using System.Web.Hosting;
using System.Web.Compilation;
using HttpException = System.Web.HttpException;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Web.Configuration;
using System.Web.Instrumentation;


/// <internalonly/>
/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public abstract class TemplateParser : BaseParser, IAssemblyDependencyParser {

    internal const string CodeFileBaseClassAttributeName = "codefilebaseclass";

    // The <compilation> config section
    private CompilationSection _compConfig;
    internal CompilationSection CompConfig {
        get { return _compConfig; }
    }

    // The <pages> config section
    private PagesSection _pagesConfig;
    internal PagesSection PagesConfig {
        get { return _pagesConfig; }
    }

    // const masks into the BitVector32
    private const int isServerTag                   = 0x00000001;
    private const int inScriptTag                   = 0x00000002;
    private const int ignoreScriptTag               = 0x00000004;
    private const int ignoreNextSpaceString         = 0x00000008;
    internal const int requiresCompilation          = 0x00000010;   // Has constructs that require compilation
    private const int ignoreControlProperties       = 0x00000020;
    internal const int aspCompatMode                = 0x00000040;
    private const int hasCodeBehind                 = 0x00000080;
    private const int inDesigner                    = 0x00000100;
    private const int ignoreParseErrors             = 0x00000200;
    private const int mainDirectiveSpecified        = 0x00000400;
    private const int mainDirectiveHandled          = 0x00000800;
    private const int useExplicit                   = 0x00001000;
    private const int hasDebugAttribute             = 0x00002000;
    private const int debug                         = 0x00004000;
    private const int noLinePragmas                 = 0x00008000;
    private const int strict                        = 0x00010000;
    internal const int noAutoEventWireup            = 0x00020000;
    private const int attemptedImplicitResources    = 0x00040000;
    internal const int buffer                       = 0x00080000;
    internal const int requiresSessionState         = 0x00100000;
    internal const int readOnlySessionState         = 0x00200000;
    internal const int validateRequest              = 0x00400000;
    internal const int asyncMode                    = 0x00800000;
    private const int throwOnFirstParseError        = 0x01000000;
    private const int ignoreParserFilter            = 0x02000000;
    internal const int calledFromParseControlFlag   = 0x04000000;
    #pragma warning disable 0649
    internal SimpleBitVector32 flags;
    #pragma warning restore 0649

    private MainTagNameToTypeMapper _typeMapper;
    internal MainTagNameToTypeMapper TypeMapper { get { return _typeMapper; } }

    internal ICollection UserControlRegisterEntries { get { return TypeMapper.UserControlRegisterEntries; } }
    internal List<TagNamespaceRegisterEntry> TagRegisterEntries { get { return TypeMapper.TagRegisterEntries; } }

    private Stack _builderStack; // Stack of BuilderStackEntry's
    internal Stack BuilderStack {
        get {
            EnsureRootBuilderCreated();
            return _builderStack;
        }
    }

    private string _id;
    private StringSet _idList;
    private Stack _idListStack;
    private ScriptBlockData _currentScript;
    private StringBuilder _literalBuilder;

    // The line number in file currently being parsed
    internal int _lineNumber;

    // The line number at which the current script block started
    private int _scriptStartLineNumber;

    // String that contains the data to be parsed
    private string _text;
    public string Text {
        get { return _text; }
        internal set { _text = value; }
    }

    // The class from which to inherit if we are compiling a class
    private Type _baseType;
    internal Type BaseType {
        get { return _baseType; }
        set { _baseType = value; }
    }

    // The namespace and name of the class from which to inherit.  Only used with code separation,
    // since we don't have the live Type in that case (not yet compiled)
    private string _baseTypeNamespace;
    internal string BaseTypeNamespace { get { return _baseTypeNamespace; } }
    private string _baseTypeName;
    internal string BaseTypeName { get { return _baseTypeName; } }

    internal bool IgnoreControlProperties {
        get { return flags[ignoreControlProperties]; }
        set { flags[ignoreControlProperties] = value; }
    }

    // Indicates whether the parser should throw on the first error.
    internal bool ThrowOnFirstParseError {
        get { return flags[throwOnFirstParseError]; }
        set { flags[throwOnFirstParseError] = value; }
    }

    // The interfaces that we implement (ArrayList of Type objects)
    private ArrayList _implementedInterfaces;
    internal ArrayList ImplementedInterfaces { get { return _implementedInterfaces; } }

    internal bool HasCodeBehind { get { return flags[hasCodeBehind]; } }

    internal abstract Type DefaultBaseType { get; }

    internal PageParserFilter _pageParserFilter;

    private IImplicitResourceProvider _implicitResourceProvider;

    // The FInDesigner property gets used by control builders so that
    // they can behave differently if needed.
    internal virtual bool FInDesigner {
        get { return flags[inDesigner]; }
        set { flags[inDesigner] = value; }
    }

    // When this is set, we ignore parse errors and keep on processing the page as
    // well as possible.  This is used for the Venus CBM scenario
    internal virtual bool IgnoreParseErrors {
        get { return flags[ignoreParseErrors]; }
        set { flags[ignoreParseErrors] = value; }
    }

    // When true, it is not legal to have any constructs that require compilation
    private CompilationMode _compilationMode;
    internal CompilationMode CompilationMode {
        get {
            // When precompiling for deployment, always compile everything (VSWhidbey 266509)
            if (BuildManager.PrecompilingForDeployment)
                return CompilationMode.Always;

            return _compilationMode;
        }
        set {
            if (value == CompilationMode.Never && flags[requiresCompilation]) {
                ProcessError(SR.GetString(SR.Compilmode_not_allowed));
            }

            _compilationMode = value;
        }
    }

    private ParserErrorCollection _parserErrors;
    private ParserErrorCollection ParserErrors {
        get {
            if (_parserErrors == null) {
                _parserErrors = new ParserErrorCollection();
            }

            return _parserErrors;
        }
    }

    private bool HasParserErrors {
        get { return _parserErrors != null && _parserErrors.Count > 0; }
    }

    // Method to report parser errors.
    protected void ProcessError(string message) {
        // Ignore the errors if in that mode.
        if (IgnoreParseErrors) {
            return;
        }

        // Rethrow as innerexception if in that mode.
        if (ThrowOnFirstParseError) {
            throw new HttpException(message);
        }

        // otherwise add to the error collection with proper info.
        ParserError parseError = new ParserError(message, CurrentVirtualPath, _lineNumber);
        ParserErrors.Add(parseError);

        // If there is a CBM callback, inform it of the error
        BuildManager.ReportParseError(parseError);
    }

    // Method to report exception, this is called when external exceptions are caught in the parser.
    protected void ProcessException(Exception ex) {
        // Ignore the errors if in that mode.
        if (IgnoreParseErrors) {
            return;
        }

        // Rethrow as innerexception if in that mode or it is a compile exception.
        if (ThrowOnFirstParseError || ex is HttpCompileException) {
            if (ex is HttpParseException)
                throw ex;
            throw new HttpParseException(ex.Message, ex);
        }

        // If it is already a parser exception remember the location corresponding to 
        // the original error.
        ParserError parseError;

        HttpParseException hpe = ex as HttpParseException;
        if (hpe != null) {
            parseError = new ParserError(hpe.Message, hpe.VirtualPath, hpe.Line);
        }
        else {
            parseError = new ParserError(ex.Message, CurrentVirtualPath, _lineNumber);
        }

        // Remember the original exception.
        parseError.Exception = ex;

        ParserErrors.Add(parseError);

        // If there is a CBM callback, inform it of the error only if the HttpParseException comes
        // from the current virtualpath. Since if the exception is thrown from parsing another file,
        // it would have been reported already.
        if (hpe == null || CurrentVirtualPath.Equals(hpe.VirtualPathObject)) {
            BuildManager.ReportParseError(parseError);
        }
    }

    // When true, there are construct that require compilation
    internal virtual bool RequiresCompilation {
        get {
            // By default, require compilation.  The Page parser overrides this
            // and can allow no-compile pages depending on the page contents
            return true;
        }
    }

    internal virtual bool IsCodeAllowed {
        get {
            // If it's a no-compile page, code is not allowed
            if (CompilationMode == CompilationMode.Never)
                return false;

            // Likewise, check if the PageParserFilter allows code
            if (_pageParserFilter != null && !_pageParserFilter.AllowCode)
                return false;

            return true;
        }
    }

    internal void EnsureCodeAllowed() {

        // If it's a no-compile page, fail since there is code on it.
        // Likewise if the PageParserFilter returns IsCodeAllowed == false
        if (!IsCodeAllowed) {
            ProcessError(SR.GetString(SR.Code_not_allowed));
        }

        // Remember the fact that this page MUST be compiled
        flags[requiresCompilation] = true;
    }

    // This is called whenever we parse an attribute that requires compilation
    internal void OnFoundAttributeRequiringCompilation(string attribName) {

        // If compilation is not alowed, fail
        if (!IsCodeAllowed) {
            ProcessError(SR.GetString(SR.Attrib_not_allowed, attribName));
        }

        // Remember the fact that this page MUST be compiled
        flags[requiresCompilation] = true;
    }

    // This is called whenever we parse a directive that requires compilation
    internal void OnFoundDirectiveRequiringCompilation(string directiveName) {

        // If compilation is not alowed, fail
        if (!IsCodeAllowed) {
            ProcessError(SR.GetString(SR.Directive_not_allowed, directiveName));
        }

        // Remember the fact that this page MUST be compiled
        flags[requiresCompilation] = true;
    }

    // This is called whenever we parse an event attribute on a tag
    internal void OnFoundEventHandler(string directiveName) {

        // If compilation is not alowed, fail
        if (!IsCodeAllowed) {
            ProcessError(SR.GetString(SR.Event_not_allowed, directiveName));
        }

        // Remember the fact that this page MUST be compiled
        flags[requiresCompilation] = true;
    }

    private IDesignerHost _designerHost;
    private ITypeResolutionService _typeResolutionService;
    internal IDesignerHost DesignerHost {
        get {
            Debug.Assert(FInDesigner, "DesignerHost should be accessed only when FInDesigner == true");
            return _designerHost;
        }
        set {
            Debug.Assert(FInDesigner, "DesignerHost should be accessed only when FInDesigner == true");
            _designerHost = value;

            _typeResolutionService = null;
            if (_designerHost != null) {
                _typeResolutionService = (ITypeResolutionService)_designerHost.GetService(typeof(ITypeResolutionService));
                if (_typeResolutionService == null) {
                    throw new ArgumentException(SR.GetString(SR.TypeResService_Needed));
                }
            }
        }
    }

    // true if we're parsing global.asax
    internal virtual bool FApplicationFile { get { return false; } }

    // The global delegate to use for the DataBind event on controls when
    // the parser is run in design-mode.
    private EventHandler _designTimeDataBindHandler;
    internal EventHandler DesignTimeDataBindHandler {
        get { return _designTimeDataBindHandler; }
        set { _designTimeDataBindHandler = value; }
    }

    // Used to detect circular references
    private StringSet _circularReferenceChecker;

    // The set of assemblies that the build system is telling us we will be linked with
    private ICollection _referencedAssemblies;

    // The set of assemblies that this file is explicitly asking for
    private AssemblySet _assemblyDependencies;
    internal AssemblySet AssemblyDependencies {
        get { return _assemblyDependencies; }
    }

    // The list of virtual paths to source files we are dependent on
    private StringSet _sourceDependencies;
    internal StringSet SourceDependencies {
        get { return _sourceDependencies; }
    }

    // The collection of <object> tags with scope=session
    internal HttpStaticObjectsCollection _sessionObjects;
    internal HttpStaticObjectsCollection SessionObjects {
        get { return _sessionObjects; }
    }

    // The collection of <object> tags with scope=application
    internal HttpStaticObjectsCollection _applicationObjects;
    internal HttpStaticObjectsCollection ApplicationObjects {
        get { return _applicationObjects; }
    }

    // data that was obtained from parsing the input file

    private RootBuilder _rootBuilder;
    internal RootBuilder RootBuilder {
        get {
            EnsureRootBuilderCreated();
            return _rootBuilder;
        }
    }

    // Main directive attributes coming from config
    internal IDictionary _mainDirectiveConfigSettings;

    // <namespace name, NamespaceEntry>
    private Hashtable _namespaceEntries;
    internal Hashtable NamespaceEntries { get { return _namespaceEntries; } }

    private CompilerType _compilerType;
    internal CompilerType CompilerType { get { return _compilerType; } }

    // the server side scripts (list of ScriptBlockData's)
    private ArrayList _scriptList;
    internal ArrayList ScriptList { get { return _scriptList; } }

    // the hash code which determines the set of controls on the page
    private HashCodeCombiner _typeHashCode = new HashCodeCombiner();
    internal int TypeHashCode { get { return _typeHashCode.CombinedHash32; } }

    // The <object> tags local to the page.  Entries are ObjectTagBuilder's.
    private ArrayList _pageObjectList;
    internal ArrayList PageObjectList { get { return _pageObjectList; } }

    // Record extra parse data
    private ParseRecorder _parseRecorders = ParseRecorder.Null;
    internal ParseRecorder ParseRecorders { get { return _parseRecorders; } }

    // Data parsed from the directives

    internal CompilerParameters CompilParams { get { return _compilerType.CompilerParameters; } }

    internal bool FExplicit { get { return flags[useExplicit]; } }

    internal bool FLinePragmas { get { return !flags[noLinePragmas]; } }

    private int _warningLevel=-1;
    private string _compilerOptions;

    internal bool FStrict { get { return flags[strict]; } }

    // File that we must be compiled with, aka code besides (optional)
    private VirtualPath _codeFileVirtualPath;
    internal VirtualPath CodeFileVirtualPath { get { return _codeFileVirtualPath; } }

    // Name that the user wants to give to the generated class
    private string _generatedClassName;
    internal string GeneratedClassName { get { return _generatedClassName; } }

    // Name that the user wants to give to the generated namespace
    private string _generatedNamespace = null;
    internal string GeneratedNamespace {
        get {
            // If no namespace was specified, use "ASP"
            if (_generatedNamespace == null)
                return BaseCodeDomTreeGenerator.defaultNamespace;

            return _generatedNamespace;
        }
    }

    private ControlBuilderInterceptor _controlBuilderInterceptor;
    internal ControlBuilderInterceptor ControlBuilderInterceptor {
        get {
            if (_controlBuilderInterceptor == null && CompConfig != null && CompConfig.ControlBuilderInterceptorTypeInternal != null) {
                _controlBuilderInterceptor = (ControlBuilderInterceptor) Activator.CreateInstance(CompConfig.ControlBuilderInterceptorTypeInternal);
            }
            return _controlBuilderInterceptor;
        }
    }

    /// <devdoc>
    /// Parse the input into a Control. This is used to parse in a control dynamically from some
    /// textual content.
    /// </devdoc>
    internal static Control ParseControl(string content, VirtualPath virtualPath, bool ignoreFilter) {
        if (content == null) {
            return null;
        }

        ITemplate t = ParseTemplate(content, virtualPath, ignoreFilter);

        // Create a parent control to hold the controls we parsed
        Control c = new Control();
        t.InstantiateIn(c);

        return c;
    }

    public static ITemplate ParseTemplate(string content, string virtualPath, bool ignoreFilter) {
        return ParseTemplate(content, VirtualPath.Create(virtualPath), ignoreFilter);
    }

    private static ITemplate ParseTemplate(string content, VirtualPath virtualPath, bool ignoreFilter) {
        TemplateParser parser = new UserControlParser();
        return parser.ParseTemplateInternal(content, virtualPath, ignoreFilter);
    }

    private ITemplate ParseTemplateInternal(string content, VirtualPath virtualPath, bool ignoreFilter) {

        // Use the passed in virtualPath, since we need to have one, and the content string
        // itself doesn't have one.
        CurrentVirtualPath = virtualPath;
        CompilationMode = CompilationMode.Never;
        _text = content;

        // Ignore the PageParserFilter when processing ParserControl/ParseTemplate (VSWhidbey 361509)
        // Allow the ignore action to be controlled by a parameter (DevDiv 38679)
        flags[ignoreParserFilter] = ignoreFilter;
        flags[calledFromParseControlFlag] = true;

        Parse();

        Debug.Assert(RootBuilder != null);
        return RootBuilder;
    }

    /*
     * Do some initialization before the parsing
     */
    internal virtual void PrepareParse() {
        if (_circularReferenceChecker == null)
            _circularReferenceChecker = new CaseInsensitiveStringSet();

        _baseType = DefaultBaseType;

        // Initialize the main directive
        _mainDirectiveConfigSettings = CreateEmptyAttributeBag();

        // Get the config sections we care about
        if (!FInDesigner) {
            _compConfig = MTConfigUtil.GetCompilationConfig(CurrentVirtualPath);
            _pagesConfig = MTConfigUtil.GetPagesConfig(CurrentVirtualPath);
        }

        // Get default settings from config
        ProcessConfigSettings();

        // Initialize the type mapper
        // This must follow processing of config, so it can use the results
        _typeMapper = new MainTagNameToTypeMapper(this as BaseTemplateParser);

        // Register the <object> tag
        _typeMapper.RegisterTag("object", typeof(System.Web.UI.ObjectTag));

        _sourceDependencies = new CaseInsensitiveStringSet();

        // Create and seed the stack of ID lists.
        _idListStack = new Stack();
        _idList = new CaseInsensitiveStringSet();

        _scriptList = new ArrayList();

        // Optionally collect additional parse data for render tracing
        InitializeParseRecorders();
    }

    private void InitializeParseRecorders() {
        if (FInDesigner)
            return;

        _parseRecorders = ParseRecorder.CreateRecorders(this);
    }

    private void EnsureRootBuilderCreated() {

        // Create it on demand
        if (_rootBuilder != null)
            return;

        if (BaseType == DefaultBaseType) {
            // If the base type is the default, no need to look up the attribute
            _rootBuilder = CreateDefaultFileLevelBuilder();
        }
        else {
            // Look for a custom attribute
            Type fileLevelBuilderType = GetFileLevelControlBuilderType();

            if (fileLevelBuilderType == null) {
                // No custom type: use the default
                _rootBuilder = CreateDefaultFileLevelBuilder();
            }
            else {
                // Create the custom file level builder
                _rootBuilder = (RootBuilder) HttpRuntime.CreateNonPublicInstance(
                    fileLevelBuilderType);
            }
        }

        _rootBuilder.Line = 1;
        _rootBuilder.Init(this, null, null, null, null, null);
        _rootBuilder.SetTypeMapper(TypeMapper);

        _rootBuilder.VirtualPath = CurrentVirtualPath;

        // Create and seed the stack of builders.
        _builderStack = new Stack();
        _builderStack.Push(new BuilderStackEntry(RootBuilder, null, null, 0, null, 0));
    }

    internal virtual Type DefaultFileLevelBuilderType {
        get {
            return typeof(RootBuilder);
        }
    }

    internal virtual RootBuilder CreateDefaultFileLevelBuilder() {

        // By default, create a RootBuilder
        return new RootBuilder();
    }

    private Type GetFileLevelControlBuilderType() {
        // Check whether the control's class exposes a custom file level builder type
        FileLevelControlBuilderAttribute cba = null;
        object[] attrs = BaseType.GetCustomAttributes(
            typeof(FileLevelControlBuilderAttribute), /*inherit*/ true);
        if ((attrs != null) && (attrs.Length > 0)) {
            Debug.Assert(attrs[0] is FileLevelControlBuilderAttribute);
            cba = (FileLevelControlBuilderAttribute)attrs[0];
        }

        if (cba == null)
            return null;

        // Make sure the type has the correct base class
        Util.CheckAssignableType(DefaultFileLevelBuilderType, cba.BuilderType);

        return cba.BuilderType;
    }

    // Get default settings from config
    internal virtual void ProcessConfigSettings() {
        if (_compConfig != null) {
            flags[useExplicit] = _compConfig.Explicit;
            flags[strict] = _compConfig.Strict;
        }

        if (PagesConfig != null) {
            _namespaceEntries = PagesConfig.Namespaces.NamespaceEntries;

            // Clone it so we don't modify the config settings
            if (_namespaceEntries != null)
                _namespaceEntries = (Hashtable) _namespaceEntries.Clone();

            if (!flags[ignoreParserFilter]) {
                // Check if a filter is registered, and if so initialize it
                _pageParserFilter = PageParserFilter.Create(PagesConfig, CurrentVirtualPath, this);
            }
        }
    }

    internal void Parse(ICollection referencedAssemblies, VirtualPath virtualPath) {

        _referencedAssemblies = referencedAssemblies;
        CurrentVirtualPath = virtualPath;

        Parse();
    }

    /*
     * Parse the input
     */
    internal void Parse() {

        // Always set the culture to Invariant when parsing (ASURT 99071)
        Thread currentThread = Thread.CurrentThread;
        CultureInfo prevCulture = currentThread.CurrentCulture;
        System.Web.Util.Debug.Trace("Culture", "Before parsing, culture is " + prevCulture.DisplayName);
        currentThread.CurrentCulture = CultureInfo.InvariantCulture;

        try {
            try {
                // Do some initialization before the parsing
                PrepareParse();
                ParseInternal();
                HandlePostParse();
            }
            finally {
                // Restore the previous culture
                System.Web.Util.Debug.Trace("Culture", "After parsing, culture is " + currentThread.CurrentCulture.DisplayName);
                currentThread.CurrentCulture = prevCulture;
                System.Web.Util.Debug.Trace("Culture", "Restored culture to " + prevCulture.DisplayName);
            }
        }
        catch { throw; }    // Prevent Exception Filter Security Issue (ASURT 122835)
    }

    internal virtual void ParseInternal() {
        // Parse either the file or string
        if (_text != null) {
            ParseString(_text, CurrentVirtualPath, Encoding.UTF8);
        }
        else {
            AddSourceDependency(CurrentVirtualPath);
            ParseFile(null /*physicalPath*/, CurrentVirtualPath.VirtualPathString);
        }
    }

    internal TemplateParser() {
        ThrowOnFirstParseError = true;
    }

    /*
     * Parse the contents of the input file
     */

    protected void ParseFile(string physicalPath, string virtualPath) {
        ParseFile(physicalPath, VirtualPath.Create(virtualPath));
    }

    internal void ParseFile(string physicalPath, VirtualPath virtualPath) {

        // Determine the file used for the circular references checker.  Normally,
        // we use the virtualPath, but we use the physical path if it specified,
        // as is the case for <!-- #include file="foo.inc" -->
        string fileToReferenceCheck = physicalPath != null ? physicalPath : virtualPath.VirtualPathString;

        // Check for circular references of include files
        if (_circularReferenceChecker.Contains(fileToReferenceCheck)) {
            ProcessError(SR.GetString(SR.Circular_include));

            return;
        }

        // Add the current file to the circular references checker.
        _circularReferenceChecker.Add(fileToReferenceCheck);

        try {
            // Open a TextReader either from the physical or virtual path
            StreamReader reader;
            if (physicalPath != null) {
                using (reader = Util.ReaderFromFile(physicalPath, CurrentVirtualPath)) {
                    ParseReader(reader, virtualPath);
                }
            }
            else {
                // Open a TextReader for the virtualPath we're parsing
                using (Stream stream = virtualPath.OpenFile()) {
                    reader = Util.ReaderFromStream(stream, CurrentVirtualPath);
                    ParseReader(reader, virtualPath);
                }
            }
        }
        finally {
            // Remove the current file from the circular references checker
            _circularReferenceChecker.Remove(fileToReferenceCheck);
        }
    }

    /*
     * Parse the contents of the TextReader
     */
    private void ParseReader(StreamReader reader, VirtualPath virtualPath) {
        string s = reader.ReadToEnd();

        // Save the text of the input file in case it's trivial
        _text = s;

        ParseString(s, virtualPath, reader.CurrentEncoding);
    }

    private void AddLiteral(string literal) {

        if (_literalBuilder == null)
            _literalBuilder = new StringBuilder();

        _literalBuilder.Append(literal);
    }

    private string GetLiteral() {
        if (_literalBuilder == null)
            return null;

        return _literalBuilder.ToString();
    }

    /*
     * Update the hash code of the Type we're creating by xor'ing it with
     * a string.
     */
    internal void UpdateTypeHashCode(string text) {
        _typeHashCode.AddObject(text);
    }

    /*
     * Parse the contents of the string, and catch exceptions
     */
    internal void ParseString(string text, VirtualPath virtualPath, Encoding fileEncoding) {

        System.Web.Util.Debug.Trace("Template", "Starting parse at " + DateTime.Now);

        // Save the previous base dirs and line number
        VirtualPath prevVirtualPath = CurrentVirtualPath;
        int prevLineNumber = _lineNumber;

        // Set the new current base dirs and line number
        CurrentVirtualPath = virtualPath;
        _lineNumber = 1;

        // Always ignore the spaces at the beginning of a string
        flags[ignoreNextSpaceString] = true;

        try {
            ParseStringInternal(text, fileEncoding);

            // If there are parser errors caught in the parser
            if (HasParserErrors) {
                ParserError firstError = ParserErrors[0];

                Exception originalException = firstError.Exception;

                // Use the first error as the inner exception if not already caught one.
                if (originalException == null) {
                    originalException = new HttpException(firstError.ErrorText);
                }

                // Make it a HttpParseException with proper info.
                HttpParseException ex = new HttpParseException(firstError.ErrorText,
                    originalException, firstError.VirtualPath, Text, firstError.Line);

                // Add the rest of the errors
                for (int i = 1; i < ParserErrors.Count; i++) {
                    ex.ParserErrors.Add(ParserErrors[i]);
                }

                // throw the new exception
                throw ex;
            }

            // Make sure that if any code calls ProcessError/ProcessException after this point,
            // it throws the error right away, since we won't look at ParserErrors/_firstParseException
            // anymore
            ThrowOnFirstParseError = true;
        }
        catch (Exception e) {
            ErrorFormatter errorFormatter = null;

            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_PRE_PROCESSING);
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);

            // Check if the exception has a formatter
            errorFormatter = HttpException.GetErrorFormatter(e);

            // If it doesn't, throw a parse exception
            if (errorFormatter == null) {

                throw new HttpParseException(e.Message, e,
                    CurrentVirtualPath, text, _lineNumber);
            }
            else {
                // Otherwise, just rethrow it
                throw;
            }
        }
        finally {
            // Restore the previous base dirs and line number
            CurrentVirtualPath = prevVirtualPath;
            _lineNumber = prevLineNumber;
        }

        System.Web.Util.Debug.Trace("Template", "Ending parse at " + DateTime.Now);
    }

#if PROFILE_REGEX
    private Match RunTagRegex(string text, int textPos) {
        int i=1;
        if (i==0)
            throw new HttpException("Bogus exception just to prevent method inlining");

        return TagRegex.Match(text, textPos);
    }

    private Match RunDirectiveRegex(string text, int textPos) {
        int i=1;
        if (i==0)
            throw new HttpException("Bogus exception just to prevent method inlining");

        return directiveRegex.Match(text, textPos);
    }

    private Match RunEndTagRegex(string text, int textPos) {
        int i=1;
        if (i==0)
            throw new HttpException("Bogus exception just to prevent method inlining");

        return endtagRegex.Match(text, textPos);
    }

    private Match RunCodeBlockRegex(string text, int textPos) {
        int i=1;
        if (i==0)
            throw new HttpException("Bogus exception just to prevent method inlining");

        return aspCodeRegex.Match(text, textPos);
    }

    private Match RunExprCodeBlockRegex(string text, int textPos) {
        int i=1;
        if (i==0)
            throw new HttpException("Bogus exception just to prevent method inlining");

        return aspExprRegex.Match(text, textPos);
    }

    private Match RunCommentRegex(string text, int textPos) {
        int i=1;
        if (i==0)
            throw new HttpException("Bogus exception just to prevent method inlining");

        return commentRegex.Match(text, textPos);
    }

    private Match RunIncludeRegex(string text, int textPos) {
        int i=1;
        if (i==0)
            throw new HttpException("Bogus exception just to prevent method inlining");

        return includeRegex.Match(text, textPos);
    }

    private Match RunTextRegex(string text, int textPos) {
        int i=1;
        if (i==0)
            throw new HttpException("Bogus exception just to prevent method inlining");

        return textRegex.Match(text, textPos);
    }
#endif // PROFILE_REGEX

    /*
     * Parse the contents of the string
     */
    private void ParseStringInternal(string text, Encoding fileEncoding) {
        int textPos = 0;

        // Find the last '>' in the input string
        int lastGTIndex = text.LastIndexOf('>');

        Regex tagRegex = TagRegex;

        for (;;) {
            Match match;

            // 1: scan for text up to the next tag.

#if PROFILE_REGEX
            if ((match = RunTextRegex(text, textPos)).Success)
#else
            if ((match = textRegex.Match(text, textPos)).Success)
#endif
            {
                // Append the text to the literal builder
                AddLiteral(match.ToString());

                _lineNumber += Util.LineCount(text, textPos,
                                         match.Index + match.Length);
                textPos = match.Index + match.Length;
            }

            // we might be done now

            if (textPos == text.Length)
                break;

            // 2: handle constructs that start with <

            // This later gets set to true if we match a regex, but do not
            // process the match
            bool fMatchedButNotProcessed = false;

            // Check to see if it's a directive (i.e. <%@ %> block)

            if (!flags[inScriptTag] &&
#if PROFILE_REGEX
                (match = RunDirectiveRegex(text, textPos)).Success)
#else
                (match = directiveRegex.Match(text, textPos)).Success)
#endif
            {
                ProcessLiteral();

                // Get all the directives into a bag
                ParsedAttributeCollection directive;
                string duplicateAttribute;
                string directiveName = ProcessAttributes(text, match, out directive, true, out duplicateAttribute);

                try {
                    // If there is a parser filter, give it a chance to look at the directive
                    PreprocessDirective(directiveName, directive);

                    ProcessDirective(directiveName, directive);
                }
                catch(Exception e) {
                    ProcessException(e);
                }

                // If we just found the main directive, and it uses a codeFile, check if we need to create
                // a modified version of the file (used for updatable deployment precompilation)
                if (directiveName.Length == 0 && _codeFileVirtualPath != null) {
                    CreateModifiedMainDirectiveFileIfNeeded(text, match, directive, fileEncoding);
                }

                // Always ignore the spaces after a directive
                flags[ignoreNextSpaceString] = true;
            }

            // Check to see if it's a server side include
            // e.g. <!-- #include file="foo.inc" -->

#if PROFILE_REGEX
            else if ((match = RunIncludeRegex(text, textPos)).Success)
#else
            else if ((match = includeRegex.Match(text, textPos)).Success)
#endif
            {
                try {
                    ProcessServerInclude(match);
                }
                catch(Exception ex) {
                    ProcessException(ex);
                }
            }

            // Check to see if it's a comment <%-- --%> block

#if PROFILE_REGEX
            else if ((match = RunCommentRegex(text, textPos)).Success)
#else
            else if ((match = commentRegex.Match(text, textPos)).Success)
#endif
            {
                // Just skip it
            }

            // Check to see if it's an expression code block (i.e. <%= ... %> block)

            else if (!flags[inScriptTag] &&
#if PROFILE_REGEX
                     (match = RunExprCodeBlockRegex(text, textPos)).Success)
#else
                     (match = aspExprRegex.Match(text, textPos)).Success)
#endif
            {
                ProcessCodeBlock(match, CodeBlockType.Expression, text);
            }

            // Check to see if it's an encoded expression code block (i.e. <%: ... %> block)

            else if (!flags[inScriptTag] && (match = aspEncodedExprRegex.Match(text, textPos)).Success) {
                ProcessCodeBlock(match, CodeBlockType.EncodedExpression, text);
            }

            // Check to see if it's a databinding expression block (i.e. <%# ... %> block)
            // This does not include <%# %> blocks used as values for
            // attributes of server tags.

            else if (!flags[inScriptTag] &&
                     (match = databindExprRegex.Match(text, textPos)).Success) {
                ProcessCodeBlock(match, CodeBlockType.DataBinding, text);
            }

            // Check to see if it's a code block (<% ... %>)

            else if (!flags[inScriptTag] &&
#if PROFILE_REGEX
                     (match = RunCodeBlockRegex(text, textPos)).Success)
#else
                     (match = aspCodeRegex.Match(text, textPos)).Success)
#endif
            {
                string code = match.Groups["code"].Value.Trim();
                if (code.StartsWith("$", StringComparison.Ordinal)) {
                    ProcessError(SR.GetString(SR.ExpressionBuilder_LiteralExpressionsNotAllowed, match.ToString(), code));
                }
                else {
                    ProcessCodeBlock(match, CodeBlockType.Code, text);
                }
            }

            // Check to see if it's a tag.  Don't run the tag regex if there is no '>' after the
            // current position, since we know it cannot match, and it can take an exponential
            // amount of time to run (VSWhidbey 141878,358072)

            else if (!flags[inScriptTag] &&
#if PROFILE_REGEX
                     (match = RunTagRegex(text, textPos)).Success)
#else
                     lastGTIndex > textPos && (match = tagRegex.Match(text, textPos)).Success)
#endif
            {
                try {
                    if (!ProcessBeginTag(match, text))
                        fMatchedButNotProcessed = true;
                }
                catch (Exception ex) {
                    ProcessException(ex);
                }
            }

            // Check to see if it's an end tag

#if PROFILE_REGEX
            else if ((match = RunEndTagRegex(text, textPos)).Success)
#else
            else if ((match = endtagRegex.Match(text, textPos)).Success)
#endif
            {
                if (!ProcessEndTag(match))
                    fMatchedButNotProcessed = true;
            }

            // Did we process the block that started with a '<'?
            if (match == null || !match.Success || fMatchedButNotProcessed) {
                // If we could not match the '<' at all, check for some
                // specific syntax errors
                if (!fMatchedButNotProcessed && !flags[inScriptTag])
                    DetectSpecialServerTagError(text, textPos);

                // Skip the '<'
                textPos++;
                AddLiteral("<");
            }
            else {
                _lineNumber += Util.LineCount(text, textPos,
                                         match.Index + match.Length);
                textPos = match.Index + match.Length;
            }

            // we might be done now
            if (textPos == text.Length)
                break;
        }

        if (flags[inScriptTag] && !IgnoreParseErrors) {
            // Change the line number to where the script tag started to get
            // the correct error message (ASURT 13698).
            _lineNumber = _scriptStartLineNumber;

            ProcessError(SR.GetString(SR.Unexpected_eof_looking_for_tag, "script"));
            return;
        }

        // Process the final literal (if any)
        ProcessLiteral();
    }

    // Used for updatable deployment precompilation
    void CreateModifiedMainDirectiveFileIfNeeded(string text, Match match,
            ParsedAttributeCollection mainDirective, Encoding fileEncoding) {

        TextWriter precompTargetWriter = BuildManager.GetUpdatableDeploymentTargetWriter(CurrentVirtualPath, fileEncoding);

        // If we're not precompiling for deployment, there is nothing to do here
        if (precompTargetWriter == null)
            return;

        using (precompTargetWriter) {

            // Write out everything up to the main directive
            precompTargetWriter.Write(text.Substring(0, match.Index));

            precompTargetWriter.Write("<%@ " + DefaultDirectiveName);

            // Go through all the attributes on the main directive
            foreach (DictionaryEntry entry in mainDirective) {

                string attribName = (string) entry.Key;
                string attribValue = (string) entry.Value;

                // Remove the codefile and CodeFileBaseClass attributes
                if (StringUtil.EqualsIgnoreCase(attribName, "codefile")) continue;
                if (StringUtil.EqualsIgnoreCase(attribName, CodeFileBaseClassAttributeName)) continue;

                // Write out a special token for the inherits attribute.  It will later be replaced by
                // the full type later in the precompilation.  We can't do it here because we don't know
                // the assembly name yet (VSWhidbey 467936)
                if (StringUtil.EqualsIgnoreCase(attribName, "inherits")) {
                    attribValue = BuildManager.UpdatableInheritReplacementToken;
                }

                precompTargetWriter.Write(" ");
                precompTargetWriter.Write(attribName);
                precompTargetWriter.Write("=\"");
                precompTargetWriter.Write(attribValue);
                precompTargetWriter.Write("\"");
            }

            precompTargetWriter.Write(" %>");

            // Write out everything after the main directive
            precompTargetWriter.Write(text.Substring(match.Index+match.Length));
        }
    }

    /*
     * Do what needs to be done before returning after the parsing is complete
     */
    internal virtual void HandlePostParse() {

        // If there was no main directive in the page, process settings that may have come from config
        if (!flags[mainDirectiveHandled]) {
            ProcessMainDirective(_mainDirectiveConfigSettings);
            flags[mainDirectiveHandled] = true;
        }

        // We need to check the PageParserFilter here to handle the case where the base was specified
        // in web.config, and was *not* overridden with an 'inherits' attribute.
        if (_pageParserFilter != null) {
            if (!_pageParserFilter.AllowBaseType(BaseType)) {
                throw new HttpException(
                    SR.GetString(SR.Base_type_not_allowed, BaseType.FullName));
            }
        }

        // If there is more than one builder on the stack, some tag was
        // not correctly closed, which is an error.
        if (BuilderStack.Count > 1) {
            BuilderStackEntry entry = (BuilderStackEntry) _builderStack.Peek();

            string message = SR.GetString(SR.Unexpected_eof_looking_for_tag, entry._tagName);
            ProcessException(new HttpParseException(message, null, entry.VirtualPath, entry._inputText, entry.Line));

            return;
        }

        // If no language was specified in the page
        if (_compilerType == null) {

            if (!FInDesigner) {
                // Get a default from config
                _compilerType = CompilationUtil.GetDefaultLanguageCompilerInfo(
                    _compConfig, CurrentVirtualPath);
            }
            else {
                // Get default from code
                _compilerType = CompilationUtil.GetCodeDefaultLanguageCompilerInfo();
            }
        }

        CompilerParameters compilParams = _compilerType.CompilerParameters;

        // Override certain settings if they were specified on the page
        if (flags[hasDebugAttribute])
            compilParams.IncludeDebugInformation = flags[debug];

        // Debugging requires medium trust level
        if (compilParams.IncludeDebugInformation)
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, SR.Debugging_not_supported_in_low_trust);

        // If warningLevel was specified in the page, use it
        if (_warningLevel >= 0) {
            compilParams.WarningLevel = _warningLevel;
            compilParams.TreatWarningsAsErrors = (_warningLevel>0);
        }
        if (_compilerOptions != null)
            compilParams.CompilerOptions = _compilerOptions;

        // Tell the filter (if any) that the parsing is complete
        if (_pageParserFilter != null)
            _pageParserFilter.ParseComplete(RootBuilder);

        // Tell the ParseRecorders that parsing is complete
        ParseRecorders.ParseComplete(RootBuilder);
    }

    /*
     * Process all the text in the literal StringBuilder, and reset it
     */
    private void ProcessLiteral() {
        // Debug.Trace("Template", "Literal text: \"" + _literalBuilder.ToString() + "\"");

        // Get the current literal string
        string literal = GetLiteral();

        // Nothing to do if it's empty
        if (String.IsNullOrEmpty(literal)) {
            flags[ignoreNextSpaceString] = false;
            return;
        }

        // In global.asax, we don't allow random rendering content
        if (FApplicationFile) {
            // Make sure the literal is just white spaces
            int iFirstNonWhiteSpace = Util.FirstNonWhiteSpaceIndex(literal);

            // Only move the line number if not ignore parser error, otherwise the linenumber
            // of other valid elements will be off.
            if (iFirstNonWhiteSpace >= 0 && !IgnoreParseErrors) {
                // Move the line number back to the first non-whitespace
                _lineNumber -= Util.LineCount(literal, iFirstNonWhiteSpace, literal.Length);

                ProcessError(SR.GetString(SR.Invalid_app_file_content));
            }
        }
        else {

            // Check if we should ignore the string (ASURT 8186)
            bool fIgnoreThisLiteral = false;
            if (flags[ignoreNextSpaceString]) {
                flags[ignoreNextSpaceString] = false;

                if (Util.IsWhiteSpaceString(literal))
                    fIgnoreThisLiteral = true;
            }

            if (!fIgnoreThisLiteral) {

                // Process the settings that may come from config when the first non-trivial literal is parsed. VS Whidbey 141882
                if (!flags[mainDirectiveHandled]) {
                    ProcessMainDirective(_mainDirectiveConfigSettings);
                    flags[mainDirectiveHandled] = true;
                }

                // Add it to the top builder
                ControlBuilder builder = ((BuilderStackEntry) BuilderStack.Peek())._builder;
                try {
                    builder.AppendLiteralString(literal);
                }
                catch (Exception e) {
                    if (!IgnoreParseErrors) {
                        // If there was an error during the parsing of the literal, move
                        // the line number back to the beginning of the literal
                        int iFirstNonWhiteSpace = Util.FirstNonWhiteSpaceIndex(literal);
                        if (iFirstNonWhiteSpace < 0) iFirstNonWhiteSpace = 0;
                        _lineNumber -= Util.LineCount(literal, iFirstNonWhiteSpace, literal.Length);

                        ProcessException(e);
                    }
                }

                // Update the hash code with a fixed string, to mark that there is
                // a literal, but allow it to change without affecting the hash.
                UpdateTypeHashCode("string");
            }
        }

        // Reset the StringBuilder for the next literal
        _literalBuilder = null;
    }

    /*
     * Process a server side SCRIPT tag
     */
    private void ProcessServerScript() {
        // Get the contents of the script tag
        string script = GetLiteral();

        // Nothing to do if it's empty.
        // Unless we're in GenerateCodeCompileUnit mode, in which case
        // we want the empty block (VSWhidbey 112777)
        if (String.IsNullOrEmpty(script)) {
            if (!IgnoreParseErrors)
                return;

            script = String.Empty;
        }

        // Add this script to the script builder, unless we're
        // supposed to ignore it
        if (!flags[ignoreScriptTag]) {

            // First, give the PageParserFilter a chance to handle the code block
            if (!PageParserFilterProcessedCodeBlock(CodeConstructType.ScriptTag, script, _currentScript.Line)) {
                // Make sure it's legal to have code in this page
                EnsureCodeAllowed();

                _currentScript.Script = script;
                _scriptList.Add(_currentScript);
                _currentScript = null;
            }
        }
        // Reset the StringBuilder for the next literal
        _literalBuilder = null;
    }

    internal virtual void CheckObjectTagScope(ref ObjectTagScope scope) {

        // Map the default scope to Page
        if (scope == ObjectTagScope.Default)
            scope = ObjectTagScope.Page;

        // Check for invalid scopes
        if (scope != ObjectTagScope.Page) {
            throw new HttpException(
                SR.GetString(SR.App_session_only_valid_in_global_asax));
        }
    }

    /*
     * Process an Object tag, depending on its scope
     */
    private void ProcessObjectTag(ObjectTagBuilder objectBuilder) {

        ObjectTagScope scope = objectBuilder.Scope;
        CheckObjectTagScope(ref scope);

        // Page and AppInstance are treated identically
        if (scope == ObjectTagScope.Page ||
            scope == ObjectTagScope.AppInstance) {
            if (_pageObjectList == null)
                _pageObjectList = new ArrayList();

            _pageObjectList.Add(objectBuilder);
        }
        else if (scope == ObjectTagScope.Session) {
            if (_sessionObjects == null)
                _sessionObjects = new HttpStaticObjectsCollection();

            _sessionObjects.Add(objectBuilder.ID,
                objectBuilder.ObjectType,
                objectBuilder.LateBound);
        }
        else if (scope == ObjectTagScope.Application) {
            if (_applicationObjects == null)
                _applicationObjects = new HttpStaticObjectsCollection();

            _applicationObjects.Add(objectBuilder.ID,
                objectBuilder.ObjectType,
                objectBuilder.LateBound);
        }
        else {
            Debug.Assert(false, "Unexpected scope!");
        }
    }

    /*
     * Add a child builder to a builder
     */
    private void AppendSubBuilder(ControlBuilder builder, ControlBuilder subBuilder) {
        // Check if it's an object tag
        if (subBuilder is ObjectTagBuilder) {
            ProcessObjectTag((ObjectTagBuilder) subBuilder);
            return;
        }

        builder.AppendSubBuilder(subBuilder);
    }

    /*
     * Process an opening tag (possibly self-closed)
     */
    // Used to generate unique id's
    private int _controlCount;
    private bool ProcessBeginTag(Match match, string inputText) {
        string tagName = match.Groups["tagname"].Value;

        // Get all the attributes into a bag
        ParsedAttributeCollection attribs;
        string duplicateAttribute;
        ProcessAttributes(inputText, match, out attribs, false /*fDirective*/, out duplicateAttribute);

        // Check if the tag is self closed
        bool fSelfClosed = match.Groups["empty"].Success;

        // Is it a server side script tag?
        if (StringUtil.EqualsIgnoreCase(tagName, "script") && flags[isServerTag]) {
            ProcessScriptTag(match, inputText, attribs, fSelfClosed);
            return true;
        }

        // Process the settings that may come from config when the first non-trivial tag is parsed.  VS Whidbey 141882
        if (!flags[mainDirectiveHandled]) {
            ProcessMainDirective(_mainDirectiveConfigSettings);
            flags[mainDirectiveHandled] = true;
        }

        ControlBuilder parentBuilder = null;
        ControlBuilder subBuilder = null;
        Type childType = null;

        // This could be a property of an object that is filterable
        string realTagName;
        string filter = Util.ParsePropertyDeviceFilter(tagName, out realTagName);

        // Check if the parent builder wants to create a subcontrol for this tag.
        if (BuilderStack.Count > 1) {

            parentBuilder = ((BuilderStackEntry) _builderStack.Peek())._builder;

            // If the parent builder is a StringPropertyBuilder, we want to treat everything
            // in it as a literal, so we always return false here (VSWhidbey 285429)
            if (parentBuilder is StringPropertyBuilder)
                return false;

            subBuilder = parentBuilder.CreateChildBuilder(filter, realTagName, attribs, 
                this, parentBuilder, _id, _lineNumber, CurrentVirtualPath, ref childType, false);
        }

        // If not, use the root builder if runat=server is there.
        if (subBuilder == null && flags[isServerTag]) {
            subBuilder = RootBuilder.CreateChildBuilder(filter, realTagName, attribs, 
                this, parentBuilder, _id, _lineNumber, CurrentVirtualPath, ref childType, false);
        }

        // In case we find that the top stack item has the same name as the
        // current tag, we increase a count on the stack item.  This way, we
        // know that we need to ignore the corresponding closing tag (ASURT 50795)
        if (subBuilder == null && _builderStack.Count > 1 && !fSelfClosed) {
            BuilderStackEntry stackEntry = (BuilderStackEntry) _builderStack.Peek();
            if (StringUtil.EqualsIgnoreCase(tagName, stackEntry._tagName))
                stackEntry._repeatCount++;
        }

        // We could not get the type of a server control from that tag
        if (subBuilder == null) {
            // If it wasn't marked as runat=server, ignore
            if (!flags[isServerTag] || IgnoreParseErrors)
                return false;

            // If it was marked as runat=server, fail
            ProcessError(SR.GetString(SR.Unknown_server_tag, tagName));
            return true;
        }

        // We have a server control

        // If we have a control type filter, make sure the child control is allowed
        if (_pageParserFilter != null) {
            Debug.Assert(childType != null);

            if (!_pageParserFilter.AllowControlInternal(childType, subBuilder)) {
                ProcessError(SR.GetString(SR.Control_type_not_allowed, childType.FullName));
                return true;
            }
        }

        // Make sure it doesn't have duplicated attributes
        if (duplicateAttribute != null) {
            ProcessError(SR.GetString(SR.Duplicate_attr_in_tag, duplicateAttribute));
        }

        // Get the id from the builder.  Note that it may be null even if _id was not initially null,
        // if the builder is not for a Control (VSWhidbey 406302)
        _id = subBuilder.ID;

        // If it has an id, enforce validity and uniqueness
        if (_id != null) {
            if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(_id)) {
                ProcessError(SR.GetString(SR.Invalid_identifier, _id));
                return true;
            }

            if (_idList.Contains(_id)) {
                ProcessError(SR.GetString(SR.Id_already_used, _id));
                return true;
            }

            _idList.Add(_id);
        }
        else if (flags[isServerTag]) {
            // Make sure that cached controls always have a fixed id to prevent
            // unpredictable behavior (ASURT 83402)
            PartialCachingAttribute cacheAttrib = (PartialCachingAttribute)
                TypeDescriptor.GetAttributes(childType)[typeof(PartialCachingAttribute)];

            // If we are parsing a theme file, the controls do not have an ID,
            // and we should not be adding one. (Dev10 
            if (!(subBuilder.Parser is PageThemeParser) && cacheAttrib != null) {
                _id = "_ctrl_" + _controlCount.ToString(NumberFormatInfo.InvariantInfo);
                subBuilder.ID = _id;
                _controlCount++;
                // Controls can't be filtered, so use the default filter
                subBuilder.PreprocessAttribute(String.Empty, "id", _id, false /*mainDirectiveMode*/);
            }
        }


        // Take care of the previous literal string
        ProcessLiteral();

        if (childType != null) {
            // Update the hash code with the name of the control's type
            UpdateTypeHashCode(childType.FullName);
        }

        // If the server control has a body, and if it didn't self-close
        // (i.e. wasn't terminated with "/>"), put it on the stack of controls.
        if (!fSelfClosed && subBuilder.HasBody()) {

            // If it's a template, push a new ID list (ASURT 72773)
            if (subBuilder is TemplateBuilder && ((TemplateBuilder)subBuilder).AllowMultipleInstances) {
                _idListStack.Push(_idList);
                _idList = new CaseInsensitiveStringSet();
            }

            _builderStack.Push(new BuilderStackEntry(subBuilder, tagName,
                               CurrentVirtualPathString, _lineNumber,
                               inputText, match.Index + match.Length));

            // Optionally record begin tag position data
            ParseRecorders.RecordBeginTag(subBuilder, match);
        }
        else {
            // Append the sub builder to the current builder
            parentBuilder = ((BuilderStackEntry) _builderStack.Peek())._builder;
            AppendSubBuilder(parentBuilder, subBuilder);

            // Tell the builder that we're done parsing its control
            subBuilder.CloseControl();

            // Optionally record empty tag position data
            ParseRecorders.RecordEmptyTag(subBuilder, match);
        }

        return true;
    }

    /*
     * Process a <script runat=server> tag
     */
    private void ProcessScriptTag(Match match, string text, IDictionary attribs, bool fSelfClosed) {
        ProcessLiteral();

        // Always ignore the spaces after a script tag
        flags[ignoreNextSpaceString] = true;

        // Check if there is a 'src' attribute
        VirtualPath virtualPath = Util.GetAndRemoveVirtualPathAttribute(attribs, "src");
        if (virtualPath != null) {

            // Make sure it's legal to have code in this page
            EnsureCodeAllowed();

            // Get a full path to the script file
            virtualPath = ResolveVirtualPath(virtualPath);

            // Make sure access to the file is allowed (VSWhidbey 195545)
            HttpRuntime.CheckVirtualFilePermission(virtualPath.VirtualPathString);

            AddSourceDependency(virtualPath);

            ProcessLanguageAttribute((string)attribs["language"]);
            _currentScript = new ScriptBlockData(1, 1, virtualPath.VirtualPathString);

            _currentScript.Script = Util.StringFromVirtualPath(virtualPath);

            // Add this script to the script builder
            _scriptList.Add(_currentScript);
            _currentScript = null;

            // If the script tag is not self closed (even though it has a
            // src attribute), continue processing it, but eventually
            // ignore the content (ASURT 8883)
            if (!fSelfClosed) {
                flags[inScriptTag] = true;
                _scriptStartLineNumber = _lineNumber;
                flags[ignoreScriptTag] = true;
            }

            return;
        }

        ProcessLanguageAttribute((string)attribs["language"]);


        // Look for the last newline before the script block code string
        int startOfCode = match.Index + match.Length;
        int newlineIndex = text.LastIndexOfAny(s_newlineChars, startOfCode-1);

        // Use it to calculate the column where the code starts,
        // which improves the debugging experience (VSWhidbey 87172)
        int column = startOfCode-newlineIndex;
        Debug.Assert(column > 0);

        _currentScript = new ScriptBlockData(_lineNumber, column, CurrentVirtualPathString);

        // No 'src' attribute.  Make sure tag is not self closed.
        if (fSelfClosed) {
            ProcessError(SR.GetString(SR.Script_tag_without_src_must_have_content));
        }

        flags[inScriptTag] = true;
        _scriptStartLineNumber = _lineNumber;
    }

    /*
     * Called when a '</' sequence is seen. This means we can start closing
     * tags.
     */
    private bool ProcessEndTag(Match match) {
        string tagName = match.Groups["tagname"].Value;

        // If we are in the middle of a server side SCRIPT tag
        if (flags[inScriptTag]) {
            // Ignore anything that's not a </script>
            if (!StringUtil.EqualsIgnoreCase(tagName, "script"))
                return false;

            ProcessServerScript();

            flags[inScriptTag] = false;
            flags[ignoreScriptTag] = false;

            return true;
        }

        // See if anyone on the stack cares about termination.
        return MaybeTerminateControl(tagName, match);
    }

    internal bool IsExpressionBuilderValue(string val) {
        return ControlBuilder.expressionBuilderRegex.Match(val, 0).Success;
    }


    internal abstract string DefaultDirectiveName { get; }

    // If there is a parser filter, give it a chance to look at the directive
    internal void PreprocessDirective(string directiveName, IDictionary directive) {

        // No parser filter: done
        if (_pageParserFilter == null)
            return;

        if (directiveName.Length == 0)
            directiveName = DefaultDirectiveName;

        _pageParserFilter.PreprocessDirective(directiveName, directive);
    }

    /*
     * Process a <%@ %> block
     */
    internal virtual void ProcessDirective(string directiveName, IDictionary directive) {

        // Check for the main directive, which is "page" for an aspx,
        // and "application" for global.asax
        if (directiveName.Length == 0) {

            if (FInDesigner) {
                return;
            }

            // Make sure the main directive was not already specified
            if (flags[mainDirectiveSpecified]) {
                ProcessError(SR.GetString(SR.Only_one_directive_allowed, DefaultDirectiveName));

                return;
            }

            // If there are some config settings that were not overriden, add them to the list
            if (_mainDirectiveConfigSettings != null) {
                // Go through all the config attributes
                foreach (DictionaryEntry entry in _mainDirectiveConfigSettings) {

                    // If it was overridden, ignore the config setting
                    if (directive.Contains(entry.Key))
                        continue;

                    // Add it to the list
                    directive[entry.Key] = entry.Value;
                }
            }

            ProcessMainDirective(directive);

            flags[mainDirectiveSpecified] = true;
            flags[mainDirectiveHandled] = true;
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "assembly")) {
            // Assembly directive

            // Even though this only makes sense for compiled pages, Sharepoint needs us to
            // ignore instead of throw when the page in non-compiled.

            // Remove the attributes as we get them from the dictionary
            string assemblyName = Util.GetAndRemoveNonEmptyAttribute(directive, "name");
            VirtualPath src = Util.GetAndRemoveVirtualPathAttribute(directive, "src");

            // If there are some attributes left, fail
            Util.CheckUnknownDirectiveAttributes(directiveName, directive);

            if (assemblyName != null && src != null) {
                ProcessError(SR.GetString(SR.Attributes_mutually_exclusive, "Name", "Src"));
            }

            if (assemblyName != null) {
                AddAssemblyDependency(assemblyName);
            }
            // Is it a source file that needs to be compiled on the fly
            else if (src != null) {
                ImportSourceFile(src);
            }
            else {
                ProcessError(SR.GetString(SR.Missing_attr, "name"));
            }
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "import")) {

            // Import directive

            ProcessImportDirective(directiveName, directive);
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "implements")) {
            // 'implements' directive

            // We must compile the page if it asks to implement an interface
            OnFoundDirectiveRequiringCompilation(directiveName);

            // Remove the attributes as we get them from the dictionary
            string interfaceName = Util.GetAndRemoveRequiredAttribute(directive, "interface");

            // If there are some attributes left, fail
            Util.CheckUnknownDirectiveAttributes(directiveName, directive);

            Type interfaceType = GetType(interfaceName);

            // Make sure that it's an interface
            if (!interfaceType.IsInterface) {
                ProcessError(SR.GetString(SR.Invalid_type_to_implement, interfaceName));

                return;
            }

            // Add the interface type to the list
            if (_implementedInterfaces == null) {
                _implementedInterfaces = new ArrayList();
            }
            _implementedInterfaces.Add(interfaceType);
        }
        else if (!FInDesigner) {
            ProcessError(SR.GetString(SR.Unknown_directive, directiveName));
        }
    }

    internal virtual void ProcessMainDirective(IDictionary mainDirective) {

        // Used to store some temporary data resulting from the parsing of the directive
        IDictionary parseData = new HybridDictionary();

        // Keep track of unknown attributes
        ParsedAttributeCollection unknownAttributes = null;

        // Go through all the attributes on the directive
        foreach (DictionaryEntry entry in mainDirective) {

            string attribName = (string)entry.Key;

            // Parse out the device name, if any
            string deviceName = Util.ParsePropertyDeviceFilter(attribName, out attribName);

            try {
                // Try to process the attribute, and if not, keep track of it in the 'unknown' list
                if (!ProcessMainDirectiveAttribute(deviceName, attribName, (string)entry.Value, parseData)) {
                    if (unknownAttributes == null) {
                        unknownAttributes = CreateEmptyAttributeBag();
                    }

                    unknownAttributes.AddFilteredAttribute(deviceName, attribName, (string)entry.Value);
                }
            }
            catch (Exception e) {
                ProcessException(e);
            }
        }

        // Allow some postprocessing to happen, in case attributes have dependencies
        // on each other (e.g. mutually exclusive attributes).
        PostProcessMainDirectiveAttributes(parseData);

        // We should always set the control type of the root builder regardless of unknown attributes
        RootBuilder.SetControlType(BaseType);

        // If we didn't have any unknown attributes, we're done
        if (unknownAttributes == null)
            return;

        RootBuilder.ProcessImplicitResources(unknownAttributes);

        // Process all the unknown attributes
        foreach (FilteredAttributeDictionary filteredAttributes in unknownAttributes.GetFilteredAttributeDictionaries()) {
            string filter = filteredAttributes.Filter;

            foreach (DictionaryEntry attribute in filteredAttributes) {
                string attribName = (string)attribute.Key;
                ProcessUnknownMainDirectiveAttribute(filter, attribName, (string) attribute.Value);
            }
        }
    }

    internal virtual bool ProcessMainDirectiveAttribute(string deviceName, string name,
        string value, IDictionary parseData) {

        switch (name) {
        // Ignore description and codebehind attributes
        case "description":
        case "codebehind":
            break;

        case "debug":
            flags[debug] = Util.GetBooleanAttribute(name, value);
            if (flags[debug] && !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                throw new HttpException(SR.GetString(SR.Insufficient_trust_for_attribute, "debug"));
            }

            flags[hasDebugAttribute] = true;
            break;

        case "linepragmas":
            flags[noLinePragmas] = !Util.GetBooleanAttribute(name, value);
            break;

        case "warninglevel":
            _warningLevel = Util.GetNonNegativeIntegerAttribute(name, value);
            break;

        case "compileroptions":
            // This only makes sense for compiled pages
            OnFoundAttributeRequiringCompilation(name);

            string compilerOptions = value.Trim();

            CompilationUtil.CheckCompilerOptionsAllowed(compilerOptions, false /*config*/, null, 0);

            _compilerOptions = compilerOptions;
            break;

        // These two really only make sense in VB
        case "explicit":
            flags[useExplicit] = Util.GetBooleanAttribute(name, value);
            break;
        case "strict":
            flags[strict] = Util.GetBooleanAttribute(name, value);
            break;

        case "language":
            ValidateBuiltInAttribute(deviceName, name, value);
            string language = Util.GetNonEmptyAttribute(name, value);
            ProcessLanguageAttribute(language);
            break;

        // A "src" attribute is equivalent to an imported source file
        case "src":
            // This only makes sense for compiled pages
            OnFoundAttributeRequiringCompilation(name);

            // Remember the src assembly for post processing
            parseData[name] = Util.GetNonEmptyAttribute(name, value);
            break;

        case "inherits":
            // Remember the base class for post processing
            parseData[name] = Util.GetNonEmptyAttribute(name, value);
            break;

        case "classname":

            // Even though this only makes sense for compiled pages, Sharepoint needs us to
            // ignore instead of throw when the page in non-compiled.

            _generatedClassName = Util.GetNonEmptyFullClassNameAttribute(name, value,
                ref _generatedNamespace);
            break;

        case "codefile":
            // This only makes sense for compiled pages
            OnFoundAttributeRequiringCompilation(name);

            try {
                ProcessCodeFile(VirtualPath.Create(Util.GetNonEmptyAttribute(name, value)));
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
            break;

        default:
            // We didn't handle the attribute
            return false;
        }

        // The attribute was handled

        // Make sure no device filter or resource expression was specified
        ValidateBuiltInAttribute(deviceName, name, value);

        return true;
    }

    // Throw an exception if there is a device filter or resource expression
    internal void ValidateBuiltInAttribute(string deviceName, string name, string value) {
        Debug.Assert(deviceName != null);

        if (IsExpressionBuilderValue(value)) {
            ProcessError(SR.GetString(SR.Illegal_Resource_Builder, name));
        }

        if (deviceName.Length > 0) {
            ProcessError(SR.GetString(SR.Illegal_Device, name));
        }
    }

    internal virtual void ProcessUnknownMainDirectiveAttribute(string filter, string attribName, string value) {
        // By default, it is not legal to have unknown attributes.  But derived parser
        // classes can change this behavior
        ProcessError(SR.GetString(SR.Attr_not_supported_in_directive,
                attribName, DefaultDirectiveName));
    }

    internal virtual void PostProcessMainDirectiveAttributes(IDictionary parseData) {

        // Post process the src and inherits attributes

        string src = (string) parseData["src"];
        Assembly assembly = null;
        if (src != null) {
            try {
                assembly = ImportSourceFile(VirtualPath.Create(src));
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        // Was a code file base type specified in the directive
        string codeFileBaseTypeName = (string)parseData[CodeFileBaseClassAttributeName];

        // If so, there must also be a CodeFile attribute
        if (codeFileBaseTypeName != null && _codeFileVirtualPath == null) {
            throw new HttpException(SR.GetString(SR.CodeFileBaseClass_Without_Codefile));
        }

        // Was a base type specified in the directive
        string baseTypeName = (string) parseData["inherits"];
        if (baseTypeName != null) {
            try {
                ProcessInheritsAttribute(baseTypeName, codeFileBaseTypeName, src, assembly);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }
        else {
            if (_codeFileVirtualPath != null) {
                throw new HttpException(SR.GetString(SR.Codefile_without_inherits));
            }
        }
    }

    private void ProcessInheritsAttribute(string baseTypeName, string codeFileBaseTypeName,
        string src, Assembly assembly) {

        // If a code file is used, then the inherits attribute points to the class in that file,
        // which is not yet compiled.  In that case, we cannot get the Type, so we just keep
        // track of the class name (and namespace)
        if (_codeFileVirtualPath != null) {
            _baseTypeName = Util.GetNonEmptyFullClassNameAttribute("inherits", baseTypeName,
                ref _baseTypeNamespace);

            // Now set baseTypeName to codeFileBaseTypeName, so that if it was set, it will
            // be used as the BaseType during parsing (DevDiv 43024)
            baseTypeName = codeFileBaseTypeName;

            if (baseTypeName == null)
                return;
        }

        Type baseType = null;
        if (assembly != null)
            baseType = assembly.GetType(baseTypeName, false /* throwOnError */, true /* caseInsensitive */);
        else {
            try {
                baseType = GetType(baseTypeName);
            }
            catch {
                // We couldn't load the inherits type.  If the classname attribute has a
                // namespace, check if the inherits type exists in there (VSWhidbey 297056)

                // If the classname attribute doesn't have a namespace, give up
                if (_generatedNamespace == null)
                    throw;

                // If the inherits attribute already had a namespace, don't try this
                if (baseTypeName.IndexOf('.') >= 0)
                    throw;

                try {
                    // Try loading the inherit using the classname's namespace
                    string baseTypeNameWithNS = _generatedNamespace + "." + baseTypeName;
                    baseType = GetType(baseTypeNameWithNS);
                }
                catch {}

                // If that failed too, rethrow the original exception
                if (baseType == null)
                    throw;
            }
        }

        // Make sure we successfully got the Type of the base class
        if (baseType == null) {
            Debug.Assert(assembly != null, "assembly != null");
            ProcessError(SR.GetString(SR.Non_existent_base_type, baseTypeName, src));

            return;
        }

        // Make sure the base type extends the DefaultBaseType (Page or UserControl)
        if (!DefaultBaseType.IsAssignableFrom(baseType)) {
            ProcessError(SR.GetString(SR.Invalid_type_to_inherit_from, baseTypeName,
                    _baseType.FullName));

            return;
        }

        // If we have a control type filter, make sure the base type is allowed
        if (_pageParserFilter != null) {

            if (!_pageParserFilter.AllowBaseType(baseType)) {
                throw new HttpException(
                    SR.GetString(SR.Base_type_not_allowed, baseType.FullName));
            }
        }

        _baseType = baseType;

        // Now that we have the base type, we can create the RootBuilder
        Debug.Assert(_rootBuilder == null);
        EnsureRootBuilderCreated();

        // Make sure we link with the assembly of the base type (ASURT 101778)
        AddTypeDependency(_baseType);

        // Remember the fact that the page uses codebehind
        flags[hasCodeBehind] = true;
    }

    private void ProcessImportDirective(string directiveName, IDictionary directive) {

        // Even though this only makes sense for compiled pages, Sharepoint needs us to
        // ignore instead of throw when the page in non-compiled.

        // Remove the attributes as we get them from the dictionary
        string ns = Util.GetAndRemoveNonEmptyNoSpaceAttribute(directive, "namespace");

        if (ns == null)
            ProcessError(SR.GetString(SR.Missing_attr, "namespace"));
        else
            AddImportEntry(ns);

        // If there are some attributes left, fail
        Util.CheckUnknownDirectiveAttributes(directiveName, directive);
    }

    /*
     * Process a language attribute, as can appear in the Page directive and in
     * <script runat=server> tags.
     */
    private void ProcessLanguageAttribute(string language) {
        if (language == null)
            return;

        // We don't have CompilationConfig at design-time and the language attribute isn't used either.
        if (FInDesigner)
            return;

        CompilerType compilerType = CompilationUtil.GetCompilerInfoFromLanguage(
            CurrentVirtualPath, language);

        // Make sure we don't get conflicting languages
        if (_compilerType != null &&
            _compilerType.CodeDomProviderType != compilerType.CodeDomProviderType) {
            ProcessError(SR.GetString(SR.Mixed_lang_not_supported, language));

            return;
        }

        _compilerType = compilerType;
    }

    /*
     * Process a compileFile attribute (aka code besides)
     */
    private void ProcessCodeFile(VirtualPath codeFileVirtualPath) {

        Debug.Assert(_codeFileVirtualPath == null);

        _codeFileVirtualPath = ResolveVirtualPath(codeFileVirtualPath);

        // Get the language for the code beside page
        CompilerType compilerType = CompilationUtil.GetCompilerInfoFromVirtualPath(
            _codeFileVirtualPath);

        // Make sure we don't get conflicting languages
        if (_compilerType != null &&
            _compilerType.CodeDomProviderType != compilerType.CodeDomProviderType) {
            ProcessError(SR.GetString(SR.Inconsistent_CodeFile_Language));

            return;
        }

        // Check if it's trying to go cross app, or points to a special directory.
        // It's important to do this before checkin existence, to avoid revealing information
        // about other apps (VSWhidbey 442957)
        BuildManager.ValidateCodeFileVirtualPath(_codeFileVirtualPath);

        // Make sure the file exists
        Util.CheckVirtualFileExists(_codeFileVirtualPath);

        _compilerType = compilerType;

        // Add the code file to the list of files we depend on
        AddSourceDependency(_codeFileVirtualPath);
    }

    /*
     * Compile a source file into an assembly, and import it
     */
    private Assembly ImportSourceFile(VirtualPath virtualPath) {

        // If it's a no-compile page, ignore the imported source file
        if (CompilationMode == CompilationMode.Never)
            return null;

        // Get a full path to the source file
        virtualPath = ResolveVirtualPath(virtualPath);

        // If we have a page parser filter, make sure the reference is allowed
        if (_pageParserFilter != null && !_pageParserFilter.AllowVirtualReference(CompConfig, virtualPath)) {
            ProcessError(SR.GetString(SR.Reference_not_allowed, virtualPath));
        }

        // Add the source file to the list of files we depend on
        AddSourceDependency(virtualPath);

        // Compile it into an assembly

        BuildResultCompiledAssembly result = BuildManager.GetVPathBuildResult(
            virtualPath) as BuildResultCompiledAssembly;
        if (result == null) {
            ProcessError(SR.GetString(SR.Not_a_src_file, virtualPath));
        }

        Assembly a = result.ResultAssembly;

        // Add a dependency to the assembly and its dependencies
        AddAssemblyDependency(a, true /*addDependentAssemblies*/);

        return a;
    }

    /*
     * If we could not match the '<' at all, check for some specific syntax
     * errors.
     */
    private void DetectSpecialServerTagError(string text, int textPos) {

        if (IgnoreParseErrors) return;

        // If it started with <%, it's probably not closed (ASURT 13661)
        if (text.Length > textPos+1 && text[textPos+1] == '%') {
            ProcessError(SR.GetString(SR.Malformed_server_block));

            return;
        }

        // Search for the end of the tag ('>')
        Match match = gtRegex.Match(text, textPos);

        // No match, return
        if (!match.Success)
            return;

        // Get the complete potential tag
        string tag = text.Substring(textPos, match.Index-textPos+2);

        // Check if it's a case of nested <% %> block in a server tag (ASURT 8714)

        // If the tag does not contain runat=server, do nothing
        match = runatServerRegex.Match(tag);
        if (!match.Success)
            return;

        // If it has runat=server, but there is a '<' before it, don't fail, since
        // this '<' is probably the true tag start, and it will be processed later (ASURT 39531)
        // But ignore "<%" (ASURT 77554)
        Match matchLessThan = ltRegex.Match(tag, 1);
        if (matchLessThan.Success && matchLessThan.Index < match.Index)
            return;

        System.Web.Util.Debug.Trace("Template", "Found malformed server tag: " + tag);

        // Remove all <% %> constructs from within it.
        string tag2 = serverTagsRegex.Replace(tag, String.Empty);

        // If there were some <% %> constructs in the tag
        if ((object)tag2 != (object)tag) {
            // If it can be parsed as a tag after we removed the <% %> constructs, fail
            if (TagRegex.Match(tag2).Success) {
                ProcessError(SR.GetString(SR.Server_tags_cant_contain_percent_constructs));

                return;
            }
        }

        // Give a more generic error (fixed 18969, 30312)
        ProcessError(SR.GetString(SR.Malformed_server_tag));
    }

    /*
     * Add an entry to our list of NamespaceEntry's
     */
    internal void AddImportEntry(string ns) {

        // We're about to modify the list of namespaces, so if we already
        // have one (coming from config), clone it so we don't modify theirs.
        if (_namespaceEntries != null)
            _namespaceEntries = (Hashtable) _namespaceEntries.Clone();
        else
            _namespaceEntries = new Hashtable();

        NamespaceEntry namespaceEntry = new NamespaceEntry();
        namespaceEntry.Namespace = ns;

        namespaceEntry.Line = _lineNumber;
        namespaceEntry.VirtualPath = CurrentVirtualPathString;

        _namespaceEntries[ns] = namespaceEntry;
    }

    internal Assembly LoadAssembly(string assemblyName, bool throwOnFail) {

        if (_typeResolutionService != null) {
            AssemblyName asmName = new AssemblyName(assemblyName);
            return _typeResolutionService.GetAssembly(asmName, throwOnFail);
        }

        return _compConfig.LoadAssembly(assemblyName, throwOnFail);
    }

    internal Type GetType(string typeName, bool ignoreCase) {
        return GetType(typeName, ignoreCase, /* throwOnError */ true);
    }

    /*
     * Look for a type by name in the assemblies that this page links with
     */
    internal Type GetType(string typeName, bool ignoreCase, bool throwOnError) {

        // If it contains an assembly name, parse it out and load the assembly (ASURT 53589)
        Assembly a = null;
        int commaIndex = Util.CommaIndexInTypeName(typeName);
        if (commaIndex > 0) {
            string assemblyName = typeName.Substring(commaIndex + 1).Trim();
            typeName = typeName.Substring(0, commaIndex).Trim();

            try {
                a = LoadAssembly(assemblyName, !FInDesigner /*throwOnFail*/);
            }
            catch {
                throw new HttpException(
                    SR.GetString(SR.Assembly_not_compiled, assemblyName));
            }
        }

        // If we got an assembly, load the type from it
        if (a != null)
            return a.GetType(typeName, throwOnError, ignoreCase);

        // Otherwise, look for the type in the referenced assemblies (given by the build system)
        Type t;
        t = Util.GetTypeFromAssemblies(_referencedAssemblies, typeName, ignoreCase);
        if (t != null)
            return t;

        // Or in the assemblies that this page depends on
        t = Util.GetTypeFromAssemblies(AssemblyDependencies, typeName, ignoreCase);
        if (t != null)
            return t;

        if (throwOnError) {
            throw new HttpException(
                SR.GetString(SR.Invalid_type, typeName));
        }

        return null;
    }

    /*
     * Look for a type by name in the assemblies that this page links with
     */
    internal Type GetType(string typeName) {
        return GetType(typeName, false /*ignoreCase*/);
    }

    /*
     * Process a server side include.  e.g. <!-- #include file="foo.inc" -->
     */
    private void ProcessServerInclude(Match match) {
        if (flags[inScriptTag]) {
            throw new HttpException(
                SR.GetString(SR.Include_not_allowed_in_server_script_tag));
        }

        ProcessLiteral();

        string pathType = match.Groups["pathtype"].Value;
        string filename = match.Groups["filename"].Value;
        //System.Web.Util.Debug.Trace("Template", "#Include " + pathType + "=" + filename);

        if (filename.Length == 0) {
            ProcessError(SR.GetString(SR.Empty_file_name));

            return;
        }

        VirtualPath newVirtualPath = CurrentVirtualPath;
        string newPhysicalPath = null;

        if (StringUtil.EqualsIgnoreCase(pathType, "file")) {

            if (UrlPath.IsAbsolutePhysicalPath(filename)) {
                // If it's an absolute physical path, use it as is
                newPhysicalPath = filename;
            }
            else {

                // If it's relative, try to treat it as virtual

                bool treatAsVirtual = true;

                try {
                    newVirtualPath = ResolveVirtualPath(VirtualPath.Create(filename));
                }
                catch {
                    // If this fails, it probably means it tried to escape the root of the app.
                    // In that case, fall back to treating it as physical (VSWhidbey 339705)
                    treatAsVirtual = false;
                }

                if (treatAsVirtual) {
                    HttpRuntime.CheckVirtualFilePermission(newVirtualPath.VirtualPathString);
                    AddSourceDependency(newVirtualPath);
                }
                else {
                    // Treat it as relative to the physical path of the current page
                    string currentPhysicalDir = Path.GetDirectoryName(
                        CurrentVirtualPath.MapPath());
                    newPhysicalPath = Path.GetFullPath(Path.Combine(currentPhysicalDir, filename.Replace('/', '\\')));
                }
            }
        }
        else if (StringUtil.EqualsIgnoreCase(pathType, "virtual")) {
            newVirtualPath = ResolveVirtualPath(VirtualPath.Create(filename));
            HttpRuntime.CheckVirtualFilePermission(newVirtualPath.VirtualPathString);
            AddSourceDependency(newVirtualPath);
        }
        else {
            ProcessError(SR.GetString(SR.Only_file_virtual_supported_on_server_include));

            return;
        }

        if (newPhysicalPath != null) {
            // Make sure that access to the file is permitted (ASURT 73792,85467)
            HttpRuntime.CheckFilePermission(newPhysicalPath);
        }

        // If there is a filter, check whether it allows this include file
        if (_pageParserFilter != null && !_pageParserFilter.AllowServerSideInclude(newVirtualPath.VirtualPathString)) {
            ProcessError(SR.GetString(SR.Include_not_allowed, newVirtualPath));
        }

        // Parse the included file recursively
        ParseFile(newPhysicalPath, newVirtualPath);

        // Always ignore the spaces after an include directive
        flags[ignoreNextSpaceString] = true;
    }

    private static char[] s_newlineChars = new char[] { '\r', '\n' };

    /*
     *  Handle <%= ... %>, <%# ... %> and <% ... %> blocks
     */
    private void ProcessCodeBlock(Match match, CodeBlockType blockType, string text) {

        // Take care of the previous literal string
        ProcessLiteral();

        // Get the piece of code
        Group codeGroup = match.Groups["code"];
        string code = codeGroup.Value;

        bool encode = match.Groups["encode"].Success;

        // Replace "%\>" with "%>" (ASURT 7175)
        code = code.Replace(@"%\>", "%>");

        int lineNumber = _lineNumber;
        int column = -1;

        if (blockType != CodeBlockType.Code) {

            // It a <%= %>, <%# %> or <%: %> block.  We need to do special handling of newline chars

            // If there are newlines in the beginning of the code string we need to get rid of them,
            // and adjust the line pragma accordingly.  This is needed because some compilers (like VB)
            // don't support multiline expression (ASURT 13662)
            int newlineIndex = -1;
            for (int i = 0; i < code.Length && Char.IsWhiteSpace(code[i]); i++) {
                if (code[i] == '\r' || (code[i] == '\n' && (i == 0 || code[i - 1] != '\r'))) {
                    lineNumber++;
                    newlineIndex = i;
                }
                else if (code[i] == '\n') {
                    newlineIndex = i;
                }
            }

            if (newlineIndex >= 0) {
                // If we found some newlines in the beginning, get rid of them.  Note
                // that we preserve the spaces, to get correct column information
                code = code.Substring(newlineIndex + 1);

                // The code starts at column 1 (since we keep the spaces)
                column = 1;
            }

            // Same deal for the end of the string: look for the first newline
            // after the last non-blank chararacter
            newlineIndex = -1;
            for (int i = code.Length - 1; i >= 0 && Char.IsWhiteSpace(code[i]); i--) {
                if (code[i] == '\r' || code[i] == '\n')
                    newlineIndex = i;
            }

            // And if we found one, remove it and everything after
            if (newlineIndex >= 0)
                code = code.Substring(0, newlineIndex);

            // Disallow empty expressions (ASURT 40124)
            // Do not treat as error in CBM. This is necessary so we still generate
            // code blocks for empty expressions. (VSWhidbey 406212)
            if (!IgnoreParseErrors && Util.IsWhiteSpaceString(code)) {
                ProcessError(SR.GetString(SR.Empty_expression));

                return;
            }
        }

        if (column < 0) {

            // It a <% %> block.  Newline chars are not a problem.

            // Look for the last newline before the code string
            int newlineIndex = text.LastIndexOfAny(s_newlineChars, codeGroup.Index-1);

            // Use it to calculate the column where the code starts,
            // which improves the debugging experience (VSWhidbey 87172)
            column = codeGroup.Index-newlineIndex;
            Debug.Assert(column > 0);
        }

        ControlBuilder builder = ((BuilderStackEntry) BuilderStack.Peek())._builder;
        ControlBuilder subBuilder;

        // First, give the PageParserFilter a chance to handle the code block
        if (!PageParserFilterProcessedCodeBlock(CodeConstructTypeFromCodeBlockType(blockType), code, lineNumber)) {

            // Make sure it's legal to have code in this page
            EnsureCodeAllowed();

            // Add the code block to the top builder
            subBuilder = new CodeBlockBuilder(blockType, code, lineNumber, column, CurrentVirtualPath, encode);

            AppendSubBuilder(builder, subBuilder);

            // Optionally record code block position data
            ParseRecorders.RecordCodeBlock(subBuilder, match);
        }

        // Always ignore the spaces after a <% ... %> block
        if (blockType == CodeBlockType.Code)
            flags[ignoreNextSpaceString] = true;
    }

    // Map a CodeBlockType to the equivalent CodeConstructType
    private static CodeConstructType CodeConstructTypeFromCodeBlockType(CodeBlockType blockType) {
        switch (blockType) {
            case CodeBlockType.Code:
                return CodeConstructType.CodeSnippet;
            case CodeBlockType.Expression:
                return CodeConstructType.ExpressionSnippet;
            case CodeBlockType.EncodedExpression:
                return CodeConstructType.EncodedExpressionSnippet;
            case CodeBlockType.DataBinding:
                return CodeConstructType.DataBindingSnippet;
            default:
                Debug.Assert(false);
                return CodeConstructType.CodeSnippet;
        }
    }

    private bool PageParserFilterProcessedCodeBlock(CodeConstructType codeConstructType,
        string code, int lineNumber) {

        // This requires a PageParserFilter, and CompilationMode must allow it
        if (_pageParserFilter == null || CompilationMode == CompilationMode.Never)
            return false;

        // Temporarily adjust the line number to match what's passed in.  This makes it correctly
        // point to the begining of <script> blocks rather than the end
        int restoreLineNumber = _lineNumber;
        _lineNumber = lineNumber;
        try {
            // Ask the PageParserFilter whether it wants to process it
            return _pageParserFilter.ProcessCodeConstruct(codeConstructType, code);
        }
        finally {
            _lineNumber = restoreLineNumber;
        }
    }

    internal bool PageParserFilterProcessedDataBindingAttribute(string controlId, string attributeName,
        string code) {

        // This requires a PageParserFilter, and CompilationMode must allow it
        if (_pageParserFilter == null || CompilationMode == CompilationMode.Never)
            return false;

        // Ask the PageParserFilter whether it wants to process it
        return _pageParserFilter.ProcessDataBindingAttribute(controlId, attributeName, code);
    }

    internal bool PageParserFilterProcessedEventHookupAttribute(string controlId, string eventName,
        string handlerName) {

        // This requires a PageParserFilter, and CompilationMode must allow it
        if (_pageParserFilter == null || CompilationMode == CompilationMode.Never)
            return false;

        // Ask the PageParserFilter whether it wants to process it
        return _pageParserFilter.ProcessEventHookup(controlId, eventName, handlerName); 
    }

    // Add a ControlBuilder in the tree at the current parser position
    internal void AddControl(Type type, IDictionary attributes) {
        ControlBuilder parentBuilder = ((BuilderStackEntry)BuilderStack.Peek())._builder;

        ControlBuilder subBuilder = ControlBuilder.CreateBuilderFromType(this, parentBuilder,
            type, null, null, attributes, _lineNumber,
            CurrentVirtualPath.VirtualPathString);

        AppendSubBuilder(parentBuilder, subBuilder);
    }

    /*
     * Adds attributes and their values to the attribs
     * Sets the _id and isServerTag data members as appropriate.
     * If fDirective is true, we are being called for a <%@ %> block, in
     * which case the name of the directive is returned (e.g. "page")
     */
    private string ProcessAttributes(string text, Match match, out ParsedAttributeCollection attribs,
                                     bool fDirective, out string duplicateAttribute) {
        string directiveName = String.Empty;
        attribs = CreateEmptyAttributeBag();;
        CaptureCollection attrnames = match.Groups["attrname"].Captures;
        CaptureCollection attrvalues = match.Groups["attrval"].Captures;
        CaptureCollection equalsign = null;
        if (fDirective)
            equalsign = match.Groups["equal"].Captures;

        flags[isServerTag] = false;
        _id = null;

        duplicateAttribute = null;

        for (int i = 0; i < attrnames.Count; i++) {
            string attribName = attrnames[i].ToString();

            // If processing a directive, set the attribute name to lower case for easier processing later
            if (fDirective)
                attribName = attribName.ToLower(CultureInfo.InvariantCulture);

            Capture attrValue = attrvalues[i];
            string attribValue = attrValue.ToString();

            // Any of the attributes could be filtered
            string realAttributeName = String.Empty;
            string filter = Util.ParsePropertyDeviceFilter(attribName, out realAttributeName);

            // Always HTML decode all attributes (ASURT 54544)
            attribValue = HttpUtility.HtmlDecode(attribValue);

            // If we're parsing a directive, check if there is an equal sign.
            bool fHasEqual = false;
            if (fDirective)
                fHasEqual = (equalsign[i].ToString().Length > 0);

            // If this is a server ID, remember it
            // 

            if (StringUtil.EqualsIgnoreCase(realAttributeName, "id")) {
                _id = attribValue;
            }
            else if (StringUtil.EqualsIgnoreCase(realAttributeName, "runat")) {
                // Make sure no device filter or resource expression was specified (VSWhidbey 85325)
                ValidateBuiltInAttribute(filter, realAttributeName, attribValue);

                // Only runat=server is valid
                if (!StringUtil.EqualsIgnoreCase(attribValue, "server")) {
                    ProcessError(SR.GetString(SR.Runat_can_only_be_server));
                }

                // Set a flag if we see runat=server
                flags[isServerTag] = true;
                attribName = null;       // Don't put it in attribute bag
            }
            else if (FInDesigner && StringUtil.EqualsIgnoreCase(realAttributeName, "ignoreParentFrozen")) {
                // VSWhidbey 537398: "ignoreParentFrozen" is a special expando used in Venus. Ideally
                // Venus would hide the expando altogether but that's not practical in Whidbey.
                attribName = null;
            }

            if (attribName != null) {
                // A <%@ %> block can have two formats:
                // <%@ directive foo=1 bar=hello %>
                // <%@ foo=1 bar=hello %>
                // Check if we have the first format
                if (fDirective && !fHasEqual && i==0) {
                    directiveName = attribName;
                    if (string.Compare(directiveName, DefaultDirectiveName,
                        StringComparison.OrdinalIgnoreCase) == 0) {
                        directiveName = String.Empty;
                    }
                    continue;
                }

                try {
                    // Don't allow filters in directive other than the main one
                    if (fDirective && directiveName.Length > 0 && filter.Length > 0) {
                        ProcessError(SR.GetString(SR.Device_unsupported_in_directive, directiveName));

                        continue;
                    }

                    attribs.AddFilteredAttribute(filter, realAttributeName, attribValue);
                    //Since the attribute column values are only used for generating line pragmas at design time for intellisense to work,
                    //we populate that only at design time so that runtime memory usage is not affected.
                    if (BuildManagerHost.InClientBuildManager) {
                        //Linenumber = Linenumber of tag beginning + new lines between tag beginning and attribute value beginning.
                        //Column = number of characters from the last new line (before the attrValue in the entire Text) till attrValue.
                        int lineNumber = _lineNumber + Util.LineCount(text, match.Index, attrValue.Index);
                        int column = attrValue.Index - text.LastIndexOfAny(s_newlineChars, attrValue.Index - 1);
                        attribs.AddAttributeValuePositionInformation(realAttributeName, lineNumber, column);
                    }
                }
                catch (ArgumentException) {
                    // Duplicate attribute.  We can't throw until we find out if
                    // it's a server side tag (ASURT 51273)
                    duplicateAttribute = attribName;
                }
                catch (Exception ex) {
                    ProcessException(ex);
                }
            }
        }

        if (duplicateAttribute != null && fDirective) {
            ProcessError(SR.GetString(SR.Duplicate_attr_in_directive, duplicateAttribute));
        }

        return directiveName;
    }

    private static ParsedAttributeCollection CreateEmptyAttributeBag() {
        // use a ParsedAttributeCollection to preserve the order and store filtered information
        return new ParsedAttributeCollection();
    }

    private bool MaybeTerminateControl(string tagName, Match match) {

        BuilderStackEntry stackEntry = (BuilderStackEntry) BuilderStack.Peek();
        ControlBuilder builder = stackEntry._builder;

        // If the tag doesn't match, return false
        if (stackEntry._tagName == null || !StringUtil.EqualsIgnoreCase(stackEntry._tagName, tagName)) {
            return false;
        }

        // If the repeat count is non-zero, just decrease it
        if (stackEntry._repeatCount > 0) {
            stackEntry._repeatCount--;
            return false;
        }

        // Take care of the previous literal string
        ProcessLiteral();

        // If the builder wants the raw text of the tag, give it to it
        if (builder.NeedsTagInnerText()) {
            try {
                builder.SetTagInnerText(stackEntry._inputText.Substring(
                      stackEntry._textPos,
                      match.Index - stackEntry._textPos));
            }
            catch (Exception e) {
                if (!IgnoreParseErrors) {
                    // Reset the line number to the beginning of the tag if there is an error
                    _lineNumber = builder.Line;
                    ProcessException(e);

                    return true;
                }
            }
        }

        // If it's ending a template, pop the idList (ASURT 72773)
        if (builder is TemplateBuilder && ((TemplateBuilder)builder).AllowMultipleInstances)
            _idList = (StringSet) _idListStack.Pop();

        // Pop the top entry from the stack
        _builderStack.Pop();

        // Give the builder to its parent
        AppendSubBuilder(((BuilderStackEntry) _builderStack.Peek())._builder, builder);

        // Tell the builder that we're done parsing its control
        builder.CloseControl();

        // Optionally record end tag position data
        ParseRecorders.RecordEndTag(builder, match);

        return true;
    }

    /*
     * Map a type name to a Type.
     */
    internal Type MapStringToType(string typeName, IDictionary attribs) {
        return RootBuilder.GetChildControlType(typeName, attribs);
    }

    /*
     * Add a file as a dependency of the file we're parsing
     */
    internal void AddSourceDependency(VirtualPath fileName) {

        // Tell the filter that a dependency was added
        if (_pageParserFilter != null) {
            _pageParserFilter.OnDependencyAdded();
            _pageParserFilter.OnDirectDependencyAdded();
        }

        AddSourceDependency2(fileName);
    }

    /*
     * Add a file as a dependency of the file we're parsing
     */
    private void AddSourceDependency2(VirtualPath fileName) {
        if (_sourceDependencies == null)
            _sourceDependencies = new CaseInsensitiveStringSet();

        _sourceDependencies.Add(fileName.VirtualPathString);
    }

    /*
     * Add a BuildResult's source dependencies to our own source dependencies
     */
    internal void AddBuildResultDependency(BuildResult result) {

        // Add one direct dependency
        if (_pageParserFilter != null)
            _pageParserFilter.OnDirectDependencyAdded();

        if (result.VirtualPathDependencies == null)
            return;

        foreach (string virtualPath in result.VirtualPathDependencies) {

            // Add one dependency for each file (to include direct and indirect)
            if (_pageParserFilter != null)
                _pageParserFilter.OnDependencyAdded();

            AddSourceDependency2(VirtualPath.Create(virtualPath));
        }
    }

    /*
     * Add a type that we must 'link' with in order to build
     */
    internal void AddTypeDependency(Type type) {
        // We must link with all the types in the inheritance hierarchy (ASURT 83509)
        AddBaseTypeDependencies(type);

        // Add an import for the namespace of the type (if any)
        // Per ASURT 83942, only do this for namespaces we generate (e.g. ASP & _ASP)
        if (type.Namespace != null && BaseCodeDomTreeGenerator.IsAspNetNamespace(type.Namespace))
            AddImportEntry(type.Namespace);
    }

    /*
     * Add as dependencies all the assembly in the inheritance chain of a Type,
     * including interfaces.
     */
    private void AddBaseTypeDependencies(Type type) {
        Assembly a = type.Module.Assembly;

        // If the type is in a standard assembly, don't bother
        if (a == typeof(string).Assembly || a == typeof(Page).Assembly || a == typeof(Uri).Assembly)
            return;

        AddAssemblyDependency(a);

        // Recurse on the base Type
        if (type.BaseType != null)
            AddBaseTypeDependencies(type.BaseType);

        // Recurse on all the implemented interfaces
        Type[] interfaceTypes = type.GetInterfaces();
        foreach (Type interfaceType in interfaceTypes)
            AddBaseTypeDependencies(interfaceType);
    }

    /*
     * Add an assembly that we must 'link' with in order to build
     */
    internal Assembly AddAssemblyDependency(string assemblyName, bool addDependentAssemblies) {

        Assembly assembly = LoadAssembly(assemblyName, !FInDesigner /*throwOnFail*/);

        if (assembly != null)
            AddAssemblyDependency(assembly, addDependentAssemblies);

        return assembly;
    }
    internal Assembly AddAssemblyDependency(string assemblyName) {
        return AddAssemblyDependency(assemblyName, false /*addDependentAssemblies*/);
    }

    /*
     * Add an assembly that we must 'link' with in order to build
     */
    internal void AddAssemblyDependency(Assembly assembly, bool addDependentAssemblies) {
        if (_assemblyDependencies == null)
            _assemblyDependencies = new AssemblySet();

        if (_typeResolutionService != null)
            _typeResolutionService.ReferenceAssembly(assembly.GetName());

        _assemblyDependencies.Add(assembly);

        // If addDependentAssemblies is true, add its dependent assemblies as well
        if (addDependentAssemblies) {
            AssemblySet assemblyDependencies = Util.GetReferencedAssemblies(assembly);
            AddAssemblyDependencies(assemblyDependencies);
        }
    }
    internal void AddAssemblyDependency(Assembly assembly) {
        AddAssemblyDependency(assembly, false /*addDependentAssemblies*/);
    }

    /*
     * Add a set of assemblies that we must 'link' with in order to build
     */
    private void AddAssemblyDependencies(AssemblySet assemblyDependencies) {
        if (assemblyDependencies == null)
            return;

        foreach (Assembly a in assemblyDependencies)
            AddAssemblyDependency(a);
    }


    /// <internalonly/>
    ICollection IAssemblyDependencyParser.AssemblyDependencies {
        get {
            return AssemblyDependencies;
        }
    }

    internal IImplicitResourceProvider GetImplicitResourceProvider() {

        // 
        if (FInDesigner)
            return null;

        // If we already attempted to get them, return whatever we got
        if (flags[attemptedImplicitResources])
            return _implicitResourceProvider;

        flags[attemptedImplicitResources] = true;

        IResourceProvider resourceProvider = ResourceExpressionBuilder.GetLocalResourceProvider(_rootBuilder.VirtualPath);
        if (resourceProvider == null)
            return null;

        // If the resource provider is also an IImplicitResourceProvider, use that
        _implicitResourceProvider = resourceProvider as IImplicitResourceProvider;

        // Otherwise, use the default IImplicitResourceProvider implementation
        if (_implicitResourceProvider == null)
            _implicitResourceProvider = new DefaultImplicitResourceProvider(resourceProvider);

        return _implicitResourceProvider;
    }
}

/*
 * Base class for classes that contain source file & line information for error reporting
 */
internal abstract class SourceLineInfo {

    // Source file where the information appears
    private string _virtualPath;
    internal string VirtualPath {
        get { return _virtualPath;}
        set { _virtualPath = value;}
    }

    // Line number in the source file where the information appears
    private int _line;
    internal int Line {
        get { return _line;}
        set { _line = value;}
    }
}


/*
 * Objects that are placed on the BuilderStack
 */
internal class BuilderStackEntry: SourceLineInfo {
    internal BuilderStackEntry (ControlBuilder builder,
                       string tagName, string virtualPath, int line,
                       string inputText, int textPos) {

        _builder = builder;
        _tagName = tagName;
        VirtualPath = virtualPath;
        Line = line;
        _inputText = inputText;
        _textPos = textPos;
    }

    internal ControlBuilder _builder;
    internal string _tagName;

    // the input string that contains the tag
    internal string _inputText;

    // Offset in the input string of the beginning of the tag's contents
    internal int _textPos;

    // Used to deal with non server tags nested in server tag with the same name
    internal int _repeatCount;
}


/*
 * Entry representing an import directive.
 * e.g. <%@ import namespace="System.Web.UI" %>
 */
internal class NamespaceEntry: SourceLineInfo {
    private string _namespace;

    internal NamespaceEntry() {
    }

    internal string Namespace {
        get { return _namespace;}
        set { _namespace = value;}
    }
}

internal class ScriptBlockData: SourceLineInfo {
    protected string _script;

    internal ScriptBlockData(int line, int column, string virtualPath) {
        Line = line;
        Column = column;
        VirtualPath = virtualPath;
    }

    // Line number in the source file where the information appears
    private int _column;
    internal int Column {
        get { return _column;}
        set { _column = value;}
    }

    internal string Script {
        get { return _script;}
        set { _script = value;}
    }
}
}
