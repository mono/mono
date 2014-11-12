using System.Diagnostics.Contracts;
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
 **
 ** Class: BinaryParser
 **
 **
 ** Purpose: Parses Binary Stream
 **
 **
 ===========================================================*/


namespace System.Runtime.Serialization.Formatters.Binary {

    using System;
    using System.IO;
    using System.Collections;
    using System.Reflection;
    using System.Globalization;
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Text;

    internal sealed  class __BinaryParser
    {
        internal ObjectReader objectReader;
        internal Stream input;
        internal long topId;
        internal long headerId;
        internal SizedArray objectMapIdTable;
        internal SizedArray assemIdToAssemblyTable;    // Used to hold assembly information        
        internal SerStack stack = new SerStack("ObjectProgressStack");

        internal BinaryTypeEnum expectedType = BinaryTypeEnum.ObjectUrt;
        internal Object expectedTypeInformation;
        internal ParseRecord PRS;

        private BinaryAssemblyInfo systemAssemblyInfo;
        private BinaryReader dataReader;
        private static Encoding encoding = new UTF8Encoding(false, true);

        private SerStack opPool;        

        internal __BinaryParser(Stream stream, ObjectReader objectReader)
        {
            input = stream;
            this.objectReader = objectReader;
               dataReader = new BinaryReader(input, encoding);
        }

        internal BinaryAssemblyInfo SystemAssemblyInfo
        {
            get {
                if (systemAssemblyInfo == null)
                    systemAssemblyInfo = new BinaryAssemblyInfo(Converter.urtAssemblyString, Converter.urtAssembly);
                return systemAssemblyInfo;
            }
        }

        internal SizedArray ObjectMapIdTable
        {
            get {
            if (objectMapIdTable == null)
                objectMapIdTable = new SizedArray();

            return objectMapIdTable;
            }
        }

        internal SizedArray AssemIdToAssemblyTable
        {
            get {
                if (assemIdToAssemblyTable == null)
                {
                    assemIdToAssemblyTable = new SizedArray(2);
                }
                return assemIdToAssemblyTable;
            }
        }

        internal ParseRecord prs
        {
            get{
                if (PRS == null)
                    PRS = new ParseRecord();
                return PRS; 
            }
        }

        /*
         * Parse the input
         * Reads each record from the input stream. If the record is a primitive type (A number)
         *  then it doesn't have a BinaryHeaderEnum byte. For this case the expected type
         *  has been previously set to Primitive
         * @internalonly
         */     
        [System.Security.SecurityCritical]  // auto-generated
        internal void Run()
        {
            try
            {
                bool isLoop = true;
                ReadBegin();
                ReadSerializationHeaderRecord();
                while (isLoop)
                {
                    SerTrace.Log( this, "Run loop ",((Enum)expectedType).ToString());
                    BinaryHeaderEnum binaryHeaderEnum = BinaryHeaderEnum.Object;
                    switch (expectedType)
                    {
                    case BinaryTypeEnum.ObjectUrt:
                    case BinaryTypeEnum.ObjectUser:
                    case BinaryTypeEnum.String:
                    case BinaryTypeEnum.Object:
                    case BinaryTypeEnum.ObjectArray:
                    case BinaryTypeEnum.StringArray:
                    case BinaryTypeEnum.PrimitiveArray:
                        Byte inByte = dataReader.ReadByte();
                        binaryHeaderEnum = (BinaryHeaderEnum)inByte;
                        //Console.WriteLine("Beginning of loop "+((Enum)binaryHeaderEnum).ToString());
                        switch (binaryHeaderEnum)
                        {
                        case BinaryHeaderEnum.Assembly:
                        case BinaryHeaderEnum.CrossAppDomainAssembly:
                            ReadAssembly(binaryHeaderEnum);
                            break;
                        case BinaryHeaderEnum.Object:
                            ReadObject();
                            break;
                        case BinaryHeaderEnum.CrossAppDomainMap:
                            ReadCrossAppDomainMap();
                            break;
                        case BinaryHeaderEnum.ObjectWithMap:
                        case BinaryHeaderEnum.ObjectWithMapAssemId:
                            ReadObjectWithMap(binaryHeaderEnum);
                            break;
                        case BinaryHeaderEnum.ObjectWithMapTyped:
                        case BinaryHeaderEnum.ObjectWithMapTypedAssemId:
                            ReadObjectWithMapTyped(binaryHeaderEnum);                                   
                            break;
#if FEATURE_REMOTING                            
                        case BinaryHeaderEnum.MethodCall:
                        case BinaryHeaderEnum.MethodReturn:
                            ReadMethodObject(binaryHeaderEnum);                                 
                            break;
#endif                            
                        case BinaryHeaderEnum.ObjectString:
                        case BinaryHeaderEnum.CrossAppDomainString:
                            ReadObjectString(binaryHeaderEnum);
                            break;
                        case BinaryHeaderEnum.Array:
                        case BinaryHeaderEnum.ArraySinglePrimitive:
                        case BinaryHeaderEnum.ArraySingleObject:
                        case BinaryHeaderEnum.ArraySingleString:
                            ReadArray(binaryHeaderEnum);
                            break;
                        case BinaryHeaderEnum.MemberPrimitiveTyped:
                            ReadMemberPrimitiveTyped();
                            break;                                                              
                        case BinaryHeaderEnum.MemberReference:
                            ReadMemberReference();
                            break;
                        case BinaryHeaderEnum.ObjectNull:
                        case BinaryHeaderEnum.ObjectNullMultiple256:
                        case BinaryHeaderEnum.ObjectNullMultiple:
                            ReadObjectNull(binaryHeaderEnum);
                            break;
                        case BinaryHeaderEnum.MessageEnd:
                            isLoop = false;
                            ReadMessageEnd();
                            ReadEnd();
                            break;
                        default:
                            throw new SerializationException(Environment.GetResourceString("Serialization_BinaryHeader",inByte));
                        }
                        break;
                    case BinaryTypeEnum.Primitive:
                        ReadMemberPrimitiveUnTyped();
                        break;
                    default:
                        throw new SerializationException(Environment.GetResourceString("Serialization_TypeExpected"));

                    }

                    // If an assembly is encountered, don't advance
                    // object Progress, 
                    if (binaryHeaderEnum != BinaryHeaderEnum.Assembly)
                    {
                        // End of parse loop.
                        bool isData = false;
                        // Set up loop for next iteration.
                        // If this is an object, and the end of object has been reached, then parse object end.
                        while (!isData)
                        {
                            ObjectProgress op = (ObjectProgress)stack.Peek();
                            if (op == null)
                            {
                                // No more object on stack, then the next record is a top level object
                                SerTrace.Log( this, "Run loop op null, top level object");                      
                                expectedType = BinaryTypeEnum.ObjectUrt;
                                expectedTypeInformation = null;
                                isData = true;
                            }
                            else
                            {
                                SerTrace.Log( this, "Run loop op not null, continue object");
                                // Find out what record is expected next
                                isData = op.GetNext(out op.expectedType, out op.expectedTypeInformation);
                                expectedType = op.expectedType;
                                expectedTypeInformation = op.expectedTypeInformation;
                                SerTrace.Log( this, "Run loop opName ",op.name,", expectedType ",((Enum)expectedType).ToString()," expectedTypeInformation, ",expectedTypeInformation);

                                SerTrace.Log( this, "Run ",isData);     
                                if (!isData)
                                {
                                    // No record is expected next, this is the end of an object or array
                                    SerTrace.Log( this, "Run End of Object ");
                                    stack.Dump();

                                    prs.Init();
                                    if (op.memberValueEnum == InternalMemberValueE.Nested)
                                    {
                                        // Nested object
                                        prs.PRparseTypeEnum = InternalParseTypeE.MemberEnd;
                                        prs.PRmemberTypeEnum = op.memberTypeEnum;
                                        prs.PRmemberValueEnum = op.memberValueEnum;
                                        objectReader.Parse(prs);
                                    }
                                    else
                                    {
                                        // Top level object
                                        prs.PRparseTypeEnum = InternalParseTypeE.ObjectEnd;
                                        prs.PRmemberTypeEnum = op.memberTypeEnum;
                                        prs.PRmemberValueEnum = op.memberValueEnum;                             
                                        objectReader.Parse(prs);
                                    }
                                    stack.Pop();
                                    PutOp(op);
                                }
                            }
                        }
                    }
                }
            }
            catch (EndOfStreamException)
            {

                // EOF should never be thrown since there is a MessageEnd record to stop parsing
                BCLDebug.Trace("BINARY", "\n*****EOF*************************\n");
                throw new SerializationException(Environment.GetResourceString("Serialization_StreamEnd"));             
            }
        }


        internal void ReadBegin()
        {
            BCLDebug.Trace("BINARY", "\n%%%%%BinaryReaderBegin%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");
        }

        internal void ReadEnd()
        {
            BCLDebug.Trace("BINARY","\n%%%%%BinaryReaderEnd%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");
        }

        /*
         * Primitive Reads from Stream
         * @internalonly
         */

        internal bool ReadBoolean()
        {
            return dataReader.ReadBoolean();
        }

        internal Byte ReadByte()
        {
            return dataReader.ReadByte();
        }

        internal Byte[] ReadBytes(int length)
        {
            return dataReader.ReadBytes(length);
        }

        // Note: this method does a blocking read!
        internal void ReadBytes(byte[] byteA, int offset, int size)
        {
            while (size > 0)
            {
                int n = dataReader.Read(byteA, offset, size);
                if (n == 0)
                    __Error.EndOfFile();
                offset += n;
                size -= n;
            }
        }

        internal Char ReadChar()
        {
            return dataReader.ReadChar();
        }

        internal Char[] ReadChars(int length)
        {
            return dataReader.ReadChars(length);            
        }       

        internal Decimal ReadDecimal()
        {
            return Decimal.Parse(dataReader.ReadString(), CultureInfo.InvariantCulture);
        }

        internal Single ReadSingle()
        {
            return dataReader.ReadSingle();
        }   

        internal Double ReadDouble()
        {
            return dataReader.ReadDouble();
        }

        internal Int16 ReadInt16()
        {
            return dataReader.ReadInt16();
        }

        internal Int32 ReadInt32()
        {
            return dataReader.ReadInt32();
        }

        internal Int64 ReadInt64()
        {
            return dataReader.ReadInt64();
        }

        internal SByte ReadSByte()
        {
            return(SByte)ReadByte();
        }

        internal String ReadString()
        {
            return dataReader.ReadString();
        }

        internal TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(ReadInt64());
        }

        internal DateTime ReadDateTime()
        {
            return DateTime.FromBinaryRaw(ReadInt64());
        }

        internal UInt16 ReadUInt16()
        {
            return dataReader.ReadUInt16();         
        }

        internal UInt32 ReadUInt32()
        {
            return dataReader.ReadUInt32();         
        }

        internal UInt64 ReadUInt64()
        {
            return dataReader.ReadUInt64();         
        }

        // Binary Stream Record Reads
        [System.Security.SecurityCritical]  // auto-generated
        internal void ReadSerializationHeaderRecord()
        {
            SerTrace.Log( this, "ReadSerializationHeaderRecord");
            SerializationHeaderRecord record = new SerializationHeaderRecord();
                record.Read(this);
                record.Dump();
             this.topId = (record.topId > 0 ? objectReader.GetId(record.topId) : record.topId);
             this.headerId = (record.headerId > 0 ? objectReader.GetId(record.headerId) : record.headerId);
        }

        [System.Security.SecurityCritical]
        internal void ReadAssembly(BinaryHeaderEnum binaryHeaderEnum)
        {
            SerTrace.Log( this, "ReadAssembly");
            BinaryAssembly record = new BinaryAssembly();
            if (binaryHeaderEnum == BinaryHeaderEnum.CrossAppDomainAssembly)
            {
                BinaryCrossAppDomainAssembly crossAppDomainAssembly = new BinaryCrossAppDomainAssembly();
                crossAppDomainAssembly.Read(this);
                crossAppDomainAssembly.Dump();
                record.assemId = crossAppDomainAssembly.assemId;
                record.assemblyString = objectReader.CrossAppDomainArray(crossAppDomainAssembly.assemblyIndex) as String;
                if (record.assemblyString == null)
                    throw new SerializationException(Environment.GetResourceString("Serialization_CrossAppDomainError","String", crossAppDomainAssembly.assemblyIndex));

            }
            else
            {
                record.Read(this);
                record.Dump();
            }

            AssemIdToAssemblyTable[record.assemId] = new BinaryAssemblyInfo(record.assemblyString);
        }

#if FEATURE_REMOTING
        [System.Security.SecurityCritical]  // auto-generated
        internal void ReadMethodObject(BinaryHeaderEnum binaryHeaderEnum)
        {
            SerTrace.Log( this, "ReadMethodObject");
            if (binaryHeaderEnum == BinaryHeaderEnum.MethodCall)
            {
                BinaryMethodCall record = new BinaryMethodCall();
                record.Read(this);
                record.Dump();
                objectReader.SetMethodCall(record);
            }
            else
            {
                BinaryMethodReturn record = new BinaryMethodReturn();
                record.Read(this);
                record.Dump();
                objectReader.SetMethodReturn(record);
            }
        }
#endif

        private BinaryObject binaryObject;

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadObject()
        {
            SerTrace.Log( this, "ReadObject");

            if (binaryObject == null)
                binaryObject = new BinaryObject();
            binaryObject.Read(this);
            binaryObject.Dump();

            ObjectMap objectMap = (ObjectMap)ObjectMapIdTable[binaryObject.mapId];
            if (objectMap == null)
                throw new SerializationException(Environment.GetResourceString("Serialization_Map",binaryObject.mapId));

            ObjectProgress op = GetOp();
            ParseRecord pr = op.pr;
            stack.Push(op);

            op.objectTypeEnum = InternalObjectTypeE.Object;
            op.binaryTypeEnumA = objectMap.binaryTypeEnumA;
            op.memberNames = objectMap.memberNames;
            op.memberTypes = objectMap.memberTypes;
            op.typeInformationA = objectMap.typeInformationA;
            op.memberLength = op.binaryTypeEnumA.Length;
            ObjectProgress objectOp = (ObjectProgress)stack.PeekPeek();
            if ((objectOp == null) || (objectOp.isInitial))
            {
                // Non-Nested Object
                SerTrace.Log( this, "ReadObject non-nested ");              
                op.name = objectMap.objectName;
                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                op.memberValueEnum = InternalMemberValueE.Empty;            
            }
            else
            {
                // Nested Object
                SerTrace.Log( this, "ReadObject nested ");                              
                pr.PRparseTypeEnum = InternalParseTypeE.Member;
                pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                op.memberValueEnum = InternalMemberValueE.Nested;

                switch (objectOp.objectTypeEnum)
                {
                case InternalObjectTypeE.Object:
                    pr.PRname = objectOp.name;                      
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                    op.memberTypeEnum = InternalMemberTypeE.Field;
                    break;
                case InternalObjectTypeE.Array:
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                    op.memberTypeEnum = InternalMemberTypeE.Item;                   
                    break;
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_Map",((Enum)objectOp.objectTypeEnum).ToString()));                                     
                }
            }


            pr.PRobjectId = objectReader.GetId((long)binaryObject.objectId);
            SerTrace.Log( this, "ReadObject binaryObject.objectId ",pr.PRobjectId);                         
            pr.PRobjectInfo = objectMap.CreateObjectInfo(ref pr.PRsi, ref pr.PRmemberData);

            if (pr.PRobjectId == topId)
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;

            pr.PRobjectTypeEnum = InternalObjectTypeE.Object;       
            pr.PRkeyDt = objectMap.objectName;
            pr.PRdtType = objectMap.objectType;
            pr.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            objectReader.Parse(pr);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void ReadCrossAppDomainMap()
        {
            SerTrace.Log( this, "ReadObjectWithCrossAppDomainMap");
            BinaryCrossAppDomainMap record = new BinaryCrossAppDomainMap();
            record.Read(this);
            record.Dump();
            Object mapObject = objectReader.CrossAppDomainArray(record.crossAppDomainArrayIndex);
            BinaryObjectWithMap binaryObjectWithMap = mapObject as BinaryObjectWithMap;
            if (binaryObjectWithMap != null)
            {
                binaryObjectWithMap.Dump();
                ReadObjectWithMap(binaryObjectWithMap);
            }
            else
            {
                BinaryObjectWithMapTyped binaryObjectWithMapTyped = mapObject as BinaryObjectWithMapTyped;
                if (binaryObjectWithMapTyped != null)
                {
#if _DEBUG                    
                    binaryObjectWithMapTyped.Dump();
#endif
                    ReadObjectWithMapTyped(binaryObjectWithMapTyped);
                }
                else
                    throw new SerializationException(Environment.GetResourceString("Serialization_CrossAppDomainError","BinaryObjectMap", mapObject));
            }
        }


        private BinaryObjectWithMap bowm;

        [System.Security.SecurityCritical]  // auto-generated
        internal void ReadObjectWithMap(BinaryHeaderEnum binaryHeaderEnum)
        {
            SerTrace.Log( this, "ReadObjectWithMap");
            if (bowm == null)
                bowm = new BinaryObjectWithMap(binaryHeaderEnum);
            else
                bowm.binaryHeaderEnum = binaryHeaderEnum;
            bowm.Read(this);
            bowm.Dump();
            ReadObjectWithMap(bowm);
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadObjectWithMap(BinaryObjectWithMap record)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            ObjectProgress op = GetOp();
            ParseRecord pr = op.pr;
            stack.Push(op);


            if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapAssemId)
            {
                if (record.assemId < 1)
                    throw new SerializationException(Environment.GetResourceString("Serialization_Assembly",record.name));

                assemblyInfo = ((BinaryAssemblyInfo)AssemIdToAssemblyTable[record.assemId]);

                if (assemblyInfo == null)
                    throw new SerializationException(Environment.GetResourceString("Serialization_Assembly",record.assemId+" "+record.name));
                SerTrace.Log( this, "ReadObjectWithMap  lookup assemIdToAssembly assemId ",record.assemId," assembly ",assemblyInfo.assemblyString);                
            }
            else if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMap)
            {

                assemblyInfo = SystemAssemblyInfo; //Urt assembly
            }

            Type objectType = objectReader.GetType(assemblyInfo, record.name);

            ObjectMap objectMap = ObjectMap.Create(record.name, objectType, record.memberNames, objectReader, record.objectId, assemblyInfo);
            ObjectMapIdTable[record.objectId] = objectMap;

            op.objectTypeEnum = InternalObjectTypeE.Object;     
            op.binaryTypeEnumA = objectMap.binaryTypeEnumA;
            op.typeInformationA = objectMap.typeInformationA;       
            op.memberLength = op.binaryTypeEnumA.Length;
            op.memberNames = objectMap.memberNames;
            op.memberTypes = objectMap.memberTypes;

            ObjectProgress objectOp = (ObjectProgress)stack.PeekPeek();

            if ((objectOp == null) || (objectOp.isInitial))
            {
                // Non-Nested Object
                op.name = record.name;
                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                op.memberValueEnum = InternalMemberValueE.Empty;                                    

            }
            else
            {
                // Nested Object
                pr.PRparseTypeEnum = InternalParseTypeE.Member;
                pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                op.memberValueEnum = InternalMemberValueE.Nested;                       

                switch (objectOp.objectTypeEnum)
                {
                case InternalObjectTypeE.Object:
                    pr.PRname = objectOp.name;                      
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                    op.memberTypeEnum = InternalMemberTypeE.Field;                  
                    break;
                case InternalObjectTypeE.Array:
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                    op.memberTypeEnum = InternalMemberTypeE.Field;                  
                    break;
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_ObjectTypeEnum",((Enum)objectOp.objectTypeEnum).ToString()));                      
                }

            }
            pr.PRobjectTypeEnum = InternalObjectTypeE.Object;
            pr.PRobjectId = objectReader.GetId((long)record.objectId);
            pr.PRobjectInfo = objectMap.CreateObjectInfo(ref pr.PRsi, ref pr.PRmemberData);

            if (pr.PRobjectId == topId)
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;

            pr.PRkeyDt = record.name;
            pr.PRdtType = objectMap.objectType;
            pr.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            objectReader.Parse(pr);
        }

        private BinaryObjectWithMapTyped bowmt;

        [System.Security.SecurityCritical]  // auto-generated
        internal void ReadObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)     
        {
            SerTrace.Log( this, "ReadObjectWithMapTyped");
            if (bowmt == null)
                bowmt = new BinaryObjectWithMapTyped(binaryHeaderEnum);
            else
                bowmt.binaryHeaderEnum = binaryHeaderEnum;
            bowmt.Read(this);
#if _DEBUG            
            bowmt.Dump();
#endif
            ReadObjectWithMapTyped(bowmt);
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadObjectWithMapTyped(BinaryObjectWithMapTyped record)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            ObjectProgress op = GetOp();
            ParseRecord pr = op.pr;
            stack.Push(op);

            if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                if (record.assemId < 1)
                    throw new SerializationException(Environment.GetResourceString("Serialization_AssemblyId",record.name));

                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[record.assemId];
                if (assemblyInfo == null)
                    throw new SerializationException(Environment.GetResourceString("Serialization_AssemblyId",record.assemId+" "+record.name));

                SerTrace.Log( this, "ReadObjectWithMapTyped  lookup assemIdToAssembly assemId ",record.assemId," assembly ",assemblyInfo.assemblyString);                               
            }
            else if (record.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTyped)
            {
                assemblyInfo = SystemAssemblyInfo; // Urt assembly
            }

            ObjectMap objectMap = ObjectMap.Create(record.name, record.memberNames, record.binaryTypeEnumA, record.typeInformationA, record.memberAssemIds, objectReader, record.objectId, assemblyInfo, AssemIdToAssemblyTable);
            ObjectMapIdTable[record.objectId] = objectMap;
            op.objectTypeEnum = InternalObjectTypeE.Object;
            op.binaryTypeEnumA = objectMap.binaryTypeEnumA;
            op.typeInformationA = objectMap.typeInformationA;               
            op.memberLength = op.binaryTypeEnumA.Length;
            op.memberNames = objectMap.memberNames;
            op.memberTypes = objectMap.memberTypes;

            ObjectProgress objectOp = (ObjectProgress)stack.PeekPeek();

            if ((objectOp == null) || (objectOp.isInitial))
            {
                // Non-Nested Object
                op.name = record.name;
                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                op.memberValueEnum = InternalMemberValueE.Empty;                        
            }
            else
            {
                // Nested Object
                pr.PRparseTypeEnum = InternalParseTypeE.Member;
                pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                op.memberValueEnum = InternalMemberValueE.Nested;           

                switch (objectOp.objectTypeEnum)
                {
                case InternalObjectTypeE.Object:
                    pr.PRname = objectOp.name;                      
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                    op.memberTypeEnum = InternalMemberTypeE.Field;                  
                    break;
                case InternalObjectTypeE.Array:
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                    op.memberTypeEnum = InternalMemberTypeE.Item;                   
                    break;
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_ObjectTypeEnum",((Enum)objectOp.objectTypeEnum).ToString()));
                }

            }

            pr.PRobjectTypeEnum = InternalObjectTypeE.Object;
            pr.PRobjectInfo = objectMap.CreateObjectInfo(ref pr.PRsi, ref pr.PRmemberData);
            pr.PRobjectId = objectReader.GetId((long)record.objectId);              
            if (pr.PRobjectId == topId)
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;
            pr.PRkeyDt = record.name;
            pr.PRdtType = objectMap.objectType;
            pr.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            objectReader.Parse(pr);
        }

        internal BinaryObjectString objectString;
        internal BinaryCrossAppDomainString crossAppDomainString;

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadObjectString(BinaryHeaderEnum binaryHeaderEnum)
        {
            SerTrace.Log( this, "ReadObjectString");

            if (objectString == null)
                objectString = new BinaryObjectString();

            if (binaryHeaderEnum == BinaryHeaderEnum.ObjectString)
            {
                objectString.Read(this);
                objectString.Dump();
            }
            else
            {
                if (crossAppDomainString == null)
                    crossAppDomainString = new BinaryCrossAppDomainString();
                crossAppDomainString.Read(this);
                crossAppDomainString.Dump();
                objectString.value = objectReader.CrossAppDomainArray(crossAppDomainString.value) as String;
                if (objectString.value == null)
                    throw new SerializationException(Environment.GetResourceString("Serialization_CrossAppDomainError","String", crossAppDomainString.value));

                objectString.objectId = crossAppDomainString.objectId;
            }

            prs.Init();
            prs.PRparseTypeEnum = InternalParseTypeE.Object;
            prs.PRobjectId = objectReader.GetId(objectString.objectId);

            if (prs.PRobjectId == topId)
                prs.PRobjectPositionEnum = InternalObjectPositionE.Top;

            prs.PRobjectTypeEnum = InternalObjectTypeE.Object;

            ObjectProgress objectOp = (ObjectProgress)stack.Peek();

            prs.PRvalue = objectString.value;
            prs.PRkeyDt = "System.String";
            prs.PRdtType = Converter.typeofString;
            prs.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            prs.PRvarValue = objectString.value; //Need to set it because ObjectReader is picking up value from variant, not pr.PRvalue

            if (objectOp == null)
            {
                // Top level String
                SerTrace.Log( this, "ReadObjectString, Non-Nested");            
                prs.PRparseTypeEnum = InternalParseTypeE.Object;
                prs.PRname = "System.String";
            }
            else
            {
                // Nested in an Object

                SerTrace.Log( this, "ReadObjectString, Nested");
                prs.PRparseTypeEnum = InternalParseTypeE.Member;
                prs.PRmemberValueEnum = InternalMemberValueE.InlineValue;

                switch (objectOp.objectTypeEnum)
                {
                case InternalObjectTypeE.Object:
                    prs.PRname = objectOp.name;
                    prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                    break;
                case InternalObjectTypeE.Array:                 
                    prs.PRmemberTypeEnum = InternalMemberTypeE.Item;
                    break;
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_ObjectTypeEnum",((Enum)objectOp.objectTypeEnum).ToString()));                      
                }

            }

            objectReader.Parse(prs);
        }



        internal MemberPrimitiveTyped memberPrimitiveTyped;

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadMemberPrimitiveTyped()
        {
            SerTrace.Log( this, "ReadObjectPrimitive");

            if (memberPrimitiveTyped == null)
                memberPrimitiveTyped = new MemberPrimitiveTyped();

            memberPrimitiveTyped.Read(this);
            memberPrimitiveTyped.Dump();

            prs.PRobjectTypeEnum = InternalObjectTypeE.Object; //Get rid of 
            ObjectProgress objectOp = (ObjectProgress)stack.Peek();

            prs.Init();
            prs.PRvarValue = memberPrimitiveTyped.value;
            prs.PRkeyDt = Converter.ToComType(memberPrimitiveTyped.primitiveTypeEnum);
            prs.PRdtType = Converter.ToType(memberPrimitiveTyped.primitiveTypeEnum);
            prs.PRdtTypeCode = memberPrimitiveTyped.primitiveTypeEnum;

            if (objectOp == null)
            {
                // Top level boxed primitive
                SerTrace.Log( this, "ReadObjectPrimitive, Non-Nested");         
                prs.PRparseTypeEnum = InternalParseTypeE.Object;
                prs.PRname = "System.Variant";
            }
            else
            {
                // Nested in an Object
                SerTrace.Log( this, "ReadObjectPrimitive, Nested");

                prs.PRparseTypeEnum = InternalParseTypeE.Member;
                prs.PRmemberValueEnum = InternalMemberValueE.InlineValue;

                switch (objectOp.objectTypeEnum)
                {
                case InternalObjectTypeE.Object:
                    prs.PRname = objectOp.name;
                    prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                    break;
                case InternalObjectTypeE.Array:
                    prs.PRmemberTypeEnum = InternalMemberTypeE.Item;
                    break;
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_ObjectTypeEnum",((Enum)objectOp.objectTypeEnum).ToString()));                                              
                }
            }

            objectReader.Parse(prs);
        }


        [System.Security.SecurityCritical]  // auto-generated
        private void ReadArray(BinaryHeaderEnum binaryHeaderEnum)
        {
            BinaryAssemblyInfo assemblyInfo = null;
            SerTrace.Log( this, "ReadArray ");
            BinaryArray record = new BinaryArray(binaryHeaderEnum);
            record.Read(this);
#if _DEBUG                        
            record.Dump();

            SerTrace.Log( this, "Read 1 ",((Enum)binaryHeaderEnum).ToString());
#endif
            if (record.binaryTypeEnum == BinaryTypeEnum.ObjectUser)
            {
                if (record.assemId < 1)
                    throw new SerializationException(Environment.GetResourceString("Serialization_AssemblyId",record.typeInformation));

                assemblyInfo = (BinaryAssemblyInfo)AssemIdToAssemblyTable[record.assemId];
                SerTrace.Log( this, "ReadArray  lookup assemIdToAssembly assemId ",record.assemId," assembly ",assemblyInfo.assemblyString);                                
            }
            else
                assemblyInfo = SystemAssemblyInfo; //Urt assembly

            ObjectProgress op = GetOp();
            ParseRecord pr = op.pr;

            op.objectTypeEnum = InternalObjectTypeE.Array;
            op.binaryTypeEnum = record.binaryTypeEnum;
            op.typeInformation = record.typeInformation;

            ObjectProgress objectOp = (ObjectProgress)stack.PeekPeek();
            if ((objectOp == null) || (record.objectId > 0))
            {
                // Non-Nested Object
                op.name = "System.Array";
                pr.PRparseTypeEnum = InternalParseTypeE.Object;
                op.memberValueEnum = InternalMemberValueE.Empty;                                    
            }
            else
            {
                // Nested Object            
                pr.PRparseTypeEnum = InternalParseTypeE.Member;
                pr.PRmemberValueEnum = InternalMemberValueE.Nested;
                op.memberValueEnum = InternalMemberValueE.Nested;                       

                switch (objectOp.objectTypeEnum)
                {
                case InternalObjectTypeE.Object:
                    pr.PRname = objectOp.name;                                  
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Field;
                    op.memberTypeEnum = InternalMemberTypeE.Field;                  
                    pr.PRkeyDt = objectOp.name;
                    pr.PRdtType = objectOp.dtType;
                    break;
                case InternalObjectTypeE.Array:
                    pr.PRmemberTypeEnum = InternalMemberTypeE.Item;
                    op.memberTypeEnum = InternalMemberTypeE.Item;                   
                    break;
                default:
                    throw new SerializationException(Environment.GetResourceString("Serialization_ObjectTypeEnum",((Enum)objectOp.objectTypeEnum).ToString()));                                              
                }
            }


            pr.PRobjectId = objectReader.GetId((long)record.objectId);
            if (pr.PRobjectId == topId)
                pr.PRobjectPositionEnum = InternalObjectPositionE.Top;
            else if ((headerId > 0) &&(pr.PRobjectId == headerId))
                pr.PRobjectPositionEnum = InternalObjectPositionE.Headers; // Headers are an array of header objects
            else
                pr.PRobjectPositionEnum    = InternalObjectPositionE.Child;

            pr.PRobjectTypeEnum = InternalObjectTypeE.Array;

            BinaryConverter.TypeFromInfo(record.binaryTypeEnum, record.typeInformation, objectReader, assemblyInfo,
                                         out pr.PRarrayElementTypeCode, out pr.PRarrayElementTypeString,
                                         out pr.PRarrayElementType, out pr.PRisArrayVariant);

            pr.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;


            pr.PRrank = record.rank;
            pr.PRlengthA = record.lengthA;
            pr.PRlowerBoundA = record.lowerBoundA;
            bool isPrimitiveArray = false;

            switch (record.binaryArrayTypeEnum)
            {
            case BinaryArrayTypeEnum.Single:
            case BinaryArrayTypeEnum.SingleOffset:                  
                op.numItems = record.lengthA[0];
                pr.PRarrayTypeEnum = InternalArrayTypeE.Single;
                if (Converter.IsWriteAsByteArray(pr.PRarrayElementTypeCode) &&
                    (record.lowerBoundA[0] == 0))
                {
                    isPrimitiveArray = true;
                    ReadArrayAsBytes(pr);
                }
                break;
            case BinaryArrayTypeEnum.Jagged:
            case BinaryArrayTypeEnum.JaggedOffset:                  
                op.numItems = record.lengthA[0];
                pr.PRarrayTypeEnum = InternalArrayTypeE.Jagged;                 
                break;
            case BinaryArrayTypeEnum.Rectangular:
            case BinaryArrayTypeEnum.RectangularOffset:                 
                int arrayLength = 1;
                for (int i=0; i<record.rank; i++)
                    arrayLength = arrayLength*record.lengthA[i];
                op.numItems = arrayLength;
                pr.PRarrayTypeEnum = InternalArrayTypeE.Rectangular;                                        
                break;
            default:
                throw new SerializationException(Environment.GetResourceString("Serialization_ArrayType",((Enum)record.binaryArrayTypeEnum).ToString()));
            }

            if (!isPrimitiveArray)
                stack.Push(op);
            else
            {
                PutOp(op);
            }

            SerTrace.Log( this, "ReadArray ",((Enum)record.binaryArrayTypeEnum).ToString()," length ",op.numItems);             
            objectReader.Parse(pr);

            if (isPrimitiveArray)
            {
                pr.PRparseTypeEnum = InternalParseTypeE.ObjectEnd;
                objectReader.Parse(pr);
            }
        }

        private byte[] byteBuffer;
        private const int chunkSize = 4096;

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadArrayAsBytes(ParseRecord pr)
        {
            if (pr.PRarrayElementTypeCode == InternalPrimitiveTypeE.Byte)
                pr.PRnewObj = ReadBytes(pr.PRlengthA[0]);
            else if (pr.PRarrayElementTypeCode == InternalPrimitiveTypeE.Char)
                pr.PRnewObj = ReadChars(pr.PRlengthA[0]);
            else
            {
                int typeLength = Converter.TypeLength(pr.PRarrayElementTypeCode);

                pr.PRnewObj = Converter.CreatePrimitiveArray(pr.PRarrayElementTypeCode, pr.PRlengthA[0]);

                //pr.PRnewObj = Array.CreateInstance(pr.PRarrayElementType, pr.PRlengthA[0]);
                Contract.Assert((pr.PRnewObj != null),"[BinaryParser expected a Primitive Array]");

                Array array = (Array)pr.PRnewObj;
                int arrayOffset = 0;
                if (byteBuffer == null)
                    byteBuffer = new byte[chunkSize];

                while (arrayOffset < array.Length)
                {
                    int numArrayItems = Math.Min(chunkSize/typeLength, array.Length-arrayOffset);
                    int bufferUsed = numArrayItems*typeLength;
                    ReadBytes(byteBuffer, 0, bufferUsed);
#if BIGENDIAN
                    // we know that we are reading a primitive type, so just do a simple swap
                    for (int i = 0; i < bufferUsed; i += typeLength) 
                    {
                        for (int j = 0; j < typeLength / 2; j++) 
                        {
                            byte tmp = byteBuffer[i + j];
                            byteBuffer[i + j] = byteBuffer[i + typeLength - 1 - j];
                            byteBuffer[i + typeLength - 1 - j] = tmp;
                        }
                    }
#endif
                    Buffer.InternalBlockCopy(byteBuffer, 0, array, arrayOffset*typeLength, bufferUsed);
                    arrayOffset += numArrayItems;
                }
            }
        }

        internal MemberPrimitiveUnTyped memberPrimitiveUnTyped;

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadMemberPrimitiveUnTyped()
        {
            SerTrace.Log( this, "ReadMemberPrimitiveUnTyped ");     
            ObjectProgress objectOp = (ObjectProgress)stack.Peek();
            if (memberPrimitiveUnTyped == null)
                memberPrimitiveUnTyped = new MemberPrimitiveUnTyped();
            memberPrimitiveUnTyped.Set((InternalPrimitiveTypeE)expectedTypeInformation);
            memberPrimitiveUnTyped.Read(this);
            memberPrimitiveUnTyped.Dump();

            prs.Init();
            prs.PRvarValue = memberPrimitiveUnTyped.value;

            prs.PRdtTypeCode = (InternalPrimitiveTypeE)expectedTypeInformation;
            prs.PRdtType = Converter.ToType(prs.PRdtTypeCode);
            prs.PRparseTypeEnum = InternalParseTypeE.Member;
            prs.PRmemberValueEnum = InternalMemberValueE.InlineValue;

            if (objectOp.objectTypeEnum == InternalObjectTypeE.Object)
            {
                prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                prs.PRname = objectOp.name;
            }
            else
                prs.PRmemberTypeEnum = InternalMemberTypeE.Item;            

            objectReader.Parse(prs);
        }


        internal MemberReference memberReference;

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadMemberReference()
        {
            SerTrace.Log( this, "ReadMemberReference ");

            if (memberReference == null)
                memberReference = new MemberReference();
            memberReference.Read(this);
            memberReference.Dump();

            ObjectProgress objectOp = (ObjectProgress)stack.Peek();

            prs.Init();
            prs.PRidRef = objectReader.GetId((long)memberReference.idRef);
            prs.PRparseTypeEnum = InternalParseTypeE.Member;
            prs.PRmemberValueEnum = InternalMemberValueE.Reference;

            if (objectOp.objectTypeEnum == InternalObjectTypeE.Object)
            {
                prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                prs.PRname = objectOp.name;
                prs.PRdtType = objectOp.dtType;
            }
            else
                prs.PRmemberTypeEnum = InternalMemberTypeE.Item;


            objectReader.Parse(prs);
        }

        internal ObjectNull objectNull;

        [System.Security.SecurityCritical]  // auto-generated
        private void ReadObjectNull(BinaryHeaderEnum binaryHeaderEnum)
        {
            SerTrace.Log( this, "ReadObjectNull ");

            if (objectNull == null)
                objectNull = new ObjectNull();

            objectNull.Read(this, binaryHeaderEnum);
            objectNull.Dump();

            ObjectProgress objectOp = (ObjectProgress)stack.Peek();

            prs.Init();
            prs.PRparseTypeEnum = InternalParseTypeE.Member;
            prs.PRmemberValueEnum = InternalMemberValueE.Null;

            if (objectOp.objectTypeEnum == InternalObjectTypeE.Object)
            {
                prs.PRmemberTypeEnum = InternalMemberTypeE.Field;
                prs.PRname = objectOp.name;
                prs.PRdtType = objectOp.dtType;         
            }
            else
            {
                prs.PRmemberTypeEnum = InternalMemberTypeE.Item;
                prs.PRnullCount = objectNull.nullCount; 
                //only one null position has been incremented by GetNext
                //The position needs to be reset for the rest of the nulls
                objectOp.ArrayCountIncrement(objectNull.nullCount-1); 
            }
            objectReader.Parse(prs);
        }

        internal static volatile MessageEnd messageEnd;

        [System.Security.SecurityCritical]
        private void ReadMessageEnd()
        {
            SerTrace.Log( this, "ReadMessageEnd ");

            if (messageEnd == null)
                messageEnd = new MessageEnd();

            messageEnd.Read(this);

            messageEnd.Dump();

            if (!stack.IsEmpty())
            {
                SerTrace.Log( this, "ReadMessageEnd  Stack not empty ");
                stack.Dump();
                throw new SerializationException(Environment.GetResourceString("Serialization_StreamEnd"));
            }
        }


        // ReadValue from stream using InternalPrimitiveTypeE code
        internal Object ReadValue(InternalPrimitiveTypeE code)
        {
            SerTrace.Log( this, "ReadValue ",((Enum)code).ToString());
            Object var = null;

            switch (code)
            {
            case InternalPrimitiveTypeE.Boolean:
                var = ReadBoolean();
                break;
            case InternalPrimitiveTypeE.Byte:
                var = ReadByte();
                break;
            case InternalPrimitiveTypeE.Char:
                var = ReadChar();           
                break;
            case InternalPrimitiveTypeE.Double:
                var = ReadDouble();
                break;
            case InternalPrimitiveTypeE.Int16:
                var = ReadInt16();
                break;
            case InternalPrimitiveTypeE.Int32:
                var = ReadInt32();
                break;
            case InternalPrimitiveTypeE.Int64:
                var = ReadInt64();          
                break;
            case InternalPrimitiveTypeE.SByte:
                var = ReadSByte();
                break;
            case InternalPrimitiveTypeE.Single:
                var = ReadSingle();         
                break;
            case InternalPrimitiveTypeE.UInt16:
                var = ReadUInt16();                     
                break;
            case InternalPrimitiveTypeE.UInt32:
                var = ReadUInt32();                                 
                break;
            case InternalPrimitiveTypeE.UInt64:
                var = ReadUInt64();                                             
                break;
            case InternalPrimitiveTypeE.Decimal:
                var = ReadDecimal();                    
                break;
            case InternalPrimitiveTypeE.TimeSpan:
                var = ReadTimeSpan();                                       
                break;
            case InternalPrimitiveTypeE.DateTime:
                var = ReadDateTime();                                                           
                break;
            default:
                throw new SerializationException(Environment.GetResourceString("Serialization_TypeCode",((Enum)code).ToString()));
            }
            SerTrace.Log( "ReadValue Exit ",var);
            return var;
        }

        private ObjectProgress GetOp()
        {
            ObjectProgress op = null;

            if (opPool != null && !opPool.IsEmpty())
            {
                op = (ObjectProgress)opPool.Pop();
                op.Init();
            }
            else
                op = new ObjectProgress();

            return op;
        }

        private void PutOp(ObjectProgress op)
        {
            if (opPool == null)
                opPool = new SerStack("opPool");
            opPool.Push(op);
        }

    }
                }
    
