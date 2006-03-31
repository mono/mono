//
// flatten.cs
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Samples {
	public partial class flatten: Form {

		// default values
		private float translateX = 0f;
		private float translateY = 10f;
		private float flat = 0.1f;

		private Pen default_pen;
		private Pen flat_pen;

		public flatten ()
		{
			InitializeComponent ();

			object[] shapes = Samples.Common.Shapes.GetList ();
			shapeComboBox.Items.AddRange (shapes);

			default_pen = new System.Drawing.Pen (System.Drawing.Color.Black, 2);
			flat_pen = new System.Drawing.Pen (System.Drawing.Color.Red, 1);

			translateXtextBox.Text = translateX.ToString ();
			translateYtextBox.Text = translateY.ToString ();
			flattenTextBox.Text = flat.ToString ();
		}

		private void Flattener_Paint (object sender, PaintEventArgs e)
		{
			GraphicsPath path = Samples.Common.Shapes.GetShape (shapeComboBox.SelectedIndex);
			if (path == null)
				return;

			e.Graphics.DrawPath (default_pen, path);
			int pcount = path.PointCount;

			if ((translateX != 0f) || (translateY != 0f)) {
				Matrix translateMatrix = new Matrix ();
				translateMatrix.Translate (translateX, translateY);
				path.Flatten (translateMatrix, flat);
			} else {
				path.Flatten (null, flat);
			}
			e.Graphics.DrawPath (flat_pen, path);

			int fcount = path.PointCount;
			path.Dispose ();

			infoLabel.Text = System.String.Format ("Path Points: {0}, Flat Points: {1}", pcount, fcount);
		}

		private void redrawButton_Click (object sender, EventArgs e)
		{
			if (Single.TryParse (translateXtextBox.Text, out translateX)) {
				translateXtextBox.BackColor = SystemColors.Window;
			} else {
				translateXtextBox.BackColor = Color.Red;
			}

			if (Single.TryParse (translateYtextBox.Text, out translateY)) {
				translateYtextBox.BackColor = SystemColors.Window;
			} else {
				translateYtextBox.BackColor = Color.Red;
			}

			if (Single.TryParse (flattenTextBox.Text, out flat)) {
				flattenTextBox.BackColor = SystemColors.Window;
			} else {
				flattenTextBox.BackColor = Color.Red;
			}

			Invalidate ();
			Update ();
		}

		[STAThread]
		static void Main ()
		{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new flatten ());
		}
	}
}