//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    
    class MessageLoggingFilterTraceRecord : TraceRecord
    {
        XPathMessageFilter filter;

        internal MessageLoggingFilterTraceRecord(XPathMessageFilter filter)
        {
            this.filter = filter;
        }

        internal override string EventId { get { return BuildEventId("MessageLoggingFilter"); } }

        internal override void WriteTo(XmlWriter writer)
        {
            filter.WriteXPathTo(writer, "", "Filter", "", false);
        }
    }
}
