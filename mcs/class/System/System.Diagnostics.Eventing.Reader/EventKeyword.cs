namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventKeyword
    {
        private bool dataReady;
        private string displayName;
        private string name;
        private ProviderMetadata pmReference;
        private object syncObject;
        private long value;

        internal EventKeyword(long value, ProviderMetadata pmReference)
        {
            this.value = value;
            this.pmReference = pmReference;
            this.syncObject = new object();
        }

        internal EventKeyword(string name, long value, string displayName)
        {
            this.value = value;
            this.name = name;
            this.displayName = displayName;
            this.dataReady = true;
            this.syncObject = new object();
        }

        internal void PrepareData()
        {
            if (!this.dataReady)
            {
                lock (this.syncObject)
                {
                    if (!this.dataReady)
                    {
                        IEnumerable<EventKeyword> keywords = this.pmReference.Keywords;
                        this.name = null;
                        this.displayName = null;
                        this.dataReady = true;
                        foreach (EventKeyword keyword in keywords)
                        {
                            if (keyword.Value == this.value)
                            {
                                this.name = keyword.Name;
                                this.displayName = keyword.DisplayName;
                                break;
                            }
                        }
                    }
                }
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

        public string Name
        {
            get
            {
                this.PrepareData();
                return this.name;
            }
        }

        public long Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
        }
    }
}

