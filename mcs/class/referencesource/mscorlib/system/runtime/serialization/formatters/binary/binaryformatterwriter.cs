// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
 **
 ** Class: BinaryWriter
 **
 **
 ** Purpose: Writes primitive values to a stream
 **
 **
 ===========================================================*/

namespace System.Runtime.Serialization.Formatters.Binary {

    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Globalization;
    using System.Runtime.Serialization.Formatters;
    using System.Configuration.Assemblies;
    using System.Threading;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;

    internal sealed class __BinaryWriter
    {
        internal Stream sout;
        internal FormatterTypeStyle formatterTypeStyle;
        internal Hashtable objectMapTable;
        internal ObjectWriter objectWriter = null;
        internal BinaryWriter dataWriter = null;

// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal int m_nestedObjectCount;
#pragma warning restore 0414
        private int nullCount = 0; //Count of consecutive array nulls

        // Constructor
        internal __BinaryWriter(Stream sout, ObjectWriter objectWriter, FormatterTypeStyle formatterTypeStyle)
        {
            SerTrace.Log( this, "BinaryWriter ");
            this.sout = sout;
            this.formatterTypeStyle = formatterTypeStyle;
            this.objectWriter = objectWriter;
            m_nestedObjectCount = 0;
            dataWriter = new BinaryWriter(sout, Encoding.UTF8);
        }

        internal void WriteBegin()
        {
            BCLDebug.Trace("BINARY", "\n%%%%%BinaryWriterBegin%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");
        }

        internal void WriteEnd()
        {
            BCLDebug.Trace("BINARY", "\n%%%%%BinaryWriterEnd%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");
            dataWriter.Flush();
        }

        // Methods to write a value onto the stream
        internal void WriteBoolean(Boolean value)
        {
            dataWriter.Write(value);
        }

        internal void WriteByte(Byte value)
        {
            dataWriter.Write(value);
        }

        private void WriteBytes(Byte[] value)
        {
            dataWriter.Write(value);
        }

        private void WriteBytes(byte[] byteA, int offset, int size)
        {
            dataWriter.Write(byteA, offset, size);
        }

        internal void WriteChar(Char value)
        {
            dataWriter.Write(value);
        }

        internal void WriteChars(Char[] value)
        {
            dataWriter.Write(value);
        }


        internal void WriteDecimal(Decimal value)
        {
            WriteString(value.ToString(CultureInfo.InvariantCulture));
        }

        internal void WriteSingle(Single value)
        {
            dataWriter.Write(value);
        }

        internal void WriteDouble(Double value)
        {
            dataWriter.Write(value);
        }

        internal void WriteInt16(Int16 value)
        {
            dataWriter.Write(value);
        }

        internal void WriteInt32(Int32 value)
        {
            dataWriter.Write(value);
        }

        internal void WriteInt64(Int64 value)
        {
            dataWriter.Write(value);
        }

        internal void WriteSByte(SByte value)
        {
            WriteByte((Byte)value);
        }

        internal void WriteString(String value)
        {
            dataWriter.Write(value);
        }

        internal void WriteTimeSpan(TimeSpan value)
        {
            WriteInt64(value.Ticks);
        }

        internal void WriteDateTime(DateTime value)
        {
            WriteInt64(value.ToBinaryRaw());
        }

        internal void WriteUInt16(UInt16 value)
        {
            dataWriter.Write(value);
        }

        internal void WriteUInt32(UInt32 value)
        {
            dataWriter.Write(value);
        }

        internal void WriteUInt64(UInt64 value)
        {
            dataWriter.Write(value);
        }

        internal void WriteObjectEnd(NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
        }

        internal void WriteSerializationHeaderEnd()
        {
            MessageEnd record = new MessageEnd();
            record.Dump(sout);
            record.Write(this);
        }

        // Methods to write Binary Serialization Record onto the stream, a record is composed of primitive types

        internal void WriteSerializationHeader(int topId, int headerId, int minorVersion, int majorVersion)
        {
            SerializationHeaderRecord record = new SerializationHeaderRecord(BinaryHeaderEnum.SerializedStreamHeader, topId, headerId, minorVersion, majorVersion);
            record.Dump();
            record.Write(this);
        }


        internal BinaryMethodCall binaryMethodCall;
        internal void WriteMethodCall()
        {
            if (binaryMethodCall == null)
                binaryMethodCall = new BinaryMethodCall();

            binaryMethodCall.Dump();
            binaryMethodCall.Write(this);
        }

        internal Object[] WriteCallArray(String uri, String methodName, String typeName, Type[] instArgs, Object[] args, Object methodSignature, Object callContext, Object[] properties)
        {
            if (binaryMethodCall == null)
                binaryMethodCall = new BinaryMethodCall();
            return binaryMethodCall.WriteArray(uri, methodName, typeName, instArgs, args, methodSignature, callContext, properties);
        }

        internal BinaryMethodReturn binaryMethodReturn;
        internal void WriteMethodReturn()
        {
            if (binaryMethodReturn == null)
                binaryMethodReturn = new BinaryMethodReturn();
            binaryMethodReturn.Dump();
            binaryMethodReturn.Write(this);
        }

        internal Object[] WriteReturnArray(Object returnValue, Object[] args, Exception exception, Object callContext, Object[] properties)
        {
            if (binaryMethodReturn == null)
                binaryMethodReturn = new BinaryMethodReturn();
            return binaryMethodReturn.WriteArray(returnValue, args, exception, callContext, properties);
        }

        internal BinaryObject binaryObject;
        internal BinaryObjectWithMap binaryObjectWithMap;
        internal BinaryObjectWithMapTyped binaryObjectWithMapTyped;
        //internal BinaryCrossAppDomainMap crossAppDomainMap;

        internal void WriteObject(NameInfo nameInfo, NameInfo typeNameInfo, int numMembers, String[] memberNames, Type[] memberTypes, WriteObjectInfo[] memberObjectInfos)
        {
            InternalWriteItemNull();
            int assemId;
#if _DEBUG                        
            nameInfo.Dump("WriteObject nameInfo");
            typeNameInfo.Dump("WriteObject typeNameInfo");
#endif            

            int objectId = (int)nameInfo.NIobjectId;

            //if (objectId < 0)
            //  objectId = --m_nestedObjectCount;

            if (objectId > 0)
            {
                BCLDebug.Trace("BINARY", "-----Top Level Object-----");
            }

            String objectName = null;
            if (objectId < 0)
            {
                // Nested Object
                objectName = typeNameInfo.NIname;
            }
            else
            {
                // Non-Nested
                objectName = nameInfo.NIname;
            }
            SerTrace.Log( this, "WriteObject objectName ",objectName);

            if (objectMapTable == null)
            {
                objectMapTable = new Hashtable();
            }

            ObjectMapInfo objectMapInfo = (ObjectMapInfo)objectMapTable[objectName];

            if (objectMapInfo != null && objectMapInfo.isCompatible(numMembers, memberNames, memberTypes))
            {
                // Object
                if (binaryObject == null)
                    binaryObject = new BinaryObject();            
                binaryObject.Set(objectId, objectMapInfo.objectId);
#if _DEBUG                        
                binaryObject.Dump();
#endif
                binaryObject.Write(this);
            }
            else if (!typeNameInfo.NItransmitTypeOnObject)
            {

                // ObjectWithMap
                if (binaryObjectWithMap == null)
                    binaryObjectWithMap = new BinaryObjectWithMap();

                // BCL types are not placed into table
                assemId = (int)typeNameInfo.NIassemId;
                binaryObjectWithMap.Set(objectId, objectName, numMembers, memberNames, assemId);

                binaryObjectWithMap.Dump();
                binaryObjectWithMap.Write(this);
                if (objectMapInfo == null)
                    objectMapTable.Add(objectName, new ObjectMapInfo(objectId, numMembers, memberNames, memberTypes));
            }
            else
            {
                // ObjectWithMapTyped
                BinaryTypeEnum[] binaryTypeEnumA = new BinaryTypeEnum[numMembers];
                Object[] typeInformationA = new Object[numMembers];
                int[] assemIdA = new int[numMembers];
                for (int i=0; i<numMembers; i++)
                {
                    Object typeInformation = null;

                    binaryTypeEnumA[i] = BinaryConverter.GetBinaryTypeInfo(memberTypes[i], memberObjectInfos[i], null, objectWriter, out typeInformation, out assemId);
                    typeInformationA[i] = typeInformation;
                    assemIdA[i] = assemId;
                    SerTrace.Log( this, "WriteObject ObjectWithMapTyped memberNames "
                                  ,memberNames[i],", memberType ",memberTypes[i]," binaryTypeEnum ",((Enum)binaryTypeEnumA[i]).ToString()
                                  ,", typeInformation ",typeInformationA[i]," assemId ",assemIdA[i]);
                }

                if (binaryObjectWithMapTyped == null)
                    binaryObjectWithMapTyped = new BinaryObjectWithMapTyped();            

                // BCL types are not placed in table
                assemId = (int)typeNameInfo.NIassemId;
                binaryObjectWithMapTyped.Set(objectId, objectName, numMembers,memberNames, binaryTypeEnumA, typeInformationA, assemIdA, assemId);
#if _DEBUG
                binaryObjectWithMapTyped.Dump();
#endif
                binaryObjectWithMapTyped.Write(this);
                if (objectMapInfo == null)
                    objectMapTable.Add(objectName, new ObjectMapInfo(objectId, numMembers, memberNames, memberTypes));
            }
        }

        internal BinaryObjectString binaryObjectString;
        internal BinaryCrossAppDomainString binaryCrossAppDomainString;

        internal void WriteObjectString(int objectId, String value)
        {
            InternalWriteItemNull();

            if (binaryObjectString == null)
                binaryObjectString = new BinaryObjectString();            
            binaryObjectString.Set(objectId, value);
#if _DEBUG                        
            binaryObjectString.Dump();
#endif
            binaryObjectString.Write(this);
        }

        internal BinaryArray binaryArray;

        [System.Security.SecurityCritical]  // auto-generated
        internal void WriteSingleArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound, Array array)
        {
            InternalWriteItemNull();            
#if _DEBUG                        
            arrayNameInfo.Dump("WriteSingleArray arrayNameInfo");
            arrayElemTypeNameInfo.Dump("WriteSingleArray arrayElemTypeNameInfo");
#endif
            BinaryArrayTypeEnum binaryArrayTypeEnum;
            Int32[] lengthA = new Int32[1];
            lengthA[0] = length;
            Int32[] lowerBoundA = null;
            Object typeInformation = null;

            if (lowerBound == 0)
            {
                binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
            }
            else
            {
                binaryArrayTypeEnum = BinaryArrayTypeEnum.SingleOffset;
                lowerBoundA = new Int32[1];
                lowerBoundA[0] = lowerBound;
            }

            int assemId;

            BinaryTypeEnum binaryTypeEnum = BinaryConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo.NItype, objectInfo, arrayElemTypeNameInfo.NIname, objectWriter, out typeInformation, out assemId);

            if (binaryArray == null)
                binaryArray = new BinaryArray();
            binaryArray.Set((int)arrayNameInfo.NIobjectId, (int)1, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, binaryArrayTypeEnum, assemId);

            if (arrayNameInfo.NIobjectId >0)
            {
                BCLDebug.Trace("BINARY", "-----Top Level Object-----");
            }
#if _DEBUG                        
            binaryArray.Dump();
#endif
            binaryArray.Write(this);

            if (Converter.IsWriteAsByteArray(arrayElemTypeNameInfo.NIprimitiveTypeEnum) && (lowerBound == 0))
            {
                //array is written out as an array of bytes
                if (arrayElemTypeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Byte)
                    WriteBytes((Byte[])array);
                else if (arrayElemTypeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Char)
                    WriteChars((char[])array);
                else
                    WriteArrayAsBytes(array, Converter.TypeLength(arrayElemTypeNameInfo.NIprimitiveTypeEnum));
            }
        }

        byte[] byteBuffer = null;
        int chunkSize = 4096;

        [System.Security.SecurityCritical]  // auto-generated
        private void WriteArrayAsBytes(Array array, int typeLength)
        {
            InternalWriteItemNull();
            int byteLength = array.Length*typeLength;
            int arrayOffset = 0;
            if (byteBuffer == null)
                byteBuffer = new byte[chunkSize];

            while (arrayOffset < array.Length)
            {
                int numArrayItems = Math.Min(chunkSize/typeLength, array.Length-arrayOffset);
                int bufferUsed = numArrayItems*typeLength;
                Buffer.InternalBlockCopy(array, arrayOffset*typeLength, byteBuffer, 0, bufferUsed);
#if BIGENDIAN
                // we know that we are writing a primitive type, so just do a simple swap
                for (int i = 0; i < bufferUsed; i += typeLength) 
                {
                    for (int j = 0; j < typeLength / 2; j++) 
                    {
                        byte tmp = byteBuffer[i + j];
                        byteBuffer[i + j] = byteBuffer[i + typeLength-1 - j];
                        byteBuffer[i + typeLength-1 - j] = tmp;
                    }
                }
#endif
                WriteBytes(byteBuffer, 0, bufferUsed);
                arrayOffset += numArrayItems;
            }
        }


        internal void WriteJaggedArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound)
        {
#if _DEBUG                        
            arrayNameInfo.Dump("WriteRectangleArray arrayNameInfo");
            arrayElemTypeNameInfo.Dump("WriteRectangleArray arrayElemTypeNameInfo");
#endif     
            InternalWriteItemNull();
            BinaryArrayTypeEnum binaryArrayTypeEnum;
            Int32[] lengthA = new Int32[1];
            lengthA[0] = length;
            Int32[] lowerBoundA = null;
            Object typeInformation = null;
            int assemId = 0;

            if (lowerBound == 0)
            {
                binaryArrayTypeEnum = BinaryArrayTypeEnum.Jagged;
            }
            else
            {
                binaryArrayTypeEnum = BinaryArrayTypeEnum.JaggedOffset;
                lowerBoundA = new Int32[1];
                lowerBoundA[0] = lowerBound;
            }

            BinaryTypeEnum binaryTypeEnum = BinaryConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo.NItype, objectInfo, arrayElemTypeNameInfo.NIname, objectWriter, out typeInformation, out assemId);

            if (binaryArray == null)
                binaryArray = new BinaryArray();
            binaryArray.Set((int)arrayNameInfo.NIobjectId, (int)1, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, binaryArrayTypeEnum, assemId);

            if (arrayNameInfo.NIobjectId >0)
            {
                BCLDebug.Trace("BINARY", "-----Top Level Object-----");
            }
#if _DEBUG                        
            binaryArray.Dump();
#endif
            binaryArray.Write(this);
        }

        internal void WriteRectangleArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int rank, int[] lengthA, int[] lowerBoundA)
        {
#if _DEBUG                        
            arrayNameInfo.Dump("WriteRectangleArray arrayNameInfo");
            arrayElemTypeNameInfo.Dump("WriteRectangleArray arrayElemTypeNameInfo");
#endif      
            InternalWriteItemNull();

            BinaryArrayTypeEnum binaryArrayTypeEnum = BinaryArrayTypeEnum.Rectangular;
            Object typeInformation = null;
            int assemId = 0;
            BinaryTypeEnum binaryTypeEnum = BinaryConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo.NItype, objectInfo, arrayElemTypeNameInfo.NIname, objectWriter, out typeInformation, out assemId);

            if (binaryArray == null)
                binaryArray = new BinaryArray();

            for (int i=0; i<rank; i++)
            {
                if (lowerBoundA[i] != 0)
                {
                    binaryArrayTypeEnum = BinaryArrayTypeEnum.RectangularOffset;
                    break;
                }

            }

            binaryArray.Set((int)arrayNameInfo.NIobjectId, rank, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, binaryArrayTypeEnum, assemId);

            if (arrayNameInfo.NIobjectId >0)
            {
                BCLDebug.Trace("BINARY", "-----Top Level Object-----");
            }
#if _DEBUG                        
            binaryArray.Dump();
#endif
            binaryArray.Write(this);
        }


        [System.Security.SecurityCritical]  // auto-generated
        internal void WriteObjectByteArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound, Byte[] byteA)
        {
#if _DEBUG                        
            arrayNameInfo.Dump("WriteObjectByteArray arrayNameInfo");
            arrayElemTypeNameInfo.Dump("WriteObjectByteArray arrayElemTypeNameInfo");
#endif      
            InternalWriteItemNull();
            WriteSingleArray(memberNameInfo, arrayNameInfo, objectInfo, arrayElemTypeNameInfo, length, lowerBound, byteA);
        }

        internal MemberPrimitiveUnTyped memberPrimitiveUnTyped;
        internal MemberPrimitiveTyped memberPrimitiveTyped;

        internal void WriteMember(NameInfo memberNameInfo, NameInfo typeNameInfo, Object value)
        {
#if _DEBUG                        
            SerTrace.Log("BinaryWriter", "Write Member memberName ",memberNameInfo.NIname,", value ",value);
            memberNameInfo.Dump("WriteMember memberNameInfo");
            typeNameInfo.Dump("WriteMember typeNameInfo");
#endif      
            InternalWriteItemNull();
            InternalPrimitiveTypeE typeInformation = typeNameInfo.NIprimitiveTypeEnum;

            // Writes Members with primitive values

            if (memberNameInfo.NItransmitTypeOnMember)
            {
                if (memberPrimitiveTyped == null)
                    memberPrimitiveTyped = new MemberPrimitiveTyped();
                memberPrimitiveTyped.Set((InternalPrimitiveTypeE)typeInformation, value);

                if (memberNameInfo.NIisArrayItem)
                {
                    BCLDebug.Trace("BINARY",  "-----item-----");
                }
                else
                {
                    BCLDebug.Trace("BINARY","-----",memberNameInfo.NIname,"-----");
                }
                memberPrimitiveTyped.Dump();

                memberPrimitiveTyped.Write(this);
            }
            else
            {
                if (memberPrimitiveUnTyped == null)
                    memberPrimitiveUnTyped = new MemberPrimitiveUnTyped();
                memberPrimitiveUnTyped.Set(typeInformation, value);

                if (memberNameInfo.NIisArrayItem)
                {
                    BCLDebug.Trace("BINARY", "-----item-----");
                }
                else
                {
                    BCLDebug.Trace("BINARY", "-----",memberNameInfo.NIname,"-----");
                }
                memberPrimitiveUnTyped.Dump();

                memberPrimitiveUnTyped.Write(this);

            }
        }

        internal ObjectNull objectNull;


        internal void WriteNullMember(NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
#if _DEBUG                        
            typeNameInfo.Dump("WriteNullMember typeNameInfo");
#endif
            InternalWriteItemNull();
            if (objectNull == null)
                objectNull = new ObjectNull();

            if (memberNameInfo.NIisArrayItem)
            {
                BCLDebug.Trace("BINARY",  "-----item-----");
            }
            else
            {
                objectNull.SetNullCount(1);
                BCLDebug.Trace("BINARY", "-----",memberNameInfo.NIname,"-----");
                objectNull.Dump();
                objectNull.Write(this);
                nullCount = 0;
            }
        }

        internal MemberReference memberReference;

        internal void WriteMemberObjectRef(NameInfo memberNameInfo, int idRef)
        {
            InternalWriteItemNull();
            if (memberReference == null)
                memberReference = new MemberReference();
            memberReference.Set(idRef);

            if (memberNameInfo.NIisArrayItem)
            {
                BCLDebug.Trace("BINARY", "-----item-----");
            }
            else
            {
                BCLDebug.Trace("BINARY", "-----",memberNameInfo.NIname,"-----");
            }
            memberReference.Dump();

            memberReference.Write(this);
        }

        internal void WriteMemberNested(NameInfo memberNameInfo)
        {
            InternalWriteItemNull();
            if (memberNameInfo.NIisArrayItem)
            {
                BCLDebug.Trace("BINARY", "-----item-----");
            }
            else
            {
                BCLDebug.Trace("BINARY", "-----",memberNameInfo.NIname,"-----");
            }
        }

        internal void WriteMemberString(NameInfo memberNameInfo, NameInfo typeNameInfo, String value)
        {
            InternalWriteItemNull();
            if (memberNameInfo.NIisArrayItem)
            {
                BCLDebug.Trace("BINARY", "-----item-----");
            }
            else
            {
                BCLDebug.Trace("BINARY", "-----",memberNameInfo.NIname,"-----");
            }
            WriteObjectString((int)typeNameInfo.NIobjectId, value);
        }

        internal void WriteItem(NameInfo itemNameInfo, NameInfo typeNameInfo, Object value)
        {
            InternalWriteItemNull();
            WriteMember(itemNameInfo, typeNameInfo, value);
        }

        internal void WriteNullItem(NameInfo itemNameInfo, NameInfo typeNameInfo)
        {
            nullCount++;
            InternalWriteItemNull();
        }

        internal void WriteDelayedNullItem()
        {
            nullCount++;
        }

        internal void WriteItemEnd()
        {
            InternalWriteItemNull();
        }

        private void InternalWriteItemNull()
        {
            if (nullCount > 0)
            {
                if (objectNull == null)
                    objectNull = new ObjectNull();
                objectNull.SetNullCount(nullCount);
                BCLDebug.Trace("BINARY",  "-----item-----");
                objectNull.Dump();
                objectNull.Write(this);
                nullCount = 0;
            }
        }

        internal void WriteItemObjectRef(NameInfo nameInfo, int idRef)
        {
            InternalWriteItemNull();
            WriteMemberObjectRef(nameInfo, idRef);
        }


        internal BinaryAssembly binaryAssembly;
        internal BinaryCrossAppDomainAssembly crossAppDomainAssembly;

        internal void WriteAssembly(Type type, String assemblyString, int assemId, bool isNew)
        {
            SerTrace.Log( this,"WriteAssembly type ",type,", id ",assemId,", name ", assemblyString,", isNew ",isNew);
            //If the file being tested wasn't built as an assembly, then we're going to get null back
            //for the assembly name.  This is very unfortunate.
            InternalWriteItemNull();
            if (assemblyString==null)
            {
                assemblyString=String.Empty;
            }

            if (isNew)
            {
                if (binaryAssembly == null)
                    binaryAssembly = new BinaryAssembly();
                binaryAssembly.Set(assemId, assemblyString);
                binaryAssembly.Dump();
                binaryAssembly.Write(this);
            }
        }

        // Method to write a value onto a stream given its primitive type code
        internal void WriteValue(InternalPrimitiveTypeE code, Object value)
        {
            SerTrace.Log( this, "WriteValue Entry ",((Enum)code).ToString()," " , ((value==null)?"<null>":value.GetType().ToString()) , " ",value);

            switch (code)
            {
            case InternalPrimitiveTypeE.Boolean:
                WriteBoolean(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.Byte:
                WriteByte(Convert.ToByte(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.Char:
                WriteChar(Convert.ToChar(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.Double:
                WriteDouble(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.Int16:
                WriteInt16(Convert.ToInt16(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.Int32:
                WriteInt32(Convert.ToInt32(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.Int64:
                WriteInt64(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.SByte:
                WriteSByte(Convert.ToSByte(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.Single:
                WriteSingle(Convert.ToSingle(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.UInt16:
                WriteUInt16(Convert.ToUInt16(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.UInt32:
                WriteUInt32(Convert.ToUInt32(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.UInt64:
                WriteUInt64(Convert.ToUInt64(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.Decimal:
                WriteDecimal(Convert.ToDecimal(value, CultureInfo.InvariantCulture));
                break;
            case InternalPrimitiveTypeE.TimeSpan:
                WriteTimeSpan((TimeSpan)value);
                break;
            case InternalPrimitiveTypeE.DateTime:
                WriteDateTime((DateTime)value);
                break;
            default:
                throw new SerializationException(Environment.GetResourceString("Serialization_TypeCode",((Enum)code).ToString()));
            }
            SerTrace.Log( this, "Write Exit ");
        }
    }

    internal sealed class ObjectMapInfo
    {
        internal int objectId;
        int numMembers;
        String[] memberNames;
        Type[] memberTypes;

        internal ObjectMapInfo(int objectId, int numMembers, String[] memberNames, Type[] memberTypes)
        {
            this.objectId = objectId;
            this.numMembers = numMembers;
            this.memberNames = memberNames;
            this.memberTypes = memberTypes;
        }

        internal bool isCompatible(int numMembers, String[] memberNames, Type[] memberTypes)
        {
            bool result = true;
            if (this.numMembers == numMembers)
            {
                for (int i=0; i<numMembers; i++)
                {
                    if (!(this.memberNames[i].Equals(memberNames[i])))
                    {
                        result = false;
                        break;
                    }
                    if ((memberTypes != null) && (this.memberTypes[i] != memberTypes[i]))
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
                result = false;
            return result;
        }

    }
}
