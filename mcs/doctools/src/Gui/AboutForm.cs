// AboutForm.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

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
