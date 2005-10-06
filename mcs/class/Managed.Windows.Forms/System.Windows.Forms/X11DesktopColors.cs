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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Dennis Bartok	(pbartok@novell.com)
//
//

using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	internal class X11DesktopColors {
		#region Structs	& Enums
		[StructLayout(LayoutKind.Sequential)]	
		internal struct GdkColorStruct {
			internal int pixel;
			internal short red;
			internal short green;
			internal short blue;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct GtkStyleStruct {
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=12)]
			internal byte[] obj; /* GObject is 12 bytes*/
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=5)]
			internal GdkColorStruct[] fg;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=5)]
			internal GdkColorStruct[] bg;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=5)]
			internal GdkColorStruct[] light;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=5)]
			internal GdkColorStruct[] dark;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=5)]
			internal GdkColorStruct[] mid;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=5)]
			internal GdkColorStruct[] text;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=5)]
			internal GdkColorStruct[] baseclr;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=5)]
			internal GdkColorStruct[] text_aa;              /* Halfway between text/base */

			internal GdkColorStruct black;
			internal GdkColorStruct white;

			/* TODO: There is more stuff that we will add when we need it*/
		}

		private enum Desktop {
			Gtk,
			KDE,
			Unknown
		}
		#endregion	// Structs & Enums

		#region Local Variables
		static private Desktop		desktop;
		#endregion	// Local Variables

		#region Constructors
		static X11DesktopColors() {
			// FIXME - figure out what desktop manager we're using; for now we hardcode gtk
			desktop = Desktop.Gtk;

			switch(desktop) {
				case Desktop.Gtk: {
					//IntPtr		dispmgr;
					//IntPtr		gdkdisplay;
					IntPtr		widget;
					IntPtr		style_ptr;
					GtkStyleStruct	style;
					int		argc = 0;
					string		argv = "";

					try {
						gtk_init_check (out argc, argv);
						//dispmgr =  gdk_display_manager_get ();
						//gdkdisplay =  gdk_display_manager_get_default_display (dispmgr);
						gtk_init_check (out argc, argv);

						widget = gtk_invisible_new ();
						gtk_widget_ensure_style (widget);
						style_ptr = gtk_widget_get_style (widget);

						style = (GtkStyleStruct) Marshal.PtrToStructure (style_ptr, typeof (GtkStyleStruct));
						
						ThemeEngine.Current.ColorControl = ColorFromGdkColor (style.bg[0]);
						ThemeEngine.Current.ColorControlText = ColorFromGdkColor (style.fg[0]);
						ThemeEngine.Current.ColorControlDark = ColorFromGdkColor (style.dark[0]);
						ThemeEngine.Current.ColorControlLight = ColorFromGdkColor (style.light[0]);
						ThemeEngine.Current.ColorControlLightLight = ControlPaint.Light(ColorFromGdkColor (style.light[0]));
						ThemeEngine.Current.ColorControlDarkDark = ControlPaint.Dark(ColorFromGdkColor (style.dark[0]));

						widget = gtk_menu_new ();
						gtk_widget_ensure_style (widget);
						style_ptr = gtk_widget_get_style (widget);

						style = (GtkStyleStruct) Marshal.PtrToStructure (style_ptr, typeof (GtkStyleStruct));

						ThemeEngine.Current.ColorMenu = ColorFromGdkColor (style.bg [0]);
						ThemeEngine.Current.ColorMenuText = ColorFromGdkColor (style.text [0]);
						Console.WriteLine("Gtk colorscheme read");
					}

					catch (DllNotFoundException) {
						Console.WriteLine("Gtk not found (missing LD_LIBRARY_PATH to libgtk-x11-2.0.so?), using built-in colorscheme");
					}

					catch {
						Console.WriteLine("Gtk colorscheme read failure, using built-in colorscheme");
					}
					break;
				}

				default: {
					Console.WriteLine("Unknown destop manager, using default colors");
					break;
				}
			}
		}
		#endregion	// Constructors

		#region Properties
		#endregion	// Properties

		#region Methods
		static internal void Initialize() {
			// Do nothing; all is done in our static ctor
		}

		private static Color ColorFromGdkColor (GdkColorStruct gtkcolor) {
			return Color.FromArgb (255, 
				(gtkcolor.red >> 8)  & 0xff, 
				(gtkcolor.green  >> 8) & 0xff,
				(gtkcolor.blue >> 8) & 0xff );
		}
		
		#endregion	// Methods

		#region	DllImports
		[DllImport("libgtk-x11-2.0.so")]
		static extern bool gtk_init_check (out int argc, string argv);

		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_display_manager_get ();

		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_display_manager_get_default_display (IntPtr display_manager);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_invisible_new ();

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_menu_new ();

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_menu_item_new_with_label (string label);

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_widget_ensure_style (IntPtr raw);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_widget_get_style (IntPtr raw);
		#endregion	// DllImports
	}
}
