//
// SqlSharpCli.cs - main driver for SqlSharp
//
//                    Currently, only working on a command line interface for SqlSharp
//
//                    However, once GTK# and System.Windows.Forms are good-to-go,
//                    I would like to create a SqlSharpGui using this.
//
//                    It would be nice if this is included as part of Mono
//                    extra goodies under Mono.Data.SqlSharp.
//
//                    Also, this makes a good Test program for Mono System.Data.
//                    For more information about Mono::, 
//                    visit http://www.go-mono.com/
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (C)Copyright 2002 Daniel Morgan
//

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Mono.Data.SqlSharp {
	
	// SQL Sharp - Command Line Interface
	public class SqlSharpCli {
	
		private IDbConnection conn = null;
		string provider = "POSTGRESCLIENT";
		// string connectionString = "server=DANPC;database=pubs;uid=sa;pwd=freetds";
		string connectionString = "host=localhost;dbname=test;user=postgres";

		public void DisplayResult(IDataReader reader, DataTable schemaTable) {

			StringBuilder line = null;
			string lineOut;

			int spacing = 0;
			int columnSize = 0;
			
			char spacingChar = ' '; // a space
			line = new StringBuilder();
			Console.WriteLine("Fields in Query Result: " + reader.FieldCount);
			
			foreach(DataRow schemaRow in schemaTable.Rows) {
				string columnHeader = (string) schemaRow["ColumnName"];
				int columnHeaderSize = columnHeader.Length;
				line.Append(columnHeader);
				line.Append(" ");
					
				// spacing
				columnSize = (int) schemaRow["ColumnSize"];
				if(columnHeaderSize < columnSize) {
					spacing = columnSize - columnHeaderSize;
					line.Append(spacingChar, spacing);
				}

			}
			lineOut = line.ToString();
			Console.WriteLine(line);
			line = null;
			line = new StringBuilder();

			// DEBUG - need to know the columnSize
			/*
			foreach(DataRow schemaRow in schemaTable.Rows) {
				columnSize = (int) schemaRow["ColumnSize"];
				line.Append(columnSize.ToString());
				line.Append(" ");
			}		
			lineOut = line.ToString();
			Console.WriteLine(line);
			Console.WriteLine("");
			line = null;
			*/
								
			int rows = 0;

			// column data
			while(reader.Read()) {
				rows++;

				line = new StringBuilder();
				for(int c = 0; c < reader.FieldCount; c++) {
					int dataLen = 0;
					string dataValue;
					string dataType;	
					DataRow row = schemaTable.Rows[c];
					string colhdr = (string) row["ColumnName"];
					columnSize = (int) row["ColumnSize"];
					dataType = (string) row["DataType"];
					if(dataType.Equals("System.Boolean")) {
						columnSize = 5;
					}
					int columnHeaderLen = colhdr.Length;
												
					if(reader.IsDBNull(c)) {
						dataValue = "";
						dataLen = 0;
					}
					else {
						object obj = reader.GetValue(c);						
							
						dataValue = obj.ToString();
						dataLen = dataValue.Length;
						line.Append(dataValue);
					}
					line.Append(" ");

					// spacing
					spacingChar = ' ';
					if(dataLen < columnSize) {
						spacing = columnSize - dataLen;
						line.Append(spacingChar, spacing);
					}
					spacingChar = ' ';
					if(columnSize < columnHeaderLen) {
						spacing = columnHeaderLen - columnSize;
						line.Append(spacingChar, spacing);
					}
						
				}
				lineOut = line.ToString();
				Console.WriteLine(lineOut);
				line = null;
			}
			Console.WriteLine("\nRows retrieved: " + rows);
		}

		public void DisplayData(IDataReader reader) {

			DataTable schemaTable;
			int ResultSet = 0;

			Console.WriteLine("Display any result sets...");

			do {
				ResultSet++;
				Console.WriteLine("Display the result set " + ResultSet);
				
				schemaTable = reader.GetSchemaTable();
				if(reader.RecordsAffected >= 0)
					Console.WriteLine("SQL Command Records Affected: " + reader.RecordsAffected);
				else if(schemaTable == null)
					Console.WriteLine("SQL Command executed.");
				else {
					DisplayResult(reader, schemaTable);
				}
			} while(reader.NextResult());
		}

		public void RunStatement(string sql, string provider) {
			Console.WriteLine("Execute SQL: " + sql);

			IDbCommand cmd = null;
			IDataReader reader = null;

			switch(provider) {
			case "POSTGRESCLIENT":
				cmd = new SqlCommand();
				break;
			default:
				Console.WriteLine("Error: PostgreSQL is only supported, and it through SqlClient.");
				return;
			}
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Connection = conn;
			try {
				reader = cmd.ExecuteReader();
				Console.WriteLine("Executed okay.");
				DisplayData(reader);
				reader.Close();
				reader = null;
				cmd = null;
			}
			catch(Exception e) {
				Console.WriteLine("Exception Caught Executing SQL: " + e);
				cmd = null;
			}
		}

		public void ShowHelp() {
			Console.WriteLine(@"Type:  \Q to quit");
			Console.WriteLine(@"       \ConnectionString to set the ConnectionString");
			Console.WriteLine(@"       \Provider to set the Provider:");
			Console.WriteLine(@"                 {OleDb,SqlClient,");
			Console.WriteLine(@"                  OracleClient,PostgresClient}");
			Console.WriteLine(@"       \Open to open the connection");
			Console.WriteLine(@"       \Close to close the connection");
			Console.WriteLine(@"       \Execute to execute SQL command(s)/queries(s)");
			Console.WriteLine(@"       \h to show this help.");
			Console.WriteLine("\nThe default Provider is " + provider);
			Console.WriteLine("\nThe default ConnectionString is: ");
			Console.WriteLine("    \"" + connectionString + "\"");
			Console.WriteLine();
		}
		
		public void Run(string[] args) {
			string entry = "";
			string[] parms;
			StringBuilder build = null;

			Console.WriteLine("Welcome to SQL#. The interactive SQL command-line client ");
			Console.WriteLine("for Mono.Data.  See http://www.go-mono.com/ for more details.\n");
			ShowHelp();
			
			while(entry.ToUpper().Equals("\\Q") == false) {
				
				Console.Write("\nSQL# ");
				entry = Console.ReadLine();

				Console.WriteLine("Entered: " + entry);
				
				if(entry.Substring(0,1).Equals("\\")) {
					// maybe a SQL# Command was found
					parms = entry.Split(new char[1] {' '});
					Console.WriteLine("Parms: " + parms.Length); 
					string userCmd = parms[0].ToUpper();
					switch(userCmd) {
					case "\\PROVIDER":
						if(parms.Length == 2) {
							string parm = parms[1].ToUpper();
							switch(parm) {
							case "OLEDB":
							case "ORACLECLIENT":
								Console.WriteLine("Error: Provider not currently supported.");
								break;
							case "SQLCLIENT":
								provider = "POSTGRESCLIENT";
								Console.WriteLine("Warning: Currently, the SqlClient provider is the PostgreSQL provider.");
								break;
							case "POSTGRESCLIENT":
								provider = parm;
								break;
							default:
								Console.WriteLine("Error: " + "Bad argument or Provider not supported.");
								break;
							}
							Console.WriteLine("Provider: " + provider);
						}
						else
							Console.WriteLine("Error: provider only has one parameter.");

						break;
					case "\\CONNECTIONSTRING":
						if(entry.Length > 18)
							connectionString = entry.Substring(18, entry.Length - 18);
						else
							connectionString = "";
						Console.WriteLine("ConnectionString: " + connectionString);
						Console.WriteLine("Lenght of ConnectionString: " + connectionString.Length);
						break;
					case "\\OPEN":
						Console.WriteLine("Attempt to Open...");
						switch(provider) {
						case "POSTGRESCLIENT":
							conn = new SqlConnection();
							break;
						default:
							Console.WriteLine("Error: Currently, PostgreSQL is the only provider supported, and it through SqlClient.");
							break;
						}
						conn.ConnectionString = connectionString;
						try {
							conn.Open();
						}
						catch(Exception e) {
							Console.WriteLine("Exception Caught Opening. " + e);
							conn = null;
						}
						if(conn.State == ConnectionState.Open)
							Console.WriteLine("Assuming Open was successfully.");
						break;
					case "\\CLOSE":
						Console.WriteLine("Attempt to Close...");
						bool bCloseError = false;
						try {
							conn.Close();
						}
						catch(Exception e) {
							Console.WriteLine("Exeception Caught Closing. " + e);
							bCloseError = true;
						}
						if(bCloseError == false)
							Console.WriteLine("Assuming Close was successfull.");
						break;
					case "\\EXECUTE":
						if(conn == null)
							Console.WriteLine("Error: connection is not Open.");
						else if(conn.State == ConnectionState.Closed)
							Console.WriteLine("Error: connection is not Open.");
						else {
							if(build == null)
								Console.WriteLine("Error: SQL Buffer is empty.");
							else {
								string builtSQL = build.ToString();
								Console.WriteLine("SQL: " + builtSQL);

								RunStatement(builtSQL, provider);
							}
							build = null;
						}
						break;
					case "\\H":
						ShowHelp();
						break;
					case "\\Q":
						break;
					default:
						Console.WriteLine("Error: Unknown user command.");
						break;
					}
				
				}
				else if(entry.IndexOf(";") >= 0) {
					// most likely the end of SQL Command or Query found
					// execute the SQL
					if(conn == null)
						Console.WriteLine("Error: connection is not Open.");
					else if(conn.State == ConnectionState.Closed)
						Console.WriteLine("Error: connection is not Open.");
					else {
						Console.WriteLine("if build == null");
						if(build == null) {
							Console.WriteLine("build is null, do new");
							build = new StringBuilder();
						}
						Console.WriteLine("append entry");
						build.Append(entry);
						Console.WriteLine("build: " + build.ToString());
						Console.WriteLine("RunStatement");
						RunStatement(build.ToString(), provider);
				
						build = null;
					}
				}
				else {
					// most likely a part of a SQL Command or Query found
					// append this part of the SQL
					if(build == null) {
						build = new StringBuilder();
					}
					build.Append(entry + " ");
					Console.WriteLine("build: " + build.ToString());
				}
			}			
		}
	}

	public class SqlSharpDriver {

		public static void Main(string[] args) {
			SqlSharpCli sqlCommandLineEngine = new SqlSharpCli();
			sqlCommandLineEngine.Run(args);
		}
	}
}
