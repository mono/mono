//------------------------------------------------------------------------------
// <copyright file="PagesSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;

    /*                <!-- pages Attributes:
          buffer="[true|false]"                         // Default: true
          enableSessionState="[true|false|ReadOnly]"    // Default: true
          enableViewState="[true|false]"                // Default: true
          enableViewStateMac="[true|false]"             // Default: true
          enableEventValidation="[true|false]"          // Default: true
          maxPageStateFieldLength="[int]"               // Default: -1(off)
          smartNavigation="[true|false]"                // Default: false
          autoEventWireup="[true|false]"                // Default: true
          pageBaseType="[typename]"                     // Default: System.Web.UI.Page
          userControlBaseType="[typename]"              // Default: System.Web.UI.UserControl
          validateRequest="[true|false]"                // Default: true
          compilationMode="[Auto|Never|Always]"         // Default: Always
          viewStateEncryptionMode=[Auto|Never|Always]"  // Default: Auto
          maintainScrollPositionOnPostBack="[true|false]"   // Default: false
          asyncTimeout="[seconds]"                      // Default: 45
          renderAllHiddenFieldsAtTopOfForm="[true|false]"   // Default: true
          clientIDMode="[Inherit|AutoID|Predictable|Static]"    // Default: Predictable
        -->
        <pages buffer="true" enableSessionState="true" enableViewState="true"
            enableViewStateMac="true" enableEventValidation="true" autoEventWireup="true" validateRequest="true" maintainScrollPositionOnPostBack="true">

            <!-- controls example:
            Note that this section is only valid in web.config in the application root.
            The same tagPrefix can be used to map to multiple assemblies or namespaces
            as shown in the example.
            <controls>
                <add tagPrefix="acme" tagName="uc" src="controls/uc.ascx" />
                <add tagPrefix="my" namespace="MyControls.BasicControls" assembly="MyControls" />
                <add tagPrefix="my" namespace="MyControls.EnhancedControls" assembly="MyControls" />
            </controls>
            -->

            <controls>
                <add tagPrefix="asp" namespace="System.Web.UI.WebControls.WebParts" assembly="System.Web, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
            </controls>

            <namespaces>
                <add namespace="System" />
                <add namespace="System.Collections" />
                <add namespace="System.Collections.Specialized" />
                <add namespace="System.ComponentModel" />
                <add namespace="System.Configuration" />
                <add namespace="System.Text" />
                <add namespace="System.Text.RegularExpressions" />
                <add namespace="System.Web" />
                <add namespace="System.Web.Caching" />
                <add namespace="System.Web.SessionState" />
                <add namespace="System.Web.Security" />
                <add namespace="System.Web.Profile" />
                <add namespace="System.Web.UI" />
                <add namespace="System.Web.UI.Imaging" />
                <add namespace="System.Web.UI.WebControls" />
                <add namespace="System.Web.UI.WebControls.WebParts" />
                <add namespace="System.Web.UI.HtmlControls" />
            </namespaces>

            <!-- tagMapping example:
            <tagMapping>
                <add tagTypeName="[type name]"
                     mappedTagTypeName="[type name]" />
                <remove tagTypeName="[type name]" />
                <clear />
            </tagMapping>
            -->
        </pages>



*/

    public sealed class PagesSection : ConfigurationSection {
        private static readonly Version _controlRenderingDefaultVersion = VersionUtil.FrameworkDefault;
        private static readonly Version _controlRenderingMinimumVersion = VersionUtil.Framework35;

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propBuffer =
            new ConfigurationProperty("buffer", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propControlRenderingCompatibilityVersion =
            new ConfigurationProperty("controlRenderingCompatibilityVersion",
                                      typeof(Version),
                                      _controlRenderingDefaultVersion,
                                      StdValidatorsAndConverters.VersionConverter, //typeConverter
                                      new VersionValidator(_controlRenderingMinimumVersion), //baseValidator
                                      ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableSessionState =
            new ConfigurationProperty("enableSessionState", typeof(string), "true", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableViewState =
            new ConfigurationProperty("enableViewState", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableViewStateMac =
            new ConfigurationProperty("enableViewStateMac", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableEventValidation =
            new ConfigurationProperty("enableEventValidation", typeof(bool), Page.EnableEventValidationDefault, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propSmartNavigation =
            new ConfigurationProperty("smartNavigation", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propAutoEventWireup =
            new ConfigurationProperty("autoEventWireup", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPageBaseType =
            new ConfigurationProperty("pageBaseType", typeof(string), "System.Web.UI.Page", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUserControlBaseType =
            new ConfigurationProperty("userControlBaseType", typeof(string), "System.Web.UI.UserControl", ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propValidateRequest =
            new ConfigurationProperty("validateRequest", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMasterPageFile =
            new ConfigurationProperty("masterPageFile", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTheme =
            new ConfigurationProperty("theme", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propNamespaces =
            new ConfigurationProperty("namespaces", typeof(NamespaceCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propControls =
            new ConfigurationProperty("controls", typeof(TagPrefixCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propTagMapping =
            new ConfigurationProperty("tagMapping", typeof(TagMapCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propMaxPageStateFieldLength =
            new ConfigurationProperty("maxPageStateFieldLength", typeof(int), Page.DefaultMaxPageStateFieldLength, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCompilationMode =
            new ConfigurationProperty("compilationMode", typeof(CompilationMode), CompilationMode.Always, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propStyleSheetTheme =
            new ConfigurationProperty("styleSheetTheme", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPageParserFilterType =
            new ConfigurationProperty("pageParserFilterType", typeof(string), String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propViewStateEncryptionMode =
            new ConfigurationProperty("viewStateEncryptionMode", typeof(ViewStateEncryptionMode), ViewStateEncryptionMode.Auto, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaintainScrollPosition =
            new ConfigurationProperty("maintainScrollPositionOnPostBack", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propAsyncTimeout =
            new ConfigurationProperty("asyncTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromSeconds((double)Page.DefaultAsyncTimeoutSeconds),
                                        StdValidatorsAndConverters.TimeSpanSecondsConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRenderAllHiddenFieldsAtTopOfForm =
            new ConfigurationProperty("renderAllHiddenFieldsAtTopOfForm", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propClientIDMode =
            new ConfigurationProperty("clientIDMode", typeof(ClientIDMode), ClientIDMode.Predictable, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propIgnoreDeviceFilters =
            new ConfigurationProperty("ignoreDeviceFilters", typeof(IgnoreDeviceFilterElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        private VirtualPath _virtualPath;
        private string _masterPageFile;
        private Type _pageBaseType;
        private Type _userControlBaseType;
        private Type _pageParserFilterType;
        private bool _themeChecked;
        private bool _styleSheetThemeChecked;
        private ClientIDMode? _clientIDMode;
        private Version _controlRenderingCompatibilityVersion;

        // <prefix, TagNamespaceRegisterEntry>
        private TagNamespaceRegisterEntryTable _tagNamespaceRegisterEntries;

        // <prefix:tagname, UserControlRegisterEntry>
        private Hashtable _userControlRegisterEntries;


        static PagesSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propBuffer);
            _properties.Add(_propControlRenderingCompatibilityVersion);
            _properties.Add(_propEnableSessionState);
            _properties.Add(_propEnableViewState);
            _properties.Add(_propEnableViewStateMac);
            _properties.Add(_propEnableEventValidation);
            _properties.Add(_propSmartNavigation);
            _properties.Add(_propAutoEventWireup);
            _properties.Add(_propPageBaseType);
            _properties.Add(_propUserControlBaseType);
            _properties.Add(_propValidateRequest);
            _properties.Add(_propMasterPageFile);
            _properties.Add(_propTheme);
            _properties.Add(_propStyleSheetTheme);
            _properties.Add(_propNamespaces);
            _properties.Add(_propControls);
            _properties.Add(_propTagMapping);
            _properties.Add(_propMaxPageStateFieldLength);
            _properties.Add(_propCompilationMode);
            _properties.Add(_propPageParserFilterType);
            _properties.Add(_propViewStateEncryptionMode);
            _properties.Add(_propMaintainScrollPosition);
            _properties.Add(_propAsyncTimeout);
            _properties.Add(_propRenderAllHiddenFieldsAtTopOfForm);
            _properties.Add(_propClientIDMode);
            _properties.Add(_propIgnoreDeviceFilters);
        }

                public PagesSection()                 {
                }
                /*
                protected override void InitializeDefault()
                {
        /* No Init Basic Map
            Controls.Add(new TagPrefixInfo("asp", "System.Web.UI.WebControls.WebParts",
                        "System.Web, Version="+ThisAssembly.Version+", Culture=neutral, PublicKeyToken="+AssemblyRef.MicrosoftPublicKey,
                        null, null));
*/
        /*
        }
*/
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("buffer", DefaultValue = true)]
        public bool Buffer {
            get {
                return (bool)base[_propBuffer];
            }
            set {
                base[_propBuffer] = value;
            }
        }

        [ConfigurationProperty("enableSessionState", DefaultValue = "true")]
        public PagesEnableSessionState EnableSessionState {
            get {   // note that the values of true and false are True and False
                // in the enum and need to be true and false in the file
                // so we cannot simple use the values from the enum
                // "true" and "false" are not legal values for the enum
                // since they are part of the language
                PagesEnableSessionState temp = PagesEnableSessionState.True;
                switch ((string)base[_propEnableSessionState]) {
                    case "true":
                        temp = PagesEnableSessionState.True;
                        break;
                    case "false":
                        temp = PagesEnableSessionState.False;
                        break;
                    case "ReadOnly":
                        temp = PagesEnableSessionState.ReadOnly;
                        break;
                    default:
                        // throw here cause this is a bad value
                        string PropName = _propEnableSessionState.Name;
                        string LegalValues = "true, false, ReadOnly";
                        throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_enum_attribute, PropName, LegalValues));
                }
                return (PagesEnableSessionState)temp;
            }
            set {
                string tempStr = "true";
                switch (value) {
                    case PagesEnableSessionState.True:
                        tempStr = "true";
                        break;
                    case PagesEnableSessionState.False:
                        tempStr = "false";
                        break;
                    case PagesEnableSessionState.ReadOnly:
                        tempStr = "ReadOnly";
                        break;
                    default:
                        tempStr = "true";
                        break;
                }
                base[_propEnableSessionState] = tempStr;
            }
        }

        [ConfigurationProperty("enableViewState", DefaultValue = true)]
        public bool EnableViewState {
            get {
                return (bool)base[_propEnableViewState];
            }
            set {
                base[_propEnableViewState] = value;
            }
        }

        [ConfigurationProperty("enableViewStateMac", DefaultValue = true)]
        public bool EnableViewStateMac {
            get {
                return (bool)base[_propEnableViewStateMac];
            }
            set {
                base[_propEnableViewStateMac] = value;
            }
        }

        [ConfigurationProperty("enableEventValidation", DefaultValue = Page.EnableEventValidationDefault)]
        public bool EnableEventValidation {
            get {
                return (bool)base[_propEnableEventValidation];
            }
            set {
                base[_propEnableEventValidation] = value;
            }
        }

        [ConfigurationProperty("smartNavigation", DefaultValue = false)]
        public bool SmartNavigation {
            get {
                return (bool)base[_propSmartNavigation];
            }
            set {
                base[_propSmartNavigation] = value;
            }
        }

        [ConfigurationProperty("autoEventWireup", DefaultValue = true)]
        public bool AutoEventWireup {
            get {
                return (bool)base[_propAutoEventWireup];
            }
            set {
                base[_propAutoEventWireup] = value;
            }
        }

        [ConfigurationProperty("maintainScrollPositionOnPostBack", DefaultValue = false)]
        public bool MaintainScrollPositionOnPostBack {
            get {
                return (bool)base[_propMaintainScrollPosition];
            }
            set {
                base[_propMaintainScrollPosition] = value;
            }
        }


        [ConfigurationProperty("pageBaseType", DefaultValue = "System.Web.UI.Page")]
        public string PageBaseType {
            get {
                return (string)base[_propPageBaseType];
            }
            set {
                base[_propPageBaseType] = value;
            }
        }

        [ConfigurationProperty("userControlBaseType", DefaultValue = "System.Web.UI.UserControl")]
        public string UserControlBaseType {
            get {
                return (string)base[_propUserControlBaseType];
            }
            set {
                base[_propUserControlBaseType] = value;
            }
        }

        internal Type PageBaseTypeInternal {
            get {
                if (_pageBaseType == null &&
                    ElementInformation.Properties[_propPageBaseType.Name].ValueOrigin != PropertyValueOrigin.Default) {
                    lock (this) {
                        if (_pageBaseType == null) {
                            Type pageBaseType = ConfigUtil.GetType(PageBaseType, "pageBaseType", this);
                            ConfigUtil.CheckBaseType(typeof(System.Web.UI.Page), pageBaseType, "pageBaseType", this);
                            _pageBaseType = pageBaseType;
                        }
                    }
                }

                return _pageBaseType;
            }
        }

        internal Type UserControlBaseTypeInternal {
            get {
                if (_userControlBaseType == null &&
                    ElementInformation.Properties[_propUserControlBaseType.Name].ValueOrigin != PropertyValueOrigin.Default) {
                    lock (this) {
                        if (_userControlBaseType == null) {
                            Type userControlBaseType = ConfigUtil.GetType(
                                            UserControlBaseType, 
                                            "userControlBaseType", 
                                            this);
                            ConfigUtil.CheckBaseType(typeof(System.Web.UI.UserControl), 
                                                     userControlBaseType, 
                                                     "userControlBaseType", 
                                                     this);
                            _userControlBaseType = userControlBaseType;
                        }
                    }
                }

                return _userControlBaseType;
            }
        }

        [ConfigurationProperty("pageParserFilterType", DefaultValue = "")]
        public string PageParserFilterType {
            get {
                return (string)base[_propPageParserFilterType];
            }
            set {
                base[_propPageParserFilterType] = value;
            }
        }

        internal Type PageParserFilterTypeInternal {
            get {
                if (PageParser.DefaultPageParserFilterType != null) {
                    return PageParser.DefaultPageParserFilterType;
                }

                // If pageParserFilterType is an empty string, we treat this as meaning 'no filter',
                // possibly overriding one specified on a parent web.config
                if (_pageParserFilterType == null && !String.IsNullOrEmpty(PageParserFilterType)) {
                    Type pageParserFilterType = ConfigUtil.GetType(PageParserFilterType, "pageParserFilterType", this);
                    ConfigUtil.CheckBaseType(typeof(PageParserFilter), pageParserFilterType, "pageParserFilterType", this);
                    _pageParserFilterType = pageParserFilterType;
                }

                return _pageParserFilterType;
            }
        }

        internal PageParserFilter CreateControlTypeFilter() {
            Type pageParserFilterType = PageParserFilterTypeInternal;

            // If no filter type is registered, return null
            if (pageParserFilterType == null)
                return null;

            // Create an instance of the filter
            return (PageParserFilter)HttpRuntime.CreateNonPublicInstance(pageParserFilterType);
        }

        [ConfigurationProperty("validateRequest", DefaultValue = true)]
        public bool ValidateRequest {
            get {
                return (bool)base[_propValidateRequest];
            }
            set {
                base[_propValidateRequest] = value;
            }
        }

        [ConfigurationProperty("masterPageFile", DefaultValue = "")]
        public string MasterPageFile {
            get {
                return (string)base[_propMasterPageFile];
            }
            set {
                base[_propMasterPageFile] = value;
            }
        }

        internal string MasterPageFileInternal {
            get {
                if (_masterPageFile == null) {
                    String masterPageFile = MasterPageFile;

                    if (!String.IsNullOrEmpty(masterPageFile)) {
                        if (UrlPath.IsAbsolutePhysicalPath(masterPageFile)) {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.Physical_path_not_allowed, masterPageFile),
                                ElementInformation.Properties["masterPageFile"].Source,
                                ElementInformation.Properties["masterPageFile"].LineNumber);
                        }

                        VirtualPath masterPageVirtualPath;

                        try {
                            masterPageVirtualPath = VirtualPath.CreateNonRelative(masterPageFile);
                        }
                        catch (Exception ex) {
                            throw new ConfigurationErrorsException(ex.Message, ex,
                                ElementInformation.Properties["masterPageFile"].Source,
                                ElementInformation.Properties["masterPageFile"].LineNumber);
                        }

                        if (!Util.VirtualFileExistsWithAssert(masterPageVirtualPath)) {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.FileName_does_not_exist, masterPageFile),
                                ElementInformation.Properties["masterPageFile"].Source,
                                ElementInformation.Properties["masterPageFile"].LineNumber);
                        }

                        string extension = UrlPath.GetExtension(masterPageFile);
                        Type buildProviderType =
                            CompilationUtil.GetBuildProviderTypeFromExtension(_virtualPath, extension, BuildProviderAppliesTo.Web, false);

                        if (!typeof(MasterPageBuildProvider).IsAssignableFrom(buildProviderType)) {
                            throw new ConfigurationErrorsException(
                                SR.GetString(SR.Bad_masterPage_ext),
                                ElementInformation.Properties["masterPageFile"].Source,
                                ElementInformation.Properties["masterPageFile"].LineNumber);
                        }

                        // Convert it to appRelative format
                        masterPageFile = masterPageVirtualPath.AppRelativeVirtualPathString;
                    }
                    else {
                        masterPageFile = String.Empty;
                    }

                    _masterPageFile = masterPageFile;
                }

                return _masterPageFile;
            }
        }

        [ConfigurationProperty("theme", DefaultValue = "")]
        public string Theme {
            get {
                return (string)base[_propTheme];
            }
            set {
                base[_propTheme] = value;
            }
        }

        internal string ThemeInternal {
            get {
                string themeName = Theme;

                if (!_themeChecked) {
                    if ((!String.IsNullOrEmpty(themeName)) && (!Util.ThemeExists(themeName))) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Page_theme_not_found, themeName), 
                            ElementInformation.Properties["theme"].Source, 
                            ElementInformation.Properties["theme"].LineNumber);
                    }
                    _themeChecked = true;
                }

                return themeName;
            }
        }

        [ConfigurationProperty("styleSheetTheme", DefaultValue = "")]
        public string StyleSheetTheme {
            get {
                return (string)base[_propStyleSheetTheme];
            }
            set {
                base[_propStyleSheetTheme] = value;
            }
        }

        internal string StyleSheetThemeInternal {
            get {
                string styleSheetThemeName = StyleSheetTheme;

                if (!_styleSheetThemeChecked) {
                    if (!String.IsNullOrEmpty(styleSheetThemeName) && 
                        (!Util.ThemeExists(styleSheetThemeName))) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Page_theme_not_found, styleSheetThemeName), 
                            ElementInformation.Properties["styleSheetTheme"].Source, 
                            ElementInformation.Properties["styleSheetTheme"].LineNumber);
                    }
                    _styleSheetThemeChecked = true;
                }

                return styleSheetThemeName;
            }
        }

        [ConfigurationProperty("namespaces")]
        public NamespaceCollection Namespaces {
            get {
                return (NamespaceCollection)base[_propNamespaces];
            }
        }

        [ConfigurationProperty("controls")]
        public TagPrefixCollection Controls {
            get {
                return (TagPrefixCollection)base[_propControls];
            }
        }

        [ConfigurationProperty("maxPageStateFieldLength", DefaultValue = -1)]
        public int MaxPageStateFieldLength {
            get {
                return (int)base[_propMaxPageStateFieldLength];
            }
            set {
                base[_propMaxPageStateFieldLength] = value;
            }
        }

        [ConfigurationProperty("tagMapping")]
        public TagMapCollection TagMapping {
            get {
                return (TagMapCollection)base[_propTagMapping];
            }
        }

        [ConfigurationProperty("compilationMode", DefaultValue = CompilationMode.Always)]
        public CompilationMode CompilationMode {
            get {
                return (CompilationMode)base[_propCompilationMode];
            }
            set {
                base[_propCompilationMode] = value;
            }
        }

        [ConfigurationProperty("viewStateEncryptionMode", DefaultValue = ViewStateEncryptionMode.Auto)]
        public ViewStateEncryptionMode ViewStateEncryptionMode {
            get {
                return (ViewStateEncryptionMode)base[_propViewStateEncryptionMode];
            }
            set {
                base[_propViewStateEncryptionMode] = value;
            }
        }

        [ConfigurationProperty("asyncTimeout", DefaultValue = "00:00:45")]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan AsyncTimeout {
            get {
                return (TimeSpan)base[_propAsyncTimeout];
            }
            set {
                base[_propAsyncTimeout] = value;
            }
        }

        [ConfigurationProperty("renderAllHiddenFieldsAtTopOfForm", DefaultValue = true)]
        public bool RenderAllHiddenFieldsAtTopOfForm {
            get {
                return (bool)base[_propRenderAllHiddenFieldsAtTopOfForm];
            }
            set {
                base[_propRenderAllHiddenFieldsAtTopOfForm] = value;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="Member")]
        [ConfigurationProperty("clientIDMode", DefaultValue = ClientIDMode.Predictable)]
        public ClientIDMode ClientIDMode {
            get {
                if (_clientIDMode == null) {
                    _clientIDMode = (ClientIDMode)base[_propClientIDMode];
                }
                return (ClientIDMode)_clientIDMode;
            }
            set {
                base[_propClientIDMode] = value;
                _clientIDMode = value;
            }
        }

        [ConfigurationProperty("controlRenderingCompatibilityVersion", DefaultValue = VersionUtil.FrameworkDefaultString)]
        [ConfigurationValidator(typeof(VersionValidator))]
        [TypeConverter(typeof(VersionConverter))]
        public Version ControlRenderingCompatibilityVersion {
            get {
                if(_controlRenderingCompatibilityVersion == null) {
                    _controlRenderingCompatibilityVersion = (Version)base[_propControlRenderingCompatibilityVersion];
                }
                return _controlRenderingCompatibilityVersion;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                base[_propControlRenderingCompatibilityVersion] = value;
                _controlRenderingCompatibilityVersion = value;
            }
        }

        [ConfigurationProperty("ignoreDeviceFilters")]
        public IgnoreDeviceFilterElementCollection IgnoreDeviceFilters {
            get {
                return (IgnoreDeviceFilterElementCollection)base[_propIgnoreDeviceFilters];
            }
        }

        internal TagNamespaceRegisterEntryTable TagNamespaceRegisterEntriesInternal {
            get {
                if (_tagNamespaceRegisterEntries == null) {
                    lock (this) {
                        if (_tagNamespaceRegisterEntries == null) {
                            FillInRegisterEntries();
                        }
                    }
                }

                return _tagNamespaceRegisterEntries;
            }
        }

        internal void FillInRegisterEntries() {
            // 





            TagNamespaceRegisterEntryTable tagNamespaceRegisterEntries = new TagNamespaceRegisterEntryTable();
            foreach (TagNamespaceRegisterEntry entry in DefaultTagNamespaceRegisterEntries) {
                tagNamespaceRegisterEntries[entry.TagPrefix] = new ArrayList(new object[] { entry });
            }

            Hashtable userControlRegisterEntries = new Hashtable(StringComparer.OrdinalIgnoreCase);

            // Fill in the collection
            foreach (TagPrefixInfo tpi in Controls) {
                if (!String.IsNullOrEmpty(tpi.TagName)) {
                    UserControlRegisterEntry ucRegisterEntry = new UserControlRegisterEntry(tpi.TagPrefix, tpi.TagName);
                    ucRegisterEntry.ComesFromConfig = true;
                    try {
                        ucRegisterEntry.UserControlSource = VirtualPath.CreateNonRelative(tpi.Source);
                    }
                    catch (Exception e) {
                        throw new ConfigurationErrorsException(e.Message, e,
                            tpi.ElementInformation.Properties["src"].Source,
                            tpi.ElementInformation.Properties["src"].LineNumber);
                    }

                    userControlRegisterEntries[ucRegisterEntry.Key] = ucRegisterEntry;
                }
                else if (!String.IsNullOrEmpty(tpi.Namespace)) {
                    TagNamespaceRegisterEntry nsRegisterEntry = new TagNamespaceRegisterEntry(tpi.TagPrefix, tpi.Namespace, tpi.Assembly);
                    ArrayList entries = null;

                    entries = (ArrayList)tagNamespaceRegisterEntries[tpi.TagPrefix];
                    if (entries == null) {
                        entries = new ArrayList();
                        tagNamespaceRegisterEntries[tpi.TagPrefix] = entries;
                    }

                    entries.Add(nsRegisterEntry);
                }
            }

            _tagNamespaceRegisterEntries = tagNamespaceRegisterEntries;
            _userControlRegisterEntries = userControlRegisterEntries;
        }

        internal static ICollection DefaultTagNamespaceRegisterEntries {
            get {
                TagNamespaceRegisterEntry aspEntry = new TagNamespaceRegisterEntry("asp", "System.Web.UI.WebControls", AssemblyRef.SystemWeb);
                TagNamespaceRegisterEntry mobileEntry = new TagNamespaceRegisterEntry("mobile", "System.Web.UI.MobileControls", AssemblyRef.SystemWebMobile);

                return new TagNamespaceRegisterEntry[] { aspEntry, mobileEntry };
            }
        }

        internal Hashtable UserControlRegisterEntriesInternal {
            get {
                if (_userControlRegisterEntries == null) {
                    lock (this) {
                        if (_userControlRegisterEntries == null) {
                            FillInRegisterEntries();
                        }
                    }
                }
                return _userControlRegisterEntries;
            }
        }

        protected override void DeserializeSection(XmlReader reader) {
            WebContext context;

            base.DeserializeSection(reader);

            // Determine hosting context
            context = EvaluationContext.HostingContext as WebContext;

            if (context != null) {
                // Make sure it has a trailing slash as it is used as a base path to Combine with relative
                _virtualPath = VirtualPath.CreateNonRelativeTrailingSlashAllowNull(context.Path);
            }
        }

        // This is called as the last step of the deserialization process before the newly created section is seen by the consumer.
        // We can use it to change defaults on-the-fly.
        protected override void SetReadOnly() {
            // Unless overridden, set <pages controlRenderingCompatibilityVersion="4.5" />
            ConfigUtil.SetFX45DefaultValue(this, _propControlRenderingCompatibilityVersion, VersionUtil.Framework45);

            base.SetReadOnly();
        }
    }
}
