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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Alan McGovern (alan.mcgovern@gmail.com)
//


using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
    public class DrawListViewColumnHeaderEventArgs : EventArgs
    {
        #region Private Fields

        private Color backColor;
        private Rectangle bounds;
        private int columnIndex;
        private bool drawDefault;
        private Font font;
        private Color foreColor;
        private Graphics graphics;
        private ColumnHeader header;
        private ListViewItemStates state;

        #endregion Private Fields


        #region Properties

        public Color BackColor {
            get { return backColor; }
        }

        public Rectangle Bounds {
            get { return bounds; }
        }

        public int ColumnIndex {
            get { return columnIndex; }
        }

        public bool DrawDefault {
            get { return drawDefault; }
            set { drawDefault = value; }
        }

        public Font Font {
            get { return font; }
        }

        public Color ForeColor {
            get { return foreColor; }
        }

        public Graphics Graphics {
            get { return graphics; }
        }

        public ColumnHeader Header {
            get { return header; }
        }

        public ListViewItemStates State {
            get { return state; }
        }

        #endregion Properties


        #region Constructors

        public DrawListViewColumnHeaderEventArgs(Graphics graphics, Rectangle bounds, int columnIndex,
                                        ColumnHeader header, ListViewItemStates state, Color foreColor,
                                        Color backColor, Font font)
        {
            this.backColor = backColor;
            this.bounds = bounds;
            this.columnIndex = columnIndex;
            this.font = font;
            this.foreColor = foreColor;
            this.graphics = graphics;
            this.header = header;
            this.state = state;
        }

        #endregion Constructors


        #region Methods

        public void DrawBackground ()
        {
		// Always draw a non-pushed button
		ThemeEngine.Current.CPDrawButton (graphics, bounds, ButtonState.Normal);
        }

        public void DrawText ()
        {
		DrawText (TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        }

        public void DrawText (TextFormatFlags flags)
        {
		// Text adjustments
		Rectangle text_bounds = new Rectangle (bounds.X + 8, bounds.Y, bounds.Width - 13, bounds.Height);
		TextRenderer.DrawText (graphics, header.Text, font, text_bounds, foreColor, flags);
        }

        #endregion Methods
    }
}
