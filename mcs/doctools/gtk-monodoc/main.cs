//
// main.cs: Main entry point for the Gtk-based MonoDoc tool
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc.
using GLib;
using Gtk;
using Gdk;
using GtkSharp;
using Gnome;
using Glade;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;

using Mono.Document.Library;

class DocumentationEditor {
	static void Main (string[] args)
	{
		DocumentationEditor de = new DocumentationEditor (args);

		de.Run ();
	}

	// Gnome program
	Program program;

	// Our Glade GUI XML
	Glade.XML gxml;

	// Toplevel window
	Gnome.App main_window;

	// File selector
	Gtk.FileSelection fsel_window;

	// TreeView
	Gtk.TreeView tv;

	//
	Gtk.TreeStore store;

	GLib.Value tree_label_methods, tree_label_properties, tree_label_fields;
	GLib.Value tree_label_operators, tree_label_events, tree_label_constructors;
	
	DocumentationEditor (string [] args)
	{
		program = new Program ("MonoDoc", "0.1", Modules.UI, args);
		gxml = new Glade.XML (null, "gtk-monodoc.glade", null, null);

		main_window = (App) gxml ["main-window"];
		gxml.Autoconnect (this);

		//
		// Customize the TreeView
		Gtk.Container tc = (Gtk.Container) gxml ["tree-container"];
		store = new TreeStore ((int)TypeFundamentals.TypeString);
		tv = new TreeView (store);
		tc.Add (tv);
		tv.Show ();
		
		TreeViewColumn NameCol = new TreeViewColumn ();
		CellRenderer NameRenderer = new CellRendererText ();
		
		NameCol.Title = "Name";
		NameCol.PackStart (NameRenderer, true);
		NameCol.AddAttribute (NameRenderer, "text", 0);
		NameCol.AddAttribute (NameRenderer, "markup", 0);
		tv.AppendColumn (NameCol);

		//
		// Define our labels
		//
		tree_label_methods = new GLib.Value ("Methods");
		tree_label_properties = new GLib.Value ("Properties");
		tree_label_fields = new GLib.Value ("Fields");
		tree_label_operators = new GLib.Value ("Operators");
		tree_label_events = new GLib.Value ("Events");
		tree_label_constructors = new GLib.Value ("Constructors");
	}

	void OnOpenActivate (object sender, EventArgs a)
	{
		if (fsel_window == null){
			fsel_window = new FileSelection ("Select project to open");

			fsel_window.HideFileopButtons ();
			fsel_window.OkButton.Clicked += new EventHandler (FileSelectionOk);
			fsel_window.CancelButton.Clicked += new EventHandler (FileSelectionCancel);
		}
		
		fsel_window.ShowAll ();
	}

	void OnAboutActivate (object sender, EventArgs a)
	{
		Pixbuf pixbuf = new Pixbuf (null, "mono.png");
		
		About about = new About ("Mono Documentation Editor", "0.1",
					 "Copyright (C) 2002 Ximian, Inc.",
					 "",
					 new string [] { "Miguel de Icaza (miguel@ximian.com)" },
					 new string [] { },
					 "", pixbuf);
		about.Run ();
	}

	//
	// string to Hashtable map.
	//
	Hashtable namespaces;
	
	void PopulateTree (DocParser parser)
	{
		store.Clear ();

		//
		// Pull out all the namespaces first
		//
		namespaces = new Hashtable ();
		
		foreach (DocType type in parser.DocTypes){
			string ns = type.Namespace;

			Hashtable h;
			if (!namespaces.Contains (ns)){
				h = new Hashtable ();
				namespaces [ns] = h;
			} else
				h = (Hashtable) namespaces [ns];

			h [type.Name] = type;
		}

		ArrayList l = new ArrayList ();
		foreach (DictionaryEntry de in namespaces)
			l.Add (de.Key);
		l.Sort ();

		TreeIter ns_iter = new TreeIter ();
		foreach (string k in l){
			Hashtable h = (Hashtable) namespaces [k];

			GLib.Value str_namespace = new GLib.Value (k);
			store.Append (out ns_iter);
			store.SetValue (ns_iter, 0, str_namespace);

			ArrayList ch = new ArrayList ();
			foreach (DictionaryEntry de in h)
				ch.Add (de.Key);
			ch.Sort ();
			TreeIter class_iter = new TreeIter ();
			foreach (string n in ch){
				GLib.Value str_class = new GLib.Value (n);
				store.Append (out class_iter, ns_iter);
				store.SetValue (class_iter, 0, str_class);

				DocType type = (DocType) h [n];
				PopulateMember (class_iter, type, tree_label_constructors, type.Constructors, true);
				PopulateMember (class_iter, type, tree_label_methods, type.Methods, true);
				PopulateMember (class_iter, type, tree_label_properties, type.Properties, false);
				PopulateMember (class_iter, type, tree_label_operators, type.Operators, false);
				PopulateMember (class_iter, type, tree_label_fields, type.Fields, false);
				PopulateMember (class_iter, type, tree_label_events, type.Events, false);
			}
		}
	}

	void PopulateMember (TreeIter parent, DocType type, GLib.Value label, ArrayList array, bool is_method)
	{
		if (array.Count == 0)
			return;

		TreeIter group_iter = new TreeIter ();
		store.Append (out group_iter, parent);
		store.SetValue (group_iter, 0, label);

		TreeIter item_iter = new TreeIter ();
		foreach (DocMember member in array){
			string name = member.FullName;
			
			if (is_method & name.IndexOf ("(") == -1)
				name = member.FullName + " ()";
			
			GLib.Value caption = new GLib.Value (name);

			store.Append (out item_iter, group_iter);
			store.SetValue (item_iter, 0, caption);
		}
	}
	
	void FileSelectionOk (object o, EventArgs args)
	{
		string filename = fsel_window.SelectionEntry.Text;

		if (File.Exists (filename)){
			DocParser parser = new DocParser (filename);
			PopulateTree (parser);

			string basename;
			int p = filename.IndexOf ("/");
			if (p != -1)
				basename = filename.Substring (p + 1);
			else
				basename = filename;
			
			main_window.Title = "MonoDoc: " + filename;
		} else {
			Error (String.Format ("File {0} does not exist", filename));
		}
		fsel_window.Hide ();
	}

	public static void Error (string msg)
	{
		MessageDialog d = new MessageDialog (null, 0, MessageType.Error, ButtonsType.Ok, msg);
		
		d.Run ();
		d.Destroy ();
	}
	
	void FileSelectionCancel (object o, EventArgs args)
	{
		fsel_window.Hide ();
	}
	
	void Run ()
	{
		program.Run ();
	}
}
