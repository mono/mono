using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Monodoc.Providers
{
	// Common functionality between ecma-provider and ecmauncompiled-provider
	internal class EcmaDoc
	{
		public static void PopulateTreeFromIndexFile (string indexFilePath,
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
						var url = "ecma:" + id + '#' + typeCaption + '/';
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
			return XDocument.Load (finalPath);
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
				reader.ReadToFollowing ("summary");
				var summary = reader.ReadInnerXml ();
				reader.ReadToFollowing ("remarks");
				var remarks = reader.ReadInnerXml ();

				return new XElement ("class",
				                     new XAttribute ("name", name ?? string.Empty),
				                     new XAttribute ("fullname", fullName ?? string.Empty),
				                     new XAttribute ("assembly", assemblyName ?? string.Empty),
				                     new XElement ("summary", new XCData (summary)),
				                     new XElement ("remarks", new XCData (remarks)));
			}
		}
	}
}
