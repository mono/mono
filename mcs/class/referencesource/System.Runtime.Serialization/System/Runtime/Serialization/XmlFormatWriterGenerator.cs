//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.Serialization.Diagnostics.Application;
    using System.Xml;

#if USE_REFEMIT
    public delegate void XmlFormatClassWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context,ClassDataContract dataContract);
    public delegate void XmlFormatCollectionWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context, CollectionDataContract dataContract);

    public sealed class XmlFormatWriterGenerator
#else
    internal delegate void XmlFormatClassWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context, ClassDataContract dataContract);
    internal delegate void XmlFormatCollectionWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context, CollectionDataContract dataContract);

    internal sealed class XmlFormatWriterGenerator
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that was produced within an assert.")]
        [SecurityCritical]
        CriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.")]
        [SecurityCritical]
        public XmlFormatWriterGenerator()
        {
            helper = new CriticalHelper();
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        internal XmlFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
        {
            try
            {
                if (TD.DCGenWriterStartIsEnabled())
                {
                    TD.DCGenWriterStart("Class", classContract.UnderlyingType.FullName);
                }

                return helper.GenerateClassWriter(classContract);
            }
            finally
            {
                if (TD.DCGenWriterStopIsEnabled())
                {
                    TD.DCGenWriterStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.")]
        [SecurityCritical]
        internal XmlFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
        {
            try
            {
                if (TD.DCGenWriterStartIsEnabled())
                {
                    TD.DCGenWriterStart("Collection", collectionContract.UnderlyingType.FullName);
                }

                return helper.GenerateCollectionWriter(collectionContract);

            }
            finally
            {
                if (TD.DCGenWriterStopIsEnabled())
                {
                    TD.DCGenWriterStop();
                }
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Handles all aspects of IL generation including initializing the DynamicMethod."
            + " Changes to how IL generated could affect how data is serialized and what gets access to data,"
            + " therefore we mark it for review so that changes to generation logic are reviewed.")]
        class CriticalHelper
        {
            CodeGenerator ilg;
            ArgBuilder xmlWriterArg;
            ArgBuilder contextArg;
            ArgBuilder dataContractArg;
            LocalBuilder objectLocal;

            // Used for classes
            LocalBuilder contractNamespacesLocal;
            LocalBuilder memberNamesLocal;
            LocalBuilder childElementNamespacesLocal;
            int typeIndex = 1;
            int childElementIndex = 0;

            internal XmlFormatClassWriterDelegate GenerateClassWriter(ClassDataContract classContract)
            {
                ilg = new CodeGenerator();
                bool memberAccessFlag = classContract.RequiresMemberAccessForWrite(null);
                try
                {
                    ilg.BeginMethod("Write" + classContract.StableName.Name + "ToXml", Globals.TypeOfXmlFormatClassWriterDelegate, memberAccessFlag);
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
                DemandSerializationFormatterPermission(classContract);
                DemandMemberAccessPermission(memberAccessFlag);
                if (classContract.IsReadOnlyContract)
                {
                    ThrowIfCannotSerializeReadOnlyTypes(classContract);
                }
                WriteClass(classContract);
                return (XmlFormatClassWriterDelegate)ilg.EndMethod();
            }

            internal XmlFormatCollectionWriterDelegate GenerateCollectionWriter(CollectionDataContract collectionContract)
            {
                ilg = new CodeGenerator();
                bool memberAccessFlag = collectionContract.RequiresMemberAccessForWrite(null);
                try
                {
                    ilg.BeginMethod("Write" + collectionContract.StableName.Name + "ToXml", Globals.TypeOfXmlFormatCollectionWriterDelegate, memberAccessFlag);
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
                return (XmlFormatCollectionWriterDelegate)ilg.EndMethod();
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
                    ilg.Call(contextArg, XmlFormatGeneratorStatics.WriteISerializableMethod, xmlWriterArg, objectLocal);
                else
                {
                    if (classContract.ContractNamespaces.Length > 1)
                    {
                        contractNamespacesLocal = ilg.DeclareLocal(typeof(XmlDictionaryString[]), "contractNamespaces");
                        ilg.Load(dataContractArg);
                        ilg.LoadMember(XmlFormatGeneratorStatics.ContractNamespacesField);
                        ilg.Store(contractNamespacesLocal);
                    }

                    memberNamesLocal = ilg.DeclareLocal(typeof(XmlDictionaryString[]), "memberNames");
                    ilg.Load(dataContractArg);
                    ilg.LoadMember(XmlFormatGeneratorStatics.MemberNamesField);
                    ilg.Store(memberNamesLocal);

                    for (int i = 0; i < classContract.ChildElementNamespaces.Length; i++)
                    {
                        if (classContract.ChildElementNamespaces[i] != null)
                        {
                            childElementNamespacesLocal = ilg.DeclareLocal(typeof(XmlDictionaryString[]), "childElementNamespaces");
                            ilg.Load(dataContractArg);
                            ilg.LoadMember(XmlFormatGeneratorStatics.ChildElementNamespacesProperty);
                            ilg.Store(childElementNamespacesLocal);
                        }
                    }

                    if (classContract.HasExtensionData)
                    {
                        LocalBuilder extensionDataLocal = ilg.DeclareLocal(Globals.TypeOfExtensionDataObject, "extensionData");
                        ilg.Load(objectLocal);
                        ilg.ConvertValue(objectLocal.LocalType, Globals.TypeOfIExtensibleDataObject);
                        ilg.LoadMember(XmlFormatGeneratorStatics.ExtensionDataProperty);
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

                LocalBuilder namespaceLocal = ilg.DeclareLocal(typeof(XmlDictionaryString), "ns");
                if (contractNamespacesLocal == null)
                {
                    ilg.Load(dataContractArg);
                    ilg.LoadMember(XmlFormatGeneratorStatics.NamespaceProperty);
                }
                else
                    ilg.LoadArrayElement(contractNamespacesLocal, typeIndex - 1);
                ilg.Store(namespaceLocal);

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
                    bool writeXsiType = CheckIfMemberHasConflict(member, classContract, derivedMostClassContract);
                    if (writeXsiType || !TryWritePrimitive(memberType, memberValue, member.MemberInfo, null /*arrayItemIndex*/, namespaceLocal, null /*nameLocal*/, i + childElementIndex))
                    {
                        WriteStartElement(memberType, classContract.Namespace, namespaceLocal, null /*nameLocal*/, i + childElementIndex);
                        if (classContract.ChildElementNamespaces[i + childElementIndex] != null)
                        {
                            ilg.Load(xmlWriterArg);
                            ilg.LoadArrayElement(childElementNamespacesLocal, i + childElementIndex);
                            ilg.Call(XmlFormatGeneratorStatics.WriteNamespaceDeclMethod);
                        }
                        if (memberValue == null)
                            memberValue = LoadMemberValue(member);
                        WriteValue(memberValue, writeXsiType);
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
                LocalBuilder itemNamespace = ilg.DeclareLocal(typeof(XmlDictionaryString), "itemNamespace");
                ilg.Load(dataContractArg);
                ilg.LoadMember(XmlFormatGeneratorStatics.NamespaceProperty);
                ilg.Store(itemNamespace);

                LocalBuilder itemName = ilg.DeclareLocal(typeof(XmlDictionaryString), "itemName");
                ilg.Load(dataContractArg);
                ilg.LoadMember(XmlFormatGeneratorStatics.CollectionItemNameProperty);
                ilg.Store(itemName);

                if (collectionContract.ChildElementNamespace != null)
                {
                    ilg.Load(xmlWriterArg);
                    ilg.Load(dataContractArg);
                    ilg.LoadMember(XmlFormatGeneratorStatics.ChildElementNamespaceProperty);
                    ilg.Call(XmlFormatGeneratorStatics.WriteNamespaceDeclMethod);
                }

                if (collectionContract.Kind == CollectionKind.Array)
                {
                    Type itemType = collectionContract.ItemType;
                    LocalBuilder i = ilg.DeclareLocal(Globals.TypeOfInt, "i");

                    ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementArrayCountMethod, xmlWriterArg, objectLocal);

                    if (!TryWritePrimitiveArray(collectionContract.UnderlyingType, itemType, objectLocal, itemName, itemNamespace))
                    {
                        ilg.For(i, 0, objectLocal);
                        if (!TryWritePrimitive(itemType, null /*value*/, null /*memberInfo*/, i /*arrayItemIndex*/, itemNamespace, itemName, 0 /*nameIndex*/))
                        {
                            WriteStartElement(itemType, collectionContract.Namespace, itemNamespace, itemName, 0 /*nameIndex*/);
                            ilg.LoadArrayElement(objectLocal, i);
                            LocalBuilder memberValue = ilg.DeclareLocal(itemType, "memberValue");
                            ilg.Stloc(memberValue);
                            WriteValue(memberValue, false /*writeXsiType*/);
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
                                moveNextMethod = XmlFormatGeneratorStatics.MoveNextMethod;
                            if (getCurrentMethod == null)
                                getCurrentMethod = XmlFormatGeneratorStatics.GetCurrentMethod;
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
                        ilg.ConvertValue(collectionContract.GetEnumeratorMethod.ReturnType, Globals.TypeOfIDictionaryEnumerator);
                        ilg.New(XmlFormatGeneratorStatics.DictionaryEnumeratorCtor);
                    }
                    else if (isGenericDictionary)
                    {
                        Type ctorParam = Globals.TypeOfIEnumeratorGeneric.MakeGenericType(Globals.TypeOfKeyValuePair.MakeGenericType(keyValueTypes));
                        ConstructorInfo dictEnumCtor = enumeratorType.GetConstructor(Globals.ScanAllMembers, null, new Type[] { ctorParam }, null);
                        ilg.ConvertValue(collectionContract.GetEnumeratorMethod.ReturnType, ctorParam);
                        ilg.New(dictEnumCtor);
                    }
                    ilg.Stloc(enumerator);

                    ilg.ForEach(currentValue, elementType, enumeratorType, enumerator, getCurrentMethod);
                    if (incrementCollectionCountMethod == null)
                    {
                        ilg.Call(contextArg, XmlFormatGeneratorStatics.IncrementItemCountMethod, 1);
                    }
                    if (!TryWritePrimitive(elementType, currentValue, null /*memberInfo*/, null /*arrayItemIndex*/, itemNamespace, itemName, 0 /*nameIndex*/))
                    {
                        WriteStartElement(elementType, collectionContract.Namespace, itemNamespace, itemName, 0 /*nameIndex*/);

                        if (isGenericDictionary || isDictionary)
                        {
                            ilg.Call(dataContractArg, XmlFormatGeneratorStatics.GetItemContractMethod);
                            ilg.Load(xmlWriterArg);
                            ilg.Load(currentValue);
                            ilg.ConvertValue(currentValue.LocalType, Globals.TypeOfObject);
                            ilg.Load(contextArg);
                            ilg.Call(XmlFormatGeneratorStatics.WriteXmlValueMethod);
                        }
                        else
                        {
                            WriteValue(currentValue, false /*writeXsiType*/);
                        }
                        WriteEndElement();
                    }
                    ilg.EndForEach(moveNextMethod);
                }
            }

            bool TryWritePrimitive(Type type, LocalBuilder value, MemberInfo memberInfo, LocalBuilder arrayItemIndex, LocalBuilder ns, LocalBuilder name, int nameIndex)
            {
                PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(type);
                if (primitiveContract == null || primitiveContract.UnderlyingType == Globals.TypeOfObject)
                    return false;

                // load xmlwriter
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
                    ilg.LoadArrayElement(memberNamesLocal, nameIndex);
                }
                // load namespace
                ilg.Load(ns);
                // call method to write primitive
                ilg.Call(primitiveContract.XmlFormatWriterMethod);
                return true;
            }

            bool TryWritePrimitiveArray(Type type, Type itemType, LocalBuilder value, LocalBuilder itemName, LocalBuilder itemNamespace)
            {
                PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(itemType);
                if (primitiveContract == null)
                    return false;

                string writeArrayMethod = null;
                switch (Type.GetTypeCode(itemType))
                {
                    case TypeCode.Boolean:
                        writeArrayMethod = "WriteBooleanArray";
                        break;
                    case TypeCode.DateTime:
                        writeArrayMethod = "WriteDateTimeArray";
                        break;
                    case TypeCode.Decimal:
                        writeArrayMethod = "WriteDecimalArray";
                        break;
                    case TypeCode.Int32:
                        writeArrayMethod = "WriteInt32Array";
                        break;
                    case TypeCode.Int64:
                        writeArrayMethod = "WriteInt64Array";
                        break;
                    case TypeCode.Single:
                        writeArrayMethod = "WriteSingleArray";
                        break;
                    case TypeCode.Double:
                        writeArrayMethod = "WriteDoubleArray";
                        break;
                    default:
                        break;
                }
                if (writeArrayMethod != null)
                {
                    ilg.Load(xmlWriterArg);
                    ilg.Load(value);
                    ilg.Load(itemName);
                    ilg.Load(itemNamespace);
                    ilg.Call(typeof(XmlWriterDelegator).GetMethod(writeArrayMethod, Globals.ScanAllMembers, null, new Type[] { type, typeof(XmlDictionaryString), typeof(XmlDictionaryString) }, null));
                    return true;
                }
                return false;
            }

            void WriteValue(LocalBuilder memberValue, bool writeXsiType)
            {
                Type memberType = memberValue.LocalType;
                if (memberType.IsPointer)
                {
                    ilg.Load(memberValue);
                    ilg.Load(memberType);
                    ilg.Call(XmlFormatGeneratorStatics.BoxPointer);
                    memberType = Globals.TypeOfReflectionPointer;
                    memberValue = ilg.DeclareLocal(memberType, "memberValueRefPointer");
                    ilg.Store(memberValue);
                }
                bool isNullableOfT = (memberType.IsGenericType &&
                                      memberType.GetGenericTypeDefinition() == Globals.TypeOfNullable);
                if (memberType.IsValueType && !isNullableOfT)
                {
                    PrimitiveDataContract primitiveContract = PrimitiveDataContract.GetPrimitiveDataContract(memberType);
                    if (primitiveContract != null && !writeXsiType)
                        ilg.Call(xmlWriterArg, primitiveContract.XmlFormatContentWriterMethod, memberValue);
                    else
                        InternalSerialize(XmlFormatGeneratorStatics.InternalSerializeMethod, memberValue, memberType, writeXsiType);
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
                    if (primitiveContract != null && primitiveContract.UnderlyingType != Globals.TypeOfObject && !writeXsiType)
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
                            memberValue, memberType, writeXsiType);

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

            bool NeedsPrefix(Type type, XmlDictionaryString ns)
            {
                return type == Globals.TypeOfXmlQualifiedName && (ns != null && ns.Value != null && ns.Value.Length > 0);
            }

            void WriteStartElement(Type type, XmlDictionaryString ns, LocalBuilder namespaceLocal, LocalBuilder nameLocal, int nameIndex)
            {
                bool needsPrefix = NeedsPrefix(type, ns);
                ilg.Load(xmlWriterArg);
                // prefix
                if (needsPrefix)
                    ilg.Load(Globals.ElementPrefix);

                // localName
                if (nameLocal == null)
                    ilg.LoadArrayElement(memberNamesLocal, nameIndex);
                else
                    ilg.Load(nameLocal);

                // namespace
                ilg.Load(namespaceLocal);

                ilg.Call(needsPrefix ? XmlFormatGeneratorStatics.WriteStartElementMethod3 : XmlFormatGeneratorStatics.WriteStartElementMethod2);
            }

            void WriteEndElement()
            {
                ilg.Call(xmlWriterArg, XmlFormatGeneratorStatics.WriteEndElementMethod);
            }

            bool CheckIfMemberHasConflict(DataMember member, ClassDataContract classContract, ClassDataContract derivedMostClassContract)
            {
                // Check for conflict with base type members
                if (CheckIfConflictingMembersHaveDifferentTypes(member))
                    return true;

                // Check for conflict with derived type members
                string name = member.Name;
                string ns = classContract.StableName.Namespace;
                ClassDataContract currentContract = derivedMostClassContract;
                while (currentContract != null && currentContract != classContract)
                {
                    if (ns == currentContract.StableName.Namespace)
                    {
                        List<DataMember> members = currentContract.Members;
                        for (int j = 0; j < members.Count; j++)
                        {
                            if (name == members[j].Name)
                                return CheckIfConflictingMembersHaveDifferentTypes(members[j]);
                        }
                    }
                    currentContract = currentContract.BaseContract;
                }

                return false;
            }

            bool CheckIfConflictingMembersHaveDifferentTypes(DataMember member)
            {
                while (member.ConflictingMember != null)
                {
                    if (member.MemberType != member.ConflictingMember.MemberType)
                        return true;
                    member = member.ConflictingMember;
                }
                return false;
            }

#if NotUsed
        static Hashtable nsToPrefixTable = new Hashtable(4);
        internal static string GetPrefix(string ns)
        {
            string prefix = (string)nsToPrefixTable[ns];
            if (prefix == null)
            {
                lock (nsToPrefixTable)
                {
                    if (prefix == null)
                    {
                        prefix = "p" + nsToPrefixTable.Count;
                        nsToPrefixTable.Add(ns, prefix);
                    }
                }
            }
            return prefix;
        }
#endif

        }
    }
}


