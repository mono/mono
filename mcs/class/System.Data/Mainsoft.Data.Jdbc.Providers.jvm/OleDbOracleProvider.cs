//
// Mainsoft.Data.Jdbc.Providers.OleDbOracleProvider
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

using System;
using System.Collections;
using Mainsoft.Data.Configuration;
using System.Reflection;

namespace Mainsoft.Data.Jdbc.Providers
{
	public class OleDbOracleProvider : GenericProvider
	{
		#region Consts

		private const string Port = "Port";
		private const string ROWID = "ROWID";
		private const string DBTYPE_CHAR = "DBTYPE_CHAR";

		#endregion //Consts

		#region oracle.sql.Types constants

		private enum JavaSqlTypes {
			ARRAY = 2003 ,
			BIGINT = -5, 
			BINARY = -2 ,
			BIT = -7 ,
			BLOB = 2004, 
			BOOLEAN = 16, 
			CHAR = 1, 
			CLOB = 2005, 
			DATALINK = 70, 
			DATE = 91, 
			DECIMAL = 3, 
			DISTINCT = 2001, 
			DOUBLE = 8, 
			FLOAT = 6, 
			INTEGER = 4, 
			JAVA_OBJECT = 2000, 
			LONGVARBINARY = -4,
			LONGVARCHAR = -1, 
			NULL = 0, 
			NUMERIC = 2 ,
			OTHER = 1111 ,
			REAL = 7 ,
			REF = 2006 ,
			SMALLINT = 5,
			STRUCT = 2002, 
			TIME = 92, 
			TIMESTAMP = 93, 
			TINYINT = -6, 
			VARBINARY = -3, 
			VARCHAR = 12,

			//ORACLE types, see oracle.jdbc.OracleTypes
			BINARY_FLOAT  = 100,
			BINARY_DOUBLE =	101,
			ROWID =	-8,
			CURSOR = -10,
			TIMESTAMPNS = -100,
			TIMESTAMPTZ = -101,
			TIMESTAMPLTZ = -102,
			INTERVALYM 	= -103,
			INTERVALDS 	= -104,
		}

		#endregion

		#region Fields

		#endregion // Fields

		#region Constructors

		public OleDbOracleProvider (IDictionary providerInfo) : base (providerInfo)
		{
		}

		#endregion // Constructors

		#region Properties

		#endregion // Properties

		#region Methods

		public override IConnectionStringDictionary GetConnectionStringBuilder (string connectionString)
		{
			IConnectionStringDictionary conectionStringBuilder = base.GetConnectionStringBuilder (connectionString);

			string port = (string) conectionStringBuilder [Port];
			if (port == null || port.Length == 0) {
				port = (string) ProviderInfo [Port];
				conectionStringBuilder.Add (Port, port);
			}
			
			return conectionStringBuilder;
		}

		public override java.sql.Connection GetConnection(IConnectionStringDictionary conectionStringBuilder) {
			return new OracleConnection(base.GetConnection (conectionStringBuilder));
		}


		#endregion //Methods

		#region OracleConnection

		sealed class OracleConnection : Connection {

			public OracleConnection(java.sql.Connection connection)
				: base(connection) {}

			public override java.sql.Statement createStatement() {
				return new OracleStatement (base.createStatement());
			}

			public override java.sql.Statement createStatement(int arg_0, int arg_1) {
				return new OracleStatement (base.createStatement(arg_0, arg_1));
			}

			public override java.sql.Statement createStatement(int arg_0, int arg_1, int arg_2) {
				return new OracleStatement (base.createStatement(arg_0, arg_1, arg_2));
			}

			public override java.sql.CallableStatement prepareCall(string arg_0) {
				return new OracleCallableStatement(base.prepareCall (arg_0));
			}

			public override java.sql.CallableStatement prepareCall(string arg_0, int arg_1, int arg_2) {
				return new OracleCallableStatement(base.prepareCall (arg_0, arg_1, arg_2));
			}

			public override java.sql.CallableStatement prepareCall(string arg_0, int arg_1, int arg_2, int arg_3) {
				return new OracleCallableStatement(base.prepareCall (arg_0, arg_1, arg_2, arg_3));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1, int arg_2) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1, arg_2));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, int arg_1, int arg_2, int arg_3) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1, arg_2, arg_3));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, int[] arg_1) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1));
			}

			public override java.sql.PreparedStatement prepareStatement(string arg_0, string[] arg_1) {
				return new OraclePreparedStatement(base.prepareStatement (arg_0, arg_1));
			}
		}

		#endregion

		sealed class OracleStatement : Statement {

			public OracleStatement (java.sql.Statement statement) 
				: base (statement) {
			}

			public override java.sql.ResultSet executeQuery(string arg_0) {
				return new OracleResultSet (base.executeQuery(arg_0));
			}

			public override java.sql.ResultSet getResultSet() {
				return new OracleResultSet (base.getResultSet());
			}
		}

		sealed class OraclePreparedStatement : PreparedStatement, IPreparedStatement {
			readonly MethodInfo _info;

			public OraclePreparedStatement(java.sql.PreparedStatement statement)
				: base(statement) {
				_info = Wrapped.GetType().GetMethod("setFixedCHAR");
			}

			public override java.sql.ResultSet executeQuery(string arg_0) {
				return new OracleResultSet (base.executeQuery(arg_0));
			}

			#region IPreparedStatement Members

			public void setBit(int parameterIndex, int value) {
				base.setInt(parameterIndex, value);
			}

			public void setChar(int parameterIndex, string value) {
				if (_info == null) {
					base.setString(parameterIndex, value);
					return;
				}

				_info.Invoke(Wrapped, new object[] {
							new java.lang.Integer(parameterIndex),
							value});
			}

			public void setNumeric(int parameterIndex, java.math.BigDecimal value) {
				base.setBigDecimal(parameterIndex, value);
			}

			public void setReal(int parameterIndex, double value) {
				base.setDouble(parameterIndex, value);
			}

			#endregion
		}

		sealed class OracleCallableStatement : CallableStatement, IPreparedStatement {
			readonly MethodInfo _info;

			public OracleCallableStatement(java.sql.CallableStatement statement)
				: base(statement) {
				_info = Wrapped.GetType().GetMethod("setFixedCHAR");
			}

			public override java.sql.ResultSet executeQuery(string arg_0) {
				return new OracleResultSet (base.executeQuery(arg_0));
			}

			#region IPreparedStatement Members

			public void setBit(int parameterIndex, int value) {
				base.setInt(parameterIndex, value);
			}

			public void setChar(int parameterIndex, string value) {
				if (_info == null) {
					base.setString(parameterIndex, value);
					return;
				}

				_info.Invoke(Wrapped, new object[] {
								new java.lang.Integer(parameterIndex),
								value});
			}

			public void setNumeric(int parameterIndex, java.math.BigDecimal value) {
				base.setBigDecimal(parameterIndex, value);
			}

			public void setReal(int parameterIndex, double value) {
				base.setDouble(parameterIndex, value);
			}

			#endregion

		}

		sealed class OracleResultSet : ResultSet {
			public OracleResultSet (java.sql.ResultSet resultSet) : base (resultSet) {
			}

			public override java.sql.ResultSetMetaData getMetaData() {
				return new OracleResultSetMetaData (base.getMetaData ());
			}
		}

		sealed class OracleResultSetMetaData: ResultSetMetaData {
			public OracleResultSetMetaData (java.sql.ResultSetMetaData resultSetMetaData)
				: base (resultSetMetaData) {
			}

			public override int getColumnType(int arg_0) {
				int jdbcType = base.getColumnType (arg_0);
				if ((JavaSqlTypes)jdbcType == JavaSqlTypes.ROWID)
					return (int)JavaSqlTypes.VARCHAR;

				return jdbcType;
			}

			public override string getColumnTypeName(int arg_0) {
				string columnTypeName = base.getColumnTypeName (arg_0);
				if (ROWID == columnTypeName)
					return DBTYPE_CHAR;

				return columnTypeName;
			}
		}

	}
}
