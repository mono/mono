//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
//
//   Jordi Mas i Hernandez <jordimash@gmail.com>
//
//

#if NET_2_0

using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{
	public sealed class SystemFonts
	{
		private static Font caption = null;
		private static Font defaultfnt = null;
		private static Font dialog = null;
		private static Font icon = null;
		private static Font menu = null;
		private static Font message = null;
		private static Font smallcaption = null;
		private static Font status = null;
		
		static SystemFonts ()
		{

		}

		private SystemFonts()
		{

		}

		public static Font GetFontByName (string name)
		{
			if (name == "CaptionFont")
				return CaptionFont;

			if (name == "DefaultFont")
				return DefaultFont;

			if (name == "DialogFont")
				return DialogFont;	

			if (name == "IconTitleFont")
				return IconTitleFont;

			if (name == "MenuFont")
				return MenuFont;

			if (name == "MessageBoxFont")
				return MessageBoxFont;

			if (name == "SmallCaptionFont")
				return SmallCaptionFont;

			if (name == "StatusFont")
				return StatusFont;			
			
			return null;
		}

		public static Font CaptionFont { 
			get {
				if (caption == null) {
					caption = new Font ("Microsoft Sans Serif", 11);
					caption.SysFontName = "CaptionFont";
				}
				return caption;
			}			
		}

		public static Font DefaultFont  { 
			get {
				if (defaultfnt == null) {
					defaultfnt = new Font ("Microsoft Sans Serif", 8.25f);
					defaultfnt.SysFontName = "DefaultFont";
				}
				return defaultfnt;
			}			
		}

		public static Font DialogFont  { 
			get {
				if (dialog == null) {
					dialog = new Font ("Tahoma", 8);
					dialog.SysFontName = "DialogFont";
				}
				return dialog;
			}			
		}

		public static Font IconTitleFont  { 
			get {
				if (icon == null) {
					icon = new Font ("Microsoft Sans Serif", 11);	
					icon.SysFontName = "IconTitleFont";
				}
				return icon;
			}			
		}

		public static Font MenuFont  { 
			get {
				if (menu == null) {
					menu = new Font ("Microsoft Sans Serif", 11);
					menu.SysFontName = "MenuFont";
				}
				return menu;
			}			
		}

		public static Font MessageBoxFont  { 
			get {
				if (message == null) {
					message = new Font ("Microsoft Sans Serif", 11);
					message.SysFontName = "MessageBoxFont";
				}
				return message;
			}			
		}

		public static Font SmallCaptionFont  { 
			get {
				if (smallcaption == null) {
					smallcaption = new Font ("Microsoft Sans Serif", 11);
					smallcaption.SysFontName = "SmallCaptionFont";
				}
				return smallcaption;
			}			
		}

		public static Font StatusFont  { 
			get {
				if (status == null) {
					status = new Font ("Microsoft Sans Serif", 11);
					status.SysFontName = "StatusFont";
				}
				return status;
			}			
		}	      
	}
}

#endif