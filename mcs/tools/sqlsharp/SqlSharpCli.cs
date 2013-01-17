//
// SqlSharpCli.cs - main driver for Mono SQL Query Command Line Interface
//                  found in mcs/tools/SqlSharp
//
//                  This program is included in Mono and is licenced under the GPL.
//                  http://www.fsf.org/licenses/gpl.html  
//
//                  For more information about Mono, 
//                  visit http://www.mono-project.com/
//
// To build SqlSharpCli.cs
// $ mcs /out:sqlsharp.exe SqlSharpCli.cs /r:System.Data.dll
//
// To run with mono:
// $ mono sqlsharp.exe
//
// To run batch commands and get the output, do something like:
// $ cat commands_example.txt | mono sqlsharp.exe -s > results.txt
//
// Author:
//    Daniel Morgan <monodanmorg@yahoo.com>
//
// (C)Copyright 2002-2004, 2008 Daniel Morgan
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;

namespace Mono.Data.SqlSharp {

	public enum FileFormat {
		Html,
		Xml,
		CommaSeparatedValues,
		TabSeparated,
		Normal
	}

	// SQL Sharp - Command Line Interface
	public class SqlSharpCli 
	{
		// provider supports
		private bool UseParameters = true;
		private bool UseSimpleReader = false;
	
		private IDbConnection conn = null;
                		
		private string provider = ""; // name of internal provider
		// {OleDb,SqlClient,MySql,Odbc,Oracle,
		// PostgreSql,SqlLite,Sybase,Tds} however, it
		// can be set to LOADEXTPROVIDER to load an external provider
		private string providerAssembly = "";
		// filename of assembly
		// for example: "Mono.Data.MySql"
		private string providerConnectionClass = "";
		// Connection class
		// in the provider assembly that implements the IDbConnection 
		// interface.  for example: "Mono.Data.MySql.MySqlConnection"
		Type conType;
		private StringBuilder build = null; // SQL string to build
		private string buff = ""; // SQL string buffer

		private string connectionString = "";

		private string inputFilename = "";
		private string outputFilename = "";
		private StreamReader inputFilestream = null;
		private StreamWriter outputFilestream = null;

		private string factoryName = null; 
		private DbProviderFactory factory = null;

		private FileFormat outputFileFormat = FileFormat.Html;

		private bool silent = false;
		private bool showHeader = true;

		private Hashtable internalVariables = new Hashtable();
				
		// DisplayResult - used to Read() display a result set
		//                   called by DisplayData()

		public bool DisplayResult (IDataReader reader, DataTable schemaTable)
		{
			StringBuilder column = null;
			StringBuilder line = null;
			StringBuilder hdrUnderline = null;
			string outData = "";
			int hdrLen = 0;
			
			int spacing = 0;
			int columnSize = 0;
			int c;
			
			char spacingChar = ' '; // a space
			char underlineChar = '='; // an equal sign

			string dataType; // .NET Type
			Type theType; 
			DataRow row; // schema row

			line = new StringBuilder ();
			hdrUnderline = new StringBuilder ();
			
			OutputLine ("");
			
			for (c = 0; c < reader.FieldCount; c++) {
				try {			
					DataRow schemaRow = schemaTable.Rows [c];
					string columnHeader = reader.GetName (c);
					if (columnHeader.Equals (""))
						columnHeader = "column";
					if (columnHeader.Length > 32)
						columnHeader = columnHeader.Substring (0,32);
					
					// spacing
					columnSize = (int) schemaRow ["ColumnSize"];
					theType = reader.GetFieldType (c);
					dataType = theType.ToString ();

					switch (dataType) {
					case "System.DateTime":
						columnSize = 25;
						break;
					case "System.Boolean":
						columnSize = 5;
						break;
					case "System.Byte":
						columnSize = 1;
						break;
					case "System.Single":
						columnSize = 12;
						break;
					case "System.Double":
						columnSize = 21;
						break;
					case "System.Int16":
					case "System.Unt16":
						columnSize = 5;
						break;
					case "System.Int32":
					case "System.UInt32":
						columnSize = 10;
						break;
					case "System.Int64":
						columnSize = 19;
						break;
					case "System.UInt64":
						columnSize = 20;
						break;
					case "System.Decimal":
						columnSize = 29;
						break;
					}

					if (columnSize < 0)
						columnSize = 32;
					if (columnSize > 32)
						columnSize = 32;

					hdrLen = columnHeader.Length;
					if (hdrLen < 0)
						hdrLen = 0;
					if (hdrLen > 32)
						hdrLen = 32;

					hdrLen = System.Math.Max (hdrLen, columnSize);

					line.Append (columnHeader);
					if (columnHeader.Length < hdrLen) {
						spacing = hdrLen - columnHeader.Length;
						line.Append (spacingChar, spacing);
					}
					hdrUnderline.Append (underlineChar, hdrLen);

					line.Append (" ");
					hdrUnderline.Append (" ");
				}
				catch (Exception e) {
					OutputLine ("Error: Unable to display header: " + e.Message);
					return false;
				}
			}
			OutputHeader (line.ToString ());
			line = null;
			
			OutputHeader (hdrUnderline.ToString ());
			OutputHeader ("");
			hdrUnderline = null;		
								
			int numRows = 0;

			// column data
			try {
				while (reader.Read ()) {
					numRows++;
				
					line = new StringBuilder ();
					for(c = 0; c < reader.FieldCount; c++) {
						int dataLen = 0;
						string dataValue = "";
						column = new StringBuilder ();
						outData = "";
					
						row = schemaTable.Rows [c];
						string colhdr = (string) reader.GetName (c);
						if (colhdr.Equals (""))
							colhdr = "column";
						if (colhdr.Length > 32)
							colhdr = colhdr.Substring (0, 32);

						columnSize = (int) row ["ColumnSize"];
						theType = reader.GetFieldType (c);
						dataType = theType.ToString ();

						switch (dataType) {
						case "System.DateTime":
							columnSize = 25;
							break;
						case "System.Boolean":
							columnSize = 5;
							break;
						case "System.Byte":
							columnSize = 1;
							break;
						case "System.Single":
							columnSize = 12;
							break;
						case "System.Double":
							columnSize = 21;
							break;
						case "System.Int16":
						case "System.Unt16":
							columnSize = 5;
							break;
						case "System.Int32":
						case "System.UInt32":
							columnSize = 10;
							break;
						case "System.Int64":
							columnSize = 19;
							break;
						case "System.UInt64":
							columnSize = 20;
							break;
						case "System.Decimal":
							columnSize = 29;
							break;
						}

						if (columnSize < 0)
							columnSize = 32;
						if (columnSize > 32)
							columnSize = 32;

						hdrLen = colhdr.Length;
						if (hdrLen < 0)
							hdrLen = 0;
						if (hdrLen > 32)
							hdrLen = 32;

						columnSize = System.Math.Max (colhdr.Length, columnSize);

						dataValue = "";
						dataLen = 0;

						if (!reader.IsDBNull (c)) {
							object o = reader.GetValue (c);
							if (o.GetType ().ToString ().Equals ("System.Byte[]"))
								dataValue = GetHexString ( (byte[]) o);
							else
								dataValue = o.ToString ();

							dataLen = dataValue.Length;
							
							if (dataLen <= 0) {
								dataValue = "";
								dataLen = 0;
							}
							if (dataLen > 32) {
								dataValue = dataValue.Substring (0, 32);
								dataLen = 32;
							}

							if (dataValue.Equals(""))
								dataLen = 0;
						}
						columnSize = System.Math.Max (columnSize, dataLen);
					
						if (dataLen < columnSize) {
							switch (dataType) {
							case "System.Byte":
							case "System.SByte":
							case "System.Int16":
							case "System.UInt16":
							case "System.Int32":
							case "System.UInt32":
							case "System.Int64":
							case "System.UInt64":
							case "System.Single":
							case "System.Double":
							case "System.Decimal":
								outData = dataValue.PadLeft (columnSize);
								break;
							default:
								outData = dataValue.PadRight (columnSize);
								break;
							}
						}
						else
							outData = dataValue;

						line.Append (outData);
						line.Append (" ");
					}
					OutputData (line.ToString ());
				}
			}
			catch (Exception rr) {
				OutputLine ("Error: Unable to read next row: " + rr.Message);
				return false;
			}
		
			OutputLine ("\nRows retrieved: " + numRows.ToString ());

			return true; // return true - success
		}

		public static string GetHexString (byte[] bytes) 
		{ 			
			string bvalue = "";
			
			if (bytes.Length > 0) {
				StringBuilder sb = new StringBuilder ();

				for (int z = 0; z < bytes.Length; z++)
					sb.AppendFormat("{0:X2}", bytes [z]);

				bvalue = "0x" + sb.ToString ();
			}
	
			return bvalue;
		}
		
		public void OutputDataToHtmlFile (IDataReader rdr, DataTable dt) 
		{        		
			StringBuilder strHtml = new StringBuilder ();

			strHtml.Append ("<html> \n <head> <title>");
			strHtml.Append ("Results");
			strHtml.Append ("</title> </head>");
			strHtml.Append ("<body>");
			strHtml.Append ("<h1> Results </h1>");
			strHtml.Append ("<table border=1>");
		
			outputFilestream.WriteLine (strHtml.ToString ());

			strHtml = new StringBuilder ();

			strHtml.Append ("<tr>");
			foreach (DataRow schemaRow in dt.Rows) {
				strHtml.Append ("<td> <b>");
				object dataObj = schemaRow ["ColumnName"];
				string sColumnName = dataObj.ToString ();
				strHtml.Append (sColumnName);
				strHtml.Append ("</b> </td>");
			}
			strHtml.Append ("</tr>");
			outputFilestream.WriteLine (strHtml.ToString ());
			strHtml = null;

			int col = 0;
			string dataValue = "";
			
			while (rdr.Read ()) {
				strHtml = new StringBuilder ();

				strHtml.Append ("<tr>");
				for (col = 0; col < rdr.FieldCount; col++) {
						
					// column data
					if (rdr.IsDBNull (col) == true)
						dataValue = "NULL";
					else {
						object obj = rdr.GetValue (col);
						dataValue = obj.ToString ();
					}
					strHtml.Append ("<td>");
					strHtml.Append (dataValue);
					strHtml.Append ("</td>");
				}
				strHtml.Append ("\t\t</tr>");
				outputFilestream.WriteLine (strHtml.ToString ());
				strHtml = null;
			}
			outputFilestream.WriteLine (" </table> </body> \n </html>");
			strHtml = null;
		}
		
		// DisplayData - used to display any Result Sets
		//                 from execution of SQL SELECT Query or Queries
		//                 called by DisplayData. 
		//                 ExecuteSql() only calls this function
		//                 for a Query, it does not get
		//                 for a Command.
		public void DisplayData (IDataReader reader) 
		{
			DataTable schemaTable = null;
			int ResultSet = 0;

			do {
				// by Default, SqlDataReader has the 
				// first Result set if any

				ResultSet++;
				OutputLine ("Display the result set " + ResultSet);
				
				schemaTable = reader.GetSchemaTable ();
				
				if (reader.FieldCount > 0) {
					// SQL Query (SELECT)
					// RecordsAffected -1 and DataTable has a reference
					OutputQueryResult (reader, schemaTable);
				}
				else if (reader.RecordsAffected >= 0) {
					// SQL Command (INSERT, UPDATE, or DELETE)
					// RecordsAffected >= 0
					Console.WriteLine ("SQL Command Records Affected: " + reader.RecordsAffected);
				}
				else {
					// SQL Command (not INSERT, UPDATE, nor DELETE)
					// RecordsAffected -1 and DataTable has a null reference
					Console.WriteLine ("SQL Command Executed.");
				}
				
				// get next result set (if anymore is left)
			} while (reader.NextResult ());
		}

		// display the result in a simple way
		// new ADO.NET providers may have not certain
		// things implemented yet, such as, TableSchema
		// support
		public void DisplayDataSimple (IDataReader reader) 
		{				
			int row = 0;
			Console.WriteLine ("Reading Data using simple reader...");
			while (reader.Read ()){
				row++;
				Console.WriteLine ("Row: " + row);
				for (int col = 0; col < reader.FieldCount; col++) {
					int co = col + 1;
					Console.WriteLine ("  Field: " + co);
					
					string dname = (string) reader.GetName (col);
					if (dname == null)
						dname = "?column?";
					if (dname.Equals (String.Empty))
						dname = "?column?";
					Console.WriteLine ("      Name: " + dname);

					string dvalue = "";
					if (reader.IsDBNull (col))
						dvalue = "(null)";
					else
						dvalue = reader.GetValue (col).ToString ();
					Console.WriteLine ("      Value: " + dvalue);
				}
			}
			Console.WriteLine ("\n" + row + " ROWS RETRIEVED\n");
		}

		public void OutputQueryResult (IDataReader dreader, DataTable dtable) 
		{
			if (outputFilestream == null) {
				DisplayResult (dreader, dtable);
			}
			else {
				switch (outputFileFormat) {
				case FileFormat.Normal:
					DisplayResult (dreader, dtable);
					break;
				case FileFormat.Html:
					OutputDataToHtmlFile (dreader, dtable);
					break;
				default:
					Console.WriteLine ("Error: Output data file format not supported.");
					break;
				}
			}
		}

		public void BuildParameters (IDbCommand cmd) 
		{
			if (UseParameters == true) {

				ParametersBuilder parmsBuilder = new ParametersBuilder (cmd, BindVariableCharacter.Colon);
			
				Console.WriteLine ("Get Parameters (if any)...");
				parmsBuilder.ParseParameters ();
				IList parms = (IList) cmd.Parameters;
		
				Console.WriteLine ("Print each parm...");
				for (int p = 0; p < parms.Count; p++) {
					string theParmName;

					IDataParameter prm = (IDataParameter) parms[p];
					theParmName = prm.ParameterName;
				
					string inValue = "";
					bool found;
					if (parmsBuilder.ParameterMarkerCharacter == '?') {
						Console.Write ("Enter Parameter " + 
							(p + 1).ToString() +
							": ");
						inValue = Console.ReadLine();
						prm.Value = inValue;
					}
					else {
						found = GetInternalVariable (theParmName, out inValue);
						if (found == true) {
							prm.Value = inValue;
						}
						else {
							Console.Write ("Enter Parameter " + (p + 1).ToString () +
								": " + theParmName + ": ");
							inValue = Console.ReadLine ();
							prm.Value = inValue;
						}
					}
				}
				parmsBuilder = null;
			}
		}

		// ExecuteSql - Execute the SQL Command(s) and/or Query(ies)
		public void ExecuteSql (string sql) 
		{
			string msg = "";

			IDbCommand cmd = null;
			IDataReader reader = null;

			cmd = conn.CreateCommand();

			// set command properties
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Connection = conn;

			BuildParameters (cmd);

			try {
				reader = cmd.ExecuteReader ();

				if (UseSimpleReader == false)
					DisplayData (reader);
				else
					DisplayDataSimple (reader);

				reader.Close ();
				reader = null;
			}
			catch (Exception e) {
				msg = "Error: " + e.Message;
				Console.WriteLine (msg);
				reader = null;
			}
			finally {
				cmd = null;
			}
		}

		// ExecuteSql - Execute the SQL Commands (no SELECTs)
		public void ExecuteSqlNonQuery (string sql) 
		{
			string msg = "";

			IDbCommand cmd = null;
			int rowsAffected = -1;
			
			cmd = conn.CreateCommand();

			// set command properties
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Connection = conn;

			BuildParameters(cmd);

			try {
				rowsAffected = cmd.ExecuteNonQuery ();
				cmd = null;
				Console.WriteLine ("Rows affected: " + rowsAffected);
			}
			catch(Exception e) {
				msg = "Error: " + e.Message;
				Console.WriteLine (msg);
			}
			finally {
				cmd = null;
			}
		}

		public void ExecuteSqlScalar(string sql) 
		{
			string msg = "";

			IDbCommand cmd = null;
			string retrievedValue = "";
			
			cmd = conn.CreateCommand ();

			// set command properties
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Connection = conn;

			BuildParameters(cmd);

			try {
				retrievedValue = (string) cmd.ExecuteScalar ().ToString ();
				Console.WriteLine ("Retrieved value: " + retrievedValue);
			}
			catch(Exception e) {
				msg = "Error: " + e.Message;
				Console.WriteLine (msg);
			}
			finally {
				cmd = null;
			}
		}

		public void ExecuteSqlXml(string sql, string[] parms) 
		{
			string filename = "";

			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters");
				return;
			}
			try {
				filename = parms [1];
			}
			catch (Exception e) {
				Console.WriteLine ("Error: Unable to setup output results file. " + e.Message);
				return;
			}

			try {	
				IDbCommand cmd = null;
				
				cmd = conn.CreateCommand ();

				// set command properties
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = sql;
				cmd.Connection = conn;

				BuildParameters (cmd);
				DataSet dataSet = new DataSet ();
				DbDataAdapter adapter = CreateNewDataAdapter (cmd, conn);
				adapter.Fill (dataSet);	
				dataSet.WriteXml (filename);
				OutputLine ("Data written to xml file: " + filename);
			}
			catch (Exception exexml) {
				Console.WriteLine ("Error: Execute SQL XML Failure: " + exexml);
			}
		}

		public DbDataAdapter CreateNewDataAdapter (IDbCommand command, IDbConnection connection) 
		{
			DbDataAdapter adapter = null;

			if (factory != null) {
		                adapter = factory.CreateDataAdapter();
            			DbCommand cmd = (DbCommand) command;
                 		adapter.SelectCommand = cmd;
			}
			else {
				switch(provider) {
				case "OLEDB":
					adapter = (DbDataAdapter) new OleDbDataAdapter ();
					break;
				case "SQLCLIENT":
					adapter = (DbDataAdapter) new SqlDataAdapter ();
					break;
				case "LOADEXTPROVIDER":
					adapter = CreateExternalDataAdapter (command, connection);
					if (adapter == null)
						return null;
					break;
				default:
					Console.WriteLine("Error: Data Adapter not found in provider.");
					return null;
				}
			
				IDbDataAdapter dbAdapter = (IDbDataAdapter) adapter;
				dbAdapter.SelectCommand = command;
			}
			return adapter;
		}

		public DbDataAdapter CreateExternalDataAdapter (IDbCommand command, IDbConnection connection) 
		{
			DbDataAdapter adapter = null;

			Assembly ass = Assembly.Load (providerAssembly); 
			Type [] types = ass.GetTypes (); 
			foreach (Type t in types) { 
				if (t.IsSubclassOf (typeof (System.Data.Common.DbDataAdapter))) {
					if (t.Namespace.Equals (conType.Namespace))
						adapter = (DbDataAdapter) Activator.CreateInstance (t);
				}
			}
                        
			return adapter;
		}

		// like ShowHelp - but only show at the beginning
		// only the most important commands are shown
		// like help and quit
		public void StartupHelp () 
		{
			OutputLine (@"Type:  \Q to quit");
			OutputLine (@"       \ConnectionString to set the ConnectionString");
			OutputLine (@"       \Provider to set the Provider:");
			OutputLine (@"                 {OleDb,SqlClient,MySql,Odbc,DB2,");
			OutputLine (@"                  Oracle,PostgreSql,Sqlite,Sybase,Tds)");
			OutputLine (@"       \Open to open the connection");
			OutputLine (@"       \Close to close the connection");
			OutputLine (@"       \e to execute SQL query (SELECT)");
			OutputLine (@"       \h to show help (all commands).");
			OutputLine (@"       \defaults to show default variables.");
			OutputLine ("");
		}
		
		// ShowHelp - show the help - command a user can enter
		public void ShowHelp () 
		{
			Console.WriteLine ("");
			Console.WriteLine (@"Type:  \Q to quit");
			Console.WriteLine (@"       \ListP or \ListProviders to get factory providers");
			Console.WriteLine (@"       \CS or \ConnectionString to set the ConnectionString");
			Console.WriteLine (@"       \BCS to Build Connection String");
			Console.WriteLine (@"       \P or \Provider to set the Provider:");
			Console.WriteLine (@"                 {OleDb,SqlClient,MySql,Odbc,");
			Console.WriteLine (@"                  Oracle,PostgreSql,Sqlite,Sybase,Firebird}");
			Console.WriteLine (@"       \Open to open the connection");
			Console.WriteLine (@"       \Close to close the connection");
			Console.WriteLine (@"       \e to execute SQL query (SELECT)");
			Console.WriteLine (@"       \exenonquery to execute an SQL non query (not a SELECT).");
			Console.WriteLine (@"       \exescalar to execute SQL to get a single row and single column.");
			Console.WriteLine (@"       \exexml FILENAME to execute SQL and save output to XML file.");
			if (!WaitForEnterKey ())
				return;
			Console.WriteLine (@"       \f FILENAME to read a batch of SQL# commands from file.");
			Console.WriteLine (@"       \o FILENAME to write result of commands executed to file.");
			Console.WriteLine (@"       \load FILENAME to load from file SQL commands into SQL buffer.");
			Console.WriteLine (@"       \save FILENAME to save SQL commands from SQL buffer to file.");
			Console.WriteLine (@"       \h to show help (all commands).");
			Console.WriteLine (@"       \defaults to show default variables, such as,");
			Console.WriteLine (@"            Provider and ConnectionString.");
			Console.WriteLine (@"       \s {TRUE, FALSE} to silent messages.");
			Console.WriteLine (@"       \r to reset or clear the query buffer.");
			if (!WaitForEnterKey ())
				return;
			Console.WriteLine (@"       \set NAME VALUE to set an internal variable.");
			Console.WriteLine (@"       \unset NAME to remove an internal variable.");
			Console.WriteLine (@"       \variable NAME to display the value of an internal variable.");
			Console.WriteLine (@"       \loadextprovider ASSEMBLY CLASS to load the provider"); 
			Console.WriteLine (@"            use the complete name of its assembly and");
			Console.WriteLine (@"            its Connection class.");
			Console.WriteLine (@"       \print - show what's in the SQL buffer now.");
			Console.WriteLine (@"       \UseParameters (TRUE,FALSE) to use parameters when executing SQL.");
			Console.WriteLine (@"       \UseSimpleReader (TRUE,FALSE) to use simple reader when displaying results.");
			Console.WriteLine ();
		}

		public bool WaitForEnterKey () 
		{
                        Console.Write("Waiting... Press Enter key to continue. ");
			string entry = Console.ReadLine();
			if (entry.ToUpper() == "Q")
				return false;
			return true;
		}

		// ShowDefaults - show defaults for connection variables
		public void ShowDefaults() 
		{
			Console.WriteLine ();
			if (provider.Equals (String.Empty) && factory == null)
				Console.WriteLine ("Provider is not set.");
			else if(factory != null) {
				Console.WriteLine ("The default Provider is " + factoryName);
			}
			else {
				Console.WriteLine ("The default Provider is " + provider);
				if (provider.Equals ("LOADEXTPROVIDER")) {
					Console.WriteLine ("  Assembly: " + providerAssembly);
					Console.WriteLine ("  Connection Class: " + providerConnectionClass);
				}
			}
			Console.WriteLine ();
			if (connectionString.Equals (""))
				Console.WriteLine ("ConnectionString is not set.");
			else {
				Console.WriteLine ("The default ConnectionString is: ");
				Console.WriteLine ("    \"" + connectionString + "\"");
				Console.WriteLine ();
			}
		}

		// OpenDataSource - open connection to the data source
		public void OpenDataSource () 
		{
			string msg = "";

			if (factoryName.Equals(String.Empty) && provider.Equals(String.Empty)) {
				Console.Error.WriteLine("Provider not set.");
				return;
			}

			if (IsOpen()) {
				Console.Error.WriteLine("Error: already connected.");
				return;
			}
			
			OutputLine ("Opening connection...");

			try {
				if (!factoryName.Equals(String.Empty))
					conn = factory.CreateConnection();
				else {
					switch (provider) {
					case "OLEDB":
						conn = new OleDbConnection ();
						break;
					case "SQLCLIENT":
						conn = new SqlConnection ();
						break;
					case "LOADEXTPROVIDER":
						if (LoadExternalProvider () == false)
							return;
						break;
					default:
						Console.WriteLine ("Error: Bad argument or provider not supported.");
						return;
					}
				}
			} catch (Exception e) {
				msg = "Error: Unable to create Connection object because: " + e.Message;
				Console.WriteLine (msg);
				return;
			}

			conn.ConnectionString = connectionString;
			
			try {
				conn.Open ();
				if (conn.State == ConnectionState.Open)
					OutputLine ("Open was successfull.");
			} catch (Exception e) {
				msg = "Exception Caught Opening. " + e.Message;
				Console.WriteLine (msg);
				conn = null;
			}
		}

		// CloseDataSource - close the connection to the data source
		public void CloseDataSource () {
			string msg = "";
			
			if (conn != null) {
				OutputLine ("Attempt to Close...");
				try {
					conn.Close ();
					OutputLine ("Close was successfull.");
				} catch(Exception e) {
					msg = "Exeception Caught Closing. " + e.Message;
					Console.WriteLine (msg);
				}
				conn = null;
			}
		}

		public bool IsOpen () {
			if (conn != null)
				if (conn.State.Equals(ConnectionState.Open))
					return true;
			return false;
		}

		// ChangeProvider - change the provider string variable
		public void ChangeProvider (string[] parms) {

			if (IsOpen()) {
				Console.Error.WriteLine("Error: already connected.");
				return;
			}

			factory = null;
			factoryName = null;
			connectionString = "";
			provider = "";

			if (parms.Length == 2) {
				string parm = parms [1].ToUpper ();
				switch (parm) {
				case "ORACLE":
				case "ORACLECLIENT":
				case "SYSTEM.DATA.ORACLECLIENT":
					factoryName = "SYSTEM.DATA.ORACLECLIENT";
					break;
				case "SYBASE":
				case "MONO.DATA.SYBASECLIENT":
					factoryName = "MONO.DATA.SYBASECLIENT";
					break;
				case "BYTEFX":
				case "MYSQL":
				case "MYSQL.DATA.MYSQLCLIENT":
					factoryName = "MYSQL.DATA.MYSQLCLIENT";
					break;
				case "SQLITE":
				case "MONO.DATA.SQLITE":
					factoryName = "MONO.DATA.SQLITE";
					break;
				case "ODBC": 
				case "SYSTEM.DATA.ODBC":
					factoryName = "SYSTEM.DATA.ODBC";
					break;
				case "OLEDB":
				case "SYSTEM.DATA.OLEDB":
					factoryName = "SYSTEM.DATA.OLEDB";
					break;
				case "FIREBIRD":
				case "FIREBIRDSQL.DATA.FIREBIRD":
					factoryName = "FIREBIRDSQL.DATA.FIREBIRD";
					break;
				case "POSTGRESQL":
				case "NPGSQL":
				case "NPGSQL.DATA":
					factoryName = "NPGSQL.DATA";
					break;
				case "SQLCLIENT":
				case "SYSTEM.DATA.SQLCLIENT":
					factoryName = "SYSTEM.DATA.SQLCLIENT";
					break;
				default:
					Console.WriteLine ("Error: " + "Bad argument or Provider not supported.");
					return;
				}
				try {
					factory = DbProviderFactories.GetFactory(factoryName);
				} catch(ConfigurationException) {
					Console.Error.WriteLine("*** Error: Unable to load provider factory: " + 
						factoryName + "\n" + 
						"*** Check your machine.config to see if the provider is " +
						"listed under section system.data and DbProviderFactories " +
						"and that your provider assembly is in the GAC.  Your provider " +
						"may not support ADO.NET 2.0 factory and other features yet.");
					factoryName = null;
					ChangeProviderBackwardsCompat (parms);
					return;
				}
				OutputLine ("The default Provider is " + factoryName);
			}
			else
				Console.WriteLine ("Error: provider only has one parameter.");
		}

		public void ChangeProviderBackwardsCompat (string[] parms) 
		{
			Console.Error.WriteLine ("*** Setting provider using Backwards Compatibility mode.");

			string[] extp;

			if (parms.Length == 2) {
				string parm = parms [1].ToUpper ();
				switch (parm) {
				case "ORACLE":
					extp = new string[3] {
								     "\\loadextprovider",
								     @"System.Data.OracleClient, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
								     "System.Data.OracleClient.OracleConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "TDS":
					extp = new string[3] {
								     "\\loadextprovider",
								     @"Mono.Data.TdsClient, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756",
								     "Mono.Data.TdsClient.TdsConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "SYBASE":
					extp = new string[3] {
								     "\\loadextprovider",
								     @"Mono.Data.SybaseClient, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756",
								     "Mono.Data.SybaseClient.SybaseConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "BYTEFX":
					extp = new string[3] {
								     "\\loadextprovider",
								     @"ByteFX.Data, Version=0.7.6.1, Culture=neutral, PublicKeyToken=0738eb9f132ed756",
								     "ByteFX.Data.MySqlClient.MySqlConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "MYSQL":
				case "MYSQLNET":
					extp = new string[3] {
								     "\\loadextprovider",
								     @"MySql.Data, Version=1.0.7.30073, Culture=neutral, PublicKeyToken=8e323390df8d9ed4",
								     "MySql.Data.MySqlClient.MySqlConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "SQLITE":
					extp = new string[3] {
								     "\\loadextprovider",
								     @"Mono.Data.SqliteClient, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756",
								     "Mono.Data.SqliteClient.SqliteConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = true;
					break;
				case "SQLCLIENT":
					UseParameters = false;
					UseSimpleReader = false;
					provider = parm;
					break;
				case "ODBC": // for MS NET 1.1 and above
					extp = new string[3] {
								     "\\loadextprovider",
								     @"System.Data, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
								     "System.Data.Odbc.OdbcConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "MSODBC": // for MS NET 1.0
					extp = new string[3] {
								     "\\loadextprovider",
								     @"Microsoft.Data.Odbc, Culture=neutral, PublicKeyToken=b77a5c561934e089, Version=1.0.3300.0",
								     "Microsoft.Data.Odbc.OdbcConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "OLEDB":
					UseParameters = false;
					UseSimpleReader = true;
					provider = parm;
					break;
				case "FIREBIRD":
					extp = new string[3] {
								     "\\loadextprovider",
								     @"FirebirdSql.Data.Firebird, Version=1.7.1.0, Culture=neutral, PublicKeyToken=0706f5520aae4ff4",
								     "FirebirdSql.Data.Firebird.FbConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "POSTGRESQL":
				case "NPGSQL":
					extp = new string[3] {
								     "\\loadextprovider",
								     @"Npgsql, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7",
								     "Npgsql.NpgsqlConnection"};
					SetupExternalProvider (extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				default:
					Console.WriteLine ("Error: " + "Bad argument or Provider not supported.");
					break;
				}
				OutputLine ("The default Provider is " + provider);
				if (provider.Equals ("LOADEXTPROVIDER")) {
					OutputLine ("          Assembly: " + 
						providerAssembly);
					OutputLine ("  Connection Class: " + 
						providerConnectionClass);
				}
			}
			else
				Console.WriteLine ("Error: provider only has one parameter.");
		}

		// ChangeConnectionString - change the connection string variable
		public void ChangeConnectionString (string[] parms, string entry) 
		{		
			if (parms.Length >= 2) 
				connectionString = entry.Substring (parms[0].Length, entry.Length - (parms[0].Length + 1));
			else
				connectionString = "";
		}

		public void SetupOutputResultsFile (string[] parms) {
			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters");
				return;
			}
			try {
				outputFilestream = new StreamWriter (parms[1]);
			}
			catch (Exception e) {
				Console.WriteLine ("Error: Unable to setup output results file. " + e.Message);
				return;
			}
		}

		public void SetupInputCommandsFile (string[] parms) 
		{
			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters");
				return;
			}
			try {
				inputFilestream = new StreamReader (parms[1]);
			}
			catch (Exception e) {
				Console.WriteLine ("Error: Unable to setup input commmands file. " + e.Message);
				return;
			}	
		}

		public void LoadBufferFromFile (string[] parms) 
		{
			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters");
				return;
			}
			string inFilename = parms[1];
			try {
				StreamReader sr = new StreamReader (inFilename);
				StringBuilder buffer = new StringBuilder ();
				string NextLine;
			
				while ((NextLine = sr.ReadLine ()) != null) {
					buffer.Append (NextLine);
					buffer.Append ("\n");
				}
				sr.Close ();
				buff = buffer.ToString ();
				build = null;
				build = new StringBuilder ();
				build.Append(buff);
			}
			catch (Exception e) {
				Console.WriteLine ("Error: Unable to read file into SQL Buffer. " + e.Message);
			}
		}

		public void SaveBufferToFile(string[] parms) 
		{
			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters");
				return;
			}
			string outFilename = parms[1];
			try {
				StreamWriter sw = new StreamWriter (outFilename);
				sw.WriteLine (buff);
				sw.Close ();
			}
			catch (Exception e) {
				Console.WriteLine ("Error: Could not save SQL Buffer to file." + e.Message);
			}
		}

		public void SetUseParameters (string[] parms) 
		{
			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters");
				return;
			}
			string parm = parms[1].ToUpper ();
			if (parm.Equals ("TRUE"))
				UseParameters = true;
			else if (parm.Equals ("FALSE"))
				UseParameters = false;
			else
				Console.WriteLine ("Error: invalid parameter.");

		}

		public void SetUseSimpleReader (string[] parms) 
		{
			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters");
				return;
			}
			string parm = parms[1].ToUpper ();
			if (parm.Equals ("TRUE"))
				UseSimpleReader = true;
			else if (parm.Equals ("FALSE"))
				UseSimpleReader = false;
			else
				Console.WriteLine ("Error: invalid parameter.");
		}

		public void SetupSilentMode (string[] parms) 
		{
			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters");
				return;
			}
			string parm = parms[1].ToUpper ();
			if (parm.Equals ("TRUE"))
				silent = true;
			else if (parm.Equals ("FALSE"))
				silent = false;
			else
				Console.WriteLine ("Error: invalid parameter.");
		}

		public void SetInternalVariable(string[] parms) 
		{
			if (parms.Length < 2) {
				Console.WriteLine ("Error: wrong number of parameters.");
				return;
			}
			string parm = parms[1];
			StringBuilder ps = new StringBuilder ();
			
			for (int i = 2; i < parms.Length; i++)
				ps.Append (parms[i]);

			internalVariables[parm] = ps.ToString ();
		}

		public void UnSetInternalVariable(string[] parms) 
		{
			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters.");
				return;
			}
			string parm = parms[1];

			try {
				internalVariables.Remove (parm);
			} catch(Exception e) {
				Console.WriteLine ("Error: internal variable does not exist: " + e.Message);
			}
		}

		public void ShowInternalVariable(string[] parms) 
		{
			string internalVariableValue = "";

			if (parms.Length != 2) {
				Console.WriteLine ("Error: wrong number of parameters.");
				return;
			}
						
			string parm = parms[1];

			if (GetInternalVariable(parm, out internalVariableValue) == true)
				Console.WriteLine ("Internal Variable - Name: " + 
					parm + "  Value: " + internalVariableValue);
		}

		public bool GetInternalVariable(string name, out string sValue) 
		{
			sValue = "";
			bool valueReturned = false;

			try {
				if (internalVariables.ContainsKey (name) == true) {
					sValue = (string) internalVariables[name];
					valueReturned = true;
				}
				else
					Console.WriteLine ("Error: internal variable does not exist.");

			}
			catch(Exception e) {
				Console.WriteLine ("Error: internal variable does not exist: "+	e.Message);
			}
			return valueReturned;
		}

		public void SetupExternalProvider(string[] parms) 
		{
			if (parms.Length != 3) {
				Console.WriteLine ("Error: Wrong number of parameters.");
				return;
			}
			provider = "LOADEXTPROVIDER";
			providerAssembly = parms[1];
			providerConnectionClass = parms[2];
		}

		public bool LoadExternalProvider () 
		{
			string msg = "";
			
			bool success = false;

			// For example: for the MySQL provider in Mono.Data.MySql
			//   \LoadExtProvider Mono.Data.MySql Mono.Data.MySql.MySqlConnection
			//   \ConnectionString dbname=test
			//   \open
			//   insert into sometable (tid, tdesc, aint) values ('abc','def',12)
			//   \exenonquery
			//   \close
			//   \quit

			try {
				OutputLine ("Loading external provider...");

				Assembly ps = Assembly.Load (providerAssembly);
				conType = ps.GetType (providerConnectionClass);
				conn = (IDbConnection) Activator.CreateInstance (conType);
				success = true;
				
				OutputLine ("External provider loaded.");
				UseParameters = false;
			} catch(FileNotFoundException f) {
				msg = "Error: unable to load the assembly of the provider: " + providerAssembly + " : " + f.Message;
				Console.WriteLine(msg);
			}
			catch(Exception e) {
				msg = "Error: unable to load the assembly of the provider: " + providerAssembly + " : " + e.Message;
				Console.WriteLine(msg);
			}
			return success;
		}

		// used for outputting message, but if silent is set,
		// don't display
		public void OutputLine (string line) 
		{
			if (silent == false)
				OutputData (line);
		}

		// used for outputting the header columns of a result
		public void OutputHeader (string line) 
		{
			if (showHeader == true)
				OutputData (line);
		}

		// OutputData() - used for outputting data
		//  if an output filename is set, then the data will
		//  go to a file; otherwise, it will go to the Console.
		public void OutputData(string line) 
		{
			if (outputFilestream == null)
				Console.WriteLine (line);
			else
				outputFilestream.WriteLine (line);
		}

		// HandleCommand - handle SqlSharpCli commands entered
		public void HandleCommand (string entry) 
		{		
			string[] parms;
			
			parms = entry.Split (new char[1] {' '});
			string userCmd = parms[0].ToUpper ();

			switch (userCmd) {
			case "\\LISTPROVIDERS":
			case "\\LISTP":
				ListProviders ();
				break;
			case "\\PROVIDER":
			case "\\P":
				ChangeProvider (parms);
				break;
			case "\\CONNECTIONSTRING":
			case "\\CS":
				ChangeConnectionString (parms, entry);
				break;
			case "\\LOADEXTPROVIDER":
				SetupExternalProvider (parms);
				break;
			case "\\OPEN":
				OpenDataSource ();
				break;
			case "\\CLOSE":
				CloseDataSource ();
				break;
			case "\\S":
				SetupSilentMode (parms);
				break;
			case "\\E":
			case "\\EXEQUERY":
			case "\\EXEREADER":
			case "\\EXECUTE":
				// Execute SQL Commands or Queries
				if (conn == null)
					Console.WriteLine ("Error: connection is not Open.");
				else if (conn.State == ConnectionState.Closed)
					Console.WriteLine ("Error: connection is not Open.");
				else {
					if (build == null)
						Console.WriteLine ("Error: SQL Buffer is empty.");
					else {
						buff = build.ToString ();
						ExecuteSql (buff);
					}
					build = null;
				}
				break;
			case "\\EXENONQUERY":
				if (conn == null)
					Console.WriteLine ("Error: connection is not Open.");
				else if (conn.State == ConnectionState.Closed)
					Console.WriteLine ("Error: connection is not Open.");
				else {
					if (build == null)
						Console.WriteLine ("Error: SQL Buffer is empty.");
					else {
						buff = build.ToString ();
						ExecuteSqlNonQuery (buff);
					}
					build = null;
				}
				break;
			case "\\EXESCALAR":
				if (conn == null)
					Console.WriteLine ("Error: connection is not Open.");
				else if (conn.State == ConnectionState.Closed)
					Console.WriteLine ("Error: connection is not Open.");
				else {
					if (build == null)
						Console.WriteLine ("Error: SQL Buffer is empty.");
					else {
						buff = build.ToString ();
						ExecuteSqlScalar (buff);
					}
					build = null;
				}
				break;
			case "\\EXEXML":
				// \exexml OUTPUT_FILENAME
				if (conn == null)
					Console.WriteLine ("Error: connection is not Open.");
				else if (conn.State == ConnectionState.Closed)
					Console.WriteLine ("Error: connection is not Open.");
				else {
					if (build == null)
						Console.WriteLine ("Error: SQL Buffer is empty.");
					else {
						buff = build.ToString ();
						ExecuteSqlXml (buff, parms);
					}
					build = null;
				}
				break;
			case "\\F":
				SetupInputCommandsFile (parms);
				break;
			case "\\O":
				SetupOutputResultsFile (parms);
				break;
			case "\\LOAD":
				// Load file into SQL buffer: \load FILENAME
				LoadBufferFromFile (parms);
				break;
			case "\\SAVE":
				// Save SQL buffer to file: \save FILENAME
				SaveBufferToFile (parms);
				break;
			case "\\H":
			case "\\HELP":
				// Help
				ShowHelp ();
				break;
			case "\\DEFAULTS":
				// show the defaults for provider and connection strings
				ShowDefaults ();
				break;
			case "\\BCS":
				BuildConnectionString ();
				break;
			case "\\Q": 
			case "\\QUIT":
				// Quit
				break;
			case "\\CLEAR":
			case "\\RESET":
			case "\\R": 
				// reset (clear) the query buffer
				build = null;
				break;
			case "\\SET":
				// sets internal variable
				// \set name value
				SetInternalVariable (parms);
				break;
			case "\\UNSET":
				// deletes internal variable
				// \unset name
				UnSetInternalVariable (parms);
				break;
			case "\\VARIABLE":
				ShowInternalVariable (parms);
				break;
			case "\\PRINT":
				if (build == null)
					Console.WriteLine ("SQL Buffer is empty.");
				else
					Console.WriteLine ("SQL Bufer:\n" + buff);
				break;
			case "\\USEPARAMETERS":
				SetUseParameters (parms);
				break;
			case "\\USESIMPLEREADER":
				SetUseSimpleReader (parms);
				break;
			default:
				// Error
				Console.WriteLine ("Error: Unknown user command.");
				break;
			}
		}

		public void ListProviders() 
		{
			DataTable table = DbProviderFactories.GetFactoryClasses();
			Console.WriteLine("List of Providers:");
			for (int r = 0; r < table.Rows.Count; r++)
			{     	        
				Console.WriteLine("---------------------");
				Console.WriteLine("   Name: " + table.Rows[r][0].ToString());
				Console.WriteLine("      Description: " + table.Rows[r][1].ToString());
				Console.WriteLine("      InvariantName: " + table.Rows[r][2].ToString());
				Console.WriteLine("      AssemblyQualifiedName: " + table.Rows[r][3].ToString());
			}
			Console.WriteLine("---------------------");
			Console.WriteLine("Providers found: " + table.Rows.Count.ToString());
		}

		public void DealWithArgs(string[] args) 
		{
			for (int a = 0; a < args.Length; a++) {
				if (args[a].Substring (0,1).Equals ("-")) {
					string arg = args [a].ToUpper ().Substring (1, args [a].Length - 1);
					switch (arg) {
					case "S":
						silent = true;
						break;
					case "F":		
						if (a + 1 >= args.Length)
							Console.WriteLine ("Error: Missing FILENAME for -f switch");
						else {
							inputFilename = args [a + 1];
							inputFilestream = new StreamReader (inputFilename);
						}
						break;
					case "O":
						if (a + 1 >= args.Length)
							Console.WriteLine ("Error: Missing FILENAME for -o switch");
						else {
							outputFilename = args [a + 1];
							outputFilestream = new StreamWriter (outputFilename);
						}
						break;
					default:
						Console.WriteLine ("Error: Unknow switch: " + args [a]);
						break;
					}
				}
			}
		}

		public string GetPasswordFromConsole ()
		{
			StringBuilder pb = new StringBuilder ();
			Console.Write ("\nPassword: ");
			ConsoleKeyInfo cki = Console.ReadKey (true);

			while (cki.Key != ConsoleKey.Enter) {
				if (cki.Key == ConsoleKey.Backspace) {
					if (pb.Length > 0) {
						pb.Remove (pb.Length - 1, 1);
						Console.Write ("\b");
						Console.Write (" ");
						Console.Write ("\b");
					}
				} else {
					pb.Append (cki.KeyChar);
					Console.Write ("*");
				}
				cki = Console.ReadKey (true);
			}

			Console.WriteLine ();
			return pb.ToString ();
		}

		public string ReadSqlSharpCommand()
		{
			string entry = "";

			if (inputFilestream == null) {
				if (silent == false)
					Console.Error.Write ("\nSQL# ");
				entry = Console.ReadLine ();
			}
			else {
				try {
					entry = inputFilestream.ReadLine ();
					if (entry == null) {
						OutputLine ("Executing SQL# Commands from file done.");
					}
				}
				catch (Exception e) {
					Console.WriteLine ("Error: Reading command from file: " + e.Message);
				}
				if (silent == false)
					Console.Error.Write ("\nSQL# ");
				entry = Console.ReadLine ();
			}
			return entry;
		}

		public string ReadConnectionOption(string option, string defaultVal)
		{
			Console.Error.Write ("\nConnectionString Option: {0} [{1}] SQL# ", option, defaultVal);
			return Console.ReadLine ();
		}

		public void BuildConnectionString ()
		{
			if (factory == null) {
				Console.WriteLine("Provider is not set.");
				return;
			}

			DbConnectionStringBuilder sb = factory.CreateConnectionStringBuilder ();
			if (!connectionString.Equals(String.Empty))
				sb.ConnectionString = connectionString;

			bool found = false;
			foreach (string key in sb.Keys) {
				if (key.ToUpper().Equals("PASSWORD") || key.ToUpper().Equals("PWD")) {
					string pwd = GetPasswordFromConsole ();
					try {
						sb[key] = pwd;
					} catch(Exception e) {
						Console.Error.WriteLine("Error: unable to set key.  Reason: " + e.Message);
						return;
					}
				} else {
					string defaultVal = sb[key].ToString ();
					String val = "";
					val = ReadConnectionOption (key, defaultVal);
					if (val.ToUpper ().Equals ("\\STOP"))
						return;
					if (val != "") {
						try {
							sb[key] = val;
						} catch(Exception e) {
							Console.Error.WriteLine("Error: unable to set key.  Reason: " + e.Message);
							return;
						}
					}
				}
				found = true;
			}
			if (!found) {
				Console.Error.WriteLine("Warning: your provider does not subclass DbConnectionStringBuilder fully.");
				return;
			}
				
			connectionString = sb.ConnectionString;
			Console.WriteLine("ConnectionString is set.");
		}
		
		public void Run (string[] args) 
		{
			DealWithArgs (args);

			string entry = "";
			build = null;

			if (silent == false) {
				Console.WriteLine ("Welcome to SQL#. The interactive SQL command-line client ");
				Console.WriteLine ("for Mono.Data.  See http://www.mono-project.com/ for more details.\n");
						
				StartupHelp ();
				ShowDefaults ();
			}
			
			while (entry.ToUpper ().Equals ("\\Q") == false &&
				entry.ToUpper ().Equals ("\\QUIT") == false) {
				
				while ((entry = ReadSqlSharpCommand ()) == "") {}
			
				
				if (entry.Substring(0,1).Equals ("\\")) {
					HandleCommand (entry);
				}
				else if (entry.IndexOf(";") >= 0) {
					// most likely the end of SQL Command or Query found
					// execute the SQL
					if (conn == null)
						Console.WriteLine ("Error: connection is not Open.");
					else if (conn.State == ConnectionState.Closed)
						Console.WriteLine ("Error: connection is not Open.");
					else {
						if (build == null) {
							build = new StringBuilder ();
						}
						build.Append (entry);
						//build.Append ("\n");
						buff = build.ToString ();
						ExecuteSql (buff);
						build = null;
					}
				}
				else {
					// most likely a part of a SQL Command or Query found
					// append this part of the SQL
					if (build == null) {
						build = new StringBuilder ();
					}
					build.Append (entry + "\n");
					buff = build.ToString ();
				}
			}			
			CloseDataSource ();
			if (outputFilestream != null)
				outputFilestream.Close ();
		}
	}

	public enum BindVariableCharacter {
		Colon,         // ':'  - named parameter - :name
		At,            // '@'  - named parameter - @name
		QuestionMark,  // '?'  - positioned parameter - ?
		SquareBrackets // '[]' - delimited named parameter - [name]
	}

	public class ParametersBuilder 
	{
		private BindVariableCharacter bindCharSetting;
		private char bindChar;
		private IDataParameterCollection parms;
		private string sql;
		private IDbCommand cmd;
			
		private void SetBindCharacter () 
		{
			switch(bindCharSetting) {
			case BindVariableCharacter.Colon:
				bindChar = ':';
				break;
			case BindVariableCharacter.At:
				bindChar = '@';
				break;
			case BindVariableCharacter.SquareBrackets:
				bindChar = '[';
				break;
			case BindVariableCharacter.QuestionMark:
				bindChar = '?';
				break;
			}
		}

		public ParametersBuilder (IDbCommand command, BindVariableCharacter bindVarChar) 
		{
			cmd = command;
			sql = cmd.CommandText;
			parms = cmd.Parameters;
			bindCharSetting = bindVarChar;
			SetBindCharacter();
		}	

		public char ParameterMarkerCharacter {
			get {
				return bindChar;
			}
		}

		public int ParseParameters () 
		{	
			int numParms = 0;

			char[] chars = sql.ToCharArray ();
			bool bStringConstFound = false;

			for (int i = 0; i < chars.Length; i++) {
				if (chars[i] == '\'') {
					if (bStringConstFound == true)
						bStringConstFound = false;
					else
						bStringConstFound = true;
				}
				else if (chars[i] == bindChar && 
					bStringConstFound == false) {
					if (bindChar != '?') {
						StringBuilder parm = new StringBuilder ();
						i++;
						if (bindChar.Equals ('[')) {
							bool endingBracketFound = false;
							while (i <= chars.Length) {
								char ch;
								if (i == chars.Length)
									ch = ' '; // a space
								else
									ch = chars[i];

								if (Char.IsLetterOrDigit (ch) || ch == ' ') {
									parm.Append (ch);
								}
								else if (ch == ']') {
									endingBracketFound = true;
									string p = parm.ToString ();
									AddParameter (p);
									numParms ++;
									break;
								}
								else throw new Exception("SQL Parser Error: Invalid character in parameter name");
								i++;
							}
							i--;
							if (endingBracketFound == false)
								throw new Exception("SQL Parser Error: Ending bracket not found for parameter");
						}
						else {
							while (i <= chars.Length) {
								char ch;
								if (i == chars.Length)
									ch = ' '; // a space
								else
									ch = chars[i];

								if (Char.IsLetterOrDigit(ch)) {
									parm.Append (ch);
								}
								else {

									string p = parm.ToString ();
									AddParameter (p);
									numParms ++;
									break;
								}
								i++;
							}
							i--;
						}
					}
					else {
						// placeholder paramaeter for ?
						string p = numParms.ToString ();
						AddParameter (p);
						numParms ++;
					}
				}			
			}
			return numParms;
		}

		public void AddParameter (string p) 
		{
			Console.WriteLine ("Add Parameter: " + p);
			if (parms.Contains (p) == false) {
				IDataParameter prm = cmd.CreateParameter ();
				prm.ParameterName = p;
				prm.Direction = ParameterDirection.Input;
				prm.DbType = DbType.String; // default
				prm.Value = ""; // default
				cmd.Parameters.Add(prm);
			}
		}

	}
	
	public class SqlSharpDriver 
	{
		public static void Main (string[] args) 
		{
			SqlSharpCli sqlCommandLineEngine = new SqlSharpCli ();
			sqlCommandLineEngine.Run (args);
		}
	}
}

