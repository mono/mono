//
// System.Windows.Forms.ControlPaint.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//	 Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc 2002/3
//


using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides methods used to paint common Windows controls and their elements.
	/// </summary>
	
	[MonoTODO]
	public sealed class ControlPaint {
		static int		RGBMax=255;
		static int		HLSMax=255;
		static Color	colorHighlight;
		static Color	colorLight;
		static Color	colorShadow;
		static Color	colorDarkShadow;
		static Color	colorSurface;
		static Pen		penHighlight;
		static Pen		penLight;
		static Pen		penShadow;
		static Pen		penDarkShadow;

		static ControlPaint() {
			colorHighlight=Win32ToColor(Win32.GetSysColor(GetSysColorIndex.COLOR_3DHIGHLIGHT));
			colorLight=Win32ToColor(Win32.GetSysColor(GetSysColorIndex.COLOR_3DLIGHT));
			colorShadow=Win32ToColor(Win32.GetSysColor(GetSysColorIndex.COLOR_3DSHADOW));
			colorDarkShadow=Win32ToColor(Win32.GetSysColor(GetSysColorIndex.COLOR_3DDKSHADOW));
			colorSurface=Win32ToColor(Win32.GetSysColor(GetSysColorIndex.COLOR_3DFACE));

			penHighlight=new Pen(colorHighlight);
			penLight=new Pen(colorLight);
			penShadow=new Pen(colorShadow);
			penDarkShadow= new Pen(colorDarkShadow);
		}

		private static Color Win32ToColor(int Win32Color) {
			return(Color.FromArgb(
				(int)(Win32Color) & 0xff0000 >> 16,		// blue
				(int)(Win32Color) & 0xff00 >> 8,			// green
				(int)(Win32Color) & 0xff					// red
			));
		}

		#region Properties
		[MonoTODO]
		public static Color ContrastControlDark {

			get { throw new NotImplementedException (); }
		}
		#endregion
		
		#region Helpers
		internal static void Color2HBS(Color color, out int h, out int l, out int s) {
			int	r;
			int	g;
			int	b;
			int	cMax;
			int	cMin;
			int	rDelta;
			int	gDelta;
			int	bDelta;
	
			r=color.R;
			g=color.G;
			b=color.B;

			cMax = Math.Max(Math.Max(r, g), b);
			cMin = Math.Min(Math.Min(r, g), b);

			l = (((cMax+cMin)*HLSMax)+RGBMax)/(2*RGBMax);

			if (cMax==cMin) {		// Achromatic
				h=0;					// h undefined
				s=0;
				l=r;
				return;
			}

			/* saturation */
			if (l<=(HLSMax/2)) {
				s=(((cMax-cMin)*HLSMax)+((cMax+cMin)/2))/(cMax+cMin);
			} else {
				s=(((cMax-cMin)*HLSMax)+((2*RGBMax-cMax-cMin)/2))/(2*RGBMax-cMax-cMin);
			}

			/* hue */
			rDelta=(((cMax-r)*(HLSMax/6))+((cMax-cMin)/2))/(cMax-cMin);
			gDelta=(((cMax-g)*(HLSMax/6))+((cMax-cMin)/2))/(cMax-cMin);
			bDelta=(((cMax-b)*(HLSMax/6))+((cMax-cMin)/2))/(cMax-cMin);

			if (r == cMax) {
				h=bDelta - gDelta;
			} else if (g == cMax) {
				h=(HLSMax/3) + rDelta - bDelta;
			} else { /* B == cMax */
				h=((2*HLSMax)/3) + gDelta - rDelta;
			}

			if (h<0) {
				h+=HLSMax;
			}

			if (h>HLSMax) {
				h-=HLSMax;
			}
		}

		private static int HueToRGB(int n1, int n2, int hue) {
			if (hue<0) {
				hue+=HLSMax;
			}

			if (hue>HLSMax) {
				hue -= HLSMax;
			}

			/* return r,g, or b value from this tridrant */ 
			if (hue<(HLSMax/6)) {
				return(n1+(((n2-n1)*hue+(HLSMax/12))/(HLSMax/6)));
			}

			if (hue<(HLSMax/2)) {
				return(n2);
			}

			if (hue<((HLSMax*2)/3)) {
				return(n1+(((n2-n1)*(((HLSMax*2)/3)-hue)+(HLSMax/12))/(HLSMax/6)));
			} else {
				return(n1);
			}
		}

		private static Color HBS2Color(int hue, int lum, int sat) {
			int	R;
			int	G;
			int	B;
			int	Magic1;
			int	Magic2;

			if (sat == 0) {            /* Achromatic */ 
				R=G=B=(lum*RGBMax)/HLSMax;
				// FIXME : Should throw exception if hue!=0
			} else {
				if (lum<=(HLSMax/2)) {
					Magic2=(lum*(HLSMax+sat)+(HLSMax/2))/HLSMax;
				} else {
					Magic2=sat+lum-((sat*lum)+(HLSMax/2))/HLSMax;
				}
				Magic1=2*lum-Magic2;
				
				R = Math.Min(255, (HueToRGB(Magic1,Magic2,hue+(HLSMax/3))*RGBMax+(HLSMax/2))/HLSMax);
				G = Math.Min(255, (HueToRGB(Magic1,Magic2,hue)*RGBMax+(HLSMax/2))/HLSMax);
				B = Math.Min(255, (HueToRGB(Magic1,Magic2,hue-(HLSMax/3))*RGBMax+(HLSMax/2))/HLSMax);
			}
			return(Color.FromArgb(R, G, B));
		}
		#endregion

		#region Methods
		/// following methods were not stubbed out, because they only support .NET framework:
		
		[MonoTODO]
		public static IntPtr CreateHBitmap16Bit(Bitmap bitmap,Color background){
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static IntPtr CreateHBitmapColorMask(Bitmap bitmap,IntPtr monochromeMask){
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static IntPtr CreateHBitmapTransparencyMask(Bitmap bitmap){
			throw new NotImplementedException ();
		}

		public static Color Dark(Color baseColor) {
			return Dark(baseColor, 10.0f);
		}
		
		public static Color Dark(Color baseColor,float percOfDarkDark) {
			int H, I, S;
			ControlPaint.Color2HBS(baseColor, out H, out I, out S);
			int NewIntensity = Math.Max(0, I - ((255*(int)percOfDarkDark) / 100));
			return ControlPaint.HBS2Color(H, NewIntensity, S);
		}
		
		public static Color DarkDark(Color baseColor) {
			return Dark(baseColor, 20.0f);
		}
		
		public static void DrawBorder(Graphics graphics, Rectangle bounds, Color color, ButtonBorderStyle style) {
			DrawBorder(graphics, bounds, color, 1, style, color, 1, style, color, 1, style, color, 1, style);
		}
		
		public static void DrawBorder( Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor, int bottomWidth,
			ButtonBorderStyle bottomStyle) {

			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
#if false
			IntPtr hdc = graphics.GetHdc();

			RECT rc = new RECT();

			// Top side
			Win32.SetBkColor(hdc, (uint)Win32.RGB(topColor));
			rc.left = bounds.Left;
			rc.top = bounds.Top;
			rc.right = bounds.Right - rightWidth;
			rc.bottom = bounds.Top + topWidth;
			Win32.ExtTextOut(hdc, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);
			// Left side
			Win32.SetBkColor(hdc, (uint)Win32.RGB(leftColor));
			rc.right = bounds.Left + leftWidth;
			rc.bottom = bounds.Bottom - bottomWidth;
			Win32.ExtTextOut(hdc, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);
			// Right side
			Win32.SetBkColor(hdc, (uint)Win32.RGB(rightColor));
			rc.left = bounds.Right - rightWidth;
			rc.right = bounds.Right;
			Win32.ExtTextOut(hdc, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);
			// Bottom side
			Win32.SetBkColor(hdc, (uint)Win32.RGB(bottomColor));
			rc.left = bounds.Left;
			rc.top = bounds.Bottom - bottomWidth;
			rc.bottom = bounds.Bottom;
			Win32.ExtTextOut(hdc, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);

			graphics.ReleaseHdc(hdc);
#endif
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

					Color2HBS(color, out hue, out brightness, out saturation);

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
								colorGrade=HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
								break;
							}
						
							case Border3DSide.Right: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
								break;
							}

							case Border3DSide.Top: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
								break;
							}
					
							case Border3DSide.Bottom: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
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

					Color2HBS(color, out hue, out brightness, out saturation);

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
								colorGrade=HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
								break;
							}
						
							case Border3DSide.Right: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
								break;
							}

							case Border3DSide.Top: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
								break;
							}
					
							case Border3DSide.Bottom: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
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
		
		public static void DrawBorder3D(Graphics graphics, Rectangle rectangle) {
			DrawBorder3D(graphics, rectangle, Border3DStyle.Etched, Border3DSide.All);
		}
		
		public static void DrawBorder3D(Graphics graphics, Rectangle rectangle, Border3DStyle style) {
			DrawBorder3D(graphics, rectangle, style, Border3DSide.All);
		}
		
		public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height) {
			DrawBorder3D(graphics, new Rectangle(x, y, width, height), Border3DStyle.Etched, Border3DSide.All);
		}

		public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height, Border3DStyle style) {
			DrawBorder3D(graphics, new Rectangle(x, y, width, height), style, Border3DSide.All);
		}

		public static void DrawBorder3D( Graphics graphics, int x, int y, int width, int height, Border3DStyle style,Border3DSide sides) {
			DrawBorder3D( graphics, new Rectangle(x, y, width, height), style, sides);
		}

		public static void DrawBorder3D( Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides) {
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
			penTopLeft=penShadow;
			penTopLeftInner=penShadow;
			penBottomRight=penShadow;
			penBottomRightInner=penShadow;

			if ((style & Border3DStyle.RaisedOuter)!=0) {
				penTopLeft=penLight;
				penBottomRight=penDarkShadow;
				if ((style & (Border3DStyle.RaisedInner | Border3DStyle.SunkenInner))!=0) {
					doInner=true;
				}
			} else if ((style & Border3DStyle.SunkenOuter)!=0) {
				penTopLeft=penShadow;
				penBottomRight=penHighlight;
				if ((style & (Border3DStyle.RaisedInner | Border3DStyle.SunkenInner))!=0) {
					doInner=true;
				}
			}

			if ((style & Border3DStyle.RaisedInner)!=0) {
				if (doInner) {
					penTopLeftInner=penHighlight;
					penBottomRightInner=penShadow;
				} else {
					penTopLeft=penHighlight;
					penBottomRight=penShadow;
				}
			} else if ((style & Border3DStyle.SunkenInner)!=0) {
				if (doInner) {
					penTopLeftInner=penDarkShadow;
					penBottomRightInner=penLight;
				} else {
					penTopLeft=penDarkShadow;
					penBottomRight=penLight;
				}
			}

			if ((sides & Border3DSide.Middle)!=0) {
				SolidBrush	sb = new SolidBrush(colorSurface);
				graphics.FillRectangle(sb, rect);
				sb.Dispose();
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

		[MonoTODO]
		public static void DrawButton( Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControl( graphics, rectangle, (uint)DrawFrameControlTypes.DFC_BUTTON,
				(uint)state | (uint)DrawFrameControlStates.DFCS_BUTTONPUSH);
		}

		[MonoTODO]
		public static void DrawButton( Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawButton( graphics, new Rectangle(x, y, width, height), state);
		}

		[MonoTODO]
		public static void DrawCaptionButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			CaptionButton button,
			ButtonState state) {
			//FIXME:
		}

		public static void DrawCheckBox( Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControl (graphics, rectangle, (uint)DrawFrameControlTypes.DFC_BUTTON, (uint)state | (uint)DrawFrameControlStates.DFCS_BUTTONCHECK);
		}
		
		[MonoTODO]
		public static void DrawCaptionButton( Graphics graphics, Rectangle rectangle,CaptionButton button, ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawCheckBox(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawCheckBox(graphics, new Rectangle(x, y, width, height), state);
		}
		
		[MonoTODO]
		public static void DrawComboButton(
			Graphics graphics,
			Rectangle rectangle,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawComboButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawContainerGrabHandle(Graphics graphics,Rectangle bounds) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawFocusRectangle( Graphics graphics, Rectangle rectangle) {
			RECT rc = new RECT();
			rc.left = rectangle.Left;
			rc.top = rectangle.Top;
			rc.right = rectangle.Right;
			rc.bottom = rectangle.Bottom;
			IntPtr hdc = graphics.GetHdc();
			int res = Win32.DrawFocusRect( hdc, ref rc);
			graphics.ReleaseHdc(hdc);
		}
		
		[MonoTODO]
		public static void DrawFocusRectangle( Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor) {
			//FIXME: what to do with colors ?
			DrawFocusRectangle( graphics, rectangle);			
		}
		
		[MonoTODO]
		public static void DrawGrabHandle(
			Graphics graphics,
			Rectangle rectangle,
			bool primary,
			bool enabled) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawGrid(
			Graphics graphics,
			Rectangle area,
			Size pixelsBetweenDots,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawImageDisabled(
			Graphics graphics,
			Image image,
			int x,
			int y,
			Color background) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawLockedFrame(
			Graphics graphics,
			Rectangle rectangle,
			bool primary) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMenuGlyph(
			Graphics graphics,
			Rectangle rectangle,
			MenuGlyph glyph) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMenuGlyph(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			MenuGlyph glyph) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMixedCheckBox(
			Graphics graphics,
			Rectangle rectangle,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMixedCheckBox(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}

		internal static void CopyImageTransparent (IntPtr targetDC, IntPtr sourceDC, Rectangle rectangle, Color transparentColor) {
			// Monochrome mask
			IntPtr maskDC = Win32.CreateCompatibleDC (sourceDC);
			IntPtr maskBmp = Win32.CreateBitmap (rectangle.Width, rectangle.Height, 1, 1, IntPtr.Zero);
			IntPtr oldMaskBmp = Win32.SelectObject (maskDC, maskBmp);

			uint oldColor = Win32.SetBkColor (sourceDC, (uint)Win32.RGB (transparentColor));
			Win32.StretchBlt (maskDC, 0, 0, rectangle.Width, rectangle.Height, sourceDC, 
				0, 0, rectangle.Width, rectangle.Height, PatBltTypes.SRCCOPY);
			Win32.SetBkColor (sourceDC, oldColor);

			Win32.StretchBlt (targetDC, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
				sourceDC, 0, 0, rectangle.Width, rectangle.Height, PatBltTypes.SRCINVERT);

			uint oldBkClr = Win32.SetBkColor (targetDC, 0xFFFFFF);
			int oldTextClr = Win32.SetTextColor (targetDC, 0);
			Win32.StretchBlt (targetDC, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
				maskDC, 0, 0, rectangle.Width, rectangle.Height, PatBltTypes.SRCAND);
			Win32.SetTextColor (targetDC, oldTextClr);
			Win32.SetBkColor (targetDC, oldBkClr);

			Win32.StretchBlt (targetDC, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
				sourceDC, 0, 0, rectangle.Width, rectangle.Height, PatBltTypes.SRCINVERT);

			Win32.SelectObject (maskDC, oldMaskBmp);
			Win32.DeleteDC (maskDC);
			Win32.DeleteObject (maskBmp);
		}

		internal static void DrawFrameControl(Graphics graphics, Rectangle rectangle, uint Type, uint State) {
			switch(Type) {
				case (uint)DrawFrameControlTypes.DFC_BUTTON: {
					graphics.DrawLine(penHighlight, rectangle.Left, rectangle.Y, rectangle.Right, rectangle.Y);
					break;
				}

				case (uint)DrawFrameControlTypes.DFC_CAPTION: {
					break;
				}

				case (uint)DrawFrameControlTypes.DFC_MENU: {
					break;
				}

				case (uint)DrawFrameControlTypes.DFC_SCROLL: {
					break;
				}
			}
		}
	
		internal static void DrawFrameControlHelper (Graphics graphics, Rectangle rectangle, uint type, uint state) {

			IntPtr targetDC = graphics.GetHdc ();
			Bitmap bmp = new Bitmap (rectangle.Width, rectangle.Height);
			Graphics g = Graphics.FromImage (bmp);

			IntPtr memDC = g.GetHdc ();

			RECT rc = new RECT();
			rc.left = 0;
			rc.top = 0;
			rc.right = rectangle.Width;
			rc.bottom = rectangle.Height;

			Color transparentColor = Color.FromArgb (0, 0, 1);
			uint oldBk = Win32.SetBkColor (memDC, (uint)Win32.RGB(transparentColor));
			Win32.ExtTextOut (memDC, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);
			Win32.SetBkColor (memDC, oldBk);

			int res = Win32.DrawFrameControl( memDC, ref rc, type, state);

			CopyImageTransparent (targetDC, memDC, rectangle, transparentColor);

			g.ReleaseHdc(memDC);
			g.Dispose();
			bmp.Dispose();
			graphics.ReleaseHdc (targetDC);
		}

		public static void DrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControl (graphics,  rectangle, (uint)DrawFrameControlTypes.DFC_BUTTON, (uint)state | (uint)DrawFrameControlStates.DFCS_BUTTONRADIO);
		}
		
		[MonoTODO]
		public static void DrawRadioButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			DrawRadioButton(graphics, new Rectangle(x, y, width, height), state);
		}
		
		[MonoTODO]
		public static void DrawReversibleFrame(
			Rectangle rectangle,
			Color backColor,
			FrameStyle style) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawReversibleLine(
			Point start,
			Point end,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawScrollButton(
			Graphics graphics,
			Rectangle rectangle,
			ScrollButton button,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawScrollButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ScrollButton button,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSelectionFrame(
			Graphics graphics,
			bool active,
			Rectangle outsideRect,
			Rectangle insideRect,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSizeGrip(
			Graphics graphics,
			Color backColor,
			Rectangle bounds) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSizeGrip(
			Graphics graphics,
			Color backColor,
			int x,
			int y,
			int width,
			int height) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawStringDisabled(Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle, StringFormat format) {
			Rectangle rect = new Rectangle((int)layoutRectangle.Left, (int)layoutRectangle.Top, (int)layoutRectangle.Width, (int)layoutRectangle.Height);
			RECT rc = new RECT();
			
			rect.Offset(1,1);
			rc.left = rect.Left;
			rc.top = rect.Top;
			rc.right = rect.Right;
			rc.bottom = rect.Bottom;
			
			IntPtr hdc = graphics.GetHdc();
			
			int prevColor = Win32.SetTextColor(hdc, Win32.GetSysColor(GetSysColorIndex.COLOR_3DHILIGHT));
			BackgroundMode prevBkMode = Win32.SetBkMode(hdc, BackgroundMode.TRANSPARENT);
			IntPtr prevFont = Win32.SelectObject(hdc, font.ToHfont());
			
			Win32.DrawText(hdc, s, s.Length, ref rc, Win32.StringFormat2DrawTextFormat(format));
			
			rect.Offset(-1,-1);
			rc.left = rect.Left;
			rc.top = rect.Top;
			rc.right = rect.Right;
			rc.bottom = rect.Bottom;
			Win32.SetTextColor(hdc, Win32.GetSysColor(GetSysColorIndex.COLOR_3DSHADOW));
			Win32.DrawText(hdc, s, s.Length, ref rc,  Win32.StringFormat2DrawTextFormat(format));
			
			Win32.SelectObject(hdc, prevFont);
			Win32.SetBkMode(hdc, prevBkMode);
			Win32.SetTextColor(hdc, prevColor);
			
			graphics.ReleaseHdc(hdc);
		}
		
		[MonoTODO]
		public static void FillReversibleRectangle(
			Rectangle rectangle,
			Color backColor) {
			//FIXME:
		}
		
		public static Color Light(Color baseColor) {
			return Light( baseColor, 10.0f);
		}
		
		public static Color Light(Color baseColor,float percOfLightLight) {
			int H, I, S;

			ControlPaint.Color2HBS(baseColor, out H, out I, out S);
			int NewIntensity = Math.Min( 255, I + ((255*(int)percOfLightLight)/100));
			return ControlPaint.HBS2Color(H, NewIntensity, S);
		}

		public static Color LightLight(Color baseColor) {
			return Light( baseColor, 20.0f);
		}
		#endregion
	}
}
