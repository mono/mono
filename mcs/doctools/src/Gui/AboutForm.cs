// AboutForm.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	public class AboutForm : Form
	{
		private PictureBox aboutLogo;
		private Button     closeButton;

		public AboutForm()
		{
			aboutLogo   = new PictureBox();
			closeButton = new Button();

			this.SuspendLayout();

			// aboutLogo properties
			aboutLogo.Dock  = DockStyle.Fill;
			aboutLogo.Image = GuiResources.AboutMonodocBitmap;
			aboutLogo.Name  = "aboutLogo";
			aboutLogo.Size  = new Size(300, 300);

			// closeButton properties
			closeButton.Location = new Point(96, 272);
			closeButton.Name     = "closeButton";
			closeButton.Size     = new Size(104, 24);
			closeButton.TabIndex = 0;
			closeButton.Text     = GuiResources.GetString("Buttons.Close");

			
			// form properties
			this.AutoScaleBaseSize = new Size(5, 13);
			this.BackColor         = Color.White;
			this.ClientSize        = new Size(300, 300);
			this.FormBorderStyle   = FormBorderStyle.FixedSingle;
			this.Icon              = GuiResources.OpenBookIcon;
			this.MaximizeBox       = false;
			this.MinimizeBox       = false;
			this.Name              = "AboutForm";
			this.ShowInTaskbar     = false;
			this.StartPosition     = FormStartPosition.CenterParent;
			this.Text              = GuiResources.GetString("Form.About.Title");

			// bind events
			closeButton.Click += new EventHandler(closeButton_Click);

			// add components
			this.Controls.AddRange(new Control[] { closeButton, aboutLogo});

			this.ResumeLayout(false);
		}

		// events
		private void closeButton_Click(object sender, EventArgs args)
		{
			this.Close();
		}
	}
}
