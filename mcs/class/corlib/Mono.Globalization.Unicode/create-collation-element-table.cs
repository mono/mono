//
//
// CollationbElementTableCodeGenerator.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright 2005 Novell, Inc
//
// It supports creation of CollationElementTable.cs
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
			Console.WriteLine ("static readonly short [] collElem = new short [] {");
			DumpArray (collElem, CollationElementTableUtil.Indexer.TotalCount, true);
			Console.WriteLine ("};");
			Console.WriteLine ("static readonly SortKeyValue [] keyValues = new SortKeyValue [] {");
			for (int i = 0; i < keyCount; i++) {
				SortKeyValue s = keyValues [i];
				Console.WriteLine ("	new SortKeyValue ({0}, {1}, {2}, {3}, {4}),",
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
					int l = getCP ? CollationElementTableUtil.Indexer.GetCodePointForIndex (i) : i;
					Console.WriteLine ("// {0:X04}-{1:X04}", l - 15, l);
				}
			}
		}

		private void Parse ()
		{
			int [] v = new int [4];

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
				int collElemIdx = CollationElementTableUtil.Indexer.GetIndexForCodePoint (cp);
				if (collElemIdx < 0) {
					Console.Error.WriteLine ("WARNING: handle character {0:x} in collation element table.", cp);
					continue;
				}

				line = line.Substring (line.IndexOf (';') + 1).Trim ();
				// count entries in a line
				int entryPerLine = 0;
				for (int e = 0; (e = line.IndexOf ('[', e + 1)) >= 0;)
					entryPerLine++;

				int start = 0;
				for (int e = 0; e < entryPerLine; e++) {
					start = line.IndexOf ('[', start) + 1;
					string s = line.Substring (start, line.IndexOf (']', start) - start - 1);

					bool alt = false;
					if (s [0] == '*')
						alt = true;
					string [] vslist = s.Substring (1).Split ('.');
					for (int i = 0; i < 4; i++)
						v [i] = int.Parse (vslist [i], NumberStyles.HexNumber);
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

		private void AddEntry (bool alt, int [] v)
		{
			if (keyCount == keyValues.Length) {
				SortKeyValue [] tmp = new SortKeyValue [keyCount * 2];
				Array.Copy (keyValues, tmp, keyCount);
				keyValues = tmp;
			}
			keyValues [keyCount] =
				new SortKeyValue (alt,
				v [0], v [1], v [2], v [3]);
			keyCount++;
		}
	}
}

