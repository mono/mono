/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Microsoft.Scripting.Math {
    /// <summary>
    /// arbitrary precision integers
    /// </summary>
    [Serializable]
    public sealed class BigInteger : IFormattable, IComparable, IConvertible, IEquatable<BigInteger> {
        private const int BitsPerDigit = 32;
        private const ulong Base = 0x100000000;

        // -1 if negative, +1 if positive, 0 if zero.
        private readonly short sign;

        // Non-null. data[0] is the least significant 32 bits.
        private readonly uint[] data;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigInteger Zero = new BigInteger(0, new uint[0]);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigInteger One = new BigInteger(+1, new uint[] { 1 });
        private const int bias = 1075;

        [CLSCompliant(false)]
        public static BigInteger Create(ulong v) {
            return new BigInteger(+1, (uint)v, (uint)(v >> BitsPerDigit));
        }

        [CLSCompliant(false)]
        public static BigInteger Create(uint v) {
            if (v == 0) return Zero;
            else if (v == 1) return One;
            else return new BigInteger(+1, v);
        }

        public static BigInteger Create(long v) {
            ulong x;
            int s = +1;
            if (v < 0) {
                x = (ulong)-v; s = -1;
            } else {
                x = (ulong)v;
            }

            return new BigInteger(s, (uint)x, (uint)(x >> BitsPerDigit));
        }

        public static BigInteger Create(int v) {
            if (v == 0) return Zero;
            else if (v == 1) return One;
            else if (v < 0) return new BigInteger(-1, (uint)-v);
            else return new BigInteger(+1, (uint)v);
        }

        private const Int32 DecimalScaleFactorMask = 0x00FF0000;
        private const Int32 DecimalSignMask = unchecked((Int32)0x80000000);

        public static BigInteger Create(decimal v) {
            // First truncate to get scale to 0 and extract bits
            int[] bits = Decimal.GetBits(Decimal.Truncate(v));

            Debug.Assert(bits.Length == 4 && (bits[3] & DecimalScaleFactorMask) == 0);

            int size = 3;
            while (size > 0 && bits[size - 1] == 0) size--;

            if (size == 0) {
                return BigInteger.Zero;
            }

            UInt32[] array = new UInt32[size];
            array[0] = (UInt32)bits[0];
            if (size > 1) array[1] = (UInt32)bits[1];
            if (size > 2) array[2] = (UInt32)bits[2];

            return new BigInteger(((bits[3] & DecimalSignMask) != 0) ? -1 : +1, array);
        }

        /// <summary>
        /// Create a BigInteger from a little-endian twos-complement byte array
        /// (inverse of ToByteArray())
        /// </summary>
        public static BigInteger Create(byte[] v) {
	    if (v == null)
		throw new ArgumentNullException ("v");
            if (v.Length == 0) return Create(0);

            int byteCount = v.Length;
            int unalignedBytes = byteCount % 4;
            int dwordCount = byteCount / 4 + (unalignedBytes == 0 ? 0 : 1);
            uint[] data = new uint[dwordCount];

            bool isNegative = (v[byteCount - 1] & 0x80) == 0x80;

            bool isZero = true;

            // Copy all dwords, except but don't do the last one if it's not a full four bytes
            int curDword, curByte, byteInDword;
            curByte = 3;
            for (curDword = 0; curDword < dwordCount - (unalignedBytes == 0 ? 0 : 1); curDword++) {
                byteInDword = 0;
                while (byteInDword < 4) {
                    if (v[curByte] != 0x00) isZero = false;
                    data[curDword] <<= 8;
                    data[curDword] |= v[curByte];
                    curByte--;
                    byteInDword++;
                }
                curByte += 8;
            }

            // Copy the last dword specially if it's not aligned
            if (unalignedBytes != 0) {
                if (isNegative) data[dwordCount - 1] = 0xffffffff;
                for (curByte = byteCount - 1; curByte >= byteCount - unalignedBytes; curByte--) {
                    if (v[curByte] != 0x00) isZero = false;
                    data[curDword] <<= 8;
                    data[curDword] |= v[curByte];
                }
            }

            if (isZero) return Zero;

            if (isNegative) {
                makeTwosComplement(data);
                return new BigInteger(-1, data);
            }
            return new BigInteger(1, data);
        }


        private static bool Negative(byte[] v) {
            return ((v[7] & 0x80) != 0);
        }

        private static ushort Exponent(byte[] v) {
            return (ushort)((((ushort)(v[7] & 0x7F)) << (ushort)4) | (((ushort)(v[6] & 0xF0)) >> 4));
        }

        private static ulong Mantissa(byte[] v) {
            uint i1 = ((uint)v[0] | ((uint)v[1] << 8) | ((uint)v[2] << 16) | ((uint)v[3] << 24));
            uint i2 = ((uint)v[4] | ((uint)v[5] << 8) | ((uint)(v[6] & 0xF) << 16));

            return (ulong)((ulong)i1 | ((ulong)i2 << 32));
        }

        public static BigInteger Create(double v) {
            if (Double.IsNaN(v) || Double.IsInfinity(v)) {
                throw new OverflowException();
            }

            byte[] bytes = System.BitConverter.GetBytes(v);
            ulong mantissa = Mantissa(bytes);
            if (mantissa == 0) {
                // 1.0 * 2**exp, we have a power of 2
                int exponent = Exponent(bytes);
                if (exponent == 0) return Zero;

                BigInteger res = Negative(bytes) ? Negate(One) : One;
                res = res << (exponent - 0x3ff);
                return res;
            } else {
                // 1.mantissa * 2**exp
                int exponent = Exponent(bytes);
                mantissa |= 0x10000000000000ul;
                BigInteger res = BigInteger.Create(mantissa);
                res = exponent > bias ? res << (exponent - bias) : res >> (bias - exponent);
                return Negative(bytes) ? res * (-1) : res;
            }
        }

        public static implicit operator BigInteger(byte i) {
            return Create((uint)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(sbyte i) {
            return Create((int)i);
        }

        public static implicit operator BigInteger(short i) {
            return Create((int)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(ushort i) {
            return Create((uint)i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(uint i) {
            return Create(i);
        }

        public static implicit operator BigInteger(int i) {
            return Create(i);
        }

        [CLSCompliant(false)]
        public static implicit operator BigInteger(ulong i) {
            return Create(i);
        }

        public static implicit operator BigInteger(long i) {
            return Create(i);
        }

        public static implicit operator BigInteger(decimal i) {
            return Create(i);
        }

        public static explicit operator BigInteger(double self) {
            return Create(self);
        }

        public static explicit operator BigInteger(float self) {
            return Create((double)self);
        }

        public static explicit operator double(BigInteger self) {
            if (object.ReferenceEquals(self, null)) {
                throw new ArgumentNullException("self");
            }
            return self.ToFloat64();
        }

        public static explicit operator float(BigInteger self) {
            if (object.ReferenceEquals(self, null)) {
                throw new ArgumentNullException("self");
            }
            return checked((float)self.ToFloat64());
        }

        public static explicit operator decimal(BigInteger self) {
            decimal res;
            if (self.AsDecimal(out res)) {
                return res;
            }
            throw new OverflowException();
        }

        public static explicit operator byte(BigInteger self) {
            int tmp;
            if (self.AsInt32(out tmp)) {
                return checked((byte)tmp);
            }
            throw new OverflowException();
        }

        [CLSCompliant(false)]
        public static explicit operator sbyte(BigInteger self) {
            int tmp;
            if (self.AsInt32(out tmp)) {
                return checked((sbyte)tmp);
            }
            throw new OverflowException();
        }

        [CLSCompliant(false)]
        public static explicit operator UInt16(BigInteger self) {
            int tmp;
            if (self.AsInt32(out tmp)) {
                return checked((UInt16)tmp);
            }
            throw new OverflowException();
        }

        public static explicit operator Int16(BigInteger self) {
            int tmp;
            if (self.AsInt32(out tmp)) {
                return checked((Int16)tmp);
            }
            throw new OverflowException();
        }

        [CLSCompliant(false)]
        public static explicit operator UInt32(BigInteger self) {
            uint tmp;
            if (self.AsUInt32(out tmp)) {
                return tmp;
            }
            throw new OverflowException();
        }

        public static explicit operator Int32(BigInteger self) {
            int tmp;
            if (self.AsInt32(out tmp)) {
                return tmp;
            }
            throw new OverflowException();
        }

        public static explicit operator Int64(BigInteger self) {
            long tmp;
            if (self.AsInt64(out tmp)) {
                return tmp;
            }
            throw new OverflowException();
        }

        [CLSCompliant(false)]
        public static explicit operator UInt64(BigInteger self) {
            ulong tmp;
            if (self.AsUInt64(out tmp)) {
                return tmp;
            }
            throw new OverflowException();
        }

        public BigInteger(BigInteger copy) {
            if (object.ReferenceEquals(copy, null)) {
                throw new ArgumentNullException("copy");
            }
            this.sign = copy.sign;
            this.data = copy.data;
        }

        [CLSCompliant(false)]
        public BigInteger(int sign, params uint[] data) {
	    if (data == null)
		throw new ArgumentNullException ("data");
            if (!(sign >= -1 && sign <= +1))
		throw new ArgumentException ("sign");
            int length = GetLength(data);
	    if (!(length == 0 || sign != 0))
		throw new ArgumentException ("sign");
            
            this.data = data;
            this.sign = (short)(length == 0 ? 0 : sign);
        }

        /// <summary>
        /// Return the magnitude of this BigInteger as an array of zero or more uints.
        /// Element zero is the value of the least significant four bytes, element one is
        /// the value of the four next most significant bytes, etc.
        /// 
        /// The returned data is the unsigned magnitude of the number. To determine the sign,
        /// use GetSign().
        /// 
        /// It is guaranteed that the highest element of the returned array is never zero.
        /// This means that if the value of this BigInteger is zero, a zero-length array
        /// is returned.
        /// </summary>
        [CLSCompliant(false)]
        public uint[] GetWords() {
            if (sign == 0) return new uint[0];
            int w = GetWordCount();
            uint[] bits = new uint[w];
            Array.Copy(data, bits, w);
            return bits;
        }

        [CLSCompliant(false)]
        public uint GetWord(int index) {
            return data[index];
        }

        public int GetBitCount() {
            if (IsZero()) {
                return 0;
            }
            int w = GetWordCount() - 1;
            uint b = data[w];
            Debug.Assert(b > 0);
            int result = w * 32;
            do {
                b >>= 1;
                result++;
            } while (b > 0);

            return result;
        }

        public int GetByteCount() {
            return (GetBitCount() + 7) / 8;
        }

        /// <summary>
        /// Return the sign of this BigInteger: -1, 0, or 1.
        /// </summary>
        public short Sign {
            get {
                return sign;
            }
        }

        public bool AsInt64(out long ret) {
            ret = 0;
            if (sign == 0) return true;
            if (GetWordCount() > 2) return false;
            if (data.Length == 1) {
                ret = sign * (long)data[0];
                return true;
            }
            ulong tmp = (((ulong)data[1]) << 32 | (ulong)data[0]);
            if (tmp > 0x8000000000000000) return false;
            if (tmp == 0x8000000000000000 && sign == 1) return false;
            ret = ((long)tmp) * sign;
            return true;
        }

        [CLSCompliant(false)]
        public bool AsUInt32(out uint ret) {
            ret = 0;
            if (sign == 0) return true;
            if (sign < 0) return false;
            if (GetWordCount() > 1) return false;
            ret = data[0];
            return true;
        }

        [CLSCompliant(false)]
        public bool AsUInt64(out ulong ret) {
            ret = 0;
            if (sign == 0) return true;
            if (sign < 0) return false;
            if (GetWordCount() > 2) return false;
            ret = (ulong)data[0];
            if (data.Length > 1) {
                ret |= ((ulong)data[1]) << 32;
            }
            return true;
        }

        public bool AsInt32(out int ret) {
            ret = 0;
            if (sign == 0) return true;
            if (GetWordCount() > 1) return false;
            if (data[0] > 0x80000000) return false;
            if (data[0] == 0x80000000 && sign == 1) return false;
            ret = (int)data[0];
            ret *= sign;
            return true;
        }

        public bool AsDecimal(out Decimal ret) {
            if (sign == 0) {
                ret = Decimal.Zero;
                return true;
            }

            int length = GetWordCount();
            if (length > 3) {
                ret = default(Decimal);
                return false;
            }

            int lo = 0, mi = 0, hi = 0;
            if (length > 2) hi = (Int32)data[2];
            if (length > 1) mi = (Int32)data[1];
            if (length > 0) lo = (Int32)data[0];

            ret = new Decimal(lo, mi, hi, sign < 0, 0);
            return true;
        }


        [CLSCompliant(false)]
        public uint ToUInt32() {
            uint ret;
            if (AsUInt32(out ret)) return ret;
            throw new OverflowException("big integer won't fit into uint");
        }

        public int ToInt32() {
            int ret;
            if (AsInt32(out ret)) return ret;
            throw new OverflowException("big integer won't fit into int");
        }

        public decimal ToDecimal() {
            decimal ret;
            if (AsDecimal(out ret)) return ret;
            throw new OverflowException("big integer won't fit into decimal");
        }

        [CLSCompliant(false)]
        public ulong ToUInt64() {
            ulong ret;
            if (AsUInt64(out ret)) return ret;
            throw new OverflowException("big integer won't fit into ulong");
        }

        public long ToInt64() {
            long ret;
            if (AsInt64(out ret)) return ret;
            throw new OverflowException("big integer won't fit into long");
        }

        public bool TryToFloat64(out double result) {
            return double.TryParse(ToString(10),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                out result);
        }

        public double ToFloat64() {
            return double.Parse(
                ToString(10),
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat
                );
        }

        public int GetWordCount() {
            return GetLength(data);
        }

        private static int GetLength(uint[] data) {
            int ret = data.Length - 1;
            while (ret >= 0 && data[ret] == 0) ret--;
            return ret + 1;
        }


        private static uint[] copy(uint[] v) {
            uint[] ret = new uint[v.Length];
            Array.Copy(v, ret, v.Length);
            return ret;
        }

        private static uint[] resize(uint[] v, int len) {
            if (v.Length == len) return v;
            uint[] ret = new uint[len];
            int n = System.Math.Min(v.Length, len);
            for (int i = 0; i < n; i++) {
                ret[i] = v[i];
            }
            return ret;
        }

        private static uint[] InternalAdd(uint[] x, int xl, uint[] y, int yl) {
            Debug.Assert(xl >= yl);
            uint[] z = new uint[xl];

            int i;
            ulong sum = 0;
            for (i = 0; i < yl; i++) {
                sum = sum + x[i] + y[i];
                z[i] = (uint)sum;
                sum >>= BitsPerDigit;
            }

            for (; i < xl && sum != 0; i++) {
                sum = sum + x[i];
                z[i] = (uint)sum;
                sum >>= BitsPerDigit;
            }
            if (sum != 0) {
                z = resize(z, xl + 1);
                z[i] = (uint)sum;
            } else {
                for (; i < xl; i++) {
                    z[i] = x[i];
                }
            }
            return z;
        }

        private static uint[] sub(uint[] x, int xl, uint[] y, int yl) {
            Debug.Assert(xl >= yl);
            uint[] z = new uint[xl];

            int i;
            bool borrow = false;
            for (i = 0; i < yl; i++) {
                uint xi = x[i];
                uint yi = y[i];
                if (borrow) {
                    if (xi == 0) {
                        xi = 0xffffffff;
                        borrow = true;
                    } else {
                        xi -= 1;
                        borrow = false;
                    }
                }
                if (yi > xi) borrow = true;
                z[i] = xi - yi;
            }

            if (borrow) {
                for (; i < xl; i++) {
                    uint xi = x[i];
                    z[i] = xi - 1;
                    if (xi != 0) { i++; break; }
                }
            }
            for (; i < xl; i++) {
                z[i] = x[i];
            }
            return z;  // may have leading zeros
        }

        private static uint[] add0(uint[] x, int xl, uint[] y, int yl) {
            if (xl >= yl) return InternalAdd(x, xl, y, yl);
            else return InternalAdd(y, yl, x, xl);
        }

        public static int Compare(BigInteger x, BigInteger y) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (object.ReferenceEquals(y, null)) {
                throw new ArgumentNullException("y");
            }
            if (x.sign == y.sign) {
                int xl = x.GetWordCount();
                int yl = y.GetWordCount();
                if (xl == yl) {
                    for (int i = xl - 1; i >= 0; i--) {
                        if (x.data[i] == y.data[i]) continue;
                        return x.data[i] > y.data[i] ? x.sign : -x.sign;
                    }
                    return 0;
                } else {
                    return xl > yl ? +x.sign : -x.sign;
                }
            } else {
                return x.sign > y.sign ? +1 : -1;
            }
        }

        public static bool operator ==(BigInteger x, int y) {
            return x == (BigInteger)y;
        }

        public static bool operator !=(BigInteger x, int y) {
            return !(x == y);
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

            return x == BigInteger.Create(y);
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
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (object.ReferenceEquals(y, null)) {
                throw new ArgumentNullException("y");
            }

            if (x.sign == y.sign) {
                return new BigInteger(x.sign, add0(x.data, x.GetWordCount(), y.data, y.GetWordCount()));
            } else {
                return x - new BigInteger(-y.sign, y.data);  //??? performance issue
            }
        }

        public static BigInteger Subtract(BigInteger x, BigInteger y) {
            return x - y;
        }

        public static BigInteger operator -(BigInteger x, BigInteger y) {
            int c = Compare(x, y);
            if (c == 0) return Zero;

            if (x.sign == y.sign) {
                uint[] z;
                switch (c * x.sign) {
                    case +1:
                        z = sub(x.data, x.GetWordCount(), y.data, y.GetWordCount());
                        break;
                    case -1:
                        z = sub(y.data, y.GetWordCount(), x.data, x.GetWordCount());
                        break;
                    default:
                        return Zero;
                }
                return new BigInteger(c, z);
            } else {
                uint[] z = add0(x.data, x.GetWordCount(), y.data, y.GetWordCount());
                return new BigInteger(c, z);
            }
        }

        public static BigInteger Multiply(BigInteger x, BigInteger y) {
            return x * y;
        }

        public static BigInteger operator *(BigInteger x, BigInteger y) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (object.ReferenceEquals(y, null)) {
                throw new ArgumentNullException("y");
            }
            int xl = x.GetWordCount();
            int yl = y.GetWordCount();
            int zl = xl + yl;
            uint[] xd = x.data, yd = y.data, zd = new uint[zl];

            for (int xi = 0; xi < xl; xi++) {
                uint xv = xd[xi];
                int zi = xi;
                ulong carry = 0;
                for (int yi = 0; yi < yl; yi++) {
                    carry = carry + ((ulong)xv) * yd[yi] + zd[zi];
                    zd[zi++] = (uint)carry;
                    carry >>= BitsPerDigit;
                }
                while (carry != 0) {
                    carry += zd[zi];
                    zd[zi++] = (uint)carry;
                    carry >>= BitsPerDigit;
                }
            }

            return new BigInteger(x.sign * y.sign, zd);
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

        private static int GetNormalizeShift(uint value) {
            int shift = 0;

            if ((value & 0xFFFF0000) == 0) { value <<= 16; shift += 16; }
            if ((value & 0xFF000000) == 0) { value <<= 8; shift += 8; }
            if ((value & 0xF0000000) == 0) { value <<= 4; shift += 4; }
            if ((value & 0xC0000000) == 0) { value <<= 2; shift += 2; }
            if ((value & 0x80000000) == 0) { value <<= 1; shift += 1; }

            return shift;
        }

        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Scripting.Math.BigInteger")]
        private static void TestNormalize(uint[] u, uint[] un, int shift) {
            BigInteger i = new BigInteger(1, u);
            BigInteger j = new BigInteger(1, un);
            BigInteger k = j >> shift;

            Debug.Assert(i == k);
        }

        [Conditional("DEBUG")]
        private static void TestDivisionStep(uint[] un, uint[] vn, uint[] q, uint[] u, uint[] v) {
            int n = GetLength(v);
            int shift = GetNormalizeShift(v[n - 1]);

            BigInteger uni = new BigInteger(1, un);
            BigInteger vni = new BigInteger(1, vn);
            BigInteger qi = new BigInteger(1, q);
            BigInteger ui = new BigInteger(1, u);

            BigInteger expected = vni * qi + uni;
            BigInteger usi = ui << shift;

            Debug.Assert(expected == usi);
        }

        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.Scripting.Math.BigInteger")]
        private static void TestResult(uint[] u, uint[] v, uint[] q, uint[] r) {
            BigInteger ui = new BigInteger(1, u);
            BigInteger vi = new BigInteger(1, v);
            BigInteger qi = new BigInteger(1, q);
            BigInteger ri = new BigInteger(1, r);

            BigInteger viqi = vi * qi;
            BigInteger expected = viqi + ri;
            Debug.Assert(ui == expected);
            Debug.Assert(ri < vi);
        }

        private static void Normalize(uint[] u, int l, uint[] un, int shift) {
            Debug.Assert(un.Length == l || un.Length == l + 1);
            Debug.Assert(un.Length == l + 1 || ((u[l - 1] << shift) >> shift) == u[l - 1]);
            Debug.Assert(0 <= shift && shift < 32);

            uint carry = 0;
            int i;
            if (shift > 0) {
                int rshift = BitsPerDigit - shift;
                for (i = 0; i < l; i++) {
                    uint ui = u[i];
                    un[i] = (ui << shift) | carry;
                    carry = ui >> rshift;
                }
            } else {
                for (i = 0; i < l; i++) {
                    un[i] = u[i];
                }
            }

            while (i < un.Length) {
                un[i++] = 0;
            }

            if (carry != 0) {
                Debug.Assert(l == un.Length - 1);
                un[l] = carry;
            }

            TestNormalize(u, un, shift);
        }

        private static void Unnormalize(uint[] un, out uint[] r, int shift) {
            Debug.Assert(0 <= shift && shift < 32);

            int length = GetLength(un);
            r = new uint[length];

            if (shift > 0) {
                int lshift = 32 - shift;
                uint carry = 0;
                for (int i = length - 1; i >= 0; i--) {
                    uint uni = un[i];
                    r[i] = (uni >> shift) | carry;
                    carry = (uni << lshift);
                }
            } else {
                for (int i = 0; i < length; i++) {
                    r[i] = un[i];
                }
            }

            TestNormalize(r, un, shift);
        }

        private static void DivModUnsigned(uint[] u, uint[] v, out uint[] q, out uint[] r) {
            int m = GetLength(u);
            int n = GetLength(v);

            if (n <= 1) {
                if (n == 0) {
                    throw new DivideByZeroException();
                }

                //  Divide by single digit
                //
                ulong rem = 0;
                uint v0 = v[0];
                q = new uint[m];
                r = new uint[1];

                for (int j = m - 1; j >= 0; j--) {
                    rem *= Base;
                    rem += u[j];

                    ulong div = rem / v0;
                    rem -= div * v0;
                    q[j] = (uint)div;
                }
                r[0] = (uint)rem;
            } else if (m >= n) {
                int shift = GetNormalizeShift(v[n - 1]);

                uint[] un = new uint[m + 1];
                uint[] vn = new uint[n];

                Normalize(u, m, un, shift);
                Normalize(v, n, vn, shift);

                q = new uint[m - n + 1];
                r = null;

                TestDivisionStep(un, vn, q, u, v);

                //  Main division loop
                //
                for (int j = m - n; j >= 0; j--) {
                    ulong rr, qq;
                    int i;

                    rr = Base * un[j + n] + un[j + n - 1];
                    qq = rr / vn[n - 1];
                    rr -= qq * vn[n - 1];

                    Debug.Assert((Base * un[j + n] + un[j + n - 1]) == qq * vn[n - 1] + rr);

                    for (; ; ) {
                        // Estimate too big ?
                        //
                        if ((qq >= Base) || (qq * vn[n - 2] > (rr * Base + un[j + n - 2]))) {
                            qq--;
                            rr += (ulong)vn[n - 1];
                            if (rr < Base) continue;
                        }
                        break;
                    }

                    Debug.Assert((Base * un[j + n] + un[j + n - 1]) == qq * vn[n - 1] + rr);

                    //  Multiply and subtract
                    //
                    long b = 0;
                    long t = 0;
                    for (i = 0; i < n; i++) {
                        ulong p = vn[i] * qq;
                        t = (long)un[i + j] - (long)(uint)p - b;
                        un[i + j] = (uint)t;
                        p >>= 32;
                        t >>= 32;
                        Debug.Assert(t == 0 || t == -1 || t == -2);
                        b = (long)p - t;
                    }
                    t = (long)un[j + n] - b;
                    un[j + n] = (uint)t;

                    //  Store the calculated value
                    //
                    q[j] = (uint)qq;

                    //  Add back vn[0..n] to un[j..j+n]
                    //
                    if (t < 0) {
                        q[j]--;
                        ulong c = 0;
                        for (i = 0; i < n; i++) {
                            c = (ulong)vn[i] + un[j + i] + c;
                            un[j + i] = (uint)c;
                            c >>= 32;
                        }
                        c += (ulong)un[j + n];
                        un[j + n] = (uint)c;
                    }

                    TestDivisionStep(un, vn, q, u, v);
                }

                Unnormalize(un, out r, shift);

                //  Test normalized value ... Call TestNormalize
                //  only pass the values in different order.
                //
                TestNormalize(r, un, shift);
            } else {
                q = new uint[] { 0 };
                r = u;
            }

            TestResult(u, v, q, r);
        }

        public static BigInteger DivRem(BigInteger x, BigInteger y, out BigInteger remainder) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (object.ReferenceEquals(y, null)) {
                throw new ArgumentNullException("y");
            }

            uint[] q;
            uint[] r;

            DivModUnsigned(x.data, y.data, out q, out r);

            remainder = new BigInteger(x.sign, r);
            return new BigInteger(x.sign * y.sign, q);
        }

        private static uint extend(uint v, ref bool seenNonZero) {
            if (seenNonZero) {
                return ~v;
            } else {
                if (v == 0) {
                    return 0;
                } else {
                    seenNonZero = true;
                    return ~v + 1;
                }
            }
        }

        private static uint getOne(bool isNeg, uint[] data, int i, ref bool seenNonZero) {
            if (i < data.Length) {
                uint ret = data[i];
                return isNeg ? extend(ret, ref seenNonZero) : ret;
            } else {
                return isNeg ? uint.MaxValue : 0;
            }
        }

        /// <summary>
        /// Do an in-place twos complement of d and also return the result.
        /// </summary>
        private static uint[] makeTwosComplement(uint[] d) {
            // first do complement and +1 as long as carry is needed
            int i = 0;
            uint v = 0;
            for (; i < d.Length; i++) {
                v = ~d[i] + 1;
                d[i] = v;
                if (v != 0) { i++; break; }
            }

            if (v != 0) {
                // now ones complement is sufficient
                for (; i < d.Length; i++) {
                    d[i] = ~d[i];
                }
            } else {
                //??? this is weird
                d = resize(d, d.Length + 1);
                d[d.Length - 1] = 1;
            }
            return d;
        }

        public static BigInteger BitwiseAnd(BigInteger x, BigInteger y) {
            return x & y;
        }

        public static BigInteger operator &(BigInteger x, BigInteger y) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (object.ReferenceEquals(y, null)) {
                throw new ArgumentNullException("y");
            }
            int xl = x.GetWordCount(), yl = y.GetWordCount();
            uint[] xd = x.data, yd = y.data;

            int zl = System.Math.Max(xl, yl);
            uint[] zd = new uint[zl];

            bool negx = x.sign == -1, negy = y.sign == -1;
            bool seenNonZeroX = false, seenNonZeroY = false;
            for (int i = 0; i < zl; i++) {
                uint xu = getOne(negx, xd, i, ref seenNonZeroX);
                uint yu = getOne(negy, yd, i, ref seenNonZeroY);
                zd[i] = xu & yu;
            }

            if (negx && negy) {

                return new BigInteger(-1, makeTwosComplement(zd));
            } else if (negx || negy) {
                return new BigInteger(+1, zd);
            } else {
                return new BigInteger(+1, zd);
            }
        }

        public static BigInteger BitwiseOr(BigInteger x, BigInteger y) {
            return x | y;
        }

        public static BigInteger operator |(BigInteger x, BigInteger y) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (object.ReferenceEquals(y, null)) {
                throw new ArgumentNullException("y");
            }
            int xl = x.GetWordCount(), yl = y.GetWordCount();
            uint[] xd = x.data, yd = y.data;

            int zl = System.Math.Max(xl, yl);
            uint[] zd = new uint[zl];

            bool negx = x.sign == -1, negy = y.sign == -1;
            bool seenNonZeroX = false, seenNonZeroY = false;
            for (int i = 0; i < zl; i++) {
                uint xu = getOne(negx, xd, i, ref seenNonZeroX);
                uint yu = getOne(negy, yd, i, ref seenNonZeroY);
                zd[i] = xu | yu;
            }

            if (negx && negy) {
                return new BigInteger(-1, makeTwosComplement(zd));
            } else if (negx || negy) {
                return new BigInteger(-1, makeTwosComplement(zd));
            } else {
                return new BigInteger(+1, zd);
            }
        }

        public static BigInteger Xor(BigInteger x, BigInteger y) {
            return x ^ y;
        }

        public static BigInteger operator ^(BigInteger x, BigInteger y) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (object.ReferenceEquals(y, null)) {
                throw new ArgumentNullException("y");
            }
            int xl = x.GetWordCount(), yl = y.GetWordCount();
            uint[] xd = x.data, yd = y.data;

            int zl = System.Math.Max(xl, yl);
            uint[] zd = new uint[zl];

            bool negx = x.sign == -1, negy = y.sign == -1;
            bool seenNonZeroX = false, seenNonZeroY = false;
            for (int i = 0; i < zl; i++) {
                uint xu = getOne(negx, xd, i, ref seenNonZeroX);
                uint yu = getOne(negy, yd, i, ref seenNonZeroY);
                zd[i] = xu ^ yu;
            }

            if (negx && negy) {
                return new BigInteger(+1, zd);
            } else if (negx || negy) {
                return new BigInteger(-1, makeTwosComplement(zd));
            } else {
                return new BigInteger(+1, zd);
            }
        }

        public static BigInteger LeftShift(BigInteger x, int shift) {
            return x << shift;
        }

        public static BigInteger operator <<(BigInteger x, int shift) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (shift == 0) return x;
            else if (shift < 0) return x >> -shift;

            int digitShift = shift / BitsPerDigit;
            int smallShift = shift - (digitShift * BitsPerDigit);

            int xl = x.GetWordCount();
            uint[] xd = x.data;
            int zl = xl + digitShift + 1;
            uint[] zd = new uint[zl];

            if (smallShift == 0) {
                for (int i = 0; i < xl; i++) {
                    zd[i + digitShift] = xd[i];
                }
            } else {
                int carryShift = BitsPerDigit - smallShift;
                uint carry = 0;
                int i;
                for (i = 0; i < xl; i++) {
                    uint rot = xd[i];
                    zd[i + digitShift] = rot << smallShift | carry;
                    carry = rot >> carryShift;
                }
                zd[i + digitShift] = carry;
            }
            return new BigInteger(x.sign, zd);
        }

        public static BigInteger RightShift(BigInteger x, int shift) {
            return x >> shift;
        }

        public static BigInteger operator >>(BigInteger x, int shift) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            if (shift == 0) return x;
            else if (shift < 0) return x << -shift;

            int digitShift = shift / BitsPerDigit;
            int smallShift = shift - (digitShift * BitsPerDigit);

            int xl = x.GetWordCount();
            uint[] xd = x.data;
            int zl = xl - digitShift;
            if (zl < 0) zl = 0;
            uint[] zd = new uint[zl];

            if (smallShift == 0) {
                for (int i = xl - 1; i >= digitShift; i--) {
                    zd[i - digitShift] = xd[i];
                }
            } else {
                int carryShift = BitsPerDigit - smallShift;
                uint carry = 0;
                for (int i = xl - 1; i >= digitShift; i--) {
                    uint rot = xd[i];
                    zd[i - digitShift] = rot >> smallShift | carry;
                    carry = rot << carryShift;
                }
            }
            return new BigInteger(x.sign, zd);
        }

        public static BigInteger Negate(BigInteger x) {
            return -x;
        }

        public static BigInteger operator -(BigInteger x) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            return new BigInteger(-x.sign, x.data);
        }

        public BigInteger OnesComplement() {
            return ~this;
        }

        public static BigInteger operator ~(BigInteger x) {
            if (object.ReferenceEquals(x, null)) {
                throw new ArgumentNullException("x");
            }
            return -(x + One);
        }

        public BigInteger Abs() {
            if (this.sign == -1) return -this;
            else return this;
        }

        public BigInteger Power(int exp) {
            if (exp == 0) return One;
            if (exp < 0) {
                throw new ArgumentOutOfRangeException("exp", "exp must be >= 0");
            }
            BigInteger factor = this;
            BigInteger result = One;
            while (exp != 0) {
                if ((exp & 1) != 0) result = result * factor;
                if (exp == 1) break;  // avoid costly factor.square()
                factor = factor.Square();
                exp >>= 1;
            }
            return result;
        }

        public BigInteger ModPow(int power, BigInteger mod) {
            if (object.ReferenceEquals(mod, null)) {
                throw new ArgumentNullException("mod");
            }

            if (power < 0) {
                throw new ArgumentOutOfRangeException("power", "power must be >= 0");
            }
            BigInteger factor = this;
            BigInteger result = One % mod; // Handle special case of power=0, mod=1
            while (power != 0) {
                if ((power & 1) != 0) {
                    result = result * factor;
                    result = result % mod;
                }
                if (power == 1) break;  // avoid costly factor.Square()
                factor = factor.Square();
                factor = factor % mod;
                power >>= 1;
            }
            return result;
        }

        public BigInteger ModPow(BigInteger power, BigInteger mod) {
            if (object.ReferenceEquals(power, null)) {
                throw new ArgumentNullException("power");
            }
            if (object.ReferenceEquals(mod, null)) {
                throw new ArgumentNullException("mod");
            }

            if (power < 0) {
                throw new ArgumentOutOfRangeException("power", "power must be >= 0");
            }

            BigInteger factor = this;
            BigInteger result = One % mod;
            while (power != Zero) {
                if (power.IsOdd()) {
                    result = result * factor;
                    result = result % mod;
                }
                if (power == One) break;  // avoid costly factor.Square()
                factor = factor.Square();
                factor = factor % mod;
                power >>= 1;
            }
            return result;
        }

        public BigInteger Square() {
            return this * this;
        }

        public override string ToString() {
            return ToString(10);
        }

        // generated by scripts/radix_generator.py
        private static readonly uint[] maxCharsPerDigit = { 0, 0, 31, 20, 15, 13, 12, 11, 10, 10, 9, 9, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };
        private static readonly uint[] groupRadixValues = { 0, 0, 2147483648, 3486784401, 1073741824, 1220703125, 2176782336, 1977326743, 1073741824, 3486784401, 1000000000, 2357947691, 429981696, 815730721, 1475789056, 2562890625, 268435456, 410338673, 612220032, 893871739, 1280000000, 1801088541, 2494357888, 3404825447, 191102976, 244140625, 308915776, 387420489, 481890304, 594823321, 729000000, 887503681, 1073741824, 1291467969, 1544804416, 1838265625, 2176782336 };

	public static ArgumentOutOfRangeException MakeArgumentOutOfRangeException(string paramName, object actualValue, string message) {
		return new ArgumentOutOfRangeException(paramName, string.Format("{0} (actual value is '{1}')", message, actualValue));
	}
	
        internal static string BigIntegerToString(uint[] d, int sign, int radix) {
            if (radix < 2) {
		    throw MakeArgumentOutOfRangeException("radix", radix, "radix must be >= 2");
            }
            if (radix > 36) {
                throw MakeArgumentOutOfRangeException("radix", radix, "radix must be <= 36");
            }

            int dl = d.Length;
            if (dl == 0) {
                return "0";
            }

            List<uint> digitGroups = new List<uint>();

            uint groupRadix = groupRadixValues[radix];
            while (dl > 0) {
                uint rem = div(d, ref dl, groupRadix);
                digitGroups.Add(rem);
            }

            StringBuilder ret = new StringBuilder();
            if (sign == -1) {
                ret.Append("-");
            }

            int digitIndex = digitGroups.Count - 1;

            char[] tmpDigits = new char[maxCharsPerDigit[radix]];

            AppendRadix((uint)digitGroups[digitIndex--], (uint)radix, tmpDigits, ret, false);
            while (digitIndex >= 0) {
                AppendRadix((uint)digitGroups[digitIndex--], (uint)radix, tmpDigits, ret, true);
            }
            return ret.Length == 0 ? "0" : ret.ToString();
        }

        private static uint div(uint[] n, ref int nl, uint d) {
            ulong rem = 0;
            int i = nl;
            bool seenNonZero = false;
            while (--i >= 0) {
                rem <<= BitsPerDigit;
                rem |= n[i];
                uint v = (uint)(rem / d);
                n[i] = v;
                if (v == 0) {
                    if (!seenNonZero) nl--;
                } else {
                    seenNonZero = true;
                }
                rem %= d;
            }
            return (uint)rem;
        }

        private static void AppendRadix(uint rem, uint radix, char[] tmp, StringBuilder buf, bool leadingZeros) {
            const string symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int digits = tmp.Length;
            int i = digits;
            while (i > 0 && (leadingZeros || rem != 0)) {
                uint digit = rem % radix;
                rem /= radix;
                tmp[--i] = symbols[(int)digit];
            }
            if (leadingZeros) buf.Append(tmp);
            else buf.Append(tmp, i, digits - i);
        }

	public string ToString(int radix) {
            return BigIntegerToString(copy(data), sign, radix);
        }

        public override int GetHashCode() {
            // The Object.GetHashCode function needs to be consistent with the Object.Equals function.
            // Languages that build on top of this may have a more flexible equality function and 
            // so may not be able to use this hash function directly.
            // For example, Python allows BigInteger(10) == int32(10), so hashing a BigInt over the Int32
            // domain should return the same value as a hash of the Int32.

            // If this is in the int32 range, this hash function returns the integer.
            if (data.Length == 0) {
                return 0;
            }

            // Add up all uints. We want to incorporate all bits to get good hash distribution. 
            uint total = 0;
            foreach (uint x in data) {
                total = unchecked(total + x);
            }

            int hash = unchecked((int)total);

            // The sign is not part of the data array, so explicitly incorporate that.
            // This is also needed to ensure that hash(-x) == -x for int32.
            if (IsNegative()) {
                return unchecked(-hash);
            } else {
                return hash;
            }
        }

        public override bool Equals(object obj) {
            return Equals(obj as BigInteger);
        }

        public bool Equals(BigInteger other) {
            if (object.ReferenceEquals(other, null)) return false;
            return this == other;
        }


        public bool IsNegative() {
            return sign < 0;
        }

        public bool IsZero() {
            return sign == 0;
        }

        public bool IsPositive() {
            return sign > 0;
        }

        private bool IsOdd() {
            // must have the lowest-order bit set to 1
            return (data != null && data.Length > 0 && ((data[0] & 1) != 0));
        }


        public double Log(Double newBase) {
            if (IsNegative() || newBase == 1.0D || this == Zero || (newBase == 0.0D && this != One)) {
                return Double.NaN;
            } else if (newBase == Double.PositiveInfinity) {
                return this == One ? 0.0D : Double.NaN;
            }

            int length = GetLength(data) - 1;
            int bitCount = -1;
            for (int curBit = 31; curBit >= 0; curBit--) {
                if ((data[length] & (1 << curBit)) != 0) {
                    bitCount = curBit + length * 32;
                    break;
                }
            }

            long bitlen = bitCount;
            Double c = 0, d = 1;

            BigInteger testBit = BigInteger.One;
            long tempBitlen = bitlen;
            while (tempBitlen > Int32.MaxValue) {
                testBit = testBit << Int32.MaxValue;
                tempBitlen -= Int32.MaxValue;
            }
            testBit = testBit << (int)tempBitlen;

            for (long curbit = bitlen; curbit >= 0; --curbit) {
                if ((this & testBit) != BigInteger.Zero)
                    c += d;
                d *= 0.5;
                testBit = testBit >> 1;
            }
            return (System.Math.Log(c) + System.Math.Log(2) * bitlen) / System.Math.Log(newBase);
        }

        /// <summary>
        /// Calculates the natural logarithm of the BigInteger.
        /// </summary>
        public double Log() {
            return Log(System.Math.E);
        }

        /// <summary>
        /// Calculates log base 10 of a BigInteger.
        /// </summary>
        public double Log10() {
            return Log(10);
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

        #region IConvertible Members

        public TypeCode GetTypeCode() {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider) {
            return this != Zero;
        }

        public byte ToByte(IFormatProvider provider) {
            uint ret;
            if (AsUInt32(out ret) && (ret & ~0xFF) == 0) {
                return (byte)ret;
            }
            throw new OverflowException("big integer won't fit into byte");
        }

        /// <summary>
        /// Return the value of this BigInteger as a little-endian twos-complement
        /// byte array, using the fewest number of bytes possible. If the value is zero,
        /// return an array of one byte whose element is 0x00.
        /// </summary>
        public byte[] ToByteArray() {
            // We could probably make this more efficient by eliminating one of the passes.
            // The current code does one pass for uint array -> byte array conversion,
            // and then a another pass to remove unneeded bytes at the top of the array.
            if (0 == sign) return new byte[] { 0 };

            uint[] dwords;
            byte highByte;

            if (-1 == sign) {
                dwords = (uint[])this.data.Clone();
                makeTwosComplement(dwords);
                highByte = 0xff;
            } else {
                dwords = this.data;
                highByte = 0x00;
            }

            byte[] bytes = new byte[4 * dwords.Length];
            int curByte = 0;
            uint dword;
            for (int i = 0; i < dwords.Length; i++) {
                dword = dwords[i];
                for (int j = 0; j < 4; j++) {
                    bytes[curByte++] = (byte)(dword & 0xff);
                    dword >>= 8;
                }
            }

            // find highest significant byte
            int msb;
            for (msb = bytes.Length - 1; msb > 0; msb--) {
                if (bytes[msb] != highByte) break;
            }
            // ensure high bit is 0 if positive, 1 if negative
            bool needExtraByte = (bytes[msb] & 0x80) != (highByte & 0x80);

            byte[] trimmedBytes = new byte[msb + 1 + (needExtraByte ? 1 : 0)];
            Array.Copy(bytes, trimmedBytes, msb + 1);

            if (needExtraByte) trimmedBytes[trimmedBytes.Length - 1] = highByte;

            return trimmedBytes;
        }

        public char ToChar(IFormatProvider provider) {
            int ret;
            if (AsInt32(out ret) && (ret <= Char.MaxValue) && (ret >= Char.MinValue)) {
                return (char)ret;
            }
            throw new OverflowException("big integer won't fit into char");
        }

        public DateTime ToDateTime(IFormatProvider provider) {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider) {
            decimal ret;
            if (AsDecimal(out ret)) return ret;
            throw new OverflowException("big integer won't fit into decimal");
        }

        public double ToDouble(IFormatProvider provider) {
            return ToFloat64();
        }

        public short ToInt16(IFormatProvider provider) {
            int ret;
            if (AsInt32(out ret) && (ret <= short.MaxValue) && (ret >= short.MinValue)) {
                return (short)ret;
            }
            throw new OverflowException("big integer won't fit into short");
        }

        public int ToInt32(IFormatProvider provider) {
            int ret;
            if (AsInt32(out ret)) {
                return ret;
            }
            throw new OverflowException("big integer won't fit into int");
        }

        public long ToInt64(IFormatProvider provider) {
            long ret;
            if (AsInt64(out ret)) {
                return ret;
            }
            throw new OverflowException("big integer won't fit into long");
        }

        [CLSCompliant(false)]
        public sbyte ToSByte(IFormatProvider provider) {
            int ret;
            if (AsInt32(out ret) && (ret <= sbyte.MaxValue) && (ret >= sbyte.MinValue)) {
                return (sbyte)ret;
            }
            throw new OverflowException("big integer won't fit into sbyte");
        }

        public float ToSingle(IFormatProvider provider) {
            return checked((float)ToDouble(provider));
        }

        public string ToString(IFormatProvider provider) {
            return ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider) {
            if (conversionType == typeof(BigInteger)) {
                return this;
            }
            throw new NotImplementedException();
        }

        [CLSCompliant(false)]
        public ushort ToUInt16(IFormatProvider provider) {
            uint ret;
            if (AsUInt32(out ret) && ret <= ushort.MaxValue) {
                return (ushort)ret;
            }
            throw new OverflowException("big integer won't fit into ushort");
        }

        [CLSCompliant(false)]
        public uint ToUInt32(IFormatProvider provider) {
            uint ret;
            if (AsUInt32(out ret)) {
                return ret;
            }
            throw new OverflowException("big integer won't fit into uint");
        }

        [CLSCompliant(false)]
        public ulong ToUInt64(IFormatProvider provider) {
            ulong ret;
            if (AsUInt64(out ret)) {
                return ret;
            }
            throw new OverflowException("big integer won't fit into ulong");
        }

        #endregion

        #region IFormattable Members

        string IFormattable.ToString(string format, IFormatProvider formatProvider) {
            if (format == null) return this.ToString();

            switch (format[0]) {
                case 'd':
                case 'D':
                    if (format.Length > 1) {
                        int precision = Convert.ToInt32(format.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        string baseStr = ToString(10);
                        if (baseStr.Length < precision) {
                            string additional = new String('0', precision - baseStr.Length);
                            if (baseStr[0] != '-') {
                                return additional + baseStr;
                            } else {
                                return "-" + additional + baseStr.Substring(1);
                            }
                        }
                        return baseStr;
                    }
                    return ToString(10);
                case 'x':
                case 'X':
                    StringBuilder res = new StringBuilder(ToString(16));
                    if (format[0] == 'x') {
                        for (int i = 0; i < res.Length; i++) {
                            if (res[i] >= 'A' && res[i] <= 'F') {
                                res[i] = Char.ToLower(res[i], CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    if (format.Length > 1) {
                        int precision = Convert.ToInt32(format.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        if (res.Length < precision) {
                            string additional = new String('0', precision - res.Length);
                            if (res[0] != '-') {
                                res.Insert(0, additional);
                            } else {
                                res.Insert(1, additional);
                            }
                        }
                    }

                    return res.ToString();
                default:
                    throw new NotImplementedException("format not implemented");
            }
        }

        #endregion        
    }
}
