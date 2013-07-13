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


		[Test]
		public void CustomEncodingGetDecoder ()
		{
			var encoding = new CustomEncoding ();
			var decoder = encoding.GetDecoder ();
			Assert.IsNotNull (decoder);
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

		[Test]
		public void Bug10789 ()
		{
			byte[] bytes = new byte[100];
			char[] chars = new char[100];  

			Decoder conv = Encoding.UTF8.GetDecoder ();
			int charsUsed, bytesUsed;
			bool completed;
			
			conv.Convert (bytes, 0, 0, chars, 100, 0, false, out bytesUsed, out charsUsed, out completed);

			Assert.IsTrue (completed, "#1");
			Assert.AreEqual (0, charsUsed, "#2");
			Assert.AreEqual (0, bytesUsed, "#3");
		}
	}
}
