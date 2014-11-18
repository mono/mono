//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.CompilerServices;

#if USE_REFEMIT
    public sealed class SurrogateDataContract : DataContract
#else
    internal sealed class SurrogateDataContract : DataContract
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        SurrogateDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal SurrogateDataContract(Type type, ISerializationSurrogate serializationSurrogate)
            : base(new SurrogateDataContractCriticalHelper(type, serializationSurrogate))
        {
            helper = base.Helper as SurrogateDataContractCriticalHelper;
        }

        internal ISerializationSurrogate SerializationSurrogate
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical serializationSurrogate property.",
                Safe = "serializationSurrogate only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.SerializationSurrogate; }
        }

        public override void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            SerializationInfo serInfo = new SerializationInfo(UnderlyingType, XmlObjectSerializer.FormatterConverter, !context.UnsafeTypeForwardingEnabled);
            SerializationSurrogateGetObjectData(obj, serInfo, context.GetStreamingContext());
            context.WriteSerializationInfo(xmlWriter, UnderlyingType, serInfo);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls the critical methods of ISurrogateSelector", Safe = "Demands for FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        object SerializationSurrogateSetObjectData(object obj, SerializationInfo serInfo, StreamingContext context)
        {
            return SerializationSurrogate.SetObjectData(obj, serInfo, context, null);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls the critical methods of IObjectReference", Safe = "Demands for FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static object GetRealObject(IObjectReference obj, StreamingContext context)
        {
            return obj.GetRealObject(context);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls the critical methods of FormatterServices", Safe = "Demands for FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        object GetUninitializedObject(Type objType)
        {
            return FormatterServices.GetUninitializedObject(objType);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls the critical methods of ISerializationSurrogate", Safe = "Demands for FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        void SerializationSurrogateGetObjectData(object obj, SerializationInfo serInfo, StreamingContext context)
        {
            SerializationSurrogate.GetObjectData(obj, serInfo, context);
        }

        public override object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            xmlReader.Read();
            Type objType = UnderlyingType;
            object obj = objType.IsArray ? Array.CreateInstance(objType.GetElementType(), 0) : GetUninitializedObject(objType);
            context.AddNewObject(obj);
            string objectId = context.GetObjectId();
            SerializationInfo serInfo = context.ReadSerializationInfo(xmlReader, objType);
            object newObj = SerializationSurrogateSetObjectData(obj, serInfo, context.GetStreamingContext());
            if (newObj == null)
                newObj = obj;
            if (newObj is IDeserializationCallback)
                ((IDeserializationCallback)newObj).OnDeserialization(null);
            if (newObj is IObjectReference)
                newObj = GetRealObject((IObjectReference)newObj, context.GetStreamingContext());
            context.ReplaceDeserializedObject(objectId, obj, newObj);
            xmlReader.ReadEndElement();
            return newObj;
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for for (de)serializing with ISerializationSurrogate."
            + " Since it accesses data on the base type that is cached statically, we lock down access to it.")]
        [SecurityCritical(SecurityCriticalScope.Everything)]
        class SurrogateDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            ISerializationSurrogate serializationSurrogate;

            internal SurrogateDataContractCriticalHelper(Type type, ISerializationSurrogate serializationSurrogate)
                : base(type)
            {
                this.serializationSurrogate = serializationSurrogate;
                string name, ns;
                DataContract.GetDefaultStableName(DataContract.GetClrTypeFullName(type), out name, out ns);
                SetDataContractName(CreateQualifiedName(name, ns));
            }

            internal ISerializationSurrogate SerializationSurrogate
            {
                get { return serializationSurrogate; }
            }
        }
    }
}

