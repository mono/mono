// ExampleCodeEditorForm.cs
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
	/// Summary description for ExampleCodeEditorForm.
	/// </summary>
	public class ExampleCodeEditorForm : System.Windows.Forms.Form
	{
		// TODO: figure out how to munge TextBox to support simple autoindent.

		private System.Windows.Forms.Label labelExampleLanguage;
		private System.Windows.Forms.ComboBox comboBoxExampleLanguage;
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.TextBox textBoxExampleText;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ExampleCodeEditorForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.labelExampleLanguage = new System.Windows.Forms.Label();
			this.comboBoxExampleLanguage = new System.Windows.Forms.ComboBox();
			this.buttonOk = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.textBoxExampleText = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// labelExampleLanguage
			// 
			this.labelExampleLanguage.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.labelExampleLanguage.Location = new System.Drawing.Point(168, 8);
			this.labelExampleLanguage.Name = "labelExampleLanguage";
			this.labelExampleLanguage.Size = new System.Drawing.Size(112, 16);
			this.labelExampleLanguage.TabIndex = 0;
			this.labelExampleLanguage.Text = "Example Language:";
			this.labelExampleLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxExampleLanguage
			// 
			this.comboBoxExampleLanguage.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.comboBoxExampleLanguage.Items.AddRange(new object[] {
																		 "C#"});
			this.comboBoxExampleLanguage.Location = new System.Drawing.Point(280, 6);
			this.comboBoxExampleLanguage.Name = "comboBoxExampleLanguage";
			this.comboBoxExampleLanguage.Size = new System.Drawing.Size(121, 21);
			this.comboBoxExampleLanguage.TabIndex = 1;
			// 
			// buttonOk
			// 
			this.buttonOk.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonOk.Location = new System.Drawing.Point(248, 353);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(64, 24);
			this.buttonOk.TabIndex = 2;
			this.buttonOk.Text = "OK";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(328, 353);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(64, 24);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Cancel";
			// 
			// textBoxExampleText
			// 
			this.textBoxExampleText.AcceptsReturn = true;
			this.textBoxExampleText.AcceptsTab = true;
			this.textBoxExampleText.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.textBoxExampleText.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxExampleText.Location = new System.Drawing.Point(8, 32);
			this.textBoxExampleText.Multiline = true;
			this.textBoxExampleText.Name = "textBoxExampleText";
			this.textBoxExampleText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxExampleText.Size = new System.Drawing.Size(392, 313);
			this.textBoxExampleText.TabIndex = 4;
			this.textBoxExampleText.Text = "// example code";
			this.textBoxExampleText.WordWrap = false;
			// 
			// ExampleCodeEditorForm
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(408, 382);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.textBoxExampleText,
																		  this.buttonCancel,
																		  this.buttonOk,
																		  this.comboBoxExampleLanguage,
																		  this.labelExampleLanguage});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ExampleCodeEditorForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Example Code Editor";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
