//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Xml;
    using System.Runtime.Serialization;
    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;
    using System.Xml.Serialization;

    class SingleBodyParameterXmlSerializerMessageFormatter : SingleBodyParameterMessageFormatter
    {
        XmlObjectSerializer cachedOutputSerializer;
        Type cachedOutputSerializerType;
        List<Type> knownTypes;
        Type parameterType;
        UnwrappedTypesXmlSerializerManager serializerManager;
        XmlObjectSerializer[] serializers;
        Object thisLock;
        UnwrappedTypesXmlSerializerManager.TypeSerializerPair[] typeSerializerPairs;

        public SingleBodyParameterXmlSerializerMessageFormatter(OperationDescription operation, Type parameterType, bool isRequestFormatter, XmlSerializerOperationBehavior xsob, UnwrappedTypesXmlSerializerManager serializerManager)
            : base(operation, isRequestFormatter, "XmlSerializer")
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            if (parameterType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterType");
            }
            if (xsob == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xsob");
            }
            if (serializerManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializerManager");
            }
            this.serializerManager = serializerManager;
            this.parameterType = parameterType;
            List<Type> operationTypes = new List<Type>();
            operationTypes.Add(parameterType);
            this.knownTypes = new List<Type>();
            if (operation.KnownTypes != null)
            {
                foreach (Type knownType in operation.KnownTypes)
                {
                    this.knownTypes.Add(knownType);
                    operationTypes.Add(knownType);
                }
            }
            Type nullableType = SingleBodyParameterDataContractMessageFormatter.UnwrapNullableType(this.parameterType);
            if (nullableType != this.parameterType)
            {
                this.knownTypes.Add(nullableType);
                operationTypes.Add(nullableType);
            }
            this.serializerManager.RegisterType(this, operationTypes);
            thisLock = new Object();
        }

        protected override XmlObjectSerializer[] GetInputSerializers()
        {
            lock (thisLock)
            {
                EnsureSerializers();
                return this.serializers;
            }
        }

        protected override XmlObjectSerializer GetOutputSerializer(Type type)
        {
            lock (thisLock)
            {
                if (this.cachedOutputSerializerType != type)
                {
                    Type typeForSerializer = GetTypeForSerializer(type, this.parameterType, this.knownTypes);
                    EnsureSerializers();
                    bool foundSerializer = false;
                    if (this.typeSerializerPairs != null)
                    {
                        for (int i = 0; i < this.typeSerializerPairs.Length; ++i)
                        {
                            if (typeForSerializer == this.typeSerializerPairs[i].Type)
                            {
                                this.cachedOutputSerializer = this.typeSerializerPairs[i].Serializer;
                                this.cachedOutputSerializerType = type;
                                foundSerializer = true;
                                break;
                            }
                        }
                    }
                    if (!foundSerializer)
                    {
                        return null;
                    }
                }
                return this.cachedOutputSerializer;
            }
        }

        //  must be called under a lock
        void EnsureSerializers()
        {
            if (this.typeSerializerPairs == null)
            {
                this.typeSerializerPairs = this.serializerManager.GetOperationSerializers(this);
                this.serializers = new XmlObjectSerializer[this.typeSerializerPairs.Length];
                for (int i = 0; i < this.typeSerializerPairs.Length; ++i)
                {
                    this.serializers[i] = this.typeSerializerPairs[i].Serializer;
                }
            }
        }
    }
}

