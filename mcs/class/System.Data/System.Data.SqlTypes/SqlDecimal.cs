//
// System.Data.SqlTypes.SqlDecimal
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;
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

		public static readonly byte MaxPrecision = 38; 
		public static readonly byte MaxScale = 28;
		public static readonly SqlDecimal MaxValue = new SqlDecimal (79228162514264337593543950335.0);
		public static readonly SqlDecimal MinValue = new SqlDecimal (-79228162514264337593543950335.0);
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

			this.positive = ((binData[3] >> SIGN_SHIFT) > 0);
			this.value = new int[4];
			this.value[0] = binData[0];
			this.value[1] = binData[1];
			this.value[2] = binData[2];
			this.value[3] = 0;
			notNull = true;
		}

		public SqlDecimal (double value) : this ((decimal)value) { }
		public SqlDecimal (int value) : this ((decimal)value) { }
		public SqlDecimal (long value) : this ((decimal)value) { }

		public SqlDecimal (byte bPrecision, byte bScale, bool fPositive, int[] bits) : this (bPrecision, bScale, fPositive, bits[0], bits[1], bits[2], bits[3]) { }

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
		}

		#endregion

		#region Properties

		[MonoTODO]
		public byte[] BinData {
			get { throw new NotImplementedException (); }
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

				return new decimal (value[0], value[1], value[2], positive, scale);
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public static SqlDecimal Abs (SqlDecimal n)
		{
			throw new NotImplementedException();
		}

		public static SqlDecimal Add (SqlDecimal x, SqlDecimal y)
		{
			return (x + y);
		}

		[MonoTODO]
		public static SqlDecimal AdjustScale (SqlDecimal n, int digits, bool fRound)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlDecimal Ceiling (SqlDecimal n)
		{
			throw new NotImplementedException();
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

		[MonoTODO]
		public static SqlDecimal ConvertToPrecScale (SqlDecimal n, int precision, int scale)
		{
			throw new NotImplementedException ();
		}

		public static SqlDecimal Divide (SqlDecimal x, SqlDecimal y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlDecimal))
				return false;
			else
				return (bool) (this == (SqlDecimal)value);
		}

		public static SqlBoolean Equals (SqlDecimal x, SqlDecimal y)
		{
			return (x == y);
		}

		[MonoTODO]
		public static SqlDecimal Floor (SqlDecimal n)
		{
			throw new NotImplementedException ();
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
			return (int)this.Value;
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
			throw new NotImplementedException ();		       
		}

		[MonoTODO]
		public static SqlDecimal Power (SqlDecimal n, double exp)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlDecimal Round (SqlDecimal n, int position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlInt32 Sign (SqlDecimal n)
		{
			throw new NotImplementedException ();
		}

		public static SqlDecimal Subtract (SqlDecimal x, SqlDecimal y)
		{
			return (x - y);
		}

		public double ToDouble ()
		{
			return ((double)this.Value);
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
			else
				return value.ToString ();
		}

		[MonoTODO]
		public static SqlDecimal Truncate (SqlDecimal n, int position)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public static SqlDecimal operator / (SqlDecimal x, SqlDecimal y)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean operator == (SqlDecimal x, SqlDecimal y)
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
					return new SqlBoolean (false);
			}
			return new SqlBoolean (true);
		}

		public static SqlBoolean operator > (SqlDecimal x, SqlDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;

			if (x.Scale > y.Scale)
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
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
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

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
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

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
				x = SqlDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = SqlDecimal.AdjustScale(y, x.Scale - y.Scale, true);

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
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static explicit operator SqlDecimal (SqlSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlDecimal ((decimal)x.Value);
		}

		public static explicit operator SqlDecimal (SqlString x)
		{
			return Parse (x.Value);
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
			
