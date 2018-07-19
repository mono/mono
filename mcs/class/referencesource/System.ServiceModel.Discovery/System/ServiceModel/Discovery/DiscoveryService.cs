//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Discovery.Version11;
    using System.ServiceModel.Discovery.VersionApril2005;
    using System.ServiceModel.Discovery.VersionCD1;
    using System.Xml;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

    [Fx.Tag.XamlVisible(false)]
    public abstract class DiscoveryService :
        IDiscoveryContractAdhocApril2005,
        IDiscoveryContractManagedApril2005,
        IDiscoveryContractAdhoc11,
        IDiscoveryContractManaged11,
        IDiscoveryContractAdhocCD1,
        IDiscoveryContractManagedCD1,
        IDiscoveryServiceImplementation
    {
        DiscoveryMessageSequenceGenerator messageSequenceGenerator;
        DuplicateDetector<UniqueId> duplicateDetector;

        protected DiscoveryService()
            : this(new DiscoveryMessageSequenceGenerator())
        {
        }

        protected DiscoveryService(DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator)
            : this(discoveryMessageSequenceGenerator, DiscoveryDefaults.DuplicateMessageHistoryLength)
        {
        }

        protected DiscoveryService(
            DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator,
            int duplicateMessageHistoryLength)
        {
            if (discoveryMessageSequenceGenerator == null)
            {
                throw FxTrace.Exception.ArgumentNull("messageSequenceGenerator");
            }
            if (duplicateMessageHistoryLength < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange(
                    "duplicateMessageHistoryLength",
                    duplicateMessageHistoryLength,
                    SR.DiscoveryNegativeDuplicateMessageHistoryLength);
            }
            if (duplicateMessageHistoryLength > 0)
            {
                this.duplicateDetector = new DuplicateDetector<UniqueId>(duplicateMessageHistoryLength);
            }
            this.messageSequenceGenerator = discoveryMessageSequenceGenerator;
        }

        internal DiscoveryMessageSequenceGenerator MessageSequenceGenerator
        {
            get
            {
                return this.messageSequenceGenerator;
            }
        }

        void IDiscoveryContractApril2005.ProbeOperation(ProbeMessageApril2005 request)
        {
            Fx.Assert("The sync method IDiscoveryContractApril2005.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractApril2005.BeginProbeOperation(ProbeMessageApril2005 request, AsyncCallback callback, object state)
        {
            return new ProbeDuplexApril2005AsyncResult(request, this, null, callback, state);
        }

        void IDiscoveryContractApril2005.EndProbeOperation(IAsyncResult result)
        {
            ProbeDuplexApril2005AsyncResult.End(result);
        }

        void IDiscoveryContractApril2005.ResolveOperation(ResolveMessageApril2005 request)
        {
            Fx.Assert("The sync method IDiscoveryContractApril2005.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractApril2005.BeginResolveOperation(ResolveMessageApril2005 request, AsyncCallback callback, object state)
        {
            return new ResolveDuplexApril2005AsyncResult(request, this, null, callback, state);
        }

        void IDiscoveryContractApril2005.EndResolveOperation(IAsyncResult result)
        {
            ResolveDuplexApril2005AsyncResult.End(result);
        }

        void IDiscoveryContractAdhoc11.ProbeOperation(ProbeMessage11 request)
        {
            Fx.Assert("The sync method IDiscoveryContractAdhoc11.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractAdhoc11.BeginProbeOperation(ProbeMessage11 request, AsyncCallback callback, object state)
        {
            return new ProbeDuplex11AsyncResult(request, this, null, callback, state);
        }

        void IDiscoveryContractAdhoc11.EndProbeOperation(IAsyncResult result)
        {
            ProbeDuplex11AsyncResult.End(result);
        }

        void IDiscoveryContractAdhoc11.ResolveOperation(ResolveMessage11 request)
        {
            Fx.Assert("The sync method IDiscoveryContractAdhoc11.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractAdhoc11.BeginResolveOperation(ResolveMessage11 request, AsyncCallback callback, object state)
        {
            return new ResolveDuplex11AsyncResult(request, this, null, callback, state);
        }

        void IDiscoveryContractAdhoc11.EndResolveOperation(IAsyncResult result)
        {
            ResolveDuplex11AsyncResult.End(result);
        }

        ProbeMatchesMessage11 IDiscoveryContractManaged11.ProbeOperation(ProbeMessage11 request)
        {
            Fx.Assert("The sync method IDiscoveryContractManaged11.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
            return null;
        }

        IAsyncResult IDiscoveryContractManaged11.BeginProbeOperation(ProbeMessage11 request, AsyncCallback callback, object state)
        {
            return new ProbeRequestResponse11AsyncResult(request, this, callback, state);
        }

        ProbeMatchesMessage11 IDiscoveryContractManaged11.EndProbeOperation(IAsyncResult result)
        {
            return ProbeRequestResponse11AsyncResult.End(result);
        }

        ResolveMatchesMessage11 IDiscoveryContractManaged11.ResolveOperation(ResolveMessage11 request)
        {
            Fx.Assert("The sync method IDiscoveryContractManaged11.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");

            return null;
        }

        IAsyncResult IDiscoveryContractManaged11.BeginResolveOperation(ResolveMessage11 request, AsyncCallback callback, object state)
        {
            return new ResolveRequestResponse11AsyncResult(request, this, callback, state);
        }

        ResolveMatchesMessage11 IDiscoveryContractManaged11.EndResolveOperation(IAsyncResult result)
        {
            return ResolveRequestResponse11AsyncResult.End(result);
        }

        void IDiscoveryContractAdhocCD1.ProbeOperation(ProbeMessageCD1 request)
        {
            Fx.Assert("The sync method IDiscoveryContractAdhocCD1.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractAdhocCD1.BeginProbeOperation(ProbeMessageCD1 request, AsyncCallback callback, object state)
        {
            return new ProbeDuplexCD1AsyncResult(request, this, null, callback, state);
        }

        void IDiscoveryContractAdhocCD1.EndProbeOperation(IAsyncResult result)
        {
            ProbeDuplexCD1AsyncResult.End(result);
        }

        void IDiscoveryContractAdhocCD1.ResolveOperation(ResolveMessageCD1 request)
        {
            Fx.Assert("The sync method IDiscoveryContractAdhocCD1.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractAdhocCD1.BeginResolveOperation(ResolveMessageCD1 request, AsyncCallback callback, object state)
        {
            return new ResolveDuplexCD1AsyncResult(request, this, null, callback, state);
        }

        void IDiscoveryContractAdhocCD1.EndResolveOperation(IAsyncResult result)
        {
            ResolveDuplexCD1AsyncResult.End(result);
        }

        ProbeMatchesMessageCD1 IDiscoveryContractManagedCD1.ProbeOperation(ProbeMessageCD1 request)
        {
            Fx.Assert("The sync method IDiscoveryContractManagedCD1.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
            return null;
        }

        IAsyncResult IDiscoveryContractManagedCD1.BeginProbeOperation(ProbeMessageCD1 request, AsyncCallback callback, object state)
        {
            return new ProbeRequestResponseCD1AsyncResult(request, this, callback, state);
        }

        ProbeMatchesMessageCD1 IDiscoveryContractManagedCD1.EndProbeOperation(IAsyncResult result)
        {
            return ProbeRequestResponseCD1AsyncResult.End(result);
        }

        ResolveMatchesMessageCD1 IDiscoveryContractManagedCD1.ResolveOperation(ResolveMessageCD1 request)
        {
            Fx.Assert("The sync method IDiscoveryContractManagedCD1.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
            return null;
        }

        IAsyncResult IDiscoveryContractManagedCD1.BeginResolveOperation(ResolveMessageCD1 request, AsyncCallback callback, object state)
        {
            return new ResolveRequestResponseCD1AsyncResult(request, this, callback, state);
        }

        ResolveMatchesMessageCD1 IDiscoveryContractManagedCD1.EndResolveOperation(IAsyncResult result)
        {
            return ResolveRequestResponseCD1AsyncResult.End(result);
        }

        bool IDiscoveryServiceImplementation.IsDuplicate(UniqueId messageId)
        {
            return (this.duplicateDetector != null) && (!this.duplicateDetector.AddIfNotDuplicate(messageId));
        }

        DiscoveryMessageSequence IDiscoveryServiceImplementation.GetNextMessageSequence()
        {
            return this.messageSequenceGenerator.Next();
        }

        IAsyncResult IDiscoveryServiceImplementation.BeginFind(FindRequestContext findRequestContext, AsyncCallback callback, object state)
        {
            return this.OnBeginFind(findRequestContext, callback, state);
        }

        void IDiscoveryServiceImplementation.EndFind(IAsyncResult result)
        {
            this.OnEndFind(result);
        }

        IAsyncResult IDiscoveryServiceImplementation.BeginResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
        {
            return this.OnBeginResolve(resolveCriteria, callback, state);
        }

        EndpointDiscoveryMetadata IDiscoveryServiceImplementation.EndResolve(IAsyncResult result)
        {
            return this.OnEndResolve(result);
        }

        internal static bool EnsureMessageId()
        {
            if (OperationContext.Current.IncomingMessageHeaders.MessageId == null)
            {
                if (TD.DiscoveryMessageWithNullMessageIdIsEnabled())
                {
                    TD.DiscoveryMessageWithNullMessageId(
                        null,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}/{1}",
                            ProtocolStrings.TracingStrings.Probe,
                            ProtocolStrings.TracingStrings.Resolve));
                }

                return false;
            }

            return true;
        }

        internal static bool EnsureReplyTo()
        {
            OperationContext context = OperationContext.Current;
            if (context.IncomingMessageHeaders.ReplyTo == null)
            {
                if (TD.DiscoveryMessageWithNullReplyToIsEnabled())
                {
                    EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(context.IncomingMessage);
                    TD.DiscoveryMessageWithNullReplyTo(eventTraceActivity, context.IncomingMessageHeaders.MessageId.ToString());
                }

                return false;
            }

            return true;
        }

        protected abstract IAsyncResult OnBeginFind(FindRequestContext findRequestContext, AsyncCallback callback, object state);
        protected abstract void OnEndFind(IAsyncResult result);

        protected abstract IAsyncResult OnBeginResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state);
        protected abstract EndpointDiscoveryMetadata OnEndResolve(IAsyncResult result);
    }
}
