using System;
using System.Data;
using System.Data.SqlTypes;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for Field.
	/// </summary>
	internal class Field
	{
/*		protected	SqlByte		byteValue;
		protected	SqlDateTime	dateValue;
		protected	SqlString	stringValue;
		protected	SqlInt32	int32Value;
		protected	SqlInt64	int64Value;
		protected	SqlDouble	doubleValue;
		protected	SqlSingle	singleValue;
		protected	SqlMoney	moneyValue;
		protected	SqlDecimal	decimalValue;
		protected	SqlBinary	binaryValue;*/
		protected	object		value;

		protected	string		tableName;
		protected	string		colName;
		protected	int			colLen;
		protected   string		catalogName;
		protected	DbType		dbType;
		protected	bool		hasValue;

		public Field()
		{
			hasValue = false;
		}

		public string ColumnName 
		{
			get { return colName; }
			set { colName = value; }
		}

		public int ColumnLength
		{
			get { return colLen; }
			set { colLen = value; }
		}

		public string TableName 
		{
			get { return tableName; }
			set { tableName = value; }
		}

		public string CatalogName 
		{
			get { return catalogName; }
			set { catalogName = value; }
		}

	}
}
