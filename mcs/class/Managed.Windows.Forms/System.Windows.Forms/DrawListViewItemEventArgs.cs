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
    public class DrawListViewItemEventArgs : EventArgs
    {
        #region Private Fields

        private Rectangle bounds;
        private bool drawDefault;
        private Graphics graphics;
        private ListViewItem item;
        private int itemIndex;
        private ListViewItemStates state;

        #endregion Private Fields


        #region Properties

        public bool DrawDefault {
            get { return drawDefault; }
            set { drawDefault = value; }
        }

        public Rectangle Bounds {
            get { return bounds; }
        }

        public Graphics Graphics {
            get { return graphics; }
        }

        public ListViewItem Item {
            get { return item; }
        }

        public int ItemIndex {
            get { return itemIndex; }
        }

        public ListViewItemStates State {
            get { return state; }
        }

        #endregion Properties


        #region Constructors

        public DrawListViewItemEventArgs (Graphics graphics, ListViewItem item,
                                        Rectangle bounds, int itemIndex, ListViewItemStates state)
        {
            this.graphics = graphics;
            this.item = item;
            this.bounds = bounds;
            this.itemIndex = itemIndex;
            this.state = state;
        }

        #endregion Constructors


        #region Public Methods

        public void DrawBackground ()
        {
		graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (item.BackColor), bounds);
        }

        public void DrawFocusRectangle ()
        {
		if ((state & ListViewItemStates.Focused) != 0)
			ThemeEngine.Current.CPDrawFocusRectangle (graphics, bounds, item.ListView.ForeColor, item.ListView.BackColor);
        }

        public void DrawText ()
        {
		DrawText (TextFormatFlags.Default);
        }

        public void DrawText (TextFormatFlags flags)
        {
		TextRenderer.DrawText (graphics, item.Text, item.Font, bounds, item.ForeColor, flags);
        }

        #endregion Public Methods
    }
}
