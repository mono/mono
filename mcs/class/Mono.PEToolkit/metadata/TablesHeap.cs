/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Collections;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// Metadata tables heap (#~).
	/// </summary>
	/// <remarks>
	/// Partition II; Chapter 21 & 23.1.6
	/// </remarks>
	public class TablesHeap : TablesHeapBase {

		private long valid;  // bitvector of valid tables
		                     //(64-bit, max index = TableId.MAX)
		private int numTabs; // number of tables (calculated from valid)

		private long sorted; // bitvector of sorted tables (64-bit)

		// schema version (currently 1.0)
		private byte verMaj;
		private byte verMin;

		// bitvector for heap-size flags:
		// bit 1 - if set #Strings heap uses wide indices (dword)
		// bit 2 - if set #GUID heap uses wide indices
		// bit 3 - if set #Blob heap uses wide indices
		// otherwise (particular bit is not set) index size is word.
		private byte heapSizes;


		private Hashtable tables;


		internal TablesHeap (MDStream stream) : base (stream)
		{
		}


		/// <summary>
		/// Gets or sets bitvector of valid tables (64-bit).
		/// </summary>
		public override long Valid {
			get {
				return valid;
			}
			set {
				valid = value;
			}
		}

		/// <summary>
		/// Gets or sets bitvector of sorted tables (64-bit).
		/// </summary>
		public override long Sorted {
			get {
				return sorted;
			}
			set {
				sorted = value;
			}
		}


		//
		// "Universal" accessors for Valid and Sorted bitvectors.
		//


		public bool IsValid (TableId tab)
		{
			return (valid & (1L << (int) tab)) != 0;
		}

		public void SetValid (TableId tab, bool b)
		{
			long mask = 1L << (int) tab;
			if (b) {
				valid |= mask;
			} else {
				valid &= ~mask;
			}
		}


		/// <summary>
		/// True if the given table in this heap is sorted.
		/// </summary>
		/// <param name="tab"></param>
		/// <returns></returns>
		public bool IsSorted (TableId tab)
		{
			return (sorted & (1L << (int) tab)) != 0;
		}

		/// <summary>
		/// Marks specified table in this heap as sorted or unsorted.
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="b"></param>
		public void SetSorted (TableId tab, bool b)
		{
			long mask = 1L << (int) tab;
			if (b) {
				sorted |= mask;
			} else {
				sorted &= ~mask;
			}
		}



		public byte HeapSizes {
			get {
				return heapSizes;
			}
			set {
				heapSizes = value;
			}
		}

		public int StringsIndexSize {
			get {
				return 2 + ((heapSizes & 1) << 1);
			}
		}

		public int GUIDIndexSize {
			get {
				return 2 + (heapSizes & 2);
			}
		}

		public int BlobIndexSize {
			get {
				return 2 + ((heapSizes & 4) >> 1);
			}
		}



		unsafe override public void FromRawData (byte [] rawData)
		{
			valid = 0;
			sorted = 0;

			if (rawData == null || rawData.Length < 24) {
				throw new BadMetaDataException ("Invalid header for #~ heap.");
			}

			verMaj = rawData [4];
			verMin = rawData [5];
			heapSizes = rawData [6];

			valid = LEBitConverter.ToInt64 (rawData, 8);
			sorted = LEBitConverter.ToInt64 (rawData, 16);

			// Calc number of tables from valid bitvector.
			numTabs = 0;
			for (int i = (int) TableId.Count; --i >= 0;) {
				numTabs += (int) (valid >> i) & 1;
			}

			int [] rows = new int [(int) TableId.Count];
			Array.Clear (rows, 0, rows.Length);
			int offs = 24; // offset to #~::Rows
			for (int i = 0; i < numTabs; i++) {
				int n = -1;
				int vpos = -1;
				long v = valid;
				while (n < i && v != 0) {
					n += (int) (v & 1L);
					v >>= 1;
					vpos++;
				}
				if (vpos != -1) {
					rows [vpos] = LEBitConverter.ToInt32 (rawData, offs);
					offs += sizeof (int);
				}
			}

			// TODO: this could be called from constructor
			// This sequence: MDHeap::.ctor -> FromRawData -> RegisterTable
			// and we are making "this" available here, before the object
			// is fully constructed. This is bad, fix it somehow.
			TabsDecoder.DecodePhysicalTables (this, rawData, offs, rows);

		}


		public void RegisterTable (MDTable tab)
		{
			if (tables == null) tables = new Hashtable (64);
			tables [tab.Id] = tab;
		}

		public MDTable this [TableId id] {
			get {
				return tables [id] as MDTable;
			}
		}

		public ICollection Tables {
			get {
				return tables.Values;
			}
		}


	}

}
