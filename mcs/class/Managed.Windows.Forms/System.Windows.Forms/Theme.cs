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
//
// $Revision: 1.18 $
// $Modtime: $
// $Log: Theme.cs,v $
// Revision 1.18  2004/11/08 20:40:08  jackson
// Render the little scrolling jimmi in the correct location with bottom aligned tabs
//
// Revision 1.17  2004/11/04 11:26:09  ravindra
// 	- Changed default ListView values signatures (prefixed all with ListView).
// 	- Fixed default size values for VScrollBar and HScrollBar.
// 	- Fixed DrawListViewItem method.
//
// Revision 1.16  2004/11/02 02:47:55  jackson
// New rendering and sizing code for tab controls
//
// Revision 1.15  2004/10/30 10:23:02  ravindra
// Drawing ListView and some default values.
//
// Revision 1.14  2004/10/26 09:35:18  ravindra
// Added some default values for ListView control.
//
// Revision 1.13  2004/10/18 04:49:06  pbartok
// - Added ToolTip abstracts
//
// Revision 1.12  2004/10/15 15:08:49  ravindra
// Added ColumnHeaderHeight property in Theme for ListView.
//
// Revision 1.11  2004/10/05 09:03:55  ravindra
// 	- Added DrawListView method and ListViewDefaultSize property.
//
// Revision 1.10  2004/09/28 18:44:25  pbartok
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
// Revision 1.9  2004/09/17 12:18:42  jordi
// Very early menu support
//
// Revision 1.8  2004/09/07 17:12:26  jordi
// GroupBox control
//
// Revision 1.7  2004/09/07 09:40:15  jordi
// LinkLabel fixes, methods, multiple links
//
// Revision 1.6  2004/09/02 16:32:54  jordi
// implements resource pool for pens, brushes, and hatchbruses
//
// Revision 1.5  2004/08/25 20:04:40  ravindra
// Added the missing divider code and grip for ToolBar Control.
//
// Revision 1.4  2004/08/24 18:37:02  jordi
// fixes formmating, methods signature, and adds missing events
//
// Revision 1.3  2004/08/24 16:16:46  jackson
// Handle drawing picture boxes in the theme now. Draw picture box borders and obey sizing modes
//
// Revision 1.2  2004/08/20 00:12:51  jordi
// fixes methods signature
//
// Revision 1.1  2004/08/19 22:26:30  jordi
// move themes from an interface to a class
//
//

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;

namespace System.Windows.Forms
{
	
	
	// Implements a pool of system resources	
	internal class SystemResPool
	{
		private Hashtable pens = new Hashtable ();
		private Hashtable solidbrushes = new Hashtable ();
		private Hashtable hatchbrushes = new Hashtable ();
		
		public SystemResPool () {}
		
		public Pen GetPen (Color color)
		{
			string hash = color.ToString();			
			
			if (pens.Contains (hash))
				return (Pen) pens[hash];				
			
			Pen pen = new Pen (color);
			pens.Add (hash, pen);
			return pen;
		}		
		
		public SolidBrush GetSolidBrush (Color color)
		{
			string hash = color.ToString ();
						
			if (solidbrushes.Contains (hash))
				return (SolidBrush) solidbrushes[hash];							
			
			SolidBrush brush = new SolidBrush (color);
			solidbrushes.Add (hash, brush);
			return brush;
		}		
		
		public HatchBrush GetHatchBrush (HatchStyle hatchStyle, Color foreColor, Color backColor)
		{
			string hash = hatchStyle.ToString () + foreColor.ToString () + backColor.ToString ();			
						
			if (hatchbrushes.Contains (hash))
				return (HatchBrush) hatchbrushes[hash];							
			
			HatchBrush brush = new HatchBrush (hatchStyle, foreColor, backColor);
			hatchbrushes.Add (hash, brush);
			return brush;
		}
		
	}

	internal abstract class Theme
	{		
		protected Array syscolors;
		protected Font default_font;
		protected Color defaultWindowBackColor;
		protected Color defaultWindowForeColor;		
		internal SystemResPool ResPool = new SystemResPool ();
	
		/* Default properties */		
		public virtual Color ColorScrollbar {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_SCROLLBAR);}
		}

		public virtual Color ColorBackground {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_BACKGROUND);}
		}

		public virtual Color ColorActiveTitle {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_ACTIVECAPTION);}
		}

		public virtual Color ColorInactiveTitle {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_INACTIVECAPTION);}
		}

		public virtual Color ColorMenu {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_MENU);}
		}

		public virtual Color ColorWindow {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_WINDOW);}
		}

		public virtual Color ColorWindowFrame {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_WINDOWFRAME);}
		}

		public virtual Color ColorMenuText {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_MENUTEXT);}
		}

		public virtual Color ColorWindowText {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_WINDOWTEXT);}
		}

		public virtual Color ColorTitleText {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_CAPTIONTEXT);}
		}

		public virtual Color ColorActiveBorder {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_ACTIVEBORDER);}
		}

		public virtual Color ColorInactiveBorder{
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_INACTIVEBORDER);}
		}

		public virtual Color ColorAppWorkSpace {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_APPWORKSPACE);}
		}

		public virtual Color ColorHilight {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_HIGHLIGHT);}
		}

		public virtual Color ColorHilightText {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_HIGHLIGHTTEXT);}
		}

		public virtual Color ColorButtonFace {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_BTNFACE);}
		}

		public virtual Color ColorButtonShadow {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_BTNSHADOW);}
		}

		public virtual Color ColorGrayText {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_GRAYTEXT);}
		}

		public virtual Color ColorButtonText {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_BTNTEXT);}
		}

		public virtual Color ColorInactiveTitleText {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_INACTIVECAPTIONTEXT);}
		}

		public virtual Color ColorButtonHilight {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_BTNHIGHLIGHT);}
		}

		public virtual Color ColorButtonDkShadow {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_3DDKSHADOW);}
		}

		public virtual Color ColorButtonLight {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_3DLIGHT);}
		}

		public virtual Color ColorInfoText {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_INFOTEXT);}
		}

		public virtual Color ColorInfoWindow {
			get {return GetColor (XplatUIWin32.GetSysColorIndex.COLOR_INFOBK);}
		}

		public virtual Color DefaultControlBackColor {
			get { return ColorButtonFace; }
		}

		public virtual Color DefaultControlForeColor {
			get { return ColorButtonText; }
		}

		public virtual Font DefaultFont {
			get { return default_font; }
		}

		public virtual Color DefaultWindowBackColor {
			get { return defaultWindowBackColor; }			
		}

		public virtual Color DefaultWindowForeColor {
			get { return defaultWindowForeColor; }
		}

		public virtual Color GetColor (XplatUIWin32.GetSysColorIndex idx)
		{
			return (Color) syscolors.GetValue ((int)idx);
		}

		public virtual void SetColor (XplatUIWin32.GetSysColorIndex idx, Color color)
		{
			syscolors.SetValue (color, (int) idx);
		}

		#region Principal Theme Methods
		// If the theme writes directly to a window instead of a device context
		public abstract bool DoubleBufferingSupported {get;}
		#endregion	// Principal Theme Methods

		#region	OwnerDraw Support
		public abstract void DrawOwnerDrawBackground (DrawItemEventArgs e);
		public abstract void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e);
		#endregion	// OwnerDraw Support

		#region Button
		#endregion	// Button

		#region ButtonBase
		// Drawing
		public abstract void DrawButtonBase(Graphics dc, Rectangle clip_area, ButtonBase button);

		// Sizing
		public abstract Size ButtonBaseDefaultSize{get;}
		#endregion	// ButtonBase

		#region CheckBox
		public abstract void DrawCheckBox(Graphics dc, Rectangle clip_area, CheckBox checkbox);
		#endregion	// CheckBox

		#region Control
		#endregion	// Control

		#region GroupBox
		// Drawing
		public abstract void DrawGroupBox (Graphics dc,  Rectangle clip_area, GroupBox box);

		// Sizing
		public abstract Size GroupBoxDefaultSize{get;}
		#endregion	// GroupBox

		#region HScrollBar
		public abstract Size HScrollBarDefaultSize{get;}	// Default size of the scrollbar
		#endregion	// HScrollBar

		#region Label
		// Drawing
		public abstract void DrawLabel (Graphics dc, Rectangle clip_rectangle, Label label);

		// Sizing
		public abstract Size LabelDefaultSize{get;}
		#endregion	// Label

		#region LinkLabel
		#endregion	// LinkLabel
		
		#region ListBox
		// Drawing
		public abstract void DrawListBox (Graphics dc, Rectangle clip_rectangle, ListBox control);
		public abstract void DrawListBoxItem (Graphics dc, int elem, Rectangle rect, ListBox ctrl);		
		

		// Sizing
		public abstract int DrawListBoxDecorationTop (BorderStyle border_style);
		public abstract int DrawListBoxDecorationBottom (BorderStyle border_style);
		public abstract int DrawListBoxDecorationRight (BorderStyle border_style);
		public abstract int DrawListBoxDecorationLeft (BorderStyle border_style);
		#endregion	// ListBox
		
		#region ListView
		// Drawing
		public abstract void DrawListView (Graphics dc, Rectangle clip_rectangle, ListView control);

		// Sizing
		public abstract Size ListViewCheckBoxSize { get; }
		public abstract int ListViewColumnHeaderHeight { get; }
		public abstract int ListViewDefaultColumnWidth { get; }
		public abstract int ListViewVerticalSpacing { get; }
		public abstract int ListViewEmptyColumnWidth { get; }
		public abstract int ListViewHorizontalSpacing { get; }
		public abstract Size ListViewDefaultSize { get; }
		#endregion	// ListView

		#region Panel
		// Sizing
		public abstract Size PanelDefaultSize{get;}
		#endregion	// Panel

		#region PictureBox
		// Drawing
		public abstract void DrawPictureBox (Graphics dc, PictureBox pb);

		// Sizing
		public abstract Size PictureBoxDefaultSize{get;}
		#endregion	// PictureBox

		#region ProgressBar
		// Drawing
		public abstract void DrawProgressBar (Graphics dc, Rectangle clip_rectangle, ProgressBar progress_bar);

		// Sizing
		public abstract Size ProgressBarDefaultSize{get;}
		#endregion	// ProgressBar

		#region RadioButton
		// Drawing
		public abstract void DrawRadioButton (Graphics dc, Rectangle clip_rectangle, RadioButton radio_button);

		// Sizing
		public abstract Size RadioButtonDefaultSize{get;}
		#endregion	// RadioButton

		#region ScrollBar
		// Drawing
		//public abstract void DrawScrollBar (Graphics dc, Rectangle area, ScrollBar bar, ref Rectangle thumb_pos, ref Rectangle first_arrow_area, ref Rectangle second_arrow_area, ButtonState first_arrow, ButtonState second_arrow, ref int scrollbutton_width, ref int scrollbutton_height, bool vert);
		public abstract void DrawScrollBar (Graphics dc, Rectangle clip_rectangle, ScrollBar bar);

		// Sizing
		public abstract int ScrollBarButtonSize {get;}		// Size of the scroll button
		#endregion	// ScrollBar

		#region StatusBar
		// Drawing
		public abstract void DrawStatusBar (Graphics dc, Rectangle clip_rectangle, StatusBar sb);

		// Sizing
		public abstract int StatusBarSizeGripWidth {get;}		// Size of Resize area
		public abstract int StatusBarHorzGapWidth {get;}	// Gap between panels
		public abstract Size StatusBarDefaultSize{get;}
		#endregion	// StatusBar

		#region TabControl
		public abstract Size TabControlDefaultItemSize { get; }
		public abstract Point TabControlDefaultPadding { get; }
		public abstract int TabControlMinimumTabWidth { get; }

		public abstract Rectangle GetTabControlLeftScrollRect (TabControl tab);
		public abstract Rectangle GetTabControlRightScrollRect (TabControl tab);
		public abstract Rectangle GetTabControlDisplayRectangle (TabControl tab);
		public abstract Size TabControlGetSpacing (TabControl tab);
		public abstract void DrawTabControl (Graphics dc, Rectangle area, TabControl tab);
		#endregion

		#region	ToolBar
		// Drawing
		public abstract void DrawToolBar (Graphics dc, Rectangle clip_rectangle, ToolBar control);

		// Sizing
		public abstract int ToolBarGripWidth {get;}		 // Grip width for the ToolBar
		public abstract int ToolBarImageGripWidth {get;}	 // Grip width for the Image on the ToolBarButton
		public abstract int ToolBarSeparatorWidth {get;}	 // width of the separator
		public abstract int ToolBarDropDownWidth { get; }	 // width of the dropdown arrow rect
		public abstract int ToolBarDropDownArrowWidth { get; }	 // width for the dropdown arrow on the ToolBarButton
		public abstract int ToolBarDropDownArrowHeight { get; }	 // height for the dropdown arrow on the ToolBarButton
		public abstract Size ToolBarDefaultSize{get;}
		#endregion	// ToolBar

		#region ToolTip
		public abstract void DrawToolTip(Graphics dc, Rectangle clip_rectangle, ToolTip tt);
		public abstract Size ToolTipSize(ToolTip tt, string text);
		#endregion	// ToolTip

		#region MonthCalendar

		public abstract void DrawMonthCalendar(Graphics dc, Rectangle clip_rectangle, MonthCalendar month_calendar);

		#endregion 	// MonthCalendar

		#region TrackBar
		// Drawing
		public abstract void DrawTrackBar (Graphics dc, Rectangle clip_rectangle, TrackBar tb);
//public abstract void DrawTrackBar (Graphics dc, Rectangle area, TrackBar tb, 
//ref Rectangle thumb_pos, 
//ref Rectangle thumb_area, 
//bool highli_thumb, 
//float ticks, 
//int value_pos, 
//bool mouse_value);

		// Sizing
		public abstract Size TrackBarDefaultSize{get; }		// Default size for the TrackBar control
		#endregion	// TrackBar

		#region VScrollBar
		public abstract Size VScrollBarDefaultSize{get;}	// Default size of the scrollbar
		#endregion	// VScrollBar


		#region	ControlPaint Methods
		public abstract void CPDrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle);

		public abstract void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides);
		public abstract void CPDrawButton (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state);
		public abstract void CPDrawCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawContainerGrabHandle (Graphics graphics, Rectangle bounds);
		public abstract void CPDrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor);
		public abstract void CPDrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled);
		public abstract void CPDrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor);
		public abstract void CPDrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background);
		public abstract void CPDrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary);
		public abstract void CPDrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph);
		public abstract void CPDrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state);
		public abstract void CPDrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style);
		public abstract void CPDrawReversibleLine (Point start, Point end, Color backColor);
		public abstract void CPDrawScrollButton (Graphics graphics, Rectangle rectangle, ScrollButton button, ButtonState state);
		public abstract void CPDrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor);
		public abstract void CPDrawSizeGrip (Graphics graphics, Color backColor, Rectangle bounds);
		public abstract void CPDrawStringDisabled (Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle,
			StringFormat format);
		public abstract void CPDrawBorderStyle (Graphics dc, Rectangle area, BorderStyle border_style);
		#endregion	// ControlPaint Methods
	}
}
