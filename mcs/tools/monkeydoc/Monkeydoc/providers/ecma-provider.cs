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

using Mono.Lucene.Net.Index;
using Mono.Lucene.Net.Documents;

using Monkeydoc.Ecma;
using Mono.Utilities;

namespace MonkeyDoc.Providers
{
	public enum EcmaNodeType {
		Invalid,
		Namespace,
		Type,
		Member,
		Meta, // A node that's here to serve as a header for other node
	}

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
			var root = tree.RootNode;
			var storage = tree.HelpSource.Storage;
			int resID = 0;

			foreach (var asm in directories) {
				var indexFilePath = Path.Combine (asm, "index.xml");
				if (!File.Exists (indexFilePath)) {
					Console.Error.WriteLine ("Warning: couldn't process directory `{0}' as it has no index.xml file", asm);
					continue;
				}
				using (var reader = XmlReader.Create (File.OpenRead (indexFilePath))) {
					reader.ReadToFollowing ("Types");
					var types = XElement.Load (reader.ReadSubtree ());

					foreach (var ns in types.Elements ("Namespace")) {
						var nsName = (string)ns.Attribute ("Name");
						var nsNode = root.GetOrCreateNode (!string.IsNullOrEmpty (nsName) ? nsName : "global::", "N:" + ns.Attribute ("Name").Value);

						foreach (var type in ns.Elements ("Type")) {
							// Add the XML file corresponding to the type to our storage
							var id = resID++;
							var typeFilePath = Path.Combine (asm, nsName, Path.ChangeExtension (type.Attribute ("Name").Value, ".xml"));
							if (!File.Exists (typeFilePath)) {
								Console.WriteLine ("Warning: couldn't process type file `{0}' as it doesn't exist", typeFilePath);
								continue;
							}
							using (var file = File.OpenRead (typeFilePath))
								storage.Store (id.ToString (), file);

							var url = "ecma:" + id + type.Attribute ("Name").Value;
							var typeNode = nsNode.CreateNode ((string)(type.Attribute ("DisplayName") ?? type.Attribute ("Name")), url);

							// Add meta "Members" node
							typeNode.CreateNode ("Members", "*");
							var typeDocument = XDocument.Load (typeFilePath);
							var membersNode = typeDocument.Root.Element ("Members");
							if (membersNode == null || !membersNode.Elements ().Any ())
								continue;
							var members = membersNode.Elements ("Member").ToLookup (m => m.Element ("MemberType").Value);
							foreach (var memberType in members) {
								// We pluralize the member type to get the caption and take the first letter as URL
								var node = typeNode.CreateNode (PluralizeMemberType (memberType.Key), memberType.Key[0].ToString ());
								int memberIndex = 0;
								// We do not escape much member name here
								foreach (var member in memberType)
									node.CreateNode (MakeMemberCaption (member), (memberIndex++).ToString ());
							}
						}
						nsNode.Sort ();
					}
					root.Sort ();
				}
			}
		}

		string PluralizeMemberType (string memberType)
		{
			switch (memberType) {
			case "Property":
				return "Properties";
			default:
				return memberType + "s";
			}
		}

		string MakeMemberCaption (XElement member)
		{
			var caption = (string)member.Attribute ("MemberName");
			var args = member.Element ("Parameters");
			if (args != null && args.Elements ("Parameter").Any ()) {
				caption += '(';
				caption += args.Elements ("Parameter")
				               .Select (p => (string)p.Attribute ("Type"))
				               .Aggregate ((p1, p2) => p1 + "," + p2);
				caption += ')';
			}
			
			return caption;
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
		const string EcmaPrefix = "ecma:";
		EcmaUrlParser parser = new EcmaUrlParser ();
		LRUCache<string, Node> cache = new LRUCache<string, Node> (4);

		public EcmaHelpSource (string base_file, bool create) : base (base_file, create)
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

		public override DocumentType GetDocumentTypeForId (string id, out Dictionary<string, string> extraParams)
		{
			extraParams = null;
			int interMark = id.LastIndexOf ('?');
			if (interMark != -1)
				extraParams = id.Substring (interMark)
					.Split ('&')
					.Select (nvp => {
						var eqIdx = nvp.IndexOf ('=');
						return new { Key = nvp.Substring (0, eqIdx < 0 ? nvp.Length : eqIdx), Value = nvp.Substring (eqIdx + 1) };
					})
					.ToDictionary (kvp => kvp.Key, kvp => kvp.Value );

			return DocumentType.EcmaXml;
		}

		public override string GetPublicUrl (Node node)
		{
			string url = string.Empty;
			var type = GetNodeType (node);
			//Console.WriteLine ("GetPublicUrl {0} : {1} [{2}]", node.Element, node.Caption, type.ToString ());
			switch (type) {
			case EcmaNodeType.Namespace:
				return node.Element; // A namespace node has already a well formated internal url
			case EcmaNodeType.Type:
				return MakeTypeNodeUrl (node);
			case EcmaNodeType.Meta:
				return MakeTypeNodeUrl (GetNodeTypeParent (node)) + GenerateMetaSuffix (node);
			case EcmaNodeType.Member:
				var typeChar = GetNodeMemberTypeChar (node);
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

		EcmaNodeType GetNodeType (Node node)
		{
			// We guess the node type by checking the depth level it's at in the tree
			int level = GetNodeLevel (node);
			switch (level) {
			case 0:
				return EcmaNodeType.Namespace;
			case 1:
				return EcmaNodeType.Type;
			case 2:
				return EcmaNodeType.Meta;
			case 3: // Here it's either a member or, in case of overload, a meta
				return node.IsLeaf ? EcmaNodeType.Member : EcmaNodeType.Meta;
			case 4: // At this level, everything is necessarily a member
				return EcmaNodeType.Member;
			default:
				return EcmaNodeType.Invalid;
			}
		}

		int GetNodeLevel (Node node)
		{
			int i = 0;
			for (; !node.Element.StartsWith ("root:/", StringComparison.OrdinalIgnoreCase); i++) {
				//Console.WriteLine ("\tLevel {0} : {1} {2}", i, node.Element, node.Caption);
				node = node.Parent;
			}
			return i - 1;
		}

		char GetNodeMemberTypeChar (Node node)
		{
			int level = GetNodeLevel (node);
			// Only methods/operators can be under a meta node, so in case the member level is
			// deeper than normal (which indicate an overload meta), return 'M' directly
			return level == 3 ? node.Parent.Element[0] : node.Parent.Parent.Element[0];
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
			while (GetNodeType (node) != EcmaNodeType.Type) {
				suffix = '/' + node.Element + suffix;
				node = node.Parent;
			}
			return suffix;
		}

		public override string GetInternalIdForUrl (string url, out Node node)
		{
			var id = string.Empty;
			node = null;

			if (!url.StartsWith (EcmaPrefix, StringComparison.OrdinalIgnoreCase)) {
				node = MatchNode (url);
				if (node == null)
					return null;
				id = node.GetInternalUrl ();
			}

			if (id.StartsWith (UriPrefix, StringComparison.OrdinalIgnoreCase))
				id = id.Substring (UriPrefix.Length);
			else if (id.StartsWith ("N:", StringComparison.OrdinalIgnoreCase))
				id = "xml.summary." + id.Substring ("N:".Length);

			var hashIndex = id.IndexOf ('#');
			var hash = string.Empty;
			if (hashIndex != -1) {
				hash = id.Substring (hashIndex + 1);
				id = id.Substring (0, hashIndex);
			}

			return id + GetArgs (hash, node);
		}

		public override Node MatchNode (string url)
		{
			Node node = null;
			if ((node = cache.Get (url)) == null) {
				node = InternalMatchNode (url);
				if (node != null)
					cache.Put (url, node);
			}
			return node;
		}

		public Node InternalMatchNode (string url)
		{
			Node result = null;
			EcmaDesc desc;
			if (!parser.TryParse (url, out desc))
				return null;

			// Namespace search
			Node currentNode = Tree.RootNode;
			Node searchNode = new Node () { Caption = desc.Namespace };
			int index = currentNode.Nodes.BinarySearch (searchNode, EcmaGenericNodeComparer.Instance);
			if (index >= 0)
				result = currentNode.Nodes[index];
			if (desc.DescKind == EcmaDesc.Kind.Namespace || index < 0)
				return result;

			// Type search
			currentNode = result;
			result = null;
			searchNode.Caption = desc.ToCompleteTypeName ();
			index = currentNode.Nodes.BinarySearch (searchNode, EcmaTypeNodeComparer.Instance);
			if (index >= 0)
				result = currentNode.Nodes[index];
			if ((desc.DescKind == EcmaDesc.Kind.Type && !desc.IsEtc) || index < 0)
				return result;

			// Member selection
			currentNode = result;
			result = null;
			var caption = desc.IsEtc ? EtcKindToCaption (desc.Etc) : MemberKindToCaption (desc.DescKind);
			currentNode = FindNodeForCaption (currentNode.Nodes, caption);
			if (currentNode == null 
			    || (desc.IsEtc && desc.DescKind == EcmaDesc.Kind.Type && string.IsNullOrEmpty (desc.EtcFilter)))
				return currentNode;

			// Member search
			result = null;
			var format = desc.DescKind == EcmaDesc.Kind.Constructor ? EcmaDesc.Format.WithArgs : EcmaDesc.Format.WithoutArgs;
			searchNode.Caption = desc.ToCompleteMemberName (format);
			index = currentNode.Nodes.BinarySearch (searchNode, EcmaGenericNodeComparer.Instance);
			if (index < 0)
				return null;
			result = currentNode.Nodes[index];
			if (result.Nodes.Count == 0 || desc.IsEtc)
				return result;

			// Overloads search
			currentNode = result;
			searchNode.Caption = desc.ToCompleteMemberName (EcmaDesc.Format.WithArgs);
			index = currentNode.Nodes.BinarySearch (searchNode, EcmaGenericNodeComparer.Instance);
			if (index < 0)
				return result;
			result = result.Nodes[index];
			
			return result;
		}

		// This comparer returns the answer straight from caption comparison
		class EcmaGenericNodeComparer : IComparer<Node>
		{
			public static readonly EcmaGenericNodeComparer Instance = new EcmaGenericNodeComparer ();

			public int Compare (Node n1, Node n2)
			{
				return string.Compare (n1.Caption, n2.Caption, StringComparison.OrdinalIgnoreCase);
			}
		}

		// This comparer take into account the space in the caption
		class EcmaTypeNodeComparer : IComparer<Node>
		{
			public static readonly EcmaTypeNodeComparer Instance = new EcmaTypeNodeComparer ();

			public int Compare (Node n1, Node n2)
			{
				int length1 = CaptionLength (n1.Caption);
				int length2 = CaptionLength (n2.Caption);

				return string.Compare (n1.Caption, 0, n2.Caption, 0, Math.Max (length1, length2), StringComparison.OrdinalIgnoreCase);
			}

			int CaptionLength (string caption)
			{
				var length = caption.LastIndexOf (' ');
				return length == -1 ? caption.Length : length;
			}
		}

		string EtcKindToCaption (char etc)
		{
			switch (etc) {
			case 'M':
				return "Methods";
			case 'P':
				return "Properties";
			case 'C':
				return "Constructors";
			case 'F':
				return "Fields";
			case 'E':
				return "Events";
			case 'O':
				return "Operators";
			case '*':
				return "Members";
			default:
				return null;
			}
		}

		string MemberKindToCaption (EcmaDesc.Kind kind)
		{
			switch (kind) {
			case EcmaDesc.Kind.Method:
				return "Methods";
			case EcmaDesc.Kind.Property:
				return "Properties";
			case EcmaDesc.Kind.Constructor:
				return "Constructors";
			case EcmaDesc.Kind.Field:
				return "Fields";
			case EcmaDesc.Kind.Event:
				return "Events";
			case EcmaDesc.Kind.Operator:
				return "Operators";
			default:
				return null;
			}
		}

		Node FindNodeForCaption (List<Node> nodes, string caption)
		{
			foreach (var node in nodes)
				if (node.Caption.Equals (caption, StringComparison.Ordinal))
					return node;
			return null;
		}

		string GetArgs (string hash, Node node)
		{
			var args = new Dictionary<string, string> ();
			
			args["source-id"] = SourceID.ToString ();
			
			if (node != null) {
				switch (GetNodeType (node)) {
				case EcmaNodeType.Namespace:
					args["show"] = "namespace";
					args["namespace"] =  node.Element.Substring ("N:".Length);
					break;
				}
			}

			if (!string.IsNullOrEmpty (hash))
				args["hash"] = hash;

			return "?" + string.Join ("&", args.Select (kvp => kvp.Key == kvp.Value ? kvp.Key : kvp.Key + '=' + kvp.Value));
		}
	}
}
