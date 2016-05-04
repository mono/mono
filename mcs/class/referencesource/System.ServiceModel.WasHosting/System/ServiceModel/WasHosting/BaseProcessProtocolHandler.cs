//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.WasHosting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics; 
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.Web;
    using System.Web.Hosting;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Activation;
    using System.Runtime;

    abstract class BaseProcessProtocolHandler : ProcessProtocolHandler
    {
        string protocolId;
        IAdphManager adphManager;
        
        // Mapping from listenerChannelId->listenerChannelContext (i.e. IAdphManager, appId)
        Dictionary<int, ListenerChannelContext> listenerChannelIdMapping = new Dictionary<int, ListenerChannelContext>();

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "Instantiated by ASP.NET")]
        protected BaseProcessProtocolHandler(string protocolId)
            : base()
        {
            this.protocolId = protocolId;
        }

        internal virtual void HandleStartListenerChannelError(IListenerChannelCallback listenerChannelCallback, Exception ex)
        {
            // This is the workaround to let WAS know that the LC is started and then gracefully stopped.
            listenerChannelCallback.ReportStarted();
            listenerChannelCallback.ReportStopped(0);
        } 

        // Start per-process listening for messages
        public override void StartListenerChannel(IListenerChannelCallback listenerChannelCallback, IAdphManager adphManager)
        {
            DiagnosticUtility.DebugAssert(listenerChannelCallback != null, "The listenerChannelCallback parameter must not be null");
            DiagnosticUtility.DebugAssert(adphManager != null, "The adphManager parameter must not be null");

            int channelId = listenerChannelCallback.GetId();
            ListenerChannelContext listenerChannelContext;
            lock (this.listenerChannelIdMapping)
            {
                if (!listenerChannelIdMapping.TryGetValue(channelId, out listenerChannelContext))
                {
                    int listenerChannelDataLength = listenerChannelCallback.GetBlobLength();
                    byte[] listenerChannelData = new byte[listenerChannelDataLength];
                    listenerChannelCallback.GetBlob(listenerChannelData, ref listenerChannelDataLength);
                    Debug.Print("BaseProcessProtocolHandler.StartListenerChannel() GetBlob() contains " + listenerChannelDataLength + " bytes");
                    listenerChannelContext = ListenerChannelContext.Hydrate(listenerChannelData);
                    this.listenerChannelIdMapping.Add(channelId, listenerChannelContext);
                    Debug.Print("BaseProcessProtocolHandler.StartListenerChannel() listenerChannelContext.ListenerChannelId: " + listenerChannelContext.ListenerChannelId);
                }
            }

            if (this.adphManager == null)
            {
                this.adphManager = adphManager;
            }

            try
            {
                // wether or not a previous AppDomain was running, we're going to start a new one now:
                Debug.Print("BaseProcessProtocolHandler.StartListenerChannel() calling StartAppDomainProtocolListenerChannel(appKey:" + listenerChannelContext.AppKey + " protocolId:" + protocolId + ")");
                adphManager.StartAppDomainProtocolListenerChannel(listenerChannelContext.AppKey, protocolId, listenerChannelCallback);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                DiagnosticUtility.TraceHandledException(ex, TraceEventType.Error);

                HandleStartListenerChannelError(listenerChannelCallback, ex);
            }
        }

        public override void StopProtocol(bool immediate)
        {
            Debug.Print("BaseProcessProtocolHandler.StopProtocol(protocolId:" + protocolId + ", immediate:" + immediate + ")");
        }

        public override void StopListenerChannel(int listenerChannelId, bool immediate)
        {
            Debug.Print("BaseProcessProtocolHandler.StopListenerChannel(protocolId:" + protocolId + ", listenerChannelId:" + listenerChannelId + ", immediate:" + immediate + ")");
            ListenerChannelContext listenerChannelContext = this.listenerChannelIdMapping[listenerChannelId];
            adphManager.StopAppDomainProtocolListenerChannel(listenerChannelContext.AppKey, protocolId, listenerChannelId, immediate);

            lock (this.listenerChannelIdMapping)
            {
                // Remove the channel id.
                this.listenerChannelIdMapping.Remove(listenerChannelId);
            }
        }
    }
}
