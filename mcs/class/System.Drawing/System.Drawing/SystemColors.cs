//
// System.Drawing.SystemColors.cs
//
// Copyright (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Peter Dennis Bartok (pbartok@novell.com)
//


// The following method can be (and was) used to generate the defaults for the properties
//private static void GenerateColorTable (Type type) {
//	PropertyInfo [] props = type.GetProperties ();
//	foreach (PropertyInfo prop in props){
//		if (prop.PropertyType != typeof (Color))
//			continue;
//
//		MethodInfo getget = prop.GetGetMethod ();
//		if (getget == null || getget.IsStatic == false)
//			continue;
//
//		object o = prop.GetValue (null, null);
//
//		Color c = (Color) o;
//
//		StringBuilder name = new StringBuilder(prop.Name);
//		name[0] = Char.ToLower(name[0]);
//		for (int i = 1; i < name.Length; i++) {
//			if (Char.IsUpper(name[i])) {
//				name[i] = Char.ToLower(name[i]);
//				name.Insert(i, '_');
//			}
//		}
//
//		Console.WriteLine("static private Color " + name.ToString() + " = Color.FromArgbSystem (" + 
//			c.A.ToString() + ", " +
//			c.R.ToString() + ", " +
//			c.G.ToString() + ", " +
//			c.B.ToString() + ", " +
//			"\"" + prop.Name + "\", " +
//			"KnownColor." + prop.Name + ");");
//	}
//}


using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Drawing {
	public sealed class SystemColors
	{
		// Default Windows XP color table
		static private Color active_border = Color.FromArgbSystem (255, 212, 208, 200, "ActiveBorder", KnownColor.ActiveBorder);
		static private Color active_caption = Color.FromArgbSystem (255, 0, 164, 164, "ActiveCaption", KnownColor.ActiveCaption);
		static private Color active_caption_text = Color.FromArgbSystem (255, 255, 255, 255, "ActiveCaptionText", KnownColor.ActiveCaptionText);
		static private Color app_workspace = Color.FromArgbSystem (255, 128, 128, 128, "AppWorkspace", KnownColor.AppWorkspace);
		static private Color control = Color.FromArgbSystem (255, 212, 208, 200, "Control", KnownColor.Control);
		static private Color control_dark = Color.FromArgbSystem (255, 128, 128, 128, "ControlDark", KnownColor.ControlDark);
		static private Color control_dark_dark = Color.FromArgbSystem (255, 64, 64, 64, "ControlDarkDark", KnownColor.ControlDarkDark);
		static private Color control_light = Color.FromArgbSystem (255, 212, 208, 200, "ControlLight", KnownColor.ControlLight);
		static private Color control_light_light = Color.FromArgbSystem (255, 255, 255, 255, "ControlLightLight", KnownColor.ControlLightLight);
		static private Color control_text = Color.FromArgbSystem (255, 0, 0, 0, "ControlText", KnownColor.ControlText);
		static private Color desktop = Color.FromArgbSystem (255, 0, 0, 0, "Desktop", KnownColor.Desktop);
		static private Color gray_text = Color.FromArgbSystem (255, 128, 128, 128, "GrayText", KnownColor.GrayText);
		static private Color highlight = Color.FromArgbSystem (255, 10, 36, 106, "Highlight", KnownColor.Highlight);
		static private Color highlight_text = Color.FromArgbSystem (255, 255, 255, 255, "HighlightText", KnownColor.HighlightText);
		static private Color hot_track = Color.FromArgbSystem (255, 0, 0, 128, "HotTrack", KnownColor.HotTrack);
		static private Color inactive_border = Color.FromArgbSystem (255, 212, 208, 200, "InactiveBorder", KnownColor.InactiveBorder);
		static private Color inactive_caption = Color.FromArgbSystem (255, 128, 128, 128, "InactiveCaption", KnownColor.InactiveCaption);
		static private Color inactive_caption_text = Color.FromArgbSystem (255, 212, 208, 200, "InactiveCaptionText", KnownColor.InactiveCaptionText);
		static private Color info = Color.FromArgbSystem (255, 255, 255, 225, "Info", KnownColor.Info);
		static private Color info_text = Color.FromArgbSystem (255, 0, 0, 0, "InfoText", KnownColor.InfoText);
		static private Color menu = Color.FromArgbSystem (255, 212, 208, 200, "Menu", KnownColor.Menu);
		static private Color menu_text = Color.FromArgbSystem (255, 0, 0, 0, "MenuText", KnownColor.MenuText);
		static private Color scroll_bar = Color.FromArgbSystem (255, 212, 208, 200, "ScrollBar", KnownColor.ScrollBar);
		// This would give a better looking gray background instead of the high-contrast white background
		//static private Color window = Color.FromArgbSystem (255, 212, 208, 200, "Window", KnownColor.Window);
		static private Color window = Color.FromArgbSystem (255, 255, 255, 255, "Window", KnownColor.Window);
		static private Color window_frame = Color.FromArgbSystem (255, 0, 0, 0, "WindowFrame", KnownColor.WindowFrame);
		static private Color window_text = Color.FromArgbSystem (255, 0, 0, 0, "WindowText", KnownColor.WindowText);

#if NET_2_0
		static private Color button_face = Color.FromArgbSystem (255, 192, 192, 192, "ButtonFace", KnownColor.ButtonFace);
		static private Color button_highlight = Color.FromArgbSystem (255, 255, 255, 255, "ButtonHighlight", KnownColor.ButtonHighlight);
		static private Color button_shadow = Color.FromArgbSystem (255, 128, 128, 128, "ButtonShadow", KnownColor.ButtonShadow);
		static private Color gradient_activecaption = Color.FromArgbSystem (255, 16, 132, 208, "GradientActiveCaption", KnownColor.GradientActiveCaption);
		static private Color gradient_inactivecaption = Color.FromArgbSystem (255, 181, 181, 181, "GradientInactiveCaption", KnownColor.GradientInactiveCaption);
		static private Color menubar = Color.FromArgbSystem (255, 192, 192, 192, "MenuBar", KnownColor.MenuBar);
		static private Color menu_highlight = Color.FromArgbSystem (255, 0, 0, 0, "MenuHighlight", KnownColor.MenuHighlight);
#endif

#if old
		static private Color active_border = Color.FromArgbSystem (255, 131, 153, 177, "ActiveBorder", KnownColor.ActiveBorder);
		static private Color active_caption = Color.FromArgbSystem (255, 79, 101, 125, "ActiveCaption", KnownColor.ActiveCaption);
		static private Color active_caption_text = Color.FromArgbSystem (255, 255, 255, 255, "ActiveCaptionText", KnownColor.ActiveCaptionText);
		static private Color app_workspace = Color.FromArgbSystem (255, 128, 128, 128, "AppWorkspace", KnownColor.AppWorkspace);
		static private Color control = Color.FromArgbSystem (255, 192, 192, 192, "Control", KnownColor.Control);
		static private Color control_dark = Color.FromArgbSystem (255, 79, 101, 125, "ControlDark", KnownColor.ControlDark);
		static private Color control_dark_dark = Color.FromArgbSystem (255, 0, 0, 0, "ControlDarkDark", KnownColor.ControlDarkDark);
		static private Color control_light = Color.FromArgbSystem (255, 131, 153, 177, "ControlLight", KnownColor.ControlLight);
		static private Color control_light_light = Color.FromArgbSystem (255, 193, 204, 217, "ControlLightLight", KnownColor.ControlLightLight);
		static private Color control_text = Color.FromArgbSystem (255, 0, 0, 0, "ControlText", KnownColor.ControlText);
		static private Color desktop = Color.FromArgbSystem (255, 0, 0, 0, "Desktop", KnownColor.Desktop);
		static private Color gray_text = Color.FromArgbSystem (255, 79, 101, 125, "GrayText", KnownColor.GrayText);
		static private Color highlight = Color.FromArgbSystem (255, 0, 0, 128, "Highlight", KnownColor.Highlight);
		static private Color highlight_text = Color.FromArgbSystem (255, 255, 255, 255, "HighlightText", KnownColor.HighlightText);
		static private Color hot_track = Color.FromArgbSystem (255, 0, 0, 255, "HotTrack", KnownColor.HotTrack);
		static private Color inactive_border = Color.FromArgbSystem (255, 131, 153, 177, "InactiveBorder", KnownColor.InactiveBorder);
		static private Color inactive_caption = Color.FromArgbSystem (255, 128, 128, 128, "InactiveCaption", KnownColor.InactiveCaption);
		static private Color inactive_caption_text = Color.FromArgbSystem (255, 193, 204, 217, "InactiveCaptionText", KnownColor.InactiveCaptionText);
		static private Color info = Color.FromArgbSystem (255, 255, 255, 255, "Info", KnownColor.Info);
		static private Color info_text = Color.FromArgbSystem (255, 0, 0, 0, "InfoText", KnownColor.InfoText);
		static private Color menu = Color.FromArgbSystem (255, 131, 153, 177, "Menu", KnownColor.Menu);
		static private Color menu_text = Color.FromArgbSystem (255, 0, 0, 0, "MenuText", KnownColor.MenuText);
		static private Color scroll_bar = Color.FromArgbSystem (255, 193, 204, 217, "ScrollBar", KnownColor.ScrollBar);
		static private Color window = Color.FromArgbSystem (255, 255, 255, 255, "Window", KnownColor.Window);
		static private Color window_frame = Color.FromArgbSystem (255, 0, 0, 0, "WindowFrame", KnownColor.WindowFrame);
		static private Color window_text = Color.FromArgbSystem (255, 0, 0, 0, "WindowText", KnownColor.WindowText);
#endif
		private enum GetSysColorIndex {
			COLOR_SCROLLBAR			= 0,
			COLOR_BACKGROUND		= 1,
			COLOR_ACTIVECAPTION		= 2,
			COLOR_INACTIVECAPTION		= 3,
			COLOR_MENU			= 4,
			COLOR_WINDOW			= 5,
			COLOR_WINDOWFRAME		= 6,
			COLOR_MENUTEXT			= 7,
			COLOR_WINDOWTEXT		= 8,
			COLOR_CAPTIONTEXT		= 9,
			COLOR_ACTIVEBORDER		= 10,
			COLOR_INACTIVEBORDER		= 11,
			COLOR_APPWORKSPACE		= 12,
			COLOR_HIGHLIGHT			= 13,
			COLOR_HIGHLIGHTTEXT		= 14,
			COLOR_BTNFACE			= 15,
			COLOR_BTNSHADOW			= 16,
			COLOR_GRAYTEXT			= 17,
			COLOR_BTNTEXT			= 18,
			COLOR_INACTIVECAPTIONTEXT	= 19,
			COLOR_BTNHIGHLIGHT		= 20,
			COLOR_3DDKSHADOW		= 21,
			COLOR_3DLIGHT			= 22,
			COLOR_INFOTEXT			= 23,
			COLOR_INFOBK			= 24,
			
			COLOR_HOTLIGHT			= 26,
			COLOR_GRADIENTACTIVECAPTION	= 27,
			COLOR_GRADIENTINACTIVECAPTION	= 28,
			COLOR_MENUHIGHLIGHT		= 29,
			COLOR_MENUBAR			= 30,

			COLOR_DESKTOP			= 1,
			COLOR_3DFACE			= 16,
			COLOR_3DSHADOW			= 16,
			COLOR_3DHIGHLIGHT		= 20,
			COLOR_3DHILIGHT			= 20,
			COLOR_BTNHILIGHT		= 20,

			COLOR_MAXVALUE			= 30,/* Maximum value */
		}       

		[DllImport ("user32.dll", EntryPoint="GetSysColor", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetSysColor(GetSysColorIndex index);

		private static Color GetSysColor(GetSysColorIndex index, string name, KnownColor knownColor) {
			uint color;

			color = Win32GetSysColor(index);

			return Color.FromArgbSystem(255, (byte)(color & 0xff), (byte)((color >> 8) & 0xff), (byte)((color >> 16) & 0xff), name, knownColor);
		}

		// When this method is called, we teach any new color(s) to the Color class
		private static void UpdateColors() {
			active_border = Color.UpdateKnownColor (active_border.A, active_border.R, active_border.G, active_border.B, "ActiveBorder", KnownColor.ActiveBorder);
			active_caption = Color.UpdateKnownColor (active_caption.A, active_caption.R, active_caption.G, active_caption.B, "ActiveCaption", KnownColor.ActiveCaption);
			active_caption_text = Color.UpdateKnownColor (active_caption_text.A, active_caption_text.R, active_caption_text.G, active_caption_text.B, "ActiveCaptionText", KnownColor.ActiveCaptionText);
			app_workspace = Color.UpdateKnownColor (app_workspace.A, app_workspace.R, app_workspace.G, app_workspace.B, "AppWorkspace", KnownColor.AppWorkspace);
			control = Color.UpdateKnownColor (control.A, control.R, control.G, control.B, "Control", KnownColor.Control);
			control_dark = Color.UpdateKnownColor (control_dark.A, control_dark.R, control_dark.G, control_dark.B, "ControlDark", KnownColor.ControlDark);
			control_dark_dark = Color.UpdateKnownColor (control_dark_dark.A, control_dark_dark.R, control_dark_dark.G, control_dark_dark.B, "ControlDarkDark", KnownColor.ControlDarkDark);
			control_light = Color.UpdateKnownColor (control_light.A, control_light.R, control_light.G, control_light.B, "ControlLight", KnownColor.ControlLight);
			control_light_light = Color.UpdateKnownColor (control_light_light.A, control_light_light.R, control_light_light.G, control_light_light.B, "ControlLightLight", KnownColor.ControlLightLight);
			control_text = Color.UpdateKnownColor (control_text.A, control_text.R, control_text.G, control_text.B, "ControlText", KnownColor.ControlText);
			desktop = Color.UpdateKnownColor (desktop.A, desktop.R, desktop.G, desktop.B, "Desktop", KnownColor.Desktop);
			gray_text = Color.UpdateKnownColor (gray_text.A, gray_text.R, gray_text.G, gray_text.B, "GrayText", KnownColor.GrayText);
			highlight = Color.UpdateKnownColor (highlight.A, highlight.R, highlight.G, highlight.B, "Highlight", KnownColor.Highlight);
			highlight_text = Color.UpdateKnownColor (highlight_text.A, highlight_text.R, highlight_text.G, highlight_text.B, "HighlightText", KnownColor.HighlightText);
			hot_track = Color.UpdateKnownColor (hot_track.A, hot_track.R, hot_track.G, hot_track.B, "HotTrack", KnownColor.HotTrack);
			inactive_border = Color.UpdateKnownColor (inactive_border.A, inactive_border.R, inactive_border.G, inactive_border.B, "InactiveBorder", KnownColor.InactiveBorder);
			inactive_caption = Color.UpdateKnownColor (inactive_caption.A, inactive_caption.R, inactive_caption.G, inactive_caption.B, "InactiveCaption", KnownColor.InactiveCaption);
			inactive_caption_text = Color.UpdateKnownColor (inactive_caption_text.A, inactive_caption_text.R, inactive_caption_text.G, inactive_caption_text.B, "InactiveCaptionText", KnownColor.InactiveCaptionText);
			info = Color.UpdateKnownColor (info.A, info.R, info.G, info.B, "Info", KnownColor.Info);
			info_text = Color.UpdateKnownColor (info_text.A, info_text.R, info_text.G, info_text.B, "InfoText", KnownColor.InfoText);
			menu = Color.UpdateKnownColor (menu.A, menu.R, menu.G, menu.B, "Menu", KnownColor.Menu);
			menu_text = Color.UpdateKnownColor (menu_text.A, menu_text.R, menu_text.G, menu_text.B, "MenuText", KnownColor.MenuText);
			scroll_bar = Color.UpdateKnownColor (scroll_bar.A, scroll_bar.R, scroll_bar.G, scroll_bar.B, "ScrollBar", KnownColor.ScrollBar);
			window = Color.UpdateKnownColor (window.A, window.R, window.G, window.B, "Window", KnownColor.Window);
			window_frame = Color.UpdateKnownColor (window_frame.A, window_frame.R, window_frame.G, window_frame.B, "WindowFrame", KnownColor.WindowFrame);
			window_text = Color.UpdateKnownColor (window_text.A, window_text.R, window_text.G, window_text.B, "WindowText", KnownColor.WindowText);
		}

		static SystemColors ()
		{
			if (GDIPlus.RunningOnWindows ()) {
				// If we're on Windows we should behave like MS and pull the colors
				InitOnWindows ();
			}
		}

		static void InitOnWindows ()
		{
			active_border = GetSysColor(GetSysColorIndex.COLOR_ACTIVEBORDER, "ActiveBorder", KnownColor.ActiveBorder);
			active_caption = GetSysColor(GetSysColorIndex.COLOR_ACTIVECAPTION, "ActiveCaption", KnownColor.ActiveCaption);
			active_caption_text = GetSysColor(GetSysColorIndex.COLOR_CAPTIONTEXT, "ActiveCaptionText", KnownColor.ActiveCaptionText);
			app_workspace = GetSysColor(GetSysColorIndex.COLOR_APPWORKSPACE, "AppWorkspace", KnownColor.AppWorkspace);
			control = GetSysColor(GetSysColorIndex.COLOR_BTNFACE, "Control", KnownColor.Control);
			control_dark = GetSysColor(GetSysColorIndex.COLOR_BTNSHADOW, "ControlDark", KnownColor.ControlDark);
			control_dark_dark = GetSysColor(GetSysColorIndex.COLOR_3DDKSHADOW, "ControlDarkDark", KnownColor.ControlDarkDark);
			control_light = GetSysColor(GetSysColorIndex.COLOR_3DLIGHT, "ControlLight", KnownColor.ControlLight);
			control_light_light = GetSysColor(GetSysColorIndex.COLOR_BTNHIGHLIGHT, "ControlLightLight", KnownColor.ControlLightLight);
			control_text = GetSysColor(GetSysColorIndex.COLOR_BTNTEXT, "ControlText", KnownColor.ControlText);
			desktop = GetSysColor(GetSysColorIndex.COLOR_DESKTOP, "Desktop", KnownColor.Desktop);
			gray_text = GetSysColor(GetSysColorIndex.COLOR_GRAYTEXT, "GrayText", KnownColor.GrayText);
			highlight = GetSysColor(GetSysColorIndex.COLOR_HIGHLIGHT, "Highlight", KnownColor.Highlight);
			highlight_text = GetSysColor(GetSysColorIndex.COLOR_HIGHLIGHTTEXT, "HighlightText", KnownColor.HighlightText);
			hot_track = GetSysColor(GetSysColorIndex.COLOR_HOTLIGHT, "HotTrack", KnownColor.HotTrack);
			inactive_border = GetSysColor(GetSysColorIndex.COLOR_INACTIVEBORDER, "InactiveBorder", KnownColor.InactiveBorder);
			inactive_caption = GetSysColor(GetSysColorIndex.COLOR_INACTIVECAPTION, "InactiveCaption", KnownColor.InactiveCaption);
			inactive_caption_text = GetSysColor(GetSysColorIndex.COLOR_INACTIVECAPTIONTEXT, "InactiveCaptionText", KnownColor.InactiveCaptionText);
			info = GetSysColor(GetSysColorIndex.COLOR_INFOBK, "Info", KnownColor.Info);
			info_text = GetSysColor(GetSysColorIndex.COLOR_INFOTEXT, "InfoText", KnownColor.InfoText);
			menu = GetSysColor(GetSysColorIndex.COLOR_MENU, "Menu", KnownColor.Menu);
			menu_text = GetSysColor(GetSysColorIndex.COLOR_MENUTEXT, "MenuText", KnownColor.MenuText);
			scroll_bar = GetSysColor(GetSysColorIndex.COLOR_SCROLLBAR, "ScrollBar", KnownColor.ScrollBar);
			window = GetSysColor(GetSysColorIndex.COLOR_WINDOW, "Window", KnownColor.Window);
			window_frame = GetSysColor(GetSysColorIndex.COLOR_WINDOWFRAME, "WindowFrame", KnownColor.WindowFrame);
			window_text = GetSysColor(GetSysColorIndex.COLOR_WINDOWTEXT, "WindowText", KnownColor.WindowText);
			
#if NET_2_0
			button_face = GetSysColor (GetSysColorIndex.COLOR_BTNFACE, "ButtonFace", KnownColor.ButtonFace);
			button_highlight = GetSysColor (GetSysColorIndex.COLOR_BTNHIGHLIGHT, "ButtonHighlight", KnownColor.ButtonHighlight);
			button_shadow = GetSysColor (GetSysColorIndex.COLOR_BTNSHADOW, "ButtonShadow", KnownColor.ButtonShadow);
			gradient_activecaption = GetSysColor (GetSysColorIndex.COLOR_GRADIENTACTIVECAPTION, "GradientActiveCaption", KnownColor.GradientActiveCaption);
			gradient_inactivecaption = GetSysColor (GetSysColorIndex.COLOR_GRADIENTINACTIVECAPTION, "GradientInactiveCaption", KnownColor.GradientInactiveCaption);
			menubar = GetSysColor (GetSysColorIndex.COLOR_MENUBAR, "MenuBar", KnownColor.MenuBar);
			menu_highlight = GetSysColor (GetSysColorIndex.COLOR_MENUHIGHLIGHT, "MenuHighlight", KnownColor.MenuHighlight);
#endif

		}


		private SystemColors ()
		{
		}

		static public Color ActiveBorder
		{	
			get {
				return active_border;
			}
		}

		static public Color ActiveCaption
		{	
			get {
				return active_caption;
			}
		}

		static public Color ActiveCaptionText
		{	
			get {
				return active_caption_text;
			}
		}

		static public Color AppWorkspace
		{	
			get {
				return app_workspace;
			}
		}

		static public Color Control
		{	
			get {
				return control;
			}
		}

		static public Color ControlDark
		{	
			get {
				return control_dark;
			}
		}

		static public Color ControlDarkDark
		{	
			get {
				return control_dark_dark;
			}
		}

		static public Color ControlLight
		{	
			get {
				return control_light;
			}
		}

		static public Color ControlLightLight
		{	
			get {
				return control_light_light;
			}
		}

		static public Color ControlText
		{	
			get {
				return control_text;
			}
		}

		static public Color Desktop
		{	
			get {
				return desktop;
			}
		}

		static public Color GrayText
		{	
			get {
				return gray_text;
			}
		}

		static public Color Highlight
		{	
			get {
				return highlight;
			}
		}

		static public Color HighlightText
		{	
			get {
				return highlight_text;
			}
		}

		static public Color HotTrack
		{	
			get {
				return hot_track;
			}
		}

		static public Color InactiveBorder
		{	
			get {
				return inactive_border;
			}
		}

		static public Color InactiveCaption
		{	
			get {
				return inactive_caption;
			}
		}

		static public Color InactiveCaptionText
		{	
			get {
				return inactive_caption_text;
			}
		}

		static public Color Info
		{	
			get {
				return info;
			}
		}

		static public Color InfoText
		{	
			get {
				return info_text;
			}
		}

		static public Color Menu
		{	
			get {
				return menu;
			}
		}

		static public Color MenuText
		{	
			get {
				return menu_text;
			}
		}

		static public Color ScrollBar
		{	
			get {
				return scroll_bar;
			}
		}

		static public Color Window
		{	
			get {
				return window;
			}
		}

		static public Color WindowFrame
		{	
			get {
				return window_frame;
			}
		}

		static public Color WindowText
		{	
			get {
				return window_text;
			}
		}
#if NET_2_0

		static public Color ButtonFace
		{	
			get {
				return button_face;
			}
		}

		static public Color ButtonHighlight
		{	
			get {
				return button_highlight;
			}
		}

		static public Color ButtonShadow
		{	
			get {
				return button_shadow;
			}
		}

		static public Color GradientActiveCaption
		{	
			get {
				return gradient_activecaption;
			}
		}

		static public Color GradientInactiveCaption
		{	
			get {
				return gradient_inactivecaption;
			}
		}

		static public Color MenuBar
		{	
			get {
				return menubar;
			}
		}

		static public Color MenuHighlight
		{	
			get {
				return menu_highlight;
			}
		}
#endif

	}
}

