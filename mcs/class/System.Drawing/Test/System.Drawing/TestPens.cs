// Tests for System.Drawing.Pens.cs
//
// Author:
//     Ravindra (rkumar@novell.com)
//

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

namespace MonoTests.System.Drawing
{
	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class PensTest : Assertion
	{
		[SetUp]
		public void SetUp () { }

		[TearDown]
		public void TearDown () { }
		
		[Test]
		public void TestEquals ()
		{
			Pen pen1 = Pens.Blue;
			Pen pen2 = Pens.Blue;
			
			AssertEquals ("Equals", true, pen1.Equals (pen2));			
		}

		[Test]
		public void TestAliceBlue ()
		{
			Pen pen = Pens.AliceBlue;
			AssertEquals ("P1#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P1#2", pen.Color, Color.AliceBlue);

			try {
				pen.Color = Color.AliceBlue;
				Fail ("P1#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P1#3", true);
			}
		}

		[Test]
		public void TestAntiqueWhite ()
		{
			Pen pen = Pens.AntiqueWhite;
			AssertEquals ("P2#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P2#2", pen.Color, Color.AntiqueWhite);

			try {
				pen.Color = Color.AntiqueWhite;
				Fail ("P2#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P2#3", true);
			}
		}

		[Test]
		public void TestAqua ()
		{
			Pen pen = Pens.Aqua;
			AssertEquals ("P3#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P3#2", pen.Color, Color.Aqua);

			try {
				pen.Color = Color.Aqua;
				Fail ("P3#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P3#3", true);
			}
		}

		[Test]
		public void TestAquamarine ()
		{
			Pen pen = Pens.Aquamarine;
			AssertEquals ("P4#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P4#2", pen.Color, Color.Aquamarine);

			try {
				pen.Color = Color.Aquamarine;
				Fail ("P4#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P4#3", true);
			}
		}

		[Test]
		public void TestAzure ()
		{
			Pen pen = Pens.Azure;
			AssertEquals ("P5#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P5#2", pen.Color, Color.Azure);

			try {
				pen.Color = Color.Azure;
				Fail ("P5#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P5#3", true);
			}
		}

		[Test]
		public void TestBeige ()
		{
			Pen pen = Pens.Beige;
			AssertEquals ("P6#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P6#2", pen.Color, Color.Beige);

			try {
				pen.Color = Color.Beige;
				Fail ("P6#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P6#3", true);
			}
		}

		[Test]
		public void TestBisque ()
		{
			Pen pen = Pens.Bisque;
			AssertEquals ("P7#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P7#2", pen.Color, Color.Bisque);

			try {
				pen.Color = Color.Bisque;
				Fail ("P7#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P7#3", true);
			}
		}

		[Test]
		public void TestBlack ()
		{
			Pen pen = Pens.Black;
			AssertEquals ("P8#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P8#2", pen.Color, Color.Black);

			try {
				pen.Color = Color.Black;
				Fail ("P8#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P8#3", true);
			}
		}

		[Test]
		public void TestBlanchedAlmond ()
		{
			Pen pen = Pens.BlanchedAlmond;
			AssertEquals ("P9#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P9#2", pen.Color, Color.BlanchedAlmond);

			try {
				pen.Color = Color.BlanchedAlmond;
				Fail ("P9#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P9#3", true);
			}
		}

		[Test]
		public void TestBlue ()
		{
			Pen pen = Pens.Blue;
			AssertEquals ("P10#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P10#2", pen.Color, Color.Blue);

			try {
				pen.Color = Color.Blue;
				Fail ("P10#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P10#3", true);
			}
		}

		[Test]
		public void TestBlueViolet ()
		{
			Pen pen = Pens.BlueViolet;
			AssertEquals ("P11#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P11#2", pen.Color, Color.BlueViolet);

			try {
				pen.Color = Color.BlueViolet;
				Fail ("P11#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P11#3", true);
			}
		}

		[Test]
		public void TestBrown ()
		{
			Pen pen = Pens.Brown;
			AssertEquals ("P12#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P12#2", pen.Color, Color.Brown);

			try {
				pen.Color = Color.Brown;
				Fail ("P12#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P12#3", true);
			}
		}

		[Test]
		public void TestBurlyWood ()
		{
			Pen pen = Pens.BurlyWood;
			AssertEquals ("P13#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P13#2", pen.Color, Color.BurlyWood);

			try {
				pen.Color = Color.BurlyWood;
				Fail ("P13#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P13#3", true);
			}
		}

		[Test]
		public void TestCadetBlue ()
		{
			Pen pen = Pens.CadetBlue;
			AssertEquals ("P14#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P14#2", pen.Color, Color.CadetBlue);

			try {
				pen.Color = Color.CadetBlue;
				Fail ("P14#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P14#3", true);
			}
		}

		[Test]
		public void TestChartreuse ()
		{
			Pen pen = Pens.Chartreuse;
			AssertEquals ("P15#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P15#2", pen.Color, Color.Chartreuse);

			try {
				pen.Color = Color.Chartreuse;
				Fail ("P15#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P15#3", true);
			}
		}

		[Test]
		public void TestChocolate ()
		{
			Pen pen = Pens.Chocolate;
			AssertEquals ("P16#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P16#2", pen.Color, Color.Chocolate);

			try {
				pen.Color = Color.Chocolate;
				Fail ("P16#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P16#3", true);
			}
		}

		[Test]
		public void TestCoral ()
		{
			Pen pen = Pens.Coral;
			AssertEquals ("P17#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P17#2", pen.Color, Color.Coral);

			try {
				pen.Color = Color.Coral;
				Fail ("P17#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P17#3", true);
			}
		}

		[Test]
		public void TestCornflowerBlue ()
		{
			Pen pen = Pens.CornflowerBlue;
			AssertEquals ("P18#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P18#2", pen.Color, Color.CornflowerBlue);

			try {
				pen.Color = Color.CornflowerBlue;
				Fail ("P18#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P18#3", true);
			}
		}

		[Test]
		public void TestCornsilk ()
		{
			Pen pen = Pens.Cornsilk;
			AssertEquals ("P19#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P19#2", pen.Color, Color.Cornsilk);

			try {
				pen.Color = Color.Cornsilk;
				Fail ("P19#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P19#3", true);
			}
		}

		[Test]
		public void TestCrimson ()
		{
			Pen pen = Pens.Crimson;
			AssertEquals ("P20#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P20#2", pen.Color, Color.Crimson);

			try {
				pen.Color = Color.Crimson;
				Fail ("P20#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P20#3", true);
			}
		}

		[Test]
		public void TestCyan ()
		{
			Pen pen = Pens.Cyan;
			AssertEquals ("P21#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P21#2", pen.Color, Color.Cyan);

			try {
				pen.Color = Color.Cyan;
				Fail ("P21#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P21#3", true);
			}
		}

		[Test]
		public void TestDarkBlue ()
		{
			Pen pen = Pens.DarkBlue;
			AssertEquals ("P22#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P22#2", pen.Color, Color.DarkBlue);

			try {
				pen.Color = Color.DarkBlue;
				Fail ("P22#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P22#3", true);
			}
		}

		[Test]
		public void TestDarkCyan ()
		{
			Pen pen = Pens.DarkCyan;
			AssertEquals ("P23#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P23#2", pen.Color, Color.DarkCyan);

			try {
				pen.Color = Color.DarkCyan;
				Fail ("P23#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P23#3", true);
			}
		}

		[Test]
		public void TestDarkGoldenrod ()
		{
			Pen pen = Pens.DarkGoldenrod;
			AssertEquals ("P24#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P24#2", pen.Color, Color.DarkGoldenrod);

			try {
				pen.Color = Color.DarkGoldenrod;
				Fail ("P24#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P24#3", true);
			}
		}

		[Test]
		public void TestDarkGray ()
		{
			Pen pen = Pens.DarkGray;
			AssertEquals ("P25#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P25#2", pen.Color, Color.DarkGray);

			try {
				pen.Color = Color.DarkGray;
				Fail ("P25#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P25#3", true);
			}
		}

		[Test]
		public void TestDarkGreen ()
		{
			Pen pen = Pens.DarkGreen;
			AssertEquals ("P26#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P26#2", pen.Color, Color.DarkGreen);

			try {
				pen.Color = Color.DarkGreen;
				Fail ("P26#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P26#3", true);
			}
		}

		[Test]
		public void TestDarkKhaki ()
		{
			Pen pen = Pens.DarkKhaki;
			AssertEquals ("P27#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P27#2", pen.Color, Color.DarkKhaki);

			try {
				pen.Color = Color.DarkKhaki;
				Fail ("P27#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P27#3", true);
			}
		}

		[Test]
		public void TestDarkMagenta ()
		{
			Pen pen = Pens.DarkMagenta;
			AssertEquals ("P28#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P28#2", pen.Color, Color.DarkMagenta);

			try {
				pen.Color = Color.DarkMagenta;
				Fail ("P28#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P28#3", true);
			}
		}

		[Test]
		public void TestDarkOliveGreen ()
		{
			Pen pen = Pens.DarkOliveGreen;
			AssertEquals ("P29#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P29#2", pen.Color, Color.DarkOliveGreen);

			try {
				pen.Color = Color.DarkOliveGreen;
				Fail ("P29#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P29#3", true);
			}
		}

		[Test]
		public void TestDarkOrange ()
		{
			Pen pen = Pens.DarkOrange;
			AssertEquals ("P30#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P30#2", pen.Color, Color.DarkOrange);

			try {
				pen.Color = Color.DarkOrange;
				Fail ("P30#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P30#3", true);
			}
		}

		[Test]
		public void TestDarkOrchid ()
		{
			Pen pen = Pens.DarkOrchid;
			AssertEquals ("P31#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P31#2", pen.Color, Color.DarkOrchid);

			try {
				pen.Color = Color.DarkOrchid;
				Fail ("P31#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P31#3", true);
			}
		}

		[Test]
		public void TestDarkRed ()
		{
			Pen pen = Pens.DarkRed;
			AssertEquals ("P32#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P32#2", pen.Color, Color.DarkRed);

			try {
				pen.Color = Color.DarkRed;
				Fail ("P32#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P32#3", true);
			}
		}

		[Test]
		public void TestDarkSalmon ()
		{
			Pen pen = Pens.DarkSalmon;
			AssertEquals ("P33#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P33#2", pen.Color, Color.DarkSalmon);

			try {
				pen.Color = Color.DarkSalmon;
				Fail ("P33#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P33#3", true);
			}
		}

		[Test]
		public void TestDarkSeaGreen ()
		{
			Pen pen = Pens.DarkSeaGreen;
			AssertEquals ("P34#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P34#2", pen.Color, Color.DarkSeaGreen);

			try {
				pen.Color = Color.DarkSeaGreen;
				Fail ("P34#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P34#3", true);
			}
		}

		[Test]
		public void TestDarkSlateBlue ()
		{
			Pen pen = Pens.DarkSlateBlue;
			AssertEquals ("P35#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P35#2", pen.Color, Color.DarkSlateBlue);

			try {
				pen.Color = Color.DarkSlateBlue;
				Fail ("P35#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P35#3", true);
			}
		}

		[Test]
		public void TestDarkSlateGray ()
		{
			Pen pen = Pens.DarkSlateGray;
			AssertEquals ("P36#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P36#2", pen.Color, Color.DarkSlateGray);

			try {
				pen.Color = Color.DarkSlateGray;
				Fail ("P36#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P36#3", true);
			}
		}

		[Test]
		public void TestDarkTurquoise ()
		{
			Pen pen = Pens.DarkTurquoise;
			AssertEquals ("P37#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P37#2", pen.Color, Color.DarkTurquoise);

			try {
				pen.Color = Color.DarkTurquoise;
				Fail ("P37#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P37#3", true);
			}
		}

		[Test]
		public void TestDarkViolet ()
		{
			Pen pen = Pens.DarkViolet;
			AssertEquals ("P38#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P38#2", pen.Color, Color.DarkViolet);

			try {
				pen.Color = Color.DarkViolet;
				Fail ("P38#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P38#3", true);
			}
		}

		[Test]
		public void TestDeepPink ()
		{
			Pen pen = Pens.DeepPink;
			AssertEquals ("P39#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P39#2", pen.Color, Color.DeepPink);

			try {
				pen.Color = Color.DeepPink;
				Fail ("P39#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P39#3", true);
			}
		}

		[Test]
		public void TestDeepSkyBlue ()
		{
			Pen pen = Pens.DeepSkyBlue;
			AssertEquals ("P40#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P40#2", pen.Color, Color.DeepSkyBlue);

			try {
				pen.Color = Color.DeepSkyBlue;
				Fail ("P40#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P40#3", true);
			}
		}

		[Test]
		public void TestDimGray ()
		{
			Pen pen = Pens.DimGray;
			AssertEquals ("P41#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P41#2", pen.Color, Color.DimGray);

			try {
				pen.Color = Color.DimGray;
				Fail ("P41#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P41#3", true);
			}
		}

		[Test]
		public void TestDodgerBlue ()
		{
			Pen pen = Pens.DodgerBlue;
			AssertEquals ("P42#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P42#2", pen.Color, Color.DodgerBlue);

			try {
				pen.Color = Color.DodgerBlue;
				Fail ("P42#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P42#3", true);
			}
		}

		[Test]
		public void TestFirebrick ()
		{
			Pen pen = Pens.Firebrick;
			AssertEquals ("P43#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P43#2", pen.Color, Color.Firebrick);

			try {
				pen.Color = Color.Firebrick;
				Fail ("P43#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P43#3", true);
			}
		}

		[Test]
		public void TestFloralWhite ()
		{
			Pen pen = Pens.FloralWhite;
			AssertEquals ("P44#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P44#2", pen.Color, Color.FloralWhite);

			try {
				pen.Color = Color.FloralWhite;
				Fail ("P44#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P44#3", true);
			}
		}

		[Test]
		public void TestForestGreen ()
		{
			Pen pen = Pens.ForestGreen;
			AssertEquals ("P45#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P45#2", pen.Color, Color.ForestGreen);

			try {
				pen.Color = Color.ForestGreen;
				Fail ("P45#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P45#3", true);
			}
		}

		[Test]
		public void TestFuchsia ()
		{
			Pen pen = Pens.Fuchsia;
			AssertEquals ("P46#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P46#2", pen.Color, Color.Fuchsia);

			try {
				pen.Color = Color.Fuchsia;
				Fail ("P46#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P46#3", true);
			}
		}

		[Test]
		public void TestGainsboro ()
		{
			Pen pen = Pens.Gainsboro;
			AssertEquals ("P47#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P47#2", pen.Color, Color.Gainsboro);

			try {
				pen.Color = Color.Gainsboro;
				Fail ("P47#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P47#3", true);
			}
		}

		[Test]
		public void TestGhostWhite ()
		{
			Pen pen = Pens.GhostWhite;
			AssertEquals ("P48#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P48#2", pen.Color, Color.GhostWhite);

			try {
				pen.Color = Color.GhostWhite;
				Fail ("P48#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P48#3", true);
			}
		}

		[Test]
		public void TestGold ()
		{
			Pen pen = Pens.Gold;
			AssertEquals ("P49#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P49#2", pen.Color, Color.Gold);

			try {
				pen.Color = Color.Gold;
				Fail ("P49#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P49#3", true);
			}
		}

		[Test]
		public void TestGoldenrod ()
		{
			Pen pen = Pens.Goldenrod;
			AssertEquals ("P50#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P50#2", pen.Color, Color.Goldenrod);

			try {
				pen.Color = Color.Goldenrod;
				Fail ("P50#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P50#3", true);
			}
		}

		[Test]
		public void TestGray ()
		{
			Pen pen = Pens.Gray;
			AssertEquals ("P51#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P51#2", pen.Color, Color.Gray);

			try {
				pen.Color = Color.Gray;
				Fail ("P51#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P51#3", true);
			}
		}

		[Test]
		public void TestGreen ()
		{
			Pen pen = Pens.Green;
			AssertEquals ("P52#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P52#2", pen.Color, Color.Green);

			try {
				pen.Color = Color.Green;
				Fail ("P52#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P52#3", true);
			}
		}

		[Test]
		public void TestGreenYellow ()
		{
			Pen pen = Pens.GreenYellow;
			AssertEquals ("P53#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P53#2", pen.Color, Color.GreenYellow);

			try {
				pen.Color = Color.GreenYellow;
				Fail ("P53#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P53#3", true);
			}
		}

		[Test]
		public void TestHoneydew ()
		{
			Pen pen = Pens.Honeydew;
			AssertEquals ("P54#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P54#2", pen.Color, Color.Honeydew);

			try {
				pen.Color = Color.Honeydew;
				Fail ("P54#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P54#3", true);
			}
		}

		[Test]
		public void TestHotPink ()
		{
			Pen pen = Pens.HotPink;
			AssertEquals ("P55#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P55#2", pen.Color, Color.HotPink);

			try {
				pen.Color = Color.HotPink;
				Fail ("P55#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P55#3", true);
			}
		}

		[Test]
		public void TestIndianRed ()
		{
			Pen pen = Pens.IndianRed;
			AssertEquals ("P56#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P56#2", pen.Color, Color.IndianRed);

			try {
				pen.Color = Color.IndianRed;
				Fail ("P56#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P56#3", true);
			}
		}

		[Test]
		public void TestIndigo ()
		{
			Pen pen = Pens.Indigo;
			AssertEquals ("P57#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P57#2", pen.Color, Color.Indigo);

			try {
				pen.Color = Color.Indigo;
				Fail ("P57#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P57#3", true);
			}
		}

		[Test]
		public void TestIvory ()
		{
			Pen pen = Pens.Ivory;
			AssertEquals ("P58#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P58#2", pen.Color, Color.Ivory);

			try {
				pen.Color = Color.Ivory;
				Fail ("P58#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P58#3", true);
			}
		}

		[Test]
		public void TestKhaki ()
		{
			Pen pen = Pens.Khaki;
			AssertEquals ("P59#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P59#2", pen.Color, Color.Khaki);

			try {
				pen.Color = Color.Khaki;
				Fail ("P59#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P59#3", true);
			}
		}

		[Test]
		public void TestLavender ()
		{
			Pen pen = Pens.Lavender;
			AssertEquals ("P60#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P60#2", pen.Color, Color.Lavender);

			try {
				pen.Color = Color.Lavender;
				Fail ("P60#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P60#3", true);
			}
		}

		[Test]
		public void TestLavenderBlush ()
		{
			Pen pen = Pens.LavenderBlush;
			AssertEquals ("P61#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P61#2", pen.Color, Color.LavenderBlush);

			try {
				pen.Color = Color.LavenderBlush;
				Fail ("P61#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P61#3", true);
			}
		}

		[Test]
		public void TestLawnGreen ()
		{
			Pen pen = Pens.LawnGreen;
			AssertEquals ("P62#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P62#2", pen.Color, Color.LawnGreen);

			try {
				pen.Color = Color.LawnGreen;
				Fail ("P62#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P62#3", true);
			}
		}

		[Test]
		public void TestLemonChiffon ()
		{
			Pen pen = Pens.LemonChiffon;
			AssertEquals ("P63#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P63#2", pen.Color, Color.LemonChiffon);

			try {
				pen.Color = Color.LemonChiffon;
				Fail ("P63#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P63#3", true);
			}
		}

		[Test]
		public void TestLightBlue ()
		{
			Pen pen = Pens.LightBlue;
			AssertEquals ("P64#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P64#2", pen.Color, Color.LightBlue);

			try {
				pen.Color = Color.LightBlue;
				Fail ("P64#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P64#3", true);
			}
		}

		[Test]
		public void TestLightCoral ()
		{
			Pen pen = Pens.LightCoral;
			AssertEquals ("P65#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P65#2", pen.Color, Color.LightCoral);

			try {
				pen.Color = Color.LightCoral;
				Fail ("P65#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P65#3", true);
			}
		}

		[Test]
		public void TestLightCyan ()
		{
			Pen pen = Pens.LightCyan;
			AssertEquals ("P66#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P66#2", pen.Color, Color.LightCyan);

			try {
				pen.Color = Color.LightCyan;
				Fail ("P66#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P66#3", true);
			}
		}

		[Test]
		public void TestLightGoldenrodYellow ()
		{
			Pen pen = Pens.LightGoldenrodYellow;
			AssertEquals ("P67#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P67#2", pen.Color, Color.LightGoldenrodYellow);

			try {
				pen.Color = Color.LightGoldenrodYellow;
				Fail ("P67#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P67#3", true);
			}
		}

		[Test]
		public void TestLightGray ()
		{
			Pen pen = Pens.LightGray;
			AssertEquals ("P68#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P68#2", pen.Color, Color.LightGray);

			try {
				pen.Color = Color.LightGray;
				Fail ("P68#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P68#3", true);
			}
		}

		[Test]
		public void TestLightGreen ()
		{
			Pen pen = Pens.LightGreen;
			AssertEquals ("P69#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P69#2", pen.Color, Color.LightGreen);

			try {
				pen.Color = Color.LightGreen;
				Fail ("P69#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P69#3", true);
			}
		}

		[Test]
		public void TestLightPink ()
		{
			Pen pen = Pens.LightPink;
			AssertEquals ("P70#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P70#2", pen.Color, Color.LightPink);

			try {
				pen.Color = Color.LightPink;
				Fail ("P70#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P70#3", true);
			}
		}

		[Test]
		public void TestLightSalmon ()
		{
			Pen pen = Pens.LightSalmon;
			AssertEquals ("P71#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P71#2", pen.Color, Color.LightSalmon);

			try {
				pen.Color = Color.LightSalmon;
				Fail ("P71#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P71#3", true);
			}
		}

		[Test]
		public void TestLightSeaGreen ()
		{
			Pen pen = Pens.LightSeaGreen;
			AssertEquals ("P72#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P72#2", pen.Color, Color.LightSeaGreen);

			try {
				pen.Color = Color.LightSeaGreen;
				Fail ("P72#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P72#3", true);
			}
		}

		[Test]
		public void TestLightSkyBlue ()
		{
			Pen pen = Pens.LightSkyBlue;
			AssertEquals ("P73#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P73#2", pen.Color, Color.LightSkyBlue);

			try {
				pen.Color = Color.LightSkyBlue;
				Fail ("P73#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P73#3", true);
			}
		}

		[Test]
		public void TestLightSlateGray ()
		{
			Pen pen = Pens.LightSlateGray;
			AssertEquals ("P74#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P74#2", pen.Color, Color.LightSlateGray);

			try {
				pen.Color = Color.LightSlateGray;
				Fail ("P74#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P74#3", true);
			}
		}

		[Test]
		public void TestLightSteelBlue ()
		{
			Pen pen = Pens.LightSteelBlue;
			AssertEquals ("P75#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P75#2", pen.Color, Color.LightSteelBlue);

			try {
				pen.Color = Color.LightSteelBlue;
				Fail ("P75#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P75#3", true);
			}
		}

		[Test]
		public void TestLightYellow ()
		{
			Pen pen = Pens.LightYellow;
			AssertEquals ("P76#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P76#2", pen.Color, Color.LightYellow);

			try {
				pen.Color = Color.LightYellow;
				Fail ("P76#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P76#3", true);
			}
		}

		[Test]
		public void TestLime ()
		{
			Pen pen = Pens.Lime;
			AssertEquals ("P77#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P77#2", pen.Color, Color.Lime);

			try {
				pen.Color = Color.Lime;
				Fail ("P77#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P77#3", true);
			}
		}

		[Test]
		public void TestLimeGreen ()
		{
			Pen pen = Pens.LimeGreen;
			AssertEquals ("P78#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P78#2", pen.Color, Color.LimeGreen);

			try {
				pen.Color = Color.LimeGreen;
				Fail ("P78#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P78#3", true);
			}
		}

		[Test]
		public void TestLinen ()
		{
			Pen pen = Pens.Linen;
			AssertEquals ("P79#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P79#2", pen.Color, Color.Linen);

			try {
				pen.Color = Color.Linen;
				Fail ("P79#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P79#3", true);
			}
		}

		[Test]
		public void TestMagenta ()
		{
			Pen pen = Pens.Magenta;
			AssertEquals ("P80#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P80#2", pen.Color, Color.Magenta);

			try {
				pen.Color = Color.Magenta;
				Fail ("P80#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P80#3", true);
			}
		}

		[Test]
		public void TestMaroon ()
		{
			Pen pen = Pens.Maroon;
			AssertEquals ("P81#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P81#2", pen.Color, Color.Maroon);

			try {
				pen.Color = Color.Maroon;
				Fail ("P81#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P81#3", true);
			}
		}

		[Test]
		public void TestMediumAquamarine ()
		{
			Pen pen = Pens.MediumAquamarine;
			AssertEquals ("P82#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P82#2", pen.Color, Color.MediumAquamarine);

			try {
				pen.Color = Color.MediumAquamarine;
				Fail ("P82#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P82#3", true);
			}
		}

		[Test]
		public void TestMediumBlue ()
		{
			Pen pen = Pens.MediumBlue;
			AssertEquals ("P83#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P83#2", pen.Color, Color.MediumBlue);

			try {
				pen.Color = Color.MediumBlue;
				Fail ("P83#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P83#3", true);
			}
		}

		[Test]
		public void TestMediumOrchid ()
		{
			Pen pen = Pens.MediumOrchid;
			AssertEquals ("P84#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P84#2", pen.Color, Color.MediumOrchid);

			try {
				pen.Color = Color.MediumOrchid;
				Fail ("P84#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P84#3", true);
			}
		}

		[Test]
		public void TestMediumPurple ()
		{
			Pen pen = Pens.MediumPurple;
			AssertEquals ("P85#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P85#2", pen.Color, Color.MediumPurple);

			try {
				pen.Color = Color.MediumPurple;
				Fail ("P85#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P85#3", true);
			}
		}

		[Test]
		public void TestMediumSeaGreen ()
		{
			Pen pen = Pens.MediumSeaGreen;
			AssertEquals ("P86#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P86#2", pen.Color, Color.MediumSeaGreen);

			try {
				pen.Color = Color.MediumSeaGreen;
				Fail ("P86#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P86#3", true);
			}
		}

		[Test]
		public void TestMediumSlateBlue ()
		{
			Pen pen = Pens.MediumSlateBlue;
			AssertEquals ("P87#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P87#2", pen.Color, Color.MediumSlateBlue);

			try {
				pen.Color = Color.MediumSlateBlue;
				Fail ("P87#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P87#3", true);
			}
		}

		[Test]
		public void TestMediumSpringGreen ()
		{
			Pen pen = Pens.MediumSpringGreen;
			AssertEquals ("P88#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P88#2", pen.Color, Color.MediumSpringGreen);

			try {
				pen.Color = Color.MediumSpringGreen;
				Fail ("P88#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P88#3", true);
			}
		}

		[Test]
		public void TestMediumTurquoise ()
		{
			Pen pen = Pens.MediumTurquoise;
			AssertEquals ("P89#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P89#2", pen.Color, Color.MediumTurquoise);

			try {
				pen.Color = Color.MediumTurquoise;
				Fail ("P89#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P89#3", true);
			}
		}

		[Test]
		public void TestMediumVioletRed ()
		{
			Pen pen = Pens.MediumVioletRed;
			AssertEquals ("P90#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P90#2", pen.Color, Color.MediumVioletRed);

			try {
				pen.Color = Color.MediumVioletRed;
				Fail ("P90#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P90#3", true);
			}
		}

		[Test]
		public void TestMidnightBlue ()
		{
			Pen pen = Pens.MidnightBlue;
			AssertEquals ("P91#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P91#2", pen.Color, Color.MidnightBlue);

			try {
				pen.Color = Color.MidnightBlue;
				Fail ("P91#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P91#3", true);
			}
		}

		[Test]
		public void TestMintCream ()
		{
			Pen pen = Pens.MintCream;
			AssertEquals ("P92#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P92#2", pen.Color, Color.MintCream);

			try {
				pen.Color = Color.MintCream;
				Fail ("P92#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P92#3", true);
			}
		}

		[Test]
		public void TestMistyRose ()
		{
			Pen pen = Pens.MistyRose;
			AssertEquals ("P93#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P93#2", pen.Color, Color.MistyRose);

			try {
				pen.Color = Color.MistyRose;
				Fail ("P93#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P93#3", true);
			}
		}

		[Test]
		public void TestMoccasin ()
		{
			Pen pen = Pens.Moccasin;
			AssertEquals ("P94#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P94#2", pen.Color, Color.Moccasin);

			try {
				pen.Color = Color.Moccasin;
				Fail ("P94#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P94#3", true);
			}
		}

		[Test]
		public void TestNavajoWhite ()
		{
			Pen pen = Pens.NavajoWhite;
			AssertEquals ("P95#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P95#2", pen.Color, Color.NavajoWhite);

			try {
				pen.Color = Color.NavajoWhite;
				Fail ("P95#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P95#3", true);
			}
		}

		[Test]
		public void TestNavy ()
		{
			Pen pen = Pens.Navy;
			AssertEquals ("P96#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P96#2", pen.Color, Color.Navy);

			try {
				pen.Color = Color.Navy;
				Fail ("P96#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P96#3", true);
			}
		}

		[Test]
		public void TestOldLace ()
		{
			Pen pen = Pens.OldLace;
			AssertEquals ("P97#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P97#2", pen.Color, Color.OldLace);

			try {
				pen.Color = Color.OldLace;
				Fail ("P97#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P97#3", true);
			}
		}

		[Test]
		public void TestOlive ()
		{
			Pen pen = Pens.Olive;
			AssertEquals ("P98#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P98#2", pen.Color, Color.Olive);

			try {
				pen.Color = Color.Olive;
				Fail ("P98#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P98#3", true);
			}
		}

		[Test]
		public void TestOliveDrab ()
		{
			Pen pen = Pens.OliveDrab;
			AssertEquals ("P99#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P99#2", pen.Color, Color.OliveDrab);

			try {
				pen.Color = Color.OliveDrab;
				Fail ("P99#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P99#3", true);
			}
		}

		[Test]
		public void TestOrange ()
		{
			Pen pen = Pens.Orange;
			AssertEquals ("P100#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P100#2", pen.Color, Color.Orange);

			try {
				pen.Color = Color.Orange;
				Fail ("P100#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P100#3", true);
			}
		}

		[Test]
		public void TestOrangeRed ()
		{
			Pen pen = Pens.OrangeRed;
			AssertEquals ("P101#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P101#2", pen.Color, Color.OrangeRed);

			try {
				pen.Color = Color.OrangeRed;
				Fail ("P101#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P101#3", true);
			}
		}

		[Test]
		public void TestOrchid ()
		{
			Pen pen = Pens.Orchid;
			AssertEquals ("P102#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P102#2", pen.Color, Color.Orchid);

			try {
				pen.Color = Color.Orchid;
				Fail ("P102#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P102#3", true);
			}
		}

		[Test]
		public void TestPaleGoldenrod ()
		{
			Pen pen = Pens.PaleGoldenrod;
			AssertEquals ("P103#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P103#2", pen.Color, Color.PaleGoldenrod);

			try {
				pen.Color = Color.PaleGoldenrod;
				Fail ("P103#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P103#3", true);
			}
		}

		[Test]
		public void TestPaleGreen ()
		{
			Pen pen = Pens.PaleGreen;
			AssertEquals ("P104#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P104#2", pen.Color, Color.PaleGreen);

			try {
				pen.Color = Color.PaleGreen;
				Fail ("P104#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P104#3", true);
			}
		}

		[Test]
		public void TestPaleTurquoise ()
		{
			Pen pen = Pens.PaleTurquoise;
			AssertEquals ("P105#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P105#2", pen.Color, Color.PaleTurquoise);

			try {
				pen.Color = Color.PaleTurquoise;
				Fail ("P105#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P105#3", true);
			}
		}

		[Test]
		public void TestPaleVioletRed ()
		{
			Pen pen = Pens.PaleVioletRed;
			AssertEquals ("P106#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P106#2", pen.Color, Color.PaleVioletRed);

			try {
				pen.Color = Color.PaleVioletRed;
				Fail ("P106#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P106#3", true);
			}
		}

		[Test]
		public void TestPapayaWhip ()
		{
			Pen pen = Pens.PapayaWhip;
			AssertEquals ("P107#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P107#2", pen.Color, Color.PapayaWhip);

			try {
				pen.Color = Color.PapayaWhip;
				Fail ("P107#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P107#3", true);
			}
		}

		[Test]
		public void TestPeachPuff ()
		{
			Pen pen = Pens.PeachPuff;
			AssertEquals ("P108#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P108#2", pen.Color, Color.PeachPuff);

			try {
				pen.Color = Color.PeachPuff;
				Fail ("P108#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P108#3", true);
			}
		}

		[Test]
		public void TestPeru ()
		{
			Pen pen = Pens.Peru;
			AssertEquals ("P109#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P109#2", pen.Color, Color.Peru);

			try {
				pen.Color = Color.Peru;
				Fail ("P109#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P109#3", true);
			}
		}

		[Test]
		public void TestPink ()
		{
			Pen pen = Pens.Pink;
			AssertEquals ("P110#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P110#2", pen.Color, Color.Pink);

			try {
				pen.Color = Color.Pink;
				Fail ("P110#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P110#3", true);
			}
		}

		[Test]
		public void TestPlum ()
		{
			Pen pen = Pens.Plum;
			AssertEquals ("P111#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P111#2", pen.Color, Color.Plum);

			try {
				pen.Color = Color.Plum;
				Fail ("P111#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P111#3", true);
			}
		}

		[Test]
		public void TestPowderBlue ()
		{
			Pen pen = Pens.PowderBlue;
			AssertEquals ("P112#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P112#2", pen.Color, Color.PowderBlue);

			try {
				pen.Color = Color.PowderBlue;
				Fail ("P112#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P112#3", true);
			}
		}

		[Test]
		public void TestPurple ()
		{
			Pen pen = Pens.Purple;
			AssertEquals ("P113#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P113#2", pen.Color, Color.Purple);

			try {
				pen.Color = Color.Purple;
				Fail ("P113#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P113#3", true);
			}
		}

		[Test]
		public void TestRed ()
		{
			Pen pen = Pens.Red;
			AssertEquals ("P114#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P114#2", pen.Color, Color.Red);

			try {
				pen.Color = Color.Red;
				Fail ("P114#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P114#3", true);
			}
		}

		[Test]
		public void TestRosyBrown ()
		{
			Pen pen = Pens.RosyBrown;
			AssertEquals ("P115#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P115#2", pen.Color, Color.RosyBrown);

			try {
				pen.Color = Color.RosyBrown;
				Fail ("P115#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P115#3", true);
			}
		}

		[Test]
		public void TestRoyalBlue ()
		{
			Pen pen = Pens.RoyalBlue;
			AssertEquals ("P116#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P116#2", pen.Color, Color.RoyalBlue);

			try {
				pen.Color = Color.RoyalBlue;
				Fail ("P116#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P116#3", true);
			}
		}

		[Test]
		public void TestSaddleBrown ()
		{
			Pen pen = Pens.SaddleBrown;
			AssertEquals ("P117#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P117#2", pen.Color, Color.SaddleBrown);

			try {
				pen.Color = Color.SaddleBrown;
				Fail ("P117#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P117#3", true);
			}
		}

		[Test]
		public void TestSalmon ()
		{
			Pen pen = Pens.Salmon;
			AssertEquals ("P118#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P118#2", pen.Color, Color.Salmon);

			try {
				pen.Color = Color.Salmon;
				Fail ("P118#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P118#3", true);
			}
		}

		[Test]
		public void TestSandyBrown ()
		{
			Pen pen = Pens.SandyBrown;
			AssertEquals ("P119#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P119#2", pen.Color, Color.SandyBrown);

			try {
				pen.Color = Color.SandyBrown;
				Fail ("P119#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P119#3", true);
			}
		}

		[Test]
		public void TestSeaGreen ()
		{
			Pen pen = Pens.SeaGreen;
			AssertEquals ("P120#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P120#2", pen.Color, Color.SeaGreen);

			try {
				pen.Color = Color.SeaGreen;
				Fail ("P120#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P120#3", true);
			}
		}

		[Test]
		public void TestSeaShell ()
		{
			Pen pen = Pens.SeaShell;
			AssertEquals ("P121#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P121#2", pen.Color, Color.SeaShell);

			try {
				pen.Color = Color.SeaShell;
				Fail ("P121#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P121#3", true);
			}
		}

		[Test]
		public void TestSienna ()
		{
			Pen pen = Pens.Sienna;
			AssertEquals ("P122#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P122#2", pen.Color, Color.Sienna);

			try {
				pen.Color = Color.Sienna;
				Fail ("P122#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P122#3", true);
			}
		}

		[Test]
		public void TestSilver ()
		{
			Pen pen = Pens.Silver;
			AssertEquals ("P123#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P123#2", pen.Color, Color.Silver);

			try {
				pen.Color = Color.Silver;
				Fail ("P123#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P123#3", true);
			}
		}

		[Test]
		public void TestSkyBlue ()
		{
			Pen pen = Pens.SkyBlue;
			AssertEquals ("P124#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P124#2", pen.Color, Color.SkyBlue);

			try {
				pen.Color = Color.SkyBlue;
				Fail ("P124#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P124#3", true);
			}
		}

		[Test]
		public void TestSlateBlue ()
		{
			Pen pen = Pens.SlateBlue;
			AssertEquals ("P125#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P125#2", pen.Color, Color.SlateBlue);

			try {
				pen.Color = Color.SlateBlue;
				Fail ("P125#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P125#3", true);
			}
		}

		[Test]
		public void TestSlateGray ()
		{
			Pen pen = Pens.SlateGray;
			AssertEquals ("P126#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P126#2", pen.Color, Color.SlateGray);

			try {
				pen.Color = Color.SlateGray;
				Fail ("P126#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P126#3", true);
			}
		}

		[Test]
		public void TestSnow ()
		{
			Pen pen = Pens.Snow;
			AssertEquals ("P127#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P127#2", pen.Color, Color.Snow);

			try {
				pen.Color = Color.Snow;
				Fail ("P127#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P127#3", true);
			}
		}

		[Test]
		public void TestSpringGreen ()
		{
			Pen pen = Pens.SpringGreen;
			AssertEquals ("P128#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P128#2", pen.Color, Color.SpringGreen);

			try {
				pen.Color = Color.SpringGreen;
				Fail ("P128#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P128#3", true);
			}
		}

		[Test]
		public void TestSteelBlue ()
		{
			Pen pen = Pens.SteelBlue;
			AssertEquals ("P129#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P129#2", pen.Color, Color.SteelBlue);

			try {
				pen.Color = Color.SteelBlue;
				Fail ("P129#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P129#3", true);
			}
		}

		[Test]
		public void TestTan ()
		{
			Pen pen = Pens.Tan;
			AssertEquals ("P130#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P130#2", pen.Color, Color.Tan);

			try {
				pen.Color = Color.Tan;
				Fail ("P130#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P130#3", true);
			}
		}

		[Test]
		public void TestTeal ()
		{
			Pen pen = Pens.Teal;
			AssertEquals ("P131#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P131#2", pen.Color, Color.Teal);

			try {
				pen.Color = Color.Teal;
				Fail ("P131#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P131#3", true);
			}
		}

		[Test]
		public void TestThistle ()
		{
			Pen pen = Pens.Thistle;
			AssertEquals ("P132#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P132#2", pen.Color, Color.Thistle);

			try {
				pen.Color = Color.Thistle;
				Fail ("P132#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P132#3", true);
			}
		}

		[Test]
		public void TestTomato ()
		{
			Pen pen = Pens.Tomato;
			AssertEquals ("P133#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P133#2", pen.Color, Color.Tomato);

			try {
				pen.Color = Color.Tomato;
				Fail ("P133#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P133#3", true);
			}
		}

		[Test]
		public void TestTransparent ()
		{
			Pen pen = Pens.Transparent;
			AssertEquals ("P134#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P134#2", pen.Color, Color.Transparent);

			try {
				pen.Color = Color.Transparent;
				Fail ("P134#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P134#3", true);
			}
		}

		[Test]
		public void TestTurquoise ()
		{
			Pen pen = Pens.Turquoise;
			AssertEquals ("P135#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P135#2", pen.Color, Color.Turquoise);

			try {
				pen.Color = Color.Turquoise;
				Fail ("P135#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P135#3", true);
			}
		}

		[Test]
		public void TestViolet ()
		{
			Pen pen = Pens.Violet;
			AssertEquals ("P136#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P136#2", pen.Color, Color.Violet);

			try {
				pen.Color = Color.Violet;
				Fail ("P136#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P136#3", true);
			}
		}

		[Test]
		public void TestWheat ()
		{
			Pen pen = Pens.Wheat;
			AssertEquals ("P137#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P137#2", pen.Color, Color.Wheat);

			try {
				pen.Color = Color.Wheat;
				Fail ("P137#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P137#3", true);
			}
		}

		[Test]
		public void TestWhite ()
		{
			Pen pen = Pens.White;
			AssertEquals ("P138#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P138#2", pen.Color, Color.White);

			try {
				pen.Color = Color.White;
				Fail ("P138#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P138#3", true);
			}
		}

		[Test]
		public void TestWhiteSmoke ()
		{
			Pen pen = Pens.WhiteSmoke;
			AssertEquals ("P139#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P139#2", pen.Color, Color.WhiteSmoke);

			try {
				pen.Color = Color.WhiteSmoke;
				Fail ("P139#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P139#3", true);
			}
		}

		[Test]
		public void TestYellow ()
		{
			Pen pen = Pens.Yellow;
			AssertEquals ("P140#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P140#2", pen.Color, Color.Yellow);

			try {
				pen.Color = Color.Yellow;
				Fail ("P140#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P140#3", true);
			}
		}

		[Test]
		public void TestYellowGreen ()
		{
			Pen pen = Pens.YellowGreen;
			AssertEquals ("P141#1", pen.PenType, PenType.SolidColor);
			AssertEquals ("P141#2", pen.Color, Color.YellowGreen);

			try {
				pen.Color = Color.YellowGreen;
				Fail ("P141#3: must throw ArgumentException");
			} catch (ArgumentException) {
				Assert ("P141#3", true);
			}
		}
	}
}

// Following code was used to generate the test methods above
//
//Type type = typeof (Pens);
//PropertyInfo [] properties = type.GetProperties ();
//int count = 1;
//foreach (PropertyInfo property in properties) {
//	Console.WriteLine();
//	Console.WriteLine("\t\t[Test]");
//	Console.WriteLine("\t\tpublic void Test" + property.Name + " ()");
//	Console.WriteLine("\t\t{");
//	Console.WriteLine("\t\t\tPen pen = Pens." + property.Name + ";");
//	Console.WriteLine("\t\t\tAssertEquals (\"P" + count + "#1\", pen.PenType, PenType.SolidColor);");
//	Console.WriteLine("\t\t\tAssertEquals (\"P" + count + "#2\", pen.Color, Color." + property.Name + ");\n");
//
//	Console.WriteLine("\t\t\ttry {");
//	Console.WriteLine("\t\t\t\tpen.Color = Color." + property.Name + ";");
//	Console.WriteLine("\t\t\t\tFail (\"P" + count + "#3: must throw ArgumentException\");");
//	Console.WriteLine("\t\t\t} catch (ArgumentException) {");
//	Console.WriteLine("\t\t\t\tAssert (\"P" + count + "#3\", true);");
//	Console.WriteLine("\t\t\t}");
//	Console.WriteLine("\t\t}");
//	count++;
//}
