//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Runtime.Diagnostics;
    using System.Web;
    using System.Xml;
    
    class HttpRequestTraceRecord : TraceRecord
    {
        HttpRequest request;

        internal HttpRequestTraceRecord(HttpRequest request)
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
            writer.WriteElementString("Path", this.request.Path);
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
