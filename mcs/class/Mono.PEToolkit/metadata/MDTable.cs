/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit.Metadata {

	public interface MDTable {
		/// <summary>
		/// Number of rows in the table.
		/// </summary>
		int NumberOfRows {get;}

		/// <summary>
		/// Gets ot sets a row in the metadata table.
		/// </summary>
		Row this [int rowNum] {get; set;}

		void Add(Row row);

		void FromRawData(byte [] buff, int offs, int numRows);

		string Name {get;}

		TableId Id {get;}

		MDHeap Heap {get;}

		void Dump(TextWriter writer);
	}


	public abstract class MDTableBase : MDTable {
		protected ArrayList rows; // rows storage
		protected MDHeap heap;    // base heap

		public MDTableBase(MDHeap heap)
		{
			rows = new ArrayList();
			this.heap = heap;

			if (heap is TablesHeap) {
				(heap as TablesHeap).RegisterTable(this);
			}
		}

		public virtual int NumberOfRows {
			get {
				return rows.Count;
			}
		}


		public virtual Row this [int rowNum] {
			get {
				if (rowNum < 0) throw new IndexOutOfRangeException("Row[]");

				// Zero row, special case
				if (rowNum == 0) return NullRow.Instance;
				return rows [rowNum - 1] as Row;
			}
			set {
				rows.Insert(rowNum, value);
			}
		}

		public virtual void Add(Row row)
		{
			rows.Add(row);
		}

		public abstract void FromRawData(byte [] buff, int offs, int numRows);

		public abstract string Name {get;}

		public abstract TableId Id {get;}

		public virtual MDHeap Heap {
			get {
				return heap;
			}
		}

		public virtual void Dump(TextWriter writer)
		{
			writer.WriteLine("=========================================");
			writer.WriteLine("Table '{0}', id = {1} (0x{2}), rows = {3}",
				Name, Id, ((int) Id).ToString("X"), NumberOfRows);
			int n = 1;
			foreach (Row row in rows) {
				writer.WriteLine();
				writer.WriteLine("Row #{0}", n++);
				writer.WriteLine("-------------");
				row.Dump(writer);
				writer.WriteLine();
			}
		}

	}

}
