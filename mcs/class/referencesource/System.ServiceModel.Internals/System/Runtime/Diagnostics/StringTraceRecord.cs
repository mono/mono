//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System;
    using System.Xml;    

    class StringTraceRecord : TraceRecord
    {
        string elementName;
        string content;

        internal StringTraceRecord(string elementName, string content)
        {
            this.elementName = elementName;
            this.content = content;
        }

        internal override string EventId
        {
            get { return BuildEventId("String"); }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            writer.WriteElementString(elementName, content);
        }
    }
}
