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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//	This is an experimental GTK theme. 
//
//	Comments:
//		- For now we would keep all the themes in the same assembly to have
//		handy the internals methods. 
//		- We are using Pinovoke for now to access GTK/GDK to avoid adding 
//		gtk-sharp as a SWF dependency
//		- The ThemeGtk comes from ThemeWin32Classic, we use it as the default
//		implementation for the methods that we are not taking care of.
//		- When GDK is initialised it opens its own display. There is not way of changing it,
//		then we use that display as SWF display
//		- You can activate this Theme in Linux doing export MONO_THEME=gtk
//		- GTK paints controls into a window no a device context. We should inverstigate if we 
//		we can encapsulate a dc in a gtkwindow.
//
// $Revision: 1.6 $
// $Modtime: $
// $Log: ThemeGtk.cs,v $
// Revision 1.6  2004/09/28 18:44:25  pbartok
// - Streamlined Theme interfaces:
//   * Each DrawXXX method for a control now is passed the object for the
//     control to be drawn in order to allow accessing any state the theme
//     might require
//
//   * ControlPaint methods for the theme now have a CP prefix to avoid
//     name clashes with the Draw methods for controls
//
//   * Every control now retrieves it's DefaultSize from the current theme
//
// Revision 1.5  2004/09/02 16:32:54  jordi
// implements resource pool for pens, brushes, and hatchbruses
//
// Revision 1.4  2004/08/24 18:37:02  jordi
// fixes formmating, methods signature, and adds missing events
//
// Revision 1.3  2004/08/20 20:39:51  jordi
// use style_attach
//
// Revision 1.2  2004/08/20 00:55:28  jordi
// fixes button order
//
// Revision 1.1  2004/08/19 22:27:40  jordi
// experimental GTK theme support
//
//
// NOT COMPLETE

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	internal class ThemeGtk : ThemeWin32Classic
	{		
		/* GTK enums */
		internal enum StateType 
		{
			Normal,
			Active,
			Prelight,
			Selected,
			Insensitive,
		}	

		internal enum ShadowType 
		{
			None,
			In,
			Out,
			EtchedIn,
			EtchedOut,
		}	

		internal enum ArrowType 
		{
			Up,
			Down,
			Left,
			Right,
		}

		/* Structs */
		[StructLayout(LayoutKind.Sequential)]	
		internal struct GdkColorStruct
		{
		 	internal int pixel;
		 	internal short red;
		 	internal short green;
		   	internal short blue;
		}

		[StructLayout(LayoutKind.Sequential)]	
		internal struct GtkStyleStruct
		{
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
			internal GdkColorStruct[] text_aa;		/* Halfway between text/base */
  
			internal GdkColorStruct black;
			internal GdkColorStruct white;

		 	/* TODO: There is more stuff that we will add when we need it*/
		}

			
		/* GDK imports */
		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_display_manager_get ();

		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_display_manager_get_default_display (IntPtr display_manager);

		[DllImport("libgdk-x11-2.0.so")]
		internal static extern void gdk_display_manager_set_default_display (IntPtr display_manager, IntPtr display);

		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_x11_display_get_xdisplay (IntPtr display);

		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_window_foreign_new_for_display (IntPtr display, uint anid);

		[DllImport("libgdk-x11-2.0.so")]
		static extern bool gdk_init_check(out int argc, string argv);	

		/* GTK imports */		
		[DllImport("libgtk-x11-2.0.so")]
		static extern bool gtk_init_check (out int argc, string argv);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_adjustment_new (double value, double lower, double upper, double step_increment, double page_increment, double page_size);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_rc_get_style (IntPtr widget);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_vscrollbar_new(IntPtr adjustment);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_style_attach (IntPtr raw, IntPtr window);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_rc_style_new ();

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_invisible_new ();

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_widget_ensure_style (IntPtr raw);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_widget_get_style (IntPtr raw);

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_style_detach (IntPtr raw);

		/* GTK Drawing */
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_handle (IntPtr style, IntPtr window, int state_type, int shadow_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height, int orientation);

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_arrow (IntPtr style, IntPtr window, int state_type, int shadow_type, 
			IntPtr area, IntPtr widget, string detail, int arrow_type, bool fill, int x, int y, int width, int height);

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_slider(IntPtr style, IntPtr window, int state_type, int shadow_type, 
			IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height, int orientation);

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_box(IntPtr style, IntPtr window, int state_type, int shadow_type, 
			IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);

		/* Data */
		static protected IntPtr dispmgr;
		static protected IntPtr gdkdisplay;
		static protected IntPtr widget;
		static protected IntPtr style;
		static protected SolidBrush br_buttonface;
		static protected SolidBrush br_buttontext;

		public static void InitGtk ()
		{	
			Console.WriteLine ("ThemeGtk Init");		
			int argc = 0;
			string argv = "";
			
			gdk_init_check (out argc, argv);	

			dispmgr =  gdk_display_manager_get ();
			gdkdisplay =  gdk_display_manager_get_default_display (dispmgr);
			gtk_init_check (out argc, argv);

			widget = gtk_invisible_new ();
			gtk_widget_ensure_style (widget);
			style = gtk_widget_get_style (widget);			

			XplatUIX11.SetDisplay (gdk_x11_display_get_xdisplay (gdkdisplay));
		}

		public void LoadSysDefaultColors ()
		{
			GtkStyleStruct style_struct; 			
			
			style_struct = (GtkStyleStruct) Marshal.PtrToStructure (style, typeof (GtkStyleStruct));			
			defaultWindowBackColor = ColorFromGdkColor (style_struct.bg[0]);
			defaultWindowForeColor = ColorFromGdkColor (style_struct.fg[0]);
		}

		public ThemeGtk () : base ()
		{
			Console.WriteLine ("ThemeGtk constructor");
			InitGtk ();
			default_font =	new Font (FontFamily.GenericSansSerif, 8.25f);
			
			LoadSysDefaultColors ();	

			br_buttonface = new SolidBrush (defaultWindowBackColor);		
			br_buttontext = new SolidBrush (defaultWindowForeColor);			
		}	

		public override bool DoubleBufferingSupported {
			get {return false; }
		}	


		public void DrawScrollButton (Graphics dc, Rectangle area, ScrollButton type, ButtonState state,
			IntPtr gdkwindow, IntPtr style)
		{
			ArrowType arrow_type = 0;

			gtk_paint_box (style, 
					gdkwindow, 
					(int) StateType.Normal,
					(int) ShadowType.Out,
					IntPtr.Zero,
					IntPtr.Zero,
					"trough",					
					area.X, area.Y,
					area.Width, area.Height);
			
			/* Calc arrows coordinates */
			switch (type) {
			case ScrollButton.Up:
				arrow_type = ArrowType.Up;
				break;
			case ScrollButton.Down:
				arrow_type = ArrowType.Down;
				break;
			case ScrollButton.Right:
				arrow_type = ArrowType.Right;
				break;
			case ScrollButton.Left:
				arrow_type = ArrowType.Left;
				break;
			default:
				break;
			}
				

			gtk_paint_arrow (style, 
					gdkwindow, 
					(int) StateType.Normal,
					(int) ShadowType.In,
					IntPtr.Zero,
					IntPtr.Zero,
					"",		
					(int) arrow_type, true, 
					area.X + ((area.Width - (area.Width/2) ) / 2), 
					area.Y + ((area.Height - (area.Height/2) ) / 2),
					area.Width / 2, area.Height / 2);			
		
		}
#if updated		
		public override void DrawScrollBar (Graphics dc, Rectangle area, ScrollBar bar,
			ref Rectangle thumb_pos, ref Rectangle first_arrow_area, ref Rectangle second_arrow_area, 
			ButtonState first_arrow, ButtonState second_arrow, ref int scrollbutton_width, 
			ref int scrollbutton_height, bool vert)
		{
			IntPtr gdkwindow = gdk_window_foreign_new_for_display (gdkdisplay, (uint) bar.Handle);			
			IntPtr adj = gtk_adjustment_new (0, 0, 0, 0, 0, 0);
                        IntPtr scrollbar = gtk_vscrollbar_new (adj);
			IntPtr style;
                                       
                        style = gtk_rc_get_style (scrollbar);		
			style = gtk_style_attach (style, gdkwindow);  // need it			
			
			/* Background */
			gtk_paint_box (style, 
				gdkwindow, 
                     		(int) StateType.Active,
                     		(int) ShadowType.In,
                     		IntPtr.Zero,
				IntPtr.Zero,
				"trough",
                 		area.X, area.Y,
				area.Width, area.Height);

			/* See gtk_range_expose */
			first_arrow_area.X = first_arrow_area. Y = 0;
			first_arrow_area.Width = scrollbutton_width;
			first_arrow_area.Height = scrollbutton_height;

			if (vert) {		

				second_arrow_area.X = 0;
				second_arrow_area.Y = area.Height - scrollbutton_height;
				second_arrow_area.Width = scrollbutton_width;
				second_arrow_area.Height = scrollbutton_height;

				/* First button*/
			 	DrawScrollButton (dc, first_arrow_area, ScrollButton.Up, first_arrow,
					gdkwindow, style);

				/* Second button*/
			 	DrawScrollButton (dc, second_arrow_area, ScrollButton.Down, second_arrow,
					gdkwindow, style);

			} else {

				second_arrow_area.Y = 0;
				second_arrow_area.X = area.Width - scrollbutton_width;
				second_arrow_area.Width = scrollbutton_width;
				second_arrow_area.Height = scrollbutton_height;

				/* First button*/
			 	DrawScrollButton (dc, first_arrow_area, ScrollButton.Left, first_arrow,
					gdkwindow, style);

				/* Second button*/
			 	DrawScrollButton (dc, second_arrow_area, ScrollButton.Right, second_arrow,
					gdkwindow, style);

			}

			/* Slider */
			gtk_paint_slider (style, 
				gdkwindow, 
				(int) StateType.Normal,
				(int) ShadowType.Out,
				IntPtr.Zero,
				IntPtr.Zero,
				"",		
				thumb_pos.X, thumb_pos.Y,
				thumb_pos.Width, thumb_pos.Height,
				(int) Orientation.Vertical);

			//gtk_style_detach (style);
		}
#endif
		
		private static Color ColorFromGdkColor (GdkColorStruct gtkcolor)
		{
			return Color.FromArgb (255, 
				(gtkcolor.red >> 8)  & 0xff, 
				(gtkcolor.green  >> 8) & 0xff,
				(gtkcolor.blue >> 8) & 0xff );
		}

	} //class
}
