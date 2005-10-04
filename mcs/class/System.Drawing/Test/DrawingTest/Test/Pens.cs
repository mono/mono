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
	public class PensFixture
	{

		[SetUp]
		public void SetUp () 
		{
		}

		#region names array
		private string [] ar_pens = {
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

		string [] ar_system_pens = {
									   "ActiveCaptionText",
									   "Control",
									   "ControlText",
									   "ControlDark",
									   "ControlDarkDark",
									   "ControlLight",
									   "ControlLightLight",
									   "GrayText",
									   "Highlight",
									   "HighlightText",
									   "InactiveCaptionText",
									   "InfoText",
									   "MenuText",
									   "WindowFrame",
									   "WindowText"};
		#endregion


		[Test]
		public void PensPropertyCount()
		{
			Type t = typeof(Pens);
			PropertyInfo [] pi = t.GetProperties(BindingFlags.Static | BindingFlags.Public);

			int i = 0;
			foreach (PropertyInfo p in pi)
			{
				if (p.PropertyType == typeof(Pen))
				{
					i++;
				}
			}
			Assert.AreEqual(ar_pens.Length, i, "Number of Pens");
		}

		[Test]
		public void SystemPensPropertyCount()
		{
			Type t = typeof(SystemPens);
			PropertyInfo [] pi = t.GetProperties(BindingFlags.Static | BindingFlags.Public);

			int i = 0;
			foreach (PropertyInfo p in pi)
			{
				if (p.PropertyType == typeof(Pen))
				{
					i++;
				}
			}
			Assert.AreEqual(ar_system_pens.Length, i, "Number of System Pens");
		}

		[Test]
		public void PensProperties () 
		{
			Type t = typeof(Pens);
			foreach (string s in ar_pens)
			{
				MemberInfo [] mi = t.GetMember(s);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						Pen pen = (Pen)p.GetValue(null, null);
						Assert.AreEqual("Color [" + s + "]", pen.Color.ToString());
					}
					else
					{
						Assert.Fail(s + " is not property of Pens class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in Pens class");
				}
			}
		}

		[Test]
		public void SystemPensProperties () 
		{
			Type t = typeof(SystemPens);
			foreach (string s in ar_system_pens)
			{
				MemberInfo [] mi = t.GetMember(s);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						Pen pen = (Pen)p.GetValue(null, null);
						Assert.AreEqual("Color [" + s + "]", pen.Color.ToString());
					}
					else
					{
						Assert.Fail(s + " is not property of SystemPens class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in SystemPens class");
				}
			}
		}

		[Test]
		public void PenAssignValue () 
		{
			Type t = typeof(Pens);
			foreach (string s in ar_pens)
			{
				MemberInfo [] mi = t.GetMember(s);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						Pen pen = (Pen)p.GetValue(null, null);

						try
						{
							pen.Color = Color.AliceBlue;
							Assert.Fail("Pen.Color must throw exception");
						}
						catch(ArgumentException)
						{
							Assert.IsTrue(true);
						}
					}
					else
					{
						Assert.Fail(s + " is not property of Pens class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in Pens class");
				}
			}
		}

		[Test]
		public void SystemPenAssignValue () 
		{
			Type t = typeof(SystemPens);
			foreach (string s in ar_system_pens)
			{
				MemberInfo [] mi = t.GetMember(s);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						Pen pen = (Pen)p.GetValue(null, null);

						try
						{
							pen.Color = Color.AliceBlue;
							Assert.Fail("SystemPen.Color must throw exception");
						}
						catch(ArgumentException)
						{
							Assert.IsTrue(true);
						}
					}
					else
					{
						Assert.Fail(s + " is not property of SystemPen class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in SystemPen class");
				}
			}
		}
	}
}
