//
// EncodingTester.cs
//
// Author:
//	Marcos Henrich  <marcos.henrich@xamarin.com>
//
// (C) 2014 Xamarin, Inc.
// 

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoTests.System.Text
{
	class EncodingTester
	{
		class DecoderTestFallbackBuffer : DecoderFallbackBuffer
		{
			DecoderFallbackBuffer buffer;
			private FallbackDelegate fallbackAction;

			public DecoderTestFallbackBuffer (DecoderReplacementFallback fallback, FallbackDelegate fallbackAction)
			{
				this.fallbackAction = fallbackAction;
				buffer = new DecoderReplacementFallbackBuffer (fallback);
			}

			public override bool Fallback (byte [] bytesUnknown, int index)
			{
				fallbackAction (bytesUnknown, index);
				return buffer.Fallback (bytesUnknown, index);
			}

			public override char GetNextChar ()
			{
				return buffer.GetNextChar ();
			}

			public override bool MovePrevious ()
			{
				return buffer.MovePrevious ();
			}

			public override int Remaining
			{
				get { return buffer.Remaining; }
			}

			public override void Reset ()
			{
				buffer.Reset ();
			}
		}

		class DecoderTestFallback : DecoderFallback
		{
			private DecoderReplacementFallback fallback;
			private FallbackDelegate fallbackAction;

			public DecoderTestFallback (FallbackDelegate fallbackAction)
			{
				this.fallbackAction = fallbackAction;
			}

			public override DecoderFallbackBuffer CreateFallbackBuffer ()
			{
				fallback = new DecoderReplacementFallback ();
				return new DecoderTestFallbackBuffer (fallback, fallbackAction);
			}

			public override int MaxCharCount
			{
				get { return fallback.MaxCharCount; }
			}
		}

		public delegate void FallbackDelegate (byte [] bytesUnknown, int index);

		Encoding encoding;

		byte [][] expectedUnknownBytes;
		int expectedUnknownBytesIndex;

		public EncodingTester (string encodingName)
		{
			var decoderFallback = new DecoderTestFallback (this.DecoderFallback);
			encoding = Encoding.GetEncoding (encodingName, new EncoderReplacementFallback(), decoderFallback);
		}

		private void DecoderFallback (byte [] bytesUnknown, int index)
		{
			if (expectedUnknownBytesIndex == expectedUnknownBytes.Length)
				expectedUnknownBytesIndex = 0;

			var expectedBytes = expectedUnknownBytes [expectedUnknownBytesIndex++];
			Assert.AreEqual (expectedBytes, bytesUnknown);
		}

		public void TestDecoderFallback (byte [] data, string expectedString,  params byte [][] expectedUnknownBytes)
		{
			lock (this)
			{
				this.expectedUnknownBytes = expectedUnknownBytes;
				this.expectedUnknownBytesIndex = 0;

				Assert.AreEqual (expectedString.Length, encoding.GetCharCount (data));
				Assert.AreEqual (expectedUnknownBytesIndex, expectedUnknownBytes.Length);

				Assert.AreEqual (expectedString, encoding.GetString (data));
				Assert.AreEqual (expectedUnknownBytesIndex, expectedUnknownBytes.Length);
			}
		}
	}
}