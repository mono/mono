// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
 **
 ** Class: SerObjectInfo
 **
 **
 ** Purpose: Holds information about an objects Members
 **
 **
 ===========================================================*/


namespace System.Runtime.Serialization.Formatters.Binary
{
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Threading;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Diagnostics;
    using System.Globalization;
    using System.Diagnostics.Contracts;
    using System.Security;
    using System.Security.Permissions;

    // This class contains information about an object. It is used so that
    // the rest of the Formatter routines can use a common interface for
    // a normal object, an ISerializable object, and a surrogate object
    //
    // The methods in this class are for the internal use of the Formatters.
    // There use will be restricted when signing is supported for assemblies
    internal sealed class WriteObjectInfo
    {
        internal int objectInfoId;

        internal Object obj;
        internal Type objectType;

        internal bool isSi = false;
        internal bool isNamed = false;
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal bool isTyped = false;
#pragma warning restore 0414
        internal bool isArray = false;

        internal SerializationInfo si = null;

        internal SerObjectInfoCache cache = null;

        internal Object[] memberData = null;
        internal ISerializationSurrogate serializationSurrogate = null;

        internal StreamingContext context;

        internal SerObjectInfoInit serObjectInfoInit = null;

        // Writing and Parsing information
        internal long objectId;
        internal long assemId;

        // Binder information
        string binderTypeName;
        string binderAssemblyString;

        internal WriteObjectInfo()
        {
        }

        internal void ObjectEnd()
        {
            SerTrace.Log( this, objectInfoId," objectType ",objectType," ObjectEnd");
            PutObjectInfo(serObjectInfoInit, this);
        }

        private void InternalInit()
        {
            SerTrace.Log( this, objectInfoId," objectType ",objectType," InternalInit");
            obj = null;
            objectType = null;
            isSi = false;
            isNamed = false;
            isTyped = false;
            isArray = false;
            si = null;
            cache = null;
            memberData = null;

            // Writing and Parsing information
            objectId = 0;
            assemId = 0;

            // Binder information
            binderTypeName = null;
            binderAssemblyString = null;
        }


        [System.Security.SecurityCritical]  // auto-generated
        internal static WriteObjectInfo Serialize(Object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter, SerializationBinder binder)
        {
            WriteObjectInfo soi = GetObjectInfo(serObjectInfoInit);

            soi.InitSerialize(obj, surrogateSelector, context, serObjectInfoInit, converter, objectWriter, binder);
            return soi;
        }

        // Write constructor
        [System.Security.SecurityCritical]  // auto-generated
        internal void InitSerialize(Object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter, SerializationBinder binder)
        {
            SerTrace.Log( this, objectInfoId," Constructor 1 ",obj);
            this.context = context;
            this.obj = obj;
            this.serObjectInfoInit = serObjectInfoInit;
            ISurrogateSelector surrogateSelectorTemp;

#if  FEATURE_REMOTING        
            if (RemotingServices.IsTransparentProxy(obj))
                objectType = Converter.typeofMarshalByRefObject;
            else
#endif
                objectType = obj.GetType();

            if (objectType.IsArray)
            {
                isArray = true;
                InitNoMembers();
                return;
            }

            InvokeSerializationBinder(binder);

            SerTrace.Log( this, objectInfoId," Constructor 1 trace 2");

            objectWriter.ObjectManager.RegisterObject(obj);
            if (surrogateSelector != null && (serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out surrogateSelectorTemp)) != null)
            {
                SerTrace.Log( this, objectInfoId," Constructor 1 trace 3");
                si = new SerializationInfo(objectType, converter);
                if (!objectType.IsPrimitive)
                    serializationSurrogate.GetObjectData(obj, si, context);
                InitSiWrite();
            }
            else if (obj is ISerializable)
            {
                if (!objectType.IsSerializable) {
                    throw new SerializationException(Environment.GetResourceString("Serialization_NonSerType",
                                                                   objectType.FullName, objectType.Assembly.FullName));
                }
                si = new SerializationInfo(objectType, converter, !FormatterServices.UnsafeTypeForwardersIsEnabled());
#if FEATURE_SERIALIZATION
                ((ISerializable)obj).GetObjectData(si, context);
#endif
                SerTrace.Log( this, objectInfoId," Constructor 1 trace 4 ISerializable "+objectType);
                InitSiWrite();
                CheckTypeForwardedFrom(cache, objectType, binderAssemblyString);
            }
            else
            {
                SerTrace.Log(this, objectInfoId," Constructor 1 trace 5");
                InitMemberInfo();
                CheckTypeForwardedFrom(cache, objectType, binderAssemblyString);
            }
        }

        [Conditional("SER_LOGGING")]
        private void DumpMemberInfo()
        {
            for (int i=0; i<cache.memberInfos.Length; i++)
            {
                SerTrace.Log( this, objectInfoId," Constructor 1 memberInfos data ",cache.memberInfos[i].Name," ",memberData[i]);

            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static WriteObjectInfo Serialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, SerializationBinder binder)
        {
            WriteObjectInfo soi = GetObjectInfo(serObjectInfoInit);
            soi.InitSerialize(objectType, surrogateSelector, context, serObjectInfoInit, converter, binder);
            return soi;
        }

        // Write Constructor used for array types or null members
        [System.Security.SecurityCritical]  // auto-generated
        internal void InitSerialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, SerializationBinder binder)
        {

            SerTrace.Log( this, objectInfoId," Constructor 2 ",objectType);

            this.objectType = objectType;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;

            if (objectType.IsArray)
            {
                InitNoMembers();
                return;
            }

            InvokeSerializationBinder(binder);

            ISurrogateSelector surrogateSelectorTemp = null;

            if (surrogateSelector!=null)
                serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out surrogateSelectorTemp);

            if (serializationSurrogate != null)
            {
                // surrogate does not have this problem since user has pass in through the BF's ctor
                si = new SerializationInfo(objectType, converter);
                cache = new SerObjectInfoCache(objectType);

                isSi = true;
            }
            else if (Object.ReferenceEquals(objectType, Converter.typeofObject))
            {
            }
            else if (Converter.typeofISerializable.IsAssignableFrom(objectType))
            {
                si = new SerializationInfo(objectType, converter, !FormatterServices.UnsafeTypeForwardersIsEnabled());
                cache = new SerObjectInfoCache(objectType);
                CheckTypeForwardedFrom(cache, objectType, binderAssemblyString);

                isSi = true;
            }

            if (!isSi)
            {
                InitMemberInfo();
                CheckTypeForwardedFrom(cache, objectType, binderAssemblyString);
            }

            SerTrace.Log( this,objectInfoId," ", objectType," InitSerialize Exit ",isSi);
        }


        private void InitSiWrite()
        {
            SerTrace.Log( this, objectInfoId," InitSiWrite Entry ");

            SerializationInfoEnumerator siEnum = null;
            isSi = true;
            siEnum = si.GetEnumerator();
            int infoLength = 0;

            infoLength = si.MemberCount;

            int count = infoLength;

            // For ISerializable cache cannot be saved because each object instance can have different values
            // BinaryWriter only puts the map on the wire if the ISerializable map cannot be reused.
            TypeInformation typeInformation = null;
            string fullTypeName = si.FullTypeName;
            string assemblyString = si.AssemblyName;
            bool hasTypeForwardedFrom = false;

            if (!si.IsFullTypeNameSetExplicit)
            {
                typeInformation = BinaryFormatter.GetTypeInformation(si.ObjectType);
                fullTypeName = typeInformation.FullTypeName;
                hasTypeForwardedFrom = typeInformation.HasTypeForwardedFrom;
            }

            if (!si.IsAssemblyNameSetExplicit)
            {
                if (typeInformation == null)
                {
                    typeInformation = BinaryFormatter.GetTypeInformation(si.ObjectType);
                }
                assemblyString = typeInformation.AssemblyString;
                hasTypeForwardedFrom = typeInformation.HasTypeForwardedFrom;
            }

            cache = new SerObjectInfoCache(fullTypeName, assemblyString, hasTypeForwardedFrom);

            cache.memberNames = new String[count];
            cache.memberTypes = new Type[count];
            memberData = new Object[count];

            siEnum = si.GetEnumerator();
            for (int i=0; siEnum.MoveNext(); i++)
            {
                cache.memberNames[i] = siEnum.Name;
                cache.memberTypes[i] = siEnum.ObjectType;
                memberData[i] = siEnum.Value;
                SerTrace.Log( this,objectInfoId+" ",objectType," InitSiWrite ",cache.memberNames[i]," Type ",cache.memberTypes[i]," data ",memberData[i]);
            }

            isNamed = true;
            isTyped = false;

            SerTrace.Log(this, objectInfoId," InitSiWrite Exit ");
        }

        private static void CheckTypeForwardedFrom(SerObjectInfoCache cache, Type objectType, string binderAssemblyString)
        {
            // If we're about to use the [TypeForwardedFrom] attribute for the assembly name
            if (cache.hasTypeForwardedFrom && binderAssemblyString == null)
            {
                if (!FormatterServices.UnsafeTypeForwardersIsEnabled())
                {
                    Assembly objectAssembly = objectType.Assembly;

                    // cache.assemblyString will be set to the value of the AssemblyFullName set on the TypeForwardedFrom attribute
                    if (!SerializationInfo.IsAssemblyNameAssignmentSafe(objectAssembly.FullName, cache.assemblyString)
                        && !objectAssembly.IsFullyTrusted)
                    {
                        // if the object assembly is partially trusted, we will block the TypeForwardedFrom case
                        throw new SecurityException(Environment.GetResourceString("Serialization_RequireFullTrust", objectType));
                    }
                }
            }
        }

        private void InitNoMembers()
        {
            cache = (SerObjectInfoCache)serObjectInfoInit.seenBeforeTable[objectType];
            if (cache == null)
            {
                SerTrace.Log( this,objectInfoId," ", objectType," InitMemberInfo new cache");
                cache = new SerObjectInfoCache(objectType);
                serObjectInfoInit.seenBeforeTable.Add(objectType, cache);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void InitMemberInfo()
        {
            SerTrace.Log( this,objectInfoId," ", objectType," InitMemberInfo Entry");


            cache = (SerObjectInfoCache)serObjectInfoInit.seenBeforeTable[objectType];
            if (cache == null)
            {
                SerTrace.Log( this,objectInfoId," ", objectType," InitMemberInfo new cache");
                cache = new SerObjectInfoCache(objectType);

                cache.memberInfos = FormatterServices.GetSerializableMembers(objectType, context);
                int count = cache.memberInfos.Length;
                cache.memberNames = new String[count];
                cache.memberTypes = new Type[count];

                // Calculate new arrays
                for (int i=0; i<count; i++)
                {
                    cache.memberNames[i] = cache.memberInfos[i].Name;
                    cache.memberTypes[i] = GetMemberType(cache.memberInfos[i]);
                    SerTrace.Log( this, objectInfoId," InitMemberInfo name ",cache.memberNames[i],", type ",cache.memberTypes[i],", memberInfoType ",cache.memberInfos[i].GetType());
                }
                serObjectInfoInit.seenBeforeTable.Add(objectType, cache);
            }

            if (obj != null)
            {
                memberData = FormatterServices.GetObjectData(obj, cache.memberInfos);
                DumpMemberInfo();
            }

            isTyped = true;
            isNamed = true;
            SerTrace.Log( this,objectInfoId," ", objectType," InitMemberInfo Exit");
        }



        // Return type name for the object.

        internal  String GetTypeFullName()
        {
            SerTrace.Log( this,objectInfoId," ", objectType," GetTypeFullName isSi ",isSi, " "+cache.fullTypeName);
            return binderTypeName ?? cache.fullTypeName;
        }

        internal  String GetAssemblyString()
        {
            SerTrace.Log( this,objectInfoId," ", objectType," GetAssemblyString Entry isSi ",isSi, " ",cache.assemblyString);
            return binderAssemblyString ?? cache.assemblyString;
        }

        private void InvokeSerializationBinder(SerializationBinder binder)
        {
            if (binder != null)
            {
                binder.BindToName(objectType, out binderAssemblyString, out binderTypeName);
            }
        }


        // Retrieves the member type from the MemberInfo

        internal  Type GetMemberType(MemberInfo objMember)
        {
            Type objectType = null;

            if (objMember is FieldInfo)
            {
                objectType = ((FieldInfo)objMember).FieldType;
                SerTrace.Log( this, objectInfoId," ", "GetMemberType FieldInfo ",objectType);
            }
            else if (objMember is PropertyInfo)
            {
                objectType = ((PropertyInfo)objMember).PropertyType;
                SerTrace.Log( this,objectInfoId," ", "GetMemberType PropertyInfo ",objectType);
            }
            else
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_SerMemberInfo",objMember.GetType()));
            }

            return objectType;
        }

        internal  void GetMemberInfo(out String[] outMemberNames, out Type[] outMemberTypes, out Object[] outMemberData)
        {
            outMemberNames = cache.memberNames;
            outMemberTypes = cache.memberTypes;
            outMemberData = memberData;

            if (isSi)
            {
                if (!isNamed)
                    throw new SerializationException(Environment.GetResourceString("Serialization_ISerializableMemberInfo"));
            }
        }

        private static WriteObjectInfo GetObjectInfo(SerObjectInfoInit serObjectInfoInit)
        {
            WriteObjectInfo objectInfo = null;

            if (!serObjectInfoInit.oiPool.IsEmpty())
            {
                objectInfo = (WriteObjectInfo)serObjectInfoInit.oiPool.Pop();
                objectInfo.InternalInit();
                //SerTrace.Log( "GetObjectInfo",objectInfo.objectInfoId," GetObjectInfo from pool");
            }
            else
            {
                objectInfo = new WriteObjectInfo();
                objectInfo.objectInfoId = serObjectInfoInit.objectInfoIdCount++;
                //SerTrace.Log( "GetObjectInfo",objectInfo.objectInfoId," GetObjectInfo new not from pool");
            }

            return objectInfo;
        }

        private static void PutObjectInfo(SerObjectInfoInit serObjectInfoInit, WriteObjectInfo objectInfo)
        {
            serObjectInfoInit.oiPool.Push(objectInfo);
            //SerTrace.Log( "PutObjectInfo",objectInfo.objectInfoId," PutObjectInfo to pool");
        }
    }

    internal sealed class ReadObjectInfo
    {
        internal int objectInfoId;
        internal static int readObjectInfoCounter;

        internal Type objectType;

        internal ObjectManager objectManager;

        internal int count;

        internal bool isSi = false;
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal bool isNamed = false;
#pragma warning restore 0414
        internal bool isTyped = false;
        internal bool bSimpleAssembly = false;

        internal SerObjectInfoCache cache;

        internal String[] wireMemberNames;
        internal Type[] wireMemberTypes;

        private int lastPosition = 0;

// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal ISurrogateSelector surrogateSelector = null;
#pragma warning restore 0414
        internal ISerializationSurrogate serializationSurrogate = null;

        internal StreamingContext context;


        // Si Read
        internal List<Type> memberTypesList;

        internal SerObjectInfoInit serObjectInfoInit = null;

        internal IFormatterConverter formatterConverter;

        internal ReadObjectInfo()
        {
        }

        internal void ObjectEnd()
        {
            SerTrace.Log( this, objectInfoId," objectType ",objectType," ObjectEnd");
        }

        internal void PrepareForReuse()
        {
            lastPosition = 0;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static ReadObjectInfo Create(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            ReadObjectInfo soi = GetObjectInfo(serObjectInfoInit);
            soi.Init(objectType, surrogateSelector, context, objectManager, serObjectInfoInit, converter, bSimpleAssembly);
            return soi;
        }


        // Read Constructor
        [System.Security.SecurityCritical]  // auto-generated
        internal void Init(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {

            SerTrace.Log( this, objectInfoId," Constructor 3 ",objectType);

            this.objectType = objectType;
            this.objectManager = objectManager;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.formatterConverter = converter;
            this.bSimpleAssembly = bSimpleAssembly;

            InitReadConstructor(objectType, surrogateSelector, context);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static ReadObjectInfo Create(Type objectType, String[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            ReadObjectInfo soi = GetObjectInfo(serObjectInfoInit);
            soi.Init(objectType, memberNames,memberTypes, surrogateSelector, context, objectManager, serObjectInfoInit, converter, bSimpleAssembly);
            return soi;
        }

        // Read Constructor
        [System.Security.SecurityCritical]  // auto-generated
        internal void Init(Type objectType, String[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
        {
            SerTrace.Log( this,objectInfoId, " Constructor 5 ",objectType);
            this.objectType = objectType;
            this.objectManager = objectManager;
            this.wireMemberNames = memberNames;
            this.wireMemberTypes = memberTypes;
            this.context = context;
            this.serObjectInfoInit = serObjectInfoInit;
            this.formatterConverter = converter;
            this.bSimpleAssembly = bSimpleAssembly;
            if (memberNames != null)
                isNamed = true;
            if (memberTypes != null)
                isTyped = true;

            if ((object)objectType != null)
                InitReadConstructor(objectType, surrogateSelector, context);
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void InitReadConstructor(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context)
        {
            SerTrace.Log( this,objectInfoId," ", objectType," InitReadConstructor Entry ",objectType);

            if (objectType.IsArray)
            {
                InitNoMembers();
                return;
            }

            ISurrogateSelector surrogateSelectorTemp = null;

            if (surrogateSelector!=null)
                serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out surrogateSelectorTemp);

            if (serializationSurrogate != null)
            {
                isSi = true;
            }
            else if (Object.ReferenceEquals(objectType, Converter.typeofObject))
            {
            }
            else if (Converter.typeofISerializable.IsAssignableFrom(objectType))
                isSi = true;

            if (isSi)
            {
                InitSiRead();
            }
            else
            {
                InitMemberInfo();
            }
            SerTrace.Log( this,objectInfoId," ", objectType," InitReadConstructor Exit ",isSi);
        }

        private void InitSiRead()
        {
            if (memberTypesList != null)
            {
                memberTypesList = new List<Type>(20);
            }
        }

        private void InitNoMembers()
        {
            SerTrace.Log( this,objectInfoId," ", objectType," InitMemberInfo new cache");
            cache = new SerObjectInfoCache(objectType);
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void InitMemberInfo()
        {
            SerTrace.Log( this,objectInfoId," ", objectType," InitMemberInfo Entry");

            SerTrace.Log( this,objectInfoId," ", objectType," InitMemberInfo new cache");
            cache = new SerObjectInfoCache(objectType);
            cache.memberInfos = FormatterServices.GetSerializableMembers(objectType, context);
            count = cache.memberInfos.Length;
            cache.memberNames = new String[count];
            cache.memberTypes = new Type[count];

            // Calculate new arrays
            for (int i=0; i<count; i++)
            {
                cache.memberNames[i] = cache.memberInfos[i].Name;
                cache.memberTypes[i] = GetMemberType(cache.memberInfos[i]);
                SerTrace.Log( this, objectInfoId," InitMemberInfo name ",cache.memberNames[i],", type ",cache.memberTypes[i],", memberInfoType ",cache.memberInfos[i].GetType());
            }

            isTyped = true;
            isNamed = true;
            SerTrace.Log( this,objectInfoId," ", objectType," InitMemberInfo Exit");
        }

        // Get the memberInfo for a memberName
        internal  MemberInfo GetMemberInfo(String name)
        {
            SerTrace.Log( this,objectInfoId," ", objectType," GetMemberInfo Entry ",name);
            if (cache == null)
                return null;
            if (isSi)
                throw new SerializationException(Environment.GetResourceString("Serialization_MemberInfo",objectType+" "+name));
            if (cache.memberInfos == null)
                throw new SerializationException(Environment.GetResourceString("Serialization_NoMemberInfo",objectType+" "+name));
            int position = Position(name);
            if (position != -1)
                return cache.memberInfos[Position(name)];
            else
                return null;
        }

        // Get the ObjectType for a memberName
        internal  Type GetType(String name)
        {
            SerTrace.Log( this,objectInfoId," ", objectType," GetType Entry ",name);
            Type type = null;
            int position = Position(name);
            if (position == -1)
                return null;
            
            if (isTyped)
                type = cache.memberTypes[position];
            else
                type = (Type)memberTypesList[position];

            if ((object)type == null)
                throw new SerializationException(Environment.GetResourceString("Serialization_ISerializableTypes",objectType+" "+name));

            SerTrace.Log( this,objectInfoId," ", objectType," GetType Exit ",type);
            return type;
        }


        // Adds the value for a memberName
        internal  void AddValue(String name, Object value, ref SerializationInfo si, ref Object[] memberData)
        {
            SerTrace.Log( this,objectInfoId," ", objectType," AddValue ",name," ",value," isSi ",isSi);
            if (isSi)
            {
                si.AddValue(name, value);
            }
            else
            {
                //                Console.WriteLine("Calling add value for " + name + " with value " + value);
                int position = Position(name);

                // If a member in the stream is not found, ignore it
                if (position != -1)
                    memberData[position] = value;
            }
        }

        internal void InitDataStore(ref SerializationInfo si, ref Object[] memberData)
        {
            if (isSi)
            {
                if (si == null)
                    si = new SerializationInfo(objectType, formatterConverter);
            }
            else
            {
                if (memberData == null && cache != null)
                    memberData = new Object[cache.memberNames.Length];
            }
        }


        // Records an objectId in a member when the actual object for that member is not yet known
        internal  void RecordFixup(long objectId, String name, long idRef)
        {

            if (isSi)
            {
                SerTrace.Log( this,objectInfoId," ", objectType, " RecordFixup  RecordDelayedFixup objectId ",objectId," name ",name," idRef ",idRef," isSi ",isSi);
                objectManager.RecordDelayedFixup(objectId, name, idRef);
            }
            else
            {
                SerTrace.Log( this,objectInfoId," ", objectType," RecordFixup  objectId ",objectId," name ",name," idRef ",idRef," isSi ",isSi);                                            
                int position = Position(name);
                if (position != -1)
                    objectManager.RecordFixup(objectId, cache.memberInfos[position], idRef);
            }
        }

        // Fills in the values for an object
        [System.Security.SecurityCritical]  // auto-generated
        internal  void PopulateObjectMembers(Object obj, Object[] memberData)
        {
            SerTrace.Log( this,objectInfoId," ", objectType," PopulateObjectMembers  isSi ",isSi);
            if (!isSi && memberData != null)
            {
                DumpPopulate(cache.memberInfos, memberData);

                FormatterServices.PopulateObjectMembers(obj, cache.memberInfos, memberData);
            }
        }

        [Conditional("SER_LOGGING")]
        private void DumpPopulate(MemberInfo[] memberInfos, Object[] memberData)
        {
            for (int i=0; i<memberInfos.Length; i++)
            {
                SerTrace.Log( this,objectInfoId," ", objectType," PopulateObjectMembers ",memberInfos[i].Name," ",memberData[i]);

            }
        }

        [Conditional("SER_LOGGING")]
        private void DumpPopulateSi()
        {
            //SerTrace.Log( this,objectInfoId," ", objectType," PopulateObjectMembers SetObjectData, ISerializable obj ");
            //SerializationInfoEnumerator siEnum = si.GetEnumerator();
            //for (int i=0; siEnum.MoveNext(); i++)
            //{
            //    SerTrace.Log( this,objectInfoId," ",objectType," Populate Si ",siEnum.Name," ",siEnum.Value);
            //}
        }

        // Specifies the position in the memberNames array of this name

        private int Position(String name)
        {
            SerTrace.Log( this, objectInfoId," Position ",lastPosition," ",name);
            if (cache == null)
                return -1;
            
            if (cache.memberNames.Length >0 && cache.memberNames[lastPosition].Equals(name))
            {
                return lastPosition;
            }
            else if ((++lastPosition < cache.memberNames.Length) && (cache.memberNames[lastPosition].Equals(name)))
            {
                return lastPosition;
            }
            else
            {
                // Search for name
                SerTrace.Log( this, objectInfoId," Position miss search for name "+name);
                for (int i=0; i<cache.memberNames.Length; i++)
                {
                    if (cache.memberNames[i].Equals(name))
                    {
                        lastPosition = i;
                        return lastPosition;
                    }
                }

                //throw new SerializationException(String.Format(Environment.GetResourceString("Serialization_MissingMember"),name,objectType));
                lastPosition = 0;
                return -1;
            }
        }

        // Return the member Types in order of memberNames
        internal  Type[] GetMemberTypes(String[] inMemberNames, Type objectType)
        {
            if (isSi)
                throw new SerializationException(Environment.GetResourceString("Serialization_ISerializableTypes",objectType));

            //Contract.Assert(cache!=null, "[ReadObjectInfo::GetMemberTypes] cache!=null");
            if (cache == null)
                return null;

            if (cache.memberTypes == null)
            {
                cache.memberTypes = new Type[count];
                for (int i = 0; i<count; i++)
                    cache.memberTypes[i] = GetMemberType(cache.memberInfos[i]);
            }

            bool memberMissing = false;
            // If the field length in the stream is < cache a member is missing
            if (inMemberNames.Length < cache.memberInfos.Length)
                memberMissing = true;

            Type[] outMemberTypes = new Type[cache.memberInfos.Length];

            bool isFound = false;

            for (int i = 0; i < cache.memberInfos.Length; i++)
            {
                if (!memberMissing && inMemberNames[i].Equals(cache.memberInfos[i].Name))
                    outMemberTypes[i] = cache.memberTypes[i];
                else
                {
                    // MemberNames on wire in different order then memberInfos returned by reflection
                    isFound = false;
                    for (int j = 0; j < inMemberNames.Length; j++)
                    {
                        if (cache.memberInfos[i].Name.Equals(inMemberNames[j]))
                        {
                            outMemberTypes[i] = cache.memberTypes[i];
                            SerTrace.Log( this,objectInfoId," ", objectType," GetMemberTypes memberName ",cache.memberTypes[i]," ",i," memberTypes ",outMemberTypes[j]," ",j);
                            isFound = true;
                            break;
                        }
                    }
                    if (!isFound)
                    {
                        // A field on the type isnt found. See if the field has OptionallySerializable and the type has the deserialization constructor
                        Object [] attrs = cache.memberInfos[i].GetCustomAttributes(typeof(OptionalFieldAttribute), false);
                        if ((attrs == null || attrs.Length == 0) && !bSimpleAssembly){
                            // the member isnt optionally serializable
                            throw new SerializationException(Environment.GetResourceString("Serialization_MissingMember", cache.memberNames[i], objectType, typeof(OptionalFieldAttribute).FullName));
                        }
                    }
                }
            }
            return outMemberTypes;
        }




        // Retrieves the member type from the MemberInfo
        internal  Type GetMemberType(MemberInfo objMember)
        {
            Type objectType = null;

            if (objMember is FieldInfo)
            {
                objectType = ((FieldInfo)objMember).FieldType;
                SerTrace.Log( this, objectInfoId," ", "GetMemberType FieldInfo ",objectType);
            }
            else if (objMember is PropertyInfo)
            {
                objectType = ((PropertyInfo)objMember).PropertyType;
                SerTrace.Log( this,objectInfoId," ", "GetMemberType PropertyInfo ",objectType);
            }
            else
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_SerMemberInfo",objMember.GetType()));
            }

            return objectType;
        }


        private static ReadObjectInfo GetObjectInfo(SerObjectInfoInit serObjectInfoInit)
        {
            ReadObjectInfo roi =  new ReadObjectInfo();
            roi.objectInfoId = Interlocked.Increment(ref readObjectInfoCounter);
            return roi;
        }

    }


    internal sealed class SerObjectInfoInit
    {
        internal Hashtable seenBeforeTable = new Hashtable();
        internal int objectInfoIdCount = 1;
        internal SerStack oiPool = new SerStack("SerObjectInfo Pool");
    }

    internal sealed class SerObjectInfoCache
    {
        internal String fullTypeName = null;
        internal String assemblyString = null;
        internal bool hasTypeForwardedFrom = false;
        internal MemberInfo[] memberInfos = null;
        internal String[] memberNames = null;
        internal Type[] memberTypes = null;

        internal SerObjectInfoCache(string typeName, string assemblyName, bool hasTypeForwardedFrom)
        {
            this.fullTypeName = typeName;
            this.assemblyString = assemblyName;
            this.hasTypeForwardedFrom = hasTypeForwardedFrom;
        }

        internal SerObjectInfoCache(Type type)
        {
            TypeInformation typeInformation = BinaryFormatter.GetTypeInformation(type);
            this.fullTypeName = typeInformation.FullTypeName;
            this.assemblyString = typeInformation.AssemblyString;
            this.hasTypeForwardedFrom = typeInformation.HasTypeForwardedFrom;
        }
    }

    internal sealed class TypeInformation
    {
        private String fullTypeName = null;
        private String assemblyString = null;
        private bool hasTypeForwardedFrom = false;

        internal String FullTypeName
        {
            get
            {
                return fullTypeName;
            }
        }

        internal String AssemblyString
        {
            get
            {
                return assemblyString;
            }
        }

        internal bool HasTypeForwardedFrom
        {
            get
            {
                return hasTypeForwardedFrom;
            }
        }

        internal TypeInformation(string fullTypeName, string assemblyString, bool hasTypeForwardedFrom)
        {
            this.fullTypeName = fullTypeName;
            this.assemblyString = assemblyString;
            this.hasTypeForwardedFrom = hasTypeForwardedFrom;
        }

    }
}
