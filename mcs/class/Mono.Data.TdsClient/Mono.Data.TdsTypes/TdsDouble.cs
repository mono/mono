//
// Mono.Data.TdsTypes.TdsDouble
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
	public struct TdsDouble : INullable, IComparable
	{
		#region Fields
		double value;

		private bool notNull;

		public static readonly TdsDouble MaxValue = new TdsDouble (1.7976931348623157e308);
		public static readonly TdsDouble MinValue = new TdsDouble (-1.7976931348623157e308);
		public static readonly TdsDouble Null;
		public static readonly TdsDouble Zero = new TdsDouble (0);

		#endregion

		#region Constructors

		public TdsDouble (double value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return !notNull; }
		}

		public double Value { 
			get { 
				if (this.IsNull) 
					throw new TdsNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static TdsDouble Add (TdsDouble x, TdsDouble y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is TdsDouble))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsDouble"));
			else if (((TdsDouble)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsDouble)value).Value);
		}

		public static TdsDouble Divide (TdsDouble x, TdsDouble y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsDouble))
				return false;
			else
				return (bool) (this == (TdsDouble)value);
		}

		public static TdsBoolean Equals (TdsDouble x, TdsDouble y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long)value;
			return (int)(LongValue ^ (LongValue >> 32));
			
		}

		public static TdsBoolean GreaterThan (TdsDouble x, TdsDouble y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsDouble x, TdsDouble y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsDouble x, TdsDouble y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsDouble x, TdsDouble y)
		{
			return (x <= y);
		}

		public static TdsDouble Multiply (TdsDouble x, TdsDouble y)
		{
			return (x * y);
		}

		public static TdsBoolean NotEquals (TdsDouble x, TdsDouble y)
		{
			return (x != y);
		}

		public static TdsDouble Parse (string s)
		{
			return new TdsDouble (Double.Parse (s));
		}

		public static TdsDouble Subtract (TdsDouble x, TdsDouble y)
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
				return String.Empty;
			else
				return value.ToString ();
		}

		public static TdsDouble operator + (TdsDouble x, TdsDouble y)
		{
			return new TdsDouble (x.Value + y.Value);
		}

		public static TdsDouble operator / (TdsDouble x, TdsDouble y)
		{
			return new TdsDouble (x.Value / y.Value);
		}

		public static TdsBoolean operator == (TdsDouble x, TdsDouble y)
		{
			if (x.IsNull || y.IsNull) 	
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}

		public static TdsBoolean operator > (TdsDouble x, TdsDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value > y.Value);
		}

		public static TdsBoolean operator >= (TdsDouble x, TdsDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value >= y.Value);
		}

		public static TdsBoolean operator != (TdsDouble x, TdsDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (!(x.Value == y.Value));
		}

		public static TdsBoolean operator < (TdsDouble x, TdsDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value < y.Value);
		}

		public static TdsBoolean operator <= (TdsDouble x, TdsDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value <= y.Value);
		}

		public static TdsDouble operator * (TdsDouble x, TdsDouble y)
		{
			return new TdsDouble (x.Value * y.Value);
		}

		public static TdsDouble operator - (TdsDouble x, TdsDouble y)
		{
			return new TdsDouble (x.Value - y.Value);
		}

		public static TdsDouble operator - (TdsDouble n)
		{
			return new TdsDouble (-(n.Value));
		}

		public static explicit operator TdsDouble (TdsBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDouble ((double)x.ByteValue);
		}

		public static explicit operator double (TdsDouble x)
		{
			return x.Value;
		}

		public static explicit operator TdsDouble (TdsString x)
		{
			return TdsDouble.Parse (x.Value);
		}

		public static implicit operator TdsDouble (double x)
		{
			return new TdsDouble (x);
		}

		public static implicit operator TdsDouble (TdsByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDouble ((double)x.Value);
		}

		public static implicit operator TdsDouble (TdsDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDouble ((double)x.Value);
		}

		public static implicit operator TdsDouble (TdsInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDouble ((double)x.Value);
		}

		public static implicit operator TdsDouble (TdsInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDouble ((double)x.Value);
		}

		public static implicit operator TdsDouble (TdsInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDouble ((double)x.Value);
		}

		public static implicit operator TdsDouble (TdsMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDouble ((double)x.Value);
		}

		public static implicit operator TdsDouble (TdsSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new TdsDouble ((double)x.Value);
		}

		#endregion
	}
}
			
