//
// DecoderReplacementFallbackBuffer.cs
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

using Buffer = System.Text.DecoderReplacementFallbackBuffer;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class DecoderReplacementFallbackBufferTest
	{
		private Buffer NewInstance ()
		{
			return new Buffer (new DecoderReplacementFallback ());
		}

		[Test]
		public void FallbackEmptyDefault ()
		{
			Buffer b = NewInstance ();
			Assert.IsTrue (b.Fallback (new byte [] {}, 0), "#0");
			Assert.IsFalse (b.MovePrevious (), "#1");
			Assert.AreEqual (1, b.Remaining, "#2");
			Assert.AreEqual ('?', b.GetNextChar (), "#3");
			Assert.AreEqual (0, b.Remaining, "#4");
			// the string is already consumed.
			Assert.AreEqual (char.MinValue, b.GetNextChar (), "#5");
		}

		[Test]
		public void FallbackEmptyForEncodingUTF8 ()
		{
			Buffer b = Encoding.UTF8.DecoderFallback.CreateFallbackBuffer () as Buffer;
			Assert.IsFalse (b.Fallback (new byte [] {}, 0), "#1");
			Assert.IsFalse (b.MovePrevious (), "#2");
			Assert.AreEqual (0, b.Remaining, "#3");
			// the string does not exist.
			Assert.AreEqual (char.MinValue, b.GetNextChar (), "#4");
		}

		[Test]
		public void FallbackSequential ()
		{
			Buffer b = NewInstance ();
			b.Fallback (new byte [] {}, 0);
			b.GetNextChar ();
			b.Fallback (new byte [] {}, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FallbackRecursiveError ()
		{
			Buffer b = NewInstance ();
			b.Fallback (new byte [] {}, 0);
			b.Fallback (new byte [] {}, 0);
		}

		[Test]
		public void Iterate ()
		{
			Assert.AreEqual ('\0', Encoding.UTF8.DecoderFallback
				.CreateFallbackBuffer ().GetNextChar (), "#1");

			Buffer b = NewInstance ();
			Assert.AreEqual (1, b.Remaining, "#2");
			Assert.AreEqual ('?', b.GetNextChar (), "#3");
			Assert.AreEqual (0, b.Remaining, "#4");
			Assert.AreEqual ('\0', b.GetNextChar (), "#5");
			Assert.IsTrue (b.MovePrevious (), "#6");
			Assert.AreEqual (1, b.Remaining, "#7");
			Assert.IsFalse (b.MovePrevious (), "#8");
			Assert.AreEqual ('?', b.GetNextChar (), "#9");
		}
	}
}

#endif

