//
// System.Windows.Drawing.Brushes.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//	Suesan Chaney
//	  Peter Bartok (pbartok@novell.com)
//
// (C) Ximian, Inc., 2002 http://www.ximian.com
// (C) Novell, Inc., 2004 http://www.novell.com
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

namespace System.Drawing 
{
	public sealed class Brushes 
	{
		private static SolidBrush aliceBlue = null;
		private static SolidBrush antiqueWhite = null;
		private static SolidBrush aqua = null;
		private static SolidBrush aquamarine = null;
		private static SolidBrush azure = null;
		private static SolidBrush beige = null;
		private static SolidBrush bisque = null;
		private static SolidBrush black = null;
		private static SolidBrush blanchedAlmond = null;
		private static SolidBrush blue = null;
		private static SolidBrush blueViolet = null;
		private static SolidBrush brown = null;
		private static SolidBrush burlyWood = null;
		private static SolidBrush cadetBlue = null;
		private static SolidBrush chartreuse = null;
		private static SolidBrush chocolate = null;
		private static SolidBrush coral = null;
		private static SolidBrush cornflowerBlue = null;
		private static SolidBrush cornsilk = null;
		private static SolidBrush crimson = null;
		private static SolidBrush cyan = null;
		private static SolidBrush darkBlue = null;
		private static SolidBrush darkCyan = null;
		private static SolidBrush darkGoldenrod = null;
		private static SolidBrush darkGray = null;
		private static SolidBrush darkGreen = null;
		private static SolidBrush darkKhaki = null;
		private static SolidBrush darkMagenta = null;
		private static SolidBrush darkOliveGreen = null;
		private static SolidBrush darkOrange = null;
		private static SolidBrush darkOrchid = null;
		private static SolidBrush darkRed = null;
		private static SolidBrush darkSalmon = null;
		private static SolidBrush darkSeaGreen = null;
		private static SolidBrush darkSlateBlue = null;
		private static SolidBrush darkSlateGray = null;
		private static SolidBrush darkTurquoise = null;
		private static SolidBrush darkViolet = null;
		private static SolidBrush deepPink = null;
		private static SolidBrush deepSkyBlue = null;
		private static SolidBrush dimGray = null;
		private static SolidBrush dodgerBlue = null;
		private static SolidBrush firebrick = null;
		private static SolidBrush floralWhite = null;
		private static SolidBrush forestGreen = null;
		private static SolidBrush fuchsia = null;
		private static SolidBrush gainsboro = null;
		private static SolidBrush ghostWhite = null;
		private static SolidBrush gold = null;
		private static SolidBrush goldenrod = null;
		private static SolidBrush gray = null;
		private static SolidBrush green = null;
		private static SolidBrush greenYellow = null;
		private static SolidBrush honeydew = null;
		private static SolidBrush hotPink = null;
		private static SolidBrush indianRed = null;
		private static SolidBrush indigo = null;
		private static SolidBrush ivory = null;
		private static SolidBrush khaki = null;
		private static SolidBrush lavender = null;
		private static SolidBrush lavenderBlush = null;
		private static SolidBrush lawnGreen = null;
		private static SolidBrush lemonChiffon = null;
		private static SolidBrush lightBlue = null;
		private static SolidBrush lightCoral = null;
		private static SolidBrush lightCyan = null;
		private static SolidBrush lightGoldenrodYellow = null;
		private static SolidBrush lightGray = null;
		private static SolidBrush lightGreen = null;
		private static SolidBrush lightPink = null;
		private static SolidBrush lightSalmon = null;
		private static SolidBrush lightSeaGreen = null;
		private static SolidBrush lightSkyBlue = null;
		private static SolidBrush lightSlateGray = null;
		private static SolidBrush lightSteelBlue = null;
		private static SolidBrush lightYellow = null;
		private static SolidBrush lime = null;
		private static SolidBrush limeGreen = null;
		private static SolidBrush linen = null;
		private static SolidBrush magenta = null;
		private static SolidBrush maroon = null;
		private static SolidBrush mediumAquamarine = null;
		private static SolidBrush mediumBlue = null;
		private static SolidBrush mediumOrchid = null;
		private static SolidBrush mediumPurple = null;
		private static SolidBrush mediumSeaGreen = null;
		private static SolidBrush mediumSlateBlue = null;
		private static SolidBrush mediumSpringGreen = null;
		private static SolidBrush mediumTurquoise = null;
		private static SolidBrush mediumVioletRed = null;
		private static SolidBrush midnightBlue = null;
		private static SolidBrush mintCream = null;
		private static SolidBrush mistyRose = null;
		private static SolidBrush moccasin = null;
		private static SolidBrush navajoWhite = null;
		private static SolidBrush navy = null;
		private static SolidBrush oldLace = null;
		private static SolidBrush olive = null;
		private static SolidBrush oliveDrab = null;
		private static SolidBrush orange = null;
		private static SolidBrush orangeRed = null;
		private static SolidBrush orchid = null;
		private static SolidBrush paleGoldenrod = null;
		private static SolidBrush paleGreen = null;
		private static SolidBrush paleTurquoise = null;
		private static SolidBrush paleVioletRed = null;
		private static SolidBrush papayaWhip = null;
		private static SolidBrush peachPuff = null;
		private static SolidBrush peru = null;
		private static SolidBrush pink = null;
		private static SolidBrush plum = null;
		private static SolidBrush powderBlue = null;
		private static SolidBrush purple = null;
		private static SolidBrush red = null;
		private static SolidBrush rosyBrown = null;
		private static SolidBrush royalBlue = null;
		private static SolidBrush saddleBrown = null;
		private static SolidBrush salmon = null;
		private static SolidBrush sandyBrown = null;
		private static SolidBrush seaGreen = null;
		private static SolidBrush seaShell = null;
		private static SolidBrush sienna = null;
		private static SolidBrush silver = null;
		private static SolidBrush skyBlue = null;
		private static SolidBrush slateBlue = null;
		private static SolidBrush slateGray = null;
		private static SolidBrush snow = null;
		private static SolidBrush springGreen = null;
		private static SolidBrush steelBlue = null;
		private static SolidBrush tan = null;
		private static SolidBrush teal = null;
		private static SolidBrush thistle = null;
		private static SolidBrush tomato = null;
		private static SolidBrush transparent = null;
		private static SolidBrush turquoise = null;
		private static SolidBrush violet = null;
		private static SolidBrush wheat = null;
		private static SolidBrush white = null;
		private static SolidBrush whiteSmoke = null;
		private static SolidBrush yellow = null;
		private static SolidBrush yellowGreen = null;

		// We intentionally do not set the is_modifiable=false flag on
		// the brushes, to stay Microsoft compatible

		private Brushes () { }
	
		public static Brush AliceBlue {
			get {
				if (aliceBlue==null) {
					aliceBlue=new SolidBrush(Color.AliceBlue);
				}
				return(aliceBlue);
			}
		}

		public static Brush AntiqueWhite {
			get {
				if (antiqueWhite==null) {
					antiqueWhite=new SolidBrush(Color.AntiqueWhite);
				}
				return(antiqueWhite);
			}
		}

		public static Brush Aqua {
			get {
				if (aqua==null) {
					aqua=new SolidBrush(Color.Aqua);
				}
				return(aqua);
			}
		}

		public static Brush Aquamarine {
			get {
				if (aquamarine==null) {
					aquamarine=new SolidBrush(Color.Aquamarine);
				}
				return(aquamarine);
			}
		}

		public static Brush Azure {
			get {
				if (azure==null) {
					azure=new SolidBrush(Color.Azure);
				}
				return(azure);
			}
		}

		public static Brush Beige {
			get {
				if (beige==null) {
					beige=new SolidBrush(Color.Beige);
				}
				return(beige);
			}
		}

		public static Brush Bisque {
			get {
				if (bisque==null) {
					bisque=new SolidBrush(Color.Bisque);
				}
				return(bisque);
			}
		}

		public static Brush Black {
			get {
				if (black==null) {
					black=new SolidBrush(Color.Black);
				}
				return(black);
			}
		}

		public static Brush BlanchedAlmond {
			get {
				if (blanchedAlmond==null) {
					blanchedAlmond=new SolidBrush(Color.BlanchedAlmond);
				}
				return(blanchedAlmond);
			}
		}

		public static Brush Blue {
			get {
				if (blue==null) {
					blue=new SolidBrush(Color.Blue);
				}
				return(blue);
			}
		}

		public static Brush BlueViolet {
			get {
				if (blueViolet==null) {
					blueViolet=new SolidBrush(Color.BlueViolet);
				}
				return(blueViolet);
			}
		}

		public static Brush Brown {
			get {
				if (brown==null) {
					brown=new SolidBrush(Color.Brown);
				}
				return(brown);
			}
		}

		public static Brush BurlyWood {
			get {
				if (burlyWood==null) {
					burlyWood=new SolidBrush(Color.BurlyWood);
				}
				return(burlyWood);
			}
		}

		public static Brush CadetBlue {
			get {
				if (cadetBlue==null) {
					cadetBlue=new SolidBrush(Color.CadetBlue);
				}
				return(cadetBlue);
			}
		}

		public static Brush Chartreuse {
			get {
				if (chartreuse==null) {
					chartreuse=new SolidBrush(Color.Chartreuse);
				}
				return(chartreuse);
			}
		}

		public static Brush Chocolate {
			get {
				if (chocolate==null) {
					chocolate=new SolidBrush(Color.Chocolate);
				}
				return(chocolate);
			}
		}

		public static Brush Coral {
			get {
				if (coral==null) {
					coral=new SolidBrush(Color.Coral);
				}
				return(coral);
			}
		}

		public static Brush CornflowerBlue {
			get {
				if (cornflowerBlue==null) {
					cornflowerBlue=new SolidBrush(Color.CornflowerBlue);
				}
				return(cornflowerBlue);
			}
		}

		public static Brush Cornsilk {
			get {
				if (cornsilk==null) {
					cornsilk=new SolidBrush(Color.Cornsilk);
				}
				return(cornsilk);
			}
		}

		public static Brush Crimson {
			get {
				if (crimson==null) {
					crimson=new SolidBrush(Color.Crimson);
				}
				return(crimson);
			}
		}

		public static Brush Cyan {
			get {
				if (cyan==null) {
					cyan=new SolidBrush(Color.Cyan);
				}
				return(cyan);
			}
		}

		public static Brush DarkBlue {
			get {
				if (darkBlue==null) {
					darkBlue=new SolidBrush(Color.DarkBlue);
				}
				return(darkBlue);
			}
		}

		public static Brush DarkCyan {
			get {
				if (darkCyan==null) {
					darkCyan=new SolidBrush(Color.DarkCyan);
				}
				return(darkCyan);
			}
		}

		public static Brush DarkGoldenrod {
			get {
				if (darkGoldenrod==null) {
					darkGoldenrod=new SolidBrush(Color.DarkGoldenrod);
				}
				return(darkGoldenrod);
			}
		}

		public static Brush DarkGray {
			get {
				if (darkGray==null) {
					darkGray=new SolidBrush(Color.DarkGray);
				}
				return(darkGray);
			}
		}

		public static Brush DarkGreen {
			get {
				if (darkGreen==null) {
					darkGreen=new SolidBrush(Color.DarkGreen);
				}
				return(darkGreen);
			}
		}

		public static Brush DarkKhaki {
			get {
				if (darkKhaki==null) {
					darkKhaki=new SolidBrush(Color.DarkKhaki);
				}
				return(darkKhaki);
			}
		}

		public static Brush DarkMagenta {
			get {
				if (darkMagenta==null) {
					darkMagenta=new SolidBrush(Color.DarkMagenta);
				}
				return(darkMagenta);
			}
		}

		public static Brush DarkOliveGreen {
			get {
				if (darkOliveGreen==null) {
					darkOliveGreen=new SolidBrush(Color.DarkOliveGreen);
				}
				return(darkOliveGreen);
			}
		}

		public static Brush DarkOrange {
			get {
				if (darkOrange==null) {
					darkOrange=new SolidBrush(Color.DarkOrange);
				}
				return(darkOrange);
			}
		}

		public static Brush DarkOrchid {
			get {
				if (darkOrchid==null) {
					darkOrchid=new SolidBrush(Color.DarkOrchid);
				}
				return(darkOrchid);
			}
		}

		public static Brush DarkRed {
			get {
				if (darkRed==null) {
					darkRed=new SolidBrush(Color.DarkRed);
				}
				return(darkRed);
			}
		}

		public static Brush DarkSalmon {
			get {
				if (darkSalmon==null) {
					darkSalmon=new SolidBrush(Color.DarkSalmon);
				}
				return(darkSalmon);
			}
		}

		public static Brush DarkSeaGreen {
			get {
				if (darkSeaGreen==null) {
					darkSeaGreen=new SolidBrush(Color.DarkSeaGreen);
				}
				return(darkSeaGreen);
			}
		}

		public static Brush DarkSlateBlue {
			get {
				if (darkSlateBlue==null) {
					darkSlateBlue=new SolidBrush(Color.DarkSlateBlue);
				}
				return(darkSlateBlue);
			}
		}

		public static Brush DarkSlateGray {
			get {
				if (darkSlateGray==null) {
					darkSlateGray=new SolidBrush(Color.DarkSlateGray);
				}
				return(darkSlateGray);
			}
		}

		public static Brush DarkTurquoise {
			get {
				if (darkTurquoise==null) {
					darkTurquoise=new SolidBrush(Color.DarkTurquoise);
				}
				return(darkTurquoise);
			}
		}

		public static Brush DarkViolet {
			get {
				if (darkViolet==null) {
					darkViolet=new SolidBrush(Color.DarkViolet);
				}
				return(darkViolet);
			}
		}

		public static Brush DeepPink {
			get {
				if (deepPink==null) {
					deepPink=new SolidBrush(Color.DeepPink);
				}
				return(deepPink);
			}
		}

		public static Brush DeepSkyBlue {
			get {
				if (deepSkyBlue==null) {
					deepSkyBlue=new SolidBrush(Color.DeepSkyBlue);
				}
				return(deepSkyBlue);
			}
		}

		public static Brush DimGray {
			get {
				if (dimGray==null) {
					dimGray=new SolidBrush(Color.DimGray);
				}
				return(dimGray);
			}
		}

		public static Brush DodgerBlue {
			get {
				if (dodgerBlue==null) {
					dodgerBlue=new SolidBrush(Color.DodgerBlue);
				}
				return(dodgerBlue);
			}
		}

		public static Brush Firebrick {
			get {
				if (firebrick==null) {
					firebrick=new SolidBrush(Color.Firebrick);
				}
				return(firebrick);
			}
		}

		public static Brush FloralWhite {
			get {
				if (floralWhite==null) {
					floralWhite=new SolidBrush(Color.FloralWhite);
				}
				return(floralWhite);
			}
		}

		public static Brush ForestGreen {
			get {
				if (forestGreen==null) {
					forestGreen=new SolidBrush(Color.ForestGreen);
				}
				return(forestGreen);
			}
		}

		public static Brush Fuchsia {
			get {
				if (fuchsia==null) {
					fuchsia=new SolidBrush(Color.Fuchsia);
				}
				return(fuchsia);
			}
		}

		public static Brush Gainsboro {
			get {
				if (gainsboro==null) {
					gainsboro=new SolidBrush(Color.Gainsboro);
				}
				return(gainsboro);
			}
		}

		public static Brush GhostWhite {
			get {
				if (ghostWhite==null) {
					ghostWhite=new SolidBrush(Color.GhostWhite);
				}
				return(ghostWhite);
			}
		}

		public static Brush Gold {
			get {
				if (gold==null) {
					gold=new SolidBrush(Color.Gold);
				}
				return(gold);
			}
		}

		public static Brush Goldenrod {
			get {
				if (goldenrod==null) {
					goldenrod=new SolidBrush(Color.Goldenrod);
				}
				return(goldenrod);
			}
		}

		public static Brush Gray {
			get {
				if (gray==null) {
					gray=new SolidBrush(Color.Gray);
				}
				return(gray);
			}
		}

		public static Brush Green {
			get {
				if (green==null) {
					green=new SolidBrush(Color.Green);
				}
				return(green);
			}
		}

		public static Brush GreenYellow {
			get {
				if (greenYellow==null) {
					greenYellow=new SolidBrush(Color.GreenYellow);
				}
				return(greenYellow);
			}
		}

		public static Brush Honeydew {
			get {
				if (honeydew==null) {
					honeydew=new SolidBrush(Color.Honeydew);
				}
				return(honeydew);
			}
		}

		public static Brush HotPink {
			get {
				if (hotPink==null) {
					hotPink=new SolidBrush(Color.HotPink);
				}
				return(hotPink);
			}
		}

		public static Brush IndianRed {
			get {
				if (indianRed==null) {
					indianRed=new SolidBrush(Color.IndianRed);
				}
				return(indianRed);
			}
		}

		public static Brush Indigo {
			get {
				if (indigo==null) {
					indigo=new SolidBrush(Color.Indigo);
				}
				return(indigo);
			}
		}

		public static Brush Ivory {
			get {
				if (ivory==null) {
					ivory=new SolidBrush(Color.Ivory);
				}
				return(ivory);
			}
		}

		public static Brush Khaki {
			get {
				if (khaki==null) {
					khaki=new SolidBrush(Color.Khaki);
				}
				return(khaki);
			}
		}

		public static Brush Lavender {
			get {
				if (lavender==null) {
					lavender=new SolidBrush(Color.Lavender);
				}
				return(lavender);
			}
		}

		public static Brush LavenderBlush {
			get {
				if (lavenderBlush==null) {
					lavenderBlush=new SolidBrush(Color.LavenderBlush);
				}
				return(lavenderBlush);
			}
		}

		public static Brush LawnGreen {
			get {
				if (lawnGreen==null) {
					lawnGreen=new SolidBrush(Color.LawnGreen);
				}
				return(lawnGreen);
			}
		}

		public static Brush LemonChiffon {
			get {
				if (lemonChiffon==null) {
					lemonChiffon=new SolidBrush(Color.LemonChiffon);
				}
				return(lemonChiffon);
			}
		}

		public static Brush LightBlue {
			get {
				if (lightBlue==null) {
					lightBlue=new SolidBrush(Color.LightBlue);
				}
				return(lightBlue);
			}
		}

		public static Brush LightCoral {
			get {
				if (lightCoral==null) {
					lightCoral=new SolidBrush(Color.LightCoral);
				}
				return(lightCoral);
			}
		}

		public static Brush LightCyan {
			get {
				if (lightCyan==null) {
					lightCyan=new SolidBrush(Color.LightCyan);
				}
				return(lightCyan);
			}
		}

		public static Brush LightGoldenrodYellow {
			get {
				if (lightGoldenrodYellow==null) {
					lightGoldenrodYellow=new SolidBrush(Color.LightGoldenrodYellow);
				}
				return(lightGoldenrodYellow);
			}
		}

		public static Brush LightGray {
			get {
				if (lightGray==null) {
					lightGray=new SolidBrush(Color.LightGray);
				}
				return(lightGray);
			}
		}

		public static Brush LightGreen {
			get {
				if (lightGreen==null) {
					lightGreen=new SolidBrush(Color.LightGreen);
				}
				return(lightGreen);
			}
		}

		public static Brush LightPink {
			get {
				if (lightPink==null) {
					lightPink=new SolidBrush(Color.LightPink);
				}
				return(lightPink);
			}
		}

		public static Brush LightSalmon {
			get {
				if (lightSalmon==null) {
					lightSalmon=new SolidBrush(Color.LightSalmon);
				}
				return(lightSalmon);
			}
		}

		public static Brush LightSeaGreen {
			get {
				if (lightSeaGreen==null) {
					lightSeaGreen=new SolidBrush(Color.LightSeaGreen);
				}
				return(lightSeaGreen);
			}
		}

		public static Brush LightSkyBlue {
			get {
				if (lightSkyBlue==null) {
					lightSkyBlue=new SolidBrush(Color.LightSkyBlue);
				}
				return(lightSkyBlue);
			}
		}

		public static Brush LightSlateGray {
			get {
				if (lightSlateGray==null) {
					lightSlateGray=new SolidBrush(Color.LightSlateGray);
				}
				return(lightSlateGray);
			}
		}

		public static Brush LightSteelBlue {
			get {
				if (lightSteelBlue==null) {
					lightSteelBlue=new SolidBrush(Color.LightSteelBlue);
				}
				return(lightSteelBlue);
			}
		}

		public static Brush LightYellow {
			get {
				if (lightYellow==null) {
					lightYellow=new SolidBrush(Color.LightYellow);
				}
				return(lightYellow);
			}
		}

		public static Brush Lime {
			get {
				if (lime==null) {
					lime=new SolidBrush(Color.Lime);
				}
				return(lime);
			}
		}

		public static Brush LimeGreen {
			get {
				if (limeGreen==null) {
					limeGreen=new SolidBrush(Color.LimeGreen);
				}
				return(limeGreen);
			}
		}

		public static Brush Linen {
			get {
				if (linen==null) {
					linen=new SolidBrush(Color.Linen);
				}
				return(linen);
			}
		}

		public static Brush Magenta {
			get {
				if (magenta==null) {
					magenta=new SolidBrush(Color.Magenta);
				}
				return(magenta);
			}
		}

		public static Brush Maroon {
			get {
				if (maroon==null) {
					maroon=new SolidBrush(Color.Maroon);
				}
				return(maroon);
			}
		}

		public static Brush MediumAquamarine {
			get {
				if (mediumAquamarine==null) {
					mediumAquamarine=new SolidBrush(Color.MediumAquamarine);
				}
				return(mediumAquamarine);
			}
		}

		public static Brush MediumBlue {
			get {
				if (mediumBlue==null) {
					mediumBlue=new SolidBrush(Color.MediumBlue);
				}
				return(mediumBlue);
			}
		}

		public static Brush MediumOrchid {
			get {
				if (mediumOrchid==null) {
					mediumOrchid=new SolidBrush(Color.MediumOrchid);
				}
				return(mediumOrchid);
			}
		}

		public static Brush MediumPurple {
			get {
				if (mediumPurple==null) {
					mediumPurple=new SolidBrush(Color.MediumPurple);
				}
				return(mediumPurple);
			}
		}

		public static Brush MediumSeaGreen {
			get {
				if (mediumSeaGreen==null) {
					mediumSeaGreen=new SolidBrush(Color.MediumSeaGreen);
				}
				return(mediumSeaGreen);
			}
		}

		public static Brush MediumSlateBlue {
			get {
				if (mediumSlateBlue==null) {
					mediumSlateBlue=new SolidBrush(Color.MediumSlateBlue);
				}
				return(mediumSlateBlue);
			}
		}

		public static Brush MediumSpringGreen {
			get {
				if (mediumSpringGreen==null) {
					mediumSpringGreen=new SolidBrush(Color.MediumSpringGreen);
				}
				return(mediumSpringGreen);
			}
		}

		public static Brush MediumTurquoise {
			get {
				if (mediumTurquoise==null) {
					mediumTurquoise=new SolidBrush(Color.MediumTurquoise);
				}
				return(mediumTurquoise);
			}
		}

		public static Brush MediumVioletRed {
			get {
				if (mediumVioletRed==null) {
					mediumVioletRed=new SolidBrush(Color.MediumVioletRed);
				}
				return(mediumVioletRed);
			}
		}

		public static Brush MidnightBlue {
			get {
				if (midnightBlue==null) {
					midnightBlue=new SolidBrush(Color.MidnightBlue);
				}
				return(midnightBlue);
			}
		}

		public static Brush MintCream {
			get {
				if (mintCream==null) {
					mintCream=new SolidBrush(Color.MintCream);
				}
				return(mintCream);
			}
		}

		public static Brush MistyRose {
			get {
				if (mistyRose==null) {
					mistyRose=new SolidBrush(Color.MistyRose);
				}
				return(mistyRose);
			}
		}

		public static Brush Moccasin {
			get {
				if (moccasin==null) {
					moccasin=new SolidBrush(Color.Moccasin);
				}
				return(moccasin);
			}
		}

		public static Brush NavajoWhite {
			get {
				if (navajoWhite==null) {
					navajoWhite=new SolidBrush(Color.NavajoWhite);
				}
				return(navajoWhite);
			}
		}

		public static Brush Navy {
			get {
				if (navy==null) {
					navy=new SolidBrush(Color.Navy);
				}
				return(navy);
			}
		}

		public static Brush OldLace {
			get {
				if (oldLace==null) {
					oldLace=new SolidBrush(Color.OldLace);
				}
				return(oldLace);
			}
		}

		public static Brush Olive {
			get {
				if (olive==null) {
					olive=new SolidBrush(Color.Olive);
				}
				return(olive);
			}
		}

		public static Brush OliveDrab {
			get {
				if (oliveDrab==null) {
					oliveDrab=new SolidBrush(Color.OliveDrab);
				}
				return(oliveDrab);
			}
		}

		public static Brush Orange {
			get {
				if (orange==null) {
					orange=new SolidBrush(Color.Orange);
				}
				return(orange);
			}
		}

		public static Brush OrangeRed {
			get {
				if (orangeRed==null) {
					orangeRed=new SolidBrush(Color.OrangeRed);
				}
				return(orangeRed);
			}
		}

		public static Brush Orchid {
			get {
				if (orchid==null) {
					orchid=new SolidBrush(Color.Orchid);
				}
				return(orchid);
			}
		}

		public static Brush PaleGoldenrod {
			get {
				if (paleGoldenrod==null) {
					paleGoldenrod=new SolidBrush(Color.PaleGoldenrod);
				}
				return(paleGoldenrod);
			}
		}

		public static Brush PaleGreen {
			get {
				if (paleGreen==null) {
					paleGreen=new SolidBrush(Color.PaleGreen);
				}
				return(paleGreen);
			}
		}

		public static Brush PaleTurquoise {
			get {
				if (paleTurquoise==null) {
					paleTurquoise=new SolidBrush(Color.PaleTurquoise);
				}
				return(paleTurquoise);
			}
		}

		public static Brush PaleVioletRed {
			get {
				if (paleVioletRed==null) {
					paleVioletRed=new SolidBrush(Color.PaleVioletRed);
				}
				return(paleVioletRed);
			}
		}

		public static Brush PapayaWhip {
			get {
				if (papayaWhip==null) {
					papayaWhip=new SolidBrush(Color.PapayaWhip);
				}
				return(papayaWhip);
			}
		}

		public static Brush PeachPuff {
			get {
				if (peachPuff==null) {
					peachPuff=new SolidBrush(Color.PeachPuff);
				}
				return(peachPuff);
			}
		}

		public static Brush Peru {
			get {
				if (peru==null) {
					peru=new SolidBrush(Color.Peru);
				}
				return(peru);
			}
		}

		public static Brush Pink {
			get {
				if (pink==null) {
					pink=new SolidBrush(Color.Pink);
				}
				return(pink);
			}
		}

		public static Brush Plum {
			get {
				if (plum==null) {
					plum=new SolidBrush(Color.Plum);
				}
				return(plum);
			}
		}

		public static Brush PowderBlue {
			get {
				if (powderBlue==null) {
					powderBlue=new SolidBrush(Color.PowderBlue);
				}
				return(powderBlue);
			}
		}

		public static Brush Purple {
			get {
				if (purple==null) {
					purple=new SolidBrush(Color.Purple);
				}
				return(purple);
			}
		}

		public static Brush Red {
			get {
				if (red==null) {
					red=new SolidBrush(Color.Red);
				}
				return(red);
			}
		}

		public static Brush RosyBrown {
			get {
				if (rosyBrown==null) {
					rosyBrown=new SolidBrush(Color.RosyBrown);
				}
				return(rosyBrown);
			}
		}

		public static Brush RoyalBlue {
			get {
				if (royalBlue==null) {
					royalBlue=new SolidBrush(Color.RoyalBlue);
				}
				return(royalBlue);
			}
		}

		public static Brush SaddleBrown {
			get {
				if (saddleBrown==null) {
					saddleBrown=new SolidBrush(Color.SaddleBrown);
				}
				return(saddleBrown);
			}
		}

		public static Brush Salmon {
			get {
				if (salmon==null) {
					salmon=new SolidBrush(Color.Salmon);
				}
				return(salmon);
			}
		}

		public static Brush SandyBrown {
			get {
				if (sandyBrown==null) {
					sandyBrown=new SolidBrush(Color.SandyBrown);
				}
				return(sandyBrown);
			}
		}

		public static Brush SeaGreen {
			get {
				if (seaGreen==null) {
					seaGreen=new SolidBrush(Color.SeaGreen);
				}
				return(seaGreen);
			}
		}

		public static Brush SeaShell {
			get {
				if (seaShell==null) {
					seaShell=new SolidBrush(Color.SeaShell);
				}
				return(seaShell);
			}
		}

		public static Brush Sienna {
			get {
				if (sienna==null) {
					sienna=new SolidBrush(Color.Sienna);
				}
				return(sienna);
			}
		}

		public static Brush Silver {
			get {
				if (silver==null) {
					silver=new SolidBrush(Color.Silver);
				}
				return(silver);
			}
		}

		public static Brush SkyBlue {
			get {
				if (skyBlue==null) {
					skyBlue=new SolidBrush(Color.SkyBlue);
				}
				return(skyBlue);
			}
		}

		public static Brush SlateBlue {
			get {
				if (slateBlue==null) {
					slateBlue=new SolidBrush(Color.SlateBlue);
				}
				return(slateBlue);
			}
		}

		public static Brush SlateGray {
			get {
				if (slateGray==null) {
					slateGray=new SolidBrush(Color.SlateGray);
				}
				return(slateGray);
			}
		}

		public static Brush Snow {
			get {
				if (snow==null) {
					snow=new SolidBrush(Color.Snow);
				}
				return(snow);
			}
		}

		public static Brush SpringGreen {
			get {
				if (springGreen==null) {
					springGreen=new SolidBrush(Color.SpringGreen);
				}
				return(springGreen);
			}
		}

		public static Brush SteelBlue {
			get {
				if (steelBlue==null) {
					steelBlue=new SolidBrush(Color.SteelBlue);
				}
				return(steelBlue);
			}
		}

		public static Brush Tan {
			get {
				if (tan==null) {
					tan=new SolidBrush(Color.Tan);
				}
				return(tan);
			}
		}

		public static Brush Teal {
			get {
				if (teal==null) {
					teal=new SolidBrush(Color.Teal);
				}
				return(teal);
			}
		}

		public static Brush Thistle {
			get {
				if (thistle==null) {
					thistle=new SolidBrush(Color.Thistle);
				}
				return(thistle);
			}
		}

		public static Brush Tomato {
			get {
				if (tomato==null) {
					tomato=new SolidBrush(Color.Tomato);
				}
				return(tomato);
			}
		}

		public static Brush Transparent {
			get {
				if (transparent==null) {
					transparent=new SolidBrush(Color.Transparent);
				}
				return(transparent);
			}
		}

		public static Brush Turquoise {
			get {
				if (turquoise==null) {
					turquoise=new SolidBrush(Color.Turquoise);
				}
				return(turquoise);
			}
		}

		public static Brush Violet {
			get {
				if (violet==null) {
					violet=new SolidBrush(Color.Violet);
				}
				return(violet);
			}
		}

		public static Brush Wheat {
			get {
				if (wheat==null) {
					wheat=new SolidBrush(Color.Wheat);
				}
				return(wheat);
			}
		}

		public static Brush White {
			get {
				if (white==null) {
					white=new SolidBrush(Color.White);
				}
				return(white);
			}
		}

		public static Brush WhiteSmoke {
			get {
				if (whiteSmoke==null) {
					whiteSmoke=new SolidBrush(Color.WhiteSmoke);
				}
				return(whiteSmoke);
			}
		}

		public static Brush Yellow {
			get {
				if (yellow==null) {
					yellow=new SolidBrush(Color.Yellow);
				}
				return(yellow);
			}
		}

		public static Brush YellowGreen {
			get {
				if (yellowGreen==null) {
					yellowGreen=new SolidBrush(Color.YellowGreen);
				}
				return(yellowGreen);
			}
		}

	}
}
