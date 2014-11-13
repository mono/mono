//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Diagnostics;

    class HostedTcpTransportManager : SharedTcpTransportManager
    {
        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile bool settingsApplied;
        Action<Uri> onViaCallback;

        public HostedTcpTransportManager(BaseUriWithWildcard baseAddress)
            : base(baseAddress.BaseAddress)
        {
            this.HostNameComparisonMode = baseAddress.HostNameComparisonMode;
            this.onViaCallback = new Action<Uri>(OnVia);
        }

        internal void Start(int queueId, Guid token, Action messageReceivedCallback)
        {
            SetMessageReceivedCallback(messageReceivedCallback);
            OnOpenInternal(queueId, token);
        }

        internal override void OnOpen()
        {
            // This is intentionally empty.
        }

        internal override void OnClose(TimeSpan timeout)
        {
            // This is intentionally empty.
        }

        internal override void OnAbort()
        {
            // This is intentionally empty.
        }

        internal void Stop(TimeSpan timeout)
        {
            CleanUp(false, timeout);
            settingsApplied = false;
        }

        protected override Action<Uri> GetOnViaCallback()
        {
            return this.onViaCallback;
        }

        void OnVia(Uri address)
        {
            Debug.Print("HostedTcpTransportManager.OnVia() address: " + address + " calling EnsureServiceAvailable()");
            ServiceHostingEnvironment.EnsureServiceAvailable(address.LocalPath);
        }

        protected override void OnSelecting(TcpChannelListener channelListener)
        {
            if (settingsApplied)
            {
                return;
            }

            lock (ThisLock)
            {
                if (settingsApplied)
                {
                    // Use the first one.
                    return;
                }

                this.ApplyListenerSettings(channelListener);
                settingsApplied = true;
            }
        }
    }
}
