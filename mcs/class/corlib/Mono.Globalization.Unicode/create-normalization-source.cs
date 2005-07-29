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
using System.Globalization;
using System.IO;

namespace Mono.Globalization.Unicode
{
	internal class NormalizationCodeGenerator
	{
		private int lineCount = 0;
		int singleCount = 1, multiCount = 1, propValueCount = 1;
//		int [] singleNorm = new int [550];
//		int [] multiNorm = new int [280];
		int [] prop = new int [char.MaxValue];

		public const int NoNfd = 1;
		public const int NoNfkd = 2;
		public const int NoNfc = 4;
		public const int MaybeNfc = 8;
		public const int NoNfkc = 16;
		public const int MaybeNfkc = 32;
		public const int FullCompositionExclusion = 64;
//		public const int ExpandOnNfd = 128;
//		public const int ExpandOnNfc = 256;
//		public const int ExpandOnNfkd = 512;
//		public const int ExpandOnNfkc = 1024;

		public static void Main ()
		{
			new NormalizationCodeGenerator ().Run ();
		}

		private void Run ()
		{
			try {
				Parse ();
				Serialize ();
			} catch (Exception ex) {
				throw new InvalidOperationException ("Internal error at line " + lineCount + " : " + ex);
			}
		}

		TextWriter CSOut = Console.Out;
		TextWriter COut = TextWriter.Null;

		private void Serialize ()
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
			CSOut.WriteLine ("static readonly byte [] props = new byte [] {");
			COut.WriteLine ("static const guint8 props [] = {");
			DumpArray (prop, NormalizationTableUtil.PropCount, true);
			CSOut.WriteLine ("};");
			COut.WriteLine ("0};");

			COut.Close ();
		}

		private void DumpArray (int [] array, int count, bool getCP)
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
					int l = getCP ? NormalizationTableUtil.PropCP (i) : i;
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
			int idx = NormalizationTableUtil.PropIdx (cp);
			if (cpEnd < 0)
				prop [idx] |= flag;
			else {
				int idxEnd = NormalizationTableUtil.PropIdx (cpEnd);
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
	}
}

