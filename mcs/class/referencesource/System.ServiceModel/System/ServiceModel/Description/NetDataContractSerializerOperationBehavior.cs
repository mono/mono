//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Xml;

    internal class NetDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        internal NetDataContractSerializerOperationBehavior(OperationDescription operation)
            : base(operation)
        {
        }

        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer(name, ns);
        }

        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer(name, ns);
        }

        internal static NetDataContractSerializerOperationBehavior ApplyTo(OperationDescription operation)
        {
            NetDataContractSerializerOperationBehavior netDataContractSerializerOperationBehavior = null;
            DataContractSerializerOperationBehavior dataContractSerializerOperationBehavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();

            if (dataContractSerializerOperationBehavior != null)
            {
                netDataContractSerializerOperationBehavior = new NetDataContractSerializerOperationBehavior(operation);
                operation.Behaviors.Remove(dataContractSerializerOperationBehavior);
                operation.Behaviors.Add(netDataContractSerializerOperationBehavior);

                return netDataContractSerializerOperationBehavior;
            }

            return null;
        }
    }
}
