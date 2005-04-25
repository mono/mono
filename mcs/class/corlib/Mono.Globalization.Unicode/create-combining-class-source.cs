//
//
// create-combining-class-source.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright 2005 Novell, Inc
//
// It creates combining class information table.
//

using System;
using System.Globalization;
using System.IO;

namespace Mono.Globalization.Unicode
{
	internal class CombiningClassCodeGenerator
	{
		private int lineCount = 0;

		public static void Main ()
		{
			new CombiningClassCodeGenerator ().Run ();
		}

		private void Run ()
		{
			try {
				Process ();
			} catch (Exception ex) {
				throw new InvalidOperationException ("Internal error at line " + lineCount + " : " + ex);
			}
		}

		private void Process ()
		{
			Console.WriteLine ("public static byte GetCombiningClass (int c)");
			Console.WriteLine ("{");
			Console.WriteLine ("	switch (c) {");

			TextReader reader = Console.In;
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

				int cpEnd = -1;
				if (line [n] == '.' && line [n + 1] == '.')
					cpEnd = int.Parse (line.Substring (n + 2, n), NumberStyles.HexNumber);
				int nameStart = line.IndexOf (';') + 1;
				int valueStart = line.IndexOf (';', nameStart) + 1;
				string val = valueStart == 0 ? line.Substring (nameStart) :
					line.Substring (nameStart, valueStart - nameStart - 1);
				SetProp (cp, cpEnd, short.Parse (val));
			}

			Console.WriteLine ("		return {0};", prevVal);
			Console.WriteLine ("	default:");
			Console.WriteLine ("		return 0;");
			Console.WriteLine ("	}");
			Console.WriteLine ("}");

			reader.Close ();
		}

		private short prevVal;

		private void SetProp (int cp, int cpEnd, short val)
		{
			if (val == 0)
				return;

			if (prevVal != val && prevVal != 0)
				Console.WriteLine ("\t\treturn {0};", prevVal);
			prevVal = val;

			if (cpEnd < 0)
				Console.WriteLine ("\tcase 0x{0:X}:", cp);
			else
				for (int i = cp; i <= cpEnd; i++)
					Console.WriteLine ("\tcase 0x{0:X}:", i);
		}
	}
}

