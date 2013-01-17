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

		[Test]
		public void CustomEncodingGetEncoder ()
		{
			var encoding = new CustomEncoding ();
			var encoder = encoding.GetEncoder ();
			Assert.IsNotNull (encoder);
		}

		[Test]
		public void ConvertZeroCharacters ()
		{
			int charsUsed, bytesUsed;
			bool completed;
			byte [] bytes = new byte [0];

			Encoding.UTF8.GetEncoder ().Convert (
				new char[0], 0, 0, bytes, 0, bytes.Length, true,
				out charsUsed, out bytesUsed, out completed);

			Assert.IsTrue (completed, "#1");
			Assert.AreEqual (0, charsUsed, "#2");
			Assert.AreEqual (0, bytesUsed, "#3");
		}

		class CustomEncoding : Encoding {

			public override int GetByteCount (char [] chars, int index, int count)
			{
				throw new NotSupportedException ();
			}

			public override int GetBytes (char [] chars, int charIndex, int charCount, byte [] bytes, int byteIndex)
			{
				throw new NotSupportedException ();
			}

			public override int GetCharCount (byte [] bytes, int index, int count)
			{
				throw new NotSupportedException ();
			}

			public override int GetChars (byte [] bytes, int byteIndex, int byteCount, char [] chars, int charIndex)
			{
				throw new NotSupportedException ();
			}

			public override int GetMaxByteCount (int charCount)
			{
				throw new NotSupportedException ();
			}

			public override int GetMaxCharCount (int byteCount)
			{
				throw new NotSupportedException ();
			}
		}
#endif
	}
}
