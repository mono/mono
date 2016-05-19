//------------------------------------------------------------------------------
// <copyright file="WebPageListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.UI;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web {

    [HostProtection(Synchronization=true)]
    public class WebPageTraceListener : TraceListener {


        public WebPageTraceListener() {
        }

        // the listener apis

        public override void Write(string message)
        {
            if (Filter != null && !Filter.ShouldTrace(null, String.Empty, TraceEventType.Verbose, 0, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context != null) context.Trace.WriteInternal(message, false);
        }


        public override void Write(string message, string category)
        {
            if (Filter != null && !Filter.ShouldTrace(null, String.Empty, TraceEventType.Verbose, 0, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context != null) context.Trace.WriteInternal(category, message, false);
        }


        public override void WriteLine(string message)
        {
            if (Filter != null && !Filter.ShouldTrace(null, String.Empty, TraceEventType.Verbose, 0, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context != null) context.Trace.WriteInternal(message, false);
        }


        public override void WriteLine(string message, string category)
        {
            if (Filter != null && !Filter.ShouldTrace(null, String.Empty, TraceEventType.Verbose, 0, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context != null) context.Trace.WriteInternal(category, message, false);
        }


        public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType severity, int id, string message) {
            if (Filter != null && !Filter.ShouldTrace(eventCache, source, severity, id, message, null, null, null))
                return;

            HttpContext context = HttpContext.Current;
            if (context == null)
                return;

            string messagestring = SR.GetString(SR.WebPageTraceListener_Event) + " " + id + ": " + message;
            if (severity <= TraceEventType.Warning)
                context.Trace.WarnInternal(source, messagestring, false);
            else
                context.Trace.WriteInternal(source, messagestring, false);
        }

        public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType severity, int id, string format, params object[] args) {
            TraceEvent(eventCache, source, severity, id, String.Format(CultureInfo.InvariantCulture, format, args));
        }
    }
}

