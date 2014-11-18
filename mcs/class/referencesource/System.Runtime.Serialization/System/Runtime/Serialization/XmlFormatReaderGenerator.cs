//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization.Diagnostics.Application;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

#if USE_REFEMIT
    public delegate object XmlFormatClassReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString[] memberNames, XmlDictionaryString[] memberNamespaces);
    public delegate object XmlFormatCollectionReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, CollectionDataContract collectionContract);
    public delegate void XmlFormatGetOnlyCollectionReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, CollectionDataContract collectionContract);

    public sealed class XmlFormatReaderGenerator
#else
    internal delegate object XmlFormatClassReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString[] memberNames, XmlDictionaryString[] memberNamespaces);
    internal delegate object XmlFormatCollectionReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, CollectionDataContract collectionContract);
    internal delegate void XmlFormatGetOnlyCollectionReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, CollectionDataContract collectionContract);

    internal sealed class XmlFormatReaderGenerator
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that was produced within an assert.")]
        [SecurityCritical]
        CriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.")]
        [SecurityCritical]
        public XmlFormatReaderGenerator()
        {
            helper = new CriticalHelper();
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        public XmlFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
        {
            try
            {
                if (TD.DCGenReaderStartIsEnabled())
                {
                    TD.DCGenReaderStart("Class", classContract.UnderlyingType.FullName);
                }

                return helper.GenerateClassReader(classContract);
            }
            finally
            {
                if (TD.DCGenReaderStopIsEnabled())
                {
                    TD.DCGenReaderStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        public XmlFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
        {
            try
            {
                if (TD.DCGenReaderStartIsEnabled())
                {
                    TD.DCGenReaderStart("Collection", collectionContract.UnderlyingType.FullName);
                }

                return helper.GenerateCollectionReader(collectionContract);
            }
            finally
            {
                if (TD.DCGenReaderStopIsEnabled())
                {
                    TD.DCGenReaderStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        public XmlFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
        {
            try
            {
                if (TD.DCGenReaderStartIsEnabled())
                {
                    TD.DCGenReaderStart("GetOnlyCollection", collectionContract.UnderlyingType.FullName);
                }

                return helper.GenerateGetOnlyCollectionReader(collectionContract);
            }
            finally
            {
                if (TD.DCGenReaderStopIsEnabled())
                {
                    TD.DCGenReaderStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Handles all aspects of IL generation including initializing the DynamicMethod."
            + " Changes to how IL generated could affect how data is deserialized and what gets access to data,"
            + " therefore we mark it for review so that changes to generation logic are reviewed.")]
        class CriticalHelper
        {
            CodeGenerator ilg;
            LocalBuilder objectLocal;
            Type objectType;
            ArgBuilder xmlReaderArg;
            ArgBuilder contextArg;
            ArgBuilder memberNamesArg;
            ArgBuilder memberNamespacesArg;
            ArgBuilder collectionContractArg;

            public XmlFormatClassReaderDelegate GenerateClassReader(ClassDataContract classContract)
            {
                ilg = new CodeGenerator();
                bool memberAccessFlag = classContract.RequiresMemberAccessForRead(null);
                try
                {
                    ilg.BeginMethod("Read" + classContract.StableName.Name + "FromXml", Globals.TypeOfXmlFormatClassReaderDelegate, memberAccessFlag);
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
                LocalBuilder objectId = null;
                if (HasFactoryMethod(classContract))
                {
                    objectId = ilg.DeclareLocal(Globals.TypeOfString, "objectIdRead");
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.GetObjectIdMethod);
                    ilg.Stloc(objectId);
                }
                if (classContract.IsISerializable)
                    ReadISerializable(classContract);
                else
                    ReadClass(classContract);
                bool isFactoryType = InvokeFactoryMethod(classContract, objectId);
                if (Globals.TypeOfIDeserializationCallback.IsAssignableFrom(classContract.UnderlyingType))
                    ilg.Call(objectLocal, XmlFormatGeneratorStatics.OnDeserializationMethod, null);
                InvokeOnDeserialized(classContract);
                if (objectId == null || !isFactoryType)
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
                return (XmlFormatClassReaderDelegate)ilg.EndMethod();
            }

            public XmlFormatCollectionReaderDelegate GenerateCollectionReader(CollectionDataContract collectionContract)
            {
                ilg = GenerateCollectionReaderHelper(collectionContract, false /*isGetOnlyCollection*/);
                ReadCollection(collectionContract);
                ilg.Load(objectLocal);
                ilg.ConvertValue(objectLocal.LocalType, ilg.CurrentMethod.ReturnType);
                return (XmlFormatCollectionReaderDelegate)ilg.EndMethod();
            }

            public XmlFormatGetOnlyCollectionReaderDelegate GenerateGetOnlyCollectionReader(CollectionDataContract collectionContract)
            {
                ilg = GenerateCollectionReaderHelper(collectionContract, true /*isGetOnlyCollection*/);
                ReadGetOnlyCollection(collectionContract);
                return (XmlFormatGetOnlyCollectionReaderDelegate)ilg.EndMethod();
            }

            CodeGenerator GenerateCollectionReaderHelper(CollectionDataContract collectionContract, bool isGetOnlyCollection)
            {
                ilg = new CodeGenerator();
                bool memberAccessFlag = collectionContract.RequiresMemberAccessForRead(null);
                try
                {
                    if (isGetOnlyCollection)
                    {
                        ilg.BeginMethod("Read" + collectionContract.StableName.Name + "FromXml" + "IsGetOnly", Globals.TypeOfXmlFormatGetOnlyCollectionReaderDelegate, memberAccessFlag);
                    }
                    else
                    {
                        ilg.BeginMethod("Read" + collectionContract.StableName.Name + "FromXml" + string.Empty, Globals.TypeOfXmlFormatCollectionReaderDelegate, memberAccessFlag);
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

            void InitArgs()
            {
                xmlReaderArg = ilg.GetArg(0);
                contextArg = ilg.GetArg(1);
                memberNamesArg = ilg.GetArg(2);
                memberNamespacesArg = ilg.GetArg(3);
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
                    ilg.Call(null, XmlFormatGeneratorStatics.GetUninitializedObjectMethod, DataContract.GetIdForInitialization(classContract));
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

            bool InvokeFactoryMethod(ClassDataContract classContract, LocalBuilder objectId)
            {
                if (HasFactoryMethod(classContract))
                {
                    ilg.Load(contextArg);
                    ilg.LoadAddress(objectLocal);
                    ilg.ConvertAddress(objectLocal.LocalType, Globals.TypeOfIObjectReference);
                    ilg.Load(objectId);
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
                    ilg.New(XmlFormatGeneratorStatics.ExtensionDataObjectCtor);
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

                LocalBuilder memberIndexLocal = ilg.DeclareLocal(Globals.TypeOfInt, "memberIndex", -1);

                int firstRequiredMember;
                bool[] requiredMembers = GetRequiredMembers(classContract, out firstRequiredMember);
                bool hasRequiredMembers = (firstRequiredMember < memberCount);
                LocalBuilder requiredIndexLocal = hasRequiredMembers ? ilg.DeclareLocal(Globals.TypeOfInt, "requiredIndex", firstRequiredMember) : null;

                object forReadElements = ilg.For(null, null, null);
                ilg.Call(null, XmlFormatGeneratorStatics.MoveToNextElementMethod, xmlReaderArg);
                ilg.IfFalseBreak(forReadElements);
                if (hasRequiredMembers)
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.GetMemberIndexWithRequiredMembersMethod, xmlReaderArg, memberNamesArg, memberNamespacesArg, memberIndexLocal, requiredIndexLocal, extensionDataLocal);
                else
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.GetMemberIndexMethod, xmlReaderArg, memberNamesArg, memberNamespacesArg, memberIndexLocal, extensionDataLocal);
                if (memberCount > 0)
                {
                    Label[] memberLabels = ilg.Switch(memberCount);
                    ReadMembers(classContract, requiredMembers, memberLabels, memberIndexLocal, requiredIndexLocal);
                    ilg.EndSwitch();
                }
                else
                {
                    ilg.Pop();
                }
                ilg.EndFor();
                if (hasRequiredMembers)
                {
                    ilg.If(requiredIndexLocal, Cmp.LessThan, memberCount);
                    ilg.Call(null, XmlFormatGeneratorStatics.ThrowRequiredMemberMissingExceptionMethod, xmlReaderArg, memberIndexLocal, requiredIndexLocal, memberNamesArg);
                    ilg.EndIf();
                }
            }

            int ReadMembers(ClassDataContract classContract, bool[] requiredMembers, Label[] memberLabels, LocalBuilder memberIndexLocal, LocalBuilder requiredIndexLocal)
            {
                int memberCount = (classContract.BaseContract == null) ? 0 : ReadMembers(classContract.BaseContract, requiredMembers,
                    memberLabels, memberIndexLocal, requiredIndexLocal);

                for (int i = 0; i < classContract.Members.Count; i++, memberCount++)
                {
                    DataMember dataMember = classContract.Members[i];
                    Type memberType = dataMember.MemberType;
                    ilg.Case(memberLabels[memberCount], dataMember.Name);
                    if (dataMember.IsRequired)
                    {
                        int nextRequiredIndex = memberCount + 1;
                        for (; nextRequiredIndex < requiredMembers.Length; nextRequiredIndex++)
                            if (requiredMembers[nextRequiredIndex])
                                break;
                        ilg.Set(requiredIndexLocal, nextRequiredIndex);
                    }

                    LocalBuilder value = null;

                    if (dataMember.IsGetOnlyCollection)
                    {
                        ilg.LoadAddress(objectLocal);
                        ilg.LoadMember(dataMember.MemberInfo);
                        value = ilg.DeclareLocal(memberType, dataMember.Name + "Value");
                        ilg.Stloc(value);
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.StoreCollectionMemberInfoMethod, value);
                        ReadValue(memberType, dataMember.Name, classContract.StableName.Namespace);
                    }
                    else
                    {
                        value = ReadValue(memberType, dataMember.Name, classContract.StableName.Namespace);
                        ilg.LoadAddress(objectLocal);
                        ilg.ConvertAddress(objectLocal.LocalType, objectType);
                        ilg.Ldloc(value);
                        ilg.StoreMember(dataMember.MemberInfo);
                    }
                    ilg.Set(memberIndexLocal, memberCount);
                    ilg.EndCase();
                }
                return memberCount;
            }

            bool[] GetRequiredMembers(ClassDataContract contract, out int firstRequiredMember)
            {
                int memberCount = contract.MemberNames.Length;
                bool[] requiredMembers = new bool[memberCount];
                GetRequiredMembers(contract, requiredMembers);
                for (firstRequiredMember = 0; firstRequiredMember < memberCount; firstRequiredMember++)
                    if (requiredMembers[firstRequiredMember])
                        break;
                return requiredMembers;
            }

            int GetRequiredMembers(ClassDataContract contract, bool[] requiredMembers)
            {
                int memberCount = (contract.BaseContract == null) ? 0 : GetRequiredMembers(contract.BaseContract, requiredMembers);
                List<DataMember> members = contract.Members;
                for (int i = 0; i < members.Count; i++, memberCount++)
                {
                    requiredMembers[memberCount] = members[i].IsRequired;
                }
                return memberCount;
            }

            void ReadISerializable(ClassDataContract classContract)
            {
                ConstructorInfo ctor = classContract.GetISerializableConstructor();
                ilg.LoadAddress(objectLocal);
                ilg.ConvertAddress(objectLocal.LocalType, objectType);
                ilg.Call(contextArg, XmlFormatGeneratorStatics.ReadSerializationInfoMethod, xmlReaderArg, classContract.UnderlyingType);
                ilg.Load(contextArg);
                ilg.LoadMember(XmlFormatGeneratorStatics.GetStreamingContextMethod);
                ilg.Call(ctor);
            }

            LocalBuilder ReadValue(Type type, string name, string ns)
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
                        ThrowValidationException(SR.GetString(SR.ValueTypeCannotBeNull, DataContract.GetClrTypeFullName(type)));
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
                        ThrowValidationException(SR.GetString(SR.ValueTypeCannotHaveId, DataContract.GetClrTypeFullName(type)));
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
                        InternalDeserialize(value, type, name, ns);
                    }
                    // Deserialize ref
                    ilg.Else();
                    if (type.IsValueType)
                        ThrowValidationException(SR.GetString(SR.ValueTypeCannotHaveRef, DataContract.GetClrTypeFullName(type)));
                    else
                    {
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.GetExistingObjectMethod, objectId, type, name, ns);
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
                    InternalDeserialize(value, type, name, ns);
                }

                return value;
            }

            void InternalDeserialize(LocalBuilder value, Type type, string name, string ns)
            {
                ilg.Load(contextArg);
                ilg.Load(xmlReaderArg);
                Type declaredType = type.IsPointer ? Globals.TypeOfReflectionPointer : type;
                ilg.Load(DataContract.GetId(declaredType.TypeHandle));
                ilg.Ldtoken(declaredType);
                ilg.Load(name);
                ilg.Load(ns);
                ilg.Call(XmlFormatGeneratorStatics.InternalDeserializeMethod);

                if (type.IsPointer)
                    ilg.Call(XmlFormatGeneratorStatics.UnboxPointer);
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
                            constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
                            break;
                        case CollectionKind.Dictionary:
                            type = Globals.TypeOfHashtable;
                            constructor = XmlFormatGeneratorStatics.HashtableCtor;
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
                string itemName = collectionContract.ItemName;
                string itemNs = collectionContract.StableName.Namespace;

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

                LocalBuilder size = ilg.DeclareLocal(Globals.TypeOfInt, "arraySize");
                ilg.Call(contextArg, XmlFormatGeneratorStatics.GetArraySizeMethod);
                ilg.Stloc(size);

                LocalBuilder objectId = ilg.DeclareLocal(Globals.TypeOfString, "objectIdRead");
                ilg.Call(contextArg, XmlFormatGeneratorStatics.GetObjectIdMethod);
                ilg.Stloc(objectId);

                bool canReadPrimitiveArray = false;
                if (isArray && TryReadPrimitiveArray(type, itemType, size))
                {
                    canReadPrimitiveArray = true;
                    ilg.IfNot();
                }

                ilg.If(size, Cmp.EqualTo, -1);

                LocalBuilder growingCollection = null;
                if (isArray)
                {
                    growingCollection = ilg.DeclareLocal(type, "growingCollection");
                    ilg.NewArray(itemType, 32);
                    ilg.Stloc(growingCollection);
                }
                LocalBuilder i = ilg.DeclareLocal(Globals.TypeOfInt, "i");
                object forLoop = ilg.For(i, 0, Int32.MaxValue);
                IsStartElement(memberNamesArg, memberNamespacesArg);
                ilg.If();
                ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                LocalBuilder value = ReadCollectionItem(collectionContract, itemType, itemName, itemNs);
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
                ilg.Else();

                ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, size);
                if (isArray)
                {
                    ilg.NewArray(itemType, size);
                    ilg.Stloc(objectLocal);
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, objectLocal);
                }
                LocalBuilder j = ilg.DeclareLocal(Globals.TypeOfInt, "j");
                ilg.For(j, 0, size);
                IsStartElement(memberNamesArg, memberNamespacesArg);
                ilg.If();
                LocalBuilder itemValue = ReadCollectionItem(collectionContract, itemType, itemName, itemNs);
                if (isArray)
                    ilg.StoreArrayElement(objectLocal, j, itemValue);
                else
                    StoreCollectionValue(objectLocal, itemValue, collectionContract);
                ilg.Else();
                HandleUnexpectedItemInCollection(j);
                ilg.EndIf();
                ilg.EndFor();
                ilg.Call(contextArg, XmlFormatGeneratorStatics.CheckEndOfArrayMethod, xmlReaderArg, size, memberNamesArg, memberNamespacesArg);
                ilg.EndIf();

                if (canReadPrimitiveArray)
                {
                    ilg.Else();
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.AddNewObjectWithIdMethod, objectId, objectLocal);
                    ilg.EndIf();
                }
            }

            void ReadGetOnlyCollection(CollectionDataContract collectionContract)
            {
                Type type = collectionContract.UnderlyingType;
                Type itemType = collectionContract.ItemType;
                bool isArray = (collectionContract.Kind == CollectionKind.Array);
                string itemName = collectionContract.ItemName;
                string itemNs = collectionContract.StableName.Namespace;

                objectLocal = ilg.DeclareLocal(type, "objectDeserialized");
                ilg.Load(contextArg);
                ilg.LoadMember(XmlFormatGeneratorStatics.GetCollectionMemberMethod);
                ilg.ConvertValue(Globals.TypeOfObject, type);
                ilg.Stloc(objectLocal);

                //check that items are actually going to be deserialized into the collection
                IsStartElement(memberNamesArg, memberNamespacesArg);
                ilg.If();
                ilg.If(objectLocal, Cmp.EqualTo, null);
                ilg.Call(null, XmlFormatGeneratorStatics.ThrowNullValueReturnedForGetOnlyCollectionExceptionMethod, type);

                ilg.Else();
                LocalBuilder size = ilg.DeclareLocal(Globals.TypeOfInt, "arraySize");
                if (isArray)
                {
                    ilg.Load(objectLocal);
                    ilg.Call(XmlFormatGeneratorStatics.GetArrayLengthMethod);
                    ilg.Stloc(size);
                }

                ilg.Call(contextArg, XmlFormatGeneratorStatics.AddNewObjectMethod, objectLocal);

                LocalBuilder i = ilg.DeclareLocal(Globals.TypeOfInt, "i");
                object forLoop = ilg.For(i, 0, Int32.MaxValue);
                IsStartElement(memberNamesArg, memberNamespacesArg);
                ilg.If();
                ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                LocalBuilder value = ReadCollectionItem(collectionContract, itemType, itemName, itemNs);
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
                ilg.Call(contextArg, XmlFormatGeneratorStatics.CheckEndOfArrayMethod, xmlReaderArg, size, memberNamesArg, memberNamespacesArg);

                ilg.EndIf();
                ilg.EndIf();
            }

            bool TryReadPrimitiveArray(Type type, Type itemType, LocalBuilder size)
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
                    case TypeCode.DateTime:
                        readArrayMethod = "TryReadDateTimeArray";
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
                    default:
                        break;
                }
                if (readArrayMethod != null)
                {
                    ilg.Load(xmlReaderArg);
                    ilg.Load(contextArg);
                    ilg.Load(memberNamesArg);
                    ilg.Load(memberNamespacesArg);
                    ilg.Load(size);
                    ilg.Ldloca(objectLocal);
                    ilg.Call(typeof(XmlReaderDelegator).GetMethod(readArrayMethod, Globals.ScanAllMembers));
                    return true;
                }
                return false;
            }

            LocalBuilder ReadCollectionItem(CollectionDataContract collectionContract, Type itemType, string itemName, string itemNs)
            {
                if (collectionContract.Kind == CollectionKind.Dictionary || collectionContract.Kind == CollectionKind.GenericDictionary)
                {
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.ResetAttributesMethod);
                    LocalBuilder value = ilg.DeclareLocal(itemType, "valueRead");
                    ilg.Load(collectionContractArg);
                    ilg.Call(XmlFormatGeneratorStatics.GetItemContractMethod);
                    ilg.Load(xmlReaderArg);
                    ilg.Load(contextArg);
                    ilg.Call(XmlFormatGeneratorStatics.ReadXmlValueMethod);
                    ilg.ConvertValue(Globals.TypeOfObject, itemType);
                    ilg.Stloc(value);
                    return value;
                }
                else
                {
                    return ReadValue(itemType, itemName, itemNs);
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

                    ilg.Call(collection, collectionContract.AddMethod, pairKey, pairValue);
                    if (collectionContract.AddMethod.ReturnType != Globals.TypeOfVoid)
                        ilg.Pop();
                }
                else
                {
                    ilg.Call(collection, collectionContract.AddMethod, value);
                    if (collectionContract.AddMethod.ReturnType != Globals.TypeOfVoid)
                        ilg.Pop();
                }
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
                ilg.Call(xmlReaderArg, XmlFormatGeneratorStatics.IsStartElementMethod2, nameArg, nsArg);
            }

            void IsStartElement()
            {
                ilg.Call(xmlReaderArg, XmlFormatGeneratorStatics.IsStartElementMethod0);
            }

            void IsEndElement()
            {
                ilg.Load(xmlReaderArg);
                ilg.LoadMember(XmlFormatGeneratorStatics.NodeTypeProperty);
                ilg.Load(XmlNodeType.EndElement);
                ilg.Ceq();
            }

            void ThrowUnexpectedStateException(XmlNodeType expectedState)
            {
                ilg.Call(null, XmlFormatGeneratorStatics.CreateUnexpectedStateExceptionMethod, expectedState, xmlReaderArg);
                ilg.Throw();
            }

            void ThrowValidationException(string msg, params object[] values)
            {
                if (values != null && values.Length > 0)
                    ilg.CallStringFormat(msg, values);
                else
                    ilg.Load(msg);
                ThrowValidationException();
            }

            void ThrowValidationException()
            {
                ilg.New(XmlFormatGeneratorStatics.SerializationExceptionCtor);
                ilg.Throw();
            }

        }

        [Fx.Tag.SecurityNote(Critical = "Elevates by calling GetUninitializedObject which has a LinkDemand.",
            Safe = "Marked as such so that it's callable from transparent generated IL. Takes id as parameter which "
            + " is guaranteed to be in internal serialization cache.")]
        [SecuritySafeCritical]
#if USE_REFEMIT
        public static object UnsafeGetUninitializedObject(int id)
#else
        static internal object UnsafeGetUninitializedObject(int id)
#endif
        {
            return FormatterServices.GetUninitializedObject(DataContract.GetDataContractForInitialization(id).TypeForInitialization);
        }
    }
}

