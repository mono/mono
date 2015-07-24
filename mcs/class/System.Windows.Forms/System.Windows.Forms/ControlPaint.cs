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
//	Peter Bartok	pbartok@novell.com
//


// NOT COMPLETE

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Windows.Forms {
	public sealed class ControlPaint {
		#region Local Variables
		static int		RGBMax=255;
		static int		HLSMax=255;
		#endregion	// Local Variables

		#region Private Enumerations


		#region Constructor
		// Prevent a public constructor from being created
		private ControlPaint() {
		}
		#endregion	// Constructor


		#endregion	// Private Enumerations

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

		internal static Color HBS2Color(int hue, int lum, int sat) {
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
			return (Color.FromArgb(R, G, B));
		}
		#endregion	// Helpers

		#region Public Static Properties
		public static Color ContrastControlDark {
			get { return(SystemColors.ControlDark); }
		}
		#endregion	// Public Static Properties

		#region Public Static Methods
		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		public static IntPtr CreateHBitmap16Bit (Bitmap bitmap, Color background)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		public static IntPtr CreateHBitmapColorMask (Bitmap bitmap, IntPtr monochromeMask)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		public static IntPtr CreateHBitmapTransparencyMask (Bitmap bitmap)
		{
			throw new NotImplementedException ();
		}

		public static Color Light(Color baseColor) {
			return Light(baseColor, 0.5f);
		}

		public static Color Light (Color baseColor, float percOfLightLight)
		{
			if (baseColor.ToArgb () == ThemeEngine.Current.ColorControl.ToArgb ()) {
				int r_sub, g_sub, b_sub;
				Color color;

				if (percOfLightLight <= 0f)
					return ThemeEngine.Current.ColorControlLight;

				if (percOfLightLight == 1.0f)
					return ThemeEngine.Current.ColorControlLightLight;
				
				r_sub = ThemeEngine.Current.ColorControlLightLight.R - ThemeEngine.Current.ColorControlLight.R;
				g_sub = ThemeEngine.Current.ColorControlLightLight.G - ThemeEngine.Current.ColorControlLight.G;
				b_sub = ThemeEngine.Current.ColorControlLightLight.B - ThemeEngine.Current.ColorControlLight.B;

				color = Color.FromArgb (ThemeEngine.Current.ColorControlLight.A,
						(int) (ThemeEngine.Current.ColorControlLight.R + (r_sub * percOfLightLight)),
						(int) (ThemeEngine.Current.ColorControlLight.G + (g_sub * percOfLightLight)),
						(int) (ThemeEngine.Current.ColorControlLight.B + (b_sub * percOfLightLight)));
				return color;
 			}
			
			int H, I, S;

			ControlPaint.Color2HBS (baseColor, out H, out I, out S);
			int NewIntensity = Math.Min (255, I + (int) ((255 - I) * 0.5f * percOfLightLight));
			
			return ControlPaint.HBS2Color (H, NewIntensity, S);
		}

		public static Color LightLight (Color baseColor)
		{
			return Light(baseColor, 1.0f);
		}

		public static Color Dark (Color baseColor)
		{
			return Dark(baseColor, 0.5f);
		}

		public static Color Dark (Color baseColor, float percOfDarkDark)
		{
			if (baseColor.ToArgb () == ThemeEngine.Current.ColorControl.ToArgb ()) {
				
				int r_sub, g_sub, b_sub;
				Color color;

				if (percOfDarkDark <= 0f)
					return ThemeEngine.Current.ColorControlDark;

				if (percOfDarkDark == 1.0f)
					return ThemeEngine.Current.ColorControlDarkDark;

				r_sub = ThemeEngine.Current.ColorControlDarkDark.R - ThemeEngine.Current.ColorControlDark.R;
				g_sub = ThemeEngine.Current.ColorControlDarkDark.G - ThemeEngine.Current.ColorControlDark.G;
				b_sub = ThemeEngine.Current.ColorControlDarkDark.B - ThemeEngine.Current.ColorControlDark.B;

				color = Color.FromArgb (ThemeEngine.Current.ColorControlDark.A,
						(int) (ThemeEngine.Current.ColorControlDark.R + (r_sub * percOfDarkDark)),
						(int) (ThemeEngine.Current.ColorControlDark.G + (g_sub * percOfDarkDark)),
						(int) (ThemeEngine.Current.ColorControlDark.B + (b_sub * percOfDarkDark)));
				return color;
 			}
		
			int H, I, S;

			ControlPaint.Color2HBS(baseColor, out H, out I, out S);
			int PreIntensity = Math.Max (0, I - (int) (I * 0.333f));
			int NewIntensity = Math.Max (0, PreIntensity - (int) (PreIntensity * percOfDarkDark));
			return ControlPaint.HBS2Color(H, NewIntensity, S);
		}

		public static Color DarkDark (Color baseColor)
		{
			return Dark(baseColor, 1.0f);
		}

		public static void DrawBorder (Graphics graphics, Rectangle bounds, Color color, ButtonBorderStyle style)
		{
			int line_width_top_left = 1;
			int line_width_bottom_right = 1;
			
			if (style == ButtonBorderStyle.Inset)
				line_width_top_left = 2;
			if (style == ButtonBorderStyle.Outset) {
				line_width_bottom_right = 2;
				line_width_top_left = 2;
			}
			
			DrawBorder(graphics, bounds, color, line_width_top_left, style, color, line_width_top_left, style, color, line_width_bottom_right, style, color, line_width_bottom_right, style);
		}

		internal static void DrawBorder (Graphics graphics, RectangleF bounds, Color color, ButtonBorderStyle style)
		{
			int line_width_top_left = 1;
			int line_width_bottom_right = 1;
			
			if (style == ButtonBorderStyle.Inset)
				line_width_top_left = 2;
			if (style == ButtonBorderStyle.Outset) {
				line_width_bottom_right = 2;
				line_width_top_left = 2;
			}
			
			ThemeEngine.Current.CPDrawBorder (graphics, bounds, color, line_width_top_left, style, color, line_width_top_left, style, color, line_width_bottom_right, style, color, line_width_bottom_right, style);
		}

		public static void DrawBorder( Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor, int bottomWidth,
			ButtonBorderStyle bottomStyle) {

			ThemeEngine.Current.CPDrawBorder (graphics, bounds, leftColor, leftWidth,
				leftStyle, topColor, topWidth, topStyle, rightColor, rightWidth, rightStyle,
				bottomColor, bottomWidth, bottomStyle);
		}


		public static void DrawBorder3D(Graphics graphics, Rectangle rectangle) {
			DrawBorder3D(graphics, rectangle, Border3DStyle.Etched, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
		}

		public static void DrawBorder3D(Graphics graphics, Rectangle rectangle, Border3DStyle style) {
			DrawBorder3D(graphics, rectangle, style, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
		}

		public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height) {
			DrawBorder3D(graphics, new Rectangle(x, y, width, height), Border3DStyle.Etched, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
		}

		public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height, Border3DStyle style) {
			DrawBorder3D(graphics, new Rectangle(x, y, width, height), style, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
		}

		public static void DrawBorder3D( Graphics graphics, int x, int y, int width, int height, Border3DStyle style,Border3DSide sides) {
			DrawBorder3D( graphics, new Rectangle(x, y, width, height), style, sides);
		}

		public static void DrawBorder3D( Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides) {

			ThemeEngine.Current.CPDrawBorder3D (graphics, rectangle, style, sides);
		}

		public static void DrawButton( Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawButton(graphics, new Rectangle(x, y, width, height), state);
		}

		public static void DrawButton( Graphics graphics, Rectangle rectangle, ButtonState state) {

			ThemeEngine.Current.CPDrawButton (graphics, rectangle, state);
		}


		public static void DrawCaptionButton(Graphics graphics, int x, int y, int width, int height, CaptionButton button, ButtonState state) {
			DrawCaptionButton(graphics, new Rectangle(x, y, width, height), button, state);
		}

		public static void DrawCaptionButton(Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state) {

			ThemeEngine.Current.CPDrawCaptionButton (graphics, rectangle, button, state);
		}

		public static void DrawCheckBox(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawCheckBox(graphics, new Rectangle(x, y, width, height), state);
		}

		public static void DrawCheckBox(Graphics graphics, Rectangle rectangle, ButtonState state) {

			ThemeEngine.Current.CPDrawCheckBox (graphics, rectangle, state);
		}

		public static void DrawComboButton(Graphics graphics, Rectangle rectangle, ButtonState state) {

			ThemeEngine.Current.CPDrawComboButton (graphics, rectangle,  state);
		}

		public static void DrawComboButton(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawComboButton(graphics, new Rectangle(x, y, width, height), state);
		}

		public static void DrawContainerGrabHandle(Graphics graphics, Rectangle bounds) {

			ThemeEngine.Current.CPDrawContainerGrabHandle (graphics, bounds);
		}

		public static void DrawFocusRectangle( Graphics graphics, Rectangle rectangle) {
			DrawFocusRectangle(graphics, rectangle, SystemColors.Control, SystemColors.ControlText);
		}

		public static void DrawFocusRectangle( Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor) {

			ThemeEngine.Current.CPDrawFocusRectangle (graphics, rectangle, foreColor, backColor);
		}

		public static void DrawGrabHandle(Graphics graphics, Rectangle rectangle, bool primary, bool enabled) {

			ThemeEngine.Current.CPDrawGrabHandle (graphics, rectangle, primary, enabled);
		}

		public static void DrawGrid(Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor) {

			ThemeEngine.Current.CPDrawGrid (graphics, area, pixelsBetweenDots, backColor);
		}

		public static void DrawImageDisabled(Graphics graphics, Image image, int x, int y, Color background) {

			ThemeEngine.Current.CPDrawImageDisabled (graphics, image, x, y, background);
		}

		public static void DrawLockedFrame(Graphics graphics, Rectangle rectangle, bool primary) {

			ThemeEngine.Current.CPDrawLockedFrame (graphics, rectangle, primary);
		}

		public static void DrawMenuGlyph(Graphics graphics, Rectangle rectangle, MenuGlyph glyph) {

			ThemeEngine.Current.CPDrawMenuGlyph (graphics, rectangle, glyph, ThemeEngine.Current.ColorMenuText, Color.Empty);
		}

		public static void DrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph, Color foreColor, Color backColor)
		{
			ThemeEngine.Current.CPDrawMenuGlyph (graphics, rectangle, glyph, foreColor, backColor);
		}
	
		public static void DrawMenuGlyph(Graphics graphics, int x, int y, int width, int height, MenuGlyph glyph) {
			DrawMenuGlyph(graphics, new Rectangle(x, y, width, height), glyph);
		}

		public static void DrawMenuGlyph (Graphics graphics, int x, int y, int width, int height, MenuGlyph glyph, Color foreColor, Color backColor)
		{
			DrawMenuGlyph (graphics, new Rectangle (x, y, width, height), glyph, foreColor, backColor);
		}

		public static void DrawMixedCheckBox(Graphics graphics, Rectangle rectangle, ButtonState state) {
			ThemeEngine.Current.CPDrawMixedCheckBox (graphics, rectangle, state);
		}

		public static void DrawMixedCheckBox(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawMixedCheckBox(graphics, new Rectangle(x, y, width, height), state);
		}


		public static void DrawRadioButton(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawRadioButton(graphics, new Rectangle(x, y, width, height), state);
		}

		public static void DrawRadioButton(Graphics graphics, Rectangle rectangle, ButtonState state) {

			ThemeEngine.Current.CPDrawRadioButton (graphics, rectangle, state);
		}

		public static void DrawReversibleFrame(Rectangle rectangle, Color backColor, FrameStyle style) {
			XplatUI.DrawReversibleFrame (rectangle, backColor, style);
		}

		public static void DrawReversibleLine(Point start, Point end, Color backColor) {
			XplatUI.DrawReversibleLine (start, end, backColor);
		}

		public static void FillReversibleRectangle(Rectangle rectangle, Color backColor) {
			XplatUI.FillReversibleRectangle (rectangle, backColor);
		}

		public static void DrawScrollButton (Graphics graphics, int x, int y, int width, int height, ScrollButton button, ButtonState state) {
			ThemeEngine.Current.CPDrawScrollButton (graphics, new Rectangle(x, y, width, height), button, state);
		}

		public static void DrawScrollButton (Graphics graphics, Rectangle rectangle, ScrollButton button, ButtonState state) {
			ThemeEngine.Current.CPDrawScrollButton (graphics, rectangle, button, state);
		}

		[MonoTODO ("Stub, does nothing")]
		private static bool DSFNotImpl = false;
		public static void DrawSelectionFrame(Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect, Color backColor) {
			if (!DSFNotImpl) {
				DSFNotImpl = true;
				Console.WriteLine("NOT IMPLEMENTED: DrawSelectionFrame(Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect, Color backColor)");
			}
			//throw new NotImplementedException();
		}

		public static void DrawSizeGrip (Graphics graphics, Color backColor, Rectangle bounds)
		{
			ThemeEngine.Current.CPDrawSizeGrip (graphics,  backColor,  bounds);
		}

		public static void DrawSizeGrip(Graphics graphics, Color backColor, int x, int y, int width, int height) {
			DrawSizeGrip(graphics, backColor, new Rectangle(x, y, width, height));
		}

		public static void DrawStringDisabled(Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle, StringFormat format) {

			ThemeEngine.Current.CPDrawStringDisabled (graphics, s, font, color, layoutRectangle, format);
		}

		public static void DrawStringDisabled (IDeviceContext dc, string s, Font font, Color color, Rectangle layoutRectangle, TextFormatFlags format)
		{
			ThemeEngine.Current.CPDrawStringDisabled (dc, s, font, color, layoutRectangle, format);
		}
		
		public static void DrawVisualStyleBorder (Graphics graphics, Rectangle bounds)
		{
			ThemeEngine.Current.CPDrawVisualStyleBorder (graphics, bounds);
		}
		#endregion	// Public Static Methods
	}
}
