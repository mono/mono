// GenericEditorForm.cs
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	/// <summary>
	/// Summary description for GenericEditorForm.
	/// </summary>
	public class GenericEditorForm : System.Windows.Forms.Form
	{
		protected System.Windows.Forms.ToolBar toolBar;
		protected System.Windows.Forms.Label labelSummary;
		protected System.Windows.Forms.TextBox textBoxSummary;
		protected System.Windows.Forms.Label labelRemarks;
		protected System.Windows.Forms.TextBox textBoxRemarks;
		protected System.Windows.Forms.Label labelSeeAlso;
		protected System.Windows.Forms.ComboBox comboBoxLanguage;
		protected System.Windows.Forms.Label labelLanguage;
		protected System.Windows.Forms.ListView listViewSeeAlso;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GenericEditorForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// TODO: hack so I can see the toolbar and ListView
			toolBar.ImageList = AssemblyTreeImages.List;
			listViewSeeAlso.SmallImageList = AssemblyTreeImages.List;

			for (int j = 0; j < toolBar.ImageList.Images.Count; j++) 
			{
				ToolBarButton b = new ToolBarButton();
				b.ImageIndex    = j;
				toolBar.Buttons.Add(b);

				ListViewItem i = new ListViewItem("See Also " + j.ToString(), j);
				listViewSeeAlso.Items.Add(i);
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
			this.toolBar = new System.Windows.Forms.ToolBar();
			this.labelSummary = new System.Windows.Forms.Label();
			this.textBoxSummary = new System.Windows.Forms.TextBox();
			this.labelRemarks = new System.Windows.Forms.Label();
			this.textBoxRemarks = new System.Windows.Forms.TextBox();
			this.labelSeeAlso = new System.Windows.Forms.Label();
			this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
			this.labelLanguage = new System.Windows.Forms.Label();
			this.listViewSeeAlso = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// toolBar
			// 
			this.toolBar.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.toolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar.Divider = false;
			this.toolBar.Dock = System.Windows.Forms.DockStyle.None;
			this.toolBar.DropDownArrows = true;
			this.toolBar.Location = new System.Drawing.Point(8, 0);
			this.toolBar.Name = "toolBar";
			this.toolBar.ShowToolTips = true;
			this.toolBar.Size = new System.Drawing.Size(416, 23);
			this.toolBar.TabIndex = 0;
			this.toolBar.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBar.Wrappable = false;
			// 
			// labelSummary
			// 
			this.labelSummary.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelSummary.Location = new System.Drawing.Point(8, 64);
			this.labelSummary.Name = "labelSummary";
			this.labelSummary.Size = new System.Drawing.Size(416, 16);
			this.labelSummary.TabIndex = 1;
			this.labelSummary.Text = "Summary";
			// 
			// textBoxSummary
			// 
			this.textBoxSummary.AcceptsReturn = true;
			this.textBoxSummary.AcceptsTab = true;
			this.textBoxSummary.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.textBoxSummary.Location = new System.Drawing.Point(8, 80);
			this.textBoxSummary.Multiline = true;
			this.textBoxSummary.Name = "textBoxSummary";
			this.textBoxSummary.Size = new System.Drawing.Size(416, 72);
			this.textBoxSummary.TabIndex = 2;
			this.textBoxSummary.Text = "Insert summary here.";
			// 
			// labelRemarks
			// 
			this.labelRemarks.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelRemarks.Location = new System.Drawing.Point(8, 160);
			this.labelRemarks.Name = "labelRemarks";
			this.labelRemarks.Size = new System.Drawing.Size(416, 16);
			this.labelRemarks.TabIndex = 3;
			this.labelRemarks.Text = "Remarks";
			// 
			// textBoxRemarks
			// 
			this.textBoxRemarks.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.textBoxRemarks.Location = new System.Drawing.Point(8, 176);
			this.textBoxRemarks.Multiline = true;
			this.textBoxRemarks.Name = "textBoxRemarks";
			this.textBoxRemarks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxRemarks.Size = new System.Drawing.Size(416, 192);
			this.textBoxRemarks.TabIndex = 3;
			this.textBoxRemarks.Text = "Insert remarks here.";
			// 
			// labelSeeAlso
			// 
			this.labelSeeAlso.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelSeeAlso.Location = new System.Drawing.Point(8, 376);
			this.labelSeeAlso.Name = "labelSeeAlso";
			this.labelSeeAlso.Size = new System.Drawing.Size(416, 16);
			this.labelSeeAlso.TabIndex = 5;
			this.labelSeeAlso.Text = "See Also";
			// 
			// comboBoxLanguage
			// 
			this.comboBoxLanguage.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.comboBoxLanguage.Location = new System.Drawing.Point(304, 32);
			this.comboBoxLanguage.Name = "comboBoxLanguage";
			this.comboBoxLanguage.Size = new System.Drawing.Size(121, 21);
			this.comboBoxLanguage.TabIndex = 1;
			this.comboBoxLanguage.Text = "English (en)";
			// 
			// labelLanguage
			// 
			this.labelLanguage.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.labelLanguage.Location = new System.Drawing.Point(8, 34);
			this.labelLanguage.Name = "labelLanguage";
			this.labelLanguage.Size = new System.Drawing.Size(296, 16);
			this.labelLanguage.TabIndex = 8;
			this.labelLanguage.Text = "Language:";
			this.labelLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// listViewSeeAlso
			// 
			this.listViewSeeAlso.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.listViewSeeAlso.GridLines = true;
			this.listViewSeeAlso.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listViewSeeAlso.LabelWrap = false;
			this.listViewSeeAlso.Location = new System.Drawing.Point(8, 392);
			this.listViewSeeAlso.Name = "listViewSeeAlso";
			this.listViewSeeAlso.Size = new System.Drawing.Size(416, 72);
			this.listViewSeeAlso.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewSeeAlso.TabIndex = 4;
			this.listViewSeeAlso.View = System.Windows.Forms.View.List;
			// 
			// GenericEditorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(432, 477);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.listViewSeeAlso,
																		  this.labelLanguage,
																		  this.comboBoxLanguage,
																		  this.labelSeeAlso,
																		  this.textBoxRemarks,
																		  this.labelRemarks,
																		  this.textBoxSummary,
																		  this.labelSummary,
																		  this.toolBar});
			this.Name = "GenericEditorForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "GenericEditorForm";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
