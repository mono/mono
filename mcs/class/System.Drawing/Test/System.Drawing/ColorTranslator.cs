//
// ColorTranslator class testing unit
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ColorTranslatorTest {

		[Test]
		public void FromHtml_Null ()
		{
			Assert.AreEqual (0, ColorTranslator.FromHtml (null).ToArgb ());
		}

		[Test]
		public void FromHtml_Empty ()
		{
			Assert.AreEqual (0, ColorTranslator.FromHtml (String.Empty).ToArgb ());
		}

		[Test]
		public void FromHtml_KnownValues ()
		{
			Assert.AreEqual (SystemColors.Control, ColorTranslator.FromHtml ("buttonface"), "buttonface");
			Assert.AreEqual (SystemColors.ActiveCaptionText, ColorTranslator.FromHtml ("CAPTIONTEXT"), "captiontext");
			Assert.AreEqual (SystemColors.ControlDarkDark, ColorTranslator.FromHtml ("threedDARKshadow"), "threeddarkshadow");
			Assert.AreEqual (SystemColors.Desktop, ColorTranslator.FromHtml ("background"), "background");
			Assert.AreEqual (SystemColors.ControlText, ColorTranslator.FromHtml ("ButtonText"), "buttontext");
			Assert.AreEqual (SystemColors.Info, ColorTranslator.FromHtml ("infobackground"), "infobackground");
		}

		[Test]
		public void FromHtml_Int ()
		{
			Assert.AreEqual (-1, ColorTranslator.FromHtml ("-1").ToArgb (), "-1");
			Assert.AreEqual (0, ColorTranslator.FromHtml ("0").ToArgb (), "0");
			Assert.AreEqual (1, ColorTranslator.FromHtml ("1").ToArgb (), "1");
		}

		[Test]
		public void FromHtml_PoundInt ()
		{
			Assert.AreEqual (0, ColorTranslator.FromHtml ("#0").ToArgb (), "#0");
			Assert.AreEqual (1, ColorTranslator.FromHtml ("#1").ToArgb (), "#1");
			Assert.AreEqual (255, ColorTranslator.FromHtml ("#FF").ToArgb (), "#FF");
			Assert.AreEqual (-15654349, ColorTranslator.FromHtml ("#123").ToArgb (), "#123");
			Assert.AreEqual (-1, ColorTranslator.FromHtml ("#FFF").ToArgb (), "#FFF");
			Assert.AreEqual (65535, ColorTranslator.FromHtml ("#FFFF").ToArgb (), "#FFFF");
			Assert.AreEqual (-15584170, ColorTranslator.FromHtml ("#123456").ToArgb (), "#123456");
			Assert.AreEqual (-1, ColorTranslator.FromHtml ("#FFFFFF").ToArgb (), "#FFFFFF");
			Assert.AreEqual (305419896, ColorTranslator.FromHtml ("#12345678").ToArgb (), "#12345678");
			Assert.AreEqual (-1, ColorTranslator.FromHtml ("#FFFFFFFF").ToArgb (), "#FFFFFFFF");
			
			Assert.AreEqual (Color.White, ColorTranslator.FromHtml ("#FFFFFF"), "used to resolve to some KnownColor");
			Assert.AreEqual (Color.White, ColorTranslator.FromHtml ("0xFFFFFF"), "used to resolve to some KnownColor");
		}

		[Test]
		public void FromHtml_PoundNegative ()
		{
			Assert.Throws<ArgumentException> (() => ColorTranslator.FromHtml ("#-1"));
		}

		[Test]
		public void FromHtml_PoundTooLarge ()
		{
			Assert.Throws<ArgumentException> (() => ColorTranslator.FromHtml ("#100000000"));
		}

		[Test]
		public void FromHtml_Unknown ()
		{
			Assert.Throws<ArgumentException> (() => ColorTranslator.FromHtml ("unknown-color-test"));
		}

		[Test]
		public void FromHtml ()
		{
			Color [] colors = new Color [] {
Color.Aqua, Color.Black, Color.Blue, Color.Fuchsia, Color.Gray,
Color.Green, Color.Lime, Color.Maroon, Color.Navy, Color.Olive,
Color.Purple, Color.Red, Color.Silver, Color.Teal, Color.White,
Color.Yellow,

SystemColors.ActiveBorder, SystemColors.ActiveCaption,
SystemColors.Control, 
//SystemColors.ControlLightLight,
SystemColors.ActiveCaptionText, SystemColors.GrayText,
//SystemColors.InactiveBorder, SystemColors.InactiveCaption,
SystemColors.InfoText, SystemColors.Menu,
SystemColors.ControlDarkDark, 
//SystemColors.ControlText, SystemColors.ControlDark,
SystemColors.Window,
SystemColors.AppWorkspace, SystemColors.Desktop,
//SystemColors.ControlDark,
SystemColors.ControlText,
SystemColors.Highlight, SystemColors.HighlightText,
//SystemColors.InactiveCaptionText,
SystemColors.Info,
SystemColors.MenuText, SystemColors.ScrollBar,
//SystemColors.ControlLight, SystemColors.ControlLightLight
			};
			string [] htmlColors = new string [] {
"Aqua", "Black", "Blue", "Fuchsia", "Gray", "Green",
"Lime", "Maroon", "Navy", "Olive", "Purple", "Red",
"Silver", "Teal", "White", "Yellow",

"activeborder", "activecaption", "buttonface",
//"buhighlight",
"captiontext", "graytext",
//"iborder", "Icaption", 
"infotext", "menu", "threeddarkshadow",
//"thrface", "Threedshadow",
"window", "appworkspace",
"background", 
//"bshadow",
"buttontext", "highlight",
"highlighttext",
//"icaptiontext",
"infobackground",
"menutext", "scrollbar", 
//"thhighlight", "thlightshadow"
			};
		
			for (int i=0; i<colors.Length; i++)
				Assert.AreEqual (colors[i], ColorTranslator.FromHtml (htmlColors [i]));
		}

		[Test] // 340917
		public void FromHtml_LightGrey ()
		{
			Assert.AreEqual (Color.LightGray, ColorTranslator.FromHtml(ColorTranslator.ToHtml(Color.LightGray)));
		}

		[Test]
		public void FromOle ()
		{
			Assert.AreEqual (Color.FromArgb (0x10, 0x20, 0x30), ColorTranslator.FromOle (0x302010));
			Assert.AreEqual (Color.FromArgb (0xbb, 0x20, 0x30), ColorTranslator.FromOle (unchecked ((int)0xee3020bb)));
		}

		[Test]
		public void FromWin32 ()
		{
			Assert.AreEqual (Color.FromArgb (0x10, 0x20, 0x30), ColorTranslator.FromWin32 (0x302010));
			Assert.AreEqual (Color.FromArgb (0xbb, 0x20, 0x30), ColorTranslator.FromWin32 (unchecked ((int)0xee3020bb)));
		}

		[Test]
		public void ToHtml ()
		{
			string [] htmlColors = new string [] {
"activeborder", "activecaption", "captiontext", "appworkspace", "buttonface",
"buttonshadow", "threeddarkshadow", "buttonface", "buttonhighlight", "buttontext",
"background", "graytext", "highlight", "highlighttext", "highlight", "inactiveborder",
"inactivecaption", "inactivecaptiontext", "infobackground", "infotext", "menu",
"menutext", "scrollbar", "window", "windowframe", "windowtext", 

"Transparent", "AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige",
"Bisque", "Black", "BlanchedAlmond", "Blue", "BlueViolet", "Brown", "BurlyWood",
"CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk",
"Crimson", "Cyan", "DarkBlue", "DarkCyan", "DarkGoldenrod", "DarkGray", "DarkGreen",
"DarkKhaki", "DarkMagenta", "DarkOliveGreen", "DarkOrange", "DarkOrchid", "DarkRed",
"DarkSalmon", "DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray", "DarkTurquoise", "DarkViolet",
"DeepPink", "DeepSkyBlue", "DimGray", "DodgerBlue", "Firebrick", "FloralWhite", "ForestGreen",
"Fuchsia", "Gainsboro", "GhostWhite", "Gold", "Goldenrod", "Gray", "Green", "GreenYellow",
"Honeydew", "HotPink", "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender", "LavenderBlush",
"LawnGreen", "LemonChiffon", "LightBlue", "LightCoral", "LightCyan", "LightGoldenrodYellow",
"LightGrey", "LightGreen", "LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue",
"LightSlateGray", "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen", "Magenta",
"Maroon", "MediumAquamarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen",
"MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed", "MidnightBlue",
"MintCream", "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab",
"Orange", "OrangeRed", "Orchid", "PaleGoldenrod", "PaleGreen", "PaleTurquoise", "PaleVioletRed",
"PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum", "PowderBlue", "Purple", "Red", "RosyBrown",
"RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown", "SeaGreen", "SeaShell", "Sienna", "Silver",
"SkyBlue", "SlateBlue", "SlateGray", "Snow", "SpringGreen", "SteelBlue", "Tan", "Teal",
"Thistle", "Tomato", "Turquoise", "Violet", "Wheat", "White", "WhiteSmoke", "Yellow", "YellowGreen",
											};

			for (KnownColor i=KnownColor.ActiveBorder; i<=KnownColor.YellowGreen; i++)
				Assert.AreEqual (htmlColors[(int)i-1], ColorTranslator.ToHtml (Color.FromKnownColor (i)));
		}

		[Test]
		public void ToOle () {
			Assert.AreEqual (0x302010, ColorTranslator.ToOle (Color.FromArgb (0x10, 0x20, 0x30)));
			Assert.AreEqual (unchecked ((int)0x3020bb), ColorTranslator.ToOle (Color.FromArgb (0xee, 0xbb, 0x20, 0x30)));
		}

		[Test]
		public void ToWin32 () {
			Assert.AreEqual (0x302010, ColorTranslator.ToWin32 (Color.FromArgb (0x10, 0x20, 0x30)));
			Assert.AreEqual (unchecked ((int)0x3020bb), ColorTranslator.ToWin32 (Color.FromArgb (0xee, 0xbb, 0x20, 0x30)));
		}

	}
}

