//
// System.Data.Sql.ISqlGetTypedData
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.SqlTypes;
using System.Xml;

namespace System.Data.Sql {
	public interface ISqlGetTypedData : IGetTypedData
	{
		#region Methods

		SqlBinary GetSqlBinary (int i);
		SqlBoolean GetSqlBoolean (int i);
		SqlByte GetSqlByte (int i);
		SqlBytes GetSqlBytes (int i);
		SqlBytes GetSqlBytesRef (int i);
		SqlChars GetSqlChars (int i);
		SqlChars GetSqlCharsRef (int i);
		SqlDate GetSqlDate (int i);
		SqlDateTime GetSqlDateTime (int i);
		SqlDecimal GetSqlDecimal (int i);
		SqlDouble GetSqlDouble (int i);
		SqlGuid GetSqlGuid (int i);
		SqlInt16 GetSqlInt16 (int i);
		SqlInt32 GetSqlInt32 (int i);
		SqlInt64 GetSqlInt64 (int i);
		SqlMetaData GetSqlMetaData (int i);
		SqlMoney GetSqlMoney (int i);
		SqlSingle GetSqlSingle (int i);
		SqlString GetSqlString (int i);
		SqlTime GetSqlTime (int i);
		SqlUtcDateTime GetSqlUtcDateTime (int i);
		object GetSqlValue (int i);
		SqlXmlReader GetSqlXmlReader (int i);

		#endregion // Methods
	}
}

#endif
