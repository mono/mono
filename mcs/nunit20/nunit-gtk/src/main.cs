//
// main.cs: nunit-gtk
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Gdk;
using GLib;
using Gnome;
using Gtk;
using GtkSharp;
using NUnit.Core;

namespace Mono.NUnit.GUI
{
	class ErrorDialog
	{
		[Glade.Widget]
		Dialog errorDialog;

		[Glade.Widget]
		Label message;
		
		public ErrorDialog (string title, string text)
		{
			Glade.XML gXML = new Glade.XML (null, "nunit-gtk.glade", "errorDialog", null);
			gXML.Autoconnect (this);
			errorDialog.Title = title;
			errorDialog.Response += new ResponseHandler (OnResponse);
			message.Markup = text;
		}

		void OnResponse (object sender, ResponseArgs args)
		{
		}

		public void Run ()
		{
			errorDialog.Run ();
			errorDialog.Destroy ();
		}
	}

	public class NUnitGUI : Program, EventListener
	{
		static string version;
		static string title;
		static string copyright;
		static string description;
		static readonly string gconfPath = "/apps/mononunitgui/";
		
		[Glade.Widget("nunitgui")]
		Gtk.Window window;
		[Glade.Widget]
		AppBar appbar;

		// Run frame
		[Glade.Widget]
		ProgressBar frameProgress;
		[Glade.Widget]
		Label frameLabel;

		// Menu
		[Glade.Widget]
		ImageMenuItem menuRecent;

		// Notebook
		[Glade.Widget]
		TreeView failures;
		[Glade.Widget]
		Label failuresLabel;

		[Glade.Widget]
		TreeView notRun;
		[Glade.Widget]
		Label notRunLabel;

		[Glade.Widget]
		TextView stdoutTV;
		[Glade.Widget]
		Label stdoutLabel;

		[Glade.Widget]
		TextView stderrTV;
		[Glade.Widget]
		Label stderrLabel;

		Label [] nbLabels;
		//

		[Glade.Widget]
		TreeView assemblyView;
		[Glade.Widget]
		Paned hpaned;

		string [] args;
		AssemblyStore store;
		TreeStore notRunStore;
		TreeStore failuresStore;
		int ntests;
		int finishedTests;
		TextWriter origStdout = Console.Out;
		TextWriter origStderr = Console.Error;
		StringWriter stdout = new StringWriter ();
		StringWriter stderr = new StringWriter ();
		Hashtable errorIters;

		static NUnitGUI ()
		{
			Assembly assembly = Assembly.GetExecutingAssembly ();
			version = assembly.GetName ().Version.ToString ();
			object [] att = assembly.GetCustomAttributes (typeof (AssemblyTitleAttribute), false);
			title = ((AssemblyTitleAttribute) att [0]).Title;
			att = assembly.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			copyright = ((AssemblyCopyrightAttribute) att [0]).Copyright;
			att = assembly.GetCustomAttributes (typeof (AssemblyDescriptionAttribute), false);
			description = ((AssemblyDescriptionAttribute) att [0]).Description;
		}
		
		public NUnitGUI (string [] args, params object [] props)
			: base (title, version, Modules.UI, args, props)
		{
			Glade.XML gXML = new Glade.XML (null, "nunit-gtk.glade", "nunitgui", null);
			gXML.Autoconnect (this);
			nbLabels = new Label [] {failuresLabel, notRunLabel, stderrLabel, stdoutLabel};

			this.args = args;

			CellRendererPixbuf pr = new CellRendererPixbuf ();
			CellRendererText tr = new CellRendererText ();
			TreeViewColumn nameCol = new TreeViewColumn ();
			nameCol.PackStart (pr, false);
			nameCol.SetCellDataFunc (pr, CircleRenderer.CellDataFunc, IntPtr.Zero, null);
			nameCol.PackStart (tr, false);
			nameCol.AddAttribute (tr, "text", 1);
			assemblyView.AppendColumn (nameCol);

			if (args.Length == 1) {
				LoadAssembly (args [0]);
			} else {
				window.Title = title;
				appbar.SetStatus ("No assembly loaded.");
			}
			
			SetupRecentAssembliesMenu (null, null);
			Settings.RecentassembliesChanged += new GConf.NotifyEventHandler (SetupRecentAssembliesMenu);

			window.Resize (Settings.Width, Settings.Height);
			hpaned.Position = Settings.Hpaned;
			window.ShowAll ();
		}

		void LoadAssembly (string name)
		{
			window.Title = String.Format ("{0} - [{1}]", title, name);
			appbar.SetStatus ("Loading " + name + "...");
			frameProgress.Fraction = 0.0;
			frameProgress.Text = "";
			frameLabel.Text = "";

			errorIters = null;
			if (notRunStore != null)
				notRunStore.Clear ();

			if (failuresStore != null)
				failuresStore.Clear ();

			if (store != null) {
				store.Clear ();
				store.Dispose ();
			}

			stdoutTV.Buffer.Clear ();
			stderrTV.Buffer.Clear ();
			foreach (Label l in nbLabels)
				SetColorLabel (l, false);

			AddRecent (name);
			store = new AssemblyStore (name);
			store.FixtureLoadError += new FixtureLoadErrorHandler (OnFixtureLoadError);
			store.FixtureAdded += new FixtureAddedEventHandler (OnFixtureAdded);
			assemblyView.Model = store;
			store.Load ();
		}

		void AddRecent (string name)
		{
			string [] recent = Settings.Recentassemblies.Split (':');
			ArrayList list = new ArrayList (recent);
			list.Remove ("");

			int i;
			if ((i = list.IndexOf (name)) != -1) {
				if (list.Count == 1)
					return;

				list.RemoveAt (i);
				list.Insert (0, name);
			} else {
				list.Add (name);
			}

			while (list.Count > 10)
				list.RemoveAt (0);

			recent = (string []) list.ToArray (typeof (string));
			Settings.Recentassemblies = String.Join (":", recent);
		}

		void RemoveRecent (string name)
		{
			string [] recent = Settings.Recentassemblies.Split (':');
			ArrayList list = new ArrayList (recent);
			list.Remove ("");
			list.Remove (name);

			recent = (string []) list.ToArray (typeof (string));
			Settings.Recentassemblies = String.Join (":", recent);
		}

		// assemblyView events
		//	Used for the 3 treeviews
		void OnRowActivated (object sender, RowActivatedArgs args)
		{
			TreeView tv = (TreeView) sender;
			TreePath path = args.Path;
			if (tv.RowExpand (path))
				tv.CollapseRow (path);
			else
				tv.ExpandRow (path, true);
		}

		// AssemblyStore events
		void OnFixtureAdded (object sender, FixtureAddedEventArgs args)
		{
			if (args.Current == args.Total) {
				appbar.Progress.Fraction = 0.0;
				appbar.SetStatus (args.Total + " tests loaded.");
				SetOriginalWriters ();
			} else {
				string msg = String.Format ("Loading test {0} of {1}", args.Current, args.Total);
				appbar.Progress.Fraction = args.Current / (double) args.Total;
				appbar.SetStatus (msg);
			}
		}

		void OnFixtureLoadError (object sender, FixtureLoadErrorEventArgs args)
		{
			store.Clear ();
			store = null;
			RemoveRecent (args.FileName);
			appbar.SetStatus ("Error loading assembly");
			Error ("Error loading '" + args.FileName + "'", args.Message);
			appbar.SetStatus ("");
		}

		// Window event handlers
		void OnWindowDelete (object sender, EventArgs args)
		{
			OnQuitActivate (sender, args);
		}

		// Menu and toolbar event handlers
		void OnQuitActivate (object sender, EventArgs args)
		{
			Settings.Width = window.Allocation.width;
			Settings.Height = window.Allocation.height;
			Settings.Hpaned = hpaned.Position;
			Quit ();
		}

		void OnExitActivate (object sender, EventArgs args)
		{
			OnQuitActivate (sender, args);
		}

		void OnCopyActivate (object sender, EventArgs args)
		{
			Console.WriteLine ("OnCopy");
		}

		void OnOpenActivate (object sender, EventArgs args)
		{
			FileDialog fd = new FileDialog ();
			fd.Run ();
			if (fd.Ok)
				LoadAssembly (fd.Filename);
		}

		void OnAboutActivate (object sender, EventArgs args)
		{
			Pixbuf logo = new Pixbuf (null, "nunit-gui.png");
			string [] authors = new string[] { "Gonzalo Paniagua Javier (gonzalo@ximian.com)" };

			string [] documentors = new string[] {};

			About about = new About (title, version, copyright,
						 description, authors, documentors, "", logo);
			about.Show ();
		}

		void OnPreferencesActivate (object sender, EventArgs args)
		{
			Console.WriteLine ("OnPreferencesActivate");
		}

		void OnRunActivate (object sender, EventArgs args)
		{
			if (assemblyView.Model == null)
				return;

			TreeSelection selection = assemblyView.Selection;
			TreeModel model;
			TreeIter iter = new TreeIter ();

			if (!selection.GetSelected (out model, ref iter)) {
				appbar.SetStatus ("You have to select a test to run.");
				return;
			}
			
			if (errorIters != null)
				errorIters.Clear ();

			if (notRunStore != null)
				notRunStore.Clear ();

			if (failuresStore != null)
				failuresStore.Clear ();

			stdoutTV.Buffer.Clear ();
			stderrTV.Buffer.Clear ();

			ntests = -1;
			finishedTests = 0;
			frameProgress.Fraction = 0.0;

			appbar.SetStatus ("Running tests...");
			SetStringWriters ();
			store.RunTestAtIter (iter, this, ref ntests);
		}

		void LoadRecent (object sender, EventArgs args)
		{
			MenuItem item = (MenuItem) sender;
			string assembly = (string) item.GetData ("assemblyName");
			LoadAssembly (assembly);
		}

		void OnClearRecent (object sender, EventArgs args)
		{
			Settings.Recentassemblies = "";
		}

		// Notebook
		void OnSwitchPage (object sender, GtkSharp.SwitchPageArgs args)
		{
			Notebook nb = (Notebook) sender;
			if (nb.Page != args.PageNum) {
				SetColorLabel (nbLabels [nb.Page], false);
			}

			SetColorLabel (nbLabels [args.PageNum], false);
		}

		// Interface NUnit.Core.EventListener
		void EventListener.TestStarted (TestCase testCase)
		{
			frameLabel.Text = "Test: " + testCase.FullName;
		}
			
		void EventListener.TestFinished (TestCaseResult result)
		{
			if (ntests == -1)
				return;

			frameProgress.Fraction = ++finishedTests / (double) ntests;
			frameProgress.Text = String.Format ("{0}/{1}", finishedTests, ntests);
			if (finishedTests == ntests)
				appbar.SetStatus ("");

			if (result.Executed == false) {
				AddIgnored (result.Test.FullName, result.Test.IgnoreReason);
			} else if (result.IsFailure) {
				AddError (result);
			}

			CheckWriters ();
		}

		void EventListener.SuiteStarted (TestSuite suite)
		{
			frameLabel.Text = "Suite: " + suite.FullName;
		}

		void EventListener.SuiteFinished (TestSuiteResult result)
		{
		}

		// Misc.
		void AddIgnored (string name, string reason)
		{
			if (notRunStore == null) {
				notRunStore = new TreeStore ((uint) TypeFundamentals.TypeString);
				CellRendererText tr = new CellRendererText ();
				TreeViewColumn col = new TreeViewColumn ();
				col.PackStart (tr, false);
				col.AddAttribute (tr, "text", 0);
				notRun.AppendColumn (col);
				notRun.Model = notRunStore;
				notRun.ShowAll ();
			}

			TreeIter iter;
			notRunStore.Append (out iter);
			notRunStore.SetValue (iter, 0, new Value (name));
			notRunStore.Append (out iter, iter);
			notRunStore.SetValue (iter, 0, new Value (reason));
			SetColorLabel (notRunLabel, true);
		}

		void AddError (TestCaseResult result)
		{
			if (failuresStore == null) {
				failuresStore = new TreeStore ((uint) TypeFundamentals.TypeString);
				CellRendererText tr = new CellRendererText ();
				TreeViewColumn col = new TreeViewColumn ();
				col.PackStart (tr, false);
				col.AddAttribute (tr, "text", 0);
				failures.AppendColumn (col);
				failures.Model = failuresStore;
				failures.ShowAll ();
			}

			if (errorIters == null)
				errorIters = new Hashtable ();

			int dot;
			TreeIter main = TreeIter.Zero;
			TreeIter iter;
			string fullname = result.Test.FullName;
			if ((dot = fullname.LastIndexOf ('.')) != -1) {
				string key = fullname.Substring (0, dot);
				if (!errorIters.ContainsKey (key)) {
					failuresStore.Append (out main);
					errorIters [key] = main;
				} else {
					main = (TreeIter) errorIters [key];
				}
				failuresStore.SetValue (main, 0, new Value (key));
			} else {
				failuresStore.Append (out main);
				errorIters [fullname] = main;
				failuresStore.SetValue (main, 0, new Value (fullname));
			}

			failuresStore.Append (out iter, main);
			failuresStore.SetValue (iter, 0, new Value (result.Test.Name));
			main = iter;
			failuresStore.Append (out iter, main);
			failuresStore.SetValue (iter, 0, new Value (result.Message));
			failuresStore.Append (out iter, main);
			failuresStore.SetValue (iter, 0, new Value (result.StackTrace));
			SetColorLabel (failuresLabel, true);
		}

		void SetOriginalWriters ()
		{
			Console.SetOut (origStdout);
			Console.SetError (origStderr);
		}

		void SetStringWriters ()
		{
			Console.SetOut (stdout);
			Console.SetError (stderr);
		}

		void CheckWriters ()
		{
			StringBuilder sb = stdout.GetStringBuilder ();
			if (sb.Length != 0) {
				InsertOutText (stdoutTV, sb.ToString ());
				sb.Length = 0;
				SetColorLabel (stdoutLabel, true);
			}

			sb = stderr.GetStringBuilder ();
			if (sb.Length != 0) {
				stderrTV.Buffer.InsertAtCursor (sb.ToString ());
				sb.Length = 0;
				SetColorLabel (stderrLabel, true);
			}
		}

		void InsertOutText (TextView tv, string str)
		{
			TextBuffer buf = tv.Buffer;
			buf.InsertAtCursor (str);
		}

		void SetColorLabel (Label label, bool color)
		{
			string text = label.Text;
			if (color)
				label.Markup = String.Format ("<span foreground=\"blue\">{0}</span>", text);
			else
				label.Markup = text;
		}
		
		void Error (string title, string text)
		{
			ErrorDialog ed = new ErrorDialog (title, text);
			ed.Run ();
		}

		void SetupRecentAssembliesMenu (object sender, GConf.NotifyEventArgs args)
		{
			string [] recent = Settings.Recentassemblies.Split (':');
			if (recent.Length == 0) {
				menuRecent.Submenu = null;
				return;
			}

			EventHandler cb = new EventHandler (LoadRecent);
			Menu menu = new Menu ();
			int index = 1;
			foreach (string s in recent) {
				if (s == "")
					continue;
				MenuItem item = new MenuItem (String.Format ("_{0}. {1}",
									     index++,
									     s.Replace ("_", "__")));
				item.SetData ("assemblyName", s);
				item.Activated += cb;
				menu.Append (item);
			}
			menu.ShowAll ();
			menuRecent.Submenu = menu;
		}

		// Main
		static void Main (string [] args)
		{
		 	LogFunc logFunc = new LogFunc (Log.PrintTraceLogFunction);
		 	Log.SetLogHandler ("GLib-GObject", LogLevelFlags.All, logFunc);
		 	Log.SetLogHandler ("Gtk", LogLevelFlags.All, logFunc);
			new NUnitGUI (args).Run ();
		}
	}
}

