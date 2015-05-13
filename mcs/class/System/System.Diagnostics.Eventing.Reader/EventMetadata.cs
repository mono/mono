namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class EventMetadata
    {
        private byte channelId;
        private string description;
        private long id;
        private long keywords;
        private byte level;
        private short opcode;
        private ProviderMetadata pmReference;
        private int task;
        private string template;
        private byte version;

        internal EventMetadata(uint id, byte version, byte channelId, byte level, byte opcode, short task, long keywords, string template, string description, ProviderMetadata pmReference)
        {
            this.id = id;
            this.version = version;
            this.channelId = channelId;
            this.level = level;
            this.opcode = opcode;
            this.task = task;
            this.keywords = keywords;
            this.template = template;
            this.description = description;
            this.pmReference = pmReference;
        }

        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.description;
            }
        }

        public long Id
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.id;
            }
        }

        public IEnumerable<EventKeyword> Keywords
        {
            get
            {
                List<EventKeyword> list = new List<EventKeyword>();
                ulong keywords = (ulong) this.keywords;
                ulong num2 = 9223372036854775808L;
                for (int i = 0; i < 0x40; i++)
                {
                    if ((keywords & num2) > 0L)
                    {
                        list.Add(new EventKeyword((long) num2, this.pmReference));
                    }
                    num2 = num2 >> 1;
                }
                return list;
            }
        }

        public EventLevel Level
        {
            get
            {
                return new EventLevel(this.level, this.pmReference);
            }
        }

        public EventLogLink LogLink
        {
            get
            {
                return new EventLogLink(this.channelId, this.pmReference);
            }
        }

        public EventOpcode Opcode
        {
            get
            {
                return new EventOpcode(this.opcode, this.pmReference);
            }
        }

        public EventTask Task
        {
            get
            {
                return new EventTask(this.task, this.pmReference);
            }
        }

        public string Template
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.template;
            }
        }

        public byte Version
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.version;
            }
        }
    }
}

