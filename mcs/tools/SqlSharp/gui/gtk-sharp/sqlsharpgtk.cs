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
// To be included with Mono as a SQL query tool under the X11 license.
//

namespace Mono.Data.SqlSharp.Gui.GtkSharp {
	using System;
	using System.Data;
	using System.Drawing;
	using System.Text;
	using System.IO;
	using Gtk;
	using GtkSharp;
	using SqlEditorSharp;
	using System.Reflection;
	using System.Runtime.Remoting;

	public class SqlSharpGtk {       
		// these will be moved once a SqlSharpWindow has been created
		IDbConnection conn = null;
		public TextBuffer buf;
		TextView text;
		TextTag textTag;
		string provider = "MYSQL";
		SqlEditor editor;

		public SqlSharpGtk () {
			ScrolledWindow swin;

			Window win = new Window ("SQL#");
			win.DeleteEvent += new DeleteEventHandler (Window_Delete);
			win.BorderWidth = 4;
			win.DefaultSize = new Size (450, 300);
			
			VBox vbox = new VBox (false, 4);
			win.Add (vbox);

			MenuBar mb = new MenuBar ();
			Menu file_menu = new Menu ();

			MenuItem connect_item = new MenuItem("Connect");
			connect_item.Activated += new EventHandler (connect_cb);
			file_menu.Append (connect_item);

			MenuItem disconnect_item = new MenuItem("Disconnect");
			disconnect_item.Activated += new EventHandler (disconnect_cb);
			file_menu.Append (disconnect_item);

			MenuItem execute_item = new MenuItem("Execute");
			execute_item.Activated += new EventHandler (execute_cb);
			file_menu.Append (execute_item);

			MenuItem exit_item = new MenuItem("Exit");
			exit_item.Activated += new EventHandler (exit_cb);
			file_menu.Append (exit_item);

			MenuItem file_item = new MenuItem("File");
			file_item.Submenu = file_menu;
			mb.Append (file_item);
			vbox.PackStart(mb, false, false, 0);
			
			// SQL Editor (top TextView)
			editor = new SqlEditor();
			vbox.PackStart (editor, true, true, 0);		

			// Output Results (TextView)
			swin = new ScrolledWindow (new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0), new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0));
			swin.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			swin.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			swin.ShadowType = Gtk.ShadowType.In;
			vbox.PackStart (swin, true, true, 0);

			TextTagTable textTagTable = new TextTagTable();
			textTag = new TextTag("normaltext");
			textTag.Family = "courier";
			textTag.Foreground = "black";
			textTagTable.Add(textTag);

			buf = new TextBuffer (textTagTable);
			text = new TextView (buf);
			text.Editable = false;
			swin.Add (text);		

			win.ShowAll ();
		}

		public void AppendText(TextBuffer buffer, string text)	{

			TextIter iter;
			int char_count = 0;

			if(!text.Equals("")) {
				
				char_count = buffer.CharCount;
				char_count = 0 > char_count - 1 ? 0 : char_count;
				buffer.GetIterAtOffset(out iter, char_count);
				buffer.Insert(iter, text, -1);
			}
			char_count = buffer.CharCount;
			char_count = 0 > char_count - 1 ? 0 : char_count;
			buffer.GetIterAtOffset(out iter, char_count);
			buffer.Insert(iter, "\n", -1);

			TextIter start_iter, end_iter;
			buf.GetIterAtOffset(out start_iter, 0);
			char_count = buf.CharCount;
			buf.GetIterAtOffset(out end_iter, char_count);
			buf.ApplyTagByName("normaltext", start_iter, end_iter);
		}

		public bool LoadExternalProvider(string providerAssembly,
			string providerConnectionClass) {
			
			bool success = false;

			try {
				Console.WriteLine("Loading external provider...");
				Console.Out.Flush();

				Assembly ps = Assembly.Load(providerAssembly);
				Type typ = ps.GetType(providerConnectionClass);
				conn = (IDbConnection) Activator.CreateInstance(typ);
				success = true;
				
				Console.WriteLine("External provider loaded.");
				Console.Out.Flush();
			}
			catch(FileNotFoundException f) {
				Console.WriteLine("Error: unable to load the assembly of the provider: " + 
					providerAssembly);
				return false;
			}
			return success;
		}

		public void StartUp() {

			AppendText(buf, "SQL# for GTK# on Mono.");
			AppendText(buf, "Load Connection Class...");
			LoadExternalProvider(
				"Mono.Data.MySql",
				"Mono.Data.MySql.MySqlConnection");
			AppendText(buf, "Done.");
		}

		public static int Main (string[] args)
		{
			Application.Init ();

			SqlSharpGtk sqlSharp = new SqlSharpGtk();

			sqlSharp.StartUp();

			Application.Run ();

			return 0;
		}

		static void Window_Delete (object o, DeleteEventArgs args)
		{
			Application.Quit ();
		}

		void exit_cb (object o, EventArgs args) {
			AppendText(buf, "Exiting...");
			Application.Quit ();
		}

		void connect_cb (object o, EventArgs args) {
			AppendText(buf, "Connecting...");
			conn.ConnectionString = "dbname=test";
			try {
				conn.Open();
			}
			catch(Exception e) {
				Console.WriteLine("Error: Unable to connect: " + e.Message);
				return;
			}
			AppendText(buf, "Connected.");
		}

		void disconnect_cb (object o, EventArgs args) {
			AppendText(buf, "Disconnecting...");
			try {
				conn.Close();
			}
			catch(Exception e) {
				Console.WriteLine("Error: Unable to disconnect." + e.Message);
				return;
			}
			AppendText(buf, "Disconnected.");
		}

		void execute_cb (object o, EventArgs args) {
						
			AppendText(buf, "Executing...");

			IDbCommand cmd;
			cmd = conn.CreateCommand();

			// get text from SQL editor
			int char_count = 0;
			TextIter start_iter, end_iter;
			TextBuffer exeBuff;
			exeBuff = editor.SqlBuffer;
			exeBuff.GetIterAtOffset(out start_iter, 0);
			char_count = exeBuff.CharCount;
			exeBuff.GetIterAtOffset(out end_iter, char_count);
			string sql;
			sql = exeBuff.GetText(start_iter, end_iter, false);
			
			cmd.CommandText = sql;

			IDataReader reader;
			Console.WriteLine("Executing SQL: " + sql);
			reader = cmd.ExecuteReader();
			
			AppendText(buf, "Display Data...");
			DisplayData(reader);

		}
		public void DisplayResult(IDataReader reader, DataTable schemaTable) {

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

			line = new StringBuilder();
			hdrUnderline = new StringBuilder();

			OutputLine("Fields in Query Result: " + 
				reader.FieldCount);
			OutputLine("");
			
			for(c = 0; c < schemaTable.Rows.Count; c++) {
							
				DataRow schemaRow = schemaTable.Rows[c];
				string columnHeader = (string) schemaRow["ColumnName"];
				if(columnHeader.Equals(""))
					columnHeader = "?column?";
				if(columnHeader.Length > 32)
					columnHeader = columnHeader.Substring(0,32);											
					
				// spacing
				columnSize = (int) schemaRow["ColumnSize"];
				theType = (Type) schemaRow["DataType"];
				dataType = theType.ToString();
				dataTypeName = reader.GetDataTypeName(c);

				switch(dataType) {
				case "System.DateTime":
					columnSize = 19;
					break;
				case "System.Boolean":
					columnSize = 5;
					break;
				}

				if(provider.Equals("POSTGRESQL") ||
					provider.Equals("MYSQL"))
					if(dataTypeName.Equals("text"))				
						columnSize = 32; // text will be truncated to 32

				hdrLen = (columnHeader.Length > columnSize) ? 
					columnHeader.Length : columnSize;

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
			OutputHeader(line.ToString());
			line = null;
			
			OutputHeader(hdrUnderline.ToString());
			OutputHeader("");
			hdrUnderline = null;		
								
			int rows = 0;

			// column data
			while(reader.Read()) {
				rows++;
				
				line = new StringBuilder();
				for(c = 0; c < reader.FieldCount; c++) {
					int dataLen = 0;
					string dataValue = "";
					column = new StringBuilder();
					outData = "";
					
					row = schemaTable.Rows[c];
					string colhdr = (string) row["ColumnName"];
					if(colhdr.Equals(""))
						colhdr = "?column?";
					if(colhdr.Length > 32)
						colhdr = colhdr.Substring(0, 32);

					columnSize = (int) row["ColumnSize"];
					theType = (Type) row["DataType"];
					dataType = theType.ToString();
					
					dataTypeName = reader.GetDataTypeName(c);

					switch(dataType) {
					case "System.DateTime":
						columnSize = 19;
						break;
					case "System.Boolean":
						columnSize = 5;
						break;
					}

					if(provider.Equals("POSTGRESQL") ||
						provider.Equals("MYSQL"))
						if(dataTypeName.Equals("text"))				
							columnSize = 32; // text will be truncated to 32

					columnSize = (colhdr.Length > columnSize) ? 
						colhdr.Length : columnSize;

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
							dataValue = reader.GetValue(c).ToString();
						}

						dataLen = dataValue.Length;
						if(dataLen < 0) {
							dataValue = "";
							dataLen = 0;
						}
						if(dataLen > 32) {
							dataValue = dataValue.Substring(0,32);
							dataLen = 32;
						}
					}
					columnSize = columnSize > dataLen ? columnSize : dataLen;
					
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
							outData = column.ToString() + dataValue;
							break;
						default:
							outData = dataValue + column.ToString();
							break;
						}
					}
					else
						outData = dataValue;

					line.Append(outData);
					line.Append(" ");
				}
				OutputData(line.ToString());
				line = null;
			}
			OutputLine("\nRows retrieved: " + rows.ToString());
		}

		public void DisplayData(IDataReader reader) {

			DataTable schemaTable = null;
			int ResultSet = 0;

			OutputLine("Display any result sets...");

			do {
				// by Default, SqlDataReader has the 
				// first Result set if any

				ResultSet++;
				OutputLine("Display the result set " + ResultSet);			
				
				if(reader.FieldCount > 0) {
					// SQL Query (SELECT)
					// RecordsAffected -1 and DataTable has a reference
					schemaTable = reader.GetSchemaTable();
					DisplayResult(reader, schemaTable);
				}
				else if(reader.RecordsAffected >= 0) {
					// SQL Command (INSERT, UPDATE, or DELETE)
					// RecordsAffected >= 0
					Console.WriteLine("SQL Command Records Affected: " + reader.RecordsAffected);
				}
				else {
					// SQL Command (not INSERT, UPDATE, nor DELETE)
					// RecordsAffected -1 and DataTable has a null reference
					Console.WriteLine("SQL Command Executed.");
				}
				
				// get next result set (if anymore is left)
			} while(reader.NextResult());
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

	}
}
