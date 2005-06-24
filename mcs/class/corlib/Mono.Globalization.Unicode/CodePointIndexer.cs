using System;
using System.Globalization;
using System.Text;

namespace Mono.Globalization.Unicode
{
	internal class CodePointIndexer
	{
		public static Array CompressArray (
			Array source, Type type, CodePointIndexer indexer)
		{
			int totalCount = 0;
			for (int i = 0; i < indexer.ranges.Length; i++)
				totalCount += indexer.ranges [i].Count;

			Array ret = Array.CreateInstance (type, totalCount);
			for (int i = 0; i < indexer.ranges.Length; i++)
				Array.Copy (
					source,
					indexer.ranges [i].Start,
					ret,
					indexer.ranges [i].IndexStart,
					indexer.ranges [i].Count);
			return ret;
		}

		// This class is used to compactize indexes to limited areas so that
		// we can save extraneous 0,0,0,0,0... in the tables.
		internal class TableRange
		{
			public TableRange (int start, int end, int indexStart)
			{
				Start = start;
				End = end;
				Count = End - Start;
				IndexStart = indexStart;
				IndexEnd = IndexStart + Count;
			}

			public readonly int Start;
			public readonly int End;
			public readonly int Count;
			public readonly int IndexStart;
			public readonly int IndexEnd;
		}

		readonly TableRange [] ranges;

		public readonly int TotalCount;

		int defaultIndex;
		int defaultCP;

		public CodePointIndexer (int [] starts, int [] ends, int defaultIndex, int defaultCP)
		{
			this.defaultIndex = defaultIndex;
			this.defaultCP = defaultCP;
			ranges = new TableRange [starts.Length];
			for (int i = 0; i < ranges.Length; i++)
				ranges [i] = new TableRange (starts [i],
					ends [i], i == 0 ? 0 :
					ranges [i - 1].IndexStart +
					ranges [i - 1].Count);
			for (int i = 0; i < ranges.Length; i++)
				TotalCount += ranges [i].Count;

//			for (int i = 0; i < ranges.Length; i++)
//				Console.Error.WriteLine ("RANGES [{0}] : {1:x} to {2:x} index {3:x} to {4:x}. total {5:x}", i, ranges [i].Start, ranges [i].End, ranges [i].IndexStart, ranges [i].IndexEnd, ranges [i].Count);
//			Console.Error.WriteLine ("Total items: {0:X} ({1})", TotalCount, TotalCount);
		}

		public int ToIndex (int cp)
		{
			for (int t = 0; t < ranges.Length; t++)
				if (ranges [t].Start <= cp && cp < ranges [t].End)
					return cp - ranges [t].Start + ranges [t].IndexStart;
			return defaultIndex;
//			throw new SystemException (String.Format ("Should not happen: no map definition for cp {0:x}({1})", cp, (char) cp));
		}

		public int ToCodePoint (int i)
		{
			for (int t = 0; t < ranges.Length; t++) {
				if (t > 0 && i < ranges [t - 1].IndexEnd)
					return -1; // unexpected out of range
				if (ranges [t].IndexStart <= i &&
					i < ranges [t].IndexEnd)
					return i - ranges [t].IndexStart
						+ ranges [t].Start;
			}
			return defaultCP;
//			throw new SystemException (String.Format ("Should not happen: no map definition for index {0:x}({1})", i, i));
		}
	}
}
