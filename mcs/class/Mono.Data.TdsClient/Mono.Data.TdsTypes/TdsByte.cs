//
// Mono.Data.TdsTypes.TdsByte
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
	public struct TdsByte : INullable, IComparable
	{
		#region Fields

		byte value;
		private bool notNull;

		public static readonly TdsByte MaxValue = new TdsByte (0xff);
		public static readonly TdsByte MinValue = new TdsByte (0);
		public static readonly TdsByte Null;
		public static readonly TdsByte Zero = new TdsByte (0);

		#endregion

		#region Constructors

		public TdsByte (byte value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public byte Value { 
			get { 
				if (this.IsNull) 
					throw new TdsNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static TdsByte Add (TdsByte x, TdsByte y)
		{
			return (x + y);
		}

		public static TdsByte BitwiseAnd (TdsByte x, TdsByte y)
		{
			return (x & y);
		}

		public static TdsByte BitwiseOr (TdsByte x, TdsByte y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is TdsByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsByte"));
			else if (((TdsByte)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsByte)value).Value);
		}

		public static TdsByte Divide (TdsByte x, TdsByte y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsByte))
				return false;
			else
				return (bool) (this == (TdsByte)value);
		}

		public static TdsBoolean Equals (TdsByte x, TdsByte y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static TdsBoolean GreaterThan (TdsByte x, TdsByte y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsByte x, TdsByte y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsByte x, TdsByte y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsByte x, TdsByte y)
		{
			return (x <= y);
		}

		public static TdsByte Mod (TdsByte x, TdsByte y)
		{
			return (x % y);
		}

		public static TdsByte Multiply (TdsByte x, TdsByte y)
		{
			return (x * y);
		}

		public static TdsBoolean NotEquals (TdsByte x, TdsByte y)
		{
			return (x != y);
		}

		public static TdsByte OnesComplement (TdsByte x)
		{
			return ~x;
		}

		public static TdsByte Parse (string s)
		{
			return new TdsByte (Byte.Parse (s));
		}

		public static TdsByte Subtract (TdsByte x, TdsByte y)
		{
			return (x - y);
		}

		public TdsBoolean ToTdsBoolean ()
		{
			return ((TdsBoolean)this);
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

		public static TdsByte Xor (TdsByte x, TdsByte y)
		{
			return (x ^ y);
		}

		public static TdsByte operator + (TdsByte x, TdsByte y)
		{
			return new TdsByte ((byte) (x.Value + y.Value));
		}

		public static TdsByte operator & (TdsByte x, TdsByte y)
		{
			return new TdsByte ((byte) (x.Value & y.Value));
		}

		public static TdsByte operator | (TdsByte x, TdsByte y)
		{
			return new TdsByte ((byte) (x.Value | y.Value));
		}

		public static TdsByte operator / (TdsByte x, TdsByte y)
		{
			return new TdsByte ((byte) (x.Value / y.Value));
		}

		public static TdsBoolean operator == (TdsByte x, TdsByte y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}

		public static TdsByte operator ^ (TdsByte x, TdsByte y)
		{
			return new TdsByte ((byte) (x.Value ^ y.Value));
		}

		public static TdsBoolean operator > (TdsByte x, TdsByte y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value > y.Value);
		}

		public static TdsBoolean operator >= (TdsByte x, TdsByte y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value >= y.Value);
		}

		public static TdsBoolean operator != (TdsByte x, TdsByte y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (!(x.Value == y.Value));
		}

		public static TdsBoolean operator < (TdsByte x, TdsByte y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value < y.Value);
		}

		public static TdsBoolean operator <= (TdsByte x, TdsByte y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value <= y.Value);
		}

		public static TdsByte operator % (TdsByte x, TdsByte y)
		{
			return new TdsByte ((byte) (x.Value % y.Value));
		}

		public static TdsByte operator * (TdsByte x, TdsByte y)
		{
			return new TdsByte ((byte) (x.Value * y.Value));
		}

		public static TdsByte operator ~ (TdsByte x)
		{
			return new TdsByte ((byte) ~x.Value);
		}

		public static TdsByte operator - (TdsByte x, TdsByte y)
		{
			return new TdsByte ((byte) (x.Value - y.Value));
		}

		public static explicit operator TdsByte (TdsBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsByte (x.ByteValue);
		}

		public static explicit operator byte (TdsByte x)
		{
			return x.Value;
		}

		public static explicit operator TdsByte (TdsDecimal x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsByte ((byte)x.Value);
		}

		public static explicit operator TdsByte (TdsDouble x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsByte ((byte)x.Value);
		}

		public static explicit operator TdsByte (TdsInt16 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsByte ((byte)x.Value);
		}

		public static explicit operator TdsByte (TdsInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsByte ((byte)x.Value);
		}

		public static explicit operator TdsByte (TdsInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsByte ((byte)x.Value);
		}

		public static explicit operator TdsByte (TdsMoney x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsByte ((byte)x.Value);
		}

		public static explicit operator TdsByte (TdsSingle x)
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsByte ((byte)x.Value);
		}


		public static explicit operator TdsByte (TdsString x)
		{
			return TdsByte.Parse (x.Value);
		}

		public static implicit operator TdsByte (byte x)
		{
			return new TdsByte (x);
		}
		
		#endregion
	}
}
			
