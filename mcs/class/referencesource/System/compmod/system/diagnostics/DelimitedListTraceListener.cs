//------------------------------------------------------------------------------
// <copyright file="DelimitedListTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


using System;
using System.Text;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Security.Permissions;
using System.Runtime.Versioning;

namespace System.Diagnostics {
    [HostProtection(Synchronization=true)]
    public class DelimitedListTraceListener : TextWriterTraceListener {
        string delimiter = ";";
        string secondaryDelim = ",";
        bool initializedDelim = false;

        public DelimitedListTraceListener(Stream stream) : base(stream) {
        }

        public DelimitedListTraceListener(Stream stream, string name) : base(stream, name) {
        }

        public DelimitedListTraceListener(TextWriter writer) : base(writer) {
        }

        public DelimitedListTraceListener(TextWriter writer, string name) : base(writer, name) {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public DelimitedListTraceListener(string fileName) : base (fileName) {
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public DelimitedListTraceListener(string fileName, string name) : base(fileName, name) {
        }

        public string Delimiter {
            get {
                lock(this) { // Probably overkill
                    if (!initializedDelim) {

                        if (Attributes.ContainsKey("delimiter"))
                            delimiter = Attributes["delimiter"];

                        initializedDelim = true;
                    }
                }
                return delimiter; 
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("Delimiter");

                if (value.Length == 0)
                    throw new ArgumentException(SR.GetString("Generic_ArgCantBeEmptyString", "Delimiter"));
                
                lock(this) {
                    delimiter = value;
                    initializedDelim = true;
                }
                
                if (delimiter == ",")
                    secondaryDelim = ";";
                else
                    secondaryDelim = ",";
            }
        }

        protected override internal string[] GetSupportedAttributes() {
            return new String[]{"delimiter"};
        }

        
        public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string format, params object[] args) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, format, args)) 
                return;

            WriteHeader(source, eventType, id);

            if (args != null)
                WriteEscaped(String.Format(CultureInfo.InvariantCulture, format, args));
            else
                WriteEscaped(format);
            Write(Delimiter); // Use get_Delimiter

            // one more delimiter for the data object
            Write(Delimiter); // Use get_Delimiter

            WriteFooter(eventCache);
        }

        public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, int id, string message) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, message)) 
                return;
            
            WriteHeader(source, eventType, id);

            WriteEscaped(message);
            Write(Delimiter); // Use get_Delimiter

            // one more delimiter for the data object
            Write(Delimiter); // Use get_Delimiter

            WriteFooter(eventCache);
        }
        
        public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, object data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data)) 
                return;

            WriteHeader(source, eventType, id);

            // first a delimiter for the message
            Write(Delimiter); // Use get_Delimiter

            WriteEscaped(data.ToString());
            Write(Delimiter); // Use get_Delimiter

            WriteFooter(eventCache);
        }

        public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, params object[] data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data)) 
                return;

            WriteHeader(source, eventType, id);

            // first a delimiter for the message
            Write(Delimiter); // Use get_Delimiter

            if (data != null) {
                for (int i=0; i<data.Length; i++) {
                    if (i != 0)
                        Write(secondaryDelim);
                    WriteEscaped(data[i].ToString());
                }
            }
            Write(Delimiter); // Use get_Delimiter

            WriteFooter(eventCache);
        }

        private void WriteHeader(String source, TraceEventType eventType, int id) {
            WriteEscaped(source);
            Write(Delimiter); // Use get_Delimiter

            Write(eventType.ToString());
            Write(Delimiter); // Use get_Delimiter

            Write(id.ToString(CultureInfo.InvariantCulture));
            Write(Delimiter); // Use get_Delimiter
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private void WriteFooter(TraceEventCache eventCache) {
            if (eventCache != null) {
                if (IsEnabled(TraceOptions.ProcessId))
                    Write(eventCache.ProcessId.ToString(CultureInfo.InvariantCulture));
                Write(Delimiter); // Use get_Delimiter

                if (IsEnabled(TraceOptions.LogicalOperationStack))
                    WriteStackEscaped(eventCache.LogicalOperationStack);
                Write(Delimiter); // Use get_Delimiter

                if (IsEnabled(TraceOptions.ThreadId))
                    WriteEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
                Write(Delimiter); // Use get_Delimiter

                if (IsEnabled(TraceOptions.DateTime))
                    WriteEscaped(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
                Write(Delimiter); // Use get_Delimiter

                if (IsEnabled(TraceOptions.Timestamp))
                    Write(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                Write(Delimiter); // Use get_Delimiter

                if (IsEnabled(TraceOptions.Callstack))
                    WriteEscaped(eventCache.Callstack);
            }
            else {
                for (int i=0; i<5; i++)
                    Write(Delimiter); // Use get_Delimiter 
            }

            WriteLine("");
        }

        private void WriteEscaped(string message) {
            if (!String.IsNullOrEmpty(message)) {
                StringBuilder sb = new StringBuilder("\"");
                int index;
                int lastindex = 0;
                while((index = message.IndexOf('"', lastindex)) != -1) {
                    sb.Append(message, lastindex, index - lastindex);
                    sb.Append("\"\"");
                    lastindex = index + 1;
                }

                sb.Append(message, lastindex, message.Length - lastindex);
                sb.Append("\"");
                Write(sb.ToString());
            }
        }

        private void WriteStackEscaped(Stack stack) {
            StringBuilder sb = new StringBuilder("\"");
            bool first = true;
            foreach (Object obj in stack) {
                if (!first)
                    sb.Append(", ");
                else
                    first = false;
                
                string operation = obj.ToString();
                
                int index;
                int lastindex = 0;
                while((index = operation.IndexOf('"', lastindex)) != -1) {
                    sb.Append(operation, lastindex, index - lastindex);
                    sb.Append("\"\"");
                    lastindex = index + 1;
                }

                sb.Append(operation, lastindex, operation.Length - lastindex);
            }
            sb.Append("\"");
            Write(sb.ToString());
        }
        
    }
}
