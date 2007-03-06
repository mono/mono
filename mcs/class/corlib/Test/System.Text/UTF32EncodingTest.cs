using NUnit.Framework;
using System;
using System.Text;

#if NET_2_0

namespace MonoTests.System.Text {

	[TestFixture]
	public class UTF32EncodingTest : Assertion {

		[Test]
		public void TestGetPreamble() {
			byte[] lePreamble = new UTF32Encoding(false, true).GetPreamble();
			if (!AreEqual(lePreamble, new byte[]{ 0xff, 0xfe, 0, 0 })) {
				Fail ("Little-endian UTF32 preamble is incorrect");
			}

			byte[] bePreamble = new UTF32Encoding(true, true).GetPreamble();
			if (!AreEqual(bePreamble, new byte[]{ 0, 0, 0xfe, 0xff })) {
				Fail ("Big-endian UTF32 preamble is incorrect");
			}
		}

		private bool AreEqual(byte[] a, byte[] b) {
			if (a.Length != b.Length)
				return false;
			for (int i = 0; i < a.Length; ++i) {
				if (a[i] != b[i])
					return false;
			}
			return true;
		}
	}
}

#endif

