//
// create-combining-class-source.cs :
//	creates combining class information table.
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

using Util = Mono.Globalization.Unicode.NormalizationTableUtil;

namespace Mono.Globalization.Unicode
{
	internal class CombiningClassCodeGenerator
	{
		private int lineCount = 0;

		TextWriter CSCodeOut = TextWriter.Null;//Console.Out;
		TextWriter CSTableOut = Console.Out;//TextWriter.Null;

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
			CSCodeOut.WriteLine ("public static byte GetCombiningClass (int c)");
			CSCodeOut.WriteLine ("{");
			CSCodeOut.WriteLine ("	switch (c) {");

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

			CSCodeOut.WriteLine ("		return {0};", prevVal);
			CSCodeOut.WriteLine ("	default:");
			CSCodeOut.WriteLine ("		return 0;");
			CSCodeOut.WriteLine ("	}");
			CSCodeOut.WriteLine ("}");

			reader.Close ();

			byte [] ret = CodePointIndexer.CompressArray (
				values, typeof (byte), Util.Combining) as byte [];

			CSTableOut.WriteLine ("public static byte [] combiningClass = new byte [] {");
			for (int i = 0; i < ret.Length; i++) {
				byte value = ret [i];
				if (value < 10)
					CSTableOut.Write ("{0},", value);
				else
					CSTableOut.Write ("0x{0:X02},", value);
				if (i % 16 == 15)
					CSTableOut.WriteLine (" // {0:X04}", Util.Combining.ToCodePoint (i - 15));
			}
			CSTableOut.WriteLine ("};");
		}

		private short prevVal;

		byte [] values = new byte [0x20000];

		private void SetProp (int cp, int cpEnd, short val)
		{
			if (val == 0)
				return;

			if (prevVal != val && prevVal != 0)
				CSCodeOut.WriteLine ("\t\treturn {0};", prevVal);
			prevVal = val;

			if (cpEnd < 0) {
				CSCodeOut.WriteLine ("\tcase 0x{0:X}:", cp);
				values [cp] = (byte) val;
			}
			else
				for (int i = cp; i <= cpEnd; i++) {
					CSCodeOut.WriteLine ("\tcase 0x{0:X}:", i);
					values [i] = (byte) val;
				}
		}
	}
}

