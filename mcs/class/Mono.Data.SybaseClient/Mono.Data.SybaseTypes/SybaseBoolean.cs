//
// Mono.Data.SybaseTypes.SybaseBoolean
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Based on System.Data.SqlTypes.SqlBoolean
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
	public struct SybaseBoolean : INullable, IComparable 
	{
		#region Fields

		byte value;
		
		// default is false
		private bool notNull;

		public static readonly SybaseBoolean False = new SybaseBoolean (false);
		public static readonly SybaseBoolean Null;
		public static readonly SybaseBoolean One = new SybaseBoolean (1);
		public static readonly SybaseBoolean True = new SybaseBoolean (true);
		public static readonly SybaseBoolean Zero = new SybaseBoolean (0);

		#endregion // Fields

		#region Constructors

		public SybaseBoolean (bool value) 
		{
			this.value = (byte) (value ? 1 : 0);
			notNull = true;
		}

		public SybaseBoolean (int value) 
		{
			this.value = (byte) (value != 0 ? 1 : 0);
			notNull = true;
		}

		#endregion // Constructors

		#region Properties

		public byte ByteValue {
			get {
				if (this.IsNull)
					throw new SybaseNullValueException(Locale.GetText("The property is set to null."));
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
					throw new SybaseNullValueException(Locale.GetText("The property is set to null."));
				else
					return this.IsTrue;
			}
		}

		#endregion // Properties

		public static SybaseBoolean And (SybaseBoolean x, SybaseBoolean y) 
		{
			return (x & y);
		}

		public int CompareTo (object value) 
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseBoolean))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseBoolean"));
			else if (((SybaseBoolean)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseBoolean)value).ByteValue);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SybaseByte))
				return false;
			else
				return (bool) (this == (SybaseBoolean)value);
		}

		public static SybaseBoolean Equals(SybaseBoolean x, SybaseBoolean y) 
		{
			return (x == y);
		}

		public override int GetHashCode() 
		{
			return (int)value;
		}

		public static SybaseBoolean NotEquals(SybaseBoolean x, SybaseBoolean y) 
		{
			return (x != y);
		}

		public static SybaseBoolean OnesComplement(SybaseBoolean x) 
		{
			return ~x;
		}

		public static SybaseBoolean Or(SybaseBoolean x, SybaseBoolean y) 
		{
			return (x | y);
		}

		public static SybaseBoolean Parse(string s) 
		{
			return new SybaseBoolean (Boolean.Parse (s));
		}

		public SybaseByte ToSybaseByte() 
		{
			return new SybaseByte (value);
		}

		// **************************************************
		// Conversion from SybaseBoolean to other SybaseTypes
		// **************************************************

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

		public SybaseInt32 ToSybaseInt32() 
		{
			return ((SybaseInt32)this);
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

		public SybaseString ToSybaseString() 
		{
			if (this.IsNull)
			        return new SybaseString ("Null");
			if (this.IsTrue)
				return new SybaseString ("True");
			else
				return new SybaseString ("False");
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
		public static SybaseBoolean Xor(SybaseBoolean x, SybaseBoolean y) 
		{
			return (x ^ y);
		}

		// **************************************************
		// Public Operators
		// **************************************************

		// Bitwise AND
		public static SybaseBoolean operator & (SybaseBoolean x, SybaseBoolean y)
		{
			return new SybaseBoolean (x.Value & y.Value);
		}

		// Bitwise OR
		public static SybaseBoolean operator | (SybaseBoolean x, SybaseBoolean y)
		{
			return new SybaseBoolean (x.Value | y.Value);

		}

		// Compares two instances for equality
		public static SybaseBoolean operator == (SybaseBoolean x, SybaseBoolean y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value == y.Value);
		}
		
		// Bitwize exclusive-OR (XOR)
		public static SybaseBoolean operator ^ (SybaseBoolean x, SybaseBoolean y) 
		{
			return new SybaseBoolean (x.Value ^ y.Value);
		}

		// test Value of SybaseBoolean to determine it is false.
		public static bool operator false (SybaseBoolean x) 
		{
			return x.IsFalse;
		}

		// in-equality
		public static SybaseBoolean operator != (SybaseBoolean x, SybaseBoolean y)
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (x.Value != y.Value);
		}

		// Logical NOT
		public static SybaseBoolean operator ! (SybaseBoolean x) 
		{
			if (x.IsNull)
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (!x.Value);
		}

		// One's Complement
		public static SybaseBoolean operator ~ (SybaseBoolean x) 
		{
			return new SybaseBoolean (~x.ByteValue);
		}

		// test to see if value is true
		public static bool operator true (SybaseBoolean x) 
		{
			return x.IsTrue;
		}

		// ****************************************
		// Type Conversion 
		// ****************************************

		
		// SybaseBoolean to Boolean
		public static explicit operator bool (SybaseBoolean x) 
		{
			return x.Value;
		}

		
		// SybaseByte to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseByte x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseBoolean ((int)x.Value);
		}

		// SybaseDecimal to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseDecimal x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseBoolean ((int)x.Value);
		}
		
		// SybaseDouble to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseDouble x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseBoolean ((int)x.Value);
		}

		// SybaseInt16 to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseInt16 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseBoolean ((int)x.Value);
		}

		// SybaseInt32 to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseInt32 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseBoolean (x.Value);
		}

		// SybaseInt64 to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseInt64 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseBoolean ((int)x.Value);
		}

		// SybaseMoney to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseMoney x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseBoolean ((int)x.Value);
		}

		// SybaseSingle to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseSingle x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SybaseBoolean ((int)x.Value);
		}

		// SybaseString to SybaseBoolean
		public static explicit operator SybaseBoolean (SybaseString x) 
		{
			return SybaseBoolean.Parse (x.Value);
		}

		// Boolean to SybaseBoolean
		public static implicit operator SybaseBoolean (bool x) 
		{
			return new SybaseBoolean (x);
		}
	}
}
