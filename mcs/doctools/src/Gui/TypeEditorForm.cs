// TypeEditorForm.cs
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

namespace Mono.Doc.Gui
{
	public class TypeEditorForm : GenericEditorForm
	{
		private System.Windows.Forms.Label labelMembers;
		private System.Windows.Forms.ComboBox comboBoxMembers;

		private void InitializeComponent()
		{
			this.labelMembers = new System.Windows.Forms.Label();
			this.comboBoxMembers = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// toolBar
			// 
			//this.toolBar.ButtonSize = new System.Drawing.Size(43, 22);
			this.toolBar.Visible = true;
			// 
			// labelSummary
			// 
			this.labelSummary.Visible = true;
			// 
			// textBoxSummary
			// 
			this.textBoxSummary.Visible = true;
			// 
			// labelRemarks
			// 
			this.labelRemarks.Visible = true;
			// 
			// textBoxRemarks
			// 
			this.textBoxRemarks.Visible = true;
			// 
			// labelSeeAlso
			// 
			this.labelSeeAlso.Visible = true;
			// 
			// comboBoxLanguage
			// 
			this.comboBoxLanguage.ItemHeight = 13;
			this.comboBoxLanguage.Visible = true;
			// 
			// labelLanguage
			// 
			this.labelLanguage.Location = new System.Drawing.Point(232, 34);
			this.labelLanguage.Size = new System.Drawing.Size(72, 16);
			this.labelLanguage.Visible = true;
			// 
			// listView1
			// 
			this.listViewSeeAlso.Visible = true;
			// 
			// labelMembers
			// 
			this.labelMembers.Location = new System.Drawing.Point(8, 34);
			this.labelMembers.Name = "labelMembers";
			this.labelMembers.Size = new System.Drawing.Size(56, 16);
			this.labelMembers.TabIndex = 10;
			this.labelMembers.Text = "Members:";
			// 
			// comboBoxMembers
			// 
			this.comboBoxMembers.Location = new System.Drawing.Point(64, 32);
			this.comboBoxMembers.Name = "comboBoxMembers";
			this.comboBoxMembers.Size = new System.Drawing.Size(168, 21);
			this.comboBoxMembers.TabIndex = 11;
			this.comboBoxMembers.Text = "comboBox1";
			// 
			// TypeEditorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(432, 469);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.listViewSeeAlso,
																		  this.labelLanguage,
																		  this.comboBoxLanguage,
																		  this.labelSeeAlso,
																		  this.textBoxRemarks,
																		  this.labelRemarks,
																		  this.textBoxSummary,
																		  this.labelSummary,
																		  this.toolBar,
																		  this.comboBoxMembers,
																		  this.labelMembers});
			this.Name = "TypeEditorForm";
			this.Text = "TypeEditorForm";
			this.ResumeLayout(false);

		}
	
		public TypeEditorForm() : base()
		{
			InitializeComponent();
		}
	}
}
