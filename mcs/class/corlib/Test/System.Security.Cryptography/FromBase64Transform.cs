//
// TestSuite.System.Security.Cryptography.FromBase64Transform.cs
//
// Author:
//      Martin Baulig (martin@gnome.org)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell  http://www.novell.com
//

using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class FromBase64TransformTest {

		private FromBase64Transform _algo;

		[SetUp]
		public void SetUp ()
		{
			_algo = new FromBase64Transform ();
		}

		protected void TransformFinalBlock (string name, byte[] input, byte[] expected,
						    int inputOffset, int inputCount)
		{
			byte[] output = _algo.TransformFinalBlock (input, inputOffset, inputCount);

			Assert.AreEqual (expected.Length, output.Length, name);
			for (int i = 0; i < expected.Length; i++)
				Assert.AreEqual (expected [i], output [i], name + "(" + i + ")");
		}

		protected void TransformFinalBlock (string name, byte[] input, byte[] expected)
		{
			TransformFinalBlock (name, input, expected, 0, input.Length);
		}

		[Test]
		public void Properties () 
		{
			Assert.IsTrue (_algo.CanReuseTransform, "CanReuseTransform");
			Assert.IsTrue (!_algo.CanTransformMultipleBlocks, "CanTransformMultipleBlocks");
			Assert.AreEqual (1, _algo.InputBlockSize, "InputBlockSize");
			Assert.AreEqual (3, _algo.OutputBlockSize, "OutputBlockSize");
		}

		[Test]
		public void A0 ()
		{
			byte[] input = { 114, 108, 112, 55, 81, 115, 110, 69 };
			byte[] expected = { 174, 90, 123, 66, 201, 196 };

			_algo = new FromBase64Transform (FromBase64TransformMode.DoNotIgnoreWhiteSpaces);
			TransformFinalBlock ("#A0", input, expected);
		}

		[Test]
		public void A1 ()
		{
			byte[] input = { 114, 108, 112, 55, 81, 115, 110, 69 };
			byte[] expected = { 174, 90, 123, 66, 201, 196 };

			TransformFinalBlock ("#A1", input, expected);
		}

		[Test]
		public void A2 () 
		{
			byte[] input = { 114, 108, 112, 55, 81, 115, 61, 61 };
			byte[] expected = { 174, 90, 123, 66 };

			TransformFinalBlock ("#A2", input, expected);
		}

		[Test]
		public void A3 () 
		{
			byte[] input = { 114, 108, 112, 55, 81, 115, 61, 61 };
			byte[] expected = { 150, 158, 208 };

			TransformFinalBlock ("#A3", input, expected, 1, 5);
		}

		[Test]
		public void IgnoreTAB () 
		{
			byte[] input = { 9, 114, 108, 112, 55, 9, 81, 115, 61, 61, 9 };
			byte[] expected = { 174, 90, 123, 66 };

			TransformFinalBlock ("IgnoreTAB", input, expected);
		}

		[Test]
		public void IgnoreLF () 
		{
			byte[] input = { 10, 114, 108, 112, 55, 10, 81, 115, 61, 61, 10 };
			byte[] expected = { 174, 90, 123, 66 };

			TransformFinalBlock ("IgnoreLF", input, expected);
		}

		[Test]
		public void IgnoreCR () 
		{
			byte[] input = { 13, 114, 108, 112, 55, 13, 81, 115, 61, 61, 13 };
			byte[] expected = { 174, 90, 123, 66 };

			TransformFinalBlock ("IgnoreCR", input, expected);
		}

		[Test]
		public void IgnoreSPACE () 
		{
			byte[] input = { 32, 114, 108, 112, 55, 32, 81, 115, 61, 61, 32 };
			byte[] expected = { 174, 90, 123, 66 };

			TransformFinalBlock ("IgnoreSPACE", input, expected);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void DontIgnore () 
		{
			byte[] input = { 7, 114, 108, 112, 55, 81, 115, 61, 61 };
			byte[] expected = { 174, 90, 123, 66 };

			TransformFinalBlock ("DontIgnore", input, expected);
		}

		[Test]
		public void ReuseTransform () 
		{
			byte[] input = { 114, 108, 112, 55, 81, 115, 61, 61 };
			byte[] expected = { 174, 90, 123, 66 };

			TransformFinalBlock ("UseTransform", input, expected);
			TransformFinalBlock ("ReuseTransform", input, expected);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void ReuseDisposedTransform () 
		{
			byte[] input = { 114, 108, 112, 55, 81, 115, 61, 61 };
			byte[] output = new byte [16];

			_algo.Clear ();
			_algo.TransformBlock (input, 0, input.Length, output, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void ReuseDisposedTransformFinal () 
		{
			byte[] input = { 114, 108, 112, 55, 81, 115, 61, 61 };

			_algo.Clear ();
			_algo.TransformFinalBlock (input, 0, input.Length);
		}

		[Test]
		public void InvalidLength () 
		{
			byte[] input = { 114, 108, 112 };
			byte[] result = _algo.TransformFinalBlock (input, 0, input.Length);
			Assert.AreEqual (0, result.Length);
		}

		[Test]
		public void InvalidData () 
		{
			byte[] input = { 114, 108, 112, 32 };
			byte[] result = _algo.TransformFinalBlock (input, 0, input.Length);
			Assert.AreEqual (0, result.Length);
		}

		[Test]
		public void Dispose () 
		{
			byte[] input = { 114, 108, 112, 55, 81, 115, 61, 61 };
			byte[] expected = { 174, 90, 123, 66 };
			byte[] output = null;

			using (ICryptoTransform t = new FromBase64Transform ()) {
				output = t.TransformFinalBlock (input, 0, input.Length);
			}

			Assert.AreEqual (expected.Length, output.Length, "IDisposable");
			for (int i = 0; i < expected.Length; i++)
				Assert.AreEqual (expected[i], output[i], "IDisposable(" + i + ")");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformBlock_Input_Null () 
		{
			byte[] output = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformBlock (null, 0, output.Length, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformBlock_InputOffset_Negative () 
		{
			byte[] input = new byte [16];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformBlock (input, -1, input.Length, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputOffset_Overflow () 
		{
			byte[] input = new byte [16];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformBlock (input, Int32.MaxValue, input.Length, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void TransformBlock_InputCount_Negative () 
		{
			byte[] input = new byte [16];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformBlock (input, 0, -1, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (OutOfMemoryException))]
		public void TransformBlock_InputCount_Overflow () 
		{
			byte[] input = new byte [16];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformBlock (input, 0, Int32.MaxValue, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void TransformBlock_Output_Null () 
		{
			byte[] input = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformBlock (input, 0, input.Length, null, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void TransformBlock_OutputOffset_Negative () 
		{
			byte[] input = new byte [16];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformBlock (input, 0, input.Length, output, -1);
			}
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void TransformBlock_OutputOffset_Overflow () 
		{
			byte[] input = new byte [16];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformBlock (input, 0, input.Length, output, Int32.MaxValue);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformFinalBlock_Input_Null () 
		{
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformFinalBlock (null, 0, 16);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformFinalBlock_InputOffset_Negative () 
		{
			byte[] input = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformFinalBlock (input, -1, input.Length);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputOffset_Overflow () 
		{
			byte[] input = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformFinalBlock (input, Int32.MaxValue, input.Length);
			}
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void TransformFinalBlock_InputCount_Negative () 
		{
			byte[] input = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformFinalBlock (input, 0, -1);
			}
		}

		[Test]
		[ExpectedException (typeof (OutOfMemoryException))]
		public void TransformFinalBlock_InputCount_Overflow () 
		{
			byte[] input = new byte [16];
			using (ICryptoTransform t = new FromBase64Transform ()) {
				t.TransformFinalBlock (input, 0, Int32.MaxValue);
			}
		}
	}
}
