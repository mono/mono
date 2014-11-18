//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Reflection;
    using System.ServiceModel;
    using System.Xml;

#if USE_REFEMIT
    public class JsonDataContract
#else
    class JsonDataContract
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + "Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        JsonDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        protected JsonDataContract(DataContract traditionalDataContract)
        {
            this.helper = new JsonDataContractCriticalHelper(traditionalDataContract);
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        protected JsonDataContract(JsonDataContractCriticalHelper helper)
        {
            this.helper = helper;
        }

        internal virtual string TypeName
        {
            get { return null; }
        }

        protected JsonDataContractCriticalHelper Helper
        {
            [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
                + "Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
            [SecurityCritical]
            get { return helper; }
        }

        protected DataContract TraditionalDataContract
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical TraditionalDataContract from the helper.",
                Safe = "TraditionalDataContract only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.helper.TraditionalDataContract; }
        }

        Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical KnownDataContracts from the helper.",
                Safe = "KnownDataContracts only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return this.helper.KnownDataContracts; }
        }

        [Fx.Tag.SecurityNote(Critical = "Fetches the critical JsonDataContract from the helper.",
            Safe = "JsonDataContract only needs to be protected for write.")]
        [SecuritySafeCritical]
        public static JsonDataContract GetJsonDataContract(DataContract traditionalDataContract)
        {
            return JsonDataContractCriticalHelper.GetJsonDataContract(traditionalDataContract);
        }

        public object ReadJsonValue(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            PushKnownDataContracts(context);
            object deserializedObject = ReadJsonValueCore(jsonReader, context);
            PopKnownDataContracts(context);
            return deserializedObject;
        }

        public virtual object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            return TraditionalDataContract.ReadXmlValue(jsonReader, context);
        }

        public void WriteJsonValue(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            PushKnownDataContracts(context);
            WriteJsonValueCore(jsonWriter, obj, context, declaredTypeHandle);
            PopKnownDataContracts(context);
        }

        public virtual void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            TraditionalDataContract.WriteXmlValue(jsonWriter, obj, context);
        }

        protected static object HandleReadValue(object obj, XmlObjectSerializerReadContext context)
        {
            context.AddNewObject(obj);
            return obj;
        }

        protected static bool TryReadNullAtTopLevel(XmlReaderDelegator reader)
        {
            while (reader.MoveToAttribute(JsonGlobals.typeString) && (reader.Value == JsonGlobals.nullString))
            {
                reader.Skip();
                reader.MoveToElement();
                return true;
            }

            reader.MoveToElement();
            return false;
        }

        protected void PopKnownDataContracts(XmlObjectSerializerContext context)
        {
            if (KnownDataContracts != null)
            {
                context.scopedKnownTypes.Pop();
            }
        }

        protected void PushKnownDataContracts(XmlObjectSerializerContext context)
        {
            if (KnownDataContracts != null)
            {
                context.scopedKnownTypes.Push(KnownDataContracts);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for (de)serializing types."
            + "Since the data is cached statically, we lock down access to it.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
#if USE_REFEMIT
        public class JsonDataContractCriticalHelper
#else
        internal class JsonDataContractCriticalHelper
#endif
        {

            static object cacheLock = new object();
            static object createDataContractLock = new object();

            static JsonDataContract[] dataContractCache = new JsonDataContract[32];
            static int dataContractID = 0;

            static TypeHandleRef typeHandleRef = new TypeHandleRef();
            static Dictionary<TypeHandleRef, IntRef> typeToIDCache = new Dictionary<TypeHandleRef, IntRef>(new TypeHandleRefEqualityComparer());
            Dictionary<XmlQualifiedName, DataContract> knownDataContracts;
            DataContract traditionalDataContract;
            string typeName;

            internal JsonDataContractCriticalHelper(DataContract traditionalDataContract)
            {
                this.traditionalDataContract = traditionalDataContract;
                AddCollectionItemContractsToKnownDataContracts();
                this.typeName = string.IsNullOrEmpty(traditionalDataContract.Namespace.Value) ? traditionalDataContract.Name.Value : string.Concat(traditionalDataContract.Name.Value, JsonGlobals.NameValueSeparatorString, XmlObjectSerializerWriteContextComplexJson.TruncateDefaultDataContractNamespace(traditionalDataContract.Namespace.Value));
            }

            internal Dictionary<XmlQualifiedName, DataContract> KnownDataContracts
            {
                get { return this.knownDataContracts; }
            }

            internal DataContract TraditionalDataContract
            {
                get { return this.traditionalDataContract; }
            }

            internal virtual string TypeName
            {
                get { return this.typeName; }
            }

            public static JsonDataContract GetJsonDataContract(DataContract traditionalDataContract)
            {
                int id = JsonDataContractCriticalHelper.GetId(traditionalDataContract.UnderlyingType.TypeHandle);
                JsonDataContract dataContract = dataContractCache[id];
                if (dataContract == null)
                {
                    dataContract = CreateJsonDataContract(id, traditionalDataContract);
                    dataContractCache[id] = dataContract;
                }
                return dataContract;
            }

            internal static int GetId(RuntimeTypeHandle typeHandle)
            {
                lock (cacheLock)
                {
                    IntRef id;
                    typeHandleRef.Value = typeHandle;
                    if (!typeToIDCache.TryGetValue(typeHandleRef, out id))
                    {
                        int value = dataContractID++;
                        if (value >= dataContractCache.Length)
                        {
                            int newSize = (value < Int32.MaxValue / 2) ? value * 2 : Int32.MaxValue;
                            if (newSize <= value)
                            {
                                Fx.Assert("DataContract cache overflow");
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SerializationException(System.Runtime.Serialization.SR.GetString(System.Runtime.Serialization.SR.DataContractCacheOverflow)));
                            }
                            Array.Resize<JsonDataContract>(ref dataContractCache, newSize);
                        }
                        id = new IntRef(value);
                        try
                        {
                            typeToIDCache.Add(new TypeHandleRef(typeHandle), id);
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(ex.Message, ex);
                        }
                    }
                    return id.Value;
                }
            }

            static JsonDataContract CreateJsonDataContract(int id, DataContract traditionalDataContract)
            {
                lock (createDataContractLock)
                {
                    JsonDataContract dataContract = dataContractCache[id];
                    if (dataContract == null)
                    {
                        Type traditionalDataContractType = traditionalDataContract.GetType();
                        if (traditionalDataContractType == typeof(ObjectDataContract))
                        {
                            dataContract = new JsonObjectDataContract(traditionalDataContract);
                        }
                        else if (traditionalDataContractType == typeof(StringDataContract))
                        {
                            dataContract = new JsonStringDataContract((StringDataContract)traditionalDataContract);
                        }
                        else if (traditionalDataContractType == typeof(UriDataContract))
                        {
                            dataContract = new JsonUriDataContract((UriDataContract)traditionalDataContract);
                        }
                        else if (traditionalDataContractType == typeof(QNameDataContract))
                        {
                            dataContract = new JsonQNameDataContract((QNameDataContract)traditionalDataContract);
                        }
                        else if (traditionalDataContractType == typeof(ByteArrayDataContract))
                        {
                            dataContract = new JsonByteArrayDataContract((ByteArrayDataContract)traditionalDataContract);
                        }
                        else if (traditionalDataContract.IsPrimitive ||
                            traditionalDataContract.UnderlyingType == Globals.TypeOfXmlQualifiedName)
                        {
                            dataContract = new JsonDataContract(traditionalDataContract);
                        }
                        else if (traditionalDataContractType == typeof(ClassDataContract))
                        {
                            dataContract = new JsonClassDataContract((ClassDataContract)traditionalDataContract);
                        }
                        else if (traditionalDataContractType == typeof(EnumDataContract))
                        {
                            dataContract = new JsonEnumDataContract((EnumDataContract)traditionalDataContract);
                        }
                        else if ((traditionalDataContractType == typeof(GenericParameterDataContract)) ||
                            (traditionalDataContractType == typeof(SpecialTypeDataContract)))
                        {
                            dataContract = new JsonDataContract(traditionalDataContract);
                        }
                        else if (traditionalDataContractType == typeof(CollectionDataContract))
                        {
                            dataContract = new JsonCollectionDataContract((CollectionDataContract)traditionalDataContract);
                        }
                        else if (traditionalDataContractType == typeof(XmlDataContract))
                        {
                            dataContract = new JsonXmlDataContract((XmlDataContract)traditionalDataContract);
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("traditionalDataContract",
                                SR.GetString(SR.JsonTypeNotSupportedByDataContractJsonSerializer, traditionalDataContract.UnderlyingType));
                        }
                    }
                    return dataContract;
                }
            }

            void AddCollectionItemContractsToKnownDataContracts()
            {
                if (traditionalDataContract.KnownDataContracts != null)
                {
                    foreach (KeyValuePair<XmlQualifiedName, DataContract> knownDataContract in traditionalDataContract.KnownDataContracts)
                    {
                        if (!object.ReferenceEquals(knownDataContract, null))
                        {
                            CollectionDataContract collectionDataContract = knownDataContract.Value as CollectionDataContract;
                            while (collectionDataContract != null)
                            {
                                DataContract itemContract = collectionDataContract.ItemContract;
                                if (knownDataContracts == null)
                                {
                                    knownDataContracts = new Dictionary<XmlQualifiedName, DataContract>();
                                }

                                if (!knownDataContracts.ContainsKey(itemContract.StableName))
                                {
                                    knownDataContracts.Add(itemContract.StableName, itemContract);
                                }

                                if (collectionDataContract.ItemType.IsGenericType
                                    && collectionDataContract.ItemType.GetGenericTypeDefinition() == typeof(KeyValue<,>))
                                {
                                    DataContract itemDataContract = DataContract.GetDataContract(Globals.TypeOfKeyValuePair.MakeGenericType(collectionDataContract.ItemType.GetGenericArguments()));
                                    if (!knownDataContracts.ContainsKey(itemDataContract.StableName))
                                    {
                                        knownDataContracts.Add(itemDataContract.StableName, itemDataContract);
                                    }
                                }

                                if (!(itemContract is CollectionDataContract))
                                {
                                    break;
                                }
                                collectionDataContract = itemContract as CollectionDataContract;
                            }
                        }
                    }
                }
            }
        }

    }
}
