//
// clippy.cs
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
	public partial class Clippy: Form {

		private GraphicsPath path;
		private Region clipping_region;

		public Clippy ()
		{
			InitializeComponent ();

			object[] shapes = Samples.Common.Shapes.GetList ();
			shapeComboBox.Items.AddRange (shapes);
			clippingComboBox.Items.AddRange (shapes);
		}

		public bool Clip {
			get { return clipCheckBox.Checked; }
		}

		public bool ShowClippingRegion {
			get { return showRegionCheckBox.Checked; }
		}

		public GraphicsPath Path {
			get {
				if (path == null) {
					path =  Samples.Common.Shapes.GetShape (clippingComboBox.SelectedIndex);
					Matrix m = new Matrix ();
					m.Translate (20, 20);
					path.Transform (m);
				}
				return path;
			}
		}

		public Region ClippingRegion {
			get {
				if (clipping_region == null) {
					clipping_region = new Region (Path);
				}
				return clipping_region;
			}
		}

		private void Form1_Paint (object sender, PaintEventArgs e)
		{
			Region original = null;
			if (ShowClippingRegion) {
				e.Graphics.DrawPath (Pens.Red, Path);
			}

			if (Clip) {
				original = e.Graphics.Clip;
				e.Graphics.Clip = ClippingRegion;
			}

			GraphicsPath shape = Samples.Common.Shapes.GetShape (shapeComboBox.SelectedIndex);
			if (shape == null)
				return;
			shape.FillMode = fillModecheckBox.Checked ? FillMode.Winding : FillMode.Alternate;

			e.Graphics.FillPath (Brushes.Blue, shape);

			if (original != null) {
				e.Graphics.Clip.Dispose ();
				e.Graphics.Clip = original;
			}
		}

		private void UpdateDisplay (object sender, EventArgs e)
		{
			Invalidate ();
			Update ();
		}

		private void UpdateShapes (object sender, EventArgs e)
		{
			path = null;
			clipping_region = null;
			UpdateDisplay (sender, e);
		}

		[STAThread]
		static void Main ()
		{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new Clippy ());
		}
	}
}