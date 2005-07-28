//
// create-collation-element-table.cs :
//	supports creation of CollationElementTable.cs
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
	internal class CollationElementTableCodeGenerator
	{
		private int lineCount = 0;

		int keyCount = 1; // 0 indicates that there is no matching entry
		int [] collElem = new int [CollationElementTableUtil.Indexer.TotalCount];
		SortKeyValue [] keyValues = new SortKeyValue [32768];

		public static void Main ()
		{
			new CollationElementTableCodeGenerator ().Run ();
		}

		public void Run ()
		{
			try {
				Parse ();
				Serialize ();
			} catch (Exception) {
				Console.Error.WriteLine ("Internal error at line " + lineCount);
				throw;
			}
			/*
			int lastQ = -1;
			for (int x = 1; x < CollationElementTableUtil.Indexer.TotalCount; x++) {
				int i = CollationElementTableUtil.Indexer.GetIndexForCodePoint (x);
				int q = keyValues [collElem [i]].Quarternary;
//				if (i != q && q != 0)
//					Console.Error.WriteLine ("differs : {0} -> {1}", i, q);
				if (q != 0) {
					if (lastQ >= q)
						Console.Error.WriteLine ("latter was smaller at {2} : {0} / {1}", lastQ, q, i);
					lastQ = q;
				}
			}
			*/
		}

		private void Serialize ()
		{
			Console.WriteLine ("static readonly int [] collElem = new int [] {");
			DumpArray (collElem, CollationElementTableUtil.Indexer.TotalCount, true);
			Console.WriteLine ("};");
			Console.WriteLine ("static readonly SortKeyValue [] keyValues = new SortKeyValue [] {");
			for (int i = 0; i < keyCount; i++) {
				SortKeyValue s = keyValues [i];
				Console.WriteLine ("	new SortKeyValue ({0}, 0x{1:X04}, 0x{2:X04}, 0x{3:X04}, 0x{4:X04}),",
					s.Alt ? "true" : "false", s.Primary, s.Secondary,
					s.Thirtiary, s.Quarternary);
			}
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
					int l = getCP ? CollationElementTableUtil.Indexer.ToCodePoint (i) : i;
					Console.WriteLine ("// {0:X04}-{1:X04}", l - 15, l);
				}
			}
		}

		private void Parse ()
		{
			ushort [] v = new ushort [4];

			TextReader reader = Console.In;
			while (reader.Peek () != -1) {
				string line = reader.ReadLine ();
				lineCount++;
				if (line.StartsWith ("@"))
					continue; // @version, @variable etc.
				int idx = line.IndexOf ('#');
				if (idx >= 0)
					line = line.Substring (0, idx);
				if (line.Length == 0)
					continue;

				int cp = int.Parse (line.Substring (0, 5), NumberStyles.HexNumber);
				int collElemIdx = CollationElementTableUtil.Indexer.ToIndex (cp);
				if (collElemIdx < 0) {
					Console.Error.WriteLine ("WARNING: handle character {0:x} in collation element table.", cp);
					continue;
				}

				line = line.Substring (line.IndexOf (';') + 1).Trim ();
				// count entries in a line
				int entryPerLine = 0;
				for (int e = 0; (e = line.IndexOf ('[', e) + 1) > 0;)
					entryPerLine++;

				int start = 0;
				for (int e = 0; e < entryPerLine; e++) {
					start = line.IndexOf ('[', start) + 1;
					string s = line.Substring (start, line.IndexOf (']', start) - start);

					bool alt = false;
					if (s [0] == '*')
						alt = true;
					string [] vslist = s.Substring (1).Split ('.');
					bool skip = false;
					for (int i = 0; i < 4; i++) {
						if (vslist [i].Length > 4)
							skip = true;
						else
							v [i] = ushort.Parse (vslist [i], NumberStyles.HexNumber);
					}
					if (skip) {
//						Console.Error.WriteLine ("WARNING: skipped entry {0:X}", cp);
						continue;
					}
					idx = keyCount;
					if (entryPerLine == 1) {
						// idx = 0 means "no matching entry", so here we start from 1
						for (idx = 1; idx < keyCount; idx++) {
							SortKeyValue k = keyValues [idx];
							if (k.Alt == alt &&
								k.Primary == v [0] &&
								k.Secondary == v [1] &&
								k.Thirtiary == v [2] &&
								k.Quarternary == v [3])
								break;
						}
					}
					if (idx == keyCount)
						AddEntry (alt, v);
				}
				if (entryPerLine == 1)
					collElem [collElemIdx] = idx;
				else
					collElem [collElemIdx] = 
						(short) (keyCount - entryPerLine)
						+ (short) (entryPerLine << 16);
			}
			reader.Close ();
		}

		private void AddEntry (bool alt, ushort [] v)
		{
			if (keyCount == keyValues.Length) {
				SortKeyValue [] tmp = new SortKeyValue [keyCount * 2];
				Array.Copy (keyValues, tmp, keyCount);
				keyValues = tmp;
			}
			keyValues [keyCount] =
				new SortKeyValue (alt,
				v [0], (byte) v [1], (byte) v [2], v [3]);
			keyCount++;
		}
	}
}

