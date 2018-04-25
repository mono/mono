//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System.Security;
    using System.Xml;

    class Attributes
    {
        [Fx.Tag.SecurityNote(Critical = "Static field used to store the attribute names to read during deserialization."
            + " Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static XmlDictionaryString[] serializationLocalNames;

        [Fx.Tag.SecurityNote(Critical = "Static field used to store the attribute names to read during deserialization."
            + " Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static XmlDictionaryString[] schemaInstanceLocalNames;

        [Fx.Tag.SecurityNote(Critical = "Initializes critical static fields.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        static Attributes()
        {
            serializationLocalNames = new XmlDictionaryString[]
            {
                DictionaryGlobals.IdLocalName,
                DictionaryGlobals.ArraySizeLocalName,
                DictionaryGlobals.RefLocalName,
                DictionaryGlobals.ClrTypeLocalName,
                DictionaryGlobals.ClrAssemblyLocalName,
                DictionaryGlobals.ISerializableFactoryTypeLocalName
            };

            schemaInstanceLocalNames = new XmlDictionaryString[]
            {
                DictionaryGlobals.XsiNilLocalName,
                DictionaryGlobals.XsiTypeLocalName
            };
        }

        internal string Id;
        internal string Ref;
        internal string XsiTypeName;
        internal string XsiTypeNamespace;
        internal string XsiTypePrefix;
        internal bool XsiNil;
        internal string ClrAssembly;
        internal string ClrType;
        internal int ArraySZSize;
        internal string FactoryTypeName;
        internal string FactoryTypeNamespace;
        internal string FactoryTypePrefix;
        internal bool UnrecognizedAttributesFound;

        [SecuritySafeCritical]
        internal void Read(XmlReaderDelegator reader)
        {
            Reset();

            while (reader.MoveToNextAttribute())
            {
                switch (reader.IndexOfLocalName(serializationLocalNames, DictionaryGlobals.SerializationNamespace))
                {
                    case 0:
                        ReadId(reader);
                        break;
                    case 1:
                        ReadArraySize(reader);
                        break;
                    case 2:
                        ReadRef(reader);
                        break;
                    case 3:
                        ClrType = reader.Value;
                        break;
                    case 4:
                        ClrAssembly = reader.Value;
                        break;
                    case 5:
                        ReadFactoryType(reader);
                        break;
                    default:
                        switch (reader.IndexOfLocalName(schemaInstanceLocalNames, DictionaryGlobals.SchemaInstanceNamespace))
                        {
                            case 0:
                                ReadXsiNil(reader);
                                break;
                            case 1:
                                ReadXsiType(reader);
                                break;
                            default:
                                if (!reader.IsNamespaceUri(DictionaryGlobals.XmlnsNamespace))
                                    UnrecognizedAttributesFound = true;
                                break;
                        }
                        break;
                }
            }
            reader.MoveToElement();
        }

        internal void Reset()
        {
            Id = Globals.NewObjectId;
            Ref = Globals.NewObjectId;
            XsiTypeName = null;
            XsiTypeNamespace = null;
            XsiTypePrefix = null;
            XsiNil = false;
            ClrAssembly = null;
            ClrType = null;
            ArraySZSize = -1;
            FactoryTypeName = null;
            FactoryTypeNamespace = null;
            FactoryTypePrefix = null;
            UnrecognizedAttributesFound = false;
        }

        void ReadId(XmlReaderDelegator reader)
        {
            Id = reader.ReadContentAsString();
            if (string.IsNullOrEmpty(Id))
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.InvalidXsIdDefinition, Id)));
            }
        }

        void ReadRef(XmlReaderDelegator reader)
        {
            Ref = reader.ReadContentAsString();
            if (string.IsNullOrEmpty(Ref))
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.InvalidXsRefDefinition, Ref)));
            }
        }

        void ReadXsiNil(XmlReaderDelegator reader)
        {
            XsiNil = reader.ReadContentAsBoolean();
        }

        void ReadArraySize(XmlReaderDelegator reader)
        {
            ArraySZSize = reader.ReadContentAsInt();
            if (ArraySZSize < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.InvalidSizeDefinition, ArraySZSize)));
        }

        void ReadXsiType(XmlReaderDelegator reader)
        {
            string xsiTypeString = reader.Value;
            if (xsiTypeString != null && xsiTypeString.Length > 0)
                XmlObjectSerializerReadContext.ParseQualifiedName(xsiTypeString, reader, out XsiTypeName, out XsiTypeNamespace, out XsiTypePrefix);
        }

        void ReadFactoryType(XmlReaderDelegator reader)
        {
            string factoryTypeString = reader.Value;
            if (factoryTypeString != null && factoryTypeString.Length > 0)
                XmlObjectSerializerReadContext.ParseQualifiedName(factoryTypeString, reader, out FactoryTypeName, out FactoryTypeNamespace, out FactoryTypePrefix);
        }

    }
}
