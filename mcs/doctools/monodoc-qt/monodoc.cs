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
			Progress progress;

			public ListView (QWidget parent) : base (parent)
			{
				SetRootIsDecorated (true);
				AddColumn ("Namespace");
				SetSorting (-1);
				progress = new Progress (this);
            }

			public void LoadFile (string filename)
			{
				if(File.Exists (filename)) {
					progress.Show ();
					DocParser parser = new DocParser (filename);
					progress.SetTotalSteps (parser.DocTypes.Count - 1);
					progress.SetLabelText ("Loading Master.xml file");
					SetUpdatesEnabled (false);
					int i = 0;
					foreach(DocType type in parser.DocTypes) {
						progress.SetProgress (i++);
						ListItem typeitem = new ListItem (GetNamespaceItem (type.Namespace), type.Name, type);
						ProcessMember (type.Dtors, typeitem);
						ProcessMember (type.Events, typeitem);
						ProcessMember (type.Fields, typeitem);
						ProcessMember (type.Properties, typeitem);
						ProcessMember (type.Methods, typeitem);
						ProcessMember (type.Ctors, typeitem);
					}
					SetUpdatesEnabled (true);
            		Repaint ();
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
					SetUpdatesEnabled (true);
					_namespace = new ListItem (this, name);
            		Repaint ();
					SetUpdatesEnabled (false);
				}
				return _namespace;
			}
		}

		private class DocEdit : QWidgetStack {

			public DocEdit (QWidget parent) : base (parent)
			{
				SetMargin (10);
			}

			public void ListViewChanged (QListViewItem item)
			{
				ListItem listItem = (item as ListItem);
				if (listItem.IsName)
					return;
				if (listItem != null && !listItem.IsBuilt)
					AddWidget (listItem.BuildEditForm ());
				RaiseWidget (listItem.EditForm);
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

		private class TypeEdit : QVGroupBox {

			public TypeEdit (string name) : base (name)
			{
				SetInsideMargin (20);
				QLabel sum = new QLabel (
					"<summary> Provide a brief (usually one sentence) description of a member or type.",
					 this);
				QLineEdit sumedit = new QLineEdit (this);

				QLabel rem = new QLabel (
					"<remarks> Provide verbose information for a type or member.",
					this);
				QTextEdit remedit = new QTextEdit (this);
				remedit.SetTextFormat (Qt.TextFormat.RichText);

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

		private class Progress : QProgressDialog {
			
			QPushButton pb;
			
			public Progress (QWidget parent) : base (parent, "", true)
			{
				SetLabelText ("Parsing Master.xml file");
				SetTotalSteps (500);
				pb = new QPushButton (null);
				SetCancelButton (pb);
				pb.Hide ();
			}

			new public void Show ()
			{
				ForceShow ();
				SetProgress (1);
			}
		}
	}
}
