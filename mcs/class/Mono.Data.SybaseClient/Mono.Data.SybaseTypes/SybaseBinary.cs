//
// Mono.Data.SybaseTypes.SybaseBinary
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Based on System.Data.SqlTypes.SqlBinary
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
	public struct SybaseBinary : INullable, IComparable
	{
		#region Fields

		byte[] value;
		private bool notNull;

		public static readonly SybaseBinary Null;

		#endregion

		#region Constructors
		
		public SybaseBinary (byte[] value) 
		{
			this.value = value;
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public byte this[int index] {
			get { 
				if (this.IsNull)
					throw new SybaseNullValueException ("The property contains Null.");
				else if (index >= this.Length)
					throw new SybaseNullValueException ("The index parameter indicates a position beyond the length of the byte array.");
				else
					return value [index]; 
			}
		}

		public int Length {
			get { 
				if (this.IsNull)
					throw new SybaseNullValueException ("The property contains Null.");
				else
					return value.Length;
			}
		}

		public byte[] Value 
		{
			get { 
				if (this.IsNull) 
					throw new SybaseNullValueException ("The property contains Null.");
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		public int CompareTo (object value) 
		{
			if (value == null)
				return 1;
			else if (!(value is SybaseBinary))
				throw new ArgumentException ("Value is not a Mono.Data.SybaseTypes.SybaseBinary.");
			else if (((SybaseBinary) value).IsNull)
				return 1;
			else
				return Compare (this, (SybaseBinary) value);
		}

		public static SybaseBinary Concat (SybaseBinary x, SybaseBinary y) 
		{
			return (x + y);
		}

		public override bool Equals (object value) 
		{
			if (!(value is SybaseBinary))
				return false;
			else
				return (bool) (this == (SybaseBinary)value);
		}

		public static SybaseBoolean Equals (SybaseBinary x, SybaseBinary y) 
		{
			return (x == y);
		}

		public override int GetHashCode () 
		{
			int result = 10;
			for (int i = 0; i < value.Length; i += 1) 
				result = 91 * result + ((int) value [i]);
			return result;
		}

		#endregion

		#region Operators

		public static SybaseBoolean GreaterThan (SybaseBinary x, SybaseBinary y) 
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseBinary x, SybaseBinary y) 
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseBinary x, SybaseBinary y) 
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseBinary x, SybaseBinary y) 
		{
			return (x <= y);
		}

		public static SybaseBoolean NotEquals (SybaseBinary x, SybaseBinary y) 
		{
			return (x != y);
		}

		public SybaseGuid ToSybaseGuid () 
		{
			return new SybaseGuid (value);
		}

		public override string ToString () 
		{
			if (IsNull)
				return "null";
			return String.Format ("SybaseBinary ({0})", Length);
		}

		#endregion

		#region Operators

		public static SybaseBinary operator + (SybaseBinary x, SybaseBinary y) 
		{
			byte[] b = new byte [x.Length + y.Length];
			int j = 0;
			int i;

			for (i = 0; i < x.Length; i += 1)
				b [i] = x [i];

			for (; i < x.Length + y.Length; i += 1) {
				b [i] = y [j];
				j += 1;
			}

			return new SybaseBinary (b);
		}

		public static SybaseBoolean operator == (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (Compare (x, y) == 0);
		}

		public static SybaseBoolean operator > (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (Compare (x, y) > 0);
		}

		public static SybaseBoolean operator >= (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (Compare (x, y) >= 0);
		}

		public static SybaseBoolean operator != (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (Compare (x, y) != 0);
		}

		public static SybaseBoolean operator < (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (Compare (x, y) < 0);
		}

		public static SybaseBoolean operator <= (SybaseBinary x, SybaseBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SybaseBoolean.Null;
			else
				return new SybaseBoolean (Compare (x, y) <= 0);
		}

		public static explicit operator byte[] (SybaseBinary x) 
		{
			return x.Value;
		}

		public static explicit operator SybaseBinary (SybaseGuid x) 
		{
			return new SybaseBinary (x.ToByteArray ());
		}

		public static implicit operator SybaseBinary (byte[] x) 
		{
			return new SybaseBinary (x);
		}

		private static int Compare (SybaseBinary x, SybaseBinary y)
		{
			int lengthDiff = 0;
			
			if (x.Length != y.Length) {
				lengthDiff = x.Length - y.Length;
				
				// if diff more than 0, x is longer
				if (lengthDiff > 0) {
					for (int i = x.Length - 1; i > x.Length - lengthDiff; i -= 1) 
						if (x [i] != (byte) 0)
							return 1;
				} else {
					for (int i = y.Length - 1; i > y.Length - lengthDiff; i -= 1) 
						if (y [i] != (byte) 0)
							return -1;
				}
			}

			// choose shorter
			int len = (lengthDiff > 0) ? y.Length : x.Length;

			for (int i = len - 1; i > 0; i -= 1) {
				byte bx = x [i];
				byte by = y [i];

				if (bx > by)
					return 1;
				else if (bx < by)
					return -1;
			}

			return 0;
		}

		#endregion
	}
}
