//------------------------------------------------------------------------------
// <copyright file="EtwListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics.Eventing{

    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventProviderTraceListener : TraceListener
    {
        //
        // The listener uses the EtwProvider base class.
        // Because Listener data is not schematized at the moment the listener will
        // log events using WriteMessageEvent method. 
        // 
        // Because WriteMessageEvent takes a string as the event payload 
        // all the overriden loging methods convert the arguments into strings.
        // Event payload is "delimiter" separated, which can be configured
        // 
        // 
        private EventProvider m_provider;
        private const string s_nullStringValue = "null";
        private const string s_nullStringComaValue = "null,";
        private const string s_nullCStringValue = ": null";
        private const string s_activityIdString = "activityId=";
        private const string s_relatedActivityIdString = "relatedActivityId=";
        private const string s_callStackString = " : CallStack:";
        private const string s_optionDelimiter = "delimiter";
        private string m_delimiter = ";";
        private int m_initializedDelim = 0;
        private const uint s_keyWordMask = 0xFFFFFF00;
        private const int s_defaultPayloadSize = 512;
        private object m_Lock = new object();

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public string Delimiter
        {
            get
            {
                if (m_initializedDelim == 0)
                {
                    lock (m_Lock)
                    {
                        if (m_initializedDelim == 0)
                        {
                            if (Attributes.ContainsKey(s_optionDelimiter))
                            {
                                m_delimiter = Attributes[s_optionDelimiter];

                            }
                            m_initializedDelim = 1;
                        }
                    }

                if (m_delimiter == null)
                    throw new ArgumentNullException("Delimiter");

                if (m_delimiter.Length == 0)
                    throw new ArgumentException(SR.GetString(SR.Argument_NeedNonemptyDelimiter));

                }
                return m_delimiter;
            }

            [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Delimiter");

                if (value.Length == 0)
                    throw new ArgumentException(SR.GetString(SR.Argument_NeedNonemptyDelimiter));

                lock (m_Lock)
                {
                    m_delimiter = value;
                    m_initializedDelim = 1;
                }
            }
        }

        protected override string[] GetSupportedAttributes()
        {
            return new String[] { s_optionDelimiter };
        }

        /// <summary>
        /// This method creates an instance of the ETW provider.
        /// The guid argument must be a valid GUID or a format exeption will be
        /// thrown when creating an instance of the ControlGuid. 
        /// We need to be running on Vista or above. If not an 
        /// PlatformNotSupported exception will be thrown by the EventProvider. 
        /// </summary>
        public EventProviderTraceListener(string providerId)
        {
            InitProvider(providerId);
        }

        public EventProviderTraceListener(string providerId, string name)
            : base(name)
        {
            InitProvider(providerId);
        }

        public EventProviderTraceListener(string providerId, string name, string delimiter)
            : base(name)
        {
            if (delimiter == null)
                throw new ArgumentNullException("delimiter");

            if (delimiter.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Argument_NeedNonemptyDelimiter));

            m_delimiter = delimiter;
            m_initializedDelim = 1;
            InitProvider(providerId);
        }

        private void InitProvider(string providerId)
        {

            Guid controlGuid = new Guid(providerId);
            //
            // Create The ETW TraceProvider
            //			

            m_provider = new EventProvider(controlGuid);
        }

        //
        // override Listener methods
        //
        public sealed override void Flush()
        {
        }

        public sealed override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        public override void Close()
        {
            m_provider.Close();
        }

        public sealed override void Write(string message)
        {
            if (!m_provider.IsEnabled()) 
            {
                return;
            }

            m_provider.WriteMessageEvent(message, (byte)TraceEventType.Information, 0);
        }

        public sealed override void WriteLine(string message)
        {
            Write(message);
        }

        //
        // For all the methods below the string to be logged contains:
        // m_delimeter seperated data converted to string
        // followed by the callstack if any. 
        // "id : Data1, Data2... : callstack : callstack value"
        //
        // The source parameter is ignored.
        // 
        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (!m_provider.IsEnabled())
            {
                return;
            }

            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null,null,null,null))
            {
                return;
            }

            StringBuilder dataString = new StringBuilder(s_defaultPayloadSize);

            if (data != null)
            {
                dataString.Append(data.ToString());
            }
            else
            {
                dataString.Append(s_nullCStringValue);
            }

            if ((eventCache != null) && (TraceOutputOptions & TraceOptions.Callstack) != 0)
            {
                dataString.Append(s_callStackString);
                dataString.Append(eventCache.Callstack);
                m_provider.WriteMessageEvent(
                            dataString.ToString(),
                            (byte)eventType,
                            (long)eventType & s_keyWordMask);
            }
            else
            {
                m_provider.WriteMessageEvent(dataString.ToString(),
                            (byte)eventType,
                            (long)eventType & s_keyWordMask);
            }
        }

        public sealed override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, params object[] data)
        {
            if (!m_provider.IsEnabled())
            {
                return;
            }

            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null))
            {
                return;
            }
            
            int index;
            StringBuilder dataString = new StringBuilder(s_defaultPayloadSize);

            if ((data != null) && (data.Length > 0) )
            {
                for (index = 0; index < (data.Length - 1); index++)
                {
                    if (data[index] != null)
                    {
                        dataString.Append(data[index].ToString());
                        dataString.Append(Delimiter);
                    }
                    else
                    {
                        dataString.Append(s_nullStringComaValue);
                    }
                }

                if (data[index] != null)
                {
                    dataString.Append(data[index].ToString());
                }
                else
                {
                    dataString.Append(s_nullStringValue);
                }
            }
            else
            {
                dataString.Append(s_nullStringValue);
            }

            if ((eventCache != null) && (TraceOutputOptions & TraceOptions.Callstack) != 0)
            {
                dataString.Append(s_callStackString);
                dataString.Append(eventCache.Callstack);
                m_provider.WriteMessageEvent(
                            dataString.ToString(),
                            (byte)eventType,
                            (long)eventType & s_keyWordMask);
            }
            else
            {
                m_provider.WriteMessageEvent(dataString.ToString(), 
                            (byte)eventType, 
                            (long)eventType & s_keyWordMask);
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (!m_provider.IsEnabled())
            {
                return;
            }

            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null))
            {
                return;
            }

            if ((eventCache != null) && (TraceOutputOptions & TraceOptions.Callstack) != 0)
            {
                m_provider.WriteMessageEvent(s_callStackString + eventCache.Callstack, 
                            (byte)eventType, 
                            (long)eventType & s_keyWordMask);
            }
            else
            {
                m_provider.WriteMessageEvent(String.Empty, 
                            (byte)eventType, 
                            (long)eventType & s_keyWordMask);
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (!m_provider.IsEnabled())
            {
                return;
            }

            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null))
            {
                return;
            }

            StringBuilder dataString = new StringBuilder(s_defaultPayloadSize);
            dataString.Append(message);

            if ((eventCache != null) && (TraceOutputOptions & TraceOptions.Callstack) != 0)
            {
                dataString.Append(s_callStackString);
                dataString.Append(eventCache.Callstack);
                m_provider.WriteMessageEvent(
                            dataString.ToString(),
                            (byte)eventType,
                            (long)eventType & s_keyWordMask);
            }
            else
            {
                m_provider.WriteMessageEvent(dataString.ToString(),
                            (byte)eventType,
                            (long)eventType & s_keyWordMask);
            }
        }

        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (!m_provider.IsEnabled())
            {
                return;
            }

            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null))
            {
                return;
            }

            if (args == null)
            {
                if ((eventCache != null) && (TraceOutputOptions & TraceOptions.Callstack) != 0)
                {
                    m_provider.WriteMessageEvent(format + s_callStackString + eventCache.Callstack,
                                (byte)eventType, 
                                (long)eventType & s_keyWordMask);
                }
                else
                {
                    m_provider.WriteMessageEvent(format, 
                                (byte)eventType, 
                                (long)eventType & s_keyWordMask);
                }
            }
            else
            {
                if ((eventCache != null) && (TraceOutputOptions & TraceOptions.Callstack) != 0)
                {
                    m_provider.WriteMessageEvent(String.Format(CultureInfo.InvariantCulture, format, args) + s_callStackString + eventCache.Callstack,
                                (byte)eventType, 
                                (long)eventType & s_keyWordMask);
                }
                else
                {
                    m_provider.WriteMessageEvent(String.Format(CultureInfo.InvariantCulture, format, args), 
                                (byte)eventType, 
                                (long)eventType&s_keyWordMask);
                }
            }
        }

        public override void Fail(string message, string detailMessage)
        {
            StringBuilder failMessage = new StringBuilder(message);
            if (detailMessage != null)
            {
                failMessage.Append(" ");
                failMessage.Append(detailMessage);
            }

            this.TraceEvent(null, null, TraceEventType.Error, 0, failMessage.ToString());
        }

        [System.Security.SecurityCritical]
        public sealed override void TraceTransfer(TraceEventCache eventCache, String source, int id, string message, Guid relatedActivityId)
        {
            if (!m_provider.IsEnabled())
            {
                return;
            }

            StringBuilder dataString = new StringBuilder(s_defaultPayloadSize);
            object correlationId = Trace.CorrelationManager.ActivityId;

            if (correlationId != null)
            {
                Guid activityId = (Guid)correlationId;
                dataString.Append(s_activityIdString);
                dataString.Append(activityId.ToString());
                dataString.Append(Delimiter); 
            }


            dataString.Append(s_relatedActivityIdString);
            dataString.Append(relatedActivityId.ToString());
            dataString.Append(Delimiter + message);

            if ((eventCache != null) && (TraceOutputOptions & TraceOptions.Callstack) != 0)
            {
                dataString.Append(s_callStackString);
                dataString.Append(eventCache.Callstack);
                m_provider.WriteMessageEvent(
                            dataString.ToString(), 
                            0, 
                            (long)TraceEventType.Transfer);
            }
            else
            {
                m_provider.WriteMessageEvent(dataString.ToString(), 
                            0, 
                            (long)TraceEventType.Transfer);
            }
        }
    }
}
