namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Collections.Generic;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventLogLink
    {
        private uint channelId;
        private string channelName;
        private bool dataReady;
        private string displayName;
        private bool isImported;
        private ProviderMetadata pmReference;
        private object syncObject;

        internal EventLogLink(uint channelId, ProviderMetadata pmReference)
        {
            this.channelId = channelId;
            this.pmReference = pmReference;
            this.syncObject = new object();
        }

        internal EventLogLink(string channelName, bool isImported, string displayName, uint channelId)
        {
            this.channelName = channelName;
            this.isImported = isImported;
            this.displayName = displayName;
            this.channelId = channelId;
            this.dataReady = true;
            this.syncObject = new object();
        }

        private void PrepareData()
        {
            if (!this.dataReady)
            {
                lock (this.syncObject)
                {
                    if (!this.dataReady)
                    {
                        IEnumerable<EventLogLink> logLinks = this.pmReference.LogLinks;
                        this.channelName = null;
                        this.isImported = false;
                        this.displayName = null;
                        this.dataReady = true;
                        foreach (EventLogLink link in logLinks)
                        {
                            if (link.ChannelId == this.channelId)
                            {
                                this.channelName = link.LogName;
                                this.isImported = link.IsImported;
                                this.displayName = link.DisplayName;
                                this.dataReady = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        internal uint ChannelId
        {
            get
            {
                return this.channelId;
            }
        }

        public string DisplayName
        {
            get
            {
                this.PrepareData();
                return this.displayName;
            }
        }

        public bool IsImported
        {
            get
            {
                this.PrepareData();
                return this.isImported;
            }
        }

        public string LogName
        {
            get
            {
                this.PrepareData();
                return this.channelName;
            }
        }
    }
}

