//
// A provider to display man pages
//
// Authors:
//   Johannes Roith <johannes@roith.de>
//   Jonathan Pryor <jpryor@novell.com>
//
// (C) 2008 Novell, Inc.

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Monodoc.Providers
{
	public class ManProvider : Provider
	{
		string[] tocFiles;
	
		public ManProvider (string[] handbookTocFiles)
		{
			tocFiles = handbookTocFiles;

			// huh...
			if (!File.Exists (tocFiles[0]))
				throw new FileNotFoundException (String.Format ("The table of contents, `{0}' does not exist", tocFiles[0]));
		}

		public override void PopulateTree (Tree tree)
		{
			foreach(string TocFile in tocFiles) {
				XmlDocument doc = new XmlDocument();
				doc.Load (TocFile);

				XmlNodeList nodeList = doc.GetElementsByTagName("manpage");
				Node nodeToAddChildrenTo = tree.RootNode;
				var storage = nodeToAddChildrenTo.Tree.HelpSource.Storage;

				foreach (XmlNode node in nodeList) {

					XmlAttribute name = node.Attributes["name"];
					XmlAttribute page = node.Attributes["page"];

					if (name == null || page == null) continue;

					if (!File.Exists (page.Value))
						continue;

					string target = "man:" + name.Value;
					nodeToAddChildrenTo.CreateNode (name.Value, target);

					if (File.Exists (page.Value))
						using (var file = File.OpenRead (page.Value))
							storage.Store (name.Value, file);
				}
			}
		}

		public override void CloseTree (HelpSource hs, Tree tree)
		{
		}
	}

	public class ManHelpSource : HelpSource
	{
		const string ManPrefix = "man:";
		Dictionary<string, Node> nodesMap;

		public ManHelpSource (string base_file, bool create) : base (base_file, create)
		{
			nodesMap = Tree.RootNode.ChildNodes.ToDictionary (n => n.Element);
		}

		// Since man always has a flat tree and rather small amount of item
		// we store them in a dictionary
		public override Node MatchNode (string url)
		{
			Node result;
			return nodesMap.TryGetValue (url, out result) ? result : null;
		}

		public override DocumentType GetDocumentTypeForId (string id)
		{
			return id == "root:" ? DocumentType.TocXml : DocumentType.Man;
		}

		public override bool IsGeneratedContent (string id)
		{
			return id == "root:";
		}
	
		public override string GetText (string url)
		{
			return TreeDumper.ExportToTocXml (Tree.RootNode, "Mono Documentation Library", "Available man pages:");
		}

		protected override string UriPrefix {
			get {
				return ManPrefix;
			}
		}
	}
}
