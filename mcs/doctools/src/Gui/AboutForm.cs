//   File: AboutForm.cs
//   Desc: 'About Monodoc' dialog box.
// Author: John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
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
		private PictureBox mAboutLogo;
		private Button     mCloseButton;

		public AboutForm()
		{
			mAboutLogo   = new PictureBox();
			mCloseButton = new Button();

			this.SuspendLayout();

			// mAboutLogo properties
			mAboutLogo.Dock  = DockStyle.Fill;
			mAboutLogo.Image = GuiResources.AboutMonodocBitmap;
			mAboutLogo.Name  = "mAboutLogo";
			mAboutLogo.Size  = new Size(300, 300);

			// mCloseButton properties
			mCloseButton.Location = new Point(96, 272);
			mCloseButton.Name     = "mCloseButton";
			mCloseButton.Size     = new Size(104, 24);
			mCloseButton.TabIndex = 0;
			mCloseButton.Text     = GuiResources.GetString("Buttons.Close");

			
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
			mCloseButton.Click += new EventHandler(mCloseButton_Click);

			// add components
			this.Controls.AddRange(new Control[] { mCloseButton, mAboutLogo});

			this.ResumeLayout(false);
		}

		private void mCloseButton_Click(object sender, EventArgs args)
		{
			this.Close();
		}
	}
}
