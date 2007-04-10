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
	}
}
