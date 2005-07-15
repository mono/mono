using System;
using System.Globalization;

using Uni = Mono.Globalization.Unicode.MSCompatUnicodeTable;

namespace Mono.Globalization.Unicode
{
	class TestDriver
	{
		SimpleCollator coll = new SimpleCollator (CultureInfo.InvariantCulture);

		#region Testing bits

		static void Main (string [] args)
		{
			if (args.Length > 0 && args [0] == "--generate")
				new TestDriver ().Generate ();
			if (args.Length > 0 && args [0] == "--check")
				new TestDriver ().CheckCultures ();
			else
				new TestDriver ().Run ();
		}

		void CheckCultures ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (
				CultureTypes.AllCultures))
				Console.WriteLine ("Culture {0}({1}) : OK: {2}", ci.LCID, ci.Name, new SimpleCollator (ci));
		}

		void Run ()
		{
LastIndexOf ("\u30D1\u30FC", "\u30A2", CompareOptions.IgnoreNonSpace);
return;
			/*
			DumpSortKey ("AE");
			DumpSortKey ("\u00C6");
			DumpSortKey ("ABCABC", 5, 1, CompareOptions.IgnoreCase);
			DumpSortKey ("-");
			DumpSortKey ("--");
			DumpSortKey ("A-B-C");
			DumpSortKey ("A\u0304");
			DumpSortKey ("\u0100");

			Compare ("1", "2");
			Compare ("A", "a");
			Compare ("A", "a", CompareOptions.IgnoreCase);
			Compare ("\uFF10", "0", CompareOptions.IgnoreWidth);
			Compare ("\uFF21", "a", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
			Compare ("12", "1");
			Compare ("AE", "\u00C6");
			Compare ("AB\u01c0C", "A\u01c0B\u01c0C", CompareOptions.IgnoreSymbols);
			Compare ("A\u0304", "\u0100"); // diacritical weight addition
			Compare ("ABCABC", 5, 1, "c", 0, 1, CompareOptions.IgnoreCase);
			Compare ("-d:NET_1_1", 0, 1, "-", 0, 1, CompareOptions.None);

			IndexOf ("ABC", '1', CompareOptions.None);
			IndexOf ("ABCABC", 'c', CompareOptions.IgnoreCase);
			IndexOf ("ABCABC", '\uFF22', CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
			IndexOf ("ABCDE", '\u0117', CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
			IndexOf ("ABCABC", 'B', 1, 5, CompareOptions.IgnoreCase);
			IndexOf ("\u00E6", 'a', CompareOptions.None);

			LastIndexOf ("ABC", '1', CompareOptions.None);
			LastIndexOf ("ABCABC", 'c', CompareOptions.IgnoreCase);
			LastIndexOf ("ABCABC", '\uFF22', CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
			LastIndexOf ("ABCDE", '\u0117', CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);

			IsPrefix ("ABC", "c", CompareOptions.IgnoreCase);
			IsPrefix ("BC", "c", CompareOptions.IgnoreCase);
			IsPrefix ("C", "c", CompareOptions.IgnoreCase);
			IsPrefix ("EDCBA", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
			IsPrefix ("ABC", "AB", CompareOptions.IgnoreCase);
			IsPrefix ("ae", "\u00E6", CompareOptions.None);
			IsPrefix ("\u00E6", "ae", CompareOptions.None);
			IsPrefix ("\u00E6", "a", CompareOptions.None);
			IsPrefix ("\u00E6s", "ae", CompareOptions.None);
			IsPrefix ("\u00E6", "aes", CompareOptions.None);
			IsPrefix ("--start", "--", CompareOptions.None);
			IsPrefix ("-d:NET_1_1", "-", CompareOptions.None);
			IsPrefix ("-d:NET_1_1", "@", CompareOptions.None);

			IsSuffix ("ABC", "c", CompareOptions.IgnoreCase);
			IsSuffix ("BC", "c", CompareOptions.IgnoreCase);
			IsSuffix ("CBA", "c", CompareOptions.IgnoreCase);
			IsSuffix ("ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
			IsSuffix ("\u00E6", "a", CompareOptions.None);
			IsSuffix ("\u00E6", "e", CompareOptions.None);
			IsSuffix ("\u00E6", "ae", CompareOptions.None);
			IsSuffix ("ae", "\u00E6", CompareOptions.None);
			IsSuffix ("e", "\u00E6", CompareOptions.None);

			IndexOf ("ABC", "1", CompareOptions.None);
			IndexOf ("ABCABC", "c", CompareOptions.IgnoreCase);
			IndexOf ("ABCABC", "\uFF22", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
			IndexOf ("ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
			IndexOf ("ABCABC", "BC", CompareOptions.IgnoreCase);
			IndexOf ("BBCBBC", "BC", CompareOptions.IgnoreCase);
			IndexOf ("ABCDEF", "BCD", 0, 3, CompareOptions.IgnoreCase);
			IndexOf ("-ABC", "-", CompareOptions.None);
			IndexOf ("--ABC", "--", CompareOptions.None);

			LastIndexOf ("ABC", "1", CompareOptions.None);
			LastIndexOf ("ABCABC", "c", CompareOptions.IgnoreCase);
			LastIndexOf ("ABCABC", "\uFF22", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
			LastIndexOf ("ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
			LastIndexOf ("ABCABC", "BC", CompareOptions.IgnoreCase);
			LastIndexOf ("BBCBBC", "BC", CompareOptions.IgnoreCase);
			LastIndexOf ("original", "rig", CompareOptions.None);
			LastIndexOf ("\u00E6", "ae", CompareOptions.None);
			LastIndexOf ("-ABC", "-", CompareOptions.None);
			LastIndexOf ("--ABC", "--", CompareOptions.None);
			*/

			coll = new SimpleCollator (new CultureInfo ("hu"));
			DumpSortKey ("ZSAZS1");
			IsSuffix ("zs", "z", CompareOptions.None);
			IsSuffix ("zs", "s", CompareOptions.None);
			IsSuffix ("zs", "zs", CompareOptions.None);
			IsSuffix ("sz", "z", CompareOptions.None);
			IsSuffix ("sz", "s", CompareOptions.None);
			IsSuffix ("--ABC", "--", CompareOptions.None);
			IsSuffix ("ABC--", "--", CompareOptions.None);

/*
			coll = new SimpleCollator (new CultureInfo (""));
			Compare ("c\u00F4te", "cot\u00E9");
			DumpSortKey ("c\u00F4te");
			DumpSortKey ("cot\u00E9");
			coll = new SimpleCollator (new CultureInfo ("fr"));
			Compare ("c\u00F4te", "cot\u00E9");
			DumpSortKey ("c\u00F4te");
			DumpSortKey ("cot\u00E9");
*/
		}

		void Generate ()
		{
			// dump sortkey for every single character.
			for (int i = 0; i <= char.MaxValue; i++) {
				byte [] data = coll.GetSortKey (new string ((char) i, 1), CompareOptions.StringSort).KeyData;
				if (data.Length == 5 && data [0] == 1 && data [1] == 1 &&
					data [2] == 1 && data [3] == 1 && data [4] == 0)
					continue;
				foreach (byte b in data)
					Console.Write ("{0:X02} ", b);
				Console.WriteLine (" : {0:X04}, {1}",
					i, Char.GetUnicodeCategory ((char) i));
			}
		}

		void Compare (string s1, string s2)
		{
			Compare (s1, s2, CompareOptions.None);
		}

		void Compare (string s1, string s2, CompareOptions opt)
		{
			Console.Error.WriteLine ("compare ({3}): {0} {1} / {2}",
				coll.Compare (s1, s2, opt), s1, s2, opt);
		}

		void Compare (string s1, int idx1, int len1, string s2, int idx2, int len2, CompareOptions opt)
		{
			Console.Error.WriteLine ("compare ({3} {4} {5} {6} {7}): {0} {1} / {2}",
				coll.Compare (s1, idx1, len1, s2, idx2, len2, opt), s1, s2,
					opt, idx1, len1, idx2, len2);
		}

		void IndexOf (string s, char c, CompareOptions opt)
		{
			IndexOf (s, c, 0, s.Length, opt);
		}

		void IndexOf (string s, char c, int idx, int len, CompareOptions opt)
		{
			Console.Error.WriteLine ("cIndex ({3} {4} {5}): {0} {1} / {2}",
				coll.IndexOf (s, c, idx, len, opt), s, c, opt, idx, len);
		}

		void IndexOf (string s1, string s2, CompareOptions opt)
		{
			IndexOf (s1, s2, 0, s1.Length, opt);
		}

		void IndexOf (string s1, string s2, int idx, int len, CompareOptions opt)
		{
			Console.Error.WriteLine ("sIndex ({3} {4} {5}): {0} {1} / {2}",
				coll.IndexOf (s1, s2, idx, len, opt), s1, s2, opt, idx, len);
		}

		void IsPrefix (string s1, string s2, CompareOptions opt)
		{
			Console.Error.WriteLine ("prefix ({3}): {0} {1} / {2}",
				coll.IsPrefix (s1, s2, opt), s1, s2, opt);
		}

		void LastIndexOf (string s, char c, CompareOptions opt)
		{
			Console.Error.WriteLine ("cLast ({3}): {0} {1} / {2}",
				coll.LastIndexOf (s, c, opt), s, c, opt);
		}

		void LastIndexOf (string s1, string s2, CompareOptions opt)
		{
			Console.Error.WriteLine ("sLast ({3}): {0} {1} / {2}",
				coll.LastIndexOf (s1, s2, opt), s1, s2, opt);
		}

		void LastIndexOf (string s1, string s2, int idx, int len, CompareOptions opt)
		{
			Console.Error.WriteLine ("sLast ({3},{4},{5}): {0} {1} / {2}",
				coll.LastIndexOf (s1, s2, idx, len, opt), s1, s2, opt, idx, len);
		}

		void IsSuffix (string s1, string s2, CompareOptions opt)
		{
			Console.Error.WriteLine ("suffix ({3}): {0} {1} / {2}",
				coll.IsSuffix (s1, s2, opt), s1, s2, opt);
		}

		void DumpSortKey (string s)
		{
			DumpSortKey (s, 0, s.Length, CompareOptions.None);
		}

		void DumpSortKey (string s, int idx, int len, CompareOptions opt)
		{
			byte [] data = coll.GetSortKey (s, idx, len, opt).KeyData;
			foreach (byte b in data)
				Console.Error.Write ("{0:X02} ", b);
			Console.Error.WriteLine (" : {0} ({1} {2} {3})", s, opt, idx, len);
		}

		#endregion
	}
}
