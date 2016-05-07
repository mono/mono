//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Runtime.DurableInstancing;

    public abstract class SendReceiveExtension
    {
        public abstract HostSettings HostSettings { get; }

        public abstract void Send(MessageContext message, SendSettings settings, InstanceKey correlatesWith, Bookmark sendCompleteBookmark);

        public abstract void Cancel(Bookmark bookmark);

        public abstract void OnUninitializeCorrelation(InstanceKey correlationKey);

        public void RegisterReceive(ReceiveSettings settings, InstanceKey correlatesWith, Bookmark receiveBookmark)
        {
            this.OnRegisterReceive(settings, correlatesWith, receiveBookmark);
        }

        protected abstract void OnRegisterReceive(ReceiveSettings settings, InstanceKey correlatesWith, Bookmark receiveBookmark);
    }
}
