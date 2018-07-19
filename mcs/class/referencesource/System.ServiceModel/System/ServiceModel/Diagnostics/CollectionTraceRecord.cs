//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections;
    using System.Runtime.Diagnostics;
    using System.Xml;

    class CollectionTraceRecord : TraceRecord
    {
        IEnumerable entries;
        string collectionName;
        string elementName;

        public CollectionTraceRecord(string collectionName, string elementName, IEnumerable entries)
        {
            this.collectionName = String.IsNullOrEmpty(collectionName) ? "Elements" : collectionName;
            this.elementName = String.IsNullOrEmpty(elementName) ? "Element" : elementName;
            this.entries = entries;
        }

        internal override string EventId { get { return BuildEventId("Collection"); } }

        internal override void WriteTo(XmlWriter xml)
        {
            if (this.entries != null)
            {
                xml.WriteStartElement(this.collectionName);
                foreach (object element in this.entries)
                {
                    xml.WriteElementString(this.elementName, element == null ? "null" : element.ToString());
                }
                xml.WriteEndElement();
            }
        }
    }

}
