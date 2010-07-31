//
// SqlSharpGtk - Mono SQL# For GTK# - SQL Query and Configuration tool for 
//               Mono.Data providers
//
// Author:
//     Daniel Morgan <danielmorgan@verizon.net>
//
// (C)Copyright 2002, 2003 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.Gui.GtkSharp 
{
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
	using System.Reflection;
	using System.Runtime.Remoting;
	using System.Runtime.InteropServices;
	using System.Diagnostics;

	using Gdk;
	using Gtk;
	using GtkSharp;
	
	using Mono.GtkSharp.Goodies;

	using Gtk.Controls;

	using SqlEditorSharp;

	public enum OutputResults 
	{
		TextView,
		DataGrid
	}

	public enum ExecuteOutputType 
	{
		Normal,
		XmlFile,
		HtmlFile,
		CsvFile
	}

	public class EditorTab 
	{
		public SqlEditorSharp editor;
		public Label label;
		public string filename;
		public string basefilename;
		public int page;
	}

	public class SqlSharpGtk 
	{
		static int SqlWindowCount = 0;

		private IDbConnection conn = null;
		public DbProvider dbProvider = null;
		private Type connectionType = null;
		private Type adapterType = null;
		public Assembly providerAssembly = null;
		public string connectionString = "";	
		
		private Statusbar statusBar;
		private Toolbar toolbar;

		int lastUnknownFile = 0;

		// OutputResults
		private VBox outbox;
		// OutputResults.TextView
		private ScrolledWindow swin;
		public TextBuffer buf;
		private TextView textView;
		private TextTag textTag;
		// OutputResults.DataGrid
		private DataGrid grid;

		private Gtk.Window win;

		public static readonly string ApplicationName = "Mono SQL# For GTK#";

		private OutputResults outputResults;

		public DbProviderCollection providerList;
		Notebook sourceFileNotebook;
		Notebook resultsNotebook;
		ArrayList editorTabs = new ArrayList();

		public SqlSharpGtk () 
		{
			CreateGui ();
			SqlWindowCount ++;
			LoadProviders ();
		}

		public void Show () 
		{
			win.ShowAll ();
		}

		public void CreateGui() 
		{
			win = new Gtk.Window (ApplicationName);
			win.DeleteEvent += new GtkSharp.DeleteEventHandler(OnWindow_Delete);
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

			// SQL Editor (top TextView panel)
			sourceFileNotebook = new Notebook();
			sourceFileNotebook.Scrollable = true;
			NewEditorTab();
			paned.Add1 (sourceFileNotebook);
			sourceFileNotebook.SwitchPage += new 
				GtkSharp.SwitchPageHandler(OnEditorTabSwitched);

			// bottom panel
			resultsNotebook = CreateOutputResultsGui ();
			paned.Add2 (resultsNotebook);

			statusBar = new Statusbar ();
			vbox.PackEnd (statusBar, false, false, 0);
			
			outputResults = OutputResults.TextView;
			ToggleResultsOutput ();
		}

		EditorTab NewEditorTab () 
		{
			SqlEditorSharp editor;
			editor = new SqlEditorSharp ();
			editor.UseSyntaxHiLighting = true;
			editor.View.Show ();
			editor.View.KeyPressEvent +=
				new GtkSharp.KeyPressEventHandler(OnKeyPressEventKey);

			lastUnknownFile ++;
			string unknownFile = "Unknown" + 
				lastUnknownFile.ToString() + ".sql";
			Label label = new Label(unknownFile);
			label.Show();
			sourceFileNotebook.AppendPage(editor, label);
			sourceFileNotebook.ShowAll ();
			sourceFileNotebook.ResizeChildren ();

			sourceFileNotebook.CurrentPage = -1;
			
			EditorTab tab = new EditorTab();
			tab.editor = editor;
			tab.label = label;
			tab.filename = "";
			tab.basefilename = unknownFile;
			tab.page = sourceFileNotebook.CurrentPage;
			editorTabs.Add(tab);
			editor.Tab = tab;
			UpdateTitleBar(tab);

			return tab;
		}

		// bottom panel
		Notebook CreateOutputResultsGui () 
		{
			Label label;
			Notebook results = new Notebook();
			results.TabPos = PositionType.Bottom;
			
			grid = CreateOutputResultsDataGrid ();
			grid.Show();
			label = new Label("Grid");
			results.AppendPage(grid, label);	

			swin = CreateOutputResultsTextView ();
			swin.Show();
			label = new Label("Log");
			results.AppendPage(swin, label);
			
			sourceFileNotebook.ShowAll ();
			sourceFileNotebook.ResizeChildren ();

			return results;

		}

		DataGrid CreateOutputResultsDataGrid () 
		{
			return new DataGrid ();
		}

		ScrolledWindow CreateOutputResultsTextView () 
		{
			ScrolledWindow sw;
			sw = new ScrolledWindow (
				new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0), 
				new Adjustment (0.0, 0.0, 0.0, 0.0, 0.0, 0.0));
			sw.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.ShadowType = Gtk.ShadowType.In;		
			
			textView = new TextView ();
			buf = textView.Buffer;
			textView.Editable = false;
			textView.ModifyFont (Pango.FontDescription.FromString ("courier new"));
			sw.Add (textView);		

			return sw;
		}

		void OnKeyPressEventKey(object o, GtkSharp.KeyPressEventArgs args) 
		{
			if (o is TextView) {
				TextView tv = (TextView) o;
				//Gdk.EventKey k = args.Event;

				// if the F5 key was pressed
				if (args.Event.keyval == 0xFFC2) {
					if (tv.Editable == true) {
						// execute SQL
						ExecuteSQL (ExecuteOutputType.Normal, "");
					}
				}
			}
		}

		Toolbar CreateToolbar () 
		{
			Toolbar toolbar = new Toolbar ();

			toolbar.ToolbarStyle = ToolbarStyle.Icons;

			toolbar.AppendItem ("Execute", 
				"Execute SQL Commands.", String.Empty,
				new Gtk.Image (Stock.Execute, IconSize.SmallToolbar),
				new Gtk.SignalFunc (OnToolbar_Execute));	
			
			toolbar.AppendItem ("DataGrid", 
				"Toggle Results to DataGrid or TextView", String.Empty,
				new Gtk.Image (Stock.GoDown, IconSize.SmallToolbar),
				new Gtk.SignalFunc (OnToolbar_ToggleResultsOutput));	

			return toolbar;
		}

		// TODO: use the ProviderFactory in Mono.Data 
		//       to load providers
		//       instead of what's below
		public void LoadProviders () 
		{
			providerList = new DbProviderCollection ();
			
			providerList.Add (new DbProvider (
				"MYSQL",
				"MySQL (Mono)",
				"Mono.Data.MySql",
				"Mono.Data.MySql.MySqlConnection",
				"Mono.Data.MySql.MySqlDataAdapter",
				false ));
			providerList.Add (new DbProvider (
				"MYSQLNET",
				"MySQL (ByteFX)",
				"ByteFX.Data",
				"ByteFX.Data.MySQLClient.MySQLConnection",
				"ByteFX.Data.MySQLClient.MySQLDataAdapter",
				false ));
			providerList.Add (new DbProvider (
				"POSTGRESQL",
				"PostgreSQL (Mono)",
				"Mono.Data.PostgreSqlClient",
				"Mono.Data.PostgreSqlClient.PgSqlConnection",
				"Mono.Data.PostgreSqlClient.PgSqlDataAdapter",
				false ));
			providerList.Add (new DbProvider (
				"NPGSQL",
				"PostgreSQL (Npgsql)",
				"Npgsql",
				"Npgsql.NpgsqlConnection",
				"Npgsql.NpgsqlDataAdapter",
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
				"Mono.Data.TdsClient.TdsDataAdapter",
				false ));
			providerList.Add (new DbProvider (
				"ODBC",
				"ODBC",
				"",
				"",
				"",
				true ));
			providerList.Add (new DbProvider (
				"OLEDB",
				"OLE DB",
				"",
				"",
				"",
				true ));
			providerList.Add (new DbProvider (
				"SQLITE",
				"SQL Lite",
				"Mono.Data.SqliteClient",
				"Mono.Data.SqliteClient.SqliteConnection",
				"Mono.Data.SqliteClient.SqliteDataAdapter",
				false ));
			providerList.Add (new DbProvider (
				"SYBASE",
				"Sybase",
				"Mono.Data.SybaseClient",
				"Mono.Data.SybaseClient.SybaseConnection",
				"Mono.Data.SybaseClient.SybaseDataAdapter",
				false ));
			providerList.Add (new DbProvider (
				"DB2",
				"IBM DB2",
				"Mono.Data.DB2Client",
				"Mono.Data.DB2Client.DB2ClientConnection",
				"Mono.Data.DB2Client.DB2ClientDataAdapter",
				false ));
			providerList.Add (new DbProvider (
				"ORACLE",
				"Oracle",
				"System.Data.OracleClient",
				"System.Data.OracleClient.OracleConnection",
				"System.Data.OracleClient.OracleDataAdapter",
				false ));
		}
		
		public MenuBar CreateMenuBar () 
		{
			MenuBar menuBar = new MenuBar ();
			Menu menu;
			Menu submenu;
			MenuItem item;
			MenuItem barItem;
			MenuItem subitem;

			// File menu
			menu = new Menu ();

			item = new MenuItem ("New SQL# _Window");
			item.Activated += new EventHandler (OnMenu_FileNewSqlWindow);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("_New");
			item.Activated += new EventHandler (OnMenu_FileNew);
			menu.Append (item);

			item = new MenuItem ("_Open...");
			item.Activated += new EventHandler (OnMenu_FileOpen);
			menu.Append (item);

			item = new MenuItem ("_Save");
			item.Activated += new EventHandler (OnMenu_FileSave);
			menu.Append (item);

			item = new MenuItem ("Save _As...");
			item.Activated += new EventHandler (OnMenu_FileSaveAs);
			menu.Append (item);

			item = new MenuItem ("Close");
			item.Activated += new EventHandler (OnMenu_FileClose);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			// TODO: submenu Save Output
			submenu = new Menu ();
			subitem = new MenuItem ("CSV - Comma Separated Values");
			//subitem.Activated += new EventHandler (OnMenu_FileSaveOutput_CSV);
			submenu.Append(subitem);
			subitem = new MenuItem ("TAB - Tab Separated Values");
			//subitem.Activated += new EventHandler (OnMenu_FileSaveOutput_TAB);
			submenu.Append(subitem);
			subitem = new MenuItem ("XML");
			//subitem.Activated += new EventHandler (OnMenu_FileSaveOutput_XML);
			submenu.Append(subitem);

			item = new MenuItem ("Save _Output...");
			item.Submenu = submenu;
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("E_xit");
			item.Activated += new EventHandler (OnMenu_FileExit);
			menu.Append (item);

			barItem = new MenuItem ("_File");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			// Edit menu

			menu = new Menu ();

			item = new MenuItem ("_Undo");
			//item.Activated += new EventHandler (OnMenu_EditUndo);
			menu.Append (item);

			item = new MenuItem ("_Redo");
			//item.Activated += new EventHandler (OnMenu_EditRedo);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("Cu_t");
			//item.Activated += new EventHandler (OnMenu_EditCut);
			menu.Append (item);

			item = new MenuItem ("_Copy");
			//item.Activated += new EventHandler (OnMenu_EditCopy);
			menu.Append (item);

			item = new MenuItem ("_Paste");
			//item.Activated += new EventHandler (OnMenu_EditPaste);
			menu.Append (item);

			item = new MenuItem ("_Delete");
			//item.Activated += new EventHandler (OnMenu_EditDelete);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("_Find and Replace...");
			//item.Activated += new EventHandler (OnMenu_EditFindReplace);
			menu.Append (item);

			menu.Append (new SeparatorMenuItem ());

			item = new MenuItem ("_Options");
			//item.Activated += new EventHandler (OnMenu_EditOptions);
			menu.Append (item);

			barItem = new MenuItem ("_Edit");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			// Session menu
			menu = new Menu ();

			item = new MenuItem ("_Connect");
			item.Activated += new EventHandler (OnMenu_SessionConnect);
			menu.Append (item);

			item = new MenuItem ("_Disconnect");
			item.Activated += new EventHandler (OnMenu_SessionDisconnect);
			menu.Append (item);

			barItem = new MenuItem ("_Session");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			// Command menu
			menu = new Menu ();

			item = new MenuItem ("_Execute");
			item.Activated += new EventHandler (OnMenu_CommandExecute);
			menu.Append (item);

			item = new MenuItem ("_Execute With Output to XML");
			item.Activated += new EventHandler (OnMenu_CommandExecuteXML);
			menu.Append (item);

			item = new MenuItem ("_Execute With Output to CSV");
			item.Activated += new EventHandler (OnMenu_CommandExecuteCSV);
			menu.Append (item);

			item = new MenuItem ("_Execute With Output to HTML");
			item.Activated += new EventHandler (OnMenu_CommandExecuteHTML);
			menu.Append (item);

			barItem = new MenuItem ("_Command");
			barItem.Submenu = menu;
			menuBar.Append (barItem);

			return menuBar;
		}

		void AppendText (string text) 
		{
			AppendText (buf, text);
		}

		public void AppendTextWithoutScroll (TextBuffer buffer, string text) 
		{
			TextIter iter;
			text = text.Replace("\0","");
			buffer.MoveMark(buf.InsertMark, buffer.EndIter);
			if (text.Equals ("") == false) {				
				iter = buffer.EndIter;
				buffer.Insert (iter, text);
			}
			iter = buffer.EndIter;
			buffer.Insert (iter, "\n");
		}

		// WriteLine() to output text to bottom TextView
		// for displaying result sets and logging messages
		public void AppendText (TextBuffer buffer, string text) 
		{
			AppendTextWithoutScroll(buffer,text);
			while (Application.EventsPending ()) 
				Application.RunIteration ();
			textView.ScrollToMark (buf.InsertMark, 0.4, true, 0.0, 1.0);
		}

		public bool LoadExternalProvider (string strProviderAssembly,
						string providerConnectionClass) 
		{		
			try {
				SqlSharpGtk.DebugWriteLine ("Loading external provider...");
				providerAssembly = null;
				providerAssembly = Assembly.Load (strProviderAssembly);
				Type typ = providerAssembly.GetType (providerConnectionClass);
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

		void QuitApplication() 
		{
			if(conn != null)
				if(conn.State == ConnectionState.Open) {
					Console.WriteLine("Closing connection...");
					conn.Close();
					conn = null;
					Console.WriteLine("Connection closed.");
				}

			if(grid.DataSource != null) {
				grid.Clear ();
				grid.DataSource = null;
				grid.DataMember = "";
				grid = null;
			}

			SqlWindowCount --;
			if(SqlWindowCount == 0)
				Application.Quit ();
			else
				win.Destroy ();
		}

		void UpdateTitleBar(EditorTab tab) 
		{
			string title = "";
			if(tab != null) {
				if(tab.filename.Equals(""))
					title = tab.label.Text + " - " + ApplicationName;
				else
					title = tab.filename + " - " + ApplicationName;
			}
			else {
				title = ApplicationName;
			}
			win.Title = title;
		}

		void OnEditorTabSwitched (object o, GtkSharp.SwitchPageArgs args) 
		{
			int page = (int) args.PageNum;
			EditorTab tab = FindEditorTab(page);
			UpdateTitleBar (tab);
		}

		void OnWindow_Delete (object o, GtkSharp.DeleteEventArgs args) 
		{
			QuitApplication();
		}

		void OnExit (Gtk.Object o) 
		{
			QuitApplication();
		}

		void OnMenu_FileNewSqlWindow (object o, EventArgs args) 
		{
			SqlSharpGtk sqlSharp = new SqlSharpGtk ();
			sqlSharp.Show ();
		}

		void OnMenu_FileNew (object o, EventArgs args) 
		{
			NewEditorTab();
			sourceFileNotebook.CurrentPage = -1;
		}

		void OnMenu_FileOpen (object o, EventArgs args) 
		{
			FileSelectionDialog openFileDialog = 
				new FileSelectionDialog ("Open File",
				new FileSelectionEventHandler (OnOpenFile));
		}

		void OnOpenFile (object o, FileSelectionEventArgs args) 
		{
			EditorTab etab = NewEditorTab();
			try {
				etab.editor.LoadFromFile (args.Filename);
			}
			catch(Exception openFileException) {
				Error("Error: Could not open file: \n" + 
					args.Filename + 
					"\n\nReason: " + 
					openFileException.Message);
				return;
			}
			TextBuffer buf = etab.editor.Buffer;
			buf.Modified = false;
			string basefile = Path.GetFileName (args.Filename);
			etab.label.Text = basefile;
			etab.basefilename = basefile;
			etab.filename = args.Filename;
			sourceFileNotebook.CurrentPage = -1;
			UpdateTitleBar(etab);
		}

		EditorTab FindEditorTab (int searchPage) 
		{
			EditorTab tab = null;
			for (int t = 0; t < editorTabs.Count; t++) {
				tab = (EditorTab) editorTabs[t];
				if (tab.page == searchPage)
					return tab;
			}
			return tab;
		}

		void OnMenu_FileSave (object o, EventArgs args) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab tab = FindEditorTab(page);

			if(tab.filename.Equals(""))
				SaveAs();
			else {
				SaveFile(tab.filename);
				tab.label.Text = tab.basefilename;
			}
		}

		void SaveFile (string filename) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab etab = FindEditorTab(page);

			try {
				// FIXME: if file exists, ask if you want to 
				//        overwrite.   currently, it overwrites
				//        without asking.
				etab.editor.SaveToFile (filename);
			} catch(Exception saveFileException) {
				Error("Error: Could not open file: \n" + 
					filename + 
					"\n\nReason: " + 
					saveFileException.Message);
				return;
			}
			TextBuffer buf = etab.editor.Buffer;
			buf.Modified = false;
		}

		void OnMenu_FileSaveAs (object o, EventArgs args) 
		{
			SaveAs();
		}

		void SaveAs() 
		{
			FileSelectionDialog openFileDialog = 
				new FileSelectionDialog ("File Save As",
				new FileSelectionEventHandler (OnSaveAsFile));
		}

		void OnSaveAsFile (object o, FileSelectionEventArgs args) 
		{
			int page = sourceFileNotebook.CurrentPage;
			EditorTab etab = FindEditorTab(page);

			SaveFile(args.Filename);

			string basefile = Path.GetFileName (args.Filename);
			etab.label.Text = basefile;
			etab.basefilename = basefile;
			etab.filename = args.Filename;
			UpdateTitleBar(etab);
		}

		void OnMenu_FileClose (object o, EventArgs args) 
		{
			CloseEditor();
		}

		void OnCloseEditor (object obj, EventArgs args) 
		{
			CloseEditor();
		}

		void CloseEditor () 
		{
			int page = sourceFileNotebook.CurrentPage;
			SqlEditorSharp sqlEditor;
			sqlEditor = (SqlEditorSharp) sourceFileNotebook.GetNthPage(page);
			TextBuffer buffer = sqlEditor.Buffer;
			if(buffer.Modified) {
				// TODO: if text modified, 
				// ask if user wants to save
				// before closing.
				// use MessageDialog to prompt
				RemoveEditorTab (sqlEditor.Tab, page);
			}
			else {
				RemoveEditorTab (sqlEditor.Tab, page);
			}
			sqlEditor = null;
			buffer = null;
		}

		void RemoveEditorTab (EditorTab tab, int page) 
		{
			tab.editor.Clear();
			tab.editor.Tab = null;
			tab.editor = null;
			tab.label = null;
			editorTabs.Remove(tab);
			sourceFileNotebook.RemovePage (page);
			sourceFileNotebook.QueueDraw();
			tab = null;
		}

		void OnMenu_FileExit (object o, EventArgs args) 
		{
			QuitApplication ();
		}

		void OnMenu_SessionConnect (object o, EventArgs args) 
		{	
			LoginDialog login = new LoginDialog (this);
			login = null;
		}

		void OnMenu_SessionDisconnect (object o, EventArgs args) 
		{
			AppendText(buf, "Disconnecting...");
			try {
				conn.Close ();
				conn = null;
			}
			catch (Exception e) {
				Error ("Error: Unable to disconnect." + 
					e.Message);
				conn = null;
				return;
			}
			AppendText (buf, "Disconnected.");
		}

		void OnToolbar_ToggleResultsOutput () 
		{
			ToggleResultsOutput ();
		}

		void ToggleResultsOutput () 
		{
			if (outputResults == OutputResults.TextView) {
				outputResults = OutputResults.DataGrid;
			}
			else if (outputResults == OutputResults.DataGrid) {
				outputResults = OutputResults.TextView;
			}
		}

		public void OnToolbar_Execute () 
		{
			ExecuteSQL (ExecuteOutputType.Normal, "");
		}

		// Execute SQL Commands
		void ExecuteSQL (ExecuteOutputType outputType, string filename) 
		{		
			if (conn == null) {
				AppendText (buf, "Error: Not Connected.");
				return;
			}

			DataTable schemaTable = null;

			int page = sourceFileNotebook.CurrentPage;
			EditorTab tab = FindEditorTab(page);

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
				TextIter start_iter, end_iter;
				TextBuffer exeBuff;
				exeBuff = tab.editor.Buffer;
				start_iter = exeBuff.StartIter;
				end_iter = exeBuff.EndIter;
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
			
			if ((outputResults == OutputResults.TextView && 
				outputType == ExecuteOutputType.Normal) ||
				outputType == ExecuteOutputType.HtmlFile ||
				outputType == ExecuteOutputType.CsvFile) {

				try {
					reader = cmd.ExecuteReader ();
				}
				catch (Exception e) {
					//msg = "SQL Execution Error: " + e.Message;
					msg = "SQL Execution Error: " + e;
					Error (msg);
					return;
				}
			
				if (reader == null) {
					Error("Error: reader is null");
					return;
				}
			}

			try {
				if (outputResults == OutputResults.TextView && 
					outputType == ExecuteOutputType.Normal) {

					DisplayData (reader);
					// clean up
					reader.Close ();
					reader.Dispose ();
					reader = null;
				}
				else if(outputType == ExecuteOutputType.HtmlFile) {
					schemaTable = reader.GetSchemaTable();
					if(schemaTable != null && reader.FieldCount > 0) {
						OutputDataToHtmlFile(reader, schemaTable, filename);
					}
					else {
						AppendText("Command executed.");
					}
					// clean up
					reader.Close ();
					reader.Dispose ();
					reader = null;
				}
				else if(outputType == ExecuteOutputType.CsvFile) {
					schemaTable = reader.GetSchemaTable();
					if(schemaTable != null && reader.FieldCount > 0) {
						OutputDataToCsvFile(reader, schemaTable, filename);
					}
					else {
						AppendText("Command executed.");
					}
					// clean up
					reader.Close ();
					reader.Dispose ();
					reader = null;
				}
				else {
					DataTable dataTable = LoadDataTable (cmd);
					switch(outputType) {
					case ExecuteOutputType.Normal:
						AppendText("set DataGrid.DataSource to DataTable...");
						grid.DataSource = dataTable;
						AppendText("DataBind...");
						grid.DataBind ();
						AppendText("Clean up...");
						// clean up
						grid.DataSource = null;
						break;
					case ExecuteOutputType.XmlFile:
						AppendText("Create DataSet...");
						DataSet dataSet = new DataSet();
						AppendText("Add DataTable to DataSet's DataTableCollection...");
						dataSet.Tables.Add(dataTable);
						AppendText("Write DataSet to XML file: " + 
							filename);
						dataSet.WriteXml(filename);
						AppendText("Clean up...");
						dataSet = null;
						break;
					}
					// clean up
					dataTable.Clear();
					dataTable.Dispose();
					dataTable = null;
					AppendText("Done.");
					cmd.Dispose();
					cmd = null;
				}
			}
			catch (Exception e) {
				//msg = "Error Displaying Data: " + e.Message;
				msg = "Error Displaying Data: " + e;
				Error (msg);
			}
		}

		public void OutputDataToHtmlFile(IDataReader rdr, DataTable dt, string file) 
		{     		
			AppendText("Outputting results to HTML file " + file + "...");
			StreamWriter outputFilestream = null;
			try {
				outputFilestream = new StreamWriter(file);
			}
			catch(Exception e) {
				Error("Error: Unable to setup output results file. " + 
					e.Message);
				return;
			}

			StringBuilder strHtml = new StringBuilder();

			strHtml.Append("<html>\n<head><title>");
			strHtml.Append("Results");
			strHtml.Append("</title></head>\n");
			strHtml.Append("<body>\n");
			strHtml.Append("<h1>Results</h1>\n");
			strHtml.Append("\t<table border=1>\n");
		
			outputFilestream.WriteLine(strHtml.ToString());

			strHtml = null;
			strHtml = new StringBuilder();

			strHtml.Append("\t\t<tr>\n");
			for (int c = 0; c < rdr.FieldCount; c++) {
				strHtml.Append("\t\t\t<td><b>");
				string sColumnName = rdr.GetName(c);
				strHtml.Append(sColumnName);
				strHtml.Append("</b></td>\n");
			}
			strHtml.Append("\t\t</tr>\n");
			outputFilestream.WriteLine(strHtml.ToString());
			strHtml = null;

			int col = 0;
			string dataValue = "";
			
			while(rdr.Read()) {
				strHtml = new StringBuilder();

				strHtml.Append("\t\t<tr>\n");
				for(col = 0; col < rdr.FieldCount; col++) {
						
					// column data
					if(rdr.IsDBNull(col) == true)
						dataValue = "NULL";
					else {
						object obj = rdr.GetValue(col);
						dataValue = obj.ToString();
					}
					strHtml.Append("\t\t\t<td>");
					strHtml.Append(dataValue);
					strHtml.Append("</td>\n");
				}
				strHtml.Append("\t\t</tr>\n");
				outputFilestream.WriteLine(strHtml.ToString());
				strHtml = null;
			}
			outputFilestream.WriteLine("\t</table>\n</body>\n</html>\n");
			strHtml = null;
			outputFilestream.Close();
			outputFilestream = null;
			AppendText("Outputting file done.");
		}

		public void OutputDataToCsvFile(IDataReader rdr, DataTable dt, string file) 
		{     		
			AppendText("Outputting results to CSV file " + file + "...");
			StreamWriter outputFilestream = null;
			try {
				outputFilestream = new StreamWriter(file);
			}
			catch(Exception e) {
				Error("Error: Unable to setup output results file. " + 
					e.Message);
				return;
			}

			StringBuilder strCsv = null;

			int col = 0;
			string dataValue = "";
			
			while(rdr.Read()) {
				strCsv = new StringBuilder();
				
				for(col = 0; col < rdr.FieldCount; col++) {
					if(col > 0)
						strCsv.Append(",");

					// column data
					if(rdr.IsDBNull(col) == true)
						dataValue = "\"\"";
					else {
						object obj = rdr.GetValue(col);
						dataValue = "\"" + obj.ToString() + "\"";
					}
					strCsv.Append(dataValue);
				}
				outputFilestream.WriteLine(strCsv.ToString());
				strCsv = null;
			}
			strCsv = null;
			outputFilestream.Close();
			outputFilestream = null;
			AppendText("Outputting file done.");
		}

		void OnMenu_CommandExecute (object o, EventArgs args) 
		{
			ExecuteSQL (ExecuteOutputType.Normal, "");
		}

		void OnMenu_CommandExecuteXML (object o, EventArgs args) 
		{
			ExecuteAndSaveResultsToFile (ExecuteOutputType.XmlFile);
		}

		void OnMenu_CommandExecuteCSV (object o, EventArgs args) 
		{
			ExecuteAndSaveResultsToFile (ExecuteOutputType.CsvFile);
		}

		void OnMenu_CommandExecuteHTML (object o, EventArgs args) 
		{
			ExecuteAndSaveResultsToFile (ExecuteOutputType.HtmlFile);
		}

		ExecuteOutputType outType;
		void ExecuteAndSaveResultsToFile(ExecuteOutputType oType) 
		{
			outType = oType;
			FileSelectionDialog openFileDialog = 
				new FileSelectionDialog ("Results File Save As",
				new FileSelectionEventHandler (OnSaveExeOutFile));
		}

		void OnSaveExeOutFile (object o, FileSelectionEventArgs args) 
		{
			ExecuteSQL (outType, args.Filename);
		}

		public void DisplayResult (IDataReader reader, DataTable schemaTable) 
		{
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
				Error ("Error: Unable to get FieldCount: " +
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
					Error ("Error: Unable to display header: " +
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
					
						if(dataLen < columnSize) {
							switch(dataType) {
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
								outData = dataValue.PadLeft(columnSize);
								break;
							default:
								outData = dataValue.PadRight(columnSize);
								break;
							}
							outData = outData + " ";
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
			AppendText("");
		}

		public void DisplayData(IDataReader reader) 
		{
			bool another = false;
			DataTable schemaTable = null;
			int ResultSet = 0;

			OutputLine ("Display any result sets...");
			
			do {
				// by Default, data reader has the 
				// first Result set if any

				ResultSet++;
				OutputLine ("Display the result set " + ResultSet);			
				
				if (reader.FieldCount > 0) {
					// SQL Query (SELECT)
					// RecordsAffected -1 and DataTable has a reference
					try {
						schemaTable = reader.GetSchemaTable ();
					}
					catch (Exception es) {
						Error ("Error: Unable to get schema table: " + 
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
		public void OutputLine(string line) 
		{
			//if(silent == false)
			OutputData(line);
		}

		// used for outputting the header columns of a result
		public void OutputHeader(string line) 
		{
			//if(showHeader == true)
			OutputData(line);
		}

		// OutputData() - used for outputting data
		//  if an output filename is set, then the data will
		//  go to a file; otherwise, it will go to the Console.
		public void OutputData(string line) 
		{
			//if(outputFilestream == null)
			//	Console.WriteLine(line);
			//else
			//	outputFilestream.WriteLine(line);
			AppendTextWithoutScroll(buf,line);
		}

		public void Error(string message) 
		{
			Console.WriteLine(message);
			Console.Out.Flush();
			AppendText(buf, message);
		}

		bool OpenInternalProvider () 
		{
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

		bool OpenExternalProvider() 
		{
			bool success = false;
			
			success = LoadExternalProvider (
					dbProvider.Assembly,
					dbProvider.ConnectionClass);
						
			return success;
		}

		public DbDataAdapter CreateDbDataAdapter (IDbCommand cmd) 
		{
			string msg = "";
			DbDataAdapter dbAdapter = null;
			if (dbProvider.InternalProvider == true) {
				dbAdapter = CreateInternalDataAdapter (cmd);
			}
			else {
				dbAdapter = CreateExternalDataAdapter (dbProvider.AdapterClass, cmd);
			}
			return dbAdapter;
		}

		public DbDataAdapter CreateInternalDataAdapter (IDbCommand cmd) 
		{		
			string msg = "";
			DbDataAdapter dbAdapter = null;
			string providerKey = dbProvider.Key;
			switch (providerKey.ToUpper ()) {
			case "SQLCLIENT":
				try {
					dbAdapter = new SqlDataAdapter (cmd as SqlCommand);
				}
				catch (Exception e) {
					msg = "Error: unable to create adapter: " +
						e.Message;
					Error (msg);
					return null;
				}
				break;
				
			case "OLEDB":
				try {
					dbAdapter = new OleDbDataAdapter (cmd as OleDbCommand);
				}
				catch (Exception e) {
					msg = "Error: unable to create adapter: " +
						e.Message;
					Error (msg);
					return null;
				}
				break;
			case "ODBC":
					try {
						dbAdapter = new OdbcDataAdapter (cmd as OdbcCommand);
					}
					catch (Exception e) {
						msg = "Error: unable to create adapter: " +
							e.Message;
						Error (msg);
						return null;
					}
				break;
			}
			return dbAdapter;
		}

		public DbDataAdapter CreateExternalDataAdapter (string adapterClass, IDbCommand cmd) 
		{
			adapterType = providerAssembly.GetType (adapterClass);
			System.Object ad = Activator.CreateInstance (adapterType);

			// set property SelectCommand on DbDataAdapter
			PropertyInfo prop = adapterType.GetProperty("SelectCommand");
			prop.SetValue (ad, cmd, null);

			return (DbDataAdapter) ad;
		}

		public DataTable LoadDataTable (IDbCommand dbcmd) 
		{
			string status = String.Empty;

			AppendText("Create DbDataAdapter...");
			SqlSharpDataAdapter adapter = new SqlSharpDataAdapter (dbcmd);
			
			AppendText("Create DataTable...");
			DataTable dataTable = new DataTable ();

			AppendText("Fill data into DataTable via DbDataAdapter...");

			int rowsAddedOrRefreshed = 0;
			IDataReader reader = null;
			
			try {
				reader = dbcmd.ExecuteReader ();
				if (reader.FieldCount > 0)
					rowsAddedOrRefreshed = adapter.FillTable (dataTable, reader);
			}
			catch(Exception sqle) {
				status = "Error: " + sqle.Message;
			}

			if (status.Equals(String.Empty)) {
				AppendText("Rows successfully Added or Refreshed in the DataTable: " + 
					rowsAddedOrRefreshed);
				int rowsAffected = reader.RecordsAffected;
				AppendText("Rows Affected: " + rowsAffected);

				int fields = ((IDataRecord) reader).FieldCount;
				AppendText("Field Count: " + fields);
			
				if (fields > 0) {
					status = "Rows Selected: " + rowsAddedOrRefreshed +
						"  Fields: " + fields;
				}
				else {
					status = "Rows Modified: " + rowsAffected;
				}
			}
			AppendText("Status: " + status);

			adapter.Dispose();
			adapter = null;

			AppendText("Return DataTable...");
			return dataTable;
		}

		public bool OpenDataSource () 
		{
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

		public static void DebugWriteLine (string text) 
		{
#if DEBUG
			Console.WriteLine (text);
			Console.Out.Flush ();
#endif // DEBUG
		}

		public static int Main (string[] args) 
		{		
			Application.Init ();
			SqlSharpGtk sqlSharp = new SqlSharpGtk ();
			sqlSharp.Show ();			
			Application.Run ();
			return 0;
		}
	}
}
