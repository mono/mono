// UnexpectedErrorForm.cs
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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	/// <summary>
	/// Summary description for UnexpectedErrorForm.
	/// </summary>
	public class UnexpectedErrorForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox pictureBoxExplosion;
		private System.Windows.Forms.Label labelErrorExplanation;
		private System.Windows.Forms.TextBox textBoxErrorMessage;
		private System.Windows.Forms.Button buttonSendErrorReport;
		private System.Windows.Forms.Button buttonQuit;
		private System.Windows.Forms.Button buttonContinue;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public UnexpectedErrorForm(Exception e)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			this.pictureBoxExplosion.Image = GuiResources.ErrorExplosionBitmap;

			this.textBoxErrorMessage.Text +=
				"Exception: " + e.GetType().FullName + "\n" +
				"Message: " + e.Message + "\n\n" +
				"Stack Trace: \n" + e.StackTrace + "\n\n";

			while (e.InnerException != null)
			{
				e = e.InnerException;
				this.textBoxErrorMessage.Text +=
					"--- NESTED EXCEPTION ---\n" +
					"Exception: " + e.GetType().FullName + "\n" +
					"Message: " + e.Message + "\n\n" +
					"Stack Trace: \n" + e.StackTrace + "\n\n";
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBoxExplosion = new System.Windows.Forms.PictureBox();
			this.labelErrorExplanation = new System.Windows.Forms.Label();
			this.textBoxErrorMessage = new System.Windows.Forms.TextBox();
			this.buttonQuit = new System.Windows.Forms.Button();
			this.buttonSendErrorReport = new System.Windows.Forms.Button();
			this.buttonContinue = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// pictureBoxExplosion
			// 
			this.pictureBoxExplosion.Location = new System.Drawing.Point(8, 8);
			this.pictureBoxExplosion.Name = "pictureBoxExplosion";
			this.pictureBoxExplosion.Size = new System.Drawing.Size(65, 64);
			this.pictureBoxExplosion.TabIndex = 0;
			this.pictureBoxExplosion.TabStop = false;
			// 
			// labelErrorExplanation
			// 
			this.labelErrorExplanation.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.labelErrorExplanation.Location = new System.Drawing.Point(96, 8);
			this.labelErrorExplanation.Name = "labelErrorExplanation";
			this.labelErrorExplanation.Size = new System.Drawing.Size(216, 64);
			this.labelErrorExplanation.TabIndex = 1;
			this.labelErrorExplanation.Text = "Kaboom!  Monodoc has thrown an unexpected exception.  Seeing as we\'re in heavy de" +
				"velopment, this is hardly surprising.  Relevant debugging information follows.";
			// 
			// textBoxErrorMessage
			// 
			this.textBoxErrorMessage.AcceptsReturn = true;
			this.textBoxErrorMessage.Location = new System.Drawing.Point(8, 88);
			this.textBoxErrorMessage.Multiline = true;
			this.textBoxErrorMessage.Name = "textBoxErrorMessage";
			this.textBoxErrorMessage.ReadOnly = true;
			this.textBoxErrorMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxErrorMessage.Size = new System.Drawing.Size(304, 216);
			this.textBoxErrorMessage.TabIndex = 2;
			this.textBoxErrorMessage.Text = "";
			// 
			// buttonQuit
			// 
			this.buttonQuit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonQuit.Location = new System.Drawing.Point(144, 312);
			this.buttonQuit.Name = "buttonQuit";
			this.buttonQuit.TabIndex = 3;
			this.buttonQuit.Text = "Quit";
			this.buttonQuit.Click += new System.EventHandler(this.buttonExit_Click);
			// 
			// buttonSendErrorReport
			// 
			this.buttonSendErrorReport.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonSendErrorReport.Location = new System.Drawing.Point(8, 312);
			this.buttonSendErrorReport.Name = "buttonSendErrorReport";
			this.buttonSendErrorReport.Size = new System.Drawing.Size(117, 23);
			this.buttonSendErrorReport.TabIndex = 4;
			this.buttonSendErrorReport.Text = "Send Error Report";
			this.buttonSendErrorReport.Click += new System.EventHandler(this.buttonSendErrorReport_Click);
			// 
			// buttonContinue
			// 
			this.buttonContinue.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonContinue.Location = new System.Drawing.Point(232, 312);
			this.buttonContinue.Name = "buttonContinue";
			this.buttonContinue.TabIndex = 5;
			this.buttonContinue.Text = "Continue";
			this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
			// 
			// UnexpectedErrorForm
			// 
			this.AcceptButton = this.buttonQuit;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonContinue;
			this.ClientSize = new System.Drawing.Size(320, 341);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.buttonContinue,
																		  this.buttonSendErrorReport,
																		  this.buttonQuit,
																		  this.textBoxErrorMessage,
																		  this.labelErrorExplanation,
																		  this.pictureBoxExplosion});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UnexpectedErrorForm";
			this.Text = "Monodoc Error";
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonExit_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void buttonSendErrorReport_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show("TODO: This is coming, as soon as I figure out the best " +
				"delivery mechanism.  Promise.", "Unimplemented Feature"
				);
		}

		private void buttonContinue_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show("TODO: implement continue.", "Unimplemented Feature");
		}
	}
}
