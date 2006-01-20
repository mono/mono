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

// TODO: fix position of button focus rectangle
//

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
		static extern IntPtr gtk_hscrollbar_new(IntPtr adjustment);

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
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_button_new ();
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_progress_bar_new ();
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_radio_button_new (IntPtr group);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern IntPtr gtk_check_button_new ();

		/* GTK Drawing */
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_handle (IntPtr style, IntPtr window, int state_type, int shadow_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height, int orientation);

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
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_hline(IntPtr style, IntPtr window, int state_type, IntPtr area, IntPtr widget, string detail, int x1, int x2, int y);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_vline(IntPtr style, IntPtr window, int state_type, IntPtr area, IntPtr widget, string detail, int y1, int y2, int x);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_check(IntPtr style, IntPtr window, int state_type, int shadow_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_focus(IntPtr style, IntPtr window, int state_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_widget_size_allocate (IntPtr widget, ref Rectangle allocation);
		
		[DllImport("libgtk-x11-2.0.so")]
		static extern void gtk_paint_option (IntPtr style, IntPtr window, int state_type, int shadow_type, IntPtr area, IntPtr widget, string detail, int x, int y, int width, int height);
		
		/* Data */
		static protected IntPtr dispmgr;
		static protected IntPtr gdkdisplay;
		static protected IntPtr widget;
		static protected IntPtr style;
		
		IntPtr global_gtk_button = IntPtr.Zero;
		IntPtr global_gtk_button_style = IntPtr.Zero;
		
		IntPtr global_gtk_vscrollbar = IntPtr.Zero;
		IntPtr global_gtk_vscrollbar_style = IntPtr.Zero;
		
		IntPtr global_gtk_hscrollbar = IntPtr.Zero;
		IntPtr global_gtk_hscrollbar_style = IntPtr.Zero;
		
		IntPtr global_gtk_progress_bar = IntPtr.Zero;
		IntPtr global_gtk_progress_bar_style = IntPtr.Zero;
		
		IntPtr global_gtk_radio_button = IntPtr.Zero;
		IntPtr global_gtk_radio_button_style = IntPtr.Zero;
		
		IntPtr global_gtk_check_button = IntPtr.Zero;
		IntPtr global_gtk_check_button_style = IntPtr.Zero;
		
		IntPtr current_gdk_window = IntPtr.Zero;
		IntPtr current_style = IntPtr.Zero;

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

			XplatUIX11.GetInstance().SetDisplay (gdk_x11_display_get_xdisplay (gdkdisplay));
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

			always_draw_hotkeys = true;
			
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
		}	

		public override bool DoubleBufferingSupported {
			get {return false; }
		}
		
		protected override void ButtonBase_DrawButton(ButtonBase button, Graphics dc)
		{
			Rectangle buttonRectangle = button.ClientRectangle;
			
			IntPtr gdkwindow = gdk_window_foreign_new_for_display (gdkdisplay, (uint) button.Handle);
			
			IntPtr button_style = gtk_style_attach (global_gtk_button_style, gdkwindow);  // need it
			
			StateType state_type = StateType.Normal;
			ShadowType shadow_type = button.flat_style == FlatStyle.Flat ? ShadowType.In : ShadowType.Out;
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
			
			if (button.flat_style == FlatStyle.Flat)
				gtk_paint_flat_box (button_style, gdkwindow,
						    (int) state_type,
						    (int) shadow_type,
						    IntPtr.Zero,
						    IntPtr.Zero,
						    detail,
						    buttonRectangle.X, buttonRectangle.Y,
						    buttonRectangle.Width, buttonRectangle.Height);
			else
			if (button.flat_style != FlatStyle.Popup || (button.flat_style == FlatStyle.Popup && button.is_entered))
				gtk_paint_box (button_style, gdkwindow,
					       (int) state_type,
					       (int) shadow_type,
					       IntPtr.Zero,
					       IntPtr.Zero,
					       detail,
					       buttonRectangle.X, buttonRectangle.Y,
					       buttonRectangle.Width, buttonRectangle.Height);
		}
		
		protected override void ButtonBase_DrawFocus( ButtonBase button, Graphics dc ) {
			
			if ( !button.is_enabled)
				return;
			
			Rectangle focus_rect = new Rectangle( button.ClientRectangle.X + 4, button.ClientRectangle.Y + 4, button.ClientRectangle.Width - 9, button.ClientRectangle.Height - 9 );
			
			IntPtr gdkwindow = gdk_window_foreign_new_for_display (gdkdisplay, (uint) button.Handle);
			
			IntPtr button_style = gtk_style_attach (global_gtk_button_style, gdkwindow);  // need it
			
			gtk_paint_focus (button_style,
					 gdkwindow,
					 (int) StateType.Active,
					 IntPtr.Zero,
					 IntPtr.Zero,
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
			
			IntPtr gdkwindow = gdk_window_foreign_new_for_display (gdkdisplay, (uint) bar.Handle);
			
			IntPtr scrollbar_style;
			
			Rectangle allocation = new Rectangle (bar.ClientRectangle.X, bar.ClientRectangle.Y, bar.ClientRectangle.Width, bar.ClientRectangle.Height);
			
			// fix for artefacts
			Color fix_color = bar.Parent != null ? bar.Parent.BackColor : ColorControl;
			
			if (bar.vert) {
				scrollbar_style = global_gtk_vscrollbar_style;
				
				gtk_widget_size_allocate (global_gtk_vscrollbar, ref allocation);
				
				// fix for artefacts
				dc.FillRectangle (ResPool.GetSolidBrush (fix_color), 
						  bar.ClientRectangle.X, bar.ClientRectangle.Y, bar.ClientRectangle.Width, 3);
				dc.FillRectangle (ResPool.GetSolidBrush (fix_color), 
						  bar.ClientRectangle.X, bar.ClientRectangle.Bottom - 4, bar.ClientRectangle.Width, 3);
			} else {
				scrollbar_style = global_gtk_hscrollbar_style;
				
				gtk_widget_size_allocate (global_gtk_hscrollbar, ref allocation);
				
				// fix for artefacts
				dc.FillRectangle (ResPool.GetSolidBrush (fix_color), 
						  bar.ClientRectangle.X, bar.ClientRectangle.Y, 3, bar.ClientRectangle.Height);
				dc.FillRectangle (ResPool.GetSolidBrush (fix_color), 
						  bar.ClientRectangle.Right - 4, bar.ClientRectangle.Y, 3, bar.ClientRectangle.Height);
			}
			
			scrollbar_style = gtk_style_attach (scrollbar_style, gdkwindow);  // need it
			
			current_gdk_window = gdkwindow;
			current_style = scrollbar_style;
			
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
		}
		
		protected override void ScrollBar_DrawThumb( ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc ) {
			if ( bar.Enabled)
				DrawScrollBarThumb( dc, thumb_pos, bar );
		}
		
		protected override void ScrollBar_Vertical_Draw_ThumbMoving_None (int scrollbutton_height, ScrollBar bar, Rectangle clip, Graphics dc)
		{
			Rectangle r = new Rectangle (0,
						     scrollbutton_height, bar.ClientRectangle.Width, bar.ClientRectangle.Height - (scrollbutton_height * 2));
			gtk_paint_box (current_style, 
				       current_gdk_window, 
				       (int) StateType.Active,
				       (int) ShadowType.In,
				       IntPtr.Zero,
				       global_gtk_vscrollbar,
				       "vscrollbar",
				       r.X, r.Y,
				       r.Width, r.Height);
		}
		
		protected override void ScrollBar_Horizontal_Draw_ThumbMoving_None (int scrollbutton_width, ScrollBar bar, Rectangle clip, Graphics dc)
		{
			Rectangle r = new Rectangle (scrollbutton_width,
						     0, bar.ClientRectangle.Width - (scrollbutton_width * 2), bar.ClientRectangle.Height);
			
			gtk_paint_box (current_style, 
				       current_gdk_window, 
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
				       current_gdk_window, 
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
			
			IntPtr gdkwindow = gdk_window_foreign_new_for_display (gdkdisplay, (uint) ctrl.Handle);
			
			IntPtr progressbar_style = gtk_style_attach (global_gtk_progress_bar_style, gdkwindow);  // need it
			
			// draw background
			gtk_paint_box (progressbar_style,
				       gdkwindow,
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
			if (ctrl.Value == 0)
				return;
			
			if (ctrl.Value == ctrl.Maximum)
				barpos_pixels = client_area.Width + 2;
			else
				barpos_pixels = (((ctrl.Value - ctrl.Minimum) * client_area.Width) / (ctrl.Maximum - ctrl.Minimum)) + 1;
			
			gtk_paint_box (progressbar_style, 
				       gdkwindow, 
				       (int) StateType.Prelight,
				       (int) ShadowType.Out,
				       IntPtr.Zero,
				       global_gtk_vscrollbar,
				       "bar",
				       client_area.X - 1, client_area.Y - 1,
				       barpos_pixels, client_area.Height + 2);
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
			IntPtr gdkwindow = gdk_window_foreign_new_for_display (gdkdisplay, (uint) radio_button.Handle);
			
			IntPtr radiobutton_style = gtk_style_attach (global_gtk_radio_button_style, gdkwindow);
			
			current_gdk_window = gdkwindow;
			current_style = radiobutton_style;
			
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
			
			gtk_paint_option (radiobutton_style,
					  gdkwindow,
					  (int) state_type,
					  (int) shadow_type,
					  IntPtr.Zero,
					  global_gtk_radio_button,
					  "radiobutton",
					  radiobutton_rectangle.X,
					  radiobutton_rectangle.Y,
					  radiobutton_rectangle.Width,
					  radiobutton_rectangle.Height);
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
						 current_gdk_window,
						 (int) StateType.Active,
						 IntPtr.Zero,
						 IntPtr.Zero,
						 "radiobutton",
						 text_rectangle.X,
						 text_rectangle.Y,
						 text_rectangle.Width,
						 text_rectangle.Height);
			}
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
			IntPtr gdkwindow = gdk_window_foreign_new_for_display (gdkdisplay, (uint) checkbox.Handle);
			
			IntPtr checkbox_style = gtk_style_attach (global_gtk_check_button_style, gdkwindow);
			
			current_gdk_window = gdkwindow;
			current_style = checkbox_style;
			
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
			
			gtk_paint_check (checkbox_style,
					 gdkwindow,
					 (int) state_type,
					 (int) shadow_type,
					 IntPtr.Zero,
					 global_gtk_radio_button,
					 "checkbutton",
					 checkbox_rectangle.X,
					 checkbox_rectangle.Y,
					 checkbox_rectangle.Width,
					 checkbox_rectangle.Height);
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
						 current_gdk_window,
						 (int) StateType.Active,
						 IntPtr.Zero,
						 IntPtr.Zero,
						 "checkbutton",
						 text_rectangle.X,
						 text_rectangle.Y,
						 text_rectangle.Width,
						 text_rectangle.Height);
			}
		}
		#endregion	// CheckBox
		
		public override void CPDrawButton (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
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
			
			IntPtr drawbutton_style = gtk_style_attach (global_gtk_button_style, current_gdk_window);  // need it
			
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
			
			gtk_paint_box (drawbutton_style, current_gdk_window,
				       (int) state_type,
				       (int) shadow_type,
				       IntPtr.Zero,
				       IntPtr.Zero,
				       detail,
				       rectangle.X, rectangle.Y,
				       rectangle.Width, rectangle.Height);
		}
		
		/* Scroll button: regular button + direction arrow */
		public override void CPDrawScrollButton (Graphics dc, Rectangle area, ScrollButton scroll_button_type, ButtonState state)
		{
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
					 current_gdk_window, 
					 (int) state_type,
					 (int) ShadowType.Out,
					 IntPtr.Zero,
					 IntPtr.Zero,
					 "",		
					 (int) arrow_type, true, 
					 arrow_x, 
					 arrow_y,
					 arrow_width, arrow_height);
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
					       current_gdk_window, 
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
					       current_gdk_window, 
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
