//
// main.cs: Main entry point for the Gtk-based MonoDoc tool
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

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

	Gtk.Notebook notebook;
	
	DocumentationEditor (string [] args)
	{
		program = new Program ("MonoDoc", "0.1", Modules.UI, args);
		gxml = new Glade.XML (null, "gtk-monodoc.glade", null, null);

		LoadWidgetPointers ();

		//
		// Customize the TreeView
		Gtk.Container tc = (Gtk.Container) gxml ["tree-container"];
		store = new TreeStore ((int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString);
		tv = new TreeView (store);
		tv.Selection.Mode = SelectionMode.Single;
		tv.Selection.Changed += new EventHandler (TreeSelectionChanged);
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

	Gtk.Label namespace_name;
	Gtk.TextView namespace_summary, namespace_remarks;

	Gtk.Label class_name;
	Gtk.TextView class_summary, class_remarks;
	Gtk.Entry class_see_also;
	Gtk.CheckButton class_thread_safe;

	Gtk.Label method_name;
	Gtk.TextView method_summary, method_remarks, method_exceptions, method_return, method_parameters;
	Gtk.Entry method_see_also, method_example;

	Gtk.Label property_name;
	Gtk.TextView property_summary, property_remarks, property_value;
	Gtk.Entry property_see_also, property_example;

	Gtk.Label field_name;
	Gtk.TextView field_summary, field_remarks;
	Gtk.Entry field_see_also, field_example;
	
	void LoadWidgetPointers ()
	{
		main_window = (App) gxml ["main-window"];
		gxml.Autoconnect (this);
		notebook = (Notebook) gxml ["notebook"];
		notebook.ShowTabs = false;

		namespace_name = (Gtk.Label) gxml ["namespace-name"];
		namespace_summary = (Gtk.TextView) gxml ["namespace-summary"];
		namespace_remarks = (Gtk.TextView) gxml ["namespace-remarks"];

		class_name = (Gtk.Label) gxml ["class-name"];
		class_summary = (Gtk.TextView) gxml ["class-summary"];
		class_remarks = (Gtk.TextView) gxml ["class-remarks"];
		class_thread_safe = (Gtk.CheckButton)gxml ["class-thread-safe"];
		class_see_also = (Gtk.Entry) gxml ["class-see-also"];

		method_name = (Gtk.Label) gxml ["method-name"];
		method_summary = (Gtk.TextView) gxml ["method-summary"];
		method_parameters = (Gtk.TextView) gxml ["method-parameters"];
		method_return = (Gtk.TextView) gxml ["method-return-value"];
		method_exceptions = (Gtk.TextView) gxml ["method-exceptions"];
		method_remarks = (Gtk.TextView) gxml ["method-remarks"];
		method_see_also = (Gtk.Entry) gxml ["method-see-also"];
		method_example = (Gtk.Entry) gxml ["method-example"];

		property_name = (Gtk.Label) gxml ["property-name"];
		property_summary = (Gtk.TextView) gxml ["property-summary"];
		property_value = (Gtk.TextView) gxml ["property-value"];
		property_remarks = (Gtk.TextView) gxml ["property-remarks"];
		property_see_also = (Gtk.Entry) gxml ["property-see-also"];
		property_example = (Gtk.Entry) gxml ["property-example"];

		field_name = (Gtk.Label) gxml ["field-name"];
		field_summary = (Gtk.TextView) gxml ["field-summary"];
		field_remarks = (Gtk.TextView) gxml ["field-remarks"];
		field_see_also = (Gtk.Entry) gxml ["field-see-also"];
		field_example = (Gtk.Entry) gxml ["field-example"];
	}

	void LoadConstructor (string name)
	{
		notebook.Page = 2;
		method_name.Text = "Constructor: " + name;
	}
	
	void LoadNamespace (string name)
	{
		notebook.Page = 0;
		namespace_name.Text = name;
	}

	void LoadClass (string name)
	{
		notebook.Page = 1;
		class_name.Text = name;
	}

	void LoadMethod (string name)
	{
		notebook.Page = 2;

		if (name.IndexOf ("(") == -1)
			method_name.Text = name + " ()";
		else
			method_name.Text = name;
	}

	void LoadProperty (string name)
	{
		notebook.Page = 3;
		property_name.Text = name;
	}
	
	void LoadField (string name)
	{
		notebook.Page = 4;
		field_name.Text = name;
	}

	void LoadOperator (string name)
	{
		notebook.Page = 5;
	}

	void LoadEvent (string name)
	{
		notebook.Page = 6;
	}

	void TreeSelectionChanged (object sender, EventArgs a)
	{
		Gtk.TreeIter iter = new Gtk.TreeIter ();
		Gtk.TreeModel model;

		if (tv.Selection.GetSelected (out model, ref iter)){
			GLib.Value val = new GLib.Value ();
			
			store.GetValue (iter, 1, val);
			string key = (string) val;
			
			if (key == "namespace"){
				GLib.Value ns = new GLib.Value ();
				store.GetValue (iter, 0, ns);
				LoadNamespace ((string) ns);
			} else if (key.StartsWith ("class:"))
				LoadClass (key.Substring (6));
			else if (key.StartsWith ("mc"))
				LoadConstructor (key.Substring (2));
			else if (key.StartsWith ("mm"))
				LoadMethod (key.Substring (2));
			else if (key.StartsWith ("mp"))
				LoadProperty (key.Substring (2));
			else if (key.StartsWith ("mo"))
				LoadOperator (key.Substring (2));
			else if (key.StartsWith ("mf"))
				LoadField (key.Substring (2));
			else if (key.StartsWith ("me"))
				LoadEvent (key.Substring (2));
		}
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

	void OnQuitActivate (object sender, EventArgs a)
	{
		program.Quit ();
	}

	void OnAboutActivate (object sender, EventArgs a)
	{
		Pixbuf pixbuf = new Pixbuf (null, "mono.png");
		
		About about = new About ("Mono Documentation Editor", "0.1",
					 "Copyright (C) 2002 Ximian, Inc.",
					 "",
					 new string [] { "Miguel de Icaza (miguel@ximian.com)", "Duncan Mak (duncan@ximian.com" },
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
		GLib.Value ns_value = new GLib.Value ("namespace");
		foreach (string k in l){
			Hashtable h = (Hashtable) namespaces [k];

			GLib.Value str_namespace = new GLib.Value (k);
			store.Append (out ns_iter);
			store.SetValue (ns_iter, 0, str_namespace);
			store.SetValue (ns_iter, 1, ns_value);

			ArrayList ch = new ArrayList ();
			foreach (DictionaryEntry de in h)
				ch.Add (de.Key);
			ch.Sort ();
			TreeIter class_iter = new TreeIter ();
			foreach (string n in ch){
				string full = k + "." + n;
				GLib.Value str_class = new GLib.Value (n);
				store.Append (out class_iter, ns_iter);
				store.SetValue (class_iter, 0, str_class);
				store.SetValue (class_iter, 1, new GLib.Value ("class:" + full));

				DocType type = (DocType) h [n];
				PopulateMember (class_iter, type, "c", tree_label_constructors, type.Constructors, true);
				PopulateMember (class_iter, type, "m", tree_label_methods, type.Methods, true);
				PopulateMember (class_iter, type, "p", tree_label_properties, type.Properties, false);
				PopulateMember (class_iter, type, "o", tree_label_operators, type.Operators, false);
				PopulateMember (class_iter, type, "f", tree_label_fields, type.Fields, false);
				PopulateMember (class_iter, type, "e", tree_label_events, type.Events, false);
			}
		}
	}

	GLib.Value nothing;

	void PopulateMember (TreeIter parent, DocType type, string p, GLib.Value label, ArrayList array, bool is_method)
	{
		if (array.Count == 0)
			return;

		if (nothing == null)
			nothing = new GLib.Value ("nothing");
		
		TreeIter group_iter = new TreeIter ();
		store.Append (out group_iter, parent);
		store.SetValue (group_iter, 0, label);
		store.SetValue (group_iter, 1, nothing);

		TreeIter item_iter = new TreeIter ();
		foreach (DocMember member in array){
			string name = member.FullName;
			
			if (is_method & name.IndexOf ("(") == -1)
				name = member.FullName + " ()";
			
			GLib.Value caption = new GLib.Value (name);

			store.Append (out item_iter, group_iter);
			store.SetValue (item_iter, 0, caption);
			GLib.Value data = new GLib.Value ("m" + p  + type.Namespace + "." + type.Name + ":" + member.FullName);
			store.SetValue (item_iter, 1, data);
		}
	}
	
	void FileSelectionOk (object o, EventArgs args)
	{
		string filename = fsel_window.Filename;

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

public class DocumentableObject {
	protected string name;

	public DocumentableObject (string name)
	{
		this.name = name;
	}
}

public class Class : DocumentableObject {
	public Class (string name) : base (name)
	{
	}
}

public class Namespace : DocumentableObject {
	public Namespace (string name) : base (name)
	{
	}
}
