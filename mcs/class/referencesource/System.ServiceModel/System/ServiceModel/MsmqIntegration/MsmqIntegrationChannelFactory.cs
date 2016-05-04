//------------------------------------------------------------  
// Copyright (c) Microsoft Corporation.  All rights reserved.   
//------------------------------------------------------------  
namespace System.ServiceModel.MsmqIntegration
{
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Runtime.Serialization.Formatters.Binary; // for BinaryFormatter
    using System.Xml.Serialization; // for XmlSerializer 
    using System.IO; // for Stream
    using System.Collections.Specialized; //For HybridDictionary
    using System.ServiceModel.Channels;

    sealed class MsmqIntegrationChannelFactory : MsmqChannelFactoryBase<IOutputChannel>
    {
        ActiveXSerializer activeXSerializer;
        BinaryFormatter binaryFormatter;
        MsmqMessageSerializationFormat serializationFormat;
        HybridDictionary xmlSerializerTable;
        const int maxSerializerTableSize = 1024;

        internal MsmqIntegrationChannelFactory(MsmqIntegrationBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context, null)
        {
            this.serializationFormat = bindingElement.SerializationFormat;
        }

        BinaryFormatter BinaryFormatter
        {
            get
            {
                if (this.binaryFormatter == null)
                {
                    lock (ThisLock)
                    {
                        if (this.binaryFormatter == null)
                            this.binaryFormatter = new BinaryFormatter();
                    }
                }

                return this.binaryFormatter;
            }
        }


        ActiveXSerializer ActiveXSerializer
        {
            get
            {
                if (this.activeXSerializer == null)
                {
                    lock (ThisLock)
                    {
                        if (this.activeXSerializer == null)
                            this.activeXSerializer = new ActiveXSerializer();
                    }
                }

                return this.activeXSerializer;
            }
        }


        XmlSerializer GetXmlSerializerForType(Type serializedType)
        {
            if (this.xmlSerializerTable == null)
            {
                lock (ThisLock)
                {
                    if (this.xmlSerializerTable == null)
                        this.xmlSerializerTable = new HybridDictionary();
                }
            }

            XmlSerializer serializer = (XmlSerializer)this.xmlSerializerTable[serializedType];
            if (serializer != null)
            {
                return serializer;
            }

            lock (ThisLock)
            {
                if (this.xmlSerializerTable.Count >= maxSerializerTableSize)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new CommunicationException(SR.GetString(SR.MsmqSerializationTableFull, maxSerializerTableSize)));
                // double-locking
                serializer = (XmlSerializer)this.xmlSerializerTable[serializedType];
                if (serializer != null)
                {
                    return serializer;
                }

                serializer = new XmlSerializer(serializedType);
                this.xmlSerializerTable[serializedType] = serializer;

                return serializer;
            }
        }


        public MsmqMessageSerializationFormat SerializationFormat
        {
            get
            {
                ThrowIfDisposed();
                return this.serializationFormat;
            }
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            base.ValidateScheme(via);
            return new MsmqIntegrationOutputChannel(this, to, via, ManualAddressing);
        }

        //
        // Returns stream containing serialized body
        // In case of MsmqMessageSerializationFormat.Stream,
        // returns body as Stream, and throws exception if body is not a Stream
        //
        internal Stream Serialize(MsmqIntegrationMessageProperty property)
        {
            Stream stream;
            switch (this.SerializationFormat)
            {
                case MsmqMessageSerializationFormat.Xml:
                    stream = new MemoryStream();
                    XmlSerializer serializer = GetXmlSerializerForType(property.Body.GetType());
                    serializer.Serialize(stream, property.Body);
                    return stream;

                case MsmqMessageSerializationFormat.Binary:
                    stream = new MemoryStream();
                    BinaryFormatter.Serialize(stream, property.Body);
                    // need to set BodyType to a magic number used by System.Messaging
                    property.BodyType = 0x300;
                    return stream;

                case MsmqMessageSerializationFormat.ActiveX:
                    if (property.BodyType.HasValue)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MsmqCannotUseBodyTypeWithActiveXSerialization)));
                    }

                    stream = new MemoryStream();
                    int bodyType = 0;
                    ActiveXSerializer.Serialize(stream as MemoryStream, property.Body, ref bodyType);
                    property.BodyType = bodyType;
                    return stream;

                case MsmqMessageSerializationFormat.ByteArray:
                    // body MUST be byte array
                    byte[] byteArray = property.Body as byte[];
                    if (byteArray == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqByteArrayBodyExpected)));

                    stream = new MemoryStream();
                    stream.Write(byteArray, 0, byteArray.Length);
                    return stream;

                case MsmqMessageSerializationFormat.Stream:
                    // body MUST be a stream
                    Stream bodyStream = property.Body as Stream;
                    if (bodyStream == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqStreamBodyExpected)));

                    // NOTE: we don't copy here as a perf optimization, but this might be dangerous
                    return bodyStream;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.MsmqUnsupportedSerializationFormat, this.SerializationFormat)));
            }

        }

    }
}

