// monodoc.cs - Qt# Mono Documentation Tool
//
// Author: Adam Treat <manyoso@yahoo.com>
// (c) 2002 Adam Treat
// Licensed under the terms of the GNU GPL

namespace Mono.Document.Editor {

	using Qt;
	using System;
	using System.IO;
	using System.Text;
	using System.Reflection;
	using System.Collections;
	using Mono.Document.Library;

	[DeclareQtSignal ("Load (String)")]
	public class MonoDoc : QMainWindow {

		private DocEdit docedit;
		private ListView listview;
		private QMenuBar menubar;
		private QPopupMenu filemenu;
		private QPopupMenu settingsmenu;
		private QPopupMenu aboutmenu;
		private Options options;

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

			settingsmenu = new QPopupMenu (null, "settingsmenu");
			settingsmenu.InsertItem ("&Configure MonoDoc...", this, SLOT ("Options ()"));

			aboutmenu = new QPopupMenu (null, "helpmenu");
			aboutmenu.InsertItem ("&About MonoDoc", this, SLOT ("AboutMonoDoc ()"));

			menubar = new QMenuBar (this, "");
			menubar.InsertItem ("&File", filemenu);
			menubar.InsertItem ("&Settings", settingsmenu);
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

			options = new Options (this);
			Connect (options, SIGNAL ("OkClicked ()"), this, SLOT ("WriteInit ()"));

			SetCentralWidget (split);
			Configure ();
		}

		public void OpenFile ()
		{
			string filename = QFileDialog.GetOpenFileName (Global.InitDir, "*.xml", this, "open", "Open Master File", "*.xml", true);

			if (filename != null) {
				Global.LastOpened = filename;
				Global.InitDir = Path.GetDirectoryName (filename);
				WriteInit ();
				Emit ("Load (String)", filename);
			}
		}

		public void AboutMonoDoc ()
		{
			QMessageBox.Information (this, "About MonoDoc", "MonoDoc - The Mono Documentation Editor");
		}

		private void Configure ()
		{
			SetGeometry (50, 500, 800, 600);
			SetCaption ("MonoDoc -  The Mono Documentation Editor");
			SetIcon (Global.IMono);

			if (!Directory.Exists (Global.MonoDocDir))
				Directory.CreateDirectory (Global.MonoDocDir);

			if (File.Exists (Global.MonoDocDir+"/monodoc")) {
				StreamReader st = File.OpenText (Global.MonoDocDir+"/monodoc");
				Global.InitDir = st.ReadLine ();
				Global.LastOpened = st.ReadLine ();
				if (Global.LastOpened != null) {
					options.IsChecked (true);
					Emit ("Load (String)", Global.LastOpened);
				}
			}
		}

		public void WriteInit ()
		{
			StreamWriter st = File.CreateText (Global.MonoDocDir+"/monodoc");
			st.WriteLine (Global.InitDir);
			if (options.OpenPrevious)
				st.WriteLine (Global.LastOpened);
			st.Flush ();
		}

		public void Options ()
		{
			options.Show ();
		}
	}
}
