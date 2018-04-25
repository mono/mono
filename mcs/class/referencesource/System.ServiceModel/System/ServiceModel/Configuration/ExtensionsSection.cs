//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    public partial class ExtensionsSection : ConfigurationSection
    {
        [ConfigurationProperty(ConfigurationStrings.BehaviorExtensions)]
        public ExtensionElementCollection BehaviorExtensions
        {
            get { return (ExtensionElementCollection)base[ConfigurationStrings.BehaviorExtensions]; }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingElementExtensions)]
        public ExtensionElementCollection BindingElementExtensions
        {
            get { return (ExtensionElementCollection)base[ConfigurationStrings.BindingElementExtensions]; }
        }

        [ConfigurationProperty(ConfigurationStrings.BindingExtensions)]
        public ExtensionElementCollection BindingExtensions
        {
            get { return (ExtensionElementCollection)base[ConfigurationStrings.BindingExtensions]; }
        }

        [ConfigurationProperty(ConfigurationStrings.EndpointExtensions)]
        public ExtensionElementCollection EndpointExtensions
        {
            get { return (ExtensionElementCollection)base[ConfigurationStrings.EndpointExtensions]; }
        }

        void InitializeBehaviorElementExtensions()
        {
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ClientCredentials, typeof(ClientCredentialsElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ServiceCredentials, typeof(ServiceCredentialsElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.CallbackDebugSectionName, typeof(CallbackDebugElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ClientViaSectionName, typeof(ClientViaElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.SynchronousReceiveSectionName, typeof(SynchronousReceiveElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.DispatcherSynchronizationSectionName, typeof(DispatcherSynchronizationElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ServiceMetadataPublishingSectionName, typeof(ServiceMetadataPublishingElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ServiceDebugSectionName, typeof(ServiceDebugElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ServiceAuthenticationManagerSectionName, typeof(ServiceAuthenticationElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ServiceAuthorizationSectionName, typeof(ServiceAuthorizationElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ServiceSecurityAuditSectionName, typeof(ServiceSecurityAuditElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ServiceThrottlingSectionName, typeof(ServiceThrottlingElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.TransactedBatchingSectionName, typeof(TransactedBatchingElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.DataContractSerializerSectionName, typeof(DataContractSerializerElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.ServiceTimeouts, typeof(ServiceTimeoutsElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.CallbackTimeouts, typeof(CallbackTimeoutsElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.UseRequestHeadersForMetadataAddress, typeof(UseRequestHeadersForMetadataAddressElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.Clear, typeof(ClearBehaviorElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement(ConfigurationStrings.Remove, typeof(RemoveBehaviorElement).AssemblyQualifiedName));
        }

        void InitializeBindingElementExtenions()
        {
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.BinaryMessageEncodingSectionName, typeof(BinaryMessageEncodingElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.CompositeDuplexSectionName, typeof(CompositeDuplexElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.OneWaySectionName, typeof(OneWayElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.TransactionFlowSectionName, typeof(TransactionFlowElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.HttpsTransportSectionName, typeof(HttpsTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.HttpTransportSectionName, typeof(HttpTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.MsmqIntegrationSectionName, typeof(MsmqIntegrationElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.MsmqTransportSectionName, typeof(MsmqTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.MtomMessageEncodingSectionName, typeof(MtomMessageEncodingElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.NamedPipeTransportSectionName, typeof(NamedPipeTransportElement).AssemblyQualifiedName));
#pragma warning disable 0618
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.PeerTransportSectionName, typeof(PeerTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.PnrpPeerResolverSectionName, typeof(PnrpPeerResolverElement).AssemblyQualifiedName));
#pragma warning restore 0618
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.PrivacyNoticeSectionName, typeof(PrivacyNoticeElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.ReliableSessionSectionName, typeof(ReliableSessionElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.SecuritySectionName, typeof(SecurityElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.SslStreamSecuritySectionName, typeof(SslStreamSecurityElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.TcpTransportSectionName, typeof(TcpTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.TextMessageEncodingSectionName, typeof(TextMessageEncodingElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.UnrecognizedPolicyAssertionSectionName, typeof(UnrecognizedPolicyAssertionElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.UseManagedPresentationSectionName, typeof(UseManagedPresentationElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.WindowsStreamSecuritySectionName, typeof(WindowsStreamSecurityElement).AssemblyQualifiedName));

            if (OSEnvironmentHelper.IsApplicationTargeting45)
            {
                this.BindingElementExtensions.Add(new ExtensionElement(ConfigurationStrings.UdpTransportSectionName, ConfigurationStrings.UdpTransportElementType));
            }
        }

        void InitializeBindingExtensions()
        {
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.BasicHttpBindingCollectionElementName, typeof(BasicHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.CustomBindingCollectionElementName, typeof(CustomBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.MsmqIntegrationBindingCollectionElementName, typeof(MsmqIntegrationBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.NetMsmqBindingCollectionElementName, typeof(NetMsmqBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.NetNamedPipeBindingCollectionElementName, typeof(NetNamedPipeBindingCollectionElement).AssemblyQualifiedName));
#pragma warning disable 0618
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.NetPeerTcpBindingCollectionElementName, typeof(NetPeerTcpBindingCollectionElement).AssemblyQualifiedName));
#pragma warning restore 0618
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.NetTcpBindingCollectionElementName, typeof(NetTcpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.WSDualHttpBindingCollectionElementName, typeof(WSDualHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.WSFederationHttpBindingCollectionElementName, typeof(WSFederationHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.WS2007FederationHttpBindingCollectionElementName, typeof(WS2007FederationHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.WSHttpBindingCollectionElementName, typeof(WSHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.WS2007HttpBindingCollectionElementName, typeof(WS2007HttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.MexHttpBindingCollectionElementName, typeof(MexHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.MexHttpsBindingCollectionElementName, typeof(MexHttpsBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.MexNamedPipeBindingCollectionElementName, typeof(MexNamedPipeBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.MexTcpBindingCollectionElementName, typeof(MexTcpBindingCollectionElement).AssemblyQualifiedName));

            if (OSEnvironmentHelper.IsApplicationTargeting45)
            {
                this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.UdpBindingCollectionElementName, ConfigurationStrings.UdpBindingCollectionElementType));
                this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.NetHttpBindingCollectionElementName, typeof(NetHttpBindingCollectionElement).AssemblyQualifiedName));
                this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.NetHttpsBindingCollectionElementName, typeof(NetHttpsBindingCollectionElement).AssemblyQualifiedName));
                this.BindingExtensions.Add(new ExtensionElement(ConfigurationStrings.BasicHttpsBindingCollectionElementName, typeof(BasicHttpsBindingCollectionElement).AssemblyQualifiedName));
            }
        }

        void InitializeEndpointExtensions()
        {
            this.EndpointExtensions.Add(new ExtensionElement(ConfigurationStrings.MexStandardEndpointCollectionElementName, typeof(ServiceMetadataEndpointCollectionElement).AssemblyQualifiedName));
        }

        protected override void InitializeDefault()
        {
            this.InitializeBehaviorElementExtensions();
            this.InitializeBindingElementExtenions();
            this.InitializeBindingExtensions();
            this.InitializeEndpointExtensions();
        }

        // Be sure to update UnsafeLookupAssociatedCollection if you modify this method
        internal static ExtensionElementCollection LookupAssociatedCollection(Type extensionType, ContextInformation evaluationContext, out string collectionName)
        {
            collectionName = GetExtensionType(extensionType);
            return ExtensionsSection.LookupCollection(collectionName, evaluationContext);
        }

        // Be sure to update LookupAssociatedCollection if you modify this method
        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeLookupCollection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static ExtensionElementCollection UnsafeLookupAssociatedCollection(Type extensionType, ContextInformation evaluationContext, out string collectionName)
        {
            collectionName = GetExtensionType(extensionType);
            return ExtensionsSection.UnsafeLookupCollection(collectionName, evaluationContext);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview.")]
        static string GetExtensionType(Type extensionType)
        {
            string collectionName = String.Empty;

            if (extensionType.IsSubclassOf(typeof(BehaviorExtensionElement)))
            {
                collectionName = ConfigurationStrings.BehaviorExtensions;
            }
            else if (extensionType.IsSubclassOf(typeof(BindingElementExtensionElement)))
            {
                collectionName = ConfigurationStrings.BindingElementExtensions;
            }
            else if (extensionType.IsSubclassOf(typeof(BindingCollectionElement)))
            {
                collectionName = ConfigurationStrings.BindingExtensions;
            }
            else if (extensionType.IsSubclassOf(typeof(EndpointCollectionElement)))
            {
                collectionName = ConfigurationStrings.EndpointExtensions;
            }
            else
            {
                // LookupAssociatedCollection built on assumption that extensionType is valid.
                // This should be protected at the callers site.  If assumption is invalid, then
                // configuration system is in an indeterminate state.  Need to stop in a manner that
                // user code can not capture.
                Fx.Assert(String.Format(CultureInfo.InvariantCulture, "{0} is not a type supported by the ServiceModelExtensionsSection collections.", extensionType.AssemblyQualifiedName));
                DiagnosticUtility.FailFast(String.Format(CultureInfo.InvariantCulture, "{0} is not a type supported by the ServiceModelExtensionsSection collections.", extensionType.AssemblyQualifiedName));
            }

            return collectionName;
        }

        /// Be sure to update UnsafeLookupCollection if you modify this method
        internal static ExtensionElementCollection LookupCollection(string collectionName, ContextInformation evaluationContext)
        {
            ExtensionElementCollection collection = null;
            ExtensionsSection extensionsSection = null;

            if (null != evaluationContext)
            {
                extensionsSection = (ExtensionsSection)ConfigurationHelpers.GetAssociatedSection(evaluationContext, ConfigurationStrings.ExtensionsSectionPath);
            }
            else
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                        TraceCode.EvaluationContextNotFound,
                        SR.GetString(SR.TraceCodeEvaluationContextNotFound),
                        null,
                        (Exception)null);
                }

                extensionsSection = (ExtensionsSection)ConfigurationHelpers.GetSection(ConfigurationStrings.ExtensionsSectionPath);
            }

            switch (collectionName)
            {
                case (ConfigurationStrings.BehaviorExtensions):
                    collection = extensionsSection.BehaviorExtensions;
                    break;
                case (ConfigurationStrings.BindingElementExtensions):
                    collection = extensionsSection.BindingElementExtensions;
                    break;
                case (ConfigurationStrings.BindingExtensions):
                    collection = extensionsSection.BindingExtensions;
                    break;
                case (ConfigurationStrings.EndpointExtensions):
                    collection = extensionsSection.EndpointExtensions;
                    break;
                default:
                    // LookupCollection built on assumption that collectionName is valid.
                    // This should be protected at the callers site.  If assumption is invalid, then
                    // configuration system is in an indeterminate state.  Need to stop in a manner that
                    // user code can not capture.
                    Fx.Assert(String.Format(CultureInfo.InvariantCulture, "{0} is not a valid ServiceModelExtensionsSection collection name.", collectionName));
                    DiagnosticUtility.FailFast(String.Format(CultureInfo.InvariantCulture, "{0} is not a valid ServiceModelExtensionsSection collection name.", collectionName));
                    break;
            }

            return collection;
        }

        // Be sure to update LookupCollection if you modify this method
        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetAssociatedSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static ExtensionElementCollection UnsafeLookupCollection(string collectionName, ContextInformation evaluationContext)
        {
            ExtensionElementCollection collection = null;
            ExtensionsSection extensionsSection = null;

            if (null != evaluationContext)
            {
                extensionsSection = (ExtensionsSection)ConfigurationHelpers.UnsafeGetAssociatedSection(evaluationContext, ConfigurationStrings.ExtensionsSectionPath);
            }
            else
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                        TraceCode.EvaluationContextNotFound,
                        SR.GetString(SR.TraceCodeEvaluationContextNotFound),
                        null,
                        (Exception)null);
                }

                extensionsSection = (ExtensionsSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ExtensionsSectionPath);
            }

            switch (collectionName)
            {
                case (ConfigurationStrings.BehaviorExtensions):
                    collection = extensionsSection.BehaviorExtensions;
                    break;
                case (ConfigurationStrings.BindingElementExtensions):
                    collection = extensionsSection.BindingElementExtensions;
                    break;
                case (ConfigurationStrings.BindingExtensions):
                    collection = extensionsSection.BindingExtensions;
                    break;
                case (ConfigurationStrings.EndpointExtensions):
                    collection = extensionsSection.EndpointExtensions;
                    break;
                default:
                    // LookupCollection built on assumption that collectionName is valid.
                    // This should be protected at the callers site.  If assumption is invalid, then
                    // configuration system is in an indeterminate state.  Need to stop in a manner that
                    // user code can not capture.
                    Fx.Assert(String.Format(CultureInfo.InvariantCulture, "{0} is not a valid ServiceModelExtensionsSection collection name.", collectionName));
                    DiagnosticUtility.FailFast(String.Format(CultureInfo.InvariantCulture, "{0} is not a valid ServiceModelExtensionsSection collection name.", collectionName));
                    break;
            }

            return collection;
        }
    }
}
