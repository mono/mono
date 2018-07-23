// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
    // Implements the Decimal data type. The Decimal data type can
    // represent values ranging from -79,228,162,514,264,337,593,543,950,335 to
    // 79,228,162,514,264,337,593,543,950,335 with 28 significant digits. The
    // Decimal data type is ideally suited to financial calculations that
    // require a large number of significant digits and no round-off errors.
    //
    // The finite set of values of type Decimal are of the form m
    // / 10e, where m is an integer such that
    // -296 <; m <; 296, and e is an integer
    // between 0 and 28 inclusive.
    //
    // Contrary to the float and double data types, decimal
    // fractional numbers such as 0.1 can be represented exactly in the
    // Decimal representation. In the float and double
    // representations, such numbers are often infinite fractions, making those
    // representations more prone to round-off errors.
    //
    // The Decimal class implements widening conversions from the
    // ubyte, char, short, int, and long types
    // to Decimal. These widening conversions never loose any information
    // and never throw exceptions. The Decimal class also implements
    // narrowing conversions from Decimal to ubyte, char,
    // short, int, and long. These narrowing conversions round
    // the Decimal value towards zero to the nearest integer, and then
    // converts that integer to the destination type. An OverflowException
    // is thrown if the result is not within the range of the destination type.
    //
    // The Decimal class provides a widening conversion from
    // Currency to Decimal. This widening conversion never loses any
    // information and never throws exceptions. The Currency class provides
    // a narrowing conversion from Decimal to Currency. This
    // narrowing conversion rounds the Decimal to four decimals and then
    // converts that number to a Currency. An OverflowException
    // is thrown if the result is not within the range of the Currency type.
    //
    // The Decimal class provides narrowing conversions to and from the
    // float and double types. A conversion from Decimal to
    // float or double may loose precision, but will not loose
    // information about the overall magnitude of the numeric value, and will never
    // throw an exception. A conversion from float or double to
    // Decimal throws an OverflowException if the value is not within
    // the range of the Decimal type.
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
#if !MONO
    [System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
#endif
    public partial struct Decimal : IFormattable, IComparable, IConvertible, IComparable<Decimal>, IEquatable<Decimal>, IDeserializationCallback, ISpanFormattable
    {
        // Sign mask for the flags field. A value of zero in this bit indicates a
        // positive Decimal value, and a value of one in this bit indicates a
        // negative Decimal value.
        // 
        // Look at OleAut's DECIMAL_NEG constant to check for negative values
        // in native code.
        private const uint SignMask = 0x80000000;

        // Scale mask for the flags field. This byte in the flags field contains
        // the power of 10 to divide the Decimal value by. The scale byte must
        // contain a value between 0 and 28 inclusive.
        private const uint ScaleMask = 0x00FF0000;

        // Number of bits scale is shifted by.
        private const int ScaleShift = 16;

        // Constant representing the Decimal value 0.
        public const Decimal Zero = 0m;

        // Constant representing the Decimal value 1.
        public const Decimal One = 1m;

        // Constant representing the Decimal value -1.
        public const Decimal MinusOne = -1m;

        // Constant representing the largest possible Decimal value. The value of
        // this constant is 79,228,162,514,264,337,593,543,950,335.
        public const Decimal MaxValue = 79228162514264337593543950335m;

        // Constant representing the smallest possible Decimal value. The value of
        // this constant is -79,228,162,514,264,337,593,543,950,335.
        public const Decimal MinValue = -79228162514264337593543950335m;

        private const int CurrencyScale = 4; // Divide the "Int64" representation by 1E4 to get the "true" value of the Currency.

        // The lo, mid, hi, and flags fields contain the representation of the
        // Decimal value. The lo, mid, and hi fields contain the 96-bit integer
        // part of the Decimal. Bits 0-15 (the lower word) of the flags field are
        // unused and must be zero; bits 16-23 contain must contain a value between
        // 0 and 28, indicating the power of 10 to divide the 96-bit integer part
        // by to produce the Decimal value; bits 24-30 are unused and must be zero;
        // and finally bit 31 indicates the sign of the Decimal value, 0 meaning
        // positive and 1 meaning negative.
        //
        // NOTE: Do not change the offsets of these fields. This structure maps to the OleAut DECIMAL structure
        // and can be passed as such in P/Invokes.
        [FieldOffset(0)]
        private int flags; // Do not rename (binary serialization)
        [FieldOffset(4)]
        private int hi; // Do not rename (binary serialization)
        [FieldOffset(8)]
        private int lo; // Do not rename (binary serialization)
        [FieldOffset(12)]
        private int mid; // Do not rename (binary serialization)

        // NOTE: This set of fields overlay the ones exposed to serialization (which have to be signed ints for serialization compat.)
        // The code inside Decimal was ported from C++ and expect unsigned values.
        [FieldOffset(0), NonSerialized]
        private uint uflags;
        [FieldOffset(4), NonSerialized]
        private uint uhi;
        [FieldOffset(8), NonSerialized]
        private uint ulo;
        [FieldOffset(12), NonSerialized]
        private uint umid;

        /// <summary>
        /// The low and mid fields combined in little-endian order
        /// </summary>
        [FieldOffset(8), NonSerialized]
        private ulong ulomidLE;

        // Constructs a Decimal from an integer value.
        //
        public Decimal(int value)
#if __MonoCS__
        : this()
#endif
        {
            //  JIT today can't inline methods that contains "starg" opcode.
            //  For more details, see DevDiv Bugs 81184: x86 JIT CQ: Removing the inline striction of "starg".
            int value_copy = value;
            if (value_copy >= 0)
            {
                uflags = 0;
            }
            else
            {
                uflags = SignMask;
                value_copy = -value_copy;
            }
            lo = value_copy;
            mid = 0;
            hi = 0;
        }

        // Constructs a Decimal from an unsigned integer value.
        //
        [CLSCompliant(false)]
        public Decimal(uint value)
#if __MonoCS__
        : this()
#endif
        {
            uflags = 0;
            ulo = value;
            umid = 0;
            uhi = 0;
        }

        // Constructs a Decimal from a long value.
        //
        public Decimal(long value)
#if __MonoCS__
        : this()
#endif
        {
            //  JIT today can't inline methods that contains "starg" opcode.
            //  For more details, see DevDiv Bugs 81184: x86 JIT CQ: Removing the inline striction of "starg".
            long value_copy = value;
            if (value_copy >= 0)
            {
                uflags = 0;
            }
            else
            {
                uflags = SignMask;
                value_copy = -value_copy;
            }
            Low64 = (ulong)value_copy;
            uhi = 0;
        }

        // Constructs a Decimal from an unsigned long value.
        //
        [CLSCompliant(false)]
        public Decimal(ulong value)
#if __MonoCS__
        : this()
#endif
        {
            uflags = 0;
            Low64 = value;
            uhi = 0;
        }

        // Constructs a Decimal from a float value.
        //
        public Decimal(float value)
        {
            DecCalc.VarDecFromR4(value, out this);
        }

        // Constructs a Decimal from a double value.
        //
        public Decimal(double value)
        {
            DecCalc.VarDecFromR8(value, out this);
        }

        //
        // Decimal <==> Currency conversion.
        //
        // A Currency represents a positive or negative decimal value with 4 digits past the decimal point. The actual Int64 representation used by these methods
        // is the currency value multiplied by 10,000. For example, a currency value of $12.99 would be represented by the Int64 value 129,900.
        //

        public static Decimal FromOACurrency(long cy)
        {
            Decimal d = default(Decimal);

            ulong absoluteCy; // has to be ulong to accommodate the case where cy == long.MinValue.
            if (cy < 0)
            {
                d.IsNegative = true;
                absoluteCy = (ulong)(-cy);
            }
            else
            {
                absoluteCy = (ulong)cy;
            }

            // In most cases, FromOACurrency() produces a Decimal with Scale set to 4. Unless, that is, some of the trailing digits past the decimal point are zero,
            // in which case, for compatibility with .Net, we reduce the Scale by the number of zeros. While the result is still numerically equivalent, the scale does
            // affect the ToString() value. In particular, it prevents a converted currency value of $12.95 from printing uglily as "12.9500".
            int scale = CurrencyScale;
            if (absoluteCy != 0)  // For compatibility, a currency of 0 emits the Decimal "0.0000" (scale set to 4).
            {
                while (scale != 0 && ((absoluteCy % 10) == 0))
                {
                    scale--;
                    absoluteCy /= 10;
                }
            }

            // No need to set d.Hi32 - a currency will never go high enough for it to be anything other than zero.
            d.Low64 = absoluteCy;
            d.Scale = scale;
            return d;
        }

        public static long ToOACurrency(Decimal value)
        {
            return DecCalc.VarCyFromDec(ref value);
        }

        private static bool IsValid(uint flags) => (flags & ~(SignMask | ScaleMask)) == 0 && ((flags & ScaleMask) <= (28 << 16));

        // Constructs a Decimal from an integer array containing a binary
        // representation. The bits argument must be a non-null integer
        // array with four elements. bits[0], bits[1], and
        // bits[2] contain the low, middle, and high 32 bits of the 96-bit
        // integer part of the Decimal. bits[3] contains the scale factor
        // and sign of the Decimal: bits 0-15 (the lower word) are unused and must
        // be zero; bits 16-23 must contain a value between 0 and 28, indicating
        // the power of 10 to divide the 96-bit integer part by to produce the
        // Decimal value; bits 24-30 are unused and must be zero; and finally bit
        // 31 indicates the sign of the Decimal value, 0 meaning positive and 1
        // meaning negative.
        //
        // Note that there are several possible binary representations for the
        // same numeric value. For example, the value 1 can be represented as {1,
        // 0, 0, 0} (integer value 1 with a scale factor of 0) and equally well as
        // {1000, 0, 0, 0x30000} (integer value 1000 with a scale factor of 3).
        // The possible binary representations of a particular value are all
        // equally valid, and all are numerically equivalent.
        //
        public Decimal(int[] bits)
#if __MonoCS__
        : this()
#endif
        {
            if (bits == null)
                throw new ArgumentNullException(nameof(bits));
            if (bits.Length == 4)
            {
                uint f = (uint)bits[3];
                if (IsValid(f))
                {
                    lo = bits[0];
                    mid = bits[1];
                    hi = bits[2];
                    uflags = f;
                    return;
                }
            }
            throw new ArgumentException(SR.Arg_DecBitCtor);
        }

        // Constructs a Decimal from its constituent parts.
        // 
        public Decimal(int lo, int mid, int hi, bool isNegative, byte scale)
#if __MonoCS__
        : this()
#endif
        {
            if (scale > 28)
                throw new ArgumentOutOfRangeException(nameof(scale), SR.ArgumentOutOfRange_DecimalScale);
            this.lo = lo;
            this.mid = mid;
            this.hi = hi;
            uflags = ((uint)scale) << 16;
            if (isNegative)
                uflags |= SignMask;
        }

        void IDeserializationCallback.OnDeserialization(Object sender)
        {
            // OnDeserialization is called after each instance of this class is deserialized.
            // This callback method performs decimal validation after being deserialized.
            if (!IsValid(uflags))
                throw new SerializationException(SR.Overflow_Decimal);
        }

        // Constructs a Decimal from its constituent parts.
        private Decimal(ulong ulomidLE, uint hi, uint flags)
#if __MonoCS__
        : this()
#endif
        {
            this.ulomidLE = ulomidLE;
            this.uhi = hi;
            this.uflags = flags;
        }

        // Returns the absolute value of the given Decimal. If d is
        // positive, the result is d. If d is negative, the result
        // is -d.
        //
        internal static Decimal Abs(ref Decimal d)
        {
            return new Decimal(d.ulomidLE, d.uhi, d.uflags & ~SignMask);
        }


        // Adds two Decimal values.
        //
        public static Decimal Add(Decimal d1, Decimal d2)
        {
            DecCalc.VarDecAdd(ref d1, ref d2);
            return d1;
        }


        // Rounds a Decimal to an integer value. The Decimal argument is rounded
        // towards positive infinity.
        public static Decimal Ceiling(Decimal d)
        {
            uint flags = d.uflags;
            if ((flags & ScaleMask) != 0)
                DecCalc.InternalRound(ref d, (byte)(flags >> ScaleShift), DecCalc.RoundingMode.Ceiling);
            return d;
        }

        // Compares two Decimal values, returning an integer that indicates their
        // relationship.
        //
        public static int Compare(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecCmp(ref d1, ref d2);
        }

        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type Decimal, this method throws an ArgumentException.
        // 
        public int CompareTo(Object value)
        {
            if (value == null)
                return 1;
            if (!(value is Decimal))
                throw new ArgumentException(SR.Arg_MustBeDecimal);

            Decimal other = (Decimal)value;
            return DecCalc.VarDecCmp(ref this, ref other);
        }

        public int CompareTo(Decimal value)
        {
            return DecCalc.VarDecCmp(ref this, ref value);
        }

        // Divides two Decimal values.
        //
        public static Decimal Divide(Decimal d1, Decimal d2)
        {
            DecCalc.VarDecDiv(ref d1, ref d2);
            return d1;
        }

        // Checks if this Decimal is equal to a given object. Returns true
        // if the given object is a boxed Decimal and its value is equal to the
        // value of this Decimal. Returns false otherwise.
        //
        public override bool Equals(Object value)
        {
            if (value is Decimal)
            {
                Decimal other = (Decimal)value;
                return DecCalc.VarDecCmp(ref this, ref other) == 0;
            }
            return false;
        }

        public bool Equals(Decimal value)
        {
            return DecCalc.VarDecCmp(ref this, ref value) == 0;
        }

        // Returns the hash code for this Decimal.
        //
        public unsafe override int GetHashCode()
        {
            double dbl = DecCalc.VarR8FromDec(ref this);
            if (dbl == 0.0)
                // Ensure 0 and -0 have the same hash code
                return 0;

            // conversion to double is lossy and produces rounding errors so we mask off the lowest 4 bits
            // 
            // For example these two numerically equal decimals with different internal representations produce
            // slightly different results when converted to double:
            //
            // decimal a = new decimal(new int[] { 0x76969696, 0x2fdd49fa, 0x409783ff, 0x00160000 });
            //                     => (decimal)1999021.176470588235294117647000000000 => (double)1999021.176470588
            // decimal b = new decimal(new int[] { 0x3f0f0f0f, 0x1e62edcc, 0x06758d33, 0x00150000 }); 
            //                     => (decimal)1999021.176470588235294117647000000000 => (double)1999021.1764705882
            //
            return (int)(((((uint*)&dbl)[0]) & 0xFFFFFFF0) ^ ((uint*)&dbl)[1]);
        }

        // Compares two Decimal values for equality. Returns true if the two
        // Decimal values are equal, or false if they are not equal.
        //
        public static bool Equals(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecCmp(ref d1, ref d2) == 0;
        }

        // Rounds a Decimal to an integer value. The Decimal argument is rounded
        // towards negative infinity.
        //
        public static Decimal Floor(Decimal d)
        {
            uint flags = d.uflags;
            if ((flags & ScaleMask) != 0)
                DecCalc.InternalRound(ref d, (byte)(flags >> ScaleShift), DecCalc.RoundingMode.Floor);
            return d;
        }

        // Converts this Decimal to a string. The resulting string consists of an
        // optional minus sign ("-") followed to a sequence of digits ("0" - "9"),
        // optionally followed by a decimal point (".") and another sequence of
        // digits.
        //
        public override String ToString()
        {
            return Number.FormatDecimal(this, null, NumberFormatInfo.CurrentInfo);
        }

        public String ToString(String format)
        {
            return Number.FormatDecimal(this, format, NumberFormatInfo.CurrentInfo);
        }

        public String ToString(IFormatProvider provider)
        {
            return Number.FormatDecimal(this, null, NumberFormatInfo.GetInstance(provider));
        }

        public String ToString(String format, IFormatProvider provider)
        {
            return Number.FormatDecimal(this, format, NumberFormatInfo.GetInstance(provider));
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider provider = null)
        {
            return Number.TryFormatDecimal(this, format, NumberFormatInfo.GetInstance(provider), destination, out charsWritten);
        }


        // Converts a string to a Decimal. The string must consist of an optional
        // minus sign ("-") followed by a sequence of digits ("0" - "9"). The
        // sequence of digits may optionally contain a single decimal point (".")
        // character. Leading and trailing whitespace characters are allowed.
        // Parse also allows a currency symbol, a trailing negative sign, and
        // parentheses in the number.
        //
        public static Decimal Parse(String s)
        {
            if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
            return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo);
        }

        internal const NumberStyles InvalidNumberStyles = ~(NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
                                                           | NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign
                                                           | NumberStyles.AllowParentheses | NumberStyles.AllowDecimalPoint
                                                           | NumberStyles.AllowThousands | NumberStyles.AllowExponent
                                                           | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowHexSpecifier);

        internal static void ValidateParseStyleFloatingPoint(NumberStyles style)
        {
            // Check for undefined flags
            if ((style & InvalidNumberStyles) != 0)
            {
                throw new ArgumentException(SR.Argument_InvalidNumberStyles, nameof(style));
            }
            if ((style & NumberStyles.AllowHexSpecifier) != 0)
            { // Check for hex number
                throw new ArgumentException(SR.Arg_HexStyleNotSupported);
            }
        }

        public static Decimal Parse(String s, NumberStyles style)
        {
            ValidateParseStyleFloatingPoint(style);
            if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
            return Number.ParseDecimal(s, style, NumberFormatInfo.CurrentInfo);
        }

        public static Decimal Parse(String s, IFormatProvider provider)
        {
            if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
            return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.GetInstance(provider));
        }

        public static Decimal Parse(String s, NumberStyles style, IFormatProvider provider)
        {
            ValidateParseStyleFloatingPoint(style);
            if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
            return Number.ParseDecimal(s, style, NumberFormatInfo.GetInstance(provider));
        }

        public static Decimal Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider provider = null)
        {
            ValidateParseStyleFloatingPoint(style);
            return Number.ParseDecimal(s, style, NumberFormatInfo.GetInstance(provider));
        }

        public static Boolean TryParse(String s, out Decimal result)
        {
            if (s == null)
            {
                result = 0;
                return false;
            }

            return Number.TryParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out result);
        }

        public static bool TryParse(ReadOnlySpan<char> s, out decimal result)
        {
            return Number.TryParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out result);
        }

        public static Boolean TryParse(String s, NumberStyles style, IFormatProvider provider, out Decimal result)
        {
            ValidateParseStyleFloatingPoint(style);
            if (s == null)
            {
                result = 0;
                return false;
            }
            return Number.TryParseDecimal(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider provider, out decimal result)
        {
            ValidateParseStyleFloatingPoint(style);
            return Number.TryParseDecimal(s, style, NumberFormatInfo.GetInstance(provider), out result);
        }

        // Returns a binary representation of a Decimal. The return value is an
        // integer array with four elements. Elements 0, 1, and 2 contain the low,
        // middle, and high 32 bits of the 96-bit integer part of the Decimal.
        // Element 3 contains the scale factor and sign of the Decimal: bits 0-15
        // (the lower word) are unused; bits 16-23 contain a value between 0 and
        // 28, indicating the power of 10 to divide the 96-bit integer part by to
        // produce the Decimal value; bits 24-30 are unused; and finally bit 31
        // indicates the sign of the Decimal value, 0 meaning positive and 1
        // meaning negative.
        //
        public static int[] GetBits(Decimal d)
        {
            return new int[] { d.lo, d.mid, d.hi, d.flags };
        }

        internal static void GetBytes(Decimal d, byte[] buffer)
        {
            Debug.Assert((buffer != null && buffer.Length >= 16), "[GetBytes]buffer != null && buffer.Length >= 16");
            buffer[0] = (byte)d.lo;
            buffer[1] = (byte)(d.lo >> 8);
            buffer[2] = (byte)(d.lo >> 16);
            buffer[3] = (byte)(d.lo >> 24);

            buffer[4] = (byte)d.mid;
            buffer[5] = (byte)(d.mid >> 8);
            buffer[6] = (byte)(d.mid >> 16);
            buffer[7] = (byte)(d.mid >> 24);

            buffer[8] = (byte)d.hi;
            buffer[9] = (byte)(d.hi >> 8);
            buffer[10] = (byte)(d.hi >> 16);
            buffer[11] = (byte)(d.hi >> 24);

            buffer[12] = (byte)d.flags;
            buffer[13] = (byte)(d.flags >> 8);
            buffer[14] = (byte)(d.flags >> 16);
            buffer[15] = (byte)(d.flags >> 24);
        }

        // Returns the larger of two Decimal values.
        //
        internal static ref Decimal Max(ref Decimal d1, ref Decimal d2)
        {
            return ref DecCalc.VarDecCmp(ref d1, ref d2) >= 0 ? ref d1 : ref d2;
        }

        // Returns the smaller of two Decimal values.
        //
        internal static ref Decimal Min(ref Decimal d1, ref Decimal d2)
        {
            return ref DecCalc.VarDecCmp(ref d1, ref d2) < 0 ? ref d1 : ref d2;
        }


        public static Decimal Remainder(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecMod(ref d1, ref d2);
        }

        // Multiplies two Decimal values.
        //
        public static Decimal Multiply(Decimal d1, Decimal d2)
        {
            DecCalc.VarDecMul(ref d1, ref d2);
            return d1;
        }

        // Returns the negated value of the given Decimal. If d is non-zero,
        // the result is -d. If d is zero, the result is zero.
        //
        public static Decimal Negate(Decimal d)
        {
            return new Decimal(d.ulomidLE, d.uhi, d.uflags ^ SignMask);
        }

        // Rounds a Decimal value to a given number of decimal places. The value
        // given by d is rounded to the number of decimal places given by
        // decimals. The decimals argument must be an integer between
        // 0 and 28 inclusive.
        //
        // By default a mid-point value is rounded to the nearest even number. If the mode is
        // passed in, it can also round away from zero.

        public static Decimal Round(Decimal d) => Round(ref d, 0, MidpointRounding.ToEven);
        public static Decimal Round(Decimal d, int decimals) => Round(ref d, decimals, MidpointRounding.ToEven);
        public static Decimal Round(Decimal d, MidpointRounding mode) => Round(ref d, 0, mode);
        public static Decimal Round(Decimal d, int decimals, MidpointRounding mode) => Round(ref d, decimals, mode);

        private static Decimal Round(ref Decimal d, int decimals, MidpointRounding mode)
        {
            if ((uint)decimals > 28)
                throw new ArgumentOutOfRangeException(nameof(decimals), SR.ArgumentOutOfRange_DecimalRound);
            if ((uint)mode > (uint)MidpointRounding.AwayFromZero)
                throw new ArgumentException(SR.Format(SR.Argument_InvalidEnumValue, mode, nameof(MidpointRounding)), nameof(mode));

            int scale = d.Scale - decimals;
            if (scale > 0)
                DecCalc.InternalRound(ref d, (uint)scale, (DecCalc.RoundingMode)mode);
            return d;
        }

        internal static int Sign(ref decimal d) => (d.lo | d.mid | d.hi) == 0 ? 0 : (d.flags >> 31) | 1;

        // Subtracts two Decimal values.
        //
        public static Decimal Subtract(Decimal d1, Decimal d2)
        {
            DecCalc.VarDecSub(ref d1, ref d2);
            return d1;
        }

        // Converts a Decimal to an unsigned byte. The Decimal value is rounded
        // towards zero to the nearest integer value, and the result of this
        // operation is returned as a byte.
        //
        public static byte ToByte(Decimal value)
        {
            uint temp;
            try
            {
                temp = ToUInt32(value);
            }
            catch (OverflowException e)
            {
                throw new OverflowException(SR.Overflow_Byte, e);
            }
            if (temp != (byte)temp) throw new OverflowException(SR.Overflow_Byte);
            return (byte)temp;
        }

        // Converts a Decimal to a signed byte. The Decimal value is rounded
        // towards zero to the nearest integer value, and the result of this
        // operation is returned as a byte.
        //
        [CLSCompliant(false)]
        public static sbyte ToSByte(Decimal value)
        {
            int temp;
            try
            {
                temp = ToInt32(value);
            }
            catch (OverflowException e)
            {
                throw new OverflowException(SR.Overflow_SByte, e);
            }
            if (temp != (sbyte)temp) throw new OverflowException(SR.Overflow_SByte);
            return (sbyte)temp;
        }

        // Converts a Decimal to a short. The Decimal value is
        // rounded towards zero to the nearest integer value, and the result of
        // this operation is returned as a short.
        //
        public static short ToInt16(Decimal value)
        {
            int temp;
            try
            {
                temp = ToInt32(value);
            }
            catch (OverflowException e)
            {
                throw new OverflowException(SR.Overflow_Int16, e);
            }
            if (temp != (short)temp) throw new OverflowException(SR.Overflow_Int16);
            return (short)temp;
        }

        // Converts a Decimal to a double. Since a double has fewer significant
        // digits than a Decimal, this operation may produce round-off errors.
        //
        public static double ToDouble(Decimal d)
        {
            return DecCalc.VarR8FromDec(ref d);
        }

        // Converts a Decimal to an integer. The Decimal value is rounded towards
        // zero to the nearest integer value, and the result of this operation is
        // returned as an integer.
        //
        public static int ToInt32(Decimal d)
        {
            Truncate(ref d);
            if ((d.hi | d.mid) == 0)
            {
                int i = d.lo;
                if (!d.IsNegative)
                {
                    if (i >= 0) return i;
                }
                else
                {
                    i = -i;
                    if (i <= 0) return i;
                }
            }
            throw new OverflowException(SR.Overflow_Int32);
        }

        // Converts a Decimal to a long. The Decimal value is rounded towards zero
        // to the nearest integer value, and the result of this operation is
        // returned as a long.
        //
        public static long ToInt64(Decimal d)
        {
            Truncate(ref d);
            if (d.uhi == 0)
            {
                long l = (long)d.Low64;
                if (!d.IsNegative)
                {
                    if (l >= 0) return l;
                }
                else
                {
                    l = -l;
                    if (l <= 0) return l;
                }
            }
            throw new OverflowException(SR.Overflow_Int64);
        }

        // Converts a Decimal to an ushort. The Decimal 
        // value is rounded towards zero to the nearest integer value, and the 
        // result of this operation is returned as an ushort.
        //
        [CLSCompliant(false)]
        public static ushort ToUInt16(Decimal value)
        {
            uint temp;
            try
            {
                temp = ToUInt32(value);
            }
            catch (OverflowException e)
            {
                throw new OverflowException(SR.Overflow_UInt16, e);
            }
            if (temp != (ushort)temp) throw new OverflowException(SR.Overflow_UInt16);
            return (ushort)temp;
        }

        // Converts a Decimal to an unsigned integer. The Decimal 
        // value is rounded towards zero to the nearest integer value, and the 
        // result of this operation is returned as an unsigned integer.
        //
        [CLSCompliant(false)]
        public static uint ToUInt32(Decimal d)
        {
            Truncate(ref d);
            if ((d.uhi | d.umid) == 0)
            {
                uint i = d.ulo;
                if (!d.IsNegative || i == 0)
                    return i;
            }
            throw new OverflowException(SR.Overflow_UInt32);
        }

        // Converts a Decimal to an unsigned long. The Decimal 
        // value is rounded towards zero to the nearest integer value, and the 
        // result of this operation is returned as a long.
        //
        [CLSCompliant(false)]
        public static ulong ToUInt64(Decimal d)
        {
            Truncate(ref d);
            if (d.uhi == 0)
            {
                ulong l = d.Low64;
                if (!d.IsNegative || l == 0)
                    return l;
            }
            throw new OverflowException(SR.Overflow_UInt64);
        }

        // Converts a Decimal to a float. Since a float has fewer significant
        // digits than a Decimal, this operation may produce round-off errors.
        //
        public static float ToSingle(Decimal d)
        {
            return DecCalc.VarR4FromDec(ref d);
        }

        // Truncates a Decimal to an integer value. The Decimal argument is rounded
        // towards zero to the nearest integer value, corresponding to removing all
        // digits after the decimal point.
        //
        public static Decimal Truncate(Decimal d)
        {
            Truncate(ref d);
            return d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Truncate(ref Decimal d)
        {
            uint flags = d.uflags;
            if ((flags & ScaleMask) != 0)
                DecCalc.InternalRound(ref d, (byte)(flags >> ScaleShift), DecCalc.RoundingMode.Truncate);
        }

        public static implicit operator Decimal(byte value)
        {
            return new Decimal(value);
        }

        [CLSCompliant(false)]
        public static implicit operator Decimal(sbyte value)
        {
            return new Decimal(value);
        }

        public static implicit operator Decimal(short value)
        {
            return new Decimal(value);
        }

        [CLSCompliant(false)]
        public static implicit operator Decimal(ushort value)
        {
            return new Decimal(value);
        }

        public static implicit operator Decimal(char value)
        {
            return new Decimal(value);
        }

        public static implicit operator Decimal(int value)
        {
            return new Decimal(value);
        }

        [CLSCompliant(false)]
        public static implicit operator Decimal(uint value)
        {
            return new Decimal(value);
        }

        public static implicit operator Decimal(long value)
        {
            return new Decimal(value);
        }

        [CLSCompliant(false)]
        public static implicit operator Decimal(ulong value)
        {
            return new Decimal(value);
        }


        public static explicit operator Decimal(float value)
        {
            return new Decimal(value);
        }

        public static explicit operator Decimal(double value)
        {
            return new Decimal(value);
        }

        public static explicit operator byte(Decimal value)
        {
            return ToByte(value);
        }

        [CLSCompliant(false)]
        public static explicit operator sbyte(Decimal value)
        {
            return ToSByte(value);
        }

        public static explicit operator char(Decimal value)
        {
            UInt16 temp;
            try
            {
                temp = ToUInt16(value);
            }
            catch (OverflowException e)
            {
                throw new OverflowException(SR.Overflow_Char, e);
            }
            return (char)temp;
        }

        public static explicit operator short(Decimal value)
        {
            return ToInt16(value);
        }

        [CLSCompliant(false)]
        public static explicit operator ushort(Decimal value)
        {
            return ToUInt16(value);
        }

        public static explicit operator int(Decimal value)
        {
            return ToInt32(value);
        }

        [CLSCompliant(false)]
        public static explicit operator uint(Decimal value)
        {
            return ToUInt32(value);
        }

        public static explicit operator long(Decimal value)
        {
            return ToInt64(value);
        }

        [CLSCompliant(false)]
        public static explicit operator ulong(Decimal value)
        {
            return ToUInt64(value);
        }

        public static explicit operator float(Decimal value)
        {
            return ToSingle(value);
        }

        public static explicit operator double(Decimal value)
        {
            return ToDouble(value);
        }

        public static Decimal operator +(Decimal d)
        {
            return d;
        }

        public static Decimal operator -(Decimal d)
        {
            return Negate(d);
        }

        public static Decimal operator ++(Decimal d)
        {
            return Add(d, One);
        }

        public static Decimal operator --(Decimal d)
        {
            return Subtract(d, One);
        }

        public static Decimal operator +(Decimal d1, Decimal d2)
        {
            return Add(d1, d2);
        }

        public static Decimal operator -(Decimal d1, Decimal d2)
        {
            return Subtract(d1, d2);
        }

        public static Decimal operator *(Decimal d1, Decimal d2)
        {
            return Multiply(d1, d2);
        }

        public static Decimal operator /(Decimal d1, Decimal d2)
        {
            return Divide(d1, d2);
        }

        public static Decimal operator %(Decimal d1, Decimal d2)
        {
            return Remainder(d1, d2);
        }

        public static bool operator ==(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecCmp(ref d1, ref d2) == 0;
        }

        public static bool operator !=(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecCmp(ref d1, ref d2) != 0;
        }

        public static bool operator <(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecCmp(ref d1, ref d2) < 0;
        }

        public static bool operator <=(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecCmp(ref d1, ref d2) <= 0;
        }

        public static bool operator >(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecCmp(ref d1, ref d2) > 0;
        }

        public static bool operator >=(Decimal d1, Decimal d2)
        {
            return DecCalc.VarDecCmp(ref d1, ref d2) >= 0;
        }

        //
        // IConvertible implementation
        //

        public TypeCode GetTypeCode()
        {
            return TypeCode.Decimal;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this);
        }


        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException(String.Format(SR.InvalidCast_FromTo, "Decimal", "Char"));
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this);
        }

        Decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return this;
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(String.Format(SR.InvalidCast_FromTo, "Decimal", "DateTime"));
        }

        Object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType((IConvertible)this, type, provider);
        }

#if __MonoCS__

        // Low level accessors used by a DecCalc and formatting 
        internal uint High
        {
            get { return uhi; }
            set { uhi = value; }
        }

        internal uint Low
        {
            get { return ulo; }
            set { ulo = value; }
        }

        internal uint Mid
        {
            get { return umid; }
            set { umid = value; }
        }

        internal bool IsNegative
        {
            get { return flags < 0; }
            set { uflags = (uflags & ~SignMask) | (value ? SignMask : 0); }
        }

        internal int Scale
        {
            get { return (byte)(uflags >> ScaleShift); }
            set { uflags = (uflags & ~ScaleMask) | ((uint)value << ScaleShift); }
        }

        private ulong Low64
        {
#if BIGENDIAN && !MONO
            get { return ((ulong)umid << 32) | ulo; }
            set { umid = (uint)(value >> 32); ulo = (uint)value; }
#else
            get => ulomidLE;
            set => ulomidLE = value;
#endif
        }

        #region APIs need by number formatting.

        internal static uint DecDivMod1E9(ref Decimal value)
        {
            return DecCalc.DecDivMod1E9(ref value);
        }

        internal static void DecAddInt32(ref Decimal value, uint i)
        {
            DecCalc.DecAddInt32(ref value, i);
        }

        internal static void DecMul10(ref Decimal value)
        {
            DecCalc.DecMul10(ref value);
        }

        #endregion

        /// <summary>
        /// Class that contains all the mathematical calculations for decimal. Most of which have been ported from oleaut32.
        /// </summary>
        private static class DecCalc
        {
            // Constant representing the negative number that is the closest possible
            // Decimal value to -0m.
            private const Decimal NearNegativeZero = -0.000000000000000000000000001m;

            // Constant representing the positive number that is the closest possible
            // Decimal value to +0m.
            private const Decimal NearPositiveZero = +0.000000000000000000000000001m;

            private const int DEC_SCALE_MAX = 28;

            private const uint TenToPowerNine = 1000000000;
            private const ulong TenToPowerEighteen = 1000000000000000000;

            // The maximum power of 10 that a 32 bit integer can store
            private const Int32 MaxInt32Scale = 9;
            // The maximum power of 10 that a 64 bit integer can store
            private const Int32 MaxInt64Scale = 19;

            // Fast access for 10^n where n is 0-9        
            private static readonly UInt32[] s_powers10 = new UInt32[] {
                1,
                10,
                100,
                1000,
                10000,
                100000,
                1000000,
                10000000,
                100000000,
                1000000000
            };

            // Fast access for 10^n where n is 1-19
            private static readonly ulong[] s_ulongPowers10 = new ulong[] {
                10,
                100,
                1000,
                10000,
                100000,
                1000000,
                10000000,
                100000000,
                1000000000,
                10000000000,
                100000000000,
                1000000000000,
                10000000000000,
                100000000000000,
                1000000000000000,
                10000000000000000,
                100000000000000000,
                1000000000000000000,
                10000000000000000000,
            };

            private static readonly double[] s_doublePowers10 = new double[] {
                1, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9,
                1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19,
                1e20, 1e21, 1e22, 1e23, 1e24, 1e25, 1e26, 1e27, 1e28, 1e29,
                1e30, 1e31, 1e32, 1e33, 1e34, 1e35, 1e36, 1e37, 1e38, 1e39,
                1e40, 1e41, 1e42, 1e43, 1e44, 1e45, 1e46, 1e47, 1e48, 1e49,
                1e50, 1e51, 1e52, 1e53, 1e54, 1e55, 1e56, 1e57, 1e58, 1e59,
                1e60, 1e61, 1e62, 1e63, 1e64, 1e65, 1e66, 1e67, 1e68, 1e69,
                1e70, 1e71, 1e72, 1e73, 1e74, 1e75, 1e76, 1e77, 1e78, 1e79,
                1e80
            };

            #region Decimal Math Helpers

            private static unsafe uint GetExponent(float f)
            {
                // Based on pulling out the exp from this single struct layout
                //typedef struct {
                //    ULONG mant:23;
                //    ULONG exp:8;
                //    ULONG sign:1;
                //} SNGSTRUCT;

                return (byte)(*(uint*)&f >> 23);
            }

            private static unsafe uint GetExponent(double d)
            {
                // Based on pulling out the exp from this double struct layout
                //typedef struct {
                //   DWORDLONG mant:52;
                //   DWORDLONG signexp:12;
                // } DBLSTRUCT;

                return (uint)(*(ulong*)&d >> 52) & 0x7FFu;
            }

            private static ulong UInt32x32To64(uint a, uint b)
            {
                return (ulong)a * (ulong)b;
            }

            private static ulong UInt64x64To128(ulong a, ulong b, out ulong dlHi)
            {
                ulong low = (uint)a * (ulong)(uint)b; // lo partial prod
                ulong mid = (uint)a * (b >> 32); // mid 1 partial prod
                ulong high = (a >> 32) * (b >> 32);
                high += mid >> 32;
                low += mid <<= 32;
                if (low < mid)  // test for carry
                    high++;

                mid = (a >> 32) * (uint)b;
                high += mid >> 32;
                low += mid <<= 32;
                if (low < mid)  // test for carry
                    high++;

                dlHi = high;
                return low;
            }

            /***
             * Div96By32
             *
             * Entry:
             *   bufNum - 96-bit dividend as array of ULONGs, least-sig first
             *   ulDen   - 32-bit divisor.
             *
             * Purpose:
             *   Do full divide, yielding 96-bit result and 32-bit remainder.
             *
             * Exit:
             *   Quotient overwrites dividend.
             *   Returns remainder.
             *
             * Exceptions:
             *   None.
             *
             ***********************************************************************/
            private static uint Div96By32(ref Buf12 bufNum, uint ulDen)
            {
                // TODO: https://github.com/dotnet/coreclr/issues/3439
                ulong tmp, div;
                if (bufNum.U2 != 0)
                {
                    tmp = bufNum.High64;
                    div = tmp / ulDen;
                    bufNum.High64 = div;
                    tmp = ((tmp - div * ulDen) << 32) | bufNum.U0;
                    if (tmp == 0)
                        return 0;
                    uint div32 = (uint)(tmp / ulDen);
                    bufNum.U0 = div32;
                    return (uint)tmp - div32 * ulDen;
                }

                tmp = bufNum.Low64;
                if (tmp == 0)
                    return 0;
                div = tmp / ulDen;
                bufNum.Low64 = div;
                return (uint)(tmp - div * ulDen);
            }

            private static uint Div96ByConst100000000(ref Buf12 bufNum)
            {
                const uint ulDen = 100000000;
                ulong tmp = bufNum.High64;
                ulong div = tmp / ulDen;
                bufNum.High64 = div;
                tmp = ((tmp - div * ulDen) << 32) | bufNum.U0;
                uint div32 = (uint)(tmp / ulDen);
                bufNum.U0 = div32;
                return (uint)tmp - div32 * ulDen;
            }

            private static uint Div96ByConst10000(ref Buf12 bufNum)
            {
                const uint ulDen = 10000;
                ulong tmp = bufNum.High64;
                ulong div = tmp / ulDen;
                bufNum.High64 = div;
                tmp = ((tmp - div * ulDen) << 32) | bufNum.U0;
                uint div32 = (uint)(tmp / ulDen);
                bufNum.U0 = div32;
                return (uint)tmp - div32 * ulDen;
            }

            private static uint Div96ByConst100(ref Buf12 bufNum)
            {
                const uint ulDen = 100;
                ulong tmp = bufNum.High64;
                ulong div = tmp / ulDen;
                bufNum.High64 = div;
                tmp = ((tmp - div * ulDen) << 32) | bufNum.U0;
                uint div32 = (uint)(tmp / ulDen);
                bufNum.U0 = div32;
                return (uint)tmp - div32 * ulDen;
            }

            private static uint Div96ByConst10(ref Buf12 bufNum)
            {
                const uint ulDen = 10;
                ulong tmp = bufNum.High64;
                ulong div = tmp / ulDen;
                bufNum.High64 = div;
                tmp = ((tmp - div * ulDen) << 32) | bufNum.U0;
                uint div32 = (uint)(tmp / ulDen);
                bufNum.U0 = div32;
                return (uint)tmp - div32 * ulDen;
            }

            /***
             * Div96By64
             *
             * Entry:
             *   bufNum - 96-bit dividend as array of ULONGs, least-sig first
             *   sdlDen  - 64-bit divisor.
             *
             * Purpose:
             *   Do partial divide, yielding 32-bit result and 64-bit remainder.
             *   Divisor must be larger than upper 64 bits of dividend.
             *
             * Exit:
             *   Remainder overwrites lower 64-bits of dividend.
             *   Returns quotient.
             *
             * Exceptions:
             *   None.
             *
             ***********************************************************************/
            private static uint Div96By64(ref Buf12 bufNum, ulong den)
            {
                uint quo;
                ulong num;
                uint num2 = bufNum.U2;
                if (num2 == 0)
                {
                    num = bufNum.Low64;
                    if (num < den)
                        // Result is zero.  Entire dividend is remainder.
                        return 0;

                    // TODO: https://github.com/dotnet/coreclr/issues/3439
                    quo = (uint)(num / den);
                    num -= quo * den; // remainder
                    bufNum.Low64 = num;
                    return quo;
                }

                uint denHigh32 = (uint)(den >> 32);
                if (num2 >= denHigh32)
                {
                    // Divide would overflow.  Assume a quotient of 2^32, and set
                    // up remainder accordingly.
                    //
                    num = bufNum.Low64;
                    num -= den << 32;
                    quo = 0;

                    // Remainder went negative.  Add divisor back in until it's positive,
                    // a max of 2 times.
                    //
                    do
                    {
                        quo--;
                        num += den;
                    } while (num >= den);

                    bufNum.Low64 = num;
                    return quo;
                }

                // Hardware divide won't overflow
                //
                ulong num64 = bufNum.High64;
                if (num64 < denHigh32)
                    // Result is zero.  Entire dividend is remainder.
                    //
                    return 0;

                // TODO: https://github.com/dotnet/coreclr/issues/3439
                quo = (uint)(num64 / denHigh32);
                num = bufNum.U0 | ((num64 - quo * denHigh32) << 32); // remainder

                // Compute full remainder, rem = dividend - (quo * divisor).
                //
                ulong prod = UInt32x32To64(quo, (uint)den); // quo * lo divisor
                num -= prod;

                if (num > ~prod)
                {
                    // Remainder went negative.  Add divisor back in until it's positive,
                    // a max of 2 times.
                    //
                    do
                    {
                        quo--;
                        num += den;
                    } while (num >= den);
                }

                bufNum.Low64 = num;
                return quo;
            }

            /***
             * Div128By96
             *
             * Entry:
             *   bufNum - 128-bit dividend as array of ULONGs, least-sig first
             *   bufDen - 96-bit divisor.
             *
             * Purpose:
             *   Do partial divide, yielding 32-bit result and 96-bit remainder.
             *   Top divisor ULONG must be larger than top dividend ULONG.  This is
             *   assured in the initial call because the divisor is normalized
             *   and the dividend can't be.  In subsequent calls, the remainder
             *   is multiplied by 10^9 (max), so it can be no more than 1/4 of
             *   the divisor which is effectively multiplied by 2^32 (4 * 10^9).
             *
             * Exit:
             *   Remainder overwrites lower 96-bits of dividend.
             *   Returns quotient.
             *
             * Exceptions:
             *   None.
             *
             ***********************************************************************/
            private static uint Div128By96(ref Buf16 bufNum, ref Buf12 bufDen)
            {
                ulong dividend = bufNum.High64;
                uint den = bufDen.U2;
                if (dividend < den)
                    // Result is zero.  Entire dividend is remainder.
                    //
                    return 0;

                // TODO: https://github.com/dotnet/coreclr/issues/3439
                uint quo = (uint)(dividend / den);
                uint remainder = (uint)dividend - quo * den;

                // Compute full remainder, rem = dividend - (quo * divisor).
                //
                ulong prod1 = UInt32x32To64(quo, bufDen.U0); // quo * lo divisor
                ulong prod2 = UInt32x32To64(quo, bufDen.U1); // quo * mid divisor
                prod2 += prod1 >> 32;
                prod1 = (uint)prod1 | (prod2 << 32);
                prod2 >>= 32;

                ulong num = bufNum.Low64;
                num -= prod1;
                remainder -= (uint)prod2;

                // Propagate carries
                //
                if (num > ~prod1)
                {
                    remainder--;
                    if (remainder < ~(uint)prod2)
                        goto PosRem;
                }
                else if (remainder <= ~(uint)prod2)
                    goto PosRem;
                {
                    // Remainder went negative.  Add divisor back in until it's positive,
                    // a max of 2 times.
                    //
                    prod1 = bufDen.Low64;

                    for (;;)
                    {
                        quo--;
                        num += prod1;
                        remainder += den;

                        if (num < prod1)
                        {
                            // Detected carry. Check for carry out of top
                            // before adding it in.
                            //
                            if (remainder++ < den)
                                break;
                        }
                        if (remainder < den)
                            break; // detected carry
                    }
                }
PosRem:

                bufNum.Low64 = num;
                bufNum.U2 = remainder;
                return quo;
            }

            /***
             * IncreaseScale
             *
             * Entry:
             *   bufNum - 96-bit number as array of ULONGs, least-sig first
             *   ulPwr   - Scale factor to multiply by
             *
             * Purpose:
             *   Multiply the two numbers.  The low 96 bits of the result overwrite
             *   the input.  The last 32 bits of the product are the return value.
             *
             * Exit:
             *   Returns highest 32 bits of product.
             *
             * Exceptions:
             *   None.
             *
             ***********************************************************************/
            private static uint IncreaseScale(ref Buf12 bufNum, uint ulPwr)
            {
                ulong tmp = UInt32x32To64(bufNum.U0, ulPwr);
                bufNum.U0 = (uint)tmp;
                tmp >>= 32;
                tmp += UInt32x32To64(bufNum.U1, ulPwr);
                bufNum.U1 = (uint)tmp;
                tmp >>= 32;
                tmp += UInt32x32To64(bufNum.U2, ulPwr);
                bufNum.U2 = (uint)tmp;
                return (uint)(tmp >> 32);
            }

            private static void IncreaseScale64(ref Buf12 bufNum, uint ulPwr)
            {
                ulong tmp = UInt32x32To64(bufNum.U0, ulPwr);
                bufNum.U0 = (uint)tmp;
                tmp >>= 32;
                tmp += UInt32x32To64(bufNum.U1, ulPwr);
                bufNum.High64 = tmp;
            }

            /***
            * ScaleResult
            *
            * Entry:
            *   bufRes - Array of ULONGs with value, least-significant first.
            *   iHiRes  - Index of last non-zero value in bufRes.
            *   iScale  - Scale factor for this value, range 0 - 2 * DEC_SCALE_MAX
            *
            * Purpose:
            *   See if we need to scale the result to fit it in 96 bits.
            *   Perform needed scaling.  Adjust scale factor accordingly.
            *
            * Exit:
            *   bufRes updated in place, always 3 ULONGs.
            *   New scale factor returned.
            *
            ***********************************************************************/
            private static unsafe int ScaleResult(Buf24* bufRes, uint iHiRes, int iScale)
            {
                Debug.Assert(iHiRes < bufRes->Length);
                uint* rgulRes = (uint*)bufRes;

                // See if we need to scale the result.  The combined scale must
                // be <= DEC_SCALE_MAX and the upper 96 bits must be zero.
                // 
                // Start by figuring a lower bound on the scaling needed to make
                // the upper 96 bits zero.  iHiRes is the index into rgulRes[]
                // of the highest non-zero ULONG.
                // 
                int iNewScale = 0;
                if (iHiRes > 2)
                {
                    iNewScale = (int)iHiRes * 32 - 64 - 1;
                    iNewScale -= LeadingZeroCount(rgulRes[iHiRes]);

                    // Multiply bit position by log10(2) to figure it's power of 10.
                    // We scale the log by 256.  log(2) = .30103, * 256 = 77.  Doing this 
                    // with a multiply saves a 96-byte lookup table.  The power returned
                    // is <= the power of the number, so we must add one power of 10
                    // to make it's integer part zero after dividing by 256.
                    // 
                    // Note: the result of this multiplication by an approximation of
                    // log10(2) have been exhaustively checked to verify it gives the 
                    // correct result.  (There were only 95 to check...)
                    // 
                    iNewScale = ((iNewScale * 77) >> 8) + 1;

                    // iNewScale = min scale factor to make high 96 bits zero, 0 - 29.
                    // This reduces the scale factor of the result.  If it exceeds the
                    // current scale of the result, we'll overflow.
                    // 
                    if (iNewScale > iScale)
                        goto ThrowOverflow;
                }

                // Make sure we scale by enough to bring the current scale factor
                // into valid range.
                //
                if (iNewScale < iScale - DEC_SCALE_MAX)
                    iNewScale = iScale - DEC_SCALE_MAX;

                if (iNewScale != 0)
                {
                    // Scale by the power of 10 given by iNewScale.  Note that this is 
                    // NOT guaranteed to bring the number within 96 bits -- it could 
                    // be 1 power of 10 short.
                    //
                    iScale -= iNewScale;
                    uint ulSticky = 0;
                    uint quotient, remainder = 0;

                    for (;;)
                    {
                        ulSticky |= remainder; // record remainder as sticky bit

                        uint ulPwr;
                        uint high = rgulRes[iHiRes];
                        // Scaling loop specialized for each power of 10 because division by constant is an order of magnitude faster (especially for 64-bit division that's actually done by 128bit DIV on x64)
                        switch (iNewScale)
                        {
                            case 1:
                                {
                                    const uint power = 10;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                            case 2:
                                {
                                    const uint power = 100;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                            case 3:
                                {
                                    const uint power = 1000;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                            case 4:
                                {
                                    const uint power = 10000;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                            case 5:
                                {
                                    const uint power = 100000;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                            case 6:
                                {
                                    const uint power = 1000000;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                            case 7:
                                {
                                    const uint power = 10000000;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                            case 8:
                                {
                                    const uint power = 100000000;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                            default:
                                {
                                    const uint power = TenToPowerNine;
                                    remainder = high - (quotient = high / power) * power;
                                    for (uint i = iHiRes - 1; (int)i >= 0; i--)
                                    {
                                        ulong num = rgulRes[i] + ((ulong)remainder << 32);
                                        remainder = (uint)num - (rgulRes[i] = (uint)(num / power)) * power;
                                    }
                                    ulPwr = power;
                                    break;
                                }
                        }
                        rgulRes[iHiRes] = quotient;
                        // If first quotient was 0, update iHiRes.
                        //
                        if (quotient == 0 && iHiRes != 0)
                            iHiRes--;

                        iNewScale -= MaxInt32Scale;
                        if (iNewScale > 0)
                            continue; // scale some more

                        // If we scaled enough, iHiRes would be 2 or less.  If not,
                        // divide by 10 more.
                        //
                        if (iHiRes > 2)
                        {
                            if (iScale == 0)
                                goto ThrowOverflow;
                            iNewScale = 1;
                            iScale--;
                            continue; // scale by 10
                        }

                        // Round final result.  See if remainder >= 1/2 of divisor.
                        // If remainder == 1/2 divisor, round up if odd or sticky bit set.
                        //
                        ulPwr >>= 1;  // power of 10 always even
                        if (ulPwr <= remainder && (ulPwr < remainder || ((rgulRes[0] & 1) | ulSticky) != 0) && ++rgulRes[0] == 0)
                        {
                            uint iCur = 0;
                            do
                            {
                                Debug.Assert(iCur + 1 < bufRes->Length);
                            }
                            while (++rgulRes[++iCur] == 0);

                            if (iCur > 2)
                            {
                                // The rounding caused us to carry beyond 96 bits. 
                                // Scale by 10 more.
                                //
                                if (iScale == 0)
                                    goto ThrowOverflow;
                                iHiRes = iCur;
                                ulSticky = 0;  // no sticky bit
                                remainder = 0; // or remainder
                                iNewScale = 1;
                                iScale--;
                                continue; // scale by 10
                            }
                        }

                        break;
                    } // for(;;)
                }
                return iScale;

ThrowOverflow:
                throw new OverflowException(SR.Overflow_Decimal);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int LeadingZeroCount(uint value)
            {
#if CORECLR
                if (System.Runtime.Intrinsics.X86.Lzcnt.IsSupported)
                    return (int)System.Runtime.Intrinsics.X86.Lzcnt.LeadingZeroCount(value);
#endif

                int c = 1;
                if ((value & 0xFFFF0000) == 0)
                {
                    value <<= 16;
                    c += 16;
                }
                if ((value & 0xFF000000) == 0)
                {
                    value <<= 8;
                    c += 8;
                }
                if ((value & 0xF0000000) == 0)
                {
                    value <<= 4;
                    c += 4;
                }
                if ((value & 0xC0000000) == 0)
                {
                    value <<= 2;
                    c += 2;
                }
                return c + ((int)value >> 31);
            }

            // Adjust the quotient to deal with an overflow. We need to divide by 10, 
            // feed in the high bit to undo the overflow and then round as required, 
            private static int OverflowUnscale(ref Buf12 bufQuo, int iScale, bool fRemainder)
            {
                if (--iScale < 0)
                    throw new OverflowException(SR.Overflow_Decimal);

                Debug.Assert(bufQuo.U2 == 0);

                // We have overflown, so load the high bit with a one.
                const ulong highbit = 1UL << 32;
                bufQuo.U2 = (uint)(highbit / 10);
                ulong tmp = ((highbit % 10) << 32) + bufQuo.U1;
                uint div = (uint)(tmp / 10);
                bufQuo.U1 = div;
                tmp = ((tmp - div * 10) << 32) + bufQuo.U0;
                div = (uint)(tmp / 10);
                bufQuo.U0 = div;
                uint remainder = (uint)(tmp - div * 10);
                // The remainder is the last digit that does not fit, so we can use it to work out if we need to round up
                if (remainder > 5 || remainder == 5 && (fRemainder || (bufQuo.U0 & 1) != 0))
                    Add32To96(ref bufQuo, 1);
                return iScale;
            }

            /***
            * SearchScale
            *
            * Entry:
            *   bufQuo - 96-bit quotient
            *   iScale  - Scale factor of quotient, range -DEC_SCALE_MAX to DEC_SCALE_MAX-1
            *
            * Purpose:
            *   Determine the max power of 10, <= 9, that the quotient can be scaled
            *   up by and still fit in 96 bits.
            *
            * Exit:
            *   Returns power of 10 to scale by.
            *
            ***********************************************************************/
            private static int SearchScale(ref Buf12 bufQuo, int iScale)
            {
                const uint OVFL_MAX_9_HI = 4;
                const uint OVFL_MAX_8_HI = 42;
                const uint OVFL_MAX_7_HI = 429;
                const uint OVFL_MAX_6_HI = 4294;
                const uint OVFL_MAX_5_HI = 42949;
                const uint OVFL_MAX_4_HI = 429496;
                const uint OVFL_MAX_3_HI = 4294967;
                const uint OVFL_MAX_2_HI = 42949672;
                const uint OVFL_MAX_1_HI = 429496729;
                const ulong OVFL_MAX_9_MIDLO = 5441186219426131129;

                uint ulResHi = bufQuo.U2;
                ulong ulResMidLo = bufQuo.Low64;
                int iCurScale = 0;

                // Quick check to stop us from trying to scale any more.
                //
                if (ulResHi > OVFL_MAX_1_HI)
                {
                    goto HaveScale;
                }

                var powerOvfl = PowerOvflValues;
                if (iScale > DEC_SCALE_MAX - 9)
                {
                    // We can't scale by 10^9 without exceeding the max scale factor.
                    // See if we can scale to the max.  If not, we'll fall into
                    // standard search for scale factor.
                    //
                    iCurScale = DEC_SCALE_MAX - iScale;
                    if (ulResHi < powerOvfl[iCurScale - 1].Hi)
                        goto HaveScale;
                }
                else if (ulResHi < OVFL_MAX_9_HI || ulResHi == OVFL_MAX_9_HI && ulResMidLo <= OVFL_MAX_9_MIDLO)
                    return 9;

                // Search for a power to scale by < 9.  Do a binary search.
                //
                if (ulResHi > OVFL_MAX_5_HI)
                {
                    if (ulResHi > OVFL_MAX_3_HI)
                    {
                        iCurScale = 2;
                        if (ulResHi > OVFL_MAX_2_HI)
                            iCurScale--;
                    }
                    else
                    {
                        iCurScale = 4;
                        if (ulResHi > OVFL_MAX_4_HI)
                            iCurScale--;
                    }
                }
                else
                {
                    if (ulResHi > OVFL_MAX_7_HI)
                    {
                        iCurScale = 6;
                        if (ulResHi > OVFL_MAX_6_HI)
                            iCurScale--;
                    }
                    else
                    {
                        iCurScale = 8;
                        if (ulResHi > OVFL_MAX_8_HI)
                            iCurScale--;
                    }
                }

                // In all cases, we already found we could not use the power one larger.
                // So if we can use this power, it is the biggest, and we're done.  If
                // we can't use this power, the one below it is correct for all cases 
                // unless it's 10^1 -- we might have to go to 10^0 (no scaling).
                // 
                if (ulResHi == powerOvfl[iCurScale - 1].Hi && ulResMidLo > powerOvfl[iCurScale - 1].MidLo)
                    iCurScale--;

                HaveScale:
                // iCurScale = largest power of 10 we can scale by without overflow, 
                // iCurScale < 9.  See if this is enough to make scale factor 
                // positive if it isn't already.
                // 
                if (iCurScale + iScale < 0)
                    throw new OverflowException(SR.Overflow_Decimal);

                return iCurScale;
            }

            // Add a 32 bit unsigned long to an array of 3 unsigned longs representing a 96 integer
            // Returns false if there is an overflow
            private static bool Add32To96(ref Buf12 bufNum, uint ulValue)
            {
                if ((bufNum.Low64 += ulValue) < ulValue)
                {
                    if (++bufNum.U2 == 0)
                        return false;
                }
                return true;
            }

            // DecAddSub adds or subtracts two decimal values.  On return, d1 contains the result
            // of the operation.  Passing in true for bSign means subtract and false means add.
            private static unsafe void DecAddSub(ref Decimal d1, ref Decimal d2, bool bSign)
            {
                ulong low64 = d1.Low64;
                uint high = d1.High, flags = d1.uflags, d2flags = d2.uflags;

                uint xorflags = d2flags ^ flags;
                bSign ^= (xorflags & SignMask) != 0;

                if ((xorflags & ScaleMask) == 0)
                {
                    // Scale factors are equal, no alignment necessary.
                    //
                    goto AlignedAdd;
                }
                else
                {
                    // Scale factors are not equal.  Assume that a larger scale
                    // factor (more decimal places) is likely to mean that number
                    // is smaller.  Start by guessing that the right operand has
                    // the larger scale factor.  The result will have the larger
                    // scale factor.
                    //
                    uint d1flags = flags;
                    flags = d2flags & ScaleMask | flags & SignMask; // scale factor of "smaller",  but sign of "larger"
                    int iScale = (int)(flags - d1flags) >> ScaleShift;

                    if (iScale < 0)
                    {
                        // Guessed scale factor wrong. Swap operands.
                        //
                        iScale = -iScale;
                        flags = d1flags;
                        if (bSign)
                            flags ^= SignMask;
                        low64 = d2.Low64;
                        high = d2.High;
                        d2 = d1;
                    }

                    Buf24 bufNum;
                    _ = &bufNum; // workaround for CS0165
                    uint ulPwr, iHiProd;
                    ulong tmp64;

                    // d1 will need to be multiplied by 10^iScale so
                    // it will have the same scale as d2.  We could be
                    // extending it to up to 192 bits of precision.
                    //
                    if (iScale <= MaxInt32Scale)
                    {
                        // Scaling won't make it larger than 4 ULONGs
                        //
                        ulPwr = s_powers10[iScale];
                        ulong tmpLow = UInt32x32To64((uint)low64, ulPwr);
                        tmp64 = (low64 >> 32) * ulPwr + (tmpLow >> 32);
                        low64 = (uint)tmpLow + (tmp64 << 32);
                        tmp64 >>= 32;
                        tmp64 += UInt32x32To64(high, ulPwr);
                        if (tmp64 <= uint.MaxValue)
                        {
                            // Result fits in 96 bits.  Use standard aligned add.
                            //
                            high = (uint)tmp64;
                            goto AlignedAdd;
                        }
                        bufNum.Low64 = low64;
                        bufNum.Mid64 = tmp64;
                        iHiProd = 3;
                    }
                    else
                    {
                        iHiProd = 2;

                        // Scan for zeros in the upper words.
                        //
                        if (high == 0)
                        {
                            iHiProd = 1;
                            if (low64 >> 32 == 0)
                            {
                                iHiProd = 0;
                                if ((uint)low64 == 0)
                                {
                                    // Left arg is zero, return right.
                                    //
                                    uint signFlags = flags & SignMask;
                                    if (bSign)
                                        signFlags ^= SignMask;
                                    d1 = d2;
                                    d1.uflags = d2.uflags & ScaleMask | signFlags;
                                    return;
                                }
                            }
                        }

                        // Have to scale by a bunch.  Move the number to a buffer
                        // where it has room to grow as it's scaled.
                        //
                        bufNum.Low64 = low64;
                        bufNum.U2 = high;

                        // Scaling loop, up to 10^9 at a time.  iHiProd stays updated
                        // with index of highest non-zero ULONG.
                        //
                        for (; iScale > 0; iScale -= MaxInt32Scale)
                        {
                            if (iScale >= MaxInt32Scale)
                                ulPwr = TenToPowerNine;
                            else
                                ulPwr = s_powers10[iScale];

                            tmp64 = 0;
                            uint* rgulNum = (uint*)&bufNum;
                            for (uint iCur = 0; ;)
                            {
                                Debug.Assert(iCur < bufNum.Length);
                                tmp64 += UInt32x32To64(rgulNum[iCur], ulPwr);
                                rgulNum[iCur] = (uint)tmp64;
                                iCur++;
                                tmp64 >>= 32;
                                if (iCur > iHiProd)
                                    break;
                            }

                            if ((uint)tmp64 != 0)
                            {
                                // We're extending the result by another ULONG.
                                Debug.Assert(iHiProd + 1 < bufNum.Length);
                                rgulNum[++iHiProd] = (uint)tmp64;
                            }
                        }
                    }

                    // Scaling complete, do the add.  Could be subtract if signs differ.
                    //
                    tmp64 = bufNum.Low64;
                    low64 = d2.Low64;
                    uint tmpHigh = bufNum.U2;
                    high = d2.High;

                    if (bSign)
                    {
                        // Signs differ, subtract.
                        //
                        low64 = tmp64 - low64;
                        high = tmpHigh - high;

                        // Propagate carry
                        //
                        if (low64 > tmp64)
                        {
                            high--;
                            if (high < tmpHigh)
                                goto NoCarry;
                        }
                        else if (high <= tmpHigh)
                            goto NoCarry;

                        // If rgulNum has more than 96 bits of precision, then we need to 
                        // carry the subtraction into the higher bits.  If it doesn't, 
                        // then we subtracted in the wrong order and have to flip the 
                        // sign of the result.
                        // 
                        if (iHiProd <= 2)
                        {
                            goto SignFlip;
                        }

                        uint* rgulNum = (uint*)&bufNum;
                        uint iCur = 3;
                        do
                        {
                            Debug.Assert(iCur < bufNum.Length);
                        } while (rgulNum[iCur++]-- == 0);
                        Debug.Assert(iHiProd < bufNum.Length);
                        if (rgulNum[iHiProd] == 0)
                            iHiProd--;
                    }
                    else
                    {
                        // Signs the same, add.
                        //
                        low64 += tmp64;
                        high += tmpHigh;

                        // Propagate carry
                        //
                        if (low64 < tmp64)
                        {
                            high++;
                            if (high > tmpHigh)
                                goto NoCarry;
                        }
                        else if (high >= tmpHigh)
                            goto NoCarry;

                        uint* rgulNum = (uint*)&bufNum;
                        uint iCur = 3;
                        do
                        {
                            Debug.Assert(iCur < bufNum.Length);
                            if (iHiProd < iCur)
                            {
                                rgulNum[iCur] = 1;
                                iHiProd = iCur;
                                break;
                            }
                        } while (++rgulNum[iCur++] == 0);
                    }
NoCarry:

                    if (iHiProd > 2)
                    {
                        bufNum.Low64 = low64;
                        bufNum.U2 = high;
                        int scale = ScaleResult(&bufNum, iHiProd, (byte)(flags >> ScaleShift));
                        flags = (flags & ~ScaleMask) | ((uint)scale << ScaleShift);

                        low64 = bufNum.Low64;
                        high = bufNum.U2;
                    }

                    goto ReturnResult;
                }

SignFlip:
                {
                    // Got negative result.  Flip its sign.
                    flags ^= SignMask;
                    high = ~high;
                    low64 = (ulong)-(long)low64;
                    if (low64 == 0)
                        high++;
                    goto ReturnResult;
                }

AlignedScale:
                {
                    // The addition carried above 96 bits.
                    // Divide the value by 10, dropping the scale factor.
                    // 
                    if ((flags & ScaleMask) == 0)
                        throw new OverflowException(SR.Overflow_Decimal);
                    flags -= 1 << ScaleShift;

                    const uint den = 10;
                    ulong num = high + (1UL << 32);
                    high = (uint)(num / den);
                    num = ((num - high * den) << 32) + (low64 >> 32);
                    uint div = (uint)(num / den);
                    num = ((num - div * den) << 32) + (uint)low64;
                    low64 = div;
                    low64 <<= 32;
                    div = (uint)(num / den);
                    low64 += div;
                    div = (uint)num - div * den;

                    // See if we need to round up.
                    //
                    if (div >= 5 && (div > 5 || (low64 & 1) != 0))
                    {
                        if (++low64 == 0)
                            high++;
                    }
                    goto ReturnResult;
                }

AlignedAdd:
                {
                    ulong d1Low64 = low64;
                    uint d1High = high;
                    if (bSign)
                    {
                        // Signs differ - subtract
                        //
                        low64 = d1Low64 - d2.Low64;
                        high = d1High - d2.High;

                        // Propagate carry
                        //
                        if (low64 > d1Low64)
                        {
                            high--;
                            if (high >= d1High)
                                goto SignFlip;
                        }
                        else if (high > d1High)
                            goto SignFlip;
                    }
                    else
                    {
                        // Signs are the same - add
                        //
                        low64 = d1Low64 + d2.Low64;
                        high = d1High + d2.High;

                        // Propagate carry
                        //
                        if (low64 < d1Low64)
                        {
                            high++;
                            if (high <= d1High)
                                goto AlignedScale;
                        }
                        else if (high < d1High)
                            goto AlignedScale;
                    }
                    goto ReturnResult;
                }

ReturnResult:
                d1.uflags = flags;
                d1.High = high;
                d1.Low64 = low64;
                return;
            }

#endregion

            //**********************************************************************
            // VarCyFromDec - Convert Currency to Decimal (similar to OleAut32 api.)
            //**********************************************************************
            internal static long VarCyFromDec(ref Decimal pdecIn)
            {
                long value;

                int scale = pdecIn.Scale - 4;
                // Need to scale to get 4 decimal places.  -4 <= scale <= 24.
                //
                if (scale < 0)
                {
                    if (pdecIn.High != 0)
                        goto ThrowOverflow;
                    uint pwr = s_powers10[-scale];
                    ulong high = UInt32x32To64(pwr, pdecIn.Mid);
                    if (high > uint.MaxValue)
                        goto ThrowOverflow;
                    ulong low = UInt32x32To64(pwr, pdecIn.Low);
                    low += high <<= 32;
                    if (low < high)
                        goto ThrowOverflow;
                    value = (long)low;
                }
                else
                {
                    if (scale != 0)
                        InternalRound(ref pdecIn, (uint)scale, RoundingMode.ToEven);
                    if (pdecIn.High != 0)
                        goto ThrowOverflow;
                    value = (long)pdecIn.Low64;
                }

                if (value < 0 && (value != long.MinValue || !pdecIn.IsNegative))
                    goto ThrowOverflow;

                if (pdecIn.IsNegative)
                    value = -value;

                return value;

ThrowOverflow:
                throw new OverflowException(SR.Overflow_Currency);
            }

            //**********************************************************************
            // VarDecCmp - Decimal Compare updated to return values similar to ICompareTo
            //**********************************************************************
            internal static int VarDecCmp(ref Decimal pdecL, ref Decimal pdecR)
            {
                if ((pdecR.Low | pdecR.Mid | pdecR.High) == 0)
                {
                    if ((pdecL.Low | pdecL.Mid | pdecL.High) == 0)
                        return 0;
                    return (pdecL.flags >> 31) | 1;
                }
                if ((pdecL.Low | pdecL.Mid | pdecL.High) == 0)
                    return -((pdecR.flags >> 31) | 1);

                int sign = (pdecL.flags >> 31) - (pdecR.flags >> 31);
                if (sign != 0)
                    return sign;
                return VarDecCmpSub(ref pdecL, ref pdecR);
            }

            private static int VarDecCmpSub(ref Decimal d1, ref Decimal d2)
            {
                int flags = d2.flags;
                int sign = (flags >> 31) | 1;
                int iScale = flags - d1.flags;

                ulong low64 = d1.Low64;
                uint high = d1.High;

                ulong d2Low64 = d2.Low64;
                uint d2High = d2.High;

                if (iScale != 0)
                {
                    iScale >>= ScaleShift;

                    // Scale factors are not equal. Assume that a larger scale factor (more decimal places) is likely to mean that number is smaller.
                    // Start by guessing that the right operand has the larger scale factor.
                    if (iScale < 0)
                    {
                        // Guessed scale factor wrong. Swap operands.
                        iScale = -iScale;
                        sign = -sign;

                        ulong tmp64 = low64;
                        low64 = d2Low64;
                        d2Low64 = tmp64;

                        uint tmp = high;
                        high = d2High;
                        d2High = tmp;
                    }

                    // d1 will need to be multiplied by 10^iScale so it will have the same scale as d2.
                    // Scaling loop, up to 10^9 at a time.
                    do
                    {
                        uint ulPwr = iScale >= MaxInt32Scale ? TenToPowerNine : s_powers10[iScale];
                        ulong tmpLow = UInt32x32To64((uint)low64, ulPwr);
                        ulong tmp = (low64 >> 32) * ulPwr + (tmpLow >> 32);
                        low64 = (uint)tmpLow + (tmp << 32);
                        tmp >>= 32;
                        tmp += UInt32x32To64(high, ulPwr);
                        // If the scaled value has more than 96 significant bits then it's greater than d2
                        if (tmp > uint.MaxValue)
                            return sign;
                        high = (uint)tmp;
                    } while ((iScale -= MaxInt32Scale) > 0);
                }

                uint cmpHigh = high - d2High;
                if (cmpHigh != 0)
                {
                    // check for overflow
                    if (cmpHigh > high)
                        sign = -sign;
                    return sign;
                }

                ulong cmpLow64 = low64 - d2Low64;
                if (cmpLow64 == 0)
                    sign = 0;
                // check for overflow
                else if (cmpLow64 > low64)
                    sign = -sign;
                return sign;
            }

            //**********************************************************************
            // VarDecMul - Decimal Multiply
            //**********************************************************************
            internal static unsafe void VarDecMul(ref Decimal pdecL, ref Decimal pdecR)
            {
                int iScale = (byte)(pdecL.uflags + pdecR.uflags >> ScaleShift);

                if ((pdecL.High | pdecL.Mid | pdecR.High | pdecR.Mid) == 0)
                {
                    // Upper 64 bits are zero.
                    //
                    ulong low64 = UInt32x32To64(pdecL.Low, pdecR.Low);
                    if (iScale > DEC_SCALE_MAX)
                    {
                        // Result iScale is too big.  Divide result by power of 10 to reduce it.
                        // If the amount to divide by is > 19 the result is guaranteed
                        // less than 1/2.  [max value in 64 bits = 1.84E19]
                        //
                        iScale -= DEC_SCALE_MAX + 1;
                        if (iScale >= MaxInt64Scale)
                            goto ReturnZero;

                        ulong ulPwr = s_ulongPowers10[iScale];

                        // TODO: https://github.com/dotnet/coreclr/issues/3439
                        ulong div = low64 / ulPwr;
                        ulong remainder = low64 - div * ulPwr;
                        low64 = div;

                        // Round result.  See if remainder >= 1/2 of divisor.
                        // Divisor is a power of 10, so it is always even.
                        //
                        ulPwr >>= 1;
                        if (remainder >= ulPwr && (remainder > ulPwr || (low64 & 1) > 0))
                            low64++;

                        iScale = DEC_SCALE_MAX;
                    }
                    pdecL.Low64 = low64;
                }
                else
                {
                    // At least one operand has bits set in the upper 64 bits.
                    //
                    // Compute and accumulate the 9 partial products into a 
                    // 192-bit (24-byte) result.
                    //
                    //        [l-h][l-m][l-l]      left high, middle, low
                    //         x    [r-h][r-m][r-l]      right high, middle, low
                    // ------------------------------
                    //
                    //             [0-h][0-l]      l-l * r-l
                    //        [1ah][1al]      l-l * r-m
                    //        [1bh][1bl]      l-m * r-l
                    //       [2ah][2al]          l-m * r-m
                    //       [2bh][2bl]          l-l * r-h
                    //       [2ch][2cl]          l-h * r-l
                    //      [3ah][3al]          l-m * r-h
                    //      [3bh][3bl]          l-h * r-m
                    // [4-h][4-l]              l-h * r-h
                    // ------------------------------
                    // [p-5][p-4][p-3][p-2][p-1][p-0]      prod[] array
                    //
                    uint iHiProd;
                    Buf24 bufProd;
                    _ = &bufProd; // workaround for CS0165

                    ulong tmp = UInt32x32To64(pdecL.Low, pdecR.Low);
                    bufProd.U0 = (uint)tmp;

                    ulong tmp2 = UInt32x32To64(pdecL.Low, pdecR.Mid) + (tmp >> 32);

                    tmp = UInt32x32To64(pdecL.Mid, pdecR.Low);
                    tmp += tmp2; // this could generate carry
                    bufProd.U1 = (uint)tmp;
                    if (tmp < tmp2) // detect carry
                        tmp2 = 1UL << 32;
                    else
                        tmp2 = 0;
                    tmp2 += tmp >> 32;

                    tmp = UInt32x32To64(pdecL.Mid, pdecR.Mid) + tmp2;

                    if ((pdecL.High | pdecR.High) > 0)
                    {
                        // Highest 32 bits is non-zero.     Calculate 5 more partial products.
                        //
                        tmp2 = UInt32x32To64(pdecL.Low, pdecR.High);
                        tmp += tmp2; // this could generate carry
                        ulong tmp3 = 0;
                        if (tmp < tmp2) // detect carry
                            tmp3 = 1UL << 32;

                        tmp2 = UInt32x32To64(pdecL.High, pdecR.Low);
                        tmp += tmp2; // this could generate carry
                        bufProd.U2 = (uint)tmp;
                        if (tmp < tmp2) // detect carry
                            tmp3 += 1UL << 32;
                        tmp2 = tmp3 + (tmp >> 32);

                        tmp = UInt32x32To64(pdecL.Mid, pdecR.High);
                        tmp += tmp2; // this could generate carry
                        tmp3 = 0;
                        if (tmp < tmp2) // detect carry
                            tmp3 = 1UL << 32;

                        tmp2 = UInt32x32To64(pdecL.High, pdecR.Mid);
                        tmp += tmp2; // this could generate carry
                        bufProd.U3 = (uint)tmp;
                        if (tmp < tmp2) // detect carry
                            tmp3 += 1UL << 32;
                        tmp3 += tmp >> 32;

                        tmp = UInt32x32To64(pdecL.High, pdecR.High) + tmp3;
                        bufProd.High64 = tmp;

                        iHiProd = 5;
                    }
                    else if (tmp != 0)
                    {
                        bufProd.Mid64 = tmp;
                        iHiProd = 3;
                    }
                    else
                        iHiProd = 1;

                    // Check for leading zero ULONGs on the product
                    //
                    uint* rgulProd = (uint*)&bufProd;
                    while (rgulProd[iHiProd] == 0)
                    {
                        if (iHiProd == 0)
                            goto ReturnZero;
                        iHiProd--;
                    }

                    if (iHiProd > 2 || iScale > DEC_SCALE_MAX)
                    {
                        iScale = ScaleResult(&bufProd, iHiProd, iScale);
                    }

                    pdecL.Low64 = bufProd.Low64;
                    pdecL.High = bufProd.U2;
                }

                pdecL.uflags = ((pdecR.uflags ^ pdecL.uflags) & SignMask) | ((uint)iScale << ScaleShift);
                return;

ReturnZero:
                pdecL = default(Decimal);
            }

            //**********************************************************************
            // VarDecFromR4 - Convert float to Decimal
            //**********************************************************************
#if PROJECTN // Workaround ProjectN bug #555233
            [MethodImplAttribute(MethodImplOptions.NoOptimization)]
#endif
            internal static void VarDecFromR4(float input, out Decimal pdecOut)
            {
                pdecOut = new Decimal();

                // The most we can scale by is 10^28, which is just slightly more
                // than 2^93.  So a float with an exponent of -94 could just
                // barely reach 0.5, but smaller exponents will always round to zero.
                //
                const uint SNGBIAS = 126;
                int iExp = (int)(GetExponent(input) - SNGBIAS);
                if (iExp < -94)
                    return; // result should be zeroed out

                if (iExp > 96)
                    goto ThrowOverflow;

                uint flags = 0;
                if (input < 0)
                {
                    input = -input;
                    flags = SignMask;
                }

                // Round the input to a 7-digit integer.  The R4 format has
                // only 7 digits of precision, and we want to keep garbage digits
                // out of the Decimal were making.
                //
                // Calculate max power of 10 input value could have by multiplying 
                // the exponent by log10(2).  Using scaled integer multiplcation, 
                // log10(2) * 2 ^ 16 = .30103 * 65536 = 19728.3.
                //
                double dbl = input;
                int iPower = 6 - ((iExp * 19728) >> 16);
                // iPower is between -22 and 35

                if (iPower >= 0)
                {
                    // We have less than 7 digits, scale input up.
                    //
                    if (iPower > DEC_SCALE_MAX)
                        iPower = DEC_SCALE_MAX;

                    dbl *= s_doublePowers10[iPower];
                }
                else
                {
                    if (iPower != -1 || dbl >= 1E7)
                        dbl /= s_doublePowers10[-iPower];
                    else
                        iPower = 0; // didn't scale it
                }

                Debug.Assert(dbl < 1E7);
                if (dbl < 1E6 && iPower < DEC_SCALE_MAX)
                {
                    dbl *= 10;
                    iPower++;
                    Debug.Assert(dbl >= 1E6);
                }

                // Round to integer
                //
                uint ulMant = (uint)dbl;
                dbl -= (double)ulMant;  // difference between input & integer
                if (dbl > 0.5 || dbl == 0.5 && (ulMant & 1) != 0)
                    ulMant++;

                if (ulMant == 0)
                    return;  // result should be zeroed out

                if (iPower < 0)
                {
                    // Add -iPower factors of 10, -iPower <= (29 - 7) = 22.
                    //
                    iPower = -iPower;
                    if (iPower < 10)
                    {
                        pdecOut.Low64 = UInt32x32To64(ulMant, s_powers10[iPower]);
                    }
                    else
                    {
                        // Have a big power of 10.
                        //
                        if (iPower > 18)
                        {
                            ulong low64 = UInt32x32To64(ulMant, s_powers10[iPower - 18]);
                            low64 = UInt64x64To128(low64, TenToPowerEighteen, out ulong tmplong);
                            ulong hi64 = tmplong;
                            if (hi64 > uint.MaxValue)
                                goto ThrowOverflow;
                            pdecOut.Low64 = low64;
                            pdecOut.High = (uint)hi64;
                        }
                        else
                        {
                            ulong low64 = UInt32x32To64(ulMant, s_powers10[iPower - 9]);
                            ulong hi64 = UInt32x32To64(TenToPowerNine, (uint)(low64 >> 32));
                            low64 = UInt32x32To64(TenToPowerNine, (uint)low64);
                            pdecOut.Low = (uint)low64;
                            hi64 += low64 >> 32;
                            pdecOut.Mid = (uint)hi64;
                            hi64 >>= 32;
                            pdecOut.High = (uint)hi64;
                        }
                    }
                }
                else
                {
                    // Factor out powers of 10 to reduce the scale, if possible.
                    // The maximum number we could factor out would be 6.  This
                    // comes from the fact we have a 7-digit number, and the 
                    // MSD must be non-zero -- but the lower 6 digits could be 
                    // zero.  Note also the scale factor is never negative, so
                    // we can't scale by any more than the power we used to
                    // get the integer.
                    //
                    int lmax = iPower;
                    if (lmax > 6)
                        lmax = 6;

                    if ((ulMant & 0xF) == 0 && lmax >= 4)
                    {
                        const uint den = 10000;
                        uint div = ulMant / den;
                        if (ulMant == div * den)
                        {
                            ulMant = div;
                            iPower -= 4;
                            lmax -= 4;
                        }
                    }

                    if ((ulMant & 3) == 0 && lmax >= 2)
                    {
                        const uint den = 100;
                        uint div = ulMant / den;
                        if (ulMant == div * den)
                        {
                            ulMant = div;
                            iPower -= 2;
                            lmax -= 2;
                        }
                    }

                    if ((ulMant & 1) == 0 && lmax >= 1)
                    {
                        const uint den = 10;
                        uint div = ulMant / den;
                        if (ulMant == div * den)
                        {
                            ulMant = div;
                            iPower--;
                        }
                    }

                    flags |= (uint)iPower << ScaleShift;
                    pdecOut.Low = ulMant;
                }

                pdecOut.uflags = flags;
                return;

ThrowOverflow:
                throw new OverflowException(SR.Overflow_Decimal);
            }

            //**********************************************************************
            // VarDecFromR8 - Convert double to Decimal
            //**********************************************************************
#if PROJECTN // Workaround ProjectN bug #555233
            [MethodImplAttribute(MethodImplOptions.NoOptimization)]
#endif
            internal static void VarDecFromR8(double input, out Decimal pdecOut)
            {
                pdecOut = new Decimal();

                // The most we can scale by is 10^28, which is just slightly more
                // than 2^93.  So a float with an exponent of -94 could just
                // barely reach 0.5, but smaller exponents will always round to zero.
                //
                const uint DBLBIAS = 1022;
                int iExp = (int)(GetExponent(input) - DBLBIAS);
                if (iExp < -94)
                    return; // result should be zeroed out

                if (iExp > 96)
                    goto ThrowOverflow;

                uint flags = 0;
                if (input < 0)
                {
                    input = -input;
                    flags = SignMask;
                }

                // Round the input to a 15-digit integer.  The R8 format has
                // only 15 digits of precision, and we want to keep garbage digits
                // out of the Decimal were making.
                //
                // Calculate max power of 10 input value could have by multiplying 
                // the exponent by log10(2).  Using scaled integer multiplcation, 
                // log10(2) * 2 ^ 16 = .30103 * 65536 = 19728.3.
                //
                double dbl = input;
                int iPower = 14 - ((iExp * 19728) >> 16);
                // iPower is between -14 and 43

                if (iPower >= 0)
                {
                    // We have less than 15 digits, scale input up.
                    //
                    if (iPower > DEC_SCALE_MAX)
                        iPower = DEC_SCALE_MAX;

                    dbl *= s_doublePowers10[iPower];
                }
                else
                {
                    if (iPower != -1 || dbl >= 1E15)
                        dbl /= s_doublePowers10[-iPower];
                    else
                        iPower = 0; // didn't scale it
                }

                Debug.Assert(dbl < 1E15);
                if (dbl < 1E14 && iPower < DEC_SCALE_MAX)
                {
                    dbl *= 10;
                    iPower++;
                    Debug.Assert(dbl >= 1E14);
                }

                // Round to int64
                //
                ulong ulMant = (ulong)dbl;
                dbl -= (double)ulMant;  // difference between input & integer
                if (dbl > 0.5 || dbl == 0.5 && (ulMant & 1) != 0)
                    ulMant++;

                if (ulMant == 0)
                    return;  // result should be zeroed out

                if (iPower < 0)
                {
                    // Add -iPower factors of 10, -iPower <= (29 - 15) = 14.
                    //
                    iPower = -iPower;
                    if (iPower < 10)
                    {
                        var pow10 = s_powers10[iPower];
                        ulong low64 = UInt32x32To64((uint)ulMant, pow10);
                        ulong hi64 = UInt32x32To64((uint)(ulMant >> 32), pow10);
                        pdecOut.Low = (uint)low64;
                        hi64 += low64 >> 32;
                        pdecOut.Mid = (uint)hi64;
                        hi64 >>= 32;
                        pdecOut.High = (uint)hi64;
                    }
                    else
                    {
                        // Have a big power of 10.
                        //
                        Debug.Assert(iPower <= 14);
                        ulong low64 = UInt64x64To128(ulMant, s_ulongPowers10[iPower - 1], out ulong tmplong);
                        ulong hi64 = tmplong;
                        if (hi64 > uint.MaxValue)
                            goto ThrowOverflow;
                        pdecOut.Low64 = low64;
                        pdecOut.High = (uint)hi64;
                    }
                }
                else
                {
                    // Factor out powers of 10 to reduce the scale, if possible.
                    // The maximum number we could factor out would be 14.  This
                    // comes from the fact we have a 15-digit number, and the 
                    // MSD must be non-zero -- but the lower 14 digits could be 
                    // zero.  Note also the scale factor is never negative, so
                    // we can't scale by any more than the power we used to
                    // get the integer.
                    //
                    int lmax = iPower;
                    if (lmax > 14)
                        lmax = 14;

                    if ((byte)ulMant == 0 && lmax >= 8)
                    {
                        const uint den = 100000000;
                        ulong div = ulMant / den;
                        if (ulMant == div * den)
                        {
                            ulMant = div;
                            iPower -= 8;
                            lmax -= 8;
                        }
                    }

                    if (((uint)ulMant & 0xF) == 0 && lmax >= 4)
                    {
                        const uint den = 10000;
                        ulong div = ulMant / den;
                        if (ulMant == div * den)
                        {
                            ulMant = div;
                            iPower -= 4;
                            lmax -= 4;
                        }
                    }

                    if (((uint)ulMant & 3) == 0 && lmax >= 2)
                    {
                        const uint den = 100;
                        ulong div = ulMant / den;
                        if (ulMant == div * den)
                        {
                            ulMant = div;
                            iPower -= 2;
                            lmax -= 2;
                        }
                    }

                    if (((uint)ulMant & 1) == 0 && lmax >= 1)
                    {
                        const uint den = 10;
                        ulong div = ulMant / den;
                        if (ulMant == div * den)
                        {
                            ulMant = div;
                            iPower--;
                        }
                    }

                    flags |= (uint)iPower << ScaleShift;
                    pdecOut.Low64 = ulMant;
                }

                pdecOut.uflags = flags;
                return;

ThrowOverflow:
                throw new OverflowException(SR.Overflow_Decimal);
            }

            //**********************************************************************
            // VarR4ToDec - Convert Decimal to float
            //**********************************************************************
            internal static float VarR4FromDec(ref Decimal pdecIn)
            {
                return (float)VarR8FromDec(ref pdecIn);
            }

            //**********************************************************************
            // VarR8ToDec - Convert Decimal to double
            //**********************************************************************
            internal static double VarR8FromDec(ref Decimal pdecIn)
            {
                // Value taken via reverse engineering the double that corrisponds to 2^65. (oleaut32 has ds2to64 = DEFDS(0, 0, DBLBIAS + 65, 0))
                const double ds2to64 = 1.8446744073709552e+019;

                double dbl = ((double)pdecIn.Low64 +
                    (double)pdecIn.High * ds2to64) / s_doublePowers10[pdecIn.Scale];

                if (pdecIn.IsNegative)
                    dbl = -dbl;

                return dbl;
            }

            // VarDecAdd divides two decimal values.  On return, d1 contains the result
            // of the operation
            internal static void VarDecAdd(ref Decimal d1, ref Decimal d2)
            {
                DecAddSub(ref d1, ref d2, false);
            }

            // VarDecSub divides two decimal values.  On return, d1 contains the result
            // of the operation.
            internal static void VarDecSub(ref Decimal d1, ref Decimal d2)
            {
                DecAddSub(ref d1, ref d2, true);
            }

            // VarDecDiv divides two decimal values.  On return, d1 contains the result
            // of the operation.
            internal static unsafe void VarDecDiv(ref Decimal d1, ref Decimal d2)
            {
                Buf12 bufQuo, bufDivisor;
                _ = &bufQuo; // workaround for CS0165
                _ = &bufDivisor; // workaround for CS0165
                uint ulPwr;
                int iCurScale;

                int iScale = (sbyte)(d1.uflags - d2.uflags >> ScaleShift);
                bool fUnscale = false;
                uint ulTmp;

                if (d2.High == 0 && d2.Mid == 0)
                {
                    // Divisor is only 32 bits.  Easy divide.
                    //
                    uint den = d2.Low;
                    if (den == 0)
                        throw new DivideByZeroException(SR.Overflow_Decimal);

                    bufQuo.Low64 = d1.Low64;
                    bufQuo.U2 = d1.High;
                    uint remainder = Div96By32(ref bufQuo, den);

                    for (;;)
                    {
                        if (remainder == 0)
                        {
                            if (iScale < 0)
                            {
                                iCurScale = Math.Min(9, -iScale);
                                goto HaveScale;
                            }
                            break;
                        }

                        // We need to unscale if and only if we have a non-zero remainder
                        fUnscale = true;

                        // We have computed a quotient based on the natural scale 
                        // ( <dividend scale> - <divisor scale> ).  We have a non-zero 
                        // remainder, so now we should increase the scale if possible to 
                        // include more quotient bits.
                        // 
                        // If it doesn't cause overflow, we'll loop scaling by 10^9 and 
                        // computing more quotient bits as long as the remainder stays 
                        // non-zero.  If scaling by that much would cause overflow, we'll 
                        // drop out of the loop and scale by as much as we can.
                        // 
                        // Scaling by 10^9 will overflow if rgulQuo[2].rgulQuo[1] >= 2^32 / 10^9 
                        // = 4.294 967 296.  So the upper limit is rgulQuo[2] == 4 and 
                        // rgulQuo[1] == 0.294 967 296 * 2^32 = 1,266,874,889.7+.  Since 
                        // quotient bits in rgulQuo[0] could be all 1's, then 1,266,874,888 
                        // is the largest value in rgulQuo[1] (when rgulQuo[2] == 4) that is 
                        // assured not to overflow.
                        // 
                        if (iScale == DEC_SCALE_MAX || (iCurScale = SearchScale(ref bufQuo, iScale)) == 0)
                        {
                            // No more scaling to be done, but remainder is non-zero.
                            // Round quotient.
                            //
                            ulTmp = remainder << 1;
                            if (ulTmp < remainder || ulTmp >= den && (ulTmp > den || (bufQuo.U0 & 1) != 0))
                                goto RoundUp;
                            break;
                        }

                        HaveScale:
                        ulPwr = s_powers10[iCurScale];
                        iScale += iCurScale;

                        if (IncreaseScale(ref bufQuo, ulPwr) != 0)
                            goto ThrowOverflow;

                        ulong num = UInt32x32To64(remainder, ulPwr);
                        // TODO: https://github.com/dotnet/coreclr/issues/3439
                        uint div = (uint)(num / den);
                        remainder = (uint)num - div * den;

                        if (!Add32To96(ref bufQuo, div))
                        {
                            iScale = OverflowUnscale(ref bufQuo, iScale, remainder != 0);
                            break;
                        }
                    } // for (;;)
                }
                else
                {
                    // Divisor has bits set in the upper 64 bits.
                    //
                    // Divisor must be fully normalized (shifted so bit 31 of the most 
                    // significant ULONG is 1).  Locate the MSB so we know how much to 
                    // normalize by.  The dividend will be shifted by the same amount so 
                    // the quotient is not changed.
                    //
                    bufDivisor.Low64 = d2.Low64;
                    ulTmp = d2.High;
                    bufDivisor.U2 = ulTmp;
                    if (ulTmp == 0)
                        ulTmp = d2.Mid;

                    iCurScale = LeadingZeroCount(ulTmp);

                    // Shift both dividend and divisor left by iCurScale.
                    // 
                    Buf16 bufRem;
                    _ = &bufRem; // workaround for CS0165
                    bufRem.Low64 = d1.Low64 << iCurScale;
                    bufRem.High64 = (d1.Mid + ((ulong)d1.High << 32)) >> (31 - iCurScale) >> 1;

                    ulong divisor = bufDivisor.Low64 << iCurScale;

                    if (bufDivisor.U2 == 0)
                    {
                        // Have a 64-bit divisor in sdlDivisor.  The remainder 
                        // (currently 96 bits spread over 4 ULONGs) will be < divisor.
                        // 

                        bufQuo.U1 = Div96By64(ref *(Buf12*)&bufRem.U1, divisor);
                        bufQuo.U0 = Div96By64(ref *(Buf12*)&bufRem, divisor);

                        for (;;)
                        {
                            if (bufRem.Low64 == 0)
                            {
                                if (iScale < 0)
                                {
                                    iCurScale = Math.Min(9, -iScale);
                                    goto HaveScale64;
                                }
                                break;
                            }

                            // We need to unscale if and only if we have a non-zero remainder
                            fUnscale = true;

                            // Remainder is non-zero.  Scale up quotient and remainder by 
                            // powers of 10 so we can compute more significant bits.
                            // 
                            if (iScale == DEC_SCALE_MAX || (iCurScale = SearchScale(ref bufQuo, iScale)) == 0)
                            {
                                // No more scaling to be done, but remainder is non-zero.
                                // Round quotient.
                                //
                                ulong tmp = bufRem.Low64;
                                if ((long)tmp < 0 || (tmp <<= 1) > divisor ||
                                  (tmp == divisor && (bufQuo.U0 & 1) != 0))
                                    goto RoundUp;
                                break;
                            }

                            HaveScale64:
                            ulPwr = s_powers10[iCurScale];
                            iScale += iCurScale;

                            if (IncreaseScale(ref bufQuo, ulPwr) != 0)
                                goto ThrowOverflow;

                            IncreaseScale64(ref *(Buf12*)&bufRem, ulPwr);
                            ulTmp = Div96By64(ref *(Buf12*)&bufRem, divisor);
                            if (!Add32To96(ref bufQuo, ulTmp))
                            {
                                iScale = OverflowUnscale(ref bufQuo, iScale, bufRem.Low64 != 0);
                                break;
                            }
                        } // for (;;)
                    }
                    else
                    {
                        // Have a 96-bit divisor in rgulDivisor[].
                        //
                        // Start by finishing the shift left by iCurScale.
                        //
                        uint tmp = (uint)(bufDivisor.High64 >> (31 - iCurScale) >> 1);
                        bufDivisor.Low64 = divisor;
                        bufDivisor.U2 = tmp;

                        // The remainder (currently 96 bits spread over 4 ULONGs) 
                        // will be < divisor.
                        //
                        bufQuo.Low64 = Div128By96(ref bufRem, ref bufDivisor);

                        for (;;)
                        {
                            if ((bufRem.Low64 | bufRem.U2) == 0)
                            {
                                if (iScale < 0)
                                {
                                    iCurScale = Math.Min(9, -iScale);
                                    goto HaveScale96;
                                }
                                break;
                            }

                            // We need to unscale if and only if we have a non-zero remainder
                            fUnscale = true;

                            // Remainder is non-zero.  Scale up quotient and remainder by 
                            // powers of 10 so we can compute more significant bits.
                            //
                            if (iScale == DEC_SCALE_MAX || (iCurScale = SearchScale(ref bufQuo, iScale)) == 0)
                            {
                                // No more scaling to be done, but remainder is non-zero.
                                // Round quotient.
                                //
                                if ((int)bufRem.U2 < 0)
                                {
                                    goto RoundUp;
                                }

                                ulTmp = bufRem.U1 >> 31;
                                bufRem.Low64 <<= 1;
                                bufRem.U2 = (bufRem.U2 << 1) + ulTmp;

                                if (bufRem.U2 > bufDivisor.U2 || bufRem.U2 == bufDivisor.U2 &&
                                  (bufRem.Low64 > bufDivisor.Low64 || bufRem.Low64 == bufDivisor.Low64 &&
                                  (bufQuo.U0 & 1) != 0))
                                    goto RoundUp;
                                break;
                            }

                            HaveScale96:
                            ulPwr = s_powers10[iCurScale];
                            iScale += iCurScale;

                            if (IncreaseScale(ref bufQuo, ulPwr) != 0)
                                goto ThrowOverflow;

                            bufRem.U3 = IncreaseScale(ref *(Buf12*)&bufRem, ulPwr);
                            ulTmp = Div128By96(ref bufRem, ref bufDivisor);
                            if (!Add32To96(ref bufQuo, ulTmp))
                            {
                                iScale = OverflowUnscale(ref bufQuo, iScale, (bufRem.Low64 | bufRem.High64) != 0);
                                break;
                            }
                        } // for (;;)
                    }
                }

Unscale:
                ulong low64 = bufQuo.Low64;
                uint high = bufQuo.U2;
                // We need to unscale if and only if we have a non-zero remainder
                if (fUnscale)
                {
                    // Try extracting any extra powers of 10 we may have 
                    // added.  We do this by trying to divide out 10^8, 10^4, 10^2, and 10^1.
                    // If a division by one of these powers returns a zero remainder, then
                    // we keep the quotient.  If the remainder is not zero, then we restore
                    // the previous value.
                    // 
                    // Since 10 = 2 * 5, there must be a factor of 2 for every power of 10
                    // we can extract.  We use this as a quick test on whether to try a
                    // given power.
                    // 
                    while ((byte)low64 == 0 && iScale >= 8)
                    {
                        if (Div96ByConst100000000(ref bufQuo) == 0)
                        {
                            low64 = bufQuo.Low64;
                            high = bufQuo.U2;
                            iScale -= 8;
                        }
                        else
                        {
                            bufQuo.Low64 = low64;
                            bufQuo.U2 = high;
                            break;
                        }
                    }

                    if (((uint)low64 & 0xF) == 0 && iScale >= 4)
                    {
                        if (Div96ByConst10000(ref bufQuo) == 0)
                        {
                            low64 = bufQuo.Low64;
                            high = bufQuo.U2;
                            iScale -= 4;
                        }
                        else
                        {
                            bufQuo.Low64 = low64;
                            bufQuo.U2 = high;
                        }
                    }

                    if (((uint)low64 & 3) == 0 && iScale >= 2)
                    {
                        if (Div96ByConst100(ref bufQuo) == 0)
                        {
                            low64 = bufQuo.Low64;
                            high = bufQuo.U2;
                            iScale -= 2;
                        }
                        else
                        {
                            bufQuo.Low64 = low64;
                            bufQuo.U2 = high;
                        }
                    }

                    if (((uint)low64 & 1) == 0 && iScale >= 1 && Div96ByConst10(ref bufQuo) == 0)
                    {
                        low64 = bufQuo.Low64;
                        high = bufQuo.U2;
                        iScale -= 1;
                    }
                }

                d1.uflags = ((d1.uflags ^ d2.uflags) & SignMask) | ((uint)iScale << ScaleShift);
                d1.High = high;
                d1.Low64 = low64;
                return;

RoundUp:
                {
                    if (++bufQuo.Low64 == 0 && ++bufQuo.U2 == 0)
                    {
                        iScale = OverflowUnscale(ref bufQuo, iScale, true);
                    }
                    goto Unscale;
                }

ThrowOverflow:
                throw new OverflowException(SR.Overflow_Decimal);
            }

            //**********************************************************************
            // VarDecMod - Computes the remainder between two decimals
            //**********************************************************************
            internal static Decimal VarDecMod(ref Decimal d1, ref Decimal d2)
            {
                // OleAut doesn't provide a VarDecMod.            

                // In the operation x % y the sign of y does not matter. Result will have the sign of x.
                d2.uflags = (d2.uflags & ~SignMask) | (d1.uflags & SignMask);


                // This piece of code is to work around the fact that Dividing a decimal with 28 digits number by decimal which causes
                // causes the result to be 28 digits, can cause to be incorrectly rounded up.
                // eg. Decimal.MaxValue / 2 * Decimal.MaxValue will overflow since the division by 2 was rounded instead of being truncked.
                if (Math.Abs(d1) < Math.Abs(d2))
                {
                    return d1;
                }
                d1 -= d2;

                if (d1 == 0)
                {
                    // The sign of D1 will be wrong here. Fall through so that we still get a DivideByZeroException
                    d1.uflags = (d1.uflags & ~SignMask) | (d2.uflags & SignMask);
                }

                // Formula:  d1 - (RoundTowardsZero(d1 / d2) * d2)            
                Decimal dividedResult = Truncate(d1 / d2);
                Decimal multipliedResult = dividedResult * d2;
                Decimal result = d1 - multipliedResult;
                // See if the result has crossed 0
                if ((d1.uflags & SignMask) != (result.uflags & SignMask))
                {
                    if (NearNegativeZero <= result && result <= NearPositiveZero)
                    {
                        // Certain Remainder operations on decimals with 28 significant digits round
                        // to [+-]0.000000000000000000000000001m instead of [+-]0m during the intermediate calculations. 
                        // 'zero' results just need their sign corrected.
                        result.uflags = (result.uflags & ~SignMask) | (d1.uflags & SignMask);
                    }
                    else
                    {
                        // If the division rounds up because it runs out of digits, the multiplied result can end up with a larger
                        // absolute value and the result of the formula crosses 0. To correct it can add the divisor back.
                        result += d2;
                    }
                }

                return result;
            }

            internal enum RoundingMode
            {
                ToEven = 0,
                AwayFromZero = 1,
                Truncate = 2,
                Floor = 3,
                Ceiling = 4,
            }

            // Does an in-place round by the specified scale
            internal static void InternalRound(ref Decimal d, uint scale, RoundingMode mode)
            {
                // the scale becomes the desired decimal count
                d.uflags -= scale << ScaleShift;

                uint remainder, sticky = 0, power;
                // First divide the value by constant 10^9 up to three times
                while (scale >= MaxInt32Scale)
                {
                    scale -= MaxInt32Scale;

                    const uint divisor = TenToPowerNine;
                    uint n = d.uhi;
                    if (n == 0)
                    {
                        ulong tmp = d.Low64;
                        ulong div = tmp / divisor;
                        d.Low64 = div;
                        remainder = (uint)(tmp - div * divisor);
                    }
                    else
                    {
                        uint q;
                        d.uhi = q = n / divisor;
                        remainder = n - q * divisor;
                        n = d.umid;
                        if ((n | remainder) != 0)
                        {
                            d.umid = q = (uint)((((ulong)remainder << 32) | n) / divisor);
                            remainder = n - q * divisor;
                        }
                        n = d.ulo;
                        if ((n | remainder) != 0)
                        {
                            d.ulo = q = (uint)((((ulong)remainder << 32) | n) / divisor);
                            remainder = n - q * divisor;
                        }
                    }
                    power = divisor;
                    if (scale == 0)
                        goto checkRemainder;
                    sticky |= remainder;
                }

                {
                    power = s_powers10[scale];
                    // TODO: https://github.com/dotnet/coreclr/issues/3439
                    uint n = d.uhi;
                    if (n == 0)
                    {
                        ulong tmp = d.Low64;
                        if (tmp == 0)
                        {
                            if (mode <= RoundingMode.Truncate)
                                goto done;
                            remainder = 0;
                            goto checkRemainder;
                        }
                        ulong div = tmp / power;
                        d.Low64 = div;
                        remainder = (uint)(tmp - div * power);
                    }
                    else
                    {
                        uint q;
                        d.uhi = q = n / power;
                        remainder = n - q * power;
                        n = d.umid;
                        if ((n | remainder) != 0)
                        {
                            d.umid = q = (uint)((((ulong)remainder << 32) | n) / power);
                            remainder = n - q * power;
                        }
                        n = d.ulo;
                        if ((n | remainder) != 0)
                        {
                            d.ulo = q = (uint)((((ulong)remainder << 32) | n) / power);
                            remainder = n - q * power;
                        }
                    }
                }

checkRemainder:
                if (mode == RoundingMode.Truncate)
                    goto done;
                else if (mode == RoundingMode.ToEven)
                {
                    // To do IEEE rounding, we add LSB of result to sticky bits so either causes round up if remainder * 2 == last divisor.
                    remainder <<= 1;
                    if ((sticky | d.ulo & 1) != 0)
                        remainder++;
                    if (power >= remainder)
                        goto done;
                }
                else if (mode == RoundingMode.AwayFromZero)
                {
                    // Round away from zero at the mid point.
                    remainder <<= 1;
                    if (power > remainder)
                        goto done;
                }
                else if (mode == RoundingMode.Floor)
                {
                    // Round toward -infinity if we have chopped off a non-zero amount from a negative value.
                    if ((remainder | sticky) == 0 || !d.IsNegative)
                        goto done;
                }
                else
                {
                    Debug.Assert(mode == RoundingMode.Ceiling);
                    // Round toward infinity if we have chopped off a non-zero amount from a positive value.
                    if ((remainder | sticky) == 0 || d.IsNegative)
                        goto done;
                }
                if (++d.Low64 == 0)
                    d.uhi++;
done:
                return;
            }

#region Number Formatting helpers

            private static uint D32DivMod1E9(uint hi32, ref uint lo32)
            {
                ulong n = (ulong)hi32 << 32 | lo32;
                lo32 = (uint)(n / 1000000000);
                return (uint)(n % 1000000000);
            }

            internal static uint DecDivMod1E9(ref Decimal value)
            {
                return D32DivMod1E9(D32DivMod1E9(D32DivMod1E9(0,
                                                              ref value.uhi),
                                                 ref value.umid),
                                    ref value.ulo);
            }

            internal static void DecAddInt32(ref Decimal value, uint i)
            {
                if (D32AddCarry(ref value.ulo, i))
                {
                    if (D32AddCarry(ref value.umid, 1))
                        D32AddCarry(ref value.uhi, 1);
                }
            }

            private static bool D32AddCarry(ref uint value, uint i)
            {
                uint v = value;
                uint sum = v + i;
                value = sum;
                return (sum < v) || (sum < i);
            }

            internal static void DecMul10(ref Decimal value)
            {
                Decimal d = value;
                DecShiftLeft(ref value);
                DecShiftLeft(ref value);
                DecAdd(ref value, d);
                DecShiftLeft(ref value);
            }

            private static void DecShiftLeft(ref Decimal value)
            {
                uint c0 = (value.Low & 0x80000000) != 0 ? 1u : 0u;
                uint c1 = (value.Mid & 0x80000000) != 0 ? 1u : 0u;
                value.Low = value.Low << 1;
                value.Mid = (value.Mid << 1) | c0;
                value.High = (value.High << 1) | c1;
            }

            private static void DecAdd(ref Decimal value, Decimal d)
            {
                if (D32AddCarry(ref value.ulo, d.Low))
                {
                    if (D32AddCarry(ref value.umid, 1))
                        D32AddCarry(ref value.uhi, 1);
                }

                if (D32AddCarry(ref value.umid, d.Mid))
                    D32AddCarry(ref value.uhi, 1);

                D32AddCarry(ref value.uhi, d.High);
            }

#endregion

            struct PowerOvfl
            {
                public readonly uint Hi;
                public readonly ulong MidLo;

                public PowerOvfl(uint hi, uint mid, uint lo)
                {
                    Hi = hi;
                    MidLo = ((ulong)mid << 32) + lo;
                }
            }

            static readonly PowerOvfl[] PowerOvflValues = new[]
            {
                // This is a table of the largest values that can be in the upper two
                // ULONGs of a 96-bit number that will not overflow when multiplied
                // by a given power.  For the upper word, this is a table of 
                // 2^32 / 10^n for 1 <= n <= 8.  For the lower word, this is the
                // remaining fraction part * 2^32.  2^32 = 4294967296.
                //
                new PowerOvfl(429496729, 2576980377, 2576980377),  // 10^1 remainder 0.6
                new PowerOvfl(42949672,  4123168604, 687194767),   // 10^2 remainder 0.16
                new PowerOvfl(4294967,   1271310319, 2645699854),  // 10^3 remainder 0.616
                new PowerOvfl(429496,    3133608139, 694066715),   // 10^4 remainder 0.1616
                new PowerOvfl(42949,     2890341191, 2216890319),  // 10^5 remainder 0.51616
                new PowerOvfl(4294,      4154504685, 2369172679),  // 10^6 remainder 0.551616
                new PowerOvfl(429,       2133437386, 4102387834),  // 10^7 remainder 0.9551616
                new PowerOvfl(42,        4078814305, 410238783),   // 10^8 remainder 0.09991616
            };

            [StructLayout(LayoutKind.Explicit)]
            private struct Buf12
            {
                [FieldOffset(0 * 4)]
                public uint U0;
                [FieldOffset(1 * 4)]
                public uint U1;
                [FieldOffset(2 * 4)]
                public uint U2;

                [FieldOffset(0)]
                private ulong ulo64LE;
                [FieldOffset(4)]
                private ulong uhigh64LE;

                public ulong Low64
                {
#if BIGENDIAN && !MONO
                    get => ((ulong)U1 << 32) | U0;
                    set { U1 = (uint)(value >> 32); U0 = (uint)value; }
#else
                    get => ulo64LE;
                    set => ulo64LE = value;
#endif
                }

                /// <summary>
                /// U1-U2 combined (overlaps with Low64)
                /// </summary>
                public ulong High64
                {
#if BIGENDIAN && !MONO
                    get => ((ulong)U2 << 32) | U1;
                    set { U2 = (uint)(value >> 32); U1 = (uint)value; }
#else
                    get => uhigh64LE;
                    set => uhigh64LE = value;
#endif
                }
            }

            [StructLayout(LayoutKind.Explicit)]
            private struct Buf16
            {
                [FieldOffset(0 * 4)]
                public uint U0;
                [FieldOffset(1 * 4)]
                public uint U1;
                [FieldOffset(2 * 4)]
                public uint U2;
                [FieldOffset(3 * 4)]
                public uint U3;

                [FieldOffset(0 * 8)]
                private ulong ulo64LE;
                [FieldOffset(1 * 8)]
                private ulong uhigh64LE;

                public ulong Low64
                {
#if BIGENDIAN && !MONO
                    get => ((ulong)U1 << 32) | U0;
                    set { U1 = (uint)(value >> 32); U0 = (uint)value; }
#else
                    get => ulo64LE;
                    set => ulo64LE = value;
#endif
                }

                public ulong High64
                {
#if BIGENDIAN && !MONO
                    get => ((ulong)U3 << 32) | U2;
                    set { U3 = (uint)(value >> 32); U2 = (uint)value; }
#else
                    get => uhigh64LE;
                    set => uhigh64LE = value;
#endif
                }
            }

            [StructLayout(LayoutKind.Explicit)]
            private struct Buf24
            {
                [FieldOffset(0 * 4)]
                public uint U0;
                [FieldOffset(1 * 4)]
                public uint U1;
                [FieldOffset(2 * 4)]
                public uint U2;
                [FieldOffset(3 * 4)]
                public uint U3;
                [FieldOffset(4 * 4)]
                public uint U4;
                [FieldOffset(5 * 4)]
                public uint U5;

                [FieldOffset(0 * 8)]
                private ulong ulo64LE;
                [FieldOffset(1 * 8)]
                private ulong umid64LE;
                [FieldOffset(2 * 8)]
                private ulong uhigh64LE;

                public ulong Low64
                {
#if BIGENDIAN && !MONO
                    get => ((ulong)U1 << 32) | U0;
                    set { U1 = (uint)(value >> 32); U0 = (uint)value; }
#else
                    get => ulo64LE;
                    set => ulo64LE = value;
#endif
                }

                public ulong Mid64
                {
#if BIGENDIAN && !MONO
                    get => ((ulong)U3 << 32) | U2;
                    set { U3 = (uint)(value >> 32); U2 = (uint)value; }
#else
                    get => umid64LE;
                    set => umid64LE = value;
#endif
                }

                public ulong High64
                {
#if BIGENDIAN && !MONO
                    get => ((ulong)U5 << 32) | U4;
                    set { U5 = (uint)(value >> 32); U4 = (uint)value; }
#else
                    get => uhigh64LE;
                    set => uhigh64LE = value;
#endif
                }

                public int Length => 6;
            }
        }
#endif
    }
}
