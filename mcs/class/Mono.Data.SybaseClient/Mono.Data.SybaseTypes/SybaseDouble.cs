//
// Mono.Data.SybaseTypes.SybaseDouble
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Based on System.Data.SqlTypes.SqlDouble
//
// (C) Ximian, Inc. 2002-2003
// (C) Copyright Tim Coleman, 2002-2003
//

using Mono.Data.SybaseClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.SybaseTypes {
	public struct SybaseDouble : INullable, IComparable
	{
		#region Fields
		double value;

		private bool notNull;

		public static readonly SybaseDouble MaxValue = new SybaseDouble (1.7976931348623157E+308);
		public static readonly SybaseDouble MinValue = new SybaseDouble (-1.7976931348623157E+308);
		public static readonly SybaseDouble Null;
		public static readonly SybaseDouble Zero = new SybaseDouble (0);

		#endregion

		#region Constructors

		public SybaseDouble (double value) 
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
					throw new SybaseNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SybaseDouble Add (SybaseDouble x, SybaseDouble y)
		{
			return (x + y);
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseDouble))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseDouble"));
			else if (((SybaseDouble)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseDouble)value).Value);
		}

		public static SybaseDouble Divide (SybaseDouble x, SybaseDouble y)
		{
			return (x / y);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseDouble))
				return false;
			else
				return (bool) (this == (SybaseDouble)value);
		}

		public static SybaseBoolean Equals (SybaseDouble x, SybaseDouble y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			long LongValue = (long)value;
			return (int)(LongValue ^ (LongValue >> 32));
			
		}

		public static SybaseBoolean GreaterThan (SybaseDouble x, SybaseDouble y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseDouble x, SybaseDouble y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseDouble x, SybaseDouble y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseDouble x, SybaseDouble y)
		{
			return (x <= y);
		}

		public static SybaseDouble Multiply (SybaseDouble x, SybaseDouble y)
		{
			return (x * y);
		}

		public static SybaseBoolean NotEquals (SybaseDouble x, SybaseDouble y)
		{
			return (x != y);
		}

		public static SybaseDouble Parse (string s)
		{
			return new SybaseDouble (Double.Parse (s));
		}

		public static SybaseDouble Subtract (SybaseDouble x, SybaseDouble y)
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
				return String.Empty;
			else
				return value.ToString ();
		}

		public static SybaseDouble operator + (SybaseDouble x, SybaseDouble y)
		{
			return new SybaseDouble (x.Value + y.Value);
		}

		public static SybaseDouble operator / (SybaseDouble x, SybaseDouble y)
		{
			return new SybaseDouble (x.Value / y.Value);
		}

		public static SybaseBoolean operator == (SybaseDouble x, SybaseDouble y)
		{
			if (x.IsNull || y.IsNull) 	
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}

		public static SybaseBoolean operator > (SybaseDouble x, SybaseDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value > y.Value);
		}

		public static SybaseBoolean operator >= (SybaseDouble x, SybaseDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value >= y.Value);
		}

		public static SybaseBoolean operator != (SybaseDouble x, SybaseDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (!(x.Value == y.Value));
		}

		public static SybaseBoolean operator < (SybaseDouble x, SybaseDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value < y.Value);
		}

		public static SybaseBoolean operator <= (SybaseDouble x, SybaseDouble y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value <= y.Value);
		}

		public static SybaseDouble operator * (SybaseDouble x, SybaseDouble y)
		{
			return new SybaseDouble (x.Value * y.Value);
		}

		public static SybaseDouble operator - (SybaseDouble x, SybaseDouble y)
		{
			return new SybaseDouble (x.Value - y.Value);
		}

		public static SybaseDouble operator - (SybaseDouble n)
		{
			return new SybaseDouble (-(n.Value));
		}

		public static explicit operator SybaseDouble (SybaseBoolean x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDouble ((double)x.ByteValue);
		}

		public static explicit operator double (SybaseDouble x)
		{
			return x.Value;
		}

		public static explicit operator SybaseDouble (SybaseString x)
		{
			return SybaseDouble.Parse (x.Value);
		}

		public static implicit operator SybaseDouble (double x)
		{
			return new SybaseDouble (x);
		}

		public static implicit operator SybaseDouble (SybaseByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDouble ((double)x.Value);
		}

		public static implicit operator SybaseDouble (SybaseDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDouble ((double)x.Value);
		}

		public static implicit operator SybaseDouble (SybaseInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDouble ((double)x.Value);
		}

		public static implicit operator SybaseDouble (SybaseInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDouble ((double)x.Value);
		}

		public static implicit operator SybaseDouble (SybaseInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDouble ((double)x.Value);
		}

		public static implicit operator SybaseDouble (SybaseMoney x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDouble ((double)x.Value);
		}

		public static implicit operator SybaseDouble (SybaseSingle x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SybaseDouble ((double)x.Value);
		}

		#endregion
	}
}
			
