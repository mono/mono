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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
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
using System.Threading;

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
		
		private FolderBrowserDialogPanel folderBrowserDialogPanel;
		
		private bool treeViewFull = false;
		#endregion	// Local Variables
		
		#region Public Constructors
		public FolderBrowserDialog( )
		{
			form = new FolderBrowserDialogForm( this );
			
			form.Size =  new Size( 322, 288 );
			
			form.MinimumSize = new Size( 322, 288 );
			
			form.Text = "Search Folder";
			
			folderBrowserDialogPanel = new FolderBrowserDialogPanel( this );
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
				folderBrowserDialogPanel.DescriptionLabel.Text = value;
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
				folderBrowserDialogPanel.RootFolder = value;
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
				folderBrowserDialogPanel.SelectedPath = value;
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
				
				if ( value )
					folderBrowserDialogPanel.NewFolderButton.Enabled = true;
				else
					folderBrowserDialogPanel.NewFolderButton.Enabled = false;
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
			
			ResetPanelValues( );
		}
		
		private void ResetPanelValues( )
		{
			folderBrowserDialogPanel.NewFolderButton.Enabled = true;
			folderBrowserDialogPanel.RootFolder = rootFolder;
			folderBrowserDialogPanel.SelectedPath = "";
			folderBrowserDialogPanel.DescriptionLabel.Text = "";
		}
		
		protected override bool RunDialog( IntPtr hwndOwner )
		{
			form.Controls.Add( folderBrowserDialogPanel );
			if ( !treeViewFull )
			{
				folderBrowserDialogPanel.FillTreeView( );
				treeViewFull = true;
			}
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
			
			private Environment.SpecialFolder rootFolder;
			private string selectedPath;
			
			private ImageList imageList;
			
			private Hashtable dirHashTable = new Hashtable();
			
			private FolderBrowserTreeNode selectedPathNode = null;
			
			private string globalPath = "";
			
			private TreeNode globalTreeNode = null;
			
			private Thread reader_thread = null;
			
			public delegate void ThreadEventHandler( object sender, ThreadEventArgs e );
			
			private ThreadEventHandler OnThreadTreeViewUpdate;
			
			public FolderBrowserDialogPanel( FolderBrowserDialog folderBrowserDialog )
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
				descriptionLabel.Text = "";
				
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
				
				folderBrowserTreeView.AfterSelect += new TreeViewEventHandler( OnAfterSelectFolderBrowserTreeView );
				
				OnThreadTreeViewUpdate = new ThreadEventHandler( ThreadTreeViewUpdate );
			}
			
			public Label DescriptionLabel
			{
				set
				{
					descriptionLabel = value;
				}
				
				get
				{
					return descriptionLabel;
				}
			}
			
			public Button NewFolderButton
			{
				set
				{
					newFolderButton = value;
				}
				
				get
				{
					return newFolderButton;
				}
			}
			
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
				StopThread( );
				
				folderBrowserDialog.SelectedPath = selectedPath;
				
				folderBrowserDialog.form.Controls.Remove( this );
				folderBrowserDialog.form.DialogResult = DialogResult.OK;
			}
			
			void OnClickCancelButton( object sender, EventArgs e )
			{
				StopThread( );
				
				folderBrowserDialog.form.Controls.Remove( this );
				folderBrowserDialog.form.DialogResult = DialogResult.Cancel;
			}
			
			void OnAfterSelectFolderBrowserTreeView( object sender, TreeViewEventArgs e )
			{
				if ( e.Node == null ) return;
				
				FolderBrowserTreeNode tn = e.Node as FolderBrowserTreeNode;
				
				selectedPath = tn.FullPathName;
			}
			
			// FIXME
			// this needs some work, because almost no paths are available for
			// Environment.GetFolderPath( Environment.SpecialFolder.xxx)
			// under non windows platforms !!!!!!!!!
			public void FillTreeView( )
			{
				selectedPathNode = null;
				
				Cursor oldCursor = Cursor;
				Cursor = Cursors.WaitCursor;
				
				if ( rootFolder == Environment.SpecialFolder.Desktop )
				{
					folderBrowserTreeView.BeginUpdate( );
					string path = Environment.GetFolderPath( rootFolder );
					FolderBrowserTreeNode node = new FolderBrowserTreeNode( Path.GetFileName( path ) );
					node.FullPathName = path;
					node.ImageIndex = 1;
					folderBrowserTreeView.Nodes.Add( node );
					folderBrowserTreeView.EndUpdate( );
					
					globalPath = Environment.GetFolderPath( Environment.SpecialFolder.Personal );
					globalTreeNode = null;
					
					StartThread( );
					
//					folderBrowserTreeView.BeginUpdate();
//					GetAllSubDirs( Environment.GetFolderPath( Environment.SpecialFolder.MyComputer ), null );
//					folderBrowserTreeView.EndUpdate();
				}
				else
				{
					folderBrowserTreeView.BeginUpdate( );
					GetAllSubDirs( Environment.GetFolderPath( rootFolder ), null );
					folderBrowserTreeView.EndUpdate( );
				}
				
				if ( selectedPathNode != null )
				{
					folderBrowserTreeView.SelectedNode = selectedPathNode;
				}
				
				Cursor = oldCursor;
			}
			
			void StartThread( )
			{
				reader_thread = new Thread( new ThreadStart( ThreadFunc ) );
				reader_thread.Start( );
			}
			
			void StopThread( )
			{
				if ( reader_thread != null )
				{
					// is there any other safe and clean
					// way to stop a thread that calls a recursive
					// method ??
					// maybe throw some other exception...
					reader_thread.Abort( );
					reader_thread = null;
				}
			}
			
			void ThreadFunc( )
			{
				Console.WriteLine( "Starting thread..." );
				
				try
				{
					
					GetAllSubDirs( globalPath, globalTreeNode );
				}
				catch (ThreadAbortException)
				{
					Console.WriteLine( "Thread aborted..." );
				}
				finally
				{
					Console.WriteLine( "Leaving thread..." );
					reader_thread = null;
				}
			}
			
			void ThreadTreeViewUpdate( object sender, ThreadEventArgs e )
			{
				if ( e.ParentTreeNode == null )
				{
					folderBrowserTreeView.Nodes.Add( e.FolderBrowserTreeNode );
				}
				else
				{
					e.ParentTreeNode.Nodes.Add( e.FolderBrowserTreeNode );
				}
			}
			
			private void GetAllSubDirs( string path, TreeNode parent )
			{
				string shortname = Path.GetFileName( path );
				
				// no hidden dirs in *nix
				if ( shortname.StartsWith( "." ) ) return;
				
				FolderBrowserTreeNode node = new FolderBrowserTreeNode( shortname );
				node.FullPathName = path;
				
				if ( selectedPath == path )
				{
					selectedPathNode = node;
					node.EnsureVisible( );
				}
				
				node.ImageIndex = NodeImageIndex( path );
				
				BeginInvoke( OnThreadTreeViewUpdate, new object[] { this, new ThreadEventArgs( node, parent ) } );
				
				Thread.Sleep( 20 );
				
				try
				{
					string[] directories = Directory.GetDirectories( path );
					
					foreach ( string s in directories )
						GetAllSubDirs( s, node );
				}
				catch ( Exception ex ) // if we have no permission
				{
					Console.WriteLine( ex.ToString( ) );
				}
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
			
			public class ThreadEventArgs : EventArgs
			{
				FolderBrowserTreeNode folderBrowserTreeNode;
				
				TreeNode parentTreeNode;
				
				public FolderBrowserTreeNode FolderBrowserTreeNode
				{
					get
					{
						return folderBrowserTreeNode;
					}
				}
				
				public TreeNode ParentTreeNode
				{
					get
					{
						return parentTreeNode;
					}
				}
				
				public ThreadEventArgs( FolderBrowserTreeNode folderBrowserTreeNode, TreeNode parentTreeNode )
				{
					this.folderBrowserTreeNode = folderBrowserTreeNode;
					this.parentTreeNode = parentTreeNode;
				}
			}
			
			internal class FolderBrowserTreeNode : TreeNode
			{
				private string fullPathName = "";
				
				public FolderBrowserTreeNode( string text )
				: base( text )
				{}
				
				public string FullPathName
				{
					set
					{
						fullPathName = value;
					}
					
					get
					{
						return fullPathName;
					}
				}
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
