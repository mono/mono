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
// $Revision: 1.35 $
// $Modtime: $
// $Log: ThemeWin32Classic.cs,v $
// Revision 1.35  2004/09/07 17:12:26  jordi
// GroupBox control
//
// Revision 1.34  2004/09/07 09:40:15  jordi
// LinkLabel fixes, methods, multiple links
//
// Revision 1.33  2004/09/05 08:03:51  jordi
// fixes bugs, adds flashing on certain situations
//
// Revision 1.32  2004/09/02 16:32:54  jordi
// implements resource pool for pens, brushes, and hatchbruses
//
// Revision 1.31  2004/08/25 20:04:40  ravindra
// Added the missing divider code and grip for ToolBar Control.
//
// Revision 1.30  2004/08/25 18:29:14  jordi
// new methods, properties, and fixes for progressbar
//
// Revision 1.29  2004/08/25 00:43:13  ravindra
// Fixed wrapping related issues in ToolBar control.
//
// Revision 1.28  2004/08/24 18:37:02  jordi
// fixes formmating, methods signature, and adds missing events
//
// Revision 1.27  2004/08/24 16:16:46  jackson
// Handle drawing picture boxes in the theme now. Draw picture box borders and obey sizing modes
//
// Revision 1.26  2004/08/21 01:52:08  ravindra
// Improvments in mouse event handling in the ToolBar control.
//
// Revision 1.25  2004/08/20 00:12:51  jordi
// fixes methods signature
//
// Revision 1.24  2004/08/19 22:25:31  jordi
// theme enhancaments
//
// Revision 1.23  2004/08/18 19:16:53  jordi
// Move colors to a table
//
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

	internal class ThemeWin32Classic : Theme
	{		

		/* Default colors for Win32 classic theme */
		uint [] theme_colors = {							/* AARRGGBB */
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_SCROLLBAR,			0xffc0c0c0,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_BACKGROUND,			0xff008080,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_ACTIVECAPTION,		0xff000080,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_INACTIVECAPTION,		0xff808080,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_MENU,			0xffc0c0c0,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_WINDOW,			0xffffffff,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_WINDOWFRAME,			0xff000000,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_MENUTEXT,			0xff000000,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_WINDOWTEXT,			0xff000000,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_CAPTIONTEXT,			0xffffffff,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_ACTIVEBORDER,		0xffc0c0c0,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_INACTIVEBORDER,		0xffc0c0c0,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_APPWORKSPACE,		0xff808080,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_HIGHLIGHT,			0xff000080,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_HIGHLIGHTTEXT,		0xffffffff,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_BTNFACE,			0xffc0c0c0,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_BTNSHADOW,			0xff808080,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_GRAYTEXT,			0xff808080,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_BTNTEXT,			0xff000000,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_INACTIVECAPTIONTEXT,		0xffc0c0c0,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_BTNHIGHLIGHT,		0xffffffff,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_3DDKSHADOW,			0xff000000,			
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_3DLIGHT,			0xffe0e0e0,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_INFOTEXT,			0xff000000,
			(uint) XplatUIWin32.GetSysColorIndex.COLOR_INFOBK,			0xffffffff,
		  
		};		
		
		static protected Pen pen_ticks;		
		static protected SolidBrush br_arrow;
		static protected SolidBrush br_progressbarblock;		
		static protected Pen pen_arrow;
				

		/* Cache */
		protected SolidBrush label_br_fore_color;
		protected SolidBrush label_br_back_color;		

		public ThemeWin32Classic ()
		{
			label_br_fore_color = null;
			label_br_back_color = null;						

			/* Init Default colour array*/
			syscolors =  Array.CreateInstance (typeof (Color), (uint) XplatUIWin32.GetSysColorIndex.COLOR_MAXVALUE+1);
			
			for (int i = 0; i < theme_colors.Length; i +=2) 
				syscolors.SetValue (Color.FromArgb ((int)theme_colors[i+1]), (int) theme_colors[i]);
 
			pen_ticks = new Pen (Color.Black);			
			br_arrow = new SolidBrush (Color.Black);
			pen_arrow = new Pen (Color.Black);
			br_progressbarblock = new SolidBrush (Color.FromArgb (255, 0, 0, 128));			

			defaultWindowBackColor = Color.FromArgb (255, 10, 10, 10);
			defaultWindowForeColor = ColorButtonText;
			default_font =	new Font (FontFamily.GenericSansSerif, 8.25f);
		}	

		public override bool WriteToWindow {
			get {return false; }
		}
		
		public override int SizeGripWidth {
			get { return 15; }
		}

		public override int StatusBarHorzGapWidth {
			get { return 3; }
		}

		public override int ScrollBarButtonSize {
			get { return 16; }
		}

		/*
		 * ToolBar Control properties
		 */
		// Grip width for the ToolBar
		public override int ToolBarGripWidth
		{
			get { return 2;}
		}

		// Grip width for the Image on the ToolBarButton
		public override int ToolBarImageGripWidth
		{
			get { return 2;}
		}

		// width of the separator
		public override int ToolBarSeparatorWidth {
			get { return 4; }
		}

		// width of the dropdown arrow rect
		public override int ToolBarDropDownWidth {
			get { return 13; }
		}

		// width for the dropdown arrow on the ToolBarButton
		public override int ToolBarDropDownArrowWidth {
			get { return 5;}
		}

		// height for the dropdown arrow on the ToolBarButton
		public override int ToolBarDropDownArrowHeight {
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

		public override void DrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle)
		{
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
		}

		public override void DrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides)
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


		public override void DrawButton (Graphics graphics, Rectangle rectangle, ButtonState state)
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


		public override void DrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state)
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


		public override void DrawCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state)
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

		public override void DrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state)
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
				graphics.FillRectangle(ResPool.GetHatchBrush (HatchStyle.Percent50, SystemColors.ControlLight, ColorButtonHilight),rectangle);				
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


		public override void DrawContainerGrabHandle (Graphics graphics, Rectangle bounds)
		{
			
			Pen			pen	= new Pen(Color.Black, 1);
			Rectangle	rect	= new Rectangle(bounds.X, bounds.Y, bounds.Width-1, bounds.Height-1);	// Dunno why, but MS does it that way, too
			int			X;
			int			Y;

			graphics.FillRectangle(ResPool.GetSolidBrush (ColorButtonText), rect);
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


		public override void DrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor)
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


		public override void DrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled)
		{
			SolidBrush	sb;
			Pen			pen;

			if (primary==true) {
				pen=new Pen(Color.Black, 1);
				if (enabled==true) {
					sb=ResPool.GetSolidBrush (ColorButtonText);
				} else {
					sb=ResPool.GetSolidBrush (ColorButtonFace);
				}
			} else {
				pen=new Pen(Color.White, 1);
				if (enabled==true) {
					sb=new SolidBrush(Color.Black);
				} else {
					sb=ResPool.GetSolidBrush (ColorButtonFace);
				}
			}
			graphics.FillRectangle(sb, rectangle);
			graphics.DrawRectangle(pen, rectangle);
			sb.Dispose();
			pen.Dispose();
		}


		public override void DrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor)
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

		public override void DrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background)
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


		public override void DrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary)
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


		public override void DrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph)
		{
			Rectangle	rect;
			int			lineWidth;

			// MS seems to draw the background white
			graphics.FillRectangle(ResPool.GetSolidBrush (ColorButtonText), rectangle);

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

					sb=ResPool.GetSolidBrush (ColorButtonText);
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

		public override void DrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state)
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


		public override void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style)
		{

		}


		public override void DrawReversibleLine (Point start, Point end, Color backColor)
		{

		}


		/* Scroll button: regular button + direction arrow */
		public override void DrawScrollButton (Graphics dc, Rectangle area, ScrollButton type, ButtonState state)
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
						dc.DrawLine (ResPool.GetPen (ColorGrayText), x + i, y - i, x + i + 6 - 2*i, y - i);

				
				dc.FillRectangle (br_arrow, x + 3, area.Y + 6, 1, 1);				
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
						dc.DrawLine (ResPool.GetPen (ColorGrayText), x + i, y + i, x + i + 8 - 2*i, y + i);

				
				dc.FillRectangle (br_arrow, x + 4, y + 4, 1, 1);
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
						dc.DrawLine (ResPool.GetPen (ColorGrayText), x - i, y + i, x - i, y + i + 6 - 2*i);

				dc.FillRectangle (br_arrow, x - 3, y + 3, 1, 1);
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
						dc.DrawLine (ResPool.GetPen (ColorGrayText), x + i, y + i, x + i, y + i + 8 - 2*i);

				dc.FillRectangle (br_arrow, x + 4, y + 4, 1, 1);				
				break;
			}

			default:
				break;

			}
		}


		public  override void DrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor)
		{

		}


		public override void DrawSizeGrip (Graphics dc, Color backColor, Rectangle bounds)
		{
			Point pt = new Point (bounds.Right - 2, bounds.Bottom - 1);

			dc.DrawLine (ResPool.GetPen (ColorButtonFace), pt.X - 12, pt.Y, pt.X, pt.Y);
			dc.DrawLine (ResPool.GetPen (ColorButtonFace), pt.X, pt.Y, pt.X, pt.Y - 13);

			// diagonals
			for (int i = 0; i < 11; i += 4) {
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), pt.X - i, pt.Y, pt.X + 1, pt.Y - i - 2);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), pt.X - i - 1, pt.Y, pt.X + 1, pt.Y - i - 2);
			}

			for (int i = 3; i < 13; i += 4)
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), pt.X - i, pt.Y, pt.X + 1, pt.Y - i - 1);
		}


		public  override void DrawStringDisabled (Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle,
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


		public override void DrawScrollBar (Graphics dc, Rectangle area, ScrollBar bar,
			ref Rectangle thumb_pos, ref Rectangle first_arrow_area, ref Rectangle second_arrow_area, 
			ButtonState first_arrow, ButtonState second_arrow, ref int scrollbutton_width, 
			ref int scrollbutton_height, bool vert)
		{
			
			if (vert) {		

				first_arrow_area.X = first_arrow_area. Y = 0;
				first_arrow_area.Width = bar.Width;
				first_arrow_area.Height = scrollbutton_height;

				second_arrow_area.X = 0;
				second_arrow_area.Y = area.Height - scrollbutton_height;
				second_arrow_area.Width = bar.Width;
				second_arrow_area.Height = scrollbutton_height;
				thumb_pos.Width = bar.Width;

				/* Buttons */
				DrawScrollButton (dc, first_arrow_area, ScrollButton.Up, first_arrow);
				DrawScrollButton (dc, second_arrow_area, ScrollButton.Down, second_arrow);				

				/* Background */
				switch (bar.thumb_moving) {
				case ScrollBar.ThumbMoving.None:				
				{
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace), 0,  
						scrollbutton_height, area.Width, area.Height - (scrollbutton_height * 2));
					
					break;
				}
				case ScrollBar.ThumbMoving.Forward: {
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace),
						0,  scrollbutton_height,
						area.Width, thumb_pos.Y - scrollbutton_height);
												
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63,63,63), Color.Black),
						0, thumb_pos.Y + thumb_pos.Height,
						area.Width, area.Height -  (thumb_pos.Y + thumb_pos.Height) - scrollbutton_height);						
						
					break;
				}
				
				case ScrollBar.ThumbMoving.Backwards: {
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63,63,63), Color.Black),
						0,  scrollbutton_height,
						area.Width, thumb_pos.Y - scrollbutton_height);
												
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace),
						0, thumb_pos.Y + thumb_pos.Height,
						area.Width, area.Height -  (thumb_pos.Y + thumb_pos.Height) - scrollbutton_height);						
						
					break;
				}
				
				default:
					break;
				}
					
			}
			else {
				
				first_arrow_area.X = first_arrow_area. Y = 0;
				first_arrow_area.Width = scrollbutton_width;
				first_arrow_area.Height = bar.Height;

				second_arrow_area.Y = 0;
				second_arrow_area.X = area.Width - scrollbutton_width;
				second_arrow_area.Width = scrollbutton_width;
				second_arrow_area.Height = bar.Height;
				thumb_pos.Height = bar.Height;

				/* Buttons */
				DrawScrollButton (dc, first_arrow_area, ScrollButton.Left, first_arrow );
				DrawScrollButton (dc, second_arrow_area, ScrollButton.Right, second_arrow);

				/* Background */
				//dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace), scrollbutton_width, 
				//	0, area.Width - (scrollbutton_width * 2), area.Height);
					
				switch (bar.thumb_moving) {
				case ScrollBar.ThumbMoving.None:				
				{
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace), scrollbutton_width,
						0, area.Width - (scrollbutton_width * 2), area.Height);
					
					break;
				}
				
				case ScrollBar.ThumbMoving.Forward: {
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace),
						scrollbutton_width,  0,
						thumb_pos.X - scrollbutton_width, area.Height);
												
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63,63,63), Color.Black),
						thumb_pos.X + thumb_pos.Width, 0,
						area.Width -  (thumb_pos.X + thumb_pos.Width) - scrollbutton_width, area.Height);
						
					break;
				}
				
				case ScrollBar.ThumbMoving.Backwards: {
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, Color.FromArgb (255, 63,63,63), Color.Black),
						scrollbutton_width,  0,
						thumb_pos.X - scrollbutton_width, area.Height);
												
					dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace),
						thumb_pos.X + thumb_pos.Width, 0,
						area.Width -  (thumb_pos.X + thumb_pos.Width) - scrollbutton_width, area.Height);

						
					break;
				}
				
				default:
					break;
				}
			}

			/* Thumb */
			if (bar.Enabled)
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
			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonShadow), channel_startpoint.X, channel_startpoint.Y,
				1, thumb_area.Height);
			
			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonDkShadow), channel_startpoint.X + 1, channel_startpoint.Y,
				1, thumb_area.Height);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonHilight), channel_startpoint.X + 3, channel_startpoint.Y,
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

				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y, thumb_pos.X , thumb_pos.Y + 10);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y, thumb_pos.X + 16, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X + 16, thumb_pos.Y, thumb_pos.X + 16 + 4, thumb_pos.Y + 4);
				
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X +1, thumb_pos.Y + 9, thumb_pos.X +15, thumb_pos.Y  +9);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 16, thumb_pos.Y + 9, thumb_pos.X +16 + 4, thumb_pos.Y  +9 - 4);

				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X, thumb_pos.Y  + 10, thumb_pos.X +16, thumb_pos.Y +10);
				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X + 16, thumb_pos.Y  + 10, thumb_pos.X  +16 + 5, thumb_pos.Y +10 - 5);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 16, 8);
				dc.FillRectangle (br_thumb, thumb_pos.X + 17, thumb_pos.Y + 2, 1, 6);
				dc.FillRectangle (br_thumb, thumb_pos.X + 18, thumb_pos.Y + 3, 1, 4);
				dc.FillRectangle (br_thumb, thumb_pos.X + 19, thumb_pos.Y + 4, 1, 2);

				break;
			}
			case TickStyle.TopLeft:
			{
				thumb_pos.X = channel_startpoint.X - 10;

				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X + 4 + 16, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 4);

				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X  + 4, thumb_pos.Y + 9, thumb_pos.X + 4 + 16 , thumb_pos.Y+ 9);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 4, thumb_pos.Y  + 9, thumb_pos.X, thumb_pos.Y + 5);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X  + 19, thumb_pos.Y + 9, thumb_pos.X  +19 , thumb_pos.Y+ 1);

				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X  + 4, thumb_pos.Y+ 10, thumb_pos.X  + 4 + 16, thumb_pos.Y+ 10);
				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X  + 4, thumb_pos.Y + 10, thumb_pos.X  -1, thumb_pos.Y+ 5);
				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X + 20, thumb_pos.Y, thumb_pos.X+ 20, thumb_pos.Y + 10);

				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 15, 8);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 1, 6);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 1, 4);
				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 1, 2);

				break;
			}

			case TickStyle.Both:
			{
				thumb_pos.X = area.X + 10;
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 9);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y, thumb_pos.X + 19, thumb_pos.Y);

				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 1, thumb_pos.Y + 9, thumb_pos.X+ 19, thumb_pos.Y  + 9);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X  + 10, thumb_pos.Y+ 1, thumb_pos.X + 19, thumb_pos.Y  + 8);

				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X+ 20, thumb_pos.Y  +10);
				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X  + 20, thumb_pos.Y, thumb_pos.X  + 20, thumb_pos.Y+ 9);

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
			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonShadow), channel_startpoint.X, channel_startpoint.Y,
				thumb_area.Width, 1);
			
			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonDkShadow), channel_startpoint.X, channel_startpoint.Y + 1,
				thumb_area.Width, 1);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonHilight), channel_startpoint.X, channel_startpoint.Y +3,
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

				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 16);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y + 16, thumb_pos.X + 4, thumb_pos.Y + 16 + 4);

				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X +9, thumb_pos.Y +15);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 9, thumb_pos.Y + 16, thumb_pos.X +9 - 4, thumb_pos.Y +16 + 4);

				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X +10, thumb_pos.Y +16);
				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X + 10, thumb_pos.Y + 16, thumb_pos.X +10 - 5, thumb_pos.Y +16 + 5);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 16);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 17, 6, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 18, 4, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 19, 2, 1);
				break;
			}
			case TickStyle.TopLeft:	{
				thumb_pos.Y = channel_startpoint.Y - 10;

				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X, thumb_pos.Y + 4 + 16);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X + 4, thumb_pos.Y);

				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 9, thumb_pos.Y + 4 + 16);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 9, thumb_pos.Y + 19, thumb_pos.X + 1 , thumb_pos.Y +19);

				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 10, thumb_pos.Y + 4 + 16);
				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y -1);
				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 10, thumb_pos.Y + 20);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 8, 15);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 6, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 4, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 2, 1);
				break;
			}

			case TickStyle.Both: {
				thumb_pos.Y = area.Y + 10;
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y, thumb_pos.X + 9, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 19);

				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X + 9, thumb_pos.Y + 19);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), thumb_pos.X + 1, thumb_pos.Y + 10, thumb_pos.X + 8, thumb_pos.Y + 19);

				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X +10, thumb_pos.Y + 20);
				dc.DrawLine (ResPool.GetPen (ColorButtonDkShadow), thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 9, thumb_pos.Y + 20);

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

		public override void DrawTrackBar (Graphics dc, Rectangle area, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area,  bool highli_thumb,
			float ticks, int value_pos, bool mouse_value)

		{
			Brush br_thumb;			

			if (highli_thumb == true)
				br_thumb = (Brush) ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonHilight, ColorButtonFace);
			else
				br_thumb = ResPool.GetSolidBrush (ColorButtonFace);

			
			/* Control Background */
			if (tb.BackColor == DefaultControlBackColor)
				dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonFace), area);
			else
				dc.FillRectangle (ResPool.GetSolidBrush (tb.BackColor), area);
			

			if (tb.Focused) {
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonFace, Color.Black), area.X, area.Y, area.Width - 1, 1);
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonFace, Color.Black), area.X, area.Y + area.Height - 1, area.Width - 1, 1);
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonFace, Color.Black), area.X, area.Y, 1, area.Height - 1);
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorButtonFace, Color.Black), area.X + area.Width - 1, area.Y, 1, area.Height - 1);
			}

			if (tb.Orientation == Orientation.Vertical) 
				DrawTrackBar_Vertical (dc, area, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
			
			else
				DrawTrackBar_Horizontal (dc, area, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
		}

		public override void DrawProgressBar (Graphics dc, Rectangle area,  Rectangle client_area,
			int barpos_pixels, int block_width)

		{
			int space_betweenblocks = 2;
			int increment = block_width + space_betweenblocks;
			int x = client_area.X;

			/* Draw border */
			DrawBorder3D (dc, area, Border3DStyle.SunkenInner, Border3DSide.All);
			
			/* Draw Blocks */
			while ((x - client_area.X) < barpos_pixels) {

				dc.FillRectangle (br_progressbarblock, x, client_area.Y, block_width, client_area.Height);
				x  = x + increment;
			}			
		}
		

		public  override void DrawLabel (Graphics dc, Rectangle area, BorderStyle border_style, string text, 
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

		public  override void DrawStatusBar (Graphics dc, Rectangle area, StatusBar sb)
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


		public override void DrawStatusBarPanel (Graphics dc, Rectangle area, int index,
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

		public override void DrawPictureBox (Graphics dc, PictureBox pb)
		{
			Rectangle client = pb.ClientRectangle;
			int x, y, width, height;

			dc.FillRectangle (new SolidBrush (pb.BackColor), client);
			DrawBorderStyle (dc, client, pb.BorderStyle);

			x = y = 0;
			switch (pb.SizeMode) {
			case PictureBoxSizeMode.StretchImage:
				width = client.Width;
				height = client.Height;
				break;
			case PictureBoxSizeMode.CenterImage:
				width = client.Width;
				height = client.Height;
				x = width / 2;
				y = (height - pb.Image.Height) / 2;
				break;
			default:
				// Normal, AutoSize
				width = client.Width;
				height = client.Height;
				break;
			}
			dc.DrawImage (pb.Image, x, y, width, height);
			
		}

		public  override void DrawOwnerDrawBackground (DrawItemEventArgs e)
		{
			if (e.State == DrawItemState.Selected) {
				e.Graphics.FillRectangle (SystemBrushes.Highlight, e.Bounds);
				return;
			}

			e.Graphics.FillRectangle (GetControlBackBrush (e.BackColor), e.Bounds);
		}

		public  override void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e)
		{
			if (e.State == DrawItemState.Focus)
				DrawFocusRectangle (e.Graphics, e.Bounds, e.ForeColor, e.BackColor);
		}

		public  override void DrawToolBar (Graphics dc, ToolBar control, StringFormat format)
		{
			// Exclude the area for divider
			Rectangle paint_area = new Rectangle (0, ThemeEngine.Current.ToolBarGripWidth / 2, 
							      control.Width, control.Height - ThemeEngine.Current.ToolBarGripWidth / 2);
			bool flat = (control.Appearance == ToolBarAppearance.Flat);

			dc.FillRectangle (SystemBrushes.Control, paint_area);
			DrawBorderStyle (dc, paint_area, control.BorderStyle);

			if (control.Divider)
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), 0, 0, paint_area.Width, 0);

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
						if (button.Pushed || button.Pressed)
							ControlPaint.DrawBorder3D (dc, buttonArea, Border3DStyle.SunkenOuter,
										   Border3DSide.All);
						else if (button.Hilight) {
							dc.DrawRectangle (ResPool.GetPen (ColorButtonText), buttonArea);
							if (! ddRect.IsEmpty) {
								dc.DrawLine (ResPool.GetPen (ColorButtonText), ddRect.X, ddRect.Y, ddRect.X, 
									     ddRect.Y + ddRect.Height);
								buttonArea.Width -= this.ToolBarDropDownWidth;
							}
						}
					}
					else { // normal toolbar
						if (button.Pushed || button.Pressed) {
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
		
		public override void DrawGroupBox (Graphics dc,  Rectangle area, GroupBox box)
		{
			SizeF size;
			int width, y;
			Rectangle rect = box.ClientRectangle;
			Color disabled = ThemeEngine.Current.ColorGrayText;
			
			Pen pen_light = ResPool.GetPen (Color.FromArgb (255,255,255,255));
			Pen pen_dark = ResPool.GetPen (Color.FromArgb (255, 128, 128,128));
			
			// TODO: When the Light and Dark methods work this code should be activate it
			//Pen pen_light = new Pen (ControlPaint.Light (disabled, 1));
			//Pen pen_dark = new Pen (ControlPaint.Dark (disabled, 0));

			dc.FillRectangle (ResPool.GetSolidBrush (box.BackColor), rect);

			size = dc.MeasureString (box.Text, box.Font);
			width = (int) size.Width;
			
			if (width > box.Width - 16)
				width = box.Width - 16;
			
			y = box.Font.Height / 2;
			
			/* Draw group box*/
			dc.DrawLine (pen_dark, 0, y, 8, y); // top 
			dc.DrawLine (pen_light, 0, y + 1, 8, y + 1);			
			dc.DrawLine (pen_dark, 8 + width, y, box.Width, y);			
			dc.DrawLine (pen_light, 8 + width, y + 1, box.Width, y + 1);
			
			dc.DrawLine (pen_dark, 0, y + 1, 0, box.Height); // left
			dc.DrawLine (pen_light, 1, y + 1, 1, box.Height);			
			
			dc.DrawLine (pen_dark, 0, box.Height - 2, box.Width,  box.Height - 2); // bottom
			dc.DrawLine (pen_light, 0, box.Height - 1, box.Width,  box.Height - 1);
			
			dc.DrawLine (pen_dark, box.Width - 2, y,  box.Width - 2, box.Height - 2); // right
			dc.DrawLine (pen_light, box.Width - 1, y, box.Width - 1, box.Height - 2);
			
			
			/* Text */
			if (box.Enabled)
				dc.DrawString (box.Text, box.Font, new SolidBrush (box.ForeColor), 10, 0);
			else
				DrawStringDisabled (dc, box.Text, box.Font, box.ForeColor, 
					new RectangleF (10, 0, width,  box.Font.Height), new StringFormat ());					
				
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
					dc.DrawLine (ResPool.GetPen (ColorButtonShadow), buttonArea.X + 1, buttonArea.Y, 
						     buttonArea.X + 1, buttonArea.Height);
					dc.DrawLine (ResPool.GetPen (ColorButtonHilight), buttonArea.X + 1 + (int) ResPool.GetPen (ColorButtonFace).Width,
						     buttonArea.Y, buttonArea.X + 1 + (int) ResPool.GetPen (ColorButtonFace).Width, buttonArea.Height);
					/* draw a horizontal separator */
					if (button.Wrapper) {
						int y = buttonArea.Height + this.ToolBarSeparatorWidth / 2;
						dc.DrawLine (ResPool.GetPen (ColorButtonShadow), 0, y, controlArea.Width, y);
						dc.DrawLine (ResPool.GetPen (ColorButtonHilight), 0, y + 1 + (int) ResPool.GetPen (ColorButtonFace).Width, controlArea.Width,
							     y + 1 + (int) ResPool.GetPen (ColorButtonFace).Width);
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
					dc.FillRectangle (SystemBrushes.Control, toggleArea);
					//dc.FillRectangle (new SolidBrush (Color.FromArgb(255, 180, 190, 214)), toggleArea);
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
		public void DrawScrollButtonPrimitive (Graphics dc, Rectangle area, ButtonState state)
		{
			if ((state & ButtonState.Pushed) == ButtonState.Pushed) {
				dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonFace), area.X + 1,
					area.Y + 1, area.Width - 2 , area.Height - 2);

				dc.DrawRectangle (ResPool.GetPen (ColorButtonShadow), area.X,
					area.Y, area.Width, area.Height);

				return;
			}			

			dc.FillRectangle (new SolidBrush (Color.Blue), area);
			
			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonFace), area.X, area.Y, area.Width, 1);
			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonFace), area.X, area.Y, 1, area.Height);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonHilight), area.X + 1, area.Y + 1, area.Width - 1, 1);
			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonHilight), area.X + 1, area.Y + 2, 1,
				area.Height - 4);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonShadow), area.X + 1, area.Y + area.Height - 2,
				area.Width - 2, 1);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonDkShadow), area.X, area.Y + area.Height -1,
				area.Width , 1);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonShadow), area.X + area.Width - 2,
				area.Y + 1, 1, area.Height -3);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonDkShadow), area.X + area.Width -1,
				area.Y, 1, area.Height - 1);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorButtonFace), area.X + 2,
				area.Y + 2, area.Width - 4, area.Height - 4);
			
		}
		
		public override void DrawBorderStyle (Graphics dc, Rectangle area, BorderStyle border_style)
		{
			switch (border_style){
			case BorderStyle.Fixed3D:				
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), area.X, area.Y, area.X +area.Width, area.Y);
				dc.DrawLine (ResPool.GetPen (ColorButtonShadow), area.X, area.Y, area.X, area.Y + area.Height);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), area.X , area.Y + area.Height - 1, area.X + area.Width , 
					area.Y + area.Height - 1);
				dc.DrawLine (ResPool.GetPen (ColorButtonHilight), area.X + area.Width -1 , area.Y, area.X + area.Width -1, 
					area.Y + area.Height);
				break;
			case BorderStyle.FixedSingle:
				dc.DrawRectangle (ResPool.GetPen (ColorWindowFrame), area.X, area.Y, area.Width - 1, area.Height - 1);
				break;
			case BorderStyle.None:
			default:
				break;
			}
			
		}

		protected SolidBrush GetControlBackBrush (Color c)
		{
			if (c == DefaultControlBackColor)
				return ResPool.GetSolidBrush (ColorButtonFace);
			return new SolidBrush (c);
		}

		protected SolidBrush GetControlForeBrush (Color c)
		{
			if (c == DefaultControlForeColor)
				return ResPool.GetSolidBrush (ColorButtonText);
			return new SolidBrush (c);
		}

	} //class
}
