// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !SILVERLIGHT

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.ComponentModel.Composition.Diagnostics
{
    partial class TraceContext : IDisposable
    {
        private class TraceContextTraceListener : TraceListener
        {
            private readonly Collection<TraceEventDetails> _traceEvents = new Collection<TraceEventDetails>();

            public IList<TraceEventDetails> TraceEvents
            {
                get { return _traceEvents; }
            }

            public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
            {
                _traceEvents.Add(new TraceEventDetails(eventCache, source, eventType, (TraceId)id, format, args));
            }

            public override void Write(string message)
            {
                throw new NotImplementedException();
            }

            public override void WriteLine(string message)
            {
                throw new NotImplementedException();
            }
        }
    }
}

#endif