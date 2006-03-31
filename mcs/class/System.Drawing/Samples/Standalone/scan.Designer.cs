//
// scan.Designer.cs
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

namespace Samples {
	public partial class scan {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.infoLabel = new System.Windows.Forms.Label ();
			this.scansCheckBox = new System.Windows.Forms.CheckBox ();
			this.label3 = new System.Windows.Forms.Label ();
			this.matrixComboBox = new System.Windows.Forms.ComboBox ();
			this.label1 = new System.Windows.Forms.Label ();
			this.shapeComboBox = new System.Windows.Forms.ComboBox ();
			this.scansTextBox = new System.Windows.Forms.TextBox ();
			this.SuspendLayout ();
			// 
			// infoLabel
			// 
			this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				    | System.Windows.Forms.AnchorStyles.Right)));
			this.infoLabel.Location = new System.Drawing.Point (300, 6);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size (228, 13);
			this.infoLabel.TabIndex = 28;
			this.infoLabel.Text = "-";
			// 
			// scansCheckBox
			// 
			this.scansCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.scansCheckBox.Checked = true;
			this.scansCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.scansCheckBox.ForeColor = System.Drawing.Color.Red;
			this.scansCheckBox.Location = new System.Drawing.Point (154, 316);
			this.scansCheckBox.Name = "scansCheckBox";
			this.scansCheckBox.Size = new System.Drawing.Size (121, 17);
			this.scansCheckBox.TabIndex = 27;
			this.scansCheckBox.Text = "Display Scans";
			this.scansCheckBox.CheckedChanged += new System.EventHandler (this.OnDisplayScans);
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.ForeColor = System.Drawing.Color.Red;
			this.label3.Location = new System.Drawing.Point (151, 273);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (100, 13);
			this.label3.TabIndex = 26;
			this.label3.Text = "Matrix";
			// 
			// matrixComboBox
			// 
			this.matrixComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.matrixComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.matrixComboBox.Location = new System.Drawing.Point (154, 289);
			this.matrixComboBox.Name = "matrixComboBox";
			this.matrixComboBox.Size = new System.Drawing.Size (121, 21);
			this.matrixComboBox.TabIndex = 25;
			this.matrixComboBox.SelectedIndexChanged += new System.EventHandler (this.OnMatrixChange);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.ForeColor = System.Drawing.Color.Blue;
			this.label1.Location = new System.Drawing.Point (10, 273);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (100, 13);
			this.label1.TabIndex = 24;
			this.label1.Text = "Shape";
			// 
			// shapeComboBox
			// 
			this.shapeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shapeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.shapeComboBox.Location = new System.Drawing.Point (13, 289);
			this.shapeComboBox.Name = "shapeComboBox";
			this.shapeComboBox.Size = new System.Drawing.Size (121, 21);
			this.shapeComboBox.TabIndex = 23;
			this.shapeComboBox.SelectedIndexChanged += new System.EventHandler (this.OnShapeChange);
			// 
			// scansTextBox
			// 
			this.scansTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				    | System.Windows.Forms.AnchorStyles.Left)
				    | System.Windows.Forms.AnchorStyles.Right)));
			this.scansTextBox.Font = new System.Drawing.Font ("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this.scansTextBox.Location = new System.Drawing.Point (300, 22);
			this.scansTextBox.Multiline = true;
			this.scansTextBox.Name = "scansTextBox";
			this.scansTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.scansTextBox.Size = new System.Drawing.Size (228, 288);
			this.scansTextBox.TabIndex = 29;
			this.scansTextBox.WordWrap = false;
			// 
			// Form1
			// 
			this.ClientSize = new System.Drawing.Size (539, 345);
			this.Controls.Add (this.scansTextBox);
			this.Controls.Add (this.infoLabel);
			this.Controls.Add (this.scansCheckBox);
			this.Controls.Add (this.label3);
			this.Controls.Add (this.matrixComboBox);
			this.Controls.Add (this.label1);
			this.Controls.Add (this.shapeComboBox);
			this.Name = "Form1";
			this.Text = "GDI+ Scans Tester";
			this.Paint += new System.Windows.Forms.PaintEventHandler (this.Form1_Paint);
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.CheckBox scansCheckBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox matrixComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox shapeComboBox;
		private System.Windows.Forms.TextBox scansTextBox;
	}
}

