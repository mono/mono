// MainForm.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
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
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace Mono.Doc.Gui
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem menuProject;
		private System.Windows.Forms.MenuItem menuEdit;
		private System.Windows.Forms.MenuItem menuWindow;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuProjectExit;
		private System.Windows.Forms.MenuItem menuWindowCascade;
		private System.Windows.Forms.MenuItem menuWindowTile;
		private System.Windows.Forms.MenuItem menuWindowTileHorizontal;
		private System.Windows.Forms.MenuItem menuHelpAbout;
		private System.Windows.Forms.StatusBar status;
		private System.Windows.Forms.TreeView tree;
		private System.Windows.Forms.Splitter verticalSplitter;
		private System.Windows.Forms.MenuItem menuWindowNewGenericDebug;
		private System.Windows.Forms.MenuItem menuWindowNewTypeDebug;

		private TreeNode shortcuts;
		private System.Windows.Forms.ContextMenu treeContextMenu;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MainForm()
		{
			InitializeComponent();

			// tree stuff
			// FIXME: hardcoded hack
			AssemblyTreeLoader.LoadTree(tree, @"F:\projects\mcs\class\lib\System.dll");
			// test 'Shortcuts' idea
			shortcuts                    = new TreeNode("Shortcuts");
			shortcuts.ImageIndex         = AssemblyTreeImages.Shortcuts;
			shortcuts.SelectedImageIndex = AssemblyTreeImages.Shortcuts;
			tree.Nodes.Insert(0, shortcuts);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuProject = new System.Windows.Forms.MenuItem();
			this.menuProjectExit = new System.Windows.Forms.MenuItem();
			this.menuEdit = new System.Windows.Forms.MenuItem();
			this.menuWindow = new System.Windows.Forms.MenuItem();
			this.menuWindowNewGenericDebug = new System.Windows.Forms.MenuItem();
			this.menuWindowNewTypeDebug = new System.Windows.Forms.MenuItem();
			this.menuWindowCascade = new System.Windows.Forms.MenuItem();
			this.menuWindowTile = new System.Windows.Forms.MenuItem();
			this.menuWindowTileHorizontal = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new System.Windows.Forms.MenuItem();
			this.status = new System.Windows.Forms.StatusBar();
			this.tree = new System.Windows.Forms.TreeView();
			this.treeContextMenu = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.verticalSplitter = new System.Windows.Forms.Splitter();
			this.SuspendLayout();
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuProject,
																					 this.menuEdit,
																					 this.menuWindow,
																					 this.menuHelp});
			// 
			// menuProject
			// 
			this.menuProject.Index = 0;
			this.menuProject.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuProjectExit});
			this.menuProject.Text = "Project";
			// 
			// menuProjectExit
			// 
			this.menuProjectExit.Index = 0;
			this.menuProjectExit.Text = "Exit";
			this.menuProjectExit.Click += new System.EventHandler(this.menuProjectExit_Click);
			// 
			// menuEdit
			// 
			this.menuEdit.Index = 1;
			this.menuEdit.Text = "Edit";
			// 
			// menuWindow
			// 
			this.menuWindow.Index = 2;
			this.menuWindow.MdiList = true;
			this.menuWindow.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuWindowNewGenericDebug,
																					   this.menuWindowNewTypeDebug,
																					   this.menuWindowCascade,
																					   this.menuWindowTile,
																					   this.menuWindowTileHorizontal});
			this.menuWindow.Text = "Window";
			// 
			// menuWindowNewGenericDebug
			// 
			this.menuWindowNewGenericDebug.Index = 0;
			this.menuWindowNewGenericDebug.Text = "New Generic (debug)";
			this.menuWindowNewGenericDebug.Click += new System.EventHandler(this.menuWindowNewDebug_Click);
			// 
			// menuWindowNewTypeDebug
			// 
			this.menuWindowNewTypeDebug.Index = 1;
			this.menuWindowNewTypeDebug.Text = "New Type (debug)";
			this.menuWindowNewTypeDebug.Click += new System.EventHandler(this.menuWindowNewTypeDebug_Click);
			// 
			// menuWindowCascade
			// 
			this.menuWindowCascade.Index = 2;
			this.menuWindowCascade.Text = "Cascade";
			// 
			// menuWindowTile
			// 
			this.menuWindowTile.Index = 3;
			this.menuWindowTile.Text = "Tile";
			// 
			// menuWindowTileHorizontal
			// 
			this.menuWindowTileHorizontal.Index = 4;
			this.menuWindowTileHorizontal.Text = "Tile Horizontal";
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 3;
			this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuHelpAbout});
			this.menuHelp.Text = "Help";
			// 
			// menuHelpAbout
			// 
			this.menuHelpAbout.Index = 0;
			this.menuHelpAbout.Text = "About...";
			this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
			// 
			// status
			// 
			this.status.Location = new System.Drawing.Point(0, 586);
			this.status.Name = "status";
			this.status.Size = new System.Drawing.Size(672, 22);
			this.status.TabIndex = 1;
			this.status.Text = "Ready";
			// 
			// tree
			// 
			this.tree.Dock = System.Windows.Forms.DockStyle.Left;
			this.tree.ImageIndex = -1;
			this.tree.Name = "tree";
			this.tree.PathSeparator = "/";
			this.tree.SelectedImageIndex = -1;
			this.tree.Size = new System.Drawing.Size(121, 586);
			this.tree.TabIndex = 2;
			this.tree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tree_MouseUp);
			this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_AfterSelect);
			// 
			// treeContextMenu
			// 
			this.treeContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.menuItem1,
																							this.menuItem2});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Add to Shortcuts";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.Text = "Remove Shortcut";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// verticalSplitter
			// 
			this.verticalSplitter.Location = new System.Drawing.Point(121, 0);
			this.verticalSplitter.Name = "verticalSplitter";
			this.verticalSplitter.Size = new System.Drawing.Size(3, 586);
			this.verticalSplitter.TabIndex = 3;
			this.verticalSplitter.TabStop = false;
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(672, 608);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.verticalSplitter,
																		  this.tree,
																		  this.status});
			this.IsMdiContainer = true;
			this.Menu = this.mainMenu;
			this.Name = "MainForm";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}
		#endregion

		// EVENTS /////////
		private void menuProjectExit_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void menuHelpAbout_Click(object sender, System.EventArgs e)
		{
			Form about = new AboutForm();
			about.ShowDialog();
		}

		private void menuWindowNewDebug_Click(object sender, System.EventArgs e)
		{
			Form debugForm      = new GenericEditorForm();
			debugForm.MdiParent = this;
			debugForm.Show();
		}

		private void menuWindowNewTypeDebug_Click(object sender, System.EventArgs e)
		{
			Form debugForm      = new TypeEditorForm();
			debugForm.MdiParent = this;
			debugForm.Show();
		}

		private void tree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			this.status.Text = (string) e.Node.Tag;
		}

		// more shortcuts testing
		private void menuItem1_Click(object sender, System.EventArgs e)
		{
			TreeNode sel = tree.SelectedNode;
			if (shortcuts != sel)
			{
				bool isNewShortcut = true;

				foreach (TreeNode node in shortcuts.Nodes)
				{
					if (sel.Tag == node.Tag) 
					{
						isNewShortcut = false;
						break;
					}
				}

				if (isNewShortcut)
				{
					shortcuts.Nodes.Add((TreeNode) sel.Clone());
				}
			}
		}

		// more shortcuts testing
		private void tree_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				tree.SelectedNode = tree.GetNodeAt(e.X, e.Y);

				if (tree.SelectedNode == null)
				{
					// they didn't click on a node; no context menu
				}
				else
				{
					// there is an active node; customize context menu
					Point ctxLoc = this.PointToClient(tree.PointToScreen(new Point(e.X, e.Y)));
					this.treeContextMenu.Show(tree, ctxLoc);
				}
			}
		}

		// more shortcuts testing
		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			TreeNode sel = tree.SelectedNode;

			foreach (TreeNode node in shortcuts.Nodes)
			{
				if (node == sel)
				{
					shortcuts.Nodes.Remove(sel);
					break;
				}
			}
		}
	}
}
