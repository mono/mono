//
// binary.cs
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

	public partial class binary: Form {

		private Region r1;
		private Region r2;
		private Region op;
		private Graphics gfx;

		public binary ()
		{
			InitializeComponent ();

			object[] shapes = Samples.Common.Shapes.GetList ();
			shape1ComboBox.Items.AddRange (shapes);
			shape2ComboBox.Items.AddRange (shapes);

			object[] ops = new object[] {
				"Union",
				"Intersect",
				"Exclude",
				"Complement",
				"Xor"
			};
			shape3comboBox.Items.AddRange (ops);

			object[] matrices = Samples.Common.Matrices.GetList ();
			matrix1comboBox.Items.AddRange (matrices);
			matrix2comboBox.Items.AddRange (matrices);
			matrix3comboBox.Items.AddRange (matrices);
		}

		private void Form1_Paint (object sender, PaintEventArgs e)
		{
			gfx = e.Graphics;

			if (shape1checkBox.Checked && (r1 != null))
				gfx.FillRegion (Brushes.Red, r1);

			if (shape2checkBox.Checked && (r2 != null))
				gfx.FillRegion (Brushes.Green, r2);

			if (shape3checkBox.Checked && (op != null))
				gfx.FillRegion (Brushes.Blue, op);
		}

		private void resetButton_Click (object sender, EventArgs e)
		{
			shape1ComboBox.SelectedIndex = -1;
			if (r1 != null) {
				r1.Dispose ();
				r1 = null;
			}

			shape2ComboBox.SelectedIndex = -1;
			if (r2 != null) {
				r2.Dispose ();
				r2 = null;
			}

			if (op != null) {
				op.Dispose ();
				op = null;
			}

			UpdateUI ();
		}

		private void UpdateUI ()
		{
			Invalidate ();
			Update ();
		}

		private GraphicsPath GetShape (ComboBox cb)
		{
			return Samples.Common.Shapes.GetShape (cb.SelectedIndex);
		}

		private void UpdateShape1 ()
		{
			GraphicsPath path = GetShape (shape1ComboBox);
			if (path != null) {
				r1 = new Region (path);
				path.Dispose ();
			}
		}

		private void shape1_Changed (object sender, EventArgs e)
		{
			UpdateShape1 ();
			if (op != null) {
				op.Dispose ();
				op = null;
			}

			UpdateUI ();
		}

		private void UpdateShape2 ()
		{
			GraphicsPath path = GetShape (shape2ComboBox);
			if (path != null) {
				r2 = new Region (path);
				path.Dispose ();
			}
		}

		private void shape2_Changed (object sender, EventArgs e)
		{
			UpdateShape2 ();
			if (op != null) {
				op.Dispose ();
				op = null;
			}

			UpdateUI ();
		}

		private void UpdateShape3 ()
		{
			if (shape3comboBox.SelectedIndex == -1)
				return;

			if (r1 != null) {
				if (op != null)
					op.Dispose ();
				op = r1.Clone ();
				if (r2 != null) {

					switch (shape3comboBox.SelectedIndex) {
					case 0:
						op.Union (r2);
						break;
					case 1:
						op.Intersect (r2);
						break;
					case 2:
						op.Exclude (r2);
						break;
					case 3:
						op.Complement (r2);
						break;
					case 4:
						op.Xor (r2);
						break;
					}
				}
			}
		}

		private void shape3_SelectedIndexChanged (object sender, EventArgs e)
		{
			UpdateShape3 ();
			UpdateUI ();
		}


		private Matrix GetMatrix (ComboBox cb)
		{
			return Samples.Common.Matrices.GetMatrix (cb.SelectedIndex);
		}

		private void matrix1_Changed (object sender, EventArgs e)
		{
			if (r1 != null) {
				// an earlier matrix could be applied
				UpdateShape1 ();
				Matrix m = GetMatrix (matrix1comboBox);
				if (m != null) {
					r1.Transform (m);
					m.Dispose ();
				}
				// this can also affects the resulting operation
				matrix3_Changed (sender, e);
				UpdateUI ();
			}
		}

		private void matrix2_Changed (object sender, EventArgs e)
		{
			if (r2 != null) {
				// an earlier matrix could be applied
				UpdateShape2 ();
				Matrix m = GetMatrix (matrix2comboBox);
				if (m != null) {
					r2.Transform (m);
					m.Dispose ();
				}
				// this can also affects the resulting operation
				matrix3_Changed (sender, e);
				UpdateUI ();
			}
		}

		private void matrix3_Changed (object sender, EventArgs e)
		{
			if (op != null) {
				// an earlier matrix could be applied
				UpdateShape3 ();
				Matrix m = GetMatrix (matrix3comboBox);
				if (m != null) {
					op.Transform (m);
					m.Dispose ();
				}
				UpdateUI ();
			}
		}

		private void OnDisplayChanged (object sender, EventArgs e)
		{
			UpdateUI ();
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			string msg = System.String.Empty;
			if (r1 != null) {
				if (r1.IsVisible (e.X, e.Y, gfx))
					msg = "shape1 ";
			}
			if (r2 != null) {
				if (r2.IsVisible (e.X, e.Y, gfx))
					msg += "shape2 ";
			}
			if (op != null) {
				if (op.IsVisible (e.X, e.Y, gfx))
					msg += "operation";
			}

			if (msg.Length > 0) {
				infoLabel.Text = System.String.Format ("Click ({0},{1}) is inside: {2}",
					e.X, e.Y, msg);
			} else {
				infoLabel.Text = System.String.Format ("Click ({0},{1}) is outside any region",
					e.X, e.Y);
			}

			base.OnMouseDown (e);
		}

		[STAThread]
		static void Main ()
		{
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			Application.Run (new binary ());
		}
	}
}