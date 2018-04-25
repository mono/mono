//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Transactions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;

    class HostedAspNetEnvironment : AspNetEnvironment
    {
        // On IIS 8.0 (or later) the "WEBSOCKET_VERSION" server property indicates the WebSocket protocol version supported by the server.
        // The IIS WebSocket module sets this property when initialized.
        private const string WebSocketVersionServerProperty = "WEBSOCKET_VERSION";

        // Indicates if we determined the WebSocket version. If false, we'll need to check the "WEBSOCKET_VERSION" server property.
        private static bool isWebSocketVersionSet = false;

        // Provides the version of the WebSocket protocol supported by IIS.
        private static string webSocketVersion;

        // used to cache SiteName|ApplicationVirtualPath
        static string cachedServiceReference;

        // used to cache if windows auth is being used
        Nullable<bool> isWindowsAuthentication;

        HostedAspNetEnvironment()
            : base()
        {
        }

        public override bool AspNetCompatibilityEnabled
        {
            get
            {
                return ServiceHostingEnvironment.AspNetCompatibilityEnabled;
            }
        }

        public override string ConfigurationPath
        {
            get
            {
                if (ServiceHostingEnvironment.CurrentVirtualPath != null)
                {
                    return ServiceHostingEnvironment.CurrentVirtualPath + "web.config";
                }
                else
                {
                    return base.ConfigurationPath;
                }
            }
        }

        public override bool IsConfigurationBased
        {
            get
            {
                return ServiceHostingEnvironment.IsConfigurationBased;
            }
        }

        public override string CurrentVirtualPath
        {
            get
            {
                return ServiceHostingEnvironment.CurrentVirtualPath;
            }
        }

        public override string XamlFileBaseLocation
        {
            get
            {
                return ServiceHostingEnvironment.XamlFileBaseLocation;
            }
        }

        public override bool UsingIntegratedPipeline
        {
            get
            {
                return HttpRuntime.UsingIntegratedPipeline;
            }
        }

        // Provides the version of the WebSocket protocol supported by IIS.
        // Returns null if WebSockets are not supported (because the IIS WebSocketModule is not installed or enabled).
        public override string WebSocketVersion
        {
            get
            {
                return isWebSocketVersionSet ? webSocketVersion : null;
            }
        }

        public static void Enable()
        {
            AspNetEnvironment hostedEnvironment = new HostedAspNetEnvironment();
            AspNetEnvironment.Current = hostedEnvironment;
        }

        /// <summary>
        /// Tries to set the 'WebSocketVersion' property. The first call of this method sets the property (based on the "WEBSOCKET_VERSION" server property). 
        /// Subsequent calls do nothing.
        /// </summary>
        /// <param name="application">The HttpApplication used to determine the WebSocket version.</param>
        /// <remarks>
        /// Take caution when calling this method. The method initializes the 'WebSocketVersion' property based on the "WEBSOCKET_VERSION" server variable.
        /// This variable gets set by the WebSocketModule when it's loaded by IIS. If you call this method too early (before IIS got a chance to load the module list), 
        /// this method might incorrectly set 'WebSocketVersion' to 'null'.
        /// </remarks>
        public static void TrySetWebSocketVersion(HttpApplication application)
        {
            if (!isWebSocketVersionSet)
            {
                webSocketVersion = application.Request.ServerVariables[WebSocketVersionServerProperty];
                isWebSocketVersionSet = true;
            }
        }

        public override void AddHostingBehavior(ServiceHostBase serviceHost, ServiceDescription description)
        {
            VirtualPathExtension virtualPathExtension = serviceHost.Extensions.Find<VirtualPathExtension>();
            if (virtualPathExtension != null)
            {
                description.Behaviors.Add(new HostedBindingBehavior(virtualPathExtension));
            }

            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if (ServiceMetadataBehavior.IsMetadataEndpoint(description, endpoint))
                {
                    endpoint.Behaviors.Add(new HostedMetadataExchangeEndpointBehavior());
                }
            }
        }

        public override bool IsWebConfigAboveApplication(object configHostingContext)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            WebContext context = configHostingContext as WebContext;
            if (context != null)
            {
                return context.ApplicationLevel == WebApplicationLevel.AboveApplication;
            }

            return false; // if we don't recognize the context we can't enforce the special web.config logic
        }

        public override void EnsureCompatibilityRequirements(ServiceDescription description)
        {
            AspNetCompatibilityRequirementsAttribute aspNetCompatibilityRequirements = description.Behaviors.Find<AspNetCompatibilityRequirementsAttribute>();
            if (aspNetCompatibilityRequirements == null)
            {
                aspNetCompatibilityRequirements = new AspNetCompatibilityRequirementsAttribute();
                description.Behaviors.Add(aspNetCompatibilityRequirements);
            }
        }

        public override bool TryGetFullVirtualPath(out string virtualPath)
        {
            // subclass will use the virtual path from the compiled string
            virtualPath = ServiceHostingEnvironment.FullVirtualPath;
            return true;
        }

        public override string GetAnnotationFromHost(ServiceHostBase host)
        {
            //Format Website name\Application Virtual Path|\relative service virtual path|serviceName 
            if (host != null && host.Extensions != null)
            {
                string serviceName = (host.Description != null) ? host.Description.Name : string.Empty;
                string application = ServiceHostingEnvironment.ApplicationVirtualPath;
                string servicePath = string.Empty;
                VirtualPathExtension extension = host.Extensions.Find<VirtualPathExtension>();
                if (extension != null && extension.VirtualPath != null)
                {
                    servicePath = extension.VirtualPath.Replace("~", application + "|");
                    return string.Format(CultureInfo.InvariantCulture, "{0}{1}|{2}", ServiceHostingEnvironment.SiteName, servicePath, serviceName);
                }
            }
            if (string.IsNullOrEmpty(HostedAspNetEnvironment.cachedServiceReference))
            {
                HostedAspNetEnvironment.cachedServiceReference = string.Format(CultureInfo.InvariantCulture, "{0}{1}", ServiceHostingEnvironment.SiteName, ServiceHostingEnvironment.ApplicationVirtualPath);
            }
            return HostedAspNetEnvironment.cachedServiceReference;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - can be called outside of user context.")]
        public override void EnsureAllReferencedAssemblyLoaded()
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            BuildManager.GetReferencedAssemblies();
        }

        public override BaseUriWithWildcard GetBaseUri(string transportScheme, Uri listenUri)
        {
            BaseUriWithWildcard baseAddress = null;
            HostedTransportConfigurationBase hostedConfiguration =
                HostedTransportConfigurationManager.GetConfiguration(transportScheme) as HostedTransportConfigurationBase;
            if (hostedConfiguration != null)
            {
                baseAddress = hostedConfiguration.FindBaseAddress(listenUri);
                if (baseAddress == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_TransportBindingNotFound(listenUri.ToString())));
                }
            }
            return baseAddress;
        }

        public override void ValidateHttpSettings(string virtualPath, bool isMetadataListener, bool usingDefaultSpnList, ref AuthenticationSchemes bindingElementAuthenticationSchemes, ref ExtendedProtectionPolicy extendedProtectionPolicy, ref string realm)
        {
            // Verify the authentication settings
            AuthenticationSchemes hostSupportedSchemes = HostedTransportConfigurationManager.MetabaseSettings.GetAuthenticationSchemes(virtualPath);

            if ((bindingElementAuthenticationSchemes & hostSupportedSchemes) == 0)
            {
                if (bindingElementAuthenticationSchemes == AuthenticationSchemes.Negotiate ||
                    bindingElementAuthenticationSchemes == AuthenticationSchemes.Ntlm ||
                    bindingElementAuthenticationSchemes == AuthenticationSchemes.IntegratedWindowsAuthentication)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.Hosting_AuthSchemesRequireWindowsAuth));
                }
                else
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.Hosting_AuthSchemesRequireOtherAuth(bindingElementAuthenticationSchemes.ToString())));
                }
            }

            //only use AuthenticationSchemes, which are supported both in IIS and the WCF binding
            bindingElementAuthenticationSchemes &= hostSupportedSchemes;

            if (bindingElementAuthenticationSchemes != AuthenticationSchemes.Anonymous)
            {
                //Compare the ExtendedProtectionPolicy setttings to IIS
                ExtendedProtectionPolicy iisPolicy = HostedTransportConfigurationManager.MetabaseSettings.GetExtendedProtectionPolicy(virtualPath);

                if (iisPolicy == null) //OS doesn't support CBT
                {
                    if (extendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Always)
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(SR.ExtendedProtectionNotSupported));
                    }
                }
                else
                {
                    if (isMetadataListener && ChannelBindingUtility.IsDefaultPolicy(extendedProtectionPolicy))
                    {
                        //push the IIS policy onto the metadataListener if and only if the default policy is 
                        //in force. policy for non metadata listeners will still have to match IIS policy.
                        extendedProtectionPolicy = iisPolicy;
                    }
                    else
                    {
                        if (!ChannelBindingUtility.AreEqual(iisPolicy, extendedProtectionPolicy))
                        {
                            string mismatchErrorMessage;
                            if (iisPolicy.PolicyEnforcement != extendedProtectionPolicy.PolicyEnforcement)
                            {
                                mismatchErrorMessage = SR.ExtendedProtectionPolicyEnforcementMismatch(iisPolicy.PolicyEnforcement, extendedProtectionPolicy.PolicyEnforcement);
                            }
                            else if (iisPolicy.ProtectionScenario != extendedProtectionPolicy.ProtectionScenario)
                            {
                                mismatchErrorMessage = SR.ExtendedProtectionPolicyScenarioMismatch(iisPolicy.ProtectionScenario, extendedProtectionPolicy.ProtectionScenario);
                            }
                            else 
                            {
                                Fx.Assert(iisPolicy.CustomChannelBinding != extendedProtectionPolicy.CustomChannelBinding, "new case in ChannelBindingUtility.AreEqual to account for");
                                mismatchErrorMessage = SR.ExtendedProtectionPolicyCustomChannelBindingMismatch;
                            }

                            if (mismatchErrorMessage != null)
                            {
                                throw FxTrace.Exception.AsError(new NotSupportedException(SR.Hosting_ExtendedProtectionPoliciesMustMatch(mismatchErrorMessage)));
                            }
                        }

                        //when using the default SPN list we auto generate, we should make sure that the IIS policy is also the default...
                        ServiceNameCollection listenerSpnList = usingDefaultSpnList ? null : extendedProtectionPolicy.CustomServiceNames;
                        if (!ChannelBindingUtility.IsSubset(iisPolicy.CustomServiceNames, listenerSpnList))
                        {
                            throw FxTrace.Exception.AsError(new NotSupportedException(SR.Hosting_ExtendedProtectionPoliciesMustMatch(SR.Hosting_ExtendedProtectionSPNListNotSubset)));
                        }
                    }
                }
            }

            

            // Do not set realm for Cassini.
            if (!ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                // Set the realm
                realm = HostedTransportConfigurationManager.MetabaseSettings.GetRealm(virtualPath);
            }
        }

        public override bool ValidateHttpsSettings(string virtualPath, ref bool requireClientCertificate)
        {
            // Do not validate settings for Cassini. Actually current implementation of Cassini does not support HTTPS.
            if (ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                return false;
            }

            // Validate Ssl Settings
            HttpAccessSslFlags sslFlags = HostedTransportConfigurationManager.MetabaseSettings.GetAccessSslFlags(virtualPath);
            HttpAccessSslFlags channelListenerSslFlags = HttpAccessSslFlags.None;

            // Validating SSL flags. SslRequireCert means "require client certificate" in IIS terminology.
            if ((sslFlags & HttpAccessSslFlags.SslRequireCert) != 0)
            {
                // Require SSL.
                // We apply IIS settings to the ChannelListener to fix the endpoint
                requireClientCertificate = true;
            }
            else if (requireClientCertificate &&
                // Validating SSL flags. SslNegotiateCert means "accept client certificate" in IIS terminology.
                // We want to allow SslNegotiateCert in IIS to support hosting one endpoint requiring client
                // certificates and another endpoint not using client certificates in the same VirtualDirectory.
                // HttpsChannelListener.ValidateAuthentication ensures that authentication is denied for services
                // requiring client certificates when the client does not present one.
                (sslFlags & HttpAccessSslFlags.SslNegotiateCert) == 0)
            {
                // IIS ignores client cert but the binding requires it.
                channelListenerSslFlags |= HttpAccessSslFlags.SslRequireCert;

                throw FxTrace.Exception.AsError(new NotSupportedException(SR.Hosting_SslSettingsMisconfigured(
                    channelListenerSslFlags.ToString(), sslFlags.ToString())));
            }

            return (sslFlags & HttpAccessSslFlags.SslMapCert) != 0;
        }

        public override void ProcessNotMatchedEndpointAddress(Uri uri, string endpointName)
        {
            if (!object.ReferenceEquals(uri.Scheme, Uri.UriSchemeHttp) &&
                !object.ReferenceEquals(uri.Scheme, Uri.UriSchemeHttps))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_NonHTTPInCompatibilityMode(endpointName)));
            }
        }

        public override void ValidateCompatibilityRequirements(AspNetCompatibilityRequirementsMode compatibilityMode)
        {
            if (compatibilityMode == AspNetCompatibilityRequirementsMode.Allowed)
            {
                return;
            }
            else if (ServiceHostingEnvironment.AspNetCompatibilityEnabled &&
                compatibilityMode == AspNetCompatibilityRequirementsMode.NotAllowed)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_ServiceCompatibilityNotAllowed));
            }
            else if (!ServiceHostingEnvironment.AspNetCompatibilityEnabled &&
                compatibilityMode == AspNetCompatibilityRequirementsMode.Required)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_ServiceCompatibilityRequire));
            }
        }

        public override IAspNetMessageProperty GetHostingProperty(Message message)
        {
            return GetHostingProperty(message, false);
        }

        public override IAspNetMessageProperty GetHostingProperty(Message message, bool removeFromMessage)
        {
            IAspNetMessageProperty result = null;
            object property;

            if (message.Properties.TryGetValue(HostingMessageProperty.Name, out property))
            {
                result = (HostingMessageProperty)property;
                if (removeFromMessage)
                {
                    message.Properties.Remove(HostingMessageProperty.Name);
                }
            }

            return result;
        }

        public override void PrepareMessageForDispatch(Message message)
        {
            ReceiveContext context = null;
            if (ReceiveContext.TryGet(message, out context) && !(context is ReceiveContextBusyCountWrapper))
            {
                ReceiveContextBusyCountWrapper wrapper = new ReceiveContextBusyCountWrapper(context);
                message.Properties.Remove(ReceiveContext.Name);
                message.Properties.Add(ReceiveContext.Name, wrapper);
            }
        }

        public override void ApplyHostedContext(TransportChannelListener listener, BindingContext context)
        {
            VirtualPathExtension virtualPathExtension = context.BindingParameters.Find<VirtualPathExtension>();

            if (virtualPathExtension != null)
            {
                HostedMetadataBindingParameter metadataBindingParameter = context.BindingParameters.Find<HostedMetadataBindingParameter>();
                listener.ApplyHostedContext(virtualPathExtension.VirtualPath, metadataBindingParameter != null);
            }
        }

        internal override void AddMetadataBindingParameters(Uri listenUri, KeyedByTypeCollection<IServiceBehavior> serviceBehaviors, BindingParameterCollection bindingParameters)
        {
            if (serviceBehaviors.Find<HostedBindingBehavior>() != null)
            {
                bindingParameters.Add(new HostedMetadataBindingParameter());
            }

            VirtualPathExtension virtualPathExtension = bindingParameters.Find<VirtualPathExtension>();

            if (virtualPathExtension != null)
            {
                AuthenticationSchemes hostSupportedAuthenticationSchemes = AspNetEnvironment.Current.GetAuthenticationSchemes(listenUri);

                if (hostSupportedAuthenticationSchemes != AuthenticationSchemes.None)
                {
                    if (bindingParameters.Find<AuthenticationSchemesBindingParameter>() == null)
                    {
                        bindingParameters.Add(new AuthenticationSchemesBindingParameter(hostSupportedAuthenticationSchemes));
                    }
                }
            }

            base.AddMetadataBindingParameters(listenUri, serviceBehaviors, bindingParameters);
        }

        internal override bool IsMetadataListener(BindingParameterCollection bindingParameters)
        {
            return base.IsMetadataListener(bindingParameters) || bindingParameters.Find<HostedMetadataBindingParameter>() != null;
        }

        public override void IncrementBusyCount()
        {
            HostingEnvironmentWrapper.IncrementBusyCount();
        }

        public override void DecrementBusyCount()
        {
            HostingEnvironmentWrapper.DecrementBusyCount();
        }
        
        public override bool TraceIncrementBusyCountIsEnabled()
        {
            return TD.IncrementBusyCountIsEnabled();
        }

        public override bool TraceDecrementBusyCountIsEnabled()
        {
            return TD.DecrementBusyCountIsEnabled();
        }
        public override void TraceIncrementBusyCount(string data)
        {
            if (data == null)
            {
                data = SR.DefaultBusyCountSource;
            }
            TD.IncrementBusyCount(data);
        }

        public override void TraceDecrementBusyCount(string data)
        {
            if (data == null)
            {
                data = SR.DefaultBusyCountSource;
            }
            TD.DecrementBusyCount(data);
        }

        public override object GetConfigurationSection(string sectionPath)
        {
            return GetSectionFromWebConfigurationManager(sectionPath, ServiceHostingEnvironment.FullVirtualPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical method UnsafeGetSectionFromWebConfigurationManager which elevates.")]
        [SecurityCritical]
        public override object UnsafeGetConfigurationSection(string sectionPath)
        {
            return UnsafeGetSectionFromWebConfigurationManager(sectionPath, ServiceHostingEnvironment.FullVirtualPath);
        }

        public override bool IsSimpleApplicationHost
        {
            get
            {
                return ServiceHostingEnvironment.IsSimpleApplicationHost;
            }
        }

        public override AuthenticationSchemes GetAuthenticationSchemes(Uri baseAddress)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            string fileName = VirtualPathUtility.GetFileName(baseAddress.AbsolutePath);
            string virtualPath = ServiceHostingEnvironment.CurrentVirtualPath;
            string completePath;
            if (virtualPath != null && virtualPath.EndsWith("/", StringComparison.Ordinal))
            {
                completePath = virtualPath + fileName;
            }
            else
            {
                completePath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", virtualPath, fileName);
            }
            AuthenticationSchemes supportedSchemes = HostedTransportConfigurationManager.MetabaseSettings.GetAuthenticationSchemes(completePath);

            return supportedSchemes;
        }

        [Fx.Tag.SecurityNote(Critical = "Handles config objects, which should not be leaked.",
             Safe = "Doesn't leak config objects out of SecurityCritical code.")]
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override bool IsWindowsAuthenticationConfigured()
        {
            if (!this.isWindowsAuthentication.HasValue)
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

                AuthenticationSection authSection = (AuthenticationSection)UnsafeGetConfigurationSection("system.web/authentication");
                if (authSection != null)
                {
                    this.isWindowsAuthentication = (authSection.Mode == AuthenticationMode.Windows);
                }
                else
                {
                    this.isWindowsAuthentication = false;
                }
            }

            return this.isWindowsAuthentication.Value;
        }

        /// Be sure to update UnsafeGetSectionFromWebConfigurationManager if you modify this method
        [MethodImpl(MethodImplOptions.NoInlining)]
        static object GetSectionFromWebConfigurationManager(string sectionPath, string virtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            if (virtualPath != null)
            {
                return WebConfigurationManager.GetSection(sectionPath, virtualPath);
            }
            else
            {
                return WebConfigurationManager.GetSection(sectionPath);
            }
        }

        // Be sure to update GetSectionFromWebConfigurationManager if you modify this method
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts, Justification = "This is from an internal helper class and users have no way to pass arbitrary information to this code.")]
        [Fx.Tag.SecurityNote(Critical = "Asserts ConfigurationPermission in order to fetch config from WebConfigurationManager,"
            + "caller must guard return value.")]
        [SecurityCritical]
        [ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static object UnsafeGetSectionFromWebConfigurationManager(string sectionPath, string virtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            if (virtualPath != null)
            {
                return WebConfigurationManager.GetSection(sectionPath, virtualPath);
            }
            else
            {
                return WebConfigurationManager.GetSection(sectionPath);
            }
        }

        public override bool IsWithinApp(string absoluteVirtualPath)
        {
            return HostedTransportConfigurationManager.MetabaseSettings.IsWithinApp(absoluteVirtualPath);
        }

        // This class is intended to be empty.
        class HostedMetadataBindingParameter
        {
        }

        class HostedMetadataExchangeEndpointBehavior : IEndpointBehavior
        {
            void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
                bindingParameters.Add(new HostedMetadataBindingParameter());
            }

            void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, Dispatcher.ClientRuntime clientRuntime)
            {
            }

            void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, Dispatcher.EndpointDispatcher endpointDispatcher)
            {
            }

            void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
            {
            }
        }

        class ReceiveContextBusyCountWrapper : ReceiveContext
        {
            ReceiveContext wrappedContext;
            //possible values are 0 and 1.
            //using an integer to allow usage with Interlocked methods
            //synchronized access needed as there could be ---- between calls
            //to EndComplete and Tx notification.
            int busyCount;
            //possible values are 0 and 1
            //using an integer to allow usage with Interlocked methods
            //synchronized access needed as there could be ---- between calls
            //to EndComplete and Tx Status notification.
            int ambientTransactionCount;

            internal ReceiveContextBusyCountWrapper(ReceiveContext context)
            {
                this.wrappedContext = context;
                this.wrappedContext.Faulted += new EventHandler(OnWrappedContextFaulted);
                AspNetEnvironment.Current.IncrementBusyCount();
                if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceIncrementBusyCount(this.GetType().FullName);
                }
                Interlocked.Increment(ref busyCount);
            }

            protected override void OnAbandon(TimeSpan timeout)
            {
                this.wrappedContext.Abandon(timeout);
                DecrementBusyCount();
            }

            protected override IAsyncResult OnBeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.wrappedContext.BeginAbandon(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginComplete(TimeSpan timeout, AsyncCallback callback, object state)
            {
                RegisterForTransactionNotification(Transaction.Current);
                return this.wrappedContext.BeginComplete(timeout, callback, state);
            }

            protected override void OnComplete(TimeSpan timeout)
            {
                RegisterForTransactionNotification(Transaction.Current);
                this.wrappedContext.Complete(timeout);
                DecrementOnNoAmbientTransaction();

            }

            protected override void OnEndAbandon(IAsyncResult result)
            {
                this.wrappedContext.EndAbandon(result);
                DecrementBusyCount();
            }

            protected override void OnEndComplete(IAsyncResult result)
            {
                this.wrappedContext.EndComplete(result);
                DecrementOnNoAmbientTransaction();
            }

            protected override void OnFaulted()
            {
                try
                {
                    this.wrappedContext.Fault();
                }
                finally
                {
                    base.OnFaulted();
                }
            }

            void OnWrappedContextFaulted(object sender, EventArgs e)
            {
                try
                {
                    Fault();
                }
                finally
                {
                    DecrementBusyCount();
                }
            }

            void RegisterForTransactionNotification(Transaction transaction)
            {
                if (Transaction.Current != null)
                {
                    ReceiveContextEnlistmentNotification notification = new ReceiveContextEnlistmentNotification(this);
                    transaction.EnlistVolatile(notification, EnlistmentOptions.None);
                    Interlocked.Increment(ref this.ambientTransactionCount);
                }
            }

            void DecrementOnNoAmbientTransaction()
            {
                if (Interlocked.Exchange(ref this.ambientTransactionCount, 0) != 1)
                {
                    DecrementBusyCount();
                }

            }

            void DecrementBusyCount()
            {
                if (Interlocked.Exchange(ref this.busyCount, 0) == 1)
                {
                    AspNetEnvironment.Current.DecrementBusyCount();
                    if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                    {
                        AspNetEnvironment.Current.TraceDecrementBusyCount(this.GetType().FullName);
                    }
                }
            }

            class ReceiveContextEnlistmentNotification : IEnlistmentNotification
            {
                ReceiveContextBusyCountWrapper context;

                internal ReceiveContextEnlistmentNotification(ReceiveContextBusyCountWrapper context)
                {
                    this.context = context;
                }

                public void Commit(Enlistment enlistment)
                {
                    this.context.DecrementBusyCount();
                    enlistment.Done();
                }

                public void InDoubt(Enlistment enlistment)
                {
                    this.context.DecrementBusyCount();
                    enlistment.Done();
                }

                public void Prepare(PreparingEnlistment preparingEnlistment)
                {
                    preparingEnlistment.Prepared();
                }

                public void Rollback(Enlistment enlistment)
                {
                    enlistment.Done();
                }
            }
        }
    }
}
