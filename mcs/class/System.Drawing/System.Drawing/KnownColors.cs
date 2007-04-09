//
// System.Drawing.KnownColors
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Peter Dennis Bartok (pbartok@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

namespace System.Drawing {

#if NET_2_0
	internal static class KnownColors {
#else
	internal class KnownColors {

		private KnownColors ()
		{
		}
#endif
		static Color[] knownColors = new Color [] {
			Color.Empty,
			Color.FromArgbSystem (0xFF, 0xD4, 0xD0, 0xC8, "ActiveBorder", KnownColor.ActiveBorder), 
			Color.FromArgbSystem (0xFF, 0x00, 0x54, 0xE3, "ActiveCaption", KnownColor.ActiveCaption), 
			Color.FromArgbSystem (0xFF, 0xFF, 0xFF, 0xFF, "ActiveCaptionText", KnownColor.ActiveCaptionText), 
			Color.FromArgbSystem (0xFF, 0x80, 0x80, 0x80, "AppWorkspace", KnownColor.AppWorkspace), 
			Color.FromArgbSystem (0xFF, 0xEC, 0xE9, 0xD8, "Control", KnownColor.Control), 
			Color.FromArgbSystem (0xFF, 0xAC, 0xA8, 0x99, "ControlDark", KnownColor.ControlDark), 
			Color.FromArgbSystem (0xFF, 0x71, 0x6F, 0x64, "ControlDarkDark", KnownColor.ControlDarkDark), 
			Color.FromArgbSystem (0xFF, 0xF1, 0xEF, 0xE2, "ControlLight", KnownColor.ControlLight), 
			Color.FromArgbSystem (0xFF, 0xFF, 0xFF, 0xFF, "ControlLightLight", KnownColor.ControlLightLight), 
			Color.FromArgbSystem (0xFF, 0x00, 0x00, 0x00, "ControlText", KnownColor.ControlText), 
			Color.FromArgbSystem (0xFF, 0x00, 0x4E, 0x98, "Desktop", KnownColor.Desktop), 
			Color.FromArgbSystem (0xFF, 0xAC, 0xA8, 0x99, "GrayText", KnownColor.GrayText), 
			Color.FromArgbSystem (0xFF, 0x31, 0x6A, 0xC5, "Highlight", KnownColor.Highlight), 
			Color.FromArgbSystem (0xFF, 0xFF, 0xFF, 0xFF, "HighlightText", KnownColor.HighlightText), 
			Color.FromArgbSystem (0xFF, 0x00, 0x00, 0x80, "HotTrack", KnownColor.HotTrack), 
			Color.FromArgbSystem (0xFF, 0xD4, 0xD0, 0xC8, "InactiveBorder", KnownColor.InactiveBorder), 
			Color.FromArgbSystem (0xFF, 0x7A, 0x96, 0xDF, "InactiveCaption", KnownColor.InactiveCaption), 
			Color.FromArgbSystem (0xFF, 0xD8, 0xE4, 0xF8, "InactiveCaptionText", KnownColor.InactiveCaptionText), 
			Color.FromArgbSystem (0xFF, 0xFF, 0xFF, 0xE1, "Info", KnownColor.Info), 
			Color.FromArgbSystem (0xFF, 0x00, 0x00, 0x00, "InfoText", KnownColor.InfoText), 
			Color.FromArgbSystem (0xFF, 0xFF, 0xFF, 0xFF, "Menu", KnownColor.Menu), 
			Color.FromArgbSystem (0xFF, 0x00, 0x00, 0x00, "MenuText", KnownColor.MenuText), 
			Color.FromArgbSystem (0xFF, 0xD4, 0xD0, 0xC8, "ScrollBar", KnownColor.ScrollBar), 
			Color.FromArgbSystem (0xFF, 0xFF, 0xFF, 0xFF, "Window", KnownColor.Window), 
			Color.FromArgbSystem (0xFF, 0x00, 0x00, 0x00, "WindowFrame", KnownColor.WindowFrame), 
			Color.FromArgbSystem (0xFF, 0x00, 0x00, 0x00, "WindowText", KnownColor.WindowText), 
			Color.FromArgbNamed (0x00, 0xFF, 0xFF, 0xFF, "Transparent", KnownColor.Transparent), 
			Color.FromArgbNamed (0xFF, 0xF0, 0xF8, 0xFF, "AliceBlue", KnownColor.AliceBlue), 
			Color.FromArgbNamed (0xFF, 0xFA, 0xEB, 0xD7, "AntiqueWhite", KnownColor.AntiqueWhite), 
			Color.FromArgbNamed (0xFF, 0x00, 0xFF, 0xFF, "Aqua", KnownColor.Aqua), 
			Color.FromArgbNamed (0xFF, 0x7F, 0xFF, 0xD4, "Aquamarine", KnownColor.Aquamarine), 
			Color.FromArgbNamed (0xFF, 0xF0, 0xFF, 0xFF, "Azure", KnownColor.Azure), 
			Color.FromArgbNamed (0xFF, 0xF5, 0xF5, 0xDC, "Beige", KnownColor.Beige), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xE4, 0xC4, "Bisque", KnownColor.Bisque), 
			Color.FromArgbNamed (0xFF, 0x00, 0x00, 0x00, "Black", KnownColor.Black), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xEB, 0xCD, "BlanchedAlmond", KnownColor.BlanchedAlmond), 
			Color.FromArgbNamed (0xFF, 0x00, 0x00, 0xFF, "Blue", KnownColor.Blue), 
			Color.FromArgbNamed (0xFF, 0x8A, 0x2B, 0xE2, "BlueViolet", KnownColor.BlueViolet), 
			Color.FromArgbNamed (0xFF, 0xA5, 0x2A, 0x2A, "Brown", KnownColor.Brown), 
			Color.FromArgbNamed (0xFF, 0xDE, 0xB8, 0x87, "BurlyWood", KnownColor.BurlyWood), 
			Color.FromArgbNamed (0xFF, 0x5F, 0x9E, 0xA0, "CadetBlue", KnownColor.CadetBlue), 
			Color.FromArgbNamed (0xFF, 0x7F, 0xFF, 0x00, "Chartreuse", KnownColor.Chartreuse), 
			Color.FromArgbNamed (0xFF, 0xD2, 0x69, 0x1E, "Chocolate", KnownColor.Chocolate), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x7F, 0x50, "Coral", KnownColor.Coral), 
			Color.FromArgbNamed (0xFF, 0x64, 0x95, 0xED, "CornflowerBlue", KnownColor.CornflowerBlue), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xF8, 0xDC, "Cornsilk", KnownColor.Cornsilk), 
			Color.FromArgbNamed (0xFF, 0xDC, 0x14, 0x3C, "Crimson", KnownColor.Crimson), 
			Color.FromArgbNamed (0xFF, 0x00, 0xFF, 0xFF, "Cyan", KnownColor.Cyan), 
			Color.FromArgbNamed (0xFF, 0x00, 0x00, 0x8B, "DarkBlue", KnownColor.DarkBlue), 
			Color.FromArgbNamed (0xFF, 0x00, 0x8B, 0x8B, "DarkCyan", KnownColor.DarkCyan), 
			Color.FromArgbNamed (0xFF, 0xB8, 0x86, 0x0B, "DarkGoldenrod", KnownColor.DarkGoldenrod), 
			Color.FromArgbNamed (0xFF, 0xA9, 0xA9, 0xA9, "DarkGray", KnownColor.DarkGray), 
			Color.FromArgbNamed (0xFF, 0x00, 0x64, 0x00, "DarkGreen", KnownColor.DarkGreen), 
			Color.FromArgbNamed (0xFF, 0xBD, 0xB7, 0x6B, "DarkKhaki", KnownColor.DarkKhaki), 
			Color.FromArgbNamed (0xFF, 0x8B, 0x00, 0x8B, "DarkMagenta", KnownColor.DarkMagenta), 
			Color.FromArgbNamed (0xFF, 0x55, 0x6B, 0x2F, "DarkOliveGreen", KnownColor.DarkOliveGreen), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x8C, 0x00, "DarkOrange", KnownColor.DarkOrange), 
			Color.FromArgbNamed (0xFF, 0x99, 0x32, 0xCC, "DarkOrchid", KnownColor.DarkOrchid), 
			Color.FromArgbNamed (0xFF, 0x8B, 0x00, 0x00, "DarkRed", KnownColor.DarkRed), 
			Color.FromArgbNamed (0xFF, 0xE9, 0x96, 0x7A, "DarkSalmon", KnownColor.DarkSalmon), 
			Color.FromArgbNamed (0xFF, 0x8F, 0xBC, 0x8B, "DarkSeaGreen", KnownColor.DarkSeaGreen), 
			Color.FromArgbNamed (0xFF, 0x48, 0x3D, 0x8B, "DarkSlateBlue", KnownColor.DarkSlateBlue), 
			Color.FromArgbNamed (0xFF, 0x2F, 0x4F, 0x4F, "DarkSlateGray", KnownColor.DarkSlateGray), 
			Color.FromArgbNamed (0xFF, 0x00, 0xCE, 0xD1, "DarkTurquoise", KnownColor.DarkTurquoise), 
			Color.FromArgbNamed (0xFF, 0x94, 0x00, 0xD3, "DarkViolet", KnownColor.DarkViolet), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x14, 0x93, "DeepPink", KnownColor.DeepPink), 
			Color.FromArgbNamed (0xFF, 0x00, 0xBF, 0xFF, "DeepSkyBlue", KnownColor.DeepSkyBlue), 
			Color.FromArgbNamed (0xFF, 0x69, 0x69, 0x69, "DimGray", KnownColor.DimGray), 
			Color.FromArgbNamed (0xFF, 0x1E, 0x90, 0xFF, "DodgerBlue", KnownColor.DodgerBlue), 
			Color.FromArgbNamed (0xFF, 0xB2, 0x22, 0x22, "Firebrick", KnownColor.Firebrick), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xFA, 0xF0, "FloralWhite", KnownColor.FloralWhite), 
			Color.FromArgbNamed (0xFF, 0x22, 0x8B, 0x22, "ForestGreen", KnownColor.ForestGreen), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x00, 0xFF, "Fuchsia", KnownColor.Fuchsia), 
			Color.FromArgbNamed (0xFF, 0xDC, 0xDC, 0xDC, "Gainsboro", KnownColor.Gainsboro), 
			Color.FromArgbNamed (0xFF, 0xF8, 0xF8, 0xFF, "GhostWhite", KnownColor.GhostWhite), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xD7, 0x00, "Gold", KnownColor.Gold), 
			Color.FromArgbNamed (0xFF, 0xDA, 0xA5, 0x20, "Goldenrod", KnownColor.Goldenrod), 
			Color.FromArgbNamed (0xFF, 0x80, 0x80, 0x80, "Gray", KnownColor.Gray), 
			Color.FromArgbNamed (0xFF, 0x00, 0x80, 0x00, "Green", KnownColor.Green), 
			Color.FromArgbNamed (0xFF, 0xAD, 0xFF, 0x2F, "GreenYellow", KnownColor.GreenYellow), 
			Color.FromArgbNamed (0xFF, 0xF0, 0xFF, 0xF0, "Honeydew", KnownColor.Honeydew), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x69, 0xB4, "HotPink", KnownColor.HotPink), 
			Color.FromArgbNamed (0xFF, 0xCD, 0x5C, 0x5C, "IndianRed", KnownColor.IndianRed), 
			Color.FromArgbNamed (0xFF, 0x4B, 0x00, 0x82, "Indigo", KnownColor.Indigo), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xFF, 0xF0, "Ivory", KnownColor.Ivory), 
			Color.FromArgbNamed (0xFF, 0xF0, 0xE6, 0x8C, "Khaki", KnownColor.Khaki), 
			Color.FromArgbNamed (0xFF, 0xE6, 0xE6, 0xFA, "Lavender", KnownColor.Lavender), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xF0, 0xF5, "LavenderBlush", KnownColor.LavenderBlush), 
			Color.FromArgbNamed (0xFF, 0x7C, 0xFC, 0x00, "LawnGreen", KnownColor.LawnGreen), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xFA, 0xCD, "LemonChiffon", KnownColor.LemonChiffon), 
			Color.FromArgbNamed (0xFF, 0xAD, 0xD8, 0xE6, "LightBlue", KnownColor.LightBlue), 
			Color.FromArgbNamed (0xFF, 0xF0, 0x80, 0x80, "LightCoral", KnownColor.LightCoral), 
			Color.FromArgbNamed (0xFF, 0xE0, 0xFF, 0xFF, "LightCyan", KnownColor.LightCyan), 
			Color.FromArgbNamed (0xFF, 0xFA, 0xFA, 0xD2, "LightGoldenrodYellow", KnownColor.LightGoldenrodYellow), 
			Color.FromArgbNamed (0xFF, 0xD3, 0xD3, 0xD3, "LightGray", KnownColor.LightGray), 
			Color.FromArgbNamed (0xFF, 0x90, 0xEE, 0x90, "LightGreen", KnownColor.LightGreen), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xB6, 0xC1, "LightPink", KnownColor.LightPink), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xA0, 0x7A, "LightSalmon", KnownColor.LightSalmon), 
			Color.FromArgbNamed (0xFF, 0x20, 0xB2, 0xAA, "LightSeaGreen", KnownColor.LightSeaGreen), 
			Color.FromArgbNamed (0xFF, 0x87, 0xCE, 0xFA, "LightSkyBlue", KnownColor.LightSkyBlue), 
			Color.FromArgbNamed (0xFF, 0x77, 0x88, 0x99, "LightSlateGray", KnownColor.LightSlateGray), 
			Color.FromArgbNamed (0xFF, 0xB0, 0xC4, 0xDE, "LightSteelBlue", KnownColor.LightSteelBlue), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xFF, 0xE0, "LightYellow", KnownColor.LightYellow), 
			Color.FromArgbNamed (0xFF, 0x00, 0xFF, 0x00, "Lime", KnownColor.Lime), 
			Color.FromArgbNamed (0xFF, 0x32, 0xCD, 0x32, "LimeGreen", KnownColor.LimeGreen), 
			Color.FromArgbNamed (0xFF, 0xFA, 0xF0, 0xE6, "Linen", KnownColor.Linen), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x00, 0xFF, "Magenta", KnownColor.Magenta), 
			Color.FromArgbNamed (0xFF, 0x80, 0x00, 0x00, "Maroon", KnownColor.Maroon), 
			Color.FromArgbNamed (0xFF, 0x66, 0xCD, 0xAA, "MediumAquamarine", KnownColor.MediumAquamarine), 
			Color.FromArgbNamed (0xFF, 0x00, 0x00, 0xCD, "MediumBlue", KnownColor.MediumBlue), 
			Color.FromArgbNamed (0xFF, 0xBA, 0x55, 0xD3, "MediumOrchid", KnownColor.MediumOrchid), 
			Color.FromArgbNamed (0xFF, 0x93, 0x70, 0xDB, "MediumPurple", KnownColor.MediumPurple), 
			Color.FromArgbNamed (0xFF, 0x3C, 0xB3, 0x71, "MediumSeaGreen", KnownColor.MediumSeaGreen), 
			Color.FromArgbNamed (0xFF, 0x7B, 0x68, 0xEE, "MediumSlateBlue", KnownColor.MediumSlateBlue), 
			Color.FromArgbNamed (0xFF, 0x00, 0xFA, 0x9A, "MediumSpringGreen", KnownColor.MediumSpringGreen), 
			Color.FromArgbNamed (0xFF, 0x48, 0xD1, 0xCC, "MediumTurquoise", KnownColor.MediumTurquoise), 
			Color.FromArgbNamed (0xFF, 0xC7, 0x15, 0x85, "MediumVioletRed", KnownColor.MediumVioletRed), 
			Color.FromArgbNamed (0xFF, 0x19, 0x19, 0x70, "MidnightBlue", KnownColor.MidnightBlue), 
			Color.FromArgbNamed (0xFF, 0xF5, 0xFF, 0xFA, "MintCream", KnownColor.MintCream), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xE4, 0xE1, "MistyRose", KnownColor.MistyRose), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xE4, 0xB5, "Moccasin", KnownColor.Moccasin), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xDE, 0xAD, "NavajoWhite", KnownColor.NavajoWhite), 
			Color.FromArgbNamed (0xFF, 0x00, 0x00, 0x80, "Navy", KnownColor.Navy), 
			Color.FromArgbNamed (0xFF, 0xFD, 0xF5, 0xE6, "OldLace", KnownColor.OldLace), 
			Color.FromArgbNamed (0xFF, 0x80, 0x80, 0x00, "Olive", KnownColor.Olive), 
			Color.FromArgbNamed (0xFF, 0x6B, 0x8E, 0x23, "OliveDrab", KnownColor.OliveDrab), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xA5, 0x00, "Orange", KnownColor.Orange), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x45, 0x00, "OrangeRed", KnownColor.OrangeRed), 
			Color.FromArgbNamed (0xFF, 0xDA, 0x70, 0xD6, "Orchid", KnownColor.Orchid), 
			Color.FromArgbNamed (0xFF, 0xEE, 0xE8, 0xAA, "PaleGoldenrod", KnownColor.PaleGoldenrod), 
			Color.FromArgbNamed (0xFF, 0x98, 0xFB, 0x98, "PaleGreen", KnownColor.PaleGreen), 
			Color.FromArgbNamed (0xFF, 0xAF, 0xEE, 0xEE, "PaleTurquoise", KnownColor.PaleTurquoise), 
			Color.FromArgbNamed (0xFF, 0xDB, 0x70, 0x93, "PaleVioletRed", KnownColor.PaleVioletRed), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xEF, 0xD5, "PapayaWhip", KnownColor.PapayaWhip), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xDA, 0xB9, "PeachPuff", KnownColor.PeachPuff), 
			Color.FromArgbNamed (0xFF, 0xCD, 0x85, 0x3F, "Peru", KnownColor.Peru), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xC0, 0xCB, "Pink", KnownColor.Pink), 
			Color.FromArgbNamed (0xFF, 0xDD, 0xA0, 0xDD, "Plum", KnownColor.Plum), 
			Color.FromArgbNamed (0xFF, 0xB0, 0xE0, 0xE6, "PowderBlue", KnownColor.PowderBlue), 
			Color.FromArgbNamed (0xFF, 0x80, 0x00, 0x80, "Purple", KnownColor.Purple), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x00, 0x00, "Red", KnownColor.Red), 
			Color.FromArgbNamed (0xFF, 0xBC, 0x8F, 0x8F, "RosyBrown", KnownColor.RosyBrown), 
			Color.FromArgbNamed (0xFF, 0x41, 0x69, 0xE1, "RoyalBlue", KnownColor.RoyalBlue), 
			Color.FromArgbNamed (0xFF, 0x8B, 0x45, 0x13, "SaddleBrown", KnownColor.SaddleBrown), 
			Color.FromArgbNamed (0xFF, 0xFA, 0x80, 0x72, "Salmon", KnownColor.Salmon), 
			Color.FromArgbNamed (0xFF, 0xF4, 0xA4, 0x60, "SandyBrown", KnownColor.SandyBrown), 
			Color.FromArgbNamed (0xFF, 0x2E, 0x8B, 0x57, "SeaGreen", KnownColor.SeaGreen), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xF5, 0xEE, "SeaShell", KnownColor.SeaShell), 
			Color.FromArgbNamed (0xFF, 0xA0, 0x52, 0x2D, "Sienna", KnownColor.Sienna), 
			Color.FromArgbNamed (0xFF, 0xC0, 0xC0, 0xC0, "Silver", KnownColor.Silver), 
			Color.FromArgbNamed (0xFF, 0x87, 0xCE, 0xEB, "SkyBlue", KnownColor.SkyBlue), 
			Color.FromArgbNamed (0xFF, 0x6A, 0x5A, 0xCD, "SlateBlue", KnownColor.SlateBlue), 
			Color.FromArgbNamed (0xFF, 0x70, 0x80, 0x90, "SlateGray", KnownColor.SlateGray), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xFA, 0xFA, "Snow", KnownColor.Snow), 
			Color.FromArgbNamed (0xFF, 0x00, 0xFF, 0x7F, "SpringGreen", KnownColor.SpringGreen), 
			Color.FromArgbNamed (0xFF, 0x46, 0x82, 0xB4, "SteelBlue", KnownColor.SteelBlue), 
			Color.FromArgbNamed (0xFF, 0xD2, 0xB4, 0x8C, "Tan", KnownColor.Tan), 
			Color.FromArgbNamed (0xFF, 0x00, 0x80, 0x80, "Teal", KnownColor.Teal), 
			Color.FromArgbNamed (0xFF, 0xD8, 0xBF, 0xD8, "Thistle", KnownColor.Thistle), 
			Color.FromArgbNamed (0xFF, 0xFF, 0x63, 0x47, "Tomato", KnownColor.Tomato), 
			Color.FromArgbNamed (0xFF, 0x40, 0xE0, 0xD0, "Turquoise", KnownColor.Turquoise), 
			Color.FromArgbNamed (0xFF, 0xEE, 0x82, 0xEE, "Violet", KnownColor.Violet), 
			Color.FromArgbNamed (0xFF, 0xF5, 0xDE, 0xB3, "Wheat", KnownColor.Wheat), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xFF, 0xFF, "White", KnownColor.White), 
			Color.FromArgbNamed (0xFF, 0xF5, 0xF5, 0xF5, "WhiteSmoke", KnownColor.WhiteSmoke), 
			Color.FromArgbNamed (0xFF, 0xFF, 0xFF, 0x00, "Yellow", KnownColor.Yellow), 
			Color.FromArgbNamed (0xFF, 0x9A, 0xCD, 0x32, "YellowGreen", KnownColor.YellowGreen), 
#if NET_2_0
			Color.FromArgbSystem (0xFF, 0xEC, 0xE9, 0xD8, "ButtonFace", KnownColor.ButtonFace), 
			Color.FromArgbSystem (0xFF, 0xFF, 0xFF, 0xFF, "ButtonHighlight", KnownColor.ButtonHighlight), 
			Color.FromArgbSystem (0xFF, 0xAC, 0xA8, 0x99, "ButtonShadow", KnownColor.ButtonShadow), 
			Color.FromArgbSystem (0xFF, 0x3D, 0x95, 0xFF, "GradientActiveCaption", KnownColor.GradientActiveCaption), 
			Color.FromArgbSystem (0xFF, 0x9D, 0xB9, 0xEB, "GradientInactiveCaption", KnownColor.GradientInactiveCaption), 
			Color.FromArgbSystem (0xFF, 0xEC, 0xE9, 0xD8, "MenuBar", KnownColor.MenuBar), 
			Color.FromArgbSystem (0xFF, 0x31, 0x6A, 0xC5, "MenuHighlight", KnownColor.MenuHighlight), 
#endif
		};

		static KnownColors ()
		{
			if (GDIPlus.RunningOnWindows ()) {
				// If we're on Windows we should behave like MS and pull the colors
				InitOnWindows ();
			}
		}

		private static void SetWindowsColor (GetSysColorIndex index, string name, KnownColor knownColor)
		{
			uint rgb = GDIPlus.Win32GetSysColor (index);
			Color color = Color.FromArgbSystem (255, (byte)rgb, (byte)(rgb >> 8), (byte)(rgb >> 16), name, knownColor);
			knownColors [(int)knownColor] = color;
		}

		static void InitOnWindows ()
		{
			SetWindowsColor (GetSysColorIndex.COLOR_ACTIVEBORDER, "ActiveBorder", KnownColor.ActiveBorder);
			SetWindowsColor (GetSysColorIndex.COLOR_ACTIVECAPTION, "ActiveCaption", KnownColor.ActiveCaption);
			SetWindowsColor (GetSysColorIndex.COLOR_CAPTIONTEXT, "ActiveCaptionText", KnownColor.ActiveCaptionText);
			SetWindowsColor (GetSysColorIndex.COLOR_APPWORKSPACE, "AppWorkspace", KnownColor.AppWorkspace);
			SetWindowsColor (GetSysColorIndex.COLOR_BTNFACE, "Control", KnownColor.Control);
			SetWindowsColor (GetSysColorIndex.COLOR_BTNSHADOW, "ControlDark", KnownColor.ControlDark);
			SetWindowsColor (GetSysColorIndex.COLOR_3DDKSHADOW, "ControlDarkDark", KnownColor.ControlDarkDark);
			SetWindowsColor (GetSysColorIndex.COLOR_3DLIGHT, "ControlLight", KnownColor.ControlLight);
			SetWindowsColor (GetSysColorIndex.COLOR_BTNHIGHLIGHT, "ControlLightLight", KnownColor.ControlLightLight);
			SetWindowsColor (GetSysColorIndex.COLOR_BTNTEXT, "ControlText", KnownColor.ControlText);
			SetWindowsColor (GetSysColorIndex.COLOR_DESKTOP, "Desktop", KnownColor.Desktop);
			SetWindowsColor (GetSysColorIndex.COLOR_GRAYTEXT, "GrayText", KnownColor.GrayText);
			SetWindowsColor (GetSysColorIndex.COLOR_HIGHLIGHT, "Highlight", KnownColor.Highlight);
			SetWindowsColor (GetSysColorIndex.COLOR_HIGHLIGHTTEXT, "HighlightText", KnownColor.HighlightText);
			SetWindowsColor (GetSysColorIndex.COLOR_HOTLIGHT, "HotTrack", KnownColor.HotTrack);
			SetWindowsColor (GetSysColorIndex.COLOR_INACTIVEBORDER, "InactiveBorder", KnownColor.InactiveBorder);
			SetWindowsColor (GetSysColorIndex.COLOR_INACTIVECAPTION, "InactiveCaption", KnownColor.InactiveCaption);
			SetWindowsColor (GetSysColorIndex.COLOR_INACTIVECAPTIONTEXT, "InactiveCaptionText", KnownColor.InactiveCaptionText);
			SetWindowsColor (GetSysColorIndex.COLOR_INFOBK, "Info", KnownColor.Info);
			SetWindowsColor (GetSysColorIndex.COLOR_INFOTEXT, "InfoText", KnownColor.InfoText);
			SetWindowsColor (GetSysColorIndex.COLOR_MENU, "Menu", KnownColor.Menu);
			SetWindowsColor (GetSysColorIndex.COLOR_MENUTEXT, "MenuText", KnownColor.MenuText);
			SetWindowsColor (GetSysColorIndex.COLOR_SCROLLBAR, "ScrollBar", KnownColor.ScrollBar);
			SetWindowsColor (GetSysColorIndex.COLOR_WINDOW, "Window", KnownColor.Window);
			SetWindowsColor (GetSysColorIndex.COLOR_WINDOWFRAME, "WindowFrame", KnownColor.WindowFrame);
			SetWindowsColor (GetSysColorIndex.COLOR_WINDOWTEXT, "WindowText", KnownColor.WindowText);
#if NET_2_0
			SetWindowsColor (GetSysColorIndex.COLOR_BTNFACE, "ButtonFace", KnownColor.ButtonFace);
			SetWindowsColor (GetSysColorIndex.COLOR_BTNHIGHLIGHT, "ButtonHighlight", KnownColor.ButtonHighlight);
			SetWindowsColor (GetSysColorIndex.COLOR_BTNSHADOW, "ButtonShadow", KnownColor.ButtonShadow);
			SetWindowsColor (GetSysColorIndex.COLOR_GRADIENTACTIVECAPTION, "GradientActiveCaption", KnownColor.GradientActiveCaption);
			SetWindowsColor (GetSysColorIndex.COLOR_GRADIENTINACTIVECAPTION, "GradientInactiveCaption", KnownColor.GradientInactiveCaption);
			SetWindowsColor (GetSysColorIndex.COLOR_MENUBAR, "MenuBar", KnownColor.MenuBar);
			SetWindowsColor (GetSysColorIndex.COLOR_MENUHIGHLIGHT, "MenuHighlight", KnownColor.MenuHighlight);
#endif
		}

		public static Color[] Values {
			get { return knownColors; }
		}

		public static Color FromKnownColor (KnownColor kc)
		{
			int n = (int)kc;
			if ((n <= 0) || (n >= knownColors.Length)) {
				// This is what it returns!
				Color c = Color.FromArgb (0, 0, 0, 0);
				c.name = kc.ToString ();
				c.state |= (short) Color.ColorType.Named;
				c.knownColor = (short) kc;
				return c;
			}
			return knownColors [n];
		}

		// When this method is called, we teach any new color(s) to the Color class
		// NOTE: This is called (reflection) by System.Windows.Forms.Theme (this isn't dead code)
		public static Color Update (int knownColor, int color)
		{
			Color c = knownColors[knownColor];
			c.state = (short) (Color.ColorType.ARGB | Color.ColorType.Known | Color.ColorType.Named | Color.ColorType.System);
			c.value = color;
			knownColors[knownColor] = c;
			return c;
		}
	}
}
