//
// clippy.Designer.cs
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
	public partial class Clippy {
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
			this.clipCheckBox = new System.Windows.Forms.CheckBox ();
			this.showRegionCheckBox = new System.Windows.Forms.CheckBox ();
			this.clippingComboBox = new System.Windows.Forms.ComboBox ();
			this.shapeComboBox = new System.Windows.Forms.ComboBox ();
			this.label1 = new System.Windows.Forms.Label ();
			this.label2 = new System.Windows.Forms.Label ();
			this.fillModecheckBox = new System.Windows.Forms.CheckBox ();
			this.SuspendLayout ();
			// 
			// clipCheckBox
			// 
			this.clipCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.clipCheckBox.Location = new System.Drawing.Point (12, 296);
			this.clipCheckBox.Name = "clipCheckBox";
			this.clipCheckBox.Size = new System.Drawing.Size (43, 17);
			this.clipCheckBox.TabIndex = 0;
			this.clipCheckBox.Text = "Clip";
			this.clipCheckBox.CheckedChanged += new System.EventHandler (this.UpdateDisplay);
			// 
			// showRegionCheckBox
			// 
			this.showRegionCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showRegionCheckBox.Location = new System.Drawing.Point (61, 296);
			this.showRegionCheckBox.Name = "showRegionCheckBox";
			this.showRegionCheckBox.Size = new System.Drawing.Size (145, 17);
			this.showRegionCheckBox.TabIndex = 1;
			this.showRegionCheckBox.Text = "Show Clipping Region";
			this.showRegionCheckBox.CheckedChanged += new System.EventHandler (this.UpdateDisplay);
			// 
			// clippingComboBox
			// 
			this.clippingComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.clippingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.clippingComboBox.Location = new System.Drawing.Point (153, 269);
			this.clippingComboBox.Name = "clippingComboBox";
			this.clippingComboBox.Size = new System.Drawing.Size (121, 21);
			this.clippingComboBox.TabIndex = 2;
			this.clippingComboBox.SelectedIndexChanged += new System.EventHandler (this.UpdateShapes);
			// 
			// shapeComboBox
			// 
			this.shapeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shapeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.shapeComboBox.Location = new System.Drawing.Point (12, 269);
			this.shapeComboBox.Name = "shapeComboBox";
			this.shapeComboBox.Size = new System.Drawing.Size (121, 21);
			this.shapeComboBox.TabIndex = 3;
			this.shapeComboBox.SelectedIndexChanged += new System.EventHandler (this.UpdateShapes);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.Location = new System.Drawing.Point (9, 253);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (100, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Shape";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.Location = new System.Drawing.Point (150, 253);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (100, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Clipping Shape";
			// 
			// fillModecheckBox
			// 
			this.fillModecheckBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.fillModecheckBox.Location = new System.Drawing.Point (212, 296);
			this.fillModecheckBox.Name = "fillModecheckBox";
			this.fillModecheckBox.Size = new System.Drawing.Size (81, 17);
			this.fillModecheckBox.TabIndex = 6;
			this.fillModecheckBox.Text = "Winding";
			this.fillModecheckBox.CheckedChanged += new System.EventHandler (this.UpdateDisplay);
			// 
			// Clippy
			// 
			this.ClientSize = new System.Drawing.Size (292, 325);
			this.Controls.Add (this.fillModecheckBox);
			this.Controls.Add (this.label2);
			this.Controls.Add (this.label1);
			this.Controls.Add (this.shapeComboBox);
			this.Controls.Add (this.clippingComboBox);
			this.Controls.Add (this.showRegionCheckBox);
			this.Controls.Add (this.clipCheckBox);
			this.Name = "Clippy";
			this.Text = "GDI+ Clip Tester";
			this.Paint += new System.Windows.Forms.PaintEventHandler (this.Form1_Paint);
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.CheckBox clipCheckBox;
		private System.Windows.Forms.CheckBox showRegionCheckBox;
		private System.Windows.Forms.ComboBox clippingComboBox;
		private System.Windows.Forms.ComboBox shapeComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox fillModecheckBox;
	}
}

