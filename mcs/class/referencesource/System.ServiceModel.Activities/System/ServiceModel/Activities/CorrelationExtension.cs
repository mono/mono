//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Channels;
    using System.Xml.Linq;

    class CorrelationExtension
    {
        public CorrelationExtension(XName scopeName)
        {
            ScopeName = scopeName;
        }

        public XName ScopeName { get; private set; }

        public InstanceKey GenerateKey(IDictionary<string, string> keyData)
        {
            return new CorrelationKey(keyData, ScopeName.ToString(), null);
        }
    }
}
