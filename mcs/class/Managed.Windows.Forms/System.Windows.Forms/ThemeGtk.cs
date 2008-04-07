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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Alexander Olk, alex.olk@googlemail.com
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
//		- GTK paints controls into a window not a device context. We should inverstigate if we 
//		we can encapsulate a dc in a gtkwindow.


// NOT COMPLETE

// TODO:	- fix position of button focus rectangle
//		- fix TrackBar drawing location


//#define _EXPERIMENTAL_

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;

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
		internal struct GObjectStruct {
			public IntPtr Instance;
			public IntPtr ref_count;
			public IntPtr data;
		}


		[StructLayout(LayoutKind.Sequential)]	
		internal struct GtkStyleStruct
		{
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
		static extern bool gdk_init_check(IntPtr argc, IntPtr argv);	
		
		//[DllImport("libgdk-x11-2.0.so")]
		//static extern IntPtr gdk_pixmap_new (IntPtr drawable, int width, int height, int depth);
		
		//[DllImport("libgdk-x11-2.0.so")]
		//static extern IntPtr gdk_pixbuf_get_from_drawable (IntPtr dest, IntPtr drawable_src, IntPtr cmap,
		//						   int src_x, int src_y, int dest_x, int dest_y, int width, int height);
		
		//[DllImport("libgdk-x11-2.0.so")]
		//static extern bool gdk_pixbuf_save_to_buffer (IntPtr pixbuf, out IntPtr buffer, out UIntPtr buffer_size, string type, out IntPtr error, IntPtr option_dummy);
		
		//[DllImport("libgdk-x11-2.0.so")]
		//static extern IntPtr gdk_drawable_get_colormap (IntPtr drawable);
		
		//[DllImport("libgdk-x11-2.0.so")]
		//static extern IntPtr gdk_colormap_get_system ();
		
		//[DllImport("libgdk-x11-2.0.so")]
		//static extern IntPtr gdk_pixbuf_new (int colorspace, bool has_alpha, int bits_per_sample, int width, int height);
		
		//[DllImport("libgdk-x11-2.0.so")]
		//static extern IntPtr gdk_gc_new (IntPtr drawable);
		
		/* glib imports*/
		//[DllImport("libglib-2.0.so")]
		//static extern void g_free (IntPtr mem);
		
		//[DllImport("libgobject-2.0.so")]
		//static extern void g_object_unref (IntPtr nativeObject);

		/* GTK imports */		
		[DllImport("libgtk-x11-2.0.so")]
		static extern bool gtk_init_check (IntPtr argc, IntPtr argv);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_adjustment_new (double value, double lower, double upper, double step_increment, double page_increment, double page_size);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_rc_get_style (IntPtr widget);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_vscrollbar_new(IntPtr adjustment);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_hscrollbar_new(IntPtr adjustment);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_style_attach (IntPtr raw, IntPtr window);

		//[DllImport("libgtk-x11-2.0.so")]
		//static extern IntPtr gtk_rc_style_new ();

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_invisible_new ();

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_widget_ensure_style (IntPtr raw);

		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_widget_get_style (IntPtr raw);

		//[DllImport("libgtk-x11-2.0.so")]
		//static extern void gtk_style_detach (IntPtr raw);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_button_new ();
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_progress_bar_new ();
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_radio_button_new (IntPtr group);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_check_button_new ();
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_hscale_new (IntPtr adjustment);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_vscale_new (IntPtr adjustment);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_range_set_range (IntPtr range, double min, double max);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_range_set_value (IntPtr range, double value);

		/* GTK Drawing */
		//[DllImport("libgtk-x11-2.0.so")]
		//static extern void gtk_paint_handle (IntPtr style, IntPtr window, int state_type, int shadow_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height, int orientation);

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_arrow (IntPtr style, IntPtr window, int state_type, int shadow_type, 
						    IntPtr area, IntPtr widget, string detail, int arrow_type, bool fill, int x, int y, int width, int height);

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_slider (IntPtr style, IntPtr window, int state_type, int shadow_type, 
						     IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height, int orientation);

		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_box (IntPtr style, IntPtr window, int state_type, int shadow_type, 
						  IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_flat_box (IntPtr style, IntPtr window, int state_type, int shadow_type,
						       IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);
		
		//[DllImport("libgtk-x11-2.0.so")]
		//static extern void gtk_paint_hline(IntPtr style, IntPtr window, int state_type, IntPtr area, IntPtr widget, string detail, int x1, int x2, int y);
		
		//[DllImport("libgtk-x11-2.0.so")]
		//static extern void gtk_paint_vline(IntPtr style, IntPtr window, int state_type, IntPtr area, IntPtr widget, string detail, int y1, int y2, int x);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_check(IntPtr style, IntPtr window, int state_type, int shadow_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_focus(IntPtr style, IntPtr window, int state_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_widget_size_allocate (IntPtr widget, ref Rectangle allocation);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_option (IntPtr style, IntPtr window, int state_type, int shadow_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_widget_grab_focus (IntPtr widget);
		
		/* Data */
		static protected IntPtr dispmgr;
		static protected IntPtr gdkdisplay;
		static protected IntPtr widget;
		static protected IntPtr global_style;
		
		#if _EXPERIMENTAL_
		static protected IntPtr global_color_map = IntPtr.Zero;
		#endif
		
		static protected IntPtr global_gtk_button = IntPtr.Zero;
		static protected IntPtr global_gtk_button_style = IntPtr.Zero;
		
		static protected IntPtr global_gtk_vscrollbar = IntPtr.Zero;
		static protected IntPtr global_gtk_vscrollbar_style = IntPtr.Zero;
		
		static protected IntPtr global_gtk_hscrollbar = IntPtr.Zero;
		static protected IntPtr global_gtk_hscrollbar_style = IntPtr.Zero;
		
		static protected IntPtr global_gtk_progress_bar = IntPtr.Zero;
		static protected IntPtr global_gtk_progress_bar_style = IntPtr.Zero;
		
		static protected IntPtr global_gtk_radio_button = IntPtr.Zero;
		static protected IntPtr global_gtk_radio_button_style = IntPtr.Zero;
		
		static protected IntPtr global_gtk_check_button = IntPtr.Zero;
		static protected IntPtr global_gtk_check_button_style = IntPtr.Zero;
		
		static protected IntPtr global_gtk_hscale = IntPtr.Zero;
		static protected IntPtr global_gtk_hscale_style = IntPtr.Zero;
		
		static protected IntPtr global_gtk_vscale = IntPtr.Zero;
		static protected IntPtr global_gtk_vscale_style = IntPtr.Zero;
		
		static protected IntPtr current_gdk_drawable = IntPtr.Zero;
		static protected IntPtr current_style = IntPtr.Zero;
		static protected IntPtr current_widget = IntPtr.Zero;

		public static void InitGtk ()
		{	
			Console.WriteLine ("ThemeGtk Init");		
			
			gdk_init_check (IntPtr.Zero, IntPtr.Zero);	

			dispmgr =  gdk_display_manager_get ();
			gdkdisplay =  gdk_display_manager_get_default_display (dispmgr);
			gtk_init_check (IntPtr.Zero, IntPtr.Zero);

			widget = gtk_invisible_new ();
			gtk_widget_ensure_style (widget);
			global_style = gtk_widget_get_style (widget);			

			XplatUIX11.GetInstance().SetDisplay (gdk_x11_display_get_xdisplay (gdkdisplay));
			
			global_gtk_button = gtk_button_new();
			gtk_widget_ensure_style (global_gtk_button);
			global_gtk_button_style = gtk_rc_get_style (global_gtk_button);
			
			IntPtr adj = gtk_adjustment_new (0, 0, 0, 0, 0, 0);
                        global_gtk_vscrollbar = gtk_vscrollbar_new (adj);
			gtk_widget_ensure_style (global_gtk_vscrollbar);
			global_gtk_vscrollbar_style = gtk_rc_get_style (global_gtk_vscrollbar);
			
			global_gtk_hscrollbar = gtk_hscrollbar_new (adj);
			gtk_widget_ensure_style (global_gtk_hscrollbar);
			global_gtk_hscrollbar_style = gtk_rc_get_style (global_gtk_hscrollbar);
			
			global_gtk_progress_bar = gtk_progress_bar_new ();
			gtk_widget_ensure_style (global_gtk_progress_bar);
			global_gtk_progress_bar_style = gtk_rc_get_style (global_gtk_progress_bar);
			
			global_gtk_radio_button = gtk_radio_button_new (IntPtr.Zero);
			gtk_widget_ensure_style (global_gtk_radio_button);
			global_gtk_radio_button_style = gtk_rc_get_style (global_gtk_radio_button);
			
			global_gtk_check_button = gtk_check_button_new ();
			gtk_widget_ensure_style (global_gtk_check_button);
			global_gtk_check_button_style = gtk_rc_get_style (global_gtk_check_button);
			
			global_gtk_hscale = gtk_hscale_new (adj);
			gtk_widget_ensure_style (global_gtk_hscale);
			global_gtk_hscale_style = gtk_rc_get_style (global_gtk_hscale);
			
			global_gtk_vscale = gtk_vscale_new (adj);
			gtk_widget_ensure_style (global_gtk_vscale);
			global_gtk_vscale_style = gtk_rc_get_style (global_gtk_vscale);
			
			#if _EXPERIMENTAL_
			global_color_map = gdk_colormap_get_system ();
			#endif
		}

		public void LoadSysDefaultColors ()
		{
			GtkStyleStruct style_struct; 			
			
			style_struct = (GtkStyleStruct) Marshal.PtrToStructure (global_style, typeof (GtkStyleStruct));			
			defaultWindowBackColor = ColorFromGdkColor (style_struct.bg[0]);
			defaultWindowForeColor = ColorFromGdkColor (style_struct.fg[0]);
		}

		public ThemeGtk () : base ()
		{
			Console.WriteLine ("ThemeGtk constructor");
			InitGtk ();
			
			LoadSysDefaultColors ();	
		}	

		public override bool DoubleBufferingSupported {
			#if _EXPERIMENTAL_
			get {return true; }
			#else
			get {return false; }
			#endif
		}
		
		private void SetDrawableAndStyle (Control control)
		{
			#if _EXPERIMENTAL_
			if (current_gdk_drawable != IntPtr.Zero) {
				g_object_unref (current_gdk_drawable);
				current_gdk_drawable = IntPtr.Zero;
			}
			current_gdk_drawable = gdk_pixmap_new (IntPtr.Zero, control.ClientRectangle.Width, control.ClientRectangle.Height, 24);
			#else
			current_gdk_drawable = gdk_window_foreign_new_for_display (gdkdisplay, (uint) control.Handle);
			#endif
			
			IntPtr tmp_style = IntPtr.Zero;
			
			if (control is ButtonBase) {
				tmp_style = global_gtk_button_style;
				current_widget = global_gtk_button;
			} else
			if (control is ScrollBar) {
				ScrollBar bar = control as ScrollBar;
				if (bar.vert) {
					tmp_style = global_gtk_vscrollbar_style;
					current_widget = global_gtk_vscrollbar;
				} else {
					tmp_style = global_gtk_hscrollbar_style;
					current_widget = global_gtk_hscrollbar;
				}
			} else
			if (control is ProgressBar) {
				tmp_style = global_gtk_progress_bar_style;
				current_widget = global_gtk_progress_bar;
			} else
			if (control is RadioButton) {
				tmp_style = global_gtk_radio_button_style;
				current_widget = global_gtk_radio_button;
			} else
			if (control is CheckBox) {
				tmp_style = global_gtk_check_button_style;
				current_widget = global_gtk_check_button;
			} else
			if (control is TrackBar) {
				TrackBar bar = control as TrackBar;
				if (bar.Orientation == Orientation.Vertical) {
					tmp_style = global_gtk_vscale_style;
					current_widget = global_gtk_vscale;
				} else {
					tmp_style = global_gtk_hscale_style;
					current_widget = global_gtk_hscale;
				}
			} else
				tmp_style = global_style;
			
			current_style = gtk_style_attach (tmp_style, current_gdk_drawable);  // need it
		}
		
		#if _EXPERIMENTAL_
		private void SetDrawableAndStyle (Rectangle area, Type type, Orientation orientation)
		{
			if (current_gdk_drawable != IntPtr.Zero) {
				g_object_unref (current_gdk_drawable);
				current_gdk_drawable = IntPtr.Zero;
			}
			current_gdk_drawable = gdk_pixmap_new (IntPtr.Zero, area.Width, area.Height, 24);
			
			IntPtr tmp_style = IntPtr.Zero;
			
			if (type == typeof(ButtonBase)) {
				tmp_style = global_gtk_button_style;
				current_widget = global_gtk_button;
			} else
			if (type == typeof(ScrollBar)) {
				if (orientation == Orientation.Vertical) {
					tmp_style = global_gtk_vscrollbar_style;
					current_widget = global_gtk_vscrollbar;
				} else {
					tmp_style = global_gtk_hscrollbar_style;
					current_widget = global_gtk_hscrollbar;
				}
			} else
			if (type == typeof(ProgressBar)) {
				tmp_style = global_gtk_progress_bar_style;
				current_widget = global_gtk_progress_bar;
			} else
			if (type == typeof(RadioButton)) {
				tmp_style = global_gtk_radio_button_style;
				current_widget = global_gtk_radio_button;
			} else
			if (type == typeof(CheckBox)) {
				tmp_style = global_gtk_check_button_style;
				current_widget = global_gtk_check_button;
			} else
			if (type == typeof(TrackBar)) {
				if (orientation == Orientation.Vertical) {
					tmp_style = global_gtk_vscale_style;
					current_widget = global_gtk_vscale;
				} else {
					tmp_style = global_gtk_hscale_style;
					current_widget = global_gtk_hscale;
				}
			} else
				tmp_style = global_style;
			
			current_style = gtk_style_attach (tmp_style, current_gdk_drawable);  // need it
		}
		#endif
		
		#if _EXPERIMENTAL_
		private void DrawDrawableToDC (Graphics dc, Control control)
		{
			IntPtr new_pixbuf = gdk_pixbuf_new (0, true, 8, control.ClientRectangle.Width, control.ClientRectangle.Height);
			
			gdk_pixbuf_get_from_drawable (new_pixbuf,
						      current_gdk_drawable,
						      global_color_map,
						      0,
						      0,
						      0,
						      0,
						      -1,
						      -1);
			
			IntPtr error = IntPtr.Zero;
			IntPtr buffer;
			UIntPtr buffer_size_as_ptr;
			string type = "png";
			
			bool saved = gdk_pixbuf_save_to_buffer (new_pixbuf, out buffer, out buffer_size_as_ptr, type, out error, IntPtr.Zero);
			
			if (!saved)
				return;
			
			int buffer_size = (int) (uint) buffer_size_as_ptr;
			byte[] result = new byte [buffer_size];
			Marshal.Copy (buffer, result, 0, (int) buffer_size);
			g_free (buffer);
			g_object_unref (new_pixbuf);
			
			Image image = null;
			using (MemoryStream s = new MemoryStream (result))
				image = Image.FromStream (s);
			
			dc.DrawImage (image, control.ClientRectangle);
		}
		
		private void DrawDrawableToDC (Graphics dc, Rectangle area)
		{
			IntPtr new_pixbuf = gdk_pixbuf_new (0, true, 8, area.Width, area.Height);
			
			gdk_pixbuf_get_from_drawable (new_pixbuf,
						      current_gdk_drawable,
						      global_color_map,
						      0,
						      0,
						      0,
						      0,
						      -1,
						      -1);
			
			IntPtr error = IntPtr.Zero;
			IntPtr buffer;
			UIntPtr buffer_size_as_ptr;
			string type = "png";
			
			bool saved = gdk_pixbuf_save_to_buffer (new_pixbuf, out buffer, out buffer_size_as_ptr, type, out error, IntPtr.Zero);
			
			if (!saved)
				return;
			
			int buffer_size = (int) (uint) buffer_size_as_ptr;
			byte[] result = new byte [buffer_size];
			Marshal.Copy (buffer, result, 0, (int) buffer_size);
			g_free (buffer);
			g_object_unref (new_pixbuf);
			
			Image image = null;
			using (MemoryStream s = new MemoryStream (result))
				image = Image.FromStream (s);
			
			dc.DrawImage (image, area);
		}
		#endif
		
		public override void DrawButtonBase (Graphics dc, Rectangle clip_area, ButtonBase button)
		{
			SetDrawableAndStyle (button);
			
			// Draw the button: fill rectangle, draw border, etc.
			ButtonBase_DrawButton (button, dc);
			
			// First, draw the image
			if ((button.image != null) || (button.image_list != null))
				ButtonBase_DrawImage (button, dc);
			
			// Draw the focus rectangle
			if (button.has_focus)
				ButtonBase_DrawFocus (button, dc);
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, button);
			#endif

			// Now the text
			if (button.Text != null && button.Text != String.Empty)
				ButtonBase_DrawText (button, dc);
		}
		
		protected override void ButtonBase_DrawButton(ButtonBase button, Graphics dc)
		{
			Rectangle buttonRectangle = button.ClientRectangle;
			
			StateType state_type = StateType.Normal;
			ShadowType shadow_type = button.FlatStyle == FlatStyle.Flat ? ShadowType.In : ShadowType.Out;
			string detail = "buttondefault";
			
			if (((button is CheckBox) && (((CheckBox)button).check_state == CheckState.Checked)) ||
			    ((button is RadioButton) && (((RadioButton)button).check_state == CheckState.Checked))) {
				state_type = StateType.Active;
				shadow_type = ShadowType.In;
				detail = "button";
			} else
			if (!button.is_enabled) {
				state_type = StateType.Insensitive;
			} else
			if (button.is_pressed) {
				state_type = StateType.Active;
				shadow_type = ShadowType.In;
				detail = "button";
			} else
			if (button.is_entered) {
				state_type = StateType.Prelight;
			}
			
			if (button.Focused)
				gtk_widget_grab_focus (global_gtk_button);
			
			if (button.FlatStyle == FlatStyle.Flat)
				gtk_paint_flat_box (current_style,
						    current_gdk_drawable,
						    (int) state_type,
						    (int) shadow_type,
						    IntPtr.Zero,
						    global_gtk_button,
						    detail,
						    buttonRectangle.X, buttonRectangle.Y,
						    buttonRectangle.Width, buttonRectangle.Height);
			else
			if (button.FlatStyle != FlatStyle.Popup || (button.FlatStyle == FlatStyle.Popup && button.is_entered))
				gtk_paint_box (current_style,
					       current_gdk_drawable,
					       (int) state_type,
					       (int) shadow_type,
					       IntPtr.Zero,
					       global_gtk_button,
					       detail,
					       buttonRectangle.X, buttonRectangle.Y,
					       buttonRectangle.Width, buttonRectangle.Height);
		}
		
		protected override void ButtonBase_DrawFocus (ButtonBase button, Graphics dc)
		{
			if (!button.is_enabled)
				return;
			
			Rectangle focus_rect = new Rectangle (button.ClientRectangle.X + 4, button.ClientRectangle.Y + 4, button.ClientRectangle.Width - 9, button.ClientRectangle.Height - 9);
			
			gtk_widget_grab_focus (global_gtk_button);
			
			gtk_paint_focus (current_style,
					 current_gdk_drawable,
					 (int) StateType.Active,
					 IntPtr.Zero,
					 global_gtk_button,
					 "button",
					 focus_rect.X,
					 focus_rect.Y,
					 focus_rect.Width,
					 focus_rect.Height);
		}
		
		#region ScrollBar
		public override void DrawScrollBar( Graphics dc, Rectangle clip, ScrollBar bar ) {
			int		scrollbutton_width = bar.scrollbutton_width;
			int		scrollbutton_height = bar.scrollbutton_height;
			Rectangle	first_arrow_area;
			Rectangle	second_arrow_area;			
			Rectangle	thumb_pos;
			
			SetDrawableAndStyle (bar);
			
			Rectangle allocation = new Rectangle (bar.ClientRectangle.X, bar.ClientRectangle.Y, bar.ClientRectangle.Width, bar.ClientRectangle.Height);
			
			// fix for artefacts
			Color fix_color = bar.Parent != null ? bar.Parent.BackColor : ColorControl;
			
			if (bar.vert) {
				gtk_widget_size_allocate (global_gtk_vscrollbar, ref allocation);
				
				// fix for artefacts
				dc.FillRectangle (ResPool.GetSolidBrush (fix_color), 
						  bar.ClientRectangle.X, bar.ClientRectangle.Y, bar.ClientRectangle.Width, 3);
				dc.FillRectangle (ResPool.GetSolidBrush (fix_color), 
						  bar.ClientRectangle.X, bar.ClientRectangle.Bottom - 4, bar.ClientRectangle.Width, 3);
			} else {
				gtk_widget_size_allocate (global_gtk_hscrollbar, ref allocation);
				
				// fix for artefacts
				dc.FillRectangle (ResPool.GetSolidBrush (fix_color), 
						  bar.ClientRectangle.X, bar.ClientRectangle.Y, 3, bar.ClientRectangle.Height);
				dc.FillRectangle (ResPool.GetSolidBrush (fix_color), 
						  bar.ClientRectangle.Right - 4, bar.ClientRectangle.Y, 3, bar.ClientRectangle.Height);
			}
			
			thumb_pos = bar.ThumbPos;
			
			if ( bar.vert ) {
				first_arrow_area = new Rectangle( 0, 0, bar.Width, scrollbutton_height + 1 );
				bar.FirstArrowArea = first_arrow_area;
				
				second_arrow_area = new Rectangle( 0, bar.ClientRectangle.Height - scrollbutton_height - 1, bar.Width, scrollbutton_height + 1 );
				bar.SecondArrowArea = second_arrow_area;
				
				thumb_pos.Width = bar.Width;
				bar.ThumbPos = thumb_pos;
				
				ScrollBar_Vertical_Draw_ThumbMoving_None (scrollbutton_height, bar, clip, dc);
				
				/* Buttons */
				if ( clip.IntersectsWith( first_arrow_area ) )
					CPDrawScrollButton( dc, first_arrow_area, ScrollButton.Up, bar.firstbutton_state );
				if ( clip.IntersectsWith( second_arrow_area ) )
					CPDrawScrollButton( dc, second_arrow_area, ScrollButton.Down, bar.secondbutton_state );
			} else {
				first_arrow_area = new Rectangle( 0, 0, scrollbutton_width + 1, bar.Height );
				bar.FirstArrowArea = first_arrow_area;
				
				second_arrow_area = new Rectangle( bar.ClientRectangle.Width - scrollbutton_width - 1, 0, scrollbutton_width + 1, bar.Height );
				bar.SecondArrowArea = second_arrow_area;
				
				thumb_pos.Height = bar.Height;
				bar.ThumbPos = thumb_pos;
				
				/* Background */					
				ScrollBar_Horizontal_Draw_ThumbMoving_None (scrollbutton_width, bar, clip, dc);
				
				/* Buttons */
				if ( clip.IntersectsWith( first_arrow_area ) )
					CPDrawScrollButton( dc, first_arrow_area, ScrollButton.Left, bar.firstbutton_state );
				if ( clip.IntersectsWith( second_arrow_area ) )
					CPDrawScrollButton( dc, second_arrow_area, ScrollButton.Right, bar.secondbutton_state );
			}
			
			/* Thumb */
			ScrollBar_DrawThumb( bar, thumb_pos, clip, dc );
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, bar);
			#endif
		}
		
		protected override void ScrollBar_DrawThumb( ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc ) {
			if ( bar.Enabled)
				DrawScrollBarThumb( dc, thumb_pos, bar );
		}
		
		protected void ScrollBar_Vertical_Draw_ThumbMoving_None (int scrollbutton_height, ScrollBar bar, Rectangle clip, Graphics dc)
		{
			Rectangle r = new Rectangle (0,
						     scrollbutton_height, bar.ClientRectangle.Width, bar.ClientRectangle.Height - (scrollbutton_height * 2));
			gtk_paint_box (current_style, 
				       current_gdk_drawable, 
				       (int) StateType.Active,
				       (int) ShadowType.In,
				       IntPtr.Zero,
				       global_gtk_vscrollbar,
				       "vscrollbar",
				       r.X, r.Y,
				       r.Width, r.Height);
		}
		
		protected void ScrollBar_Horizontal_Draw_ThumbMoving_None (int scrollbutton_width, ScrollBar bar, Rectangle clip, Graphics dc)
		{
			Rectangle r = new Rectangle (scrollbutton_width,
						     0, bar.ClientRectangle.Width - (scrollbutton_width * 2), bar.ClientRectangle.Height);
			
			gtk_paint_box (current_style, 
				       current_gdk_drawable, 
				       (int) StateType.Active,
				       (int) ShadowType.In,
				       IntPtr.Zero,
				       global_gtk_hscrollbar,
				       "hscrollbar",
				       r.X, r.Y,
				       r.Width, r.Height);
		}
		
		private void DrawScrollBarThumb( Graphics dc, Rectangle area, ScrollBar bar ) {
			IntPtr gtk_scrollbar = bar.vert ? global_gtk_vscrollbar : global_gtk_hscrollbar;
			
			gtk_paint_box (current_style, 
				       current_gdk_drawable, 
				       (int) StateType.Active,
				       (int) ShadowType.Out,
				       IntPtr.Zero,
				       gtk_scrollbar,
				       "slider",
				       area.X, area.Y,
				       area.Width, area.Height);
		}
		#endregion	// ScrollBar
		
		#region ProgressBar
		public override void DrawProgressBar (Graphics dc, Rectangle clip_rect, ProgressBar ctrl)
		{
			Rectangle	client_area = ctrl.client_area;
			int		barpos_pixels;
			
			SetDrawableAndStyle (ctrl);
			
			// draw background
			gtk_paint_box (current_style,
				       current_gdk_drawable,
				       (int) StateType.Normal,
				       (int) ShadowType.In, 
				       IntPtr.Zero,
				       global_gtk_progress_bar,
				       "trough",
				       ctrl.ClientRectangle.X, 
				       ctrl.ClientRectangle.Y,
				       ctrl.ClientRectangle.Width,
				       ctrl.ClientRectangle.Height);
			
			// don't draw the bar if Value is = 0
			if (ctrl.Value <= 0)
				return;
			
			int value = ctrl.Value;
			
			if (value > ctrl.Maximum)
				value = ctrl.Maximum;
			
			if (value == ctrl.Maximum)
				barpos_pixels = client_area.Width + 2;
			else
				barpos_pixels = (((value - ctrl.Minimum) * client_area.Width) / (ctrl.Maximum - ctrl.Minimum)) + 1;
			
			gtk_paint_box (current_style, 
				       current_gdk_drawable, 
				       (int) StateType.Prelight,
				       (int) ShadowType.Out,
				       IntPtr.Zero,
				       global_gtk_progress_bar,
				       "bar",
				       client_area.X - 1, client_area.Y - 1,
				       barpos_pixels, client_area.Height + 2);
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, ctrl);
			#endif
		}
		#endregion	// ProgressBar
		
		#region RadioButton
		protected override void RadioButton_DrawButton (RadioButton radio_button, Graphics dc, ButtonState state, Rectangle radiobutton_rectangle)
		{
			// we currently don't care for flat or popup radio buttons
			if (radio_button.appearance == Appearance.Button) {
				DrawButtonBase (dc, radio_button.ClientRectangle, radio_button);
			} else {
				DrawRadioButton (dc, radio_button, state, radiobutton_rectangle);
			}
		}
		
		private void DrawRadioButton (Graphics dc, RadioButton radio_button, ButtonState state, Rectangle radiobutton_rectangle)
		{
			SetDrawableAndStyle (radio_button);
			
			ShadowType shadow_type;
			
			if (!radio_button.Enabled)
				shadow_type = ShadowType.Out;
			else
				shadow_type = radio_button.Checked ? ShadowType.In : ShadowType.EtchedIn;
			
			StateType state_type = StateType.Normal;
			
			if (!radio_button.Enabled)
				state_type = StateType.Insensitive;
			else
			if (radio_button.is_pressed)
				state_type = StateType.Active;
			else
			if (radio_button.is_entered)
				state_type = StateType.Prelight;
			
			gtk_paint_option (current_style,
					  current_gdk_drawable,
					  (int) state_type,
					  (int) shadow_type,
					  IntPtr.Zero,
					  global_gtk_radio_button,
					  "radiobutton",
					  radiobutton_rectangle.X,
					  radiobutton_rectangle.Y,
					  radiobutton_rectangle.Width,
					  radiobutton_rectangle.Height);
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, radio_button);
			#endif
		}
		
		protected override void RadioButton_DrawText (RadioButton radio_button, Rectangle text_rectangle, Graphics dc, StringFormat text_format)
		{
			if (radio_button.Appearance != Appearance.Button)
				base.RadioButton_DrawText (radio_button, text_rectangle, dc, text_format);
		}
	
		protected override void RadioButton_DrawFocus (RadioButton radio_button, Graphics dc, Rectangle text_rectangle)
		{
			if (radio_button.Focused && radio_button.appearance != Appearance.Button) {
				gtk_paint_focus (current_style,
						 current_gdk_drawable,
						 (int) StateType.Active,
						 IntPtr.Zero,
						 global_gtk_radio_button,
						 "radiobutton",
						 text_rectangle.X,
						 text_rectangle.Y,
						 text_rectangle.Width,
						 text_rectangle.Height);
			}
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, radio_button);
			#endif
		}
		#endregion	// RadioButton
		
		#region CheckBox
		protected override void CheckBox_DrawCheckBox (Graphics dc, CheckBox checkbox, ButtonState state, Rectangle checkbox_rectangle)
		{
			// render as per normal button
			if (checkbox.appearance == Appearance.Button) {
				DrawButtonBase (dc, checkbox.ClientRectangle, checkbox);
			} else {
				InternalDrawCheckBox (dc, checkbox, state, checkbox_rectangle);
			}
		}
		
		private void InternalDrawCheckBox (Graphics dc, CheckBox checkbox, ButtonState state, Rectangle checkbox_rectangle)
		{
			SetDrawableAndStyle (checkbox);
			
			ShadowType shadow_type;
			
			if (!checkbox.Enabled)
				shadow_type = ShadowType.Out;
			else
				shadow_type = checkbox.Checked ? ShadowType.In : ShadowType.EtchedIn;
			
			StateType state_type = StateType.Normal;
			
			if (!checkbox.Enabled)
				state_type = StateType.Insensitive;
			else
			if (checkbox.is_pressed)
				state_type = StateType.Active;
			else
			if (checkbox.is_entered)
				state_type = StateType.Prelight;
			
			gtk_paint_check (current_style,
					 current_gdk_drawable,
					 (int) state_type,
					 (int) shadow_type,
					 IntPtr.Zero,
					 global_gtk_check_button,
					 "checkbutton",
					 checkbox_rectangle.X,
					 checkbox_rectangle.Y,
					 checkbox_rectangle.Width,
					 checkbox_rectangle.Height);
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, checkbox);
			#endif
		}
		
		protected override void CheckBox_DrawText (CheckBox checkbox, Rectangle text_rectangle, Graphics dc, StringFormat text_format)
		{
			if (checkbox.Appearance != Appearance.Button)
				base.CheckBox_DrawText (checkbox, text_rectangle, dc, text_format);
		}
		
		protected override void CheckBox_DrawFocus( CheckBox checkbox, Graphics dc, Rectangle text_rectangle )
		{
			if (checkbox.Focused && checkbox.appearance != Appearance.Button) {
				gtk_paint_focus (current_style,
						 current_gdk_drawable,
						 (int) StateType.Active,
						 IntPtr.Zero,
						 global_gtk_check_button,
						 "checkbutton",
						 text_rectangle.X,
						 text_rectangle.Y,
						 text_rectangle.Width,
						 text_rectangle.Height);
			}
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, checkbox);
			#endif
		}
		#endregion	// CheckBox
		
		#region	TrackBar
		private void DrawTrackBar_Vertical (Graphics dc, Rectangle clip_rectangle, TrackBar tb,
						    ref Rectangle thumb_pos, ref Rectangle thumb_area,
						    float ticks, int value_pos, bool mouse_value)
		{			
			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			const int space_from_right = 8;
			const int space_from_left = 8;
			Rectangle area = tb.ClientRectangle;
			
			Rectangle allocation = new Rectangle (area.X, area.Y, area.Width, area.Height);
			
			gtk_widget_size_allocate (current_widget, ref allocation);
			
			gtk_range_set_range (current_widget, tb.Minimum, tb.Maximum);
			gtk_range_set_value (current_widget, tb.Value);
			
			ShadowType shadow_type = ShadowType.In;
			
			if (!tb.Enabled)
				shadow_type = ShadowType.Out;
			
			StateType state_type = StateType.Normal;
			
			if (!tb.Enabled)
				state_type = StateType.Insensitive;
			else
			if (tb.is_entered)
				state_type = StateType.Prelight;
			
			switch (tb.TickStyle) 	{
			case TickStyle.BottomRight:
			case TickStyle.None:
				channel_startpoint.Y = 8;
				channel_startpoint.X = 9;
				bottomtick_startpoint.Y = 13;
				bottomtick_startpoint.X = 24;				
				break;
			case TickStyle.TopLeft:
				channel_startpoint.Y = 8;
				channel_startpoint.X = 19;
				toptick_startpoint.Y = 13;
				toptick_startpoint.X = 8;
				break;
			case TickStyle.Both:
				channel_startpoint.Y = 8;
				channel_startpoint.X = 18;	
				bottomtick_startpoint.Y = 13;
				bottomtick_startpoint.X = 32;				
				toptick_startpoint.Y = 13;
				toptick_startpoint.X = 8;				
				break;
			default:
				break;
			}
			
			thumb_area.X = area.X + channel_startpoint.X;
			thumb_area.Y = area.Y + channel_startpoint.Y;
			thumb_area.Height = area.Height - space_from_right - space_from_left;
			thumb_area.Width = 4;
			
			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / (tb.Maximum - tb.Minimum);
			
			/* Convert thumb position from mouse position to value*/
			if (mouse_value) {
				
				if (value_pos >= channel_startpoint.Y)
					value_pos = (int)(((float) (value_pos - channel_startpoint.Y)) / pixels_betweenticks);
				else
					value_pos = 0;			
				
				if (value_pos + tb.Minimum > tb.Maximum)
					value_pos = tb.Maximum - tb.Minimum;
                                
				tb.Value = value_pos + tb.Minimum;
			}		
			
			thumb_pos.Width = 13;
			thumb_pos.Height = 29;
			
			thumb_pos.Y = channel_startpoint.Y + (int) (pixels_betweenticks * (float) value_pos) - (thumb_pos.Height / 3);
			
			if (thumb_pos.Y < channel_startpoint.Y)
				thumb_pos.Y = channel_startpoint.Y;
			
			if (thumb_pos.Y > thumb_area.Bottom - 29)
				thumb_pos.Y = thumb_area.Bottom - 29;
			
			/* Draw channel */
			gtk_paint_box (current_style,
				       current_gdk_drawable,
				       (int)state_type,
				       (int)shadow_type,
				       IntPtr.Zero,
				       current_widget,
				       "trough",
				       thumb_area.X,
				       thumb_area.Y,
				       4,
				       thumb_area.Height);
			
			/* Draw thumb */
			thumb_pos.X = channel_startpoint.X + 2 - thumb_pos.Width / 2;
			
			shadow_type = ShadowType.Out;
			
			gtk_paint_slider (current_style,
					  current_gdk_drawable,
					  (int)state_type,
					  (int)shadow_type,
					  IntPtr.Zero,
					  current_widget,
					  "vscale",
					  thumb_pos.X,
					  thumb_pos.Y,
					  thumb_pos.Width,
					  thumb_pos.Height,
					  0);
			
			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / ticks;
			
			/* Draw ticks*/
			thumb_area.X = thumb_pos.X;
			thumb_area.Y = channel_startpoint.Y;
			thumb_area.Width = thumb_pos.Width;
			
			Region outside = new Region (area);
			outside.Exclude (thumb_area);			
			
			if (outside.IsVisible (clip_rectangle)) {				
				if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight ||
				    ((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {	
					
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) 	{					
						if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len + 1)
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + bottomtick_startpoint.X , area.Y + bottomtick_startpoint.Y  + inc, 
								     area.X + bottomtick_startpoint.X  + 3, area.Y + bottomtick_startpoint.Y + inc);
						else
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + bottomtick_startpoint.X, area.Y + bottomtick_startpoint.Y  + inc, 
								     area.X + bottomtick_startpoint.X  + 2, area.Y + bottomtick_startpoint.Y + inc);
					}
				}
				
				if (pixels_betweenticks > 0 &&  ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft ||
				    ((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {
					
					pixel_len = thumb_area.Height - 11;
					pixels_betweenticks = pixel_len / ticks;
					
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
						if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len + 1)
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + toptick_startpoint.X  - 3 , area.Y + toptick_startpoint.Y + inc, 
								     area.X + toptick_startpoint.X, area.Y + toptick_startpoint.Y + inc);
						else
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + toptick_startpoint.X  - 2, area.Y + toptick_startpoint.Y + inc, 
								     area.X + toptick_startpoint.X, area.Y + toptick_startpoint.Y  + inc);
					}			
				}
			}
			
			outside.Dispose ();
		}
		
		private void DrawTrackBar_Horizontal (Graphics dc, Rectangle clip_rectangle, TrackBar tb,
						      ref Rectangle thumb_pos, ref Rectangle thumb_area,
						      float ticks, int value_pos, bool mouse_value)
		{			
			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			const int space_from_right = 8;
			const int space_from_left = 8;
			Rectangle area = tb.ClientRectangle;
			
			Rectangle allocation = new Rectangle (area.X, area.Y, area.Width, area.Height);
			
			gtk_widget_size_allocate (current_widget, ref allocation);
			
			gtk_range_set_range (current_widget, tb.Minimum, tb.Maximum);
			gtk_range_set_value (current_widget, tb.Value);
			
			ShadowType shadow_type = ShadowType.In;
			
			if (!tb.Enabled)
				shadow_type = ShadowType.Out;
			
			StateType state_type = StateType.Normal;
			
			if (!tb.Enabled)
				state_type = StateType.Insensitive;
			else
			if (tb.is_entered)
				state_type = StateType.Prelight;
			
			switch (tb.TickStyle) {
			case TickStyle.BottomRight:
			case TickStyle.None:
				channel_startpoint.X = 8;
				channel_startpoint.Y = 9;
				bottomtick_startpoint.X = 13;
				bottomtick_startpoint.Y = 24;				
				break;
			case TickStyle.TopLeft:
				channel_startpoint.X = 8;
				channel_startpoint.Y = 19;
				toptick_startpoint.X = 13;
				toptick_startpoint.Y = 8;
				break;
			case TickStyle.Both:
				channel_startpoint.X = 8;
				channel_startpoint.Y = 18;	
				bottomtick_startpoint.X = 13;
				bottomtick_startpoint.Y = 32;				
				toptick_startpoint.X = 13;
				toptick_startpoint.Y = 8;				
				break;
			default:
				break;
			}
			
			thumb_area.X = area.X + channel_startpoint.X;
			thumb_area.Y = area.Y + channel_startpoint.Y;
			thumb_area.Width = area.Width - space_from_right - space_from_left;
			thumb_area.Height = 4;
			
			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / (tb.Maximum - tb.Minimum);
			
			/* Convert thumb position from mouse position to value*/
			if (mouse_value) {			
				if (value_pos >= channel_startpoint.X)
					value_pos = (int)(((float) (value_pos - channel_startpoint.X)) / pixels_betweenticks);
				else
					value_pos = 0;				
				
				if (value_pos + tb.Minimum > tb.Maximum)
					value_pos = tb.Maximum - tb.Minimum;
                                
				tb.Value = value_pos + tb.Minimum;
			}			
			
			thumb_pos.Width = 29;
			thumb_pos.Height = 13;
			
			thumb_pos.X = channel_startpoint.X + (int) (pixels_betweenticks * (float) value_pos) - (thumb_pos.Width / 3);
			
			
			if (thumb_pos.X < channel_startpoint.X)
				thumb_pos.X = channel_startpoint.X;
			
			if (thumb_pos.X > thumb_area.Right - 29)
				thumb_pos.X = thumb_area.Right - 29;
			
			/* Draw channel */
			gtk_paint_box (current_style,
				       current_gdk_drawable,
				       (int)state_type,
				       (int)shadow_type,
				       IntPtr.Zero,
				       current_widget,
				       "trough",
				       thumb_area.X,
				       thumb_area.Y,
				       thumb_area.Width,
				       4);
			
			/* Draw thumb */
			
			thumb_pos.Y = channel_startpoint.Y + 2 - thumb_pos.Height / 2;
			
			shadow_type = ShadowType.Out;
			
			gtk_paint_slider (current_style,
					  current_gdk_drawable,
					  (int)state_type,
					  (int)shadow_type,
					  IntPtr.Zero,
					  current_widget,
					  "hscale",
					  thumb_pos.X,
					  thumb_pos.Y,
					  thumb_pos.Width,
					  thumb_pos.Height,
					  0);
			
			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / ticks;
			
			/* Draw ticks*/
			thumb_area.Y = thumb_pos.Y;
			thumb_area.X = channel_startpoint.X;
			thumb_area.Height = thumb_pos.Height;
			Region outside = new Region (area);
			outside.Exclude (thumb_area);			
			
			if (outside.IsVisible (clip_rectangle)) {				
				if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight ||
				    ((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {				
					
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
						if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len + 1)
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + bottomtick_startpoint.X + inc , area.Y + bottomtick_startpoint.Y, 
								     area.X + bottomtick_startpoint.X + inc , area.Y + bottomtick_startpoint.Y + 3);
						else
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + bottomtick_startpoint.X + inc, area.Y + bottomtick_startpoint.Y, 
								     area.X + bottomtick_startpoint.X + inc, area.Y + bottomtick_startpoint.Y + 2);
					}
				}
				
				if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft ||
				    ((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {
					
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
						if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len + 1)
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + toptick_startpoint.X + inc , area.Y + toptick_startpoint.Y - 3, 
								     area.X + toptick_startpoint.X + inc , area.Y + toptick_startpoint.Y);
						else
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + toptick_startpoint.X + inc, area.Y + toptick_startpoint.Y - 2, 
								     area.X + toptick_startpoint.X + inc, area.Y + toptick_startpoint.Y);
					}			
				}
			}
			
			outside.Dispose ();			
		}
		
		public override void DrawTrackBar (Graphics dc, Rectangle clip_rectangle, TrackBar tb)
		{
			int		value_pos;
			bool		mouse_value;
			float		ticks = (tb.Maximum - tb.Minimum) / tb.tickFrequency; /* N of ticks draw*/
			Rectangle	area;
			Rectangle	thumb_pos = tb.ThumbPos;
			Rectangle	thumb_area = tb.ThumbArea;
			
			if (tb.thumb_pressed) {
				value_pos = tb.thumb_mouseclick;
				mouse_value = true;
			} else {
				value_pos = tb.Value - tb.Minimum;
				mouse_value = false;
			}
			
			area = tb.ClientRectangle;
			
			SetDrawableAndStyle (tb);
			
			/* Control Background */
			if (tb.BackColor == DefaultControlBackColor) {
				dc.FillRectangle (ResPool.GetSolidBrush (ColorControl), clip_rectangle);
			} else {
				dc.FillRectangle (ResPool.GetSolidBrush (tb.BackColor), clip_rectangle);
			}
			
			if (tb.Orientation == Orientation.Vertical) {
				DrawTrackBar_Vertical (dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
						       ticks, value_pos, mouse_value);
				
			} else {
				DrawTrackBar_Horizontal (dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
							 ticks, value_pos, mouse_value);
			}
			
			if (tb.Enabled && tb.Focused)
				gtk_paint_focus (current_style,
						 current_gdk_drawable, 
						 (int)StateType.Normal,
						 IntPtr.Zero,
						 current_widget, 
						 "trough",
						 area.X,
						 area.Y,
						 area.Width,
						 area.Height);
			
			tb.ThumbPos = thumb_pos;
			tb.ThumbArea = thumb_area;
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, tb);
			#endif
		}
		#endregion	// TrackBar
		
		public override void CPDrawButton (Graphics dc, Rectangle rectangle, ButtonState state)
		{
			#if _EXPERIMENTAL_
			SetDrawableAndStyle (rectangle, typeof(ButtonBase), Orientation.Horizontal);
			#endif
			
			bool is_pushed = false;
//			bool is_checked = false;
//			bool is_flat = false;
			bool is_inactive = false;
			
			if ((state & ButtonState.Pushed) != 0) {
				is_pushed = true;
			}
			
//			if ((state & ButtonState.Checked) != 0) {
//				is_checked = true;
//			}
//			
//			if ((state & ButtonState.Flat) != 0) {
//				is_flat = true;
//			}
			
			if ((state & ButtonState.Inactive) != 0) {
				is_inactive = true;
			}
			
			IntPtr drawbutton_style = gtk_style_attach (global_gtk_button_style, current_gdk_drawable);  // need it
			
			StateType state_type = StateType.Normal;
			ShadowType shadow_type = ShadowType.Out;
			string detail = "buttondefault";
			
			if (is_inactive) {
				state_type = StateType.Insensitive;
			} else
			if (is_pushed) {
				state_type = StateType.Active;
				shadow_type = ShadowType.In;
				detail = "button";
			}
			
			gtk_paint_box (drawbutton_style, current_gdk_drawable,
				       (int) state_type,
				       (int) shadow_type,
				       IntPtr.Zero,
				       IntPtr.Zero,
				       detail,
				       rectangle.X, rectangle.Y,
				       rectangle.Width, rectangle.Height);
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, rectangle);
			#endif
		}
		
		/* Scroll button: regular button + direction arrow */
		public override void CPDrawScrollButton (Graphics dc, Rectangle area, ScrollButton scroll_button_type, ButtonState state)
		{
			#if _EXPERIMENTAL_
			Orientation orientation = Orientation.Vertical;
			if (scroll_button_type == ScrollButton.Left || scroll_button_type == ScrollButton.Right)
				orientation = Orientation.Horizontal;
			SetDrawableAndStyle (area, typeof(ScrollBar), orientation);
			#endif
			
			bool enabled = (state == ButtonState.Inactive) ? false: true;
			
			StateType state_type = enabled ? StateType.Normal : StateType.Insensitive;
			
			DrawScrollButtonPrimitive (dc, area, state, scroll_button_type);
			
			if (area.Width < 12 || area.Height < 12) /* Cannot see a thing at smaller sizes */
				return;
			
			ArrowType arrow_type = 0;
			
			switch (scroll_button_type) {
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
			
			int centerX = area.Left + area.Width / 2;
			int centerY = area.Top + area.Height / 2;
			int arrow_x = 0, arrow_y = 0, arrow_height = 0, arrow_width = 0;
			
			switch (scroll_button_type) {
			case ScrollButton.Down:
			case ScrollButton.Up:
				arrow_x = centerX - 4;
				arrow_y = centerY - 2;
				arrow_width = 8;
				arrow_height = 4;
				break;
			case ScrollButton.Left:
			case ScrollButton.Right:
				arrow_x = centerX - 2;
				arrow_y = centerY - 4;
				arrow_width = 4;
				arrow_height = 8;
				break;
			default:
				break;
			}
			
			gtk_paint_arrow (current_style, 
					 current_gdk_drawable, 
					 (int) state_type,
					 (int) ShadowType.Out,
					 IntPtr.Zero,
					 current_widget,
					 "",		
					 (int) arrow_type, true, 
					 arrow_x, 
					 arrow_y,
					 arrow_width, arrow_height);
			
			current_widget = IntPtr.Zero;
			
			#if _EXPERIMENTAL_
			DrawDrawableToDC (dc, area);
			#endif
		}
		
		public void DrawScrollButtonPrimitive (Graphics dc, Rectangle area, ButtonState state, ScrollButton scroll_button_type)
		{
			StateType state_type = StateType.Normal;
			ShadowType shadow_type = ShadowType.Out;
			
			if ((state & ButtonState.Pushed) == ButtonState.Pushed) {
				state_type = StateType.Active;
				shadow_type = ShadowType.In;
			}
			
			switch (scroll_button_type) {
			case ScrollButton.Left:
			case ScrollButton.Right:
				gtk_paint_box (current_style, 
					       current_gdk_drawable, 
					       (int) state_type,
					       (int) shadow_type,
					       IntPtr.Zero,
					       global_gtk_hscrollbar,
					       "stepper",
					       area.X, area.Y,
					       area.Width, area.Height);
				break;
			case ScrollButton.Up:
			case ScrollButton.Down:
				gtk_paint_box (current_style, 
					       current_gdk_drawable, 
					       (int) state_type,
					       (int) shadow_type,
					       IntPtr.Zero,
					       global_gtk_vscrollbar,
					       "stepper",
					       area.X, area.Y,
					       area.Width, area.Height);
				break;
			}
		}
		
		private static Color ColorFromGdkColor (GdkColorStruct gtkcolor)
		{
			return Color.FromArgb (255, 
				(gtkcolor.red >> 8)  & 0xff, 
				(gtkcolor.green  >> 8) & 0xff,
				(gtkcolor.blue >> 8) & 0xff );
		}

	} //class
}
