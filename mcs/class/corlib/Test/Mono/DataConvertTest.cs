using System;
using System.Collections;
using System.Text;
using NUnit.Framework;
using Mono;

#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif

namespace MonoTests {

	[TestFixture]
	public class DataConverterTest
	{
		const string TEST_STRING = "Alignment test";
	
		static string Dump (byte [] d)
		{
			StringBuilder sb = new StringBuilder ();
			
			for (int i = 0; i < d.Length; i++){
				sb.Append (String.Format ("{0:x2} ", d [i]));
			}
			return sb.ToString ().Trim ();
		}
	
		[Test]
		public void PackTests ()
		{
			Assert.AreEqual (Dump (DataConverter.Pack ("z8", "hello")), "68 65 6c 6c 6f 00");
			Assert.AreEqual (Dump (DataConverter.Pack ("z6", "hello")), "68 00 65 00 6c 00 6c 00 6f 00 00 00");
			Assert.AreEqual (Dump (DataConverter.Pack ("CCCC", 65, 66, 67, 68)), "41 42 43 44");
	
			Assert.AreEqual (Dump (DataConverter.Pack ("4C", 65, 66, 67, 68, 69, 70)),  "41 42 43 44");
			Assert.AreEqual (Dump (DataConverter.Pack ("^iii", 0x1234abcd, 0x7fadb007)), "12 34 ab cd 7f ad b0 07 00 00 00 00");
			Assert.AreEqual (Dump (DataConverter.Pack ("_s!i", 0x7b, 0x12345678)), "7b 00 00 00 78 56 34 12");
		}
	
		[Test]
		public void ArrayTests ()
		{
			byte [] source = new byte [] { 1, 2, 3, 4 };
			byte [] dest = new byte [4];
	
			int l = DataConverter.Int32FromBE (source, 0);
			Assert.AreEqual (l, 0x01020304);
		}
	
		[Test]
		public void StringAlignment ()
		{
			byte[] packed = Mono.DataConverter.Pack ("bz8", 1, TEST_STRING);
				
			IList unpacked = Mono.DataConverter.Unpack ("bz8", packed, 0);
			
			Assert.AreEqual(1, (byte) unpacked[0]);
			Assert.AreEqual(TEST_STRING, new string((char[]) unpacked[1]));
		}

		[Test]
		public void UnpackTests ()
		{
			float f = (float)DataConverter.Unpack ("%f", DataConverter.Pack ("f", 3.14), 0) [0];
			Assert.That ((f - 3.14f), Is.LessThanOrEqualTo (Single.Epsilon));
		}
	}
}