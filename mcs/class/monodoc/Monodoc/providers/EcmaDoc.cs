using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

using Monodoc.Ecma;

namespace Monodoc.Providers
{
	public enum EcmaNodeType {
		Invalid,
		Namespace,
		Type,
		Member,
		Meta, // A node that's here to serve as a header for other node
	}

	// Common functionality between ecma-provider and ecmauncompiled-provider
	internal class EcmaDoc
	{
		static EcmaUrlParser parser = new EcmaUrlParser ();

		public static void PopulateTreeFromIndexFile (string indexFilePath,
		                                              string idPrefix,
		                                              Tree tree,
		                                              IDocStorage storage,
		                                              Dictionary<string, XElement> nsSummaries,
		                                              Func<XElement, string> indexGenerator = null)
		{
			var root = tree.RootNode;
			int resID = 0;
			var asm = Path.GetDirectoryName (indexFilePath);

			storage = storage ?? new Storage.NullStorage ();
			// nsSummaries is allowed to be null if the user doesn't care about it
			nsSummaries = nsSummaries ?? new Dictionary<string, XElement> ();
			// default index generator uses a counter
			indexGenerator = indexGenerator ?? (_ => resID++.ToString ());

			using (var reader = XmlReader.Create (File.OpenRead (indexFilePath))) {
				reader.ReadToFollowing ("Types");
				var types = XElement.Load (reader.ReadSubtree ());

				foreach (var ns in types.Elements ("Namespace")) {
					var nsName = (string)ns.Attribute ("Name");
					nsName = !string.IsNullOrEmpty (nsName) ? nsName : "global";
					var nsNode = root.GetOrCreateNode (nsName, "N:" + nsName);

					XElement nsElements;
					if (!nsSummaries.TryGetValue (nsName, out nsElements))
						nsSummaries[nsName] = nsElements = new XElement ("elements",
						                                                 new XElement ("summary"),
						                                                 new XElement ("remarks"));

					foreach (var type in ns.Elements ("Type")) {
						// Add the XML file corresponding to the type to our storage
						var id = indexGenerator (type);
						string typeFilePath;
						var typeDocument = EcmaDoc.LoadTypeDocument (asm, nsName, type.Attribute ("Name").Value, out typeFilePath);
						if (typeDocument == null)
							continue;
						using (var file = File.OpenRead (typeFilePath))
							storage.Store (id, file);
						nsElements.Add (ExtractClassSummary (typeFilePath));

						var typeCaption = EcmaDoc.GetTypeCaptionFromIndex (type);
						var url = idPrefix + id + '#' + typeCaption + '/';
						typeCaption = EcmaDoc.GetTypeCaptionFromIndex (type, true);
						var typeNode = nsNode.CreateNode (typeCaption, url);

						// Add meta "Members" node
						typeNode.CreateNode ("Members", "*");
						var membersNode = typeDocument.Root.Element ("Members");
						if (membersNode == null || !membersNode.Elements ().Any ())
							continue;
						var members = membersNode
							.Elements ("Member")
							.ToLookup (EcmaDoc.GetMemberType);

						foreach (var memberType in members) {
							// We pluralize the member type to get the caption and take the first letter as URL
							var node = typeNode.CreateNode (EcmaDoc.PluralizeMemberType (memberType.Key), memberType.Key[0].ToString ());
							var memberIndex = 0;

							var isCtors = memberType.Key[0] == 'C';

							// We do not escape much member name here
							foreach (var memberGroup in memberType.GroupBy (m => MakeMemberCaption (m, isCtors))) {
								if (memberGroup.Count () > 1) {
									// Generate overload
									var overloadCaption = MakeMemberCaption (memberGroup.First (), false);
									var overloadNode = node.CreateNode (overloadCaption, overloadCaption);
									foreach (var member in memberGroup)
										overloadNode.CreateNode (MakeMemberCaption (member, true), (memberIndex++).ToString ());
									overloadNode.Sort ();
								} else {
									// We treat constructor differently by showing their argument list in all cases
									node.CreateNode (MakeMemberCaption (memberGroup.First (), isCtors), (memberIndex++).ToString ());
								}
							}
							node.Sort ();
						}
					}

					nsNode.Sort ();
				}
				root.Sort ();
			}
		}

		// Utility methods

		public static XDocument LoadTypeDocument (string basePath, string nsName, string typeName)
		{
			string dummy;
			return LoadTypeDocument (basePath, nsName, typeName, out dummy);
		}

		public static XDocument LoadTypeDocument (string basePath, string nsName, string typeName, out string finalPath)
		{
			finalPath = Path.Combine (basePath, nsName, Path.ChangeExtension (typeName, ".xml"));
			if (!File.Exists (finalPath)) {
				Console.Error.WriteLine ("Warning: couldn't process type file `{0}' as it doesn't exist", finalPath);
				return null;
			}

			XDocument doc = null;
			try {
				doc = XDocument.Load (finalPath);
			} catch (Exception e) {
				Console.WriteLine ("Document `{0}' is unparsable, {1}", finalPath, e.ToString ());
			}

			return doc;
		}

		public static string GetTypeCaptionFromIndex (XElement typeNodeFromIndex, bool full = false)
		{
			var t = typeNodeFromIndex;
			var c = ((string)(t.Attribute ("DisplayName") ?? t.Attribute ("Name"))).Replace ('+', '.');
			if (full)
				c += " " + (string)t.Attribute ("Kind");
			return c;
		}

		public static string PluralizeMemberType (string memberType)
		{
			switch (memberType) {
			case "Property":
				return "Properties";
			default:
				return memberType + "s";
			}
		}

		public static string GetMemberType (XElement m)
		{
			return m.Attribute ("MemberName").Value.StartsWith ("op_") ? "Operator" : m.Element ("MemberType").Value;
		}

		public static string MakeMemberCaption (XElement member, bool withArguments)
		{
			var caption = (string)member.Attribute ("MemberName");
			// Use type name instead of .ctor for cosmetic sake
			if (caption == ".ctor") {
				caption = (string)member.Ancestors ("Type").First ().Attribute ("Name");
				// If this is an inner type ctor, strip the parent type reference
				var plusIndex = caption.LastIndexOf ('+');
				if (plusIndex != -1)
					caption = caption.Substring (plusIndex + 1);
			}
			if (caption.StartsWith ("op_")) {
				string sig;
				caption = MakeOperatorSignature (member, out sig);
				caption = withArguments ? sig : caption;
				return caption;
			}
			if (withArguments) {
				var args = member.Element ("Parameters");
				caption += '(';
				if (args != null && args.Elements ("Parameter").Any ()) {
					caption += args.Elements ("Parameter")
						.Select (p => (string)p.Attribute ("Type"))
						.Aggregate ((p1, p2) => p1 + "," + p2);
				}
				caption += ')';
			}
			
			return caption;
		}

		public static Node MatchNodeWithEcmaUrl (string url, Tree tree)
		{
			Node result = null;
			EcmaDesc desc;
			if (!parser.TryParse (url, out desc))
				return null;

			// Namespace search
			Node currentNode = tree.RootNode;
			Node searchNode = new Node () { Caption = desc.Namespace };
			int index = currentNode.ChildNodes.BinarySearch (searchNode, EcmaGenericNodeComparer.Instance);
			if (index >= 0)
				result = currentNode.ChildNodes[index];
			if (desc.DescKind == EcmaDesc.Kind.Namespace || index < 0)
				return result;

			// Type search
			currentNode = result;
			result = null;
			searchNode.Caption = desc.ToCompleteTypeName ();
			if (!desc.GenericTypeArgumentsIsNumeric)
				index = currentNode.ChildNodes.BinarySearch (searchNode, EcmaTypeNodeComparer.Instance);
			else
				index = GenericTypeBacktickSearch (currentNode.ChildNodes, desc);
			if (index >= 0)
				result = currentNode.ChildNodes[index];
			if ((desc.DescKind == EcmaDesc.Kind.Type && !desc.IsEtc) || index < 0)
				return result;

			// Member selection
			currentNode = result;
			result = null;
			var caption = desc.IsEtc ? EtcKindToCaption (desc.Etc) : MemberKindToCaption (desc.DescKind);
			currentNode = FindNodeForCaption (currentNode.ChildNodes, caption);
			if (currentNode == null
			    || (desc.IsEtc && desc.DescKind == EcmaDesc.Kind.Type && string.IsNullOrEmpty (desc.EtcFilter)))
				return currentNode;

			// Member search
			result = null;
			var format = desc.DescKind == EcmaDesc.Kind.Constructor ? EcmaDesc.Format.WithArgs : EcmaDesc.Format.WithoutArgs;
			searchNode.Caption = desc.ToCompleteMemberName (format);
			index = currentNode.ChildNodes.BinarySearch (searchNode, EcmaGenericNodeComparer.Instance);
			if (index < 0)
				return null;
			result = currentNode.ChildNodes[index];
			if (result.ChildNodes.Count == 0 || desc.IsEtc)
				return result;

			// Overloads search
			currentNode = result;
			searchNode.Caption = desc.ToCompleteMemberName (EcmaDesc.Format.WithArgs);
			index = currentNode.ChildNodes.BinarySearch (searchNode, EcmaGenericNodeComparer.Instance);
			if (index < 0)
				return result;
			result = result.ChildNodes[index];

			return result;
		}

		static int GenericTypeBacktickSearch (IList<Node> childNodes, EcmaDesc desc)
		{
			/* Our strategy is to search for the non-generic variant of the type
			 * (which in most case should fail) and then use the closest index
			 * to linearily search for the generic variant with the right generic arg number
			 */
			var searchNode = new Node () { Caption = desc.TypeName };
			int index = childNodes.BinarySearch (searchNode, EcmaTypeNodeComparer.Instance);
			// Place the index in the right start position
			if (index < 0)
				index = ~index;

			for (int i = index; i < childNodes.Count; i++) {
				var currentNode = childNodes[i];
				// Find the index of the generic argument list
				int genericIndex = currentNode.Caption.IndexOf ('<');
				// If we are not on the same base type name anymore, there is no point
				int captionSlice = genericIndex != -1 ? genericIndex : currentNode.Caption.LastIndexOf (' ');
				if (string.Compare (searchNode.Caption, 0,
				                    currentNode.Caption, 0,
				                    Math.Max (captionSlice, searchNode.Caption.Length),
				                    StringComparison.Ordinal) != 0)
					break;

				var numGenerics = CountTypeGenericArguments (currentNode.Caption, genericIndex);
				if (numGenerics == desc.GenericTypeArguments.Count) {
					// Simple comparison if we are not looking for an inner type
					if (desc.NestedType == null)
						return i;
					// If more complicated, we fallback to using EcmaUrlParser
					var caption = currentNode.Caption;
					caption = "T:" + caption.Substring (0, caption.LastIndexOf (' ')).Replace ('.', '+');
					EcmaDesc otherDesc;
					var parser = new EcmaUrlParser ();
					if (parser.TryParse (caption, out otherDesc) && desc.NestedType.Equals (otherDesc.NestedType))
						return i;
				}
			}

			return -1;
		}

		// This comparer returns the answer straight from caption comparison
		class EcmaGenericNodeComparer : IComparer<Node>
		{
			public static readonly EcmaGenericNodeComparer Instance = new EcmaGenericNodeComparer ();

			public int Compare (Node n1, Node n2)
			{
				return string.Compare (n1.Caption, n2.Caption, StringComparison.Ordinal);
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

				return string.Compare (n1.Caption, 0, n2.Caption, 0, Math.Max (length1, length2), StringComparison.Ordinal);
			}

			int CaptionLength (string caption)
			{
				var length = caption.LastIndexOf (' ');
				return length == -1 ? caption.Length : length;
			}
		}

		public static Dictionary<string, string> GetContextForEcmaNode (string hash, string sourceID, Node node)
		{
			var args = new Dictionary<string, string> ();

			args["source-id"] = sourceID;

			if (node != null) {
				var nodeType = GetNodeType (node);
				switch (nodeType) {
				case EcmaNodeType.Namespace:
					args["show"] = "namespace";
					args["namespace"] = node.Element.Substring ("N:".Length);
					break;
				case EcmaNodeType.Type:
					args["show"] = "typeoverview";
					break;
				case EcmaNodeType.Member:
				case EcmaNodeType.Meta:
					switch (GetNodeMemberTypeChar (node)){
					case 'C':
						args["membertype"] = "Constructor";
						break;
					case 'M':
						args["membertype"] = "Method";
						break;
					case 'P':
						args["membertype"] = "Property";
						break;
					case 'F':
						args["membertype"] = "Field";
						break;
					case 'E':
						args["membertype"] = "Event";
						break;
					case 'O':
						args["membertype"] = "Operator";
						break;
					case 'X':
						args["membertype"] = "ExtensionMethod";
						break;
					case '*':
						args["membertype"] = "All";
						break;
					}

					if (nodeType == EcmaNodeType.Meta) {
						args["show"] = "members";
						args["index"] = "all";
					} else {
						args["show"] = "member";
						args["index"] = node.Element;
					}
					break;
				}
			}

			if (!string.IsNullOrEmpty (hash))
				args["hash"] = hash;

			return args;
		}

		public static EcmaNodeType GetNodeType (Node node)
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

		public static char GetNodeMemberTypeChar (Node node)
		{
			int level = GetNodeLevel (node);
			// We try to reach the member group node depending on node nested level
			switch (level) {
			case 2:
				return node.Element[0];
			case 3:
				return node.Parent.Element[0];
			case 4:
				return node.Parent.Parent.Element[0];
			default:
				throw new ArgumentException ("node", "Couldn't determine member type of node `" + node.Caption + "'");
			}
		}

		public static int GetNodeLevel (Node node)
		{
			int i = 0;
			for (; !node.Element.StartsWith ("root:/", StringComparison.OrdinalIgnoreCase); i++) {
				node = node.Parent;
				if (node == null)
					return i - 1;
			}
			return i - 1;
		}

		public static string EtcKindToCaption (char etc)
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

		public static string MemberKindToCaption (EcmaDesc.Kind kind)
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

		public static Node FindNodeForCaption (IList<Node> nodes, string caption)
		{
			foreach (var node in nodes)
				if (node.Caption.Equals (caption, StringComparison.OrdinalIgnoreCase))
					return node;
			return null;
		}

		public static int CountTypeGenericArguments (string typeDefinition, int startIndex = 0)
		{
			int nestedLevel = 0;
			int count = 0;
			bool started = false;

			foreach (char c in typeDefinition.Skip (startIndex)) {
				switch (c) {
				case '<':
					if (!started)
						count = 1;
					started = true;
					nestedLevel++;
					break;
				case ',':
					if (started && nestedLevel == 1)
						count++;
					break;
				case '>':
					nestedLevel--;
					break;
				}
			}

			return count;
		}

		internal static string MakeOperatorSignature (XElement member, out string memberSignature)
		{
			string name = (string)member.Attribute ("MemberName");
			var nicename = name.Substring(3);
			memberSignature = null;

			switch (name) {
			// unary operators: no overloading possible	[ECMA-335 §10.3.1]
			case "op_UnaryPlus":                    // static     R operator+       (T)
			case "op_UnaryNegation":                // static     R operator-       (T)
			case "op_LogicalNot":                   // static     R operator!       (T)
			case "op_OnesComplement":               // static     R operator~       (T)
			case "op_Increment":                    // static     R operator++      (T)
			case "op_Decrement":                    // static     R operator--      (T)
			case "op_True":                         // static  bool operator true   (T)
			case "op_False":                        // static  bool operator false  (T)
			case "op_AddressOf":                    // static     R operator&       (T)
			case "op_PointerDereference":           // static     R operator*       (T)
				memberSignature = nicename;
				break;
			// conversion operators: overloading based on parameter and return type [ECMA-335 §10.3.3]
			case "op_Implicit":                    // static implicit operator R (T)
			case "op_Explicit":                    // static explicit operator R (T)
				nicename = name.EndsWith ("Implicit") ? "ImplicitConversion" : "ExplicitConversion";
				string arg = (string)member.Element ("Parameters").Element ("Parameter").Attribute ("Type");
				string ret = (string)member.Element ("ReturnValue").Element ("ReturnType");
				memberSignature = arg + " to " + ret;
				break;
			// binary operators: overloading is possible [ECMA-335 §10.3.2]
			default:
				memberSignature =
					nicename + "("
					+ string.Join (",", member.Element ("Parameters").Elements ("Parameter").Select (p => (string)p.Attribute ("Type")))
					+ ")";
				break;
			}

			return nicename;
		}

		static XElement ExtractClassSummary (string typeFilePath)
		{
			using (var reader = XmlReader.Create (typeFilePath)) {
				reader.ReadToFollowing ("Type");
				var name = reader.GetAttribute ("Name");
				var fullName = reader.GetAttribute ("FullName");
				reader.ReadToFollowing ("AssemblyName");
				var assemblyName = reader.ReadElementString ();
				var summary = reader.ReadToFollowing ("summary") ? XElement.Load (reader.ReadSubtree ()) : new XElement ("summary");
				var remarks = reader.ReadToFollowing ("remarks") ? XElement.Load (reader.ReadSubtree ()) : new XElement ("remarks");

				return new XElement ("class",
				                     new XAttribute ("name", name ?? string.Empty),
				                     new XAttribute ("fullname", fullName ?? string.Empty),
				                     new XAttribute ("assembly", assemblyName ?? string.Empty),
				                     summary,
				                     remarks);
			}
		}
	}
}
