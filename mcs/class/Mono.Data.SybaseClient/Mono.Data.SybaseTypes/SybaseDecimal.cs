//
// Mono.Data.SybaseTypes.SybaseDecimal
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright Tim Coleman, 2002
//

using Mono.Data.Tds.Protocol;
using System;
using System.Data.SqlTypes;
using System.Globalization;

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

			this.precision = MaxPrecision; // this value seems unclear

			this.scale = (byte)(binData[3] >> SCALE_SHIFT);
			if (this.scale > MaxScale || (this.scale & RESERVED_SS32_BITS) != 0)
				throw new ArgumentException(Locale.GetText ("Invalid scale"));

			this.positive = ((binData[3] >> SIGN_SHIFT) > 0);
			this.value = new int[4];
			this.value[0] = binData[0];
			this.value[1] = binData[1];
			this.value[2] = binData[2];
			this.value[3] = 0;
			notNull = true;
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
				else 
					if (this.value[3] > 0)
						throw new OverflowException ();
					else
						System.Console.WriteLine( "boo!" );
						return new decimal (value[0], value[1], value[2], !positive, scale);
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public static SybaseDecimal Abs (SybaseDecimal n)
		{
			throw new NotImplementedException();
		}

		public static SybaseDecimal Add (SybaseDecimal x, SybaseDecimal y)
		{
			return (x + y);
		}

		[MonoTODO]
		public static SybaseDecimal AdjustScale (SybaseDecimal n, int digits, bool fRound)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseDecimal Ceiling (SybaseDecimal n)
		{
			throw new NotImplementedException();
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

		[MonoTODO]
		public static SybaseDecimal ConvertToPrecScale (SybaseDecimal n, int precision, int scale)
		{
			throw new NotImplementedException ();
		}

		public static SybaseDecimal Divide (SybaseDecimal x, SybaseDecimal y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseDecimal))
				return false;
			else
				return (bool) (this == (SybaseDecimal)value);
		}

		public static SybaseBoolean Equals (SybaseDecimal x, SybaseDecimal y)
		{
			return (x == y);
		}

		[MonoTODO]
		public static SybaseDecimal Floor (SybaseDecimal n)
		{
			throw new NotImplementedException ();
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
			return (int)this.Value;
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

		[MonoTODO]
		public static SybaseDecimal Parse (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseDecimal Power (SybaseDecimal n, double exp)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseDecimal Round (SybaseDecimal n, int position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseInt32 Sign (SybaseDecimal n)
		{
			throw new NotImplementedException ();
		}

		public static SybaseDecimal Subtract (SybaseDecimal x, SybaseDecimal y)
		{
			return (x - y);
		}

		public double ToDouble ()
		{
			return ((double)this.Value);
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
			else
				return value.ToString ();
		}

		[MonoTODO]
		public static SybaseDecimal Truncate (SybaseDecimal n, int position)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public static SybaseDecimal operator / (SybaseDecimal x, SybaseDecimal y)
		{
			throw new NotImplementedException ();
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
			
