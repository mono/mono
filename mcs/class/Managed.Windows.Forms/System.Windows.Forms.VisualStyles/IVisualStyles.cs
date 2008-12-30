//
// IVisualStyles.cs: An implementation of VisualStyleRenderer and
// VisualStyleInformation.
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

using HRESULT = System.Int32;
using System.Drawing;
namespace System.Windows.Forms.VisualStyles
{
	interface IVisualStyles
	{
		#region UxTheme
		HRESULT UxThemeCloseThemeData (IntPtr hTheme);
		HRESULT UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds);
		HRESULT UxThemeDrawThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, Rectangle clipRectangle);
		HRESULT UxThemeDrawThemeEdge (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds,Edges edges, EdgeStyle style, EdgeEffects effects, out Rectangle result);
		HRESULT UxThemeDrawThemeParentBackground (IDeviceContext dc, Rectangle bounds, Control childControl);
		HRESULT UxThemeDrawThemeText (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, String text, TextFormatFlags textFlags, Rectangle bounds);
		HRESULT UxThemeGetThemeBackgroundContentRect (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Rectangle result);
		HRESULT UxThemeGetThemeBackgroundExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle contentBounds, out Rectangle result);
		HRESULT UxThemeGetThemeBackgroundRegion (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, out Region result);
		HRESULT UxThemeGetThemeBool (IntPtr hTheme, int iPartId, int iStateId, BooleanProperty prop, out bool result);
		HRESULT UxThemeGetThemeColor (IntPtr hTheme, int iPartId, int iStateId, ColorProperty prop, out Color result);
		HRESULT UxThemeGetThemeEnumValue (IntPtr hTheme, int iPartId, int iStateId, EnumProperty prop, out int result);
		HRESULT UxThemeGetThemeFilename (IntPtr hTheme, int iPartId, int iStateId, FilenameProperty prop, out string result);
		HRESULT UxThemeGetThemeInt (IntPtr hTheme, int iPartId, int iStateId, IntegerProperty prop, out int result);
		HRESULT UxThemeGetThemeMargins (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, MarginProperty prop, out Padding result);
		HRESULT UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, Rectangle bounds, ThemeSizeType type, out Size result);
		HRESULT UxThemeGetThemePartSize (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, ThemeSizeType type, out Size result);
		HRESULT UxThemeGetThemePosition (IntPtr hTheme, int iPartId, int iStateId, PointProperty prop, out Point result);
		HRESULT UxThemeGetThemeString (IntPtr hTheme, int iPartId, int iStateId, StringProperty prop, out string result);
		HRESULT UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, Rectangle bounds, out Rectangle result);
		HRESULT UxThemeGetThemeTextExtent (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, string textToDraw, TextFormatFlags flags, out Rectangle result);
		HRESULT UxThemeGetThemeTextMetrics (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, out TextMetrics result);
		HRESULT UxThemeHitTestThemeBackground (IntPtr hTheme, IDeviceContext dc, int iPartId, int iStateId, HitTestOptions options, Rectangle backgroundRectangle, IntPtr hrgn, Point pt, out HitTestCode result);
		bool UxThemeIsAppThemed ();
		bool UxThemeIsThemeActive ();
		bool UxThemeIsThemeBackgroundPartiallyTransparent (IntPtr hTheme, int iPartId, int iStateId);
		bool UxThemeIsThemePartDefined (IntPtr hTheme, int iPartId);
		IntPtr UxThemeOpenThemeData (IntPtr hWnd, String classList);
		#endregion
		#region VisualStyleInformation
		string VisualStyleInformationAuthor { get; }
		string VisualStyleInformationColorScheme { get; }
		string VisualStyleInformationCompany { get; }
		Color VisualStyleInformationControlHighlightHot { get; }
		string VisualStyleInformationCopyright { get; }
		string VisualStyleInformationDescription { get; }
		string VisualStyleInformationDisplayName { get; }
		string VisualStyleInformationFileName { get; }
		bool VisualStyleInformationIsSupportedByOS { get; }
		int VisualStyleInformationMinimumColorDepth { get; }
		string VisualStyleInformationSize { get; }
		bool VisualStyleInformationSupportsFlatMenus { get; }
		Color VisualStyleInformationTextControlBorder { get; }
		string VisualStyleInformationUrl { get; }
		string VisualStyleInformationVersion { get; }
		#endregion
		#region VisualStyleRenderer
		void VisualStyleRendererDrawBackgroundExcludingArea (IntPtr theme, IDeviceContext dc, int part, int state, Rectangle bounds, Rectangle excludedArea);
		#endregion
	}
}
