//------------------------------------------------------------------------------
// <copyright file="PartialCachingControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Web;
using System.Web.Util;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Security.Permissions;


// Keeps track of one call to Page Register* API
// The semantics of the fields depends to the call type
[Serializable]
internal class RegisterCallData {
    internal ClientAPIRegisterType Type;
    internal ScriptKey Key;
    internal string StringParam1;
    internal string StringParam2;
    internal string StringParam3;
}

// Data that we need to cache
[Serializable]
internal class PartialCachingCacheEntry {
    internal    Guid                      _cachedVaryId;
    internal    string                    _dependenciesKey;
    internal    string[]                  _dependencies; // file dependencies

    internal string OutputString;
    internal string CssStyleString;
    internal ArrayList RegisteredClientCalls;
}

/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
[
ToolboxItem(false)
]
public abstract class BasePartialCachingControl : Control {

    internal Control _cachedCtrl;
    private long _nonVaryHashCode;
    internal string _ctrlID;
    internal string _guid;
    internal DateTime _utcExpirationTime;
    internal bool _useSlidingExpiration;
    internal HttpCacheVaryByParams _varyByParamsCollection;
    internal string[] _varyByControlsCollection;
    internal string _varyByCustom;
    internal string _sqlDependency;
    internal string _provider;
    internal bool _cachingDisabled;
    private string _outputString;
    private string _cssStyleString;
    private string _cacheKey;
    private CacheDependency _cacheDependency;
    private PartialCachingCacheEntry _cacheEntry;
    private ControlCachePolicy _cachePolicy;
    private ArrayList _registeredCallDataForEventValidation;
    private ArrayList _registeredStyleInfo = null;

    internal const char varySeparator = ';';
    internal const string varySeparatorString = ";";

    internal override void InitRecursive(Control namingContainer) {

        HashCodeCombiner combinedHashCode = new HashCodeCombiner();

        _cacheKey = ComputeNonVaryCacheKey(combinedHashCode);

        // Save the non-varying hash, so we don't need to recalculate it later
        _nonVaryHashCode = combinedHashCode.CombinedHash;

        PartialCachingCacheEntry cacheEntry = null;

        // Check if there is a cache entry for the non-varying key
        object tmpCacheEntry = OutputCache.GetFragment(_cacheKey, _provider);

        if (tmpCacheEntry != null) {
            ControlCachedVary cachedVary = tmpCacheEntry as ControlCachedVary;
            if (cachedVary != null) {
                string varyCachedKey = ComputeVaryCacheKey(combinedHashCode, cachedVary);

                // Check if there is a cache entry for the varying key
                cacheEntry = (PartialCachingCacheEntry) OutputCache.GetFragment(varyCachedKey, _provider);
                if (cacheEntry != null && cacheEntry._cachedVaryId != cachedVary.CachedVaryId) {
                    cacheEntry = null;
                    // explicitly remove the entry
                    OutputCache.RemoveFragment(varyCachedKey, _provider);
                }
            }
            else {
                // If it wasn't a ControlCachedVary, it must be a PartialCachingCacheEntry
                cacheEntry = (PartialCachingCacheEntry) tmpCacheEntry;
            }
        }

        // If it's a cache miss, create the control and make it our child
        if (cacheEntry == null) {

            // Cache miss

            _cacheEntry = new PartialCachingCacheEntry();

            _cachedCtrl = CreateCachedControl();
            Controls.Add(_cachedCtrl);

            // Make sure the Page knows about us while the control's OnInit is called
            Page.PushCachingControl(this);
            base.InitRecursive(namingContainer);
            Page.PopCachingControl();
        }
        else {

            // Cache hit

            _outputString = cacheEntry.OutputString;
            _cssStyleString = cacheEntry.CssStyleString;

            // If any calls to Register* API's were made when the control was run,
            // make them now to restore correct behavior (VSWhidbey 80907)
            if (cacheEntry.RegisteredClientCalls != null) {
                foreach (RegisterCallData registerCallData in cacheEntry.RegisteredClientCalls) {
                    switch (registerCallData.Type) {

                        case ClientAPIRegisterType.WebFormsScript:
                            Page.RegisterWebFormsScript();
                            break;

                        case ClientAPIRegisterType.PostBackScript:
                            Page.RegisterPostBackScript();
                            break;

                        case ClientAPIRegisterType.FocusScript:
                            Page.RegisterFocusScript();
                            break;

                        case ClientAPIRegisterType.ClientScriptBlocks:
                        case ClientAPIRegisterType.ClientScriptBlocksWithoutTags:
                        case ClientAPIRegisterType.ClientStartupScripts:
                        case ClientAPIRegisterType.ClientStartupScriptsWithoutTags:
                            Page.ClientScript.RegisterScriptBlock(registerCallData.Key,
                                registerCallData.StringParam2, registerCallData.Type);
                            break;

                        case ClientAPIRegisterType.OnSubmitStatement:
                            Page.ClientScript.RegisterOnSubmitStatementInternal(registerCallData.Key,
                                registerCallData.StringParam2);
                            break;

                        case ClientAPIRegisterType.ArrayDeclaration:
                            Page.ClientScript.RegisterArrayDeclaration(registerCallData.StringParam1,
                                registerCallData.StringParam2);
                            break;

                        case ClientAPIRegisterType.HiddenField:
                            Page.ClientScript.RegisterHiddenField(registerCallData.StringParam1,
                                registerCallData.StringParam2);
                            break;

                        case ClientAPIRegisterType.ExpandoAttribute:
                            Page.ClientScript.RegisterExpandoAttribute(registerCallData.StringParam1,
                                registerCallData.StringParam2, registerCallData.StringParam3, false);
                            break;

                        case ClientAPIRegisterType.EventValidation:
                            if (_registeredCallDataForEventValidation == null) {
                                _registeredCallDataForEventValidation = new ArrayList();
                            }

                            _registeredCallDataForEventValidation.Add(registerCallData);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            }

            base.InitRecursive(namingContainer);
        }
    }

    internal override void LoadRecursive() {

        // If we're in a cache hit, don't do anything special
        if (_outputString != null) {
            base.LoadRecursive();
            return;
        }

        // Make sure the Page knows about us while the control's OnLoad is called
        Page.PushCachingControl(this);
        base.LoadRecursive();
        Page.PopCachingControl();
    }

    internal override void PreRenderRecursiveInternal() {

        // If we're in a cache hit, don't do anything special
        if (_outputString != null) {
            base.PreRenderRecursiveInternal();

            // register the cached styles on the Header control.
            if (_cssStyleString != null && Page.Header != null) {
                Page.Header.RegisterCssStyleString(_cssStyleString);
            }

            return;
        }

        // Make sure the Page knows about us while the control's OnPreRender is called
        Page.PushCachingControl(this);
        base.PreRenderRecursiveInternal();
        Page.PopCachingControl();
    }


    /// <internalonly/>
    public override void Dispose() {
        if (_cacheDependency != null) {
            _cacheDependency.Dispose();
            _cacheDependency = null;
        }

        base.Dispose();
    }

    internal abstract Control CreateCachedControl();


    /// <devdoc>
    ///    <para> Gets or sets the CacheDependency used to cache the control output.</para>
    /// </devdoc>
    public CacheDependency Dependency {
        get { return _cacheDependency; }
        set { _cacheDependency = value; }
    }


    public ControlCachePolicy CachePolicy {
        get {
            // Create the ControlCachePolicy object on demand
            if (_cachePolicy == null)
                _cachePolicy = new ControlCachePolicy(this);

            return _cachePolicy;
        }
    }

    internal HttpCacheVaryByParams VaryByParams {
        get {
            if (_varyByParamsCollection == null) {
                _varyByParamsCollection = new HttpCacheVaryByParams();
                _varyByParamsCollection.IgnoreParams = true;
            }

            return _varyByParamsCollection;
        }
    }
    
    internal string VaryByControl {
        get {
            if (_varyByControlsCollection == null)
                return String.Empty;

            return String.Join(varySeparatorString, _varyByControlsCollection);
        }
        
        set {
            if (String.IsNullOrEmpty(value)) {
                _varyByControlsCollection = null;
            }
            else {
                _varyByControlsCollection = value.Split(varySeparator);
            }
        }
    }
    
    internal TimeSpan Duration {
        get {
            // Special case MaxValue
            if (_utcExpirationTime == DateTime.MaxValue)
                return TimeSpan.MaxValue;

            return _utcExpirationTime - DateTime.UtcNow;
        }
        
        set {
            if (value == TimeSpan.MaxValue) {
                // If it's the max timespan, just make it DateTime.MaxValue to avoid
                // an overflow when adding (VSWhidbey 273271)
                _utcExpirationTime = DateTime.MaxValue;
            }
            else {
                // Compute the expiration time
                _utcExpirationTime = DateTime.UtcNow.Add(value);
            }
        }
    }

    private void RegisterValidationEvents() {
        if (_registeredCallDataForEventValidation != null) {
            foreach (RegisterCallData registerCallData in _registeredCallDataForEventValidation) {
                Page.ClientScript.RegisterForEventValidation(registerCallData.StringParam1,
                    registerCallData.StringParam2);
            }
        }
    }

    internal void RegisterStyleInfo(SelectorStyleInfo selectorInfo) {
        if (_registeredStyleInfo == null) {
            _registeredStyleInfo = new ArrayList();
        }

        _registeredStyleInfo.Add(selectorInfo);
    }

    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    protected internal override void Render(HtmlTextWriter output) {
        CacheDependency sqlCacheDep = null;

        // If the output is cached, use it and do nothing else
        if (_outputString != null) {
            output.Write(_outputString);
            RegisterValidationEvents();
            return;
        }

        // If caching was turned off, just render the control
        if (_cachingDisabled || !RuntimeConfig.GetAppConfig().OutputCache.EnableFragmentCache) {
            _cachedCtrl.RenderControl(output);
            return;
        }

        // Create SQL cache dependency before we render the page
        if (_sqlDependency != null) {
            sqlCacheDep = SqlCacheDependency.CreateOutputCacheDependency(_sqlDependency);
        }

        _cacheEntry.CssStyleString = GetCssStyleRenderString(output.GetType());

        // Create a new HtmlTextWriter, with the same type as the current one (see ASURT 118922)
        StringWriter tmpWriter = new StringWriter();
        HtmlTextWriter tmpHtmlWriter = Page.CreateHtmlTextWriterFromType(tmpWriter, output.GetType());
        CacheDependency cacheDep;
        TextWriter savedWriter = Context.Response.SwitchWriter(tmpWriter);

        try {
            // Make sure the Page knows about us while the control's OnPreRender is called
            Page.PushCachingControl(this);
            _cachedCtrl.RenderControl(tmpHtmlWriter);
            Page.PopCachingControl();
        }
        finally {
            Context.Response.SwitchWriter(savedWriter);
        }

        _cacheEntry.OutputString = tmpWriter.ToString();

        // Send the output to the response
        output.Write(_cacheEntry.OutputString);

        // Cache the output

        cacheDep = _cacheDependency;

        if (sqlCacheDep != null) {
            if (cacheDep == null) {
                cacheDep = sqlCacheDep;
            }
            else {
                AggregateCacheDependency aggr = new AggregateCacheDependency();

                aggr.Add(cacheDep);
                aggr.Add(sqlCacheDep);
                cacheDep = aggr;
            }
        }

        ControlCachedVary cachedVary = null;
        string realItemCacheKey;
        // If there are no varies, use the non-varying key
        if (_varyByParamsCollection == null && _varyByControlsCollection == null && _varyByCustom == null) {
            realItemCacheKey = _cacheKey;
        }
        else {
            string[] varyByParams = null;
            if (_varyByParamsCollection != null)
                varyByParams = _varyByParamsCollection.GetParams();

            cachedVary = new ControlCachedVary(varyByParams, _varyByControlsCollection, _varyByCustom);

            HashCodeCombiner combinedHashCode = new HashCodeCombiner(_nonVaryHashCode);
            realItemCacheKey = ComputeVaryCacheKey(combinedHashCode, cachedVary);
        }

        // Compute the correct expiration, sliding or absolute
        DateTime utcExpirationTime;
        TimeSpan slidingExpiration;
        if (_useSlidingExpiration) {
            utcExpirationTime = Cache.NoAbsoluteExpiration;
            slidingExpiration = _utcExpirationTime - DateTime.UtcNow;
        }
        else {
            utcExpirationTime = _utcExpirationTime;
            slidingExpiration = Cache.NoSlidingExpiration;
        }
        
        try {
            OutputCache.InsertFragment(_cacheKey, cachedVary,
                                       realItemCacheKey, _cacheEntry,
                                       cacheDep /*dependencies*/,
                                       utcExpirationTime, slidingExpiration,
                                       _provider);
        }
        catch {
            if (cacheDep != null) {
                cacheDep.Dispose();
            }
            throw;
        }
    }

    // Return the key used to cache the output
    private string ComputeNonVaryCacheKey(HashCodeCombiner combinedHashCode) {
        // Create a cache key by combining various elements

        // Start with the guid
        combinedHashCode.AddObject(_guid);

        // Make the key vary based on the type of the writer (ASURT 118922)
        HttpBrowserCapabilities browserCap = Context.Request.Browser;
        if (browserCap != null)
            combinedHashCode.AddObject(browserCap.TagWriter);

        return CacheInternal.PrefixPartialCachingControl + combinedHashCode.CombinedHashString;
    }

    private string ComputeVaryCacheKey(HashCodeCombiner combinedHashCode,
        ControlCachedVary cachedVary) {

        // Add something to the has to differentiate it from the non-vary hash.
        // This is needed in case this method doesn't add anything else to the hash (VSWhidbey 194199)
        combinedHashCode.AddInt(1);

        // Get the request value collection
        NameValueCollection reqValCollection;
        HttpRequest request = Page.Request;
        if (request != null && request.HttpVerb == HttpVerb.POST) {
            // Bug 6129: Partial cache key should include posted form values in postbacks.
            // Include both QueryString and Form values (but not Cookies or Server Variables like Request.Params does).
            // Per Request.Params behavior, add QueryString values before Form values
            reqValCollection = new NameValueCollection(request.QueryString);
            reqValCollection.Add(request.Form);
        }
        else {
            // Use the existing value if possible to avoid recreating a NameValueCollection
            reqValCollection = Page.RequestValueCollection;
            // If it's not set, get it based on the method
            if (reqValCollection == null) {
                reqValCollection = Page.GetCollectionBasedOnMethod(true /*dontReturnNull*/);
            }
        }

        if (cachedVary._varyByParams != null) {

            ICollection itemsToUseForHashCode;

            // If '*' was specified, use all the items in the request collection.
            // Otherwise, use only those specified.
            if (cachedVary._varyByParams.Length == 1 && cachedVary._varyByParams[0] == "*")
                itemsToUseForHashCode = reqValCollection;
            else
                itemsToUseForHashCode = cachedVary._varyByParams;

            // Add the items and their values to compute the hash code
            foreach (string varyByParam in itemsToUseForHashCode) {

                // Note: we use to ignore certain system fields here (like VIEWSTATE), but decided
                // not to for consistency with pahe output caching (VSWhidbey 196267, 479252)

                combinedHashCode.AddCaseInsensitiveString(varyByParam);
                string val = reqValCollection[varyByParam];
                if (val != null)
                    combinedHashCode.AddObject(val);
            }
        }

        if (cachedVary._varyByControls != null) {

            // Prepend them with a prefix to make them fully qualified
            string prefix;
            if (NamingContainer == Page) {
                // No prefix if it's the page
                prefix = String.Empty;
            }
            else {
                prefix = NamingContainer.UniqueID;
                Debug.Assert(!String.IsNullOrEmpty(prefix));
                prefix += IdSeparator;
            }

            prefix += _ctrlID + IdSeparator;

            // Add all the relative vary params and their values to the hash code
            foreach (string varyByParam in cachedVary._varyByControls) {

                string temp = prefix + varyByParam.Trim();
                combinedHashCode.AddCaseInsensitiveString(temp);
                string val = reqValCollection[temp];
                if (val != null)
                    combinedHashCode.AddObject(reqValCollection[temp]);
            }
        }

        if (cachedVary._varyByCustom != null) {
            string customString = Context.ApplicationInstance.GetVaryByCustomString(
                Context, cachedVary._varyByCustom);
            if (customString != null)
                combinedHashCode.AddObject(customString);
        }

        return CacheInternal.PrefixPartialCachingControl + combinedHashCode.CombinedHashString;
    }

    private string GetCssStyleRenderString(Type htmlTextWriterType) {
        // Nothing to do if no styles are registered.
        if (_registeredStyleInfo == null) {
            return null;
        }

        // Create an empty cssStringWriter
        StringWriter cssStringWriter = new StringWriter(CultureInfo.CurrentCulture);

        // Create a new HtmlTextWriter, with the same type as the current one
        HtmlTextWriter cssHtmlTextWriter = 
            Page.CreateHtmlTextWriterFromType(cssStringWriter, htmlTextWriterType);

        CssTextWriter cssWriter = new CssTextWriter(cssHtmlTextWriter);

        foreach (SelectorStyleInfo si in _registeredStyleInfo) {
            HtmlHead.RenderCssRule(cssWriter, si.selector, si.style, si.urlResolver);
        }

        // Return the css style rendered string
        return cssStringWriter.ToString();
    }

    internal void SetVaryByParamsCollectionFromString(string varyByParams) {

        Debug.Assert(_varyByParamsCollection == null);

        if (varyByParams == null)
            return;

        string[] varyByParamsStrings = varyByParams.Split(varySeparator);
        _varyByParamsCollection = new HttpCacheVaryByParams();
        _varyByParamsCollection.ResetFromParams(varyByParamsStrings);
    }

    internal void RegisterPostBackScript() {
        RegisterClientCall(ClientAPIRegisterType.PostBackScript, String.Empty, null);
    }

    internal void RegisterFocusScript() {
        RegisterClientCall(ClientAPIRegisterType.FocusScript, String.Empty, null);
    }

    internal void RegisterWebFormsScript() {
        RegisterClientCall(ClientAPIRegisterType.WebFormsScript, String.Empty, null);
    }

    private void RegisterClientCall(ClientAPIRegisterType type,
        ScriptKey scriptKey, string stringParam2) {

        // Keep track of the call, in order to be able to call it again when there is a cache hit.

        RegisterCallData registerCallData = new RegisterCallData();
        registerCallData.Type = type;
        registerCallData.Key = scriptKey;
        registerCallData.StringParam2 = stringParam2;

        if (_cacheEntry.RegisteredClientCalls == null)
            _cacheEntry.RegisteredClientCalls = new ArrayList();

        _cacheEntry.RegisteredClientCalls.Add(registerCallData);
    }

    private void RegisterClientCall(ClientAPIRegisterType type,
        string stringParam1, string stringParam2) {
        RegisterClientCall(type, stringParam1, stringParam2, null);
    }

    private void RegisterClientCall(ClientAPIRegisterType type,
        string stringParam1, string stringParam2, string stringParam3) {

        // Keep track of the call, in order to be able to call it again when there is a cache hit.

        RegisterCallData registerCallData = new RegisterCallData();
        registerCallData.Type = type;
        registerCallData.StringParam1 = stringParam1;
        registerCallData.StringParam2 = stringParam2;
        registerCallData.StringParam3 = stringParam3;

        if (_cacheEntry.RegisteredClientCalls == null)
            _cacheEntry.RegisteredClientCalls = new ArrayList();

        _cacheEntry.RegisteredClientCalls.Add(registerCallData);
    }

    internal void RegisterScriptBlock(ClientAPIRegisterType type, ScriptKey key, string script) {
        RegisterClientCall(type, key, script);
    }

    internal void RegisterOnSubmitStatement(ScriptKey key, string script) {
        RegisterClientCall(ClientAPIRegisterType.OnSubmitStatement, key, script);
    }

    internal void RegisterArrayDeclaration(string arrayName, string arrayValue) {
        RegisterClientCall(ClientAPIRegisterType.ArrayDeclaration,
            arrayName, arrayValue);
    }

    internal void RegisterHiddenField(string hiddenFieldName, string hiddenFieldInitialValue) {
        RegisterClientCall(ClientAPIRegisterType.HiddenField,
            hiddenFieldName, hiddenFieldInitialValue);
    }

    internal void RegisterExpandoAttribute(string controlID, string attributeName, string attributeValue) {
        RegisterClientCall(ClientAPIRegisterType.ExpandoAttribute, controlID, attributeName, attributeValue);
    }

    internal void RegisterForEventValidation(string uniqueID, string argument) {
        RegisterClientCall(ClientAPIRegisterType.EventValidation, uniqueID, argument);
    }
}


/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public class StaticPartialCachingControl : BasePartialCachingControl {

    private BuildMethod _buildMethod;


    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public StaticPartialCachingControl(string ctrlID, string guid, int duration,
        string varyByParams, string varyByControls, string varyByCustom,
        BuildMethod buildMethod)
        :this(ctrlID, guid, duration, varyByParams, varyByControls,
            varyByCustom, null, buildMethod, null)
    {
    }


    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public StaticPartialCachingControl(string ctrlID, string guid, int duration,
        string varyByParams, string varyByControls, string varyByCustom, string sqlDependency,
        BuildMethod buildMethod)
        :this(ctrlID, guid, duration, varyByParams, varyByControls,
            varyByCustom, sqlDependency, buildMethod, null)
    {
    }

    public StaticPartialCachingControl(string ctrlID, string guid, int duration,
        string varyByParams, string varyByControls, string varyByCustom, string sqlDependency, 
        BuildMethod buildMethod, string providerName) {
        _ctrlID = ctrlID;
        Duration = new TimeSpan(0 /*hours*/, 0 /*mins*/, duration /*seconds*/);

        SetVaryByParamsCollectionFromString(varyByParams);

        if (varyByControls != null)
            _varyByControlsCollection = varyByControls.Split(varySeparator);
        _varyByCustom = varyByCustom;
        _guid = guid;
        _buildMethod = buildMethod;
        _sqlDependency = sqlDependency;
        _provider = providerName;
    }

    internal override Control CreateCachedControl() {
        return _buildMethod();
    }

    /*
     * Called by generated code (hence must be public).
     * Create a StaticPartialCachingControl and add it as a child
     */

    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    static public void BuildCachedControl(Control parent, string ctrlID, string guid,
        int duration, string varyByParams, string varyByControls, string varyByCustom,
        BuildMethod buildMethod) {
        BuildCachedControl(parent, ctrlID, guid, duration, varyByParams,
            varyByControls, varyByCustom, null, buildMethod, null);
    }


    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    static public void BuildCachedControl(Control parent, string ctrlID, string guid,
        int duration, string varyByParams, string varyByControls, string varyByCustom, string sqlDependency,
        BuildMethod buildMethod) {
        BuildCachedControl(parent, ctrlID, guid, duration, varyByParams,
            varyByControls, varyByCustom, sqlDependency, buildMethod, null);
    }

    static public void BuildCachedControl(Control parent, string ctrlID, string guid,
        int duration, string varyByParams, string varyByControls, string varyByCustom, string sqlDependency, 
        BuildMethod buildMethod, string providerName) {

        StaticPartialCachingControl pcc = new StaticPartialCachingControl(
            ctrlID, guid, duration, varyByParams, varyByControls, varyByCustom, sqlDependency,
            buildMethod, providerName);

        ((IParserAccessor)parent).AddParsedSubObject(pcc);
    }
}


/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public class PartialCachingControl : BasePartialCachingControl {

    private IWebObjectFactory _objectFactory;
    private Type _createCachedControlType;
    private object[] _args;


    public Control CachedControl { get { return _cachedCtrl; } }

    internal PartialCachingControl(IWebObjectFactory objectFactory, Type createCachedControlType,
        PartialCachingAttribute cacheAttrib, string cacheKey, object[] args) {
        string providerName = cacheAttrib.ProviderName;
        _ctrlID = cacheKey;
        Duration = new TimeSpan(0 /*hours*/, 0 /*mins*/, cacheAttrib.Duration /*seconds*/);

        SetVaryByParamsCollectionFromString(cacheAttrib.VaryByParams);

        if (cacheAttrib.VaryByControls != null)
            _varyByControlsCollection = cacheAttrib.VaryByControls.Split(varySeparator);
        _varyByCustom = cacheAttrib.VaryByCustom;
        _sqlDependency = cacheAttrib.SqlDependency;
        if (providerName == OutputCache.ASPNET_INTERNAL_PROVIDER_NAME) {
            providerName = null;
        }
        _provider = providerName;
        _guid = cacheKey;
        _objectFactory = objectFactory;
        _createCachedControlType = createCachedControlType;
        _args = args;
    }

    internal override Control CreateCachedControl() {

        Control cachedControl;

        if (_objectFactory != null) {
            cachedControl = (Control) _objectFactory.CreateInstance();
        }
        else {
            // Instantiate the control
            cachedControl = (Control) HttpRuntime.CreatePublicInstance(_createCachedControlType, _args);
        }

        // If it's a user control, do some extra initialization
        UserControl uc = cachedControl as UserControl;
        if (uc != null)
            uc.InitializeAsUserControl(Page);

        cachedControl.ID = _ctrlID;

        return cachedControl;
    }
}

/*
 * Holds param names that this cached item varies by.
 */
[Serializable]
internal class ControlCachedVary {
    private           Guid      _cachedVaryId;
    internal readonly string[]  _varyByParams;
    internal readonly string    _varyByCustom;
    internal readonly string[]  _varyByControls;

    internal Guid CachedVaryId { get { return _cachedVaryId; } }

    internal ControlCachedVary(string[] varyByParams,
        string[] varyByControls, string varyByCustom) {
        _varyByParams = varyByParams;
        _varyByControls = varyByControls;
        _varyByCustom = varyByCustom;
        _cachedVaryId = Guid.NewGuid();
    }

    public override bool Equals (Object obj) {

        if (!(obj is ControlCachedVary))
            return false;

        ControlCachedVary cv = (ControlCachedVary) obj;

        return  _varyByCustom == cv._varyByCustom               &&
                StringUtil.StringArrayEquals(_varyByParams, cv._varyByParams) &&
                StringUtil.StringArrayEquals(_varyByControls, cv._varyByControls);
    }

    public override int GetHashCode () {
        HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
        
        // Cast _varyByCustom to an object, since the HashCodeCombiner.AddObject(string)
        // overload uses StringUtil.GetStringHashCode().  We want to use String.GetHashCode()
        // in this method, since we do not require a stable hash code across architectures.
        hashCodeCombiner.AddObject((object)_varyByCustom);
        
        hashCodeCombiner.AddArray(_varyByParams);
        hashCodeCombiner.AddArray(_varyByControls);
        return hashCodeCombiner.CombinedHash32;
    }
}

}

