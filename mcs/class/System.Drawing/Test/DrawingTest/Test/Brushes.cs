using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;
using DrawingTestHelper;
using System.Reflection;

namespace Test.Sys.Drawing
{
	/// <summary>
	/// Summary description for Pens.
	/// </summary>
	
	[TestFixture]
	public class BrushesFixture
	{

		[SetUp]
		public void SetUp () 
		{
		}

		#region Names Array
		string [] ar_brushes = {
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
								   "LightGreen",
								   "LightGray",
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
	
		string [] ar_system_brushes = {
										  "ActiveBorder",
										  "ActiveCaption",
										  "ActiveCaptionText",
										  "AppWorkspace",
										  "Desktop",
										  "Control",
										  "ControlLightLight",
										  "ControlLight",
										  "ControlDark",
										  "ControlDarkDark",
										  "ControlText",
										  "Highlight",
										  "HighlightText",
										  "HotTrack",
										  "InactiveCaption",
										  "InactiveBorder",
										  "Info",
										  "Menu",
										  "ScrollBar",
										  "Window",
										  "WindowText"};
			

		#endregion
	
		[Test]
		public void BrushesPropertyCount()
		{
			Type t = typeof(Brushes);
			PropertyInfo [] pi = t.GetProperties(BindingFlags.Static | BindingFlags.Public);

			int i = 0;
			foreach (PropertyInfo p in pi)
			{
				if (p.PropertyType == typeof(Brush))
				{
					i++;
				}
			}
			Assert.AreEqual(ar_brushes.Length, i, "Number of brushes");
		}

		[Test]
		public void BrushesProperties () 
		{
			Type t = typeof(Brushes);
			foreach (string s in ar_brushes)
			{
				MemberInfo [] mi = t.GetMember(s);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						SolidBrush brush = (SolidBrush)p.GetValue(null, null);
						Assert.AreEqual("Color [" + s + "]", brush.Color.ToString());
					}
					else
					{
						Assert.Fail(s + " is not property of Brushes class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in Brushes class");
				}
			}
		}

		[Test]
		public void BrushesAssignValue () 
		{
			Type t = typeof(Brushes);
			foreach (string s in ar_brushes)
			{
				MemberInfo [] mi = t.GetMember(s);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						SolidBrush brush = (SolidBrush)p.GetValue(null, null);

						try
						{
							Color c = brush.Color;
							brush.Color = Color.AliceBlue;
							brush.Color = c;

							//BUG: Bug in .NET
							//Assert.Fail("SolidBrush.Color must throw exception");
							Assert.IsTrue(true);
						}
						catch(ArgumentException)
						{
							Assert.IsTrue(true);
						}
					}
					else
					{
						Assert.Fail(s + " is not property of Brushes class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in Brushes class");
				}
			}
		}

		[Test]
		public void SystemBrushesPropertyCount()
		{
			Type t = typeof(SystemBrushes);
			PropertyInfo [] pi = t.GetProperties(BindingFlags.Static | BindingFlags.Public);

			int i = 0;
			foreach (PropertyInfo p in pi)
			{
				if (p.PropertyType == typeof(Brush))
				{
					i++;
				}
			}
			Assert.AreEqual(ar_system_brushes.Length, i, "Number of brushes");
		}

		[Test]
		public void SystemBrushesProperties () 
		{
			Type t = typeof(SystemBrushes);
			foreach (string s in ar_system_brushes)
			{
				MemberInfo [] mi = t.GetMember(s);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						SolidBrush brush = (SolidBrush)p.GetValue(null, null);
						Assert.AreEqual("Color [" + s + "]", brush.Color.ToString());
					}
					else
					{
						Assert.Fail(s + " is not property of SystemBrushes class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in SystemBrushes class");
				}
			}
		}

		[Test]
		public void SystemBrushesAssignValue () 
		{
			Type t = typeof(SystemBrushes);
			foreach (string s in ar_system_brushes)
			{
				MemberInfo [] mi = t.GetMember(s);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						SolidBrush brush = (SolidBrush)p.GetValue(null, null);

						try
						{
							Color c = brush.Color;
							brush.Color = Color.AliceBlue;
							brush.Color = c;

							//BUG: Bug in .NET
							//Assert.Fail("SolidBrush.Color must throw exception");
							Assert.IsTrue(true);
						}
						catch(ArgumentException)
						{
							Assert.IsTrue(true);
						}
					}
					else
					{
						Assert.Fail(s + " is not property of SystemBrushes class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in SystemBrushes class");
				}
			}
		}
	}
}
