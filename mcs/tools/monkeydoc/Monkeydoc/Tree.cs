using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

namespace MonkeyDoc
{
	/// <summary>
	///    This tree is populated by the documentation providers, or populated
	///    from a binary encoding of the tree.  The format of the tree is designed
	///    to minimize the need to load it in full.
	/// </summary>

	/* Ideally this class should also be abstracted to let user have something
	 * else than a file as a backing store, a database for instance
	 */
	public class Tree
	{
		public readonly HelpSource HelpSource;
	
		FileStream InputStream;
		BinaryReader InputReader;

		// This is the node which contains all the other node of the tree
		Node rootNode;

		/// <summary>
		///   Load from file constructor
		/// </summary>
		public Tree (HelpSource hs, string filename)
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
			var position = InputReader.ReadInt32 ();
			rootNode = new Node (this, position);
			InflateNode (rootNode);

			HelpSource = hs;
		}

		/// <summary>
		///    Tree creation and merged tree constructor
		/// </summary>
		public Tree (HelpSource hs, string caption, string url) : this (hs, null, caption, url)
		{
		}

		public Tree (HelpSource hs, Node parent, string caption, string element)
		{
			HelpSource = hs;
			rootNode = parent == null ? new Node (this, caption, element) : new Node (parent, caption, element);
		}

		/// <summary>
		///    Saves the tree into the specified file using the help file format.
		/// </summary>
		public void Save (string file)
		{
			Encoding utf8 = new UTF8Encoding (false, true);
			using (FileStream output = File.OpenWrite (file)){
				// Skip over the pointer to the first node.
				output.Position = 8;
			
				using (BinaryWriter writer = new BinaryWriter (output, utf8)) {
					// Recursively dump
					rootNode.Serialize (output, writer);

					output.Position = 0;
					writer.Write (new byte [] { (byte) 'M', (byte) 'o', (byte) 'H', (byte) 'P' });
					writer.Write (rootNode.Address);
				}
			}
		}

		public Node RootNode {
			get {
				return rootNode;
			}
		}

		static bool GoodSig (byte [] sig)
		{
			if (sig.Length != 4)
				return false;
			return sig [0] == (byte) 'M'
				&& sig [1] == (byte) 'o'
			    && sig [2] == (byte) 'H'
				&& sig [3] == (byte) 'P';
		}

		public void InflateNode (Node baseNode)
		{
			var address = baseNode.Address;
			if (address < 0)
				address = -address;

			InputStream.Position = address;
			baseNode.Deserialize (InputReader);
		}
	}
	
	public class Node : IComparable<Node>, IComparable
	{
		readonly Tree tree;
		string caption, element;
		public bool Documented;
		bool loaded;
		Node parent;
		List<Node> nodes;
		Dictionary<string, Node> childrenLookup;
		/* Address has three types of value, 
		 *   _ 0 is for no on-disk representation
		 *   _ >0 is a valid address that is loaded immediately
		 *   _ <0 is a valid negated address to indicate lazy loading
		 */
		int address;

		public Node (Node parent, string caption, string element) : this (parent.Tree, caption, element)
		{
			this.parent = parent;
		}

		internal Node (Tree tree, string caption, string element)
		{
			this.tree = tree;
			this.caption = caption;
			this.element = element;
		}
	
		/// <summary>
		///    Creates a node from an on-disk representation
		/// </summary>
		internal Node (Node parent, int address) : this (parent.tree, address)
		{
			this.parent = parent;
		}

		internal Node (Tree tree, int address)
		{
			this.address = address;
			this.tree = tree;
			if (address > 0)
				LoadNode ();
		}

		/* This is solely used for MatchNode to check for equality */
		internal Node ()
		{
		}

		void LoadNode ()
		{
			tree.InflateNode (this);
			if (parent != null)
				parent.RegisterFullNode (this);
		}

		public void AddNode (Node n)
		{
			nodes.Add (n);
			n.parent = this;
			n.Documented = true;
			RegisterFullNode (n);
		}

		public void DeleteNode (Node n)
		{
			nodes.Remove (n);
			if (!string.IsNullOrEmpty (n.element))
				childrenLookup.Remove (n.element);
		}

		// When a child node is inflated, it calls this method
		// so that we can add it to our lookup for quick search
		void RegisterFullNode (Node child)
		{
			if (childrenLookup == null)
				childrenLookup = new Dictionary<string, Node> ();
			if (!string.IsNullOrEmpty (child.element))
				childrenLookup[child.element] = child;
		}

		public List<Node> Nodes {
			get {
				EnsureLoaded ();
				return nodes != null ? nodes : new List<Node> ();
			}
		}

		public string Element {
			get {
				EnsureLoaded ();
				return element;
			}
			set {
				element = value;
			}
		}

		public string Caption {
			get {
				EnsureLoaded ();
				return caption;
			}
			internal set {
				caption = value;
			}
		}
	
		public Node Parent {
			get {
				return parent;
			}
		}

		public Tree Tree {
			get {
				return tree;
			}
		}

		internal int Address {
			get {
				return address;
			}
		}
	
		/// <summary>
		///   Creates a new node, in the locator entry point, and with
		///   a user visible caption of @caption
		/// </summary>
		public Node CreateNode (string c_caption, string c_element)
		{
			EnsureNodes ();

			Node t = new Node (this, c_caption, c_element);
			nodes.Add (t);
			childrenLookup[c_element] = t;

			return t;
		}

		public Node GetOrCreateNode (string c_caption, string c_element)
		{
			if (nodes == null)
				return CreateNode (c_caption, c_element);
			if (childrenLookup.Count != nodes.Count || (nodes.Count == 0 && childrenLookup.Count != nodes.Capacity))
				UpdateLookup ();

			Node result;
			if (!childrenLookup.TryGetValue (c_element, out result))
				result = CreateNode (c_caption, c_element);
			return result;
		}

		public void EnsureNodes ()
		{
			if (nodes == null) {
				nodes = new List<Node> ();
				childrenLookup = new Dictionary<string, Node> ();
			}
		}

		public void EnsureLoaded ()
		{
			if (address < 0 && !loaded) {
				LoadNode ();
				loaded = true;
			}
		}

		void UpdateLookup ()
		{
			foreach (var node in nodes)
				childrenLookup[node.Element] = node;
		}
	
		public bool IsLeaf {
			get {
				return nodes == null || nodes.Count == 0;
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

		internal void Deserialize (BinaryReader reader)
		{
			int count = DecodeInt (reader);
			element = reader.ReadString ();
			caption = reader.ReadString ();

			if (count == 0)
				return;
		
			nodes = new List<Node> (count);
			for (int i = 0; i < count; i++) {
				int child_address = DecodeInt (reader);
							      
				Node t = new Node (this, -child_address);
				nodes.Add (t);
			}
		}

		internal void Serialize (FileStream output, BinaryWriter writer)
		{
			if (nodes != null)
				foreach (Node child in nodes)
					child.Serialize (output, writer);

			address = (int) output.Position;
			EncodeInt (writer, nodes == null ? 0 : (int) nodes.Count);
			writer.Write (element);
			writer.Write (caption);

			if (nodes != null)
				foreach (Node child in nodes)
					EncodeInt (writer, child.address);
		}

		public void Sort ()
		{
			if (nodes != null)
				nodes.Sort ();
		}

		internal string GetInternalUrl ()
		{
			EnsureLoaded ();
			if (element.IndexOf (":") != -1 || parent == null)
				return element;

			var parentUrl = parent.GetInternalUrl ();
			return parentUrl.EndsWith ("/") ? parentUrl + element : parentUrl + "/" + element;
		}
		
		public string PublicUrl {
			get {
				var url = GetInternalUrl ();
				return tree.HelpSource != null ? tree.HelpSource.GetPublicUrl (this) : url;
			}
		}

		int IComparable.CompareTo (object obj)
		{
			Node other = obj as Node;
			if (other == null)
				return -1;
			return CompareToInternal (other);
		}

		int IComparable<Node>.CompareTo (Node obj)
		{
			return CompareToInternal (obj);
		}

		int CompareToInternal (Node other)
		{
			EnsureLoaded ();
			other.EnsureLoaded ();

			var cap1 = caption;
			var cap2 = other.caption;

			/* Some node (notably from ecmaspec) have number prepended to them
			 * which we need to sort better by padding them to the same number
			 * of digits
			 */
			if (char.IsDigit (cap1[0]) && char.IsDigit (cap2[0])) {
				int c1 = cap1.TakeWhile (char.IsDigit).Count ();
				int c2 = cap2.TakeWhile (char.IsDigit).Count ();
				
				if (c1 != c2) {
					cap1 = cap1.PadLeft (cap1.Length + Math.Max (0, c2 - c1), '0');
					cap2 = cap2.PadLeft (cap2.Length + Math.Max (0, c1 - c2), '0');
				}
			}

			return string.Compare (cap1, cap2, StringComparison.OrdinalIgnoreCase);
		}
	}

	public static class TreeDumper
	{
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

		public static string ExportToTocXml (Node root, string title, string desc)
		{
			if (root == null)
				throw new ArgumentNullException ("root");
			// Return a toc index of sub-nodes
			StringBuilder buf = new StringBuilder ();
			var writer = XmlWriter.Create (buf);
			writer.WriteStartElement ("toc");
			writer.WriteAttributeString ("title", title ?? string.Empty);
			writer.WriteElementString ("description", desc ?? string.Empty);
			writer.WriteStartElement ("list");
			foreach (Node n in root.Nodes) {
				writer.WriteStartElement ("item");
				writer.WriteAttributeString ("url", n.Element);
				writer.WriteValue (n.Caption);
				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();
			writer.WriteEndElement ();
			writer.Flush ();
			writer.Close ();

			return buf.ToString ();
		}
	}
}
