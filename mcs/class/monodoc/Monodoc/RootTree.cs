using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

using Monodoc.Providers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;

namespace Monodoc
{
	public
#if LEGACY_MODE
	partial
#endif
	class RootTree : Tree
	{
		public const int MonodocVersion = 2;
		const string RootNamespace = "root:/";
		string basedir;
		static List<string> uncompiledHelpSourcePaths = new List<string>();
		HashSet<string> loadedSourceFiles = new HashSet<string>();
		List<HelpSource> helpSources = new List<HelpSource>();
		Dictionary<string, Node> nameToNode = new Dictionary<string, Node>();
		Dictionary<string, HelpSource> nameToHelpSource = new Dictionary<string, HelpSource>();

		public IList<HelpSource> HelpSources {
			get {
				return this.helpSources.AsReadOnly();
			}
		}

		public DateTime LastHelpSourceTime {
			get;
			set;
		}

		static bool IsUnix {
			get {
				int platform = (int)Environment.OSVersion.Platform;
				return platform == 4 || platform == 128 || platform == 6;
			}
		}

		RootTree () : base (null, "Mono Documentation", "root:")
		{
			base.RootNode.EnsureNodes();
			this.LastHelpSourceTime = DateTime.Now;
		}

		public static void AddUncompiledSource (string path)
		{
			uncompiledHelpSourcePaths.Add (path);
		}

		public static RootTree LoadTree ()
		{
			return RootTree.LoadTree (RootTree.ProbeBaseDirectories ());
		}

		static string ProbeBaseDirectories ()
		{
			string result = ".";
			try {
				result = Settings.Get ("docPath") ?? ".";
			} catch {}

			return result;
		}

		public static RootTree LoadTree (string basedir, bool includeExternal = true)
		{
			if (string.IsNullOrEmpty (basedir))
				throw new ArgumentNullException ("basedir");
			if (!Directory.Exists (basedir))
				throw new ArgumentException ("basedir", string.Format ("Base documentation directory at '{0}' doesn't exist", basedir));

			XmlDocument xmlDocument = new XmlDocument ();
			string filename = Path.Combine (basedir, "monodoc.xml");
			xmlDocument.Load (filename);
			IEnumerable<string> sourceFiles = Directory.EnumerateFiles (Path.Combine (basedir, "sources"), "*.source");
			if (includeExternal)
				sourceFiles = sourceFiles.Concat (RootTree.ProbeExternalDirectorySources ());
			return RootTree.LoadTree (basedir, xmlDocument, sourceFiles);
		}

		static IEnumerable<string> ProbeExternalDirectorySources ()
		{
			IEnumerable<string> enumerable = Enumerable.Empty<string> ();
			try {
				string path = Settings.Get ("docExternalPath");
				enumerable = enumerable.Concat (System.IO.Directory.EnumerateFiles (path, "*.source"));
			}
			catch {}

			if (Directory.Exists ("/Library/Frameworks/Mono.framework/External/monodoc"))
				enumerable = enumerable.Concat (Directory.EnumerateFiles ("/Library/Frameworks/Mono.framework/External/monodoc", "*.source"));
			return enumerable;
		}

		public static RootTree LoadTree (string indexDir, XmlDocument docTree, IEnumerable<string> sourceFiles)
		{
			if (docTree == null) {
				docTree = new XmlDocument ();
				using  (Stream manifestResourceStream = typeof (RootTree).Assembly.GetManifestResourceStream ("monodoc.xml")) {
					docTree.Load (manifestResourceStream);
				}
			}

			sourceFiles =  (sourceFiles ?? new string[0]);
			RootTree rootTree = new RootTree ();
			rootTree.basedir = indexDir;
			XmlNodeList xml_node_list = docTree.SelectNodes ("/node/node");
			rootTree.nameToNode["root"] = rootTree.RootNode;
			rootTree.nameToNode["libraries"] = rootTree.RootNode;
			rootTree.Populate (rootTree.RootNode, xml_node_list);

			if (rootTree.LookupEntryPoint ("various") == null) {
				Console.Error.WriteLine ("No 'various' doc node! Check monodoc.xml!");
				Node rootNode = rootTree.RootNode;
			}

			foreach (string current in sourceFiles)
				rootTree.AddSourceFile (current);

			foreach (string path in uncompiledHelpSourcePaths) {
				var hs = new Providers.EcmaUncompiledHelpSource (path);
				hs.RootTree = rootTree;
				rootTree.helpSources.Add (hs);
				string epath = "extra-help-source-" + hs.Name;
				Node hsn = rootTree.RootNode.CreateNode (hs.Name, "root:/" + epath);
				rootTree.nameToHelpSource [epath] = hs;
				hsn.EnsureNodes ();
				foreach (Node n in hs.Tree.RootNode.Nodes)
					hsn.AddNode (n);
			}

			RootTree.PurgeNode (rootTree.RootNode);
			rootTree.RootNode.Sort ();
			return rootTree;
		}

		public void AddSource (string sourcesDir)
		{
			IEnumerable<string> enumerable = Directory.EnumerateFiles (sourcesDir, "*.source");
			foreach (string current in enumerable)
				if (!this.AddSourceFile (current))
					Console.Error.WriteLine ("Error: Could not load source file {0}", current);
		}

		public bool AddSourceFile (string sourceFile)
		{
			if (this.loadedSourceFiles.Contains (sourceFile))
				return false;

			Node node = this.LookupEntryPoint ("various") ?? base.RootNode;
			XmlDocument xmlDocument = new XmlDocument ();
			try {
				xmlDocument.Load (sourceFile);
			} catch {
				bool result = false;
				return result;
			}

			XmlNodeList extra_nodes = xmlDocument.SelectNodes ("/monodoc/node");
			if (extra_nodes.Count > 0)
				this.Populate (node, extra_nodes);

			XmlNodeList sources = xmlDocument.SelectNodes ("/monodoc/source");
			if (sources == null) {
				Console.Error.WriteLine ("Error: No <source> section found in the {0} file", sourceFile);
				return false;
			}

			loadedSourceFiles.Add (sourceFile);
			foreach (XmlNode xmlNode in sources) {
				XmlAttribute a = xmlNode.Attributes["provider"];
				if (a == null) {
					Console.Error.WriteLine ("Error: no provider in <source>");
					continue;
				}
				string provider = a.InnerText;
				a = xmlNode.Attributes["basefile"];
				if (a == null) {
					Console.Error.WriteLine ("Error: no basefile in <source>");
					continue;
				}
				string basefile = a.InnerText;
				a = xmlNode.Attributes["path"];
				if (a == null) {
					Console.Error.WriteLine ("Error: no path in <source>");
					continue;
				}
				string path = a.InnerText;
				string basefilepath = Path.Combine (Path.GetDirectoryName (sourceFile), basefile);
				HelpSource helpSource = RootTree.GetHelpSource (provider, basefilepath);
				if (helpSource != null) {
					helpSource.RootTree = this;
					this.helpSources.Add (helpSource);
					this.nameToHelpSource[path] = helpSource;
					Node node2 = this.LookupEntryPoint (path);
					if (node2 == null) {
						Console.Error.WriteLine ("node `{0}' is not defined on the documentation map", path);
						node2 = node;
					}
					foreach (Node current in helpSource.Tree.RootNode.Nodes) {
						node2.AddNode (current);
					}
					node2.Sort ();
				}
			}
			return true;
		}

		static bool PurgeNode (Node node)
		{
			bool result = false;
			if (!node.Documented)
			{
				List<Node> list = new List<Node> ();
				foreach (Node current in node.Nodes)
				{
					bool flag = RootTree.PurgeNode (current);
					if (flag)
					{
						list.Add (current);
					}
				}
				result =  (node.Nodes.Count == list.Count);
				foreach (Node current2 in list)
				{
					node.DeleteNode (current2);
				}
			}
			return result;
		}

		public static string[] GetSupportedFormats ()
		{
			return new string[]
			{
				"ecma",
				"ecmaspec",
				"error",
				"man",
				"xhtml"
			};
		}

		public static HelpSource GetHelpSource (string provider, string basefilepath)
		{
			HelpSource result;
			try {
				switch (provider) {
				case "xhtml":
				case "hb":
					result = new XhtmlHelpSource (basefilepath, false);
					break;
				case "man":
					result = new ManHelpSource (basefilepath, false);
					break;
				case "error":
					result = new ErrorHelpSource (basefilepath, false);
					break;
				case "ecmaspec":
					result = new EcmaSpecHelpSource (basefilepath, false);
					break;
				case "ecma":
					result = new EcmaHelpSource (basefilepath, false);
					break;
				default:
					Console.Error.WriteLine ("Error: Unknown provider specified: {0}", provider);
					result = null;
					break;
				}
			} catch (FileNotFoundException) {
				Console.Error.WriteLine ("Error: did not find one of the files in sources/" + basefilepath);
				result = null;
			}
			return result;
		}

		public static Provider GetProvider (string provider, params string[] basefilepaths)
		{
			switch (provider) {
			case "ecma":
				return new EcmaProvider (basefilepaths[0]);
			case "ecmaspec":
				return new EcmaSpecProvider (basefilepaths[0]);
			case "error":
				return new ErrorProvider (basefilepaths[0]);
			case "man":
				return new ManProvider (basefilepaths);
			case "xhml":
			case "hb":
				return new XhtmlProvider (basefilepaths[0]);
			}

			throw new NotSupportedException (provider);
		}

		void Populate (Node parent, XmlNodeList xml_node_list)
		{
			foreach (XmlNode xmlNode in xml_node_list) {
				XmlAttribute e = xmlNode.Attributes["parent"];
				Node parent2 = null;
				if (e != null && this.nameToNode.TryGetValue (e.InnerText, out parent2)) {
					xmlNode.Attributes.Remove (e);
					Populate (parent2, xmlNode.SelectNodes ("."));
					continue;
				}
				e = xmlNode.Attributes["label"];
				if (e == null) {
					Console.Error.WriteLine ("`label' attribute missing in <node>");
					continue;
				}
				string label = e.InnerText;
				e = xmlNode.Attributes["name"];
				if (e == null) {
					Console.Error.WriteLine ("`name' attribute missing in <node>");
					continue;
				}
				string name = e.InnerText;
				Node orCreateNode = parent.GetOrCreateNode (label, "root:/" + name);
				orCreateNode.EnsureNodes ();
				this.nameToNode[name] = orCreateNode;
				XmlNodeList xmlNodeList = xmlNode.SelectNodes ("./node");
				if (xmlNodeList != null) {
					this.Populate (orCreateNode, xmlNodeList);
				}
			}
		}

		public Node LookupEntryPoint (string name)
		{
			Node result = null;
			if (!this.nameToNode.TryGetValue (name, out result)) {
				result = null;
			}
			return result;
		}

		public TOutput RenderUrl<TOutput> (string url, IDocGenerator<TOutput> generator, HelpSource hintSource = null)
		{
			Node dummy;
			return RenderUrl<TOutput> (url, generator, out dummy, hintSource);
		}

		public TOutput RenderUrl<TOutput> (string url, IDocGenerator<TOutput> generator, out Node node, HelpSource hintSource = null)
		{
			node = null;
			string internalId = null;
			HelpSource hs = GetHelpSourceAndIdForUrl (url, hintSource, out internalId, out node);
			return generator.Generate (hs, internalId);
		}

		public HelpSource GetHelpSourceAndIdForUrl (string url, out string internalId)
		{
			Node dummy;
			return GetHelpSourceAndIdForUrl (url, out internalId, out dummy);
		}

		public HelpSource GetHelpSourceAndIdForUrl (string url, out string internalId, out Node node)
		{
			return GetHelpSourceAndIdForUrl (url, null, out internalId, out node);
		}

		public HelpSource GetHelpSourceAndIdForUrl (string url, HelpSource hintSource, out string internalId, out Node node)
		{
			node = null;
			internalId = null;

			if (url.StartsWith ("root:/", StringComparison.OrdinalIgnoreCase))
				return this.GetHelpSourceAndIdFromName (url.Substring ("root:/".Length), out internalId, out node);

			HelpSource helpSource = hintSource;
			if (helpSource == null || string.IsNullOrEmpty (internalId = helpSource.GetInternalIdForUrl (url, out node))) {
				helpSource = null;
				foreach (var hs in helpSources.Where (h => h.CanHandleUrl (url))) {
					if (!string.IsNullOrEmpty (internalId = hs.GetInternalIdForUrl (url, out node))) {
						helpSource = hs;
						break;
					}
				}
			}

			return helpSource;
		}

		public HelpSource GetHelpSourceAndIdFromName (string name, out string internalId, out Node node)
		{
			internalId = "root:";
			node = this.LookupEntryPoint (name);

			return node == null ? null : node.Nodes.Select (n => n.Tree.HelpSource).Where (hs => hs != null).Distinct ().FirstOrDefault ();
		}

		public HelpSource GetHelpSourceFromId (int id)
		{
			return  (id < 0 || id >= this.helpSources.Count) ? null : this.helpSources[id];
		}

		public Stream GetImage (string url)
		{
			if (url.StartsWith ("source-id:", StringComparison.OrdinalIgnoreCase)) {
				string text = url.Substring (10);
				int num = text.IndexOf (":");
				string text2 = text.Substring (0, num);
				int id = 0;
				try {
					id = int.Parse (text2);
				} catch {
					Console.Error.WriteLine ("Failed to parse source-id url: {0} `{1}'", url, text2);
					return null;
				}
				HelpSource helpSourceFromId = this.GetHelpSourceFromId (id);
				return helpSourceFromId.GetImage (text.Substring (num + 1));
			}
			Assembly assembly = Assembly.GetAssembly (typeof (RootTree));
			return assembly.GetManifestResourceStream (url);
		}

		public IndexReader GetIndex ()
		{
			try {
				string text = Path.Combine (this.basedir, "monodoc.index");
				if (File.Exists (text))
					return IndexReader.Load (text);

				text = Path.Combine (Settings.Get ("monodocIndexDirectory"), "monodoc.index");
				return IndexReader.Load (text);
			} catch {
				return null;
			}
		}

		public static void MakeIndex ()
		{
			RootTree rootTree = RootTree.LoadTree ();
			rootTree.GenerateIndex ();
		}

		public void GenerateIndex ()
		{
			IndexMaker indexMaker = new IndexMaker ();
			foreach (HelpSource current in this.helpSources)
				current.PopulateIndex (indexMaker);
			string text = Path.Combine (this.basedir, "monodoc.index");
			try {
				indexMaker.Save (text);
			} catch (UnauthorizedAccessException) {
				text = Path.Combine (Settings.Get ("docPath"), "monodoc.index");
				try {
					indexMaker.Save (text);
				} catch (UnauthorizedAccessException) {
					Console.WriteLine ("Unable to write index file in {0}", Path.Combine (Settings.Get ("docPath"), "monodoc.index"));
					return;
				}
			}
			if (RootTree.IsUnix)
				RootTree.chmod (text, 420);

			Console.WriteLine ("Documentation index at {0} updated", text);
		}

		public SearchableIndex GetSearchIndex ()
		{
			try {
				string text = Path.Combine (this.basedir, "search_index");
				if (System.IO.Directory.Exists (text))
					return SearchableIndex.Load (text);
				text = Path.Combine (Settings.Get ("docPath"), "search_index");
				return SearchableIndex.Load (text);
			} catch {
				return null;
			}
		}

		public static void MakeSearchIndex ()
		{
			RootTree rootTree = RootTree.LoadTree ();
			rootTree.GenerateSearchIndex ();
		}

		public void GenerateSearchIndex ()
		{
			Console.WriteLine ("Loading the monodoc tree...");
			string text = Path.Combine (this.basedir, "search_index");
			IndexWriter indexWriter;
			var analyzer = new StandardAnalyzer (Lucene.Net.Util.Version.LUCENE_CURRENT);
			var directory = Lucene.Net.Store.FSDirectory.Open (text);

			try {
				if (!Directory.Exists (text))
					Directory.CreateDirectory (text);
				indexWriter = new IndexWriter (directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);
			} catch (UnauthorizedAccessException) {
				try {
					text = Path.Combine (Settings.Get ("docPath"), "search_index");
					if (!Directory.Exists (text))
						Directory.CreateDirectory (text);
					indexWriter = new IndexWriter (directory, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);
				} catch (UnauthorizedAccessException) {
					Console.WriteLine ("You don't have permissions to write on " + text);
					return;
				}
			}
			Console.WriteLine ("Collecting and adding documents...");
			foreach (HelpSource current in this.helpSources) {
				current.PopulateSearchableIndex (indexWriter);
			}
			Console.WriteLine ("Closing...");
			indexWriter.Optimize ();
			indexWriter.Close ();
		}

		[DllImport ("libc")]
		static extern int chmod (string filename, int mode);
	}
}
