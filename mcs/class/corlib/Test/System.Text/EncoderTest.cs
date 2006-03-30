//
// EncoderTest.cs
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
	public class EncoderTest
	{
#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertNullChars ()
		{
			int bytesUsed, charsUsed;
			bool done;
			Encoding.UTF8.GetEncoder ().Convert (
				null, 0, 100, new byte [100], 0, 100, false,
				out bytesUsed, out charsUsed, out done);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertNullBytes ()
		{
			int bytesUsed, charsUsed;
			bool done;
			Encoding.UTF8.GetEncoder ().Convert (
				new char [100], 0, 100, null, 0, 100, false,
				out bytesUsed, out charsUsed, out done);
		}

		[Test]
		public void ConvertLimitedDestination ()
		{
			byte [] bytes = new byte [10000];
			char [] chars = new char [10000];

			Encoder conv = Encoding.UTF8.GetEncoder ();
			int bytesUsed, charsUsed;
			bool done;

			conv.Convert (chars, 0, 10000, bytes, 0, 1000, true,
				      out bytesUsed, out charsUsed, out done);

			Assert.IsFalse (done, "#1");
			Assert.AreEqual (625, bytesUsed, "#2");
			Assert.AreEqual (625, charsUsed, "#3");
		}
#endif
	}
}
