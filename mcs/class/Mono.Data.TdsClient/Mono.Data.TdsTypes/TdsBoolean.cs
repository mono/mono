//
// Mono.Data.TdsTypes.TdsBoolean
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
	public struct TdsBoolean : INullable, IComparable 
	{
		#region Fields

		byte value;
		
		// default is false
		private bool notNull;

		public static readonly TdsBoolean False = new TdsBoolean (false);
		public static readonly TdsBoolean Null;
		public static readonly TdsBoolean One = new TdsBoolean (1);
		public static readonly TdsBoolean True = new TdsBoolean (true);
		public static readonly TdsBoolean Zero = new TdsBoolean (0);

		#endregion // Fields

		#region Constructors

		public TdsBoolean (bool value) 
		{
			this.value = (byte) (value ? 1 : 0);
			notNull = true;
		}

		public TdsBoolean (int value) 
		{
			this.value = (byte) (value != 0 ? 1 : 0);
			notNull = true;
		}

		#endregion // Constructors

		#region Properties

		public byte ByteValue {
			get {
				if (this.IsNull)
					throw new TdsNullValueException(Locale.GetText("The property is set to null."));
				else
					return value;
			}
		}

		public bool IsFalse {
			get { 
				if (this.IsNull) 
					return false;
				else 
					return (value == 0);
			}
		}

		public bool IsNull {
			get { 
				return !notNull;
			}
		}

		public bool IsTrue {
			get { 
				if (this.IsNull) 
					return false;
				else 	
					return (value != 0);
			}
		}

		public bool Value {
			get { 
				if (this.IsNull)
					throw new TdsNullValueException(Locale.GetText("The property is set to null."));
				else
					return this.IsTrue;
			}
		}

		#endregion // Properties

		public static TdsBoolean And (TdsBoolean x, TdsBoolean y) 
		{
			return (x & y);
		}

		public int CompareTo (object value) 
		{
			if (value == null)
				return 1;
			else if (!(value is TdsBoolean))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsBoolean"));
			else if (((TdsBoolean)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsBoolean)value).ByteValue);
		}

		public override bool Equals(object value) 
		{
			if (!(value is TdsByte))
				return false;
			else
				return (bool) (this == (TdsBoolean)value);
		}

		public static TdsBoolean Equals(TdsBoolean x, TdsBoolean y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			return (int)value;
		}

		public static TdsBoolean NotEquals(TdsBoolean x, TdsBoolean y) 
		{
			return (x != y);
		}

		public static TdsBoolean OnesComplement(TdsBoolean x) 
		{
			return ~x;
		}

		public static TdsBoolean Or(TdsBoolean x, TdsBoolean y) 
		{
			return (x | y);
		}

		public static TdsBoolean Parse(string s) 
		{
			return new TdsBoolean (Boolean.Parse (s));
		}

		public TdsByte ToTdsByte() 
		{
			return new TdsByte (value);
		}

		// **************************************************
		// Conversion from TdsBoolean to other TdsTypes
		// **************************************************

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

		public TdsInt32 ToTdsInt32() 
		{
			return ((TdsInt32)this);
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

		public TdsString ToTdsString() 
		{
			if (this.IsNull)
			        return new TdsString ("Null");
			if (this.IsTrue)
				return new TdsString ("True");
			else
				return new TdsString ("False");
		}

		public override string ToString() 
		{
			if (this.IsNull)
			        return "Null";
			if (this.IsTrue)
				return "True";
			else
				return "False";
		}

		// Bitwise exclusive-OR (XOR)
		public static TdsBoolean Xor(TdsBoolean x, TdsBoolean y) 
		{
			return (x ^ y);
		}

		// **************************************************
		// Public Operators
		// **************************************************

		// Bitwise AND
		public static TdsBoolean operator & (TdsBoolean x, TdsBoolean y)
		{
			return new TdsBoolean (x.Value & y.Value);
		}

		// Bitwise OR
		public static TdsBoolean operator | (TdsBoolean x, TdsBoolean y)
		{
			return new TdsBoolean (x.Value | y.Value);

		}

		// Compares two instances for equality
		public static TdsBoolean operator == (TdsBoolean x, TdsBoolean y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value == y.Value);
		}
		
		// Bitwize exclusive-OR (XOR)
		public static TdsBoolean operator ^ (TdsBoolean x, TdsBoolean y) 
		{
			return new TdsBoolean (x.Value ^ y.Value);
		}

		// test Value of TdsBoolean to determine it is false.
		public static bool operator false (TdsBoolean x) 
		{
			return x.IsFalse;
		}

		// in-equality
		public static TdsBoolean operator != (TdsBoolean x, TdsBoolean y)
		{
			if (x.IsNull || y.IsNull) 
				return TdsBoolean.Null;
			else
				return new TdsBoolean (x.Value != y.Value);
		}

		// Logical NOT
		public static TdsBoolean operator ! (TdsBoolean x) 
		{
			if (x.IsNull)
				return TdsBoolean.Null;
			else
				return new TdsBoolean (!x.Value);
		}

		// One's Complement
		public static TdsBoolean operator ~ (TdsBoolean x) 
		{
			return new TdsBoolean (~x.ByteValue);
		}

		// test to see if value is true
		public static bool operator true (TdsBoolean x) 
		{
			return x.IsTrue;
		}

		// ****************************************
		// Type Conversion 
		// ****************************************

		
		// TdsBoolean to Boolean
		public static explicit operator bool (TdsBoolean x) 
		{
			return x.Value;
		}

		
		// TdsByte to TdsBoolean
		public static explicit operator TdsBoolean (TdsByte x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsBoolean ((int)x.Value);
		}

		// TdsDecimal to TdsBoolean
		public static explicit operator TdsBoolean (TdsDecimal x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsBoolean ((int)x.Value);
		}
		
		// TdsDouble to TdsBoolean
		public static explicit operator TdsBoolean (TdsDouble x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsBoolean ((int)x.Value);
		}

		// TdsInt16 to TdsBoolean
		public static explicit operator TdsBoolean (TdsInt16 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsBoolean ((int)x.Value);
		}

		// TdsInt32 to TdsBoolean
		public static explicit operator TdsBoolean (TdsInt32 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsBoolean (x.Value);
		}

		// TdsInt64 to TdsBoolean
		public static explicit operator TdsBoolean (TdsInt64 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsBoolean ((int)x.Value);
		}

		// TdsMoney to TdsBoolean
		public static explicit operator TdsBoolean (TdsMoney x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsBoolean ((int)x.Value);
		}

		// TdsSingle to TdsBoolean
		public static explicit operator TdsBoolean (TdsSingle x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new TdsBoolean ((int)x.Value);
		}

		// TdsString to TdsBoolean
		public static explicit operator TdsBoolean (TdsString x) 
		{
			return TdsBoolean.Parse (x.Value);
		}

		// Boolean to TdsBoolean
		public static implicit operator TdsBoolean (bool x) 
		{
			return new TdsBoolean (x);
		}
	}
}
