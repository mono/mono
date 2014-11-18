//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.Xml;
    using System.Security;

    class JsonEnumDataContract : JsonDataContract
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + "Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        JsonEnumDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        public JsonEnumDataContract(EnumDataContract traditionalDataContract)
            : base(new JsonEnumDataContractCriticalHelper(traditionalDataContract))
        {
            this.helper = base.Helper as JsonEnumDataContractCriticalHelper;
        }

        public bool IsULong
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsULong property.",
                Safe = "IsULong only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.helper.IsULong; }
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            object enumValue;
            if (IsULong)
            {
                enumValue = Enum.ToObject(TraditionalDataContract.UnderlyingType, jsonReader.ReadElementContentAsUnsignedLong());
            }
            else
            {
                enumValue = Enum.ToObject(TraditionalDataContract.UnderlyingType, jsonReader.ReadElementContentAsLong());
            }

            if (context != null)
            {
                context.AddNewObject(enumValue);
            }
            return enumValue;
        }

        public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            if (IsULong)
            {
                jsonWriter.WriteUnsignedLong(((IConvertible)obj).ToUInt64(null));
            }
            else
            {
                jsonWriter.WriteLong(((IConvertible)obj).ToInt64(null));
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for (de)serializing types."
            + "Since the data is cached statically, we lock down access to it.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        class JsonEnumDataContractCriticalHelper : JsonDataContractCriticalHelper
        {
            bool isULong;

            public JsonEnumDataContractCriticalHelper(EnumDataContract traditionalEnumDataContract)
                : base(traditionalEnumDataContract)
            {
                isULong = traditionalEnumDataContract.IsULong;
            }

            public bool IsULong
            {
                get { return this.isULong; }
            }
        }
    }
}
