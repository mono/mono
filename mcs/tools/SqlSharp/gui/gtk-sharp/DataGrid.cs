//
// DataGrid - attempt at creating a DataGrid for GTK#
//            using a GTK# TreeView.  The goal is to have similar
//            functionality to a System.Windows.Forms.DataGrid
//            or System.Web.UI.WebControls.DataGrid.  This includes
//            data binding support.
//    
// Based on the sample/TreeViewDemo.cs
//
// Author: Kristian Rietveld <kris@gtk.org>
//         Daniel Morgan <danmorg@sc.rr.com>
//
// (c) 2002 Kristian Rietveld
// (c) 2002 Daniel Morgan
//

#define DataGridMain

namespace Gtk.Controls {

	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Drawing;
	using System.Reflection;
	using System.Text;
	
	using GLib;
	using Gtk;
	using GtkSharp;

	// FIXME: these don't belong here
	using System.Runtime.InteropServices;
	using System.Data;
	using System.Data.Common;
	using System.Data.SqlClient;
	using Mono.Data.PostgreSqlClient;

	public class DataGridColumn {
		private string columnName;

		public string ColumnName {
			get {
				return columnName;
			}
			set {
				columnName = value;
			}
		}
	}

	public class DataGrid {

		// FIXME: this don't belong here
		public static DataTable dataTable = null;

		private static ListStore store = null;
		private static Dialog dialog = null;
		private static Label dialog_label = null;

		public static DataGridColumn[] gridColumns;

		// FIXME: need to place in a base class
		//        the DataSource, DataMember, DataBind()
		//        public members.
		//        maybe we can call the base class
		//        BaseDataList for GTK#?

		private static object dataSource = null;

		private static string dataMember = "";

		public static object DataSource {
			get {
				return dataSource;
			}
			set {
				dataSource = value;
			}
		}

		private static string DataMember {
			get {
				return dataMember;
			}
			set {
				dataMember = value;
			}
		}

		private static void DataBind () 
		{
			AppendText ("Data binding...");
			
			UpdateDialog ("Data binding {0}", "...");	

			if (store != null)
				return;

			System.Object o = null;
			o = GetResolvedDataSource (DataSource, DataMember);
			IEnumerable ie = (IEnumerable) o;
			ITypedList tlist = (ITypedList) o;

			// FIXME: does not belong in this base method
			TreeIter iter = new TreeIter();
			// create list store for treeview
			store = new ListStore ((int)TypeFundamentals.TypeString);	
			
			PropertyDescriptorCollection pdc = tlist.GetItemProperties(new PropertyDescriptor[0]);
			gridColumns = new DataGridColumn[pdc.Count];

			// define the columns in the treeview store
			// based on the schema of the result
			Console.WriteLine("pdc.Count: " + pdc.Count.ToString());
			int[] theTypes = new int[pdc.Count];
			for(int col = 0; col < pdc.Count; col++) {
				theTypes[col] = (int)TypeFundamentals.TypeString;
			}
			// FIXME: does not belong in base method
			SetColumnTypes (theTypes);

			int colndx = -1;
			foreach(PropertyDescriptor pd in pdc) {
				colndx ++;
				gridColumns[colndx] = new DataGridColumn();
				gridColumns[colndx].ColumnName = pd.Name;
				
				AppendText("DisplayName: " + pd.DisplayName);
				AppendText("Name: " + pd.Name);
				AppendText("Type: " + pd.GetType().ToString());
				AppendText("PropertyType: " + pd.PropertyType.ToString());
				AppendText("ComponentType: " + pd.ComponentType.ToString());				

				// DataView
				AppendText("o is " + o.GetType().ToString());

				// System.ComponentModel.PropertyDescriptorCollection
				AppendText("pdc is " + pdc.GetType().ToString());

				// System.Data.DataColumnPropertyDescriptor
				AppendText("pd is " + pd.GetType().ToString());										
			}

			foreach(System.Object obj in ie) {
				Console.WriteLine("enumerated Object");
				ICustomTypeDescriptor custom = (ICustomTypeDescriptor) obj;
				PropertyDescriptorCollection properties;
				properties = custom.GetProperties ();
				
				iter = NewRow ();
				int cv = 0;
				foreach(PropertyDescriptor property in properties) {
					string propValue = property.GetValue (obj).ToString();
					Console.WriteLine("   PropertyDescriptor Name: " + property.Name);
					Console.WriteLine("                     Value: " + propValue.ToString());
					SetColumnValue (iter, cv, propValue);
					cv++;
				}
			}
			AppendText("Data Binding Done.");
		}

		// borrowed from Mono's System.Web implementation
		public static IEnumerable GetResolvedDataSource(object source, string member) {
			if(source != null && source is IListSource) {
				IListSource src = (IListSource)source;
				IList list = src.GetList();
				if(!src.ContainsListCollection) {
					return list;
				}
				if(list != null && list is ITypedList) {

					ITypedList tlist = (ITypedList)list;
					PropertyDescriptorCollection pdc = tlist.GetItemProperties(new PropertyDescriptor[0]);
					if(pdc != null && pdc.Count > 0) {
						PropertyDescriptor pd = null;
						if(member != null && member.Length > 0) {
							pd = pdc.Find(member, true);
						} else {
							pd = pdc[0];
						}
						if(pd != null) {
							object rv = pd.GetValue(list[0]);
							if(rv != null && rv is IEnumerable) {
								return (IEnumerable)rv;
							}
						}
						throw new Exception("ListSource_Missing_DataMember");
					}
					throw new Exception("ListSource_Without_DataMembers");
				}
			}
			if(source is IEnumerable) {
				return (IEnumerable)source;
			}
			return null;
		}

		// FIXME: temporarily here until ListStore.SetColumTypes() is fixed
		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_list_store_set_column_types(IntPtr raw, int n_columns, int[] types);
		public static void SetColumnTypes(int[] types) {
			gtk_list_store_set_column_types(((IWrapper)store).Handle, types.Length, types);
		}

		// for DEBUG only
		public static void AppendText (string text) {
			Console.WriteLine (text);
			Console.Out.Flush ();
		}

		public static TreeIter NewRow () { 
			TreeIter rowTreeIter = new TreeIter();
			store.Append (out rowTreeIter);
			return rowTreeIter;
		}

		public static void AddRow (object[] columnValues) {	
			TreeIter iter = NewRow ();
			
			for(int col = 0; col < columnValues.Length; col++) {

				string cellValue = columnValues[col].ToString ();
				SetColumnValue (iter, col, cellValue);
			}
		}

		private static void PopulateStore () {
			if (store != null)
				return;

			TreeIter iter = new TreeIter();

			// create list store for treeview
			store = new ListStore ((int)TypeFundamentals.TypeString);
			
			// define the columns in the treeview store
			// based on the schema of the result
			int[] theTypes = new int[dataTable.Columns.Count];
			for(int col = 0; col < dataTable.Columns.Count; col++) {
				theTypes[col] = (int)TypeFundamentals.TypeString;
				Console.WriteLine("Column: " + dataTable.Columns[col].ColumnName);
			}
			SetColumnTypes (theTypes);

			// set data in result to tree view store		
			for(int row = 0; row < dataTable.Rows.Count; row++) {
				UpdateDialog ("Loading Row {0}", row.ToString ());			
                                				
				DataRow dataRow = dataTable.Rows[row];
				iter = NewRow ();
				
				for(int cv = 0; cv < dataTable.Columns.Count; cv++) {
					string columnValue = dataRow[dataTable.Columns[cv]].ToString();
					SetColumnValue (iter, cv, columnValue);
				}				
			}
		}

		public static void SetColumnValue (TreeIter iter, int y, string cellValue) {
			GLib.Value cell = new GLib.Value (cellValue);
			store.SetValue (iter, y, cell);	
		}

		// FIXME; this don't belong here
		public static void GetDataFromDatabase () {		
			string connection;
			string sql;
			connection = "Server=DANPC;Database=pubs;User ID=danmorg;Password=freetds";
			sql = "select fname, lname, emp_id, hire_date from employee";
			SqlDataAdapter adapter;
			DataSet dataSet = null;
			adapter = new SqlDataAdapter (sql, connection);
			dataSet = new DataSet ();
			string table = "employee";
			adapter.Fill (dataSet, "employee");
			dataTable = dataSet.Tables["employee"];
		}

		// FIXME: this don't belong
		public static void Main (string[] args) {
			// FIXME: this don't belong here
			GetDataFromDatabase ();

			Application.Init ();
			Idle.Add (new IdleHandler (IdleCB));
			Application.Run ();
		}

		public static bool IdleCB () {
			
			// data binding DataTable to DataGird
			DataSource = dataTable;
			DataBind ();

			// PopulateStore ();
			Window win = new Window ("GTK# DataGrid Demo");
			win.DeleteEvent += new DeleteEventHandler (DeleteCB);
			win.DefaultSize = new Size (640,480);

			ScrolledWindow sw = new ScrolledWindow ();
			win.Add (sw);

			TreeView tv = new TreeView (store);
			tv.HeadersVisible = true;

			AutoCreateTreeViewColumns (tv);

			sw.Add (tv);
			
			dialog.Destroy ();
			dialog = null;

			win.ShowAll ();
			return false;
		}

		private static void AutoCreateTreeViewColumns (TreeView theTreeView) {
			Console.WriteLine("AutoCreateTreeViewColumns BEGIN");

			//for(int col = 0; col < dataTable.Columns.Count; col++) {
			for(int col = 0; col < gridColumns.Length; col++) {
				// escape underscore _ because it is used
				// as the underline in menus and labels
				StringBuilder name = new StringBuilder ();
				//foreach(char ch in dataTable.Columns[col].ColumnName) {
				foreach(char ch in gridColumns[col].ColumnName) {
					if (ch == '_')
						name.Append ("__");
					else
						name.Append (ch);
				}

				TreeViewColumn tvc;
				tvc = CreateColumn(theTreeView, col, name.ToString());
				theTreeView.AppendColumn (tvc);
			}
			Console.WriteLine("AutoCreateTreeViewColumns END");
		}

		private static TreeViewColumn CreateColumn (TreeView theTreeView, int col, string columnName) {
			Console.WriteLine("CreateColumn BEGIN");

			TreeViewColumn NameCol = new TreeViewColumn ();
			CellRenderer NameRenderer = new CellRendererText ();

			NameCol.Title = columnName;
			NameCol.PackStart (NameRenderer, true);
			NameCol.AddAttribute (NameRenderer, "text", col);

			Console.WriteLine("CreateColumn END");

			return NameCol;
		}

		private static void DeleteCB (System.Object o, DeleteEventArgs args) {
			Application.Quit ();
			args.RetVal = true;
		}

		private static void UpdateDialog (string format, params object[] args) {
			string text = String.Format (format, args);

			if (dialog == null) {
				dialog = new Dialog ();
				dialog.Title = "Loading data...";
				dialog.AddButton (Stock.Cancel, 1);
				dialog.Response += new ResponseHandler (ResponseCB);
				dialog.DefaultSize = new Size (480, 100);
					
				VBox vbox = dialog.VBox;
				HBox hbox = new HBox (false, 4);
				vbox.PackStart (hbox, true, true, 0);
				
				Gtk.Image icon = new Gtk.Image (Stock.DialogInfo, IconSize.Dialog);
				hbox.PackStart (icon, false, false, 0);
				dialog_label = new Label (text);
				hbox.PackStart (dialog_label, false, false, 0);
				dialog.ShowAll ();
			} else {
				dialog_label.Text = text;
				while (Application.EventsPending ())
					Application.RunIteration ();
			}
		}

		private static void ResponseCB (object obj, ResponseArgs args) {
			Application.Quit ();
			System.Environment.Exit (0);
		}
	}
}
