//
// System.Data.Common.AbstractDataReader
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//   Konstantin Triger (kostat@mainsoft.com)
//

using System;
using System.Data;
using System.Collections;
using System.Data.ProviderBase;

using java.io;
using java.sql;

namespace System.Data.Common
{
	public abstract class AbstractDataReader : DbDataReaderBase, ISafeDataRecord {
		#region Fields

		private ResultSetMetaData _resultsMetaData;
		protected AbstractDbCommand _command;
		private DataTable _schemaTable;
		private ReaderState _readerState = ReaderState.Uninitialized;

		private IReaderCacheContainer[] _readerCache;
		private int _currentCacheFilledPosition; 
		private Stack _resultSetStack = new Stack();

		[Flags]
		private enum ReaderState { Uninitialized = 0, Empty = 1, HasRows = 2, FirstRed = 4, Eof = 8, Fetching = 16 };

		internal enum SCHEMA_TABLE { ColumnName,
			ColumnOrdinal,
			ColumnSize,
			NumericPrecision,
			NumericScale,
			IsUnique,
			IsKey,
			BaseServerName,
			BaseCatalogName,
			BaseColumnName,
			BaseSchemaName,
			BaseTableName,
			DataType,
			AllowDBNull,
			ProviderType,
			IsAliased,
			IsExpression,
			IsIdentity,
			IsAutoIncrement,
			IsRowVersion,
			IsHidden,
			IsLong,
			IsReadOnly};

		#endregion // Fields

		#region Constructors

		protected AbstractDataReader() : base (CommandBehavior.Default) {
		}
		
		public AbstractDataReader(AbstractDbCommand command): base(command.Behavior) {
			_command = command;
			if (_command.Connection != null) {
				((AbstractDBConnection)_command.Connection).AddReference(this);
			}
		}

		#endregion // Constructors

		#region Properties

		public override bool HasRows {
			get {
				if (IsClosed) {
					throw new InvalidOperationException("Invalid attempt to HasRows when reader is closed.");
				}

				try {
					if(null == Results)
						return false;
				}
				catch(SystemException) {
					//suppress
					return false;
				}

				return (_readerState & ReaderState.HasRows) != 0;
			}
		}

		public override int RecordsAffected
		{
			// MSDN : The RecordsAffected property is not set 
			// until all rows are read and you close the reader.
			get { 
				return _command.RecordsAffected; 
			}
		}

		public override int FieldCount
		{
			get {
				if (ResultsMetaData == null)
					return 0;

				try {
					return ResultsMetaData.getColumnCount();
				}
				catch (SQLException exp) {
					throw CreateException(exp);
				}

			}
		}

		protected internal CommandBehavior Behavior
		{
			get {
				return _command.Behavior;
			}
		}

		public override Object this[String columnName]
		{
			get {
				try {
					int columnIndex = Results.findColumn(columnName) - 1;
					return this[columnIndex];
				}
				catch (SQLException exp) {
					throw new IndexOutOfRangeException(exp.Message, exp);
				}				
			}
		}

		public override Object this[int columnIndex]
		{
			get { return GetValue(columnIndex); }
		}

		protected ResultSet Results
		{
			get {
				if (_readerState == ReaderState.Uninitialized) {

					if (_resultSetStack.Count == 0) {
						ResultSet resultSet =  _command.CurrentResultSet;
						if (resultSet == null)
							return null;

						_resultSetStack.Push(resultSet);
					}

					_readerState = ReaderState.Fetching;
					for (;;) {
						try {
							Configuration.BooleanSetting prefetchSchema = Configuration.Switches.PrefetchSchema;

							if (prefetchSchema == Configuration.BooleanSetting.NotSet) {
								AbstractDBConnection conn = (AbstractDBConnection)((ICloneable)_command.Connection);
								string driverName = conn.JdbcConnection.getMetaData().getDriverName();
								if (driverName.IndexOf("DB2") >= 0)
									prefetchSchema = Configuration.BooleanSetting.True;
							}

							if (prefetchSchema == Configuration.BooleanSetting.True)
								GetSchemaTable();

							ResultSet resultSet = (ResultSet)_resultSetStack.Peek();
							if (resultSet.next()) {
								_readerState = (ReaderState.HasRows | ReaderState.FirstRed);
								ResultSetMetaData rsMetaData = ResultsMetaData;
								DbTypes.JavaSqlTypes javaSqlType = (DbTypes.JavaSqlTypes)rsMetaData.getColumnType(1);
								if (javaSqlType == DbTypes.JavaSqlTypes.OTHER) {
									object value = GetValue(0);
									if (value != null && value is ResultSet) {
										_resultsMetaData = null;
										_readerCache = null;
										SchemaTable = null;
										_readerState = ReaderState.Fetching;
										_resultSetStack.Push(value);
										continue;
									}
								}
							}
							else
								_readerState = ReaderState.Empty;

							break;
						}
						catch(SQLException e) {
							throw CreateException(e);
						}
					}
				}

				return (_resultSetStack.Count > 0) ? (ResultSet)_resultSetStack.Peek() : null;
			}
		}

		protected ResultSetMetaData ResultsMetaData
		{
			get {
				ResultSet results = Results;
				if (results == null) {
					return null;
				}
				if(_resultsMetaData == null) {
					_resultsMetaData = results.getMetaData();
				}
				return _resultsMetaData;
			}			
		}

		protected DataTable SchemaTable
		{
			get {
				if (_schemaTable == null) {
					_schemaTable = ConstructSchemaTable();
				}
				return _schemaTable;
			}

			set {_schemaTable = value; }
		}

		internal protected IReaderCacheContainer[] ReaderCache
		{
			get {
				if (_readerCache == null) {
					_readerCache = CreateReaderCache();
					_currentCacheFilledPosition = -1;
				}
				return _readerCache;
			}
		}

		#endregion // Properties

		#region Methods

		protected abstract int GetProviderType(int jdbcType);

		protected abstract SystemException CreateException(string message, SQLException e);

		protected abstract SystemException CreateException(IOException e);

		protected SystemException CreateException(SQLException e)
		{
			return CreateException(e.Message,e);	
		}

		private bool CloseCurrentResultSet() {
			if (_resultSetStack.Count > 0) {
				try{
					_resultsMetaData = null;
					_readerCache = null;
					_readerState = ReaderState.Uninitialized;
					ResultSet rs = (ResultSet)_resultSetStack.Pop();
					rs.close();
					return true;
				}
				catch (SQLException exp) {
					throw CreateException(exp);
				}
			}

			return false;
		}

		// FIXME : add Close(bool readAllRecords) and pass this bool to skip looping over NextResult(), override AbstractDbCommand.ExecuteScalar
		public override void Close()
		{
			if (IsClosed)
				return;

			try {
				CloseCurrentResultSet();
				_command.OnReaderClosed(this);
			}
			finally {
				CloseInternal();
			}
		}

		internal void CloseInternal()
		{
			_resultsMetaData = null;
			_readerCache = null;
			_isClosed = true;
		}

		public override bool NextResult()
		{
			CloseCurrentResultSet();

			if ((_command.Behavior & CommandBehavior.SingleResult) != 0) {
				while (CloseCurrentResultSet());
				while (_command.NextResultSet());
				return false;
			}

			try {
				while (_resultSetStack.Count > 0) {
					ResultSet rs = (ResultSet)_resultSetStack.Peek();

					if(!rs.next()) {
						CloseCurrentResultSet();
						continue;
					}

					// must be a ResultSet
					object childRs = rs.getObject(1);
					if (childRs != null) {
						SchemaTable = null;
						_resultSetStack.Push(childRs);
						return true;
					}
				}
			}
			catch (SQLException exp) {
				throw CreateException(exp);
			}
				
			if (_command.NextResultSet()) {
				SchemaTable = null;	
				return true;
			}
			return false;
		}

		public override bool Read()
		{
			if(null == Results ||
				(_readerState & (ReaderState.HasRows | ReaderState.Eof)) != ReaderState.HasRows)
				return false;

			bool firstRead = false;

			try {
				if ((_readerState & ReaderState.FirstRed) != 0) {
					firstRead = true;
					_readerState &= ~ReaderState.FirstRed;
					return true;
				}
				else {
					bool next = Results.next();

					if (!next)
						_readerState |= ReaderState.Eof;

					return next;
				}
			}			
			catch (SQLException exp) {
				// suppress exception as .Net does
				return false;
			}
			finally {
				// in case of first read we could sampled the first value
				// to see whether there is a resultset, so _currentCacheFilledPosition
				// might be already inited
				if (!firstRead)
					_currentCacheFilledPosition = -1;
			}
		}

		public override bool GetBoolean(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((BooleanReaderCacheContainer)ReaderCache[columnIndex]).GetBoolean();
		}

		public bool GetBooleanSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is BooleanReaderCacheContainer) {
				return GetBoolean(columnIndex);
			}
			else {
				return Convert.ToBoolean(GetValue(columnIndex));
			}
		}

		public override byte GetByte(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((ByteReaderCacheContainer)ReaderCache[columnIndex]).GetByte();
		}

		public byte GetByteSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is ByteReaderCacheContainer) {
				return GetByte(columnIndex);
			}
			else {
				return Convert.ToByte(GetValue(columnIndex));
			}
		}

		public override long GetBytes(
			int columnIndex,
			long dataIndex,
			byte[] buffer,
			int bufferIndex,
			int length)
		{
			FillReaderCache(columnIndex);
			byte[] byteArr = ((BytesReaderCacheContainer)ReaderCache[columnIndex]).GetBytes();
			long actualLength = ((dataIndex + length) >= byteArr.Length) ? (byteArr.Length - dataIndex) : length;
			Array.Copy(byteArr,dataIndex,buffer,bufferIndex,actualLength);
			return actualLength;
		}

		public virtual byte[] GetBytes(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((BytesReaderCacheContainer)ReaderCache[columnIndex]).GetBytes();
		}

		public override char GetChar(int columnIndex)
		{
			FillReaderCache(columnIndex);
			string s = ((StringReaderCacheContainer)ReaderCache[columnIndex]).GetString();
			if(s == null) {
				return '\0';
			}
			return s[0];
		}

		public char GetCharSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is StringReaderCacheContainer) {
				return GetChar(columnIndex);
			}
			else {
				return Convert.ToChar(GetValue(columnIndex));
			}
		}

		public override long GetChars(
			int columnIndex,
			long dataIndex,
			char[] buffer,
			int bufferIndex,
			int length)
		{
			FillReaderCache(columnIndex);
			char[] charArr = ((CharsReaderCacheContainer)ReaderCache[columnIndex]).GetChars();
			long actualLength = ((dataIndex + length) >= charArr.Length) ? (charArr.Length - dataIndex) : length;
			Array.Copy(charArr,dataIndex,buffer,bufferIndex,actualLength);
			return actualLength;
		}

		public override string GetDataTypeName(int columnIndex)
		{
			try {
				if (ResultsMetaData == null) {
					return String.Empty;
				}
				return ResultsMetaData.getColumnTypeName(columnIndex + 1);
			}
			catch (SQLException exp) {
				throw CreateException(exp);
			}
		}

		public override DateTime GetDateTime(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((DateTimeReaderCacheContainer)ReaderCache[columnIndex]).GetDateTime();
		}

		public virtual TimeSpan GetTimeSpan(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((TimeSpanReaderCacheContainer)ReaderCache[columnIndex]).GetTimeSpan();
		}

		public override Guid GetGuid(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((GuidReaderCacheContainer)ReaderCache[columnIndex]).GetGuid();
		}

		public override decimal GetDecimal(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((DecimalReaderCacheContainer)ReaderCache[columnIndex]).GetDecimal();
		}

		public decimal GetDecimalSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is DecimalReaderCacheContainer) {
				return GetDecimal(columnIndex);
			}
			else {
				return Convert.ToDecimal(GetValue(columnIndex));
			}
		}

		public override double GetDouble(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((DoubleReaderCacheContainer)ReaderCache[columnIndex]).GetDouble();
		}

		public double GetDoubleSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is DoubleReaderCacheContainer) {
				return GetDouble(columnIndex);
			}
			else {
				return Convert.ToDouble(GetValue(columnIndex));
			}
		}

		public override float GetFloat(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((FloatReaderCacheContainer)ReaderCache[columnIndex]).GetFloat();
		}

		public float GetFloatSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is FloatReaderCacheContainer) {
				return GetFloat(columnIndex);
			}
			else {
				return Convert.ToSingle(GetValue(columnIndex));
			}
		}

		public override short GetInt16(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((Int16ReaderCacheContainer)ReaderCache[columnIndex]).GetInt16();
		}

		public short GetInt16Safe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is Int16ReaderCacheContainer) {
				return GetInt16(columnIndex);
			}
			else {
				return Convert.ToInt16(GetValue(columnIndex));
			}
		}

		public override int GetInt32(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((Int32ReaderCacheContainer)ReaderCache[columnIndex]).GetInt32();
		}

		public int GetInt32Safe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is Int32ReaderCacheContainer) {
				return GetInt32(columnIndex);
			}
			else {
				return Convert.ToInt32(GetValue(columnIndex));
			}
		}

		public override long GetInt64(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((Int64ReaderCacheContainer)ReaderCache[columnIndex]).GetInt64();
		}

		public long GetInt64Safe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is Int64ReaderCacheContainer) {
				return GetInt64(columnIndex);
			}
			else {
				return Convert.ToInt64(GetValue(columnIndex));
			}
		}

		public override string GetName(int columnIndex)
		{
			try {
				if (ResultsMetaData == null) {
					return String.Empty;
				}
				return ResultsMetaData.getColumnName(columnIndex + 1);
			}
			catch (SQLException exp) {
				throw new IndexOutOfRangeException(exp.Message, exp);
			}
		}

		public override int GetOrdinal(String columnName)
		{
			try {
				int retVal = Results.findColumn(columnName);
				if(retVal != -1) {
					retVal -= 1;
				}
				return  retVal;
			}
			catch (SQLException exp) {
				throw new IndexOutOfRangeException(exp.Message, exp);
			}
		}

		public override string GetString(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((StringReaderCacheContainer)ReaderCache[columnIndex]).GetString();
		}

		public string GetStringSafe(int columnIndex) {
			if (ReaderCache[columnIndex] is StringReaderCacheContainer) {
				return GetString(columnIndex);
			}
			else {
				return Convert.ToString(GetValue(columnIndex));
			}
		}

		public override object GetValue(int columnIndex)
		{
			FillReaderCache(columnIndex);
			if (ReaderCache[columnIndex].IsNull()) {
				return DBNull.Value;
			}
			return ReaderCache[columnIndex].GetValue();
		}

		public override int GetValues(Object[] values)
		{	
			int columnCount = FieldCount;
			int i = 0;
			for (; i < values.Length && i < columnCount; i++) {
				values[i] = GetValue(i);
			}
			return i;
		}

		private void FillReaderCache(int columnIndex)
		{
			try {
				IReaderCacheContainer[] readerCache = ReaderCache;
				if ((Behavior & CommandBehavior.SequentialAccess) == 0) {					
					while (_currentCacheFilledPosition < columnIndex) {
						_currentCacheFilledPosition++;
						readerCache[_currentCacheFilledPosition].Fetch(Results,_currentCacheFilledPosition);
					}					
				}
				else {
					readerCache[columnIndex].Fetch(Results,columnIndex);
				}
			}
			catch(SQLException e) {
				_currentCacheFilledPosition = -1;
				throw CreateException(e);
			}
			catch (IOException e) {
				_currentCacheFilledPosition = -1;
				throw CreateException(e);
			}
		}

		private IReaderCacheContainer[] CreateReaderCache()
		{
			try {
				IReaderCacheContainer[] readerCache = new IReaderCacheContainer[FieldCount];
				for(int i=0; i < readerCache.Length; i++) {
					DbTypes.JavaSqlTypes javaSqlType = (DbTypes.JavaSqlTypes) ResultsMetaData.getColumnType(i + 1);
					switch (javaSqlType) {
						case DbTypes.JavaSqlTypes.ARRAY :
							readerCache[i] = new ArrayReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.BIGINT :
							readerCache[i] = new Int64ReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.BINARY :
						case DbTypes.JavaSqlTypes.VARBINARY :
						case DbTypes.JavaSqlTypes.LONGVARBINARY :
							readerCache[i] = new BytesReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.BIT :
							readerCache[i] = new BooleanReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.BLOB :
							readerCache[i] = new BlobReaderCacheContainer();
							break;	
						case DbTypes.JavaSqlTypes.CHAR :						
							if ("uniqueidentifier".Equals(ResultsMetaData.getColumnTypeName(i + 1))) {
								readerCache[i] = new GuidReaderCacheContainer();
							}
							else {
								readerCache[i] = new StringReaderCacheContainer();
							}
							break;
						case DbTypes.JavaSqlTypes.CLOB :
							readerCache[i] = new ClobReaderCacheContainer();
							break;		
						case DbTypes.JavaSqlTypes.TIME :
							readerCache[i] = new TimeSpanReaderCacheContainer();
							break;	
						case DbTypes.JavaSqlTypes.DATE :
							AbstractDBConnection conn = (AbstractDBConnection)((ICloneable)_command.Connection);
							string driverName = conn.JdbcConnection.getMetaData().getDriverName();

							if (driverName.StartsWith("PostgreSQL")) {
								readerCache[i] = new DateTimeReaderCacheContainer();
								break;
							}
							else
								goto case DbTypes.JavaSqlTypes.TIMESTAMP;
						case DbTypes.JavaSqlTypes.TIMESTAMP :				
							readerCache[i] = new TimestampReaderCacheContainer();
							break;		
						case DbTypes.JavaSqlTypes.DECIMAL :
						case DbTypes.JavaSqlTypes.NUMERIC :
							// jdbc driver for oracle identitfies both FLOAT and NUMBEr columns as 
							// java.sql.Types.NUMERIC (2), columnTypeName NUMBER, columnClassName java.math.BigDecimal 
							// therefore we relay on scale
							int scale = ResultsMetaData.getScale(i + 1);
							if (scale == -127) {
								// Oracle db type FLOAT
								readerCache[i] = new DoubleReaderCacheContainer();
							}
							else {
								readerCache[i] = new DecimalReaderCacheContainer();
							}
							break;		
						case DbTypes.JavaSqlTypes.DOUBLE :
						case DbTypes.JavaSqlTypes.FLOAT :
							readerCache[i] = new DoubleReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.INTEGER :
							readerCache[i] = new Int32ReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.LONGVARCHAR :
						case DbTypes.JavaSqlTypes.VARCHAR :
							readerCache[i] = new StringReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.NULL :
							readerCache[i] = new NullReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.REAL :
							readerCache[i] = new FloatReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.REF :
							readerCache[i] = new RefReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.SMALLINT :
							readerCache[i] = new Int16ReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.TINYINT :
							readerCache[i] = new ByteReaderCacheContainer();
							break;
						case DbTypes.JavaSqlTypes.DISTINCT :
						case DbTypes.JavaSqlTypes.JAVA_OBJECT :
						case DbTypes.JavaSqlTypes.OTHER :
						case DbTypes.JavaSqlTypes.STRUCT :
						default :
							readerCache[i] = new ObjectReaderCacheContainer();
							break;
					}
					//				((ReaderCacheContainerBase)readerCache[i])._jdbcType = (int) javaSqlType;
				}

				return readerCache;
			}
			catch(SQLException e) {
				throw CreateException(e);
			}
		}

		public override bool IsDBNull(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ReaderCache[columnIndex].IsNull();
		}

		public override Type GetFieldType(int i)
		{
			try {
				int javaSqlType = ResultsMetaData.getColumnType(i + 1);
				return DbConvert.JavaSqlTypeToClrType(javaSqlType);
			}
			catch (SQLException exp) {
				throw new IndexOutOfRangeException(exp.Message, exp);
			}
		}

		public IDataReader GetData(int i)
		{
			throw new NotSupportedException();
		}

		public override DataTable GetSchemaTable()
		{
			if (SchemaTable.Rows != null && SchemaTable.Rows.Count > 0) {
				return SchemaTable;
			}
            
			ResultSetMetaData metaData;			
			if (Behavior == CommandBehavior.SchemaOnly) {
				try {
					metaData = ((PreparedStatement)_command.JdbcStatement).getMetaData();
				}
				catch(SQLException e) {
					throw CreateException("CommandBehaviour.SchemaOnly is not supported by the JDBC driver.",e);
				}
			}
			else {
				metaData = ResultsMetaData;
			}

			if (metaData == null) {
				return SchemaTable;
			}

			DatabaseMetaData dbMetaData = null;
			AbstractDBConnection clonedConnection = null;
			if ((_command.Behavior & CommandBehavior.KeyInfo) != 0) {
				clonedConnection = (AbstractDBConnection)((ICloneable)_command.Connection).Clone();

				try {
					clonedConnection.Open();
					dbMetaData = clonedConnection.JdbcConnection.getMetaData();
				}
				catch {
					//suppress
					if (clonedConnection != null) {
						clonedConnection.Close();
					}
				}			
			}
			
			try {
				int tmp;				
				for(int i = 1; i <= metaData.getColumnCount(); i++) {
					DataRow row = SchemaTable.NewRow ();
					string columnName = metaData.getColumnLabel(i);
					string baseColumnName = metaData.getColumnName(i);
	
					row [(int)SCHEMA_TABLE.ColumnName] = columnName; // maybe we should use metaData.getColumnLabel(i);
					row [(int)SCHEMA_TABLE.ColumnSize] = metaData.getColumnDisplaySize(i);
					row [(int)SCHEMA_TABLE.ColumnOrdinal]		= i - 1;
					try {
						// FIXME : workaround for Oracle JDBC driver bug
						// getPrecision on BLOB, CLOB, NCLOB throws NumberFormatException
						tmp = metaData.getPrecision(i);
					}
					catch(java.lang.NumberFormatException e) {
						// supress exception
						tmp = 255;
					}
					row [(int)SCHEMA_TABLE.NumericPrecision] = Convert.ToInt16(tmp > 255 ? 255 : tmp);
					tmp = metaData.getScale(i);
					row [(int)SCHEMA_TABLE.NumericScale] = Convert.ToInt16(tmp > 255 ? 255 : tmp);

					row [(int)SCHEMA_TABLE.BaseServerName] = DBNull.Value;
				
					string catalog = null;
					try {
						catalog = metaData.getCatalogName(i);
					}
					catch (Exception e) {
						// supress exception
					}
					if (catalog != null && catalog.Length == 0)
						catalog =  ((AbstractDBConnection)_command.Connection).JdbcConnection.getCatalog();
					row [(int)SCHEMA_TABLE.BaseCatalogName] = catalog;
					row [(int)SCHEMA_TABLE.BaseColumnName] = baseColumnName;

					string schemaName;
					string tableName;

					try {
						tableName = metaData.getTableName(i);
					}
					catch {
						tableName = null;
					}

					try {
						schemaName = metaData.getSchemaName(i);
					}
					catch {
						schemaName = null;
					}

					if (tableName != null && tableName.Length == 0)
						tableName = null;
					if (schemaName != null && schemaName.Length == 0)
						schemaName = null;

					row [(int)SCHEMA_TABLE.BaseSchemaName] = schemaName;
					row [(int)SCHEMA_TABLE.BaseTableName] = tableName;


					row [(int)SCHEMA_TABLE.AllowDBNull] = Convert.ToBoolean(metaData.isNullable(i));
				
					InitKeyInfo(row, dbMetaData, catalog, schemaName, tableName);
				
					row [(int)SCHEMA_TABLE.IsAliased] = columnName != baseColumnName;
					row [(int)SCHEMA_TABLE.IsExpression] = false;

					row [(int)SCHEMA_TABLE.IsAutoIncrement] = metaData.isAutoIncrement(i);

					row [(int)SCHEMA_TABLE.IsHidden] = false;
					row [(int)SCHEMA_TABLE.IsReadOnly] = metaData.isReadOnly(i);

					int columnType = metaData.getColumnType(i);
					string columnTypeName = metaData.getColumnTypeName(i);
					if(columnType == Types.ARRAY) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = typeof (java.sql.Array);
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.BIGINT) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfInt64;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.BINARY) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByteArray;
						row [(int)SCHEMA_TABLE.IsLong] = true;
					}
					else if(columnType == Types.BIT) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfBoolean;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.BLOB) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByteArray;
						row [(int)SCHEMA_TABLE.IsLong] = true;
					}
					else if(columnType == Types.CHAR) {
						// FIXME : specific for Microsoft SQl Server driver
						if (columnTypeName.Equals("uniqueidentifier")) {
							row [(int)SCHEMA_TABLE.ProviderType] = DbType.Guid;
							row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfGuid;
							row [(int)SCHEMA_TABLE.IsLong] = false;
						}
						else {
							row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
							row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfString;
							row [(int)SCHEMA_TABLE.IsLong] = false;
						}
					}
					else if(columnType == Types.CLOB) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfString; // instead og .java.sql.Clob
						row [(int)SCHEMA_TABLE.IsLong] = true;
					}
					else if(columnType == Types.DATE) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDateTime;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.DECIMAL) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDecimal;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
						//                else if(columnType == Types.DISTINCT)
						//                {
						//                    row ["ProviderType = (int)GetProviderType(columnType);
						//                    row ["DataType = typeof (?);
						//                    row ["IsLong = false;
						//                }
					else if(columnType == Types.DOUBLE) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDouble; // was typeof(float)
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.FLOAT) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDouble;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.REAL) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfFloat;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.INTEGER) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfInt32;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.JAVA_OBJECT) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfObject;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.LONGVARBINARY) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByteArray;
						row [(int)SCHEMA_TABLE.IsLong] = true;
					}
					else if(columnType == Types.LONGVARCHAR) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfString;
						row [(int)SCHEMA_TABLE.IsLong] = true;
					}
					else if(columnType == Types.NUMERIC) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDecimal;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.REF) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = typeof (java.sql.Ref);
						row [(int)SCHEMA_TABLE.IsLong] = true;
					}
					else if(columnType == Types.SMALLINT) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfInt16;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.STRUCT) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = typeof (java.sql.Struct);
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.TIME) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfTimespan;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.TIMESTAMP) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDateTime;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.TINYINT) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByte;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else if(columnType == Types.VARBINARY) {
						row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByteArray;
						row [(int)SCHEMA_TABLE.IsLong] = true;
					}
					else if(columnType == Types.VARCHAR) {
						// FIXME : specific for Microsoft SQl Server driver
						if (columnTypeName.Equals("sql_variant")) {
							row [(int)SCHEMA_TABLE.ProviderType] = DbType.Object;
							row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfObject;
							row [(int)SCHEMA_TABLE.IsLong] = false;
						}
						else {
							row [(int)SCHEMA_TABLE.ProviderType] = GetProviderType(columnType);
							row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfString;// (char[]);
							row [(int)SCHEMA_TABLE.IsLong] = false;//true;
						}
					}
					else if(columnType == -8 && columnTypeName.Equals("ROWID")) {
						// FIXME : specific for Oracle JDBC driver : OracleTypes.ROWID
						row [(int)SCHEMA_TABLE.ProviderType] = DbType.String;
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfString;
						row [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else {
						row [(int)SCHEMA_TABLE.ProviderType] = DbType.Object;
						row [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfObject;
						row [(int)SCHEMA_TABLE.IsLong] = true;
					}
					SchemaTable.Rows.Add (row);
				}
			}
			catch (SQLException e) {				
				throw CreateException(e);
			}
			finally {
				if (clonedConnection != null) {
					clonedConnection.Close();
				}
			}			
			return SchemaTable;
		}

		private void InitKeyInfo(DataRow row, DatabaseMetaData dbMetaData, String catalog, String schema, String table) {
			string column = (string)row [(int)SCHEMA_TABLE.BaseColumnName];

			row [(int)SCHEMA_TABLE.IsUnique] = false;
			row [(int)SCHEMA_TABLE.IsKey] = false;
			row [(int)SCHEMA_TABLE.IsIdentity] = false;
			row [(int)SCHEMA_TABLE.IsRowVersion] = false;

			if ((_command.Behavior & CommandBehavior.KeyInfo) == 0)
				return;

			if(table == null || column == null || dbMetaData == null)
				return;

			ResultSet indexInfoRes = dbMetaData.getIndexInfo(catalog,schema,table,true,false);
			try {
				while(indexInfoRes.next()) {
					if(indexInfoRes.getString("COLUMN_NAME") == column)
						row [(int)SCHEMA_TABLE.IsUnique] = true;
				}
			}
			finally {
				indexInfoRes.close();
			}

			ResultSet versionCol = dbMetaData.getVersionColumns(catalog, schema, table);
			try {
				while(versionCol.next()) {
					if(versionCol.getString("COLUMN_NAME") == column) {
						if (DatabaseMetaData__Finals.versionColumnPseudo == versionCol.getShort("PSEUDO_COLUMN")) {
							row [(int)SCHEMA_TABLE.IsIdentity] = true;
							row [(int)SCHEMA_TABLE.IsRowVersion] = true;
						}
					}
				}
			}
			finally {
				versionCol.close();
			}

			ResultSet bestRowId = dbMetaData.getBestRowIdentifier(catalog, schema, table, DatabaseMetaData__Finals.bestRowTemporary, false);
			try {
				while(bestRowId.next()) {
					if(bestRowId.getString("COLUMN_NAME") == column)
						row [(int)SCHEMA_TABLE.IsKey] = true;
				}
			}
			finally {
				bestRowId.close();
			}
		}

		protected static DataTable ConstructSchemaTable ()
		{
			Type booleanType = DbTypes.TypeOfBoolean;
			Type stringType = DbTypes.TypeOfString;
			Type intType = DbTypes.TypeOfInt32;
			Type typeType = DbTypes.TypeOfType;
			Type shortType = DbTypes.TypeOfInt16;

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName", stringType);
			schemaTable.Columns.Add ("ColumnOrdinal", intType);
			schemaTable.Columns.Add ("ColumnSize", intType);
			schemaTable.Columns.Add ("NumericPrecision", shortType);
			schemaTable.Columns.Add ("NumericScale", shortType);
			schemaTable.Columns.Add ("IsUnique", booleanType);
			schemaTable.Columns.Add ("IsKey", booleanType);
			schemaTable.Columns.Add ("BaseServerName", stringType);
			schemaTable.Columns.Add ("BaseCatalogName", stringType);
			schemaTable.Columns.Add ("BaseColumnName", stringType);
			schemaTable.Columns.Add ("BaseSchemaName", stringType);
			schemaTable.Columns.Add ("BaseTableName", stringType);
			schemaTable.Columns.Add ("DataType", typeType);
			schemaTable.Columns.Add ("AllowDBNull", booleanType);
			schemaTable.Columns.Add ("ProviderType", intType);
			schemaTable.Columns.Add ("IsAliased", booleanType);
			schemaTable.Columns.Add ("IsExpression", booleanType);
			schemaTable.Columns.Add ("IsIdentity", booleanType);
			schemaTable.Columns.Add ("IsAutoIncrement", booleanType);
			schemaTable.Columns.Add ("IsRowVersion", booleanType);
			schemaTable.Columns.Add ("IsHidden", booleanType);
			schemaTable.Columns.Add ("IsLong", booleanType);
			schemaTable.Columns.Add ("IsReadOnly", booleanType);
			return schemaTable;
		}

		#endregion // Methods
	}
}