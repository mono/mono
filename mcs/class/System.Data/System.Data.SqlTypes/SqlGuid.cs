//
// System.Data.SqlTypes.SqlGuid
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlGuid : INullable, IComparable
	{
		#region Fields
		private Guid value;

		public static readonly SqlGuid Null;

		#endregion

		#region Constructors

		public SqlGuid (byte[] value) 
		{
			this.value = new Guid (value);
		}

		public SqlGuid (Guid g) 
		{
			this.value = g;
		}

		public SqlGuid (string s) 
		{
			this.value = new Guid (s);
		}

		public SqlGuid (int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k) 
		{
			this.value = new Guid (a, b, c, d, e, f, g, h, i, j, k);
		}

		#endregion

		#region Properties

		[MonoTODO]
		public bool IsNull {
			get { throw new NotImplementedException (); }
		}

		public Guid Value { 
			get { return value; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlGuid x, SqlGuid y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return 42;
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

		[MonoTODO]
		public static SqlGuid Parse (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] ToByteArray()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlBinary ToSqlBinary ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlString ToSqlString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean operator == (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		[MonoTODO]
		public static SqlBoolean operator > (SqlGuid x, SqlGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator >= (SqlGuid x, SqlGuid y)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean operator != (SqlGuid x, SqlGuid y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		[MonoTODO]
		public static SqlBoolean operator < (SqlGuid x, SqlGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator <= (SqlGuid x, SqlGuid y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlGuid (SqlBinary x)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator Guid (SqlGuid x)
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SqlGuid (SqlString x)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlGuid (Guid x)
		{
			return new SqlGuid (x);
		}

		#endregion
	}
}
			
