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

namespace Mono.MonoBASIC {

	/// <summary>
	///   Keeps track of the location in the program
	/// </summary>
	///
	/// <remarks>
	///   This uses a compact representation and a couple of auxiliary
	///   structures to keep track of tokens to (line, col, file) mappings.
	///
	public struct Location {
		const int NUM_ROW_BITS = 16;
		const int NUM_COL_BITS =  8;
		const int NUM_FILE_BITS = (32-NUM_ROW_BITS-NUM_COL_BITS);
		
		const int NUM_FILE_SHIFTS = 0;
		const int NUM_ROW_SHIFTS = NUM_FILE_BITS;
		const int NUM_COL_SHIFTS =  NUM_ROW_BITS+NUM_FILE_BITS;

		const int FILE_MASK = (1<<NUM_FILE_BITS)-1;
		const int ROW_MASK = ((1<<NUM_ROW_BITS)-1)<<NUM_FILE_BITS;
		const int COL_MASK = ((1<<NUM_COL_BITS)-1)<<(NUM_ROW_BITS+NUM_FILE_BITS);
		
		public int token; // ordered triplet: (Row, Col, File Index)

		static ArrayList source_list;
		static Hashtable source_files;
		static int source_count;
		static int current_source;
		static Hashtable sym_docs;

		public readonly static Location Null;
		
		static Location ()
		{
			source_files = new Hashtable ();
			source_list = new ArrayList ();
			current_source = 0;
			sym_docs = new Hashtable ();
			Null.token = 0;
		}

		static public void SetCurrentSource(string name)
		{
			int index;
			
			if (!source_files.Contains (name)) {
				index = ++source_count;
				source_files.Add (name, index);
				source_list.Add (name);
			}
			else {
				index = (int)source_files[name];
			}

			current_source = index;
		}
		
		public Location (int row, int col)
		{
			if (row < 0 || col <  0)
				token = 0;
			else {
				if (col > 255)
					col = 255;
				token = (current_source<<NUM_FILE_SHIFTS) + (row<<NUM_ROW_SHIFTS) + (col<<NUM_COL_SHIFTS);
			}
		}

		public override string ToString ()
		{
			return Name + ": (" + Row + ")";
		}
		
		static public bool IsNull (Location l)
		{
			return l.token == 0;
		}

		public string Name {
			get {
				if(token == 0)
					return "Internal";

				int index = (token & FILE_MASK)>>NUM_FILE_SHIFTS;
				string file = (string) source_list [index - 1];
				return file;
			}
		}

		public int Row {
			get {
				if (token == 0)
					return 1;

				return (token & ROW_MASK)>>NUM_ROW_SHIFTS;
			}
		}

		public int Col {
			get {
				if (token == 0)
					return 1;
				
				return (token & COL_MASK)>>NUM_COL_SHIFTS;
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
					doc = sw.DefineDocument (path, SymLanguageType.Basic,
								 SymLanguageVendor.Microsoft,
								 SymDocumentType.Text);

					sym_docs.Add (path, doc);
				}

				return doc;
			}
		}
	}
}
