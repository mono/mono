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
//
// $Revision: 1.8 $
// $Modtime: $
// $Log: ITheme.cs,v $
// Revision 1.8  2004/08/10 19:21:27  jordi
// scrollbar enhancements and standarize on win colors defaults
//
// Revision 1.7  2004/08/10 18:52:30  jackson
// Implement DrawItem functionality
//
// Revision 1.6  2004/08/09 21:36:34  jackson
// Add support for drawing status bar and getting status bar item sizes
//
// Revision 1.5  2004/08/09 17:01:02  jackson
// The theme engine now controls default window properties
//
// Revision 1.3  2004/08/08 22:54:21  jordi
// Label BorderStyles
//
// Revision 1.2  2004/08/07 19:05:44  jordi
// Theme colour support and GetSysColor defines
//
// Revision 1.1  2004/07/26 17:42:03  jordi
// Theme support
//
//

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Windows.Forms
{
	internal interface ITheme
	{
		/* Default properties */
		Color DefaultControlBackColor {get;}
		Color DefaultControlForeColor {get;}
		Font DefaultFont {get;}
		Color DefaultWindowForeColor {get;}
		Color DefaultWindowBackColor {get;}

		/* Windows System Colors. Based on Wine */
		Color ColorScrollbar {get;} 		//COLOR_SCROLLBAR
		Color ColorBackground {get;} 		//COLOR_BACKGROUND
		Color ColorActiveTitle {get;} 		//COLOR_ACTIVECAPTION
		Color ColorInactiveTitle {get;}		//COLOR_INACTIVECAPTION
		Color ColorMenu {get;} 			//COLOR_MENU
		Color ColorWindow {get;} 		//COLOR_WINDOW
		Color ColorWindowFrame {get;} 		//COLOR_WINDOWFRAME
		Color ColorMenuText {get;} 		//COLOR_MENUTEXT 
		Color ColorWindowText {get;} 		//COLOR_WINDOWTEXT
		Color ColorTitleText {get;} 		//COLOR_CAPTIONTEXT 
		Color ColorActiveBorder {get;} 		//COLOR_ACTIVEBORDER
		Color ColorInactiveBorder {get;} 	//COLOR_INACTIVEBORDER 
		Color ColorAppWorkSpace {get;} 		//COLOR_APPWORKSPACE
		Color ColorHilight {get;} 		//COLOR_HIGHLIGHT
		Color ColorHilightText {get;} 		//COLOR_HIGHLIGHTTEXT			
		Color ColorButtonFace {get;} 		//COLOR_BTNFACE
		Color ColorButtonShadow {get;} 		//COLOR_BTNSHADOW
		Color ColorGrayText {get;} 		//COLOR_GRAYTEXT
		Color ColorButtonText {get;} 		//COLOR_BTNTEXT
		Color ColorInactiveTitleText {get;} 	//COLOR_INACTIVECAPTIONTEXT
		Color ColorButtonHilight {get;} 	//COLOR_BTNHIGHLIGHT
		Color ColorButtonDkShadow {get;} 	//COLOR_3DDKSHADOW
		Color ColorButtonLight {get;} 		//COLOR_3DLIGHT
		Color ColorInfoText {get;} 		//COLOR_INFOTEXT
		Color ColorInfoWindow {get;} 		//COLOR_INFOBK
		Color ColorButtonAlternateFace {get;} 	//COLOR_ALTERNATEBTNFACE
		Color ColorHotTrackingColor {get;} 	//COLOR_HOTLIGHT
		Color ColorGradientActiveTitle {get;} 	//COLOR_GRADIENTACTIVECAPTION
		Color ColorGradientInactiveTitle {get;} //COLOR_GRADIENTINACTIVECAPTION

		/*
		  Control sizing properties
		*/

		int SizeGripWidth {get;}
		int StatusBarHorzGapWidth {get;}
		int ScrollBarButtonSize {get;}

		/*
			Methods that mimic ControlPaint signature and draw basic objects
		*/

		void DrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle);

		void DrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides);

		void DrawButton (Graphics graphics, Rectangle rectangle, ButtonState state);

		void DrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state);

		void DrawCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state);

		void DrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state);

		void DrawContainerGrabHandle (Graphics graphics, Rectangle bounds);

		void DrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor);

		void DrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled);

		void DrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor);

		void DrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background);

		void DrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary);

		void DrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph);

		void DrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state);

		void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style);

		void DrawReversibleLine (Point start, Point end, Color backColor);

		void DrawScrollButton (Graphics graphics, Rectangle rectangle, ScrollButton button, ButtonState state);

		void DrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor);

		void DrawSizeGrip (Graphics graphics, Color backColor, Rectangle bounds);

		void DrawStringDisabled (Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle,
			StringFormat format);

		/*
			Methods that draw complex controls
		*/

		void DrawLabel (Graphics dc, Rectangle area, BorderStyle border_style, string text, 
			Color fore_color, Color back_color, Font font, StringFormat string_format, bool Enabled);

		void DrawScrollBar (Graphics dc, Rectangle area, Rectangle thumb_pos,
			ref Rectangle first_arrow_area, ref Rectangle second_arrow_area,
			ButtonState first_arrow, ButtonState second_arrow,
			ref int scrollbutton_width, ref int scrollbutton_height,
			bool enabled, bool vertical);


		void DrawTrackBar (Graphics dc, Rectangle area, ref Rectangle thumb_pos,
			 ref Rectangle thumb_area, TickStyle style, int ticks, Orientation orientation, bool focused);

		void DrawProgressBar (Graphics dc, Rectangle area,  Rectangle client_area,
			int barpos_pixels, int block_width);		

		void DrawStatusBar (Graphics dc, Rectangle area, StatusBar sb);

		void DrawOwnerDrawBackground (DrawItemEventArgs e);

		void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e);

	}
}
