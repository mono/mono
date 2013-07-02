using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Monodoc
{
	public
#if LEGACY_MODE
	partial
#endif
	class Node : IComparable<Node>, IComparable
	{
		readonly Tree parentTree;
		string caption, element, pubUrl;
		public bool Documented;
		bool loaded;
		Node parent;
		List<Node> nodes;
#if LEGACY_MODE
		ArrayList legacyNodes;
#endif
		Dictionary<string, Node> childrenLookup;
		bool elementSort;
		/* Address has three types of value, 
		 *   _ 0 is for no on-disk representation
		 *   _ >0 is a valid address that is loaded immediately
		 *   _ <0 is a valid negated address to indicate lazy loading
		 */
		int address;

#if LEGACY_MODE
		[Obsolete ("Tree inheriting Node is being phased out. Use the `Tree.RootNode' property instead")]
		public Node (string caption, string element)
		{
			this.parentTree = (Tree) this;
			this.caption = caption;
			this.element = element;
			parent = null;
		}
#endif

		public Node (Node parent, string caption, string element) : this (parent.Tree, caption, element)
		{
			this.parent = parent;
		}

		internal Node (Tree tree, string caption, string element)
		{
			this.parentTree = tree;
			this.caption = caption;
			this.element = element;
			this.elementSort = parentTree.HelpSource != null && parentTree.HelpSource.SortType == SortType.Element;
		}
	
		/// <summary>
		///    Creates a node from an on-disk representation
		/// </summary>
		internal Node (Node parent, int address) : this (parent.parentTree, address)
		{
			this.parent = parent;
		}

		internal Node (Tree tree, int address)
		{
			this.address = address;
			this.parentTree = tree;
			this.elementSort = parentTree.HelpSource != null && parentTree.HelpSource.SortType == SortType.Element;
			if (address > 0)
				LoadNode ();
		}

		/* This is solely used for MatchNode to check for equality */
		internal Node ()
		{
		}

		void LoadNode ()
		{
			parentTree.InflateNode (this);
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

		[Obsolete ("Use ChildNodes")]
		public ArrayList Nodes {
			get {
				if (legacyNodes == null)
					legacyNodes = new ArrayList (ChildNodes as ICollection);
				return legacyNodes;
			}
		}

		public IList<Node> ChildNodes {
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
				return parentTree;
			}
		}

		internal int Address {
			get {
				return address;
			}
#if LEGACY_MODE
			set {
				address = value;
			}
#endif
		}
	
		/// <summary>
		///   Creates a new node, in the locator entry point, and with
		///   a user visible caption of @caption
		/// </summary>
		public Node CreateNode (string c_caption, string c_element)
		{
			EnsureNodes ();
			if (string.IsNullOrEmpty (c_caption))
				throw new ArgumentNullException ("c_caption");
			if (string.IsNullOrEmpty (c_element))
				throw new ArgumentNullException ("c_element");

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

			if (parentTree.ForceResort)
				nodes.Sort ();
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
				if (pubUrl != null)
					return pubUrl;
				return pubUrl = parentTree.HelpSource != null ? parentTree.HelpSource.GetPublicUrl (this) : GetInternalUrl ();
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

			var cap1 = elementSort ? element : caption;
			var cap2 = elementSort ? other.element : other.caption;

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

			return string.Compare (cap1, cap2, StringComparison.Ordinal);
		}
	}

	internal static class IListExtensions
	{
		// TODO: if the backing store ever change from List<T>, we need to tune these methods to have a fallback mechanism
		public static int BinarySearch<T> (this IList<T> ilist, T item)
		{
			var list = ilist as List<T>;
			if (list == null)
				throw new NotSupportedException ();
			return list.BinarySearch (item);
		}

		public static int BinarySearch<T> (this IList<T> ilist, T item, IComparer<T> comparer)
		{
			var list = ilist as List<T>;
			if (list == null)
				throw new NotSupportedException ();
			return list.BinarySearch (item, comparer);
		}
	}
}
