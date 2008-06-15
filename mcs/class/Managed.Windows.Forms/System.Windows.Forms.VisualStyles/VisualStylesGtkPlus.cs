//
// VisualStylesGtkPlus.cs: IVisualStyles that uses GtkPlus.
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
// Copyright (c) 2008 George Giolfan
//
// Authors:
//	George Giolfan (georgegiolfan@yahoo.com)
//

using System.Drawing;
using System.Collections.Generic;
namespace System.Windows.Forms.VisualStyles
{
	class VisualStylesGtkPlus : IVisualStyles
	{
		public static bool Initialize ()
		{
			return GtkPlus.Initialize ();
		}
		static GtkPlus GtkPlus {
			get {
				return GtkPlus.Instance;
			}
		}

		enum S {
			S_OK,
			S_FALSE
		}
		enum ThemeHandle {
			BUTTON = 1
		}

		#region UxTheme
		public int UxThemeCloseThemeData (IntPtr hTheme)
		{
#if DEBUG
			return (int)((Enum.IsDefined (typeof (ThemeHandle), (int)hTheme)) ? S.S_OK : S.S_FALSE);
#else
			return (int)S.S_OK;
#endif
		}
		public int UxThemeDrawThemeParentBackground (IDeviceContext dc, Rectangle bounds, Control childControl)
		{
			return (int)S.S_FALSE;
		}
		public int UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Rectangle clipRectangle)
		{
			return (int)(DrawBackground ((ThemeHandle)(int)hTheme, dc, iPartId, iStateId, bounds, clipRectangle, true) ? S.S_OK : S.S_FALSE);
		}
		static bool DrawBackground (ThemeHandle themHandle, IDeviceContext dc, int part, int state, Rectangle bounds, Rectangle clipRectangle, bool clipRectangleSpecified) {
			switch (themHandle) {
			case ThemeHandle.BUTTON:
				switch ((BUTTONPARTS)part) {
				case BUTTONPARTS.BP_CHECKBOX:
					switch ((CHECKBOXSTATES)state) {
					case CHECKBOXSTATES.CBS_UNCHECKEDNORMAL: GtkPlus.PaintCheckBox (dc, bounds); return true;
					default: return false;
					}
				default: return false;
				}
			default: return false;
			}
		}
		public int UxThemeDrawThemeEdge (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Edges edges, EdgeStyle style, EdgeEffects effects, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds)
		{
			return (int)(DrawBackground ((ThemeHandle)(int)hTheme, dc, iPartId, iStateId, bounds, Rectangle.Empty, false) ? S.S_OK : S.S_FALSE);
		}
		public int UxThemeDrawThemeText (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string text, TextFormatFlags textFlags, Rectangle bounds)
		{
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeBackgroundContentRect (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeBackgroundExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle contentBounds, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeBackgroundRegion (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Region result)
		{
			result = null;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeBool (IntPtr hTheme, int iPartId, int iStateId, BooleanProperty prop, out bool result)
		{
			result = false;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeColor (IntPtr hTheme, int iPartId, int iStateId, ColorProperty prop, out Color result)
		{
			result = Color.Black;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeEnumValue (IntPtr hTheme, int iPartId, int iStateId, EnumProperty prop, out int result)
		{
			result = 0;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeFilename (IntPtr hTheme, int iPartId, int iStateId, FilenameProperty prop, out string result)
		{
			result = null;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeInt (IntPtr hTheme, int iPartId, int iStateId, IntegerProperty prop, out int result)
		{
			result = 0;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeMargins (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, MarginProperty prop, out Padding result)
		{
			result = Padding.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, ThemeSizeType type, out Size result)
		{
			result = Size.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, ThemeSizeType type, out Size result)
		{
			result = Size.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemePosition (IntPtr hTheme, int iPartId, int iStateId, PointProperty prop, out Point result)
		{
			result = Point.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeString (IntPtr hTheme, int iPartId, int iStateId, StringProperty prop, out string result)
		{
			result = null;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, Rectangle bounds, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, out Rectangle result)
		{
			result = Rectangle.Empty;
			return (int)S.S_FALSE;
		}
		public int UxThemeGetThemeTextMetrics (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, out TextMetrics result)
		{
			result = new TextMetrics ();
			return (int)S.S_FALSE;
		}
		public int UxThemeHitTestThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, HitTestOptions options, Rectangle backgroundRectangle, IntPtr hrgn, Point pt, out HitTestCode result)
		{
			result = HitTestCode.Bottom;
			return (int)S.S_FALSE;
		}
		public bool UxThemeIsAppThemed ()
		{
			return true;
		}
		public bool UxThemeIsThemeActive ()
		{
			return true;
		}
		public bool UxThemeIsThemeBackgroundPartiallyTransparent (IntPtr hTheme, int iPartId, int iStateId)
		{
			return true;
		}
		public bool UxThemeIsThemePartDefined (IntPtr hTheme, int iPartId)
		{
			switch ((ThemeHandle)(int)hTheme) {
			case ThemeHandle.BUTTON:
				switch ((BUTTONPARTS)iPartId) {
				case BUTTONPARTS.BP_CHECKBOX: return true;
				default: return false;
				}
			default: return false;
			}
		}
		public IntPtr UxThemeOpenThemeData (IntPtr hWnd, string classList)
		{
			ThemeHandle theme_handle;
			try {
				theme_handle = (ThemeHandle)Enum.Parse (typeof (ThemeHandle), classList);
			} catch (ArgumentException) {
				return IntPtr.Zero;
			}
			return (IntPtr)(int)theme_handle;
		}
		#endregion
		#region VisualStyleInformation
		public string VisualStyleInformationAuthor {
			get {
				return null;
			}
		}
		public string VisualStyleInformationColorScheme {
			get {
				return null;
			}
		}
		public string VisualStyleInformationCompany {
			get {
				return null;
			}
		}
		public Color VisualStyleInformationControlHighlightHot {
			get {
				return Color.Black;
			}
		}
		public string VisualStyleInformationCopyright {
			get {
				return null;
			}
		}
		public string VisualStyleInformationDescription {
			get {
				return null;
			}
		}
		public string VisualStyleInformationDisplayName {
			get {
				return null;
			}
		}
		public bool VisualStyleInformationIsSupportedByOS {
			get {
				return true;
			}
		}
		public int VisualStyleInformationMinimumColorDepth {
			get {
				return 0;
			}
		}
		public string VisualStyleInformationSize {
			get {
				return null;
			}
		}
		public bool VisualStyleInformationSupportsFlatMenus {
			get {
				return false;
			}
		}
		public Color VisualStyleInformationTextControlBorder {
			get {
				return Color.Black;
			}
		}
		public string VisualStyleInformationUrl {
			get {
				return null;	
			}
		}
		public string VisualStyleInformationVersion {
			get {
				return null;	
			}
		}
		#endregion
		#region VisualStyleRenderer
		public void VisualStyleRendererDrawBackgroundExcludingArea (IntPtr theme, IDeviceContext dc, int part, int state, Rectangle bounds, Rectangle excludedArea)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
