//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.ServiceModel.Dispatcher;

    static class ComPlusTraceRecord
    {

        public static void SerializeRecord (XmlWriter xmlWriter, object o)
        {
            DataContractSerializer serializer = DataContractSerializerDefaults.CreateSerializer(((o == null) ? typeof(object) : o.GetType()), DataContractSerializerDefaults.MaxItemsInObjectGraph);
            serializer.WriteObject(xmlWriter, o);
        }
    }
}
