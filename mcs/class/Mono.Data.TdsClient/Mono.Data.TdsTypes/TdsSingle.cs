//
// System.Data.TdsTypes.TdsSingle
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

using Mono.Data.TdsClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.TdsTypes {
	public struct TdsSingle : INullable, IComparable
	{
		#region Fields

		float value;

		private bool notNull;

		public static readonly TdsSingle MaxValue = new TdsSingle (3.40282346638528859e38);
		public static readonly TdsSingle MinValue = new TdsSingle (-3.40282346638528859e38);
		public static readonly TdsSingle Null;
		public static readonly TdsSingle Zero = new TdsSingle (0);

		#endregion

		#region Constructors

		public TdsSingle (double value) 
		{
			this.value = (float)value;
			notNull = true;
		}

		public TdsSingle (float value) 
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
					throw new TdsNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static TdsSingle Add (TdsSingle x, TdsSingle y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is TdsSingle))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsSingle"));
			else if (((TdsSingle)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsSingle)value).Value);
		}

		public static TdsSingle Divide (TdsSingle x, TdsSingle y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsSingle))
				return false;
			else
				return (bool) (this == (TdsSingle)value);
		}

		public static TdsBoolean Equals (TdsSingle x, TdsSingle y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long) value;
			return (int)(LongValue ^ (LongValue >> 32));
		}

		public static TdsBoolean GreaterThan (TdsSingle x, TdsSingle y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsSingle x, TdsSingle y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsSingle x, TdsSingle y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsSingle x, TdsSingle y)
		{
			return (x <= y);
		}

		public static TdsSingle Multiply (TdsSingle x, TdsSingle y)
		{
			return (x * y);
		}

		public static TdsBoolean NotEquals (TdsSingle x, TdsSingle y)
		{
			return (x != y);
		}

		public static TdsSingle Parse (string s)
		{
			return new TdsSingle (Single.Parse (s));
		}

		public static TdsSingle Subtract (TdsSingle x, TdsSingle y)
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

		public TdsInt64 ToTdsInt64 ()
		{
			return ((TdsInt64)this);
		}

		public TdsMoney ToTdsMoney ()
		{
			return ((TdsMoney)this);
		}


		public TdsString ToTdsString ()
		{
			return ((TdsString)this);
		}

		public override string ToString ()
		{
			return value.ToString ();
		}

		public static TdsSingle operator + (TdsSingle x, TdsSingle y)
		{
			return new TdsSingle (x.Value + y.Value);
		}

		public static TdsSingle operator / (TdsSingle x, TdsSingle y)
		{
			return new TdsSingle (x.Value / y.Value);
		}

		public static TdsBoolean operator == (TdsSingle x, TdsSingle y)
		{
			if (x.IsNull || y .IsNull) return TdsBoolean.Null;
			return new TdsBoolean (x.Value == y.Value);
		}

		public static TdsBoolean operator > (TdsSingle x, TdsSingle y)
		{
			if (x.IsNull || y .IsNull) return TdsBoolean.Null;
			return new TdsBoolean (x.Value > y.Value);
		}

		public static TdsBoolean operator >= (TdsSingle x, TdsSingle y)
		{
			if (x.IsNull || y .IsNull) return TdsBoolean.Null;
			return new TdsBoolean (x.Value >= y.Value);
		}

		public static TdsBoolean operator != (TdsSingle x, TdsSingle y)
		{
			if (x.IsNull || y .IsNull) return TdsBoolean.Null;
			return new TdsBoolean (!(x.Value == y.Value));
		}

		public static TdsBoolean operator < (TdsSingle x, TdsSingle y)
		{
			if (x.IsNull || y .IsNull) return TdsBoolean.Null;
			return new TdsBoolean (x.Value < y.Value);
		}

		public static TdsBoolean operator <= (TdsSingle x, TdsSingle y)
		{
			if (x.IsNull || y .IsNull) return TdsBoolean.Null;
			return new TdsBoolean (x.Value <= y.Value);
		}

		public static TdsSingle operator * (TdsSingle x, TdsSingle y)
		{
			return new TdsSingle (x.Value * y.Value);
		}

		public static TdsSingle operator - (TdsSingle x, TdsSingle y)
		{
			return new TdsSingle (x.Value - y.Value);
		}

		public static TdsSingle operator - (TdsSingle n)
		{
			return new TdsSingle (-(n.Value));
		}

		public static explicit operator TdsSingle (TdsBoolean x)
		{
			return new TdsSingle((float)x.ByteValue);
		}

		public static explicit operator TdsSingle (TdsDouble x)
		{
			return new TdsSingle((float)x.Value);
		}

		public static explicit operator float (TdsSingle x)
		{
			return x.Value;
		}

		public static explicit operator TdsSingle (TdsString x)
		{
			return TdsSingle.Parse (x.Value);
		}

		public static implicit operator TdsSingle (float x)
		{
			return new TdsSingle (x);
		}

		public static implicit operator TdsSingle (TdsByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsSingle((float)x.Value);
		}

		public static implicit operator TdsSingle (TdsDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsSingle((float)x.Value);
		}

		public static implicit operator TdsSingle (TdsInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsSingle((float)x.Value);
		}

		public static implicit operator TdsSingle (TdsInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsSingle((float)x.Value);
		}

		public static implicit operator TdsSingle (TdsInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsSingle((float)x.Value);
		}

		public static implicit operator TdsSingle (TdsMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsSingle((float)x.Value);
		}

		#endregion
	}
}
			
