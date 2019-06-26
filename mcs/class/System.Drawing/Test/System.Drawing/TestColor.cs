//
// Tests for System.Drawing.Color.cs
//
// Authors:
//	Ravindra (rkumar@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ColorTest {

		[Test]
		public void TestArgbValues ()
		{
			Color color;

			color = Color.Transparent;
			Assert.AreEqual (0, color.A, "#Transparent.A");
			Assert.AreEqual (255, color.R, "#Transparent.R");
			Assert.AreEqual (255, color.G, "#Transparent.G");
			Assert.AreEqual (255, color.B, "#Transparent.B");

			color = Color.AliceBlue;
			Assert.AreEqual (255, color.A, "#AliceBlue.A");
			Assert.AreEqual (240, color.R, "#AliceBlue.R");
			Assert.AreEqual (248, color.G, "#AliceBlue.G");
			Assert.AreEqual (255, color.B, "#AliceBlue.B");

			color = Color.AntiqueWhite;
			Assert.AreEqual (255, color.A, "#AntiqueWhite.A");
			Assert.AreEqual (250, color.R, "#AntiqueWhite.R");
			Assert.AreEqual (235, color.G, "#AntiqueWhite.G");
			Assert.AreEqual (215, color.B, "#AntiqueWhite.B");

			color = Color.Aqua;
			Assert.AreEqual (255, color.A, "#Aqua.A");
			Assert.AreEqual (0, color.R, "#Aqua.R");
			Assert.AreEqual (255, color.G, "#Aqua.G");
			Assert.AreEqual (255, color.B, "#Aqua.B");

			color = Color.Aquamarine;
			Assert.AreEqual (255, color.A, "#Aquamarine.A");
			Assert.AreEqual (127, color.R, "#Aquamarine.R");
			Assert.AreEqual (255, color.G, "#Aquamarine.G");
			Assert.AreEqual (212, color.B, "#Aquamarine.B");

			color = Color.Azure;
			Assert.AreEqual (255, color.A, "#Azure.A");
			Assert.AreEqual (240, color.R, "#Azure.R");
			Assert.AreEqual (255, color.G, "#Azure.G");
			Assert.AreEqual (255, color.B, "#Azure.B");

			color = Color.Beige;
			Assert.AreEqual (255, color.A, "#Beige.A");
			Assert.AreEqual (245, color.R, "#Beige.R");
			Assert.AreEqual (245, color.G, "#Beige.G");
			Assert.AreEqual (220, color.B, "#Beige.B");

			color = Color.Bisque;
			Assert.AreEqual (255, color.A, "#Bisque.A");
			Assert.AreEqual (255, color.R, "#Bisque.R");
			Assert.AreEqual (228, color.G, "#Bisque.G");
			Assert.AreEqual (196, color.B, "#Bisque.B");

			color = Color.Black;
			Assert.AreEqual (255, color.A, "#Black.A");
			Assert.AreEqual (0, color.R, "#Black.R");
			Assert.AreEqual (0, color.G, "#Black.G");
			Assert.AreEqual (0, color.B, "#Black.B");

			color = Color.BlanchedAlmond;
			Assert.AreEqual (255, color.A, "#BlanchedAlmond.A");
			Assert.AreEqual (255, color.R, "#BlanchedAlmond.R");
			Assert.AreEqual (235, color.G, "#BlanchedAlmond.G");
			Assert.AreEqual (205, color.B, "#BlanchedAlmond.B");

			color = Color.Blue;
			Assert.AreEqual (255, color.A, "#Blue.A");
			Assert.AreEqual (0, color.R, "#Blue.R");
			Assert.AreEqual (0, color.G, "#Blue.G");
			Assert.AreEqual (255, color.B, "#Blue.B");

			color = Color.BlueViolet;
			Assert.AreEqual (255, color.A, "#BlueViolet.A");
			Assert.AreEqual (138, color.R, "#BlueViolet.R");
			Assert.AreEqual (43, color.G, "#BlueViolet.G");
			Assert.AreEqual (226, color.B, "#BlueViolet.B");

			color = Color.Brown;
			Assert.AreEqual (255, color.A, "#Brown.A");
			Assert.AreEqual (165, color.R, "#Brown.R");
			Assert.AreEqual (42, color.G, "#Brown.G");
			Assert.AreEqual (42, color.B, "#Brown.B");

			color = Color.BurlyWood;
			Assert.AreEqual (255, color.A, "#BurlyWood.A");
			Assert.AreEqual (222, color.R, "#BurlyWood.R");
			Assert.AreEqual (184, color.G, "#BurlyWood.G");
			Assert.AreEqual (135, color.B, "#BurlyWood.B");

			color = Color.CadetBlue;
			Assert.AreEqual (255, color.A, "#CadetBlue.A");
			Assert.AreEqual (95, color.R, "#CadetBlue.R");
			Assert.AreEqual (158, color.G, "#CadetBlue.G");
			Assert.AreEqual (160, color.B, "#CadetBlue.B");

			color = Color.Chartreuse;
			Assert.AreEqual (255, color.A, "#Chartreuse.A");
			Assert.AreEqual (127, color.R, "#Chartreuse.R");
			Assert.AreEqual (255, color.G, "#Chartreuse.G");
			Assert.AreEqual (0, color.B, "#Chartreuse.B");

			color = Color.Chocolate;
			Assert.AreEqual (255, color.A, "#Chocolate.A");
			Assert.AreEqual (210, color.R, "#Chocolate.R");
			Assert.AreEqual (105, color.G, "#Chocolate.G");
			Assert.AreEqual (30, color.B, "#Chocolate.B");

			color = Color.Coral;
			Assert.AreEqual (255, color.A, "#Coral.A");
			Assert.AreEqual (255, color.R, "#Coral.R");
			Assert.AreEqual (127, color.G, "#Coral.G");
			Assert.AreEqual (80, color.B, "#Coral.B");

			color = Color.CornflowerBlue;
			Assert.AreEqual (255, color.A, "#CornflowerBlue.A");
			Assert.AreEqual (100, color.R, "#CornflowerBlue.R");
			Assert.AreEqual (149, color.G, "#CornflowerBlue.G");
			Assert.AreEqual (237, color.B, "#CornflowerBlue.B");

			color = Color.Cornsilk;
			Assert.AreEqual (255, color.A, "#Cornsilk.A");
			Assert.AreEqual (255, color.R, "#Cornsilk.R");
			Assert.AreEqual (248, color.G, "#Cornsilk.G");
			Assert.AreEqual (220, color.B, "#Cornsilk.B");

			color = Color.Crimson;
			Assert.AreEqual (255, color.A, "#Crimson.A");
			Assert.AreEqual (220, color.R, "#Crimson.R");
			Assert.AreEqual (20, color.G, "#Crimson.G");
			Assert.AreEqual (60, color.B, "#Crimson.B");

			color = Color.Cyan;
			Assert.AreEqual (255, color.A, "#Cyan.A");
			Assert.AreEqual (0, color.R, "#Cyan.R");
			Assert.AreEqual (255, color.G, "#Cyan.G");
			Assert.AreEqual (255, color.B, "#Cyan.B");

			color = Color.DarkBlue;
			Assert.AreEqual (255, color.A, "#DarkBlue.A");
			Assert.AreEqual (0, color.R, "#DarkBlue.R");
			Assert.AreEqual (0, color.G, "#DarkBlue.G");
			Assert.AreEqual (139, color.B, "#DarkBlue.B");

			color = Color.DarkCyan;
			Assert.AreEqual (255, color.A, "#DarkCyan.A");
			Assert.AreEqual (0, color.R, "#DarkCyan.R");
			Assert.AreEqual (139, color.G, "#DarkCyan.G");
			Assert.AreEqual (139, color.B, "#DarkCyan.B");

			color = Color.DarkGoldenrod;
			Assert.AreEqual (255, color.A, "#DarkGoldenrod.A");
			Assert.AreEqual (184, color.R, "#DarkGoldenrod.R");
			Assert.AreEqual (134, color.G, "#DarkGoldenrod.G");
			Assert.AreEqual (11, color.B, "#DarkGoldenrod.B");

			color = Color.DarkGray;
			Assert.AreEqual (255, color.A, "#DarkGray.A");
			Assert.AreEqual (169, color.R, "#DarkGray.R");
			Assert.AreEqual (169, color.G, "#DarkGray.G");
			Assert.AreEqual (169, color.B, "#DarkGray.B");

			color = Color.DarkGreen;
			Assert.AreEqual (255, color.A, "#DarkGreen.A");
			Assert.AreEqual (0, color.R, "#DarkGreen.R");
			Assert.AreEqual (100, color.G, "#DarkGreen.G");
			Assert.AreEqual (0, color.B, "#DarkGreen.B");

			color = Color.DarkKhaki;
			Assert.AreEqual (255, color.A, "#DarkKhaki.A");
			Assert.AreEqual (189, color.R, "#DarkKhaki.R");
			Assert.AreEqual (183, color.G, "#DarkKhaki.G");
			Assert.AreEqual (107, color.B, "#DarkKhaki.B");

			color = Color.DarkMagenta;
			Assert.AreEqual (255, color.A, "#DarkMagenta.A");
			Assert.AreEqual (139, color.R, "#DarkMagenta.R");
			Assert.AreEqual (0, color.G, "#DarkMagenta.G");
			Assert.AreEqual (139, color.B, "#DarkMagenta.B");

			color = Color.DarkOliveGreen;
			Assert.AreEqual (255, color.A, "#DarkOliveGreen.A");
			Assert.AreEqual (85, color.R, "#DarkOliveGreen.R");
			Assert.AreEqual (107, color.G, "#DarkOliveGreen.G");
			Assert.AreEqual (47, color.B, "#DarkOliveGreen.B");

			color = Color.DarkOrange;
			Assert.AreEqual (255, color.A, "#DarkOrange.A");
			Assert.AreEqual (255, color.R, "#DarkOrange.R");
			Assert.AreEqual (140, color.G, "#DarkOrange.G");
			Assert.AreEqual (0, color.B, "#DarkOrange.B");

			color = Color.DarkOrchid;
			Assert.AreEqual (255, color.A, "#DarkOrchid.A");
			Assert.AreEqual (153, color.R, "#DarkOrchid.R");
			Assert.AreEqual (50, color.G, "#DarkOrchid.G");
			Assert.AreEqual (204, color.B, "#DarkOrchid.B");

			color = Color.DarkRed;
			Assert.AreEqual (255, color.A, "#DarkRed.A");
			Assert.AreEqual (139, color.R, "#DarkRed.R");
			Assert.AreEqual (0, color.G, "#DarkRed.G");
			Assert.AreEqual (0, color.B, "#DarkRed.B");

			color = Color.DarkSalmon;
			Assert.AreEqual (255, color.A, "#DarkSalmon.A");
			Assert.AreEqual (233, color.R, "#DarkSalmon.R");
			Assert.AreEqual (150, color.G, "#DarkSalmon.G");
			Assert.AreEqual (122, color.B, "#DarkSalmon.B");

			color = Color.DarkSeaGreen;
			Assert.AreEqual (255, color.A, "#DarkSeaGreen.A");
			Assert.AreEqual (143, color.R, "#DarkSeaGreen.R");
			Assert.AreEqual (188, color.G, "#DarkSeaGreen.G");
			Assert.AreEqual (139, color.B, "#DarkSeaGreen.B");

			color = Color.DarkSlateBlue;
			Assert.AreEqual (255, color.A, "#DarkSlateBlue.A");
			Assert.AreEqual (72, color.R, "#DarkSlateBlue.R");
			Assert.AreEqual (61, color.G, "#DarkSlateBlue.G");
			Assert.AreEqual (139, color.B, "#DarkSlateBlue.B");

			color = Color.DarkSlateGray;
			Assert.AreEqual (255, color.A, "#DarkSlateGray.A");
			Assert.AreEqual (47, color.R, "#DarkSlateGray.R");
			Assert.AreEqual (79, color.G, "#DarkSlateGray.G");
			Assert.AreEqual (79, color.B, "#DarkSlateGray.B");

			color = Color.DarkTurquoise;
			Assert.AreEqual (255, color.A, "#DarkTurquoise.A");
			Assert.AreEqual (0, color.R, "#DarkTurquoise.R");
			Assert.AreEqual (206, color.G, "#DarkTurquoise.G");
			Assert.AreEqual (209, color.B, "#DarkTurquoise.B");

			color = Color.DarkViolet;
			Assert.AreEqual (255, color.A, "#DarkViolet.A");
			Assert.AreEqual (148, color.R, "#DarkViolet.R");
			Assert.AreEqual (0, color.G, "#DarkViolet.G");
			Assert.AreEqual (211, color.B, "#DarkViolet.B");

			color = Color.DeepPink;
			Assert.AreEqual (255, color.A, "#DeepPink.A");
			Assert.AreEqual (255, color.R, "#DeepPink.R");
			Assert.AreEqual (20, color.G, "#DeepPink.G");
			Assert.AreEqual (147, color.B, "#DeepPink.B");

			color = Color.DeepSkyBlue;
			Assert.AreEqual (255, color.A, "#DeepSkyBlue.A");
			Assert.AreEqual (0, color.R, "#DeepSkyBlue.R");
			Assert.AreEqual (191, color.G, "#DeepSkyBlue.G");
			Assert.AreEqual (255, color.B, "#DeepSkyBlue.B");

			color = Color.DimGray;
			Assert.AreEqual (255, color.A, "#DimGray.A");
			Assert.AreEqual (105, color.R, "#DimGray.R");
			Assert.AreEqual (105, color.G, "#DimGray.G");
			Assert.AreEqual (105, color.B, "#DimGray.B");

			color = Color.DodgerBlue;
			Assert.AreEqual (255, color.A, "#DodgerBlue.A");
			Assert.AreEqual (30, color.R, "#DodgerBlue.R");
			Assert.AreEqual (144, color.G, "#DodgerBlue.G");
			Assert.AreEqual (255, color.B, "#DodgerBlue.B");

			color = Color.Firebrick;
			Assert.AreEqual (255, color.A, "#Firebrick.A");
			Assert.AreEqual (178, color.R, "#Firebrick.R");
			Assert.AreEqual (34, color.G, "#Firebrick.G");
			Assert.AreEqual (34, color.B, "#Firebrick.B");

			color = Color.FloralWhite;
			Assert.AreEqual (255, color.A, "#FloralWhite.A");
			Assert.AreEqual (255, color.R, "#FloralWhite.R");
			Assert.AreEqual (250, color.G, "#FloralWhite.G");
			Assert.AreEqual (240, color.B, "#FloralWhite.B");

			color = Color.ForestGreen;
			Assert.AreEqual (255, color.A, "#ForestGreen.A");
			Assert.AreEqual (34, color.R, "#ForestGreen.R");
			Assert.AreEqual (139, color.G, "#ForestGreen.G");
			Assert.AreEqual (34, color.B, "#ForestGreen.B");

			color = Color.Fuchsia;
			Assert.AreEqual (255, color.A, "#Fuchsia.A");
			Assert.AreEqual (255, color.R, "#Fuchsia.R");
			Assert.AreEqual (0, color.G, "#Fuchsia.G");
			Assert.AreEqual (255, color.B, "#Fuchsia.B");

			color = Color.Gainsboro;
			Assert.AreEqual (255, color.A, "#Gainsboro.A");
			Assert.AreEqual (220, color.R, "#Gainsboro.R");
			Assert.AreEqual (220, color.G, "#Gainsboro.G");
			Assert.AreEqual (220, color.B, "#Gainsboro.B");

			color = Color.GhostWhite;
			Assert.AreEqual (255, color.A, "#GhostWhite.A");
			Assert.AreEqual (248, color.R, "#GhostWhite.R");
			Assert.AreEqual (248, color.G, "#GhostWhite.G");
			Assert.AreEqual (255, color.B, "#GhostWhite.B");

			color = Color.Gold;
			Assert.AreEqual (255, color.A, "#Gold.A");
			Assert.AreEqual (255, color.R, "#Gold.R");
			Assert.AreEqual (215, color.G, "#Gold.G");
			Assert.AreEqual (0, color.B, "#Gold.B");

			color = Color.Goldenrod;
			Assert.AreEqual (255, color.A, "#Goldenrod.A");
			Assert.AreEqual (218, color.R, "#Goldenrod.R");
			Assert.AreEqual (165, color.G, "#Goldenrod.G");
			Assert.AreEqual (32, color.B, "#Goldenrod.B");

			color = Color.Gray;
			Assert.AreEqual (255, color.A, "#Gray.A");
			Assert.AreEqual (128, color.R, "#Gray.R");
			Assert.AreEqual (128, color.G, "#Gray.G");
			Assert.AreEqual (128, color.B, "#Gray.B");

			color = Color.Green;
			Assert.AreEqual (255, color.A, "#Green.A");
			Assert.AreEqual (0, color.R, "#Green.R");
			// This test should compare Green.G with 255, but
			// MS is using a value of 128 for Green.G
			Assert.AreEqual (128, color.G, "#Green.G");
			Assert.AreEqual (0, color.B, "#Green.B");

			color = Color.GreenYellow;
			Assert.AreEqual (255, color.A, "#GreenYellow.A");
			Assert.AreEqual (173, color.R, "#GreenYellow.R");
			Assert.AreEqual (255, color.G, "#GreenYellow.G");
			Assert.AreEqual (47, color.B, "#GreenYellow.B");

			color = Color.Honeydew;
			Assert.AreEqual (255, color.A, "#Honeydew.A");
			Assert.AreEqual (240, color.R, "#Honeydew.R");
			Assert.AreEqual (255, color.G, "#Honeydew.G");
			Assert.AreEqual (240, color.B, "#Honeydew.B");

			color = Color.HotPink;
			Assert.AreEqual (255, color.A, "#HotPink.A");
			Assert.AreEqual (255, color.R, "#HotPink.R");
			Assert.AreEqual (105, color.G, "#HotPink.G");
			Assert.AreEqual (180, color.B, "#HotPink.B");

			color = Color.IndianRed;
			Assert.AreEqual (255, color.A, "#IndianRed.A");
			Assert.AreEqual (205, color.R, "#IndianRed.R");
			Assert.AreEqual (92, color.G, "#IndianRed.G");
			Assert.AreEqual (92, color.B, "#IndianRed.B");

			color = Color.Indigo;
			Assert.AreEqual (255, color.A, "#Indigo.A");
			Assert.AreEqual (75, color.R, "#Indigo.R");
			Assert.AreEqual (0, color.G, "#Indigo.G");
			Assert.AreEqual (130, color.B, "#Indigo.B");

			color = Color.Ivory;
			Assert.AreEqual (255, color.A, "#Ivory.A");
			Assert.AreEqual (255, color.R, "#Ivory.R");
			Assert.AreEqual (255, color.G, "#Ivory.G");
			Assert.AreEqual (240, color.B, "#Ivory.B");

			color = Color.Khaki;
			Assert.AreEqual (255, color.A, "#Khaki.A");
			Assert.AreEqual (240, color.R, "#Khaki.R");
			Assert.AreEqual (230, color.G, "#Khaki.G");
			Assert.AreEqual (140, color.B, "#Khaki.B");

			color = Color.Lavender;
			Assert.AreEqual (255, color.A, "#Lavender.A");
			Assert.AreEqual (230, color.R, "#Lavender.R");
			Assert.AreEqual (230, color.G, "#Lavender.G");
			Assert.AreEqual (250, color.B, "#Lavender.B");

			color = Color.LavenderBlush;
			Assert.AreEqual (255, color.A, "#LavenderBlush.A");
			Assert.AreEqual (255, color.R, "#LavenderBlush.R");
			Assert.AreEqual (240, color.G, "#LavenderBlush.G");
			Assert.AreEqual (245, color.B, "#LavenderBlush.B");

			color = Color.LawnGreen;
			Assert.AreEqual (255, color.A, "#LawnGreen.A");
			Assert.AreEqual (124, color.R, "#LawnGreen.R");
			Assert.AreEqual (252, color.G, "#LawnGreen.G");
			Assert.AreEqual (0, color.B, "#LawnGreen.B");

			color = Color.LemonChiffon;
			Assert.AreEqual (255, color.A, "#LemonChiffon.A");
			Assert.AreEqual (255, color.R, "#LemonChiffon.R");
			Assert.AreEqual (250, color.G, "#LemonChiffon.G");
			Assert.AreEqual (205, color.B, "#LemonChiffon.B");

			color = Color.LightBlue;
			Assert.AreEqual (255, color.A, "#LightBlue.A");
			Assert.AreEqual (173, color.R, "#LightBlue.R");
			Assert.AreEqual (216, color.G, "#LightBlue.G");
			Assert.AreEqual (230, color.B, "#LightBlue.B");

			color = Color.LightCoral;
			Assert.AreEqual (255, color.A, "#LightCoral.A");
			Assert.AreEqual (240, color.R, "#LightCoral.R");
			Assert.AreEqual (128, color.G, "#LightCoral.G");
			Assert.AreEqual (128, color.B, "#LightCoral.B");

			color = Color.LightCyan;
			Assert.AreEqual (255, color.A, "#LightCyan.A");
			Assert.AreEqual (224, color.R, "#LightCyan.R");
			Assert.AreEqual (255, color.G, "#LightCyan.G");
			Assert.AreEqual (255, color.B, "#LightCyan.B");

			color = Color.LightGoldenrodYellow;
			Assert.AreEqual (255, color.A, "#LightGoldenrodYellow.A");
			Assert.AreEqual (250, color.R, "#LightGoldenrodYellow.R");
			Assert.AreEqual (250, color.G, "#LightGoldenrodYellow.G");
			Assert.AreEqual (210, color.B, "#LightGoldenrodYellow.B");

			color = Color.LightGreen;
			Assert.AreEqual (255, color.A, "#LightGreen.A");
			Assert.AreEqual (144, color.R, "#LightGreen.R");
			Assert.AreEqual (238, color.G, "#LightGreen.G");
			Assert.AreEqual (144, color.B, "#LightGreen.B");

			color = Color.LightGray;
			Assert.AreEqual (255, color.A, "#LightGray.A");
			Assert.AreEqual (211, color.R, "#LightGray.R");
			Assert.AreEqual (211, color.G, "#LightGray.G");
			Assert.AreEqual (211, color.B, "#LightGray.B");

			color = Color.LightPink;
			Assert.AreEqual (255, color.A, "#LightPink.A");
			Assert.AreEqual (255, color.R, "#LightPink.R");
			Assert.AreEqual (182, color.G, "#LightPink.G");
			Assert.AreEqual (193, color.B, "#LightPink.B");

			color = Color.LightSalmon;
			Assert.AreEqual (255, color.A, "#LightSalmon.A");
			Assert.AreEqual (255, color.R, "#LightSalmon.R");
			Assert.AreEqual (160, color.G, "#LightSalmon.G");
			Assert.AreEqual (122, color.B, "#LightSalmon.B");

			color = Color.LightSeaGreen;
			Assert.AreEqual (255, color.A, "#LightSeaGreen.A");
			Assert.AreEqual (32, color.R, "#LightSeaGreen.R");
			Assert.AreEqual (178, color.G, "#LightSeaGreen.G");
			Assert.AreEqual (170, color.B, "#LightSeaGreen.B");

			color = Color.LightSkyBlue;
			Assert.AreEqual (255, color.A, "#LightSkyBlue.A");
			Assert.AreEqual (135, color.R, "#LightSkyBlue.R");
			Assert.AreEqual (206, color.G, "#LightSkyBlue.G");
			Assert.AreEqual (250, color.B, "#LightSkyBlue.B");

			color = Color.LightSlateGray;
			Assert.AreEqual (255, color.A, "#LightSlateGray.A");
			Assert.AreEqual (119, color.R, "#LightSlateGray.R");
			Assert.AreEqual (136, color.G, "#LightSlateGray.G");
			Assert.AreEqual (153, color.B, "#LightSlateGray.B");

			color = Color.LightSteelBlue;
			Assert.AreEqual (255, color.A, "#LightSteelBlue.A");
			Assert.AreEqual (176, color.R, "#LightSteelBlue.R");
			Assert.AreEqual (196, color.G, "#LightSteelBlue.G");
			Assert.AreEqual (222, color.B, "#LightSteelBlue.B");

			color = Color.LightYellow;
			Assert.AreEqual (255, color.A, "#LightYellow.A");
			Assert.AreEqual (255, color.R, "#LightYellow.R");
			Assert.AreEqual (255, color.G, "#LightYellow.G");
			Assert.AreEqual (224, color.B, "#LightYellow.B");

			color = Color.Lime;
			Assert.AreEqual (255, color.A, "#Lime.A");
			Assert.AreEqual (0, color.R, "#Lime.R");
			Assert.AreEqual (255, color.G, "#Lime.G");
			Assert.AreEqual (0, color.B, "#Lime.B");

			color = Color.LimeGreen;
			Assert.AreEqual (255, color.A, "#LimeGreen.A");
			Assert.AreEqual (50, color.R, "#LimeGreen.R");
			Assert.AreEqual (205, color.G, "#LimeGreen.G");
			Assert.AreEqual (50, color.B, "#LimeGreen.B");

			color = Color.Linen;
			Assert.AreEqual (255, color.A, "#Linen.A");
			Assert.AreEqual (250, color.R, "#Linen.R");
			Assert.AreEqual (240, color.G, "#Linen.G");
			Assert.AreEqual (230, color.B, "#Linen.B");

			color = Color.Magenta;
			Assert.AreEqual (255, color.A, "#Magenta.A");
			Assert.AreEqual (255, color.R, "#Magenta.R");
			Assert.AreEqual (0, color.G, "#Magenta.G");
			Assert.AreEqual (255, color.B, "#Magenta.B");

			color = Color.Maroon;
			Assert.AreEqual (255, color.A, "#Maroon.A");
			Assert.AreEqual (128, color.R, "#Maroon.R");
			Assert.AreEqual (0, color.G, "#Maroon.G");
			Assert.AreEqual (0, color.B, "#Maroon.B");

			color = Color.MediumAquamarine;
			Assert.AreEqual (255, color.A, "#MediumAquamarine.A");
			Assert.AreEqual (102, color.R, "#MediumAquamarine.R");
			Assert.AreEqual (205, color.G, "#MediumAquamarine.G");
			Assert.AreEqual (170, color.B, "#MediumAquamarine.B");

			color = Color.MediumBlue;
			Assert.AreEqual (255, color.A, "#MediumBlue.A");
			Assert.AreEqual (0, color.R, "#MediumBlue.R");
			Assert.AreEqual (0, color.G, "#MediumBlue.G");
			Assert.AreEqual (205, color.B, "#MediumBlue.B");

			color = Color.MediumOrchid;
			Assert.AreEqual (255, color.A, "#MediumOrchid.A");
			Assert.AreEqual (186, color.R, "#MediumOrchid.R");
			Assert.AreEqual (85, color.G, "#MediumOrchid.G");
			Assert.AreEqual (211, color.B, "#MediumOrchid.B");

			color = Color.MediumPurple;
			Assert.AreEqual (255, color.A, "#MediumPurple.A");
			Assert.AreEqual (147, color.R, "#MediumPurple.R");
			Assert.AreEqual (112, color.G, "#MediumPurple.G");
			Assert.AreEqual (219, color.B, "#MediumPurple.B");

			color = Color.MediumSeaGreen;
			Assert.AreEqual (255, color.A, "#MediumSeaGreen.A");
			Assert.AreEqual (60, color.R, "#MediumSeaGreen.R");
			Assert.AreEqual (179, color.G, "#MediumSeaGreen.G");
			Assert.AreEqual (113, color.B, "#MediumSeaGreen.B");

			color = Color.MediumSlateBlue;
			Assert.AreEqual (255, color.A, "#MediumSlateBlue.A");
			Assert.AreEqual (123, color.R, "#MediumSlateBlue.R");
			Assert.AreEqual (104, color.G, "#MediumSlateBlue.G");
			Assert.AreEqual (238, color.B, "#MediumSlateBlue.B");

			color = Color.MediumSpringGreen;
			Assert.AreEqual (255, color.A, "#MediumSpringGreen.A");
			Assert.AreEqual (0, color.R, "#MediumSpringGreen.R");
			Assert.AreEqual (250, color.G, "#MediumSpringGreen.G");
			Assert.AreEqual (154, color.B, "#MediumSpringGreen.B");

			color = Color.MediumTurquoise;
			Assert.AreEqual (255, color.A, "#MediumTurquoise.A");
			Assert.AreEqual (72, color.R, "#MediumTurquoise.R");
			Assert.AreEqual (209, color.G, "#MediumTurquoise.G");
			Assert.AreEqual (204, color.B, "#MediumTurquoise.B");

			color = Color.MediumVioletRed;
			Assert.AreEqual (255, color.A, "#MediumVioletRed.A");
			Assert.AreEqual (199, color.R, "#MediumVioletRed.R");
			Assert.AreEqual (21, color.G, "#MediumVioletRed.G");
			Assert.AreEqual (133, color.B, "#MediumVioletRed.B");

			color = Color.MidnightBlue;
			Assert.AreEqual (255, color.A, "#MidnightBlue.A");
			Assert.AreEqual (25, color.R, "#MidnightBlue.R");
			Assert.AreEqual (25, color.G, "#MidnightBlue.G");
			Assert.AreEqual (112, color.B, "#MidnightBlue.B");

			color = Color.MintCream;
			Assert.AreEqual (255, color.A, "#MintCream.A");
			Assert.AreEqual (245, color.R, "#MintCream.R");
			Assert.AreEqual (255, color.G, "#MintCream.G");
			Assert.AreEqual (250, color.B, "#MintCream.B");

			color = Color.MistyRose;
			Assert.AreEqual (255, color.A, "#MistyRose.A");
			Assert.AreEqual (255, color.R, "#MistyRose.R");
			Assert.AreEqual (228, color.G, "#MistyRose.G");
			Assert.AreEqual (225, color.B, "#MistyRose.B");

			color = Color.Moccasin;
			Assert.AreEqual (255, color.A, "#Moccasin.A");
			Assert.AreEqual (255, color.R, "#Moccasin.R");
			Assert.AreEqual (228, color.G, "#Moccasin.G");
			Assert.AreEqual (181, color.B, "#Moccasin.B");

			color = Color.NavajoWhite;
			Assert.AreEqual (255, color.A, "#NavajoWhite.A");
			Assert.AreEqual (255, color.R, "#NavajoWhite.R");
			Assert.AreEqual (222, color.G, "#NavajoWhite.G");
			Assert.AreEqual (173, color.B, "#NavajoWhite.B");

			color = Color.Navy;
			Assert.AreEqual (255, color.A, "#Navy.A");
			Assert.AreEqual (0, color.R, "#Navy.R");
			Assert.AreEqual (0, color.G, "#Navy.G");
			Assert.AreEqual (128, color.B, "#Navy.B");

			color = Color.OldLace;
			Assert.AreEqual (255, color.A, "#OldLace.A");
			Assert.AreEqual (253, color.R, "#OldLace.R");
			Assert.AreEqual (245, color.G, "#OldLace.G");
			Assert.AreEqual (230, color.B, "#OldLace.B");

			color = Color.Olive;
			Assert.AreEqual (255, color.A, "#Olive.A");
			Assert.AreEqual (128, color.R, "#Olive.R");
			Assert.AreEqual (128, color.G, "#Olive.G");
			Assert.AreEqual (0, color.B, "#Olive.B");

			color = Color.OliveDrab;
			Assert.AreEqual (255, color.A, "#OliveDrab.A");
			Assert.AreEqual (107, color.R, "#OliveDrab.R");
			Assert.AreEqual (142, color.G, "#OliveDrab.G");
			Assert.AreEqual (35, color.B, "#OliveDrab.B");

			color = Color.Orange;
			Assert.AreEqual (255, color.A, "#Orange.A");
			Assert.AreEqual (255, color.R, "#Orange.R");
			Assert.AreEqual (165, color.G, "#Orange.G");
			Assert.AreEqual (0, color.B, "#Orange.B");

			color = Color.OrangeRed;
			Assert.AreEqual (255, color.A, "#OrangeRed.A");
			Assert.AreEqual (255, color.R, "#OrangeRed.R");
			Assert.AreEqual (69, color.G, "#OrangeRed.G");
			Assert.AreEqual (0, color.B, "#OrangeRed.B");

			color = Color.Orchid;
			Assert.AreEqual (255, color.A, "#Orchid.A");
			Assert.AreEqual (218, color.R, "#Orchid.R");
			Assert.AreEqual (112, color.G, "#Orchid.G");
			Assert.AreEqual (214, color.B, "#Orchid.B");

			color = Color.PaleGoldenrod;
			Assert.AreEqual (255, color.A, "#PaleGoldenrod.A");
			Assert.AreEqual (238, color.R, "#PaleGoldenrod.R");
			Assert.AreEqual (232, color.G, "#PaleGoldenrod.G");
			Assert.AreEqual (170, color.B, "#PaleGoldenrod.B");

			color = Color.PaleGreen;
			Assert.AreEqual (255, color.A, "#PaleGreen.A");
			Assert.AreEqual (152, color.R, "#PaleGreen.R");
			Assert.AreEqual (251, color.G, "#PaleGreen.G");
			Assert.AreEqual (152, color.B, "#PaleGreen.B");

			color = Color.PaleTurquoise;
			Assert.AreEqual (255, color.A, "#PaleTurquoise.A");
			Assert.AreEqual (175, color.R, "#PaleTurquoise.R");
			Assert.AreEqual (238, color.G, "#PaleTurquoise.G");
			Assert.AreEqual (238, color.B, "#PaleTurquoise.B");

			color = Color.PaleVioletRed;
			Assert.AreEqual (255, color.A, "#PaleVioletRed.A");
			Assert.AreEqual (219, color.R, "#PaleVioletRed.R");
			Assert.AreEqual (112, color.G, "#PaleVioletRed.G");
			Assert.AreEqual (147, color.B, "#PaleVioletRed.B");

			color = Color.PapayaWhip;
			Assert.AreEqual (255, color.A, "#PapayaWhip.A");
			Assert.AreEqual (255, color.R, "#PapayaWhip.R");
			Assert.AreEqual (239, color.G, "#PapayaWhip.G");
			Assert.AreEqual (213, color.B, "#PapayaWhip.B");

			color = Color.PeachPuff;
			Assert.AreEqual (255, color.A, "#PeachPuff.A");
			Assert.AreEqual (255, color.R, "#PeachPuff.R");
			Assert.AreEqual (218, color.G, "#PeachPuff.G");
			Assert.AreEqual (185, color.B, "#PeachPuff.B");

			color = Color.Peru;
			Assert.AreEqual (255, color.A, "#Peru.A");
			Assert.AreEqual (205, color.R, "#Peru.R");
			Assert.AreEqual (133, color.G, "#Peru.G");
			Assert.AreEqual (63, color.B, "#Peru.B");

			color = Color.Pink;
			Assert.AreEqual (255, color.A, "#Pink.A");
			Assert.AreEqual (255, color.R, "#Pink.R");
			Assert.AreEqual (192, color.G, "#Pink.G");
			Assert.AreEqual (203, color.B, "#Pink.B");

			color = Color.Plum;
			Assert.AreEqual (255, color.A, "#Plum.A");
			Assert.AreEqual (221, color.R, "#Plum.R");
			Assert.AreEqual (160, color.G, "#Plum.G");
			Assert.AreEqual (221, color.B, "#Plum.B");

			color = Color.PowderBlue;
			Assert.AreEqual (255, color.A, "#PowderBlue.A");
			Assert.AreEqual (176, color.R, "#PowderBlue.R");
			Assert.AreEqual (224, color.G, "#PowderBlue.G");
			Assert.AreEqual (230, color.B, "#PowderBlue.B");

			color = Color.Purple;
			Assert.AreEqual (255, color.A, "#Purple.A");
			Assert.AreEqual (128, color.R, "#Purple.R");
			Assert.AreEqual (0, color.G, "#Purple.G");
			Assert.AreEqual (128, color.B, "#Purple.B");

			color = Color.Red;
			Assert.AreEqual (255, color.A, "#Red.A");
			Assert.AreEqual (255, color.R, "#Red.R");
			Assert.AreEqual (0, color.G, "#Red.G");
			Assert.AreEqual (0, color.B, "#Red.B");

			color = Color.RosyBrown;
			Assert.AreEqual (255, color.A, "#RosyBrown.A");
			Assert.AreEqual (188, color.R, "#RosyBrown.R");
			Assert.AreEqual (143, color.G, "#RosyBrown.G");
			Assert.AreEqual (143, color.B, "#RosyBrown.B");

			color = Color.RoyalBlue;
			Assert.AreEqual (255, color.A, "#RoyalBlue.A");
			Assert.AreEqual (65, color.R, "#RoyalBlue.R");
			Assert.AreEqual (105, color.G, "#RoyalBlue.G");
			Assert.AreEqual (225, color.B, "#RoyalBlue.B");

			color = Color.SaddleBrown;
			Assert.AreEqual (255, color.A, "#SaddleBrown.A");
			Assert.AreEqual (139, color.R, "#SaddleBrown.R");
			Assert.AreEqual (69, color.G, "#SaddleBrown.G");
			Assert.AreEqual (19, color.B, "#SaddleBrown.B");

			color = Color.Salmon;
			Assert.AreEqual (255, color.A, "#Salmon.A");
			Assert.AreEqual (250, color.R, "#Salmon.R");
			Assert.AreEqual (128, color.G, "#Salmon.G");
			Assert.AreEqual (114, color.B, "#Salmon.B");

			color = Color.SandyBrown;
			Assert.AreEqual (255, color.A, "#SandyBrown.A");
			Assert.AreEqual (244, color.R, "#SandyBrown.R");
			Assert.AreEqual (164, color.G, "#SandyBrown.G");
			Assert.AreEqual (96, color.B, "#SandyBrown.B");

			color = Color.SeaGreen;
			Assert.AreEqual (255, color.A, "#SeaGreen.A");
			Assert.AreEqual (46, color.R, "#SeaGreen.R");
			Assert.AreEqual (139, color.G, "#SeaGreen.G");
			Assert.AreEqual (87, color.B, "#SeaGreen.B");

			color = Color.SeaShell;
			Assert.AreEqual (255, color.A, "#SeaShell.A");
			Assert.AreEqual (255, color.R, "#SeaShell.R");
			Assert.AreEqual (245, color.G, "#SeaShell.G");
			Assert.AreEqual (238, color.B, "#SeaShell.B");

			color = Color.Sienna;
			Assert.AreEqual (255, color.A, "#Sienna.A");
			Assert.AreEqual (160, color.R, "#Sienna.R");
			Assert.AreEqual (82, color.G, "#Sienna.G");
			Assert.AreEqual (45, color.B, "#Sienna.B");

			color = Color.Silver;
			Assert.AreEqual (255, color.A, "#Silver.A");
			Assert.AreEqual (192, color.R, "#Silver.R");
			Assert.AreEqual (192, color.G, "#Silver.G");
			Assert.AreEqual (192, color.B, "#Silver.B");

			color = Color.SkyBlue;
			Assert.AreEqual (255, color.A, "#SkyBlue.A");
			Assert.AreEqual (135, color.R, "#SkyBlue.R");
			Assert.AreEqual (206, color.G, "#SkyBlue.G");
			Assert.AreEqual (235, color.B, "#SkyBlue.B");

			color = Color.SlateBlue;
			Assert.AreEqual (255, color.A, "#SlateBlue.A");
			Assert.AreEqual (106, color.R, "#SlateBlue.R");
			Assert.AreEqual (90, color.G, "#SlateBlue.G");
			Assert.AreEqual (205, color.B, "#SlateBlue.B");

			color = Color.SlateGray;
			Assert.AreEqual (255, color.A, "#SlateGray.A");
			Assert.AreEqual (112, color.R, "#SlateGray.R");
			Assert.AreEqual (128, color.G, "#SlateGray.G");
			Assert.AreEqual (144, color.B, "#SlateGray.B");

			color = Color.Snow;
			Assert.AreEqual (255, color.A, "#Snow.A");
			Assert.AreEqual (255, color.R, "#Snow.R");
			Assert.AreEqual (250, color.G, "#Snow.G");
			Assert.AreEqual (250, color.B, "#Snow.B");

			color = Color.SpringGreen;
			Assert.AreEqual (255, color.A, "#SpringGreen.A");
			Assert.AreEqual (0, color.R, "#SpringGreen.R");
			Assert.AreEqual (255, color.G, "#SpringGreen.G");
			Assert.AreEqual (127, color.B, "#SpringGreen.B");

			color = Color.SteelBlue;
			Assert.AreEqual (255, color.A, "#SteelBlue.A");
			Assert.AreEqual (70, color.R, "#SteelBlue.R");
			Assert.AreEqual (130, color.G, "#SteelBlue.G");
			Assert.AreEqual (180, color.B, "#SteelBlue.B");

			color = Color.Tan;
			Assert.AreEqual (255, color.A, "#Tan.A");
			Assert.AreEqual (210, color.R, "#Tan.R");
			Assert.AreEqual (180, color.G, "#Tan.G");
			Assert.AreEqual (140, color.B, "#Tan.B");

			color = Color.Teal;
			Assert.AreEqual (255, color.A, "#Teal.A");
			Assert.AreEqual (0, color.R, "#Teal.R");
			Assert.AreEqual (128, color.G, "#Teal.G");
			Assert.AreEqual (128, color.B, "#Teal.B");

			color = Color.Thistle;
			Assert.AreEqual (255, color.A, "#Thistle.A");
			Assert.AreEqual (216, color.R, "#Thistle.R");
			Assert.AreEqual (191, color.G, "#Thistle.G");
			Assert.AreEqual (216, color.B, "#Thistle.B");

			color = Color.Tomato;
			Assert.AreEqual (255, color.A, "#Tomato.A");
			Assert.AreEqual (255, color.R, "#Tomato.R");
			Assert.AreEqual (99, color.G, "#Tomato.G");
			Assert.AreEqual (71, color.B, "#Tomato.B");

			color = Color.Turquoise;
			Assert.AreEqual (255, color.A, "#Turquoise.A");
			Assert.AreEqual (64, color.R, "#Turquoise.R");
			Assert.AreEqual (224, color.G, "#Turquoise.G");
			Assert.AreEqual (208, color.B, "#Turquoise.B");

			color = Color.Violet;
			Assert.AreEqual (255, color.A, "#Violet.A");
			Assert.AreEqual (238, color.R, "#Violet.R");
			Assert.AreEqual (130, color.G, "#Violet.G");
			Assert.AreEqual (238, color.B, "#Violet.B");

			color = Color.Wheat;
			Assert.AreEqual (255, color.A, "#Wheat.A");
			Assert.AreEqual (245, color.R, "#Wheat.R");
			Assert.AreEqual (222, color.G, "#Wheat.G");
			Assert.AreEqual (179, color.B, "#Wheat.B");

			color = Color.White;
			Assert.AreEqual (255, color.A, "#White.A");
			Assert.AreEqual (255, color.R, "#White.R");
			Assert.AreEqual (255, color.G, "#White.G");
			Assert.AreEqual (255, color.B, "#White.B");

			color = Color.WhiteSmoke;
			Assert.AreEqual (255, color.A, "#WhiteSmoke.A");
			Assert.AreEqual (245, color.R, "#WhiteSmoke.R");
			Assert.AreEqual (245, color.G, "#WhiteSmoke.G");
			Assert.AreEqual (245, color.B, "#WhiteSmoke.B");

			color = Color.Yellow;
			Assert.AreEqual (255, color.A, "#Yellow.A");
			Assert.AreEqual (255, color.R, "#Yellow.R");
			Assert.AreEqual (255, color.G, "#Yellow.G");
			Assert.AreEqual (0, color.B, "#Yellow.B");

			color = Color.YellowGreen;
			Assert.AreEqual (255, color.A, "#YellowGreen.A");
			Assert.AreEqual (154, color.R, "#YellowGreen.R");
			Assert.AreEqual (205, color.G, "#YellowGreen.G");
			Assert.AreEqual (50, color.B, "#YellowGreen.B");
		}
		
		static bool FloatsAlmostEqual (float v1, float v2)
		{
			float error = Math.Abs(v1-v2)/(v1+v2+float.Epsilon);
			return error < 0.0001;
		}

		[Test]
		public void TestHBSValues ()
		{
			Assert.AreEqual (0.0f, Color.Black.GetBrightness (), "BrightnessBlack");
			Assert.AreEqual (1.0f, Color.White.GetBrightness (), "BrightnessWhite");
		
			Color c1 = Color.FromArgb (0, 13, 45, 7); //just some random color
			Assert.IsTrue (FloatsAlmostEqual (110.5263f, c1.GetHue ()), "Hue1");
			Assert.IsTrue (FloatsAlmostEqual (0.1019608f, c1.GetBrightness ()), "Brightness1");
			Assert.IsTrue (FloatsAlmostEqual (0.7307692f, c1.GetSaturation ()), "Saturation1");
	
			Color c2 = Color.FromArgb (0, 112, 75, 29); //another random color
			Assert.IsTrue (FloatsAlmostEqual (33.25302f, c2.GetHue ()), "Hue2");
			Assert.IsTrue (FloatsAlmostEqual (0.2764706f, c2.GetBrightness ()), "Brightness2");
			Assert.IsTrue (FloatsAlmostEqual (0.5886525f, c2.GetSaturation ()), "Saturation2");
		}
		[Test]
		public void TestEquals ()
		{
			Color color = Color.Red;
			Color color1 = Color.FromArgb (color.R, color.G, color.B);
			Assert.IsTrue (!(color == color1), "Named color not == unnamed color");
			Assert.IsTrue (!(color1 == color), "Named unnamed color == color not");
			Assert.IsTrue ((color != color1), "Named color != unnamed color");
			Assert.IsTrue ((color1 != color), "Named unnamed color != color not");
			Assert.IsTrue (!color.Equals (color1), "Named color not equals unnamed color");
			Assert.IsTrue (!color1.Equals (color), "Named unnamed color equals color not");
			color = Color.FromArgb (0, color1);
			Assert.IsTrue (!color.Equals (color1), "Alpha takes part in comparison");
		}
		[Test]
		public void TestIsEmpty ()
		{
			Color color = Color.Empty;
			Assert.IsTrue (color.IsEmpty, "Empty color");
			Assert.IsTrue (!Color.FromArgb (0, Color.Black).IsEmpty, "Not empty color");
		}

		[Test]
		public void IsKnownColor ()
		{
			Assert.IsTrue (Color.FromKnownColor(KnownColor.AliceBlue).IsKnownColor, "KnownColor");
			Assert.IsTrue (Color.FromName("AliceBlue").IsKnownColor, "KnownColor");
			Assert.AreEqual (false, Color.FromArgb (Color.AliceBlue.A, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B).IsKnownColor, "Not KnownColor");
		}

		[Test]
		public void IsNamedColor ()
		{
			Assert.IsTrue (Color.AliceBlue.IsNamedColor, "NamedColor");
			Assert.IsTrue (Color.FromKnownColor(KnownColor.AliceBlue).IsNamedColor, "NamedColor");
			Assert.IsTrue (Color.FromName("AliceBlue").IsNamedColor, "NamedColor");
			Assert.AreEqual (false, Color.FromArgb (Color.AliceBlue.A, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B).IsNamedColor, "Not NamedColor");
		}

		[Test]
		public void IsSystemColor ()
		{
			Color c = Color.FromKnownColor (KnownColor.ActiveBorder);
			Assert.IsTrue (c.IsSystemColor, "SystemColor#1");
			Assert.IsTrue (Color.FromName("ActiveBorder").IsSystemColor, "SystemColor#2");
			Assert.AreEqual (false, Color.FromArgb (c.A, c.R, c.G, c.B).IsSystemColor, "Not SystemColor#1");
			Assert.AreEqual (false, Color.FromKnownColor(KnownColor.AliceBlue).IsSystemColor, "Not SystemColor#2");
			Assert.AreEqual (false, Color.FromName("AliceBlue").IsSystemColor, "Not SystemColor#3");
		}

		[Test]
		public void Name ()
		{
			Assert.AreEqual ("AliceBlue", Color.AliceBlue.Name, "Color.Name#1");
			Assert.AreEqual ("ActiveBorder", Color.FromKnownColor (KnownColor.ActiveBorder).Name, "Color.Name#2");
			Assert.AreEqual ("1122ccff", Color.FromArgb(0x11, 0x22, 0xcc, 0xff).Name, "Color.Name#3");
		}
		[Test]
		public void GetHashCodeTest ()
		{
			Color c = Color.AliceBlue;
			Assert.AreEqual (false, Color.FromArgb (c.A, c.R, c.G, c.B).GetHashCode () == c.GetHashCode (), "GHC#1");
			Assert.AreEqual (c.GetHashCode (), Color.FromName ("AliceBlue").GetHashCode (), "GHC#2");
		}
		[Test]
		public void ToArgb ()
		{
			Assert.AreEqual (0x11cc8833, Color.FromArgb (0x11, 0xcc, 0x88, 0x33).ToArgb (), "#1");
			Assert.AreEqual (unchecked((int)0xf1cc8833), Color.FromArgb (0xf1, 0xcc, 0x88, 0x33).ToArgb (), "#2");
		}

		[Test]
		public void ToKnownColor ()
		{
			Assert.AreEqual (KnownColor.ActiveBorder, Color.FromName ("ActiveBorder").ToKnownColor (), "TKC#1");
			Assert.AreEqual (KnownColor.AliceBlue, Color.AliceBlue.ToKnownColor (), "TKC#2");
			KnownColor zero = Color.FromArgb (1, 2, 3, 4).ToKnownColor ();
			Assert.AreEqual ((KnownColor)0, zero, "TKC#3");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("Color [AliceBlue]", Color.AliceBlue.ToString (), "TS#1");
			Assert.AreEqual ("Color [ActiveBorder]", Color.FromKnownColor (KnownColor.ActiveBorder).ToString (), "TS#2");
			Assert.AreEqual ("Color [A=1, R=2, G=3, B=4]", Color.FromArgb(1, 2, 3, 4).ToString (), "TS#3");
			Assert.AreEqual ("Color [Empty]", Color.Empty.ToString (), "TS#4");
		}

		[Test]
		public void Equality ()
		{
			Color c = Color.AliceBlue;
			Assert.IsTrue (c == Color.FromName ("AliceBlue"), "EQ#1");
			Assert.AreEqual (false, c == Color.FromArgb (c.A, c.R, c.G, c.B), "EQ#2");
			Assert.IsTrue (c.Equals (Color.FromName ("AliceBlue")), "EQ#3");
			Assert.AreEqual (false, c.Equals(Color.FromArgb (c.A, c.R, c.G, c.B)), "EQ#4");
			Assert.AreEqual (false, c != Color.FromName ("AliceBlue"), "EQ#5");
			Assert.IsTrue(c != Color.FromArgb (c.A, c.R, c.G, c.B), "EQ#6");
		}

		[Test]
		public void FromArgb_InvalidAlpha1 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (-1, Color.Red));
		}

		[Test]
		public void FromArgb_InvalidAlpha2 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (256, Color.Red));
		}

		[Test]
		public void FromArgb_InvalidAlpha3 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (-1, 0, 0, 0));
		}

		[Test]
		public void FromArgb_InvalidAlpha4 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (256, 0, 0, 0));
		}

		[Test]
		public void FromArgb_InvalidRed1 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (-1, 0, 0));
		}

		[Test]
		public void FromArgb_InvalidRed2 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (256, 0, 0));
		}

		[Test]
		public void FromArgb_InvalidRed3 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, -1, 0, 0));
		}

		[Test]
		public void FromArgb_InvalidRed4 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, 256, 0, 0));
		}

		[Test]
		public void FromArgb_InvalidGreen1 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, -1, 0));
		}

		[Test]
		public void FromArgb_InvalidGreen2 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, 256, 0));
		}

		[Test]
		public void FromArgb_InvalidGreen3 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, 0, -1, 0));
		}

		[Test]
		public void FromArgb_InvalidGreen4 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, 0, 256, 0));
		}

		[Test]
		public void FromArgb_InvalidBlue1 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, 0, -1));
		}

		[Test]
		public void FromArgb_InvalidBlue2 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, 0, 256));
		}

		[Test]
		public void FromArgb_InvalidBlue3 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, 0, 0, -1));
		}

		[Test]
		public void FromArgb_InvalidBlue4 ()
		{
			Assert.Throws<ArgumentException> (() => Color.FromArgb (0, 0, 0, 256));
		}

		[Test]
		public void FromName_Invalid () {
			Color c = Color.FromName ("OingoBoingo");
			Assert.IsTrue (c.IsNamedColor, "#1");
			Assert.AreEqual (c.ToArgb (), 0, "#2");
			Assert.AreEqual (c.Name, "OingoBoingo", "#3");
		}

		private void CheckRed (string message, Color color)
		{
			Assert.AreEqual (255, color.A, "A");
			Assert.AreEqual (255, color.R, "R");
			Assert.AreEqual (0, color.G, "G");
			Assert.AreEqual (0, color.B, "B");
			Assert.AreEqual ("Red", color.Name, "Name");
			Assert.IsTrue (!color.IsEmpty, "IsEmpty");
			Assert.IsTrue (color.IsKnownColor, "IsKnownColor");
			Assert.IsTrue (color.IsNamedColor, "IsNamedColor");
			Assert.IsTrue (!color.IsSystemColor, "IsSystemColor");
		}

		[Test]
		public void SerializationRoundtrip ()
		{
			Color c = Color.Red;
			CheckRed ("original", c);

			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, c);

			ms.Position = 0;
			Color color = (Color) bf.Deserialize (ms);
			CheckRed ("deserialized", color);
		}

		// serialized under MS 2.0, can be deserialized on both MS 1.1 SP1 and 2.0
		static byte[] color_red = { 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 
			0x00, 0x00, 0x00, 0x0C, 0x02, 0x00, 0x00, 0x00, 0x51, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x44, 
			0x72, 0x61, 0x77, 0x69, 0x6E, 0x67, 0x2C, 0x20, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x32, 
			0x2E, 0x30, 0x2E, 0x30, 0x2E, 0x30, 0x2C, 0x20, 0x43, 0x75, 0x6C, 0x74, 0x75, 0x72, 0x65, 0x3D, 0x6E, 
			0x65, 0x75, 0x74, 0x72, 0x61, 0x6C, 0x2C, 0x20, 0x50, 0x75, 0x62, 0x6C, 0x69, 0x63, 0x4B, 0x65, 0x79, 
			0x54, 0x6F, 0x6B, 0x65, 0x6E, 0x3D, 0x62, 0x30, 0x33, 0x66, 0x35, 0x66, 0x37, 0x66, 0x31, 0x31, 0x64, 
			0x35, 0x30, 0x61, 0x33, 0x61, 0x05, 0x01, 0x00, 0x00, 0x00, 0x14, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 
			0x2E, 0x44, 0x72, 0x61, 0x77, 0x69, 0x6E, 0x67, 0x2E, 0x43, 0x6F, 0x6C, 0x6F, 0x72, 0x04, 0x00, 0x00, 
			0x00, 0x04, 0x6E, 0x61, 0x6D, 0x65, 0x05, 0x76, 0x61, 0x6C, 0x75, 0x65, 0x0A, 0x6B, 0x6E, 0x6F, 0x77, 
			0x6E, 0x43, 0x6F, 0x6C, 0x6F, 0x72, 0x05, 0x73, 0x74, 0x61, 0x74, 0x65, 0x01, 0x00, 0x00, 0x00, 0x09, 
			0x07, 0x07, 0x02, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x8D, 0x00, 
			0x01, 0x00, 0x0B };

		[Test]
		public void Deserialize ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream (color_red);
			Color color = (Color) bf.Deserialize (ms);
			CheckRed ("deserialized", color);
		}

		static byte [] color_blue = {
			0,1,0,0,0,255,255,255,255,1,0,0,0,0,0,0,0,12,2,0,0,0,81,83,121,115,116,101,109,46,68,114,97,119,105,
			110,103,44,32,86,101,114,115,105,111,110,61,50,46,48,46,48,46,48,44,32,67,117,108,116,117,114,101,
			61,110,101,117,116,114,97,108,44,32,80,117,98,108,105,99,75,101,121,84,111,107,101,110,61,98,48,51,
			102,53,102,55,102,49,49,100,53,48,97,51,97,5,1,0,0,0,20,83,121,115,116,101,109,46,68,114,97,119,105,
			110,103,46,67,111,108,111,114,4,0,0,0,4,110,97,109,101,5,118,97,108,117,101,10,107,110,111,119,110,
			67,111,108,111,114,5,115,116,97,116,101,1,0,0,0,9,7,7,2,0,0,0,10,0,0,0,0,0,0,0,0,37,0,1,0,11 };

		[Test]
		public void Deserialize2 ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream (color_blue);
			Color color = (Color) bf.Deserialize (ms);
			Assert.AreEqual (255, color.A, "A");
			Assert.AreEqual (0, color.R, "R");
			Assert.AreEqual (0, color.G, "G");
			Assert.AreEqual (255, color.B, "B");
			Assert.AreEqual ("Blue", color.Name, "Name");
			Assert.IsTrue (!color.IsEmpty, "IsEmpty");
			Assert.IsTrue (color.IsKnownColor, "IsKnownColor");
			Assert.IsTrue (color.IsNamedColor, "IsNamedColor");
			Assert.IsTrue (!color.IsSystemColor, "IsSystemColor");
		}

		static byte [] color_blue_fromargb = {
			0,1,0,0,0,255,255,255,255,1,0,0,0,0,0,0,0,12,2,0,0,0,81,83,121,115,116,101,109,46,68,114,97,119,105,
			110,103,44,32,86,101,114,115,105,111,110,61,50,46,48,46,48,46,48,44,32,67,117,108,116,117,114,101,
			61,110,101,117,116,114,97,108,44,32,80,117,98,108,105,99,75,101,121,84,111,107,101,110,61,98,48,51,
			102,53,102,55,102,49,49,100,53,48,97,51,97,5,1,0,0,0,20,83,121,115,116,101,109,46,68,114,97,119,105,
			110,103,46,67,111,108,111,114,4,0,0,0,4,110,97,109,101,5,118,97,108,117,101,10,107,110,111,119,110,
			67,111,108,111,114,5,115,116,97,116,101,1,0,0,0,9,7,7,2,0,0,0,10,255,0,0,255,0,0,0,0,0,0,2,0,11};

		[Test]
		public void Deserialize3 ()
		{
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream (color_blue_fromargb);
			Color color = (Color) bf.Deserialize (ms);
			Assert.AreEqual (255, color.A, "A");
			Assert.AreEqual (0, color.R, "R");
			Assert.AreEqual (0, color.G, "G");
			Assert.AreEqual (255, color.B, "B");
			Assert.AreEqual ("ff0000ff", color.Name, "Name");
			Assert.IsTrue (!color.IsEmpty, "IsEmpty");
			Assert.IsTrue (!color.IsKnownColor, "IsKnownColor");
			Assert.IsTrue (!color.IsNamedColor, "IsNamedColor");
			Assert.IsTrue (!color.IsSystemColor, "IsSystemColor");
		}

		static byte [] _serializedV11 = {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x02, 0x00,
			0x00, 0x00, 0x54, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e,
			0x44, 0x72, 0x61, 0x77, 0x69, 0x6e, 0x67, 0x2c, 0x20, 0x56,
			0x65, 0x72, 0x73, 0x69, 0x6f, 0x6e, 0x3d, 0x31, 0x2e, 0x30,
			0x2e, 0x35, 0x30, 0x30, 0x30, 0x2e, 0x30, 0x2c, 0x20, 0x43,
			0x75, 0x6c, 0x74, 0x75, 0x72, 0x65, 0x3d, 0x6e, 0x65, 0x75,
			0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50, 0x75, 0x62, 0x6c,
			0x69, 0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b, 0x65, 0x6e,
			0x3d, 0x62, 0x30, 0x33, 0x66, 0x35, 0x66, 0x37, 0x66, 0x31,
			0x31, 0x64, 0x35, 0x30, 0x61, 0x33, 0x61, 0x05, 0x01, 0x00,
			0x00, 0x00, 0x14, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e,
			0x44, 0x72, 0x61, 0x77, 0x69, 0x6e, 0x67, 0x2e, 0x43, 0x6f,
			0x6c, 0x6f, 0x72, 0x04, 0x00, 0x00, 0x00, 0x05, 0x76, 0x61,
			0x6c, 0x75, 0x65, 0x0a, 0x6b, 0x6e, 0x6f, 0x77, 0x6e, 0x43,
			0x6f, 0x6c, 0x6f, 0x72, 0x05, 0x73, 0x74, 0x61, 0x74, 0x65,
			0x04, 0x6e, 0x61, 0x6d, 0x65, 0x00, 0x00, 0x00, 0x01, 0x09,
			0x07, 0x07, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x4f, 0x00, 0x01, 0x00, 0x0a, 0x0b
		};

		[Test]
		public void Deserialize4 ()
		{
			Color c;
			using (MemoryStream ms = new MemoryStream ()) {
				BinaryFormatter formatter = new BinaryFormatter ();

				ms.Write (_serializedV11, 0, _serializedV11.Length);
				ms.Position = 0;

				c = (Color) formatter.Deserialize (ms);
				Assert.AreEqual (Color.Green, c, "#1");
			}
		}

#if !MOBILE
		private void Compare (KnownColor kc, GetSysColorIndex index)
		{
			// we get BGR than needs to be converted into ARGB
			Color sc = ColorTranslator.FromWin32 ((int)GDIPlus.Win32GetSysColor (index));
			Assert.AreEqual (Color.FromKnownColor (kc).ToArgb (), sc.ToArgb (), kc.ToString ());
		}

		[Test]
		public void WindowsSystemColors ()
		{
			if (!GDIPlus.RunningOnWindows ())
				return;
			// ensure we can read *correctly* the Windows desktop colors
			Compare (KnownColor.ActiveBorder, GetSysColorIndex.COLOR_ACTIVEBORDER);
			Compare (KnownColor.ActiveCaption, GetSysColorIndex.COLOR_ACTIVECAPTION);
			Compare (KnownColor.ActiveCaptionText, GetSysColorIndex.COLOR_CAPTIONTEXT);
			Compare (KnownColor.AppWorkspace, GetSysColorIndex.COLOR_APPWORKSPACE);
			Compare (KnownColor.Control, GetSysColorIndex.COLOR_BTNFACE);
			Compare (KnownColor.ControlDark, GetSysColorIndex.COLOR_BTNSHADOW);
			Compare (KnownColor.ControlDarkDark, GetSysColorIndex.COLOR_3DDKSHADOW);
			Compare (KnownColor.ControlLight, GetSysColorIndex.COLOR_3DLIGHT);
			Compare (KnownColor.ControlLightLight, GetSysColorIndex.COLOR_BTNHIGHLIGHT);
			Compare (KnownColor.ControlText, GetSysColorIndex.COLOR_BTNTEXT);
			Compare (KnownColor.Desktop, GetSysColorIndex.COLOR_DESKTOP);
			Compare (KnownColor.GrayText, GetSysColorIndex.COLOR_GRAYTEXT);
			Compare (KnownColor.Highlight, GetSysColorIndex.COLOR_HIGHLIGHT);
			Compare (KnownColor.HighlightText, GetSysColorIndex.COLOR_HIGHLIGHTTEXT);
			Compare (KnownColor.HotTrack, GetSysColorIndex.COLOR_HOTLIGHT);
			Compare (KnownColor.InactiveBorder, GetSysColorIndex.COLOR_INACTIVEBORDER);
			Compare (KnownColor.InactiveCaption, GetSysColorIndex.COLOR_INACTIVECAPTION);
			Compare (KnownColor.InactiveCaptionText, GetSysColorIndex.COLOR_INACTIVECAPTIONTEXT);
			Compare (KnownColor.Info, GetSysColorIndex.COLOR_INFOBK);
			Compare (KnownColor.InfoText, GetSysColorIndex.COLOR_INFOTEXT);
			Compare (KnownColor.Menu, GetSysColorIndex.COLOR_MENU);
			Compare (KnownColor.MenuText, GetSysColorIndex.COLOR_MENUTEXT);
			Compare (KnownColor.ScrollBar, GetSysColorIndex.COLOR_SCROLLBAR);
			Compare (KnownColor.Window, GetSysColorIndex.COLOR_WINDOW);
			Compare (KnownColor.WindowFrame, GetSysColorIndex.COLOR_WINDOWFRAME);
			Compare (KnownColor.WindowText, GetSysColorIndex.COLOR_WINDOWTEXT);
			Compare (KnownColor.ButtonFace, GetSysColorIndex.COLOR_BTNFACE);
			Compare (KnownColor.ButtonHighlight, GetSysColorIndex.COLOR_BTNHIGHLIGHT);
			Compare (KnownColor.ButtonShadow, GetSysColorIndex.COLOR_BTNSHADOW);
			Compare (KnownColor.GradientActiveCaption, GetSysColorIndex.COLOR_GRADIENTACTIVECAPTION);
			Compare (KnownColor.GradientInactiveCaption, GetSysColorIndex.COLOR_GRADIENTINACTIVECAPTION);
			Compare (KnownColor.MenuBar, GetSysColorIndex.COLOR_MENUBAR);
			Compare (KnownColor.MenuHighlight, GetSysColorIndex.COLOR_MENUHIGHLIGHT);
		}
#endif
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
