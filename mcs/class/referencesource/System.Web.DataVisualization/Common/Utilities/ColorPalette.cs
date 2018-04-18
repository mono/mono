//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ColorPalette.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//				System.Web.UI.WebControls[Windows.Forms].Charting.Utilities
//
//	Classes:	ChartPaletteColors
//
//  Purpose:	A utility class which defines chart palette colors.
//              These palettes are used to assign unique colors to 
//              different chart series. For some chart types, like 
//              Pie, different colors are applied on the data point 
//              level.
//
//              Selected chart series/points palette is exposed 
//              through Chart.Palette property. Series.Palette 
//              property should be used to set different palette 
//              color for each point of the series. 
//
//	Reviewed:	AG - August 7, 2002
//              AG - Microsoft 5, 2007
//
//===================================================================

#region Used Namespaces

using System;
using System.Drawing;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
#else
	using System.Web.UI.DataVisualization.Charting;
#endif
#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	#region Color palettes enumeration

	/// <summary>
	/// Chart color palettes enumeration
	/// </summary>
	public enum ChartColorPalette
	{ 
		/// <summary>
		/// Palette not set.
		/// </summary>
		None, 

		/// <summary>
        /// Bright palette.
		/// </summary>
		Bright, 

		/// <summary>
		/// Palette with gray scale colors.
		/// </summary>
		Grayscale, 

		/// <summary>
		/// Palette with Excel style colors.
		/// </summary>
		Excel,

		/// <summary>
		/// Palette with LightStyle style colors.
		/// </summary>
		Light,

		/// <summary>
		/// Palette with Pastel style colors.
		/// </summary>
		Pastel,

		/// <summary>
		/// Palette with Earth Tones style colors.
		/// </summary>
		EarthTones,

		/// <summary>
		/// Palette with SemiTransparent style colors.
		/// </summary>
		SemiTransparent, 

		/// <summary>
		/// Palette with Berry style colors.
		/// </summary>
		Berry,

		/// <summary>
		/// Palette with Chocolate style colors.
		/// </summary>
		Chocolate,

		/// <summary>
		/// Palette with Fire style colors.
		/// </summary>
		Fire,

		/// <summary>
		/// Palette with SeaGreen style colors.
		/// </summary>
		SeaGreen,

		/// <summary>
		/// Bright pastel palette.
		/// </summary>
		BrightPastel
	};

	#endregion	
}

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Utilities
#else
	namespace System.Web.UI.DataVisualization.Charting.Utilities
#endif
{
	/// <summary>
	/// ChartPaletteColors is a utility class which provides access 
    /// to the predefined chart color palettes. These palettes are 
    /// used to assign unique colors to different chart series. 
    /// For some chart types, like Pie, different colors are applied 
    /// on the data point level.
    /// 
    /// GetPaletteColors method takes a ChartColorPalette enumeration 
    /// as a parameter and returns back an array of Colors. Each 
    /// palette contains different number of colors but it is a 
    /// good practice to keep this number around 15.
	/// </summary>
	internal static class ChartPaletteColors
	{
		#region Fields

		// Fields which store the palette color values
        private static Color[] _colorsGrayScale = InitializeGrayScaleColors();
		private static	Color[] _colorsDefault = {
			Color.Green,
			Color.Blue,
			Color.Purple,
			Color.Lime,
			Color.Fuchsia,
			Color.Teal,
			Color.Yellow,
			Color.Gray,
			Color.Aqua,
			Color.Navy,
			Color.Maroon,
			Color.Red,
			Color.Olive,
			Color.Silver,
			Color.Tomato,
			Color.Moccasin
			};
		
		private static	Color[] _colorsPastel = {
													Color.SkyBlue,
													Color.LimeGreen,
													Color.MediumOrchid,
													Color.LightCoral,
													Color.SteelBlue,
													Color.YellowGreen,
													Color.Turquoise,
													Color.HotPink,
													Color.Khaki,
													Color.Tan,
													Color.DarkSeaGreen,
													Color.CornflowerBlue,
													Color.Plum,
													Color.CadetBlue,
													Color.PeachPuff,
													Color.LightSalmon
												};

		private static	Color[] _colorsEarth = {
												   Color.FromArgb(255, 128, 0),
												   Color.DarkGoldenrod,
												   Color.FromArgb(192, 64, 0),
												   Color.OliveDrab,
												   Color.Peru,
												   Color.FromArgb(192, 192, 0),
												   Color.ForestGreen,
												   Color.Chocolate,
												   Color.Olive,
												   Color.LightSeaGreen,
												   Color.SandyBrown,
												   Color.FromArgb(0, 192, 0),
												   Color.DarkSeaGreen,
												   Color.Firebrick,
												   Color.SaddleBrown,
												   Color.FromArgb(192, 0, 0)
											   };

		private static	Color[] _colorsSemiTransparent = {
													Color.FromArgb(150, 255, 0, 0),
													Color.FromArgb(150, 0, 255, 0),
													Color.FromArgb(150, 0, 0, 255),
													Color.FromArgb(150, 255, 255, 0),
													Color.FromArgb(150, 0, 255, 255),
													Color.FromArgb(150, 255, 0, 255),
													Color.FromArgb(150, 170, 120, 20),
													Color.FromArgb(80, 255, 0, 0),
													Color.FromArgb(80, 0, 255, 0),
													Color.FromArgb(80, 0, 0, 255),
													Color.FromArgb(80, 255, 255, 0),
													Color.FromArgb(80, 0, 255, 255),
													Color.FromArgb(80, 255, 0, 255),
													Color.FromArgb(80, 170, 120, 20),
													Color.FromArgb(150, 100, 120, 50),
													Color.FromArgb(150, 40, 90, 150)
											  };
		
		private static	Color[] _colorsLight = {
												   Color.Lavender,
												   Color.LavenderBlush,
												   Color.PeachPuff,
												   Color.LemonChiffon,
												   Color.MistyRose,
												   Color.Honeydew,
												   Color.AliceBlue,
												   Color.WhiteSmoke,
												   Color.AntiqueWhite,
												   Color.LightCyan
											   };

		private static	Color[] _colorsExcel = {
			Color.FromArgb(153,153,255),
			Color.FromArgb(153,51,102),
			Color.FromArgb(255,255,204),
			Color.FromArgb(204,255,255),
			Color.FromArgb(102,0,102),
			Color.FromArgb(255,128,128),
			Color.FromArgb(0,102,204),
			Color.FromArgb(204,204,255),
			Color.FromArgb(0,0,128),
			Color.FromArgb(255,0,255),
			Color.FromArgb(255,255,0),
			Color.FromArgb(0,255,255),
			Color.FromArgb(128,0,128),
			Color.FromArgb(128,0,0),
			Color.FromArgb(0,128,128),
			Color.FromArgb(0,0,255)};

		private static	Color[] _colorsBerry = {
												  Color.BlueViolet,
												  Color.MediumOrchid,
												  Color.RoyalBlue,
												  Color.MediumVioletRed,
												  Color.Blue,
												  Color.BlueViolet,
												  Color.Orchid,
												  Color.MediumSlateBlue,
												  Color.FromArgb(192, 0, 192),
												  Color.MediumBlue,
												  Color.Purple
											  };

		private static	Color[] _colorsChocolate = {
												  Color.Sienna,
												  Color.Chocolate,
												  Color.DarkRed,
												  Color.Peru,
												  Color.Brown,
												  Color.SandyBrown,
												  Color.SaddleBrown,
												  Color.FromArgb(192, 64, 0),
												  Color.Firebrick,
												  Color.FromArgb(182, 92, 58)
											  };

		private static	Color[] _colorsFire = {
													  Color.Gold,
													  Color.Red,
													  Color.DeepPink,
													  Color.Crimson,
													  Color.DarkOrange,
													  Color.Magenta,
													  Color.Yellow,
													  Color.OrangeRed,
													  Color.MediumVioletRed,
													  Color.FromArgb(221, 226, 33)
												  };

		private static	Color[] _colorsSeaGreen = {
												 Color.SeaGreen,
												 Color.MediumAquamarine,
												 Color.SteelBlue,
												 Color.DarkCyan,
												 Color.CadetBlue,
												 Color.MediumSeaGreen,
												 Color.MediumTurquoise,
												 Color.LightSteelBlue,
												 Color.DarkSeaGreen,
												 Color.SkyBlue
											 };

        private static Color[] _colorsBrightPastel = {
												   Color.FromArgb(65, 140, 240),
												   Color.FromArgb(252, 180, 65),
												   Color.FromArgb(224, 64, 10),
												   Color.FromArgb(5, 100, 146),
												   Color.FromArgb(191, 191, 191),
												   Color.FromArgb(26, 59, 105),
												   Color.FromArgb(255, 227, 130),
												   Color.FromArgb(18, 156, 221),
												   Color.FromArgb(202, 107, 75),
												   Color.FromArgb(0, 92, 219),
												   Color.FromArgb(243, 210, 136),
												   Color.FromArgb(80, 99, 129),
												   Color.FromArgb(241, 185, 168),
												   Color.FromArgb(224, 131, 10),
												   Color.FromArgb(120, 147, 190)
											   };

		#endregion
		
		#region Constructor

		/// <summary>
		/// Initializes the GrayScale color array
		/// </summary>
		private static Color[] InitializeGrayScaleColors()
		{
			// Define gray scale colors
			Color[] grayScale = new Color[16];
			for(int i = 0; i < grayScale.Length; i++)
			{
				int colorValue = 200 - i * (180/16);
				grayScale[i] = Color.FromArgb(colorValue, colorValue, colorValue);
			}

            return grayScale;
		}

		#endregion

		#region Methods

        /// <summary>
        /// Return array of colors for the specified palette. Number of
        /// colors returned varies depending on the palette selected.
        /// </summary>
        /// <param name="palette">Palette to get the colors for.</param>
        /// <returns>Array of colors.</returns>
		public static Color[] GetPaletteColors(ChartColorPalette palette)
		{
			switch(palette)
			{
				case(ChartColorPalette.None):
				{
                    throw (new ArgumentException(SR.ExceptionPaletteIsEmpty));
				}
				case(ChartColorPalette.Bright):
					return _colorsDefault;
				case(ChartColorPalette.Grayscale):
                    return _colorsGrayScale;
				case(ChartColorPalette.Excel):
					return _colorsExcel;
				case(ChartColorPalette.Pastel):
					return _colorsPastel;
				case(ChartColorPalette.Light):
					return _colorsLight;
				case(ChartColorPalette.EarthTones):
					return _colorsEarth;
				case(ChartColorPalette.SemiTransparent):
					return _colorsSemiTransparent;
				case(ChartColorPalette.Berry):
					return _colorsBerry;
				case(ChartColorPalette.Chocolate):
					return _colorsChocolate;
				case(ChartColorPalette.Fire):
					return _colorsFire;
				case(ChartColorPalette.SeaGreen):
					return _colorsSeaGreen;
				case(ChartColorPalette.BrightPastel):
                    return _colorsBrightPastel;
			}
			return null;
		}

		#endregion
	}
}
