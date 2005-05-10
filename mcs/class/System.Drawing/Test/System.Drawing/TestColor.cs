//
// Tests for System.Drawing.Color.cs
//
// Author:
//	Ravindra (rkumar@novell.com)
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


using System;
using System.Drawing;
using NUnit.Framework;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	public class ColorTest : Assertion
	{
		[SetUp]
		public void GetReady () { }

		[TearDown]
		public void Clean () { }

		[Test]
		public void TestArgbValues ()
		{
			Color color;

			color = Color.Transparent;
			AssertEquals ("#Transparent.A", 0, color.A);
			AssertEquals ("#Transparent.R", 255, color.R);
			AssertEquals ("#Transparent.G", 255, color.G);
			AssertEquals ("#Transparent.B", 255, color.B);

			color = Color.AliceBlue;
			AssertEquals ("#AliceBlue.A", 255, color.A);
			AssertEquals ("#AliceBlue.R", 240, color.R);
			AssertEquals ("#AliceBlue.G", 248, color.G);
			AssertEquals ("#AliceBlue.B", 255, color.B);

			color = Color.AntiqueWhite;
			AssertEquals ("#AntiqueWhite.A", 255, color.A);
			AssertEquals ("#AntiqueWhite.R", 250, color.R);
			AssertEquals ("#AntiqueWhite.G", 235, color.G);
			AssertEquals ("#AntiqueWhite.B", 215, color.B);

			color = Color.Aqua;
			AssertEquals ("#Aqua.A", 255, color.A);
			AssertEquals ("#Aqua.R", 0, color.R);
			AssertEquals ("#Aqua.G", 255, color.G);
			AssertEquals ("#Aqua.B", 255, color.B);

			color = Color.Aquamarine;
			AssertEquals ("#Aquamarine.A", 255, color.A);
			AssertEquals ("#Aquamarine.R", 127, color.R);
			AssertEquals ("#Aquamarine.G", 255, color.G);
			AssertEquals ("#Aquamarine.B", 212, color.B);

			color = Color.Azure;
			AssertEquals ("#Azure.A", 255, color.A);
			AssertEquals ("#Azure.R", 240, color.R);
			AssertEquals ("#Azure.G", 255, color.G);
			AssertEquals ("#Azure.B", 255, color.B);

			color = Color.Beige;
			AssertEquals ("#Beige.A", 255, color.A);
			AssertEquals ("#Beige.R", 245, color.R);
			AssertEquals ("#Beige.G", 245, color.G);
			AssertEquals ("#Beige.B", 220, color.B);

			color = Color.Bisque;
			AssertEquals ("#Bisque.A", 255, color.A);
			AssertEquals ("#Bisque.R", 255, color.R);
			AssertEquals ("#Bisque.G", 228, color.G);
			AssertEquals ("#Bisque.B", 196, color.B);

			color = Color.Black;
			AssertEquals ("#Black.A", 255, color.A);
			AssertEquals ("#Black.R", 0, color.R);
			AssertEquals ("#Black.G", 0, color.G);
			AssertEquals ("#Black.B", 0, color.B);

			color = Color.BlanchedAlmond;
			AssertEquals ("#BlanchedAlmond.A", 255, color.A);
			AssertEquals ("#BlanchedAlmond.R", 255, color.R);
			AssertEquals ("#BlanchedAlmond.G", 235, color.G);
			AssertEquals ("#BlanchedAlmond.B", 205, color.B);

			color = Color.Blue;
			AssertEquals ("#Blue.A", 255, color.A);
			AssertEquals ("#Blue.R", 0, color.R);
			AssertEquals ("#Blue.G", 0, color.G);
			AssertEquals ("#Blue.B", 255, color.B);

			color = Color.BlueViolet;
			AssertEquals ("#BlueViolet.A", 255, color.A);
			AssertEquals ("#BlueViolet.R", 138, color.R);
			AssertEquals ("#BlueViolet.G", 43, color.G);
			AssertEquals ("#BlueViolet.B", 226, color.B);

			color = Color.Brown;
			AssertEquals ("#Brown.A", 255, color.A);
			AssertEquals ("#Brown.R", 165, color.R);
			AssertEquals ("#Brown.G", 42, color.G);
			AssertEquals ("#Brown.B", 42, color.B);

			color = Color.BurlyWood;
			AssertEquals ("#BurlyWood.A", 255, color.A);
			AssertEquals ("#BurlyWood.R", 222, color.R);
			AssertEquals ("#BurlyWood.G", 184, color.G);
			AssertEquals ("#BurlyWood.B", 135, color.B);

			color = Color.CadetBlue;
			AssertEquals ("#CadetBlue.A", 255, color.A);
			AssertEquals ("#CadetBlue.R", 95, color.R);
			AssertEquals ("#CadetBlue.G", 158, color.G);
			AssertEquals ("#CadetBlue.B", 160, color.B);

			color = Color.Chartreuse;
			AssertEquals ("#Chartreuse.A", 255, color.A);
			AssertEquals ("#Chartreuse.R", 127, color.R);
			AssertEquals ("#Chartreuse.G", 255, color.G);
			AssertEquals ("#Chartreuse.B", 0, color.B);

			color = Color.Chocolate;
			AssertEquals ("#Chocolate.A", 255, color.A);
			AssertEquals ("#Chocolate.R", 210, color.R);
			AssertEquals ("#Chocolate.G", 105, color.G);
			AssertEquals ("#Chocolate.B", 30, color.B);

			color = Color.Coral;
			AssertEquals ("#Coral.A", 255, color.A);
			AssertEquals ("#Coral.R", 255, color.R);
			AssertEquals ("#Coral.G", 127, color.G);
			AssertEquals ("#Coral.B", 80, color.B);

			color = Color.CornflowerBlue;
			AssertEquals ("#CornflowerBlue.A", 255, color.A);
			AssertEquals ("#CornflowerBlue.R", 100, color.R);
			AssertEquals ("#CornflowerBlue.G", 149, color.G);
			AssertEquals ("#CornflowerBlue.B", 237, color.B);

			color = Color.Cornsilk;
			AssertEquals ("#Cornsilk.A", 255, color.A);
			AssertEquals ("#Cornsilk.R", 255, color.R);
			AssertEquals ("#Cornsilk.G", 248, color.G);
			AssertEquals ("#Cornsilk.B", 220, color.B);

			color = Color.Crimson;
			AssertEquals ("#Crimson.A", 255, color.A);
			AssertEquals ("#Crimson.R", 220, color.R);
			AssertEquals ("#Crimson.G", 20, color.G);
			AssertEquals ("#Crimson.B", 60, color.B);

			color = Color.Cyan;
			AssertEquals ("#Cyan.A", 255, color.A);
			AssertEquals ("#Cyan.R", 0, color.R);
			AssertEquals ("#Cyan.G", 255, color.G);
			AssertEquals ("#Cyan.B", 255, color.B);

			color = Color.DarkBlue;
			AssertEquals ("#DarkBlue.A", 255, color.A);
			AssertEquals ("#DarkBlue.R", 0, color.R);
			AssertEquals ("#DarkBlue.G", 0, color.G);
			AssertEquals ("#DarkBlue.B", 139, color.B);

			color = Color.DarkCyan;
			AssertEquals ("#DarkCyan.A", 255, color.A);
			AssertEquals ("#DarkCyan.R", 0, color.R);
			AssertEquals ("#DarkCyan.G", 139, color.G);
			AssertEquals ("#DarkCyan.B", 139, color.B);

			color = Color.DarkGoldenrod;
			AssertEquals ("#DarkGoldenrod.A", 255, color.A);
			AssertEquals ("#DarkGoldenrod.R", 184, color.R);
			AssertEquals ("#DarkGoldenrod.G", 134, color.G);
			AssertEquals ("#DarkGoldenrod.B", 11, color.B);

			color = Color.DarkGray;
			AssertEquals ("#DarkGray.A", 255, color.A);
			AssertEquals ("#DarkGray.R", 169, color.R);
			AssertEquals ("#DarkGray.G", 169, color.G);
			AssertEquals ("#DarkGray.B", 169, color.B);

			color = Color.DarkGreen;
			AssertEquals ("#DarkGreen.A", 255, color.A);
			AssertEquals ("#DarkGreen.R", 0, color.R);
			AssertEquals ("#DarkGreen.G", 100, color.G);
			AssertEquals ("#DarkGreen.B", 0, color.B);

			color = Color.DarkKhaki;
			AssertEquals ("#DarkKhaki.A", 255, color.A);
			AssertEquals ("#DarkKhaki.R", 189, color.R);
			AssertEquals ("#DarkKhaki.G", 183, color.G);
			AssertEquals ("#DarkKhaki.B", 107, color.B);

			color = Color.DarkMagenta;
			AssertEquals ("#DarkMagenta.A", 255, color.A);
			AssertEquals ("#DarkMagenta.R", 139, color.R);
			AssertEquals ("#DarkMagenta.G", 0, color.G);
			AssertEquals ("#DarkMagenta.B", 139, color.B);

			color = Color.DarkOliveGreen;
			AssertEquals ("#DarkOliveGreen.A", 255, color.A);
			AssertEquals ("#DarkOliveGreen.R", 85, color.R);
			AssertEquals ("#DarkOliveGreen.G", 107, color.G);
			AssertEquals ("#DarkOliveGreen.B", 47, color.B);

			color = Color.DarkOrange;
			AssertEquals ("#DarkOrange.A", 255, color.A);
			AssertEquals ("#DarkOrange.R", 255, color.R);
			AssertEquals ("#DarkOrange.G", 140, color.G);
			AssertEquals ("#DarkOrange.B", 0, color.B);

			color = Color.DarkOrchid;
			AssertEquals ("#DarkOrchid.A", 255, color.A);
			AssertEquals ("#DarkOrchid.R", 153, color.R);
			AssertEquals ("#DarkOrchid.G", 50, color.G);
			AssertEquals ("#DarkOrchid.B", 204, color.B);

			color = Color.DarkRed;
			AssertEquals ("#DarkRed.A", 255, color.A);
			AssertEquals ("#DarkRed.R", 139, color.R);
			AssertEquals ("#DarkRed.G", 0, color.G);
			AssertEquals ("#DarkRed.B", 0, color.B);

			color = Color.DarkSalmon;
			AssertEquals ("#DarkSalmon.A", 255, color.A);
			AssertEquals ("#DarkSalmon.R", 233, color.R);
			AssertEquals ("#DarkSalmon.G", 150, color.G);
			AssertEquals ("#DarkSalmon.B", 122, color.B);

			color = Color.DarkSeaGreen;
			AssertEquals ("#DarkSeaGreen.A", 255, color.A);
			AssertEquals ("#DarkSeaGreen.R", 143, color.R);
			AssertEquals ("#DarkSeaGreen.G", 188, color.G);
			AssertEquals ("#DarkSeaGreen.B", 139, color.B);

			color = Color.DarkSlateBlue;
			AssertEquals ("#DarkSlateBlue.A", 255, color.A);
			AssertEquals ("#DarkSlateBlue.R", 72, color.R);
			AssertEquals ("#DarkSlateBlue.G", 61, color.G);
			AssertEquals ("#DarkSlateBlue.B", 139, color.B);

			color = Color.DarkSlateGray;
			AssertEquals ("#DarkSlateGray.A", 255, color.A);
			AssertEquals ("#DarkSlateGray.R", 47, color.R);
			AssertEquals ("#DarkSlateGray.G", 79, color.G);
			AssertEquals ("#DarkSlateGray.B", 79, color.B);

			color = Color.DarkTurquoise;
			AssertEquals ("#DarkTurquoise.A", 255, color.A);
			AssertEquals ("#DarkTurquoise.R", 0, color.R);
			AssertEquals ("#DarkTurquoise.G", 206, color.G);
			AssertEquals ("#DarkTurquoise.B", 209, color.B);

			color = Color.DarkViolet;
			AssertEquals ("#DarkViolet.A", 255, color.A);
			AssertEquals ("#DarkViolet.R", 148, color.R);
			AssertEquals ("#DarkViolet.G", 0, color.G);
			AssertEquals ("#DarkViolet.B", 211, color.B);

			color = Color.DeepPink;
			AssertEquals ("#DeepPink.A", 255, color.A);
			AssertEquals ("#DeepPink.R", 255, color.R);
			AssertEquals ("#DeepPink.G", 20, color.G);
			AssertEquals ("#DeepPink.B", 147, color.B);

			color = Color.DeepSkyBlue;
			AssertEquals ("#DeepSkyBlue.A", 255, color.A);
			AssertEquals ("#DeepSkyBlue.R", 0, color.R);
			AssertEquals ("#DeepSkyBlue.G", 191, color.G);
			AssertEquals ("#DeepSkyBlue.B", 255, color.B);

			color = Color.DimGray;
			AssertEquals ("#DimGray.A", 255, color.A);
			AssertEquals ("#DimGray.R", 105, color.R);
			AssertEquals ("#DimGray.G", 105, color.G);
			AssertEquals ("#DimGray.B", 105, color.B);

			color = Color.DodgerBlue;
			AssertEquals ("#DodgerBlue.A", 255, color.A);
			AssertEquals ("#DodgerBlue.R", 30, color.R);
			AssertEquals ("#DodgerBlue.G", 144, color.G);
			AssertEquals ("#DodgerBlue.B", 255, color.B);

			color = Color.Firebrick;
			AssertEquals ("#Firebrick.A", 255, color.A);
			AssertEquals ("#Firebrick.R", 178, color.R);
			AssertEquals ("#Firebrick.G", 34, color.G);
			AssertEquals ("#Firebrick.B", 34, color.B);

			color = Color.FloralWhite;
			AssertEquals ("#FloralWhite.A", 255, color.A);
			AssertEquals ("#FloralWhite.R", 255, color.R);
			AssertEquals ("#FloralWhite.G", 250, color.G);
			AssertEquals ("#FloralWhite.B", 240, color.B);

			color = Color.ForestGreen;
			AssertEquals ("#ForestGreen.A", 255, color.A);
			AssertEquals ("#ForestGreen.R", 34, color.R);
			AssertEquals ("#ForestGreen.G", 139, color.G);
			AssertEquals ("#ForestGreen.B", 34, color.B);

			color = Color.Fuchsia;
			AssertEquals ("#Fuchsia.A", 255, color.A);
			AssertEquals ("#Fuchsia.R", 255, color.R);
			AssertEquals ("#Fuchsia.G", 0, color.G);
			AssertEquals ("#Fuchsia.B", 255, color.B);

			color = Color.Gainsboro;
			AssertEquals ("#Gainsboro.A", 255, color.A);
			AssertEquals ("#Gainsboro.R", 220, color.R);
			AssertEquals ("#Gainsboro.G", 220, color.G);
			AssertEquals ("#Gainsboro.B", 220, color.B);

			color = Color.GhostWhite;
			AssertEquals ("#GhostWhite.A", 255, color.A);
			AssertEquals ("#GhostWhite.R", 248, color.R);
			AssertEquals ("#GhostWhite.G", 248, color.G);
			AssertEquals ("#GhostWhite.B", 255, color.B);

			color = Color.Gold;
			AssertEquals ("#Gold.A", 255, color.A);
			AssertEquals ("#Gold.R", 255, color.R);
			AssertEquals ("#Gold.G", 215, color.G);
			AssertEquals ("#Gold.B", 0, color.B);

			color = Color.Goldenrod;
			AssertEquals ("#Goldenrod.A", 255, color.A);
			AssertEquals ("#Goldenrod.R", 218, color.R);
			AssertEquals ("#Goldenrod.G", 165, color.G);
			AssertEquals ("#Goldenrod.B", 32, color.B);

			color = Color.Gray;
			AssertEquals ("#Gray.A", 255, color.A);
			AssertEquals ("#Gray.R", 128, color.R);
			AssertEquals ("#Gray.G", 128, color.G);
			AssertEquals ("#Gray.B", 128, color.B);

			color = Color.Green;
			AssertEquals ("#Green.A", 255, color.A);
			AssertEquals ("#Green.R", 0, color.R);
			// This test should compare Green.G with 255, but
			// MS is using a value of 128 for Green.G
			AssertEquals ("#Green.G", 128, color.G);
			AssertEquals ("#Green.B", 0, color.B);

			color = Color.GreenYellow;
			AssertEquals ("#GreenYellow.A", 255, color.A);
			AssertEquals ("#GreenYellow.R", 173, color.R);
			AssertEquals ("#GreenYellow.G", 255, color.G);
			AssertEquals ("#GreenYellow.B", 47, color.B);

			color = Color.Honeydew;
			AssertEquals ("#Honeydew.A", 255, color.A);
			AssertEquals ("#Honeydew.R", 240, color.R);
			AssertEquals ("#Honeydew.G", 255, color.G);
			AssertEquals ("#Honeydew.B", 240, color.B);

			color = Color.HotPink;
			AssertEquals ("#HotPink.A", 255, color.A);
			AssertEquals ("#HotPink.R", 255, color.R);
			AssertEquals ("#HotPink.G", 105, color.G);
			AssertEquals ("#HotPink.B", 180, color.B);

			color = Color.IndianRed;
			AssertEquals ("#IndianRed.A", 255, color.A);
			AssertEquals ("#IndianRed.R", 205, color.R);
			AssertEquals ("#IndianRed.G", 92, color.G);
			AssertEquals ("#IndianRed.B", 92, color.B);

			color = Color.Indigo;
			AssertEquals ("#Indigo.A", 255, color.A);
			AssertEquals ("#Indigo.R", 75, color.R);
			AssertEquals ("#Indigo.G", 0, color.G);
			AssertEquals ("#Indigo.B", 130, color.B);

			color = Color.Ivory;
			AssertEquals ("#Ivory.A", 255, color.A);
			AssertEquals ("#Ivory.R", 255, color.R);
			AssertEquals ("#Ivory.G", 255, color.G);
			AssertEquals ("#Ivory.B", 240, color.B);

			color = Color.Khaki;
			AssertEquals ("#Khaki.A", 255, color.A);
			AssertEquals ("#Khaki.R", 240, color.R);
			AssertEquals ("#Khaki.G", 230, color.G);
			AssertEquals ("#Khaki.B", 140, color.B);

			color = Color.Lavender;
			AssertEquals ("#Lavender.A", 255, color.A);
			AssertEquals ("#Lavender.R", 230, color.R);
			AssertEquals ("#Lavender.G", 230, color.G);
			AssertEquals ("#Lavender.B", 250, color.B);

			color = Color.LavenderBlush;
			AssertEquals ("#LavenderBlush.A", 255, color.A);
			AssertEquals ("#LavenderBlush.R", 255, color.R);
			AssertEquals ("#LavenderBlush.G", 240, color.G);
			AssertEquals ("#LavenderBlush.B", 245, color.B);

			color = Color.LawnGreen;
			AssertEquals ("#LawnGreen.A", 255, color.A);
			AssertEquals ("#LawnGreen.R", 124, color.R);
			AssertEquals ("#LawnGreen.G", 252, color.G);
			AssertEquals ("#LawnGreen.B", 0, color.B);

			color = Color.LemonChiffon;
			AssertEquals ("#LemonChiffon.A", 255, color.A);
			AssertEquals ("#LemonChiffon.R", 255, color.R);
			AssertEquals ("#LemonChiffon.G", 250, color.G);
			AssertEquals ("#LemonChiffon.B", 205, color.B);

			color = Color.LightBlue;
			AssertEquals ("#LightBlue.A", 255, color.A);
			AssertEquals ("#LightBlue.R", 173, color.R);
			AssertEquals ("#LightBlue.G", 216, color.G);
			AssertEquals ("#LightBlue.B", 230, color.B);

			color = Color.LightCoral;
			AssertEquals ("#LightCoral.A", 255, color.A);
			AssertEquals ("#LightCoral.R", 240, color.R);
			AssertEquals ("#LightCoral.G", 128, color.G);
			AssertEquals ("#LightCoral.B", 128, color.B);

			color = Color.LightCyan;
			AssertEquals ("#LightCyan.A", 255, color.A);
			AssertEquals ("#LightCyan.R", 224, color.R);
			AssertEquals ("#LightCyan.G", 255, color.G);
			AssertEquals ("#LightCyan.B", 255, color.B);

			color = Color.LightGoldenrodYellow;
			AssertEquals ("#LightGoldenrodYellow.A", 255, color.A);
			AssertEquals ("#LightGoldenrodYellow.R", 250, color.R);
			AssertEquals ("#LightGoldenrodYellow.G", 250, color.G);
			AssertEquals ("#LightGoldenrodYellow.B", 210, color.B);

			color = Color.LightGreen;
			AssertEquals ("#LightGreen.A", 255, color.A);
			AssertEquals ("#LightGreen.R", 144, color.R);
			AssertEquals ("#LightGreen.G", 238, color.G);
			AssertEquals ("#LightGreen.B", 144, color.B);

			color = Color.LightGray;
			AssertEquals ("#LightGray.A", 255, color.A);
			AssertEquals ("#LightGray.R", 211, color.R);
			AssertEquals ("#LightGray.G", 211, color.G);
			AssertEquals ("#LightGray.B", 211, color.B);

			color = Color.LightPink;
			AssertEquals ("#LightPink.A", 255, color.A);
			AssertEquals ("#LightPink.R", 255, color.R);
			AssertEquals ("#LightPink.G", 182, color.G);
			AssertEquals ("#LightPink.B", 193, color.B);

			color = Color.LightSalmon;
			AssertEquals ("#LightSalmon.A", 255, color.A);
			AssertEquals ("#LightSalmon.R", 255, color.R);
			AssertEquals ("#LightSalmon.G", 160, color.G);
			AssertEquals ("#LightSalmon.B", 122, color.B);

			color = Color.LightSeaGreen;
			AssertEquals ("#LightSeaGreen.A", 255, color.A);
			AssertEquals ("#LightSeaGreen.R", 32, color.R);
			AssertEquals ("#LightSeaGreen.G", 178, color.G);
			AssertEquals ("#LightSeaGreen.B", 170, color.B);

			color = Color.LightSkyBlue;
			AssertEquals ("#LightSkyBlue.A", 255, color.A);
			AssertEquals ("#LightSkyBlue.R", 135, color.R);
			AssertEquals ("#LightSkyBlue.G", 206, color.G);
			AssertEquals ("#LightSkyBlue.B", 250, color.B);

			color = Color.LightSlateGray;
			AssertEquals ("#LightSlateGray.A", 255, color.A);
			AssertEquals ("#LightSlateGray.R", 119, color.R);
			AssertEquals ("#LightSlateGray.G", 136, color.G);
			AssertEquals ("#LightSlateGray.B", 153, color.B);

			color = Color.LightSteelBlue;
			AssertEquals ("#LightSteelBlue.A", 255, color.A);
			AssertEquals ("#LightSteelBlue.R", 176, color.R);
			AssertEquals ("#LightSteelBlue.G", 196, color.G);
			AssertEquals ("#LightSteelBlue.B", 222, color.B);

			color = Color.LightYellow;
			AssertEquals ("#LightYellow.A", 255, color.A);
			AssertEquals ("#LightYellow.R", 255, color.R);
			AssertEquals ("#LightYellow.G", 255, color.G);
			AssertEquals ("#LightYellow.B", 224, color.B);

			color = Color.Lime;
			AssertEquals ("#Lime.A", 255, color.A);
			AssertEquals ("#Lime.R", 0, color.R);
			AssertEquals ("#Lime.G", 255, color.G);
			AssertEquals ("#Lime.B", 0, color.B);

			color = Color.LimeGreen;
			AssertEquals ("#LimeGreen.A", 255, color.A);
			AssertEquals ("#LimeGreen.R", 50, color.R);
			AssertEquals ("#LimeGreen.G", 205, color.G);
			AssertEquals ("#LimeGreen.B", 50, color.B);

			color = Color.Linen;
			AssertEquals ("#Linen.A", 255, color.A);
			AssertEquals ("#Linen.R", 250, color.R);
			AssertEquals ("#Linen.G", 240, color.G);
			AssertEquals ("#Linen.B", 230, color.B);

			color = Color.Magenta;
			AssertEquals ("#Magenta.A", 255, color.A);
			AssertEquals ("#Magenta.R", 255, color.R);
			AssertEquals ("#Magenta.G", 0, color.G);
			AssertEquals ("#Magenta.B", 255, color.B);

			color = Color.Maroon;
			AssertEquals ("#Maroon.A", 255, color.A);
			AssertEquals ("#Maroon.R", 128, color.R);
			AssertEquals ("#Maroon.G", 0, color.G);
			AssertEquals ("#Maroon.B", 0, color.B);

			color = Color.MediumAquamarine;
			AssertEquals ("#MediumAquamarine.A", 255, color.A);
			AssertEquals ("#MediumAquamarine.R", 102, color.R);
			AssertEquals ("#MediumAquamarine.G", 205, color.G);
			AssertEquals ("#MediumAquamarine.B", 170, color.B);

			color = Color.MediumBlue;
			AssertEquals ("#MediumBlue.A", 255, color.A);
			AssertEquals ("#MediumBlue.R", 0, color.R);
			AssertEquals ("#MediumBlue.G", 0, color.G);
			AssertEquals ("#MediumBlue.B", 205, color.B);

			color = Color.MediumOrchid;
			AssertEquals ("#MediumOrchid.A", 255, color.A);
			AssertEquals ("#MediumOrchid.R", 186, color.R);
			AssertEquals ("#MediumOrchid.G", 85, color.G);
			AssertEquals ("#MediumOrchid.B", 211, color.B);

			color = Color.MediumPurple;
			AssertEquals ("#MediumPurple.A", 255, color.A);
			AssertEquals ("#MediumPurple.R", 147, color.R);
			AssertEquals ("#MediumPurple.G", 112, color.G);
			AssertEquals ("#MediumPurple.B", 219, color.B);

			color = Color.MediumSeaGreen;
			AssertEquals ("#MediumSeaGreen.A", 255, color.A);
			AssertEquals ("#MediumSeaGreen.R", 60, color.R);
			AssertEquals ("#MediumSeaGreen.G", 179, color.G);
			AssertEquals ("#MediumSeaGreen.B", 113, color.B);

			color = Color.MediumSlateBlue;
			AssertEquals ("#MediumSlateBlue.A", 255, color.A);
			AssertEquals ("#MediumSlateBlue.R", 123, color.R);
			AssertEquals ("#MediumSlateBlue.G", 104, color.G);
			AssertEquals ("#MediumSlateBlue.B", 238, color.B);

			color = Color.MediumSpringGreen;
			AssertEquals ("#MediumSpringGreen.A", 255, color.A);
			AssertEquals ("#MediumSpringGreen.R", 0, color.R);
			AssertEquals ("#MediumSpringGreen.G", 250, color.G);
			AssertEquals ("#MediumSpringGreen.B", 154, color.B);

			color = Color.MediumTurquoise;
			AssertEquals ("#MediumTurquoise.A", 255, color.A);
			AssertEquals ("#MediumTurquoise.R", 72, color.R);
			AssertEquals ("#MediumTurquoise.G", 209, color.G);
			AssertEquals ("#MediumTurquoise.B", 204, color.B);

			color = Color.MediumVioletRed;
			AssertEquals ("#MediumVioletRed.A", 255, color.A);
			AssertEquals ("#MediumVioletRed.R", 199, color.R);
			AssertEquals ("#MediumVioletRed.G", 21, color.G);
			AssertEquals ("#MediumVioletRed.B", 133, color.B);

			color = Color.MidnightBlue;
			AssertEquals ("#MidnightBlue.A", 255, color.A);
			AssertEquals ("#MidnightBlue.R", 25, color.R);
			AssertEquals ("#MidnightBlue.G", 25, color.G);
			AssertEquals ("#MidnightBlue.B", 112, color.B);

			color = Color.MintCream;
			AssertEquals ("#MintCream.A", 255, color.A);
			AssertEquals ("#MintCream.R", 245, color.R);
			AssertEquals ("#MintCream.G", 255, color.G);
			AssertEquals ("#MintCream.B", 250, color.B);

			color = Color.MistyRose;
			AssertEquals ("#MistyRose.A", 255, color.A);
			AssertEquals ("#MistyRose.R", 255, color.R);
			AssertEquals ("#MistyRose.G", 228, color.G);
			AssertEquals ("#MistyRose.B", 225, color.B);

			color = Color.Moccasin;
			AssertEquals ("#Moccasin.A", 255, color.A);
			AssertEquals ("#Moccasin.R", 255, color.R);
			AssertEquals ("#Moccasin.G", 228, color.G);
			AssertEquals ("#Moccasin.B", 181, color.B);

			color = Color.NavajoWhite;
			AssertEquals ("#NavajoWhite.A", 255, color.A);
			AssertEquals ("#NavajoWhite.R", 255, color.R);
			AssertEquals ("#NavajoWhite.G", 222, color.G);
			AssertEquals ("#NavajoWhite.B", 173, color.B);

			color = Color.Navy;
			AssertEquals ("#Navy.A", 255, color.A);
			AssertEquals ("#Navy.R", 0, color.R);
			AssertEquals ("#Navy.G", 0, color.G);
			AssertEquals ("#Navy.B", 128, color.B);

			color = Color.OldLace;
			AssertEquals ("#OldLace.A", 255, color.A);
			AssertEquals ("#OldLace.R", 253, color.R);
			AssertEquals ("#OldLace.G", 245, color.G);
			AssertEquals ("#OldLace.B", 230, color.B);

			color = Color.Olive;
			AssertEquals ("#Olive.A", 255, color.A);
			AssertEquals ("#Olive.R", 128, color.R);
			AssertEquals ("#Olive.G", 128, color.G);
			AssertEquals ("#Olive.B", 0, color.B);

			color = Color.OliveDrab;
			AssertEquals ("#OliveDrab.A", 255, color.A);
			AssertEquals ("#OliveDrab.R", 107, color.R);
			AssertEquals ("#OliveDrab.G", 142, color.G);
			AssertEquals ("#OliveDrab.B", 35, color.B);

			color = Color.Orange;
			AssertEquals ("#Orange.A", 255, color.A);
			AssertEquals ("#Orange.R", 255, color.R);
			AssertEquals ("#Orange.G", 165, color.G);
			AssertEquals ("#Orange.B", 0, color.B);

			color = Color.OrangeRed;
			AssertEquals ("#OrangeRed.A", 255, color.A);
			AssertEquals ("#OrangeRed.R", 255, color.R);
			AssertEquals ("#OrangeRed.G", 69, color.G);
			AssertEquals ("#OrangeRed.B", 0, color.B);

			color = Color.Orchid;
			AssertEquals ("#Orchid.A", 255, color.A);
			AssertEquals ("#Orchid.R", 218, color.R);
			AssertEquals ("#Orchid.G", 112, color.G);
			AssertEquals ("#Orchid.B", 214, color.B);

			color = Color.PaleGoldenrod;
			AssertEquals ("#PaleGoldenrod.A", 255, color.A);
			AssertEquals ("#PaleGoldenrod.R", 238, color.R);
			AssertEquals ("#PaleGoldenrod.G", 232, color.G);
			AssertEquals ("#PaleGoldenrod.B", 170, color.B);

			color = Color.PaleGreen;
			AssertEquals ("#PaleGreen.A", 255, color.A);
			AssertEquals ("#PaleGreen.R", 152, color.R);
			AssertEquals ("#PaleGreen.G", 251, color.G);
			AssertEquals ("#PaleGreen.B", 152, color.B);

			color = Color.PaleTurquoise;
			AssertEquals ("#PaleTurquoise.A", 255, color.A);
			AssertEquals ("#PaleTurquoise.R", 175, color.R);
			AssertEquals ("#PaleTurquoise.G", 238, color.G);
			AssertEquals ("#PaleTurquoise.B", 238, color.B);

			color = Color.PaleVioletRed;
			AssertEquals ("#PaleVioletRed.A", 255, color.A);
			AssertEquals ("#PaleVioletRed.R", 219, color.R);
			AssertEquals ("#PaleVioletRed.G", 112, color.G);
			AssertEquals ("#PaleVioletRed.B", 147, color.B);

			color = Color.PapayaWhip;
			AssertEquals ("#PapayaWhip.A", 255, color.A);
			AssertEquals ("#PapayaWhip.R", 255, color.R);
			AssertEquals ("#PapayaWhip.G", 239, color.G);
			AssertEquals ("#PapayaWhip.B", 213, color.B);

			color = Color.PeachPuff;
			AssertEquals ("#PeachPuff.A", 255, color.A);
			AssertEquals ("#PeachPuff.R", 255, color.R);
			AssertEquals ("#PeachPuff.G", 218, color.G);
			AssertEquals ("#PeachPuff.B", 185, color.B);

			color = Color.Peru;
			AssertEquals ("#Peru.A", 255, color.A);
			AssertEquals ("#Peru.R", 205, color.R);
			AssertEquals ("#Peru.G", 133, color.G);
			AssertEquals ("#Peru.B", 63, color.B);

			color = Color.Pink;
			AssertEquals ("#Pink.A", 255, color.A);
			AssertEquals ("#Pink.R", 255, color.R);
			AssertEquals ("#Pink.G", 192, color.G);
			AssertEquals ("#Pink.B", 203, color.B);

			color = Color.Plum;
			AssertEquals ("#Plum.A", 255, color.A);
			AssertEquals ("#Plum.R", 221, color.R);
			AssertEquals ("#Plum.G", 160, color.G);
			AssertEquals ("#Plum.B", 221, color.B);

			color = Color.PowderBlue;
			AssertEquals ("#PowderBlue.A", 255, color.A);
			AssertEquals ("#PowderBlue.R", 176, color.R);
			AssertEquals ("#PowderBlue.G", 224, color.G);
			AssertEquals ("#PowderBlue.B", 230, color.B);

			color = Color.Purple;
			AssertEquals ("#Purple.A", 255, color.A);
			AssertEquals ("#Purple.R", 128, color.R);
			AssertEquals ("#Purple.G", 0, color.G);
			AssertEquals ("#Purple.B", 128, color.B);

			color = Color.Red;
			AssertEquals ("#Red.A", 255, color.A);
			AssertEquals ("#Red.R", 255, color.R);
			AssertEquals ("#Red.G", 0, color.G);
			AssertEquals ("#Red.B", 0, color.B);

			color = Color.RosyBrown;
			AssertEquals ("#RosyBrown.A", 255, color.A);
			AssertEquals ("#RosyBrown.R", 188, color.R);
			AssertEquals ("#RosyBrown.G", 143, color.G);
			AssertEquals ("#RosyBrown.B", 143, color.B);

			color = Color.RoyalBlue;
			AssertEquals ("#RoyalBlue.A", 255, color.A);
			AssertEquals ("#RoyalBlue.R", 65, color.R);
			AssertEquals ("#RoyalBlue.G", 105, color.G);
			AssertEquals ("#RoyalBlue.B", 225, color.B);

			color = Color.SaddleBrown;
			AssertEquals ("#SaddleBrown.A", 255, color.A);
			AssertEquals ("#SaddleBrown.R", 139, color.R);
			AssertEquals ("#SaddleBrown.G", 69, color.G);
			AssertEquals ("#SaddleBrown.B", 19, color.B);

			color = Color.Salmon;
			AssertEquals ("#Salmon.A", 255, color.A);
			AssertEquals ("#Salmon.R", 250, color.R);
			AssertEquals ("#Salmon.G", 128, color.G);
			AssertEquals ("#Salmon.B", 114, color.B);

			color = Color.SandyBrown;
			AssertEquals ("#SandyBrown.A", 255, color.A);
			AssertEquals ("#SandyBrown.R", 244, color.R);
			AssertEquals ("#SandyBrown.G", 164, color.G);
			AssertEquals ("#SandyBrown.B", 96, color.B);

			color = Color.SeaGreen;
			AssertEquals ("#SeaGreen.A", 255, color.A);
			AssertEquals ("#SeaGreen.R", 46, color.R);
			AssertEquals ("#SeaGreen.G", 139, color.G);
			AssertEquals ("#SeaGreen.B", 87, color.B);

			color = Color.SeaShell;
			AssertEquals ("#SeaShell.A", 255, color.A);
			AssertEquals ("#SeaShell.R", 255, color.R);
			AssertEquals ("#SeaShell.G", 245, color.G);
			AssertEquals ("#SeaShell.B", 238, color.B);

			color = Color.Sienna;
			AssertEquals ("#Sienna.A", 255, color.A);
			AssertEquals ("#Sienna.R", 160, color.R);
			AssertEquals ("#Sienna.G", 82, color.G);
			AssertEquals ("#Sienna.B", 45, color.B);

			color = Color.Silver;
			AssertEquals ("#Silver.A", 255, color.A);
			AssertEquals ("#Silver.R", 192, color.R);
			AssertEquals ("#Silver.G", 192, color.G);
			AssertEquals ("#Silver.B", 192, color.B);

			color = Color.SkyBlue;
			AssertEquals ("#SkyBlue.A", 255, color.A);
			AssertEquals ("#SkyBlue.R", 135, color.R);
			AssertEquals ("#SkyBlue.G", 206, color.G);
			AssertEquals ("#SkyBlue.B", 235, color.B);

			color = Color.SlateBlue;
			AssertEquals ("#SlateBlue.A", 255, color.A);
			AssertEquals ("#SlateBlue.R", 106, color.R);
			AssertEquals ("#SlateBlue.G", 90, color.G);
			AssertEquals ("#SlateBlue.B", 205, color.B);

			color = Color.SlateGray;
			AssertEquals ("#SlateGray.A", 255, color.A);
			AssertEquals ("#SlateGray.R", 112, color.R);
			AssertEquals ("#SlateGray.G", 128, color.G);
			AssertEquals ("#SlateGray.B", 144, color.B);

			color = Color.Snow;
			AssertEquals ("#Snow.A", 255, color.A);
			AssertEquals ("#Snow.R", 255, color.R);
			AssertEquals ("#Snow.G", 250, color.G);
			AssertEquals ("#Snow.B", 250, color.B);

			color = Color.SpringGreen;
			AssertEquals ("#SpringGreen.A", 255, color.A);
			AssertEquals ("#SpringGreen.R", 0, color.R);
			AssertEquals ("#SpringGreen.G", 255, color.G);
			AssertEquals ("#SpringGreen.B", 127, color.B);

			color = Color.SteelBlue;
			AssertEquals ("#SteelBlue.A", 255, color.A);
			AssertEquals ("#SteelBlue.R", 70, color.R);
			AssertEquals ("#SteelBlue.G", 130, color.G);
			AssertEquals ("#SteelBlue.B", 180, color.B);

			color = Color.Tan;
			AssertEquals ("#Tan.A", 255, color.A);
			AssertEquals ("#Tan.R", 210, color.R);
			AssertEquals ("#Tan.G", 180, color.G);
			AssertEquals ("#Tan.B", 140, color.B);

			color = Color.Teal;
			AssertEquals ("#Teal.A", 255, color.A);
			AssertEquals ("#Teal.R", 0, color.R);
			AssertEquals ("#Teal.G", 128, color.G);
			AssertEquals ("#Teal.B", 128, color.B);

			color = Color.Thistle;
			AssertEquals ("#Thistle.A", 255, color.A);
			AssertEquals ("#Thistle.R", 216, color.R);
			AssertEquals ("#Thistle.G", 191, color.G);
			AssertEquals ("#Thistle.B", 216, color.B);

			color = Color.Tomato;
			AssertEquals ("#Tomato.A", 255, color.A);
			AssertEquals ("#Tomato.R", 255, color.R);
			AssertEquals ("#Tomato.G", 99, color.G);
			AssertEquals ("#Tomato.B", 71, color.B);

			color = Color.Turquoise;
			AssertEquals ("#Turquoise.A", 255, color.A);
			AssertEquals ("#Turquoise.R", 64, color.R);
			AssertEquals ("#Turquoise.G", 224, color.G);
			AssertEquals ("#Turquoise.B", 208, color.B);

			color = Color.Violet;
			AssertEquals ("#Violet.A", 255, color.A);
			AssertEquals ("#Violet.R", 238, color.R);
			AssertEquals ("#Violet.G", 130, color.G);
			AssertEquals ("#Violet.B", 238, color.B);

			color = Color.Wheat;
			AssertEquals ("#Wheat.A", 255, color.A);
			AssertEquals ("#Wheat.R", 245, color.R);
			AssertEquals ("#Wheat.G", 222, color.G);
			AssertEquals ("#Wheat.B", 179, color.B);

			color = Color.White;
			AssertEquals ("#White.A", 255, color.A);
			AssertEquals ("#White.R", 255, color.R);
			AssertEquals ("#White.G", 255, color.G);
			AssertEquals ("#White.B", 255, color.B);

			color = Color.WhiteSmoke;
			AssertEquals ("#WhiteSmoke.A", 255, color.A);
			AssertEquals ("#WhiteSmoke.R", 245, color.R);
			AssertEquals ("#WhiteSmoke.G", 245, color.G);
			AssertEquals ("#WhiteSmoke.B", 245, color.B);

			color = Color.Yellow;
			AssertEquals ("#Yellow.A", 255, color.A);
			AssertEquals ("#Yellow.R", 255, color.R);
			AssertEquals ("#Yellow.G", 255, color.G);
			AssertEquals ("#Yellow.B", 0, color.B);

			color = Color.YellowGreen;
			AssertEquals ("#YellowGreen.A", 255, color.A);
			AssertEquals ("#YellowGreen.R", 154, color.R);
			AssertEquals ("#YellowGreen.G", 205, color.G);
			AssertEquals ("#YellowGreen.B", 50, color.B);
		}
		
		[Test]
		public void TestHBSValues ()
		{
			AssertEquals ("BrightnessBlack", 0.0f, Color.Black.GetBrightness ());
			AssertEquals ("BrightnessWhite", 1.0f, Color.White.GetBrightness ());
		
			Color c1 = Color.FromArgb (0, 13, 45, 7); //just some random color
			AssertEquals ("Hue1",        110.5263f, c1.GetHue ());
			AssertEquals ("Brightness1", 0.1019608f, c1.GetBrightness ());
			AssertEquals ("Saturation1", 0.7307692f, c1.GetSaturation ());
	
			Color c2 = Color.FromArgb (0, 112, 75, 29); //another random color
			AssertEquals ("Hue2",        33.25302f, c2.GetHue ());
			AssertEquals ("Brightness2", 0.2764706f, c2.GetBrightness ());
			AssertEquals ("Saturation2", 0.5886525f, c2.GetSaturation ());
		}
	}
}

// Following code was used to generate the TestArgbValues method on MS. 
//
//		Type cType = typeof (Color);
//		PropertyInfo [] properties = cType.GetProperties (BindingFlags.Public | BindingFlags.Static);
////		Console.WriteLine ("number: " + properties.Length);
//
//		Console.WriteLine("\t\t[Test]");
//		Console.WriteLine("\t\tpublic void TestArgbValues ()");
//		Console.WriteLine("\t\t{");
//		Console.WriteLine("\t\t\tColor color;");
//		foreach (PropertyInfo property in properties) {
//
//			Console.WriteLine();
//			Console.WriteLine("\t\t\tcolor = Color.{0};", property.Name);
//			Color color = Color.FromName (property.Name);
//			Console.WriteLine("\t\t\tAssertEquals (\"#{0}.A\", {1}, color.A);", property.Name, color.A);
//			Console.WriteLine("\t\t\tAssertEquals (\"#{0}.R\", {1}, color.R);", property.Name, color.R);
//			Console.WriteLine("\t\t\tAssertEquals (\"#{0}.G\", {1}, color.G);", property.Name, color.G);
//			Console.WriteLine("\t\t\tAssertEquals (\"#{0}.B\", {1}, color.B);", property.Name, color.B);
//		}
//		Console.WriteLine("\t\t}");
