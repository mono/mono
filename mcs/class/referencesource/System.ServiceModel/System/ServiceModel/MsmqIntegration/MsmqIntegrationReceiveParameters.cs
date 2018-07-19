//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;

    sealed class MsmqIntegrationReceiveParameters
        : MsmqReceiveParameters
    {
        MsmqMessageSerializationFormat serializationFormat;
        Type[] targetSerializationTypes;

        internal MsmqIntegrationReceiveParameters(MsmqIntegrationBindingElement bindingElement)
            : base(bindingElement)
        {
            this.serializationFormat = bindingElement.SerializationFormat;

            List<Type> knownTypes = new List<Type>();
            if (null != bindingElement.TargetSerializationTypes)
            {
                foreach (Type type in bindingElement.TargetSerializationTypes)
                    if (! knownTypes.Contains(type))
                        knownTypes.Add(type);
            }
            this.targetSerializationTypes = knownTypes.ToArray();
        }

        internal MsmqMessageSerializationFormat SerializationFormat
        {
            get { return this.serializationFormat; }
        }

        internal Type[] TargetSerializationTypes
        {
            get { return this.targetSerializationTypes; }
        }
    }
}
