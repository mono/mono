//
// ButtonRenderer.cs
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
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms
{
	public sealed class ButtonRenderer
	{
		private static bool always_use_visual_styles = false;

		#region Private Constructor
		private ButtonRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawButton (Graphics g, Rectangle bounds, PushButtonState state)
		{
			DrawButton (g, bounds, String.Empty, null, TextFormatFlags.Default, null, Rectangle.Empty, false, state);
		}

		public static void DrawButton (Graphics g, Rectangle bounds, bool focused, PushButtonState state)
		{
			DrawButton (g, bounds, String.Empty, null, TextFormatFlags.Default, null, Rectangle.Empty, focused, state);
		}

		public static void DrawButton (Graphics g, Rectangle bounds, Image image, Rectangle imageBounds, bool focused, PushButtonState state)
		{
			DrawButton (g, bounds, String.Empty, null, TextFormatFlags.Default, image, imageBounds, focused, state);
		}

		public static void DrawButton (Graphics g, Rectangle bounds, string buttonText, Font font, bool focused, PushButtonState state)
		{
			DrawButton (g, bounds, buttonText, font, TextFormatFlags.HorizontalCenter, null, Rectangle.Empty, focused, state);
		}

		public static void DrawButton (Graphics g, Rectangle bounds, string buttonText, Font font, TextFormatFlags flags, bool focused, PushButtonState state)
		{
			DrawButton (g, bounds, buttonText, font, flags, null, Rectangle.Empty, focused, state);
		}

		public static void DrawButton (Graphics g, Rectangle bounds, string buttonText, Font font, Image image, Rectangle imageBounds, bool focused, PushButtonState state)
		{
			DrawButton (g, bounds, buttonText, font, TextFormatFlags.HorizontalCenter, image, imageBounds, focused, state);
		}

		public static void DrawButton (Graphics g, Rectangle bounds, string buttonText, Font font, TextFormatFlags flags, Image image, Rectangle imageBounds, bool focused, PushButtonState state)
		{
			if (Application.RenderWithVisualStyles || always_use_visual_styles == true) {
				VisualStyleRenderer vsr = GetPushButtonRenderer (state);

				vsr.DrawBackground (g, bounds);

				if (image != null)
					vsr.DrawImage (g, imageBounds, image);
			} else {
				if (state == PushButtonState.Pressed)
					ControlPaint.DrawButton (g, bounds, ButtonState.Pushed);
				else
					ControlPaint.DrawButton (g, bounds, ButtonState.Normal);

				if (image != null)
					g.DrawImage (image, imageBounds);
			}

			Rectangle focus_rect = bounds;
			focus_rect.Inflate (-3, -3);

			if (focused)
				ControlPaint.DrawFocusRectangle (g, focus_rect);

			if (buttonText != String.Empty)
				if (state == PushButtonState.Disabled)
					TextRenderer.DrawText (g, buttonText, font, focus_rect, SystemColors.GrayText, flags);
				else
					TextRenderer.DrawText (g, buttonText, font, focus_rect, SystemColors.ControlText, flags);
		}

		public static bool IsBackgroundPartiallyTransparent (PushButtonState state)
		{
			if (!VisualStyleRenderer.IsSupported)
				return false;

			VisualStyleRenderer vsr = GetPushButtonRenderer (state);

			return vsr.IsBackgroundPartiallyTransparent ();
		}

		public static void DrawParentBackground (Graphics g, Rectangle bounds, Control childControl)
		{
			if (!VisualStyleRenderer.IsSupported)
				return;
			
			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.Button.PushButton.Default);

			vsr.DrawParentBackground (g, bounds, childControl);
		}
		#endregion

		#region Private Static Methods
		internal static VisualStyleRenderer GetPushButtonRenderer (PushButtonState state)
		{
			switch (state) {
				case PushButtonState.Normal:
					return new VisualStyleRenderer (VisualStyleElement.Button.PushButton.Normal);
				case PushButtonState.Hot:
					return new VisualStyleRenderer (VisualStyleElement.Button.PushButton.Hot);
				case PushButtonState.Pressed:
					return new VisualStyleRenderer (VisualStyleElement.Button.PushButton.Pressed);
				case PushButtonState.Disabled:
					return new VisualStyleRenderer (VisualStyleElement.Button.PushButton.Disabled);
				case PushButtonState.Default:
				default:
					return new VisualStyleRenderer (VisualStyleElement.Button.PushButton.Default);
			}
		}
		#endregion

		#region Public Static Properties
		public static bool RenderMatchingApplicationState {
			get { return !always_use_visual_styles; }
			set { always_use_visual_styles = !value; }
		}
		#endregion
	}
}