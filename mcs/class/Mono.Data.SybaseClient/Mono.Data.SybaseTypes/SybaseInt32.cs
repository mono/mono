//
// Mono.Data.SybaseTypes.SybaseInt32
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

using Mono.Data.SybaseClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.SybaseTypes {
	public struct SybaseInt32 : INullable, IComparable 
	{
		#region Fields

		int value;
		private bool notNull;

		public static readonly SybaseInt32 MaxValue = new SybaseInt32 (2147483647);
		public static readonly SybaseInt32 MinValue = new SybaseInt32 (-2147483648);
		public static readonly SybaseInt32 Null;
		public static readonly SybaseInt32 Zero = new SybaseInt32 (0);

		#endregion

		#region Constructors

		public SybaseInt32(int value) 
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
					throw new SybaseNullValueException ();
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public static SybaseInt32 Add (SybaseInt32 x, SybaseInt32 y) 
		{
			return (x + y);
		}

		public static SybaseInt32 BitwiseAnd(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x & y);
		}
		
		public static SybaseInt32 BitwiseOr(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x | y);
		}

		public int CompareTo(object value) 
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseInt32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseInt32"));
			else if (((SybaseInt32)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseInt32)value).Value);
		}

		public static SybaseInt32 Divide(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x / y);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SybaseInt32))
				return false;
			else
				return (bool) (this == (SybaseInt32)value);
		}

		public static SybaseBoolean Equals(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			return value;
		}

		public static SybaseBoolean GreaterThan (SybaseInt32 x, SybaseInt32 y) 
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseInt32 x, SybaseInt32 y) 
		{
			return (x >= y);
		}
                
		public static SybaseBoolean LessThan(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x <= y);
		}

		public static SybaseInt32 Mod(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x % y);
		}

		public static SybaseInt32 Multiply(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x * y);
		}

		public static SybaseBoolean NotEquals(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x != y);
		}

		public static SybaseInt32 OnesComplement(SybaseInt32 x) 
		{
			return ~x;
		}

		public static SybaseInt32 Parse(string s) 
		{
			return new SybaseInt32 (Int32.Parse (s));
		}

		public static SybaseInt32 Subtract(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x - y);
		}

		public SybaseBoolean ToSybaseBoolean() 
		{
			return ((SybaseBoolean)this);
		}

		public SybaseByte ToSybaseByte() 
		{
			return ((SybaseByte)this);
		}

		public SybaseDecimal ToSybaseDecimal() 
		{
			return ((SybaseDecimal)this);
		}

		public SybaseDouble ToSybaseDouble() 	
		{
			return ((SybaseDouble)this);
		}

		public SybaseInt16 ToSybaseInt16() 
		{
			return ((SybaseInt16)this);
		}

		public SybaseInt64 ToSybaseInt64() 
		{
			return ((SybaseInt64)this);
		}

		public SybaseMoney ToSybaseMoney() 
		{
			return ((SybaseMoney)this);
		}

		public SybaseSingle ToSybaseSingle() 
		{
			return ((SybaseSingle)this);
		}

		public SybaseString ToSybaseString ()
		{
			return ((SybaseString)this);
		}

		public override string ToString() 
		{
			if (this.IsNull)
				return "Null";
			else
				return value.ToString ();
		}

		public static SybaseInt32 Xor(SybaseInt32 x, SybaseInt32 y) 
		{
			return (x ^ y);
		}

		#endregion

		#region Operators

		// Compute Addition
		public static SybaseInt32 operator + (SybaseInt32 x, SybaseInt32 y) 
		{
			return new SybaseInt32 (x.Value + y.Value);
		}

		// Bitwise AND
		public static SybaseInt32 operator & (SybaseInt32 x, SybaseInt32 y) 
		{
			return new SybaseInt32 (x.Value & y.Value);
		}

		// Bitwise OR
		public static SybaseInt32 operator | (SybaseInt32 x, SybaseInt32 y) 
		{
			return new SybaseInt32 (x.Value | y.Value);
		}

		// Compute Division
		public static SybaseInt32 operator / (SybaseInt32 x, SybaseInt32 y) 
		{
			return new SybaseInt32 (x.Value / y.Value);
		}

		// Compare Equality
		public static SybaseBoolean operator == (SybaseInt32 x, SybaseInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}

		// Bitwise Exclusive-OR (XOR)
		public static SybaseInt32 operator ^ (SybaseInt32 x, SybaseInt32 y) 
		{
			return new SybaseInt32 (x.Value ^ y.Value);
		}

		// > Compare
		public static SybaseBoolean operator >(SybaseInt32 x, SybaseInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value > y.Value);
		}

		// >= Compare
		public static SybaseBoolean operator >= (SybaseInt32 x, SybaseInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value >= y.Value);
		}

		// != Inequality Compare
		public static SybaseBoolean operator != (SybaseInt32 x, SybaseInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value != y.Value);
		}
		
		// < Compare
		public static SybaseBoolean operator < (SybaseInt32 x, SybaseInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value < y.Value);
		}

		// <= Compare
		public static SybaseBoolean operator <= (SybaseInt32 x, SybaseInt32 y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value <= y.Value);
		}

		// Compute Modulus
		public static SybaseInt32 operator % (SybaseInt32 x, SybaseInt32 y) 
		{
			return new SybaseInt32 (x.Value % y.Value);
		}

		// Compute Multiplication
		public static SybaseInt32 operator * (SybaseInt32 x, SybaseInt32 y) 
		{
			return new SybaseInt32 (x.Value * y.Value);
		}

		// Ones Complement
		public static SybaseInt32 operator ~ (SybaseInt32 x) 
		{
			return new SybaseInt32 (~x.Value);
		}

		// Subtraction
		public static SybaseInt32 operator - (SybaseInt32 x, SybaseInt32 y) 
		{
			return new SybaseInt32 (x.Value - y.Value);
		}

		// Negates the Value
		public static SybaseInt32 operator - (SybaseInt32 x) 
		{
			return new SybaseInt32 (-x.Value);
		}

		// Type Conversions
		public static explicit operator SybaseInt32 (SybaseBoolean x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SybaseInt32 ((int)x.ByteValue);
		}

		public static explicit operator SybaseInt32 (SybaseDecimal x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SybaseInt32 ((int)x.Value);
		}

		public static explicit operator SybaseInt32 (SybaseDouble x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SybaseInt32 ((int)x.Value);
		}

		public static explicit operator int (SybaseInt32 x)
		{
			return x.Value;
		}

		public static explicit operator SybaseInt32 (SybaseInt64 x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SybaseInt32 ((int)x.Value);
		}

		public static explicit operator SybaseInt32(SybaseMoney x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SybaseInt32 ((int)x.Value);
		}

		public static explicit operator SybaseInt32(SybaseSingle x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SybaseInt32 ((int)x.Value);
		}

		public static explicit operator SybaseInt32(SybaseString x) 
		{
			return SybaseInt32.Parse (x.Value);
		}

		public static implicit operator SybaseInt32(int x) 
		{
			return new SybaseInt32 (x);
		}

		public static implicit operator SybaseInt32(SybaseByte x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SybaseInt32 ((int)x.Value);
		}

		public static implicit operator SybaseInt32(SybaseInt16 x) 
		{
			if (x.IsNull) 
				return Null;
			else 
				return new SybaseInt32 ((int)x.Value);
		}

		#endregion
	}
}
