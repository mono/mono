//
// Mono.Data.SybaseTypes.SybaseGuid
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

using Mono.Data.SybaseClient;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Mono.Data.SybaseTypes {
	public struct SybaseGuid : INullable, IComparable
	{
		#region Fields

	        Guid value;

		private bool notNull;

		public static readonly SybaseGuid Null;

		#endregion

		#region Constructors

		public SybaseGuid (byte[] value) 
		{
			this.value = new Guid (value);
			notNull = true;
		}

		public SybaseGuid (Guid g) 
		{
			this.value = g;
			notNull = true;
		}

		public SybaseGuid (string s) 
		{
			this.value = new Guid (s);
			notNull = true;
		}

		public SybaseGuid (int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) 
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
			else if (!(value is SybaseGuid))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SybaseTypes.SybaseGuid"));
			else if (((SybaseGuid)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SybaseGuid)value).Value);
		}

		public override bool Equals (object value)
		{
			if (!(value is SybaseGuid))
				return false;
			else
				return (bool) (this == (SybaseGuid)value);
		}

		public static SybaseBoolean Equals (SybaseGuid x, SybaseGuid y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 42;
		}

		public static SybaseBoolean GreaterThan (SybaseGuid x, SybaseGuid y)
		{
			return (x > y);
		}

		public static SybaseBoolean GreaterThanOrEqual (SybaseGuid x, SybaseGuid y)
		{
			return (x >= y);
		}

		public static SybaseBoolean LessThan (SybaseGuid x, SybaseGuid y)
		{
			return (x < y);
		}

		public static SybaseBoolean LessThanOrEqual (SybaseGuid x, SybaseGuid y)
		{
			return (x <= y);
		}

		public static SybaseBoolean NotEquals (SybaseGuid x, SybaseGuid y)
		{
			return (x != y);
		}

		[MonoTODO]
		public static SybaseGuid Parse (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] ToByteArray()
		{
			throw new NotImplementedException ();
		}

		public SybaseBinary ToSybaseBinary ()
		{
			return ((SybaseBinary)this);
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

		public static SybaseBoolean operator == (SybaseGuid x, SybaseGuid y)
		{
			if (x.IsNull || y.IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (x.Value == y.Value);
		}

		[MonoTODO]
		public static SybaseBoolean operator > (SybaseGuid x, SybaseGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseBoolean operator >= (SybaseGuid x, SybaseGuid y)
		{
			throw new NotImplementedException ();
		}

		public static SybaseBoolean operator != (SybaseGuid x, SybaseGuid y)
		{
			if (x.IsNull || y.IsNull) return SybaseBoolean.Null;
			return new SybaseBoolean (!(x.Value == y.Value));
		}

		[MonoTODO]
		public static SybaseBoolean operator < (SybaseGuid x, SybaseGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SybaseBoolean operator <= (SybaseGuid x, SybaseGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SybaseGuid (SybaseBinary x)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator Guid (SybaseGuid x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SybaseGuid (SybaseString x)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator SybaseGuid (Guid x)
		{
			return new SybaseGuid (x);
		}

		#endregion
	}
}
			
