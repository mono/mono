//
// ToolStripItemTextRenderEventArgs.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Drawing;

namespace System.Windows.Forms
{
	public class ToolStripItemTextRenderEventArgs : ToolStripItemRenderEventArgs
	{
		private string text;
		private Color text_color;
		private ToolStripTextDirection text_direction;
		private Font text_font;
		private TextFormatFlags text_format;
		private Rectangle text_rectangle;

		#region Public Constructors
		public ToolStripItemTextRenderEventArgs (Graphics g, ToolStripItem item, string text, Rectangle textRectangle, Color textColor, Font textFont, ContentAlignment textAlign)
			: base (g, item)
		{
			this.text = text;
			this.text_rectangle = textRectangle;
			this.text_color = textColor;
			this.text_font = textFont;
			this.text_direction = item.TextDirection;

			switch (textAlign) {
				case ContentAlignment.BottomCenter:
					this.text_format = TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
					break;
				case ContentAlignment.BottomLeft:
					this.text_format = TextFormatFlags.Bottom | TextFormatFlags.Left;
					break;
				case ContentAlignment.BottomRight:
					this.text_format = TextFormatFlags.Bottom | TextFormatFlags.Right;
					break;
				case ContentAlignment.MiddleCenter:
					this.text_format = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
					break;
				case ContentAlignment.MiddleLeft:
				default:
					this.text_format = (TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
					break;
				case ContentAlignment.MiddleRight:
					this.text_format = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
					break;
				case ContentAlignment.TopCenter:
					this.text_format = TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
					break;
				case ContentAlignment.TopLeft:
					this.text_format = TextFormatFlags.Top | TextFormatFlags.Left;
					break;
				case ContentAlignment.TopRight:
					this.text_format = TextFormatFlags.Top | TextFormatFlags.Right;
					break;
			}

			if ((Application.KeyboardCapture == null || !ToolStripManager.ActivatedByKeyboard) && !SystemInformation.MenuAccessKeysUnderlined)
				this.text_format |= TextFormatFlags.HidePrefix;
		}

		public ToolStripItemTextRenderEventArgs (Graphics g, ToolStripItem item, string text, Rectangle textRectangle, Color textColor, Font textFont, TextFormatFlags format)
			: base (g, item)
		{
			this.text = text;
			this.text_rectangle = textRectangle;
			this.text_color = textColor;
			this.text_font = textFont;
			this.text_format = format;
			this.text_direction = ToolStripTextDirection.Horizontal;
		}
		#endregion

		#region Public Properties
		public string Text {
			get { return this.text; }
			set { this.text = value; }
		}

		public Color TextColor {
			get { return this.text_color; }
			set { this.text_color = value; }
		}

		public ToolStripTextDirection TextDirection {
			get { return this.text_direction; }
			set { this.text_direction = value; }
		}

		public Font TextFont {
			get { return this.text_font; }
			set { this.text_font = value; }
		}

		public TextFormatFlags TextFormat {
			get { return this.text_format; }
			set { this.text_format = value; }
		}

		public Rectangle TextRectangle {
			get { return this.text_rectangle; }
			set { this.text_rectangle = value; }
		}
		#endregion
	}
}
