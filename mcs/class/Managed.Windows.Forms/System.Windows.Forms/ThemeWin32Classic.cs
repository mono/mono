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
//	Peter Bartok, pbartok@novell.com
//
//
//
// $Revision: 1.22 $
// $Modtime: $
// $Log: ThemeWin32Classic.cs,v $
// Revision 1.22  2004/08/17 19:29:11  jackson
// Don't use KnownColor to create colours. It has a large startup time.
//
// Revision 1.21  2004/08/15 23:20:54  ravindra
// Changes to Theme for ToolBar control and also dos2unix format.
//
// Revision 1.20  2004/08/13 21:22:18  jordi
// removes redundant code and fixes issues with tickposition
//
// Revision 1.19  2004/08/12 20:29:01  jordi
// Trackbar enhancement, fix mouse problems, highli thumb, etc
//
// Revision 1.18  2004/08/12 18:54:37  jackson
// Handle owner draw status bars
//
// Revision 1.17  2004/08/11 01:31:35  jackson
// Create Brushes as little as possible
//
// Revision 1.16  2004/08/10 19:21:27  jordi
// scrollbar enhancements and standarize on win colors defaults
//
// Revision 1.15  2004/08/10 18:52:30  jackson
// Implement DrawItem functionality
//
// Revision 1.14  2004/08/09 21:34:54  jackson
// Add support for drawing status bar and get status bar item sizes
//
// Revision 1.13  2004/08/09 21:21:49  jackson
// Use known colors for default control colours
//
// Revision 1.12  2004/08/09 21:12:15  jackson
// Make the default font static, it is static in control so this doesn't change functionality and creating fonts is sloooooow.
//
// Revision 1.11  2004/08/09 17:31:13  jackson
// New names for control properties
//
// Revision 1.10  2004/08/09 17:00:00  jackson
// Add default window color properties
//
// Revision 1.9  2004/08/09 16:17:19  jackson
// Use correct default back color
//
// Revision 1.8  2004/08/09 15:53:12  jackson
// Themes now handle default control properties so coloring will be consistent
//
// Revision 1.7  2004/08/08 22:54:21  jordi
// Label BorderStyles
//
// Revision 1.6  2004/08/08 18:09:53  jackson
// Add pen_buttonface
//
// Revision 1.5  2004/08/08 17:34:28  jordi
// Use Windows Standard Colours
//
// Revision 1.4  2004/08/07 23:31:15  jordi
// fixes label bug and draw method name
//
// Revision 1.3  2004/08/07 19:05:44  jordi
// Theme colour support and GetSysColor defines
//
// Revision 1.2  2004/08/07 00:01:39  pbartok
// - Fixed some rounding issues with float/int
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
	internal class ThemeWin32Classic : ITheme
	{		
		static private Pen pen_ticks;
		static private Pen pen_disabled;
		static private SolidBrush br_arrow;
		static private SolidBrush br_disabled;
		static private HatchBrush br_focus;		
		static private SolidBrush br_progressbarblock;		
		static private Pen pen_arrow;

		static private SolidBrush br_buttonface;
		static private SolidBrush br_buttonshadow;
		static private SolidBrush br_buttondkshadow;
		static private SolidBrush br_buttonhilight;
		static private SolidBrush br_buttontext;
		static private SolidBrush br_menutext;

		static private Pen pen_buttonshadow;
		static private Pen pen_buttondkshadow;
		static private Pen pen_buttonhilight;
		static private Pen pen_buttonface;
		static private Pen pen_buttontext;
		static private Pen pen_windowframe;

		static private Font default_font;

		/* Cache */
		private SolidBrush label_br_fore_color;
		private SolidBrush label_br_back_color;		
		private HatchBrush br_scrollbar_backgr;
		private HatchBrush br_trackbar_thumbhili;
		private SolidBrush br_trackbarbg;

		public ThemeWin32Classic ()
		{
			label_br_fore_color = null;
			label_br_back_color = null;
			br_scrollbar_backgr = null;
			br_trackbarbg = null;
			br_trackbar_thumbhili = null;
 
			pen_ticks = new Pen (Color.Black);			
			br_arrow = new SolidBrush (Color.Black);			
			br_focus = new HatchBrush (HatchStyle.Percent50, ColorButtonFace, Color.Black);
			pen_arrow = new Pen (Color.Black);
			br_progressbarblock = new  SolidBrush (Color.FromArgb (255, 49, 106, 197));			

			br_buttonface = new SolidBrush (ColorButtonFace);
			br_buttonshadow = new SolidBrush (ColorButtonShadow);
			br_buttondkshadow = new SolidBrush (ColorButtonDkShadow);
			br_buttonhilight = new SolidBrush (ColorButtonHilight);
			br_buttontext = new SolidBrush (ColorButtonText);
			pen_buttonshadow = new Pen (ColorButtonShadow);
			pen_buttondkshadow = new Pen (ColorButtonDkShadow);
			pen_buttonhilight = new Pen (ColorButtonHilight);
			pen_buttonface = new Pen (ColorButtonFace);
			pen_buttontext = new Pen (ColorButtonText);
			pen_windowframe = new Pen (ColorWindowFrame);

			default_font =	new Font (FontFamily.GenericSansSerif, 8.25f);
		}
		/* Windows System Colors. Based on Wine */
		public Color ColorScrollbar {
			get {return Color.FromArgb (255, 192, 192, 192);}
		}

		public Color ColorBackground{
			get {return Color.FromArgb (255, 0, 128, 128);}
		}

		public Color ColorActiveTitle{
			get {return Color.FromArgb (255, 0, 0, 128);}
		}

		public Color ColorInactiveTitle{
			get {return Color.FromArgb (255, 128, 128, 128);}
		}

		public Color ColorMenu{
			get {return Color.FromArgb (255, 192, 192, 192);}
		}

		public Color ColorWindow{
			get {return Color.FromArgb (255, 255, 255, 255);}
		}

		public Color ColorWindowFrame{
			get {return Color.FromArgb (255, 0, 0, 0);}
		}

		public Color ColorMenuText{
			get {return Color.FromArgb (255, 0, 0, 0);}
		}

		public Color ColorWindowText{
			get {return Color.FromArgb (255, 0, 0, 0);}
		}

		public Color ColorTitleText{
			get {return Color.FromArgb (255, 255, 255, 255);}
		}

		public Color ColorActiveBorder{
			get {return Color.FromArgb (255, 192, 192, 192);}
		}

		public Color ColorInactiveBorder{
			get {return Color.FromArgb (255, 192, 192, 192);}
		}

		public Color ColorAppWorkSpace{
			get {return Color.FromArgb (255, 128, 128, 128);}
		}

		public Color ColorHilight{
			get {return Color.FromArgb (255, 0, 0, 128);}
		}

		public Color ColorHilightText{
			get {return Color.FromArgb (255, 255, 255, 255);}
		}

		public Color ColorButtonFace{
			get {return Color.FromArgb (255, 192, 192, 192);}
		}

		public Color ColorButtonShadow{
			get {return Color.FromArgb (255, 128, 128, 128);}
		}

		public Color ColorGrayText{
			get {return Color.FromArgb (255, 128, 128, 128);}
		}

		public Color ColorButtonText{
			get {return Color.FromArgb (255, 0, 0, 0);}
		}

		public Color ColorInactiveTitleText{
			get {return Color.FromArgb (255, 192, 192, 192);}
		}

		public Color ColorButtonHilight{
			get {return Color.FromArgb (255, 255, 255, 255);}
		}

		public Color ColorButtonDkShadow{
			get {return Color.FromArgb (255, 0, 0, 0);}
		}

		public Color ColorButtonLight{
			get {return Color.FromArgb (255, 224, 224, 224);}
		}

		public Color ColorInfoText{
			get {return Color.FromArgb (255, 0, 0, 0);}
		}

		public Color ColorInfoWindow{
			get {return Color.FromArgb (255, 255, 255, 225);}
		}

		public Color ColorButtonAlternateFace{
			get {return Color.FromArgb (255, 180, 180, 180);}
		}

		public Color ColorHotTrackingColor{
			get {return Color.FromArgb (255, 0, 0, 255);}
		}

		public Color ColorGradientActiveTitle{
			get {return Color.FromArgb (255, 16, 132, 208);}
		}

		public Color ColorGradientInactiveTitle {
			get {return Color.FromArgb (255, 181, 181, 181);}
		}

		public Color DefaultControlBackColor {
			get { return ColorButtonFace; }
		}

		public Color DefaultControlForeColor {
			get { return ColorButtonText; }
		}

		public Font DefaultFont {
			get { return default_font; }
		}

		public Color DefaultWindowBackColor {
			get { return Color.FromArgb (255, 10, 10, 10); }
		}

		public Color DefaultWindowForeColor {
			get { return ColorButtonText; }
		}

		public int SizeGripWidth {
			get { return 15; }
		}

		public int StatusBarHorzGapWidth {
			get { return 3; }
		}

		public int ScrollBarButtonSize {
			get { return 16; }
		}

		/*
		 * ToolBar Control properties
		 */
		// Grip width for the Image on the ToolBarButton
		public int ToolBarImageGripWidth {
			get { return 2;}
		}

		// width of the separator
		public int ToolBarSeparatorWidth {
			get { return 4; }
		}

		// width of the dropdown arrow rect
		public int ToolBarDropDownWidth {
			get { return 13; }
		}

		// width for the dropdown arrow on the ToolBarButton
		public int ToolBarDropDownArrowWidth {
			get { return 5;}
		}

		// height for the dropdown arrow on the ToolBarButton
		public int ToolBarDropDownArrowHeight {
			get { return 3;}
		}

		private enum DrawFrameControlStates
		{
			ButtonCheck		= 0x0000,
			ButtonRadioImage	= 0x0001,
			ButtonRadioMask		= 0x0002,
			ButtonRadio		= 0x0004,
			Button3State		= 0x0008,
			ButtonPush		= 0x0010,

			CaptionClose		= 0x0000,
			CaptionMin		= 0x0001,
			CaptionMax		= 0x0002,
			CaptionRestore		= 0x0004,
			CaptionHelp		= 0x0008,

			MenuArrow		= 0x0000,
			MenuCheck		= 0x0001,
			MenuBullet		= 0x0002,
			MenuArrowRight		= 0x0004,

			ScrollUp		= 0x0000,
			ScrollDown		= 0x0001,
			ScrollLeft		= 0x0002,
			ScrollRight		= 0x0003,
			ScrollComboBox		= 0x0005,
			ScrollSizeGrip		= 0x0008,
			ScrollSizeGripRight	= 0x0010,

			Inactive		= 0x0100,
			Pushed			= 0x0200,
			Checked			= 0x0400,
			Transparent		= 0x0800,
			Hot			= 0x1000,
			AdjustRect		= 0x2000,
			Flat			= 0x4000,
			Mono			= 0x8000

		}

		private enum DrawFrameControlTypes
		{
			Caption	= 1,
			Menu	= 2,
			Scroll	= 3,
			Button	= 4
		}

		/*
			Methods that mimic ControlPaint signature and draw basic objects
		*/

		public void DrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle)
		{
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
		}

		public void DrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides)
		{
			Pen			penTopLeft;
			Pen			penTopLeftInner;
			Pen			penBottomRight;
			Pen			penBottomRightInner;
			Rectangle	rect= new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			bool			doInner = false;

			if ((style & Border3DStyle.Adjust)!=0) {
				rect.Y-=2;
				rect.X-=2;
				rect.Width+=4;
				rect.Height+=4;
			}

			/* default to flat */
			penTopLeft=SystemPens.ControlDark;
			penTopLeftInner=SystemPens.ControlDark;
			penBottomRight=SystemPens.ControlDark;
			penBottomRightInner=SystemPens.ControlDark;

			if ((style & Border3DStyle.RaisedOuter)!=0) {
				penTopLeft=SystemPens.ControlLightLight;
				penBottomRight=SystemPens.ControlDarkDark;
				if ((style & (Border3DStyle.RaisedInner | Border3DStyle.SunkenInner))!=0) {
					doInner=true;
				}
			} else if ((style & Border3DStyle.SunkenOuter)!=0) {
				penTopLeft=SystemPens.ControlDarkDark;
				penBottomRight=SystemPens.ControlLightLight;
				if ((style & (Border3DStyle.RaisedInner | Border3DStyle.SunkenInner))!=0) {
					doInner=true;
				}
			}

			if ((style & Border3DStyle.RaisedInner)!=0) {
				if (doInner) {
					penTopLeftInner=SystemPens.ControlLight;
					penBottomRightInner=SystemPens.ControlDark;
				} else {
					penTopLeft=SystemPens.ControlLightLight;
					penBottomRight=SystemPens.ControlDarkDark;
				}
			} else if ((style & Border3DStyle.SunkenInner)!=0) {
				if (doInner) {
					penTopLeftInner=SystemPens.ControlDark;
					penBottomRightInner=SystemPens.ControlLight;
				} else {
					penTopLeft=SystemPens.ControlDarkDark;
					penBottomRight=SystemPens.ControlLightLight;
				}
			}

			if ((sides & Border3DSide.Middle)!=0) {
				graphics.FillRectangle(SystemBrushes.Control, rect);
			}

			if ((sides & Border3DSide.Left)!=0) {
				graphics.DrawLine(penTopLeft, rect.Left, rect.Bottom-1, rect.Left, rect.Top);
				if (doInner) {
					graphics.DrawLine(penTopLeftInner, rect.Left+1, rect.Bottom-1, rect.Left+1, rect.Top);
				}
			}

			if ((sides & Border3DSide.Top)!=0) {
				graphics.DrawLine(penTopLeft, rect.Left, rect.Top, rect.Right-1, rect.Top);

				if (doInner) {
					if ((sides & Border3DSide.Left)!=0) {
						graphics.DrawLine(penTopLeftInner, rect.Left+1, rect.Top+1, rect.Right-1, rect.Top+1);
					} else {
						graphics.DrawLine(penTopLeftInner, rect.Left, rect.Top+1, rect.Right-1, rect.Top+1);
					}
				}
			}

			if ((sides & Border3DSide.Right)!=0) {
				graphics.DrawLine(penBottomRight, rect.Right-1, rect.Top, rect.Right-1, rect.Bottom-1);

				if (doInner) {
					if ((sides & Border3DSide.Top)!=0) {
						graphics.DrawLine(penBottomRightInner, rect.Right-2, rect.Top+1, rect.Right-2, rect.Bottom-1);
					} else {
						graphics.DrawLine(penBottomRightInner, rect.Right-2, rect.Top, rect.Right-2, rect.Bottom-1);
					}
				}
			}

			if ((sides & Border3DSide.Bottom)!=0) {
				int	left=rect.Left;

				if ((sides & Border3DSide.Left)!=0) {
					left+=1;
				}

				graphics.DrawLine(penBottomRight, rect.Left, rect.Bottom-1, rect.Right-1, rect.Bottom-1);

				if (doInner) {
					if ((sides & Border3DSide.Right)!=0) {
						graphics.DrawLine(penBottomRightInner, left, rect.Bottom-2, rect.Right-2, rect.Bottom-2);
					} else {
						graphics.DrawLine(penBottomRightInner, left, rect.Bottom-2, rect.Right-1, rect.Bottom-2);
					}
				}
			}

		}


		public void DrawButton (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonPush;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}
			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);
		}


		public void DrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state)
		{
			Rectangle	captionRect;
			int			lineWidth;

			DrawButton(graphics, rectangle, state);

			if (rectangle.Width<rectangle.Height) {
				captionRect=new Rectangle(rectangle.X+1, rectangle.Y+rectangle.Height/2-rectangle.Width/2+1, rectangle.Width-4, rectangle.Width-4);
			} else {
				captionRect=new Rectangle(rectangle.X+rectangle.Width/2-rectangle.Height/2+1, rectangle.Y+1, rectangle.Height-4, rectangle.Height-4);
			}

			if ((state & ButtonState.Pushed)!=0) {
				captionRect=new Rectangle(rectangle.X+2, rectangle.Y+2, rectangle.Width-3, rectangle.Height-3);
			}

			/* Make sure we've got at least a line width of 1 */
			lineWidth=Math.Max(1, captionRect.Width/7);

			switch(button) {
				case CaptionButton.Close: {
					Pen	pen;

					if ((state & ButtonState.Inactive)!=0) {
						pen=new Pen(ColorButtonHilight, lineWidth);
						DrawCaptionHelper(graphics, ColorButtonHilight, pen, lineWidth, 1, captionRect, button);
						pen.Dispose();

						pen=new Pen(ColorButtonShadow, lineWidth);
						DrawCaptionHelper(graphics, ColorButtonShadow, pen, lineWidth, 0, captionRect, button);
						pen.Dispose();
						return;
					} else {
						pen=new Pen(SystemColors.ControlText, lineWidth);
						DrawCaptionHelper(graphics, SystemColors.ControlText, pen, lineWidth, 0, captionRect, button);
						pen.Dispose();
						return;
					}
				}

				case CaptionButton.Help:
				case CaptionButton.Maximize:
				case CaptionButton.Minimize:
				case CaptionButton.Restore: {
					if ((state & ButtonState.Inactive)!=0) {
						DrawCaptionHelper(graphics, ColorButtonHilight, SystemPens.ControlLightLight, lineWidth, 1, captionRect, button);

						DrawCaptionHelper(graphics, ColorButtonShadow, SystemPens.ControlDark, lineWidth, 0, captionRect, button);
						return;
					} else {
						DrawCaptionHelper(graphics, SystemColors.ControlText, SystemPens.ControlText, lineWidth, 0, captionRect, button);
						return;
					}
				}
			}
		}


		public void DrawCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonCheck;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}

			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);

		}

		public void DrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			Point[]			arrow = new Point[3];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;

			if ((state & ButtonState.Checked)!=0) {
				HatchBrush	hatchBrush=new HatchBrush(HatchStyle.Percent50, SystemColors.ControlLight, ColorButtonHilight);
				graphics.FillRectangle(hatchBrush,rectangle);
				hatchBrush.Dispose();
			}

			if ((state & ButtonState.Flat)!=0) {
				ControlPaint.DrawBorder(graphics, rectangle, ColorButtonShadow, ButtonBorderStyle.Solid);
			} else {
				if ((state & (ButtonState.Pushed | ButtonState.Checked))!=0) {
					DrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
				} else {
					DrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
				}
			}

			rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
			centerX=rect.Left+rect.Width/2;
			centerY=rect.Top+rect.Height/2;
			shiftX=Math.Max(1, rect.Width/8);
			shiftY=Math.Max(1, rect.Height/8);

			if ((state & ButtonState.Pushed)!=0) {
				shiftX++;
				shiftY++;
			}

			rect.Y-=shiftY;
			centerY-=shiftY;
			P1=new Point(rect.Left, centerY);
			P2=new Point(rect.Right, centerY);
			P3=new Point(centerX, rect.Bottom);

			arrow[0]=P1;
			arrow[1]=P2;
			arrow[2]=P3;

			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				graphics.FillPolygon(SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				/* Move away from the shadow */
				P1.X-=1;		P1.Y-=1;
				P2.X-=1;		P2.Y-=1;
				P3.X-=1;		P3.Y-=1;

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;


				graphics.FillPolygon(SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);
			}
		}


		public void DrawContainerGrabHandle (Graphics graphics, Rectangle bounds)
		{
			SolidBrush	sb		= br_buttontext;
			Pen			pen	= new Pen(Color.Black, 1);
			Rectangle	rect	= new Rectangle(bounds.X, bounds.Y, bounds.Width-1, bounds.Height-1);	// Dunno why, but MS does it that way, too
			int			X;
			int			Y;

			graphics.FillRectangle(sb, rect);
			graphics.DrawRectangle(pen, rect);

			X=rect.X+rect.Width/2;
			Y=rect.Y+rect.Height/2;

			/* Draw the cross */
			graphics.DrawLine(pen, X, rect.Y+2, X, rect.Bottom-2);
			graphics.DrawLine(pen, rect.X+2, Y, rect.Right-2, Y);

			/* Draw 'arrows' for vertical lines */
			graphics.DrawLine(pen, X-1, rect.Y+3, X+1, rect.Y+3);
			graphics.DrawLine(pen, X-1, rect.Bottom-3, X+1, rect.Bottom-3);

			/* Draw 'arrows' for horizontal lines */
			graphics.DrawLine(pen, rect.X+3, Y-1, rect.X+3, Y+1);
			graphics.DrawLine(pen, rect.Right-3, Y-1, rect.Right-3, Y+1);

		}


		public void DrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor)
		{
			//Color			colorForeInverted;
			Color			colorBackInverted;
			Pen			pen;

			//colorForeInverted=Color.FromArgb(Math.Abs(foreColor.R-255), Math.Abs(foreColor.G-255), Math.Abs(foreColor.B-255));
			//pen=new Pen(colorForeInverted, 1);
			// MS seems to always use black
			pen=new Pen(Color.Black, 1);
			graphics.DrawRectangle(pen, rectangle);
			pen.Dispose();

			colorBackInverted=Color.FromArgb(Math.Abs(backColor.R-255), Math.Abs(backColor.G-255), Math.Abs(backColor.B-255));
			pen=new Pen(colorBackInverted, 1);
			pen.DashStyle=DashStyle.Dot;
			graphics.DrawRectangle(pen, rectangle);
			pen.Dispose();
		}


		public void DrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled)
		{
			SolidBrush	sb;
			Pen			pen;

			if (primary==true) {
				pen=new Pen(Color.Black, 1);
				if (enabled==true) {
					sb=br_buttontext;
				} else {
					sb=br_buttonface;
				}
			} else {
				pen=new Pen(Color.White, 1);
				if (enabled==true) {
					sb=new SolidBrush(Color.Black);
				} else {
					sb=br_buttonface;
				}
			}
			graphics.FillRectangle(sb, rectangle);
			graphics.DrawRectangle(pen, rectangle);
			sb.Dispose();
			pen.Dispose();
		}


		public void DrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor)
		{
			Color	foreColor;
			int	h;
			int	b;
			int	s;

			ControlPaint.Color2HBS(backColor, out h, out b, out s);

			if (b>127) {
				foreColor=Color.Black;
			} else {
				foreColor=Color.White;
			}

#if false
			/* Commented out until I take the time and figure out
				which HatchStyle will match requirements. The code below
				is only correct for Percent50.
			*/
			if (pixelsBetweenDots.Width==pixelsBetweenDots.Height) {
				HatchBrush	brush=null;

				switch(pixelsBetweenDots.Width) {
					case 2: brush=new HatchBrush(HatchStyle.Percent50, foreColor, backColor); break;
					case 4: brush=new HatchBrush(HatchStyle.Percent25, foreColor, backColor); break;
					case 5: brush=new HatchBrush(HatchStyle.Percent20, foreColor, backColor); break;
					default: {
						/* Have to do it the slow way */
						break;
					}
				}
				if (brush!=null) {
					graphics.FillRectangle(brush, area);
					pen.Dispose();
					brush.Dispose();
					return;
				}
			}
#endif
			/* Slow method */

			Bitmap bitmap = new Bitmap(area.Width, area.Height, graphics);

			for (int x=0; x<area.Width; x+=pixelsBetweenDots.Width) {
				for (int y=0; y<area.Height; y+=pixelsBetweenDots.Height) {
					bitmap.SetPixel(x, y, foreColor);
				}
			}
			graphics.DrawImage(bitmap, area.X, area.Y, area.Width, area.Height);
			bitmap.Dispose();
		}

		public void DrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background)
		{
			/*
				Microsoft seems to ignore the background and simply make
				the image grayscale. At least when having > 256 colors on
				the display.
			*/

			ImageAttributes	imageAttributes=new ImageAttributes();
			ColorMatrix			colorMatrix=new ColorMatrix(new float[][] {
// This table would create a perfect grayscale image, based on luminance
//				new float[]{0.3f,0.3f,0.3f,0,0},
//				new float[]{0.59f,0.59f,0.59f,0,0},
//				new float[]{0.11f,0.11f,0.11f,0,0},
//				new float[]{0,0,0,1,0,0},
//				new float[]{0,0,0,0,1,0},
//				new float[]{0,0,0,0,0,1}

// This table generates a image that is grayscaled and then
// brightened up. Seems to match MS close enough.
				new float[]{0.2f,0.2f,0.2f,0,0},
				new float[]{0.41f,0.41f,0.41f,0,0},
				new float[]{0.11f,0.11f,0.11f,0,0},
				new float[]{0.15f,0.15f,0.15f,1,0,0},
				new float[]{0.15f,0.15f,0.15f,0,1,0},
				new float[]{0.15f,0.15f,0.15f,0,0,1}
			});

			imageAttributes.SetColorMatrix(colorMatrix);
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			imageAttributes.Dispose();
		}


		public void DrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary)
		{
			Pen	penBorder;
			Pen	penInside;

			if (primary) {
				penBorder=new Pen(Color.White, 2);
				penInside=new Pen(Color.Black, 1);
			} else {
				penBorder=new Pen(Color.Black, 2);
				penInside=new Pen(Color.White, 1);
			}
			penBorder.Alignment=PenAlignment.Inset;
			penInside.Alignment=PenAlignment.Inset;

			graphics.DrawRectangle(penBorder, rectangle);
			graphics.DrawRectangle(penInside, rectangle.X+2, rectangle.Y+2, rectangle.Width-5, rectangle.Height-5);
			penBorder.Dispose();
			penInside.Dispose();
		}


		public void DrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph)
		{
			Rectangle	rect;
			int			lineWidth;

			// MS seems to draw the background white
			graphics.FillRectangle(br_buttontext, rectangle);

			switch(glyph) {
				case MenuGlyph.Arrow: {
					Point[]			arrow = new Point[3];
					Point				P1;
					Point				P2;
					Point				P3;
					int				centerX;
					int				centerY;
					int				shiftX;
					int				shiftY;

					rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
					centerX=rect.Left+rect.Width/2;
					centerY=rect.Top+rect.Height/2;
					shiftX=Math.Max(1, rect.Width/8);
					shiftY=Math.Max(1, rect.Height/8);

					rect.X-=shiftX;
					centerX-=shiftX;

					P1=new Point(centerX, rect.Top-1);
					P2=new Point(centerX, rect.Bottom);
					P3=new Point(rect.Right, centerY);

					arrow[0]=P1;
					arrow[1]=P2;
					arrow[2]=P3;

					graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);

					return;
				}

				case MenuGlyph.Bullet: {
					SolidBrush	sb;

					lineWidth=Math.Max(2, rectangle.Width/3);
					rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);

					sb=br_buttontext;
					graphics.FillEllipse(sb, rect);
					sb.Dispose();
					return;
				}

				case MenuGlyph.Checkmark: {
					int			Scale;

					lineWidth=Math.Max(2, rectangle.Width/6);
					Scale=Math.Max(1, rectangle.Width/12);

					rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);

					for (int i=0; i<lineWidth; i++) {
						graphics.DrawLine(SystemPens.MenuText, rect.Left+lineWidth/2, rect.Top+lineWidth+i, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i);
						graphics.DrawLine(SystemPens.MenuText, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i, rect.Left+lineWidth/2+6*Scale, rect.Top+lineWidth-2*Scale+i);
					}
					return;
				}
			}

		}

		public void DrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state)
		{
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonRadio;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}
			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);

		}


		public void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style)
		{

		}


		public void DrawReversibleLine (Point start, Point end, Color backColor)
		{

		}


		/* Scroll button: regular button + direction arrow */
		public void DrawScrollButton (Graphics dc, Rectangle area, ScrollButton type, ButtonState state)
		{
			bool enabled = (state == ButtonState.Inactive) ? false: true;			
					
			DrawScrollButtonPrimitive (dc, area, state);

			/* Paint arrows */
			switch (type) {
			case ScrollButton.Up:
			{
				int x = area.X +  (area.Width / 2) - 4;
				int y = area.Y + 9;

				for (int i = 0; i < 3; i++)
					if (enabled)
						dc.DrawLine (pen_arrow, x + i, y - i, x + i + 6 - 2*i, y - i);
					else
						dc.DrawLine (pen_disabled, x + i, y - i, x + i + 6 - 2*i, y - i);

				if (enabled)
					dc.FillRectangle (br_arrow, x + 3, area.Y + 6, 1, 1);
				else
					dc.FillRectangle (br_disabled, x + 3, area.Y + 6, 1, 1);

				break;
			}
			case ScrollButton.Down:
			{
				int x = area.X +  (area.Width / 2) - 4;
				int y = area.Y + 5;

				for (int i = 4; i != 0; i--)
					if (enabled)
						dc.DrawLine (pen_arrow, x + i, y + i, x + i + 8 - 2*i, y + i);
					else
						dc.DrawLine (pen_disabled, x + i, y + i, x + i + 8 - 2*i, y + i);

				if (enabled)
					dc.FillRectangle (br_arrow, x + 4, y + 4, 1, 1);
				else
					dc.FillRectangle (br_disabled, x + 4, y + 4, 1, 1);

				break;
			}

			case ScrollButton.Left:
			{
				int y = area.Y +  (area.Height / 2) - 4;
				int x = area.X + 9;

				for (int i = 0; i < 3; i++)
					if (enabled)
						dc.DrawLine (pen_arrow, x - i, y + i, x - i, y + i + 6 - 2*i);
					else
						dc.DrawLine (pen_disabled, x - i, y + i, x - i, y + i + 6 - 2*i);

				if (enabled)
					dc.FillRectangle (br_arrow, x - 3, y + 3, 1, 1);
				else
					dc.FillRectangle (br_disabled, x - 3, y + 3, 1, 1);

				break;
			}

			case ScrollButton.Right:
			{
				int y = area.Y +  (area.Height / 2) - 4;
				int x = area.X + 5;

				for (int i = 4; i != 0; i--)
					if (enabled)
						dc.DrawLine (pen_arrow, x + i, y + i, x + i, y + i + 8 - 2*i);
					else
						dc.DrawLine (pen_disabled, x + i, y + i, x + i, y + i + 8 - 2*i);

				if (enabled)
					dc.FillRectangle (br_arrow, x + 4, y + 4, 1, 1);
				else
					dc.FillRectangle (br_disabled, x + 3, y + 3, 1, 1);

				break;
			}

			default:
				break;

			}
		}


		public void DrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor)
		{

		}


		public void DrawSizeGrip (Graphics dc, Color backColor, Rectangle bounds)
		{
			Point pt = new Point (bounds.Right - 2, bounds.Bottom - 1);

			dc.DrawLine (pen_buttonface, pt.X - 12, pt.Y, pt.X, pt.Y);
			dc.DrawLine (pen_buttonface, pt.X, pt.Y, pt.X, pt.Y - 13);

			// diagonals
			for (int i = 0; i < 11; i += 4) {
				dc.DrawLine (pen_buttonshadow, pt.X - i, pt.Y, pt.X + 1, pt.Y - i - 2);
				dc.DrawLine (pen_buttonshadow, pt.X - i - 1, pt.Y, pt.X + 1, pt.Y - i - 2);
			}

			for (int i = 3; i < 13; i += 4)
				dc.DrawLine (pen_buttonhilight, pt.X - i, pt.Y, pt.X + 1, pt.Y - i - 1);
		}


		public void DrawStringDisabled (Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle,
			StringFormat format)
		{
			SolidBrush	brush;

			brush=new SolidBrush(ControlPaint.Light(color, 25));

			layoutRectangle.Offset(1.0f, 1.0f);
			graphics.DrawString(s, font, brush, layoutRectangle, format);

			brush.Color=ControlPaint.Dark(color, 35);
			layoutRectangle.Offset(-1.0f, -1.0f);
			graphics.DrawString(s, font, brush, layoutRectangle, format);

			brush.Dispose();
		}

		/*
			Methods that draw complex controls
		*/

		public void DrawScrollBar (Graphics dc, Rectangle area, Rectangle thumb_pos,
			ref Rectangle first_arrow_area, ref Rectangle second_arrow_area,
			ButtonState first_arrow, ButtonState second_arrow,
			ref int scrollbutton_width, ref int scrollbutton_height,
			bool enabled, bool vertical)
		{

			if (br_scrollbar_backgr == null)
				br_scrollbar_backgr = new HatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace);

			if (vertical) {		

				first_arrow_area.X = first_arrow_area. Y = 0;
				first_arrow_area.Width = scrollbutton_width;
				first_arrow_area.Height = scrollbutton_height;

				second_arrow_area.X = 0;
				second_arrow_area.Y = area.Height - scrollbutton_height;
				second_arrow_area.Width = scrollbutton_width;
				second_arrow_area.Height = scrollbutton_height;

				/* Buttons */
				DrawScrollButton (dc, first_arrow_area, ScrollButton.Up, first_arrow);
				DrawScrollButton (dc, second_arrow_area, ScrollButton.Down, second_arrow);				

				/* Background */
				dc.FillRectangle (br_scrollbar_backgr, 0,  scrollbutton_height, area.Width,
					area.Height - (scrollbutton_height * 2));
			}
			else {
				
				first_arrow_area.X = first_arrow_area. Y = 0;
				first_arrow_area.Width = scrollbutton_width;
				first_arrow_area.Height = scrollbutton_height;

				second_arrow_area.Y = 0;
				second_arrow_area.X = area.Width - scrollbutton_width;
				second_arrow_area.Width = scrollbutton_width;
				second_arrow_area.Height = scrollbutton_height;

				/* Buttons */
				DrawScrollButton (dc, first_arrow_area, ScrollButton.Left, first_arrow );
				DrawScrollButton (dc, second_arrow_area, ScrollButton.Right, second_arrow);

				/* Background */
				dc.FillRectangle (br_scrollbar_backgr, scrollbutton_width, 0, area.Width - (scrollbutton_width * 2),
				 	area.Height);
			}

			/* Thumbail */
			if (enabled)
				DrawScrollButtonPrimitive (dc, thumb_pos, ButtonState.Normal);			
		}

		/*
			DrawTrackBar
		*/

		/* Vertical trackbar */
		private void DrawTrackBar_Vertical (Graphics dc, Rectangle area, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area,  Brush br_thumb,
			float ticks, int value_pos, bool mouse_value)		
		{			
			
			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			const int space_from_right = 8;
			const int space_from_left = 8;			
			
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

			/* Draw channel */
			dc.FillRectangle (br_buttonshadow, channel_startpoint.X, channel_startpoint.Y,
				1, thumb_area.Height);
			
			dc.FillRectangle (br_buttondkshadow, channel_startpoint.X + 1, channel_startpoint.Y,
				1, thumb_area.Height);

			dc.FillRectangle (br_buttonhilight, channel_startpoint.X + 3, channel_startpoint.Y,
				1, thumb_area.Height);

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
			
			thumb_pos.Y = channel_startpoint.Y + (int) (pixels_betweenticks * (float) value_pos);
			
			/* Draw thumb fixed 10x22 size */
			thumb_pos.Width = 10;
			thumb_pos.Height = 22;

			switch (tb.TickStyle) 	{
			case TickStyle.BottomRight:
			case TickStyle.None:
			{
				thumb_pos.X = channel_startpoint.X - 8;

				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y, thumb_pos.X , thumb_pos.Y + 10);
				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 16, thumb_pos.Y);
				dc.DrawLine (pen_buttonhilight, thumb_pos.X + 16, thumb_pos.Y, thumb_pos.X + 16 + 4, thumb_pos.Y + 4);
				
				dc.DrawLine (pen_buttonshadow, thumb_pos.X +1, thumb_pos.Y + 9, thumb_pos.X +15, thumb_pos.Y  +9);
				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 16, thumb_pos.Y + 9, thumb_pos.X +16 + 4, thumb_pos.Y  +9 - 4);

				dc.DrawLine (pen_buttondkshadow, thumb_pos.X, thumb_pos.Y  + 10, thumb_pos.X +16, thumb_pos.Y +10);
				dc.DrawLine (pen_buttondkshadow, thumb_pos.X + 16, thumb_pos.Y  + 10, thumb_pos.X  +16 + 5, thumb_pos.Y +10 - 5);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 16, 8);
				dc.FillRectangle (br_thumb, thumb_pos.X + 17, thumb_pos.Y + 2, 1, 6);
				dc.FillRectangle (br_thumb, thumb_pos.X + 18, thumb_pos.Y + 3, 1, 4);
				dc.FillRectangle (br_thumb, thumb_pos.X + 19, thumb_pos.Y + 4, 1, 2);

				break;
			}
			case TickStyle.TopLeft:
			{
				thumb_pos.X = channel_startpoint.X - 10;

				dc.DrawLine (pen_buttonhilight, thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X + 4 + 16, thumb_pos.Y);
				dc.DrawLine (pen_buttonhilight, thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 4);

				dc.DrawLine (pen_buttonshadow, thumb_pos.X  + 4, thumb_pos.Y + 9, thumb_pos.X + 4 + 16 , thumb_pos.Y+ 9);
				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 4, thumb_pos.Y  + 9, thumb_pos.X, thumb_pos.Y + 5);
				dc.DrawLine (pen_buttonshadow, thumb_pos.X  + 19, thumb_pos.Y + 9, thumb_pos.X  +19 , thumb_pos.Y+ 1);

				dc.DrawLine (pen_buttondkshadow, thumb_pos.X  + 4, thumb_pos.Y+ 10, thumb_pos.X  + 4 + 16, thumb_pos.Y+ 10);
				dc.DrawLine (pen_buttondkshadow, thumb_pos.X  + 4, thumb_pos.Y + 10, thumb_pos.X  -1, thumb_pos.Y+ 5);
				dc.DrawLine (pen_buttondkshadow, thumb_pos.X + 20, thumb_pos.Y, thumb_pos.X+ 20, thumb_pos.Y + 10);

				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 15, 8);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 1, 6);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 1, 4);
				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 1, 2);

				break;
			}

			case TickStyle.Both:
			{
				thumb_pos.X = area.X + 10;
				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 9);
				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 19, thumb_pos.Y);

				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 1, thumb_pos.Y + 9, thumb_pos.X+ 19, thumb_pos.Y  + 9);
				dc.DrawLine (pen_buttonshadow, thumb_pos.X  + 10, thumb_pos.Y+ 1, thumb_pos.X + 19, thumb_pos.Y  + 8);

				dc.DrawLine (pen_buttondkshadow, thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X+ 20, thumb_pos.Y  +10);
				dc.DrawLine (pen_buttondkshadow, thumb_pos.X  + 20, thumb_pos.Y, thumb_pos.X  + 20, thumb_pos.Y+ 9);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 18, 8);

				break;
			}

			default:
				break;
			}

			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / ticks;				
			
			/* Draw ticks*/
			if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight ||
				((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {	
				
				for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) 	{					
					if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len +1)
						dc.DrawLine (pen_ticks, area.X + bottomtick_startpoint.X , area.Y + bottomtick_startpoint.Y  + inc, 
							area.X + bottomtick_startpoint.X  + 3, area.Y + bottomtick_startpoint.Y + inc);
					else
						dc.DrawLine (pen_ticks, area.X + bottomtick_startpoint.X, area.Y + bottomtick_startpoint.Y  + inc, 
							area.X + bottomtick_startpoint.X  + 2, area.Y + bottomtick_startpoint.Y + inc);
				}
			}

			if (pixels_betweenticks > 0 &&  ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft ||
				((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {

				pixel_len = thumb_area.Height - 11;
				pixels_betweenticks = pixel_len / ticks;
				
				for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) 
				{
					//Console.WriteLine ("{0} {1} {2}", pixel_len, inc, pixels_betweenticks );
					if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len +1)
						dc.DrawLine (pen_ticks, area.X + toptick_startpoint.X  - 3 , area.Y + toptick_startpoint.Y + inc, 
							area.X + toptick_startpoint.X, area.Y + toptick_startpoint.Y + inc);
					else
						dc.DrawLine (pen_ticks, area.X + toptick_startpoint.X  - 2, area.Y + toptick_startpoint.Y + inc, 
							area.X + toptick_startpoint.X, area.Y + toptick_startpoint.Y  + inc);
				}			
			}
		}

		/* 
			Horizontal trackbar 
		  
		 	Does not matter the size of the control, Win32 always draws:
		 		- Ticks starting from pixel 13, 8
		 		- Channel starting at pos 8, 19 and ends at Width - 8
		 		- Autosize makes always the control 40 pixels height
		 		- Ticks are draw at (channel.Witdh - 10) / (Maximum - Minimum)
				
		*/
		private void DrawTrackBar_Horizontal (Graphics dc, Rectangle area, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area, Brush br_thumb,
			float ticks, int value_pos, bool mouse_value)
		{			
			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			const int space_from_right = 8;
			const int space_from_left = 8;		
						
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
			
			/* Draw channel */
			dc.FillRectangle (br_buttonshadow, channel_startpoint.X, channel_startpoint.Y,
				thumb_area.Width, 1);
			
			dc.FillRectangle (br_buttondkshadow, channel_startpoint.X, channel_startpoint.Y + 1,
				thumb_area.Width, 1);

			dc.FillRectangle (br_buttonhilight, channel_startpoint.X, channel_startpoint.Y +3,
				thumb_area.Width, 1);

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
			
			thumb_pos.X = channel_startpoint.X + (int) (pixels_betweenticks * (float) value_pos);
			
			/* Draw thumb fixed 10x22 size */
			thumb_pos.Width = 10;
			thumb_pos.Height = 22;

			switch (tb.TickStyle) {
			case TickStyle.BottomRight:
			case TickStyle.None: 
			{
				thumb_pos.Y = channel_startpoint.Y - 8;

				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y);
				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 16);
				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y + 16, thumb_pos.X + 4, thumb_pos.Y + 16 + 4);

				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X +9, thumb_pos.Y +15);
				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 9, thumb_pos.Y + 16, thumb_pos.X +9 - 4, thumb_pos.Y +16 + 4);

				dc.DrawLine (pen_buttondkshadow, thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X +10, thumb_pos.Y +16);
				dc.DrawLine (pen_buttondkshadow, thumb_pos.X + 10, thumb_pos.Y + 16, thumb_pos.X +10 - 5, thumb_pos.Y +16 + 5);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 16);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 17, 6, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 18, 4, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 19, 2, 1);
				break;
			}
			case TickStyle.TopLeft:	{
				thumb_pos.Y = channel_startpoint.Y - 10;

				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X, thumb_pos.Y + 4 + 16);
				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X + 4, thumb_pos.Y);

				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 9, thumb_pos.Y + 4 + 16);
				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y);
				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 9, thumb_pos.Y + 19, thumb_pos.X + 1 , thumb_pos.Y +19);

				dc.DrawLine (pen_buttondkshadow, thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 10, thumb_pos.Y + 4 + 16);
				dc.DrawLine (pen_buttondkshadow, thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y -1);
				dc.DrawLine (pen_buttondkshadow, thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 10, thumb_pos.Y + 20);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 8, 15);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 6, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 4, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 2, 1);
				break;
			}

			case TickStyle.Both: {
				thumb_pos.Y = area.Y + 10;
				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y, thumb_pos.X + 9, thumb_pos.Y);
				dc.DrawLine (pen_buttonhilight, thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 19);

				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X + 9, thumb_pos.Y + 19);
				dc.DrawLine (pen_buttonshadow, thumb_pos.X + 1, thumb_pos.Y + 10, thumb_pos.X + 8, thumb_pos.Y + 19);

				dc.DrawLine (pen_buttondkshadow, thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X +10, thumb_pos.Y + 20);
				dc.DrawLine (pen_buttondkshadow, thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 9, thumb_pos.Y + 20);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 18);

				break;
			}

			default:
				break;
			}

			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / ticks;

			/* Draw ticks*/
			if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight ||
				((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {				
				
				for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
					if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len +1)
						dc.DrawLine (pen_ticks, area.X + bottomtick_startpoint.X + inc , area.Y + bottomtick_startpoint.Y, 
							area.X + bottomtick_startpoint.X + inc , area.Y + bottomtick_startpoint.Y + 3);
					else
						dc.DrawLine (pen_ticks, area.X + bottomtick_startpoint.X + inc, area.Y + bottomtick_startpoint.Y, 
							area.X + bottomtick_startpoint.X + inc, area.Y + bottomtick_startpoint.Y + 2);
				}
			}

			if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft ||
				((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {
				
				for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
					if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len +1)
						dc.DrawLine (pen_ticks, area.X + toptick_startpoint.X + inc , area.Y + toptick_startpoint.Y - 3, 
							area.X + toptick_startpoint.X + inc , area.Y + toptick_startpoint.Y);
					else
						dc.DrawLine (pen_ticks, area.X + toptick_startpoint.X + inc, area.Y + toptick_startpoint.Y - 2, 
							area.X + toptick_startpoint.X + inc, area.Y + toptick_startpoint.Y );
					}			
			}
		}

		public void DrawTrackBar (Graphics dc, Rectangle area, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area,  bool highli_thumb,
			float ticks, int value_pos, bool mouse_value)

		{
			Brush br_thumb;			

			if (highli_thumb == true) {
				if (br_trackbar_thumbhili == null)
					br_trackbar_thumbhili = new HatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace);

				br_thumb = (Brush) br_trackbar_thumbhili;
			}
			else
				br_thumb = br_buttonface;

			
			/* Control Background */
			if (tb.BackColor == DefaultControlBackColor)
				dc.FillRectangle (br_buttonface, area);
			else 	{
				if (br_trackbarbg == null || br_trackbarbg.Color != tb.BackColor)
					br_trackbarbg = new SolidBrush (tb.BackColor);

				dc.FillRectangle (br_trackbarbg, area);
			}

			if (tb.Focused) {
				dc.FillRectangle (br_focus, area.X, area.Y, area.Width - 1, 1);
				dc.FillRectangle (br_focus, area.X, area.Y + area.Height - 1, area.Width - 1, 1);
				dc.FillRectangle (br_focus, area.X, area.Y, 1, area.Height - 1);
				dc.FillRectangle (br_focus, area.X + area.Width - 1, area.Y, 1, area.Height - 1);
			}

			if (tb.Orientation == Orientation.Vertical) 
				DrawTrackBar_Vertical (dc, area, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
			
			else
				DrawTrackBar_Horizontal (dc, area, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
		}

		public void DrawProgressBar (Graphics dc, Rectangle area,  Rectangle client_area,
			int barpos_pixels, int block_width)

		{
			int space_betweenblocks = 2;
			int increment = block_width + space_betweenblocks;
			int x = client_area.X;

			/* Background*/
			dc.FillRectangle (br_buttonface, area);

			/* Draw background*/

			while ((x - client_area.X) < barpos_pixels) {
				dc.FillRectangle (br_progressbarblock, x, client_area.Y, block_width, client_area.Height);
				x  = x + increment;
			}

			/* Draw border */
			DrawBorder3D (dc, area, Border3DStyle.SunkenInner, Border3DSide.All);
			
		}
		

		public void DrawLabel (Graphics dc, Rectangle area, BorderStyle border_style, string text, 
			Color fore_color, Color back_color, Font font, StringFormat string_format, bool Enabled)

		{
			if (label_br_fore_color == null || label_br_fore_color.Color != fore_color) 
				label_br_fore_color = GetControlForeBrush (fore_color);

			if (label_br_back_color == null || label_br_back_color.Color != back_color) 
				label_br_back_color = GetControlBackBrush (back_color);

			dc.FillRectangle (label_br_back_color, area);
			
			DrawBorderStyle (dc, area, border_style);		

			if (Enabled)
				dc.DrawString (text, font, label_br_fore_color, area, string_format);
			else
				ControlPaint.DrawStringDisabled (dc, text, font, fore_color, area, string_format);
		
		}

		public void DrawStatusBar (Graphics dc, Rectangle area, StatusBar sb)
		{
			int horz_border = 2;
			int vert_border = 2;

			dc.FillRectangle (GetControlBackBrush (sb.BackColor), area);
			
			if (sb.ShowPanels && sb.Panels.Count == 0) {
				// Create a default panel.
				SolidBrush br_forecolor = GetControlForeBrush (sb.ForeColor);
				
				StatusBarPanel panel = new StatusBarPanel ();
				Rectangle new_area = new Rectangle (area.X + horz_border,
						area.Y + horz_border,
						area.Width - SizeGripWidth - horz_border,
						area.Height - horz_border);
				DrawStatusBarPanel (dc, new_area, -1, br_forecolor, panel);
			} else if (sb.ShowPanels) {
				SolidBrush br_forecolor = GetControlForeBrush (sb.ForeColor);
				int prev_x = area.X + horz_border;
				int y = area.Y + vert_border;
				for (int i = 0; i < sb.Panels.Count; i++) {
					Rectangle pr = new Rectangle (prev_x, y,
							sb.Panels [i].Width, area.Height);
					prev_x += pr.Width + StatusBarHorzGapWidth;
					DrawStatusBarPanel (dc, pr, i, br_forecolor, sb.Panels [i]);
				}
			}

			if (sb.SizingGrip)
				DrawSizeGrip (dc, ColorButtonFace, area);

		}


		public void DrawStatusBarPanel (Graphics dc, Rectangle area, int index,
				SolidBrush br_forecolor, StatusBarPanel panel)
		{
			int border_size = 3; // this is actually const, even if the border style is none

			area.Height -= border_size;
			if (panel.BorderStyle != StatusBarPanelBorderStyle.None) {
				Border3DStyle border_style = Border3DStyle.SunkenInner;
				if (panel.BorderStyle == StatusBarPanelBorderStyle.Raised)
					border_style = Border3DStyle.RaisedOuter;
				DrawBorder3D(dc, area, border_style, Border3DSide.All);
			}

			if (panel.Style == StatusBarPanelStyle.OwnerDraw) {
                                StatusBarDrawItemEventArgs e = new StatusBarDrawItemEventArgs (
                                        dc, panel.Parent.Font, area, index, DrawItemState.Default,
                                        panel, panel.Parent.ForeColor, panel.Parent.BackColor);
                                panel.Parent.OnDrawItemInternal (e);
                                return;
                        }

			int left = area.Left;
			if (panel.Icon != null) {
				left += 2;
				int size = area.Height - border_size;
				Rectangle ia = new Rectangle (left, border_size, size, size);
				dc.DrawIcon (panel.Icon, left, area.Top);
				left += panel.Icon.Width;
			}

			if (panel.Text == String.Empty)
				return;

			string text = panel.Text;
			StringFormat string_format = new StringFormat ();
			string_format.LineAlignment = StringAlignment.Center;
			string_format.Alignment = StringAlignment.Near;
			string_format.FormatFlags = StringFormatFlags.NoWrap;

			if (text [0] == '\t') {
				string_format.Alignment = StringAlignment.Center;
				text = text.Substring (1);
				if (text [0] == '\t') {
					string_format.Alignment = StringAlignment.Far;
					text = text.Substring (1);
				}
			}

			float x = left + border_size;
			float y = ((area.Bottom - area.Top) / 2.0F) + border_size;

			dc.DrawString (text, panel.Parent.Font, br_forecolor, x, y, string_format);
		}

		public void DrawOwnerDrawBackground (DrawItemEventArgs e)
		{
			if (e.State == DrawItemState.Selected) {
				e.Graphics.FillRectangle (SystemBrushes.Highlight, e.Bounds);
				return;
			}

			e.Graphics.FillRectangle (GetControlBackBrush (e.BackColor), e.Bounds);
		}

		public void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e)
		{
			if (e.State == DrawItemState.Focus)
				DrawFocusRectangle (e.Graphics, e.Bounds, e.ForeColor, e.BackColor);
		}

		public void DrawToolBar (Graphics dc, ToolBar control, StringFormat format)
		{
			Rectangle paint_area = control.ClientRectangle;
			dc.FillRectangle (br_buttonface, paint_area);
			DrawBorderStyle (dc, paint_area, control.BorderStyle);
			bool flat = (control.Appearance == ToolBarAppearance.Flat);

			foreach (ToolBarButton button in control.Buttons) {

				Image image = null;
				Rectangle buttonArea = button.Rectangle;
				Rectangle imgRect = Rectangle.Empty;  // rect to draw the image
				Rectangle txtRect = buttonArea;       // rect to draw the text
				Rectangle ddRect = Rectangle.Empty;   // rect for the drop down arrow

				// calculate different rects and draw the frame if its not separator button
				if (button.Style != ToolBarButtonStyle.Separator) {
					/* Adjustment for drop down arrow */
					if (button.Style == ToolBarButtonStyle.DropDownButton && control.DropDownArrows) {
						ddRect.X = buttonArea.X + buttonArea.Width - this.ToolBarDropDownWidth;
						ddRect.Y = buttonArea.Y;
						ddRect.Width = this.ToolBarDropDownWidth;
						ddRect.Height = buttonArea.Height;
					}

					// calculate txtRect and imgRect, if imageIndex and imageList are present
					if (button.ImageIndex > -1 && control.ImageList != null) {
						if (button.ImageIndex < control.ImageList.Images.Count)
							image = control.ImageList.Images [button.ImageIndex];
						// draw the image at the centre if textalignment is underneath
						if (control.TextAlign == ToolBarTextAlign.Underneath) {
							imgRect.X = buttonArea.X + ((buttonArea.Width - ddRect.Width 
										     - control.ImageSize.Width) / 2) 
								                 + this.ToolBarImageGripWidth;
							imgRect.Y = buttonArea.Y + this.ToolBarImageGripWidth;
							imgRect.Width = control.ImageSize.Width;
							imgRect.Height = control.ImageSize.Height;

							txtRect.X = buttonArea.X;
							txtRect.Y = buttonArea.Y + imgRect.Height + 2 * this.ToolBarImageGripWidth;
							txtRect.Width = buttonArea.Width - ddRect.Width;
							txtRect.Height = buttonArea.Height - imgRect.Height 
								                           - 2 * this.ToolBarImageGripWidth;
						}
						else {
							imgRect.X = buttonArea.X + this.ToolBarImageGripWidth;
							imgRect.Y = buttonArea.Y + this.ToolBarImageGripWidth;
							imgRect.Width = control.ImageSize.Width;
							imgRect.Height = control.ImageSize.Height;

							txtRect.X = buttonArea.X + imgRect.Width + 2 * this.ToolBarImageGripWidth;
							txtRect.Y = buttonArea.Y;
							txtRect.Width = buttonArea.Width - imgRect.Width 
								                         - 2 * this.ToolBarImageGripWidth - ddRect.Width;
							txtRect.Height = buttonArea.Height;
						}
					}
					/* Draw the button frame, only if it is not a separator */
					if (flat) { 
						if (button.Pushed)
							ControlPaint.DrawBorder3D (dc, buttonArea, Border3DStyle.SunkenOuter,
										   Border3DSide.All);
						else if (button.Hilight) {
							dc.DrawRectangle (pen_buttonhilight, buttonArea);
							if (! ddRect.IsEmpty) {
								dc.DrawLine (pen_buttonhilight, ddRect.X, ddRect.Y, ddRect.X, 
									     ddRect.Y + ddRect.Height);
								buttonArea.Width -= this.ToolBarDropDownWidth;
							}
						}
					}
					else { // normal toolbar
						if (button.Pushed) {
							ControlPaint.DrawBorder3D (dc, buttonArea, Border3DStyle.SunkenInner,
										   Border3DSide.All);
							if (! ddRect.IsEmpty) {
								ControlPaint.DrawBorder3D (dc, ddRect, Border3DStyle.SunkenInner,
											   Border3DSide.Left);
								buttonArea.Width -= this.ToolBarDropDownWidth;
							}
						}
						else {
							ControlPaint.DrawBorder3D (dc, buttonArea, Border3DStyle.RaisedInner,
										   Border3DSide.All);
							if (! ddRect.IsEmpty) {
								ControlPaint.DrawBorder3D (dc, ddRect, Border3DStyle.RaisedInner,
											   Border3DSide.Left);
								buttonArea.Width -= this.ToolBarDropDownWidth;
							}
						}
					}
				}
				DrawToolBarButton (dc, button, control.Font, format, paint_area, buttonArea,
						   imgRect, image, txtRect, ddRect, flat);
			}
		}

		/*
		 * Private methods
		 */

		private void DrawToolBarButton (Graphics dc, ToolBarButton button, Font font, StringFormat format,
						Rectangle controlArea, Rectangle buttonArea, Rectangle imgRect, 
						Image image, Rectangle txtRect, Rectangle ddRect, bool flat)
		{
			if (! button.Visible)
				return;

			switch (button.Style) {

			case ToolBarButtonStyle.Separator:
				// separator is drawn only in the case of flat appearance
				if (flat) {
					dc.DrawLine (pen_buttonshadow, buttonArea.X + 1, buttonArea.Y, 
						     buttonArea.X + 1, buttonArea.Height);
					dc.DrawLine (pen_buttonhilight, buttonArea.X + 1 + (int) pen_buttonface.Width,
						     buttonArea.Y, buttonArea.X + 1 + (int) pen_buttonface.Width, buttonArea.Height);
					/* draw a horizontal separator */
					if (button.Wrapper) {
						int y = buttonArea.Height + this.ToolBarSeparatorWidth / 2;
						dc.DrawLine (pen_buttonshadow, 0, y, controlArea.Width, y);
						dc.DrawLine (pen_buttonhilight, 0, y + 1 + (int) pen_buttonface.Width, controlArea.Width,
							     y + 1 + (int) pen_buttonface.Width);
					}
				}
				break;

			case ToolBarButtonStyle.ToggleButton:
				Rectangle toggleArea = Rectangle.Empty;
				toggleArea.X = buttonArea.X + this.ToolBarImageGripWidth;
				toggleArea.Y = buttonArea.Y + this.ToolBarImageGripWidth;
				toggleArea.Width = buttonArea.Width - 2 * this.ToolBarImageGripWidth;
				toggleArea.Height = buttonArea.Height - 2 * this.ToolBarImageGripWidth;
				if (button.PartialPush && button.Pushed) {
					dc.FillRectangle (SystemBrushes.ControlLightLight, toggleArea);
					if (! imgRect.IsEmpty) {
						if (button.Enabled && image != null)
							button.Parent.ImageList.Draw (dc, imgRect.X, imgRect.Y, imgRect.Width, 
										      imgRect.Height, button.ImageIndex);
						else {
							dc.FillRectangle (new SolidBrush (ColorGrayText), imgRect);
							ControlPaint.DrawBorder3D (dc, imgRect, Border3DStyle.SunkenOuter,
										   Border3DSide.Right | Border3DSide.Bottom);
						}
					}
					if (button.Enabled)
						dc.DrawString (button.Text, font, SystemBrushes.ControlText, txtRect, format);
					else
						ControlPaint.DrawStringDisabled (dc, button.Text, font, SystemColors.ControlLightLight,
										 txtRect, format);
				}

				else if (button.PartialPush) {
					dc.FillRectangle (SystemBrushes.ControlLight, toggleArea);
					if (! imgRect.IsEmpty) {
						if (button.Enabled && image != null)
							button.Parent.ImageList.Draw (dc, imgRect.X, imgRect.Y, imgRect.Width,
										      imgRect.Height, button.ImageIndex);
						else {
							dc.FillRectangle (new SolidBrush (ColorGrayText), imgRect);
							ControlPaint.DrawBorder3D (dc, imgRect, Border3DStyle.SunkenOuter,
										   Border3DSide.Right | Border3DSide.Bottom);
						}
					}
					if (button.Enabled)
						dc.DrawString (button.Text, font, SystemBrushes.ControlText, txtRect, format);
					else
						ControlPaint.DrawStringDisabled (dc, button.Text, font, SystemColors.ControlLightLight,
										 txtRect, format);
				}

				else if (button.Pushed) {
					dc.FillRectangle (SystemBrushes.ControlLightLight, toggleArea);
					if (! imgRect.IsEmpty) {
						if (button.Enabled && image != null)
							button.Parent.ImageList.Draw (dc, imgRect.X, imgRect.Y, imgRect.Width,
										      imgRect.Height, button.ImageIndex);
						else {
							dc.FillRectangle (new SolidBrush (ColorGrayText), imgRect);
							ControlPaint.DrawBorder3D (dc, imgRect, Border3DStyle.SunkenOuter,
										   Border3DSide.Right | Border3DSide.Bottom);
						}
					}
					if (button.Enabled)
						dc.DrawString (button.Text, font, SystemBrushes.ControlText, txtRect, format);
					else
						ControlPaint.DrawStringDisabled (dc, button.Text, font, SystemColors.ControlLightLight,
										 txtRect, format);
				}

				else {
					if (! imgRect.IsEmpty) {
						if (button.Enabled && image != null)
							button.Parent.ImageList.Draw (dc, imgRect.X, imgRect.Y, imgRect.Width,
										      imgRect.Height, button.ImageIndex);
						else {
							dc.FillRectangle (new SolidBrush (ColorGrayText), imgRect);
							ControlPaint.DrawBorder3D (dc, imgRect, Border3DStyle.SunkenOuter,
										   Border3DSide.Right | Border3DSide.Bottom);
						}
					}
					if (button.Enabled)
						dc.DrawString (button.Text, font, SystemBrushes.ControlText, txtRect, format);
					else
						ControlPaint.DrawStringDisabled (dc, button.Text, font, SystemColors.ControlLightLight,
										 txtRect, format);
				}
				break;

			case ToolBarButtonStyle.DropDownButton:
				// draw the dropdown arrow
				if (! ddRect.IsEmpty) {
					PointF [] vertices = new PointF [3];
					PointF ddCenter = new PointF (ddRect.X + (ddRect.Width/2.0f), ddRect.Y + (ddRect.Height/2.0f));
					vertices [0].X = ddCenter.X - this.ToolBarDropDownArrowWidth / 2.0f + 0.5f;
					vertices [0].Y = ddCenter.Y;
					vertices [1].X = ddCenter.X + this.ToolBarDropDownArrowWidth / 2.0f + 0.5f;
					vertices [1].Y = ddCenter.Y;
					vertices [2].X = ddCenter.X + 0.5f; // 0.5 is added for adjustment
					vertices [2].Y = ddCenter.Y + this.ToolBarDropDownArrowHeight;
					dc.FillPolygon (SystemBrushes.ControlText, vertices);
				}
				goto case ToolBarButtonStyle.PushButton;

			case ToolBarButtonStyle.PushButton:
				if (! imgRect.IsEmpty){
					if (button.Enabled && image != null)
						button.Parent.ImageList.Draw (dc, imgRect.X, imgRect.Y, imgRect.Width, imgRect.Height,
									      button.ImageIndex);
					else {
						dc.FillRectangle (new SolidBrush (ColorGrayText), imgRect);
						ControlPaint.DrawBorder3D (dc, imgRect, Border3DStyle.SunkenOuter,
									   Border3DSide.Right | Border3DSide.Bottom);
					}
				}
				if (button.Enabled)
					dc.DrawString (button.Text, font, SystemBrushes.ControlText, txtRect, format);
				else
					ControlPaint.DrawStringDisabled (dc, button.Text, font, SystemColors.ControlLightLight,
									 txtRect, format);
				break;
			}
		}

		private static void DrawBorderInternal(Graphics graphics, int startX, int startY, int endX, int endY,
			int width, Color color, ButtonBorderStyle style, Border3DSide side) {

			Pen	pen=new Pen(color, 1);

			switch(style) {
				case ButtonBorderStyle.Solid: {
					pen.DashStyle=DashStyle.Solid;
					break;
				}

				case ButtonBorderStyle.Dashed: {
					pen.DashStyle=DashStyle.Dash;
					break;
				}

				case ButtonBorderStyle.Dotted: {
					pen.DashStyle=DashStyle.Dot;
					break;
				}

				case ButtonBorderStyle.Inset: {
					pen.DashStyle=DashStyle.Solid;
					break;
				}

				case ButtonBorderStyle.Outset: {
					pen.DashStyle=DashStyle.Solid;
					break;
				}

				default:
				case ButtonBorderStyle.None: {
					pen.Dispose();
					return;
				}
			}


			switch(style) {
				case ButtonBorderStyle.Outset: {
					Color		colorGrade;
					int		hue, brightness, saturation;
					int		brightnessSteps;
					int		brightnessDownSteps;

					ControlPaint.Color2HBS(color, out hue, out brightness, out saturation);

					brightnessDownSteps=brightness/width;
					if (brightness>127) {
						brightnessSteps=Math.Max(6, (160-brightness)/width);
					} else {
						brightnessSteps=(127-brightness)/width;
					}

					for (int i=0; i<width; i++) {
						switch(side) {
							case Border3DSide.Left:	{
								pen.Dispose();
								colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
								break;
							}

							case Border3DSide.Right: {
								pen.Dispose();
								colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
								break;
							}

							case Border3DSide.Top: {
								pen.Dispose();
								colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
								break;
							}

							case Border3DSide.Bottom: {
								pen.Dispose();
								colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
								break;
							}
						}
					}
					break;
				}

				case ButtonBorderStyle.Inset: {
					Color		colorGrade;
					int		hue, brightness, saturation;
					int		brightnessSteps;
					int		brightnessDownSteps;

					ControlPaint.Color2HBS(color, out hue, out brightness, out saturation);

					brightnessDownSteps=brightness/width;
					if (brightness>127) {
						brightnessSteps=Math.Max(6, (160-brightness)/width);
					} else {
						brightnessSteps=(127-brightness)/width;
					}

					for (int i=0; i<width; i++) {
						switch(side) {
							case Border3DSide.Left:	{
								pen.Dispose();
								colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
								break;
							}

							case Border3DSide.Right: {
								pen.Dispose();
								colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
								break;
							}

							case Border3DSide.Top: {
								pen.Dispose();
								colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
								break;
							}

							case Border3DSide.Bottom: {
								pen.Dispose();
								colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
								break;
							}
						}
					}
					break;
				}

				/*
					I decided to have the for-loop duplicated for speed reasons;
					that way we only have to switch once (as opposed to have the
					for-loop around the switch)
				*/
				default: {
					switch(side) {
						case Border3DSide.Left:	{
							for (int i=0; i<width; i++) {
								graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
							}
							break;
						}

						case Border3DSide.Right: {
							for (int i=0; i<width; i++) {
								graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
							}
							break;
						}

						case Border3DSide.Top: {
							for (int i=0; i<width; i++) {
								graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
							}
							break;
						}

						case Border3DSide.Bottom: {
							for (int i=0; i<width; i++) {
								graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
							}
							break;
						}
					}
					break;
				}
			}
			pen.Dispose();
		}

		/*
			This function actually draws the various caption elements.
			This way we can scale them nicely, no matter what size, and they
			still look like MS's scaled caption buttons. (as opposed to scaling a bitmap)
		*/

		private static void DrawCaptionHelper(Graphics graphics, Color color, Pen pen, int lineWidth, int shift, Rectangle captionRect, CaptionButton button) {
			switch(button) {
				case CaptionButton.Close: {
					pen.StartCap=LineCap.Triangle;
					pen.EndCap=LineCap.Triangle;
					if (lineWidth<2) {
						graphics.DrawLine(pen, captionRect.Left+2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
						graphics.DrawLine(pen, captionRect.Right-2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
					}

					graphics.DrawLine(pen, captionRect.Left+2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
					graphics.DrawLine(pen, captionRect.Right-2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
					return;
				}

				case CaptionButton.Help: {
					StringFormat	sf = new StringFormat();
					SolidBrush		sb = new SolidBrush(color);
					Font				font = new Font("Microsoft Sans Serif", captionRect.Height, FontStyle.Bold, GraphicsUnit.Pixel);

					sf.Alignment=StringAlignment.Center;
					sf.LineAlignment=StringAlignment.Center;


					graphics.DrawString("?", font, sb, captionRect.X+captionRect.Width/2+shift, captionRect.Y+captionRect.Height/2+shift+lineWidth/2, sf);

					sf.Dispose();
					sb.Dispose();
					font.Dispose();

					return;
				}

				case CaptionButton.Maximize: {
					/* Top 'caption bar' line */
					for (int i=0; i<Math.Max(2, lineWidth); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+2*lineWidth+shift+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift+i);
					}

					/* Left side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
					}

					/* Right side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Bottom-lineWidth+shift);
					}

					/* Bottom line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
					}
					return;
				}

				case CaptionButton.Minimize: {
					/* Bottom line */
					for (int i=0; i<Math.Max(2, lineWidth); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth+shift, captionRect.Bottom-lineWidth+shift-i);
					}
					return;
				}

				case CaptionButton.Restore: {
					/** First 'window' **/
					/* Top 'caption bar' line */
					for (int i=0; i<Math.Max(2, lineWidth); i++) {
						graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift, captionRect.Top+2*lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift-i);
					}

					/* Left side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+4*lineWidth+shift);
					}

					/* Right side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+5*lineWidth-lineWidth/2+shift);
					}

					/* Bottom line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i);
					}

					/** Second 'window' **/
					/* Top 'caption bar' line */
					for (int i=0; i<Math.Max(2, lineWidth); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+4*lineWidth+shift+1-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+4*lineWidth+shift+1-i);
					}

					/* Left side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+4*lineWidth+shift+1, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
					}

					/* Right side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Top+4*lineWidth+shift+1, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Bottom-lineWidth+shift);
					}

					/* Bottom line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
					}

					return;
				}

			}
		}

		[MonoTODO("Finish drawing code for Caption, Menu and Scroll")]
		private void DrawFrameControl(Graphics graphics, Rectangle rectangle, DrawFrameControlTypes Type, DrawFrameControlStates State) 
{
			switch(Type) {
				case DrawFrameControlTypes.Button: {
					if ((State & DrawFrameControlStates.ButtonPush)!=0) {
						/* Goes first, affects the background */
						if ((State & DrawFrameControlStates.Checked)!=0) {
							HatchBrush	hatchBrush=new HatchBrush(HatchStyle.Percent50, SystemColors.ControlLight, SystemColors.ControlLightLight);
							graphics.FillRectangle(hatchBrush,rectangle);
							hatchBrush.Dispose();
						}

						if ((State & DrawFrameControlStates.Pushed)!=0) {
							DrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
						} else if ((State & DrawFrameControlStates.Flat)!=0) {
							ControlPaint.DrawBorder(graphics, rectangle, ColorButtonShadow, ButtonBorderStyle.Solid);
						} else if ((State & DrawFrameControlStates.Inactive)!=0) {
							/* Same as normal, it would seem */
							DrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
						} else {
							DrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
						}
					} else if ((State & DrawFrameControlStates.ButtonRadio)!=0) {
						Pen			penFatDark	= new Pen(ColorButtonShadow, 2);
						Pen			penFatLight	= new Pen(SystemColors.ControlLight, 2);
						int			lineWidth;

						graphics.DrawArc(penFatDark, rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 135, 180);
						graphics.DrawArc(penFatLight, rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 315, 180);

						graphics.DrawArc(SystemPens.ControlDark, rectangle, 135, 180);
						graphics.DrawArc(SystemPens.ControlLightLight, rectangle, 315, 180);

						lineWidth=Math.Max(1, Math.Min(rectangle.Width, rectangle.Height)/3);

						if ((State & DrawFrameControlStates.Checked)!=0) {
							SolidBrush	buttonBrush;

							if ((State & DrawFrameControlStates.Inactive)!=0) {
								buttonBrush=(SolidBrush)SystemBrushes.ControlDark;
							} else {
								buttonBrush=(SolidBrush)SystemBrushes.ControlText;
							}
							graphics.FillPie(buttonBrush, rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2, 0, 359);
						}
						penFatDark.Dispose();
						penFatLight.Dispose();
					} else if ((State & DrawFrameControlStates.ButtonRadioImage)!=0) {
						throw new NotImplementedException () ;
					} else if ((State & DrawFrameControlStates.ButtonRadioMask)!=0) {
						throw new NotImplementedException ();
					} else {	/* Must be Checkbox */
						Pen			pen;
						int			lineWidth;
						Rectangle	rect;
						int			Scale;

						/* FIXME: I'm sure there's an easier way to calculate all this, but it should do for now */

						/* Goes first, affects the background */
						if ((State & DrawFrameControlStates.Pushed)!=0) {
							HatchBrush	hatchBrush=new HatchBrush(HatchStyle.Percent50, SystemColors.ControlLight, SystemColors.ControlLightLight);
							graphics.FillRectangle(hatchBrush,rectangle);
							hatchBrush.Dispose();
						}

						/* Draw the sunken frame */
						if ((State & DrawFrameControlStates.Flat)!=0) {
							ControlPaint.DrawBorder(graphics, rectangle, ColorButtonShadow, ButtonBorderStyle.Solid);
						} else {
							DrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
						}

						/* Make sure we've got at least a line width of 1 */
						lineWidth=Math.Max(3, rectangle.Width/6);
						Scale=Math.Max(1, rectangle.Width/12);

						rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);
						if ((State & DrawFrameControlStates.Inactive)!=0) {
							pen=SystemPens.ControlDark;
						} else {
							pen=SystemPens.ControlText;
						}

						if ((State & DrawFrameControlStates.Checked)!=0) {
							/* Need to draw a check-mark */
							for (int i=0; i<lineWidth; i++) {
								graphics.DrawLine(pen, rect.Left+lineWidth/2, rect.Top+lineWidth+i, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i);
								graphics.DrawLine(pen, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i, rect.Left+lineWidth/2+6*Scale, rect.Top+lineWidth-2*Scale+i);
							}

						}
					}
					return;
				}

				case DrawFrameControlTypes.Caption: {
					// FIXME:
					break;
				}

				case DrawFrameControlTypes.Menu: {
					// FIXME:
					break;
				}

				case DrawFrameControlTypes.Scroll: {
					// FIXME:
					break;
				}
			}
		}

		/* Generic scroll button */
		static public void DrawScrollButtonPrimitive (Graphics dc, Rectangle area, ButtonState state)
		{
			if ((state & ButtonState.Pushed) == ButtonState.Pushed) {
				dc.FillRectangle (br_buttonface, area.X + 1,
					area.Y + 1, area.Width - 2 , area.Height - 2);

				dc.DrawRectangle (pen_buttonshadow, area.X,
					area.Y, area.Width, area.Height);
			}

			if (state == ButtonState.Normal) {

				dc.FillRectangle (new SolidBrush (Color.Blue), area);
				
				dc.FillRectangle (br_buttonface, area.X, area.Y, area.Width, 1);
				dc.FillRectangle (br_buttonface, area.X, area.Y, 1, area.Height);

				dc.FillRectangle (br_buttonhilight, area.X + 1, area.Y + 1, area.Width - 1, 1);
				dc.FillRectangle (br_buttonhilight, area.X + 1, area.Y + 2, 1,
					area.Height - 4);

				dc.FillRectangle (br_buttonshadow, area.X + 1, area.Y + area.Height - 2,
					area.Width - 2, 1);

				dc.FillRectangle (br_buttondkshadow, area.X, area.Y + area.Height -1,
					area.Width , 1);

				dc.FillRectangle (br_buttonshadow, area.X + area.Width - 2,
					area.Y + 1, 1, area.Height -3);

				dc.FillRectangle (br_buttondkshadow, area.X + area.Width -1,
					area.Y, 1, area.Height - 1);

				dc.FillRectangle (br_buttonface, area.X + 2,
					area.Y + 2, area.Width - 4, area.Height - 4);
			}
		}

		
		private void DrawBorderStyle (Graphics dc, Rectangle area, BorderStyle border_style)
		{
			switch (border_style){
			case BorderStyle.Fixed3D:				
				dc.DrawLine (pen_buttonshadow, area.X, area.Y, area.X +area.Width, area.Y);
				dc.DrawLine (pen_buttonshadow, area.X, area.Y, area.X, area.Y + area.Height);
				dc.DrawLine (pen_buttonhilight, area.X , area.Y + area.Height - 1, area.X + area.Width , 
					area.Y + area.Height - 1);
				dc.DrawLine (pen_buttonhilight, area.X + area.Width -1 , area.Y, area.X + area.Width -1, 
					area.Y + area.Height);
				break;
			case BorderStyle.FixedSingle:
				dc.DrawRectangle (pen_windowframe, area.X, area.Y, area.Width - 1, area.Height - 1);
				break;
			case BorderStyle.None:
			default:
				break;
			}
			
		}

		private SolidBrush GetControlBackBrush (Color c)
		{
			if (c == DefaultControlBackColor)
				return br_buttonface;
			return new SolidBrush (c);
		}

		private SolidBrush GetControlForeBrush (Color c)
		{
			if (c == DefaultControlForeColor)
				return br_buttontext;
			return new SolidBrush (c);
		}

	} //class
}
