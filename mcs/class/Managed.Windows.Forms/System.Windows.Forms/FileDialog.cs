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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//
//  Alexander Olk	xenomorph2@onlinehome.de
//

// NOT COMPLETE - work in progress

using System;
using System.Drawing;
using System.ComponentModel;
using System.Resources;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace System.Windows.Forms
{
	public abstract class FileDialog : CommonDialog
	{
		internal FileDialogPanel fileDialogPanel;
		
		private bool addExtension = true;
		private bool checkFileExists = false;
		private bool checkPathExists = true;
		private string defaultExt = "";
		private bool dereferenceLinks = true;
		private string fileName = "";
		private string[] fileNames;
		private string filter;
		private int filterIndex = 1;
		private string initialDirectory = "";
		private bool restoreDirectory = false;
		private bool showHelp = false;
		private string title = "";
		private bool validateNames = true;
		
		internal string openSaveButtonText;
		internal string searchSaveLabelText;
		internal bool fileDialogShowReadOnly;
		
		public bool AddExtension
		{
			get
			{
				return addExtension;
			}
			
			set
			{
				addExtension = value;
			}
		}
		
		public virtual bool CheckFileExists
		{
			get
			{
				return checkFileExists;
			}
			
			set
			{
				checkFileExists = value;
			}
		}
		
		public bool CheckPathExists
		{
			get
			{
				return checkPathExists;
			}
			
			set
			{
				checkPathExists = value;
			}
		}
		
		public string DefaultExt
		{
			get
			{
				return defaultExt;
			}
			
			set
			{
				defaultExt = value;
			}
		}
		
		public bool DereferenceLinks
		{
			get
			{
				return dereferenceLinks;
			}
			
			set
			{
				dereferenceLinks = value;
			}
		}
		
		public string FileName
		{
			get
			{
				return fileName;
			}
			
			set
			{
				fileName = value;
			}
		}
		
		public string[] FileNames
		{
			get
			{
				return fileNames;
			}
		}
		
		public string Filter
		{
			get
			{
				return filter;
			}
			
			set
			{
				if ( value == null )
					throw new NullReferenceException( "Filter" );
				
				filter = value;
				
				SplitFilter( );
				
				fileDialogPanel.UpdateFilters( filterArrayList );
			}
		}
		
		public int FilterIndex
		{
			get
			{
				return filterIndex;
			}
			
			set
			{
				filterIndex = value;
			}
		}
		
		public string InitialDirectory
		{
			get
			{
				return initialDirectory;
			}
			
			set
			{
				initialDirectory = value;
			}
		}
		
		public bool RestoreDirectory
		{
			get
			{
				return restoreDirectory;
			}
			
			set
			{
				restoreDirectory = value;
			}
		}
		
		public bool ShowHelp
		{
			get
			{
				return showHelp;
			}
			
			set
			{
				showHelp = value;
				fileDialogPanel.ResizeAndRelocateForHelpOrReadOnly( );
			}
		}
		
		public string Title
		{
			get
			{
				return title;
			}
			
			set
			{
				title = value;
				
				form.Text = title;
			}
		}
		
		public bool ValidateNames
		{
			get
			{
				return validateNames;
			}
			
			set
			{
				validateNames = value;
			}
		}
		
		internal string OpenSaveButtonText
		{
			set
			{
				openSaveButtonText = value;
			}
			
			get
			{
				return openSaveButtonText;
			}
		}
		
		public string SearchSaveLabelText
		{
			set
			{
				searchSaveLabelText = value;
			}
			
			get
			{
				return searchSaveLabelText;
			}
		}
		
		public bool FileDialogShowReadOnly
		{
			set
			{
				fileDialogShowReadOnly = value;
			}
			
			get
			{
				return fileDialogShowReadOnly;
			}
		}
		
		public override void Reset( )
		{
			addExtension = true;
			checkFileExists = false;
			checkPathExists = true;
			defaultExt = "";
			dereferenceLinks = true;
			fileName = "";
			fileNames = null;
			filter = "";
			filterIndex = 1;
			initialDirectory = "";
			restoreDirectory = false;
			showHelp = false;
			title = "";
			validateNames = true;
		}
		
		public override string ToString( )
		{
			return base.ToString( );
		}
		
		public event CancelEventHandler FileOk;
		
		protected  override IntPtr HookProc( IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam )
		{
			throw new NotImplementedException( );
		}
		
		protected void OnFileOk( CancelEventArgs e )
		{
			if ( FileOk != null ) FileOk( this, e );
		}
		
		[MonoTODO]
		protected  override bool RunDialog( IntPtr hWndOwner )
		{
			form.Controls.Add( fileDialogPanel );
			
			return true;
		}
		
		private void SplitFilter( )
		{
			filterArrayList.Clear( );
			
			if ( filter == null )
				throw new NullReferenceException( "Filter" );
			
			// FIXME: default, MS default is nothing, nada...
			if ( filter.Length == 0 )
				filter = "All Files (*.*)|*.*";
			
			string[] filters = filter.Split( new Char[] {'|'} );
			
			if ( ( filters.Length % 2 ) != 0 )
				throw new ArgumentException( "Filter" );
			
			for ( int i = 0; i < filters.Length; i += 2 )
			{
				FilterStruct filterStruct = new FilterStruct( filters[ i ], filters[ i + 1 ] );
				
				filterArrayList.Add( filterStruct );
			}
			
//			if ( filterAL.Count > 1 )
//				filterAL.Sort();
		}
		
		internal void SendHelpRequest( EventArgs e )
		{
			OnHelpRequest( e );
		}
		
		internal struct FilterStruct
		{
			public string filterName;
			public StringCollection filters;
			
			public FilterStruct( string filterName, string filter )
			{
				this.filterName = filterName;
				
				filters =  new StringCollection( );
				
				SplitFilters( filter );
			}
			
			private void SplitFilters( string filter )
			{
				string[] split = filter.Split( new Char[] {';'} );
				
				filters.AddRange( split );
			}
		}
		
		internal ArrayList filterArrayList = new ArrayList();
		
		internal class FileDialogPanel : Panel
		{
			internal struct FileStruct
			{
				public FileStruct( string fullname, FileAttributes attributes )
				{
					this.fullname = fullname;
					this.attributes = attributes;
				}
				
				public string fullname;
				public FileAttributes attributes;
			}
			
			private Button cancelButton;
			private ToolBarButton upToolBarButton;
//			private ToolBar bigButtonToolBar;
			private ButtonPanel buttonPanel;
			private Button openSaveButton;
			private Button helpButton;
			private Label fileTypeLabel;
			private ToolBarButton menueToolBarButton;
			private ContextMenu menueToolBarButtonContextMenu;
			private ToolBarButton desktopToolBarButton;
			private ToolBar smallButtonToolBar;
			private ComboBox dirComboBox;
			private ToolBarButton lastUsedToolBarButton;
			private ComboBox fileNameComboBox;
			private ToolBarButton networkToolBarButton;
			private Label fileNameLabel;
			private FileListView fileListView;
			private Label searchSaveLabel;
			private ToolBarButton newdirToolBarButton;
			private ToolBarButton backToolBarButton;
			private ToolBarButton homeToolBarButton;
			private ToolBarButton workplaceToolBarButton;
			private ComboBox fileTypeComboBox;
//			private ImageList imageListLeftToolbar;
			private ImageList imageListTopToolbar;
			
			private FileDialog fileDialog;
			
			private string currentDirectoryName;
			
			// store current directoryInfo
			private DirectoryInfo directoryInfo;
			
			// store the FileStruct of all files in the current directory
			internal Hashtable fileHashtable = new Hashtable();
			
			// store DirectoryInfo for backButton
			internal Stack directoryStack = new Stack();
			
			internal string currentFileName = "";
			
			private MenuItem previousCheckedMenuItem;
			
			public FileDialogPanel( FileDialog fileDialog )
			{
				this.fileDialog = fileDialog;
				
				fileTypeComboBox = new ComboBox( );
				workplaceToolBarButton = new ToolBarButton( );
				homeToolBarButton = new ToolBarButton( );
				backToolBarButton = new ToolBarButton( );
				newdirToolBarButton = new ToolBarButton( );
				searchSaveLabel = new Label( );
				fileListView = new FileListView( this );
				fileNameLabel = new Label( );
				networkToolBarButton = new ToolBarButton( );
				fileNameComboBox = new ComboBox( );
				lastUsedToolBarButton = new ToolBarButton( );
				dirComboBox = new ComboBox( );
				smallButtonToolBar = new ToolBar( );
				desktopToolBarButton = new ToolBarButton( );
				menueToolBarButton = new ToolBarButton( );
				fileTypeLabel = new Label( );
				openSaveButton = new Button( );
				helpButton = new Button( );
//				bigButtonToolBar = new ToolBar( );
				buttonPanel = new ButtonPanel( this );
				upToolBarButton = new ToolBarButton( );
				cancelButton = new Button( );
				imageListTopToolbar = new ImageList( );
//				imageListLeftToolbar = new ImageList( );
				menueToolBarButtonContextMenu = new ContextMenu( );
				
				SuspendLayout( );
				
				// imageListLeftToolbar
//				imageListLeftToolbar.ColorDepth = ColorDepth.Depth32Bit;
//				imageListLeftToolbar.ImageSize = new Size( 48, 48 );
//				imageListLeftToolbar.Images.Add( (Image)Locale.GetResource( "last_open" ) );
//				imageListLeftToolbar.Images.Add( (Image)Locale.GetResource( "desktop" ) );
//				imageListLeftToolbar.Images.Add( (Image)Locale.GetResource( "folder_with_paper" ) );
//				imageListLeftToolbar.Images.Add( (Image)Locale.GetResource( "monitor-computer" ) );
//				imageListLeftToolbar.Images.Add( (Image)Locale.GetResource( "monitor-planet" ) );
//				imageListLeftToolbar.TransparentColor = Color.Transparent;
				
				//imageListTopToolbar
				imageListTopToolbar.ColorDepth = ColorDepth.Depth32Bit;
				imageListTopToolbar.ImageSize = new Size( 16, 16 ); // 16, 16
				imageListTopToolbar.Images.Add( (Image)Locale.GetResource( "back_arrow" ) );
				imageListTopToolbar.Images.Add( (Image)Locale.GetResource( "folder_arrow_up" ) );
				imageListTopToolbar.Images.Add( (Image)Locale.GetResource( "folder_star" ) );
				imageListTopToolbar.Images.Add( (Image)Locale.GetResource( "window" ) );
				imageListTopToolbar.TransparentColor = Color.Transparent;
				
				// searchLabel
				searchSaveLabel.FlatStyle = FlatStyle.System;
				searchSaveLabel.Location = new Point( 7, 8 );
				searchSaveLabel.Size = new Size( 72, 21 );
				searchSaveLabel.TabIndex = 0;
				searchSaveLabel.Text = fileDialog.SearchSaveLabelText;
				searchSaveLabel.TextAlign = ContentAlignment.MiddleRight;
				
				// dirComboBox
				dirComboBox.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Top | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				dirComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
				dirComboBox.Location = new Point( 99, 8 );
				dirComboBox.Size = new Size( 260, 21 );
				dirComboBox.TabIndex = 1;
				
				// fileListView
				fileListView.Anchor = ( (AnchorStyles)( ( ( ( AnchorStyles.Top | AnchorStyles.Bottom ) | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				fileListView.Location = new Point( 99, 37 );
				fileListView.Size = new Size( 449, 282 );
				fileListView.Columns.Add( " Name", 170, HorizontalAlignment.Left );
				fileListView.Columns.Add( "Size ", 80, HorizontalAlignment.Right );
				fileListView.Columns.Add( " Type", 100, HorizontalAlignment.Left );
				fileListView.Columns.Add( " Last Access", 150, HorizontalAlignment.Left );
				fileListView.AllowColumnReorder = true;
				fileListView.TabIndex = 2;
				
				// fileNameLabel
				fileNameLabel.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Left ) ) );
				fileNameLabel.FlatStyle = FlatStyle.System;
				fileNameLabel.Location = new Point( 102, 330 );
				fileNameLabel.Size = new Size( 70, 21 );
				fileNameLabel.TabIndex = 6;
				fileNameLabel.Text = "Filename:";
				fileNameLabel.TextAlign = ContentAlignment.MiddleLeft;
				
				// fileNameComboBox
				fileNameComboBox.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Bottom | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				fileNameComboBox.Location = new Point( 195, 330 );
				fileNameComboBox.Size = new Size( 245, 21 );
				fileNameComboBox.TabIndex = 4;
				
				// fileTypeLabel
				fileTypeLabel.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Left ) ) );
				fileTypeLabel.FlatStyle = FlatStyle.System;
				fileTypeLabel.Location = new Point( 102, 356 );
				fileTypeLabel.Size = new Size( 70, 21 );
				fileTypeLabel.TabIndex = 5;
				fileTypeLabel.Text = "Filetype:";
				fileTypeLabel.TextAlign = ContentAlignment.MiddleLeft;
				
				// fileTypeComboBox
				fileTypeComboBox.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Bottom | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				fileTypeComboBox.Location = new Point( 195, 356 );
				fileTypeComboBox.Size = new Size( 245, 21 );
				fileTypeComboBox.TabIndex = 6;
				
				// smallButtonToolBar
				smallButtonToolBar.Anchor = ( (AnchorStyles)( ( AnchorStyles.Top | AnchorStyles.Right ) ) );
				smallButtonToolBar.Appearance = ToolBarAppearance.Flat;
				smallButtonToolBar.AutoSize = false;
				smallButtonToolBar.Buttons.AddRange( new ToolBarButton[] {
														backToolBarButton,
														upToolBarButton,
														newdirToolBarButton,
														menueToolBarButton} );
				smallButtonToolBar.ButtonSize = new Size( 21, 16 ); // 21, 16
				smallButtonToolBar.Divider = false;
				smallButtonToolBar.Dock = DockStyle.None;
				smallButtonToolBar.DropDownArrows = true;
				smallButtonToolBar.ImageList = imageListTopToolbar;
				smallButtonToolBar.Location = new Point( 372, 8 );
				smallButtonToolBar.ShowToolTips = true;
				smallButtonToolBar.Size = new Size( 110, 20 );
				smallButtonToolBar.TabIndex = 10;
				smallButtonToolBar.TextAlign = ToolBarTextAlign.Right;
				
				// backToolBarButton
				backToolBarButton.ImageIndex = 0;
				backToolBarButton.Enabled = false;
				backToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
				
				// upToolBarButton
				upToolBarButton.ImageIndex = 1;
				upToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
				
				// newdirToolBarButton
				newdirToolBarButton.ImageIndex = 2;
				newdirToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
				
				// menueToolBarButton
				menueToolBarButton.ImageIndex = 3;
				menueToolBarButton.DropDownMenu = menueToolBarButtonContextMenu;
				menueToolBarButton.Style = ToolBarButtonStyle.DropDownButton;
				
//				// bigButtonToolBar
//				bigButtonToolBar.Appearance = ToolBarAppearance.Flat;
//				bigButtonToolBar.AutoSize = false;
//				bigButtonToolBar.Buttons.AddRange( new ToolBarButton[] {
//													  lastUsedToolBarButton,
//													  desktopToolBarButton,
//													  homeToolBarButton,
//													  workplaceToolBarButton,
//													  networkToolBarButton} );
//				bigButtonToolBar.ButtonSize = new Size( 82, 68 );
//				bigButtonToolBar.Dock = DockStyle.None;
//				bigButtonToolBar.Location = new Point( 7, 37 );
//				bigButtonToolBar.ShowToolTips = true;
//				bigButtonToolBar.Size = new Size( 85, 412 ); // 85, 412
//				bigButtonToolBar.ImageList = imageListLeftToolbar;
//				bigButtonToolBar.BackColor = Color.FromArgb( 128, 128, 128 );
//				bigButtonToolBar.TabIndex = 3;
				
				buttonPanel.Dock = DockStyle.None;
				buttonPanel.Location = new Point( 7, 37 );
				buttonPanel.TabIndex = 3;
				
				// lastUsedToolBarButton
//				lastUsedToolBarButton.ImageIndex = 0;
//				lastUsedToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
//				lastUsedToolBarButton.Text = "Last files";
				
				// desktopToolBarButton
//				desktopToolBarButton.ImageIndex = 1;
//				desktopToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
//				desktopToolBarButton.Text = "Desktop";
				
				// homeToolBarButton
//				homeToolBarButton.ImageIndex = 2;
//				homeToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
//				homeToolBarButton.Text = "Home";
				
				// workplaceToolBarButton
//				workplaceToolBarButton.ImageIndex = 3;
//				workplaceToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
//				workplaceToolBarButton.Text = "Workplace";
				
				// networkToolBarButton
//				networkToolBarButton.ImageIndex = 4;
//				networkToolBarButton.Style = ToolBarButtonStyle.ToggleButton;
//				networkToolBarButton.Text = "Network";
				
				// menueToolBarButtonContextMenu
				MenuItem mi = new MenuItem( "Small Icon", new EventHandler( OnClickMenuToolBarContextMenu ) );
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				mi = new MenuItem( "Tiles", new EventHandler( OnClickMenuToolBarContextMenu ) );
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				mi = new MenuItem( "Large Icon", new EventHandler( OnClickMenuToolBarContextMenu ) );
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				mi = new MenuItem( "List", new EventHandler( OnClickMenuToolBarContextMenu ) );
				mi.Checked = true;
				previousCheckedMenuItem = mi;
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				mi = new MenuItem( "Details", new EventHandler( OnClickMenuToolBarContextMenu ) );
				menueToolBarButtonContextMenu.MenuItems.Add( mi );
				
				// openButton
				openSaveButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Right ) ) );
				openSaveButton.FlatStyle = FlatStyle.System;
				openSaveButton.Location = new Point( 475, 330 );
				openSaveButton.Size = new Size( 72, 21 );
				openSaveButton.TabIndex = 8;
				openSaveButton.Text = fileDialog.OpenSaveButtonText;
				
				// cancelButton
				cancelButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Right ) ) );
				cancelButton.FlatStyle = FlatStyle.System;
				cancelButton.Location = new Point( 475, 356 );
				cancelButton.Size = new Size( 72, 21 );
				cancelButton.TabIndex = 9;
				cancelButton.Text = "Cancel";
				
				// helpButton
				helpButton.Anchor = ( (AnchorStyles)( ( AnchorStyles.Bottom | AnchorStyles.Right ) ) );
				helpButton.FlatStyle = FlatStyle.System;
				helpButton.Location = new Point( 475, 350 );
				helpButton.Size = new Size( 72, 21 );
				helpButton.TabIndex = 10;
				helpButton.Text = "Help";
				helpButton.Hide( );
				
				ClientSize = new Size( 554, 405 ); // 384
				
				Controls.Add( smallButtonToolBar );
				Controls.Add( cancelButton );
				Controls.Add( openSaveButton );
				Controls.Add( helpButton );
				Controls.Add( fileTypeLabel );
				Controls.Add( fileNameLabel );
				Controls.Add( fileTypeComboBox );
				Controls.Add( fileNameComboBox );
				Controls.Add( dirComboBox );
				Controls.Add( searchSaveLabel );
				Controls.Add( fileListView );
//				Controls.Add( bigButtonToolBar );
				Controls.Add( buttonPanel );
				
				ResumeLayout( false );
				
				if ( fileDialog.InitialDirectory == String.Empty )
					currentDirectoryName = Environment.CurrentDirectory;
				
				directoryInfo = new DirectoryInfo( currentDirectoryName );
				
				fileListView.UpdateFileListView( );
				
				openSaveButton.Click += new EventHandler( OnClickOpenButton );
				cancelButton.Click += new EventHandler( OnClickCancelButton );
				helpButton.Click += new EventHandler( OnClickHelpButton );
				
				smallButtonToolBar.ButtonClick += new ToolBarButtonClickEventHandler( OnClickSmallButtonToolBar );
				
				// Key events DONT'T work
				fileNameComboBox.KeyUp += new KeyEventHandler( OnKeyUpFileNameComboBox );
				
				// FIXME: Default for Filter is "", aka nothing, nada...
				// which means, show all files
				FilterStruct fs = new FilterStruct( "All Files (*.*)", "*.*" ); // set default filter;
				
				fileDialog.filterArrayList.Add( fs );
				
				UpdateFilters( fileDialog.filterArrayList );
			}
			
			public ComboBox FileNameComboBox
			{
				set
				{
					fileNameComboBox = value;
				}
				
				get
				{
					return fileNameComboBox;
				}
			}
			
			public string CurrentFileName
			{
				set
				{
					currentFileName = value;
				}
				
				get
				{
					return currentFileName;
				}
			}
			
			public DirectoryInfo DirectoryInfo
			{
				set
				{
					directoryInfo = value;
				}
				
				get
				{
					return directoryInfo;
				}
			}
			
			void OnClickOpenButton( object sender, EventArgs e )
			{
				Console.WriteLine( "OnClickOpenButton currentFileName: " + currentFileName );
				
				currentFileName.Trim( );
				
				if ( currentFileName.Length == 0 )
					return;
				
				if ( fileDialog.CheckFileExists )
				{
					if ( !File.Exists( currentFileName ) )
					{
						string message = currentFileName + " doesn't exist. Please verify that you have entered the correct file name.";
						MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OK, MessageBoxIcon.Warning );
						
						currentFileName = "";
						
						return;
					}
				}
				
				if ( fileDialog.CheckPathExists )
				{
					if ( !Directory.Exists( currentDirectoryName ) )
					{
						string message = currentDirectoryName + " doesn't exist. Please verify that you have entered the correct directory name.";
						MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OK, MessageBoxIcon.Warning );
						
						currentDirectoryName = Environment.CurrentDirectory;
						
						return;
					}
				}
				
				fileDialog.FileName = currentFileName;
				
				CancelEventArgs cancelEventArgs = new CancelEventArgs( );
				
				cancelEventArgs.Cancel = false;
				
				fileDialog.OnFileOk( cancelEventArgs );
				
				fileDialog.form.Controls.Remove( this );
				fileDialog.form.DialogResult = DialogResult.OK;
			}
			
			void OnClickCancelButton( object sender, EventArgs e )
			{
				CancelEventArgs cancelEventArgs = new CancelEventArgs( );
				
				cancelEventArgs.Cancel = true;
				
				fileDialog.OnFileOk( cancelEventArgs );
				
				fileDialog.form.Controls.Remove( this );
				fileDialog.form.DialogResult = DialogResult.Cancel;
			}
			
			void OnClickHelpButton( object sender, EventArgs e )
			{
				fileDialog.SendHelpRequest( e );
			}
			
			void OnClickSmallButtonToolBar( object sender, ToolBarButtonClickEventArgs e )
			{
				if ( e.Button == upToolBarButton )
				{
					if ( directoryInfo.Parent != null )
					{
						PushDirectory( directoryInfo );
						
						directoryInfo = directoryInfo.Parent;
						
						currentDirectoryName = directoryInfo.FullName;
						
						fileListView.UpdateFileListView( );
					}
				}
				else
				if ( e.Button == backToolBarButton )
				{
					if ( directoryStack.Count != 0 )
					{
						PopDirectory( );
						
						fileListView.UpdateFileListView( );
					}
				}
			}
			
			void OnClickMenuToolBarContextMenu( object sender, EventArgs e )
			{
				MenuItem senderMenuItem = (MenuItem)sender;
				
				previousCheckedMenuItem.Checked = false;
				senderMenuItem.Checked = true;
				previousCheckedMenuItem = senderMenuItem;
				
				// FIXME...
				
				switch ( senderMenuItem.Index  )
				{
					case 0:
						fileListView.View = View.SmallIcon;
						break;
					case 1:
						fileListView.View = View.LargeIcon;
						break;
					case 2:
						fileListView.View = View.LargeIcon;
						break;
					case 3:
						fileListView.View = View.List;
						break;
					case 4:
						fileListView.View = View.Details;
						break;
					default:
						break;
				}
				
			}
			
			void OnKeyUpFileNameComboBox( object sender, KeyEventArgs e )
			{
				Console.WriteLine( "OnKeyUpFileNameComboBox" );
				
				if ( e.KeyCode == Keys.Enter )
				{
					Console.WriteLine( "OnKeyUpFileNameComboBox e.KeyCode =..." );
					currentFileName = currentDirectoryName + fileNameComboBox.Text;
					ForceDialogEnd( );
				}
			}
			
			public void UpdateFilters( ArrayList filters )
			{
				fileTypeComboBox.Items.Clear( );
				
				fileTypeComboBox.BeginUpdate( );
				
				foreach ( FilterStruct fs in filters )
				{
					fileTypeComboBox.Items.Add( fs.filterName );
				}
				
				fileTypeComboBox.SelectedIndex = fileDialog.FilterIndex - 1;
				
				fileTypeComboBox.EndUpdate( );
				
				// TODO: Update the fileListView also...
			}
			
			public void ChangeDirectory( string path )
			{
				currentDirectoryName = path;
				
				PushDirectory( directoryInfo );
				
				directoryInfo = new DirectoryInfo( path );
				
				fileListView.UpdateFileListView( );
			}
			
			public void ForceDialogEnd( )
			{
				OnClickOpenButton( this, EventArgs.Empty );
			}
			
			private void PushDirectory( DirectoryInfo di )
			{
				directoryStack.Push( directoryInfo );
				backToolBarButton.Enabled = true;
			}
			
			private void PopDirectory( )
			{
				directoryInfo = (DirectoryInfo)directoryStack.Pop( );
				
				currentDirectoryName = directoryInfo.FullName;
				
				if ( directoryStack.Count == 0 )
					backToolBarButton.Enabled = false;
			}
			
			public void ResizeAndRelocateForHelpOrReadOnly( )
			{
				if ( fileDialog.ShowHelp || fileDialog.FileDialogShowReadOnly )
				{
					fileListView.Size = new Size( 449, 250 );
					fileNameLabel.Location = new Point( 102, 298 );
					fileNameComboBox.Location = new Point( 195, 298 );
					fileTypeLabel.Location = new Point( 102, 324 );
					fileTypeComboBox.Location = new Point( 195, 324 );
					openSaveButton.Location = new Point( 475, 298 );
					cancelButton.Location = new Point( 475, 324 );
				}
				else
				{
					fileListView.Size = new Size( 449, 282 );
					fileNameLabel.Location = new Point( 102, 330 );
					fileNameComboBox.Location = new Point( 195, 330 );
					fileTypeLabel.Location = new Point( 102, 356 );
					fileTypeComboBox.Location = new Point( 195, 356 );
					openSaveButton.Location = new Point( 475, 330 );
					cancelButton.Location = new Point( 475, 356 );
				}
				
				if ( fileDialog.ShowHelp )
					helpButton.Show( );
			}
			
			// FileListView
			internal class FileListView : ListView
			{
				private ImageList fileListViewSmallImageList = new ImageList();
				private ImageList fileListViewBigImageList = new ImageList();
				
				private FileDialogPanel fileDialogPanel;
				
				public FileListView( FileDialogPanel fileDialogPanel )
				{
					this.fileDialogPanel = fileDialogPanel;
					
					fileListViewSmallImageList.ColorDepth = ColorDepth.Depth32Bit;
					fileListViewSmallImageList.ImageSize = new Size( 16, 16 );
					fileListViewSmallImageList.Images.Add( (Image)Locale.GetResource( "paper" ) );
					fileListViewSmallImageList.Images.Add( (Image)Locale.GetResource( "folder" ) );
					fileListViewSmallImageList.TransparentColor = Color.Transparent;
					
					fileListViewBigImageList.ColorDepth = ColorDepth.Depth32Bit;
					fileListViewBigImageList.ImageSize = new Size( 48, 48 );
					fileListViewBigImageList.Images.Add( (Image)Locale.GetResource( "paper" ) );
					fileListViewBigImageList.Images.Add( (Image)Locale.GetResource( "folder" ) );
					fileListViewBigImageList.TransparentColor = Color.Transparent;
					
					SmallImageList = fileListViewSmallImageList;
					LargeImageList = fileListViewBigImageList;
					
					View = View.List;
				}
				
				public void UpdateFileListView( )
				{
					DirectoryInfo directoryInfo = fileDialogPanel.DirectoryInfo;
					
					DirectoryInfo[] directoryInfoArray = directoryInfo.GetDirectories( );
					FileInfo[] fileInfoArray = directoryInfo.GetFiles( );
					
					fileDialogPanel.fileHashtable.Clear( );
					
					BeginUpdate( );
					
					Items.Clear( );
					
					foreach ( DirectoryInfo directoryInfoi in directoryInfoArray )
					{
						FileStruct fileStruct = new FileStruct( );
						
						fileStruct.fullname = directoryInfoi.FullName;
						
						ListViewItem listViewItem = new ListViewItem( directoryInfoi.Name );
						
						listViewItem.ImageIndex = 1;
						
						listViewItem.SubItems.Add( "" );
						listViewItem.SubItems.Add( "Directory" );
						listViewItem.SubItems.Add( directoryInfoi.LastAccessTime.ToShortDateString( ) + " " + directoryInfoi.LastAccessTime.ToShortTimeString( ) );
						
						fileStruct.attributes = FileAttributes.Directory;
						
						fileDialogPanel.fileHashtable.Add( directoryInfoi.Name, fileStruct );
						
						Items.Add( listViewItem );
					}
					
					foreach ( FileInfo fileInfo in fileInfoArray )
					{
						FileStruct fileStruct = new FileStruct( );
						
						fileStruct.fullname = fileInfo.FullName;
						
						ListViewItem listViewItem = new ListViewItem( fileInfo.Name );
						
						listViewItem.ImageIndex = 0;
						
						long fileLen = 1;
						if ( fileInfo.Length > 1024 )
							fileLen = fileInfo.Length / 1024;
						
						listViewItem.SubItems.Add( fileLen.ToString( ) + " KB" );
						listViewItem.SubItems.Add( "File" );
						listViewItem.SubItems.Add( fileInfo.LastAccessTime.ToShortDateString( ) + " " + fileInfo.LastAccessTime.ToShortTimeString( ) );
						
						fileStruct.attributes = FileAttributes.Normal;
						
						fileDialogPanel.fileHashtable.Add( fileInfo.Name, fileStruct );
						
						Items.Add( listViewItem );
					}
					
					Console.WriteLine( "Items.Count: " + Items.Count );
					
					EndUpdate( );
				}
				
				protected override void OnClick( EventArgs e )
				{
					ListViewItem listViewItem = SelectedItems[ 0 ];
					
					FileStruct fileStruct = (FileStruct)fileDialogPanel.fileHashtable[ listViewItem.Text ];
					
					if ( fileStruct.attributes != FileAttributes.Directory )
					{
						fileDialogPanel.FileNameComboBox.Text = listViewItem.Text;
						fileDialogPanel.CurrentFileName = fileStruct.fullname;
					}
					
					base.OnClick( e );
				}
				
				protected override void OnDoubleClick( EventArgs e )
				{
					ListViewItem listViewItem = SelectedItems[ 0 ];
					
					FileStruct fileStruct = (FileStruct)fileDialogPanel.fileHashtable[ listViewItem.Text ];
					
					if ( fileStruct.attributes == FileAttributes.Directory )
					{
						fileDialogPanel.ChangeDirectory( fileStruct.fullname );
					}
					else
					{
						fileDialogPanel.FileNameComboBox.Text =  listViewItem.Text;
						fileDialogPanel.CurrentFileName = fileStruct.fullname;
						fileDialogPanel.ForceDialogEnd( );
						return;
					}
					
					base.OnDoubleClick( e );
				}
			}
			
			// helper class until ToolBar is working correctly
			internal class ButtonPanel : Panel
			{
				private FileDialogPanel fileDialogPanel;
				
				private Button lastOpenButton;
				private Button desktopButton;
				private Button homeButton;
				private Button workplaceButton;
				private Button networkButton;
				
				private ImageList imageList = new ImageList();
				
				public ButtonPanel( FileDialogPanel fileDialogPanel )
				{
					this.fileDialogPanel = fileDialogPanel;
					
					BorderStyle = BorderStyle.Fixed3D;
					BackColor = Color.FromArgb( 128, 128, 128 );
					Size = new Size( 85, 336 );
					
					// use ImageList to scale the bitmaps
					imageList.ColorDepth = ColorDepth.Depth32Bit;
					imageList.ImageSize = new Size( 38, 38 );
					imageList.Images.Add( (Image)Locale.GetResource( "last_open" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "desktop" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "folder_with_paper" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "monitor-computer" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "monitor-planet" ) );
					imageList.TransparentColor = Color.Transparent;
					
					lastOpenButton = new Button( );
					desktopButton = new Button( );
					homeButton = new Button( );
					workplaceButton = new Button( );
					networkButton = new Button( );
					
					lastOpenButton.Image = imageList.Images[ 0 ];
					lastOpenButton.ImageAlign = ContentAlignment.TopCenter;
					lastOpenButton.TextAlign = ContentAlignment.BottomCenter;
					lastOpenButton.ForeColor = Color.White;
					lastOpenButton.FlatStyle = FlatStyle.Popup;
					lastOpenButton.Size = new Size( 82, 64 );
					lastOpenButton.Location = new Point( 0, 2 );
					lastOpenButton.Text = "Last Open";
					lastOpenButton.Click += new EventHandler( OnClickButton );
					
					desktopButton.Image = imageList.Images[ 1 ];
					desktopButton.ImageAlign = ContentAlignment.TopCenter;
					desktopButton.TextAlign = ContentAlignment.BottomCenter;
					desktopButton.ForeColor = Color.White;
					desktopButton.FlatStyle = FlatStyle.Popup;
					desktopButton.Size = new Size( 82, 64 );
					desktopButton.Location = new Point( 0, 66 );
					desktopButton.Text = "Desktop";
					desktopButton.Click += new EventHandler( OnClickButton );
					
					homeButton.Image = imageList.Images[ 2 ];
					homeButton.ImageAlign = ContentAlignment.TopCenter;
					homeButton.TextAlign = ContentAlignment.BottomCenter;
					homeButton.ForeColor = Color.White;
					homeButton.FlatStyle = FlatStyle.Popup;
					homeButton.Size = new Size( 82, 64 );
					homeButton.Location = new Point( 0, 130 );
					homeButton.Text = "Home";
					homeButton.Click += new EventHandler( OnClickButton );
					
					workplaceButton.Image = imageList.Images[ 3 ];
					workplaceButton.ImageAlign = ContentAlignment.TopCenter;
					workplaceButton.TextAlign = ContentAlignment.BottomCenter;
					workplaceButton.ForeColor = Color.White;
					workplaceButton.FlatStyle = FlatStyle.Popup;
					workplaceButton.Size = new Size( 82, 64 );
					workplaceButton.Location = new Point( 0, 194 );
					workplaceButton.Text = "Workplace";
					workplaceButton.Click += new EventHandler( OnClickButton );
					
					networkButton.Image = imageList.Images[ 4 ];
					networkButton.ImageAlign = ContentAlignment.TopCenter;
					networkButton.TextAlign = ContentAlignment.BottomCenter;
					networkButton.ForeColor = Color.White;
					networkButton.FlatStyle = FlatStyle.Popup;
					networkButton.Size = new Size( 82, 64 );
					networkButton.Location = new Point( 0, 258 );
					networkButton.Text = "Network";
					networkButton.Click += new EventHandler( OnClickButton );
					
					Controls.Add( lastOpenButton );
					Controls.Add( desktopButton );
					Controls.Add( homeButton );
					Controls.Add( workplaceButton );
					Controls.Add( networkButton );
				}
				
				void OnClickButton( object sender, EventArgs e )
				{
					if ( sender == lastOpenButton )
					{
						
					}
					else
					if ( sender == desktopButton )
					{
						fileDialogPanel.ChangeDirectory( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ) );
					}
					else
					if ( sender == homeButton )
					{
						fileDialogPanel.ChangeDirectory( Environment.GetFolderPath( Environment.SpecialFolder.Personal ) );
					}
					else
					if ( sender == workplaceButton )
					{
//						fileDialogPanel.ChangeDirectory( Environment.GetFolderPath( Environment.SpecialFolder.MyComputer ) );
					}
					else
					if ( sender == networkButton )
					{
						
					}
				}
			}
		}
	}
}


