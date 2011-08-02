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

namespace System.Windows.Forms
{
    public class DrawListViewSubItemEventArgs : EventArgs
    {
        #region Private Fields

        private Rectangle bounds;
        private int columnIndex;
		private bool drawDefault;
        private Graphics graphics;
        private ColumnHeader header;
        private ListViewItem item;
        private int itemIndex;
        private ListViewItemStates itemState;
        private ListViewItem.ListViewSubItem subItem;

        #endregion Private Fields


        #region Properties

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

        public Graphics Graphics {
            get { return graphics; }
        }

        public ColumnHeader Header {
            get { return header; }
        }

        public ListViewItem Item {
            get { return item; }
        }

        public int ItemIndex {
            get { return itemIndex; }
        }

        public ListViewItemStates ItemState {
            get { return itemState; }
        }

        public ListViewItem.ListViewSubItem SubItem {
            get { return this.subItem; }
        }

        #endregion Properties 


        #region Constructors

        public DrawListViewSubItemEventArgs(Graphics graphics, Rectangle bounds,
                                            ListViewItem item, ListViewItem.ListViewSubItem subItem,
                                            int itemIndex, int columnIndex,
                                            ColumnHeader header, ListViewItemStates itemState)
        {
            this.bounds = bounds;
            this.columnIndex = columnIndex;
            this.graphics = graphics;
            this.header = header;
            this.item = item;
            this.itemIndex = itemIndex;
            this.itemState = itemState;
            this.subItem = subItem;
        }

        #endregion Constructors


        #region Public Methods

        public void DrawBackground ()
        {
		graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (subItem.BackColor), bounds);
        }

        public void DrawFocusRectangle (Rectangle bounds)
        {
		if ((itemState & ListViewItemStates.Focused) != 0) {
			Rectangle rect = new Rectangle (bounds.X + 1, bounds.Y + 1, bounds.Width - 1, bounds.Height - 1);
			ThemeEngine.Current.CPDrawFocusRectangle (graphics, rect, subItem.ForeColor, subItem.BackColor);
		}
        }

        public void DrawText ()
        {
		DrawText (TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter);
        }

        public void DrawText (TextFormatFlags flags)
        {
		// Text adjustments
		Rectangle text_bounds = new Rectangle (bounds.X + 8, bounds.Y, bounds.Width - 13, bounds.Height);
		TextRenderer.DrawText (graphics, subItem.Text, subItem.Font, text_bounds, subItem.ForeColor, flags);
        }

        #endregion Public Methods
    }
}
