//
// TestSuite.System.Security.Cryptography.FromBase64Transform.cs
//
// Author:
//      Martin Baulig (martin@gnome.org)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	public class FromBase64TransformTest : TestCase {
		private FromBase64Transform _algo;

		protected override void SetUp() {
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

		public void TestFinalBlock ()
		{
			{
				byte[] input = { 114, 108, 112, 55, 81, 115, 110, 69 };
				byte[] expected = { 174, 90, 123, 66, 201, 196 };

				TransformFinalBlock ("#A1", input, expected);
			}
			{
				byte[] input = { 114, 108, 112, 55, 81, 115, 61, 61 };
				byte[] expected = { 174, 90, 123, 66 };

				TransformFinalBlock ("#A2", input, expected);
			}
			{
				byte[] input = { 114, 108, 112, 55, 81, 115, 61, 61 };
				byte[] expected = { 150, 158, 208 };

				TransformFinalBlock ("#A3", input, expected, 1, 5);
			}
		}

	}
}
