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
	/// Summary description for EditPropertyForm.
	/// </summary>
	public class EditPropertyForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label labelPropertyName;
		private System.Windows.Forms.Label labelPropertyValue;
		private System.Windows.Forms.TextBox textBoxPropertyName;
		private System.Windows.Forms.TextBox textBoxPropertyValue;
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.Button buttonCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditPropertyForm()
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
			this.labelPropertyName = new System.Windows.Forms.Label();
			this.labelPropertyValue = new System.Windows.Forms.Label();
			this.textBoxPropertyName = new System.Windows.Forms.TextBox();
			this.textBoxPropertyValue = new System.Windows.Forms.TextBox();
			this.buttonOk = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelPropertyName
			// 
			this.labelPropertyName.Location = new System.Drawing.Point(8, 8);
			this.labelPropertyName.Name = "labelPropertyName";
			this.labelPropertyName.Size = new System.Drawing.Size(100, 16);
			this.labelPropertyName.TabIndex = 0;
			this.labelPropertyName.Text = "Property Name";
			// 
			// labelPropertyValue
			// 
			this.labelPropertyValue.Location = new System.Drawing.Point(8, 56);
			this.labelPropertyValue.Name = "labelPropertyValue";
			this.labelPropertyValue.Size = new System.Drawing.Size(96, 16);
			this.labelPropertyValue.TabIndex = 1;
			this.labelPropertyValue.Text = "Property Value";
			// 
			// textBoxPropertyName
			// 
			this.textBoxPropertyName.Location = new System.Drawing.Point(8, 24);
			this.textBoxPropertyName.Name = "textBoxPropertyName";
			this.textBoxPropertyName.Size = new System.Drawing.Size(272, 20);
			this.textBoxPropertyName.TabIndex = 2;
			this.textBoxPropertyName.Text = "New Property";
			// 
			// textBoxPropertyValue
			// 
			this.textBoxPropertyValue.Location = new System.Drawing.Point(8, 72);
			this.textBoxPropertyValue.Name = "textBoxPropertyValue";
			this.textBoxPropertyValue.Size = new System.Drawing.Size(272, 20);
			this.textBoxPropertyValue.TabIndex = 3;
			this.textBoxPropertyValue.Text = "Property Value";
			// 
			// buttonOk
			// 
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Location = new System.Drawing.Point(120, 104);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.TabIndex = 4;
			this.buttonOk.Text = "OK";
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(208, 104);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// EditPropertyForm
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(292, 136);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.buttonCancel,
																		  this.buttonOk,
																		  this.textBoxPropertyValue,
																		  this.textBoxPropertyName,
																		  this.labelPropertyValue,
																		  this.labelPropertyName});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "EditPropertyForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Project Property";
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void buttonOk_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#region Public Instance Properties

		public string PropertyName
		{
			get { return this.textBoxPropertyName.Text;  }
			set { this.textBoxPropertyName.Text = value; }
		}

		public string PropertyValue
		{
			get { return this.textBoxPropertyValue.Text;  }
			set { this.textBoxPropertyValue.Text = value; }
		}

		#endregion // Public Instance Properties
	}
}
