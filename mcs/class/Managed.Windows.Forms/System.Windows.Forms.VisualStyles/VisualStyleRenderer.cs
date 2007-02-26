//
// VisualStyleRenderer.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System.Drawing;

namespace System.Windows.Forms.VisualStyles
{
	public sealed class VisualStyleRenderer
	{
		private string class_name;
		private int part;
		private int state;
		private IntPtr theme;
		private int last_hresult = 0;

		#region Public Constructors
		public VisualStyleRenderer (string className, int part, int state)
		{
			this.SetParameters (className, part, state);
		}

		public VisualStyleRenderer (VisualStyleElement element)
		{
			this.SetParameters (element);
		}
		#endregion

		#region Public Properties
		public String Class { get { return this.class_name; } }
		public IntPtr Handle { get { return this.theme; } }
		public int LastHResult { get { return this.last_hresult; } }
		public int Part { get { return this.part; } }
		public int State { get { return this.state; } }
		
		public static bool IsSupported {
			get {
				if (!VisualStyleInformation.IsEnabledByUser) 
					return false;
				
				if (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled ||
					Application.VisualStyleState == VisualStyleState.ClientAreaEnabled)
						return true;
						
				return false;
			}
		}
		#endregion

		#region Public Static Methods
		public static bool IsElementDefined (VisualStyleElement element)
		{
			if (!IsSupported)
				throw new InvalidOperationException ("Visual Styles are not enabled.");

			IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, element.ClassName);
			bool retval = UXTheme.IsThemePartDefined (theme, element.Part, 0);
			UXTheme.CloseThemeData (theme);

			return retval;
		}
		#endregion

		#region Public Instance Methods
		public void DrawBackground (IDeviceContext dc, Rectangle bounds)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);

			last_hresult = UXTheme.DrawThemeBackground (theme, dc.GetHdc (), this.part, this.state, ref BoundsRect, IntPtr.Zero);
			dc.ReleaseHdc ();
		}

		public void DrawBackground (IDeviceContext dc, Rectangle bounds, Rectangle clipRectangle)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			XplatUIWin32.RECT ClipRect = XplatUIWin32.RECT.FromRectangle (clipRectangle);

			last_hresult = UXTheme.DrawThemeBackground (theme, dc.GetHdc (), this.part, this.state, ref BoundsRect, ref ClipRect);
			dc.ReleaseHdc ();
		}

		public Rectangle DrawEdge (IDeviceContext dc, Rectangle bounds, Edges edges, EdgeStyle style, EdgeEffects effects)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			XplatUIWin32.RECT retval;

			last_hresult = UXTheme.DrawThemeEdge (theme, dc.GetHdc (), this.part, this.state, ref BoundsRect, (uint)style, (uint)edges + (uint)effects, out retval);
			dc.ReleaseHdc ();
			return retval.ToRectangle ();
		}

		public void DrawImage (Graphics g, Rectangle bounds, ImageList imageList, int imageIndex)
		{
			if (g == null)
				throw new ArgumentNullException ("g");
			if (imageIndex < 0 || imageIndex > imageList.Images.Count - 1)
				throw new ArgumentOutOfRangeException ("imageIndex");
			if (imageList.Images[imageIndex] == null)
				throw new ArgumentNullException ("imageIndex");

			g.DrawImage (imageList.Images[imageIndex], bounds);
		}

		public void DrawImage (Graphics g, Rectangle bounds, Image image)
		{
			if (g == null)
				throw new ArgumentNullException ("g");
			if (image == null)
				throw new ArgumentNullException ("image");

			g.DrawImage (image, bounds);
		}

		public void DrawParentBackground (IDeviceContext dc, Rectangle bounds, Control childControl)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);

			using (Graphics g = Graphics.FromHwnd (childControl.Handle)) {
				last_hresult = UXTheme.DrawThemeParentBackground (childControl.Handle, g.GetHdc (), ref BoundsRect);
				g.ReleaseHdc ();
			}
		}

		public void DrawText (IDeviceContext dc, Rectangle bounds, string textToDraw, bool drawDisabled, TextFormatFlags flags)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);

			last_hresult = UXTheme.DrawThemeText (theme, dc.GetHdc (), this.part, this.state, textToDraw, textToDraw.Length, (uint)flags, 0, ref BoundsRect);
			dc.ReleaseHdc ();
		}

		public void DrawText (IDeviceContext dc, Rectangle bounds, string textToDraw, bool drawDisabled)
		{
			this.DrawText (dc, bounds, textToDraw, drawDisabled, TextFormatFlags.Default);
		}

		public void DrawText (IDeviceContext dc, Rectangle bounds, string textToDraw)
		{
			this.DrawText (dc, bounds, textToDraw, false, TextFormatFlags.Default);
		}

		public Rectangle GetBackgroundContentRectangle (IDeviceContext dc, Rectangle bounds)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			XplatUIWin32.RECT retval;

			last_hresult = UXTheme.GetThemeBackgroundContentRect (theme, dc.GetHdc (), this.part, this.state, ref BoundsRect, out retval);
			dc.ReleaseHdc ();

			return retval.ToRectangle ();
		}

		public Rectangle GetBackgroundExtent (IDeviceContext dc, Rectangle contentBounds)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (contentBounds);
			XplatUIWin32.RECT retval = new XplatUIWin32.RECT ();

			last_hresult = UXTheme.GetThemeBackgroundExtent (theme, dc.GetHdc (), this.part, this.state, ref BoundsRect, ref retval);
			dc.ReleaseHdc ();

			return retval.ToRectangle ();
		}

		[System.Security.SuppressUnmanagedCodeSecurity]
		public Region GetBackgroundRegion (IDeviceContext dc, Rectangle contentBounds)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (contentBounds);
			IntPtr retval;

			last_hresult = UXTheme.GetThemeBackgroundRegion (theme, dc.GetHdc (), this.part, this.state, ref BoundsRect, out retval);
			dc.ReleaseHdc ();

			return Region.FromHrgn (retval);
		}

		public bool GetBoolean (BooleanProperty prop)
		{
			if (!Enum.IsDefined (typeof (BooleanProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (BooleanProperty));

			int retval;
			last_hresult = UXTheme.GetThemeBool (theme, this.part, this.state, (int)prop, out retval);

			return retval == 0 ? false : true;
		}

		public Color GetColor (ColorProperty prop)
		{
			if (!Enum.IsDefined (typeof (ColorProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (ColorProperty));

			int retval;
			last_hresult = UXTheme.GetThemeColor (theme, this.part, this.state, (int)prop, out retval);

			return System.Drawing.Color.FromArgb ((int)(0x000000FFU & retval),
			     (int)(0x0000FF00U & retval) >> 8, (int)(0x00FF0000U & retval) >> 16);
		}
		
		public int GetEnumValue (EnumProperty prop)
		{
			if (!Enum.IsDefined (typeof (EnumProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (EnumProperty));

			int retval;
			last_hresult = UXTheme.GetThemeEnumValue (theme, this.part, this.state, (int)prop, out retval);

			return retval;
		}
		
		public string GetFilename (FilenameProperty prop)
		{
			if (!Enum.IsDefined (typeof (FilenameProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (FilenameProperty));

			Text.StringBuilder sb = new Text.StringBuilder (255);
			last_hresult = UXTheme.GetThemeFilename (theme, this.part, this.state, (int)prop, sb, sb.Capacity);

			return sb.ToString ();
		}
		
		[MonoTODO(@"I can't get MS's to return anything but null, so I can't really get this one right")]
		public Font GetFont (IDeviceContext dc, FontProperty prop)
		{
			throw new NotImplementedException();
			//if (dc == null)
			//        throw new ArgumentNullException ("dc");
			//if (!Enum.IsDefined (typeof (FontProperty), prop))
			//        throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (FontProperty));

			//UXTheme.LOGFONT lf = new UXTheme.LOGFONT();

			//UXTheme.GetThemeFont (theme, dc.GetHdc (), this.part, this.state, (int)prop, out lf);
			//IntPtr fontPtr = UXTheme.CreateFontIndirect(lf);
			//dc.ReleaseHdc();

			//return Font.FromLogFont(lf);
			//return null;
		}
		
		public int GetInteger (IntegerProperty prop)
		{
			if (!Enum.IsDefined (typeof (IntegerProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (IntegerProperty));

			int retval;
			last_hresult = UXTheme.GetThemeInt (theme, this.part, this.state, (int)prop, out retval);

			return retval;
		}
		
		[MonoTODO(@"MS's causes a PInvokeStackUnbalance on me, so this is not verified against MS.")]
		public Padding GetMargins (IDeviceContext dc, MarginProperty prop)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");
			if (!Enum.IsDefined (typeof (MarginProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (MarginProperty));


			UXTheme.MARGINS retval = new UXTheme.MARGINS ();
			XplatUIWin32.RECT BoundsRect;

			last_hresult = UXTheme.GetThemeMargins (theme, dc.GetHdc (), this.part, this.state, (int)prop, out BoundsRect, out retval);
			dc.ReleaseHdc ();

			return retval.ToPadding();
		}
		
		public Size GetPartSize (IDeviceContext dc, Rectangle bounds, ThemeSizeType type)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");
			if (!Enum.IsDefined (typeof (ThemeSizeType), type))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)type, typeof (ThemeSizeType));

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			UXTheme.SIZE retval;

			last_hresult = UXTheme.GetThemePartSize (theme, dc.GetHdc (), this.part, this.state, ref BoundsRect, (int)type, out retval);
			dc.ReleaseHdc ();

			return retval.ToSize();
		}

		public Size GetPartSize (IDeviceContext dc, ThemeSizeType type)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");
			if (!Enum.IsDefined (typeof (ThemeSizeType), type))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)type, typeof (ThemeSizeType));

			UXTheme.SIZE retval;

			last_hresult = UXTheme.GetThemePartSize (theme, dc.GetHdc (), this.part, this.state, 0, (int)type, out retval);
			dc.ReleaseHdc ();

			return retval.ToSize ();
		}

		public Point GetPoint (PointProperty prop)
		{
			if (!Enum.IsDefined (typeof (PointProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (PointProperty));

			POINT retval;
			last_hresult = UXTheme.GetThemePosition (theme, this.part, this.state, (int)prop, out retval);

			return retval.ToPoint();
		}
		
		[MonoTODO(@"Can't find any values that return anything on MS to test against")]
		public string GetString (StringProperty prop)
		{
			if (!Enum.IsDefined (typeof (StringProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (StringProperty));

			Text.StringBuilder sb = new Text.StringBuilder (255);
			last_hresult = UXTheme.GetThemeString (theme, this.part, this.state, (int)prop, sb, sb.Capacity);

			return sb.ToString ();
		}
		
		public Rectangle GetTextExtent (IDeviceContext dc, Rectangle bounds, string textToDraw, TextFormatFlags flags)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			XplatUIWin32.RECT retval;
			
			last_hresult = UXTheme.GetThemeTextExtent (theme, dc.GetHdc (), this.part, this.state, textToDraw, textToDraw.Length, (int)flags, ref BoundsRect, out retval);
			dc.ReleaseHdc ();

			return retval.ToRectangle ();
		}

		public Rectangle GetTextExtent (IDeviceContext dc, string textToDraw, TextFormatFlags flags)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT retval;
			
			last_hresult = UXTheme.GetThemeTextExtent (theme, dc.GetHdc (), this.part, this.state, textToDraw, textToDraw.Length, (int)flags, 0, out retval);
			dc.ReleaseHdc ();

			return retval.ToRectangle ();
		}
		
		public TextMetrics GetTextMetrics (IDeviceContext dc)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc", "dc cannot be null.");

			XplatUIWin32.TEXTMETRIC metrics;
			
			last_hresult = UXTheme.GetThemeTextMetrics (theme, dc.GetHdc (), this.part, this.state, out metrics);
			dc.ReleaseHdc ();

			TextMetrics retval = new TextMetrics ();
			retval.Ascent = metrics.tmAscent;
			retval.AverageCharWidth = metrics.tmAveCharWidth;
			retval.BreakChar =(char)metrics.tmBreakChar;
			retval.CharSet = (TextMetricsCharacterSet)metrics.tmCharSet;
			retval.DefaultChar = (char)metrics.tmDefaultChar;
			retval.Descent = metrics.tmDescent;
			retval.DigitizedAspectX = metrics.tmDigitizedAspectX;
			retval.DigitizedAspectY = metrics.tmDigitizedAspectY;
			retval.ExternalLeading = metrics.tmExternalLeading;
			retval.FirstChar = (char)metrics.tmFirstChar;
			retval.Height = metrics.tmHeight;
			retval.InternalLeading = metrics.tmInternalLeading;
			retval.Italic = metrics.tmItalic == 0 ? false : true;
			retval.LastChar = (char)metrics.tmLastChar;
			retval.MaxCharWidth = metrics.tmMaxCharWidth;
			retval.Overhang = metrics.tmOverhang;
			retval.PitchAndFamily = (TextMetricsPitchAndFamilyValues)metrics.tmPitchAndFamily;
			retval.StruckOut = metrics.tmStruckOut == 0 ? false : true;
			retval.Underlined = metrics.tmUnderlined == 0 ? false : true;
			retval.Weight = metrics.tmWeight;

			return retval;
		}

		public HitTestCode HitTestBackground (IDeviceContext dc, Rectangle backgroundRectangle, IntPtr hRgn, Point pt, HitTestOptions options)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (backgroundRectangle);
			int retval;

			last_hresult = UXTheme.HitTestThemeBackground (theme, dc.GetHdc (), this.part, this.state, (uint)options, ref BoundsRect, hRgn, new POINT(pt.X, pt.Y), out retval);
			dc.ReleaseHdc ();

			return (HitTestCode)retval;
		}

		public HitTestCode HitTestBackground (Graphics g, Rectangle backgroundRectangle, Region region, Point pt, HitTestOptions options)
		{
			if (g == null)
				throw new ArgumentNullException ("g");

			IntPtr hRgn = region.GetHrgn(g);
			
			return this.HitTestBackground(g, backgroundRectangle, hRgn, pt, options);
		}

		public HitTestCode HitTestBackground (IDeviceContext dc, Rectangle backgroundRectangle, Point pt, HitTestOptions options)
		{
			return this.HitTestBackground (dc, backgroundRectangle, IntPtr.Zero, pt, options);
		}

		public bool IsBackgroundPartiallyTransparent ()
		{
			int retval = UXTheme.IsThemeBackgroundPartiallyTransparent (theme, this.part, this.state);

			return retval == 0 ? false : true;
		}

		public void SetParameters (string className, int part, int state)
		{
			if (theme != IntPtr.Zero)
				last_hresult = UXTheme.CloseThemeData (theme);

			if (!IsSupported)
				throw new InvalidOperationException ("Visual Styles are not enabled.");

			this.class_name = className;
			this.part = part;
			this.state = state;
			theme = UXTheme.OpenThemeData (IntPtr.Zero, this.class_name);

			if (!UXTheme.IsThemePartDefined (theme, this.part, 0))
				throw new ArgumentException ("This element is not supported by the current visual style.");
		}

		public void SetParameters (VisualStyleElement element)
		{
			this.SetParameters (element.ClassName, element.Part, element.State);
		}
		#endregion
	}
}
#endif