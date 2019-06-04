// UnixMarshalTests.cs - NUnit2 Test Cases for Mono.Unix.UnixMarshal class
//
// Authors:
//  Jonathan Pryor (jonpryor@vt.edu)
//
// (c) 2005 Jonathan Pryor
//

using NUnit.Framework;
using System;
using System.Collections.Generic;
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

	[TestFixture, Category ("NotOnWindows")]
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
		public void BXC10074 ()
		{
			var result = UnixMarshal.StringToHeap (null, Encoding.ASCII);
			Assert.AreEqual (IntPtr.Zero, result, "This used to crash due to a NullReferenceException");
		}

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

		[Test]
		public void TestUtf32PtrToString ()
		{
			var utf32NativeEndianNoBom = new UTF32Encoding(
				bigEndian: !BitConverter.IsLittleEndian,
				byteOrderMark: false,
				throwOnInvalidCharacters: true
			);

			// assemble a buffer that contains:
			// 1. eight garbage bytes
			// 2. the native-endian UTF-32 string "Hello, World" without BOM
			// 3. four 0 bytes (as a C wide string terminator)
			// 4. the native-endian UTF-32 string "broken" without BOM
			// 5. eight 0 bytes
			// 6. four garbage bytes
			var buf = new List<byte>();
			for (int i = 0; i < 2; ++i) {
				buf.Add((byte)0x12);
				buf.Add((byte)0x34);
				buf.Add((byte)0x56);
				buf.Add((byte)0x78);
			}

			buf.AddRange(utf32NativeEndianNoBom.GetBytes("Hello, World"));

			for (int i = 0; i < 4; ++i) {
				buf.Add((byte)0x00);
			}

			buf.AddRange(utf32NativeEndianNoBom.GetBytes("broken"));

			for (int i = 0; i < 8; ++i) {
				buf.Add((byte)0x00);
			}

			buf.Add((byte)0x12);
			buf.Add((byte)0x34);
			buf.Add((byte)0x56);
			buf.Add((byte)0x78);

			// get the array version of this
			var bufArr = buf.ToArray();

			// allocate a buffer that will contain this string
			IntPtr bufPtr = UnixMarshal.AllocHeap(bufArr.Length);
			string returned;
			try
			{
				// copy it in
				Marshal.Copy(bufArr, 0, bufPtr, bufArr.Length);

				// try getting it back
				returned = UnixMarshal.PtrToString(bufPtr + 8, utf32NativeEndianNoBom);
			}
			finally
			{
				UnixMarshal.FreeHeap(bufPtr);
			}

			Assert.AreEqual("Hello, World", returned);
		}
	}
}

