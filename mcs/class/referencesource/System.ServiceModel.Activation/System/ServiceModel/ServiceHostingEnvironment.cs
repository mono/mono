//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activation.Diagnostics;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Routing;
    using System.Xaml.Hosting.Configuration;
    using SR2 = System.ServiceModel.Activation.SR;
    using TD2 = System.ServiceModel.Diagnostics.Application.TD;

    [TypeForwardedFrom("System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public static class ServiceHostingEnvironment
    {
        static object syncRoot = new object();

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile HostingManager hostingManager;
        static bool isHosted;
        static bool isSimpleApplicationHost;
        static Int64 requestCount;
        static bool canGetHtmlErrorMessage = true;
        static string siteName;
        static string applicationVirtualPath;
        static string serviceActivationElementPath;
        static int insufficientMemoryLogCount;
        static DateTime insufficientMemoryLogStartInterval = DateTime.MinValue;
        static readonly TimeSpan InsufficientMemoryLogIntervalDuration = TimeSpan.FromHours(1);

        internal const string VerbPost = "POST";
        internal const string ISAPIApplicationIdPrefix = "/LM/W3SVC/";
        internal const string RelativeVirtualPathPrefix = "~";
        internal const string ServiceParserDelimiter = "|";
        internal const string RootVirtualPath = "~/";
        internal const string PathSeparatorString = "/";

        const char FileExtensionSeparator = '.';
        const char UriSchemeSeparator = ':';
        const char PathSeparator = '/';
        const string SystemWebComma = "System.Web,";
        const int MaxInsufficientMemoryLogCount = 10;

        [Fx.Tag.SecurityNote(Critical = "Calls into an unsafe UnsafeLogEvent method.",
            Safe = "Event identities cannot be spoofed as they are constants determined inside the method.")]
        [SecuritySafeCritical]
        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                Exception exception = e.ExceptionObject as Exception;
                DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.WebHost,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WebHostUnhandledException, true,
                    TraceUtility.CreateSourceString(sender),
                    exception == null ? string.Empty : exception.ToString());
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called by ProcessRequest outside of the restricted SecurityContext.")]
        public static bool AspNetCompatibilityEnabled
        {
            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            get
            {
                if (!IsHosted)
                {
                    return false;
                }

                return IsAspNetCompatibilityEnabled();
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called by ProcessRequest outside of the restricted SecurityContext.")]
        public static bool MultipleSiteBindingsEnabled
        {
            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            get
            {
                if (!IsHosted)
                    return false;

                return IsMultipleSiteBindingsEnabledEnabled();
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called by ServiceHostFactory.CreateServiceHost.")]
        internal static Uri[] PrefixFilters
        {
            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview")]
            get
            {
                if (!IsHosted)
                {
                    return null;
                }

                return GetBaseAddressPrefixFilters();
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool IsAspNetCompatibilityEnabled()
        {
            return hostingManager.AspNetCompatibilityEnabled;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool IsMultipleSiteBindingsEnabledEnabled()
        {
            return hostingManager.MultipleSiteBindingsEnabled;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static Uri[] GetBaseAddressPrefixFilters()
        {
            return hostingManager.BaseAddressPrefixFilters;
        }

        public static void EnsureServiceAvailable(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
            {
                throw FxTrace.Exception.ArgumentNull("virtualPath");
            }

            if (virtualPath.IndexOf(UriSchemeSeparator) > 0)
            {
                throw FxTrace.Exception.Argument("virtualPath", SR2.Hosting_AddressIsAbsoluteUri(virtualPath));
            }

            EventTraceActivity eventTraceActivity = null;
            if (Fx.Trace.IsEtwProviderEnabled)
            {
                eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
            }

            EnsureInitialized();
            virtualPath = NormalizeVirtualPath(virtualPath);
            EnsureServiceAvailableFast(virtualPath, eventTraceActivity);
        }

        internal static void EnsureServiceAvailableFast(string relativeVirtualPath, EventTraceActivity eventTraceActivity)
        {
            try
            {
                hostingManager.EnsureServiceAvailable(relativeVirtualPath, eventTraceActivity);
            }
            catch (ServiceActivationException exception)
            {
                LogServiceActivationException(exception, eventTraceActivity);

                throw;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into an unsafe UnsafeLogEvent method.",
            Safe = "Event identities cannot be spoofed as they are constants determined inside the method.")]
        [SecuritySafeCritical]
        private static void LogServiceActivationException(ServiceActivationException exception, EventTraceActivity eventTraceActivity)
        {
            if (exception.InnerException is HttpException)
            {
                string messageAsString = SafeTryGetHtmlErrorMessage((HttpException)exception.InnerException);
                if (string.IsNullOrEmpty(messageAsString))
                {
                    messageAsString = exception.Message;
                }
                DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.WebHost,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WebHostHttpError, true,
                    TraceUtility.CreateSourceString(hostingManager),
                    messageAsString, exception.ToString());
            }
            else if (exception.InnerException is InsufficientMemoryException)
            {
                //Fix for CSDMain #113776
                //This logic prevents InsufficientMemoryExceptions from flooding the event log by logging at most a fixed number ('MaxInsufficientMemoryLogCount') of these exceptions 
                //per fixed time interval ('InsufficientMemoryLogIntervalDuration').  If this limit is hit, no exceptions of this type are logged for a full time interval.
                DateTime now = DateTime.UtcNow;
                bool shouldLog = false;
                bool reachedMax = false;
                if (now - insufficientMemoryLogStartInterval > InsufficientMemoryLogIntervalDuration || insufficientMemoryLogCount < MaxInsufficientMemoryLogCount)
                {
                    //This lock ensures that the log count is only reset once, and that no race conditions exist for the log count and insufficientMemoryLogStartInterval
                    //These 2 static variables are only modified within this lock, and only read in this lock and in its containing if statement.
                    lock (ThisLock)
                    {
                        if (now - insufficientMemoryLogStartInterval > InsufficientMemoryLogIntervalDuration)
                        {
                            insufficientMemoryLogCount = 0;
                            insufficientMemoryLogStartInterval = now;
                        }
                        if (insufficientMemoryLogCount < MaxInsufficientMemoryLogCount)
                        {
                            insufficientMemoryLogCount++;
                            shouldLog = true;
                        }
                        if (insufficientMemoryLogCount == MaxInsufficientMemoryLogCount)
                        {
                            //We set the 'insufficientMemoryLogStartInterval' to DateTime.Now so that no InsufficientMemoryExceptions are logged for a full time interval of duration "InsufficientMemoryLogIntervalDuration"
                            insufficientMemoryLogStartInterval = now;
                            reachedMax = true;
                        }
                    }
                }

                if (shouldLog)
                {
                    //The lock above ensures that this line is hit no more than MaxInsufficientMemoryLogCount per time interval.
                    DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.WebHost,
                        (uint)System.Runtime.Diagnostics.EventLogEventId.WebHostFailedToProcessRequest, true,
                        TraceUtility.CreateSourceString(hostingManager), exception.ToString());

                    //The lock above ensures that this if statement is entered exactly once if >= MaxInsufficientMemoryLogCount InsufficientMemoryExceptions are thrown in one time interval.
                    if (reachedMax)
                    {
                        DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Warning, (ushort)System.Runtime.Diagnostics.EventLogCategory.WebHost,
                            (uint)System.Runtime.Diagnostics.EventLogEventId.WebHostNotLoggingInsufficientMemoryExceptionsOnActivationForNextTimeInterval, true,
                            SR2.Hosting_NotLoggingInsufficientMemoryExceptionsOnActivationForNextTimeInterval(InsufficientMemoryLogIntervalDuration.ToString()),
                            TraceUtility.CreateSourceString(hostingManager));
                    }
                }
            }
            else
            {
                DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Error, (ushort)System.Runtime.Diagnostics.EventLogCategory.WebHost,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WebHostFailedToProcessRequest, true,
                    TraceUtility.CreateSourceString(hostingManager), exception.ToString());
            }
            if (TD2.ServiceExceptionIsEnabled())
            {
                TD2.ServiceException(eventTraceActivity, exception.ToString(), typeof(ServiceActivationException).FullName);
            }
        }

        static string SafeTryGetHtmlErrorMessage(HttpException exception)
        {
            if (exception != null && canGetHtmlErrorMessage)
            {
                try
                {
                    return exception.GetHtmlErrorMessage();
                }
                catch (SecurityException e)
                {
                    canGetHtmlErrorMessage = false;

                    // not re-throwing on purpose
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                }
            }
            return null;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
        internal static void IncrementRequestCount(ref EventTraceActivity eventTraceActivity, string requestUrl)
        {
            Interlocked.Increment(ref requestCount);
            
            if (Fx.Trace.IsEtwProviderEnabled)
            {
                // aspnet provider might provide a guid. We will transfer this over
                // We use a new id since the thread that comes might be reusing it.
                Guid relatedId = EventTraceActivity.GetActivityIdFromThread();
                eventTraceActivity = new EventTraceActivity();                
                if (TD.WebHostRequestStartIsEnabled())
                {                
                    TD.WebHostRequestStart(eventTraceActivity, AppDomain.CurrentDomain.FriendlyName, requestUrl, relatedId);
                }
            }            
        }

        internal static void DecrementRequestCount(EventTraceActivity eventTraceActivity)
        {
            Interlocked.Decrement(ref requestCount);
            Fx.Assert(requestCount >= 0, "Request count should always be non-nagative.");
            if (requestCount == 0)
            {
                if (hostingManager != null)
                {
                    hostingManager.NotifyAllRequestDone();
                }
            }
            if (TD.WebHostRequestStopIsEnabled())
            {
                TD.WebHostRequestStop(eventTraceActivity);
            }
        }

        internal static string CurrentVirtualPath
        {
            get
            {
                Fx.Assert(IsHosted, "CurrentVirtualPath should not be called from non web-hosted environment.");
                return HostingManager.CurrentVirtualPath;
            }
        }

        internal static string ServiceActivationElementPath
        {
            get
            {
                if (ServiceHostingEnvironment.serviceActivationElementPath == null)
                {
                    ServiceHostingEnvironment.serviceActivationElementPath = string.Format(CultureInfo.CurrentCulture, "{0}/{1}",
                        ConfigurationStrings.ServiceHostingEnvironmentSectionPath, ConfigurationStrings.ServiceActivations);
                }
                return ServiceHostingEnvironment.serviceActivationElementPath;
            }
        }

        internal static string SiteName
        {
            get
            {
                if (ServiceHostingEnvironment.siteName == null)
                {
                    ServiceHostingEnvironment.siteName = HostingEnvironment.SiteName;
                }
                return ServiceHostingEnvironment.siteName;
            }
        }

        internal static string ApplicationVirtualPath
        {
            get
            {
                if (ServiceHostingEnvironment.applicationVirtualPath == null)
                {
                    ServiceHostingEnvironment.applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                }
                return ServiceHostingEnvironment.applicationVirtualPath;
            }
        }
        internal static string FullVirtualPath
        {
            get
            {
                Fx.Assert(IsHosted, "FullVirtualPath should not be called from non web-hosted environment.");
                return HostingManager.FullVirtualPath;
            }
        }
        internal static string XamlFileBaseLocation
        {
            get
            {
                Fx.Assert(IsHosted, "XamlFileBaseLocation should not be called from non web-hosted environment.");
                return HostingManager.XamlFileBaseLocation;
            }
        }
        internal static bool IsConfigurationBased
        {
            get
            {
                Fx.Assert(IsHosted, "IsConfigurationBased should not be called from non web-hosted environment.");
                return HostingManager.IsConfigurationBased;
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called by ProcessRequest outside of the restricted SecurityContext.")]
        internal static ServiceType GetServiceType(string extension)
        {
            Fx.Assert(IsHosted, "GetServiceType should not be called from non web-hosted environment.");
            return hostingManager.GetServiceType(extension);
        }


        internal static bool EnsureWorkflowService(string path)
        {
            Fx.Assert(IsHosted, "EnsureWorkflowService should not be called from non web-hosted environment.");

            PathInfo pathInfo = PathCache.EnsurePathInfo(path);
            return pathInfo.IsWorkflowService();
        }

        internal static bool IsRecycling
        {
            get
            {
                Fx.Assert(IsHosted, "IsRecycling should not be called from non web-hosted environment.");
                return hostingManager.IsRecycling;
            }
        }

        static object ThisLock
        {
            get
            {
                return syncRoot;
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called by ProcessRequest outside of the restricted SecurityContext.")]
        internal static bool IsConfigurationBasedService(HttpApplication application)
        {
            Fx.Assert(IsHosted, "IsConfigurationBased should not be called from non web-hosted environment.");
            string dummyString;
            return IsConfigurationBasedService(application, out dummyString);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called by MsmqHostedTransportManager outside of the restricted SecurityContext.")]
        internal static bool IsConfigurationBasedService(string virtualPath)
        {
            Fx.Assert(IsHosted, "IsConfigurationBased should not be called from non web-hosted environment.");
            return hostingManager.IsConfigurationBasedServiceVirtualPath(virtualPath);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called by ProcessRequest outside of the restricted SecurityContext.")]
        internal static bool IsConfigurationBasedService(HttpApplication application, out string matchedVirtualPath)
        {
            Fx.Assert(IsHosted, "IsConfigurationBased should not be called from non web-hosted environment.");
            bool isCBAService = false;
            matchedVirtualPath = null;
            string virtualPath = application.Request.AppRelativeCurrentExecutionFilePath;
            if (!string.IsNullOrEmpty(virtualPath) && hostingManager.IsConfigurationBasedServiceVirtualPath(virtualPath))
            {
                matchedVirtualPath = virtualPath;
                isCBAService = true;
            }
            return isCBAService;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called by ProcessRequest outside of the restricted SecurityContext.")]
        internal static void SafeEnsureInitialized()
        {
            if (hostingManager == null)
            {
                AspNetPartialTrustHelpers.PartialTrustInvoke(new ContextCallback(OnEnsureInitialized), null);
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
        internal static void EnsureAllReferencedAssemblyLoaded()
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            BuildManager.GetReferencedAssemblies();
        }

        static void OnEnsureInitialized(object state)
        {
            EnsureInitialized();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void EnsureInitialized()
        {
            System.ServiceModel.Diagnostics.TraceUtility.SetEtwProviderId();
            if (hostingManager != null)
            {
                return;
            }

            FxTrace.Trace.SetAnnotation(() => System.ServiceModel.Diagnostics.TraceUtility.GetAnnotation(OperationContext.Current));

            lock (ThisLock)
            {
                if (hostingManager != null)
                {
                    return;
                }

                if (!HostingEnvironmentWrapper.IsHosted)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.Hosting_ProcessNotExecutingUnderHostedContext, "ServiceHostingEnvironment.EnsureServiceAvailable")));
                }

                HostingManager tempHostingManager = new HostingManager();

                // register the following code when we use the service environment class
                // the first time
                HookADUnhandledExceptionEvent();

                isSimpleApplicationHost = GetIsSimpleApplicationHost();

                HostedAspNetEnvironment.Enable();
                isHosted = true;

                hostingManager = tempHostingManager;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Satisfies a LinkDemand for SecurityPermission(ControlAppDomain) on HookADUnhandledExceptionEvent.",
            Safe = "No control flow in for handler.")]
        [SecuritySafeCritical]
        static void HookADUnhandledExceptionEvent()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        }

        [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical property UnsafeApplicationID to get application id with an elevation.",
            Safe = "Processes result into a simple bool which is not protected.")]
        [SecuritySafeCritical]
        static bool GetIsSimpleApplicationHost()
        {
            // ASPNET won't provide API to check Cassini. But it's safe and performant to check only
            // the ApplicationID prefix (MessageBus 
            return (string.Compare(ISAPIApplicationIdPrefix, 0,
                    HostingEnvironmentWrapper.UnsafeApplicationID, 0, ISAPIApplicationIdPrefix.Length, StringComparison.OrdinalIgnoreCase) != 0);
        }

        // customer input can be "/appname/<folder>/filename" or "~/<folder>/filename, we will normalize them to application relative one
        // i.e., "~/<folder>/filename
        internal static string NormalizeVirtualPath(string virtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            string processedVirtualPath = null;

            try
            {
                // Convert the virtual path to relative if not already is.
                processedVirtualPath = VirtualPathUtility.ToAppRelative(virtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath);
            }
            catch (HttpException exception)
            {
                // We want to throw an ArgumentException.
                throw FxTrace.Exception.AsError(new ArgumentException(exception.Message, "virtualPath", exception));
            }

            if (string.IsNullOrEmpty(processedVirtualPath) ||
                !processedVirtualPath.StartsWith(RelativeVirtualPathPrefix, StringComparison.Ordinal))
            {
                throw FxTrace.Exception.Argument("virtualPath",
                    SR2.Hosting_AddressPointsOutsideTheVirtualDirectory(virtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath));
            }

            // Find the position to start.
            int pos = processedVirtualPath.IndexOf(FileExtensionSeparator);

            while (pos > 0)
            {
                // Search inside the processedVirtualPath to find the extension.
                pos = processedVirtualPath.IndexOf(PathSeparator, pos + 1);

                string subVirtualPath = (pos == -1) ? processedVirtualPath : processedVirtualPath.Substring(0, pos);
                string extension = VirtualPathUtility.GetExtension(subVirtualPath);
                if ((!string.IsNullOrEmpty(extension)) &&
                     ServiceHostingEnvironment.GetServiceType(extension) != ServiceType.Unknown)
                {
                    // Remove the pathinfo.
                    return subVirtualPath;
                }
            }

            throw FxTrace.Exception.AsError(new EndpointNotFoundException(SR2.Hosting_ServiceNotExist(virtualPath)));
        }

        internal static bool IsHosted
        {
            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            get
            {
                return isHosted;
            }
        }

        internal static bool IsSimpleApplicationHost
        {
            get
            {
                Fx.Assert(IsHosted, "IsSimpleApplicationHost should not be called from non web-hosted environment.");
                return isSimpleApplicationHost;
            }
        }
        
        internal enum ServiceType
        {
            Unknown = 0,
            WCF,
            Workflow
        }

        class HostingManager : IRegisteredObject
        {
            readonly CollectibleLRUCache<string, ServiceHostBase> directory;
            readonly ExtensionHelper extensions;
            bool aspNetCompatibilityEnabled;
            bool multipleSiteBindingsEnabled;
            bool isUnregistered;
            bool isRecycling;
            bool isStopStarted;
            static bool canDebugPrint = true;
            object activationLock = new object();
            Uri[] baseAddressPrefixFilters;
            Hashtable serviceActivations;
            //used to track if HostingEnvironment.RegisterObject has been called.
            bool isRegistered;

            // One instance per appdomain, don't need to be disposed.
            ManualResetEvent allRequestDoneInStop = new ManualResetEvent(false);

            [Fx.Tag.SecurityNote(Critical = "Admin-provided value that allows for machine resource allocation.")]
            [SecurityCritical]
            int minFreeMemoryPercentageToActivateService;

            bool closeIdleServicesAtLowMemory;

            [ThreadStatic]
            static string currentVirtualPath;

            [ThreadStatic]
            static string fullVirtualPath;

            [ThreadStatic]
            static string xamlFileBaseLocation;

            [ThreadStatic]
            static bool isConfigurationBased;

            [ThreadStatic]
            static bool isAspNetRoutedRequest;

            internal HostingManager()
            {
                this.directory = new CollectibleLRUCache<string, ServiceHostBase>(16, StringComparer.OrdinalIgnoreCase);
                this.extensions = new ExtensionHelper();
                LoadConfigParameters();
            }

            [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical method UnsafeGetSection to get config with an elevation. Sets minFreeMemoryPercentageToActivateService",
                Safe = "Does not leak config objects.")]
            [SecuritySafeCritical]
            void LoadConfigParameters()
            {
                ServiceHostingEnvironmentSection section = ServiceHostingEnvironmentSection.UnsafeGetSection();
                this.aspNetCompatibilityEnabled = section.AspNetCompatibilityEnabled;
                this.multipleSiteBindingsEnabled = section.MultipleSiteBindingsEnabled;
                this.minFreeMemoryPercentageToActivateService = section.MinFreeMemoryPercentageToActivateService;
                this.closeIdleServicesAtLowMemory = section.CloseIdleServicesAtLowMemory;
                List<Uri> prefixFilters = new List<Uri>();

                foreach (BaseAddressPrefixFilterElement element in section.BaseAddressPrefixFilters)
                {
                    prefixFilters.Add(element.Prefix);
                }
                this.baseAddressPrefixFilters = prefixFilters.ToArray();
                this.serviceActivations = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                foreach (ServiceActivationElement element in section.ServiceActivations)
                {
                    if (string.IsNullOrEmpty(element.Factory) && string.IsNullOrEmpty(element.Service))
                    {
                        throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.Hosting_NoServiceAndFactorySpecifiedForFilelessService(ConfigurationStrings.Factory, ConfigurationStrings.Service, element.RelativeAddress, ServiceActivationElementPath)));
                    }

                    string normalizedRelativeAddress = NormalizedRelativeAddress(element.RelativeAddress);
                    string value = string.Format(CultureInfo.CurrentCulture, "{0}|{1}|{2}", normalizedRelativeAddress, element.Factory, element.Service);

                    try
                    {
                        this.serviceActivations.Add(normalizedRelativeAddress, value);
                        if (TD.CBAEntryReadIsEnabled())
                        {
                            TD.CBAEntryRead(element.RelativeAddress, normalizedRelativeAddress);
                        }
                    }
                    catch (ArgumentException)
                    {
                        throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.Hosting_RelativeAddressHasBeenAdded(element.RelativeAddress, ServiceActivationElementPath)));
                    }
                }
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            internal ServiceType GetServiceType(string extension)
            {
                return extensions.GetServiceType(extension);
            }

            internal bool AspNetCompatibilityEnabled
            {
                [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
                get
                {
                    return this.aspNetCompatibilityEnabled;
                }
            }

            internal bool MultipleSiteBindingsEnabled
            {
                [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
                get
                {
                    return this.multipleSiteBindingsEnabled;
                }
            }

            internal Uri[] BaseAddressPrefixFilters
            {
                [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
                get
                {
                    return this.baseAddressPrefixFilters;
                }
            }

            internal static string CurrentVirtualPath
            {
                get
                {
                    return currentVirtualPath;
                }
            }

            internal static string FullVirtualPath
            {
                get
                {
                    return fullVirtualPath;
                }
            }

            internal static string XamlFileBaseLocation
            {
                get
                {
                    return xamlFileBaseLocation;
                }
            }

            internal static bool IsConfigurationBased
            {
                get
                {
                    return isConfigurationBased;
                }
            }

            object ActivationLock
            {
                get
                {
                    return this.activationLock;
                }
            }

            internal bool IsRecycling
            {
                get
                {
                    return isRecycling;
                }
            }

            internal string NormalizedRelativeAddress(string relativeAddress)
            {
                // since it is almost impossible for us to validate the format of a relativeAddress
                // we just take what users' inputs but we need to normalize them with a formal format
                // so that we can index them in a table.
                // we will convert "[folder/]filename.extension" to "~/[folder/]filename.extension"
                string originalRelativeAddress = relativeAddress;

                try
                {
                    if (VirtualPathUtility.IsAbsolute(relativeAddress))
                    {
                        throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.Hosting_RelativeAddressFormatError(relativeAddress)));
                    }

                    relativeAddress = VirtualPathUtility.Combine(RootVirtualPath, relativeAddress);
                    string extension = VirtualPathUtility.GetExtension(relativeAddress);
                    if (string.IsNullOrEmpty(extension))
                    {
                        throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.Hosting_NoValidExtensionFoundForRegistedFilelessService(originalRelativeAddress, ServiceActivationElementPath)));
                    }
                    else if (GetServiceType(extension) == ServiceType.Unknown)
                    {
                        throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.Hosting_RelativeAddressExtensionNotSupportError(extension, originalRelativeAddress, ServiceActivationElementPath)));
                    }
                }
                // since we did Empty/Null string checking in configuration element validator, we should not hit ArgumentException, just catch HttpException for invalid characher 
                catch (HttpException ex)
                {
                    throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR2.Hosting_RelativeAddressFormatError(originalRelativeAddress), ex));
                }
                return relativeAddress;
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            internal bool IsConfigurationBasedServiceVirtualPath(string normalizedVirtualPath)
            {
                return this.serviceActivations.ContainsKey(normalizedVirtualPath);
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            internal bool TryGetCompiledCustomStringFromCBA(string normalizedVirtualPath, out string compiledCustomString)
            {
                compiledCustomString = null;
                bool isCBAService = false;
                if (isConfigurationBased)
                {
                    compiledCustomString = (string)serviceActivations[normalizedVirtualPath];
                    isCBAService = true;
                }
                return isCBAService;
            }

            internal void EnsureServiceAvailable(string normalizedVirtualPath, EventTraceActivity eventTraceActivity)
            {
                TryDebugPrint("HostingManager.EnsureServiceAvailable(" + normalizedVirtualPath + ")");

                ServiceActivationInfo activationInfo = null;

                // 1. Try finding the service without a lock.
                activationInfo = (ServiceActivationInfo)this.directory[normalizedVirtualPath];
                if (activationInfo != null && activationInfo.Initialized)
                {
                    return;
                }

                if (TD.ServiceActivationStartIsEnabled())
                {
                    TD.ServiceActivationStart(eventTraceActivity);
                }

                // 2. Special casing two cases (Routing and Config-based activation)
                isAspNetRoutedRequest = ServiceRouteHandler.IsActiveAspNetRoute(normalizedVirtualPath);
                isConfigurationBased = IsConfigurationBasedServiceVirtualPath(normalizedVirtualPath);

                // Check service file existence if not config based activation or aspnet routing. 
                if (!isAspNetRoutedRequest && !isConfigurationBased
                     && !HostingEnvironmentWrapper.ServiceFileExists(normalizedVirtualPath))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new EndpointNotFoundException(
                                SR2.Hosting_ServiceNotExist(
                                VirtualPathUtility.ToAbsolute(normalizedVirtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath))));
                }

                // 3. Use global lock to create ServiceActivationInfo if necessary.
                using (directory.CreateWriterLockScope())
                {
                    FailActivationIfRecyling(normalizedVirtualPath);

                    // We need to call RegisterObject inside the WriterLockScope because it would ---- with UnregisterObject
                    if (!isRegistered)
                    {
                        RegisterObject();
                        isRegistered = true;
                    }

                    activationInfo = (ServiceActivationInfo)this.directory.UnsafeGet(normalizedVirtualPath);
                    if (activationInfo != null)
                    {
                        if (activationInfo.Initialized)
                        {
                            return;
                        }

                        if (activationInfo.LastException != null)
                        {
                            // Remove the last failed item from the cache
                            directory.UnsafeRemove(activationInfo);
                            activationInfo = null;
                        }
                    }

                    if (activationInfo == null)
                    {
                        activationInfo = new ServiceActivationInfo(hostingManager, normalizedVirtualPath);
                        directory.UnsafeAdd(activationInfo);
                    }
                }

                // 4. Use local lock to activate the service.
                lock (activationInfo)
                {
                    if (activationInfo.Initialized)
                    {
                        return;
                    }

                    if (activationInfo.LastException != null)
                    {
                        // The previous activation for the same service has failed while the current request
                        // is being processed. There is no need to re-activate the service.
                        throw FxTrace.Exception.AsError(activationInfo.LastException);
                    }

                    Exception lastException = null;
                    try
                    {
                        FailActivationIfRecyling(normalizedVirtualPath);

                        CheckMemoryCloseIdleServices(eventTraceActivity);
                        ActivateService(activationInfo, eventTraceActivity);

                        FailActivationIfRecyling(normalizedVirtualPath);
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(
                                TraceEventType.Information, TraceCode.WebHostServiceActivated, SR2.TraceCodeWebHostServiceActivated,
                                new StringTraceRecord("VirtualPath", VirtualPathUtility.ToAbsolute(normalizedVirtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath)), this, (Exception)null);
                        }

                        if (TD.ServiceHostStartedIsEnabled())
                        {
                            string serviceName = string.Empty;
                            ServiceHostBase host = activationInfo.Value as ServiceHostBase;
                            if (host != null)
                            {
                                if (null != host.Description.ServiceType)
                                {
                                    serviceName = host.Description.ServiceType.FullName;
                                }
                                else
                                {
                                    serviceName = host.Description.Namespace + host.Description.Name;
                                }
                            }
                            if (string.IsNullOrEmpty(serviceName))
                            {
                                serviceName = SR2.ServiceTypeUnknown;
                            }

                            string servicePath = normalizedVirtualPath.Replace("~", ServiceHostingEnvironment.ApplicationVirtualPath + "|");
                            string hostReference = string.Format(CultureInfo.InvariantCulture, "{0}{1}|{2}", ServiceHostingEnvironment.SiteName, servicePath, host.Description.Name);
                            TD.ServiceHostStarted(eventTraceActivity, serviceName, hostReference);
                        }

                        activationInfo.SetInitialized();
                    }
                    catch (HttpCompileException ex)
                    {
                        lastException = new ServiceActivationException(SR2.Hosting_ServiceCannotBeActivated(VirtualPathUtility.ToAbsolute(normalizedVirtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath), ex.Message), ex);
                        throw FxTrace.Exception.AsError(lastException);
                    }
                    catch (ServiceActivationException ex)
                    {
                        lastException = ex;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // If it is a fatal exception, don't wrap it.
                        if (Fx.IsFatal(ex))
                        {
                            lastException = ex;
                            throw;
                        }

                        lastException = new ServiceActivationException(SR2.Hosting_ServiceCannotBeActivated(VirtualPathUtility.ToAbsolute(normalizedVirtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath), ex.Message), ex);
                        throw FxTrace.Exception.AsError(lastException);
                    }
                    finally
                    {
                        currentVirtualPath = null;
                        fullVirtualPath = null;
                        xamlFileBaseLocation = null;

                        if (lastException != null)
                        {
                            activationInfo.SetLastException(lastException);
                        }
                    }
                }

                FailActivationIfRecyling(normalizedVirtualPath);
                if (TD.ServiceActivationStopIsEnabled())
                {
                    TD.ServiceActivationStop(eventTraceActivity);
                }
            }

            void CheckMemoryCloseIdleServices(EventTraceActivity eventTraceActivity)
            {
                lock (ActivationLock)
                {
                    bool shouldWaitForCollectComplete = false;

                    ulong availableMemoryBytes;
                    if (!CheckMemoryGates(out availableMemoryBytes))
                    {
                        using (directory.CreateWriterLockScope())
                        {
                            int totalCount = directory.Count;
                            if (!directory.UnsafeBeginBatchCollect())
                            {
                                throw FxTrace.Exception.AsError(new InsufficientMemoryException(
                                    System.ServiceModel.Activation.SR.Hosting_MemoryGatesCheckFailed(availableMemoryBytes,
                                    this.minFreeMemoryPercentageToActivateService)));
                            }

                            if (directory.Count < totalCount)
                            {
                                if (TD.IdleServicesClosedIsEnabled())
                                {
                                    TD.IdleServicesClosed(eventTraceActivity, totalCount - directory.Count, totalCount);
                                }

                                shouldWaitForCollectComplete = true;
                            }
                        }
                    }

                    // Recycling needs to happen outside of the WriterLockScope
                    if (shouldWaitForCollectComplete)
                    {
                        directory.EndBatchCollect();
                    }
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Accesses minFreeMemoryPercentageToActivateService, calls Check.",
                Safe = "No input / output, safe operation if called with administrator-provided value.")]
            [SecuritySafeCritical]
            bool CheckMemoryGates(out ulong availableMemoryBytes)
            {
                return ServiceMemoryGates.Check(this.minFreeMemoryPercentageToActivateService, !this.closeIdleServicesAtLowMemory, out availableMemoryBytes);
            }

            void ActivateService(ServiceActivationInfo serviceActivationInfo, EventTraceActivity eventTraceActivity)
            {
                string normalizedVirtualPath = serviceActivationInfo.GetKey();
                ServiceHostBase service = CreateService(normalizedVirtualPath, eventTraceActivity);
                FailActivationIfRecyling(normalizedVirtualPath);
                serviceActivationInfo.SetService(service, this.closeIdleServicesAtLowMemory);

                try
                {
                    service.Open();
                }
                finally
                {
                    if (service.State != CommunicationState.Opened)
                    {
                        // Abort the service to clear possible cached information.
                        service.Abort();
                    }
                }
                if (TD.AspNetRoutingServiceIsEnabled() && isAspNetRoutedRequest)
                {
                    TD.AspNetRoutingService(eventTraceActivity, normalizedVirtualPath);
                }
            }

            // Why this triple try blocks instead of using "using" statement:
            // 1. "using" will do the impersonation prior to entering the try, 
            //    which leaves an opertunity to Thread.Abort this thread and get it to exit the method still impersonated.
            // 2. put the assignment of unsafeImpersonate in a finally block 
            //    in order to prevent Threat.Abort after impersonation but before the assignment.
            // 3. the finally of a "using" doesn't run until exception filters higher up the stack have executed.
            //    they will do so in the impersonated context if an exception is thrown inside the try.
            // In sumary, this should prevent the thread from existing this method well still impersonated. 
            [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical method UnsafeImpersonate to establish the impersonation context.",
                Safe = "Does not leak anything, does not let caller influence impersonation.")]
            [SecuritySafeCritical]
            string GetCompiledCustomString(string normalizedVirtualPath)
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

                try
                {
                    IDisposable unsafeImpersonate = null;
                    try
                    {
                        string result = null;
                        if (!this.TryGetCompiledCustomStringFromCBA(normalizedVirtualPath, out result))
                        {
                            try
                            {
                            }
                            finally
                            {
                                unsafeImpersonate = HostingEnvironmentWrapper.UnsafeImpersonate();
                            }
                            result = BuildManager.GetCompiledCustomString(normalizedVirtualPath);
                        }
                        return result;
                    }
                    finally
                    {
                        if (null != unsafeImpersonate)
                        {
                            unsafeImpersonate.Dispose();
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            [SecuritySafeCritical]
            internal Type GetCompiledType(string normalizedVirtualPath)
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

                try
                {
                    IDisposable unsafeImpersonate = null;
                    try
                    {
                        try
                        {
                        }
                        finally
                        {
                            unsafeImpersonate = HostingEnvironmentWrapper.UnsafeImpersonate();
                        }
                        return BuildManager.GetCompiledType(normalizedVirtualPath);
                    }
                    finally
                    {
                        if (null != unsafeImpersonate)
                        {
                            unsafeImpersonate.Dispose();
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            static Uri[] FilterBaseAddressList(Uri[] baseAddresses, Uri[] prefixFilters)
            {
                // Precondition assumption: 
                // filterAddresses only contains one Uri per scheme. 
                // Enforced by throwing exception when duplicates found.
                List<Uri> results = new List<Uri>();
                Dictionary<string, Uri> schemeMappings = new Dictionary<string, Uri>();

                foreach (Uri filterUri in prefixFilters)
                {
                    if (!schemeMappings.ContainsKey(filterUri.Scheme))
                    {
                        schemeMappings.Add(filterUri.Scheme, filterUri);
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.BaseAddressDuplicateScheme, filterUri.Scheme)));
                    }
                }

                foreach (Uri baseUri in baseAddresses)
                {
                    string scheme = baseUri.Scheme;
                    if (schemeMappings.ContainsKey(scheme))
                    {
                        Uri filterUri = schemeMappings[scheme];

                        if ((baseUri.Port == filterUri.Port) &&
                           (string.Compare(baseUri.Host, filterUri.Host, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            results.Add(baseUri);
                        }
                    }
                    else
                    {
                        results.Add(baseUri);
                    }
                }
                return results.ToArray();
            }

            ServiceHostBase CreateService(string normalizedVirtualPath, EventTraceActivity eventTraceActivity)
            {
                string virtualPath;
                string factoryType = "";
                string constructorString;
                ServiceHostBase service = null;
                ServiceHostFactoryBase factory = null;
                string[] compiledStrings = null;
                string compiledString = "";

                if (TD.CompilationStartIsEnabled())
                {
                    TD.CompilationStart(eventTraceActivity);
                }

                // 0. Check AspNet Routing vs CBA 
                // check whether there is a conflict between CBA and AspNetRouting
                // if there is a conflict, using AspNet routing policy to decide which service should be activated 
                // we treat CBA as file. RouteExistingFiles is false means Routing should not override File
                // Todo: when there is a conflict between file/CBA adn route and routing policy was changed dynamically, we still use the old service CSD105890
                if (isAspNetRoutedRequest && isConfigurationBased)
                {
                    if (!RouteTable.Routes.RouteExistingFiles)
                    {
                        ServiceRouteHandler.MarkARouteAsInactive(normalizedVirtualPath);
                        isAspNetRoutedRequest = false;
                    }
                    else
                    {
                        isConfigurationBased = false;
                    }
                }

                // 1. Compile the service
                // The expected format is:
                //      <virtualPath>|<type>|<constructorstring>
                // The first two cannot be empty. 
                if (!isAspNetRoutedRequest)
                {
                    compiledString = GetCompiledCustomString(normalizedVirtualPath);
                    if (string.IsNullOrEmpty(compiledString))
                    {
                        // Assume it is a workflow service - optimize by not calling BuildManager.GetCompiledType
                        // but we need to convert the filename to case sensitive one from the physical file
                        // e.g., incoming request with ~/file.xamlx but physical file has name FiLe.Xamlx
                        // we should convert the virtualPath to ~/FiLe.Xamlx, so that mex can show right case
                        // we cannot make directory path case sensitive as we cannot get this path info with right case
                        string fileName = HostingEnvironmentWrapper.GetServiceFile(normalizedVirtualPath).Name;
                        string pathSegment = normalizedVirtualPath.Substring(0, normalizedVirtualPath.LastIndexOf(PathSeparator) + 1);
                        normalizedVirtualPath = String.Format(CultureInfo.CurrentCulture, "{0}{1}", pathSegment, fileName);
                        constructorString = virtualPath = normalizedVirtualPath;
                        factory = CreateWorkflowServiceHostFactory(normalizedVirtualPath);
                    }
                    else
                    {
                        TryDebugPrint("HostingManager.CreateService() BuildManager.GetCompiledCustomString() returned compiledString: " + compiledString);
                        compiledStrings = compiledString.Split(ServiceParserDelimiter.ToCharArray());
                        if (compiledStrings.Length < 3)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.Hosting_CompilationResultInvalid(normalizedVirtualPath)));
                        }
                        virtualPath = compiledStrings[0];
                        factoryType = compiledStrings[1];
                        constructorString = compiledStrings[2];
                    }
                }
                else
                {
                    ServiceDeploymentInfo serviceInfo = ServiceRouteHandler.GetServiceInfo(normalizedVirtualPath);
                    // use the registered virtualpath to ensure correct case in asp.net route 
                    virtualPath = serviceInfo.VirtualPath;
                    constructorString = serviceInfo.ServiceType;
                    factory = serviceInfo.ServiceHostFactory;
                }

                // We get the virtual path from compiled string so that it will have the correct case.
                // normalizedVirtualPath should be application relative e.g., ~/service.svc
                // absolute path start with / and application name, e.g., /appName/service.svc
                normalizedVirtualPath = virtualPath;

                // convert relative virtualpath to app absolute one for consistency, since we gave an absolute path in compiledcustomstring previously 
                // xamlx, CBA, and AspNet routing use relative virtualpath, while configuration/administration needs an absolute one 
                virtualPath = VirtualPathUtility.ToAbsolute(virtualPath);

                // 2. Add the base addresses
                Uri[] baseAddresses = HostedTransportConfigurationManager.GetBaseAddresses(virtualPath);
                Uri[] prefixFilters = ServiceHostingEnvironment.PrefixFilters;

                if (!this.multipleSiteBindingsEnabled && prefixFilters != null && prefixFilters.Length > 0)
                {
                    baseAddresses = FilterBaseAddressList(baseAddresses, prefixFilters);
                }

                fullVirtualPath = virtualPath;
                if (fullVirtualPath.Length == 0)
                {
                    fullVirtualPath = "/";
                }

                // Get the current virtual path (full path except for the .svc file name).
                currentVirtualPath = virtualPath.Substring(0, virtualPath.LastIndexOf(PathSeparator));
                if (currentVirtualPath.Length == 0)
                {
                    currentVirtualPath = "/";
                    xamlFileBaseLocation = RootVirtualPath;
                }
                else
                {
                    // add trailing slash to support ../a.xamlx in the case .xamlx file is wrapped with .svc
                    // otherwise when combining ~/sub with ../a.xamlx, VirtualPathUtility will return wrong value ~/a.xamlx
                    xamlFileBaseLocation = VirtualPathUtility.AppendTrailingSlash(currentVirtualPath);
                }

                if (isConfigurationBased)
                {
                    xamlFileBaseLocation = RootVirtualPath;
                    if (TD.CBAMatchFoundIsEnabled())
                    {
                        TD.CBAMatchFound(eventTraceActivity, normalizedVirtualPath);
                    }
                }

                if (TD.ServiceHostFactoryCreationStartIsEnabled())
                {
                    TD.ServiceHostFactoryCreationStart(eventTraceActivity);
                }

                // 3. Create service
                if (factory == null)
                {
                    if (string.IsNullOrEmpty(factoryType))
                    {
                        Fx.Assert(!string.IsNullOrEmpty(compiledString), "The compiled string can't be null or empty");
                        factory = new ServiceHostFactory();
                    }
                    else
                    {
                        Type compiledType = Type.GetType(factoryType);
                        //check the type from the assemblies in current domain
                        //since compiledcustomstring does not contain fullname for configured virtual path
                        if (compiledType == null && isConfigurationBased)
                        {
                            EnsureAllReferencedAssemblyLoaded();
                            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                            for (int i = 0; i < assemblies.Length; i++)
                            {
                                compiledType = assemblies[i].GetType(factoryType, false);
                                if (compiledType != null)
                                {
                                    break;
                                }
                            }
                        }
                        if (compiledType == null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.Hosting_FactoryTypeNotResolved(factoryType)));
                        }
                        if (!typeof(ServiceHostFactoryBase).IsAssignableFrom(compiledType))
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.Hosting_IServiceHostNotImplemented(factoryType)));
                        }
                        ConstructorInfo ctor = compiledType.GetConstructor(new Type[] { });
                        if (ctor == null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.Hosting_NoDefaultCtor(factoryType)));
                        }
                        factory = (ServiceHostFactoryBase)ctor.Invoke(new object[] { });
                    }
                }

                if (TD.ServiceHostFactoryCreationStopIsEnabled())
                {
                    TD.ServiceHostFactoryCreationStop(eventTraceActivity);
                }

                // Push assembly context into ServiceHostFactory
                // it is OK for us to ignore CBA case here since no referenced assembly in compiledString for CBA 
                // but do not do it for AspNet routing, since there is no compiledString                
                if (factory is ServiceHostFactory && !isConfigurationBased && !isAspNetRoutedRequest)
                {
                    Fx.Assert(!string.IsNullOrEmpty(compiledString), "The compiled string can't be null or empty");
                    for (int index = 3; index < compiledStrings.Length; ++index)
                    {
                        ((ServiceHostFactory)factory).AddAssemblyReference(compiledStrings[index]);
                    }
                }

                if (TD.CreateServiceHostStartIsEnabled())
                {
                    TD.CreateServiceHostStart(eventTraceActivity);
                }

                service = factory.CreateServiceHost(constructorString, baseAddresses);

                if (TD.CreateServiceHostStopIsEnabled())
                {
                    TD.CreateServiceHostStop(eventTraceActivity);
                }

                if (service == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.Hosting_ServiceHostBaseIsNull(constructorString)));
                }

                // 4. Create VirtualPathExtension for ServiceHostBase
                service.Extensions.Add(new VirtualPathExtension(normalizedVirtualPath, ServiceHostingEnvironment.ApplicationVirtualPath, ServiceHostingEnvironment.SiteName));

                if (service.Description != null)
                {
                    service.Description.Behaviors.Add(new ApplyHostConfigurationBehavior());
                    if (this.multipleSiteBindingsEnabled &&
                        service.Description.Behaviors.Find<UseRequestHeadersForMetadataAddressBehavior>() == null)
                    {
                        service.Description.Behaviors.Add(new UseRequestHeadersForMetadataAddressBehavior());
                    }
                }

                if (TD.CompilationStopIsEnabled())
                {
                    TD.CompilationStop(eventTraceActivity);
                }

                return service;
            }

            //NoInlining - we don't want to load Workflow dlls while activating 3.0 services 
            [MethodImpl(MethodImplOptions.NoInlining)]
            ServiceHostFactoryBase CreateWorkflowServiceHostFactory(string path)
            {
                return PathCache.EnsurePathInfo(path).ServiceModelActivationHandler.GetFactory();
            }

            void FailActivationIfRecyling(string normalizedVirtualPath)
            {
                if (IsRecycling)
                {
                    InvalidOperationException exception = new InvalidOperationException(
                        SR2.Hosting_EnvironmentShuttingDown(normalizedVirtualPath,
                        HostingEnvironmentWrapper.ApplicationVirtualPath));
                    throw FxTrace.Exception.AsError(new ServiceActivationException(exception.Message, exception));
                }
            }

            public void Stop(bool immediate)
            {
                if (!immediate)
                {
                    // Try to wait for all requests to be done, then close all the ServiceHosts.
                    ActionItem.Schedule(new Action<object>(WaitAndCloseCallback), this);
                }
                else
                {
                    // Will execute here only if HostingEnvironment.UnregisterObject hasn't been called.
                    Abort();
                }
            }

            [Conditional("DEBUG")]
            static void TryDebugPrint(string message)
            {
                if (canDebugPrint)
                {
                    try
                    {
                        Debug.Print(message);
                    }
                    catch (SecurityException e)
                    {
                        canDebugPrint = false;

                        // not re-throwing on purpose
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                    }
                }
            }

            void OnServiceClosed(ServiceActivationInfo serviceActivationInfo)
            {
                if (!isRecycling)
                {
                    using (this.directory.CreateWriterLockScope())
                    {
                        this.directory.UnsafeRemove(serviceActivationInfo);

                        // At the time when we just removed all the service, we will unregister 
                        // from HostingEnvironement.
                        UnregisterObject();
                    }
                }
            }

            void OnServiceFaulted(ServiceHostBase host)
            {
                host.Abort();
            }

            void OnServiceBusyCountIncremented(ServiceActivationInfo serviceActivationInfo)
            {
                this.directory.Touch(serviceActivationInfo.GetKey());
            }

            internal void NotifyAllRequestDone()
            {
                if (isStopStarted)
                {
                    allRequestDoneInStop.Set();
                }
            }

            void Abort()
            {
                allRequestDoneInStop.Set();

                directory.Abort();
                using (directory.CreateWriterLockScope())
                {
                    // We need to set isRecycling inside lock because we want to make sure no 
                    // new request will be handed once we start to shut down.
                    isRecycling = true;

                    if (UnregisterObject())
                    {
                        return;
                    }
                }
            }

            void WaitAndCloseCallback(object obj)
            {
                isStopStarted = true;
                if (ServiceHostingEnvironment.requestCount != 0)
                {
                    allRequestDoneInStop.WaitOne();
                }

                using (directory.CreateWriterLockScope())
                {
                    // We need to set isRecycling inside lock because we want to make sure no 
                    // new request will be handed once we start to shut down.
                    isRecycling = true;
                    directory.UnsafeBeginBatchCollect(true);
                }

                directory.EndBatchCollect();

                using (directory.CreateWriterLockScope())
                {
                    UnregisterObject();
                }
            }

            internal static void LogServiceCloseError(string virtualPath, Exception exception, object source)
            {
                if (DiagnosticUtility.ShouldTraceError)
                {
                    TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.WebHostServiceCloseFailed, SR2.TraceCodeWebHostServiceCloseFailed,
                        new StringTraceRecord("VirtualPath", VirtualPathUtility.ToAbsolute(virtualPath, HostingEnvironmentWrapper.ApplicationVirtualPath)),
                        source, exception);
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Uses HostingEnvironmentWrapper.UnsafeRegisterObject which is critical.",
                Safe = "Does not allow the caller to control the variable -- only registers 'this'.")]
            [SecuritySafeCritical]
            void RegisterObject()
            {
                HostingEnvironmentWrapper.UnsafeRegisterObject(this);
            }

            // Note : this method should only be called under lock of the global lock.
            [Fx.Tag.SecurityNote(Critical = "Uses HostingEnvironmentWrapper.UnsafeRegisterObject which is critical.",
                Safe = "Does not allow the caller to control the variable -- only registers 'this'.")]
            [SecuritySafeCritical]
            bool UnregisterObject()
            {
                if (directory.Count == 0)
                {
                    if (!isUnregistered)
                    {
                        isUnregistered = true;
                        HostingEnvironmentWrapper.UnsafeUnregisterObject(this);
                    }
                    return true;
                }

                return false;
            }

            class ExtensionHelper
            {
                readonly IDictionary<string, BuildProviderInfo> buildProviders;

                [Fx.Tag.SecurityNote(Critical = "Loads config through an elevation and stores results.",
                    Safe = "Stores results in BuildProviderInfo instances which restrict access to the BuildProvider config object.")]
                [SecuritySafeCritical]
                public ExtensionHelper()
                {
                    AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
                    buildProviders = new Dictionary<string, BuildProviderInfo>(8, StringComparer.OrdinalIgnoreCase);
                    CompilationSection compilationSection = (CompilationSection)HostedAspNetEnvironment.UnsafeGetSectionFromWebConfigurationManager("system.web/compilation", null);
                    foreach (System.Web.Configuration.BuildProvider buildProvider in compilationSection.BuildProviders)
                    {
                        buildProviders.Add(buildProvider.Extension, new BuildProviderInfo(buildProvider));
                    }
                }

                [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
                public ServiceType GetServiceType(string extension)
                {
                    ServiceType serviceType = ServiceType.Unknown;
                    BuildProviderInfo info;
                    if (buildProviders.TryGetValue(extension, out info))
                    {
                        if (info.IsSupported)
                        {
                            serviceType = ServiceType.WCF;
                        }
                        else if (info.IsXamlBuildProvider)
                        {
                            serviceType = ServiceType.Workflow;
                        }
                    }
                    return serviceType;
                }
            }

            class ServiceActivationInfo : CollectibleLRUCache<string, ServiceHostBase>.CollectibleNode
            {
                HostingManager manager;
                string virtualPath;
                EventHandler serviceClosedHandler;
                EventHandler serviceFaultedHandler;
                bool initialized;
                Exception lastException;
                public ServiceActivationInfo(HostingManager manager, string virtualPath)
                {
                    this.manager = manager;
                    this.virtualPath = virtualPath;
                }

                public bool Initialized
                {
                    get
                    {
                        return this.initialized;
                    }
                }

                public Exception LastException
                {
                    get
                    {
                        return this.lastException;
                    }

                }

                public void SetLastException(Exception lastException)
                {
                    Fx.Assert(!this.initialized, "The ServiceActivationInfo should not be in the initialized state");
                    Abort();

                    this.lastException = lastException;
                }

                public void SetInitialized()
                {
                    this.initialized = true;
                }

                public override string GetKey()
                {
                    return this.virtualPath;
                }

                public void SetService(ServiceHostBase service, bool shouldTrackBusyCountIncrement)
                {
                    this.Value = service;
                    this.serviceClosedHandler = new EventHandler(OnServiceClosed);
                    this.serviceFaultedHandler = new EventHandler(OnServiceFaulted);
                    service.Closed += this.serviceClosedHandler;
                    service.Faulted += this.serviceFaultedHandler;

                    if (shouldTrackBusyCountIncrement)
                    {
                        // If recycling is enabled, we record the busy count events for best LRU statistics
                        service.BusyCountIncremented += OnServiceBusyCountIncremented;
                    }
                }

                public override bool CanClose()
                {
                    if (this.lastException != null)
                    {
                        return true;
                    }

                    // We can remove the entry from the cache only if it is initialized.
                    if (!this.initialized)
                    {
                        return false;
                    }

                    if (this.Value != null)
                    {
                        return (this.Value.BusyCount == 0);
                    }

                    return true;
                }

                // The caller of this method needs to remove the entry from the cache explicitly.
                public override IAsyncResult BeginClose(AsyncCallback callback, object state)
                {
                    ServiceHostBase service = this.Value;
                    if (service == null)
                        return null;

                    UnregisterEvents(service);

                    try
                    {
                        return service.BeginClose(callback, state);
                    }
                    catch (Exception exception)
                    {
                        // If BeginClose throw an exception, abort should already have been called.
                        if (!Fx.IsFatal(exception))
                        {
                            HostingManager.LogServiceCloseError(this.virtualPath, exception, this);
                        }

                        if (!(exception is CommunicationException))
                        {
                            throw;
                        }
                    }

                    return new CompletedAsyncResult(callback, state);
                }

                public override void EndClose(IAsyncResult result)
                {
                    if (result is CompletedAsyncResult)
                    {
                        CompletedAsyncResult.End(result);
                        return;
                    }

                    ServiceHostBase service = this.Value;
                    if (service != null)
                    {
                        try
                        {
                            service.EndClose(result);
                        }
                        catch (Exception exception)
                        {
                            //If EndClose throw an exception, abort should already have been called.
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }

                            // Ignore exceptions occurred when exception happened
                            HostingManager.LogServiceCloseError(this.virtualPath, exception, this);
                        }

                        this.Value = null;
                    }
                }

                public void OnServiceBusyCountIncremented(object sender, EventArgs args)
                {
                    manager.OnServiceBusyCountIncremented(this);
                }

                public override void Abort()
                {
                    ServiceHostBase service = this.Value;
                    if (service != null)
                    {
                        UnregisterEvents(service);
                        service.Abort();
                        this.Value = null;
                    }
                }

                void OnServiceClosed(object sender, EventArgs args)
                {
                    if ((ServiceHostBase)sender == this.Value)
                    {
                        manager.OnServiceClosed(this);
                    }
                }

                void OnServiceFaulted(object sender, EventArgs args)
                {
                    manager.OnServiceFaulted((ServiceHostBase)sender);
                }

                void UnregisterEvents(ServiceHostBase service)
                {
                    if (this.serviceClosedHandler != null)
                    {
                        service.Closed -= this.serviceClosedHandler;
                        this.serviceClosedHandler = null;
                    }

                    if (this.serviceFaultedHandler != null)
                    {
                        service.Faulted -= this.serviceFaultedHandler;
                        this.serviceFaultedHandler = null;
                    }
                }
            }
        }

        class BuildProviderInfo
        {
            [Fx.Tag.SecurityNote(Critical = "Stores the result of an elevation.")]
            [SecurityCritical]
            System.Web.Configuration.BuildProvider buildProvider;

            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile bool initialized;
            bool isSupported;
            bool isXamlBuildProvider;
            object thisLock = new object();

            [Fx.Tag.SecurityNote(Critical = "Stores the result of an elevation.",
                Safe = "Stores it in a Critical field.")]
            [SecuritySafeCritical]
            public BuildProviderInfo(System.Web.Configuration.BuildProvider buildProvider)
            {
                this.buildProvider = buildProvider;
            }

            string BuildProviderType
            {
                [Fx.Tag.SecurityNote(Critical = "Accesses the SecurityCritical buildProvider field.",
                    Safe = "Returns the Type property, which is allowed; doesn't leak the BuildProvider instance.")]
                [SecuritySafeCritical]
                get { return buildProvider.Type; }
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            public bool IsSupported
            {
                get
                {
                    EnsureInitialized();
                    return this.isSupported;
                }
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            public bool IsXamlBuildProvider
            {
                get
                {
                    EnsureInitialized();
                    return this.isXamlBuildProvider;
                }

            }

            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
            void EnsureInitialized()
            {
                if (initialized)
                {
                    return;
                }

                lock (thisLock)
                {
                    if (initialized)
                    {
                        return;
                    }

                    Type type = Type.GetType(BuildProviderType, false);
                    if (type == null)
                    {
                        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        for (int i = 0; i < assemblies.Length; i++)
                        {
                            type = assemblies[i].GetType(BuildProviderType, false);
                            if (type != null)
                            {
                                break;
                            }
                        }
                    }

                    if (type != null)
                    {
                        object[] attributes = ServiceReflector.GetCustomAttributes(type, typeof(ServiceActivationBuildProviderAttribute), true);
                        if (attributes.Length > 0)
                        {
                            this.isSupported = true;
                        }
                        else
                        {
                            //to accomodate for subclasses of XamlBuildProvider
                            if (typeof(System.Xaml.Hosting.XamlBuildProvider).IsAssignableFrom(type))
                            {
                                this.isXamlBuildProvider = true;
                            }
                        }
                    }

                    ClearBuildProvider();
                    initialized = true;
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Accesses the SecurityCritical buildProvider field. Can be called outside user context.",
                Safe = "Just clears it, doesn't leak anything.")]
            [SecuritySafeCritical]
            void ClearBuildProvider()
            {
                this.buildProvider = null;
            }
        }

        static class PathCache
        {
            static Hashtable pathCache = new Hashtable(StringComparer.OrdinalIgnoreCase);
            static object writeLock = new object();

            public static PathInfo EnsurePathInfo(string path)
            {
                PathInfo pathInfo = (PathInfo)pathCache[path];
                if (pathInfo != null)
                {
                    return pathInfo;
                }

                lock (writeLock)
                {
                    pathInfo = (PathInfo)pathCache[path];
                    if (pathInfo != null)
                    {
                        return pathInfo;
                    }

                    if (HostingEnvironmentWrapper.ServiceFileExists(path))
                    {
                        pathInfo = new PathInfo(path);
                        pathCache.Add(path, pathInfo);
                        return pathInfo;
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(new EndpointNotFoundException(SR2.Hosting_ServiceNotExist(path)));
                    }
                }
            }
        }

        class PathInfo
        {
            string path;

            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile PathType type;
            object writeLock;
            Type hostedXamlType;
            Type serviceModelActivationHandlerType;
            IServiceModelActivationHandler serviceModelActivationHandler;

            public PathInfo(string path)
            {
                this.type = PathType.Unknown;
                this.path = path;
                this.writeLock = new object();
            }

            public IServiceModelActivationHandler ServiceModelActivationHandler
            {
                get
                {
                    if (this.serviceModelActivationHandler == null)
                    {
                        if (IsWorkflowService())
                        {
                            this.serviceModelActivationHandler =
                                CreateServiceModelActivationHandler(serviceModelActivationHandlerType) as IServiceModelActivationHandler;
                        }
                        else
                        {
                            //The control can come here when the hosted file is a valid XAML (service OR otherwise) but is configured with 
                            //a handler that does NOT implement IServiceModelActivationHandler and aspnetCompat=true
                            throw FxTrace.Exception.AsError(
                                new EndpointNotFoundException(SR2.Hosting_InvalidHandlerForWorkflowService(
                                    this.serviceModelActivationHandlerType.FullName, this.hostedXamlType.FullName, this.path)));
                        }
                    }
                    return this.serviceModelActivationHandler;
                }
            }

            static object CreateServiceModelActivationHandler(Type type)
            {
                //The handler/factory should have an empty constructor but need not be public
                return Activator.CreateInstance(type,
                BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, null, null);
            }

            public bool IsWorkflowService()
            {
                if (this.type == PathType.Unknown)
                {
                    //Cache won't be available if it is invoked first time 
                    //Use a local "lock" specifically for this url 
                    lock (this.writeLock)
                    {
                        if (this.type == PathType.Unknown)
                        {
                            hostedXamlType = hostingManager.GetCompiledType(this.path);
                            if (IsConfiguredWithSMActivationHandler())
                            {
                                this.type = PathType.WorkflowService;
                            }
                            else
                            {
                                this.type = PathType.NotWorkflowService;
                            }
                        }
                    }
                }
                if (this.type == PathType.WorkflowService)
                {
                    return true;
                }
                return false;
            }

            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "This method doesn't allow callers to access sensitive information, operations, or resources that can be used in a destructive manner.")]
            bool IsConfiguredWithSMActivationHandler()
            {
                if (XamlHostingConfiguration.TryGetHttpHandlerType(this.path, this.hostedXamlType, out this.serviceModelActivationHandlerType))
                {
                    if (typeof(IServiceModelActivationHandler).IsAssignableFrom(this.serviceModelActivationHandlerType))
                    {
                        return true;
                    }
                }
                return false;
            }

            enum PathType
            {
                Unknown,
                WorkflowService,
                NotWorkflowService
            }
        }
    }
}
