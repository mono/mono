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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.Theming.Default
{
	/// <summary>
	/// Summary description for Button.
	/// </summary>
	internal class RadioButtonPainter
	{
		public RadioButtonPainter ()
		{
		}

		protected SystemResPool ResPool { get { return ThemeEngine.Current.ResPool; } }

		public void PaintRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, ElementState state, FlatStyle style, bool isChecked)
		{
			switch (style) {
				case FlatStyle.Standard:
				case FlatStyle.System:
					switch (state) {
						case ElementState.Normal:
							DrawNormalRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Hot:
							DrawHotRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Pressed:
							DrawPressedRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Disabled:
							DrawDisabledRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
					}
					break;
				case FlatStyle.Flat:
					switch (state) {
						case ElementState.Normal:
							DrawFlatNormalRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Hot:
							DrawFlatHotRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Pressed:
							DrawFlatPressedRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Disabled:
							DrawFlatDisabledRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
					}
					break;
				case FlatStyle.Popup:
					switch (state) {
						case ElementState.Normal:
							DrawPopupNormalRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Hot:
							DrawPopupHotRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Pressed:
							DrawPopupPressedRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
						case ElementState.Disabled:
							DrawPopupDisabledRadioButton (g, bounds, backColor, foreColor, isChecked);
							break;
					}
					break;
			}
		}

		#region Standard
		public virtual void DrawNormalRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			ButtonState bs = ButtonState.Normal;

			if (isChecked)
				bs |= ButtonState.Checked;

			ControlPaint.DrawRadioButton (g, bounds, bs);
		}

		public virtual void DrawHotRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			DrawNormalRadioButton (g, bounds, backColor, foreColor, isChecked);
		}

		public virtual void DrawPressedRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			ButtonState bs = ButtonState.Pushed;

			if (isChecked)
				bs |= ButtonState.Checked;

			ControlPaint.DrawRadioButton (g, bounds, bs);
		}

		public virtual void DrawDisabledRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			ButtonState bs = ButtonState.Inactive;

			if (isChecked)
				bs |= ButtonState.Checked;

			ControlPaint.DrawRadioButton (g, bounds, bs);
		}
		#endregion

		#region FlatStyle
		public virtual void DrawFlatNormalRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			g.DrawArc (SystemPens.ControlDarkDark, bounds, 0, 359);
			g.FillPie (SystemBrushes.ControlLightLight, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2, 0, 359);

			if (isChecked)
				DrawFlatRadioGlyphDot (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawFlatHotRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			g.DrawArc (SystemPens.ControlDarkDark, bounds, 0, 359);
			g.FillPie (SystemBrushes.ControlLight, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2, 0, 359);

			if (isChecked)
				DrawFlatRadioGlyphDot (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawFlatPressedRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			g.DrawArc (SystemPens.ControlDarkDark, bounds, 0, 359);
			g.FillPie (SystemBrushes.ControlLightLight, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2, 0, 359);

			if (isChecked)
				DrawFlatRadioGlyphDot (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawFlatDisabledRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			g.FillPie (SystemBrushes.Control, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2, 0, 359);
			g.DrawArc (SystemPens.ControlDark, bounds, 0, 359);

			if (isChecked)
				DrawFlatRadioGlyphDot (g, bounds, SystemColors.ControlDark);
		}
		#endregion

		#region Popup
		public virtual void DrawPopupNormalRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			g.FillPie (SystemBrushes.ControlLightLight, bounds, 0, 359);
			g.DrawArc (SystemPens.ControlDark, bounds, 0, 359);

			if (isChecked)
				DrawFlatRadioGlyphDot (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawPopupHotRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			g.FillPie (SystemBrushes.ControlLightLight, bounds, 0, 359);
			g.DrawArc (SystemPens.ControlLight, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2, 0, 359);

			g.DrawArc (SystemPens.ControlDark, bounds, 135, 180);
			g.DrawArc (SystemPens.ControlLightLight, bounds, 315, 180);

			if (isChecked)
				DrawFlatRadioGlyphDot (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawPopupPressedRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			g.FillPie (SystemBrushes.ControlLightLight, bounds, 0, 359);
			g.DrawArc (SystemPens.ControlLight, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2, 0, 359);

			g.DrawArc (SystemPens.ControlDark, bounds, 135, 180);
			g.DrawArc (SystemPens.ControlLightLight, bounds, 315, 180);

			if (isChecked)
				DrawFlatRadioGlyphDot (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawPopupDisabledRadioButton (Graphics g, Rectangle bounds, Color backColor, Color foreColor, bool isChecked)
		{
			g.FillPie (SystemBrushes.Control, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2, 0, 359);
			g.DrawArc (SystemPens.ControlDark, bounds, 0, 359);

			if (isChecked)
				DrawFlatRadioGlyphDot (g, bounds, SystemColors.ControlDarkDark);
		}
		#endregion

		#region Glyph
		protected void DrawFlatRadioGlyphDot (Graphics g, Rectangle bounds, Color dotColor)
		{
			int lineWidth = Math.Max (1, Math.Min (bounds.Width, bounds.Height) / 3);

			Pen dot_pen = ResPool.GetPen (dotColor);
			Brush dot_brush = ResPool.GetSolidBrush (dotColor);

			if (bounds.Height > 13) {
				g.FillPie (dot_brush, bounds.X + lineWidth, bounds.Y + lineWidth, bounds.Width - lineWidth * 2, bounds.Height - lineWidth * 2, 0, 359);
			} else {
				int x_half_pos = (bounds.Width / 2) + bounds.X;
				int y_half_pos = (bounds.Height / 2) + bounds.Y;

				g.DrawLine (dot_pen, x_half_pos - 1, y_half_pos, x_half_pos + 2, y_half_pos);
				g.DrawLine (dot_pen, x_half_pos - 1, y_half_pos + 1, x_half_pos + 2, y_half_pos + 1);

				g.DrawLine (dot_pen, x_half_pos, y_half_pos - 1, x_half_pos, y_half_pos + 2);
				g.DrawLine (dot_pen, x_half_pos + 1, y_half_pos - 1, x_half_pos + 1, y_half_pos + 2);
			}
		}
		#endregion
	}
}
