//
// System.Data.SqlTypes.SqlBinary
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
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
		public static readonly SqlBinary Null;
		
		[MonoTODO]
		public SqlBinary (byte[] value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int CompareTo (object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBinary Concat (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals (object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean Equals(SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean GreaterThan (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean GreaterThanOrEqual (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean LessThan (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean LessThanOrEqual (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean NotEquals (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlGuid ToSqlGuid () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString () {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool IsNull {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public byte this[int index] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int Length {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public byte[] Value {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static SqlBinary operator + (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator == (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator > (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator >= (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator != (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator < (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator <= (SqlBinary x, SqlBinary y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator byte[] (SqlBinary x) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static explicit operator SqlBinary (SqlGuid x) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static implicit operator SqlBinary (byte[] x) {
			throw new NotImplementedException ();
		}
	}
}
			
