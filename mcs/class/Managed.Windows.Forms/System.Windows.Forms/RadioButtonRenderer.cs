//
// RadioButtonRenderer.cs
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
	public sealed class RadioButtonRenderer
	{
		private static bool always_use_visual_styles = false;

		#region Private Constructor
		private RadioButtonRenderer () { }
		#endregion

		#region Public Static Methods
		public static void DrawRadioButton (Graphics g, Point glyphLocation, RadioButtonState state)
		{
			DrawRadioButton (g, glyphLocation, Rectangle.Empty, String.Empty, null, TextFormatFlags.HorizontalCenter, null, Rectangle.Empty, false, state);
		}

		public static void DrawRadioButton (Graphics g, Point glyphLocation, Rectangle textBounds, string radioButtonText, Font font, bool focused, RadioButtonState state)
		{
			DrawRadioButton (g, glyphLocation, textBounds, radioButtonText, font, TextFormatFlags.HorizontalCenter, null, Rectangle.Empty, focused, state);
		}

		public static void DrawRadioButton (Graphics g, Point glyphLocation, Rectangle textBounds, string radioButtonText, Font font, TextFormatFlags flags, bool focused, RadioButtonState state)
		{
			DrawRadioButton (g, glyphLocation, textBounds, radioButtonText, font, flags, null, Rectangle.Empty, focused, state);
		}

		public static void DrawRadioButton (Graphics g, Point glyphLocation, Rectangle textBounds, string radioButtonText, Font font, Image image, Rectangle imageBounds, bool focused, RadioButtonState state)
		{
			DrawRadioButton (g, glyphLocation, textBounds, radioButtonText, font, TextFormatFlags.HorizontalCenter, image, imageBounds, focused, state);
		}

		public static void DrawRadioButton (Graphics g, Point glyphLocation, Rectangle textBounds, string radioButtonText, Font font, TextFormatFlags flags, Image image, Rectangle imageBounds, bool focused, RadioButtonState state)
		{
			Rectangle bounds = new Rectangle (glyphLocation, GetGlyphSize (g, state));

			if (Application.RenderWithVisualStyles || always_use_visual_styles == true) {
				VisualStyleRenderer vsr = GetRadioButtonRenderer (state);

				vsr.DrawBackground (g, bounds);

				if (image != null)
					vsr.DrawImage (g, imageBounds, image);

				if (focused)
					ControlPaint.DrawFocusRectangle (g, textBounds);

				if (radioButtonText != String.Empty)
					if (state == RadioButtonState.CheckedDisabled || state == RadioButtonState.UncheckedDisabled)
						TextRenderer.DrawText (g, radioButtonText, font, textBounds, SystemColors.GrayText, flags);
					else
						TextRenderer.DrawText (g, radioButtonText, font, textBounds, SystemColors.ControlText, flags);
			}
			else {
				switch (state) {
					case RadioButtonState.CheckedDisabled:
						ControlPaint.DrawRadioButton (g, bounds, ButtonState.Inactive | ButtonState.Checked);
						break;
					case RadioButtonState.CheckedHot:
					case RadioButtonState.CheckedNormal:
						ControlPaint.DrawRadioButton (g, bounds, ButtonState.Checked);
						break;
					case RadioButtonState.CheckedPressed:
						ControlPaint.DrawRadioButton (g, bounds, ButtonState.Pushed | ButtonState.Checked);
						break;
					case RadioButtonState.UncheckedDisabled:
					case RadioButtonState.UncheckedPressed:
						ControlPaint.DrawRadioButton (g, bounds, ButtonState.Inactive);
						break;
					case RadioButtonState.UncheckedHot:
					case RadioButtonState.UncheckedNormal:
						ControlPaint.DrawRadioButton (g, bounds, ButtonState.Normal);
						break;
				}

				if (image != null)
					g.DrawImage (image, imageBounds);
			
				if (focused)
					ControlPaint.DrawFocusRectangle (g, textBounds);

				if (radioButtonText != String.Empty)
					TextRenderer.DrawText (g, radioButtonText, font, textBounds, SystemColors.ControlText, flags);
			}

		}

		public static bool IsBackgroundPartiallyTransparent (RadioButtonState state)
		{
			if (!VisualStyleRenderer.IsSupported)
				return false;

			VisualStyleRenderer vsr = GetRadioButtonRenderer (state);

			return vsr.IsBackgroundPartiallyTransparent ();
		}

		public static void DrawParentBackground (Graphics g, Rectangle bounds, Control childControl)
		{
			if (!VisualStyleRenderer.IsSupported)
				return;

			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.UncheckedNormal);

			vsr.DrawParentBackground (g, bounds, childControl);
		}

		public static Size GetGlyphSize (Graphics g, RadioButtonState state)
		{
			if (!VisualStyleRenderer.IsSupported)
				return new Size (13, 13);

			VisualStyleRenderer vsr = GetRadioButtonRenderer(state);

			return vsr.GetPartSize (g, ThemeSizeType.Draw);
		}
		#endregion

		#region Private Static Methods
		private static VisualStyleRenderer GetRadioButtonRenderer (RadioButtonState state)
		{
			switch (state) {
				case RadioButtonState.CheckedDisabled:
					return new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.CheckedDisabled);
				case RadioButtonState.CheckedHot:
					return new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.CheckedHot);
				case RadioButtonState.CheckedNormal:
					return new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.CheckedNormal);
				case RadioButtonState.CheckedPressed:
					return new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.CheckedPressed);
				case RadioButtonState.UncheckedDisabled:
					return new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.UncheckedDisabled);
				case RadioButtonState.UncheckedHot:
					return new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.UncheckedHot);
				case RadioButtonState.UncheckedNormal:
				default:
					return new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.UncheckedNormal);
				case RadioButtonState.UncheckedPressed:
					return new VisualStyleRenderer (VisualStyleElement.Button.RadioButton.UncheckedPressed);
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