//
// System.Data.Common.AbstractDataReader
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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


using System;
using System.Data;
using System.Collections;
using System.Data.Common;

using java.io;
using java.sql;

namespace System.Data.ProviderBase
{
	public abstract class AbstractDataReader : DbDataReader, ISafeDataRecord {

		#region Fields

		private ResultSetMetaData _resultsMetaData;
		protected AbstractDbCommand _command;
		private DataTable _schemaTable;
		private ReaderState _readerState = ReaderState.Uninitialized;

		private IReaderCacheContainer[] _readerCache;
		private int _currentCacheFilledPosition; 
		private Stack _resultSetStack = new Stack();
		private bool _isClosed = false;

		[Flags]
		private enum ReaderState { Uninitialized = 0, Empty = 1, HasRows = 2, FirstRed = 4, Eof = 8, Fetching = 16 };

		protected internal enum SCHEMA_TABLE { ColumnName,
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
		
		protected AbstractDataReader(AbstractDbCommand command) {
			_command = command;
			if (_command.Connection != null) {
				((AbstractDBConnection)_command.Connection).AddReference(this);
			}
		}

		#endregion // Constructors

		#region Properties

		public override int Depth {
			get { return 0; }
		}

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
								if (driverName.IndexOf("DB2", StringComparison.Ordinal) >= 0)
									prefetchSchema = Configuration.BooleanSetting.True;
							}

							if (prefetchSchema == Configuration.BooleanSetting.True)
								GetSchemaTable();

							ResultSet resultSet = (ResultSet)_resultSetStack.Peek();
							if (resultSet.next()) {
								_readerState = (ReaderState.HasRows | ReaderState.FirstRed);
								ResultSetMetaData rsMetaData = ResultsMetaData;
								DbConvert.JavaSqlTypes javaSqlType = (DbConvert.JavaSqlTypes)rsMetaData.getColumnType(1);
								if (javaSqlType == DbConvert.JavaSqlTypes.OTHER) {
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

		public override bool IsClosed {
			get { return _isClosed; }
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

		public override IEnumerator GetEnumerator ()
		{
			bool closeReader = (Behavior & CommandBehavior.CloseConnection) != 0;
			return new DbEnumerator (this , closeReader);
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
			return ((BytesReaderCacheContainer)ReaderCache[columnIndex])
				.GetBytes(dataIndex, buffer, bufferIndex, length);
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
			return ((CharsReaderCacheContainer)ReaderCache[columnIndex])
				.GetChars(dataIndex, buffer, bufferIndex, length);
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
			return GetDateTimeUnsafe(columnIndex);
		}

		DateTime GetDateTimeUnsafe(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((DateTimeReaderCacheContainer)ReaderCache[columnIndex]).GetDateTime();
		}

		public DateTime GetDateTimeSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is DateTimeReaderCacheContainer) {
				return GetDateTimeUnsafe(columnIndex);
			}
			else {
				return Convert.ToDateTime(GetValue(columnIndex));
			}
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
			return GetDecimalUnsafe(columnIndex);
		}

		decimal GetDecimalUnsafe(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((DecimalReaderCacheContainer)ReaderCache[columnIndex]).GetDecimal();
		}

		public decimal GetDecimalSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is DecimalReaderCacheContainer) {
				return GetDecimalUnsafe(columnIndex);
			}
			else {
				return Convert.ToDecimal(GetValue(columnIndex));
			}
		}

		public override double GetDouble(int columnIndex)
		{
			return GetDoubleUnsafe(columnIndex);
		}

		double GetDoubleUnsafe(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((DoubleReaderCacheContainer)ReaderCache[columnIndex]).GetDouble();
		}

		public double GetDoubleSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is DoubleReaderCacheContainer) {
				return GetDoubleUnsafe(columnIndex);
			}
			else {
				return Convert.ToDouble(GetValue(columnIndex));
			}
		}

		public override float GetFloat(int columnIndex)
		{
			return GetFloatUnsafe(columnIndex);
		}

		float GetFloatUnsafe(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((FloatReaderCacheContainer)ReaderCache[columnIndex]).GetFloat();
		}

		public float GetFloatSafe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is FloatReaderCacheContainer) {
				return GetFloatUnsafe(columnIndex);
			}
			else {
				return Convert.ToSingle(GetValue(columnIndex));
			}
		}

		public override short GetInt16(int columnIndex)
		{
			return GetInt16Unsafe(columnIndex);
		}

		short GetInt16Unsafe(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((Int16ReaderCacheContainer)ReaderCache[columnIndex]).GetInt16();
		}

		public short GetInt16Safe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is Int16ReaderCacheContainer) {
				return GetInt16Unsafe(columnIndex);
			}
			else {
				return Convert.ToInt16(GetValue(columnIndex));
			}
		}

		public override int GetInt32(int columnIndex)
		{
			return GetInt32Unsafe(columnIndex);
		}

		int GetInt32Unsafe(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((Int32ReaderCacheContainer)ReaderCache[columnIndex]).GetInt32();
		}

		public int GetInt32Safe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is Int32ReaderCacheContainer) {
				return GetInt32Unsafe(columnIndex);
			}
			else {
				return Convert.ToInt32(GetValue(columnIndex));
			}
		}

		public override long GetInt64(int columnIndex)
		{
			return GetInt64Unsafe(columnIndex);
		}

		long GetInt64Unsafe(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((Int64ReaderCacheContainer)ReaderCache[columnIndex]).GetInt64();
		}

		public long GetInt64Safe(int columnIndex)
		{
			if (ReaderCache[columnIndex] is Int64ReaderCacheContainer) {
				return GetInt64Unsafe(columnIndex);
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
			return GetStringUnsafe(columnIndex);
		}

		string GetStringUnsafe(int columnIndex)
		{
			FillReaderCache(columnIndex);
			return ((StringReaderCacheContainer)ReaderCache[columnIndex]).GetString();
		}

		public string GetStringSafe(int columnIndex) {
			if (ReaderCache[columnIndex] is StringReaderCacheContainer) {
				return GetStringUnsafe(columnIndex);
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
						readerCache[_currentCacheFilledPosition].Fetch(Results,_currentCacheFilledPosition, false);
					}					
				}
				else {
					readerCache[columnIndex].Fetch(Results,columnIndex, true);
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

		protected virtual IReaderCacheContainer CreateReaderCacheContainer(int jdbcType, int columnIndex) {
			switch ((DbConvert.JavaSqlTypes)jdbcType) {
				case DbConvert.JavaSqlTypes.ARRAY :
					return new ArrayReaderCacheContainer();
				case DbConvert.JavaSqlTypes.BIGINT :
					return new Int64ReaderCacheContainer();
				case DbConvert.JavaSqlTypes.BINARY :
				case DbConvert.JavaSqlTypes.VARBINARY :
				case DbConvert.JavaSqlTypes.LONGVARBINARY :
					return new BytesReaderCacheContainer();
				case DbConvert.JavaSqlTypes.BIT :
					return new BooleanReaderCacheContainer();
				case DbConvert.JavaSqlTypes.BLOB :
					return new BlobReaderCacheContainer();
				case DbConvert.JavaSqlTypes.VARCHAR:
				case DbConvert.JavaSqlTypes.CHAR :						
					if (String.CompareOrdinal("uniqueidentifier", ResultsMetaData.getColumnTypeName(columnIndex)) == 0) {
						return new GuidReaderCacheContainer();
					}
					else {
						return new StringReaderCacheContainer();
					}
				case DbConvert.JavaSqlTypes.CLOB :
					return new ClobReaderCacheContainer();
				case DbConvert.JavaSqlTypes.TIME :
					return new TimeSpanReaderCacheContainer();
				case DbConvert.JavaSqlTypes.DATE :
					AbstractDBConnection conn = (AbstractDBConnection)((ICloneable)_command.Connection);
					string driverName = conn.JdbcConnection.getMetaData().getDriverName();

					if (driverName.StartsWith("PostgreSQL")) {
						return new DateTimeReaderCacheContainer();
					}
					else
						goto case DbConvert.JavaSqlTypes.TIMESTAMP;
				case DbConvert.JavaSqlTypes.TIMESTAMP :				
					return new TimestampReaderCacheContainer();
				case DbConvert.JavaSqlTypes.DECIMAL :
				case DbConvert.JavaSqlTypes.NUMERIC :
					// jdbc driver for oracle identitfies both FLOAT and NUMBEr columns as 
					// java.sql.Types.NUMERIC (2), columnTypeName NUMBER, columnClassName java.math.BigDecimal 
					// therefore we relay on scale
					int scale = ResultsMetaData.getScale(columnIndex);
					if (scale == -127) {
						// Oracle db type FLOAT
						return new DoubleReaderCacheContainer();
					}
					else {
						return new DecimalReaderCacheContainer();
					}
				case DbConvert.JavaSqlTypes.DOUBLE :
				case DbConvert.JavaSqlTypes.FLOAT :
					return new DoubleReaderCacheContainer();
				case DbConvert.JavaSqlTypes.INTEGER :
					return new Int32ReaderCacheContainer();
				case DbConvert.JavaSqlTypes.LONGVARCHAR :
					return new StringReaderCacheContainer();
				case DbConvert.JavaSqlTypes.NULL :
					return new NullReaderCacheContainer();
				case DbConvert.JavaSqlTypes.REAL :
					return new FloatReaderCacheContainer();
				case DbConvert.JavaSqlTypes.REF :
					return new RefReaderCacheContainer();
				case DbConvert.JavaSqlTypes.SMALLINT :
					return new Int16ReaderCacheContainer();
				case DbConvert.JavaSqlTypes.TINYINT :
					return new ByteReaderCacheContainer();
				case DbConvert.JavaSqlTypes.DISTINCT :
				case DbConvert.JavaSqlTypes.JAVA_OBJECT :
				case DbConvert.JavaSqlTypes.OTHER :
				case DbConvert.JavaSqlTypes.STRUCT :
				default :
					return new ObjectReaderCacheContainer();
			}
		}

		private IReaderCacheContainer[] CreateReaderCache()
		{
			try {
				IReaderCacheContainer[] readerCache = new IReaderCacheContainer[FieldCount];
				for(int i=1; i <= readerCache.Length; i++)
					readerCache[i-1] = CreateReaderCacheContainer(ResultsMetaData.getColumnType(i), i);

				return readerCache;
			}
			catch(SQLException e) {
				throw CreateException(e);
			}
		}

		protected bool IsNumeric(int columnIndex)
		{
			return ReaderCache[columnIndex].IsNumeric();
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

		protected virtual void SetSchemaType(DataRow schemaRow, ResultSetMetaData metaData, int columnIndex) {
			DbConvert.JavaSqlTypes columnType = (DbConvert.JavaSqlTypes)metaData.getColumnType(columnIndex);

			switch (columnType) {
				case DbConvert.JavaSqlTypes.ARRAY: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = typeof (java.sql.Array);
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.BIGINT: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfInt64;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.BINARY: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByteArray;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = true;
					break;
				}
				case DbConvert.JavaSqlTypes.BIT: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfBoolean;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.BLOB: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByteArray;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = true;
					break;
				}
				case DbConvert.JavaSqlTypes.VARCHAR:
				case DbConvert.JavaSqlTypes.CHAR: {
					// FIXME : specific for Microsoft SQl Server driver
					if (String.CompareOrdinal(metaData.getColumnTypeName(columnIndex), "uniqueidentifier") == 0) {
						schemaRow [(int)SCHEMA_TABLE.ProviderType] = DbType.Guid;
						schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfGuid;
						schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else
					if (String.CompareOrdinal(metaData.getColumnTypeName(columnIndex), "sql_variant") == 0) {
						schemaRow [(int)SCHEMA_TABLE.ProviderType] = DbType.Object;
						schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfObject;
						schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else {
						schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
						schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfString;
						schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					}
					break;
				}
				case DbConvert.JavaSqlTypes.CLOB: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfString; // instead og .java.sql.Clob
					schemaRow [(int)SCHEMA_TABLE.IsLong] = true;
					break;
				}
				case DbConvert.JavaSqlTypes.DATE: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDateTime;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
					//                else if(DbConvert.JavaSqlTypes.DISTINCT)
					//                {
					//                    schemaRow ["ProviderType = (int)GetProviderType((int)columnType);
					//                    schemaRow ["DataType = typeof (?);
					//                    schemaRow ["IsLong = false;
					//                }
				case DbConvert.JavaSqlTypes.DOUBLE: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDouble; // was typeof(float)
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.FLOAT: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDouble;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.REAL: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfFloat;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.INTEGER: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfInt32;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.JAVA_OBJECT: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfObject;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.LONGVARBINARY: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByteArray;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = true;
					break;
				}
				case DbConvert.JavaSqlTypes.LONGVARCHAR: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfString;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = true;
					break;
				}
				case DbConvert.JavaSqlTypes.DECIMAL:
				case DbConvert.JavaSqlTypes.NUMERIC: {
					int scale = ResultsMetaData.getScale(columnIndex);
					if (scale == -127) {
						schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
						schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDouble;
						schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					}
					else {
						schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
						schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDecimal;
						schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					}
					break;
				}
				case DbConvert.JavaSqlTypes.REF: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = typeof (java.sql.Ref);
					schemaRow [(int)SCHEMA_TABLE.IsLong] = true;
					break;
				}
				case DbConvert.JavaSqlTypes.SMALLINT: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfInt16;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.STRUCT: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = typeof (java.sql.Struct);
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.TIME: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfTimespan;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.TIMESTAMP: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfDateTime;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.TINYINT: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByte;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				}
				case DbConvert.JavaSqlTypes.VARBINARY: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfByteArray;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = true;
					break;
				}
				default: {
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = DbType.Object;
					schemaRow [(int)SCHEMA_TABLE.DataType] = DbTypes.TypeOfObject;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = true;
					break;
				}
			}
		}

		public override DataTable GetSchemaTable()
		{
			if (SchemaTable.Rows != null && SchemaTable.Rows.Count > 0) {
				return SchemaTable;
			}
            
			ResultSetMetaData metaData;			
			if (Behavior == CommandBehavior.SchemaOnly) {
				try {
					metaData = ((PreparedStatement)_command.Statement).getMetaData();
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

					SetSchemaType(row, metaData, i);

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

			ResultSet primaryKeys = dbMetaData.getPrimaryKeys(catalog,schema,table);
			bool primaryKeyExists = false;
			int columnCount = 0;
			try {
				while(primaryKeys.next()) {
					columnCount++;
					if(primaryKeys.getString("COLUMN_NAME") == column) {
						row [(int)SCHEMA_TABLE.IsKey] = true;
						primaryKeyExists = true;
					}
				}
				// column constitutes a key by itself, so it should be marked as unique 
				if ((columnCount == 1) && (((bool)row [(int)SCHEMA_TABLE.IsKey]) == true)) {
					row [(int)SCHEMA_TABLE.IsUnique] = true;
				}
			}
			finally {
				primaryKeys.close();
			}

			ResultSet indexInfoRes = dbMetaData.getIndexInfo(catalog,schema,table,true,false);
			string currentIndexName = null;
			columnCount = 0;
			bool belongsToCurrentIndex = false;
			bool atFirstIndex = true;
			bool uniqueKeyExists = false;
			try {
				while(indexInfoRes.next()) {
					if (indexInfoRes.getShort("TYPE") ==  DatabaseMetaData__Finals.tableIndexStatistic) {
						// index of type tableIndexStatistic identifies table statistics - ignore it
						continue;
					}
					
					uniqueKeyExists = true;
					string iname = indexInfoRes.getString("INDEX_NAME");
					if (currentIndexName == iname) {
						// we're within the rows of the same index 
						columnCount++;
					}
					else {
						// we jump to row of new index 
						if (belongsToCurrentIndex && columnCount == 1) {
							// there is a constraint of type UNIQUE that applies only to this column
							row [(int)SCHEMA_TABLE.IsUnique] = true;
						}

						if (currentIndexName != null) {
							atFirstIndex = false;
						}
						currentIndexName = iname;
						columnCount = 1;
						belongsToCurrentIndex = false;
					}

					if(indexInfoRes.getString("COLUMN_NAME") == column) {
						// FIXME : this will cause "spare" columns marked as IsKey. Needs future investigation.
						// only the first index we met should be marked as a key
						//if (atFirstIndex) {
							row [(int)SCHEMA_TABLE.IsKey] = true;
						//}
						belongsToCurrentIndex = true;						
					}
				}
				// the column appears in the last index, which is single-column
				if (belongsToCurrentIndex && columnCount == 1) {
					// there is a constraint of type UNIQUE that applies only to this column
					row [(int)SCHEMA_TABLE.IsUnique] = true;
				}
			}
			finally {
				indexInfoRes.close();
			}			

			if(!primaryKeyExists && !uniqueKeyExists) {
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