//
// System.Data.SqlTypes.SqlGuid
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Globalization;

namespace System.Data.SqlTypes
{
	public struct SqlGuid : INullable, IComparable
	{
		#region Fields

	        Guid value;

		private bool notNull;

		public static readonly SqlGuid Null;

		#endregion

		#region Constructors

		public SqlGuid (byte[] value) 
		{
			this.value = new Guid (value);
			notNull = true;
		}

		public SqlGuid (Guid g) 
		{
			this.value = g;
			notNull = true;
		}

		public SqlGuid (string s) 
		{
			this.value = new Guid (s);
			notNull = true;
		}

		public SqlGuid (int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) 
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
					throw new SqlNullValueException ("The property contains Null.");
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
			else if (!(value is SqlGuid))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlGuid"));
			else if (((SqlGuid)value).IsNull)
				return 1;
			else
				return this.value.CompareTo (((SqlGuid)value).Value);
		}

		public override bool Equals (object value)
		{
			if (!(value is SqlGuid))
				return false;
			else
				return (bool) (this == (SqlGuid)value);
		}

		public static SqlBoolean Equals (SqlGuid x, SqlGuid y)
		{
			return (x == y);
		}

		public override int GetHashCode ()
		{
			byte [] bytes  = this.ToByteArray ();
			
			int result = 10;
			foreach (byte b in  bytes) {
				result = 91 * result + b.GetHashCode ();
			}

			return result;
		}

		public static SqlBoolean GreaterThan (SqlGuid x, SqlGuid y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlGuid x, SqlGuid y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlGuid x, SqlGuid y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlGuid x, SqlGuid y)
		{
			return (x <= y);
		}

		public static SqlBoolean NotEquals (SqlGuid x, SqlGuid y)
		{
			return (x != y);
		}

		public static SqlGuid Parse (string s)
		{
			return new SqlGuid (s);
		}

		public byte[] ToByteArray()
		{
			return value.ToByteArray ();
		}

		public SqlBinary ToSqlBinary ()
		{
			return ((SqlBinary)this);
		}

		public SqlString ToSqlString ()
		{
			return ((SqlString)this);
		}

		public override string ToString ()
		{
			if (this.IsNull)
				return String.Empty;
			else
				return value.ToString ();
		}

		public static SqlBoolean operator == (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.Value.CompareTo (y.Value) > 0)
				return new SqlBoolean (true);
			else
				return new SqlBoolean (false);
		}

		public static SqlBoolean operator >= (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			
			if (x.Value.CompareTo (y.Value) >= 0)
				return new SqlBoolean (true);
			else
				return new SqlBoolean (false);

		}

		public static SqlBoolean operator != (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.Value.CompareTo (y.Value) < 0)
				return new SqlBoolean (true);
			else
				return new SqlBoolean (false);

		}

		public static SqlBoolean operator <= (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.Value.CompareTo (y.Value) <= 0)
				return new SqlBoolean (true);
			else
				return new SqlBoolean (false);
		}

		public static explicit operator SqlGuid (SqlBinary x)
		{
			return new SqlGuid (x.Value);
		}

		public static explicit operator Guid (SqlGuid x)
		{
			return x.Value;
		}

		public static explicit operator SqlGuid (SqlString x)
		{
			return new SqlGuid (x.Value);
		}

		public static implicit operator SqlGuid (Guid x)
		{
			return new SqlGuid (x);
		}

		#endregion
	}
}
			
