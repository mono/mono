//
// Tests for System.Drawing.Drawing2D.ColorBlend.cs
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
	public class ColorBlendTest : Assertion
	{

		[TearDown]
		public void TearDown () { }

		[SetUp]
		public void SetUp () { }

		[Test]
		public void TestConstructors ()
		{
			ColorBlend cb0 = new ColorBlend ();

			AssertEquals ("C#1", 1, cb0.Colors.Length);
			AssertEquals ("C#2", 1, cb0.Positions.Length);

			ColorBlend cb1 = new ColorBlend (1);

			AssertEquals ("C#3", 1, cb1.Colors.Length);
			AssertEquals ("C#4", 1, cb1.Positions.Length);
		}

		[Test]
		public void TestProperties () 
		{
			ColorBlend cb0 = new ColorBlend ();

			AssertEquals ("P#1", Color.Empty, cb0.Colors[0]);
			AssertEquals ("P#2", 0, cb0.Positions[0]);

			ColorBlend cb1 = new ColorBlend (1);
			float[] positions = {0.0F, 0.5F, 1.0F};
			Color[] colors = {Color.Red, Color.White, Color.Black};
			cb1.Colors = colors;
			cb1.Positions = positions;

			AssertEquals ("P#3", colors[0], cb1.Colors[0]);
			AssertEquals ("P#4", colors[1], cb1.Colors[1]);
			AssertEquals ("P#5", colors[2], cb1.Colors[2]);
			AssertEquals ("P#6", positions[0], cb1.Positions[0]);
			AssertEquals ("P#7", positions[1], cb1.Positions[1]);
			AssertEquals ("P#8", positions[2], cb1.Positions[2]);
		}
	}
}
