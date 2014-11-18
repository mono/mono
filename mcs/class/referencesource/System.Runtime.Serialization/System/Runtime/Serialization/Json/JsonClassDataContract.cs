//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.Threading;
    using System.Xml;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Security;

    class JsonClassDataContract : JsonDataContract
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + "Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        JsonClassDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        public JsonClassDataContract(ClassDataContract traditionalDataContract)
            : base(new JsonClassDataContractCriticalHelper(traditionalDataContract))
        {
            this.helper = base.Helper as JsonClassDataContractCriticalHelper;
        }

        internal JsonFormatClassReaderDelegate JsonFormatReaderDelegate
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical JsonFormatReaderDelegate property.",
                Safe = "JsonFormatReaderDelegate only needs to be protected for write.")]
            [SecuritySafeCritical]
            get
            {
                if (helper.JsonFormatReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (helper.JsonFormatReaderDelegate == null)
                        {
                            if (TraditionalClassDataContract.IsReadOnlyContract)
                            {
                                DataContract.ThrowInvalidDataContractException(TraditionalClassDataContract.DeserializationExceptionMessage, null /*type*/);
                            }
                            JsonFormatClassReaderDelegate tempDelegate = new JsonFormatReaderGenerator().GenerateClassReader(TraditionalClassDataContract);
                            Thread.MemoryBarrier();
                            helper.JsonFormatReaderDelegate = tempDelegate;
                        }
                    }
                }
                return helper.JsonFormatReaderDelegate;
            }
        }

        internal JsonFormatClassWriterDelegate JsonFormatWriterDelegate
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical JsonFormatWriterDelegate property.",
                Safe = "JsonFormatWriterDelegate only needs to be protected for write.")]
            [SecuritySafeCritical]
            get
            {
                if (helper.JsonFormatWriterDelegate == null)
                {
                    lock (this)
                    {
                        if (helper.JsonFormatWriterDelegate == null)
                        {
                            JsonFormatClassWriterDelegate tempDelegate = new JsonFormatWriterGenerator().GenerateClassWriter(TraditionalClassDataContract);
                            Thread.MemoryBarrier();
                            helper.JsonFormatWriterDelegate = tempDelegate;
                        }
                    }
                }
                return helper.JsonFormatWriterDelegate;
            }
        }

        internal XmlDictionaryString[] MemberNames
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical MemberNames property.",
                Safe = "MemberNames only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.helper.MemberNames; }
        }

        internal override string TypeName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical TypeName property.",
                Safe = "TypeName only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.helper.TypeName; }
        }


        ClassDataContract TraditionalClassDataContract
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical TraditionalClassDataContract property.",
                Safe = "TraditionalClassDataContract only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.helper.TraditionalClassDataContract; }
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            jsonReader.Read();
            object o = JsonFormatReaderDelegate(jsonReader, context, XmlDictionaryString.Empty, MemberNames);
            jsonReader.ReadEndElement();
            return o;
        }

        public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            jsonWriter.WriteAttributeString(null, JsonGlobals.typeString, null, JsonGlobals.objectString);
            JsonFormatWriterDelegate(jsonWriter, obj, context, TraditionalClassDataContract, MemberNames);
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for (de)serializing types."
            + "Since the data is cached statically, we lock down access to it.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        class JsonClassDataContractCriticalHelper : JsonDataContractCriticalHelper
        {
            JsonFormatClassReaderDelegate jsonFormatReaderDelegate;
            JsonFormatClassWriterDelegate jsonFormatWriterDelegate;
            XmlDictionaryString[] memberNames;
            ClassDataContract traditionalClassDataContract;
            string typeName;

            public JsonClassDataContractCriticalHelper(ClassDataContract traditionalDataContract)
                : base(traditionalDataContract)
            {
                this.typeName = string.IsNullOrEmpty(traditionalDataContract.Namespace.Value) ? traditionalDataContract.Name.Value : string.Concat(traditionalDataContract.Name.Value, JsonGlobals.NameValueSeparatorString, XmlObjectSerializerWriteContextComplexJson.TruncateDefaultDataContractNamespace(traditionalDataContract.Namespace.Value));
                this.traditionalClassDataContract = traditionalDataContract;
                CopyMembersAndCheckDuplicateNames();
            }

            internal JsonFormatClassReaderDelegate JsonFormatReaderDelegate
            {
                get { return this.jsonFormatReaderDelegate; }
                set { this.jsonFormatReaderDelegate = value; }
            }

            internal JsonFormatClassWriterDelegate JsonFormatWriterDelegate
            {
                get { return this.jsonFormatWriterDelegate; }
                set { this.jsonFormatWriterDelegate = value; }
            }

            internal XmlDictionaryString[] MemberNames
            {
                get { return this.memberNames; }
            }

            internal ClassDataContract TraditionalClassDataContract
            {
                get { return this.traditionalClassDataContract; }
            }

            void CopyMembersAndCheckDuplicateNames()
            {
                if (traditionalClassDataContract.MemberNames != null)
                {
                    int memberCount = traditionalClassDataContract.MemberNames.Length;
                    Dictionary<string, object> memberTable = new Dictionary<string, object>(memberCount);
                    XmlDictionaryString[] decodedMemberNames = new XmlDictionaryString[memberCount];
                    for (int i = 0; i < memberCount; i++)
                    {
                        if (memberTable.ContainsKey(traditionalClassDataContract.MemberNames[i].Value))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(SR.GetString(SR.JsonDuplicateMemberNames,
                                DataContract.GetClrTypeFullName(traditionalClassDataContract.UnderlyingType), traditionalClassDataContract.MemberNames[i].Value)));
                        }
                        else
                        {
                            memberTable.Add(traditionalClassDataContract.MemberNames[i].Value, null);
                            decodedMemberNames[i] = DataContractJsonSerializer.ConvertXmlNameToJsonName(traditionalClassDataContract.MemberNames[i]);
                        }
                    }
                    this.memberNames = decodedMemberNames;
                }
            }
        }

    }
}
