//
// System.Data.SqlTypes.SqlBinary
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc.
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Globalization;

namespace System.Data.SqlTypes
{
	/// <summary>
	/// Represents a variable-length stream of binary data to be stored in or retrieved from a database.
	/// </summary>
	public struct SqlBinary : INullable, IComparable
	{

		#region Fields

		byte[] value;
		private bool notNull;

		public static readonly SqlBinary Null;

		#endregion

		#region Constructors
		
		public SqlBinary (byte[] value) 
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
					throw new SqlNullValueException ("The property contains Null.");
				else if (index >= this.Length)
					throw new SqlNullValueException ("The index parameter indicates a position beyond the length of the byte array.");
				else
					return value [index]; 
			}
		}

		public int Length {
			get { 
				if (this.IsNull)
					throw new SqlNullValueException ("The property contains Null.");
				else
					return value.Length;
			}
		}

		public byte[] Value 
		{
			get { 
				if (this.IsNull) 
					throw new SqlNullValueException ("The property contains Null.");
				else 
					return value; 
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public int CompareTo (object value) 
		{
			throw new NotImplementedException ();
		}

		public static SqlBinary Concat (SqlBinary x, SqlBinary y) 
		{
			return (x + y);
		}

		public override bool Equals (object value) 
		{
			if (!(value is SqlBinary))
				return false;
			else
				return (bool) (this == (SqlBinary)value);
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

		#endregion

		#region Operators

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

		#endregion

		#region Operators

		[MonoTODO]
		public static SqlBinary operator + (SqlBinary x, SqlBinary y) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator == (SqlBinary x, SqlBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator > (SqlBinary x, SqlBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator >= (SqlBinary x, SqlBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator != (SqlBinary x, SqlBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator < (SqlBinary x, SqlBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SqlBoolean operator <= (SqlBinary x, SqlBinary y) 
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		public static explicit operator byte[] (SqlBinary x) 
		{
			return x.Value;
		}

		[MonoTODO]
		public static explicit operator SqlBinary (SqlGuid x) 
		{
			throw new NotImplementedException ();
		}

		public static implicit operator SqlBinary (byte[] x) 
		{
			return new SqlBinary (x);
		}

		#endregion
	}
}
