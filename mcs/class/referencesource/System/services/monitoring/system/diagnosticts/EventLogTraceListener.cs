//------------------------------------------------------------------------------
// <copyright file="EventLogTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 */
namespace System.Diagnostics {
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>Provides a simple listener for directing tracing or
    ///       debugging output to a <see cref='T:System.IO.TextWriter'/> or to a <see cref='T:System.IO.Stream'/>, such as <see cref='F:System.Console.Out'/> or
    ///    <see cref='T:System.IO.FileStream'/>.</para>
    /// </devdoc>
    [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public sealed class EventLogTraceListener : TraceListener {
        private EventLog eventLog;
        private bool nameSet;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.EventLogTraceListener'/> class without a trace
        ///    listener.</para>
        /// </devdoc>
        public EventLogTraceListener() {
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.EventLogTraceListener'/> class using the
        ///    specified event log.</para>
        /// </devdoc>
        public EventLogTraceListener(EventLog eventLog)
            : base((eventLog != null) ? eventLog.Source : string.Empty) {
            this.eventLog = eventLog;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Diagnostics.EventLogTraceListener'/> class using the
        ///    specified source.</para>
        /// </devdoc>
        public EventLogTraceListener(string source) {
            eventLog = new EventLog();
            eventLog.Source = source;
        }

        /// <devdoc>
        ///    <para>Gets or sets the event log to write to.</para>
        /// </devdoc>
        public EventLog EventLog {
            get {
                return eventLog;
            }

            set {
                eventLog = value;
            }
        }

        /// <devdoc>
        ///    <para> Gets or sets the
        ///       name of this trace listener.</para>
        /// </devdoc>
        public override string Name {
            get {
                if (nameSet == false && eventLog != null) {
                    nameSet = true;
                    base.Name = eventLog.Source;
                }

                return base.Name;
            }

            set {
                nameSet = true;
                base.Name = value;
            }
        }

        /// <devdoc>
        ///    <para>Closes the text writer so that it no longer receives tracing or
        ///       debugging output.</para>
        /// </devdoc>
        public override void Close() {
            if (eventLog != null)
                eventLog.Close();
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    this.Close();
                }
                else {
                    // clean up resources
                    if (eventLog != null)
                        eventLog.Close();
                    eventLog = null;
                }
             } 
             finally {
                base.Dispose(disposing);
             }
        }

        /// <devdoc>
        ///    <para>Writes a message to this instance's event log.</para>
        /// </devdoc>
        public override void Write(string message) {
            if (eventLog != null) eventLog.WriteEntry(message);
        }

        /// <devdoc>
        ///    <para>Writes a message to this instance's event log followed by a line terminator.
        ///       The default line terminator is a carriage return followed by a line feed
        ///       (\r\n).</para>
        /// </devdoc>
        public override void WriteLine(string message) {
            Write(message);
        }

        [
        ComVisible(false)
        ]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id,
                                                    string format, params object[] args)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, severity, id, format, args)) 
                return;

            EventInstance data = CreateEventInstance(severity, id);

            if (args == null) {
                eventLog.WriteEvent(data, format);
            }
            else if(String.IsNullOrEmpty(format)) {
                string[] strings = new string[args.Length];
                for (int i=0; i<args.Length; i++) {
                    strings[i] = args[i].ToString();
                }

                eventLog.WriteEvent(data, strings);
            }
            else {
                eventLog.WriteEvent(data, String.Format(CultureInfo.InvariantCulture, format,args));
            }

        }

        [
        ComVisible(false)
        ]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id,
                                                    string message)
        {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, severity, id, message)) 
                return;

            EventInstance data = CreateEventInstance(severity, id);

            eventLog.WriteEvent(data, message);
        }

        [ComVisible(false)]
        public override void TraceData(TraceEventCache eventCache, String source, TraceEventType severity, int id, object data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, severity, id, null, null, data)) 
                return;

            EventInstance inst = CreateEventInstance(severity, id);
            eventLog.WriteEvent(inst, new object[] {data} );

            
        }
        
        [ComVisible(false)]
        public override void TraceData(TraceEventCache eventCache, String source, TraceEventType severity, int id, params object[] data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, severity, id, null, null, null, data)) 
                return;

            EventInstance inst = CreateEventInstance(severity, id);

            StringBuilder sb = new StringBuilder();
            if (data != null) {
                for (int i=0; i< data.Length; i++) {
                    if (i != 0)
                        sb.Append(", ");

                    if (data[i] != null)
                        sb.Append(data[i].ToString());
                }
            }

            eventLog.WriteEvent(inst, new object[] {sb.ToString()});
        }
        
        private EventInstance CreateEventInstance(TraceEventType severity, int id) {
            // Win32 EventLog has an implicit cap at ushort.MaxValue
            // We need to cap this explicitly to prevent larger value 
            // being wrongly casted 
            if (id > ushort.MaxValue)
                id = ushort.MaxValue;

            // Ideally we need to pick a value other than '0' as zero is 
            // a commonly used EventId by most applications 
            if (id < ushort.MinValue)
                id = ushort.MinValue;
            
            EventInstance data = new EventInstance(id, 0);

            if (severity == TraceEventType.Error || severity == TraceEventType.Critical) 
                data.EntryType = EventLogEntryType.Error;
            else if (severity == TraceEventType.Warning)
                data.EntryType = EventLogEntryType.Warning;
            else                
                data.EntryType = EventLogEntryType.Information;

            return data;
        }

    }
}

