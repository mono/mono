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
		protected	SqlDateTime	m_DateTime;
		protected	SqlString	m_StringValue;
		protected	SqlInt64	m_IntValue;
		protected	SqlDouble	m_DoubleValue;
		protected	SqlSingle	m_SingleValue;
		protected	SqlMoney	m_MoneyValue;
		protected	SqlDecimal	m_DecimalValue;
		protected	SqlBinary	m_BinaryValue;

		protected	string		m_TableName;
		protected	string		m_ColName;
		protected	int			m_ColLen;
		protected	DbType		m_DbType;
		protected	bool		m_HasValue;

		public Field()
		{
			m_HasValue = false;
		}

		public string ColumnName 
		{
			get { return m_ColName; }
		}

		public int ColumnLength
		{
			get { return m_ColLen; }
		}

		public string TableName 
		{
			get { return m_TableName; }
		}
	}
}
