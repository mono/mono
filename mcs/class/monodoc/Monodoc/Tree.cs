using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

namespace Monodoc
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
#if LEGACY_MODE
		: Node
#endif
	{
		public const long CurrentVersionNumber = 1;
		const int VersionNumberKey = -(int)'v';
		public readonly HelpSource HelpSource;
	
		FileStream InputStream;
		BinaryReader InputReader;

#if !LEGACY_MODE
		// This is the node which contains all the other node of the tree
		Node rootNode;
#endif

		/// <summary>
		///   Load from file constructor
		/// </summary>
		public Tree (HelpSource hs, string filename)
#if LEGACY_MODE
			: base (null, null)
#endif
		{
			HelpSource = hs;
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
			// Try to read old version information
			if (InputReader.ReadInt32 () == VersionNumberKey)
				VersionNumber = InputReader.ReadInt64 ();
			else {
				// We try to see if there is a version number at the end of the file
				InputStream.Seek (-(4 + 8), SeekOrigin.End); // VersionNumberKey + long
				try {
					if (InputReader.ReadInt32 () == VersionNumberKey)
						VersionNumber = InputReader.ReadInt64 ();
				} catch {}
				// We set the stream back at the beginning of the node definition list
				InputStream.Position = 4;
			}

			var position = InputReader.ReadInt32 ();
#if !LEGACY_MODE
			rootNode = new Node (this, position);
#else
			Address = position;
#endif
			InflateNode (RootNode);
		}

		/// <summary>
		///    Tree creation and merged tree constructor
		/// </summary>
		public Tree (HelpSource hs, string caption, string url)
#if !LEGACY_MODE
			: this (hs, null, caption, url)
		{
		}
#else
			: base (caption, url)
		{
			HelpSource = hs;
		}
#endif


		public Tree (HelpSource hs, Node parent, string caption, string element)
#if LEGACY_MODE
			: base (parent, caption, element)
#endif
		{
			HelpSource = hs;
#if !LEGACY_MODE
			rootNode = parent == null ? new Node (this, caption, element) : new Node (parent, caption, element);
#endif
		}

		/// <summary>
		///    Saves the tree into the specified file using the help file format.
		/// </summary>
		public void Save (string file)
		{
			Encoding utf8 = new UTF8Encoding (false, true);
			using (FileStream output = File.OpenWrite (file)){
				// Skip over the pointer to the first node.
				output.Position = 4 + 4;
			
				using (BinaryWriter writer = new BinaryWriter (output, utf8)) {
					// Recursively dump
					RootNode.Serialize (output, writer);
					// We want to generate 2.10 compatible files so we write the version number at the end
					writer.Write (VersionNumberKey);
					writer.Write (CurrentVersionNumber);

					output.Position = 0;
					writer.Write (new byte [] { (byte) 'M', (byte) 'o', (byte) 'H', (byte) 'P' });
					writer.Write (RootNode.Address);
				}
			}
		}

		public Node RootNode {
			get {
#if LEGACY_MODE
				return this;
#else
				return rootNode;
#endif
			}
		}

		public long VersionNumber {
			get;
			private set;
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

		// Nodes use this value to know if they should manually re-sort their child
		// if they come from an older generator version
		internal bool ForceResort {
			get {
				return VersionNumber == 0;
			}
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
			if (node.ChildNodes.Count == 0)
				return;

			indent++;
			foreach (Node n in node.ChildNodes)
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
			foreach (Node n in root.ChildNodes) {
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
