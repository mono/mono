using System;
//using System.Collections;
//using System.ComponentModel;
//using System.Drawing;
//using System.Data;
using System.Windows.Forms;

using AST = antlr.collections.AST;

namespace antlr.debug.misc
{
	/// <summary>
	/// Summary description for myJTreeASTPanel.
	/// </summary>
	public class JTreeASTPanel : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.TreeView tree;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private JTreeASTPanel()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitForm call

		}

		public JTreeASTPanel(TreeViewEventHandler afterSelectHandler, AST rootAST) : this()
		{
			tree.AfterSelect += afterSelectHandler;
			tree.BeforeExpand += new TreeViewCancelEventHandler(ASTTreeNode.tree_BeforeExpand);
			tree.Nodes.Add(new ASTTreeNode(rootAST));
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tree = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// tree
			// 
			this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tree.ImageIndex = -1;
			this.tree.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.tree.Location = new System.Drawing.Point(5, 5);
			this.tree.Name = "tree";
			this.tree.SelectedImageIndex = -1;
			this.tree.Size = new System.Drawing.Size(140, 140);
			this.tree.TabIndex = 0;
			// 
			// JTreeASTPanel
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tree});
			this.DockPadding.All = 5;
			this.Name = "JTreeASTPanel";
			this.ResumeLayout(false);

		}
		#endregion

	}

	internal class ASTTreeNode : TreeNode
	{
		private  AST	ASTNode_;
		internal bool	IsAlreadyExpanded = false;

		public AST ASTNode
		{
			get { return ASTNode_;  }
			set { ASTNode_ = value; }
		}

		public ASTTreeNode(AST a)
		{
			ASTNode_ = a;
			this.Text = a.ToString();
			this.Nodes.Add("Loading.....");
		}

		internal static void tree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			ASTTreeNode thisNode   = (ASTTreeNode)e.Node;
			AST			parentAST  = thisNode.ASTNode;
			AST			childAST;

			if (!thisNode.IsAlreadyExpanded)
			{
				thisNode.Nodes.Clear();
				childAST = parentAST.getFirstChild();
				while (null != childAST)
				{
					thisNode.Nodes.Add(new ASTTreeNode(childAST));
					childAST = childAST.getNextSibling();
				}
				thisNode.IsAlreadyExpanded = true;
			}
		}

	}
}
