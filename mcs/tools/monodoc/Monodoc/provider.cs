//
// Provider: shared code and interfaces for providers
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002, Ximian, Inc.
//
// TODO:
//   Each node should have a provider link
//
//   Should encode numbers using a runlength encoding to save space
//
namespace Monodoc {
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using ICSharpCode.SharpZipLib.Zip;

using Monodoc.Lucene.Net.Index;
using Monodoc.Lucene.Net.Analysis.Standard;

using Mono.Documentation;

/// <summary>
///    This tree is populated by the documentation providers, or populated
///    from a binary encoding of the tree.  The format of the tree is designed
///    to minimize the need to load it in full.
/// </summary>
public class Tree : Node {

#region Loading the tree from a file

	/// <summary>
	///   Our HelpSource container
	/// </summary>
	public readonly HelpSource HelpSource;
	
	internal FileStream InputStream;
	internal BinaryReader InputReader;

	/// <summary>
	///   Load from file constructor
	/// </summary>
	public Tree (HelpSource hs, string filename) : base (null, null)
	{
		Encoding utf8 = new UTF8Encoding (false, true);

		if (!File.Exists (filename)){
			throw new FileNotFoundException ();
		}
		
		InputStream = File.OpenRead (filename);
		InputReader = new BinaryReader (InputStream, utf8);
		byte [] sig = InputReader.ReadBytes (4);
		
		if (!GoodSig (sig))
			throw new Exception ("Invalid file format");
		
		InputStream.Position = 4;
		position = InputReader.ReadInt32 ();

		LoadNode ();
		HelpSource = hs;
	}

	/// <summary>
	///    Tree creation and merged tree constructor
	/// </summary>
	public Tree (HelpSource hs, string caption, string url) : base (caption, url)
	{
		HelpSource = hs;
	}

	public Tree (HelpSource hs, Node parent, string caption, string element) : base (parent, caption, element)
	{
		HelpSource = hs;
	}

#endregion

	/// <summary>
	///    Saves the tree into the specified file using the help file format.
	/// </summary>
	public void Save (string file)
	{
		Encoding utf8 = new UTF8Encoding (false, true);
		using (FileStream output = File.OpenWrite (file)){
			// Skip over the pointer to the first node.
			output.Position = 8;
			
			using (BinaryWriter writer = new BinaryWriter (output, utf8)){
				// Recursively dump
				Dump (output, writer);

				output.Position = 0;
				writer.Write (new byte [] { (byte) 'M', (byte) 'o', (byte) 'H', (byte) 'P' });
				writer.Write (position);
			}
		}
	}

	static bool GoodSig (byte [] sig)
	{
		if (sig.Length != 4)
			return false;
		if (sig [0] != (byte) 'M' ||
		    sig [1] != (byte) 'o' ||
		    sig [2] != (byte) 'H' ||
		    sig [3] != (byte) 'P')
			return false;
		return true;
	}

}

public class Node : IComparable {
	string caption, element;
	public bool Documented;
	public readonly Tree tree;
	Node parent;
	protected ArrayList nodes;
	protected internal int position;

	static ArrayList empty = ArrayList.ReadOnly(new ArrayList(0));

	/// <summary>
	///    Creates a node, called by the Tree.
	/// </summary>
	public Node (string caption, string element)
	{
		this.tree = (Tree) this;
		this.caption = caption;
		this.element = element;
		parent = null;
	}

	public Node (Node parent, string caption, string element)
	{
		this.parent = parent;
		this.tree = parent.tree;
		this.caption = caption;
		this.element = element;
	}
	
	/// <summary>
	///    Creates a node from an on-disk representation
	/// </summary>
	Node (Node parent, int address)
	{
		this.parent = parent;
		position = address;
		this.tree = parent.tree;
		if (address > 0)
			LoadNode ();
	}

	public void AddNode (Node n)
	{
		Nodes.Add (n);
		n.parent = this;
		n.Documented = true;
	}

	public void DelNode (Node n)
	{
		Nodes.Remove (n);
	}

	public ArrayList Nodes {
		get {
			if (position < 0)
				LoadNode ();
			return nodes != null ? nodes : empty;
		}
	}

	public string Element {
		get {
			if (position < 0)
				LoadNode ();
			return element;
		}

		set {
			element = value;
		}
	}

	public string Caption {
		get {
			if (position < 0)
				LoadNode ();
			return caption;
		}
	}
	
	public Node Parent {
		get {
			return parent;
		}
	}
		
	public void LoadNode ()
	{
		if (position < 0)
			position = -position;

		tree.InputStream.Position = position;
		BinaryReader reader = tree.InputReader;
		int count = DecodeInt (reader);
		element = reader.ReadString ();
		caption = reader.ReadString ();
		if (count == 0)
			return;
		
		nodes = new ArrayList (count);
		for (int i = 0; i < count; i++){
			int child_address = DecodeInt (reader);
							      
			Node t = new Node (this, -child_address);
			nodes.Add (t);
		}
	}
	
	/// <summary>
	///   Creates a new node, in the locator entry point, and with
	///   a user visible caption of @caption
	/// </summary>
	public Node CreateNode (string c_caption, string c_element)
	{
		if (nodes == null)
			nodes = new ArrayList ();

		Node t = new Node (this, c_caption, c_element);
		nodes.Add (t);
		return t;
	}

	/// <summary>
	///   Looks up or creates a new node, in the locator entry point, and with
	///   a user visible caption of @caption.  This is different from
	///   CreateNode in that it will look up an existing node for the given @locator.
	/// </summary>
	public Node LookupNode (string c_caption, string c_element)
	{
		if (nodes == null)
			return CreateNode (c_caption, c_element);

		foreach (Node n in nodes){
			if (n.element == c_element)
				return n;
		}
		return CreateNode (c_caption, c_element);
	}

	public void EnsureNodes ()
	{
		if (nodes == null)
			nodes = new ArrayList ();
	}
	
	public bool IsLeaf {
		get {
			return nodes == null;
		}
	}

	void EncodeInt (BinaryWriter writer, int value)
	{
		do {
			int high = (value >> 7) & 0x01ffffff;
			byte b = (byte)(value & 0x7f);

			if (high != 0) {
				b = (byte)(b | 0x80);
			}
			
			writer.Write(b);
			value = high;
		} while(value != 0);
	}

	int DecodeInt (BinaryReader reader)
	{
		int ret = 0;
		int shift = 0;
		byte b;
		
                        do {
                                b = reader.ReadByte();

                                ret = ret | ((b & 0x7f) << shift);
                                shift += 7;
                        } while ((b & 0x80) == 0x80);
			
                        return ret;
	}

	internal void Dump (FileStream output, BinaryWriter writer)
	{
		if (nodes != null){
			foreach (Node child in nodes){
				child.Dump (output, writer);
			}
		}
		position = (int) output.Position;
		EncodeInt (writer, nodes == null ? 0 : (int) nodes.Count);
		writer.Write (element);
		writer.Write (caption);

		if (nodes != null){
			foreach (Node child in nodes){
				EncodeInt (writer, child.position);
			}
		}
	}

	static int indent;

	static void Indent ()
	{
		for (int i = 0; i < indent; i++)
			Console.Write ("   ");
	}
	
	public static void PrintTree (Node node)
	{
		Indent ();
		Console.WriteLine ("{0},{1}\t[PublicUrl: {2}]", node.Element, node.Caption, node.PublicUrl);
		if (node.Nodes.Count == 0)
			return;

		indent++;
		foreach (Node n in node.Nodes)
			PrintTree (n);
		indent--;
	}

	public void Sort ()
	{
		if (nodes != null)
			nodes.Sort ();
	}

	[Obsolete("Use PublicUrl")]
	public string URL {
		get {
			if (position < 0)
				LoadNode ();

			if (element.IndexOf (":") >= 0)
				return element;

			if (parent != null){
				string url = parent.URL;

				if (url.EndsWith ("/"))
					return url + element;
				else
					return parent.URL + "/" + element;
			} else
				return element;
		}
	}

	public string PublicUrl {
		get {
			return tree.HelpSource != null
				? tree.HelpSource.GetPublicUrl (URL)
				: URL;
		}
	}

	int IComparable.CompareTo (object obj)
	{
		Node other = obj as Node;
		if (other == null)
			return -1;

		if (position < 0)
			LoadNode ();
		if (other.position < 0)
			other.LoadNode ();

		Regex digits = new Regex (@"([\d]+)|([^\d]+)");
		MatchEvaluator eval = delegate (Match m) {
			return (m.Value.Length > 0 && char.IsDigit (m.Value [0])) 
				? m.Value.PadLeft (System.Math.Max (caption.Length, other.caption.Length)) 
				: m.Value;
		};
		return digits.Replace (caption, eval).CompareTo (digits.Replace (other.caption, eval));
	}
}

//
// The HelpSource class keeps track of the archived data, and its
// tree
//
public class HelpSource {
	static int id;
	public static bool use_css = false;
	public static string css_code;
	public static string CssCode {
		get {
			if (css_code != null)
				return css_code;

			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
			Stream str_css = assembly.GetManifestResourceStream ("base.css");
			StringBuilder sb = new StringBuilder ((new StreamReader (str_css)).ReadToEnd());
			sb.Replace ("@@FONT_FAMILY@@", SettingsHandler.Settings.preferred_font_family);
			sb.Replace ("@@FONT_SIZE@@", SettingsHandler.Settings.preferred_font_size.ToString());
			css_code = sb.ToString ();
			return css_code;
		}
		set { css_code = value; }
	}

	public virtual string InlineCss {
		get { return CssCode; }
	}

	public virtual string InlineJavaScript {
		get { return null; }
	}

	public static bool FullHtml = true;

	// should only be enabled by ASP.NET webdoc
	public static bool UseWebdocCache;

	//
	// The unique ID for this HelpSource.
	//
	int source_id;
	DateTime zipFileWriteTime;
	string name;
	TraceLevel trace_level = TraceLevel.Warning;
	protected bool nozip;
	protected string base_dir;

	public HelpSource (string base_filename, bool create)
	{
		this.name = Path.GetFileName (base_filename);
		tree_filename = base_filename + ".tree";
		zip_filename = base_filename + ".zip";
		base_dir = XmlDocUtils.GetCacheDirectory (base_filename);
		if (UseWebdocCache && !create && Directory.Exists (base_dir)) {
			nozip = true;
		}

		if (create)
			SetupForOutput ();
		else 
			Tree = new Tree (this, tree_filename);

		source_id = id++;
		try {
			FileInfo fi = new FileInfo (zip_filename);
			zipFileWriteTime = fi.LastWriteTime;
		} catch {
			zipFileWriteTime = DateTime.Now;
		}
	}
	
	public HelpSource() {
		Tree = new Tree (this, "Blah", "Blah");
		source_id = id++;
	}

	public DateTime ZipFileWriteTime {
		get {
			return zipFileWriteTime;
		}
	}
	
	public int SourceID {
		get {
			return source_id;
		}
	}
	
	public string Name {
		get {
			return name;
		}
	}

	public TraceLevel TraceLevel {
		get { return trace_level; }
		set { trace_level = value; }
	}
	
	ZipFile zip_file;
	
	/// <summary>
	///   Returns a stream from the packaged help source archive
	/// </summary>
	public virtual Stream GetHelpStream (string id)
	{
		if (nozip) {
			string path = XmlDocUtils.GetCachedFileName (base_dir, id);
			if (File.Exists (path))
				return File.OpenRead (path);
			return null;
		}

		if (zip_file == null)
			zip_file = new ZipFile (zip_filename);

		ZipEntry entry = zip_file.GetEntry (id);
		if (entry != null)
			return zip_file.GetInputStream (entry);
		return null;
	}
	
	public string GetRealPath (string file)
	{
		if (zip_file == null)
			zip_file = new ZipFile (zip_filename);

		ZipEntry entry = zip_file.GetEntry (file);
		if (entry != null && entry.ExtraData != null)
			return ConvertToString (entry.ExtraData);
		return null;
	}
	
	public XmlReader GetHelpXml (string id)
	{
		if (nozip) {
			Stream s = File.OpenRead (XmlDocUtils.GetCachedFileName (base_dir, id));
			string url = "monodoc:///" + SourceID + "@" + System.Web.HttpUtility.UrlEncode (id) + "@";
			return new XmlTextReader (url, s);
		}

		if (zip_file == null)
			zip_file = new ZipFile (zip_filename);

		ZipEntry entry = zip_file.GetEntry (id);
		if (entry != null) {
			Stream s = zip_file.GetInputStream (entry);
			string url = "monodoc:///" + SourceID + "@" + System.Web.HttpUtility.UrlEncode (id) + "@";
			return new XmlTextReader (url, s);
		}
		return null;
	}
	
	public virtual XmlDocument GetHelpXmlWithChanges (string id)
	{
		if (nozip) {
			Stream s = File.OpenRead (XmlDocUtils.GetCachedFileName (base_dir, id));
			string url = "monodoc:///" + SourceID + "@" + System.Web.HttpUtility.UrlEncode (id) + "@";
			XmlReader r = new XmlTextReader (url, s);
			XmlDocument ret = new XmlDocument ();
			ret.Load (r);
			return ret;
		}

		if (zip_file == null)
			zip_file = new ZipFile (zip_filename);

		ZipEntry entry = zip_file.GetEntry (id);
		if (entry != null) {
			Stream s = zip_file.GetInputStream (entry);
			string url = "monodoc:///" + SourceID + "@" + System.Web.HttpUtility.UrlEncode (id) + "@";
			XmlReader r = new XmlTextReader (url, s);
			XmlDocument ret = new XmlDocument ();
			ret.Load (r);
			
			if (entry.ExtraData != null)
				EditingUtils.AccountForChanges (ret, Name, ConvertToString (entry.ExtraData));
			
			return ret;
		}
		return null;	
	}
	
	/// <summary>
	///   Get a nice, unique expression for any XPath node that you get.
	///   This function is used by editing to get the expression to put
	///   on to the file. The idea is to create an expression that is resistant
	///   to changes in the structure of the XML.
	/// </summary>
	public virtual string GetNodeXPath (XPathNavigator n)
	{
		return EditingUtils.GetXPath (n.Clone ());
	}
	
	public string GetEditUri (XPathNavigator n)
	{
		return EditingUtils.FormatEditUri (n.BaseURI, GetNodeXPath (n));
	}
	
	static string ConvertToString (byte[] data)
	{
		return Encoding.UTF8.GetString(data);
	}
	
	static byte[] ConvertToArray (string str)
	{
		return Encoding.UTF8.GetBytes(str);
	}

	/// <summary>
	///   The tree that is being populated
	/// </summary>
	public Tree Tree;
	public RootTree RootTree;

	// Base filename used by this HelpSource.
	string tree_filename, zip_filename;

	// Used for ziping. 
	const int buffer_size = 65536;
	ZipOutputStream zip_output;
	byte [] buffer;
	
	HelpSource (string base_filename)
	{
	}
		
	void SetupForOutput ()
	{
		Tree = new Tree (this, "", "");

		FileStream stream = File.Create (zip_filename);
		
		zip_output = new ZipOutputStream (stream);
		zip_output.SetLevel (9);

		buffer = new byte [buffer_size];
	}		

	/// <summary>
	///   Saves the tree and the archive
	/// </summary>
	public void Save ()
	{
		Tree.Save (tree_filename);
		zip_output.Finish ();
		zip_output.Close ();
	}

	int code;

	string GetNewCode ()
	{
		return String.Format ("{0}", code++);
	}

	/// <summary>
	///   Providers call this to store a file they will need, and the return value
	///   is the name that was assigned to it
	/// </summary>
	public string PackFile (string file)
	{
		string entry_name = GetNewCode ();
		return PackFile (file, entry_name);
	}

	public string PackFile (string file, string entry_name)
	{
		using (FileStream input = File.OpenRead (file)) {
			PackStream (input, entry_name, file);
		}

		return entry_name;
	}
	
	public void PackStream (Stream s, string entry_name)
	{
		PackStream (s, entry_name, null);
	}
	
	void PackStream (Stream s, string entry_name, string realPath)
	{
		ZipEntry entry = new ZipEntry (entry_name);
				
		if (realPath != null)
			entry.ExtraData = ConvertToArray (realPath);
		
		zip_output.PutNextEntry (entry);
		int n;
			
		while ((n = s.Read (buffer, 0, buffer_size)) > 0){
			zip_output.Write (buffer, 0, n);
		}	
	}
	
	public void PackXml (string fname, XmlDocument doc, string real_path)
	{ 
		ZipEntry entry = new ZipEntry (fname); 
		if (real_path != null) 
			entry.ExtraData = ConvertToArray(real_path);

		zip_output.PutNextEntry (entry);
		XmlTextWriter xmlWriter = new XmlTextWriter (zip_output, Encoding.UTF8);
		doc.WriteContentTo (xmlWriter);
		xmlWriter.Flush ();
	}
	
	public virtual void RenderPreviewDocs (XmlNode newNode, XmlWriter writer)
	{
		throw new NotImplementedException ();
	}

	public virtual string GetPublicUrl (string id)
	{
		return id;
	}
	
	public virtual string GetText (string url, out Node n)
	{
		n = null;
		return null;
	}

	protected string GetCachedText (string url)
	{
		if (!nozip)
			return null;
		string file = XmlDocUtils.GetCachedFileName (base_dir, url);
		if (!File.Exists (file))
			return null;
		return File.OpenText (file).ReadToEnd ();
	}

	public virtual Stream GetImage (string url)
	{
		return null;
	}
	
	//
	// Default method implementation does not satisfy the request
	//
	public virtual string RenderTypeLookup (string prefix, string ns, string type, string member, out Node n)
	{
		n = null;
		return null;
	}

	public virtual string RenderNamespaceLookup (string nsurl, out Node n)
	{
		n = null;
		return null;
	}

	//
	// Populates the index.
	//
	public virtual void PopulateIndex (IndexMaker index_maker)
	{
	}
	
	//
	// Build an html document
	//
	public static string BuildHtml (string css, string html_code)
	{
		return BuildHtml (css, null, html_code);
	}

	internal static string BuildHtml (string css, string js, string html_code) {
		if (!FullHtml) {
			return html_code;
		}
		StringWriter output = new StringWriter ();
		output.Write ("<html><head>");
		output.Write ("<style type=\"text/css\">");
		output.Write (CssCode);
		output.Write (css);
		output.Write ("</style>");

		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
		Stream str_js = assembly.GetManifestResourceStream ("helper.js");
		StringBuilder sb = new StringBuilder ((new StreamReader (str_js)).ReadToEnd());
		output.Write ("<script type=\"text/JavaScript\">\n");
		output.Write (sb.ToString ());
		output.Write ("</script>\n");
		
		if (js != null) {
			output.Write ("<script type=\"text/JavaScript\">\n");
			output.Write (js);
			output.Write ("\n</script>");
		}

		output.Write ("</head><body>");
		output.Write (html_code);
		output.Write ("</body></html>");
		return output.ToString ();
	}

	//
	// Create different Documents for adding to Lucene search index
	// The default action is do nothing. Subclasses should add the docs
	// 
	public virtual void PopulateSearchableIndex (IndexWriter writer) {
		return;
	}

	public void Message (TraceLevel level, string format, params object[] args)
	{
		if ((int) level <= (int) trace_level)
			Console.WriteLine (format, args);
	}

	public void Error (string format, params object[] args)
	{
		Console.Error.WriteLine (format, args);
	}
}

public abstract class Provider {
	//
	// This code is used to "tag" all the different sources
	//
	static short serial;

	public int code;
	
	public Provider ()
	{
		code = serial++;
	}

	public abstract void PopulateTree (Tree tree);

	//
	// Called at shutdown time after the tree has been populated to perform
	// any fixups or final tasks.
	//
	public abstract void CloseTree (HelpSource hs, Tree tree);
}

public class RootTree : Tree {
	string basedir;
	
	public static ArrayList UncompiledHelpSources = new ArrayList();
	
	public const int MonodocVersion = 1;
	
	public static RootTree LoadTree ()
	{
		return LoadTree (null);
	}
	
	//
	// Loads the tree layout
	//
	public static RootTree LoadTree (string basedir)
	{
		if (basedir == null) {
			string myPath = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			string cfgFile = myPath + ".config";
			if (!File.Exists (cfgFile)) {
				basedir = ".";
			}
			else {
				XmlDocument d = new XmlDocument ();
				d.Load (cfgFile);
				basedir = d.SelectSingleNode ("config/path").Attributes ["docsPath"].Value;
			}
			//basedir = "/Library/Frameworks/Mono.framework/Versions/Current/lib/monodoc/";
		}

		//
		// Load the layout
		//
		XmlDocument doc = new XmlDocument ();
		string layout = Path.Combine (basedir, "monodoc.xml");
		doc.Load (layout);

		string osxExternalDir = "/Library/Frameworks/Mono.framework/External/monodoc";
		string[] osxExternalSources = Directory.Exists (osxExternalDir)
			? Directory.GetFiles (osxExternalDir, "*.source")
			: new string[0];

		return LoadTree (basedir, doc, 
				Directory.GetFiles (Path.Combine (basedir, "sources"), "*.source")
				.Concat (osxExternalSources));
	}


	// Compatibility shim w/ Mono 2.6
	public static RootTree LoadTree (string indexDir, XmlDocument docTree, IEnumerable sourceFiles)
	{
		return LoadTree (indexDir, docTree, sourceFiles.Cast<string>());
	}

	public static RootTree LoadTree (string indexDir, XmlDocument docTree, IEnumerable<string> sourceFiles)
	{
		if (docTree == null) {
			docTree = new XmlDocument ();
			using (var defTree = typeof(RootTree).Assembly.GetManifestResourceStream ("monodoc.xml"))
				docTree.Load (defTree);
		}

		sourceFiles = sourceFiles ?? new string [0];

		//
		// Load the layout
		//

		RootTree root = new RootTree ();
		root.basedir = indexDir;

		XmlNodeList nodes = docTree.SelectNodes ("/node/node");

		root.name_to_node ["root"] = root;
		root.name_to_node ["libraries"] = root;
		root.Populate (root, nodes);

		Node third_party = root.LookupEntryPoint ("various");
		if (third_party == null) {
			Console.Error.WriteLine ("No 'various' doc node! Check monodoc.xml!");
			third_party = root;
		}

		//
		// Load the sources
		//
		foreach (var sourceFile in sourceFiles)
			root.AddSourceFile (sourceFile);
		
		foreach (string path in UncompiledHelpSources) {
			EcmaUncompiledHelpSource hs = new EcmaUncompiledHelpSource(path);
			hs.RootTree = root;
			root.help_sources.Add (hs);
			string epath = "extra-help-source-" + hs.Name;
			Node hsn = root.CreateNode (hs.Name, "root:/" + epath);
			root.name_to_hs [epath] = hs;
			hsn.EnsureNodes ();
			foreach (Node n in hs.Tree.Nodes){
				hsn.AddNode (n);
			}
		}
		
		// Clean the tree
		PurgeNode(root);

		root.Sort ();

		return root;
	}

	public void AddSource (string sources_dir)
	{
		string [] files = Directory.GetFiles (sources_dir);

		foreach (string file in files){
			if (!file.EndsWith (".source"))
				continue;
			AddSourceFile (file);
		}
	}

	public void AddSourceFile (string sourceFile)
	{
		Node third_party = LookupEntryPoint ("various") ?? this;

		XmlDocument doc = new XmlDocument ();
		try {
			doc.Load (sourceFile);
		}
		catch {
			Console.Error.WriteLine ("Error: Could not load source file {0}", sourceFile);
			return;
		}

		XmlNodeList extra_nodes = doc.SelectNodes ("/monodoc/node");
		if (extra_nodes.Count > 0)
			Populate (third_party, extra_nodes);

		XmlNodeList sources = doc.SelectNodes ("/monodoc/source");
		if (sources == null){
			Console.Error.WriteLine ("Error: No <source> section found in the {0} file", sourceFile);
			return;
		}
		foreach (XmlNode source in sources){
			XmlAttribute a = source.Attributes ["provider"];
			if (a == null){
				Console.Error.WriteLine ("Error: no provider in <source>");
				continue;
			}
			string provider = a.InnerText;
			a = source.Attributes ["basefile"];
			if (a == null){
				Console.Error.WriteLine ("Error: no basefile in <source>");
				continue;
			}
			string basefile = a.InnerText;
			a = source.Attributes ["path"];
			if (a == null){
				Console.Error.WriteLine ("Error: no path in <source>");
				continue;
			}
			string path = a.InnerText;

			string basefilepath = Path.Combine (Path.GetDirectoryName (sourceFile), basefile);
			HelpSource hs = GetHelpSource (provider, basefilepath);
			if (hs == null)
				continue;
			hs.RootTree = this;
			help_sources.Add (hs);
			name_to_hs [path] = hs;

			Node parent = LookupEntryPoint (path);
			if (parent == null){
				Console.Error.WriteLine ("node `{0}' is not defined on the documentation map", path);
				parent = third_party;
			}

			foreach (Node n in hs.Tree.Nodes){
				parent.AddNode (n);
			}
			parent.Sort ();
		}
	}
	
	// Delete nodes which does not have documentaiton (source)
	static bool PurgeNode(Node node)
	{
		bool purge = false;
		
		if (!node.Documented)
		{
			ArrayList del_child = new ArrayList();
			//Delete node unless any child has documentation
			bool purged_child = false;
			foreach (Node child in node.Nodes)
			{
				purged_child = PurgeNode(child);
				if (purged_child) 
				{
					del_child.Add(child);
				}
			}
				
			// delete the node if all its children are to be deleted
			purge = (node.Nodes.Count == del_child.Count); 
				
			// delete children
			foreach (Node child in del_child)
			{
				node.DelNode(child);
			}
		}
		
		return purge;
	}

	public static string[] GetSupportedFormats ()
	{
		return new string[]{
			"ecma", 
			"ecmaspec", 
			"error", 
			"hb", 
			"man", 
			"simple", 
			"xhtml"
		};
	}
	
	public static HelpSource GetHelpSource (string provider, string basefilepath)
	{
		try {
			switch (provider){
			case "ecma":
				return new EcmaHelpSource (basefilepath, false);
			case "ecma-uncompiled":
				return new EcmaUncompiledHelpSource (basefilepath);
			case "monohb":
				return new MonoHBHelpSource(basefilepath, false);
			case "xhtml": case "hb":
				return new XhtmlHelpSource (basefilepath, false);
			case "man":
				return new ManHelpSource (basefilepath, false);
			case "simple":
				return new SimpleHelpSource (basefilepath, false);
			case "error":
				return new ErrorHelpSource (basefilepath, false);
			case "ecmaspec":
				return new EcmaSpecHelpSource (basefilepath, false);
			case "addins":
				return new AddinsHelpSource (basefilepath, false);
			default:
				Console.Error.WriteLine ("Error: Unknown provider specified: {0}", provider);
				break;
			}
			return null;
		}
		catch (FileNotFoundException) {
			Console.Error.WriteLine ("Error: did not find one of the files in sources/"+basefilepath);
			return null;
		}
	}

	public static Provider GetProvider (string provider, params string[] basefilepaths)
	{
		switch (provider) {
		case "addins":
			return new AddinsProvider (basefilepaths [0]);
		case "ecma": {
			EcmaProvider p = new EcmaProvider ();
			foreach (string d in basefilepaths)
				p.AddDirectory (d);
			return p;
		}
		case "ecmaspec":
			return new EcmaSpecProvider (basefilepaths [0]);
		case "error":
			return new ErrorProvider (basefilepaths [0]);
		case "man":
			return new ManProvider (basefilepaths);
		case "simple":
			return new SimpleProvider (basefilepaths [0]);
		case "xhtml":
		case "hb":
			return new XhtmlProvider (basefilepaths [0]);
		default:
			throw new NotSupportedException (provider);
		}
	}

	//
	// Maintains the name to node mapping
	//
	Hashtable name_to_node = new Hashtable ();
	Hashtable name_to_hs = new Hashtable ();
	
	void Populate (Node parent, XmlNodeList xml_node_list)
	{
		foreach (XmlNode xml_node in xml_node_list){
			XmlAttribute e = xml_node.Attributes ["parent"];
			if (e != null && name_to_node.ContainsKey (e.InnerText)) {
				Node p = (Node) name_to_node [e.InnerText];
				xml_node.Attributes.Remove (e);
				Populate (p, xml_node.SelectNodes ("."));
				continue;
			}
			e = xml_node.Attributes ["label"];
			if (e == null){
				Console.Error.WriteLine ("`label' attribute missing in <node>");
				continue;
			}
			string label = e.InnerText;
			e = xml_node.Attributes ["name"];
			if (e == null){
				Console.Error.WriteLine ("`name' attribute missing in <node>");
				continue;
			}
			string name = e.InnerText;

			Node n = parent.LookupNode (label, "root:/" + name);
			n.EnsureNodes ();
			name_to_node [name] = n;
			XmlNodeList children = xml_node.SelectNodes ("./node");
			if (children != null)
				Populate (n, children);
		}
	}

	public Node LookupEntryPoint (string name)
	{
		return (Node) name_to_node [name];
	}
	
	ArrayList help_sources;
	DateTime lastHelpSourceTime;
	
	RootTree () : base (null, "Mono Documentation", "root:")
	{
		nodes = new ArrayList ();
		help_sources = new ArrayList ();
		lastHelpSourceTime = DateTime.MinValue;
	}

	public DateTime LastHelpSourceTime {
		get {
			return lastHelpSourceTime;
		}
	}
	
	public static bool GetNamespaceAndType (string url, out string ns, out string type)
	{
		int nsidx = -1;
		int numLt = 0;
		for (int i = 0; i < url.Length; ++i) {
			char c = url [i];
			switch (c) {
			case '<':
			case '{':
				++numLt;
				break;
			case '>':
			case '}':
				--numLt;
				break;
			case '.':
				if (numLt == 0)
					nsidx = i;
				break;
			}
		}

		if (nsidx == -1) {
			Console.Error.WriteLine ("Did not find dot in: " + url);
			ns = null;
			type = null;
			return false;
		}
		ns = url.Substring (0, nsidx);
		type = url.Substring (nsidx + 1);
		
		//Console.Error.WriteLine ("GetNameSpaceAndType (ns={0}, type={1}", ns, type);
		return true;
	}

	public XmlDocument GetHelpXml (string url)
	{
		string rest = url.Substring (2);
		string ns, type;

		if (!GetNamespaceAndType (rest, out ns, out type))
			return null;

		foreach (HelpSource hs in help_sources) {
			EcmaHelpSource ehs = hs as EcmaHelpSource;
			if (ehs == null)
				continue;
			string id = ehs.GetIdFromUrl ("T:", ns, type);
			if (id == null)
				continue;
			XmlDocument doc = hs.GetHelpXmlWithChanges (id);
			if (doc != null)
				return doc;
		}
		return null;
	}
	
	public string TypeLookup (string url, out Node match_node)
	{
		string rest = Regex.Replace (url, @"^T:\s*", "");
		string ns, type;

		if (!GetNamespaceAndType (rest, out ns, out type)){
			match_node = null;
			return null;
		}
		
		foreach (HelpSource hs in help_sources){
			string s = hs.RenderTypeLookup ("T:", ns, type, null, out match_node);
			
			if (s != null) {
				lastHelpSourceTime = hs.ZipFileWriteTime;
				return s;
			}
		}
		match_node = null;
		return null;
	}

	public string MemberLookup (string prefix, string url, out Node match_node)
	{
		string rest = Regex.Replace (url, @"^.:\s*", "");
		
		// Dots in the arg list (for methods) confuse this.
		// Chop off the arg list for now and put it back later.
		string arglist = "";
		int argliststart = rest.IndexOf("(");
		if (argliststart >= 0) {
			arglist = rest.Substring(argliststart);
			rest = rest.Substring(0, argliststart);
		}

		string ns_type, member;
	
		if (prefix != "C:") {
			int member_idx = rest.LastIndexOf (".");
	
			// The dot in .ctor (if it's a M: link) would confuse this.
			if (rest.EndsWith("..ctor")) member_idx--;
	
			ns_type = rest.Substring (0, member_idx);
			member = rest.Substring (member_idx + 1);
		} else {
			// C: links don't have the .ctor member part as it would in a M: link
			// Even though externally C: links are different from M: links,
			// C: links get transformed into M:-style links (with .ctor) here.
			ns_type = rest;
			member = ".ctor";
		}
 

		//Console.WriteLine ("NS_TYPE: {0}  MEMBER: {1}", ns_type, member);

		string ns, type;
		if (!GetNamespaceAndType (ns_type, out ns, out type)){
			match_node = null;
			return null;
		}
		
		foreach (HelpSource hs in help_sources){
			string s = hs.RenderTypeLookup (prefix, ns, type, member + arglist, out match_node);
			
			if (s != null) {
				lastHelpSourceTime = hs.ZipFileWriteTime;
				return s;
			}
		}
		match_node = null;
		return null;
	}

	public Stream GetImage (string url)
	{
		if (url.StartsWith ("source-id:")){
			string rest = url.Substring (10);
			int p = rest.IndexOf (":");
			string str_idx = rest.Substring (0, p);
			int idx = 0;

			try {
				idx = Int32.Parse (str_idx);
			} catch {
				Console.Error.WriteLine ("Failed to parse source-id url: {0} `{1}'", url, str_idx);
				return null;
			}

			HelpSource hs = GetHelpSourceFromId (idx);
			lastHelpSourceTime = hs.ZipFileWriteTime;
			return hs.GetImage (rest.Substring (p + 1));
		} else {
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly (typeof(RootTree));			
			return assembly.GetManifestResourceStream (url);
		}
		lastHelpSourceTime = DateTime.MinValue;
		return null;
	}
	
	public HelpSource GetHelpSourceFromId (int id)
	{
		return (HelpSource) help_sources [id];
	}
	
	//
	// Fetches the node title
	//
	public string GetTitle (string url)
	{
		Node match_node;

		if (url == null || url.StartsWith ("root:"))
			return "Mono Documentation";
		
		if (url.Length > 2 && url [1] == ':'){
			switch (url [0]){
			case 'N':
				return url.Substring (2) + " Namespace";

			case 'T':
				string s = TypeLookup (url, out match_node);
				if (match_node != null)
					return match_node.Caption;
				return url.Substring (2) + " type";

		case 'M':
		case 'F':
		case 'P':
		case 'E':
		case 'C':
		case 'O':
			MemberLookup (url.Substring (0,2), url, out match_node);
			if (match_node != null)
				return match_node.Caption;
			break;
			}
		}
		
		return "Mono Documentation";
	}
	
	string home_cache;
	/// <summary>
	///    Allows every HelpSource to try to provide the content for this
	///    URL.
	/// </summary>
	public string RenderUrl (string url, out Node match_node)
	{
		lastHelpSourceTime = DateTime.MinValue;
		if (url == "root:") {
			match_node = this;

			// look whether there are contribs
			GlobalChangeset chgs = EditingUtils.changes;
			StringBuilder con = new StringBuilder ();
			
			//add links to the contrib
			int oldContrib = 0, contribs = 0;
			con.Append ("<ul>");
			foreach (DocSetChangeset dscs in chgs.DocSetChangesets) 
				foreach (FileChangeset fcs in dscs.FileChangesets) 
					foreach (Change c in fcs.Changes) {
						if (c.NodeUrl == null) {
							if (c.Serial == SettingsHandler.Settings.SerialNumber)
								oldContrib++;
						} else if (c.Serial == SettingsHandler.Settings.SerialNumber) {
							contribs++;
							con.Append (String.Format ("<li><a href=\"{0}\">{0}</a></li>", c.NodeUrl));
						}
					}
			
			string contrib = (oldContrib + contribs) == 1?"There is {0} contribution":"There are {0} contributions";
			con.Insert (0, String.Format (contrib, oldContrib + contribs) + " pending upload <i>(Contributing--&gt; Upload)</i>", 1);
			con.Append ("</ul>");
			if (oldContrib == 1)
				con.Append ("<i>You have 1 contribution that is not listed below that will be sent the next time you upload contributions. Only contributions made from now on will be listed.</i>");
			else if (oldContrib > 1)
				con.Append ("<i>You have " + oldContrib + "contributions that are not listed below and will be sent the next time you upload contributions. Only contributions made from now on will be listed.</i>");

			//start the rendering
			if (!HelpSource.use_css) {
				StringBuilder sb = new StringBuilder ("<table bgcolor=\"#b0c4de\" width=\"100%\" cellpadding=\"5\"><tr><td><h3>Mono Documentation Library</h3></td></tr></table>");
			
				foreach (Node n in Nodes)
					sb.AppendFormat ("<a href='{0}'>{1}</a><br/>", n.Element, n.Caption);
			
				//contributions
				sb.Append ("<br><table bgcolor=\"#fff3f3\" width=\"100%\" cellpadding=\"5\"><tr><td>");
				sb.Append ("<h5>Contributions</h5><br>");
				if ((oldContrib + contribs) == 0) {
 					sb.Append ("<p><b>You have not made any contributions yet.</b></p>");
 					sb.Append ("<p>The Documentation of the libraries is not complete and your contributions would be greatly appreciated. The procedure is easy, browse to the part of the documentation you want to contribute to and click on the <font color=\"blue\">[Edit]</font> link to start writing the documentation.</p>");
 					sb.Append ("<p>When you are happy with your changes, use the Contributing--&gt; Upload Contributions menu to send your contributions to our server.</p></div>");
				} else {
					sb.Append (con.ToString ());
				}
				sb.Append ("</td></tr></table>");
				return sb.ToString ();	
			} else {
				if (home_cache == null) {
					System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly (typeof (HelpSource));
					Stream hp_stream = assembly.GetManifestResourceStream ("home.html");
					home_cache = (new StreamReader (hp_stream)).ReadToEnd ();
				}
				StringBuilder sb = new StringBuilder (home_cache);
				// adjust fonts
				sb.Replace ("@@FONT_FAMILY@@", SettingsHandler.Settings.preferred_font_family);
				sb.Replace ("@@FONT_SIZE@@", SettingsHandler.Settings.preferred_font_size.ToString());
				//contributions
				var visible = SettingsHandler.Settings.EnableEditing ? "block;" : "none;";
				if ((oldContrib + contribs) == 0) {
					sb.Replace ("@@CONTRIB_DISP@@", "display: none;");
                                        sb.Replace ("@@NO_CONTRIB_DISP@@", "display: " + visible);
				} else {
					sb.Replace ("@@CONTRIB_DISP@@", "display: " + visible);
					sb.Replace ("@@NO_CONTRIB_DISP@@", "display: none;");
					sb.Replace ("@@CONTRIBS@@", con.ToString ());
				}
				sb.Replace ("@@EDITING_ENABLED@@", "display: " + visible);
					
				// load the url of nodes
				String add_str;
				StringBuilder urls = new StringBuilder ();
				foreach (Node n in Nodes) {
					add_str = String.Format ("<li><a href=\"{0}\">{1}</a></li>", n.Element, n.Caption);
					urls.Append (add_str);
				}
				sb.Replace ("@@API_DOCS@@", urls.ToString ());
						
				return sb.ToString ();
			}
		} 
		
		if (url.StartsWith ("root:")) {
			match_node = ((Node)name_to_node [url.Substring (6)]);
			HelpSource hs = ((HelpSource)name_to_hs [url.Substring (6)]);
			if (hs == null) 
			{
				return GenerateNodeIndex(match_node);
			}
				
			Node dummy;
			lastHelpSourceTime = hs.ZipFileWriteTime;
			return hs.GetText ("root:", out dummy);
		}
	
		
		if (url.StartsWith ("source-id:")){
			string rest = url.Substring (10);
			int p = rest.IndexOf (":");
			string str_idx = rest.Substring (0, p);
			int idx = 0;

			try {
				idx = Int32.Parse (str_idx);
			} catch {
				Console.Error.WriteLine ("Failed to parse source-id url: {0} `{1}'", url, str_idx);
				match_node = null;
				return null;
			}
			HelpSource hs = (HelpSource) help_sources [idx];
			// Console.WriteLine ("Attempting to get docs from: " + rest.Substring (p + 1));
			lastHelpSourceTime = hs.ZipFileWriteTime;
			return hs.GetText (rest.Substring (p + 1), out match_node);
		}

		if (url.Length < 2){
			match_node = null;
			return null;
		}
		
		string prefix = url.Substring (0, 2);
		
		switch (prefix.ToUpper ()){
		case "N:":
			foreach (HelpSource hs in help_sources){
				string s = hs.RenderNamespaceLookup (url, out match_node);
				if (s != null) {
					lastHelpSourceTime = hs.ZipFileWriteTime;
					return s;
				}
			}
			match_node = null;
			return null;

		case "T:":
			return TypeLookup (url, out match_node);

		case "M:":
		case "F:":
		case "P:":
		case "E:":
		case "C:":
		case "O:":
			return MemberLookup (prefix, url, out match_node);
		
		default:
			foreach (HelpSource hs in help_sources){
				string s = hs.GetText (url, out match_node);
				
				if (s != null) {
					lastHelpSourceTime = hs.ZipFileWriteTime;
					return s;
				}
			}
			match_node = null;
			return null;
		}
	}
	
	public string GenerateNodeIndex (Node node)
	{
		StringBuilder buf = new StringBuilder();
		buf.AppendFormat("<H3>{0}</H3>", node.Caption);
		buf.Append("<ul>");
		foreach (Node child in node.Nodes)
		{
			buf.AppendFormat("<li><a href=\"{0}\">{1}</a>", child.URL, child.Caption);
		}
		buf.Append("</ul>");
		return buf.ToString();
	}
	
	public IndexReader GetIndex ()
	{
		//try to load from basedir
		string index_file = Path.Combine (basedir, "monodoc.index");
		if (File.Exists (index_file))
			return IndexReader.Load (index_file);
		//then, try to load from config dir
		index_file = Path.Combine (SettingsHandler.Path, "monodoc.index");
		return IndexReader.Load (index_file);
		
	}

	public static void MakeIndex ()
	{
		RootTree root = LoadTree ();
		if (root == null)
			return;

		IndexMaker index_maker = new IndexMaker ();
		
		foreach (HelpSource hs in root.help_sources){
			hs.PopulateIndex (index_maker);
		}

		// if the user has no write permissions use config dir
		string path = Path.Combine (root.basedir, "monodoc.index");
		try {
			index_maker.Save (path);
		} catch (System.UnauthorizedAccessException) {
			path = Path.Combine (SettingsHandler.Path, "monodoc.index");
			try {
				index_maker.Save (path);
			} catch (System.UnauthorizedAccessException) {
				Console.WriteLine ("Unable to write index file in {0}", Path.Combine (SettingsHandler.Path, "monodoc.index")); 
				return;
			}
		}

		if (IsUnix){
			// No octal in C#, how lame is that
			chmod (path, 0x1a4);
		}
		Console.WriteLine ("Documentation index updated");
	}

	static bool IsUnix {
		get {
			int p = (int) Environment.OSVersion.Platform;
			return ((p == 4) || (p == 128) || (p == 6));
                }
        }

	// Search Index
	public SearchableIndex GetSearchIndex ()
	{
		//try to load from basedir
		string index_file = Path.Combine (basedir, "search_index");
		if (Directory.Exists (index_file))
			return SearchableIndex.Load (index_file);
		//then, try to load from config dir
		index_file = Path.Combine (SettingsHandler.Path, "search_index");
		return SearchableIndex.Load (index_file);
	}

	public static void MakeSearchIndex ()
	{
		// Loads the RootTree
		Console.WriteLine ("Loading the monodoc tree...");
		RootTree root = LoadTree ();
		if (root == null)
			return;

		string dir = Path.Combine (root.basedir, "search_index");
		IndexWriter writer;
		//try to create the dir to store the index
		try {
			if (!Directory.Exists (dir)) 
				Directory.CreateDirectory (dir);

			writer = new IndexWriter(Lucene.Net.Store.FSDirectory.GetDirectory(dir, true), new StandardAnalyzer(), true);
		} catch (UnauthorizedAccessException) {
			//try in the .config directory
			try {
				dir = Path.Combine (SettingsHandler.Path, "search_index");
				if (!Directory.Exists (dir)) 
					Directory.CreateDirectory (dir);

				writer = new IndexWriter(Lucene.Net.Store.FSDirectory.GetDirectory(dir, true), new StandardAnalyzer(), true);
			} catch (UnauthorizedAccessException) {
				Console.WriteLine ("You don't have permissions to write on " + dir);
				return;
			}
		}

		//Collect all the documents
		Console.WriteLine ("Collecting and adding documents...");
		foreach (HelpSource hs in root.HelpSources) 
			hs.PopulateSearchableIndex (writer);
	
		//Optimize and close
		Console.WriteLine ("Closing...");
		writer.Optimize();
		writer.Close();
	}


	public ICollection HelpSources { get { return new ArrayList(help_sources); } }

	[System.Runtime.InteropServices.DllImport ("libc")]
	static extern int chmod (string filename, int mode);
}
}
