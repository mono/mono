//
// Tests for System.Drawing.Drawing2D.Blend.cs
//
// Author:
//   Ravindra (rkumar@novell.com)
//
// Copyright (C) 2004 Novell, Inc. http://www.novell.com
//

using NUnit.Framework;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	public class BlendTest : Assertion
	{

		[TearDown]
		public void TearDown () { }

		[SetUp]
		public void SetUp () { }

		[Test]
		public void TestConstructors ()
		{
			Blend blend0 = new Blend ();

			AssertEquals ("C#1", 1, blend0.Factors.Length);
			AssertEquals ("C#2", 1, blend0.Positions.Length);

			Blend blend1 = new Blend (1);

			AssertEquals ("C#3", 1, blend1.Factors.Length);
			AssertEquals ("C#4", 1, blend1.Positions.Length);
		}

		[Test]
		public void TestProperties () 
		{
			Blend blend0 = new Blend ();

			AssertEquals ("P#1", 0, blend0.Factors[0]);
			AssertEquals ("P#2", 0, blend0.Positions[0]);

			Blend blend1 = new Blend (1);
			float[] positions = {0.0F, 0.5F, 1.0F};
			float[] factors = {0.0F, 0.5F, 1.0F};
			blend1.Factors = factors;
			blend1.Positions = positions;

			AssertEquals ("P#3", factors[0], blend1.Factors[0]);
			AssertEquals ("P#4", factors[1], blend1.Factors[1]);
			AssertEquals ("P#5", factors[2], blend1.Factors[2]);
			AssertEquals ("P#6", positions[0], blend1.Positions[0]);
			AssertEquals ("P#7", positions[1], blend1.Positions[1]);
			AssertEquals ("P#8", positions[2], blend1.Positions[2]);
		}
	}
}
