//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Runtime.Serialization.Diagnostics.Application;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    delegate object JsonFormatClassReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContextComplexJson context, XmlDictionaryString emptyDictionaryString, XmlDictionaryString[] memberNames);
    delegate object JsonFormatCollectionReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContextComplexJson context, XmlDictionaryString emptyDictionaryString, XmlDictionaryString itemName, CollectionDataContract collectionContract);
    delegate void JsonFormatGetOnlyCollectionReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContextComplexJson context, XmlDictionaryString emptyDictionaryString, XmlDictionaryString itemName, CollectionDataContract collectionContract);

    sealed class JsonFormatReaderGenerator
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that was produced within an assert.")]
        [SecurityCritical]
        CriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.")]
        [SecurityCritical]
        public JsonFormatReaderGenerator()
        {
            helper = new CriticalHelper();
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        public JsonFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
        {


            try
            {
                if (TD.DCJsonGenReaderStartIsEnabled())
                {
                    TD.DCJsonGenReaderStart("Class", classContract.UnderlyingType.FullName);
                }

                return helper.GenerateClassReader(classContract);
            }
            finally
            {
                if (TD.DCJsonGenReaderStopIsEnabled())
                {
                    TD.DCJsonGenReaderStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        public JsonFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
        {
            try
            {
                if (TD.DCJsonGenReaderStartIsEnabled())
                {
                    TD.DCJsonGenReaderStart("Collection", collectionContract.StableName.Name);
                }

                return helper.GenerateCollectionReader(collectionContract);

            }
            finally
            {
                if (TD.DCJsonGenReaderStopIsEnabled())
                {
                    TD.DCJsonGenReaderStop();
                }
            }

        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        public JsonFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
        {

            try
            {
                if (TD.DCJsonGenReaderStartIsEnabled())
                {
                    TD.DCJsonGenReaderStart("GetOnlyCollection", collectionContract.UnderlyingType.FullName);
                }

                return helper.GenerateGetOnlyCollectionReader(collectionContract);

            }
            finally
            {
                if (TD.DCJsonGenReaderStopIsEnabled())
                {
                    TD.DCJsonGenReaderStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - handles all aspects of IL generation including initializing the DynamicMethod."
            + "Changes to how IL generated could affect how data is deserialized and what gets access to data, "
            + "therefore we mark it for review so that changes to generation logic are reviewed.")]
        class CriticalHelper
        {
            CodeGenerator ilg;
            LocalBuilder objectLocal;
            Type objectType;
            ArgBuilder xmlReaderArg;
            ArgBuilder contextArg;
            ArgBuilder memberNamesArg;
            ArgBuilder collectionContractArg;
            ArgBuilder emptyDictionaryStringArg;

            public JsonFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
            {
                ilg = new CodeGenerator();
                bool memberAccessFlag = classContract.RequiresMemberAccessForRead(null);
                try
                {
                    BeginMethod(ilg, "Read" + classContract.StableName.Name + "FromJson", typeof(JsonFormatClassReaderDelegate), memberAccessFlag);
                }
                catch (SecurityException securityException)
                {
                    if (memberAccessFlag && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        classContract.RequiresMemberAccessForRead(securityException);
                    }
                    else
                    {
                        throw;
                    }
                }
                InitArgs();
                DemandSerializationFormatterPermission(classContract);
                DemandMemberAccessPermission(memberAccessFlag);
                CreateObject(classContract);
                ilg.Call(contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, objectLocal);
                InvokeOnDeserializing(classContract);
                if (classContract.IsISerializable)
                    ReadISerializable(classContract);
                else
                    ReadClass(classContract);
                if (Globals.TypeOfIDeserializationCallback.IsAssignableFrom(classContract.UnderlyingType))
                    ilg.Call(objectLocal, JsonFormatGeneratorStatics.OnDeserializationMethod, null);
                InvokeOnDeserialized(classContract);
                if (!InvokeFactoryMethod(classContract))
                {
                    ilg.Load(objectLocal);
                    // Do a conversion back from DateTimeOffsetAdapter to DateTimeOffset after deserialization.
                    // DateTimeOffsetAdapter is used here for deserialization purposes to bypass the ISerializable implementation
                    // on DateTimeOffset; which does not work in partial trust.

                    if (classContract.UnderlyingType == Globals.TypeOfDateTimeOffsetAdapter)
                    {
                        ilg.ConvertValue(objectLocal.LocalType, Globals.TypeOfDateTimeOffsetAdapter);
                        ilg.Call(XmlFormatGeneratorStatics.GetDateTimeOffsetMethod);
                        ilg.ConvertValue(Globals.TypeOfDateTimeOffset, ilg.CurrentMethod.ReturnType);
                    }
                    else
                    {
                        ilg.ConvertValue(objectLocal.LocalType, ilg.CurrentMethod.ReturnType);
                    }
                }
                return (JsonFormatClassReaderDelegate)ilg.EndMethod();
            }

            public JsonFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
            {
                ilg = GenerateCollectionReaderHelper(collectionContract, false /*isGetOnlyCollection*/);
                ReadCollection(collectionContract);
                ilg.Load(objectLocal);
                ilg.ConvertValue(objectLocal.LocalType, ilg.CurrentMethod.ReturnType);
                return (JsonFormatCollectionReaderDelegate)ilg.EndMethod();
            }

            public JsonFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
            {
                ilg = GenerateCollectionReaderHelper(collectionContract, true /*isGetOnlyCollection*/);
                ReadGetOnlyCollection(collectionContract);
                return (JsonFormatGetOnlyCollectionReaderDelegate)ilg.EndMethod();
            }

            CodeGenerator GenerateCollectionReaderHelper(CollectionDataContract collectionContract, bool isGetOnlyCollection)
            {
                ilg = new CodeGenerator();
                bool memberAccessFlag = collectionContract.RequiresMemberAccessForRead(null);
                try
                {
                    if (isGetOnlyCollection)
                    {
                        BeginMethod(ilg, "Read" + collectionContract.StableName.Name + "FromJson" + "IsGetOnly", typeof(JsonFormatGetOnlyCollectionReaderDelegate), memberAccessFlag);
                    }
                    else
                    {
                        BeginMethod(ilg, "Read" + collectionContract.StableName.Name + "FromJson", typeof(JsonFormatCollectionReaderDelegate), memberAccessFlag);
                    }
                }
                catch (SecurityException securityException)
                {
                    if (memberAccessFlag && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        collectionContract.RequiresMemberAccessForRead(securityException);
                    }
                    else
                    {
                        throw;
                    }
                }
                InitArgs();
                DemandMemberAccessPermission(memberAccessFlag);
                collectionContractArg = ilg.GetArg(4);
                return ilg;
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

                DynamicMethod dynamicMethod = new DynamicMethod(methodName, signature.ReturnType, paramTypes, typeof(JsonFormatReaderGenerator).Module, allowPrivateMemberAccess);
                ilg.BeginMethod(dynamicMethod, delegateType, methodName, paramTypes, allowPrivateMemberAccess);
#endif
            }

            void InitArgs()
            {
                xmlReaderArg = ilg.GetArg(0);
                contextArg = ilg.GetArg(1);
                emptyDictionaryStringArg = ilg.GetArg(2);
                memberNamesArg = ilg.GetArg(3);
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


            void CreateObject(ClassDataContract classContract)
            {
                Type type = objectType = classContract.UnderlyingType;
                if (type.IsValueType && !classContract.IsNonAttributedType)
                    type = Globals.TypeOfValueType;

                objectLocal = ilg.DeclareLocal(type, "objectDeserialized");

                if (classContract.UnderlyingType == Globals.TypeOfDBNull)
                {
                    ilg.LoadMember(Globals.TypeOfDBNull.GetField("Value"));
                    ilg.Stloc(objectLocal);
                }
                else if (classContract.IsNonAttributedType)
                {
                    if (type.IsValueType)
                    {
                        ilg.Ldloca(objectLocal);
                        ilg.InitObj(type);
                    }
                    else
                    {
                        ilg.New(classContract.GetNonAttributedTypeConstructor());
                        ilg.Stloc(objectLocal);
                    }
                }
                else
                {
                    ilg.Call(null, JsonFormatGeneratorStatics.GetUninitializedObjectMethod, DataContract.GetIdForInitialization(classContract));
                    ilg.ConvertValue(Globals.TypeOfObject, type);
                    ilg.Stloc(objectLocal);
                }
            }

            void InvokeOnDeserializing(ClassDataContract classContract)
            {
                if (classContract.BaseContract != null)
                    InvokeOnDeserializing(classContract.BaseContract);
                if (classContract.OnDeserializing != null)
                {
                    ilg.LoadAddress(objectLocal);
                    ilg.ConvertAddress(objectLocal.LocalType, objectType);
                    ilg.Load(contextArg);
                    ilg.LoadMember(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                    ilg.Call(classContract.OnDeserializing);
                }
            }

            void InvokeOnDeserialized(ClassDataContract classContract)
            {
                if (classContract.BaseContract != null)
                    InvokeOnDeserialized(classContract.BaseContract);
                if (classContract.OnDeserialized != null)
                {
                    ilg.LoadAddress(objectLocal);
                    ilg.ConvertAddress(objectLocal.LocalType, objectType);
                    ilg.Load(contextArg);
                    ilg.LoadMember(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                    ilg.Call(classContract.OnDeserialized);
                }
            }

            bool HasFactoryMethod(ClassDataContract classContract)
            {
                return Globals.TypeOfIObjectReference.IsAssignableFrom(classContract.UnderlyingType);
            }

            bool InvokeFactoryMethod(ClassDataContract classContract)
            {
                if (HasFactoryMethod(classContract))
                {
                    ilg.Load(contextArg);
                    ilg.LoadAddress(objectLocal);
                    ilg.ConvertAddress(objectLocal.LocalType, Globals.TypeOfIObjectReference);
                    ilg.Load(Globals.NewObjectId);
                    ilg.Call(XmlFormatGeneratorStatics.GetRealObjectMethod);
                    ilg.ConvertValue(Globals.TypeOfObject, ilg.CurrentMethod.ReturnType);
                    return true;
                }
                return false;
            }

            void ReadClass(ClassDataContract classContract)
            {
                if (classContract.HasExtensionData)
                {
                    LocalBuilder extensionDataLocal = ilg.DeclareLocal(Globals.TypeOfExtensionDataObject, "extensionData");
                    ilg.New(JsonFormatGeneratorStatics.ExtensionDataObjectCtor);
                    ilg.Store(extensionDataLocal);
                    ReadMembers(classContract, extensionDataLocal);

                    ClassDataContract currentContract = classContract;
                    while (currentContract != null)
                    {
                        MethodInfo extensionDataSetMethod = currentContract.ExtensionDataSetMethod;
                        if (extensionDataSetMethod != null)
                            ilg.Call(objectLocal, extensionDataSetMethod, extensionDataLocal);
                        currentContract = currentContract.BaseContract;
                    }
                }
                else
                    ReadMembers(classContract, null /*extensionDataLocal*/);
            }

            void ReadMembers(ClassDataContract classContract, LocalBuilder extensionDataLocal)
            {
                int memberCount = classContract.MemberNames.Length;
                ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, memberCount);

                BitFlagsGenerator expectedElements = new BitFlagsGenerator(memberCount, ilg, classContract.UnderlyingType.Name + "_ExpectedElements");
                byte[] requiredElements = new byte[expectedElements.GetLocalCount()];
                SetRequiredElements(classContract, requiredElements);
                SetExpectedElements(expectedElements, 0 /*startIndex*/);

                LocalBuilder memberIndexLocal = ilg.DeclareLocal(Globals.TypeOfInt, "memberIndex", -1);
                Label throwDuplicateMemberLabel = ilg.DefineLabel();
                Label throwMissingRequiredMembersLabel = ilg.DefineLabel();

                object forReadElements = ilg.For(null, null, null);
                ilg.Call(null, XmlFormatGeneratorStatics.MoveToNextElementMethod, xmlReaderArg);
                ilg.IfFalseBreak(forReadElements);
                ilg.Call(contextArg, JsonFormatGeneratorStatics.GetJsonMemberIndexMethod, xmlReaderArg, memberNamesArg, memberIndexLocal, extensionDataLocal);
                if (memberCount > 0)
                {
                    Label[] memberLabels = ilg.Switch(memberCount);
                    ReadMembers(classContract, expectedElements, memberLabels, throwDuplicateMemberLabel, memberIndexLocal);
                    ilg.EndSwitch();
                }
                else
                {
                    ilg.Pop();
                }
                ilg.EndFor();
                CheckRequiredElements(expectedElements, requiredElements, throwMissingRequiredMembersLabel);
                Label endOfTypeLabel = ilg.DefineLabel();
                ilg.Br(endOfTypeLabel);

                ilg.MarkLabel(throwDuplicateMemberLabel);
                ilg.Call(null, JsonFormatGeneratorStatics.ThrowDuplicateMemberExceptionMethod, objectLocal, memberNamesArg, memberIndexLocal);

                ilg.MarkLabel(throwMissingRequiredMembersLabel);
                ilg.Load(objectLocal);
                ilg.ConvertValue(objectLocal.LocalType, Globals.TypeOfObject);
                ilg.Load(memberNamesArg);
                expectedElements.LoadArray();
                LoadArray(requiredElements, "requiredElements");
                ilg.Call(JsonFormatGeneratorStatics.ThrowMissingRequiredMembersMethod);

                ilg.MarkLabel(endOfTypeLabel);
            }

            int ReadMembers(ClassDataContract classContract, BitFlagsGenerator expectedElements,
                Label[] memberLabels, Label throwDuplicateMemberLabel, LocalBuilder memberIndexLocal)
            {
                int memberCount = (classContract.BaseContract == null) ? 0 :
                    ReadMembers(classContract.BaseContract, expectedElements, memberLabels, throwDuplicateMemberLabel, memberIndexLocal);

                for (int i = 0; i < classContract.Members.Count; i++, memberCount++)
                {
                    DataMember dataMember = classContract.Members[i];
                    Type memberType = dataMember.MemberType;
                    ilg.Case(memberLabels[memberCount], dataMember.Name);
                    ilg.Set(memberIndexLocal, memberCount);
                    expectedElements.Load(memberCount);
                    ilg.Brfalse(throwDuplicateMemberLabel);
                    LocalBuilder value = null;
                    if (dataMember.IsGetOnlyCollection)
                    {
                        ilg.LoadAddress(objectLocal);
                        ilg.LoadMember(dataMember.MemberInfo);
                        value = ilg.DeclareLocal(memberType, dataMember.Name + "Value");
                        ilg.Stloc(value);
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.StoreCollectionMemberInfoMethod, value);
                        ReadValue(memberType, dataMember.Name);
                    }
                    else
                    {
                        value = ReadValue(memberType, dataMember.Name);
                        ilg.LoadAddress(objectLocal);
                        ilg.ConvertAddress(objectLocal.LocalType, objectType);
                        ilg.Ldloc(value);
                        ilg.StoreMember(dataMember.MemberInfo);
                    }
                    ResetExpectedElements(expectedElements, memberCount);
                    ilg.EndCase();
                }
                return memberCount;
            }

            void CheckRequiredElements(BitFlagsGenerator expectedElements, byte[] requiredElements, Label throwMissingRequiredMembersLabel)
            {
                for (int i = 0; i < requiredElements.Length; i++)
                {
                    ilg.Load(expectedElements.GetLocal(i));
                    ilg.Load(requiredElements[i]);
                    ilg.And();
                    ilg.Load(0);
                    ilg.Ceq();
                    ilg.Brfalse(throwMissingRequiredMembersLabel);
                }
            }

            void LoadArray(byte[] array, string name)
            {
                LocalBuilder localArray = ilg.DeclareLocal(Globals.TypeOfByteArray, name);
                ilg.NewArray(typeof(byte), array.Length);
                ilg.Store(localArray);
                for (int i = 0; i < array.Length; i++)
                {
                    ilg.StoreArrayElement(localArray, i, array[i]);
                }
                ilg.Load(localArray);
            }

            int SetRequiredElements(ClassDataContract contract, byte[] requiredElements)
            {
                int memberCount = (contract.BaseContract == null) ? 0 :
                    SetRequiredElements(contract.BaseContract, requiredElements);
                List<DataMember> members = contract.Members;
                for (int i = 0; i < members.Count; i++, memberCount++)
                {
                    if (members[i].IsRequired)
                    {
                        BitFlagsGenerator.SetBit(requiredElements, memberCount);
                    }
                }
                return memberCount;
            }

            void SetExpectedElements(BitFlagsGenerator expectedElements, int startIndex)
            {
                int memberCount = expectedElements.GetBitCount();
                for (int i = startIndex; i < memberCount; i++)
                {
                    expectedElements.Store(i, true);
                }
            }

            void ResetExpectedElements(BitFlagsGenerator expectedElements, int index)
            {
                expectedElements.Store(index, false);
            }

            void ReadISerializable(ClassDataContract classContract)
            {
                ConstructorInfo ctor = classContract.UnderlyingType.GetConstructor(Globals.ScanAllMembers, null, JsonFormatGeneratorStatics.SerInfoCtorArgs, null);
                if (ctor == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.SerializationInfo_ConstructorNotFound, DataContract.GetClrTypeFullName(classContract.UnderlyingType))));
                ilg.LoadAddress(objectLocal);
                ilg.ConvertAddress(objectLocal.LocalType, objectType);
                ilg.Call(contextArg, XmlFormatGeneratorStatics.ReadSerializationInfoMethod, xmlReaderArg, classContract.UnderlyingType);
                ilg.Load(contextArg);
                ilg.LoadMember(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                ilg.Call(ctor);
            }

            LocalBuilder ReadValue(Type type, string name)
            {
                LocalBuilder value = ilg.DeclareLocal(type, "valueRead");
                LocalBuilder nullableValue = null;
                int nullables = 0;
                while (type.IsGenericType && type.GetGenericTypeDefinition() == Globals.TypeOfNullable)
                {
                    nullables++;
                    type = type.GetGenericArguments()[0];
                }

                PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(type);
                if ((primitiveContract != null && primitiveContract.UnderlyingType != Globals.TypeOfObject) || nullables != 0 || type.IsValueType)
                {
                    LocalBuilder objectId = ilg.DeclareLocal(Globals.TypeOfString, "objectIdRead");
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.ReadAttributesMethod, xmlReaderArg);
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.ReadIfNullOrRefMethod, xmlReaderArg, type, DataContract.IsTypeSerializable(type));
                    ilg.Stloc(objectId);
                    // Deserialize null
                    ilg.If(objectId, Cmp.EqualTo, Globals.NullObjectId);
                    if (nullables != 0)
                    {
                        ilg.LoadAddress(value);
                        ilg.InitObj(value.LocalType);
                    }
                    else if (type.IsValueType)
                        ThrowSerializationException(SR.GetString(SR.ValueTypeCannotBeNull, DataContract.GetClrTypeFullName(type)));
                    else
                    {
                        ilg.Load(null);
                        ilg.Stloc(value);
                    }

                    // Deserialize value

                    // Compare against Globals.NewObjectId, which is set to string.Empty
                    ilg.ElseIfIsEmptyString(objectId);
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.GetObjectIdMethod);
                    ilg.Stloc(objectId);
                    if (type.IsValueType)
                    {
                        ilg.IfNotIsEmptyString(objectId);
                        ThrowSerializationException(SR.GetString(SR.ValueTypeCannotHaveId, DataContract.GetClrTypeFullName(type)));
                        ilg.EndIf();
                    }
                    if (nullables != 0)
                    {
                        nullableValue = value;
                        value = ilg.DeclareLocal(type, "innerValueRead");
                    }

                    if (primitiveContract != null && primitiveContract.UnderlyingType != Globals.TypeOfObject)
                    {
                        ilg.Call(xmlReaderArg, primitiveContract.XmlFormatReaderMethod);
                        ilg.Stloc(value);
                        if (!type.IsValueType)
                            ilg.Call(contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, value);
                    }
                    else
                    {
                        InternalDeserialize(value, type, name);
                    }
                    // Deserialize ref
                    ilg.Else();
                    if (type.IsValueType)
                        ThrowSerializationException(SR.GetString(SR.ValueTypeCannotHaveRef, DataContract.GetClrTypeFullName(type)));
                    else
                    {
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.GetExistingObjectMethod, objectId, type, name, string.Empty);
                        ilg.ConvertValue(Globals.TypeOfObject, type);
                        ilg.Stloc(value);
                    }
                    ilg.EndIf();

                    if (nullableValue != null)
                    {
                        ilg.If(objectId, Cmp.NotEqualTo, Globals.NullObjectId);
                        WrapNullableObject(value, nullableValue, nullables);
                        ilg.EndIf();
                        value = nullableValue;
                    }
                }
                else
                {
                    InternalDeserialize(value, type, name);
                }

                return value;
            }

            void InternalDeserialize(LocalBuilder value, Type type, string name)
            {
                ilg.Load(contextArg);
                ilg.Load(xmlReaderArg);
                Type declaredType = type.IsPointer ? Globals.TypeOfReflectionPointer : type;
                ilg.Load(DataContract.GetId(declaredType.TypeHandle));
                ilg.Ldtoken(declaredType);
                ilg.Load(name);
                // Empty namespace
                ilg.Load(string.Empty);
                ilg.Call(XmlFormatGeneratorStatics.InternalDeserializeMethod);

                if (type.IsPointer)
                    ilg.Call(JsonFormatGeneratorStatics.UnboxPointer);
                else
                    ilg.ConvertValue(Globals.TypeOfObject, type);
                ilg.Stloc(value);
            }

            void WrapNullableObject(LocalBuilder innerValue, LocalBuilder outerValue, int nullables)
            {
                Type innerType = innerValue.LocalType, outerType = outerValue.LocalType;
                ilg.LoadAddress(outerValue);
                ilg.Load(innerValue);
                for (int i = 1; i < nullables; i++)
                {
                    Type type = Globals.TypeOfNullable.MakeGenericType(innerType);
                    ilg.New(type.GetConstructor(new Type[] { innerType }));
                    innerType = type;
                }
                ilg.Call(outerType.GetConstructor(new Type[] { innerType }));
            }

            void ReadCollection(CollectionDataContract collectionContract)
            {
                Type type = collectionContract.UnderlyingType;
                Type itemType = collectionContract.ItemType;
                bool isArray = (collectionContract.Kind == CollectionKind.Array);
                ConstructorInfo constructor = collectionContract.Constructor;
                if (type.IsInterface)
                {
                    switch (collectionContract.Kind)
                    {
                        case CollectionKind.GenericDictionary:
                            type = Globals.TypeOfDictionaryGeneric.MakeGenericType(itemType.GetGenericArguments());
                            constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Globals.EmptyTypeArray, null);
                            break;
                        case CollectionKind.Dictionary:
                            type = Globals.TypeOfHashtable;
                            constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Globals.EmptyTypeArray, null);
                            break;
                        case CollectionKind.Collection:
                        case CollectionKind.GenericCollection:
                        case CollectionKind.Enumerable:
                        case CollectionKind.GenericEnumerable:
                        case CollectionKind.List:
                        case CollectionKind.GenericList:
                            type = itemType.MakeArrayType();
                            isArray = true;
                            break;
                    }
                }

                objectLocal = ilg.DeclareLocal(type, "objectDeserialized");
                if (!isArray)
                {
                    if (type.IsValueType)
                    {
                        ilg.Ldloca(objectLocal);
                        ilg.InitObj(type);
                    }
                    else
                    {
                        ilg.New(constructor);
                        ilg.Stloc(objectLocal);
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, objectLocal);
                    }
                }

                bool canReadSimpleDictionary = collectionContract.Kind == CollectionKind.Dictionary ||
                                               collectionContract.Kind == CollectionKind.GenericDictionary;
                if (canReadSimpleDictionary)
                {
                    ilg.Load(contextArg);
                    ilg.LoadMember(JsonFormatGeneratorStatics.UseSimpleDictionaryFormatReadProperty);
                    ilg.If();

                    ReadSimpleDictionary(collectionContract, itemType);

                    ilg.Else();
                }

                LocalBuilder objectId = ilg.DeclareLocal(Globals.TypeOfString, "objectIdRead");
                ilg.Call(contextArg, XmlFormatGeneratorStatics.GetObjectIdMethod);
                ilg.Stloc(objectId);

                bool canReadPrimitiveArray = false;
                if (isArray && TryReadPrimitiveArray(itemType))
                {
                    canReadPrimitiveArray = true;
                    ilg.IfNot();
                }

                LocalBuilder growingCollection = null;
                if (isArray)
                {
                    growingCollection = ilg.DeclareLocal(type, "growingCollection");
                    ilg.NewArray(itemType, 32);
                    ilg.Stloc(growingCollection);
                }
                LocalBuilder i = ilg.DeclareLocal(Globals.TypeOfInt, "i");
                object forLoop = ilg.For(i, 0, Int32.MaxValue);
                // Empty namespace
                IsStartElement(memberNamesArg, emptyDictionaryStringArg);
                ilg.If();
                ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                LocalBuilder value = ReadCollectionItem(collectionContract, itemType);
                if (isArray)
                {
                    MethodInfo ensureArraySizeMethod = XmlFormatGeneratorStatics.EnsureArraySizeMethod.MakeGenericMethod(itemType);
                    ilg.Call(null, ensureArraySizeMethod, growingCollection, i);
                    ilg.Stloc(growingCollection);
                    ilg.StoreArrayElement(growingCollection, i, value);
                }
                else
                    StoreCollectionValue(objectLocal, value, collectionContract);
                ilg.Else();
                IsEndElement();
                ilg.If();
                ilg.Break(forLoop);
                ilg.Else();
                HandleUnexpectedItemInCollection(i);
                ilg.EndIf();
                ilg.EndIf();

                ilg.EndFor();
                if (isArray)
                {
                    MethodInfo trimArraySizeMethod = XmlFormatGeneratorStatics.TrimArraySizeMethod.MakeGenericMethod(itemType);
                    ilg.Call(null, trimArraySizeMethod, growingCollection, i);
                    ilg.Stloc(objectLocal);
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.AddNewObjectWithIdMethod, objectId, objectLocal);
                }

                if (canReadPrimitiveArray)
                {
                    ilg.Else();
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.AddNewObjectWithIdMethod, objectId, objectLocal);
                    ilg.EndIf();
                }

                if (canReadSimpleDictionary)
                {
                    ilg.EndIf();
                }
            }

            void ReadSimpleDictionary(CollectionDataContract collectionContract, Type keyValueType)
            {
                Type[] keyValueTypes = keyValueType.GetGenericArguments();
                Type keyType = keyValueTypes[0];
                Type valueType = keyValueTypes[1];

                int keyTypeNullableDepth = 0;
                Type keyTypeOriginal = keyType;
                while (keyType.IsGenericType && keyType.GetGenericTypeDefinition() == Globals.TypeOfNullable)
                {
                    keyTypeNullableDepth++;
                    keyType = keyType.GetGenericArguments()[0];
                }

                ClassDataContract keyValueDataContract = (ClassDataContract)collectionContract.ItemContract;
                DataContract keyDataContract = keyValueDataContract.Members[0].MemberTypeContract;

                KeyParseMode keyParseMode = KeyParseMode.Fail;

                if (keyType == Globals.TypeOfString || keyType == Globals.TypeOfObject)
                {
                    keyParseMode = KeyParseMode.AsString;
                }
                else if (keyType.IsEnum)
                {
                    keyParseMode = KeyParseMode.UsingParseEnum;
                }
                else if (keyDataContract.ParseMethod != null)
                {
                    keyParseMode = KeyParseMode.UsingCustomParse;
                }

                if (keyParseMode == KeyParseMode.Fail)
                {
                    ThrowSerializationException(
                        SR.GetString(
                            SR.KeyTypeCannotBeParsedInSimpleDictionary,
                                DataContract.GetClrTypeFullName(collectionContract.UnderlyingType),
                                DataContract.GetClrTypeFullName(keyType)));
                }
                else
                {
                    LocalBuilder nodeType = ilg.DeclareLocal(typeof(XmlNodeType), "nodeType");

                    ilg.BeginWhileCondition();
                    ilg.Call(xmlReaderArg, JsonFormatGeneratorStatics.MoveToContentMethod);
                    ilg.Stloc(nodeType);
                    ilg.Load(nodeType);
                    ilg.Load(XmlNodeType.EndElement);
                    ilg.BeginWhileBody(Cmp.NotEqualTo);

                    ilg.Load(nodeType);
                    ilg.Load(XmlNodeType.Element);
                    ilg.If(Cmp.NotEqualTo);
                    ThrowUnexpectedStateException(XmlNodeType.Element);
                    ilg.EndIf();

                    ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);

                    if (keyParseMode == KeyParseMode.UsingParseEnum)
                    {
                        ilg.Load(keyType);
                    }

                    ilg.Load(xmlReaderArg);
                    ilg.Call(JsonFormatGeneratorStatics.GetJsonMemberNameMethod);

                    if (keyParseMode == KeyParseMode.UsingParseEnum)
                    {
                        ilg.Call(JsonFormatGeneratorStatics.ParseEnumMethod);
                        ilg.ConvertValue(Globals.TypeOfObject, keyType);
                    }
                    else if (keyParseMode == KeyParseMode.UsingCustomParse)
                    {
                        ilg.Call(keyDataContract.ParseMethod);
                    }
                    LocalBuilder pairKey = ilg.DeclareLocal(keyType, "key");
                    ilg.Stloc(pairKey);
                    if (keyTypeNullableDepth > 0)
                    {
                        LocalBuilder pairKeyNullable = ilg.DeclareLocal(keyTypeOriginal, "keyOriginal");
                        WrapNullableObject(pairKey, pairKeyNullable, keyTypeNullableDepth);
                        pairKey = pairKeyNullable;
                    }

                    LocalBuilder pairValue = ReadValue(valueType, String.Empty);
                    StoreKeyValuePair(objectLocal, collectionContract, pairKey, pairValue);

                    ilg.EndWhile();
                }
            }

            void ReadGetOnlyCollection(CollectionDataContract collectionContract)
            {
                Type type = collectionContract.UnderlyingType;
                Type itemType = collectionContract.ItemType;
                bool isArray = (collectionContract.Kind == CollectionKind.Array);
                LocalBuilder size = ilg.DeclareLocal(Globals.TypeOfInt, "arraySize");

                objectLocal = ilg.DeclareLocal(type, "objectDeserialized");
                ilg.Load(contextArg);
                ilg.LoadMember(XmlFormatGeneratorStatics.GetCollectionMemberMethod);
                ilg.ConvertValue(Globals.TypeOfObject, type);
                ilg.Stloc(objectLocal);

                bool canReadSimpleDictionary = collectionContract.Kind == CollectionKind.Dictionary ||
                                               collectionContract.Kind == CollectionKind.GenericDictionary;
                if (canReadSimpleDictionary)
                {
                    ilg.Load(contextArg);
                    ilg.LoadMember(JsonFormatGeneratorStatics.UseSimpleDictionaryFormatReadProperty);
                    ilg.If();

                    ilg.If(objectLocal, Cmp.EqualTo, null);
                    ilg.Call(null, XmlFormatGeneratorStatics.ThrowNullValueReturnedForGetOnlyCollectionExceptionMethod, type);
                    ilg.Else();

                    ReadSimpleDictionary(collectionContract, itemType);

                    ilg.Call(contextArg, XmlFormatGeneratorStatics.CheckEndOfArrayMethod, xmlReaderArg, size, memberNamesArg, emptyDictionaryStringArg);

                    ilg.EndIf();

                    ilg.Else();
                }

                //check that items are actually going to be deserialized into the collection
                IsStartElement(memberNamesArg, emptyDictionaryStringArg);
                ilg.If();
                ilg.If(objectLocal, Cmp.EqualTo, null);
                ilg.Call(null, XmlFormatGeneratorStatics.ThrowNullValueReturnedForGetOnlyCollectionExceptionMethod, type);

                ilg.Else();

                if (isArray)
                {
                    ilg.Load(objectLocal);
                    ilg.Call(XmlFormatGeneratorStatics.GetArrayLengthMethod);
                    ilg.Stloc(size);
                }

                LocalBuilder i = ilg.DeclareLocal(Globals.TypeOfInt, "i");
                object forLoop = ilg.For(i, 0, Int32.MaxValue);
                IsStartElement(memberNamesArg, emptyDictionaryStringArg);
                ilg.If();
                ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                LocalBuilder value = ReadCollectionItem(collectionContract, itemType);
                if (isArray)
                {
                    ilg.If(size, Cmp.EqualTo, i);
                    ilg.Call(null, XmlFormatGeneratorStatics.ThrowArrayExceededSizeExceptionMethod, size, type);
                    ilg.Else();
                    ilg.StoreArrayElement(objectLocal, i, value);
                    ilg.EndIf();
                }
                else
                    StoreCollectionValue(objectLocal, value, collectionContract);
                ilg.Else();
                IsEndElement();
                ilg.If();
                ilg.Break(forLoop);
                ilg.Else();
                HandleUnexpectedItemInCollection(i);
                ilg.EndIf();
                ilg.EndIf();
                ilg.EndFor();
                ilg.Call(contextArg, XmlFormatGeneratorStatics.CheckEndOfArrayMethod, xmlReaderArg, size, memberNamesArg, emptyDictionaryStringArg);

                ilg.EndIf();
                ilg.EndIf();

                if (canReadSimpleDictionary)
                {
                    ilg.EndIf();
                }
            }

            bool TryReadPrimitiveArray(Type itemType)
            {
                PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(itemType);
                if (primitiveContract == null)
                    return false;

                string readArrayMethod = null;
                switch (Type.GetTypeCode(itemType))
                {
                    case TypeCode.Boolean:
                        readArrayMethod = "TryReadBooleanArray";
                        break;
                    case TypeCode.Decimal:
                        readArrayMethod = "TryReadDecimalArray";
                        break;
                    case TypeCode.Int32:
                        readArrayMethod = "TryReadInt32Array";
                        break;
                    case TypeCode.Int64:
                        readArrayMethod = "TryReadInt64Array";
                        break;
                    case TypeCode.Single:
                        readArrayMethod = "TryReadSingleArray";
                        break;
                    case TypeCode.Double:
                        readArrayMethod = "TryReadDoubleArray";
                        break;
                    case TypeCode.DateTime:
                        readArrayMethod = "TryReadJsonDateTimeArray";
                        break;
                    default:
                        break;
                }
                if (readArrayMethod != null)
                {
                    ilg.Load(xmlReaderArg);
                    ilg.ConvertValue(typeof(XmlReaderDelegator), typeof(JsonReaderDelegator));
                    ilg.Load(contextArg);
                    ilg.Load(memberNamesArg);
                    // Empty namespace
                    ilg.Load(emptyDictionaryStringArg);
                    // -1 Array Size
                    ilg.Load(-1);
                    ilg.Ldloca(objectLocal);
                    ilg.Call(typeof(JsonReaderDelegator).GetMethod(readArrayMethod, Globals.ScanAllMembers));
                    return true;
                }
                return false;
            }

            LocalBuilder ReadCollectionItem(CollectionDataContract collectionContract, Type itemType)
            {
                if (collectionContract.Kind == CollectionKind.Dictionary || collectionContract.Kind == CollectionKind.GenericDictionary)
                {
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.ResetAttributesMethod);
                    LocalBuilder value = ilg.DeclareLocal(itemType, "valueRead");
                    ilg.Load(collectionContractArg);
                    ilg.Call(JsonFormatGeneratorStatics.GetItemContractMethod);
                    ilg.Call(JsonFormatGeneratorStatics.GetRevisedItemContractMethod);
                    ilg.Load(xmlReaderArg);
                    ilg.Load(contextArg);
                    ilg.Call(JsonFormatGeneratorStatics.ReadJsonValueMethod);
                    ilg.ConvertValue(Globals.TypeOfObject, itemType);
                    ilg.Stloc(value);
                    return value;
                }
                else
                {
                    return ReadValue(itemType, JsonGlobals.itemString);
                }
            }

            void StoreCollectionValue(LocalBuilder collection, LocalBuilder value, CollectionDataContract collectionContract)
            {
                if (collectionContract.Kind == CollectionKind.GenericDictionary || collectionContract.Kind == CollectionKind.Dictionary)
                {
                    ClassDataContract keyValuePairContract = DataContract.GetDataContract(value.LocalType) as ClassDataContract;
                    if (keyValuePairContract == null)
                    {
                        Fx.Assert("Failed to create contract for KeyValuePair type");
                    }
                    DataMember keyMember = keyValuePairContract.Members[0];
                    DataMember valueMember = keyValuePairContract.Members[1];
                    LocalBuilder pairKey = ilg.DeclareLocal(keyMember.MemberType, keyMember.Name);
                    LocalBuilder pairValue = ilg.DeclareLocal(valueMember.MemberType, valueMember.Name);
                    ilg.LoadAddress(value);
                    ilg.LoadMember(keyMember.MemberInfo);
                    ilg.Stloc(pairKey);
                    ilg.LoadAddress(value);
                    ilg.LoadMember(valueMember.MemberInfo);
                    ilg.Stloc(pairValue);

                    StoreKeyValuePair(collection, collectionContract, pairKey, pairValue);
                }
                else
                {
                    ilg.Call(collection, collectionContract.AddMethod, value);
                    if (collectionContract.AddMethod.ReturnType != Globals.TypeOfVoid)
                        ilg.Pop();
                }
            }

            void StoreKeyValuePair(LocalBuilder collection, CollectionDataContract collectionContract, LocalBuilder pairKey, LocalBuilder pairValue)
            {
                ilg.Call(collection, collectionContract.AddMethod, pairKey, pairValue);
                if (collectionContract.AddMethod.ReturnType != Globals.TypeOfVoid)
                    ilg.Pop();
            }

            void HandleUnexpectedItemInCollection(LocalBuilder iterator)
            {
                IsStartElement();
                ilg.If();
                ilg.Call(contextArg, XmlFormatGeneratorStatics.SkipUnknownElementMethod, xmlReaderArg);
                ilg.Dec(iterator);
                ilg.Else();
                ThrowUnexpectedStateException(XmlNodeType.Element);
                ilg.EndIf();
            }

            void IsStartElement(ArgBuilder nameArg, ArgBuilder nsArg)
            {
                ilg.Call(xmlReaderArg, JsonFormatGeneratorStatics.IsStartElementMethod2, nameArg, nsArg);
            }

            void IsStartElement()
            {
                ilg.Call(xmlReaderArg, JsonFormatGeneratorStatics.IsStartElementMethod0);
            }

            void IsEndElement()
            {
                ilg.Load(xmlReaderArg);
                ilg.LoadMember(JsonFormatGeneratorStatics.NodeTypeProperty);
                ilg.Load(XmlNodeType.EndElement);
                ilg.Ceq();
            }

            void ThrowUnexpectedStateException(XmlNodeType expectedState)
            {
                ilg.Call(null, XmlFormatGeneratorStatics.CreateUnexpectedStateExceptionMethod, expectedState, xmlReaderArg);
                ilg.Throw();
            }

            void ThrowSerializationException(string msg, params object[] values)
            {
                if (values != null && values.Length > 0)
                    ilg.CallStringFormat(msg, values);
                else
                    ilg.Load(msg);
                ThrowSerializationException();
            }

            void ThrowSerializationException()
            {
                ilg.New(JsonFormatGeneratorStatics.SerializationExceptionCtor);
                ilg.Throw();
            }

            enum KeyParseMode
            {
                Fail,
                AsString,
                UsingParseEnum,
                UsingCustomParse
            }
        }
    }
}

