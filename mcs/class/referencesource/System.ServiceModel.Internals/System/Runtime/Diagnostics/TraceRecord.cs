//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System.Xml;

    [Serializable]
    class TraceRecord
    {
        protected const string EventIdBase = "http://schemas.microsoft.com/2006/08/ServiceModel/";
        protected const string NamespaceSuffix = "TraceRecord";

        internal virtual string EventId { get { return BuildEventId("Empty"); } }

        internal virtual void WriteTo(XmlWriter writer) 
        {
        }

        protected string BuildEventId(string eventId)
        {
            return TraceRecord.EventIdBase + eventId + TraceRecord.NamespaceSuffix;
        }

        protected string XmlEncode(string text)
        {
            return DiagnosticTraceBase.XmlEncode(text);
        }
    }
}
