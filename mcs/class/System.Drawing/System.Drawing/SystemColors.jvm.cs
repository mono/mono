//
// System.Drawing.SystemColors.cs
//
// Authors:
//	Andrew Skiba (andrews@mainsoft.com)
//  Konstantin Triger (kostat@mainsoft.com)
//
// Copyright (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

namespace System.Drawing {

	public sealed class SystemColors
	{
		private SystemColors (){}

		static Color [] SystemColorsCache {
			get {
				return KnownColors.Values;
			}
		}

		static public Color ActiveBorder
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ActiveBorder];
			}
		}

		static public Color ActiveCaption
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ActiveCaption];
			}
		}

		static public Color ActiveCaptionText
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ActiveCaptionText];
			}
		}

		static public Color AppWorkspace
		{	
			get {
				return SystemColorsCache[(int)KnownColor.AppWorkspace];
			}
		}

		static public Color Control
		{	
			get {
				return SystemColorsCache[(int)KnownColor.Control];
			}
		}

		static public Color ControlDark
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ControlDark];
			}
		}

		static public Color ControlDarkDark
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ControlDarkDark];
			}
		}

		static public Color ControlLight
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ControlLight];
			}
		}

		static public Color ControlLightLight
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ControlLightLight];
			}
		}

		static public Color ControlText
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ControlText];
			}
		}

		static public Color Desktop
		{	
			get {
				return SystemColorsCache[(int)KnownColor.Desktop];
			}
		}

		static public Color GrayText
		{	
			get {
				return SystemColorsCache[(int)KnownColor.GrayText];
			}
		}

		static public Color Highlight
		{	
			get {
				return SystemColorsCache[(int)KnownColor.Highlight];
			}
		}

		static public Color HighlightText
		{	
			get {
				return SystemColorsCache[(int)KnownColor.HighlightText];
			}
		}

		static public Color HotTrack
		{	
			get {
				return SystemColorsCache[(int)KnownColor.HotTrack];
			}
		}

		static public Color InactiveBorder
		{	
			get {
				return SystemColorsCache[(int)KnownColor.InactiveBorder];
			}
		}

		static public Color InactiveCaption
		{	
			get {
				return SystemColorsCache[(int)KnownColor.InactiveCaption];
			}
		}

		static public Color InactiveCaptionText
		{	
			get {
				return SystemColorsCache[(int)KnownColor.InactiveCaptionText];
			}
		}

		static public Color Info
		{	
			get {
				return SystemColorsCache[(int)KnownColor.Info];
			}
		}

		static public Color InfoText
		{	
			get {
				return SystemColorsCache[(int)KnownColor.InfoText];
			}
		}

		static public Color Menu
		{	
			get {
				return SystemColorsCache[(int)KnownColor.Menu];
			}
		}

		static public Color MenuText
		{	
			get {
				return SystemColorsCache[(int)KnownColor.MenuText];
			}
		}

		static public Color ScrollBar
		{	
			get {
				return SystemColorsCache[(int)KnownColor.ScrollBar];
			}
		}

		static public Color Window
		{	
			get {
				return SystemColorsCache[(int)KnownColor.Window];
			}
		}

		static public Color WindowFrame
		{	
			get {
				return SystemColorsCache[(int)KnownColor.WindowFrame];
			}
		}

		static public Color WindowText
		{	
			get {
				return SystemColorsCache[(int)KnownColor.WindowText];
			}
		}

#if NET_2_0

		static public Color ButtonFace
		{	
			get {
				return SystemColorsCache [(int) KnownColor.ButtonFace];
			}
		}

		static public Color ButtonHighlight
		{	
			get {
				return SystemColorsCache [(int) KnownColor.ButtonHighlight];
			}
		}

		static public Color ButtonShadow
		{	
			get {
				return SystemColorsCache [(int) KnownColor.ButtonShadow];
			}
		}

		static public Color GradientActiveCaption
		{	
			get {
				return SystemColorsCache [(int) KnownColor.GradientActiveCaption];
			}
		}

		static public Color GradientInactiveCaption
		{	
			get {
				return SystemColorsCache [(int) KnownColor.GradientInactiveCaption];
			}
		}

		static public Color MenuBar
		{	
			get {
				return SystemColorsCache [(int) KnownColor.MenuBar];
			}
		}

		static public Color MenuHighlight
		{	
			get {
				return SystemColorsCache [(int) KnownColor.MenuHighlight];
			}
		}
#endif
	}
}
