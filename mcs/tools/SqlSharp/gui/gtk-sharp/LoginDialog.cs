// LoginDialog.cs
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//
// (C)Copyright 2002 by Daniel Morgan
//
// To be included with Mono as a SQL query tool licensed under the GPL license.
//

namespace Mono.Data.SqlSharp.Gui.GtkSharp {
	using System;
	using System.Collections;
	using System.Data;
	using System.Drawing;
	using System.Text;
	using System.IO;
	using Gtk;
	using GtkSharp;
	using SqlEditorSharp;
	using System.Reflection;
	using System.Runtime.Remoting;
	using System.Diagnostics;

	public class LoginDialog {
		
		Dialog dialog;
		Entry connection_entry;
		Entry provider_entry;
		SqlSharpGtk sqlSharp;
		OptionMenu providerOptionMenu;

		public LoginDialog(SqlSharpGtk sqlSharpGtk) { 
			sqlSharp = sqlSharpGtk;
			CreateGui();
		}

		public void CreateGui() {

			dialog = new Dialog ();
			dialog.Title = "Login";
			dialog.BorderWidth = 3;
			dialog.VBox.BorderWidth = 5;
			dialog.HasSeparator = false;

			Frame frame = new Frame ("Connection");
			string image = Stock.DialogInfo;
			
			HBox hbox = new HBox (false, 2);
			hbox.BorderWidth = 5;
			hbox.PackStart (new Gtk.Image (image, IconSize.Dialog), true, true, 0);
		
			Table table = new Table (2, 3, false);
			hbox.PackStart (table);
			table.ColSpacings = 4;
			table.RowSpacings = 4;
			Label label = null;

			label = Label.NewWithMnemonic ("_Provider");
			table.Attach (label, 0, 1, 0, 1);
			providerOptionMenu = CreateProviderOptionMenu();
			table.Attach (providerOptionMenu, 1, 2, 0, 1);
			
			label = Label.NewWithMnemonic ("_Connection String");
			table.Attach (label, 0, 1, 1, 2);
			connection_entry = new Entry ();
			table.Attach (connection_entry, 1, 2, 1, 2);

			frame.Add (hbox);

			dialog.VBox.PackStart (frame, true, true, 0);

			Button button = null;
			button = Button.NewFromStock (Stock.Ok);
			button.Clicked += new EventHandler (Connect_Action);
			button.CanDefault = true;
			dialog.ActionArea.PackStart (button, true, true, 0);
			button.GrabDefault ();

			button = Button.NewFromStock (Stock.Cancel);
			button.Clicked += new EventHandler (Dialog_Cancel);
			dialog.ActionArea.PackStart (button, true, true, 0);
			dialog.Modal = true;

			dialog.ShowAll ();
		}

		public OptionMenu CreateProviderOptionMenu() {

			OptionMenu optionMenu = new OptionMenu();
			
			Menu providerMenu = new Menu ();
			MenuItem menuItem;
			
			for(int i = 0; i < sqlSharp.providerList.Count; i++) {
				DbProvider p = sqlSharp.providerList[i];
				menuItem = new MenuItem(p.Name);
				providerMenu.Append (menuItem);
			}	
			
			optionMenu.Menu = providerMenu;
			optionMenu.Changed += new EventHandler (provider_changed_cb);

			optionMenu.SetHistory(0);
			SetProviderSelection(0);

			return optionMenu;
		}

		void provider_changed_cb (object o, EventArgs args) {
			int selected = providerOptionMenu.History;
			SetProviderSelection(selected);
		}

		void SetProviderSelection (int selected) {
			sqlSharp.dbProvider = sqlSharp.providerList[selected];
		}

		void Connect_Action (object o, EventArgs args) {
			try {
				string connection = "";

				connection = connection_entry.Text;

				sqlSharp.connectionString = connection;				
				sqlSharp.OpenDataSource();
				
			} catch (Exception e) {
				sqlSharp.AppendText(sqlSharp.buf, 
					"Error: Unable to connect.");
			}
			dialog.Destroy ();
			dialog = null;
		}

		void Dialog_Cancel (object o, EventArgs args) {
			dialog.Destroy ();
			dialog = null;
		}
	}
}
