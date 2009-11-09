// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !SILVERLIGHT

using System;
using System.Diagnostics;

namespace System.ComponentModel.Composition.Diagnostics
{
    [CLSCompliant(false)]
    public class TraceEventDetails
    {
        public TraceEventDetails(TraceEventCache eventCache, string source, TraceEventType eventType, TraceId id, string format, params object[] args)
        {
            EventCache = eventCache;
            Source = source;
            EventType = eventType;
            Id = id;
            Format = format;
            Args = args;
        }

        public TraceEventCache EventCache
        {
            get;
            private set;
        }

        public string Source
        {
            get;
            private set;
        }

        public TraceEventType EventType
        {
            get;
            private set;
        }

        public TraceId Id
        {
            get;
            private set;
        }

        public string Format
        {
            get;
            private set;
        }

        public object[] Args
        {
            get;
            private set;
        }
    }
}

#endif