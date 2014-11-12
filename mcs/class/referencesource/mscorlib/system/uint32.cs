// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  UInt32
**
**
** Purpose: This class will encapsulate an uint and 
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
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Diagnostics.Contracts;

    // * Wrapper for unsigned 32 bit integers.
    [Serializable]
    [CLSCompliant(false), System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)] 
    [System.Runtime.InteropServices.ComVisible(true)]
#if GENERICS_WORK
    public struct UInt32 : IComparable, IFormattable, IConvertible
        , IComparable<UInt32>, IEquatable<UInt32>
///     , IArithmetic<UInt32>
#else
    public struct UInt32 : IComparable, IFormattable, IConvertible
#endif
    {
        private uint m_value;

        public const uint MaxValue = (uint)0xffffffff;
        public const uint MinValue = 0U;
    
    
        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type UInt32, this method throws an ArgumentException.
        // 
        public int CompareTo(Object value) {
            if (value == null) {
                return 1;
            }
            if (value is UInt32) {
                // Need to use compare because subtraction will wrap
                // to positive for very large neg numbers, etc.
                uint i = (uint)value;
                if (m_value < i) return -1;
                if (m_value > i) return 1;
                return 0;
            }
            throw new ArgumentException(Environment.GetResourceString("Arg_MustBeUInt32"));
        }

        public int CompareTo(UInt32 value) {
            // Need to use compare because subtraction will wrap
            // to positive for very large neg numbers, etc.
            if (m_value < value) return -1;
            if (m_value > value) return 1;
            return 0;
        }
    
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif    
        public override bool Equals(Object obj) {
            if (!(obj is UInt32)) {
                return false;
            }
            return m_value == ((UInt32)obj).m_value;
        }

        public bool Equals(UInt32 obj)
        {
            return m_value == obj;
        }

        // The absolute value of the int contained.
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public override int GetHashCode() {
            return ((int) m_value);
        }
    
        // The base 10 representation of the number with no extra padding.
        [System.Security.SecuritySafeCritical]  // auto-generated
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public override String ToString() {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatUInt32(m_value, null, NumberFormatInfo.CurrentInfo);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatUInt32(m_value, null, NumberFormatInfo.GetInstance(provider));
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(String format) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatUInt32(m_value, format, NumberFormatInfo.CurrentInfo);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public String ToString(String format, IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatUInt32(m_value, format, NumberFormatInfo.GetInstance(provider));
        }

        [CLSCompliant(false)]
        public static uint Parse(String s) {
            return Number.ParseUInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }
        
        [CLSCompliant(false)]
        public static uint Parse(String s, NumberStyles style) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseUInt32(s, style, NumberFormatInfo.CurrentInfo);
        }


        [CLSCompliant(false)]
        public static uint Parse(String s, IFormatProvider provider) {
            return Number.ParseUInt32(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }

        [CLSCompliant(false)]
        public static uint Parse(String s, NumberStyles style, IFormatProvider provider) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseUInt32(s, style, NumberFormatInfo.GetInstance(provider));
        }

        [CLSCompliant(false)]
        public static bool TryParse(String s, out UInt32 result) {
            return Number.TryParseUInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        [CLSCompliant(false)]
        public static bool TryParse(String s, NumberStyles style, IFormatProvider provider, out UInt32 result) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.TryParseUInt32(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        //
        // IConvertible implementation
        // 
        
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public TypeCode GetTypeCode() {
            return TypeCode.UInt32;
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
            return m_value;
        }

        /// <internalonly/>
        long IConvertible.ToInt64(IFormatProvider provider) {
            return Convert.ToInt64(m_value);
        }

        /// <internalonly/>
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
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
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", "UInt32", "DateTime"));
        }

        /// <internalonly/>
        Object IConvertible.ToType(Type type, IFormatProvider provider) {
            return Convert.DefaultToType((IConvertible)this, type, provider);
        }

///#if GENERICS_WORK
///        //
///        // IArithmetic<UInt32> implementation
///        //
///
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.AbsoluteValue(out bool overflowed) {
///            overflowed = false;
///            return m_value;
///        }
///
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.Negate(out bool overflowed) {
///            overflowed = (m_value != 0);
///            return m_value;
///        }
///
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.Sign(out bool overflowed) {           
///            overflowed = false;
///            return (UInt32) (m_value == 0 ? 0 : 1);
///        }
///
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.Add(UInt32 addend, out bool overflowed) {
///            ulong ul = ((ulong)m_value) + addend;
///            overflowed = (ul > MaxValue);
///            return (UInt32) ul;
///        }
///
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.Subtract(UInt32 subtrahend, out bool overflowed) {
///            long l = ((long)m_value) - subtrahend;
///            overflowed = (l < MinValue);            
///            return (UInt32) l;
///        }
///
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.Multiply(UInt32 multiplier, out bool overflowed) {
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
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.Divide(UInt32 divisor, out bool overflowed) {
///            overflowed = false;
///            return (UInt32) (m_value / divisor);  // divide by zero will fail
///        }
///
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.DivideRemainder(UInt32 divisor, out UInt32 remainder, out bool overflowed) {
///            overflowed = false;
///            remainder = (UInt32) (m_value % divisor);
///            return (UInt32) (m_value / divisor);  // divide by zero will fail
///        }
///
///        /// <internalonly/>
///        UInt32 IArithmetic<UInt32>.Remainder(UInt32 divisor, out bool overflowed) {
///            overflowed = false;
///            return (UInt32) (m_value % divisor);
///        }
///
///        /// <internalonly/>
///        ArithmeticDescriptor<UInt32> IArithmetic<UInt32>.GetDescriptor() {
///            if (s_descriptor == null) {
///                s_descriptor = new UInt32ArithmeticDescriptor( ArithmeticCapabilities.One
///                                                             | ArithmeticCapabilities.Zero
///                                                             | ArithmeticCapabilities.MaxValue
///                                                             | ArithmeticCapabilities.MinValue
///                                                             | ArithmeticCapabilities.Unsigned);
///            }
///            return s_descriptor;
///        }
///
///        private static UInt32ArithmeticDescriptor s_descriptor;
/// 
///        class UInt32ArithmeticDescriptor : ArithmeticDescriptor<UInt32> {
///            public UInt32ArithmeticDescriptor(ArithmeticCapabilities capabilities) : base(capabilities) {}
///
///            public override UInt32 One {
///                get {
///                    return (UInt32) 1;
///                }
///            }
///
///            public override UInt32 Zero {
///                get {
///                    return (UInt32) 0;
///                }
///            }
///
///            public override UInt32 MinValue {
///                get {
///                    return UInt32.MinValue;
///                }
///            }
///
///            public override UInt32 MaxValue {
///                get {
///                    return UInt32.MaxValue;
///                }
///            }
///        }
///#endif // #if GENERICS_WORK
    }
}
