//
// Mono.Data.SybaseTypes.SybaseDecimal
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Based on System.Data.SqlTypes.SqlDecimal
//
// (C) Ximian, Inc. 2002-2003
// (C) Copyright Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Mono.Data.Tds.Protocol;
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;

namespace Mono.Data.SybaseTypes {
	public struct SybaseDecimal : INullable, IComparable
	{

		#region Fields

		int[] value;
		byte precision;
		byte scale;
		bool positive;

		bool notNull;

		// borrowed from System.Decimal
		const int SCALE_SHIFT = 16;
		const int SIGN_SHIFT = 31;
		const int RESERVED_SS32_BITS = 0x7F00FFFF;
		const ulong LIT_GUINT64_HIGHBIT = 0x8000000000000000;
		const ulong LIT_GUINT32_HIGHBIT = 0x80000000;
		const byte DECIMAL_MAX_INTFACTORS = 9;
		static uint [] constantsDecadeInt32Factors = new uint [10]
			{
				1u, 10u, 100u, 1000u, 10000u, 100000u, 1000000u,
				10000000u, 100000000u, 1000000000u
			};

		public static readonly byte MaxPrecision = 38;
		public static readonly byte MaxScale = 38;

		public static readonly SybaseDecimal MaxValue = new SybaseDecimal (MaxPrecision, (byte)0, true, (int)716002642, Int32.MaxValue, (int)1518778966, (int)1262177448);
		public static readonly SybaseDecimal MinValue = new SybaseDecimal (MaxPrecision, (byte)0, false, (int)716002642, Int32.MaxValue, (int)1518778966, (int)1262177448);
		public static readonly SybaseDecimal Null;

		#endregion

		#region Constructors

		public SybaseDecimal (decimal value) 
		{
			int[] binData = Decimal.GetBits (value);

			this.scale = (byte)(binData[3] >> SCALE_SHIFT);
			if (this.scale > MaxScale || (this.scale & RESERVED_SS32_BITS) != 0)
				throw new ArgumentException(Locale.GetText ("Invalid scale"));

			this.value = new int[4];
			this.value[0] = binData[0];
			this.value[1] = binData[1];
			this.value[2] = binData[2];
			this.value[3] = 0;
			notNull = true;

			positive = (value >= 0);
			precision = GetPrecision (value);
		}

		public SybaseDecimal (double value) : this ((decimal)value) { }
		public SybaseDecimal (int value) : this ((decimal)value) { }
		public SybaseDecimal (long value) : this ((decimal)value) { }

		public SybaseDecimal (byte bPrecision, byte bScale, bool fPositive, int[] bits) : this (bPrecision, bScale, fPositive, bits[0], bits[1], bits[2], bits[3]) { }

		public SybaseDecimal (byte bPrecision, byte bScale, bool fPositive, int data1, int data2, int data3, int data4) 
		{
			this.precision = bPrecision;
			this.scale = bScale;
			this.positive = fPositive;
			this.value = new int[4];
			this.value[0] = data1;
			this.value[1] = data2;
			this.value[2] = data3;
			this.value[3] = data4;
			notNull = true;

			if (precision < scale)
				throw new ArgumentException ("Invalid scale");
			if (this.ToDouble () > (System.Math.Pow (10, 38) -1) || this.ToDouble () < -(System.Math.Pow (10, 38)))
				throw new SybaseTypeException ("Can't convert to SybaseDecimal.");
		}

		#endregion

		#region Properties

		public byte[] BinData {
			get { 
				byte[] b = new byte [value.Length * 4];
				int j = 0;
				for (int i = 0; i < value.Length; i += 1) {
					b [j++] = (byte) (0xff & value [i]);
					b [j++] = (byte) (0xff & value [i] >> 8);
					b [j++] = (byte) (0xff & value [i] >> 16);
					b [j++] = (byte) (0xff & value [i] >> 24);
				}
				return b;
			}
		}

		public int[] Data { 
			get { 
				if (this.IsNull)
					throw new SybaseNullValueException ();
				else
					return (value);
			}
		}

		public bool IsNull { 
			get { return !notNull; }
		}

		public bool IsPositive { 
			get { return positive; }
		}

		public byte Precision { 
			get { return precision; }
		}

		public byte Scale { 
			get { return scale; }
		}

		public decimal Value { 
			get { 
				if (this.IsNull) 
					throw new SybaseNullValueException ();
				if (this.value[3] > 0)
					throw new OverflowException ();
				return new decimal (value[0], value[1], value[2], !positive, scale);
			}
		}

		#endregion

		#region Methods

		public static SybaseDecimal Abs (SybaseDecimal n)
		{
			return new SybaseDecimal (n.Precision, n.Scale, true, n.BinData [0], n.BinData [1], n.BinData [2], n.BinData [3]);
		}

		public static SybaseDecimal Add (SybaseDecimal x, SybaseDecimal y)
		{
			return (x + y);
		}

		public static SybaseDecimal AdjustScale (SybaseDecimal n, int digits, bool fRound)
		{
			byte prec = n.Precision;
			if (n.IsNull)
				throw new SybaseNullValueException ();
			if (digits > 0)
				prec = (byte) (prec + digits);
			if (fRound)
				n = Round (n, digits + n.Scale);
			return new SybaseDecimal (prec, (byte) (n.Scale + digits), n.IsPositive, n.Data);
		}

		public static SybaseDecimal Ceiling (SybaseDecimal n)
		{
			return AdjustScale (n, -(n.Scale), true);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseDecimal))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseDecimal"));
			else if (((SybaseDecimal)value).IsNull)
				return 1;
			else
				return this.Value.CompareTo (((SybaseDecimal)value).Value);
		}

		public static SybaseDecimal ConvertToPrecScale (SybaseDecimal n, int precision, int scale)
		{
			return new SybaseDecimal ((byte) precision, (byte) scale, n.IsPositive, n.Data);
		}

		public static SybaseDecimal Divide (SybaseDecimal x, SybaseDecimal y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseDecimal))
				return false;
			else if (this.IsNull && ((SybaseDecimal) value).IsNull)
				return true;
			else if (((SybaseDecimal) value).IsNull)
				return false;
			else
				return (bool) (this == (SybaseDecimal)value);
		}

		public static SybaseBoolean Equals (SybaseDecimal x, SybaseDecimal y)
		{
			return (x == y);
		}

		public static SybaseDecimal Floor (SybaseDecimal n)
		{
			return AdjustScale (n, -(n.Scale), false);
		}

		internal static SybaseDecimal FromTdsBigDecimal (TdsBigDecimal x)
		{
			if (x == null)
				return Null;
			else
				return new SybaseDecimal (x.Precision, x.Scale, !x.IsNegative, x.Data);
                }

		public override int GetHashCode ()
		{
			int result = 10;
			result = 91 * result + this.Data [0];
			result = 91 * result + this.Data [1];
			result = 91 * result + this.Data [2];
			result = 91 * result + this.Data [3];
			result = 91 * result + (int) this.Scale;
			result = 91 * result + (int) this.Precision;

			return result;
		}

		public static SybaseBoolean GreaterThan (SybaseDecimal x, SybaseDecimal y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseDecimal x, SybaseDecimal y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseDecimal x, SybaseDecimal y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseDecimal x, SybaseDecimal y)
		{
			return (x <= y);
		}

		public static SybaseDecimal Multiply (SybaseDecimal x, SybaseDecimal y)
		{
			return (x * y);
		}

		public static SybaseBoolean NotEquals (SybaseDecimal x, SybaseDecimal y)
		{
			return (x != y);
		}

		public static SybaseDecimal Parse (string s)
		{
			if (s == null)
				throw new ArgumentNullException ();
			else
				return SybaseDouble.Parse (s).ToSybaseDecimal ();
		}

		public static SybaseDecimal Power (SybaseDecimal n, double exp)
		{
			if (n.IsNull)
				return SybaseDecimal.Null;
			return new SybaseDecimal (System.Math.Pow (n.ToDouble (), exp));
		}

		public static SybaseDecimal Round (SybaseDecimal n, int position)
		{
			if (n.IsNull)
				throw new SybaseNullValueException ();
			SybaseDecimal result = new SybaseDecimal (System.Math.Round ((double) (n.ToDouble () * System.Math.Pow (10, position))));
			result = result / new SybaseDecimal (System.Math.Pow (10, position));
			return result;
		}

		public static SybaseInt32 Sign (SybaseDecimal n)
		{
			SybaseInt32 result = 0;
			if (n >= new SybaseDecimal (0))
				result = 1;
			else
				result = -1;
			return result;
		}

		public static SybaseDecimal Subtract (SybaseDecimal x, SybaseDecimal y)
		{
			return (x - y);
		}

		private static byte GetPrecision (decimal value)
		{
			string str = value.ToString ();
			byte result = 0;
			foreach (char c in str) 
				if (c >= '0' && c <= '9')
					result ++;
			return result;
		}

		public double ToDouble ()
		{
			// FIXME: This is the wrong way to do this
			double d = (uint) this.Data [0];
			d += ((uint) this.Data [1]) * System.Math.Pow (2, 32);
			d += ((uint) this.Data [2]) * System.Math.Pow (2, 64);
			d += ((uint) this.Data [3]) * System.Math.Pow (2, 96);
			d /= System.Math.Pow (10, scale);
			return d;
		}

		public SybaseBoolean ToSybaseBoolean ()
		{
			return ((SybaseBoolean)this);
		}
		
		public SybaseByte ToSybaseByte ()
		{
			return ((SybaseByte)this);
		}

		public SybaseDouble ToSybaseDouble ()
		{
			return ((SybaseDouble)this);
		}

		public SybaseInt16 ToSybaseInt16 ()
		{
			return ((SybaseInt16)this);
		}

		public SybaseInt32 ToSybaseInt32 ()
		{
			return ((SybaseInt32)this);
		}

		public SybaseInt64 ToSybaseInt64 ()
		{
			return ((SybaseInt64)this);
		}

		public SybaseMoney ToSybaseMoney ()
		{
			return ((SybaseMoney)this);
		}

		public SybaseSingle ToSybaseSingle ()
		{
			return ((SybaseSingle)this);
		}

		public SybaseString ToSybaseString ()
		{
			return ((SybaseString)this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return String.Empty;

			// convert int [4] -> ulong [2]
			ulong lo = (uint) this.Data [0] + (ulong) ((ulong) this.Data [1] << 32);
			ulong hi = (uint) this.Data [2] + (ulong) ((ulong) this.Data [3] << 32);

			uint rest = 0;
			StringBuilder result = new StringBuilder ();
			for (int i = 0; lo != 0 || hi != 0; i += 1) {
				Div128By32 (ref hi, ref lo, 10, ref rest);
				result.Insert (0, rest.ToString ());
			}
			while (result.Length < Precision)
				result.Append ("0");
			while (result.Length > Precision)
				result.Remove (result.Length - 1, 1);
			if (Scale > 0)
				result.Insert (result.Length - Scale, ".");
			return result.ToString ();
		}

		// from decimal.c
		private static int Div128By32 (ref ulong hi, ref ulong lo, uint divider)
		{
			uint t = 0;
			return Div128By32 (ref hi, ref lo, divider, ref t);
		}

		// from decimal.c
		private static int Div128By32 (ref ulong hi, ref ulong lo, uint divider, ref uint rest)
		{
			ulong a = 0;
			ulong b = 0;
			ulong c = 0;

			a = (uint) (hi >> 32);
			b = a / divider;
			a -= b * divider;
			a <<= 32;
			a |= (uint) hi;
			c = a / divider;
			a -= c * divider;
			a <<= 32;
			hi = b << 32 | (uint) c;

			a = (uint) (lo >> 32);
			b = a / divider;
			a -= b * divider;
			a <<= 32;
			a |= (uint) lo;
			c = a / divider;
			a -= c * divider;
			a <<= 32;
			lo = b << 32 | (uint) c;

			rest = (uint) a;
			a <<= 1;

			return (a > divider || (a == divider && (c & 1) == 1)) ? 1 : 0;
		}

		[MonoTODO ("Find out what is the right way to set scale and precision")]
		private static SybaseDecimal DecimalDiv (SybaseDecimal x, SybaseDecimal y)
		{
			ulong lo = 0;
			ulong hi = 0;
			int sc = 0; // scale
			int texp = 0;
			byte prec = 0;

			prec = x.Precision >= y.Precision ? x.Precision : y.Precision;
			DecimalDivSub (ref x, ref y, ref lo, ref hi, ref texp);

			sc = x.Scale - y.Scale;

			Rescale128 (ref lo, ref hi, ref sc, texp, 0, 38, 1);

			uint r = 0;
			while (prec < sc) {
				Div128By32 (ref hi, ref lo, 10, ref r);
				sc -= 1;
			}

			if (r >= 5)
				lo += 1;
		
			while ((((double) hi) * System.Math.Pow (2, 64) + lo) - System.Math.Pow (10, prec) > 0)
				prec += 1;

			while ((prec + sc) > MaxScale) {
				Div128By32 (ref hi, ref lo, 10, ref r);
				sc -= 1;
				if (r >= 5)
					lo += 1;
			}

			int resultLo = (int) lo;
			int resultMi = (int) (lo >> 32);
			int resultMi2 = (int) hi;
			int resultHi = (int) (hi >> 32);

			return new SybaseDecimal (prec, (byte) sc, true, resultLo, resultMi, resultMi2, resultHi);
		}

		// From decimal.c
		private static void Rescale128 (ref ulong clo, ref ulong chi, ref int scale, int texp, int minScale, int maxScale, int roundFlag) 
		{
			uint factor = 0;
			uint overhang = 0;
			int sc = 0;
			int i = 0;
			int roundBit = 0;

			sc = scale;
			if (texp > 0) {
				// reduce exp
				while (texp > 0 && sc <= maxScale) {
					overhang = (uint) (chi >> 64);
					while (texp > 0 && (((clo & 1) == 0) || overhang > 0)) {
						if (--texp == 0)
							roundBit = (int) (clo & 1);
						RShift128 (ref clo, ref chi);
						overhang = (uint) (chi >> 32);
					}

					if (texp > DECIMAL_MAX_INTFACTORS)
						i = DECIMAL_MAX_INTFACTORS;
					else
						i = texp;
					
					if (sc + i > maxScale) 
						i = maxScale - sc;
					if (i == 0)
						break;
					
					texp -= i;
					sc += i;

					// 10^i/2^i=5^i
					factor = constantsDecadeInt32Factors [i] >> i;
					Mult128By32 (ref clo, ref chi, factor, 0);
				}

				while (texp > 0) {
					if (--texp == 0)
						roundBit = (int) (clo & 1);
					RShift128 (ref clo, ref chi);

				}
			}

			while (sc > maxScale) {
				i = scale - maxScale;
				if (i > DECIMAL_MAX_INTFACTORS)
					i = DECIMAL_MAX_INTFACTORS;
				sc -= i;
				roundBit = Div128By32 (ref clo, ref chi, constantsDecadeInt32Factors [i]);
			}

			while (sc < minScale) {
				if (roundFlag == 0)
					roundBit = 0;
				i = minScale - sc;
				if (i > DECIMAL_MAX_INTFACTORS)
					i = DECIMAL_MAX_INTFACTORS;
				sc += i;
				Mult128By32 (ref clo, ref chi, constantsDecadeInt32Factors [i], roundBit);
				roundBit = 0;
			}
			scale = sc;
			Normalize128 (ref clo, ref chi, ref sc, roundFlag, roundBit);
		}

		// from decimal.c
		private static void Normalize128 (ref ulong clo, ref ulong chi, ref int scale, int roundFlag, int roundBit)
		{
			if ((roundFlag != 0) && (roundBit != 0))
				RoundUp128 (ref clo, ref chi);
		}

		// from decimal.c
		private static void RoundUp128 (ref ulong lo, ref ulong hi)
		{
			if ((++lo) == 0)
				++hi;
		}

		// from decimal.c
		private static void DecimalDivSub (ref SybaseDecimal x, ref SybaseDecimal y, ref ulong clo, ref ulong chi, ref int exp)
		{
			ulong xlo, xmi, xhi;
			ulong tlo = 0;
			ulong tmi = 0;
			ulong thi = 0;
			uint ylo = 0;
			uint ymi = 0;
			uint ymi2 = 0;
			uint yhi = 0;
			int ashift = 0;
			int bshift = 0;
			int extraBit = 0;

			xhi = (ulong) ((ulong) x.Data [3] << 32) | (ulong) x.Data [2];
			xmi = (ulong) ((ulong) x.Data [1] << 32) | (ulong) x.Data [0];
			xlo = (uint) 0;
			ylo = (uint) y.Data [0];
			ymi = (uint) y.Data [1];
			ymi2 = (uint) y.Data [2];
			yhi = (uint) y.Data [3];

			if (ylo == 0 && ymi == 0 && ymi2 == 0 && yhi == 0)
				throw new DivideByZeroException ();
			if (xmi == 0 && xhi == 0) {
				clo = chi = 0;
				return;
			}

			// enlarge dividend to get maximal precision
			for (ashift = 0; (xhi & LIT_GUINT64_HIGHBIT) == 0; ++ashift) 
				LShift128 (ref xmi, ref xhi);

			// ensure that divisor is at least 2^95
			for (bshift = 0; (yhi & LIT_GUINT32_HIGHBIT) == 0; ++bshift) 
				LShift128 (ref ylo, ref ymi, ref ymi2, ref yhi);

			thi = ((ulong) yhi) << 32 | (ulong) ymi2;
			tmi = ((ulong) ymi) << 32 | (ulong) ylo;
			tlo = 0;

			if (xhi > thi || (xhi == thi && xmi >= tmi)) {
				Sub192 (xlo, xmi, xhi, tlo, tmi, thi, ref xlo, ref xmi, ref xhi);
				extraBit = 1;
			} else {
				extraBit = 0;
			}

			Div192By128To128 (xlo, xmi, xhi, ylo, ymi, ymi2, yhi, ref clo, ref chi);

			exp = 128 + ashift - bshift;

			if (extraBit != 0) {
				RShift128 (ref clo, ref chi);
				chi += LIT_GUINT64_HIGHBIT;
				exp -= 1;
			}

			// try loss free right shift
			while (exp > 0 && (clo & 1) == 0) {
				RShift128 (ref clo, ref chi);
				exp -= 1;
			}
		}

		// From decimal.c
		private static void RShift192 (ref ulong lo, ref ulong mi, ref ulong hi)
		{
			lo >>= 1;
			if ((mi & 1) != 0)
				lo |= LIT_GUINT64_HIGHBIT;
			
			mi >>= 1;
			if ((hi & 1) != 0)
				mi |= LIT_GUINT64_HIGHBIT;
	
			hi >>= 1;
		}

		// From decimal.c
		private static void RShift128 (ref ulong lo, ref ulong hi)
		{
			lo >>= 1;
			if ((hi & 1) != 0)
				lo |= LIT_GUINT64_HIGHBIT;
			hi >>= 1;
		}

		// From decimal.c
		private static void LShift128 (ref ulong lo, ref ulong hi)
		{
			hi <<= 1;

			if ((lo & LIT_GUINT64_HIGHBIT) != 0)
				hi += 1;
			
			lo <<= 1;
		}

		// From decimal.c
		private static void LShift128 (ref uint lo, ref uint mi, ref uint mi2, ref uint hi)
		{
			hi <<= 1;
			if ((mi2 & LIT_GUINT32_HIGHBIT) != 0)
				hi += 1;

			mi2 <<= 1;
			if ((mi & LIT_GUINT32_HIGHBIT) != 0)
				mi2 += 1;

			mi <<= 1;
			if ((lo & LIT_GUINT32_HIGHBIT) != 0)
				mi += 1;

			lo <<= 1;
		}

		// From decimal.c
		private static void Div192By128To128 (ulong xlo, ulong xmi, ulong xhi, uint ylo, uint ymi, uint ymi2, uint yhi, ref ulong clo, ref ulong chi)
		{
			ulong rlo, rmi, rhi; // remainders
			uint h, c;

			rlo = xlo;
			rmi = xmi;
			rhi = xhi;

			h = Div192By128To32WithRest (ref rlo, ref rmi, ref rhi, ylo, ymi, ymi2, yhi);

			// mid 32 bit
			rhi = (rhi << 32) | (rmi >> 32);
			rmi = (rmi << 32) | (rlo >> 32);
			rlo <<= 32;

			chi = (((ulong)h) << 32) | Div192By128To32WithRest (ref rlo, ref rmi, ref rhi, ylo, ymi, ymi2, yhi);

			// low 32 bit
			rhi = (rhi << 32) | (rmi >> 32);
			rmi = (rmi << 32) | (rlo >> 32);
			rlo <<= 32;

			h = Div192By128To32WithRest (ref rlo, ref rmi, ref rhi, ylo, ymi, ymi2, yhi);

			// estimate lowest 32 bit (two last bits may be wrong)
			if (rhi >= yhi)
				c =  0xFFFFFFFF;
			else {
				rhi <<= 32;
				c = (uint)(rhi / yhi);
			}

			clo = (((ulong)h) << 32) | c;
		}

		// From decimal.c
		private static uint Div192By128To32WithRest (ref ulong xlo, ref ulong xmi, ref ulong xhi, uint ylo, uint ymi, uint ymi2, uint yhi)
		{
			ulong rlo, rmi, rhi; // remainder
			ulong tlo = 0;
			ulong thi = 0;
			uint c;

			rlo = xlo;
			rmi = xmi;
			rhi = xhi;

			if (rhi >= (((ulong)yhi << 32)))
				c = 0xFFFFFFFF;
			else
				c = (uint) (rhi / yhi);

			Mult128By32To128 (ylo, ymi, ymi2, yhi, c, ref tlo, ref thi);
			Sub192 (rlo, rmi, rhi, 0, tlo, thi, ref rlo, ref rmi, ref rhi);

			while (((long)rhi) < 0) {
				c--;
				Add192 (rlo, rmi, rhi, 0, (((ulong)ymi) << 32) | ylo, yhi | ymi2, ref rlo, ref rmi, ref rhi);
			}
			xlo = rlo;
			xmi = rmi;
			xhi = rhi;

			return c;
		}

		// From decimal.c
		private static void Mult192By32 (ref ulong clo, ref ulong cmi, ref ulong chi, ulong factor, int roundBit)
		{
			ulong a = 0;
			uint h0 = 0;
			uint h1 = 0;

			a = ((ulong)(uint)clo) * factor;

			if (roundBit != 0)
				a += factor / 2;

			h0 = (uint)a;
			a >>= 32;
			a += (clo >> 32) * factor;
			h1 = (uint)a;

			clo = ((ulong)h1) << 32 | h0;

			a >>= 32;
			a += ((ulong)(uint)cmi) * factor;
			h0 = (uint)a;

			a >>= 32;
			a += (cmi >> 32) * factor;
			h1 = (uint)a;

			cmi = ((ulong)h1) << 32 | h0;
			a >>= 32;
			a += ((ulong)(uint)chi) * factor;
			h0 = (uint)a;

			a >>= 32;
			a += (chi >> 32) * factor;
			h1 = (uint)a;
			chi = ((ulong)h1) << 32 | h0;
		}

		// From decimal.c
		private static void Mult128By32 (ref ulong clo, ref ulong chi, uint factor, int roundBit)
		{
			ulong a = 0;
			uint h0 = 0;
			uint h1 = 0;

			a = ((ulong)(uint)clo) * factor;

			if (roundBit != 0)
				a += factor / 2;

			h0 = (uint)a;

			a >>= 32;
			a += (clo >> 32) * factor;
			h1 = (uint)a;

			clo = ((ulong)h1) << 32 | h0;

			a >>= 32;
			a += ((ulong)(uint)chi) * factor;
			h0 = (uint)a;

			a >>= 32;
			a += (chi >> 32) * factor;
			h1 = (uint)a;

			chi = ((ulong)h1) << 32 | h0;
		}

		// From decimal.c
		private static void Mult128By32To128 (uint xlo, uint xmi, uint xmi2, uint xhi, uint factor, ref ulong clo, ref ulong chi)
		{
			ulong a;
			uint h0, h1, h2;

			a = ((ulong)xlo) * factor;
			h0 = (uint)a;

			a >>= 32;
			a += ((ulong)xmi) * factor;
			h1 = (uint)a;

			a >>= 32;
			a += ((ulong)xmi2) * factor;
			h2 = (uint)a;

			a >>= 32;
			a += ((ulong)xhi) * factor;

			clo = ((ulong)h1) << 32 | h0;
			chi = a | h2;
		}

		// From decimal.c
		private static void Add192 (ulong xlo, ulong xmi, ulong xhi, ulong ylo, ulong ymi, ulong yhi, ref ulong clo, ref ulong cmi, ref ulong chi)
		{
			xlo += ylo;
			if (xlo < ylo) {
				xmi++;
				if (xmi == 0)
					xhi++;
			}

			xmi += ymi;

			if (xmi < ymi)
				xmi++;

			xhi += yhi;
			clo = xlo;
			cmi = xmi;
			chi = xhi;
		}

		// From decimal.c
		private static void Sub192 (ulong xlo, ulong xmi, ulong xhi, ulong ylo, ulong ymi, ulong yhi, ref ulong lo, ref ulong mi, ref ulong hi)
		{
			ulong clo = 0;
			ulong cmi = 0;
			ulong chi = 0;

			clo = xlo - ylo;
			cmi = xmi - ymi;
			chi = xhi - yhi;

			if (xlo < ylo) {
				if (cmi == 0)
					chi--;
				cmi--;
			}

			if (xmi < ymi)
				chi--;

			lo = clo;
			mi = cmi;
			hi = chi;
		}

		public static SybaseDecimal Truncate (SybaseDecimal n, int position)
		{
			return new SybaseDecimal ((byte) n.Precision, (byte) position, n.IsPositive, n.Data);
		}

		public static SybaseDecimal operator + (SybaseDecimal x, SybaseDecimal y)
		{
			// if one of them is negative, perform subtraction
			if (x.IsPositive && !y.IsPositive) return x - y;
			if (y.IsPositive && !x.IsPositive) return y - x;
		
			// adjust the scale to the smaller of the two beforehand
			if (x.Scale > y.Scale)
				x = SybaseDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SybaseDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			// set the precision to the greater of the two
			byte resultPrecision;
			if (x.Precision > y.Precision)
				resultPrecision = x.Precision;
			else
				resultPrecision = y.Precision;
				
			int[] xData = x.Data;
			int[] yData = y.Data;
			int[] resultBits = new int[4];

			ulong res; 
			ulong carry = 0;

			// add one at a time, and carry the results over to the next
			for (int i = 0; i < 4; i +=1)
			{
				carry = 0;
				res = (ulong)(xData[i]) + (ulong)(yData[i]) + carry;
				if (res > Int32.MaxValue)
				{
					carry = res - Int32.MaxValue;
					res = Int32.MaxValue;
				}
				resultBits [i] = (int)res;
			}

			// if we have carry left, then throw an exception
			if (carry > 0)
				throw new OverflowException ();
			else
				return new SybaseDecimal (resultPrecision, x.Scale, x.IsPositive, resultBits);
		}

		public static SybaseDecimal operator / (SybaseDecimal x, SybaseDecimal y)
		{
			return DecimalDiv (x, y);
		}

		public static SybaseBoolean operator == (SybaseDecimal x, SybaseDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;

			if (x.Scale > y.Scale)
				x = SybaseDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SybaseDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 0; i < 4; i += 1)
			{
				if (x.Data[i] != y.Data[i])
					return new SybaseBoolean (false);
			}
			return new SybaseBoolean (true);
		}

		public static SybaseBoolean operator > (SybaseDecimal x, SybaseDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;

			if (x.Scale > y.Scale)
				x = SybaseDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SybaseDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new SybaseBoolean (x.Data[i] > y.Data[i]);
			}
			return new SybaseBoolean (false);
		}

		public static SybaseBoolean operator >= (SybaseDecimal x, SybaseDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;

			if (x.Scale > y.Scale)
				x = SybaseDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SybaseDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new SybaseBoolean (x.Data[i] >= y.Data[i]);
			}
			return new SybaseBoolean (true);
		}

		public static SybaseBoolean operator != (SybaseDecimal x, SybaseDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;

			if (x.Scale > y.Scale)
				x = SybaseDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SybaseDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 0; i < 4; i += 1)
			{
				if (x.Data[i] != y.Data[i])
					return new SybaseBoolean (true);
			}
			return new SybaseBoolean (false);
		}

		public static SybaseBoolean operator < (SybaseDecimal x, SybaseDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;

			if (x.Scale > y.Scale)
				x = SybaseDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SybaseDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;

				return new SybaseBoolean (x.Data[i] < y.Data[i]);
			}
			return new SybaseBoolean (false);
		}

		public static SybaseBoolean operator <= (SybaseDecimal x, SybaseDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;

			if (x.Scale > y.Scale)
				x = SybaseDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SybaseDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new SybaseBoolean (x.Data[i] <= y.Data[i]);
			}
			return new SybaseBoolean (true);
		}

		public static SybaseDecimal operator * (SybaseDecimal x, SybaseDecimal y)
		{
			// adjust the scale to the smaller of the two beforehand
			if (x.Scale > y.Scale)
				x = SybaseDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SybaseDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			// set the precision to the greater of the two
			byte resultPrecision;
			if (x.Precision > y.Precision)
				resultPrecision = x.Precision;
			else
				resultPrecision = y.Precision;
				
			int[] xData = x.Data;
			int[] yData = y.Data;
			int[] resultBits = new int[4];

			ulong res; 
			ulong carry = 0;

			// multiply one at a time, and carry the results over to the next
			for (int i = 0; i < 4; i +=1)
			{
				carry = 0;
				res = (ulong)(xData[i]) * (ulong)(yData[i]) + carry;
				if (res > Int32.MaxValue)
				{
					carry = res - Int32.MaxValue;
					res = Int32.MaxValue;
				}
				resultBits [i] = (int)res;
			}

			// if we have carry left, then throw an exception
			if (carry > 0)
				throw new OverflowException ();
			else
				return new SybaseDecimal (resultPrecision, x.Scale, (x.IsPositive == y.IsPositive), resultBits);
				
		}

		public static SybaseDecimal operator - (SybaseDecimal x, SybaseDecimal y)
		{
			if (x.IsPositive && !y.IsPositive) return x + y;
			if (!x.IsPositive && y.IsPositive) return -(x + y);
			if (!x.IsPositive && !y.IsPositive) return y - x;

			// otherwise, x is positive and y is positive
			bool resultPositive = (bool)(x > y);
			int[] yData = y.Data;

			for (int i = 0; i < 4; i += 1) yData[i] = -yData[i];

			SybaseDecimal yInverse = new SybaseDecimal (y.Precision, y.Scale, y.IsPositive, yData);

			if (resultPositive)
				return x + yInverse;
			else
				return -(x + yInverse);
		}

		public static SybaseDecimal operator - (SybaseDecimal n)
		{
			return new SybaseDecimal (n.Precision, n.Scale, !n.IsPositive, n.Data);
		}

		public static explicit operator SybaseDecimal (SybaseBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDecimal ((decimal)x.ByteValue);
		}

		public static explicit operator Decimal (SybaseDecimal n)
		{
			return n.Value;
		}

		public static explicit operator SybaseDecimal (SybaseDouble x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDecimal ((decimal)x.Value);
		}

		public static explicit operator SybaseDecimal (SybaseSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDecimal ((decimal)x.Value);
		}

		[MonoTODO]
		public static explicit operator SybaseDecimal (SybaseString x)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator SybaseDecimal (decimal x)
		{
			return new SybaseDecimal (x);
		}

		public static implicit operator SybaseDecimal (SybaseByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDecimal ((decimal)x.Value);
		}

		public static implicit operator SybaseDecimal (SybaseInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDecimal ((decimal)x.Value);
		}

		public static implicit operator SybaseDecimal (SybaseInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDecimal ((decimal)x.Value);
		}

		public static implicit operator SybaseDecimal (SybaseInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDecimal ((decimal)x.Value);
		}

		public static implicit operator SybaseDecimal (SybaseMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDecimal ((decimal)x.Value);
		}

		#endregion
	}
}
			
