//
// Mono.Data.SybaseTypes.SybaseInt64
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

using Mono.Data.SybaseClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.SybaseTypes {
	public struct SybaseInt64 : INullable, IComparable
	{
		#region Fields

		long value;

		private bool notNull;
		
		public static readonly SybaseInt64 MaxValue = new SybaseInt64 (9223372036854775807);
		public static readonly SybaseInt64 MinValue = new SybaseInt64 (-9223372036854775808);

		public static readonly SybaseInt64 Null;
		public static readonly SybaseInt64 Zero = new SybaseInt64 (0);

		#endregion

		#region Constructors

		public SybaseInt64 (long value) 
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
					throw new SybaseNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SybaseInt64 Add (SybaseInt64 x, SybaseInt64 y)
		{
			return (x + y);
		}

		public static SybaseInt64 BitwiseAnd (SybaseInt64 x, SybaseInt64 y)
		{
			return (x & y);
		}

		public static SybaseInt64 BitwiseOr (SybaseInt64 x, SybaseInt64 y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseInt64))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseInt64"));
			else if (((SybaseInt64)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseInt64)value).Value);
		}

		public static SybaseInt64 Divide (SybaseInt64 x, SybaseInt64 y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseInt64))
				return false;
			else
				return (bool) (this == (SybaseInt64)value);
		}

		public static SybaseBoolean Equals (SybaseInt64 x, SybaseInt64 y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)(value & 0xffffffff) ^ (int)(value >> 32);
		}

		public static SybaseBoolean GreaterThan (SybaseInt64 x, SybaseInt64 y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseInt64 x, SybaseInt64 y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseInt64 x, SybaseInt64 y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseInt64 x, SybaseInt64 y)
		{
			return (x <= y);
		}

		public static SybaseInt64 Mod (SybaseInt64 x, SybaseInt64 y)
		{
			return (x % y);
		}

		public static SybaseInt64 Multiply (SybaseInt64 x, SybaseInt64 y)
		{
			return (x * y);
		}

		public static SybaseBoolean NotEquals (SybaseInt64 x, SybaseInt64 y)
		{
			return (x != y);
		}

		public static SybaseInt64 OnesComplement (SybaseInt64 x)
		{
			return ~x;
		}


		public static SybaseInt64 Parse (string s)
		{
			return new SybaseInt64 (Int64.Parse (s));
		}

		public static SybaseInt64 Subtract (SybaseInt64 x, SybaseInt64 y)
		{
			return (x - y);
		}

		public SybaseBoolean ToSybaseBoolean ()
		{
			return ((SybaseBoolean)this);
		}
		
		public SybaseByte ToSybaseByte ()
		{
			return ((SybaseByte)this);
		}

		public SybaseDecimal ToSybaseDecimal ()
		{
			return ((SybaseDecimal)this);
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
				return "Null";

			return value.ToString ();
		}

		public static SybaseInt64 Xor (SybaseInt64 x, SybaseInt64 y)
		{
			return (x ^ y);
		}

		public static SybaseInt64 operator + (SybaseInt64 x, SybaseInt64 y)
		{
			return new SybaseInt64 (x.Value + y.Value);
		}

		public static SybaseInt64 operator & (SybaseInt64 x, SybaseInt64 y)
		{
			return new SybaseInt64 (x.value & y.Value);
		}

		public static SybaseInt64 operator | (SybaseInt64 x, SybaseInt64 y)
		{
			return new SybaseInt64 (x.value | y.Value);
		}

		public static SybaseInt64 operator / (SybaseInt64 x, SybaseInt64 y)
		{
			return new SybaseInt64 (x.Value / y.Value);
		}

		public static SybaseBoolean operator == (SybaseInt64 x, SybaseInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}

		public static SybaseInt64 operator ^ (SybaseInt64 x, SybaseInt64 y)
		{
			return new SybaseInt64 (x.Value ^ y.Value);
		}

		public static SybaseBoolean operator > (SybaseInt64 x, SybaseInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value > y.Value);
		}

		public static SybaseBoolean operator >= (SybaseInt64 x, SybaseInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value >= y.Value);
		}

		public static SybaseBoolean operator != (SybaseInt64 x, SybaseInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (!(x.Value == y.Value));
		}

		public static SybaseBoolean operator < (SybaseInt64 x, SybaseInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value < y.Value);
		}

		public static SybaseBoolean operator <= (SybaseInt64 x, SybaseInt64 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value <= y.Value);
		}

		public static SybaseInt64 operator % (SybaseInt64 x, SybaseInt64 y)
		{
			return new SybaseInt64(x.Value % y.Value);
		}

		public static SybaseInt64 operator * (SybaseInt64 x, SybaseInt64 y)
		{
			return new SybaseInt64 (x.Value * y.Value);
		}

		public static SybaseInt64 operator ~ (SybaseInt64 x)
		{
			return new SybaseInt64 (~(x.Value));
		}

		public static SybaseInt64 operator - (SybaseInt64 x, SybaseInt64 y)
		{
			return new SybaseInt64 (x.Value - y.Value);
		}

		public static SybaseInt64 operator - (SybaseInt64 n)
		{
			return new SybaseInt64 (-(n.Value));
		}

		public static explicit operator SybaseInt64 (SybaseBoolean x)
		{
			if (x.IsNull) 
				return SybaseInt64.Null;
			else
				return new SybaseInt64 ((long)x.ByteValue);
		}

		public static explicit operator SybaseInt64 (SybaseDecimal x)
		{
			if (x.IsNull) 
				return SybaseInt64.Null;
			else
				return new SybaseInt64 ((long)x.Value);
		}

		public static explicit operator SybaseInt64 (SybaseDouble x)
		{
			if (x.IsNull) 
				return SybaseInt64.Null;
			else
				return new SybaseInt64 ((long)x.Value);
		}

		public static explicit operator long (SybaseInt64 x)
		{
			return x.Value;
		}

		public static explicit operator SybaseInt64 (SybaseMoney x)
		{
			if (x.IsNull) 
				return SybaseInt64.Null;
			else
				return new SybaseInt64 ((long)x.Value);
		}

		public static explicit operator SybaseInt64 (SybaseSingle x)
		{
			if (x.IsNull) 
				return SybaseInt64.Null;
			else
				return new SybaseInt64 ((long)x.Value);
		}

		public static explicit operator SybaseInt64 (SybaseString x)
		{
			return SybaseInt64.Parse (x.Value);
		}

		public static implicit operator SybaseInt64 (long x)
		{
			return new SybaseInt64 (x);
		}

		public static implicit operator SybaseInt64 (SybaseByte x)
		{
			if (x.IsNull) 
				return SybaseInt64.Null;
			else
				return new SybaseInt64 ((long)x.Value);
		}

		public static implicit operator SybaseInt64 (SybaseInt16 x)
		{
			if (x.IsNull) 
				return SybaseInt64.Null;
			else
				return new SybaseInt64 ((long)x.Value);
		}

		public static implicit operator SybaseInt64 (SybaseInt32 x)
		{
			if (x.IsNull) 
				return SybaseInt64.Null;
			else
				return new SybaseInt64 ((long)x.Value);
		}

		#endregion
	}
}
			
