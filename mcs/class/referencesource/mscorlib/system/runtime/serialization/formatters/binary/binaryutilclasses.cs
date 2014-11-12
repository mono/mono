// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
 **
 ** Class: CommonClasses
 **
 **
 ** Purpose: utility classes
 **
 **
 ===========================================================*/


// All classes and methods in here are only for the internal use by the XML and Binary Formatters.
// They are public so that the XMLFormatter can address them. Eventually they will
// be signed so that they can't be used by external applications.

namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Text;
    using System.Globalization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Diagnostics;

    // The ParseRecord class holds the parsed XML information. There is a
    // ParsedRecord for each XML Element
    internal sealed class ParseRecord 
#if _DEBUG
                                        : ITrace
#endif                                        
    {
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal static int parseRecordIdCount = 1;

        internal int PRparseRecordId = 0;
#pragma warning restore 0414

        // Enums
        internal InternalParseTypeE PRparseTypeEnum = InternalParseTypeE.Empty;
        internal InternalObjectTypeE PRobjectTypeEnum = InternalObjectTypeE.Empty;
        internal InternalArrayTypeE PRarrayTypeEnum = InternalArrayTypeE.Empty;
        internal InternalMemberTypeE PRmemberTypeEnum = InternalMemberTypeE.Empty;
        internal InternalMemberValueE PRmemberValueEnum = InternalMemberValueE.Empty;
        internal InternalObjectPositionE PRobjectPositionEnum = InternalObjectPositionE.Empty;

        // Object
        internal String PRname;

        // Value
        internal String PRvalue;
        internal Object PRvarValue;

        // dt attribute
        internal String PRkeyDt;
        internal Type PRdtType;
        internal InternalPrimitiveTypeE PRdtTypeCode;
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal bool PRisVariant = false; // Used by Binary
        internal bool PRisEnum = false;
#pragma warning restore 0414

        // Object ID
        internal long PRobjectId;

        // Reference ID
        internal long PRidRef;

        // Array

        // Array Element Type
        internal String PRarrayElementTypeString;
        internal Type PRarrayElementType;
        internal bool PRisArrayVariant = false;
        internal InternalPrimitiveTypeE PRarrayElementTypeCode;

        // Parsed array information
        internal int PRrank;
        internal int[] PRlengthA;
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal int[] PRpositionA;
        internal int[] PRlowerBoundA;
        internal int[] PRupperBoundA;
#pragma warning restore 0414

        // Array map for placing array elements in array
        internal int[] PRindexMap;
        internal int PRmemberIndex;
        internal int PRlinearlength;
        internal int[] PRrectangularMap;
        internal bool   PRisLowerBound;

        // SerializedStreamHeader information
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal long PRtopId;
        internal long PRheaderId;
#pragma warning restore 0414


        // MemberInfo accumulated during parsing of members

        internal ReadObjectInfo PRobjectInfo;

        // ValueType Fixup needed
        internal bool PRisValueTypeFixup = false;

        // Created object
        internal Object PRnewObj;
        internal Object[] PRobjectA; //optimization, will contain object[]
        internal PrimitiveArray PRprimitiveArray; // for Primitive Soap arrays, optimization
        internal bool PRisRegistered; // Used when registering nested classes
        internal Object[] PRmemberData; // member data is collected here before populating
        internal SerializationInfo  PRsi;

        internal int PRnullCount; // Count of consecutive nulls within an array


        internal ParseRecord()
        {
        }


#if _DEBUG
        // Get a String describing the ParseRecord
        // ITrace
        public String Trace()
        {
            return "ParseRecord"+PRparseRecordId+" ParseType "+ ((Enum)PRparseTypeEnum).ToString() +" name "+PRname+" keyDt "+Util.PString(PRkeyDt);
        }
#endif

        // Initialize ParseRecord. Called when reusing.
        internal void Init()
        {
            // Enums
            PRparseTypeEnum = InternalParseTypeE.Empty;
            PRobjectTypeEnum = InternalObjectTypeE.Empty;
            PRarrayTypeEnum = InternalArrayTypeE.Empty;
            PRmemberTypeEnum = InternalMemberTypeE.Empty;
            PRmemberValueEnum = InternalMemberValueE.Empty;
            PRobjectPositionEnum = InternalObjectPositionE.Empty;

            // Object
            PRname = null;

            // Value
            PRvalue = null;

            // dt attribute
            PRkeyDt = null;
            PRdtType = null;
            PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            PRisEnum = false;

            // Object ID
            PRobjectId = 0;

            // Reference ID
            PRidRef = 0;

            // Array

            // Array Element Type
            PRarrayElementTypeString = null;
            PRarrayElementType = null;
            PRisArrayVariant = false;
            PRarrayElementTypeCode = InternalPrimitiveTypeE.Invalid;


            // Parsed array information
            PRrank = 0;
            PRlengthA = null;
            PRpositionA = null;
            PRlowerBoundA = null;
            PRupperBoundA = null;

            // Array map for placing array elements in array
            PRindexMap = null;
            PRmemberIndex = 0;
            PRlinearlength = 0;
            PRrectangularMap = null;
            PRisLowerBound = false;

            // SerializedStreamHeader information
            PRtopId = 0;
            PRheaderId = 0;

            // ValueType Fixup needed
            PRisValueTypeFixup = false;

            PRnewObj = null;
            PRobjectA = null;
            PRprimitiveArray = null;
            PRobjectInfo = null;
            PRisRegistered = false;
            PRmemberData = null;
            PRsi = null;

            PRnullCount = 0;
        }

#if _DEBUG        // Dump ParseRecord.
        [Conditional("SER_LOGGING")]
        internal void Dump()
        {
            SerTrace.Log("ParseRecord Dump ",PRparseRecordId);
            SerTrace.Log("Enums");
            Util.NVTrace("ParseType",((Enum)PRparseTypeEnum).ToString());
            Util.NVTrace("ObjectType",((Enum)PRobjectTypeEnum).ToString());
            Util.NVTrace("ArrayType",((Enum)PRarrayTypeEnum).ToString());
            Util.NVTrace("MemberType",((Enum)PRmemberTypeEnum).ToString());
            Util.NVTrace("MemberValue",((Enum)PRmemberValueEnum).ToString());
            Util.NVTrace("ObjectPosition",((Enum)PRobjectPositionEnum).ToString());
            SerTrace.Log("Basics");
            Util.NVTrace("Name",PRname);
            Util.NVTrace("Value ",PRvalue);
            Util.NVTrace("varValue ",PRvarValue);
            if (PRvarValue != null)
                Util.NVTrace("varValue type",PRvarValue.GetType());

            Util.NVTrace("keyDt",PRkeyDt);
            Util.NVTrace("dtType",PRdtType);
            Util.NVTrace("code",((Enum)PRdtTypeCode).ToString());
            Util.NVTrace("objectID",PRobjectId);
            Util.NVTrace("idRef",PRidRef);
            Util.NVTrace("isEnum",PRisEnum);
            SerTrace.Log("Array ");
            Util.NVTrace("arrayElementTypeString",PRarrayElementTypeString);
            Util.NVTrace("arrayElementType",PRarrayElementType);
            Util.NVTrace("arrayElementTypeCode",((Enum)PRarrayElementTypeCode).ToString());
            Util.NVTrace("isArrayVariant",PRisArrayVariant);
            Util.NVTrace("rank",PRrank);
            Util.NVTrace("dimensions", Util.PArray(PRlengthA));
            Util.NVTrace("position", Util.PArray(PRpositionA));
            Util.NVTrace("lowerBoundA", Util.PArray(PRlowerBoundA));
            Util.NVTrace("upperBoundA", Util.PArray(PRupperBoundA));
            SerTrace.Log("Header ");
            Util.NVTrace("nullCount", PRnullCount);

            SerTrace.Log("New Object");
            if (PRnewObj != null)
                Util.NVTrace("newObj", PRnewObj);
        }
#endif
    }

#if _DEBUG
    internal interface ITrace
    {
        String Trace();
    }
#endif

    // Implements a stack used for parsing
    internal sealed class SerStack
    {
        internal Object[] objects = new Object[5];
        internal String stackId;
        internal int top = -1;
// disable csharp compiler warning #0414: field assigned unused value
#pragma warning disable 0414
        internal int next = 0;
#pragma warning restore 0414

        internal SerStack()
        {
            stackId = "System";
        }

        internal SerStack(String stackId) {
            this.stackId = stackId;
        }

        // Push the object onto the stack
        internal void Push(Object obj) {
#if _DEBUG        
            SerTrace.Log(this, "Push ",stackId," ",((obj is ITrace)?((ITrace)obj).Trace():""));
#endif
            if (top == (objects.Length -1))
            {
                IncreaseCapacity();
            }
            objects[++top] = obj;
        }

        // Pop the object from the stack
        internal Object Pop() {
            if (top < 0)
                return null;

            Object obj = objects[top];
            objects[top--] = null;
#if _DEBUG        
            SerTrace.Log(this, "Pop ",stackId," ",((obj is ITrace)?((ITrace)obj).Trace():""));
#endif
            return obj;
        }

        internal void IncreaseCapacity() {
            int size = objects.Length * 2;
            Object[] newItems = new Object[size];
            Array.Copy(objects, 0, newItems, 0, objects.Length);
            objects = newItems;
        }

        // Gets the object on the top of the stack
        internal Object Peek() {
            if (top < 0)
                return null;
#if _DEBUG        
            SerTrace.Log(this, "Peek ",stackId," ",((objects[top] is ITrace)?((ITrace)objects[top]).Trace():""));
#endif
            return objects[top];
        }

        // Gets the second entry in the stack.
        internal Object PeekPeek() {
            if (top < 1)
                return null;
#if _DEBUG            
            SerTrace.Log(this, "PeekPeek ",stackId," ",((objects[top - 1] is ITrace)?((ITrace)objects[top - 1]).Trace():""));
#endif
            return objects[top - 1];
        }

        // The number of entries in the stack
        internal int Count() {
            return top + 1;
        }

        // The number of entries in the stack
        internal bool IsEmpty() {
            if (top > 0)
                return false;
            else
                return true;
        }

        [Conditional("SER_LOGGING")]
        internal void Dump()
        {
            for (int i=0; i<Count(); i++)
            {
                Object obj = objects[i];
#if _DEBUG                
                SerTrace.Log(this, "Stack Dump ",stackId," "+((obj is ITrace)?((ITrace)obj).Trace():""));
#endif
            }
        }
    }


    // Implements a Growable array

    [Serializable]
    internal sealed class SizedArray : ICloneable
    {
        internal Object[] objects = null;
        internal Object[] negObjects = null;

        internal SizedArray()
        {
            objects = new Object[16];
            negObjects = new Object[4];
        }

        internal SizedArray(int length)
        {
            objects = new Object[length];
            negObjects = new Object[length];
        }

        private SizedArray(SizedArray sizedArray)
        {
            objects = new Object[sizedArray.objects.Length];
            sizedArray.objects.CopyTo(objects, 0);
            negObjects = new Object[sizedArray.negObjects.Length];
            sizedArray.negObjects.CopyTo(negObjects, 0);
        }

        public Object Clone()
        {
            return new SizedArray(this);
        }

        internal Object this[int index]
        {
            get
            {
                if (index < 0)
                {
                    if (-index > negObjects.Length - 1)
                        return null;
                    return negObjects[-index];
                }
                else
                {
                    if (index > objects.Length - 1)
                        return null;
                    return objects[index];
                }
            }
            set
            {
                if (index < 0)
                {
                    if (-index > negObjects.Length-1 )
                    {
                        IncreaseCapacity(index);
                    }
                    negObjects[-index] = value;

                }
                else
                {
                    if (index > objects.Length-1 )
                    {
                        IncreaseCapacity(index);
                    }
                    if (objects[index] != null)
                    {
                        //Console.WriteLine("SizedArray Setting a non-zero "+index+" "+value);
                    }
                    objects[index] = value;
                }
            }
        }

        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    int size = Math.Max(negObjects.Length * 2, (-index)+1);
                    Object[] newItems = new Object[size];
                    Array.Copy(negObjects, 0, newItems, 0, negObjects.Length);
                    negObjects = newItems;
                }
                else
                {
                    int size = Math.Max(objects.Length * 2, index+1);
                    Object[] newItems = new Object[size];
                    Array.Copy(objects, 0, newItems, 0, objects.Length);
                    objects = newItems;
                }
            }
            catch (Exception)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_CorruptedStream"));
            }
        }

    }

    [Serializable]
    internal sealed class IntSizedArray : ICloneable
    {
        internal int[] objects = new int[16];
        internal int[] negObjects = new int[4];

        public IntSizedArray()
        {
        }

        private IntSizedArray(IntSizedArray sizedArray)
        {
            objects = new int[sizedArray.objects.Length];
            sizedArray.objects.CopyTo(objects, 0);
            negObjects = new int[sizedArray.negObjects.Length];
            sizedArray.negObjects.CopyTo(negObjects, 0);
        }

        public Object Clone()
        {
            return new IntSizedArray(this);
        }


        internal int this[int index]
        {
            get
            {
                if (index < 0)
                {
                    if (-index > negObjects.Length-1 )
                        return 0;
                    return negObjects[-index];
                }
                else
                {
                    if (index > objects.Length-1 )
                        return 0;
                    return objects[index];
                }
            }
            set
            {
                if (index < 0)
                {
                    if (-index > negObjects.Length-1 )
                    {
                        IncreaseCapacity(index);
                    }
                    negObjects[-index] = value;

                }
                else
                {
                    if (index > objects.Length-1 )
                    {
                        IncreaseCapacity(index);
                    }
                    objects[index] = value;
                }
            }
        }

        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    int size = Math.Max(negObjects.Length * 2, (-index)+1);
                    int[] newItems = new int[size];
                    Array.Copy(negObjects, 0, newItems, 0, negObjects.Length);
                    negObjects = newItems;
                }
                else
                {
                    int size = Math.Max(objects.Length * 2, index+1);
                    int[] newItems = new int[size];
                    Array.Copy(objects, 0, newItems, 0, objects.Length);
                    objects = newItems;
                }
            }
            catch (Exception)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_CorruptedStream"));
            }
        }
    }

    internal sealed class NameCache
    {
        static System.Collections.Concurrent.ConcurrentDictionary<string, object> ht = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();
        String name = null;

        internal Object GetCachedValue(String name)
        {
            this.name = name;
            object value;
            return ht.TryGetValue(name, out value) ? value : null;
        }

        internal void SetCachedValue(Object value)
        {
            ht[name] = value;
        }
    }

#if _DEBUG
    // Utilities
    internal static class Util
    {
        // Replaces a null string with an empty string
        internal static String PString(String value)
        {
            if (value == null)
                return "";
            else
                return value;
        }

        // Converts an object to a string and checks for nulls

        internal static String PString(Object value)
        {
            if (value == null)
                return "";
            else
                return value.ToString();
        }

        // Converts a single int array to a string

        internal static String PArray(int[] array)
        {
            if (array != null)
            {
                StringBuilder sb = new StringBuilder(10);
                sb.Append("[");
                for (int i=0; i<array.Length; i++)
                {
                    sb.Append(array[i]);
                    if (i != array.Length -1)
                        sb.Append(",");
                }
                sb.Append("]");
                return sb.ToString();
            }
            else
                return "";
        }

        // Traces an name value pair

        [Conditional("SER_LOGGING")]
        internal static void NVTrace(String name, String value)
        {
            SerTrace.Log("  "+name+((value == null)?" = null":" = "+value));
        }

        // Traces an name value pair
        [Conditional("SER_LOGGING")]
        internal static void NVTrace(String name, Object value)
        {
            SerTrace.Log("  "+name+((value == null)?" = null":" = "+value.ToString()));
        }

        // Traces an name value pair

        [Conditional("_LOGGING")]
        internal static void NVTraceI(String name, String value)
        {
            BCLDebug.Trace("Binary", "  "+name+((value == null)?" = null":" = "+value));
        }

        // Traces an name value pair
        [Conditional("_LOGGING")]
        internal static void NVTraceI(String name, Object value)
        {
            BCLDebug.Trace("Binary", "  "+name+((value == null)?" = null":" = "+value.ToString()));
        }
    }
#endif


    // Used to fixup value types. Only currently used for valuetypes which are array items.
    internal sealed class ValueFixup
    {
        internal ValueFixupEnum valueFixupEnum = ValueFixupEnum.Empty;
        internal Array arrayObj;
        internal int[] indexMap;
        internal Object header = null;
        internal Object memberObject;
        internal static volatile MemberInfo valueInfo;
        internal ReadObjectInfo objectInfo;
        internal String memberName;

        internal ValueFixup(Array arrayObj, int[] indexMap)
        {
#if _DEBUG
            SerTrace.Log(this, "Array Constructor ",arrayObj);
#endif
            valueFixupEnum = ValueFixupEnum.Array;
            this.arrayObj = arrayObj;
            this.indexMap = indexMap;
        }

        internal ValueFixup(Object memberObject, String memberName, ReadObjectInfo objectInfo)
        {
#if _DEBUG
            SerTrace.Log(this, "Member Constructor ",memberObject);
#endif
            valueFixupEnum = ValueFixupEnum.Member;
            this.memberObject = memberObject;
            this.memberName = memberName;
            this.objectInfo = objectInfo;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void Fixup(ParseRecord record, ParseRecord parent) {
            Object obj = record.PRnewObj;
#if _DEBUG
            SerTrace.Log(this, "Fixup ",obj," ",((Enum)valueFixupEnum).ToString());
#endif
            switch (valueFixupEnum)
            {
            case ValueFixupEnum.Array:
                arrayObj.SetValue(obj, indexMap);
                break;
            case ValueFixupEnum.Header:
                Type type = typeof(Header);
                if (valueInfo == null)
                {
                    MemberInfo[] valueInfos = type.GetMember("Value");
                    if (valueInfos.Length != 1)
                        throw new SerializationException(Environment.GetResourceString("Serialization_HeaderReflection",valueInfos.Length));
                    valueInfo = valueInfos[0];
                }
                FormatterServices.SerializationSetValue(valueInfo, header, obj);
                break;
            case ValueFixupEnum.Member:
                SerTrace.Log(this, "Fixup Member new object value ",obj," memberObject ",memberObject);

                if (objectInfo.isSi)
                {
                    SerTrace.Log(this, "Recording a fixup on member: ", memberName,
                                 " in object id", parent.PRobjectId, " Required Object ", record.PRobjectId);
                    objectInfo.objectManager.RecordDelayedFixup(parent.PRobjectId, memberName, record.PRobjectId);
//                          Console.WriteLine("SerializationInfo: Main Object ({0}): {1}. SubObject ({2}): {3}", parent.PRobjectId,
//                                            objectInfo.obj, record.PRobjectId, obj);
                }
                else
                {
                    MemberInfo memberInfo = objectInfo.GetMemberInfo(memberName);
                    SerTrace.Log(this, "Recording a fixup on member:", memberInfo, " in object id ",
                                 parent.PRobjectId," Required Object", record.PRobjectId);
                    if (memberInfo != null)
                        objectInfo.objectManager.RecordFixup(parent.PRobjectId, memberInfo, record.PRobjectId);
//                          Console.WriteLine("MemberFixup: Main Object({0}): {1}. SubObject({2}): {3}", parent.PRobjectId,
//                                            objectInfo.obj.GetType(), record.PRobjectId, obj.GetType());

                }
                break;
            }
        }

#if _DEBUG
        public String Trace()
        {
            return "ValueFixup"+((Enum)valueFixupEnum).ToString();
        }
#endif
    }

    // Class used to transmit Enums from the XML and Binary Formatter class to the ObjectWriter and ObjectReader class
    internal sealed class InternalFE
    {
        internal FormatterTypeStyle FEtypeFormat;
        internal FormatterAssemblyStyle FEassemblyFormat;
        internal TypeFilterLevel FEsecurityLevel;        
        internal InternalSerializerTypeE FEserializerTypeEnum;
    }


    internal sealed class NameInfo
    {
        internal String NIFullName; // Name from SerObjectInfo.GetType
        internal long NIobjectId;
        internal long NIassemId;
        internal InternalPrimitiveTypeE NIprimitiveTypeEnum = InternalPrimitiveTypeE.Invalid;
        internal Type NItype;
        internal bool NIisSealed;
        internal bool NIisArray;
        internal bool NIisArrayItem;
        internal bool NItransmitTypeOnObject;
        internal bool NItransmitTypeOnMember;
        internal bool NIisParentTypeOnObject;
        internal InternalArrayTypeE NIarrayEnum;

        internal NameInfo()
        {
        }


        internal void Init()
        {
            NIFullName = null;
            NIobjectId = 0;
            NIassemId = 0;
            NIprimitiveTypeEnum = InternalPrimitiveTypeE.Invalid;
            NItype = null;
            NIisSealed = false;
            NItransmitTypeOnObject = false;
            NItransmitTypeOnMember = false;
            NIisParentTypeOnObject = false;
            NIisArray = false;
            NIisArrayItem = false;
            NIarrayEnum = InternalArrayTypeE.Empty;
            NIsealedStatusChecked = false;
        }

#if _DEBUG
        [Conditional("SER_LOGGING")]
        internal void Dump(String value)
        {
            Util.NVTrace("name", NIFullName);
            Util.NVTrace("objectId", NIobjectId);
            Util.NVTrace("assemId", NIassemId);
            Util.NVTrace("primitiveTypeEnum", ((Enum)NIprimitiveTypeEnum).ToString());
            Util.NVTrace("type", NItype);
            Util.NVTrace("isSealed", NIisSealed);
            Util.NVTrace("transmitTypeOnObject", NItransmitTypeOnObject);
            Util.NVTrace("transmitTypeOnMember", NItransmitTypeOnMember);
            Util.NVTrace("isParentTypeOnObject", NIisParentTypeOnObject);
            Util.NVTrace("isArray", NIisArray);
            Util.NVTrace("isArrayItem", NIisArrayItem);
            Util.NVTrace("arrayEnum", ((Enum)NIarrayEnum).ToString());
        }
#endif
        private bool NIsealedStatusChecked = false;
        public bool IsSealed
        {
            get {
                if (!NIsealedStatusChecked)
                {
                    NIisSealed = NItype.IsSealed;
                    NIsealedStatusChecked = true;
                }
                return NIisSealed;
            }
        }

        public String NIname
        {
            get {
                if (this.NIFullName == null)
                    this.NIFullName = NItype.FullName;

                return this.NIFullName;
            }
            set {
                this.NIFullName = value;
            }
        }
    }

    internal sealed class PrimitiveArray
    {
        InternalPrimitiveTypeE code;
        Boolean[] booleanA = null;
        Char[] charA = null;
        Double[] doubleA = null;
        Int16[] int16A = null;
        Int32[] int32A = null;
        Int64[] int64A = null;
        SByte[] sbyteA = null;
        Single[] singleA = null;
        UInt16[] uint16A = null;
        UInt32[] uint32A = null;
        UInt64[] uint64A = null;


        internal PrimitiveArray(InternalPrimitiveTypeE code, Array array)
        {
            Init(code, array);
        }

        internal void Init(InternalPrimitiveTypeE code, Array array)
        {
            this.code = code;
            switch (code)
            {
            case InternalPrimitiveTypeE.Boolean:
                booleanA = (Boolean[])array;
                break;
            case InternalPrimitiveTypeE.Char:
                charA = (Char[])array;
                break;
            case InternalPrimitiveTypeE.Double:
                doubleA = (Double[])array;
                break;
            case InternalPrimitiveTypeE.Int16:
                int16A = (Int16[])array;
                break;
            case InternalPrimitiveTypeE.Int32:
                int32A = (Int32[])array;
                break;
            case InternalPrimitiveTypeE.Int64:
                int64A = (Int64[])array;
                break;
            case InternalPrimitiveTypeE.SByte:
                sbyteA = (SByte[])array;
                break;
            case InternalPrimitiveTypeE.Single:
                singleA = (Single[])array;
                break;
            case InternalPrimitiveTypeE.UInt16:
                uint16A = (UInt16[])array;
                break;
            case InternalPrimitiveTypeE.UInt32:
                uint32A = (UInt32[])array;
                break;
            case InternalPrimitiveTypeE.UInt64:
                uint64A = (UInt64[])array;
                break;
            }
        }
        
        internal void SetValue(String value, int index)
        {
            switch (code)
            {
            case InternalPrimitiveTypeE.Boolean:
                booleanA[index] = Boolean.Parse(value);
                break;
            case InternalPrimitiveTypeE.Char:
                if ((value[0] == '_') && (value.Equals("_0x00_")))
                    charA[index] = Char.MinValue;
                else
                    charA[index] = Char.Parse(value);
                break;
            case InternalPrimitiveTypeE.Double:
                doubleA[index] = Double.Parse(value, CultureInfo.InvariantCulture);
                break;
            case InternalPrimitiveTypeE.Int16:
                int16A[index] = Int16.Parse(value, CultureInfo.InvariantCulture);
                break;
            case InternalPrimitiveTypeE.Int32:
                int32A[index] = Int32.Parse(value, CultureInfo.InvariantCulture);
                break;
            case InternalPrimitiveTypeE.Int64:
                int64A[index] = Int64.Parse(value, CultureInfo.InvariantCulture);
                break;
            case InternalPrimitiveTypeE.SByte:
                sbyteA[index] = SByte.Parse(value, CultureInfo.InvariantCulture);
                break;
            case InternalPrimitiveTypeE.Single:
                singleA[index] = Single.Parse(value, CultureInfo.InvariantCulture);
                break;
            case InternalPrimitiveTypeE.UInt16:
                uint16A[index] = UInt16.Parse(value, CultureInfo.InvariantCulture);
                break;
            case InternalPrimitiveTypeE.UInt32:
                uint32A[index] = UInt32.Parse(value, CultureInfo.InvariantCulture);
                break;
            case InternalPrimitiveTypeE.UInt64:
                uint64A[index] = UInt64.Parse(value, CultureInfo.InvariantCulture);
                break;
            }
        }
    }
}

