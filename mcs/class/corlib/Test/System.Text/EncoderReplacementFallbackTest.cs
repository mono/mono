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
			Assert.AreEqual (String.Empty, f.DefaultString, "#6");
			Assert.AreEqual (0, f.MaxCharCount, "#7");

			f = new MyEncoding ().EncoderFallback as EncoderReplacementFallback;
			Assert.IsNotNull (f, "#8");
			Assert.AreEqual (String.Empty, f.DefaultString, "#9");
			Assert.AreEqual (0, f.MaxCharCount, "#10");

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
		[ExpectedException (typeof (InvalidOperationException))]
		public void DontChangeReadOnlyCodePageEncoderFallback ()
		{
			Encoding.GetEncoding (932).EncoderFallback =
				new EncoderReplacementFallback ();
		}

		[Test]
		// Don't throw an exception
		public void SetEncoderFallback ()
		{
			new MyEncoding ().EncoderFallback =
				new EncoderReplacementFallback ();
			new MyEncoding (1).EncoderFallback =
				new EncoderReplacementFallback ();
			Encoding.Default.GetEncoder ().Fallback =
				new EncoderReplacementFallback ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void EncodingSetNullEncoderFallback ()
		{
			new MyEncoding ().EncoderFallback = null;
		}


		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void EncoderSetNullFallback ()
		{
			Encoding.Default.GetEncoder ().Fallback = null;
		}
	}
}

#endif

