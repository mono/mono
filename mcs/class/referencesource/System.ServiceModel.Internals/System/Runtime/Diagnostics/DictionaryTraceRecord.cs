//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Diagnostics
{
    using System.Xml;
    using System.Collections;

    class DictionaryTraceRecord : TraceRecord
    {
        IDictionary dictionary;

        internal DictionaryTraceRecord(IDictionary dictionary)
        {
            this.dictionary = dictionary;
        }

        internal override string EventId { get { return TraceRecord.EventIdBase + "Dictionary" + TraceRecord.NamespaceSuffix; } }

        internal override void WriteTo(XmlWriter xml)
        {
            if (this.dictionary != null)
            {
                foreach (object key in this.dictionary.Keys)
                {
                    object value = this.dictionary[key];
                    xml.WriteElementString(key.ToString(), value == null ? string.Empty : value.ToString());
                }
            }
        }
    }
}
