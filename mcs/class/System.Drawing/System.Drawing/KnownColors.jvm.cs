//
// System.Drawing.KnownColors.cs
//
// Authors:
//  Konstantin Triger (kostat@mainsoft.com)
//
// Copyright (C) 2007 Mainsoft Corporation (http://www.mainsoft.com)
// 

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

using awt = java.awt;

namespace System.Drawing
{
	sealed class KnownColors
	{
		static readonly Color [] SystemColorsCache;

		KnownColors () { }

		static KnownColors ()
		{
#if !NET_2_0
			SystemColorsCache = new Color[27]; //enough for all system color indices
#else
			SystemColorsCache = new Color [175]; //enough for all system color indices
#endif

			if (Graphics.IsHeadless) {
				SystemColorsCache[(int)KnownColor.ActiveBorder] = Color.FromArgbSystem (255, 212, 208, 200, "ActiveBorder", KnownColor.ActiveBorder);
				SystemColorsCache[(int)KnownColor.ActiveCaption] = Color.FromArgbSystem (255, 10, 36, 106, "ActiveCaption", KnownColor.ActiveCaption);
				SystemColorsCache[(int)KnownColor.ActiveCaptionText] = Color.FromArgbSystem (255, 255, 255, 255, "ActiveCaptionText", KnownColor.ActiveCaptionText);
				//TODO: find a more close approximation
				SystemColorsCache[(int)KnownColor.AppWorkspace] = Color.FromArgbSystem (255, 128, 128, 128, "AppWorkspace", KnownColor.AppWorkspace);
				SystemColorsCache[(int)KnownColor.Control] = Color.FromArgbSystem (255, 212, 208, 200, "Control", KnownColor.Control);
				SystemColorsCache[(int)KnownColor.ControlDark] = Color.FromArgbSystem (255, 128, 128, 128, "ControlDark", KnownColor.ControlDark);
				SystemColorsCache[(int)KnownColor.ControlDarkDark] = Color.FromArgbSystem (255, 64, 64, 64, "ControlDarkDark", KnownColor.ControlDarkDark);
				SystemColorsCache[(int)KnownColor.ControlLight] = Color.FromArgbSystem (255, 212, 208, 200, "ControlLight", KnownColor.ControlLight);
				SystemColorsCache[(int)KnownColor.ControlLightLight] = Color.FromArgbSystem (255, 255, 255, 255, "ControlLightLight", KnownColor.ControlLightLight);
				SystemColorsCache[(int)KnownColor.ControlText] = Color.FromArgbSystem (255, 0, 0, 0, "ControlText", KnownColor.ControlText);
				SystemColorsCache[(int)KnownColor.Desktop] = Color.FromArgbSystem (255, 58, 110, 165, "Desktop", KnownColor.Desktop);
				SystemColorsCache[(int)KnownColor.GrayText] = Color.FromArgbSystem (255, 128, 128, 128, "GrayText", KnownColor.GrayText);
				SystemColorsCache[(int)KnownColor.Highlight] = Color.FromArgbSystem (255, 10, 36, 106, "Highlight", KnownColor.Highlight);
				SystemColorsCache[(int)KnownColor.HighlightText] = Color.FromArgbSystem (255, 255, 255, 255, "HighlightText", KnownColor.HighlightText);
				//TODO: find a more close approximation
				SystemColorsCache[(int)KnownColor.HotTrack] = Color.FromArgbSystem (255, 0, 0, 128, "HotTrack", KnownColor.HotTrack);
				SystemColorsCache[(int)KnownColor.InactiveBorder] = Color.FromArgbSystem (255, 212, 208, 200, "InactiveBorder", KnownColor.InactiveBorder);
				SystemColorsCache[(int)KnownColor.InactiveCaption] = Color.FromArgbSystem (255, 128, 128, 128, "InactiveCaption", KnownColor.InactiveCaption);
				SystemColorsCache[(int)KnownColor.InactiveCaptionText] = Color.FromArgbSystem (255, 212, 208, 200, "InactiveCaptionText", KnownColor.InactiveCaptionText);
				SystemColorsCache[(int)KnownColor.Info] = Color.FromArgbSystem (255, 255, 255, 225, "Info", KnownColor.Info);
				SystemColorsCache[(int)KnownColor.InfoText] = Color.FromArgbSystem (255, 0, 0, 0, "InfoText", KnownColor.InfoText);
				SystemColorsCache[(int)KnownColor.Menu] = Color.FromArgbSystem (255, 212, 208, 200, "Menu", KnownColor.Menu);
				SystemColorsCache[(int)KnownColor.MenuText] = Color.FromArgbSystem (255, 0, 0, 0, "MenuText", KnownColor.MenuText);
				SystemColorsCache[(int)KnownColor.ScrollBar] = Color.FromArgbSystem (255, 212, 208, 200, "ScrollBar", KnownColor.ScrollBar);
				SystemColorsCache[(int)KnownColor.Window] = Color.FromArgbSystem (255, 255, 255, 255, "Window", KnownColor.Window);
				SystemColorsCache[(int)KnownColor.WindowFrame] = Color.FromArgbSystem (255, 0, 0, 0, "WindowFrame", KnownColor.WindowFrame);
				SystemColorsCache[(int)KnownColor.WindowText] = Color.FromArgbSystem (255, 0, 0, 0, "WindowText", KnownColor.WindowText);

#if NET_2_0
				SystemColorsCache [(int) KnownColor.ButtonFace] = Color.FromArgbSystem (255, 192, 192, 192, "ButtonFace", KnownColor.ButtonFace);
				SystemColorsCache [(int) KnownColor.ButtonHighlight] = Color.FromArgbSystem (255, 255, 255, 255, "ButtonHighlight", KnownColor.ButtonHighlight);
				SystemColorsCache [(int) KnownColor.ButtonShadow] = Color.FromArgbSystem (255, 128, 128, 128, "ButtonShadow", KnownColor.ButtonShadow);
				SystemColorsCache [(int) KnownColor.GradientActiveCaption] = Color.FromArgbSystem (255, 16, 132, 208, "GradientActiveCaption", KnownColor.GradientActiveCaption);
				SystemColorsCache [(int) KnownColor.GradientInactiveCaption] = Color.FromArgbSystem (255, 181, 181, 181, "GradientInactiveCaption", KnownColor.GradientInactiveCaption);
				SystemColorsCache [(int) KnownColor.MenuBar] = Color.FromArgbSystem (255, 192, 192, 192, "MenuBar", KnownColor.MenuBar);
				SystemColorsCache [(int) KnownColor.MenuHighlight] = Color.FromArgbSystem (255, 0, 0, 0, "MenuHighlight", KnownColor.MenuHighlight);
#endif
			}
			else {
				SystemColorsCache[(int)KnownColor.ActiveBorder] = FromNativeColor (awt.SystemColor.activeCaptionBorder, "ActiveBorder", KnownColor.ActiveBorder);
				SystemColorsCache[(int)KnownColor.ActiveCaption] = FromNativeColor (awt.SystemColor.activeCaption, "ActiveCaption", KnownColor.ActiveCaption);
				SystemColorsCache[(int)KnownColor.ActiveCaptionText] = FromNativeColor (awt.SystemColor.activeCaptionText, "ActiveCaptionText", KnownColor.ActiveCaptionText);
				//TODO: find a more close approximation
				SystemColorsCache[(int)KnownColor.AppWorkspace] = FromNativeColor (awt.SystemColor.desktop, "AppWorkspace", KnownColor.AppWorkspace);
				SystemColorsCache[(int)KnownColor.Control] = FromNativeColor (awt.SystemColor.control, "Control", KnownColor.Control);
				SystemColorsCache[(int)KnownColor.ControlDark] = FromNativeColor (awt.SystemColor.controlShadow, "ControlDark", KnownColor.ControlDark);
				SystemColorsCache[(int)KnownColor.ControlDarkDark] = FromNativeColor (awt.SystemColor.controlDkShadow, "ControlDarkDark", KnownColor.ControlDarkDark);
				SystemColorsCache[(int)KnownColor.ControlLight] = FromNativeColor (awt.SystemColor.controlHighlight, "ControlLight", KnownColor.ControlLight);
				SystemColorsCache[(int)KnownColor.ControlLightLight] = FromNativeColor (awt.SystemColor.controlLtHighlight, "ControlLightLight", KnownColor.ControlLightLight);
				SystemColorsCache[(int)KnownColor.ControlText] = FromNativeColor (awt.SystemColor.controlText, "ControlText", KnownColor.ControlText);
				SystemColorsCache[(int)KnownColor.Desktop] = FromNativeColor (awt.SystemColor.desktop, "Desktop", KnownColor.Desktop);
				SystemColorsCache[(int)KnownColor.GrayText] = FromNativeColor (awt.SystemColor.textInactiveText, "GrayText", KnownColor.GrayText);
				SystemColorsCache[(int)KnownColor.Highlight] = FromNativeColor (awt.SystemColor.textHighlight, "Highlight", KnownColor.Highlight);
				SystemColorsCache[(int)KnownColor.HighlightText] = FromNativeColor (awt.SystemColor.textHighlightText, "HighlightText", KnownColor.HighlightText);
				//TODO: find a more close approximation
				SystemColorsCache[(int)KnownColor.HotTrack] = FromNativeColor (awt.SystemColor.textText, "HotTrack", KnownColor.HotTrack);
				SystemColorsCache[(int)KnownColor.InactiveBorder] = FromNativeColor (awt.SystemColor.inactiveCaptionBorder, "InactiveBorder", KnownColor.InactiveBorder);
				SystemColorsCache[(int)KnownColor.InactiveCaption] = FromNativeColor (awt.SystemColor.inactiveCaption, "InactiveCaption", KnownColor.InactiveCaption);
				SystemColorsCache[(int)KnownColor.InactiveCaptionText] = FromNativeColor (awt.SystemColor.inactiveCaptionText, "InactiveCaptionText", KnownColor.InactiveCaptionText);
				SystemColorsCache[(int)KnownColor.Info] = FromNativeColor (awt.SystemColor.info, "Info", KnownColor.Info);
				SystemColorsCache[(int)KnownColor.InfoText] = FromNativeColor (awt.SystemColor.infoText, "InfoText", KnownColor.InfoText);
				SystemColorsCache[(int)KnownColor.Menu] = FromNativeColor (awt.SystemColor.menu, "Menu", KnownColor.Menu);
				SystemColorsCache[(int)KnownColor.MenuText] = FromNativeColor (awt.SystemColor.menuText, "MenuText", KnownColor.MenuText);
				SystemColorsCache[(int)KnownColor.ScrollBar] = FromNativeColor (awt.SystemColor.scrollbar, "ScrollBar", KnownColor.ScrollBar);
				SystemColorsCache[(int)KnownColor.Window] = FromNativeColor (awt.SystemColor.window, "Window", KnownColor.Window);
				SystemColorsCache[(int)KnownColor.WindowFrame] = FromNativeColor (awt.SystemColor.windowBorder, "WindowFrame", KnownColor.WindowFrame);
				SystemColorsCache[(int)KnownColor.WindowText] = FromNativeColor (awt.SystemColor.windowText, "WindowText", KnownColor.WindowText);

#if NET_2_0
				SystemColorsCache [(int) KnownColor.ButtonFace] = FromNativeColor (awt.SystemColor.control, "ButtonFace", KnownColor.ButtonFace);
				SystemColorsCache [(int) KnownColor.ButtonHighlight] = FromNativeColor (awt.SystemColor.controlHighlight, "ButtonHighlight", KnownColor.ButtonHighlight);
				SystemColorsCache [(int) KnownColor.ButtonShadow] = FromNativeColor (awt.SystemColor.controlShadow, "ButtonShadow", KnownColor.ButtonShadow);
				SystemColorsCache [(int) KnownColor.GradientActiveCaption] = FromNativeColor (awt.SystemColor.activeCaption, "GradientActiveCaption", KnownColor.GradientActiveCaption);
				SystemColorsCache [(int) KnownColor.GradientInactiveCaption] = FromNativeColor (awt.SystemColor.inactiveCaption, "GradientInactiveCaption", KnownColor.GradientInactiveCaption);
				SystemColorsCache [(int) KnownColor.MenuBar] = FromNativeColor (awt.SystemColor.menu, "MenuBar", KnownColor.MenuBar);
				SystemColorsCache [(int) KnownColor.MenuHighlight] = FromNativeColor (awt.SystemColor.menu, "MenuHighlight", KnownColor.MenuHighlight);
#endif
			}

			Color [] namedColors = new Color [] {
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
			};

			Array.Copy (namedColors, 0, SystemColorsCache, (int) KnownColor.Transparent, namedColors.Length);
		}

		static private Color FromNativeColor (
			awt.Color awtColor, string name, KnownColor knownColor) {
			return Color.FromArgbSystem (awtColor.getAlpha(),
				awtColor.getRed(), awtColor.getGreen(), awtColor.getBlue(),
				name, knownColor);
		}

		public static Color [] Values {
			get { return SystemColorsCache; }
		}

		public static Color FromKnownColor (KnownColor kc) {
			int n = (int) kc;
			if ((n <= 0) || (n >= Values.Length)) {
				// This is what it returns!
				Color c = Color.FromArgb (0, 0, 0, 0);
				c.name = kc.ToString ();
				c.state |= (short) Color.ColorType.Named;
				c.knownColor = (short) kc;
				return c;
			}
			return Values [n];
		}

				public static string GetName (short kc)
		{
			switch (kc) {
			case 1:		return "ActiveBorder";
			case 2:		return "ActiveCaption";
			case 3:		return "ActiveCaptionText";
			case 4:		return "AppWorkspace";
			case 5:		return "Control";
			case 6:		return "ControlDark";
			case 7:		return "ControlDarkDark";
			case 8:		return "ControlLight";
			case 9:		return "ControlLightLight";
			case 10:	return "ControlText";
			case 11:	return "Desktop";
			case 12:	return "GrayText";
			case 13:	return "Highlight";
			case 14:	return "HighlightText";
			case 15:	return "HotTrack";
			case 16:	return "InactiveBorder";
			case 17:	return "InactiveCaption";
			case 18:	return "InactiveCaptionText";
			case 19:	return "Info";
			case 20:	return "InfoText";
			case 21:	return "Menu";
			case 22:	return "MenuText";
			case 23:	return "ScrollBar";
			case 24:	return "Window";
			case 25:	return "WindowFrame";
			case 26:	return "WindowText";
			case 27:	return "Transparent";
			case 28:	return "AliceBlue";
			case 29:	return "AntiqueWhite";
			case 30:	return "Aqua";
			case 31:	return "Aquamarine";
			case 32:	return "Azure";
			case 33:	return "Beige";
			case 34:	return "Bisque";
			case 35:	return "Black";
			case 36:	return "BlanchedAlmond";
			case 37:	return "Blue";
			case 38:	return "BlueViolet";
			case 39:	return "Brown";
			case 40:	return "BurlyWood";
			case 41:	return "CadetBlue";
			case 42:	return "Chartreuse";
			case 43:	return "Chocolate";
			case 44:	return "Coral";
			case 45:	return "CornflowerBlue";
			case 46:	return "Cornsilk";
			case 47:	return "Crimson";
			case 48:	return "Cyan";
			case 49:	return "DarkBlue";
			case 50:	return "DarkCyan";
			case 51:	return "DarkGoldenrod";
			case 52:	return "DarkGray";
			case 53:	return "DarkGreen";
			case 54:	return "DarkKhaki";
			case 55:	return "DarkMagenta";
			case 56:	return "DarkOliveGreen";
			case 57:	return "DarkOrange";
			case 58:	return "DarkOrchid";
			case 59:	return "DarkRed";
			case 60:	return "DarkSalmon";
			case 61:	return "DarkSeaGreen";
			case 62:	return "DarkSlateBlue";
			case 63:	return "DarkSlateGray";
			case 64:	return "DarkTurquoise";
			case 65:	return "DarkViolet";
			case 66:	return "DeepPink";
			case 67:	return "DeepSkyBlue";
			case 68:	return "DimGray";
			case 69:	return "DodgerBlue";
			case 70:	return "Firebrick";
			case 71:	return "FloralWhite";
			case 72:	return "ForestGreen";
			case 73:	return "Fuchsia";
			case 74:	return "Gainsboro";
			case 75:	return "GhostWhite";
			case 76:	return "Gold";
			case 77:	return "Goldenrod";
			case 78:	return "Gray";
			case 79:	return "Green";
			case 80:	return "GreenYellow";
			case 81:	return "Honeydew";
			case 82:	return "HotPink";
			case 83:	return "IndianRed";
			case 84:	return "Indigo";
			case 85:	return "Ivory";
			case 86:	return "Khaki";
			case 87:	return "Lavender";
			case 88:	return "LavenderBlush";
			case 89:	return "LawnGreen";
			case 90:	return "LemonChiffon";
			case 91:	return "LightBlue";
			case 92:	return "LightCoral";
			case 93:	return "LightCyan";
			case 94:	return "LightGoldenrodYellow";
			case 95:	return "LightGray";
			case 96:	return "LightGreen";
			case 97:	return "LightPink";
			case 98:	return "LightSalmon";
			case 99:	return "LightSeaGreen";
			case 100:	return "LightSkyBlue";
			case 101:	return "LightSlateGray";
			case 102:	return "LightSteelBlue";
			case 103:	return "LightYellow";
			case 104:	return "Lime";
			case 105:	return "LimeGreen";
			case 106:	return "Linen";
			case 107:	return "Magenta";
			case 108:	return "Maroon";
			case 109:	return "MediumAquamarine";
			case 110:	return "MediumBlue";
			case 111:	return "MediumOrchid";
			case 112:	return "MediumPurple";
			case 113:	return "MediumSeaGreen";
			case 114:	return "MediumSlateBlue";
			case 115:	return "MediumSpringGreen";
			case 116:	return "MediumTurquoise";
			case 117:	return "MediumVioletRed";
			case 118:	return "MidnightBlue";
			case 119:	return "MintCream";
			case 120:	return "MistyRose";
			case 121:	return "Moccasin";
			case 122:	return "NavajoWhite";
			case 123:	return "Navy";
			case 124:	return "OldLace";
			case 125:	return "Olive";
			case 126:	return "OliveDrab";
			case 127:	return "Orange";
			case 128:	return "OrangeRed";
			case 129:	return "Orchid";
			case 130:	return "PaleGoldenrod";
			case 131:	return "PaleGreen";
			case 132:	return "PaleTurquoise";
			case 133:	return "PaleVioletRed";
			case 134:	return "PapayaWhip";
			case 135:	return "PeachPuff";
			case 136:	return "Peru";
			case 137:	return "Pink";
			case 138:	return "Plum";
			case 139:	return "PowderBlue";
			case 140:	return "Purple";
			case 141:	return "Red";
			case 142:	return "RosyBrown";
			case 143:	return "RoyalBlue";
			case 144:	return "SaddleBrown";
			case 145:	return "Salmon";
			case 146:	return "SandyBrown";
			case 147:	return "SeaGreen";
			case 148:	return "SeaShell";
			case 149:	return "Sienna";
			case 150:	return "Silver";
			case 151:	return "SkyBlue";
			case 152:	return "SlateBlue";
			case 153:	return "SlateGray";
			case 154:	return "Snow";
			case 155:	return "SpringGreen";
			case 156:	return "SteelBlue";
			case 157:	return "Tan";
			case 158:	return "Teal";
			case 159:	return "Thistle";
			case 160:	return "Tomato";
			case 161:	return "Turquoise";
			case 162:	return "Violet";
			case 163:	return "Wheat";
			case 164:	return "White";
			case 165:	return "WhiteSmoke";
			case 166:	return "Yellow";
			case 167:	return "YellowGreen";
#if NET_2_0
			case 168:	return "ButtonFace";
			case 169:	return "ButtonHighlight";
			case 170:	return "ButtonShadow";
			case 171:	return "GradientActiveCaption";
			case 172:	return "GradientInactiveCaption";
			case 173:	return "MenuBar";
			case 174:	return "MenuHighlight";
#endif
			default:	return String.Empty;
			}
		}

		public static string GetName (KnownColor kc)
		{
			return GetName ((short)kc);
		}

		public static Color FindColorMatch (Color c)
		{
			uint argb = (uint) c.ToArgb ();
			for (int i = 0; i < Values.Length; i++) {
				if (argb == Values [i].ToArgb ())
					return KnownColors.FromKnownColor ((KnownColor) i);
			}
			return Color.Empty;
		}
	}
}
