//------------------------------------------------------------------------------
// <copyright file="XmlReaderSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System.IO;
using System.Diagnostics;
using System.Security.Permissions;
#if !SILVERLIGHT
using Microsoft.Win32;
using System.Globalization;
using System.Security;
using System.Xml.Schema;
using System.Xml.XmlConfiguration;
#endif
using System.Runtime.Versioning;

namespace System.Xml {

    // XmlReaderSettings class specifies basic features of an XmlReader.
#if !SILVERLIGHT
    [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]
#endif
    public sealed class XmlReaderSettings {

        //
        // Fields
        //

#if ASYNC || FEATURE_NETCORE
        bool useAsync;
#endif


        // Nametable
        XmlNameTable nameTable;

        // XmlResolver
        XmlResolver xmlResolver;

        // Text settings
        int lineNumberOffset;
        int linePositionOffset;

        // Conformance settings
        ConformanceLevel conformanceLevel;
        bool checkCharacters;

        long maxCharactersInDocument;
        long maxCharactersFromEntities;

        // Filtering settings
        bool ignoreWhitespace;
        bool ignorePIs;
        bool ignoreComments;

        // security settings
        DtdProcessing dtdProcessing;

#if !SILVERLIGHT
        //Validation settings
        ValidationType validationType;
        XmlSchemaValidationFlags validationFlags;
        XmlSchemaSet schemas;
        ValidationEventHandler valEventHandler;
#endif

        // other settings
        bool closeInput;

        // read-only flag
        bool isReadOnly;

        //
        // Constructor
        //
        public XmlReaderSettings() {
            Initialize();
        }

#if !FEATURE_LEGACYNETCF
        // introduced for supporting design-time loading of phone assemblies
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
        public XmlReaderSettings(XmlResolver resolver) {
            Initialize(resolver);
        }

        //
        // Properties
        //

#if ASYNC || FEATURE_NETCORE
        public bool Async {
            get {
                return useAsync;
            }
            set {
                CheckReadOnly("Async");
                useAsync = value;
            }
        }
#endif

        // Nametable
        public XmlNameTable NameTable {
            get {
                return nameTable;
            }
            set {
                CheckReadOnly("NameTable");
                nameTable = value;
            }
        }

#if !SILVERLIGHT
        // XmlResolver
        internal bool IsXmlResolverSet {
            get;
            set; // keep set internal as we need to call it from the schema validation code
        }
#endif

        public XmlResolver XmlResolver {
            set {
                CheckReadOnly("XmlResolver");
                xmlResolver = value;
#if !SILVERLIGHT
                IsXmlResolverSet = true;
#endif
            }
        }

        internal XmlResolver GetXmlResolver() {
            return xmlResolver;
        }

#if !SILVERLIGHT
        //This is used by get XmlResolver in Xsd.
        //Check if the config set to prohibit default resovler
        //notice we must keep GetXmlResolver() to avoid dead lock when init System.Config.ConfigurationManager
        internal XmlResolver GetXmlResolver_CheckConfig() { 
            if (System.Xml.XmlConfiguration.XmlReaderSection.ProhibitDefaultUrlResolver && !IsXmlResolverSet)
                return null;
            else
                return xmlResolver;
        }
#endif

        // Text settings
        public int LineNumberOffset {
            get {
                return lineNumberOffset;
            }
            set {
                CheckReadOnly("LineNumberOffset");
                lineNumberOffset = value;
            }
        }

        public int LinePositionOffset {
            get {
                return linePositionOffset;
            }
            set {
                CheckReadOnly("LinePositionOffset");
                linePositionOffset = value;
            }
        }

        // Conformance settings
        public ConformanceLevel ConformanceLevel {
            get {
                return conformanceLevel;
            }
            set {
                CheckReadOnly("ConformanceLevel");

                if ((uint)value > (uint)ConformanceLevel.Document) {
                    throw new ArgumentOutOfRangeException("value");
                }
                conformanceLevel = value;
            }
        }

        public bool CheckCharacters {
            get {
                return checkCharacters;
            }
            set {
                CheckReadOnly("CheckCharacters");
                checkCharacters = value;
            }
        }

        public long MaxCharactersInDocument {
            get {
                return maxCharactersInDocument;
            }
            set {
                CheckReadOnly("MaxCharactersInDocument");
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                maxCharactersInDocument = value;
            }
        }

        public long MaxCharactersFromEntities {
            get {
                return maxCharactersFromEntities;
            }
            set {
                CheckReadOnly("MaxCharactersFromEntities");
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                maxCharactersFromEntities = value;
            }
        }

        // Filtering settings
        public bool IgnoreWhitespace {
            get {
                return ignoreWhitespace;
            }
            set {
                CheckReadOnly("IgnoreWhitespace");
                ignoreWhitespace = value;
            }
        }

        public bool IgnoreProcessingInstructions {
            get {
                return ignorePIs;
            }
            set {
                CheckReadOnly("IgnoreProcessingInstructions");
                ignorePIs = value;
            }
        }

        public bool IgnoreComments {
            get {
                return ignoreComments;
            }
            set {
                CheckReadOnly("IgnoreComments");
                ignoreComments = value;
            }
        }

#if !SILVERLIGHT
        [Obsolete("Use XmlReaderSettings.DtdProcessing property instead.")]
        public bool ProhibitDtd {
            get {
                return dtdProcessing == DtdProcessing.Prohibit;
            }
            set {
                CheckReadOnly("ProhibitDtd");
                dtdProcessing = value ? DtdProcessing.Prohibit : DtdProcessing.Parse;
            }
        }
#endif

        public DtdProcessing DtdProcessing {
            get {
                return dtdProcessing;
            }
            set {
                CheckReadOnly("DtdProcessing");

                if ((uint)value > (uint)DtdProcessing.Parse) {
                    throw new ArgumentOutOfRangeException("value");
                }
                dtdProcessing = value;
            }
        }

        public bool CloseInput {
            get {
                return closeInput;
            }
            set {
                CheckReadOnly("CloseInput");
                closeInput = value;
            }
        }

#if !SILVERLIGHT
        public ValidationType ValidationType {
            get {
                return validationType;
            }
            set {
                CheckReadOnly("ValidationType");

                if ((uint)value > (uint)ValidationType.Schema) {
                    throw new ArgumentOutOfRangeException("value");
                }
                validationType = value;
            }
        }

        public XmlSchemaValidationFlags ValidationFlags {
            get {
                return validationFlags;
            }
            set {
                CheckReadOnly("ValidationFlags");

                if ((uint)value > (uint)(XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ProcessSchemaLocation |
                                           XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints |
                                           XmlSchemaValidationFlags.AllowXmlAttributes)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                validationFlags = value;
            }
        }

        public XmlSchemaSet Schemas {
            get {
                if (schemas == null) {
                    schemas = new XmlSchemaSet();
                }
                return schemas;
            }
            set {
                CheckReadOnly("Schemas");
                schemas = value;
            }
        }

        public event ValidationEventHandler ValidationEventHandler {
            add {
                CheckReadOnly("ValidationEventHandler");
                valEventHandler += value;
            }
            remove {
                CheckReadOnly("ValidationEventHandler");
                valEventHandler -= value;
            }
        }
#endif

        //
        // Public methods
        //
        public void Reset() {
            CheckReadOnly("Reset");
            Initialize();
        }

        public XmlReaderSettings Clone() {
            XmlReaderSettings clonedSettings = this.MemberwiseClone() as XmlReaderSettings;
            clonedSettings.ReadOnly = false;
            return clonedSettings;
        }

        //
        // Internal methods
        //
#if !SILVERLIGHT
        internal ValidationEventHandler GetEventHandler() {
            return valEventHandler;
        }
#endif


#if !SILVERLIGHT
        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
#endif
        internal XmlReader CreateReader(String inputUri, XmlParserContext inputContext) {
            if (inputUri == null) {
                throw new ArgumentNullException("inputUri");
            }
            if (inputUri.Length == 0) {
                throw new ArgumentException(Res.GetString(Res.XmlConvert_BadUri), "inputUri");
            }

            // resolve and open the url
            XmlResolver tmpResolver = this.GetXmlResolver();
            if (tmpResolver == null) {
                tmpResolver = CreateDefaultResolver();
            }

            // create text XML reader
            XmlReader reader = new XmlTextReaderImpl(inputUri, this, inputContext, tmpResolver);

#if !SILVERLIGHT
            // wrap with validating reader
            if (this.ValidationType != ValidationType.None) {
                reader = AddValidation(reader);
            }
#endif

#if ASYNC
            if (useAsync) {
                reader = XmlAsyncCheckReader.CreateAsyncCheckWrapper(reader);
            }
#endif
            return reader;
        }

        internal XmlReader CreateReader(Stream input, Uri baseUri, string baseUriString, XmlParserContext inputContext) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }
            if (baseUriString == null) {
                if (baseUri == null) {
                    baseUriString = string.Empty;
                }
                else {
                    baseUriString = baseUri.ToString();
                }
            }

            // create text XML reader
            XmlReader reader = new XmlTextReaderImpl(input, null, 0, this, baseUri, baseUriString, inputContext, closeInput);

#if !SILVERLIGHT
            // wrap with validating reader
            if (this.ValidationType != ValidationType.None) {
                reader = AddValidation(reader);
            }
#endif

#if ASYNC
            if (useAsync) {
                reader = XmlAsyncCheckReader.CreateAsyncCheckWrapper(reader);
            }
#endif

            return reader;
        }

        internal XmlReader CreateReader(TextReader input, string baseUriString, XmlParserContext inputContext) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }
            if (baseUriString == null) {
                baseUriString = string.Empty;
            }

            // create xml text reader
            XmlReader reader = new XmlTextReaderImpl(input, this, baseUriString, inputContext);

#if !SILVERLIGHT
            // wrap with validating reader
            if (this.ValidationType != ValidationType.None) {
                reader = AddValidation(reader);
            }
#endif

#if ASYNC
            if (useAsync) {
                reader = XmlAsyncCheckReader.CreateAsyncCheckWrapper(reader);
            }
#endif

            return reader;
        }

        internal XmlReader CreateReader(XmlReader reader) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }
#if !ASYNC || SILVERLIGHT || FEATURE_NETCORE
            // wrap with conformance layer (if needed)
            return AddConformanceWrapper(reader);
#else
            return AddValidationAndConformanceWrapper(reader);
#endif // !ASYNC || SILVERLIGHT || FEATURE_NETCORE
        }

        internal bool ReadOnly {
            get {
                return isReadOnly;
            }
            set {
                isReadOnly = value;
            }
        }

        void CheckReadOnly(string propertyName) {
            if (isReadOnly) {
                throw new XmlException(Res.Xml_ReadOnlyProperty, this.GetType().Name + '.' + propertyName);
            }
        }

        //
        // Private methods
        //
        void Initialize() {
            Initialize(null);
        }

        void Initialize(XmlResolver resolver) {
            nameTable = null;
#if !SILVERLIGHT
            if (!EnableLegacyXmlSettings())
            {
                xmlResolver = resolver;
                // limit the entity resolving to 10 million character. the caller can still
                // override it to any other value or set it to zero for unlimiting it
                maxCharactersFromEntities = (long) 1e7;
            }
            else
#endif            
            {
                xmlResolver = (resolver == null ? CreateDefaultResolver() : resolver);
                maxCharactersFromEntities = 0;
            }
            lineNumberOffset = 0;
            linePositionOffset = 0;
            checkCharacters = true;
            conformanceLevel = ConformanceLevel.Document;

            ignoreWhitespace = false;
            ignorePIs = false;
            ignoreComments = false;
            dtdProcessing = DtdProcessing.Prohibit;
            closeInput = false;

            maxCharactersInDocument = 0;

#if !SILVERLIGHT
            schemas = null;
            validationType = ValidationType.None;
            validationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints;
            validationFlags |= XmlSchemaValidationFlags.AllowXmlAttributes;
#endif

#if ASYNC || FEATURE_NETCORE
            useAsync = false;
#endif

            isReadOnly = false;
#if !SILVERLIGHT
            IsXmlResolverSet = false;
#endif
        }

        static XmlResolver CreateDefaultResolver() {
#if SILVERLIGHT
            return new XmlXapResolver();
#else
            return new XmlUrlResolver();
#endif
        }

#if !SILVERLIGHT
        internal XmlReader AddValidation(XmlReader reader) {
            if (this.validationType == ValidationType.Schema) {
                XmlResolver resolver = GetXmlResolver_CheckConfig();

                if (resolver == null &&
                    !this.IsXmlResolverSet &&
                    !EnableLegacyXmlSettings())
                {
                    resolver = new XmlUrlResolver();
                }
                reader = new XsdValidatingReader(reader, resolver, this);
            }
            else if (this.validationType == ValidationType.DTD) {
                reader = CreateDtdValidatingReader(reader);
            }
            return reader;
        }

        private XmlReader AddValidationAndConformanceWrapper(XmlReader reader) {
            // wrap with DTD validating reader
            if (this.validationType == ValidationType.DTD) {
                reader = CreateDtdValidatingReader(reader);
            }
            // add conformance checking (must go after DTD validation because XmlValidatingReader works only on XmlTextReader),
            // but before XSD validation because of typed value access
            reader = AddConformanceWrapper(reader);

            if (this.validationType == ValidationType.Schema) {
                reader = new XsdValidatingReader(reader, GetXmlResolver_CheckConfig(), this);
            }
            return reader;
        }

        private XmlValidatingReaderImpl CreateDtdValidatingReader(XmlReader baseReader) {
            return new XmlValidatingReaderImpl(baseReader, this.GetEventHandler(), (this.ValidationFlags & XmlSchemaValidationFlags.ProcessIdentityConstraints) != 0);
        }
#endif // !SILVERLIGHT

        internal XmlReader AddConformanceWrapper(XmlReader baseReader) {
            XmlReaderSettings baseReaderSettings = baseReader.Settings;
            bool checkChars = false;
            bool noWhitespace = false;
            bool noComments = false;
            bool noPIs = false;
            DtdProcessing dtdProc = (DtdProcessing)(-1);
            bool needWrap = false;

            if (baseReaderSettings == null) {

#pragma warning disable 618

#if SILVERLIGHT
                if (this.conformanceLevel != ConformanceLevel.Auto) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_IncompatibleConformanceLevel, this.conformanceLevel.ToString()));
                }
#else
                if (this.conformanceLevel != ConformanceLevel.Auto && this.conformanceLevel != XmlReader.GetV1ConformanceLevel(baseReader)) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_IncompatibleConformanceLevel, this.conformanceLevel.ToString()));
                }
#endif

#if !SILVERLIGHT
                // get the V1 XmlTextReader ref
                XmlTextReader v1XmlTextReader = baseReader as XmlTextReader;
                if (v1XmlTextReader == null) {
                    XmlValidatingReader vr = baseReader as XmlValidatingReader;
                    if (vr != null) {
                        v1XmlTextReader = (XmlTextReader)vr.Reader;
                    }
                }
#endif

                // assume the V1 readers already do all conformance checking; 
                // wrap only if IgnoreWhitespace, IgnoreComments, IgnoreProcessingInstructions or ProhibitDtd is true;
                if (this.ignoreWhitespace) {
                    WhitespaceHandling wh = WhitespaceHandling.All;
#if !SILVERLIGHT
                    // special-case our V1 readers to see if whey already filter whitespaces
                    if (v1XmlTextReader != null) {
                        wh = v1XmlTextReader.WhitespaceHandling;
                    }
#endif
                    if (wh == WhitespaceHandling.All) {
                        noWhitespace = true;
                        needWrap = true;
                    }
                }
                if (this.ignoreComments) {
                    noComments = true;
                    needWrap = true;
                }
                if (this.ignorePIs) {
                    noPIs = true;
                    needWrap = true;
                }
                // DTD processing
                DtdProcessing baseDtdProcessing = DtdProcessing.Parse;
#if !SILVERLIGHT
                if (v1XmlTextReader != null) {
                    baseDtdProcessing = v1XmlTextReader.DtdProcessing;
                }
#endif
                if ((this.dtdProcessing == DtdProcessing.Prohibit && baseDtdProcessing != DtdProcessing.Prohibit) ||
                    (this.dtdProcessing == DtdProcessing.Ignore && baseDtdProcessing == DtdProcessing.Parse)) {
                    dtdProc = this.dtdProcessing;
                    needWrap = true;
                }
#pragma warning restore 618
            }
            else {
                if (this.conformanceLevel != baseReaderSettings.ConformanceLevel && this.conformanceLevel != ConformanceLevel.Auto) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_IncompatibleConformanceLevel, this.conformanceLevel.ToString()));
                }
                if (this.checkCharacters && !baseReaderSettings.CheckCharacters) {
                    checkChars = true;
                    needWrap = true;
                }
                if (this.ignoreWhitespace && !baseReaderSettings.IgnoreWhitespace) {
                    noWhitespace = true;
                    needWrap = true;
                }
                if (this.ignoreComments && !baseReaderSettings.IgnoreComments) {
                    noComments = true;
                    needWrap = true;
                }
                if (this.ignorePIs && !baseReaderSettings.IgnoreProcessingInstructions) {
                    noPIs = true;
                    needWrap = true;
                }

                if ((this.dtdProcessing == DtdProcessing.Prohibit && baseReaderSettings.DtdProcessing != DtdProcessing.Prohibit) ||
                    (this.dtdProcessing == DtdProcessing.Ignore && baseReaderSettings.DtdProcessing == DtdProcessing.Parse)) {
                    dtdProc = this.dtdProcessing;
                    needWrap = true;
                }
            }

            if (needWrap) {
                IXmlNamespaceResolver readerAsNSResolver = baseReader as IXmlNamespaceResolver;
                if (readerAsNSResolver != null) {
                    return new XmlCharCheckingReaderWithNS(baseReader, readerAsNSResolver, checkChars, noWhitespace, noComments, noPIs, dtdProc);
                }
                else {
                    return new XmlCharCheckingReader(baseReader, checkChars, noWhitespace, noComments, noPIs, dtdProc);
                }
            }
            else {
                return baseReader;
            }
        }

#if !SILVERLIGHT
        private static bool? s_enableLegacyXmlSettings = null;

        static internal bool EnableLegacyXmlSettings()
        {
            if (s_enableLegacyXmlSettings.HasValue)
            {
                return s_enableLegacyXmlSettings.Value;
            }

            if (!System.Xml.BinaryCompatibility.TargetsAtLeast_Desktop_V4_5_2)
            {
                s_enableLegacyXmlSettings = true;
                return s_enableLegacyXmlSettings.Value;
            }

            bool enableSettings = false; // default value
            if (!ReadSettingsFromRegistry(Registry.LocalMachine, ref enableSettings))
            {
                // still ok if this call return false too as we'll use the default value which is false
                ReadSettingsFromRegistry(Registry.CurrentUser, ref enableSettings);
            }

            s_enableLegacyXmlSettings = enableSettings;
            return s_enableLegacyXmlSettings.Value;
        }

        [RegistryPermission(SecurityAction.Assert, Unrestricted = true)]
        [SecuritySafeCritical]
        private static bool ReadSettingsFromRegistry(RegistryKey hive, ref bool value)
        {
            const string regValueName = "EnableLegacyXmlSettings";
            const string regValuePath = @"SOFTWARE\Microsoft\.NETFramework\XML";

            try
            {                                                                     
                using (RegistryKey xmlRegKey = hive.OpenSubKey(regValuePath, false))
                {
                    if (xmlRegKey != null)
                    {
                        if (xmlRegKey.GetValueKind(regValueName) == RegistryValueKind.DWord)
                        {
                            value = ((int)xmlRegKey.GetValue(regValueName)) == 1;
                            return true;
                        }
                    }
                }
            }
            catch { /* use the default if we couldn't read the key */ }

            return false;
        }

#endif // SILVERLIGHT
        
    }
}
