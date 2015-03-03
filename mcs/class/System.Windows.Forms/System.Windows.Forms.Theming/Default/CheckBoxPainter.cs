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
	internal class CheckBoxPainter
	{
		public CheckBoxPainter ()
		{
		}

		protected SystemResPool ResPool { get { return ThemeEngine.Current.ResPool; } }

		public void PaintCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, ElementState state, FlatStyle style, CheckState checkState)
		{
			switch (style) {
				case FlatStyle.Standard:
				case FlatStyle.System:
					switch (state) {
						case ElementState.Normal:
							DrawNormalCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Hot:
							DrawHotCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Pressed:
							DrawPressedCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Disabled:
							DrawDisabledCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
					}
					break;
				case FlatStyle.Flat:
					switch (state) {
						case ElementState.Normal:
							DrawFlatNormalCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Hot:
							DrawFlatHotCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Pressed:
							DrawFlatPressedCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Disabled:
							DrawFlatDisabledCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
					}
					break;
				case FlatStyle.Popup:
					switch (state) {
						case ElementState.Normal:
							DrawPopupNormalCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Hot:
							DrawPopupHotCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Pressed:
							DrawPopupPressedCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
						case ElementState.Disabled:
							DrawPopupDisabledCheckBox (g, bounds, backColor, foreColor, checkState);
							break;
					}
					break;
			}
		}

		#region Standard
		public virtual void DrawNormalCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			int check_box_visible_size = (bounds.Height > bounds.Width) ? bounds.Width : bounds.Height;
			int x_pos = Math.Max (0, bounds.X + (bounds.Width / 2) - check_box_visible_size / 2);
			int y_pos = Math.Max (0, bounds.Y + (bounds.Height / 2) - check_box_visible_size / 2);

			Rectangle rect = new Rectangle (x_pos, y_pos, check_box_visible_size, check_box_visible_size);

			g.FillRectangle (SystemBrushes.ControlLightLight, rect.X + 2, rect.Y + 2, rect.Width - 3, rect.Height - 3);

			Pen pen = SystemPens.ControlDark;
			g.DrawLine (pen, rect.X, rect.Y, rect.X, rect.Bottom - 2);
			g.DrawLine (pen, rect.X + 1, rect.Y, rect.Right - 2, rect.Y);

			pen = SystemPens.ControlDarkDark;
			g.DrawLine (pen, rect.X + 1, rect.Y + 1, rect.X + 1, rect.Bottom - 3);
			g.DrawLine (pen, rect.X + 2, rect.Y + 1, rect.Right - 3, rect.Y + 1);

			pen = SystemPens.ControlLightLight;
			g.DrawLine (pen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1);
			g.DrawLine (pen, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);

			// oh boy, matching ms is like fighting against windmills
			using (Pen h_pen = new Pen (ResPool.GetHatchBrush (HatchStyle.Percent50,
									   Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255),
											   ColorControl.G, ColorControl.B), ColorControl))) {
				g.DrawLine (h_pen, rect.X + 1, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
				g.DrawLine (h_pen, rect.Right - 2, rect.Y + 1, rect.Right - 2, rect.Bottom - 2);
			}
			
			if (state == CheckState.Checked)
				DrawCheck (g, bounds, Color.Black);
			else if (state == CheckState.Indeterminate)
				DrawCheck (g, bounds, SystemColors.ControlDark);
		}
		
		public virtual void DrawHotCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			DrawNormalCheckBox (g, bounds, backColor, foreColor, state);
		}

		public virtual void DrawPressedCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			int check_box_visible_size = (bounds.Height > bounds.Width) ? bounds.Width : bounds.Height;
			int x_pos = Math.Max (0, bounds.X + (bounds.Width / 2) - check_box_visible_size / 2);
			int y_pos = Math.Max (0, bounds.Y + (bounds.Height / 2) - check_box_visible_size / 2);

			Rectangle rect = new Rectangle (x_pos, y_pos, check_box_visible_size, check_box_visible_size);

			g.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50,
								 Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255),
										 ColorControl.G, ColorControl.B),
								 ColorControl), rect.X + 2, rect.Y + 2, rect.Width - 3, rect.Height - 3);

			Pen pen = SystemPens.ControlDark;
			g.DrawLine (pen, rect.X, rect.Y, rect.X, rect.Bottom - 2);
			g.DrawLine (pen, rect.X + 1, rect.Y, rect.Right - 2, rect.Y);

			pen = SystemPens.ControlDarkDark;
			g.DrawLine (pen, rect.X + 1, rect.Y + 1, rect.X + 1, rect.Bottom - 3);
			g.DrawLine (pen, rect.X + 2, rect.Y + 1, rect.Right - 3, rect.Y + 1);

			pen = SystemPens.ControlLightLight;
			g.DrawLine (pen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1);
			g.DrawLine (pen, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);

			// oh boy, matching ms is like fighting against windmills
			using (Pen h_pen = new Pen (ResPool.GetHatchBrush (HatchStyle.Percent50,
									   Color.FromArgb (Clamp (ColorControl.R + 3, 0, 255),
											   ColorControl.G, ColorControl.B), ColorControl))) {
				g.DrawLine (h_pen, rect.X + 1, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
				g.DrawLine (h_pen, rect.Right - 2, rect.Y + 1, rect.Right - 2, rect.Bottom - 2);
			}

			if (state == CheckState.Checked)
				DrawCheck (g, bounds, Color.Black);
			else if (state == CheckState.Indeterminate)
				DrawCheck (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawDisabledCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			DrawPressedCheckBox (g, bounds, backColor, foreColor, CheckState.Unchecked);

			if (state == CheckState.Checked || state == CheckState.Indeterminate)
				DrawCheck (g, bounds, SystemColors.ControlDark);
		}
		#endregion

		#region FlatStyle
		public virtual void DrawFlatNormalCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			Rectangle checkbox_rectangle;
			Rectangle fill_rectangle;

			// set up our rectangles first
			// clip two pixels from bottom right for non popup rendered checkboxes
			checkbox_rectangle = new Rectangle (bounds.X, bounds.Y, Math.Max (bounds.Width - 2, 0), Math.Max (bounds.Height - 2, 0));
			fill_rectangle = new Rectangle (checkbox_rectangle.X + 1, checkbox_rectangle.Y + 1, Math.Max (checkbox_rectangle.Width - 2, 0), Math.Max (checkbox_rectangle.Height - 2, 0));

			g.FillRectangle (ResPool.GetSolidBrush (ControlPaint.LightLight (backColor)), fill_rectangle);
			ControlPaint.DrawBorder (g, checkbox_rectangle, foreColor, ButtonBorderStyle.Solid);

			bounds.Offset (-1, 0);
			
			if (state == CheckState.Checked)
				DrawCheck (g, bounds, Color.Black);
			else if (state == CheckState.Indeterminate)
				DrawCheck (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawFlatHotCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			Rectangle checkbox_rectangle;
			Rectangle fill_rectangle;

			// set up our rectangles first
			// clip two pixels from bottom right for non popup rendered checkboxes
			checkbox_rectangle = new Rectangle (bounds.X, bounds.Y, Math.Max (bounds.Width - 2, 0), Math.Max (bounds.Height - 2, 0));
			fill_rectangle = new Rectangle (checkbox_rectangle.X + 1, checkbox_rectangle.Y + 1, Math.Max (checkbox_rectangle.Width - 2, 0), Math.Max (checkbox_rectangle.Height - 2, 0));

			g.FillRectangle (ResPool.GetSolidBrush (backColor), fill_rectangle);
			ControlPaint.DrawBorder (g, checkbox_rectangle, foreColor, ButtonBorderStyle.Solid);

			bounds.Offset (-1, 0);

			if (state == CheckState.Checked)
				DrawCheck (g, bounds, Color.Black);
			else if (state == CheckState.Indeterminate)
				DrawCheck (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawFlatPressedCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			DrawFlatNormalCheckBox (g, bounds, backColor, foreColor, state);
		}

		public virtual void DrawFlatDisabledCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			Rectangle checkbox_rectangle;

			checkbox_rectangle = new Rectangle (bounds.X, bounds.Y, Math.Max (bounds.Width - 2, 0), Math.Max (bounds.Height - 2, 0));

			ControlPaint.DrawBorder (g, checkbox_rectangle, foreColor, ButtonBorderStyle.Solid);

			bounds.Offset (-1, 0);

			if (state == CheckState.Checked || state == CheckState.Indeterminate)
				DrawCheck (g, bounds, SystemColors.ControlDarkDark);
		}
		#endregion

		#region Popup
		public virtual void DrawPopupNormalCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			DrawFlatNormalCheckBox (g, bounds, backColor, foreColor, state);
		}

		public virtual void DrawPopupHotCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			Rectangle checkbox_rectangle;
			Rectangle fill_rectangle;

			// clip one pixel from bottom right for non popup rendered checkboxes
			checkbox_rectangle = new Rectangle (bounds.X, bounds.Y, Math.Max (bounds.Width - 1, 0), Math.Max (bounds.Height - 1, 0));
			fill_rectangle = new Rectangle (checkbox_rectangle.X + 1, checkbox_rectangle.Y + 1, Math.Max (checkbox_rectangle.Width - 3, 0), Math.Max (checkbox_rectangle.Height - 3, 0));

			g.FillRectangle (ResPool.GetSolidBrush (ControlPaint.LightLight (backColor)), fill_rectangle);
			
			// draw sunken effect
			ThemeEngine.Current.CPDrawBorder3D (g, checkbox_rectangle, Border3DStyle.SunkenInner, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, backColor);

			bounds.Offset (-1, 0);
			
			if (state == CheckState.Checked)
				DrawCheck (g, bounds, Color.Black);
			else if (state == CheckState.Indeterminate)
				DrawCheck (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawPopupPressedCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			Rectangle checkbox_rectangle;
			Rectangle fill_rectangle;

			// clip one pixel from bottom right for non popup rendered checkboxes
			checkbox_rectangle = new Rectangle (bounds.X, bounds.Y, Math.Max (bounds.Width - 1, 0), Math.Max (bounds.Height - 1, 0));
			fill_rectangle = new Rectangle (checkbox_rectangle.X + 1, checkbox_rectangle.Y + 1, Math.Max (checkbox_rectangle.Width - 3, 0), Math.Max (checkbox_rectangle.Height - 3, 0));

			g.FillRectangle (ResPool.GetSolidBrush (backColor), fill_rectangle);

			// draw sunken effect
			ThemeEngine.Current.CPDrawBorder3D (g, checkbox_rectangle, Border3DStyle.SunkenInner, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, backColor);

			bounds.Offset (-1, 0);

			if (state == CheckState.Checked)
				DrawCheck (g, bounds, Color.Black);
			else if (state == CheckState.Indeterminate)
				DrawCheck (g, bounds, SystemColors.ControlDarkDark);
		}

		public virtual void DrawPopupDisabledCheckBox (Graphics g, Rectangle bounds, Color backColor, Color foreColor, CheckState state)
		{
			DrawFlatDisabledCheckBox (g, bounds, backColor, foreColor, state);
		}
		#endregion

		#region Check
		public virtual void DrawCheck (Graphics g, Rectangle bounds, Color checkColor)
		{
			int check_size = (bounds.Height > bounds.Width) ? bounds.Width / 2 : bounds.Height / 2;

			Pen check_pen = ResPool.GetPen (checkColor);

			if (check_size < 7) {
				int lineWidth = Math.Max (3, check_size / 3);
				int Scale = Math.Max (1, check_size / 9);

				Rectangle rect = new Rectangle (bounds.X + (bounds.Width / 2) - (check_size / 2) - 1, bounds.Y + (bounds.Height / 2) - (check_size / 2) - 1,
								check_size, check_size);

				for (int i = 0; i < lineWidth; i++) {
					g.DrawLine (check_pen, rect.Left + lineWidth / 2, rect.Top + lineWidth + i, rect.Left + lineWidth / 2 + 2 * Scale, rect.Top + lineWidth + 2 * Scale + i);
					g.DrawLine (check_pen, rect.Left + lineWidth / 2 + 2 * Scale, rect.Top + lineWidth + 2 * Scale + i, rect.Left + lineWidth / 2 + 6 * Scale, rect.Top + lineWidth - 2 * Scale + i);
				}
			} else {
				int lineWidth = Math.Max (3, check_size / 3) + 1;

				int x_half = bounds.Width / 2;
				int y_half = bounds.Height / 2;

				Rectangle rect = new Rectangle (bounds.X + x_half - (check_size / 2) - 1, bounds.Y + y_half - (check_size / 2),
								check_size, check_size);

				int gradient_left = check_size / 3;
				int gradient_right = check_size - gradient_left - 1;

				for (int i = 0; i < lineWidth; i++) {
					g.DrawLine (check_pen, rect.X, rect.Bottom - 1 - gradient_left - i, rect.X + gradient_left, rect.Bottom - 1 - i);
					g.DrawLine (check_pen, rect.X + gradient_left, rect.Bottom - 1 - i, rect.Right - 1, rect.Bottom - i - 1 - gradient_right);
				}
			}
		}
		#endregion
		
		#region Private Methods
		private int Clamp (int value, int lower, int upper)
		{
			if (value < lower) return lower;
			else if (value > upper) return upper;
			else return value;
		}

		private Color ColorControl {
			get { return SystemColors.Control; }
		}
		#endregion
	}
}
