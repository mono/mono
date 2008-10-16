//
// A provider that uses Windows help file xhtml TOC files and looks for the
// referenced documents to create the help source. 
//
// Authors:
// Copyright 2003 Lee Mallabone <gnome@fonicmonkey.net>
//   Johannes Roith <johannes@roith.de>
//   Miguel de Icaza <miguel@ximian.com>
//
// Known problems:
//   * Should update the "out Node" when getting data.
//   * Should replace the img src links before packing the file.

namespace Monodoc { 
using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

//
// The simple provider generates the information source
//
public class XhtmlProvider : Provider {
	string tocFile;
	
	public XhtmlProvider (string handbookTocFile)
	{
		tocFile = handbookTocFile;
		if (!File.Exists (tocFile))
			throw new FileNotFoundException (String.Format ("The table of contents, `{0}' does not exist", tocFile));
		
	}

	public override void PopulateTree (Tree tree)
	{
		new SimpleHandbookTOCParser(tree, tocFile);
	}


	public override void CloseTree (HelpSource hs, Tree tree)
	{
	}
}

//
// The HelpSource is used during the rendering phase.
//

public class XhtmlHelpSource : HelpSource {
	
	public XhtmlHelpSource (string base_file, bool create) : base (base_file, create) {}
	private const string XHTML_PREFIX = "xhtml:";
	
	public override string GetText (string url, out Node match_node)
	{
		match_node = null;
		
		if (url == "root:") {
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<table width=\"100%\" bgcolor=\"#b0c4de\" cellpadding=\"5\"><tr><td><h3>Mono Handbook</h3></tr></td></table>");
			foreach (Node n in Tree.Nodes) {
				if (n.IsLeaf) { 
					sb.AppendFormat ("<a href='{0}'>{1}</a><br/>", 
						n.Element.Replace ("source-id:NNN", "source-id:" + SourceID), 
						n.Caption);
				} else {
					sb.AppendFormat ("<h2>{0}</h2>", n.Caption);
					foreach (Node subNode in n.Nodes) {
						sb.AppendFormat ("<a href='{0}'>{1}</a><br/>", 
							subNode.Element.Replace ("source-id:NNN", "source-id:" + SourceID), 
							subNode.Caption);
					}
				}
			}
			
			return sb.ToString ();
		}
		
		if (url.IndexOf (XHTML_PREFIX) > -1)
			return GetTextFromUrl (url);

		return null;
	}
	
	public virtual XmlDocument ProcessContent (XmlDocument docToProcess)
	{
		return docToProcess;
	}

	public static string GetAbsoluteLink(string target, string url)
	{
			
		string value = null;
		
		if (target.StartsWith ("#") ||
		    target.StartsWith ("T:") ||
		    target.StartsWith ("M:") ||
		    target.StartsWith ("P:") ||
		    target.StartsWith ("T:") ||
		    target.StartsWith ("E:") ||
		    target.StartsWith ("F:") ||
		    target.StartsWith ("O:") ||
		    target.StartsWith ("N:") ||
		    target.StartsWith ("api:"))
			return null;
		
		int endp = target.IndexOf(':');
		
		if (endp == -1)
			endp = 0;
		string protocol = target.Substring(0, endp);
		switch (protocol) {
		case "mailto": 
		case "http":
		case "https":
		case "ftp":
		case "news":
		case "irc":
			break;
		default:
			// handle absolute urls like: /html/en/images/empty.png
			if (!target.StartsWith("/")) {
				
				// url is something like "gnome/bindings/mono.html"
				// This will get the path "gnome/bindings"
				
				int slash = url.LastIndexOf ("/");
				string tmpurl = url;
				
				if (slash != -1)
					tmpurl  = url.Substring(0, slash);
				
				// Count "../" in target and go one level down
				// for each in tmpurl, eventually, then remove "../".
				
				Regex reg1 = new Regex("../");
				MatchCollection matches = reg1.Matches(target);
				
				for(int i = 1; i < matches.Count; i++) {
					slash = tmpurl.LastIndexOf ("/");
					if (slash != -1) 
						tmpurl  = tmpurl.Substring(0, slash);
				}
				
				target = target.Replace("../", "");
				
				value = tmpurl + "/" + target;
				
			} else {
				value = target.Substring(1, target.Length - 1);
			}
			break;
		}
		return value;
	}
	
	private XmlDocument RewriteLinks(XmlDocument docToProcess, string url)
	{
                XmlNodeList nodeList = docToProcess.GetElementsByTagName("a");
		
	        foreach(XmlNode node in nodeList) {
			
			XmlElement element = (XmlElement) node;
			
			if (element.HasAttribute("href") ){
				
				XmlAttribute href = element.GetAttributeNode("href");
				string target = href.Value;
				
				target = GetAbsoluteLink(target, url);
				if (target != null) {
					string newtarget = String.Format ("source-id:{0}:xhtml:{1}", SourceID, target);
					href.Value = newtarget;
				}
			}
                }

                nodeList = docToProcess.GetElementsByTagName("img");

                foreach(XmlNode node in nodeList) {
                                                                                                                                    
                        XmlElement element = (XmlElement) node;
                                                                                                                                    
                        if (element.HasAttribute("src") ){
                                                                                                                                    
                                XmlAttribute href = element.GetAttributeNode("src");
                                string target = href.Value;
                                                                                                                                    
                                target = GetAbsoluteLink(target, url);
                                if (target != null) {
                                        string newtarget = String.Format ("source-id:{0}:xhtml:{1}", SourceID, target);
                                        href.Value = newtarget;
                                }
                        }		
		}

		return docToProcess;
	}

		public override  Stream GetImage(string url)  {

		// Remove "xhtml:" prefix including any help-source id on the front.
		int prefixStart = url.IndexOf(XHTML_PREFIX);
		if (prefixStart > -1)
			url = url.Substring (prefixStart + 6);

		// Otherwise the last element of the url is the file code we got.
		int pound = url.LastIndexOf ("#");
		string code;
		if (pound == -1)
			code = url;
		else
			code = url.Substring (pound+1);

		if (code == null)
		{
			Console.WriteLine("Warning, NULL url!");
		}
	
		Stream s = GetHelpStream (code);

		return s;
	}

	XmlDocument ShowComments(XmlDocument docToProcess, string code) {

                                                                                                                                               
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(docToProcess.NameTable);
                nsmgr.AddNamespace("default", "http://www.w3.org/1999/xhtml");
                nsmgr.AddNamespace("monodoc", "http://www.go-mono.org/xml/monodoc");
                nsmgr.PushScope();
                                                                                                                                               
                XmlElement root = docToProcess.DocumentElement;
                XmlNode body = root.SelectSingleNode("/default:html/default:body", nsmgr);

		string html   = "<br /><hr /><br />";

		CommentService commentservice = new CommentService();
		Comment[] comments = commentservice.GetCommentsByUrl("monohb@" + code);

		if (comments == null) {
			html += "No comments available.";
		}
		else {

		foreach(Comment comment in comments) {

			if (comment.Title == "")
				comment.Title = "[No Title]";

			html += "<table width=\"100%\" cellpadding=\"4\" cellspacing=\"2\"  bgcolor=\"#efefef\">\n";
			html += "<tr><td valign=\"top\" bgcolor=\"#c0c0c0\" class=\"commenthead\" colspan=\"2\"><b>"
					 + comment.Title + "</b> </td></tr>\n";
			html += "<tr><td width=\"100\" valign=\"top\" class=\"commentcontent\"><b>Author:</b>"
					 + "</td><td valign=\"top\" class=\"commentcontent\">" + comment.Author + "</td></tr>\n";
			html += "<tr><td width=\"100\" valign=\"top\" class=\"commentcontent\"><b>Mail:</b>"
					+ "</td><td valign=\"top\" class=\"commentcontent\">" + comment.Mail + "</td></tr>\n";
			html += "<tr><td width=\"100\" valign=\"top\" class=\"commentcontent\"><b>Date:</b>"
					+ "</td><td valign=\"top\" class=\"commentcontent\">" + comment.Date + "</td></tr>\n";
			html += "<tr><td width=\"100\" valign=\"top\" class=\"commentcontent\"><b>Comment:</b>"
					+ "</td><td valign=\"top\" class=\"commentcontent\">" + comment.Text + "</td></tr>\n";
			html += "</table>\n\n<br /><br />\n"	;

		}
		}
		Console.WriteLine("monohb@" + code);
		body.InnerXml += html;
		return docToProcess;
	}

	string GetTextFromUrl (string url)
	{
		// Remove "xhtml:" prefix including any help-source id on the front.
		int prefixStart = url.IndexOf(XHTML_PREFIX);
		if (prefixStart > -1)
			url = url.Substring (prefixStart + 6);

		// Otherwise the last element of the url is the file code we got.
		int pound = url.LastIndexOf ("#");
		string code;
		if (pound == -1)
			code = url;
		else
			code = url.Substring (pound+1);

		if (code == null)
		{
			Console.WriteLine("Warning, NULL url!");
			return "<html>url was null</html>";
		}

		Stream s = GetHelpStream (code);
		if (s == null)
			return String.Format ("<html>No stream for this node: {0} with code ({1})</html>", url, code);

		//
		// Now, get the file type
		//
		//int slash = url.LastIndexOf ("/");
		string fname = url; //url.Substring (slash + 1, pound - slash - 1).ToLower ();

		if (s != null && (fname.EndsWith (".html") || fname.EndsWith (".htm") || fname.EndsWith(".xhtml")))
		{
			XmlDocument newdoc = new XmlDocument();
			try {
				newdoc.Load(s);
			} catch (XmlException e) {
				return "<html>XML Error when loading <b>" + url + "</b>:<br>" + e.Message
					+ "<br /><pre>" + e.ToString () + "</pre></html>";
			}
			
			XmlDocument processedDoc = ProcessContent(newdoc);

			if (SettingsHandler.Settings.ShowComments)
				processedDoc = ShowComments(processedDoc, code);
			XmlDocument docForMonodoc = RewriteLinks(processedDoc, url);
			return docForMonodoc.DocumentElement.InnerXml; // get rid of <body>
		}

		else if (s != null && (fname.EndsWith (".gif") || fname.EndsWith (".jpeg") || fname.EndsWith (".jpg")  || fname.EndsWith(".png")))
                {
			return "Images are not yet handled.";
		}
		else
		{
			return String.Format("<html>Unsupported file name: {0}</html>", fname);
		}
	}

	public override void PopulateIndex (IndexMaker index_maker)
	{
		PopulateIndexFromNodes (Tree);
	}

	void PopulateIndexFromNodes (Node start)
	{
		ArrayList nodes = start.Nodes;
		
		if (nodes == null)
			Console.WriteLine ("Leaf: " + start.Caption);
		else {
			Console.WriteLine ("Root: " + start.Caption);
			foreach (Node n in nodes)
				PopulateIndexFromNodes (n);
		}
	}
}




// Simple Parser for the Handbook TOC format
public class SimpleHandbookTOCParser
{

	public XmlDocument newdoc;
	public static Node nodeToAddChildrenTo;
//	Tree monodocTree;

	public static string spaces = "";

	System.Collections.ArrayList tempfiles = new System.Collections.ArrayList ();

  	public SimpleHandbookTOCParser(Tree monodocTree, string tocFile)
  	{
		XmlDocument doc = new XmlDocument();
		doc.Load(tocFile);

		XmlNodeList nodeList = doc.GetElementsByTagName("body");
		XmlNodeList bodylist = nodeList[0].ChildNodes[1].ChildNodes;
		//Node top = monodocTree.LookupNode ("Mono handbook root", "hb:");
		nodeToAddChildrenTo = monodocTree;
		ParseUl(bodylist[1].ChildNodes,monodocTree);

		foreach (string file in tempfiles)
			System.IO.File.Delete (file);
   	}

	//
	// For the given attribute in the nodes, packages all the files listed
	// this is used for pulling all files referenced by <a href="xxx"> or
	// <img src="xxx">
	//
	static Hashtable packed_files = new Hashtable ();
	
	public static void IncludeAttribLinks(XmlNodeList nodeList, string attrname, string filename)
	{
	   	foreach(XmlNode node in nodeList) {
			XmlAttribute attr = node.Attributes [attrname];
			if (attr == null) continue;
				
	        	Console.WriteLine(spaces + "   " + attr.Value);
	               	string linkfilename = attr.Value;
	                linkfilename = XhtmlHelpSource.GetAbsoluteLink(linkfilename, filename);
	                if (linkfilename != null) {
				if (File.Exists(linkfilename) && packed_files [linkfilename] == null){
					packed_files [linkfilename] = linkfilename;
					nodeToAddChildrenTo.tree.HelpSource.PackFile (linkfilename, linkfilename);
				} else
					Console.WriteLine (spaces + "Warning: file {0} not found", linkfilename);
					
			}
		}
	}

	public void ParseUl(XmlNodeList items, Node monoTreeNode)
	{
		Node latestNodeAddition = monoTreeNode;
		
		for (int i = 0;i < items.Count;i++){    
			if (items[i].LocalName == "li"){
				string[] attribs = ParseLi(items[i]);
				
				string filename = attribs[1];
				
				if (i+1 == items.Count || items[i+1].LocalName == "ul"){
					Console.WriteLine(spaces + "+" + attribs[0] + ": " + filename);
					// Put the node in the monodoc toc.
					
					// FIXME: Change this to include the help-source ID?
					// Not really sure what's going on here.....

					// An empty node with subnodes
					if (filename == "html/en/empty.html") {
						// emptysub.html indicates, that a subpage should be generated...
						// For later use.

					string exportstr = "<html><head><title>Monodoc</title></head><body><i>Currently Navigation is recommended through the treeview.</i><br />This chapter contains the following entries:<br /><br />";

						if (items.Count > i+1 && items[i+1].HasChildNodes) {
							foreach(XmlNode node in items[i+1].ChildNodes) {
								if (node.LocalName == "li") {
									string[] list = ParseLi(node);
									if (list[1] == "html/en/empty.html")
	                                                                       exportstr += list[0] + "<br />";
									else
										exportstr += "<a href=\"/" + list[1] + "\">" + list[0] + "</a><br />";
								}
							}

						}

						exportstr += "</body></html>";
						Random R = new Random();
						string rf = "mgrand_" + R.Next() + ".html";
						using (FileStream fs = new  FileStream(rf , FileMode.OpenOrCreate, FileAccess.Write)){
							StreamWriter streamWriter = new StreamWriter(fs);
                                                                                                                                              
							streamWriter.WriteLine(exportstr);
							streamWriter.Close();
						}

						filename = rf;  //"html/en/emptysub.html";
						tempfiles.Add (rf);
					}
						
					nodeToAddChildrenTo = latestNodeAddition.CreateNode (attribs[0].Trim(), "xhtml:" + filename);
					
				} else {
					Console.WriteLine( spaces + attribs[0] + ": " + filename);
					// Put the node in the monodoc toc.
					latestNodeAddition.CreateNode (attribs[0].Trim(), "xhtml:" + filename);
				}
				// Put the file in the archive.
				if (File.Exists(filename)){
					if (packed_files [filename] == null){
						packed_files [filename] = filename;
						nodeToAddChildrenTo.tree.HelpSource.PackFile (filename, filename);
					}
				}
				
				string fullpath = Path.Combine(Environment.CurrentDirectory, attribs[1]);
				if(File.Exists(fullpath)) {
					try {
						XmlDocument newdoc = new XmlDocument();
						
						newdoc.Load(fullpath);
						IncludeAttribLinks(newdoc.GetElementsByTagName("a"),"href", filename);
						IncludeAttribLinks(newdoc.GetElementsByTagName("img"),"src",  filename);
						IncludeAttribLinks(newdoc.GetElementsByTagName("link"),"href",  filename);
					} catch {
						Console.WriteLine(spaces + "-- PARSE ERROR --");
						throw;
					}
				} 
				
			}
			
			if (items[i].LocalName == "ul"){
				spaces += "      ";
				ParseUl(items[i].ChildNodes, nodeToAddChildrenTo);
				nodeToAddChildrenTo = latestNodeAddition;
				spaces = spaces.Substring(6);
			}
		}
	}
	
	public string[] ParseLi(XmlNode me)
	{
		string[] values = {null, null};

		try {
		
		foreach (XmlNode param in me.ChildNodes[0].ChildNodes){
			if (param.Attributes.GetNamedItem("name").Value == "Name")
				values[0] =  param.Attributes.GetNamedItem("value").Value;
			
			if (param.Attributes.GetNamedItem("name").Value == "Local")
				values[1] =  param.Attributes.GetNamedItem("value").Value;
		}
		} catch {
			Console.WriteLine ("At: " + me.InnerXml);
			throw;
		}
		
		return values;
		
	}
}
}
