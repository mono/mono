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
	public class FromBase64TransformTest : Assertion {

		private FromBase64Transform _algo;

		[SetUp]
		private void SetUp ()
		{
			_algo = new FromBase64Transform ();
		}

		protected void TransformFinalBlock (string name, byte[] input, byte[] expected,
						    int inputOffset, int inputCount)
		{
			byte[] output = _algo.TransformFinalBlock (input, inputOffset, inputCount);

			AssertEquals (name, expected.Length, output.Length);
			for (int i = 0; i < expected.Length; i++)
				AssertEquals (name + "(" + i + ")", expected [i], output [i]);
		}

		protected void TransformFinalBlock (string name, byte[] input, byte[] expected)
		{
			TransformFinalBlock (name, input, expected, 0, input.Length);
		}

		[Test]
		public void Properties () 
		{
			Assert ("CanReuseTransform", _algo.CanReuseTransform);
			Assert ("CanTransformMultipleBlocks", !_algo.CanTransformMultipleBlocks);
			AssertEquals ("InputBlockSize", 1, _algo.InputBlockSize);
			AssertEquals ("OutputBlockSize", 3, _algo.OutputBlockSize);
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
			byte[] expected = { 174, 90, 123, 66 };

			TransformFinalBlock ("UseTransform", input, expected);
			_algo.Clear ();
			TransformFinalBlock ("ReuseTransform", input, expected);
		}
	}
}
