//
// GtkTypeDisplayer.cs: 
//   Display types using Gtk#.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

// #define TRACE

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

// for GUI support
using GLib;
using Glade;
using Gtk;
using GtkSharp;
using System.Drawing;

namespace Mono.TypeReflector
{
	delegate void OpenFileSuccess (string filename);

	class OpenFileDialog
	{
		private FileSelection file = new FileSelection ("Open an Assembly");
		private OpenFileSuccess onSuccess;

		public OpenFileDialog (OpenFileSuccess success)
		{
			onSuccess = success;
			file.DeleteEvent += new DeleteEventHandler (delete_event);
			file.CancelButton.Clicked += new EventHandler (cancel_event);
			file.OkButton.Clicked += new EventHandler (ok_event);
			file.Show ();
		}

		private void delete_event (object o, DeleteEventArgs args)
		{
			cancel_event (o, args);
		}

		private void cancel_event (object o, EventArgs args)
		{
			file.Hide ();
		}

		private void ok_event (object o, EventArgs args)
		{
			file.Hide ();
			onSuccess (file.Filename);
		}
	}

	public class GtkTypeDisplayer : TypeDisplayer
	{
		private TreeStore store;
		private Window mainWindow;
		private Statusbar statusbar;
		private Window aboutWindow;
		private static int windows = 0;

		public override int MaxDepth {
			set {/* ignore */}
		}

		public override bool RequireTypes {
			get {return false;}
		}

		public GtkTypeDisplayer ()
		{
      Console.WriteLine ("GtkTypeDisplayer created");
			if (++windows == 1)
				Application.Init ();

			InitMainWindow ();
		}

		private void InitMainWindow ()
		{
			Glade.XML gxml = new Glade.XML (null, "gtk/type-reflector.glade", "main_window", null);
			try {
				gxml.Autoconnect (this);
			} catch (Exception e) {
				Console.WriteLine ("** Error with glade: " + e.ToString());
				throw;
			}

			store = new TreeStore ((int) TypeFundamentals.TypeString);

			TreeView tv = (TreeView) gxml ["treeview"];
			tv.Model = store;
			// tv.HeadersVisible = true;
			tv.HeadersVisible = false;
			tv.RowExpanded += new RowExpandedHandler (RowExpanded);

			TreeViewColumn nameCol = new TreeViewColumn ();
			CellRenderer nameRenderer = new CellRendererText ();
			nameCol.Title = "Stuff";
			nameCol.PackStart (nameRenderer, true);
			nameCol.AddAttribute (nameRenderer, "text", 0);
			tv.AppendColumn (nameCol);

			mainWindow = (Window) gxml ["main_window"];
			statusbar = (Statusbar) gxml ["status_bar"];
		}

		private void RowExpanded (object o, RowExpandedArgs args)
		{
			Console.WriteLine ("** row expanded!");
			Console.WriteLine ("  ** Iter={0}, Path={1}", args.Iter, args.Path);
			/*
			NodeValue v = new NodeValue ();
			store.GetValue (args.Iter, 0, v);
			string s = (string) v;
			Console.WriteLine ("  ** v.Node={0}; v={1}", v.Node, s);
			GLib.Value gv = new GLib.Value ();
			store.GetValue (args.Iter, 1, gv);
			GLib.Object go = (GLib.Object) gv;
			NodeInfo ni = (NodeInfo) go.GetData ("mykey");
			Console.WriteLine ("  ** ni={0}", ni);
			 */
		}

		private Node CreateNode (Type type)
		{
			Node root = new Node (Formatter, Finder);
			root.NodeInfo = new NodeInfo (null, type);
			return root;
		}

		public override void AddType (Type type)
		{
			Console.WriteLine ("Adding Type: " + type.FullName);
			base.AddType (type);
		}

		public override void Run ()
		{
			ShowTypes();
			Application.Run ();
		}

		private void ShowTypes ()
		{
			Console.WriteLine ("Showing Types...");
			foreach (Assembly a in Assemblies) {
				Console.WriteLine ("Assembly: " + a.FullName);
				TreeIter ai;
				store.Append (out ai);
				store.SetValue (ai, 0, new GLib.Value (string.Format ("Assembly: {0}", a.FullName)));

				foreach (string ns in Namespaces (a)) {
					Console.WriteLine ("Namespace: " + ns);
					TreeIter ni;
					store.Append (out ni, ai);
					store.SetValue (ni, 0, new GLib.Value (string.Format ("Namespace: {0}", ns)));
					foreach (Type type in Types (a, ns))
						AddType (type, ni);
				}
			}
			Console.WriteLine ("----");

			mainWindow.ShowAll ();
		}
		
		private void AddType (Type type, TreeIter parent)
		{
			Console.WriteLine ("Type: " + type.FullName);
			TreeIter p;
			store.Append (out p, parent);
			// store.SetValue (p, 0, new NodeValue (CreateNode(type)));
			NodeInfo r = new NodeInfo (null, type);
			store.SetValue (p, 0, new GLib.Value (Formatter.GetDescription (r)));

			foreach (NodeInfo ni in Finder.GetChildren (r)) {
				TreeIter i;
				store.Append (out i, p);
				store.SetValue (i, 0, 
					new GLib.Value (Formatter.GetDescription (ni)));
				TreeIter j;
				store.Append (out j, i);
				store.SetValue (j, 0, new GLib.Value ("Dummy"));
			}
		}

		// Gtk#/Glade# Required Functions...
		public void on_main_window_delete_event (object o, DeleteEventArgs args) 
		{
			on_file_quit_activate (o, args);
		}

		public void on_file_new_activate (object o, EventArgs args)
		{
			/* ignore */
		}

		public void on_file_open_activate (object o, EventArgs args)
		{
			OpenFileDialog ofd = new OpenFileDialog (new OpenFileSuccess (OpenAssembly));
		}

		private void OpenAssembly (string assembly)
		{
			GtkTypeDisplayer d = new GtkTypeDisplayer ();
			d.Finder = Finder;
			d.Formatter = Formatter;
			d.Options = Options;
			TypeLoader tl = TypeReflectorApp.CreateLoader (Options);
      tl.Assemblies = new string[]{assembly};
			try {
				TypeReflectorApp.FindTypes (d, tl, new string[]{"."});
				d.ShowTypes ();
			}
			catch (Exception e) {
				// should be an Alert.
				Console.WriteLine ("Unable to load Assembly '{0}': {1}",
						assembly, e.ToString());
			}
		}

    public void on_ok_button_clicked (object o, EventArgs args)
    {
      Console.WriteLine ("ok clicked; args=" + args);
    }

		public void on_file_save_activate (object o, EventArgs args)
		{
			/* ignore */
		}

		public void on_file_save_as_activate (object o, EventArgs args)
		{
			/* ignore */
		}

		public void on_file_quit_activate (object o, EventArgs args)
		{
			if (--windows == 0)
				Application.Quit ();
		}

		public void on_edit_cut_activate (object o, EventArgs args)
		{
			/* ignore */
		}

		public void on_edit_copy_activate (object o, EventArgs args)
		{
			/* ignore */
		}

		public void on_edit_paste_activate (object o, EventArgs args)
		{
			/* ignore */
		}

		public void on_edit_delete_activate (object o, EventArgs args)
		{
			/* ignore */
		}

		public void on_view_formatter_default_activate (object o, EventArgs args)
		{
		}

		public void on_view_formatter_vb_activate (object o, EventArgs args)
		{
		}

		public void on_view_formatter_csharp_activate (object o, EventArgs args)
		{
		}

		public void on_view_finder_reflection_activate (object o, EventArgs args)
		{
		}

		public void on_view_finder_explicit_activate (object o, EventArgs args)
		{
		}

		public void on_help_about_activate (object o, EventArgs args)
		{
			/* ignore */
			TypeReflectorApp.PrintVersion ();
			Glade.XML gxml = new Glade.XML (null, "gtk/type-reflector.glade", "about_window", null);
			try {
				gxml.Autoconnect (this);
			} catch (Exception e) {
				Console.WriteLine ("** Error with glade: " + e.ToString());
				throw;
			}

			aboutWindow = (Window) gxml ["about_window"];
		}
	}
}

