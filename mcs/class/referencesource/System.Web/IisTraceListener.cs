//------------------------------------------------------------------------------
// <copyright file="IisTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web {

    [HostProtection(Synchronization=true)]
    public sealed class IisTraceListener : TraceListener {


        public IisTraceListener() {
            // only supported on IIS version 7 and later
            HttpContext context = HttpContext.Current;
            if (context != null) {
                if (!HttpRuntime.UseIntegratedPipeline && !(context.WorkerRequest is ISAPIWorkerRequestInProcForIIS7)) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_7));
                }
            }
        }

        // the listener apis

        public override void Write(string message)
        {
            if (Filter != null && !Filter.ShouldTrace(null, String.Empty, TraceEventType.Verbose, 0, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context != null) {
                context.WorkerRequest.RaiseTraceEvent(IntegratedTraceType.TraceWrite, message);
            }
        }


        public override void Write(string message, string category)
        {
            if (Filter != null && !Filter.ShouldTrace(null, String.Empty, TraceEventType.Verbose, 0, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context != null) {
                context.WorkerRequest.RaiseTraceEvent(IntegratedTraceType.TraceWrite, message);
            }
        }


        public override void WriteLine(string message)
        {
            if (Filter != null && !Filter.ShouldTrace(null, String.Empty, TraceEventType.Verbose, 0, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context != null) {
                context.WorkerRequest.RaiseTraceEvent(IntegratedTraceType.TraceWrite, message);
            }
        }


        public override void WriteLine(string message, string category)
        {
            if (Filter != null && !Filter.ShouldTrace(null, String.Empty, TraceEventType.Verbose, 0, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context != null) {
                context.WorkerRequest.RaiseTraceEvent(IntegratedTraceType.TraceWrite, message);
            }
        }

    	public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, object data) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null)) 
                return;
            HttpContext context = HttpContext.Current;
            if (context != null) {
                string datastring = String.Empty;
                if (data != null) {
                    datastring = data.ToString();
                }
                context.WorkerRequest.RaiseTraceEvent(Convert(eventType), AppendTraceOptions(eventCache, datastring));
            }
        }

    	public override void TraceData(TraceEventCache eventCache, String source, TraceEventType eventType, int id, params object[] data) {
            HttpContext context = HttpContext.Current;
            if (context == null)
                return;
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data)) 
                return;

            StringBuilder sb = new StringBuilder();
            if (data != null) {
                for (int i=0; i< data.Length; i++) {
                    if (i != 0)
                        sb.Append(", ");

                    if (data[i] != null)
                        sb.Append(data[i].ToString());
                }
            }
            if (context != null) {
                context.WorkerRequest.RaiseTraceEvent(Convert(eventType), AppendTraceOptions(eventCache, sb.ToString()));
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType severity, int id, string message) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, severity, id, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context == null)
                return;

            context.WorkerRequest.RaiseTraceEvent(Convert(severity), AppendTraceOptions(eventCache, message));
        }

        public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType severity, int id, string format, params object[] args) {
            TraceEvent(eventCache, source, severity, id, String.Format(CultureInfo.InvariantCulture, format, args));
        }

        // append trace options to message and return the result
        private String AppendTraceOptions(TraceEventCache eventCache, String message) {
            if (eventCache == null || TraceOutputOptions == TraceOptions.None) {
                return message;
            }

            StringBuilder sb = new StringBuilder(message, 1024); 
            
            if (IsEnabled(TraceOptions.ProcessId)) {
                sb.Append("\r\nProcessId=");
                sb.Append(eventCache.ProcessId);
            }

            if (IsEnabled(TraceOptions.LogicalOperationStack)) {
                sb.Append("\r\nLogicalOperationStack=");
                bool first = true;
                foreach (Object obj in eventCache.LogicalOperationStack) {
                    if (!first) {
                        sb.Append(", ");
                    }
                    else {
                        first = false;
                    }                    
                    sb.Append(obj);
                }
            }

            if (IsEnabled(TraceOptions.ThreadId)) {
                sb.Append("\r\nThreadId=");
                sb.Append(eventCache.ThreadId);
            }

            if (IsEnabled(TraceOptions.DateTime)) {
                sb.Append("\r\nDateTime=");
                sb.Append(eventCache.DateTime.ToString("o", CultureInfo.InvariantCulture));
            }

            if (IsEnabled(TraceOptions.Timestamp)) {
                sb.Append("\r\nTimestamp=");
                sb.Append(eventCache.Timestamp);
            }

            if (IsEnabled(TraceOptions.Callstack)) {
                sb.Append("\r\nCallstack=");
                sb.Append(eventCache.Callstack);
            }

            return sb.ToString();
        }

        private bool IsEnabled(TraceOptions opts) {
            return (opts & TraceOutputOptions) != 0;
        }

        private IntegratedTraceType Convert(TraceEventType tet) {
            switch (tet) {
                case TraceEventType.Critical:
                     return IntegratedTraceType.DiagCritical;
                case TraceEventType.Error:
                     return IntegratedTraceType.DiagError;
                case TraceEventType.Warning:
                     return IntegratedTraceType.DiagWarning;
                case TraceEventType.Information:
                     return IntegratedTraceType.DiagInfo;
                case TraceEventType.Verbose:
                     return IntegratedTraceType.DiagVerbose;
                case TraceEventType.Start:
                     return IntegratedTraceType.DiagStart;
                case TraceEventType.Stop:
                     return IntegratedTraceType.DiagStop;
                case TraceEventType.Suspend:
                     return IntegratedTraceType.DiagSuspend;
                case TraceEventType.Resume:
                     return IntegratedTraceType.DiagResume;
                case TraceEventType.Transfer:
                     return IntegratedTraceType.DiagTransfer;
            }
            // Default to verbose logging
            return IntegratedTraceType.DiagVerbose;
        } 
    }
}

