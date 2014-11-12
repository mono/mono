// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Int64.cs
**
**
** Purpose: This class will encapsulate a long and provide an
**          Object representation of it.
**
** 
===========================================================*/
namespace System {
    
    using System;
    using System.Globalization;
///#if GENERICS_WORK
///    using System.Numerics;
///#endif
    using System.Runtime.InteropServices;
    using System.Diagnostics.Contracts;

[Serializable]
[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
[System.Runtime.InteropServices.ComVisible(true)]
#if GENERICS_WORK
    public struct Int64 : IComparable, IFormattable, IConvertible
        , IComparable<Int64>, IEquatable<Int64>
///     , IArithmetic<Int64>
#else
    public struct Int64 : IComparable, IFormattable, IConvertible
#endif
    {
        internal long m_value;
    
        public const long MaxValue = 0x7fffffffffffffffL;
        public const long MinValue = unchecked((long)0x8000000000000000L);

        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type Int64, this method throws an ArgumentException.
        // 
        public int CompareTo(Object value) {
            if (value == null) {
                return 1;
            }
            if (value is Int64) {
                // Need to use compare because subtraction will wrap
                // to positive for very large neg numbers, etc.
                long i = (long)value;
                if (m_value < i) return -1;
                if (m_value > i) return 1;
                return 0;
            }
            throw new ArgumentException (Environment.GetResourceString("Arg_MustBeInt64"));
        }

        public int CompareTo(Int64 value) {
            // Need to use compare because subtraction will wrap
            // to positive for very large neg numbers, etc.
            if (m_value < value) return -1;
            if (m_value > value) return 1;
            return 0;
        }
    
        public override bool Equals(Object obj) {
            if (!(obj is Int64)) {
                return false;
            }
            return m_value == ((Int64)obj).m_value;
        }

        public bool Equals(Int64 obj)
        {
            return m_value == obj;
        }

        // The value of the lower 32 bits XORed with the uppper 32 bits.
        public override int GetHashCode() {
            return (unchecked((int)((long)m_value)) ^ (int)(m_value >> 32));
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override String ToString() {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt64(m_value, null, NumberFormatInfo.CurrentInfo);
        }
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt64(m_value, null, NumberFormatInfo.GetInstance(provider));
        }
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(String format) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt64(m_value, format, NumberFormatInfo.CurrentInfo);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(String format, IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt64(m_value, format, NumberFormatInfo.GetInstance(provider));
        }

        public static long Parse(String s) {
            return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }
    
        public static long Parse(String s, NumberStyles style) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseInt64(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static long Parse(String s, IFormatProvider provider) {
            return Number.ParseInt64(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }


        // Parses a long from a String in the given style.  If
        // a NumberFormatInfo isn't specified, the current culture's 
        // NumberFormatInfo is assumed.
        // 
        public static long Parse(String s, NumberStyles style, IFormatProvider provider) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseInt64(s, style, NumberFormatInfo.GetInstance(provider));
        }

        public static Boolean TryParse(String s, out Int64 result) {
            return Number.TryParseInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        public static Boolean TryParse(String s, NumberStyles style, IFormatProvider provider, out Int64 result) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.TryParseInt64(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        //
        // IConvertible implementation
        // 
        
        public TypeCode GetTypeCode() {
            return TypeCode.Int64;
        }

        /// <internalonly/>
        bool IConvertible.ToBoolean(IFormatProvider provider) {
            return Convert.ToBoolean(m_value);
        }

        /// <internalonly/>
        char IConvertible.ToChar(IFormatProvider provider) {
            return Convert.ToChar(m_value);
        }

        /// <internalonly/>
        sbyte IConvertible.ToSByte(IFormatProvider provider) {
            return Convert.ToSByte(m_value);
        }

        /// <internalonly/>
        byte IConvertible.ToByte(IFormatProvider provider) {
            return Convert.ToByte(m_value);
        }

        /// <internalonly/>
        short IConvertible.ToInt16(IFormatProvider provider) {
            return Convert.ToInt16(m_value);
        }

        /// <internalonly/>
        ushort IConvertible.ToUInt16(IFormatProvider provider) {
            return Convert.ToUInt16(m_value);
        }

        /// <internalonly/>
        int IConvertible.ToInt32(IFormatProvider provider) {
            return Convert.ToInt32(m_value);
        }

        /// <internalonly/>
        uint IConvertible.ToUInt32(IFormatProvider provider) {
            return Convert.ToUInt32(m_value);
        }

        /// <internalonly/>
        long IConvertible.ToInt64(IFormatProvider provider) {
            return m_value;
        }

        /// <internalonly/>
        ulong IConvertible.ToUInt64(IFormatProvider provider) {
            return Convert.ToUInt64(m_value);
        }

        /// <internalonly/>
        float IConvertible.ToSingle(IFormatProvider provider) {
            return Convert.ToSingle(m_value);
        }

        /// <internalonly/>
        double IConvertible.ToDouble(IFormatProvider provider) {
            return Convert.ToDouble(m_value);
        }

        /// <internalonly/>
        Decimal IConvertible.ToDecimal(IFormatProvider provider) {
            return Convert.ToDecimal(m_value);
        }

        /// <internalonly/>
        DateTime IConvertible.ToDateTime(IFormatProvider provider) {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", "Int64", "DateTime"));
        }

        /// <internalonly/>
        Object IConvertible.ToType(Type type, IFormatProvider provider) {
            return Convert.DefaultToType((IConvertible)this, type, provider);
        }

///#if GENERICS_WORK
///        //
///        // IArithmetic<Int64> implementation
///        //
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.AbsoluteValue(out bool overflowed) {
///            overflowed = (m_value == MinValue);  // -m_value overflows
///            return (m_value < 0 ? -m_value : m_value);
///        }
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.Negate(out bool overflowed) {
///            overflowed = (m_value == MinValue); // Negate(MinValue) overflows
///            return (-m_value);
///        }
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.Sign(out bool overflowed) {
///            overflowed = false;
///            return (m_value >= 0 ? (m_value == 0 ? 0 : 1) : -1);
///        }
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.Add(Int64 addend, out bool overflowed) {
///            //
///            //   true arithmetic range check       =>     re-written for signed int
///            // -------------------------------          -------------------------------
///            // (  ((m_value + addend) > MaxValue)  =>   (  (addend > 0 && m_value > MaxValue - addend) 
///            //  ||((m_value + addend) < MinValue))       ||(addend < 0 && m_value < MinValue - addend) )
///
///
///            overflowed = ((addend > 0) && (m_value > (MaxValue - addend))) ||
///                       ((addend < 0) && (m_value < (MinValue - addend)));
///            return unchecked(m_value + addend);
///        }
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.Subtract(Int64 subtrahend, out bool overflowed) {
///            //
///            //   true arithmetic range check           =>    re-written for signed int
///            // -------------------------------             -------------------------------
///            // (  ((m_value - subtrahend) > MaxValue)  =>   (  (subtrahend < 0 && m_value > MaxValue + subtrahend) 
///            //  ||((m_value - subtrahend) < MinValue))       ||(subtrahend > 0 && m_value < MinValue + subtrahend) )
///
///            overflowed = ((subtrahend < 0) && (m_value > (MaxValue + subtrahend))) ||
///                       ((subtrahend > 0) && (m_value < (MinValue + subtrahend)));
///            return unchecked(m_value - subtrahend);
///        }
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.Multiply(Int64 multiplier, out bool overflowed) {
///            overflowed = Int64MultiplyOverflowed(m_value, multiplier);
///            return unchecked(m_value * multiplier);
///        }
///
///        //
///        // Please refer to VM\jithelpers.cpp JIT_LMulOvf for more detailed information
///        //
///        // We perform this overflow check here instead of simply using a 'checked' operation
///        // as it is roughly 1,345X faster.
///        //
///        static Boolean Int64MultiplyOverflowed(Int64 val1, Int64 val2) {
///            Int64 ret;
///
///            // Remember the sign of the result
///            Int32 sign = (Int32) (Hi32Bits(val1) ^ Hi32Bits(val2));
///
///            // Convert to unsigned multiplication
///            if (val1 < 0) val1 = -val1;
///            if (val2 < 0) val2 = -val2;
///
///            // Get the upper 32 bits of the numbers
///            UInt32 val1High = Hi32Bits(val1);
///            UInt32 val2High = Hi32Bits(val2);
///
///            UInt64 valMid;
///
///            if (val1High == 0) {
///                // Compute the 'middle' bits of the long multiplication
///                valMid = Mul32x32To64(val2High, (UInt32)val1);
///            }
///            else {
///                if (val2High != 0)
///                    return true;
///                // Compute the 'middle' bits of the long multiplication
///                valMid = Mul32x32To64(val1High, (UInt32)val2);
///            }
///
///                // See if any bits after bit 32 are set
///            if (Hi32Bits((Int64)valMid) != 0)
///                return true;
///
///            ret = (Int64)  (Mul32x32To64((UInt32)val1, (UInt32)val2) + (valMid << 32));
///
///            // check for overflow
///            if (Hi32Bits(ret) < (UInt32)valMid)
///                return true;
///
///            if (sign >= 0) {
///                // have we spilled into the sign bit?
///                if (ret < 0)
///                    return true;
///            }
///            else {
///                ret = -ret;
///                // have we spilled into the sign bit?
///                if (ret > 0)
///                    return true;
///            }
///            return false;        
///        }
///
///        //
///        // helper method to get high 32-bit of 64-bit int
///        //
///        static UInt32 Hi32Bits(Int64 x) {
///            return ((UInt32)((UInt64)(x) >> 32));
///        }
///
///        //
///        // helper method to multiply two 32-bit uints
///        //
///        static UInt64 Mul32x32To64(UInt32 x, UInt32 y) {
///            return ((UInt64)(x) * (UInt64)(y));
///        }
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.Divide(Int64 divisor, out bool overflowed) {
///            // signed integer division can overflow.  Consider the following
///            // 8-bit case: -128/-1 = 128.
///            // 128 won't fit into a signed 8-bit integer, instead you will end up
///            // with -128.
///            //
///            // Because of this corner case, we must check if the numerator
///            // is MinValue and if the denominator is -1.
///
///            overflowed = (divisor == -1 && m_value == MinValue);
///
///            if (overflowed) {
///                // we special case (MinValue / (-1)) for Int32 and Int64 as
///                // unchecked still throws OverflowException when variables
///                // are used instead of constants
///                return MinValue;
///            }
///            else {
///                return unchecked(m_value / divisor);   
///            }
///        }
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.DivideRemainder(Int64 divisor, out Int64 remainder, out bool overflowed) {
///            overflowed = (divisor == -1 && m_value == MinValue);
///
///            if (overflowed) {
///                // we special case (MinValue / (-1)) for Int32 and Int64 as
///                // unchecked still throws OverflowException when variables
///                // are used instead of constants
///                remainder = 0;
///                return MinValue;
///            }
///            else {
///                remainder = (m_value % divisor);
///                return unchecked(m_value / divisor);   
///            }
///        }
///
///        /// <internalonly/>
///        Int64 IArithmetic<Int64>.Remainder(Int64 divisor, out bool overflowed) {
///            overflowed = false;
///
///            if (divisor == -1 && m_value == MinValue) {
///                // we special case (MinValue % (-1)) for Int32 and Int64 as
///                // unchecked still throws OverflowException when variables
///                // are used instead of constants
///                return 0;
///            }
///            else {
///                return (m_value % divisor);
///            }
///        }
///
///        /// <internalonly/>
///        ArithmeticDescriptor<Int64> IArithmetic<Int64>.GetDescriptor() {
///            if (s_descriptor == null) {
///                s_descriptor = new Int64ArithmeticDescriptor( ArithmeticCapabilities.One
///                                                             | ArithmeticCapabilities.Zero
///                                                             | ArithmeticCapabilities.MaxValue
///                                                             | ArithmeticCapabilities.MinValue);
///            }
///            return s_descriptor;
///        }
///
///        private static Int64ArithmeticDescriptor s_descriptor;
/// 
///        class Int64ArithmeticDescriptor : ArithmeticDescriptor<Int64> {
///            public Int64ArithmeticDescriptor(ArithmeticCapabilities capabilities) : base(capabilities) {}
///
///            public override Int64 One {
///                get {
///                    return (Int64) 1;
///                }
///            }
///
///            public override Int64 Zero {
///                get {
///                    return (Int64) 0;
///                }
///            }
///
///            public override Int64 MinValue {
///                get {
///                    return Int64.MinValue;
///                }
///            }
///
///            public override Int64 MaxValue {
///                get {
///                    return Int64.MaxValue;
///                }
///            }
///        }
///#endif // #if GENERICS_WORK

    }
}
