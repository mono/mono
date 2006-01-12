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
// Copyright (c) 2005-2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Alexander Olk (xenomorph2@onlinehome.de)
//
//

// NOT COMPLETE
// TODO:
// - create new folder if NewFolderButton is pressed
// - better handling of Environment.SpecialFolders
// - fix: if SelectedPath != "" and it is beyond RootFolder then show it (currently TreeNode.EnsureVisible() is missing...)

using System;
using System.Drawing;
using System.ComponentModel;
using System.Resources;
using System.IO;
using System.Collections;

namespace System.Windows.Forms
{
	[DefaultEvent("HelpRequest")]
	[DefaultProperty("SelectedPath")]
	[Designer("System.Windows.Forms.Design.FolderBrowserDialogDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public sealed class FolderBrowserDialog : CommonDialog
	{
		#region Local Variables
		private string description = "";
		private Environment.SpecialFolder rootFolder = Environment.SpecialFolder.Desktop;
		private string selectedPath = "";
		private bool showNewFolderButton = true;

		#endregion	// Local Variables
		
		#region Public Constructors
		public FolderBrowserDialog( )
		{
			form = new FolderBrowserDialogForm( this );
			form.Size =  new Size( 322, 288 );
			form.MinimumSize = new Size( 322, 288 );
			form.Text = "Search Folder";
		}
		#endregion	// Public Constructors
		
		#region Public Instance Properties
		[Browsable(true)]
		[DefaultValue("")]
		[Localizable(true)]
		public string Description
		{
			set
			{
				description = value;
			}
			
			get
			{
				return description;
			}
		}
		
		[Browsable(true)]
		[DefaultValue(Environment.SpecialFolder.Desktop)]
		[Localizable(false)]
		public Environment.SpecialFolder RootFolder
		{
			set
			{
				rootFolder = value;
			}
			
			get
			{
				return rootFolder;
			}
		}
		
		[Browsable(true)]
		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.SelectedPathEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		public string SelectedPath
		{
			set
			{
				selectedPath = value;
			}
			
			get
			{
				return selectedPath;
			}
		}

		[Browsable(true)]
		[DefaultValue(true)]
		[Localizable(false)]
		public bool ShowNewFolderButton
		{
			set
			{
				showNewFolderButton = value;
			}
			
			get
			{
				return showNewFolderButton;
			}
		}
		#endregion	// Public Instance Properties
		
		#region Public Instance Methods
		public override void Reset( )
		{
			description = "";
			rootFolder = Environment.SpecialFolder.Desktop;
			selectedPath = "";
			showNewFolderButton = true;
		}
		
		protected override bool RunDialog( IntPtr hwndOwner )
		{
			FolderBrowserDialogPanel fb = new FolderBrowserDialogPanel (this);
			form.Controls.Add (fb);
			return true;
		}
		#endregion	// Public Instance Methods

		#region Internal Methods
		internal class FolderBrowserDialogForm : DialogForm
		{
			internal FolderBrowserDialogForm( CommonDialog owner )
			: base( owner )
			{}
			
			protected override CreateParams CreateParams
			{
				get
				{
					CreateParams	cp;
					
					ControlBox = true;
					MinimizeBox = false;
					MaximizeBox = false;
					
					cp = base.CreateParams;
					cp.Style = (int)( WindowStyles.WS_POPUP | WindowStyles.WS_CAPTION | WindowStyles.WS_SYSMENU | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS );
					cp.Style |= (int)WindowStyles.WS_OVERLAPPEDWINDOW;

					if (!is_enabled) {
						cp.Style |= (int)(WindowStyles.WS_DISABLED);
					}
					
					return cp;
				}
			}
		}
		
		internal class FolderBrowserDialogPanel : Panel
		{
			private Label descriptionLabel;
			private Button cancelButton;
			private Button okButton;
			private TreeView folderBrowserTreeView;
			private Button newFolderButton;
			
			private FolderBrowserDialog folderBrowserDialog;
			private string selectedPath;
			
			private ImageList imageList;
			private TreeNode selectedPathNode = null;
			private TreeNode root_node;

			public FolderBrowserDialogPanel (FolderBrowserDialog folderBrowserDialog)
			{
				this.folderBrowserDialog = folderBrowserDialog;
				
				newFolderButton = new Button( );
				folderBrowserTreeView = new TreeView( );
				okButton = new Button( );
				cancelButton = new Button( );
				descriptionLabel = new Label( );
				
				imageList = new ImageList( );
				
				folderBrowserDialog.form.AcceptButton = okButton;
				folderBrowserDialog.form.CancelButton = cancelButton;
				
				SuspendLayout( );
				
				// descriptionLabel
				descriptionLabel.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Top | AnchorStyles.Left )
				| AnchorStyles.Right ) ) );
				descriptionLabel.Location = new Point( 17, 14 );
				descriptionLabel.Size = new Size( 290, 40 );
				descriptionLabel.TabIndex = 0;
				descriptionLabel.Text = folderBrowserDialog.Description;
				
				// folderBrowserTreeView
				folderBrowserTreeView.Anchor = ( (AnchorStyles)( ( ( ( AnchorStyles.Top | AnchorStyles.Bottom )
				| AnchorStyles.Left )
				| AnchorStyles.Right ) ) );
				folderBrowserTreeView.ImageIndex = -1;
				folderBrowserTreeView.Location = new Point( 20, 61 );
				folderBrowserTreeView.SelectedImageIndex = -1;
				folderBrowserTreeView.Size = new Size( 278, 153 );
				folderBrowserTreeView.TabIndex = 1;
				folderBrowserTreeView.ImageList = imageList;
				folderBrowserTreeView.ShowLines = false;
				folderBrowserTreeView.ShowPlusMinus = true;
				folderBrowserTreeView.HotTracking = true;
				//folderBrowserTreeView.Indent = 2;
				
				// newFolderButton
				newFolderButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Left ) ) );
				newFolderButton.FlatStyle = FlatStyle.System;
				newFolderButton.Location = new Point( 14, 230 );
				newFolderButton.Size = new Size( 125, 23 );
				newFolderButton.TabIndex = 2;
				newFolderButton.Text = "New Folder";
				newFolderButton.Enabled = folderBrowserDialog.ShowNewFolderButton;
				
				// okButton
				okButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Right ) ) );
				okButton.FlatStyle = FlatStyle.System;
				okButton.Location = new Point( 142, 230 );
				okButton.Size = new Size( 80, 23 );
				okButton.TabIndex = 3;
				okButton.Text = "OK";
				
				// cancelButton
				cancelButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Right ) ) );
				cancelButton.DialogResult = DialogResult.Cancel;
				cancelButton.FlatStyle = FlatStyle.System;
				cancelButton.Location = new Point( 226, 230 );
				cancelButton.Size = new Size( 80, 23 );
				cancelButton.TabIndex = 4;
				cancelButton.Text = "Cancel";
				
				// FolderBrowserDialog
				ClientSize = new Size( 322, 288 );
				Dock = DockStyle.Fill;

				Controls.Add( cancelButton );
				Controls.Add( okButton );
				Controls.Add( newFolderButton );
				Controls.Add( folderBrowserTreeView );
				Controls.Add( descriptionLabel );
				ResumeLayout( false );
				
				SetupImageList( );
				
				okButton.Click += new EventHandler( OnClickOKButton );
				cancelButton.Click += new EventHandler( OnClickCancelButton );

				string root_path = Environment.GetFolderPath (folderBrowserDialog.RootFolder);
				root_node = new TreeNode (Path.GetFileName (root_path));
				root_node.Tag = root_path;
				root_node.ImageIndex = NodeImageIndex (root_path);

				// If we add the sub nodes before the root is added to the
				// tree no refreshing will be done whil adding
				if (folderBrowserDialog.RootFolder == Environment.SpecialFolder.Desktop) {


					// Add something similar to 'My Computer'
					TreeNode mycomp = new TreeNode ("My Computer");
					if (Path.DirectorySeparatorChar == '/')
						mycomp.Tag = "/";
					else
						mycomp.Tag = Environment.GetFolderPath (Environment.SpecialFolder.MyComputer);
					mycomp.ImageIndex = NodeImageIndex ((string) mycomp.Tag);

					// A home directory
					string home_path = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
					TreeNode home = new TreeNode (Path.GetFileName (home_path));
					home.Tag = home_path;
					home.ImageIndex = NodeImageIndex (home_path);
					
					// This is so we get the expand box
					mycomp.Nodes.Add (new TreeNode (String.Empty));
					home.Nodes.Add (new TreeNode (String.Empty)); 

					root_node.Nodes.Add (mycomp);
					root_node.Nodes.Add (home);
					root_node.Expand ();
				} else {
					FillNode (root_node);
				}

				folderBrowserTreeView.Nodes.Add (root_node);

				folderBrowserTreeView.BeforeExpand += new TreeViewCancelEventHandler (OnBeforeExpand);
				folderBrowserTreeView.AfterSelect += new TreeViewEventHandler( OnAfterSelectFolderBrowserTreeView );
			}
			
			public string SelectedPath
			{
				set
				{
					selectedPath = value;
				}
				
				get
				{
					return selectedPath;
				}
			}
			
			void OnClickOKButton( object sender, EventArgs e )
			{
				folderBrowserDialog.SelectedPath = selectedPath;
				
				folderBrowserDialog.form.Controls.Remove( this );
				folderBrowserDialog.form.DialogResult = DialogResult.OK;
			}
			
			void OnClickCancelButton( object sender, EventArgs e )
			{
				folderBrowserDialog.form.Controls.Remove( this );
				folderBrowserDialog.form.DialogResult = DialogResult.Cancel;
			}
			
			void OnAfterSelectFolderBrowserTreeView( object sender, TreeViewEventArgs e )
			{
				if (e.Node == null)
					return;
				selectedPath = (string) e.Node.Tag;
			}

			private void OnBeforeExpand (object sender, TreeViewCancelEventArgs e)
			{
				if (e.Node == root_node)
					return;
				FillNode (e.Node);
			}

			private void OnAfterCollapse (object sender, TreeViewCancelEventArgs e)
			{
				if (e.Node == root_node)
					return;
				e.Node.Nodes.Clear ();
			}

			private void FillNode (TreeNode node)
			{
				Cursor old = folderBrowserTreeView.Cursor;
				folderBrowserTreeView.Cursor = Cursors.WaitCursor;

				folderBrowserTreeView.BeginUpdate ();

				node.Nodes.Clear ();
				string path = node.Tag as string;
				string [] dirs = Directory.GetDirectories (path);

				foreach (string s in dirs) {
					string name = Path.GetFileName (s);
					// filter out . directories
					if (name.StartsWith ("."))
						continue;
					TreeNode child = new TreeNode (name);
					child.Tag = s;
					child.ImageIndex = NodeImageIndex (s);

					try {
						// so we get the plus
						string [] subdirs = Directory.GetDirectories (s);
						foreach (string subdir in subdirs) {
							// filter out . directories (le sigh)
							string subdirname = Path.GetFileName (subdir);
							if (!subdirname.StartsWith (".")) {
								child.Nodes.Add (new TreeNode (String.Empty));
								break;
							}
						}
					} catch {
						// Probably don't have access
					}

					node.Nodes.Add (child);
				}

				folderBrowserTreeView.EndUpdate ();
				folderBrowserTreeView.Cursor = old;
			}

			private int NodeImageIndex( string path )
			{
				int index = 5;
				
				if ( path == Environment.GetFolderPath( Environment.SpecialFolder.Desktop ) )
					index = 1;
				else
				if ( path == Environment.GetFolderPath( Environment.SpecialFolder.Personal ) )
					index = 2;
				
				return index;
			}
			
			private void SetupImageList( )
			{
				imageList.ColorDepth = ColorDepth.Depth32Bit;
				imageList.ImageSize = new Size( 16, 16 );
				imageList.Images.Add( (Image)Locale.GetResource( "last_open" ) );
				imageList.Images.Add( (Image)Locale.GetResource( "desktop" ) );
				imageList.Images.Add( (Image)Locale.GetResource( "folder_with_paper" ) );
				imageList.Images.Add( (Image)Locale.GetResource( "monitor-computer" ) );
				imageList.Images.Add( (Image)Locale.GetResource( "monitor-planet" ) );
				imageList.Images.Add( (Image)Locale.GetResource( "folder" ) );
				imageList.Images.Add( (Image)Locale.GetResource( "paper" ) );
				imageList.TransparentColor = Color.Transparent;
			}
		}
		#endregion	// Internal Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler HelpRequest;
		#endregion
	}
}
