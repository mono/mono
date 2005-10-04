using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;
using DrawingTestHelper;
using System.Reflection;

namespace Test.Sys.Drawing
{
	/// <summary>
	/// Summary description for Enums.
	/// </summary>
	
	[TestFixture]
	public class Enums
	{
		#region Names Arrays
		string [] ar_known_color = {
									   "ActiveBorder",
									   "ActiveCaption",
									   "ActiveCaptionText",
									   "AppWorkspace",
									   "Control",
									   "ControlDark",
									   "ControlDarkDark",
									   "ControlLight",
									   "ControlLightLight",
									   "ControlText",
									   "Desktop",
									   "GrayText",
									   "Highlight",
									   "HighlightText",
									   "HotTrack",
									   "InactiveBorder",
									   "InactiveCaption",
									   "InactiveCaptionText",
									   "Info",
									   "InfoText",
									   "Menu",
									   "MenuText",
									   "ScrollBar",
									   "Window",
									   "WindowFrame",
									   "WindowText",
									   "Transparent",
									   "AliceBlue",
									   "AntiqueWhite",
									   "Aqua",
									   "Aquamarine",
									   "Azure",
									   "Beige",
									   "Bisque",
									   "Black",
									   "BlanchedAlmond",
									   "Blue",
									   "BlueViolet",
									   "Brown",
									   "BurlyWood",
									   "CadetBlue",
									   "Chartreuse",
									   "Chocolate",
									   "Coral",
									   "CornflowerBlue",
									   "Cornsilk",
									   "Crimson",
									   "Cyan",
									   "DarkBlue",
									   "DarkCyan",
									   "DarkGoldenrod",
									   "DarkGray",
									   "DarkGreen",
									   "DarkKhaki",
									   "DarkMagenta",
									   "DarkOliveGreen",
									   "DarkOrange",
									   "DarkOrchid",
									   "DarkRed",
									   "DarkSalmon",
									   "DarkSeaGreen",
									   "DarkSlateBlue",
									   "DarkSlateGray",
									   "DarkTurquoise",
									   "DarkViolet",
									   "DeepPink",
									   "DeepSkyBlue",
									   "DimGray",
									   "DodgerBlue",
									   "Firebrick",
									   "FloralWhite",
									   "ForestGreen",
									   "Fuchsia",
									   "Gainsboro",
									   "GhostWhite",
									   "Gold",
									   "Goldenrod",
									   "Gray",
									   "Green",
									   "GreenYellow",
									   "Honeydew",
									   "HotPink",
									   "IndianRed",
									   "Indigo",
									   "Ivory",
									   "Khaki",
									   "Lavender",
									   "LavenderBlush",
									   "LawnGreen",
									   "LemonChiffon",
									   "LightBlue",
									   "LightCoral",
									   "LightCyan",
									   "LightGoldenrodYellow",
									   "LightGray",
									   "LightGreen",
									   "LightPink",
									   "LightSalmon",
									   "LightSeaGreen",
									   "LightSkyBlue",
									   "LightSlateGray",
									   "LightSteelBlue",
									   "LightYellow",
									   "Lime",
									   "LimeGreen",
									   "Linen",
									   "Magenta",
									   "Maroon",
									   "MediumAquamarine",
									   "MediumBlue",
									   "MediumOrchid",
									   "MediumPurple",
									   "MediumSeaGreen",
									   "MediumSlateBlue",
									   "MediumSpringGreen",
									   "MediumTurquoise",
									   "MediumVioletRed",
									   "MidnightBlue",
									   "MintCream",
									   "MistyRose",
									   "Moccasin",
									   "NavajoWhite",
									   "Navy",
									   "OldLace",
									   "Olive",
									   "OliveDrab",
									   "Orange",
									   "OrangeRed",
									   "Orchid",
									   "PaleGoldenrod",
									   "PaleGreen",
									   "PaleTurquoise",
									   "PaleVioletRed",
									   "PapayaWhip",
									   "PeachPuff",
									   "Peru",
									   "Pink",
									   "Plum",
									   "PowderBlue",
									   "Purple",
									   "Red",
									   "RosyBrown",
									   "RoyalBlue",
									   "SaddleBrown",
									   "Salmon",
									   "SandyBrown",
									   "SeaGreen",
									   "SeaShell",
									   "Sienna",
									   "Silver",
									   "SkyBlue",
									   "SlateBlue",
									   "SlateGray",
									   "Snow",
									   "SpringGreen",
									   "SteelBlue",
									   "Tan",
									   "Teal",
									   "Thistle",
									   "Tomato",
									   "Turquoise",
									   "Violet",
									   "Wheat",
									   "White",
									   "WhiteSmoke",
									   "Yellow",
									   "YellowGreen"};

		string [] ar_font_style = {
									  "Regular",
									  "Bold",
									  "Italic",
									  "Underline",
									  "Strikeout"};

		
		string [] ar_content_alignment = {
											 "TopLeft",
											 "TopCenter",
											 "TopRight",
											 "MiddleLeft",
											 "MiddleCenter",
											 "MiddleRight",
											 "BottomLeft",
											 "BottomCenter",
											 "BottomRight"};					

		string [] ar_string_alignment = {
											"Near",
											"Center",
											"Far"};					


		string [] ar_string_digit_substitute = {
												   "User",
												   "None",
												   "National",
												   "Traditional"};					

		string [] ar_string_unit = {
									   "World",
									   "Display",
									   "Pixel",
									   "Point",
									   "Inch",
									   "Document",
									   "Millimeter",
									   "Em"};					

		string [] ar_string_trimming = {
										   "None",
										   "Character",
										   "Word",
										   "EllipsisCharacter",
										   "EllipsisWord",
										   "EllipsisPath"};					

		#endregion
		
		[SetUp]
		public void SetUp () 
		{
		}

		#region KnownColor
		[Test]
		public void KnownColors()
		{
			Type t = typeof(KnownColor);

			foreach (string s in ar_known_color)
			{
				try
				{
					FieldInfo fi = t.GetField(s);
					Assert.AreEqual(s, fi.Name);
				}
				catch (Exception)
				{
					Assert.Fail("Color " + s + " is not found");
				}
			}
		}

		[Test]
		public void KnownColorsCount()
		{
			Type t = typeof(KnownColor);

			MemberInfo [] mi = t.GetFields(BindingFlags.Static | BindingFlags.Public);
			Assert.AreEqual(ar_known_color.Length, mi.Length);
		}
		#endregion

		#region FontStyle
		[Test]
		public void FontStyles()
		{
			Type t = typeof(FontStyle);

			foreach (string s in ar_font_style)
			{
				try
				{
					FieldInfo fi = t.GetField(s);
					Assert.AreEqual(s, fi.Name);
				}
				catch (Exception)
				{
					Assert.Fail("Font Style " + s + " is not found");
				}
			}
		}

		[Test]
		public void FontStylesCount()
		{
			Type t = typeof(FontStyle);

			MemberInfo [] mi = t.GetFields(BindingFlags.Static | BindingFlags.Public);
			Assert.AreEqual(ar_font_style.Length, mi.Length);
		}
		#endregion
		
		#region ContentAligment
		[Test]
		public void ContentAlignments()
		{
			Type t = typeof(ContentAlignment);

			foreach (string s in ar_content_alignment)
			{
				try
				{
					FieldInfo fi = t.GetField(s);
					Assert.AreEqual(s, fi.Name);
				}
				catch (Exception)
				{
					Assert.Fail("ContentAligment " + s + " is not found");
				}
			}
		}

		[Test]
		public void ContentAlignmentsCount()
		{
			Type t = typeof(ContentAlignment);

			MemberInfo [] mi = t.GetFields(BindingFlags.Static | BindingFlags.Public);
			Assert.AreEqual(ar_content_alignment.Length, mi.Length);
		}
		#endregion
		
		#region StringAligment
		[Test]
		public void StringAlignments()
		{
			Type t = typeof(StringAlignment);

			foreach (string s in ar_string_alignment)
			{
				try
				{
					FieldInfo fi = t.GetField(s);
					Assert.AreEqual(s, fi.Name);
				}
				catch (Exception)
				{
					Assert.Fail("Font Style " + s + " is not found");
				}
			}
		}

		[Test]
		public void StringAlignmentsCount()
		{
			Type t = typeof(StringAlignment);

			MemberInfo [] mi = t.GetFields(BindingFlags.Static | BindingFlags.Public);
			Assert.AreEqual(ar_string_alignment.Length, mi.Length);
		}
		#endregion
		
		#region StringDigitSubstitute
		[Test]
		public void StringDigitSubstitutes()
		{
			Type t = typeof(StringDigitSubstitute);

			foreach (string s in ar_string_digit_substitute)
			{
				try
				{
					FieldInfo fi = t.GetField(s);
					Assert.AreEqual(s, fi.Name);
				}
				catch (Exception)
				{
					Assert.Fail("Font Style " + s + " is not found");
				}
			}
		}

		[Test]
		public void StringDigitSubstitutesCount()
		{
			Type t = typeof(StringDigitSubstitute);

			MemberInfo [] mi = t.GetFields(BindingFlags.Static | BindingFlags.Public);
			Assert.AreEqual(ar_string_digit_substitute.Length, mi.Length);
		}
		#endregion
		
		#region StringUnit
		[Test]
		public void StringUnits()
		{
			Type t = typeof(StringUnit);

			foreach (string s in ar_string_unit)
			{
				try
				{
					FieldInfo fi = t.GetField(s);
					Assert.AreEqual(s, fi.Name);
				}
				catch (Exception)
				{
					Assert.Fail("Font Style " + s + " is not found");
				}
			}
		}

		[Test]
		public void StringUnitsCount()
		{
			Type t = typeof(StringUnit);

			MemberInfo [] mi = t.GetFields(BindingFlags.Static | BindingFlags.Public);
			Assert.AreEqual(ar_string_unit.Length, mi.Length);
		}
		#endregion
		
		#region StringTrimming
		[Test]
		public void StringTrimmings()
		{
			Type t = typeof(StringTrimming);

			foreach (string s in ar_string_trimming)
			{
				try
				{
					FieldInfo fi = t.GetField(s);
					Assert.AreEqual(s, fi.Name);
				}
				catch (Exception)
				{
					Assert.Fail("Font Style " + s + " is not found");
				}
			}
		}

		[Test]
		public void StringTrimmingsCount()
		{
			Type t = typeof(StringTrimming);

			MemberInfo [] mi = t.GetFields(BindingFlags.Static | BindingFlags.Public);
			Assert.AreEqual(ar_string_trimming.Length, mi.Length);
		}
		#endregion

	}
}
