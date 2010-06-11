//
// The provider for a tree of ECMA documents
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Joshua Tauberer (tauberer@for.net)
//
// (C) 2002, 2003 Ximian, Inc.
// (C) 2003 Joshua Tauberer.
//
// TODO:
//   Should cluster together constructors
//
// Easy:
//   Should render attributes on the signature.
//   Include examples as well.
//
namespace Monodoc {
using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;
using System.Collections;
using Monodoc.Lucene.Net.Index;
using Monodoc.Lucene.Net.Documents;

using Mono.Documentation;

using BF = System.Reflection.BindingFlags;

//
// Helper routines to extract information from an Ecma XML document
//
public static class EcmaDoc {
	public static string GetFullClassName (XmlDocument doc)
	{
		return doc.SelectSingleNode ("/Type").Attributes ["FullName"].InnerText;
	}
	
	public static string GetClassName (XmlDocument doc)
	{
		return GetDisplayName (doc.SelectSingleNode ("/Type")).Replace ("+", ".");
	}

	public static string GetDisplayName (XmlNode type)
	{
		if (type.Attributes ["DisplayName"] != null) {
			return type.Attributes ["DisplayName"].Value;
		}
		return type.Attributes ["Name"].Value;
	}

	public static string GetClassAssembly (XmlDocument doc)
	{
		return doc.SelectSingleNode ("/Type/AssemblyInfo/AssemblyName").InnerText;
	}

	public static string GetClassNamespace (XmlDocument doc)
	{
		string s = doc.SelectSingleNode ("/Type").Attributes ["FullName"].InnerText;

		return s.Substring (0, s.LastIndexOf ("."));
	}
	
	public static string GetTypeKind (XmlDocument doc)
	{
		XmlNode node = doc.SelectSingleNode ("/Type/Base/BaseTypeName");

		if (node == null){
			if (GetFullClassName (doc) == "System.Object")
				return "Class";
			return "Interface";
		}

		switch (node.InnerText){

		case "System.Delegate":
		case "System.MulticastDelegate":
			return "Delegate";
		case "System.ValueType":
			return "Structure";
		case "System.Enum":
			return "Enumeration";
		default:
			return "Class";
		}
	}

	//
	// Utility function: converts a fully .NET qualified type name into a C#-looking one
	//
	public static string ConvertCTSName (string type)
	{
		if (!type.StartsWith ("System."))
			return type;

		if (type.EndsWith ("*"))
			return ConvertCTSName(type.Substring(0, type.Length - 1)) + "*";
		if (type.EndsWith ("&"))
			return ConvertCTSName(type.Substring(0, type.Length - 1)) + "&";
		if (type.EndsWith ("[]"))
			return ConvertCTSName(type.Substring(0, type.Length - 2)) + "[]";

		switch (type) {
		case "System.Byte": return "byte";
		case "System.SByte": return "sbyte";
		case "System.Int16": return "short";
		case "System.Int32": return "int";
		case "System.Int64": return "long";
			
		case "System.UInt16": return "ushort";
		case "System.UInt32": return "uint";
		case "System.UInt64": return "ulong";
			
		case "System.Single":  return "float";
		case "System.Double":  return "double";
		case "System.Decimal": return "decimal";
		case "System.Boolean": return "bool";
		case "System.Char":    return "char";
		case "System.String":  return "string";
			
		case "System.Object":  return "object";
		case "System.Void":  return "void";
		}

		if (type.LastIndexOf(".") == 6)
			return type.Substring(7);
		
		return type;
	}

	internal static string GetNamespaceFile (string dir, string ns)
	{
		string nsxml = Path.Combine (dir, "ns-" + ns + ".xml");
		if (!File.Exists (nsxml))
			nsxml = Path.Combine (dir, ns + ".xml");
		return nsxml;
	}

	public static string GetCref (XmlElement member)
	{
		string typeName = XmlDocUtils.ToEscapedTypeName (member.SelectSingleNode("/Type/@FullName").InnerText);
		if (member.Name == "Type")
			return "T:" + typeName;
		string memberType = member.SelectSingleNode("MemberType").InnerText;
		switch (memberType) {
			case "Constructor":
				return "C:" + typeName + MakeArgs(member);
			case "Event":
				return "E:" + typeName + "." + XmlDocUtils.ToEscapedMemberName (member.GetAttribute("MemberName"));
			case "Field":
				return "F:" + typeName + "." + XmlDocUtils.ToEscapedMemberName (member.GetAttribute("MemberName"));
			case "Method": {
				string name = "M:" + typeName + "." + XmlDocUtils.ToEscapedMemberName (member.GetAttribute("MemberName")) + MakeArgs(member);
				if (member.GetAttribute("MemberName") == "op_Implicit" || member.GetAttribute("MemberName") == "op_Explicit")
					name += "~" + XmlDocUtils.ToTypeName (member.SelectSingleNode("ReturnValue/ReturnType").InnerText, member);
				return name;
			}
			case "Property":
				return "P:" + typeName + "." + XmlDocUtils.ToEscapedMemberName (member.GetAttribute("MemberName")) + MakeArgs(member);
			default:
				throw new NotSupportedException ("MemberType '" + memberType + "' is not supported.");
		}
	}
	
	private static string MakeArgs (XmlElement member)
	{
		XmlNodeList parameters = member.SelectNodes ("Parameters/Parameter");
		if (parameters.Count == 0)
			return "";
		StringBuilder args = new StringBuilder ();
		args.Append ("(");
		args.Append (XmlDocUtils.ToTypeName (parameters [0].Attributes ["Type"].Value, member));
		for (int i = 1; i < parameters.Count; ++i) {
			args.Append (",");
			args.Append (XmlDocUtils.ToTypeName (parameters [i].Attributes ["Type"].Value, member));
		}
		args.Append (")");
		return args.ToString ();
	}
}

//
// The Ecma documentation provider:
//
// It populates a tree with contents during the help assembly
//
public class EcmaProvider : Provider {
	ArrayList/*<string>*/ asm_dirs = new ArrayList ();

	public EcmaProvider ()
	{
	}

	public EcmaProvider (string base_directory)
	{
		AddDirectory (base_directory);
	}

	public void AddDirectory (string directory)
	{
		if (!Directory.Exists (directory))
			throw new FileNotFoundException (String.Format ("The directory `{0}' does not exist", directory));
		asm_dirs.Add (directory);
	}
	
	public override void PopulateTree (Tree tree)
	{
		ArrayList ns_dirs = new ArrayList ();
		foreach (string asm in asm_dirs) {
			ns_dirs.AddRange (Directory.GetDirectories (asm));
		}

		foreach (string ns in ns_dirs){
			string basedir = Directory.GetParent (ns).FullName;
			string [] files = Directory.GetFiles (ns);
			Node ns_node = null;
			string tn = null;
			
			Hashtable nsnodes = new Hashtable();

			foreach (string file in files){
				if (!file.EndsWith (".xml"))
					continue;

				if (ns_node == null) {
					tn = Path.GetFileName (ns);
					tree.HelpSource.Message (TraceLevel.Info, "Processing namespace {0}", tn);
					ns_node = tree.LookupNode (tn, "N:" + tn);
					string ns_summary_file = EcmaDoc.GetNamespaceFile (basedir, tn);
					
					nsnodes[ns_node] = nsnodes;
					
					if (File.Exists (ns_summary_file)) {
						XmlDocument nsSummaryFile = new XmlDocument ();
						nsSummaryFile.Load (ns_summary_file);
						namespace_realpath [tn] = ns_summary_file;
						
						XmlNode ns_summary = nsSummaryFile.SelectSingleNode ("Namespace/Docs/summary");
						if (ns_summary != null && ns_summary.InnerText.Trim () != "To be added." && ns_summary.InnerText != "") {
							namespace_summaries [tn]  = detached.ImportNode (ns_summary, true);
							namespace_remarks [tn]    = detached.ImportNode (nsSummaryFile.SelectSingleNode ("Namespace/Docs/remarks"), true);
						}
						
					} else if (!namespace_summaries.ContainsKey (tn)) {
						namespace_summaries [tn] = null;
						namespace_remarks [tn] = null;
					}
				}
				tree.HelpSource.Message (TraceLevel.Verbose, "    Processing input file {0}", Path.GetFileName (file));

				PopulateClass (tree, tn, ns_node, file);
			}
			
			// Sort the list of types in each namespace
			foreach (Node ns_node2 in nsnodes.Keys)
				ns_node2.Sort();
		}

	}
		
	class TypeInfo : IComparable {
		public string type_assembly;
		public string type_name;
		public string type_full;
		public string type_kind;
		public XmlNode type_doc;

		public TypeInfo (string k, string a, string f, string s, XmlNode n)
		{
			type_assembly = a;
			type_name = s;
			type_doc = n;
			type_kind = k;
			type_full = f;
		}

		public int CompareTo (object b)
		{
			TypeInfo na = this;
			TypeInfo nb = (TypeInfo) b;

			return String.Compare (na.type_full, nb.type_full);
		}
	}
	
	//
	// Packs a file with the summary data
	//
	public override void CloseTree (HelpSource hs, Tree tree)
	{
		foreach (DictionaryEntry de in class_summaries){
			XmlDocument doc = new XmlDocument ();
			string ns = (string) de.Key;
			
			ArrayList list = (ArrayList) de.Value;
			list.Sort();

			XmlElement elements = doc.CreateElement ("elements");
			doc.AppendChild (elements);
			
			if (namespace_summaries [ns] != null)
				elements.AppendChild (doc.ImportNode ((XmlNode)namespace_summaries [ns],true));
			else
				elements.AppendChild (doc.CreateElement("summary"));
			
			if (namespace_remarks [ns] != null)
				elements.AppendChild (doc.ImportNode ((XmlNode)namespace_remarks [ns],true));
			else
				elements.AppendChild (doc.CreateElement("remarks"));
			
			hs.Message (TraceLevel.Info, "Have {0} elements in the {1}", list.Count, ns);
			foreach (TypeInfo p in list){
				XmlElement e = null;
				
				switch (p.type_kind){
				case "Class":
					e = doc.CreateElement ("class"); 
					break;
					
				case "Enumeration":
					e = doc.CreateElement ("enum");
					break;
					
				case "Structure":
					e = doc.CreateElement ("struct");
					break;
					
				case "Delegate":
					e = doc.CreateElement ("delegate");
					break;
					
				case "Interface":
					e = doc.CreateElement ("interface");
					break;
				}
				
				e.SetAttribute ("name", p.type_name);
				e.SetAttribute ("fullname", p.type_full);
				e.SetAttribute ("assembly", p.type_assembly);
				XmlNode copy = doc.ImportNode (p.type_doc, true);
				e.AppendChild (copy);
				elements.AppendChild (e);
			}
			hs.PackXml ("xml.summary." + ns, doc,(string) namespace_realpath[ns]);
		}
		
		
		XmlDocument nsSummary = new XmlDocument ();
		XmlElement root = nsSummary.CreateElement ("elements");
		nsSummary.AppendChild (root);
		
		foreach (DictionaryEntry de in namespace_summaries) {
			XmlNode n = (XmlNode)de.Value;
			XmlElement summary = nsSummary.CreateElement ("namespace");
			summary.SetAttribute ("ns", (string)de.Key);
			root.AppendChild (summary);
			if (n != null)
				summary.AppendChild (nsSummary.ImportNode (n,true));
			else
				summary.AppendChild (nsSummary.CreateElement("summary"));
		}
		tree.HelpSource.PackXml ("mastersummary.xml", nsSummary, null);
		AddExtensionMethods (tree);
	}

	void AddExtensionMethods (Tree tree)
	{
		XmlDocument extensions = null;
		XmlNode root = null;
		int numMethods = 0;
		foreach (string asm in asm_dirs) {
			string overview_file = Path.Combine (asm, "index.xml");
			if (File.Exists (overview_file)) {
				XmlDocument overview = new XmlDocument ();
				overview.Load (overview_file);
				XmlNodeList e = overview.SelectNodes ("/Overview/ExtensionMethods/*");
				if (e.Count > 0) {
					if (extensions == null) {
						extensions = new XmlDocument ();
						root = extensions.CreateElement ("ExtensionMethods");
						extensions.AppendChild (root);
					}
					foreach (XmlNode n in e) {
						++numMethods;
						root.AppendChild (extensions.ImportNode (n, true));
					}
				}
			}
		}
		if (extensions != null) {
			tree.HelpSource.Message (TraceLevel.Info, "Have {0} extension methods", numMethods);
			tree.HelpSource.PackXml ("ExtensionMethods.xml", extensions, "ExtensionMethods.xml");
		}
	}
	       
	Hashtable/*<string, List<TypeInfo>>*/ class_summaries = new Hashtable ();
	Hashtable/*<string, XmlNode>*/ namespace_summaries = new Hashtable ();
	Hashtable/*<string, XmlNode>*/ namespace_remarks = new Hashtable ();
	Hashtable/*<string, string -- path>*/ namespace_realpath = new Hashtable ();

	XmlDocument detached = new XmlDocument ();
	
	void PopulateClass (Tree tree, string ns, Node ns_node, string file)
	{
		XmlDocument doc = new XmlDocument ();
		doc.Load (file);
		
		string name = EcmaDoc.GetClassName (doc);
		string assembly = EcmaDoc.GetClassAssembly (doc);
		string kind = EcmaDoc.GetTypeKind (doc);
		string full = EcmaDoc.GetFullClassName (doc);

		Node class_node;
		string file_code = ns_node.tree.HelpSource.PackFile (file);

		XmlNode class_summary = detached.ImportNode (doc.SelectSingleNode ("/Type/Docs/summary"), true);
		ArrayList l = (ArrayList) class_summaries [ns];
		if (l == null){
			l = new ArrayList ();
			class_summaries [ns] = (object) l;
		}
		l.Add (new TypeInfo (kind, assembly, full, name, class_summary));
	       
		class_node = ns_node.LookupNode (String.Format ("{0} {1}", name, kind), "ecma:" + file_code + "#" + name + "/");
		
		if (kind == "Delegate") {
			if (doc.SelectSingleNode("/Type/ReturnValue") == null)
				tree.HelpSource.Message (TraceLevel.Error, "Delegate " + name + " does not have a ReturnValue node.  See the ECMA-style updates.");
		}

		if (kind == "Enumeration")
			return;

		if (kind == "Delegate")
			return;
		
		//
		// Always add the Members node
		//
		class_node.CreateNode ("Members", "*");

		PopulateMember (doc, name, class_node, "Constructor", "Constructors");
		PopulateMember (doc, name, class_node, "Method", "Methods");
		PopulateMember (doc, name, class_node, "Property", "Properties");
		PopulateMember (doc, name, class_node, "Field", "Fields");
		PopulateMember (doc, name, class_node, "Event", "Events");
		PopulateMember (doc, name, class_node, "Operator", "Operators");
	}

	class NodeIndex {
		public XmlNode node;
		public int     index;

		public NodeIndex (XmlNode node, int index)
		{
			this.node = node;
			this.index = index;
		}
	}

	struct NodeCompare : IComparer {
		public int Compare (object a, object b)
		{
			NodeIndex na = (NodeIndex) a;
			NodeIndex nb = (NodeIndex) b;

			return String.Compare (na.node.Attributes ["MemberName"].InnerText,
					       nb.node.Attributes ["MemberName"].InnerText);
		}
	}

	string GetMemberName (XmlNode node)
	{
		return node.Attributes ["MemberName"].InnerText;
	}
	
	//
	// Performs an XPath query on the document to extract the nodes for the various members
	// we also use some extra text to pluralize the caption
	//
	void PopulateMember (XmlDocument doc, string typename, Node node, string type, string caption)
	{
		string select = type;
		if (select == "Operator") select = "Method";
		
		XmlNodeList list1 = doc.SelectNodes (String.Format ("/Type/Members/Member[MemberType=\"{0}\"]", select));
		ArrayList list = new ArrayList();
		int i = 0;
		foreach (XmlElement n in list1) {
			n.SetAttribute("assembler_index", (i++).ToString());
			if (type == "Method" && GetMemberName(n).StartsWith("op_")) continue;
			if (type == "Operator" && !GetMemberName(n).StartsWith("op_")) continue;
			list.Add(n);
		}
		
		int count = list.Count;
		
		if (count == 0)
			return;

		Node nodes_node;
		string key = type.Substring (0, 1);
		nodes_node = node.CreateNode (caption, key);
		
		switch (type) {
			case "Event":
			case "Field":
				foreach (XmlElement n in list)
					nodes_node.CreateNode (GetMemberName (n), n.GetAttribute("assembler_index"));
				break;

			case "Constructor":
				foreach (XmlElement n in list)
					nodes_node.CreateNode (EcmaHelpSource.MakeSignature(n, typename), n.GetAttribute("assembler_index"));
				break;

			case "Property": // properties with indexers can be overloaded too
			case "Method":
			case "Operator":
				foreach (XmlElement n in list) {
					bool multiple = false;
					foreach (XmlNode nn in list) {
						if (n != nn && GetMemberName(n) == nn.Attributes ["MemberName"].InnerText) {
							multiple = true;
							break;
						}
					}
					
					string group, name, sig;
					if (type != "Operator") {
						name = GetMemberName(n);
						sig = EcmaHelpSource.MakeSignature(n, null);
						group = name;
					} else {
						EcmaHelpSource.MakeOperatorSignature(n, out name, out sig);
						group = name;
					}
					
					if (multiple) {
						nodes_node.LookupNode (group, group)
							.CreateNode (sig, n.GetAttribute("assembler_index"));
					} else {
						nodes_node.CreateNode (name, n.GetAttribute("assembler_index"));
					}
				}
				
				foreach (Node n in nodes_node.Nodes) {
					if (!n.IsLeaf)
						n.Sort ();
				}
				
				break;
				
			default:
				throw new InvalidOperationException();
		}
		
		nodes_node.Sort ();
	}

}

//
// The Ecma HelpSource provider
//
public class EcmaHelpSource : HelpSource {

	public EcmaHelpSource (string base_file, bool create) : base (base_file, create)
	{
		ExtObject = new ExtensionObject (this);
	}

	public EcmaHelpSource ()
	{
		ExtObject = new ExtensionObject (this);
	}

	static string css_ecma;
	public static string css_ecma_code {
		get {
			if (css_ecma != null)
				return css_ecma;
			if (use_css) {
				System.Reflection.Assembly assembly = typeof(EcmaHelpSource).Assembly;
				Stream str_css = assembly.GetManifestResourceStream ("mono-ecma.css");
				css_ecma = (new StreamReader (str_css)).ReadToEnd();
			} else {
				css_ecma = String.Empty;
			}
			return css_ecma;
		}
	}

	public override string InlineCss {
		get {return base.InlineCss + css_ecma_code;}
	}

	static string js;
	public static string js_code {
		get {
			if (js != null)
				return js;
			if (use_css) {
				System.Reflection.Assembly assembly = typeof(EcmaHelpSource).Assembly;
				Stream str_js = assembly.GetManifestResourceStream ("helper.js");
				js = (new StreamReader (str_js)).ReadToEnd();
			} else {
				js = String.Empty;
			}
			return js;
		}
	}

	public override string InlineJavaScript {
		get {return js_code + base.InlineJavaScript;}
	}

	public override string GetPublicUrl (string url)
	{
		if (url == null || url.Length == 0)
			return url;
		try {
			string rest;
			XmlDocument d = GetXmlFromUrl (url, out rest);
			if (rest == "")
				return EcmaDoc.GetCref (d.DocumentElement);
			XmlElement e = GetDocElement (d, rest);
			if (e == null)
				return EcmaDoc.GetCref (d.DocumentElement) + "/" + rest;
			return EcmaDoc.GetCref (e);
		}
		catch (Exception e) {
			return url;
		}
	}

	private static XmlElement GetDocElement (XmlDocument d, string rest)
	{
		string memberType = null;
		string memberIndex = null;

		string [] nodes = rest.Split (new char [] {'/'});
		
		switch (nodes.Length) {
			// e.g. C; not supported.
			case 1:
				return null;
			// e.g. C/0 or M/MethodName; the latter isn't supported.
			case 2:
				try {
					// XPath wants 1-based indexes, while the url uses 0-based values.
					memberIndex = (int.Parse (nodes [1]) + 1).ToString ();
					memberType  = GetMemberType (nodes [0]);
				} catch {
					return null;
				}
				break;
			// e.g. M/MethodName/0
			case 3:
				memberIndex = (int.Parse (nodes [2]) + 1).ToString ();
				memberType  = GetMemberType (nodes [0]);
				break;
			// not supported
			default:
				return null;
		}
		string xpath = "/Type/Members/Member[MemberType=\"" + memberType + "\"]" + 
				"[position()=" + memberIndex + "]";
		return (XmlElement) d.SelectSingleNode (xpath);
	}

	private static string GetMemberType (string type)
	{
		switch (type) {
			case "C": return "Constructor";
			case "E": return "Event";
			case "F": return "Field";
			case "M": return "Method";
			case "P": return "Property";
			default:
				throw new NotSupportedException ("Member Type: '" + type + "'.");
		}
	}

	public override string GetText (string url, out Node match_node)
	{
		match_node = null;

		string cached = GetCachedText (url);
		if (cached != null)
			return cached;
		
		if (url == "root:")
		{
			XmlReader summary = GetHelpXml ("mastersummary.xml");
			if (summary == null)
				return null;

			XsltArgumentList args = new XsltArgumentList();
			args.AddExtensionObject("monodoc:///extensions", ExtObject);
			args.AddParam("show", "", "masteroverview");
			string s = Htmlize(summary, args);
			return BuildHtml (css_ecma_code, js_code, s); 
		}
		
		if (url.StartsWith ("ecma:")) {
			string s = GetTextFromUrl (url);
			return BuildHtml (css_ecma_code, js_code, s); 
		}

		return null;
	}


	string RenderMemberLookup (string typename, string member, ref Node type_node)
	{
		if (type_node.Nodes == null)
			return null;

		string membername = member;
		string[] argtypes = null;
		if (member.IndexOf("(") > 0) {
			membername = membername.Substring(0, member.IndexOf("("));
			member = member.Replace("@", "&");
			
			// reform the member signature with CTS names

			string x = member.Substring(member.IndexOf("(")+1);
			argtypes = x.Substring(0, x.Length-1).Split(',', ':'); // operator signatures have colons

			if (membername == ".ctor")
				membername = typename;

			member = membername + "(";
			for (int i = 0; i < argtypes.Length; i++) {
				argtypes[i] = EcmaDoc.ConvertCTSName(argtypes[i]);
				if (i > 0) member += ",";
				member += argtypes[i];
			}
			member += ")";
		}
		
		// Check if a node caption matches exactly
		
		bool isoperator = false;
		
		if ((membername == "op_Implicit" || membername == "op_Explicit") && argtypes.Length == 2) {
			isoperator = true;
			membername = "Conversion";
			member = argtypes[0] + " to " + argtypes[1];
		} else if (membername.StartsWith("op_")) {
			isoperator = true;
			membername = membername.Substring(3);
		}

		foreach (Node x in type_node.Nodes){
			if (x.Nodes == null)
				continue;
			if (isoperator && x.Caption != "Operators")
				continue;
			
			foreach (Node m in x.Nodes) {
				string caption = m.Caption;
				string ecaption = ToEscapedMemberName (caption);
				if (m.IsLeaf) {
					// No overloading (usually), is just the member name.  The whole thing for constructors.
					if (caption == membername || caption == member ||
							ecaption == membername || ecaption == member) {
						type_node = m;
						return GetTextFromUrl (m.URL);
					}
				} else if (caption == member || ecaption == member) {
					// Though there are overloads, no arguments are in the url, so use this base node
					type_node = m;
					return GetTextFromUrl (m.URL);
				} else {
					// Check subnodes which are the overloads -- must match signature
					foreach (Node mm in m.Nodes) {
						ecaption = ToEscapedTypeName (mm.Caption);
						if (mm.Caption == member || ecaption == member) {
							type_node = mm;
							return GetTextFromUrl (mm.URL);
						}
					}
				}
			}
		}
		
		return null;
	}

	public static string MakeSignature (XmlNode n, string cstyleclass)
	{
		string sig;

		if (cstyleclass == null)
			sig = n.Attributes["MemberName"].InnerText;
		else {
			// constructor style
			sig = cstyleclass;
		}
	
		/*if (n.SelectSingleNode("MemberType").InnerText == "Method" || n.SelectSingleNode("MemberType").InnerText == "Constructor") {*/ // properties with indexers too
			XmlNodeList paramnodes = n.SelectNodes("Parameters/Parameter");
			sig += "(";
			bool first = true;
			foreach (XmlNode p in paramnodes) {
				if (!first) sig += ",";
				string type = p.Attributes["Type"].InnerText;
				type = EcmaDoc.ConvertCTSName(type);
				sig += type;
				first = false;
			}
			sig += ")";
		//}

		return sig;
	}
	
	public static void MakeOperatorSignature (XmlNode n, out string nicename, out string sig)
	{
		string name;

		name = n.Attributes["MemberName"].InnerText;
		nicename = name.Substring(3);
		
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
				sig = nicename;
				break;
			
			// binary operators: overloading is possible [ECMA-335 §10.3.2]
			case "op_Addition":                     // static    R operator+    (T1, T2)
			case "op_Subtraction":                  // static    R operator-    (T1, T2)
			case "op_Multiply":                     // static    R operator*    (T1, T2)
			case "op_Division":                     // static    R operator/    (T1, T2)
			case "op_Modulus":                      // static    R operator%    (T1, T2)
			case "op_ExclusiveOr":                  // static    R operator^    (T1, T2)
			case "op_BitwiseAnd":                   // static    R operator&    (T1, T2)
			case "op_BitwiseOr":                    // static    R operator|    (T1, T2)
			case "op_LogicalAnd":                   // static    R operator&&   (T1, T2)
			case "op_LogicalOr":                    // static    R operator||   (T1, T2)
			case "op_Assign":                       // static    R operator=    (T1, T2)
			case "op_LeftShift":                    // static    R operator<<   (T1, T2)
			case "op_RightShift":                   // static    R operator>>   (T1, T2)
			case "op_SignedRightShift":             // static    R operator>>   (T1, T2)
			case "op_UnsignedRightShift":           // static    R operator>>>  (T1, T2)
			case "op_Equality":                     // static bool operator==   (T1, T2)
			case "op_GreaterThan":                  // static bool operator>    (T1, T2)
			case "op_LessThan":                     // static bool operator<    (T1, T2)
			case "op_Inequality":                   // static bool operator!=   (T1, T2)
			case "op_GreaterThanOrEqual":           // static bool operator>=   (T1, T2)
			case "op_LessThanOrEqual":              // static bool operator<=   (T1, T2)
			case "op_UnsignedRightShiftAssignment": // static    R operator>>>= (T1, T2)
			case "op_MemberSelection":              // static    R operator->   (T1, T2)
			case "op_RightShiftAssignment":         // static    R operator>>=  (T1, T2)
			case "op_MultiplicationAssignment":     // static    R operator*=   (T1, T2)
			case "op_PointerToMemberSelection":     // static    R operator->*  (T1, T2)
			case "op_SubtractionAssignment":        // static    R operator-=   (T1, T2)
			case "op_ExclusiveOrAssignment":        // static    R operator^=   (T1, T2)
			case "op_LeftShiftAssignment":          // static    R operator<<=  (T1, T2)
			case "op_ModulusAssignment":            // static    R operator%=   (T1, T2)
			case "op_AdditionAssignment":           // static    R operator+=   (T1, T2)
			case "op_BitwiseAndAssignment":         // static    R operator&=   (T1, T2)
			case "op_BitwiseOrAssignment":          // static    R operator|=   (T1, T2)
			case "op_Comma":                        // static    R operator,    (T1, T2)
			case "op_DivisionAssignment":           // static    R operator/=   (T1, T2)
			default:                                // If all else fails, assume it can be overridden...whatever it is.
				XmlNodeList paramnodes = n.SelectNodes("Parameters/Parameter");
				sig = nicename + "(";
				bool first = true;
				foreach (XmlNode p in paramnodes) {
					if (!first) sig += ",";
					string type = p.Attributes["Type"].InnerText;
					type = EcmaDoc.ConvertCTSName(type);
					sig += type;
					first = false;
				}
				sig += ")";
				break;
			
			// conversion operators: overloading based on parameter and return type [ECMA-335 §10.3.3]
			case "op_Implicit":                    // static implicit operator R (T)
			case "op_Explicit":                    // static explicit operator R (T)
				nicename = "Conversion";
				string arg = n.SelectSingleNode("Parameters/Parameter/@Type").InnerText;
				string ret = n.SelectSingleNode("ReturnValue/ReturnType").InnerText;
				sig = EcmaDoc.ConvertCTSName(arg) + " to " + EcmaDoc.ConvertCTSName(ret);
				break;
		}	
	}

	//
	// This routine has to perform a lookup on a type.
	//
	// Example: T:System.Text.StringBuilder
	//
	// The prefix is the kind of opereation being requested (T:, E:, M: etc)
	// ns is the namespace being looked up
	// type is the type being requested
	//
	// This has to walk our toplevel (which is always a namespace)
	// And then the type space, and then walk further down depending on the request
	//
	public override string RenderTypeLookup (string prefix, string ns, string type, string member, out Node match_node)
	{
		string url = GetUrlForType (prefix, ns, type, member, out match_node);
		if (url == null) return null;
		return GetTextFromUrl (url);
	}

	public virtual string GetIdFromUrl (string prefix, string ns, string type)
	{
		Node tmp_node = new Node (Tree, "", "");
		string url = GetUrlForType (prefix, ns, type, null, out tmp_node);
		if (url == null) return null;
		return GetFile (url.Substring (5), out url);
	}
	
	public string GetUrlForType (string prefix, string ns, string type, string member, out Node match_node)
	{
		if (!prefix.EndsWith(":"))
			throw new ArgumentException("prefix");

		if (member != null)
			member = member.Replace ("#", ".").Replace ("{", "<").Replace ("}", ">");
			
		// If a nested type, compare only inner type name to node list.
		// This should be removed when the node list doesn't lose the containing type name.
		type = ToEscapedTypeName (type.Replace("+", "."));
		MatchAttempt[] attempts = GetAttempts (ns, type);

		foreach (Node ns_node in Tree.Nodes){
			string ns_node_namespace = ns_node.Element.Substring (2);

			if (!MatchesNamespace (attempts, ns_node_namespace, out type))
				continue;
			
			foreach (Node type_node in ns_node.Nodes){
				string element = type_node.Element;
				
				string cname;
				if (element.StartsWith("T:")) {
					cname = element.Substring(2);
					string _ns;
					RootTree.GetNamespaceAndType (cname, out _ns, out cname);
					cname = ToEscapedTypeName (cname);
					int pidx = cname.LastIndexOf (".");
					cname = cname.Substring(pidx+1);
					pidx = cname.LastIndexOf ("/");
					if (pidx != -1)
						cname = cname.Substring(0, pidx);
					cname = cname.Replace("+", ".");
				} else {				
					int pidx = element.IndexOf ("#");
					int sidx = element.IndexOf ("/");
					cname = element.Substring (pidx + 1, sidx-pidx-1);
					cname = ToEscapedTypeName (cname);
				}
				
				//Console.WriteLine ("t:{0} cn:{1} p:{2}", type, cname, prefix);

				if (type == cname && prefix == "T:") {
					match_node = type_node;
					return type_node.URL;
				} else if (type.StartsWith (cname)){
					int p = cname.Length;

					match_node = type_node;
					if (type == cname){
						string ret = RenderMemberLookup (type, member, ref match_node);
						if (ret == null)
							return type_node.URL;
						return match_node.URL;

					} else if (type [p] == '/'){
						//
						// This handles summaries
						//
						match_node = null;
						foreach (Node nd in type_node.Nodes) {
							if (nd.Element [nd.Element.Length - 1] == type [p + 1]) {
								match_node = nd;
								break;
							}
						}
						
						string ret = type_node.URL;
						if (!ret.EndsWith("/")) ret += "/";
						return ret + type.Substring (p + 1);
					}
				}
			}
		}

		match_node = null;
		return null;
	}

	struct MatchAttempt {
		public string Namespace;
		public string Type;

		public MatchAttempt (string ns, string t)
		{
			Namespace = ns;
			Type = t;
		}
	}

	private static MatchAttempt[] GetAttempts (string ns, string type)
	{
		MatchAttempt[] attempts = new MatchAttempt [Count (ns, '.') + 1];
		int wns = 0;
		for (int i = 0; i < ns.Length; ++i) {
			if (ns [i] == '.')
				attempts [wns++] = new MatchAttempt (ns.Substring (0, i), 
						ns.Substring (i+1) + "." + type);
		}
		attempts [wns++] = new MatchAttempt (ns, type);
		return attempts;
	}

	private static int Count (string s, char c)
	{
		int n = 0;
		for (int i = 0; i < s.Length; ++i)
			if (s [i] == c)
				++n;
		return n;
	}

	private static bool MatchesNamespace (MatchAttempt[] attempts, string ns, out string type)
	{
		for (int i = 0; i < attempts.Length; ++i)
			if (ns == attempts [i].Namespace) {
				type = attempts [i].Type;
				return true;
			}
		type = null;
		return false;
	}
	
	public static string ToEscapedTypeName (string typename)
	{
		return ToEscapedName (typename, "`");
	}

	static string ToEscapedName (string name, string escape)
	{
		StringBuilder filename = new StringBuilder (name.Length);
		int numArgs = 0;
		int numLt = 0;
		bool copy = true;

		for (int i = 0; i < name.Length; ++i) {
			char c = name [i];
			switch (c) {
				case '{':
				case '<':
					copy = false;
					++numLt;
					break;
				case '}':
				case '>':
					--numLt;
					if (numLt == 0) {
						filename.Append (escape).Append ((numArgs+1).ToString());
						numArgs = 0;
						copy = true;
					}
					break;
				case ',':
					if (numLt == 1)
						++numArgs;
					break;
				default:
					if (copy)
						filename.Append (c);
					break;
			}
		}
		return filename.ToString ();
	}

	static string ToEscapedMemberName (string membername)
	{
		return ToEscapedName (membername, "``");
	}
	
	public override string GetNodeXPath (XPathNavigator n)
	{
		if (n.Matches ("/Type/Docs/param")) {
			string type_name = (string) n.Evaluate ("string (ancestor::Type/@FullName)");
			string param_name = (string) n.Evaluate ("string (@name)");
			
			return String.Format ("/Type [@FullName = '{0}']/Docs/param[@name='{1}']", type_name, param_name);
		}

		if (n.Matches ("/Type/Docs/*")) {
			string type_name = (string) n.Evaluate ("string (ancestor::Type/@FullName)");
			
			return String.Format ("/Type [@FullName = '{0}']/Docs/{1}", type_name, n.Name);
		}
		
		if (n.Matches ("/Type/Members/Member/Docs/*")) {
			string type_name = (string) n.Evaluate ("string (ancestor::Type/@FullName)");
			string member_name = (string) n.Evaluate ("string (ancestor::Member/@MemberName)");
			string member_sig = (string) n.Evaluate ("string (ancestor::Member/MemberSignature [@Language='C#']/@Value)");
			string param_name = (string) n.Evaluate ("string (@name)");
			
			if (param_name == null || param_name == "") {
				return String.Format (
				"/Type [@FullName = '{0}']/Members/Member [@MemberName = '{1}'][MemberSignature [@Language='C#']/@Value = '{2}']/Docs/{3}",
				type_name, member_name, member_sig, n.Name);
			} else {
				return String.Format (
				"/Type [@FullName = '{0}']/Members/Member [@MemberName = '{1}'][MemberSignature [@Language='C#']/@Value = '{2}']/Docs/param [@name = '{3}']",
				type_name, member_name, member_sig, param_name);
			}
		}
		
		Message (TraceLevel.Warning, "WARNING: Was not able to get clean XPath expression for node {0}", EditingUtils.GetXPath (n));
		return base.GetNodeXPath (n);
	}

	protected virtual XmlDocument GetNamespaceDocument (string ns)
	{
		return GetHelpXmlWithChanges ("xml.summary." + ns);
	}

	public override string RenderNamespaceLookup (string nsurl, out Node match_node)
	{
		foreach (Node ns_node in Tree.Nodes){
			if (ns_node.Element != nsurl)
				continue;

			match_node = ns_node;
			string ns_name = nsurl.Substring (2);
			
			XmlDocument doc = GetNamespaceDocument (ns_name);
			if (doc == null)
				return null;

			XsltArgumentList args = new XsltArgumentList();
			args.AddExtensionObject("monodoc:///extensions", ExtObject);
			args.AddParam("show", "", "namespace");
			args.AddParam("namespace", "", ns_name);
			string s = Htmlize(new XmlNodeReader (doc), args);
			return BuildHtml (css_ecma_code, js_code, s); 

		}
		match_node = null;
		return null;
	}

	private string SelectString(XmlNode node, string xpath) {
		XmlNode ret = node.SelectSingleNode(xpath);
		if (ret == null) return "";
		return ret.InnerText;
	}
	private int SelectCount(XmlNode node, string xpath) {
		return node.SelectNodes(xpath).Count;
	}

	//
	// Returns the XmlDocument from the given url, and fills in `rest'
	//
	protected virtual XmlDocument GetXmlFromUrl(string url, out string rest) {
		// Remove ecma:
		url = url.Substring (5);
		string file = GetFile (url, out rest);

		// Console.WriteLine ("Got [{0}] and [{1}]", file, rest);
		return GetHelpXmlWithChanges (file);
	}
	
	string GetTextFromUrl (string url)
	{
		if (nozip) {
			string path = XmlDocUtils.GetCachedFileName (base_dir, url);
			if (File.Exists (path))
				return File.OpenText (path).ReadToEnd ();
			return null;
		}

		string rest, rest2;
		Node node;

		XmlDocument doc = GetXmlFromUrl (url, out rest);
		
		// Load base-type information so the stylesheet can draw the inheritance
		// tree and (optionally) include inherited members in the members list.
		XmlElement basenode = (XmlElement)doc.SelectSingleNode("Type/Base");
		XmlElement membersnode = (XmlElement)doc.SelectSingleNode("Type/Members");
		XmlNode basetype = doc.SelectSingleNode("Type/Base/BaseTypeName");
		int baseindex = 0;
		while (basetype != null) {
			// Add the ParentType node to Type/Base
			XmlElement inheritancenode = doc.CreateElement("ParentType");
			inheritancenode.SetAttribute("Type", basetype.InnerText);
			inheritancenode.SetAttribute("Order", (baseindex++).ToString());
			basenode.AppendChild(inheritancenode);
			
			// Load the base type XML data
			int dot = basetype.InnerText.LastIndexOf('.');
			string ns = basetype.InnerText.Substring(0, dot == -1 ? 0 : dot);
			string n = basetype.InnerText.Substring(dot == -1 ? 0 : dot+1);
			string basetypeurl = GetUrlForType("T:", ns, n, null, out node);
			XmlDocument basetypedoc = null;
			if (basetypeurl != null)
				basetypedoc = GetXmlFromUrl (basetypeurl, out rest2);
			if (basetypedoc == null) {
				inheritancenode.SetAttribute("Incomplete", "1");
				break;
			}
			
			if (SettingsHandler.Settings.ShowInheritedMembers) {
				// Add inherited members
				foreach (XmlElement member in basetypedoc.SelectNodes("Type/Members/Member[not(MemberType='Constructor')]")) {
					string sig = SelectString(member, "MemberSignature[@Language='C#']/@Value");
					if (sig.IndexOf(" static ") >= 0) continue;
					
					// If the signature of member matches the signature of a member already in the XML,
					// don't add it.
					string xpath = "@MemberName='" + SelectString(member, "@MemberName") + "'";
					xpath       += " and ReturnValue/ReturnType='" + SelectString(member, "ReturnValue/ReturnType") + "'";
					xpath       += " and count(Parameters/Parameter)=" + SelectCount(member, "Parameters/Parameter") + "";
					int pi = 1;
					foreach (XmlElement p in member.SelectNodes("Parameters/Parameter")) {
						xpath   += " and Parameters/Parameter[position()=" + pi + "]/@Type = '" + p.GetAttribute("Type") + "'";
						pi++;
					}
					
					// If a signature match is found, don't add.
					XmlNode match = membersnode.SelectSingleNode("Member[" + xpath + "]");
					if (match != null)
						continue;
					
					member.SetAttribute("DeclaredIn", basetype.InnerText);
					membersnode.AppendChild(membersnode.OwnerDocument.ImportNode(member, true));				
				}
			}
			
			basetype = basetypedoc.SelectSingleNode("Type/Base/BaseTypeName");
		}
		ArrayList extensions = new ArrayList ();
		AddExtensionMethodsFromHelpSource (extensions, this);
		foreach (HelpSource hs in RootTree.HelpSources) {
			EcmaHelpSource es = hs as EcmaHelpSource;
			if (es == null)
				continue;
			if (es == this)
				continue;
			AddExtensionMethodsFromHelpSource (extensions, es);
		}
		XmlDocUtils.AddExtensionMethods (doc, extensions, delegate (string s) {
				s = s.StartsWith ("T:") ? s : "T:" + s;
				return RootTree.GetHelpXml (s);
		});

		XsltArgumentList args = new XsltArgumentList();

		args.AddExtensionObject("monodoc:///extensions", ExtObject);
		
		if (rest == "") {
			args.AddParam("show", "", "typeoverview");
			string s = Htmlize(new XmlNodeReader (doc), args);
			return BuildHtml (css_ecma_code, js_code, s); 
		}
		
		string [] nodes = rest.Split (new char [] {'/'});
		
		switch (nodes.Length) {
			case 1:
				args.AddParam("show", "", "members");
				args.AddParam("index", "", "all");
				break;
			case 2:
				// Could either be a single member, or an overload thingy
				try {
					int.Parse (nodes [1]); // is it an int
					
					args.AddParam("show", "", "member");
					args.AddParam("index", "", nodes [1]);
				} catch {
					args.AddParam("show", "", "overloads");
					args.AddParam("index", "", nodes [1]);
				}
				break;
			case 3:
				args.AddParam("show", "", "member");
				args.AddParam("index", "", nodes [2]);
				break;
			default:
				return "What the hell is this URL " + url;
		}

		switch (nodes [0]){
		case "C":
			args.AddParam("membertype", "", "Constructor");
			break;
		case "M":
			args.AddParam("membertype", "", "Method");
			break;
		case "P":
			args.AddParam("membertype", "", "Property");
			break;
		case "F":
			args.AddParam("membertype", "", "Field");
			break;
		case "E":
			args.AddParam("membertype", "", "Event");
			break;
		case "O":
			args.AddParam("membertype", "", "Operator");
			break;
		case "X":
			args.AddParam("membertype", "", "ExtensionMethod");
			break;
		case "*":
			args.AddParam("membertype", "", "All");
			break;
		default:
			return "Unknown url: " + url;
		}

		string html = Htmlize(new XmlNodeReader (doc), args);
		return BuildHtml (css_ecma_code, js_code, html); 
	}

	void AddExtensionMethodsFromHelpSource (ArrayList extensions, EcmaHelpSource es)
	{
		Stream s = es.GetHelpStream ("ExtensionMethods.xml");
		if (s != null) {
			XmlDocument d = new XmlDocument ();
			d.Load (s);
			foreach (XmlNode n in d.SelectNodes ("/ExtensionMethods/*")) {
				extensions.Add (n);
			}
		}
	}

	
	public override void RenderPreviewDocs (XmlNode newNode, XmlWriter writer)
	{
		XsltArgumentList args = new XsltArgumentList ();
		args.AddExtensionObject ("monodoc:///extensions", ExtObject);
		
		Htmlize (new XmlNodeReader (newNode), args, writer);
	}

	static XslCompiledTransform ecma_transform;

	public string Htmlize (XmlReader ecma_xml)
	{
		return Htmlize(ecma_xml, null);
	}

	public string Htmlize (XmlReader ecma_xml, XsltArgumentList args)
	{
		EnsureTransform ();
		
		var output = new StringBuilder ();
		ecma_transform.Transform (ecma_xml, 
				args, 
				XmlWriter.Create (output, ecma_transform.OutputSettings),
				CreateDocumentResolver ());
		return output.ToString ();
	}

	protected virtual XmlResolver CreateDocumentResolver ()
	{
		// results in using XmlUrlResolver
		return null;
	}
	
	static void Htmlize (XmlReader ecma_xml, XsltArgumentList args, XmlWriter w)
	{
		EnsureTransform ();
		
		if (ecma_xml == null)
			return;

		ecma_transform.Transform (ecma_xml, args, w, null);
	}
	
	static XslCompiledTransform ecma_transform_css, ecma_transform_no_css;
	static void EnsureTransform ()
	{
		if (ecma_transform == null) {
			ecma_transform_css = new XslCompiledTransform ();
			ecma_transform_no_css = new XslCompiledTransform ();
			Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
			
			Stream stream = assembly.GetManifestResourceStream ("mono-ecma-css.xsl");
			XmlReader xml_reader = new XmlTextReader (stream);
			XmlResolver r = new ManifestResourceResolver (".");
			ecma_transform_css.Load (xml_reader, XsltSettings.TrustedXslt, r);
			
			stream = assembly.GetManifestResourceStream ("mono-ecma.xsl");
			xml_reader = new XmlTextReader (stream);
			ecma_transform_no_css.Load (xml_reader, XsltSettings.TrustedXslt, r);
		}
		if (use_css)
			ecma_transform = ecma_transform_css;
		else
			ecma_transform = ecma_transform_no_css;
	}

	// This ExtensionObject stuff is used to check at run time whether
	// types and members have been implemented and whether there are any
	// MonoTODO attributes left on them. 

	public readonly ExtensionObject ExtObject;
	public class ExtensionObject {
		readonly EcmaHelpSource hs;

		//
		// We are setting this to quiet now, as we need to transition
		// monodoc to run with the 2.x runtime and provide accurate
		// information in those cases.
		//
		bool quiet = true;
		
		public ExtensionObject (EcmaHelpSource hs)
		{
			this.hs = hs;
		}
		
		public string Colorize(string code, string lang) {
			return(Mono.Utilities.Colorizer.Colorize(code,lang));
		}
		// Used by stylesheet to nicely reformat the <see cref=> tags. 
		public string MakeNiceSignature(string sig, string contexttype)
		{
			if (sig.Length < 3)
				return sig;
			if (sig[1] != ':')
				return sig;

			char s = sig[0];
			sig = sig.Substring(2);
			
			switch (s) {
				case 'N': return sig;
				case 'T': return ShortTypeName(sig, contexttype);

				case 'C': case 'M': case 'P': case 'F': case 'E':
					string type, mem, arg;
					
					// Get arguments
					int paren;
					if (s == 'C' || s == 'M')
						paren = sig.IndexOf("(");
					else if (s == 'P')
						paren = sig.IndexOf("[");
					else
						paren = 0;
					
					if (paren > 0 && paren < sig.Length-1) {
						string[] args = sig.Substring(paren+1, sig.Length-paren-2).Split(',');						
						for (int i = 0; i < args.Length; i++)
							args[i] = ShortTypeName(args[i], contexttype);
						arg = "(" + String.Join(", ", args) + ")";
						sig = sig.Substring(0, paren); 
					} else {
						arg = "";
					}

					// Get type and member names
					int dot = sig.LastIndexOf(".");
					if (s == 'C' || dot <= 0 || dot == sig.Length-1) {
						mem = "";
						type = sig;
					} else {
						type = sig.Substring(0, dot);
						mem = sig.Substring(dot);
					}
						
					type = ShortTypeName(type, contexttype);
					
					return type + mem + arg;

				default:
					return sig;
			}
		}
		
		public string EditUrl (XPathNodeIterator itr)
		{
			if (itr.MoveNext ())
				return hs.GetEditUri (itr.Current);
			
			return "";
		}

		public string EditUrlNamespace (XPathNodeIterator itr, string ns, string section)
		{
			if (hs is EcmaUncompiledHelpSource)
				return "edit:file:" + Path.Combine(((EcmaUncompiledHelpSource)hs).BasePath, ns + ".xml") + "@/Namespace/Docs/" + section; 
			else if (itr.MoveNext ())
				return EditingUtils.FormatEditUri(itr.Current.BaseURI, "/elements/" + section);
			return "";
		}

		private static string ShortTypeName(string name, string contexttype)
		{
			int dot = contexttype.LastIndexOf(".");
			if (dot < 0) return name;
			string contextns = contexttype.Substring(0, dot+1);

			if (name == contexttype)
				return name.Substring(dot+1);
			
			if (name.StartsWith(contextns))
				return name.Substring(contextns.Length);
			
			return name.Replace("+", ".");
		}

		public string MonoImpInfo(string assemblyname, string typename, string membername, string arglist, bool strlong)
		{
			if (quiet)
				return "";
				
			ArrayList a = new ArrayList();
			if (arglist != "") a.Add(arglist);
			return MonoImpInfo(assemblyname, typename, membername, a, strlong);
		}

		public string MonoImpInfo(string assemblyname, string typename, string membername, XPathNodeIterator itr, bool strlong)
		{
			if (quiet)
				return "";
				
			ArrayList rgs = new ArrayList ();
			while (itr.MoveNext ())
				rgs.Add (itr.Current.Value);
			
			return MonoImpInfo (assemblyname, typename, membername, rgs, strlong);
		}
		
		public string MonoImpInfo(string assemblyname, string typename, string membername, ArrayList arglist, bool strlong)
		{
			try {
				Assembly assembly = null;
				
				try {
					assembly = Assembly.LoadWithPartialName(assemblyname);
				} catch (Exception) {
					// nothing.
				}
				
				if (assembly == null) {
					/*if (strlong) return "The assembly " + assemblyname + " is not available to MonoDoc.";
					else return "";*/
					return ""; // silently ignore
				}

				Type t = assembly.GetType(typename, false);
				if (t == null) {
					if (strlong)
						return typename + " has not been implemented.";
					else
						return "Not implemented.";
				}

				// The following code is flakey and fails to find existing members
				return "";
#if false
				MemberInfo[] mis = t.GetMember(membername, BF.Static | BF.Instance | BF.Public | BF.NonPublic);

				if (mis.Length == 0)
					return "This member has not been implemented.";
				if (mis.Length == 1)
					return MonoImpInfo(mis[0], "member", strlong);

				// Scan for the appropriate member
				foreach (MemberInfo mi in mis) {
					System.Reflection.ParameterInfo[] pis;

					if (mi is MethodInfo || mi is ConstructorInfo)
						pis = ((MethodBase)mi).GetParameters();
					else if (mi is PropertyInfo)
						pis = ((PropertyInfo)mi).GetIndexParameters();
					else
						pis = null;
					
					if (pis != null) {
						bool good = true;
						if (pis.Length != arglist.Count) continue;
						for (int i = 0; i < pis.Length; i++) {
							if (pis[i].ParameterType.FullName != (string)arglist[i]) { good = false; break; }
						}
						if (!good) continue;
					}

					return MonoImpInfo(mi, "member", strlong);
				}
#endif
			} catch (Exception) {
				return "";
			}
		}
		
		public string MonoImpInfo(System.Reflection.MemberInfo mi, string itemtype, bool strlong)
		{
			if (quiet)
				return "";
				
			string s = "";

			object[] atts = mi.GetCustomAttributes(true);
			int todoctr = 0;
			foreach (object att in atts) if (att.GetType().Name == "MonoTODOAttribute") todoctr++;

			if (todoctr > 0) {
				if (strlong)
					s = "This " + itemtype + " is marked as being unfinished.<BR/>\n";
				else 
					s = "Unfinished.";
			}

			return s;
		}

		public string MonoImpInfo(string assemblyname, string typename, bool strlong)
		{
			if (quiet)
				return "";
				
			try {
				if (assemblyname == "")
					return "";

				Assembly assembly = Assembly.LoadWithPartialName(assemblyname);
				if (assembly == null)
					return "";

				Type t = assembly.GetType(typename, false);
				if (t == null) {
					if (strlong)
						return typename + " has not been implemented.";
					else
						return "Not implemented.";
				}

				string s = MonoImpInfo(t, "type", strlong);

				if (strlong) {
					MemberInfo[] mis = t.GetMembers(BF.Static | BF.Instance | BF.Public | BF.NonPublic);

					// Scan members for MonoTODO attributes
					int mctr = 0;
					foreach (MemberInfo mi in mis) {
						string mii = MonoImpInfo(mi, null, false);
						if (mii != "") mctr++; 
					}
					if (mctr > 0) {
						s += "This type has " + mctr + " members that are marked as unfinished.<BR/>";
					}
				}

				return s;

			} catch (Exception) {
				return "";
			}			
		}

		public bool MonoEditing ()
		{
			return SettingsHandler.Settings.EnableEditing;
		}
		
		public bool IsToBeAdded(string text) {
			return text.StartsWith("To be added");
		}
	}

	//
	// This takes one of the ecma urls, which look like this:
	// ecma:NUMERIC_ID#OPAQUE/REST
	//
	// NUMERIC_ID is the numeric ID assigned by the compressor
	// OPAQUE is opaque for node rendering (it typically contains T:System.Byte for example)
	// REST is the rest of the argument used to decode information
	//
	static string GetFile (string url, out string rest)
	{
		int pound = url.IndexOf ("#");
		int slash = url.IndexOf ("/");
		
		string fname = url.Substring (0, pound);
		rest = url.Substring (slash+1);

		return fname;
	}

#if false
	// This should have a little cache or something.
	static XmlDocument GetDocument (HelpSource hs, string fname)
	{
		Stream s = hs.GetHelpStream (fname);
		if (s == null){
			Error ("Could not fetch document {0}", fname);
			return null;
		}
		
		XmlDocument doc = new XmlDocument ();

		doc.Load (s);
		
		return doc;
	}
#endif
	
	string GetKindFromCaption (string s)
	{
		int p = s.LastIndexOf (' ');
		if (p > 0)
			return s.Substring (p + 1);
		return null;
	}
	
	//
	// Obtain an URL of the type T:System.Object from the node
	// 
	public static string GetNiceUrl (Node node) {
		if (node.Element.StartsWith("N:"))
			return node.Element;
		string name, full;
		int bk_pos = node.Caption.IndexOf (' ');
		// node from an overview
		if (bk_pos != -1) {
			name = node.Caption.Substring (0, bk_pos);
			full = node.Parent.Caption + "." + name.Replace ('.', '+');
			return "T:" + full;
		}
		// node that lists constructors, methods, fields, ...
		if ((node.Caption == "Constructors") || (node.Caption == "Fields") || (node.Caption == "Events") 
			|| (node.Caption == "Members") || (node.Caption == "Properties") || (node.Caption == "Methods")
			|| (node.Caption == "Operators")) {
			bk_pos = node.Parent.Caption.IndexOf (' ');
			name = node.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			return "T:" + full + "/" + node.Element; 
		}
		int pr_pos = node.Caption.IndexOf ('(');
		// node from a constructor
		if (node.Parent.Element == "C") {
			name = node.Parent.Parent.Parent.Caption;
			int idx = node.URL.IndexOf ('/');
			return node.URL[idx+1] + ":" + name + "." + node.Caption.Replace ('.', '+');
		// node from a method with one signature, field, property, operator
		} else if (pr_pos == -1) {
			bk_pos = node.Parent.Parent.Caption.IndexOf (' ');
			name = node.Parent.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			int idx = node.URL.IndexOf ('/');
			return node.URL[idx+1] + ":" + full + "." + node.Caption;
		// node from a method with several signatures
		} else {
			bk_pos = node.Parent.Parent.Parent.Caption.IndexOf (' ');
			name = node.Parent.Parent.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			int idx = node.URL.IndexOf ('/');
			return node.URL[idx+1] + ":" + full + "." + node.Caption;
		}
	}
				
	//
	// Populates the index.
	//
	public override void PopulateIndex (IndexMaker index_maker)
	{
		foreach (Node ns_node in Tree.Nodes){
			foreach (Node type_node in ns_node.Nodes){
				string typename = type_node.Caption.Substring (0, type_node.Caption.IndexOf (' '));
				string full = ns_node.Caption + "." + typename;

				string doc_tag = GetKindFromCaption (type_node.Caption);
				string url = "T:" + full;
					
				if (doc_tag == "Class" || doc_tag == "Structure" || doc_tag == "Interface"){

					index_maker.Add (type_node.Caption, typename, url);
					index_maker.Add (full + " " + doc_tag, full, url);

					foreach (Node c in type_node.Nodes){
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
					
					foreach (Node c in type_node.Nodes){
						switch (c.Caption){
						case "Constructors":
							break;
						case "Fields":
							foreach (Node nc in c.Nodes){
								string res = nc.Caption;

								string nurl = String.Format ("F:{0}.{1}", full, res);
								index_maker.Add (String.Format ("{0}.{1} field", typename, res),
										 keybase + res, nurl);
								index_maker.Add (String.Format ("{0} field", res), res, nurl);
							}

							break;
						case "Events":
							foreach (Node nc in c.Nodes){
								string res = nc.Caption;
								string nurl = String.Format ("E:{0}.{1}", full, res);
								
								index_maker.Add (String.Format ("{0}.{1} event", typename, res),
										 keybase + res, nurl);
								index_maker.Add (String.Format ("{0} event", res), res, nurl);
							}
							break;
						case "Properties":
							foreach (Node nc in c.Nodes){
								string res = nc.Caption;
								string nurl = String.Format ("P:{0}.{1}", full, res);
								index_maker.Add (String.Format ("{0}.{1} property", typename, res),
										 keybase + res, nurl);
								index_maker.Add (String.Format ("{0} property", res), res, nurl);
							}
							break;
						case "Methods":
							foreach (Node nc in c.Nodes){
								string res = nc.Caption;
								int p = res.IndexOf ("(");
								if (p > 0)
									res = res.Substring (0, p); 
								string nurl = String.Format ("M:{0}.{1}", full, res);
								index_maker.Add (String.Format ("{0}.{1} method", typename, res),
										 keybase + res, nurl);
								index_maker.Add (String.Format ("{0} method", res), res, nurl);
							}
					
							break;
						case "Operators":
							foreach (Node nc in c.Nodes){
								string res = nc.Caption;
								string nurl = String.Format ("O:{0}.{1}", full, res);
								index_maker.Add (String.Format ("{0}.{1} operator", typename, res),
										 keybase + res, nurl);
							}
							break;
						}
					}
				} else if (doc_tag == "Enumeration"){
					//
					// Enumerations: add the enumeration values
					//
					index_maker.Add (type_node.Caption, typename, url);
					index_maker.Add (full + " " + doc_tag, full, url);

					// Now, pull the values.
					string rest;
					XmlDocument x = GetXmlFromUrl (type_node.URL, out rest);
					if (x == null)
						continue;
					
					XmlNodeList members = x.SelectNodes ("/Type/Members/Member");

					if (members == null)
						continue;

					foreach (XmlNode member_node in members){
						string enum_value = member_node.Attributes ["MemberName"].InnerText;
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
	//
	// Create list of documents for searching
	//
	public override void PopulateSearchableIndex (IndexWriter writer)
	{
		StringBuilder text;
		foreach (Node ns_node in Tree.Nodes) {
			Message (TraceLevel.Info, "\tNamespace: {0} ({1})", ns_node.Caption, ns_node.Nodes.Count);
			foreach (Node type_node in ns_node.Nodes) {
				string typename = type_node.Caption.Substring (0, type_node.Caption.IndexOf (' '));
				string full = ns_node.Caption + "." + typename;
				string doc_tag = GetKindFromCaption (type_node.Caption);
				string url = "T:" + full;
				string rest;
				XmlDocument xdoc = GetXmlFromUrl (type_node.URL, out rest);
				if (xdoc == null)
					continue;
				
				// 
				// For classes, structures or interfaces add a doc for the overview and
				// add a doc for every constructor, method, event, ...
				// 
				if (doc_tag == "Class" || doc_tag == "Structure" || doc_tag == "Interface"){
					
					// Adds a doc for every overview of every type
					SearchableDocument doc = new SearchableDocument ();
					doc.title = type_node.Caption;
					doc.hottext = typename;
					doc.url = url;
					
					XmlNode node_sel = xdoc.SelectSingleNode ("/Type/Docs");
					text  = new StringBuilder ();
					GetTextFromNode (node_sel, text);
					doc.text = text.ToString ();

					text  = new StringBuilder ();
					GetExamples (node_sel, text);
					doc.examples = text.ToString ();
					
					writer.AddDocument (doc.LuceneDoc);

					//Add docs for contructors, methods, etc.
					foreach (Node c in type_node.Nodes) { // c = Constructors || Fields || Events || Properties || Methods || Operators
						
						if (c.Element == "*")
							continue;
						int i = 1;
						foreach (Node nc in c.Nodes) {
							//xpath to the docs xml node
							string xpath;
							if (c.Caption == "Constructors")
								xpath = String.Format ("/Type/Members/Member[{0}]/Docs", i++);
							else if (c.Caption == "Operators")
								xpath = String.Format ("/Type/Members/Member[@MemberName='op_{0}']/Docs", nc.Caption);
							else
								xpath = String.Format ("/Type/Members/Member[@MemberName='{0}']/Docs", nc.Caption);
							//construct url of the form M:Array.Sort
							string urlnc;
							if (c.Caption == "Constructors")
								urlnc = String.Format ("{0}:{1}.{2}", c.Caption[0], ns_node.Caption, nc.Caption);
							else
								urlnc = String.Format ("{0}:{1}.{2}.{3}", c.Caption[0], ns_node.Caption, typename, nc.Caption);

							//create the doc
							SearchableDocument doc_nod = new SearchableDocument ();
							doc_nod.title = LargeName (nc);
							//dont add the parameters to the hottext
							int ppos = nc.Caption.IndexOf ('(');
							if (ppos != -1)
								doc_nod.hottext =  nc.Caption.Substring (0, ppos);
							else
								doc_nod.hottext = nc.Caption;

							doc_nod.url = urlnc;

							XmlNode xmln = xdoc.SelectSingleNode (xpath);
							if (xmln == null) {
								Error ("Problem: {0}, with xpath: {1}", urlnc, xpath);
								continue;
							}

							text = new StringBuilder ();
							GetTextFromNode (xmln, text);
							doc_nod.text = text.ToString ();

							text = new StringBuilder ();
							GetExamples (xmln, text);
							doc_nod.examples = text.ToString ();

							writer.AddDocument (doc_nod.LuceneDoc);
						}
					}
				//
				// Enumerations: add the enumeration values
				//
				} else if (doc_tag == "Enumeration"){
										
					XmlNodeList members = xdoc.SelectNodes ("/Type/Members/Member");
					if (members == null)
						continue;

					text = new StringBuilder ();
					foreach (XmlNode member_node in members) {
						string enum_value = member_node.Attributes ["MemberName"].InnerText;
						text.Append (enum_value);
						text.Append (" ");
						GetTextFromNode (member_node["Docs"], text);
						text.Append ("\n");
					}
					SearchableDocument doc = new SearchableDocument ();

					text = new StringBuilder ();
					GetExamples (xdoc.SelectSingleNode ("/Type/Docs"), text);
					doc.examples = text.ToString ();

					doc.title = type_node.Caption;
					doc.hottext = xdoc.DocumentElement.Attributes["Name"].Value;
					doc.url = url;
					doc.text = text.ToString();
					writer.AddDocument (doc.LuceneDoc);
				//
				// Add delegates
				//
				} else if (doc_tag == "Delegate"){
					SearchableDocument doc = new SearchableDocument ();
					doc.title = type_node.Caption;
					doc.hottext = xdoc.DocumentElement.Attributes["Name"].Value;
					doc.url = url; 
					
					XmlNode node_sel = xdoc.SelectSingleNode ("/Type/Docs");

					text = new StringBuilder ();
					GetTextFromNode (node_sel, text);
					doc.text = text.ToString();

					text = new StringBuilder ();
					GetExamples (node_sel, text);
					doc.examples = text.ToString();

					writer.AddDocument (doc.LuceneDoc);
				} 
			}
		}
	}
	
	//
	// Extract the interesting text from the docs node
	//
	void GetTextFromNode (XmlNode n, StringBuilder sb) 
	{
		//don't include example code
		if (n.Name == "code") 
			return;

		//include the url to which points the see tag
		if (n.Name == "see" && n.Attributes.Count > 0)
				sb.Append (n.Attributes [0].Value);
		
		//include the name of the parameter
		if (n.Name == "paramref" && n.Attributes.Count > 0)
			sb.Append (n.Attributes [0].Value);

		//include the contents for the node that contains text
		if (n.NodeType == XmlNodeType.Text)
			sb.Append (n.Value);
		
		//add the rest of xml tags recursively
		if (n.HasChildNodes)
			foreach (XmlNode n_child in n.ChildNodes)
				GetTextFromNode (n_child, sb);
	}
	//
	// Extract the code nodes from the docs
	//
	void GetExamples (XmlNode n, StringBuilder sb)
	{
		if (n.Name == "code") {
			sb.Append (n.InnerText);
		} else {
			if (n.HasChildNodes)
				foreach (XmlNode n_child in n.ChildNodes)
					GetExamples (n_child, sb);
		}
	}
	//
	// Extract a large name for the Node
	//  (copied from mono-tools/docbrowser/browser.Render()
	static string LargeName (Node matched_node)
	{
		string[] parts = matched_node.URL.Split('/', '#');			
		if(parts.Length == 3 && parts[2] != String.Empty) { //List of Members, properties, events, ...
			return parts[1] + ": " + matched_node.Caption;
		} else if(parts.Length >= 4) { //Showing a concrete Member, property, ...					
			return parts[1] + "." + matched_node.Caption;
		} else {
			return matched_node.Caption;
		}
	}

}

public class EcmaUncompiledHelpSource : EcmaHelpSource {
	readonly DirectoryInfo basedir;
	readonly XmlDocument basedoc;
	
	public new readonly string Name;
	public     readonly string BasePath;
	
	public EcmaUncompiledHelpSource (string base_file) : base ()
	{
		Message (TraceLevel.Info, "Loading uncompiled help from " + base_file);
		
		basedir = new DirectoryInfo(base_file);
		BasePath = basedir.FullName;
		
		basedoc = new XmlDocument();
		basedoc.Load(Path.Combine(basedir.FullName, "index.xml"));
		
		Name = basedoc.SelectSingleNode("Overview/Title").InnerText;
		
		bool has_content = false;
		
		foreach (XmlElement ns in basedoc.SelectNodes("Overview/Types/Namespace")) {
			has_content = true;
			Node nsnode = Tree.CreateNode(ns.GetAttribute("Name"), "N:" + ns.GetAttribute("Name"));
			
			bool has_types = false;
			foreach (XmlElement t in ns.SelectNodes("Type")) {
				has_types = true;
				string typename = EcmaDoc.GetDisplayName (t).Replace("+", ".");
				
				// Must load in each document to get the list of members...
				XmlDocument typedoc = new XmlDocument();
				typedoc.Load(Path.Combine(Path.Combine(basedir.FullName, ns.GetAttribute("Name")), t.GetAttribute("Name") + ".xml"));
				string kind = EcmaDoc.GetTypeKind (typedoc);
				
				string url = ns.GetAttribute("Name") + "." + t.GetAttribute("Name");
				Node typenode = nsnode.CreateNode(typename + " " + kind, "T:" + url);				
				//Node typemembers = typenode.CreateNode("Members", "T:" + url + "/*");
				
				Hashtable groups = new Hashtable();
				Hashtable groups_count = new Hashtable();
				foreach (XmlElement member in typedoc.SelectNodes("Type/Members/Member")) {
					string membername = member.GetAttribute("MemberName");
					string membertype = member.SelectSingleNode("MemberType").InnerText;
					
					if (membertype == "Constructor")
						membername = t.GetAttribute("Name");
					if (membername.StartsWith("op_"))
						membertype = "Operator";
					
					Node group;
					if (groups.ContainsKey(membertype)) {
						group = (Node)groups[membertype];
					} else {
						string membertypeplural = membertype + "s";
						if (membertypeplural == "Propertys") membertypeplural = "Properties";
						
						group = typenode.CreateNode(membertypeplural, "T:" + url + "/" + membertype[0]);
						groups[membertype] = group;
						groups_count[membertype] = 0;
					}
					
					if (membertype == "Constructor" || membertype == "Method" || 
						(membertype == "Property" && member.SelectNodes("Parameters/Parameter").Count > 0)) {
						membername = EcmaHelpSource.MakeSignature(member, membertype == "Constructor" ? membername : null);
					} else if (membertype == "Operator") {
						string dummy;
						EcmaHelpSource.MakeOperatorSignature(member, out dummy, out membername);
					}
					
					int index = (int)groups_count[membertype];
					groups_count[membertype] = index + 1;
					
					group.CreateNode(membername, index.ToString());
				}

				foreach (Node group in groups.Values)
					group.Sort();			
			}
			
			if (has_types)
				nsnode.Sort();
		}
		
		if (has_content)
			Tree.Sort();
	}
	
	public override string GetIdFromUrl (string prefix, string ns, string type)
	{
		if (prefix != "T:")
			throw new NotImplementedException();
		return Path.Combine(Path.Combine(basedir.FullName, ns), type + ".xml");
	}

	protected override XmlDocument GetXmlFromUrl(string url, out string rest) {
		// strip off the T:
		url = url.Substring(2);
		
		int sidx = url.IndexOf("/");
		if (sidx == -1) {
			rest = "";
		} else {
			rest = url.Substring(sidx+1);
			url = url.Substring(0, sidx);
		}
		
		string ns, type;
		if (!RootTree.GetNamespaceAndType (url, out ns, out type)) {
			Message (TraceLevel.Error, "Could not determine namespace/type for {0}", url);
			return null;
		}
		
		string file = Path.Combine(Path.Combine(basedir.FullName, ns), 
				ToEscapedTypeName (type).Replace ('.', '+') + ".xml");
		if (!new FileInfo(file).Exists) return null;
		
		XmlDocument typedoc = new XmlDocument();
		typedoc.Load(file);
		return typedoc;
	}
	
	public override string GetText (string url, out Node match_node) {
		if (url == "root:") {
			match_node = null;
			
			//load index.xml
			XmlDocument index = new XmlDocument ();
			index.Load (Path.Combine (basedir.FullName, "index.xml"));
			XmlNodeList nodes = index.SelectNodes ("/Overview/Types/Namespace");
			
			//recreate masteroverview.xml
			XmlDocument summary = new XmlDocument ();
			XmlElement elements = summary.CreateElement ("elements");
			foreach (XmlNode node in nodes) {
				XmlElement ns = summary.CreateElement ("namespace");
				XmlAttribute attr = summary.CreateAttribute ("ns");
				attr.Value = EcmaDoc.GetDisplayName (node);
				ns.Attributes.Append (attr);
				elements.AppendChild (ns);
			}
			summary.AppendChild (elements);

			XmlReader reader = new XmlTextReader (new StringReader (summary.OuterXml));

			//transform the recently created masteroverview.xml
			XsltArgumentList args = new XsltArgumentList();
			args.AddExtensionObject("monodoc:///extensions", ExtObject);
			args.AddParam("show", "", "masteroverview");
			string s = Htmlize(reader, args);
			return BuildHtml (css_ecma_code, js_code, s); 
		}
		return base.GetText(url, out match_node);
	}
	
	protected override XmlDocument GetNamespaceDocument (string ns)
	{
		XmlDocument nsdoc = new XmlDocument();
		nsdoc.Load (EcmaDoc.GetNamespaceFile (basedir.FullName, ns));
		
		XmlDocument elements = new XmlDocument();
		XmlElement docnode = elements.CreateElement("elements");
		elements.AppendChild (docnode);
		
		foreach (XmlElement doc in nsdoc.SelectNodes("Namespace/Docs/*")) {
			docnode.AppendChild(elements.ImportNode(doc, true));
		}
				
		foreach (XmlElement t in basedoc.SelectNodes("Overview/Types/Namespace[@Name='" + ns + "']/Type")) {
			XmlDocument typedoc = new XmlDocument();
			typedoc.Load(Path.Combine(Path.Combine(basedir.FullName, ns), t.GetAttribute("Name") + ".xml"));
			
			string typekind;
			switch (EcmaDoc.GetTypeKind(typedoc)) {
			case "Class": typekind = "class"; break;
			case "Enumeration": typekind = "enum"; break;
			case "Structure": typekind = "struct"; break;
			case "Delegate": typekind = "delegate"; break;
			case "Interface": typekind = "interface"; break;
			default: throw new InvalidOperationException();
			}
			
			XmlElement typenode = elements.CreateElement(typekind);
			typenode.SetAttribute("name", EcmaDoc.GetDisplayName (t).Replace ('+', '.'));
			typenode.SetAttribute("fullname", ns + "." + t.GetAttribute("Name"));
			typenode.AppendChild(elements.ImportNode(typedoc.SelectSingleNode("Type/Docs/summary"), true));
			
			docnode.AppendChild(typenode);
		}

		return elements;
	}
	
	public override Stream GetHelpStream (string id)
	{
		if (id == "ExtensionMethods.xml") {
			// TODO: generate ExtensionMethods.xml based on index.xml contents.
		}
		return null;
	}

	public override XmlDocument GetHelpXmlWithChanges (string id)
	{
		XmlDocument doc = new XmlDocument ();
		doc.Load (id);
		return doc;
	}
	
	class UncompiledResolver : XmlResolver {
		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			return null;
		}

		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			return null;
		}

		public override System.Net.ICredentials Credentials {
			set {/* ignore */}
		}
	}

	protected override XmlResolver CreateDocumentResolver ()
	{
		return new UncompiledResolver ();
	}
}

}


