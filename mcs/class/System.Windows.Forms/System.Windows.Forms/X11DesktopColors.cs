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
//	Alexander Olk		(alex.olk@googlemail.com)
//
//

using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System;

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
		internal struct GObjectStruct {
			public IntPtr Instance;
			public IntPtr ref_count;
			public IntPtr data;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct GtkStyleStruct {
			internal GObjectStruct obj;
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
			FindDesktopEnvironment();
			
			switch(desktop) {
				case Desktop.Gtk: {
					//IntPtr		dispmgr;
					//IntPtr		gdkdisplay;
					IntPtr		widget;
					IntPtr		style_ptr;
					GtkStyleStruct	style;

					try {
						GtkInit();
						//dispmgr =  gdk_display_manager_get ();
						//gdkdisplay =  gdk_display_manager_get_default_display (dispmgr);

						widget = gtk_invisible_new ();
						gtk_widget_ensure_style (widget);
						style_ptr = gtk_widget_get_style (widget);

						style = (GtkStyleStruct) Marshal.PtrToStructure (style_ptr, typeof (GtkStyleStruct));
						
						ThemeEngine.Current.ColorControl = ColorFromGdkColor (style.bg[0]);
						ThemeEngine.Current.ColorControlText = ColorFromGdkColor (style.fg[0]);
						ThemeEngine.Current.ColorControlDark = ColorFromGdkColor (style.dark[0]);
						ThemeEngine.Current.ColorControlLight = ColorFromGdkColor (style.light[0]);
						ThemeEngine.Current.ColorControlLightLight = ControlPaint.Light (ThemeEngine.Current.ColorControlLight);
						ThemeEngine.Current.ColorControlDarkDark = ControlPaint.Dark (ThemeEngine.Current.ColorControlDark);
						ThemeEngine.Current.ColorButtonFace = ColorFromGdkColor (style.bg[0]);

						// We don't want ControlLight to disappear on a white background!
						if (ThemeEngine.Current.ColorControlLight.ToArgb () == Color.White.ToArgb ()) {
							ThemeEngine.Current.ColorControlLight = Color.FromArgb (255, 227, 227, 227);
						}
						widget = gtk_menu_new ();
						gtk_widget_ensure_style (widget);
						style_ptr = gtk_widget_get_style (widget);

						style = (GtkStyleStruct) Marshal.PtrToStructure (style_ptr, typeof (GtkStyleStruct));

						ThemeEngine.Current.ColorMenu = ColorFromGdkColor (style.bg [0]);
						ThemeEngine.Current.ColorMenuText = ColorFromGdkColor (style.text [0]);
					}

					catch (DllNotFoundException) {
						Console.Error.WriteLine("Gtk not found (missing LD_LIBRARY_PATH to libgtk-x11-2.0.so.0?), using built-in colorscheme");
					}

					catch {
						Console.Error.WriteLine("Gtk colorscheme read failure, using built-in colorscheme");
					}
					break;
				}
					
				case Desktop.KDE: {
						if (! ReadKDEColorsheme() )
							Console.Error.WriteLine("KDE colorscheme read failure, using built-in colorscheme");
						break;
					}
					
				default: {
					break;
				}
			}
		}

		static void GtkInit ()
		{
			gtk_init_check (IntPtr.Zero, IntPtr.Zero);
		}
		#endregion	// Constructors

		#region Properties
		static void FindDesktopEnvironment() {
			desktop = Desktop.Gtk;
			string session =  Environment.GetEnvironmentVariable("DESKTOP_SESSION");
				
			if ( session != null ) {
				session = session.ToUpper( );
					
				if ( session == "DEFAULT" ) {
					string helper = Environment.GetEnvironmentVariable("KDE_FULL_SESSION");
						
					if ( helper != null )
						desktop = Desktop.KDE;
				} else
				if ( session.StartsWith("KDE") )
					desktop = Desktop.KDE;
			}
		}
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
		
		private static bool ReadKDEColorsheme() {
			string full_kdegloabals_filename = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
				+ "/"
				+ ".kde/share/config/kdeglobals";
			
			if (!File.Exists(full_kdegloabals_filename)) 
				return false;
			
			StreamReader sr = new StreamReader(full_kdegloabals_filename);
			
			string line = sr.ReadLine();
			
			Color tmp_color;
			
			while (line != null) {
				line = line.Trim();
				
				if (line.StartsWith( "background=")) {
					tmp_color = GetColorFromKDEString(line);
					
					if (tmp_color != Color.Empty) {
						ThemeEngine.Current.ColorControl = tmp_color;
						ThemeEngine.Current.ColorMenu = tmp_color;
					}
				} else
				if (line.StartsWith( "foreground=")) {
					tmp_color = GetColorFromKDEString(line);
					
					if (tmp_color != Color.Empty) {
						ThemeEngine.Current.ColorControlText = tmp_color;
						ThemeEngine.Current.ColorMenuText = tmp_color;						
					}
				} else
				if (line.StartsWith("selectBackground")) {
					tmp_color = GetColorFromKDEString(line);
					
					if (tmp_color != Color.Empty) {
						ThemeEngine.Current.ColorHighlight = tmp_color;
					}
				} else
				if (line.StartsWith("selectForeground")) {
					tmp_color = GetColorFromKDEString(line);
					
					if (tmp_color != Color.Empty) {
						ThemeEngine.Current.ColorHighlightText = tmp_color;
					}
				}
				
				line = sr.ReadLine();
			}
			
			sr.Close();
			
			return true;
		}
		
		private static Color GetColorFromKDEString(string line) {
			string[] split = line.Split(new char[] {'='});
			
			if (split.Length > 0) {
				line = split[1];
				
				split = line.Split(new char[] {','});
				
				if (split.Length == 3) {
					int r = System.Convert.ToInt32(split[0]);
					int g = System.Convert.ToInt32(split[1]);
					int b = System.Convert.ToInt32(split[2]);
					
					return Color.FromArgb(r, g, b);
				}
			}
			
			return Color.Empty;
		}
		#endregion	// Methods

		#region	DllImports
		const string libgdk = "libgdk-x11-2.0";
		const string libgtk = "libgtk-x11-2.0";
		
		[DllImport(libgtk)]
		static extern bool gtk_init_check (IntPtr argc, IntPtr argv);

		[DllImport(libgdk)]
		internal static extern IntPtr gdk_display_manager_get ();

		[DllImport(libgdk)]
		internal static extern IntPtr gdk_display_manager_get_default_display (IntPtr display_manager);

		[DllImport(libgtk)]
		static extern IntPtr gtk_invisible_new ();

		[DllImport(libgtk)]
		static extern IntPtr gtk_menu_new ();

		//[DllImport(libgtk)]
		//static extern IntPtr gtk_menu_item_new_with_label (string label);

		[DllImport(libgtk)]
		static extern void gtk_widget_ensure_style (IntPtr raw);

		[DllImport(libgtk)]
		static extern IntPtr gtk_widget_get_style (IntPtr raw);
		#endregion	// DllImports
	}
}
