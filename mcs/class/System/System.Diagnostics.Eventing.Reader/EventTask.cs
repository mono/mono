namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventTask
    {
        private bool dataReady;
        private string displayName;
        private Guid guid;
        private string name;
        private ProviderMetadata pmReference;
        private object syncObject;
        private int value;

        internal EventTask(int value, ProviderMetadata pmReference)
        {
            this.value = value;
            this.pmReference = pmReference;
            this.syncObject = new object();
        }

        internal EventTask(string name, int value, string displayName, Guid guid)
        {
            this.value = value;
            this.name = name;
            this.displayName = displayName;
            this.guid = guid;
            this.dataReady = true;
            this.syncObject = new object();
        }

        internal void PrepareData()
        {
            lock (this.syncObject)
            {
                if (!this.dataReady)
                {
                    IEnumerable<EventTask> tasks = this.pmReference.Tasks;
                    this.name = null;
                    this.displayName = null;
                    this.guid = Guid.Empty;
                    this.dataReady = true;
                    foreach (EventTask task in tasks)
                    {
                        if (task.Value == this.value)
                        {
                            this.name = task.Name;
                            this.displayName = task.DisplayName;
                            this.guid = task.EventGuid;
                            this.dataReady = true;
                            break;
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

        public Guid EventGuid
        {
            get
            {
                this.PrepareData();
                return this.guid;
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

        public int Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
        }
    }
}

