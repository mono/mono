//
// Mono.Data.TdsTypes.TdsDecimal
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
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

namespace Mono.Data.TdsTypes {
	public struct TdsDecimal : INullable, IComparable
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

		public static readonly TdsDecimal MaxValue = new TdsDecimal (MaxPrecision, (byte)0, true, (int)716002642, Int32.MaxValue, (int)1518778966, (int)1262177448);
		public static readonly TdsDecimal MinValue = new TdsDecimal (MaxPrecision, (byte)0, false, (int)716002642, Int32.MaxValue, (int)1518778966, (int)1262177448);
		public static readonly TdsDecimal Null;

		#endregion

		#region Constructors

		public TdsDecimal (decimal value) 
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

		public TdsDecimal (double value) : this ((decimal)value) { }
		public TdsDecimal (int value) : this ((decimal)value) { }
		public TdsDecimal (long value) : this ((decimal)value) { }

		public TdsDecimal (byte bPrecision, byte bScale, bool fPositive, int[] bits) : this (bPrecision, bScale, fPositive, bits[0], bits[1], bits[2], bits[3]) { }

		public TdsDecimal (byte bPrecision, byte bScale, bool fPositive, int data1, int data2, int data3, int data4) 
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
					throw new TdsNullValueException ();
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
					throw new TdsNullValueException ();
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
		public static TdsDecimal Abs (TdsDecimal n)
		{
			throw new NotImplementedException();
		}

		public static TdsDecimal Add (TdsDecimal x, TdsDecimal y)
		{
			return (x + y);
		}

		[MonoTODO]
		public static TdsDecimal AdjustScale (TdsDecimal n, int digits, bool fRound)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsDecimal Ceiling (TdsDecimal n)
		{
			throw new NotImplementedException();
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is TdsDecimal))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsDecimal"));
			else if (((TdsDecimal)value).IsNull)
				return 1;
			else
				return this.Value.CompareTo (((TdsDecimal)value).Value);
		}

		[MonoTODO]
		public static TdsDecimal ConvertToPrecScale (TdsDecimal n, int precision, int scale)
		{
			throw new NotImplementedException ();
		}

		public static TdsDecimal Divide (TdsDecimal x, TdsDecimal y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsDecimal))
				return false;
			else
				return (bool) (this == (TdsDecimal)value);
		}

		public static TdsBoolean Equals (TdsDecimal x, TdsDecimal y)
		{
			return (x == y);
		}

		[MonoTODO]
		public static TdsDecimal Floor (TdsDecimal n)
		{
			throw new NotImplementedException ();
		}

		internal static TdsDecimal FromTdsBigDecimal (TdsBigDecimal x)
		{
			if (x == null)
				return Null;
			else
				return new TdsDecimal (x.Precision, x.Scale, !x.IsNegative, x.Data);
                }

		public override int GetHashCode ()
		{
			return (int)this.Value;
		}

		public static TdsBoolean GreaterThan (TdsDecimal x, TdsDecimal y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsDecimal x, TdsDecimal y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsDecimal x, TdsDecimal y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsDecimal x, TdsDecimal y)
		{
			return (x <= y);
		}

		public static TdsDecimal Multiply (TdsDecimal x, TdsDecimal y)
		{
			return (x * y);
		}

		public static TdsBoolean NotEquals (TdsDecimal x, TdsDecimal y)
		{
			return (x != y);
		}

		[MonoTODO]
		public static TdsDecimal Parse (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsDecimal Power (TdsDecimal n, double exp)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsDecimal Round (TdsDecimal n, int position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsInt32 Sign (TdsDecimal n)
		{
			throw new NotImplementedException ();
		}

		public static TdsDecimal Subtract (TdsDecimal x, TdsDecimal y)
		{
			return (x - y);
		}

		public double ToDouble ()
		{
			return ((double)this.Value);
		}

		public TdsBoolean ToTdsBoolean ()
		{
			return ((TdsBoolean)this);
		}
		
		public TdsByte ToTdsByte ()
		{
			return ((TdsByte)this);
		}

		public TdsDouble ToTdsDouble ()
		{
			return ((TdsDouble)this);
		}

		public TdsInt16 ToTdsInt16 ()
		{
			return ((TdsInt16)this);
		}

		public TdsInt32 ToTdsInt32 ()
		{
			return ((TdsInt32)this);
		}

		public TdsInt64 ToTdsInt64 ()
		{
			return ((TdsInt64)this);
		}

		public TdsMoney ToTdsMoney ()
		{
			return ((TdsMoney)this);
		}

		public TdsSingle ToTdsSingle ()
		{
			return ((TdsSingle)this);
		}

		public TdsString ToTdsString ()
		{
			return ((TdsString)this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return String.Empty;
			else
				return value.ToString ();
		}

		[MonoTODO]
		public static TdsDecimal Truncate (TdsDecimal n, int position)
		{
			throw new NotImplementedException ();
		}

		public static TdsDecimal operator + (TdsDecimal x, TdsDecimal y)
		{
			// if one of them is negative, perform subtraction
			if (x.IsPositive && !y.IsPositive) return x - y;
			if (y.IsPositive && !x.IsPositive) return y - x;
		
			// adjust the scale to the smaller of the two beforehand
			if (x.Scale > y.Scale)
				x = TdsDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = TdsDecimal.AdjustScale(y, x.Scale - y.Scale, true);

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
				return new TdsDecimal (resultPrecision, x.Scale, x.IsPositive, resultBits);
		}

		[MonoTODO]
		public static TdsDecimal operator / (TdsDecimal x, TdsDecimal y)
		{
			throw new NotImplementedException ();
		}

		public static TdsBoolean operator == (TdsDecimal x, TdsDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;

			if (x.Scale > y.Scale)
				x = TdsDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = TdsDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 0; i < 4; i += 1)
			{
				if (x.Data[i] != y.Data[i])
					return new TdsBoolean (false);
			}
			return new TdsBoolean (true);
		}

		public static TdsBoolean operator > (TdsDecimal x, TdsDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;

			if (x.Scale > y.Scale)
				x = TdsDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = TdsDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new TdsBoolean (x.Data[i] > y.Data[i]);
			}
			return new TdsBoolean (false);
		}

		public static TdsBoolean operator >= (TdsDecimal x, TdsDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;

			if (x.Scale > y.Scale)
				x = TdsDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = TdsDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new TdsBoolean (x.Data[i] >= y.Data[i]);
			}
			return new TdsBoolean (true);
		}

		public static TdsBoolean operator != (TdsDecimal x, TdsDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;

			if (x.Scale > y.Scale)
				x = TdsDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = TdsDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 0; i < 4; i += 1)
			{
				if (x.Data[i] != y.Data[i])
					return new TdsBoolean (true);
			}
			return new TdsBoolean (false);
		}

		public static TdsBoolean operator < (TdsDecimal x, TdsDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;

			if (x.Scale > y.Scale)
				x = TdsDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = TdsDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;

				return new TdsBoolean (x.Data[i] < y.Data[i]);
			}
			return new TdsBoolean (false);
		}

		public static TdsBoolean operator <= (TdsDecimal x, TdsDecimal y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;

			if (x.Scale > y.Scale)
				x = TdsDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = TdsDecimal.AdjustScale(y, x.Scale - y.Scale, true);

			for (int i = 3; i >= 0; i -= 1)
			{
				if (x.Data[i] == 0 && y.Data[i] == 0) 
					continue;
				else
					return new TdsBoolean (x.Data[i] <= y.Data[i]);
			}
			return new TdsBoolean (true);
		}

		public static TdsDecimal operator * (TdsDecimal x, TdsDecimal y)
		{
			// adjust the scale to the smaller of the two beforehand
			if (x.Scale > y.Scale)
				x = TdsDecimal.AdjustScale(x, y.Scale - x.Scale, true);
			else if (y.Scale > x.Scale)
				y = TdsDecimal.AdjustScale(y, x.Scale - y.Scale, true);

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
				return new TdsDecimal (resultPrecision, x.Scale, (x.IsPositive == y.IsPositive), resultBits);
				
		}

		public static TdsDecimal operator - (TdsDecimal x, TdsDecimal y)
		{
			if (x.IsPositive && !y.IsPositive) return x + y;
			if (!x.IsPositive && y.IsPositive) return -(x + y);
			if (!x.IsPositive && !y.IsPositive) return y - x;

			// otherwise, x is positive and y is positive
			bool resultPositive = (bool)(x > y);
			int[] yData = y.Data;

			for (int i = 0; i < 4; i += 1) yData[i] = -yData[i];

			TdsDecimal yInverse = new TdsDecimal (y.Precision, y.Scale, y.IsPositive, yData);

			if (resultPositive)
				return x + yInverse;
			else
				return -(x + yInverse);
		}

		public static TdsDecimal operator - (TdsDecimal n)
		{
			return new TdsDecimal (n.Precision, n.Scale, !n.IsPositive, n.Data);
		}

		public static explicit operator TdsDecimal (TdsBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDecimal ((decimal)x.ByteValue);
		}

		public static explicit operator Decimal (TdsDecimal n)
		{
			return n.Value;
		}

		public static explicit operator TdsDecimal (TdsDouble x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDecimal ((decimal)x.Value);
		}

		public static explicit operator TdsDecimal (TdsSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDecimal ((decimal)x.Value);
		}

		[MonoTODO]
		public static explicit operator TdsDecimal (TdsString x)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator TdsDecimal (decimal x)
		{
			return new TdsDecimal (x);
		}

		public static implicit operator TdsDecimal (TdsByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDecimal ((decimal)x.Value);
		}

		public static implicit operator TdsDecimal (TdsInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDecimal ((decimal)x.Value);
		}

		public static implicit operator TdsDecimal (TdsInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDecimal ((decimal)x.Value);
		}

		public static implicit operator TdsDecimal (TdsInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDecimal ((decimal)x.Value);
		}

		public static implicit operator TdsDecimal (TdsMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDecimal ((decimal)x.Value);
		}

		#endregion
	}
}
			
