//
// System.Drawing.SystemColors.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// 
// Generated using a slightly modified version of the program listed inside comments
// in Color.cs
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

namespace System.Drawing {

	public sealed class SystemColors
	{

		private SystemColors ()
		{
		}

		static public Color ActiveBorder
		{	
			get {
				return Color.FromArgbSystem (255, 131, 153, 177, "ActiveBorder", KnownColor.ActiveBorder);
			}
		}

		static public Color ActiveCaption
		{	
			get {
				return Color.FromArgbSystem (255, 79, 101, 125, "ActiveCaption", KnownColor.ActiveCaption);
			}
		}

		static public Color ActiveCaptionText
		{	
			get {
				return Color.FromArgbSystem (255, 255, 255, 255, "ActiveCaptionText", KnownColor.ActiveCaptionText);
			}
		}

		static public Color AppWorkspace
		{	
			get {
				return Color.FromArgbSystem (255, 128, 128, 128, "AppWorkspace", KnownColor.AppWorkspace);
			}
		}

		static public Color Control
		{	
			get {
				return Color.FromArgbSystem (255, 192, 192, 192, "Control", KnownColor.Control);
			}
		}

		static public Color ControlDark
		{	
			get {
				return Color.FromArgbSystem (255, 79, 101, 125, "ControlDark", KnownColor.ControlDark);
			}
		}

		static public Color ControlDarkDark
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 0, "ControlDarkDark", KnownColor.ControlDarkDark);
			}
		}

		static public Color ControlLight
		{	
			get {
				return Color.FromArgbSystem (255, 131, 153, 177, "ControlLight", KnownColor.ControlLight);
			}
		}

		static public Color ControlLightLight
		{	
			get {
				return Color.FromArgbSystem (255, 193, 204, 217, "ControlLightLight", KnownColor.ControlLightLight);
			}
		}

		static public Color ControlText
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 0, "ControlText", KnownColor.ControlText);
			}
		}

		static public Color Desktop
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 0, "Desktop", KnownColor.Desktop);
			}
		}

		static public Color GrayText
		{	
			get {
				return Color.FromArgbSystem (255, 79, 101, 125, "GrayText", KnownColor.GrayText);
			}
		}

		static public Color Highlight
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 128, "Highlight", KnownColor.Highlight);
			}
		}

		static public Color HighlightText
		{	
			get {
				return Color.FromArgbSystem (255, 255, 255, 255, "HighlightText", KnownColor.HighlightText);
			}
		}

		static public Color HotTrack
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 255, "HotTrack", KnownColor.HotTrack);
			}
		}

		static public Color InactiveBorder
		{	
			get {
				return Color.FromArgbSystem (255, 131, 153, 177, "InactiveBorder", KnownColor.InactiveBorder);
			}
		}

		static public Color InactiveCaption
		{	
			get {
				return Color.FromArgbSystem (255, 128, 128, 128, "InactiveCaption", KnownColor.InactiveCaption);
			}
		}

		static public Color InactiveCaptionText
		{	
			get {
				return Color.FromArgbSystem (255, 193, 204, 217, "InactiveCaptionText", KnownColor.InactiveCaptionText);
			}
		}

		static public Color Info
		{	
			get {
				return Color.FromArgbSystem (255, 255, 255, 255, "Info", KnownColor.Info);
			}
		}

		static public Color InfoText
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 0, "InfoText", KnownColor.InfoText);
			}
		}

		static public Color Menu
		{	
			get {
				return Color.FromArgbSystem (255, 131, 153, 177, "Menu", KnownColor.Menu);
			}
		}

		static public Color MenuText
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 0, "MenuText", KnownColor.MenuText);
			}
		}

		static public Color ScrollBar
		{	
			get {
				return Color.FromArgbSystem (255, 193, 204, 217, "ScrollBar", KnownColor.ScrollBar);
			}
		}

		static public Color Window
		{	
			get {
				return Color.FromArgbSystem (255, 255, 255, 255, "Window", KnownColor.Window);
			}
		}

		static public Color WindowFrame
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 0, "WindowFrame", KnownColor.WindowFrame);
			}
		}

		static public Color WindowText
		{	
			get {
				return Color.FromArgbSystem (255, 0, 0, 0, "WindowText", KnownColor.WindowText);
			}
		}
	}
}

