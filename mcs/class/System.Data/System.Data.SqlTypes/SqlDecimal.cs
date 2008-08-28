//
// System.Data.SqlTypes.SqlDecimal
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@koti.soon.fi>
//
// (C) Copyright 2002 Tim Coleman
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if !TARGET_JVM
using Mono.Data.Tds.Protocol;
#endif
using System;
using System.Xml;
using System.Text;
using System.Xml.Schema;
using System.Globalization;
using System.Xml.Serialization;

namespace System.Data.SqlTypes
{
#if NET_2_0
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
#endif
	public struct SqlDecimal : INullable, IComparable
#if NET_2_0
				, IXmlSerializable
#endif
	{
		#region Fields

		int[] value;
		byte precision;
		byte scale;
		bool positive;

		private bool notNull;

		// borrowed from System.Decimal
		const int SCALE_SHIFT = 16;
		const int SIGN_SHIFT = 31;
		const int RESERVED_SS32_BITS = 0x7F00FFFF;
		const ulong LIT_GUINT64_HIGHBIT = 0x8000000000000000;
		const ulong LIT_GUINT32_HIGHBIT = 0x80000000;
		const byte DECIMAL_MAX_INTFACTORS = 9;
		static uint [] constantsDecadeInt32Factors = new uint [10] {
			1u, 10u, 100u, 1000u, 10000u, 100000u, 1000000u, 
			10000000u, 100000000u, 1000000000u};

		public static readonly byte MaxPrecision = 38; 
		public static readonly byte MaxScale = 38;

		// This should be 99999999999999999999999999999999999999
		public static readonly SqlDecimal MaxValue = new SqlDecimal (MaxPrecision, 
									     (byte)0, 
									     true, 
									     (int)-1, 
									     160047679,
									     1518781562, 
									     1262177448);
		// This should be -99999999999999999999999999999999999999
		public static readonly SqlDecimal MinValue = new SqlDecimal (MaxPrecision, 
									     (byte)0, false,
									     -1,
									     160047679,
									     1518781562,
									     1262177448);

		public static readonly SqlDecimal Null;

		#endregion

		#region Constructors

		public SqlDecimal (decimal value)
		{
			int[] binData = Decimal.GetBits (value);

			this.precision = MaxPrecision; // this value seems unclear

			this.scale = (byte)(((uint)binData [3]) >> SCALE_SHIFT);
			
			if (this.scale > MaxScale || ((uint)binData [3] & RESERVED_SS32_BITS) != 0)
				throw new ArgumentException(Locale.GetText ("Invalid scale"));

			this.value = new int[4];
			this.value[0] = binData[0];
			this.value[1] = binData[1];
			this.value[2] = binData[2];
			this.value[3] = 0;

			positive = (value >= 0);
			notNull = true;
			precision = GetPrecision (value);
		}

		public SqlDecimal (double dVal) : this ((decimal) dVal)
		{
			SqlDecimal n = this;
			int digits = 17 - precision;
			if (digits > 0)
				n = AdjustScale (this, digits, false);
			else
				n = Round (this, 17);
			this.notNull = n.notNull;
			this.positive = n.positive;
			this.precision = n.precision;
			this.scale = n.scale;
			this.value = n.value;
		}

		public SqlDecimal (int value) : this ((decimal) value)
		{
		}

		public SqlDecimal (long value) : this ((decimal) value)
		{
		}

		public SqlDecimal (byte bPrecision, byte bScale, bool fPositive, int[] bits) : this (bPrecision, bScale, fPositive, bits[0], bits[1], bits[2], bits[3])
		{
		}

		public SqlDecimal (byte bPrecision, byte bScale, bool fPositive, int data1, int data2, int data3, int data4)
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
				throw new SqlTypeException (Locale.GetText ("Invalid presicion/scale combination."));

			if (precision > 38)
				throw new SqlTypeException (Locale.GetText ("Invalid precision/scale combination."));

			if (this.ToDouble () > (Math.Pow (10, 38) - 1) ||
			    this.ToDouble () < -(Math.Pow (10, 38)))
				throw new OverflowException ("Can't convert to SqlDecimal, Out of range ");
		}

		#endregion

		#region Properties

		public byte[] BinData {
			get {
				byte [] b = new byte [value.Length * 4];
				
				int j = 0;
				for (int i = 0; i < value.Length; i++) {

					b [j++] = (byte)(0xff & value [i]);
					b [j++] = (byte)(0xff & value [i] >> 8);
					b [j++] = (byte)(0xff & value [i] >> 16);
					b [j++] = (byte)(0xff & value [i] >> 24);
				}

				return b;
			}
		}

		public int[] Data {
			get {
				if (this.IsNull)
					throw new SqlNullValueException ();
				// Data should always return clone, not to be modified
				int [] ret = new int [4];
				ret [0] = value [0];
				ret [1] = value [1];
				ret [2] = value [2];
				ret [3] = value [3];
				return ret;
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
					throw new SqlNullValueException ();

				if (this.value[3] > 0)
					throw new OverflowException ();

				return new decimal (value[0], value[1], value[2], !positive, scale);
			}
		}

		#endregion

		#region Methods

		public static SqlDecimal Abs (SqlDecimal n)
		{
			if (!n.notNull)
				return n;
			return new SqlDecimal (n.Precision, n.Scale, true, n.Data);
		}

		public static SqlDecimal Add (SqlDecimal x, SqlDecimal y)
		{
			return (x + y);
		}

		public static SqlDecimal AdjustScale (SqlDecimal n, int digits, bool fRound)
		{
			byte prec = n.Precision;
			if (n.IsNull)
				throw new SqlNullValueException ();

			byte scale;
			if (digits == 0)
				return n;
			else if (digits > 0) {
				prec = (byte)(prec + digits);
				scale = (byte) (n.scale + digits);
				// use Math.Pow once the Ctr (double) is fixed to  handle
				// values greater than Decimal.MaxValue
				// the current code creates too many sqldecimal objects 
				//n = n * (new SqlDecimal ((double)Math.Pow (10, digits)));
				for (int i = 0; i < digits; i++)
					n *= 10;
			} else {
				if (n.Scale < Math.Abs (digits))
					throw new SqlTruncateException ();

				if (fRound)
					n = Round (n, digits + n.scale);
				else
					n = Round (Truncate (n, digits + n.scale), digits + n.scale);
				scale = n.scale;
			}

			return new SqlDecimal (prec, scale, n.positive, n.Data);
		}

		public static SqlDecimal Ceiling (SqlDecimal n)
		{
			if (!n.notNull)
				return n;
			return AdjustScale (n, -(n.Scale), true);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			if (!(value is SqlDecimal))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlDecimal"));

			return CompareTo ((SqlDecimal) value);
		}

#if NET_2_0
		public
#endif
		int CompareTo (SqlDecimal value)
		{
			if (value.IsNull)
				return 1;
			else
				return this.Value.CompareTo (value.Value);
		}

		public static SqlDecimal ConvertToPrecScale (SqlDecimal n, int precision, int scale)
		{
			int prec = n.Precision;
			int sc = n.Scale;
			n = AdjustScale (n, scale-n.scale, true);
			if ((n.Scale >= sc) && (precision < n.Precision))
				throw new SqlTruncateException ();
			else{
				prec = precision;
				return new SqlDecimal ((byte)prec, n.scale, n.IsPositive, n.Data);
			}
		}

		public static SqlDecimal Divide (SqlDecimal x, SqlDecimal y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlDecimal))
				return false;
			else if (this.IsNull)
				return ((SqlDecimal)value).IsNull;
			else if (((SqlDecimal)value).IsNull)
				return false;
			else
				return (bool) (this == (SqlDecimal)value);
		}

		public static SqlBoolean Equals (SqlDecimal x, SqlDecimal y)
		{
			return (x == y);
		}

		public static SqlDecimal Floor (SqlDecimal n)
		{
			return AdjustScale (n, -(n.Scale), false);
		}

#if !TARGET_JVM
		internal static SqlDecimal FromTdsBigDecimal (TdsBigDecimal x)
		{
			if (x == null)
				return Null;
			else
				return new SqlDecimal (x.Precision, x.Scale, !x.IsNegative, x.Data);
		}
#endif

		public override int GetHashCode ()
		{
			int result = 10;
			result = 91 * result + this.Data[0];
			result = 91 * result + this.Data[1];
			result = 91 * result + this.Data[2];
			result = 91 * result + this.Data[3];
			result = 91 * result + (int) this.Scale;
			result = 91 * result + (int) this.Precision;

			return result;
		}

		public static SqlBoolean GreaterThan (SqlDecimal x, SqlDecimal y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlDecimal x, SqlDecimal y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlDecimal x, SqlDecimal y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlDecimal x, SqlDecimal y)
		{
			return (x <= y);
		}

		public static SqlDecimal Multiply (SqlDecimal x, SqlDecimal y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlDecimal x, SqlDecimal y)
		{
			return (x != y);
		}

		public static SqlDecimal Parse (string s)
		{
			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("string s"));
			else
				return new SqlDecimal (Decimal.Parse (s));
		}

		public static SqlDecimal Power (SqlDecimal n, double exp)
		{
			if (n.IsNull)
				return SqlDecimal.Null;

			return new SqlDecimal (Math.Pow (n.ToDouble (), exp));
		}

		public static SqlDecimal Round (SqlDecimal n, int position)
		{
			if (n.IsNull)
				throw new SqlNullValueException ();

			decimal d = n.Value;
			d = Decimal.Round (d, position);
			return new SqlDecimal (d);
		}

		public static SqlInt32 Sign (SqlDecimal n)
		{
			if (n.IsNull)
				return SqlInt32.Null;
			return (SqlInt32) (n.IsPositive ? 1 : -1);
		}

		public static SqlDecimal Subtract (SqlDecimal x, SqlDecimal y)
		{
			return (x - y);
		}

		private byte GetPrecision (decimal value)
		{
			string str = value.ToString ();
			byte result = 0;

			foreach (char c in str) {
				
				if (c >= '0' && c <= '9')
					result++;
			}
			
			return result;
		}

		public double ToDouble ()
		{
			// FIXME: This is wrong way to do this
			double d = (uint)this.Data [0];
			d += ((uint)this.Data [1]) * Math.Pow (2, 32);
			d += ((uint)this.Data [2]) * Math.Pow (2, 64);
			d += ((uint)this.Data [3]) * Math.Pow (2, 96);
			d = d / Math.Pow (10, scale);

			return d;
		}

		public SqlBoolean ToSqlBoolean ()
		{
			return ((SqlBoolean) this);
		}
		
		public SqlByte ToSqlByte ()
		{
			return ((SqlByte) this);
		}

		public SqlDouble ToSqlDouble ()
		{
			return ((SqlDouble) this);
		}

		public SqlInt16 ToSqlInt16 ()
		{
			return ((SqlInt16) this);
		}

		public SqlInt32 ToSqlInt32 ()
		{
			return ((SqlInt32) this);
		}

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64) this);
		}

		public SqlMoney ToSqlMoney ()
		{
			return ((SqlMoney) this);
		}

		public SqlSingle ToSqlSingle ()
		{
			return ((SqlSingle) this);
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString) this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return "Null";
			
			// convert int [4] --> ulong [2]
			ulong lo = (uint)this.Data [0];
			lo += (ulong)((ulong)this.Data [1] << 32);
			ulong hi = (uint)this.Data [2];
			hi += (ulong)((ulong)this.Data [3] << 32);

			uint rest = 0;
			StringBuilder Result = new StringBuilder ();
			for (int i = 0; lo != 0 || hi != 0; i++) {
				Div128By32 (ref hi, ref lo, 10, ref rest);
				Result.Insert (0, rest.ToString ());
			}

			while (Result.Length > this.Precision)
				Result.Remove (Result.Length - 1, 1);
 
			if (this.Scale > 0)
				Result.Insert (Result.Length - this.Scale, ".");

			if (!positive)
				Result.Insert (0, '-');

			return Result.ToString ();
		}

		// From decimal.c
		private static int Div128By32(ref ulong hi, ref ulong lo, uint divider)
		{
			uint t = 0;
			return Div128By32 (ref hi, ref lo, divider, ref t);
		}

		// From decimal.c
		private static int Div128By32(ref ulong hi, ref ulong lo, uint divider, ref uint rest)
		{
			ulong a = 0;
			ulong b = 0;
			ulong c = 0;
			
			a = (uint)(hi >> 32);
			b = a / divider;
			a -= b * divider;
			a <<= 32;
			a |= (uint)hi;
			c = a / divider;
			a -= c * divider;
			a <<= 32;
			hi = b << 32 | (uint)c;
			
			a |= (uint)(lo >> 32);
			b = a / divider;
			a -= b * divider;
			a <<= 32;
			a |= (uint)lo;
			c = a / divider;
			a -= c * divider;
			lo = b << 32 | (uint)c;
			rest = (uint)a;
			a <<= 1;

			return (a > divider || (a == divider && (c & 1) == 1)) ? 1 : 0;

		}

		[MonoTODO("Find out what is the right way to set scale and precision")]
		private static SqlDecimal DecimalDiv (SqlDecimal x, SqlDecimal y)
		{
			ulong lo = 0;
			ulong hi = 0;
			int sc = 0; // scale
			int texp = 0;
			byte prec = 0; // precision
			bool positive = ! (x.positive ^ y.positive);

			prec = x.Precision >= y.Precision ? x.Precision : y.Precision;
			DecimalDivSub (ref x, ref y, ref lo, ref hi, ref texp);

			sc = x.Scale - y.Scale;

			Rescale128 (ref lo, ref hi, ref sc, texp, 0, 38, 1);

			uint r = 0;
			while (prec < sc) {
				Div128By32(ref hi, ref lo, 10, ref r);
				sc--;
			}

			if (r >= 5) 
				lo++;

			while ((((double)hi) * Math.Pow(2,64) + lo) - Math.Pow (10, prec) > 0)
				prec++;

			while ((prec + sc) > MaxScale) {
				Div128By32(ref hi, ref lo, 10, ref r);
				sc--;
				if (r >= 5)
					lo++;
			}

			int resultLo = (int) lo;
			int resultMi = (int) (lo >> 32);
			int resultMi2 = (int) (hi);
			int resultHi = (int) (hi >> 32);

			return new SqlDecimal (prec, (byte) sc, positive, resultLo,
						       resultMi, resultMi2,
						       resultHi);
		}

		// From decimal.c
		private static void Rescale128 (ref ulong clo, ref ulong chi, 
					     ref int scale, int texp,
					     int minScale, int maxScale,
					     int roundFlag)
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
					overhang = (uint)(chi >> 64);
					while (texp > 0 && (((clo & 1) == 0) || overhang > 0)) {
						if (--texp == 0)
							roundBit = (int)(clo & 1);
						RShift128 (ref clo, ref chi);

						overhang = (uint)(chi >> 32);
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
						roundBit = (int)(clo & 1);
					RShift128 (ref clo, ref chi);
				}
			}
	
			while (sc > maxScale) {
				i = scale - maxScale;
				if (i > DECIMAL_MAX_INTFACTORS)
					i = DECIMAL_MAX_INTFACTORS;
				sc -= i;
				roundBit = Div128By32 (ref clo, ref chi, 
						       constantsDecadeInt32Factors[i]);
			}

			while (sc < minScale) {
				if (roundFlag == 0)
					roundBit = 0;
				i = minScale - sc;
				if (i > DECIMAL_MAX_INTFACTORS)
					i = DECIMAL_MAX_INTFACTORS;
				sc += i;
				Mult128By32 (ref clo, ref chi, 
					     constantsDecadeInt32Factors[i], roundBit);
				roundBit = 0;
			}
			scale = sc;
			Normalize128 (ref clo, ref chi, ref sc, roundFlag, roundBit);
		}

		// From decimal.c
		private static void Normalize128(ref ulong clo, ref ulong chi, ref int scale, int roundFlag, int roundBit)
		{
			int sc = scale;
			
			scale = sc;
			if ((roundFlag != 0) && (roundBit != 0)) 
				RoundUp128 (ref clo, ref chi); 
		}

		// From decimal.c
		private static void RoundUp128(ref ulong lo, ref ulong hi)
		{
			    if ((++lo) == 0) 
				    ++hi;
		}
		
		// From decimal.c
		private static void DecimalDivSub (ref SqlDecimal x, ref SqlDecimal y, ref ulong clo, ref ulong chi, ref int exp)
		{
			ulong xlo, xmi, xhi;
			ulong tlo = 0; 
			ulong tmi = 0;
			ulong thi = 0;;
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
			
			thi = ((ulong)yhi) << 32 | (ulong)ymi2;
			tmi = ((ulong)ymi) << 32 | (ulong)ylo;
			tlo = 0;

			if (xhi > thi || (xhi == thi && xmi >=tmi)) {
				Sub192(xlo, xmi, xhi, tlo, tmi, thi, ref xlo, ref xmi, ref xhi);
				extraBit = 1;
			} else {
				extraBit = 0;
			}
			
			Div192By128To128 (xlo, xmi, xhi, ylo, ymi, ymi2, yhi, ref clo, ref chi);
 
			exp = 128 + ashift - bshift;

			if (extraBit != 0) {
				RShift128 (ref clo, ref chi);
				chi += LIT_GUINT64_HIGHBIT;
				exp--;
			}

			// try loss free right shift
			while (exp > 0 && (clo & 1) == 0) {
				RShift128 (ref clo, ref chi);
				exp--;
			}
		}

		// From decimal.c
		/*
		private static void RShift192(ref ulong lo, ref ulong mi, ref ulong hi)
		{
			lo >>= 1;
			if ((mi & 1) != 0)
				lo |= LIT_GUINT64_HIGHBIT;
			
			mi >>= 1;
			if ((hi & 1) != 0)
				mi |= LIT_GUINT64_HIGHBIT;

			hi >>= 1;
		}
		*/

		// From decimal.c
		private static void RShift128(ref ulong lo, ref ulong hi)
		{
			lo >>=1;
			if ((hi & 1) != 0) 
				lo |= LIT_GUINT64_HIGHBIT;
			hi >>= 1;
		}
		
		// From decimal.c
		private static void LShift128(ref ulong lo, ref ulong hi)
		{
			hi <<= 1;

			if ((lo & LIT_GUINT64_HIGHBIT) != 0) 
				hi++;

			lo <<= 1;
		}
		
		// From decimal.c
		private static void LShift128(ref uint lo, ref uint mi, ref uint mi2, ref uint hi)
		{
			hi <<= 1;
			if ((mi2 & LIT_GUINT32_HIGHBIT) != 0) 
				hi++;

			mi2 <<= 1;
			if ((mi & LIT_GUINT32_HIGHBIT) != 0) 
				mi2++;
			
			mi <<= 1;
			if ((lo & LIT_GUINT32_HIGHBIT) != 0) 
				mi++;

			lo <<= 1;
		}

		// From decimal.c
		private static void Div192By128To128 (ulong xlo, ulong xmi, ulong xhi,
					       uint ylo, uint ymi, uint ymi2, 
					       uint yhi, ref ulong clo, ref ulong chi)
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

			chi = (((ulong)h) << 32) | Div192By128To32WithRest (
				ref rlo, ref rmi, ref rhi, ylo, ymi, ymi2, yhi);

			// low 32 bit
			rhi = (rhi << 32) | (rmi >> 32);
			rmi = (rmi << 32) | (rlo >> 32);
			rlo <<= 32;

			h = Div192By128To32WithRest (ref rlo, ref rmi, ref rhi,
						     ylo, ymi, ymi2, yhi);

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
		private static uint Div192By128To32WithRest(ref ulong xlo, ref ulong xmi,
						ref ulong xhi, uint ylo, 
						uint ymi, uint ymi2, uint yhi)
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
		/*
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
		*/

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
		private static void Mult128By32To128(uint xlo, uint xmi, uint xmi2, uint xhi,
					 uint factor, ref ulong clo, ref ulong chi)
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
		private static void Add192 (ulong xlo, ulong xmi, ulong xhi,
				     ulong ylo, ulong ymi, ulong yhi,
				     ref ulong clo, ref ulong cmi, ref ulong chi)
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
		private static void Sub192 (ulong xlo, ulong xmi, ulong xhi,
				     ulong ylo, ulong ymi, ulong yhi,
				     ref ulong lo, ref ulong mi, ref ulong hi)
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

		public static SqlDecimal Truncate (SqlDecimal n, int position)
		{
			int diff = n.scale - position;
			if (diff == 0)
				return n;
			int [] data = n.Data;
			decimal d = new decimal (data [0], data [1], data [2], !n.positive, 0);
			decimal x = 10;
			for (int i = 0; i < diff; i++, x *= 10)
				d = d - d % x;
			data = Decimal.GetBits (d);
			data [3] = 0;
			return new SqlDecimal (n.precision, n.scale, n.positive, data);
		}

		public static SqlDecimal operator + (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull)
				return SqlDecimal.Null;
			 //if one of them is negative, perform subtraction
			if (x.IsPositive && !y.IsPositive){
				y = new SqlDecimal (y.Precision, y.Scale, !y.IsPositive, y.Data);
				return (x - y);
			}
			if (!x.IsPositive && y.IsPositive){
				x = new SqlDecimal (x.Precision, x.Scale, !x.IsPositive, x.Data);
				return (y - x);
			}
			if (!x.IsPositive && !y.IsPositive){
				x = new SqlDecimal (x.Precision, x.Scale, !x.IsPositive, x.Data);
				y = new SqlDecimal (y.Precision, y.Scale, !y.IsPositive, y.Data);
				x = (x + y);
				return new SqlDecimal (x.Precision, x.Scale, !x.IsPositive, x.Data);
			}
			// adjust the scale to the larger of the two beforehand
			if (x.scale > y.scale)
				y = SqlDecimal.AdjustScale (y, x.scale - y.scale, false); 
			else if (y.scale > x.scale)
				x = SqlDecimal.AdjustScale (x, y.scale - x.scale, false); 

			byte resultPrecision = (byte)(Math.Max (x.Scale, y.Scale) +
	 					 Math.Max (x.Precision - x.Scale, y.Precision - y.Scale) + 1);

			if (resultPrecision > MaxPrecision)
				resultPrecision = MaxPrecision;
			
			int [] xData = x.Data; 
			int [] yData = y.Data;
			int [] resultBits = new int[4];
			ulong carry = 0;
			ulong res = 0;
			for (int i = 0; i < 4; i++){
				res = (ulong)((uint)xData [i]) + (ulong)((uint)yData [i]) + carry;
				resultBits [i] = (int) (res & (UInt32.MaxValue));
				carry = res >> 32;
			}

			if (carry > 0)
				throw new OverflowException ();
			else
				return new SqlDecimal (resultPrecision, x.Scale, x.IsPositive, resultBits);
		}

		public static SqlDecimal operator / (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull)
				return SqlDecimal.Null;

			return DecimalDiv (x, y);
		}

		public static SqlBoolean operator == (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			if (x.IsPositive != y.IsPositive)
				return SqlBoolean.False;

			if (x.Scale > y.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, false);
			else if (y.Scale > x.Scale)
				x = SqlDecimal.AdjustScale(y, y.Scale - x.Scale, false);

			for (int i = 0; i < 4; i += 1)
				if (x.Data[i] != y.Data[i])
					return SqlBoolean.False;

			return SqlBoolean.True;
		}

		public static SqlBoolean operator > (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			if (x.IsPositive != y.IsPositive)
				return new SqlBoolean (x.IsPositive);

			if (x.Scale > y.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, false);
			else if (y.Scale > x.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, false);

			for (int i = 3; i >= 0; i--)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new SqlBoolean (x.Data[i] > y.Data[i]);
			}
			return new SqlBoolean (false);
		}

		public static SqlBoolean operator >= (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.IsPositive != y.IsPositive)
				return new SqlBoolean (x.IsPositive);

			if (x.Scale > y.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);
			else if (y.Scale > x.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new SqlBoolean (x.Data[i] >= y.Data[i]);
			}
			return new SqlBoolean (true);
		}

		public static SqlBoolean operator != (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			if (x.IsPositive != y.IsPositive)
				return SqlBoolean.True;

			if (x.Scale > y.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 0; i < 4; i += 1)
			{
				if (x.Data[i] != y.Data[i])
					return SqlBoolean.True;
			}
			return SqlBoolean.False;
		}

		public static SqlBoolean operator < (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			if (x.IsPositive != y.IsPositive)
				return new SqlBoolean (y.IsPositive);

			if (x.Scale > y.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);
			else if (y.Scale > x.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);

			for (int i = 3; i >= 0; i -= 1) {
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				return new SqlBoolean (x.Data[i] < y.Data[i]);
			}
			return new SqlBoolean (false);
		}

		public static SqlBoolean operator <= (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.IsPositive != y.IsPositive)
				return new SqlBoolean (y.IsPositive);

			if (x.Scale > y.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);
			else if (y.Scale > x.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);

			for (int i = 3; i >= 0; i -= 1) {
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new SqlBoolean (x.Data[i] <= y.Data[i]);
			}
			return new SqlBoolean (true);
		}

		public static SqlDecimal operator * (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull)
				return SqlDecimal.Null;

			// set the precision to the greater of the two
			byte resultPrecision = (byte)(x.Precision + y.Precision + 1);
			byte resultScale = (byte)(x.Scale + y.Scale);
			if (resultPrecision > MaxPrecision)
				resultPrecision = MaxPrecision;

			int[] xData = x.Data;
			int[] yData = y.Data;
			int[] resultBits = new int[4];

			ulong res; 
			ulong carry = 0;

			for (int i=0; i<4; ++i) {
				res = 0;
				for (int j=i; j<=i; ++j)
					res += ((ulong)(uint) xData[j]) *  ((ulong)(uint) yData[i-j]);
				resultBits [i] = (int) ((res + carry) & UInt32.MaxValue);
				carry = res >> 32;
			}
			
			// if we have carry left, then throw an exception
			if (carry > 0)
				throw new OverflowException ();
			else
				return new SqlDecimal (resultPrecision, resultScale, (x.IsPositive == y.IsPositive), resultBits);
		}

		public static SqlDecimal operator - (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull)
				return SqlDecimal.Null;

			if (x.IsPositive && !y.IsPositive){
				y = new SqlDecimal (y.Precision, y.Scale, !y.IsPositive, y.Data);
				return x + y;
			}
			if (!x.IsPositive && y.IsPositive){
				x = new SqlDecimal (x.Precision, x.Scale, !x.IsPositive, x.Data);
				x = (x + y);
				return new SqlDecimal (x.Precision, x.Scale, false, x.Data);
			}
			if (!x.IsPositive && !y.IsPositive){
				y = new SqlDecimal (y.Precision, y.Scale, !y.IsPositive, y.Data);
				x = new SqlDecimal(x.Precision, x.Scale, !x.IsPositive, x.Data);
				return (y - x);
			}
			// adjust the scale to the larger of the two beforehand
			if (x.scale > y.scale)
				y = SqlDecimal.AdjustScale (y, x.scale - y.scale, false); 
			else if (y.scale > x.scale)
				x = SqlDecimal.AdjustScale (x, y.scale - x.scale, false);

			//calculation of the new Precision for the result
			byte resultPrecision = (byte)(Math.Max (x.Scale, y.Scale) +
					Math.Max (x.Precision - x.Scale, y.Precision - y.Scale));

			int[] op1_Data;
			int[] op2_Data;
			if (x >= y) {
				op1_Data = x.Data;
				op2_Data = y.Data;
			} else {
				op1_Data = y.Data;
				op2_Data = x.Data;
			}

			ulong res = 0;
			int carry = 0;
			int[] resultBits = new int[4];


			/*
			 if ((uint)op2_Data [i] > (uint)op1_Data [i]) {
				 carry = UInt32.MaxValue;
				 op2_Data [i] = op2_Data [i] >> 1;
			 } else
				 carr = 0;
				res = (uint)carry; +(ulong)((uint)op1_Data [i]) - (ulong)((uint)op2_Data [i]) 
			*/

			for (int i = 0; i < 4; i += 1) {
				res = (ulong)((uint)op1_Data [i]) - (ulong)((uint)op2_Data [i]) + (ulong)carry;
				carry = 0;
				if ((uint)op2_Data [i] > (uint)op1_Data [i])
					carry = -1;
				resultBits [i] = (int)res;
			}

			if (carry > 0)
				throw new OverflowException ();
			else
				return new SqlDecimal (resultPrecision, x.Scale, (x>=y).Value, resultBits);
		}

		public static SqlDecimal operator - (SqlDecimal x)
		{
			return new SqlDecimal (x.Precision, x.Scale, !x.IsPositive, x.Data);
		}

		public static explicit operator SqlDecimal (SqlBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDecimal ((decimal)x.ByteValue);
		}

		public static explicit operator Decimal (SqlDecimal x)
		{
			return x.Value;
		}

		public static explicit operator SqlDecimal (SqlDouble x)
		{
			checked {
				if (x.IsNull) 
					return Null;
				else
					return new SqlDecimal ((double)x.Value);
			}
		}

		public static explicit operator SqlDecimal (SqlSingle x)
		{
			checked {
				if (x.IsNull) 
					return Null;
				else
					return new SqlDecimal ((double)x.Value);
			}
		}

		public static explicit operator SqlDecimal (SqlString x)
		{
			checked {
				return Parse (x.Value);
			}
		}

#if NET_2_0
		public static explicit operator SqlDecimal (double x)
		{
			return new SqlDecimal (x);
		}

		public static implicit operator SqlDecimal (long x)
		{
			return new SqlDecimal (x);
		}
#endif

		public static implicit operator SqlDecimal (decimal x)
		{
			return new SqlDecimal (x);
		}

		public static implicit operator SqlDecimal (SqlByte x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDecimal ((decimal) x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt16 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDecimal ((decimal) x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDecimal ((decimal) x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDecimal ((decimal) x.Value);
		}

		public static implicit operator SqlDecimal (SqlMoney x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlDecimal ((decimal) x.Value);
		}

#if NET_2_0
		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			if (schemaSet != null && schemaSet.Count == 0) {
				XmlSchema xs = new XmlSchema ();
				XmlSchemaComplexType ct = new XmlSchemaComplexType ();
				ct.Name = "decimal";
				xs.Items.Add (ct);
				schemaSet.Add (xs);
			}
			return new XmlQualifiedName ("decimal", "http://www.w3.org/2001/XMLSchema");
		}
		
		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}
		
		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			SqlDecimal retval;

			if (reader == null)
				return;

			switch (reader.ReadState) {
			case ReadState.EndOfFile:
			case ReadState.Error:
			case ReadState.Closed:
				return;
			}

			// Skip XML declaration and prolog
			// or do I need to validate for the <SqlInt32> tag?
			reader.MoveToContent ();
			if (reader.EOF)
				return;

			reader.Read ();
			if (reader.NodeType == XmlNodeType.EndElement)
				return;

			if (reader.Value.Length > 0) {
				if (String.Compare ("Null", reader.Value) == 0) {
					// means a null reference/invalid value
					notNull = false;
					return; 
				}
				// FIXME: do we need to handle the FormatException?
				retval = new SqlDecimal (Decimal.Parse (reader.Value));

				// SqlDecimal.Data returns a clone'd array
				this.value = retval.Data; 
				this.notNull = true;
				this.scale = retval.Scale;
				this.precision = retval.Precision;
				this.positive = retval.IsPositive;
			}
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			writer.WriteString (this.Value.ToString ());
		}
#endif

		#endregion
	}
}
