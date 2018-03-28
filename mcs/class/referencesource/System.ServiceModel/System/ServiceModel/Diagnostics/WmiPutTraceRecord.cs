//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Xml;
    
    class WmiPutTraceRecord : TraceRecord
    {
        string originalValue;
        string newValue;
        string valueName;

        internal WmiPutTraceRecord(string valueName, object originalValue, object newValue)
        {
            Fx.Assert(!String.IsNullOrEmpty(valueName), "valueName must be set");
            this.valueName = valueName;
            this.originalValue = originalValue == null ? SR.GetString(SR.ConfigNull) : originalValue.ToString();
            this.newValue = newValue == null ? SR.GetString(SR.ConfigNull) : newValue.ToString();
        }

        internal override string EventId { get { return BuildEventId("WmiPut"); } }

        internal override void WriteTo(XmlWriter xml)
        {
            xml.WriteElementString("ValueName", this.valueName);
            xml.WriteElementString("OriginalValue", this.originalValue);
            xml.WriteElementString("NewValue", this.newValue);
        }
    }
}
