//
// Tests for System.Drawing.Brushes.cs
//
// Authors:
//	Ravindra (rkumar@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

namespace MonoTests.System.Drawing {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class BrushesTest {

		[Test]
		public void Equality ()
		{
			Brush brush1 = Brushes.Blue;
			Brush brush2 = Brushes.Blue;
			Assert.IsTrue (brush1.Equals (brush2), "Equals");
			Assert.IsTrue (Object.ReferenceEquals (brush1, brush2), "ReferenceEquals");
		}

		[Test]
		public void Dispose ()
		{
			Brushes.YellowGreen.Dispose ();
			// a "normal" SolidBrush would throw an ArgumentException here
			Assert.Throws<ArgumentException> (() => Brushes.YellowGreen.Clone ());
			// and it is! so watch your brushes ;-)
		}

		[Test]
		public void Properties ()
		{
			Brush br;
			SolidBrush solid;

			br = Brushes.Transparent;
			Assert.IsTrue ((br is SolidBrush), "P1#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Transparent, solid.Color, "P1#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P1#3");
			Assert.AreEqual (Color.Red, (Brushes.Transparent as SolidBrush).Color, "P1#4");
			solid.Color = Color.Transparent; // revert to correct color (for other unit tests)

			br = Brushes.AliceBlue;
			Assert.IsTrue ((br is SolidBrush), "P2#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.AliceBlue, solid.Color, "P2#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P2#3");
			Assert.AreEqual (Color.Red, (Brushes.AliceBlue as SolidBrush).Color, "P2#4");
			solid.Color = Color.AliceBlue; // revert to correct color (for other unit tests)

			br = Brushes.AntiqueWhite;
			Assert.IsTrue ((br is SolidBrush), "P3#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.AntiqueWhite, solid.Color, "P3#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P3#3");
			Assert.AreEqual (Color.Red, (Brushes.AntiqueWhite as SolidBrush).Color, "P3#4");
			solid.Color = Color.AntiqueWhite; // revert to correct color (for other unit tests)

			br = Brushes.Aqua;
			Assert.IsTrue ((br is SolidBrush), "P4#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Aqua, solid.Color, "P4#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P4#3");
			Assert.AreEqual (Color.Red, (Brushes.Aqua as SolidBrush).Color, "P4#4");
			solid.Color = Color.Aqua; // revert to correct color (for other unit tests)

			br = Brushes.Aquamarine;
			Assert.IsTrue ((br is SolidBrush), "P5#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Aquamarine, solid.Color, "P5#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P5#3");
			Assert.AreEqual (Color.Red, (Brushes.Aquamarine as SolidBrush).Color, "P5#4");
			solid.Color = Color.Aquamarine; // revert to correct color (for other unit tests)

			br = Brushes.Azure;
			Assert.IsTrue ((br is SolidBrush), "P6#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Azure, solid.Color, "P6#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P6#3");
			Assert.AreEqual (Color.Red, (Brushes.Azure as SolidBrush).Color, "P6#4");
			solid.Color = Color.Azure; // revert to correct color (for other unit tests)

			br = Brushes.Beige;
			Assert.IsTrue ((br is SolidBrush), "P7#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Beige, solid.Color, "P7#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P7#3");
			Assert.AreEqual (Color.Red, (Brushes.Beige as SolidBrush).Color, "P7#4");
			solid.Color = Color.Beige; // revert to correct color (for other unit tests)

			br = Brushes.Bisque;
			Assert.IsTrue ((br is SolidBrush), "P8#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Bisque, solid.Color, "P8#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P8#3");
			Assert.AreEqual (Color.Red, (Brushes.Bisque as SolidBrush).Color, "P8#4");
			solid.Color = Color.Bisque; // revert to correct color (for other unit tests)

			br = Brushes.Black;
			Assert.IsTrue ((br is SolidBrush), "P9#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Black, solid.Color, "P9#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P9#3");
			Assert.AreEqual (Color.Red, (Brushes.Black as SolidBrush).Color, "P9#4");
			solid.Color = Color.Black; // revert to correct color (for other unit tests)

			br = Brushes.BlanchedAlmond;
			Assert.IsTrue ((br is SolidBrush), "P10#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.BlanchedAlmond, solid.Color, "P10#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P10#3");
			Assert.AreEqual (Color.Red, (Brushes.BlanchedAlmond as SolidBrush).Color, "P10#4");
			solid.Color = Color.BlanchedAlmond; // revert to correct color (for other unit tests)

			br = Brushes.Blue;
			Assert.IsTrue ((br is SolidBrush), "P11#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Blue, solid.Color, "P11#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P11#3");
			Assert.AreEqual (Color.Red, (Brushes.Blue as SolidBrush).Color, "P11#4");
			solid.Color = Color.Blue; // revert to correct color (for other unit tests)

			br = Brushes.BlueViolet;
			Assert.IsTrue ((br is SolidBrush), "P12#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.BlueViolet, solid.Color, "P12#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P12#3");
			Assert.AreEqual (Color.Red, (Brushes.BlueViolet as SolidBrush).Color, "P12#4");
			solid.Color = Color.BlueViolet; // revert to correct color (for other unit tests)

			br = Brushes.Brown;
			Assert.IsTrue ((br is SolidBrush), "P13#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Brown, solid.Color, "P13#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P13#3");
			Assert.AreEqual (Color.Red, (Brushes.Brown as SolidBrush).Color, "P13#4");
			solid.Color = Color.Brown; // revert to correct color (for other unit tests)

			br = Brushes.BurlyWood;
			Assert.IsTrue ((br is SolidBrush), "P14#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.BurlyWood, solid.Color, "P14#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P14#3");
			Assert.AreEqual (Color.Red, (Brushes.BurlyWood as SolidBrush).Color, "P14#4");
			solid.Color = Color.BurlyWood; // revert to correct color (for other unit tests)

			br = Brushes.CadetBlue;
			Assert.IsTrue ((br is SolidBrush), "P15#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.CadetBlue, solid.Color, "P15#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P15#3");
			Assert.AreEqual (Color.Red, (Brushes.CadetBlue as SolidBrush).Color, "P15#4");
			solid.Color = Color.CadetBlue; // revert to correct color (for other unit tests)

			br = Brushes.Chartreuse;
			Assert.IsTrue ((br is SolidBrush), "P16#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Chartreuse, solid.Color, "P16#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P16#3");
			Assert.AreEqual (Color.Red, (Brushes.Chartreuse as SolidBrush).Color, "P16#4");
			solid.Color = Color.Chartreuse; // revert to correct color (for other unit tests)

			br = Brushes.Chocolate;
			Assert.IsTrue ((br is SolidBrush), "P17#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Chocolate, solid.Color, "P17#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P17#3");
			Assert.AreEqual (Color.Red, (Brushes.Chocolate as SolidBrush).Color, "P17#4");
			solid.Color = Color.Chocolate; // revert to correct color (for other unit tests)

			br = Brushes.Coral;
			Assert.IsTrue ((br is SolidBrush), "P18#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Coral, solid.Color, "P18#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P18#3");
			Assert.AreEqual (Color.Red, (Brushes.Coral as SolidBrush).Color, "P18#4");
			solid.Color = Color.Coral; // revert to correct color (for other unit tests)

			br = Brushes.CornflowerBlue;
			Assert.IsTrue ((br is SolidBrush), "P19#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.CornflowerBlue, solid.Color, "P19#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P19#3");
			Assert.AreEqual (Color.Red, (Brushes.CornflowerBlue as SolidBrush).Color, "P19#4");
			solid.Color = Color.CornflowerBlue; // revert to correct color (for other unit tests)

			br = Brushes.Cornsilk;
			Assert.IsTrue ((br is SolidBrush), "P20#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Cornsilk, solid.Color, "P20#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P20#3");
			Assert.AreEqual (Color.Red, (Brushes.Cornsilk as SolidBrush).Color, "P20#4");
			solid.Color = Color.Cornsilk; // revert to correct color (for other unit tests)

			br = Brushes.Crimson;
			Assert.IsTrue ((br is SolidBrush), "P21#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Crimson, solid.Color, "P21#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P21#3");
			Assert.AreEqual (Color.Red, (Brushes.Crimson as SolidBrush).Color, "P21#4");
			solid.Color = Color.Crimson; // revert to correct color (for other unit tests)

			br = Brushes.Cyan;
			Assert.IsTrue ((br is SolidBrush), "P22#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Cyan, solid.Color, "P22#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P22#3");
			Assert.AreEqual (Color.Red, (Brushes.Cyan as SolidBrush).Color, "P22#4");
			solid.Color = Color.Cyan; // revert to correct color (for other unit tests)

			br = Brushes.DarkBlue;
			Assert.IsTrue ((br is SolidBrush), "P23#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkBlue, solid.Color, "P23#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P23#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkBlue as SolidBrush).Color, "P23#4");
			solid.Color = Color.DarkBlue; // revert to correct color (for other unit tests)

			br = Brushes.DarkCyan;
			Assert.IsTrue ((br is SolidBrush), "P24#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkCyan, solid.Color, "P24#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P24#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkCyan as SolidBrush).Color, "P24#4");
			solid.Color = Color.DarkCyan; // revert to correct color (for other unit tests)

			br = Brushes.DarkGoldenrod;
			Assert.IsTrue ((br is SolidBrush), "P25#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkGoldenrod, solid.Color, "P25#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P25#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkGoldenrod as SolidBrush).Color, "P25#4");
			solid.Color = Color.DarkGoldenrod; // revert to correct color (for other unit tests)

			br = Brushes.DarkGray;
			Assert.IsTrue ((br is SolidBrush), "P26#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkGray, solid.Color, "P26#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P26#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkGray as SolidBrush).Color, "P26#4");
			solid.Color = Color.DarkGray; // revert to correct color (for other unit tests)

			br = Brushes.DarkGreen;
			Assert.IsTrue ((br is SolidBrush), "P27#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkGreen, solid.Color, "P27#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P27#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkGreen as SolidBrush).Color, "P27#4");
			solid.Color = Color.DarkGreen; // revert to correct color (for other unit tests)

			br = Brushes.DarkKhaki;
			Assert.IsTrue ((br is SolidBrush), "P28#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkKhaki, solid.Color, "P28#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P28#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkKhaki as SolidBrush).Color, "P28#4");
			solid.Color = Color.DarkKhaki; // revert to correct color (for other unit tests)

			br = Brushes.DarkMagenta;
			Assert.IsTrue ((br is SolidBrush), "P29#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkMagenta, solid.Color, "P29#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P29#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkMagenta as SolidBrush).Color, "P29#4");
			solid.Color = Color.DarkMagenta; // revert to correct color (for other unit tests)

			br = Brushes.DarkOliveGreen;
			Assert.IsTrue ((br is SolidBrush), "P30#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkOliveGreen, solid.Color, "P30#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P30#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkOliveGreen as SolidBrush).Color, "P30#4");
			solid.Color = Color.DarkOliveGreen; // revert to correct color (for other unit tests)

			br = Brushes.DarkOrange;
			Assert.IsTrue ((br is SolidBrush), "P31#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkOrange, solid.Color, "P31#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P31#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkOrange as SolidBrush).Color, "P31#4");
			solid.Color = Color.DarkOrange; // revert to correct color (for other unit tests)

			br = Brushes.DarkOrchid;
			Assert.IsTrue ((br is SolidBrush), "P32#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkOrchid, solid.Color, "P32#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P32#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkOrchid as SolidBrush).Color, "P32#4");
			solid.Color = Color.DarkOrchid; // revert to correct color (for other unit tests)

			br = Brushes.DarkRed;
			Assert.IsTrue ((br is SolidBrush), "P33#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkRed, solid.Color, "P33#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P33#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkRed as SolidBrush).Color, "P33#4");
			solid.Color = Color.DarkRed; // revert to correct color (for other unit tests)

			br = Brushes.DarkSalmon;
			Assert.IsTrue ((br is SolidBrush), "P34#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkSalmon, solid.Color, "P34#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P34#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkSalmon as SolidBrush).Color, "P34#4");
			solid.Color = Color.DarkSalmon; // revert to correct color (for other unit tests)

			br = Brushes.DarkSeaGreen;
			Assert.IsTrue ((br is SolidBrush), "P35#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkSeaGreen, solid.Color, "P35#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P35#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkSeaGreen as SolidBrush).Color, "P35#4");
			solid.Color = Color.DarkSeaGreen; // revert to correct color (for other unit tests)

			br = Brushes.DarkSlateBlue;
			Assert.IsTrue ((br is SolidBrush), "P36#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkSlateBlue, solid.Color, "P36#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P36#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkSlateBlue as SolidBrush).Color, "P36#4");
			solid.Color = Color.DarkSlateBlue; // revert to correct color (for other unit tests)

			br = Brushes.DarkSlateGray;
			Assert.IsTrue ((br is SolidBrush), "P37#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkSlateGray, solid.Color, "P37#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P37#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkSlateGray as SolidBrush).Color, "P37#4");
			solid.Color = Color.DarkSlateGray; // revert to correct color (for other unit tests)

			br = Brushes.DarkTurquoise;
			Assert.IsTrue ((br is SolidBrush), "P38#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkTurquoise, solid.Color, "P38#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P38#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkTurquoise as SolidBrush).Color, "P38#4");
			solid.Color = Color.DarkTurquoise; // revert to correct color (for other unit tests)

			br = Brushes.DarkViolet;
			Assert.IsTrue ((br is SolidBrush), "P39#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DarkViolet, solid.Color, "P39#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P39#3");
			Assert.AreEqual (Color.Red, (Brushes.DarkViolet as SolidBrush).Color, "P39#4");
			solid.Color = Color.DarkViolet; // revert to correct color (for other unit tests)

			br = Brushes.DeepPink;
			Assert.IsTrue ((br is SolidBrush), "P40#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DeepPink, solid.Color, "P40#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P40#3");
			Assert.AreEqual (Color.Red, (Brushes.DeepPink as SolidBrush).Color, "P40#4");
			solid.Color = Color.DeepPink; // revert to correct color (for other unit tests)

			br = Brushes.DeepSkyBlue;
			Assert.IsTrue ((br is SolidBrush), "P41#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DeepSkyBlue, solid.Color, "P41#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P41#3");
			Assert.AreEqual (Color.Red, (Brushes.DeepSkyBlue as SolidBrush).Color, "P41#4");
			solid.Color = Color.DeepSkyBlue; // revert to correct color (for other unit tests)

			br = Brushes.DimGray;
			Assert.IsTrue ((br is SolidBrush), "P42#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DimGray, solid.Color, "P42#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P42#3");
			Assert.AreEqual (Color.Red, (Brushes.DimGray as SolidBrush).Color, "P42#4");
			solid.Color = Color.DimGray; // revert to correct color (for other unit tests)

			br = Brushes.DodgerBlue;
			Assert.IsTrue ((br is SolidBrush), "P43#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.DodgerBlue, solid.Color, "P43#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P43#3");
			Assert.AreEqual (Color.Red, (Brushes.DodgerBlue as SolidBrush).Color, "P43#4");
			solid.Color = Color.DodgerBlue; // revert to correct color (for other unit tests)

			br = Brushes.Firebrick;
			Assert.IsTrue ((br is SolidBrush), "P44#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Firebrick, solid.Color, "P44#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P44#3");
			Assert.AreEqual (Color.Red, (Brushes.Firebrick as SolidBrush).Color, "P44#4");
			solid.Color = Color.Firebrick; // revert to correct color (for other unit tests)

			br = Brushes.FloralWhite;
			Assert.IsTrue ((br is SolidBrush), "P45#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.FloralWhite, solid.Color, "P45#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P45#3");
			Assert.AreEqual (Color.Red, (Brushes.FloralWhite as SolidBrush).Color, "P45#4");
			solid.Color = Color.FloralWhite; // revert to correct color (for other unit tests)

			br = Brushes.ForestGreen;
			Assert.IsTrue ((br is SolidBrush), "P46#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.ForestGreen, solid.Color, "P46#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P46#3");
			Assert.AreEqual (Color.Red, (Brushes.ForestGreen as SolidBrush).Color, "P46#4");
			solid.Color = Color.ForestGreen; // revert to correct color (for other unit tests)

			br = Brushes.Fuchsia;
			Assert.IsTrue ((br is SolidBrush), "P47#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Fuchsia, solid.Color, "P47#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P47#3");
			Assert.AreEqual (Color.Red, (Brushes.Fuchsia as SolidBrush).Color, "P47#4");
			solid.Color = Color.Fuchsia; // revert to correct color (for other unit tests)

			br = Brushes.Gainsboro;
			Assert.IsTrue ((br is SolidBrush), "P48#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Gainsboro, solid.Color, "P48#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P48#3");
			Assert.AreEqual (Color.Red, (Brushes.Gainsboro as SolidBrush).Color, "P48#4");
			solid.Color = Color.Gainsboro; // revert to correct color (for other unit tests)

			br = Brushes.GhostWhite;
			Assert.IsTrue ((br is SolidBrush), "P49#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.GhostWhite, solid.Color, "P49#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P49#3");
			Assert.AreEqual (Color.Red, (Brushes.GhostWhite as SolidBrush).Color, "P49#4");
			solid.Color = Color.GhostWhite; // revert to correct color (for other unit tests)

			br = Brushes.Gold;
			Assert.IsTrue ((br is SolidBrush), "P50#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Gold, solid.Color, "P50#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P50#3");
			Assert.AreEqual (Color.Red, (Brushes.Gold as SolidBrush).Color, "P50#4");
			solid.Color = Color.Gold; // revert to correct color (for other unit tests)

			br = Brushes.Goldenrod;
			Assert.IsTrue ((br is SolidBrush), "P51#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Goldenrod, solid.Color, "P51#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P51#3");
			Assert.AreEqual (Color.Red, (Brushes.Goldenrod as SolidBrush).Color, "P51#4");
			solid.Color = Color.Goldenrod; // revert to correct color (for other unit tests)

			br = Brushes.Gray;
			Assert.IsTrue ((br is SolidBrush), "P52#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Gray, solid.Color, "P52#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P52#3");
			Assert.AreEqual (Color.Red, (Brushes.Gray as SolidBrush).Color, "P52#4");
			solid.Color = Color.Gray; // revert to correct color (for other unit tests)

			br = Brushes.Green;
			Assert.IsTrue ((br is SolidBrush), "P53#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Green, solid.Color, "P53#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P53#3");
			Assert.AreEqual (Color.Red, (Brushes.Green as SolidBrush).Color, "P53#4");
			solid.Color = Color.Green; // revert to correct color (for other unit tests)

			br = Brushes.GreenYellow;
			Assert.IsTrue ((br is SolidBrush), "P54#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.GreenYellow, solid.Color, "P54#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P54#3");
			Assert.AreEqual (Color.Red, (Brushes.GreenYellow as SolidBrush).Color, "P54#4");
			solid.Color = Color.GreenYellow; // revert to correct color (for other unit tests)

			br = Brushes.Honeydew;
			Assert.IsTrue ((br is SolidBrush), "P55#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Honeydew, solid.Color, "P55#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P55#3");
			Assert.AreEqual (Color.Red, (Brushes.Honeydew as SolidBrush).Color, "P55#4");
			solid.Color = Color.Honeydew; // revert to correct color (for other unit tests)

			br = Brushes.HotPink;
			Assert.IsTrue ((br is SolidBrush), "P56#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.HotPink, solid.Color, "P56#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P56#3");
			Assert.AreEqual (Color.Red, (Brushes.HotPink as SolidBrush).Color, "P56#4");
			solid.Color = Color.HotPink; // revert to correct color (for other unit tests)

			br = Brushes.IndianRed;
			Assert.IsTrue ((br is SolidBrush), "P57#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.IndianRed, solid.Color, "P57#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P57#3");
			Assert.AreEqual (Color.Red, (Brushes.IndianRed as SolidBrush).Color, "P57#4");
			solid.Color = Color.IndianRed; // revert to correct color (for other unit tests)

			br = Brushes.Indigo;
			Assert.IsTrue ((br is SolidBrush), "P58#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Indigo, solid.Color, "P58#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P58#3");
			Assert.AreEqual (Color.Red, (Brushes.Indigo as SolidBrush).Color, "P58#4");
			solid.Color = Color.Indigo; // revert to correct color (for other unit tests)

			br = Brushes.Ivory;
			Assert.IsTrue ((br is SolidBrush), "P59#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Ivory, solid.Color, "P59#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P59#3");
			Assert.AreEqual (Color.Red, (Brushes.Ivory as SolidBrush).Color, "P59#4");
			solid.Color = Color.Ivory; // revert to correct color (for other unit tests)

			br = Brushes.Khaki;
			Assert.IsTrue ((br is SolidBrush), "P60#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Khaki, solid.Color, "P60#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P60#3");
			Assert.AreEqual (Color.Red, (Brushes.Khaki as SolidBrush).Color, "P60#4");
			solid.Color = Color.Khaki; // revert to correct color (for other unit tests)

			br = Brushes.Lavender;
			Assert.IsTrue ((br is SolidBrush), "P61#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Lavender, solid.Color, "P61#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P61#3");
			Assert.AreEqual (Color.Red, (Brushes.Lavender as SolidBrush).Color, "P61#4");
			solid.Color = Color.Lavender; // revert to correct color (for other unit tests)

			br = Brushes.LavenderBlush;
			Assert.IsTrue ((br is SolidBrush), "P62#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LavenderBlush, solid.Color, "P62#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P62#3");
			Assert.AreEqual (Color.Red, (Brushes.LavenderBlush as SolidBrush).Color, "P62#4");
			solid.Color = Color.LavenderBlush; // revert to correct color (for other unit tests)

			br = Brushes.LawnGreen;
			Assert.IsTrue ((br is SolidBrush), "P63#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LawnGreen, solid.Color, "P63#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P63#3");
			Assert.AreEqual (Color.Red, (Brushes.LawnGreen as SolidBrush).Color, "P63#4");
			solid.Color = Color.LawnGreen; // revert to correct color (for other unit tests)

			br = Brushes.LemonChiffon;
			Assert.IsTrue ((br is SolidBrush), "P64#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LemonChiffon, solid.Color, "P64#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P64#3");
			Assert.AreEqual (Color.Red, (Brushes.LemonChiffon as SolidBrush).Color, "P64#4");
			solid.Color = Color.LemonChiffon; // revert to correct color (for other unit tests)

			br = Brushes.LightBlue;
			Assert.IsTrue ((br is SolidBrush), "P65#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightBlue, solid.Color, "P65#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P65#3");
			Assert.AreEqual (Color.Red, (Brushes.LightBlue as SolidBrush).Color, "P65#4");
			solid.Color = Color.LightBlue; // revert to correct color (for other unit tests)

			br = Brushes.LightCoral;
			Assert.IsTrue ((br is SolidBrush), "P66#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightCoral, solid.Color, "P66#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P66#3");
			Assert.AreEqual (Color.Red, (Brushes.LightCoral as SolidBrush).Color, "P66#4");
			solid.Color = Color.LightCoral; // revert to correct color (for other unit tests)

			br = Brushes.LightCyan;
			Assert.IsTrue ((br is SolidBrush), "P67#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightCyan, solid.Color, "P67#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P67#3");
			Assert.AreEqual (Color.Red, (Brushes.LightCyan as SolidBrush).Color, "P67#4");
			solid.Color = Color.LightCyan; // revert to correct color (for other unit tests)

			br = Brushes.LightGoldenrodYellow;
			Assert.IsTrue ((br is SolidBrush), "P68#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightGoldenrodYellow, solid.Color, "P68#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P68#3");
			Assert.AreEqual (Color.Red, (Brushes.LightGoldenrodYellow as SolidBrush).Color, "P68#4");
			solid.Color = Color.LightGoldenrodYellow; // revert to correct color (for other unit tests)

			br = Brushes.LightGreen;
			Assert.IsTrue ((br is SolidBrush), "P69#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightGreen, solid.Color, "P69#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P69#3");
			Assert.AreEqual (Color.Red, (Brushes.LightGreen as SolidBrush).Color, "P69#4");
			solid.Color = Color.LightGreen; // revert to correct color (for other unit tests)

			br = Brushes.LightGray;
			Assert.IsTrue ((br is SolidBrush), "P70#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightGray, solid.Color, "P70#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P70#3");
			Assert.AreEqual (Color.Red, (Brushes.LightGray as SolidBrush).Color, "P70#4");
			solid.Color = Color.LightGray; // revert to correct color (for other unit tests)

			br = Brushes.LightPink;
			Assert.IsTrue ((br is SolidBrush), "P71#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightPink, solid.Color, "P71#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P71#3");
			Assert.AreEqual (Color.Red, (Brushes.LightPink as SolidBrush).Color, "P71#4");
			solid.Color = Color.LightPink; // revert to correct color (for other unit tests)

			br = Brushes.LightSalmon;
			Assert.IsTrue ((br is SolidBrush), "P72#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightSalmon, solid.Color, "P72#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P72#3");
			Assert.AreEqual (Color.Red, (Brushes.LightSalmon as SolidBrush).Color, "P72#4");
			solid.Color = Color.LightSalmon; // revert to correct color (for other unit tests)

			br = Brushes.LightSeaGreen;
			Assert.IsTrue ((br is SolidBrush), "P73#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightSeaGreen, solid.Color, "P73#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P73#3");
			Assert.AreEqual (Color.Red, (Brushes.LightSeaGreen as SolidBrush).Color, "P73#4");
			solid.Color = Color.LightSeaGreen; // revert to correct color (for other unit tests)

			br = Brushes.LightSkyBlue;
			Assert.IsTrue ((br is SolidBrush), "P74#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightSkyBlue, solid.Color, "P74#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P74#3");
			Assert.AreEqual (Color.Red, (Brushes.LightSkyBlue as SolidBrush).Color, "P74#4");
			solid.Color = Color.LightSkyBlue; // revert to correct color (for other unit tests)

			br = Brushes.LightSlateGray;
			Assert.IsTrue ((br is SolidBrush), "P75#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightSlateGray, solid.Color, "P75#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P75#3");
			Assert.AreEqual (Color.Red, (Brushes.LightSlateGray as SolidBrush).Color, "P75#4");
			solid.Color = Color.LightSlateGray; // revert to correct color (for other unit tests)

			br = Brushes.LightSteelBlue;
			Assert.IsTrue ((br is SolidBrush), "P76#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightSteelBlue, solid.Color, "P76#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P76#3");
			Assert.AreEqual (Color.Red, (Brushes.LightSteelBlue as SolidBrush).Color, "P76#4");
			solid.Color = Color.LightSteelBlue; // revert to correct color (for other unit tests)

			br = Brushes.LightYellow;
			Assert.IsTrue ((br is SolidBrush), "P77#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LightYellow, solid.Color, "P77#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P77#3");
			Assert.AreEqual (Color.Red, (Brushes.LightYellow as SolidBrush).Color, "P77#4");
			solid.Color = Color.LightYellow; // revert to correct color (for other unit tests)

			br = Brushes.Lime;
			Assert.IsTrue ((br is SolidBrush), "P78#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Lime, solid.Color, "P78#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P78#3");
			Assert.AreEqual (Color.Red, (Brushes.Lime as SolidBrush).Color, "P78#4");
			solid.Color = Color.Lime; // revert to correct color (for other unit tests)

			br = Brushes.LimeGreen;
			Assert.IsTrue ((br is SolidBrush), "P79#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.LimeGreen, solid.Color, "P79#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P79#3");
			Assert.AreEqual (Color.Red, (Brushes.LimeGreen as SolidBrush).Color, "P79#4");
			solid.Color = Color.LimeGreen; // revert to correct color (for other unit tests)

			br = Brushes.Linen;
			Assert.IsTrue ((br is SolidBrush), "P80#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Linen, solid.Color, "P80#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P80#3");
			Assert.AreEqual (Color.Red, (Brushes.Linen as SolidBrush).Color, "P80#4");
			solid.Color = Color.Linen; // revert to correct color (for other unit tests)

			br = Brushes.Magenta;
			Assert.IsTrue ((br is SolidBrush), "P81#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Magenta, solid.Color, "P81#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P81#3");
			Assert.AreEqual (Color.Red, (Brushes.Magenta as SolidBrush).Color, "P81#4");
			solid.Color = Color.Magenta; // revert to correct color (for other unit tests)

			br = Brushes.Maroon;
			Assert.IsTrue ((br is SolidBrush), "P82#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Maroon, solid.Color, "P82#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P82#3");
			Assert.AreEqual (Color.Red, (Brushes.Maroon as SolidBrush).Color, "P82#4");
			solid.Color = Color.Maroon; // revert to correct color (for other unit tests)

			br = Brushes.MediumAquamarine;
			Assert.IsTrue ((br is SolidBrush), "P83#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumAquamarine, solid.Color, "P83#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P83#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumAquamarine as SolidBrush).Color, "P83#4");
			solid.Color = Color.MediumAquamarine; // revert to correct color (for other unit tests)

			br = Brushes.MediumBlue;
			Assert.IsTrue ((br is SolidBrush), "P84#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumBlue, solid.Color, "P84#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P84#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumBlue as SolidBrush).Color, "P84#4");
			solid.Color = Color.MediumBlue; // revert to correct color (for other unit tests)

			br = Brushes.MediumOrchid;
			Assert.IsTrue ((br is SolidBrush), "P85#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumOrchid, solid.Color, "P85#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P85#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumOrchid as SolidBrush).Color, "P85#4");
			solid.Color = Color.MediumOrchid; // revert to correct color (for other unit tests)

			br = Brushes.MediumPurple;
			Assert.IsTrue ((br is SolidBrush), "P86#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumPurple, solid.Color, "P86#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P86#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumPurple as SolidBrush).Color, "P86#4");
			solid.Color = Color.MediumPurple; // revert to correct color (for other unit tests)

			br = Brushes.MediumSeaGreen;
			Assert.IsTrue ((br is SolidBrush), "P87#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumSeaGreen, solid.Color, "P87#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P87#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumSeaGreen as SolidBrush).Color, "P87#4");
			solid.Color = Color.MediumSeaGreen; // revert to correct color (for other unit tests)

			br = Brushes.MediumSlateBlue;
			Assert.IsTrue ((br is SolidBrush), "P88#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumSlateBlue, solid.Color, "P88#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P88#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumSlateBlue as SolidBrush).Color, "P88#4");
			solid.Color = Color.MediumSlateBlue; // revert to correct color (for other unit tests)

			br = Brushes.MediumSpringGreen;
			Assert.IsTrue ((br is SolidBrush), "P89#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumSpringGreen, solid.Color, "P89#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P89#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumSpringGreen as SolidBrush).Color, "P89#4");
			solid.Color = Color.MediumSpringGreen; // revert to correct color (for other unit tests)

			br = Brushes.MediumTurquoise;
			Assert.IsTrue ((br is SolidBrush), "P90#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumTurquoise, solid.Color, "P90#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P90#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumTurquoise as SolidBrush).Color, "P90#4");
			solid.Color = Color.MediumTurquoise; // revert to correct color (for other unit tests)

			br = Brushes.MediumVioletRed;
			Assert.IsTrue ((br is SolidBrush), "P91#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MediumVioletRed, solid.Color, "P91#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P91#3");
			Assert.AreEqual (Color.Red, (Brushes.MediumVioletRed as SolidBrush).Color, "P91#4");
			solid.Color = Color.MediumVioletRed; // revert to correct color (for other unit tests)

			br = Brushes.MidnightBlue;
			Assert.IsTrue ((br is SolidBrush), "P92#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MidnightBlue, solid.Color, "P92#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P92#3");
			Assert.AreEqual (Color.Red, (Brushes.MidnightBlue as SolidBrush).Color, "P92#4");
			solid.Color = Color.MidnightBlue; // revert to correct color (for other unit tests)

			br = Brushes.MintCream;
			Assert.IsTrue ((br is SolidBrush), "P93#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MintCream, solid.Color, "P93#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P93#3");
			Assert.AreEqual (Color.Red, (Brushes.MintCream as SolidBrush).Color, "P93#4");
			solid.Color = Color.MintCream; // revert to correct color (for other unit tests)

			br = Brushes.MistyRose;
			Assert.IsTrue ((br is SolidBrush), "P94#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.MistyRose, solid.Color, "P94#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P94#3");
			Assert.AreEqual (Color.Red, (Brushes.MistyRose as SolidBrush).Color, "P94#4");
			solid.Color = Color.MistyRose; // revert to correct color (for other unit tests)

			br = Brushes.Moccasin;
			Assert.IsTrue ((br is SolidBrush), "P95#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Moccasin, solid.Color, "P95#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P95#3");
			Assert.AreEqual (Color.Red, (Brushes.Moccasin as SolidBrush).Color, "P95#4");
			solid.Color = Color.Moccasin; // revert to correct color (for other unit tests)

			br = Brushes.NavajoWhite;
			Assert.IsTrue ((br is SolidBrush), "P96#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.NavajoWhite, solid.Color, "P96#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P96#3");
			Assert.AreEqual (Color.Red, (Brushes.NavajoWhite as SolidBrush).Color, "P96#4");
			solid.Color = Color.NavajoWhite; // revert to correct color (for other unit tests)

			br = Brushes.Navy;
			Assert.IsTrue ((br is SolidBrush), "P97#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Navy, solid.Color, "P97#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P97#3");
			Assert.AreEqual (Color.Red, (Brushes.Navy as SolidBrush).Color, "P97#4");
			solid.Color = Color.Navy; // revert to correct color (for other unit tests)

			br = Brushes.OldLace;
			Assert.IsTrue ((br is SolidBrush), "P98#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.OldLace, solid.Color, "P98#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P98#3");
			Assert.AreEqual (Color.Red, (Brushes.OldLace as SolidBrush).Color, "P98#4");
			solid.Color = Color.OldLace; // revert to correct color (for other unit tests)

			br = Brushes.Olive;
			Assert.IsTrue ((br is SolidBrush), "P99#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Olive, solid.Color, "P99#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P99#3");
			Assert.AreEqual (Color.Red, (Brushes.Olive as SolidBrush).Color, "P99#4");
			solid.Color = Color.Olive; // revert to correct color (for other unit tests)

			br = Brushes.OliveDrab;
			Assert.IsTrue ((br is SolidBrush), "P100#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.OliveDrab, solid.Color, "P100#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P100#3");
			Assert.AreEqual (Color.Red, (Brushes.OliveDrab as SolidBrush).Color, "P100#4");
			solid.Color = Color.OliveDrab; // revert to correct color (for other unit tests)

			br = Brushes.Orange;
			Assert.IsTrue ((br is SolidBrush), "P101#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Orange, solid.Color, "P101#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P101#3");
			Assert.AreEqual (Color.Red, (Brushes.Orange as SolidBrush).Color, "P101#4");
			solid.Color = Color.Orange; // revert to correct color (for other unit tests)

			br = Brushes.OrangeRed;
			Assert.IsTrue ((br is SolidBrush), "P102#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.OrangeRed, solid.Color, "P102#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P102#3");
			Assert.AreEqual (Color.Red, (Brushes.OrangeRed as SolidBrush).Color, "P102#4");
			solid.Color = Color.OrangeRed; // revert to correct color (for other unit tests)

			br = Brushes.Orchid;
			Assert.IsTrue ((br is SolidBrush), "P103#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Orchid, solid.Color, "P103#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P103#3");
			Assert.AreEqual (Color.Red, (Brushes.Orchid as SolidBrush).Color, "P103#4");
			solid.Color = Color.Orchid; // revert to correct color (for other unit tests)

			br = Brushes.PaleGoldenrod;
			Assert.IsTrue ((br is SolidBrush), "P104#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.PaleGoldenrod, solid.Color, "P104#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P104#3");
			Assert.AreEqual (Color.Red, (Brushes.PaleGoldenrod as SolidBrush).Color, "P104#4");
			solid.Color = Color.PaleGoldenrod; // revert to correct color (for other unit tests)

			br = Brushes.PaleGreen;
			Assert.IsTrue ((br is SolidBrush), "P105#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.PaleGreen, solid.Color, "P105#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P105#3");
			Assert.AreEqual (Color.Red, (Brushes.PaleGreen as SolidBrush).Color, "P105#4");
			solid.Color = Color.PaleGreen; // revert to correct color (for other unit tests)

			br = Brushes.PaleTurquoise;
			Assert.IsTrue ((br is SolidBrush), "P106#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.PaleTurquoise, solid.Color, "P106#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P106#3");
			Assert.AreEqual (Color.Red, (Brushes.PaleTurquoise as SolidBrush).Color, "P106#4");
			solid.Color = Color.PaleTurquoise; // revert to correct color (for other unit tests)

			br = Brushes.PaleVioletRed;
			Assert.IsTrue ((br is SolidBrush), "P107#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.PaleVioletRed, solid.Color, "P107#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P107#3");
			Assert.AreEqual (Color.Red, (Brushes.PaleVioletRed as SolidBrush).Color, "P107#4");
			solid.Color = Color.PaleVioletRed; // revert to correct color (for other unit tests)

			br = Brushes.PapayaWhip;
			Assert.IsTrue ((br is SolidBrush), "P108#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.PapayaWhip, solid.Color, "P108#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P108#3");
			Assert.AreEqual (Color.Red, (Brushes.PapayaWhip as SolidBrush).Color, "P108#4");
			solid.Color = Color.PapayaWhip; // revert to correct color (for other unit tests)

			br = Brushes.PeachPuff;
			Assert.IsTrue ((br is SolidBrush), "P109#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.PeachPuff, solid.Color, "P109#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P109#3");
			Assert.AreEqual (Color.Red, (Brushes.PeachPuff as SolidBrush).Color, "P109#4");
			solid.Color = Color.PeachPuff; // revert to correct color (for other unit tests)

			br = Brushes.Peru;
			Assert.IsTrue ((br is SolidBrush), "P110#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Peru, solid.Color, "P110#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P110#3");
			Assert.AreEqual (Color.Red, (Brushes.Peru as SolidBrush).Color, "P110#4");
			solid.Color = Color.Peru; // revert to correct color (for other unit tests)

			br = Brushes.Pink;
			Assert.IsTrue ((br is SolidBrush), "P111#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Pink, solid.Color, "P111#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P111#3");
			Assert.AreEqual (Color.Red, (Brushes.Pink as SolidBrush).Color, "P111#4");
			solid.Color = Color.Pink; // revert to correct color (for other unit tests)

			br = Brushes.Plum;
			Assert.IsTrue ((br is SolidBrush), "P112#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Plum, solid.Color, "P112#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P112#3");
			Assert.AreEqual (Color.Red, (Brushes.Plum as SolidBrush).Color, "P112#4");
			solid.Color = Color.Plum; // revert to correct color (for other unit tests)

			br = Brushes.PowderBlue;
			Assert.IsTrue ((br is SolidBrush), "P113#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.PowderBlue, solid.Color, "P113#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P113#3");
			Assert.AreEqual (Color.Red, (Brushes.PowderBlue as SolidBrush).Color, "P113#4");
			solid.Color = Color.PowderBlue; // revert to correct color (for other unit tests)

			br = Brushes.Purple;
			Assert.IsTrue ((br is SolidBrush), "P114#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Purple, solid.Color, "P114#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P114#3");
			Assert.AreEqual (Color.Red, (Brushes.Purple as SolidBrush).Color, "P114#4");
			solid.Color = Color.Purple; // revert to correct color (for other unit tests)

			br = Brushes.Red;
			Assert.IsTrue ((br is SolidBrush), "P115#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Red, solid.Color, "P115#2");
			solid.Color = Color.White;
			Assert.AreEqual (Color.White, solid.Color, "P115#3");
			Assert.AreEqual (Color.White, (Brushes.Red as SolidBrush).Color, "P115#4");
			solid.Color = Color.Red; // revert to correct color (for other unit tests)

			br = Brushes.RosyBrown;
			Assert.IsTrue ((br is SolidBrush), "P116#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.RosyBrown, solid.Color, "P116#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P116#3");
			Assert.AreEqual (Color.Red, (Brushes.RosyBrown as SolidBrush).Color, "P116#4");
			solid.Color = Color.RosyBrown; // revert to correct color (for other unit tests)

			br = Brushes.RoyalBlue;
			Assert.IsTrue ((br is SolidBrush), "P117#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.RoyalBlue, solid.Color, "P117#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P117#3");
			Assert.AreEqual (Color.Red, (Brushes.RoyalBlue as SolidBrush).Color, "P117#4");
			solid.Color = Color.RoyalBlue; // revert to correct color (for other unit tests)

			br = Brushes.SaddleBrown;
			Assert.IsTrue ((br is SolidBrush), "P118#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SaddleBrown, solid.Color, "P118#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P118#3");
			Assert.AreEqual (Color.Red, (Brushes.SaddleBrown as SolidBrush).Color, "P118#4");
			solid.Color = Color.SaddleBrown; // revert to correct color (for other unit tests)

			br = Brushes.Salmon;
			Assert.IsTrue ((br is SolidBrush), "P119#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Salmon, solid.Color, "P119#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P119#3");
			Assert.AreEqual (Color.Red, (Brushes.Salmon as SolidBrush).Color, "P119#4");
			solid.Color = Color.Salmon; // revert to correct color (for other unit tests)

			br = Brushes.SandyBrown;
			Assert.IsTrue ((br is SolidBrush), "P120#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SandyBrown, solid.Color, "P120#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P120#3");
			Assert.AreEqual (Color.Red, (Brushes.SandyBrown as SolidBrush).Color, "P120#4");
			solid.Color = Color.SandyBrown; // revert to correct color (for other unit tests)

			br = Brushes.SeaGreen;
			Assert.IsTrue ((br is SolidBrush), "P121#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SeaGreen, solid.Color, "P121#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P121#3");
			Assert.AreEqual (Color.Red, (Brushes.SeaGreen as SolidBrush).Color, "P121#4");
			solid.Color = Color.SeaGreen; // revert to correct color (for other unit tests)

			br = Brushes.SeaShell;
			Assert.IsTrue ((br is SolidBrush), "P122#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SeaShell, solid.Color, "P122#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P122#3");
			Assert.AreEqual (Color.Red, (Brushes.SeaShell as SolidBrush).Color, "P122#4");
			solid.Color = Color.SeaShell; // revert to correct color (for other unit tests)

			br = Brushes.Sienna;
			Assert.IsTrue ((br is SolidBrush), "P123#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Sienna, solid.Color, "P123#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P123#3");
			Assert.AreEqual (Color.Red, (Brushes.Sienna as SolidBrush).Color, "P123#4");
			solid.Color = Color.Sienna; // revert to correct color (for other unit tests)

			br = Brushes.Silver;
			Assert.IsTrue ((br is SolidBrush), "P124#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Silver, solid.Color, "P124#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P124#3");
			Assert.AreEqual (Color.Red, (Brushes.Silver as SolidBrush).Color, "P124#4");
			solid.Color = Color.Silver; // revert to correct color (for other unit tests)

			br = Brushes.SkyBlue;
			Assert.IsTrue ((br is SolidBrush), "P125#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SkyBlue, solid.Color, "P125#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P125#3");
			Assert.AreEqual (Color.Red, (Brushes.SkyBlue as SolidBrush).Color, "P125#4");
			solid.Color = Color.SkyBlue; // revert to correct color (for other unit tests)

			br = Brushes.SlateBlue;
			Assert.IsTrue ((br is SolidBrush), "P126#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SlateBlue, solid.Color, "P126#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P126#3");
			Assert.AreEqual (Color.Red, (Brushes.SlateBlue as SolidBrush).Color, "P126#4");
			solid.Color = Color.SlateBlue; // revert to correct color (for other unit tests)

			br = Brushes.SlateGray;
			Assert.IsTrue ((br is SolidBrush), "P127#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SlateGray, solid.Color, "P127#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P127#3");
			Assert.AreEqual (Color.Red, (Brushes.SlateGray as SolidBrush).Color, "P127#4");
			solid.Color = Color.SlateGray; // revert to correct color (for other unit tests)

			br = Brushes.Snow;
			Assert.IsTrue ((br is SolidBrush), "P128#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Snow, solid.Color, "P128#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P128#3");
			Assert.AreEqual (Color.Red, (Brushes.Snow as SolidBrush).Color, "P128#4");
			solid.Color = Color.Snow; // revert to correct color (for other unit tests)

			br = Brushes.SpringGreen;
			Assert.IsTrue ((br is SolidBrush), "P129#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SpringGreen, solid.Color, "P129#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P129#3");
			Assert.AreEqual (Color.Red, (Brushes.SpringGreen as SolidBrush).Color, "P129#4");
			solid.Color = Color.SpringGreen; // revert to correct color (for other unit tests)

			br = Brushes.SteelBlue;
			Assert.IsTrue ((br is SolidBrush), "P130#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.SteelBlue, solid.Color, "P130#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P130#3");
			Assert.AreEqual (Color.Red, (Brushes.SteelBlue as SolidBrush).Color, "P130#4");
			solid.Color = Color.SteelBlue; // revert to correct color (for other unit tests)

			br = Brushes.Tan;
			Assert.IsTrue ((br is SolidBrush), "P131#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Tan, solid.Color, "P131#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P131#3");
			Assert.AreEqual (Color.Red, (Brushes.Tan as SolidBrush).Color, "P131#4");
			solid.Color = Color.Tan; // revert to correct color (for other unit tests)

			br = Brushes.Teal;
			Assert.IsTrue ((br is SolidBrush), "P132#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Teal, solid.Color, "P132#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P132#3");
			Assert.AreEqual (Color.Red, (Brushes.Teal as SolidBrush).Color, "P132#4");
			solid.Color = Color.Teal; // revert to correct color (for other unit tests)

			br = Brushes.Thistle;
			Assert.IsTrue ((br is SolidBrush), "P133#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Thistle, solid.Color, "P133#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P133#3");
			Assert.AreEqual (Color.Red, (Brushes.Thistle as SolidBrush).Color, "P133#4");
			solid.Color = Color.Thistle; // revert to correct color (for other unit tests)

			br = Brushes.Tomato;
			Assert.IsTrue ((br is SolidBrush), "P134#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Tomato, solid.Color, "P134#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P134#3");
			Assert.AreEqual (Color.Red, (Brushes.Tomato as SolidBrush).Color, "P134#4");
			solid.Color = Color.Tomato; // revert to correct color (for other unit tests)

			br = Brushes.Turquoise;
			Assert.IsTrue ((br is SolidBrush), "P135#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Turquoise, solid.Color, "P135#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P135#3");
			Assert.AreEqual (Color.Red, (Brushes.Turquoise as SolidBrush).Color, "P135#4");
			solid.Color = Color.Turquoise; // revert to correct color (for other unit tests)

			br = Brushes.Violet;
			Assert.IsTrue ((br is SolidBrush), "P136#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Violet, solid.Color, "P136#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P136#3");
			Assert.AreEqual (Color.Red, (Brushes.Violet as SolidBrush).Color, "P136#4");
			solid.Color = Color.Violet; // revert to correct color (for other unit tests)

			br = Brushes.Wheat;
			Assert.IsTrue ((br is SolidBrush), "P137#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Wheat, solid.Color, "P137#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P137#3");
			Assert.AreEqual (Color.Red, (Brushes.Wheat as SolidBrush).Color, "P137#4");
			solid.Color = Color.Wheat; // revert to correct color (for other unit tests)

			br = Brushes.White;
			Assert.IsTrue ((br is SolidBrush), "P138#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.White, solid.Color, "P138#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P138#3");
			Assert.AreEqual (Color.Red, (Brushes.White as SolidBrush).Color, "P138#4");
			solid.Color = Color.White; // revert to correct color (for other unit tests)

			br = Brushes.WhiteSmoke;
			Assert.IsTrue ((br is SolidBrush), "P139#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.WhiteSmoke, solid.Color, "P139#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P139#3");
			Assert.AreEqual (Color.Red, (Brushes.WhiteSmoke as SolidBrush).Color, "P139#4");
			solid.Color = Color.WhiteSmoke; // revert to correct color (for other unit tests)

			br = Brushes.Yellow;
			Assert.IsTrue ((br is SolidBrush), "P140#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.Yellow, solid.Color, "P140#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P140#3");
			Assert.AreEqual (Color.Red, (Brushes.Yellow as SolidBrush).Color, "P140#4");
			solid.Color = Color.Yellow; // revert to correct color (for other unit tests)

			/* YellowGreen is broken by "destructive" Dispose test
			br = Brushes.YellowGreen;
			Assert.IsTrue ((br is SolidBrush), "P141#1");
			solid = (SolidBrush) br;
			Assert.AreEqual (Color.YellowGreen, solid.Color, "P141#2");
			solid.Color = Color.Red;
			Assert.AreEqual (Color.Red, solid.Color, "P141#3");
			Assert.AreEqual (Color.Red, (Brushes.YellowGreen as SolidBrush).Color, "P141#4");
			solid.Color = Color.YellowGreen; // revert to correct color (for other unit tests)
			*/
		}
	}
}

// Following code was used to generate the TestProperties method.
/*
using System;
using System.Drawing;
using System.Reflection;
class Program {
	static void Main ()
	{
		Type type = typeof (Brushes);
		PropertyInfo[] properties = type.GetProperties ();
		int count = 1;
		foreach (PropertyInfo property in properties) {
			Console.WriteLine("\n\t\t\tbr = Brushes." + property.Name + ";");
			Console.WriteLine("\t\t\tAssert.IsTrue ((br is SolidBrush), \"P" + count + "#1\");");
			Console.WriteLine("\t\t\tsolid = (SolidBrush) br;");
			Console.WriteLine("\t\t\tAssert.AreEqual (Color." + property.Name + ", solid.Color, \"P" + count + "#2\");");

			if (property.Name != "Red") {
				Console.WriteLine("\t\t\tsolid.Color = Color.Red;");
				Console.WriteLine("\t\t\tAssert.AreEqual (Color.Red, solid.Color, \"P" + count + "#3\");");
				Console.WriteLine("\t\t\tAssert.AreEqual (Color.Red, (Brushes." + property.Name + " as SolidBrush).Color, \"P" + count + "#4\");");
			} else {
				Console.WriteLine("\t\t\tsolid.Color = Color.White;");
				Console.WriteLine("\t\t\tAssert.AreEqual (Color.White, solid.Color, \"P" + count + "#3\");");
				Console.WriteLine("\t\t\tAssert.AreEqual (Color.White, (Brushes." + property.Name + " as SolidBrush).Color, \"P" + count + "#4\");");
			}
			Console.WriteLine("\t\t\tsolid.Color = Color." + property.Name + "; // revert to correct color (for other unit tests)");
			count++;
		}
	}
}
 */