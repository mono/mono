//
// EncoderReplacementFallback.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//

#if NET_2_0

using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class EncoderReplacementFallbackTest
	{
		[Test]
		public void Defaults ()
		{
			EncoderReplacementFallback f =
				new EncoderReplacementFallback ();
			Assert.AreEqual ("?", f.DefaultString, "#1");
			Assert.AreEqual (1, f.MaxCharCount, "#2");

			f = new EncoderReplacementFallback (String.Empty);
			Assert.AreEqual (String.Empty, f.DefaultString, "#3");
			Assert.AreEqual (0, f.MaxCharCount, "#4");

			f = Encoding.UTF8.EncoderFallback as EncoderReplacementFallback;
			Assert.IsNotNull (f, "#5");
			Assert.AreEqual ("\uFFFD", f.DefaultString, "#6");
			Assert.AreEqual (1, f.MaxCharCount, "#7");

			// after beta2 this test became invalid.
			//f = new MyEncoding ().EncoderFallback as EncoderReplacementFallback;
			//Assert.IsNotNull (f, "#8");
			//Assert.AreEqual (String.Empty, f.DefaultString, "#9");
			//Assert.AreEqual (0, f.MaxCharCount, "#10");

			f = EncoderFallback.ReplacementFallback as EncoderReplacementFallback;
			Assert.AreEqual ("?", f.DefaultString, "#11");
			Assert.AreEqual (1, f.MaxCharCount, "#12");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DontChangeReadOnlyUTF8EncoderFallback ()
		{
			Encoding.UTF8.EncoderFallback =
				new EncoderReplacementFallback ();
		}

		[Test]
		public void DontChangeReadOnlyCodePageEncoderFallback ()
		{
			Encoding encoding = Encoding.GetEncoding (Encoding.Default.CodePage);
			try {
				encoding.EncoderFallback = new EncoderReplacementFallback ();
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CustomEncodingSetEncoderFallback ()
		{
			new MyEncoding ().EncoderFallback =
				new EncoderReplacementFallback ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EncodingSetNullEncoderFallback ()
		{
			Encoding.Default.EncoderFallback = null;
		}

		[Test]
		// Don't throw an exception
		public void SetEncoderFallback ()
		{
			Encoding.Default.GetEncoder ().Fallback =
				new EncoderReplacementFallback ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void EncoderSetNullFallback ()
		{
			Encoding.Default.GetEncoder ().Fallback = null;
		}

		[Test]
		public void Latin1Replacement ()
			// coz Latin1 is easy single byte encoding.
		{
			Encoding enc = Encoding.GetEncoding (28591); // Latin1
			byte [] reference = new byte [] {0x58, 0x58, 0x3F, 0x3F, 0x3F, 0x5A, 0x5A};
			byte [] bytes = enc.GetBytes ("XX\u3007\u4E00\u9780ZZ");
			Assert.AreEqual (reference, bytes, "#1");
		}
	}
}

#endif

