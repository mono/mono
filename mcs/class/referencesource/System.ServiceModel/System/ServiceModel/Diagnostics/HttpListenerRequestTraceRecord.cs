//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Net;
    using System.Runtime.Diagnostics;
    using System.Xml;
    
    class HttpListenerRequestTraceRecord : TraceRecord
    {
        HttpListenerRequest request;

        internal HttpListenerRequestTraceRecord(HttpListenerRequest request)
        {
            this.request = request;
        }

        internal override string EventId { get { return BuildEventId("HttpRequest"); } }

        internal override void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement("Headers");
            foreach (string key in this.request.Headers.Keys)
            {
                writer.WriteElementString(key, this.request.Headers[key]);
            }
            writer.WriteEndElement();
            writer.WriteElementString("Url", this.request.Url.ToString());
            if (this.request.QueryString != null && this.request.QueryString.Count > 0)
            {
                writer.WriteStartElement("QueryString");
                foreach (string key in this.request.QueryString.Keys)
                {
                    writer.WriteElementString(key, this.request.Headers[key]);
                }
                writer.WriteEndElement();
            }
        }
    }
}
