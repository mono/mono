//
// binary.Designer.cs
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
	public partial class binary {
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
			this.label2 = new System.Windows.Forms.Label ();
			this.label1 = new System.Windows.Forms.Label ();
			this.shape1ComboBox = new System.Windows.Forms.ComboBox ();
			this.shape2ComboBox = new System.Windows.Forms.ComboBox ();
			this.matrix1comboBox = new System.Windows.Forms.ComboBox ();
			this.matrix2comboBox = new System.Windows.Forms.ComboBox ();
			this.label3 = new System.Windows.Forms.Label ();
			this.label4 = new System.Windows.Forms.Label ();
			this.shape1checkBox = new System.Windows.Forms.CheckBox ();
			this.shape2checkBox = new System.Windows.Forms.CheckBox ();
			this.infoLabel = new System.Windows.Forms.Label ();
			this.shape3checkBox = new System.Windows.Forms.CheckBox ();
			this.label5 = new System.Windows.Forms.Label ();
			this.matrix3comboBox = new System.Windows.Forms.ComboBox ();
			this.label6 = new System.Windows.Forms.Label ();
			this.shape3comboBox = new System.Windows.Forms.ComboBox ();
			this.SuspendLayout ();
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.ForeColor = System.Drawing.Color.Green;
			this.label2.Location = new System.Drawing.Point (157, 249);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (100, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Shape #2";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.ForeColor = System.Drawing.Color.Red;
			this.label1.Location = new System.Drawing.Point (8, 249);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (100, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Shape #1";
			// 
			// shape1ComboBox
			// 
			this.shape1ComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shape1ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.shape1ComboBox.Location = new System.Drawing.Point (11, 265);
			this.shape1ComboBox.Name = "shape1ComboBox";
			this.shape1ComboBox.Size = new System.Drawing.Size (121, 21);
			this.shape1ComboBox.TabIndex = 7;
			this.shape1ComboBox.SelectedIndexChanged += new System.EventHandler (this.shape1_Changed);
			// 
			// shape2ComboBox
			// 
			this.shape2ComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shape2ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.shape2ComboBox.Location = new System.Drawing.Point (160, 265);
			this.shape2ComboBox.Name = "shape2ComboBox";
			this.shape2ComboBox.Size = new System.Drawing.Size (121, 21);
			this.shape2ComboBox.TabIndex = 6;
			this.shape2ComboBox.SelectedIndexChanged += new System.EventHandler (this.shape2_Changed);
			// 
			// matrix1comboBox
			// 
			this.matrix1comboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.matrix1comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.matrix1comboBox.Location = new System.Drawing.Point (11, 306);
			this.matrix1comboBox.Name = "matrix1comboBox";
			this.matrix1comboBox.Size = new System.Drawing.Size (121, 21);
			this.matrix1comboBox.TabIndex = 10;
			this.matrix1comboBox.SelectedIndexChanged += new System.EventHandler (this.matrix1_Changed);
			// 
			// matrix2comboBox
			// 
			this.matrix2comboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.matrix2comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.matrix2comboBox.Location = new System.Drawing.Point (160, 306);
			this.matrix2comboBox.Name = "matrix2comboBox";
			this.matrix2comboBox.Size = new System.Drawing.Size (121, 21);
			this.matrix2comboBox.TabIndex = 11;
			this.matrix2comboBox.SelectedIndexChanged += new System.EventHandler (this.matrix2_Changed);
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.ForeColor = System.Drawing.Color.Red;
			this.label3.Location = new System.Drawing.Point (8, 290);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (100, 13);
			this.label3.TabIndex = 18;
			this.label3.Text = "Matrix #1";
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.ForeColor = System.Drawing.Color.Green;
			this.label4.Location = new System.Drawing.Point (157, 289);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size (100, 13);
			this.label4.TabIndex = 19;
			this.label4.Text = "Matrix #2";
			// 
			// shape1checkBox
			// 
			this.shape1checkBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shape1checkBox.Checked = true;
			this.shape1checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.shape1checkBox.ForeColor = System.Drawing.Color.Red;
			this.shape1checkBox.Location = new System.Drawing.Point (11, 333);
			this.shape1checkBox.Name = "shape1checkBox";
			this.shape1checkBox.Size = new System.Drawing.Size (121, 17);
			this.shape1checkBox.TabIndex = 20;
			this.shape1checkBox.Text = "Display Shape #1";
			this.shape1checkBox.CheckedChanged += new System.EventHandler (this.OnDisplayChanged);
			// 
			// shape2checkBox
			// 
			this.shape2checkBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shape2checkBox.Checked = true;
			this.shape2checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.shape2checkBox.ForeColor = System.Drawing.Color.Green;
			this.shape2checkBox.Location = new System.Drawing.Point (160, 333);
			this.shape2checkBox.Name = "shape2checkBox";
			this.shape2checkBox.Size = new System.Drawing.Size (121, 17);
			this.shape2checkBox.TabIndex = 21;
			this.shape2checkBox.Text = "Display Shape #2";
			this.shape2checkBox.CheckedChanged += new System.EventHandler (this.OnDisplayChanged);
			// 
			// infoLabel
			// 
			this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				    | System.Windows.Forms.AnchorStyles.Right)));
			this.infoLabel.Location = new System.Drawing.Point (8, 353);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size (422, 15);
			this.infoLabel.TabIndex = 22;
			this.infoLabel.Text = "-";
			// 
			// shape3checkBox
			// 
			this.shape3checkBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shape3checkBox.Checked = true;
			this.shape3checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.shape3checkBox.ForeColor = System.Drawing.Color.Blue;
			this.shape3checkBox.Location = new System.Drawing.Point (308, 333);
			this.shape3checkBox.Name = "shape3checkBox";
			this.shape3checkBox.Size = new System.Drawing.Size (121, 17);
			this.shape3checkBox.TabIndex = 27;
			this.shape3checkBox.Text = "Display Shape #3";
			this.shape3checkBox.CheckedChanged += new System.EventHandler (this.OnDisplayChanged);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.ForeColor = System.Drawing.Color.Blue;
			this.label5.Location = new System.Drawing.Point (305, 290);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size (100, 13);
			this.label5.TabIndex = 26;
			this.label5.Text = "Matrix #3";
			// 
			// matrix3comboBox
			// 
			this.matrix3comboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.matrix3comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.matrix3comboBox.Location = new System.Drawing.Point (308, 306);
			this.matrix3comboBox.Name = "matrix3comboBox";
			this.matrix3comboBox.Size = new System.Drawing.Size (121, 21);
			this.matrix3comboBox.TabIndex = 25;
			this.matrix3comboBox.SelectedIndexChanged += new System.EventHandler (this.matrix3_Changed);
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label6.ForeColor = System.Drawing.Color.Blue;
			this.label6.Location = new System.Drawing.Point (305, 249);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size (100, 13);
			this.label6.TabIndex = 24;
			this.label6.Text = "Shape #3";
			// 
			// shape3comboBox
			// 
			this.shape3comboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shape3comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.shape3comboBox.Location = new System.Drawing.Point (308, 265);
			this.shape3comboBox.Name = "shape3comboBox";
			this.shape3comboBox.Size = new System.Drawing.Size (121, 21);
			this.shape3comboBox.TabIndex = 23;
			this.shape3comboBox.SelectedIndexChanged += new System.EventHandler (this.shape3_SelectedIndexChanged);
			// 
			// Form1
			// 
			this.ClientSize = new System.Drawing.Size (444, 368);
			this.Controls.Add (this.shape3checkBox);
			this.Controls.Add (this.label5);
			this.Controls.Add (this.matrix3comboBox);
			this.Controls.Add (this.label6);
			this.Controls.Add (this.shape3comboBox);
			this.Controls.Add (this.infoLabel);
			this.Controls.Add (this.shape2checkBox);
			this.Controls.Add (this.shape1checkBox);
			this.Controls.Add (this.label4);
			this.Controls.Add (this.label3);
			this.Controls.Add (this.matrix2comboBox);
			this.Controls.Add (this.matrix1comboBox);
			this.Controls.Add (this.label2);
			this.Controls.Add (this.label1);
			this.Controls.Add (this.shape1ComboBox);
			this.Controls.Add (this.shape2ComboBox);
			this.Name = "Form1";
			this.Text = "GDI+ Region Binary Operations";
			this.Paint += new System.Windows.Forms.PaintEventHandler (this.Form1_Paint);
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox shape1ComboBox;
		private System.Windows.Forms.ComboBox shape2ComboBox;
		private System.Windows.Forms.ComboBox matrix1comboBox;
		private System.Windows.Forms.ComboBox matrix2comboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox shape1checkBox;
		private System.Windows.Forms.CheckBox shape2checkBox;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.CheckBox shape3checkBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox matrix3comboBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox shape3comboBox;
	}
}

