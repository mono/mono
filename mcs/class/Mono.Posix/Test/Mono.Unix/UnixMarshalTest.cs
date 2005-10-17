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
		public void TestStringToAlloc ()
		{
			object[] data = {
				"Hello, world!", true, true,
				"ＭＳ Ｐゴシック", false, true,
			};

			for (int i = 0; i < data.Length; i += 3) {
				string s           = (string) data [i+0];
				bool valid_ascii   = (bool)   data [i+1];
				bool valid_unicode = (bool)   data [i+2];

				TestStringToAlloc (s, valid_ascii, valid_unicode);
			}
		}

		private static void TestStringToAlloc (string s, bool validAscii, bool validUnicode)
		{
			TestStringToAlloc (s, Encoding.ASCII, validAscii);
			TestStringToAlloc (s, Encoding.UTF7, validUnicode);
			TestStringToAlloc (s, Encoding.UTF8, validUnicode);
			TestStringToAlloc (s, Encoding.Unicode, validUnicode);
			TestStringToAlloc (s, Encoding.BigEndianUnicode, validUnicode);
			TestStringToAlloc (s, new RandomEncoding (), validUnicode);
		}

		private static void TestStringToAlloc (string s, Encoding e, bool mustBeEqual)
		{
			IntPtr p = UnixMarshal.StringToAlloc (s, e);
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

