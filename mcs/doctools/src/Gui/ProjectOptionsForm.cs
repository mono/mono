// ProjectOptionsForm.cs
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

using Mono.Doc.Core;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	/// <summary>
	/// Summary description for ProjectOptionsForm.
	/// </summary>
	public class ProjectOptionsForm : System.Windows.Forms.Form
	{
		private DocProject project;

		private System.Windows.Forms.Label labelAssembliesToDocument;
		private System.Windows.Forms.Label labelXmlSourceDirectories;
		private System.Windows.Forms.ListBox listBoxAssembliesToDocument;
		private System.Windows.Forms.ListBox listBoxXmlSourceDirectories;
		private System.Windows.Forms.Button buttonAssemblyAdd;
		private System.Windows.Forms.Button buttonAssemblyRemove;
		private System.Windows.Forms.Button buttonDirectoryAdd;
		private System.Windows.Forms.Button buttonDirectoryRemove;
		private System.Windows.Forms.Label labelProjectProperties;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonApply;
		private System.Windows.Forms.ToolBarButton toolBarButtonNewProperty;
		private System.Windows.Forms.ToolBarButton toolBarButtonEditProperty;
		private System.Windows.Forms.ToolBarButton toolBarButtonRemoveProperty;
		private System.Windows.Forms.ListView listViewProperties;
		private System.Windows.Forms.ColumnHeader columnHeaderPropertyName;
		private System.Windows.Forms.ColumnHeader columnHeaderPropertyValue;
		private System.Windows.Forms.ToolBar toolBarProperty;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectOptionsForm(DocProject project)
		{
			this.project = project;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			this.toolBarProperty.ImageList = AssemblyTreeImages.List; // TODO: need real images

			// load list of assemblies
			this.listBoxAssembliesToDocument.BeginUpdate();
			this.listBoxAssembliesToDocument.Items.Clear();

			foreach (string assemblyFile in project.AssemblyFiles)
			{
				this.listBoxAssembliesToDocument.Items.Add(assemblyFile);
			}
			
			this.listBoxAssembliesToDocument.EndUpdate();

			// load list of XML directories
			this.listBoxXmlSourceDirectories.BeginUpdate();
			this.listBoxXmlSourceDirectories.Items.Clear();

			foreach (string xmlDir in project.XmlDirectories)
			{
				this.listBoxXmlSourceDirectories.Items.Add(xmlDir);
			}

			this.listBoxXmlSourceDirectories.EndUpdate();

			// load properties
			foreach (string name in project.Properties.Keys)
			{
				ListViewItem entry = new ListViewItem(name);

				entry.SubItems.Add(project.Properties[name] as string);
				this.listViewProperties.Items.Add(entry);
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
			this.labelAssembliesToDocument = new System.Windows.Forms.Label();
			this.labelXmlSourceDirectories = new System.Windows.Forms.Label();
			this.listBoxAssembliesToDocument = new System.Windows.Forms.ListBox();
			this.listBoxXmlSourceDirectories = new System.Windows.Forms.ListBox();
			this.buttonAssemblyAdd = new System.Windows.Forms.Button();
			this.buttonAssemblyRemove = new System.Windows.Forms.Button();
			this.buttonDirectoryAdd = new System.Windows.Forms.Button();
			this.buttonDirectoryRemove = new System.Windows.Forms.Button();
			this.labelProjectProperties = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonApply = new System.Windows.Forms.Button();
			this.toolBarProperty = new System.Windows.Forms.ToolBar();
			this.toolBarButtonNewProperty = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonEditProperty = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonRemoveProperty = new System.Windows.Forms.ToolBarButton();
			this.listViewProperties = new System.Windows.Forms.ListView();
			this.columnHeaderPropertyName = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderPropertyValue = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// labelAssembliesToDocument
			// 
			this.labelAssembliesToDocument.Location = new System.Drawing.Point(8, 8);
			this.labelAssembliesToDocument.Name = "labelAssembliesToDocument";
			this.labelAssembliesToDocument.Size = new System.Drawing.Size(136, 16);
			this.labelAssembliesToDocument.TabIndex = 0;
			this.labelAssembliesToDocument.Text = "Assemblies to Document";
			// 
			// labelXmlSourceDirectories
			// 
			this.labelXmlSourceDirectories.Location = new System.Drawing.Point(8, 120);
			this.labelXmlSourceDirectories.Name = "labelXmlSourceDirectories";
			this.labelXmlSourceDirectories.Size = new System.Drawing.Size(128, 16);
			this.labelXmlSourceDirectories.TabIndex = 1;
			this.labelXmlSourceDirectories.Text = "XML Source Directories";
			// 
			// listBoxAssembliesToDocument
			// 
			this.listBoxAssembliesToDocument.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.listBoxAssembliesToDocument.Location = new System.Drawing.Point(8, 24);
			this.listBoxAssembliesToDocument.Name = "listBoxAssembliesToDocument";
			this.listBoxAssembliesToDocument.Size = new System.Drawing.Size(328, 82);
			this.listBoxAssembliesToDocument.TabIndex = 2;
			// 
			// listBoxXmlSourceDirectories
			// 
			this.listBoxXmlSourceDirectories.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.listBoxXmlSourceDirectories.Location = new System.Drawing.Point(8, 136);
			this.listBoxXmlSourceDirectories.Name = "listBoxXmlSourceDirectories";
			this.listBoxXmlSourceDirectories.Size = new System.Drawing.Size(328, 82);
			this.listBoxXmlSourceDirectories.TabIndex = 3;
			// 
			// buttonAssemblyAdd
			// 
			this.buttonAssemblyAdd.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.buttonAssemblyAdd.Location = new System.Drawing.Point(349, 24);
			this.buttonAssemblyAdd.Name = "buttonAssemblyAdd";
			this.buttonAssemblyAdd.TabIndex = 4;
			this.buttonAssemblyAdd.Text = "Add...";
			this.buttonAssemblyAdd.Click += new System.EventHandler(this.buttonAssemblyAdd_Click);
			// 
			// buttonAssemblyRemove
			// 
			this.buttonAssemblyRemove.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.buttonAssemblyRemove.Location = new System.Drawing.Point(349, 56);
			this.buttonAssemblyRemove.Name = "buttonAssemblyRemove";
			this.buttonAssemblyRemove.TabIndex = 5;
			this.buttonAssemblyRemove.Text = "Remove";
			this.buttonAssemblyRemove.Click += new System.EventHandler(this.buttonAssemblyRemove_Click);
			// 
			// buttonDirectoryAdd
			// 
			this.buttonDirectoryAdd.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.buttonDirectoryAdd.Location = new System.Drawing.Point(349, 136);
			this.buttonDirectoryAdd.Name = "buttonDirectoryAdd";
			this.buttonDirectoryAdd.TabIndex = 6;
			this.buttonDirectoryAdd.Text = "Add...";
			this.buttonDirectoryAdd.Click += new System.EventHandler(this.buttonDirectoryAdd_Click);
			// 
			// buttonDirectoryRemove
			// 
			this.buttonDirectoryRemove.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.buttonDirectoryRemove.Location = new System.Drawing.Point(349, 168);
			this.buttonDirectoryRemove.Name = "buttonDirectoryRemove";
			this.buttonDirectoryRemove.TabIndex = 7;
			this.buttonDirectoryRemove.Text = "Remove";
			this.buttonDirectoryRemove.Click += new System.EventHandler(this.buttonDirectoryRemove_Click);
			// 
			// labelProjectProperties
			// 
			this.labelProjectProperties.Location = new System.Drawing.Point(8, 232);
			this.labelProjectProperties.Name = "labelProjectProperties";
			this.labelProjectProperties.Size = new System.Drawing.Size(96, 16);
			this.labelProjectProperties.TabIndex = 8;
			this.labelProjectProperties.Text = "Project Properties";
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonOK.Location = new System.Drawing.Point(176, 432);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 10;
			this.buttonOK.Text = "OK";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(352, 432);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(72, 23);
			this.buttonCancel.TabIndex = 11;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonApply
			// 
			this.buttonApply.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonApply.Location = new System.Drawing.Point(264, 432);
			this.buttonApply.Name = "buttonApply";
			this.buttonApply.TabIndex = 12;
			this.buttonApply.Text = "Apply";
			this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
			// 
			// toolBarProperty
			// 
			this.toolBarProperty.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.toolBarProperty.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																							   this.toolBarButtonNewProperty,
																							   this.toolBarButtonEditProperty,
																							   this.toolBarButtonRemoveProperty});
			this.toolBarProperty.Dock = System.Windows.Forms.DockStyle.None;
			this.toolBarProperty.DropDownArrows = true;
			this.toolBarProperty.Location = new System.Drawing.Point(8, 248);
			this.toolBarProperty.Name = "toolBarProperty";
			this.toolBarProperty.ShowToolTips = true;
			this.toolBarProperty.Size = new System.Drawing.Size(416, 25);
			this.toolBarProperty.TabIndex = 15;
			this.toolBarProperty.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBarProperty.Wrappable = false;
			this.toolBarProperty.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBarProperty_ButtonClick);
			// 
			// toolBarButtonNewProperty
			// 
			this.toolBarButtonNewProperty.ImageIndex = 0;
			this.toolBarButtonNewProperty.ToolTipText = "Add a new property";
			// 
			// toolBarButtonEditProperty
			// 
			this.toolBarButtonEditProperty.ImageIndex = 1;
			this.toolBarButtonEditProperty.ToolTipText = "Edit selected property";
			// 
			// toolBarButtonRemoveProperty
			// 
			this.toolBarButtonRemoveProperty.ImageIndex = 2;
			this.toolBarButtonRemoveProperty.ToolTipText = "Remove selected properties";
			// 
			// listViewProperties
			// 
			this.listViewProperties.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.listViewProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																								 this.columnHeaderPropertyName,
																								 this.columnHeaderPropertyValue});
			this.listViewProperties.FullRowSelect = true;
			this.listViewProperties.GridLines = true;
			this.listViewProperties.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewProperties.Location = new System.Drawing.Point(8, 280);
			this.listViewProperties.Name = "listViewProperties";
			this.listViewProperties.Size = new System.Drawing.Size(416, 136);
			this.listViewProperties.TabIndex = 16;
			this.listViewProperties.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderPropertyName
			// 
			this.columnHeaderPropertyName.Text = "Name";
			this.columnHeaderPropertyName.Width = 150;
			// 
			// columnHeaderPropertyValue
			// 
			this.columnHeaderPropertyValue.Text = "Value";
			this.columnHeaderPropertyValue.Width = 262;
			// 
			// ProjectOptionsForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(432, 461);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.listViewProperties,
																		  this.toolBarProperty,
																		  this.buttonApply,
																		  this.buttonCancel,
																		  this.buttonOK,
																		  this.labelProjectProperties,
																		  this.buttonDirectoryRemove,
																		  this.buttonDirectoryAdd,
																		  this.buttonAssemblyRemove,
																		  this.buttonAssemblyAdd,
																		  this.listBoxXmlSourceDirectories,
																		  this.listBoxAssembliesToDocument,
																		  this.labelXmlSourceDirectories,
																		  this.labelAssembliesToDocument});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "ProjectOptionsForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Monodoc Project Options";
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void buttonApply_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show("TODO: implement apply");
		}

		private void buttonOK_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show("TODO: implement OK");
		}

		private void buttonAssemblyAdd_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();
			open.Filter         = "Assembly Files (*.dll; *.exe)|*.dll; *.exe|All Files (*.*)|*.*";

			if (open.ShowDialog() == DialogResult.OK)
			{
				this.listBoxAssembliesToDocument.Items.Add(open.FileName);
			}
		}

		private void buttonAssemblyRemove_Click(object sender, System.EventArgs e)
		{
			this.listBoxAssembliesToDocument.Items.Remove(
				this.listBoxAssembliesToDocument.SelectedItem
				);
		}

		private void buttonDirectoryAdd_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show("TODO: implement.  probably will have to build a dir browser widget.");
		}
		private void buttonDirectoryRemove_Click(object sender, System.EventArgs e)
		{
			this.listBoxXmlSourceDirectories.Items.Remove(
				this.listBoxXmlSourceDirectories.SelectedItem
				);
		}

		private void toolBarProperty_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (e.Button == toolBarButtonNewProperty)
			{
				EditPropertyForm edit = new EditPropertyForm();

				if (edit.ShowDialog() == DialogResult.OK)
				{
					MessageBox.Show("TODO: add new property: " + edit.PropertyName + " = " + edit.PropertyValue);
				}

				edit.Dispose();
			}
			else if (e.Button == toolBarButtonEditProperty)
			{
				MessageBox.Show("TODO: edit property");
			}
			else if (e.Button == toolBarButtonRemoveProperty)
			{
				MessageBox.Show("TODO: remove property");
			}
		}
	}
}
