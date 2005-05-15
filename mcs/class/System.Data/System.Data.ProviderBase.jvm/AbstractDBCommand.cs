//namespace System.Data.Common
//{
//
//	using java.sql;
//	using java.util;
//
//	using System;
//	using System.Collections;
//	using System.Data;
//	using System.Data.ProviderBase;
//
//	/**
//	 * @author erand
//	 */
//	public abstract class AbstractDBCommand : DbCommandBase
//	{
//		protected String _cmdText, _javaCmdText;
//		protected AbstractDBConnection _connection;
//		protected IDbTransaction _transaction;
//		protected int _cmdTimeout = 30;
//		protected CommandType _cmdType = CommandType.Text;
//		protected UpdateRowSource _updateRowSource = UpdateRowSource.Both;
//		protected Statement _statement;
//		protected IDataParameterCollection _paramCollection;
//		protected IDataReader _reader;
//		private bool _isCommandTextChanged;
//		private CommandBehavior _behvior = CommandBehavior.Default;
//
//		private static readonly int _oracleRefCursor;
//		protected const int oracleTypeRefCursor = 1111; // Types.OTHER
//        
//        
//		static AbstractDBCommand()
//		{
//			try {
//				java.lang.Class OracleTypesClass = java.lang.Class.forName("oracle.jdbc.OracleTypes");
//				_oracleRefCursor = OracleTypesClass.getField("CURSOR").getInt(null);
//			}
//			catch(java.lang.ClassNotFoundException e) {
//				// oracle driver is not in classpath - just continue
//			}
//		}
//
//		public AbstractDBCommand(
//			String cmdText,
//			AbstractDBConnection connection,
//			IDbTransaction transaction)
//		{
//			_connection = connection;
//			_cmdText = cmdText;
//			_transaction = transaction;
//			_isCommandTextChanged = true;
//		}
//    
//		public java.sql.Statement Statement
//		{
//			get
//			{
//				return _statement;
//			}
//			set
//			{
//				_statement = value;
//			}
//		}
//
//		public virtual String JavaCommandText
//		{
//			get
//			{
//				return _javaCmdText;
//			}
//			set
//			{
//				_javaCmdText = value;
//			}
//		}
//
//		protected bool getCommandTextChanged()
//		{
//			return _isCommandTextChanged;
//		}
//    
//		internal void setCommandTextChanged(bool value)
//		{
//			_isCommandTextChanged = value;
//		}
//    
//		public Statement getStatement()
//		{
//			return _statement;
//		}
//		/**
//		 * @see System.Data.IDbCommand#Cancel()
//		 */
//		public override void Cancel()
//		{
//			try
//			{
//				if (_statement != null)
//					_statement.cancel();
//			}
//			catch (SQLException exp)
//			{
//				Console.WriteLine(exp);
//			}
//		}
//
////		/**
////		 * @see System.Data.IDbCommand#ExecuteScalar()
////		 */
////		public virtual Object ExecuteScalar()
////		{
////			Object result = null;
////			IDataReader reader = ((IDbCommand)this).ExecuteReader();
////
////			if (reader != null && reader.Read())
////			{
////				result = ((IDataRecord) reader).GetValue(0);
////			}
////			reader.Close();
////
////			return result;
////		}
//
////		/**
////		 * @see System.Data.IDbCommand#CommandTimeout
////		 */
////		public virtual int CommandTimeout
////		{
////			get
////			{
////				return _cmdTimeout;
////			}
////			set
////			{
////				_cmdTimeout = value;
////			}
////		}
//
////		/**
////		 * @see System.Data.IDbCommand#CommandType
////		 */
////		public virtual CommandType CommandType
////		{
////			get
////			{
////				return _cmdType;
////			}
////			set
////			{
////				_cmdType = value;
////			}
////		}
//
////		/**
////		 * @see System.Data.IDbCommand#Connection
////		 */
////		public virtual IDbConnection Connection
////		{
////			get
////			{
////				return (IDbConnection)_connection;
////			}
////			set
////			{
////				_connection = (AbstractDBConnection) value;
////			}
////		}
//
////		/**
////		 * @see System.Data.IDbCommand#Transaction
////		 */
////		public virtual IDbTransaction Transaction
////		{
////			get
////			{
////				return _transaction;
////			}
////			set
////			{
////				_transaction = value;
////			}
////		}
//
////		/**
////		 * @see System.Data.IDbCommand#UpdatedRowSource
////		 */
////		public virtual UpdateRowSource UpdatedRowSource
////		{
////			get
////			{
////				return _updateRowSource;
////			}
////			set
////			{
////				_updateRowSource = value;
////			}
////		}
//
////		/**
////		 * @see System.Data.IDbCommand#CommandText
////		 */
////		public virtual String CommandText
////		{
////			get
////			{
////				return _cmdText;
////			}
////			set
////			{
////				if (_cmdText == null || String.Compare(_cmdText, value,  true) != 0)
////				{
////					_cmdText = value;
////					_isCommandTextChanged = true;
////				}
////			}
////		}
//
//		protected virtual void setInputForStatement(
//			PreparedStatement stm,
//			int javaType,
//			int place,
//			Object value)
//			//throws SQLException
//		{
//			// by the type of the the parameter we know wich method of
//			// the statement to use
//
//			if (value is DBNull)
//				stm.setNull(place, javaType);
//
//			else if (javaType == Types.BINARY)
//				stm.setBytes(place, vmw.common.TypeUtils.ToSByteArray((byte[]) value));
//
//			else if (javaType == Types.BLOB)
//				stm.setObject(place, value);
//
//			else if (javaType == Types.CLOB)
//				stm.setObject(place, value);
//
//			else if (javaType == Types.DISTINCT)
//				stm.setObject(place, value);
//
//			else if (javaType == Types.ARRAY)
//				stm.setObject(place, value);
//
//			else if (javaType == Types.JAVA_OBJECT)
//				stm.setObject(place, value);
//
//			else if (javaType == Types.REF)
//				stm.setObject(place, value);
//
//			else if (javaType == Types.STRUCT)
//				stm.setObject(place, value);
//
//			else if (javaType == Types.VARBINARY)
//				stm.setBytes(place, vmw.common.TypeUtils.ToSByteArray((byte[]) value));
//
//			else if (javaType == Types.BIGINT)
//				stm.setLong(place, ((java.lang.Long) value).longValue());
//
//			else if (javaType == Types.BIT)
//				stm.setBoolean(place, ((java.lang.Boolean) value).booleanValue());
//
//			else if (javaType == Types.CHAR)
//				stm.setString(place, ((java.lang.Character) value).toString());
//
//			else if (javaType == Types.DATE)
//			{
//				DateTime tmp = (DateTime) value;
//				Calendar c = vmw.common.DateTimeUtils.DateTimeToCalendar(tmp);
//				stm.setDate(
//					place, new java.sql.Date(c.getTime().getTime()));
//			}
//			else if (javaType == Types.DECIMAL || javaType == Types.NUMERIC)
//				stm.setBigDecimal(place, vmw.common.PrimitiveTypeUtils.DecimalToBigDecimal((Decimal) value));
//
//			else if (javaType == Types.DOUBLE)
//				stm.setDouble(place, ((java.lang.Double) value).doubleValue());
//
//			else if (javaType == Types.FLOAT)
//				stm.setFloat(place, ((java.lang.Float) value).floatValue());
//
//			else if (javaType == Types.INTEGER)
//				stm.setInt(place, ((java.lang.Integer) value).intValue());
//
//			else if (javaType == Types.LONGVARCHAR)
//				stm.setString(place, (String) value);
//
//			else if (javaType == Types.LONGVARBINARY)
//				stm.setBytes(place, vmw.common.TypeUtils.ToSByteArray((byte[]) value));
//
//			else if (javaType == Types.REAL)
//				stm.setFloat(place, ((java.lang.Float) value).floatValue());
//
//			else if (javaType == Types.SMALLINT)
//				stm.setShort(place, ((java.lang.Short) value).shortValue());
//
//			else if (javaType == Types.TIME)
//			{
//				Calendar c = vmw.common.DateTimeUtils.DateTimeToCalendar(value);
//				stm.setTime(
//					place,
//					new java.sql.Time(c.getTime().getTime()));
//			}
//			else if (javaType == Types.TIMESTAMP)
//			{
//				DateTime tmp = (DateTime) value;
//				Calendar c = vmw.common.DateTimeUtils.DateTimeToCalendar(value);
//				stm.setTimestamp(
//					place,
//					new java.sql.Timestamp(c.getTime().getTime()));
//			}
//
//			else if (javaType == Types.TINYINT)
//				stm.setByte(place, ((java.lang.Byte) value).byteValue());
//
//			else if (javaType == Types.VARCHAR)
//				stm.setString(place, (String) value);
//
//			else
//				stm.setObject(place, value, javaType);
//
//		}
//
//		// create the _javaCmdText from the _cmdText (the .NET text)
//		protected String createCommandForStoreProcedure(String text)
//		{
//			IDbDataParameter param;
//			bool isFirst = true;
//			IList paramCollection = (IList) ((IDbCommand)this).Parameters;
//        
//			//in .Net the name of sore procedure can be wraped by '[' and ']'
//			// so we remove them.
//			int indx1 = text.IndexOf('[');
//			if (indx1 != -1)
//			{
//				int indx2 = text.IndexOf(']', indx1);
//				if (indx2 != -1)
//					text = text.Substring(indx1 + 1, indx2 - (indx1 + 1));
//			}
//
//			java.lang.StringBuffer sb = new java.lang.StringBuffer(text);
//			sb.insert(0, "{call ");
//
//			for (int i = 0; i < paramCollection.Count; i++)
//			{
//				param = (IDbDataParameter) paramCollection[i];
//
//				if (param.Direction == ParameterDirection.ReturnValue)
//					sb = sb.insert(1, "? =");
//				else if (isFirst)
//				{
//					sb = sb.append("(?");
//					isFirst = false;
//				}
//				else
//					sb = sb.append(",?");
//			}
//
//			String retVal = sb.toString();
//
//			if (retVal.IndexOf("(") != -1)
//				retVal = retVal + ")";
//
//			retVal += "}";
//
//			_javaCmdText = retVal;
//
//			return retVal;
//
//		}
//
//		// prepare the input and output parameters for the jdbc prepared statement
//		// from the SqlParameters of this instance
//		internal virtual void prepareStatementForStoreProcedure()// throws SQLException
//		{
//			CallableStatement stm = (CallableStatement) _statement;
//			AbstractDBParameter param;
//			IList paramCollection = (IList) ((IDbCommand)this).Parameters;
//			ParameterDirection direction;
//			//int place;
//
//			// NOTE - we add 1 to the index because in .NET it is zero base
//			// and in jdbc it is one base
//			for (int i = 0; i < paramCollection.Count; i++)
//			{
//				param = (AbstractDBParameter) paramCollection[i];
//				direction = param.Direction;
//
//				if (direction == ParameterDirection.Input)
//					setInput(stm, i + 1, param);
//				else if (direction == ParameterDirection.Output)
//				{
//					setOutput(stm, i + 1, param);
//					param.setParameterPlace(i + 1);
//				}
//				else if (direction == ParameterDirection.InputOutput)
//				{
//					setInput(stm, i + 1, param);
//					setOutput(stm, i + 1, param);
//					param.setParameterPlace(i + 1);
//				}
//				else if (direction == ParameterDirection.ReturnValue)
//				{
//					setOutput(stm, 1, param);
//					param.setParameterPlace(1);
//				}
//			}
//
//		}
//
//		// set input parameter for the statement
//		protected void setInput(
//			PreparedStatement stm,
//			int place,
//			AbstractDbParameter param)
//			//throws SQLException
//		{
//			int javaType = param.JdbcType;
//			Object value = param.Value;
//
//			value = getJavaWrapperFromCSharp(value);
//
//			setInputForStatement(stm, javaType, place, value);
//		}
//        
//		public static Object getJavaWrapperFromCSharp(Object obj)
//		{
//			if (obj is bool)
//				return new java.lang.Boolean((bool)obj);
//			else if (obj is byte)
//				return new java.lang.Byte((sbyte)obj);
//			else if (obj is char)
//				return new java.lang.Character((char) obj);
//			else if (obj is short)
//				return new java.lang.Short((short)obj);
//			else if (obj is int)
//				return new java.lang.Integer((int)obj);
//			else if (obj is long)
//				return new java.lang.Long((long)obj);
//			else if (obj is float)
//				return new java.lang.Float((float)obj);
//			else if (obj is double)
//				return new java.lang.Double((double)obj);
//
//			return obj;
//
//		}
//
//		// set an output parameter to the statement.
//		protected void setOutput(
//			CallableStatement stm,
//			int place,
//			AbstractDbParameter param)
//			//throws SQLException
//		{
//			int javaType = param.JdbcType;
//
//			// the scale has a meening only in DECIMAL parameters
//			if (javaType == Types.DECIMAL || javaType == Types.NUMERIC)
//			{
//				if(param.DbType == DbType.Currency)
//					stm.registerOutParameter(place, javaType, 4);
//				else
//					stm.registerOutParameter(place, javaType, param.Scale);
//			}
//			else if (javaType == oracleTypeRefCursor) {
//				stm.registerOutParameter(place, _oracleRefCursor);
//			}
//			else {
//				stm.registerOutParameter(place, javaType);
//			}
//
//		}
//
//		// get the the output parameter for a specific statement at a specific place
//		// returns the value of the output parameter
//		protected Object getOutputValue(
//			AbstractDbParameter param,
//			CallableStatement stm)
//			//throws SQLException
//		{
//			Object retVal = null;
//
//			int place = param.getParameterPlace();
//
//			// by the type of the parameter we know wich method to use
//			// on the statement
//			int type = param.JdbcType;
//
//			if (type == Types.ARRAY)
//				retVal = stm.getObject(place);
//			else if (type == Types.BIGINT)
//				retVal = stm.getLong(place);
//			else if (type == Types.BINARY)
//				retVal = stm.getBytes(place);
//			else if (type == Types.BIT)
//				retVal = stm.getBoolean(place);
//			else if (type == Types.BLOB)
//				retVal = stm.getObject(place);
//			else if (type == Types.CHAR)
//				retVal = stm.getString(place);
//			else if (type == Types.CLOB)
//				retVal = stm.getObject(place);
//			else if (type == Types.DATE)
//			{
//				java.sql.Date date = stm.getDate(place);
//				if(date != null)
//				{
//					// convertirn sql.Date to DateTime
//					Calendar c = Calendar.getInstance();
//					c.setTime(date);
//
//					retVal = vmw.common.DateTimeUtils.CalendarToDateTime(c);
//				}
//				
//			}
//			else if (type == Types.DECIMAL)
//			{
//				java.math.BigDecimal bd = stm.getBigDecimal(place);
//				if(bd != null)
//					retVal = vmw.common.PrimitiveTypeUtils.BigDecimalToDecimal(bd);
//			}
//			else if (type == Types.DISTINCT)
//				retVal = stm.getObject(place);
//			else if (type == Types.DOUBLE)
//				retVal = stm.getDouble(place);
//			else if (type == Types.FLOAT)
//				retVal = stm.getFloat(place);
//			else if (type == Types.INTEGER)
//				retVal = stm.getInt(place);
//			else if (type == Types.JAVA_OBJECT)
//				retVal = stm.getObject(place);
//			else if (type == Types.LONGVARBINARY)
//				retVal = stm.getBytes(place);
//			else if (type == Types.LONGVARCHAR)
//				retVal = stm.getString(place);
//			else if (type == Types.NULL)
//				retVal = DBNull.Value;
//			else if (type == Types.NUMERIC)
//				retVal = stm.getBigDecimal(place);
//			else if (type == Types.OTHER)
//				retVal = stm.getObject(place);
//			else if (type == Types.REAL)
//				retVal = stm.getFloat(place);
//			else if (type == Types.REF)
//				retVal = stm.getObject(place);
//			else if (type == Types.SMALLINT)
//				retVal = stm.getShort(place);
//			else if (type == Types.STRUCT)
//				retVal = stm.getObject(place);
//			else if (type == Types.TIME)
//			{
//				Time t = stm.getTime(place);
//				if(t != null)
//				{
//					java.util.Date d = new java.util.Date(t.getTime());
//					// convertirn sql.Date to DateTime
//					Calendar c = Calendar.getInstance();
//					c.setTime(d);
//
//					retVal = vmw.common.DateTimeUtils.CalendarToDateTime(c);
//				}
//				
//			}
//			else if (type == Types.TIMESTAMP)
//			{
//				Timestamp ts = stm.getTimestamp(place);
//				if(ts != null)
//				{
//					java.util.Calendar cal = java.util.Calendar.getInstance();
//					cal.setTime(new java.util.Date(ts.getTime() + ts.getNanos()/1000000));
//					retVal = (DateTime)vmw.common.DateTimeUtils.CalendarToDateTime(cal);
//				}
//			}
//			else if (type == Types.TINYINT)
//				retVal = stm.getByte(place);
//			else if (type == Types.VARBINARY)
//				retVal = stm.getBytes(place);
//			else if (type == Types.VARCHAR)
//				retVal = stm.getString(place);
//        
//			if(stm.wasNull())
//				retVal = DBNull.Value;
//            
//			return retVal;
//		}
//
//    
////		IDbDataParameter IDbCommand.CreateParameter()
////		{
////			return null;
////		}
//    
////		IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
////		{
////			return null;
////		}
////
////		IDataReader IDbCommand.ExecuteReader()
////		{
////			return null;
////		}
////
////		int IDbCommand.ExecuteNonQuery()
////		{
////			return -1;
////		}
////
////		void IDbCommand.Prepare()
////		{
////		}
//
//        
////		public IDataParameterCollection Parameters
////		{
////			get
////			{
////				return _paramCollection;
////			}
////		}
//
//		~AbstractDBCommand()
//		{
//			Dispose();
//		}
//
//		public new void Dispose()
//		{
//			if (_paramCollection != null)
//				_paramCollection.Clear();
//
//			_connection = null;
//			_transaction = null;
//
//			if (_statement != null)
//			{
//				_statement.close();
//				_statement = null;
//			}
//			base.Dispose();
//		}
//
//		protected void CheckTrasactionContext()
//		{
//			if (_connection.Transaction != null && 
//				(Transaction == null || Transaction != _connection.Transaction)) {
//				throw new InvalidOperationException("Execute requires the command to have a transaction object" + 
//							" when the connection assigned to the command is in a pending local transaction." +
//							" The Transaction property of the command has not been initialized.");
//			}
//		}
//
//		protected String buildJavaCommand(String command)
//		{
//			IDataParameterCollection parameters = Parameters;
//			
//			if (parameters != null && parameters.Count > 0) {
//				string tokens = command;
//				System.Text.StringBuilder sb = new System.Text.StringBuilder(tokens.Length);
//
//				int currParameter = 0;
//				int currStart = 0;
//				int curr = 0;
//				char curSeparator = (char)0;
//				bool foundSeparator = false;
//
//				for(;curr<tokens.Length;curr++) {
//					switch(tokens[curr]) {
//						case '"':
//						case '\'':
//							if (foundSeparator) {
//								if (curSeparator == tokens[curr]) {
//									foundSeparator = false;
//								}
//							}
//							else {
//								// start inner string
//								foundSeparator = true;
//								curSeparator = tokens[curr];
//							}
//							break;
//						case '?':
//							if (!foundSeparator) {
//								if (curr > currStart) { 
//									// copy collected 
//									sb.Append(tokens,currStart,curr - currStart);
//								}
//								// append parameter value
//								AbstractDBParameter param = (AbstractDBParameter)parameters[currParameter++];
//								sb.Append(param.formatParameter());
//								currStart = curr+1;
//							}
//							break;
//					}
//				}
//
//				if (curr > currStart) { // end of the stirng
//					sb.Append(tokens,currStart,curr - currStart);
//				}
//
//				command = sb.ToString();
//			}
//			return command;
//		}
//
//		internal void fillParameters()
//		{
//			if(CommandType == CommandType.StoredProcedure)
//			{
//				AbstractDBParameter param;
//				CallableStatement stm = (CallableStatement)_statement;
//				// get the output parameters from the statement
//				// and put their values into the SqlParameter
//				for (int i = 0; i < _paramCollection.Count; i++)
//				{
//					param = (AbstractDBParameter)_paramCollection[i];
//
//					if (param.IsOracleRefCursor)
//						continue;
//
//					ParameterDirection direction = param.Direction;
//					if (direction == ParameterDirection.InputOutput
//						|| direction == ParameterDirection.Output
//						|| direction == ParameterDirection.ReturnValue)
//						param.Value = getOutputValue(param, stm);
//				}
//			}
//		}
//
//		protected void setCommandBehabior(CommandBehavior cmdBehavior)
//		{
//			_behvior = cmdBehavior;
//		}
//
//		internal CommandBehavior getCommandBehavior()
//		{
//			return _behvior;
//		}
//
//		protected static String GetProcedureName(string cmdText)
//		{
//			if(cmdText[0] == '[' && cmdText[cmdText.Length - 1] == ']')
//				return cmdText.Substring(1, cmdText.Length - 2);
//			return cmdText;
//		}
//        
//	}
//}