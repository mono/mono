// ProjectOptionsForm.cs
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
		private bool       isModified;

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonApply;
		private System.Windows.Forms.GroupBox groupBoxAssemblies;
		private System.Windows.Forms.Button buttonAssemblyRemove;
		private System.Windows.Forms.Button buttonAssemblyAdd;
		private System.Windows.Forms.ListBox listBoxAssembliesToDocument;
		private System.Windows.Forms.GroupBox groupBoxDirectories;
		private System.Windows.Forms.Button buttonDirectoryRemove;
		private System.Windows.Forms.Button buttonDirectoryAdd;
		private System.Windows.Forms.ListBox listBoxXmlSourceDirectories;
		private System.Windows.Forms.GroupBox groupBoxProperties;
		private System.Windows.Forms.ListView listViewProperties;
		private System.Windows.Forms.ToolBar toolBarProperties;
		private System.Windows.Forms.ToolBarButton toolBarButtonNewProperty;
		private System.Windows.Forms.ToolBarButton toolBarButtonEditProperty;
		private System.Windows.Forms.ToolBarButton toolBarButtonRemoveProperty;
		private System.Windows.Forms.ColumnHeader columnHeaderName;
		private System.Windows.Forms.ColumnHeader columnHeaderValue;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectOptionsForm(DocProject project)
		{
			//this.SetStyle(ControlStyles.DoubleBuffer, true);
			
			this.project    = project;
			this.isModified = false;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			this.toolBarProperties.ImageList = AssemblyTreeImages.List; // TODO: need real images

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
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonApply = new System.Windows.Forms.Button();
			this.groupBoxAssemblies = new System.Windows.Forms.GroupBox();
			this.buttonAssemblyRemove = new System.Windows.Forms.Button();
			this.buttonAssemblyAdd = new System.Windows.Forms.Button();
			this.listBoxAssembliesToDocument = new System.Windows.Forms.ListBox();
			this.groupBoxDirectories = new System.Windows.Forms.GroupBox();
			this.buttonDirectoryRemove = new System.Windows.Forms.Button();
			this.buttonDirectoryAdd = new System.Windows.Forms.Button();
			this.listBoxXmlSourceDirectories = new System.Windows.Forms.ListBox();
			this.groupBoxProperties = new System.Windows.Forms.GroupBox();
			this.toolBarProperties = new System.Windows.Forms.ToolBar();
			this.toolBarButtonNewProperty = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonEditProperty = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonRemoveProperty = new System.Windows.Forms.ToolBarButton();
			this.listViewProperties = new System.Windows.Forms.ListView();
			this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderValue = new System.Windows.Forms.ColumnHeader();
			this.groupBoxAssemblies.SuspendLayout();
			this.groupBoxDirectories.SuspendLayout();
			this.groupBoxProperties.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonOK.Location = new System.Drawing.Point(168, 394);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 10;
			this.buttonOK.Text = "OK";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(344, 394);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(72, 23);
			this.buttonCancel.TabIndex = 11;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonApply
			// 
			this.buttonApply.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonApply.Location = new System.Drawing.Point(256, 394);
			this.buttonApply.Name = "buttonApply";
			this.buttonApply.TabIndex = 12;
			this.buttonApply.Text = "Apply";
			this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
			// 
			// groupBoxAssemblies
			// 
			this.groupBoxAssemblies.Controls.AddRange(new System.Windows.Forms.Control[] {
																							 this.buttonAssemblyRemove,
																							 this.buttonAssemblyAdd,
																							 this.listBoxAssembliesToDocument});
			this.groupBoxAssemblies.Location = new System.Drawing.Point(9, 8);
			this.groupBoxAssemblies.Name = "groupBoxAssemblies";
			this.groupBoxAssemblies.Size = new System.Drawing.Size(408, 96);
			this.groupBoxAssemblies.TabIndex = 17;
			this.groupBoxAssemblies.TabStop = false;
			this.groupBoxAssemblies.Text = "Assemblies";
			// 
			// buttonAssemblyRemove
			// 
			this.buttonAssemblyRemove.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.buttonAssemblyRemove.Location = new System.Drawing.Point(320, 40);
			this.buttonAssemblyRemove.Name = "buttonAssemblyRemove";
			this.buttonAssemblyRemove.Size = new System.Drawing.Size(80, 19);
			this.buttonAssemblyRemove.TabIndex = 8;
			this.buttonAssemblyRemove.Text = "Remove";
			this.buttonAssemblyRemove.Click += new System.EventHandler(this.buttonAssemblyRemove_Click);
			// 
			// buttonAssemblyAdd
			// 
			this.buttonAssemblyAdd.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.buttonAssemblyAdd.Location = new System.Drawing.Point(320, 16);
			this.buttonAssemblyAdd.Name = "buttonAssemblyAdd";
			this.buttonAssemblyAdd.Size = new System.Drawing.Size(80, 19);
			this.buttonAssemblyAdd.TabIndex = 7;
			this.buttonAssemblyAdd.Text = "Add...";
			this.buttonAssemblyAdd.Click += new System.EventHandler(this.buttonAssemblyAdd_Click);
			// 
			// listBoxAssembliesToDocument
			// 
			this.listBoxAssembliesToDocument.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.listBoxAssembliesToDocument.Location = new System.Drawing.Point(8, 16);
			this.listBoxAssembliesToDocument.Name = "listBoxAssembliesToDocument";
			this.listBoxAssembliesToDocument.Size = new System.Drawing.Size(304, 69);
			this.listBoxAssembliesToDocument.TabIndex = 6;
			// 
			// groupBoxDirectories
			// 
			this.groupBoxDirectories.Controls.AddRange(new System.Windows.Forms.Control[] {
																							  this.buttonDirectoryRemove,
																							  this.buttonDirectoryAdd,
																							  this.listBoxXmlSourceDirectories});
			this.groupBoxDirectories.Location = new System.Drawing.Point(9, 112);
			this.groupBoxDirectories.Name = "groupBoxDirectories";
			this.groupBoxDirectories.Size = new System.Drawing.Size(408, 96);
			this.groupBoxDirectories.TabIndex = 18;
			this.groupBoxDirectories.TabStop = false;
			this.groupBoxDirectories.Text = "XML Source Directories";
			// 
			// buttonDirectoryRemove
			// 
			this.buttonDirectoryRemove.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.buttonDirectoryRemove.Location = new System.Drawing.Point(320, 40);
			this.buttonDirectoryRemove.Name = "buttonDirectoryRemove";
			this.buttonDirectoryRemove.Size = new System.Drawing.Size(80, 19);
			this.buttonDirectoryRemove.TabIndex = 8;
			this.buttonDirectoryRemove.Text = "Remove";
			this.buttonDirectoryRemove.Click += new System.EventHandler(this.buttonDirectoryRemove_Click);
			// 
			// buttonDirectoryAdd
			// 
			this.buttonDirectoryAdd.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.buttonDirectoryAdd.Location = new System.Drawing.Point(320, 16);
			this.buttonDirectoryAdd.Name = "buttonDirectoryAdd";
			this.buttonDirectoryAdd.Size = new System.Drawing.Size(80, 19);
			this.buttonDirectoryAdd.TabIndex = 7;
			this.buttonDirectoryAdd.Text = "Add...";
			this.buttonDirectoryAdd.Click += new System.EventHandler(this.buttonDirectoryAdd_Click);
			// 
			// listBoxXmlSourceDirectories
			// 
			this.listBoxXmlSourceDirectories.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.listBoxXmlSourceDirectories.Location = new System.Drawing.Point(8, 16);
			this.listBoxXmlSourceDirectories.Name = "listBoxXmlSourceDirectories";
			this.listBoxXmlSourceDirectories.Size = new System.Drawing.Size(304, 69);
			this.listBoxXmlSourceDirectories.TabIndex = 6;
			// 
			// groupBoxProperties
			// 
			this.groupBoxProperties.Controls.AddRange(new System.Windows.Forms.Control[] {
																							 this.toolBarProperties,
																							 this.listViewProperties});
			this.groupBoxProperties.Location = new System.Drawing.Point(9, 216);
			this.groupBoxProperties.Name = "groupBoxProperties";
			this.groupBoxProperties.Size = new System.Drawing.Size(408, 168);
			this.groupBoxProperties.TabIndex = 19;
			this.groupBoxProperties.TabStop = false;
			this.groupBoxProperties.Text = "Properties";
			// 
			// toolBarProperties
			// 
			this.toolBarProperties.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.toolBarProperties.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																								 this.toolBarButtonNewProperty,
																								 this.toolBarButtonEditProperty,
																								 this.toolBarButtonRemoveProperty});
			this.toolBarProperties.Divider = false;
			this.toolBarProperties.Dock = System.Windows.Forms.DockStyle.None;
			this.toolBarProperties.DropDownArrows = true;
			this.toolBarProperties.Location = new System.Drawing.Point(8, 16);
			this.toolBarProperties.Name = "toolBarProperties";
			this.toolBarProperties.ShowToolTips = true;
			this.toolBarProperties.Size = new System.Drawing.Size(392, 23);
			this.toolBarProperties.TabIndex = 2;
			this.toolBarProperties.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBarProperties.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBarProperties_ButtonClick);
			// 
			// toolBarButtonNewProperty
			// 
			this.toolBarButtonNewProperty.ImageIndex = 0;
			this.toolBarButtonNewProperty.ToolTipText = "New Property";
			// 
			// toolBarButtonEditProperty
			// 
			this.toolBarButtonEditProperty.Enabled = false;
			this.toolBarButtonEditProperty.ImageIndex = 1;
			this.toolBarButtonEditProperty.ToolTipText = "Edit Property";
			// 
			// toolBarButtonRemoveProperty
			// 
			this.toolBarButtonRemoveProperty.Enabled = false;
			this.toolBarButtonRemoveProperty.ImageIndex = 2;
			this.toolBarButtonRemoveProperty.ToolTipText = "Remove Property";
			// 
			// listViewProperties
			// 
			this.listViewProperties.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.listViewProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																								 this.columnHeaderName,
																								 this.columnHeaderValue});
			this.listViewProperties.FullRowSelect = true;
			this.listViewProperties.GridLines = true;
			this.listViewProperties.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewProperties.Location = new System.Drawing.Point(8, 40);
			this.listViewProperties.Name = "listViewProperties";
			this.listViewProperties.Size = new System.Drawing.Size(392, 120);
			this.listViewProperties.TabIndex = 1;
			this.listViewProperties.View = System.Windows.Forms.View.Details;
			this.listViewProperties.SelectedIndexChanged += new System.EventHandler(this.listViewProperties_SelectedIndexChanged);
			// 
			// columnHeaderName
			// 
			this.columnHeaderName.Text = "Name";
			this.columnHeaderName.Width = 171;
			// 
			// columnHeaderValue
			// 
			this.columnHeaderValue.Text = "Value";
			this.columnHeaderValue.Width = 217;
			// 
			// ProjectOptionsForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(426, 423);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBoxProperties,
																		  this.groupBoxAssemblies,
																		  this.buttonApply,
																		  this.buttonCancel,
																		  this.buttonOK,
																		  this.groupBoxDirectories});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "ProjectOptionsForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Monodoc Project Options";
			this.groupBoxAssemblies.ResumeLayout(false);
			this.groupBoxDirectories.ResumeLayout(false);
			this.groupBoxProperties.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Instance Methods

		private void SaveProjectChanges()
		{
			project.AssemblyFiles.Clear();

			foreach (string assemblyFile in this.listBoxAssembliesToDocument.Items)
			{
				project.AssemblyFiles.Add(assemblyFile);
			}

			project.XmlDirectories.Clear();
			
			foreach (string xmlDir in this.listBoxXmlSourceDirectories.Items)
			{
				project.XmlDirectories.Add(xmlDir);
			}

			project.Properties.Clear();

			foreach (ListViewItem prop in this.listViewProperties.Items)
			{
				project.Properties[prop.SubItems[0].Text] = prop.SubItems[1].Text;
			}

			project.IsModified = true;
			this.isModified    = false;
		}

		#endregion // Private Instance Methods

		private void buttonCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void buttonApply_Click(object sender, System.EventArgs e)
		{
			if (this.isModified)
			{
				SaveProjectChanges();
			}
		}

		private void buttonOK_Click(object sender, System.EventArgs e)
		{
			if (this.isModified)
			{
				SaveProjectChanges();
			}

			this.Close();
		}

		private void buttonAssemblyAdd_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();
			open.Filter         = "Assembly Files (*.dll; *.exe)|*.dll; *.exe|All Files (*.*)|*.*";

			if (open.ShowDialog() == DialogResult.OK)
			{
				this.listBoxAssembliesToDocument.Items.Add(open.FileName);
				this.isModified = true;
			}
		}

		private void buttonAssemblyRemove_Click(object sender, System.EventArgs e)
		{
			this.listBoxAssembliesToDocument.Items.Remove(
				this.listBoxAssembliesToDocument.SelectedItem
				);
			this.isModified = true;
		}

		private void buttonDirectoryAdd_Click(object sender, System.EventArgs e)
		{
			DirectorySelectorForm dirSel = new DirectorySelectorForm();
			
			if (dirSel.ShowDialog() == DialogResult.OK)
			{
				this.listBoxXmlSourceDirectories.Items.Add(dirSel.DirectoryName);
				this.isModified = true;
			}

			dirSel.Dispose();
		}
		private void buttonDirectoryRemove_Click(object sender, System.EventArgs e)
		{
			this.listBoxXmlSourceDirectories.Items.Remove(
				this.listBoxXmlSourceDirectories.SelectedItem
				);
			this.isModified = true;
		}

		private void toolBarProperties_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (e.Button == toolBarButtonNewProperty)
			{
				EditPropertyForm edit = new EditPropertyForm();

				if (edit.ShowDialog() == DialogResult.OK)
				{
					ListViewItem item = null;

					foreach (ListViewItem existingItem in this.listViewProperties.Items)
					{
						if (existingItem.Text == edit.PropertyName)
						{
							item = existingItem;
							break;
						}
					}

					if (item != null)
					{
						DialogResult dr = MessageBox.Show(
							"A property named " + edit.PropertyName + " already exists.  Overwrite?",
							"Property Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question
							);

						if (dr == DialogResult.Yes && item.SubItems[1].Text != edit.PropertyValue)
						{
							item.SubItems[0].Text = edit.PropertyName;
							item.SubItems[1].Text = edit.PropertyValue;
							this.isModified       = true;
						}
					}
					else
					{
						ListViewItem newProperty = new ListViewItem(edit.PropertyName);
						
						newProperty.SubItems.Add(edit.PropertyValue);
						this.listViewProperties.Items.Add(newProperty);
						
						newProperty.Selected = true;
						this.isModified      = true;
					}
				}

				edit.Dispose();
			}
			else if (e.Button == toolBarButtonEditProperty)
			{
				if (this.listViewProperties.SelectedItems.Count > 0)
				{
					ListViewItem selectedItem = this.listViewProperties.SelectedItems[0];
					EditPropertyForm edit     = new EditPropertyForm();
					edit.PropertyName         = selectedItem.SubItems[0].Text;
					edit.PropertyValue        = selectedItem.SubItems[1].Text;

					if (edit.ShowDialog(this) == DialogResult.OK &&
						(selectedItem.SubItems[0].Text != edit.PropertyName ||
						 selectedItem.SubItems[1].Text != edit.PropertyValue))
					{
						selectedItem.SubItems[0].Text = edit.PropertyName;
						selectedItem.SubItems[1].Text = edit.PropertyValue;
						selectedItem.Selected         = true;
						this.isModified               = true;
					}

					edit.Dispose();
				}
			}
			else if (e.Button == toolBarButtonRemoveProperty)
			{
				foreach (ListViewItem item in this.listViewProperties.SelectedItems)
				{
					item.Remove();
				}
			}
		}

		private void listViewProperties_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.listViewProperties.SelectedItems.Count > 0)
			{
				this.toolBarButtonEditProperty.Enabled   = true;
				this.toolBarButtonRemoveProperty.Enabled = true;
			}
			else
			{
				this.toolBarButtonEditProperty.Enabled   = false;
				this.toolBarButtonRemoveProperty.Enabled = false;
			}
		}
	}
}
