//
// create-normalization-source.cs : creates normalization information table.
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;

using NUtil = Mono.Globalization.Unicode.NormalizationTableUtil;

namespace Mono.Globalization.Unicode
{
	internal class NormalizationCodeGenerator
	{
		private int lineCount = 0;
		int singleCount = 1, multiCount = 1, propValueCount = 1;
//		int [] singleNorm = new int [550];
//		int [] multiNorm = new int [280];
		int [] prop = new int [char.MaxValue + 1];

		public const int NoNfd = 1;
		public const int NoNfkd = 2;
		public const int NoNfc = 4;
		public const int MaybeNfc = 8;
		public const int NoNfkc = 16;
		public const int MaybeNfkc = 32;
		public const int FullCompositionExclusion = 64;
		public const int IsUnsafe = 128;
//		public const int ExpandOnNfd = 256;
//		public const int ExpandOnNfc = 512;
//		public const int ExpandOnNfkd = 1024;
//		public const int ExpandOnNfkc = 2048;

		CharMappingComparer comparer;

		int mappedCharCount = 1;
		int [] mappedChars = new int [100];
		int [] mapIndex = new int [char.MaxValue + 1];

		ArrayList mappings = new ArrayList ();

		byte [] combining = new byte [0x20000];


		public static void Main ()
		{
			new NormalizationCodeGenerator ().Run ();
		}

		private void Run ()
		{
			comparer = new CharMappingComparer (this);
			try {
				Parse ();
			} catch (Exception ex) {
				throw new InvalidOperationException ("Internal error at line " + lineCount + " : " + ex);
			}
			RebaseUCD ();
			Serialize ();
			ProcessCombiningClass ();
		}

		TextWriter CSOut = Console.Out;
		TextWriter COut = TextWriter.Null;

		private void Serialize ()
		{
			SerializeNormalizationProps ();
			SerializeUCD ();
		}

		private void SerializeUCD ()
		{
			COut = new StreamWriter ("normalization-tables.h", true);

			// mappedChars
			COut.WriteLine ("static const guint32 mappedChars [] = {");
			CSOut.WriteLine ("static readonly int [] mappedCharsArr = new int [] {");
			DumpMapArray (mappedChars, mappedCharCount, false);
			COut.WriteLine ("0};");
			CSOut.WriteLine ("};");

			// charMapIndex
			COut.WriteLine ("static const guint16 charMapIndex [] = {");
			CSOut.WriteLine ("static readonly short [] charMapIndexArr = new short [] {");
			DumpMapArray (mapIndex, NUtil.MapCount, true);
			COut.WriteLine ("0};");
			CSOut.WriteLine ("};");

			short [] helperIndexes = new short [0x30000];

			// GetPrimaryCompositeHelperIndex ()
			int currentHead = 0;
			foreach (CharMapping m in mappings) {
				if (mappedChars [m.MapIndex] == currentHead)
					continue; // has the same head
				if (!m.IsCanonical)
					continue;
				currentHead = mappedChars [m.MapIndex];
				helperIndexes [currentHead] = (short) m.MapIndex;
			}

			helperIndexes = CodePointIndexer.CompressArray (
				helperIndexes, typeof (short), NUtil.Helper)
				as short [];

			COut.WriteLine ("static const guint16 helperIndex [] = {");
			CSOut.WriteLine ("static short [] helperIndexArr = new short [] {");
			for (int i = 0; i < helperIndexes.Length; i++) {
				short value = helperIndexes [i];
				if (value < 10)
					CSOut.Write ("{0},", value);
				else
					CSOut.Write ("0x{0:X04},", value);
				COut.Write ("{0},", value);
				if (i % 16 == 15) {
					CSOut.WriteLine (" // {0:X04}", NUtil.Helper.ToCodePoint (i - 15));
					COut.WriteLine ();
				}
			}
			COut.WriteLine ("0};");
			CSOut.WriteLine ("};");

			ushort [] mapIndexes = new ushort [char.MaxValue + 1];

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

			mapIndexes = CodePointIndexer.CompressArray (mapIndexes, typeof (ushort), NUtil.Composite) as ushort [];

			COut.WriteLine ("static const guint16 mapIdxToComposite [] = {");
			CSOut.WriteLine ("static ushort [] mapIdxToCompositeArr = new ushort [] {");
			for (int i = 0; i < mapIndexes.Length; i++) {
				ushort value = (ushort) mapIndexes [i];
				if (value < 10)
					CSOut.Write ("{0},", value);
				else
					CSOut.Write ("0x{0:X04},", value);
				COut.Write ("{0},", value);
				if (i % 16 == 15) {
					CSOut.WriteLine (" // {0:X04}", NUtil.Composite.ToCodePoint (i - 15));
					COut.WriteLine ();
				}
			}
			COut.WriteLine ("0};");
			CSOut.WriteLine ("};");

			COut.Close ();
		}

		private void DumpMapArray (int [] array, int count, bool getCP)
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
					int l = getCP ? NUtil.MapCP (i) : i;
					CSOut.WriteLine ("// {0:X04}-{1:X04}", l - 15, l);
					COut.WriteLine ();
				}
			}
		}

		private void SerializeNormalizationProps ()
		{
			COut = new StreamWriter ("normalization-tables.h", false);

			/*
			CSOut.WriteLine ("static readonly int [] singleNorm = new int [] {");
			DumpArray (singleNorm, singleCount, false);
			CSOut.WriteLine ("};");
			CSOut.WriteLine ("static readonly int [] multiNorm = new int [] {");
			DumpArray (multiNorm, multiCount, false);
			CSOut.WriteLine ("};");
			*/
			CSOut.WriteLine ("static readonly byte [] propsArr = new byte [] {");
			COut.WriteLine ("static const guint8 props [] = {");
			DumpPropArray (prop, NUtil.PropCount, true);
			CSOut.WriteLine ("};");
			COut.WriteLine ("0};");

			COut.Close ();
		}

		private void DumpPropArray (int [] array, int count, bool getCP)
		{
			if (array.Length < count)
				throw new ArgumentOutOfRangeException ("count");
			for (int i = 0; i < count; i++) {
				uint value = (uint) array [i];
				if (value < 10)
					CSOut.Write ("{0}, ", value);
				else
					CSOut.Write ("0x{0:X}, ", value);
				COut.Write ("{0},", value);
				if (i % 16 == 15) {
					int l = getCP ? NUtil.PropCP (i) : i;
					CSOut.WriteLine ("// {0:X04}-{1:X04}", l - 15, l);
					COut.WriteLine ();
				}
			}
		}

		private void RebaseUCD ()
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
					compressedMapIndex [NUtil.MapIdx (m.CodePoint)] = m.MapIndex;

			mappedChars = compressedMapping;
			mapIndex = compressedMapIndex;
			mappedCharCount = count;
		}

		private void Parse ()
		{
			ParseNormalizationProps ();
			ParseUCD ();
		}
		
		private void ParseUCD ()
		{
			lineCount = 0;
			TextReader reader = new StreamReader ("downloaded/UnicodeData.txt");
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
				string combiningCategory = canon.IndexOf ('>') < 0 ? "" : canon.Substring (1, canon.IndexOf ('>') - 1);
				string mappedCharsValue = canon;
				if (combiningCategory.Length > 0)
					mappedCharsValue = canon.Substring (combiningCategory.Length + 2).Trim ();
				if (mappedCharsValue.Length > 0) {
					int start = mappedCharCount;
					mappings.Add (new CharMapping (cp,
						mappedCharCount, 
						combiningCategory.Length == 0));
					SetCanonProp (cp, -1, mappedCharCount);
					foreach (string v in mappedCharsValue.Split (' '))
						AddMappedChars (cp,
							int.Parse (v, NumberStyles.HexNumber));
					AddMappedChars (cp, 0);
					// For canonical composite, set IsUnsafe
					if (combiningCategory == "") {
						for (int ca = start; ca < mappedCharCount - 1; ca++)
							FillUnsafe (mappedChars [ca]);
					}
				}
			}
			if (reader != Console.In)
				reader.Close ();
		}

		private void FillUnsafe (int i)
		{
			if (i < 0 || i > char.MaxValue)
				return;
			if (0x3400 <= i && i <= 0x9FBB)
				return;
			SetProp (i, -1, IsUnsafe);
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
			int idx = NUtil.MapIdx (cp);
			if (cpEnd < 0)
				mapIndex [idx] = flag;
			else {
				int idxEnd = NUtil.MapIdx (cpEnd);
				for (int i = idx; i <= idxEnd; i++)
					mapIndex [i] = flag;
			}
		}

		private void ParseNormalizationProps ()
		{
			lineCount = 0;
			TextReader reader = new StreamReader ("downloaded/DerivedNormalizationProps.txt");
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

				int cpEnd = -1;
				if (line [n] == '.' && line [n + 1] == '.')
					cpEnd = int.Parse (line.Substring (n + 2, n), NumberStyles.HexNumber);
				int nameStart = line.IndexOf (';') + 1;
				int valueStart = line.IndexOf (';', nameStart) + 1;
				string name = valueStart == 0 ? line.Substring (nameStart) :
					line.Substring (nameStart, valueStart - nameStart - 1);
				name = name.Trim ();
				string values = valueStart > 0 ?
					line.Substring (valueStart).Trim () : "";
				switch (name) {
				case "Full_Composition_Exclusion":
					SetProp (cp, cpEnd, FullCompositionExclusion);
					break;
				case "NFD_QC":
					if (cp != 0xAC00) // Hangul Syllables are computed
						SetProp (cp, cpEnd, NoNfd);
					break;
				case "NFC_QC":
					SetProp (cp, cpEnd, (values == "M") ?
						MaybeNfc :NoNfc);
					break;
				case "NFKD_QC":
					if (cp != 0xAC00) // Hangul Syllables are computed
						SetProp (cp, cpEnd, NoNfkd);
					break;
				case "NFKC_QC":
					SetProp (cp, cpEnd, (values == "M") ?
						MaybeNfkc :NoNfkc);
					break;
				/*
				case "Expands_On_NFD":
					if (cp != 0xAC00) // Hangul Syllables are computed
						SetProp (cp, cpEnd, ExpandOnNfd);
					break;
				case "Expands_On_NFC":
					SetProp (cp, cpEnd, ExpandOnNfc);
					break;
				case "Expands_On_NFKD":
					if (cp != 0xAC00) // Hangul Syllables are computed
						SetProp (cp, cpEnd, ExpandOnNfkd);
					break;
				case "Expands_On_NFKC":
					SetProp (cp, cpEnd, ExpandOnNfkc);
					break;
				*/
				/*
				case "FC_NFKC":
					int v1 = 0, v2 = 0, v3 = 0, v4 = 0;
					foreach (string s in values.Split (' ')) {
						if (s.Trim ().Length == 0)
							continue;
						int v = int.Parse (s, NumberStyles.HexNumber);
						if (v1 == 0)
							v1 = v;
						else if (v2 == 0)
							v2 = v;
						else if (v3 == 0)
							v3 = v;
						else if (v4 == 0)
							v4 = v;
						else
							throw new NotSupportedException (String.Format ("more than 4 values in FC_NFKC: {0:x}", cp));
					}
					SetNFKC (cp, cpEnd, v1, v2, v3, v4);
					break;
				*/
				}
			}
			reader.Close ();
		}

		private void SetProp (int cp, int cpEnd, int flag)
		{
			int idx = NUtil.PropIdx (cp);
			if (idx == 0)
				throw new Exception (String.Format ("Codepoint {0:X04} should be included in the indexer.", cp));
			if (cpEnd < 0)
				prop [idx] |= flag;
			else {
				int idxEnd = NUtil.PropIdx (cpEnd);
				for (int i = idx; i <= idxEnd; i++)
					prop [i] |= flag;
			}
		}

		/*
		private void SetNFKC (int cp, int cpEnd, int v1, int v2, int v3, int v4)
		{
			if (v2 == 0) {
				int idx = -1;
				for (int i = 0; i < singleCount; i++)
					if (singleNorm [i] == v1) {
						idx = i;
						break;
					}
				if (idx < 0) {
					if (singleNorm.Length == singleCount) {
						int [] tmp = new int [singleCount << 1];
						Array.Copy (singleNorm, tmp, singleCount);
						singleNorm = tmp;
						idx = singleCount;
					}
					singleNorm [singleCount++] = v1;
				}
				SetProp (cp, cpEnd, idx << 16);
			} else {
				if (multiNorm.Length == multiCount) {
					int [] tmp = new int [multiCount << 1];
					Array.Copy (multiNorm, tmp, multiCount);
					multiNorm = tmp;
				}
				SetProp (cp, cpEnd,
					(int) ((multiCount << 16) | 0xF0000000));
				multiNorm [multiCount++] = v1;
				multiNorm [multiCount++] = v2;
				multiNorm [multiCount++] = v3;
				multiNorm [multiCount++] = v4;
			}
		}
		*/

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
			NormalizationCodeGenerator parent;

			public CharMappingComparer (NormalizationCodeGenerator g)
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
				for (int i = 0; ; i++) {
					int l = parent.mappedChars [idx1 + i];
					int r = parent.mappedChars [idx2 + i];
					if (l != r)
						return l - r;
					if (l == 0)
						return 0;
				}
			}
		}

		private void ProcessCombiningClass ()
		{
			TextReader reader = new StreamReader ("downloaded/DerivedCombiningClass.txt");
			while (reader.Peek () != -1) {
				string line = reader.ReadLine ();
				lineCount++;
				int idx = line.IndexOf ('#');
				if (idx >= 0)
					line = line.Substring (0, idx).Trim ();
				if (line.Length == 0)
					continue;
				int n = 0;
				while (Char.IsDigit (line [n]) || Char.IsLetter (line [n]))
					n++;
				int cp = int.Parse (line.Substring (0, n), NumberStyles.HexNumber);
				// Windows does not handle surrogate characters.
				if (cp >= 0x10000)
					continue;

				int cpEnd = -1;
				if (line [n] == '.' && line [n + 1] == '.')
					cpEnd = int.Parse (line.Substring (n + 2, n), NumberStyles.HexNumber);
				int nameStart = line.IndexOf (';') + 1;
				int valueStart = line.IndexOf (';', nameStart) + 1;
				string val = valueStart == 0 ? line.Substring (nameStart) :
					line.Substring (nameStart, valueStart - nameStart - 1);
				SetCombiningProp (cp, cpEnd, short.Parse (val));
			}

			reader.Close ();

			byte [] ret = (byte []) CodePointIndexer.CompressArray (
				combining, typeof (byte), NUtil.Combining);

			COut = new StreamWriter ("normalization-tables.h", true);

			COut.WriteLine ("static const guint8 combiningClass [] = {");
			CSOut.WriteLine ("public static byte [] combiningClassArr = new byte [] {");
			for (int i = 0; i < ret.Length; i++) {
				byte value = ret [i];
				if (value < 10)
					CSOut.Write ("{0},", value);
				else
					CSOut.Write ("0x{0:X02},", value);
				COut.Write ("{0},", value);
				if (i % 16 == 15) {
					CSOut.WriteLine (" // {0:X04}", NUtil.Combining.ToCodePoint (i - 15));
					COut.WriteLine ();
				}
			}
			CSOut.WriteLine ("};");
			COut.WriteLine ("0};");

			COut.Close ();
		}

		private void SetCombiningProp (int cp, int cpEnd, short val)
		{
			if (val == 0)
				return;
			if (cpEnd < 0)
				combining [cp] = (byte) val;
			else
				for (int i = cp; i <= cpEnd; i++)
					combining [i] = (byte) val;
		}
	}
}

