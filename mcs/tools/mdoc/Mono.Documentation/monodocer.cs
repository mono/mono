// Updater program for syncing Mono's ECMA-style documentation files
// with an assembly.
// By Joshua Tauberer <tauberer@for.net>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Mono.Cecil;
using Mono.Options;

using MyXmlNodeList        = System.Collections.Generic.List<System.Xml.XmlNode>;
using StringList           = System.Collections.Generic.List<string>;
using StringToStringMap    = System.Collections.Generic.Dictionary<string, string>;
using StringToXmlNodeMap   = System.Collections.Generic.Dictionary<string, System.Xml.XmlNode>;

namespace Mono.Documentation {

class MDocUpdater : MDocCommand
{
	string srcPath;
	List<AssemblyDefinition> assemblies;
	readonly DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
	
	bool delete;
	bool show_exceptions;
	bool no_assembly_versions, ignore_missing_types;
	ExceptionLocations? exceptions;
	
	internal int additions = 0, deletions = 0;

	List<DocumentationImporter> importers = new List<DocumentationImporter> ();

	DocumentationEnumerator docEnum;

	string since;

	static readonly MemberFormatter docTypeFormatter     = new DocTypeMemberFormatter ();
	static readonly MemberFormatter filenameFormatter    = new FileNameMemberFormatter ();

	static MemberFormatter[] typeFormatters = new MemberFormatter[]{
		new CSharpMemberFormatter (),
		new ILMemberFormatter (),
	};

	static MemberFormatter[] memberFormatters = new MemberFormatter[]{
		new CSharpFullMemberFormatter (),
		new ILFullMemberFormatter (),
	};

	internal static readonly MemberFormatter slashdocFormatter    = new SlashDocMemberFormatter ();

	MyXmlNodeList extensionMethods = new MyXmlNodeList ();

	public override void Run (IEnumerable<string> args)
	{
		show_exceptions = DebugOutput;
		string import = null;
		var types = new List<string> ();
		var p = new OptionSet () {
			{ "delete",
				"Delete removed members from the XML files.",
				v => delete = v != null },
			{ "exceptions:",
			  "Document potential exceptions that members can generate.  {SOURCES} " +
				"is a comma-separated list of:\n" +
				"  asm      Method calls in same assembly\n" +
				"  depasm   Method calls in dependent assemblies\n" +
				"  all      Record all possible exceptions\n" +
				"  added    Modifier; only create <exception/>s\n" +
				"             for NEW types/members\n" +
				"If nothing is specified, then only exceptions from the member will " +
				"be listed.",
				v => exceptions = ParseExceptionLocations (v) },
			{ "f=",
				"Specify a {FLAG} to alter behavior.  See later -f* options for available flags.",
				v => {
					switch (v) {
						case "ignore-missing-types":
							ignore_missing_types = true;
							break;
						case "no-assembly-versions":
							no_assembly_versions = true;
							break;
						default:
							throw new Exception ("Unsupported flag `" + v + "'.");
					}
				} },
			{ "fignore-missing-types",
				"Do not report an error if a --type=TYPE type\nwas not found.",
				v => ignore_missing_types = v != null },
			{ "fno-assembly-versions",
				"Do not generate //AssemblyVersion elements.",
				v => no_assembly_versions = v != null },
			{ "i|import=", 
				"Import documentation from {FILE}.",
				v => AddImporter (v) },
			{ "L|lib=",
				"Check for assembly references in {DIRECTORY}.",
				v => assemblyResolver.AddSearchDirectory (v) },
			{ "library=",
				"Ignored for compatibility with update-ecma-xml.",
				v => {} },
			{ "o|out=",
				"Root {DIRECTORY} to generate/update documentation.",
				v => srcPath = v },
			{ "r=",
				"Search for dependent assemblies in the directory containing {ASSEMBLY}.\n" +
				"(Equivalent to '-L `dirname ASSEMBLY`'.)",
				v => assemblyResolver.AddSearchDirectory (Path.GetDirectoryName (v)) },
			{ "since=",
				"Manually specify the assembly {VERSION} that new members were added in.",
				v => since = v },
			{ "type=",
			  "Only update documentation for {TYPE}.",
				v => types.Add (v) },
		};
		var assemblies = Parse (p, args, "update", 
				"[OPTIONS]+ ASSEMBLIES",
				"Create or update documentation from ASSEMBLIES.");
		if (assemblies == null)
			return;
		if (assemblies.Count == 0)
			Error ("No assemblies specified.");

		foreach (var dir in assemblies
				.Where (a => a.Contains (Path.DirectorySeparatorChar))
				.Select (a => Path.GetDirectoryName (a)))
			assemblyResolver.AddSearchDirectory (dir);

		// PARSE BASIC OPTIONS AND LOAD THE ASSEMBLY TO DOCUMENT
		
		if (srcPath == null)
			throw new InvalidOperationException("The --out option is required.");
		
		this.assemblies = assemblies.Select (a => LoadAssembly (a)).ToList ();

		docEnum = docEnum ?? new DocumentationEnumerator ();
		
		// PERFORM THE UPDATES
		
		if (types.Count > 0) {
			types.Sort ();
			DoUpdateTypes (srcPath, types, srcPath);
		}
#if false
		else if (opts.@namespace != null)
			DoUpdateNS (opts.@namespace, Path.Combine (opts.path, opts.@namespace),
					Path.Combine (dest_dir, opts.@namespace));
#endif
		else
			DoUpdateAssemblies (srcPath, srcPath);

		Console.WriteLine("Members Added: {0}, Members Deleted: {1}", additions, deletions);
	}

	void AddImporter (string path)
	{
		try {
			XmlReader r = new XmlTextReader (path);
			if (r.Read ()) {
				while (r.NodeType != XmlNodeType.Element) {
					if (!r.Read ())
						Error ("Unable to read XML file: {0}.", path);
				}
				if (r.LocalName == "doc") {
					importers.Add (new MsxdocDocumentationImporter (path));
				}
				else if (r.LocalName == "Libraries") {
					var ecmadocs = new XmlTextReader (path);
					docEnum = new EcmaDocumentationEnumerator (this, ecmadocs);
					importers.Add (new EcmaDocumentationImporter (ecmadocs));
				}
				else
					Error ("Unsupported XML format within {0}.", path);
			}
			r.Close ();
		} catch (Exception e) {
			Environment.ExitCode = 1;
			Error ("Could not load XML file: {0}.", e.Message);
		}
	}

	static ExceptionLocations ParseExceptionLocations (string s)
	{
		ExceptionLocations loc = ExceptionLocations.Member;
		if (s == null)
			return loc;
		foreach (var type in s.Split (',')) {
			switch (type) {
				case "added":   loc |= ExceptionLocations.AddedMembers; break;
				case "all":     loc |= ExceptionLocations.Assembly | ExceptionLocations.DependentAssemblies; break;
				case "asm":     loc |= ExceptionLocations.Assembly; break;
				case "depasm":  loc |= ExceptionLocations.DependentAssemblies; break;
				default:        throw new NotSupportedException ("Unsupported --exceptions value: " + type);
			}
		}
		return loc;
	}

	internal void Warning (string format, params object[] args)
	{
		Message (TraceLevel.Warning, "mdoc: " + format, args);
	}
	
	private AssemblyDefinition LoadAssembly (string name)
	{
		AssemblyDefinition assembly = null;
		try {
			assembly = AssemblyDefinition.ReadAssembly (name, new ReaderParameters { AssemblyResolver = assemblyResolver });
		} catch (System.IO.FileNotFoundException) { }

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

	private static void WriteFile (string filename, FileMode mode, Action<TextWriter> action)
	{
		Action<string> creator = file => {
			using (var writer = OpenWrite (file, mode))
				action (writer);
		};

		MdocFile.UpdateFile (filename, creator);
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
	
	private XmlDocument CreateIndexStub()
	{
		XmlDocument index = new XmlDocument();

		XmlElement index_root = index.CreateElement("Overview");
		index.AppendChild(index_root);

		if (assemblies.Count == 0)
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

		WriteFile (outdir + "/ns-" + ns + ".xml", FileMode.CreateNew, 
				writer => WriteXml (index.DocumentElement, writer));
	}

	public void DoUpdateTypes (string basepath, List<string> typenames, string dest)
	{
		var index = CreateIndexForTypes (dest);

		var found = new HashSet<string> ();
		foreach (AssemblyDefinition assembly in assemblies) {
			foreach (TypeDefinition type in docEnum.GetDocumentationTypes (assembly, typenames)) {
				string relpath = DoUpdateType (type, basepath, dest);
				if (relpath == null)
					continue;

				found.Add (type.FullName);

				if (index == null)
					continue;

				index.Add (assembly);
				index.Add (type);
			}
		}

		if (index != null)
			index.Write ();
		
		if (ignore_missing_types)
			return;

		var notFound = from n in typenames where !found.Contains (n) select n;
		if (notFound.Any ())
			throw new InvalidOperationException("Type(s) not found: " + string.Join (", ", notFound.ToArray ()));
	}

	class IndexForTypes {

		MDocUpdater app;
		string indexFile;

		XmlDocument index;
		XmlElement index_types;
		XmlElement index_assemblies;

		public IndexForTypes (MDocUpdater app, string indexFile, XmlDocument index)
		{
			this.app        = app;
			this.indexFile  = indexFile;
			this.index      = index;

			index_types = WriteElement (index.DocumentElement, "Types");
			index_assemblies = WriteElement (index.DocumentElement, "Assemblies");
		}

		public void Add (AssemblyDefinition assembly)
		{
			if (index_assemblies.SelectSingleNode ("Assembly[@Name='" + assembly.Name.Name + "']") != null)
				return;

			app.AddIndexAssembly (assembly, index_assemblies);
		}

		public void Add (TypeDefinition type)
		{
			app.AddIndexType (type, index_types);
		}

		public void Write ()
		{
			SortIndexEntries (index_types);
			WriteFile (indexFile, FileMode.Create, 
					writer => WriteXml (index.DocumentElement, writer));
		}
	}

	IndexForTypes CreateIndexForTypes (string dest)
	{
		string indexFile = Path.Combine (dest, "index.xml");
		if (File.Exists (indexFile))
			return null;
		return new IndexForTypes (this, indexFile, CreateIndexStub ());
	}

	public string DoUpdateType (TypeDefinition type, string basepath, string dest)
	{
		if (type.Namespace == null)
			Warning ("warning: The type `{0}' is in the root namespace.  This may cause problems with display within monodoc.",
					type.FullName);
		if (!IsPublic (type))
			return null;
		
		// Must get the A+B form of the type name.
		string typename = GetTypeFileName(type);
		
		string reltypefile = DocUtils.PathCombine (DocUtils.GetNamespace (type), typename + ".xml");
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
			try {
				basefile.Load(typefile);
			} catch (Exception e) {
				throw new InvalidOperationException("Error loading " + typefile + ": " + e.Message, e);
			}
			
			DoUpdateType2("Updating", basefile, type, output, false);
		} else {
			// Stub
			XmlElement td = StubType(type, output);
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

	public void DoUpdateNS (string ns, string nspath, string outpath)
	{
		Dictionary<TypeDefinition, object> seenTypes = new Dictionary<TypeDefinition,object> ();
		AssemblyDefinition                  assembly = assemblies [0];

		foreach (System.IO.FileInfo file in new System.IO.DirectoryInfo(nspath).GetFiles("*.xml")) {
			XmlDocument basefile = new XmlDocument();
			string typefile = Path.Combine(nspath, file.Name);
			try {
				basefile.Load(typefile);
			} catch (Exception e) {
				throw new InvalidOperationException("Error loading " + typefile + ": " + e.Message, e);
			}

			string typename = 
				GetTypeFileName (basefile.SelectSingleNode("Type/@FullName").InnerText);
			TypeDefinition type = assembly.GetType(typename);
			if (type == null) {
				Warning ("Type no longer in assembly: " + typename);
				continue;
			}			

			seenTypes[type] = seenTypes;
			DoUpdateType2("Updating", basefile, type, Path.Combine(outpath, file.Name), false);
		}
		
		// Stub types not in the directory
		foreach (TypeDefinition type in docEnum.GetDocumentationTypes (assembly, null)) {
			if (type.Namespace != ns || seenTypes.ContainsKey(type))
				continue;

			XmlElement td = StubType(type, Path.Combine(outpath, GetTypeFileName(type) + ".xml"));
			if (td == null) continue;
		}
	}
	
	private static string GetTypeFileName (TypeReference type)
	{
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

	private void AddIndexAssembly (AssemblyDefinition assembly, XmlElement parent)
	{
		XmlElement index_assembly = parent.OwnerDocument.CreateElement("Assembly");
		index_assembly.SetAttribute ("Name", assembly.Name.Name);
		index_assembly.SetAttribute ("Version", assembly.Name.Version.ToString());

		AssemblyNameDefinition name = assembly.Name;
		if (name.HasPublicKey) {
			XmlElement pubkey = parent.OwnerDocument.CreateElement ("AssemblyPublicKey");
			var key = new StringBuilder (name.PublicKey.Length*3 + 2);
			key.Append ("[");
			foreach (byte b in name.PublicKey)
				key.AppendFormat ("{0,2:x2} ", b);
			key.Append ("]");
			pubkey.InnerText = key.ToString ();
			index_assembly.AppendChild (pubkey);
		}

		if (!string.IsNullOrEmpty (name.Culture)) {
			XmlElement culture = parent.OwnerDocument.CreateElement ("AssemblyCulture");
			culture.InnerText = name.Culture;
			index_assembly.AppendChild (culture);
		}

		MakeAttributes (index_assembly, GetCustomAttributes (assembly.CustomAttributes, ""));
		parent.AppendChild(index_assembly);
	}

	private void AddIndexType (TypeDefinition type, XmlElement index_types)
	{
		string typename = GetTypeFileName(type);

		// Add namespace and type nodes into the index file as needed
		string ns = DocUtils.GetNamespace (type);
		XmlElement nsnode = (XmlElement) index_types.SelectSingleNode ("Namespace[@Name='" + ns + "']");
		if (nsnode == null) {
			nsnode = index_types.OwnerDocument.CreateElement("Namespace");
			nsnode.SetAttribute ("Name", ns);
			index_types.AppendChild (nsnode);
		}
		string doc_typename = GetDocTypeName (type);
		XmlElement typenode = (XmlElement) nsnode.SelectSingleNode ("Type[@Name='" + typename + "']");
		if (typenode == null) {
			typenode = index_types.OwnerDocument.CreateElement ("Type");
			typenode.SetAttribute ("Name", typename);
			nsnode.AppendChild (typenode);
		}
		if (typename != doc_typename)
			typenode.SetAttribute("DisplayName", doc_typename);
		else
			typenode.RemoveAttribute("DisplayName");

		typenode.SetAttribute ("Kind", GetTypeKind (type));
	}

	private void DoUpdateAssemblies (string source, string dest) 
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
		
		string defaultTitle = "Untitled";
		if (assemblies.Count == 1)
			defaultTitle = assemblies[0].Name.Name;
		WriteElementInitialText(index.DocumentElement, "Title", defaultTitle);
		
		XmlElement index_types = WriteElement(index.DocumentElement, "Types");
		XmlElement index_assemblies = WriteElement(index.DocumentElement, "Assemblies");
		index_assemblies.RemoveAll ();


		HashSet<string> goodfiles = new HashSet<string> ();

		foreach (AssemblyDefinition assm in assemblies) {
			AddIndexAssembly (assm, index_assemblies);
			DoUpdateAssembly (assm, index_types, source, dest, goodfiles);
		}

		SortIndexEntries (index_types);
		
		CleanupFiles (dest, goodfiles);
		CleanupIndexTypes (index_types, goodfiles);
		CleanupExtensions (index_types);

		WriteFile (indexfile, FileMode.Create, 
				writer => WriteXml(index.DocumentElement, writer));
	}
		
	private static char[] InvalidFilenameChars = {'\\', '/', ':', '*', '?', '"', '<', '>', '|'};

	private void DoUpdateAssembly (AssemblyDefinition assembly, XmlElement index_types, string source, string dest, HashSet<string> goodfiles) 
	{
		foreach (TypeDefinition type in docEnum.GetDocumentationTypes (assembly, null)) {
			string typename = GetTypeFileName(type);
			if (!IsPublic (type) || typename.IndexOfAny (InvalidFilenameChars) >= 0)
				continue;

			string reltypepath = DoUpdateType (type, source, dest);
			if (reltypepath == null)
				continue;
			
			// Add namespace and type nodes into the index file as needed
			AddIndexType (type, index_types);
				
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

			goodfiles.Add (reltypepath);
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

	abstract class XmlNodeComparer : IComparer, IComparer<XmlNode>
	{
		public abstract int Compare (XmlNode x, XmlNode y);

		public int Compare (object x, object y)
		{
			return Compare ((XmlNode) x, (XmlNode) y);
		}
	}

	class AttributeNameComparer : XmlNodeComparer {
		string attribute;

		public AttributeNameComparer ()
			: this ("Name")
		{
		}

		public AttributeNameComparer (string attribute)
		{
			this.attribute = attribute;
		}

		public override int Compare (XmlNode x, XmlNode y)
		{
			return x.Attributes [attribute].Value.CompareTo (y.Attributes [attribute].Value);
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

	private static string GetTypeKind (TypeDefinition type)
	{
		if (type.IsEnum)
			return "Enumeration";
		if (type.IsValueType)
			return "Structure";
		if (type.IsInterface)
			return "Interface";
		if (DocUtils.IsDelegate (type))
			return "Delegate";
		if (type.IsClass || type.FullName == "System.Enum") // FIXME
			return "Class";
		throw new ArgumentException ("Unknown kind for type: " + type.FullName);
	}

	private static bool IsPublic (TypeDefinition type)
	{
		TypeDefinition decl = type;
		while (decl != null) {
			if (!(decl.IsPublic || decl.IsNestedPublic)) {
				return false;
			}
			decl = (TypeDefinition) decl.DeclaringType;
		}
		return true;
	}

	private void CleanupFiles (string dest, HashSet<string> goodfiles)
	{
		// Look for files that no longer correspond to types
		foreach (System.IO.DirectoryInfo nsdir in new System.IO.DirectoryInfo(dest).GetDirectories("*")) {
			foreach (System.IO.FileInfo typefile in nsdir.GetFiles("*.xml")) {
				string relTypeFile = Path.Combine(nsdir.Name, typefile.Name);
				if (!goodfiles.Contains (relTypeFile)) {
					XmlDocument doc = new XmlDocument ();
					doc.Load (typefile.FullName);
					XmlElement e = doc.SelectSingleNode("/Type") as XmlElement;
					if (UpdateAssemblyVersions(e, GetAssemblyVersions(), false)) {
						using (TextWriter writer = OpenWrite (typefile.FullName, FileMode.Truncate))
							WriteXml(doc.DocumentElement, writer);
						goodfiles.Add (relTypeFile);
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
		var w = new StreamWriter (
			new FileStream (path, mode),
			new UTF8Encoding (false)
		);
		w.NewLine = "\n";
		return w;
	}

	private string[] GetAssemblyVersions ()
	{
		return (from a in assemblies select GetAssemblyVersion (a)).ToArray ();
	}

	private static void CleanupIndexTypes (XmlElement index_types, HashSet<string> goodfiles)
	{
		// Look for type nodes that no longer correspond to types
		MyXmlNodeList remove = new MyXmlNodeList ();
		foreach (XmlElement typenode in index_types.SelectNodes("Namespace/Type")) {
			string fulltypename = Path.Combine (((XmlElement)typenode.ParentNode).GetAttribute("Name"), typenode.GetAttribute("Name") + ".xml");
			if (!goodfiles.Contains (fulltypename)) {
				remove.Add (typenode);
			}
		}
		foreach (XmlNode n in remove)
			n.ParentNode.RemoveChild (n);
	}

	private void CleanupExtensions (XmlElement index_types)
	{
		XmlNode e = index_types.SelectSingleNode ("/Overview/ExtensionMethods");
		if (extensionMethods.Count == 0) {
			if (e == null)
				return;
			index_types.SelectSingleNode ("/Overview").RemoveChild (e);
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
		
	public void DoUpdateType2 (string message, XmlDocument basefile, TypeDefinition type, string output, bool insertSince)
	{
		Console.WriteLine(message + ": " + type.FullName);
		
		StringToXmlNodeMap seenmembers = new StringToXmlNodeMap ();

		// Update type metadata
		UpdateType(basefile.DocumentElement, type);

		// Update existing members.  Delete member nodes that no longer should be there,
		// and remember what members are already documented so we don't add them again.
		if (true) {
			MyXmlNodeList todelete = new MyXmlNodeList ();
			foreach (DocsNodeInfo info in docEnum.GetDocumentationMembers (basefile, type)) {
				XmlElement oldmember  = info.Node;
				MemberReference oldmember2 = info.Member;
				string sig = oldmember2 != null ? memberFormatters [0].GetDeclaration (oldmember2) : null;

				// Interface implementations and overrides are deleted from the docs
				// unless the overrides option is given.
				if (oldmember2 != null && sig == null)
					oldmember2 = null;
				
				// Deleted (or signature changed)
				if (oldmember2 == null) {
					if (UpdateAssemblyVersions (oldmember, new string[]{ GetAssemblyVersion (type.Module.Assembly) }, false))
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
						Warning ("TODO: found a duplicate member '{0}', but it's not identical to the prior member found!", sig);
					continue;
				}
				
				// Update signature information
				UpdateMember(info);
				
				seenmembers.Add (sig, oldmember);
			}
			foreach (XmlElement oldmember in todelete)
				oldmember.ParentNode.RemoveChild (oldmember);
		}
		
		if (!DocUtils.IsDelegate (type)) {
			XmlNode members = WriteElement (basefile.DocumentElement, "Members");
			foreach (MemberReference m in type.GetMembers()) {
				if (m is TypeDefinition) continue;
				
				string sig = memberFormatters [0].GetDeclaration (m);
				if (sig == null) continue;
				if (seenmembers.ContainsKey(sig)) continue;
				
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

		if (output == null)
			WriteXml(basefile.DocumentElement, Console.Out);
		else {
			FileInfo file = new FileInfo (output);
			if (!file.Directory.Exists) {
				Console.WriteLine("Namespace Directory Created: " + type.Namespace);
				file.Directory.Create ();
			}
			WriteFile (output, FileMode.Create,
					writer => WriteXml(basefile.DocumentElement, writer));
		}
	}

	private string GetCodeSource (string lang, string file)
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
				Warning ("Could not load <code/> file '{0}' region '{1}': {2}",
						file, region, show_exceptions ? e.ToString () : e.Message);
				return null;
			}
		}
		try {
			using (StreamReader reader = new StreamReader (file))
				return reader.ReadToEnd ();
		} catch (Exception e) {
			Warning ("Could not load <code/> file '" + file + "': " + e.Message);
		}
		return null;
	}

	void DeleteMember (string reason, string output, XmlNode member, MyXmlNodeList todelete)
	{
		string format = output != null
			? "{0}: File='{1}'; Signature='{4}'"
			: "{0}: XPath='/Type[@FullName=\"{2}\"]/Members/Member[@MemberName=\"{3}\"]'; Signature='{4}'";
		Warning (format,
				reason, 
				output,
				member.OwnerDocument.DocumentElement.GetAttribute ("FullName"),
				member.Attributes ["MemberName"].Value, 
				member.SelectSingleNode ("MemberSignature[@Language='C#']/@Value").Value);
		if (!delete && MemberDocsHaveUserContent (member)) {
			Warning ("Member deletions must be enabled with the --delete option.");
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
	
	// UPDATE HELPER FUNCTIONS
	
	// CREATE A STUB DOCUMENTATION FILE	

	public XmlElement StubType (TypeDefinition type, string output)
	{
		string typesig = typeFormatters [0].GetDeclaration (type);
		if (typesig == null) return null; // not publicly visible
		
		XmlDocument doc = new XmlDocument();
		XmlElement root = doc.CreateElement("Type");
		doc.AppendChild (root);

		DoUpdateType2 ("New Type", doc, type, output, true);
		
		return root;
	}

	private XmlElement CreateSinceNode (XmlDocument doc)
	{
		XmlElement s = doc.CreateElement ("since");
		s.SetAttribute ("version", since);
		return s;
	}
	
	// STUBBING/UPDATING FUNCTIONS
	
	public void UpdateType (XmlElement root, TypeDefinition type)
	{
		root.SetAttribute("Name", GetDocTypeName (type));
		root.SetAttribute("FullName", GetDocTypeFullName (type));

		foreach (MemberFormatter f in typeFormatters) {
			string element = "TypeSignature[@Language='" + f.Language + "']";
			WriteElementAttribute (root, element, "Language", f.Language);
			WriteElementAttribute (root, element, "Value", f.GetDeclaration (type));
		}
		
		XmlElement ass = WriteElement(root, "AssemblyInfo");
		WriteElementText(ass, "AssemblyName", type.Module.Assembly.Name.Name);
		if (!no_assembly_versions) {
			UpdateAssemblyVersions (root, type, true);
		}
		else {
			var versions = ass.SelectNodes ("AssemblyVersion").Cast<XmlNode> ().ToList ();
			foreach (var version in versions)
				ass.RemoveChild (version);
		}
		if (!string.IsNullOrEmpty (type.Module.Assembly.Name.Culture))
			WriteElementText(ass, "AssemblyCulture", type.Module.Assembly.Name.Culture);
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
		
		if (type.IsGenericType ()) {
			MakeTypeParameters (root, type.GenericParameters);
		} else {
			ClearElement(root, "TypeParameters");
		}
		
		if (type.BaseType != null) {
			XmlElement basenode = WriteElement(root, "Base");
			
			string basetypename = GetDocTypeFullName (type.BaseType);
			if (basetypename == "System.MulticastDelegate") basetypename = "System.Delegate";
			WriteElementText(root, "Base/BaseTypeName", basetypename);
			
			// Document how this type instantiates the generic parameters of its base type
			TypeReference origBase = type.BaseType.GetElementType ();
			if (origBase.IsGenericType ()) {
				ClearElement(basenode, "BaseTypeArguments");
				GenericInstanceType baseInst             = type.BaseType as GenericInstanceType;
				IList<TypeReference> baseGenArgs    = baseInst == null ? null : baseInst.GenericArguments;
				IList<GenericParameter> baseGenParams = origBase.GenericParameters;
				if (baseGenArgs.Count != baseGenParams.Count)
					throw new InvalidOperationException ("internal error: number of generic arguments doesn't match number of generic parameters.");
				for (int i = 0; baseGenArgs != null && i < baseGenArgs.Count; i++) {
					GenericParameter param = baseGenParams [i];
					TypeReference    value = baseGenArgs [i];

					XmlElement bta = WriteElement(basenode, "BaseTypeArguments");
					XmlElement arg = bta.OwnerDocument.CreateElement("BaseTypeArgument");
					bta.AppendChild(arg);
					arg.SetAttribute ("TypeParamName", param.Name);
					arg.InnerText = GetDocTypeFullName (value);
				}
			}
		} else {
			ClearElement(root, "Base");
		}

		if (!DocUtils.IsDelegate (type) && !type.IsEnum) {
			IEnumerable<TypeReference> userInterfaces = DocUtils.GetUserImplementedInterfaces (type);
			List<string> interface_names = userInterfaces
					.Select (iface => GetDocTypeFullName (iface))
					.OrderBy (s => s)
					.ToList ();

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

		MakeAttributes (root, GetCustomAttributes (type));
		
		if (DocUtils.IsDelegate (type)) {
			MakeTypeParameters (root, type.GenericParameters);
			MakeParameters(root, type.GetMethod("Invoke").Parameters);
			MakeReturnValue(root, type.GetMethod("Invoke"));
		}
		
		DocsNodeInfo typeInfo = new DocsNodeInfo (WriteElement(root, "Docs"), type);
		MakeDocNode (typeInfo);
		
		if (!DocUtils.IsDelegate (type))
			WriteElement (root, "Members");

		NormalizeWhitespace(root);
	}

	internal static IEnumerable<T> Sort<T> (IEnumerable<T> list)
	{
		List<T> l = new List<T> (list);
		l.Sort ();
		return l;
	}

	private void UpdateMember (DocsNodeInfo info)
	{
		XmlElement me = (XmlElement) info.Node;
		MemberReference mi = info.Member;

		foreach (MemberFormatter f in memberFormatters) {
			string element = "MemberSignature[@Language='" + f.Language + "']";
			WriteElementAttribute (me, element, "Language", f.Language);
			WriteElementAttribute (me, element, "Value", f.GetDeclaration (mi));
		}

		WriteElementText(me, "MemberType", GetMemberType(mi));
		
		if (!no_assembly_versions) {
			UpdateAssemblyVersions (me, mi, true);
		}
		else {
			ClearElement (me, "AssemblyInfo");
		}

		MakeAttributes (me, GetCustomAttributes (mi));

		MakeReturnValue(me, mi);
		if (mi is MethodReference) {
			MethodReference mb = (MethodReference) mi;
			if (mb.IsGenericMethod ())
				MakeTypeParameters (me, mb.GenericParameters);
		}
		MakeParameters(me, mi);
		
		string fieldValue;
		if (mi is FieldDefinition && GetFieldConstValue ((FieldDefinition)mi, out fieldValue))
			WriteElementText(me, "MemberValue", fieldValue);
		
		info.Node = WriteElement (me, "Docs");
		MakeDocNode (info);
		UpdateExtensionMethods (me, info);
	}

	IEnumerable<string> GetCustomAttributes (MemberReference mi)
	{
		IEnumerable<string> attrs = Enumerable.Empty<string>();

		ICustomAttributeProvider p = mi as ICustomAttributeProvider;
		if (p != null)
			attrs = attrs.Concat (GetCustomAttributes (p.CustomAttributes, ""));

		PropertyDefinition pd = mi as PropertyDefinition;
		if (pd != null) {
			if (pd.GetMethod != null)
				attrs = attrs.Concat (GetCustomAttributes (pd.GetMethod.CustomAttributes, "get: "));
			if (pd.SetMethod != null)
				attrs = attrs.Concat (GetCustomAttributes (pd.SetMethod.CustomAttributes, "set: "));
		}

		EventDefinition ed = mi as EventDefinition;
		if (ed != null) {
			if (ed.AddMethod != null)
				attrs = attrs.Concat (GetCustomAttributes (ed.AddMethod.CustomAttributes, "add: "));
			if (ed.RemoveMethod != null)
				attrs = attrs.Concat (GetCustomAttributes (ed.RemoveMethod.CustomAttributes, "remove: "));
		}

		return attrs;
	}

	IEnumerable<string> GetCustomAttributes (IList<CustomAttribute> attributes, string prefix)
	{
		foreach (CustomAttribute attribute in attributes.OrderBy (ca => ca.AttributeType.FullName)) {

			TypeDefinition attrType = attribute.AttributeType as TypeDefinition;
			if (attrType != null && !IsPublic (attrType))
				continue;
			if (slashdocFormatter.GetName (attribute.AttributeType) == null)
				continue;
			
			if (Array.IndexOf (IgnorableAttributes, attribute.AttributeType.FullName) >= 0)
				continue;
			
			StringList fields = new StringList ();

			for (int i = 0; i < attribute.ConstructorArguments.Count; ++i) {
				CustomAttributeArgument argument = attribute.ConstructorArguments [i];
				fields.Add (MakeAttributesValueString (
						argument.Value,
						argument.Type));
			}
			var namedArgs =
				(from namedArg in attribute.Fields
				 select new { Type=namedArg.Argument.Type, Name=namedArg.Name, Value=namedArg.Argument.Value })
				.Concat (
						(from namedArg in attribute.Properties
						 select new { Type=namedArg.Argument.Type, Name=namedArg.Name, Value=namedArg.Argument.Value }))
				.OrderBy (v => v.Name);
			foreach (var d in namedArgs)
				fields.Add (string.Format ("{0}={1}", d.Name, 
						MakeAttributesValueString (d.Value, d.Type)));

			string a2 = String.Join(", ", fields.ToArray ());
			if (a2 != "") a2 = "(" + a2 + ")";

			string name = attribute.Constructor.DeclaringType.FullName;
			if (name.EndsWith("Attribute")) name = name.Substring(0, name.Length-"Attribute".Length);
			yield return prefix + name + a2;
		}
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

	private void UpdateExtensionMethods (XmlElement e, DocsNodeInfo info)
	{
		MethodDefinition me = info.Member as MethodDefinition;
		if (me == null)
			return;
		if (info.Parameters.Count < 1)
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
		if (!(info.Parameters [0].ParameterType is GenericParameter)) {
			AppendElementAttributeText (targets, "Target", "Type",
				slashdocFormatter.GetDeclaration (info.Parameters [0].ParameterType));
		}
		else {
			GenericParameter gp = (GenericParameter) info.Parameters [0].ParameterType;
			IList<TypeReference> constraints = gp.Constraints;
			if (constraints.Count == 0)
				AppendElementAttributeText (targets, "Target", "Type", "System.Object");
			else
				foreach (TypeReference c in constraints)
					AppendElementAttributeText(targets, "Target", "Type",
						slashdocFormatter.GetDeclaration (c));
		}
	}
	
	private static bool GetFieldConstValue (FieldDefinition field, out string value)
	{
		value = null;
		TypeDefinition type = field.DeclaringType.Resolve ();
		if (type != null && type.IsEnum) return false;
		
		if (type != null && type.IsGenericType ()) return false;
		if (!field.HasConstant)
			return false;
		if (field.IsLiteral) {
			object val = field.Constant;
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
	
	internal static XmlElement WriteElement(XmlNode parent, string element) {
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

	static XmlElement AppendElementText (XmlNode parent, string element, string value)
	{
		XmlElement n = parent.OwnerDocument.CreateElement (element);
		parent.AppendChild (n);
		n.InnerText = value;
		return n;
	}

	static XmlElement AppendElementAttributeText (XmlNode parent, string element, string attribute, string value)
	{
		XmlElement n = parent.OwnerDocument.CreateElement (element);
		parent.AppendChild (n);
		n.SetAttribute (attribute, value);
		return n;
	}

	internal static XmlNode CopyNode (XmlNode source, XmlNode dest)
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
	internal static void ClearElement(XmlElement parent, string name) {
		XmlElement node = (XmlElement)parent.SelectSingleNode(name);
		if (node != null)
			parent.RemoveChild(node);
	}
	
	// DOCUMENTATION HELPER FUNCTIONS
	
	private void MakeDocNode (DocsNodeInfo info)
	{
		List<GenericParameter> genericParams      = info.GenericParameters;
		IList<ParameterDefinition> parameters  = info.Parameters;
		TypeReference returntype                  = info.ReturnType;
		bool returnisreturn         = info.ReturnIsReturn;
		XmlElement e                = info.Node;
		bool addremarks             = info.AddRemarks;

		WriteElementInitialText(e, "summary", "To be added.");
		
		if (parameters != null) {
			string[] values = new string [parameters.Count];
			for (int i = 0; i < values.Length; ++i)
				values [i] = parameters [i].Name;
			UpdateParameters (e, "param", values);
		}

		if (genericParams != null) {
			string[] values = new string [genericParams.Count];
			for (int i = 0; i < values.Length; ++i)
				values [i] = genericParams [i].Name;
			UpdateParameters (e, "typeparam", values);
		}

		string retnodename = null;
		if (returntype != null && returntype.FullName != "System.Void") { // FIXME
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

		if (exceptions.HasValue && info.Member != null &&
				(exceptions.Value & ExceptionLocations.AddedMembers) == 0) {
			UpdateExceptions (e, info.Member);
		}

		foreach (DocumentationImporter importer in importers)
			importer.ImportDocumentation (info);
		
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
	

	private void UpdateParameters (XmlElement e, string element, string[] values)
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
						Warning ("The following param node can only be deleted if the --delete option is given: ");
						if (e.ParentNode == e.OwnerDocument.DocumentElement) {
							// delegate type
							Warning ("\tXPath=/Type[@FullName=\"{0}\"]/Docs/param[@name=\"{1}\"]",
									e.OwnerDocument.DocumentElement.GetAttribute ("FullName"),
									name);
						}
						else {
							Warning ("\tXPath=/Type[@FullName=\"{0}\"]//Member[@MemberName=\"{1}\"]/Docs/param[@name=\"{2}\"]",
									e.OwnerDocument.DocumentElement.GetAttribute ("FullName"),
									e.ParentNode.Attributes ["MemberName"].Value, 
									name);
						}
						Warning ("\tValue={0}", paramnode.OuterXml);
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

	class CrefComparer : XmlNodeComparer {

		public CrefComparer ()
		{
		}

		public override int Compare (XmlNode x, XmlNode y)
		{
			string xType = x.Attributes ["cref"].Value;
			string yType = y.Attributes ["cref"].Value;
			string xNamespace = GetNamespace (xType);
			string yNamespace = GetNamespace (yType);

			int c = xNamespace.CompareTo (yNamespace);
			if (c != 0)
				return c;
			return xType.CompareTo (yType);
		}

		static string GetNamespace (string type)
		{
			int n = type.LastIndexOf ('.');
			if (n >= 0)
				return type.Substring (0, n);
			return string.Empty;
		}
	}
	
	private void UpdateExceptions (XmlNode docs, MemberReference member)
	{
		foreach (var source in new ExceptionLookup (exceptions.Value)[member]) {
			string cref = slashdocFormatter.GetDeclaration (source.Exception);
			var node = docs.SelectSingleNode ("exception[@cref='" + cref + "']");
			if (node != null)
				continue;
			XmlElement e = docs.OwnerDocument.CreateElement ("exception");
			e.SetAttribute ("cref", cref);
			e.InnerXml = "To be added; from: <see cref=\"" + 
				string.Join ("\" />, <see cref=\"", 
						source.Sources.Select (m => slashdocFormatter.GetDeclaration (m))
						.ToArray ()) +
				"\" />";
			docs.AppendChild (e);
		}
		SortXmlNodes (docs, docs.SelectNodes ("exception"), 
				new CrefComparer ());
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
	
	private static bool UpdateAssemblyVersions (XmlElement root, MemberReference member, bool add)
	{
		TypeDefinition type = member as TypeDefinition;
		if (type == null)
			type = member.DeclaringType as TypeDefinition;
		return UpdateAssemblyVersions(root, new string[]{ GetAssemblyVersion (type.Module.Assembly) }, add);
	}
	
	private static string GetAssemblyVersion (AssemblyDefinition assembly)
	{
		return assembly.Name.Version.ToString();
	}
	
	private static bool UpdateAssemblyVersions(XmlElement root, string[] assemblyVersions, bool add)
	{
		XmlElement e = (XmlElement) root.SelectSingleNode ("AssemblyInfo");
		if (e == null) {
			e = root.OwnerDocument.CreateElement("AssemblyInfo");
			root.AppendChild(e);
		}
		List<XmlNode> matches = e.SelectNodes ("AssemblyVersion").Cast<XmlNode>()
			.Where(v => Array.IndexOf (assemblyVersions, v.InnerText) >= 0)
			.ToList ();
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

	// FIXME: get TypeReferences instead of string comparison?
	private static string[] IgnorableAttributes = {
		// Security related attributes
		"System.Reflection.AssemblyKeyFileAttribute",
		"System.Reflection.AssemblyDelaySignAttribute",
		// Present in @RefType
		"System.Runtime.InteropServices.OutAttribute",
		// For naming the indexer to use when not using indexers
		"System.Reflection.DefaultMemberAttribute",
		// for decimal constants
		"System.Runtime.CompilerServices.DecimalConstantAttribute",
		// compiler generated code
		"System.Runtime.CompilerServices.CompilerGeneratedAttribute",
		// more compiler generated code, e.g. iterator methods
		"System.Diagnostics.DebuggerHiddenAttribute",
		"System.Runtime.CompilerServices.FixedBufferAttribute",
		"System.Runtime.CompilerServices.UnsafeValueTypeAttribute",
		// extension methods
		"System.Runtime.CompilerServices.ExtensionAttribute",
	};

	private void MakeAttributes (XmlElement root, IEnumerable<string> attributes)
	{
		if (!attributes.Any ()) {
			ClearElement (root, "Attributes");
			return;
		}

		XmlElement e = (XmlElement)root.SelectSingleNode("Attributes");
		if (e != null)
			e.RemoveAll();
		else if (e == null)
			e = root.OwnerDocument.CreateElement("Attributes");
		
		foreach (string attribute in attributes) {
			XmlElement ae = root.OwnerDocument.CreateElement("Attribute");
			e.AppendChild(ae);
			
			WriteElementText(ae, "AttributeName", attribute);
		}
		
		if (e.ParentNode == null)
			root.AppendChild(e);

		NormalizeWhitespace(e);
	}

	private static string MakeAttributesValueString (object v, TypeReference valueType)
	{
		if (v == null)
			return "null";
		if (valueType.FullName == "System.Type")
			return "typeof(" + v.ToString () + ")";
		if (valueType.FullName == "System.String")
			return "\"" + v.ToString () + "\"";
		if (v is Boolean)
			return (bool)v ? "true" : "false";
		TypeDefinition valueDef = valueType.Resolve ();
		if (valueDef == null || !valueDef.IsEnum)
			return v.ToString ();
		string typename = GetDocTypeFullName (valueType);
		var values = GetEnumerationValues (valueDef);
		long c = ToInt64 (v);
		if (values.ContainsKey (c))
			return typename + "." + values [c];
		if (valueDef.CustomAttributes.Any (ca => ca.AttributeType.FullName == "System.FlagsAttribute")) {
			return string.Join (" | ",
					(from i in values.Keys
					 where (c & i) != 0
					 select typename + "." + values [i])
					.ToArray ());
		}
		return "(" + GetDocTypeFullName (valueType) + ") " + v.ToString ();
	}

	private static Dictionary<long, string> GetEnumerationValues (TypeDefinition type)
	{
		var values = new Dictionary<long, string> ();
		foreach (var f in 
				(from f in type.Fields.Cast<FieldDefinition> ()
				 where !(f.IsRuntimeSpecialName || f.IsSpecialName)
				 select f)) {
			values [ToInt64 (f.Constant)] = f.Name;
		}
		return values;
	}

	static long ToInt64 (object value)
	{
		if (value is ulong)
			return (long) (ulong) value;
		return Convert.ToInt64 (value);
	}
	
	private void MakeParameters (XmlElement root, IList<ParameterDefinition> parameters)
	{
		XmlElement e = WriteElement(root, "Parameters");
		e.RemoveAll();
		foreach (ParameterDefinition p in parameters) {
			XmlElement pe = root.OwnerDocument.CreateElement("Parameter");
			e.AppendChild(pe);
			pe.SetAttribute("Name", p.Name);
			pe.SetAttribute("Type", GetDocParameterType (p.ParameterType));
			if (p.ParameterType is ByReferenceType) {
				if (p.IsOut) pe.SetAttribute("RefType", "out");
				else pe.SetAttribute("RefType", "ref");
			}
			MakeAttributes (pe, GetCustomAttributes (p.CustomAttributes, ""));
		}
	}
	
	private void MakeTypeParameters (XmlElement root, IList<GenericParameter> typeParams)
	{
		if (typeParams == null || typeParams.Count == 0) {
			XmlElement f = (XmlElement) root.SelectSingleNode ("TypeParameters");
			if (f != null)
				root.RemoveChild (f);
			return;
		}
		XmlElement e = WriteElement(root, "TypeParameters");
		e.RemoveAll();
		foreach (GenericParameter t in typeParams) {
			XmlElement pe = root.OwnerDocument.CreateElement("TypeParameter");
			e.AppendChild(pe);
			pe.SetAttribute("Name", t.Name);
			MakeAttributes (pe, GetCustomAttributes (t.CustomAttributes, ""));
			XmlElement ce = (XmlElement) e.SelectSingleNode ("Constraints");
			IList<TypeReference> constraints = t.Constraints;
			GenericParameterAttributes attrs = t.Attributes;
			if (attrs == GenericParameterAttributes.NonVariant && constraints.Count == 0) {
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
			foreach (TypeReference c in constraints) {
				TypeDefinition cd = c.Resolve ();
				AppendElementText (ce,
						(cd != null && cd.IsInterface) ? "InterfaceName" : "BaseTypeName",
						GetDocTypeFullName (c));
			}
		}
	}

	private void MakeParameters (XmlElement root, MemberReference mi)
	{
		if (mi is MethodDefinition && ((MethodDefinition) mi).IsConstructor)
			MakeParameters (root, ((MethodDefinition)mi).Parameters);
		else if (mi is MethodDefinition) {
			MethodDefinition mb = (MethodDefinition) mi;
			IList<ParameterDefinition> parameters = mb.Parameters;
			MakeParameters(root, parameters);
			if (parameters.Count > 0 && DocUtils.IsExtensionMethod (mb)) {
				XmlElement p = (XmlElement) root.SelectSingleNode ("Parameters/Parameter[position()=1]");
				p.SetAttribute ("RefType", "this");
			}
		}
		else if (mi is PropertyDefinition) {
			IList<ParameterDefinition> parameters = ((PropertyDefinition)mi).Parameters;
			if (parameters.Count > 0)
				MakeParameters(root, parameters);
			else
				return;
		}
		else if (mi is FieldDefinition) return;
		else if (mi is EventDefinition) return;
		else throw new ArgumentException();
	}

	internal static string GetDocParameterType (TypeReference type)
	{
		return GetDocTypeFullName (type).Replace ("@", "&");
	}

	private void MakeReturnValue (XmlElement root, TypeReference type, IList<CustomAttribute> attributes) 
	{
		XmlElement e = WriteElement(root, "ReturnValue");
		e.RemoveAll();
		WriteElementText(e, "ReturnType", GetDocTypeFullName (type));
		if (attributes != null)
			MakeAttributes(e, GetCustomAttributes (attributes, ""));
	}
	
	private void MakeReturnValue (XmlElement root, MemberReference mi)
	{
		if (mi is MethodDefinition && ((MethodDefinition) mi).IsConstructor)
			return;
		else if (mi is MethodDefinition)
			MakeReturnValue (root, ((MethodDefinition)mi).ReturnType, ((MethodDefinition)mi).MethodReturnType.CustomAttributes);
		else if (mi is PropertyDefinition)
			MakeReturnValue (root, ((PropertyDefinition)mi).PropertyType, null);
		else if (mi is FieldDefinition)
			MakeReturnValue (root, ((FieldDefinition)mi).FieldType, null);
		else if (mi is EventDefinition)
			MakeReturnValue (root, ((EventDefinition)mi).EventType, null);
		else
			throw new ArgumentException(mi + " is a " + mi.GetType().FullName);
	}
	
	private XmlElement MakeMember(XmlDocument doc, DocsNodeInfo info)
	{
		MemberReference mi = info.Member;
		if (mi is TypeDefinition) return null;

		string sigs = memberFormatters [0].GetDeclaration (mi);
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
		if (exceptions.HasValue && 
				(exceptions.Value & ExceptionLocations.AddedMembers) != 0)
			UpdateExceptions (info.Node, info.Member);

		if (since != null) {
			XmlNode docs = me.SelectSingleNode("Docs");
			docs.AppendChild (CreateSinceNode (doc));
		}
		
		return me;
	}

	internal static string GetMemberName (MemberReference mi)
	{
		MethodDefinition mb = mi as MethodDefinition;
		if (mb == null) {
			PropertyDefinition pi = mi as PropertyDefinition;
			if (pi == null)
				return mi.Name;
			return DocUtils.GetPropertyName (pi);
		}
		StringBuilder sb = new StringBuilder (mi.Name.Length);
		if (!DocUtils.IsExplicitlyImplemented (mb))
			sb.Append (mi.Name);
		else {
			TypeReference iface;
			MethodReference ifaceMethod;
			DocUtils.GetInfoForExplicitlyImplementedMethod (mb, out iface, out ifaceMethod);
			sb.Append (GetDocTypeFullName (iface));
			sb.Append ('.');
			sb.Append (ifaceMethod.Name);
		}
		if (mb.IsGenericMethod ()) {
			IList<GenericParameter> typeParams = mb.GenericParameters;
			if (typeParams.Count > 0) {
				sb.Append ("<");
				sb.Append (typeParams [0].Name);
				for (int i = 1; i < typeParams.Count; ++i)
					sb.Append (",").Append (typeParams [i].Name);
				sb.Append (">");
			}
		}
		return sb.ToString ();
	}
	
	/// SIGNATURE GENERATION FUNCTIONS
	internal static bool IsPrivate (MemberReference mi)
	{
		return memberFormatters [0].GetDeclaration (mi) == null;
	}

	internal static string GetMemberType (MemberReference mi)
	{
		if (mi is MethodDefinition && ((MethodDefinition) mi).IsConstructor)
			return "Constructor";
		if (mi is MethodDefinition)
			return "Method";
		if (mi is PropertyDefinition)
			return "Property";
		if (mi is FieldDefinition)
			return "Field";
		if (mi is EventDefinition)
			return "Event";
		throw new ArgumentException();
	}

	private static string GetDocTypeName (TypeReference type)
	{
		return docTypeFormatter.GetName (type);
	}

	internal static string GetDocTypeFullName (TypeReference type)
	{
		return DocTypeFullMemberFormatter.Default.GetName (type);
	}

	internal static string GetXPathForMember (DocumentationMember member)
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
			.Append (member.SelectSingleNode ("../../@FullName").Value)
			.Append ("\"]/");
		xpath.Append ("Members/Member[@MemberName=\"")
			.Append (member.SelectSingleNode ("@MemberName").Value)
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

	public static string GetXPathForMember (MemberReference member)
	{
		StringBuilder xpath = new StringBuilder ();
		xpath.Append ("//Type[@FullName=\"")
			.Append (member.DeclaringType.FullName)
			.Append ("\"]/");
		xpath.Append ("Members/Member[@MemberName=\"")
			.Append (GetMemberName (member))
			.Append ("\"]");

		IList<ParameterDefinition> parameters = null;
		if (member is MethodDefinition)
			parameters = ((MethodDefinition) member).Parameters;
		else if (member is PropertyDefinition) {
			parameters = ((PropertyDefinition) member).Parameters;
		}
		if (parameters != null && parameters.Count > 0) {
			xpath.Append ("/Parameters[count(Parameter) = ")
				.Append (parameters.Count);
			for (int i = 0; i < parameters.Count; ++i) {
				xpath.Append (" and Parameter [").Append (i+1).Append ("]/@Type=\"");
				xpath.Append (GetDocParameterType (parameters [i].ParameterType));
				xpath.Append ("\"");
			}
			xpath.Append ("]/..");
		}
		return xpath.ToString ();
	}
}

static class CecilExtensions {
	public static IEnumerable<MemberReference> GetMembers (this TypeDefinition type)
	{
		foreach (var c in type.Methods.Where (m => m.IsConstructor))
			yield return (MemberReference) c;
		foreach (var e in type.Events)
			yield return (MemberReference) e;
		foreach (var f in type.Fields)
			yield return (MemberReference) f;
		foreach (var m in type.Methods.Where (m => !m.IsConstructor))
			yield return (MemberReference) m;
		foreach (var t in type.NestedTypes)
			yield return (MemberReference) t;
		foreach (var p in type.Properties)
			yield return (MemberReference) p;
	}

	public static IEnumerable<MemberReference> GetMembers (this TypeDefinition type, string member)
	{
		return GetMembers (type).Where (m => m.Name == member);
	}

	public static MemberReference GetMember (this TypeDefinition type, string member)
	{
		return GetMembers (type, member).EnsureZeroOrOne ();
	}

	static T EnsureZeroOrOne<T> (this IEnumerable<T> source)
	{
		if (source.Count () > 1)
			throw new InvalidOperationException ("too many matches");
		return source.FirstOrDefault ();
	}

	public static MethodDefinition GetMethod (this TypeDefinition type, string method)
	{
		return type.Methods
			.Where (m => m.Name == method)
			.EnsureZeroOrOne ();
	}

	public static IEnumerable<MemberReference> GetDefaultMembers (this TypeReference type)
	{
		TypeDefinition def = type as TypeDefinition;
		if (def == null)
			return new MemberReference [0];
		CustomAttribute defMemberAttr = def.CustomAttributes
				.FirstOrDefault (c => c.AttributeType.FullName == "System.Reflection.DefaultMemberAttribute");
		if (defMemberAttr == null)
			return new MemberReference [0];
		string name = (string) defMemberAttr.ConstructorArguments [0].Value;
		return def.Properties
				.Where (p => p.Name == name)
				.Select (p => (MemberReference) p);
	}

	public static IEnumerable<TypeDefinition> GetTypes (this AssemblyDefinition assembly)
	{
		return assembly.Modules.SelectMany (md => md.GetAllTypes ());
	}

	public static TypeDefinition GetType (this AssemblyDefinition assembly, string type)
	{
		return GetTypes (assembly)
			.Where (td => td.FullName == type)
			.EnsureZeroOrOne ();
	}

	public static bool IsGenericType (this TypeReference type)
	{
		return type.GenericParameters.Count > 0;
	}

	public static bool IsGenericMethod (this MethodReference method)
	{
		return method.GenericParameters.Count > 0;
	}

	public static MemberReference Resolve (this MemberReference member)
	{
		FieldReference fr = member as FieldReference;
		if (fr != null)
			return fr.Resolve ();
		MethodReference mr = member as MethodReference;
		if (mr != null)
			return mr.Resolve ();
		TypeReference tr = member as TypeReference;
		if (tr != null)
			return tr.Resolve ();
		PropertyReference pr = member as PropertyReference;
		if (pr != null)
			return pr;
		EventReference er = member as EventReference;
		if (er != null)
			return er;
		throw new NotSupportedException ("Cannot find definition for " + member.ToString ());
	}

	public static TypeReference GetUnderlyingType (this TypeDefinition type)
	{
		if (!type.IsEnum)
			return type;
		return type.Fields.Cast<FieldDefinition>().First (f => f.Name == "value__").FieldType;
	}

	public static IEnumerable<TypeDefinition> GetAllTypes (this ModuleDefinition self)
	{
		foreach (var type in self.Types.SelectMany (t => t.GetAllTypes ()))
			yield return type;
	}

	static IEnumerable<TypeDefinition> GetAllTypes (this TypeDefinition self)
	{
		yield return self;

		if (!self.HasNestedTypes)
			yield break;

		foreach (var type in self.NestedTypes.SelectMany (t => t.GetAllTypes ()))
			yield return type;
	}
}

static class DocUtils {
	public static bool IsExplicitlyImplemented (MethodDefinition method)
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
			MethodDefinition method, out TypeReference iface, out MethodReference ifaceMethod)
	{
		iface = null;
		ifaceMethod = null;
		if (method.Overrides.Count != 1)
			throw new InvalidOperationException ("Could not determine interface type for explicitly-implemented interface member " + method.Name);
		iface = method.Overrides [0].DeclaringType;
		ifaceMethod = method.Overrides [0];
	}

	public static string GetPropertyName (PropertyDefinition pi)
	{
		// Issue: (g)mcs-generated assemblies that explicitly implement
		// properties don't specify the full namespace, just the 
		// TypeName.Property; .NET uses Full.Namespace.TypeName.Property.
		MethodDefinition method = pi.GetMethod;
		if (method == null)
			method = pi.SetMethod;
		if (!IsExplicitlyImplemented (method))
			return pi.Name;

		// Need to determine appropriate namespace for this member.
		TypeReference iface;
		MethodReference ifaceMethod;
		GetInfoForExplicitlyImplementedMethod (method, out iface, out ifaceMethod);
		return string.Join (".", new string[]{
				DocTypeFullMemberFormatter.Default.GetName (iface),
				GetMember (pi.Name)});
	}

	public static string GetNamespace (TypeReference type)
	{
		if (type.GetElementType ().IsNested)
			type = type.GetElementType ();
		while (type != null && type.IsNested)
			type = type.DeclaringType;
		if (type == null)
			return string.Empty;
		return type.Namespace;
	}

	public static string PathCombine (string dir, string path)
	{
		if (dir == null)
			dir = "";
		if (path == null)
			path = "";
		return Path.Combine (dir, path);
	}

	public static bool IsExtensionMethod (MethodDefinition method)
	{
		return
			method.CustomAttributes.Cast<CustomAttribute> ()
					.Where (m => m.Constructor.DeclaringType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute")
					.Any () &&
			method.DeclaringType.CustomAttributes.Cast<CustomAttribute> ()
					.Where (m => m.Constructor.DeclaringType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute")
					.Any ();
	}

	public static bool IsDelegate (TypeDefinition type)
	{
		TypeReference baseRef = type.BaseType;
		if (baseRef == null)
			return false;
		return !type.IsAbstract && baseRef.FullName == "System.Delegate" || // FIXME
				baseRef.FullName == "System.MulticastDelegate";
	}

	public static List<TypeReference> GetDeclaringTypes (TypeReference type)
	{
		List<TypeReference> decls = new List<TypeReference> ();
		decls.Add (type);
		while (type.DeclaringType != null) {
			decls.Add (type.DeclaringType);
			type = type.DeclaringType;
		}
		decls.Reverse ();
		return decls;
	}

	public static int GetGenericArgumentCount (TypeReference type)
	{
		GenericInstanceType inst = type as GenericInstanceType;
		return inst != null
				? inst.GenericArguments.Count
				: type.GenericParameters.Count;
	}

	public static IEnumerable<TypeReference> GetUserImplementedInterfaces (TypeDefinition type)
	{
		HashSet<string> inheritedInterfaces = GetInheritedInterfaces (type);
		List<TypeReference> userInterfaces = new List<TypeReference> ();
		foreach (TypeReference iface in type.Interfaces) {
			TypeReference lookup = iface.Resolve () ?? iface;
			if (!inheritedInterfaces.Contains (GetQualifiedTypeName (lookup)))
				userInterfaces.Add (iface);
		}
		return userInterfaces;
	}

	private static string GetQualifiedTypeName (TypeReference type)
	{
		return "[" + type.Scope.Name + "]" + type.FullName;
	}

	private static HashSet<string> GetInheritedInterfaces (TypeDefinition type)
	{
		HashSet<string> inheritedInterfaces = new HashSet<string> ();
		Action<TypeDefinition> a = null;
		a = t => {
			if (t == null) return;
			foreach (TypeReference r in t.Interfaces) {
				inheritedInterfaces.Add (GetQualifiedTypeName (r));
				a (r.Resolve ());
			}
		};
		TypeReference baseRef = type.BaseType;
		while (baseRef != null) {
			TypeDefinition baseDef = baseRef.Resolve ();
			if (baseDef != null) {
				a (baseDef);
				baseRef = baseDef.BaseType;
			}
			else
				baseRef = null;
		}
		foreach (TypeReference r in type.Interfaces)
			a (r.Resolve ());
		return inheritedInterfaces;
	}
}

class DocsNodeInfo {
	public DocsNodeInfo (XmlElement node)
	{
		this.Node = node;
	}

	public DocsNodeInfo (XmlElement node, TypeDefinition type)
		: this (node)
	{
		SetType (type);
	}

	public DocsNodeInfo (XmlElement node, MemberReference member)
		: this (node)
	{
		SetMemberInfo (member);
	}

	void SetType (TypeDefinition type)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		Type = type;
		GenericParameters = new List<GenericParameter> (type.GenericParameters.Cast<GenericParameter> ());
		List<TypeReference> declTypes = DocUtils.GetDeclaringTypes (type);
		int maxGenArgs = DocUtils.GetGenericArgumentCount (type);
		for (int i = 0; i < declTypes.Count - 1; ++i) {
			int remove = System.Math.Min (maxGenArgs, 
					DocUtils.GetGenericArgumentCount (declTypes [i]));
			maxGenArgs -= remove;
			while (remove-- > 0)
				GenericParameters.RemoveAt (0);
		}
		if (DocUtils.IsDelegate (type)) {
			Parameters = type.GetMethod("Invoke").Parameters;
			ReturnType = type.GetMethod("Invoke").ReturnType;
			ReturnIsReturn = true;
		}
	}

	void SetMemberInfo (MemberReference member)
	{
		if (member == null)
			throw new ArgumentNullException ("member");
		ReturnIsReturn = true;
		AddRemarks = true;
		Member = member;
		
		if (member is MethodReference ) {
			MethodReference mr = (MethodReference) member;
			Parameters = mr.Parameters;
			if (mr.IsGenericMethod ()) {
				GenericParameters = new List<GenericParameter> (mr.GenericParameters.Cast<GenericParameter> ());
			}
		}
		else if (member is PropertyDefinition) {
			Parameters = ((PropertyDefinition) member).Parameters;
		}
			
		if (member is MethodDefinition) {
			ReturnType = ((MethodDefinition) member).ReturnType;
		} else if (member is PropertyDefinition) {
			ReturnType = ((PropertyDefinition) member).PropertyType;
			ReturnIsReturn = false;
		}

		// no remarks section for enum members
		if (member.DeclaringType != null && ((TypeDefinition) member.DeclaringType).IsEnum)
			AddRemarks = false;
	}

	public TypeReference ReturnType;
	public List<GenericParameter> GenericParameters;
	public IList<ParameterDefinition> Parameters;
	public bool ReturnIsReturn;
	public XmlElement Node;
	public bool AddRemarks = true;
	public MemberReference Member;
	public TypeDefinition Type;
}

class DocumentationEnumerator {

	public virtual IEnumerable<TypeDefinition> GetDocumentationTypes (AssemblyDefinition assembly, List<string> forTypes)
	{
		return GetDocumentationTypes (assembly, forTypes, null);
	}

	protected IEnumerable<TypeDefinition> GetDocumentationTypes (AssemblyDefinition assembly, List<string> forTypes, HashSet<string> seen)
	{
		foreach (TypeDefinition type in assembly.GetTypes()) {
			if (forTypes != null && forTypes.BinarySearch (type.FullName) < 0)
				continue;
			if (seen != null && seen.Contains (type.FullName))
				continue;
			yield return type;
			foreach (TypeDefinition nested in type.NestedTypes)
				yield return nested;
		}
	}

	public virtual IEnumerable<DocsNodeInfo> GetDocumentationMembers (XmlDocument basefile, TypeDefinition type)
	{
		foreach (XmlElement oldmember in basefile.SelectNodes("Type/Members/Member")) {
			if (oldmember.GetAttribute ("__monodocer-seen__") == "true") {
				oldmember.RemoveAttribute ("__monodocer-seen__");
				continue;
			}
			MemberReference m = GetMember (type, new DocumentationMember (oldmember));
			if (m == null) {
				yield return new DocsNodeInfo (oldmember);
			}
			else {
				yield return new DocsNodeInfo (oldmember, m);
			}
		}
	}

	protected static MemberReference GetMember (TypeDefinition type, DocumentationMember member)
	{
		string membertype = member.MemberType;
		
		string returntype = member.ReturnType;
		
		string docName = member.MemberName;
		string[] docTypeParams = GetTypeParameters (docName);

		// Loop through all members in this type with the same name
		foreach (MemberReference mi in GetReflectionMembers (type, docName)) {
			if (mi is TypeDefinition) continue;
			if (MDocUpdater.GetMemberType(mi) != membertype) continue;

			if (MDocUpdater.IsPrivate (mi))
				continue;

			IList<ParameterDefinition> pis = null;
			string[] typeParams = null;
			if (mi is MethodDefinition) {
				MethodDefinition mb = (MethodDefinition) mi;
				pis = mb.Parameters;
				if (docTypeParams != null && mb.IsGenericMethod ()) {
					IList<GenericParameter> args = mb.GenericParameters;
					if (args.Count == docTypeParams.Length) {
						typeParams = args.Cast<GenericParameter> ().Select (p => p.Name).ToArray ();
					}
				}
			}
			else if (mi is PropertyDefinition)
				pis = ((PropertyDefinition)mi).Parameters;
			
			int mcount = member.Parameters == null ? 0 : member.Parameters.Count;
			int pcount = pis == null ? 0 : pis.Count;
			if (mcount != pcount)
				continue;

			MethodDefinition mDef = mi as MethodDefinition;
			if (mDef != null && !mDef.IsConstructor) {
				// Casting operators can overload based on return type.
				if (returntype != GetReplacedString (
							MDocUpdater.GetDocTypeFullName (((MethodDefinition)mi).ReturnType), 
							typeParams, docTypeParams)) {
					continue;
				}
			}

			if (pcount == 0)
				return mi;
			bool good = true;
			for (int i = 0; i < pis.Count; i++) {
				string paramType = GetReplacedString (
					MDocUpdater.GetDocParameterType (pis [i].ParameterType),
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
		return types.ToArray ();
	}

	protected static IEnumerable<MemberReference> GetReflectionMembers (TypeDefinition type, string docName)
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
			foreach (MemberReference mi in type.GetMembers (docName))
				yield return mi;
			if (CountChars (docName, '.') > 0)
				// might be a property; try only type.member instead of
				// namespace.type.member.
				foreach (MemberReference mi in 
						type.GetMembers (DocUtils.GetTypeDotMember (docName)))
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
		foreach (MemberReference mi in type.GetMembers (refName))
			yield return mi;

		// case 4
		foreach (MemberReference mi in type.GetMembers (refName.Substring (startType + 1)))
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
		foreach (MemberReference mi in type.GetMembers ()) {
			if (MDocUpdater.GetMemberName (mi) == docName)
				yield return mi;
		}
	}

	static string GetReplacedString (string typeName, string[] from, string[] to)
	{
		if (from == null)
			return typeName;
		for (int i = 0; i < from.Length; ++i)
			typeName = typeName.Replace (from [i], to [i]);
		return typeName;
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
}

class EcmaDocumentationEnumerator : DocumentationEnumerator {

	XmlReader ecmadocs;
	MDocUpdater app;

	public EcmaDocumentationEnumerator (MDocUpdater app, XmlReader ecmaDocs)
	{
		this.app      = app;
		this.ecmadocs = ecmaDocs;
	}

	public override IEnumerable<TypeDefinition> GetDocumentationTypes (AssemblyDefinition assembly, List<string> forTypes)
	{
		HashSet<string> seen = new HashSet<string> ();
		return GetDocumentationTypes (assembly, forTypes, seen)
			.Concat (base.GetDocumentationTypes (assembly, forTypes, seen));
	}

	IEnumerable<TypeDefinition> GetDocumentationTypes (AssemblyDefinition assembly, List<string> forTypes, HashSet<string> seen)
	{
		int typeDepth = -1;
		while (ecmadocs.Read ()) {
			switch (ecmadocs.Name) {
				case "Type": {
					if (typeDepth == -1)
						typeDepth = ecmadocs.Depth;
					if (ecmadocs.NodeType != XmlNodeType.Element)
						continue;
					if (typeDepth != ecmadocs.Depth) // nested <TypeDefinition/> element?
						continue;
					string typename = ecmadocs.GetAttribute ("FullName");
					string typename2 = MDocUpdater.GetTypeFileName (typename);
					if (forTypes != null && 
							forTypes.BinarySearch (typename) < 0 &&
							typename != typename2 &&
							forTypes.BinarySearch (typename2) < 0)
						continue;
					TypeDefinition t;
					if ((t = assembly.GetType (typename)) == null && 
							(t = assembly.GetType (typename2)) == null)
						continue;
					seen.Add (typename);
					if (typename != typename2)
						seen.Add (typename2);
					Console.WriteLine ("  Import: {0}", t.FullName);
					if (ecmadocs.Name != "Docs") {
						int depth = ecmadocs.Depth;
						while (ecmadocs.Read ()) {
							if (ecmadocs.Name == "Docs" && ecmadocs.Depth == depth + 1)
								break;
						}
					}
					if (!ecmadocs.IsStartElement ("Docs"))
						throw new InvalidOperationException ("Found " + ecmadocs.Name + "; expecting <Docs/>!");
					yield return t;
					break;
				}
				default:
					break;
			}
		}
	}

	public override IEnumerable<DocsNodeInfo> GetDocumentationMembers (XmlDocument basefile, TypeDefinition type)
	{
		return GetMembers (basefile, type)
			.Concat (base.GetDocumentationMembers (basefile, type));
	}

	private IEnumerable<DocsNodeInfo> GetMembers (XmlDocument basefile, TypeDefinition type)
	{
		while (ecmadocs.Name != "Members" && ecmadocs.Read ()) {
			// do nothing
		}
		if (ecmadocs.IsEmptyElement)
			yield break;

		int membersDepth = ecmadocs.Depth;
		bool go = true;
		while (go && ecmadocs.Read ()) {
			switch (ecmadocs.Name) {
				case "Member": {
					if (membersDepth != ecmadocs.Depth - 1 || ecmadocs.NodeType != XmlNodeType.Element)
						continue;
					DocumentationMember dm = new DocumentationMember (ecmadocs);
					string xp = MDocUpdater.GetXPathForMember (dm);
					XmlElement oldmember = (XmlElement) basefile.SelectSingleNode (xp);
					MemberReference m;
					if (oldmember == null) {
						m = GetMember (type, dm);
						if (m == null) {
							app.Warning ("Could not import ECMA docs for `{0}'s `{1}': Member not found.",
									type.FullName, dm.MemberSignatures ["C#"]);
									// SelectSingleNode (ecmaDocsMember, "MemberSignature[@Language=\"C#\"]/@Value").Value);
							continue;
						}
						// oldmember lookup may have failed due to type parameter renames.
						// Try again.
						oldmember = (XmlElement) basefile.SelectSingleNode (MDocUpdater.GetXPathForMember (m));
						if (oldmember == null) {
							XmlElement members = MDocUpdater.WriteElement (basefile.DocumentElement, "Members");
							oldmember = basefile.CreateElement ("Member");
							oldmember.SetAttribute ("MemberName", dm.MemberName);
							members.AppendChild (oldmember);
							foreach (string key in MDocUpdater.Sort (dm.MemberSignatures.Keys)) {
								XmlElement ms = basefile.CreateElement ("MemberSignature");
								ms.SetAttribute ("Language", key);
								ms.SetAttribute ("Value", (string) dm.MemberSignatures [key]);
								oldmember.AppendChild (ms);
							}
							oldmember.SetAttribute ("__monodocer-seen__", "true");
							Console.WriteLine ("Member Added: {0}", oldmember.SelectSingleNode("MemberSignature[@Language='C#']/@Value").InnerText);
							app.additions++;
						}
					}
					else {
						m = GetMember (type, new DocumentationMember (oldmember));
						if (m == null) {
							app.Warning ("Could not import ECMA docs for `{0}'s `{1}': Member not found.",
									type.FullName, dm.MemberSignatures ["C#"]);
							continue;
						}
						oldmember.SetAttribute ("__monodocer-seen__", "true");
					}
					DocsNodeInfo node = new DocsNodeInfo (oldmember, m);
					if (ecmadocs.Name != "Docs")
						throw new InvalidOperationException ("Found " + ecmadocs.Name + "; expected <Docs/>!");
					yield return node;
					break;
				}
				case "Members":
					if (membersDepth == ecmadocs.Depth && ecmadocs.NodeType == XmlNodeType.EndElement) {
						go = false;
					}
					break;
			}
		}
	}
}

abstract class DocumentationImporter {

	public abstract void ImportDocumentation (DocsNodeInfo info);
}

class MsxdocDocumentationImporter : DocumentationImporter {

	XmlDocument slashdocs;

	public MsxdocDocumentationImporter (string file)
	{
		var xml = File.ReadAllText (file);

		// Ensure Unix line endings
		xml = xml.Replace ("\r", "");

		slashdocs = new XmlDocument();
		slashdocs.LoadXml (xml);
	}

	public override void ImportDocumentation (DocsNodeInfo info)
	{
		XmlNode elem = GetDocs (info.Member ?? info.Type);

		if (elem == null)
			return;

		XmlElement e = info.Node;

		if (elem.SelectSingleNode("summary") != null)
			MDocUpdater.ClearElement(e, "summary");
		if (elem.SelectSingleNode("remarks") != null)
			MDocUpdater.ClearElement(e, "remarks");
		if (elem.SelectSingleNode ("value") != null || elem.SelectSingleNode ("returns") != null) {
			MDocUpdater.ClearElement(e, "value");
			MDocUpdater.ClearElement(e, "returns");
		}

		foreach (XmlNode child in elem.ChildNodes) {
			switch (child.Name) {
				case "param":
				case "typeparam": {
					XmlAttribute name = child.Attributes ["name"];
					if (name == null)
						break;
					XmlElement p2 = (XmlElement) e.SelectSingleNode (child.Name + "[@name='" + name.Value + "']");
					if (p2 != null)
						p2.InnerXml = child.InnerXml;
					break;
				}
				// Occasionally XML documentation will use <returns/> on
				// properties, so let's try to normalize things.
				case "value":
				case "returns": {
					XmlElement v = e.OwnerDocument.CreateElement (info.ReturnIsReturn ? "returns" : "value");
					v.InnerXml = child.InnerXml;
					e.AppendChild (v);
					break;
				}
				case "altmember":
				case "exception":
				case "permission": {
					XmlAttribute cref = child.Attributes ["cref"] ?? child.Attributes ["name"];
					if (cref == null)
						break;
					XmlElement a = (XmlElement) e.SelectSingleNode (child.Name + "[@cref='" + cref.Value + "']");
					if (a == null) {
						a = e.OwnerDocument.CreateElement (child.Name);
						a.SetAttribute ("cref", child.Attributes ["cref"].Value);
						e.AppendChild (a);
					}
					a.InnerXml = child.InnerXml;
					break;
				}
				case "seealso": {
					XmlAttribute cref = child.Attributes ["cref"];
					if (cref == null)
						break;
					XmlElement a = (XmlElement) e.SelectSingleNode ("altmember[@cref='" + cref.Value + "']");
					if (a == null) {
						a = e.OwnerDocument.CreateElement ("altmember");
						a.SetAttribute ("cref", child.Attributes ["cref"].Value);
						e.AppendChild (a);
					}
					break;
				}
				default: {
					bool add = true;
					if (child.NodeType == XmlNodeType.Element && 
							e.SelectNodes (child.Name).Cast<XmlElement>().Any (n => n.OuterXml == child.OuterXml))
						add = false;
					if (add)
						MDocUpdater.CopyNode (child, e);
					break;
				}
			}
		}
	}

	private XmlNode GetDocs (MemberReference member)
	{
		string slashdocsig = MDocUpdater.slashdocFormatter.GetDeclaration (member);
		if (slashdocsig != null)
			return slashdocs.SelectSingleNode ("doc/members/member[@name='" + slashdocsig + "']");
		return null;
	}
}

class EcmaDocumentationImporter : DocumentationImporter {

	XmlReader ecmadocs;

	public EcmaDocumentationImporter (XmlReader ecmaDocs)
	{
		this.ecmadocs = ecmaDocs;
	}

	public override void ImportDocumentation (DocsNodeInfo info)
	{
		if (!ecmadocs.IsStartElement ("Docs")) {
			return;
		}

		XmlElement e = info.Node;

		int depth = ecmadocs.Depth;
		ecmadocs.ReadStartElement ("Docs");
		while (ecmadocs.Read ()) {
			if (ecmadocs.Name == "Docs") {
				if (ecmadocs.Depth == depth && ecmadocs.NodeType == XmlNodeType.EndElement)
					break;
				else
					throw new InvalidOperationException ("Skipped past current <Docs/> element!");
			}
			if (!ecmadocs.IsStartElement ())
				continue;
			switch (ecmadocs.Name) {
				case "param":
				case "typeparam": {
					string name = ecmadocs.GetAttribute ("name");
					if (name == null)
						break;
					XmlNode doc = e.SelectSingleNode (
							ecmadocs.Name + "[@name='" + name + "']");
					string value = ecmadocs.ReadInnerXml ();
					if (doc != null)
						doc.InnerXml = value.Replace ("\r", "");
					break;
				}
				case "altmember":
				case "exception":
				case "permission":
				case "seealso": {
					string name = ecmadocs.Name;
					string cref = ecmadocs.GetAttribute ("cref");
					if (cref == null)
						break;
					XmlNode doc = e.SelectSingleNode (
							ecmadocs.Name + "[@cref='" + cref + "']");
					string value = ecmadocs.ReadInnerXml ().Replace ("\r", "");
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
					string name = ecmadocs.Name;
					string xpath = ecmadocs.Name;
					StringList attributes = new StringList (ecmadocs.AttributeCount);
					if (ecmadocs.MoveToFirstAttribute ()) {
						do {
							attributes.Add ("@" + ecmadocs.Name + "=\"" + ecmadocs.Value + "\"");
						} while (ecmadocs.MoveToNextAttribute ());
						ecmadocs.MoveToContent ();
					}
					if (attributes.Count > 0) {
						xpath += "[" + string.Join (" and ", attributes.ToArray ()) + "]";
					}
					XmlNode doc = e.SelectSingleNode (xpath);
					string value = ecmadocs.ReadInnerXml ().Replace ("\r", "");
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

public enum MemberFormatterState {
	None,
	WithinArray,
	WithinGenericTypeParameters,
}

public abstract class MemberFormatter {

	public virtual string Language {
		get {return "";}
	}

	public virtual string GetName (MemberReference member)
	{
		TypeReference type = member as TypeReference;
		if (type != null)
			return GetTypeName (type);
		MethodReference method  = member as MethodReference;
		if (method != null && method.Name == ".ctor") // method.IsConstructor
			return GetConstructorName (method);
		if (method != null)
			return GetMethodName (method);
		PropertyReference prop = member as PropertyReference;
		if (prop != null)
			return GetPropertyName (prop);
		FieldReference field = member as FieldReference;
		if (field != null)
			return GetFieldName (field);
		EventReference e = member as EventReference;
		if (e != null)
			return GetEventName (e);
		throw new NotSupportedException ("Can't handle: " +
					(member == null ? "null" : member.GetType().ToString()));
	}

	protected virtual string GetTypeName (TypeReference type)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		return _AppendTypeName (new StringBuilder (type.Name.Length), type).ToString ();
	}

	protected virtual char[] ArrayDelimeters {
		get {return new char[]{'[', ']'};}
	}

	protected virtual MemberFormatterState MemberFormatterState { get; set; }

	protected StringBuilder _AppendTypeName (StringBuilder buf, TypeReference type)
	{
		if (type is ArrayType) {
			TypeSpecification spec = type as TypeSpecification;
			_AppendTypeName (buf, spec != null ? spec.ElementType : type.GetElementType ())
					.Append (ArrayDelimeters [0]);
			var origState = MemberFormatterState;
			MemberFormatterState = MemberFormatterState.WithinArray;
			ArrayType array = (ArrayType) type;
			int rank = array.Rank;
			if (rank > 1)
				buf.Append (new string (',', rank-1));
			MemberFormatterState = origState;
			return buf.Append (ArrayDelimeters [1]);
		}
		if (type is ByReferenceType) {
			return AppendRefTypeName (buf, type);
		}
		if (type is PointerType) {
			return AppendPointerTypeName (buf, type);
		}
		AppendNamespace (buf, type);
		if (type is GenericParameter) {
			return AppendTypeName (buf, type);
		}
		GenericInstanceType genInst = type as GenericInstanceType;
		if (type.GenericParameters.Count == 0 &&
				(genInst == null ? true : genInst.GenericArguments.Count == 0)) {
			return AppendFullTypeName (buf, type);
		}
		return AppendGenericType (buf, type);
	}

	protected virtual StringBuilder AppendNamespace (StringBuilder buf, TypeReference type)
	{
		string ns = DocUtils.GetNamespace (type);
		if (ns != null && ns.Length > 0)
			buf.Append (ns).Append ('.');
		return buf;
	}

	protected virtual StringBuilder AppendFullTypeName (StringBuilder buf, TypeReference type)
	{
		if (type.DeclaringType != null)
			AppendFullTypeName (buf, type.DeclaringType).Append (NestedTypeSeparator);
		return AppendTypeName (buf, type);
	}

	protected virtual StringBuilder AppendTypeName (StringBuilder buf, TypeReference type)
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

	protected virtual StringBuilder AppendRefTypeName (StringBuilder buf, TypeReference type)
	{
		TypeSpecification spec = type as TypeSpecification;
		return _AppendTypeName (buf, spec != null ? spec.ElementType : type.GetElementType ())
				.Append (RefTypeModifier);
	}

	protected virtual string PointerModifier {
		get {return "*";}
	}

	protected virtual StringBuilder AppendPointerTypeName (StringBuilder buf, TypeReference type)
	{
		TypeSpecification spec = type as TypeSpecification;
		return _AppendTypeName (buf, spec != null ? spec.ElementType : type.GetElementType ())
				.Append (PointerModifier);
	}

	protected virtual char[] GenericTypeContainer {
		get {return new char[]{'<', '>'};}
	}

	protected virtual char NestedTypeSeparator {
		get {return '.';}
	}

	protected virtual StringBuilder AppendGenericType (StringBuilder buf, TypeReference type)
	{
		List<TypeReference> decls = DocUtils.GetDeclaringTypes (
				type is GenericInstanceType ? type.GetElementType () : type);
		List<TypeReference> genArgs = GetGenericArguments (type);
		int argIdx = 0;
		int prev = 0;
		bool insertNested = false;
		foreach (var decl in decls) {
			TypeReference declDef = decl.Resolve () ?? decl;
			if (insertNested) {
				buf.Append (NestedTypeSeparator);
			}
			insertNested = true;
			AppendTypeName (buf, declDef);
			int ac = DocUtils.GetGenericArgumentCount (declDef);
			int c = ac - prev;
			prev = ac;
			if (c > 0) {
				buf.Append (GenericTypeContainer [0]);
				var origState = MemberFormatterState;
				MemberFormatterState = MemberFormatterState.WithinGenericTypeParameters;
				_AppendTypeName (buf, genArgs [argIdx++]);
				for (int i = 1; i < c; ++i)
					_AppendTypeName (buf.Append (","), genArgs [argIdx++]);
				MemberFormatterState = origState;
				buf.Append (GenericTypeContainer [1]);
			}
		}
		return buf;
	}

	protected List<TypeReference> GetGenericArguments (TypeReference type)
	{
		var args = new List<TypeReference> ();
		GenericInstanceType inst = type as GenericInstanceType;
		if (inst != null)
			args.AddRange (inst.GenericArguments.Cast<TypeReference> ());
		else
			args.AddRange (type.GenericParameters.Cast<TypeReference> ());
		return args;
	}

	protected virtual StringBuilder AppendGenericTypeConstraints (StringBuilder buf, TypeReference type)
	{
		return buf;
	}

	protected virtual string GetConstructorName (MethodReference constructor)
	{
		return constructor.Name;
	}

	protected virtual string GetMethodName (MethodReference method)
	{
		return method.Name;
	}

	protected virtual string GetPropertyName (PropertyReference property)
	{
		return property.Name;
	}

	protected virtual string GetFieldName (FieldReference field)
	{
		return field.Name;
	}

	protected virtual string GetEventName (EventReference e)
	{
		return e.Name;
	}

	public virtual string GetDeclaration (MemberReference member)
	{
		if (member == null)
			throw new ArgumentNullException ("member");
		TypeDefinition type = member as TypeDefinition;
		if (type != null)
			return GetTypeDeclaration (type);
		MethodDefinition method = member as MethodDefinition;
		if (method != null && method.IsConstructor)
			return GetConstructorDeclaration (method);
		if (method != null)
			return GetMethodDeclaration (method);
		PropertyDefinition prop = member as PropertyDefinition;
		if (prop != null)
			return GetPropertyDeclaration (prop);
		FieldDefinition field = member as FieldDefinition;
		if (field != null)
			return GetFieldDeclaration (field);
		EventDefinition e = member as EventDefinition;
		if (e != null)
			return GetEventDeclaration (e);
		throw new NotSupportedException ("Can't handle: " + member.GetType().ToString());
	}

	protected virtual string GetTypeDeclaration (TypeDefinition type)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		StringBuilder buf = new StringBuilder (type.Name.Length);
		_AppendTypeName (buf, type);
		AppendGenericTypeConstraints (buf, type);
		return buf.ToString ();
	}

	protected virtual string GetConstructorDeclaration (MethodDefinition constructor)
	{
		return GetConstructorName (constructor);
	}

	protected virtual string GetMethodDeclaration (MethodDefinition method)
	{
		// Special signature for destructors.
		if (method.Name == "Finalize" && method.Parameters.Count == 0)
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
		AppendParameters (buf, method, method.Parameters);
		AppendGenericMethodConstraints (buf, method);
		return buf.ToString ();
	}

	protected virtual StringBuilder AppendMethodName (StringBuilder buf, MethodDefinition method)
	{
		return buf.Append (method.Name);
	}

	protected virtual string GetFinalizerName (MethodDefinition method)
	{
		return "Finalize";
	}

	protected virtual StringBuilder AppendVisibility (StringBuilder buf, MethodDefinition method)
	{
		return buf;
	}

	protected virtual StringBuilder AppendModifiers (StringBuilder buf, MethodDefinition method)
	{
		return buf;
	}

	protected virtual StringBuilder AppendGenericMethod (StringBuilder buf, MethodDefinition method)
	{
		return buf;
	}

	protected virtual StringBuilder AppendParameters (StringBuilder buf, MethodDefinition method, IList<ParameterDefinition> parameters)
	{
		return buf;
	}

	protected virtual StringBuilder AppendGenericMethodConstraints (StringBuilder buf, MethodDefinition method)
	{
		return buf;
	}

	protected virtual string GetPropertyDeclaration (PropertyDefinition property)
	{
		return GetPropertyName (property);
	}

	protected virtual string GetFieldDeclaration (FieldDefinition field)
	{
		return GetFieldName (field);
	}

	protected virtual string GetEventDeclaration (EventDefinition e)
	{
		return GetEventName (e);
	}
}

class ILFullMemberFormatter : MemberFormatter {

	public override string Language {
		get {return "ILAsm";}
	}

	protected override char NestedTypeSeparator {
		get {
			return '/';
		}
	}

	protected override StringBuilder AppendNamespace (StringBuilder buf, TypeReference type)
	{
		if (GetBuiltinType (type.FullName) != null)
			return buf;
		string ns = DocUtils.GetNamespace (type);
		if (ns != null && ns.Length > 0) {
			if (type.IsValueType)
				buf.Append ("valuetype ");
			else
				buf.Append ("class ");
			buf.Append (ns).Append ('.');
		}
		return buf;
	}

	private static string GetBuiltinType (string t)
	{
		switch (t) {
		case "System.Byte":    return "unsigned int8";
		case "System.SByte":   return "int8";
		case "System.Int16":   return "int16";
		case "System.Int32":   return "int32";
		case "System.Int64":   return "int64";
		case "System.IntPtr":  return "native int";

		case "System.UInt16":  return "unsigned int16";
		case "System.UInt32":  return "unsigned int32";
		case "System.UInt64":  return "unsigned int64";
		case "System.UIntPtr": return "native unsigned int";

		case "System.Single":  return "float32";
		case "System.Double":  return "float64";
		case "System.Boolean": return "bool";
		case "System.Char":    return "char";
		case "System.Void":    return "void";
		case "System.String":  return "string";
		case "System.Object":  return "object";
		}
		return null;
	}

	protected override StringBuilder AppendTypeName (StringBuilder buf, string typename)
	{
		return buf.Append (typename);
	}

	protected override StringBuilder AppendTypeName (StringBuilder buf, TypeReference type)
	{
		var gp = type as GenericParameter;
		if (type is GenericParameter)
			return AppendGenericParameterConstraints (buf, (GenericParameter) type).Append (type.Name);

		string s = GetBuiltinType (type.FullName);
		if (s != null)
			return buf.Append (s);
		return base.AppendTypeName (buf, type);
	}

	private StringBuilder AppendGenericParameterConstraints (StringBuilder buf, GenericParameter type)
	{
		if (MemberFormatterState != MemberFormatterState.WithinGenericTypeParameters) {
			return buf.Append (type.Owner is TypeReference ? "!" : "!!");
		}
		GenericParameterAttributes attrs = type.Attributes;
		if ((attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
			buf.Append ("class ");
		if ((attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
			buf.Append ("struct ");
		if ((attrs & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
			buf.Append (".ctor ");
		IList<TypeReference> constraints = type.Constraints;
		MemberFormatterState = 0;
		if (constraints.Count > 0) {
			var full = new ILFullMemberFormatter ();
			buf.Append ("(").Append (full.GetName (constraints [0]));
			for (int i = 1; i < constraints.Count; ++i) {
				buf.Append (", ").Append (full.GetName (constraints [i]));
			}
			buf.Append (") ");
		}
		MemberFormatterState = MemberFormatterState.WithinGenericTypeParameters;

		if ((attrs & GenericParameterAttributes.Covariant) != 0)
			buf.Append ("+ ");
		if ((attrs & GenericParameterAttributes.Contravariant) != 0)
			buf.Append ("- ");
		return buf;
	}

	protected override string GetTypeDeclaration (TypeDefinition type)
	{
		string visibility = GetTypeVisibility (type.Attributes);
		if (visibility == null)
			return null;

		StringBuilder buf = new StringBuilder ();

		buf.Append (".class ");
		if (type.IsNested)
			buf.Append ("nested ");
		buf.Append (visibility).Append (" ");
		if (type.IsInterface)
			buf.Append ("interface ");
		if (type.IsSequentialLayout)
			buf.Append ("sequential ");
		if (type.IsAutoLayout)
			buf.Append ("auto ");
		if (type.IsAnsiClass)
			buf.Append ("ansi ");
		if (type.IsAbstract)
			buf.Append ("abstract ");
		if (type.IsSerializable)
			buf.Append ("serializable ");
		if (type.IsSealed)
			buf.Append ("sealed ");
		if (type.IsBeforeFieldInit)
			buf.Append ("beforefieldinit ");
		var state = MemberFormatterState;
		MemberFormatterState = MemberFormatterState.WithinGenericTypeParameters;
		buf.Append (GetName (type));
		MemberFormatterState = state;
		var full = new ILFullMemberFormatter ();
		if (type.BaseType != null) {
			buf.Append (" extends ");
			if (type.BaseType.FullName == "System.Object")
				buf.Append ("System.Object");
			else
				buf.Append (full.GetName (type.BaseType).Substring ("class ".Length));
		}
		bool first = true;
		foreach (var name in type.Interfaces.Cast<TypeReference>()
				.Select (i => full.GetName (i))
				.OrderBy (n => n)) {
			if (first) {
				buf.Append (" implements ");
				first = false;
			}
			else {
				buf.Append (", ");
			}
			buf.Append (name);
		}

		return buf.ToString ();
	}

	protected override StringBuilder AppendGenericType (StringBuilder buf, TypeReference type)
	{
		List<TypeReference> decls = DocUtils.GetDeclaringTypes (
				type is GenericInstanceType ? type.GetElementType () : type);
		bool first = true;
		foreach (var decl in decls) {
			TypeReference declDef = decl.Resolve () ?? decl;
			if (!first) {
				buf.Append (NestedTypeSeparator);
			}
			first = false;
			AppendTypeName (buf, declDef);
		}
		buf.Append ('<');
		first = true;
		foreach (TypeReference arg in GetGenericArguments (type)) {
			if (!first)
				buf.Append (", ");
			first = false;
			_AppendTypeName (buf, arg);
		}
		buf.Append ('>');
		return buf;
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

	protected override string GetConstructorDeclaration (MethodDefinition constructor)
	{
		return GetMethodDeclaration (constructor);
	}

	protected override string GetMethodDeclaration (MethodDefinition method)
	{
		if (method.IsPrivate && !DocUtils.IsExplicitlyImplemented (method))
			return null;

		var buf = new StringBuilder ();
		buf.Append (".method ");
		AppendVisibility (buf, method);
		if (method.IsStatic)
			buf.Append ("static ");
		if (method.IsHideBySig)
			buf.Append ("hidebysig ");
		if (method.IsPInvokeImpl) {
			var info = method.PInvokeInfo;
			buf.Append ("pinvokeimpl (\"")
				.Append (info.Module.Name)
				.Append ("\" as \"")
				.Append (info.EntryPoint)
				.Append ("\"");
			if (info.IsCharSetAuto)
				buf.Append (" auto");
			if (info.IsCharSetUnicode)
				buf.Append (" unicode");
			if (info.IsCharSetAnsi)
				buf.Append (" ansi");
			if (info.IsCallConvCdecl)
				buf.Append (" cdecl");
			if (info.IsCallConvStdCall)
				buf.Append (" stdcall");
			if (info.IsCallConvWinapi)
				buf.Append (" winapi");
			if (info.IsCallConvThiscall)
				buf.Append (" thiscall");
			if (info.SupportsLastError)
				buf.Append (" lasterr");
			buf.Append (")");
		}
		if (method.IsSpecialName)
			buf.Append ("specialname ");
		if (method.IsRuntimeSpecialName)
			buf.Append ("rtspecialname ");
		if (method.IsNewSlot)
			buf.Append ("newslot ");
		if (method.IsVirtual)
			buf.Append ("virtual ");
		if (!method.IsStatic)
			buf.Append ("instance ");
		_AppendTypeName (buf, method.ReturnType);
		buf.Append (' ')
			.Append (method.Name);
		if (method.IsGenericMethod ()) {
			var state = MemberFormatterState;
			MemberFormatterState = MemberFormatterState.WithinGenericTypeParameters;
			IList<GenericParameter> args = method.GenericParameters;
			if (args.Count > 0) {
				buf.Append ("<");
				_AppendTypeName (buf, args [0]);
				for (int i = 1; i < args.Count; ++i)
					_AppendTypeName (buf.Append (", "), args [i]);
				buf.Append (">");
			}
			MemberFormatterState = state;
		}

		buf.Append ('(');
		bool first = true;
		for (int i = 0; i < method.Parameters.Count; ++i) {
			if (!first)
				buf.Append (", ");
			first = false;
			_AppendTypeName (buf, method.Parameters [i].ParameterType);
			buf.Append (' ');
			buf.Append (method.Parameters [i].Name);
		}
		buf.Append (')');
		if (method.IsIL)
			buf.Append (" cil");
		if (method.IsRuntime)
			buf.Append (" runtime");
		if (method.IsManaged)
			buf.Append (" managed");

		return buf.ToString ();
	}

	protected override StringBuilder AppendMethodName (StringBuilder buf, MethodDefinition method)
	{
		if (DocUtils.IsExplicitlyImplemented (method)) {
			TypeReference iface;
			MethodReference ifaceMethod;
			DocUtils.GetInfoForExplicitlyImplementedMethod (method, out iface, out ifaceMethod);
			return buf.Append (new CSharpMemberFormatter ().GetName (iface))
				.Append ('.')
				.Append (ifaceMethod.Name);
		}
		return base.AppendMethodName (buf, method);
	}

	protected override string RefTypeModifier {
		get {return "";}
	}

	protected override StringBuilder AppendVisibility (StringBuilder buf, MethodDefinition method)
	{
		if (method.IsPublic)
			return buf.Append ("public ");
		if (method.IsFamilyAndAssembly)
			return buf.Append ("familyandassembly");
		if (method.IsFamilyOrAssembly)
			return buf.Append ("familyorassembly");
		if (method.IsFamily)
			return buf.Append ("family");
		return buf;
	}

	protected override StringBuilder AppendModifiers (StringBuilder buf, MethodDefinition method)
	{
		string modifiers = String.Empty;
		if (method.IsStatic) modifiers += " static";
		if (method.IsVirtual && !method.IsAbstract) {
			if ((method.Attributes & MethodAttributes.NewSlot) != 0) modifiers += " virtual";
			else modifiers += " override";
		}
		TypeDefinition declType = (TypeDefinition) method.DeclaringType;
		if (method.IsAbstract && !declType.IsInterface) modifiers += " abstract";
		if (method.IsFinal) modifiers += " sealed";
		if (modifiers == " virtual sealed") modifiers = "";

		return buf.Append (modifiers);
	}

	protected override StringBuilder AppendGenericMethod (StringBuilder buf, MethodDefinition method)
	{
		if (method.IsGenericMethod ()) {
			IList<GenericParameter> args = method.GenericParameters;
			if (args.Count > 0) {
				buf.Append ("<");
				buf.Append (args [0].Name);
				for (int i = 1; i < args.Count; ++i)
					buf.Append (",").Append (args [i].Name);
				buf.Append (">");
			}
		}
		return buf;
	}

	protected override StringBuilder AppendParameters (StringBuilder buf, MethodDefinition method, IList<ParameterDefinition> parameters)
	{
		return AppendParameters (buf, method, parameters, '(', ')');
	}

	private StringBuilder AppendParameters (StringBuilder buf, MethodDefinition method, IList<ParameterDefinition> parameters, char begin, char end)
	{
		buf.Append (begin);

		if (parameters.Count > 0) {
			if (DocUtils.IsExtensionMethod (method))
				buf.Append ("this ");
			AppendParameter (buf, parameters [0]);
			for (int i = 1; i < parameters.Count; ++i) {
				buf.Append (", ");
				AppendParameter (buf, parameters [i]);
			}
		}

		return buf.Append (end);
	}

	private StringBuilder AppendParameter (StringBuilder buf, ParameterDefinition parameter)
	{
		if (parameter.ParameterType is ByReferenceType) {
			if (parameter.IsOut)
				buf.Append ("out ");
			else
				buf.Append ("ref ");
		}
		buf.Append (GetName (parameter.ParameterType)).Append (" ");
		return buf.Append (parameter.Name);
	}

	protected override string GetPropertyDeclaration (PropertyDefinition property)
	{
		MethodDefinition gm = null, sm = null;

		string get_visible = null;
		if ((gm = property.GetMethod) != null &&
				(DocUtils.IsExplicitlyImplemented (gm) ||
				 (!gm.IsPrivate && !gm.IsAssembly && !gm.IsFamilyAndAssembly)))
			get_visible = AppendVisibility (new StringBuilder (), gm).ToString ();
		string set_visible = null;
		if ((sm = property.SetMethod) != null &&
				(DocUtils.IsExplicitlyImplemented (sm) ||
				 (!sm.IsPrivate && !sm.IsAssembly && !sm.IsFamilyAndAssembly)))
			set_visible = AppendVisibility (new StringBuilder (), sm).ToString ();

		if ((set_visible == null) && (get_visible == null))
			return null;

		string visibility;
		StringBuilder buf = new StringBuilder ()
			.Append (".property ");
		if (!(gm ?? sm).IsStatic)
			buf.Append ("instance ");
		_AppendTypeName (buf, property.PropertyType);
		buf.Append (' ').Append (property.Name);
		if (!property.HasParameters || property.Parameters.Count == 0)
			return buf.ToString ();

		buf.Append ('(');
		bool first = true;
		foreach (ParameterDefinition p in property.Parameters) {
			if (!first)
				buf.Append (", ");
			first = false;
			_AppendTypeName (buf, p.ParameterType);
		}
		buf.Append (')');

		return buf.ToString ();
	}

	protected override string GetFieldDeclaration (FieldDefinition field)
	{
		TypeDefinition declType = (TypeDefinition) field.DeclaringType;
		if (declType.IsEnum && field.Name == "value__")
			return null; // This member of enums aren't documented.

		StringBuilder buf = new StringBuilder ();
		AppendFieldVisibility (buf, field);
		if (buf.Length == 0)
			return null;

		buf.Insert (0, ".field ");

		if (field.IsStatic)
			buf.Append ("static ");
		if (field.IsInitOnly)
			buf.Append ("initonly ");
		if (field.IsLiteral)
			buf.Append ("literal ");
		_AppendTypeName (buf, field.FieldType);
		buf.Append (' ').Append (field.Name);
		AppendFieldValue (buf, field);

		return buf.ToString ();
	}

	static StringBuilder AppendFieldVisibility (StringBuilder buf, FieldDefinition field)
	{
		if (field.IsPublic)
			return buf.Append ("public ");
		if (field.IsFamilyAndAssembly)
			return buf.Append ("familyandassembly ");
		if (field.IsFamilyOrAssembly)
			return buf.Append ("familyorassembly ");
		if (field.IsFamily)
			return buf.Append ("family ");
		return buf;
	}

	static StringBuilder AppendFieldValue (StringBuilder buf, FieldDefinition field)
	{
		// enums have a value__ field, which we ignore
		if (field.DeclaringType.IsGenericType ())
			return buf;
		if (field.HasConstant && field.IsLiteral) {
			object val = null;
			try {
				val   = field.Constant;
			} catch {
				return buf;
			}
			if (val == null)
				buf.Append (" = ").Append ("null");
			else if (val is Enum)
				buf.Append (" = ")
					.Append (GetBuiltinType (field.DeclaringType.GetUnderlyingType ().FullName))
					.Append ('(')
					.Append (val.ToString ())
					.Append (')');
			else if (val is IFormattable) {
				string value = ((IFormattable)val).ToString();
				buf.Append (" = ");
				if (val is string)
					buf.Append ("\"" + value + "\"");
				else
					buf.Append (GetBuiltinType (field.DeclaringType.GetUnderlyingType ().FullName))
						.Append ('(')
						.Append (value)
						.Append (')');
			}
		}
		return buf;
	}

	protected override string GetEventDeclaration (EventDefinition e)
	{
		StringBuilder buf = new StringBuilder ();
		if (AppendVisibility (buf, e.AddMethod).Length == 0) {
			return null;
		}

		buf.Length = 0;
		buf.Append (".event ")
			.Append (GetName (e.EventType))
			.Append (' ')
			.Append (e.Name);

		return buf.ToString ();
	}
}

class ILMemberFormatter : ILFullMemberFormatter {
	protected override StringBuilder AppendNamespace (StringBuilder buf, TypeReference type)
	{
		return buf;
	}
}

class CSharpFullMemberFormatter : MemberFormatter {

	public override string Language {
		get {return "C#";}
	}

	protected override StringBuilder AppendNamespace (StringBuilder buf, TypeReference type)
	{
		string ns = DocUtils.GetNamespace (type);
		if (GetCSharpType (type.FullName) == null && ns != null && ns.Length > 0 && ns != "System")
			buf.Append (ns).Append ('.');
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

	protected override StringBuilder AppendTypeName (StringBuilder buf, TypeReference type)
	{
		if (type is GenericParameter)
			return AppendGenericParameterConstraints (buf, (GenericParameter) type).Append (type.Name);
		string t = type.FullName;
		if (!t.StartsWith ("System.")) {
			return base.AppendTypeName (buf, type);
		}

		string s = GetCSharpType (t);
		if (s != null)
			return buf.Append (s);
		
		return base.AppendTypeName (buf, type);
	}

	private StringBuilder AppendGenericParameterConstraints (StringBuilder buf, GenericParameter type)
	{
		if (MemberFormatterState != MemberFormatterState.WithinGenericTypeParameters)
			return buf;
		GenericParameterAttributes attrs = type.Attributes;
		bool isout = (attrs & GenericParameterAttributes.Covariant) != 0;
		bool isin  = (attrs & GenericParameterAttributes.Contravariant) != 0;
		if (isin)
			buf.Append ("in ");
		else if (isout)
			buf.Append ("out ");
		return buf;
	}

	protected override string GetTypeDeclaration (TypeDefinition type)
	{
		string visibility = GetTypeVisibility (type.Attributes);
		if (visibility == null)
			return null;

		StringBuilder buf = new StringBuilder ();
		
		buf.Append (visibility);
		buf.Append (" ");

		MemberFormatter full = new CSharpFullMemberFormatter ();

		if (DocUtils.IsDelegate (type)) {
			buf.Append("delegate ");
			MethodDefinition invoke = type.GetMethod ("Invoke");
			buf.Append (full.GetName (invoke.ReturnType)).Append (" ");
			buf.Append (GetName (type));
			AppendParameters (buf, invoke, invoke.Parameters);
			AppendGenericTypeConstraints (buf, type);
			buf.Append (";");

			return buf.ToString();
		}
		
		if (type.IsAbstract && !type.IsInterface)
			buf.Append("abstract ");
		if (type.IsSealed && !DocUtils.IsDelegate (type) && !type.IsValueType)
			buf.Append("sealed ");
		buf.Replace ("abstract sealed", "static");

		buf.Append (GetTypeKind (type));
		buf.Append (" ");
		buf.Append (GetCSharpType (type.FullName) == null 
				? GetName (type) 
				: type.Name);

		if (!type.IsEnum) {
			TypeReference basetype = type.BaseType;
			if (basetype != null && basetype.FullName == "System.Object" || type.IsValueType)	// FIXME
				basetype = null;

			List<string> interface_names = DocUtils.GetUserImplementedInterfaces (type)
					.Select (iface => full.GetName (iface))
					.OrderBy (s => s)
					.ToList ();

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

	static string GetTypeKind (TypeDefinition t)
	{
		if (t.IsEnum)
			return "enum";
		if (t.IsValueType)
			return "struct";
		if (t.IsClass || t.FullName == "System.Enum")
			return "class";
		if (t.IsInterface)
			return "interface";
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

	protected override StringBuilder AppendGenericTypeConstraints (StringBuilder buf, TypeReference type)
	{
		if (type.GenericParameters.Count == 0)
			return buf;
		return AppendConstraints (buf, type.GenericParameters);
	}

	private StringBuilder AppendConstraints (StringBuilder buf, IList<GenericParameter> genArgs)
	{
		foreach (GenericParameter genArg in genArgs) {
			GenericParameterAttributes attrs = genArg.Attributes;
			IList<TypeReference> constraints = genArg.Constraints;
			if (attrs == GenericParameterAttributes.NonVariant && constraints.Count == 0)
				continue;

			bool isref = (attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
			bool isvt  = (attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
			bool isnew = (attrs & GenericParameterAttributes.DefaultConstructorConstraint) != 0;
			bool comma = false;

			if (!isref && !isvt && !isnew && constraints.Count == 0)
				continue;
			buf.Append (" where ").Append (genArg.Name).Append (" : ");
			if (isref) {
				buf.Append ("class");
				comma = true;
			}
			else if (isvt) {
				buf.Append ("struct");
				comma = true;
			}
			if (constraints.Count > 0 && !isvt) {
				if (comma)
					buf.Append (", ");
				buf.Append (GetTypeName (constraints [0]));
				for (int i = 1; i < constraints.Count; ++i)
					buf.Append (", ").Append (GetTypeName (constraints [i]));
			}
			if (isnew && !isvt) {
				if (comma)
					buf.Append (", ");
				buf.Append ("new()");
			}
		}
		return buf;
	}

	protected override string GetConstructorDeclaration (MethodDefinition constructor)
	{
		StringBuilder buf = new StringBuilder ();
		AppendVisibility (buf, constructor);
		if (buf.Length == 0)
			return null;

		buf.Append (' ');
		base.AppendTypeName (buf, constructor.DeclaringType.Name).Append (' ');
		AppendParameters (buf, constructor, constructor.Parameters);
		buf.Append (';');

		return buf.ToString ();
	}
	
	protected override string GetMethodDeclaration (MethodDefinition method)
	{
		string decl = base.GetMethodDeclaration (method);
		if (decl != null)
			return decl + ";";
		return null;
	}

	protected override StringBuilder AppendMethodName (StringBuilder buf, MethodDefinition method)
	{
		if (DocUtils.IsExplicitlyImplemented (method)) {
			TypeReference iface;
			MethodReference ifaceMethod;
			DocUtils.GetInfoForExplicitlyImplementedMethod (method, out iface, out ifaceMethod);
			return buf.Append (new CSharpMemberFormatter ().GetName (iface))
				.Append ('.')
				.Append (ifaceMethod.Name);
		}
		return base.AppendMethodName (buf, method);
	}

	protected override StringBuilder AppendGenericMethodConstraints (StringBuilder buf, MethodDefinition method)
	{
		if (method.GenericParameters.Count == 0)
			return buf;
		return AppendConstraints (buf, method.GenericParameters);
	}

	protected override string RefTypeModifier {
		get {return "";}
	}

	protected override string GetFinalizerName (MethodDefinition method)
	{
		return "~" + method.DeclaringType.Name + " ()";	
	}

	protected override StringBuilder AppendVisibility (StringBuilder buf, MethodDefinition method)
	{
		if (method == null)
			return buf;
		if (method.IsPublic)
			return buf.Append ("public");
		if (method.IsFamily || method.IsFamilyOrAssembly)
			return buf.Append ("protected");
		return buf;
	}

	protected override StringBuilder AppendModifiers (StringBuilder buf, MethodDefinition method)
	{
		string modifiers = String.Empty;
		if (method.IsStatic) modifiers += " static";
		if (method.IsVirtual && !method.IsAbstract) {
			if ((method.Attributes & MethodAttributes.NewSlot) != 0) modifiers += " virtual";
			else modifiers += " override";
		}
		TypeDefinition declType = (TypeDefinition) method.DeclaringType;
		if (method.IsAbstract && !declType.IsInterface) modifiers += " abstract";
		if (method.IsFinal) modifiers += " sealed";
		if (modifiers == " virtual sealed") modifiers = "";

		return buf.Append (modifiers);
	}

	protected override StringBuilder AppendGenericMethod (StringBuilder buf, MethodDefinition method)
	{
		if (method.IsGenericMethod ()) {
			IList<GenericParameter> args = method.GenericParameters;
			if (args.Count > 0) {
				buf.Append ("<");
				buf.Append (args [0].Name);
				for (int i = 1; i < args.Count; ++i)
					buf.Append (",").Append (args [i].Name);
				buf.Append (">");
			}
		}
		return buf;
	}

	protected override StringBuilder AppendParameters (StringBuilder buf, MethodDefinition method, IList<ParameterDefinition> parameters)
	{
		return AppendParameters (buf, method, parameters, '(', ')');
	}

	private StringBuilder AppendParameters (StringBuilder buf, MethodDefinition method, IList<ParameterDefinition> parameters, char begin, char end)
	{
		buf.Append (begin);

		if (parameters.Count > 0) {
			if (DocUtils.IsExtensionMethod (method))
				buf.Append ("this ");
			AppendParameter (buf, parameters [0]);
			for (int i = 1; i < parameters.Count; ++i) {
				buf.Append (", ");
				AppendParameter (buf, parameters [i]);
			}
		}

		return buf.Append (end);
	}

	private StringBuilder AppendParameter (StringBuilder buf, ParameterDefinition parameter)
	{
		if (parameter.ParameterType is ByReferenceType) {
			if (parameter.IsOut)
				buf.Append ("out ");
			else
				buf.Append ("ref ");
		}
		buf.Append (GetName (parameter.ParameterType)).Append (" ");
		return buf.Append (parameter.Name);
	}

	protected override string GetPropertyDeclaration (PropertyDefinition property)
	{
		MethodDefinition method;

		string get_visible = null;
		if ((method = property.GetMethod) != null && 
				(DocUtils.IsExplicitlyImplemented (method) || 
				 (!method.IsPrivate && !method.IsAssembly && !method.IsFamilyAndAssembly)))
			get_visible = AppendVisibility (new StringBuilder (), method).ToString ();
		string set_visible = null;
		if ((method = property.SetMethod) != null &&
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
		method = property.SetMethod;
		if (method == null)
			method = property.GetMethod;
	
		string modifiers = String.Empty;
		if (method.IsStatic) modifiers += " static";
		if (method.IsVirtual && !method.IsAbstract) {
				if ((method.Attributes & MethodAttributes.NewSlot) != 0)
					modifiers += " virtual";
				else
					modifiers += " override";
		}
		TypeDefinition declDef = (TypeDefinition) method.DeclaringType;
		if (method.IsAbstract && !declDef.IsInterface)
			modifiers += " abstract";
		if (method.IsFinal)
			modifiers += " sealed";
		if (modifiers == " virtual sealed")
			modifiers = "";
		buf.Append (modifiers).Append (' ');

		buf.Append (GetName (property.PropertyType)).Append (' ');

		IEnumerable<MemberReference> defs = property.DeclaringType.GetDefaultMembers ();
		string name = property.Name;
		foreach (MemberReference mi in defs) {
			if (mi == property) {
				name = "this";
				break;
			}
		}
		buf.Append (name == "this" ? name : DocUtils.GetPropertyName (property));
	
		if (property.Parameters.Count != 0) {
			AppendParameters (buf, method, property.Parameters, '[', ']');
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

	protected override string GetFieldDeclaration (FieldDefinition field)
	{
		TypeDefinition declType = (TypeDefinition) field.DeclaringType;
		if (declType.IsEnum && field.Name == "value__")
			return null; // This member of enums aren't documented.

		StringBuilder buf = new StringBuilder ();
		AppendFieldVisibility (buf, field);
		if (buf.Length == 0)
			return null;

		if (declType.IsEnum)
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

	static StringBuilder AppendFieldVisibility (StringBuilder buf, FieldDefinition field)
	{
		if (field.IsPublic)
			return buf.Append ("public");
		if (field.IsFamily || field.IsFamilyOrAssembly)
			return buf.Append ("protected");
		return buf;
	}

	static StringBuilder AppendFieldValue (StringBuilder buf, FieldDefinition field)
	{
		// enums have a value__ field, which we ignore
		if (((TypeDefinition ) field.DeclaringType).IsEnum || 
				field.DeclaringType.IsGenericType ())
			return buf;
		if (field.HasConstant && field.IsLiteral) {
			object val = null;
			try {
				val   = field.Constant;
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

	protected override string GetEventDeclaration (EventDefinition e)
	{
		StringBuilder buf = new StringBuilder ();
		if (AppendVisibility (buf, e.AddMethod).Length == 0) {
			return null;
		}

		AppendModifiers (buf, e.AddMethod);

		buf.Append (" event ");
		buf.Append (GetName (e.EventType)).Append (' ');
		buf.Append (e.Name).Append (';');

		return buf.ToString ();
	}
}

class CSharpMemberFormatter : CSharpFullMemberFormatter {
	protected override StringBuilder AppendNamespace (StringBuilder buf, TypeReference type)
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
	protected override StringBuilder AppendNamespace (StringBuilder buf, TypeReference type)
	{
		return buf;
	}
}

class SlashDocMemberFormatter : MemberFormatter {

	protected override char[] GenericTypeContainer {
		get {return new char[]{'{', '}'};}
	}

	private bool AddTypeCount = true;

	private TypeReference genDeclType;
	private MethodReference genDeclMethod;

	protected override StringBuilder AppendTypeName (StringBuilder buf, TypeReference type)
	{
		if (type is GenericParameter) {
			int l = buf.Length;
			if (genDeclType != null) {
				IList<GenericParameter> genArgs = genDeclType.GenericParameters;
				for (int i = 0; i < genArgs.Count; ++i) {
					if (genArgs [i].Name == type.Name) {
						buf.Append ('`').Append (i);
						break;
					}
				}
			}
			if (genDeclMethod != null) {
				IList<GenericParameter> genArgs = null;
				if (genDeclMethod.IsGenericMethod ()) {
					genArgs = genDeclMethod.GenericParameters;
					for (int i = 0; i < genArgs.Count; ++i) {
						if (genArgs [i].Name == type.Name) {
							buf.Append ("``").Append (i);
							break;
						}
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
				int numArgs = type.GenericParameters.Count;
				if (type.DeclaringType != null)
					numArgs -= type.GenericParameters.Count;
				if (numArgs > 0) {
					buf.Append ('`').Append (numArgs);
				}
			}
		}
		return buf;
	}

	protected override StringBuilder AppendGenericType (StringBuilder buf, TypeReference type)
	{
		if (!AddTypeCount)
			base.AppendGenericType (buf, type);
		else
			AppendType (buf, type);
		return buf;
	}

	private StringBuilder AppendType (StringBuilder buf, TypeReference type)
	{
		List<TypeReference> decls = DocUtils.GetDeclaringTypes (type);
		bool insertNested = false;
		int prevParamCount = 0;
		foreach (var decl in decls) {
			if (insertNested)
				buf.Append (NestedTypeSeparator);
			insertNested = true;
			base.AppendTypeName (buf, decl);
			int argCount = DocUtils.GetGenericArgumentCount (decl);
			int numArgs = argCount - prevParamCount;
			prevParamCount = argCount;
			if (numArgs > 0)
				buf.Append ('`').Append (numArgs);
		}
		return buf;
	}

	public override string GetDeclaration (MemberReference member)
	{
		TypeReference r = member as TypeReference;
		if (r != null) {
			return "T:" + GetTypeName (r);
		}
		return base.GetDeclaration (member);
	}

	protected override string GetConstructorName (MethodReference constructor)
	{
		return GetMethodDefinitionName (constructor, "#ctor");
	}

	protected override string GetMethodName (MethodReference method)
	{
		string name = null;
		MethodDefinition methodDef = method as MethodDefinition;
		if (methodDef == null || !DocUtils.IsExplicitlyImplemented (methodDef))
			name = method.Name;
		else {
			TypeReference iface;
			MethodReference ifaceMethod;
			DocUtils.GetInfoForExplicitlyImplementedMethod (methodDef, out iface, out ifaceMethod);
			AddTypeCount = false;
			name = GetTypeName (iface) + "." + ifaceMethod.Name;
			AddTypeCount = true;
		}
		return GetMethodDefinitionName (method, name);
	}

	private string GetMethodDefinitionName (MethodReference method, string name)
	{
		StringBuilder buf = new StringBuilder ();
		buf.Append (GetTypeName (method.DeclaringType));
		buf.Append ('.');
		buf.Append (name.Replace (".", "#"));
		if (method.IsGenericMethod ()) {
			IList<GenericParameter> genArgs = method.GenericParameters;
			if (genArgs.Count > 0)
				buf.Append ("``").Append (genArgs.Count);
		}
		IList<ParameterDefinition> parameters = method.Parameters;
		try {
			genDeclType   = method.DeclaringType;
			genDeclMethod = method;
			AppendParameters (buf, method.DeclaringType.GenericParameters, parameters);
		}
		finally {
			genDeclType   = null;
			genDeclMethod = null;
		}
		return buf.ToString ();
	}

	private StringBuilder AppendParameters (StringBuilder buf, IList<GenericParameter> genArgs, IList<ParameterDefinition> parameters)
	{
		if (parameters.Count == 0)
			return buf;

		buf.Append ('(');

		AppendParameter (buf, genArgs, parameters [0]);
		for (int i = 1; i < parameters.Count; ++i) {
			buf.Append (',');
			AppendParameter (buf, genArgs, parameters [i]);
		}

		return buf.Append (')');
	}

	private StringBuilder AppendParameter (StringBuilder buf, IList<GenericParameter> genArgs, ParameterDefinition parameter)
	{
		AddTypeCount = false;
		buf.Append (GetTypeName (parameter.ParameterType));
		AddTypeCount = true;
		return buf;
	}

	protected override string GetPropertyName (PropertyReference property)
	{
		string name = null;

		PropertyDefinition propertyDef = property as PropertyDefinition;
		MethodDefinition method = null;
		if (propertyDef != null)
			method = propertyDef.GetMethod ?? propertyDef.SetMethod;
		if (method != null && !DocUtils.IsExplicitlyImplemented (method))
			name = property.Name;
		else {
			TypeReference iface;
			MethodReference ifaceMethod;
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
		IList<ParameterDefinition> parameters = property.Parameters;
		if (parameters.Count > 0) {
			genDeclType = property.DeclaringType;
			buf.Append ('(');
			IList<GenericParameter> genArgs = property.DeclaringType.GenericParameters;
			AppendParameter (buf, genArgs, parameters [0]);
			for (int i = 1; i < parameters.Count; ++i) {
				 buf.Append (',');
				 AppendParameter (buf, genArgs, parameters [i]);
			}
			buf.Append (')');
			genDeclType = null;
		}
		return buf.ToString ();
	}

	protected override string GetFieldName (FieldReference field)
	{
		return string.Format ("{0}.{1}",
			GetName (field.DeclaringType), field.Name);
	}

	protected override string GetEventName (EventReference e)
	{
		return string.Format ("{0}.{1}",
			GetName (e.DeclaringType), e.Name);
	}

	protected override string GetTypeDeclaration (TypeDefinition type)
	{
		string name = GetName (type);
		if (type == null)
			return null;
		return "T:" + name;
	}

	protected override string GetConstructorDeclaration (MethodDefinition constructor)
	{
		string name = GetName (constructor);
		if (name == null)
			return null;
		return "M:" + name;
	}

	protected override string GetMethodDeclaration (MethodDefinition method)
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

	protected override string GetPropertyDeclaration (PropertyDefinition property)
	{
		string name = GetName (property);
		if (name == null)
			return null;
		return "P:" + name;
	}

	protected override string GetFieldDeclaration (FieldDefinition field)
	{
		string name = GetName (field);
		if (name == null)
			return null;
		return "F:" + name;
	}

	protected override string GetEventDeclaration (EventDefinition e)
	{
		string name = GetName (e);
		if (name == null)
			return null;
		return "E:" + name;
	}
}

class FileNameMemberFormatter : SlashDocMemberFormatter {
	protected override StringBuilder AppendNamespace (StringBuilder buf, TypeReference type)
	{
		return buf;
	}

	protected override char NestedTypeSeparator {
		get {return '+';}
	}
}

}
