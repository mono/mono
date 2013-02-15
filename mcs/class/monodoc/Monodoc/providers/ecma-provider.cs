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
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

using Lucene.Net.Index;
using Lucene.Net.Documents;

using Monodoc.Ecma;
using Mono.Utilities;

namespace Monodoc.Providers
{
	public class EcmaProvider : Provider
	{
		HashSet<string> directories = new HashSet<string> ();

		public EcmaProvider ()
		{
		}

		public EcmaProvider (string baseDir)
		{
			AddDirectory (baseDir);
		}

		public void AddDirectory (string directory)
		{
			if (string.IsNullOrEmpty (directory))
				throw new ArgumentNullException ("directory");

			directories.Add (directory);
		}

		public override void PopulateTree (Tree tree)
		{
			var storage = tree.HelpSource.Storage;
			var nsSummaries = new Dictionary<string, XElement> ();
			int resID = 0;

			foreach (var asm in directories) {
				var indexFilePath = Path.Combine (asm, "index.xml");
				if (!File.Exists (indexFilePath)) {
					Console.Error.WriteLine ("Warning: couldn't process directory `{0}' as it has no index.xml file", asm);
					continue;
				}

				EcmaDoc.PopulateTreeFromIndexFile (indexFilePath, EcmaHelpSource.EcmaPrefix, tree, storage, nsSummaries, _ => resID++.ToString ());
			}

			foreach (var summary in nsSummaries)
				storage.Store ("xml.summary." + summary.Key, summary.Value.ToString ());

			var masterSummary = new XElement ("elements",
			                                  directories
			                                  .SelectMany (d => Directory.EnumerateFiles (d, "ns-*.xml"))
			                                  .Select (ExtractNamespaceSummary));
			storage.Store ("mastersummary.xml", masterSummary.ToString ());
		}

		XElement ExtractNamespaceSummary (string nsFile)
		{
			using (var reader = XmlReader.Create (nsFile)) {
				reader.ReadToFollowing ("Namespace");
				var name = reader.GetAttribute ("Name");
				reader.ReadToFollowing ("summary");
				var summary = reader.ReadInnerXml ();
				reader.ReadToFollowing ("remarks");
				var remarks = reader.ReadInnerXml ();

				return new XElement ("namespace",
				                     new XAttribute ("ns", name ?? string.Empty),
				                     new XElement ("summary", new XCData (summary)),
				                     new XElement ("remarks", new XCData (remarks)));
			}
		}

		public override void CloseTree (HelpSource hs, Tree tree)
		{
			AddImages (hs);
			AddExtensionMethods (hs);
		}

		void AddEcmaXml (HelpSource hs)
		{
			var xmls = directories
				.SelectMany (Directory.EnumerateDirectories) // Assemblies
				.SelectMany (Directory.EnumerateDirectories) // Namespaces
				.SelectMany (Directory.EnumerateFiles)
				.Where (f => f.EndsWith (".xml")); // Type XML files

			int resID = 0;
			foreach (var xml in xmls)
				using (var file = File.OpenRead (xml))
					hs.Storage.Store ((resID++).ToString (), file);
		}

		void AddImages (HelpSource hs)
		{
			var imgs = directories
				.SelectMany (Directory.EnumerateDirectories)
				.Select (d => Path.Combine (d, "_images"))
				.Where (Directory.Exists)
				.SelectMany (Directory.EnumerateFiles);

			foreach (var img in imgs)
				using (var file = File.OpenRead (img))
					hs.Storage.Store (Path.GetFileName (img), file);
		}

		void AddExtensionMethods (HelpSource hs)
		{
			var extensionMethods = directories
				.SelectMany (Directory.EnumerateDirectories)
				.Select (d => Path.Combine (d, "index.xml"))
				.Where (File.Exists)
				.Select (f => {
					using (var file = File.OpenRead (f)) {
						var reader = XmlReader.Create (file);
						reader.ReadToFollowing ("ExtensionMethods");
						return reader.ReadInnerXml ();
					}
			    })
			    .DefaultIfEmpty (string.Empty);

			hs.Storage.Store ("ExtensionMethods.xml",
			                  "<ExtensionMethods>" + extensionMethods.Aggregate (string.Concat) + "</ExtensionMethods>");
		}

		IEnumerable<string> GetEcmaXmls ()
		{
			return directories
				.SelectMany (Directory.EnumerateDirectories) // Assemblies
				.SelectMany (Directory.EnumerateDirectories) // Namespaces
				.SelectMany (Directory.EnumerateFiles)
				.Where (f => f.EndsWith (".xml")); // Type XML files
		}
	}

	public class EcmaHelpSource : HelpSource
	{
		internal const string EcmaPrefix = "ecma:";
		LRUCache<string, Node> cache = new LRUCache<string, Node> (4);

		public EcmaHelpSource (string base_file, bool create) : base (base_file, create)
		{
		}

		protected EcmaHelpSource () : base ()
		{
		}

		protected override string UriPrefix {
			get {
				return EcmaPrefix;
			}
		}

		public override bool CanHandleUrl (string url)
		{
			if (url.Length > 2 && url[1] == ':') {
				switch (url[0]) {
				case 'T':
				case 'M':
				case 'C':
				case 'P':
				case 'E':
				case 'F':
				case 'N':
				case 'O':
					return true;
				}
			}
			return base.CanHandleUrl (url);
		}

		// Clean the extra paramers in the id
		public override Stream GetHelpStream (string id)
		{
			var idParts = id.Split ('?');
			return base.GetHelpStream (idParts[0]);
		}

		public override Stream GetCachedHelpStream (string id)
		{
			var idParts = id.Split ('?');
			return base.GetCachedHelpStream (idParts[0]);
		}

		public override DocumentType GetDocumentTypeForId (string id)
		{
			return DocumentType.EcmaXml;
		}

		public override string GetPublicUrl (Node node)
		{
			string url = string.Empty;
			var type = EcmaDoc.GetNodeType (node);
			//Console.WriteLine ("GetPublicUrl {0} : {1} [{2}]", node.Element, node.Caption, type.ToString ());
			switch (type) {
			case EcmaNodeType.Namespace:
				return node.Element; // A namespace node has already a well formated internal url
			case EcmaNodeType.Type:
				return MakeTypeNodeUrl (node);
			case EcmaNodeType.Meta:
				return MakeTypeNodeUrl (GetNodeTypeParent (node)) + GenerateMetaSuffix (node);
			case EcmaNodeType.Member:
				var typeChar = EcmaDoc.GetNodeMemberTypeChar (node);
				var parentNode = GetNodeTypeParent (node);
				var typeNode = MakeTypeNodeUrl (parentNode).Substring (2);
				return typeChar + ":" + typeNode + MakeMemberNodeUrl (typeChar, node);
			default:
				return null;
			}
		}

		string MakeTypeNodeUrl (Node node)
		{
			// A Type node has a Element property of the form: 'ecma:{number}#{typename}/'
			var hashIndex = node.Element.IndexOf ('#');
			var typeName = node.Element.Substring (hashIndex + 1, node.Element.Length - hashIndex - 2);
			return "T:" + node.Parent.Caption + '.' + typeName.Replace ('.', '+');
		}

		string MakeMemberNodeUrl (char typeChar, Node node)
		{
			// We clean inner type ctor name which may contain the outer type name
			var caption = node.Caption;

			// Sanitize constructor caption of inner types
			if (typeChar == 'C') {
				int lastDot = -1;
				for (int i = 0; i < caption.Length && caption[i] != '('; i++)
					lastDot = caption[i] == '.' ? i : lastDot;
				return lastDot == -1 ? '.' + caption : caption.Substring (lastDot);
			}

			/* We handle type conversion operator by checking if the name contains " to "
			 * (as in 'foo to bar') and we generate a corresponding conversion signature
			 */
			if (typeChar == 'O' && caption.IndexOf (" to ") != -1) {
				var parts = caption.Split (' ');
				return "." + node.Parent.Caption + "(" + parts[0] + ", " + parts[2] + ")";
			}

			/* The goal here is to treat method which are explicit interface definition
			 * such as 'void IDisposable.Dispose ()' for which the caption is a dot
			 * expression thus colliding with the ecma parser.
			 * If the first non-alpha character in the caption is a dot then we have an
			 * explicit member implementation (we assume the interface has namespace)
			 */
			var firstNonAlpha = caption.FirstOrDefault (c => !char.IsLetterOrDigit (c));
			if (firstNonAlpha == '.')
				return "$" + caption;

			return "." + caption;
		}

		Node GetNodeTypeParent (Node node)
		{
			// Type nodes are always at level 2 so we just need to get there
			while (node != null && node.Parent != null && !node.Parent.Parent.Element.StartsWith ("root:/", StringComparison.OrdinalIgnoreCase))
				node = node.Parent;
			return node;
		}

		string GenerateMetaSuffix (Node node)
		{
			string suffix = string.Empty;
			// A meta node has always a type element to begin with
			while (EcmaDoc.GetNodeType (node) != EcmaNodeType.Type) {
				suffix = '/' + node.Element + suffix;
				node = node.Parent;
			}
			return suffix;
		}

		public override string GetInternalIdForUrl (string url, out Node node, out Dictionary<string, string> context)
		{
			var id = string.Empty;
			node = null;
			context = null;

			if (!url.StartsWith (UriPrefix, StringComparison.OrdinalIgnoreCase)) {
				node = MatchNode (url);
				if (node == null)
					return null;
				id = node.GetInternalUrl ();
			}

			string hash;
			id = GetInternalIdForInternalUrl (id, out hash);
			context = EcmaDoc.GetContextForEcmaNode (hash, SourceID.ToString (), node);

			return id;
		}

		public string GetInternalIdForInternalUrl (string internalUrl, out string hash)
		{
			var id = internalUrl;
			if (id.StartsWith (UriPrefix, StringComparison.OrdinalIgnoreCase))
				id = id.Substring (UriPrefix.Length);
			else if (id.StartsWith ("N:", StringComparison.OrdinalIgnoreCase))
				id = "xml.summary." + id.Substring ("N:".Length);

			var hashIndex = id.IndexOf ('#');
			hash = string.Empty;
			if (hashIndex != -1) {
				hash = id.Substring (hashIndex + 1);
				id = id.Substring (0, hashIndex);
			}

			return id;
		}

		public override Node MatchNode (string url)
		{
			Node node = null;
			if ((node = cache.Get (url)) == null) {
				node = EcmaDoc.MatchNodeWithEcmaUrl (url, Tree);
				if (node != null)
					cache.Put (url, node);
			}
			return node;
		}

		public override void PopulateIndex (IndexMaker index_maker)
		{
			foreach (Node ns_node in Tree.RootNode.ChildNodes){
				foreach (Node type_node in ns_node.ChildNodes){
					string typename = type_node.Caption.Substring (0, type_node.Caption.IndexOf (' '));
					string full = ns_node.Caption + "." + typename;

					string doc_tag = GetKindFromCaption (type_node.Caption);
					string url = type_node.PublicUrl;

					//
					// Add MonoMac/MonoTouch [Export] attributes, those live only in classes
					//
					XDocument type_doc = null;
					ILookup<string, XElement> prematchedMembers = null;
					bool hasExports = doc_tag == "Class" && (ns_node.Caption.StartsWith ("MonoTouch") || ns_node.Caption.StartsWith ("MonoMac"));
					if (hasExports) {
						try {
							string rest, hash;
							var id = GetInternalIdForInternalUrl (type_node.GetInternalUrl (), out hash);
							type_doc = XDocument.Load (GetHelpStream (id));
							prematchedMembers = type_doc.Root.Element ("Members").Elements ("Member").ToLookup (n => (string)n.Attribute ("MemberName"), n => n);
						} catch (Exception e) {
							Console.WriteLine ("Problem processing {0} for MonoTouch/MonoMac exports\n\n{0}", e);
							hasExports = false;
						}
					}

					if (doc_tag == "Class" || doc_tag == "Structure" || doc_tag == "Interface"){
						index_maker.Add (type_node.Caption, typename, url);
						index_maker.Add (full + " " + doc_tag, full, url);

						foreach (Node c in type_node.ChildNodes){
							switch (c.Caption){
							case "Constructors":
								index_maker.Add ("  constructors", typename+"0", url + "/C");
								break;
							case "Fields":
								index_maker.Add ("  fields", typename+"1", url + "/F");
								break;
							case "Events":
								index_maker.Add ("  events", typename+"2", url + "/E");
								break;
							case "Properties":
								index_maker.Add ("  properties", typename+"3", url + "/P");
								break;
							case "Methods":
								index_maker.Add ("  methods", typename+"4", url + "/M");
								break;
							case "Operators":
								index_maker.Add ("  operators", typename+"5", url + "/O");
								break;
							}
						}

						//
						// Now repeat, but use a different sort key, to make sure we come after
						// the summary data above, start the counter at 6
						//
						string keybase = typename + "6.";

						foreach (Node c in type_node.ChildNodes){
							var type = c.Caption[0];

							foreach (Node nc in c.ChildNodes) {
								string res = nc.Caption;
								string nurl = nc.PublicUrl;

								// Process exports
								if (hasExports && (type == 'C' || type == 'M' || type == 'P')) {
									try {
										var member = GetMemberFromCaption (type_doc, type == 'C' ? ".ctor" : res, false, prematchedMembers);
										var exports = member.Descendants ("AttributeName").Where (a => a.Value.Contains ("Foundation.Export"));
										foreach (var exportNode in exports) {
											var parts = exportNode.Value.Split ('"');
											if (parts.Length != 3) {
												Console.WriteLine ("Export attribute not found or not usable in {0}", exportNode);
											} else {
												var export = parts[1];
												index_maker.Add (export + " selector", export, nurl);
											}
										}
									} catch (Exception e) {
										Console.WriteLine ("Problem processing {0}/{1} for MonoTouch/MonoMac exports\n\n{2}", nurl, res, e);
									}
								}

								switch (type){
								case 'C':
									break;
								case 'F':
									index_maker.Add (String.Format ("{0}.{1} field", typename, res),
									                 keybase + res, nurl);
									index_maker.Add (String.Format ("{0} field", res), res, nurl);
									break;
								case 'E':
									index_maker.Add (String.Format ("{0}.{1} event", typename, res),
									                 keybase + res, nurl);
									index_maker.Add (String.Format ("{0} event", res), res, nurl);
									break;
								case 'P':
									index_maker.Add (String.Format ("{0}.{1} property", typename, res),
									                 keybase + res, nurl);
									index_maker.Add (String.Format ("{0} property", res), res, nurl);
									break;
								case 'M':
									index_maker.Add (String.Format ("{0}.{1} method", typename, res),
									                 keybase + res, nurl);
									index_maker.Add (String.Format ("{0} method", res), res, nurl);
									break;
								case 'O':
									index_maker.Add (String.Format ("{0}.{1} operator", typename, res),
									                 keybase + res, nurl);
									break;
								}
							}
						}
					} else if (doc_tag == "Enumeration"){
						//
						// Enumerations: add the enumeration values
						//
						index_maker.Add (type_node.Caption, typename, url);
						index_maker.Add (full + " " + doc_tag, full, url);

						// Now, pull the values.
						string rest, hash;
						var id = GetInternalIdForInternalUrl (type_node.GetInternalUrl (), out hash);
						var xdoc = XDocument.Load (GetHelpStream (id));
						if (xdoc == null)
							continue;

						var members = xdoc.Root.Element ("Members").Elements ("Members");
						if (members == null)
							continue;

						foreach (var member_node in members){
							string enum_value = member_node.Attribute ("MemberName").Value;
							string caption = enum_value + " value";
							index_maker.Add (caption, caption, url);
						}
					} else if (doc_tag == "Delegate"){
						index_maker.Add (type_node.Caption, typename, url);
						index_maker.Add (full + " " + doc_tag, full, url);
					}
				}
			}
		}


		public override void PopulateSearchableIndex (IndexWriter writer)
		{
			StringBuilder text = new StringBuilder ();
			SearchableDocument searchDoc = new SearchableDocument ();

			foreach (Node ns_node in Tree.RootNode.ChildNodes) {
				foreach (Node type_node in ns_node.ChildNodes) {
					string typename = type_node.Caption.Substring (0, type_node.Caption.IndexOf (' '));
					string full = ns_node.Caption + "." + typename;
					string url = type_node.PublicUrl;
					string doc_tag = GetKindFromCaption (type_node.Caption);
					string rest, hash;
					var id = GetInternalIdForInternalUrl (type_node.GetInternalUrl (), out hash);
					var xdoc = XDocument.Load (GetHelpStream (id));
					if (xdoc == null)
						continue;
					if (string.IsNullOrEmpty (doc_tag))
						continue;

					// For classes, structures or interfaces add a doc for the overview and
					// add a doc for every constructor, method, event, ...
					// doc_tag == "Class" || doc_tag == "Structure" || doc_tag == "Interface"
					if (doc_tag[0] == 'C' || doc_tag[0] == 'S' || doc_tag[0] == 'I') {
						// Adds a doc for every overview of every type
						SearchableDocument doc = searchDoc.Reset ();
						doc.Title = type_node.Caption;
						doc.HotText = typename;
						doc.Url = url;
						doc.FullTitle = full;

						var node_sel = xdoc.Root.Element ("Docs");
						text.Clear ();
						GetTextFromNode (node_sel, text);
						doc.Text = text.ToString ();

						text.Clear ();
						GetExamples (node_sel, text);
						doc.Examples = text.ToString ();

						writer.AddDocument (doc.LuceneDoc);
						var exportParsable = doc_tag[0] == 'C' && (ns_node.Caption.StartsWith ("MonoTouch") || ns_node.Caption.StartsWith ("MonoMac"));

						//Add docs for contructors, methods, etc.
						foreach (Node c in type_node.ChildNodes) { // c = Constructors || Fields || Events || Properties || Methods || Operators
							if (c.Element == "*")
								continue;
							const float innerTypeBoost = 0.2f;

							IEnumerable<Node> ncnodes = c.ChildNodes;
							// The rationale is that we need to properly handle method overloads
							// so for those method node which have children, flatten them
							if (c.Caption == "Methods") {
								ncnodes = ncnodes
									.Where (n => n.ChildNodes == null || n.ChildNodes.Count == 0)
									.Concat (ncnodes.Where (n => n.ChildNodes.Count > 0).SelectMany (n => n.ChildNodes));
							} else if (c.Caption == "Operators") {
								ncnodes = ncnodes
									.Where (n => !n.Caption.EndsWith ("Conversion"))
									.Concat (ncnodes.Where (n => n.Caption.EndsWith ("Conversion")).SelectMany (n => n.ChildNodes));
							}

							var prematchedMembers = xdoc.Root.Element ("Members").Elements ("Member").ToLookup (n => (string)n.Attribute ("MemberName"), n => n);

							foreach (Node nc in ncnodes) {
								XElement docsNode = null;
								try {
									docsNode = GetDocsFromCaption (xdoc, c.Caption[0] == 'C' ? ".ctor" : nc.Caption, c.Caption[0] == 'O', prematchedMembers);
								} catch {}
								if (docsNode == null) {
									Console.Error.WriteLine ("Problem: {0}", nc.PublicUrl);
									continue;
								}

								SearchableDocument doc_nod = searchDoc.Reset ();
								doc_nod.Title = LargeName (nc) + " " + EcmaDoc.EtcKindToCaption (c.Caption[0]);
								doc_nod.FullTitle = ns_node.Caption + '.' + typename + "::" + nc.Caption;
								doc_nod.HotText = string.Empty;

								/* Disable constructors hottext indexing as it's often "polluting" search queries
								   because it has the same hottext than standard types */
								if (c.Caption != "Constructors") {
									//dont add the parameters to the hottext
									int ppos = nc.Caption.IndexOf ('(');
									doc_nod.HotText = ppos != -1 ? nc.Caption.Substring (0, ppos) : nc.Caption;
								}

								var urlnc = nc.PublicUrl;
								doc_nod.Url = urlnc;

								text.Clear ();
								GetTextFromNode (docsNode, text);
								doc_nod.Text = text.ToString ();

								text.Clear ();
								GetExamples (docsNode, text);
								doc_nod.Examples = text.ToString ();

								Document lucene_doc = doc_nod.LuceneDoc;
								lucene_doc.Boost = innerTypeBoost;
								writer.AddDocument (lucene_doc);

								// Objective-C binding specific parsing of [Export] attributes
								if (exportParsable) {
									try {
										var exports = docsNode.Parent.Elements ("Attributes").Elements ("Attribute").Elements ("AttributeName")
											.Select (a => (string)a).Where (txt => txt.Contains ("Foundation.Export"));

										foreach (var exportNode in exports) {
											var parts = exportNode.Split ('"');
											if (parts.Length != 3) {
												Console.WriteLine ("Export attribute not found or not usable in {0}", exportNode);
												continue;
											}

											var export = parts[1];
											var export_node = searchDoc.Reset ();
											export_node.Title = export + " Export";
											export_node.FullTitle = ns_node.Caption + '.' + typename + "::" + export;
											export_node.Url = urlnc;
											export_node.HotText = export;
											export_node.Text = string.Empty;
											export_node.Examples = string.Empty;
											lucene_doc = export_node.LuceneDoc;
											lucene_doc.Boost = innerTypeBoost;
											writer.AddDocument (lucene_doc);
										}
									} catch (Exception e){
										Console.WriteLine ("Problem processing {0} for MonoTouch/MonoMac exports\n\n{0}", e);
									}
								}
							}
						}
					// doc_tag == "Enumeration"
					} else if (doc_tag[0] == 'E'){
						var members = xdoc.Root.Element ("Members").Elements ("Member");
						if (members == null)
							continue;

						text.Clear ();
						foreach (var member_node in members) {
							string enum_value = (string)member_node.Attribute ("MemberName");
							text.Append (enum_value);
							text.Append (" ");
							GetTextFromNode (member_node.Element ("Docs"), text);
							text.AppendLine ();
						}

						SearchableDocument doc = searchDoc.Reset ();

						text.Clear ();
						GetExamples (xdoc.Root.Element ("Docs"), text);
						doc.Examples = text.ToString ();

						doc.Title = type_node.Caption;
						doc.HotText = (string)xdoc.Root.Attribute ("Name");
						doc.FullTitle = full;
						doc.Url = url;
						doc.Text = text.ToString();
						writer.AddDocument (doc.LuceneDoc);
					// doc_tag == "Delegate"
					} else if (doc_tag[0] == 'D'){
						SearchableDocument doc = searchDoc.Reset ();
						doc.Title = type_node.Caption;
						doc.HotText = (string)xdoc.Root.Attribute ("Name");
						doc.FullTitle = full;
						doc.Url = url;

						var node_sel = xdoc.Root.Element ("Docs");

						text.Clear ();
						GetTextFromNode (node_sel, text);
						doc.Text = text.ToString();

						text.Clear ();
						GetExamples (node_sel, text);
						doc.Examples = text.ToString();

						writer.AddDocument (doc.LuceneDoc);
					}
				}
			}
		}

		string GetKindFromCaption (string s)
		{
			int p = s.LastIndexOf (' ');
			if (p > 0)
				return s.Substring (p + 1);
			return null;
		}

		// Extract the interesting text from the docs node
		void GetTextFromNode (XElement n, StringBuilder sb)
		{
			// Include the text content of the docs
			sb.AppendLine (n.Value);
			foreach (var tag in n.Descendants ())
				//include the url to which points the see tag and the name of the parameter
				if ((tag.Name.LocalName.Equals ("see", StringComparison.Ordinal) || tag.Name.LocalName.Equals ("paramref", StringComparison.Ordinal))
				    && tag.HasAttributes)
					sb.AppendLine ((string)tag.Attributes ().First ());
		}

		// Extract the code nodes from the docs
		void GetExamples (XElement n, StringBuilder sb)
		{
			foreach (var code in n.Descendants ("code"))
				sb.Append ((string)code);
		}

		// Extract a large name for the Node
		static string LargeName (Node matched_node)
		{
			string[] parts = matched_node.GetInternalUrl ().Split('/', '#');
			if (parts.Length == 3 && parts[2] != String.Empty) //List of Members, properties, events, ...
				return parts[1] + ": " + matched_node.Caption;
			else if(parts.Length >= 4) //Showing a concrete Member, property, ...
				return parts[1] + "." + matched_node.Caption;
			else
				return matched_node.Caption;
		}

		XElement GetMemberFromCaption (XDocument xdoc, string caption, bool isOperator, ILookup<string, XElement> prematchedMembers)
		{
			string name;
			IList<string> args;
			var doc = xdoc.Root.Element ("Members").Elements ("Member");

			if (isOperator) {
				// The first case are explicit and implicit conversion operators which are grouped specifically
				if (caption.IndexOf (" to ") != -1) {
					var convArgs = caption.Split (new[] { " to " }, StringSplitOptions.None);
					return doc
						.First (n => (AttrEq (n, "MemberName", "op_Explicit") || AttrEq (n, "MemberName", "op_Implicit"))
						        && ((string)n.Element ("ReturnValue").Element ("ReturnType")).Equals (convArgs[1], StringComparison.Ordinal)
						        && AttrEq (n.Element ("Parameters").Element ("Parameter"), "Type", convArgs[0]));
				} else {
					return doc.First (m => AttrEq (m, "MemberName", "op_" + caption));
				}
			}

			TryParseCaption (caption, out name, out args);

			if (!string.IsNullOrEmpty (name)) { // Filter member by name
				var prematched = prematchedMembers[name];
				doc = prematched.Any () ? prematched : doc.Where (m => AttrEq (m, "MemberName", name));
			}
			if (args != null && args.Count > 0) // Filter member by its argument list
				doc = doc.Where (m => m.Element ("Parameters").Elements ("Parameter").Attributes ("Type").Select (a => (string)a).SequenceEqual (args));

			return doc.First ();
		}

		XElement GetDocsFromCaption (XDocument xdoc, string caption, bool isOperator, ILookup<string, XElement> prematchedMembers)
		{
			return GetMemberFromCaption (xdoc, caption, isOperator, prematchedMembers).Element ("Docs");
		}

		// A simple stack-based parser to detect single type definition separated by commas
		IEnumerable<string> ExtractArguments (string rawArgList)
		{
			var sb = new System.Text.StringBuilder ();
			int genericDepth = 0;
			int arrayDepth = 0;

			for (int i = 0; i < rawArgList.Length; i++) {
				char c = rawArgList[i];
				switch (c) {
				case ',':
					if (genericDepth == 0 && arrayDepth == 0) {
						yield return sb.ToString ();
						sb.Clear ();
						continue;
					}
					break;
				case '<':
					genericDepth++;
					break;
				case '>':
					genericDepth--;
					break;
				case '[':
					arrayDepth++;
					break;
				case ']':
					arrayDepth--;
					break;
				}
				sb.Append (c);
			}
			if (sb.Length > 0)
				yield return sb.ToString ();
		}

		void TryParseCaption (string caption, out string name, out IList<string> argList)
		{
			name = null;
			argList = null;
			int parenIdx = caption.IndexOf ('(');
			// In case of simple name, there is no need for processing
			if (parenIdx == -1) {
				name = caption;
				return;
			}
			name = caption.Substring (0, parenIdx);
			// Now we gather the argument list if there is any
			var rawArgList = caption.Substring (parenIdx + 1, caption.Length - parenIdx - 2); // Only take what's inside the parens
			if (string.IsNullOrEmpty (rawArgList))
				return;

			argList = ExtractArguments (rawArgList).Select (arg => arg.Trim ()).ToList ();
		}

		bool AttrEq (XElement element, string attributeName, string expectedValue)
		{
			return ((string)element.Attribute (attributeName)).Equals (expectedValue, StringComparison.Ordinal);
		}
	}
}
