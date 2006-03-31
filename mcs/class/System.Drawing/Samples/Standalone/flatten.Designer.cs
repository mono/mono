//
// flatten.Designer.cs
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
	public partial class flatten {
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
			this.redrawButton = new System.Windows.Forms.Button ();
			this.translateXtextBox = new System.Windows.Forms.TextBox ();
			this.translateYtextBox = new System.Windows.Forms.TextBox ();
			this.flattenTextBox = new System.Windows.Forms.TextBox ();
			this.label1 = new System.Windows.Forms.Label ();
			this.label2 = new System.Windows.Forms.Label ();
			this.infoLabel = new System.Windows.Forms.Label ();
			this.shapeComboBox = new System.Windows.Forms.ComboBox ();
			this.label3 = new System.Windows.Forms.Label ();
			this.SuspendLayout ();
			// 
			// redrawButton
			// 
			this.redrawButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.redrawButton.Location = new System.Drawing.Point (205, 295);
			this.redrawButton.Name = "redrawButton";
			this.redrawButton.Size = new System.Drawing.Size (75, 23);
			this.redrawButton.TabIndex = 0;
			this.redrawButton.Text = "&Redraw";
			this.redrawButton.Click += new System.EventHandler (this.redrawButton_Click);
			// 
			// translateXtextBox
			// 
			this.translateXtextBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.translateXtextBox.Location = new System.Drawing.Point (12, 295);
			this.translateXtextBox.Name = "translateXtextBox";
			this.translateXtextBox.Size = new System.Drawing.Size (50, 20);
			this.translateXtextBox.TabIndex = 1;
			this.translateXtextBox.Text = "0";
			this.translateXtextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// translateYtextBox
			// 
			this.translateYtextBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.translateYtextBox.Location = new System.Drawing.Point (68, 295);
			this.translateYtextBox.Name = "translateYtextBox";
			this.translateYtextBox.Size = new System.Drawing.Size (50, 20);
			this.translateYtextBox.TabIndex = 2;
			this.translateYtextBox.Text = "10";
			this.translateYtextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// flattenTextBox
			// 
			this.flattenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.flattenTextBox.Location = new System.Drawing.Point (124, 295);
			this.flattenTextBox.Name = "flattenTextBox";
			this.flattenTextBox.Size = new System.Drawing.Size (75, 20);
			this.flattenTextBox.TabIndex = 3;
			this.flattenTextBox.Text = "10";
			this.flattenTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.Location = new System.Drawing.Point (9, 279);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (106, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Translate X, Y";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.Location = new System.Drawing.Point (121, 279);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (78, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Flatten";
			// 
			// infoLabel
			// 
			this.infoLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				    | System.Windows.Forms.AnchorStyles.Right)));
			this.infoLabel.Location = new System.Drawing.Point (12, 325);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size (268, 23);
			this.infoLabel.TabIndex = 6;
			this.infoLabel.Text = "-";
			// 
			// shapeComboBox
			// 
			this.shapeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				    | System.Windows.Forms.AnchorStyles.Right)));
			this.shapeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.shapeComboBox.Location = new System.Drawing.Point (12, 255);
			this.shapeComboBox.Name = "shapeComboBox";
			this.shapeComboBox.Size = new System.Drawing.Size (268, 21);
			this.shapeComboBox.TabIndex = 7;
			this.shapeComboBox.SelectedIndexChanged += new System.EventHandler (this.redrawButton_Click);
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.Location = new System.Drawing.Point (9, 239);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (271, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Shape";
			// 
			// Flattener
			// 
			this.ClientSize = new System.Drawing.Size (292, 347);
			this.Controls.Add (this.label3);
			this.Controls.Add (this.shapeComboBox);
			this.Controls.Add (this.infoLabel);
			this.Controls.Add (this.label2);
			this.Controls.Add (this.label1);
			this.Controls.Add (this.flattenTextBox);
			this.Controls.Add (this.translateYtextBox);
			this.Controls.Add (this.translateXtextBox);
			this.Controls.Add (this.redrawButton);
			this.Name = "Flattener";
			this.Text = "GDI+ Flatten Tester";
			this.Paint += new System.Windows.Forms.PaintEventHandler (this.Flattener_Paint);
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.Button redrawButton;
		private System.Windows.Forms.TextBox translateXtextBox;
		private System.Windows.Forms.TextBox translateYtextBox;
		private System.Windows.Forms.TextBox flattenTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.ComboBox shapeComboBox;
		private System.Windows.Forms.Label label3;
	}
}

