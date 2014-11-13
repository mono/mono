//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Xml;
 
    class HttpErrorTraceRecord : TraceRecord
    {
        string html;

        internal HttpErrorTraceRecord(string html)
        {
            this.html = XmlEncode(html);
        }

        internal override string EventId
        {
            get { return BuildEventId("HttpError"); }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            writer.WriteElementString("HtmlErrorMessage", html);
        }
    }
}
