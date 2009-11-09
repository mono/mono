// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !SILVERLIGHT

using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace System.ComponentModel.Composition.Diagnostics
{
    public partial class TraceContext : IDisposable
    {
        private readonly SourceLevels _previousLevel = TraceSourceTraceWriter.Source.Switch.Level;
        private readonly TraceContextTraceListener _listener = new TraceContextTraceListener();

        public TraceContext(SourceLevels level)
        {
            TraceSourceTraceWriter.Source.Switch.Level = level;
            TraceSourceTraceWriter.Source.Listeners.Add(_listener);
        }

        [CLSCompliant(false)]
        public TraceEventDetails LastTraceEvent
        {
            get { return _listener.TraceEvents.LastOrDefault(); }
        }

        [CLSCompliant(false)]
        public IList<TraceEventDetails> TraceEvents
        {
            get { return _listener.TraceEvents; }
        }

        public void Dispose()
        {
            TraceSourceTraceWriter.Source.Listeners.Remove(_listener);
            TraceSourceTraceWriter.Source.Switch.Level = _previousLevel;            
        }
    }
}

#endif