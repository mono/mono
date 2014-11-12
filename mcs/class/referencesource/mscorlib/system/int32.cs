// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Int32
**
**
** Purpose: A representation of a 32 bit 2's complement 
**          integer.
**
** 
===========================================================*/
namespace System {
    
    using System;
    using System.Globalization;
///#if GENERICS_WORK
///    using System.Numerics;
///#endif
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Diagnostics.Contracts;

[Serializable]
[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)] 
[System.Runtime.InteropServices.ComVisible(true)]
#if GENERICS_WORK
    public struct Int32 : IComparable, IFormattable, IConvertible
        , IComparable<Int32>, IEquatable<Int32>
///     , IArithmetic<Int32>
#else
    public struct Int32 : IComparable, IFormattable, IConvertible
#endif
    {
        internal int m_value;
    
        public const int MaxValue = 0x7fffffff;
        public const int MinValue = unchecked((int)0x80000000);
    
        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type Int32, this method throws an ArgumentException.
        // 
        public int CompareTo(Object value) {
            if (value == null) {
                return 1;
            }
            if (value is Int32) {
                // Need to use compare because subtraction will wrap
                // to positive for very large neg numbers, etc.
                int i = (int)value;
                if (m_value < i) return -1;
                if (m_value > i) return 1;
                return 0;
            }
            throw new ArgumentException (Environment.GetResourceString("Arg_MustBeInt32"));
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public int CompareTo(int value) {
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
            if (!(obj is Int32)) {
                return false;
            }
            return m_value == ((Int32)obj).m_value;
        }

#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public bool Equals(Int32 obj)
        {
            return m_value == obj;
        }

        // The absolute value of the int contained.
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        public override int GetHashCode() {
            return m_value;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        [Pure]
        public override String ToString() {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt32(m_value, null, NumberFormatInfo.CurrentInfo);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        [Pure]
        public String ToString(String format) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt32(m_value, format, NumberFormatInfo.CurrentInfo);
        }
    
        [System.Security.SecuritySafeCritical]  // auto-generated
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        [Pure]
        public String ToString(IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt32(m_value, null, NumberFormatInfo.GetInstance(provider));
        }

        [Pure]
        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(String format, IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt32(m_value, format, NumberFormatInfo.GetInstance(provider));
        }

        [Pure]
        public static int Parse(String s) {
            return Number.ParseInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }

        [Pure]
        public static int Parse(String s, NumberStyles style) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseInt32(s, style, NumberFormatInfo.CurrentInfo);
        }

        // Parses an integer from a String in the given style.  If
        // a NumberFormatInfo isn't specified, the current culture's 
        // NumberFormatInfo is assumed.
        // 
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        [Pure]
        public static int Parse(String s, IFormatProvider provider) {
            return Number.ParseInt32(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }
    
        // Parses an integer from a String in the given style.  If
        // a NumberFormatInfo isn't specified, the current culture's 
        // NumberFormatInfo is assumed.
        // 
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        [Pure]
        public static int Parse(String s, NumberStyles style, IFormatProvider provider) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.ParseInt32(s, style, NumberFormatInfo.GetInstance(provider));
        }

        // Parses an integer from a String. Returns false rather
        // than throwing exceptin if input is invalid
        // 
        [Pure]
        public static bool TryParse(String s, out Int32 result) {
            return Number.TryParseInt32(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        // Parses an integer from a String in the given style. Returns false rather
        // than throwing exceptin if input is invalid
        // 
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        [Pure]
        public static bool TryParse(String s, NumberStyles style, IFormatProvider provider, out Int32 result) {
            NumberFormatInfo.ValidateParseStyleInteger(style);
            return Number.TryParseInt32(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        //
        // IConvertible implementation
        // 
        
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        [Pure]
        public TypeCode GetTypeCode() {
            return TypeCode.Int32;
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
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        int IConvertible.ToInt32(IFormatProvider provider) {
            return m_value;
        }

        /// <internalonly/>
        uint IConvertible.ToUInt32(IFormatProvider provider) {
            return Convert.ToUInt32(m_value);
        }

        /// <internalonly/>
#if !FEATURE_CORECLR
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
#endif
        long IConvertible.ToInt64(IFormatProvider provider) {
            return Convert.ToInt64(m_value);
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
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", "Int32", "DateTime"));
        }

        /// <internalonly/>
        Object IConvertible.ToType(Type type, IFormatProvider provider) {
            return Convert.DefaultToType((IConvertible)this, type, provider);
        }

///#if GENERICS_WORK
///        //
///        // IArithmetic<Int32> implementation
///        //
///
///        /// <internalonly/>
///        Int32 IArithmetic<Int32>.AbsoluteValue(out bool overflowed) {
///            overflowed = (m_value == MinValue);  // -m_value overflows
///            return (Int32) (m_value < 0 ? -m_value : m_value);
///        }
///
///        /// <internalonly/>
///        Int32 IArithmetic<Int32>.Negate(out bool overflowed) {
///            overflowed = (m_value == MinValue); // Negate(MinValue) overflows
///            return (Int32) (-m_value);
///        }
///
///        /// <internalonly/>
///        Int32 IArithmetic<Int32>.Sign(out bool overflowed) {
///            overflowed = false;
///            return (m_value >= 0 ? (m_value == 0 ? 0 : 1) : -1);
///        }
///
///        /// <internalonly/>
///        Int32 IArithmetic<Int32>.Add(Int32 addend, out bool overflowed) {
///            long l = ((long)m_value) + addend;
///            overflowed = (l > MaxValue || l < MinValue);
///            return (Int32) l;
///        }
///
///        /// <internalonly/>
///        Int32 IArithmetic<Int32>.Subtract(Int32 subtrahend, out bool overflowed) {
///            long l = ((long)m_value) - subtrahend;
///            overflowed = (l > MaxValue || l < MinValue);
///            return (Int32) l;
///        }
///
///        /// <internalonly/>
///        Int32 IArithmetic<Int32>.Multiply(Int32 multiplier, out bool overflowed) {
///            long l = ((long)m_value) * multiplier;
///            overflowed = (l > MaxValue || l < MinValue);
///            return (Int32) l;
///        }
///
///
///        /// <internalonly/>
///        Int32 IArithmetic<Int32>.Divide(Int32 divisor, out bool overflowed) {
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
///        Int32 IArithmetic<Int32>.DivideRemainder(Int32 divisor, out Int32 remainder, out bool overflowed) {
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
///        Int32 IArithmetic<Int32>.Remainder(Int32 divisor, out bool overflowed) {
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
///        ArithmeticDescriptor<Int32> IArithmetic<Int32>.GetDescriptor() {
///            if (s_descriptor == null) {
///                s_descriptor = new Int32ArithmeticDescriptor( ArithmeticCapabilities.One
///                                                             | ArithmeticCapabilities.Zero
///                                                             | ArithmeticCapabilities.MaxValue
///                                                             | ArithmeticCapabilities.MinValue);
///            }
///            return s_descriptor;
///        }
///
///        private static Int32ArithmeticDescriptor s_descriptor;
/// 
///        class Int32ArithmeticDescriptor : ArithmeticDescriptor<Int32> {
///            public Int32ArithmeticDescriptor(ArithmeticCapabilities capabilities) : base(capabilities) {}
///
///            public override Int32 One {
///                get {
///                    return (Int32) 1;
///                }
///            }
///
///            public override Int32 Zero {
///                get {
///                    return (Int32) 0;
///                }
///            }
///
///            public override Int32 MinValue {
///                get {
///                    return Int32.MinValue;
///                }
///            }
///
///            public override Int32 MaxValue {
///                get {
///                    return Int32.MaxValue;
///                }
///            }
///        }
///#endif // #if GENERICS_WORK

    }
}
