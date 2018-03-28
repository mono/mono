//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;
    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;

    class UnwrappedTypesXmlSerializerManager
    {
        Dictionary<Type, XmlTypeMapping> allTypes;
        XmlReflectionImporter importer;
        Dictionary<Object, IList<Type>> operationTypes;
        bool serializersCreated;
        Dictionary<Type, XmlSerializer> serializersMap;
        Object thisLock;

        public UnwrappedTypesXmlSerializerManager()
        {
            this.allTypes = new Dictionary<Type, XmlTypeMapping>();
            this.serializersMap = new Dictionary<Type, XmlSerializer>();
            this.operationTypes = new Dictionary<Object, IList<Type>>();
            importer = new XmlReflectionImporter();
            this.thisLock = new Object();
        }

        public TypeSerializerPair[] GetOperationSerializers(Object key)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            lock (thisLock)
            {
                if (!this.serializersCreated)
                {
                    BuildSerializers();
                    this.serializersCreated = true;
                }
                List<TypeSerializerPair> serializers = new List<TypeSerializerPair>();
                IList<Type> operationTypes = this.operationTypes[key];
                for (int i = 0; i < operationTypes.Count; ++i)
                {
                    TypeSerializerPair pair = new TypeSerializerPair();
                    pair.Type = operationTypes[i];
                    pair.Serializer = new XmlSerializerXmlObjectSerializer(serializersMap[operationTypes[i]]);
                    serializers.Add(pair);
                }
                return serializers.ToArray();
            }
        }

        public void RegisterType(Object key, IList<Type> types)
        {
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            if (types == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("types");
            }
            lock (thisLock)
            {
                if (this.serializersCreated)
                {
                    Fx.Assert("An xml serializer type was added after the serializers were created");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.XmlSerializersCreatedBeforeRegistration)));
                }
                for (int i = 0; i < types.Count; ++i)
                {
                    if (!allTypes.ContainsKey(types[i]))
                    {
                        allTypes.Add(types[i], importer.ImportTypeMapping(types[i]));
                    }
                }
                operationTypes.Add(key, types);
            }
        }

        void BuildSerializers()
        {
            List<Type> types = new List<Type>();
            List<XmlMapping> mappings = new List<XmlMapping>();
            foreach (Type type in allTypes.Keys)
            {
                XmlTypeMapping mapping = allTypes[type];
                types.Add(type);
                mappings.Add(mapping);
            }
            XmlSerializer[] serializers = XmlSerializer.FromMappings(mappings.ToArray());
            for (int i = 0; i < types.Count; ++i)
            {
                serializersMap.Add(types[i], serializers[i]);
            }
        }

        public struct TypeSerializerPair
        {
            public XmlObjectSerializer Serializer;
            public Type Type;
        }

        class XmlSerializerXmlObjectSerializer : XmlObjectSerializer
        {
            XmlSerializer serializer;

            public XmlSerializerXmlObjectSerializer(XmlSerializer serializer)
            {
                if (serializer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
                }
                this.serializer = serializer;
            }

            public override bool IsStartObject(XmlDictionaryReader reader)
            {
                return this.serializer.CanDeserialize(reader);
            }

            public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
            {
                return this.serializer.Deserialize(reader);
            }

            public override void WriteEndObject(XmlDictionaryWriter writer)
            {
                Fx.Assert("This method should never get hit");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override void WriteObject(XmlDictionaryWriter writer, object graph)
            {
                this.serializer.Serialize(writer, graph);
            }

            public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
            {
                Fx.Assert("This method should never get hit");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
            {
                Fx.Assert("This method should never get hit");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }
    }
}

