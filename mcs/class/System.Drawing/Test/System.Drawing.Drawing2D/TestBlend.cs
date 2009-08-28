//
// Tests for System.Drawing.Drawing2D.Blend.cs
//
// Author:
//   Ravindra (rkumar@novell.com)
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;

namespace MonoTests.System.Drawing.Drawing2D
{
	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class BlendTest
	{
		[TearDown]
		public void TearDown () { }

		[SetUp]
		public void SetUp () { }

		[Test]
		public void TestConstructors ()
		{
			Blend blend0 = new Blend ();

			Assert.AreEqual (1, blend0.Factors.Length, "C#1");
			Assert.AreEqual (1, blend0.Positions.Length, "C#2");

			Blend blend1 = new Blend (1);

			Assert.AreEqual (1, blend1.Factors.Length, "C#3");
			Assert.AreEqual (1, blend1.Positions.Length, "C#4");
		}

		[Test]
		public void TestProperties () 
		{
			Blend blend0 = new Blend ();

			Assert.AreEqual (0, blend0.Factors[0], "P#1");
			Assert.AreEqual (0, blend0.Positions[0], "P#2");

			Blend blend1 = new Blend (1);
			float[] positions = {0.0F, 0.5F, 1.0F};
			float[] factors = {0.0F, 0.5F, 1.0F};
			blend1.Factors = factors;
			blend1.Positions = positions;

			Assert.AreEqual (factors[0], blend1.Factors[0], "P#3");
			Assert.AreEqual (factors[1], blend1.Factors[1], "P#4");
			Assert.AreEqual (factors[2], blend1.Factors[2], "P#5");
			Assert.AreEqual (positions[0], blend1.Positions[0], "P#6");
			Assert.AreEqual (positions[1], blend1.Positions[1], "P#7");
			Assert.AreEqual (positions[2], blend1.Positions[2], "P#8");
		}
	}
}
