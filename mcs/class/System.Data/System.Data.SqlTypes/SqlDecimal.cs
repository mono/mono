//
// System.Data.SqlTypes.SqlDecimal
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@koti.soon.fi>
//
// (C) Copyright 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Text;
using System.Globalization;

namespace System.Data.SqlTypes
{
	public struct SqlDecimal : INullable, IComparable
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
		static uint [] constantsDecadeInt32Factors = new uint [10]
		        {
				1u, 10u, 100u, 1000u, 10000u, 100000u, 1000000u, 
				10000000u, 100000000u, 1000000000u
			};

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

			if (value >= 0)
				positive = true;
			else 
				positive = false;

			notNull = true;
			precision = GetPrecision (value);
		}
				
		public SqlDecimal (double value) : this ((decimal)value) { }
		public SqlDecimal (int value) : this ((decimal)value) { }
		public SqlDecimal (long value) : this ((decimal)value) { }

		public SqlDecimal (byte bPrecision, byte bScale, bool fPositive, int[] bits) : this (bPrecision, bScale, fPositive, bits[0], bits[1], bits[2], bits[3]) { }

		[MonoTODO]
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
				throw new ArgumentException(Locale.GetText ("Invalid scale"));

			// FIXME: What is the right message of Exception
			if (this.ToDouble () > (Math.Pow (10, 38) - 1)  || 
			    this.ToDouble () < -(Math.Pow (10, 38)))
				throw new SqlTypeException ("Can't convert to SqlDecimal");
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
				return new SqlDecimal (n.Precision, n.Scale, true, 
						       n.BinData [0], n.BinData [1], 
						       n.BinData [2], n.BinData [3]);
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

			if (digits > 0)
			        prec = (byte)(prec + digits);

			if (fRound)
				n = Round (n, digits + n.Scale);

			return new SqlDecimal (prec, 
					       (byte)(n.Scale + digits), 
					       n.IsPositive, n.Data);
		}

		public static SqlDecimal Ceiling (SqlDecimal n)
		{
			return AdjustScale (n, -(n.Scale), true);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlDecimal))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlDecimal"));
			else if (((SqlDecimal)value).IsNull)
				return 1;
			else
				return this.Value.CompareTo (((SqlDecimal)value).Value);
		}

		public static SqlDecimal ConvertToPrecScale (SqlDecimal n, int precision, int scale)
		{
			return new SqlDecimal ((byte)precision, (byte)scale, n.IsPositive, n.Data);
		}

		public static SqlDecimal Divide (SqlDecimal x, SqlDecimal y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlDecimal))
				return false;
			else if (this.IsNull && ((SqlDecimal)value).IsNull)
				return true;			
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

		internal static SqlDecimal FromTdsBigDecimal (TdsBigDecimal x)
		{
			if (x == null)
				return Null;
			else
				return new SqlDecimal (x.Precision, x.Scale, !x.IsNegative, x.Data);
		}

		public override int GetHashCode ()
		{
			int result = 10;
			result = 91 * result + this.Data[0];
			result = 91 * result + this.Data[1];
			result = 91 * result + this.Data[2];
			result = 91 * result + this.Data[3];
			result = 91 * result + (int)this.Scale;
			result = 91 * result + (int)this.Precision;

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

		[MonoTODO]
		public static SqlDecimal Parse (string s)
		{
			// FIXME: Huh, There must be better way to do this
			if (s == null)
				throw new ArgumentNullException ();
			else 
				return SqlDouble.Parse (s).ToSqlDecimal ();
		}

		public static SqlDecimal Power (SqlDecimal n, double exp)
		{
			if (n.IsNull)
				return SqlDecimal.Null;

			return new SqlDecimal (Math.Pow (n.ToDouble (), exp));
		}

		[MonoTODO]
		public static SqlDecimal Round (SqlDecimal n, int position)
		{
			// FIXME: There must be better way to do this
			if (n.IsNull)
				throw new SqlNullValueException ();

			SqlDecimal result = new SqlDecimal (Math.Round (
				(double)(n.ToDouble () * Math.Pow (10, position))));

			result = result / new SqlDecimal(Math.Pow (10, position));
			
			return result;				
		}

		public static SqlInt32 Sign (SqlDecimal n)
		{
			SqlInt32 result = 0;

			if (n >= new SqlDecimal (0))
				result = 1;
			else
				result = -1;

			return result;
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
			return ((SqlBoolean)this);
		}
		
		public SqlByte ToSqlByte ()
		{
			return ((SqlByte)this);
		}

		public SqlDouble ToSqlDouble ()
		{
			return ((SqlDouble)this);
		}

		public SqlInt16 ToSqlInt16 ()
		{
			return ((SqlInt16)this);
		}

		public SqlInt32 ToSqlInt32 ()
		{
			return ((SqlInt32)this);
		}

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64)this);
		}

		public SqlMoney ToSqlMoney ()
		{
			return ((SqlMoney)this);
		}

		public SqlSingle ToSqlSingle ()
		{
			return ((SqlSingle)this);
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString)this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return String.Empty;
			
			// convert int [4] --> ulong [2]
			ulong lo = (uint)this.Data [0];
			lo += (ulong)((ulong)this.Data [1] << 32);
			ulong hi = (uint)this.Data [2];
			hi += (ulong)((ulong)this.Data [3] << 32);

			uint rest = 0;
			String result = "";
			StringBuilder Result = new StringBuilder ();
			for (int i = 0; lo != 0 || hi != 0; i++) {
			
				Div128By32 (ref hi, ref lo, 10, ref rest);
				Result.Insert (0, rest.ToString ());
			}

			while (Result.Length < this.Precision)
			        Result.Append ("0");

			while (Result.Length > this.Precision)
			       Result.Remove (Result.Length - 1, 1);
 
			if (this.Scale > 0)
			        Result.Insert (Result.Length - this.Scale, ".");

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
			int rc = 0;
			byte prec = 0; // precision

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
				
			int resultLo = (int)lo;
			int resultMi = (int)(lo >> 32);
			int resultMi2 = (int)(hi);
			int resultHi = (int)(hi >> 32);

			return new SqlDecimal (prec, (byte)sc, true, resultLo,
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
			int rc = 0;
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
					System.Console.WriteLine ("***");
					Mult128By32 (ref clo, ref chi, factor, 0);
			       		System.Console.WriteLine ((((double)chi) * Math.Pow (2,64) + clo));

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
			int deltaScale;
			
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

			xhi = (ulong)((ulong)x.Data [3] << 32) | (ulong)x.Data [2];
			xmi = (ulong)((ulong)x.Data [1] << 32) | (ulong)x.Data [0];
			xlo = (uint)0;			
			ylo = (uint)y.Data [0];
			ymi = (uint)y.Data [1];
			ymi2 = (uint)y.Data [2];
			yhi = (uint)y.Data [3];
			
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
		private static void Mult192By32 (ref ulong clo, ref ulong cmi, ref ulong chi, ulong factor, int roundBit)
		{
			ulong a = 0;
			uint h0 = 0;
			uint h1 = 0;
			uint h2 = 0;

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
			uint h2 = 0;

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
			int prec = n.Precision;// + (position - n.Scale);
			int sc = position;
			return new SqlDecimal ((byte)prec, (byte)sc,
					       n.IsPositive, n.Data);
		}

		public static SqlDecimal operator + (SqlDecimal x, SqlDecimal y)
		{
			// if one of them is negative, perform subtraction
			if (x.IsPositive && !y.IsPositive) return x - y;
			if (y.IsPositive && !x.IsPositive) return y - x;
		
			// adjust the scale to the smaller of the two beforehand
			if (x.Scale > y.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

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
				return new SqlDecimal (resultPrecision, x.Scale, x.IsPositive, resultBits);
		}

		public static SqlDecimal operator / (SqlDecimal x, SqlDecimal y)
		{
			//			return new SqlDecimal (x.Value / y.Value);
     			return DecimalDiv (x, y);
		}

		public static SqlBoolean operator == (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			if (x.Scale > y.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, false);
			else if (y.Scale > x.Scale)
				x = SqlDecimal.AdjustScale(y, y.Scale - x.Scale, false);

			for (int i = 0; i < 4; i += 1)
			{
				if (x.Data[i] != y.Data[i])
					return new SqlBoolean (false);
			}
			return new SqlBoolean (true);
		}

		public static SqlBoolean operator > (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

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

			if (x.Scale > y.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 0; i < 4; i += 1)
			{
				if (x.Data[i] != y.Data[i])
					return new SqlBoolean (true);
			}
			return new SqlBoolean (false);
		}

		public static SqlBoolean operator < (SqlDecimal x, SqlDecimal y)
		{

			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			if (x.Scale > y.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);
			else if (y.Scale > x.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
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

			if (x.Scale > y.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);
			else if (y.Scale > x.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new SqlBoolean (x.Data[i] <= y.Data[i]);
			}
			return new SqlBoolean (true);
		}

		public static SqlDecimal operator * (SqlDecimal x, SqlDecimal y)
		{
			// adjust the scale to the smaller of the two beforehand
			if (x.Scale > y.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

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
				return new SqlDecimal (resultPrecision, x.Scale, (x.IsPositive == y.IsPositive), resultBits);
				
		}

		public static SqlDecimal operator - (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsPositive && !y.IsPositive) return x + y;
			if (!x.IsPositive && y.IsPositive) return -(x + y);
			if (!x.IsPositive && !y.IsPositive) return y - x;

			// otherwise, x is positive and y is positive
			bool resultPositive = (bool)(x > y);
			int[] yData = y.Data;

			for (int i = 0; i < 4; i += 1) yData[i] = -yData[i];

			SqlDecimal yInverse = new SqlDecimal (y.Precision, y.Scale, y.IsPositive, yData);

			if (resultPositive)
				return x + yInverse;
			else
				return -(x + yInverse);
		}

		public static SqlDecimal operator - (SqlDecimal n)
		{
			return new SqlDecimal (n.Precision, n.Scale, !n.IsPositive, n.Data);
		}

		public static explicit operator SqlDecimal (SqlBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.ByteValue);
		}

		public static explicit operator Decimal (SqlDecimal n)
		{
			return n.Value;
		}

		public static explicit operator SqlDecimal (SqlDouble x)
		{
			checked {
				if (x.IsNull) 
					return Null;
				else
					return new SqlDecimal ((decimal)x.Value);
			}
		}

		public static explicit operator SqlDecimal (SqlSingle x)
		{
			checked {
				if (x.IsNull) 
					return Null;
				else
					return new SqlDecimal ((decimal)x.Value);
			}
		}

		public static explicit operator SqlDecimal (SqlString x)
		{
			checked {
				return Parse (x.Value);
			}
		}

		public static implicit operator SqlDecimal (decimal x)
		{
			return new SqlDecimal (x);
		}

		public static implicit operator SqlDecimal (SqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static implicit operator SqlDecimal (SqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static implicit operator SqlDecimal (SqlMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		#endregion
	}
}
