//------------------------------------------------------------------------------
// <copyright file="PageParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Implements the ASP.NET template parser
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Globalization;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Web.Hosting;
using System.Web.Caching;
using System.Web.Util;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Management;
using System.EnterpriseServices;
using HttpException = System.Web.HttpException;
using System.Text.RegularExpressions;
using System.Security.Permissions;

/*
 * Parser for .aspx files
 */

/// <internalonly/>
/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public sealed class PageParser : TemplateControlParser {

    private int _transactionMode = 0 /*TransactionOption.Disabled*/;
    internal int TransactionMode { get { return _transactionMode; } }

    private TraceMode _traceMode = System.Web.TraceMode.Default;
    internal TraceMode TraceMode { get { return _traceMode; } }

    private TraceEnable _traceEnabled = TraceEnable.Default;
    internal TraceEnable TraceEnabled { get { return _traceEnabled; } }

    private int _codePage;
    private string _responseEncoding;
    private int _lcid;
    private string _culture;
    private int _mainDirectiveLineNumber = 1;
    private bool _mainDirectiveMasterPageSet;

    private OutputCacheLocation _outputCacheLocation;

    internal bool FRequiresSessionState { get { return flags[requiresSessionState]; } }
    internal bool FReadOnlySessionState { get { return flags[readOnlySessionState]; } }

    private string _errorPage;

    private string _styleSheetTheme;
    internal String StyleSheetTheme { get { return _styleSheetTheme; } }

    internal bool AspCompatMode { get { return flags[aspCompatMode]; } }

    internal bool AsyncMode { get { return flags[asyncMode]; } }

    internal bool ValidateRequest { get { return flags[validateRequest]; } }

    private Type _previousPageType;
    internal Type PreviousPageType { get { return _previousPageType; } }

    private Type _masterPageType;
    internal Type MasterPageType { get { return _masterPageType; } }

    private string _configMasterPageFile;


    public PageParser() {
        flags[buffer] = Page.BufferDefault;
        flags[requiresSessionState] = true;
        flags[validateRequest] = true;
    }

    /*
     * Compile an .aspx file into a Page object
     */

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>

    private static object s_lock = new object();


    // Only allowed in full trust (ASURT 123086)
    [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
    public static IHttpHandler GetCompiledPageInstance(string virtualPath,
        string inputFile, HttpContext context) {

        // Canonicalize the path to avoid failure caused by the CheckSuspiciousPhysicalPath
        // security check, which was not meant to apply to this scenario (plus this API requires
        // full trust).  VSWhidbey 541640.
        if (!String.IsNullOrEmpty(inputFile)) {
            inputFile = Path.GetFullPath(inputFile);
        }

        return GetCompiledPageInstance(VirtualPath.Create(virtualPath), inputFile, context);
    }

    private static IHttpHandler GetCompiledPageInstance(VirtualPath virtualPath,
        string inputFile, HttpContext context) {

        // This is a hacky API that only exists to support web service's
        // DefaultWsdlHelpGenerator.aspx, which doesn't live under the app root.
        // To make this work, we add an explicit mapping from the virtual path
        // to the stream of the passed in file

        // Make it relative to the current request if necessary
        if (context != null)
            virtualPath = context.Request.FilePathObject.Combine(virtualPath);

        object virtualPathToFileMappingState = null;
        try {
            try {
                // If there is a physical path, we need to connect the virtual path to it, so that
                // the build system will use the right input file for the virtual path.
                if (inputFile != null) {
                    virtualPathToFileMappingState = HostingEnvironment.AddVirtualPathToFileMapping(
                        virtualPath, inputFile);
                }

                BuildResultCompiledType result = (BuildResultCompiledType)BuildManager.GetVPathBuildResult(
                    context, virtualPath, false /*noBuild*/, true /*allowCrossApp*/, true /*allowBuildInPrecompile*/);
                return (IHttpHandler)HttpRuntime.CreatePublicInstance(result.ResultType);
            }
            finally {
                if (virtualPathToFileMappingState != null)
                    HostingEnvironment.ClearVirtualPathToFileMapping(virtualPathToFileMappingState);
            }
        }
        catch {
            throw;
        }
    }

    internal override Type DefaultBaseType { get { return typeof(System.Web.UI.Page); } }

    internal override Type DefaultFileLevelBuilderType {
        get {
            return typeof(FileLevelPageControlBuilder);
        }
    }

    internal override RootBuilder CreateDefaultFileLevelBuilder() {

        return new FileLevelPageControlBuilder();
    }

    private void EnsureMasterPageFileFromConfigApplied() {
        // Skip if it's already applied.
        if (_mainDirectiveMasterPageSet) {
            return;
        }

        // If the masterPageFile is defined in the config
        if (_configMasterPageFile != null) {

            // Readjust the lineNumber to the location of maindirective
            int prevLineNumber = _lineNumber;
            _lineNumber = _mainDirectiveLineNumber;
            try {
                if (_configMasterPageFile.Length > 0) {
                    Type type = GetReferencedType(_configMasterPageFile);

                    // Make sure it has the correct base type
                    if (!typeof(MasterPage).IsAssignableFrom(type)) {
                        ProcessError(SR.GetString(SR.Invalid_master_base, _configMasterPageFile));
                    }
                }

                if (((FileLevelPageControlBuilder)RootBuilder).ContentBuilderEntries != null) {
                    RootBuilder.SetControlType(BaseType);
                    RootBuilder.PreprocessAttribute(String.Empty /*filter*/, "MasterPageFile", _configMasterPageFile, true /*mainDirectiveMode*/);
                }
            }
            finally {
                _lineNumber = prevLineNumber;
            }
        }

        _mainDirectiveMasterPageSet = true;
    }

    internal override void HandlePostParse() {
        base.HandlePostParse();

        EnsureMasterPageFileFromConfigApplied();
    }

    // Get default settings from config
    internal override void ProcessConfigSettings() {
        base.ProcessConfigSettings();

        if (PagesConfig != null) {
            // Check config for various attributes, and if they have non-default values,
            // set them in _mainDirectiveConfigSettings.

            if (PagesConfig.Buffer != Page.BufferDefault)
                _mainDirectiveConfigSettings["buffer"] = Util.GetStringFromBool(PagesConfig.Buffer);

            if (PagesConfig.EnableViewStateMac != Page.EnableViewStateMacDefault)
                _mainDirectiveConfigSettings["enableviewstatemac"] = Util.GetStringFromBool(PagesConfig.EnableViewStateMac);

            if (PagesConfig.EnableEventValidation != Page.EnableEventValidationDefault)
                _mainDirectiveConfigSettings["enableEventValidation"] = Util.GetStringFromBool(PagesConfig.EnableEventValidation);

            if (PagesConfig.SmartNavigation != Page.SmartNavigationDefault)
                _mainDirectiveConfigSettings["smartnavigation"] = Util.GetStringFromBool(PagesConfig.SmartNavigation);

            if (PagesConfig.ThemeInternal != null && PagesConfig.Theme.Length != 0)
                _mainDirectiveConfigSettings["theme"] = PagesConfig.Theme;

            if (PagesConfig.StyleSheetThemeInternal != null && PagesConfig.StyleSheetThemeInternal.Length != 0)
                _mainDirectiveConfigSettings["stylesheettheme"] = PagesConfig.StyleSheetThemeInternal;

            if (PagesConfig.MasterPageFileInternal != null && PagesConfig.MasterPageFileInternal.Length != 0) {
                _configMasterPageFile = PagesConfig.MasterPageFileInternal;
            }

            if (PagesConfig.ViewStateEncryptionMode != Page.EncryptionModeDefault) {
                _mainDirectiveConfigSettings["viewStateEncryptionMode"] = Enum.Format(typeof(ViewStateEncryptionMode), PagesConfig.ViewStateEncryptionMode, "G");
            }

            if (PagesConfig.MaintainScrollPositionOnPostBack != Page.MaintainScrollPositionOnPostBackDefault) {
                _mainDirectiveConfigSettings["maintainScrollPositionOnPostBack"] = Util.GetStringFromBool(PagesConfig.MaintainScrollPositionOnPostBack);
            }

            if (PagesConfig.MaxPageStateFieldLength != Page.DefaultMaxPageStateFieldLength) {
                _mainDirectiveConfigSettings["maxPageStateFieldLength"] = PagesConfig.MaxPageStateFieldLength;
            }

            flags[requiresSessionState] = ((PagesConfig.EnableSessionState == PagesEnableSessionState.True) || (PagesConfig.EnableSessionState == PagesEnableSessionState.ReadOnly));
            flags[readOnlySessionState] = (PagesConfig.EnableSessionState == PagesEnableSessionState.ReadOnly);
            flags[validateRequest] = PagesConfig.ValidateRequest;
            flags[aspCompatMode] = HttpRuntime.ApartmentThreading;
        }

        ApplyBaseType();
    }

    private void ApplyBaseType() {
        if (DefaultPageBaseType != null) {
            BaseType = DefaultPageBaseType;
        }
        else if (PagesConfig != null && PagesConfig.PageBaseTypeInternal != null) {
            BaseType = PagesConfig.PageBaseTypeInternal;
        }
    }

    internal override void ProcessDirective(string directiveName, IDictionary directive) {

        if (StringUtil.EqualsIgnoreCase(directiveName, "previousPageType")) {

            if (_previousPageType != null) {
                ProcessError(SR.GetString(SR.Only_one_directive_allowed, directiveName));
                return;
            }

            _previousPageType = GetDirectiveType(directive, directiveName);
            Util.CheckAssignableType(typeof(Page), _previousPageType);
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "masterType")) {

            if (_masterPageType != null) {
                ProcessError(SR.GetString(SR.Only_one_directive_allowed, directiveName));
                return;
            }

            _masterPageType = GetDirectiveType(directive, directiveName);
            Util.CheckAssignableType(typeof(MasterPage), _masterPageType);
        }
        else {
            base.ProcessDirective(directiveName, directive);
        }
    }

    // Override to get the location of maindirective.
    internal override void ProcessMainDirective(IDictionary mainDirective) {
        // Remember the location of the main directive.
        _mainDirectiveLineNumber = _lineNumber;

        base.ProcessMainDirective(mainDirective);
    }

    internal override bool ProcessMainDirectiveAttribute(string deviceName, string name,
        string value, IDictionary parseData) {
        
        switch (name) {

        case "errorpage":
            _errorPage = Util.GetNonEmptyAttribute(name, value);

            // Return false to let the generic attribute processing continue
            return false;

        case "contenttype":
            // Check validity
            Util.GetNonEmptyAttribute(name, value);

            // Return false to let the generic attribute processing continue
            return false;

        case "theme":
            if (IsExpressionBuilderValue(value)) {
                return false;
            }

            // Check validity
            Util.CheckThemeAttribute(value);

            // Return false to let the generic attribute processing continue
            return false;

        case "stylesheettheme":
            // Make sure no device filter or expression builder was specified
            ValidateBuiltInAttribute(deviceName, name, value);

            // Check validity
            Util.CheckThemeAttribute(value);

            _styleSheetTheme = value;
            return true;

        case "enablesessionstate":
            flags[requiresSessionState] = true;
            flags[readOnlySessionState] = false;
            if (Util.IsFalseString(value)) {
                flags[requiresSessionState] = false;
            }
            else if (StringUtil.EqualsIgnoreCase(value, "readonly")) {
                flags[readOnlySessionState] = true;
            }
            else if (!Util.IsTrueString(value)) {
                ProcessError(SR.GetString(SR.Enablesessionstate_must_be_true_false_or_readonly));
            }

            if (flags[requiresSessionState]) {
                // Session state is only available for compiled pages
                OnFoundAttributeRequiringCompilation(name);
            }
            break;

        case "culture":
            _culture = Util.GetNonEmptyAttribute(name, value);

            // Setting culture requires medium permission
            if (!HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                throw new HttpException(SR.GetString(SR.Insufficient_trust_for_attribute, "culture"));
            }

            //do not verify at parse time if potentially using browser AutoDetect
            if(StringUtil.EqualsIgnoreCase(value, HttpApplication.AutoCulture)) {
                return false;
            }


            // Create a CultureInfo just to verify validity
            CultureInfo cultureInfo;

            try {
                if(StringUtil.StringStartsWithIgnoreCase(value, HttpApplication.AutoCulture)) {
                    //safe to trim leading "auto:", string used elsewhere for null check
                    _culture = _culture.Substring(5);
                }
                cultureInfo = HttpServerUtility.CreateReadOnlyCultureInfo(_culture);
            }
            catch {
                ProcessError(SR.GetString(SR.Invalid_attribute_value, _culture, "culture"));
                return false;
            }

            // Don't allow neutral cultures (ASURT 77930)
            if (cultureInfo.IsNeutralCulture) {
                ProcessError(SR.GetString(SR.Invalid_culture_attribute,
                        Util.GetSpecificCulturesFormattedList(cultureInfo)));
            }

            // Return false to let the generic attribute processing continue
            return false;

        case "lcid":
            // Skip validity check for expression builder (e.g. <%$ ... %>)
            if (IsExpressionBuilderValue(value)) return false;

            _lcid = Util.GetNonNegativeIntegerAttribute(name, value);

            // Create a CultureInfo just to verify validity
            try {
                HttpServerUtility.CreateReadOnlyCultureInfo(_lcid);
            }
            catch {
                ProcessError(SR.GetString(SR.Invalid_attribute_value,
                    _lcid.ToString(CultureInfo.InvariantCulture), "lcid"));
            }

            // Return false to let the generic attribute processing continue
            return false;

        case "uiculture":
            // Check validity
            Util.GetNonEmptyAttribute(name, value);

            // Return false to let the generic attribute processing continue
            return false;

        case "responseencoding":
            // Skip validity check for expression builder (e.g. <%$ ... %>)
            if (IsExpressionBuilderValue(value)) return false;

            _responseEncoding = Util.GetNonEmptyAttribute(name, value);

            // Call Encoding.GetEncoding just to verify validity
            Encoding.GetEncoding(_responseEncoding);

            // Return false to let the generic attribute processing continue
            return false;

        case "codepage":
            // Skip validity check for expression builder (e.g. <%$ ... %>)
            if (IsExpressionBuilderValue(value)) return false;

            _codePage = Util.GetNonNegativeIntegerAttribute(name, value);

            // Call Encoding.GetEncoding just to verify validity
            Encoding.GetEncoding(_codePage);

            // Return false to let the generic attribute processing continue
            return false;

        case "transaction":
            // This only makes sense for compiled pages
            OnFoundAttributeRequiringCompilation(name);

            ParseTransactionAttribute(name, value);
            break;

        case "aspcompat":
            // This only makes sense for compiled pages
            OnFoundAttributeRequiringCompilation(name);

            flags[aspCompatMode] = Util.GetBooleanAttribute(name, value);

            // Only allow the use of aspcompat when we have UnmanagedCode access (ASURT 76694)
            if (flags[aspCompatMode] && !HttpRuntime.HasUnmanagedPermission()) {
                throw new HttpException(SR.GetString(SR.Insufficient_trust_for_attribute, "AspCompat"));
            }

            break;

        case "async":
            // This only makes sense for compiled pages
            OnFoundAttributeRequiringCompilation(name);

            flags[asyncMode] = Util.GetBooleanAttribute(name, value);

            // Async requires Medium trust
            if (!HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                throw new HttpException(SR.GetString(SR.Insufficient_trust_for_attribute, "async"));
            }

            break;

        case "tracemode":
            // We use TraceModeInternal instead of TraceMode to disallow the 'default' value (ASURT 75783)
            object tmpObj = Util.GetEnumAttribute(name, value, typeof(TraceModeInternal));
            _traceMode = (TraceMode) tmpObj;
            break;

        case "trace":
            bool traceEnabled = Util.GetBooleanAttribute(name, value);
            if (traceEnabled)
                _traceEnabled = TraceEnable.Enable;
            else
                _traceEnabled = TraceEnable.Disable;
            break;

        case "smartnavigation":
            // Make sure no device filter or expression builder was specified, since it doesn't make much
            // sense for smartnav (which only works on IE5.5+) (VSWhidbey 85876)
            ValidateBuiltInAttribute(deviceName, name, value);

            // Ignore it if it has default value.  Otherwise, let the generic
            // attribute processing continue
            bool smartNavigation = Util.GetBooleanAttribute(name, value);
            return (smartNavigation == Page.SmartNavigationDefault);

        case "maintainscrollpositiononpostback":
            bool maintainScrollPosition = Util.GetBooleanAttribute(name, value);
            return (maintainScrollPosition == Page.MaintainScrollPositionOnPostBackDefault);

        case "validaterequest":
            flags[validateRequest] = Util.GetBooleanAttribute(name, value);
            break;

        case "clienttarget":
            // Skip validity check for expression builder (e.g. <%$ ... %>)
            if (IsExpressionBuilderValue(value)) return false;

            // Check validity
            HttpCapabilitiesDefaultProvider.GetUserAgentFromClientTarget(CurrentVirtualPath, value);

            // Return false to let the generic attribute processing continue
            return false;

        case "masterpagefile":
            // Skip validity check for expression builder (e.g. <%$ ... %>)
            if (IsExpressionBuilderValue(value)) return false;

            if (value.Length > 0) {
                // Add dependency on the Type by calling this method
                Type type = GetReferencedType(value);

                // Make sure it has the correct base type
                if (!typeof(MasterPage).IsAssignableFrom(type)) {
                    ProcessError(SR.GetString(SR.Invalid_master_base, value));
                }

                if (deviceName.Length > 0) {
                    // Make sure the masterPageFile definition from config
                    // is applied before filtered masterPageFile attributes.
                    EnsureMasterPageFileFromConfigApplied();
                }
            }

            //VSWhidbey 479064 Remember the masterPageFile had been set even if it's empty string
            _mainDirectiveMasterPageSet = true;

            // Return false to let the generic attribute processing continue
            return false;

        default:
            // We didn't handle the attribute.  Try the base class
            return base.ProcessMainDirectiveAttribute(deviceName, name, value, parseData);
        }

        // The attribute was handled

        // Make sure no device filter or resource expression was specified
        ValidateBuiltInAttribute(deviceName, name, value);

        return true;
    }

    internal override void ProcessUnknownMainDirectiveAttribute(string filter, string attribName, string value) {

        // asynctimeout is in seconds while the corresponding public page property is a timespan
        // this requires a patch-up
        if (attribName == "asynctimeout") {
            int timeoutInSeconds = Util.GetNonNegativeIntegerAttribute(attribName, value);
            value = (new TimeSpan(0, 0, timeoutInSeconds)).ToString();
        }

        base.ProcessUnknownMainDirectiveAttribute(filter, attribName, value);
    }

    internal override void PostProcessMainDirectiveAttributes(IDictionary parseData) {

        // Can't have an error page if buffering is off
        if (!flags[buffer] && _errorPage != null) {
            ProcessError(SR.GetString(SR.Error_page_not_supported_when_buffering_off));
            return;
        }

        if (_culture != null && _lcid > 0) {
            ProcessError(SR.GetString(SR.Attributes_mutually_exclusive, "Culture", "LCID"));
            return;
        }

        if (_responseEncoding != null && _codePage > 0) {
            ProcessError(SR.GetString(SR.Attributes_mutually_exclusive, "ResponseEncoding", "CodePage"));
            return;
        }

        // async can't be combined with aspcompat
        if (AsyncMode && AspCompatMode) {
            ProcessError(SR.GetString(SR.Async_and_aspcompat));
            return;
        }

        // async can't be combined with transactions
        if (AsyncMode && _transactionMode != 0) {
            ProcessError(SR.GetString(SR.Async_and_transaction));
            return;
        }

        // Let the base class do its post processing
        base.PostProcessMainDirectiveAttributes(parseData);
    }

    private enum TraceModeInternal {
        SortByTime = 0,
        SortByCategory = 1
    }

    // This must be in its own method to avoid jitting System.EnterpriseServices.dll
    // when it is not needed (ASURT 71868)
    private void ParseTransactionAttribute(string name, string value) {
        object tmpObj = Util.GetEnumAttribute(name, value, typeof(TransactionOption));
        if (tmpObj != null) {
            _transactionMode = (int) tmpObj;

            // Add a reference to the transaction assembly only if needed
            if (_transactionMode != 0 /*TransactionOption.Disabled*/) {

                if (!HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium)) {
                    throw new HttpException(SR.GetString(SR.Insufficient_trust_for_attribute, "transaction"));
                }

                AddAssemblyDependency(typeof(TransactionOption).Assembly);
            }
        }
    }

    internal const string defaultDirectiveName = "page";
    internal override string DefaultDirectiveName {
        get { return defaultDirectiveName; }
    }

    /*
     * Process the contents of the <%@ OutputCache ... %> directive
     */
    internal override void ProcessOutputCacheDirective(string directiveName, IDictionary directive) {
        string varyByContentEncoding;
        string varyByHeader;
        string sqlDependency;
        bool noStoreValue = false;

        varyByContentEncoding = Util.GetAndRemoveNonEmptyAttribute(directive, "varybycontentencoding");
        if (varyByContentEncoding != null) {
            OutputCacheParameters.VaryByContentEncoding = varyByContentEncoding;
        }

        varyByHeader = Util.GetAndRemoveNonEmptyAttribute(directive, "varybyheader");
        if (varyByHeader != null) {
            OutputCacheParameters.VaryByHeader = varyByHeader;
        }

        object tmpObj = Util.GetAndRemoveEnumAttribute(directive, typeof(OutputCacheLocation), "location");
        if (tmpObj != null) {
            _outputCacheLocation = (OutputCacheLocation) tmpObj;
            OutputCacheParameters.Location = _outputCacheLocation;
        }

        sqlDependency = Util.GetAndRemoveNonEmptyAttribute(directive, "sqldependency");
        if (sqlDependency != null) {
            OutputCacheParameters.SqlDependency = sqlDependency;
            // Validate the sqldependency attribute
            SqlCacheDependency.ValidateOutputCacheDependencyString(sqlDependency, true);
        }

        // Get the "no store" bool value
        if (Util.GetAndRemoveBooleanAttribute(directive, "nostore", ref noStoreValue)) {
            OutputCacheParameters.NoStore = noStoreValue;
        }

        base.ProcessOutputCacheDirective(directiveName, directive);
    }


    private static Type s_defaultPageBaseType;

    public static Type DefaultPageBaseType {
        get {
            return s_defaultPageBaseType;
        }
        set {
            if (value != null && !typeof(Page).IsAssignableFrom(value)) {
                throw ExceptionUtil.PropertyInvalid("DefaultPageBaseType");
            }
            BuildManager.ThrowIfPreAppStartNotRunning();

            s_defaultPageBaseType = value;
        }
    }

    private static Type s_defaultUserContorlBaseType;

    public static Type DefaultUserControlBaseType {
        get {
            return s_defaultUserContorlBaseType;
        }
        set {
            if (value != null && !typeof(UserControl).IsAssignableFrom(value)) {
                throw ExceptionUtil.PropertyInvalid("DefaultUserControlBaseType");
            }
            BuildManager.ThrowIfPreAppStartNotRunning();

            s_defaultUserContorlBaseType = value;
        }
    }

    private static Type s_defaultApplicationBaseType;

    public static Type DefaultApplicationBaseType {
        get {
            return s_defaultApplicationBaseType;
        }
        set {
            if (value != null && !typeof(HttpApplication).IsAssignableFrom(value)) {
                throw ExceptionUtil.PropertyInvalid("DefaultApplicationBaseType");
            }
            BuildManager.ThrowIfPreAppStartNotRunning();

            s_defaultApplicationBaseType = value;
        }
    }

    private static Type s_defaultPageParserFilterType;

    public static Type DefaultPageParserFilterType {
        get {
            return s_defaultPageParserFilterType;
        }
        set {
            if (value != null && !typeof(PageParserFilter).IsAssignableFrom(value)) {
                throw ExceptionUtil.PropertyInvalid("DefaultPageParserFilterType");
            }
            BuildManager.ThrowIfPreAppStartNotRunning();

            s_defaultPageParserFilterType = value;
        }
    }

    private static bool s_enableLongStringsAsResources = true;

    public static bool EnableLongStringsAsResources {
        get {
            return s_enableLongStringsAsResources;
        }
        set {
            BuildManager.ThrowIfPreAppStartNotRunning();
            s_enableLongStringsAsResources = value;
        }
    }

    internal override bool FDurationRequiredOnOutputCache {
        get { return _outputCacheLocation != OutputCacheLocation.None; }
    }

    internal override bool FVaryByParamsRequiredOnOutputCache {
        get { return _outputCacheLocation != OutputCacheLocation.None; }
    }

    internal override string UnknownOutputCacheAttributeError {
        get { return SR.Attr_not_supported_in_pagedirective; }
    }
}

}
