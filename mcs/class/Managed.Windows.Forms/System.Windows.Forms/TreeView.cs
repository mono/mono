//
// System.Windows.Forms.TreeNode.cs
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
// Autors:
//		Marek Safar		marek.safar@seznam.cz
//
//
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class TreeView: Control
	{
		BorderStyle border_style = BorderStyle.Fixed3D;

		public TreeView()
		{
			background_color = ThemeEngine.Current.DefaultWindowBackColor;

			base.Paint += new PaintEventHandler (TreeView_Paint);

			SetStyle (ControlStyles.ResizeRedraw, true);
		}

		#region Properties

		public override Color BackColor {
			get {
				return background_color;
			}
			set {
				if (value == background_color)
					return;

				background_color = value;
//				if (BackColorChanged != null)
//					BackColorChanged (this, new EventArgs ());
//				Redraw (false);
			}
		}

		[DefaultValue (BorderStyle.Fixed3D)]
		public BorderStyle BorderStyle {
			get { 
				return border_style;
			}
			set {
				if (border_style != value) {
					border_style = value;
					//this.Redraw (false);
				}
			}
		}

		protected override System.Drawing.Size DefaultSize {
			get {
				return ThemeEngine.Current.TreeViewDefaultSize;
			}
		}

		#endregion

		void TreeView_Paint (object sender, PaintEventArgs pe)
		{
			if (this.Width <= 0 || this.Height <=  0 ||
				this.Visible == false)
				return;

			DrawTreeView (pe.Graphics, this);
			ThemeEngine.Current.CPDrawBorderStyle (pe.Graphics, this.ClientRectangle, border_style);
		}

		void DrawTreeView (Graphics dc, Control control)
		{
			dc.Clear (control.BackColor);
		}
	}
}
