// monodoc.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Util.MonoDoc.Qt {

	using Qt;
	using System;
	using System.IO;
	using System.Reflection;
	using System.Collections;
	using Mono.Util.MonoDoc.Lib;

	[DeclareQtSignal ("Load (String)")]
	public class MonoDoc : QMainWindow {

		private DocEdit docedit;
		private ListView listview;
		private QMenuBar menubar;
		private QPopupMenu filemenu;
		private QPopupMenu aboutmenu;

		public static int Main (String[] args)
		{
			QApplication app = new QApplication (args);
			MonoDoc monodoc = new MonoDoc ();
			monodoc.Show ();
			app.SetMainWidget (monodoc);
			return app.Exec ();
		}

		public MonoDoc () : base (null)
		{
			filemenu = new QPopupMenu (null, "filemenu");
			filemenu.InsertItem ("&Open", this, SLOT ("OpenFile ()"));
			filemenu.InsertItem ("&Quit", qApp, SLOT ("Quit ()"));

			aboutmenu = new QPopupMenu (null, "helpmenu");
			aboutmenu.InsertItem ("&About MonoDoc", this, SLOT ("AboutMonoDoc ()"));

			menubar = new QMenuBar (this, "");
			menubar.InsertItem ("&File", filemenu);
			menubar.InsertItem ("&About", aboutmenu);

			QSplitter split = new QSplitter (this);
			listview = new ListView (split);
			docedit = new DocEdit (split);
			split.SetOpaqueResize (true);
			split.SetResizeMode (listview, QSplitter.ResizeMode.FollowSizeHint);
			split.SetResizeMode (docedit, QSplitter.ResizeMode.FollowSizeHint);

			Connect (this, SIGNAL ("Load (String)"), listview, SLOT ("LoadFile (String)"));
			Connect (	listview, SIGNAL ("selectionChanged (QListViewItem)"),
						docedit, SLOT ("ListViewChanged (QListViewItem)"));

			SetCentralWidget (split);
			Configure ();
		}

		public void OpenFile ()
		{
			string filename = QFileDialog.GetOpenFileName (Global.InitDir, "*.xml", this, "open", "Open Master File", "*.xml", true);
			WriteInit (filename);
			if (filename != null)
				Emit ("Load (String)", filename);
		}

		public void AboutMonoDoc ()
		{
			QMessageBox.Information (this, "About MonoDoc", "MonoDoc - The Mono Documentation Editor");
		}

		private void Configure ()
		{
			if (!Directory.Exists (Global.MonoDocDir))
				Directory.CreateDirectory (Global.MonoDocDir);

			if (File.Exists (Global.MonoDocDir+"/monodoc")) {
				StreamReader st = File.OpenText (Global.MonoDocDir+"/monodoc");
				Global.InitDir = st.ReadLine ();
			}

			SetGeometry (50, 500, 800, 600);
			SetCaption ("MonoDoc -  The Mono Documentation Editor");
			SetIcon (Global.IMono);
		}

		public void WriteInit (string filename)
		{
			Global.InitDir = filename;
			StreamWriter st = File.CreateText (Global.MonoDocDir+"/monodoc");
			st.WriteLine (Global.InitDir);
			st.Flush ();
		}

		private class ListView : QListView {

			QListViewItem _namespace;

			public ListView (QWidget parent) : base (parent)
			{
				SetRootIsDecorated (true);
				AddColumn ("Namespace");
				//Console.WriteLine ("ListView "+SizeHint ().Width ());
            }

			public void LoadFile (string filename)
			{
				if(File.Exists (filename)) {
					DocParser parser = new DocParser (filename);
					foreach(DocType type in parser.DocTypes) {
						ListItem typeitem = new ListItem (GetNamespaceItem (type.Namespace), type.Name, type);
						ProcessMember (type.Ctors, typeitem);
						ProcessMember (type.Dtors, typeitem);
						ProcessMember (type.Methods, typeitem);
						ProcessMember (type.Fields, typeitem);
						ProcessMember (type.Properties, typeitem);
						ProcessMember (type.Events, typeitem);
					}
            	}
			}

			public void ProcessMember (ArrayList array, QListViewItem parent)
			{
				foreach (DocMember member in array) {
					if (parent != null && member != null) {
						new ListItem (parent, member.Name+" "+member.Args, member);
					}
				}
			}

			public QListViewItem GetNamespaceItem (string name)
			{
				_namespace = FindItem (name, 0);
				if (_namespace == null) {
					//Console.WriteLine ("ListView "+SizeHint ().Width ());
					return new ListItem (this, name);
				} else {
					return _namespace;
				}
			}
		}

		private class DocEdit : QWidgetStack {

			public DocEdit (QWidget parent) : base (parent)
			{
				SetMargin (10);
				//Console.WriteLine ("DocEdit "+SizeHint ().Width ());
			}

			public void ListViewChanged (QListViewItem item)
			{
				ListItem listItem = (item as ListItem);
				if (listItem.IsName)
					return;
				if (listItem != null && !listItem.IsBuilt)
					AddWidget (listItem.BuildEditForm ());
				RaiseWidget (listItem.EditForm);
				//Console.WriteLine ("DocEdit "+SizeHint ().Width ());
			}
		}

		private class ListItem : QListViewItem {

			DocType type;
			DocMember member;
			public bool IsBuilt, IsName= false;
			public QWidget EditForm;
			
			public ListItem (QListView parent, string text) : base (parent, text)
			{
				IsName = true;
				SetPixmap (0, Global.IName);
			}

			public ListItem (QListViewItem parent, string text, DocMember member) : base (parent, text)
			{
				this.member = member;
				if (member.IsCtor || member.IsMethod || member.IsDtor)
					SetPixmap (0, Global.IMethod);
				else if (member.IsField)
					SetPixmap (0, Global.IField);
				else if (member.IsProperty)
					SetPixmap (0, Global.IProperty);
				else if (member.IsEvent)
					SetPixmap (0, Global.IEvent);

			}

			public ListItem (QListViewItem parent, string text, DocType type) : base (parent, text)
			{
				this.type = type;
				if (type.IsClass)
					SetPixmap (0, Global.IClass);
				else if (type.IsStructure)
					SetPixmap (0, Global.IStructure);
				else if (type.IsInterface)
					SetPixmap (0, Global.IInterface);
				else if (type.IsDelegate)
					SetPixmap (0, Global.IDelegate);
				else if (type.IsEnum)
					SetPixmap (0, Global.IEnum);
			}

			public QWidget BuildEditForm ()
			{
				if (type != null)
					EditForm = new TypeEdit (type.Name);
				else if (member != null)
					EditForm = new ParamEdit (member.Name+" "+member.Args, member.Params);
				IsBuilt = true;
				return EditForm;
			}
		}

		private class TypeEdit : QGroupBox {

			public TypeEdit (string name) : base (name)
			{
				AddSpace (1);
			}
		}

		private class ParamEdit : QVGroupBox {

			public ParamEdit (string name, ArrayList _params) : base (name)
			{
				foreach (DocParam param in _params)
				{
					QHBox hbox = new QHBox (this);
					QLabel label = new QLabel (hbox);
					label.SetText (param.Name);
					QLineEdit edit = new QLineEdit (hbox);
				}
			}
		}
	}
}
