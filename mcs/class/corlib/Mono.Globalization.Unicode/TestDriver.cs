using System;
using System.Globalization;

using Uni = Mono.Globalization.Unicode.MSCompatUnicodeTable;

namespace Mono.Globalization.Unicode
{
	class TestDriver
	{
		System.IO.TextWriter Output = Console.Out;

		SimpleCollator coll = new SimpleCollator (CultureInfo.InvariantCulture);

		#region Testing bits

		static void Main ()
		{
			new TestDriver ().Run ();
		}

		void Run ()
		{
			Compare ("1", "2");
			Compare ("A", "a");
			Compare ("A", "a", CompareOptions.IgnoreCase);
			Compare ("\uFF10", "0", CompareOptions.IgnoreWidth);
			Compare ("\uFF21", "a", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
			Compare ("12", "1");
			Compare ("AE", "\u00C6");
			Compare ("AB\u01c0C", "A\u01c0B\u01c0C", CompareOptions.IgnoreSymbols);
			Compare ("A\u0304", "\u0100"); // diacritical weight addition

			IndexOf ("ABC", '1', CompareOptions.None);
			IndexOf ("ABC", 'c', CompareOptions.IgnoreCase);
			IndexOf ("ABC", '\uFF22', CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
			IndexOf ("ABCDE", '\u0117', CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);

			IsPrefix ("ABC", "c", CompareOptions.IgnoreCase);
			IsPrefix ("BC", "c", CompareOptions.IgnoreCase);
			IsPrefix ("C", "c", CompareOptions.IgnoreCase);
			IsPrefix ("E", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);

			IndexOf ("ABC", "1", CompareOptions.None);
			IndexOf ("ABC", "c", CompareOptions.IgnoreCase);
			IndexOf ("ABC", "\uFF22", CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth);
			IndexOf ("ABCDE", "\u0117", CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);

			DumpSortKey ("AE");
			DumpSortKey ("\u00C6");

/*
			// dump sortkey for every single character.
			for (int i = 0; i <= char.MaxValue; i++) {
				byte [] data = coll.GetSortKey (new string ((char) i, 1)).KeyData;
				if (data.Length == 5 && data [0] == 1 && data [1] == 1 &&
					data [2] == 1 && data [3] == 1 && data [4] == 0)
					continue;
				foreach (byte b in data)
					Output.Write ("{0:X02} ", b);
				Output.WriteLine (" : {0:X04}, {1}",
					i, Char.GetUnicodeCategory ((char) i));
			}
			Output.Close ();
*/
		}

		void Compare (string s1, string s2)
		{
			Compare (s1, s2, CompareOptions.None);
		}

		void Compare (string s1, string s2, CompareOptions opt)
		{
			Console.Error.WriteLine ("{0} {1} / {2}",
				coll.Compare (s1, s2, opt), s1, s2);
		}

		void IndexOf (string s, char c, CompareOptions opt)
		{
			Console.Error.WriteLine ("cIndex: {0} {1} / {2}",
				coll.IndexOf (s, c, opt), s, c);
		}

		void IndexOf (string s1, string s2, CompareOptions opt)
		{
			Console.Error.WriteLine ("sIndex: {0} {1} / {2}",
				coll.IndexOf (s1, s2, opt), s1, s2);
		}

		void IsPrefix (string s1, string s2, CompareOptions opt)
		{
			Console.Error.WriteLine ("IsPrefix: {0} {1} / {2}",
				coll.IsPrefix (s1, s2, opt), s1, s2);
		}

		void DumpSortKey (string s)
		{
			byte [] data = coll.GetSortKey (s).KeyData;
			foreach (byte b in data)
				Console.Error.Write ("{0:X02} ", b);
			Console.Error.WriteLine (" : {0}", s);
		}

		#endregion
	}
}
