//
// VisualStylesNative.cs: IVisualStyles that uses the Visual Styles feature of
// Windows XP and later.
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
using System.Diagnostics;
namespace System.Windows.Forms.VisualStyles
{
	class VisualStylesNative : IVisualStyles
	{
		#region UxTheme
		public int UxThemeCloseThemeData (IntPtr hTheme)
		{
			return UXTheme.CloseThemeData (hTheme);
		}
		public int UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);

			int result = UXTheme.DrawThemeBackground(hTheme, dc.GetHdc (), iPartId, iStateId, ref BoundsRect, IntPtr.Zero);
			dc.ReleaseHdc ();
			return result;
		}
		public int UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Rectangle clipRectangle)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			XplatUIWin32.RECT ClipRect = XplatUIWin32.RECT.FromRectangle (clipRectangle);

			int result = UXTheme.DrawThemeBackground (hTheme, dc.GetHdc (), iPartId, iStateId, ref BoundsRect, ref ClipRect);
			dc.ReleaseHdc ();
			return result;
		}
		public int UxThemeDrawThemeEdge (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Edges edges, EdgeStyle style, EdgeEffects effects, out Rectangle result)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			XplatUIWin32.RECT retval;

			int hresult = UXTheme.DrawThemeEdge (hTheme, dc.GetHdc (), iPartId, iStateId, ref BoundsRect, (uint)style, (uint)edges + (uint)effects, out retval);
			dc.ReleaseHdc ();
			result = retval.ToRectangle();
			return hresult;
		}
		public int UxThemeDrawThemeParentBackground (IDeviceContext dc, Rectangle bounds, Control childControl)
		{
			int result;

			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);

			using (Graphics g = Graphics.FromHwnd (childControl.Handle)) {
				IntPtr hdc = g.GetHdc ();
				result = UXTheme.DrawThemeParentBackground (childControl.Handle, hdc, ref BoundsRect);
				g.ReleaseHdc (hdc);
			}

			return result;
		}
		public int UxThemeDrawThemeText (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string text, TextFormatFlags textFlags, Rectangle bounds)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);

			int result = UXTheme.DrawThemeText (hTheme, dc.GetHdc (), iPartId, iStateId, text, text.Length, (uint)textFlags, 0, ref BoundsRect);
			dc.ReleaseHdc ();
			return result;
		}
		public int UxThemeGetThemeBackgroundContentRect (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Rectangle result)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			XplatUIWin32.RECT retval;

			int hresult = UXTheme.GetThemeBackgroundContentRect (hTheme, dc.GetHdc (), iPartId, iStateId, ref BoundsRect, out retval);
			dc.ReleaseHdc ();

			result = retval.ToRectangle ();
			return hresult;
		}
		public int UxThemeGetThemeBackgroundExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle contentBounds, out Rectangle result)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (contentBounds);
			XplatUIWin32.RECT retval = new XplatUIWin32.RECT ();

			int hresult = UXTheme.GetThemeBackgroundExtent (hTheme, dc.GetHdc (), iPartId, iStateId, ref BoundsRect, ref retval);
			dc.ReleaseHdc ();

			result = retval.ToRectangle ();
			return hresult;
		}
		public int UxThemeGetThemeBackgroundRegion (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Region result)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			IntPtr retval;

			int hresult = UXTheme.GetThemeBackgroundRegion (hTheme, dc.GetHdc (), iPartId, iStateId, ref BoundsRect, out retval);
			dc.ReleaseHdc ();

			result = Region.FromHrgn (retval);
			return hresult;
		}
		public int UxThemeGetThemeBool (IntPtr hTheme, int iPartId, int iStateId, BooleanProperty prop, out bool result)
		{
			int retval;
			int hresult = UXTheme.GetThemeBool (hTheme, iPartId, iStateId, (int)prop, out retval);

			result = retval == 0 ? false : true;
			return hresult;
		}
		public int UxThemeGetThemeColor (IntPtr hTheme, int iPartId, int iStateId, ColorProperty prop, out Color result)
		{
			int retval;
			int hresult = UXTheme.GetThemeColor (hTheme, iPartId, iStateId, (int)prop, out retval);

			result = System.Drawing.Color.FromArgb ((int)(0x000000FFU & retval),
			     (int)(0x0000FF00U & retval) >> 8, (int)(0x00FF0000U & retval) >> 16);
			return hresult;
		}
		public int UxThemeGetThemeEnumValue (IntPtr hTheme, int iPartId, int iStateId, EnumProperty prop, out int result)
		{
			int retval;
			int hresult = UXTheme.GetThemeEnumValue (hTheme, iPartId, iStateId, (int)prop, out retval);

			result = retval;
			return hresult;
		}
		public int UxThemeGetThemeFilename (IntPtr hTheme, int iPartId, int iStateId, FilenameProperty prop, out string result)
		{
			Text.StringBuilder sb = new Text.StringBuilder (255);
			int hresult = UXTheme.GetThemeFilename (hTheme, iPartId, iStateId, (int)prop, sb, sb.Capacity);

			result = sb.ToString ();
			return hresult;
		}
		public int UxThemeGetThemeInt (IntPtr hTheme, int iPartId, int iStateId, IntegerProperty prop, out int result)
		{
			int retval;
			int hresult = UXTheme.GetThemeInt (hTheme, iPartId, iStateId, (int)prop, out retval);

			result = retval;
			return hresult;
		}
		public int UxThemeGetThemeMargins (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, MarginProperty prop, out Padding result)
		{
			UXTheme.MARGINS retval = new UXTheme.MARGINS ();
			XplatUIWin32.RECT BoundsRect;

			int hresult = UXTheme.GetThemeMargins (hTheme, dc.GetHdc (), iPartId, iStateId, (int)prop, out BoundsRect, out retval);
			dc.ReleaseHdc ();

			result = retval.ToPadding();
			return hresult;
		}
		public int UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, ThemeSizeType type, out Size result)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			UXTheme.SIZE retval;

			int hresult = UXTheme.GetThemePartSize (hTheme, dc.GetHdc (), iPartId, iStateId, ref BoundsRect, (int)type, out retval);
			dc.ReleaseHdc ();

			result = retval.ToSize();
			return hresult;
		}
		public int UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, ThemeSizeType type, out Size result)
		{
			UXTheme.SIZE retval;

			int hresult = UXTheme.GetThemePartSize (hTheme, dc.GetHdc (), iPartId, iStateId, IntPtr.Zero, (int)type, out retval);
			dc.ReleaseHdc ();

			result = retval.ToSize ();
			return hresult;
		}
		public int UxThemeGetThemePosition (IntPtr hTheme, int iPartId, int iStateId, PointProperty prop, out Point result)
		{
			POINT retval;
			int hresult = UXTheme.GetThemePosition (hTheme, iPartId, iStateId, (int)prop, out retval);

			result = retval.ToPoint();
			return hresult;
		}
		public int UxThemeGetThemeString (IntPtr hTheme, int iPartId, int iStateId, StringProperty prop, out string result)
		{
			Text.StringBuilder sb = new Text.StringBuilder (255);
			int hresult = UXTheme.GetThemeString (hTheme, iPartId, iStateId, (int)prop, sb, sb.Capacity);

			result = sb.ToString ();
			return hresult;
		}
		public int UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, Rectangle bounds, out Rectangle result)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (bounds);
			XplatUIWin32.RECT retval;
			
			int hresult = UXTheme.GetThemeTextExtent (hTheme, dc.GetHdc (), iPartId, iStateId, textToDraw, textToDraw.Length, (int)flags, ref BoundsRect, out retval);
			dc.ReleaseHdc ();

			result = retval.ToRectangle ();
			return hresult;
		}
		public int UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, out Rectangle result)
		{
			XplatUIWin32.RECT retval;
			
			int hresult = UXTheme.GetThemeTextExtent (hTheme, dc.GetHdc (), iPartId, iStateId, textToDraw, textToDraw.Length, (int)flags, 0, out retval);
			dc.ReleaseHdc ();

			result = retval.ToRectangle ();
			return hresult;
		}
		public int UxThemeGetThemeTextMetrics (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, out TextMetrics result)
		{
			XplatUIWin32.TEXTMETRIC metrics;
			
			int hresult = UXTheme.GetThemeTextMetrics (hTheme, dc.GetHdc (), iPartId, iStateId, out metrics);
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

			result = retval;
			return hresult;
		}
		public int UxThemeHitTestThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, HitTestOptions options, Rectangle backgroundRectangle, IntPtr hrgn, Point pt, out HitTestCode result)
		{
			XplatUIWin32.RECT BoundsRect = XplatUIWin32.RECT.FromRectangle (backgroundRectangle);
			int retval;

			int hresult = UXTheme.HitTestThemeBackground (hTheme, dc.GetHdc (), iPartId, iStateId, (uint)options, ref BoundsRect, hrgn, new POINT(pt.X, pt.Y), out retval);
			dc.ReleaseHdc ();

			result = (HitTestCode)retval;
			return hresult;
		}
		public bool UxThemeIsAppThemed ()
		{
			return UXTheme.IsAppThemed ();
		}
		public bool UxThemeIsThemeActive ()
		{
			return UXTheme.IsThemeActive ();
		}
		public bool UxThemeIsThemePartDefined (IntPtr hTheme, int iPartId)
		{
			return UXTheme.IsThemePartDefined (hTheme, iPartId, 0);
		}
		public bool UxThemeIsThemeBackgroundPartiallyTransparent (IntPtr hTheme, int iPartId, int iStateId)
		{
			int retval = UXTheme.IsThemeBackgroundPartiallyTransparent (hTheme, iPartId, iStateId);

			return retval == 0 ? false : true;
		}
		public IntPtr UxThemeOpenThemeData (IntPtr hWnd, string classList)
		{
			return UXTheme.OpenThemeData (hWnd, classList);
		}
		#endregion
		#region VisualStyleInformation
		public string VisualStyleInformationAuthor {
			get {
				return GetData ("AUTHOR");
			}
		}
		public string VisualStyleInformationColorScheme {
			get {
				Text.StringBuilder ThemeName = new Text.StringBuilder (260);
				Text.StringBuilder ColorName = new Text.StringBuilder (260);
				Text.StringBuilder SizeName = new Text.StringBuilder (260);
				UXTheme.GetCurrentThemeName (ThemeName, ThemeName.Capacity, ColorName, ColorName.Capacity, SizeName, SizeName.Capacity);

				return ColorName.ToString ();
			}
		}
		public string VisualStyleInformationCompany {
			get {
				return GetData ("COMPANY");
			}
		}
		public Color VisualStyleInformationControlHighlightHot {
			get {
				IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, "BUTTON");

				uint retval = UXTheme.GetThemeSysColor (theme, 1621);
				UXTheme.CloseThemeData (theme);

				return System.Drawing.Color.FromArgb ((int)(0x000000FFU & retval),
					(int)(0x0000FF00U & retval) >> 8, (int)(0x00FF0000U & retval) >> 16);
			}
		}
		public string VisualStyleInformationCopyright {
			get {
				return GetData ("COPYRIGHT");
			}
		}
		public string VisualStyleInformationDescription {
			get {
				return GetData ("DESCRIPTION");
			}
		}
		public string VisualStyleInformationDisplayName {
			get {
				return GetData ("DISPLAYNAME");
			}
		}
		public string VisualStyleInformationFileName {
			get {
				Text.StringBuilder ThemeName = new Text.StringBuilder (260);
				Text.StringBuilder ColorName = new Text.StringBuilder (260);
				Text.StringBuilder SizeName = new Text.StringBuilder (260);
				UXTheme.GetCurrentThemeName (ThemeName, ThemeName.Capacity, ColorName, ColorName.Capacity, SizeName, SizeName.Capacity);

				return ThemeName.ToString ();
			}
		}
		static string GetData (string propertyName)
		{
			Text.StringBuilder ThemeName = new Text.StringBuilder (260);
			Text.StringBuilder ColorName = new Text.StringBuilder (260);
			Text.StringBuilder SizeName = new Text.StringBuilder (260);

			UXTheme.GetCurrentThemeName (ThemeName, ThemeName.Capacity, ColorName, ColorName.Capacity, SizeName, SizeName.Capacity);

			Text.StringBuilder PropertyValue = new Text.StringBuilder (260);

			UXTheme.GetThemeDocumentationProperty (ThemeName.ToString(), propertyName, PropertyValue, PropertyValue.Capacity);

			return PropertyValue.ToString ();
		}
		public bool VisualStyleInformationIsSupportedByOS {
			get {
				return IsSupported ();
			}
		}
		public int VisualStyleInformationMinimumColorDepth {
			get {
				IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, "BUTTON");
				int retval;
				
				UXTheme.GetThemeSysInt (theme, 1301, out retval);
				UXTheme.CloseThemeData (theme);

				return retval;
			}
		}
		public static bool IsSupported ()
		{
			// Supported OS's should be NT based and at least XP (XP, 2003, Vista)
			if ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version >= new Version (5, 1))) 
				return true;
			
			return false;
		}
		public string VisualStyleInformationSize {
			get {
				Text.StringBuilder ThemeName = new Text.StringBuilder (260);
				Text.StringBuilder ColorName = new Text.StringBuilder (260);
				Text.StringBuilder SizeName = new Text.StringBuilder (260);
				UXTheme.GetCurrentThemeName (ThemeName, ThemeName.Capacity, ColorName, ColorName.Capacity, SizeName, SizeName.Capacity);

				return SizeName.ToString ();
			}
		}
		public bool VisualStyleInformationSupportsFlatMenus {
			get {
				IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, "BUTTON");
				bool retval;

				retval = UXTheme.GetThemeSysBool (theme, 1001) == 0 ? false : true;
				UXTheme.CloseThemeData (theme);

				return retval;
			}
		}
		public Color VisualStyleInformationTextControlBorder {
			get {
				IntPtr theme = UXTheme.OpenThemeData (IntPtr.Zero, "EDIT");

				uint retval = UXTheme.GetThemeSysColor (theme, 1611);
				UXTheme.CloseThemeData (theme);

				return System.Drawing.Color.FromArgb ((int)(0x000000FFU & retval),
						     (int)(0x0000FF00U & retval) >> 8, (int)(0x00FF0000U & retval) >> 16);
			}
		}
		public string VisualStyleInformationUrl {
			get {
				return GetData ("URL");
			}
		}
		public string VisualStyleInformationVersion {
			get {
				return GetData ("VERSION");
			}
		}
		#endregion
		#region VisualStyleRenderer
		public void VisualStyleRendererDrawBackgroundExcludingArea (IntPtr theme, IDeviceContext dc, int part, int state, Rectangle bounds, Rectangle excludedArea)
		{
			XplatUIWin32.RECT bounds_rect = XplatUIWin32.RECT.FromRectangle (bounds);
			IntPtr hdc = dc.GetHdc ();
			XplatUIWin32.Win32ExcludeClipRect (hdc, excludedArea.Left, excludedArea.Top, excludedArea.Right, excludedArea.Bottom);
			UXTheme.DrawThemeBackground (theme, hdc, part, state, ref bounds_rect, IntPtr.Zero);
			IntPtr hrgn = XplatUIWin32.Win32CreateRectRgn (excludedArea.Left, excludedArea.Top, excludedArea.Right, excludedArea.Bottom);
			XplatUIWin32.Win32ExtSelectClipRgn (hdc, hrgn, (int)ClipCombineMode.RGN_OR);
			XplatUIWin32.Win32DeleteObject (hrgn);
			dc.ReleaseHdc ();
		}
		#endregion
	}
}
