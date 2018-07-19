//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Security.Principal;
    using System.Collections.Generic;

    abstract class NamedPipeTransportManager
        : ConnectionOrientedTransportManager<NamedPipeChannelListener>, ITransportManagerRegistration
    {
        List<SecurityIdentifier> allowedUsers;
        HostNameComparisonMode hostNameComparisonMode;
        Uri listenUri;

        protected NamedPipeTransportManager(Uri listenUri)
        {
            this.listenUri = listenUri;
        }

        protected void SetAllowedUsers(List<SecurityIdentifier> allowedUsers)
        {
            this.allowedUsers = allowedUsers;
        }

        protected void SetHostNameComparisonMode(HostNameComparisonMode hostNameComparisonMode)
        {
            this.hostNameComparisonMode = hostNameComparisonMode;
        }

        internal List<SecurityIdentifier> AllowedUsers
        {
            get
            {
                return this.allowedUsers;
            }
        }

        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }

            protected set
            {
                HostNameComparisonModeHelper.Validate(value);
                lock (base.ThisLock)
                {
                    ThrowIfOpen();
                    this.hostNameComparisonMode = value;
                }
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        internal override string Scheme
        {
            get { return Uri.UriSchemeNetPipe; }
        }

        bool AreAllowedUsersEqual(List<SecurityIdentifier> otherAllowedUsers)
        {
            return ((this.allowedUsers == otherAllowedUsers) ||
                (IsSubset(this.allowedUsers, otherAllowedUsers) && IsSubset(otherAllowedUsers, this.allowedUsers)));
        }

        protected virtual bool IsCompatible(NamedPipeChannelListener channelListener)
        {
            if (channelListener.InheritBaseAddressSettings)
            {
                return true;
            }

            return (
                base.IsCompatible(channelListener)
                && this.AreAllowedUsersEqual(channelListener.AllowedUsers)
                && (this.HostNameComparisonMode == channelListener.HostNameComparisonMode)
                );
        }

        static bool IsSubset(List<SecurityIdentifier> users1, List<SecurityIdentifier> users2)
        {
            if (users1 == null)
            {
                return true;
            }

            foreach (SecurityIdentifier user in users1)
            {
                if (!users2.Contains(user))
                {
                    return false;
                }
            }

            return true;
        }

        internal override void OnClose(TimeSpan timeout)
        {
            Cleanup();
        }

        internal override void OnAbort()
        {
            Cleanup();
            base.OnAbort();
        }

        void Cleanup()
        {
            NamedPipeChannelListener.StaticTransportManagerTable.UnregisterUri(this.ListenUri, this.HostNameComparisonMode);
        }

        protected virtual void OnSelecting(NamedPipeChannelListener channelListener)
        { }

        IList<TransportManager> ITransportManagerRegistration.Select(TransportChannelListener channelListener)
        {
            OnSelecting((NamedPipeChannelListener)channelListener);

            IList<TransportManager> result = null;
            if (this.IsCompatible((NamedPipeChannelListener)channelListener))
            {
                result = new List<TransportManager>();
                result.Add(this);
            }
            return result;
        }
    }
}
