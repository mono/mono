//
// Mono.Data.SybaseTypes.SybaseByte
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Based on System.Data.SqlTypes.SqlByte
//
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

using Mono.Data.SybaseClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.SybaseTypes {
	public struct SybaseByte : INullable, IComparable
	{
		#region Fields

		byte value;
		private bool notNull;

		public static readonly SybaseByte MaxValue = new SybaseByte (0xff);
		public static readonly SybaseByte MinValue = new SybaseByte (0);
		public static readonly SybaseByte Null;
		public static readonly SybaseByte Zero = new SybaseByte (0);

		#endregion

		#region Constructors

		public SybaseByte (byte value) 
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
					throw new SybaseNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SybaseByte Add (SybaseByte x, SybaseByte y)
		{
			return (x + y);
		}

		public static SybaseByte BitwiseAnd (SybaseByte x, SybaseByte y)
		{
			return (x & y);
		}

		public static SybaseByte BitwiseOr (SybaseByte x, SybaseByte y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseByte"));
			else if (((SybaseByte)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseByte)value).Value);
		}

		public static SybaseByte Divide (SybaseByte x, SybaseByte y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseByte))
				return false;
			else
				return (bool) (this == (SybaseByte)value);
		}

		public static SybaseBoolean Equals (SybaseByte x, SybaseByte y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SybaseBoolean GreaterThan (SybaseByte x, SybaseByte y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseByte x, SybaseByte y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseByte x, SybaseByte y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseByte x, SybaseByte y)
		{
			return (x <= y);
		}

		public static SybaseByte Mod (SybaseByte x, SybaseByte y)
		{
			return (x % y);
		}

		public static SybaseByte Multiply (SybaseByte x, SybaseByte y)
		{
			return (x * y);
		}

		public static SybaseBoolean NotEquals (SybaseByte x, SybaseByte y)
		{
			return (x != y);
		}

		public static SybaseByte OnesComplement (SybaseByte x)
		{
			return ~x;
		}

		public static SybaseByte Parse (string s)
		{
			return new SybaseByte (Byte.Parse (s));
		}

		public static SybaseByte Subtract (SybaseByte x, SybaseByte y)
		{
			return (x - y);
		}

		public SybaseBoolean ToSybaseBoolean ()
		{
			return ((SybaseBoolean)this);
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
				return "Null";
			else
				return value.ToString ();
		}

		public static SybaseByte Xor (SybaseByte x, SybaseByte y)
		{
			return (x ^ y);
		}

		public static SybaseByte operator + (SybaseByte x, SybaseByte y)
		{
			return new SybaseByte ((byte) (x.Value + y.Value));
		}

		public static SybaseByte operator & (SybaseByte x, SybaseByte y)
		{
			return new SybaseByte ((byte) (x.Value & y.Value));
		}

		public static SybaseByte operator | (SybaseByte x, SybaseByte y)
		{
			return new SybaseByte ((byte) (x.Value | y.Value));
		}

		public static SybaseByte operator / (SybaseByte x, SybaseByte y)
		{
			return new SybaseByte ((byte) (x.Value / y.Value));
		}

		public static SybaseBoolean operator == (SybaseByte x, SybaseByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}

		public static SybaseByte operator ^ (SybaseByte x, SybaseByte y)
		{
			return new SybaseByte ((byte) (x.Value ^ y.Value));
		}

		public static SybaseBoolean operator > (SybaseByte x, SybaseByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value > y.Value);
		}

		public static SybaseBoolean operator >= (SybaseByte x, SybaseByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value >= y.Value);
		}

		public static SybaseBoolean operator != (SybaseByte x, SybaseByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (!(x.Value == y.Value));
		}

		public static SybaseBoolean operator < (SybaseByte x, SybaseByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value < y.Value);
		}

		public static SybaseBoolean operator <= (SybaseByte x, SybaseByte y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value <= y.Value);
		}

		public static SybaseByte operator % (SybaseByte x, SybaseByte y)
		{
			return new SybaseByte ((byte) (x.Value % y.Value));
		}

		public static SybaseByte operator * (SybaseByte x, SybaseByte y)
		{
			return new SybaseByte ((byte) (x.Value * y.Value));
		}

		public static SybaseByte operator ~ (SybaseByte x)
		{
			return new SybaseByte ((byte) ~x.Value);
		}

		public static SybaseByte operator - (SybaseByte x, SybaseByte y)
		{
			return new SybaseByte ((byte) (x.Value - y.Value));
		}

		public static explicit operator SybaseByte (SybaseBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseByte (x.ByteValue);
		}

		public static explicit operator byte (SybaseByte x)
		{
			return x.Value;
		}

		public static explicit operator SybaseByte (SybaseDecimal x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseByte ((byte)x.Value);
		}

		public static explicit operator SybaseByte (SybaseDouble x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseByte ((byte)x.Value);
		}

		public static explicit operator SybaseByte (SybaseInt16 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseByte ((byte)x.Value);
		}

		public static explicit operator SybaseByte (SybaseInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseByte ((byte)x.Value);
		}

		public static explicit operator SybaseByte (SybaseInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseByte ((byte)x.Value);
		}

		public static explicit operator SybaseByte (SybaseMoney x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseByte ((byte)x.Value);
		}

		public static explicit operator SybaseByte (SybaseSingle x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseByte ((byte)x.Value);
		}


		public static explicit operator SybaseByte (SybaseString x)
		{
			return SybaseByte.Parse (x.Value);
		}

		public static implicit operator SybaseByte (byte x)
		{
			return new SybaseByte (x);
		}
		
		#endregion
	}
}
			
