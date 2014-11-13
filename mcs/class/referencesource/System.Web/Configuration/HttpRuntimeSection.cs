//------------------------------------------------------------------------------
// <copyright file="HttpRuntimeSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Web.Security.AntiXss;
    using System.Web.Util;

    public sealed class HttpRuntimeSection : ConfigurationSection {
#if !FEATURE_PAL // FEATURE_PAL-specific timeout values
        internal const int DefaultExecutionTimeout = 110;
#else // !FEATURE_PAL
        // The timeout needs to be extended, since Coriolis/Rotor is much slower
        // especially platforms like Solaris wan't be able to process complicated
        // requests if in debug mode.
        // Remove or change this once the real timeout is known
#if DEBUG
        internal const int DefaultExecutionTimeout = 110 * 10;
#else // DEBUG
        internal const int DefaultExecutionTimeout = 110 * 5;
#endif // DEBUG

#endif // !FEATURE_PAL
        internal const int DefaultMaxRequestLength = 4096 * 1024;  // 4MB
        internal const int DefaultRequestLengthDiskThreshold = 80 * 1024; // 80KB
        internal const int DefaultMinFreeThreads = 8;
        internal const int DefaultMinLocalRequestFreeThreads = 4;
        internal const int DefaultAppRequestQueueLimit = 100;
        internal const int DefaultShutdownTimeout = 90;
        internal const int DefaultDelayNotificationTimeout = 0;
        internal const int DefaultWaitChangeNotification = 0;
        internal const int DefaultMaxWaitChangeNotification = 0;
        internal const bool DefaultAllowDynamicModuleRegistration = true;
        internal const bool DefaultEnableKernelOutputCache = true;
        internal const bool DefaultRequireRootedSaveAsPath = true;
        internal const bool DefaultSendCacheControlHeader = true;
        internal const string DefaultEncoderType = "System.Web.Util.HttpEncoder";
        internal static readonly Version DefaultRequestValidationMode = VersionUtil.FrameworkDefault;
        internal const string DefaultRequestValidationModeString = VersionUtil.FrameworkDefaultString;
        internal const string DefaultRequestValidationType = "System.Web.Util.RequestValidator";
        internal const string DefaultRequestPathInvalidCharacters = "<,>,*,%,&,:,\\,?";
        internal const int DefaultMaxUrlLength = 260;
        internal const int DefaultMaxQueryStringLength = 2048;
        internal const bool DefaultRelaxedUrlToFileSystemMapping = false;
        internal const string DefaultTargetFramework = null;

        private AsyncPreloadModeFlags asyncPreloadModeCache;
        private bool asyncPreloadModeCached = false;

        private bool enableVersionHeaderCache = true;
        private bool enableVersionHeaderCached = false;
        private TimeSpan executionTimeoutCache;
        private bool executionTimeoutCached = false;

        private bool sendCacheControlHeaderCached = false;
        private bool sendCacheControlHeaderCache;

        private FcnMode fcnModeCache;
        private bool fcnModeCached = false;

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propAsyncPreloadMode =
            new ConfigurationProperty("asyncPreloadMode",
                                        typeof(AsyncPreloadModeFlags),
                                        AsyncPreloadModeFlags.None,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propExecutionTimeout =
            new ConfigurationProperty("executionTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromSeconds((double)DefaultExecutionTimeout),
                                        StdValidatorsAndConverters.TimeSpanSecondsConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxRequestLength =
            new ConfigurationProperty("maxRequestLength",
                                        typeof(int),
                                        4096,
                                        null,
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestLengthDiskThreshold =
            new ConfigurationProperty("requestLengthDiskThreshold",
                                        typeof(int),
                                        80,
                                        null,
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUseFullyQualifiedRedirectUrl =
            new ConfigurationProperty("useFullyQualifiedRedirectUrl",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinFreeThreads =
            new ConfigurationProperty("minFreeThreads",
                                        typeof(int),
                                        8,
                                        null,
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinLocalRequestFreeThreads =
            new ConfigurationProperty("minLocalRequestFreeThreads",
                                        typeof(int),
                                        4,
                                        null,
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propAppRequestQueueLimit =
            new ConfigurationProperty("appRequestQueueLimit",
                                        typeof(int),
                                        5000,
                                        null,
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableKernelOutputCache =
            new ConfigurationProperty("enableKernelOutputCache",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableVersionHeader =
            new ConfigurationProperty("enableVersionHeader",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequireRootedSaveAsPath =
            new ConfigurationProperty("requireRootedSaveAsPath",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnable =
            new ConfigurationProperty("enable",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDefaultRegexMatchTimeout =
            new ConfigurationProperty("defaultRegexMatchTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.Zero,
                                        null,
                                        StdValidatorsAndConverters.RegexMatchTimeoutValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propShutdownTimeout =
            new ConfigurationProperty("shutdownTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromSeconds((double)DefaultShutdownTimeout),
                                        StdValidatorsAndConverters.TimeSpanSecondsConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDelayNotificationTimeout =
            new ConfigurationProperty("delayNotificationTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromSeconds((double)DefaultDelayNotificationTimeout),
                                        StdValidatorsAndConverters.TimeSpanSecondsConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propWaitChangeNotification =
            new ConfigurationProperty("waitChangeNotification",
                                        typeof(int),
                                        0,
                                        null,
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxWaitChangeNotification =
            new ConfigurationProperty("maxWaitChangeNotification",
                                        typeof(int),
                                        0,
                                        null,
                                        StdValidatorsAndConverters.PositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnableHeaderChecking =
            new ConfigurationProperty("enableHeaderChecking",
                                        typeof(bool),
                                        true,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propSendCacheControlHeader =
            new ConfigurationProperty("sendCacheControlHeader",
                                        typeof(bool),
                                        DefaultSendCacheControlHeader,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propApartmentThreading =
            new ConfigurationProperty("apartmentThreading",
                                        typeof(bool),
                                        false,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEncoderType =
            new ConfigurationProperty("encoderType",
                                        typeof(string),
                                        DefaultEncoderType,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestValidationMode =
            new ConfigurationProperty("requestValidationMode",
                                        typeof(Version),
                                        DefaultRequestValidationMode,
                                        StdValidatorsAndConverters.VersionConverter,
                                        null,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestValidationType =
            new ConfigurationProperty("requestValidationType",
                                        typeof(string),
                                        DefaultRequestValidationType,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propRequestPathInvalidCharacters =
            new ConfigurationProperty("requestPathInvalidCharacters",
                                      typeof(string),
                                      DefaultRequestPathInvalidCharacters,
                                      StdValidatorsAndConverters.WhiteSpaceTrimStringConverter,
                                      null,
                                      ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propMaxUrlLength =
            new ConfigurationProperty("maxUrlLength",
                                        typeof(int),
                                        DefaultMaxUrlLength,
                                        null,
                                        new IntegerValidator(0, 2097151), // Max from VS 330766
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propMaxQueryStringLength =
            new ConfigurationProperty("maxQueryStringLength",
                                        typeof(int),
                                        DefaultMaxQueryStringLength,
                                        null,
                                        new IntegerValidator(0, 2097151), // Max from VS 330766
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propRelaxedUrlToFileSystemMapping =
            new ConfigurationProperty("relaxedUrlToFileSystemMapping",
                                      typeof(bool),
                                      DefaultRelaxedUrlToFileSystemMapping,
                                      ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propAllowDynamicModuleRegistration =
            new ConfigurationProperty("allowDynamicModuleRegistration",
                                      typeof(bool),
                                      DefaultAllowDynamicModuleRegistration,
                                      ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propFcnMode =
            new ConfigurationProperty("fcnMode",
                                        typeof(FcnMode),
                                        FcnMode.NotSet,
                                        ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propTargetFramework =
            new ConfigurationProperty("targetFramework",
                                        typeof(string),
                                        DefaultTargetFramework,
                                        ConfigurationPropertyOptions.None);


        private int _MaxRequestLengthBytes;
        private int _RequestLengthDiskThresholdBytes;
        private static String s_versionHeader = null;
        private Version _requestValidationMode;

        /*         <!--
        httpRuntime Attributes:
          asyncPreloadMode="None" [None | Form | FormMultiPart | AllFormTypes | NonForm | All]
          executionTimeout="[seconds]" - time in seconds before request is automatically timed out
          maxRequestLength="[KBytes]" - KBytes size of maximum request length to accept
          requestLengthDiskThreshold="[KBytes]" - KBytes threshold to use disk for posted content temporary storage
          useFullyQualifiedRedirectUrl="[true|false]" - Fully qualifiy the URL for client redirects
          minFreeThreads="[count]" - minimum number of free thread to allow execution of new requests
          minLocalRequestFreeThreads="[count]" - minimum number of free thread to allow execution of new local requests
          appRequestQueueLimit="[count]" - maximum number of requests queued for the application; the sum of requests in all application queues is bounded from above by the global requestQueueLimit in the processModel section
          enableKernelOutputCache="[true|false]" - enable the http.sys cache on IIS6 and higher - default is true
          enableVersionHeader="[true|false]" - outputs X-AspNet-Version header with each request
          requireRootedSaveAsPath="[true|false]" - the filename argument to SaveAs methods must be a rooted path
          enable="[true|false]" - enable processing requests for this application
          waitChangeNotification="[seconds]" - time in seconds to wait for another file change notification before restarting the AppDomain
          maxWaitChangeNotification="[seconds]" - maximum time in seconds to wait from the first file change notification before restarting the AppDomain
          enableHeaderChecking="[true|false]" - when true, CRLF pairs in response headers are encoded
          encoderType="[typename]" - type used for custom HTTP encoding (HTML encoding, URL encoding, etc.)
          requestValidationMode="[version]" - "2.0" to turn request validation on only for pages, "4.0" for the entire pipeline, "4.5" to enable granular request validation
          requestValidationType="[typename]" - type used for custom request validation
          RequestPathInvalidCharacters="[string]" - comma seperated list of chars in the URL to reject
          maxUrlLength="[int]" - Max length of Request.Path
          maxQueryStringLength="[int]"  - Max length of QueryString
          relaxedUrlToFileSystemMapping = "false"
          allowDynamicModuleRegistration="[true|flase]" - used to block RegisterModule method calls
          targetFramework="[string]" - framework version behavior to trigger CLR "quirks mode" settings
        -->
        <httpRuntime
            asyncPreloadMode="None"
            executionTimeout="110"
            maxRequestLength="4096"
            requestLengthDiskThreshold="80"
            useFullyQualifiedRedirectUrl="false"
            minFreeThreads="8"
            minLocalRequestFreeThreads="4"
            appRequestQueueLimit="5000"
            enableVersionHeader="true"
            requireRootedSaveAsPath="true"
            enable="true"
            encoderType="System.Web.Util.HttpEncoder"
            requestValidationMode="4.0"
            requestValidationType="System.Web.Util.RequestValidator"
            RequestPathInvalidCharacters="<,>,*,%,&,:,\,?"
            maxUrlLength="260"
            maxQueryStringLength="4096"
            relaxedUrlToFileSystemMapping = "false"
            allowDynamicModuleRegistration = "true"
/>
*/

        static HttpRuntimeSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propAsyncPreloadMode);
            _properties.Add(_propExecutionTimeout);
            _properties.Add(_propMaxRequestLength);
            _properties.Add(_propRequestLengthDiskThreshold);
            _properties.Add(_propUseFullyQualifiedRedirectUrl);
            _properties.Add(_propMinFreeThreads);
            _properties.Add(_propMinLocalRequestFreeThreads);
            _properties.Add(_propAppRequestQueueLimit);
            _properties.Add(_propEnableKernelOutputCache);
            _properties.Add(_propEnableVersionHeader);
            _properties.Add(_propRequireRootedSaveAsPath);
            _properties.Add(_propEnable);

            _properties.Add(_propDefaultRegexMatchTimeout);
            _properties.Add(_propShutdownTimeout);
            _properties.Add(_propDelayNotificationTimeout);
            _properties.Add(_propWaitChangeNotification);
            _properties.Add(_propMaxWaitChangeNotification);

            _properties.Add(_propEnableHeaderChecking);
            _properties.Add(_propSendCacheControlHeader);
            _properties.Add(_propApartmentThreading);

            _properties.Add(_propEncoderType);
            _properties.Add(_propRequestValidationMode);
            _properties.Add(_propRequestValidationType);
            _properties.Add(_propRequestPathInvalidCharacters);
            _properties.Add(_propMaxUrlLength);
            _properties.Add(_propMaxQueryStringLength);
            _properties.Add(_propRelaxedUrlToFileSystemMapping);
            _properties.Add(_propAllowDynamicModuleRegistration);
            _properties.Add(_propFcnMode);
            _properties.Add(_propTargetFramework);
        }

        public HttpRuntimeSection() {
            _MaxRequestLengthBytes = -1;
            _RequestLengthDiskThresholdBytes = -1;
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("asyncPreloadMode", DefaultValue = AsyncPreloadModeFlags.None)]
        public AsyncPreloadModeFlags AsyncPreloadMode {
            get {
                if (asyncPreloadModeCached == false) {
                    asyncPreloadModeCache = (AsyncPreloadModeFlags)base[_propAsyncPreloadMode];
                    asyncPreloadModeCached = true;
                }
                return asyncPreloadModeCache;
            }
            set {
                base[_propAsyncPreloadMode] = value;
                asyncPreloadModeCache = value;
            }
        }

        [ConfigurationProperty("fcnMode", DefaultValue = FcnMode.NotSet)]
        public FcnMode FcnMode {
            get {
                if (!fcnModeCached) {
                    fcnModeCache = (FcnMode)base[_propFcnMode];
                    fcnModeCached = true;
                }
                return fcnModeCache;
            }
            set {
                base[_propFcnMode] = value;
                fcnModeCache = value;
            }
        }

        [ConfigurationProperty("executionTimeout", DefaultValue = "00:01:50")]
        [TypeConverter(typeof(TimeSpanSecondsConverter))]
        [TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        public TimeSpan ExecutionTimeout {
            get {
                if (executionTimeoutCached == false) {
                    executionTimeoutCache = (TimeSpan)base[_propExecutionTimeout];
                    executionTimeoutCached = true;
                }
                return executionTimeoutCache;
            }
            set {
                base[_propExecutionTimeout] = value;
                executionTimeoutCache = value;
            }


        }

        [ConfigurationProperty("maxRequestLength", DefaultValue = 4096)]
        [IntegerValidator(MinValue = 0)]
        public int MaxRequestLength {
            get {
                return (int)base[_propMaxRequestLength];
            }
            set {
                if (value < RequestLengthDiskThreshold) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_max_request_length_smaller_than_max_request_length_disk_threshold),
                         ElementInformation.Properties[_propMaxRequestLength.Name].Source,
                         ElementInformation.Properties[_propMaxRequestLength.Name].LineNumber);
                }
                base[_propMaxRequestLength] = value;
            } //
        }

        [ConfigurationProperty("requestLengthDiskThreshold", DefaultValue = 80)]
        [IntegerValidator(MinValue = 1)]
        public int RequestLengthDiskThreshold {
            get {
                return (int)base[_propRequestLengthDiskThreshold];
            }
            set {
                if (value > MaxRequestLength) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_max_request_length_disk_threshold_exceeds_max_request_length),
                         ElementInformation.Properties[_propRequestLengthDiskThreshold.Name].Source,
                         ElementInformation.Properties[_propRequestLengthDiskThreshold.Name].LineNumber);
                }
                base[_propRequestLengthDiskThreshold] = value;
            }
        }

        [ConfigurationProperty("useFullyQualifiedRedirectUrl", DefaultValue = false)]
        public bool UseFullyQualifiedRedirectUrl {
            get {
                return (bool)base[_propUseFullyQualifiedRedirectUrl];
            }
            set {
                base[_propUseFullyQualifiedRedirectUrl] = value;
            }
        }

        [ConfigurationProperty("minFreeThreads", DefaultValue = 8)]
        [IntegerValidator(MinValue = 0)]
        public int MinFreeThreads {
            get {
                return (int)base[_propMinFreeThreads];
            }
            set {
                base[_propMinFreeThreads] = value;
            }
        }

        [ConfigurationProperty("minLocalRequestFreeThreads", DefaultValue = 4)]
        [IntegerValidator(MinValue = 0)]
        public int MinLocalRequestFreeThreads {
            get {
                return (int)base[_propMinLocalRequestFreeThreads];
            }
            set {
                base[_propMinLocalRequestFreeThreads] = value;
            }
        }

        [ConfigurationProperty("appRequestQueueLimit", DefaultValue = 5000)]
        [IntegerValidator(MinValue = 1)]
        public int AppRequestQueueLimit {
            get {
                return (int)base[_propAppRequestQueueLimit];
            }
            set {
                base[_propAppRequestQueueLimit] = value;
            }
        }

        [ConfigurationProperty("enableKernelOutputCache", DefaultValue = true)]
        public bool EnableKernelOutputCache {
            get {
                return (bool)base[_propEnableKernelOutputCache];
            }
            set {
                base[_propEnableKernelOutputCache] = value;
            }
        }

        [ConfigurationProperty("enableVersionHeader", DefaultValue = true)]
        public bool EnableVersionHeader {
            get {
                if (enableVersionHeaderCached == false) {
                    enableVersionHeaderCache = (bool)base[_propEnableVersionHeader];
                    enableVersionHeaderCached = true;
                }
                return enableVersionHeaderCache;
            }
            set {
                base[_propEnableVersionHeader] = value;
                enableVersionHeaderCache = value;
            }
        }

        [ConfigurationProperty("apartmentThreading", DefaultValue = false)]
        public bool ApartmentThreading {
            get {
                return (bool)base[_propApartmentThreading];
            }
            set {
                base[_propApartmentThreading] = value;
            }
        }

        [ConfigurationProperty("requireRootedSaveAsPath", DefaultValue = true)]
        public bool RequireRootedSaveAsPath {
            get {
                return (bool)base[_propRequireRootedSaveAsPath];
            }
            set {
                base[_propRequireRootedSaveAsPath] = value;
            }
        }

        [ConfigurationProperty("enable", DefaultValue = true)]
        public bool Enable {
            get {
                return (bool)base[_propEnable];
            }
            set {
                base[_propEnable] = value;
            }
        }

        [ConfigurationProperty("targetFramework", DefaultValue = DefaultTargetFramework)]
        public string TargetFramework {
            get {
                return (string)base[_propTargetFramework];
            }
            set {
                base[_propTargetFramework] = value;
            }
        }

        // method is used to provide a FrameworkName to be used for the AppDomainSetup.TargetFrameworkName property
        internal FrameworkName GetTargetFrameworkName() {
            string targetFramework = TargetFramework; // only read from property once so the value can't change in the middle of processing
            if (String.IsNullOrEmpty(targetFramework)) {
                // no target framework; let caller perform default behavior
                return null;
            }
            else {
                Version version;
                if (!Version.TryParse(targetFramework, out version)) {
                    // if this doesn't parse as a valid Version object, throw an exception containing the erroneous line in config
                    PropertyInformation targetFrameworkPropInfo = ElementInformation.Properties["targetFramework"];
                    throw new ConfigurationErrorsException(SR.GetString(SR.HttpRuntimeSection_TargetFramework_Invalid),
                        filename: targetFrameworkPropInfo.Source,
                        line: targetFrameworkPropInfo.LineNumber);
                }

                // check succeeded
                return new FrameworkName(".NETFramework", version);
            }
        }

        [ConfigurationProperty("sendCacheControlHeader", DefaultValue = DefaultSendCacheControlHeader)]
        public bool SendCacheControlHeader {
            get {
                if (sendCacheControlHeaderCached == false) {
                    sendCacheControlHeaderCache = (bool)base[_propSendCacheControlHeader];
                    sendCacheControlHeaderCached = true;
                }
                return sendCacheControlHeaderCache;
            }
            set {
                base[_propSendCacheControlHeader] = value;
                sendCacheControlHeaderCache = value;
            }
        }

        [ConfigurationProperty("defaultRegexMatchTimeout", DefaultValue = "00:00:00")]
        [RegexMatchTimeoutValidator]
        public TimeSpan DefaultRegexMatchTimeout {
            get {
                return (TimeSpan)base[_propDefaultRegexMatchTimeout];
            }
            set {
                base[_propDefaultRegexMatchTimeout] = value;
            }
        }

        [ConfigurationProperty("shutdownTimeout", DefaultValue = "00:01:30")]
        [TypeConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan ShutdownTimeout {
            get {
                return (TimeSpan)base[_propShutdownTimeout];
            }
            set {
                base[_propShutdownTimeout] = value;
            }
        }

        [ConfigurationProperty("delayNotificationTimeout", DefaultValue = "00:00:00")]
        [TypeConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan DelayNotificationTimeout {
            get {
                return (TimeSpan)base[_propDelayNotificationTimeout];
            }
            set {
                base[_propDelayNotificationTimeout] = value;
            }
        }

        [ConfigurationProperty("waitChangeNotification", DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int WaitChangeNotification {
            get {
                return (int)base[_propWaitChangeNotification];
            }
            set {
                base[_propWaitChangeNotification] = value;
            }
        }

        [ConfigurationProperty("maxWaitChangeNotification", DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int MaxWaitChangeNotification {
            get {
                return (int)base[_propMaxWaitChangeNotification];
            }
            set {
                base[_propMaxWaitChangeNotification] = value;
            }
        }

        [ConfigurationProperty("enableHeaderChecking", DefaultValue = true)]
        public bool EnableHeaderChecking {
            get {
                return (bool)base[_propEnableHeaderChecking];
            }
            set {
                base[_propEnableHeaderChecking] = value;
            }
        }

        [ConfigurationProperty("encoderType", DefaultValue = DefaultEncoderType)]
        [StringValidator(MinLength = 1)]
        public string EncoderType {
            get {
                return (string)base[_propEncoderType];
            }
            set {
                base[_propEncoderType] = value;
            }
        }

        [ConfigurationProperty("requestValidationMode", DefaultValue = DefaultRequestValidationModeString)]
        [TypeConverter(typeof(VersionConverter))]
        public Version RequestValidationMode {
            get {
                if (_requestValidationMode == null) {
                    _requestValidationMode = (Version)base[_propRequestValidationMode];
                }
                return _requestValidationMode;
            }
            set {
                _requestValidationMode = value;
                base[_propRequestValidationMode] = value;
            }
        }

        [ConfigurationProperty("requestValidationType", DefaultValue = DefaultRequestValidationType)]
        [StringValidator(MinLength = 1)]
        public string RequestValidationType {
            get {
                return (string)base[_propRequestValidationType];
            }
            set {
                base[_propRequestValidationType] = value;
            }
        }

        [ConfigurationProperty("requestPathInvalidCharacters", DefaultValue = DefaultRequestPathInvalidCharacters)]
        public string RequestPathInvalidCharacters {
            get {
                return (string)base[_propRequestPathInvalidCharacters];
            }
            set {
                base[_propRequestPathInvalidCharacters] = value;
                _RequestPathInvalidCharactersArray = null;
            }
        }

        private int _MaxUrlLength = 0;
        [ConfigurationProperty("maxUrlLength", DefaultValue = DefaultMaxUrlLength)]
        [IntegerValidator(MinValue = 0)]
        public int MaxUrlLength {
            get {
                if (_MaxUrlLength == 0)
                    _MaxUrlLength = (int)base[_propMaxUrlLength];
                return _MaxUrlLength;
            }
            set {
                _MaxUrlLength = value;
                base[_propMaxUrlLength] = value;
            }
        }


        private int _MaxQueryStringLength = 0;
        [ConfigurationProperty("maxQueryStringLength", DefaultValue = DefaultMaxQueryStringLength)]
        [IntegerValidator(MinValue = 0)]
        public int MaxQueryStringLength {
            get {
                if (_MaxQueryStringLength == 0)
                    _MaxQueryStringLength = (int)base[_propMaxQueryStringLength];
                return _MaxQueryStringLength;
            }
            set {
                _MaxQueryStringLength = value;
                base[_propMaxQueryStringLength] = value;
            }
        }

        [ConfigurationProperty("relaxedUrlToFileSystemMapping", DefaultValue = DefaultRelaxedUrlToFileSystemMapping)]
        public bool RelaxedUrlToFileSystemMapping {
            get {
                return (bool)base[_propRelaxedUrlToFileSystemMapping];
            }
            set {
                base[_propRelaxedUrlToFileSystemMapping] = value;
            }
        }

        [ConfigurationProperty("allowDynamicModuleRegistration", DefaultValue = DefaultAllowDynamicModuleRegistration)]
        public bool AllowDynamicModuleRegistration {
            get {
                return (bool)base[_propAllowDynamicModuleRegistration];
            }
            set {
                base[_propAllowDynamicModuleRegistration] = value;
            }
        }

        private int BytesFromKilobytes(int kilobytes) {
            long maxLength = kilobytes * 1024L;
            return ((maxLength < Int32.MaxValue) ? (int)maxLength : Int32.MaxValue);
        }

        internal int MaxRequestLengthBytes {
            get {
                if (_MaxRequestLengthBytes < 0) {
                    _MaxRequestLengthBytes = BytesFromKilobytes(MaxRequestLength);
                }
                return _MaxRequestLengthBytes;
            }
        }

        internal int RequestLengthDiskThresholdBytes {
            get {
                if (_RequestLengthDiskThresholdBytes < 0) {
                    _RequestLengthDiskThresholdBytes = BytesFromKilobytes(RequestLengthDiskThreshold);
                }
                return _RequestLengthDiskThresholdBytes;
            }
        }

        internal String VersionHeader {
            get {
                if (!EnableVersionHeader) {
                    return null;
                }

                if (s_versionHeader == null) {
                    String header = null;
                    // construct once (race condition here doesn't matter)
                    try {
                        String version = VersionInfo.SystemWebVersion;
                        int i = version.LastIndexOf('.');
                        if (i > 0) {
                            header = version.Substring(0, i);
                        }
                    }
                    catch {
                    }

                    if (header == null) {
                        header = String.Empty;
                    }

                    s_versionHeader = header;
                }

                return s_versionHeader;
            }
        }

        //////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////
        private char[] _RequestPathInvalidCharactersArray = null;
        internal char[] RequestPathInvalidCharactersArray {
            get {
                if (_RequestPathInvalidCharactersArray != null)
                    return _RequestPathInvalidCharactersArray;

                _RequestPathInvalidCharactersArray = DecodeAndThenSplitString(RequestPathInvalidCharacters);
                if (_RequestPathInvalidCharactersArray == null) {
                    // Maybe comma was one of the invalid chars, which can cause DecodeAndThenSplitString to fail
                    // Split using the comma and then decode each part
                    _RequestPathInvalidCharactersArray = SplitStringAndThenDecode(RequestPathInvalidCharacters);
                }

                if (_RequestPathInvalidCharactersArray == null) { // failed to construct invalid chars
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_property_generic),
                                                           ElementInformation.Properties[_propRequestPathInvalidCharacters.Name].Source,
                                                           ElementInformation.Properties[_propRequestPathInvalidCharacters.Name].LineNumber);
                }
                return _RequestPathInvalidCharactersArray;
            }
        }

        //////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////
        private static char[] DecodeAndThenSplitString(string invalidCharString) {
            if (string.IsNullOrEmpty(invalidCharString)) {
                return new char[0];
            }

            string[] stringsDecoded = HttpUtility.UrlDecode(invalidCharString, Encoding.UTF8).Split(',');
            char[] charsDecoded = new char[stringsDecoded.Length];

            for (int iter = 0; iter < stringsDecoded.Length; iter++) {
                string decodedString = stringsDecoded[iter].Trim();
                if (decodedString.Length == 1)
                    charsDecoded[iter] = decodedString[0];
                else
                    return null; // failed
            }
            return charsDecoded;
        }

        //////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////
        private static char[] SplitStringAndThenDecode(string invalidCharString) {
            if (string.IsNullOrEmpty(invalidCharString)) {
                return new char[0];
            }

            string[] stringsToDecode = invalidCharString.Split(',');
            char[] charsDecoded = new char[stringsToDecode.Length];

            for (int iter = 0; iter < stringsToDecode.Length; iter++) {
                string decodedString = HttpUtility.UrlDecode(stringsToDecode[iter], Encoding.UTF8).Trim();
                if (decodedString.Length == 1)
                    charsDecoded[iter] = decodedString[0];
                else
                    return null; // failed
            }
            return charsDecoded;
        }

        // This is called as the last step of the deserialization process before the newly created section is seen by the consumer.
        // We can use it to change defaults on-the-fly.
        protected override void SetReadOnly() {
            // Unless overridden, set <httpRuntime requestValidationMode="4.5" />
            ConfigUtil.SetFX45DefaultValue(this, _propRequestValidationMode, VersionUtil.Framework45);

            base.SetReadOnly();
        }
    }
}
