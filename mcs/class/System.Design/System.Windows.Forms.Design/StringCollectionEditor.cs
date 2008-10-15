//
// System.Windows.Forms.Design.StringCollectionEditor
// 
// Author:
//   Ivan N. Zlatev <contact@i-nz.net>
// 
// (C) 2007 Ivan N. Zlatev
// 

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

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

namespace System.Windows.Forms.Design
{
	internal class StringCollectionEditor : CollectionEditor
	{

		private class StringCollectionEditForm : CollectionEditor.CollectionForm
		{

			private System.Windows.Forms.TextBox txtItems;
			private System.Windows.Forms.Label label1;
			private System.Windows.Forms.Button butOk;
			private System.Windows.Forms.Button butCancel;

			public StringCollectionEditForm (CollectionEditor editor) : base (editor)
			{
				InitializeComponent ();
			}

#region Windows Form Designer generated code


			private void InitializeComponent()
			{
				this.txtItems = new System.Windows.Forms.TextBox();
				this.label1 = new System.Windows.Forms.Label();
				this.butOk = new System.Windows.Forms.Button();
				this.butCancel = new System.Windows.Forms.Button();
				this.SuspendLayout();
				// 
				// txtItems
				// 
				this.txtItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
																			  | System.Windows.Forms.AnchorStyles.Left)
																			 | System.Windows.Forms.AnchorStyles.Right)));
				this.txtItems.Location = new System.Drawing.Point(12, 25);
				this.txtItems.Multiline = true;
				this.txtItems.AcceptsTab = true;
				this.txtItems.Name = "txtItems";
				this.txtItems.ScrollBars = System.Windows.Forms.ScrollBars.Both;
				this.txtItems.Size = new System.Drawing.Size(378, 168);
				this.txtItems.TabIndex = 1;
				// 
				// label1
				// 
				this.label1.AutoSize = true;
				this.label1.Location = new System.Drawing.Point(9, 9);
				this.label1.Name = "label1";
				this.label1.Size = new System.Drawing.Size(227, 13);
				this.label1.TabIndex = 0;
				this.label1.Text = "&Enter the strings in the collection (one per line):";
				// 
				// butOk
				// 
				this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.butOk.DialogResult = System.Windows.Forms.DialogResult.OK;
				this.butOk.Location = new System.Drawing.Point(234, 199);
				this.butOk.Name = "butOk";
				this.butOk.Size = new System.Drawing.Size(75, 23);
				this.butOk.TabIndex = 3;
				this.butOk.Text = "OK";
				this.butOk.Click += new System.EventHandler(this.butOk_Click);
				// 
				// butCancel
				// 
				this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.butCancel.Location = new System.Drawing.Point(315, 199);
				this.butCancel.Name = "butCancel";
				this.butCancel.Size = new System.Drawing.Size(75, 23);
				this.butCancel.TabIndex = 4;
				this.butCancel.Text = "Cancel";
				this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
				// 
				// StringEditorForm
				// 
				this.ClientSize = new System.Drawing.Size(402, 228);
				this.Controls.Add(this.butCancel);
				this.Controls.Add(this.butOk);
				this.Controls.Add(this.label1);
				this.Controls.Add(this.txtItems);
				this.CancelButton = butCancel;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.Name = "StringEditorForm";
				this.Text = "String Collection Editor";
				this.ResumeLayout(false);
				this.PerformLayout();
			}

#endregion
			protected override void OnEditValueChanged ()
			{
				object[] items = base.Items;
				string text = String.Empty;

				for (int i=0; i < items.Length; i++) {
					if (items[i] is string) {
						text += ((string) items[i]);
						if (i != items.Length - 1) // no new line after the last one
							text += Environment.NewLine;
					}
				}
				txtItems.Text = text;
			}

			private void butOk_Click (object sender, EventArgs e)
			{
				if (this.txtItems.Text == String.Empty) {
					base.Items = new string[0];
				} else {
					string[] items = txtItems.Lines;
					bool lastLineEmpty = items[items.Length-1].Trim ().Length == 0;
					object[] objects = new object[lastLineEmpty ? items.Length-1 : items.Length];
					for (int i=0; i < objects.Length; i++)
						objects[i] = (object)items[i];
					base.Items = objects;
				}
			}

			private void butCancel_Click (object sender, EventArgs e)
			{
				this.Close ();
			}
		}

		public StringCollectionEditor (Type type) : base (type)
		{
		}


		protected override CollectionEditor.CollectionForm CreateCollectionForm ()
		{
			return new StringCollectionEditForm (this);
		}
	}
}
