//
// Mono.Data.TdsTypes.TdsGuid
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
	public struct TdsGuid : INullable, IComparable
	{
		#region Fields

	        Guid value;

		private bool notNull;

		public static readonly TdsGuid Null;

		#endregion

		#region Constructors

		public TdsGuid (byte[] value) 
		{
			this.value = new Guid (value);
			notNull = true;
		}

		public TdsGuid (Guid g) 
		{
			this.value = g;
			notNull = true;
		}

		public TdsGuid (string s) 
		{
			this.value = new Guid (s);
			notNull = true;
		}

		public TdsGuid (int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) 
		{
			this.value = new Guid (a, b, c, d, e, f, g, h, i, j, k);
			notNull = true;
		}

		#endregion

		#region Properties

		public bool IsNull {
			get { return !notNull; }
		}

		public Guid Value { 
			get { 
				if (this.IsNull) 
					throw new TdsNullValueException ("The property contains Null.");
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
			else if (!(value is TdsGuid))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.TdsTypes.TdsGuid"));
			else if (((TdsGuid)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((TdsGuid)value).Value);
		}

		public override bool Equals (object value)
		{
			if (!(value is TdsGuid))
				return false;
			else
				return (bool) (this == (TdsGuid)value);
		}

		public static TdsBoolean Equals (TdsGuid x, TdsGuid y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 42;
		}

		public static TdsBoolean GreaterThan (TdsGuid x, TdsGuid y)
		{
			return (x > y);
		}

		public static TdsBoolean GreaterThanOrEqual (TdsGuid x, TdsGuid y)
		{
			return (x >= y);
		}

		public static TdsBoolean LessThan (TdsGuid x, TdsGuid y)
		{
			return (x < y);
		}

		public static TdsBoolean LessThanOrEqual (TdsGuid x, TdsGuid y)
		{
			return (x <= y);
		}

		public static TdsBoolean NotEquals (TdsGuid x, TdsGuid y)
		{
			return (x != y);
		}

		[MonoTODO]
		public static TdsGuid Parse (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] ToByteArray()
		{
			throw new NotImplementedException ();
		}

		public TdsBinary ToTdsBinary ()
		{
			return ((TdsBinary)this);
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

		public static TdsBoolean operator == (TdsGuid x, TdsGuid y)
		{
			if (x.IsNull || y.IsNull) return TdsBoolean.Null;
			return new TdsBoolean (x.Value == y.Value);
		}

		[MonoTODO]
		public static TdsBoolean operator > (TdsGuid x, TdsGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsBoolean operator >= (TdsGuid x, TdsGuid y)
		{
			throw new NotImplementedException ();
		}

		public static TdsBoolean operator != (TdsGuid x, TdsGuid y)
		{
			if (x.IsNull || y.IsNull) return TdsBoolean.Null;
			return new TdsBoolean (!(x.Value == y.Value));
		}

		[MonoTODO]
		public static TdsBoolean operator < (TdsGuid x, TdsGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TdsBoolean operator <= (TdsGuid x, TdsGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator TdsGuid (TdsBinary x)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator Guid (TdsGuid x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator TdsGuid (TdsString x)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator TdsGuid (Guid x)
		{
			return new TdsGuid (x);
		}

		#endregion
	}
}
			
