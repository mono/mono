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

namespace Monodoc {
using System;
using System.IO;
using System.Text;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml;
using System.Collections;
using Monodoc.Lucene.Net.Index;
using Monodoc.Lucene.Net.Documents;

public class EcmaSpecProvider : Provider {
	string basedir;
	
	public EcmaSpecProvider (string base_directory)
	{
		basedir = base_directory;
		if (!Directory.Exists (basedir))
			throw new FileNotFoundException (String.Format ("The directory `{0}' does not exist", basedir));
	}
	
	public override void PopulateTree (Tree tree)
	{
		XPathNavigator n = new XPathDocument (Path.Combine (basedir, "toc.xml")).CreateNavigator ();
		n.MoveToRoot ();
		n.MoveToFirstChild ();
		PopulateNode (n.SelectChildren ("node", ""), tree);
		
	}
	
	void PopulateNode (XPathNodeIterator nodes, Node treeNode)
	{
		while (nodes.MoveNext ()) {
			XPathNavigator n = nodes.Current;
			string secNumber = n.GetAttribute ("number", ""),
				secName = n.GetAttribute ("name", "");
			
			Console.WriteLine ("\tSection: " + secNumber);
			treeNode.tree.HelpSource.PackFile (Path.Combine (basedir, secNumber + ".xml"), secNumber);
			Node thisNode = treeNode.LookupNode (secNumber + ": " + secName, "ecmaspec:" + secNumber);
			
			if (n.HasChildren)
				PopulateNode (n.SelectChildren ("node", ""), thisNode);
		}
	}

	public override void CloseTree (HelpSource hs, Tree tree)
	{
	}
}

public class EcmaSpecHelpSource : HelpSource {
	public EcmaSpecHelpSource (string base_file, bool create) : base (base_file, create)
	{
	}

	public override string GetText (string url, out Node match_node)
	{
		string ret = null;
		
		match_node = null;
		if (url.StartsWith ("ecmaspec:")) {
			match_node = MatchNode (Tree, url);
			ret = GetTextFromUrl (url);
		}
		
		if (url == "root:") {
			if (use_css)
				ret = "<div id=\"ecmaspec\" class=\"header\"><div class=\"title\">C# Language Specification</div></div>";
			else
			ret = "<table width=\"100%\" bgcolor=\"#b0c4de\" cellpadding=\"5\"><tr><td><h3>C# Language Specification</h3></tr></td></table>";

			match_node = Tree;
		}
		
		if (ret != null && match_node != null && match_node.Nodes != null && match_node.Nodes.Count > 0) {
			ret += "<p>In This Section:</p><ul>\n";
			foreach (Node child in match_node.Nodes) {
				ret += "<li><a href=\"" + child.URL + "\">" + child.Caption + "</a></li>\n";
			}
			ret += "</ul>\n";
		}
		if (ret != null)
			return BuildHtml (css_ecmaspec_code, ret); 
		else
			return null;
	}
	
	private Node MatchNode (Node node, string matchurl)
	{	
		foreach (Node n in node.Nodes) {
			if (matchurl == n.Element)
				return n;
			else if (matchurl.StartsWith (n.Element + ".") && !n.IsLeaf)
				return MatchNode (n, matchurl);
		}
		
		return null;
	}

	string GetTextFromUrl (string url)
	{
		Stream file_stream = GetHelpStream (url.Substring (9));
		if (file_stream == null)
			return null;
		
		return Htmlize (new XPathDocument (file_stream));
	}
	
	
	static string css_ecmaspec;
	public static string css_ecmaspec_code {
		get {
			if (css_ecmaspec != null)
				return css_ecmaspec;
			if (use_css) {
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
				Stream str_css = assembly.GetManifestResourceStream ("ecmaspec.css");
				css_ecmaspec = (new StreamReader (str_css)).ReadToEnd();
			} else {
				css_ecmaspec = String.Empty;
			}
			return css_ecmaspec;
		}
	}

	class ExtObj {
		public string Colorize (string code, string lang) {
			return (Mono.Utilities.Colorizer.Colorize(code,lang));
		}
	}
	static XslTransform ecma_transform;
	static XsltArgumentList args = new XsltArgumentList();
	static string Htmlize (XPathDocument ecma_xml)
	{
		if (ecma_transform == null){
			ecma_transform = new XslTransform ();
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
			Stream stream;
			if (use_css) 
				stream = assembly.GetManifestResourceStream ("ecmaspec-html-css.xsl");
			else 
				stream = assembly.GetManifestResourceStream ("ecmaspec-html.xsl");

			XmlReader xml_reader = new XmlTextReader (stream);
			ecma_transform.Load (xml_reader);
			args.AddExtensionObject ("monodoc:///extensions", new ExtObj()); 
		}
		
		if (ecma_xml == null) return "";

		StringWriter output = new StringWriter ();
		ecma_transform.Transform (ecma_xml, args, output);
		
		return output.ToString ();
	}

	public override void PopulateSearchableIndex (IndexWriter writer) 
	{
		foreach (Node n in Tree.Nodes)
			AddDocuments (writer, n);
	}
	void AddDocuments (IndexWriter writer, Node node) 
	{
		string url = node.URL;
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

		//Obatin the examples
		StringBuilder s2 = new StringBuilder ();
		GetExamples (nelem, s2);
		string examples = s2.ToString ();

		//Write to the Lucene Index all the parts
		SearchableDocument doc = new SearchableDocument ();
		doc.title = title;
		doc.hottext = title.Substring (title.IndexOf (':')); 
		doc.url = url;
		doc.text = text;
		doc.examples = examples;
		writer.AddDocument (doc.LuceneDoc);
		
		if (node.IsLeaf)
			return;

		foreach (Node n in node.Nodes)
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
