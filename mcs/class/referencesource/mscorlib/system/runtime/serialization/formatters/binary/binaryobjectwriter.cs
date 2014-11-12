// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
 **
 ** Class: ObjectWriter
 **
 **
 ** Purpose: Serializes an object graph into XML in SOAP format
 **
 **
 ===========================================================*/

namespace System.Runtime.Serialization.Formatters.Binary
{    
    using System;
    using System.IO;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Security;
    using System.Diagnostics;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    internal sealed  class ObjectWriter
    {
        private Queue m_objectQueue;
        private ObjectIDGenerator m_idGenerator;
        private int m_currentId;

        private ISurrogateSelector m_surrogates;
        private StreamingContext m_context;
        private __BinaryWriter serWriter;
        private SerializationObjectManager m_objectManager;

        private long topId;
        private String topName = null;
        private Header[] headers;

        private InternalFE formatterEnums;
        private SerializationBinder m_binder;

        private SerObjectInfoInit serObjectInfoInit = null;

        private IFormatterConverter m_formatterConverter;

        internal Object[] crossAppDomainArray = null;
        internal ArrayList internalCrossAppDomainArray = null;


        // XMLObjectWriter Constructor
        internal ObjectWriter(ISurrogateSelector selector, StreamingContext context, InternalFE formatterEnums, SerializationBinder binder)
        {
            m_currentId = 1;
            m_surrogates = selector;
            m_context = context;
            m_binder = binder;
            this.formatterEnums = formatterEnums;
            m_objectManager = new SerializationObjectManager(context);
            SerTrace.InfoLog(
                            formatterEnums.FEtypeFormat +" "+
                            ((Enum)formatterEnums.FEserializerTypeEnum));


        }

        // Commences the process of serializing the entire graph.
        // initialize the graph walker.
        [System.Security.SecurityCritical]  // auto-generated
        internal void Serialize(Object graph, Header[] inHeaders, __BinaryWriter serWriter, bool fCheck)
        {
            if (graph == null)
                throw new ArgumentNullException("graph", Environment.GetResourceString("ArgumentNull_Graph"));

            if (serWriter == null)
                throw new ArgumentNullException("serWriter", Environment.GetResourceString("ArgumentNull_WithParamName", "serWriter"));
            Contract.EndContractBlock();

            SerTrace.Log(this, "Serialize Entry 2 ", graph, ((headers == null) ? " no headers " : "headers "));

            if (fCheck)
            {
                CodeAccessPermission.Demand(PermissionType.SecuritySerialization);          
            }

            this.serWriter = serWriter;
            this.headers = inHeaders;

            SerTrace.Log( this, "Serialize New SerializedTypeTable");
            serWriter.WriteBegin();
            long headerId = 0;
            Object obj;
            long objectId;
            bool isNew;
            bool bMethodCall = false;
            bool bMethodReturn = false;

#if FEATURE_REMOTING        
            // Special case IMethodCallMessage and IMethodReturnMessage for performance
            IMethodCallMessage mess = graph as IMethodCallMessage;
            if (mess != null)
            {
                bMethodCall = true;
                graph = WriteMethodCall(mess);
            }
            else
            {
                IMethodReturnMessage mr = graph as IMethodReturnMessage;
                if (mr != null)
                {
                    bMethodReturn = true;
                    graph = WriteMethodReturn(mr);
                }
            }
#endif // FEATURE_REMOTING        

            if (graph == null)
            {
                WriteSerializedStreamHeader(topId, headerId);

                if (bMethodCall)
                    serWriter.WriteMethodCall();
                else if (bMethodReturn)
                    serWriter.WriteMethodReturn();

                serWriter.WriteSerializationHeaderEnd();
                serWriter.WriteEnd();
                return;
            }

            // allocations if methodCall or methodResponse and no graph
            m_idGenerator = new ObjectIDGenerator();
            m_objectQueue = new Queue();
            m_formatterConverter = new FormatterConverter();
            serObjectInfoInit = new SerObjectInfoInit();        

            topId = InternalGetId(graph, false, null, out isNew);


            if (headers != null)
                headerId = InternalGetId(headers, false, null, out isNew);
            else
                headerId = -1;

            WriteSerializedStreamHeader(topId, headerId);


            if (bMethodCall)
                serWriter.WriteMethodCall();
            else if (bMethodReturn)
                serWriter.WriteMethodReturn();


            SerTrace.Log( this, "Serialize Schedule 0");
            // Write out SerializedStream header
            if ((headers != null) && (headers.Length > 0))
                m_objectQueue.Enqueue(headers);                 

            if (graph != null)
                m_objectQueue.Enqueue(graph);
            while ((obj = GetNext(out objectId))!=null)
            {
                SerTrace.Log( this, "Serialize GetNext ",obj);
                WriteObjectInfo objectInfo = null;

                // GetNext will return either an object or a WriteObjectInfo. 
                // A WriteObjectInfo is returned if this object was member of another object
                if (obj is WriteObjectInfo)
                {
                    SerTrace.Log( this, "Serialize GetNext recognizes WriteObjectInfo");
                    objectInfo = (WriteObjectInfo)obj;
                }
                else
                {
                    objectInfo = WriteObjectInfo.Serialize(obj, m_surrogates, m_context, serObjectInfoInit, m_formatterConverter, this, m_binder);
                    objectInfo.assemId = GetAssemblyId(objectInfo);
                }


                objectInfo.objectId = objectId;
                NameInfo typeNameInfo = TypeToNameInfo(objectInfo);
                Write(objectInfo, typeNameInfo, typeNameInfo);
                PutNameInfo(typeNameInfo);
                objectInfo.ObjectEnd();
            }

            serWriter.WriteSerializationHeaderEnd();
            serWriter.WriteEnd();

            // Invoke OnSerialized Event
            m_objectManager.RaiseOnSerializedEvent();
            
            SerTrace.Log( this, "Serialize Exit ");
        }

#if FEATURE_REMOTING
        [System.Security.SecurityCritical]  // auto-generated
        private Object[] WriteMethodCall(IMethodCallMessage mcm)
        {
            // In header
            String uri = mcm.Uri;
            String methodName = mcm.MethodName;
            String typeName = mcm.TypeName;

            // Optional
            Object methodSignature = null;
            Object callContext = null;
            Object[] properties = null;

            // instantiation args
            Type[] instArgs = null;
            if (mcm.MethodBase.IsGenericMethod) 
                instArgs = mcm.MethodBase.GetGenericArguments();

            // args
            Object[] args = mcm.Args;

            IInternalMessage iim = mcm as IInternalMessage;

            // user properties (everything but special entries)
            if ((iim == null) || iim.HasProperties())
                properties = StoreUserPropertiesForMethodMessage(mcm);

            // handle method signature
            if (mcm.MethodSignature != null && RemotingServices.IsMethodOverloaded(mcm))
                methodSignature = mcm.MethodSignature;

            // handle call context
            LogicalCallContext lcc = mcm.LogicalCallContext;
            if (lcc == null)
            {
                callContext = null;
            }
            else if (lcc.HasInfo)
                callContext = lcc;
            else
            {
                // just smuggle the call id string
                callContext = lcc.RemotingData.LogicalCallID;
            }

            return serWriter.WriteCallArray(uri, methodName, typeName, instArgs, args, methodSignature, callContext, properties);
        }


        [System.Security.SecurityCritical]  // auto-generated
        private Object[] WriteMethodReturn(IMethodReturnMessage mrm)
        {
            Object    returnValue = mrm.ReturnValue;
            Object[]  args = mrm.Args;
            Exception exception = mrm.Exception;
            Object callContext;
            Object[] properties = null;

            ReturnMessage retMsg = mrm as ReturnMessage;

            // user properties (everything but special entries)
            if ((retMsg == null) || retMsg.HasProperties())
                properties = StoreUserPropertiesForMethodMessage(mrm);

            // handle call context
            LogicalCallContext lcc = mrm.LogicalCallContext;
            if (lcc == null)
            {
                callContext = null;
            }
            else if (lcc.HasInfo)
                callContext = lcc;
            else
            {
                // just smuggle the call id string
                callContext = lcc.RemotingData.LogicalCallID;
            }

           return serWriter.WriteReturnArray(returnValue, args, exception, callContext, properties);
        }

        // returns number of entries added to argsToSerialize
        [System.Security.SecurityCritical]  // auto-generated
        private static Object[] StoreUserPropertiesForMethodMessage(IMethodMessage msg)
        {
            ArrayList argsToSerialize = null;
            IDictionary properties = msg.Properties;

            if (properties == null)
                return null;

            MessageDictionary dict = properties as MessageDictionary;
            if (dict != null)
            {
                if (dict.HasUserData())
                {
                    int co = 0;
                    foreach (DictionaryEntry entry in dict.InternalDictionary)
                    {
                        if (argsToSerialize == null)
                            argsToSerialize = new ArrayList();
                        argsToSerialize.Add(entry);
                        co++;
                    }

                    return argsToSerialize.ToArray();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // <


                int co = 0;
                foreach (DictionaryEntry entry in properties)
                {
                    if (argsToSerialize == null)
                        argsToSerialize = new ArrayList();
                    argsToSerialize.Add(entry);
                    co++;
                }

                if (argsToSerialize != null)
                    return argsToSerialize.ToArray();
                else
                    return null;
            }

        } // StoreUserPropertiesForMethodMessage
#endif // FEATURE_REMOTING
        internal SerializationObjectManager ObjectManager
        {
            get { return m_objectManager; }
        }

        // Writes a given object to the stream.
        [System.Security.SecurityCritical]  // auto-generated
        private void Write(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo)
        {       
#if _DEBUG                        
            SerTrace.Log( this, "Write 1 Entry objectInfo ",objectInfo,", memberNameInfo ",memberNameInfo,", typeNameInfo ",typeNameInfo);
            memberNameInfo.Dump("Write memberNameInfo");
            typeNameInfo.Dump("Write typeNameInfo");
#endif            
            Object obj = objectInfo.obj;
            if (obj==null)
                throw new ArgumentNullException("objectInfo.obj", Environment.GetResourceString("ArgumentNull_Obj"));

            SerTrace.Log( this, "Write 1 objectInfo obj ",objectInfo.obj," objectId ", objectInfo.objectId, " objectType ", objectInfo.objectType);
            Type objType = objectInfo.objectType;
            long objectId = objectInfo.objectId;


            SerTrace.Log( this, "Write 1 ",obj," ObjectId ",objectId);

            if (Object.ReferenceEquals(objType, Converter.typeofString))
            {
                // Top level String
                memberNameInfo.NIobjectId = objectId;
                serWriter.WriteObjectString((int)objectId, obj.ToString());
            }
            else
            {

                if (objectInfo.isArray)
                {
                    WriteArray(objectInfo, memberNameInfo, null); 
                }
                else
                {
                    String[] memberNames;
                    Type[] memberTypes;
                    Object[] memberData;

                    objectInfo.GetMemberInfo(out memberNames, out memberTypes, out memberData);

                    // Only Binary needs to transmit types for ISerializable because the binary formatter transmits the types in URT format.
                    // Soap transmits all types as strings, so it is up to the ISerializable object to convert the string back to its URT type
                    if (objectInfo.isSi || CheckTypeFormat(formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways))
                    {
                        SerTrace.Log( this, "Write 1 TransmitOnObject ");
                        memberNameInfo.NItransmitTypeOnObject = true;
                        memberNameInfo.NIisParentTypeOnObject = true;
                        typeNameInfo.NItransmitTypeOnObject = true;
                        typeNameInfo.NIisParentTypeOnObject = true;                                             
                    }

                    WriteObjectInfo[] memberObjectInfos = new WriteObjectInfo[memberNames.Length];

                    // Get assembly information
                    // Binary Serializer, assembly names need to be
                    // written before objects are referenced.
                    // GetAssemId here will write out the
                    // assemblyStrings at the right Binary
                    // Serialization object boundary.
                    for (int i=0; i<memberTypes.Length; i++)
                    {
                        Type type;
                        if ((object)memberTypes[i] != null)
                            type = memberTypes[i];
                        else if (memberData[i] != null)
                            type = GetType(memberData[i]);
                        else
                            type = Converter.typeofObject;

                        SerTrace.Log( this, "Write 1 member type ",type);
                        InternalPrimitiveTypeE code = ToCode(type);
                        if ((code == InternalPrimitiveTypeE.Invalid) &&
                            (!Object.ReferenceEquals(type, Converter.typeofString)))
                        {
                            SerTrace.Log( this, "Write 1 Create ObjectInfo ", memberTypes[i], " memberData ",memberData[i]);
                            if (memberData[i] != null)
                            {
                                memberObjectInfos[i] =
                                WriteObjectInfo.Serialize
                                (
                                memberData[i],
                                m_surrogates,
                                m_context,
                                serObjectInfoInit,
                                m_formatterConverter,
                                this,
                                m_binder);                                    
                                memberObjectInfos[i].assemId = GetAssemblyId(memberObjectInfos[i]);
                            }
                            else
                            {
                                memberObjectInfos[i] =
                                WriteObjectInfo.Serialize
                                (
                                memberTypes[i],
                                m_surrogates,
                                m_context,
                                serObjectInfoInit,
                                m_formatterConverter,
                                m_binder
                                );
                                memberObjectInfos[i].assemId = GetAssemblyId(memberObjectInfos[i]);
                            }
                        }
                    }           
                    Write(objectInfo, memberNameInfo, typeNameInfo, memberNames, memberTypes, memberData, memberObjectInfos);
                    SerTrace.Log( this, "Write 1 ",obj," type ",GetType(obj));     
                }
            }
            SerTrace.Log( this, "Write 1 Exit ",obj);       
        }

        // Writes a given object to the stream.
        [System.Security.SecurityCritical]  // auto-generated
        private void Write(WriteObjectInfo objectInfo,   
                           NameInfo memberNameInfo,          
                           NameInfo typeNameInfo,            
                           String[] memberNames,             
                           Type[] memberTypes,               
                           Object[] memberData,              
                           WriteObjectInfo[] memberObjectInfos)
        {
            SerTrace.Log( this, "Write 2 Entry obj ",objectInfo.obj,". objectId ",objectInfo.objectId,", objType ",typeNameInfo.NIname,", memberName ",memberNameInfo.NIname,", memberType ",typeNameInfo.NIname);

            int numItems = memberNames.Length;
            NameInfo topNameInfo = null;

            if (memberNameInfo != null)
            {
                SerTrace.Log( this, "Write 2 ObjectBegin, memberName ",memberNameInfo.NIname);
                memberNameInfo.NIobjectId = objectInfo.objectId;
                serWriter.WriteObject(memberNameInfo, typeNameInfo, numItems, memberNames, memberTypes, memberObjectInfos);
            }
            else if ((objectInfo.objectId == topId) && (topName != null))
            {
                SerTrace.Log( this, "Write 2 ObjectBegin, topId method name ",topName);
                topNameInfo = MemberToNameInfo(topName);
                topNameInfo.NIobjectId = objectInfo.objectId;
                serWriter.WriteObject(topNameInfo, typeNameInfo, numItems, memberNames, memberTypes, memberObjectInfos);
            }
            else
            {
                if (!Object.ReferenceEquals(objectInfo.objectType, Converter.typeofString))
                {
                    SerTrace.Log( this, "Write 2 ObjectBegin, default ", typeNameInfo.NIname);
                    typeNameInfo.NIobjectId = objectInfo.objectId;
                    serWriter.WriteObject(typeNameInfo, null, numItems, memberNames, memberTypes, memberObjectInfos);
                }
            }

            if (memberNameInfo.NIisParentTypeOnObject)
            {
                memberNameInfo.NItransmitTypeOnObject = true;
                memberNameInfo.NIisParentTypeOnObject = false;
            }
            else
                memberNameInfo.NItransmitTypeOnObject = false;


            // Write members
            for (int i=0; i<numItems; i++)
            {
                WriteMemberSetup(objectInfo, memberNameInfo, typeNameInfo, memberNames[i], memberTypes[i], memberData[i], memberObjectInfos[i]);
            }

            if (memberNameInfo != null)
            {
                memberNameInfo.NIobjectId = objectInfo.objectId;
                serWriter.WriteObjectEnd(memberNameInfo, typeNameInfo);
            }
            else if ((objectInfo.objectId == topId) && (topName != null))
            {
                serWriter.WriteObjectEnd(topNameInfo, typeNameInfo);
                PutNameInfo(topNameInfo);
            }
            else
            {
                if (!Object.ReferenceEquals(objectInfo.objectType, Converter.typeofString))
                {
                    serWriter.WriteObjectEnd(typeNameInfo, typeNameInfo);                       
                }
            }

            SerTrace.Log( this, "Write 2 Exit");
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void WriteMemberSetup(WriteObjectInfo objectInfo,        
                                      NameInfo memberNameInfo,           
                                      NameInfo typeNameInfo,             
                                      String memberName,             
                                      Type memberType,               
                                      Object memberData,                 
                                      WriteObjectInfo memberObjectInfo
                                     )
        {
            NameInfo newMemberNameInfo = MemberToNameInfo(memberName);
            // newMemberNameInfo contains the member type

            if (memberObjectInfo != null)
                newMemberNameInfo.NIassemId = memberObjectInfo.assemId;
            newMemberNameInfo.NItype = memberType;

            // newTypeNameInfo contains the data type
            NameInfo newTypeNameInfo = null;
            if (memberObjectInfo == null)
            {
                newTypeNameInfo = TypeToNameInfo(memberType);
            }
            else
            {
                newTypeNameInfo = TypeToNameInfo(memberObjectInfo);
            }

            newMemberNameInfo.NItransmitTypeOnObject = memberNameInfo.NItransmitTypeOnObject;
            newMemberNameInfo.NIisParentTypeOnObject = memberNameInfo.NIisParentTypeOnObject;               
            WriteMembers(newMemberNameInfo, newTypeNameInfo, memberData, objectInfo, typeNameInfo, memberObjectInfo);
            PutNameInfo(newMemberNameInfo);
            PutNameInfo(newTypeNameInfo);
        }


        // Writes the members of an object
        [System.Security.SecurityCritical]  // auto-generated
        private void WriteMembers(NameInfo memberNameInfo,
                                  NameInfo memberTypeNameInfo,
                                  Object   memberData,
                                  WriteObjectInfo objectInfo,
                                  NameInfo typeNameInfo,
                                  WriteObjectInfo memberObjectInfo
                                 )
        {
            SerTrace.Log( this, "WriteMembers Entry memberType: ",memberTypeNameInfo.NIname," memberName: ",memberNameInfo.NIname," data: ",memberData," objectId: ",objectInfo.objectId, " Container object ",objectInfo.obj, " memberObjectinfo ",memberObjectInfo);
            Type memberType = memberNameInfo.NItype;
            bool assignUniqueIdToValueType = false;

            // Types are transmitted for a member as follows:
            // The member is of type object
            // The member object of type is ISerializable and
            //  Binary - Types always transmitted.

            if (Object.ReferenceEquals(memberType, Converter.typeofObject) || (object)Nullable.GetUnderlyingType(memberType) != null)
            {
                memberTypeNameInfo.NItransmitTypeOnMember  = true;
                memberNameInfo.NItransmitTypeOnMember  = true;              
            }

            if (CheckTypeFormat(formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways) || (objectInfo.isSi) )
            {
                memberTypeNameInfo.NItransmitTypeOnObject  = true;
                memberNameInfo.NItransmitTypeOnObject  = true;
                memberNameInfo.NIisParentTypeOnObject = true;
            }

            if (CheckForNull(objectInfo, memberNameInfo, memberTypeNameInfo, memberData))
            {
                return;
            }

            Object outObj = memberData;
            Type outType = null;

            // If member type does not equal data type, transmit type on object.
            if (memberTypeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Invalid)
            {
                outType = GetType(outObj);
                if (!Object.ReferenceEquals(memberType, outType))
                {
                    memberTypeNameInfo.NItransmitTypeOnMember  = true;
                    memberNameInfo.NItransmitTypeOnMember  = true;    
                }
            }

            if (Object.ReferenceEquals(memberType, Converter.typeofObject))
            {
                assignUniqueIdToValueType = true;
                memberType = GetType(memberData);
                if (memberObjectInfo == null)
                    TypeToNameInfo(memberType, memberTypeNameInfo);
                else
                    TypeToNameInfo(memberObjectInfo, memberTypeNameInfo);                   
                SerTrace.Log( this, "WriteMembers memberType Object, actual memberType ",memberType);                                                                               
            }

            if (memberObjectInfo != null && memberObjectInfo.isArray)
            {
                // Array
                SerTrace.Log( this, "WriteMembers IsArray");

                long arrayId = 0;
                if ((object)outType == null)
                    outType = GetType(outObj);
                // outObj is an array. It can never be a value type..
                arrayId = Schedule(outObj, false, null, memberObjectInfo);
                if (arrayId > 0)
                {
                    // Array as object
                    SerTrace.Log( this, "WriteMembers Schedule 3");
                    memberNameInfo.NIobjectId = arrayId;
                    WriteObjectRef(memberNameInfo, arrayId); 
                }
                else
                {
                    // Nested Array
                    serWriter.WriteMemberNested(memberNameInfo);

                    memberObjectInfo.objectId = arrayId;
                    memberNameInfo.NIobjectId = arrayId;
                    WriteArray(memberObjectInfo, memberNameInfo, memberObjectInfo);
                    objectInfo.ObjectEnd();

                }
                SerTrace.Log( this, "WriteMembers Array Exit ");
                return;
            }

            if (!WriteKnownValueClass(memberNameInfo, memberTypeNameInfo, memberData))
            {
                SerTrace.Log( this, "WriteMembers Object ",memberData);

#if false
                // Value or NO_ID, need to explicitly check for IsValue because a top level
                // value class has an objectId of 1
                /*
                if (memberType.IsValueType)
                {
                    SerTrace.Log( this, "WriteMembers Value Type or NO_ID parameter");

                    bool isNew;
                    memberObjectInfo.objectId = InternalGetId(outObj, assignUniqueIdToValueType, memberType, out isNew) ;
                    NameInfo newTypeNameInfo = TypeToNameInfo(memberObjectInfo);
                    newTypeNameInfo.NIobjectId = memberObjectInfo.objectId;
                    Write( memberObjectInfo, memberNameInfo, newTypeNameInfo);
                    PutNameInfo(newTypeNameInfo);
                    memberObjectInfo.ObjectEnd();
                }
                else
                */
#endif
                {
                    SerTrace.Log( this, "WriteMembers Schedule 4 ", outType, " memberInfo ",memberObjectInfo);
                    if ((object)outType == null)
                        outType = GetType(outObj);
                    long memberObjectId = Schedule(outObj, assignUniqueIdToValueType, outType, memberObjectInfo);
                    if (memberObjectId < 0)
                    {
                        // Nested object
                        SerTrace.Log( this, "WriteMembers Nesting");

                        memberObjectInfo.objectId = memberObjectId;
                        NameInfo newTypeNameInfo = TypeToNameInfo(memberObjectInfo);
                        newTypeNameInfo.NIobjectId = memberObjectId;
                        Write(memberObjectInfo, memberNameInfo, newTypeNameInfo);
                        PutNameInfo(newTypeNameInfo);
                        memberObjectInfo.ObjectEnd();
                    }
                    else
                    {
                        // Object reference
                        memberNameInfo.NIobjectId = memberObjectId;
                        WriteObjectRef(memberNameInfo, memberObjectId); 
                    }
                }
            }

            SerTrace.Log( this, "WriteMembers Exit ");
        }

        // Writes out an array
        [System.Security.SecurityCritical]  // auto-generated
        private void WriteArray(WriteObjectInfo objectInfo, NameInfo memberNameInfo, WriteObjectInfo memberObjectInfo)          
        {
            SerTrace.Log( this, "WriteArray Entry ",objectInfo.obj," ",objectInfo.objectId);

            bool isAllocatedMemberNameInfo = false;
            if (memberNameInfo == null)
            {
                memberNameInfo = TypeToNameInfo(objectInfo);
                isAllocatedMemberNameInfo = true;
            }

            memberNameInfo.NIisArray = true;

            long objectId = objectInfo.objectId;
            memberNameInfo.NIobjectId = objectInfo.objectId;

            // Get array type
            System.Array array = (System.Array)objectInfo.obj;
            //Type arrayType = array.GetType();
            Type arrayType = objectInfo.objectType;         

            // Get type of array element 
            Type arrayElemType = arrayType.GetElementType();
            WriteObjectInfo arrayElemObjectInfo = null;
            if (!arrayElemType.IsPrimitive)
            {
                arrayElemObjectInfo = WriteObjectInfo.Serialize(arrayElemType, m_surrogates, m_context, serObjectInfoInit, m_formatterConverter, m_binder);
                arrayElemObjectInfo.assemId = GetAssemblyId(arrayElemObjectInfo);
            }


            NameInfo arrayElemTypeNameInfo = null;
            if (arrayElemObjectInfo == null)
                arrayElemTypeNameInfo = TypeToNameInfo(arrayElemType);
            else
                arrayElemTypeNameInfo = TypeToNameInfo(arrayElemObjectInfo);
            arrayElemTypeNameInfo.NIisArray = arrayElemTypeNameInfo.NItype.IsArray;

            NameInfo arrayNameInfo = memberNameInfo;
            arrayNameInfo.NIobjectId = objectId;
            arrayNameInfo.NIisArray = true;
            arrayElemTypeNameInfo.NIobjectId = objectId;
            arrayElemTypeNameInfo.NItransmitTypeOnMember = memberNameInfo.NItransmitTypeOnMember;
            arrayElemTypeNameInfo.NItransmitTypeOnObject = memberNameInfo.NItransmitTypeOnObject;
            arrayElemTypeNameInfo.NIisParentTypeOnObject = memberNameInfo.NIisParentTypeOnObject;

            // Get rank and length information
            int rank = array.Rank;
            int[] lengthA = new int[rank];
            int[] lowerBoundA = new int[rank];
            int[] upperBoundA = new int[rank];                  
            for (int i=0; i<rank; i++)
            {
                lengthA[i] = array.GetLength(i);
                lowerBoundA[i] = array.GetLowerBound(i);
                upperBoundA[i] = array.GetUpperBound(i);                            
            }

            InternalArrayTypeE arrayEnum;

            if (arrayElemTypeNameInfo.NIisArray)
            {
                if (rank == 1)
                    arrayEnum = InternalArrayTypeE.Jagged;
                else
                    arrayEnum = InternalArrayTypeE.Rectangular;
            }
            else if (rank == 1)
                arrayEnum = InternalArrayTypeE.Single;
            else
                arrayEnum = InternalArrayTypeE.Rectangular;

            arrayElemTypeNameInfo.NIarrayEnum = arrayEnum;

            SerTrace.Log( this, "WriteArray ArrayInfo type ",arrayType," rank ",rank);


            // Byte array
            if ((Object.ReferenceEquals(arrayElemType, Converter.typeofByte)) && (rank == 1) && (lowerBoundA[0] == 0))
            {
                serWriter.WriteObjectByteArray(memberNameInfo, arrayNameInfo, arrayElemObjectInfo, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0], (byte[])array);
                return;
            }

            if (Object.ReferenceEquals(arrayElemType, Converter.typeofObject) || (object)Nullable.GetUnderlyingType(arrayElemType) != null)
            {
                memberNameInfo.NItransmitTypeOnMember = true;
                arrayElemTypeNameInfo.NItransmitTypeOnMember = true;
            }

            if (CheckTypeFormat(formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways))
            {
                memberNameInfo.NItransmitTypeOnObject = true;
                arrayElemTypeNameInfo.NItransmitTypeOnObject = true;                
            }

            if (arrayEnum == InternalArrayTypeE.Single)
            {
                // Single Dimensional array
                SerTrace.Log( this, "WriteArray ARRAY_SINGLE ");

                // BinaryFormatter array of primitive types is written out in the WriteSingleArray statement
                // as a byte buffer
                serWriter.WriteSingleArray(memberNameInfo, arrayNameInfo, arrayElemObjectInfo, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0], array);

                if (!(Converter.IsWriteAsByteArray(arrayElemTypeNameInfo.NIprimitiveTypeEnum) && (lowerBoundA[0] == 0)))
                {
                    Object[] objectA = null;
                    if (!arrayElemType.IsValueType)
                    {
                        // Non-primitive type array                 
                        objectA = (Object[])array;
                    }

                    int upperBound = upperBoundA[0]+1;
                    for (int i = lowerBoundA[0]; i < upperBound; i++)
                    {
                        if (objectA == null)
                            WriteArrayMember(objectInfo, arrayElemTypeNameInfo, array.GetValue(i));
                        else
                            WriteArrayMember(objectInfo, arrayElemTypeNameInfo, objectA[i]);
                    }
                    serWriter.WriteItemEnd();
                }
            }
            else if (arrayEnum == InternalArrayTypeE.Jagged)
            {
                // Jagged Array
                SerTrace.Log( this, "WriteArray ARRAY_JAGGED");

                arrayNameInfo.NIobjectId = objectId;

                serWriter.WriteJaggedArray(memberNameInfo, arrayNameInfo, arrayElemObjectInfo, arrayElemTypeNameInfo, lengthA[0], lowerBoundA[0]);

                Object[] objectA = (Object[])array;
                for (int i = lowerBoundA[0]; i < upperBoundA[0]+1; i++)
                {
                    WriteArrayMember(objectInfo, arrayElemTypeNameInfo, objectA[i]);
                }
                serWriter.WriteItemEnd();
            }
            else
            {
                // Rectangle Array
                // Get the length for all the ranks
                SerTrace.Log( this, "WriteArray ARRAY_RECTANGLE");                      

                arrayNameInfo.NIobjectId = objectId;
                serWriter.WriteRectangleArray(memberNameInfo, arrayNameInfo, arrayElemObjectInfo, arrayElemTypeNameInfo, rank, lengthA, lowerBoundA);

                IndexTraceMessage("WriteArray Rectangle  ", lengthA);

                // Check for a length of zero
                bool bzero = false;
                for (int i=0; i<rank; i++)
                {
                    if (lengthA[i] == 0)
                    {
                        bzero = true;
                        break;
                    }
                }

                if (!bzero)
                    WriteRectangle(objectInfo, rank, lengthA, array, arrayElemTypeNameInfo, lowerBoundA);
                serWriter.WriteItemEnd();
            }

            serWriter.WriteObjectEnd(memberNameInfo, arrayNameInfo); 

            PutNameInfo(arrayElemTypeNameInfo);
            if (isAllocatedMemberNameInfo)
                PutNameInfo(memberNameInfo);

            SerTrace.Log( this, "WriteArray Exit ");
        }

        // Writes out an array element
        [System.Security.SecurityCritical]  // auto-generated
        private void WriteArrayMember(WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, Object data)
        {
            SerTrace.Log( this, "WriteArrayMember ",data," baseArrayName ",arrayElemTypeNameInfo.NIname);

            arrayElemTypeNameInfo.NIisArrayItem = true;

            if (CheckForNull(objectInfo, arrayElemTypeNameInfo, arrayElemTypeNameInfo, data))
                return;

            NameInfo actualTypeInfo = null;

            Type dataType = null;

            bool isObjectOnMember = false;

            if (arrayElemTypeNameInfo.NItransmitTypeOnMember)
                isObjectOnMember = true;

            if (!isObjectOnMember && !arrayElemTypeNameInfo.IsSealed)
            {
                dataType = GetType(data);
                if (!Object.ReferenceEquals(arrayElemTypeNameInfo.NItype, dataType))
                    isObjectOnMember = true;
            }

            if (isObjectOnMember)
            {
                // Object array, need type of member
                if ((object)dataType == null)
                    dataType = GetType(data);
                actualTypeInfo = TypeToNameInfo(dataType);
                actualTypeInfo.NItransmitTypeOnMember = true;
                actualTypeInfo.NIobjectId = arrayElemTypeNameInfo.NIobjectId;
                actualTypeInfo.NIassemId = arrayElemTypeNameInfo.NIassemId;
                actualTypeInfo.NIisArrayItem = true;
            }
            else
            {
                actualTypeInfo = arrayElemTypeNameInfo;
                actualTypeInfo.NIisArrayItem = true;
            }

            if (!WriteKnownValueClass(arrayElemTypeNameInfo, actualTypeInfo, data))
            {
                Object obj = data;
                bool assignUniqueIdForValueTypes = false;
                if (Object.ReferenceEquals(arrayElemTypeNameInfo.NItype, Converter.typeofObject))
                    assignUniqueIdForValueTypes = true;

                long arrayId = Schedule(obj, assignUniqueIdForValueTypes, actualTypeInfo.NItype);
                arrayElemTypeNameInfo.NIobjectId = arrayId;
                actualTypeInfo.NIobjectId = arrayId;
                if (arrayId < 1)
                {
                        WriteObjectInfo newObjectInfo = WriteObjectInfo.Serialize(obj, m_surrogates, m_context, serObjectInfoInit, m_formatterConverter, this, m_binder);
                        newObjectInfo.objectId = arrayId;
                        if (!Object.ReferenceEquals(arrayElemTypeNameInfo.NItype, Converter.typeofObject) && (object)Nullable.GetUnderlyingType(arrayElemTypeNameInfo.NItype) == null)
                            newObjectInfo.assemId = actualTypeInfo.NIassemId; 
                        else
                            newObjectInfo.assemId = GetAssemblyId(newObjectInfo);
                        SerTrace.Log( this, "WriteArrayMembers nested");
                        NameInfo typeNameInfo = TypeToNameInfo(newObjectInfo);
                        typeNameInfo.NIobjectId = arrayId;
                        newObjectInfo.objectId = arrayId;
                        Write(newObjectInfo, actualTypeInfo, typeNameInfo);

                        newObjectInfo.ObjectEnd();
                }
                else
                {
                    serWriter.WriteItemObjectRef(arrayElemTypeNameInfo, (int)arrayId);
                }

            }
            if (arrayElemTypeNameInfo.NItransmitTypeOnMember)
                PutNameInfo(actualTypeInfo);
        }


        // Iterates over a Rectangle array, for each element of the array invokes WriteArrayMember

        [System.Security.SecurityCritical]  // auto-generated
        private void WriteRectangle(WriteObjectInfo objectInfo, int rank, int[] maxA, System.Array array, NameInfo arrayElemNameTypeInfo, int[] lowerBoundA)
        {
            IndexTraceMessage("WriteRectangle  Entry "+rank, maxA);
            int[] currentA = new int[rank];
            int[] indexMap = null;
            bool isLowerBound = false;
            if (lowerBoundA != null)
            {
                for (int i=0; i<rank; i++)
                {
                    if (lowerBoundA[i] != 0)
                        isLowerBound = true;
                }
            }
            if (isLowerBound)
                indexMap = new int[rank];

            bool isLoop = true;
            while (isLoop)
            {
                isLoop = false;
                if (isLowerBound)
                { 
                    for (int i=0; i<rank; i++)
                    {
                        indexMap[i] = currentA[i]+lowerBoundA[i];
                    }

                    WriteArrayMember(objectInfo, arrayElemNameTypeInfo, array.GetValue(indexMap));
                }
                else
                    WriteArrayMember(objectInfo, arrayElemNameTypeInfo, array.GetValue(currentA));          
                for (int irank = rank-1; irank>-1; irank--)
                {
                    // Find the current or lower dimension which can be incremented.
                    if (currentA[irank] < maxA[irank]-1)
                    {
                        // The current dimension is at maximum. Increase the next lower dimension by 1
                        currentA[irank]++;
                        if (irank < rank-1)
                        {
                            // The current dimension and higher dimensions are zeroed.
                            for (int i = irank+1; i<rank; i++)
                                currentA[i] = 0;
                        }
                        isLoop = true;
                        break;                  
                    }

                }
            }
            SerTrace.Log( this, "WriteRectangle  Exit ");
        }

        // Traces a message with an array of int
        [Conditional("SER_LOGGING")]                            
        private void IndexTraceMessage(String message, int[] index)
        {
            StringBuilder sb = StringBuilderCache.Acquire(10);
            sb.Append("[");     
            for (int i=0; i<index.Length; i++)
            {
                sb.Append(index[i]);
                if (i != index.Length -1)
                    sb.Append(",");
            }
            sb.Append("]");             
            SerTrace.Log( this, message+" ", StringBuilderCache.GetStringAndRelease(sb));
        }


        // This gives back the next object to be serialized.  Objects
        // are returned in a FIFO order based on how they were passed
        // to Schedule.  The id of the object is put into the objID parameter
        // and the Object itself is returned from the function.
        private Object GetNext(out long objID)
        {
            SerTrace.Log( this, "GetNext Entry ");      
            bool isNew;

            //The Queue is empty here.  We'll throw if we try to dequeue the empty queue.
            if (m_objectQueue.Count==0)
            {
                objID=0;
                SerTrace.Log( this, "GetNext Exit null");
                return null;
            }

            Object obj = m_objectQueue.Dequeue();
            Object realObj = null;

            // A WriteObjectInfo is queued if this object was a member of another object
            SerTrace.Log( this, "GetNext ",obj);
            if (obj is WriteObjectInfo)
            {
                SerTrace.Log( this, "GetNext recognizes WriteObjectInfo");
                realObj = ((WriteObjectInfo)obj).obj;
            }
            else
                realObj = obj;
            objID = m_idGenerator.HasId(realObj, out isNew);
            if (isNew)
            {
                SerTrace.Log( this, "Object " , realObj , " has never been assigned an id.");
                throw new SerializationException(Environment.GetResourceString("Serialization_ObjNoID",realObj));                                                    
            }

            SerTrace.Log( this, "GetNext Exit "+objID," ",realObj);            
            return obj;
        }

        Object previousObj = null;
        long previousId = 0;
        // If the type is a value type, we dont attempt to generate a unique id, unless its a boxed entity
        // (in which case, there might be 2 references to the same boxed obj. in a graph.)
        // "assignUniqueIdToValueType" is true, if the field type holding reference to "obj" is Object.
        private long InternalGetId(Object obj, bool assignUniqueIdToValueType, Type type, out bool isNew)
        {
            if (obj == previousObj)
            {
                // good for benchmarks
                isNew = false;
                return previousId;
            }
            m_idGenerator.m_currentCount = m_currentId;
            if ((object)type != null && type.IsValueType)
            {
                if (!assignUniqueIdToValueType)
                {
                    isNew = false;
                    return -1 * m_currentId++;
                }
            }
            m_currentId++;
            long retId = m_idGenerator.GetId(obj, out isNew);

            previousObj = obj;
            previousId = retId;
            return retId;
        }


        // Schedules an object for later serialization if it hasn't already been scheduled.
        // We get an ID for obj and put it on the queue for later serialization
        // if this is a new object id.

        private long Schedule(Object obj, bool assignUniqueIdToValueType, Type type)
        {
            return Schedule(obj, assignUniqueIdToValueType, type, null);
        }

        private long Schedule(Object obj, bool assignUniqueIdToValueType, Type type, WriteObjectInfo objectInfo)
        {
            SerTrace.Log( this, "Schedule Entry obj ",obj," type ", type, " objectInfo ",objectInfo);

            bool isNew;
            long id;

            if (obj==null)
            {
                SerTrace.Log(this, "Schedule Obj Null, id = 0 ");
                return 0;
            }

            id = InternalGetId(obj, assignUniqueIdToValueType, type, out isNew);           

            if (isNew && id > 0)
            {
                if (objectInfo == null)
                    m_objectQueue.Enqueue(obj);
                else
                    m_objectQueue.Enqueue(objectInfo);

            }
            SerTrace.Log( this, "Schedule Exit, id: ",id," isNew: ",isNew);     
            return id;

        }


        // Determines if a type is a primitive type, if it is it is written

        private bool WriteKnownValueClass(NameInfo memberNameInfo, NameInfo typeNameInfo, Object data) 
        {
#if _DEBUG                        
            SerTrace.Log( this, "WriteKnownValueClass Entry ",typeNameInfo.NIname," ",data," ",memberNameInfo.NIname);
            memberNameInfo.Dump("WriteKnownValueClass memberNameInfo");         
            typeNameInfo.Dump("WriteKnownValueClass typeNameInfo");
#endif            

            if (Object.ReferenceEquals(typeNameInfo.NItype, Converter.typeofString))
            {
                WriteString(memberNameInfo, typeNameInfo, data);
            }
            else
            {
                if (typeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Invalid)
                {
                    SerTrace.Log( this, "WriteKnownValueClass Exit false");                     
                    return false;
                }
                else
                {
                    if (typeNameInfo.NIisArray) // null if an array
                        serWriter.WriteItem(memberNameInfo, typeNameInfo, data);
                    else
                    {
                        serWriter.WriteMember(memberNameInfo, typeNameInfo, data);
                    }
                }
            }

            SerTrace.Log( this, "WriteKnownValueClass Exit true");
            return true;
        }


        // Writes an object reference to the stream.
        private void WriteObjectRef(NameInfo nameInfo, long objectId)
        {
            SerTrace.Log( this, "WriteObjectRef Entry ",nameInfo.NIname," ",objectId);
            serWriter.WriteMemberObjectRef(nameInfo, (int)objectId);

            SerTrace.Log( this, "WriteObjectRef Exit ");
        }



        // Writes a string into the XML stream
        private void WriteString(NameInfo memberNameInfo, NameInfo typeNameInfo, Object stringObject)
        {
            SerTrace.Log( this, "WriteString stringObject ",stringObject," memberName ",memberNameInfo.NIname);
            bool isFirstTime = true;

            long stringId = -1;

            if (!CheckTypeFormat(formatterEnums.FEtypeFormat, FormatterTypeStyle.XsdString))
                stringId= InternalGetId(stringObject, false, null, out isFirstTime);

            typeNameInfo.NIobjectId = stringId;
            SerTrace.Log( this, "WriteString stringId ",stringId," isFirstTime ",isFirstTime);

            if ((isFirstTime) || (stringId < 0))
                serWriter.WriteMemberString(memberNameInfo, typeNameInfo, (String)stringObject);
            else
                WriteObjectRef(memberNameInfo, stringId);
        }

        // Writes a null member into the stream
        private bool CheckForNull(WriteObjectInfo objectInfo, NameInfo memberNameInfo, NameInfo typeNameInfo, Object data)
        {
#if _DEBUG            
            SerTrace.Log( this, "CheckForNull Entry data ",Util.PString(data),", memberType ",Util.PString(typeNameInfo.NItype));
#endif
            bool isNull = false;

            if (data == null) // || Convert.IsDBNull(data)
                isNull = true;

            // Optimization, Null members are only written for Binary
            if ((isNull) && (((formatterEnums.FEserializerTypeEnum == InternalSerializerTypeE.Binary)) ||
                             memberNameInfo.NIisArrayItem ||
                             memberNameInfo.NItransmitTypeOnObject ||
                             memberNameInfo.NItransmitTypeOnMember ||
                             objectInfo.isSi ||
                             (CheckTypeFormat(formatterEnums.FEtypeFormat, FormatterTypeStyle.TypesAlways))))
            {
                SerTrace.Log( this, "CheckForNull Write");

                if (typeNameInfo.NIisArrayItem)
                {
                    if (typeNameInfo.NIarrayEnum == InternalArrayTypeE.Single)
                        serWriter.WriteDelayedNullItem();
                    else
                        serWriter.WriteNullItem(memberNameInfo, typeNameInfo);
                }

                else
                    serWriter.WriteNullMember(memberNameInfo, typeNameInfo);
            }
            SerTrace.Log( this, "CheckForNull Exit ",isNull);
            return isNull;
        }


        // Writes the SerializedStreamHeader
        private void WriteSerializedStreamHeader(long topId, long headerId)
        {
            serWriter.WriteSerializationHeader((int)topId, (int)headerId, 1, 0);
        }


        // Transforms a type to the serialized string form. URT Primitive types are converted to XMLData Types
        private NameInfo TypeToNameInfo(Type type, WriteObjectInfo objectInfo, InternalPrimitiveTypeE code, NameInfo nameInfo)
        {
            SerTrace.Log( this, "TypeToNameInfo Entry type ",type,", objectInfo ",objectInfo,", code ", ((Enum)code).ToString());
            if (nameInfo == null)
                nameInfo = GetNameInfo();
            else
                nameInfo.Init();

            if (code == InternalPrimitiveTypeE.Invalid)
            {
                if (objectInfo != null)
                {
                    nameInfo.NIname = objectInfo.GetTypeFullName();
                    nameInfo.NIassemId = objectInfo.assemId;                    
                }
            }
            nameInfo.NIprimitiveTypeEnum = code; 
            nameInfo.NItype = type;

            SerTrace.Log( this, "TypeToNameInfo Exit ",type, " typeName "+nameInfo.NIname);
            return nameInfo;            
        }

        private NameInfo TypeToNameInfo(Type type)
        {
            return TypeToNameInfo(type, null, ToCode(type), null);
        }

        private NameInfo TypeToNameInfo(WriteObjectInfo objectInfo)
        {
            return TypeToNameInfo(objectInfo.objectType, objectInfo, ToCode(objectInfo.objectType), null);
        }

        private NameInfo TypeToNameInfo(WriteObjectInfo objectInfo, NameInfo nameInfo)
        {
            return TypeToNameInfo(objectInfo.objectType, objectInfo, ToCode(objectInfo.objectType), nameInfo);
        }

        private void TypeToNameInfo(Type type, NameInfo nameInfo)
        {
            TypeToNameInfo(type, null, ToCode(type), nameInfo);
        }

        private NameInfo MemberToNameInfo(String name)
        {
            NameInfo memberNameInfo = GetNameInfo();
            memberNameInfo.NIname = name;
            return memberNameInfo;
        }

        Type previousType = null;
        InternalPrimitiveTypeE previousCode = InternalPrimitiveTypeE.Invalid;
        internal InternalPrimitiveTypeE ToCode(Type type)
        {
            if (Object.ReferenceEquals(previousType, type))
            {
                return previousCode;
            }
            else
            {
                InternalPrimitiveTypeE code = Converter.ToCode(type);
                if (code != InternalPrimitiveTypeE.Invalid)
                {
                    previousType = type;
                    previousCode = code;
                }
                return code;
            }
        }

        private Hashtable assemblyToIdTable = null;
        private long GetAssemblyId(WriteObjectInfo objectInfo)
        {
            //use objectInfo to get assembly string with new criteria
            SerTrace.Log( this, "GetAssemblyId Entry ",objectInfo.objectType," isSi ",objectInfo.isSi);
            if (assemblyToIdTable == null)
                assemblyToIdTable = new Hashtable(5);

            long assemId = 0;
            bool isNew = false;
            String assemblyString = objectInfo.GetAssemblyString();

            String serializedAssemblyString = assemblyString;
            if (assemblyString.Length == 0)
            {
                assemId = 0;
            }
            else if (assemblyString.Equals(Converter.urtAssemblyString))
            {
                // Urt type is an assemId of 0. No assemblyString needs
                // to be sent 
                SerTrace.Log( this, "GetAssemblyId urt Assembly String ");
                assemId = 0;
            }
            else
            {
                // Assembly needs to be sent
                // Need to prefix assembly string to separate the string names from the
                // assemblyName string names. That is a string can have the same value
                // as an assemblyNameString, but it is serialized differently

                if (assemblyToIdTable.ContainsKey(assemblyString))
                {
                    assemId = (long)assemblyToIdTable[assemblyString];
                    isNew = false;
                }
                else
                {
                    assemId = InternalGetId("___AssemblyString___"+assemblyString, false, null, out isNew);
                    assemblyToIdTable[assemblyString] = assemId;
                }

                serWriter.WriteAssembly(objectInfo.objectType, serializedAssemblyString, (int)assemId, isNew);
            }
            SerTrace.Log( this, "GetAssemblyId Exit id ",assemId," isNew ",isNew," assemblyString ",serializedAssemblyString);
            return assemId;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private Type GetType(Object obj)
        {
            Type type = null;
#if FEATURE_REMOTING        
            if (RemotingServices.IsTransparentProxy(obj))
                type = Converter.typeofMarshalByRefObject;
            else
#endif // FEATURE_REMOTING        
                type = obj.GetType();
            return type;
        }

        private SerStack niPool = new SerStack("NameInfo Pool");

        private NameInfo GetNameInfo()
        {
            NameInfo nameInfo = null;

            if (!niPool.IsEmpty())
            {
                nameInfo = (NameInfo)niPool.Pop();
                nameInfo.Init();
            }
            else
                nameInfo = new NameInfo();

            return nameInfo;
        }

        private bool CheckTypeFormat(FormatterTypeStyle test, FormatterTypeStyle want)
        {
            return(test & want) == want;
        }

        private void PutNameInfo(NameInfo nameInfo)
        {
            niPool.Push(nameInfo);
        }

    }
}



    
