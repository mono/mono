//
// Mono.Data.TdsTypes.TdsMoney
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
	public struct TdsMoney : INullable, IComparable
	{
		#region Fields

		decimal value;
		
		private bool notNull;

		public static readonly TdsMoney MaxValue = new TdsMoney (922337203685477.5807);
		public static readonly TdsMoney MinValue = new TdsMoney (-922337203685477.5808);
		public static readonly TdsMoney Null;
		public static readonly TdsMoney Zero = new TdsMoney (0);

		#endregion

		#region Constructors

		public TdsMoney (decimal value) 
		{
			this.value = value;
			notNull = true;
		}

		public TdsMoney (double value) 
		{
			this.value = (decimal)value;
			notNull = true;
		}

		public TdsMoney (int value) 
		{
			this.value = (decimal)value;
			notNull = true;
		}

		public TdsMoney (long value) 
		{
			this.value = (decimal)value;
			notNull = true;
		}

		#endregion

		#region Properties

		[MonoTODO]
		public bool IsNull { 
			get { return !notNull; }
		}

		public decimal Value { 
			get { 
				if (this.IsNull) 
					throw new TdsNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static TdsMoney Add (TdsMoney x, TdsMoney y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is TdsMoney))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsMoney"));
			else if (((TdsMoney)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsMoney)value).Value);
		}

		public static TdsMoney Divide (TdsMoney x, TdsMoney y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsMoney))
				return false;
			else
				return (bool) (this == (TdsMoney)value);
		}

		public static TdsBoolean Equals (TdsMoney x, TdsMoney y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static TdsBoolean GreaterThan (TdsMoney x, TdsMoney y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsMoney x, TdsMoney y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsMoney x, TdsMoney y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsMoney x, TdsMoney y)
		{
			return (x <= y);
		}

		public static TdsMoney Multiply (TdsMoney x, TdsMoney y)
		{
			return (x * y);
		}

		public static TdsBoolean NotEquals (TdsMoney x, TdsMoney y)
		{
			return (x != y);
		}

		public static TdsMoney Parse (string s)
		{
			decimal d = Decimal.Parse (s);

			if (d > TdsMoney.MaxValue.Value || d < TdsMoney.MinValue.Value) 
				throw new OverflowException ("");
			
			return new TdsMoney (d);
		}

		public static TdsMoney Subtract (TdsMoney x, TdsMoney y)
		{
			return (x - y);
		}

		public decimal ToDecimal ()
		{
			return value;
		}

		public double ToDouble ()
		{
			return (double)value;
		}

		public int ToInt32 ()
		{
			return (int)value;
		}

		public long ToInt64 ()
		{
			return (long)value;
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

		public static TdsMoney operator + (TdsMoney x, TdsMoney y)
		{
			return new TdsMoney (x.Value + y.Value);
		}

		public static TdsMoney operator / (TdsMoney x, TdsMoney y)
		{
			return new TdsMoney (x.Value / y.Value);
		}

		public static TdsBoolean operator == (TdsMoney x, TdsMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}

		public static TdsBoolean operator > (TdsMoney x, TdsMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value > y.Value);
		}

		public static TdsBoolean operator >= (TdsMoney x, TdsMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value >= y.Value);
		}

		public static TdsBoolean operator != (TdsMoney x, TdsMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (!(x.Value == y.Value));
		}

		public static TdsBoolean operator < (TdsMoney x, TdsMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value < y.Value);
		}

		public static TdsBoolean operator <= (TdsMoney x, TdsMoney y)
		{
			if (x.IsNull || y.IsNull) return TdsBoolean.Null;
			return new TdsBoolean (x.Value <= y.Value);
		}

		public static TdsMoney operator * (TdsMoney x, TdsMoney y)
		{
			return new TdsMoney (x.Value * y.Value);
		}

		public static TdsMoney operator - (TdsMoney x, TdsMoney y)
		{
			return new TdsMoney (x.Value - y.Value);
		}

		public static TdsMoney operator - (TdsMoney n)
		{
			return new TdsMoney (-(n.Value));
		}

		public static explicit operator TdsMoney (TdsBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsMoney ((decimal)x.ByteValue);
		}

		public static explicit operator TdsMoney (TdsDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsMoney (x.Value);
		}

		public static explicit operator TdsMoney (TdsDouble x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsMoney ((decimal)x.Value);
		}

		public static explicit operator decimal (TdsMoney x)
		{
			return x.Value;
		}

		public static explicit operator TdsMoney (TdsSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsMoney ((decimal)x.Value);
		}

		public static explicit operator TdsMoney (TdsString x)
		{
			return TdsMoney.Parse (x.Value);
		}

		public static implicit operator TdsMoney (decimal x)
		{
			return new TdsMoney (x);
		}

		public static implicit operator TdsMoney (TdsByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsMoney ((decimal)x.Value);
		}

		public static implicit operator TdsMoney (TdsInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsMoney ((decimal)x.Value);
		}

		public static implicit operator TdsMoney (TdsInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsMoney ((decimal)x.Value);
		}

		public static implicit operator TdsMoney (TdsInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsMoney ((decimal)x.Value);
		}

		#endregion
	}
}
			
