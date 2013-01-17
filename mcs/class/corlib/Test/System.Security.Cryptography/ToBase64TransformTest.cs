//
// ToBase64TransformTest.cs - NUnit Test Cases for ToBase64Transform
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class ToBase64TransformTest {

		[Test]
		public void Properties ()
		{
			ICryptoTransform t = new ToBase64Transform ();
			Assert.IsTrue (t.CanReuseTransform, "CanReuseTransform");
			Assert.IsTrue (!t.CanTransformMultipleBlocks, "CanTransformMultipleBlocks");
			Assert.AreEqual (3, t.InputBlockSize, "InputBlockSize");
			Assert.AreEqual (4, t.OutputBlockSize, "OutputBlockSize");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformBlock_NullInput () 
		{
			byte[] output = new byte [4];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformBlock (null, 0, 0, output, 0);
		}

		[Test]
		public void TransformBlock_WrongLength () 
		{
			byte[] input = new byte [6];
			byte[] output = new byte [8];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformBlock (input, 0, 6, output, 0);
			// note only the first block has been processed
			Assert.AreEqual ("41-41-41-41-00-00-00-00", BitConverter.ToString (output));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformBlock_NullOutput () 
		{
			byte[] input = new byte [3];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformBlock (input, 0, 3, null, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void TransformBlock_Dispose () 
		{
			byte[] input = new byte [3];
			byte[] output = new byte [4];
			ToBase64Transform t = new ToBase64Transform ();
			t.Clear ();
			t.TransformBlock (input, 0, input.Length, output, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformFinalBlock_Null () 
		{
			byte[] input = new byte [3];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformFinalBlock (null, 0, 3);
		}

		[Test]
		public void TransformFinalBlock_SmallLength () 
		{
			byte[] input = new byte [2]; // smaller than InputBlockSize
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformFinalBlock (input, 0, 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformFinalBlock_WrongLength () 
		{
			byte[] input = new byte [6];
			ToBase64Transform t = new ToBase64Transform ();
			t.TransformFinalBlock (input, 0, 6);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void TransformFinalBlock_Dispose () 
		{
			byte[] input = new byte [3];
			ToBase64Transform t = new ToBase64Transform ();
			t.Clear ();
			t.TransformFinalBlock (input, 0, input.Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformBlock_InputOffset_Negative () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, -1, input.Length, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputOffset_Overflow () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, Int32.MaxValue, input.Length, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputCount_Negative () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, 0, -1, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_InputCount_Overflow () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, 0, Int32.MaxValue, output, 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformBlock_OutputOffset_Negative () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, 0, input.Length, output, -1);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformBlock_OutputOffset_Overflow () 
		{
			byte[] input = new byte [15];
			byte[] output = new byte [16];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformBlock (input, 0, input.Length, output, Int32.MaxValue);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TransformFinalBlock_Input_Null () 
		{
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (null, 0, 15);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TransformFinalBlock_InputOffset_Negative () 
		{
			byte[] input = new byte [15];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (input, -1, input.Length);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputOffset_Overflow () 
		{
			byte[] input = new byte [15];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (input, Int32.MaxValue, input.Length);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputCount_Negative () 
		{
			byte[] input = new byte [15];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (input, 0, -1);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TransformFinalBlock_InputCount_Overflow () 
		{
			byte[] input = new byte [15];
			using (ICryptoTransform t = new ToBase64Transform ()) {
				t.TransformFinalBlock (input, 0, Int32.MaxValue);
			}
		}
	}
}
