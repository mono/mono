using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NUnit.Framework;
using System.Reflection;

namespace Test.Sys.Drawing
{
	/// <summary>
	/// Summary description for Pens.
	/// </summary>
	
	[TestFixture]
	public class ColorsFixture
	{

		[SetUp]
		public void SetUp () 
		{
		}

		#region names array
		private string [] ar_colors = {
										  "Transparent,0,255,255,255,1,0,0",
										  "AliceBlue,255,240,248,255,0.9705882,208,1",
										  "AntiqueWhite,255,250,235,215,0.9117647,34.28571,0.7777778",
										  "Aqua,255,0,255,255,0.5,180,1",
										  "Aquamarine,255,127,255,212,0.7490196,159.8438,1",
										  "Azure,255,240,255,255,0.9705882,180,1",
										  "Beige,255,245,245,220,0.9117647,60,0.5555556",
										  "Bisque,255,255,228,196,0.8843137,32.54237,1",
										  "Black,255,0,0,0,0,0,0",
										  "BlanchedAlmond,255,255,235,205,0.9019608,36,1",
										  "Blue,255,0,0,255,0.5,240,1",
										  "BlueViolet,255,138,43,226,0.527451,271.1476,0.7593361",
										  "Brown,255,165,42,42,0.4058824,0,0.5942029",
										  "BurlyWood,255,222,184,135,0.7,33.7931,0.5686275",
										  "CadetBlue,255,95,158,160,0.5,181.8462,0.254902",
										  "Chartreuse,255,127,255,0,0.5,90.11765,1",
										  "Chocolate,255,210,105,30,0.4705882,25,0.75",
										  "Coral,255,255,127,80,0.6568627,16.11428,1",
										  "CornflowerBlue,255,100,149,237,0.6607843,218.5401,0.7919075",
										  "Cornsilk,255,255,248,220,0.9313725,48,1",
										  "Crimson,255,220,20,60,0.4705882,348,0.8333333",
										  "Cyan,255,0,255,255,0.5,180,1",
										  "DarkBlue,255,0,0,139,0.272549,240,1",
										  "DarkCyan,255,0,139,139,0.272549,180,1",
										  "DarkGoldenrod,255,184,134,11,0.3823529,42.65896,0.8871795",
										  "DarkGray,255,169,169,169,0.6627451,0,0",
										  "DarkGreen,255,0,100,0,0.1960784,120,1",
										  "DarkKhaki,255,189,183,107,0.5803922,55.60976,0.3831776",
										  "DarkMagenta,255,139,0,139,0.272549,300,1",
										  "DarkOliveGreen,255,85,107,47,0.3019608,82,0.3896104",
										  "DarkOrange,255,255,140,0,0.5,32.94118,1",
										  "DarkOrchid,255,153,50,204,0.4980392,280.1299,0.6062992",
										  "DarkRed,255,139,0,0,0.272549,0,1",
										  "DarkSalmon,255,233,150,122,0.6960784,15.13514,0.7161291",
										  "DarkSeaGreen,255,143,188,139,0.6411765,115.102,0.2677596",
										  "DarkSlateBlue,255,72,61,139,0.3921569,248.4615,0.39",
										  "DarkSlateGray,255,47,79,79,0.2470588,180,0.2539683",
										  "DarkTurquoise,255,0,206,209,0.4098039,180.8612,1",
										  "DarkViolet,255,148,0,211,0.4137255,282.0853,1",
										  "DeepPink,255,255,20,147,0.5392157,327.5745,1",
										  "DeepSkyBlue,255,0,191,255,0.5,195.0588,1",
										  "DimGray,255,105,105,105,0.4117647,0,0",
										  "DodgerBlue,255,30,144,255,0.5588235,209.6,1",
										  "Firebrick,255,178,34,34,0.4156863,0,0.6792453",
										  "FloralWhite,255,255,250,240,0.9705882,40,1",
										  "ForestGreen,255,34,139,34,0.3392157,120,0.6069364",
										  "Fuchsia,255,255,0,255,0.5,300,1",
										  "Gainsboro,255,220,220,220,0.8627451,0,0",
										  "GhostWhite,255,248,248,255,0.9862745,240,1",
										  "Gold,255,255,215,0,0.5,50.58823,1",
										  "Goldenrod,255,218,165,32,0.4901961,42.90322,0.744",
										  "Gray,255,128,128,128,0.5019608,0,0",
										  "Green,255,0,128,0,0.2509804,120,1",
										  "GreenYellow,255,173,255,47,0.5921569,83.65385,1",
										  "Honeydew,255,240,255,240,0.9705882,120,1",
										  "HotPink,255,255,105,180,0.7058824,330,1",
										  "IndianRed,255,205,92,92,0.5823529,0,0.5305164",
										  "Indigo,255,75,0,130,0.254902,274.6154,1",
										  "Ivory,255,255,255,240,0.9705882,60,1",
										  "Khaki,255,240,230,140,0.7450981,54,0.7692308",
										  "Lavender,255,230,230,250,0.9411765,240,0.6666667",
										  "LavenderBlush,255,255,240,245,0.9705882,340,1",
										  "LawnGreen,255,124,252,0,0.4941176,90.47619,1",
										  "LemonChiffon,255,255,250,205,0.9019608,54,1",
										  "LightBlue,255,173,216,230,0.7901961,194.7368,0.5327103",
										  "LightCoral,255,240,128,128,0.7215686,0,0.7887324",
										  "LightCyan,255,224,255,255,0.9392157,180,1",
										  "LightGoldenrodYellow,255,250,250,210,0.9019608,60,0.8",
										  "LightGreen,255,144,238,144,0.7490196,120,0.734375",
										  "LightGray,255,211,211,211,0.827451,0,0",
										  "LightPink,255,255,182,193,0.8568628,350.9589,1",
										  "LightSalmon,255,255,160,122,0.7392157,17.14286,1",
										  "LightSeaGreen,255,32,178,170,0.4117647,176.7123,0.6952381",
										  "LightSkyBlue,255,135,206,250,0.754902,202.9565,0.92",
										  "LightSlateGray,255,119,136,153,0.5333334,210,0.1428572",
										  "LightSteelBlue,255,176,196,222,0.7803922,213.913,0.4107143",
										  "LightYellow,255,255,255,224,0.9392157,60,1",
										  "Lime,255,0,255,0,0.5,120,1",
										  "LimeGreen,255,50,205,50,0.5,120,0.6078432",
										  "Linen,255,250,240,230,0.9411765,30,0.6666667",
										  "Magenta,255,255,0,255,0.5,300,1",
										  "Maroon,255,128,0,0,0.2509804,0,1",
										  "MediumAquamarine,255,102,205,170,0.6019608,159.6116,0.5073892",
										  "MediumBlue,255,0,0,205,0.4019608,240,1",
										  "MediumOrchid,255,186,85,211,0.5803922,288.0952,0.5887851",
										  "MediumPurple,255,147,112,219,0.6490196,259.6262,0.5977654",
										  "MediumSeaGreen,255,60,179,113,0.4686275,146.7227,0.497908",
										  "MediumSlateBlue,255,123,104,238,0.6705883,248.5075,0.797619",
										  "MediumSpringGreen,255,0,250,154,0.4901961,156.96,1",
										  "MediumTurquoise,255,72,209,204,0.5509804,177.8102,0.5982533",
										  "MediumVioletRed,255,199,21,133,0.4313726,322.2472,0.8090909",
										  "MidnightBlue,255,25,25,112,0.2686275,240,0.6350365",
										  "MintCream,255,245,255,250,0.9803922,150,1",
										  "MistyRose,255,255,228,225,0.9411765,6,1",
										  "Moccasin,255,255,228,181,0.854902,38.10811,1",
										  "NavajoWhite,255,255,222,173,0.8392157,35.85366,1",
										  "Navy,255,0,0,128,0.2509804,240,1",
										  "OldLace,255,253,245,230,0.9470588,39.13044,0.8518519",
										  "Olive,255,128,128,0,0.2509804,60,1",
										  "OliveDrab,255,107,142,35,0.3470588,79.62617,0.6045198",
										  "Orange,255,255,165,0,0.5,38.82353,1",
										  "OrangeRed,255,255,69,0,0.5,16.23529,1",
										  "Orchid,255,218,112,214,0.6470588,302.2642,0.5888889",
										  "PaleGoldenrod,255,238,232,170,0.8,54.70588,0.6666667",
										  "PaleGreen,255,152,251,152,0.7901961,120,0.9252337",
										  "PaleTurquoise,255,175,238,238,0.809804,180,0.6494845",
										  "PaleVioletRed,255,219,112,147,0.6490196,340.3738,0.5977654",
										  "PapayaWhip,255,255,239,213,0.9176471,37.14286,1",
										  "PeachPuff,255,255,218,185,0.8627451,28.28572,1",
										  "Peru,255,205,133,63,0.5254902,29.57747,0.5867769",
										  "Pink,255,255,192,203,0.8764706,349.5238,1",
										  "Plum,255,221,160,221,0.7470589,300,0.4728682",
										  "PowderBlue,255,176,224,230,0.7960784,186.6667,0.5192308",
										  "Purple,255,128,0,128,0.2509804,300,1",
										  "Red,255,255,0,0,0.5,0,1",
										  "RosyBrown,255,188,143,143,0.6490196,0,0.2513967",
										  "RoyalBlue,255,65,105,225,0.5686275,225,0.7272727",
										  "SaddleBrown,255,139,69,19,0.3098039,25,0.7594936",
										  "Salmon,255,250,128,114,0.7137255,6.176474,0.9315069",
										  "SandyBrown,255,244,164,96,0.6666667,27.56757,0.8705882",
										  "SeaGreen,255,46,139,87,0.3627451,146.4516,0.5027027",
										  "SeaShell,255,255,245,238,0.9666667,24.70588,1",
										  "Sienna,255,160,82,45,0.4019608,19.30435,0.5609756",
										  "Silver,255,192,192,192,0.7529412,0,0",
										  "SkyBlue,255,135,206,235,0.7254902,197.4,0.7142857",
										  "SlateBlue,255,106,90,205,0.5784314,248.3478,0.5348837",
										  "SlateGray,255,112,128,144,0.5019608,210,0.1259843",
										  "Snow,255,255,250,250,0.9901961,0,1",
										  "SpringGreen,255,0,255,127,0.5,149.8824,1",
										  "SteelBlue,255,70,130,180,0.4901961,207.2727,0.44",
										  "Tan,255,210,180,140,0.6862745,34.28571,0.4375",
										  "Teal,255,0,128,128,0.2509804,180,1",
										  "Thistle,255,216,191,216,0.7980392,300,0.2427184",
										  "Tomato,255,255,99,71,0.6392157,9.130435,1",
										  "Turquoise,255,64,224,208,0.5647059,174,0.7207207",
										  "Violet,255,238,130,238,0.7215686,300,0.7605634",
										  "Wheat,255,245,222,179,0.8313726,39.09091,0.7674419",
										  "White,255,255,255,255,1,0,0",
										  "WhiteSmoke,255,245,245,245,0.9607843,0,0",
										  "Yellow,255,255,255,0,0.5,60,1",
										  "YellowGreen,255,154,205,50,0.5,79.74194,0.6078432"};

		string [] ar_system_colors = {
										 "ActiveBorder,255,212,208,200,0.8078431,40,0.122449",
										 "ActiveCaption,255,10,36,106,0.227451,223.75,0.8275862",
										 "ActiveCaptionText,255,255,255,255,1,0,0",
										 "AppWorkspace,255,128,128,128,0.5019608,0,0",
										 "Control,255,212,208,200,0.8078431,40,0.122449",
										 "ControlDark,255,128,128,128,0.5019608,0,0",
										 "ControlDarkDark,255,64,64,64,0.2509804,0,0",
										 "ControlLight,255,212,208,200,0.8078431,40,0.122449",
										 "ControlLightLight,255,255,255,255,1,0,0",
										 "ControlText,255,0,0,0,0,0,0",
										 "Desktop,255,58,110,165,0.4372549,210.8411,0.4798206",
										 "GrayText,255,128,128,128,0.5019608,0,0",
										 "Highlight,255,10,36,106,0.227451,223.75,0.8275862",
										 "HighlightText,255,255,255,255,1,0,0",
										 "HotTrack,255,0,0,128,0.2509804,240,1",
										 "InactiveBorder,255,212,208,200,0.8078431,40,0.122449",
										 "InactiveCaption,255,128,128,128,0.5019608,0,0",
										 "InactiveCaptionText,255,212,208,200,0.8078431,40,0.122449",
										 "Info,255,255,255,225,0.9411765,60,1",
										 "InfoText,255,0,0,0,0,0,0",
										 "Menu,255,212,208,200,0.8078431,40,0.122449",
										 "MenuText,255,0,0,0,0,0,0",
										 "ScrollBar,255,212,208,200,0.8078431,40,0.122449",
										 "Window,255,255,255,255,1,0,0",
										 "WindowFrame,255,0,0,0,0,0,0",
										 "WindowText,255,0,0,0,0,0,0"};

		string [] ar_system_color_conversions = {
													"ActiveBorder,activeborder",
													"ActiveCaption,activecaption",
													"ActiveCaptionText,captiontext",
													"AppWorkspace,appworkspace",
													"Control,buttonface",
													"ControlDark,buttonshadow",
													"ControlDarkDark,threeddarkshadow",
													"ControlLight,buttonface",
													"ControlLightLight,buttonhighlight",
													"ControlText,buttontext",
													"Desktop,background",
													"GrayText,graytext",
													"Highlight,highlight",
													"HighlightText,highlighttext",
													"HotTrack,highlight",
													"InactiveBorder,inactiveborder",
													"InactiveCaption,inactivecaption",
													"InactiveCaptionText,inactivecaptiontext",
													"Info,infobackground",
													"InfoText,infotext",
													"Menu,menu",
													"MenuText,menutext",
													"ScrollBar,scrollbar",
													"Window,window",
													"WindowFrame,windowframe",
													"WindowText,windowtext"};
		#endregion

	
		[Test]
		public void ColorPropertyCount()
		{
			Type t = typeof(Color);
			PropertyInfo [] pi = t.GetProperties(BindingFlags.Static | BindingFlags.Public);

			int i = 0;
			foreach (PropertyInfo p in pi)
			{
				if (p.PropertyType == typeof(Color))
				{
					i++;
				}
			}
			Assert.AreEqual(ar_colors.Length, i, "Number of Colors");
		}

		[Test]
		public void ColorProperties () 
		{
			Type t = typeof(Color);
			foreach (string s in ar_colors)
			{
				string [] col = s.Split(',');
				MemberInfo [] mi = t.GetMember(col[0]);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						Color color = (Color)p.GetValue(null, null);
						Assert.AreEqual(col[0], color.Name, col[0] + " Color Name is wrong");
						Assert.AreEqual(Convert.ToByte( col[1] ), color.A, col[0] + " Color A is wrong");
						Assert.AreEqual(Convert.ToByte( col[2] ), color.R, col[0] + " Color R is wrong");
						Assert.AreEqual(Convert.ToByte( col[3] ), color.G, col[0] + " Color G is wrong");
						Assert.AreEqual(Convert.ToByte( col[4] ), color.B, col[0] + " Color B is wrong");

						Assert.AreEqual(float.Parse(col[5]), color.GetBrightness(), 0.001F, col[0] + " Color.GetBrightness() is wrong");
						Assert.AreEqual(float.Parse(col[6]), color.GetHue(), 0.001F, col[0] + " Color.GetHue() is wrong");
						Assert.AreEqual(float.Parse(col[7]), color.GetSaturation(), 0.001F, col[0] + " Color.GetSaturation() is wrong");

						Assert.AreEqual(true, color.IsNamedColor, col[0] + " IsNamedColor is wrong");
						Assert.AreEqual(false, color.IsSystemColor, col[0] + " IsSystemColor is wrong");
						Assert.AreEqual(true, color.IsKnownColor, col[0] + " IsKnownColor is wrong");
					}
					else
					{
						Assert.Fail(s + " is not property of Color class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in Color class");
				}
			}
		}

		[Test]
		public void SystemColorPropertyCount()
		{
			Type t = typeof(SystemColors);
			PropertyInfo [] pi = t.GetProperties(BindingFlags.Static | BindingFlags.Public);

			int i = 0;
			foreach (PropertyInfo p in pi)
			{
				if (p.PropertyType == typeof(Color))
				{
					i++;
				}
			}
			Assert.AreEqual(ar_system_colors.Length, i, "Number of SystemColors");
		}

		[Test]
		public void SystemColorProperties () 
		{
			Type t = typeof(SystemColors);
			foreach (string s in ar_system_colors)
			{
				string [] col = s.Split(',');
				MemberInfo [] mi = t.GetMember(col[0]);

				if (mi.Length == 1)
				{
					if (mi[0].MemberType == MemberTypes.Property)
					{
						PropertyInfo p = (PropertyInfo)mi[0];
						Color color = (Color)p.GetValue(null, null);
						Assert.AreEqual(col[0], color.Name, col[0] + " Color Name is wrong");
						Assert.AreEqual(Convert.ToByte( col[1] ), color.A, col[0] + " Color A is wrong");
						Assert.AreEqual(Convert.ToByte( col[2] ), color.R, col[0] + " Color R is wrong");
						Assert.AreEqual(Convert.ToByte( col[3] ), color.G, col[0] + " Color G is wrong");
						Assert.AreEqual(Convert.ToByte( col[4] ), color.B, col[0] + " Color B is wrong");

						Assert.AreEqual(float.Parse(col[5]), color.GetBrightness(), 0.001F, col[0] + " Color.GetBrightness() is wrong");
						Assert.AreEqual(float.Parse(col[6]), color.GetHue(), 0.001F, col[0] + " Color.GetHue() is wrong");
						Assert.AreEqual(float.Parse(col[7]), color.GetSaturation(), 0.001F, col[0] + " Color.GetSaturation() is wrong");

						Assert.AreEqual(true, color.IsNamedColor, col[0] + " IsNamedColor is wrong");
						Assert.AreEqual(true, color.IsSystemColor, col[0] + " IsSystemColor is wrong");
						Assert.AreEqual(true, color.IsKnownColor, col[0] + " IsKnownColor is wrong");
					}
					else
					{
						Assert.Fail(s + " is not property of SystemColors class");
					}
				}
				else 
				{
					Assert.Fail("Property " + s + " not found in SystemColors class");
				}
			}
		}
		[Test]
		public void SystemColorTranslator()
		{
			Type t = typeof(SystemColors);

			foreach (string s in ar_system_color_conversions)
			{
				string [] col = s.Split(',');
				try
				{
					PropertyInfo pi = t.GetProperty(col[0]);
					Color c = (Color)pi.GetValue(null, null);

					Assert.AreEqual(col[1], ColorTranslator.ToHtml(c), col[0] + " is worng");
				}
				catch (Exception)
				{
					Assert.Fail(col[0] + " failed");
				}
			}
		}
	}
}