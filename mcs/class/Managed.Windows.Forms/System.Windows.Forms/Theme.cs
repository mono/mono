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
// $Revision: 1.2 $
// $Modtime: $
// $Log: Theme.cs,v $
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

namespace System.Windows.Forms
{
	internal abstract class Theme
	{
		protected Array syscolors;
		protected Font default_font;
		protected Color defaultWindowBackColor;
		protected Color defaultWindowForeColor;	
	
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

		// If the theme writes directly to a window instead of a device context
		public abstract bool WriteToWindow {get;}

		/*
		  	Control sizing properties
		*/

		public abstract int SizeGripWidth {get;}
		public abstract int StatusBarHorzGapWidth {get;}
		public abstract int ScrollBarButtonSize {get;}

		/*
		  	ToolBar Control properties
		 */
		public abstract int ToolBarImageGripWidth {get;}         // Grip width for the Image on the ToolBarButton
		public abstract int ToolBarSeparatorWidth {get;}         // width of the separator
		public abstract int ToolBarDropDownWidth { get; }        // width of the dropdown arrow rect
		public abstract int ToolBarDropDownArrowWidth { get; }   // width for the dropdown arrow on the ToolBarButton
		public abstract int ToolBarDropDownArrowHeight { get; }  // height for the dropdown arrow on the ToolBarButton

		/*
			Methods that mimic ControlPaint signature and draw basic objects
		*/

		public abstract void DrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle);

		public abstract void DrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides);

		public abstract void DrawButton (Graphics graphics, Rectangle rectangle, ButtonState state);

		public abstract void DrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state);

		public abstract void DrawCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state);

		public abstract void DrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state);

		public abstract void DrawContainerGrabHandle (Graphics graphics, Rectangle bounds);

		public abstract void DrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor);

		public abstract void DrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled);

		public abstract void DrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor);

		public abstract void DrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background);

		public abstract void DrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary);

		public abstract void DrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph);

		public abstract void DrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state);

		public abstract void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style);

		public abstract void DrawReversibleLine (Point start, Point end, Color backColor);

		public abstract void DrawScrollButton (Graphics graphics, Rectangle rectangle, ScrollButton button, ButtonState state);

		public abstract void DrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor);

		public abstract void DrawSizeGrip (Graphics graphics, Color backColor, Rectangle bounds);

		public abstract void DrawStringDisabled (Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle,
			StringFormat format);

		/*
			Methods that draw complete controls			
		*/

		public abstract void DrawLabel (Graphics dc, Rectangle area, BorderStyle border_style, string text, 
			Color fore_color, Color back_color, Font font, StringFormat string_format, bool Enabled);

		public abstract void DrawScrollBar (Graphics dc, Rectangle area, ScrollBar bar,
			Rectangle thumb_pos, ref Rectangle first_arrow_area, ref Rectangle second_arrow_area, 
			ButtonState first_arrow, ButtonState second_arrow, ref int scrollbutton_width, 
			ref int scrollbutton_height, bool vert);

		public abstract void DrawTrackBar (Graphics dc, Rectangle area, TrackBar tb,
				ref Rectangle thumb_pos, ref Rectangle thumb_area, bool highli_thumb,
				float ticks, int value_pos, bool mouse_value);

		public abstract void DrawProgressBar (Graphics dc, Rectangle area,  Rectangle client_area,
			int barpos_pixels, int block_width);

		public abstract void DrawToolBar (Graphics dc, ToolBar control, StringFormat format);

		public abstract void DrawStatusBar (Graphics dc, Rectangle area, StatusBar sb);

		public abstract void DrawOwnerDrawBackground (DrawItemEventArgs e);

		public abstract void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e);

		public abstract void DrawStatusBarPanel (Graphics dc, Rectangle area, int index,
			SolidBrush br_forecolor, StatusBarPanel panel);
		

	}
}
