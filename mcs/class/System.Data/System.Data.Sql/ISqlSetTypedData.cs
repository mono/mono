//
// System.Data.Sql.ISqlSetTypedData
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlTypes;

namespace System.Data.Sql {
	public interface ISqlSetTypedData : ISetTypedData
	{
		#region Methods

		void SetSqlBinary (int i, SqlBinary value);
		void SetSqlBoolean (int i, SqlBoolean value);
		void SetSqlByte (int i, SqlByte value);
		void SetSqlBytes (int i, SqlBytes buffer);
		void SetSqlBytesRef (int i, SqlBytes value);
		void SetSqlChars (int i, SqlChars value);
		void SetSqlCharsRef (int i, SqlChars buffer);
		void SetSqlDate (int i, SqlDate value);
		void SetSqlDateTime (int i, SqlDateTime value);
		void SetSqlDecimal (int i, SqlDecimal value);
		void SetSqlDouble (int i, SqlDouble value);
		void SetSqlGuid (int i, SqlGuid value);
		void SetSqlInt16 (int i, SqlInt16 value);
		void SetSqlInt32 (int i, SqlInt32 value);
		void SetSqlInt64 (int i, SqlInt64 value);
		void SetSqlMoney (int i, SqlMoney value);
		void SetSqlSingle (int i, SqlSingle value);
		void SetSqlString (int i, SqlString value);
		void SetSqlTime (int i, SqlTime value);
		void SetSqlUtcDateTime (int i, SqlUtcDateTime value);
		void SetSqlXmlReader (int i, SqlXmlReader value);

		#endregion // Methods
	}
}

#endif
