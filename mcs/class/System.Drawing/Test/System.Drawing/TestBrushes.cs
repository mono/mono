// Tests for System.Drawing.Brushes.cs
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
using System.Security.Permissions;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class BrushesTest : Assertion
	{
		[SetUp]
		public void SetUp () { }

		[TearDown]
		public void TearDown () { }
		
		[Test]
		public void TestEquals ()
		{
			Brush brush1 = Brushes.Blue;
			Brush brush2 = Brushes.Blue;
			
			AssertEquals ("Equals", true, brush1.Equals (brush2));			
		}

		[Test]
		public void TestProperties ()
		{
			Brush br;
			SolidBrush solid;

			br = Brushes.AliceBlue;
			Assert ("P1#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P1#2", solid.Color, Color.AliceBlue);
			solid.Color = Color.Red;
			AssertEquals ("P1#3", solid.Color, Color.Red);

			br = Brushes.AntiqueWhite;
			Assert ("P2#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P2#2", solid.Color, Color.AntiqueWhite);
			solid.Color = Color.Red;
			AssertEquals ("P2#3", solid.Color, Color.Red);

			br = Brushes.Aqua;
			Assert ("P3#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P3#2", solid.Color, Color.Aqua);
			solid.Color = Color.Red;
			AssertEquals ("P3#3", solid.Color, Color.Red);

			br = Brushes.Aquamarine;
			Assert ("P4#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P4#2", solid.Color, Color.Aquamarine);
			solid.Color = Color.Red;
			AssertEquals ("P4#3", solid.Color, Color.Red);

			br = Brushes.Azure;
			Assert ("P5#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P5#2", solid.Color, Color.Azure);
			solid.Color = Color.Red;
			AssertEquals ("P5#3", solid.Color, Color.Red);

			br = Brushes.Beige;
			Assert ("P6#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P6#2", solid.Color, Color.Beige);
			solid.Color = Color.Red;
			AssertEquals ("P6#3", solid.Color, Color.Red);

			br = Brushes.Bisque;
			Assert ("P7#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P7#2", solid.Color, Color.Bisque);
			solid.Color = Color.Red;
			AssertEquals ("P7#3", solid.Color, Color.Red);

			br = Brushes.Black;
			Assert ("P8#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P8#2", solid.Color, Color.Black);
			solid.Color = Color.Red;
			AssertEquals ("P8#3", solid.Color, Color.Red);

			br = Brushes.BlanchedAlmond;
			Assert ("P9#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P9#2", solid.Color, Color.BlanchedAlmond);
			solid.Color = Color.Red;
			AssertEquals ("P9#3", solid.Color, Color.Red);

			br = Brushes.Blue;
			Assert ("P10#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P10#2", solid.Color, Color.Blue);
			solid.Color = Color.Red;
			AssertEquals ("P10#3", solid.Color, Color.Red);

			br = Brushes.BlueViolet;
			Assert ("P11#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P11#2", solid.Color, Color.BlueViolet);
			solid.Color = Color.Red;
			AssertEquals ("P11#3", solid.Color, Color.Red);

			br = Brushes.Brown;
			Assert ("P12#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P12#2", solid.Color, Color.Brown);
			solid.Color = Color.Red;
			AssertEquals ("P12#3", solid.Color, Color.Red);

			br = Brushes.BurlyWood;
			Assert ("P13#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P13#2", solid.Color, Color.BurlyWood);
			solid.Color = Color.Red;
			AssertEquals ("P13#3", solid.Color, Color.Red);

			br = Brushes.CadetBlue;
			Assert ("P14#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P14#2", solid.Color, Color.CadetBlue);
			solid.Color = Color.Red;
			AssertEquals ("P14#3", solid.Color, Color.Red);

			br = Brushes.Chartreuse;
			Assert ("P15#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P15#2", solid.Color, Color.Chartreuse);
			solid.Color = Color.Red;
			AssertEquals ("P15#3", solid.Color, Color.Red);

			br = Brushes.Chocolate;
			Assert ("P16#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P16#2", solid.Color, Color.Chocolate);
			solid.Color = Color.Red;
			AssertEquals ("P16#3", solid.Color, Color.Red);

			br = Brushes.Coral;
			Assert ("P17#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P17#2", solid.Color, Color.Coral);
			solid.Color = Color.Red;
			AssertEquals ("P17#3", solid.Color, Color.Red);

			br = Brushes.CornflowerBlue;
			Assert ("P18#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P18#2", solid.Color, Color.CornflowerBlue);
			solid.Color = Color.Red;
			AssertEquals ("P18#3", solid.Color, Color.Red);

			br = Brushes.Cornsilk;
			Assert ("P19#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P19#2", solid.Color, Color.Cornsilk);
			solid.Color = Color.Red;
			AssertEquals ("P19#3", solid.Color, Color.Red);

			br = Brushes.Crimson;
			Assert ("P20#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P20#2", solid.Color, Color.Crimson);
			solid.Color = Color.Red;
			AssertEquals ("P20#3", solid.Color, Color.Red);

			br = Brushes.Cyan;
			Assert ("P21#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P21#2", solid.Color, Color.Cyan);
			solid.Color = Color.Red;
			AssertEquals ("P21#3", solid.Color, Color.Red);

			br = Brushes.DarkBlue;
			Assert ("P22#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P22#2", solid.Color, Color.DarkBlue);
			solid.Color = Color.Red;
			AssertEquals ("P22#3", solid.Color, Color.Red);

			br = Brushes.DarkCyan;
			Assert ("P23#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P23#2", solid.Color, Color.DarkCyan);
			solid.Color = Color.Red;
			AssertEquals ("P23#3", solid.Color, Color.Red);

			br = Brushes.DarkGoldenrod;
			Assert ("P24#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P24#2", solid.Color, Color.DarkGoldenrod);
			solid.Color = Color.Red;
			AssertEquals ("P24#3", solid.Color, Color.Red);

			br = Brushes.DarkGray;
			Assert ("P25#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P25#2", solid.Color, Color.DarkGray);
			solid.Color = Color.Red;
			AssertEquals ("P25#3", solid.Color, Color.Red);

			br = Brushes.DarkGreen;
			Assert ("P26#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P26#2", solid.Color, Color.DarkGreen);
			solid.Color = Color.Red;
			AssertEquals ("P26#3", solid.Color, Color.Red);

			br = Brushes.DarkKhaki;
			Assert ("P27#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P27#2", solid.Color, Color.DarkKhaki);
			solid.Color = Color.Red;
			AssertEquals ("P27#3", solid.Color, Color.Red);

			br = Brushes.DarkMagenta;
			Assert ("P28#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P28#2", solid.Color, Color.DarkMagenta);
			solid.Color = Color.Red;
			AssertEquals ("P28#3", solid.Color, Color.Red);

			br = Brushes.DarkOliveGreen;
			Assert ("P29#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P29#2", solid.Color, Color.DarkOliveGreen);
			solid.Color = Color.Red;
			AssertEquals ("P29#3", solid.Color, Color.Red);

			br = Brushes.DarkOrange;
			Assert ("P30#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P30#2", solid.Color, Color.DarkOrange);
			solid.Color = Color.Red;
			AssertEquals ("P30#3", solid.Color, Color.Red);

			br = Brushes.DarkOrchid;
			Assert ("P31#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P31#2", solid.Color, Color.DarkOrchid);
			solid.Color = Color.Red;
			AssertEquals ("P31#3", solid.Color, Color.Red);

			br = Brushes.DarkRed;
			Assert ("P32#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P32#2", solid.Color, Color.DarkRed);
			solid.Color = Color.Red;
			AssertEquals ("P32#3", solid.Color, Color.Red);

			br = Brushes.DarkSalmon;
			Assert ("P33#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P33#2", solid.Color, Color.DarkSalmon);
			solid.Color = Color.Red;
			AssertEquals ("P33#3", solid.Color, Color.Red);

			br = Brushes.DarkSeaGreen;
			Assert ("P34#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P34#2", solid.Color, Color.DarkSeaGreen);
			solid.Color = Color.Red;
			AssertEquals ("P34#3", solid.Color, Color.Red);

			br = Brushes.DarkSlateBlue;
			Assert ("P35#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P35#2", solid.Color, Color.DarkSlateBlue);
			solid.Color = Color.Red;
			AssertEquals ("P35#3", solid.Color, Color.Red);

			br = Brushes.DarkSlateGray;
			Assert ("P36#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P36#2", solid.Color, Color.DarkSlateGray);
			solid.Color = Color.Red;
			AssertEquals ("P36#3", solid.Color, Color.Red);

			br = Brushes.DarkTurquoise;
			Assert ("P37#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P37#2", solid.Color, Color.DarkTurquoise);
			solid.Color = Color.Red;
			AssertEquals ("P37#3", solid.Color, Color.Red);

			br = Brushes.DarkViolet;
			Assert ("P38#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P38#2", solid.Color, Color.DarkViolet);
			solid.Color = Color.Red;
			AssertEquals ("P38#3", solid.Color, Color.Red);

			br = Brushes.DeepPink;
			Assert ("P39#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P39#2", solid.Color, Color.DeepPink);
			solid.Color = Color.Red;
			AssertEquals ("P39#3", solid.Color, Color.Red);

			br = Brushes.DeepSkyBlue;
			Assert ("P40#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P40#2", solid.Color, Color.DeepSkyBlue);
			solid.Color = Color.Red;
			AssertEquals ("P40#3", solid.Color, Color.Red);

			br = Brushes.DimGray;
			Assert ("P41#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P41#2", solid.Color, Color.DimGray);
			solid.Color = Color.Red;
			AssertEquals ("P41#3", solid.Color, Color.Red);

			br = Brushes.DodgerBlue;
			Assert ("P42#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P42#2", solid.Color, Color.DodgerBlue);
			solid.Color = Color.Red;
			AssertEquals ("P42#3", solid.Color, Color.Red);

			br = Brushes.Firebrick;
			Assert ("P43#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P43#2", solid.Color, Color.Firebrick);
			solid.Color = Color.Red;
			AssertEquals ("P43#3", solid.Color, Color.Red);

			br = Brushes.FloralWhite;
			Assert ("P44#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P44#2", solid.Color, Color.FloralWhite);
			solid.Color = Color.Red;
			AssertEquals ("P44#3", solid.Color, Color.Red);

			br = Brushes.ForestGreen;
			Assert ("P45#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P45#2", solid.Color, Color.ForestGreen);
			solid.Color = Color.Red;
			AssertEquals ("P45#3", solid.Color, Color.Red);

			br = Brushes.Fuchsia;
			Assert ("P46#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P46#2", solid.Color, Color.Fuchsia);
			solid.Color = Color.Red;
			AssertEquals ("P46#3", solid.Color, Color.Red);

			br = Brushes.Gainsboro;
			Assert ("P47#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P47#2", solid.Color, Color.Gainsboro);
			solid.Color = Color.Red;
			AssertEquals ("P47#3", solid.Color, Color.Red);

			br = Brushes.GhostWhite;
			Assert ("P48#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P48#2", solid.Color, Color.GhostWhite);
			solid.Color = Color.Red;
			AssertEquals ("P48#3", solid.Color, Color.Red);

			br = Brushes.Gold;
			Assert ("P49#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P49#2", solid.Color, Color.Gold);
			solid.Color = Color.Red;
			AssertEquals ("P49#3", solid.Color, Color.Red);

			br = Brushes.Goldenrod;
			Assert ("P50#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P50#2", solid.Color, Color.Goldenrod);
			solid.Color = Color.Red;
			AssertEquals ("P50#3", solid.Color, Color.Red);

			br = Brushes.Gray;
			Assert ("P51#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P51#2", solid.Color, Color.Gray);
			solid.Color = Color.Red;
			AssertEquals ("P51#3", solid.Color, Color.Red);

			br = Brushes.Green;
			Assert ("P52#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P52#2", solid.Color, Color.Green);
			solid.Color = Color.Red;
			AssertEquals ("P52#3", solid.Color, Color.Red);

			br = Brushes.GreenYellow;
			Assert ("P53#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P53#2", solid.Color, Color.GreenYellow);
			solid.Color = Color.Red;
			AssertEquals ("P53#3", solid.Color, Color.Red);

			br = Brushes.Honeydew;
			Assert ("P54#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P54#2", solid.Color, Color.Honeydew);
			solid.Color = Color.Red;
			AssertEquals ("P54#3", solid.Color, Color.Red);

			br = Brushes.HotPink;
			Assert ("P55#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P55#2", solid.Color, Color.HotPink);
			solid.Color = Color.Red;
			AssertEquals ("P55#3", solid.Color, Color.Red);

			br = Brushes.IndianRed;
			Assert ("P56#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P56#2", solid.Color, Color.IndianRed);
			solid.Color = Color.Red;
			AssertEquals ("P56#3", solid.Color, Color.Red);

			br = Brushes.Indigo;
			Assert ("P57#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P57#2", solid.Color, Color.Indigo);
			solid.Color = Color.Red;
			AssertEquals ("P57#3", solid.Color, Color.Red);

			br = Brushes.Ivory;
			Assert ("P58#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P58#2", solid.Color, Color.Ivory);
			solid.Color = Color.Red;
			AssertEquals ("P58#3", solid.Color, Color.Red);

			br = Brushes.Khaki;
			Assert ("P59#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P59#2", solid.Color, Color.Khaki);
			solid.Color = Color.Red;
			AssertEquals ("P59#3", solid.Color, Color.Red);

			br = Brushes.Lavender;
			Assert ("P60#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P60#2", solid.Color, Color.Lavender);
			solid.Color = Color.Red;
			AssertEquals ("P60#3", solid.Color, Color.Red);

			br = Brushes.LavenderBlush;
			Assert ("P61#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P61#2", solid.Color, Color.LavenderBlush);
			solid.Color = Color.Red;
			AssertEquals ("P61#3", solid.Color, Color.Red);

			br = Brushes.LawnGreen;
			Assert ("P62#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P62#2", solid.Color, Color.LawnGreen);
			solid.Color = Color.Red;
			AssertEquals ("P62#3", solid.Color, Color.Red);

			br = Brushes.LemonChiffon;
			Assert ("P63#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P63#2", solid.Color, Color.LemonChiffon);
			solid.Color = Color.Red;
			AssertEquals ("P63#3", solid.Color, Color.Red);

			br = Brushes.LightBlue;
			Assert ("P64#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P64#2", solid.Color, Color.LightBlue);
			solid.Color = Color.Red;
			AssertEquals ("P64#3", solid.Color, Color.Red);

			br = Brushes.LightCoral;
			Assert ("P65#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P65#2", solid.Color, Color.LightCoral);
			solid.Color = Color.Red;
			AssertEquals ("P65#3", solid.Color, Color.Red);

			br = Brushes.LightCyan;
			Assert ("P66#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P66#2", solid.Color, Color.LightCyan);
			solid.Color = Color.Red;
			AssertEquals ("P66#3", solid.Color, Color.Red);

			br = Brushes.LightGoldenrodYellow;
			Assert ("P67#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P67#2", solid.Color, Color.LightGoldenrodYellow);
			solid.Color = Color.Red;
			AssertEquals ("P67#3", solid.Color, Color.Red);

			br = Brushes.LightGray;
			Assert ("P68#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P68#2", solid.Color, Color.LightGray);
			solid.Color = Color.Red;
			AssertEquals ("P68#3", solid.Color, Color.Red);

			br = Brushes.LightGreen;
			Assert ("P69#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P69#2", solid.Color, Color.LightGreen);
			solid.Color = Color.Red;
			AssertEquals ("P69#3", solid.Color, Color.Red);

			br = Brushes.LightPink;
			Assert ("P70#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P70#2", solid.Color, Color.LightPink);
			solid.Color = Color.Red;
			AssertEquals ("P70#3", solid.Color, Color.Red);

			br = Brushes.LightSalmon;
			Assert ("P71#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P71#2", solid.Color, Color.LightSalmon);
			solid.Color = Color.Red;
			AssertEquals ("P71#3", solid.Color, Color.Red);

			br = Brushes.LightSeaGreen;
			Assert ("P72#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P72#2", solid.Color, Color.LightSeaGreen);
			solid.Color = Color.Red;
			AssertEquals ("P72#3", solid.Color, Color.Red);

			br = Brushes.LightSkyBlue;
			Assert ("P73#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P73#2", solid.Color, Color.LightSkyBlue);
			solid.Color = Color.Red;
			AssertEquals ("P73#3", solid.Color, Color.Red);

			br = Brushes.LightSlateGray;
			Assert ("P74#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P74#2", solid.Color, Color.LightSlateGray);
			solid.Color = Color.Red;
			AssertEquals ("P74#3", solid.Color, Color.Red);

			br = Brushes.LightSteelBlue;
			Assert ("P75#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P75#2", solid.Color, Color.LightSteelBlue);
			solid.Color = Color.Red;
			AssertEquals ("P75#3", solid.Color, Color.Red);

			br = Brushes.LightYellow;
			Assert ("P76#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P76#2", solid.Color, Color.LightYellow);
			solid.Color = Color.Red;
			AssertEquals ("P76#3", solid.Color, Color.Red);

			br = Brushes.Lime;
			Assert ("P77#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P77#2", solid.Color, Color.Lime);
			solid.Color = Color.Red;
			AssertEquals ("P77#3", solid.Color, Color.Red);

			br = Brushes.LimeGreen;
			Assert ("P78#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P78#2", solid.Color, Color.LimeGreen);
			solid.Color = Color.Red;
			AssertEquals ("P78#3", solid.Color, Color.Red);

			br = Brushes.Linen;
			Assert ("P79#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P79#2", solid.Color, Color.Linen);
			solid.Color = Color.Red;
			AssertEquals ("P79#3", solid.Color, Color.Red);

			br = Brushes.Magenta;
			Assert ("P80#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P80#2", solid.Color, Color.Magenta);
			solid.Color = Color.Red;
			AssertEquals ("P80#3", solid.Color, Color.Red);

			br = Brushes.Maroon;
			Assert ("P81#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P81#2", solid.Color, Color.Maroon);
			solid.Color = Color.Red;
			AssertEquals ("P81#3", solid.Color, Color.Red);

			br = Brushes.MediumAquamarine;
			Assert ("P82#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P82#2", solid.Color, Color.MediumAquamarine);
			solid.Color = Color.Red;
			AssertEquals ("P82#3", solid.Color, Color.Red);

			br = Brushes.MediumBlue;
			Assert ("P83#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P83#2", solid.Color, Color.MediumBlue);
			solid.Color = Color.Red;
			AssertEquals ("P83#3", solid.Color, Color.Red);

			br = Brushes.MediumOrchid;
			Assert ("P84#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P84#2", solid.Color, Color.MediumOrchid);
			solid.Color = Color.Red;
			AssertEquals ("P84#3", solid.Color, Color.Red);

			br = Brushes.MediumPurple;
			Assert ("P85#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P85#2", solid.Color, Color.MediumPurple);
			solid.Color = Color.Red;
			AssertEquals ("P85#3", solid.Color, Color.Red);

			br = Brushes.MediumSeaGreen;
			Assert ("P86#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P86#2", solid.Color, Color.MediumSeaGreen);
			solid.Color = Color.Red;
			AssertEquals ("P86#3", solid.Color, Color.Red);

			br = Brushes.MediumSlateBlue;
			Assert ("P87#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P87#2", solid.Color, Color.MediumSlateBlue);
			solid.Color = Color.Red;
			AssertEquals ("P87#3", solid.Color, Color.Red);

			br = Brushes.MediumSpringGreen;
			Assert ("P88#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P88#2", solid.Color, Color.MediumSpringGreen);
			solid.Color = Color.Red;
			AssertEquals ("P88#3", solid.Color, Color.Red);

			br = Brushes.MediumTurquoise;
			Assert ("P89#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P89#2", solid.Color, Color.MediumTurquoise);
			solid.Color = Color.Red;
			AssertEquals ("P89#3", solid.Color, Color.Red);

			br = Brushes.MediumVioletRed;
			Assert ("P90#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P90#2", solid.Color, Color.MediumVioletRed);
			solid.Color = Color.Red;
			AssertEquals ("P90#3", solid.Color, Color.Red);

			br = Brushes.MidnightBlue;
			Assert ("P91#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P91#2", solid.Color, Color.MidnightBlue);
			solid.Color = Color.Red;
			AssertEquals ("P91#3", solid.Color, Color.Red);

			br = Brushes.MintCream;
			Assert ("P92#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P92#2", solid.Color, Color.MintCream);
			solid.Color = Color.Red;
			AssertEquals ("P92#3", solid.Color, Color.Red);

			br = Brushes.MistyRose;
			Assert ("P93#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P93#2", solid.Color, Color.MistyRose);
			solid.Color = Color.Red;
			AssertEquals ("P93#3", solid.Color, Color.Red);

			br = Brushes.Moccasin;
			Assert ("P94#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P94#2", solid.Color, Color.Moccasin);
			solid.Color = Color.Red;
			AssertEquals ("P94#3", solid.Color, Color.Red);

			br = Brushes.NavajoWhite;
			Assert ("P95#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P95#2", solid.Color, Color.NavajoWhite);
			solid.Color = Color.Red;
			AssertEquals ("P95#3", solid.Color, Color.Red);

			br = Brushes.Navy;
			Assert ("P96#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P96#2", solid.Color, Color.Navy);
			solid.Color = Color.Red;
			AssertEquals ("P96#3", solid.Color, Color.Red);

			br = Brushes.OldLace;
			Assert ("P97#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P97#2", solid.Color, Color.OldLace);
			solid.Color = Color.Red;
			AssertEquals ("P97#3", solid.Color, Color.Red);

			br = Brushes.Olive;
			Assert ("P98#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P98#2", solid.Color, Color.Olive);
			solid.Color = Color.Red;
			AssertEquals ("P98#3", solid.Color, Color.Red);

			br = Brushes.OliveDrab;
			Assert ("P99#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P99#2", solid.Color, Color.OliveDrab);
			solid.Color = Color.Red;
			AssertEquals ("P99#3", solid.Color, Color.Red);

			br = Brushes.Orange;
			Assert ("P100#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P100#2", solid.Color, Color.Orange);
			solid.Color = Color.Red;
			AssertEquals ("P100#3", solid.Color, Color.Red);

			br = Brushes.OrangeRed;
			Assert ("P101#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P101#2", solid.Color, Color.OrangeRed);
			solid.Color = Color.Red;
			AssertEquals ("P101#3", solid.Color, Color.Red);

			br = Brushes.Orchid;
			Assert ("P102#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P102#2", solid.Color, Color.Orchid);
			solid.Color = Color.Red;
			AssertEquals ("P102#3", solid.Color, Color.Red);

			br = Brushes.PaleGoldenrod;
			Assert ("P103#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P103#2", solid.Color, Color.PaleGoldenrod);
			solid.Color = Color.Red;
			AssertEquals ("P103#3", solid.Color, Color.Red);

			br = Brushes.PaleGreen;
			Assert ("P104#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P104#2", solid.Color, Color.PaleGreen);
			solid.Color = Color.Red;
			AssertEquals ("P104#3", solid.Color, Color.Red);

			br = Brushes.PaleTurquoise;
			Assert ("P105#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P105#2", solid.Color, Color.PaleTurquoise);
			solid.Color = Color.Red;
			AssertEquals ("P105#3", solid.Color, Color.Red);

			br = Brushes.PaleVioletRed;
			Assert ("P106#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P106#2", solid.Color, Color.PaleVioletRed);
			solid.Color = Color.Red;
			AssertEquals ("P106#3", solid.Color, Color.Red);

			br = Brushes.PapayaWhip;
			Assert ("P107#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P107#2", solid.Color, Color.PapayaWhip);
			solid.Color = Color.Red;
			AssertEquals ("P107#3", solid.Color, Color.Red);

			br = Brushes.PeachPuff;
			Assert ("P108#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P108#2", solid.Color, Color.PeachPuff);
			solid.Color = Color.Red;
			AssertEquals ("P108#3", solid.Color, Color.Red);

			br = Brushes.Peru;
			Assert ("P109#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P109#2", solid.Color, Color.Peru);
			solid.Color = Color.Red;
			AssertEquals ("P109#3", solid.Color, Color.Red);

			br = Brushes.Pink;
			Assert ("P110#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P110#2", solid.Color, Color.Pink);
			solid.Color = Color.Red;
			AssertEquals ("P110#3", solid.Color, Color.Red);

			br = Brushes.Plum;
			Assert ("P111#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P111#2", solid.Color, Color.Plum);
			solid.Color = Color.Red;
			AssertEquals ("P111#3", solid.Color, Color.Red);

			br = Brushes.PowderBlue;
			Assert ("P112#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P112#2", solid.Color, Color.PowderBlue);
			solid.Color = Color.Red;
			AssertEquals ("P112#3", solid.Color, Color.Red);

			br = Brushes.Purple;
			Assert ("P113#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P113#2", solid.Color, Color.Purple);
			solid.Color = Color.Red;
			AssertEquals ("P113#3", solid.Color, Color.Red);

			br = Brushes.Red;
			Assert ("P114#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P114#2", solid.Color, Color.Red);
			solid.Color = Color.White;
			AssertEquals ("P114#3", solid.Color, Color.White);

			br = Brushes.RosyBrown;
			Assert ("P115#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P115#2", solid.Color, Color.RosyBrown);
			solid.Color = Color.Red;
			AssertEquals ("P115#3", solid.Color, Color.Red);

			br = Brushes.RoyalBlue;
			Assert ("P116#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P116#2", solid.Color, Color.RoyalBlue);
			solid.Color = Color.Red;
			AssertEquals ("P116#3", solid.Color, Color.Red);

			br = Brushes.SaddleBrown;
			Assert ("P117#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P117#2", solid.Color, Color.SaddleBrown);
			solid.Color = Color.Red;
			AssertEquals ("P117#3", solid.Color, Color.Red);

			br = Brushes.Salmon;
			Assert ("P118#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P118#2", solid.Color, Color.Salmon);
			solid.Color = Color.Red;
			AssertEquals ("P118#3", solid.Color, Color.Red);

			br = Brushes.SandyBrown;
			Assert ("P119#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P119#2", solid.Color, Color.SandyBrown);
			solid.Color = Color.Red;
			AssertEquals ("P119#3", solid.Color, Color.Red);

			br = Brushes.SeaGreen;
			Assert ("P120#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P120#2", solid.Color, Color.SeaGreen);
			solid.Color = Color.Red;
			AssertEquals ("P120#3", solid.Color, Color.Red);

			br = Brushes.SeaShell;
			Assert ("P121#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P121#2", solid.Color, Color.SeaShell);
			solid.Color = Color.Red;
			AssertEquals ("P121#3", solid.Color, Color.Red);

			br = Brushes.Sienna;
			Assert ("P122#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P122#2", solid.Color, Color.Sienna);
			solid.Color = Color.Red;
			AssertEquals ("P122#3", solid.Color, Color.Red);

			br = Brushes.Silver;
			Assert ("P123#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P123#2", solid.Color, Color.Silver);
			solid.Color = Color.Red;
			AssertEquals ("P123#3", solid.Color, Color.Red);

			br = Brushes.SkyBlue;
			Assert ("P124#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P124#2", solid.Color, Color.SkyBlue);
			solid.Color = Color.Red;
			AssertEquals ("P124#3", solid.Color, Color.Red);

			br = Brushes.SlateBlue;
			Assert ("P125#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P125#2", solid.Color, Color.SlateBlue);
			solid.Color = Color.Red;
			AssertEquals ("P125#3", solid.Color, Color.Red);

			br = Brushes.SlateGray;
			Assert ("P126#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P126#2", solid.Color, Color.SlateGray);
			solid.Color = Color.Red;
			AssertEquals ("P126#3", solid.Color, Color.Red);

			br = Brushes.Snow;
			Assert ("P127#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P127#2", solid.Color, Color.Snow);
			solid.Color = Color.Red;
			AssertEquals ("P127#3", solid.Color, Color.Red);

			br = Brushes.SpringGreen;
			Assert ("P128#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P128#2", solid.Color, Color.SpringGreen);
			solid.Color = Color.Red;
			AssertEquals ("P128#3", solid.Color, Color.Red);

			br = Brushes.SteelBlue;
			Assert ("P129#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P129#2", solid.Color, Color.SteelBlue);
			solid.Color = Color.Red;
			AssertEquals ("P129#3", solid.Color, Color.Red);

			br = Brushes.Tan;
			Assert ("P130#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P130#2", solid.Color, Color.Tan);
			solid.Color = Color.Red;
			AssertEquals ("P130#3", solid.Color, Color.Red);

			br = Brushes.Teal;
			Assert ("P131#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P131#2", solid.Color, Color.Teal);
			solid.Color = Color.Red;
			AssertEquals ("P131#3", solid.Color, Color.Red);

			br = Brushes.Thistle;
			Assert ("P132#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P132#2", solid.Color, Color.Thistle);
			solid.Color = Color.Red;
			AssertEquals ("P132#3", solid.Color, Color.Red);

			br = Brushes.Tomato;
			Assert ("P133#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P133#2", solid.Color, Color.Tomato);
			solid.Color = Color.Red;
			AssertEquals ("P133#3", solid.Color, Color.Red);

			br = Brushes.Transparent;
			Assert ("P134#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P134#2", solid.Color, Color.Transparent);
			solid.Color = Color.Red;
			AssertEquals ("P134#3", solid.Color, Color.Red);

			br = Brushes.Turquoise;
			Assert ("P135#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P135#2", solid.Color, Color.Turquoise);
			solid.Color = Color.Red;
			AssertEquals ("P135#3", solid.Color, Color.Red);

			br = Brushes.Violet;
			Assert ("P136#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P136#2", solid.Color, Color.Violet);
			solid.Color = Color.Red;
			AssertEquals ("P136#3", solid.Color, Color.Red);

			br = Brushes.Wheat;
			Assert ("P137#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P137#2", solid.Color, Color.Wheat);
			solid.Color = Color.Red;
			AssertEquals ("P137#3", solid.Color, Color.Red);

			br = Brushes.White;
			Assert ("P138#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P138#2", solid.Color, Color.White);
			solid.Color = Color.Red;
			AssertEquals ("P138#3", solid.Color, Color.Red);

			br = Brushes.WhiteSmoke;
			Assert ("P139#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P139#2", solid.Color, Color.WhiteSmoke);
			solid.Color = Color.Red;
			AssertEquals ("P139#3", solid.Color, Color.Red);

			br = Brushes.Yellow;
			Assert ("P140#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P140#2", solid.Color, Color.Yellow);
			solid.Color = Color.Red;
			AssertEquals ("P140#3", solid.Color, Color.Red);

			br = Brushes.YellowGreen;
			Assert ("P141#1", br is SolidBrush);
			solid = (SolidBrush) br;
			AssertEquals ("P141#2", solid.Color, Color.YellowGreen);
			solid.Color = Color.Red;
			AssertEquals ("P141#3", solid.Color, Color.Red);
		}
	}
}

// Following code was used to generate the TestProperties method.
//
//Type type = typeof (Brushes);
//PropertyInfo [] properties = type.GetProperties ();
//int count = 1;
//foreach (PropertyInfo property in properties) {
//	Console.WriteLine();
//	Console.WriteLine("\t\t\tbr = Brushes." + property.Name + ";");
//	Console.WriteLine("\t\t\tAssert (\"P" + count + "#1\", br is SolidBrush);");
//	Console.WriteLine("\t\t\tsolid = (SolidBrush) br;");
//	Console.WriteLine("\t\t\tAssertEquals (\"P" + count + "#2\", solid.Color, Color." + property.Name + ");");
//
//	if (property.Name != "Red") {
//	Console.WriteLine("\t\t\tsolid.Color = Color.Red;");
//	Console.WriteLine("\t\t\tAssertEquals (\"P" + count + "#3\", solid.Color, Color.Red);");
//	} else {
//	Console.WriteLine("\t\t\tsolid.Color = Color.White;");
//	Console.WriteLine("\t\t\tAssertEquals (\"P" + count + "#3\", solid.Color, Color.White);");
//	}
//
//	count++;
//}
