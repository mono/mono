using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Mono.TypeReflector
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class SwfWindow : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.ToolBarButton openButton;
		private System.Windows.Forms.ToolBarButton preferencesButton;
		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.MenuItem menuFile;
		private System.Windows.Forms.MenuItem menuEdit;
		private System.Windows.Forms.MenuItem menuView;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuFileOpen;
		private System.Windows.Forms.MenuItem menuFileQuit;
		private System.Windows.Forms.MenuItem menuEditCopy;
		private System.Windows.Forms.MenuItem menuFormatter;
		private System.Windows.Forms.MenuItem menuFinder;
		private System.Windows.Forms.MenuItem menuViewFormatterDefault;
		private System.Windows.Forms.MenuItem menuViewFormatterVB;
		private System.Windows.Forms.MenuItem menuViewFormatterCSharp;
		private System.Windows.Forms.MenuItem menuViewFinderExplicit;
		private System.Windows.Forms.MenuItem menuViewFinderReflection;
		private System.Windows.Forms.MenuItem menuHelpAbout;
		private System.Windows.Forms.ToolBar toolBar;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.MenuItem menuFileSep1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SwfWindow ()
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
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.menuFileOpen = new System.Windows.Forms.MenuItem();
			this.menuFileSep1 = new System.Windows.Forms.MenuItem();
			this.menuFileQuit = new System.Windows.Forms.MenuItem();
			this.menuEdit = new System.Windows.Forms.MenuItem();
			this.menuEditCopy = new System.Windows.Forms.MenuItem();
			this.menuView = new System.Windows.Forms.MenuItem();
			this.menuFormatter = new System.Windows.Forms.MenuItem();
			this.menuViewFormatterDefault = new System.Windows.Forms.MenuItem();
			this.menuViewFormatterVB = new System.Windows.Forms.MenuItem();
			this.menuViewFormatterCSharp = new System.Windows.Forms.MenuItem();
			this.menuFinder = new System.Windows.Forms.MenuItem();
			this.menuViewFinderExplicit = new System.Windows.Forms.MenuItem();
			this.menuViewFinderReflection = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuHelpAbout = new System.Windows.Forms.MenuItem();
			this.toolBar = new System.Windows.Forms.ToolBar();
			this.openButton = new System.Windows.Forms.ToolBarButton();
			this.preferencesButton = new System.Windows.Forms.ToolBarButton();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.treeView = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFile,
																					 this.menuEdit,
																					 this.menuView,
																					 this.menuHelp});
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFileOpen,
																					 this.menuFileSep1,
																					 this.menuFileQuit});
			this.menuFile.Text = "&File";
			// 
			// menuFileOpen
			// 
			this.menuFileOpen.Index = 0;
			this.menuFileOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.menuFileOpen.Text = "&Open";
			this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
			// 
			// menuFileSep1
			// 
			this.menuFileSep1.Index = 1;
			this.menuFileSep1.Text = "-";
			// 
			// menuFileQuit
			// 
			this.menuFileQuit.Index = 2;
			this.menuFileQuit.Shortcut = System.Windows.Forms.Shortcut.CtrlQ;
			this.menuFileQuit.Text = "&Quit";
			this.menuFileQuit.Click += new System.EventHandler(this.menuFileQuit_Click);
			// 
			// menuEdit
			// 
			this.menuEdit.Index = 1;
			this.menuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuEditCopy});
			this.menuEdit.Text = "&Edit";
			// 
			// menuEditCopy
			// 
			this.menuEditCopy.Index = 0;
			this.menuEditCopy.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
			this.menuEditCopy.Text = "&Copy";
			this.menuEditCopy.Click += new System.EventHandler(this.menuEditCopy_Click);
			// 
			// menuView
			// 
			this.menuView.Index = 2;
			this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFormatter,
																					 this.menuFinder});
			this.menuView.Text = "&View";
			// 
			// menuFormatter
			// 
			this.menuFormatter.Index = 0;
			this.menuFormatter.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						  this.menuViewFormatterDefault,
																						  this.menuViewFormatterVB,
																						  this.menuViewFormatterCSharp});
			this.menuFormatter.Text = "F&ormatter";
			// 
			// menuViewFormatterDefault
			// 
			this.menuViewFormatterDefault.Index = 0;
			this.menuViewFormatterDefault.Text = "&Default";
			this.menuViewFormatterDefault.Click += new System.EventHandler(this.menuViewFormatterDefault_Click);
			// 
			// menuViewFormatterVB
			// 
			this.menuViewFormatterVB.Index = 1;
			this.menuViewFormatterVB.Text = "&Visual Basic .NET";
			this.menuViewFormatterVB.Click += new System.EventHandler(this.menuViewFormatterVB_Click);
			// 
			// menuViewFormatterCSharp
			// 
			this.menuViewFormatterCSharp.Index = 2;
			this.menuViewFormatterCSharp.Text = "&C#";
			this.menuViewFormatterCSharp.Click += new System.EventHandler(this.menuViewFormatterCSharp_Click);
			// 
			// menuFinder
			// 
			this.menuFinder.Index = 1;
			this.menuFinder.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuViewFinderExplicit,
																					   this.menuViewFinderReflection});
			this.menuFinder.Text = "F&inder";
			// 
			// menuViewFinderExplicit
			// 
			this.menuViewFinderExplicit.Index = 0;
			this.menuViewFinderExplicit.Text = "&Explicit";
			this.menuViewFinderExplicit.Click += new System.EventHandler(this.menuViewFinderExplicit_Click);
			// 
			// menuViewFinderReflection
			// 
			this.menuViewFinderReflection.Index = 1;
			this.menuViewFinderReflection.Text = "&Reflection";
			this.menuViewFinderReflection.Click += new System.EventHandler(this.menuViewFinderReflection_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 3;
			this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuHelpAbout});
			this.menuHelp.Text = "&Help";
			// 
			// menuHelpAbout
			// 
			this.menuHelpAbout.Index = 0;
			this.menuHelpAbout.Text = "&About";
			this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
			// 
			// toolBar
			// 
			this.toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																					   this.openButton,
																					   this.preferencesButton});
			this.toolBar.DropDownArrows = true;
			this.toolBar.Name = "toolBar";
			this.toolBar.ShowToolTips = true;
			this.toolBar.Size = new System.Drawing.Size(292, 39);
			this.toolBar.TabIndex = 0;
			this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar_ButtonClick);
			// 
			// openButton
			// 
			this.openButton.Text = "Open";
			this.openButton.ToolTipText = "Open an Assembly";
			// 
			// preferencesButton
			// 
			this.preferencesButton.Text = "Preferences";
			this.preferencesButton.ToolTipText = "Edit Program Preferences";
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 244);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(292, 22);
			this.statusBar.TabIndex = 1;
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.ImageIndex = -1;
			this.treeView.Location = new System.Drawing.Point(0, 39);
			this.treeView.Name = "treeView";
			this.treeView.SelectedImageIndex = -1;
			this.treeView.Size = new System.Drawing.Size(292, 205);
			this.treeView.TabIndex = 2;
			// 
			// SwfApp
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.treeView,
																		  this.statusBar,
																		  this.toolBar});
			this.Menu = this.mainMenu;
			this.Name = "SwfApp";
			this.Text = "Type Reflector";
			this.ResumeLayout(false);

		}
		#endregion

		protected override void OnClosing (CancelEventArgs e)
		{
			base.OnClosing (e);
			e.Cancel = true;
			FileQuitClick (this, e);
		}

		private void menuFileOpen_Click(object sender, System.EventArgs e)
		{
			FileOpenClick (sender, e);
		}

		private void menuFileQuit_Click(object sender, System.EventArgs e)
		{
			FileQuitClick (sender, e);
		}

		private void menuEditCopy_Click(object sender, System.EventArgs e)
		{
			EditCopyClick (sender, e);
		}

		private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			menuFileOpen_Click (sender, e);
		}

		private void menuViewFormatterDefault_Click(object sender, System.EventArgs e)
		{
			ViewFormatterDefaultClick (sender, e);
		}

		private void menuViewFormatterVB_Click(object sender, System.EventArgs e)
		{
			ViewFormatterVBClick (sender, e);
		}

		private void menuViewFormatterCSharp_Click(object sender, System.EventArgs e)
		{
			ViewFormatterCSharpClick (sender, e);
		}

		private void menuViewFinderExplicit_Click(object sender, System.EventArgs e)
		{
			ViewFinderExplicitClick (sender, e);
		}

		private void menuViewFinderReflection_Click(object sender, System.EventArgs e)
		{
			ViewFinderReflectionClick (sender, e);
		}

		private void menuHelpAbout_Click(object sender, System.EventArgs e)
		{
			HelpAboutClick (sender, e);
		}

		public EventHandler FileOpenClick;
		public EventHandler FileQuitClick;
		public EventHandler EditCopyClick;
		public EventHandler ViewFormatterDefaultClick;
		public EventHandler ViewFormatterVBClick;
		public EventHandler ViewFormatterCSharpClick;
		public EventHandler ViewFinderExplicitClick;
		public EventHandler ViewFinderReflectionClick;
		public EventHandler HelpAboutClick;

		public TreeView TreeView {
			get {return treeView;}
		}

		public StatusBar StatusBar {
			get {return statusBar;}
		}
	}
}

