//
// System.Data.SqlClient.SqlDataReader
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

using System.Data.SqlTypes;
using System.Data.ProviderBase;

using java.sql;

namespace System.Data.SqlClient
{
    public class SqlDataReader : AbstractDataReader
    {

		#region Constructors

		internal SqlDataReader(SqlCommand command) : base(command)
        {
        }

		#endregion // Constructors

		#region Properties

		#endregion // Properties

		#region Methods

		protected sealed override SystemException CreateException(string message, SQLException e)
		{
			return new SqlException(message, e, (SqlConnection)_command.Connection);		
		}

		protected sealed override SystemException CreateException(java.io.IOException e)
		{
			return new SqlException(e, (SqlConnection)_command.Connection);
		}

		public override String GetDataTypeName(int columnIndex)
		{
			try {
				string jdbcTypeName = Results.getMetaData().getColumnTypeName(columnIndex + 1);
				
				return SqlConvert.JdbcTypeNameToDbTypeName(jdbcTypeName);
			}
			catch (SQLException e) {
				throw CreateException(e);
			}
		}

		protected override int GetProviderType(int jdbcType)
		{
			return (int)SqlConvert.JdbcTypeToSqlDbType(jdbcType);   
		}

        // Gets the value of the specified column as a SqlBinary.
        public SqlBinary GetSqlBinary(int columnIndex)
        {
			byte[] bytes = GetBytes(columnIndex);
            if(IsDBNull(columnIndex)) {
				return SqlBinary.Null;
			}
            else {
				return new SqlBinary(bytes);
            }
        }

        // Gets the value of the specified column as a SqlBoolean.
        public SqlBoolean GetSqlBoolean(int columnIndex)
        {
			bool boolean = GetBoolean(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlBoolean.Null;
			}
			else {
				return new SqlBoolean(boolean);
            }
        }

        // Gets the value of the specified column as a SqlByte.
        public SqlByte GetSqlByte(int columnIndex)
        {
			byte byt = GetByte(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlByte.Null;
			}
            else {
                return new SqlByte(byt);
            }
        }

#if NET_2_0

		public virtual SqlBytes GetSqlBytes (int columnIndex)
		{
			byte [] bytes = GetBytes (columnIndex);
			if (IsDBNull (columnIndex)) {
				return SqlBytes.Null;
			}
			else {
				return new SqlBytes (bytes);
			}
		}

		public virtual SqlChars GetSqlChars (int columnIndex)
		{
			SqlString sqlStr = GetSqlString (columnIndex);
			if (sqlStr.IsNull) {
				return SqlChars.Null;
			}
			else {
				return new SqlChars (sqlStr);
			}
		}

		[MonoNotSupported("SqlXml is not fully implemented")]
		public virtual SqlXml GetSqlXml (int columnIndex)
		{
			throw new NotImplementedException ();
		}

#endif

        // Gets the value of the specified column as a SqlDecimal.
        public SqlDecimal GetSqlDecimal(int columnIndex)
        {
			decimal dec = GetDecimal(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlDecimal.Null;
			}
            else {
                return new SqlDecimal(dec);
            }
        }

        // Gets the value of the specified column as a SqlDateTime.
        public SqlDateTime GetSqlDateTime(int columnIndex)
        {
			DateTime dateTime = GetDateTime(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlDateTime.Null;
			}
            else {
                return new SqlDateTime(dateTime);
            }
        }

        // Gets the value of the specified column as a SqlDouble.
        public SqlDouble GetSqlDouble(int columnIndex)
        {
			double doubl = GetDouble(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlDouble.Null;
			}
            else {
                return new SqlDouble(doubl);
            }
        }

        // Gets the value of the specified column as a SqlInt16.
        public SqlInt16 GetSqlInt16(int columnIndex)
        {
			short s = GetInt16(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlInt16.Null;
			}
            else {
                return new SqlInt16(s);
            }
        }

        // Gets the value of the specified column as a SqlInt32.
        public SqlInt32 GetSqlInt32(int columnIndex)
        {
			int i = GetInt32(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlInt32.Null;
			}
            else {
                return new SqlInt32(i);
            }
        }

        // Gets the value of the specified column as a SqlInt64.
        public SqlInt64 GetSqlInt64(int columnIndex)
        {
			long l = GetInt64(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlInt64.Null;
			}
            else {
                return new SqlInt64(l);
            }
        }

        // Gets the value of the specified column as a SqlMoney.
        public SqlMoney GetSqlMoney(int columnIndex)
        {
			decimal dec = GetDecimal(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlMoney.Null;
			}
			else {
				return new SqlMoney(dec);
			}
        }

        // Gets the value of the specified column as a SqlSingle.
        public SqlSingle GetSqlSingle(int columnIndex)
        {
			float f = GetFloat(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlSingle.Null;
			}
            else {
                return new SqlSingle(f);
            }
        }

        // Gets the value of the specified column as a SqlString.
        public SqlString GetSqlString(int columnIndex)
        {
			string str = GetString(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlString.Null;
			}
            else {
                return new SqlString(str);
            }
        }

		// Gets the value of the specified column as a SqlGuid.
        public SqlGuid GetSqlGuid(int columnIndex)
        {
			object obj = GetValue(columnIndex);
            if(IsDBNull(columnIndex)) {
                return SqlGuid.Null;
			}
            else {
				if (obj is byte[]) {
					return new SqlGuid((byte[])obj);
				}
				else {
					return new SqlGuid((string)obj);
				}
            }
        }

		// Gets all the attribute columns in the current row.
        public int GetSqlValues(Object[] values)
        {
            int columnCount = FieldCount;
            int i = 0;
            for (; i < values.Length && i < columnCount; i++) {
                values[i] = GetSqlValue(i);
            }
            return i;
        }

		// Gets an Object that is a representation of the underlying SqlDbType Variant.
        public Object GetSqlValue(int columnIndex)
        {
            try {
				int jdbcType = ResultsMetaData.getColumnType(columnIndex + 1);
				SqlDbType sqlDbType = SqlConvert.JdbcTypeToSqlDbType(jdbcType);

				switch (sqlDbType) {
					case SqlDbType.BigInt : return GetSqlInt64(columnIndex);
					case SqlDbType.Binary : return GetSqlBinary(columnIndex);
					case SqlDbType.Bit : return GetSqlBoolean(columnIndex);
					case SqlDbType.Char : return GetSqlString(columnIndex);
					case SqlDbType.DateTime : return GetSqlDateTime(columnIndex);
					case SqlDbType.Decimal : return GetSqlDecimal(columnIndex);
					case SqlDbType.Float : return GetSqlDouble(columnIndex);
					case SqlDbType.Image : return GetSqlBinary(columnIndex);
					case SqlDbType.Int : return GetSqlInt32(columnIndex);
					case SqlDbType.Money : return GetSqlDecimal(columnIndex);
					case SqlDbType.NChar : return GetSqlString(columnIndex);
					case SqlDbType.NText : return GetSqlString(columnIndex);
					case SqlDbType.NVarChar : return GetSqlString(columnIndex);
					case SqlDbType.Real : return GetSqlSingle(columnIndex);
					case SqlDbType.UniqueIdentifier : return GetSqlGuid(columnIndex);
					case SqlDbType.SmallDateTime : return GetSqlDateTime(columnIndex);
					case SqlDbType.SmallInt : return GetSqlInt16(columnIndex);
					case SqlDbType.SmallMoney : return GetSqlDecimal(columnIndex);
					case SqlDbType.Text : return GetSqlString(columnIndex);
					case SqlDbType.Timestamp : return GetSqlDateTime(columnIndex);
					case SqlDbType.TinyInt : return GetSqlByte(columnIndex);
					case SqlDbType.VarBinary : return GetSqlBinary(columnIndex);
					case SqlDbType.VarChar : return GetSqlString(columnIndex);
					case SqlDbType.Variant : return GetValue(columnIndex);
					default : return GetValue(columnIndex);
				}
            }
            catch (SQLException exp) {
                throw new Exception(exp.Message);
            }
        }

#if NET_2_0
		protected bool IsCommandBehavior (CommandBehavior condition)
		{
			return (_command.Behavior & condition) == condition;
		}
#endif
		#endregion // Methods
    }
}