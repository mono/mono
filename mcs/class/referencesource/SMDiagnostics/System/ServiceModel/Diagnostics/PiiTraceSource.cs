//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Configuration;

    class PiiTraceSource : TraceSource
    {
        string eventSourceName = String.Empty;
        internal const string LogPii = "logKnownPii";
        bool shouldLogPii = false;
        bool initialized = false;
        object localSyncObject = new object();

        internal PiiTraceSource(string name, string eventSourceName)
            : base(name)
        { 
#pragma warning disable 618
            Fx.Assert(!String.IsNullOrEmpty(eventSourceName), "Event log source name must be valid");
#pragma warning restore 618
            this.eventSourceName = eventSourceName;
        }

        internal PiiTraceSource(string name, string eventSourceName, SourceLevels levels)
            : base(name, levels)
        {
#pragma warning disable 618
            Fx.Assert(!String.IsNullOrEmpty(eventSourceName), "Event log source name must be valid");
#pragma warning restore 618
            this.eventSourceName = eventSourceName;
        }

        void Initialize()
        {
            if (!this.initialized)
            {
                lock (localSyncObject)
                {
                    if (!this.initialized)
                    {
                        string attributeValue = this.Attributes[PiiTraceSource.LogPii];
                        bool shouldLogPii = false;
                        if (!string.IsNullOrEmpty(attributeValue))
                        {
                            if (!bool.TryParse(attributeValue, out shouldLogPii))
                            {
                                shouldLogPii = false;
                            }
                        }

                        if (shouldLogPii)
                        {
#pragma warning disable 618
                            System.Runtime.Diagnostics.EventLogger logger = new System.Runtime.Diagnostics.EventLogger(this.eventSourceName, null);
#pragma warning restore 618
                            if (MachineSettingsSection.EnableLoggingKnownPii)
                            {
                                logger.LogEvent(TraceEventType.Information,
                                    (ushort)System.Runtime.Diagnostics.EventLogCategory.MessageLogging,
                                    (uint)System.Runtime.Diagnostics.EventLogEventId.PiiLoggingOn,
                                    false);
                                this.shouldLogPii = true;
                            }
                            else
                            {
                                logger.LogEvent(TraceEventType.Error,
                                        (ushort)System.Runtime.Diagnostics.EventLogCategory.MessageLogging,
                                        (uint)System.Runtime.Diagnostics.EventLogEventId.PiiLoggingNotAllowed,
                                        false);
                            }
                        }
                        this.initialized = true;
                    }
                }
            }
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { PiiTraceSource.LogPii };
        }

        internal bool ShouldLogPii
        {
            get
            {
                // ShouldLogPii is called very frequently, don't call Initialize unless we have to.
                if (!this.initialized)
                {
                    Initialize();
                }
                return this.shouldLogPii;
            }
            set
            {
                // If you call this, you know what you're doing
                this.initialized = true;
                this.shouldLogPii = value;
            }
        }
    }
}
