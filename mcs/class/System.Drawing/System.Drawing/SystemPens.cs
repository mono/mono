//
// System.Drawing.SystemPens.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Novell, Inc.
//
using System;

namespace System.Drawing
{
	public sealed class SystemPens
	{
		static Pen active_caption_text;
		static Pen control;
		static Pen control_dark;
		static Pen control_dark_dark;
		static Pen control_light;
		static Pen control_light_light;
		static Pen control_text;
		static Pen gray_text;
		static Pen highlight_text;
		static Pen inactive_caption_text;
		static Pen info_text;
		static Pen menu_text;
		static Pen window_frame;
		static Pen window_text;
		
		static SystemPens ()
		{
			Console.WriteLine ("SystemPens: Need to fetch values from Win32");

			//
			// These happen to match my current theme, not right
			//
			active_caption_text = new Pen (SystemColors.ActiveCaptionText);
			control = new Pen (SystemColors.Control);
			control_dark = new Pen (SystemColors.ControlDark);
			control_dark_dark = new Pen (SystemColors.ControlDarkDark);
			control_light = new Pen (SystemColors.ControlLight);
			control_light_light = new Pen (SystemColors.ControlLightLight);
			control_text = new Pen (SystemColors.ControlText);
			gray_text = new Pen (SystemColors.GrayText);
			highlight_text = new Pen (SystemColors.HighlightText);
			inactive_caption_text = new Pen (SystemColors.InactiveCaptionText);
			menu_text = new Pen (SystemColors.MenuText);
			window_frame = new Pen (SystemColors.WindowFrame);
			window_text = new Pen (SystemColors.WindowText);
			info_text = new Pen (SystemColors.InfoText);
		}

		private SystemPens ()
		{
		}
		
		public static Pen ActiveCaptionText {
			get {
				return active_caption_text;
			}
		}
		
		public static Pen Control {
			get {
				return control;
			}
		}
		
		public static Pen ControlDark {
			get {
				return control_dark;
			}
		}
		
		public static Pen ControlDarkDark {
			get {
				return control_dark_dark;
			}
		}
		
		public static Pen ControlLight {
			get {
				return control_light;
			}
		}
		
		public static Pen ControlLightLight {
			get {
				return control_light_light;
			}
		}
		
		public static Pen ControlText {
			get {
				return control_text;
			}
		}
		
		public static Pen GrayText {
			get {
				return gray_text;
			}
		}
		
		public static Pen HighlightText {
			get {
				return highlight_text;
			}
		}
		
		public static Pen InactiveCaptionText {
			get {
				return inactive_caption_text;
			}
		}
		
		public static Pen InfoText {
			get {
				return info_text;
			}
		}
		
		public static Pen MenuText {
			get {
				return menu_text;
			}
		}
		
		public static Pen WindowFrame {
			get {
				return window_frame;
			}
		}
		
		public static Pen WindowText {
			get {
				return window_text;
			}
		}
		
	}
}
