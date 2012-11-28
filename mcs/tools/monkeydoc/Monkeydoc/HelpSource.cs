using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

using Mono.Utilities;
using Lucene.Net.Index;

namespace MonkeyDoc
{
	//
	// The HelpSource class keeps track of the archived data, and its
	// tree
	//
	public class HelpSource
	{
		static int id;

		//
		// The unique ID for this HelpSource.
		//
		int source_id;

		// The name of the HelpSource, used by all the file (.tree, .zip, ...) used by it
		string name;
		// The full directory path where the HelpSource files are located
		string basePath;

		// The tree of this help source
		Tree tree;
		string treeFilePath;
		RootTree rootTree;

		IDocCache cache = new MonkeyDoc.Caches.FileCache (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "monkeydoc", "cache"));
		IDocStorage storage;

		public HelpSource (string base_filename, bool create)
		{
			this.name = Path.GetFileName (base_filename);
			this.basePath = Path.GetDirectoryName (base_filename);
			this.treeFilePath = base_filename + ".tree";
			this.storage = new MonkeyDoc.Storage.ZipStorage (base_filename + ".zip");

			tree = create ? new Tree (this, string.Empty, string.Empty) : new Tree (this, treeFilePath);

			source_id = id++;
		}
	
		public HelpSource ()
		{
			tree = new Tree (this, "Blah", "Blah");
			source_id = id++;
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

		/* This gives the full path of the source/ directory */
		public string BaseFilePath {
			get {
				return basePath;
			}
		}

		public TraceLevel TraceLevel {
			get;
			set;
		}

		public string BaseDir {
			get {
				return basePath;
			}
		}

		public Tree Tree {
			get {
				return tree;
			}
		}

		public RootTree RootTree {
			get {
				return rootTree;
			}
			set {
				rootTree = value;
			}
		}

		public IDocCache Cache {
			get {
				return cache;
			}
		}

		public IDocStorage Storage {
			get {
				return storage;
			}
		}

		// A HelpSource may have a common prefix to its URL, give it here
		protected virtual string UriPrefix {
			get {
				return "dummy:";
			}
		}
	
		/// <summary>
		///   Returns a stream from the packaged help source archive
		/// </summary>
		public virtual Stream GetHelpStream (string id)
		{
			return storage.Retrieve (id);
		}

		public virtual Stream GetCachedHelpStream (string id)
		{
			if (string.IsNullOrEmpty (id))
				throw new ArgumentNullException ("id");
			if (!cache.CanCache (DocEntity.Text))
				return GetHelpStream (id);
			if (!cache.IsCached (id))
				cache.CacheText (id, GetHelpStream (id));
			return cache.GetCachedStream (id);
		}

		public XmlReader GetHelpXml (string id)
		{
			var url = "monodoc:///" + SourceID + "@" + Uri.EscapeDataString (id) + "@";
			var stream = cache.IsCached (id) ? cache.GetCachedStream (id) : storage.Retrieve (id);
			
			return stream == null ? null : new XmlTextReader (url, stream);
		}
	
		public virtual XmlDocument GetHelpXmlWithChanges (string id)
		{
			XmlDocument doc = new XmlDocument ();
			if (!storage.SupportRevision) {
				doc.Load (GetHelpXml (id));
			} else {
				var revManager = storage.RevisionManager;
				doc.Load (revManager.RetrieveLatestRevision (id));
			}
			return doc;
		}

		public virtual string GetCachedText (string id)
		{
			if (!cache.CanCache (DocEntity.Text))
				return GetText (id);
			if (!cache.IsCached (id))
				cache.CacheText (id, GetText (id));
			return cache.GetCachedString (id);
		}

		public virtual string GetText (string id)
		{
			return new StreamReader (GetHelpStream (id)).ReadToEnd ();
		}

		// Tells if the result for the provided id is generated dynamically
		// by the help source
		public virtual bool IsGeneratedContent (string id)
		{
			return false;
		}

		// Tells if the content of the provided id is meant to be returned raw
		public virtual bool IsRawContent (string id)
		{
			return false;
		}

		// Tells if provided id refers to a multi-content-type document if it's case
		// tells the ids it's formed of
		public virtual bool IsMultiPart (string id, out IEnumerable<string> parts)
		{
			parts = null;
			return false;
		}

		/// <summary>
		///   Saves the tree and the archive
		/// </summary>
		public void Save ()
		{
			tree.Save (treeFilePath);
			storage.Dispose ();
		}
	
		public virtual void RenderPreviewDocs (XmlNode newNode, XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		public virtual string GetPublicUrl (Node node)
		{
			return node.GetInternalUrl ();
		}

		public virtual bool CanHandleUrl (string url)
		{
			return url.StartsWith (UriPrefix, StringComparison.OrdinalIgnoreCase);
		}

		public virtual string GetInternalIdForUrl (string url, out Node node)
		{
			node = MatchNode (url);
			return node == null ? null : url.Substring (UriPrefix.Length);
		}
		
		public virtual Node MatchNode (string url)
		{
			Node current = null;

			var cache = LRUCache<string, Node>.Default;
			if ((current = cache.Get (url)) != null)
				return current;

			current = Tree.RootNode;
			var strippedUrl = url.StartsWith (UriPrefix, StringComparison.OrdinalIgnoreCase) ? url.Substring (UriPrefix.Length) : url;
			var searchNode = new Node () { Element = strippedUrl };

			do {
				int index = current.Nodes.BinarySearch (searchNode, NodeElementComparer.Instance);
				if (index >= 0) {
					Node n = current.Nodes[index];
					//Console.WriteLine ("Binarysearch success for {0} which fell on {1}", strippedUrl, n.Element);
					cache.Put (url, n);
					return n;
				}
				index = ~index;
				if (index == current.Nodes.Count) {
					//Console.WriteLine ("Match fail for {0}", strippedUrl);
					//Console.WriteLine (current.Nodes.Select (n => n.Element).Aggregate ((e1, e2) => e1 + ", " + e2));
					return SlowMatchNode (Tree.RootNode, cache, strippedUrl);
				}
				current = current.Nodes [index - 1];
				//Console.WriteLine ("Binarysearch failed for {0}, next node check is {1}", strippedUrl, current.Element);
			} while (true);

			return null;
		}

		/* That slow path is mainly here to handle ecmaspec type of url which are composed of hard to sort numbers
		 * because they don't have the same amount of digit. We could use a regex to harmonise the various number
		 * parts but then it would be quite specific. Since in the case of ecmaspec the tree is well-formed enough
		 * the "Slow" match should still be fast enough
		 */
		Node SlowMatchNode (Node current, LRUCache<string, Node> cache, string url)
		{
			//Console.WriteLine ("Entering slow path for {0} starting from {1}", url, current.Element);
			while (current != null) {
				bool stop = true;
				foreach (Node n in current.Nodes) {
					var element = n.Element.StartsWith (UriPrefix, StringComparison.OrdinalIgnoreCase) ? n.Element.Substring (UriPrefix.Length) : n.Element;
					if (url == element) {
						cache.Put (url, n);
						return n;
					} else if (url.StartsWith (element + ".", StringComparison.OrdinalIgnoreCase) && !n.IsLeaf) {
						current = n;
						stop = false;
						break;
					}
				}
				if (stop)
					current = null;
			}

			return null;
		}
		
		class NodeElementComparer : IComparer<Node>
		{
			public static NodeElementComparer Instance = new NodeElementComparer ();

			public int Compare (Node n1, Node n2)
			{
				return string.Compare (Cleanup (n1), Cleanup (n2), StringComparison.Ordinal);
			}

			string Cleanup (Node n)
			{
				var prefix = n.Tree != null && n.Tree.HelpSource != null ? n.Tree.HelpSource.UriPrefix : string.Empty;
				var element = n.Element.StartsWith (prefix, StringComparison.OrdinalIgnoreCase) ? n.Element.Substring (prefix.Length) : n.Element;
				if (char.IsDigit (element, 0)) {
					var count = element.TakeWhile (char.IsDigit).Count ();
					element = element.PadLeft (Math.Max (0, 3 - count) + element.Length, '0');
				}
				//Console.WriteLine ("Cleaned up {0} to {1}", n.Element, element);
				return element;
			}
		}

		public virtual DocumentType GetDocumentTypeForId (string id, out Dictionary<string, string> extraParams)
		{
			extraParams = null;
			return DocumentType.PlainText;
		}

		public virtual Stream GetImage (string url)
		{
			return null;
		}

		//
		// Populates the index.
		//
		public virtual void PopulateIndex (IndexMaker index_maker)
		{
		}

		//
		// Create different Documents for adding to Lucene search index
		// The default action is do nothing. Subclasses should add the docs
		// 
		public virtual void PopulateSearchableIndex (IndexWriter writer)
		{

		}
	}
}