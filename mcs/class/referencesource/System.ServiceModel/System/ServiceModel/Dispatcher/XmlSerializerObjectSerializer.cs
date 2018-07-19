//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml;
    using System.ServiceModel;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel.Description;

    internal class XmlSerializerObjectSerializer : XmlObjectSerializer
    {
        XmlSerializer serializer;
        Type rootType;
        string rootName;
        string rootNamespace;
        bool isSerializerSetExplicit = false;

        internal XmlSerializerObjectSerializer(Type type)
        {
            Initialize(type, null /*rootName*/, null /*rootNamespace*/, null /*xmlSerializer*/);
        }

        internal XmlSerializerObjectSerializer(Type type, XmlQualifiedName qualifiedName, XmlSerializer xmlSerializer)
        {
            if (qualifiedName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("qualifiedName");
            }
            Initialize(type, qualifiedName.Name, qualifiedName.Namespace, xmlSerializer);
        }

        void Initialize(Type type, string rootName, string rootNamespace, XmlSerializer xmlSerializer)
        {
            if (type == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("type");
            }
            this.rootType = type;
            this.rootName = rootName;
            this.rootNamespace = rootNamespace == null ? string.Empty : rootNamespace;
            this.serializer = xmlSerializer;

            if (this.serializer == null)
            {
                if (this.rootName == null)
                    this.serializer = new XmlSerializer(type);
                else
                {
                    XmlRootAttribute xmlRoot = new XmlRootAttribute();
                    xmlRoot.ElementName = this.rootName;
                    xmlRoot.Namespace = this.rootNamespace;
                    this.serializer = new XmlSerializer(type, xmlRoot);
                }
            }
            else
                isSerializerSetExplicit = true;

            //try to get rootName and rootNamespace from type since root name not set explicitly
            if (this.rootName == null)
            {
                XmlTypeMapping mapping = new XmlReflectionImporter().ImportTypeMapping(this.rootType);
                this.rootName = mapping.ElementName;
                this.rootNamespace = mapping.Namespace;
            }
        }

        public override void WriteObject(XmlDictionaryWriter writer, object graph)
        {
            if (this.isSerializerSetExplicit)
                this.serializer.Serialize(writer, new object[] { graph });
            else
                this.serializer.Serialize(writer, graph);
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            if (this.isSerializerSetExplicit)
            {
                object[] deserializedObjects = (object[])this.serializer.Deserialize(reader);
                if (deserializedObjects != null && deserializedObjects.Length > 0)
                    return deserializedObjects[0];
                else
                    return null;
            }
            else
                return this.serializer.Deserialize(reader);
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            reader.MoveToElement();

            if (this.rootName != null)
            {
                return reader.IsStartElement(this.rootName, this.rootNamespace);
            }
            else
            {
                return reader.IsStartElement();
            }
        }
    }
}

