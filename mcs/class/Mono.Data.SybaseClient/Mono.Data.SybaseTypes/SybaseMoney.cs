//
// Mono.Data.SybaseTypes.SybaseMoney
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Based on System.Data.SqlTypes.SqlMoney
//
// (C) Ximian, Inc. 2002-2003
// (C) Copyright Tim Coleman, 2002-2003
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
	public struct SybaseMoney : INullable, IComparable
	{
		#region Fields

		decimal value;
		
		private bool notNull;

		public static readonly SybaseMoney MaxValue = new SybaseMoney (922337203685477.5807);
		public static readonly SybaseMoney MinValue = new SybaseMoney (-922337203685477.5808);
		public static readonly SybaseMoney Null;
		public static readonly SybaseMoney Zero = new SybaseMoney (0);

		#endregion

		#region Constructors

		public SybaseMoney (decimal value) 
		{
			this.value = value;
			notNull = true;
		}

		public SybaseMoney (double value) 
		{
			this.value = (decimal)value;
			notNull = true;
		}

		public SybaseMoney (int value) 
		{
			this.value = (decimal)value;
			notNull = true;
		}

		public SybaseMoney (long value) 
		{
			this.value = (decimal)value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return !notNull; }
		}

		public decimal Value { 
			get { 
				if (this.IsNull) 
					throw new SybaseNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SybaseMoney Add (SybaseMoney x, SybaseMoney y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseMoney))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseMoney"));
			else if (((SybaseMoney)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseMoney)value).Value);
		}

		public static SybaseMoney Divide (SybaseMoney x, SybaseMoney y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseMoney))
				return false;
			else
				return (bool) (this == (SybaseMoney)value);
		}

		public static SybaseBoolean Equals (SybaseMoney x, SybaseMoney y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SybaseBoolean GreaterThan (SybaseMoney x, SybaseMoney y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseMoney x, SybaseMoney y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseMoney x, SybaseMoney y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseMoney x, SybaseMoney y)
		{
			return (x <= y);
		}

		public static SybaseMoney Multiply (SybaseMoney x, SybaseMoney y)
		{
			return (x * y);
		}

		public static SybaseBoolean NotEquals (SybaseMoney x, SybaseMoney y)
		{
			return (x != y);
		}

		public static SybaseMoney Parse (string s)
		{
			decimal d = Decimal.Parse (s);

			if (d > SybaseMoney.MaxValue.Value || d < SybaseMoney.MinValue.Value) 
				throw new OverflowException ("");
			
			return new SybaseMoney (d);
		}

		public static SybaseMoney Subtract (SybaseMoney x, SybaseMoney y)
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
				return String.Empty;
			else
				return value.ToString ();
		}

		public static SybaseMoney operator + (SybaseMoney x, SybaseMoney y)
		{
			return new SybaseMoney (x.Value + y.Value);
		}

		public static SybaseMoney operator / (SybaseMoney x, SybaseMoney y)
		{
			return new SybaseMoney (x.Value / y.Value);
		}

		public static SybaseBoolean operator == (SybaseMoney x, SybaseMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}

		public static SybaseBoolean operator > (SybaseMoney x, SybaseMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value > y.Value);
		}

		public static SybaseBoolean operator >= (SybaseMoney x, SybaseMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value >= y.Value);
		}

		public static SybaseBoolean operator != (SybaseMoney x, SybaseMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (!(x.Value == y.Value));
		}

		public static SybaseBoolean operator < (SybaseMoney x, SybaseMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value < y.Value);
		}

		public static SybaseBoolean operator <= (SybaseMoney x, SybaseMoney y)
		{
			if (x.IsNull || y.IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (x.Value <= y.Value);
		}

		public static SybaseMoney operator * (SybaseMoney x, SybaseMoney y)
		{
			return new SybaseMoney (x.Value * y.Value);
		}

		public static SybaseMoney operator - (SybaseMoney x, SybaseMoney y)
		{
			return new SybaseMoney (x.Value - y.Value);
		}

		public static SybaseMoney operator - (SybaseMoney n)
		{
			return new SybaseMoney (-(n.Value));
		}

		public static explicit operator SybaseMoney (SybaseBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseMoney ((decimal)x.ByteValue);
		}

		public static explicit operator SybaseMoney (SybaseDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseMoney (x.Value);
		}

		public static explicit operator SybaseMoney (SybaseDouble x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseMoney ((decimal)x.Value);
		}

		public static explicit operator decimal (SybaseMoney x)
		{
			return x.Value;
		}

		public static explicit operator SybaseMoney (SybaseSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseMoney ((decimal)x.Value);
		}

		public static explicit operator SybaseMoney (SybaseString x)
		{
			return SybaseMoney.Parse (x.Value);
		}

		public static implicit operator SybaseMoney (decimal x)
		{
			return new SybaseMoney (x);
		}

		public static implicit operator SybaseMoney (SybaseByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseMoney ((decimal)x.Value);
		}

		public static implicit operator SybaseMoney (SybaseInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseMoney ((decimal)x.Value);
		}

		public static implicit operator SybaseMoney (SybaseInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseMoney ((decimal)x.Value);
		}

		public static implicit operator SybaseMoney (SybaseInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseMoney ((decimal)x.Value);
		}

		#endregion
	}
}
			
