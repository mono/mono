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
			Assert.AreEqual (String.Empty, f.DefaultString, "#6");
			Assert.AreEqual (0, f.MaxCharCount, "#7");

			f = new MyEncoding ().DecoderFallback as DecoderReplacementFallback;
			Assert.IsNotNull (f, "#8");
			Assert.AreEqual (String.Empty, f.DefaultString, "#9");
			Assert.AreEqual (0, f.MaxCharCount, "#10");

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
		[ExpectedException (typeof (InvalidOperationException))]
		public void DontChangeReadOnlyCodePageDecoderFallback ()
		{
			Encoding.GetEncoding (932).DecoderFallback =
				new DecoderReplacementFallback ();
		}

		[Test]
		// Don't throw an exception
		public void SetDecoderFallback ()
		{
			new MyEncoding ().DecoderFallback =
				new DecoderReplacementFallback ();
			new MyEncoding (1).DecoderFallback =
				new DecoderReplacementFallback ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void EncodingSetNullDecoderFallback ()
		{
			new MyEncoding ().DecoderFallback = null;
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

