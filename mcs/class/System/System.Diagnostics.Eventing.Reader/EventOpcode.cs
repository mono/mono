namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventOpcode
    {
        private bool dataReady;
        private string displayName;
        private string name;
        private ProviderMetadata pmReference;
        private object syncObject;
        private int value;

        internal EventOpcode(int value, ProviderMetadata pmReference)
        {
            this.value = value;
            this.pmReference = pmReference;
            this.syncObject = new object();
        }

        internal EventOpcode(string name, int value, string displayName)
        {
            this.value = value;
            this.name = name;
            this.displayName = displayName;
            this.dataReady = true;
            this.syncObject = new object();
        }

        internal void PrepareData()
        {
            lock (this.syncObject)
            {
                if (!this.dataReady)
                {
                    IEnumerable<EventOpcode> opcodes = this.pmReference.Opcodes;
                    this.name = null;
                    this.displayName = null;
                    this.dataReady = true;
                    foreach (EventOpcode opcode in opcodes)
                    {
                        if (opcode.Value == this.value)
                        {
                            this.name = opcode.Name;
                            this.displayName = opcode.DisplayName;
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

