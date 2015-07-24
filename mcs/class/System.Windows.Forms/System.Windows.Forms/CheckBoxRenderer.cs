//
// CheckBoxRenderer.cs
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
	public sealed class CheckBoxRenderer
	{
		private static bool always_use_visual_styles = false;

		#region Private Constructor
		private CheckBoxRenderer () {}
		#endregion
		
		#region Public Static Methods
		public static void DrawCheckBox (Graphics g, Point glyphLocation, CheckBoxState state)
		{
			DrawCheckBox (g, glyphLocation, Rectangle.Empty, String.Empty, null, TextFormatFlags.HorizontalCenter, null, Rectangle.Empty, false, state);
		}

		public static void DrawCheckBox (Graphics g, Point glyphLocation, Rectangle textBounds, string checkBoxText, Font font, bool focused, CheckBoxState state)
		{
			DrawCheckBox (g, glyphLocation, textBounds, checkBoxText, font, TextFormatFlags.HorizontalCenter, null, Rectangle.Empty, focused, state);
		}

		public static void DrawCheckBox (Graphics g, Point glyphLocation, Rectangle textBounds, string checkBoxText, Font font, TextFormatFlags flags, bool focused, CheckBoxState state)
		{
			DrawCheckBox (g, glyphLocation, textBounds, checkBoxText, font, flags, null, Rectangle.Empty, focused, state);
		}

		public static void DrawCheckBox (Graphics g, Point glyphLocation, Rectangle textBounds, string checkBoxText, Font font, Image image, Rectangle imageBounds, bool focused, CheckBoxState state)
		{
			DrawCheckBox (g, glyphLocation, textBounds, checkBoxText, font, TextFormatFlags.HorizontalCenter, image, imageBounds, focused, state);
		}

		public static void DrawCheckBox (Graphics g, Point glyphLocation, Rectangle textBounds, string checkBoxText, Font font, TextFormatFlags flags, Image image, Rectangle imageBounds, bool focused, CheckBoxState state)
		{
			Rectangle bounds = new Rectangle (glyphLocation, GetGlyphSize (g, state));

			if (Application.RenderWithVisualStyles || always_use_visual_styles == true) {
				VisualStyleRenderer vsr = GetCheckBoxRenderer (state);

				vsr.DrawBackground (g, bounds);

				if (image != null)
					vsr.DrawImage (g, imageBounds, image);

				if (focused)
					ControlPaint.DrawFocusRectangle (g, textBounds);

				if (checkBoxText != String.Empty)
					if (state == CheckBoxState.CheckedDisabled || state == CheckBoxState.MixedDisabled || state == CheckBoxState.UncheckedDisabled)
						TextRenderer.DrawText (g, checkBoxText, font, textBounds, SystemColors.GrayText, flags);
					else
						TextRenderer.DrawText (g, checkBoxText, font, textBounds, SystemColors.ControlText, flags);
			} else {
				switch (state) {
					case CheckBoxState.CheckedDisabled:
					case CheckBoxState.MixedDisabled:
					case CheckBoxState.MixedPressed:
						ControlPaint.DrawCheckBox (g, bounds, ButtonState.Inactive | ButtonState.Checked);
						break;
					case CheckBoxState.CheckedHot:
					case CheckBoxState.CheckedNormal:
						ControlPaint.DrawCheckBox (g, bounds, ButtonState.Checked);
						break;
					case CheckBoxState.CheckedPressed:
						ControlPaint.DrawCheckBox (g, bounds, ButtonState.Pushed | ButtonState.Checked);
						break;
					case CheckBoxState.MixedHot:
					case CheckBoxState.MixedNormal:
						ControlPaint.DrawMixedCheckBox (g, bounds, ButtonState.Checked);
						break;
					case CheckBoxState.UncheckedDisabled:
					case CheckBoxState.UncheckedPressed:
						ControlPaint.DrawCheckBox (g, bounds, ButtonState.Inactive);
						break;
					case CheckBoxState.UncheckedHot:
					case CheckBoxState.UncheckedNormal:
						ControlPaint.DrawCheckBox (g, bounds, ButtonState.Normal);
						break;
				}

				if (image != null)
					g.DrawImage (image, imageBounds);

				if (focused)
					ControlPaint.DrawFocusRectangle (g, textBounds);

				if (checkBoxText != String.Empty)
					TextRenderer.DrawText (g, checkBoxText, font, textBounds, SystemColors.ControlText, flags);
			}
		}

		public static bool IsBackgroundPartiallyTransparent (CheckBoxState state)
		{
			if (!VisualStyleRenderer.IsSupported)
				return false;

			VisualStyleRenderer vsr = GetCheckBoxRenderer (state);

			return vsr.IsBackgroundPartiallyTransparent ();
		}

		public static void DrawParentBackground (Graphics g, Rectangle bounds, Control childControl)
		{
			if (!VisualStyleRenderer.IsSupported)
				return;
				
			VisualStyleRenderer vsr = new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.UncheckedNormal);

			vsr.DrawParentBackground (g, bounds, childControl);
		}

		public static Size GetGlyphSize (Graphics g, CheckBoxState state)
		{
			if (!VisualStyleRenderer.IsSupported)
				return new Size (13, 13);

			VisualStyleRenderer vsr = GetCheckBoxRenderer (state);

			return vsr.GetPartSize (g, ThemeSizeType.Draw);
		}
		#endregion

		#region Private Static Methods
		private static VisualStyleRenderer GetCheckBoxRenderer (CheckBoxState state)
		{
			switch (state) {
				case CheckBoxState.CheckedDisabled:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.CheckedDisabled);
				case CheckBoxState.CheckedHot:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.CheckedHot);
				case CheckBoxState.CheckedNormal:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.CheckedNormal);
				case CheckBoxState.CheckedPressed:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.CheckedPressed);
				case CheckBoxState.MixedDisabled:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.MixedDisabled);
				case CheckBoxState.MixedHot:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.MixedHot);
				case CheckBoxState.MixedNormal:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.MixedNormal);
				case CheckBoxState.MixedPressed:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.MixedPressed);
				case CheckBoxState.UncheckedDisabled:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.UncheckedDisabled);
				case CheckBoxState.UncheckedHot:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.UncheckedHot);
				case CheckBoxState.UncheckedNormal:
				default:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.UncheckedNormal);
				case CheckBoxState.UncheckedPressed:
					return new VisualStyleRenderer (VisualStyleElement.Button.CheckBox.UncheckedPressed);
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