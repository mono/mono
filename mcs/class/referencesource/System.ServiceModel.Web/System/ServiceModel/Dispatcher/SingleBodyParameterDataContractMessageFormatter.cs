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
    using System.Runtime.Serialization;
    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;
    using System.Runtime.Serialization.Json;
    using System.Collections;
    using System.Linq;

    class SingleBodyParameterDataContractMessageFormatter : SingleBodyParameterMessageFormatter
    {
        static readonly Type TypeOfNullable = typeof(Nullable<>);
        static readonly Type[] CollectionDataContractInterfaces = new Type[] { typeof(IEnumerable), typeof(IList), typeof(ICollection), typeof(IDictionary) };
        static readonly Type[] GenericCollectionDataContractInterfaces = new Type[] { typeof(IEnumerable<>), typeof(IList<>), typeof(ICollection<>), typeof(IDictionary<,>) };
        XmlObjectSerializer cachedOutputSerializer;
        Type cachedOutputSerializerType;
        bool ignoreExtensionData;
        XmlObjectSerializer[] inputSerializers;
        IList<Type> knownTypes;
        int maxItemsInObjectGraph;
        Type parameterDataContractType;
        IDataContractSurrogate surrogate;
        Object thisLock;
        bool useJsonFormat;
        bool isParameterCollectionInterfaceDataContract;
        bool isQueryable;

        public SingleBodyParameterDataContractMessageFormatter(OperationDescription operation, Type parameterType, bool isRequestFormatter, bool useJsonFormat, DataContractSerializerOperationBehavior dcsob)
            : base(operation, isRequestFormatter, "DataContractSerializer")
        {
            if (operation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operation");
            }
            if (parameterType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterType");
            }
            if (dcsob == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dcsob");
            }
            this.parameterDataContractType = DataContractSerializerOperationFormatter.GetSubstituteDataContractType(parameterType, out isQueryable);
            this.isParameterCollectionInterfaceDataContract = IsTypeCollectionInterface(this.parameterDataContractType);
            List<Type> tmp = new List<Type>();
            if (operation.KnownTypes != null)
            {
                foreach (Type knownType in operation.KnownTypes)
                {
                    tmp.Add(knownType);
                }
            }
            Type nullableType = UnwrapNullableType(this.parameterDataContractType);
            if (nullableType != this.parameterDataContractType)
            {
                tmp.Add(nullableType);
            }
            this.surrogate = dcsob.DataContractSurrogate;
            this.ignoreExtensionData = dcsob.IgnoreExtensionDataObject;
            this.maxItemsInObjectGraph = dcsob.MaxItemsInObjectGraph;
            this.knownTypes = tmp.AsReadOnly();
            ValidateType(this.parameterDataContractType, surrogate, this.knownTypes);

            this.useJsonFormat = useJsonFormat;
            CreateInputSerializers(this.parameterDataContractType);

            thisLock = new Object();
        }

        internal static Type UnwrapNullableType(Type type)
        {
            while (type.IsGenericType && type.GetGenericTypeDefinition() == TypeOfNullable)
            {
                type = type.GetGenericArguments()[0];
            }
            return type;
        }

        // The logic of this method should be kept the same as 
        // System.ServiceModel.Dispatcher.DataContractSerializerOperationFormatter.PartInfo.ReadObject
        protected override object ReadObject(Message message)
        {
            object val = base.ReadObject(message);
            if (this.isQueryable && val != null)
            {
                return Queryable.AsQueryable((IEnumerable)val);
            }
            return val;
        }

        protected override void AttachMessageProperties(Message message, bool isRequest)
        {
            if (this.useJsonFormat)
            {
                message.Properties.Add(WebBodyFormatMessageProperty.Name, WebBodyFormatMessageProperty.JsonProperty);
            }
        }

        protected override XmlObjectSerializer[] GetInputSerializers()
        {
            return this.inputSerializers;
        }

        protected override XmlObjectSerializer GetOutputSerializer(Type type)
        {
            lock (thisLock)
            {
                // if we already have a serializer for this type reuse it
                if (this.cachedOutputSerializerType != type)
                {
                    Type typeForSerializer;
                    if (this.isParameterCollectionInterfaceDataContract)
                    {
                        // if the parameterType is a collection interface, ensure the type implements it
                        if (!this.parameterDataContractType.IsAssignableFrom(type))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR2.GetString(SR2.TypeIsNotParameterTypeAndIsNotPresentInKnownTypes, type, this.OperationName, this.ContractName, parameterDataContractType)));
                        }
                        typeForSerializer = this.parameterDataContractType;
                    }
                    else
                    {
                        typeForSerializer = GetTypeForSerializer(type, this.parameterDataContractType, this.knownTypes);
                    }
                    this.cachedOutputSerializer = CreateSerializer(typeForSerializer);
                    this.cachedOutputSerializerType = type;
                }
                return this.cachedOutputSerializer;
            }
        }

        static bool IsTypeCollectionInterface(Type parameterType)
        {
            if (parameterType.IsGenericType && parameterType.IsInterface)
            {
                Type genericTypeDef = parameterType.GetGenericTypeDefinition();
                foreach (Type type in GenericCollectionDataContractInterfaces)
                {
                    if (genericTypeDef == type)
                    {
                        return true;
                    }
                }
            }
            foreach (Type type in CollectionDataContractInterfaces)
            {
                if (parameterType == type)
                {
                    return true;
                }
            }
            return false;
        }

        protected override void ValidateMessageFormatProperty(Message message)
        {
            if (this.useJsonFormat)
            {
                // useJsonFormat is always false in the green bits
                object prop;
                message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out prop);
                WebBodyFormatMessageProperty formatProperty = (prop as WebBodyFormatMessageProperty);
                if (formatProperty == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.MessageFormatPropertyNotFound, this.OperationName, this.ContractName, this.ContractNs)));
                }
                if (formatProperty.Format != WebContentFormat.Json)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR2.GetString(SR2.InvalidHttpMessageFormat, this.OperationName, this.ContractName, this.ContractNs, formatProperty.Format, WebContentFormat.Json)));
                }
            }
            else
            {
                base.ValidateMessageFormatProperty(message);
            }
        }

        static void ValidateType(Type parameterType, IDataContractSurrogate surrogate, IEnumerable<Type> knownTypes)
        {
            XsdDataContractExporter dataContractExporter = new XsdDataContractExporter();
            if (surrogate != null || knownTypes != null)
            {
                ExportOptions options = new ExportOptions();
                options.DataContractSurrogate = surrogate;
                if (knownTypes != null)
                {
                    foreach (Type knownType in knownTypes)
                    {
                        options.KnownTypes.Add(knownType);
                    }
                }
                dataContractExporter.Options = options;
            }
            dataContractExporter.GetSchemaTypeName(parameterType); // throws if parameterType is not a valid data contract
        }

        void CreateInputSerializers(Type type)
        {
            List<XmlObjectSerializer> tmp = new List<XmlObjectSerializer>();
            tmp.Add(CreateSerializer(type));
            foreach (Type knownType in this.knownTypes)
            {
                tmp.Add(CreateSerializer(knownType));
            }
            this.inputSerializers = tmp.ToArray();
        }

        XmlObjectSerializer CreateSerializer(Type type)
        {
            if (this.useJsonFormat)
            {
                return new DataContractJsonSerializer(type, this.knownTypes, this.maxItemsInObjectGraph, this.ignoreExtensionData, this.surrogate, false);
            }
            else
            {
                return new DataContractSerializer(type, this.knownTypes, this.maxItemsInObjectGraph, this.ignoreExtensionData, false, this.surrogate);
            }
        }


    }
}

