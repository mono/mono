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
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {
	/// <summary>
	///   This is one single source file.
	/// </summary>
	/// <remarks>
	///   This is intentionally a class and not a struct since we need
	///   to pass this by reference.
	/// </remarks>
	public sealed class SourceFile : ISourceFile {
		public readonly string Name;
		public readonly string Path;
		public readonly int Index;
		public SourceFileEntry SourceFileEntry;
		public bool HasLineDirective;

		public SourceFile (string name, string path, int index)
		{
			this.Index = index;
			this.Name = name;
			this.Path = path;
		}

		SourceFileEntry ISourceFile.Entry {
			get { return SourceFileEntry; }
		}

		public override string ToString ()
		{
			return String.Format ("SourceFile ({0}:{1}:{2}:{3})",
					      Name, Path, Index, SourceFileEntry);
		}
	}

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

		static ArrayList source_list;
		static Hashtable source_files;
		static int source_bits;
		static int source_mask;
		static int source_count;
		static int current_source;

		public readonly static Location Null;
		
		static Location ()
		{
			source_files = new Hashtable ();
			source_list = new ArrayList ();
			current_source = 0;
			Null.token = 0;
		}

		// <summary>
		//   This must be called before parsing/tokenizing any files.
		// </summary>
		static public void AddFile (string name)
		{
			string path = Path.GetFullPath (name);

			if (source_files.Contains (path)){
				Report.Warning (2002, name, "Source file '{0}' specified multiple times", path);
				return;
			}

			source_files.Add (path, ++source_count);
			source_list.Add (new SourceFile (name, path, source_count));
		}

		static public SourceFile[] SourceFiles {
			get {
				SourceFile[] retval = new SourceFile [source_list.Count];
				source_list.CopyTo (retval, 0);
				return retval;
			}
		}

		static int log2 (int number)
		{
			int bits = 0;
			while (number > 0) {
				bits++;
				number /= 2;
			}

			return bits;
		}

		// <summary>
		//   After adding all source files we want to compile with AddFile(), this method
		//   must be called to `reserve' an appropriate number of bits in the token for the
		//   source file.  We reserve some extra space for files we encounter via #line
		//   directives while parsing.
		// </summary>
		static public void Initialize ()
		{
			source_bits = log2 (source_list.Count) + 2;
			source_mask = (1 << source_bits) - 1;
		}

		// <remarks>
		//   This is used when we encounter a #line preprocessing directive.
		// </remarks>
		static public SourceFile LookupFile (string name)
		{
			string path = name == "" ? "" : Path.GetFullPath (name);

			if (!source_files.Contains (path)) {
				if (source_count >= (1 << source_bits))
					return new SourceFile (name, path, 0);

				source_files.Add (path, ++source_count);
				SourceFile retval = new SourceFile (name, path, source_count);
				source_list.Add (retval);
				return retval;
			}

			int index = (int) source_files [path];
			return (SourceFile) source_list [index - 1];
		}

		static public void Push (SourceFile file)
		{
			current_source = file.Index;
		}

		// <remarks>
		//   If we're compiling with debugging support, this is called between parsing
		//   and code generation to register all the source files with the
		//   symbol writer.
		// </remarks>
		static public void DefineSymbolDocuments (SymbolWriter symwriter)
		{
			foreach (SourceFile file in source_list) {
				file.SourceFileEntry = symwriter.DefineDocument (file.Path);
			}
		}
		
		public Location (int row)
		{
			if (row < 0)
				token = 0;
			else
				token = current_source + (row << source_bits);
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
			return l.token == 0;
		}

		public string Name {
			get {
				int index = token & source_mask;
				if ((token == 0) || (index == 0))
					return "Internal";

				SourceFile file = (SourceFile) source_list [index - 1];
				return file.Name;
			}
		}

		public int Row {
			get {
				if (token == 0)
					return 1;

				return token >> source_bits;
			}
		}

		public int File {
			get {
				return token & source_mask;
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
		public SourceFile SourceFile {
			get {
				int index = token & source_mask;
				if (index == 0)
					return null;
				return (SourceFile) source_list [index - 1];
			}
		}
	}
}
