//
// create-category-table.cs - Generate Unicode category tables for the
// Mono runtime.
//
// Author:
//   Damien Diederen (dd@crosstwine.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// SYNOPSIS
//   create-category-table.exe --dump <dump-file>
//   create-category-table.exe --encode <dump-file> <data-name> <h-file>
//
// DESCRIPTION
//   Dump, encode and generate (partially) bi-level category tables
//   containing variants of the Unicode category database.
//
//   With --dump <dump-file>, dump the contents of the hosting
//   runtime's database to <dump-file> in an easily-parseable ASCII
//   format.
//
//   With --encode <dump-file> <data-name> <h-file>, load a previously
//   generated dump and create the corresponding header file
//   containing two static C arrays: '<data_name>' and
//   '<data_name>_astral_index'.
//
//   The main table is linear for code points in the U+0000..U+FFFF
//   range; the 'astral_index' portion is necessary to select pages
//   related to code points in the astral planes:
//
//    data [(astral_index [(cp - 0x10000) >> 8] << 8) + (cp & 0xff)]

using System;
using System.Diagnostics;
using System.IO;
using System.Globalization;

// No .Generics mean this program can be compiled and run on v1.1
// after updating MaxCodePoint and removing Char.ConvertFromUtf32
// below.
using System.Collections;

namespace Mono.Globalization.Unicode
{
	public enum Language
	{
		C,
		CSharp
	}

	public class PagedTableEncoder
	{
		public interface IData
		{
			ushort this [int index]
			{
				get;
			}

			int Count
			{
				get;
			}
		}

		public class ArrayData : PagedTableEncoder.IData
		{
			public ArrayData (Array data)
			{
				this.data = data;
			}

			public ushort this [int index]
			{
				get {
					IConvertible value = (IConvertible)data.GetValue (index);

					return value.ToUInt16 (null);
				}
			}

			public int Count
			{
				get {
					return data.Length;
				}
			}

			Array data;
		};

		class Page
		{
			public Page (int first_base, int number, ushort [] data)
			{
				this.first_base = first_base;
				this.number = number;
				this.data = data;
			}

			public void AddIndexEntry (IndexEntry index_entry)
			{
				index_entries.Add (index_entry);
			}

			public bool Contains (ushort [] data)
			{
				for (int i = 0; i < data.Length; i++)
					if (this.data [i] != data [i])
						return false;
				return true;
			}

			public readonly int first_base;

			public readonly int number;

			public readonly ushort [] data;

			public IList index_entries = new ArrayList (2);
		}

		class IndexEntry
		{
			public IndexEntry (string key, int start, int end, Page page)
			{
				this.key = key;
				this.start = start;
				this.end = end;
				this.page = page;
			}

			public readonly string key;

			public readonly int start;

			public readonly int end;

			public readonly Page page;
		}

		class IndexEntriesComparer : IComparer
		{
			public int Compare (object x, object y)
			{
				return ((IndexEntry) x).start - ((IndexEntry) y).start;
			}
		}

		class Index
		{
			public Index (string name, IList entries)
			{
				this.name = name;
				this.entries = entries;
			}

			public readonly string name;

			public readonly IList entries;
		}

		public PagedTableEncoder (int page_bits,
					  int value_bits,
					  int index_bits,
					  bool flat_bmp,
					  string no_astral_symbol)
		{
			Debug.Assert (page_bits >= 4);
			Debug.Assert (value_bits == 8 || value_bits == 16);
			Debug.Assert (index_bits == 16 || index_bits == 32);

			this.page_size = 1 << page_bits;
			this.value_bits = value_bits;
			this.index_bits = index_bits;
			this.flat_bmp = flat_bmp;
			this.no_astral_symbol = no_astral_symbol;
		}

		public void Process (IData data, string index_name)
		{
			int end = data.Count;

			Debug.Assert (!flat_bmp || pages.Count == 0);
			Debug.Assert ((end & (page_size - 1)) == 0);

			IList entries = new ArrayList ();

			for (int page_base = 0; page_base < end; page_base += page_size) {
				ushort [] page_data = new ushort [page_size];

				for (int i = 0; i < page_size; i++) {
					ushort v = data [page_base + i];

					page_data[i] = v;
				}

				bool indexed = IsIndexed (page_base);
				Page page = GetPageForData (page_base, page_data, indexed);

				if (indexed) {
					IndexEntry index_entry = new IndexEntry (index_name, page_base,
										 page_base + page_size, page);
					page.AddIndexEntry (index_entry);
					entries.Add (index_entry);
				}
			}

			indices.Add (new Index (index_name, entries));
		}

		Page GetPageForData (int page_base, ushort [] data, bool indexed)
		{
			if (indexed) {
				// Are we in a hurry?
				foreach (Page page in pages) {
					if (page.Contains (data))
						return page;
				}
			}

			Page new_page = new Page (page_base, pages.Count, data);
			pages.Add (new_page);
			return new_page;
		}

		bool IsIndexed (int page_base)
		{
			return !flat_bmp || page_base > Char.MaxValue;
		}

		public void WriteDefinitions (Language lang, string name, TextWriter w)
		{
			WriteHeaderComment (w);
			WriteDataTable (lang, name, w);

			foreach (Index index in indices) {
				w.WriteLine ();
				WriteIndexTable (lang, index, name + '_' + index.name, w);
			}
		}

		void WriteHeaderComment (TextWriter w)
		{
			int packed_size = pages.Count * page_size * value_bits / 8;
			int total_size = packed_size;

			w.WriteLine ("/*");
			w.WriteLine (" * Value bits: {0}, Page size: {1}", value_bits, page_size);
			w.WriteLine (" * Packed table: {0} bytes", packed_size);

			foreach (Index index in indices) {
				int index_size = index.entries.Count * 2;

				w.WriteLine (" * Index {0}: {1} bytes", index.name, index_size);

				total_size += index_size;
			}

			w.WriteLine (" * Total: {0} bytes", total_size);
			w.WriteLine (" */");
		}

		public string CompoundKey (ArrayList keys)
		{
			string [] key_array = (string []) keys.ToArray (typeof (string));
			Array.Sort (key_array);
			return string.Join (", ", key_array);
		}

		public IList CollapseByIndex (IList index_entries)
		{
			if (index_entries.Count == 0)
				return index_entries;

			ArrayList entries = new ArrayList (index_entries);
			// The comparer is required for a stable sort.
			entries.Sort (new IndexEntriesComparer ());

			IndexEntry first = (IndexEntry) entries [0];
			ArrayList keys = new ArrayList ();
			keys.Add (first.key);
			int start = first.start;
			int end = first.end;
			Page page = first.page;
			IList collapsed = new ArrayList ();

			for (int i = 1; i < entries.Count; i ++) {
				IndexEntry ie = (IndexEntry) entries [i];

				if (ie.start == start && ie.end == end)
					keys.Add (ie.key);
				else {
					collapsed.Add (new IndexEntry (CompoundKey (keys), start, end, page));

					keys = new ArrayList ();
					keys.Add (ie.key);
					start = ie.start;
					end = ie.end;
					page = ie.page;
				}
			}

			collapsed.Add (new IndexEntry (CompoundKey (keys), start, end, page));
			return collapsed;
		}

		public IList CollapseByRange (IList entries)
		{
			if (entries.Count == 0)
				return entries;

			IndexEntry first = (IndexEntry) entries [0];
			string key = first.key;
			int start = first.start;
			int end = first.end;
			Page page = first.page;
			IList collapsed = new ArrayList ();

			for (int i = 1; i < entries.Count; i++) {
				IndexEntry ie = (IndexEntry) entries [i];

				if (ie.start == end && ie.key == key) {
					end = ie.end;
				} else {
					collapsed.Add (new IndexEntry (key, start, end, page));
					key = ie.key;
					start = ie.start;
					end = ie.end;
					page = ie.page;
				}
			}

			collapsed.Add (new IndexEntry (key, start, end, page));
			return collapsed;
		}

		public IList CollapseIndexEntries (IList index_entries)
		{
			return CollapseByRange (CollapseByIndex (index_entries));
		}

		void WriteDataTable (Language lang, string name, TextWriter w)
		{
			int n_entries = pages.Count * page_size;

			if (lang == Language.C)
				w.WriteLine ("static const guint{0} {1} [{2}] = ", value_bits, name, n_entries);
			else {
				string type = value_bits == 8 ? "byte" : "ushort";

				w.WriteLine ("static readonly {0} [] {1} = new {0} [{2}] ", type, name, n_entries);
			}

			string separator = TABLE_START;
			bool has_ifndef = false;
			foreach (Page page in pages) {
				has_ifndef |= MaybeWriteIfndef (page, ref separator, w);
				WritePageComment (page, ref separator, w);

				for (int i = 0; i < page_size; i += 16) {
					w.Write("{0}\t{1},{2},{3},{4},{5},{6},{7},{8}," +
						"{9},{10},{11},{12},{13},{14},{15},{16}",
						separator,
						page.data[i +  0], page.data[i +  1],
						page.data[i +  2], page.data[i +  3],
						page.data[i +  4], page.data[i +  5],
						page.data[i +  6], page.data[i +  7],
						page.data[i +  8], page.data[i +  9],
						page.data[i + 10], page.data[i + 11],
						page.data[i + 12], page.data[i + 13],
						page.data[i + 14], page.data[i + 15]);

					separator = TABLE_CONT;
				}
			}

			// Separator intentionally ignored.
			if (has_ifndef)
				w.Write ("{0}#endif", Environment.NewLine);
			w.WriteLine (TABLE_END);
		}

		bool MaybeWriteIfndef (Page page, ref string separator, TextWriter w)
		{
			if (no_astral_symbol == null || page.first_base != Char.MaxValue + 1)
				return false;

			w.WriteLine ("{0}#ifndef {1}", Environment.NewLine, no_astral_symbol);
			// Previous separator, but indented on the new line following the directive.
			separator = "\t" + separator;
			return true;
		}

		void WritePageComment (Page page, ref string separator, TextWriter w)
		{
			int uses = page.index_entries.Count;
			IList index_entries = CollapseIndexEntries (page.index_entries);

			if (uses == 0 || index_entries.Count == 1) {
				w.Write ("{0}\t/* Page {1}, {2} {3}use{4}",
					 separator, page.number, uses,
					 flat_bmp ? "indirect " : "",
					 uses != 1 ? "s" : "");

				if (index_entries.Count == 1) {
					IndexEntry ie = (IndexEntry) index_entries [0];

					w.WriteLine (": {0:X4}-{1:X4} ({2}) */", ie.start, ie.end - 1, ie.key);
				} else
					w.WriteLine (" */");
			} else {
				w.Write ("{0}\t/*{1}\t * Page {2}, {3} indirect use{4}",
					 separator, Environment.NewLine, page.number, uses, uses != 1 ? "s" : "");

				separator = ":" + Environment.NewLine + "\t *\t";
				string next_separator = "," + Environment.NewLine + "\t *\t";

				foreach (IndexEntry ie in index_entries) {
					w.Write ("{0}{1:X4}-{2:X4} ({3})", separator, ie.start, ie.end - 1, ie.key);
					separator = next_separator;
				}

				// Separator intentionally ignored.
				w.WriteLine (Environment.NewLine + "\t */");
			}

			separator = "";
		}

		void WriteIndexTable (Language lang, Index index, string name, TextWriter w)
		{
			bool ifndef_around = flat_bmp && no_astral_symbol != null;
			if (ifndef_around)
				w.WriteLine ("#ifndef {0}", no_astral_symbol);

			if (lang == Language.C)
				w.WriteLine ("static const guint{0} {1} [{2}] = ", index_bits, name, index.entries.Count);
			else {
				string type = value_bits == 16 ? "ushort" : "uint";

				w.WriteLine ("static readonly {0} [] {1} = new {0} [{2}] ", type, name, index.entries.Count);
			}

			string separator = TABLE_START;
			bool ifndef_inside = false;
			foreach (IndexEntry ie in index.entries) {
				int index_value = ie.page.number /* * page_size */;

				Debug.Assert (index_value < (1 << index_bits));

				if (!ifndef_around)
					ifndef_inside |= MaybeWriteIfndef (ie.page, ref separator, w);

				w.WriteLine ("{0}\t/* {1:X4}-{2:X4}: page {3} */",
					     separator, ie.start, ie.end - 1, ie.page.number);
				w.Write ("\t0x{0:X}", index_value);

				separator = TABLE_CONT;
			}

			// Separator intentionally ignored.
			if (ifndef_inside)
				w.Write ("{0}#endif", Environment.NewLine);
			w.WriteLine (TABLE_END);

			if (ifndef_around)
				w.WriteLine ("#endif");
		}

		readonly int page_size;

		readonly int value_bits;

		readonly int index_bits;

		readonly bool flat_bmp;

		readonly string no_astral_symbol;

		IList pages = new ArrayList ();

		IList indices = new ArrayList ();

		static readonly string TABLE_START = "{" + Environment.NewLine;
		static readonly string TABLE_CONT = "," + Environment.NewLine;
		static readonly string TABLE_END = Environment.NewLine + "};";
	}

	class CategoryTableGenerator {
		const int MaxCodePoint = 0x10ffff;

		public class HostUCData : PagedTableEncoder.IData
		{
			public ushort this [int index]
			{
				get {
					if (index <= 0xffff)
						return (ushort) Char.GetUnicodeCategory ((char) index);
					else {
						string s = Char.ConvertFromUtf32 (index);

						return (ushort) Char.GetUnicodeCategory (s, 0);
					}
				}
			}

			public int Count
			{
				get {
					return MaxCodePoint + 1;
				}
			}
		}

		public static void Dump (PagedTableEncoder.IData source, TextWriter w)
		{
			w.WriteLine ("{0}", source.Count);

			for (int cp = 0; cp <= MaxCodePoint; cp++) {
				byte cc = (byte) source [cp];

				if (cc != 0)
					w.WriteLine ("{0} {1}", cp, cc);
			}
		}

		public static PagedTableEncoder.IData ParseDump (TextReader r)
		{
			string line = r.ReadLine ();
			int count = int.Parse (line);
			byte [] data = new byte [count];

			while ((line = r.ReadLine ()) != null) {
				int n = line.IndexOf (' ');
				int cp = int.Parse (line.Substring (0, n));
				int cc = int.Parse (line.Substring (n + 1));

				if (cp < 0 || cp >= data.Length)
					throw new Exception (String.Format ("Invalid code point {0:X4}", cp));

				if (cc < 0 || cc > (int)UnicodeCategory.OtherNotAssigned)
					throw new Exception (String.Format ("Invalid category code {0}", cc));

				if (data [cp] != 0)
					throw new Exception (String.Format ("Duplicate code point {0:X4}", cp));

				data [cp] = (byte)cc;
			}

			return new PagedTableEncoder.ArrayData (data);
		}

		public static void Encode (string dump_file, string data_name, string h_file)
		{
			PagedTableEncoder.IData data;

			using (TextReader r = new StreamReader (dump_file))
				data = ParseDump (r);

			PagedTableEncoder pte = new PagedTableEncoder (8, 8, 16, true, "DISABLE_ASTRAL");
			pte.Process (data, "astral_index");

			using (TextWriter w = new StreamWriter (h_file)) {
				w.WriteLine ("/*");
				w.WriteLine (" * The {0}* tables below are automatically generated", data_name);
				w.WriteLine (" * by create-category-table(.cs), available in the mcs");
				w.WriteLine (" * sources.  DO NOT EDIT!");
				w.WriteLine (" */");
				w.WriteLine ();

				pte.WriteDefinitions (Language.C, data_name, w);
			}
		}

		public static void Main (string [] args)
		{
			for (int i = 0; i < args.Length; ) {
				if (args [i] == "--dump") {
					PagedTableEncoder.IData data = new HostUCData ();

					using (TextWriter w = new StreamWriter (args [i + 1]))
						Dump (data, w);

					i += 2;
				} else if (args [i] == "--encode") {
					Encode (args [i + 1], args [i + 2], args [i + 3]);

					i += 4;
				} else
					throw new Exception ("Unrecognized argument: " + args [i]);
			}
		}
	}
}
