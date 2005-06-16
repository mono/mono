using System;
using System.Globalization;

using Uni = Mono.Globalization.Unicode.MSCompatUnicodeTable;

namespace Mono.Globalization.Unicode
{
	class TestDriver
	{
		System.IO.TextWriter Output = Console.Out;

		SimpleCollator coll = new SimpleCollator ();

		#region Testing bits

		static void Main ()
		{
			new TestDriver ().Run ();
		}

		void Run ()
		{
			Console.Error.WriteLine (coll.Compare ("1", "2"));
			Console.Error.WriteLine (coll.Compare ("A", "a"));
			Console.Error.WriteLine (coll.Compare ("12", "1"));
			Console.Error.WriteLine (coll.Compare ("AE", "\u00C6"));

			DumpSortKey ("AE");
			DumpSortKey ("\u00C6");

			// dump sortkey for every single character.
			for (int i = 0; i <= char.MaxValue; i++) {
				byte [] data = coll.GetSortKey (new string ((char) i, 1));
				if (data.Length == 5 && data [0] == 1 && data [1] == 1 &&
					data [2] == 1 && data [3] == 1 && data [4] == 0)
					continue;
				foreach (byte b in data)
					Output.Write ("{0:X02} ", b);
				Output.WriteLine (" : {0:X04}, {1}",
					i, Char.GetUnicodeCategory ((char) i));
			}
			Output.Close ();
		}

		void DumpSortKey (string s)
		{
			byte [] data = coll.GetSortKey (s);
			foreach (byte b in data)
				Console.Error.Write ("{0:X02} ", b);
			Console.Error.WriteLine (" : {0}", s);
		}

		#endregion
	}
}
