//
// System.Data.SybaseTypes.SybaseSingle
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
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
	public struct SybaseSingle : INullable, IComparable
	{
		#region Fields

		float value;

		private bool notNull;

		public static readonly SybaseSingle MaxValue = new SybaseSingle (3.40282346638528859e38);
		public static readonly SybaseSingle MinValue = new SybaseSingle (-3.40282346638528859e38);
		public static readonly SybaseSingle Null;
		public static readonly SybaseSingle Zero = new SybaseSingle (0);

		#endregion

		#region Constructors

		public SybaseSingle (double value) 
		{
			this.value = (float)value;
			notNull = true;
		}

		public SybaseSingle (float value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return !notNull; }
		}

		public float Value { 
			get { 
				if (this.IsNull) 
					throw new SybaseNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SybaseSingle Add (SybaseSingle x, SybaseSingle y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseSingle))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseSingle"));
			else if (((SybaseSingle)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseSingle)value).Value);
		}

		public static SybaseSingle Divide (SybaseSingle x, SybaseSingle y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseSingle))
				return false;
			else
				return (bool) (this == (SybaseSingle)value);
		}

		public static SybaseBoolean Equals (SybaseSingle x, SybaseSingle y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long) value;
			return (int)(LongValue ^ (LongValue >> 32));
		}

		public static SybaseBoolean GreaterThan (SybaseSingle x, SybaseSingle y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseSingle x, SybaseSingle y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseSingle x, SybaseSingle y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseSingle x, SybaseSingle y)
		{
			return (x <= y);
		}

		public static SybaseSingle Multiply (SybaseSingle x, SybaseSingle y)
		{
			return (x * y);
		}

		public static SybaseBoolean NotEquals (SybaseSingle x, SybaseSingle y)
		{
			return (x != y);
		}

		public static SybaseSingle Parse (string s)
		{
			return new SybaseSingle (Single.Parse (s));
		}

		public static SybaseSingle Subtract (SybaseSingle x, SybaseSingle y)
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

		public SybaseInt64 ToSybaseInt64 ()
		{
			return ((SybaseInt64)this);
		}

		public SybaseMoney ToSybaseMoney ()
		{
			return ((SybaseMoney)this);
		}


		public SybaseString ToSybaseString ()
		{
			return ((SybaseString)this);
		}

		public override string ToString ()
		{
			return value.ToString ();
		}

		public static SybaseSingle operator + (SybaseSingle x, SybaseSingle y)
		{
			return new SybaseSingle (x.Value + y.Value);
		}

		public static SybaseSingle operator / (SybaseSingle x, SybaseSingle y)
		{
			return new SybaseSingle (x.Value / y.Value);
		}

		public static SybaseBoolean operator == (SybaseSingle x, SybaseSingle y)
		{
			if (x.IsNull || y .IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (x.Value == y.Value);
		}

		public static SybaseBoolean operator > (SybaseSingle x, SybaseSingle y)
		{
			if (x.IsNull || y .IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (x.Value > y.Value);
		}

		public static SybaseBoolean operator >= (SybaseSingle x, SybaseSingle y)
		{
			if (x.IsNull || y .IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (x.Value >= y.Value);
		}

		public static SybaseBoolean operator != (SybaseSingle x, SybaseSingle y)
		{
			if (x.IsNull || y .IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (!(x.Value == y.Value));
		}

		public static SybaseBoolean operator < (SybaseSingle x, SybaseSingle y)
		{
			if (x.IsNull || y .IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (x.Value < y.Value);
		}

		public static SybaseBoolean operator <= (SybaseSingle x, SybaseSingle y)
		{
			if (x.IsNull || y .IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (x.Value <= y.Value);
		}

		public static SybaseSingle operator * (SybaseSingle x, SybaseSingle y)
		{
			return new SybaseSingle (x.Value * y.Value);
		}

		public static SybaseSingle operator - (SybaseSingle x, SybaseSingle y)
		{
			return new SybaseSingle (x.Value - y.Value);
		}

		public static SybaseSingle operator - (SybaseSingle n)
		{
			return new SybaseSingle (-(n.Value));
		}

		public static explicit operator SybaseSingle (SybaseBoolean x)
		{
			return new SybaseSingle((float)x.ByteValue);
		}

		public static explicit operator SybaseSingle (SybaseDouble x)
		{
			return new SybaseSingle((float)x.Value);
		}

		public static explicit operator float (SybaseSingle x)
		{
			return x.Value;
		}

		public static explicit operator SybaseSingle (SybaseString x)
		{
			return SybaseSingle.Parse (x.Value);
		}

		public static implicit operator SybaseSingle (float x)
		{
			return new SybaseSingle (x);
		}

		public static implicit operator SybaseSingle (SybaseByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseSingle((float)x.Value);
		}

		public static implicit operator SybaseSingle (SybaseDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseSingle((float)x.Value);
		}

		public static implicit operator SybaseSingle (SybaseInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseSingle((float)x.Value);
		}

		public static implicit operator SybaseSingle (SybaseInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseSingle((float)x.Value);
		}

		public static implicit operator SybaseSingle (SybaseInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseSingle((float)x.Value);
		}

		public static implicit operator SybaseSingle (SybaseMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseSingle((float)x.Value);
		}

		#endregion
	}
}
			
