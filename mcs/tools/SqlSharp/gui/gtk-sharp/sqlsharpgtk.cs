//
// SqlSharpGtk - SQL# GUI for GTK# - SQL Query and Configuration tool for 
//               Mono.Data providers
//
//               Based on SQL# CLI (Command Line Interface)
//               and the ConsoleGtk widget in MonoLOGO (by Rachel Hestilow),
//               the GnomeDbSqlEditor, GnomeDbBrowser, GnomeDbGrid widgets 
//               and others in GnomeDb.
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//
// (C)Copyright 2002 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//
//#define DEBUG

namespace Mono.Data.SqlSharp.Gui.GtkSharp {
	using System;
	using System.Collections;
	using System.Data;
	using System.Data.Common;
	using System.Data.Odbc;
	using System.Data.OleDb;
	using System.Data.SqlClient;
	using System.Drawing;
	using System.Text;
	using System.IO;
	using Gtk;
	using GtkSharp;
	using SqlEditorSharp;
	using System.Reflection;
	using System.Runtime.Remoting;
	using System.Diagnostics;
	using Gtk.Controls;

	public class SqlSharpGtk {
		// these will be moved once a SqlSharpWindow has been created
		private IDbConnection conn = null;
		public TextBuffer buf;
		private TextView textView;
		private TextTag textTag;
		public DbProvider dbProvider;
		public string connectionString;
		private SqlEditorSharp editor;
		private ScrolledWindow swin;
		private Statusbar statusBar;
		private Toolbar toolbar;
		private DataGrid grid;
		private Window win;

		public DbProviderCollection providerList;

		public SqlSharpGtk () {

			CreateGui ();
			LoadProviders ();
		}

		public void CreateGui() {

			win = new Window ("SQL# For GTK#");
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			win.BorderWidth = 4;
			win.DefaultSize = new Size (450, 300);
			
			VBox vbox = new VBox (false, 4);
			win.Add (vbox);
			
			// Menu Bar
			MenuBar mb = CreateMenuBar ();
			vbox.PackStart(mb, false, false, 0);

			// Tool Bar
			toolbar = CreateToolbar ();
			vbox.PackStart (toolbar, false, false, 0);
			
			// Panels
			VPaned paned = new VPaned ();
			vbox.PackStart (paned, true, true, 0);

			// SQL Editor (top TextView)
			editor = new SqlEditorSharp ();
			paned.Add1 (editor);

			// Output Results (bottom TextView)
			swin = CreateOutputResultsTextView ();
			//paned.Add2 (swin);

			grid = CreateOutputResultsDataGrid ();
			paned.Add2 (grid);

			statusBar = new Statusbar ();
			vbox.PackEnd (statusBar, false, false, 0);

			win.ShowAll ();
		}

		DataGrid CreateOutputResultsDataGrid () {
			DataGrid dataGrid = null;

			dataGrid = new DataGrid ();

			return dataGrid;
		}

		ScrolledWindow CreateOutputResultsTextView () {
			ScrolledWindow sw;
			sw = new ScrolledWindow (
				new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0), 
				new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0));
			sw.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.ShadowType = Gtk.ShadowType.In;
			
			// create text tag table for font "courier"
			// to be applied to the output TextView
			TextTagTable textTagTable = new TextTagTable ();
			textTag = new TextTag ("normaltext");
			textTag.Family = "courier";
			textTag.Foreground = "black";
			textTagTable.Add (textTag);

			buf = new TextBuffer (textTagTable);
			textView = new TextView (buf);
			textView.Editable = false;
			sw.Add (textView);		

			return sw;
		}

		Toolbar CreateToolbar () {
			Toolbar toolbar = new Toolbar ();

			toolbar.AppendItem ("Execute", 
				"Execute SQL Commands.", String.Empty,
				new Gtk.Image (Stock.Execute, IconSize.LargeToolbar),
				new SignalFunc (execute_func));	

			toolbar.AppendSpace ();		

			toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;

			return toolbar;
		}

		public void LoadProviders () {
			providerList = new DbProviderCollection ();
			
			providerList.Add (new DbProvider (
				"MYSQL",
				"MySQL",
				"Mono.Data.MySql",
				"Mono.Data.MySql.MySqlConnection",
				"Mono.Data.MySql.MySqlAdapter",
				false ));
			providerList.Add (new DbProvider (
				"POSTGRESQL",
				"PostgreSQL",
				"Mono.Data.PostgreSqlClient",
				"Mono.Data.PostgreSqlClient.PgSqlConnection",
				"Mono.Data.PostgreSqlClient.PgSqlAdapter",
				false ));
			providerList.Add (new DbProvider (
				"SQLCLIENT",
				"Microsoft SQL Server",
				"",
				"",
				"",
				true ));
			providerList.Add (new DbProvider (
				"TDS",
				"TDS Generic",
				"Mono.Data.TdsClient",
				"Mono.Data.TdsClient.TdsConnection",
				"Mono.Data.TdsClient.TdsAdapter",
				false ));
			providerList.Add (new DbProvider (
				"ODBC",
				"ODBC",
				"",
				"",
				"",
				true ));
			providerList.Add (new DbProvider (
				"SQLITE",
				"SQL Lite",
				"Mono.Data.SqliteClient",
				"Mono.Data.SqliteClient.SqliteConnection",
				"Mono.Data.SqliteClient.SqliteAdapter",
				false ));
			providerList.Add (new DbProvider (
				"ORACLE",
				"Oracle 8i/9i",
				"System.Data.OracleClient",
				"System.Data.OracleClient.OracleConnection",
				"System.Data.OracleClient.OracleAdapter",
				false ));
			providerList.Add (new DbProvider (
				"SYBASE",
				"Sybase",
				"Mono.Data.SybaseClient",
				"Mono.Data.SybaseClient.SybaseConnection",
				"Mono.Data.SybaseClient.SybaseAdapter",
				false ));

		}

		public MenuBar CreateMenuBar () {

			MenuBar mb = new MenuBar ();
			Menu file_menu = new Menu ();

			MenuItem connect_item = new MenuItem ("Connect");
			connect_item.Activated += new EventHandler (connect_cb);
			file_menu.Append (connect_item);

			MenuItem disconnect_item = new MenuItem ("Disconnect");
			disconnect_item.Activated += new EventHandler (disconnect_cb);
			file_menu.Append (disconnect_item);

			MenuItem execute_item = new MenuItem ("Execute");
			execute_item.Activated += new EventHandler (execute_cb);
			file_menu.Append (execute_item);

			MenuItem exit_item = new MenuItem ("Exit");
			exit_item.Activated += new EventHandler (exit_cb);
			file_menu.Append (exit_item);

			MenuItem file_item = new MenuItem ("File");
			file_item.Submenu = file_menu;
			mb.Append (file_item);

			return mb;
		}

		// WriteLine() to output text to bottom TextView
		// for displaying result sets and messages
		public void AppendText (TextBuffer buffer, string text)	{
		
			TextIter iter;
			int char_count = 0;

			// if text not empty, output text
			if (!text.Equals ("")) {				
				char_count = buffer.CharCount;
				char_count = Math.Max (0, char_count - 1);
				buffer.GetIterAtOffset (out iter, char_count);
				buffer.Insert (iter, text, -1);
			}
			// output a new line
			char_count = buffer.CharCount;
			char_count = Math.Max (0, char_count - 1);
			buffer.GetIterAtOffset (out iter, char_count);
			buffer.Insert (iter, "\n", -1);

			// format text to "courier" font family
			TextIter start_iter, end_iter;
			buffer.GetIterAtOffset (out start_iter, 0);
			char_count = buffer.CharCount;
			char_count = Math.Max (0, char_count - 1);
			buffer.GetIterAtOffset (out end_iter, char_count);
			buffer.ApplyTagByName ("normaltext", start_iter, end_iter);

			// scroll text into view
			TextMark mark;
			mark = buf.InsertMark;
			textView.ScrollMarkOnscreen (mark);
		}

		public bool LoadExternalProvider (string providerAssembly,
			string providerConnectionClass) {
			
			try {
				SqlSharpGtk.DebugWriteLine ("Loading external provider...");
				
				Assembly ps = Assembly.Load (providerAssembly);
				Type typ = ps.GetType (providerConnectionClass);
				conn = (IDbConnection) Activator.CreateInstance (typ);
								
				SqlSharpGtk.DebugWriteLine ("External provider loaded.");
			}
			catch (Exception f) {
				string errorMessage = String.Format (
					"Error: unable to load the assembly of the provider: {1} because: {2}", 
					providerAssembly,
					f.Message);
				Error (errorMessage);
				return false;
			}
			return true;
		}

		static void Window_Delete (object o, DeleteEventArgs args) {
			Application.Quit ();
		}

		static void exit_func (Gtk.Object o) {
			Application.Quit ();
		}

		static void exit_cb (object o, EventArgs args) {
			Application.Quit ();
		}

		void connect_cb (object o, EventArgs args) {
			
			LoginDialog login = new LoginDialog (this);
			login = null;
		}

		void disconnect_cb (object o, EventArgs args) {
			AppendText(buf, "Disconnecting...");
			try {
				conn.Close ();
				conn = null;
			}
			catch (Exception e) {
				Console.WriteLine ("Error: Unable to disconnect." + 
					e.Message);
				conn = null;
				return;
			}
			AppendText (buf, "Disconnected.");
		}

		void execute_func (Gtk.Object o) {
			ExecuteSQL ();
		}

		// Execute SQL Commands
		void ExecuteSQL () {
			if (conn == null) {
				AppendText (buf, "Error: Not Connected.");
				return;
			}

			string msg = "";
			string sql = "";	

			IDbCommand cmd;
			try {
				cmd = conn.CreateCommand ();
			}
			catch (Exception ec) {
				AppendText (buf, 
					"Error: Unable to create command to execute: " + 
					ec.Message);
				return;
			}

			SqlSharpGtk.DebugWriteLine ("get text from SQL editor...");

			// get text from SQL editor
			try {
				int char_count = 0;
				TextIter start_iter, end_iter;
				TextBuffer exeBuff;
				exeBuff = editor.Buffer;
				exeBuff.GetIterAtOffset(out start_iter, 0);
				char_count = exeBuff.CharCount;
				exeBuff.GetIterAtOffset(out end_iter, char_count);
				sql = exeBuff.GetText(start_iter, end_iter, false);
			}
			catch (Exception et) {
				AppendText (buf, 
					"Error: Unable to get text from SQL editor: " + 
					et.Message);
				return;
			}
			
			try {
				cmd.CommandText = sql;
			}
			catch (Exception e) {
				AppendText (buf, 
					"Error: Unable to set SQL text to command.");
			}
			
			IDataReader reader = null;
			SqlSharpGtk.DebugWriteLine ("Executing SQL: " + sql);

			/*
			try {
				reader = cmd.ExecuteReader ();
			}
			catch (Exception e) {
				msg = "SQL Execution Error: " + e.Message;
				Error (msg);
				return;
			}
			
			if (reader == null) {
				Error("Error: reader is null");
				return;
			}
			*/
			Error("Display Data...");
			try {
				// DisplayData (reader);

				DataTable dataTable = LoadDataTable (cmd);
				grid.DataSource = dataTable;
				grid.DataBind ();
				grid.ResizeChildren ();
				grid.Show ();
				grid.ShowAll ();
				win.ShowAll ();
			} 
			catch (Exception e) {
				msg = "Error Displaying Data: " + e.Message;
				Error (msg);
			}
		}

		void execute_cb (object o, EventArgs args) {
			ExecuteSQL ();
		}

		public void DisplayResult (IDataReader reader, DataTable schemaTable) {

			const string zero = "0";
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
			string dataTypeName; // native Database type
			DataRow row; // schema row

			line = new StringBuilder ();
			hdrUnderline = new StringBuilder ();

			try {
				OutputLine ("Fields in Query Result: " + 
					reader.FieldCount);
			}
			catch(Exception e){
				Console.WriteLine ("Error: Unable to get FieldCount: " +
					e.Message);
				return;
			}
			
			OutputLine ("");
			
			for(c = 0; c < reader.FieldCount; c++) {
				try {			
					DataRow schemaRow = schemaTable.Rows[c];
					string columnHeader = reader.GetName (c);
					if (columnHeader.Equals (""))
						columnHeader = "column";
					if (columnHeader.Length > 32)
						columnHeader = columnHeader.Substring (0,32);
					
					// spacing
					columnSize = (int) schemaRow["ColumnSize"];
					theType = reader.GetFieldType(c);
					dataType = theType.ToString();
					//dataTypeName = reader.GetDataTypeName(c);

					switch(dataType) {
					case "System.DateTime":
						columnSize = 19;
						break;
					case "System.Boolean":
						columnSize = 5;
						break;
					}

					//if(provider.Equals("POSTGRESQL") ||
					//	provider.Equals("MYSQL"))
					//if(dataTypeName.Equals("text"))				
					//	columnSize = 32; // text will be truncated to 32

					hdrLen = Math.Max (columnHeader.Length, columnSize);

					if(hdrLen < 0)
						hdrLen = 0;
					if(hdrLen > 32)
						hdrLen = 32;

					line.Append(columnHeader);
					if(columnHeader.Length < hdrLen) {
						spacing = hdrLen - columnHeader.Length;
						line.Append(spacingChar, spacing);
					}
					hdrUnderline.Append(underlineChar, hdrLen);

					line.Append(" ");
					hdrUnderline.Append(" ");
				}
				catch(Exception e) {
					Console.WriteLine("Error: Unable to display header: " +
						e.Message);
					return;
				}
			}
			OutputHeader(line.ToString());
			line = null;
			
			OutputHeader(hdrUnderline.ToString());
			OutputHeader("");
			hdrUnderline = null;		
								
			int numRows = 0;

			// column data
			try {
				while(reader.Read()) {
					numRows++;
				
					line = new StringBuilder();
					for(c = 0; c < reader.FieldCount; c++) {
						int dataLen = 0;
						string dataValue = "";
						column = new StringBuilder();
						outData = "";
					
						row = schemaTable.Rows[c];
						string colhdr = (string) reader.GetName(c);
						if(colhdr.Equals(""))
							colhdr = "column";
						if(colhdr.Length > 32)
							colhdr = colhdr.Substring(0, 32);

						columnSize = (int) row["ColumnSize"];
						theType = reader.GetFieldType(c);
						dataType = theType.ToString();
					
						//dataTypeName = reader.GetDataTypeName(c);

						switch(dataType) {
						case "System.DateTime":
							columnSize = 19;
							break;
						case "System.Boolean":
							columnSize = 5;
							break;
						}

						//if(provider.Equals("POSTGRESQL") ||
						//	provider.Equals("MYSQL"))
						//if(dataTypeName.Equals("text"))				
						//	columnSize = 32; // text will be truncated to 32

						columnSize = Math.Max(colhdr.Length, columnSize);

						if(columnSize < 0)
							columnSize = 0;
						if(columnSize > 32)
							columnSize = 32;							
					
						dataValue = "";	
					
						if(reader.IsDBNull(c)) {
							dataValue = "";
							dataLen = 0;
						}
						else {											
							StringBuilder sb;
							DateTime dt;
							if(dataType.Equals("System.DateTime")) {
					
								// display date in ISO format
								// "YYYY-MM-DD HH:MM:SS"
								dt = reader.GetDateTime(c);
								sb = new StringBuilder();
								// year
								if(dt.Year < 10)
									sb.Append("000" + dt.Year);
								else if(dt.Year < 100)
									sb.Append("00" + dt.Year);
								else if(dt.Year < 1000)
									sb.Append("0" + dt.Year);
								else
									sb.Append(dt.Year);
								sb.Append("-");
								// month
								if(dt.Month < 10)
									sb.Append(zero + dt.Month);
								else
									sb.Append(dt.Month);
								sb.Append("-");
								// day
								if(dt.Day < 10)
									sb.Append(zero + dt.Day);
								else
									sb.Append(dt.Day);
								sb.Append(" ");
								// hour
								if(dt.Hour < 10)
									sb.Append(zero + dt.Hour);
								else
									sb.Append(dt.Hour);
								sb.Append(":");
								// minute
								if(dt.Minute < 10)
									sb.Append(zero + dt.Minute);
								else
									sb.Append(dt.Minute);
								sb.Append(":");
								// second
								if(dt.Second < 10)
									sb.Append(zero + dt.Second);
								else
									sb.Append(dt.Second);

								dataValue = sb.ToString();
							}
							else {
								object o = reader.GetValue(c);
								dataValue = o.ToString();
							}

							dataLen = dataValue.Length;
							if(dataLen <= 0) {
								dataValue = "";
								dataLen = 0;
							}
							if(dataLen > 32) {
								dataValue = dataValue.Substring(0,32);
								dataLen = 32;
							}
						}
						columnSize = Math.Max (columnSize, dataLen);
					
						// spacing
						spacingChar = ' ';										
						if(columnSize < colhdr.Length) {
							spacing = colhdr.Length - columnSize;
							column.Append(spacingChar, spacing);
						}
						if(dataLen < columnSize) {
							spacing = columnSize - dataLen;
							column.Append(spacingChar, spacing);
							switch(dataType) {
							case "System.Int16":
							case "System.Int32":
							case "System.Int64":
							case "System.Single":
							case "System.Double":
							case "System.Decimal":
								outData = column.ToString() + 
									dataValue;
								break;
							default:
								outData = dataValue + 
									column.ToString();
								break;
							}
						}
						else
							outData = dataValue;

						line.Append (outData);
						line.Append (" ");
					}
					OutputData (line.ToString ());
					line = null;
				}
			}
			catch (Exception rr) {
				Error ("Error: Unable to read next row: " +
					rr.Message);
				return;
			}
		
			OutputLine ("\nRows retrieved: " + numRows.ToString());
		}

		public void DisplayData(IDataReader reader) {

			bool another = false;
			DataTable schemaTable = null;
			int ResultSet = 0;

			OutputLine ("Display any result sets...");
			
			do {
				// by Default, SqlDataReader has the 
				// first Result set if any

				ResultSet++;
				OutputLine ("Display the result set " + ResultSet);			
				
				if (reader.FieldCount > 0) {
					// SQL Query (SELECT)
					// RecordsAffected -1 and DataTable has a reference
					Console.WriteLine ("Get Schema Table...");
					Console.Out.Flush ();
					try {
						schemaTable = reader.GetSchemaTable ();
					}
					catch (Exception es) {
						Error("Error: Unable to get schema table: " + 
							es.Message);
						return;
					}

					AppendText (buf, "Display Result...");
					DisplayResult (reader, schemaTable);
				}
				else if (reader.RecordsAffected >= 0) {
					// SQL Command (INSERT, UPDATE, or DELETE)
					// RecordsAffected >= 0
					int records = 0;
					try {
						records = reader.RecordsAffected;
						AppendText (buf, "SQL Command Records Affected: " + 
							records);
					}
					catch (Exception er) {
						Error ("Error: Unable to get records affected: " +
							er.Message);
						return;
					}
				}
				else {
					// SQL Command (not INSERT, UPDATE, nor DELETE)
					// RecordsAffected -1 and DataTable has a null reference
					AppendText (buf, "SQL Command Executed.");
				}
				
				// get next result set (if anymore is left)
				try {
					another = reader.NextResult ();
				}
				catch(Exception e) {
					Error ("Error: Unable to read next result: " +
						e.Message);
					return;
				}
			} while(another == true);
		}

		// used for outputting message, but if silent is set,
		// don't display
		public void OutputLine(string line) {
			//if(silent == false)
			OutputData(line);
		}

		// used for outputting the header columns of a result
		public void OutputHeader(string line) {
			//if(showHeader == true)
			OutputData(line);
		}

		// OutputData() - used for outputting data
		//  if an output filename is set, then the data will
		//  go to a file; otherwise, it will go to the Console.
		public void OutputData(string line) {
			//if(outputFilestream == null)
			//	Console.WriteLine(line);
			//else
			//	outputFilestream.WriteLine(line);
			AppendText(buf,line);
		}

		public void Error(string message) {
			Console.WriteLine(message);
			Console.Out.Flush();
			AppendText(buf, message);
		}

		bool OpenInternalProvider () {
			string msg;

			string providerKey = dbProvider.Key;
			switch (providerKey.ToUpper ()) {
			case "SQLCLIENT":
				try {
					conn = new SqlConnection ();
				}
				catch (Exception e) {
					msg = "Error: unable to create connection: " +
						e.Message;
					Error (msg);
					return false;
				}
				break;
			case "ODBC":
				try {
					conn = new OdbcConnection ();
				}
				catch (Exception e) {
					msg = "Error: unable to create connection: " +
						e.Message;
					Error (msg);
					return false;
				}
				break;
			case "OLEDB":
				try {
					conn = new OleDbConnection ();
				}
				catch (Exception e) {
					msg = "Error: unable to create connection: " +
						e.Message;
					Error (msg);
					return false;
				}
				break;
			default:
				msg = "Error: provider not supported.";
				Error (msg);
				return false;
			}
			return true;
		}

		bool OpenExternalProvider() {
			bool success = false;
			string msg;

			string providerKey = dbProvider.Key;
			switch (providerKey.ToUpper ()) {
			case "ORACLE":
				msg = "Error: Provider not currently supported.";
				Error(msg);
				break;
			default:
				success = LoadExternalProvider (
					dbProvider.Assembly,
					dbProvider.ConnectionClass);
				break;
			}
			return success;
		}

		public DbDataAdapter CreateDbDataAdapter () {
			string msg = "";
			DbDataAdapter adapter = null;
			if (dbProvider.InternalProvider == true) {
				string providerKey = dbProvider.Key;
				switch (providerKey.ToUpper ()) {
				case "SQLCLIENT":
					try {
						adapter = new SqlDataAdapter ();
					}
					catch (Exception e) {
						msg = "Error: unable to create adapter: " +
							e.Message;
						Error (msg);
						return null;
					}
					break;
				}
			}
			return adapter;
		}

		public DataTable LoadDataTable (IDbCommand cmd) {
			DbDataAdapter adapter = CreateDbDataAdapter ();
			((IDbDataAdapter) adapter).SelectCommand = cmd;
			DataTable dataTable = new DataTable ();
			adapter.Fill (dataTable);
			return dataTable;
		}

		public bool OpenDataSource () {
			string msg;
			bool gotClass = false;
			
			msg = "Attempt to open connection...";
			AppendText (buf, msg);

			conn = null;

			try {
				if (dbProvider.InternalProvider == true) {
					gotClass = OpenInternalProvider ();
				} 
				else {
					gotClass = OpenExternalProvider ();
				}
			}
			catch (Exception e) {
				msg = "Error: Unable to create Connection object. " + 
					e.Message;
				Error (msg);
				return false;
			}

			if (gotClass == false)
				return false;

			conn.ConnectionString = connectionString;
			
			try {
				conn.Open ();
				if( conn.State == ConnectionState.Open)
					AppendText (buf, "Open was successfull.");
				else {
					AppendText (buf, "Error: Open failed.");
					return false;
				}
			}
			catch (Exception e) {
				msg = "Error: Could not open data source: " + e.Message;
				Error (msg);
				conn = null;
			}
			return true;
		}

		public static void DebugWriteLine (string text) {
#if DEBUG
			Console.WriteLine (text);
			Console.Out.Flush ();
#endif // DEBUG
		}

		public static int Main (string[] args) {

			Application.Init ();
			SqlSharpGtk sqlSharp = new SqlSharpGtk ();
			Application.Run ();

			return 0;
		}
	}
}
