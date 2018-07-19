//------------------------------------------------------------------------------
// <copyright file="HttpCapabilitiesBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Base class for browser capabilities object: just a read-only dictionary
 * holder that supports Init()
 *
 * 


*/

namespace System.Web.Configuration {

    using System.Collections;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Util;
    using System.Web.UI.Adapters;
    using Debug=System.Web.Util.Debug;

    /*
     * Abstract base class for Capabilities
     */
    public class HttpCapabilitiesBase : IFilterResolutionService {

#if !DONTUSEFACTORYGENERATOR
        private static FactoryGenerator _controlAdapterFactoryGenerator;
        private static Hashtable _controlAdapterFactoryTable;
        private static object _staticLock = new object();
#endif // DONTUSEFACTORYGENERATOR
        private static object s_nullAdapterSingleton = new object();
        private bool _useOptimizedCacheKey = true;
        private static object _emptyHttpCapabilitiesBaseLock = new object();
        private static HttpCapabilitiesProvider _browserCapabilitiesProvider = null;

        private static HttpCapabilitiesBase _emptyHttpCapabilitiesBase;

        internal static HttpCapabilitiesBase EmptyHttpCapabilitiesBase {
            get {
                if (_emptyHttpCapabilitiesBase != null) {
                    return _emptyHttpCapabilitiesBase;
                }

                lock (_emptyHttpCapabilitiesBaseLock) {
                    if (_emptyHttpCapabilitiesBase != null) {
                        return _emptyHttpCapabilitiesBase;
                    }

                    _emptyHttpCapabilitiesBase = new HttpCapabilitiesBase();
                }

                return _emptyHttpCapabilitiesBase;
            }
        }

        public static HttpCapabilitiesProvider BrowserCapabilitiesProvider {
            get {
                return _browserCapabilitiesProvider;
            }
            set {
                _browserCapabilitiesProvider = value;
            }
        }

        public bool UseOptimizedCacheKey {
            get {
                return _useOptimizedCacheKey;
            }
        }

        public void DisableOptimizedCacheKey() {
            _useOptimizedCacheKey = false;
        }

        //
        // Public API for retrieving capabilities from config.
        //
        // Note that this API will return an empty HttpCapabilitiesBase
        // if capabilties cannot be found.
        //
        [ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
        public static HttpCapabilitiesBase GetConfigCapabilities(string configKey, HttpRequest request) {
            
            HttpCapabilitiesBase capabilities = null;

            if (configKey == "system.web/browserCaps") {
                // Use cached config for system.web/browserCaps
                capabilities = GetBrowserCapabilities(request);
            }
            else {
                //
                // Slower code path to get capabilities from any section 
                // that implements System.Web.Configuration.HttpCapabilitiesSectionHandler.
                // This code path will hit a demand for ConfigurationPermission.
                //
                HttpCapabilitiesDefaultProvider capsbuilder = (HttpCapabilitiesDefaultProvider) request.Context.GetSection(configKey);
                if (capsbuilder != null) {
                    if (BrowserCapabilitiesProvider != null) {
                        capsbuilder.BrowserCapabilitiesProvider = BrowserCapabilitiesProvider;
                    }
                    if (capsbuilder.BrowserCapabilitiesProvider == null) {
                        capabilities = capsbuilder.Evaluate(request);
                    }
                    else {
                        capabilities = capsbuilder.BrowserCapabilitiesProvider.GetBrowserCapabilities(request);
                    }
                }
            }

            if (capabilities == null) {
                capabilities = EmptyHttpCapabilitiesBase;
            }

            return capabilities;
        }

        //
        // Get browser capabilities from config that are stored at "system.web/browserCaps".
        //
        // This code path will use the cached config object and avoid the demand for ConfigurationPermission
        // after the first request for config.
        //
        // Note: this API will return null if the section isn't found.
        //
        internal static HttpBrowserCapabilities GetBrowserCapabilities(HttpRequest request) {

            HttpCapabilitiesBase capabilities = null;

            // Get the config evaluator from the cached config object.
            HttpCapabilitiesDefaultProvider capsbuilder = request.Context.IsRuntimeErrorReported ?
                RuntimeConfig.GetLKGConfig(request.Context).BrowserCaps : RuntimeConfig.GetConfig(request.Context).BrowserCaps;
            if (capsbuilder != null) {
                if (BrowserCapabilitiesProvider != null) {
                    capsbuilder.BrowserCapabilitiesProvider = BrowserCapabilitiesProvider;
                }
                if (capsbuilder.BrowserCapabilitiesProvider == null) {
                    capabilities = capsbuilder.Evaluate(request);
                }
                else {
                    capabilities = capsbuilder.BrowserCapabilitiesProvider.GetBrowserCapabilities(request);
                }
            }

            return (HttpBrowserCapabilities) capabilities;
        }

        /*
         * A Capabilities object is just a read-only dictionary
         */
        /// <devdoc>
        ///       <para>Allows access to individual dictionary values.</para>
        ///    </devdoc>
        public virtual String this[String key] {
            get {
                return (String)_items[key];
            }
        }

        public HtmlTextWriter CreateHtmlTextWriter(TextWriter w) {
            string mtw = HtmlTextWriter;
            if (mtw != null && mtw.Length != 0) {
                HtmlTextWriter writer = null;
                try {
                    Type writerType = BuildManager.GetType(mtw, true /* throwOnFail */, false /* ignoreCase */);
                    object[] arr = new object[1];
                    arr[0] = w;
                    writer = (HtmlTextWriter)Activator.CreateInstance(writerType, arr);
                    if (writer != null) {
                        return writer;
                    }
                }
                catch {
                    throw new Exception(SR.GetString(SR.Could_not_create_type_instance, mtw));
                }
            }
            return CreateHtmlTextWriterInternal(w);
        }

        internal HtmlTextWriter CreateHtmlTextWriterInternal(TextWriter tw) {
            Type tagWriter = TagWriter;
            if (tagWriter != null) {
                return Page.CreateHtmlTextWriterFromType(tw, tagWriter);
            }

            // Fall back to Html 3.2
            return new Html32TextWriter(tw);
        }

        /*
         * It provides an overridable Init method
         */
        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void Init() {
        }

        /*
         * The actual initializer sets up Item[] before calling Init()
         */

        internal void InitInternal(HttpBrowserCapabilities browserCaps) {
            if (_items != null) {
                throw new ArgumentException(SR.GetString(SR.Caps_cannot_be_inited_twice));
            }

            _items = browserCaps._items;
            _adapters = browserCaps._adapters;
            _browsers = browserCaps._browsers;
            _htmlTextWriter = browserCaps._htmlTextWriter;
            _useOptimizedCacheKey = browserCaps._useOptimizedCacheKey;

            Init();
        }

        internal ControlAdapter GetAdapter(System.Web.UI.Control control) {
            if (_adapters == null || _adapters.Count == 0) {
                return null;
            }

            if (control == null) {
                return null;
            }

            //see if we have already cached the type;
            Type controlType = control.GetType();
            object o = AdapterTypes[controlType];

            // Common desktop case: simply return null since we already tried to resolve the adapter.
            if (object.ReferenceEquals(o, s_nullAdapterSingleton))
                return null;

            Type adapterType = (Type)o;
            if (adapterType == null) {
                Type tempControlType = controlType;
                string controlTypename = null;
                string adapterTypename = null;

                while (adapterTypename == null && tempControlType != typeof(Control)) {
                    controlTypename = tempControlType.AssemblyQualifiedName;
                    adapterTypename = (string)Adapters[controlTypename];

                    if (adapterTypename == null) {
                        controlTypename = tempControlType.FullName;
                        adapterTypename = (string)Adapters[controlTypename];
                    }

                    if (adapterTypename != null) {
                        break;
                    }

                    tempControlType = tempControlType.BaseType;
                }

                // Remember it so that we do not walk the control hierarchy again.
                if (String.IsNullOrEmpty(adapterTypename)) {
                    AdapterTypes[controlType] = s_nullAdapterSingleton;
                    return null;
                }

                //do not thrownOnFail or ignoreCase
                adapterType = BuildManager.GetType(adapterTypename, false, false);
                if (adapterType == null) {
                    throw new Exception(SR.GetString(SR.ControlAdapters_TypeNotFound, adapterTypename));
                }

                AdapterTypes[controlType] = adapterType;
            }

#if DONTUSEFACTORYGENERATOR
            ControlAdapter adapter = (ControlAdapter) HttpRuntime.CreatePublicInstance(adapterType);
#else
            IWebObjectFactory factory = GetAdapterFactory(adapterType);
            ControlAdapter adapter = (ControlAdapter)factory.CreateInstance();
#endif // DONTUSEFACTORYGENERATOR
            adapter._control = control;

            return adapter;
        }

#if !DONTUSEFACTORYGENERATOR
        private IWebObjectFactory GetAdapterFactory(Type adapterType) {
            if (_controlAdapterFactoryGenerator == null) {
                lock (_staticLock) {
                    if (_controlAdapterFactoryGenerator == null) {
                        _controlAdapterFactoryTable = new Hashtable();
                        _controlAdapterFactoryGenerator = new FactoryGenerator();
                    }
                }
            }

            IWebObjectFactory factory = (IWebObjectFactory)_controlAdapterFactoryTable[adapterType];
            if (factory == null) {

                lock (_controlAdapterFactoryTable.SyncRoot) {
                    factory = (IWebObjectFactory)_controlAdapterFactoryTable[adapterType];

                    if (factory == null) {
                        try {
                            factory = _controlAdapterFactoryGenerator.CreateFactory(adapterType);
                        }
                        catch {
                            throw new Exception(SR.GetString(SR.Could_not_create_type_instance, adapterType.ToString()));
                        }

                        _controlAdapterFactoryTable[adapterType] = factory;
                    }
                }
            }

            return factory;
        }
#endif // DONTUSEFACTORYGENERATOR

        public IDictionary Capabilities {
            get {
                return _items;
            }
            set {
                _items = value;
            }
        }

        public IDictionary Adapters {
            get {
                if (_adapters == null) {
                    lock (_staticLock) {
                        if (_adapters == null) {
                            _adapters = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                }
                return _adapters;
            }
        }

        public string HtmlTextWriter {
            get {
                return _htmlTextWriter;
            }
            set {
                _htmlTextWriter = value;
            }
        }

        private Hashtable AdapterTypes {
            get {
                if (_adapterTypes == null) {
                    lock (_staticLock) {
                        if (_adapterTypes == null) {
                            _adapterTypes = Hashtable.Synchronized(new Hashtable());
                        }
                    }
                }
                return _adapterTypes;
            }
        }

        public string Id {
            get {
                if (_browsers != null) {
                    return (string)_browsers[_browsers.Count - 1];
                }
                else return String.Empty;
            }
        }

        public ArrayList Browsers {
            get {
                return _browsers;
            }
        }

        Hashtable _adapterTypes;
        private IDictionary _adapters;
        private string _htmlTextWriter;
        private IDictionary _items;

        public Version ClrVersion {
            get {
                Version[] clrVersions = GetClrVersions();
                if (clrVersions != null) {
                    return clrVersions[clrVersions.Length - 1];
                }

                return null;
            }
        }

        public Version[] GetClrVersions() {
            string ua = HttpCapabilitiesDefaultProvider.GetUserAgent(HttpContext.Current.Request);
            if (String.IsNullOrEmpty(ua)) {
                return null;
            }

            // Adding timeout for Regex in case of malicious UA string causing DoS
            Regex regex = RegexUtil.CreateRegex("\\.NET CLR (?'clrVersion'[0-9\\.]*)", RegexOptions.None);
            MatchCollection matches = regex.Matches(ua);

            if (matches.Count == 0) {
                return new Version[1] { new Version() };
            }

            ArrayList versionList = new ArrayList();
            foreach (Match match in matches) {
                try {
                    Version version = new Version(match.Groups["clrVersion"].Value);
                    versionList.Add(version);
                }
                catch (System.FormatException) {
                    //got imvalid version data
                }
            }
            versionList.Sort();

            return (Version[])versionList.ToArray(typeof(Version));
        }

        public string Type {
            get {
                if (!_havetype) {
                    _type = this["type"];
                    _havetype = true;
                }
                return _type;
            }
        }

        public string Browser {
            get {
                if (!_havebrowser) {
                    _browser = this["browser"];
                    _havebrowser = true;
                }
                return _browser;
            }
        }

        public string Version {
            get {
                if (!_haveversion) {
                    _version = this["version"];
                    _haveversion = true;
                }
                return _version;
            }
        }

        public int MajorVersion {
            get {
                if (!_havemajorversion) {
                    try {
                        _majorversion = int.Parse(this["majorversion"], CultureInfo.InvariantCulture);
                        _havemajorversion = true;
                    }
                    catch (FormatException e) {
                        throw BuildParseError(e, "majorversion");
                    }
                }
                return _majorversion;
            }
        }

        Exception BuildParseError(Exception e, string capsKey) {
            string message = SR.GetString(SR.Invalid_string_from_browser_caps, e.Message, capsKey, this[capsKey]);

            // to show ConfigurationException in stack trace
            ConfigurationErrorsException configEx = new ConfigurationErrorsException(message, e);

            // I want it to look like an unhandled exception
            HttpUnhandledException httpUnhandledEx = new HttpUnhandledException(null, null);

            // but show message from outer exception (it normally shows the inner-most)
            httpUnhandledEx.SetFormatter(new UseLastUnhandledErrorFormatter(configEx));

            return httpUnhandledEx;
        }

        bool CapsParseBoolDefault(string capsKey, bool defaultValue) {
            string value = this[capsKey];
            if (value == null) {
                return defaultValue;
            }

            try {
                return bool.Parse(value);
            }
            catch (FormatException) {
                return defaultValue;
            }
        }

        bool CapsParseBool(string capsKey) {
            try {
                return bool.Parse(this[capsKey]);
            }
            catch (FormatException e) {
                throw BuildParseError(e, capsKey);
            }
        }

        public string MinorVersionString {
            get {
                return this["minorversion"];
            }
        }

        public double MinorVersion {
            get {
                if (!_haveminorversion) {
                    lock(_staticLock) {
                        if (!_haveminorversion)
                        {

                            try
                            {
                                // see ASURT 11176
                                _minorversion = double.Parse(
                                    this["minorversion"],
                                    NumberStyles.Float | NumberStyles.AllowDecimalPoint,
                                    NumberFormatInfo.InvariantInfo);
                                _haveminorversion = true;
                            }
                            catch (FormatException e)
                            {
                                // Check if there's more than one decimal
                                // The only exception case we handle is of form .4.1, it becomes .4
                                string minor = this["minorversion"];
                                int firstDecimal = minor.IndexOf('.');
                                if (firstDecimal != -1)
                                {
                                    int nextDecimal = minor.IndexOf('.', firstDecimal + 1);
                                    if (nextDecimal != -1)
                                    {
                                        try
                                        {
                                            _minorversion = double.Parse(
                                                minor.Substring(0, nextDecimal),
                                                NumberStyles.Float | NumberStyles.AllowDecimalPoint,
                                                NumberFormatInfo.InvariantInfo);
                                            Thread.MemoryBarrier();
                                            _haveminorversion = true;
                                        }
                                        catch (FormatException)
                                        {
                                        }
                                    }
                                }
                                if (!_haveminorversion)
                                {
                                    throw BuildParseError(e, "minorversion");
                                }
                            }
                        }
                    }
                }
                return _minorversion;
            }
        }

        public string Platform {
            get {
                if (!_haveplatform) {
                    _platform = this["platform"];
                    _haveplatform = true;
                }
                return _platform;
            }
        }

        public Type TagWriter {
            get {
                try {
                    if (!_havetagwriter) {
                        string tagWriter = this["tagwriter"];
                        if (String.IsNullOrEmpty(tagWriter)) {
                            _tagwriter = null;
                        }
                        else if (string.Compare(tagWriter, typeof(System.Web.UI.HtmlTextWriter).FullName, StringComparison.Ordinal) == 0) {
                            _tagwriter = typeof(System.Web.UI.HtmlTextWriter);
                        }
                        else {
                            _tagwriter = BuildManager.GetType(tagWriter, true /*throwOnError*/);
                        }
                        _havetagwriter = true;
                    }
                }
                catch (Exception e) {
                    throw BuildParseError(e, "tagwriter");
                }

                return _tagwriter;
            }
        }
        public Version EcmaScriptVersion {
            get {
                if (!_haveecmascriptversion) {
                    _ecmascriptversion = new Version(this["ecmascriptversion"]);
                    _haveecmascriptversion = true;
                }
                return _ecmascriptversion;
            }
        }

        public Version MSDomVersion {
            get {
                if (!_havemsdomversion) {
                    _msdomversion = new Version(this["msdomversion"]);
                    _havemsdomversion = true;
                }
                return _msdomversion;
            }
        }

        public Version W3CDomVersion {
            get {
                if (!_havew3cdomversion) {
                    _w3cdomversion = new Version(this["w3cdomversion"]);
                    _havew3cdomversion = true;
                }
                return _w3cdomversion;
            }
        }

        public bool Beta {
            get {
                if (!_havebeta) {
                    _beta = CapsParseBool("beta");
                    _havebeta = true;
                }
                return _beta;
            }
        }

        public bool Crawler {
            get {
                if (!_havecrawler) {
                    _crawler = CapsParseBool("crawler");
                    _havecrawler = true;
                }
                return _crawler;
            }
        }

        public bool AOL {
            get {
                if (!_haveaol) {
                    _aol = CapsParseBool("aol");
                    _haveaol = true;
                }
                return _aol;
            }
        }

        public bool Win16 {
            get {
                if (!_havewin16) {
                    _win16 = CapsParseBool("win16");
                    _havewin16 = true;
                }
                return _win16;
            }
        }

        public bool Win32 {
            get {
                if (!_havewin32) {
                    _win32 = CapsParseBool("win32");
                    _havewin32 = true;
                }
                return _win32;
            }
        }

        public bool Frames {
            get {
                if (!_haveframes) {
                    _frames = CapsParseBool("frames");
                    _haveframes = true;
                }
                return _frames;
            }
        }

        public bool RequiresControlStateInSession {
            get {
                if (!_haverequiresControlStateInSession) {
                    if (this["requiresControlStateInSession"] != null) {
                        _requiresControlStateInSession = CapsParseBoolDefault("requiresControlStateInSession", false);
                    }
                    _haverequiresControlStateInSession = true;
                }
                return _requiresControlStateInSession;
            }
        }

        public bool Tables {
            get {
                if (!_havetables) {
                    _tables = CapsParseBool("tables");
                    _havetables = true;
                }
                return _tables;
            }
        }

        public bool Cookies {
            get {
                if (!_havecookies) {
                    _cookies = CapsParseBool("cookies");
                    _havecookies = true;
                }
                return _cookies;
            }
        }

        public bool VBScript {
            get {
                if (!_havevbscript) {
                    _vbscript = CapsParseBool("vbscript");
                    _havevbscript = true;
                }
                return _vbscript;
            }
        }

        [Obsolete("The recommended alternative is the EcmaScriptVersion property. A Major version value greater than or equal to 1 implies JavaScript support. http://go.microsoft.com/fwlink/?linkid=14202")]
        public bool JavaScript {
            get {
                if (!_havejavascript) {
                    _javascript = CapsParseBool("javascript");
                    _havejavascript = true;
                }
                return _javascript;
            }
        }

        public bool JavaApplets {
            get {
                if (!_havejavaapplets) {
                    _javaapplets = CapsParseBool("javaapplets");
                    _havejavaapplets = true;
                }
                return _javaapplets;
            }
        }

        public Version JScriptVersion {
            get {
                if (!_havejscriptversion) {
                    _jscriptversion = new Version(this["jscriptversion"]);
                    _havejscriptversion = true;
                }
                return _jscriptversion;
            }
        }

        public bool ActiveXControls {
            get {
                if (!_haveactivexcontrols) {
                    _activexcontrols = CapsParseBool("activexcontrols");
                    _haveactivexcontrols = true;
                }
                return _activexcontrols;
            }
        }

        public bool BackgroundSounds {
            get {
                if (!_havebackgroundsounds) {
                    _backgroundsounds = CapsParseBool("backgroundsounds");
                    _havebackgroundsounds = true;
                }
                return _backgroundsounds;
            }
        }

        public bool CDF {
            get {
                if (!_havecdf) {
                    _cdf = CapsParseBool("cdf");
                    _havecdf = true;
                }
                return _cdf;
            }
        }

        //previously in System.Web.Mobile
        public virtual String MobileDeviceManufacturer {
            get {
                if (!_haveMobileDeviceManufacturer) {
                    _mobileDeviceManufacturer = this["mobileDeviceManufacturer"];
                    _haveMobileDeviceManufacturer = true;
                }
                return _mobileDeviceManufacturer;
            }
        }


        public virtual String MobileDeviceModel {
            get {
                if (!_haveMobileDeviceModel) {
                    _mobileDeviceModel = this["mobileDeviceModel"];
                    _haveMobileDeviceModel = true;
                }
                return _mobileDeviceModel;
            }
        }


        public virtual String GatewayVersion {
            get {
                if (!_haveGatewayVersion) {
                    _gatewayVersion = this["gatewayVersion"];
                    _haveGatewayVersion = true;
                }
                return _gatewayVersion;
            }
        }


        public virtual int GatewayMajorVersion {
            get {
                if (!_haveGatewayMajorVersion) {
                    _gatewayMajorVersion = Convert.ToInt32(this["gatewayMajorVersion"], CultureInfo.InvariantCulture);
                    _haveGatewayMajorVersion = true;
                }
                return _gatewayMajorVersion;
            }
        }


        public virtual double GatewayMinorVersion {
            get {
                if (!_haveGatewayMinorVersion) {
                    // The conversion below does not use Convert.ToDouble()
                    // because it depends on the current locale.  So a german machine it would look for
                    // a comma as a seperator "1,5" where all user-agent strings use english
                    // decimal points "1.5".  URT11176
                    //
                    _gatewayMinorVersion = double.Parse(
                                        this["gatewayMinorVersion"],
                                        NumberStyles.Float | NumberStyles.AllowDecimalPoint,
                                        NumberFormatInfo.InvariantInfo);
                    _haveGatewayMinorVersion = true;
                }
                return _gatewayMinorVersion;
            }
        }

        public virtual String PreferredRenderingType {
            get {
                if (!_havePreferredRenderingType) {
                    _preferredRenderingType = this["preferredRenderingType"];
                    _havePreferredRenderingType = true;
                }
                return _preferredRenderingType;
            }
        }


        public virtual String PreferredRequestEncoding {
            get {
                if (!_havePreferredRequestEncoding) {
                    _preferredRequestEncoding = this["preferredRequestEncoding"];
                    Thread.MemoryBarrier();
                    _havePreferredRequestEncoding = true;
                }
                return _preferredRequestEncoding;
            }
        }


        public virtual String PreferredResponseEncoding {
            get {
                if (!_havePreferredResponseEncoding) {
                    _preferredResponseEncoding = this["preferredResponseEncoding"];
                    _havePreferredResponseEncoding = true;
                }
                return _preferredResponseEncoding;
            }
        }


        public virtual String PreferredRenderingMime {
            get {
                if (!_havePreferredRenderingMime) {
                    _preferredRenderingMime = this["preferredRenderingMime"];
                    _havePreferredRenderingMime = true;
                }
                return _preferredRenderingMime;
            }
        }


        public virtual String PreferredImageMime {
            get {
                if (!_havePreferredImageMime) {
                    _preferredImageMime = this["preferredImageMime"];
                    _havePreferredImageMime = true;
                }
                return _preferredImageMime;
            }
        }


        public virtual int ScreenCharactersWidth {
            get {
                if (!_haveScreenCharactersWidth) {
                    if (this["screenCharactersWidth"] == null) {
                        // calculate from best partial information

                        int screenPixelsWidthToUse = 640;
                        int characterWidthToUse = 8;

                        if (this["screenPixelsWidth"] != null && this["characterWidth"] != null) {
                            screenPixelsWidthToUse = Convert.ToInt32(this["screenPixelsWidth"], CultureInfo.InvariantCulture);
                            characterWidthToUse = Convert.ToInt32(this["characterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["screenPixelsWidth"] != null) {
                            screenPixelsWidthToUse = Convert.ToInt32(this["screenPixelsWidth"], CultureInfo.InvariantCulture);
                            characterWidthToUse = Convert.ToInt32(this["defaultCharacterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["characterWidth"] != null) {
                            screenPixelsWidthToUse = Convert.ToInt32(this["defaultScreenPixelsWidth"], CultureInfo.InvariantCulture);
                            characterWidthToUse = Convert.ToInt32(this["characterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["defaultScreenCharactersWidth"] != null) {
                            screenPixelsWidthToUse = Convert.ToInt32(this["defaultScreenCharactersWidth"], CultureInfo.InvariantCulture);
                            characterWidthToUse = 1;
                        }

                        _screenCharactersWidth = screenPixelsWidthToUse / characterWidthToUse;
                    }
                    else {
                        _screenCharactersWidth = Convert.ToInt32(this["screenCharactersWidth"], CultureInfo.InvariantCulture);
                    }
                    _haveScreenCharactersWidth = true;
                }
                return _screenCharactersWidth;
            }
        }


        public virtual int ScreenCharactersHeight {
            get {
                if (!_haveScreenCharactersHeight) {
                    if (this["screenCharactersHeight"] == null) {
                        // calculate from best partial information

                        int screenPixelHeightToUse = 480;
                        int characterHeightToUse = 12;

                        if (this["screenPixelsHeight"] != null && this["characterHeight"] != null) {
                            screenPixelHeightToUse = Convert.ToInt32(this["screenPixelsHeight"], CultureInfo.InvariantCulture);
                            characterHeightToUse = Convert.ToInt32(this["characterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["screenPixelsHeight"] != null) {
                            screenPixelHeightToUse = Convert.ToInt32(this["screenPixelsHeight"], CultureInfo.InvariantCulture);
                            characterHeightToUse = Convert.ToInt32(this["defaultCharacterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["characterHeight"] != null) {
                            screenPixelHeightToUse = Convert.ToInt32(this["defaultScreenPixelsHeight"], CultureInfo.InvariantCulture);
                            characterHeightToUse = Convert.ToInt32(this["characterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["defaultScreenCharactersHeight"] != null) {
                            screenPixelHeightToUse = Convert.ToInt32(this["defaultScreenCharactersHeight"], CultureInfo.InvariantCulture);
                            characterHeightToUse = 1;
                        }

                        _screenCharactersHeight = screenPixelHeightToUse / characterHeightToUse;
                    }
                    else {
                        _screenCharactersHeight = Convert.ToInt32(this["screenCharactersHeight"], CultureInfo.InvariantCulture);
                    }
                    _haveScreenCharactersHeight = true;
                }
                return _screenCharactersHeight;
            }
        }


        public virtual int ScreenPixelsWidth {
            get {
                if (!_haveScreenPixelsWidth) {
                    if (this["screenPixelsWidth"] == null) {
                        // calculate from best partial information

                        int screenCharactersWidthToUse = 80;
                        int characterWidthToUse = 8;

                        if (this["screenCharactersWidth"] != null && this["characterWidth"] != null) {
                            screenCharactersWidthToUse = Convert.ToInt32(this["screenCharactersWidth"], CultureInfo.InvariantCulture);
                            characterWidthToUse = Convert.ToInt32(this["characterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["screenCharactersWidth"] != null) {
                            screenCharactersWidthToUse = Convert.ToInt32(this["screenCharactersWidth"], CultureInfo.InvariantCulture);
                            characterWidthToUse = Convert.ToInt32(this["defaultCharacterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["characterWidth"] != null) {
                            screenCharactersWidthToUse = Convert.ToInt32(this["defaultScreenCharactersWidth"], CultureInfo.InvariantCulture);
                            characterWidthToUse = Convert.ToInt32(this["characterWidth"], CultureInfo.InvariantCulture);
                        }
                        else if (this["defaultScreenPixelsWidth"] != null) {
                            screenCharactersWidthToUse = Convert.ToInt32(this["defaultScreenPixelsWidth"], CultureInfo.InvariantCulture);
                            characterWidthToUse = 1;
                        }

                        _screenPixelsWidth = screenCharactersWidthToUse * characterWidthToUse;
                    }
                    else {
                        _screenPixelsWidth = Convert.ToInt32(this["screenPixelsWidth"], CultureInfo.InvariantCulture);
                    }
                    _haveScreenPixelsWidth = true;
                }
                return _screenPixelsWidth;
            }
        }


        public virtual int ScreenPixelsHeight {
            get {
                if (!_haveScreenPixelsHeight) {
                    if (this["screenPixelsHeight"] == null) {
                        int screenCharactersHeightToUse = 480 / 12;
                        int characterHeightToUse = 12;

                        if (this["screenCharactersHeight"] != null && this["characterHeight"] != null) {
                            screenCharactersHeightToUse = Convert.ToInt32(this["screenCharactersHeight"], CultureInfo.InvariantCulture);
                            characterHeightToUse = Convert.ToInt32(this["characterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["screenCharactersHeight"] != null) {
                            screenCharactersHeightToUse = Convert.ToInt32(this["screenCharactersHeight"], CultureInfo.InvariantCulture);
                            characterHeightToUse = Convert.ToInt32(this["defaultCharacterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["characterHeight"] != null) {
                            screenCharactersHeightToUse = Convert.ToInt32(this["defaultScreenCharactersHeight"], CultureInfo.InvariantCulture);
                            characterHeightToUse = Convert.ToInt32(this["characterHeight"], CultureInfo.InvariantCulture);
                        }
                        else if (this["defaultScreenPixelsHeight"] != null) {
                            screenCharactersHeightToUse = Convert.ToInt32(this["defaultScreenPixelsHeight"], CultureInfo.InvariantCulture);
                            characterHeightToUse = 1;
                        }

                        _screenPixelsHeight = screenCharactersHeightToUse * characterHeightToUse;
                    }
                    else {
                        _screenPixelsHeight = Convert.ToInt32(this["screenPixelsHeight"], CultureInfo.InvariantCulture);
                    }
                    _haveScreenPixelsHeight = true;
                }
                return _screenPixelsHeight;
            }
        }


        public virtual int ScreenBitDepth {
            get {
                if (!_haveScreenBitDepth) {
                    _screenBitDepth = Convert.ToInt32(this["screenBitDepth"], CultureInfo.InvariantCulture);
                    _haveScreenBitDepth = true;
                }
                return _screenBitDepth;
            }
        }


        public virtual bool IsColor {
            get {
                if (!_haveIsColor) {
                    String isColorString = this["isColor"];
                    if (isColorString == null) {
                        _isColor = false;
                    }
                    else {
                        _isColor = Convert.ToBoolean(this["isColor"], CultureInfo.InvariantCulture);
                    }
                    _haveIsColor = true;
                }
                return _isColor;
            }
        }


        public virtual String InputType {
            get {
                if (!_haveInputType) {
                    _inputType = this["inputType"];
                    _haveInputType = true;
                }
                return _inputType;
            }
        }


        public virtual int NumberOfSoftkeys {
            get {
                if (!_haveNumberOfSoftkeys) {
                    _numberOfSoftkeys = Convert.ToInt32(this["numberOfSoftkeys"], CultureInfo.InvariantCulture);
                    _haveNumberOfSoftkeys = true;
                }
                return _numberOfSoftkeys;
            }
        }


        public virtual int MaximumSoftkeyLabelLength {
            get {
                if (!_haveMaximumSoftkeyLabelLength) {
                    _maximumSoftkeyLabelLength = Convert.ToInt32(this["maximumSoftkeyLabelLength"], CultureInfo.InvariantCulture);
                    _haveMaximumSoftkeyLabelLength = true;
                }
                return _maximumSoftkeyLabelLength;
            }
        }


        public virtual bool CanInitiateVoiceCall {
            get {
                if (!_haveCanInitiateVoiceCall) {
                    _canInitiateVoiceCall = CapsParseBoolDefault("canInitiateVoiceCall", false);
                    _haveCanInitiateVoiceCall = true;
                }
                return _canInitiateVoiceCall;
            }
        }


        public virtual bool CanSendMail {
            get {
                if (!_haveCanSendMail) {
                    _canSendMail = CapsParseBoolDefault("canSendMail", true);
                    _haveCanSendMail = true;
                }
                return _canSendMail;
            }
        }

        public virtual bool HasBackButton {
            get {
                if (!_haveHasBackButton) {
                    _hasBackButton = CapsParseBoolDefault("hasBackButton", true);
                    _haveHasBackButton = true;
                }
                return _hasBackButton;
            }
        }

        public virtual bool RendersWmlDoAcceptsInline {
            get {
                if (!_haveRendersWmlDoAcceptsInline) {
                    _rendersWmlDoAcceptsInline = CapsParseBoolDefault("rendersWmlDoAcceptsInline", true);
                    _haveRendersWmlDoAcceptsInline = true;
                }
                return _rendersWmlDoAcceptsInline;
            }
        }

        public virtual bool RendersWmlSelectsAsMenuCards {
            get {
                if (!_haveRendersWmlSelectsAsMenuCards) {
                    _rendersWmlSelectsAsMenuCards = CapsParseBoolDefault("rendersWmlSelectsAsMenuCards", false);
                    _haveRendersWmlSelectsAsMenuCards = true;
                }
                return _rendersWmlSelectsAsMenuCards;
            }
        }

        public virtual bool RendersBreaksAfterWmlAnchor {
            get {
                if (!_haveRendersBreaksAfterWmlAnchor) {
                    _rendersBreaksAfterWmlAnchor = CapsParseBoolDefault("rendersBreaksAfterWmlAnchor", true);
                    _haveRendersBreaksAfterWmlAnchor = true;
                }
                return _rendersBreaksAfterWmlAnchor;
            }
        }

        public virtual bool RendersBreaksAfterWmlInput {
            get {
                if (!_haveRendersBreaksAfterWmlInput) {
                    _rendersBreaksAfterWmlInput = CapsParseBoolDefault("rendersBreaksAfterWmlInput", true);
                    _haveRendersBreaksAfterWmlInput = true;
                }
                return _rendersBreaksAfterWmlInput;
            }
        }

        public virtual bool RendersBreakBeforeWmlSelectAndInput {
            get {
                if (!_haveRendersBreakBeforeWmlSelectAndInput) {
                    _rendersBreakBeforeWmlSelectAndInput = CapsParseBoolDefault("rendersBreakBeforeWmlSelectAndInput", false);
                    _haveRendersBreakBeforeWmlSelectAndInput = true;
                }
                return _rendersBreakBeforeWmlSelectAndInput;
            }
        }

        public virtual bool RequiresPhoneNumbersAsPlainText {
            get {
                if (!_haveRequiresPhoneNumbersAsPlainText) {
                    _requiresPhoneNumbersAsPlainText = CapsParseBoolDefault("requiresPhoneNumbersAsPlainText", false);
                    _haveRequiresPhoneNumbersAsPlainText = true;
                }
                return _requiresPhoneNumbersAsPlainText;
            }
        }

        public virtual bool RequiresUrlEncodedPostfieldValues {
            get {
                if (!_haveRequiresUrlEncodedPostfieldValues) {
                    _requiresUrlEncodedPostfieldValues = CapsParseBoolDefault("requiresUrlEncodedPostfieldValues", true);
                    _haveRequiresUrlEncodedPostfieldValues = true;
                }
                return _requiresUrlEncodedPostfieldValues;
            }
        }

        public virtual String RequiredMetaTagNameValue {
            get {
                if (!_haveRequiredMetaTagNameValue) {
                    String value = this["requiredMetaTagNameValue"];
                    if (!String.IsNullOrEmpty(value)) {
                        _requiredMetaTagNameValue = value;
                    }
                    else {
                        _requiredMetaTagNameValue = null;
                    }
                    _haveRequiredMetaTagNameValue = true;
                }
                return _requiredMetaTagNameValue;
            }
        }

        public virtual bool RendersBreaksAfterHtmlLists {
            get {
                if (!_haveRendersBreaksAfterHtmlLists) {
                    _rendersBreaksAfterHtmlLists = CapsParseBoolDefault("rendersBreaksAfterHtmlLists", true);
                    _haveRendersBreaksAfterHtmlLists = true;
                }
                return _rendersBreaksAfterHtmlLists;
            }
        }

        public virtual bool RequiresUniqueHtmlInputNames {
            get {
                if (!_haveRequiresUniqueHtmlInputNames) {
                    _requiresUniqueHtmlInputNames = CapsParseBoolDefault("requiresUniqueHtmlInputNames", false);
                    _haveRequiresUniqueHtmlInputNames = true;
                }
                return _requiresUniqueHtmlInputNames;
            }
        }

        public virtual bool RequiresUniqueHtmlCheckboxNames {
            get {
                if (!_haveRequiresUniqueHtmlCheckboxNames) {
                    _requiresUniqueHtmlCheckboxNames = CapsParseBoolDefault("requiresUniqueHtmlCheckboxNames", false);
                    _haveRequiresUniqueHtmlCheckboxNames = true;
                }
                return _requiresUniqueHtmlCheckboxNames;
            }
        }

        public virtual bool SupportsCss {
            get {
                if (!_haveSupportsCss) {
                    _supportsCss = CapsParseBoolDefault("supportsCss", false);
                    _haveSupportsCss = true;
                }
                return _supportsCss;
            }
        }

        public virtual bool HidesRightAlignedMultiselectScrollbars {
            get {
                if (!_haveHidesRightAlignedMultiselectScrollbars) {
                    _hidesRightAlignedMultiselectScrollbars = CapsParseBoolDefault("hidesRightAlignedMultiselectScrollbars", false);
                    _haveHidesRightAlignedMultiselectScrollbars = true;
                }
                return _hidesRightAlignedMultiselectScrollbars;
            }
        }

        public virtual bool IsMobileDevice {
            get {
                if (!_haveIsMobileDevice) {
                    _isMobileDevice = CapsParseBoolDefault("isMobileDevice", false);
                    _haveIsMobileDevice = true;
                }
                return _isMobileDevice;
            }
        }

        public virtual bool RequiresAttributeColonSubstitution {
            get {
                if (!_haveRequiresAttributeColonSubstitution) {
                    _requiresAttributeColonSubstitution = CapsParseBoolDefault("requiresAttributeColonSubstitution", false);
                    _haveRequiresAttributeColonSubstitution = true;
                }
                return _requiresAttributeColonSubstitution;
            }
        }

        public virtual bool CanRenderOneventAndPrevElementsTogether {
            get {
                if (!_haveCanRenderOneventAndPrevElementsTogether) {
                    _canRenderOneventAndPrevElementsTogether = CapsParseBoolDefault("canRenderOneventAndPrevElementsTogether", true);
                    _haveCanRenderOneventAndPrevElementsTogether = true;
                }
                return _canRenderOneventAndPrevElementsTogether;
            }
        }

        public virtual bool CanRenderInputAndSelectElementsTogether {
            get {
                if (!_haveCanRenderInputAndSelectElementsTogether) {
                    _canRenderInputAndSelectElementsTogether = CapsParseBoolDefault("canRenderInputAndSelectElementsTogether", true);
                    _haveCanRenderInputAndSelectElementsTogether = true;
                }
                return _canRenderInputAndSelectElementsTogether;
            }
        }

        public virtual bool CanRenderAfterInputOrSelectElement {
            get {
                if (!_haveCanRenderAfterInputOrSelectElement) {
                    _canRenderAfterInputOrSelectElement = CapsParseBoolDefault("canRenderAfterInputOrSelectElement", true);
                    _haveCanRenderAfterInputOrSelectElement = true;
                }
                return _canRenderAfterInputOrSelectElement;
            }
        }

        public virtual bool CanRenderPostBackCards {
            get {
                if (!_haveCanRenderPostBackCards) {
                    _canRenderPostBackCards = CapsParseBoolDefault("canRenderPostBackCards", true);
                    _haveCanRenderPostBackCards = true;
                }
                return _canRenderPostBackCards;
            }
        }

        public virtual bool CanRenderMixedSelects {
            get {
                if (!_haveCanRenderMixedSelects) {
                    _canRenderMixedSelects = CapsParseBoolDefault("canRenderMixedSelects", true);
                    _haveCanRenderMixedSelects = true;
                }
                return _canRenderMixedSelects;
            }
        }

        public virtual bool CanCombineFormsInDeck {
            get {
                if (!_haveCanCombineFormsInDeck) {
                    _canCombineFormsInDeck = CapsParseBoolDefault("canCombineFormsInDeck", true);
                    _haveCanCombineFormsInDeck = true;
                }
                return _canCombineFormsInDeck;
            }
        }

        public virtual bool CanRenderSetvarZeroWithMultiSelectionList {
            get {
                if (!_haveCanRenderSetvarZeroWithMultiSelectionList) {
                    _canRenderSetvarZeroWithMultiSelectionList = CapsParseBoolDefault("canRenderSetvarZeroWithMultiSelectionList", true);
                    _haveCanRenderSetvarZeroWithMultiSelectionList = true;
                }
                return _canRenderSetvarZeroWithMultiSelectionList;
            }
        }

        public virtual bool SupportsImageSubmit {
            get {
                if (!_haveSupportsImageSubmit) {
                    _supportsImageSubmit = CapsParseBoolDefault("supportsImageSubmit", false);
                    _haveSupportsImageSubmit = true;
                }
                return _supportsImageSubmit;
            }
        }

        public virtual bool RequiresUniqueFilePathSuffix {
            get {
                if (!_haveRequiresUniqueFilePathSuffix) {
                    _requiresUniqueFilePathSuffix = CapsParseBoolDefault("requiresUniqueFilePathSuffix", false);
                    _haveRequiresUniqueFilePathSuffix = true;
                }
                return _requiresUniqueFilePathSuffix;
            }
        }

        public virtual bool RequiresNoBreakInFormatting {
            get {
                if (!_haveRequiresNoBreakInFormatting) {
                    _requiresNoBreakInFormatting = CapsParseBoolDefault("requiresNoBreakInFormatting", false);
                    _haveRequiresNoBreakInFormatting = true;
                }
                return _requiresNoBreakInFormatting;
            }
        }

        public virtual bool RequiresLeadingPageBreak {
            get {
                if (!_haveRequiresLeadingPageBreak) {
                    _requiresLeadingPageBreak = CapsParseBoolDefault("requiresLeadingPageBreak", false);
                    _haveRequiresLeadingPageBreak = true;
                }
                return _requiresLeadingPageBreak;
            }
        }

        public virtual bool SupportsSelectMultiple {
            get {
                if (!_haveSupportsSelectMultiple) {
                    _supportsSelectMultiple = CapsParseBoolDefault("supportsSelectMultiple", false);
                    _haveSupportsSelectMultiple = true;
                }
                return _supportsSelectMultiple;
            }
        }

        public /*new*/ virtual bool SupportsBold {
            get {
                if (!_haveSupportsBold) {
                    _supportsBold = CapsParseBoolDefault("supportsBold", true);
                    _haveSupportsBold = true;
                }
                return _supportsBold;
            }
        }

        public /*new*/ virtual bool SupportsItalic {
            get {
                if (!_haveSupportsItalic) {
                    _supportsItalic = CapsParseBoolDefault("supportsItalic", true);
                    _haveSupportsItalic = true;
                }
                return _supportsItalic;
            }
        }

        public virtual bool SupportsFontSize {
            get {
                if (!_haveSupportsFontSize) {
                    _supportsFontSize = CapsParseBoolDefault("supportsFontSize", false);
                    _haveSupportsFontSize = true;
                }
                return _supportsFontSize;
            }
        }

        public virtual bool SupportsFontName {
            get {
                if (!_haveSupportsFontName) {
                    _supportsFontName = CapsParseBoolDefault("supportsFontName", false);
                    _haveSupportsFontName = true;
                }
                return _supportsFontName;
            }
        }

        public virtual bool SupportsFontColor {
            get {
                if (!_haveSupportsFontColor) {
                    _supportsFontColor = CapsParseBoolDefault("supportsFontColor", false);
                    _haveSupportsFontColor = true;
                }
                return _supportsFontColor;
            }
        }

        public virtual bool SupportsBodyColor {
            get {
                if (!_haveSupportsBodyColor) {
                    _supportsBodyColor = CapsParseBoolDefault("supportsBodyColor", false);
                    _haveSupportsBodyColor = true;
                }
                return _supportsBodyColor;
            }
        }

        public virtual bool SupportsDivAlign {
            get {
                if (!_haveSupportsDivAlign) {
                    _supportsDivAlign = CapsParseBoolDefault("supportsDivAlign", false);
                    _haveSupportsDivAlign = true;
                }
                return _supportsDivAlign;
            }
        }

        public virtual bool SupportsDivNoWrap {
            get {
                if (!_haveSupportsDivNoWrap) {
                    _supportsDivNoWrap = CapsParseBoolDefault("supportsDivNoWrap", false);
                    _haveSupportsDivNoWrap = true;
                }
                return _supportsDivNoWrap;
            }
        }

        internal bool SupportsMaintainScrollPositionOnPostback {
            get {
                if (!_haveSupportsMaintainScrollPositionOnPostback) {
                    _supportsMaintainScrollPositionOnPostback = CapsParseBoolDefault("supportsMaintainScrollPositionOnPostback", false);
                    _haveSupportsMaintainScrollPositionOnPostback = true;
                }
                return _supportsMaintainScrollPositionOnPostback;
            }
        }

        public virtual bool RequiresContentTypeMetaTag {
            get {
                if (!_haveRequiresContentTypeMetaTag) {
                    _requiresContentTypeMetaTag = CapsParseBoolDefault("requiresContentTypeMetaTag", false);
                    _haveRequiresContentTypeMetaTag = true;
                }
                return _requiresContentTypeMetaTag;
            }
        }

        public virtual bool RequiresDBCSCharacter {
            get {
                if (!_haveRequiresDBCSCharacter) {
                    _requiresDBCSCharacter = CapsParseBoolDefault("requiresDBCSCharacter", false);
                    _haveRequiresDBCSCharacter = true;
                }
                return _requiresDBCSCharacter;
            }
        }

        public virtual bool RequiresHtmlAdaptiveErrorReporting {
            get {
                if (!_haveRequiresHtmlAdaptiveErrorReporting) {
                    _requiresHtmlAdaptiveErrorReporting = CapsParseBoolDefault("requiresHtmlAdaptiveErrorReporting", false);
                    _haveRequiresHtmlAdaptiveErrorReporting = true;
                }
                return _requiresHtmlAdaptiveErrorReporting;
            }
        }

        public virtual bool RequiresOutputOptimization {
            get {
                if (!_haveRequiresOutputOptimization) {
                    _requiresOutputOptimization = CapsParseBoolDefault("requiresOutputOptimization", false);
                    _haveRequiresOutputOptimization = true;
                }
                return _requiresOutputOptimization;
            }
        }

        public virtual bool SupportsAccesskeyAttribute {
            get {
                if (!_haveSupportsAccesskeyAttribute) {
                    _supportsAccesskeyAttribute = CapsParseBoolDefault("supportsAccesskeyAttribute", false);
                    _haveSupportsAccesskeyAttribute = true;
                }
                return _supportsAccesskeyAttribute;
            }
        }

        public virtual bool SupportsInputIStyle {
            get {
                if (!_haveSupportsInputIStyle) {
                    _supportsInputIStyle = CapsParseBoolDefault("supportsInputIStyle", false);
                    _haveSupportsInputIStyle = true;
                }
                return _supportsInputIStyle;
            }
        }

        public virtual bool SupportsInputMode {
            get {
                if (!_haveSupportsInputMode) {
                    _supportsInputMode = CapsParseBoolDefault("supportsInputMode", false);
                    _haveSupportsInputMode = true;
                }
                return _supportsInputMode;
            }
        }

        public virtual bool SupportsIModeSymbols {
            get {
                if (!_haveSupportsIModeSymbols) {
                    _supportsIModeSymbols = CapsParseBoolDefault("supportsIModeSymbols", false);
                    _haveSupportsIModeSymbols = true;
                }
                return _supportsIModeSymbols;
            }
        }

        public virtual bool SupportsJPhoneSymbols {
            get {
                if (!_haveSupportsJPhoneSymbols) {
                    _supportsJPhoneSymbols = CapsParseBoolDefault("supportsJPhoneSymbols", false);
                    _haveSupportsJPhoneSymbols = true;
                }
                return _supportsJPhoneSymbols;
            }
        }

        public virtual bool SupportsJPhoneMultiMediaAttributes {
            get {
                if (!_haveSupportsJPhoneMultiMediaAttributes) {
                    _supportsJPhoneMultiMediaAttributes = CapsParseBoolDefault("supportsJPhoneMultiMediaAttributes", false);
                    _haveSupportsJPhoneMultiMediaAttributes = true;
                }
                return _supportsJPhoneMultiMediaAttributes;
            }
        }

        public virtual int MaximumRenderedPageSize {
            get {
                if (!_haveMaximumRenderedPageSize) {
                    _maximumRenderedPageSize = Convert.ToInt32(this["maximumRenderedPageSize"], CultureInfo.InvariantCulture);
                    _haveMaximumRenderedPageSize = true;
                }
                return _maximumRenderedPageSize;
            }
        }

        public virtual bool RequiresSpecialViewStateEncoding {
            get {
                if (!_haveRequiresSpecialViewStateEncoding) {
                    _requiresSpecialViewStateEncoding = CapsParseBoolDefault("requiresSpecialViewStateEncoding", false);
                    _haveRequiresSpecialViewStateEncoding = true;
                }
                return _requiresSpecialViewStateEncoding;
            }
        }

        public virtual bool SupportsQueryStringInFormAction {
            get {
                if (!_haveSupportsQueryStringInFormAction) {
                    _supportsQueryStringInFormAction = CapsParseBoolDefault("supportsQueryStringInFormAction", true);
                    _haveSupportsQueryStringInFormAction = true;
                }
                return _supportsQueryStringInFormAction;
            }
        }

        public virtual bool SupportsCacheControlMetaTag {
            get {
                if (!_haveSupportsCacheControlMetaTag) {
                    _supportsCacheControlMetaTag = CapsParseBoolDefault("supportsCacheControlMetaTag", true);
                    _haveSupportsCacheControlMetaTag = true;
                }
                return _supportsCacheControlMetaTag;
            }
        }

        public virtual bool SupportsUncheck {
            get {
                if (!_haveSupportsUncheck) {
                    _supportsUncheck = CapsParseBoolDefault("supportsUncheck", true);
                    _haveSupportsUncheck = true;
                }
                return _supportsUncheck;
            }
        }

        public virtual bool CanRenderEmptySelects {
            get {
                if (!_haveCanRenderEmptySelects) {
                    _canRenderEmptySelects = CapsParseBoolDefault("canRenderEmptySelects", true);
                    _haveCanRenderEmptySelects = true;
                }
                return _canRenderEmptySelects;
            }
        }

        public virtual bool SupportsRedirectWithCookie {
            get {
                if (!_haveSupportsRedirectWithCookie) {
                    _supportsRedirectWithCookie = CapsParseBoolDefault("supportsRedirectWithCookie", true);
                    _haveSupportsRedirectWithCookie = true;
                }
                return _supportsRedirectWithCookie;
            }
        }

        public virtual bool SupportsEmptyStringInCookieValue {
            get {
                if (!_haveSupportsEmptyStringInCookieValue) {
                    _supportsEmptyStringInCookieValue = CapsParseBoolDefault("supportsEmptyStringInCookieValue", true);
                    _haveSupportsEmptyStringInCookieValue = true;
                }
                return _supportsEmptyStringInCookieValue;
            }
        }

        public virtual int DefaultSubmitButtonLimit {
            get {
                if (!_haveDefaultSubmitButtonLimit) {
                    String s = this["defaultSubmitButtonLimit"];
                    _defaultSubmitButtonLimit = s != null ? Convert.ToInt32(this["defaultSubmitButtonLimit"], CultureInfo.InvariantCulture) : 1;
                    _haveDefaultSubmitButtonLimit = true;
                }
                return _defaultSubmitButtonLimit;
            }
        }

        public virtual bool SupportsXmlHttp {
            get {
                if (!_haveSupportsXmlHttp) {
                    _supportsXmlHttp = CapsParseBoolDefault("supportsXmlHttp", false);
                    _haveSupportsXmlHttp = true;
                }
                return _supportsXmlHttp;
            }
        }

        public virtual bool SupportsCallback {
            get {
                if (!_haveSupportsCallback) {
                    _supportsCallback = CapsParseBoolDefault("supportsCallback", false);
                    _haveSupportsCallback = true;
                }
                return _supportsCallback;
            }
        }

        public virtual int MaximumHrefLength {
            get {
                if (!_haveMaximumHrefLength) {
                    _maximumHrefLength = Convert.ToInt32(this["maximumHrefLength"], CultureInfo.InvariantCulture);
                    _haveMaximumHrefLength = true;
                }
                return _maximumHrefLength;
            }
        }

        public bool IsBrowser(string browserName) {
            if (String.IsNullOrEmpty(browserName)) {
                return false;
            }

            if (_browsers == null) {
                return false;
            }

            for (int i = 0; i < _browsers.Count; i++) {
                if (String.Equals(browserName, (string)_browsers[i], StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            
            return false;
        }

        public void AddBrowser(string browserName) {
            if (_browsers == null) {
                lock (_staticLock) {
                    if (_browsers == null) {
                        _browsers = new ArrayList(6);
                    }
                }
            }
            _browsers.Add(browserName.ToLower(CultureInfo.InvariantCulture));
        }

        private ArrayList _browsers;

        volatile private string _type;
        volatile private string _browser;
        volatile private string _version;
        volatile private int _majorversion;
        private double _minorversion;
        volatile private string _platform;
        volatile private Type _tagwriter;
        volatile private Version _ecmascriptversion;
        volatile private Version _jscriptversion;
        volatile private Version _msdomversion;
        volatile private Version _w3cdomversion;

        volatile private bool _beta;
        volatile private bool _crawler;
        volatile private bool _aol;
        volatile private bool _win16;
        volatile private bool _win32;
        volatile private bool _requiresControlStateInSession;

        volatile private bool _frames;
        //private bool _supportsbold;
        //private bool _supportsitalic;
        volatile private bool _tables;
        volatile private bool _cookies;
        volatile private bool _vbscript;
        volatile private bool _javascript;
        volatile private bool _javaapplets;
        volatile private bool _activexcontrols;
        volatile private bool _backgroundsounds;
        volatile private bool _cdf;

        volatile private bool _havetype;
        volatile private bool _havebrowser;
        volatile private bool _haveversion;
        volatile private bool _havemajorversion;
        volatile private bool _haveminorversion;
        volatile private bool _haveplatform;
        volatile private bool _havetagwriter;
        volatile private bool _haveecmascriptversion;
        volatile private bool _havemsdomversion;
        volatile private bool _havew3cdomversion;

        volatile private bool _havebeta;
        volatile private bool _havecrawler;
        volatile private bool _haveaol;
        volatile private bool _havewin16;
        volatile private bool _havewin32;

        volatile private bool _haveframes;
        volatile private bool _haverequiresControlStateInSession;
        //private bool _havesupportsbold;
        //private bool _havesupportsitalic;
        volatile private bool _havetables;
        volatile private bool _havecookies;
        volatile private bool _havevbscript;
        volatile private bool _havejavascript;
        volatile private bool _havejavaapplets;
        volatile private bool _haveactivexcontrols;
        volatile private bool _havebackgroundsounds;
        volatile private bool _havecdf;

        //previously in System.Web.Mobile
        volatile private String _mobileDeviceManufacturer;
        volatile private String _mobileDeviceModel;
        volatile private String _gatewayVersion;
        volatile private int _gatewayMajorVersion;
        private double _gatewayMinorVersion;
        volatile private String _preferredRenderingType;     // 
        volatile private String _preferredRenderingMime;
        volatile private String _preferredImageMime;
        volatile private String _requiredMetaTagNameValue;
        volatile private String _preferredRequestEncoding;
        volatile private String _preferredResponseEncoding;
        volatile private int _screenCharactersWidth;
        volatile private int _screenCharactersHeight;
        volatile private int _screenPixelsWidth;
        volatile private int _screenPixelsHeight;
        volatile private int _screenBitDepth;
        volatile private bool _isColor;
        volatile private String _inputType;
        volatile private int _numberOfSoftkeys;
        volatile private int _maximumSoftkeyLabelLength;
        volatile private bool _canInitiateVoiceCall;
        volatile private bool _canSendMail;
        volatile private bool _hasBackButton;

        volatile private bool _rendersWmlDoAcceptsInline;
        volatile private bool _rendersWmlSelectsAsMenuCards;
        volatile private bool _rendersBreaksAfterWmlAnchor;
        volatile private bool _rendersBreaksAfterWmlInput;
        volatile private bool _rendersBreakBeforeWmlSelectAndInput;
        volatile private bool _requiresPhoneNumbersAsPlainText;
        volatile private bool _requiresAttributeColonSubstitution;
        volatile private bool _requiresUrlEncodedPostfieldValues;
        volatile private bool _rendersBreaksAfterHtmlLists;
        volatile private bool _requiresUniqueHtmlCheckboxNames;
        volatile private bool _requiresUniqueHtmlInputNames;
        volatile private bool _supportsCss;
        volatile private bool _hidesRightAlignedMultiselectScrollbars;
        volatile private bool _isMobileDevice;
        volatile private bool _canRenderOneventAndPrevElementsTogether;
        volatile private bool _canRenderInputAndSelectElementsTogether;
        volatile private bool _canRenderAfterInputOrSelectElement;
        volatile private bool _canRenderPostBackCards;
        volatile private bool _canRenderMixedSelects;
        volatile private bool _canCombineFormsInDeck;
        volatile private bool _canRenderSetvarZeroWithMultiSelectionList;
        volatile private bool _supportsImageSubmit;
        volatile private bool _requiresUniqueFilePathSuffix;
        volatile private bool _requiresNoBreakInFormatting;
        volatile private bool _requiresLeadingPageBreak;
        volatile private bool _supportsSelectMultiple;
        volatile private bool _supportsBold;
        volatile private bool _supportsItalic;
        volatile private bool _supportsFontSize;
        volatile private bool _supportsFontName;
        volatile private bool _supportsFontColor;
        volatile private bool _supportsBodyColor;
        volatile private bool _supportsDivAlign;
        volatile private bool _supportsDivNoWrap;
        volatile private bool _requiresHtmlAdaptiveErrorReporting;
        volatile private bool _requiresContentTypeMetaTag;
        volatile private bool _requiresDBCSCharacter;
        volatile private bool _requiresOutputOptimization;
        volatile private bool _supportsAccesskeyAttribute;
        volatile private bool _supportsInputIStyle;
        volatile private bool _supportsInputMode;
        volatile private bool _supportsIModeSymbols;
        volatile private bool _supportsJPhoneSymbols;
        volatile private bool _supportsJPhoneMultiMediaAttributes;
        volatile private int _maximumRenderedPageSize;
        volatile private bool _requiresSpecialViewStateEncoding;
        volatile private bool _supportsQueryStringInFormAction;
        volatile private bool _supportsCacheControlMetaTag;
        volatile private bool _supportsUncheck;
        volatile private bool _canRenderEmptySelects;
        volatile private bool _supportsRedirectWithCookie;
        volatile private bool _supportsEmptyStringInCookieValue;
        volatile private int _defaultSubmitButtonLimit;
        volatile private bool _supportsXmlHttp;
        volatile private bool _supportsCallback;
        volatile private bool _supportsMaintainScrollPositionOnPostback;
        volatile private int _maximumHrefLength;

        volatile private bool _haveMobileDeviceManufacturer;
        volatile private bool _haveMobileDeviceModel;
        volatile private bool _haveGatewayVersion;
        volatile private bool _haveGatewayMajorVersion;
        volatile private bool _haveGatewayMinorVersion;
        volatile private bool _havePreferredRenderingType;
        volatile private bool _havePreferredRenderingMime;
        volatile private bool _havePreferredImageMime;
        volatile private bool _havePreferredRequestEncoding;
        volatile private bool _havePreferredResponseEncoding;
        volatile private bool _haveScreenCharactersWidth;
        volatile private bool _haveScreenCharactersHeight;
        volatile private bool _haveScreenPixelsWidth;
        volatile private bool _haveScreenPixelsHeight;
        volatile private bool _haveScreenBitDepth;
        volatile private bool _haveIsColor;
        volatile private bool _haveInputType;
        volatile private bool _haveNumberOfSoftkeys;
        volatile private bool _haveMaximumSoftkeyLabelLength;
        volatile private bool _haveCanInitiateVoiceCall;
        volatile private bool _haveCanSendMail;
        volatile private bool _haveHasBackButton;
        volatile private bool _haveRendersWmlDoAcceptsInline;
        volatile private bool _haveRendersWmlSelectsAsMenuCards;
        volatile private bool _haveRendersBreaksAfterWmlAnchor;
        volatile private bool _haveRendersBreaksAfterWmlInput;
        volatile private bool _haveRendersBreakBeforeWmlSelectAndInput;
        volatile private bool _haveRequiresPhoneNumbersAsPlainText;
        volatile private bool _haveRequiresUrlEncodedPostfieldValues;
        volatile private bool _haveRequiredMetaTagNameValue;
        volatile private bool _haveRendersBreaksAfterHtmlLists;
        volatile private bool _haveRequiresUniqueHtmlCheckboxNames;
        volatile private bool _haveRequiresUniqueHtmlInputNames;
        volatile private bool _haveSupportsCss;
        volatile private bool _haveHidesRightAlignedMultiselectScrollbars;
        volatile private bool _haveIsMobileDevice;
        volatile private bool _haveCanRenderOneventAndPrevElementsTogether;
        volatile private bool _haveCanRenderInputAndSelectElementsTogether;
        volatile private bool _haveCanRenderAfterInputOrSelectElement;
        volatile private bool _haveCanRenderPostBackCards;
        volatile private bool _haveCanCombineFormsInDeck;
        volatile private bool _haveCanRenderMixedSelects;
        volatile private bool _haveCanRenderSetvarZeroWithMultiSelectionList;
        volatile private bool _haveSupportsImageSubmit;
        volatile private bool _haveRequiresUniqueFilePathSuffix;
        volatile private bool _haveRequiresNoBreakInFormatting;
        volatile private bool _haveRequiresLeadingPageBreak;
        volatile private bool _haveSupportsSelectMultiple;
        volatile private bool _haveRequiresAttributeColonSubstitution;
        volatile private bool _haveRequiresHtmlAdaptiveErrorReporting;
        volatile private bool _haveRequiresContentTypeMetaTag;
        volatile private bool _haveRequiresDBCSCharacter;
        volatile private bool _haveRequiresOutputOptimization;
        volatile private bool _haveSupportsAccesskeyAttribute;
        volatile private bool _haveSupportsInputIStyle;
        volatile private bool _haveSupportsInputMode;
        volatile private bool _haveSupportsIModeSymbols;
        volatile private bool _haveSupportsJPhoneSymbols;
        volatile private bool _haveSupportsJPhoneMultiMediaAttributes;
        volatile private bool _haveSupportsRedirectWithCookie;
        volatile private bool _haveSupportsEmptyStringInCookieValue = false;

        volatile private bool _haveSupportsBold;
        volatile private bool _haveSupportsItalic;
        volatile private bool _haveSupportsFontSize;
        volatile private bool _haveSupportsFontName;
        volatile private bool _haveSupportsFontColor;
        volatile private bool _haveSupportsBodyColor;
        volatile private bool _haveSupportsDivAlign;
        volatile private bool _haveSupportsDivNoWrap;
        volatile private bool _haveMaximumRenderedPageSize;
        volatile private bool _haveRequiresSpecialViewStateEncoding;
        volatile private bool _haveSupportsQueryStringInFormAction;
        volatile private bool _haveSupportsCacheControlMetaTag;
        volatile private bool _haveSupportsUncheck;
        volatile private bool _haveCanRenderEmptySelects;
        volatile private bool _haveDefaultSubmitButtonLimit;
        volatile private bool _haveSupportsXmlHttp;
        volatile private bool _haveSupportsCallback;
        volatile private bool _haveSupportsMaintainScrollPositionOnPostback;
        volatile private bool _haveMaximumHrefLength;
        volatile private bool _havejscriptversion;

        #region IFilterResolutionService implementation

        /// <internalonly/>
        bool IFilterResolutionService.EvaluateFilter(string filterName) {
            return IsBrowser(filterName);
        }

        /// <internalonly/>
        int IFilterResolutionService.CompareFilters(string filter1, string filter2) {
            return BrowserCapabilitiesCompiler.BrowserCapabilitiesFactory.CompareFilters(filter1, filter2);
        }
        #endregion

    }
}
