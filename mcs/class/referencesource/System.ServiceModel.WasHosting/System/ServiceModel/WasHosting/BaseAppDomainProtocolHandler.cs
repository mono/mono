//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.WasHosting
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Web;
    using System.Web.Hosting;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;


    abstract class BaseAppDomainProtocolHandler : AppDomainProtocolHandler
    {
        public readonly static TimeSpan DefaultStopTimeout = TimeSpan.FromSeconds(30);

        string protocolId;
        IListenerChannelCallback listenerChannelCallback;
        protected ListenerChannelContext listenerChannelContext;
        object syncRoot = new object();

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "Instantiated by ASP.NET")]
        protected BaseAppDomainProtocolHandler(string protocolId)
            : base()
        {
            this.protocolId = protocolId;
        }

        object ThisLock
        {
            get
            {
                return this.syncRoot;
            }
        }

        protected void OnMessageReceived()
        {
            try
            {
                IListenerChannelCallback callback = this.listenerChannelCallback;
                if (callback != null)
                {
                    callback.ReportMessageReceived();
                }
            }
            catch (COMException exception)
            {
                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                // The listener adapter might have gone away. Ignore the error.
            }
        }

        // Start per-process listening for messages
        public override void StartListenerChannel(IListenerChannelCallback listenerChannelCallback)
        {
            Debug.Print("BaseAppDomainProtocolHandler.StartListenerChannel()");
            if (listenerChannelCallback == null)
            {
                DiagnosticUtility.DebugAssert("listenerChannelCallback is null");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInternal(false);
            }

            this.listenerChannelCallback = listenerChannelCallback;

            int listenerChannelDataLength = listenerChannelCallback.GetBlobLength();
            byte[] listenerChannelData = new byte[listenerChannelDataLength];
            listenerChannelCallback.GetBlob(listenerChannelData, ref listenerChannelDataLength);
            Debug.Print("BaseAppDomainProtocolHandler.StartListenerChannel() GetBlob() contains " + listenerChannelDataLength + " bytes");

            listenerChannelContext = ListenerChannelContext.Hydrate(listenerChannelData);

            Debug.Print("BaseAppDomainProtocolHandler.StartListenerChannel() calling OnStart()");
#if DEBUG
            // Debug.Print("BaseAppDomainProtocolHandler.StartListenerChannel() waiting for you to attach the debugger to " + Process.GetCurrentProcess().ProcessName + " Pid: " + Process.GetCurrentProcess().Id);
            // for (int sleepCount = 0; sleepCount < 30 && !Debugger.IsAttached && !ListenerUnsafeNativeMethods.IsDebuggerPresent(); sleepCount++) { Thread.Sleep(500); } Debugger.Break();
#endif
            try
            {
                OnStart();

                listenerChannelCallback.ReportStarted();
                Debug.Print("BaseAppDomainProtocolHandler.StartListenerChannel() called ReportStarted()");
            }
            catch (CommunicationException exception)
            {
                Debug.Print("BaseAppDomainProtocolHandler.StartListenerChannel() failed in OnStart():\r\n" + exception);
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.WebHost,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.WebHostFailedToListen,
                    listenerChannelContext.AppKey,
                    this.protocolId,
                    TraceUtility.CreateSourceString(this),
                    exception.ToString());

                throw;
            }
        }

        protected virtual void OnStart() { }
        protected virtual void OnStop() { }

        public override void StopProtocol(bool immediate)
        {
            Debug.Print("BaseAppDomainProtocolHandler.StopProtocol() immediate: " + immediate + " calling ReportStopped()");

            Stop();
            HostingEnvironment.UnregisterObject(this);
        }

        public override void StopListenerChannel(int listenerChannelId, bool immediate)
        {
            Debug.Print("BaseAppDomainProtocolHandler.StopListenerChannel() listenerChannelId: " + listenerChannelId + " immediate: " + immediate + " calling ReportStopped()");
            if (listenerChannelId != listenerChannelContext.ListenerChannelId)
            {
                DiagnosticUtility.DebugAssert("Invalid ListenerChannel ID!");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInternal(false);
            }

            Stop();
        }

        void Stop()
        {
            lock (ThisLock)
            {
                if (this.listenerChannelCallback != null)
                {
                    OnStop();
                    this.listenerChannelCallback.ReportStopped(0);
                    this.listenerChannelCallback = null;
                }
            }
        }
    }
}

