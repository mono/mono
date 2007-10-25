//
// DecoderReplacementFallback.cs
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
	public class DecoderReplacementFallbackTest
	{
		[Test]
		public void Defaults ()
		{
			DecoderReplacementFallback f =
				new DecoderReplacementFallback ();
			Assert.AreEqual ("?", f.DefaultString, "#1");
			Assert.AreEqual (1, f.MaxCharCount, "#2");

			f = new DecoderReplacementFallback (String.Empty);
			Assert.AreEqual (String.Empty, f.DefaultString, "#3");
			Assert.AreEqual (0, f.MaxCharCount, "#4");

			f = Encoding.UTF8.DecoderFallback as DecoderReplacementFallback;
			Assert.IsNotNull (f, "#5");
			// This behavior was introduced as
			// http://support.microsoft.com/kb/940521/
			Assert.AreEqual ("\uFFFD", f.DefaultString, "#6");
			Assert.AreEqual (1, f.MaxCharCount, "#7");

			// after beta2 this test became invalid.
			//f = new MyEncoding ().DecoderFallback as DecoderReplacementFallback;
			//Assert.IsNotNull (f, "#8");
			//Assert.AreEqual (String.Empty, f.DefaultString, "#9");
			//Assert.AreEqual (0, f.MaxCharCount, "#10");

			f = DecoderFallback.ReplacementFallback as DecoderReplacementFallback;
			Assert.AreEqual ("?", f.DefaultString, "#11");
			Assert.AreEqual (1, f.MaxCharCount, "#12");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DontChangeReadOnlyUTF8DecoderFallback ()
		{
			Encoding.UTF8.DecoderFallback =
				new DecoderReplacementFallback ();
		}

		[Test]
		public void DontChangeReadOnlyCodePageDecoderFallback ()
		{
			Encoding encoding = Encoding.GetEncoding (Encoding.Default.CodePage);
			try {
				encoding.DecoderFallback = new DecoderReplacementFallback ();
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
			new MyEncoding ().DecoderFallback =
				new DecoderReplacementFallback ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EncodingSetNullDecoderFallback ()
		{
			Encoding.Default.DecoderFallback = null;
		}

		[Test]
		// Don't throw an exception
		public void SetDecoderFallback ()
		{
			Encoding.Default.GetDecoder ().Fallback =
				new DecoderReplacementFallback ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DecoderSetNullFallback ()
		{
			Encoding.Default.GetDecoder ().Fallback = null;
		}
	}
}

#endif

