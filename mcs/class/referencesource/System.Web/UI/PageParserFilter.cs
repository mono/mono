//------------------------------------------------------------------------------
// <copyright file="PageParserFilter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Abstract class implemented by objects that need to control the parsing of pages.
 *
 * Copyright (c) 2004 Microsoft Corporation
 */
namespace System.Web.UI {

using System.Globalization;
using System.Collections;
using System.Web.Configuration;
using System.Web.Compilation;
using System.Web.Util;
using System.Security.Permissions;

[AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Medium)]
[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Medium)]
public abstract class PageParserFilter {

    private VirtualPath _virtualPath;
    protected string VirtualPath { get { return _virtualPath.VirtualPathString; } }

    // The current line number being parsed
    private TemplateParser _parser;
    protected int Line { get { return _parser._lineNumber; } }

    // Dev10: 725898 WSS needs ability to detect whether ParseControl called from Page
    protected bool CalledFromParseControl { get; private set; }

    private int _numberOfControlsAllowed;
    private int _currentControlCount;

    private int _dependenciesAllowed;
    private int _currentDependenciesCount;

    private int _directDependenciesAllowed;
    private int _currentDirectDependenciesCount;

    // Create a PageParserFilter and initialize it
    internal static PageParserFilter Create(PagesSection pagesConfig, VirtualPath virtualPath, TemplateParser parser) {
        PageParserFilter pageParserFilter = pagesConfig.CreateControlTypeFilter();
        if (pageParserFilter != null)
            pageParserFilter.InitializeInternal(virtualPath, parser);

        return pageParserFilter;
    }

    internal void InitializeInternal(VirtualPath virtualPath, TemplateParser parser) {

        _parser = parser;
        Debug.Assert(_virtualPath == null);
        _virtualPath = virtualPath;

        Initialize();

        // Get the various limits we need to enforce
        _numberOfControlsAllowed = NumberOfControlsAllowed;

        // Add 1 to these two, because internally we count the file itself as a
        // dependency, but we don't want this to be reflected to the PageParserFilter
        // implementor (VSWhidbey 341708)
        _dependenciesAllowed = TotalNumberOfDependenciesAllowed+1;
        _directDependenciesAllowed = NumberOfDirectDependenciesAllowed+1;
        CalledFromParseControl = parser.flags[TemplateParser.calledFromParseControlFlag];
    }

    // initialize the filter to be used for a specific page
    protected virtual void Initialize() {
    }

    // Informs the filter that the parsing of the page is complete
    public virtual void ParseComplete(ControlBuilder rootBuilder) {
        Debug.Assert(_virtualPath != null);
    }

    // Allows the filter to return the compilation mode for the page.
    // If it doesn't want to modify it, it can just return current.
    public virtual CompilationMode GetCompilationMode(CompilationMode current) {
        return current;
    }

    // Indicates whether code is allowed on the page.  This method allows
    // forbidding code even on pages that will be compiled (for perf)
    public virtual bool AllowCode {
        get {
            return false;
        }
    }

    // Is the control Type allowed for this page
    internal bool AllowControlInternal(Type controlType, ControlBuilder builder) {

        OnControlAdded();

        return AllowControl(controlType, builder);
    }

    // Is the control Type allowed for this page
    public virtual bool AllowControl(Type controlType, ControlBuilder builder) {
        return false;
    }

    // Is this base type allowed for this page
    public virtual bool AllowBaseType(Type baseType) {
        return false;
    }

    internal bool AllowVirtualReference(CompilationSection compConfig, VirtualPath referenceVirtualPath) {

        // Get the extension, and from it the type of the BuildProvider
        string extension = referenceVirtualPath.Extension;
        Type buildProviderType = CompilationUtil.GetBuildProviderTypeFromExtension(
            compConfig, extension, BuildProviderAppliesTo.Web, false /*failIfUnknown*/);

        // If it's an unknown type, block it
        if (buildProviderType == null)
            return false;

        // Figure out the VirtualReferenceType based on the BuildProvider type
        VirtualReferenceType referenceType;
        if (buildProviderType == typeof(PageBuildProvider))
            referenceType = VirtualReferenceType.Page;
        else if (buildProviderType == typeof(UserControlBuildProvider))
            referenceType = VirtualReferenceType.UserControl;
        else if (buildProviderType == typeof(MasterPageBuildProvider))
            referenceType = VirtualReferenceType.Master;
        else if (buildProviderType == typeof(SourceFileBuildProvider))
            referenceType = VirtualReferenceType.SourceFile;
        else
            referenceType = VirtualReferenceType.Other;

        return AllowVirtualReference(referenceVirtualPath.VirtualPathString, referenceType);
    }

    // Is the virtual path reference allowed in this page.  The referenceType
    // indicates the type of references involved.
    public virtual bool AllowVirtualReference(string referenceVirtualPath, VirtualReferenceType referenceType) {
        return false;
    }

    // Is the passed in server include (<!-- #include -->) allowed
    public virtual bool AllowServerSideInclude(string includeVirtualPath) {
        return false;
    }

    public virtual void PreprocessDirective(string directiveName, IDictionary attributes) { }

    public virtual int NumberOfControlsAllowed {
        get {
            // By default, don't allow any
            return 0;
        }
    }

    public virtual int TotalNumberOfDependenciesAllowed {
        get {
            // By default, don't allow any
            return 0;
        }
    }

    public virtual int NumberOfDirectDependenciesAllowed {
        get {
            // By default, don't allow any
            return 0;
        }
    }

    private void OnControlAdded() {

        // If it's negative, there is no limit
        if (_numberOfControlsAllowed < 0)
            return;

        // Increase the control count
        _currentControlCount++;

        // Fail if the limit has been reached
        if (_currentControlCount > _numberOfControlsAllowed) {
            throw new HttpException(SR.GetString(SR.Too_many_controls, _numberOfControlsAllowed.ToString(CultureInfo.CurrentCulture)));
        }
    }

    // Called by the parser when a file dependency (direct or indirect) is added
    internal void OnDependencyAdded() {

        // If it's negative, there is no limit
        if (_dependenciesAllowed <= 0)
            return;

        // Increase the dependency count
        _currentDependenciesCount++;

        // Fail if the limit has been reached
        if (_currentDependenciesCount > _dependenciesAllowed) {
            throw new HttpException(SR.GetString(SR.Too_many_dependencies, VirtualPath,
                _dependenciesAllowed.ToString(CultureInfo.CurrentCulture)));
        }
    }

    // Called by the parser when a direct file dependency is added
    internal void OnDirectDependencyAdded() {

        // If it's negative, there is no limit
        if (_directDependenciesAllowed <= 0)
            return;

        // Increase the direct dependency count
        _currentDirectDependenciesCount++;

        // Fail if the limit has been reached
        if (_currentDirectDependenciesCount > _directDependenciesAllowed) {
            throw new HttpException(SR.GetString(SR.Too_many_direct_dependencies, VirtualPath,
                _directDependenciesAllowed.ToString(CultureInfo.CurrentCulture)));
        }
    }

    // Give the filter a chance to process a code block.  If it returns true, the
    // code block is not processed further by the parser
    public virtual bool ProcessCodeConstruct(CodeConstructType codeType, string code) {
        return false;
    }

    // Give the filter a chance to process a databinding attribute (e.g. Text=<%# expr %>)
    // If it returns true, the databinding attribute is not processed further by the parser
    public virtual bool ProcessDataBindingAttribute(string controlId, string name, string value) {
        return false;
    }

    // Give the filter a chance to process an event hookup (e.g. onclick="ClickHandler")
    // If it returns true, the event hookup is not processed further by the parser
    public virtual bool ProcessEventHookup(string controlId, string eventName, string handlerName) {
        return false;
    }

    // Return the Type that should be used for NoCompile user controls
    public virtual Type GetNoCompileUserControlType() {
        return null;
    }

    // Add a ControlBuilder in the tree at the current parser position
    protected void AddControl(Type type, IDictionary attributes) {
        _parser.AddControl(type, attributes);
    }

    // Set a property on the TemplateControl (Page/UserControl/Master)
    protected void SetPageProperty(string filter, string name, string value) {
        if (filter == null)
            filter = String.Empty;

        _parser.RootBuilder.PreprocessAttribute(filter, name, value, true /*mainDirectiveMode*/);
    }
}

// The type of reference passed to PageParserFilter.AllowVirtualReference
public enum VirtualReferenceType {
    Page,
    UserControl,
    Master,
    SourceFile,
    Other
}

// Used as parameter to the PageParserFilter.ProcessCodeConstruct API
public enum CodeConstructType {
    CodeSnippet,            // <% ... %>
    ExpressionSnippet,      // <%= ... %>
    DataBindingSnippet,     // <%# ... %>
    ScriptTag,              // <script runat="server">...</script>
    EncodedExpressionSnippet // <%: ... %>
}
}
