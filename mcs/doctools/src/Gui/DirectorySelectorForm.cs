using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace Mono.Doc.Gui
{
	/// <summary>
	/// Summary description for DirectorySelectorForm.
	/// </summary>
	public class DirectorySelectorForm : System.Windows.Forms.Form
	{
		private DirectoryInfo currentDirectory;
		private string        selectedDirectoryName;

		private System.Windows.Forms.Label labelPath;
		private System.Windows.Forms.TextBox textBoxPath;
		private System.Windows.Forms.TreeView tree;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonParentDir;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DirectorySelectorForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			this.buttonParentDir.ImageList  = AssemblyTreeImages.List;
			this.buttonParentDir.ImageIndex = AssemblyTreeImages.Shortcuts; // TODO: need prev. dir icon
			this.tree.ImageList             = AssemblyTreeImages.List;
			this.tree.ImageIndex            = AssemblyTreeImages.Shortcuts; // TODO: need folder icon
			this.tree.SelectedImageIndex    = AssemblyTreeImages.Shortcuts; // TODO: need folder icon

			if (SetCurrentDirectory(Directory.GetCurrentDirectory()))
			{
				this.textBoxPath.Text      = this.currentDirectory.FullName;
				this.selectedDirectoryName = this.currentDirectory.FullName;
				
				InitializeTree();
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

		#region Private Instance Methods

		private void InitializeTree()
		{
			this.tree.Nodes.Clear();

			foreach (DirectoryInfo dirInfo in this.currentDirectory.GetDirectories())
			{
				TreeNode dirNode = new TreeNode(dirInfo.Name);
				dirNode.Tag      = dirInfo.FullName;

				CreateSubdirectoryNodes(dirNode);
				this.tree.Nodes.Add(dirNode);
			}
		}

		private void CreateSubdirectoryNodes(TreeNode dirNode)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(dirNode.Tag as string);

			foreach (DirectoryInfo childDirInfo in dirInfo.GetDirectories())
			{
				TreeNode childDirNode = new TreeNode(childDirInfo.Name);
				childDirNode.Tag      = childDirInfo.FullName;

				dirNode.Nodes.Add(childDirNode);
			}
		}

		private bool SetCurrentDirectory(string dirName)
		{
			string        errorMessage = null;
			DirectoryInfo newDirectory = null;

			try
			{
				newDirectory = new DirectoryInfo(dirName);
			}
			catch (ArgumentNullException)
			{
				errorMessage = "cannot be null.";
			}
			catch (DirectoryNotFoundException)
			{
				errorMessage = "was not found.";
			}
			catch (ArgumentException)
			{
				errorMessage = "contains invalid characters.";
			}
			catch (PathTooLongException)
			{
				errorMessage = "is too long.";
			}

			if (errorMessage != null)
			{
				MessageBox.Show("The selected path " + errorMessage,
					"Problem Opening Directory",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
					);

				return false;
			}
			else
			{
				this.currentDirectory = newDirectory;
				
				return true;
			}
		}

		#endregion // Private Instance Methods

		#region Public Instance Properties

		public string DirectoryName
		{
			get { return selectedDirectoryName; }
		}

		#endregion // Public Instance Properties

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.labelPath = new System.Windows.Forms.Label();
			this.textBoxPath = new System.Windows.Forms.TextBox();
			this.tree = new System.Windows.Forms.TreeView();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonParentDir = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelPath
			// 
			this.labelPath.Location = new System.Drawing.Point(8, 8);
			this.labelPath.Name = "labelPath";
			this.labelPath.Size = new System.Drawing.Size(40, 16);
			this.labelPath.TabIndex = 0;
			this.labelPath.Text = "Path:";
			this.labelPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxPath
			// 
			this.textBoxPath.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.textBoxPath.Location = new System.Drawing.Point(48, 6);
			this.textBoxPath.Name = "textBoxPath";
			this.textBoxPath.Size = new System.Drawing.Size(232, 20);
			this.textBoxPath.TabIndex = 1;
			this.textBoxPath.Text = "";
			this.textBoxPath.Leave += new System.EventHandler(this.textBoxPath_Leave);
			// 
			// tree
			// 
			this.tree.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.tree.ImageIndex = -1;
			this.tree.Location = new System.Drawing.Point(8, 32);
			this.tree.Name = "tree";
			this.tree.SelectedImageIndex = -1;
			this.tree.Size = new System.Drawing.Size(304, 288);
			this.tree.TabIndex = 3;
			this.tree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tree_BeforeSelect);
			this.tree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tree_BeforeExpand);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonOK.Location = new System.Drawing.Point(144, 328);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "OK";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.buttonCancel.Location = new System.Drawing.Point(232, 328);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonParentDir
			// 
			this.buttonParentDir.Location = new System.Drawing.Point(288, 5);
			this.buttonParentDir.Name = "buttonParentDir";
			this.buttonParentDir.Size = new System.Drawing.Size(24, 23);
			this.buttonParentDir.TabIndex = 6;
			this.buttonParentDir.Text = "..";
			this.buttonParentDir.Click += new System.EventHandler(this.buttonParentDir_Click);
			// 
			// DirectorySelectorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(320, 358);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.buttonParentDir,
																		  this.buttonCancel,
																		  this.buttonOK,
																		  this.tree,
																		  this.textBoxPath,
																		  this.labelPath});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "DirectorySelectorForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Select Directory";
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void buttonOK_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void textBoxPath_Leave(object sender, System.EventArgs e)
		{
			if (SetCurrentDirectory(textBoxPath.Text))
			{
				InitializeTree();
			}
		}

		private void tree_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			foreach (TreeNode childDirNode in e.Node.Nodes)
			{
				if (childDirNode.Nodes.Count == 0)
				{
					CreateSubdirectoryNodes(childDirNode);
				}
			}
		}

		private void tree_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			string path                = e.Node.Tag as string;
			this.textBoxPath.Text      = path;
			this.selectedDirectoryName = path;
		}

		private void buttonParentDir_Click(object sender, System.EventArgs e)
		{
			DirectoryInfo parent = this.currentDirectory.Parent;

			if (parent != null && SetCurrentDirectory(parent.FullName))
			{
				textBoxPath.Text = parent.FullName;
				InitializeTree();
			}
		}
	}
}
