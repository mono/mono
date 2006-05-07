//
// Mainsoft.Data.Jdbc.Providers
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Mainsoft.Data.Jdbc.Providers {
	public class Connection : java.sql.Connection {

		readonly java.sql.Connection _connection;

		public Connection(java.sql.Connection connection) {
			_connection = connection;
		}

		protected java.sql.Connection Wrapped {
			get { return _connection; }
		}

		#region Connection Members

		public virtual bool isClosed() {
			return Wrapped.isClosed();
		}

		public virtual bool isReadOnly() {
			return Wrapped.isReadOnly();
		}

		public virtual void setTransactionIsolation(int arg_0) {
			Wrapped.setTransactionIsolation(arg_0);
		}

		public virtual void releaseSavepoint(java.sql.Savepoint arg_0) {
			Wrapped.releaseSavepoint(arg_0);
		}

		public virtual void setTypeMap(java.util.Map arg_0) {
			Wrapped.setTypeMap(arg_0);
		}

		public virtual string getCatalog() {
			return Wrapped.getCatalog();
		}

		public virtual int getHoldability() {
			return Wrapped.getHoldability();
		}

		public virtual void rollback() {
			Wrapped.rollback();
		}

		public virtual void rollback(java.sql.Savepoint arg_0) {
			Wrapped.rollback(arg_0);
		}

		public virtual java.sql.CallableStatement prepareCall(string arg_0) {
			return Wrapped.prepareCall(arg_0);
		}

		public virtual java.sql.CallableStatement prepareCall(string arg_0, int arg_1, int arg_2) {
			return Wrapped.prepareCall(arg_0, arg_1, arg_2);
		}

		public virtual java.sql.CallableStatement prepareCall(string arg_0, int arg_1, int arg_2, int arg_3) {
			return Wrapped.prepareCall(arg_0, arg_1, arg_2, arg_3);
		}

		public virtual void setAutoCommit(bool arg_0) {
			Wrapped.setAutoCommit(arg_0);
		}

		public virtual java.sql.Savepoint setSavepoint() {
			return Wrapped.setSavepoint();
		}

		public virtual java.sql.Savepoint setSavepoint(string arg_0) {
			return Wrapped.setSavepoint(arg_0);
		}

		public virtual java.sql.Statement createStatement() {
			return Wrapped.createStatement();
		}

		public virtual java.sql.Statement createStatement(int arg_0, int arg_1) {
			return Wrapped.createStatement(arg_0, arg_1);
		}

		public virtual java.sql.Statement createStatement(int arg_0, int arg_1, int arg_2) {
			return Wrapped.createStatement(arg_0, arg_1, arg_2);
		}

		public virtual void setCatalog(string arg_0) {
			Wrapped.setCatalog(arg_0);
		}

		public virtual java.sql.PreparedStatement prepareStatement(string arg_0) {
			return Wrapped.prepareStatement(arg_0);
		}

		public virtual java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1, int arg_2) {
			return Wrapped.prepareStatement(arg_0, arg_1, arg_2);
		}

		public virtual java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1, int arg_2, int arg_3) {
			return Wrapped.prepareStatement(arg_0, arg_1, arg_2, arg_3);
		}

		public virtual java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1) {
			return Wrapped.prepareStatement(arg_0, arg_1);
		}

		public virtual java.sql.PreparedStatement prepareStatement(string arg_0, int[] arg_1) {
			return Wrapped.prepareStatement(arg_0, arg_1);
		}

		public virtual java.sql.PreparedStatement prepareStatement(string arg_0, string[] arg_1) {
			return Wrapped.prepareStatement(arg_0, arg_1);
		}

		public virtual void setHoldability(int arg_0) {
			Wrapped.setHoldability(arg_0);
		}

		public virtual void commit() {
			Wrapped.commit();
		}

		public virtual java.sql.DatabaseMetaData getMetaData() {
			return Wrapped.getMetaData();
		}

		public virtual int getTransactionIsolation() {
			return Wrapped.getTransactionIsolation();
		}

		public virtual bool getAutoCommit() {
			return Wrapped.getAutoCommit();
		}

		public virtual java.sql.SQLWarning getWarnings() {
			return Wrapped.getWarnings();
		}

		public virtual java.util.Map getTypeMap() {
			return Wrapped.getTypeMap();
		}

		public virtual void close() {
			Wrapped.close();
		}

		public virtual string nativeSQL(string arg_0) {
			return Wrapped.nativeSQL(arg_0);;
		}

		public virtual void setReadOnly(bool arg_0) {
			Wrapped.setReadOnly(arg_0);
		}

		public virtual void clearWarnings() {
			Wrapped.clearWarnings();
		}

		#endregion

	}

	public class Statement : java.sql.Statement {

		readonly java.sql.Statement _statement;

		public Statement(java.sql.Statement statement) {
			_statement = statement;
		}

		protected java.sql.Statement Wrapped {
			get { return _statement; }
		}

		#region Statement Members

		public virtual java.sql.Connection getConnection() {
			return Wrapped.getConnection();
		}

		public virtual void setEscapeProcessing(bool arg_0) {
			Wrapped.setEscapeProcessing(arg_0);
		}

		public virtual void setMaxFieldSize(int arg_0) {
			Wrapped.setMaxFieldSize(arg_0);
		}

		public virtual void cancel() {
			Wrapped.cancel();
		}

		public virtual bool getMoreResults() {
			return Wrapped.getMoreResults();
		}

		public virtual bool getMoreResults(int arg_0) {
			return Wrapped.getMoreResults(arg_0);
		}

		public virtual int executeUpdate(string arg_0) {
			return Wrapped.executeUpdate(arg_0);
		}

		public virtual int executeUpdate(string arg_0, int arg_1) {
			return Wrapped.executeUpdate(arg_0, arg_1);
		}

		public virtual int executeUpdate(string arg_0, int[] arg_1) {
			return Wrapped.executeUpdate(arg_0, arg_1);
		}

		public virtual int executeUpdate(string arg_0, string[] arg_1) {
			return Wrapped.executeUpdate(arg_0, arg_1);
		}

		public virtual java.sql.ResultSet getResultSet() {
			return Wrapped.getResultSet();
		}

		public virtual int getResultSetConcurrency() {
			return Wrapped.getResultSetConcurrency();
		}

		public virtual void setQueryTimeout(int arg_0) {
			Wrapped.setQueryTimeout(arg_0);
		}

		public virtual int getUpdateCount() {
			return Wrapped.getUpdateCount();
		}

		public virtual int getResultSetType() {
			return Wrapped.getResultSetType();
		}

		public virtual int[] executeBatch() {
			return Wrapped.executeBatch();
		}

		public virtual void setFetchDirection(int arg_0) {
			Wrapped.setFetchDirection(arg_0);
		}

		public virtual void setMaxRows(int arg_0) {
			Wrapped.setMaxRows(arg_0);
		}

		public virtual void addBatch(string arg_0) {
			Wrapped.addBatch(arg_0);
		}

		public virtual bool execute(string arg_0) {
			return Wrapped.execute(arg_0);
		}

		public virtual bool execute(string arg_0, int arg_1) {
			return Wrapped.execute(arg_0, arg_1);
		}

		public virtual bool execute(string arg_0, int[] arg_1) {
			return Wrapped.execute(arg_0, arg_1);
		}

		public virtual bool execute(string arg_0, string[] arg_1) {
			return Wrapped.execute(arg_0, arg_1);
		}

		public virtual int getResultSetHoldability() {
			return Wrapped.getResultSetHoldability();
		}

		public virtual java.sql.ResultSet executeQuery(string arg_0) {
			return Wrapped.executeQuery(arg_0);
		}

		public virtual void setCursorName(string arg_0) {
			Wrapped.setCursorName(arg_0);
		}

		public virtual java.sql.SQLWarning getWarnings() {
			return Wrapped.getWarnings();
		}

		public virtual int getMaxRows() {
			return Wrapped.getMaxRows();
		}

		public virtual int getFetchSize() {
			return Wrapped.getFetchSize();
		}

		public virtual void clearBatch() {
			Wrapped.clearBatch();
		}

		public virtual int getQueryTimeout() {
			return Wrapped.getQueryTimeout();
		}

		public virtual java.sql.ResultSet getGeneratedKeys() {
			return Wrapped.getGeneratedKeys();
		}

		public virtual int getFetchDirection() {
			return Wrapped.getFetchDirection();
		}

		public virtual void close() {
			Wrapped.close();
		}

		public virtual int getMaxFieldSize() {
			return Wrapped.getMaxFieldSize();
		}

		public virtual void clearWarnings() {
			Wrapped.clearWarnings();
		}

		public virtual void setFetchSize(int arg_0) {
			Wrapped.setFetchSize(arg_0);
		}

		#endregion

	}

	public class PreparedStatement : Statement, java.sql.PreparedStatement {

		public PreparedStatement(java.sql.PreparedStatement statement) : base(statement) {
		}

		protected new java.sql.PreparedStatement Wrapped {
			get { return (java.sql.PreparedStatement)base.Wrapped; }
		}

		#region PreparedStatement Members

		public virtual void setBlob(int arg_0, java.sql.Blob arg_1) {
			Wrapped.setBlob(arg_0, arg_1);
		}

		public virtual java.sql.ParameterMetaData getParameterMetaData() {
			return Wrapped.getParameterMetaData();
		}

		public virtual void setInt(int arg_0, int arg_1) {
			Wrapped.setInt(arg_0, arg_1);
		}

		public virtual void setFloat(int arg_0, float arg_1) {
			Wrapped.setFloat(arg_0, arg_1);
		}

		public virtual int executeUpdate() {
			return Wrapped.executeUpdate();
		}

		public virtual void setLong(int arg_0, long arg_1) {
			Wrapped.setLong(arg_0, arg_1);
		}

		public virtual void setNull(int arg_0, int arg_1) {
			Wrapped.setNull(arg_0, arg_1);
		}

		public virtual void setNull(int arg_0, int arg_1, string arg_2) {
			Wrapped.setNull(arg_0, arg_1, arg_2);
		}

		public virtual void setUnicodeStream(int arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.setUnicodeStream(arg_0, arg_1, arg_2);
		}

		public virtual void setShort(int arg_0, short arg_1) {
			Wrapped.setShort(arg_0, arg_1);
		}

		public virtual void setByte(int arg_0, sbyte arg_1) {
			Wrapped.setByte(arg_0, arg_1);
		}

		public virtual bool execute() {
			return Wrapped.execute();
		}

		public virtual void addBatch() {
			Wrapped.addBatch();
		}

		public virtual void setURL(int arg_0, java.net.URL arg_1) {
			Wrapped.setURL(arg_0, arg_1);
		}

		public virtual void setBigDecimal(int arg_0, java.math.BigDecimal arg_1) {
			Wrapped.setBigDecimal(arg_0, arg_1);
		}

		public virtual void setBytes(int arg_0, sbyte[] arg_1) {
			Wrapped.setBytes(arg_0, arg_1);
		}

		public virtual void setDouble(int arg_0, double arg_1) {
			Wrapped.setDouble(arg_0, arg_1);
		}

		public virtual void setRef(int arg_0, java.sql.Ref arg_1) {
			Wrapped.setRef(arg_0, arg_1);
		}

		public virtual java.sql.ResultSetMetaData getMetaData() {
			return Wrapped.getMetaData();
		}

		public virtual void setTimestamp(int arg_0, java.sql.Timestamp arg_1) {
			Wrapped.setTimestamp(arg_0, arg_1);
		}

		public virtual void setTimestamp(int arg_0, java.sql.Timestamp arg_1, java.util.Calendar arg_2) {
			Wrapped.setTimestamp(arg_0, arg_1, arg_2);
		}

		public virtual java.sql.ResultSet executeQuery() {
			return Wrapped.executeQuery();
		}

		public virtual void setCharacterStream(int arg_0, java.io.Reader arg_1, int arg_2) {
			Wrapped.setCharacterStream(arg_0, arg_1, arg_2);
		}

		public virtual void setTime(int arg_0, java.sql.Time arg_1) {
			Wrapped.setTime(arg_0, arg_1);
		}

		public virtual void setTime(int arg_0, java.sql.Time arg_1, java.util.Calendar arg_2) {
			Wrapped.setTime(arg_0, arg_1, arg_2);
		}

		public virtual void setBoolean(int arg_0, bool arg_1) {
			Wrapped.setBoolean(arg_0, arg_1);
		}

		public virtual void setString(int arg_0, string arg_1) {
			Wrapped.setString(arg_0, arg_1);
		}

		public virtual void setBinaryStream(int arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.setBinaryStream(arg_0, arg_1, arg_2);
		}

		public virtual void clearParameters() {
			Wrapped.clearParameters();
		}

		public virtual void setObject(int arg_0, object arg_1, int arg_2, int arg_3) {
			Wrapped.setObject(arg_0, arg_1, arg_2, arg_3);
		}

		public virtual void setObject(int arg_0, object arg_1, int arg_2) {
			Wrapped.setObject(arg_0, arg_1, arg_2);
		}

		public virtual void setObject(int arg_0, object arg_1) {
			Wrapped.setObject(arg_0, arg_1);
		}

		public virtual void setArray(int arg_0, java.sql.Array arg_1) {
			Wrapped.setArray(arg_0, arg_1);
		}

		public virtual void setDate(int arg_0, java.sql.Date arg_1) {
			Wrapped.setDate(arg_0, arg_1);
		}

		public virtual void setDate(int arg_0, java.sql.Date arg_1, java.util.Calendar arg_2) {
			Wrapped.setDate(arg_0, arg_1, arg_2);
		}

		public virtual void setAsciiStream(int arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.setAsciiStream(arg_0, arg_1, arg_2);
		}

		public virtual void setClob(int arg_0, java.sql.Clob arg_1) {
			Wrapped.setClob(arg_0, arg_1);
		}

		#endregion

	}

	public class CallableStatement : PreparedStatement, java.sql.CallableStatement {

		public CallableStatement(java.sql.CallableStatement statement) : base(statement) {
	}

		protected new java.sql.CallableStatement Wrapped {
			get { return (java.sql.CallableStatement)base.Wrapped; }
		}

		#region CallableStatement Members

		public virtual java.sql.Clob getClob(int arg_0) {
			return Wrapped.getClob(arg_0);
		}

		public virtual java.sql.Clob getClob(string arg_0) {
			return Wrapped.getClob(arg_0);
		}

		public virtual int getInt(int arg_0) {
			return Wrapped.getInt(arg_0);
		}

		public virtual int getInt(string arg_0) {
			return Wrapped.getInt(arg_0);
		}

		public virtual java.sql.Array getArray(int arg_0) {
			return Wrapped.getArray(arg_0);
		}

		public virtual java.sql.Array getArray(string arg_0) {
			return Wrapped.getArray(arg_0);
		}

		public virtual void setInt(string arg_0, int arg_1) {
			Wrapped.setInt(arg_0, arg_1);
		}

		public virtual void setFloat(string arg_0, float arg_1) {
			Wrapped.setFloat(arg_0, arg_1);
		}

		public virtual java.net.URL getURL(int arg_0) {
			return Wrapped.getURL(arg_0);
		}

		public virtual java.net.URL getURL(string arg_0) {
			return Wrapped.getURL(arg_0);
		}

		public virtual void registerOutParameter(int arg_0, int arg_1) {
			Wrapped.registerOutParameter(arg_0, arg_1);
		}

		public virtual void registerOutParameter(int arg_0, int arg_1, int arg_2) {
			Wrapped.registerOutParameter(arg_0, arg_1, arg_2);
		}

		public virtual void registerOutParameter(int arg_0, int arg_1, string arg_2) {
			Wrapped.registerOutParameter(arg_0, arg_1, arg_2);
		}

		public virtual void registerOutParameter(string arg_0, int arg_1) {
			Wrapped.registerOutParameter(arg_0, arg_1);
		}

		public virtual void registerOutParameter(string arg_0, int arg_1, int arg_2) {
			Wrapped.registerOutParameter(arg_0, arg_1, arg_2);
		}

		public virtual void registerOutParameter(string arg_0, int arg_1, string arg_2) {
			Wrapped.registerOutParameter(arg_0, arg_1, arg_2);
		}

		public virtual long getLong(int arg_0) {
			return Wrapped.getLong(arg_0);
		}

		public virtual long getLong(string arg_0) {
			return Wrapped.getLong(arg_0);
		}

		public virtual void setLong(string arg_0, long arg_1) {
			Wrapped.setLong(arg_0, arg_1);
		}

		public virtual void setNull(string arg_0, int arg_1) {
			Wrapped.setNull(arg_0, arg_1);
		}

		public virtual void setNull(string arg_0, int arg_1, string arg_2) {
			Wrapped.setNull(arg_0, arg_1, arg_2);
		}

		public virtual object getObject(int arg_0) {
			return Wrapped.getObject(arg_0);
		}

		public virtual object getObject(int arg_0, java.util.Map arg_1) {
			return Wrapped.getObject(arg_0, arg_1);
		}

		public virtual object getObject(string arg_0) {
			return Wrapped.getObject(arg_0);
		}

		public virtual object getObject(string arg_0, java.util.Map arg_1) {
			return Wrapped.getObject(arg_0, arg_1);
		}

		public virtual bool wasNull() {
			return Wrapped.wasNull();
		}

		public virtual sbyte[] getBytes(int arg_0) {
			return Wrapped.getBytes(arg_0);
		}

		public virtual sbyte[] getBytes(string arg_0) {
			return Wrapped.getBytes(arg_0);
		}

		public virtual void setShort(string arg_0, short arg_1) {
			Wrapped.setShort(arg_0, arg_1);
		}

		public virtual void setByte(string arg_0, sbyte arg_1) {
			Wrapped.setByte(arg_0, arg_1);
		}

		public virtual sbyte getByte(int arg_0) {
			return Wrapped.getByte(arg_0);
		}

		public virtual sbyte getByte(string arg_0) {
			return Wrapped.getByte(arg_0);
		}

		public virtual void setURL(string arg_0, java.net.URL arg_1) {
			Wrapped.setURL(arg_0, arg_1);
		}

		public virtual double getDouble(int arg_0) {
			return Wrapped.getDouble(arg_0);
		}

		public virtual double getDouble(string arg_0) {
			return Wrapped.getDouble(arg_0);
		}

		public virtual void setBigDecimal(string arg_0, java.math.BigDecimal arg_1) {
			Wrapped.setBigDecimal(arg_0, arg_1);
		}

		public virtual float getFloat(int arg_0) {
			return Wrapped.getFloat(arg_0);
		}

		public virtual float getFloat(string arg_0) {
			return Wrapped.getFloat(arg_0);
		}

		public virtual void setBytes(string arg_0, sbyte[] arg_1) {
			Wrapped.setBytes(arg_0, arg_1);
		}

		public virtual short getShort(int arg_0) {
			return Wrapped.getShort(arg_0);
		}

		public virtual short getShort(string arg_0) {
			return Wrapped.getShort(arg_0);
		}

		public virtual java.sql.Timestamp getTimestamp(int arg_0) {
			return Wrapped.getTimestamp(arg_0);
		}

		public virtual java.sql.Timestamp getTimestamp(int arg_0, java.util.Calendar arg_1) {
			return Wrapped.getTimestamp(arg_0, arg_1);
		}

		public virtual java.sql.Timestamp getTimestamp(string arg_0) {
			return Wrapped.getTimestamp(arg_0);
		}

		public virtual java.sql.Timestamp getTimestamp(string arg_0, java.util.Calendar arg_1) {
			return Wrapped.getTimestamp(arg_0, arg_1);
		}

		public virtual void setDouble(string arg_0, double arg_1) {
			Wrapped.setDouble(arg_0, arg_1);
		}

		public virtual void setTimestamp(string arg_0, java.sql.Timestamp arg_1) {
			Wrapped.setTimestamp(arg_0, arg_1);
		}

		public virtual void setTimestamp(string arg_0, java.sql.Timestamp arg_1, java.util.Calendar arg_2) {
			Wrapped.setTimestamp(arg_0, arg_1, arg_2);
		}

		public virtual bool getBoolean(int arg_0) {
			return Wrapped.getBoolean(arg_0);
		}

		public virtual bool getBoolean(string arg_0) {
			return Wrapped.getBoolean(arg_0);
		}

		public virtual void setCharacterStream(string arg_0, java.io.Reader arg_1, int arg_2) {
			Wrapped.setCharacterStream(arg_0, arg_1, arg_2);
		}

		public virtual void setTime(string arg_0, java.sql.Time arg_1) {
			Wrapped.setTime(arg_0, arg_1);
		}

		public virtual void setTime(string arg_0, java.sql.Time arg_1, java.util.Calendar arg_2) {
			Wrapped.setTime(arg_0, arg_1, arg_2);
		}

		public virtual void setBoolean(string arg_0, bool arg_1) {
			Wrapped.setBoolean(arg_0, arg_1);
		}

		public virtual java.math.BigDecimal getBigDecimal(int arg_0, int arg_1) {
			return Wrapped.getBigDecimal(arg_0, arg_1);
		}

		public virtual java.math.BigDecimal getBigDecimal(int arg_0) {
			return Wrapped.getBigDecimal(arg_0);
		}

		public virtual java.math.BigDecimal getBigDecimal(string arg_0) {
			return Wrapped.getBigDecimal(arg_0);
		}

		public virtual java.sql.Ref getRef(int arg_0) {
			return Wrapped.getRef(arg_0);
		}

		public virtual java.sql.Ref getRef(string arg_0) {
			return Wrapped.getRef(arg_0);
		}

		public virtual string getString(int arg_0) {
			return Wrapped.getString(arg_0);
		}

		public virtual string getString(string arg_0) {
			return Wrapped.getString(arg_0);
		}

		public virtual void setString(string arg_0, string arg_1) {
			Wrapped.setString(arg_0, arg_1);
		}

		public virtual java.sql.Blob getBlob(int arg_0) {
			return Wrapped.getBlob(arg_0);
		}

		public virtual java.sql.Blob getBlob(string arg_0) {
			return Wrapped.getBlob(arg_0);
		}

		public virtual void setBinaryStream(string arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.setBinaryStream(arg_0, arg_1, arg_2);
		}

		public virtual java.sql.Time getTime(int arg_0) {
			return Wrapped.getTime(arg_0);
		}

		public virtual java.sql.Time getTime(int arg_0, java.util.Calendar arg_1) {
			return Wrapped.getTime(arg_0, arg_1);;
		}

		public virtual java.sql.Time getTime(string arg_0) {
			return Wrapped.getTime(arg_0);
		}

		public virtual java.sql.Time getTime(string arg_0, java.util.Calendar arg_1) {
			return Wrapped.getTime(arg_0, arg_1);
		}

		public virtual void setObject(string arg_0, object arg_1, int arg_2, int arg_3) {
			Wrapped.setObject(arg_0, arg_1, arg_2, arg_3);
		}

		public virtual void setObject(string arg_0, object arg_1, int arg_2) {
			Wrapped.setObject(arg_0, arg_1, arg_2);
		}

		public virtual void setObject(string arg_0, object arg_1) {
			Wrapped.setObject(arg_0, arg_1);
		}

		public virtual void setDate(string arg_0, java.sql.Date arg_1) {
			Wrapped.setDate(arg_0, arg_1);
		}

		public virtual void setDate(string arg_0, java.sql.Date arg_1, java.util.Calendar arg_2) {
			Wrapped.setDate(arg_0, arg_1, arg_2);
		}

		public virtual void setAsciiStream(string arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.setAsciiStream(arg_0, arg_1, arg_2);
		}

		public virtual java.sql.Date getDate(int arg_0) {
			return Wrapped.getDate(arg_0);
		}

		public virtual java.sql.Date getDate(int arg_0, java.util.Calendar arg_1) {
			return Wrapped.getDate(arg_0, arg_1);
		}

		public virtual java.sql.Date getDate(string arg_0) {
			return Wrapped.getDate(arg_0);
		}

		public virtual java.sql.Date getDate(string arg_0, java.util.Calendar arg_1) {
			return Wrapped.getDate(arg_0, arg_1);
		}

		#endregion

	}

	public class ResultSet : java.sql.ResultSet	{
		readonly java.sql.ResultSet _resultSet;

		public ResultSet (java.sql.ResultSet resultSet) {
			_resultSet = resultSet;
		}

		protected java.sql.ResultSet Wrapped {
			get { return _resultSet; }
		}

		#region ResultSet Members

		public virtual java.io.InputStream getBinaryStream(int arg_0) {
			return Wrapped.getBinaryStream (arg_0);			
		}

		public virtual java.io.InputStream getBinaryStream(string arg_0) {
			return Wrapped.getBinaryStream (arg_0);
		}

		public virtual void updateTime(int arg_0, java.sql.Time arg_1) {
			Wrapped.updateTime (arg_0, arg_1);
		}

		public virtual void updateTime(string arg_0, java.sql.Time arg_1) {
			Wrapped.updateTime (arg_0, arg_1);
		}

		public virtual bool next() {
			return Wrapped.next ();
		}

		public virtual int getInt(int arg_0) {
			return Wrapped.getInt (arg_0);
		}

		public virtual int getInt(string arg_0) {
			return Wrapped.getInt (arg_0);
		}

		public virtual java.io.InputStream getUnicodeStream(int arg_0) {
			return Wrapped.getUnicodeStream (arg_0);
		}

		public virtual java.io.InputStream getUnicodeStream(string arg_0) {
			return Wrapped.getUnicodeStream (arg_0);
		}

		public virtual bool isAfterLast() {
			return Wrapped.isAfterLast ();
		}

		public virtual bool isLast() {
			return Wrapped.isLast ();
		}

		public virtual bool isFirst() {
			return Wrapped.isFirst ();
		}

		public virtual void updateRow() {
			Wrapped.updateRow ();
		}

		public virtual java.net.URL getURL(int arg_0) {
			return Wrapped.getURL (arg_0);			
		}

		public virtual java.net.URL getURL(string arg_0) {
			return Wrapped.getURL (arg_0);			
		}

		public virtual bool rowDeleted() {
			return Wrapped.rowDeleted ();
		}

		public virtual void updateFloat(int arg_0, float arg_1) {
			Wrapped.updateFloat (arg_0, arg_1);
		}

		public virtual void updateFloat(string arg_0, float arg_1) {
			Wrapped.updateFloat (arg_0, arg_1);
		}

		public virtual object getObject(int arg_0) {
			return Wrapped.getObject (arg_0);
		}

		public virtual object getObject(string arg_0) {
			return Wrapped.getObject (arg_0);
		}

		public virtual object getObject(int arg_0, java.util.Map arg_1) {
			return Wrapped.getObject (arg_0, arg_1);
		}

		public virtual object getObject(string arg_0, java.util.Map arg_1) {
			return Wrapped.getObject (arg_0, arg_1);
		}

		public virtual sbyte[] getBytes(int arg_0) {
			return Wrapped.getBytes (arg_0);
		}

		public virtual sbyte[] getBytes(string arg_0) {
			return Wrapped.getBytes (arg_0);
		}

		public virtual void moveToCurrentRow() {
			Wrapped.moveToCurrentRow ();
		}

		public virtual sbyte getByte(int arg_0) {
			return Wrapped.getByte (arg_0);
		}

		public virtual sbyte getByte(string arg_0) {
			return Wrapped.getByte (arg_0);
		}

		public virtual void updateCharacterStream(int arg_0, java.io.Reader arg_1, int arg_2) {
			Wrapped.updateCharacterStream (arg_0, arg_1, arg_2);
		}

		public virtual void updateCharacterStream(string arg_0, java.io.Reader arg_1, int arg_2) {
			Wrapped.updateCharacterStream (arg_0, arg_1, arg_2);
		}

		public virtual void insertRow() {
			Wrapped.insertRow ();
		}

		public virtual void updateBoolean(int arg_0, bool arg_1) {
			Wrapped.updateBoolean (arg_0, arg_1);
		}

		public virtual void updateBoolean(string arg_0, bool arg_1) {
			Wrapped.updateBoolean (arg_0, arg_1);
		}

		public virtual bool relative(int arg_0) {
			return Wrapped.relative (arg_0);
		}

		public virtual java.sql.Timestamp getTimestamp(int arg_0) {
			return Wrapped.getTimestamp (arg_0);
		}

		public virtual java.sql.Timestamp getTimestamp(string arg_0) {
			return Wrapped.getTimestamp (arg_0);
		}

		public virtual java.sql.Timestamp getTimestamp(int arg_0, java.util.Calendar arg_1) {
			return Wrapped.getTimestamp (arg_0, arg_1);
		}

		public virtual java.sql.Timestamp getTimestamp(string arg_0, java.util.Calendar arg_1) {
			return Wrapped.getTimestamp (arg_0, arg_1);
		}

		public virtual void updateBigDecimal(int arg_0, java.math.BigDecimal arg_1) {
			Wrapped.updateBigDecimal (arg_0, arg_1);
		}

		public virtual void updateBigDecimal(string arg_0, java.math.BigDecimal arg_1) {
			Wrapped.updateBigDecimal (arg_0, arg_1);
		}

		public virtual void updateBinaryStream(int arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.updateBinaryStream (arg_0, arg_1, arg_2);
		}

		public virtual void updateBinaryStream(string arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.updateBinaryStream (arg_0, arg_1, arg_2);
		}

		public virtual void updateByte(int arg_0, sbyte arg_1) {
			Wrapped.updateByte (arg_0, arg_1);
		}

		public virtual void updateByte(string arg_0, sbyte arg_1) {
			Wrapped.updateByte (arg_0, arg_1);
		}

		public virtual bool isBeforeFirst() {
			return Wrapped.isBeforeFirst ();
		}

		public virtual bool first() {
			return Wrapped.first ();
		}

		public virtual java.math.BigDecimal getBigDecimal(int arg_0, int arg_1) {
			return Wrapped.getBigDecimal (arg_0, arg_1);
		}

		public virtual java.math.BigDecimal getBigDecimal(string arg_0, int arg_1) {
			return Wrapped.getBigDecimal (arg_0, arg_1);
		}

		public virtual java.math.BigDecimal getBigDecimal(int arg_0) {
			return Wrapped.getBigDecimal (arg_0);
		}

		public virtual java.math.BigDecimal getBigDecimal(string arg_0) {
			return Wrapped.getBigDecimal (arg_0);
		}

		public virtual int getFetchSize() {
			return Wrapped.getFetchSize ();
		}

		public virtual string getString(int arg_0) {
			return Wrapped.getString (arg_0);
		}

		public virtual string getString(string arg_0) {
			return Wrapped.getString (arg_0);
		}

		public virtual bool absolute(int arg_0) {
			return Wrapped.absolute (arg_0);
		}

		public virtual bool last() {
			return Wrapped.last ();
		}

		public virtual int getFetchDirection() {
			return Wrapped.getFetchDirection ();
		}

		public virtual java.sql.Time getTime(int arg_0) {
			return Wrapped.getTime (arg_0);
		}

		public virtual java.sql.Time getTime(string arg_0) {
			return Wrapped.getTime (arg_0);
		}

		public virtual java.sql.Time getTime(int arg_0, java.util.Calendar arg_1) {
			return Wrapped.getTime (arg_0, arg_1);
		}

		public virtual java.sql.Time getTime(string arg_0, java.util.Calendar arg_1) {
			return Wrapped.getTime (arg_0, arg_1);
		}

		public virtual void close() {
			Wrapped.close ();
		}

		public virtual void deleteRow() {
			Wrapped.deleteRow ();
		}

		public virtual void updateObject(int arg_0, object arg_1, int arg_2) {
			Wrapped.updateObject (arg_0, arg_1, arg_2);
		}

		public virtual void updateObject(int arg_0, object arg_1) {
			Wrapped.updateObject (arg_0, arg_1);
		}

		public virtual void updateObject(string arg_0, object arg_1, int arg_2) {
			Wrapped.updateObject (arg_0, arg_1, arg_2);
		}

		public virtual void updateObject(string arg_0, object arg_1) {
			Wrapped.updateObject (arg_0, arg_1);
		}

		public virtual void updateString(int arg_0, string arg_1) {
			Wrapped.updateString (arg_0, arg_1);
		}

		public virtual void updateString(string arg_0, string arg_1) {
			Wrapped.updateString (arg_0, arg_1);
		}

		public virtual java.sql.Date getDate(int arg_0) {
			return Wrapped.getDate (arg_0);
		}

		public virtual java.sql.Date getDate(string arg_0) {
			return Wrapped.getDate (arg_0);
		}

		public virtual java.sql.Date getDate(int arg_0, java.util.Calendar arg_1) {
			return Wrapped.getDate (arg_0, arg_1);
		}

		public virtual java.sql.Date getDate(string arg_0, java.util.Calendar arg_1) {
			return Wrapped.getDate (arg_0, arg_1);
		}

		public virtual java.sql.Clob getClob(int arg_0) {
			return Wrapped.getClob (arg_0);
		}

		public virtual java.sql.Clob getClob(string arg_0) {
			return Wrapped.getClob (arg_0);
		}

		public virtual void moveToInsertRow() {
			Wrapped.moveToInsertRow ();
		}

		public virtual void updateAsciiStream(int arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.updateAsciiStream (arg_0, arg_1, arg_2);
		}

		public virtual void updateAsciiStream(string arg_0, java.io.InputStream arg_1, int arg_2) {
			Wrapped.updateAsciiStream (arg_0, arg_1, arg_2);
		}

		public virtual void updateRef(int arg_0, java.sql.Ref arg_1) {
			Wrapped.updateRef (arg_0, arg_1);
		}

		public virtual void updateRef(string arg_0, java.sql.Ref arg_1) {
			Wrapped.updateRef (arg_0, arg_1);
		}

		public virtual int findColumn(string arg_0) {
			return Wrapped.findColumn (arg_0);
		}

		public virtual bool rowInserted() {
			return Wrapped.rowInserted ();
		}

		public virtual void updateBlob(int arg_0, java.sql.Blob arg_1) {
			Wrapped.updateBlob (arg_0, arg_1);
		}

		public virtual void updateBlob(string arg_0, java.sql.Blob arg_1) {
			Wrapped.updateBlob (arg_0, arg_1);
		}

		public virtual java.sql.Array getArray(int arg_0) {
			return Wrapped.getArray (arg_0);
		}

		public virtual java.sql.Array getArray(string arg_0) {
			return Wrapped.getArray (arg_0);
		}

		public virtual void updateDouble(int arg_0, double arg_1) {
			Wrapped.updateDouble (arg_0, arg_1);
		}

		public virtual void updateDouble(string arg_0, double arg_1)
		{
			Wrapped.updateDouble (arg_0, arg_1);
		}

		public virtual void updateInt(int arg_0, int arg_1) {
			Wrapped.updateInt (arg_0, arg_1);
		}

		public virtual void updateInt(string arg_0, int arg_1) {
			Wrapped.updateInt (arg_0, arg_1);
		}

		public virtual long getLong(int arg_0) {
			return Wrapped.getLong (arg_0);
		}

		public virtual long getLong(string arg_0) {
			return Wrapped.getLong (arg_0);
		}

		public virtual void updateDate(int arg_0, java.sql.Date arg_1) {
			Wrapped.updateDate (arg_0, arg_1);
		}

		public virtual void updateDate(string arg_0, java.sql.Date arg_1) {
			Wrapped.updateDate (arg_0, arg_1);
		}

		public virtual void beforeFirst() {
			Wrapped.beforeFirst ();
		}

		public virtual bool wasNull() {
			return Wrapped.wasNull ();
		}

		public virtual void updateClob(int arg_0, java.sql.Clob arg_1) {
			Wrapped.updateClob (arg_0, arg_1);
		}

		public virtual void updateClob(string arg_0, java.sql.Clob arg_1) {
			Wrapped.updateClob (arg_0, arg_1);
		}

		public virtual java.sql.Statement getStatement() {
			return Wrapped.getStatement ();
		}

		public virtual void setFetchDirection(int arg_0) {
			Wrapped.setFetchDirection (arg_0);
		}

		public virtual void cancelRowUpdates() {
			Wrapped.cancelRowUpdates ();
		}

		public virtual double getDouble(int arg_0) {
			return Wrapped.getDouble (arg_0);
		}

		public virtual double getDouble(string arg_0) {
			return Wrapped.getDouble (arg_0);
		}

		public virtual void updateBytes(int arg_0, sbyte[] arg_1) {
			Wrapped.updateBytes (arg_0, arg_1);
		}

		public virtual void updateBytes(string arg_0, sbyte[] arg_1) {
			Wrapped.updateBytes (arg_0, arg_1);
		}

		public virtual bool previous() {
			return Wrapped.previous ();
		}

		public virtual float getFloat(int arg_0) {
			return Wrapped.getFloat (arg_0);
		}

		public virtual float getFloat(string arg_0) {
			return Wrapped.getFloat (arg_0);
		}

		public virtual java.io.InputStream getAsciiStream(int arg_0) {
			return Wrapped.getAsciiStream (arg_0);
		}

		public virtual java.io.InputStream getAsciiStream(string arg_0) {
			return Wrapped.getAsciiStream (arg_0);
		}

		public virtual short getShort(int arg_0) {
			return Wrapped.getShort (arg_0);
		}

		public virtual short getShort(string arg_0) {
			return Wrapped.getShort (arg_0);
		}

		public virtual java.io.Reader getCharacterStream(int arg_0) {
			return Wrapped.getCharacterStream (arg_0);
		}

		public virtual java.io.Reader getCharacterStream(string arg_0) {
			return Wrapped.getCharacterStream (arg_0);
		}

		public virtual int getConcurrency() {
			return Wrapped.getConcurrency ();
		}

		public virtual java.sql.ResultSetMetaData getMetaData() {
			return Wrapped.getMetaData ();
		}

		public virtual bool getBoolean(int arg_0) {
			return Wrapped.getBoolean (arg_0);
		}

		public virtual bool getBoolean(string arg_0) {
			return Wrapped.getBoolean (arg_0);
		}

		public virtual void updateNull(int arg_0) {
			Wrapped.updateNull (arg_0);
		}

		public virtual void updateNull(string arg_0) {
			Wrapped.updateNull (arg_0);
		}

		public void updateArray(int arg_0, java.sql.Array arg_1) {
			Wrapped.updateArray (arg_0, arg_1);
		}

		public void updateArray(string arg_0, java.sql.Array arg_1) {
			Wrapped.updateArray (arg_0, arg_1);
		}

		public virtual void updateLong(int arg_0, long arg_1) {
			Wrapped.updateLong (arg_0, arg_1);
		}

		public virtual void updateLong(string arg_0, long arg_1) {
			Wrapped.updateLong (arg_0, arg_1);
		}

		public virtual java.sql.SQLWarning getWarnings() {
			return Wrapped.getWarnings ();
			
		}

		public virtual java.sql.Ref getRef(int arg_0) {
			return Wrapped.getRef (arg_0);
		}

		public virtual java.sql.Ref getRef(string arg_0) {
			return Wrapped.getRef (arg_0);
		}

		public virtual void updateTimestamp(int arg_0, java.sql.Timestamp arg_1) {
			Wrapped.updateTimestamp (arg_0, arg_1);
		}

		public virtual void updateTimestamp(string arg_0, java.sql.Timestamp arg_1) {
			Wrapped.updateTimestamp (arg_0, arg_1);
		}

		public virtual bool rowUpdated() {
			return Wrapped.rowUpdated ();
		}

		public virtual string getCursorName() {
			return Wrapped.getCursorName ();
		}

		public virtual java.sql.Blob getBlob(int arg_0) {
			return Wrapped.getBlob (arg_0);
		}

		public virtual java.sql.Blob getBlob(string arg_0) {
			return Wrapped.getBlob (arg_0);
		}

		public virtual void afterLast() {
			Wrapped.afterLast ();
		}

		public virtual int getRow() {
			return Wrapped.getRow ();
		}

		public virtual void refreshRow() {
			Wrapped.refreshRow ();
		}

		public virtual void updateShort(int arg_0, short arg_1) {
			Wrapped.updateShort (arg_0, arg_1);
		}

		public virtual void updateShort(string arg_0, short arg_1) {
			Wrapped.updateShort (arg_0, arg_1);
		}

		public virtual int getType() {
			return Wrapped.getType ();
		}

		public virtual void clearWarnings() {
			Wrapped.clearWarnings ();
		}

		public virtual void setFetchSize(int arg_0) {
			Wrapped.setFetchSize (arg_0);
		}

		#endregion
	}

	public class ResultSetMetaData : java.sql.ResultSetMetaData
	{
		readonly java.sql.ResultSetMetaData _resultSetMetaData;

		public ResultSetMetaData (java.sql.ResultSetMetaData resultSetMetaData) {
			_resultSetMetaData = resultSetMetaData;
		}

		protected java.sql.ResultSetMetaData Wrapped {
			get { return _resultSetMetaData; }
		}

		#region ResultSetMetaData Members

		public virtual int getColumnCount() {
			return Wrapped.getColumnCount ();
		}

		public virtual bool isReadOnly(int arg_0) {
			return Wrapped.isReadOnly (arg_0);
		}

		public virtual string getSchemaName(int arg_0) {
			return Wrapped.getSchemaName (arg_0);
		}

		public virtual int getColumnDisplaySize(int arg_0) {
			return Wrapped.getColumnDisplaySize (arg_0);
		}

		public virtual int getColumnType(int arg_0) {
			return Wrapped.getColumnType (arg_0);
		}

		public virtual bool isSigned(int arg_0) {
			return Wrapped.isSigned (arg_0);
		}

		public virtual int getScale(int arg_0) {
			return Wrapped.getScale (arg_0);
		}

		public virtual int getPrecision(int arg_0) {
			return Wrapped.getPrecision (arg_0);
		}

		public virtual bool isCaseSensitive(int arg_0) {
			return Wrapped.isCaseSensitive (arg_0);
		}

		public virtual string getTableName(int arg_0) {
			return Wrapped.getTableName (arg_0);
		}

		public virtual string getColumnName(int arg_0) {
			return Wrapped.getColumnName (arg_0);
		}

		public virtual bool isDefinitelyWritable(int arg_0) {
			return Wrapped.isDefinitelyWritable (arg_0);
		}

		public virtual bool isSearchable(int arg_0) {
			return Wrapped.isSearchable (arg_0);
		}

		public virtual string getCatalogName(int arg_0) {
			return Wrapped.getCatalogName (arg_0);
		}

		public virtual string getColumnTypeName(int arg_0) {
			return Wrapped.getColumnTypeName (arg_0);
		}

		public virtual int isNullable(int arg_0) {
			return Wrapped.isNullable (arg_0);
		}

		public virtual string getColumnClassName(int arg_0) {
			return Wrapped.getColumnClassName (arg_0);
		}

		public virtual string getColumnLabel(int arg_0) {
			return Wrapped.getColumnLabel (arg_0);
		}

		public virtual bool isAutoIncrement(int arg_0) {
			return Wrapped.isAutoIncrement (arg_0);
		}

		public virtual bool isCurrency(int arg_0) {
			return Wrapped.isCurrency (arg_0);
		}

		public virtual bool isWritable(int arg_0) {
			return Wrapped.isWritable (arg_0);
		}

		#endregion 
	}
	
		public class DatabaseMetaData : java.sql.DatabaseMetaData
	{
		readonly java.sql.DatabaseMetaData _databaseMetaData;

		public DatabaseMetaData (java.sql.DatabaseMetaData databaseMetaData) {
			_databaseMetaData = databaseMetaData;
		}

		protected java.sql.DatabaseMetaData Wrapped {
			get { return _databaseMetaData; }
		}

		#region DatabaseMetaData Members

		public virtual bool nullsAreSortedLow()
		{
			return Wrapped.nullsAreSortedLow ();
		}

		public virtual bool supportsSchemasInPrivilegeDefinitions()
		{
			return Wrapped.supportsSchemasInPrivilegeDefinitions ();
		}

		public virtual string getSearchStringEscape()
		{
			return Wrapped.getSearchStringEscape ();
		}

		public virtual bool supportsGroupByUnrelated()
		{
			return Wrapped.supportsGroupByUnrelated ();
		}

		public virtual bool supportsSchemasInDataManipulation()
		{
			return Wrapped.supportsSchemasInDataManipulation ();
		}

		public virtual bool supportsTransactions()
		{
			return Wrapped.supportsTransactions ();
		}

		public virtual bool supportsSchemasInProcedureCalls()
		{
			return Wrapped.supportsSchemasInProcedureCalls ();
		}

		public virtual java.sql.ResultSet getImportedKeys(string arg_0, string arg_1, string arg_2)
		{
			return Wrapped.getImportedKeys (arg_0, arg_1, arg_2);
		}

		public virtual bool supportsTransactionIsolationLevel(int arg_0)
		{
			return Wrapped.supportsTransactionIsolationLevel (arg_0);
		}

		public virtual bool supportsResultSetConcurrency(int arg_0, int arg_1)
		{
			return Wrapped.supportsResultSetConcurrency (arg_0, arg_1);
		}

		public virtual bool supportsSubqueriesInQuantifieds()
		{
			return Wrapped.supportsSubqueriesInQuantifieds ();
		}

		public virtual bool othersInsertsAreVisible(int arg_0)
		{
			return Wrapped.othersInsertsAreVisible (arg_0);
		}

		public virtual bool supportsSchemasInTableDefinitions()
		{
			return Wrapped.supportsSchemasInTableDefinitions ();
		}

		public virtual bool supportsGetGeneratedKeys()
		{
			return Wrapped.supportsGetGeneratedKeys ();
		}

		public virtual bool supportsConvert()
		{
			return Wrapped.supportsConvert ();
		}

		bool java.sql.DatabaseMetaData.supportsConvert(int arg_0, int arg_1)
		{
			return Wrapped.supportsConvert (arg_0, arg_1);
		}

		public virtual bool deletesAreDetected(int arg_0)
		{
			return Wrapped.deletesAreDetected (arg_0);
		}

		public virtual bool supportsExtendedSQLGrammar()
		{
			return Wrapped.supportsExtendedSQLGrammar ();
		}

		public virtual string getStringFunctions()
		{
			return Wrapped.getStringFunctions ();
		}

		public virtual java.sql.ResultSet getExportedKeys(string arg_0, string arg_1, string arg_2)
		{
			return Wrapped.getExportedKeys (arg_0, arg_1, arg_2);
		}

		public virtual bool supportsDataDefinitionAndDataManipulationTransactions()
		{
			return Wrapped.supportsDataDefinitionAndDataManipulationTransactions ();
		}

		public virtual bool supportsOrderByUnrelated()
		{
			return Wrapped.supportsOrderByUnrelated ();
		}

		public virtual bool supportsGroupByBeyondSelect()
		{
			return Wrapped.supportsGroupByBeyondSelect ();
		}

		public virtual string getDatabaseProductName()
		{
			return Wrapped.getDatabaseProductName ();
		}

		public virtual int getMaxStatementLength()
		{
			return Wrapped.getMaxStatementLength ();
		}

		public virtual java.sql.ResultSet getTypeInfo()
		{
			return Wrapped.getTypeInfo ();
		}

		public virtual bool supportsSubqueriesInIns()
		{
			return Wrapped.supportsSubqueriesInIns ();
		}

		public virtual string getSystemFunctions()
		{
			return Wrapped.getSystemFunctions ();
		}

		public virtual bool nullPlusNonNullIsNull()
		{
			return Wrapped.nullPlusNonNullIsNull ();
		}

		public virtual bool usesLocalFilePerTable()
		{
			return Wrapped.usesLocalFilePerTable ();
		}

		public virtual int getDatabaseMinorVersion()
		{
			return Wrapped.getDatabaseMinorVersion ();
		}

		public virtual bool supportsLikeEscapeClause()
		{
			return Wrapped.supportsLikeEscapeClause ();
		}

		public virtual int getMaxColumnNameLength()
		{
			return Wrapped.getMaxColumnNameLength ();
		}

		public virtual bool supportsSubqueriesInComparisons()
		{
			return Wrapped.supportsSubqueriesInComparisons ();
		}

		public virtual bool isReadOnly()
		{
			return Wrapped.isReadOnly ();
		}

		public virtual java.sql.Connection getConnection()
		{
			return Wrapped.getConnection ();
		}

		public virtual bool supportsPositionedDelete()
		{
			return Wrapped.supportsPositionedDelete ();
		}

		public virtual bool doesMaxRowSizeIncludeBlobs()
		{
			return Wrapped.doesMaxRowSizeIncludeBlobs ();
		}

		public virtual bool supportsAlterTableWithDropColumn()
		{
			return Wrapped.supportsAlterTableWithDropColumn ();
		}

		public virtual bool supportsFullOuterJoins()
		{
			return Wrapped.supportsFullOuterJoins ();
		}

		public virtual int getDatabaseMajorVersion()
		{
			return Wrapped.getDatabaseMajorVersion ();
		}

		public virtual bool supportsCorrelatedSubqueries()
		{
			return Wrapped.supportsCorrelatedSubqueries ();
		}

		public virtual bool supportsOpenStatementsAcrossCommit()
		{
			return Wrapped.supportsOpenStatementsAcrossCommit ();
		}

		public virtual bool supportsMixedCaseIdentifiers()
		{
			return Wrapped.supportsMixedCaseIdentifiers ();
		}

		public virtual string getDatabaseProductVersion()
		{
			return Wrapped.getDatabaseProductVersion ();
		}

		public virtual bool supportsUnion()
		{
			return Wrapped.supportsUnion ();
		}

		public virtual bool supportsResultSetHoldability(int arg_0)
		{
			return Wrapped.supportsResultSetHoldability (arg_0);
		}

		public virtual string getSQLKeywords()
		{
			return Wrapped.getSQLKeywords ();
		}

		public virtual bool nullsAreSortedAtEnd()
		{
			return Wrapped.nullsAreSortedAtEnd ();
		}

		public virtual bool locatorsUpdateCopy()
		{
			return Wrapped.locatorsUpdateCopy ();
		}

		public virtual bool supportsColumnAliasing()
		{
			return Wrapped.supportsColumnAliasing ();
		}

		public virtual bool supportsCatalogsInPrivilegeDefinitions()
		{
			return Wrapped.supportsCatalogsInPrivilegeDefinitions ();
		}

		public virtual bool supportsSchemasInIndexDefinitions()
		{
			return Wrapped.supportsSchemasInIndexDefinitions ();
		}

		public virtual string getDriverVersion()
		{
			return Wrapped.getDriverVersion ();
		}

		public virtual int getDriverMajorVersion()
		{
			return Wrapped.getDriverMajorVersion ();
		}

		public virtual java.sql.ResultSet getSchemas()
		{
			return Wrapped.getSchemas ();
		}

		public virtual bool supportsSelectForUpdate()
		{
			return Wrapped.supportsSelectForUpdate ();
		}

		public virtual int getResultSetHoldability()
		{
			return Wrapped.getResultSetHoldability ();
		}

		public virtual java.sql.ResultSet getAttributes(string arg_0, string arg_1, string arg_2, string arg_3)
		{
			return Wrapped.getAttributes (arg_0, arg_1, arg_2, arg_3);
		}

		public virtual bool supportsSavepoints()
		{
			return Wrapped.supportsSavepoints ();
		}

		public virtual string getUserName()
		{
			return Wrapped.getUserName ();
		}

		public virtual string getExtraNameCharacters()
		{
			return Wrapped.getExtraNameCharacters ();
		}

		public virtual bool insertsAreDetected(int arg_0)
		{
			return Wrapped.insertsAreDetected (arg_0);
		}

		public virtual java.sql.ResultSet getCatalogs()
		{
			return Wrapped.getCatalogs ();
		}

		public virtual bool nullsAreSortedAtStart()
		{
			return Wrapped.nullsAreSortedAtStart ();
		}

		public virtual bool supportsOpenCursorsAcrossRollback()
		{
			return Wrapped.supportsOpenCursorsAcrossRollback ();
		}

		public virtual int getMaxColumnsInIndex()
		{
			return Wrapped.getMaxColumnsInIndex ();
		}

		public virtual int getMaxColumnsInGroupBy()
		{
			return Wrapped.getMaxColumnsInGroupBy ();
		}

		public virtual bool supportsBatchUpdates()
		{
			return Wrapped.supportsBatchUpdates ();
		}

		public virtual java.sql.ResultSet getCrossReference(string arg_0, string arg_1, string arg_2, string arg_3, string arg_4, string arg_5)
		{
			return Wrapped.getCrossReference (arg_0, arg_1, arg_2, arg_3, arg_4, arg_5);
		}

		public virtual string getCatalogTerm()
		{
			return Wrapped.getCatalogTerm ();
		}

		public virtual int getMaxStatements()
		{
			return Wrapped.getMaxStatements ();
		}

		public virtual string getCatalogSeparator()
		{
			return Wrapped.getCatalogSeparator ();
		}

		public virtual int getMaxBinaryLiteralLength()
		{
			return Wrapped.getMaxBinaryLiteralLength ();
		}

		public virtual bool supportsOuterJoins()
		{
			return Wrapped.supportsOuterJoins ();
		}

		public virtual int getSQLStateType()
		{
			return Wrapped.getSQLStateType ();
		}

		public virtual bool dataDefinitionIgnoredInTransactions()
		{
			return Wrapped.dataDefinitionIgnoredInTransactions ();
		}

		public virtual int getMaxColumnsInSelect()
		{
			return Wrapped.getMaxColumnsInSelect ();
		}

		public virtual bool supportsIntegrityEnhancementFacility()
		{
			return Wrapped.supportsIntegrityEnhancementFacility ();
		}

		public virtual string getTimeDateFunctions()
		{
			return Wrapped.getTimeDateFunctions ();
		}

		public virtual bool ownDeletesAreVisible(int arg_0)
		{
			return Wrapped.ownDeletesAreVisible (arg_0);
		}

		public virtual java.sql.ResultSet getProcedures(string arg_0, string arg_1, string arg_2)
		{
			return Wrapped.getProcedures (arg_0, arg_1, arg_2);
		}

		public virtual string getURL()
		{
			return Wrapped.getURL ();
		}

		public virtual bool supportsStoredProcedures()
		{
			return Wrapped.supportsStoredProcedures ();
		}

		public virtual bool usesLocalFiles()
		{
			return Wrapped.usesLocalFiles ();
		}

		public virtual java.sql.ResultSet getTableTypes()
		{
			return Wrapped.getTableTypes ();
			
		}

		public virtual bool dataDefinitionCausesTransactionCommit()
		{
			return Wrapped.dataDefinitionCausesTransactionCommit ();
		}

		public virtual bool supportsGroupBy()
		{
			return Wrapped.supportsGroupBy ();
		}

		public virtual bool supportsExpressionsInOrderBy()
		{
			return Wrapped.supportsExpressionsInOrderBy ();
		}

		public virtual bool isCatalogAtStart()
		{
			return Wrapped.isCatalogAtStart ();
		}

		public virtual bool storesUpperCaseQuotedIdentifiers()
		{
			return Wrapped.storesUpperCaseQuotedIdentifiers ();
		}

		public virtual int getMaxRowSize()
		{
			return Wrapped.getMaxRowSize ();
		}

		public virtual bool supportsSubqueriesInExists()
		{
			return Wrapped.supportsSubqueriesInExists ();
		}

		public virtual bool supportsCatalogsInIndexDefinitions()
		{
			return Wrapped.supportsCatalogsInIndexDefinitions ();
		}

		public virtual bool supportsANSI92EntryLevelSQL()
		{
			return Wrapped.supportsANSI92EntryLevelSQL ();
		}

		public virtual bool storesMixedCaseQuotedIdentifiers()
		{
			return Wrapped.storesMixedCaseQuotedIdentifiers ();
		}

		public virtual bool supportsMultipleTransactions()
		{
			return Wrapped.supportsMultipleTransactions ();
		}

		public virtual bool othersUpdatesAreVisible(int arg_0)
		{
			return Wrapped.othersUpdatesAreVisible (arg_0);
		}

		public virtual java.sql.ResultSet getProcedureColumns(string arg_0, string arg_1, string arg_2, string arg_3)
		{
			return Wrapped.getProcedureColumns (arg_0, arg_1, arg_2, arg_3);
		}

		public virtual int getMaxProcedureNameLength()
		{
			return Wrapped.getMaxProcedureNameLength ();
		}

		public virtual java.sql.ResultSet getSuperTables(string arg_0, string arg_1, string arg_2)
		{
			return Wrapped.getSuperTables (arg_0, arg_1, arg_2);
		}

		public virtual int getJDBCMajorVersion()
		{
			return Wrapped.getJDBCMajorVersion ();
		}

		public virtual bool supportsPositionedUpdate()
		{
			return Wrapped.supportsPositionedUpdate ();
		}

		public virtual bool supportsLimitedOuterJoins()
		{
			return Wrapped.supportsLimitedOuterJoins ();
		}

		public virtual bool allTablesAreSelectable()
		{
			return Wrapped.allTablesAreSelectable ();
		}

		public virtual java.sql.ResultSet getSuperTypes(string arg_0, string arg_1, string arg_2)
		{
			return Wrapped.getSuperTypes (arg_0, arg_1, arg_2);
		}

		public virtual int getMaxTablesInSelect()
		{
			return Wrapped.getMaxTablesInSelect ();
		}

		public virtual string getNumericFunctions()
		{
			return Wrapped.getNumericFunctions ();
		}

		public virtual bool ownInsertsAreVisible(int arg_0)
		{
			return Wrapped.ownInsertsAreVisible (arg_0);
		}

		public virtual bool supportsResultSetType(int arg_0)
		{
			return Wrapped.supportsResultSetType (arg_0);
		}

		public virtual java.sql.ResultSet getPrimaryKeys(string arg_0, string arg_1, string arg_2)
		{
			return Wrapped.getPrimaryKeys (arg_0, arg_1, arg_2);
		}

		public virtual bool nullsAreSortedHigh()
		{
			return Wrapped.nullsAreSortedHigh ();
		}

		public virtual bool supportsAlterTableWithAddColumn()
		{
			return Wrapped.supportsAlterTableWithAddColumn ();
		}

		public virtual bool supportsDataManipulationTransactionsOnly()
		{
			return Wrapped.supportsDataManipulationTransactionsOnly ();
		}

		public virtual java.sql.ResultSet getUDTs(string arg_0, string arg_1, string arg_2, int[] arg_3)
		{
			return Wrapped.getUDTs (arg_0, arg_1, arg_2, arg_3);
		}

		public virtual int getMaxConnections()
		{
			return Wrapped.getMaxConnections ();
		}

		public virtual bool supportsNonNullableColumns()
		{
			return Wrapped.supportsNonNullableColumns ();
		}

		public virtual bool supportsTableCorrelationNames()
		{
			return Wrapped.supportsTableCorrelationNames ();
		}

		public virtual java.sql.ResultSet getVersionColumns(string arg_0, string arg_1, string arg_2)
		{
			return Wrapped.getVersionColumns (arg_0, arg_1, arg_2);
		}

		public virtual bool supportsOpenStatementsAcrossRollback()
		{
			return Wrapped.supportsOpenStatementsAcrossRollback ();
		}

		public virtual bool supportsMultipleResultSets()
		{
			return Wrapped.supportsMultipleResultSets ();
		}

		public virtual string getDriverName()
		{
			return Wrapped.getDriverName ();
		}

		public virtual bool storesLowerCaseQuotedIdentifiers()
		{
			return Wrapped.storesLowerCaseQuotedIdentifiers ();
		}

		public virtual java.sql.ResultSet getBestRowIdentifier(string arg_0, string arg_1, string arg_2, int arg_3, bool arg_4)
		{
			return Wrapped.getBestRowIdentifier (arg_0, arg_1, arg_2, arg_3, arg_4);
		}

		public virtual bool supportsOpenCursorsAcrossCommit()
		{
			return Wrapped.supportsOpenCursorsAcrossCommit ();
		}

		public virtual int getMaxCursorNameLength()
		{
			return Wrapped.getMaxCursorNameLength ();
		}

		public virtual int getMaxColumnsInOrderBy()
		{
			return Wrapped.getMaxColumnsInOrderBy ();
		}

		public virtual java.sql.ResultSet getColumns(string arg_0, string arg_1, string arg_2, string arg_3)
		{
			return Wrapped.getColumns (arg_0, arg_1, arg_2, arg_3);
		}

		public virtual bool supportsStatementPooling()
		{
			return Wrapped.supportsStatementPooling ();
		}

		public virtual bool storesMixedCaseIdentifiers()
		{
			return Wrapped.storesMixedCaseIdentifiers ();
		}

		public virtual int getMaxCharLiteralLength()
		{
			return Wrapped.getMaxCharLiteralLength ();
		}

		public virtual bool supportsUnionAll()
		{
			return Wrapped.supportsUnionAll ();
		}

		public virtual string getProcedureTerm()
		{
			return Wrapped.getProcedureTerm ();
		}

		public virtual bool storesLowerCaseIdentifiers()
		{
			return Wrapped.storesLowerCaseIdentifiers ();
		}

		public virtual bool supportsMixedCaseQuotedIdentifiers()
		{
			return Wrapped.supportsMixedCaseQuotedIdentifiers ();
		}

		public virtual int getDefaultTransactionIsolation()
		{
			return Wrapped.getDefaultTransactionIsolation ();
		}

		public virtual bool ownUpdatesAreVisible(int arg_0)
		{
			return Wrapped.ownUpdatesAreVisible (arg_0);
		}

		public virtual bool supportsNamedParameters()
		{
			return Wrapped.supportsNamedParameters ();
		}

		public virtual int getMaxColumnsInTable()
		{
			return Wrapped.getMaxColumnsInTable ();
		}

		public virtual bool supportsMinimumSQLGrammar()
		{
			return Wrapped.supportsMinimumSQLGrammar ();
		}

		public virtual bool supportsDifferentTableCorrelationNames()
		{
			return Wrapped.supportsDifferentTableCorrelationNames ();
		}

		public virtual java.sql.ResultSet getTables(string arg_0, string arg_1, string arg_2, string[] arg_3)
		{
			return Wrapped.getTables (arg_0, arg_1, arg_2, arg_3);
		}

		public virtual int getMaxIndexLength()
		{
			return Wrapped.getMaxIndexLength ();
		}

		public virtual int getMaxSchemaNameLength()
		{
			return Wrapped.getMaxSchemaNameLength ();
		}

		public virtual int getMaxCatalogNameLength()
		{
			return Wrapped.getMaxCatalogNameLength ();
		}

		public virtual int getMaxTableNameLength()
		{
			return Wrapped.getMaxTableNameLength ();
		}

		public virtual bool supportsCoreSQLGrammar()
		{
			return Wrapped.supportsCoreSQLGrammar ();
		}

		public virtual bool othersDeletesAreVisible(int arg_0)
		{
			return Wrapped.othersDeletesAreVisible (arg_0);
		}

		public virtual bool allProceduresAreCallable()
		{
			return Wrapped.allProceduresAreCallable ();
		}

		public virtual int getDriverMinorVersion()
		{
			return Wrapped.getDriverMinorVersion ();
		}

		public virtual java.sql.ResultSet getColumnPrivileges(string arg_0, string arg_1, string arg_2, string arg_3)
		{
			return Wrapped.getColumnPrivileges (arg_0, arg_1, arg_2, arg_3);
		}

		public virtual bool updatesAreDetected(int arg_0)
		{
			return Wrapped.updatesAreDetected (arg_0);
		}

		public virtual string getSchemaTerm()
		{
			return Wrapped.getSchemaTerm ();
		}

		public virtual bool supportsANSI92FullSQL()
		{
			return Wrapped.supportsANSI92FullSQL ();
		}

		public virtual java.sql.ResultSet getTablePrivileges(string arg_0, string arg_1, string arg_2)
		{
			return Wrapped.getTablePrivileges (arg_0, arg_1, arg_2);
		}

		public virtual bool supportsANSI92IntermediateSQL()
		{
			return Wrapped.supportsANSI92IntermediateSQL ();
		}

		public virtual string getIdentifierQuoteString()
		{
			return Wrapped.getIdentifierQuoteString ();
		}

		public virtual int getMaxUserNameLength()
		{
			return Wrapped.getMaxUserNameLength ();
		}

		public virtual bool supportsCatalogsInProcedureCalls()
		{
			return Wrapped.supportsCatalogsInProcedureCalls ();
		}

		public virtual bool supportsCatalogsInDataManipulation()
		{
			return Wrapped.supportsCatalogsInDataManipulation ();
		}

		public virtual bool storesUpperCaseIdentifiers()
		{
			return Wrapped.storesUpperCaseIdentifiers ();
		}

		public virtual int getJDBCMinorVersion()
		{
			return Wrapped.getJDBCMinorVersion ();
		}

		public virtual java.sql.ResultSet getIndexInfo(string arg_0, string arg_1, string arg_2, bool arg_3, bool arg_4)
		{
			return Wrapped.getIndexInfo (arg_0, arg_1, arg_2, arg_3, arg_4);
		}

		public virtual bool supportsMultipleOpenResults()
		{
			return Wrapped.supportsMultipleOpenResults ();
		}

		public virtual bool supportsCatalogsInTableDefinitions()
		{
			return Wrapped.supportsCatalogsInTableDefinitions ();
		}

		#endregion
	}

}