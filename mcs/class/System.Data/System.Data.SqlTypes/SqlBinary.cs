//
// System.Data.SqlTypes.SqlBinary
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc.
//

namespace System.Data.SqlTypes
{
	/// <summary>
	/// Represents a variable-length stream of binary data to be stored in or retrieved from a database.
	/// </summary>
	public struct SqlBinary : INullable, IComparable
	{
		private byte[] value;
		public static readonly SqlBinary Null = new SqlBinary (null);
		
		public SqlBinary (byte[] value) 
		{
			this.value = value;
		}

		[MonoTODO]
		public int CompareTo (object value) 
		{
			throw new NotImplementedException ();
		}

		public static SqlBinary Concat (SqlBinary x, SqlBinary y) 
		{
			return (x + y);
		}

		[MonoTODO]
		public override bool Equals (object value) 
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals(SqlBinary x, SqlBinary y) 
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode () 
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean GreaterThan (SqlBinary x, SqlBinary y) 
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlBinary x, SqlBinary y) 
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlBinary x, SqlBinary y) 
		{
			return (x < y);
		}

		[MonoTODO]
		public static SqlBoolean LessThanOrEqual (SqlBinary x, SqlBinary y) 
		{
			return (x <= y);
		}

		public static SqlBoolean NotEquals (SqlBinary x, SqlBinary y) 
		{
			return (x != y);
		}

		public SqlGuid ToSqlGuid () 
		{
			return new SqlGuid (value);
		}

		[MonoTODO]
		public override string ToString () 
		{
			throw new NotImplementedException ();
		}
		
		public bool IsNull {
			get { return (value == null); }
		}

		public byte this[int index] {
			get { return value[index]; }
		}

		[MonoTODO]
		public int Length {
			get { throw new NotImplementedException (); }
		}

		public byte[] Value 
		{
			get { return value; }
		}

		[MonoTODO]
		public static SqlBinary operator + (SqlBinary x, SqlBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator == (SqlBinary x, SqlBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator > (SqlBinary x, SqlBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator >= (SqlBinary x, SqlBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator != (SqlBinary x, SqlBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator < (SqlBinary x, SqlBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator <= (SqlBinary x, SqlBinary y) 
		{
			throw new NotImplementedException ();
		}

		public static explicit operator byte[] (SqlBinary x) 
		{
			return x.Value;
		}

		public static explicit operator SqlBinary (SqlGuid x) 
		{
			return x.ToSqlBinary ();
		}

		public static implicit operator SqlBinary (byte[] x) 
		{
			return new SqlBinary (x);
		}
	}
}
