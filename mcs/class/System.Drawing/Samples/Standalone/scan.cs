//
// scan.cs
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
	public partial class scan: Form {

		private Region region;
		private Matrix matrix;
		private RectangleF[] scans;

		public scan ()
		{
			InitializeComponent ();

			object[] shapes = Samples.Common.Shapes.GetList ();
			shapeComboBox.Items.AddRange (shapes);

			object[] matrices = Samples.Common.Matrices.GetList ();
			matrixComboBox.Items.AddRange (matrices);
		}

		private void Form1_Paint (object sender, PaintEventArgs e)
		{
			if (region != null)
				e.Graphics.FillRegion (Brushes.Blue, region);

			if (scansCheckBox.Checked && (scans != null) && (scans.Length > 0)) {
				e.Graphics.DrawRectangles (Pens.Red, scans);
			}
		}

		private GraphicsPath GetShape (ComboBox cb)
		{
			return Samples.Common.Shapes.GetShape (cb.SelectedIndex);
		}

		private Matrix Matrix {
			get {
				if (matrix == null)
					matrix = new Matrix ();
				return matrix;
			}
		}

		private void OnShapeChange (object sender, EventArgs e)
		{
			GraphicsPath path = GetShape (shapeComboBox);
			if (path != null) {
				if (region != null) {
					region.Dispose ();
				}
				region = new Region (path);
				scans = region.GetRegionScans (Matrix);

				infoLabel.Text = System.String.Format ("{0} rectangles to re-create the shape.", scans.Length);
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < scans.Length; i++) {
					sb.AppendFormat ("{0}: x {1}, y {2}, w {3}, h {4}{5}", i,
						scans[i].X, scans[i].Y, scans[i].Width, scans[i].Height, 
						Environment.NewLine);
				}
				scansTextBox.Text = sb.ToString ();
			}
			UpdateUI ();
		}

		private void OnMatrixChange (object sender, EventArgs e)
		{
			if (matrix != null) {
				matrix.Dispose ();
			}

			matrix = Samples.Common.Matrices.GetMatrix (matrixComboBox.SelectedIndex);

			OnShapeChange (sender, e);
		}

		private void UpdateUI ()
		{
			Invalidate ();
			Update ();
		}

		private void OnDisplayScans (object sender, EventArgs e)
		{
			UpdateUI ();
		}

		[STAThread]
		static void Main ()
		{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new scan ());
		}
	}
}