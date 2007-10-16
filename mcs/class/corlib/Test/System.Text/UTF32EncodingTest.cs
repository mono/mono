#if NET_2_0
using System;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class UTF32EncodingTest
	{
		[Test]
		public void IsBrowserDisplay ()
		{
			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.IsFalse (le.IsBrowserDisplay, "#1");

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.IsFalse (be.IsBrowserDisplay, "#2");
		}

		[Test]
		public void IsBrowserSave ()
		{
			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.IsFalse (le.IsBrowserSave);

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.IsFalse (be.IsBrowserSave, "#2");
		}

		[Test]
		public void IsMailNewsDisplay ()
		{
			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.IsFalse (le.IsMailNewsDisplay);

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.IsFalse (be.IsMailNewsDisplay, "#2");
		}

		[Test]
		public void IsMailNewsSave ()
		{
			UTF32Encoding le = new UTF32Encoding (false, true);
			Assert.IsFalse (le.IsMailNewsSave);

			UTF32Encoding be = new UTF32Encoding (true, true);
			Assert.IsFalse (be.IsMailNewsSave, "#2");
		}

		[Test]
		public void TestGetPreamble() {
			byte[] lePreamble = new UTF32Encoding(false, true).GetPreamble();
			Assert.AreEqual (new byte [] { 0xff, 0xfe, 0, 0 }, lePreamble, "#1");

			byte[] bePreamble = new UTF32Encoding(true, true).GetPreamble();
			Assert.AreEqual (new byte [] { 0, 0, 0xfe, 0xff }, bePreamble, "#2");
		}
	}
}
#endif
