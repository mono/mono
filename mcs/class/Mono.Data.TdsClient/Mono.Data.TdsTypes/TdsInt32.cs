//
// Mono.Data.TdsTypes.TdsInt32
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
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
	public struct TdsInt32 : INullable, IComparable 
	{
		#region Fields

		int value;
		private bool notNull;

		public static readonly TdsInt32 MaxValue = new TdsInt32 (2147483647);
		public static readonly TdsInt32 MinValue = new TdsInt32 (-2147483648);
		public static readonly TdsInt32 Null;
		public static readonly TdsInt32 Zero = new TdsInt32 (0);

		#endregion

		#region Constructors

		public TdsInt32(int value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public int Value {
			get { 
				if (this.IsNull) 
					throw new TdsNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static TdsInt32 Add (TdsInt32 x, TdsInt32 y) 
		{
			return (x + y);
		}

		public static TdsInt32 BitwiseAnd(TdsInt32 x, TdsInt32 y) 
		{
			return (x & y);
		}
		
		public static TdsInt32 BitwiseOr(TdsInt32 x, TdsInt32 y) 
		{
			return (x | y);
		}

		public int CompareTo(object value) 
		{
			if (value == null)
				return 1;
			else if (!(value is TdsInt32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsInt32"));
			else if (((TdsInt32)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsInt32)value).Value);
		}

		public static TdsInt32 Divide(TdsInt32 x, TdsInt32 y) 
		{
			return (x / y);
		}

		public override bool Equals(object value) 
		{
			if (!(value is TdsInt32))
				return false;
			else
				return (bool) (this == (TdsInt32)value);
		}

		public static TdsBoolean Equals(TdsInt32 x, TdsInt32 y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			return value;
		}

		public static TdsBoolean GreaterThan (TdsInt32 x, TdsInt32 y) 
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsInt32 x, TdsInt32 y) 
		{
			return (x >= y);
		}
                
		public static TdsBoolean LessThan(TdsInt32 x, TdsInt32 y) 
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual(TdsInt32 x, TdsInt32 y) 
		{
			return (x <= y);
		}

		public static TdsInt32 Mod(TdsInt32 x, TdsInt32 y) 
		{
			return (x % y);
		}

		public static TdsInt32 Multiply(TdsInt32 x, TdsInt32 y) 
		{
			return (x * y);
		}

		public static TdsBoolean NotEquals(TdsInt32 x, TdsInt32 y) 
		{
			return (x != y);
		}

		public static TdsInt32 OnesComplement(TdsInt32 x) 
		{
			return ~x;
		}

		public static TdsInt32 Parse(string s) 
		{
			return new TdsInt32 (Int32.Parse (s));
		}

		public static TdsInt32 Subtract(TdsInt32 x, TdsInt32 y) 
		{
			return (x - y);
		}

		public TdsBoolean ToTdsBoolean() 
		{
			return ((TdsBoolean)this);
		}

		public TdsByte ToTdsByte() 
		{
			return ((TdsByte)this);
		}

		public TdsDecimal ToTdsDecimal() 
		{
			return ((TdsDecimal)this);
		}

		public TdsDouble ToTdsDouble() 	
		{
			return ((TdsDouble)this);
		}

		public TdsInt16 ToTdsInt16() 
		{
			return ((TdsInt16)this);
		}

		public TdsInt64 ToTdsInt64() 
		{
			return ((TdsInt64)this);
		}

		public TdsMoney ToTdsMoney() 
		{
			return ((TdsMoney)this);
		}

		public TdsSingle ToTdsSingle() 
		{
			return ((TdsSingle)this);
		}

		public TdsString ToTdsString ()
		{
			return ((TdsString)this);
		}

		public override string ToString() 
		{
			if (this.IsNull)
				return "Null";
			else
				return value.ToString ();
		}

		public static TdsInt32 Xor(TdsInt32 x, TdsInt32 y) 
		{
			return (x ^ y);
		}

		#endregion

		#region Operators

		// Compute Addition
		public static TdsInt32 operator + (TdsInt32 x, TdsInt32 y) 
		{
			return new TdsInt32 (x.Value + y.Value);
		}

		// Bitwise AND
		public static TdsInt32 operator & (TdsInt32 x, TdsInt32 y) 
		{
			return new TdsInt32 (x.Value & y.Value);
		}

		// Bitwise OR
		public static TdsInt32 operator | (TdsInt32 x, TdsInt32 y) 
		{
			return new TdsInt32 (x.Value | y.Value);
		}

		// Compute Division
		public static TdsInt32 operator / (TdsInt32 x, TdsInt32 y) 
		{
			return new TdsInt32 (x.Value / y.Value);
		}

		// Compare Equality
		public static TdsBoolean operator == (TdsInt32 x, TdsInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}

		// Bitwise Exclusive-OR (XOR)
		public static TdsInt32 operator ^ (TdsInt32 x, TdsInt32 y) 
		{
			return new TdsInt32 (x.Value ^ y.Value);
		}

		// > Compare
		public static TdsBoolean operator >(TdsInt32 x, TdsInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value > y.Value);
		}

		// >= Compare
		public static TdsBoolean operator >= (TdsInt32 x, TdsInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value >= y.Value);
		}

		// != Inequality Compare
		public static TdsBoolean operator != (TdsInt32 x, TdsInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value != y.Value);
		}
		
		// < Compare
		public static TdsBoolean operator < (TdsInt32 x, TdsInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value < y.Value);
		}

		// <= Compare
		public static TdsBoolean operator <= (TdsInt32 x, TdsInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value <= y.Value);
		}

		// Compute Modulus
		public static TdsInt32 operator % (TdsInt32 x, TdsInt32 y) 
		{
			return new TdsInt32 (x.Value % y.Value);
		}

		// Compute Multiplication
		public static TdsInt32 operator * (TdsInt32 x, TdsInt32 y) 
		{
			return new TdsInt32 (x.Value * y.Value);
		}

		// Ones Complement
		public static TdsInt32 operator ~ (TdsInt32 x) 
		{
			return new TdsInt32 (~x.Value);
		}

		// Subtraction
		public static TdsInt32 operator - (TdsInt32 x, TdsInt32 y) 
		{
			return new TdsInt32 (x.Value - y.Value);
		}

		// Negates the Value
		public static TdsInt32 operator - (TdsInt32 x) 
		{
			return new TdsInt32 (-x.Value);
		}

		// Type Conversions
		public static explicit operator TdsInt32 (TdsBoolean x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new TdsInt32 ((int)x.ByteValue);
		}

		public static explicit operator TdsInt32 (TdsDecimal x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new TdsInt32 ((int)x.Value);
		}

		public static explicit operator TdsInt32 (TdsDouble x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new TdsInt32 ((int)x.Value);
		}

		public static explicit operator int (TdsInt32 x)
		{
			return x.Value;
		}

		public static explicit operator TdsInt32 (TdsInt64 x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new TdsInt32 ((int)x.Value);
		}

		public static explicit operator TdsInt32(TdsMoney x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new TdsInt32 ((int)x.Value);
		}

		public static explicit operator TdsInt32(TdsSingle x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new TdsInt32 ((int)x.Value);
		}

		public static explicit operator TdsInt32(TdsString x) 
		{
			return TdsInt32.Parse (x.Value);
		}

		public static implicit operator TdsInt32(int x) 
		{
			return new TdsInt32 (x);
		}

		public static implicit operator TdsInt32(TdsByte x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new TdsInt32 ((int)x.Value);
		}

		public static implicit operator TdsInt32(TdsInt16 x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new TdsInt32 ((int)x.Value);
		}

		#endregion
	}
}
