//
// create-normalization-source.cs - creates normalization information table.
//

using System;
using System.Globalization;
using System.IO;

namespace Mono.Globalization.Unicode
{
	internal class NormalizationCodeGenerator
	{
		private int lineCount = 0;
		int singleCount = 1, multiCount = 1, propValueCount = 1;
		int [] singleNorm = new int [550];
		int [] multiNorm = new int [280];
		int [] prop = new int [char.MaxValue]; // maybe it will be enough when we use CodePointIndexer
		int [] propValues = new int [1024];

		public const int NoNfd = 1;
		public const int NoNfkd = 2;
		public const int NoNfc = 4;
		public const int MaybeNfc = 8;
		public const int NoNfkc = 16;
		public const int MaybeNfkc = 32;
		public const int ExpandOnNfd = 64;
		public const int ExpandOnNfc = 128;
		public const int ExpandOnNfkd = 256;
		public const int ExpandOnNfkc = 512;
		public const int FullCompositionExclusion = 1024;

		public static void Main ()
		{
			new NormalizationCodeGenerator ().Run ();
		}

		private void Run ()
		{
			try {
				Parse ();
				MakeIndex ();
				Serialize ();
			} catch (Exception ex) {
				throw new InvalidOperationException ("Internal error at line " + lineCount + " : " + ex);
			}
		}

		private void MakeIndex ()
		{
			for (int i = 0; i < prop.Length; i++) {
				bool add = true;
				for (int v = 0; v < propValueCount; v++)
					if (propValues [v] == prop [i]) {
						prop [i] = v;
						add = false;
						break;
					}
				if (!add)
					continue;
				if (propValueCount == propValues.Length) {
					int [] tmp = new int [propValueCount * 2];
					Array.Copy (propValues, tmp, propValueCount);
					propValues = tmp;
				}
				propValues [propValueCount] = prop [i];
				prop [i] = propValueCount++;
			}
		}

		private void Serialize ()
		{
			Console.WriteLine ("static readonly int [] singleNorm = new int [] {");
			DumpArray (singleNorm, singleCount, false);
			Console.WriteLine ("};");
			Console.WriteLine ("static readonly int [] multiNorm = new int [] {");
			DumpArray (multiNorm, multiCount, false);
			Console.WriteLine ("};");
			Console.WriteLine ("static readonly byte [] propIdx = new byte [] {");
			DumpArray (prop, NormalizationTableUtil.PropCount, true);
			Console.WriteLine ("};");
			Console.WriteLine ("static readonly uint [] propValue = new uint [] {");
			DumpArray (propValues, propValueCount, false);
			Console.WriteLine ("};");
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
					int l = getCP ? NormalizationTableUtil.PropCP (i) : i;
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
					SetProp (cp, cpEnd, NoNfd);
					break;
				case "NFC_QC":
					SetProp (cp, cpEnd, (values == "M") ?
						MaybeNfc :NoNfc);
					break;
				case "NFKD_QC":
					SetProp (cp, cpEnd, NoNfkd);
					break;
				case "NFKC_QC":
					SetProp (cp, cpEnd, (values == "M") ?
						MaybeNfkc :NoNfkc);
					break;
				case "Expands_On_NFD":
					SetProp (cp, cpEnd, ExpandOnNfd);
					break;
				case "Expands_On_NFC":
					SetProp (cp, cpEnd, ExpandOnNfc);
					break;
				case "Expands_On_NFKD":
					SetProp (cp, cpEnd, ExpandOnNfkd);
					break;
				case "Expands_On_NFKC":
					SetProp (cp, cpEnd, ExpandOnNfkc);
					break;
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
				}
			}
			reader.Close ();
		}

		private void SetProp (int cp, int cpEnd, int flag)
		{
			int idx = NormalizationTableUtil.PropIdx (cp);
			if (cpEnd < 0)
				prop [idx] |= flag;
			else {
				int idxEnd = NormalizationTableUtil.PropIdx (cpEnd);
				for (int i = idx; i <= idxEnd; i++)
					prop [i] |= flag;
			}
		}

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
	}
}

