//------------------------------------------------------------------------------
// <copyright file="SimpleWebHandlerParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Implements the parser for simple web handler files
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI {

using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Runtime.Serialization;

using System;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;
using System.Web;
using System.Web.Hosting;
using System.Web.Caching;
using System.Web.Compilation;
using System.CodeDom;
using System.Web.Util;
using Debug=System.Web.Util.Debug;
using System.Web.RegularExpressions;
using System.Globalization;
using System.Security.Permissions;


/// <internalonly/>
/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public abstract class SimpleWebHandlerParser  : IAssemblyDependencyParser {
    private readonly static Regex directiveRegex = new SimpleDirectiveRegex();

    private SimpleHandlerBuildProvider _buildProvider;

    private TextReader _reader;

    private VirtualPath _virtualPath;

    // The line number in file currently being parsed
    private int _lineNumber;

    // The column number in file currently being parsed
    private int _startColumn;

    private bool _fFoundMainDirective;

    private string _typeName;
    internal string TypeName { 
        get { return _typeName; } 
    }

    private CompilerType _compilerType;

    // The string containing the code to be compiled
    private string _sourceString;

    // Assemblies to be linked with
    private AssemblySet _linkedAssemblies;

    // The set of assemblies that the build system is telling us we will be linked with
    private ICollection _referencedAssemblies;

    private static char[] s_newlineChars = new char[] { '\r', '\n' };

    private bool _ignoreParseErrors;
    internal bool IgnoreParseErrors {
        get { return _ignoreParseErrors; }
        set { _ignoreParseErrors = value; }
    }

    internal void SetBuildProvider(SimpleHandlerBuildProvider buildProvider) {
        _buildProvider = buildProvider;
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>

    // Only allowed in full trust (ASURT 124397)
    [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
    protected SimpleWebHandlerParser(HttpContext context, string virtualPath, string physicalPath) {

        // These obsolete parameters should never be set
        Debug.Assert(context == null);
        Debug.Assert(physicalPath == null);

        Debug.Assert(virtualPath != null);

        _virtualPath = VirtualPath.Create(virtualPath);
    }

    /*
     * Compile a web handler file into a Type.  Result is cached.
     */

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    protected Type GetCompiledTypeFromCache() {

        //
        // This method is practically useless, but cannot be removed to avoid a breaking change
        //

        BuildResultCompiledType result = (BuildResultCompiledType) BuildManager.GetVPathBuildResult(_virtualPath);

        return result.ResultType;
    }

    internal void Parse(ICollection referencedAssemblies) {

        _referencedAssemblies = referencedAssemblies;

        AddSourceDependency(_virtualPath);

        // Open a TextReader for the virtualPath we're parsing
        using (_reader = _buildProvider.OpenReaderInternal()) {
            ParseReader();
        }
    }

    internal CompilerType CompilerType { get { return _compilerType; } }

    internal ICollection AssemblyDependencies { get { return _linkedAssemblies; } }
    private StringSet _sourceDependencies;
    internal ICollection SourceDependencies { get { return _sourceDependencies; } }

    internal CodeCompileUnit GetCodeModel() {

        // Do we have something to compile?
        if (_sourceString == null)
            return null;

        CodeSnippetCompileUnit snippetCompileUnit = new CodeSnippetCompileUnit(_sourceString);

        // Put in some context so that the file can be debugged.
        snippetCompileUnit.LinePragma = BaseCodeDomTreeGenerator.CreateCodeLinePragmaHelper(
            _virtualPath.VirtualPathString, _lineNumber);

        return snippetCompileUnit;
    }

    internal IDictionary GetLinePragmasTable() {
        LinePragmaCodeInfo codeInfo = new LinePragmaCodeInfo();
        codeInfo._startLine = _lineNumber;
        codeInfo._startColumn = _startColumn;
        codeInfo._startGeneratedColumn = 1;
        codeInfo._codeLength = -1;
        codeInfo._isCodeNugget = false;
    
        IDictionary linePragmasTable = new Hashtable();
        linePragmasTable[_lineNumber] = codeInfo;
    
        return linePragmasTable;
    }

    internal bool HasInlineCode { get { return (_sourceString != null); } } 

    internal Type GetTypeToCache(Assembly builtAssembly) {
        Type t = null;

        // First, try to get the type from the assembly that has been built (if any)
        if (builtAssembly != null)
            t = builtAssembly.GetType(_typeName);

        // If not, try to get it from other assemblies
        if (t == null)
            t = GetType(_typeName);

        // Make sure the type derives from what we expect
        try {
            ValidateBaseType(t);
        }
        catch (Exception e) {
            throw new HttpParseException(e.Message, e, _virtualPath, _sourceString, _lineNumber);
        }

        return t;
    }

    internal virtual void ValidateBaseType(Type t) {
        // No restriction on the base type by default
    }

    /*
     * Parse the contents of the TextReader
     */
    private void ParseReader() {
        string s = _reader.ReadToEnd();

        try {
            ParseString(s);
        }
        catch (Exception e) {
            throw new HttpParseException(e.Message, e, _virtualPath, s, _lineNumber);
        }
    }

    /*
     * Parse the contents of the string
     */
    private void ParseString(string text) {
        int textPos = 0;
        Match match;
        _lineNumber = 1;

        // First, parse all the <%@ ... %> directives
        for (;;) {
            match = directiveRegex.Match(text, textPos);

            // Done with the directives?
            if (!match.Success)
                break;

            _lineNumber += Util.LineCount(text, textPos, match.Index);
            textPos = match.Index;

            // Get all the directives into a bag
            IDictionary directive = CollectionsUtil.CreateCaseInsensitiveSortedList();
            string directiveName = ProcessAttributes(match, directive);

            ProcessDirective(directiveName, directive);

            _lineNumber += Util.LineCount(text, textPos, match.Index + match.Length);
            textPos = match.Index + match.Length;

            int newlineIndex = text.LastIndexOfAny(s_newlineChars, textPos-1);
            _startColumn = textPos - newlineIndex;
        }

        if (!_fFoundMainDirective && !IgnoreParseErrors) {
            throw new HttpException(
                SR.GetString(SR.Missing_directive, DefaultDirectiveName));
        }

        // skip the directive
        string remainingText = text.Substring(textPos);

        // If there is something else in the file, it needs to be compiled
        if (!Util.IsWhiteSpaceString(remainingText))
            _sourceString = remainingText;
    }

    private string ProcessAttributes(Match match, IDictionary attribs) {
        string ret = String.Empty;
        CaptureCollection attrnames = match.Groups["attrname"].Captures;
        CaptureCollection attrvalues = match.Groups["attrval"].Captures;
        CaptureCollection equalsign = null;
        equalsign = match.Groups["equal"].Captures;

        for (int i = 0; i < attrnames.Count; i++) {
            string attribName = attrnames[i].ToString();
            string attribValue = attrvalues[i].ToString();

            // Check if there is an equal sign.
            bool fHasEqual = (equalsign[i].ToString().Length > 0);

            if (attribName != null) {
                // A <%@ %> block can have two formats:
                // <%@ directive foo=1 bar=hello %>
                // <%@ foo=1 bar=hello %>
                // Check if we have the first format
                if (!fHasEqual && i==0) {
                    ret = attribName;
                    continue;
                }

                try {
                    if (attribs != null)
                        attribs.Add(attribName, attribValue);
                }
                catch (ArgumentException) {

                    // Ignore the duplicate attributes when called from CBM
                    if (IgnoreParseErrors) continue;

                    throw new HttpException(
                        SR.GetString(SR.Duplicate_attr_in_tag, attribName));
                }
            }
        }

        return ret;
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    protected abstract string DefaultDirectiveName { get; }

    private static void ProcessCompilationParams(IDictionary directive, CompilerParameters compilParams) {
        bool fDebug = false;
        if (Util.GetAndRemoveBooleanAttribute(directive, "debug", ref fDebug))
            compilParams.IncludeDebugInformation = fDebug;

        if (compilParams.IncludeDebugInformation &&
            !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
            throw new HttpException(SR.GetString(SR.Insufficient_trust_for_attribute, "debug"));
        }

        int warningLevel=0;
        if (Util.GetAndRemoveNonNegativeIntegerAttribute(directive, "warninglevel", ref warningLevel)) {
            compilParams.WarningLevel = warningLevel;
            if (warningLevel > 0)
                compilParams.TreatWarningsAsErrors = true;
        }

        string compilerOptions = Util.GetAndRemoveNonEmptyAttribute(
            directive, "compileroptions");
        if (compilerOptions != null) {
            CompilationUtil.CheckCompilerOptionsAllowed(compilerOptions, false /*config*/, null, 0);
            compilParams.CompilerOptions = compilerOptions;
        }
    }

    /*
     * Process a <%@ %> block
     */
    internal virtual void ProcessDirective(string directiveName, IDictionary directive) {

        // Empty means default
        if (directiveName.Length == 0)
            directiveName = DefaultDirectiveName;

        // Check for the main directive
        if (IsMainDirective(directiveName)) {

            // Make sure the main directive was not already specified
            if (_fFoundMainDirective && !IgnoreParseErrors) {
                throw new HttpException(
                    SR.GetString(SR.Only_one_directive_allowed, DefaultDirectiveName));
            }

            _fFoundMainDirective = true;

            // Since description is a no op, just remove it if it's there
            directive.Remove("description");

            // Similarily, ignore 'codebehind' attribute (ASURT 4591)
            directive.Remove("codebehind");

            string language = Util.GetAndRemoveNonEmptyAttribute(directive, "language");

            // Get the compiler for the specified language (if any)
            if (language != null) {
                _compilerType = _buildProvider.GetDefaultCompilerTypeForLanguageInternal(language);
            }
            else {
                // Get a default from config
                _compilerType = _buildProvider.GetDefaultCompilerTypeInternal();
            }

            _typeName = Util.GetAndRemoveRequiredAttribute(directive, "class");

            if (_compilerType.CompilerParameters != null)
                ProcessCompilationParams(directive, _compilerType.CompilerParameters);
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "assembly")) {
            // Assembly directive

            // Remove the attributes as we get them from the dictionary
            string assemblyName = Util.GetAndRemoveNonEmptyAttribute(directive, "name");
            VirtualPath src = Util.GetAndRemoveVirtualPathAttribute(directive, "src");

            if (assemblyName != null && src != null && !IgnoreParseErrors) {
                throw new HttpException(
                    SR.GetString(SR.Attributes_mutually_exclusive, "Name", "Src"));
            }

            if (assemblyName != null) {
                AddAssemblyDependency(assemblyName);
            }
            // Is it a source file that needs to be compiled on the fly
            else if (src != null) {
                ImportSourceFile(src);
            }
            else if (!IgnoreParseErrors) {
                throw new HttpException(SR.GetString(SR.Missing_attr, "name"));
            }
        }
        else if (!IgnoreParseErrors) {
            throw new HttpException(
                SR.GetString(SR.Unknown_directive, directiveName));
        }

        // If there are some attributes left, fail
        Util.CheckUnknownDirectiveAttributes(directiveName, directive);
    }

    internal virtual bool IsMainDirective(string directiveName) {
        return (string.Compare(directiveName, DefaultDirectiveName,
            StringComparison.OrdinalIgnoreCase) == 0);
    }

    /*
     * Compile a source file into an assembly, and import it
     */
    private void ImportSourceFile(VirtualPath virtualPath) {

        // Get a full path to the source file
        VirtualPath baseVirtualDir = _virtualPath.Parent;
        VirtualPath fullVirtualPath = baseVirtualDir.Combine(virtualPath);

        // Add the source file to the list of files we depend on
        AddSourceDependency(fullVirtualPath);

        // 

        CompilationUtil.GetCompilerInfoFromVirtualPath(fullVirtualPath);

        // Compile it into an assembly

        BuildResultCompiledAssembly result = (BuildResultCompiledAssembly) BuildManager.GetVPathBuildResult(
            fullVirtualPath);
        Assembly a = result.ResultAssembly;

        // Add a dependency to the assembly
        AddAssemblyDependency(a);
    }

    /*
     * Add a file as a dependency for the DLL we're building
     */
    internal void AddSourceDependency(VirtualPath fileName) {
        if (_sourceDependencies == null)
            _sourceDependencies = new CaseInsensitiveStringSet();

        _sourceDependencies.Add(fileName.VirtualPathString);
    }

    private void AddAssemblyDependency(string assemblyName) {

        // Load and keep track of the assembly
        Assembly a = Assembly.Load(assemblyName);

        AddAssemblyDependency(a);
    }

    private void AddAssemblyDependency(Assembly assembly) {

        if (_linkedAssemblies == null)
            _linkedAssemblies = new AssemblySet();
        _linkedAssemblies.Add(assembly);
    }

    /*
     * Look for a type by name in the assemblies available to this page
     */
    private Type GetType(string typeName) {

        Type t;

        // If it contains an assembly name, just call Type.GetType (ASURT 53589)
        if (Util.TypeNameContainsAssembly(typeName)) {
            try {
                t = Type.GetType(typeName, true);
            }
            catch (Exception e) {
                throw new HttpParseException(null, e, _virtualPath, _sourceString, _lineNumber);
            }

            return t;
        }

        t = Util.GetTypeFromAssemblies(_referencedAssemblies, typeName, false /*ignoreCase*/);
        if (t != null)
            return t;

        t = Util.GetTypeFromAssemblies(_linkedAssemblies, typeName, false /*ignoreCase*/);
        if (t != null)
            return t;

        throw new HttpParseException(
            SR.GetString(SR.Could_not_create_type, typeName),
            null, _virtualPath, _sourceString, _lineNumber);
    }


    /// <internalonly/>
    ICollection IAssemblyDependencyParser.AssemblyDependencies {
        get {
            return AssemblyDependencies;
        }
    }
}


/// <internalonly/>
/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
internal class WebHandlerParser: SimpleWebHandlerParser {

    internal WebHandlerParser(string virtualPath)
        : base(null /*context*/, virtualPath, null /*physicalPath*/) {}


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    protected override string DefaultDirectiveName {
        get { return "webhandler"; }
    }

    internal override void ValidateBaseType(Type t) {
        // Make sure the type has the correct base class
        Util.CheckAssignableType(typeof(IHttpHandler), t);
    }
}


/// <internalonly/>
/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public class WebServiceParser: SimpleWebHandlerParser {


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>

    // Only allowed in full trust (ASURT 123890)
    [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
    public static Type GetCompiledType(string inputFile, HttpContext context) {

        // NOTE: the inputFile parameter should be named virtualPath, but cannot be changed
        // as it would be a minor breaking change! (VSWhidbey 80997).
        BuildResultCompiledType result = (BuildResultCompiledType) BuildManager.GetVPathBuildResult(
            context, VirtualPath.Create(inputFile));

        return result.ResultType;
    }

    internal WebServiceParser(string virtualPath)
        : base(null /*context*/, virtualPath, null /*physicalPath*/) { }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    protected override string DefaultDirectiveName {
        get { return "webservice"; }
    }
}

}
