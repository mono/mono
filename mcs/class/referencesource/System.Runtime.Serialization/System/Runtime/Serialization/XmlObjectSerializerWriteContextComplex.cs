//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Collections.Generic;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Diagnostics.Application;

#if USE_REFEMIT
    public class XmlObjectSerializerWriteContextComplex : XmlObjectSerializerWriteContext
#else
    internal class XmlObjectSerializerWriteContextComplex : XmlObjectSerializerWriteContext
#endif
    {
        protected IDataContractSurrogate dataContractSurrogate;
        SerializationMode mode;
        SerializationBinder binder;
        ISurrogateSelector surrogateSelector;
        StreamingContext streamingContext;
        Hashtable surrogateDataContracts;

        internal XmlObjectSerializerWriteContextComplex(DataContractSerializer serializer, DataContract rootTypeDataContract, DataContractResolver dataContractResolver)
            : base(serializer, rootTypeDataContract, dataContractResolver)
        {
            this.mode = SerializationMode.SharedContract;
            this.preserveObjectReferences = serializer.PreserveObjectReferences;
            this.dataContractSurrogate = serializer.DataContractSurrogate;
        }

        internal XmlObjectSerializerWriteContextComplex(NetDataContractSerializer serializer, Hashtable surrogateDataContracts)
            : base(serializer)
        {
            this.mode = SerializationMode.SharedType;
            this.preserveObjectReferences = true;
            this.streamingContext = serializer.Context;
            this.binder = serializer.Binder;
            this.surrogateSelector = serializer.SurrogateSelector;
            this.surrogateDataContracts = surrogateDataContracts;
        }

        internal XmlObjectSerializerWriteContextComplex(XmlObjectSerializer serializer, int maxItemsInObjectGraph, StreamingContext streamingContext, bool ignoreExtensionDataObject)
            : base(serializer, maxItemsInObjectGraph, streamingContext, ignoreExtensionDataObject)
        {
        }

        internal override SerializationMode Mode
        {
            get { return mode; }
        }

        internal override DataContract GetDataContract(RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract dataContract = null;
            if (mode == SerializationMode.SharedType && surrogateSelector != null)
            {
                dataContract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(surrogateSelector, streamingContext, typeHandle, type, ref surrogateDataContracts);
            }

            if (dataContract != null)
            {
                if (this.IsGetOnlyCollection && dataContract is SurrogateDataContract)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser,
                        DataContract.GetClrTypeFullName(dataContract.UnderlyingType))));
                }
                return dataContract;
            }

            return base.GetDataContract(typeHandle, type);
        }

        internal override DataContract GetDataContract(int id, RuntimeTypeHandle typeHandle)
        {
            DataContract dataContract = null;
            if (mode == SerializationMode.SharedType && surrogateSelector != null)
            {
                dataContract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(surrogateSelector, streamingContext, typeHandle, null /*type*/, ref surrogateDataContracts);
            }

            if (dataContract != null)
            {
                if (this.IsGetOnlyCollection && dataContract is SurrogateDataContract)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser,
                        DataContract.GetClrTypeFullName(dataContract.UnderlyingType))));
                }
                return dataContract;
            }

            return base.GetDataContract(id, typeHandle);
        }

        internal override DataContract GetDataContractSkipValidation(int typeId, RuntimeTypeHandle typeHandle, Type type)
        {
            DataContract dataContract = null;
            if (mode == SerializationMode.SharedType && surrogateSelector != null)
            {
                dataContract = NetDataContractSerializer.GetDataContractFromSurrogateSelector(surrogateSelector, streamingContext, typeHandle, null /*type*/, ref surrogateDataContracts);
            }

            if (dataContract != null)
            {
                if (this.IsGetOnlyCollection && dataContract is SurrogateDataContract)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser,
                        DataContract.GetClrTypeFullName(dataContract.UnderlyingType))));
                }
                return dataContract;
            }

            return base.GetDataContractSkipValidation(typeId, typeHandle, type);
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, DataContract dataContract)
        {
            if (mode == SerializationMode.SharedType)
            {
                NetDataContractSerializer.WriteClrTypeInfo(xmlWriter, dataContract, binder);
                return true;
            }
            return false;
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, Type dataContractType, string clrTypeName, string clrAssemblyName)
        {
            if (mode == SerializationMode.SharedType)
            {
                NetDataContractSerializer.WriteClrTypeInfo(xmlWriter, dataContractType, binder, clrTypeName, clrAssemblyName);
                return true;
            }
            return false;
        }

        internal override bool WriteClrTypeInfo(XmlWriterDelegator xmlWriter, Type dataContractType, SerializationInfo serInfo)
        {
            if (mode == SerializationMode.SharedType)
            {
                NetDataContractSerializer.WriteClrTypeInfo(xmlWriter, dataContractType, binder, serInfo);
                return true;
            }
            return false;
        }

        public override void WriteAnyType(XmlWriterDelegator xmlWriter, object value)
        {
            if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                xmlWriter.WriteAnyType(value);
        }

        public override void WriteString(XmlWriterDelegator xmlWriter, string value)
        {
            if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                xmlWriter.WriteString(value);
        }
        public override void WriteString(XmlWriterDelegator xmlWriter, string value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
                WriteNull(xmlWriter, typeof(string), true/*isMemberTypeSerializable*/, name, ns);
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                    xmlWriter.WriteString(value);
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        public override void WriteBase64(XmlWriterDelegator xmlWriter, byte[] value)
        {
            if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                xmlWriter.WriteBase64(value);
        }
        public override void WriteBase64(XmlWriterDelegator xmlWriter, byte[] value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
                WriteNull(xmlWriter, typeof(byte[]), true/*isMemberTypeSerializable*/, name, ns);
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                    xmlWriter.WriteBase64(value);
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        public override void WriteUri(XmlWriterDelegator xmlWriter, Uri value)
        {
            if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                xmlWriter.WriteUri(value);
        }
        public override void WriteUri(XmlWriterDelegator xmlWriter, Uri value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
                WriteNull(xmlWriter, typeof(Uri), true/*isMemberTypeSerializable*/, name, ns);
            else
            {
                xmlWriter.WriteStartElementPrimitive(name, ns);
                if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                    xmlWriter.WriteUri(value);
                xmlWriter.WriteEndElementPrimitive();
            }
        }

        public override void WriteQName(XmlWriterDelegator xmlWriter, XmlQualifiedName value)
        {
            if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                xmlWriter.WriteQName(value);
        }
        public override void WriteQName(XmlWriterDelegator xmlWriter, XmlQualifiedName value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (value == null)
                WriteNull(xmlWriter, typeof(XmlQualifiedName), true/*isMemberTypeSerializable*/, name, ns);
            else
            {
                if (ns != null && ns.Value != null && ns.Value.Length > 0)
                    xmlWriter.WriteStartElement(Globals.ElementPrefix, name, ns);
                else
                    xmlWriter.WriteStartElement(name, ns);
                if (!OnHandleReference(xmlWriter, value, false /*canContainCyclicReference*/))
                    xmlWriter.WriteQName(value);
                xmlWriter.WriteEndElement();
            }
        }

        public override void InternalSerialize(XmlWriterDelegator xmlWriter, object obj, bool isDeclaredType, bool writeXsiType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle)
        {
            if (dataContractSurrogate == null)
            {
                base.InternalSerialize(xmlWriter, obj, isDeclaredType, writeXsiType, declaredTypeID, declaredTypeHandle);
            }
            else
            {
                InternalSerializeWithSurrogate(xmlWriter, obj, isDeclaredType, writeXsiType, declaredTypeID, declaredTypeHandle);
            }
        }

        internal override bool OnHandleReference(XmlWriterDelegator xmlWriter, object obj, bool canContainCyclicReference)
        {
            if (preserveObjectReferences && !this.IsGetOnlyCollection)
            {
                bool isNew = true;
                int objectId = SerializedObjects.GetId(obj, ref isNew);
                if (isNew)
                    xmlWriter.WriteAttributeInt(Globals.SerPrefix, DictionaryGlobals.IdLocalName, DictionaryGlobals.SerializationNamespace, objectId);
                else
                {
                    xmlWriter.WriteAttributeInt(Globals.SerPrefix, DictionaryGlobals.RefLocalName, DictionaryGlobals.SerializationNamespace, objectId);
                    xmlWriter.WriteAttributeBool(Globals.XsiPrefix, DictionaryGlobals.XsiNilLocalName, DictionaryGlobals.SchemaInstanceNamespace, true);
                }
                return !isNew;
            }
            return base.OnHandleReference(xmlWriter, obj, canContainCyclicReference);
        }

        internal override void OnEndHandleReference(XmlWriterDelegator xmlWriter, object obj, bool canContainCyclicReference)
        {
            if (preserveObjectReferences && !this.IsGetOnlyCollection)
                return;
            base.OnEndHandleReference(xmlWriter, obj, canContainCyclicReference);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls the critical methods of ISurrogateSelector", Safe = "Demands for FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        bool CheckIfTypeSerializableForSharedTypeMode(Type memberType)
        {
            Fx.Assert(surrogateSelector != null, "Method should not be called when surrogateSelector is null.");
            ISurrogateSelector surrogateSelectorNotUsed;
            return (surrogateSelector.GetSurrogate(memberType, streamingContext, out surrogateSelectorNotUsed) != null);
        }

        internal override void CheckIfTypeSerializable(Type memberType, bool isMemberTypeSerializable)
        {
            if (mode == SerializationMode.SharedType && surrogateSelector != null &&
                CheckIfTypeSerializableForSharedTypeMode(memberType))
            {
                return;
            }
            else
            {
                if (dataContractSurrogate != null)
                {
                    while (memberType.IsArray)
                        memberType = memberType.GetElementType();
                    memberType = DataContractSurrogateCaller.GetDataContractType(dataContractSurrogate, memberType);
                    if (!DataContract.IsTypeSerializable(memberType))
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.TypeNotSerializable, memberType)));
                    return;
                }
            }

            base.CheckIfTypeSerializable(memberType, isMemberTypeSerializable);
        }

        internal override Type GetSurrogatedType(Type type)
        {
            if (dataContractSurrogate == null)
            {
                return base.GetSurrogatedType(type);
            }
            else
            {
                type = DataContract.UnwrapNullableType(type);
                Type surrogateType = DataContractSerializer.GetSurrogatedType(dataContractSurrogate, type);
                if (this.IsGetOnlyCollection && surrogateType != type)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.SurrogatesWithGetOnlyCollectionsNotSupportedSerDeser,
                        DataContract.GetClrTypeFullName(type))));
                }
                else
                {
                    return surrogateType;
                }
            }
        }

        void InternalSerializeWithSurrogate(XmlWriterDelegator xmlWriter, object obj, bool isDeclaredType, bool writeXsiType, int declaredTypeID, RuntimeTypeHandle declaredTypeHandle)
        {
            RuntimeTypeHandle objTypeHandle = isDeclaredType ? declaredTypeHandle : Type.GetTypeHandle(obj);
            object oldObj = obj;
            int objOldId = 0;
            Type objType = Type.GetTypeFromHandle(objTypeHandle);
            Type declaredType = GetSurrogatedType(Type.GetTypeFromHandle(declaredTypeHandle));

            if (TD.DCSerializeWithSurrogateStartIsEnabled())
            {
                TD.DCSerializeWithSurrogateStart(declaredType.FullName);
            }

            declaredTypeHandle = declaredType.TypeHandle;
            obj = DataContractSerializer.SurrogateToDataContractType(dataContractSurrogate, obj, declaredType, ref objType);
            objTypeHandle = objType.TypeHandle;
            if (oldObj != obj)
                objOldId = SerializedObjects.ReassignId(0, oldObj, obj);

            if (writeXsiType)
            {
                declaredType = Globals.TypeOfObject;
                SerializeWithXsiType(xmlWriter, obj, objTypeHandle, objType, -1, declaredType.TypeHandle, declaredType);
            }
            else if (declaredTypeHandle.Equals(objTypeHandle))
            {
                DataContract contract = GetDataContract(objTypeHandle, objType);
                SerializeWithoutXsiType(contract, xmlWriter, obj, declaredTypeHandle);
            }
            else
            {
                SerializeWithXsiType(xmlWriter, obj, objTypeHandle, objType, -1, declaredTypeHandle, declaredType);
            }
            if (oldObj != obj)
                SerializedObjects.ReassignId(objOldId, obj, oldObj);


            if (TD.DCSerializeWithSurrogateStopIsEnabled())
            {
                TD.DCSerializeWithSurrogateStop();
            }
        }

        internal override void WriteArraySize(XmlWriterDelegator xmlWriter, int size)
        {
            if (preserveObjectReferences && size > -1)
                xmlWriter.WriteAttributeInt(Globals.SerPrefix, DictionaryGlobals.ArraySizeLocalName, DictionaryGlobals.SerializationNamespace, size);
        }

    }

}
