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
		private ThemeHandleManager theme_handle_manager = new ThemeHandleManager ();

		#region Public Constructors
		public VisualStyleRenderer (string className, int part, int state)
		{
			theme_handle_manager.VisualStyleRenderer = this;
			this.SetParameters (className, part, state);
		}

		public VisualStyleRenderer (VisualStyleElement element)
			: this (element.ClassName, element.Part, element.State) {
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

			if (IsElementKnownToBeSupported (element.ClassName, element.Part, element.State))
				return true;

			IntPtr theme = VisualStyles.UxThemeOpenThemeData (IntPtr.Zero, element.ClassName);
			if (theme == IntPtr.Zero)
				return false;
			bool retval = VisualStyles.UxThemeIsThemePartDefined (theme, element.Part);
			VisualStyles.UxThemeCloseThemeData (theme);

			return retval;
		}
		#endregion

		#region Public Instance Methods
		public void DrawBackground (IDeviceContext dc, Rectangle bounds)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			last_hresult = VisualStyles.UxThemeDrawThemeBackground (theme, dc, this.part, this.state, bounds);
		}

		public void DrawBackground (IDeviceContext dc, Rectangle bounds, Rectangle clipRectangle)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			last_hresult = VisualStyles.UxThemeDrawThemeBackground (theme, dc, this.part, this.state, bounds, clipRectangle);
		}

		public Rectangle DrawEdge (IDeviceContext dc, Rectangle bounds, Edges edges, EdgeStyle style, EdgeEffects effects)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");
			
			Rectangle result;
			last_hresult = VisualStyles.UxThemeDrawThemeEdge (theme, dc, this.part, this.state, bounds, edges, style, effects, out result);
			return result;
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

			last_hresult = VisualStyles.UxThemeDrawThemeParentBackground (dc, bounds, childControl);
		}

		public void DrawText (IDeviceContext dc, Rectangle bounds, string textToDraw, bool drawDisabled, TextFormatFlags flags)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			last_hresult = VisualStyles.UxThemeDrawThemeText (theme, dc, this.part, this.state, textToDraw, flags, bounds);
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

			Rectangle result;
			last_hresult = VisualStyles.UxThemeGetThemeBackgroundContentRect (theme, dc, this.part, this.state, bounds, out result);
			return result;
		}

		public Rectangle GetBackgroundExtent (IDeviceContext dc, Rectangle contentBounds)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			Rectangle result;
			last_hresult = VisualStyles.UxThemeGetThemeBackgroundExtent (theme, dc, this.part, this.state, contentBounds, out result);
			return result;
		}

		[System.Security.SuppressUnmanagedCodeSecurity]
		public Region GetBackgroundRegion (IDeviceContext dc, Rectangle bounds)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			Region result;
			last_hresult = VisualStyles.UxThemeGetThemeBackgroundRegion (theme, dc, this.part, this.state, bounds, out result);
			return result;
		}

		public bool GetBoolean (BooleanProperty prop)
		{
			if (!Enum.IsDefined (typeof (BooleanProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (BooleanProperty));

			bool result;
			last_hresult = VisualStyles.UxThemeGetThemeBool (theme, this.part, this.state, prop, out result);
			return result;
		}

		public Color GetColor (ColorProperty prop)
		{
			if (!Enum.IsDefined (typeof (ColorProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (ColorProperty));

			Color result;
			last_hresult = VisualStyles.UxThemeGetThemeColor (theme, this.part, this.state, prop, out result);
			return result;
		}
		
		public int GetEnumValue (EnumProperty prop)
		{
			if (!Enum.IsDefined (typeof (EnumProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (EnumProperty));

			int result;
			last_hresult = VisualStyles.UxThemeGetThemeEnumValue (theme, this.part, this.state, prop, out result);
			return result;
		}
		
		public string GetFilename (FilenameProperty prop)
		{
			if (!Enum.IsDefined (typeof (FilenameProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (FilenameProperty));

			string result;
			last_hresult = VisualStyles.UxThemeGetThemeFilename (theme, this.part, this.state, prop, out result);
			return result;

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

			int result;
			last_hresult = VisualStyles.UxThemeGetThemeInt (theme, this.part, this.state, prop, out result);
			return result;
		}
		
		[MonoTODO(@"MS's causes a PInvokeStackUnbalance on me, so this is not verified against MS.")]
		public Padding GetMargins (IDeviceContext dc, MarginProperty prop)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");
			if (!Enum.IsDefined (typeof (MarginProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (MarginProperty));

			Padding result;
			last_hresult = VisualStyles.UxThemeGetThemeMargins (theme, dc, this.part, this.state, prop, out result);
			return result;
		}
		
		public Size GetPartSize (IDeviceContext dc, Rectangle bounds, ThemeSizeType type)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");
			if (!Enum.IsDefined (typeof (ThemeSizeType), type))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)type, typeof (ThemeSizeType));

			Size result;
			last_hresult = VisualStyles.UxThemeGetThemePartSize (theme, dc, this.part, this.state, bounds, type, out result);
			return result;
		}

		public Size GetPartSize (IDeviceContext dc, ThemeSizeType type)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");
			if (!Enum.IsDefined (typeof (ThemeSizeType), type))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)type, typeof (ThemeSizeType));

			Size result;
			last_hresult = VisualStyles.UxThemeGetThemePartSize (theme, dc, this.part, this.state, type, out result);
			return result;
		}

		public Point GetPoint (PointProperty prop)
		{
			if (!Enum.IsDefined (typeof (PointProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (PointProperty));

			Point result;
			last_hresult = VisualStyles.UxThemeGetThemePosition (theme, this.part, this.state, prop, out result);
			return result;
		}
		
		[MonoTODO(@"Can't find any values that return anything on MS to test against")]
		public string GetString (StringProperty prop)
		{
			if (!Enum.IsDefined (typeof (StringProperty), prop))
				throw new System.ComponentModel.InvalidEnumArgumentException ("prop", (int)prop, typeof (StringProperty));

			string result;
			last_hresult = VisualStyles.UxThemeGetThemeString (theme, this.part, this.state, prop, out result);
			return result;
		}
		
		public Rectangle GetTextExtent (IDeviceContext dc, Rectangle bounds, string textToDraw, TextFormatFlags flags)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			Rectangle result;
			last_hresult = VisualStyles.UxThemeGetThemeTextExtent (theme, dc, this.part, this.state, textToDraw, flags, bounds, out result);
			return result;
		}

		public Rectangle GetTextExtent (IDeviceContext dc, string textToDraw, TextFormatFlags flags)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			Rectangle result;
			last_hresult = VisualStyles.UxThemeGetThemeTextExtent (theme, dc, this.part, this.state, textToDraw, flags, out result);
			return result;
		}
		
		public TextMetrics GetTextMetrics (IDeviceContext dc)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc", "dc cannot be null.");

			TextMetrics result;
			last_hresult = VisualStyles.UxThemeGetThemeTextMetrics (theme, dc, this.part, this.state, out result);
			return result;
		}

		public HitTestCode HitTestBackground (IDeviceContext dc, Rectangle backgroundRectangle, IntPtr hRgn, Point pt, HitTestOptions options)
		{
			if (dc == null)
				throw new ArgumentNullException ("dc");

			HitTestCode result;
			last_hresult = VisualStyles.UxThemeHitTestThemeBackground(theme, dc, this.part, this.state, options, backgroundRectangle, hRgn, pt, out result);
			return result;
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
			return VisualStyles.UxThemeIsThemeBackgroundPartiallyTransparent (theme, this.part, this.state);
		}

		public void SetParameters (string className, int part, int state)
		{
			if (theme != IntPtr.Zero)
				last_hresult = VisualStyles.UxThemeCloseThemeData (theme);

			if (!IsSupported)
				throw new InvalidOperationException ("Visual Styles are not enabled.");

			this.class_name = className;
			this.part = part;
			this.state = state;
			theme = VisualStyles.UxThemeOpenThemeData (IntPtr.Zero, this.class_name);

			if (IsElementKnownToBeSupported (className, part, state))
				return;
			if (theme == IntPtr.Zero || !VisualStyles.UxThemeIsThemePartDefined (theme, this.part))
				throw new ArgumentException ("This element is not supported by the current visual style.");
		}

		public void SetParameters (VisualStyleElement element)
		{
			this.SetParameters (element.ClassName, element.Part, element.State);
		}
		#endregion

		#region Private Properties
		internal static IVisualStyles VisualStyles {
			get { return VisualStylesEngine.Instance; }
		}
		#endregion

		#region Private Instance Methods
		internal void DrawBackgroundExcludingArea (IDeviceContext dc, Rectangle bounds, Rectangle excludedArea)
		{
			VisualStyles.VisualStyleRendererDrawBackgroundExcludingArea (theme, dc, part, state, bounds, excludedArea);
		}
		#endregion

		#region Private Static Methods
		private static bool IsElementKnownToBeSupported (string className, int part, int state)
		{
			return className == "STATUS" && part == 0 && state == 0;
		}
		#endregion

		#region Private Classes
		private class ThemeHandleManager
		{
			public VisualStyleRenderer VisualStyleRenderer;
			~ThemeHandleManager ()
			{
				if (VisualStyleRenderer.theme == IntPtr.Zero)
					return;
				VisualStyles.UxThemeCloseThemeData (VisualStyleRenderer.theme);
			}
		}
		#endregion
	}
}
