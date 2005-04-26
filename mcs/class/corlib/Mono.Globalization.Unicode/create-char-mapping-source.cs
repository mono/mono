//
// create-char-mapping-source.cs - creates canonical/compatibility mappings.
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;

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
		ArrayList widthSensitives = new ArrayList ();

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

		private void Serialize ()
		{
			// mappedChars
			Console.WriteLine ("static readonly int [] mappedChars = new int [] {");
			DumpArray (mappedChars, mappedCharCount, false);
			Console.WriteLine ("};");

			// mapIndex
			Console.WriteLine ("static readonly short [] mapIndex= new short [] {");
			DumpArray (mapIndex, NormalizationTableUtil.MapCount, true);
			Console.WriteLine ("};");

			// GetPrimaryCompositeHelperIndex ()
			Console.WriteLine ("static short GetPrimaryCompositeHelperIndex (int head)");
			Console.WriteLine ("{");
			int currentHead = 0;
			Console.WriteLine ("	switch (head) {");
			foreach (CharMapping m in mappings) {
				if (mappedChars [m.MapIndex] == currentHead)
					continue; // has the same head
// FIXME: should be applied
//				if (!m.IsCanonical)
//					continue;
				currentHead = mappedChars [m.MapIndex];
				Console.WriteLine ("	case 0x{0:X}: return 0x{1:X};", currentHead, m.MapIndex);
			}
			Console.WriteLine ("	}");
			Console.WriteLine ("	return 0;");
			Console.WriteLine ("}");

			// GetPrimaryCompositeFromMapIndex ()
			Console.WriteLine ("static int GetPrimaryCompositeFromMapIndex (int idx)");
			Console.WriteLine ("{");
			Console.WriteLine ("	switch (idx) {");
			int currentIndex = -1;
			foreach (CharMapping m in mappings) {
				if (m.MapIndex == currentIndex)
					continue;
				if (!m.IsCanonical)
					continue;
				Console.WriteLine ("	case 0x{0:X}: return 0x{1:X};", m.MapIndex, m.CodePoint);
				currentIndex = m.MapIndex;
			}
			Console.WriteLine ("	}");
			Console.WriteLine ("	return 0;");
			Console.WriteLine ("}");

			// WidthSensitives
			Console.WriteLine ("public static int ToWidthInsensitive (int i)");
			Console.WriteLine ("{");
			Console.WriteLine ("	if (i != 0x3000 && i < 0xFF00)");
			Console.WriteLine ("		return i;");
			Console.WriteLine ("	switch (i) {");
			foreach (int i in widthSensitives)
				Console.WriteLine ("	case 0x{0:X}:", i);
			Console.WriteLine ("		return mappedChars [NormalizationTableUtil.MapIdx (i)];");
			Console.WriteLine ("	}");
			Console.WriteLine ("	return i;");
			Console.WriteLine ("}");
		}

		private void DumpArray (int [] array, int count, bool getCP)
		{
			if (array.Length < count)
				throw new ArgumentOutOfRangeException ("count");
			for (int i = 0; i < count; i++) {
				if (array [i] == 0)
					Console.Write ("0, ");
				else
					Console.Write ("0x{0:X}, ", array [i]);
				if (i % 16 == 15) {
					int l = getCP ? NormalizationTableUtil.MapCP (i) : i;
					Console.WriteLine ("// {0:X04}-{1:X04}", l - 15, l);
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

				string [] values = line.Substring (n + 1).Split (';');
				string canon = values [4];
				string combiningCategory = canon.IndexOf ('>') < 0 ? "" : canon.Substring (1, canon.IndexOf ('>') - 1);
				string mappedCharsValue = canon;
				if (combiningCategory.Length > 0)
					mappedCharsValue = canon.Substring (combiningCategory.Length + 2).Trim ();
				if (mappedCharsValue.Length > 0) {
					switch (combiningCategory) {
					case "narrow":
					case "wide":
					case "super":
					case "sub":
						widthSensitives.Add (cp);
						break;
					}
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

