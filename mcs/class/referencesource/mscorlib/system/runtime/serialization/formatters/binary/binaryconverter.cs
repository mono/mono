// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
 **
 ** Class: Converter
 **
 **
 ** Purpose: Hexify and bin.base64 conversions
 **
 **
 ===========================================================*/


namespace System.Runtime.Serialization.Formatters.Binary {

    using System.Threading;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System;
    using System.Reflection;
    using System.Globalization;
    using System.Text;
    using System.Security.Permissions;
    using System.Diagnostics.Contracts;

    sealed internal class Converter
    {
        private Converter()
        {
        }


        private static int primitiveTypeEnumLength = 17; //Number of PrimitiveTypeEnums

        // The following section are utilities to read and write XML types

        internal static InternalPrimitiveTypeE ToCode(Type type)
        {
            SerTrace.Log("Converter", "ToCode Type Entry ",type);           
            InternalPrimitiveTypeE code;

            if ((object)type != null && !type.IsPrimitive)
            {
                if (Object.ReferenceEquals(type, typeofDateTime))
                    code = InternalPrimitiveTypeE.DateTime;
                else if (Object.ReferenceEquals(type, typeofTimeSpan))
                    code = InternalPrimitiveTypeE.TimeSpan;
                else if (Object.ReferenceEquals(type, typeofDecimal))
                    code = InternalPrimitiveTypeE.Decimal;
                else
                    code = InternalPrimitiveTypeE.Invalid;
            }
            else
                code = ToPrimitiveTypeEnum(Type.GetTypeCode(type));

            SerTrace.Log("Converter", "ToCode Exit " , ((Enum)code).ToString());
            return code;
        }




        internal static bool IsWriteAsByteArray(InternalPrimitiveTypeE code)
        {
            bool isWrite = false;

            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                case InternalPrimitiveTypeE.Char:
                case InternalPrimitiveTypeE.Byte:
                case InternalPrimitiveTypeE.Double:
                case InternalPrimitiveTypeE.Int16:
                case InternalPrimitiveTypeE.Int32:
                case InternalPrimitiveTypeE.Int64:
                case InternalPrimitiveTypeE.SByte:
                case InternalPrimitiveTypeE.Single:
                case InternalPrimitiveTypeE.UInt16:
                case InternalPrimitiveTypeE.UInt32:
                case InternalPrimitiveTypeE.UInt64:
                    isWrite = true;
                    break;
            }
            return isWrite;
        }

        internal static int TypeLength(InternalPrimitiveTypeE code)
        {
            int length  = 0;

            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    length = 1;
                    break;
                case InternalPrimitiveTypeE.Char:
                    length = 2;
                    break;                  
                case InternalPrimitiveTypeE.Byte:
                    length = 1;
                    break;                  
                case InternalPrimitiveTypeE.Double:
                    length = 8;
                    break;                  
                case InternalPrimitiveTypeE.Int16:
                    length = 2;
                    break;                  
                case InternalPrimitiveTypeE.Int32:
                    length = 4;
                    break;                  
                case InternalPrimitiveTypeE.Int64:
                    length = 8;
                    break;                  
                case InternalPrimitiveTypeE.SByte:
                    length = 1;
                    break;                  
                case InternalPrimitiveTypeE.Single:
                    length = 4;
                    break;                  
                case InternalPrimitiveTypeE.UInt16:
                    length = 2;
                    break;                  
                case InternalPrimitiveTypeE.UInt32:
                    length = 4;
                    break;                  
                case InternalPrimitiveTypeE.UInt64:
                    length = 8;
                    break;                  
            }
            return length;
        }


        internal static InternalNameSpaceE GetNameSpaceEnum(InternalPrimitiveTypeE code, Type type, WriteObjectInfo objectInfo, out String typeName)
        {
            SerTrace.Log("Converter", "GetNameSpaceEnum Entry ",((Enum)code).ToString()," type ",type);                 
            InternalNameSpaceE nameSpaceEnum = InternalNameSpaceE.None;
            typeName = null;

            if (code != InternalPrimitiveTypeE.Invalid)
            {
                switch (code)
                {
                    case InternalPrimitiveTypeE.Boolean:
                    case InternalPrimitiveTypeE.Char:
                    case InternalPrimitiveTypeE.Byte:
                    case InternalPrimitiveTypeE.Double:
                    case InternalPrimitiveTypeE.Int16:
                    case InternalPrimitiveTypeE.Int32:
                    case InternalPrimitiveTypeE.Int64:
                    case InternalPrimitiveTypeE.SByte:
                    case InternalPrimitiveTypeE.Single:
                    case InternalPrimitiveTypeE.UInt16:
                    case InternalPrimitiveTypeE.UInt32:
                    case InternalPrimitiveTypeE.UInt64:
                    case InternalPrimitiveTypeE.DateTime:
                    case InternalPrimitiveTypeE.TimeSpan:
                        nameSpaceEnum = InternalNameSpaceE.XdrPrimitive;
                        typeName = "System."+ToComType(code);                       
                        break;

                    case InternalPrimitiveTypeE.Decimal:
                        nameSpaceEnum = InternalNameSpaceE.UrtSystem;
                        typeName = "System."+ToComType(code);
                        break;
                }
            }

            if ((nameSpaceEnum == InternalNameSpaceE.None) && ((object)type != null))
            {
                if (Object.ReferenceEquals(type, typeofString))
                    nameSpaceEnum = InternalNameSpaceE.XdrString;
                else
                {
                    if (objectInfo == null)
                    {
                        typeName = type.FullName;
                        if (type.Assembly == urtAssembly)
                            nameSpaceEnum = InternalNameSpaceE.UrtSystem;
                        else
                            nameSpaceEnum = InternalNameSpaceE.UrtUser;                     
                    }
                    else
                    {
                        typeName = objectInfo.GetTypeFullName();
                        if (objectInfo.GetAssemblyString().Equals(urtAssemblyString))
                            nameSpaceEnum = InternalNameSpaceE.UrtSystem;
                        else
                            nameSpaceEnum = InternalNameSpaceE.UrtUser;
                    }
                }
            }

            SerTrace.Log("Converter", "GetNameSpaceEnum Exit ", ((Enum)nameSpaceEnum).ToString()," typeName ",typeName);                                
            return nameSpaceEnum;
        }

        // Returns a COM runtime type associated with the type  code

        internal static Type ToArrayType(InternalPrimitiveTypeE code)
        {
            SerTrace.Log("Converter", "ToType Entry ", ((Enum)code).ToString());
            if (arrayTypeA == null)
                InitArrayTypeA();
            SerTrace.Log("Converter", "ToType Exit ", (((object)arrayTypeA[(int)code] == null)?"null ":arrayTypeA[(int)code].Name));                
            return arrayTypeA[(int)code];
        }


        private static volatile Type[] typeA;

        private static void InitTypeA()
        {
            Type[] typeATemp = new Type[primitiveTypeEnumLength];
            typeATemp[(int)InternalPrimitiveTypeE.Invalid] = null;
            typeATemp[(int)InternalPrimitiveTypeE.Boolean] = typeofBoolean;
            typeATemp[(int)InternalPrimitiveTypeE.Byte] = typeofByte;
            typeATemp[(int)InternalPrimitiveTypeE.Char] = typeofChar;
            typeATemp[(int)InternalPrimitiveTypeE.Decimal] = typeofDecimal;
            typeATemp[(int)InternalPrimitiveTypeE.Double] = typeofDouble;
            typeATemp[(int)InternalPrimitiveTypeE.Int16] = typeofInt16;
            typeATemp[(int)InternalPrimitiveTypeE.Int32] = typeofInt32;
            typeATemp[(int)InternalPrimitiveTypeE.Int64] = typeofInt64;
            typeATemp[(int)InternalPrimitiveTypeE.SByte] = typeofSByte;
            typeATemp[(int)InternalPrimitiveTypeE.Single] = typeofSingle;
            typeATemp[(int)InternalPrimitiveTypeE.TimeSpan] = typeofTimeSpan;
            typeATemp[(int)InternalPrimitiveTypeE.DateTime] = typeofDateTime;
            typeATemp[(int)InternalPrimitiveTypeE.UInt16] = typeofUInt16;
            typeATemp[(int)InternalPrimitiveTypeE.UInt32] = typeofUInt32;
            typeATemp[(int)InternalPrimitiveTypeE.UInt64] = typeofUInt64;
            typeA = typeATemp;
        }


        private static volatile Type[] arrayTypeA;

        private static void InitArrayTypeA()
        {
            Type[] arrayTypeATemp = new Type[primitiveTypeEnumLength];
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Invalid] = null;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Boolean] = typeofBooleanArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Byte] = typeofByteArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Char] = typeofCharArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Decimal] = typeofDecimalArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Double] = typeofDoubleArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Int16] = typeofInt16Array;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Int32] = typeofInt32Array;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Int64] = typeofInt64Array;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.SByte] = typeofSByteArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.Single] = typeofSingleArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.TimeSpan] = typeofTimeSpanArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.DateTime] = typeofDateTimeArray;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.UInt16] = typeofUInt16Array;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.UInt32] = typeofUInt32Array;
            arrayTypeATemp[(int)InternalPrimitiveTypeE.UInt64] = typeofUInt64Array;
            arrayTypeA = arrayTypeATemp;
        }


        // Returns a COM runtime type associated with the type  code

        internal static Type ToType(InternalPrimitiveTypeE code)
        {
            SerTrace.Log("Converter", "ToType Entry ", ((Enum)code).ToString());
            if (typeA == null)
                InitTypeA();
            SerTrace.Log("Converter", "ToType Exit ", (((object)typeA[(int)code] == null)?"null ":typeA[(int)code].Name));              
            return typeA[(int)code];
        }




        internal static Array CreatePrimitiveArray(InternalPrimitiveTypeE code, int length)
        {
            Array array = null;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    array = new Boolean[length];
                    break;
                case InternalPrimitiveTypeE.Byte:
                    array = new Byte[length];
                    break;
                case InternalPrimitiveTypeE.Char:
                    array = new Char[length];
                    break;
                case InternalPrimitiveTypeE.Decimal:
                    array = new Decimal[length];
                    break;
                case InternalPrimitiveTypeE.Double:
                    array = new Double[length];
                    break;
                case InternalPrimitiveTypeE.Int16:
                    array = new Int16[length];
                    break;
                case InternalPrimitiveTypeE.Int32:
                    array = new Int32[length];
                    break;
                case InternalPrimitiveTypeE.Int64:
                    array = new Int64[length];
                    break;
                case InternalPrimitiveTypeE.SByte:
                    array = new SByte[length];
                    break;
                case InternalPrimitiveTypeE.Single:
                    array = new Single[length];
                    break;
                case InternalPrimitiveTypeE.TimeSpan:
                    array = new TimeSpan[length];
                    break;
                case InternalPrimitiveTypeE.DateTime:
                    array = new DateTime[length];
                    break;
                case InternalPrimitiveTypeE.UInt16:
                    array = new UInt16[length];
                    break;
                case InternalPrimitiveTypeE.UInt32:
                    array = new UInt32[length];
                    break;
                case InternalPrimitiveTypeE.UInt64:
                    array = new UInt64[length];
                    break;
            }
            return array;
        }

        internal static bool IsPrimitiveArray(Type type, out Object typeInformation)
        {
            typeInformation = null;
            bool bIsPrimitive = true;

            if (Object.ReferenceEquals(type, typeofBooleanArray))
                typeInformation = InternalPrimitiveTypeE.Boolean;
            else if (Object.ReferenceEquals(type, typeofByteArray))
                typeInformation = InternalPrimitiveTypeE.Byte;
            else if (Object.ReferenceEquals(type, typeofCharArray))
                typeInformation = InternalPrimitiveTypeE.Char;
            else if (Object.ReferenceEquals(type, typeofDoubleArray))
                typeInformation = InternalPrimitiveTypeE.Double;
            else if (Object.ReferenceEquals(type, typeofInt16Array))
                typeInformation = InternalPrimitiveTypeE.Int16;
            else if (Object.ReferenceEquals(type, typeofInt32Array))
                typeInformation = InternalPrimitiveTypeE.Int32;
            else if (Object.ReferenceEquals(type, typeofInt64Array))
                typeInformation = InternalPrimitiveTypeE.Int64;
            else if (Object.ReferenceEquals(type, typeofSByteArray))
                typeInformation = InternalPrimitiveTypeE.SByte;
            else if (Object.ReferenceEquals(type, typeofSingleArray))
                typeInformation = InternalPrimitiveTypeE.Single;
            else if (Object.ReferenceEquals(type, typeofUInt16Array))
                typeInformation = InternalPrimitiveTypeE.UInt16;
            else if (Object.ReferenceEquals(type, typeofUInt32Array))
                typeInformation = InternalPrimitiveTypeE.UInt32;
            else if (Object.ReferenceEquals(type, typeofUInt64Array))
                typeInformation = InternalPrimitiveTypeE.UInt64;
            else
                bIsPrimitive = false;
            return bIsPrimitive;
        }


        private static volatile String[] valueA;

        private static void InitValueA()
        {
            String[] valueATemp = new String[primitiveTypeEnumLength];
            valueATemp[(int)InternalPrimitiveTypeE.Invalid] = null;
            valueATemp[(int)InternalPrimitiveTypeE.Boolean] = "Boolean";
            valueATemp[(int)InternalPrimitiveTypeE.Byte] = "Byte";
            valueATemp[(int)InternalPrimitiveTypeE.Char] = "Char";
            valueATemp[(int)InternalPrimitiveTypeE.Decimal] = "Decimal";
            valueATemp[(int)InternalPrimitiveTypeE.Double] = "Double";
            valueATemp[(int)InternalPrimitiveTypeE.Int16] = "Int16";
            valueATemp[(int)InternalPrimitiveTypeE.Int32] = "Int32";
            valueATemp[(int)InternalPrimitiveTypeE.Int64] = "Int64";
            valueATemp[(int)InternalPrimitiveTypeE.SByte] = "SByte";
            valueATemp[(int)InternalPrimitiveTypeE.Single] = "Single";
            valueATemp[(int)InternalPrimitiveTypeE.TimeSpan] = "TimeSpan";
            valueATemp[(int)InternalPrimitiveTypeE.DateTime] = "DateTime";
            valueATemp[(int)InternalPrimitiveTypeE.UInt16] = "UInt16";
            valueATemp[(int)InternalPrimitiveTypeE.UInt32] = "UInt32";
            valueATemp[(int)InternalPrimitiveTypeE.UInt64] = "UInt64";
            valueA = valueATemp;
        }

        // Returns a String containg a COM+ runtime type associated with the type code

        internal static String ToComType(InternalPrimitiveTypeE code)
        {
            SerTrace.Log("Converter", "ToComType Entry ", ((Enum)code).ToString());

            if (valueA == null)
                InitValueA();

            SerTrace.Log("Converter", "ToComType Exit ",((valueA[(int)code] == null)?"null":valueA[(int)code]));                

            return valueA[(int)code];
        }

        private static volatile TypeCode[] typeCodeA;

        private static void InitTypeCodeA()
        {
            TypeCode[] typeCodeATemp = new TypeCode[primitiveTypeEnumLength];
            typeCodeATemp[(int)InternalPrimitiveTypeE.Invalid] = TypeCode.Object;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Boolean] = TypeCode.Boolean;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Byte] = TypeCode.Byte;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Char] = TypeCode.Char;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Decimal] = TypeCode.Decimal;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Double] = TypeCode.Double;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Int16] = TypeCode.Int16;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Int32] = TypeCode.Int32;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Int64] = TypeCode.Int64;
            typeCodeATemp[(int)InternalPrimitiveTypeE.SByte] = TypeCode.SByte;
            typeCodeATemp[(int)InternalPrimitiveTypeE.Single] = TypeCode.Single;
            typeCodeATemp[(int)InternalPrimitiveTypeE.TimeSpan] = TypeCode.Object;
            typeCodeATemp[(int)InternalPrimitiveTypeE.DateTime] = TypeCode.DateTime;
            typeCodeATemp[(int)InternalPrimitiveTypeE.UInt16] = TypeCode.UInt16;
            typeCodeATemp[(int)InternalPrimitiveTypeE.UInt32] = TypeCode.UInt32;
            typeCodeATemp[(int)InternalPrimitiveTypeE.UInt64] = TypeCode.UInt64;
            typeCodeA = typeCodeATemp;
        }

        // Returns a System.TypeCode from a InternalPrimitiveTypeE
        internal static TypeCode ToTypeCode(InternalPrimitiveTypeE code)
        {
            if (typeCodeA == null)
                InitTypeCodeA();
            return typeCodeA[(int)code];
        }


        private static volatile InternalPrimitiveTypeE[] codeA;

        private static void InitCodeA()
        {
            InternalPrimitiveTypeE[] codeATemp = new InternalPrimitiveTypeE[19];
            codeATemp[(int)TypeCode.Empty] = InternalPrimitiveTypeE.Invalid;
            codeATemp[(int)TypeCode.Object] = InternalPrimitiveTypeE.Invalid;
#if !FEATURE_CORECLR
            codeATemp[(int)TypeCode.DBNull] = InternalPrimitiveTypeE.Invalid; 
#endif      
            codeATemp[(int)TypeCode.Boolean] = InternalPrimitiveTypeE.Boolean;
            codeATemp[(int)TypeCode.Char] = InternalPrimitiveTypeE.Char;
            codeATemp[(int)TypeCode.SByte] = InternalPrimitiveTypeE.SByte;
            codeATemp[(int)TypeCode.Byte] = InternalPrimitiveTypeE.Byte;
            codeATemp[(int)TypeCode.Int16] = InternalPrimitiveTypeE.Int16;
            codeATemp[(int)TypeCode.UInt16] = InternalPrimitiveTypeE.UInt16;
            codeATemp[(int)TypeCode.Int32] = InternalPrimitiveTypeE.Int32;
            codeATemp[(int)TypeCode.UInt32] = InternalPrimitiveTypeE.UInt32;
            codeATemp[(int)TypeCode.Int64] = InternalPrimitiveTypeE.Int64;
            codeATemp[(int)TypeCode.UInt64] = InternalPrimitiveTypeE.UInt64;
            codeATemp[(int)TypeCode.Single] = InternalPrimitiveTypeE.Single;
            codeATemp[(int)TypeCode.Double] = InternalPrimitiveTypeE.Double;
            codeATemp[(int)TypeCode.Decimal] = InternalPrimitiveTypeE.Decimal;
            codeATemp[(int)TypeCode.DateTime] = InternalPrimitiveTypeE.DateTime;
            codeATemp[17] = InternalPrimitiveTypeE.Invalid;
            codeATemp[(int)TypeCode.String] = InternalPrimitiveTypeE.Invalid;  
            codeA = codeATemp;                                     
        }

        // Returns a InternalPrimitiveTypeE from a System.TypeCode
        internal static InternalPrimitiveTypeE ToPrimitiveTypeEnum(TypeCode typeCode)
        {
            if (codeA == null)
                InitCodeA();
            return codeA[(int)typeCode];
        }

        // Translates a string into an Object
        internal static Object FromString(String value, InternalPrimitiveTypeE code)
        {
            Object var;
            SerTrace.Log( "Converter", "FromString Entry ",value," " , ((Enum)code).ToString());                
            // InternalPrimitiveTypeE needs to be a primitive type
            Contract.Assert((code != InternalPrimitiveTypeE.Invalid), "[Converter.FromString]!InternalPrimitiveTypeE.Invalid ");
            if (code != InternalPrimitiveTypeE.Invalid)
                var = Convert.ChangeType(value, ToTypeCode(code), CultureInfo.InvariantCulture);
            else
                var = value;
            SerTrace.Log( "Converter", "FromString Exit "+((var == null)?"null":var+" var type "+((var==null)?"<null>":var.GetType().ToString())));
            return var;
        }

        internal static Type typeofISerializable = typeof(ISerializable);
        internal static Type typeofString = typeof(String);
        internal static Type typeofConverter = typeof(Converter);
        internal static Type typeofBoolean = typeof(Boolean);
        internal static Type typeofByte = typeof(Byte);
        internal static Type typeofChar = typeof(Char);
        internal static Type typeofDecimal = typeof(Decimal);
        internal static Type typeofDouble = typeof(Double);
        internal static Type typeofInt16 = typeof(Int16);
        internal static Type typeofInt32 = typeof(Int32);
        internal static Type typeofInt64 = typeof(Int64);
        internal static Type typeofSByte = typeof(SByte);
        internal static Type typeofSingle = typeof(Single);
        internal static Type typeofTimeSpan = typeof(TimeSpan);
        internal static Type typeofDateTime = typeof(DateTime);
        internal static Type typeofUInt16 = typeof(UInt16);
        internal static Type typeofUInt32 = typeof(UInt32);
        internal static Type typeofUInt64 = typeof(UInt64);
        internal static Type typeofObject = typeof(Object);
 
        internal static Type typeofSystemVoid = typeof(void);
        internal static Assembly urtAssembly = Assembly.GetAssembly(typeofString);
        internal static String urtAssemblyString = urtAssembly.FullName;

        // Arrays
        internal static Type typeofTypeArray = typeof(System.Type[]);
        internal static Type typeofObjectArray = typeof(System.Object[]);
        internal static Type typeofStringArray = typeof(System.String[]);
        internal static Type typeofBooleanArray = typeof(Boolean[]);
        internal static Type typeofByteArray = typeof(Byte[]);
        internal static Type typeofCharArray = typeof(Char[]);
        internal static Type typeofDecimalArray = typeof(Decimal[]);
        internal static Type typeofDoubleArray = typeof(Double[]);
        internal static Type typeofInt16Array = typeof(Int16[]);
        internal static Type typeofInt32Array = typeof(Int32[]);
        internal static Type typeofInt64Array = typeof(Int64[]);
        internal static Type typeofSByteArray = typeof(SByte[]);
        internal static Type typeofSingleArray = typeof(Single[]);
        internal static Type typeofTimeSpanArray = typeof(TimeSpan[]);
        internal static Type typeofDateTimeArray = typeof(DateTime[]);
        internal static Type typeofUInt16Array = typeof(UInt16[]);
        internal static Type typeofUInt32Array = typeof(UInt32[]);
        internal static Type typeofUInt64Array = typeof(UInt64[]);
        internal static Type typeofMarshalByRefObject = typeof(System.MarshalByRefObject);
    }

}
