using System;
using System.Drawing;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ColorTranslatorFixture {
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

