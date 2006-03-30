//
// DecoderTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2006 Novell, Inc.
// 
using NUnit.Framework;
using System;
using System.Text;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class DecoderTest
	{
#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertNullChars ()
		{
			int charsUsed, bytesUsed;
			bool done;
			Encoding.UTF8.GetDecoder ().Convert (
				null, 0, 100, new char [100], 0, 100, false,
				out charsUsed, out bytesUsed, out done);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertNullBytes ()
		{
			int charsUsed, bytesUsed;
			bool done;
			Encoding.UTF8.GetDecoder ().Convert (
				new byte [100], 0, 100, null, 0, 100, false,
				out charsUsed, out bytesUsed, out done);
		}

		[Test]
		public void ConvertLimitedDestination ()
		{
			char [] chars = new char [10000];
			byte [] bytes = new byte [10000];

			Decoder conv = Encoding.UTF8.GetDecoder ();
			int charsUsed, bytesUsed;
			bool done;

			conv.Convert (bytes, 0, 10000, chars, 0, 1000, true,
				      out charsUsed, out bytesUsed, out done);

			Assert.IsFalse (done, "#1");
			Assert.AreEqual (625, charsUsed, "#2");
			Assert.AreEqual (625, bytesUsed, "#3");
		}
#endif
	}
}
