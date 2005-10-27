// UnixMarshalTests.cs - NUnit2 Test Cases for Mono.Unix.UnixMarshal class
//
// Authors:
//  Jonathan Pryor (jonpryor@vt.edu)
//
// (c) 2005 Jonathan Pryor
//

using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using Mono.Unix;

namespace MonoTests.Mono.Unix {

	class RandomEncoding : UTF8Encoding {
		public RandomEncoding ()
			: base (false, true)
		{
		}

		public override int GetMaxByteCount (int value)
		{
			return value*6;
		}
	}

	[TestFixture]
	class UnixMarshalTest {
#if false
		public static void Main ()
		{
			string s = UnixMarshal.GetErrorDescription (Errno.ERANGE);
			Console.WriteLine ("ERANGE={0}", s);
			s = UnixMarshal.GetErrorDescription ((Errno) 999999);
			Console.WriteLine ("Invalid={0}", s);
		}
#endif

		[Test]
		public void TestStringToHeap ()
		{
			object[] data = {
				"Hello, world!", true, true,
				"ＭＳ Ｐゴシック", false, true,
			};

			for (int i = 0; i < data.Length; i += 3) {
				string s           = (string) data [i+0];
				bool valid_ascii   = (bool)   data [i+1];
				bool valid_unicode = (bool)   data [i+2];

				TestStringToHeap (s, valid_ascii, valid_unicode);
			}
		}

		private static void TestStringToHeap (string s, bool validAscii, bool validUnicode)
		{
			TestStringToHeap (s, Encoding.ASCII, validAscii);
			TestStringToHeap (s, Encoding.UTF7, validUnicode);
			TestStringToHeap (s, Encoding.UTF8, validUnicode);
			TestStringToHeap (s, Encoding.Unicode, validUnicode);
			TestStringToHeap (s, Encoding.BigEndianUnicode, validUnicode);
			TestStringToHeap (s, new RandomEncoding (), validUnicode);
		}

		private static void TestStringToHeap (string s, Encoding e, bool mustBeEqual)
		{
			IntPtr p = UnixMarshal.StringToHeap (s, e);
			try {
				string _s = UnixMarshal.PtrToString (p, e);
				if (mustBeEqual)
					Assert.AreEqual (s, _s, "#TSTA (" + e.GetType() + ")");
			}
			finally {
				UnixMarshal.Free (p);
			}
		}
	}
}

