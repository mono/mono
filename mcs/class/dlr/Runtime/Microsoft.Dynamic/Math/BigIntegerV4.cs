/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
#if FEATURE_NUMERICS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.Scripting.Utils;
using BigInt = System.Numerics.BigInteger;

namespace Microsoft.Scripting.Math {
    /// <summary>
    /// arbitrary precision integers
    /// </summary>
    [Serializable]
    public sealed class BigInteger : IFormattable, IComparable, IEquatable<BigInteger> {
        internal readonly BigInt Value;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigInteger Zero = new BigInteger((BigInt)0);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigInteger One = new BigInteger((BigInt)1);

        public BigInteger(BigInt value) {
            Value = value;
        }

        [CLSCompliant(false)]
        public static BigInteger Create(ulong v) {
            return new BigInteger(new BigInt(v));
        }

        [CLSCompliant(false)]
        public static BigInteger Create(uint v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(long v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(int v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(decimal v) {
            return new BigInteger(new BigInt(v));
        }

        public static BigInteger Create(byte[] v) {
            return new BigInteger(v);
        }

        public static BigInteger Create(double v) {
            return new BigInteger(new BigInt(v));
        }

        public static implicit operator BigInteger(byte i) {
            return new BigInteger((BigInt)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(sbyte i) {
            return new BigInteger((BigInt)i);
        }

        public static implicit operator BigInteger(short i) {
            return new BigInteger((BigInt)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(ushort i) {
            return new BigInteger((BigInt)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(uint i) {
            return new BigInteger((BigInt)i);
        }

        public static implicit operator BigInteger(int i) {
            return new BigInteger((BigInt)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(ulong i) {
            return new BigInteger((BigInt)i);
        }

        public static implicit operator BigInteger(long i) {
            return new BigInteger((BigInt)i);
        }

        public static implicit operator BigInteger(decimal self) {
            return new BigInteger((BigInt)self);
        }

        public static explicit operator BigInteger(double self) {
            return new BigInteger((BigInt)self);
        }

        public static explicit operator BigInteger(float self) {
            return new BigInteger((BigInt)self);
        }

        public static explicit operator double(BigInteger self) {
            return (double)self.Value;
        }

        public static explicit operator float(BigInteger self) {
            return (float)self.Value;
        }

        public static explicit operator decimal(BigInteger self) {
            return (decimal)self.Value;
        }

        public static explicit operator byte(BigInteger self) {
            return (byte)self.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator sbyte(BigInteger self) {
            return (sbyte)self.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator UInt16(BigInteger self) {
            return (UInt16)self.Value;
        }

        public static explicit operator Int16(BigInteger self) {
            return (Int16)self.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator UInt32(BigInteger self) {
            return (UInt32)self.Value;
        }

        public static explicit operator Int32(BigInteger self) {
            return (Int32)self.Value;
        }

        public static explicit operator Int64(BigInteger self) {
            return (Int64)self.Value;
        }

        [CLSCompliant(false)]
        public static explicit operator UInt64(BigInteger self) {
            return (UInt64)self.Value;
        }

        public static implicit operator BigInteger(BigInt value) {
            return new BigInteger(value);
        }

        public static implicit operator BigInt(BigInteger value) {
            return value.Value;
        }

        public BigInteger(BigInteger copy) {
            if (object.ReferenceEquals(copy, null)) {
                throw new ArgumentNullException("copy");
            }
            Value = copy.Value;
        }

        public BigInteger(byte[] data) {
            ContractUtils.RequiresNotNull(data, "data");

            Value = new BigInt(data);
        }

        public BigInteger(int sign, byte[] data) {
            ContractUtils.RequiresNotNull(data, "data");
            ContractUtils.Requires(sign >= -1 && sign <= +1, "sign");

            Value = new BigInt(data);
            if (sign < 0) {
                Value = -Value;
            }
        }
        
        [CLSCompliant(false)]
        public BigInteger(int sign, uint[] data) {
            ContractUtils.RequiresNotNull(data, "data");
            ContractUtils.Requires(sign >= -1 && sign <= +1, "sign");
            int length = GetLength(data);
            ContractUtils.Requires(length == 0 || sign != 0, "sign");
            if (length == 0) {
                Value = 0;
                return;
            }

            bool highest = (data[length - 1] & 0x80000000) != 0;
            byte[] bytes = new byte[length * 4 + (highest ? 1 : 0)];
            int j = 0;
            for (int i = 0; i < length; i++) {
                ulong w = data[i];
                bytes[j++] = (byte)(w & 0xff);
                bytes[j++] = (byte)((w >> 8) & 0xff);
                bytes[j++] = (byte)((w >> 16) & 0xff);
                bytes[j++] = (byte)((w >> 24) & 0xff);
            }

            Value = new BigInt(bytes);
            if (sign < 0) {
                Value = -Value;
            }
        }

        [CLSCompliant(false)]
        public uint[] GetWords() {
            return Value.GetWords();
        }

        public int GetBitCount() {
            return Value.GetBitCount();
        }

        public int GetWordCount() {
            return Value.GetWordCount();
        }

        public int GetByteCount() {
            return Value.GetByteCount();
        }

        /// <summary>
        /// Return the sign of this BigInteger: -1, 0, or 1.
        /// </summary>
        public int Sign {
            get {
                return Value.Sign;
            }
        }

        public bool AsInt64(out long ret) {
            if (Value >= Int64.MinValue && Value <= Int64.MaxValue) {
                ret = (long)Value;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public bool AsUInt32(out uint ret) {
            if (Value >= UInt32.MinValue && Value <= UInt32.MaxValue) {
                ret = (UInt32)Value;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public bool AsUInt64(out ulong ret) {
            if (Value >= UInt64.MinValue && Value <= UInt64.MaxValue) {
                ret = (UInt64)Value;
                return true;
            }
            ret = 0;
            return false;
        }

        public bool AsInt32(out int ret) {
            if (Value >= Int32.MinValue && Value <= Int32.MaxValue) {
                ret = (Int32)Value;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public uint ToUInt32() {
            return (uint)Value;
        }

        public int ToInt32() {
            return (int)Value;
        }

        public decimal ToDecimal() {
            return (decimal)Value;
        }

        [CLSCompliant(false)]
        public ulong ToUInt64() {
            return (ulong)Value;
        }

        public long ToInt64() {
            return (long)Value;
        }

        private static int GetLength(uint[] data) {
            int ret = data.Length - 1;
            while (ret >= 0 && data[ret] == 0) ret--;
            return ret + 1;
        }

        public static int Compare(BigInteger x, BigInteger y) {
            return BigInt.Compare(x.Value, y.Value);
        }

        public static bool operator ==(BigInteger x, int y) {
            return x.Value == y;
        }

        public static bool operator !=(BigInteger x, int y) {
            return x.Value != y;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public static bool operator ==(BigInteger x, double y) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }

            // we can hold all double values, but not all double values
            // can hold BigInteger values, and we may lose precision.  Convert
            // the double to a big int, then compare.

            if ((y % 1) != 0) return false;  // not a whole number, can't be equal

            return x.Value == (BigInt)y;
        }

        public static bool operator ==(double x, BigInteger y) {
            return y == x;
        }

        public static bool operator !=(BigInteger x, double y) {
            return !(x == y);
        }

        public static bool operator !=(double x, BigInteger y) {
            return !(x == y);
        }


        public static bool operator ==(BigInteger x, BigInteger y) {
            return Compare(x, y) == 0;
        }

        public static bool operator !=(BigInteger x, BigInteger y) {
            return Compare(x, y) != 0;
        }
        public static bool operator <(BigInteger x, BigInteger y) {
            return Compare(x, y) < 0;
        }
        public static bool operator <=(BigInteger x, BigInteger y) {
            return Compare(x, y) <= 0;
        }
        public static bool operator >(BigInteger x, BigInteger y) {
            return Compare(x, y) > 0;
        }
        public static bool operator >=(BigInteger x, BigInteger y) {
            return Compare(x, y) >= 0;
        }

        public static BigInteger Add(BigInteger x, BigInteger y) {
            return x + y;
        }

        public static BigInteger operator +(BigInteger x, BigInteger y) {
            return new BigInteger(x.Value + y.Value);
        }

        public static BigInteger Subtract(BigInteger x, BigInteger y) {
            return x - y;
        }

        public static BigInteger operator -(BigInteger x, BigInteger y) {
            return new BigInteger(x.Value - y.Value);
        }

        public static BigInteger Multiply(BigInteger x, BigInteger y) {
            return x * y;
        }

        public static BigInteger operator *(BigInteger x, BigInteger y) {
            return new BigInteger(x.Value * y.Value);
        }

        public static BigInteger Divide(BigInteger x, BigInteger y) {
            return x / y;
        }

        public static BigInteger operator /(BigInteger x, BigInteger y) {
            BigInteger dummy;
            return DivRem(x, y, out dummy);
        }

        public static BigInteger Mod(BigInteger x, BigInteger y) {
            return x % y;
        }

        public static BigInteger operator %(BigInteger x, BigInteger y) {
            BigInteger ret;
            DivRem(x, y, out ret);
            return ret;
        }

        public static BigInteger DivRem(BigInteger x, BigInteger y, out BigInteger remainder) {
            BigInt rem;
            BigInt result = BigInt.DivRem(x.Value, y.Value, out rem);
            remainder = new BigInteger(rem);
            return new BigInteger(result);
        }

        public static BigInteger BitwiseAnd(BigInteger x, BigInteger y) {
            return x & y;
        }

        public static BigInteger operator &(BigInteger x, BigInteger y) {
            return new BigInteger(x.Value & y.Value);
        }

        public static BigInteger BitwiseOr(BigInteger x, BigInteger y) {
            return x | y;
        }

        public static BigInteger operator |(BigInteger x, BigInteger y) {
            return new BigInteger(x.Value | y.Value);
        }

        public static BigInteger Xor(BigInteger x, BigInteger y) {
            return x ^ y;
        }

        public static BigInteger operator ^(BigInteger x, BigInteger y) {
            return new BigInteger(x.Value ^ y.Value);
        }

        public static BigInteger LeftShift(BigInteger x, int shift) {
            return x << shift;
        }

        public static BigInteger operator <<(BigInteger x, int shift) {
            return new BigInteger(x.Value << shift);
        }

        public static BigInteger RightShift(BigInteger x, int shift) {
            return x >> shift;
        }

        public static BigInteger operator >>(BigInteger x, int shift) {
            return new BigInteger(x.Value >> shift);
        }

        public static BigInteger Negate(BigInteger x) {
            return -x;
        }

        public static BigInteger operator -(BigInteger x) {
            return new BigInteger(-x.Value);
        }

        public BigInteger OnesComplement() {
            return ~this;
        }

        public static BigInteger operator ~(BigInteger x) {
            return new BigInteger(~x.Value);
        }

        public BigInteger Abs() {
            return new BigInteger(BigInt.Abs(Value));
        }

        public BigInteger Power(int exp) {
            return new BigInteger(BigInt.Pow(Value, exp));
        }

        public BigInteger ModPow(int power, BigInteger mod) {
            return new BigInteger(BigInt.ModPow(Value, power, mod.Value));
        }

        public BigInteger ModPow(BigInteger power, BigInteger mod) {
            return new BigInteger(BigInt.ModPow(Value, power.Value, mod.Value));
        }

        public BigInteger Square() {
            return this * this;
        }

#if !SILVERLIGHT
        public static BigInteger Parse(string str) {
            return new BigInteger(BigInt.Parse(str));
        }
#endif

        public override string ToString() {
            return ToString(10);
        }

        public string ToString(int @base) {
            return MathUtils.BigIntegerToString(GetWords(), Sign, @base, false);
        }

        public string ToString(string format) {
            return Value.ToString(format);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as BigInteger);
        }

        public bool Equals(BigInteger other) {
            if (object.ReferenceEquals(other, null)) return false;
            return this == other;
        }

        public bool IsNegative() {
            return Value.Sign < 0;
        }

        public bool IsZero() {
            return Value.Sign == 0;
        }

        public bool IsPositive() {
            return Value.Sign > 0;
        }

        public bool IsEven {
            get { return Value.IsEven; }
        }

        public bool IsPowerOfTwo {
            get {
                return Value.IsPowerOfTwo;
            }
        }

        public double Log(Double newBase) {
            return BigInt.Log(Value, newBase);
        }

        /// <summary>
        /// Calculates the natural logarithm of the BigInteger.
        /// </summary>
        public double Log() {
            return BigInt.Log(Value);
        }

        /// <summary>
        /// Calculates log base 10 of a BigInteger.
        /// </summary>
        public double Log10() {
            return BigInt.Log10(Value);
        }

#region IComparable Members

        public int CompareTo(object obj) {
            if (obj == null) {
                return 1;
            }
            BigInteger o = obj as BigInteger;
            if (object.ReferenceEquals(o, null)) {
                throw new ArgumentException("expected integer");
            }
            return Compare(this, o);
        }

        #endregion

        /// <summary>
        /// Return the value of this BigInteger as a little-endian twos-complement
        /// byte array, using the fewest number of bytes possible. If the value is zero,
        /// return an array of one byte whose element is 0x00.
        /// </summary>
        public byte[] ToByteArray() {
            return Value.ToByteArray();
        }

        public string ToString(IFormatProvider provider) {
            return Value.ToString(provider);
        }

#region IFormattable Members

        string IFormattable.ToString(string format, IFormatProvider formatProvider) {
            return Value.ToString(format, formatProvider);
        }

        #endregion
    }
}
#endif
