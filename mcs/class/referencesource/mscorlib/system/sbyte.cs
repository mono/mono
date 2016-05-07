// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SByte
**
**
** Purpose: 
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

    // A place holder class for signed bytes.
[Serializable]
[CLSCompliant(false), System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
[System.Runtime.InteropServices.ComVisible(true)]
#if GENERICS_WORK
    public struct SByte : IComparable, IFormattable, IConvertible
    , IComparable<SByte>, IEquatable<SByte>
/// , IArithmetic<SByte>
#else
    public struct SByte : IComparable, IFormattable, IConvertible
#endif
    {
        private sbyte m_value;
    
        // The maximum value that a Byte may represent: 127.
        public const sbyte MaxValue = (sbyte)0x7F;
    
        // The minimum value that a Byte may represent: -128.
        public const sbyte MinValue = unchecked((sbyte)0x80);

    
        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type SByte, this method throws an ArgumentException.
        // 
        public int CompareTo(Object obj) {
            if (obj == null) {
                return 1;
            }
            if (!(obj is SByte)) {
                throw new ArgumentException (Environment.GetResourceString("Arg_MustBeSByte"));
            }
            return m_value - ((SByte)obj).m_value;
        }

        public int CompareTo(SByte value) {
            return m_value - value;
        }
    
        // Determines whether two Byte objects are equal.
        public override bool Equals(Object obj) {
            if (!(obj is SByte)) {
                return false;
            }
            return m_value == ((SByte)obj).m_value;
        }

        [System.Runtime.Versioning.NonVersionable]
        public bool Equals(SByte obj)
        {
            return m_value == obj;
        }

        // Gets a hash code for this instance.
        public override int GetHashCode() {
            return ((int)m_value ^ (int)m_value << 8);
        }
    
            
        // Provides a string representation of a byte.
        [System.Security.SecuritySafeCritical]  // auto-generated
        public override String ToString() {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt32(m_value, null, NumberFormatInfo.CurrentInfo);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public String ToString(IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return Number.FormatInt32(m_value, null, NumberFormatInfo.GetInstance(provider));
        }
    
        public String ToString(String format) {
            Contract.Ensures(Contract.Result<String>() != null);
            return ToString(format, NumberFormatInfo.CurrentInfo);
        }

        public String ToString(String format, IFormatProvider provider) {
            Contract.Ensures(Contract.Result<String>() != null);
            return ToString(format, NumberFormatInfo.GetInstance(provider));
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        private String ToString(String format, NumberFormatInfo info) {
            Contract.Ensures(Contract.Result<String>() != null);

            if (m_value<0 && format!=null && format.Length>0 && (format[0]=='X' || format[0]=='x')) {
                uint temp = (uint)(m_value & 0x000000FF);
                return Number.FormatUInt32(temp, format, info);
            }
            return Number.FormatInt32(m_value, format, info);
        }
    
        [CLSCompliant(false)]
        public static sbyte Parse(String s) {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo);
        }
    
        [CLSCompliant(false)]
        public static sbyte Parse(String s, NumberStyles style) {
            NumberFormatInfo.ValidateParseStyleInteger(style);  
            return Parse(s, style, NumberFormatInfo.CurrentInfo);
        }

        [CLSCompliant(false)]
        public static sbyte Parse(String s, IFormatProvider provider) {
            return Parse(s, NumberStyles.Integer, NumberFormatInfo.GetInstance(provider));
        }
    
        // Parses a signed byte from a String in the given style.  If
        // a NumberFormatInfo isn't specified, the current culture's 
        // NumberFormatInfo is assumed.
        // 
        [CLSCompliant(false)]
        public static sbyte Parse(String s, NumberStyles style, IFormatProvider provider) {
            NumberFormatInfo.ValidateParseStyleInteger(style);  
            return Parse(s, style, NumberFormatInfo.GetInstance(provider));
        }
        
        private static sbyte Parse(String s, NumberStyles style, NumberFormatInfo info) {        
                
            int i = 0;
            try {
                i = Number.ParseInt32(s, style, info);
            }
            catch(OverflowException e) {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"), e);
            }

            if ((style & NumberStyles.AllowHexSpecifier) != 0) { // We are parsing a hexadecimal number
                if ((i < 0) || i > Byte.MaxValue) {
                    throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
                }
                return (sbyte)i;
            }
                        
            if (i < MinValue || i > MaxValue) throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            return (sbyte)i;
        }

        [CLSCompliant(false)]
        public static bool TryParse(String s, out SByte result) {
            return TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        [CLSCompliant(false)]
        public static bool TryParse(String s, NumberStyles style, IFormatProvider provider, out SByte result) {
            NumberFormatInfo.ValidateParseStyleInteger(style);  
            return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }
        
        private static bool TryParse(String s, NumberStyles style, NumberFormatInfo info, out SByte result) {

            result = 0;
            int i;
            if (!Number.TryParseInt32(s, style, info, out i)) {
                return false;
            }

            if ((style & NumberStyles.AllowHexSpecifier) != 0) { // We are parsing a hexadecimal number
                if ((i < 0) || i > Byte.MaxValue) {
                    return false;
                }
                result = (sbyte)i;
                return true;
            }
                    
            if (i < MinValue || i > MaxValue) {
                return false;
            }
            result = (sbyte) i;
            return true;
        }
    
        //
        // IConvertible implementation
        // 
        
        public TypeCode GetTypeCode() {
            return TypeCode.SByte;
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
            return m_value;
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
            return m_value;
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
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", "SByte", "DateTime"));
        }

        /// <internalonly/>
        Object IConvertible.ToType(Type type, IFormatProvider provider) {
            return Convert.DefaultToType((IConvertible)this, type, provider);
        }

///#if GENERICS_WORK
///        //
///        // IArithmetic<SByte> implementation
///        //
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.AbsoluteValue(out bool overflowed) {
///            overflowed = (m_value == MinValue);  // -m_value overflows
///            return (SByte) (m_value < 0 ? -m_value : m_value);
///        }
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.Negate(out bool overflowed) {
///            overflowed = (m_value == MinValue); // Negate(MinValue) overflows
///            return (SByte) (-m_value);
///        }
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.Sign(out bool overflowed) {
///            overflowed = false;
///            return (SByte) (m_value >= 0 ? (m_value == 0 ? 0 : 1) : -1);
///        }
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.Add(SByte addend, out bool overflowed) {
///            int i = ((int)m_value) + addend;
///            overflowed = (i > MaxValue || i < MinValue);
///            return (SByte) i;
///        }
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.Subtract(SByte subtrahend, out bool overflowed) {
///            int i = ((int)m_value) - subtrahend;
///            overflowed = (i > MaxValue || i < MinValue);
///            return (SByte) i;
///        }
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.Multiply(SByte multiplier, out bool overflowed) {
///            int i = ((int)m_value) * multiplier;
///            overflowed = (i > MaxValue || i < MinValue);
///            return (SByte) i;
///        }
///
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.Divide(SByte divisor, out bool overflowed) {
///            // signed integer division can overflow.  Consider the following
///            // 8-bit case: -128/-1 = 128.
///            // 128 won't fit into a signed 8-bit integer, instead you will end up
///            // with -128.
///            //
///            // Because of this corner case, we must check if the numerator
///            // is MinValue and if the denominator is -1.
///
///            overflowed = (divisor == -1 && m_value == MinValue);
///            return (SByte) unchecked(m_value / divisor);
///        }
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.DivideRemainder(SByte divisor, out SByte remainder, out bool overflowed) {
///            remainder = (SByte) (m_value % divisor);
///            overflowed = (divisor == -1 && m_value == MinValue);
///            return (SByte) unchecked(m_value / divisor);
///        }
///
///        /// <internalonly/>
///        SByte IArithmetic<SByte>.Remainder(SByte divisor, out bool overflowed) {
///            overflowed = false;
///            return (SByte) (m_value % divisor);
///        }
///
///        /// <internalonly/>
///        ArithmeticDescriptor<SByte> IArithmetic<SByte>.GetDescriptor() {
///            if (s_descriptor == null) {
///                s_descriptor = new SByteArithmeticDescriptor( ArithmeticCapabilities.One
///                                                             | ArithmeticCapabilities.Zero
///                                                             | ArithmeticCapabilities.MaxValue
///                                                             | ArithmeticCapabilities.MinValue);
///            }
///            return s_descriptor;
///        }
///
///        private static SByteArithmeticDescriptor s_descriptor;
/// 
///        class SByteArithmeticDescriptor : ArithmeticDescriptor<SByte> {
///            public SByteArithmeticDescriptor(ArithmeticCapabilities capabilities) : base(capabilities) {}
///
///            public override SByte One {
///                get {
///                    return (SByte) 1;
///                }
///            }
///
///            public override SByte Zero {
///                get {
///                    return (SByte) 0;
///                }
///            }
///
///            public override SByte MinValue {
///                get {
///                    return SByte.MinValue;
///                }
///            }
///
///            public override SByte MaxValue {
///                get {
///                    return SByte.MaxValue;
///                }
///            }
///        }
///#endif // #if GENERICS_WORK

    }
}
