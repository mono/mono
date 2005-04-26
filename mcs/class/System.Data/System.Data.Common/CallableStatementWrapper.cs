using System;

using java.sql;
using java.util;

namespace System.Data.Common
{
	public class CallableStatementWrapper : ResultSet
	{
		#region Fields

		CallableStatement _callableStatement;

		#endregion // Fields

		#region Constructors

		public CallableStatementWrapper(CallableStatement callableStatement)
		{
			_callableStatement = callableStatement;
		}

		#endregion // constructors

		#region Methods

		public int getConcurrency() { throw new NotImplementedException(); }

		public int getFetchDirection() { throw new NotImplementedException(); }

		public int getFetchSize()  { throw new NotImplementedException(); }

		public int getRow()  { throw new NotImplementedException(); }

		public int getType()  { throw new NotImplementedException(); }

		public void afterLast()  { throw new NotImplementedException(); }

		public void beforeFirst()  { throw new NotImplementedException(); }

		public void cancelRowUpdates()  { throw new NotImplementedException(); }

		public void clearWarnings()  { throw new NotImplementedException(); }

		public void close()  { throw new NotImplementedException(); }

		public void deleteRow()  { throw new NotImplementedException(); }

		public void insertRow()  { throw new NotImplementedException(); }

		public void moveToCurrentRow()  { throw new NotImplementedException(); }

		public void moveToInsertRow()  { throw new NotImplementedException(); }

		public void refreshRow()  { throw new NotImplementedException(); }

		public void updateRow()  { throw new NotImplementedException(); }

		public bool first()  { throw new NotImplementedException(); }

		public bool isAfterLast()  { throw new NotImplementedException(); }

		public bool isBeforeFirst()  { throw new NotImplementedException(); }

		public bool isFirst()  { throw new NotImplementedException(); }

		public bool isLast()  { throw new NotImplementedException(); }

		public bool last()  { throw new NotImplementedException(); }

		public bool next()  { throw new NotImplementedException(); }

		public bool previous()  { throw new NotImplementedException(); }

		public bool rowDeleted()  { throw new NotImplementedException(); }

		public bool rowInserted()  { throw new NotImplementedException(); }

		public bool rowUpdated()  { throw new NotImplementedException(); }

		public bool wasNull()  { return _callableStatement.wasNull(); }

		public sbyte getByte(int i) { return _callableStatement.getByte(i); } 

		public double getDouble(int i)  { return _callableStatement.getDouble(i); } 

		public float getFloat(int i)  { return _callableStatement.getFloat(i); } 

		public int getInt(int i)  { return _callableStatement.getInt(i); }

		public long getLong(int i)  { return _callableStatement.getLong(i); }

		public short getShort(int i)  { return _callableStatement.getShort(i); }

		public void setFetchDirection(int i)  { throw new NotImplementedException(); }

		public void setFetchSize(int i)  { throw new NotImplementedException(); }

		public void updateNull(int i)  { throw new NotImplementedException(); }

		public bool absolute(int i)  { throw new NotImplementedException(); }

		public bool getBoolean(int i)  { return _callableStatement.getBoolean(i); }

		public bool relative(int i)  { throw new NotImplementedException(); }

		public sbyte[] getBytes(int i)  { return _callableStatement.getBytes(i); }

		public void updateByte(int i, sbyte b)  { throw new NotImplementedException(); }

		public void updateDouble(int i, double v)  { throw new NotImplementedException(); }

		public void updateFloat(int i, float v)  { throw new NotImplementedException(); }

		public void updateInt(int i, int i1)  { throw new NotImplementedException(); }

		public void updateLong(int i, long l)  { throw new NotImplementedException(); }

		public void updateShort(int i, short i1)  { throw new NotImplementedException(); }

		public void updateBoolean(int i, bool b)  { throw new NotImplementedException(); }

		public void updateBytes(int i, sbyte[] bytes)  { throw new NotImplementedException(); }

		public java.io.InputStream getAsciiStream(int i) { throw new NotImplementedException(); }

		public java.io.InputStream getBinaryStream(int i)  { throw new NotImplementedException(); }

		/**
		* @deprecated
		*/
		public java.io.InputStream getUnicodeStream(int i)  { throw new NotImplementedException(); }

		public void updateAsciiStream(int i, java.io.InputStream inputStream, int i1)  { throw new NotImplementedException(); }

		public void updateBinaryStream(int i, java.io.InputStream inputStream, int i1)  { throw new NotImplementedException(); }

		public java.io.Reader getCharacterStream(int i)  { throw new NotImplementedException(); }

		public void updateCharacterStream(int i, java.io.Reader reader, int i1)  { throw new NotImplementedException(); }

		public Object getObject(int i)  { return _callableStatement.getObject(i); }

		public void updateObject(int i, Object o)  { throw new NotImplementedException(); }

		public void updateObject(int i, Object o, int i1)  { throw new NotImplementedException(); }

		public String getCursorName()  { throw new NotImplementedException(); }

		public String getString(int i)  { return _callableStatement.getString(i); }

		public void updateString(int i, String s)  { throw new NotImplementedException(); }

		public sbyte getByte(String s)  { return _callableStatement.getByte(s); }

		public double getDouble(String s)  { return _callableStatement.getDouble(s); }

		public float getFloat(String s)  { return _callableStatement.getFloat(s); }

		public int findColumn(String s)  { throw new NotImplementedException(); }

		public int getInt(String s)  { return _callableStatement.getInt(s); }

		public long getLong(String s)  { return _callableStatement.getLong(s); }

		public short getShort(String s) { return _callableStatement.getShort(s); } 

		public void updateNull(String s)  { throw new NotImplementedException(); }

		public bool getBoolean(String s)  { return _callableStatement.getBoolean(s); }

		public sbyte[] getBytes(String s)  { return _callableStatement.getBytes(s); }

		public void updateByte(String s, sbyte b)  { throw new NotImplementedException(); }

		public void updateDouble(String s, double v)  { throw new NotImplementedException(); }

		public void updateFloat(String s, float v)  { throw new NotImplementedException(); }

		public void updateInt(String s, int i)  { throw new NotImplementedException(); }

		public void updateLong(String s, long l)  { throw new NotImplementedException(); }

		public void updateShort(String s, short i)  { throw new NotImplementedException(); }

		public void updateBoolean(String s, bool b)  { throw new NotImplementedException(); }

		public void updateBytes(String s, sbyte[] bytes)  { throw new NotImplementedException(); }

		public java.math.BigDecimal getBigDecimal(int i)  { return _callableStatement.getBigDecimal(i); }

		/**
		* @deprecated
		*/
		public java.math.BigDecimal getBigDecimal(int i, int i1)  { throw new NotImplementedException(); }

		public void updateBigDecimal(int i, java.math.BigDecimal bigDecimal)  { throw new NotImplementedException(); }

		public java.net.URL getURL(int i)  { throw new NotImplementedException(); }

		public java.sql.Array getArray(int i)  { return _callableStatement.getArray(i); }

		public void updateArray(int i, java.sql.Array array)  { throw new NotImplementedException(); }

		public Blob getBlob(int i)  { return _callableStatement.getBlob(i); }

		public void updateBlob(int i, Blob blob)  { throw new NotImplementedException(); }

		public Clob getClob(int i)  { return _callableStatement.getClob(i); }

		public void updateClob(int i, Clob clob)  { throw new NotImplementedException(); }

		public java.sql.Date getDate(int i)  { return _callableStatement.getDate(i); }

		public void updateDate(int i, java.sql.Date date)  { throw new NotImplementedException(); }

		public Ref getRef(int i)  { return _callableStatement.getRef(i); }

		public void updateRef(int i, Ref rf)  { throw new NotImplementedException(); }

		public ResultSetMetaData getMetaData()  { throw new NotImplementedException(); }

		public SQLWarning getWarnings()  { throw new NotImplementedException(); }

		public Statement getStatement()  { throw new NotImplementedException(); }

		public Time getTime(int i)  { return _callableStatement.getTime(i); }

		public void updateTime(int i, Time time)  { throw new NotImplementedException(); }

		public Timestamp getTimestamp(int i)  { return _callableStatement.getTimestamp(i); }

		public void updateTimestamp(int i, Timestamp timestamp)  { throw new NotImplementedException(); }

		public java.io.InputStream getAsciiStream(String s)  { throw new NotImplementedException(); }

		public java.io.InputStream getBinaryStream(String s)  { throw new NotImplementedException(); }

		/**
		* @deprecated
		*/
		public java.io.InputStream getUnicodeStream(String s)  { throw new NotImplementedException(); }

		public void updateAsciiStream(String s, java.io.InputStream inputStream, int i) { throw new NotImplementedException(); } 

		public void updateBinaryStream(String s, java.io.InputStream inputStream, int i)  { throw new NotImplementedException(); }

		public java.io.Reader getCharacterStream(String s)  { throw new NotImplementedException(); }

		public void updateCharacterStream(String s, java.io.Reader reader, int i)  { throw new NotImplementedException(); }

		public Object getObject(String s)  { return _callableStatement.getObject(s); }

		public void updateObject(String s, Object o)  { throw new NotImplementedException(); }

		public void updateObject(String s, Object o, int i)  { throw new NotImplementedException(); }

		public Object getObject(int i, Map map)  { throw new NotImplementedException(); }

		public String getString(String s)  { return _callableStatement.getString(s); }

		public void updateString(String s, String s1)  { throw new NotImplementedException(); }

		public java.math.BigDecimal getBigDecimal(String s)  { return _callableStatement.getBigDecimal(s); }

		/**
		* @deprecated
		*/
		public java.math.BigDecimal getBigDecimal(String s, int i)  { throw new NotImplementedException(); }

		public void updateBigDecimal(String s, java.math.BigDecimal bigDecimal)  { throw new NotImplementedException(); }

		public java.net.URL getURL(String s)  { throw new NotImplementedException(); }

		public java.sql.Array getArray(String s)  { return _callableStatement.getArray(s); }

		public void updateArray(String s, java.sql.Array array)  { throw new NotImplementedException(); }

		public Blob getBlob(String s)  { return _callableStatement.getBlob(s); }

		public void updateBlob(String s, Blob blob)  { throw new NotImplementedException(); }

		public Clob getClob(String s)  { return _callableStatement.getClob(s); }

		public void updateClob(String s, Clob clob)  { throw new NotImplementedException(); }

		public java.sql.Date getDate(String s)  { return _callableStatement.getDate(s); }

		public void updateDate(String s, java.sql.Date date)  { throw new NotImplementedException(); }

		public java.sql.Date getDate(int i, Calendar calendar)  { throw new NotImplementedException(); }

		public Ref getRef(String s)  { return _callableStatement.getRef(s); }

		public void updateRef(String s, Ref rf)  { throw new NotImplementedException(); }

		public Time getTime(String s)  { return _callableStatement.getTime(s); }

		public void updateTime(String s, Time time)  { throw new NotImplementedException(); }

		public Time getTime(int i, Calendar calendar)  { throw new NotImplementedException(); }

		public Timestamp getTimestamp(String s)  { return _callableStatement.getTimestamp(s); }

		public void updateTimestamp(String s, Timestamp timestamp)  { throw new NotImplementedException(); }

		public Timestamp getTimestamp(int i, Calendar calendar)  { throw new NotImplementedException(); }

		public Object getObject(String s, Map map)  { throw new NotImplementedException(); }

		public java.sql.Date getDate(String s, Calendar calendar)  { throw new NotImplementedException(); }

		public Time getTime(String s, Calendar calendar)  { throw new NotImplementedException(); }

		public Timestamp getTimestamp(String s, Calendar calendar)  { throw new NotImplementedException(); }

		#endregion // Methods
		
	}
}
