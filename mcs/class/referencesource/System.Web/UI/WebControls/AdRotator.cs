//------------------------------------------------------------------------------
// <copyright file="AdRotator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.IO;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.UI;
    using System.Web.Caching;
    using System.Web;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Xml;
    using System.Globalization;
    using System.Web.Util;
    using System.Reflection;
    using System.Text;


    /// <devdoc>
    ///    <para>Displays a randomly selected ad banner on a page.</para>
    /// </devdoc>
    [
    DefaultEvent("AdCreated"),
    DefaultProperty("AdvertisementFile"),
    Designer("System.Web.UI.Design.WebControls.AdRotatorDesigner, " + AssemblyRef.SystemDesign),
    ToolboxData("<{0}:AdRotator runat=\"server\"></{0}:AdRotator>")
    ]
    public class AdRotator : DataBoundControl {

        private static readonly object EventAdCreated = new object();

        private const string XmlDocumentTag = "Advertisements";
        private const string XmlDocumentRootXPath = "/" + XmlDocumentTag;
        private const string XmlAdTag = "Ad";

        private const string KeywordProperty = "Keyword";
        private const string ImpressionsProperty = "Impressions";

        // static copy of the Random object. This is a pretty hefty object to
        // initialize, so you don't want to create one each time.
        private static Random _random;

        private String _baseUrl;
        private string _advertisementFile;
        private AdCreatedEventArgs _adCreatedEventArgs;

        private AdRec [] _adRecs;
        private bool _isPostCacheAdHelper;
        private string _uniqueID;

        private static readonly Type _adrotatorType = typeof(AdRotator);
        private static readonly Type[] _AdCreatedParameterTypes = {typeof(AdCreatedEventArgs)};


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.AdRotator'/> class.</para>
        /// </devdoc>
        public AdRotator() {
        }


        /// <devdoc>
        ///    <para>Gets or sets the path to the XML file that contains advertisement data.</para>
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Behavior"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.XmlUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.AdRotator_AdvertisementFile)
        ]
        public string AdvertisementFile {
            get {
                return((_advertisementFile == null) ? String.Empty : _advertisementFile);
            }
            set {
                _advertisementFile = value;
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(AdCreatedEventArgs.AlternateTextElement),
        WebSysDescription(SR.AdRotator_AlternateTextField)
        ]
        public String AlternateTextField {
            get {
                String s = (String) ViewState["AlternateTextField"];
                return((s != null) ? s : AdCreatedEventArgs.AlternateTextElement);
            }
            set {
                ViewState["AlternateTextField"] = value;
            }
        }

        /// <devdoc>
        ///   The base url corresponds for mapping of other url elements such as
        ///   imageUrl and navigateUrl.
        /// </devdoc>
        internal String BaseUrl {
            get {
                if (_baseUrl == null) {
                    // Deal with app relative syntax (e.g. ~/foo)
                    string tplSourceDir = TemplateControlVirtualDirectory.VirtualPathString;

                    // For the AdRotator, use the AdvertisementFile directory as the base, and fall back to the
                    // page/user control location as the base.
                    String absoluteFile = null;
                    String fileDirectory = null;
                    if (!String.IsNullOrEmpty(AdvertisementFile)) {
                        absoluteFile = UrlPath.Combine(tplSourceDir, AdvertisementFile);
                        fileDirectory = UrlPath.GetDirectory(absoluteFile);
                    }

                    _baseUrl = string.Empty;
                    if (fileDirectory != null) {
                        _baseUrl = fileDirectory;
                    }
                    if (_baseUrl.Length == 0) {
                        _baseUrl = tplSourceDir;
                    }
                }
                return _baseUrl;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///    Font property. Has no effect on this control, so hide it.
        /// </devdoc>
        [
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override FontInfo Font {
            get {
                return base.Font;
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(AdCreatedEventArgs.ImageUrlElement),
        WebSysDescription(SR.AdRotator_ImageUrlField)
        ]
        public String ImageUrlField {
            get {
                String s = (String) ViewState["ImageUrlField"];
                return((s != null) ? s : AdCreatedEventArgs.ImageUrlElement);
            }
            set {
                ViewState["ImageUrlField"] = value;
            }
        }

        private bool IsTargetSet {
            get {
                return (ViewState["Target"] != null);
            }
        }

        internal bool IsPostCacheAdHelper {
            get {
                return _isPostCacheAdHelper;
            }
            set {
                _isPostCacheAdHelper = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a category keyword used for matching related advertisements in the advertisement file.</para>
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.AdRotator_KeywordFilter)
        ]
        public string KeywordFilter {
            get {
                string s = (string)ViewState["KeywordFilter"];
                return((s == null) ? String.Empty : s);
            }
            set {
                // trim the filter value
                if (String.IsNullOrEmpty(value)) {
                    ViewState.Remove("KeywordFilter");
                }
                else {
                    ViewState["KeywordFilter"] = value.Trim();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(AdCreatedEventArgs.NavigateUrlElement),
        WebSysDescription(SR.AdRotator_NavigateUrlField)
        ]
        public String NavigateUrlField {
            get {
                String s = (String) ViewState["NavigateUrlField"];
                return((s != null) ? s : AdCreatedEventArgs.NavigateUrlElement);
            }
            set {
                ViewState["NavigateUrlField"] = value;
            }
        }


        private AdCreatedEventArgs SelectedAdArgs {
            get {
                return _adCreatedEventArgs;
            }
            set {
                _adCreatedEventArgs = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets
        ///       or sets the name of the browser window or frame to display the advertisement.</para>
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Behavior"),
        DefaultValue("_top"),
        WebSysDescription(SR.AdRotator_Target),
        TypeConverter(typeof(TargetConverter))
        ]
        public string Target {
            get {
                string s = (string)ViewState["Target"];
                return((s == null) ? "_top" : s);
            }
            set {
                ViewState["Target"] = value;
            }
        }


        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.A;
            }
        }

        public override string UniqueID {
            get {
                if (_uniqueID == null) {
                    _uniqueID = base.UniqueID;
                }
                return _uniqueID;
            }
        }


        /// <devdoc>
        ///    <para>Occurs once per round trip after the creation of the
        ///       control before the page is rendered. </para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.AdRotator_OnAdCreated)
        ]
        public event AdCreatedEventHandler AdCreated {
            add {
                Events.AddHandler(EventAdCreated, value);
            }
            remove {
                Events.RemoveHandler(EventAdCreated, value);
            }
        }

        private void CheckOnlyOneDataSource() {
            int numOfDataSources = ((AdvertisementFile.Length > 0) ? 1 : 0);
            numOfDataSources += ((DataSourceID.Length > 0) ? 1 : 0);
            numOfDataSources += ((DataSource != null) ? 1 : 0);

            if (numOfDataSources > 1) {
                throw new HttpException(SR.GetString(SR.AdRotator_only_one_datasource, ID));
            }
        }

        // Currently this is designed to be called when PostCache Substitution is being initialized
        internal void CopyFrom(AdRotator adRotator) {
            _adRecs = adRotator._adRecs;

            AccessKey = adRotator.AccessKey;
            AlternateTextField = adRotator.AlternateTextField;
            Enabled = adRotator.Enabled;
            ImageUrlField = adRotator.ImageUrlField;
            NavigateUrlField = adRotator.NavigateUrlField;
            TabIndex = adRotator.TabIndex;
            Target = adRotator.Target;
            ToolTip = adRotator.ToolTip;

            string id = adRotator.ID;
            if (!String.IsNullOrEmpty(id)) {
                ID = adRotator.ClientID;
            }

            // Below are properties that need to be handled specially and saved
            // to private variables.
            _uniqueID = adRotator.UniqueID;
            _baseUrl = adRotator.BaseUrl;

            // Special copy to properties that cannot be assigned directly
            if (adRotator.HasAttributes) {
                foreach(string key in adRotator.Attributes.Keys) {
                    Attributes[key] = adRotator.Attributes[key];
                }
            }

            if (adRotator.ControlStyleCreated) {
                ControlStyle.CopyFrom(adRotator.ControlStyle);
            }
        }


        private ArrayList CreateAutoGeneratedFields(IEnumerable dataSource) {
            if (dataSource == null) {
                return null;
            }

            ArrayList generatedFields = new ArrayList();
            PropertyDescriptorCollection propertyDescriptors = null;

            if (dataSource is ITypedList) {
                propertyDescriptors =
                    ((ITypedList)dataSource).GetItemProperties(new PropertyDescriptor[0]);
            }

            if (propertyDescriptors == null) {

                IEnumerator enumerator = dataSource.GetEnumerator();
                if (enumerator.MoveNext()) {

                    Object sampleItem = enumerator.Current;
                    if (IsBindableType(sampleItem.GetType())) {
                        // Raise error since we are expecting some record
                        // containing multiple data values.
                        throw new HttpException(SR.GetString(SR.AdRotator_expect_records_with_advertisement_properties,
                                ID, sampleItem.GetType()));
                    }
                    else {
                        propertyDescriptors = TypeDescriptor.GetProperties(sampleItem);
                    }
                }
            }
            if (propertyDescriptors != null && propertyDescriptors.Count > 0) {

                foreach (PropertyDescriptor pd in propertyDescriptors) {
                    if (IsBindableType(pd.PropertyType)) {
                        generatedFields.Add(pd.Name);
                    }
                }
            }

            return generatedFields;
        }


        // 








        internal bool DoPostCacheSubstitutionAsNeeded(HtmlTextWriter writer) {
            if (!IsPostCacheAdHelper && SelectedAdArgs == null &&
                Page.Response.HasCachePolicy &&
                (int)Page.Response.Cache.GetCacheability() != (int)HttpCacheabilityLimits.None) {

                // The checking of the cacheability is to see if the page is output cached
                AdPostCacheSubstitution adPostCacheSubstitution = new AdPostCacheSubstitution(this);
                adPostCacheSubstitution.RegisterPostCacheCallBack(Context, Page, writer);
                return true;
            }
            return false;
        }

        /// <devdoc>
        ///     <para>Select an ad from ad records and create the event
        ///     argument object.</para>
        /// </devdoc>
        private AdCreatedEventArgs GetAdCreatedEventArgs() {
            IDictionary adInfo = SelectAdFromRecords();
            AdCreatedEventArgs adArgs =
                new AdCreatedEventArgs(adInfo,
                                       ImageUrlField,
                                       NavigateUrlField,
                                       AlternateTextField);
           return adArgs;
        }


        private AdRec [] GetDataSourceData(IEnumerable dataSource) {

            ArrayList fields = CreateAutoGeneratedFields(dataSource);

            ArrayList adDicts = new ArrayList();
            IEnumerator enumerator = dataSource.GetEnumerator();
            while(enumerator.MoveNext()) {
                IDictionary dict = null;
                foreach (String field in fields){
                    if (dict == null) {
                        dict = new HybridDictionary();
                    }
                    dict.Add(field, DataBinder.GetPropertyValue(enumerator.Current, field));
                }

                if (dict != null) {
                    adDicts.Add(dict);
                }
            }

            return SetAdRecs(adDicts);
        }


        /// <devdoc>
        ///   Gets the ad data for the given file by loading the file, or reading from the
        ///   application-level cache.
        /// </devdoc>
        private AdRec [] GetFileData(string fileName) {

            // VSWhidbey 208626: Adopting similar code from xml.cs to support virtual path provider

            // First, figure out if it's a physical or virtual path
            VirtualPath virtualPath;
            string physicalPath;
            ResolvePhysicalOrVirtualPath(fileName, out virtualPath, out physicalPath);

            // try to get it from the ASP.NET cache
            string fileKey = CacheInternal.PrefixAdRotator + ((!String.IsNullOrEmpty(physicalPath)) ?
                physicalPath : virtualPath.VirtualPathString);
            CacheStoreProvider cacheInternal = System.Web.HttpRuntime.Cache.InternalCache;
            AdRec[] adRecs = cacheInternal.Get(fileKey) as AdRec[];

            if (adRecs == null) {
                // Otherwise load it
                CacheDependency dependency;
                try {
                    using (Stream stream = OpenFileAndGetDependency(virtualPath, physicalPath, out dependency)) {
                        adRecs = LoadStream(stream);
                        Debug.Assert(adRecs != null);
                    }
                }
                catch (Exception e) {
                    if (!String.IsNullOrEmpty(physicalPath) && HttpRuntime.HasPathDiscoveryPermission(physicalPath)) {
                        // We want to catch the error message, but not propage the inner exception. Otherwise we can throw up
                        // logon prompts through IE;
                        throw new HttpException(SR.GetString(SR.AdRotator_cant_open_file, ID, e.Message));
                    }
                    else {
                        throw new HttpException(SR.GetString(SR.AdRotator_cant_open_file_no_permission, ID));
                    }
                }

                // Cache it, but only if we got a dependency
                if (dependency != null) {
                    using (dependency) {
                        // and store it in the cache, dependent on the file name
                        cacheInternal.Insert(fileKey, adRecs, new CacheInsertOptions() { Dependencies = dependency });
                    }
                }
            }
            return adRecs;
        }

        private static int GetRandomNumber(int maxValue) {
            if (_random == null) {
                _random = new Random();
            }
            return _random.Next(maxValue) + 1;
        }

        private AdRec [] GetXmlDataSourceData(XmlDataSource xmlDataSource) {
            Debug.Assert(xmlDataSource != null);

            XmlDocument doc = xmlDataSource.GetXmlDocument();
            if (doc == null) {
                return null;
            }
            return LoadXmlDocument(doc);
        }

        private bool IsBindableType(Type type) {
            return(type.IsPrimitive ||
                   (type == typeof(String)) ||
                   (type == typeof(DateTime)) ||
                   (type == typeof(Decimal)));
        }

        private bool IsOnAdCreatedOverridden() {
            bool result = false;
            Type type = this.GetType();
            if (type != _adrotatorType) {
                MethodInfo methodInfo = type.GetMethod("OnAdCreated",
                                                       BindingFlags.NonPublic | BindingFlags.Instance,
                                                       null,
                                                       _AdCreatedParameterTypes,
                                                       null);
                if (methodInfo.DeclaringType != _adrotatorType) {
                    result = true;
                }
            }
            return result;
        }

        private AdRec [] LoadFromXmlReader(XmlReader reader) {
            ArrayList adDicts = new ArrayList();

            while (reader.Read()) {
                if (reader.Name == "Advertisements") {
                    if (reader.Depth != 0) {
                        return null;
                    }
                    break;
                }
            }

            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Ad" && reader.Depth == 1) {

                    IDictionary dict = null;
                    reader.Read();
                    while (!(reader.NodeType == XmlNodeType.EndElement)) {
                        if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement) {
                            if (dict == null) {
                                dict = new HybridDictionary();
                            }
                            dict.Add(reader.LocalName, reader.ReadString());
                        }
                        reader.Skip();
                    }

                    if (dict != null) {
                        adDicts.Add(dict);
                    }
                }
            }

            AdRec [] adRecs = SetAdRecs(adDicts);
            return adRecs;
        }

        /// <devdoc>
        ///   Loads the given XML stream into an array of AdRec structures
        /// </devdoc>
        private AdRec [] LoadStream(Stream stream) {

            AdRec [] adRecs = null;
            try {
                // Read the XML stream into an array of dictionaries
                XmlReader reader = XmlUtils.CreateXmlReader(stream);

                // Perf: We use LoadFromXmlReader instead of LoadXmlDocument to
                // do the text parsing only once
                adRecs = LoadFromXmlReader(reader);
            }
            catch (Exception e) {
                throw new HttpException(
                    SR.GetString(SR.AdRotator_parse_error, ID, e.Message), e);
            }

            if (adRecs == null) {
                throw new HttpException(
                    SR.GetString(SR.AdRotator_no_advertisements, ID, AdvertisementFile));
            }

            return adRecs;
        }

        private AdRec [] LoadXmlDocument(XmlDocument doc) {
            // Read the XML data into an array of dictionaries
            ArrayList adDicts = new ArrayList();

            if (doc.DocumentElement != null &&
                doc.DocumentElement.LocalName == XmlDocumentTag) {

                XmlNode elem = doc.DocumentElement.FirstChild;

                while (elem != null) {
                    IDictionary dict = null;
                    if (elem.LocalName.Equals(XmlAdTag)) {
                        XmlNode prop = elem.FirstChild;
                        while (prop != null) {
                            if (prop.NodeType == XmlNodeType.Element) {
                                if (dict == null) {
                                    dict = new HybridDictionary();
                                }
                                dict.Add(prop.LocalName, prop.InnerText);
                            }
                            prop = prop.NextSibling;
                        }
                    }
                    if (dict != null) {
                        adDicts.Add(dict);
                    }
                    elem = elem.NextSibling;
                }
            }

            AdRec [] adRecs = SetAdRecs(adDicts);
            return adRecs;
        }

        /// <devdoc>
        ///   Used to determine if the advertisement meets current criteria. Does a comparison with
        ///   KeywordFilter if it is set.
        /// </devdoc>
        private bool MatchingAd(AdRec adRec, string keywordFilter) {
            Debug.Assert(keywordFilter != null && keywordFilter.Length > 0);
            return(String.Equals(keywordFilter, adRec.keyword, StringComparison.OrdinalIgnoreCase));
        }


        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.AdRotator.AdCreated'/> event for an <see cref='System.Web.UI.WebControls.AdRotator'/>.</para>
        /// </devdoc>
        protected virtual void OnAdCreated(AdCreatedEventArgs e) {
            AdCreatedEventHandler handler = (AdCreatedEventHandler)Events[EventAdCreated];
            if (handler != null) handler(this, e);
        }


        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            // VSWhidbey 419600: We just always need binding data every time since
            // AdRotator doesn't store the entire Ad data in ViewState for selecting
            // Ad during postbacks.  It's too big for storing in ViewState.
            RequiresDataBinding = true;
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets the advertisement information for rendering in its parameter, then calls
        ///     the OnAdCreated event to render the ads.</para>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // If after PreRender (which would call DataBind if DataSource or DataSourceID available)
            // and no _adRecs created, it must be the normal v1 behavior which uses ad file.
            if (_adRecs == null && AdvertisementFile.Length > 0) {
                PerformAdFileBinding();
            }

            // If handler is specified, we don't do any post-cache
            // substitution because the handler code would not be executed.
            //
            // VSWhidbey 213759: We also don't want any post-cache substitution
            // if OnAdCreated has been overridden
            if (Events[EventAdCreated] != null || IsOnAdCreatedOverridden()) {
                // Fire the user event for further customization
                SelectedAdArgs = GetAdCreatedEventArgs();
                OnAdCreated(SelectedAdArgs);
            }
        }

        private void PerformAdFileBinding() {
            // Getting ad data from physical file is V1 way which is not supported
            // by the base class DataBoundControl so we had above code to handle
            // this case.  However, we need to support DataBound control events
            // in Whidbey and since above code doesn't go through the event
            // raising in the base class DataBoundControl, here we mimic them.
            OnDataBinding(EventArgs.Empty);

            // get the ads from the file or app cache
            _adRecs = GetFileData(AdvertisementFile);

            OnDataBound(EventArgs.Empty);
        }

        protected internal override void PerformDataBinding(IEnumerable data) {
            if (data != null) {
                // We retrieve ad data from xml format in a specific way.
                XmlDataSource xmlDataSource = null;
                object dataSource = DataSource;
                if (dataSource != null) {
                    xmlDataSource = dataSource as XmlDataSource;
                }
                else { // DataSourceID case, we know that only one source is available
                    xmlDataSource = GetDataSource() as XmlDataSource;
                }

                if (xmlDataSource != null) {
                    _adRecs = GetXmlDataSourceData(xmlDataSource);
                }
                else {
                    _adRecs = GetDataSourceData(data);
                }
            }
        }

        protected override void PerformSelect() {
            // VSWhidbey 141362
            CheckOnlyOneDataSource();

            if (AdvertisementFile.Length > 0) {
                PerformAdFileBinding();
            }
            else {
                base.PerformSelect();
            }
        }

        // 
        internal AdCreatedEventArgs PickAd() {
            AdCreatedEventArgs adArgs = SelectedAdArgs;
            if (adArgs == null) {
                adArgs = GetAdCreatedEventArgs();
            }
            adArgs.ImageUrl = ResolveAdRotatorUrl(BaseUrl, adArgs.ImageUrl);
            adArgs.NavigateUrl = ResolveAdRotatorUrl(BaseUrl, adArgs.NavigateUrl);
            return adArgs;
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Displays the <see cref='System.Web.UI.WebControls.AdRotator'/> on the client.</para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            if (!DesignMode && !IsPostCacheAdHelper &&
                DoPostCacheSubstitutionAsNeeded(writer)) {
                return;
            }

            AdCreatedEventArgs adArgs = PickAd();
            RenderLink(writer, adArgs);
        }

        private void RenderLink(HtmlTextWriter writer, AdCreatedEventArgs adArgs) {
            Debug.Assert(writer != null);
            Debug.Assert(adArgs != null);

            HyperLink bannerLink = new HyperLink();

            bannerLink.NavigateUrl = adArgs.NavigateUrl;
            bannerLink.Target = Target;

            if (HasAttributes) {
                foreach(string key in Attributes.Keys) {
                    bannerLink.Attributes[key] = Attributes[key];
                }
            }

            string id = ID;
            if (!String.IsNullOrEmpty(id)) {
                bannerLink.ID = ClientID;
            }

            if (!Enabled) {
                bannerLink.Enabled = false;
            }

            // WebControl's properties use a private flag to determine if a
            // property is set and does not return the value unless the flag is
            // marked.  So here we access those properites (inherited from WebControl)
            // directly from the ViewState bag because if ViewState bag reference
            // was copied to the helper class in the optimized case during the
            // Initialize() method, the flags of the properties wouldn't be set
            // in the helper class.
            string accessKey = (string) ViewState["AccessKey"];
            if (!String.IsNullOrEmpty(accessKey)) {
                bannerLink.AccessKey = accessKey;
            }

            object o = ViewState["TabIndex"];
            if (o != null) {
                short tabIndex = (short) o;
                if (tabIndex != (short) 0) {
                    bannerLink.TabIndex = tabIndex;
                }
            }

            bannerLink.RenderBeginTag(writer);

            // create inner Image
            Image bannerImage = new Image();
            // apply styles to image
            if (ControlStyleCreated) {
                bannerImage.ApplyStyle(ControlStyle);
            }

            string alternateText = adArgs.AlternateText;
            if (!String.IsNullOrEmpty(alternateText)) {
                bannerImage.AlternateText = alternateText;
            }
            else {
                // 25914 Do not render empty 'alt' attribute if <AlternateText> tag is never specified
                IDictionary adProps = adArgs.AdProperties;
                string altTextKey = (AlternateTextField.Length != 0)
                                        ? AlternateTextField : AdCreatedEventArgs.AlternateTextElement;
                string altText = (adProps == null) ? null : (string) adProps[altTextKey];
                if (altText != null && altText.Length == 0) {
                    bannerImage.GenerateEmptyAlternateText = true;
                }
            }

            // Perf work: AdRotator should have resolved the NavigateUrl and
            // ImageUrl when assigning them and have UrlResolved set properly.
            bannerImage.UrlResolved = true;
            string imageUrl = adArgs.ImageUrl;
            if (!String.IsNullOrEmpty(imageUrl)) {
                bannerImage.ImageUrl = imageUrl;
            }

            if (adArgs.HasWidth) {
                bannerImage.ControlStyle.Width = adArgs.Width;
            }

            if (adArgs.HasHeight) {
                bannerImage.ControlStyle.Height = adArgs.Height;
            }

            string toolTip = (string) ViewState["ToolTip"];
            if (!String.IsNullOrEmpty(toolTip)) {
                bannerImage.ToolTip = toolTip;
            }

            bannerImage.RenderControl(writer);
            bannerLink.RenderEndTag(writer);
        }

        private string ResolveAdRotatorUrl(string baseUrl, string relativeUrl) {

            if ((relativeUrl == null) ||
                (relativeUrl.Length == 0) ||
                (UrlPath.IsRelativeUrl(relativeUrl) == false) ||
                (baseUrl == null) ||
                (baseUrl.Length == 0)) {
                return relativeUrl;
            }

            // make it absolute
            return UrlPath.Combine(baseUrl, relativeUrl);
        }

        /// <devdoc>
        ///     <para>Selects an advertisement from the a list of records based
        ///     on different factors.</para>
        /// </devdoc>
        private IDictionary SelectAdFromRecords() {
            if (_adRecs == null || _adRecs.Length == 0) {
                return null;
            }

            string keywordFilter = KeywordFilter;
            bool noKeywordFilter = String.IsNullOrEmpty(keywordFilter);
            if (!noKeywordFilter) {
                // do a lower case comparison
                keywordFilter = keywordFilter.ToLower(CultureInfo.InvariantCulture);
            }

            // sum the matching impressions
            int totalImpressions = 0;
            for (int i = 0; i < _adRecs.Length; i++) {
                if (noKeywordFilter || MatchingAd(_adRecs[i], keywordFilter)) {
                    totalImpressions += _adRecs[i].impressions;
                }
            }

            if (totalImpressions == 0) {
                return null;
            }

            // select one using a random number between 1 and totalImpressions
            int selectedImpression = GetRandomNumber(totalImpressions);
            int impressionCounter = 0;
            int selectedIndex = -1;
            for (int i = 0; i < _adRecs.Length; i++) {
                // Is this the ad?
                if (noKeywordFilter || MatchingAd(_adRecs[i], keywordFilter)) {
                    impressionCounter += _adRecs[i].impressions;
                    if (selectedImpression <= impressionCounter) {
                        selectedIndex = i;
                        break;
                    }
                }
            }
            Debug.Assert(selectedIndex >= 0 && selectedIndex < _adRecs.Length, "Index not found");

            return _adRecs[selectedIndex].adProperties;
        }

        private AdRec [] SetAdRecs(ArrayList adDicts) {
            if (adDicts == null || adDicts.Count == 0) {
                return null;
            }

            // Create an array of AdRec structures from the dictionaries, removing blanks
            AdRec [] adRecs = new AdRec[adDicts.Count];
            int iRec = 0;
            for (int i = 0; i < adDicts.Count; i++) {
                if (adDicts[i] != null) {
                    adRecs[iRec].Initialize((IDictionary) adDicts[i]);
                    iRec++;
                }
            }
            Debug.Assert(iRec == adDicts.Count, "Record count did not match non-null entries");

            return adRecs;
        }



        /// <devdoc>
        ///   Structure to store ads in memory for fast selection by multiple instances of adrotator
        ///   Stores the dictionary and caches some values for easier selection.
        /// </devdoc>
        private struct AdRec {
            public string keyword;
            public int impressions;
            public IDictionary adProperties;


            /// <devdoc>
            ///   Initialize the stuct based on a dictionary containing the advertisement properties
            /// </devdoc>
            public void Initialize(IDictionary adProperties) {

                // Initialize the values we need to keep for ad selection
                Debug.Assert(adProperties != null, "Required here");
                this.adProperties = adProperties;

                // remove null and trim keyword for easier comparisons.
                // VSWhidbey 114634: Be defensive and only retrieve the keyword
                // value if it is in string type
                object keywordValue = adProperties[KeywordProperty];
                if (keywordValue != null && keywordValue is string) {
                    keyword = ((string) keywordValue).Trim();
                }
                else {
                    keyword = string.Empty;
                }

                // get the impressions, but be defensive: let the schema enforce the rules. Default to 1.
                string impressionsString = adProperties[ImpressionsProperty] as string;
                if (String.IsNullOrEmpty(impressionsString) ||
                    !int.TryParse(impressionsString, NumberStyles.Integer,
                                   CultureInfo.InvariantCulture, out impressions)) {
                    impressions = 1;
                }
                if (impressions < 0) {
                    impressions = 1;
                }
            }
        }
    }
}
