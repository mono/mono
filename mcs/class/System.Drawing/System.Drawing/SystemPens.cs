//
// System.Drawing.SystemPens.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Ravindra (rkumar@novell.com)
//
// Copyright (C) 2003-2004 Novell, Inc. http://www.novell.com
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
	public sealed class SystemPens
	{
		static private Pen active_caption_text;
		static private Pen control;
		static private Pen control_dark;
		static private Pen control_dark_dark;
		static private Pen control_light;
		static private Pen control_light_light;
		static private Pen control_text;
		static private Pen gray_text;
		static private Pen highlight;
		static private Pen highlight_text;
		static private Pen inactive_caption_text;
		static private Pen info_text;
		static private Pen menu_text;
		static private Pen window_frame;
		static private Pen window_text;
		
		private SystemPens () {	}
		
		public static Pen ActiveCaptionText {
			get {
				if (active_caption_text == null) {
					active_caption_text = new Pen (SystemColors.ActiveCaptionText);
					active_caption_text.isModifiable = false;
				}
				
				return active_caption_text;
			}
		}
		
		public static Pen Control {
			get {
				if (control == null) {
					control = new Pen (SystemColors.Control);
					control.isModifiable = false;
				}
								
				return control;
			}
		}
		
		public static Pen ControlDark {
			get {
				if (control_dark == null) {
					control_dark = new Pen (SystemColors.ControlDark);
					control_dark.isModifiable = false;
				}

				return control_dark;
			}
		}
		
		public static Pen ControlDarkDark {
			get {
				if (control_dark_dark == null) {
					control_dark_dark = new Pen (SystemColors.ControlDarkDark);
					control_dark_dark.isModifiable = false;
				}
				
				return control_dark_dark;
			}
		}
		
		public static Pen ControlLight {
			get {
				if (control_light == null) {
					control_light = new Pen (SystemColors.ControlLight);
					control_light.isModifiable = false;
				}
				
				return control_light;
			}
		}
		
		public static Pen ControlLightLight {
			get {
				if (control_light_light == null) {
					control_light_light = new Pen (SystemColors.ControlLightLight);
					control_light_light.isModifiable = false;
				}
				
				return control_light_light;
			}
		}
		
		public static Pen ControlText {
			get {
				if (control_text == null) {
					control_text = new Pen (SystemColors.ControlText);
					control_text.isModifiable = false;
				}
				
				return control_text;
			}
		}
		
		public static Pen GrayText {
			get {
				if (gray_text == null) {
					gray_text = new Pen (SystemColors.GrayText);
					gray_text.isModifiable = false;
				}
				
				return gray_text;
			}
		}
		
		public static Pen Highlight {
			get {
				if (highlight == null) {
					highlight = new Pen (SystemColors.Highlight);
					highlight.isModifiable = false;
				}
				
				return highlight;
			}
		}
		
		public static Pen HighlightText {
			get {
				if (highlight_text == null) {
					highlight_text = new Pen (SystemColors.HighlightText);
					highlight_text.isModifiable = false;
				}
				
				return highlight_text;
			}
		}
		
		public static Pen InactiveCaptionText {
			get {
				if (inactive_caption_text == null) {
					inactive_caption_text = new Pen (SystemColors.InactiveCaptionText);
					inactive_caption_text.isModifiable = false;
				}
				
				return inactive_caption_text;
			}
		}
		
		public static Pen InfoText {
			get {
				if (info_text == null) {
					info_text = new Pen (SystemColors.InfoText);
					info_text.isModifiable = false;
				}
				
				return info_text;
			}
		}
		
		public static Pen MenuText {
			get {
				if (menu_text == null) {
					menu_text = new Pen (SystemColors.MenuText);
					menu_text.isModifiable = false;
				}
				
				return menu_text;
			}
		}
		
		public static Pen WindowFrame {
			get {
				if (window_frame == null) {
					window_frame = new Pen (SystemColors.WindowFrame);
					window_frame.isModifiable = false;
				}
				
				return window_frame;
			}
		}
		
		public static Pen WindowText {
			get {
				if (window_text == null) {
					window_text = new Pen (SystemColors.WindowText);
					window_text.isModifiable = false;
				}
				
				return window_text;
			}
		}
		
		public static Pen FromSystemColor (Color c)
		{
			if (c.IsSystemColor) {
				Pen newPen = new Pen (c);
				newPen.isModifiable = false;
				return newPen;
			}
			
			String message = String.Format ("The color {0} is not a system color.", c);
			throw new ArgumentException (message);
		}
		
	}
}
