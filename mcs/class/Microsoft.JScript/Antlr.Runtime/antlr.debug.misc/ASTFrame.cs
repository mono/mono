using System;
using System.Drawing;
//using System.Collections;
//using System.ComponentModel;
using System.Windows.Forms;

//using antlr;
using AST = antlr.collections.AST;

namespace antlr.debug.misc
{
	/// <summary>
	/// Summary description for myASTFrame.
	/// </summary>
	public class ASTFrame : System.Windows.Forms.Form
	{
		// The initial width and height of the frame
		private const int WIDTH = 200;
		private const int HEIGHT = 300;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ASTFrame()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.Size = new System.Drawing.Size(WIDTH,HEIGHT);
			Application.ApplicationExit += new EventHandler(Form_OnExit);
		}

		public ASTFrame(string title, AST rootAST) : this()
		{
			this.Text = title;

			JTreeASTPanel treePanel = new JTreeASTPanel(new TreeViewEventHandler(tree_AfterSelect), rootAST);			
			this.Controls.Add(treePanel);
			treePanel.Location= new Point(5, 5);
			treePanel.Dock=DockStyle.Fill;
			treePanel.Anchor=AnchorStyles.Top|AnchorStyles.Left;
		}

		private void Form_OnExit(object sender, EventArgs e)
		{
			this.Visible = false;
			this.Dispose();
		}

		private void tree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			//System.Console.Out.WriteLine("Selected: " + e.Node.Text);

			string path = e.Node.FullPath;
			path = path.Replace(e.Node.TreeView.PathSeparator, "->");
			//System.Console.Out.WriteLine(e.Node.FullPath); 
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

		public static void Main(string[] args)
		{
			// Create the tree nodes
			ASTFactory factory = new ASTFactory();
			CommonAST r = (CommonAST) factory.create(0, "ROOT");
			r.addChild((CommonAST) factory.create(0, "C1"));
			r.addChild((CommonAST) factory.create(0, "C2"));
			r.addChild((CommonAST) factory.create(0, "C3"));
			
			ASTFrame frame = new ASTFrame("AST JTree Example", r);
			Application.Run(frame);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// ASTFrame
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Name = "ASTFrame";
			this.Text = "ASTFrame";

		}
		#endregion
	}
}
