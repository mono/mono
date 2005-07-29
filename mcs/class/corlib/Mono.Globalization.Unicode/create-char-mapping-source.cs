//
// create-char-mapping-source.cs - creates canonical/compatibility mappings.
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;

using Util = Mono.Globalization.Unicode.NormalizationTableUtil;

namespace Mono.Globalization.Unicode
{
	internal class CharMappingGenerator
	{
		class CharMapping
		{
			public CharMapping (int cp, int mapIndex, bool isCanonical)
			{
				MapIndex = mapIndex;
				CodePoint = cp;
				IsCanonical = isCanonical;
			}

			public int MapIndex;
			public readonly int CodePoint;
			public readonly bool IsCanonical;
		}

		class CharMappingComparer : IComparer
		{
			CharMappingGenerator parent;

			public CharMappingComparer (CharMappingGenerator g)
			{
				parent = g;
			}

			// Note that this never considers IsCanonical
			public int Compare (object o1, object o2)
			{
				CharMapping c1 = (CharMapping) o1;
				CharMapping c2 = (CharMapping) o2;
				return CompareArray (c1.MapIndex, c2.MapIndex);
			}

			// Note that this never considers IsCanonical
			public int CompareArray (int idx1, int idx2)
			{
				for (int i = 0; parent.mappedChars [idx2 + i] != 0; i++) {
					int l = parent.mappedChars [idx1 + i];
					int r = parent.mappedChars [idx2 + i];
					if (l != r)
						return l - r;
				}
				return 0;
			}
		}

		CharMappingComparer comparer;

		private int lineCount = 0;
		int mappedCharCount = 1;
		int [] mappedChars = new int [100];
		int [] mapIndex = new int [0x5000];

		ArrayList mappings = new ArrayList ();

		public CharMappingGenerator ()
		{
			comparer = new CharMappingComparer (this);
		}

		public static void Main ()
		{
			new CharMappingGenerator ().Run ();
		}

		private void Run ()
		{
			try {
				Parse ();
				Compress ();
				Serialize ();
			} catch (Exception ex) {
				throw new InvalidOperationException ("Internal error at line " + lineCount + " : " + ex);
			}
		}

		private void Compress ()
		{
			mappings.Sort (comparer);

			// mappedChars[0] = 0. This assures that value 0 of
			// mapIndex means there is no mapping.
			int count = 1;
			int [] compressedMapping = new int [mappedCharCount];
			// Update map index.
			int [] newMapIndex = new int [mappings.Count];
			for (int mi = 0; mi < mappings.Count; mi++) {
				CharMapping m = (CharMapping) mappings [mi];
				if (mi > 0 && 0 == comparer.Compare (
					mappings [mi - 1], mappings [mi])) {
					newMapIndex [mi] = newMapIndex [mi - 1];
					continue;
				}
				newMapIndex [mi] = count;
				for (int i = m.MapIndex; mappedChars [i] != 0; i++)
					compressedMapping [count++] = mappedChars [i];
				compressedMapping [count++] = 0;
			}
			for (int mi = 0; mi < mappings.Count; mi++)
				((CharMapping) mappings [mi]).MapIndex = newMapIndex [mi];

			int [] compressedMapIndex = new int [mapIndex.Length];
			foreach (CharMapping m in mappings)
				if (m.CodePoint <= char.MaxValue)
					compressedMapIndex [MapIdx (m.CodePoint)] = m.MapIndex;

			mappedChars = compressedMapping;
			mapIndex = compressedMapIndex;
			mappedCharCount = count;
		}

		TextWriter CSOut = Console.Out;
		TextWriter CSTableOut = Console.Out;
		TextWriter COut = TextWriter.Null;

		private void Serialize ()
		{
			COut = new StreamWriter ("normalization-tables.h", true);

			// mappedChars
			COut.WriteLine ("static const guint32 mappedChars [] = {");
			CSOut.WriteLine ("static readonly int [] mappedChars = new int [] {");
			DumpArray (mappedChars, mappedCharCount, false);
			COut.WriteLine ("0};");
			CSOut.WriteLine ("};");

			// mapIndex
			COut.WriteLine ("static const guint16 mapIndex [] = {");
			CSOut.WriteLine ("static readonly short [] mapIndex= new short [] {");
			DumpArray (mapIndex, NormalizationTableUtil.MapCount, true);
			COut.WriteLine ("0};");
			CSOut.WriteLine ("};");

			short [] helperIndexes = new short [0x30000];

			// GetPrimaryCompositeHelperIndex ()
			int currentHead = 0;
			foreach (CharMapping m in mappings) {
				if (mappedChars [m.MapIndex] == currentHead)
					continue; // has the same head
// FIXME: should be applied
//				if (!m.IsCanonical)
//					continue;
				currentHead = mappedChars [m.MapIndex];
				helperIndexes [currentHead] = (short) m.MapIndex;
			}

			helperIndexes = CodePointIndexer.CompressArray (
				helperIndexes, typeof (short), Util.Helper)
				as short [];

			COut.WriteLine ("static const guint16 helperIndexes = {");
			CSTableOut.WriteLine ("static short [] helperIndexes = new short [] {");
			for (int i = 0; i < helperIndexes.Length; i++) {
				short value = helperIndexes [i];
				if (value < 10)
					CSTableOut.Write ("{0},", value);
				else
					CSTableOut.Write ("0x{0:X04},", value);
				COut.Write ("{0},", value);
				if (i % 16 == 15) {
					CSTableOut.WriteLine (" // {0:X04}", Util.Helper.ToCodePoint (i - 15));
					COut.WriteLine ();
				}
			}
			COut.WriteLine ("0};");
			CSTableOut.WriteLine ("};");

			ushort [] mapIndexes = new ushort [0x2600];

			// GetPrimaryCompositeFromMapIndex ()
			int currentIndex = -1;
			foreach (CharMapping m in mappings) {
				if (m.MapIndex == currentIndex)
					continue;
				if (!m.IsCanonical)
					continue;
				mapIndexes [m.MapIndex] = (ushort) m.CodePoint;
				currentIndex = m.MapIndex;
			}

			mapIndexes = CodePointIndexer.CompressArray (mapIndexes, typeof (ushort), Util.MapIndexes) as ushort [];

			COut.WriteLine ("static const guint16 mapIndexes [] = {");
			CSTableOut.WriteLine ("static ushort [] mapIndexes = new ushort [] {");
			for (int i = 0; i < mapIndexes.Length; i++) {
				ushort value = (ushort) mapIndexes [i];
				if (value < 10)
					CSTableOut.Write ("{0},", value);
				else
					CSTableOut.Write ("0x{0:X04},", value);
				COut.Write ("{0},", value);
				if (i % 16 == 15) {
					CSTableOut.WriteLine (" // {0:X04}", Util.MapIndexes.ToCodePoint (i - 15));
					COut.WriteLine ();
				}
			}
			COut.WriteLine ("0};");
			CSTableOut.WriteLine ("};");

			COut.Close ();
		}

		private void DumpArray (int [] array, int count, bool getCP)
		{
			if (array.Length < count)
				throw new ArgumentOutOfRangeException ("count");
			for (int i = 0; i < count; i++) {
				int value = array [i];
				if (value < 10)
					CSOut.Write ("{0}, ", value);
				else
					CSOut.Write ("0x{0:X}, ", value);
				COut.Write ("{0},", value);
				if (i % 16 == 15) {
					int l = getCP ? NormalizationTableUtil.MapCP (i) : i;
					CSOut.WriteLine ("// {0:X04}-{1:X04}", l - 15, l);
					COut.WriteLine ();
				}
			}
		}

		private void Parse ()
		{
			TextReader reader = Console.In;
			while (reader.Peek () != -1) {
				string line = reader.ReadLine ();
				lineCount++;
				int idx = line.IndexOf ('#');
				if (idx >= 0)
					line = line.Substring (0, idx);
				if (line.Length == 0)
					continue;
				int n = 0;
				while (Char.IsDigit (line [n]) || Char.IsLetter (line [n]))
					n++;
				int cp = int.Parse (line.Substring (0, n), NumberStyles.HexNumber);
				// Windows does not handle surrogate characters.
				if (cp >= 0x10000)
					continue;

				string [] values = line.Substring (n + 1).Split (';');
				string canon = values [4];
//if (values [2] != "0") Console.Error.WriteLine ("----- {0:X03} : {1:x}", int.Parse (values [2]), cp);
				string combiningCategory = canon.IndexOf ('>') < 0 ? "" : canon.Substring (1, canon.IndexOf ('>') - 1);
				string mappedCharsValue = canon;
				if (combiningCategory.Length > 0)
					mappedCharsValue = canon.Substring (combiningCategory.Length + 2).Trim ();
				if (mappedCharsValue.Length > 0) {
					mappings.Add (new CharMapping (cp,
						mappedCharCount, 
						combiningCategory.Length == 0));
					SetCanonProp (cp, -1, mappedCharCount);
					foreach (string v in mappedCharsValue.Split (' '))
						AddMappedChars (cp,
							int.Parse (v, NumberStyles.HexNumber));
					AddMappedChars (cp, 0);
				}
			}
			if (reader != Console.In)
				reader.Close ();
		}

		private void AddMappedChars (int cp, int cv)
		{
			if (mappedCharCount == mappedChars.Length) {
				int [] tmp = new int [mappedCharCount * 2];
				Array.Copy (mappedChars, tmp, mappedCharCount);
				mappedChars = tmp;
			}
			mappedChars [mappedCharCount++] = cv;
		}

		private void SetCanonProp (int cp, int cpEnd, int flag)
		{
			int idx = MapIdx (cp);
			if (cpEnd < 0)
				mapIndex [idx] = flag;
			else {
				int idxEnd = MapIdx (cpEnd);
				for (int i = idx; i <= idxEnd; i++)
					mapIndex [i] = flag;
			}
		}

		private int MapIdx (int cp)
		{
			return NormalizationTableUtil.MapIdx (cp);
		}
	}
}

