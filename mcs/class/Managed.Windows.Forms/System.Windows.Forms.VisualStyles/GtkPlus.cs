//
// GtkPlus.cs: Wraps GTK+, exposing its control painting features to code
// that uses System.Drawing types (such as IDeviceContext, Rectangle).
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
// Copyright (c) 2008 George Giolfan
//
// Authors:
//	George Giolfan (georgegiolfan@yahoo.com)
//

using gpointer = System.IntPtr;
using gboolean = System.Boolean;
using gint = System.Int32;
using guint = System.UInt32;
using guint8 = System.Byte;
using guint16 = System.UInt16;
using guint32 = System.UInt32;
using gfloat = System.Single;
using gdouble = System.Double;

using PangoFontDescriptionPointer = System.IntPtr;

using GDataPointer = System.IntPtr;
using GObjectPointer = System.IntPtr;
using GSListPointer = System.IntPtr;
using GType = System.IntPtr;
using GTypeClassPointer = System.IntPtr;
using GTypeInstancePointer = System.IntPtr;

using GdkColormapPointer = System.IntPtr;
using GdkDrawablePointer = System.IntPtr;
using GdkGCPointer = System.IntPtr;
using GdkNativeWindowPointer = System.IntPtr;
using GdkPixbufPointer = System.IntPtr;
using GdkPixmapPointer = System.IntPtr;
using GdkWindowPointer = System.IntPtr;

using GtkAdjustmentPointer = System.IntPtr;
using GtkBinPointer = System.IntPtr;
using GtkContainerPointer = System.IntPtr;
using GtkObjectPointer = System.IntPtr;
using GtkStylePointer = System.IntPtr;
using GtkToolbarPointer = System.IntPtr;
using GtkToolItemPointer = System.IntPtr;
using GtkTreeViewPointer = System.IntPtr;
using GtkTreeViewColumnPointer = System.IntPtr;
using GtkWidgetPointer = System.IntPtr;
using GtkWindowPointer = System.IntPtr;

using GtkAllocation = System.Windows.Forms.VisualStyles.GtkPlus.GdkRectangle;
using GInitiallyUnowned = System.Windows.Forms.VisualStyles.GtkPlus.GObject;

using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.VisualStyles
{
	/// <summary>
	/// </summary>
	/// <remarks>
	/// Inspired by ThemeGtk and QGtkStyle (http://labs.trolltech.com/page/Projects/Styles/GtkStyle). Tries to use the exact GTK+ logic in order to be compatible with all its themes.
	/// </remarks>
	class GtkPlus
	{
		#region Instance
		static GtkPlus instance;
		public static GtkPlus Instance {
			get { return instance; }
		}
		#endregion
		#region GTK+ initialization
		public static bool Initialize ()
		{
			try {
				if (gtk_check_version (2, 10, 0) != IntPtr.Zero)
					return false;
				//TODO: If we need to pass the actual arguments, duplicate the required code from GTK#.
				int argc = 0;
				string [] argv = new string [1];
				bool result = gtk_init_check (ref argc, ref argv);
				if (result)
					instance = new GtkPlus ();
				return result;
			} catch (DllNotFoundException) {
				return false;
			}
		}
		#endregion

		#region Fields
		readonly int WidgetTypeCount = Enum.GetNames (typeof (WidgetType)).Length;
		readonly GtkWidgetPointer [] widgets;
		readonly GtkWidgetPointer window;
		readonly GtkWidgetPointer @fixed;
		readonly GtkStylePointer [] styles;
		#region ComboBox
		readonly GtkWidgetPointer combo_box_drop_down_toggle_button;
		readonly GtkWidgetPointer combo_box_drop_down_arrow;
		GtkStylePointer combo_box_drop_down_toggle_button_style;
		GtkStylePointer combo_box_drop_down_arrow_style;
		#endregion
		#region ToolBar
		readonly GtkWidgetPointer tool_bar_button;
		readonly GtkWidgetPointer tool_bar_toggle_button;
		GtkStylePointer tool_bar_button_style;
		GtkStylePointer tool_bar_toggle_button_style;
		#endregion
		#region TreeView
		readonly GtkTreeViewColumnPointer tree_view_column;
		readonly GtkWidgetPointer tree_view_column_button;
		GtkStylePointer tree_view_column_button_style;
		#endregion
		#region Painters
		readonly ButtonPainter button_painter = new ButtonPainter ();
		readonly CheckBoxPainter check_box_painter = new CheckBoxPainter ();
		readonly RadioButtonPainter radio_button_painter = new RadioButtonPainter ();
		#region ComboBox
		readonly ComboBoxDropDownButtonPainter combo_box_drop_down_button_painter = new ComboBoxDropDownButtonPainter ();
		readonly ComboBoxBorderPainter combo_box_border_painter = new ComboBoxBorderPainter ();
		#endregion
		#region GroupBox
		readonly GroupBoxPainter group_box_painter = new GroupBoxPainter ();
		#endregion
		#region Header
		readonly HeaderPainter header_painter = new HeaderPainter ();
		#endregion
		#region ProgressBar
		readonly ProgressBarBarPainter progress_bar_bar_painter = new ProgressBarBarPainter ();
		readonly ProgressBarChunkPainter progress_bar_chunk_painter = new ProgressBarChunkPainter ();
		#endregion
		#region ScrollBar
		readonly ScrollBarArrowButtonPainter scroll_bar_arrow_button_painter = new ScrollBarArrowButtonPainter ();
		readonly ScrollBarThumbButtonPainter scroll_bar_thumb_button_painter = new ScrollBarThumbButtonPainter ();
		readonly ScrollBarTrackPainter scroll_bar_track_painter = new ScrollBarTrackPainter ();
		#endregion
		#region StatusBar
		readonly StatusBarGripperPainter status_bar_gripper_painter = new StatusBarGripperPainter ();
		#endregion
		#region TabControl
		readonly TabControlPanePainter tab_control_pane_painter = new TabControlPanePainter ();
		readonly TabControlTabItemPainter tab_control_tab_item_painter = new TabControlTabItemPainter ();
		#endregion
		readonly TextBoxPainter text_box_painter = new TextBoxPainter ();
		#region ToolBar
		readonly ToolBarPainter tool_bar_painter = new ToolBarPainter ();
		readonly ToolBarButtonPainter tool_bar_button_painter = new ToolBarButtonPainter ();
		readonly ToolBarCheckedButtonPainter tool_bar_checked_button_painter = new ToolBarCheckedButtonPainter ();
		#endregion
		#region TrackBar
		readonly TrackBarTrackPainter track_bar_track_painter = new TrackBarTrackPainter ();
		readonly TrackBarThumbPainter track_bar_thumb_painter = new TrackBarThumbPainter ();
		#endregion
		readonly TreeViewGlyphPainter tree_view_glyph_painter = new TreeViewGlyphPainter ();
		readonly UpDownPainter up_down_painter = new UpDownPainter ();
		#endregion
		#endregion
		#region Constructor and finalizer
		protected GtkPlus ()
		{
			widgets = new GtkWidgetPointer [WidgetTypeCount];
			styles = new GtkStylePointer [WidgetTypeCount];
			window = gtk_window_new (GtkWindowType.GTK_WINDOW_TOPLEVEL);
			@fixed = gtk_fixed_new ();
			gtk_container_add (window, @fixed);
			#region Widget types
			#region Button
			gtk_container_add (@fixed, widgets [(int)WidgetType.Button] = gtk_button_new ());
			GTK_WIDGET_SET_FLAGS (widgets [(int)WidgetType.Button], GtkWidgetFlags.GTK_CAN_DEFAULT);
			#endregion
			#region CheckBox
			gtk_container_add (@fixed, widgets [(int)WidgetType.CheckBox] = gtk_check_button_new ());
			#endregion
			#region ComboBox
			gtk_container_add (@fixed, widgets [(int)WidgetType.ComboBox] = gtk_combo_box_entry_new ());
			gtk_widget_realize (widgets [(int)WidgetType.ComboBox]);
			combo_box_drop_down_toggle_button = GetFirstChildWidgetOfType.Get (widgets [(int)WidgetType.ComboBox], gtk_toggle_button_get_type ());
			gtk_widget_realize (combo_box_drop_down_toggle_button);
			combo_box_drop_down_arrow = GetFirstChildWidgetOfType.Get (combo_box_drop_down_toggle_button, gtk_arrow_get_type ());
			g_object_ref (combo_box_drop_down_toggle_button_style = GetWidgetStyle (combo_box_drop_down_toggle_button));
			g_object_ref (combo_box_drop_down_arrow_style = GetWidgetStyle (combo_box_drop_down_arrow));
			#endregion
			#region GroupBox
			gtk_container_add (@fixed, widgets [(int)WidgetType.GroupBox] = gtk_frame_new (null));
			#endregion
			#region ProgressBar
			gtk_container_add (@fixed, widgets [(int)WidgetType.ProgressBar] = gtk_progress_bar_new ());
			#endregion
			#region RadioButton
			gtk_container_add (@fixed, widgets [(int)WidgetType.RadioButton] = gtk_radio_button_new (IntPtr.Zero));
			#endregion
			#region ScrollBar
			gtk_container_add (@fixed, widgets [(int)WidgetType.HScrollBar] = gtk_hscrollbar_new (IntPtr.Zero));
			gtk_container_add (@fixed, widgets [(int)WidgetType.VScrollBar] = gtk_vscrollbar_new (IntPtr.Zero));
			#endregion
			#region StatusBar
			gtk_container_add (@fixed, widgets [(int)WidgetType.StatusBar] = gtk_statusbar_new ());
			#endregion
			#region TabControl
			gtk_container_add (@fixed, widgets [(int)WidgetType.TabControl] = gtk_notebook_new ());
			#endregion
			#region TextBox
			gtk_container_add (@fixed, widgets [(int)WidgetType.TextBox] = gtk_entry_new ());
			#endregion
			#region ToolBar
			gtk_container_add (@fixed, widgets [(int)WidgetType.ToolBar] = gtk_toolbar_new ());

			GtkToolItemPointer tool_button = gtk_tool_button_new (IntPtr.Zero, null);
			gtk_toolbar_insert (widgets [(int)WidgetType.ToolBar], tool_button, -1);
			tool_bar_button = gtk_bin_get_child (tool_button);
			g_object_ref (tool_bar_button_style = GetWidgetStyle (tool_bar_button));

			GtkToolItemPointer toggle_tool_button = gtk_toggle_tool_button_new ();
			gtk_toolbar_insert (widgets [(int)WidgetType.ToolBar], toggle_tool_button, -1);
			tool_bar_toggle_button = gtk_bin_get_child (toggle_tool_button);
			g_object_ref (tool_bar_toggle_button_style = GetWidgetStyle (tool_bar_toggle_button));
			#endregion
			#region TrackBar
			gtk_container_add (@fixed, widgets [(int)WidgetType.HorizontalTrackBar] = gtk_hscale_new_with_range (0, 1, 1));
			gtk_container_add (@fixed, widgets [(int)WidgetType.VerticalTrackBar] = gtk_vscale_new_with_range (0, 1, 1));
			#endregion
			#region TreeView
			gtk_container_add (@fixed, widgets [(int)WidgetType.TreeView] = gtk_tree_view_new ());
			tree_view_column = gtk_tree_view_column_new ();
			gtk_tree_view_insert_column (widgets [(int)WidgetType.TreeView], tree_view_column, -1);
			GtkTreeViewColumn column_structure = (GtkTreeViewColumn)Marshal.PtrToStructure (tree_view_column, typeof (GtkTreeViewColumn));
			tree_view_column_button = column_structure.button;
			g_object_ref (tree_view_column_button_style = GetWidgetStyle (tree_view_column_button));
			#endregion
			#region UpDown
			GtkAdjustmentPointer adjustment = gtk_adjustment_new (0, 0, 0, 0, 0, 0);
			gtk_container_add (@fixed, widgets [(int)WidgetType.UpDown] = gtk_spin_button_new (adjustment, 0, 0));
			#endregion
			#endregion
			for (int widget_index = 0; widget_index < WidgetTypeCount; widget_index++)
				g_object_ref (styles [widget_index] = GetWidgetStyle (widgets [widget_index]));
		}
		~GtkPlus ()
		{
			gtk_object_destroy (window);
			for (int widget_index = 0; widget_index < WidgetTypeCount; widget_index++)
				g_object_unref (styles [widget_index]);
			#region ComboBox
			g_object_unref (combo_box_drop_down_toggle_button_style);
			g_object_unref (combo_box_drop_down_arrow_style);
			#endregion
			#region ToolBar
			g_object_unref (tool_bar_button_style);
			g_object_unref (tool_bar_toggle_button_style);
			#endregion
			#region TreeView
			g_object_unref (tree_view_column_button_style);
			#endregion
		}
		#endregion
		#region Controls
		#region Button
		public void ButtonPaint (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, bool @default, GtkPlusState state)
		{
			button_painter.Configure (@default, state);
			Paint (WidgetType.Button, bounds, dc, clippingArea, button_painter);
		}
		#endregion
		#region CheckBox
		public void CheckBoxPaint (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state, GtkPlusToggleButtonValue value)
		{
			check_box_painter.Configure (state, value);
			Paint (WidgetType.CheckBox, bounds, dc, clippingArea, check_box_painter);
		}
		Size GetGtkCheckButtonIndicatorSize (WidgetType widgetType)
		{
			int indicator_size = GetWidgetStyleInteger (widgets [(int)widgetType], "indicator-size");
			return new Size (indicator_size, indicator_size);
		}
		public Size CheckBoxGetSize ()
		{
			return GetGtkCheckButtonIndicatorSize (WidgetType.CheckBox);
		}
		#endregion
		#region ComboBox
		public void ComboBoxPaintDropDownButton (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state)
		{
			combo_box_drop_down_button_painter.Configure (state);
			Paint (WidgetType.ComboBox, bounds, dc, clippingArea, combo_box_drop_down_button_painter);
		}
		public void ComboBoxPaintBorder (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea)
		{
			Paint (WidgetType.ComboBox, bounds, dc, clippingArea, combo_box_border_painter);
		}
		#endregion
		#region GroupBox
		public void GroupBoxPaint (IDeviceContext dc, Rectangle bounds, Rectangle excludedArea, GtkPlusState state)
		{
			group_box_painter.Configure (state);
			PaintExcludingArea (WidgetType.GroupBox, bounds, dc, excludedArea, group_box_painter);
		}
		#endregion
		#region Header
		public void HeaderPaint (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state)
		{
			header_painter.Configure (state);
			Paint (WidgetType.TreeView, bounds, dc, clippingArea, header_painter);
		}
		#endregion
		#region ProgressBar
		public void ProgressBarPaintBar (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea)
		{
			Paint (WidgetType.ProgressBar, bounds, dc, clippingArea, progress_bar_bar_painter);
		}
		public void ProgressBarPaintChunk (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea)
		{
			Paint (WidgetType.ProgressBar, bounds, dc, clippingArea, progress_bar_chunk_painter);
		}
		public Rectangle ProgressBarGetBackgroundContentRectagle (Rectangle bounds)
		{
			GtkStyle style = (GtkStyle)Marshal.PtrToStructure (gtk_widget_get_style (widgets [(int)WidgetType.ProgressBar]), typeof(GtkStyle));
			bounds.Inflate (-style.xthickness, -style.ythickness);
			return bounds;
		}
		#endregion
		#region RadioButton
		public void RadioButtonPaint (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state, GtkPlusToggleButtonValue value)
		{
			radio_button_painter.Configure (state, value);
			Paint (WidgetType.RadioButton, bounds, dc, clippingArea, radio_button_painter);
		}
		public Size RadioButtonGetSize ()
		{
			return GetGtkCheckButtonIndicatorSize (WidgetType.RadioButton);
		}
		#endregion
		#region ScrollBar
		public void ScrollBarPaintArrowButton (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state, bool horizontal, bool upOrLeft)
		{
			scroll_bar_arrow_button_painter.Configure (state, horizontal, upOrLeft);
			Paint (horizontal ? WidgetType.HScrollBar : WidgetType.VScrollBar, bounds, dc, clippingArea, scroll_bar_arrow_button_painter);
		}
		public void ScrollBarPaintThumbButton (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state, bool horizontal)
		{
			scroll_bar_thumb_button_painter.Configure (state, horizontal);
			Paint (horizontal ? WidgetType.HScrollBar : WidgetType.VScrollBar, bounds, dc, clippingArea, scroll_bar_thumb_button_painter);
		}
		public void ScrollBarPaintTrack (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state, bool horizontal, bool upOrLeft)
		{
			scroll_bar_track_painter.Configure (state, upOrLeft);
			Paint (horizontal ? WidgetType.HScrollBar : WidgetType.VScrollBar, bounds, dc, clippingArea, scroll_bar_track_painter);
		}
		#endregion
		#region StatusBar
		public void StatusBarPaintGripper (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea)
		{
			Paint (WidgetType.StatusBar, bounds, dc, clippingArea, status_bar_gripper_painter);
		}
		#endregion
		#region TabControl
		public void TabControlPaintPane (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea)
		{
			Paint (WidgetType.TabControl, bounds, dc, clippingArea, tab_control_pane_painter);
		}
		public void TabControlPaintTabItem (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state)
		{
			tab_control_tab_item_painter.Configure (state);
			Paint (WidgetType.TabControl, bounds, dc, clippingArea, tab_control_tab_item_painter);
		}
		#endregion
		#region TextBox
		public void TextBoxPaint (IDeviceContext dc, Rectangle bounds, Rectangle excludedArea, GtkPlusState state)
		{
			text_box_painter.Configure (state);
			PaintExcludingArea (WidgetType.TextBox, bounds, dc, excludedArea, text_box_painter);
		}
		#endregion
		#region ToolBar
		public void ToolBarPaint (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea)
		{
			Paint (WidgetType.ToolBar, bounds, dc, clippingArea, tool_bar_painter);
		}
		public void ToolBarPaintButton (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state)
		{
			tool_bar_button_painter.Configure (state);
			Paint (WidgetTypeNotNeeded, bounds, dc, clippingArea, tool_bar_button_painter);
		}
		public void ToolBarPaintCheckedButton (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea)
		{
			Paint (WidgetTypeNotNeeded, bounds, dc, clippingArea, tool_bar_checked_button_painter);
		}
		#endregion
		#region TrackBar
		public void TrackBarPaintTrack (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, bool horizontal)
		{
			Paint (horizontal ? WidgetType.HorizontalTrackBar : WidgetType.VerticalTrackBar, bounds, dc, clippingArea, track_bar_track_painter);
		}
		public void TrackBarPaintThumb (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, GtkPlusState state, bool horizontal)
		{
			track_bar_thumb_painter.Configure (state, horizontal);
			Paint (horizontal ? WidgetType.HorizontalTrackBar : WidgetType.VerticalTrackBar, bounds, dc, clippingArea, track_bar_thumb_painter);
		}
		#endregion
		#region TreeView
		public void TreeViewPaintGlyph (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, bool closed)
		{
			tree_view_glyph_painter.Configure (closed);
			Paint (WidgetType.TreeView, bounds, dc, clippingArea, tree_view_glyph_painter);
		}
		#endregion
		#region UpDown
		public void UpDownPaint (IDeviceContext dc, Rectangle bounds, Rectangle clippingArea, bool up, GtkPlusState state)
		{
			up_down_painter.Configure (up, state);
			Paint (WidgetType.UpDown, bounds, dc, clippingArea, up_down_painter);
		}
		#endregion
		#endregion
		#region Painting
		void Paint (WidgetType widgetType, Rectangle bounds, IDeviceContext dc, Rectangle clippingArea, Painter painter)
		{
			Paint (widgetType, bounds, dc, TransparencyType.Alpha, Color.Black, DeviceContextType.Native, clippingArea, painter, Rectangle.Empty);
		}
		void PaintExcludingArea (WidgetType widgetType, Rectangle bounds, IDeviceContext dc, Rectangle excludedArea, Painter painter)
		{
			Paint (widgetType, bounds, dc, TransparencyType.Alpha, Color.Black, DeviceContextType.Native, bounds, painter, excludedArea);
		}
		void Paint (WidgetType widgetType, Rectangle bounds, IDeviceContext dc, TransparencyType transparencyType, Color background, DeviceContextType deviceContextType, Rectangle clippingArea, Painter painter, Rectangle excludedArea)
		{
			Rectangle painted_area = Rectangle.Intersect (bounds, clippingArea);
			if (painted_area.Width == 0 || painted_area.Height == 0)
				return;
			painted_area.Offset (-bounds.X, -bounds.Y);
			excludedArea.Offset (-bounds.X, -bounds.Y);
			GdkDrawablePointer drawable = gdk_pixmap_new (IntPtr.Zero, bounds.Width, bounds.Height, 24);
			painter.AttachStyle (widgetType, drawable, this);
			GdkPixbufPointer pixbuf;
			IntPtr pixel_data;
			int rowstride;
			GdkGCPointer gc = gdk_gc_new (drawable);
			GdkColor color = new GdkColor (background);
			gdk_gc_set_rgb_fg_color (gc, ref color);
			Paint (drawable, gc, bounds, widgetType, out pixbuf, out pixel_data, out rowstride, painted_area, painter, excludedArea);
			GdkPixbufPointer white_pixbuf = IntPtr.Zero;
			IntPtr white_pixel_data = IntPtr.Zero;
			int white_rowstride = 0;
			GdkColor white_color = new GdkColor();
			if (transparencyType == TransparencyType.Alpha) {
				white_color.red = guint16.MaxValue;
				white_color.green = guint16.MaxValue;
				white_color.blue = guint16.MaxValue;
				gdk_gc_set_rgb_fg_color (gc, ref white_color);
				Paint (drawable, gc, bounds, widgetType, out white_pixbuf, out white_pixel_data, out white_rowstride, painted_area, painter, excludedArea);
			}
			g_object_unref (gc);
			unsafe {
				byte* row = (byte*)pixel_data;
				byte* pixel;
				byte* white_row = (byte*)white_pixel_data;
				byte* white_pixel;

				for (int row_index = 0; row_index < painted_area.Height; row_index++) {
					pixel = row;
					white_pixel = white_row;
					for (int pixel_index = 0; pixel_index < painted_area.Width; pixel_index++) {
						const int GdkRedOffset = 0;
						const int GdkGreenOffset = 1;
						const int GdkBlueOffset = 2;
						const int BitmapAlphaOffset = 3;
						const int BitmapRedOffset = 2;
						const int BitmapBlueOffset = 0;
						switch (transparencyType) {
						case TransparencyType.Alpha:
							pixel [BitmapAlphaOffset] = (byte)(pixel [GdkRedOffset] - white_pixel [GdkRedOffset] + byte.MaxValue);
							break;
						case TransparencyType.Color:
							if (
								pixel [GdkRedOffset] == background.R &&
								pixel [GdkGreenOffset] == background.G &&
								pixel [GdkBlueOffset] == background.B) {
								const int AlphaFullyTransparent = 0;
								pixel [BitmapAlphaOffset] = AlphaFullyTransparent;
							}
							break;
						}

						byte temporary = pixel [GdkRedOffset];
						pixel [BitmapBlueOffset] = pixel [GdkBlueOffset];
						pixel [BitmapRedOffset] = temporary;

						const int PixelSize = 4;
						pixel += PixelSize;
						white_pixel += PixelSize;
					}
					row += rowstride;
					white_row += white_rowstride;
				}
			}
			if (transparencyType == TransparencyType.Alpha)
				g_object_unref (white_pixbuf);
			g_object_unref (drawable);
			Bitmap bitmap = new Bitmap (painted_area.Width, painted_area.Height, rowstride, PixelFormat.Format32bppPArgb, pixel_data);
			Graphics g;
			bool graphics_is_from_hdc = false;
			switch (deviceContextType) {
			case DeviceContextType.Graphics:
				g = (Graphics)dc;
				break;
			case DeviceContextType.Native:
				g = Graphics.FromHdc (dc.GetHdc ());
				break;
			default:
				g = dc as Graphics;
				if (g == null) {
					graphics_is_from_hdc = true;
					g = Graphics.FromHdc (dc.GetHdc ());
				} else
					graphics_is_from_hdc = false;
				break;
			}
			painted_area.Offset (bounds.X, bounds.Y);
			g.DrawImage (bitmap, painted_area.Location);
			switch (deviceContextType) {
			case DeviceContextType.Graphics:
				break;
			case DeviceContextType.Native:
				g.Dispose ();
				dc.ReleaseHdc ();
				break;
			default:
				if (graphics_is_from_hdc) {
					g.Dispose ();
					dc.ReleaseHdc ();
				}
				break;
			}
			bitmap.Dispose ();
			g_object_unref (pixbuf);
		}
		void Paint (GdkDrawablePointer drawable, GdkGCPointer gc, Rectangle rectangle, WidgetType widgetType, out GdkPixbufPointer pixbuf, out IntPtr pixelData, out int rowstride, Rectangle clippingArea, Painter painter, Rectangle excludedArea)
		{
			gdk_draw_rectangle (drawable, gc, true, clippingArea.X, clippingArea.Y, clippingArea.Width, clippingArea.Height);
			painter.Paint (styles [(int)widgetType], drawable, new GdkRectangle(clippingArea), widgets [(int)widgetType], 0, 0, rectangle.Width, rectangle.Height, this);
			if (excludedArea.Width != 0)
				gdk_draw_rectangle (drawable, gc, true, excludedArea.X, excludedArea.Y, excludedArea.Width, excludedArea.Height);
			if (
				(pixbuf = gdk_pixbuf_new (GdkColorspace.GDK_COLORSPACE_RGB, true, 8, clippingArea.Width, clippingArea.Height)) == IntPtr.Zero ||
				gdk_pixbuf_get_from_drawable (pixbuf, drawable, IntPtr.Zero, clippingArea.X, clippingArea.Y, 0, 0, clippingArea.Width, clippingArea.Height) == IntPtr.Zero)
				throw new OutOfMemoryException ();
			pixelData = gdk_pixbuf_get_pixels (pixbuf);
			rowstride = gdk_pixbuf_get_rowstride (pixbuf);
		}
		abstract class Painter
		{
			public virtual void AttachStyle (WidgetType widgetType, GdkDrawablePointer drawable, GtkPlus gtkPlus)
			{
				gtkPlus.styles [(int)widgetType] = gtk_style_attach (gtkPlus.styles [(int)widgetType], drawable);
			}
			public abstract void Paint (GtkStylePointer style, GdkWindowPointer window, GdkRectangle area, GtkWidgetPointer widget, gint x, gint y, gint width, gint height, GtkPlus gtkPlus);
		}
		enum TransparencyType
		{
			None,
			Color,
			Alpha
		}
		enum DeviceContextType
		{
			Unknown,
			Graphics,
			Native
		}
		#endregion
		#region Painters
		#region Button
		class ButtonPainter : Painter
		{
			bool @default;
			GtkPlusState state;
			public void Configure (bool @default, GtkPlusState state) {
				this.@default = @default;
				this.state = state;
			}
			public override void Paint (GtkStylePointer style, GdkWindowPointer window, GdkRectangle area, GtkWidgetPointer widget, gint x, gint y, gint width, gint height, GtkPlus gtkPlus)
			{
				if (@default) {
					gtk_window_set_default (gtkPlus.window, widget);
					gtk_paint_box (
						style,
						window,
						GtkStateType.GTK_STATE_NORMAL,
						GtkShadowType.GTK_SHADOW_IN,
						ref area,
						widget,
						"buttondefault",
						x, y, width, height);
					gtk_window_set_default (gtkPlus.window, IntPtr.Zero);
				} else
					gtk_paint_box (
						style,
						window,
						(GtkStateType)state,
						state == GtkPlusState.Pressed ? GtkShadowType.GTK_SHADOW_IN : GtkShadowType.GTK_SHADOW_OUT,
						ref area,
						widget,
						"button",
						x, y, width, height);
			}
		}
		#endregion
		#region CheckBox, RadioButton
		abstract class ToggleButtonPainter : Painter
		{
			GtkPlusState state;
			GtkPlusToggleButtonValue value;
			public void Configure (GtkPlusState state, GtkPlusToggleButtonValue value)
			{
				this.state = state;
				this.value = value;
			}
			public override void Paint (GtkStylePointer style, GdkWindowPointer window, GdkRectangle area, GtkWidgetPointer widget, gint x, gint y, gint width, gint height, GtkPlus gtkPlus)
			{
				PaintFunction (
					style,
					window,
					(GtkStateType)state,
					(GtkShadowType)value,
					ref area,
					widget,
					Detail,
					x, y, width,height);
			}
			protected abstract string Detail { get; }
			protected abstract ToggleButtonPaintFunction PaintFunction { get; }
		}
		delegate void ToggleButtonPaintFunction (GtkStylePointer style, GdkWindowPointer window, GtkStateType state_type, GtkShadowType shadow_type, ref GdkRectangle area, GtkWidgetPointer widget, string detail, gint x, gint y, gint width, gint height);
		class CheckBoxPainter : ToggleButtonPainter
		{
			protected override string Detail {
				get { 
					return "checkbutton";
				}
			}
			protected override ToggleButtonPaintFunction PaintFunction {
				get {
					return gtk_paint_check;
				}
			}
		}
		class RadioButtonPainter : ToggleButtonPainter
		{
			protected override string Detail {
				get { 
					return "radiobutton";
				}
			}
			protected override ToggleButtonPaintFunction PaintFunction {
				get {
					return gtk_paint_option;
				}
			}
		}
		#endregion
		#region ComboBox
		class ComboBoxDropDownButtonPainter : Painter
		{
			GtkPlusState state;
			public void Configure (GtkPlusState state)
			{
				this.state = state;
			}
			public override void AttachStyle (WidgetType widgetType, IntPtr drawable, GtkPlus gtkPlus)
			{
				gtkPlus.combo_box_drop_down_toggle_button_style = gtk_style_attach (gtkPlus.combo_box_drop_down_toggle_button_style, drawable);
				gtkPlus.combo_box_drop_down_arrow_style = gtk_style_attach (gtkPlus.combo_box_drop_down_arrow_style, drawable);
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				GtkShadowType shadow_type;
				switch (state) {
				case GtkPlusState.Disabled:
					shadow_type = GtkShadowType.GTK_SHADOW_ETCHED_IN;
					break;
				case GtkPlusState.Pressed:
					shadow_type = GtkShadowType.GTK_SHADOW_IN;
					break;
				default:
					shadow_type = GtkShadowType.GTK_SHADOW_OUT;
					break;
				}
				gtk_paint_box (
					gtkPlus.combo_box_drop_down_toggle_button_style,
					window,
					(GtkStateType)state,
					shadow_type,
					ref area,
					gtkPlus.combo_box_drop_down_toggle_button,
					"button",
					x, y, width, height);
				GtkMisc misc = (GtkMisc)Marshal.PtrToStructure (gtkPlus.combo_box_drop_down_arrow, typeof (GtkMisc));
				int extent = (int)(Math.Min (width - misc.xpad * 2, height - misc.ypad * 2) * GetWidgetStyleSingle (gtkPlus.combo_box_drop_down_arrow, "arrow-scaling"));
				gtk_paint_arrow (
				    gtkPlus.combo_box_drop_down_arrow_style,
				    window,
				    (GtkStateType)state,
				    GtkShadowType.GTK_SHADOW_NONE,
				    ref area,
				    gtkPlus.combo_box_drop_down_arrow,
				    "arrow",
				    GtkArrowType.GTK_ARROW_DOWN,
				    true,
				    (int)Math.Floor (x + misc.xpad + (width - extent) * misc.xalign),
					(int)Math.Floor (y + misc.ypad + (height - extent) * misc.yalign),
					extent, extent);
			}
		}
		class ComboBoxBorderPainter : Painter
		{
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_shadow (
					style,
					window,
					 GtkStateType.GTK_STATE_NORMAL,
					 GtkShadowType.GTK_SHADOW_IN,
					 ref area,
					 widget,
					 "combobox",
					 x, y, width, height);
			}
		}
		#endregion
		#region GroupBox
		class GroupBoxPainter : Painter
		{
			GtkPlusState state;
			public void Configure (GtkPlusState state)
			{
				this.state = state;
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_shadow (
					style,
					window,
					(GtkStateType)state,
					GtkShadowType.GTK_SHADOW_ETCHED_IN,
					ref area,
					widget,
					"frame",
					x, y, width, height);

			}
		}
		#endregion
		#region Header
		class HeaderPainter : Painter
		{
			GtkPlusState state;
			public void Configure (GtkPlusState state)
			{
				this.state = state;
			}
			public override void AttachStyle (WidgetType widgetType, IntPtr drawable, GtkPlus gtkPlus)
			{
				gtkPlus.tree_view_column_button_style = gtk_style_attach (gtkPlus.tree_view_column_button_style, drawable);
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_box (
					gtkPlus.tree_view_column_button_style,
					window,
					(GtkStateType)state,
					state == GtkPlusState.Pressed ? GtkShadowType.GTK_SHADOW_IN : GtkShadowType.GTK_SHADOW_OUT,
					ref area,
					gtkPlus.tree_view_column_button,
					"button",
					x, y, width, height);
			}
		}
		#endregion
		#region ProgressBar
		class ProgressBarBarPainter : Painter
		{
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_box (
					style,
					window,
					GtkStateType.GTK_STATE_NORMAL,
					GtkShadowType.GTK_SHADOW_IN,
					ref area,
					widget,
					"trough",
					x, y, width, height);
			}
		}
		class ProgressBarChunkPainter : Painter
		{
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_box (
					style,
					window,
					GtkStateType.GTK_STATE_PRELIGHT,
					GtkShadowType.GTK_SHADOW_OUT,
					ref area,
					widget,
					"bar",
					x, y, width, height);
			}
		}
		#endregion
		#region ScrollBar
		class ScrollBarArrowButtonPainter : Painter
		{
			GtkPlusState state;
			bool horizontal;
			bool up_or_left;
			public void Configure (GtkPlusState state, bool horizontal, bool upOrLeft)
			{
				this.state = state;
				this.horizontal = horizontal;
				up_or_left = upOrLeft;
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gboolean can_focus;
				g_object_get (widget, "can-focus", out can_focus, IntPtr.Zero);
				if (can_focus) {
				    int focus_line_width;
				    int focus_padding;
				    gtk_widget_style_get (widget,
				        "focus-line-width", out focus_line_width,
				        "focus-padding", out focus_padding,
				        IntPtr.Zero);
				    int focus_width = focus_line_width + focus_padding;
				    if (horizontal) {
				        y -= focus_width;
				        height -= 2 * focus_width;
				    } else {
				        x -= focus_width;
				        width -= 2 * focus_width;
				    }
				}
				GtkShadowType shadow_type = state == GtkPlusState.Pressed ? GtkShadowType.GTK_SHADOW_IN : GtkShadowType.GTK_SHADOW_OUT;
				string detail = horizontal ? "hscrollbar" : "vscrollbar";
				gtk_paint_box (
					style,
					window,
					(GtkStateType)state,
					shadow_type,
					ref area,
					widget,
					detail,
					x, y, width, height);
				width /= 2;
				height /= 2;
				x += width / 2;
				y += height / 2;
				if (state == GtkPlusState.Pressed) {
				    int arrow_displacement_x;
				    int arrow_displacement_y;
				    gtk_widget_style_get (widget,
				        "arrow-displacement-x", out arrow_displacement_x,
				        "arrow-displacement-y", out arrow_displacement_y,
				        IntPtr.Zero);
				    x += arrow_displacement_x;
				    y += arrow_displacement_y;
				}
				gtk_paint_arrow (
					style,
					window,
					(GtkStateType)state,
					shadow_type,
					ref area,
					widget,
					detail,
					horizontal ? up_or_left ? GtkArrowType.GTK_ARROW_LEFT : GtkArrowType.GTK_ARROW_RIGHT : up_or_left ? GtkArrowType.GTK_ARROW_UP : GtkArrowType.GTK_ARROW_DOWN,
					true,
					x, y, width, height);
			}
		}
		abstract class RangeThumbButtonPainter : Painter
		{
			GtkPlusState state;
			bool horizontal;
			protected bool Horizontal {
				get { return horizontal; }
			}
			public void Configure (GtkPlusState state, bool horizontal)
			{
				this.state = state;
				this.horizontal = horizontal;
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_slider (
					style,
					window,
					(GtkStateType)state,
					state == GtkPlusState.Pressed && GetWidgetStyleBoolean (widget, "activate-slider") ? GtkShadowType.GTK_SHADOW_IN : GtkShadowType.GTK_SHADOW_OUT,
					ref area,
					widget,
					Detail,
					x, y, width, height,
					horizontal ? GtkOrientation.GTK_ORIENTATION_HORIZONTAL : GtkOrientation.GTK_ORIENTATION_VERTICAL);
			}
			protected abstract string Detail { get; }
		}
		class ScrollBarThumbButtonPainter : RangeThumbButtonPainter 
		{
			protected override string Detail {
				get { 
					return "slider";
				}
			}
		}
		class ScrollBarTrackPainter : Painter
		{
			GtkPlusState state;
			bool up_or_left;
			public void Configure (GtkPlusState state, bool upOrLeft)
			{
				this.state = state;
				up_or_left = upOrLeft;
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_box (
					style,
					window,
					state == GtkPlusState.Pressed ? GtkStateType.GTK_STATE_ACTIVE : GtkStateType.GTK_STATE_INSENSITIVE,
					GtkShadowType.GTK_SHADOW_IN,
					ref area,
					widget,
					GetWidgetStyleBoolean (widget, "trough-side-details") ? 
					up_or_left ? "trough-upper" : "trough-lower" :
					"trough",
					x, y, width, height);
			}
		}
		#endregion
		#region StatusBar
		class StatusBarGripperPainter : Painter
		{
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_resize_grip (
					style,
					window,
					GtkStateType.GTK_STATE_NORMAL,
					ref area,
					widget,
					"statusbar",
					GdkWindowEdge.GDK_WINDOW_EDGE_SOUTH_EAST,
					x, y, width, height);
			}
		}
		#endregion
		#region TabControl
		class TabControlPanePainter : Painter
		{
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_box_gap (
					style,
					window,
					GtkStateType.GTK_STATE_NORMAL,
					GtkShadowType.GTK_SHADOW_OUT,
					ref area,
					widget,
					"notebook",
					x, y, width, height,
					GtkPositionType.GTK_POS_TOP,
					0,
					0);
			}
		}
		class TabControlTabItemPainter : Painter
		{
			GtkPlusState state;
			public void Configure (GtkPlusState state)
			{
				this.state = state;
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_extension (
					style,
					window,
					(GtkStateType)state,
					GtkShadowType.GTK_SHADOW_OUT,
					ref area,
					widget,
					"tab",
					x, y, width, height,
					GtkPositionType.GTK_POS_BOTTOM);
			}
		}
		#endregion
		#region TextBox
		class TextBoxPainter : Painter
		{
			GtkPlusState state;
			public void Configure (GtkPlusState state)
			{
				this.state = state;
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_shadow (
				    style,
				    window,
				    GtkStateType.GTK_STATE_NORMAL,
				    GtkShadowType.GTK_SHADOW_IN,
				    ref area,
				    widget,
				    "entry",
				    x, y, width, height);
				GtkStyle style_structure = (GtkStyle)Marshal.PtrToStructure (style, typeof (GtkStyle));
				x += style_structure.xthickness;
				y += style_structure.ythickness;
				width -= 2 * style_structure.xthickness;
				height -= 2 * style_structure.ythickness;
				gtk_paint_flat_box (
				    style,
				    window,
				    (GtkStateType)state,
				    GtkShadowType.GTK_SHADOW_NONE,
				    ref area,
				    widget,
				    "entry_bg",
				    x, y, width, height);
			}
		}
		#endregion
		#region ToolBar
		class ToolBarPainter : Painter
		{
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_box (
					style,
					window,
					GtkStateType.GTK_STATE_NORMAL,
					GetWidgetStyleShadowType (widget),
					ref area,
					widget,
					"toolbar",
					x, y, width, height);
			}
		}
		class ToolBarButtonPainter : Painter
		{
			GtkPlusState state;
			public void Configure (GtkPlusState state) {
				this.state = state;
			}
			public override void AttachStyle (WidgetType widgetType, IntPtr drawable, GtkPlus gtkPlus)
			{
				gtkPlus.tool_bar_button_style = gtk_style_attach (gtkPlus.tool_bar_button_style, drawable);
			}
			public override void Paint (GtkStylePointer style, GdkWindowPointer window, GdkRectangle area, GtkWidgetPointer widget, gint x, gint y, gint width, gint height, GtkPlus gtkPlus)
			{
				gtk_paint_box (
					gtkPlus.tool_bar_button_style,
					window,
					(GtkStateType)state,
					state == GtkPlusState.Pressed ? GtkShadowType.GTK_SHADOW_IN : GtkShadowType.GTK_SHADOW_OUT,
					ref area,
					gtkPlus.tool_bar_button,
					"button",
					x, y, width, height);
			}
		}
		class ToolBarCheckedButtonPainter : Painter
		{
			public override void AttachStyle (WidgetType widgetType, IntPtr drawable, GtkPlus gtkPlus)
			{
				gtkPlus.tool_bar_toggle_button_style = gtk_style_attach (gtkPlus.tool_bar_toggle_button_style, drawable);
			}
			public override void Paint (GtkStylePointer style, GdkWindowPointer window, GdkRectangle area, GtkWidgetPointer widget, gint x, gint y, gint width, gint height, GtkPlus gtkPlus)
			{
				gtk_paint_box (
					gtkPlus.tool_bar_toggle_button_style,
					window,
					GtkStateType.GTK_STATE_ACTIVE,
					GtkShadowType.GTK_SHADOW_IN,
					ref area,
					gtkPlus.tool_bar_toggle_button,
					"button",
					x, y, width, height);
			}
		}
		#endregion
		#region TrackBar
		class TrackBarTrackPainter : Painter
		{
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_box (
					style,
					window,
					GtkStateType.GTK_STATE_ACTIVE,
					GtkShadowType.GTK_SHADOW_IN,
					ref area,
					widget,
					"trough",
					x, y, width, height);
			}
		}
		class TrackBarThumbPainter : RangeThumbButtonPainter
		{
			protected override string Detail {
				get {
					return Horizontal ? "hscale" : "vscale";	
				}
			}
		}
		#endregion
		#region TreeView
		class TreeViewGlyphPainter : Painter
		{
			bool closed;
			public void Configure (bool closed)
			{
				this.closed = closed;
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				gtk_paint_expander (
					style,
					window,
					GtkStateType.GTK_STATE_NORMAL,
					ref area,
					widget,
					"treeview",
					x + width / 2,
					y + height / 2,
					closed ? GtkExpanderStyle.GTK_EXPANDER_COLLAPSED : GtkExpanderStyle.GTK_EXPANDER_EXPANDED);
			}
		}
		#endregion
		#region UpDown
		class UpDownPainter : Painter
		{
			bool up;
			GtkPlusState state;
			public void Configure (bool up, GtkPlusState state)
			{
				this.up = up;
				this.state = state;
			}
			public override void Paint (IntPtr style, IntPtr window, GdkRectangle area, IntPtr widget, int x, int y, int width, int height, GtkPlus gtkPlus)
			{
				GtkShadowType shadow_type = GetWidgetStyleShadowType (widget);
				if (shadow_type != GtkShadowType.GTK_SHADOW_NONE)
					gtk_paint_box (
						style,
						window,
						GtkStateType.GTK_STATE_NORMAL,
						shadow_type,
						ref area,
						widget,
						"spinbutton",
						x, y - (up ? 0 : height), width, height * 2);
				shadow_type = state == GtkPlusState.Pressed ? GtkShadowType.GTK_SHADOW_IN : GtkShadowType.GTK_SHADOW_OUT;
				gtk_paint_box (
				    style,
				    window,
				    (GtkStateType)state,
				    shadow_type,
				    ref area,
				    widget,
				    up ? "spinbutton_up" : "spinbutton_down",
				    x, y, width, height);
				if (up)
					y += 2;
				height -= 2;
				width -= 3;
				x += 1;
				int w = width / 2;
				w -= w % 2 - 1;
				int h = (w + 1) / 2;
				x += (width - w) / 2;
				y += (height - h) / 2;
				height = h;
				width = w;
				gtk_paint_arrow (
				    style,
				    window,
				    (GtkStateType)state,
				    shadow_type,
				    ref area,
				    widget,
				    "spinbutton",
				    up ? GtkArrowType.GTK_ARROW_UP : GtkArrowType.GTK_ARROW_DOWN,
				    true,
				    x, y, width, height);
			}
		}
		#endregion
		#endregion
		#region Widget types
		const WidgetType WidgetTypeNotNeeded = (WidgetType)0;
		enum WidgetType
		{
			Button,
			CheckBox,
			ComboBox,
			GroupBox,
			ProgressBar,
			RadioButton,
			#region ScrollBar
			HScrollBar,
			VScrollBar,
			#endregion
			StatusBar,
			TabControl,
			TextBox,
			ToolBar,
			#region TrackBar
			HorizontalTrackBar,
			VerticalTrackBar,
			#endregion
			TreeView,
			UpDown,
		}
		#endregion
		#region GTK+ utility methods
		static GtkShadowType GetWidgetStyleShadowType (GtkWidgetPointer widget)
		{
			GtkShadowType result;
			gtk_widget_style_get (widget, "shadow-type", out result, IntPtr.Zero);
			return result;
		}
		static int GetWidgetStyleInteger (GtkWidgetPointer widget, string propertyName)
		{
			gint result;
			gtk_widget_style_get (widget, propertyName, out result, IntPtr.Zero);
			return result;
		}
		static float GetWidgetStyleSingle (GtkWidgetPointer widget, string propertyName)
		{
			gfloat result;
			gtk_widget_style_get (widget, propertyName, out result, IntPtr.Zero);
			return result;
		}
		static bool GetWidgetStyleBoolean (GtkWidgetPointer widget, string propertyName)
		{
			gboolean result;
			gtk_widget_style_get (widget, propertyName, out result, IntPtr.Zero);
			return result;
		}
		#region GetFirstChildWidgetOfType
		static class GetFirstChildWidgetOfType
		{
			public static GtkWidgetPointer Get (GtkContainerPointer parent, GType childType)
			{
				Type = childType;
				Result = GtkWidgetPointer.Zero;
				ContainersToSearch = new ArrayList ();
				ContainersToSearch.Add (parent);
				while (true) {
					ArrayList containers_to_search = ContainersToSearch;
					ContainersToSearch = new ArrayList ();
					foreach (GtkContainerPointer container in containers_to_search) {
						gtk_widget_realize (container);
						gtk_container_forall (container, Callback, gpointer.Zero);
						if (Result != GtkWidgetPointer.Zero)
							return Result;
					}
					if (ContainersToSearch.Count == 0)
						return GtkWidgetPointer.Zero;
				}
			}
			static void Callback (GtkWidgetPointer widget, gpointer data)
			{
				if (Result != IntPtr.Zero)
					return;
				if (g_type_check_instance_is_a (widget, Type))
					Result = widget;
				else if (g_type_check_instance_is_a (widget, gtk_container_get_type ()))
					ContainersToSearch.Add (widget);
			}
			static GType Type;
			static GtkWidgetPointer Result;
			static ArrayList ContainersToSearch;
		}
		#endregion
		static GtkStylePointer GetWidgetStyle (GtkWidgetPointer widget)
		{
			return gtk_rc_get_style (widget);
		}
		#endregion
		#region Platform Invoke
		#region Library names
#if VISUAL_STYLES_USE_GTKPLUS_ON_WINDOWS
		const string GobjectLibraryName = "libgobject-2.0-0.dll";
		const string GdkLibraryName = "libgdk-win32-2.0-0.dll";
		const string GdkPixbufLibraryName = "libgdk_pixbuf-2.0-0.dll";
		const string GtkLibraryName = "libgtk-win32-2.0-0.dll";
#else
		const string GobjectLibraryName = "libgobject-2.0.so";
		const string GdkLibraryName = "libgdk-x11-2.0.so";
		const string GdkPixbufLibraryName = "libgdk_pixbuf-2.0.so";
		const string GtkLibraryName = "libgtk-x11-2.0.so";
#endif
		#endregion
		#region GDK
		[DllImport (GdkLibraryName)]
		static extern void gdk_draw_rectangle (GdkDrawablePointer drawable, GdkGCPointer gc, gboolean filled, gint x, gint y, gint width, gint height);
		[DllImport (GdkLibraryName)]
		static extern GdkGCPointer gdk_gc_new (GdkDrawablePointer drawable);
		[DllImport (GdkLibraryName)]
		static extern void gdk_gc_set_rgb_fg_color (GdkGCPointer gc, ref GdkColor color);
		[DllImport (GdkLibraryName)]
		static extern GdkPixbufPointer gdk_pixbuf_get_from_drawable (GdkPixbufPointer dest, GdkDrawablePointer src, GdkColormapPointer cmap, int src_x, int src_y, int dest_x, int dest_y, int width, int height);
		[DllImport (GdkLibraryName)]
		static extern GdkPixmapPointer gdk_pixmap_new (GdkDrawablePointer drawable, gint width, gint height, gint depth);
		struct GdkColor{
			public guint32 pixel;
			public guint16 red;
			public guint16 green;
			public guint16 blue;
			public GdkColor (Color value)
			{
				pixel = 0;
				red = (guint16)(value.R << 8);
				green = (guint16)(value.G << 8);
				blue = (guint16)(value.B << 8);
			}
		}
		internal struct GdkRectangle {
			public gint x;
			public gint y;
			public gint width;
			public gint height;
			public GdkRectangle (Rectangle value)
			{
				x = value.X;
				y = value.Y;
				width = value.Width;
				height = value.Height;
			}
		}
		#endregion
		#region GdkPixbuf
		[DllImport (GdkPixbufLibraryName)]
		static extern IntPtr gdk_pixbuf_get_pixels (GdkPixbufPointer pixbuf);
		[DllImport (GdkPixbufLibraryName)]
		static extern int gdk_pixbuf_get_rowstride (GdkPixbufPointer pixbuf);
		[DllImport (GdkPixbufLibraryName)]
		static extern GdkPixbufPointer gdk_pixbuf_new (GdkColorspace colorspace, gboolean has_alpha, int bits_per_sample, int width, int height);
		enum GdkColorspace
		{
			GDK_COLORSPACE_RGB
		}
		#endregion
		#region GTK
		#region Functions
		[DllImport (GtkLibraryName)]
		static extern gboolean gtk_init_check (ref int argc, ref string[] argv);
		[DllImport (GtkLibraryName)]
		static extern IntPtr gtk_check_version (guint required_major, guint required_minor, guint required_micro);
		[DllImport (GtkLibraryName)]
		static extern void gtk_container_add (GtkContainerPointer container, GtkWidgetPointer widget);
		[DllImport (GtkLibraryName)]
		static extern void gtk_container_forall (GtkContainerPointer container, GtkCallback callback, gpointer callback_data);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void GtkCallback (GtkWidgetPointer widget, gpointer data);
		[DllImport (GtkLibraryName)]
		static extern void gtk_object_destroy (GtkObjectPointer @object);
		[DllImport (GtkLibraryName)]
		static extern GtkStylePointer gtk_rc_get_style (GtkWidgetPointer widget);
		[DllImport (GtkLibraryName)]
		static extern GtkStylePointer gtk_style_attach (GtkStylePointer style, GdkWindowPointer window);
		[DllImport (GtkLibraryName)]
		static extern void gtk_widget_realize (GtkWidgetPointer widget);
		[DllImport (GtkLibraryName)]
		static extern void gtk_widget_style_get (GtkWidgetPointer widget, string property, out gint value, IntPtr nullTerminator);
		[DllImport (GtkLibraryName)]
		static extern void gtk_widget_style_get (GtkWidgetPointer widget, string property, out gfloat value, IntPtr nullTerminator);
		[DllImport (GtkLibraryName)]
		static extern void gtk_widget_style_get (GtkWidgetPointer widget, string property1, out gint value1, string property2, out gint value2, IntPtr nullTerminator);
		[DllImport (GtkLibraryName)]
		static extern void gtk_widget_style_get (GtkWidgetPointer widget, string property, out GtkShadowType value, IntPtr nullTerminator);
		[DllImport (GtkLibraryName)]
		static extern void gtk_widget_style_get (GtkWidgetPointer widget, string property, out gboolean value, IntPtr nullTerminator);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_window_new (GtkWindowType type);
		[DllImport (GtkLibraryName)]
		static extern void gtk_window_set_default (GtkWindowPointer window, GtkWidgetPointer default_widget);
		[DllImport (GtkLibraryName)]
		static extern GtkObjectPointer gtk_adjustment_new (gdouble value, gdouble lower, gdouble upper, gdouble step_increment, gdouble page_increment, gdouble page_size);
		[DllImport (GtkLibraryName)]
		static extern GtkStylePointer gtk_widget_get_style (GtkWidgetPointer widget);
		[DllImport (GtkLibraryName)]
		static extern GtkTreeViewColumnPointer gtk_tree_view_column_new ();
		[DllImport (GtkLibraryName)]
		static extern gint gtk_tree_view_insert_column (
			GtkTreeViewPointer tree_view,
			GtkTreeViewColumnPointer column,
			gint position);
		[DllImport (GtkLibraryName)]
		static extern void gtk_toolbar_insert (
			GtkToolbarPointer toolbar,
			GtkToolItemPointer item,
			gint pos);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_bin_get_child (GtkBinPointer bin);
		#region Widget type
		[DllImport (GtkLibraryName)]
		static extern GType gtk_arrow_get_type ();
		[DllImport (GtkLibraryName)]
		static extern GType gtk_container_get_type ();
		[DllImport (GtkLibraryName)]
		static extern GType gtk_toggle_button_get_type ();
		#endregion
		#region Widget creation
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_button_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_check_button_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_combo_box_entry_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_entry_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_fixed_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_frame_new (string label);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_hscale_new_with_range (gdouble min, gdouble max, gdouble step);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_hscrollbar_new (GtkAdjustmentPointer adjustment);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_notebook_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_progress_bar_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_radio_button_new (GSListPointer group);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_spin_button_new (GtkAdjustmentPointer adjustment, gdouble climb_rate, guint digits);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_statusbar_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkToolItemPointer gtk_toggle_tool_button_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_toolbar_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkToolItemPointer gtk_tool_button_new (
			GtkWidgetPointer icon_widget,
			string label);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_tree_view_new ();
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_vscale_new_with_range (gdouble min, gdouble max, gdouble step);
		[DllImport (GtkLibraryName)]
		static extern GtkWidgetPointer gtk_vscrollbar_new (GtkAdjustmentPointer adjustment);
		#endregion
		#region Painting
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_arrow (GtkStylePointer style, GdkWindowPointer window, GtkStateType state_type, GtkShadowType shadow_type, ref GdkRectangle area, GtkWidgetPointer widget, string detail, GtkArrowType arrow_type, gboolean fill, gint x, gint y, gint width, gint height);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_box (GtkStylePointer style, GdkWindowPointer window, GtkStateType state_type, GtkShadowType shadow_type, ref GdkRectangle area, GtkWidgetPointer widget, string detail, gint x, gint y, gint width, gint height);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_box_gap (
			GtkStylePointer style,
			GdkWindowPointer window,
			GtkStateType state_type,
			GtkShadowType shadow_type,
			ref GdkRectangle area,
			GtkWidgetPointer widget,
			string detail,
			gint x,
			gint y,
			gint width,
			gint height,
			GtkPositionType gap_side,
			gint gap_x,
			gint gap_width);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_check (GtkStylePointer style, GdkWindowPointer window, GtkStateType state_type, GtkShadowType shadow_type, ref GdkRectangle area, GtkWidgetPointer widget, string detail, gint x, gint y, gint width, gint height);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_expander (
			GtkStylePointer style,
			GdkWindowPointer window,
			GtkStateType state_type,
			ref GdkRectangle area,
			GtkWidgetPointer widget,
			string detail,
			gint x,
			gint y,
			GtkExpanderStyle expander_style);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_extension (
			GtkStylePointer style,
			GdkWindowPointer window,
			GtkStateType state_type,
			GtkShadowType shadow_type,
			ref GdkRectangle area,
			GtkWidgetPointer widget,
			string detail,
			gint x,
			gint y,
			gint width,
			gint height,
			GtkPositionType gap_side);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_flat_box (
			GtkStylePointer style,
			GdkWindowPointer window,
			GtkStateType state_type,
			GtkShadowType shadow_type,
			ref GdkRectangle area,
			GtkWidgetPointer widget,
			string detail,
			gint x,
			gint y,
			gint width,
			gint height);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_option (GtkStylePointer style, GdkWindowPointer window, GtkStateType state_type, GtkShadowType shadow_type, ref GdkRectangle area, GtkWidgetPointer widget, string detail, gint x, gint y, gint width, gint height);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_resize_grip (
			GtkStylePointer style,
			GdkWindowPointer window,
			GtkStateType state_type,
			ref GdkRectangle area,
			GtkWidgetPointer widget,
			string detail,
			GdkWindowEdge edge,
			gint x,
			gint y,
			gint width,
			gint height);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_shadow (GtkStylePointer style, GdkWindowPointer window, GtkStateType state_type, GtkShadowType shadow_type, ref GdkRectangle area, GtkWidgetPointer widget, string detail, gint x, gint y, gint width, gint height);
		[DllImport (GtkLibraryName)]
		static extern void gtk_paint_slider (
			GtkStylePointer style,
			GdkWindowPointer window,
			GtkStateType state_type,
			GtkShadowType shadow_type,
			ref GdkRectangle area,
			GtkWidgetPointer widget,
			string detail,
			gint x,
			gint y,
			gint width,
			gint height,
			GtkOrientation orientation);
		#endregion
		#endregion
		#region Enumerations
		internal enum GtkShadowType
		{
			GTK_SHADOW_NONE,
			GTK_SHADOW_IN,
			GTK_SHADOW_OUT,
			GTK_SHADOW_ETCHED_IN,
			GTK_SHADOW_ETCHED_OUT
		}
		enum GtkStateType
		{
			GTK_STATE_NORMAL,
			GTK_STATE_ACTIVE,
			GTK_STATE_PRELIGHT,
			GTK_STATE_SELECTED,
			GTK_STATE_INSENSITIVE
		}
		enum GtkWindowType
		{
			GTK_WINDOW_TOPLEVEL,
			GTK_WINDOW_POPUP
		}
		enum GtkArrowType
		{
			GTK_ARROW_UP,
			GTK_ARROW_DOWN,
			GTK_ARROW_LEFT,
			GTK_ARROW_RIGHT,
			GTK_ARROW_NONE
		}
		enum GtkOrientation
		{
			GTK_ORIENTATION_HORIZONTAL,
			GTK_ORIENTATION_VERTICAL
		}
		enum GtkExpanderStyle
		{
			GTK_EXPANDER_COLLAPSED,
			GTK_EXPANDER_SEMI_COLLAPSED,
			GTK_EXPANDER_SEMI_EXPANDED,
			GTK_EXPANDER_EXPANDED
		}
		enum GtkPositionType
		{
			GTK_POS_LEFT,
			GTK_POS_RIGHT,
			GTK_POS_TOP,
			GTK_POS_BOTTOM
		}
		enum GtkWidgetFlags : uint
		{
			GTK_CAN_DEFAULT = 1 << 13,
		}
		enum GdkWindowEdge
		{
			GDK_WINDOW_EDGE_NORTH_WEST,
			GDK_WINDOW_EDGE_NORTH,
			GDK_WINDOW_EDGE_NORTH_EAST,
			GDK_WINDOW_EDGE_WEST,
			GDK_WINDOW_EDGE_EAST,
			GDK_WINDOW_EDGE_SOUTH_WEST,
			GDK_WINDOW_EDGE_SOUTH,
			GDK_WINDOW_EDGE_SOUTH_EAST  
		}
		#endregion
		#region Structures
		struct GtkStyle
		{
#pragma warning disable 169
			GObject parent_instance;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=5)]
			GdkColor[] fg;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=5)]
			GdkColor[] bg;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=5)]
			GdkColor[] light;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=5)]
			GdkColor[] dark;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=5)]
			GdkColor[] mid;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=5)]
			GdkColor[] text;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=5)]
			GdkColor[] @base;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=5)]
			GdkColor[] text_aa;
			GdkColor black;
			GdkColor white;
			PangoFontDescriptionPointer font_desc;
#pragma warning restore 169
			public gint xthickness;
			public gint ythickness;
		}
		struct GtkWidget
		{
#pragma warning disable 169
			GtkObject @object;
			guint16 private_flags;
			guint8 state;
			guint8 saved_state;
			string name;
			GtkStylePointer style;
			GtkRequisition requisition;
#pragma warning restore 169
			public GtkAllocation allocation;
#pragma warning disable 169
			GdkWindowPointer window;
			GtkWidgetPointer parent;
#pragma warning restore 169
		}
		struct GtkObject
		{
#pragma warning disable 169
			GInitiallyUnowned parent_instance;
#pragma warning restore 169
			public guint32 flags;
		}
		struct GtkRequisition
		{
#pragma warning disable 169
			gint width;
			gint height;
#pragma warning restore 169
		}
		struct GtkMisc
		{
#pragma warning disable 169
			GtkWidget widget;
#pragma warning restore 169
			public gfloat xalign;
			public gfloat yalign;
			public guint16 xpad;
			public guint16 ypad;
		}
		struct GtkTreeViewColumn
		{
#pragma warning disable 169
			GtkObject parent;
			GtkWidgetPointer tree_view;
#pragma warning restore 169
			public GtkWidgetPointer button;
		}
		#endregion
		#region Macros
		static void GTK_WIDGET_SET_FLAGS (GtkWidgetPointer wid, GtkWidgetFlags flag)
		{
			GtkObject @object = (GtkObject)Marshal.PtrToStructure (wid, typeof (GtkObject));
			@object.flags |= (guint32)flag;
			Marshal.StructureToPtr (@object, wid, false);
		}
		#endregion
		#endregion
		#region GObject
		[DllImport (GobjectLibraryName)]
		static extern gpointer g_object_ref (gpointer @object);
		[DllImport (GobjectLibraryName)]
		static extern void g_object_unref (gpointer @object);
		[DllImport (GobjectLibraryName)]
		static extern gboolean g_type_check_instance_is_a (GTypeInstancePointer type_instance, GType iface_type);
		[DllImport (GobjectLibraryName)]
		static extern void g_object_get (gpointer @object, string property_name, out gboolean value, IntPtr nullTerminator);
		const int G_TYPE_FUNDAMENTAL_SHIFT = 2;
		enum G_TYPE
		{
		}
		struct GTypeInstance
		{
#pragma warning disable 169
			GTypeClassPointer g_class;
#pragma warning restore 169
		}
		internal struct GObject
		{
#pragma warning disable 169
			GTypeInstance g_type_instance;
			guint ref_count;
			GDataPointer qdata;
#pragma warning restore 169
		}
		#endregion
		#endregion
	}
	#region Exposed constants
	enum GtkPlusToggleButtonValue
	{
		Unchecked = GtkPlus.GtkShadowType.GTK_SHADOW_OUT,
		Mixed = GtkPlus.GtkShadowType.GTK_SHADOW_ETCHED_IN,
		Checked = GtkPlus.GtkShadowType.GTK_SHADOW_IN
	}
	enum GtkPlusState
	{
		Normal,
		Pressed,
		Hot,
		Selected,
		Disabled
	}
	#endregion
}
