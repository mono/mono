//
// Mono.Data.SybaseTypes.SybaseInt16
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
	public struct SybaseInt16 : INullable, IComparable
	{
		#region Fields

		short value;
		private bool notNull;

		public static readonly SybaseInt16 MaxValue = new SybaseInt16 (32767);
		public static readonly SybaseInt16 MinValue = new SybaseInt16 (-32768);
		public static readonly SybaseInt16 Null;
		public static readonly SybaseInt16 Zero = new SybaseInt16 (0);

		#endregion

		#region Constructors

		public SybaseInt16 (short value) 
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
					throw new SybaseNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SybaseInt16 Add (SybaseInt16 x, SybaseInt16 y)
		{
			return (x + y);
		}

		public static SybaseInt16 BitwiseAnd (SybaseInt16 x, SybaseInt16 y)
		{
			return (x & y);
		}

		public static SybaseInt16 BitwiseOr (SybaseInt16 x, SybaseInt16 y)
		{
			return (x | y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseInt16))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseInt16"));
			else if (((SybaseInt16)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseInt16)value).Value);
		}

		public static SybaseInt16 Divide (SybaseInt16 x, SybaseInt16 y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseInt16))
				return false;
			else
				return (bool) (this == (SybaseInt16)value);
		}

		public static SybaseBoolean Equals (SybaseInt16 x, SybaseInt16 y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SybaseBoolean GreaterThan (SybaseInt16 x, SybaseInt16 y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseInt16 x, SybaseInt16 y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseInt16 x, SybaseInt16 y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseInt16 x, SybaseInt16 y)
		{
			return (x <= y);
		}

		public static SybaseInt16 Mod (SybaseInt16 x, SybaseInt16 y)
		{
			return (x % y);
		}

		public static SybaseInt16 Multiply (SybaseInt16 x, SybaseInt16 y)
		{
			return (x * y);
		}

		public static SybaseBoolean NotEquals (SybaseInt16 x, SybaseInt16 y)
		{
			return (x != y);
		}

		public static SybaseInt16 OnesComplement (SybaseInt16 x)
		{
			return ~x;
		}

		public static SybaseInt16 Parse (string s)
		{
			return new SybaseInt16 (Int16.Parse (s));
		}

		public static SybaseInt16 Subtract (SybaseInt16 x, SybaseInt16 y)
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

		public static SybaseInt16 Xor (SybaseInt16 x, SybaseInt16 y)
		{
			return (x ^ y);
		}

		public static SybaseInt16 operator + (SybaseInt16 x, SybaseInt16 y)
		{
			return new SybaseInt16 ((short) (x.Value + y.Value));
		}

		public static SybaseInt16 operator & (SybaseInt16 x, SybaseInt16 y)
		{
			return new SybaseInt16 ((short) (x.value & y.Value));
		}

		public static SybaseInt16 operator | (SybaseInt16 x, SybaseInt16 y)
		{
			return new SybaseInt16 ((short) ((byte) x.Value | (byte) y.Value));
		}

		public static SybaseInt16 operator / (SybaseInt16 x, SybaseInt16 y)
		{
			return new SybaseInt16 ((short) (x.Value / y.Value));
		}

		public static SybaseBoolean operator == (SybaseInt16 x, SybaseInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}

		public static SybaseInt16 operator ^ (SybaseInt16 x, SybaseInt16 y)
		{
			return new SybaseInt16 ((short) (x.Value ^ y.Value));
		}

		public static SybaseBoolean operator > (SybaseInt16 x, SybaseInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value > y.Value);
		}

		public static SybaseBoolean operator >= (SybaseInt16 x, SybaseInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value >= y.Value);
		}

		public static SybaseBoolean operator != (SybaseInt16 x, SybaseInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else 
				return new SybaseBoolean (!(x.Value == y.Value));
		}

		public static SybaseBoolean operator < (SybaseInt16 x, SybaseInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value < y.Value);
		}

		public static SybaseBoolean operator <= (SybaseInt16 x, SybaseInt16 y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value <= y.Value);
		}

		public static SybaseInt16 operator % (SybaseInt16 x, SybaseInt16 y)
		{
			return new SybaseInt16 ((short) (x.Value % y.Value));
		}

		public static SybaseInt16 operator * (SybaseInt16 x, SybaseInt16 y)
		{
			return new SybaseInt16 ((short) (x.Value * y.Value));
		}

		public static SybaseInt16 operator ~ (SybaseInt16 x)
		{
			return new SybaseInt16 ((short) (~x.Value));
		}

		public static SybaseInt16 operator - (SybaseInt16 x, SybaseInt16 y)
		{
			return new SybaseInt16 ((short) (x.Value - y.Value));
		}

		public static SybaseInt16 operator - (SybaseInt16 n)
		{
			return new SybaseInt16 ((short) (-n.Value));
		}

		public static explicit operator SybaseInt16 (SybaseBoolean x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseInt16 ((short)x.ByteValue);
		}

		public static explicit operator SybaseInt16 (SybaseDecimal x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseInt16 ((short)x.Value);
		}

		public static explicit operator SybaseInt16 (SybaseDouble x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseInt16 ((short)x.Value);
		}

		public static explicit operator short (SybaseInt16 x)
		{
			return x.Value; 
		}

		public static explicit operator SybaseInt16 (SybaseInt32 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseInt16 ((short)x.Value);
		}

		public static explicit operator SybaseInt16 (SybaseInt64 x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseInt16 ((short)x.Value);
		}

		public static explicit operator SybaseInt16 (SybaseMoney x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseInt16 ((short)x.Value);
		}

		public static explicit operator SybaseInt16 (SybaseSingle x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseInt16 ((short)x.Value);
		}

		public static explicit operator SybaseInt16 (SybaseString x)
		{
			return SybaseInt16.Parse (x.Value);
		}

		public static implicit operator SybaseInt16 (short x)
		{
			return new SybaseInt16 (x);
		}

		public static implicit operator SybaseInt16 (SybaseByte x)
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseInt16 ((short)x.Value);
		}

		#endregion
	}
}
			
