//
// SwfTypeDisplayer.cs: 
//   Display types using System.Windows.Forms
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Jonathan Pryor
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
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mono.TypeReflector.Displayers.Swf
{
	class ReflectorTreeNode : TreeNode
	{
		private Node node = null;

		public ReflectorTreeNode (string text)
		{
			base.Text = text;
		}

		public ReflectorTreeNode (Node node)
		{
			this.node = node;
			base.Text = node.Description;
		}
		
		public Node Node {
			get {return node;}
		}
	}

	public class SwfTypeDisplayer : TypeDisplayer
	{
		private static int windows = 0;

		private const string dummyText = "dummy: you shouldn't see this!";

		private SwfWindow window = new SwfWindow ();

		public override int MaxDepth {
			set {/* ignore */}
		}

		public override bool RequireTypes {
			get {return false;}
		}

		public SwfTypeDisplayer ()
		{
			++windows;

			window.FileOpenClick += new EventHandler (OnFileOpen);
			window.FileQuitClick += new EventHandler (OnFileQuit);

			window.EditCopyClick += new EventHandler (OnEditCopy);

			window.ViewFormatterDefaultClick += new EventHandler (OnViewFormatterDefault);
			window.ViewFormatterVBClick += new EventHandler (OnViewFormatterVB);
			window.ViewFormatterCSharpClick += new EventHandler (OnViewFormatterCSharp);
			window.ViewFinderExplicitClick += new EventHandler (OnViewFinderExplicit);
			window.ViewFinderReflectionClick += new EventHandler (OnViewFinderReflection);

			window.HelpAboutClick += new EventHandler (OnHelpAbout);

			window.TreeView.BeforeExpand += new TreeViewCancelEventHandler (OnNodeExpand);
		}

		public override void Run ()
		{
			ShowTypes ();

			Application.Run (window);
		}

		public override void ShowError (string message)
		{
			MessageBox.Show (message, "Type Reflector", MessageBoxButtons.OK, 
					MessageBoxIcon.Error);
		}

		private void ShowTypes ()
		{
			foreach (Assembly a in Assemblies) {
				ReflectorTreeNode tn = new ReflectorTreeNode (string.Format ("Assembly: {0}", a.FullName));

				foreach (string ns in Namespaces (a)) {
					ReflectorTreeNode nn = new ReflectorTreeNode (string.Format ("Namespace: {0}", ns));
					tn.Nodes.Add (nn);

					foreach (Type type in Types (a, ns))
						AddType (type, nn);
				}

				window.TreeView.Nodes.Add (tn);
			}

			window.Show ();
		}
		
		private void AddType (Type type, ReflectorTreeNode parent)
		{
			ReflectorTreeNode tn = CreateTreeNode (type);
			tn.Nodes.Add (new ReflectorTreeNode (dummyText));
			parent.Nodes.Add (tn);
		}

		private ReflectorTreeNode CreateTreeNode (Type type)
		{
			Node root = new Node (Formatter, Finder);
			root.NodeInfo = new NodeInfo (null, type);
			return new ReflectorTreeNode (root);
		}

		// System.Windows.Forms Functions...
		private void OnFileQuit (object o, EventArgs args) 
		{
			Console.WriteLine ("Asked to quit app; windows=" + windows);
			window.Hide ();
			window.Dispose ();
			if (--windows == 0) {
				Console.WriteLine ("App.Exit");
				Application.Exit ();
			}
		}

		private void OnFileOpen (object o, EventArgs args)
		{
			OpenFileDialog ofd = new OpenFileDialog ();
			ofd.CheckFileExists = true;
			ofd.Multiselect = true;
			ofd.Title = "Open Assembly";
			ofd.ValidateNames = true;
			ofd.Filter = 
				"Assemblies (*.dll;*.exe)|*.dll;*.exe" +
				"|Dynamic Link Libraries (*.dll)|*.dll" +
				"|Executables (*.exe)|*.exe" +
				"|All Files (*.*)|*.*";

			if (ofd.ShowDialog() == DialogResult.OK) {
				OpenAssemblies (ofd.FileNames);
			}
		}

		private void OpenAssemblies (string[] assemblies)
		{
			SwfTypeDisplayer d = null;
			if (base.Assemblies.Count == 0)
				d = this;
			else {
				d = new SwfTypeDisplayer ();
				d.Finder = Finder;
				d.Formatter = Formatter;
				d.Options = Options;
			}

			TypeLoader tl = TypeReflectorApp.CreateLoader (Options);
			tl.Assemblies = assemblies;

			try {
				TypeReflectorApp.FindTypes (d, tl, new string[]{"."});
				d.ShowTypes ();
			}
			catch (Exception e) {
				ShowError (string.Format ("Unable to load Assembly '{0}': {1}",
							assemblies, e.ToString()));
			}
		}

		public void OnEditCopy (object o, EventArgs args)
		{
			/* ignore */
		}

		public void OnViewFormatterDefault (object o, EventArgs args)
		{
		}

		public void OnViewFormatterVB (object o, EventArgs args)
		{
		}

		public void OnViewFormatterCSharp (object o, EventArgs args)
		{
		}

		public void OnViewFinderReflection (object o, EventArgs args)
		{
		}

		public void OnViewFinderExplicit (object o, EventArgs args)
		{
		}

		public void OnHelpAbout (object o, EventArgs args)
		{
			/* ignore */
			MessageBox.Show (
					"Type Reflector, version x.y.  Copyright (C) 2002-2003 Jonathan Pryor",
					"About Type Reflector",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			TypeReflectorApp.PrintVersion ();
		}

		private void OnNodeExpand (object sender, TreeViewCancelEventArgs e)
		{
			/*
			Console.WriteLine ("(node-expanded (Action {0}) (Cancel {1}) (Node {2}))",
					e.Action, e.Cancel, e.Node);
			 */

			if ((e.Node.Nodes.Count > 0) && (e.Node.Nodes[0].Text == dummyText)) {
				ReflectorTreeNode tn = (ReflectorTreeNode) e.Node;
				tn.Nodes.Clear ();

				foreach (Node child in tn.Node.GetChildren()) {
					ReflectorTreeNode cn = new ReflectorTreeNode (child);
					cn.Nodes.Add (new ReflectorTreeNode (dummyText));
					tn.Nodes.Add (cn);
				}

				if (tn.Nodes.Count == 0)
					e.Cancel = true;
			}
		}
	}
}

