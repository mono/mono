namespace System.Diagnostics.Eventing
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventProviderTraceListener : TraceListener
    {
        private string m_delimiter;
        private int m_initializedDelim;
        private object m_Lock;
        private EventProvider m_provider;
        private const string s_activityIdString = "activityId=";
        private const string s_callStackString = " : CallStack:";
        private const int s_defaultPayloadSize = 0x200;
        private const uint s_keyWordMask = 0xffffff00;
        private const string s_nullCStringValue = ": null";
        private const string s_nullStringComaValue = "null,";
        private const string s_nullStringValue = "null";
        private const string s_optionDelimiter = "delimiter";
        private const string s_relatedActivityIdString = "relatedActivityId=";

        public EventProviderTraceListener(string providerId)
        {
            this.m_delimiter = ";";
            this.m_Lock = new object();
            this.InitProvider(providerId);
        }

        public EventProviderTraceListener(string providerId, string name) : base(name)
        {
            this.m_delimiter = ";";
            this.m_Lock = new object();
            this.InitProvider(providerId);
        }

        public EventProviderTraceListener(string providerId, string name, string delimiter) : base(name)
        {
            this.m_delimiter = ";";
            this.m_Lock = new object();
            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }
            if (delimiter.Length == 0)
            {
                throw new ArgumentException(System.SR.GetString("Argument_NeedNonemptyDelimiter"));
            }
            this.m_delimiter = delimiter;
            this.m_initializedDelim = 1;
            this.InitProvider(providerId);
        }

        public override void Close()
        {
            this.m_provider.Close();
        }

        public override void Fail(string message, string detailMessage)
        {
            StringBuilder builder = new StringBuilder(message);
            if (detailMessage != null)
            {
                builder.Append(" ");
                builder.Append(detailMessage);
            }
            this.TraceEvent(null, null, TraceEventType.Error, 0, builder.ToString());
        }

        public sealed override void Flush()
        {
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "delimiter" };
        }

        private void InitProvider(string providerId)
        {
            Guid providerGuid = new Guid(providerId);
            this.m_provider = new EventProvider(providerGuid);
        }

        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                if ((data != null) && (data.Length > 0))
                {
                    int index = 0;
                    while (index < (data.Length - 1))
                    {
                        if (data[index] != null)
                        {
                            builder.Append(data[index].ToString());
                            builder.Append(this.Delimiter);
                        }
                        else
                        {
                            builder.Append("null,");
                        }
                        index++;
                    }
                    if (data[index] != null)
                    {
                        builder.Append(data[index].ToString());
                    }
                    else
                    {
                        builder.Append("null");
                    }
                }
                else
                {
                    builder.Append("null");
                }
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    builder.Append(" : CallStack:");
                    builder.Append(eventCache.Callstack);
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
            }
        }

        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                if (data != null)
                {
                    builder.Append(data.ToString());
                }
                else
                {
                    builder.Append(": null");
                }
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    builder.Append(" : CallStack:");
                    builder.Append(eventCache.Callstack);
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    this.m_provider.WriteMessageEvent(" : CallStack:" + eventCache.Callstack, (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(string.Empty, (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                StringBuilder builder = new StringBuilder(0x200);
                builder.Append(message);
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    builder.Append(" : CallStack:");
                    builder.Append(eventCache.Callstack);
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(builder.ToString(), (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (this.m_provider.IsEnabled() && ((base.Filter == null) || base.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null)))
            {
                if (args == null)
                {
                    if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                    {
                        this.m_provider.WriteMessageEvent(format + " : CallStack:" + eventCache.Callstack, (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                    }
                    else
                    {
                        this.m_provider.WriteMessageEvent(format, (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                    }
                }
                else if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    this.m_provider.WriteMessageEvent(string.Format(CultureInfo.InvariantCulture, format, args) + " : CallStack:" + eventCache.Callstack, (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
                else
                {
                    this.m_provider.WriteMessageEvent(string.Format(CultureInfo.InvariantCulture, format, args), (byte) eventType, ((long) eventType) & ((long) 0xffffff00L));
                }
            }
        }

        [SecurityCritical]
        public sealed override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            if (this.m_provider.IsEnabled())
            {
                StringBuilder builder = new StringBuilder(0x200);
                object activityId = Trace.CorrelationManager.ActivityId;
                if (activityId != null)
                {
                    Guid guid = (Guid) activityId;
                    builder.Append("activityId=");
                    builder.Append(guid.ToString());
                    builder.Append(this.Delimiter);
                }
                builder.Append("relatedActivityId=");
                builder.Append(relatedActivityId.ToString());
                builder.Append(this.Delimiter + message);
                if ((eventCache != null) && ((base.TraceOutputOptions & TraceOptions.Callstack) != TraceOptions.None))
                {
                    builder.Append(" : CallStack:");
                    builder.Append(eventCache.Callstack);
                    this.m_provider.WriteMessageEvent(builder.ToString(), 0, 0x1000L);
                }
                else
                {
                    this.m_provider.WriteMessageEvent(builder.ToString(), 0, 0x1000L);
                }
            }
        }

        public sealed override void Write(string message)
        {
            if (this.m_provider.IsEnabled())
            {
                this.m_provider.WriteMessageEvent(message, 8, 0L);
            }
        }

        public sealed override void WriteLine(string message)
        {
            this.Write(message);
        }

        public string Delimiter
        {
            get
            {
                if (this.m_initializedDelim == 0)
                {
                    lock (this.m_Lock)
                    {
                        if (this.m_initializedDelim == 0)
                        {
                            if (base.Attributes.ContainsKey("delimiter"))
                            {
                                this.m_delimiter = base.Attributes["delimiter"];
                            }
                            this.m_initializedDelim = 1;
                        }
                    }
                    if (this.m_delimiter == null)
                    {
                        throw new ArgumentNullException("Delimiter");
                    }
                    if (this.m_delimiter.Length == 0)
                    {
                        throw new ArgumentException(System.SR.GetString("Argument_NeedNonemptyDelimiter"));
                    }
                }
                return this.m_delimiter;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Delimiter");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(System.SR.GetString("Argument_NeedNonemptyDelimiter"));
                }
                lock (this.m_Lock)
                {
                    this.m_delimiter = value;
                    this.m_initializedDelim = 1;
                }
            }
        }

        public sealed override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }
    }
}

