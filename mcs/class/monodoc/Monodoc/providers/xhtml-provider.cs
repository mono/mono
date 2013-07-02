//
// A provider that uses Windows help file xhtml TOC files and looks for the
// referenced documents to create the help source. 
//
// Authors:
// Copyright 2003 Lee Mallabone <gnome@fonicmonkey.net>
//   Johannes Roith <johannes@roith.de>
//   Miguel de Icaza <miguel@ximian.com>

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Monodoc.Providers
{
	public class XhtmlProvider : Provider
	{
		string tocFile;
		readonly XNamespace ns = "http://www.w3.org/1999/xhtml";
	
		public XhtmlProvider (string handbookTocFile)
		{
			tocFile = handbookTocFile;
			if (!File.Exists (tocFile))
				throw new FileNotFoundException (String.Format ("The table of contents, `{0}' does not exist", tocFile));		
		}

		public override void PopulateTree (Tree tree)
		{
			var doc = XDocument.Load (tocFile);
			var uls = doc.Descendants (ns + "body").First ().Elements (ns + "ul");
			foreach (var ul in uls)
				ParseUl (tree, tree.RootNode, ul);
		}

		void ParseUl (Tree tree, Node parent, XElement ul)
		{
			var storage = tree.HelpSource.Storage;
			foreach (var e in ul.Elements (ns + "li")) {
				var inner = e.Element (ns + "object");
				if (inner == null)
					continue;
				string caption, element;
				ObjectEntryToParams (inner, out caption, out element);
				// Don't add if the backing file doesn't exist
				if (!File.Exists (element)) {
					Console.Error.WriteLine ("File `{0}' referenced in TOC but it doesn't exist.", element);
					continue;
				}
				using (var file = File.OpenRead (element))
					storage.Store (element, file);
				parent.CreateNode (caption, XhtmlHelpSource.XhtmlPrefix + element);
			}
		}

		void ObjectEntryToParams (XElement obj, out string caption, out string element)
		{
			var ps = obj.Elements (ns + "param");
			caption = ps
				.Where (p => p.Attribute ("name").Value == "Name")
				.Select (p => (string)p.Attribute ("value"))
				.FirstOrDefault ();
			caption = caption ?? string.Empty;

			element = ps
				.Where (p => p.Attribute ("name").Value == "Local")
				.Select (p => (string)p.Attribute ("value"))
				.FirstOrDefault ();
			element = element ?? string.Empty;
		}

		public override void CloseTree (HelpSource hs, Tree tree)
		{
		}
	}

	public class XhtmlHelpSource : HelpSource
	{
		public XhtmlHelpSource (string base_file, bool create) : base (base_file, create)
		{

		}

		internal const string XhtmlPrefix = "xhtml:";

		protected override string UriPrefix {
			get {
				return XhtmlPrefix;
			}
		}

		public override SortType SortType {
			get {
				return SortType.Element;
			}
		}
		
		public override DocumentType GetDocumentTypeForId (string id)
		{
			return id == "root:" ? DocumentType.TocXml : DocumentType.MonoBook;
		}

		public override bool IsGeneratedContent (string id)
		{
			return id == "root:";
		}
	
		public override string GetText (string url)
		{
			return TreeDumper.ExportToTocXml (Tree.RootNode, "Mono Handbook", string.Empty);
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
	
		XmlDocument RewriteLinks(XmlDocument docToProcess, string url)
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

		public override void PopulateIndex (IndexMaker index_maker)
		{
			PopulateIndexFromNodes (Tree.RootNode);
		}

		void PopulateIndexFromNodes (Node start)
		{
			/*var nodes = start.Nodes;
		
			if (nodes != null) {
				foreach (Node n in nodes)
					PopulateIndexFromNodes (n);
			}*/
		}
	}
}
