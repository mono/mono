using System;
using System.Globalization;
using System.Text;

namespace Mono.Globalization.Unicode
{
	internal class CodePointIndexer
	{
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
#if DumpFullArray
		public const int TotalCount = char.MaxValue + 1;
#else
		public readonly int TotalCount;

		public CodePointIndexer (int [] starts, int [] ends)
		{
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
#endif

		public int GetIndexForCodePoint (int cp)
		{
#if DumpFullArray
			return cp;
#else
			for (int t = 0; t < ranges.Length; t++)
				if (ranges [t].Start <= cp && cp < ranges [t].End)
					return cp - ranges [t].Start + ranges [t].IndexStart;
			return 0;
//			return -1;
//			throw new SystemException (String.Format ("Should not happen: no map definition for cp {0:x}({1})", cp, (char) cp));
#endif
		}

		public int GetCodePointForIndex (int i)
		{
#if DumpFullArray
			return i;
#else
			for (int t = 0; t < ranges.Length; t++) {
				if (t > 0 && i < ranges [t - 1].IndexEnd)
					return -1; // unexpected out of range
				if (ranges [t].IndexStart <= i &&
					i < ranges [t].IndexEnd)
					return i - ranges [t].IndexStart
						+ ranges [t].Start;
			}
			return 0;
//			return -1;
//			throw new SystemException (String.Format ("Should not happen: no map definition for index {0:x}({1})", i, i));
#endif
		}
	}
}
