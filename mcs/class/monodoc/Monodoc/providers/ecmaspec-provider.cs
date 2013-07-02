//
// The ecmaspec provider is for ECMA specifications
//
// Authors:
//	John Luke (jluke@cfl.rr.com)
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// Use like this:
//   mono assembler.exe --ecmaspec DIRECTORY --out name
//

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml;
using System.Collections.Generic;
using Lucene.Net.Index;
using Lucene.Net.Documents;

namespace Monodoc.Providers
{
	public class EcmaSpecProvider : Provider
	{
		string basedir;
	
		public EcmaSpecProvider (string base_directory)
		{
			basedir = base_directory;
			if (!Directory.Exists (basedir))
				throw new DirectoryNotFoundException (String.Format ("The directory `{0}' does not exist", basedir));
		}
	
		public override void PopulateTree (Tree tree)
		{
			XPathNavigator n = new XPathDocument (Path.Combine (basedir, "toc.xml")).CreateNavigator ();
			n.MoveToRoot ();
			n.MoveToFirstChild ();
			PopulateNode (n.SelectChildren ("node", ""), tree.RootNode);
		}
	
		void PopulateNode (XPathNodeIterator nodes, Node treeNode)
		{
			foreach (XPathNavigator n in nodes) {
				string secNumber = n.GetAttribute ("number", "");
				string secName = n.GetAttribute ("name", "");

				var storage = treeNode.Tree.HelpSource.Storage;
				using (var file = File.OpenRead (Path.Combine (basedir, secNumber + ".xml")))
					storage.Store (secNumber, file);

				Node thisNode = treeNode.GetOrCreateNode (secNumber + ": " + secName, "ecmaspec:" + secNumber);
			
				if (n.HasChildren)
					PopulateNode (n.SelectChildren ("node", ""), thisNode);
			}
		}

		public override void CloseTree (HelpSource hs, Tree tree)
		{
		}
	}

	public class EcmaSpecHelpSource : HelpSource
	{
		const string EcmaspecPrefix = "ecmaspec:";
		const string TocPart = "%toc"; // What is returned as TocXml
		const string SpecPart = "%spec"; // What is returned as Ecmaspec

		public EcmaSpecHelpSource (string base_file, bool create) : base (base_file, create)
		{
		}

		public override DocumentType GetDocumentTypeForId (string id)
		{
			return id.EndsWith (TocPart) ? DocumentType.TocXml : DocumentType.EcmaSpecXml;
		}

		public override bool IsGeneratedContent (string id)
		{
			return id == "root:" || id.EndsWith (TocPart);
		}

		public override bool IsMultiPart (string id, out IEnumerable<string> parts)
		{
			if (id == "root:" || id.EndsWith (TocPart) || id.EndsWith (SpecPart)) {
				parts = null;
				return false;
			}
			parts = MakeMultiPart (id);
			return true;
		}

		IEnumerable<string> MakeMultiPart (string baseId)
		{
			yield return baseId + SpecPart;
			yield return baseId + TocPart;
		}

		public override string GetText (string id)
		{
			Node n = id == "root:" ? Tree.RootNode : MatchNode (EcmaspecPrefix + id.Substring (0, id.Length - TocPart.Length));
			if (n == null)
				throw new ArgumentException ("id", string.Format ("{0} -> {1}", id, EcmaspecPrefix + id.Substring (0, id.Length - TocPart.Length)));
			return TreeDumper.ExportToTocXml (n, "C# Language Specification", "In this section:");
		}

		public override Stream GetHelpStream (string id)
		{
			return id.EndsWith (SpecPart) ? base.GetHelpStream (id.Substring (0, id.IndexOf (SpecPart))) : base.GetHelpStream (id);
		}
	
		public override void PopulateSearchableIndex (IndexWriter writer) 
		{
			foreach (Node n in Tree.RootNode.ChildNodes)
				AddDocuments (writer, n);
		}

		protected override string UriPrefix {
			get {
				return EcmaspecPrefix;
			}
		}

		void AddDocuments (IndexWriter writer, Node node) 
		{
			string url = node.PublicUrl;
			Stream file_stream = GetHelpStream (url.Substring (9));
			if (file_stream == null) //Error
				return;
			XmlDocument xdoc = new XmlDocument ();
			xdoc.Load (new XmlTextReader (file_stream));

			//Obtain the title
			XmlNode nelem = xdoc.DocumentElement;
			string title = nelem.Attributes["number"].Value + ": " + nelem.Attributes["title"].Value;

			//Obtain the text
			StringBuilder s = new StringBuilder ();
			GetTextNode (nelem, s);
			string text = s.ToString ();

			//Obtain the examples
			StringBuilder s2 = new StringBuilder ();
			GetExamples (nelem, s2);
			string examples = s2.ToString ();

			//Write to the Lucene Index all the parts
			SearchableDocument doc = new SearchableDocument ();
			doc.Title = title;
			doc.HotText = title.Substring (title.IndexOf (':')); 
			doc.Url = url;
			doc.Text = text;
			doc.Examples = examples;
			writer.AddDocument (doc.LuceneDoc);
		
			if (node.IsLeaf)
				return;

			foreach (Node n in node.ChildNodes)
				AddDocuments (writer, n);
		}

		void GetTextNode (XmlNode n, StringBuilder s) 
		{
			//dont include c# code
			if (n.Name == "code_example")
				return;
			//include all text from nodes
			if (n.NodeType == XmlNodeType.Text)
				s.Append (n.Value);
		
			//recursively explore all nodes
			if (n.HasChildNodes)
				foreach (XmlNode n_child in n.ChildNodes)
					GetTextNode (n_child, s);
		}

		void GetExamples (XmlNode n, StringBuilder s)
		{
			if (n.Name == "code_example") {
				if (n.FirstChild.Name == "#cdata-section")
					s.Append (n.FirstChild.Value);
			} else {
				if (n.HasChildNodes)
					foreach (XmlNode n_child in n.ChildNodes)
						GetExamples (n_child, s);
			}
		}
	}
}
