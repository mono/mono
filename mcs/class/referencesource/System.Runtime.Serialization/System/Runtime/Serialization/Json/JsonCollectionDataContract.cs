//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.Threading;
    using System.Xml;
    using System.Security;

    class JsonCollectionDataContract : JsonDataContract
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + "Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        JsonCollectionDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        public JsonCollectionDataContract(CollectionDataContract traditionalDataContract)
            : base(new JsonCollectionDataContractCriticalHelper(traditionalDataContract))
        {
            this.helper = base.Helper as JsonCollectionDataContractCriticalHelper;
        }

        internal JsonFormatCollectionReaderDelegate JsonFormatReaderDelegate
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
                            if (TraditionalCollectionDataContract.IsReadOnlyContract)
                            {
                                DataContract.ThrowInvalidDataContractException(TraditionalCollectionDataContract.DeserializationExceptionMessage, null /*type*/);
                            }
                            JsonFormatCollectionReaderDelegate tempDelegate = new JsonFormatReaderGenerator().GenerateCollectionReader(TraditionalCollectionDataContract);
                            Thread.MemoryBarrier();
                            helper.JsonFormatReaderDelegate = tempDelegate;
                        }
                    }
                }
                return helper.JsonFormatReaderDelegate;
            }
        }

        internal JsonFormatGetOnlyCollectionReaderDelegate JsonFormatGetOnlyReaderDelegate
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical JsonFormatGetOnlyReaderDelegate property.",
                Safe = "JsonFormatGetOnlyReaderDelegate only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (helper.JsonFormatGetOnlyReaderDelegate == null)
                {
                    lock (this)
                    {
                        if (helper.JsonFormatGetOnlyReaderDelegate == null)
                        {
                            CollectionKind kind = this.TraditionalCollectionDataContract.Kind;
                            if (this.TraditionalDataContract.UnderlyingType.IsInterface && (kind == CollectionKind.Enumerable || kind == CollectionKind.Collection || kind == CollectionKind.GenericEnumerable))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.GetOnlyCollectionMustHaveAddMethod, DataContract.GetClrTypeFullName(this.TraditionalDataContract.UnderlyingType))));
                            }
                            if (TraditionalCollectionDataContract.IsReadOnlyContract)
                            {
                                DataContract.ThrowInvalidDataContractException(TraditionalCollectionDataContract.DeserializationExceptionMessage, null /*type*/);
                            }
                            JsonFormatGetOnlyCollectionReaderDelegate tempDelegate = new JsonFormatReaderGenerator().GenerateGetOnlyCollectionReader(TraditionalCollectionDataContract);
                            Thread.MemoryBarrier();
                            helper.JsonFormatGetOnlyReaderDelegate = tempDelegate;
                        }
                    }
                }
                return helper.JsonFormatGetOnlyReaderDelegate;
            }
        }

        internal JsonFormatCollectionWriterDelegate JsonFormatWriterDelegate
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
                            JsonFormatCollectionWriterDelegate tempDelegate = new JsonFormatWriterGenerator().GenerateCollectionWriter(TraditionalCollectionDataContract);
                            Thread.MemoryBarrier();
                            helper.JsonFormatWriterDelegate = tempDelegate;
                        }
                    }
                }
                return helper.JsonFormatWriterDelegate;
            }
        }

        CollectionDataContract TraditionalCollectionDataContract
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical TraditionalCollectionDataContract property.",
                Safe = "TraditionalCollectionDataContract only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.helper.TraditionalCollectionDataContract; }
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            jsonReader.Read();
            object o = null;
            if (context.IsGetOnlyCollection)
            {
                // IsGetOnlyCollection value has already been used to create current collectiondatacontract, value can now be reset. 
                context.IsGetOnlyCollection = false;
                JsonFormatGetOnlyReaderDelegate(jsonReader, context, XmlDictionaryString.Empty, JsonGlobals.itemDictionaryString, TraditionalCollectionDataContract);
            }
            else
            {
                o = JsonFormatReaderDelegate(jsonReader, context, XmlDictionaryString.Empty, JsonGlobals.itemDictionaryString, TraditionalCollectionDataContract);
            }
            jsonReader.ReadEndElement();
            return o;
        }

        public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            // IsGetOnlyCollection value has already been used to create current collectiondatacontract, value can now be reset. 
            context.IsGetOnlyCollection = false;
            JsonFormatWriterDelegate(jsonWriter, obj, context, TraditionalCollectionDataContract);
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for (de)serializing types."
            + "Since the data is cached statically, we lock down access to it.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        class JsonCollectionDataContractCriticalHelper : JsonDataContractCriticalHelper
        {
            JsonFormatCollectionReaderDelegate jsonFormatReaderDelegate;
            JsonFormatGetOnlyCollectionReaderDelegate jsonFormatGetOnlyReaderDelegate;
            JsonFormatCollectionWriterDelegate jsonFormatWriterDelegate;
            CollectionDataContract traditionalCollectionDataContract;

            public JsonCollectionDataContractCriticalHelper(CollectionDataContract traditionalDataContract)
                : base(traditionalDataContract)
            {
                this.traditionalCollectionDataContract = traditionalDataContract;
            }

            internal JsonFormatCollectionReaderDelegate JsonFormatReaderDelegate
            {
                get { return this.jsonFormatReaderDelegate; }
                set { this.jsonFormatReaderDelegate = value; }
            }

            internal JsonFormatGetOnlyCollectionReaderDelegate JsonFormatGetOnlyReaderDelegate
            {
                get { return this.jsonFormatGetOnlyReaderDelegate; }
                set { this.jsonFormatGetOnlyReaderDelegate = value; }
            }

            internal JsonFormatCollectionWriterDelegate JsonFormatWriterDelegate
            {
                get { return this.jsonFormatWriterDelegate; }
                set { this.jsonFormatWriterDelegate = value; }
            }

            internal CollectionDataContract TraditionalCollectionDataContract
            {
                get { return this.traditionalCollectionDataContract; }
            }

        }
    }
}
