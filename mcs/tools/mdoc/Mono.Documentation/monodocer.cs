// Updater program for syncing Mono's ECMA-style documentation files
// with an assembly.
// By Joshua Tauberer <tauberer@for.net>

using System;
using System.Collections;
#if !NET_1_0
using System.Collections.Generic;
#endif
using System.Globalization;
using System.IO;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

#if NET_1_0
using Mono.GetOptions;

using MemberInfoEnumerable = System.Collections.IEnumerable;
using MyXmlNodeList        = System.Collections.ArrayList;
using StringList           = System.Collections.ArrayList;
using StringToStringMap    = System.Collections.Hashtable;
using StringToXmlNodeMap   = System.Collections.Hashtable;
#else
using Mono.Options;

using MemberInfoEnumerable = System.Collections.Generic.IEnumerable<System.Reflection.MemberInfo>;
using MyXmlNodeList        = System.Collections.Generic.List<System.Xml.XmlNode>;
using StringList           = System.Collections.Generic.List<string>;
using StringToStringMap    = System.Collections.Generic.Dictionary<string, string>;
using StringToXmlNodeMap   = System.Collections.Generic.Dictionary<string, System.Xml.XmlNode>;
#endif

namespace Mono.Documentation {

#pragma warning disable 0618
class MDocUpdaterOptions 
#if NET_1_0
	: Options
#endif
{
#if NET_1_0
	[Option("The root {directory} of an assembly's documentation files.")]
#endif
	public string path = null;

#if NET_1_0
	[Option("When updating documentation, write the updated files to this {path}.")]
#endif
	public string updateto = null;

#if NET_1_0
	[Option(-1, "The assembly to document.  Specify a {file} path or the name of a GAC'd assembly.")]
#endif
	public string[] assembly = null;

#if NET_1_0
	[Option(-1, "Document only the {type name}d by this argument.")]
#endif
	public string[] type = null;

#if NET_1_0
	[Option("Update only the types in this {namespace}.")]
#endif
	public string @namespace = null;

#if NET_1_0
	[Option("Allow monodocer to delete members from files.")]
#endif
	public bool delete = false;

#if NET_1_0
	[Option("Include overridden methods in documentation.")]
#endif
	public bool overrides = true;

#if NET_1_0
	[Option("Don't update members.")]
#endif
	public bool ignoremembers = false;

#if NET_1_0
	[Option("Don't rename documentation XML files for missing types.  IGNORED.")]
#endif
	public bool ignore_extra_docs = false;

#if NET_1_0
	[Option("The {name} of the project this documentation is for.")]
#endif
	public string name;

#if NET_1_0
	[Option("An XML documentation {file} made by the /doc option of mcs/csc the contents of which will be imported.")]
	public string importslashdoc;
	
	[Option("An ECMA or monodoc-generated XML documemntation {file} to import.")]
	public string importecmadoc;
#endif

#if NET_1_0
	[Option("Import either a /doc or ECMA documentation file.")]
#endif
	public string import;

#if NET_1_0
	[Option("Indent the XML files nicely.")]
#endif
	public bool pretty = true;
	
#if NET_1_0
	[Option("Create a <since/> element for added types/members with the value {since}.")]
#endif
	public string since;

#if NET_1_0
	[Option("Show full stack trace on error.")]
#endif
	public bool show_exceptions;
}
#pragma warning restore

class MDocUpdater 
#if !NET_1_0
	: MDocCommand
#endif
{
	
	static string srcPath;
	static Assembly[] assemblies;
	
	static bool nooverrides = true, delete = false, ignoremembers = false;
	static bool pretty = false;
	static bool show_exceptions = false;
	
	static int additions = 0, deletions = 0;

	static string name;
	static XmlDocument slashdocs;
	static XmlReader ecmadocs;

	static string since;

	static MemberFormatter csharpFullFormatter  = new CSharpFullMemberFormatter ();
	static MemberFormatter csharpFormatter      = new CSharpMemberFormatter ();
	static MemberFormatter docTypeFormatter     = new DocTypeMemberFormatter ();
	static MemberFormatter slashdocFormatter    = new SlashDocMemberFormatter ();
	static MemberFormatter filenameFormatter    = new FileNameMemberFormatter ();

	static MyXmlNodeList extensionMethods = new MyXmlNodeList ();

	const BindingFlags DefaultBindingFlags = 
		BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
	
#if NET_1_0
	public static void Main(string[] args)
	{
		MDocUpdaterOptions opts = new MDocUpdaterOptions ();
		opts.ProcessArgs(args);

		if (args.Length == 0) {
			opts.DoHelp();
			return;
		}
		Run (opts);
	}
#else
	public override void Run (IEnumerable<string> args)
	{
		var opts = new MDocUpdaterOptions {
			overrides        = true,
			pretty           = true,
			show_exceptions  = DebugOutput,
		};

		var  types = new List<string> ();
		var p = new OptionSet () {
			{ "o|out=",
				"Root {DIRECTORY} to generate/update documentation.",
				v => opts.path = v },
			{ "i|import=", 
				"Import documentation from {FILE}.",
				v => opts.import = v },
			{ "delete",
				"Delete removed members from the XML files.",
				v => opts.delete = v != null },
			{ "since=",
				"Manually specify the assembly version that new members were added in.",
				v => opts.since = v },
			{ "type=",
			  "Only update documentation for {TYPE}.",
				v => types.Add (v) },
		};
		List<string> extra = Parse (p, args, "update", 
				"[OPTIONS]+ ASSEMBLIES",
				"Create or update documentation from ASSEMBLIES.");
		if (extra == null)
			return;
		if (extra.Count == 0)
			Error ("No assemblies specified.");
		opts.assembly = extra.ToArray ();
		if (types.Count > 0)
			opts.type = types.ToArray ();

		Run (opts);
		opts.name = ""; // remove warning about unused member
	}
#endif
		
	public static void Run (MDocUpdaterOptions opts)
	{
		nooverrides = !opts.overrides;
		delete = opts.delete;
		ignoremembers = opts.ignoremembers;
		name = opts.name;
		pretty = opts.pretty;
		since = opts.since;
		show_exceptions = opts.show_exceptions;

		try {
			// PARSE BASIC OPTIONS AND LOAD THE ASSEMBLY TO DOCUMENT
			
			if (opts.path == null)
				throw new InvalidOperationException("The path option is required.");
			
			srcPath = opts.path;

			if (opts.type != null && opts.type.Length > 0 && opts.@namespace != null)
				throw new InvalidOperationException("You cannot specify both 'type' and 'namespace'.");
			
			if (opts.assembly == null)
				throw new InvalidOperationException("The assembly option is required.");
				
			assemblies = new Assembly [opts.assembly.Length];
			for (int i = 0; i < opts.assembly.Length; i++)
				assemblies [i] = LoadAssembly (opts.assembly [i]);
				
			// IMPORT FROM /DOC?
			
#if NET_1_0
			if (opts.importslashdoc != null) {
				try {
					slashdocs = new XmlDocument();
					slashdocs.Load(opts.importslashdoc);
				} catch (Exception e) {
					Error ("Could not load /doc file: {0}", e.Message);
					Environment.ExitCode = 1;
					return;
				}
			}
			
			if (opts.importecmadoc != null) {
				try {
					ecmadocs = new XmlTextReader (opts.importecmadoc);
				} catch (Exception e) {
					Error ("Could not load ECMA XML file: {0}", e.Message);
					Environment.ExitCode = 1;
					return;
				}
			}
#endif

			if (opts.import != null && ecmadocs == null && slashdocs == null) {
				try {
					XmlReader r = new XmlTextReader (opts.import);
					if (r.Read ()) {
						while (r.NodeType != XmlNodeType.Element) {
							if (!r.Read ())
								throw new Exception ("Unable to read XML file: " + 
										opts.import);
						}
						if (r.LocalName == "doc") {
							slashdocs = new XmlDocument();
							slashdocs.Load (opts.import);
						}
						else if (r.LocalName == "Libraries") {
							ecmadocs = new XmlTextReader (opts.import);
						}
						else
							throw new Exception ("Unsupported XML format within " + opts.import);
					}
					r.Close ();
				} catch (Exception e) {
					Error ("Could not load XML file: {0}", e.Message);
					Environment.ExitCode = 1;
					return;
				}
			}
			
			// PERFORM THE UPDATES
			
			string dest_dir = opts.updateto != null ? opts.updateto : opts.path;
			if (opts.type != null && opts.type.Length > 0)
				DoUpdateTypes(opts.path, opts.type, dest_dir);
			else if (opts.@namespace != null)
				DoUpdateNS (opts.@namespace, Path.Combine (opts.path, opts.@namespace),
						Path.Combine (dest_dir, opts.@namespace));
			else
				DoUpdateAssemblies(opts.path, dest_dir);
		
		} catch (InvalidOperationException error) {
			Error (opts.show_exceptions ? error.ToString () : error.Message);
			Environment.ExitCode = 1;
			return;
			
		} catch (System.IO.IOException error) {
			Error (opts.show_exceptions ? error.ToString () : error.Message);
			Environment.ExitCode = 1;
			return;

		} catch (Exception error) {
			Error (opts.show_exceptions ? error.ToString () : error.Message);
			Environment.ExitCode = 1;
		}

		Console.WriteLine("Members Added: {0}, Members Deleted: {1}", additions, deletions);
	}

	private static 
#if !NET_1_0
		new 
#endif
		void Error (string format, params object[] args)
	{
		Console.Error.Write ("monodocer: ");
		Console.Error.WriteLine (format, args);
	}
	
	private static Assembly LoadAssembly (string name)
	{
		Assembly assembly = null;
		try {
			assembly = Assembly.LoadFile (name);
		} catch (System.IO.FileNotFoundException) { }

		if (assembly == null) {
			try {
#pragma warning disable 0612
				assembly = Assembly.LoadWithPartialName (name);
#pragma warning restore
			} catch (Exception) { }
		}
			
		if (assembly == null)
			throw new InvalidOperationException("Assembly " + name + " not found.");

		return assembly;
	}

	private static void WriteXml(XmlElement element, System.IO.TextWriter output) {
		OrderTypeAttributes (element);
		XmlTextWriter writer = new XmlTextWriter(output);
		writer.Formatting = Formatting.Indented;
		writer.Indentation = 2;
		writer.IndentChar = ' ';
		element.WriteTo(writer);
		output.WriteLine();	
	}

	private static void OrderTypeAttributes (XmlElement e)
	{
		foreach (XmlElement type in e.SelectNodes ("//Type")) {
			OrderTypeAttributes (type.Attributes);
		}
	}

	static readonly string[] TypeAttributeOrder = {
		"Name", "FullName", "FullNameSP", "Maintainer"
	};

	private static void OrderTypeAttributes (XmlAttributeCollection c)
	{
		XmlAttribute[] attrs = new XmlAttribute [TypeAttributeOrder.Length];
		for (int i = 0; i < c.Count; ++i) {
			XmlAttribute a = c [i];
			for (int j = 0; j < TypeAttributeOrder.Length; ++j) {
				if (a.Name == TypeAttributeOrder [j]) {
					attrs [j] = a;
					break;
				}
			}
		}
		for (int i = attrs.Length-1; i >= 0; --i) {
			XmlAttribute n = attrs [i];
			if (n == null)
				continue;
			XmlAttribute r = null;
			for (int j = i+1; j < attrs.Length; ++j) {
				if (attrs [j] != null) {
					r = attrs [j];
					break;
				}
			}
			if (r == null)
				continue;
			c.Remove (n);
			c.InsertBefore (n, r);
		}
	}
	
	private static XmlDocument CreateIndexStub() {
		XmlDocument index = new XmlDocument();

		XmlElement index_root = index.CreateElement("Overview");
		index.AppendChild(index_root);

		if (assemblies.Length == 0)
			throw new Exception ("No assembly");

		XmlElement index_assemblies = index.CreateElement("Assemblies");
		index_root.AppendChild(index_assemblies);

		XmlElement index_remarks = index.CreateElement("Remarks");
		index_remarks.InnerText = "To be added.";
		index_root.AppendChild(index_remarks);

		XmlElement index_copyright = index.CreateElement("Copyright");
		index_copyright.InnerText = "To be added.";
		index_root.AppendChild(index_copyright);

		XmlElement index_types = index.CreateElement("Types");
		index_root.AppendChild(index_types);
		
		return index;
	}
	
	private static void WriteNamespaceStub(string ns, string outdir) {
		XmlDocument index = new XmlDocument();

		XmlElement index_root = index.CreateElement("Namespace");
		index.AppendChild(index_root);
		
		index_root.SetAttribute("Name", ns);

		XmlElement index_docs = index.CreateElement("Docs");
		index_root.AppendChild(index_docs);

		XmlElement index_summary = index.CreateElement("summary");
		index_summary.InnerText = "To be added.";
		index_docs.AppendChild(index_summary);

		XmlElement index_remarks = index.CreateElement("remarks");
		index_remarks.InnerText = "To be added.";
		index_docs.AppendChild(index_remarks);

		using (TextWriter writer = OpenWrite (outdir + "/ns-" + ns + ".xml",  FileMode.CreateNew)) {
			WriteXml(index.DocumentElement, writer);
		}
	}

	public static void DoUpdateTypes(string basepath, string[] typenames, string dest) {
		ArrayList found = new ArrayList ();
		foreach (Assembly assembly in assemblies) {
			foreach (DocsTypeInfo docsTypeInfo in GetTypes (assembly, typenames)) {
				string relpath = DoUpdateType (docsTypeInfo.Type, basepath, dest, docsTypeInfo.EcmaDocs);
				if (relpath != null)
					found.Add (docsTypeInfo.Type.FullName);
			}
		}
		StringList notFound = new StringList (typenames.Length);
		foreach (string typename in typenames)
			if (!found.Contains (typename))
				notFound.Add (typename);
		if (notFound.Count > 0)
			throw new InvalidOperationException("Type(s) not found: " + string.Join (", ", DocUtils.ToStringArray (notFound)));
	}

	public static string DoUpdateType(Type type, string basepath, string dest, XmlReader ecmaDocsType)
	{
		if (type.Namespace == null)
			Error ("warning: The type `{0}' is in the root namespace.  This may cause problems with display within monodoc.",
					type.FullName);
		if (!IsPublic (type))
			return null;
		
		// Must get the A+B form of the type name.
		string typename = GetTypeFileName(type);
		
		string reltypefile = DocUtils.PathCombine (type.Namespace, typename + ".xml");
		string typefile = Path.Combine (basepath, reltypefile);
		System.IO.FileInfo file = new System.IO.FileInfo(typefile);

		string output = null;
		if (dest == null) {
			output = typefile;
		} else if (dest == "-") {
			output = null;
		} else {
			output = Path.Combine (dest, reltypefile);
		}	

		if (file.Exists) {
			// Update
			XmlDocument basefile = new XmlDocument();
			if (!pretty) basefile.PreserveWhitespace = true;
			try {
				basefile.Load(typefile);
			} catch (Exception e) {
				throw new InvalidOperationException("Error loading " + typefile + ": " + e.Message, e);
			}
			
			DoUpdateType2("Updating", basefile, type, output, false, ecmaDocsType);
		} else {
			// Stub
			XmlElement td = StubType(type, output, ecmaDocsType);
			if (td == null)
				return null;
			
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo (DocUtils.PathCombine (dest, type.Namespace));
			if (!dir.Exists) {
				dir.Create();
				Console.WriteLine("Namespace Directory Created: " + type.Namespace);
			}
		}
		return reltypefile;
	}

	private static XPathNavigator SelectSingleNode (XPathNavigator n, string xpath)
	{
#if !NET_1_0
		return n.SelectSingleNode (xpath);
#else
		XPathNodeIterator i = n.Select (xpath);
		XPathNavigator r = null;
		while (i.MoveNext ()) {
			r = i.Current;
		}
		return r;
#endif
	}

	public static void DoUpdateNS(string ns, string nspath, string outpath) {
		Hashtable seenTypes = new Hashtable();
		Assembly assembly = assemblies [0];
		
		foreach (System.IO.FileInfo file in new System.IO.DirectoryInfo(nspath).GetFiles("*.xml")) {
			XmlDocument basefile = new XmlDocument();
			if (!pretty) basefile.PreserveWhitespace = true;
			string typefile = Path.Combine(nspath, file.Name);
			try {
				basefile.Load(typefile);
			} catch (Exception e) {
				throw new InvalidOperationException("Error loading " + typefile + ": " + e.Message, e);
			}

			string typename = 
				GetTypeFileName (basefile.SelectSingleNode("Type/@FullName").InnerText);
			Type type = assembly.GetType(typename, false);
			if (type == null) {
				Error ("Type no longer in assembly: " + typename);
				continue;
			}			

			seenTypes[type] = seenTypes;
			DoUpdateType2("Updating", basefile, type, Path.Combine(outpath, file.Name), false, null);
		}
		
		// Stub types not in the directory
		foreach (DocsTypeInfo docsTypeInfo in GetTypes (assembly, null)) {
			Type type = docsTypeInfo.Type;
			if (type.Namespace != ns || seenTypes.ContainsKey(type))
				continue;

			XmlElement td = StubType(type, Path.Combine(outpath, GetTypeFileName(type) + ".xml"), docsTypeInfo.EcmaDocs);
			if (td == null) continue;
		}
	}
	
	private static string GetTypeFileName(Type type) {
		return filenameFormatter.GetName (type);
	}

	public static string GetTypeFileName (string typename)
	{
		StringBuilder filename = new StringBuilder (typename.Length);
		int numArgs = 0;
		int numLt = 0;
		bool copy = true;
		for (int i = 0; i < typename.Length; ++i) {
			char c = typename [i];
			switch (c) {
				case '<':
					copy = false;
					++numLt;
					break;
				case '>':
					--numLt;
					if (numLt == 0) {
						filename.Append ('`').Append ((numArgs+1).ToString());
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

	
	private static void AddIndexAssembly (Assembly assembly, XmlElement parent)
	{
		XmlElement index_assembly = parent.OwnerDocument.CreateElement("Assembly");
		index_assembly.SetAttribute("Name", assembly.GetName().Name);
		index_assembly.SetAttribute("Version", assembly.GetName().Version.ToString());
		MakeAttributes(index_assembly, assembly, true);
		parent.AppendChild(index_assembly);
	}

	private static void DoUpdateAssemblies (string source, string dest) 
	{
		string indexfile = dest + "/index.xml";
		XmlDocument index;
		if (System.IO.File.Exists(indexfile)) {
			index = new XmlDocument();
			index.Load(indexfile);

			// Format change
			ClearElement(index.DocumentElement, "Assembly");
			ClearElement(index.DocumentElement, "Attributes");
		} else {
			index = CreateIndexStub();
		}
		
		if (name == null) {
			string defaultTitle = "Untitled";
			if (assemblies.Length == 1)
				defaultTitle = assemblies[0].GetName().Name;
			WriteElementInitialText(index.DocumentElement, "Title", defaultTitle);
		} else {
			WriteElementText(index.DocumentElement, "Title", name);
		}
		
		XmlElement index_types = WriteElement(index.DocumentElement, "Types");
		XmlElement index_assemblies = WriteElement(index.DocumentElement, "Assemblies");
		index_assemblies.RemoveAll ();
		
		Hashtable goodfiles = new Hashtable();

		foreach (Assembly assm in assemblies) {
			AddIndexAssembly (assm, index_assemblies);
			DoUpdateAssembly (assm, index_types, source, dest, goodfiles);
		}

		SortIndexEntries (index_types);
		
		CleanupFiles (dest, goodfiles);
		CleanupIndexTypes (index_types, goodfiles);
		CleanupExtensions (index_types);

		using (TextWriter writer = OpenWrite (indexfile, FileMode.Create))
			WriteXml(index.DocumentElement, writer);
	}
		
	private static char[] InvalidFilenameChars = {'\\', '/', ':', '*', '?', '"', '<', '>', '|'};

	private static void DoUpdateAssembly (Assembly assembly, XmlElement index_types, string source, string dest, Hashtable goodfiles) 
	{
		foreach (DocsTypeInfo docTypeInfo in GetTypes (assembly, null)) {
			Type type = docTypeInfo.Type;
			if (!IsPublic (type) || type.FullName.IndexOfAny (InvalidFilenameChars) >= 0)
				continue;

			string reltypepath = DoUpdateType (type, source, dest, docTypeInfo.EcmaDocs);
			if (reltypepath == null)
				continue;
			
			// Add namespace and type nodes into the index file as needed
			XmlElement nsnode = (XmlElement)index_types.SelectSingleNode("Namespace[@Name='" + type.Namespace + "']");
			if (nsnode == null) {
				nsnode = index_types.OwnerDocument.CreateElement("Namespace");
				nsnode.SetAttribute("Name", type.Namespace);
				index_types.AppendChild(nsnode);
			}
			string typename = GetTypeFileName(type);
			string doc_typename = GetDocTypeName (type);
			XmlElement typenode = (XmlElement)nsnode.SelectSingleNode("Type[@Name='" + typename + "']");
			if (typenode == null) {
				typenode = index_types.OwnerDocument.CreateElement("Type");
				typenode.SetAttribute("Name", typename);
				nsnode.AppendChild(typenode);
			}
			if (typename != doc_typename)
				typenode.SetAttribute("DisplayName", doc_typename);
			else
				typenode.RemoveAttribute("DisplayName");
			typenode.SetAttribute ("Kind", GetTypeKind (type));
				
			// Ensure the namespace index file exists
			string onsdoc = DocUtils.PathCombine (dest, type.Namespace + ".xml");
			string nsdoc  = DocUtils.PathCombine (dest, "ns-" + type.Namespace + ".xml");
			if (File.Exists (onsdoc)) {
				File.Move (onsdoc, nsdoc);
			}

			if (!File.Exists (nsdoc)) {
				Console.WriteLine("New Namespace File: " + type.Namespace);
				WriteNamespaceStub(type.Namespace, dest);
			}

			// mark the file as corresponding to a type
			goodfiles[reltypepath] = goodfiles;
		}
	}

	class DocsTypeInfo {
		public Type Type;
		public XmlReader EcmaDocs;

		public DocsTypeInfo (Type type, XmlReader docs)
		{
			this.Type = type;
			this.EcmaDocs = docs;
		}
	}

	static 
#if NET_1_0
		IEnumerable
#else
		IEnumerable<Mono.Documentation.MDocUpdater.DocsTypeInfo>
#endif
		GetTypes (Assembly assembly, string[] forTypes)
	{
		Hashtable seen = null;
		if (forTypes != null)
			Array.Sort (forTypes);
		if (ecmadocs != null) {
			seen = new Hashtable ();
			int typeDepth = -1;
			while (ecmadocs.Read ()) {
				switch (ecmadocs.Name) {
					case "Type": {
						if (typeDepth == -1)
							typeDepth = ecmadocs.Depth;
						if (ecmadocs.NodeType != XmlNodeType.Element)
							continue;
						if (typeDepth != ecmadocs.Depth) // nested <Type/> element?
							continue;
						string typename = ecmadocs.GetAttribute ("FullName");
						string typename2 = GetTypeFileName (typename);
						if (forTypes != null && 
								Array.BinarySearch (forTypes, typename) < 0 &&
								typename != typename2 &&
								Array.BinarySearch (forTypes, typename2) < 0)
							continue;
						Type t;
						if ((t = assembly.GetType (typename, false)) == null &&
								(t = assembly.GetType (typename2, false)) == null)
							continue;
						seen.Add (typename, "");
						if (typename != typename2)
							seen.Add (typename2, "");
						Console.WriteLine ("  Import: {0}", t.FullName);
						yield return new DocsTypeInfo (t, ecmadocs);
						break;
					}
					default:
						break;
				}
			}
		}
		foreach (Type type in assembly.GetTypes()) {
			if (forTypes != null && Array.BinarySearch (forTypes, type.FullName) < 0)
				continue;
			if (seen != null && seen.ContainsKey (type.FullName))
				continue;
			yield return new DocsTypeInfo (type, null);
		}
	}

	private static void SortIndexEntries (XmlElement indexTypes)
	{
		XmlNodeList namespaces = indexTypes.SelectNodes ("Namespace");
		XmlNodeComparer c = new AttributeNameComparer ();
		SortXmlNodes (indexTypes, namespaces, c);

		for (int i = 0; i < namespaces.Count; ++i)
			SortXmlNodes (namespaces [i], namespaces [i].SelectNodes ("Type"), c);
	}

	private static void SortXmlNodes (XmlNode parent, XmlNodeList children, XmlNodeComparer comparer)
	{
		MyXmlNodeList l = new MyXmlNodeList (children.Count);
		for (int i = 0; i < children.Count; ++i)
			l.Add (children [i]);
		l.Sort (comparer);
		for (int i = l.Count - 1; i > 0; --i) {
			parent.InsertBefore (parent.RemoveChild ((XmlNode) l [i-1]), (XmlNode) l [i]);
		}
	}

	abstract class XmlNodeComparer : IComparer 
#if !NET_1_0
		, IComparer<XmlNode>
#endif
	{
		public abstract int Compare (XmlNode x, XmlNode y);

		public int Compare (object x, object y)
		{
			return Compare ((XmlNode) x, (XmlNode) y);
		}
	}

	class AttributeNameComparer : XmlNodeComparer {
		public override int Compare (XmlNode x, XmlNode y)
		{
			return x.Attributes ["Name"].Value.CompareTo (y.Attributes ["Name"].Value);
		}
	}
	
	class VersionComparer : XmlNodeComparer {
		public override int Compare (XmlNode x, XmlNode y)
		{
			// Some of the existing docs use e.g. 1.0.x.x, which Version doesn't like.
			string a = GetVersion (x.InnerText);
			string b = GetVersion (y.InnerText);
			return new Version (a).CompareTo (new Version (b));
		}

		static string GetVersion (string v)
		{
			int n = v.IndexOf ("x");
			if (n < 0)
				return v;
			return v.Substring (0, n-1);
		}
	}

	private static string GetTypeKind (Type type)
	{
		if (type.IsEnum)
			return "Enumeration";
		if (type.IsValueType)
			return "Structure";
		if (type.IsInterface)
			return "Interface";
		if (IsDelegate (type))
			return "Delegate";
		if (type.IsClass || type == typeof(System.Enum))
			return "Class";
		throw new ArgumentException ("Unknown kind for type: " + type.FullName);
	}

	private static bool IsPublic (Type type)
	{
		Type decl = type;
		while (decl != null) {
			if (!(decl.IsPublic || decl.IsNestedPublic)) {
				return false;
			}
			decl = decl.DeclaringType;
		}
		return true;
	}

	private static void CleanupFiles (string dest, Hashtable goodfiles)
	{
		// Look for files that no longer correspond to types
		foreach (System.IO.DirectoryInfo nsdir in new System.IO.DirectoryInfo(dest).GetDirectories("*")) {
			foreach (System.IO.FileInfo typefile in nsdir.GetFiles("*.xml")) {
				string relTypeFile = Path.Combine(nsdir.Name, typefile.Name);
				if (!goodfiles.ContainsKey(relTypeFile)) {
					XmlDocument doc = new XmlDocument ();
					doc.Load (typefile.FullName);
					XmlElement e = doc.SelectSingleNode("/Type") as XmlElement;
					if (UpdateAssemblyVersions(e, GetAssemblyVersions(), false)) {
						using (TextWriter writer = OpenWrite (typefile.FullName, FileMode.Truncate))
							WriteXml(doc.DocumentElement, writer);
						goodfiles [relTypeFile] = goodfiles;
						continue;
					}
					string newname = typefile.FullName + ".remove";
					try { System.IO.File.Delete(newname); } catch (Exception) { }
					try { typefile.MoveTo(newname); } catch (Exception) { }					
					Console.WriteLine("Class no longer present; file renamed: " + Path.Combine(nsdir.Name, typefile.Name));
				}
			}
		}
	}

	private static TextWriter OpenWrite (string path, FileMode mode)
	{
		return new StreamWriter (
			new FileStream (path, mode),
			new UTF8Encoding (false)
		);
	}

	private static string[] GetAssemblyVersions ()
	{
		StringList versions = new StringList (assemblies.Length);
		for (int i = 0; i < assemblies.Length; ++i)
			versions.Add (GetAssemblyVersion (assemblies [i]));
		return DocUtils.ToStringArray (versions);
	}
		
	private static void CleanupIndexTypes (XmlElement index_types, Hashtable goodfiles)
	{
		// Look for type nodes that no longer correspond to types
		MyXmlNodeList remove = new MyXmlNodeList ();
		foreach (XmlElement typenode in index_types.SelectNodes("Namespace/Type")) {
			string fulltypename = Path.Combine (((XmlElement)typenode.ParentNode).GetAttribute("Name"), typenode.GetAttribute("Name") + ".xml");
			if (!goodfiles.ContainsKey(fulltypename)) {
				remove.Add (typenode);
			}
		}
		foreach (XmlNode n in remove)
			n.ParentNode.RemoveChild (n);
	}

	private static void CleanupExtensions (XmlElement index_types)
	{
		XmlNode e = index_types.SelectSingleNode ("/Overview/ExtensionMethods");
		if (extensionMethods.Count == 0) {
			if (e == null)
				return;
			index_types.RemoveChild (e);
			return;
		}
		if (e == null) {
			e = index_types.OwnerDocument.CreateElement ("ExtensionMethods");
			index_types.SelectSingleNode ("/Overview").AppendChild (e);
		}
		else
			e.RemoveAll ();
		extensionMethods.Sort (DefaultExtensionMethodComparer);
		foreach (XmlNode m in extensionMethods) {
			e.AppendChild (index_types.OwnerDocument.ImportNode (m, true));
		}
	}

	class ExtensionMethodComparer : XmlNodeComparer {
		public override int Compare (XmlNode x, XmlNode y)
		{
			XmlNode xLink = x.SelectSingleNode ("Member/Link");
			XmlNode yLink = y.SelectSingleNode ("Member/Link");

			int n = xLink.Attributes ["Type"].Value.CompareTo (
					yLink.Attributes ["Type"].Value);
			if (n != 0)
				return n;
			n = xLink.Attributes ["Member"].Value.CompareTo (
					yLink.Attributes ["Member"].Value);
			if (n == 0 && !object.ReferenceEquals (x, y))
				throw new InvalidOperationException ("Duplicate extension method found!");
			return n;
		}
	}

	static readonly XmlNodeComparer DefaultExtensionMethodComparer = new ExtensionMethodComparer ();
		
	public static void DoUpdateType2(string message, XmlDocument basefile, Type type, string output, bool insertSince, XmlReader ecmaDocsType) {
		Console.WriteLine(message + ": " + type.FullName);
		
		StringToXmlNodeMap seenmembers = new StringToXmlNodeMap ();

		// Update type metadata
		UpdateType(basefile.DocumentElement, type, ecmaDocsType);

		if (ecmaDocsType != null) {
			while (ecmaDocsType.Name != "Members" && ecmaDocsType.Read ()) {
				// do nothing
			}
			if (ecmaDocsType.IsEmptyElement)
				ecmaDocsType = null;
		}

		// Update existing members.  Delete member nodes that no longer should be there,
		// and remember what members are already documented so we don't add them again.
		if (!ignoremembers) {
			MyXmlNodeList todelete = new MyXmlNodeList ();
			foreach (DocsNodeInfo info in GetDocumentationMembers (basefile, type, ecmaDocsType)) {
				XmlElement oldmember  = info.Node;
				MemberInfo oldmember2 = info.Member;
	 			string sig = oldmember2 != null ? MakeMemberSignature(oldmember2) : null;

				// Interface implementations and overrides are deleted from the docs
				// unless the overrides option is given.
				if (oldmember2 != null && (!IsNew(oldmember2) || sig == null))
					oldmember2 = null;
				
				// Deleted (or signature changed)
				if (oldmember2 == null) {
					if (UpdateAssemblyVersions (oldmember, new string[]{GetAssemblyVersion (type.Assembly)}, false))
						continue;
					DeleteMember ("Member Removed", output, oldmember, todelete);
					continue;
				}
				
				// Duplicated
				if (seenmembers.ContainsKey (sig)) {
					if (object.ReferenceEquals (oldmember, seenmembers [sig])) {
						// ignore, already seen
					}
					else if (DefaultMemberComparer.Compare (oldmember, seenmembers [sig]) == 0)
						DeleteMember ("Duplicate Member Found", output, oldmember, todelete);
					else
						Error ("TODO: found a duplicate member '{0}', but it's not identical to the prior member found!", sig);
					continue;
				}
				
				// Update signature information
				UpdateMember(info);
				
				seenmembers.Add (sig, oldmember);
			}
			foreach (XmlElement oldmember in todelete)
				oldmember.ParentNode.RemoveChild (oldmember);
		}
		
		if (!IsDelegate(type) && !ignoremembers) {
			XmlNode members = WriteElement (basefile.DocumentElement, "Members");
			foreach (MemberInfo m in Sort (type.GetMembers(DefaultBindingFlags))) {
				if (m is Type) continue;
				
				string sig = MakeMemberSignature(m);
				if (sig == null) continue;
				if (seenmembers.ContainsKey(sig)) continue;
				
				// To be nice on diffs, members/properties/events that are overrides or are interface implementations
				// are not added in.
				if (!IsNew(m)) continue;
				
				XmlElement mm = MakeMember(basefile, new DocsNodeInfo (null, m));
				if (mm == null) continue;
				members.AppendChild( mm );
	
				Console.WriteLine("Member Added: " + mm.SelectSingleNode("MemberSignature/@Value").InnerText);
				additions++;
			}
		}
		
		// Import code snippets from files
		foreach (XmlNode code in basefile.GetElementsByTagName("code")) {
			if (!(code is XmlElement)) continue;
			string file = ((XmlElement)code).GetAttribute("src");
			string lang = ((XmlElement)code).GetAttribute("lang");
			if (file != "") {
				string src = GetCodeSource (lang, Path.Combine (srcPath, file));
				if (src != null)
					code.InnerText = src;
			}
		}

		if (insertSince && since != null) {
			XmlNode docs = basefile.DocumentElement.SelectSingleNode("Docs");
			docs.AppendChild (CreateSinceNode (basefile));
		}

		do {
			XmlElement d = basefile.DocumentElement ["Docs"];
			XmlElement m = basefile.DocumentElement ["Members"];
			if (d != null && m != null)
				basefile.DocumentElement.InsertBefore (
						basefile.DocumentElement.RemoveChild (d), m);
			SortTypeMembers (m);
		} while (false);

		System.IO.TextWriter writer;
		if (output == null)
			writer = Console.Out;
		else {
			FileInfo file = new FileInfo (output);
			if (!file.Directory.Exists) {
				Console.WriteLine("Namespace Directory Created: " + type.Namespace);
				file.Directory.Create ();
			}
			writer = OpenWrite (output, FileMode.Create);
		}

		using (writer)
			WriteXml(basefile.DocumentElement, writer);
	}

	private static string GetCodeSource (string lang, string file)
	{
		int anchorStart;
		if (lang == "C#" && (anchorStart = file.IndexOf (".cs#")) >= 0) {
			// Grab the specified region
			string region = "#region " + file.Substring (anchorStart + 4);
			file          = file.Substring (0, anchorStart + 3);
			try {
				using (StreamReader reader = new StreamReader (file)) {
					string line;
					StringBuilder src = new StringBuilder ();
					int indent = -1;
					while ((line = reader.ReadLine ()) != null) {
						if (line.Trim() == region) {
							indent = line.IndexOf (region);
							continue;
						}
						if (indent >= 0 && line.Trim().StartsWith ("#endregion")) {
							break;
						}
						if (indent >= 0)
							src.Append (
									(line.Length > 0 ? line.Substring (indent) : string.Empty) +
									"\n");
					}
					return src.ToString ();
				}
			} catch (Exception e) {
				Error ("Could not load <code/> file '{0}' region '{1}': {2}",
						file, region, show_exceptions ? e.ToString () : e.Message);
				return null;
			}
		}
		try {
			using (StreamReader reader = new StreamReader (file))
				return reader.ReadToEnd ();
		} catch (Exception e) {
			Error ("Could not load <code/> file '" + file + "': " + e.Message);
		}
		return null;
	}

	private static 
#if NET_1_0
		IEnumerable
#else
		IEnumerable<DocsNodeInfo>
#endif
		GetDocumentationMembers (XmlDocument basefile, Type type, XmlReader ecmaDocsMembers)
	{
		if (ecmaDocsMembers != null) {
			int membersDepth = ecmaDocsMembers.Depth;
			bool go = true;
			while (go && ecmaDocsMembers.Read ()) {
				switch (ecmaDocsMembers.Name) {
					case "Member": {
						if (membersDepth != ecmaDocsMembers.Depth - 1 || ecmaDocsMembers.NodeType != XmlNodeType.Element)
							continue;
						DocumentationMember dm = new DocumentationMember (ecmaDocsMembers);
						string xp = GetXPathForMember (dm);
						XmlElement oldmember = (XmlElement) basefile.SelectSingleNode (xp);
						MemberInfo m;
						if (oldmember == null) {
							m = GetMember (type, dm);
							if (m == null) {
								Error ("Could not import ECMA docs for `{0}'s `{1}': MemberInfo not found.",
										type.FullName, dm.MemberSignatures ["C#"]);
										// SelectSingleNode (ecmaDocsMember, "MemberSignature[@Language=\"C#\"]/@Value").Value);
								continue;
							}
							// oldmember lookup may have failed due to type parameter renames.
							// Try again.
							oldmember = (XmlElement) basefile.SelectSingleNode (GetXPathForMember (m));
							if (oldmember == null) {
								XmlElement members = WriteElement(basefile.DocumentElement, "Members");
								oldmember = basefile.CreateElement ("Member");
								oldmember.SetAttribute ("MemberName", dm.MemberName);
								members.AppendChild (oldmember);
								foreach (string key in Sort (dm.MemberSignatures.Keys)) {
									XmlElement ms = basefile.CreateElement ("MemberSignature");
									ms.SetAttribute ("Language", key);
									ms.SetAttribute ("Value", (string) dm.MemberSignatures [key]);
									oldmember.AppendChild (ms);
								}
								oldmember.SetAttribute ("__monodocer-seen__", "true");
								Console.WriteLine ("Member Added: {0}", MakeMemberSignature (m));
								additions++;
							}
						}
						else {
							m = GetMember (type, new DocumentationMember (oldmember));
							if (m == null) {
								Error ("Could not import ECMA docs for `{0}'s `{1}': MemberInfo not found.",
										type.FullName, dm.MemberSignatures ["C#"]);
								continue;
							}
							oldmember.SetAttribute ("__monodocer-seen__", "true");
						}
						DocsNodeInfo node = new DocsNodeInfo (oldmember, m);
						if (ecmaDocsMembers.Name != "Docs")
							throw new InvalidOperationException ("Found " + ecmaDocsMembers.Name + "; expected <Docs/>!");
						node.EcmaDocs = ecmaDocsMembers;
						yield return node;
						break;
					}
					case "Members":
						if (membersDepth == ecmaDocsMembers.Depth && ecmaDocsMembers.NodeType == XmlNodeType.EndElement) {
							go = false;
						}
						break;
				}
			}
		}
		foreach (XmlElement oldmember in basefile.SelectNodes("Type/Members/Member")) {
			if (oldmember.GetAttribute ("__monodocer-seen__") == "true") {
				oldmember.RemoveAttribute ("__monodocer-seen__");
				continue;
			}
			MemberInfo m = GetMember (type, new DocumentationMember (oldmember));
			if (m == null) {
				yield return new DocsNodeInfo (oldmember);
			}
			else {
				yield return new DocsNodeInfo (oldmember, m);
			}
		}
	}

	static void DeleteMember (string reason, string output, XmlNode member, MyXmlNodeList todelete)
	{
		string format = output != null
			? "{0}: File='{1}'; Signature='{4}'"
			: "{0}: XPath='/Type[@FullName=\"{2}\"]/Members/Member[@MemberName=\"{3}\"]'; Signature='{4}'";
		Error (format,
				reason, 
				output,
				member.OwnerDocument.DocumentElement.GetAttribute ("FullName"),
				member.Attributes ["MemberName"].Value, 
				member.SelectSingleNode ("MemberSignature[@Language='C#']/@Value").Value);
		if (!delete && MemberDocsHaveUserContent (member)) {
			Error ("Member deletions must be enabled with the --delete option.");
		} else {
			todelete.Add (member);
			deletions++;
		}
	}

	class MemberComparer : XmlNodeComparer {
		public override int Compare (XmlNode x, XmlNode y)
		{
			int r;
			string xMemberName = x.Attributes ["MemberName"].Value;
			string yMemberName = y.Attributes ["MemberName"].Value;

			// generic methods *end* with '>'
			// it's possible for explicitly implemented generic interfaces to
			// contain <...> without being a generic method
			if ((!xMemberName.EndsWith (">") || !yMemberName.EndsWith (">")) &&
					(r = xMemberName.CompareTo (yMemberName)) != 0)
				return r;

			int lt;
			if ((lt = xMemberName.IndexOf ("<")) >= 0)
				xMemberName = xMemberName.Substring (0, lt);
			if ((lt = yMemberName.IndexOf ("<")) >= 0)
				yMemberName = yMemberName.Substring (0, lt);
			if ((r = xMemberName.CompareTo (yMemberName)) != 0)
				return r;

			// if @MemberName matches, then it's either two different types of
			// members sharing the same name, e.g. field & property, or it's an
			// overloaded method.
			// for different type, sort based on MemberType value.
			r = x.SelectSingleNode ("MemberType").InnerText.CompareTo (
					y.SelectSingleNode ("MemberType").InnerText);
			if (r != 0)
				return r;

			// same type -- must be an overloaded method.  Sort based on type 
			// parameter count, then parameter count, then by the parameter 
			// type names.
			XmlNodeList xTypeParams = x.SelectNodes ("TypeParameters/TypeParameter");
			XmlNodeList yTypeParams = y.SelectNodes ("TypeParameters/TypeParameter");
			if (xTypeParams.Count != yTypeParams.Count)
				return xTypeParams.Count <= yTypeParams.Count ? -1 : 1;
			for (int i = 0; i < xTypeParams.Count; ++i) {
				r = xTypeParams [i].Attributes ["Name"].Value.CompareTo (
						yTypeParams [i].Attributes ["Name"].Value);
				if (r != 0)
					return r;
			}

			XmlNodeList xParams = x.SelectNodes ("Parameters/Parameter");
			XmlNodeList yParams = y.SelectNodes ("Parameters/Parameter");
			if (xParams.Count != yParams.Count)
				return xParams.Count <= yParams.Count ? -1 : 1;
			for (int i = 0; i < xParams.Count; ++i) {
				r = xParams [i].Attributes ["Type"].Value.CompareTo (
						yParams [i].Attributes ["Type"].Value);
				if (r != 0)
					return r;
			}
			// all parameters match, but return value might not match if it was
			// changed between one version and another.
			XmlNode xReturn = x.SelectSingleNode ("ReturnValue/ReturnType");
			XmlNode yReturn = y.SelectSingleNode ("ReturnValue/ReturnType");
			if (xReturn != null && yReturn != null) {
				r = xReturn.InnerText.CompareTo (yReturn.InnerText);
				if (r != 0)
					return r;
			}

			return 0;
		}
	}

	static readonly MemberComparer DefaultMemberComparer = new MemberComparer ();

	private static void SortTypeMembers (XmlNode members)
	{
		if (members == null)
			return;
		SortXmlNodes (members, members.SelectNodes ("Member"), DefaultMemberComparer);
	}
	
	private static bool MemberDocsHaveUserContent (XmlNode e)
	{
		e = (XmlElement)e.SelectSingleNode("Docs");
		if (e == null) return false;
		foreach (XmlElement d in e.SelectNodes("*"))
			if (d.InnerText != "" && !d.InnerText.StartsWith("To be added"))
				return true;
		return false;
	}
	
	private static bool IsNew(MemberInfo m) {
		if (!nooverrides) return true;
		if (m is MethodInfo && !IsNew((MethodInfo)m)) return false;
		if (m is PropertyInfo && !IsNew(((PropertyInfo)m).GetGetMethod())) return false;
		if (m is PropertyInfo && !IsNew(((PropertyInfo)m).GetSetMethod())) return false;
		if (m is EventInfo && !IsNew(((EventInfo)m).GetAddMethod(true))) return false;
		if (m is EventInfo && !IsNew(((EventInfo)m).GetRaiseMethod())) return false;
		if (m is EventInfo && !IsNew(((EventInfo)m).GetRemoveMethod())) return false;
		return true;
	}
	
	private static bool IsNew(MethodInfo m) {
		if (m == null) return true;
		MethodInfo b = m.GetBaseDefinition();
		if (b == null || b == m) return true;
		return false;
	}
	
	// UPDATE HELPER FUNCTIONS	
	
#if false
	private static XmlElement FindMatchingMember(Type type, XmlElement newfile, XmlElement oldmember) {
		MemberInfo oldmember2 = GetMember(type, oldmember.CreateNavigator ());
		if (oldmember2 == null) return null;
		
		string membername = oldmember.GetAttribute("MemberName");
		foreach (XmlElement newmember in newfile.SelectNodes("Members/Member[@MemberName='" + membername + "']")) {
			if (GetMember(type, newmember.CreateNavigator ()) == oldmember2) return newmember;
		}
		
		return null;
	}
#endif
	
	private static MemberInfo GetMember(Type type, DocumentationMember member) {
		string membertype = member.MemberType;
		
		string returntype = member.ReturnType;
		
		string docName = member.MemberName;
		string[] docTypeParams = GetTypeParameters (docName);

		// Loop through all members in this type with the same name
		foreach (MemberInfo mi in GetReflectionMembers (type, docName)) {
			if (mi is Type) continue;
			if (GetMemberType(mi) != membertype) continue;

			string sig = MakeMemberSignature(mi);
			if (sig == null) continue; // not publicly visible

			ParameterInfo[] pis = null;
			string[] typeParams = null;
			if (mi is MethodInfo || mi is ConstructorInfo) {
				MethodBase mb = (MethodBase) mi;
				pis = mb.GetParameters();
				if (docTypeParams != null && DocUtils.GetContainsGenericParameters (mb)) {
					Type[] args = DocUtils.GetGenericArguments (mb);
					if (args.Length == docTypeParams.Length) {
						typeParams = new string [args.Length];
						for (int i = 0; i < args.Length; ++i)
							typeParams [i] = args [i].Name;
					}
				}
			}
			else if (mi is PropertyInfo)
				pis = ((PropertyInfo)mi).GetIndexParameters();
			
			int mcount = member.Parameters == null ? 0 : member.Parameters.Count;
			int pcount = pis == null ? 0 : pis.Length;
			if (mcount != pcount)
				continue;
			
			if (mi is MethodInfo) {
				// Casting operators can overload based on return type.
				if (returntype != GetReplacedString (
							GetDocTypeFullName (((MethodInfo)mi).ReturnType), 
							typeParams, docTypeParams)) {
					continue;
				}
			}

			if (pcount == 0)
				return mi;
			bool good = true;
			for (int i = 0; i < pis.Length; i++) {
				string paramType = GetReplacedString (
					GetDocParameterType (pis [i].ParameterType),
					typeParams, docTypeParams);
				if (paramType != (string) member.Parameters [i]) {
					good = false;
					break;
				}
			}
			if (!good) continue;

			return mi;
		}
		
		return null;
	}

	private static MemberInfoEnumerable GetReflectionMembers (Type type, string docName)
	{
		// need to worry about 4 forms of //@MemberName values:
		//  1. "Normal" (non-generic) member names: GetEnumerator
		//    - Lookup as-is.
		//  2. Explicitly-implemented interface member names: System.Collections.IEnumerable.Current
		//    - try as-is, and try type.member (due to "kludge" for property
		//      support.
		//  3. "Normal" Generic member names: Sort<T> (CSC)
		//    - need to remove generic parameters --> "Sort"
		//  4. Explicitly-implemented interface members for generic interfaces: 
		//    -- System.Collections.Generic.IEnumerable<T>.Current
		//    - Try as-is, and try type.member, *keeping* the generic parameters.
		//     --> System.Collections.Generic.IEnumerable<T>.Current, IEnumerable<T>.Current
		//  5. As of 2008-01-02, gmcs will do e.g. 'IFoo`1[A].Method' instead of
		//    'IFoo<A>.Method' for explicitly implemented methods; don't interpret
		//    this as (1) or (2).
		if (docName.IndexOf ('<') == -1 && docName.IndexOf ('[') == -1) {
			// Cases 1 & 2
			foreach (MemberInfo mi in type.GetMember (docName, DefaultBindingFlags))
				yield return mi;
			if (CountChars (docName, '.') > 0)
				// might be a property; try only type.member instead of
				// namespace.type.member.
				foreach (MemberInfo mi in 
						type.GetMember (DocUtils.GetTypeDotMember (docName), DefaultBindingFlags))
					yield return mi;
			yield break;
		}
		// cases 3 & 4
		int numLt = 0;
		int numDot = 0;
		int startLt, startType, startMethod;
		startLt = startType = startMethod = -1;
		for (int i = 0; i < docName.Length; ++i) {
			switch (docName [i]) {
				case '<':
					if (numLt == 0) {
						startLt = i;
					}
					++numLt;
					break;
				case '>':
					--numLt;
					if (numLt == 0 && (i + 1) < docName.Length)
						// there's another character in docName, so this <...> sequence is
						// probably part of a generic type -- case 4.
						startLt = -1;
					break;
				case '.':
					startType = startMethod;
					startMethod = i;
					++numDot;
					break;
			}
		}
		string refName = startLt == -1 ? docName : docName.Substring (0, startLt);
		// case 3
		foreach (MemberInfo mi in type.GetMember (refName, DefaultBindingFlags))
			yield return mi;

		// case 4
		foreach (MemberInfo mi in type.GetMember (refName.Substring (startType + 1), DefaultBindingFlags))
			yield return mi;

		// If we _still_ haven't found it, we've hit another generic naming issue:
		// post Mono 1.1.18, gmcs generates [[FQTN]] instead of <TypeName> for
		// explicitly-implemented METHOD names (not properties), e.g. 
		// "System.Collections.Generic.IEnumerable`1[[Foo, test, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]].GetEnumerator"
		// instead of "System.Collections.Generic.IEnumerable<Foo>.GetEnumerator",
		// which the XML docs will contain.
		//
		// Alas, we can't derive the Mono name from docName, so we need to iterate
		// over all member names, convert them into CSC format, and compare... :-(
		if (numDot == 0)
			yield break;
		foreach (MemberInfo mi in type.GetMembers (DefaultBindingFlags)) {
			if (GetMemberName (mi) == docName)
				yield return mi;
		}
	}

	static string[] GetTypeParameters (string docName)
	{
		if (docName [docName.Length-1] != '>')
			return null;
		StringList types = new StringList ();
		int endToken = docName.Length-2;
		int i = docName.Length-2;
		do {
			if (docName [i] == ',' || docName [i] == '<') {
				types.Add (docName.Substring (i + 1, endToken - i));
				endToken = i-1;
			}
			if (docName [i] == '<')
				break;
		} while (--i >= 0);

		types.Reverse ();
		return DocUtils.ToStringArray (types);
	}

	static string GetReplacedString (string typeName, string[] from, string[] to)
	{
		if (from == null)
			return typeName;
		for (int i = 0; i < from.Length; ++i)
			typeName = typeName.Replace (from [i], to [i]);
		return typeName;
	}
	
	// CREATE A STUB DOCUMENTATION FILE	

	public static XmlElement StubType(Type type, string output, XmlReader ecmaDocsType) {
		string typesig = MakeTypeSignature(type);
		if (typesig == null) return null; // not publicly visible
		
		XmlDocument doc = new XmlDocument();
		XmlElement root = doc.CreateElement("Type");
		doc.AppendChild (root);

		DoUpdateType2 ("New Type", doc, type, output, true, ecmaDocsType);
		
		return root;
	}

	private static XmlElement CreateSinceNode (XmlDocument doc)
	{
		XmlElement s = doc.CreateElement ("since");
		s.SetAttribute ("version", since);
		return s;
	}
	
	// STUBBING/UPDATING FUNCTIONS
	
	public static void UpdateType(XmlElement root, Type type, XmlReader ecmaDocsType) {
		root.SetAttribute("Name", GetDocTypeName (type));
		root.SetAttribute("FullName", GetDocTypeFullName (type));

		WriteElementAttribute(root, "TypeSignature[@Language='C#']", "Language", "C#");
		WriteElementAttribute(root, "TypeSignature[@Language='C#']", "Value", MakeTypeSignature(type));
		
		XmlElement ass = WriteElement(root, "AssemblyInfo");
		WriteElementText(ass, "AssemblyName", type.Assembly.GetName().Name);
		UpdateAssemblyVersions(root, type, true);
		if (type.Assembly.GetName().CultureInfo.Name != "")
			WriteElementText(ass, "AssemblyCulture", type.Assembly.GetName().CultureInfo.Name);
		else
			ClearElement(ass, "AssemblyCulture");
		
		// Why-oh-why do we put assembly attributes in each type file?
		// Neither monodoc nor monodocs2html use them, so I'm deleting them
		// since they're outdated in current docs, and a waste of space.
		//MakeAttributes(ass, type.Assembly, true);
		XmlNode assattrs = ass.SelectSingleNode("Attributes");
		if (assattrs != null)
			ass.RemoveChild(assattrs);
		
		NormalizeWhitespace(ass);
		
		if (DocUtils.IsGenericType (type)) {
			MakeTypeParameters (root, DocUtils.GetGenericArguments (type));
		} else {
			ClearElement(root, "TypeParameters");
		}
		
		if (type.BaseType != null) {
			XmlElement basenode = WriteElement(root, "Base");
			
			string basetypename = GetDocTypeFullName (type.BaseType);
			if (basetypename == "System.MulticastDelegate") basetypename = "System.Delegate";
			WriteElementText(root, "Base/BaseTypeName", basetypename);
			
			// Document how this type instantiates the generic parameters of its base type
			if (DocUtils.IsGenericType (type.BaseType)) {
				ClearElement(basenode, "BaseTypeArguments");
				Type[] baseGenArgs     = DocUtils.GetGenericArguments (type.BaseType);
				Type genericDefinition = DocUtils.GetGenericTypeDefinition (type.BaseType);
				Type[] genTypeDefArgs  = DocUtils.GetGenericArguments (genericDefinition);
				for (int i = 0; i < baseGenArgs.Length; i++) {
					Type typearg   = baseGenArgs [i];
					Type typeparam = genTypeDefArgs [i];
					
					XmlElement bta = WriteElement(basenode, "BaseTypeArguments");
					XmlElement arg = bta.OwnerDocument.CreateElement("BaseTypeArgument");
					bta.AppendChild(arg);
					arg.SetAttribute("TypeParamName", typeparam.Name);
					arg.InnerText = GetDocTypeFullName (typearg);
				}
			}
		} else {
			ClearElement(root, "Base");
		}

		if (!IsDelegate(type) && !type.IsEnum) {
			// Get a sorted list of interface implementations.  Don't include
			// interfaces that are implemented by a base type or another interface
			// because they go on the base type or base interface's signature.
			ArrayList interface_names = new ArrayList();
			foreach (Type i in type.GetInterfaces())
				if ((type.BaseType == null || Array.IndexOf(type.BaseType.GetInterfaces(), i) == -1) && InterfaceNotFromAnother(i, type.GetInterfaces()))
					interface_names.Add(GetDocTypeFullName (i));
			interface_names.Sort();

			XmlElement interfaces = WriteElement(root, "Interfaces");
			interfaces.RemoveAll();
			foreach (string iname in interface_names) {
				XmlElement iface = root.OwnerDocument.CreateElement("Interface");
				interfaces.AppendChild(iface);
				WriteElementText(iface, "InterfaceName", iname);
			}
		} else {
			ClearElement(root, "Interfaces");
		}

		MakeAttributes(root, type, false);
		
		if (IsDelegate(type)) {
			MakeTypeParameters (root, DocUtils.GetGenericArguments (type));
			MakeParameters(root, type.GetMethod("Invoke").GetParameters());
			MakeReturnValue(root, type.GetMethod("Invoke"));
		}
		
		DocsNodeInfo typeInfo = new DocsNodeInfo (WriteElement(root, "Docs"), type);
		if (ecmaDocsType != null) {
			if (ecmaDocsType.Name != "Docs") {
				int depth = ecmaDocsType.Depth;
				while (ecmaDocsType.Read ()) {
					if (ecmaDocsType.Name == "Docs" && ecmaDocsType.Depth == depth + 1)
						break;
				}
			}
			if (!ecmaDocsType.IsStartElement ("Docs"))
				throw new InvalidOperationException ("Found " + ecmaDocsType.Name + "; expecting <Docs/>!");
			typeInfo.EcmaDocs = ecmaDocsType;
		}
		MakeDocNode (typeInfo);
		
		if (!IsDelegate (type))
			WriteElement (root, "Members");

		NormalizeWhitespace(root);
	}

	class MemberInfoComparer : IComparer
#if !NET_1_0
														 , IComparer<MemberInfo>
#endif
	{
		public int Compare (MemberInfo x, MemberInfo y)
		{
			string xs = slashdocFormatter.GetName (x);
			string ys = slashdocFormatter.GetName (y);
			// return String.Compare (xs, ys, StringComparison.OrdinalIgnoreCase);
			return string.Compare (xs, ys, true, CultureInfo.InvariantCulture);
		}

		public int Compare (object x, object y)
		{
			return Compare ((MemberInfo) x, (MemberInfo) y);
		}
	}

	static MemberInfoComparer memberInfoComparer = new MemberInfoComparer ();

	private static MemberInfo[] Sort (MemberInfo[] members)
	{
#if NET_1_0
		ArrayList l = new ArrayList ();
		l.AddRange (members);
		l.Sort (memberInfoComparer);
		return (MemberInfo[]) l.ToArray (typeof(MemberInfo));
#else
		Array.Sort (members, memberInfoComparer);
		return members;
#endif
	}

#if NET_1_0
	static IEnumerable Sort (IEnumerable list)
	{
		ArrayList l = new ArrayList (list as ICollection);
		l.Sort ();
		return l;
	}
#else
	static IEnumerable<T> Sort<T> (IEnumerable<T> list)
	{
		List<T> l = new List<T> (list);
		l.Sort ();
		return l;
	}
#endif
	
	private static void UpdateMember(DocsNodeInfo info) {	
		XmlElement me = (XmlElement) info.Node;
		MemberInfo mi = info.Member;
		WriteElementAttribute(me, "MemberSignature[@Language='C#']", "Language", "C#");
		WriteElementAttribute(me, "MemberSignature[@Language='C#']", "Value", MakeMemberSignature(mi));

		WriteElementText(me, "MemberType", GetMemberType(mi));
		
		UpdateAssemblyVersions(me, mi, true);
		MakeAttributes(me, mi, false);
		MakeReturnValue(me, mi);
		if (mi is MethodBase) {
			MethodBase mb = (MethodBase) mi;
			if (DocUtils.GetContainsGenericParameters (mb))
				MakeTypeParameters (me, DocUtils.GetGenericArguments (mb));
		}
		MakeParameters(me, mi);
		
		string fieldValue;
		if (mi is FieldInfo && GetFieldConstValue((FieldInfo)mi, out fieldValue))
			WriteElementText(me, "MemberValue", fieldValue);
		
		info.Node = WriteElement (me, "Docs");
		MakeDocNode (info);
		UpdateExtensionMethods (me, info);
	}

	static readonly string[] ValidExtensionMembers = {
		"Docs",
		"MemberSignature",
		"MemberType",
		"Parameters",
		"ReturnValue",
		"TypeParameters",
	};

	static readonly string[] ValidExtensionDocMembers = {
		"param",
		"summary",
		"typeparam",
	};

	private static void UpdateExtensionMethods (XmlElement e, DocsNodeInfo info)
	{
		MethodInfo me = info.Member as MethodInfo;
		if (me == null)
			return;
		if (info.Parameters.Length < 1)
			return;
		if (!DocUtils.IsExtensionMethod (me))
			return;

		XmlNode em = e.OwnerDocument.CreateElement ("ExtensionMethod");
		XmlNode member = e.CloneNode (true);
		em.AppendChild (member);
		RemoveExcept (member, ValidExtensionMembers);
		RemoveExcept (member.SelectSingleNode ("Docs"), ValidExtensionDocMembers);
		WriteElementText (member, "MemberType", "ExtensionMethod");
		XmlElement link = member.OwnerDocument.CreateElement ("Link");
		link.SetAttribute ("Type", slashdocFormatter.GetName (me.DeclaringType));
		link.SetAttribute ("Member", slashdocFormatter.GetDeclaration (me));
		member.AppendChild (link);
		AddTargets (em, info);

		extensionMethods.Add (em);
	}

	private static void RemoveExcept (XmlNode node, string[] except)
	{
		if (node == null)
			return;
		MyXmlNodeList remove = null;
		foreach (XmlNode n in node.ChildNodes) {
			if (Array.BinarySearch (except, n.Name) < 0) {
				if (remove == null)
					remove = new MyXmlNodeList ();
				remove.Add (n);
			}
		}
		if (remove != null)
			foreach (XmlNode n in remove)
				node.RemoveChild (n);
	}

	private static void AddTargets (XmlNode member, DocsNodeInfo info)
	{
		XmlElement targets = member.OwnerDocument.CreateElement ("Targets");
		member.PrependChild (targets);
		if (!DocUtils.IsGenericParameter (info.Parameters [0].ParameterType))
			AppendElementAttributeText (targets, "Target", "Type",
				slashdocFormatter.GetDeclaration (info.Parameters [0].ParameterType));
		else {
			Type[] constraints = DocUtils.GetGenericParameterConstraints (
				info.Parameters [0].ParameterType);
			if (constraints.Length == 0)
				AppendElementAttributeText (targets, "Target", "Type", "System.Object");
			else
				foreach (Type c in constraints)
					AppendElementAttributeText(targets, "Target", "Type",
						slashdocFormatter.GetDeclaration (c));
		}
	}
	
	private static bool GetFieldConstValue(FieldInfo field, out string value) {
		value = null;
		if (field.DeclaringType.IsEnum) return false;
		if (DocUtils.IsGenericType (field.DeclaringType)) return false;
		if (field.IsLiteral || (field.IsStatic && field.IsInitOnly)) {
			object val;
			try {
				val = field.GetValue(null);
			} catch {
				return false;
			}
			if (val == null) value = "null";
			else if (val is Enum) value = val.ToString();
			else if (val is IFormattable) {
				value = ((IFormattable)val).ToString();
				if (val is string)
					value = "\"" + value + "\"";
			}
			if (value != null && value != "")
				return true;
		}
		return false;
	}
	
	// XML HELPER FUNCTIONS
	
	private static XmlElement WriteElement(XmlNode parent, string element) {
		XmlElement ret = (XmlElement)parent.SelectSingleNode(element);
		if (ret == null) {
			string[] path = element.Split('/');
			foreach (string p in path) {
				ret = (XmlElement)parent.SelectSingleNode(p);
				if (ret == null) {
					string ename = p;
					if (ename.IndexOf('[') >= 0) // strip off XPath predicate
						ename = ename.Substring(0, ename.IndexOf('['));
					ret = parent.OwnerDocument.CreateElement(ename);
					parent.AppendChild(ret);
					parent = ret;
				} else {
					parent = ret;
				}
			}
		}
		return ret;
	}
	private static void WriteElementText(XmlNode parent, string element, string value) {
		XmlElement node = WriteElement(parent, element);
		node.InnerText = value;
	}

#if !NET_1_0
	static XmlElement AppendElementText (XmlNode parent, string element, string value)
	{
		XmlElement n = parent.OwnerDocument.CreateElement (element);
		parent.AppendChild (n);
		n.InnerText = value;
		return n;
	}
#endif

	static XmlElement AppendElementAttributeText (XmlNode parent, string element, string attribute, string value)
	{
		XmlElement n = parent.OwnerDocument.CreateElement (element);
		parent.AppendChild (n);
		n.SetAttribute (attribute, value);
		return n;
	}

	private static XmlNode CopyNode (XmlNode source, XmlNode dest)
	{
		XmlNode copy = dest.OwnerDocument.ImportNode (source, true);
		dest.AppendChild (copy);
		return copy;
	}

	private static void WriteElementInitialText(XmlElement parent, string element, string value) {
		XmlElement node = (XmlElement)parent.SelectSingleNode(element);
		if (node != null)
			return;
		node = WriteElement(parent, element);
		node.InnerText = value;
	}
	private static void WriteElementAttribute(XmlElement parent, string element, string attribute, string value) {
		XmlElement node = WriteElement(parent, element);
		if (node.GetAttribute(attribute) == value) return;
		node.SetAttribute(attribute, value);
	}
	private static void ClearElement(XmlElement parent, string name) {
		XmlElement node = (XmlElement)parent.SelectSingleNode(name);
		if (node != null)
			parent.RemoveChild(node);
	}
	
	// DOCUMENTATION HELPER FUNCTIONS
	
	private static void MakeDocNode (DocsNodeInfo info)
	{
		Type[] genericParams        = info.GenericParameters;
		ParameterInfo[] parameters  = info.Parameters;
		Type returntype             = info.ReturnType;
		bool returnisreturn         = info.ReturnIsReturn;
		XmlElement e                = info.Node;
		bool addremarks             = info.AddRemarks;

		WriteElementInitialText(e, "summary", "To be added.");
		
		if (parameters != null) {
			string[] values = new string [parameters.Length];
			for (int i = 0; i < values.Length; ++i)
				values [i] = parameters [i].Name;
			UpdateParameters (e, "param", values);
		}

		if (genericParams != null) {
			string[] values = new string [genericParams.Length];
			for (int i = 0; i < values.Length; ++i)
				values [i] = genericParams [i].Name;
			UpdateParameters (e, "typeparam", values);
		}

		string retnodename = null;
		if (returntype != null && returntype != typeof(void)) {
			retnodename = returnisreturn ? "returns" : "value";
			string retnodename_other = !returnisreturn ? "returns" : "value";
			
			// If it has a returns node instead of a value node, change its name.
			XmlElement retother = (XmlElement)e.SelectSingleNode(retnodename_other);
			if (retother != null) {
				XmlElement retnode = e.OwnerDocument.CreateElement(retnodename);
				foreach (XmlNode node in retother)
					retnode.AppendChild(node.CloneNode(true));
				e.ReplaceChild(retnode, retother);
			} else {
				WriteElementInitialText(e, retnodename, "To be added.");
			}
		} else {
			ClearElement(e, "returns");
			ClearElement(e, "value");
		}

		if (addremarks)
			WriteElementInitialText(e, "remarks", "To be added.");

		if (info.EcmaDocs != null) {
			XmlReader r = info.EcmaDocs;
			int depth = r.Depth;
			r.ReadStartElement ("Docs");
			while (r.Read ()) {
				if (r.Name == "Docs") {
					if (r.Depth == depth && r.NodeType == XmlNodeType.EndElement)
						break;
					else
						throw new InvalidOperationException ("Skipped past current <Docs/> element!");
				}
				if (!r.IsStartElement ())
					continue;
				switch (r.Name) {
					case "param":
					case "typeparam": {
						XmlNode doc = e.SelectSingleNode (
								r.Name + "[@name='" + r.GetAttribute ("name") + "']");
						string value = r.ReadInnerXml ();
						if (doc != null)
							doc.InnerXml = value.Replace ("\r", "");
						break;
					}
					case "altmember":
					case "exception":
					case "permission":
					case "seealso": {
						string name = r.Name;
						string cref = r.GetAttribute ("cref");
						XmlNode doc = e.SelectSingleNode (
								r.Name + "[@cref='" + cref + "']");
						string value = r.ReadInnerXml ().Replace ("\r", "");
						if (doc != null)
							doc.InnerXml = value;
						else {
							XmlElement n = e.OwnerDocument.CreateElement (name);
							n.SetAttribute ("cref", cref);
							n.InnerXml = value;
							e.AppendChild (n);
						}
						break;
					}
					default: {
						string name = r.Name;
						string xpath = r.Name;
						StringList attributes = new StringList (r.AttributeCount);
						if (r.MoveToFirstAttribute ()) {
							do {
								attributes.Add ("@" + r.Name + "=\"" + r.Value + "\"");
							} while (r.MoveToNextAttribute ());
							r.MoveToContent ();
						}
						if (attributes.Count > 0) {
							xpath += "[" + string.Join (" and ", DocUtils.ToStringArray (attributes)) + "]";
						}
						XmlNode doc = e.SelectSingleNode (xpath);
						string value = r.ReadInnerXml ().Replace ("\r", "");
						if (doc != null) {
							doc.InnerXml = value;
						}
						else {
							XmlElement n = e.OwnerDocument.CreateElement (name);
							n.InnerXml = value;
							foreach (string a in attributes) {
								int eq = a.IndexOf ('=');
								n.SetAttribute (a.Substring (1, eq-1), a.Substring (eq+2, a.Length-eq-3));
							}
							e.AppendChild (n);
						}
						break;
					}
				}
			}
		}
		if (info.SlashDocs != null) {
			XmlNode elem = info.SlashDocs;
			if (elem != null) {
				if (elem.SelectSingleNode("summary") != null)
					ClearElement(e, "summary");
				if (elem.SelectSingleNode("remarks") != null)
					ClearElement(e, "remarks");
				if (elem.SelectSingleNode("value") != null)
					ClearElement(e, "value");
				if (retnodename != null && elem.SelectSingleNode(retnodename) != null)
					ClearElement(e, retnodename);

				foreach (XmlNode child in elem.ChildNodes) {
					switch (child.Name) {
						case "param":
						case "typeparam": {
							XmlElement p2 = (XmlElement) e.SelectSingleNode (child.Name + "[@name='" + child.Attributes ["name"].Value + "']");
							if (p2 != null)
								p2.InnerXml = child.InnerXml;
							break;
						}
						case "altmember":
						case "exception":
						case "permission": {
							XmlElement a = (XmlElement) e.SelectSingleNode (child.Name + "[@cref='" + child.Attributes ["cref"].Value + "']");
							if (a == null) {
								a = e.OwnerDocument.CreateElement (child.Name);
								a.SetAttribute ("cref", child.Attributes ["cref"].Value);
								e.AppendChild (a);
							}
							a.InnerXml = child.InnerXml;
							break;
						}
						case "seealso": {
							XmlElement a = (XmlElement) e.SelectSingleNode ("altmember[@cref='" + child.Attributes ["cref"].Value + "']");
							if (a == null) {
								a = e.OwnerDocument.CreateElement ("altmember");
								a.SetAttribute ("cref", child.Attributes ["cref"].Value);
								e.AppendChild (a);
							}
							break;
						}
						default:
							CopyNode (child, e);
							break;
					}
				}
			}
		}
		
		OrderDocsNodes (e, e.ChildNodes);
		NormalizeWhitespace(e);
	}

	static readonly string[] DocsNodeOrder = {
		"typeparam", "param", "summary", "returns", "value", "remarks",
	};

	private static void OrderDocsNodes (XmlNode docs, XmlNodeList children)
	{
		MyXmlNodeList newChildren = new MyXmlNodeList (children.Count);
		for (int i = 0; i < DocsNodeOrder.Length; ++i) {
			for (int j = 0; j < children.Count; ++j) {
				XmlNode c = children [j];
				if (c.Name == DocsNodeOrder [i]) {
					newChildren.Add (c);
				}
			}
		}
		if (newChildren.Count >= 0)
			docs.PrependChild ((XmlNode) newChildren [0]);
		for (int i = 1; i < newChildren.Count; ++i) {
			XmlNode prev = (XmlNode) newChildren [i-1];
			XmlNode cur  = (XmlNode) newChildren [i];
			docs.RemoveChild (cur);
			docs.InsertAfter (cur, prev);
		}
	}
	

	private static void UpdateParameters (XmlElement e, string element, string[] values)
	{	
		if (values != null) {
			XmlNode[] paramnodes = new XmlNode[values.Length];
			
			// Some documentation had param nodes with leading spaces.
			foreach (XmlElement paramnode in e.SelectNodes(element)){
				paramnode.SetAttribute("name", paramnode.GetAttribute("name").Trim());
			}
			
			// If a member has only one parameter, we can track changes to
			// the name of the parameter easily.
			if (values.Length == 1 && e.SelectNodes(element).Count == 1) {
				UpdateParameterName (e, (XmlElement) e.SelectSingleNode(element), values [0]);
			}

			bool reinsert = false;

			// Pick out existing and still-valid param nodes, and
			// create nodes for parameters not in the file.
			Hashtable seenParams = new Hashtable();
			for (int pi = 0; pi < values.Length; pi++) {
				string p = values [pi];
				seenParams[p] = pi;
				
				paramnodes[pi] = e.SelectSingleNode(element + "[@name='" + p + "']");
				if (paramnodes[pi] != null) continue;
				
				XmlElement pe = e.OwnerDocument.CreateElement(element);
				pe.SetAttribute("name", p);
				pe.InnerText = "To be added.";
				paramnodes[pi] = pe;
				reinsert = true;
			}

			// Remove parameters that no longer exist and check all params are in the right order.
			int idx = 0;
			MyXmlNodeList todelete = new MyXmlNodeList ();
			foreach (XmlElement paramnode in e.SelectNodes(element)) {
				string name = paramnode.GetAttribute("name");
				if (!seenParams.ContainsKey(name)) {
					if (!delete && !paramnode.InnerText.StartsWith("To be added")) {
						Error ("The following param node can only be deleted if the --delete option is given: ");
						if (e.ParentNode == e.OwnerDocument.DocumentElement) {
							// delegate type
							Error ("\tXPath=/Type[@FullName=\"{0}\"]/Docs/param[@name=\"{1}\"]",
									e.OwnerDocument.DocumentElement.GetAttribute ("FullName"),
									name);
						}
						else {
							Error ("\tXPath=/Type[@FullName=\"{0}\"]//Member[@MemberName=\"{1}\"]/Docs/param[@name=\"{2}\"]",
									e.OwnerDocument.DocumentElement.GetAttribute ("FullName"),
									e.ParentNode.Attributes ["MemberName"].Value, 
									name);
						}
						Error ("\tValue={0}", paramnode.OuterXml);
					} else {
						todelete.Add (paramnode);
					}
					continue;
				}

				if ((int)seenParams[name] != idx)
					reinsert = true;
				
				idx++;
			}

			foreach (XmlNode n in todelete) {
				n.ParentNode.RemoveChild (n);
			}
			
			// Re-insert the parameter nodes at the top of the doc section.
			if (reinsert)
				for (int pi = values.Length-1; pi >= 0; pi--)
					e.PrependChild(paramnodes[pi]);
		} else {
			// Clear all existing param nodes
			foreach (XmlNode paramnode in e.SelectNodes(element)) {
				if (!delete && !paramnode.InnerText.StartsWith("To be added")) {
					Console.WriteLine("The following param node can only be deleted if the --delete option is given:");
					Console.WriteLine(paramnode.OuterXml);
				} else {
					paramnode.ParentNode.RemoveChild(paramnode);
				}
			}
		}
	}

	private static void UpdateParameterName (XmlElement docs, XmlElement pe, string newName)
	{
		string existingName = pe.GetAttribute ("name");
		pe.SetAttribute ("name", newName);
		if (existingName == newName)
			return;
		foreach (XmlElement paramref in docs.SelectNodes (".//paramref"))
			if (paramref.GetAttribute ("name").Trim () == existingName)
				paramref.SetAttribute ("name", newName);
	}

	private static void NormalizeWhitespace(XmlElement e) {
		// Remove all text and whitespace nodes from the element so it
		// is outputted with nice indentation and no blank lines.
		ArrayList deleteNodes = new ArrayList();
		foreach (XmlNode n in e)
			if (n is XmlText || n is XmlWhitespace || n is XmlSignificantWhitespace)
				deleteNodes.Add(n);
		foreach (XmlNode n in deleteNodes)
				n.ParentNode.RemoveChild(n);
	}
	
	private static bool UpdateAssemblyVersions(XmlElement root, MemberInfo member, bool add)
	{
		Type type = member as Type;
		if (type == null)
			type = member.DeclaringType;
		return UpdateAssemblyVersions(root, new string[]{ GetAssemblyVersion(type.Assembly) }, add);
	}
	
	private static string GetAssemblyVersion(Assembly assembly)
	{
		return assembly.GetName().Version.ToString();
	}
	
	private static bool UpdateAssemblyVersions(XmlElement root, string[] assemblyVersions, bool add)
	{
		XmlElement e = (XmlElement) root.SelectSingleNode ("AssemblyInfo");
		if (e == null) {
			e = root.OwnerDocument.CreateElement("AssemblyInfo");
			root.AppendChild(e);
		}
		MyXmlNodeList matches = new MyXmlNodeList (assemblyVersions.Length);
		foreach (XmlElement v in e.SelectNodes ("AssemblyVersion")) {
			foreach (string sv in assemblyVersions)
				if (v.InnerText == sv)
					matches.Add (v);
		}
		// matches.Count > 0 && add: ignore -- already present
		if (matches.Count > 0 && !add) {
			foreach (XmlNode c in matches)
				e.RemoveChild (c);
		}
		else if (matches.Count == 0 && add) {
			foreach (string sv in assemblyVersions) {
				XmlElement c = root.OwnerDocument.CreateElement("AssemblyVersion");
				c.InnerText = sv;
				e.AppendChild(c);
			}
		}
		// matches.Count == 0 && !add: ignore -- already not present

		XmlNodeList avs = e.SelectNodes ("AssemblyVersion");
		SortXmlNodes (e, avs, new VersionComparer ());

		return avs.Count != 0;
	}

#if !NET_1_0
	private static Type[] IgnorableAttributes = {
		// Security related attributes
		typeof (System.Reflection.AssemblyKeyFileAttribute),
		typeof (System.Reflection.AssemblyDelaySignAttribute),
		// Present in @RefType
		typeof (System.Runtime.InteropServices.OutAttribute),
		// For naming the indexer to use when not using indexers
		typeof (System.Reflection.DefaultMemberAttribute),
		// for decimal constants
		typeof (System.Runtime.CompilerServices.DecimalConstantAttribute),
		// compiler generated code
		typeof (System.Runtime.CompilerServices.CompilerGeneratedAttribute),
		// more compiler generated code, e.g. iterator methods
		typeof (System.Diagnostics.DebuggerHiddenAttribute),
		typeof (System.Runtime.CompilerServices.FixedBufferAttribute),
		typeof (System.Runtime.CompilerServices.UnsafeValueTypeAttribute),
		// extension methods
		typeof (System.Runtime.CompilerServices.ExtensionAttribute),
	};
#endif

	private static void MakeAttributes(XmlElement root, object attributes, bool assemblyAttributes) {
		int len;
#if NET_1_0
		object[] at = ((ICustomAttributeProvider) attributes).GetCustomAttributes (false);
		len = at.Length;
#else
		System.Collections.Generic.IList<CustomAttributeData> at;
		if (attributes is Assembly)
			at = CustomAttributeData.GetCustomAttributes((Assembly)attributes);
		else if (attributes is MemberInfo)
			at = CustomAttributeData.GetCustomAttributes((MemberInfo)attributes);
		else if (attributes is Module)
			at = CustomAttributeData.GetCustomAttributes((Module)attributes);
		else if (attributes is ParameterInfo)
			at = CustomAttributeData.GetCustomAttributes((ParameterInfo)attributes);
		else
			throw new ArgumentException("unsupported type: " + attributes.GetType().ToString());
		len = at.Count;
#endif
	
		if (len == 0) {
			ClearElement(root, "Attributes");
			return;
		}

		bool b = false;
		XmlElement e = (XmlElement)root.SelectSingleNode("Attributes");
		if (e != null)
			e.RemoveAll();
		else
			e = root.OwnerDocument.CreateElement("Attributes");
		
#if !NET_1_0
		foreach (CustomAttributeData a in at) {
			if (!IsPublic (a.Constructor.DeclaringType))
				continue;
			if (slashdocFormatter.GetName (a.Constructor.DeclaringType) == null)
				continue;
			
			if (Array.IndexOf (IgnorableAttributes, a.Constructor.DeclaringType) >= 0)
				continue;
			
			b = true;
			
			StringList fields = new StringList ();

			foreach (CustomAttributeTypedArgument f in a.ConstructorArguments) {
				fields.Add(MakeAttributesValueString(f.Value));
			}
			foreach (CustomAttributeNamedArgument f in a.NamedArguments) {
				fields.Add(f.MemberInfo.Name + "=" + MakeAttributesValueString(f.TypedValue.Value));
			}

			string a2 = String.Join(", ", DocUtils.ToStringArray (fields));
			if (a2 != "") a2 = "(" + a2 + ")";
			
			XmlElement ae = root.OwnerDocument.CreateElement("Attribute");
			e.AppendChild(ae);
			
			string name = a.Constructor.DeclaringType.FullName;
			if (name.EndsWith("Attribute")) name = name.Substring(0, name.Length-"Attribute".Length);
			WriteElementText(ae, "AttributeName", name + a2);
		}
#else
		foreach (Attribute a in at) {
			if (!IsPublic (a.GetType ()))
				continue;
			if (slashdocFormatter.GetName (a.GetType ()) == null) continue; // hide non-visible attributes
			//if (assemblyAttributes && a.GetType().FullName.StartsWith("System.Reflection.")) continue;
			if (a.GetType().FullName == "System.Reflection.AssemblyKeyFileAttribute" || a.GetType().FullName == "System.Reflection.AssemblyDelaySignAttribute") continue; // hide security-related attributes

 			b = true;
 			
			// There's no way to reconstruct how the attribute's constructor was called,
			// so as a substitute, just list the value of all of the attribute's public fields.
			
 			StringList fields = new StringList ();
			foreach (PropertyInfo f in a.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance)) {
				if (f.Name == "TypeId") continue;
				
				object v;
				try {
					v = f.GetValue(a, null);
					if (v == null) v = "null";
					else if (v is string) v = "\"" + v + "\"";
					else if (v is Type) v = "typeof(" + GetCSharpFullName ((Type)v) + ")";
					else if (v is Enum) v = v.GetType().FullName + "." + v.ToString().Replace(", ", "|");
				}
				catch (Exception ex) {
					v = "/* error getting property value: " + ex.Message + " */";
				}
					
				fields.Add(f.Name + "=" + v);
			}
 			string a2 = String.Join(", ", DocUtils.ToStringArray (fields));
 			if (a2 != "") a2 = "(" + a2 + ")";
 			
 			XmlElement ae = root.OwnerDocument.CreateElement("Attribute");
 			e.AppendChild(ae);
 			
			string name = a.GetType().FullName;
 			if (name.EndsWith("Attribute")) name = name.Substring(0, name.Length-"Attribute".Length);
 			WriteElementText(ae, "AttributeName", name + a2);
 		}
#endif
		
		if (b && e.ParentNode == null)
			root.AppendChild(e);
		else if (!b)
			ClearElement(root, "Attributes");
		
		NormalizeWhitespace(e);
	}
	
#if !NET_1_0
	private static string MakeAttributesValueString(object v) {
		if (v == null) return "null";
		else if (v is string) return "\"" + v + "\"";
		else if (v is bool) return (bool)v ? "true" : "false";
		else if (v is Type) return "typeof(" + GetCSharpFullName ((Type)v) + ")";
		else if (v is Enum) {
			string typename = v.GetType ().FullName;
			return typename + "." + v.ToString().Replace(", ", " | " + typename + ".");
		}
		else return v.ToString();
	}
#endif
	
	private static void MakeParameters(XmlElement root, ParameterInfo[] parameters) {
		XmlElement e = WriteElement(root, "Parameters");
		e.RemoveAll();
		foreach (ParameterInfo p in parameters) {
			XmlElement pe = root.OwnerDocument.CreateElement("Parameter");
			e.AppendChild(pe);
			pe.SetAttribute("Name", p.Name);
			pe.SetAttribute("Type", GetDocParameterType (p.ParameterType));
			if (p.ParameterType.IsByRef) {
				if (p.IsOut) pe.SetAttribute("RefType", "out");
				else pe.SetAttribute("RefType", "ref");
			}
			MakeAttributes(pe, p, false);
		}
	}
	
	private static void MakeTypeParameters(XmlElement root, Type[] typeParams)
	{
		if (typeParams == null || typeParams.Length == 0) {
			XmlElement f = (XmlElement) root.SelectSingleNode ("TypeParameters");
			if (f != null)
				root.RemoveChild (f);
			return;
		}
		XmlElement e = WriteElement(root, "TypeParameters");
		e.RemoveAll();
		foreach (Type t in typeParams) {
			XmlElement pe = root.OwnerDocument.CreateElement("TypeParameter");
			e.AppendChild(pe);
			pe.SetAttribute("Name", t.Name);
			MakeAttributes(pe, t, false);
#if !NET_1_0
			XmlElement ce = (XmlElement) e.SelectSingleNode ("Constraints");
			GenericParameterAttributes attrs = t.GenericParameterAttributes;
			Type[] constraints = t.GetGenericParameterConstraints ();
			if (attrs == GenericParameterAttributes.None && constraints.Length == 0) {
				if (ce != null)
					e.RemoveChild (ce);
				continue;
			}
			if (ce != null)
				ce.RemoveAll();
			else {
				ce = root.OwnerDocument.CreateElement ("Constraints");
			}
			pe.AppendChild (ce);
			if ((attrs & GenericParameterAttributes.Contravariant) != 0)
				AppendElementText (ce, "ParameterAttribute", "Contravariant");
			if ((attrs & GenericParameterAttributes.Covariant) != 0)
				AppendElementText (ce, "ParameterAttribute", "Covariant");
			if ((attrs & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
				AppendElementText (ce, "ParameterAttribute", "DefaultConstructorConstraint");
			if ((attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
				AppendElementText (ce, "ParameterAttribute", "NotNullableValueTypeConstraint");
			if ((attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
				AppendElementText (ce, "ParameterAttribute", "ReferenceTypeConstraint");
			foreach (Type c in constraints) {
				AppendElementText (ce, 
						c.IsInterface ? "InterfaceName" : "BaseTypeName", GetDocTypeFullName (c));
			}
#endif
		}
	}

	private static void MakeParameters(XmlElement root, MemberInfo mi) {
		if (mi is ConstructorInfo) MakeParameters(root, ((ConstructorInfo)mi).GetParameters());
		else if (mi is MethodInfo) {
			MethodBase mb = (MethodBase) mi;
			ParameterInfo[] parameters = mb.GetParameters();
			MakeParameters(root, parameters);
			if (parameters.Length > 0 && DocUtils.IsExtensionMethod (mb)) {
				XmlElement p = (XmlElement) root.SelectSingleNode ("Parameters/Parameter[position()=1]");
				p.SetAttribute ("RefType", "this");
			}
		}
		else if (mi is PropertyInfo) {
			ParameterInfo[] parameters = ((PropertyInfo)mi).GetIndexParameters();
			if (parameters.Length > 0)
				MakeParameters(root, parameters);
			else
				return;
		}
		else if (mi is FieldInfo) return;
		else if (mi is EventInfo) return;
		else throw new ArgumentException();
	}

	private static string GetDocParameterType (Type type)
	{
		return GetDocTypeFullName (type).Replace ("@", "&");
	}

	private static void MakeReturnValue(XmlElement root, Type type, ICustomAttributeProvider attributes) {
		XmlElement e = WriteElement(root, "ReturnValue");
		e.RemoveAll();
		WriteElementText(e, "ReturnType", GetDocTypeFullName (type));
		if (attributes != null)
			MakeAttributes(e, attributes, false);
	}
	
	private static void MakeReturnValue(XmlElement root, MemberInfo mi) {
		if (mi is ConstructorInfo) return;
		else if (mi is MethodInfo) MakeReturnValue(root, ((MethodInfo)mi).ReturnType, ((MethodInfo)mi).ReturnTypeCustomAttributes);
		else if (mi is PropertyInfo) MakeReturnValue(root, ((PropertyInfo)mi).PropertyType, null);
		else if (mi is FieldInfo) MakeReturnValue(root, ((FieldInfo)mi).FieldType, null);
		else if (mi is EventInfo) MakeReturnValue(root, ((EventInfo)mi).EventHandlerType, null);
		else throw new ArgumentException(mi + " is a " + mi.GetType().FullName);
	}
	
	private static XmlElement MakeMember(XmlDocument doc, DocsNodeInfo info) {
		MemberInfo mi = info.Member;
		if (mi is Type) return null;

		string sigs = MakeMemberSignature(mi);
		if (sigs == null) return null; // not publicly visible
		
		// no documentation for property/event accessors.  Is there a better way of doing this?
		if (mi.Name.StartsWith("get_")) return null;
		if (mi.Name.StartsWith("set_")) return null;
		if (mi.Name.StartsWith("add_")) return null;
		if (mi.Name.StartsWith("remove_")) return null;
		if (mi.Name.StartsWith("raise_")) return null;
		
		XmlElement me = doc.CreateElement("Member");
		me.SetAttribute("MemberName", GetMemberName (mi));
		
		info.Node = me;
		UpdateMember(info);

		if (since != null) {
			XmlNode docs = me.SelectSingleNode("Docs");
			docs.AppendChild (CreateSinceNode (doc));
		}
		
		return me;
	}

	private static string GetMemberName (MemberInfo mi)
	{
		MethodBase mb = mi as MethodBase;
		if (mb == null) {
			PropertyInfo pi = mi as PropertyInfo;
			if (pi == null)
				return mi.Name;
			return DocUtils.GetPropertyName (pi);
		}
		StringBuilder sb = new StringBuilder (mi.Name.Length);
		if (!DocUtils.IsExplicitlyImplemented (mb))
			sb.Append (mi.Name);
		else {
			Type iface;
			MethodInfo ifaceMethod;
			DocUtils.GetInfoForExplicitlyImplementedMethod (mb, out iface, out ifaceMethod);
			sb.Append (GetDocTypeFullName (iface));
			sb.Append ('.');
			sb.Append (ifaceMethod.Name);
		}
		if (DocUtils.GetContainsGenericParameters (mb)) {
			Type[] typeParams = DocUtils.GetGenericArguments (mb);
			if (typeParams.Length > 0) {
				sb.Append ("<");
				sb.Append (typeParams [0].Name);
				for (int i = 1; i < typeParams.Length; ++i)
					sb.Append (",").Append (typeParams [i].Name);
				sb.Append (">");
			}
		}
		return sb.ToString ();
	}

	private static int CountChars (string s, char c)
	{
		int count = 0;
		for (int i = 0; i < s.Length; ++i) {
			if (s [i] == c)
				++count;
		}
		return count;
	}
	
	static bool IsDelegate(Type type) {
		return typeof(System.Delegate).IsAssignableFrom (type) && !type.IsAbstract;
	}
	
	/// SIGNATURE GENERATION FUNCTIONS
	
	private static bool InterfaceNotFromAnother(Type i, Type[] i2) {
		foreach (Type t in i2)
			if (i != t && Array.IndexOf(t.GetInterfaces(), i) != -1)
				return false;
		return true;
	}
	
	static string MakeTypeSignature (Type type) {
		return csharpFormatter.GetDeclaration (type);
	}

	static string MakeMemberSignature(MemberInfo mi) {
		return csharpFullFormatter.GetDeclaration (mi);
	}

	static string GetMemberType(MemberInfo mi) {
		if (mi is ConstructorInfo) return "Constructor";
		if (mi is MethodInfo) return "Method";
		if (mi is PropertyInfo) return "Property";
		if (mi is FieldInfo) return "Field";
		if (mi is EventInfo) return "Event";
		throw new ArgumentException();
	}

	private static string GetDocTypeName (Type type)
	{
		return docTypeFormatter.GetName (type);
	}

	private static string GetDocTypeFullName (Type type)
	{
		return DocTypeFullMemberFormatter.Default.GetName (type);
	}

	private static string GetCSharpFullName (Type type)
	{
		return DocTypeFullMemberFormatter.Default.GetName (type);
	}

	class DocsNodeInfo {
		public DocsNodeInfo (XmlElement node)
		{
			this.Node = node;
		}

		public DocsNodeInfo (XmlElement node, Type type)
			: this (node)
		{
			SetType (type);
		}

		public DocsNodeInfo (XmlElement node, MemberInfo member)
			: this (node)
		{
			SetMemberInfo (member);
		}

		public void SetType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			GenericParameters = DocUtils.GetGenericArguments (type);
			if (type.DeclaringType != null) {
				Type[] declGenParams = DocUtils.GetGenericArguments (type.DeclaringType);
				if (declGenParams != null && GenericParameters.Length == declGenParams.Length) {
					GenericParameters = null;
				}
				else if (declGenParams != null) {
					Type[] nestedParams = new Type [GenericParameters.Length - declGenParams.Length];
					for (int i = 0; i < nestedParams.Length; ++i) {
						nestedParams [i] = GenericParameters [i+declGenParams.Length];
					}
					GenericParameters = nestedParams;
				}
			}
			if (IsDelegate(type)) {
				Parameters = type.GetMethod("Invoke").GetParameters();
				ReturnType = type.GetMethod("Invoke").ReturnType;
			}
			SetSlashDocs (type);
		}

		public void SetMemberInfo (MemberInfo member)
		{
			if (member == null)
				throw new ArgumentNullException ("member");
			ReturnIsReturn = true;
			AddRemarks = true;
			Member = member;
			
			if (member is MethodInfo || member is ConstructorInfo) {
				Parameters = ((MethodBase) member).GetParameters ();
				if (DocUtils.GetContainsGenericParameters ((MethodBase) member)) {
					GenericParameters = DocUtils.GetGenericArguments ((MethodBase) member);
				}
			}
			else if (member is PropertyInfo) {
				Parameters = ((PropertyInfo) member).GetIndexParameters ();
			}
				
			if (member is MethodInfo) {
				ReturnType = ((MethodInfo) member).ReturnType;
			} else if (member is PropertyInfo) {
				ReturnType = ((PropertyInfo) member).PropertyType;
				ReturnIsReturn = false;
			}

			// no remarks section for enum members
			if (member.DeclaringType != null && member.DeclaringType.IsEnum)
				AddRemarks = false;
			SetSlashDocs (member);
		}

		private void SetSlashDocs (MemberInfo member)
		{
			if (slashdocs == null)
				return;

			string slashdocsig = slashdocFormatter.GetDeclaration (member);
			if (slashdocsig != null)
				SlashDocs = slashdocs.SelectSingleNode ("doc/members/member[@name='" + slashdocsig + "']");
		}

		public Type ReturnType;
		public Type[] GenericParameters;
		public ParameterInfo[] Parameters;
		public bool ReturnIsReturn;
		public XmlElement Node;
		public bool AddRemarks = true;
		public XmlNode SlashDocs;
		public XmlReader EcmaDocs;
		public MemberInfo Member;
	}

	static string GetXPathForMember (DocumentationMember member)
	{
		StringBuilder xpath = new StringBuilder ();
		xpath.Append ("//Members/Member[@MemberName=\"")
			.Append (member.MemberName)
			.Append ("\"]");
		if (member.Parameters != null && member.Parameters.Count > 0) {
			xpath.Append ("/Parameters[count(Parameter) = ")
				.Append (member.Parameters.Count);
			for (int i = 0; i < member.Parameters.Count; ++i) {
				xpath.Append (" and Parameter [").Append (i+1).Append ("]/@Type=\"");
				xpath.Append (member.Parameters [i]);
				xpath.Append ("\"");
			}
			xpath.Append ("]/..");
		}
		return xpath.ToString ();
	}

	public static string GetXPathForMember (XPathNavigator member)
	{
		StringBuilder xpath = new StringBuilder ();
		xpath.Append ("//Type[@FullName=\"")
			.Append (SelectSingleNode (member, "../../@FullName").Value)
			.Append ("\"]/");
		xpath.Append ("Members/Member[@MemberName=\"")
			.Append (SelectSingleNode (member, "@MemberName").Value)
			.Append ("\"]");
		XPathNodeIterator parameters = member.Select ("Parameters/Parameter");
		if (parameters.Count > 0) {
			xpath.Append ("/Parameters[count(Parameter) = ")
				.Append (parameters.Count);
			int i = 0;
			while (parameters.MoveNext ()) {
				++i;
				xpath.Append (" and Parameter [").Append (i).Append ("]/@Type=\"");
				xpath.Append (parameters.Current.Value);
				xpath.Append ("\"");
			}
			xpath.Append ("]/..");
		}
		return xpath.ToString ();
	}

	public static string GetXPathForMember (MemberInfo member)
	{
		StringBuilder xpath = new StringBuilder ();
		xpath.Append ("//Type[@FullName=\"")
			.Append (member.DeclaringType.FullName)
			.Append ("\"]/");
		xpath.Append ("Members/Member[@MemberName=\"")
			.Append (GetMemberName (member))
			.Append ("\"]");

		ParameterInfo[] parameters = null;
		if (member is MethodBase)
			parameters = ((MethodBase) member).GetParameters ();
		else if (member is PropertyInfo) {
			parameters = ((PropertyInfo) member).GetIndexParameters ();
		}
		if (parameters != null && parameters.Length > 0) {
			xpath.Append ("/Parameters[count(Parameter) = ")
				.Append (parameters.Length);
			for (int i = 0; i < parameters.Length; ++i) {
				xpath.Append (" and Parameter [").Append (i+1).Append ("]/@Type=\"");
				xpath.Append (GetDocParameterType (parameters [i].ParameterType));
				xpath.Append ("\"");
			}
			xpath.Append ("]/..");
		}
		return xpath.ToString ();
	}
}

static class DocUtils {
	public static bool GetContainsGenericParameters (Type type)
	{
#if NET_1_0
		return false;
#else
		return type.ContainsGenericParameters;
#endif
	}

	public static bool GetContainsGenericParameters (MethodBase mb)
	{
#if NET_1_0
		return false;
#else
		return mb.ContainsGenericParameters;
#endif
	}

	public static Type[] GetGenericArguments (Type type)
	{
#if NET_1_0
		return new Type [0];
#else
		return type.GetGenericArguments ();
#endif
	}

	public static Type[] GetGenericArguments (MethodBase mb)
	{
#if NET_1_0
		return new Type [0];
#else
		return mb.GetGenericArguments ();
#endif
	}

	public static Type GetGenericTypeDefinition (Type type)
	{
#if NET_1_0
		return null;
#else
		return type.GetGenericTypeDefinition ();
#endif
	}

	public static Type[] GetGenericParameterConstraints (Type type)
	{
#if NET_1_0
		return null;
#else
		return type.GetGenericParameterConstraints ();
#endif
	}

	public static bool IsGenericType (Type type)
	{
#if NET_1_0
		return false;
#else
		return type.IsGenericType;
#endif
	}

	public static bool IsGenericParameter (Type type)
	{
#if NET_1_0
		return false;
#else
		return type.IsGenericParameter;
#endif
	}

	public static bool IsExplicitlyImplemented (MethodBase method)
	{
		return method.IsPrivate && method.IsFinal && method.IsVirtual;
	}

	public static string GetTypeDotMember (string name)
	{
		int startType, startMethod;
		startType = startMethod = -1;
		for (int i = 0; i < name.Length; ++i) {
			if (name [i] == '.') {
				startType = startMethod;
				startMethod = i;
			}
		}
		return name.Substring (startType+1);
	}

	public static string GetMember (string name)
	{
		int i = name.LastIndexOf ('.');
		if (i == -1)
			return name;
		return name.Substring (i+1);
	}

	public static void GetInfoForExplicitlyImplementedMethod (
			MethodBase method, out Type iface, out MethodInfo ifaceMethod)
	{
		Type declType = method.DeclaringType;
		foreach (Type declIface in declType.GetInterfaces ()) {
			InterfaceMapping map = declType.GetInterfaceMap (declIface);
			for (int i = 0; i < map.TargetMethods.Length; ++i)
				if (method == map.TargetMethods [i]) {
					iface       = map.InterfaceType;
					ifaceMethod = map.InterfaceMethods [i];
					return;
				}
		}
		throw new InvalidOperationException ("Could not determine interface type for explicitly-implemented interface member " + method.Name);
	}

	public static string[] ToStringArray (StringList list)
	{
#if NET_1_0
		return (string[]) list.ToArray (typeof(string));
#else
		return list.ToArray ();
#endif
	}

	public static string GetPropertyName (PropertyInfo pi)
	{
		// Issue: (g)mcs-generated assemblies that explicitly implement
		// properties don't specify the full namespace, just the 
		// TypeName.Property; .NET uses Full.Namespace.TypeName.Property.
		MethodInfo method = pi.GetGetMethod (true);
		if (method == null)
			method = pi.GetSetMethod (true);
		if (!IsExplicitlyImplemented (method))
			return pi.Name;

		// Need to determine appropriate namespace for this member.
		Type iface;
		MethodInfo ifaceMethod;
		GetInfoForExplicitlyImplementedMethod (method, out iface, out ifaceMethod);
		return string.Join (".", new string[]{
				DocTypeFullMemberFormatter.Default.GetName (iface),
				GetMember (pi.Name)});
	}

	public static string PathCombine (string dir, string path)
	{
		if (dir == null)
			dir = "";
		if (path == null)
			path = "";
		return Path.Combine (dir, path);
	}

	public static bool IsExtensionMethod (MethodBase method)
	{
#if NET_1_0
		return false;
#else
		return 
			method.GetCustomAttributes (
					typeof(System.Runtime.CompilerServices.ExtensionAttribute), 
					false).Length != 0 &&
			method.DeclaringType.GetCustomAttributes (
					typeof(System.Runtime.CompilerServices.ExtensionAttribute), 
					false).Length != 0;
#endif
	}
}

class DocumentationMember {
	public StringToStringMap MemberSignatures = new StringToStringMap ();
	public string ReturnType;
	public StringList Parameters;
	public string MemberName;
	public string MemberType;

	public DocumentationMember (XmlReader reader)
	{
		MemberName = reader.GetAttribute ("MemberName");
		int depth = reader.Depth;
		bool go = true;
		StringList p = new StringList ();
		do {
			if (reader.NodeType != XmlNodeType.Element)
				continue;
			switch (reader.Name) {
				case "MemberSignature":
					MemberSignatures [reader.GetAttribute ("Language")] = reader.GetAttribute ("Value");
					break;
				case "MemberType":
					MemberType = reader.ReadElementString ();
					break;
				case "ReturnType":
					if (reader.Depth == depth + 2)
						ReturnType = reader.ReadElementString ();
					break;
				case "Parameter":
					if (reader.Depth == depth + 2)
						p.Add (reader.GetAttribute ("Type"));
					break;
				case "Docs":
					if (reader.Depth == depth + 1)
						go = false;
					break;
			}
		} while (go && reader.Read () && reader.Depth >= depth);
		if (p.Count > 0) {
			Parameters = p;
		}
	}

	public DocumentationMember (XmlNode node)
	{
		MemberName = node.Attributes ["MemberName"].Value;
		foreach (XmlNode n in node.SelectNodes ("MemberSignature")) {
			XmlAttribute l = n.Attributes ["Language"];
			XmlAttribute v = n.Attributes ["Value"];
			if (l != null && v != null)
				MemberSignatures [l.Value] = v.Value;
		}
		MemberType = node.SelectSingleNode ("MemberType").InnerText;
		XmlNode rt = node.SelectSingleNode ("ReturnValue/ReturnType");
		if (rt != null)
			ReturnType = rt.InnerText;
		XmlNodeList p = node.SelectNodes ("Parameters/Parameter");
		if (p.Count > 0) {
			Parameters = new StringList (p.Count);
			for (int i = 0; i < p.Count; ++i)
				Parameters.Add (p [i].Attributes ["Type"].Value);
		}
	}
}

public abstract class MemberFormatter {
	public string GetName (MemberInfo member)
	{
		Type type = member as Type;
		if (type != null)
			return GetTypeName (type);
		ConstructorInfo ctor = member as ConstructorInfo;
		if (ctor != null)
			return GetConstructorName (ctor);
		MethodInfo method = member as MethodInfo;
		if (method != null)
			return GetMethodName (method);
		PropertyInfo prop = member as PropertyInfo;
		if (prop != null)
			return GetPropertyName (prop);
		FieldInfo field = member as FieldInfo;
		if (field != null)
			return GetFieldName (field);
		EventInfo e = member as EventInfo;
		if (e != null)
			return GetEventName (e);
		throw new NotSupportedException ("Can't handle: " + member.GetType().ToString());
	}

	protected virtual string GetTypeName (Type type)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		return _AppendTypeName (new StringBuilder (type.Name.Length), type).ToString ();
	}

	protected virtual char[] ArrayDelimeters {
		get {return new char[]{'[', ']'};}
	}

	protected StringBuilder _AppendTypeName (StringBuilder buf, Type type)
	{
		if (type.IsArray) {
			_AppendTypeName (buf, type.GetElementType ()).Append (ArrayDelimeters [0]);
			int rank = type.GetArrayRank ();
			if (rank > 1)
				buf.Append (new string (',', rank-1));
			return buf.Append (ArrayDelimeters [1]);
		}
		if (type.IsByRef) {
			return AppendRefTypeName (buf, type);
		}
		if (type.IsPointer) {
			return AppendPointerTypeName (buf, type);
		}
		AppendNamespace (buf, type);
		if (DocUtils.IsGenericParameter (type)) {
			return AppendTypeName (buf, type);
		}
		if (!DocUtils.IsGenericType (type)) {
			return AppendFullTypeName (buf, type);
		}
		return AppendGenericType (buf, type);
	}

	protected virtual StringBuilder AppendNamespace (StringBuilder buf, Type type)
	{
		if (type.Namespace != null && type.Namespace.Length > 0)
			buf.Append (type.Namespace).Append ('.');
		return buf;
	}

	private StringBuilder AppendFullTypeName (StringBuilder buf, Type type)
	{
		if (type.DeclaringType != null)
			AppendFullTypeName (buf, type.DeclaringType).Append (NestedTypeSeparator);
		return AppendTypeName (buf, type);
	}

	protected virtual StringBuilder AppendTypeName (StringBuilder buf, Type type)
	{
		return AppendTypeName (buf, type.Name);
	}

	protected virtual StringBuilder AppendTypeName (StringBuilder buf, string typename)
	{
		int n = typename.IndexOf ("`");
		if (n >= 0)
			return buf.Append (typename.Substring (0, n));
		return buf.Append (typename);
	}

	protected virtual string RefTypeModifier {
		get {return "@";}
	}

	protected virtual StringBuilder AppendRefTypeName (StringBuilder buf, Type type)
	{
		return _AppendTypeName (buf, type.GetElementType ()).Append (RefTypeModifier);
	}

	protected virtual string PointerModifier {
		get {return "*";}
	}

	protected virtual StringBuilder AppendPointerTypeName (StringBuilder buf, Type type)
	{
		return _AppendTypeName (buf, type.GetElementType ()).Append (PointerModifier);
	}

	protected virtual char[] GenericTypeContainer {
		get {return new char[]{'<', '>'};}
	}

	protected virtual char NestedTypeSeparator {
		get {return '.';}
	}

	protected virtual StringBuilder AppendGenericType (StringBuilder buf, Type type)
	{
		Type[] genArgs = DocUtils.GetGenericArguments (type);
		int genArg = 0;
		if (type.DeclaringType != null) {
			AppendTypeName (buf, type.DeclaringType);
			if (DocUtils.IsGenericType (type.DeclaringType)) {
				buf.Append (GenericTypeContainer [0]);
				int max = DocUtils.GetGenericArguments (type.DeclaringType).Length;
				_AppendTypeName (buf, genArgs [genArg++]);
				while (genArg < max) {
					buf.Append (",");
					_AppendTypeName (buf, genArgs [genArg++]);
				}
				buf.Append (GenericTypeContainer [1]);
			}
			buf.Append (NestedTypeSeparator);
		}
		AppendTypeName (buf, type);
		if (genArg < genArgs.Length) {
			buf.Append (GenericTypeContainer [0]);
			_AppendTypeName (buf, genArgs [genArg++]);
			while (genArg < genArgs.Length) {
				buf.Append (",");
				_AppendTypeName (buf, genArgs [genArg++]);
			}
			buf.Append (GenericTypeContainer [1]);
		}
		return buf;
	}

	protected virtual StringBuilder AppendGenericTypeConstraints (StringBuilder buf, Type type)
	{
		return buf;
	}

	protected virtual string GetConstructorName (ConstructorInfo constructor)
	{
		return constructor.Name;
	}

	protected virtual string GetMethodName (MethodInfo method)
	{
		return method.Name;
	}

	protected virtual string GetPropertyName (PropertyInfo property)
	{
		return property.Name;
	}

	protected virtual string GetFieldName (FieldInfo field)
	{
		return field.Name;
	}

	protected virtual string GetEventName (EventInfo e)
	{
		return e.Name;
	}

	public string GetDeclaration (MemberInfo member)
	{
		Type type = member as Type;
		if (type != null)
			return GetTypeDeclaration (type);
		ConstructorInfo ctor = member as ConstructorInfo;
		if (ctor != null)
			return GetConstructorDeclaration (ctor);
		MethodInfo method = member as MethodInfo;
		if (method != null)
			return GetMethodDeclaration (method);
		PropertyInfo prop = member as PropertyInfo;
		if (prop != null)
			return GetPropertyDeclaration (prop);
		FieldInfo field = member as FieldInfo;
		if (field != null)
			return GetFieldDeclaration (field);
		EventInfo e = member as EventInfo;
		if (e != null)
			return GetEventDeclaration (e);
		throw new NotSupportedException ("Can't handle: " + member.GetType().ToString());
	}

	protected virtual string GetTypeDeclaration (Type type)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		StringBuilder buf = new StringBuilder (type.Name.Length);
		_AppendTypeName (buf, type);
		AppendGenericTypeConstraints (buf, type);
		return buf.ToString ();
	}

	protected virtual string GetConstructorDeclaration (ConstructorInfo constructor)
	{
		return GetConstructorName (constructor);
	}

	protected virtual string GetMethodDeclaration (MethodInfo method)
	{
		// Special signature for destructors.
		if (method.Name == "Finalize" && method.GetParameters().Length == 0)
			return GetFinalizerName (method);

		StringBuilder buf = new StringBuilder ();

		AppendVisibility (buf, method);
		if (buf.Length == 0 && 
				!(DocUtils.IsExplicitlyImplemented (method) && !method.IsSpecialName))
			return null;

		AppendModifiers (buf, method);

		if (buf.Length != 0)
			buf.Append (" ");
		buf.Append (GetName (method.ReturnType)).Append (" ");

		AppendMethodName (buf, method);
		AppendGenericMethod (buf, method).Append (" ");
		AppendParameters (buf, method, method.GetParameters ());
		AppendGenericMethodConstraints (buf, method);
		return buf.ToString ();
	}

	protected virtual StringBuilder AppendMethodName (StringBuilder buf, MethodBase method)
	{
		return buf.Append (method.Name);
	}

	protected virtual string GetFinalizerName (MethodInfo method)
	{
		return "Finalize";
	}

	protected virtual StringBuilder AppendVisibility (StringBuilder buf, MethodBase method)
	{
		return buf;
	}

	protected virtual StringBuilder AppendModifiers (StringBuilder buf, MethodInfo method)
	{
		return buf;
	}

	protected virtual StringBuilder AppendGenericMethod (StringBuilder buf, MethodInfo method)
	{
		return buf;
	}

	protected virtual StringBuilder AppendParameters (StringBuilder buf, MethodBase method, ParameterInfo[] parameters)
	{
		return buf;
	}

	protected virtual StringBuilder AppendGenericMethodConstraints (StringBuilder buf, MethodInfo method)
	{
		return buf;
	}

	protected virtual string GetPropertyDeclaration (PropertyInfo property)
	{
		return GetPropertyName (property);
	}

	protected virtual string GetFieldDeclaration (FieldInfo field)
	{
		return GetFieldName (field);
	}

	protected virtual string GetEventDeclaration (EventInfo e)
	{
		return GetEventName (e);
	}
}

class CSharpFullMemberFormatter : MemberFormatter {

	protected override StringBuilder AppendNamespace (StringBuilder buf, Type type)
	{
		if (GetCSharpType (type.FullName) == null && type.Namespace != null && type.Namespace.Length > 0 && type.Namespace != "System")
			buf.Append (type.Namespace).Append ('.');
		return buf;
	}

	private string GetCSharpType (string t)
	{
		switch (t) {
		case "System.Byte":    return "byte";
		case "System.SByte":   return "sbyte";
		case "System.Int16":   return "short";
		case "System.Int32":   return "int";
		case "System.Int64":   return "long";

		case "System.UInt16":  return "ushort";
		case "System.UInt32":  return "uint";
		case "System.UInt64":  return "ulong";

		case "System.Single":  return "float";
		case "System.Double":  return "double";
		case "System.Decimal": return "decimal";
		case "System.Boolean": return "bool";
		case "System.Char":    return "char";
		case "System.Void":    return "void";
		case "System.String":  return "string";
		case "System.Object":  return "object";
		}
		return null;
	}

	protected override StringBuilder AppendTypeName (StringBuilder buf, Type type)
	{
		if (DocUtils.IsGenericParameter (type))
			return buf.Append (type.Name);
		string t = type.FullName;
		if (!t.StartsWith ("System.")) {
			return base.AppendTypeName (buf, type);
		}

		string s = GetCSharpType (t);
		if (s != null)
			return buf.Append (s);
		
		return base.AppendTypeName (buf, type);
	}

	protected override string GetTypeDeclaration (Type type)
	{
		string visibility = GetTypeVisibility (type.Attributes);
		if (visibility == null)
			return null;

		StringBuilder buf = new StringBuilder ();
		
		buf.Append (visibility);
		buf.Append (" ");

		MemberFormatter full = new CSharpFullMemberFormatter ();

		if (IsDelegate(type)) {
			buf.Append("delegate ");
			MethodInfo invoke = type.GetMethod ("Invoke");
			buf.Append (full.GetName (invoke.ReturnType)).Append (" ");
			buf.Append (GetName (type));
			AppendParameters (buf, invoke, invoke.GetParameters ());
			AppendGenericTypeConstraints (buf, type);
			buf.Append (";");

			return buf.ToString();
		}
		
		if (type.IsAbstract && !type.IsInterface)
			buf.Append("abstract ");
		if (type.IsSealed && !IsDelegate(type) && !type.IsValueType)
			buf.Append("sealed ");
		buf.Replace ("abstract sealed", "static");

		buf.Append (GetTypeKind (type));
		buf.Append (" ");
		buf.Append (GetCSharpType (type.FullName) == null 
				? GetName (type) 
				: type.Name);

		if (!type.IsEnum) {
			Type basetype = type.BaseType;
			if (basetype == typeof(object) || type.IsValueType) // don't show this in signatures
				basetype = null;
			
			ArrayList interface_names = new ArrayList ();
			foreach (Type i in type.GetInterfaces ())
				if ((type.BaseType == null || Array.IndexOf (type.BaseType.GetInterfaces (), i) == -1) && 
						InterfaceNotFromAnother (i, type.GetInterfaces ()))
					interface_names.Add (full.GetName (i));
			interface_names.Sort ();
			
			if (basetype != null || interface_names.Count > 0)
				buf.Append (" : ");
			
			if (basetype != null) {
				buf.Append (full.GetName (basetype));
				if (interface_names.Count > 0)
					buf.Append (", ");
			}
			
			for (int i = 0; i < interface_names.Count; i++){
				if (i != 0)
					buf.Append (", ");
				buf.Append (interface_names [i]);
			}
			AppendGenericTypeConstraints (buf, type);
		}

		return buf.ToString ();
	}

	static string GetTypeKind (Type t)
	{
		if (t.IsEnum)
			return "enum";
		if (t.IsClass || t == typeof(System.Enum))
			return "class";
		if (t.IsInterface)
			return "interface";
		if (t.IsValueType)
			return "struct";
		throw new ArgumentException(t.FullName);
	}

	static string GetTypeVisibility (TypeAttributes ta)
	{
		switch (ta & TypeAttributes.VisibilityMask) {
		case TypeAttributes.Public:
		case TypeAttributes.NestedPublic:
			return "public";

		case TypeAttributes.NestedFamily:
		case TypeAttributes.NestedFamORAssem:
			return "protected";

		default:
			return null;
		}
	}

	static bool IsDelegate(Type type)
	{
		return typeof (System.Delegate).IsAssignableFrom (type) && !type.IsAbstract;
	}
	
	private static bool InterfaceNotFromAnother(Type i, Type[] i2)
	{
		foreach (Type t in i2)
			if (i != t && Array.IndexOf (t.GetInterfaces(), i) != -1)
				return false;
		return true;
	}

	protected override StringBuilder AppendGenericTypeConstraints (StringBuilder buf, Type type)
	{
		if (!DocUtils.GetContainsGenericParameters (type))
			return buf;
		return AppendConstraints (buf, DocUtils.GetGenericArguments (type));
	}

	private StringBuilder AppendConstraints (StringBuilder buf, Type[] genArgs)
	{
#if !NET_1_0
		foreach (Type genArg in genArgs) {
			GenericParameterAttributes attrs = genArg.GenericParameterAttributes;
			Type[] constraints = genArg.GetGenericParameterConstraints ();
			if (attrs == GenericParameterAttributes.None && constraints.Length == 0)
				continue;
			buf.Append (" where ").Append (genArg.Name).Append (" : ");
			bool isref = (attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
			bool isvt  = (attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
			bool isnew = (attrs & GenericParameterAttributes.DefaultConstructorConstraint) != 0;
			bool comma = false;
			if (isref) {
				buf.Append ("class");
				comma = true;
			}
			else if (isvt) {
				buf.Append ("struct");
				comma = true;
			}
			if (constraints.Length > 0 && !isvt) {
				if (comma)
					buf.Append (", ");
				buf.Append (GetTypeName (constraints [0]));
				for (int i = 1; i < constraints.Length; ++i)
					buf.Append (", ").Append (GetTypeName (constraints [i]));
			}
			if (isnew && !isvt) {
				if (comma)
					buf.Append (", ");
				buf.Append ("new()");
			}
		}
#endif
		return buf;
	}

	protected override string GetConstructorDeclaration (ConstructorInfo constructor)
	{
		StringBuilder buf = new StringBuilder ();
		AppendVisibility (buf, constructor);
		if (buf.Length == 0)
			return null;

		buf.Append (' ');
		base.AppendTypeName (buf, constructor.DeclaringType.Name).Append (' ');
		AppendParameters (buf, constructor, constructor.GetParameters ());
		buf.Append (';');

		return buf.ToString ();
	}
	
	protected override string GetMethodDeclaration (MethodInfo method)
	{
		string decl = base.GetMethodDeclaration (method);
		if (decl != null)
			return decl + ";";
		return null;
	}

	protected override StringBuilder AppendMethodName (StringBuilder buf, MethodBase method)
	{
		if (DocUtils.IsExplicitlyImplemented (method)) {
			Type iface;
			MethodInfo ifaceMethod;
			DocUtils.GetInfoForExplicitlyImplementedMethod (method, out iface, out ifaceMethod);
			return buf.Append (new CSharpMemberFormatter ().GetName (iface))
				.Append ('.')
				.Append (ifaceMethod.Name);
		}
		return base.AppendMethodName (buf, method);
	}

	protected override StringBuilder AppendGenericMethodConstraints (StringBuilder buf, MethodInfo method)
	{
		if (!DocUtils.GetContainsGenericParameters (method))
			return buf;
		return AppendConstraints (buf, DocUtils.GetGenericArguments (method));
	}

	protected override string RefTypeModifier {
		get {return "";}
	}

	protected override string GetFinalizerName (MethodInfo method)
	{
		return "~" + method.DeclaringType.Name + " ()";	
	}

	protected override StringBuilder AppendVisibility (StringBuilder buf, MethodBase method)
	{
		if (method == null)
			return buf;
		if (method.IsPublic)
			return buf.Append ("public");
		if (method.IsFamily || method.IsFamilyOrAssembly)
			return buf.Append ("protected");
		return buf;
	}

	protected override StringBuilder AppendModifiers (StringBuilder buf, MethodInfo method)
	{
		string modifiers = String.Empty;
		if (method.IsStatic) modifiers += " static";
		if (method.IsVirtual && !method.IsAbstract) {
			if ((method.Attributes & MethodAttributes.NewSlot) != 0) modifiers += " virtual";
			else modifiers += " override";
		}
		if (method.IsAbstract && !method.DeclaringType.IsInterface) modifiers += " abstract";
		if (method.IsFinal) modifiers += " sealed";
		if (modifiers == " virtual sealed") modifiers = "";

		return buf.Append (modifiers);
	}

	protected override StringBuilder AppendGenericMethod (StringBuilder buf, MethodInfo method)
	{
		if (DocUtils.GetContainsGenericParameters (method)) {
			Type[] args = DocUtils.GetGenericArguments (method);
			if (args.Length > 0) {
				buf.Append ("<");
				buf.Append (args [0].Name);
				for (int i = 1; i < args.Length; ++i)
					buf.Append (",").Append (args [i].Name);
				buf.Append (">");
			}
		}
		return buf;
	}

	protected override StringBuilder AppendParameters (StringBuilder buf, MethodBase method, ParameterInfo[] parameters)
	{
		return AppendParameters (buf, method, parameters, '(', ')');
	}

	private StringBuilder AppendParameters (StringBuilder buf, MethodBase method, ParameterInfo[] parameters, char begin, char end)
	{
		buf.Append (begin);

		if (parameters.Length > 0) {
			if (DocUtils.IsExtensionMethod (method))
				buf.Append ("this ");
			AppendParameter (buf, parameters [0]);
			for (int i = 1; i < parameters.Length; ++i) {
				buf.Append (", ");
				AppendParameter (buf, parameters [i]);
			}
		}

		return buf.Append (end);
	}

	private StringBuilder AppendParameter (StringBuilder buf, ParameterInfo parameter)
	{
		if (parameter.ParameterType.IsByRef) {
			if (parameter.IsOut)
				buf.Append ("out ");
			else
				buf.Append ("ref ");
		}
		buf.Append (GetName (parameter.ParameterType)).Append (" ");
		return buf.Append (parameter.Name);
	}

	protected override string GetPropertyDeclaration (PropertyInfo property)
	{
		MethodInfo method;

		string get_visible = null;
		if ((method = property.GetGetMethod (true)) != null && 
				(DocUtils.IsExplicitlyImplemented (method) || 
				 (!method.IsPrivate && !method.IsAssembly && !method.IsFamilyAndAssembly)))
			get_visible = AppendVisibility (new StringBuilder (), method).ToString ();
		string set_visible = null;
		if ((method = property.GetSetMethod (true)) != null &&
				(DocUtils.IsExplicitlyImplemented (method) || 
				 (!method.IsPrivate && !method.IsAssembly && !method.IsFamilyAndAssembly)))
			set_visible = AppendVisibility (new StringBuilder (), method).ToString ();

		if ((set_visible == null) && (get_visible == null))
			return null;

		string visibility;
		StringBuilder buf = new StringBuilder ();
		if (get_visible != null && (set_visible == null || (set_visible != null && get_visible == set_visible)))
			buf.Append (visibility = get_visible);
		else if (set_visible != null && get_visible == null)
			buf.Append (visibility = set_visible);
		else
			buf.Append (visibility = "public");

		// Pick an accessor to use for static/virtual/override/etc. checks.
		method = property.GetSetMethod (true);
		if (method == null)
			method = property.GetGetMethod (true);
	
		string modifiers = String.Empty;
		if (method.IsStatic) modifiers += " static";
		if (method.IsVirtual && !method.IsAbstract) {
				if ((method.Attributes & MethodAttributes.NewSlot) != 0)
					modifiers += " virtual";
				else
					modifiers += " override";
		}
		if (method.IsAbstract && !method.DeclaringType.IsInterface)
			modifiers += " abstract";
		if (method.IsFinal)
			modifiers += " sealed";
		if (modifiers == " virtual sealed")
			modifiers = "";
		buf.Append (modifiers).Append (' ');

		buf.Append (GetName (property.PropertyType)).Append (' ');
	
		MemberInfo[] defs = property.DeclaringType.GetDefaultMembers ();
		string name = property.Name;
		foreach (MemberInfo mi in defs) {
			if (mi == property) {
				name = "this";
				break;
			}
		}
		buf.Append (name == "this" ? name : DocUtils.GetPropertyName (property));
	
		if (property.GetIndexParameters ().Length != 0) {
			AppendParameters (buf, method, property.GetIndexParameters (), '[', ']');
		}

		buf.Append (" {");
		if (set_visible != null) {
			if (set_visible != visibility)
				buf.Append (' ').Append (set_visible);
			buf.Append (" set;");
		}
		if (get_visible != null) {
			if (get_visible != visibility)
				buf.Append (' ').Append (get_visible);
			buf.Append (" get;");
		}
		buf.Append (" }");
	
		return buf [0] != ' ' ? buf.ToString () : buf.ToString (1, buf.Length-1);
	}

	protected override string GetFieldDeclaration (FieldInfo field)
	{
		if (field.DeclaringType.IsEnum && field.Name == "value__")
			return null; // This member of enums aren't documented.

		StringBuilder buf = new StringBuilder ();
		AppendFieldVisibility (buf, field);
		if (buf.Length == 0)
			return null;

		if (field.DeclaringType.IsEnum)
			return field.Name;

		if (field.IsStatic && !field.IsLiteral)
			buf.Append (" static");
		if (field.IsInitOnly)
			buf.Append (" readonly");
		if (field.IsLiteral)
			buf.Append (" const");

		buf.Append (' ').Append (GetName (field.FieldType)).Append (' ');
		buf.Append (field.Name);
		AppendFieldValue (buf, field);
		buf.Append (';');

		return buf.ToString ();
	}

	static StringBuilder AppendFieldVisibility (StringBuilder buf, FieldInfo field)
	{
		if (field.IsPublic)
			return buf.Append ("public");
		if (field.IsFamily || field.IsFamilyOrAssembly)
			return buf.Append ("protected");
		return buf;
	}

	static StringBuilder AppendFieldValue (StringBuilder buf, FieldInfo field)
	{
		// enums have a value__ field, which we ignore, and FieldInfo.GetValue()
		// on a GenericType results in InvalidOperationException
		if (field.DeclaringType.IsEnum || 
				DocUtils.IsGenericType (field.DeclaringType))
			return buf;
		if (field.IsLiteral || (field.IsStatic && field.IsInitOnly)) {
			object val = null;
			try {
				val   = field.GetValue (null);
			} catch {
				return buf;
			}
			if (val == null)
				buf.Append (" = ").Append ("null");
			else if (val is Enum)
				buf.Append (" = ").Append (val.ToString ());
			else if (val is IFormattable) {
				string value = ((IFormattable)val).ToString();
				if (val is string)
					value = "\"" + value + "\"";
				buf.Append (" = ").Append (value);
			}
		}
		return buf;
	}

	protected override string GetEventDeclaration (EventInfo e)
	{
		StringBuilder buf = new StringBuilder ();
		if (AppendVisibility (buf, e.GetAddMethod (true)).Length == 0) {
			return null;
		}

		AppendModifiers (buf, e.GetAddMethod (true));

		buf.Append (" event ");
		buf.Append (GetName (e.EventHandlerType)).Append (' ');
		buf.Append (e.Name).Append (';');

		return buf.ToString ();
	}
}

class CSharpMemberFormatter : CSharpFullMemberFormatter {
	protected override StringBuilder AppendNamespace (StringBuilder buf, Type type)
	{
		return buf;
	}
}

class DocTypeFullMemberFormatter : MemberFormatter {
	public static readonly MemberFormatter Default = new DocTypeFullMemberFormatter ();

	protected override char NestedTypeSeparator {
		get {return '+';}
	}
}

class DocTypeMemberFormatter : DocTypeFullMemberFormatter {
	protected override StringBuilder AppendNamespace (StringBuilder buf, Type type)
	{
		return buf;
	}
}

class SlashDocMemberFormatter : MemberFormatter {

	protected override char[] GenericTypeContainer {
		get {return new char[]{'{', '}'};}
	}

	private bool AddTypeCount = true;

	protected override string GetTypeName (Type type)
	{
		return base.GetTypeName (type);
	}

	private Type genDeclType;
	private MethodBase genDeclMethod;

	protected override StringBuilder AppendTypeName (StringBuilder buf, Type type)
	{
		if (DocUtils.IsGenericParameter (type)) {
			int l = buf.Length;
			if (genDeclType != null) {
				Type[] genArgs = DocUtils.GetGenericArguments (genDeclType);
				for (int i = 0; i < genArgs.Length; ++i) {
					if (genArgs [i].Name == type.Name) {
						buf.Append ('`').Append (i);
						break;
					}
				}
			}
			if (genDeclMethod != null) {
				Type[] genArgs = null;
				if (DocUtils.GetContainsGenericParameters (genDeclMethod)) {
					genArgs = DocUtils.GetGenericArguments (genDeclMethod);
				}
				else
					genArgs = new Type[0];
				for (int i = 0; i < genArgs.Length; ++i) {
					if (genArgs [i].Name == type.Name) {
						buf.Append ("``").Append (i);
						break;
					}
				}
			}
			if (genDeclType == null && genDeclMethod == null) {
				// Probably from within an explicitly implemented interface member,
				// where CSC uses parameter names instead of indices (why?), e.g.
				// MyList`2.Mono#DocTest#Generic#IFoo{A}#Method``1(`0,``0) instead of
				// MyList`2.Mono#DocTest#Generic#IFoo{`0}#Method``1(`0,``0).
				buf.Append (type.Name);
			}
			if (buf.Length == l) {
				throw new Exception (string.Format (
						"Unable to translate generic parameter {0}; genDeclType={1}, genDeclMethod={2}", 
						type.Name, genDeclType, genDeclMethod));
			}
		}
		else {
			base.AppendTypeName (buf, type);
			if (AddTypeCount) {
				int numArgs = DocUtils.GetGenericArguments (type).Length;
				if (type.DeclaringType != null)
					numArgs -= DocUtils.GetGenericArguments (type).Length;
				if (numArgs > 0) {
					buf.Append ('`').Append (numArgs);
				}
			}
		}
		return buf;
	}

	protected override StringBuilder AppendGenericType (StringBuilder buf, Type type)
	{
		if (!AddTypeCount)
			base.AppendGenericType (buf, type);
		else
			AppendType (buf, type);
		return buf;
	}

	private StringBuilder AppendType (StringBuilder buf, Type type)
	{
		int numArgs = DocUtils.GetGenericArguments (type).Length;
		if (type.DeclaringType != null) {
			AppendType (buf, type.DeclaringType).Append (NestedTypeSeparator);
			numArgs -= DocUtils.GetGenericArguments (type.DeclaringType).Length;
		}
		base.AppendTypeName (buf, type);
		if (numArgs > 0) {
			buf.Append ('`').Append (numArgs);
		}
		return buf;
	}

	protected override string GetConstructorName (ConstructorInfo constructor)
	{
		return GetMethodBaseName (constructor, "#ctor");
	}

	protected override string GetMethodName (MethodInfo method)
	{
		string name = null;
		if (!DocUtils.IsExplicitlyImplemented (method))
			name = method.Name;
		else {
			Type iface;
			MethodInfo ifaceMethod;
			DocUtils.GetInfoForExplicitlyImplementedMethod (method, out iface, out ifaceMethod);
			AddTypeCount = false;
			name = GetTypeName (iface) + "." + ifaceMethod.Name;
			AddTypeCount = true;
		}
		return GetMethodBaseName (method, name);
	}

	private string GetMethodBaseName (MethodBase method, string name)
	{
		StringBuilder buf = new StringBuilder ();
		buf.Append (GetTypeName (method.DeclaringType));
		buf.Append ('.');
		buf.Append (name.Replace (".", "#"));
		if (DocUtils.GetContainsGenericParameters (method)) {
			Type[] genArgs = DocUtils.GetGenericArguments (method);
			if (genArgs.Length > 0)
				buf.Append ("``").Append (genArgs.Length);
		}
		ParameterInfo[] parameters = method.GetParameters ();
		genDeclType = method.DeclaringType;
		genDeclMethod = method;
		AppendParameters (buf, DocUtils.GetGenericArguments (method.DeclaringType), parameters);
		genDeclType = null;
		genDeclMethod = null;
		return buf.ToString ();
	}

	private StringBuilder AppendParameters (StringBuilder buf, Type[] genArgs, ParameterInfo[] parameters)
	{
		if (parameters.Length == 0)
			return buf;

		buf.Append ('(');

		AppendParameter (buf, genArgs, parameters [0]);
		for (int i = 1; i < parameters.Length; ++i) {
			buf.Append (',');
			AppendParameter (buf, genArgs, parameters [i]);
		}

		return buf.Append (')');
	}

	private StringBuilder AppendParameter (StringBuilder buf, Type[] genArgs, ParameterInfo parameter)
	{
		AddTypeCount = false;
		buf.Append (GetTypeName (parameter.ParameterType));
		AddTypeCount = true;
		return buf;
	}

	protected override string GetPropertyName (PropertyInfo property)
	{
		string name = null;

		MethodInfo method = property.GetGetMethod (true);
		if (method == null)
			method = property.GetSetMethod (true);
		if (!DocUtils.IsExplicitlyImplemented (method))
			name = property.Name;
		else {
			Type iface;
			MethodInfo ifaceMethod;
			DocUtils.GetInfoForExplicitlyImplementedMethod (method, out iface, out ifaceMethod);
			AddTypeCount = false;
			name = string.Join ("#", new string[]{
					GetTypeName (iface).Replace (".", "#"),
					DocUtils.GetMember (property.Name)
			});
			AddTypeCount = true;
		}

		StringBuilder buf = new StringBuilder ();
		buf.Append (GetName (property.DeclaringType));
		buf.Append ('.');
		buf.Append (name);
		ParameterInfo[] parameters = property.GetIndexParameters ();
		if (parameters.Length > 0) {
			genDeclType = property.DeclaringType;
			buf.Append ('(');
			Type[] genArgs = DocUtils.GetGenericArguments (property.DeclaringType);
			AppendParameter (buf, genArgs, parameters [0]);
			for (int i = 1; i < parameters.Length; ++i) {
				 buf.Append (',');
				 AppendParameter (buf, genArgs, parameters [i]);
			}
			buf.Append (')');
			genDeclType = null;
		}
		return buf.ToString ();
	}

	protected override string GetFieldName (FieldInfo field)
	{
		return string.Format ("{0}.{1}",
			GetName (field.DeclaringType), field.Name);
	}

	protected override string GetEventName (EventInfo e)
	{
		return string.Format ("{0}.{1}",
			GetName (e.DeclaringType), e.Name);
	}

	protected override string GetTypeDeclaration (Type type)
	{
		string name = GetName (type);
		if (type == null)
			return null;
		return "T:" + name;
	}

	protected override string GetConstructorDeclaration (ConstructorInfo constructor)
	{
		string name = GetName (constructor);
		if (name == null)
			return null;
		return "M:" + name;
	}

	protected override string GetMethodDeclaration (MethodInfo method)
	{
		string name = GetName (method);
		if (name == null)
			return null;
		if (method.Name == "op_Implicit" || method.Name == "op_Explicit") {
			genDeclType = method.DeclaringType;
			genDeclMethod = method;
			name += "~" + GetName (method.ReturnType);
			genDeclType = null;
			genDeclMethod = null;
		}
		return "M:" + name;
	}

	protected override string GetPropertyDeclaration (PropertyInfo property)
	{
		string name = GetName (property);
		if (name == null)
			return null;
		return "P:" + name;
	}

	protected override string GetFieldDeclaration (FieldInfo field)
	{
		string name = GetName (field);
		if (name == null)
			return null;
		return "F:" + name;
	}

	protected override string GetEventDeclaration (EventInfo e)
	{
		string name = GetName (e);
		if (name == null)
			return null;
		return "E:" + name;
	}
}

class FileNameMemberFormatter : SlashDocMemberFormatter {
	protected override StringBuilder AppendNamespace (StringBuilder buf, Type type)
	{
		return buf;
	}

	protected override char NestedTypeSeparator {
		get {return '+';}
	}
}

}
