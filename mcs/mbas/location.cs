//
// location.cs: Keeps track of the location of source code entity
//
// Author:
//   Miguel de Icaza
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.IO;
using System.Collections;
using System.Diagnostics.SymbolStore;

namespace Mono.CSharp {
	/// <summary>
	///   Keeps track of the location in the program
	/// </summary>
	///
	/// <remarks>
	///   This uses a compact representation and a couple of auxiliary
	///   structures to keep track of tokens to (file,line) mappings.
	///
	///   We could probably also keep track of columns by storing those
	///   in 8 bits (and say, map anything after char 255 to be `255+').
	/// </remarks>
	public struct Location {
		public int token; 

		static Hashtable map;
		static Hashtable sym_docs;
		static ArrayList list;
		static int global_count;
		static int module_base;

		public readonly static Location Null;
		
		static Location ()
		{
			map = new Hashtable ();
			list = new ArrayList ();
			sym_docs = new Hashtable ();
			global_count = 0;
			module_base = 0;
			Null.token = -1;
		}

		static public void Push (string name)
		{
			map.Remove (global_count);
			map.Add (global_count, name);
			list.Add (global_count);
			module_base = global_count;
		}
		
		public Location (int row)
		{
			if (row < 0)
				token = -1;
			else {
				token = module_base + row;
				if (global_count < token)
					global_count = token;
			}
		}

		public override string ToString ()
		{
			return Name + ": (" + Row + ")";
		}
		
		/// <summary>
		///   Whether the Location is Null
		/// </summary>
		static public bool IsNull (Location l)
		{
			return l.token == -1;
		}

		public string Name {
			get {
				int best = 0;
				
				if (token < 0)
					return "Internal";

				foreach (int b in list){
					if (token > b)
						best = b;
				}
				return (string) map [best];
			}
		}

		public int Row {
			get {
				int best = 0;
				
				if (token < 0)
					return 1;
				
				foreach (int b in list){
					if (token > b)
						best = b;
				}
				return token - best;
			}
		}

		// The ISymbolDocumentWriter interface is used by the symbol writer to
		// describe a single source file - for each source file there's exactly
		// one corresponding ISymbolDocumentWriter instance.
		//
		// This class has an internal hash table mapping source document names
		// to such ISymbolDocumentWriter instances - so there's exactly one
		// instance per document.
		//
		// This property returns the ISymbolDocumentWriter instance which belongs
		// to the location's source file.
		//
		// If we don't have a symbol writer, this property is always null.
		public ISymbolDocumentWriter SymbolDocument {
			get {
				ISymbolWriter sw = CodeGen.SymbolWriter;
				ISymbolDocumentWriter doc;

				if (token < 0)
					return null;

				// If we don't have a symbol writer, return null.
				if (sw == null)
					return null;

				string path = Path.GetFullPath (Name);

				if (sym_docs.Contains (path))
					// If we already created an ISymbolDocumentWriter
					// instance for this document, return it.
					doc = (ISymbolDocumentWriter) sym_docs [path];
				else {
					// Create a new ISymbolDocumentWriter instance and
					// store it in the hash table.
					doc = sw.DefineDocument (path, SymLanguageType.CSharp,
								 SymLanguageVendor.Microsoft,
								 SymDocumentType.Text);

					sym_docs.Add (path, doc);
				}

				return doc;
			}
		}
	}
}
