//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Discovery.Version11;
    using System.ServiceModel.Discovery.VersionApril2005;
    using System.ServiceModel.Discovery.VersionCD1;
    using System.Xml;

    [Fx.Tag.XamlVisible(false)]
    public abstract class DiscoveryProxy :
        IAnnouncementContractApril2005,
        IAnnouncementContract11,
        IAnnouncementContractCD1,
        IDiscoveryContractAdhocApril2005,
        IDiscoveryContractManagedApril2005,
        IDiscoveryContractAdhoc11,
        IDiscoveryContractManaged11,
        IDiscoveryContractAdhocCD1,
        IDiscoveryContractManagedCD1,
        IAnnouncementServiceImplementation,
        IDiscoveryServiceImplementation,
        IMulticastSuppressionImplementation
    {
        DiscoveryMessageSequenceGenerator messageSequenceGenerator;
        DuplicateDetector<UniqueId> duplicateDetector;

        protected DiscoveryProxy()
            : this(new DiscoveryMessageSequenceGenerator())
        {
        }

        protected DiscoveryProxy(DiscoveryMessageSequenceGenerator messageSequenceGenerator)
            : this(messageSequenceGenerator, DiscoveryDefaults.DuplicateMessageHistoryLength)
        {
        }

        protected DiscoveryProxy(
            DiscoveryMessageSequenceGenerator messageSequenceGenerator,
            int duplicateMessageHistoryLength)
        {
            if (messageSequenceGenerator == null)
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
            this.messageSequenceGenerator = messageSequenceGenerator;
        }

        void IAnnouncementContractApril2005.HelloOperation(HelloMessageApril2005 message)
        {
            Fx.Assert("The [....] method IAnnouncementContractApril2005.HelloOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContractApril2005.BeginHelloOperation(HelloMessageApril2005 message, AsyncCallback callback, object state)
        {
            return new HelloOperationApril2005AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContractApril2005.EndHelloOperation(IAsyncResult result)
        {
            HelloOperationApril2005AsyncResult.End(result);
        }

        void IAnnouncementContractApril2005.ByeOperation(ByeMessageApril2005 message)
        {
            Fx.Assert("The [....] method IAnnouncementContractApril2005.ByeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContractApril2005.BeginByeOperation(ByeMessageApril2005 message, AsyncCallback callback, object state)
        {
            return new ByeOperationApril2005AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContractApril2005.EndByeOperation(IAsyncResult result)
        {
            ByeOperationApril2005AsyncResult.End(result);
        }

        void IAnnouncementContract11.HelloOperation(HelloMessage11 message)
        {
            Fx.Assert("The [....] method IAnnouncementContract11.HelloOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContract11.BeginHelloOperation(HelloMessage11 message, AsyncCallback callback, object state)
        {
            return new HelloOperation11AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContract11.EndHelloOperation(IAsyncResult result)
        {
            HelloOperation11AsyncResult.End(result);
        }

        void IAnnouncementContract11.ByeOperation(ByeMessage11 message)
        {
            Fx.Assert("The [....] method IAnnouncementContract11.ByeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContract11.BeginByeOperation(ByeMessage11 message, AsyncCallback callback, object state)
        {
            return new ByeOperation11AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContract11.EndByeOperation(IAsyncResult result)
        {
            ByeOperation11AsyncResult.End(result);
        }

        void IAnnouncementContractCD1.HelloOperation(HelloMessageCD1 message)
        {
            Fx.Assert("The [....] method IAnnouncementContractCD1.HelloOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContractCD1.BeginHelloOperation(HelloMessageCD1 message, AsyncCallback callback, object state)
        {
            return new HelloOperationCD1AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContractCD1.EndHelloOperation(IAsyncResult result)
        {
            HelloOperationCD1AsyncResult.End(result);
        }

        void IAnnouncementContractCD1.ByeOperation(ByeMessageCD1 message)
        {
            Fx.Assert("The [....] method IAnnouncementContractCD1.ByeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IAnnouncementContractCD1.BeginByeOperation(ByeMessageCD1 message, AsyncCallback callback, object state)
        {
            return new ByeOperationCD1AsyncResult(this, message, callback, state);
        }

        void IAnnouncementContractCD1.EndByeOperation(IAsyncResult result)
        {
            ByeOperationCD1AsyncResult.End(result);
        }

        void IDiscoveryContractApril2005.ProbeOperation(ProbeMessageApril2005 request)
        {
            Fx.Assert("The [....] method IDiscoveryContractApril2005.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractApril2005.BeginProbeOperation(ProbeMessageApril2005 request, AsyncCallback callback, object state)
        {
            return new ProbeDuplexApril2005AsyncResult(request, this, this, callback, state);
        }

        void IDiscoveryContractApril2005.EndProbeOperation(IAsyncResult result)
        {
            ProbeDuplexApril2005AsyncResult.End(result);
        }

        void IDiscoveryContractApril2005.ResolveOperation(ResolveMessageApril2005 request)
        {
            Fx.Assert("The [....] method IDiscoveryContractApril2005.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractApril2005.BeginResolveOperation(ResolveMessageApril2005 request, AsyncCallback callback, object state)
        {
            return new ResolveDuplexApril2005AsyncResult(request, this, this, callback, state);
        }

        void IDiscoveryContractApril2005.EndResolveOperation(IAsyncResult result)
        {
            ResolveDuplexApril2005AsyncResult.End(result);
        }

        void IDiscoveryContractAdhoc11.ProbeOperation(ProbeMessage11 request)
        {
            Fx.Assert("The [....] method IDiscoveryContractAdhoc11.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractAdhoc11.BeginProbeOperation(ProbeMessage11 request, AsyncCallback callback, object state)
        {
            return new ProbeDuplex11AsyncResult(request, this, this, callback, state);
        }

        void IDiscoveryContractAdhoc11.EndProbeOperation(IAsyncResult result)
        {
            ProbeDuplex11AsyncResult.End(result);
        }

        void IDiscoveryContractAdhoc11.ResolveOperation(ResolveMessage11 request)
        {
            Fx.Assert("The [....] method IDiscoveryContractAdhoc11.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractAdhoc11.BeginResolveOperation(ResolveMessage11 request, AsyncCallback callback, object state)
        {
            return new ResolveDuplex11AsyncResult(request, this, this, callback, state);
        }

        void IDiscoveryContractAdhoc11.EndResolveOperation(IAsyncResult result)
        {
            ResolveDuplex11AsyncResult.End(result);
        }

        ProbeMatchesMessage11 IDiscoveryContractManaged11.ProbeOperation(ProbeMessage11 request)
        {
            Fx.Assert("The [....] method IDiscoveryContractManaged11.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
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
            Fx.Assert("The [....] method IDiscoveryContractManaged11.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
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
            Fx.Assert("The [....] method IDiscoveryContractAdhocCD1.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractAdhocCD1.BeginProbeOperation(ProbeMessageCD1 request, AsyncCallback callback, object state)
        {
            return new ProbeDuplexCD1AsyncResult(request, this, this, callback, state);
        }

        void IDiscoveryContractAdhocCD1.EndProbeOperation(IAsyncResult result)
        {
            ProbeDuplexCD1AsyncResult.End(result);
        }

        void IDiscoveryContractAdhocCD1.ResolveOperation(ResolveMessageCD1 request)
        {
            Fx.Assert("The [....] method IDiscoveryContractAdhocCD1.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
        }

        IAsyncResult IDiscoveryContractAdhocCD1.BeginResolveOperation(ResolveMessageCD1 request, AsyncCallback callback, object state)
        {
            return new ResolveDuplexCD1AsyncResult(request, this, this, callback, state);
        }

        void IDiscoveryContractAdhocCD1.EndResolveOperation(IAsyncResult result)
        {
            ResolveDuplexCD1AsyncResult.End(result);
        }

        ProbeMatchesMessageCD1 IDiscoveryContractManagedCD1.ProbeOperation(ProbeMessageCD1 request)
        {
            Fx.Assert("The [....] method IDiscoveryContractManagedCD1.ProbeOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
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
            Fx.Assert("The [....] method IDiscoveryContractManagedCD1.ResolveOperation must not get invoked. It is marked with PreferAsyncInvocation flag.");
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

        bool IAnnouncementServiceImplementation.IsDuplicate(UniqueId messageId)
        {
            return (this.duplicateDetector != null) && (!this.duplicateDetector.AddIfNotDuplicate(messageId));
        }

        IAsyncResult IAnnouncementServiceImplementation.OnBeginOnlineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {
            return this.OnBeginOnlineAnnouncement(messageSequence, endpointDiscoveryMetadata, callback, state);
        }

        void IAnnouncementServiceImplementation.OnEndOnlineAnnouncement(IAsyncResult result)
        {
            this.OnEndOnlineAnnouncement(result);
        }

        IAsyncResult IAnnouncementServiceImplementation.OnBeginOfflineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {
            return this.OnBeginOfflineAnnouncement(messageSequence, endpointDiscoveryMetadata, callback, state);
        }

        void IAnnouncementServiceImplementation.OnEndOfflineAnnouncement(IAsyncResult result)
        {
            this.OnEndOfflineAnnouncement(result);
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

        IAsyncResult IMulticastSuppressionImplementation.BeginShouldRedirectFind(FindCriteria findCriteria, AsyncCallback callback, object state)
        {
            return this.BeginShouldRedirectFind(findCriteria, callback, state);
        }

        bool IMulticastSuppressionImplementation.EndShouldRedirectFind(IAsyncResult result, out Collection<EndpointDiscoveryMetadata> redirectionEndpoints)
        {
            return this.EndShouldRedirectFind(result, out redirectionEndpoints);
        }

        IAsyncResult IMulticastSuppressionImplementation.BeginShouldRedirectResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
        {
            return this.BeginShouldRedirectResolve(resolveCriteria, callback, state);
        }

        bool IMulticastSuppressionImplementation.EndShouldRedirectResolve(IAsyncResult result, out Collection<EndpointDiscoveryMetadata> redirectionEndpoints)
        {
            return this.EndShouldRedirectResolve(result, out redirectionEndpoints);
        }

        protected virtual IAsyncResult BeginShouldRedirectFind(FindCriteria resolveCriteria, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<bool>(false, callback, state);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, Justification = "This is a Try pattern that requires out parameter.")]
        protected virtual bool EndShouldRedirectFind(IAsyncResult result, out Collection<EndpointDiscoveryMetadata> redirectionEndpoints)
        {
            redirectionEndpoints = null;
            return CompletedAsyncResult<bool>.End(result);
        }

        protected virtual IAsyncResult BeginShouldRedirectResolve(ResolveCriteria findCriteria, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<bool>(false, callback, state);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, Justification = "This is a Try pattern that requires out parameter.")]
        protected virtual bool EndShouldRedirectResolve(IAsyncResult result, out Collection<EndpointDiscoveryMetadata> redirectionEndpoints)
        {
            redirectionEndpoints = null;
            return CompletedAsyncResult<bool>.End(result);
        }


        protected abstract IAsyncResult OnBeginOnlineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state);
        protected abstract void OnEndOnlineAnnouncement(IAsyncResult result);

        protected abstract IAsyncResult OnBeginOfflineAnnouncement(
            DiscoveryMessageSequence messageSequence,
            EndpointDiscoveryMetadata endpointDiscoveryMetadata,
            AsyncCallback callback,
            object state);
        protected abstract void OnEndOfflineAnnouncement(IAsyncResult result);

        protected abstract IAsyncResult OnBeginFind(FindRequestContext findRequestContext, AsyncCallback callback, object state);
        protected abstract void OnEndFind(IAsyncResult result);

        protected abstract IAsyncResult OnBeginResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state);
        protected abstract EndpointDiscoveryMetadata OnEndResolve(IAsyncResult result);
    }
}
