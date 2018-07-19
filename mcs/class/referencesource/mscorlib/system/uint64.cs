// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  UInt64
**
** Purpose: This class will encapsulate an unsigned long and 
**          provide an Object representation of it.
**
** 
===========================================================*/
namespace System {
    using System.Globalization;
    using System;
///#if GENERICS_WORK
///    using System.Numerics;
///#endif
    using System.Runtime.InteropServices;
    using System.Diagnostics.Contracts;

    // Wrapper for unsigned 64 bit integers.
[Serializable]
[CLSCompliant(false), System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
[System.Runtime.InteropServices.ComVisible(true)]
#if GENERICS_WORK
    public struct UInt64 : IComparable, IFormattable, IConvertible
        , IComparable<UInt64>, IEquatable<UInt64>
///     , IArithmetic<UInt64>
#else
    public struct UInt64 : IComparable, IFormattable, IConvertible
#endif
    {
        private ulong m_value;
    
        public const ulong MaxValue = (ulong) 0xffffffffffffffffL;
        public const ulong MinValue = 0x0;
        
        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type UInt64, this method throws an ArgumentException.
        // 
        public int CompareTo(Object value) {
            if (value == null) {
                return 1;
            }
            if (value is UInt64) {
                // Need to use compare because subtraction will wrap
                // to positive for very large neg numbers, etc.
                ulong i = (ulong)value;
                if (m_value < i) return -1;
                if (m_value > i) return 1;
                return 0;
            }
            throw new ArgumentException (Environment.GetResourceString("Arg_MustBeUInt64"));
        }

        public int CompareTo(UInt64 value) {
            // Need to use compare because subtraction will wrap
            // to positive for very large neg numbers, etc.
            if (m_value < value) return -1;
            if (m_value > value) return 1;
            return 0;
        }
    
        public override bool Equals(Object obj) {
            if (!(obj is UInt64)) {
                return false;
            }
            return m_value == ((UInt64)obj).m_value;
        }

        [System.Runtime.Versioning.NonVersionable]
        public bool Equals(UInt64 obj)
        {
            return m_value == obj;
        }

        // The value of the lower 32 bits XORed with the uppper 32 bits.
        public override int GetHashCode() {
            return ((int)m_value) ^ (int)(m_value >> 32);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public override String ToString() {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatUInt64(m_value, null, NumberFormatInfo.CurrentInfo);
        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatUInt64(m_value, null, NumberFormatInfo.GetInstance(provider));
        }        
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(String format) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatUInt64(m_value, format, NumberFormatInfo.CurrentInfo);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(String format, IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatUInt64(m_value, format, NumberFormatInfo.GetInstance(provider));
        }

        [CLSCompliant(false)]
        public static ulong Parse(String s) {
            return Number.ParseUInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }
    
        [CLSCompliant(false)]
        public static ulong Parse(String s, NumberStyles style) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseUInt64(s, style, NumberFormatInfo.CurrentInfo);
        }

        [CLSCompliant(false)]
        public static ulong Parse(string s, IFormatProvider provider) {
            return Number.ParseUInt64(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        [CLSCompliant(false)]
        public static ulong Parse(String s, NumberStyles style, IFormatProvider provider) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseUInt64(s, style, NumberFormatInfo.GetInstance(provider));
        }

        [CLSCompliant(false)]
        public static Boolean TryParse(String s, out UInt64 result) {
            return Number.TryParseUInt64(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        [CLSCompliant(false)]
        public static Boolean TryParse(String s, NumberStyles style, IFormatProvider provider, out UInt64 result) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.TryParseUInt64(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        //
        // IConvertible implementation
        // 
        
        public TypeCode GetTypeCode() {
            return TypeCode.UInt64;
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
            return Convert.ToInt64(m_value);
        }

        /// <internalonly/>
        ulong IConvertible.ToUInt64(IFormatProvider provider) {
            return m_value;
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
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", "UInt64", "DateTime"));
        }

        /// <internalonly/>
        Object IConvertible.ToType(Type type, IFormatProvider provider) {
           return Convert.DefaultToType((IConvertible)this, type, provider);
        }


///#if GENERICS_WORK
///        //
///        // IArithmetic<UInt64> implementation
///        //
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.AbsoluteValue(out bool overflowed) {
///            overflowed = false;
///            return m_value;
///        }
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.Negate(out bool overflowed) {
///            overflowed = (m_value != 0);
///            return m_value;
///        }
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.Sign(out bool overflowed) {
///            overflowed = false;
///            return (UInt64)(m_value > 0 ? 1 : 0);
///        }
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.Add(UInt64 addend, out bool overflowed) {
///            //
///            //   true arithmetic range check     =>     re-written for unsigned int
///            // -------------------------------        -------------------------------
///            // ((m_value + addend) > MaxValue)   =>   (m_value > (MaxValue - addend))
///            //
///
///            overflowed = (m_value > (MaxValue - addend));
///            return unchecked(m_value + addend);
///        }
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.Subtract(UInt64 subtrahend, out bool overflowed) {
///            //
///            //   true arithmetic range check         =>     re-written for unsigned int
///            // -------------------------------            -------------------------------
///            // ((m_value - subtrahend) < MinValue)   =>   (m_value < (MinValue + subtrahend))
///            //
///
///            overflowed = (m_value < (MinValue + subtrahend));
///            return unchecked(m_value - subtrahend);
///        }
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.Multiply(UInt64 multiplier, out bool overflowed) {
///            //
///            //   true arithmetic range check         =>     re-written for unsigned int
///            // -------------------------------            -------------------------------
///            // ((m_value * multiplier) > MaxValue)   =>   (multiplier != 0) && (m_value > (MaxValue / multiplier))
///            //
///
///            overflowed = (multiplier != 0) && (m_value > (MaxValue / multiplier));
///            return unchecked(m_value * multiplier);
///        }
///
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.Divide(UInt64 divisor, out bool overflowed) {
///            overflowed = false;
///            return checked(m_value / divisor);  // can throw DivideByZeroException
///        }
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.DivideRemainder(UInt64 divisor, out UInt64 remainder, out bool overflowed) {
///            overflowed = false;
///            remainder = (m_value % divisor);
///            return checked(m_value / divisor);  // can throw DivideByZeroException
///        }
///
///        /// <internalonly/>
///        UInt64 IArithmetic<UInt64>.Remainder(UInt64 divisor, out bool overflowed) {
///            overflowed = false;
///            return (m_value % divisor);
///        }
///
///        /// <internalonly/>
///        ArithmeticDescriptor<UInt64> IArithmetic<UInt64>.GetDescriptor() {
///            if (s_descriptor == null) {
///                s_descriptor = new UInt64ArithmeticDescriptor( ArithmeticCapabilities.One
///                                                             | ArithmeticCapabilities.Zero
///                                                             | ArithmeticCapabilities.MaxValue
///                                                             | ArithmeticCapabilities.MinValue
///                                                             | ArithmeticCapabilities.Unsigned);
///            }
///            return s_descriptor;
///        }
///
///        private static UInt64ArithmeticDescriptor s_descriptor;
/// 
///        class UInt64ArithmeticDescriptor : ArithmeticDescriptor<UInt64> {
///            public UInt64ArithmeticDescriptor(ArithmeticCapabilities capabilities) : base(capabilities) {}
///
///            public override UInt64 One {
///                get {
///                    return (UInt64) 1;
///                }
///            }
///
///            public override UInt64 Zero {
///                get {
///                    return (UInt64) 0;
///                }
///            }
///
///            public override UInt64 MinValue {
///                get {
///                    return UInt64.MinValue;
///                }
///            }
///
///            public override UInt64 MaxValue {
///                get {
///                    return UInt64.MaxValue;
///                }
///            }
///        }
///#endif // #if GENERICS_WORK
    }
}
