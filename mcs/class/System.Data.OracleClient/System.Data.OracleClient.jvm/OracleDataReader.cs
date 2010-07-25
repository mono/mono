//
// System.Data.OracleClient.OracleDataReader
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

using System.Data.Common;
using System.Data.ProviderBase;

using java.sql;

namespace System.Data.OracleClient {
	public sealed class OracleDataReader : AbstractDataReader {
		#region Fields

		#endregion // Fields

		#region Constructors

		internal OracleDataReader(OracleCommand command) : base(command) {
		}

		#endregion // Constructors

		#region Methods

		protected sealed override SystemException CreateException(string message, SQLException e) {
			return new OracleException(message,e, (OracleConnection)_command.Connection);		
		}

		protected sealed override SystemException CreateException(java.io.IOException e) {
			return new OracleException(e, (OracleConnection)_command.Connection);		
		}

		public override String GetDataTypeName(int columnIndex) {
			try {
				string jdbcTypeName = Results.getMetaData().getColumnTypeName(columnIndex + 1);
				
				return OracleConvert.JdbcTypeNameToDbTypeName(jdbcTypeName);
			}
			catch (SQLException e) {
				throw CreateException(e);
			}
		}

		protected override int GetProviderType(int jdbcType) {
			return (int)OracleConvert.JdbcTypeToOracleType(jdbcType);   
		}

		protected override IReaderCacheContainer CreateReaderCacheContainer(int jdbcType, int columnIndex) {
			switch ((JavaSqlTypes)jdbcType) {
				case JavaSqlTypes.BINARY_FLOAT:
					jdbcType = (int)JavaSqlTypes.REAL;
					break;
				case JavaSqlTypes.BINARY_DOUBLE:
					jdbcType = (int)JavaSqlTypes.DOUBLE;
					break;
				case JavaSqlTypes.ROWID:
					jdbcType = (int)JavaSqlTypes.VARCHAR;
					break;
//				case JavaSqlTypes.CURSOR:
//					jdbcType = JavaSqlTypes.OTHER;
//					break;
				case JavaSqlTypes.TIMESTAMPNS:
					jdbcType = (int)JavaSqlTypes.TIMESTAMP;
					break;
				case JavaSqlTypes.TIMESTAMPTZ:
					jdbcType = (int)JavaSqlTypes.TIMESTAMP;
					break;
				case JavaSqlTypes.TIMESTAMPLTZ: 
					jdbcType = (int)JavaSqlTypes.TIMESTAMP;
					break;
				case JavaSqlTypes.INTERVALYM:
					jdbcType = (int)JavaSqlTypes.INTEGER;
					break;
				case JavaSqlTypes.INTERVALDS:
					jdbcType = (int)JavaSqlTypes.TIMESTAMP;
					break;
			}
			return base.CreateReaderCacheContainer (jdbcType, columnIndex);
		}


		protected override void SetSchemaType(DataRow schemaRow, ResultSetMetaData metaData, int columnIndex) {
			JavaSqlTypes columnType = (JavaSqlTypes)metaData.getColumnType(columnIndex);
			switch (columnType) {
				case JavaSqlTypes.BINARY_FLOAT:
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfFloat;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				case JavaSqlTypes.BINARY_DOUBLE:
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfDouble;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				case JavaSqlTypes.ROWID:
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfString;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				case JavaSqlTypes.CURSOR:
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfDouble;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				case JavaSqlTypes.TIMESTAMPNS:
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfTimespan;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				case JavaSqlTypes.TIMESTAMPTZ:
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfTimespan;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				case JavaSqlTypes.TIMESTAMPLTZ: 
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfTimespan;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				case JavaSqlTypes.INTERVALYM:
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfUInt32;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				case JavaSqlTypes.INTERVALDS:
					schemaRow [(int)SCHEMA_TABLE.ProviderType] = GetProviderType((int)columnType);
					schemaRow [(int)SCHEMA_TABLE.DataType] = OracleConvert.TypeOfTimespan;
					schemaRow [(int)SCHEMA_TABLE.IsLong] = false;
					break;
				default:
					base.SetSchemaType(schemaRow, metaData, columnIndex);
					break;
			}
		}

		public override decimal GetDecimal(int i) {
			if (IsNumeric(i))
				return GetDecimalSafe(i);

			return base.GetDecimal(i);
		}

		public override double GetDouble(int i) {
			if (IsNumeric(i))
				return GetDoubleSafe(i);

			return base.GetDouble(i);
		}

		public override float GetFloat(int i) {
			if (IsNumeric(i))
				return GetFloatSafe(i);

			return base.GetFloat(i);
		}
//
//		OracleClient does not "excuse" for Int16
//
//		public override short GetInt16(int i) {
//			if (IsNumeric(i))
//				return GetInt16Safe(i);
//
//			return base.GetInt16(i);
//		}


		public override int GetInt32(int i) {
			if (IsNumeric(i))
				return GetInt32Safe(i);

			return base.GetInt32(i);
		}

		public override long GetInt64(int i) {
			if (IsNumeric(i))
				return GetInt64Safe(i);

			return base.GetInt64(i);
		}

#if SUPPORT_ORACLE_TYPES
		#region GetOracleXXX

		public OracleBFile GetOracleBFile(
			int i
			) {
			throw new NotImplementedException();
		}

		public OracleBinary GetOracleBinary(
			int i
			) {
			throw new NotImplementedException();
		}

		public OracleDateTime GetOracleDateTime(
			int i
			) {
			throw new NotImplementedException();
		}

		public OracleLob GetOracleLob(
			int i
			) {
			throw new NotImplementedException();
		}

		public OracleMonthSpan GetOracleMonthSpan(
			int i
			) {
			throw new NotImplementedException();
		}

		public OracleNumber GetOracleNumber(
			int i
			) {
			throw new NotImplementedException();
		}

		public OracleString GetOracleString(
			int i
			) {
			throw new NotImplementedException();
		}

		public OracleTimeSpan GetOracleTimeSpan(
			int i
			) {
			throw new NotImplementedException();
		}

		public object GetOracleValue(
			int i
			) {
			throw new NotImplementedException();
		}

		public int GetOracleValues(
			object[] values
			) {
			throw new NotImplementedException();
		}

		#endregion
#endif

		#endregion // Methods
	}
}