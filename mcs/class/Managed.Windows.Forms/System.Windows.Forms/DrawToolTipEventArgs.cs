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
//	Rolf Bjarne Kvinge  <RKvinge@novell.com>
//
//
// COMPLETE


using System.Drawing;

namespace System.Windows.Forms
{
	public class DrawToolTipEventArgs : EventArgs
	{
		private Control associated_control;
		private IWin32Window associated_window;
		private Color back_color;
		private Font font;
		private Rectangle bounds;
		private Color fore_color;
		private Graphics graphics;
		private string tooltip_text;

		public DrawToolTipEventArgs (Graphics graphics, IWin32Window associatedWindow, Control associatedControl, Rectangle bounds, string toolTipText, Color backColor, Color foreColor, Font font)
		{
			this.graphics = graphics;
			this.associated_window = associatedWindow;
			this.associated_control = associatedControl;
			this.bounds = bounds;
			this.tooltip_text = toolTipText;
			this.back_color = backColor;
			this.fore_color = foreColor;
			this.font = font;
		}

		public void DrawBackground ()
		{
			graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (back_color), bounds);
		}

		public void DrawBorder ()
		{
			ControlPaint.DrawBorder (graphics, bounds, SystemColors.WindowFrame, ButtonBorderStyle.Solid);
		}

		public void DrawText ()
		{
			DrawText (TextFormatFlags.HidePrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
		}

		public void DrawText (TextFormatFlags flags)
		{
			TextRenderer.DrawTextInternal (graphics, tooltip_text, font, bounds, fore_color, flags, false);
		}

		public Control AssociatedControl {
			get {
				return associated_control;
			}
		}

		public IWin32Window AssociatedWindow {
			get {
				return associated_window;
			}
		}

		public Rectangle Bounds {
			get {
				return bounds;
			}
		}

		public Font Font {
			get {
				return font;
			}
		}

		public Graphics Graphics {
			get {
				return graphics;
			}
		}

		public string ToolTipText {
			get {
				return tooltip_text;
			}
		}
	}
}