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
using System.Runtime.InteropServices;
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
	public class UnixMarshalTest {
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

				StringToHeap (s, valid_ascii, valid_unicode);
			}
		}

		private static void StringToHeap (string s, bool validAscii, bool validUnicode)
		{
			StringToHeap (s, Encoding.ASCII, validAscii);
			StringToHeap (s, Encoding.UTF7, validUnicode);
			StringToHeap (s, Encoding.UTF8, validUnicode);
			StringToHeap (s, Encoding.Unicode, validUnicode);
			StringToHeap (s, Encoding.BigEndianUnicode, validUnicode);
			StringToHeap (s, new RandomEncoding (), validUnicode);
		}

		private static void StringToHeap (string s, Encoding e, bool mustBeEqual)
		{
			IntPtr p = UnixMarshal.StringToHeap (s, e);
			try {
				string _s = UnixMarshal.PtrToString (p, e);
				if (mustBeEqual)
					Assert.AreEqual (s, _s, "#TSTA (" + e.GetType() + ")");
			}
			finally {
				UnixMarshal.FreeHeap (p);
			}
		}
		
		[Test]
		public void TestPtrToString ()
		{
			IntPtr p = UnixMarshal.AllocHeap (1);
			Marshal.WriteByte (p, 0);
			string s = UnixMarshal.PtrToString (p);
			UnixMarshal.FreeHeap (p);
		}
	}
}

