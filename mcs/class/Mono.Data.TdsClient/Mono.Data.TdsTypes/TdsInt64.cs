//
// Mono.Data.TdsTypes.TdsInt64
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

using Mono.Data.TdsClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.TdsTypes {
	public struct TdsInt64 : INullable, IComparable
	{
		#region Fields

		long value;

		private bool notNull;
		
		public static readonly TdsInt64 MaxValue = new TdsInt64 (9223372036854775807);
		public static readonly TdsInt64 MinValue = new TdsInt64 (-9223372036854775808);

		public static readonly TdsInt64 Null;
		public static readonly TdsInt64 Zero = new TdsInt64 (0);

		#endregion

		#region Constructors

		public TdsInt64 (long value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return !notNull; }
		}

		public long Value { 
			get { 
				if (this.IsNull) 
					throw new TdsNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static TdsInt64 Add (TdsInt64 x, TdsInt64 y)
		{
			return (x + y);
		}

		public static TdsInt64 BitwiseAnd (TdsInt64 x, TdsInt64 y)
		{
			return (x & y);
		}

		public static TdsInt64 BitwiseOr (TdsInt64 x, TdsInt64 y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is TdsInt64))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsInt64"));
			else if (((TdsInt64)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsInt64)value).Value);
		}

		public static TdsInt64 Divide (TdsInt64 x, TdsInt64 y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsInt64))
				return false;
			else
				return (bool) (this == (TdsInt64)value);
		}

		public static TdsBoolean Equals (TdsInt64 x, TdsInt64 y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)(value & 0xffffffff) ^ (int)(value >> 32);
		}

		public static TdsBoolean GreaterThan (TdsInt64 x, TdsInt64 y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsInt64 x, TdsInt64 y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsInt64 x, TdsInt64 y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsInt64 x, TdsInt64 y)
		{
			return (x <= y);
		}

		public static TdsInt64 Mod (TdsInt64 x, TdsInt64 y)
		{
			return (x % y);
		}

		public static TdsInt64 Multiply (TdsInt64 x, TdsInt64 y)
		{
			return (x * y);
		}

		public static TdsBoolean NotEquals (TdsInt64 x, TdsInt64 y)
		{
			return (x != y);
		}

		public static TdsInt64 OnesComplement (TdsInt64 x)
		{
			return ~x;
		}


		public static TdsInt64 Parse (string s)
		{
			return new TdsInt64 (Int64.Parse (s));
		}

		public static TdsInt64 Subtract (TdsInt64 x, TdsInt64 y)
		{
			return (x - y);
		}

		public TdsBoolean ToTdsBoolean ()
		{
			return ((TdsBoolean)this);
		}
		
		public TdsByte ToTdsByte ()
		{
			return ((TdsByte)this);
		}

		public TdsDecimal ToTdsDecimal ()
		{
			return ((TdsDecimal)this);
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
				return "Null";

			return value.ToString ();
		}

		public static TdsInt64 Xor (TdsInt64 x, TdsInt64 y)
		{
			return (x ^ y);
		}

		public static TdsInt64 operator + (TdsInt64 x, TdsInt64 y)
		{
			return new TdsInt64 (x.Value + y.Value);
		}

		public static TdsInt64 operator & (TdsInt64 x, TdsInt64 y)
		{
			return new TdsInt64 (x.value & y.Value);
		}

		public static TdsInt64 operator | (TdsInt64 x, TdsInt64 y)
		{
			return new TdsInt64 (x.value | y.Value);
		}

		public static TdsInt64 operator / (TdsInt64 x, TdsInt64 y)
		{
			return new TdsInt64 (x.Value / y.Value);
		}

		public static TdsBoolean operator == (TdsInt64 x, TdsInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}

		public static TdsInt64 operator ^ (TdsInt64 x, TdsInt64 y)
		{
			return new TdsInt64 (x.Value ^ y.Value);
		}

		public static TdsBoolean operator > (TdsInt64 x, TdsInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value > y.Value);
		}

		public static TdsBoolean operator >= (TdsInt64 x, TdsInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value >= y.Value);
		}

		public static TdsBoolean operator != (TdsInt64 x, TdsInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (!(x.Value == y.Value));
		}

		public static TdsBoolean operator < (TdsInt64 x, TdsInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value < y.Value);
		}

		public static TdsBoolean operator <= (TdsInt64 x, TdsInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value <= y.Value);
		}

		public static TdsInt64 operator % (TdsInt64 x, TdsInt64 y)
		{
			return new TdsInt64(x.Value % y.Value);
		}

		public static TdsInt64 operator * (TdsInt64 x, TdsInt64 y)
		{
			return new TdsInt64 (x.Value * y.Value);
		}

		public static TdsInt64 operator ~ (TdsInt64 x)
		{
			return new TdsInt64 (~(x.Value));
		}

		public static TdsInt64 operator - (TdsInt64 x, TdsInt64 y)
		{
			return new TdsInt64 (x.Value - y.Value);
		}

		public static TdsInt64 operator - (TdsInt64 n)
		{
			return new TdsInt64 (-(n.Value));
		}

		public static explicit operator TdsInt64 (TdsBoolean x)
		{
			if (x.IsNull) 
				return TdsInt64.Null;
			else
				return new TdsInt64 ((long)x.ByteValue);
		}

		public static explicit operator TdsInt64 (TdsDecimal x)
		{
			if (x.IsNull) 
				return TdsInt64.Null;
			else
				return new TdsInt64 ((long)x.Value);
		}

		public static explicit operator TdsInt64 (TdsDouble x)
		{
			if (x.IsNull) 
				return TdsInt64.Null;
			else
				return new TdsInt64 ((long)x.Value);
		}

		public static explicit operator long (TdsInt64 x)
		{
			return x.Value;
		}

		public static explicit operator TdsInt64 (TdsMoney x)
		{
			if (x.IsNull) 
				return TdsInt64.Null;
			else
				return new TdsInt64 ((long)x.Value);
		}

		public static explicit operator TdsInt64 (TdsSingle x)
		{
			if (x.IsNull) 
				return TdsInt64.Null;
			else
				return new TdsInt64 ((long)x.Value);
		}

		public static explicit operator TdsInt64 (TdsString x)
		{
			return TdsInt64.Parse (x.Value);
		}

		public static implicit operator TdsInt64 (long x)
		{
			return new TdsInt64 (x);
		}

		public static implicit operator TdsInt64 (TdsByte x)
		{
			if (x.IsNull) 
				return TdsInt64.Null;
			else
				return new TdsInt64 ((long)x.Value);
		}

		public static implicit operator TdsInt64 (TdsInt16 x)
		{
			if (x.IsNull) 
				return TdsInt64.Null;
			else
				return new TdsInt64 ((long)x.Value);
		}

		public static implicit operator TdsInt64 (TdsInt32 x)
		{
			if (x.IsNull) 
				return TdsInt64.Null;
			else
				return new TdsInt64 ((long)x.Value);
		}

		#endregion
	}
}
			
