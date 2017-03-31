// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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
using System.Data.OracleClient ;
using System.IO;
using System.Collections;
using Sys = System;
using MonoTests.System.Data.Utils.Data;

// Provide All Data required by the diffderent tests e.g.DataTable, DataRow ...
namespace MonoTests.System.Data.Utils {
	/// <summary>
	/// Types of Database Servers that tests can be run on.
	/// </summary>
	public enum DataBaseServer {
		SQLServer,
		Oracle,
		DB2,
		Sybase,
		PostgreSQL,
		Unknown
	}

	public class ConnectedDataProvider {

		#region Private
		//A string containing all printable charachters.
		private const string SAMPLE_STRING = "abcdefghijklmnopqrstuvwxyz1234567890~!@#$%^&*()_+-=[]\\|;:,./<>? ";
		#endregion

		#region Public
		/// <summary>
		/// Name of the table in the database, that contain columns of simple types.
		/// </summary>
		public const string SIMPLE_TYPES_TABLE_NAME = "TYPES_SIMPLE";
		/// <summary>
		/// Name of the table in the database, that contain columns of extended types.
		/// </summary>
		public const string EXTENDED_TYPES_TABLE_NAME = "TYPES_EXTENDED";
		/// <summary>
		/// Name of the table in the database, that contain columns of DB specific types.
		/// </summary>
		public const string SPECIFIC_TYPES_TABLE_NAME = "TYPES_SPECIFIC";
		#endregion

		public static string ConnectionString {
			get {
                                string connection_string = Environment.GetEnvironmentVariable ("MONO_TESTS_ORACLE_CONNECTION_STRING");
                                if(connection_string == null)
                                        NUnit.Framework.Assert.Ignore ("Please consult README.tests.");
                                return connection_string;
			}
		}

		//	SQLClient does not allow to use the Provider token
		//	since Provider is always the first parameter(in GHT framework),
		//	we trim it.
		public static string ConnectionStringSQLClient {
			get {
				return ConnectionString.Substring(ConnectionString.IndexOf(";"));
			}
		}
	

		/// <summary>
		/// Resolves the type of DB server specified by the "ADOConString.txt" file.
		/// </summary>
		/// <returns>The type of DB server specified by the "ADOConString.txt" file.</returns>
		public static DataBaseServer GetDbType() {
			return ConnectedDataProvider.GetDbType(ConnectedDataProvider.ConnectionString);
		}
		
		/// <summary>
		/// Resolves the type of DB server that the specified connection refers.
		/// </summary>
		/// <param name="OleCon">A valid connection object to a DataBase.</param>
		/// <returns>The type of DB server that the specified connection refers to.</returns>
		public static DataBaseServer GetDbType(Sys.Data.OracleClient.OracleConnection OleCon) {
			return ConnectedDataProvider.GetDbType(OleCon.ConnectionString);
		}

		/// <summary>
		/// Resolves the type of DB server that the specified connection string refers.
		/// </summary>
		/// <param name="ConnectionString">A valid connection string to a DataBase server.</param>
		/// <returns>The type of DB server that the specified connection string refers to.</returns>
		public static DataBaseServer GetDbType(string ConnectionString) {
			return DataBaseServer.Oracle;
		}

		/// <summary>
		/// Creates a DbTypeParametersCollection with default types and data for the TYPES_SIMPLE table.
		/// </summary>
		/// <returns>The initialized DbTypeParametersCollection</returns>
		public static DbTypeParametersCollection GetSimpleDbTypesParameters() {
			DbTypeParametersCollection row = new DbTypeParametersCollection(SIMPLE_TYPES_TABLE_NAME);
			switch (ConnectedDataProvider.GetDbType(ConnectedDataProvider.ConnectionString)) {
					#region SQLServer
				case MonoTests.System.Data.Utils.DataBaseServer.SQLServer:
					row.Add("bit", true, 1);
					row.Add("tinyint", (byte)25, 1);
					row.Add("smallint", (Int16)77, 2);
					row.Add("int", (Int32)2525, 4);
					row.Add("bigint", (Int64)25251414, 8);
					row.Add("decimal", 10M, 9);	//(Decimal)10
					row.Add("numeric", 123123M, 9); //(Decimal)123123
					row.Add("float", 17.1414257, 8);
					row.Add("real", (float)0.71425, 4);
					row.Add("char", "abcdefghij", 10);
					row.Add("nchar", "klmnopqrst", 10);
					row.Add("varchar", "qwertasdfg", 50);
					row.Add("nvarchar", "qwertasdfg", 50);
					break;
					#endregion

					#region Sybase
				case MonoTests.System.Data.Utils.DataBaseServer.Sybase:
					//row.Add("BIT", true, 1);
					row.Add("TINYINT", (byte)25, 1);
					row.Add("SMALLINT", (Int16)77, 2);
					row.Add("INT", (Int32)2525, 4);
					//row.Add("BIGINT", (Int64)25251414, 8);
					row.Add("DECIMAL", 10M, 9);	//(Decimal)10
					row.Add("NUMERIC", 123123M, 9); //(Decimal)123123
					row.Add("FLOAT", 17.1414257, 8);
					row.Add("REAL", (float)0.71425, 4);
					row.Add("CHAR", "abcdefghij", 10);
					row.Add("NCHAR", "klmnopqrst", 10);
					row.Add("VARCHAR", "qwertasdfg", 50);
					row.Add("NVARCHAR", "qwertasdfg", 50);
					break;
					#endregion

					#region ORACLE
				case MonoTests.System.Data.Utils.DataBaseServer.Oracle:
					row.Add("NUMBER", 21M, 22);	//(Decimal)21
					row.Add("LONG", SAMPLE_STRING, 2147483647);	//Default data type in .NET is system.String.
					row.Add("FLOAT", 1.234, 22);
					row.Add("VARCHAR", "qwertasdfg", 10);
					row.Add("NVARCHAR", "qwertasdfg", 20);
					row.Add("CHAR", "abcdefghij", 10);
					row.Add("NCHAR", "abcdefghij", 10);
					break;
					#endregion

					#region DB2
				case MonoTests.System.Data.Utils.DataBaseServer.DB2:
					row.Add("SMALLINT", (Int16)2, 2);
					row.Add("INTEGER", 7777, 4);
					row.Add("BIGINT", (Int64)21767267, 8);
					row.Add("DECIMAL", 123M, 9); //(decimal)123
					row.Add("REAL", (float)0.7, 4);
					row.Add("DOUBLE", 1.7, 8);
					row.Add("CHARACTER", "abcdefghij", 10);
					row.Add("VARCHAR", "qwertasdfg", 10);
					row.Add("LONGVARCHAR", SAMPLE_STRING, 32000);
					break;
					#endregion

					#region PostgreSQL
				case MonoTests.System.Data.Utils.DataBaseServer.PostgreSQL:
					
					// PostgreSQL ODBC Type BOOL returns String with value "1" 
					// so we don't run it on .NET
					//					if (!GHTEnvironment.IsJavaRunTime())
					//					{
					//						row.Add("BOOL", "1", 1);
					//					}
					//					else
					//					{
					row.Add("BOOL", (bool)true, 1);
					//					}

					row.Add("INT2", (Int16)21, 2);
					row.Add("INT4", (Int32)30000, 4);
					row.Add("INT8", (Int64)30001, 8);
					row.Add("NUMERIC", (decimal)100000M, 10); //(decimal)100000
					row.Add("FLOAT4", (Single)7.23157, 4);
					row.Add("FLOAT8", (Double)7.123456, 8);
					row.Add("VARCHAR", "qwertasdfg", 10);
					row.Add("CHAR", "abcdefghij", 10);
					row.Add("NCHAR", "klmnopqrst", 10);
					break;
					#endregion
			}
			return row;
		}
		/// <summary>
		/// Creates a DbTypeParametersCollection with default types and data for the TYPES_EXTENDED table.
		/// </summary>
		/// <returns>The initialized DbTypeParametersCollection</returns>
		public static DbTypeParametersCollection GetExtendedDbTypesParameters() {
			DbTypeParametersCollection row = new DbTypeParametersCollection(EXTENDED_TYPES_TABLE_NAME);
			switch (ConnectedDataProvider.GetDbType(ConnectedDataProvider.ConnectionString)) {
					#region SQLServer
				case MonoTests.System.Data.Utils.DataBaseServer.SQLServer:
					row.Add("text", SAMPLE_STRING, 16);
					row.Add("ntext", SAMPLE_STRING, 16);
					row.Add("binary", new byte[]	{0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0}, 50);
					row.Add("varbinary", new byte[]	{0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0}, 50);
					row.Add("datetime", new DateTime(2004, 8, 9, 20, 30, 15, 500), 8);
					row.Add("smalldatetime", new DateTime(2004, 8, 9, 20, 30, 00), 4);
					break;
					#endregion

					#region Sybase
				case MonoTests.System.Data.Utils.DataBaseServer.Sybase:
					row.Add("TEXT", SAMPLE_STRING, 16);
					//There is probably a bug in the jdbc driver , we've tried to insert this string using
					//sybase command tool and it gave the same result (3850)
					row.Add("NTEXT", SAMPLE_STRING.Trim() , 16);
					row.Add("BINARY", new byte[]	{0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0}, 50);
					row.Add("VARBINARY", new byte[]	{0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
														0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0}, 50);
					row.Add("DATETIME", new DateTime(2004, 8, 9, 20, 30, 15, 500), 8);
					row.Add("SMALLDATETIME", new DateTime(2004, 8, 9, 20, 30, 00), 4);
					break;
					#endregion

					#region ORACLE
				case MonoTests.System.Data.Utils.DataBaseServer.Oracle:
					row.Add("RAW", new byte[]	{0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0}, 10);
					row.Add("LONGRAW", new byte[]	{0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
														,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													}, 100);
					row.Add("DATE", new DateTime(2004, 8, 9, 20, 30, 15), 7);
					
					// The .NET Framework provides support for Oracle LOBs in the OracleClient namespace, but not in the Oracle namespace.
					// Since Visual MainWin does not support the OracleClient namespace, a partial support for this important feature is provided in the Oracle namespace.
					// See ms-help://MS.VSCC.2003/VMW.GH.1033/ghdoc/vmwdoc_ADONET_data_access_limitations_51.htm
					break;
					#endregion

					#region DB2
				case MonoTests.System.Data.Utils.DataBaseServer.DB2:
					row.Add("DATE", new DateTime(2004, 8, 9, 20, 30, 15, 500).Date);
					row.Add("TIME", new TimeSpan(20, 30, 15));
					row.Add("TIMESTAMP", new DateTime(2004, 8, 9, 20, 30, 15, 500));
					row.Add("BLOB", new byte[]	{0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
													,0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0
												});
					row.Add("CLOB", SAMPLE_STRING
						+ SAMPLE_STRING
						+ SAMPLE_STRING
						+ SAMPLE_STRING
						+ SAMPLE_STRING
						+ SAMPLE_STRING
						);
					row.Add("DBCLOB", SAMPLE_STRING
						+ SAMPLE_STRING
						+ SAMPLE_STRING
						+ SAMPLE_STRING
						+ SAMPLE_STRING
						+ SAMPLE_STRING
						);
					break;
					#endregion

					#region PostgreSQL
				case MonoTests.System.Data.Utils.DataBaseServer.PostgreSQL:
					row.Add("BYTEA", new byte[]	{0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
													0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
													0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
													0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0,
													0x00, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xFF, 0xF0}, 50);
					row.Add("DATE", new DateTime(2004, 8, 9));
					row.Add("TEXT", "abcdefg", 16); 
					row.Add("TIME", new Sys.TimeSpan(02,02,02));
					row.Add("TIMESTAMP", new DateTime(2004, 8, 9, 20, 30, 15, 500), 8);
					break;
					#endregion

			}
			return row;
		}
	}
}