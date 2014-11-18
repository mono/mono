
namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization.Diagnostics.Application;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    delegate void JsonFormatClassWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, ClassDataContract dataContract, XmlDictionaryString[] memberNames);
    delegate void JsonFormatCollectionWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, CollectionDataContract dataContract);

    class JsonFormatWriterGenerator
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that was produced within an assert.")]
        [SecurityCritical]
        CriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.")]
        [SecurityCritical]
        public JsonFormatWriterGenerator()
        {
            helper = new CriticalHelper();
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        internal JsonFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
        {
            try
            {
                if (TD.DCJsonGenWriterStartIsEnabled())
                {
                    TD.DCJsonGenWriterStart("Class", classContract.UnderlyingType.FullName);
                }

                return helper.GenerateClassWriter(classContract);
            }
            finally
            {
                if (TD.DCJsonGenWriterStopIsEnabled())
                {
                    TD.DCJsonGenWriterStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        internal JsonFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
        {
            try
            {
                if (TD.DCJsonGenWriterStartIsEnabled())
                {
                    TD.DCJsonGenWriterStart("Collection", collectionContract.UnderlyingType.FullName);
                }

                return helper.GenerateCollectionWriter(collectionContract);
            }
            finally
            {
                if (TD.DCJsonGenWriterStopIsEnabled())
                {
                    TD.DCJsonGenWriterStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - handles all aspects of IL generation including initializing the DynamicMethod."
            + "Changes to how IL generated could affect how data is deserialized and what gets access to data, "
            + "therefore we mark it for review so that changes to generation logic are reviewed.")]
        class CriticalHelper
        {
            CodeGenerator ilg;
            ArgBuilder xmlWriterArg;
            ArgBuilder contextArg;
            ArgBuilder dataContractArg;
            LocalBuilder objectLocal;

            // Used for classes
            ArgBuilder memberNamesArg;
            int typeIndex = 1;
            int childElementIndex = 0;

            internal JsonFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
            {
                ilg = new CodeGenerator();
                bool memberAccessFlag = classContract.RequiresMemberAccessForWrite(null);
                try
                {
                    BeginMethod(ilg, "Write" + classContract.StableName.Name + "ToJson", typeof(JsonFormatClassWriterDelegate), memberAccessFlag);
                }
                catch (SecurityException securityException)
                {
                    if (memberAccessFlag && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        classContract.RequiresMemberAccessForWrite(securityException);
                    }
                    else
                    {
                        throw;
                    }
                }
                InitArgs(classContract.UnderlyingType);
                memberNamesArg = ilg.GetArg(4);
                DemandSerializationFormatterPermission(classContract);
                DemandMemberAccessPermission(memberAccessFlag);
                if (classContract.IsReadOnlyContract)
                {
                    ThrowIfCannotSerializeReadOnlyTypes(classContract);
                }
                WriteClass(classContract);
                return (JsonFormatClassWriterDelegate)ilg.EndMethod();
            }

            internal JsonFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
            {
                ilg = new CodeGenerator();
                bool memberAccessFlag = collectionContract.RequiresMemberAccessForWrite(null);
                try
                {
                    BeginMethod(ilg, "Write" + collectionContract.StableName.Name + "ToJson", typeof(JsonFormatCollectionWriterDelegate), memberAccessFlag);
                }
                catch (SecurityException securityException)
                {
                    if (memberAccessFlag && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        collectionContract.RequiresMemberAccessForWrite(securityException);
                    }
                    else
                    {
                        throw;
                    }
                }
                InitArgs(collectionContract.UnderlyingType);
                DemandMemberAccessPermission(memberAccessFlag);
                if (collectionContract.IsReadOnlyContract)
                {
                    ThrowIfCannotSerializeReadOnlyTypes(collectionContract);
                }
                WriteCollection(collectionContract);
                return (JsonFormatCollectionWriterDelegate)ilg.EndMethod();
            }

            void BeginMethod(CodeGenerator ilg, string methodName, Type delegateType, bool allowPrivateMemberAccess)
            {
#if USE_REFEMIT
                ilg.BeginMethod(methodName, delegateType, allowPrivateMemberAccess);
#else
                MethodInfo signature = delegateType.GetMethod("Invoke");
                ParameterInfo[] parameters = signature.GetParameters();
                Type[] paramTypes = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    paramTypes[i] = parameters[i].ParameterType;

                DynamicMethod dynamicMethod = new DynamicMethod(methodName, signature.ReturnType, paramTypes, typeof(JsonFormatWriterGenerator).Module, allowPrivateMemberAccess);
                ilg.BeginMethod(dynamicMethod, delegateType, methodName, paramTypes, allowPrivateMemberAccess);
#endif
            }

            void InitArgs(Type objType)
            {
                xmlWriterArg = ilg.GetArg(0);
                contextArg = ilg.GetArg(2);
                dataContractArg = ilg.GetArg(3);

                objectLocal = ilg.DeclareLocal(objType, "objSerialized");
                ArgBuilder objectArg = ilg.GetArg(1);
                ilg.Load(objectArg);

                // Copy the data from the DataTimeOffset object passed in to the DateTimeOffsetAdapter.
                // DateTimeOffsetAdapter is used here for serialization purposes to bypass the ISerializable implementation
                // on DateTimeOffset; which does not work in partial trust.

                if (objType == Globals.TypeOfDateTimeOffsetAdapter)
                {
                    ilg.ConvertValue(objectArg.ArgType, Globals.TypeOfDateTimeOffset);
                    ilg.Call(XmlFormatGeneratorStatics.GetDateTimeOffsetAdapterMethod);
                }
                else
                {
                    ilg.ConvertValue(objectArg.ArgType, objType);
                }
                ilg.Stloc(objectLocal);
            }


            void DemandMemberAccessPermission(bool memberAccessFlag)
            {
                if (memberAccessFlag)
                {
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.DemandMemberAccessPermissionMethod);
                }
            }

            void DemandSerializationFormatterPermission(ClassDataContract classContract)
            {
                if (!classContract.HasDataContract && !classContract.IsNonAttributedType)
                {
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.DemandSerializationFormatterPermissionMethod);
                }
            }

            void ThrowIfCannotSerializeReadOnlyTypes(ClassDataContract classContract)
            {
                ThrowIfCannotSerializeReadOnlyTypes(XmlFormatGeneratorStatics.ClassSerializationExceptionMessageProperty);
            }

            void ThrowIfCannotSerializeReadOnlyTypes(CollectionDataContract classContract)
            {
                ThrowIfCannotSerializeReadOnlyTypes(XmlFormatGeneratorStatics.CollectionSerializationExceptionMessageProperty);
            }

            void ThrowIfCannotSerializeReadOnlyTypes(PropertyInfo serializationExceptionMessageProperty)
            {
                ilg.Load(contextArg);
                ilg.LoadMember(XmlFormatGeneratorStatics.SerializeReadOnlyTypesProperty);
                ilg.IfNot();
                ilg.Load(dataContractArg);
                ilg.LoadMember(serializationExceptionMessageProperty);
                ilg.Load(null);
                ilg.Call(XmlFormatGeneratorStatics.ThrowInvalidDataContractExceptionMethod);
                ilg.EndIf();
            }

            void InvokeOnSerializing(ClassDataContract classContract)
            {
                if (classContract.BaseContract != null)
                    InvokeOnSerializing(classContract.BaseContract);
                if (classContract.OnSerializing != null)
                {
                    ilg.LoadAddress(objectLocal);
                    ilg.Load(contextArg);
                    ilg.Call(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                    ilg.Call(classContract.OnSerializing);
                }
            }

            void InvokeOnSerialized(ClassDataContract classContract)
            {
                if (classContract.BaseContract != null)
                    InvokeOnSerialized(classContract.BaseContract);
                if (classContract.OnSerialized != null)
                {
                    ilg.LoadAddress(objectLocal);
                    ilg.Load(contextArg);
                    ilg.Call(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                    ilg.Call(classContract.OnSerialized);
                }
            }

            void WriteClass(ClassDataContract classContract)
            {
                InvokeOnSerializing(classContract);

                if (classContract.IsISerializable)
                {
                    ilg.Call(contextArg, JsonFormatGeneratorStatics.WriteJsonISerializableMethod, xmlWriterArg, objectLocal);
                }
                else
                {
                    if (classContract.HasExtensionData)
                    {
                        LocalBuilder extensionDataLocal = ilg.DeclareLocal(Globals.TypeOfExtensionDataObject, "extensionData");
                        ilg.Load(objectLocal);
                        ilg.ConvertValue(objectLocal.LocalType, Globals.TypeOfIExtensibleDataObject);
                        ilg.LoadMember(JsonFormatGeneratorStatics.ExtensionDataProperty);
                        ilg.Store(extensionDataLocal);
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.WriteExtensionDataMethod, xmlWriterArg, extensionDataLocal, -1);
                        WriteMembers(classContract, extensionDataLocal, classContract);
                    }
                    else
                        WriteMembers(classContract, null, classContract);
                }
                InvokeOnSerialized(classContract);
            }

            int WriteMembers(ClassDataContract classContract, LocalBuilder extensionDataLocal, ClassDataContract derivedMostClassContract)
            {
                int memberCount = (classContract.BaseContract == null) ? 0 :
                    WriteMembers(classContract.BaseContract, extensionDataLocal, derivedMostClassContract);

                ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, classContract.Members.Count);

                for (int i = 0; i < classContract.Members.Count; i++, memberCount++)
                {
                    DataMember member = classContract.Members[i];
                    Type memberType = member.MemberType;
                    LocalBuilder memberValue = null;
                    if (member.IsGetOnlyCollection)
                    {
                        ilg.Load(contextArg);
                        ilg.Call(XmlFormatGeneratorStatics.StoreIsGetOnlyCollectionMethod);
                    }
                    if (!member.EmitDefaultValue)
                    {
                        memberValue = LoadMemberValue(member);
                        ilg.IfNotDefaultValue(memberValue);
                    }

                    bool requiresNameAttribute = DataContractJsonSerializer.CheckIfXmlNameRequiresMapping(classContract.MemberNames[i]);
                    if (requiresNameAttribute || !TryWritePrimitive(memberType, memberValue, member.MemberInfo, null /*arrayItemIndex*/, null /*nameLocal*/, i + childElementIndex))
                    {
                        // Note: DataContractSerializer has member-conflict logic here to deal with the schema export
                        //       requirement that the same member can't be of two different types.
                        if (requiresNameAttribute)
                        {
                            ilg.Call(null, JsonFormatGeneratorStatics.WriteJsonNameWithMappingMethod, xmlWriterArg, memberNamesArg, i + childElementIndex);
                        }
                        else
                        {
                            WriteStartElement(null /*nameLocal*/, i + childElementIndex);
                        }
                        if (memberValue == null)
                            memberValue = LoadMemberValue(member);
                        WriteValue(memberValue);
                        WriteEndElement();
                    }
                    if (classContract.HasExtensionData)
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.WriteExtensionDataMethod, xmlWriterArg, extensionDataLocal, memberCount);
                    if (!member.EmitDefaultValue)
                    {
                        if (member.IsRequired)
                        {
                            ilg.Else();
                            ilg.Call(null, XmlFormatGeneratorStatics.ThrowRequiredMemberMustBeEmittedMethod, member.Name, classContract.UnderlyingType);
                        }
                        ilg.EndIf();
                    }
                }

                typeIndex++;
                childElementIndex += classContract.Members.Count;
                return memberCount;
            }

            private LocalBuilder LoadMemberValue(DataMember member)
            {
                ilg.LoadAddress(objectLocal);
                ilg.LoadMember(member.MemberInfo);
                LocalBuilder memberValue = ilg.DeclareLocal(member.MemberType, member.Name + "Value");
                ilg.Stloc(memberValue);
                return memberValue;
            }

            void WriteCollection(CollectionDataContract collectionContract)
            {
                LocalBuilder itemName = ilg.DeclareLocal(typeof(XmlDictionaryString), "itemName");
                ilg.Load(contextArg);
                ilg.LoadMember(JsonFormatGeneratorStatics.CollectionItemNameProperty);
                ilg.Store(itemName);

                if (collectionContract.Kind == CollectionKind.Array)
                {
                    Type itemType = collectionContract.ItemType;
                    LocalBuilder i = ilg.DeclareLocal(Globals.TypeOfInt, "i");

                    ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementArrayCountMethod, xmlWriterArg, objectLocal);

                    if (!TryWritePrimitiveArray(collectionContract.UnderlyingType, itemType, objectLocal, itemName))
                    {
                        WriteArrayAttribute();
                        ilg.For(i, 0, objectLocal);
                        if (!TryWritePrimitive(itemType, null /*value*/, null /*memberInfo*/, i /*arrayItemIndex*/, itemName, 0 /*nameIndex*/))
                        {
                            WriteStartElement(itemName, 0 /*nameIndex*/);
                            ilg.LoadArrayElement(objectLocal, i);
                            LocalBuilder memberValue = ilg.DeclareLocal(itemType, "memberValue");
                            ilg.Stloc(memberValue);
                            WriteValue(memberValue);
                            WriteEndElement();
                        }
                        ilg.EndFor();
                    }
                }
                else
                {
                    MethodInfo incrementCollectionCountMethod = null;
                    switch (collectionContract.Kind)
                    {
                        case CollectionKind.Collection:
                        case CollectionKind.List:
                        case CollectionKind.Dictionary:
                            incrementCollectionCountMethod = XmlFormatGeneratorStatics.IncrementCollectionCountMethod;
                            break;
                        case CollectionKind.GenericCollection:
                        case CollectionKind.GenericList:
                            incrementCollectionCountMethod = XmlFormatGeneratorStatics.IncrementCollectionCountGenericMethod.MakeGenericMethod(collectionContract.ItemType);
                            break;
                        case CollectionKind.GenericDictionary:
                            incrementCollectionCountMethod = XmlFormatGeneratorStatics.IncrementCollectionCountGenericMethod.MakeGenericMethod(Globals.TypeOfKeyValuePair.MakeGenericType(collectionContract.ItemType.GetGenericArguments()));
                            break;
                    }
                    if (incrementCollectionCountMethod != null)
                    {
                        ilg.Call(contextArg, incrementCollectionCountMethod, xmlWriterArg, objectLocal);
                    }

                    bool isDictionary = false, isGenericDictionary = false;
                    Type enumeratorType = null;
                    Type[] keyValueTypes = null;
                    if (collectionContract.Kind == CollectionKind.GenericDictionary)
                    {
                        isGenericDictionary = true;
                        keyValueTypes = collectionContract.ItemType.GetGenericArguments();
                        enumeratorType = Globals.TypeOfGenericDictionaryEnumerator.MakeGenericType(keyValueTypes);
                    }
                    else if (collectionContract.Kind == CollectionKind.Dictionary)
                    {
                        isDictionary = true;
                        keyValueTypes = new Type[] { Globals.TypeOfObject, Globals.TypeOfObject };
                        enumeratorType = Globals.TypeOfDictionaryEnumerator;
                    }
                    else
                    {
                        enumeratorType = collectionContract.GetEnumeratorMethod.ReturnType;
                    }
                    MethodInfo moveNextMethod = enumeratorType.GetMethod(Globals.MoveNextMethodName, BindingFlags.Instance | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
                    MethodInfo getCurrentMethod = enumeratorType.GetMethod(Globals.GetCurrentMethodName, BindingFlags.Instance | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
                    if (moveNextMethod == null || getCurrentMethod == null)
                    {
                        if (enumeratorType.IsInterface)
                        {
                            if (moveNextMethod == null)
                                moveNextMethod = JsonFormatGeneratorStatics.MoveNextMethod;
                            if (getCurrentMethod == null)
                                getCurrentMethod = JsonFormatGeneratorStatics.GetCurrentMethod;
                        }
                        else
                        {
                            Type ienumeratorInterface = Globals.TypeOfIEnumerator;
                            CollectionKind kind = collectionContract.Kind;
                            if (kind == CollectionKind.GenericDictionary || kind == CollectionKind.GenericCollection || kind == CollectionKind.GenericEnumerable)
                            {
                                Type[] interfaceTypes = enumeratorType.GetInterfaces();
                                foreach (Type interfaceType in interfaceTypes)
                                {
                                    if (interfaceType.IsGenericType
                                        && interfaceType.GetGenericTypeDefinition() == Globals.TypeOfIEnumeratorGeneric
                                        && interfaceType.GetGenericArguments()[0] == collectionContract.ItemType)
                                    {
                                        ienumeratorInterface = interfaceType;
                                        break;
                                    }
                                }
                            }
                            if (moveNextMethod == null)
                                moveNextMethod = CollectionDataContract.GetTargetMethodWithName(Globals.MoveNextMethodName, enumeratorType, ienumeratorInterface);
                            if (getCurrentMethod == null)
                                getCurrentMethod = CollectionDataContract.GetTargetMethodWithName(Globals.GetCurrentMethodName, enumeratorType, ienumeratorInterface);
                        }
                    }
                    Type elementType = getCurrentMethod.ReturnType;
                    LocalBuilder currentValue = ilg.DeclareLocal(elementType, "currentValue");

                    LocalBuilder enumerator = ilg.DeclareLocal(enumeratorType, "enumerator");
                    ilg.Call(objectLocal, collectionContract.GetEnumeratorMethod);
                    if (isDictionary)
                    {
                        ConstructorInfo dictEnumCtor = enumeratorType.GetConstructor(Globals.ScanAllMembers, null, new Type[] { Globals.TypeOfIDictionaryEnumerator }, null);
                        ilg.ConvertValue(collectionContract.GetEnumeratorMethod.ReturnType, Globals.TypeOfIDictionaryEnumerator);
                        ilg.New(dictEnumCtor);
                    }
                    else if (isGenericDictionary)
                    {
                        Type ctorParam = Globals.TypeOfIEnumeratorGeneric.MakeGenericType(Globals.TypeOfKeyValuePair.MakeGenericType(keyValueTypes));
                        ConstructorInfo dictEnumCtor = enumeratorType.GetConstructor(Globals.ScanAllMembers, null, new Type[] { ctorParam }, null);
                        ilg.ConvertValue(collectionContract.GetEnumeratorMethod.ReturnType, ctorParam);
                        ilg.New(dictEnumCtor);
                    }
                    ilg.Stloc(enumerator);

                    bool canWriteSimpleDictionary = isDictionary || isGenericDictionary;
                    if (canWriteSimpleDictionary)
                    {
                        Type genericDictionaryKeyValueType = Globals.TypeOfKeyValue.MakeGenericType(keyValueTypes);
                        PropertyInfo genericDictionaryKeyProperty = genericDictionaryKeyValueType.GetProperty(JsonGlobals.KeyString);
                        PropertyInfo genericDictionaryValueProperty = genericDictionaryKeyValueType.GetProperty(JsonGlobals.ValueString);

                        ilg.Load(contextArg);
                        ilg.LoadMember(JsonFormatGeneratorStatics.UseSimpleDictionaryFormatWriteProperty);
                        ilg.If();
                        WriteObjectAttribute();
                        LocalBuilder pairKey = ilg.DeclareLocal(Globals.TypeOfString, "key");
                        LocalBuilder pairValue = ilg.DeclareLocal(keyValueTypes[1], "value");
                        ilg.ForEach(currentValue, elementType, enumeratorType, enumerator, getCurrentMethod);

                        ilg.LoadAddress(currentValue);
                        ilg.LoadMember(genericDictionaryKeyProperty);
                        ilg.ToString(keyValueTypes[0]);
                        ilg.Stloc(pairKey);

                        ilg.LoadAddress(currentValue);
                        ilg.LoadMember(genericDictionaryValueProperty);
                        ilg.Stloc(pairValue);

                        WriteStartElement(pairKey, 0 /*nameIndex*/);
                        WriteValue(pairValue);
                        WriteEndElement();

                        ilg.EndForEach(moveNextMethod);
                        ilg.Else();
                    }

                    WriteArrayAttribute();

                    ilg.ForEach(currentValue, elementType, enumeratorType, enumerator, getCurrentMethod);
                    if (incrementCollectionCountMethod == null)
                    {
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                    }
                    if (!TryWritePrimitive(elementType, currentValue, null /*memberInfo*/, null /*arrayItemIndex*/, itemName, 0 /*nameIndex*/))
                    {
                        WriteStartElement(itemName, 0 /*nameIndex*/);

                        if (isGenericDictionary || isDictionary)
                        {
                            ilg.Call(dataContractArg, JsonFormatGeneratorStatics.GetItemContractMethod);
                            ilg.Call(JsonFormatGeneratorStatics.GetRevisedItemContractMethod);
                            ilg.Call(JsonFormatGeneratorStatics.GetJsonDataContractMethod);
                            ilg.Load(xmlWriterArg);
                            ilg.Load(currentValue);
                            ilg.ConvertValue(currentValue.LocalType, Globals.TypeOfObject);
                            ilg.Load(contextArg);
                            ilg.Load(currentValue.LocalType);
                            ilg.LoadMember(JsonFormatGeneratorStatics.TypeHandleProperty);
                            ilg.Call(JsonFormatGeneratorStatics.WriteJsonValueMethod);
                        }
                        else
                        {
                            WriteValue(currentValue);
                        }
                        WriteEndElement();
                    }
                    ilg.EndForEach(moveNextMethod);

                    if (canWriteSimpleDictionary)
                    {
                        ilg.EndIf();
                    }
                }
            }

            bool TryWritePrimitive(Type type, LocalBuilder value, MemberInfo memberInfo, LocalBuilder arrayItemIndex, LocalBuilder name, int nameIndex)
            {
                PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(type);
                if (primitiveContract == null || primitiveContract.UnderlyingType == Globals.TypeOfObject)
                    return false;

                // load writer
                if (type.IsValueType)
                {
                    ilg.Load(xmlWriterArg);
                }
                else
                {
                    ilg.Load(contextArg);
                    ilg.Load(xmlWriterArg);
                }
                // load primitive value 
                if (value != null)
                {
                    ilg.Load(value);
                }
                else if (memberInfo != null)
                {
                    ilg.LoadAddress(objectLocal);
                    ilg.LoadMember(memberInfo);
                }
                else
                {
                    ilg.LoadArrayElement(objectLocal, arrayItemIndex);
                }
                // load name
                if (name != null)
                {
                    ilg.Load(name);
                }
                else
                {
                    ilg.LoadArrayElement(memberNamesArg, nameIndex);
                }
                // load namespace
                ilg.Load(null);
                // call method to write primitive
                ilg.Call(primitiveContract.XmlFormatWriterMethod);
                return true;
            }

            bool TryWritePrimitiveArray(Type type, Type itemType, LocalBuilder value, LocalBuilder itemName)
            {
                PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(itemType);
                if (primitiveContract == null)
                    return false;

                string writeArrayMethod = null;
                switch (Type.GetTypeCode(itemType))
                {
                    case TypeCode.Boolean:
                        writeArrayMethod = "WriteJsonBooleanArray";
                        break;
                    case TypeCode.DateTime:
                        writeArrayMethod = "WriteJsonDateTimeArray";
                        break;
                    case TypeCode.Decimal:
                        writeArrayMethod = "WriteJsonDecimalArray";
                        break;
                    case TypeCode.Int32:
                        writeArrayMethod = "WriteJsonInt32Array";
                        break;
                    case TypeCode.Int64:
                        writeArrayMethod = "WriteJsonInt64Array";
                        break;
                    case TypeCode.Single:
                        writeArrayMethod = "WriteJsonSingleArray";
                        break;
                    case TypeCode.Double:
                        writeArrayMethod = "WriteJsonDoubleArray";
                        break;
                    default:
                        break;
                }
                if (writeArrayMethod != null)
                {
                    WriteArrayAttribute();
                    ilg.Call(xmlWriterArg, typeof(JsonWriterDelegator).GetMethod(writeArrayMethod, Globals.ScanAllMembers, null, new Type[] { type, typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null), value, itemName, null);
                    return true;
                }
                return false;
            }

            void WriteArrayAttribute()
            {
                ilg.Call(xmlWriterArg, JsonFormatGeneratorStatics.WriteAttributeStringMethod,
                    null /* prefix */,
                    JsonGlobals.typeString /* local name */,
                    string.Empty /* namespace */,
                    JsonGlobals.arrayString /* value */);
            }

            void WriteObjectAttribute()
            {
                ilg.Call(xmlWriterArg, JsonFormatGeneratorStatics.WriteAttributeStringMethod,
                    null /* prefix */,
                    JsonGlobals.typeString /* local name */,
                    null /* namespace */,
                    JsonGlobals.objectString /* value */);
            }

            void WriteValue(LocalBuilder memberValue)
            {
                Type memberType = memberValue.LocalType;
                if (memberType.IsPointer)
                {
                    ilg.Load(memberValue);
                    ilg.Load(memberType);
                    ilg.Call(JsonFormatGeneratorStatics.BoxPointer);
                    memberType = Globals.TypeOfReflectionPointer;
                    memberValue = ilg.DeclareLocal(memberType, "memberValueRefPointer");
                    ilg.Store(memberValue);
                }
                bool isNullableOfT = (memberType.IsGenericType &&
                                      memberType.GetGenericTypeDefinition() == Globals.TypeOfNullable);
                if (memberType.IsValueType && !isNullableOfT)
                {
                    PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(memberType);
                    if (primitiveContract != null)
                        ilg.Call(xmlWriterArg, primitiveContract.XmlFormatContentWriterMethod, memberValue);
                    else
                        InternalSerialize(XmlFormatGeneratorStatics.InternalSerializeMethod, memberValue, memberType, false /* writeXsiType */);
                }
                else
                {
                    if (isNullableOfT)
                    {
                        memberValue = UnwrapNullableObject(memberValue); //Leaves !HasValue on stack
                        memberType = memberValue.LocalType;
                    }
                    else
                    {
                        ilg.Load(memberValue);
                        ilg.Load(null);
                        ilg.Ceq();
                    }
                    ilg.If();
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.WriteNullMethod, xmlWriterArg, memberType, DataContract.IsTypeSerializable(memberType));
                    ilg.Else();
                    PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(memberType);
                    if (primitiveContract != null && primitiveContract.UnderlyingType != Globals.TypeOfObject)
                    {
                        if (isNullableOfT)
                        {
                            ilg.Call(xmlWriterArg, primitiveContract.XmlFormatContentWriterMethod, memberValue);
                        }
                        else
                        {
                            ilg.Call(contextArg, primitiveContract.XmlFormatContentWriterMethod, xmlWriterArg, memberValue);
                        }
                    }
                    else
                    {
                        if (memberType == Globals.TypeOfObject || //boxed Nullable<T>
                            memberType == Globals.TypeOfValueType ||
                            ((IList)Globals.TypeOfNullable.GetInterfaces()).Contains(memberType))
                        {
                            ilg.Load(memberValue);
                            ilg.ConvertValue(memberValue.LocalType, Globals.TypeOfObject);
                            memberValue = ilg.DeclareLocal(Globals.TypeOfObject, "unwrappedMemberValue");
                            memberType = memberValue.LocalType;
                            ilg.Stloc(memberValue);
                            ilg.If(memberValue, Cmp.EqualTo, null);
                            ilg.Call(contextArg, XmlFormatGeneratorStatics.WriteNullMethod, xmlWriterArg, memberType, DataContract.IsTypeSerializable(memberType));
                            ilg.Else();
                        }
                        InternalSerialize((isNullableOfT ? XmlFormatGeneratorStatics.InternalSerializeMethod : XmlFormatGeneratorStatics.InternalSerializeReferenceMethod),
                            memberValue, memberType, false /* writeXsiType */);

                        if (memberType == Globals.TypeOfObject) //boxed Nullable<T>
                            ilg.EndIf();
                    }
                    ilg.EndIf();
                }
            }

            void InternalSerialize(MethodInfo methodInfo, LocalBuilder memberValue, Type memberType, bool writeXsiType)
            {
                ilg.Load(contextArg);
                ilg.Load(xmlWriterArg);
                ilg.Load(memberValue);
                ilg.ConvertValue(memberValue.LocalType, Globals.TypeOfObject);
                LocalBuilder typeHandleValue = ilg.DeclareLocal(typeof(RuntimeTypeHandle), "typeHandleValue");
                ilg.Call(null, typeof(Type).GetMethod("GetTypeHandle"), memberValue);
                ilg.Stloc(typeHandleValue);
                ilg.LoadAddress(typeHandleValue);
                ilg.Ldtoken(memberType);
                ilg.Call(typeof(RuntimeTypeHandle).GetMethod("Equals", new Type[] { typeof(RuntimeTypeHandle) }));
                ilg.Load(writeXsiType);
                ilg.Load(DataContract.GetId(memberType.TypeHandle));
                ilg.Ldtoken(memberType);
                ilg.Call(methodInfo);
            }

            LocalBuilder UnwrapNullableObject(LocalBuilder memberValue)// Leaves !HasValue on stack
            {
                Type memberType = memberValue.LocalType;
                Label onNull = ilg.DefineLabel();
                Label end = ilg.DefineLabel();
                ilg.Load(memberValue);
                while (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == Globals.TypeOfNullable)
                {
                    Type innerType = memberType.GetGenericArguments()[0];
                    ilg.Dup();
                    ilg.Call(XmlFormatGeneratorStatics.GetHasValueMethod.MakeGenericMethod(innerType));
                    ilg.Brfalse(onNull);
                    ilg.Call(XmlFormatGeneratorStatics.GetNullableValueMethod.MakeGenericMethod(innerType));
                    memberType = innerType;
                }
                memberValue = ilg.DeclareLocal(memberType, "nullableUnwrappedMemberValue");
                ilg.Stloc(memberValue);
                ilg.Load(false); //isNull
                ilg.Br(end);
                ilg.MarkLabel(onNull);
                ilg.Pop();
                ilg.Call(XmlFormatGeneratorStatics.GetDefaultValueMethod.MakeGenericMethod(memberType));
                ilg.Stloc(memberValue);
                ilg.Load(true); //isNull
                ilg.MarkLabel(end);
                return memberValue;
            }

            void WriteStartElement(LocalBuilder nameLocal, int nameIndex)
            {
                ilg.Load(xmlWriterArg);

                // localName
                if (nameLocal == null)
                    ilg.LoadArrayElement(memberNamesArg, nameIndex);
                else
                    ilg.Load(nameLocal);

                // namespace
                ilg.Load(null);

                if (nameLocal != null && nameLocal.LocalType == typeof(string))
                {
                    ilg.Call(JsonFormatGeneratorStatics.WriteStartElementStringMethod);
                }
                else
                {
                    ilg.Call(JsonFormatGeneratorStatics.WriteStartElementMethod);
                }
            }

            void WriteEndElement()
            {
                ilg.Call(xmlWriterArg, JsonFormatGeneratorStatics.WriteEndElementMethod);
            }
        }
    }
}
