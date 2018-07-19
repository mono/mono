//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

namespace System.ServiceModel.Dispatcher
{
    class DataContractSerializerFaultFormatter : FaultFormatter
    {
        internal DataContractSerializerFaultFormatter(Type[] detailTypes)
            : base(detailTypes)
        {
        }

        internal DataContractSerializerFaultFormatter(SynchronizedCollection<FaultContractInfo> faultContractInfoCollection)
            : base(faultContractInfoCollection)
        {
        }
    }
}
