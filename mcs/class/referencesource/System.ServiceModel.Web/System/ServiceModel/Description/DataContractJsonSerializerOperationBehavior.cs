//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel.Description;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Runtime.Serialization.Json;

    class DataContractJsonSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        bool alwaysEmitTypeInformation;

        public DataContractJsonSerializerOperationBehavior(OperationDescription description, int maxItemsInObjectGraph, bool ignoreExtensionDataObject, IDataContractSurrogate dataContractSurrogate, bool alwaysEmitTypeInformation)
            : base(description)
        {
            this.MaxItemsInObjectGraph = maxItemsInObjectGraph;
            this.IgnoreExtensionDataObject = ignoreExtensionDataObject;
            this.DataContractSurrogate = dataContractSurrogate;
            this.alwaysEmitTypeInformation = alwaysEmitTypeInformation;
        }

        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new DataContractJsonSerializer(type, name, knownTypes, this.MaxItemsInObjectGraph, this.IgnoreExtensionDataObject, this.DataContractSurrogate, alwaysEmitTypeInformation);
        }

        public override XmlObjectSerializer CreateSerializer(Type type, System.Xml.XmlDictionaryString name, System.Xml.XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new DataContractJsonSerializer(type, name, knownTypes, this.MaxItemsInObjectGraph, this.IgnoreExtensionDataObject, this.DataContractSurrogate, alwaysEmitTypeInformation);
        }
    }
}
