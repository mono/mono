//
// Mono.Data.TdsTypes.TdsInt16
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
	public struct TdsInt16 : INullable, IComparable
	{
		#region Fields

		short value;
		private bool notNull;

		public static readonly TdsInt16 MaxValue = new TdsInt16 (32767);
		public static readonly TdsInt16 MinValue = new TdsInt16 (-32768);
		public static readonly TdsInt16 Null;
		public static readonly TdsInt16 Zero = new TdsInt16 (0);

		#endregion

		#region Constructors

		public TdsInt16 (short value) 
		{
			this.value = value;
			notNull = true;;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return !notNull; }
		}

		public short Value { 
			get { 
				if (this.IsNull) 
					throw new TdsNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static TdsInt16 Add (TdsInt16 x, TdsInt16 y)
		{
			return (x + y);
		}

		public static TdsInt16 BitwiseAnd (TdsInt16 x, TdsInt16 y)
		{
			return (x & y);
		}

		public static TdsInt16 BitwiseOr (TdsInt16 x, TdsInt16 y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is TdsInt16))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsInt16"));
			else if (((TdsInt16)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsInt16)value).Value);
		}

		public static TdsInt16 Divide (TdsInt16 x, TdsInt16 y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsInt16))
				return false;
			else
				return (bool) (this == (TdsInt16)value);
		}

		public static TdsBoolean Equals (TdsInt16 x, TdsInt16 y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static TdsBoolean GreaterThan (TdsInt16 x, TdsInt16 y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsInt16 x, TdsInt16 y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsInt16 x, TdsInt16 y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsInt16 x, TdsInt16 y)
		{
			return (x <= y);
		}

		public static TdsInt16 Mod (TdsInt16 x, TdsInt16 y)
		{
			return (x % y);
		}

		public static TdsInt16 Multiply (TdsInt16 x, TdsInt16 y)
		{
			return (x * y);
		}

		public static TdsBoolean NotEquals (TdsInt16 x, TdsInt16 y)
		{
			return (x != y);
		}

		public static TdsInt16 OnesComplement (TdsInt16 x)
		{
			return ~x;
		}

		public static TdsInt16 Parse (string s)
		{
			return new TdsInt16 (Int16.Parse (s));
		}

		public static TdsInt16 Subtract (TdsInt16 x, TdsInt16 y)
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
				return "Null";
			else
				return value.ToString ();
		}

		public static TdsInt16 Xor (TdsInt16 x, TdsInt16 y)
		{
			return (x ^ y);
		}

		public static TdsInt16 operator + (TdsInt16 x, TdsInt16 y)
		{
			return new TdsInt16 ((short) (x.Value + y.Value));
		}

		public static TdsInt16 operator & (TdsInt16 x, TdsInt16 y)
		{
			return new TdsInt16 ((short) (x.value & y.Value));
		}

		public static TdsInt16 operator | (TdsInt16 x, TdsInt16 y)
		{
			return new TdsInt16 ((short) ((byte) x.Value | (byte) y.Value));
		}

		public static TdsInt16 operator / (TdsInt16 x, TdsInt16 y)
		{
			return new TdsInt16 ((short) (x.Value / y.Value));
		}

		public static TdsBoolean operator == (TdsInt16 x, TdsInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}

		public static TdsInt16 operator ^ (TdsInt16 x, TdsInt16 y)
		{
			return new TdsInt16 ((short) (x.Value ^ y.Value));
		}

		public static TdsBoolean operator > (TdsInt16 x, TdsInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value > y.Value);
		}

		public static TdsBoolean operator >= (TdsInt16 x, TdsInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value >= y.Value);
		}

		public static TdsBoolean operator != (TdsInt16 x, TdsInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else 
				return new TdsBoolean (!(x.Value == y.Value));
		}

		public static TdsBoolean operator < (TdsInt16 x, TdsInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value < y.Value);
		}

		public static TdsBoolean operator <= (TdsInt16 x, TdsInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value <= y.Value);
		}

		public static TdsInt16 operator % (TdsInt16 x, TdsInt16 y)
		{
			return new TdsInt16 ((short) (x.Value % y.Value));
		}

		public static TdsInt16 operator * (TdsInt16 x, TdsInt16 y)
		{
			return new TdsInt16 ((short) (x.Value * y.Value));
		}

		public static TdsInt16 operator ~ (TdsInt16 x)
		{
			return new TdsInt16 ((short) (~x.Value));
		}

		public static TdsInt16 operator - (TdsInt16 x, TdsInt16 y)
		{
			return new TdsInt16 ((short) (x.Value - y.Value));
		}

		public static TdsInt16 operator - (TdsInt16 n)
		{
			return new TdsInt16 ((short) (-n.Value));
		}

		public static explicit operator TdsInt16 (TdsBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsInt16 ((short)x.ByteValue);
		}

		public static explicit operator TdsInt16 (TdsDecimal x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsInt16 ((short)x.Value);
		}

		public static explicit operator TdsInt16 (TdsDouble x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsInt16 ((short)x.Value);
		}

		public static explicit operator short (TdsInt16 x)
		{
			return x.Value; 
		}

		public static explicit operator TdsInt16 (TdsInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsInt16 ((short)x.Value);
		}

		public static explicit operator TdsInt16 (TdsInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsInt16 ((short)x.Value);
		}

		public static explicit operator TdsInt16 (TdsMoney x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsInt16 ((short)x.Value);
		}

		public static explicit operator TdsInt16 (TdsSingle x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsInt16 ((short)x.Value);
		}

		public static explicit operator TdsInt16 (TdsString x)
		{
			return TdsInt16.Parse (x.Value);
		}

		public static implicit operator TdsInt16 (short x)
		{
			return new TdsInt16 (x);
		}

		public static implicit operator TdsInt16 (TdsByte x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsInt16 ((short)x.Value);
		}

		#endregion
	}
}
			
