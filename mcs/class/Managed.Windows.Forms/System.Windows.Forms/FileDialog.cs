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

// TODO:
// file/path security stuff ???

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
		internal enum FileDialogType
		{
			OpenFileDialog,
			SaveFileDialog
		}
		
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
		internal bool fileDialogMultiSelect;
		internal bool saveDialogCreatePrompt = false;
		internal bool saveDialogOverwritePrompt = true;
		
		private bool showHiddenFiles = false;
		
		internal FileDialogType fileDialogType;
		
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
				
				// if there is a dot remove it and everything before it
				if ( defaultExt.LastIndexOf( '.' ) != - 1 )
				{
					string[] split = defaultExt.Split( new char[] { '.' } );
					defaultExt = split[ split.Length - 1 ];
				}
			}
		}
		
		// in MS.NET it doesn't make a difference if
		// DerefenceLinks is true or false
		// if the selected file is a link FileDialog
		// always returns the link
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
				if ( fileDialogMultiSelect )
					return fileNames;
				
				return null;
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
		
		// this one is a hard one ;)
		// Win32 filename:
		// - up to MAX_PATH characters (windef.h) = 260
		// - no trailing dots or spaces
		// - case preserving
		// - etc...
		// NTFS/Posix filename:
		// - up to 32,768 Unicode characters
		// - trailing periods or spaces
		// - case sensitive
		// - etc...
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
		
		internal string SearchSaveLabelText
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
		
		internal bool FileDialogShowReadOnly
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
		
		internal bool FileDialogMultiSelect
		{
			set
			{
				fileDialogMultiSelect = value;
				fileDialogPanel.MultiSelect = value;
			}
			
			get
			{
				return fileDialogMultiSelect;
			}
		}
		
		internal ArrayList FilterArrayList
		{
			get
			{
				return filterArrayList;
			}
		}
		
		// extension to MS.NET framework...
		public bool ShowHiddenFiles
		{
			set
			{
				showHiddenFiles = value;
			}
			
			get
			{
				return showHiddenFiles;
			}
		}
		
		internal bool SaveDialogCreatePrompt
		{
			set
			{
				saveDialogCreatePrompt = value;
			}
			
			get
			{
				return saveDialogCreatePrompt;
			}
		}
		
		internal bool SaveDialogOverwritePrompt
		{
			set
			{
				saveDialogOverwritePrompt = value;
			}
			
			get
			{
				return saveDialogOverwritePrompt;
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
			
			if ( filter.Length == 0 )
				return;
			
			string[] filters = filter.Split( new Char[] {'|'} );
			
			if ( ( filters.Length % 2 ) != 0 )
				throw new ArgumentException( "Filter" );
			
			for ( int i = 0; i < filters.Length; i += 2 )
			{
				FilterStruct filterStruct = new FilterStruct( filters[ i ], filters[ i + 1 ] );
				
				filterArrayList.Add( filterStruct );
			}
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
		
		internal void SetFilenames( string[] filenames )
		{
			fileNames = filenames;
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
			private ButtonPanel buttonPanel;
			private Button openSaveButton;
			private Button helpButton;
			private Label fileTypeLabel;
			private ToolBarButton menueToolBarButton;
			private ContextMenu menueToolBarButtonContextMenu;
			private ToolBarButton desktopToolBarButton;
			private ToolBar smallButtonToolBar;
			private DirComboBox dirComboBox;
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
			private ImageList imageListTopToolbar;
			
			internal FileDialog fileDialog;
			
			private string currentDirectoryName;
			
			internal string currentFileName = "";
			
			// store current directoryInfo
			private DirectoryInfo directoryInfo;
			
			// store the FileStruct of all files in the current directory
			internal Hashtable fileHashtable = new Hashtable();
			
			// store DirectoryInfo for backButton
			internal Stack directoryStack = new Stack();
			
			private MenuItem previousCheckedMenuItem;
			
			private bool multiSelect = false;
			
			private string restoreDirectory = "";
			
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
				dirComboBox = new DirComboBox( this );
				smallButtonToolBar = new ToolBar( );
				desktopToolBarButton = new ToolBarButton( );
				menueToolBarButton = new ToolBarButton( );
				fileTypeLabel = new Label( );
				openSaveButton = new Button( );
				fileDialog.form.AcceptButton = openSaveButton;
				helpButton = new Button( );
				buttonPanel = new ButtonPanel( this );
				upToolBarButton = new ToolBarButton( );
				cancelButton = new Button( );
				fileDialog.form.CancelButton = cancelButton;
				imageListTopToolbar = new ImageList( );
				menueToolBarButtonContextMenu = new ContextMenu( );
				
				SuspendLayout( );
				
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
				
				// fileListView
				fileListView.Anchor = ( (AnchorStyles)( ( ( ( AnchorStyles.Top | AnchorStyles.Bottom ) | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				fileListView.Location = new Point( 99, 37 );
				fileListView.Size = new Size( 449, 282 );
				fileListView.Columns.Add( " Name", 170, HorizontalAlignment.Left );
				fileListView.Columns.Add( "Size ", 80, HorizontalAlignment.Right );
				fileListView.Columns.Add( " Type", 100, HorizontalAlignment.Left );
				fileListView.Columns.Add( " Last Access", 150, HorizontalAlignment.Left );
				fileListView.AllowColumnReorder = true;
				fileListView.MultiSelect = false;
				fileListView.TabIndex = 2;
				
				// dirComboBox
				dirComboBox.Anchor = ( (AnchorStyles)( ( ( AnchorStyles.Top | AnchorStyles.Left ) | AnchorStyles.Right ) ) );
				dirComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
				dirComboBox.Location = new Point( 99, 8 );
				dirComboBox.Size = new Size( 260, 21 );
				dirComboBox.TabIndex = 1;
				
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
				
				buttonPanel.Dock = DockStyle.None;
				buttonPanel.Location = new Point( 7, 37 );
				buttonPanel.TabIndex = 3;
				
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
				Controls.Add( fileListView );
				Controls.Add( fileTypeLabel );
				Controls.Add( fileNameLabel );
				Controls.Add( fileTypeComboBox );
				Controls.Add( fileNameComboBox );
				Controls.Add( dirComboBox );
				Controls.Add( searchSaveLabel );
				Controls.Add( buttonPanel );
				
				ResumeLayout( false );
				
				if ( fileDialog.InitialDirectory == String.Empty )
					currentDirectoryName = Environment.CurrentDirectory;
				else
					currentDirectoryName = fileDialog.InitialDirectory;
				
				directoryInfo = new DirectoryInfo( currentDirectoryName );
				
				dirComboBox.CurrentPath = currentDirectoryName;
				
				if ( fileDialog.RestoreDirectory )
					restoreDirectory = currentDirectoryName;
				
				fileListView.UpdateFileListView( );
				
				openSaveButton.Click += new EventHandler( OnClickOpenSaveButton );
				cancelButton.Click += new EventHandler( OnClickCancelButton );
				helpButton.Click += new EventHandler( OnClickHelpButton );
				
				smallButtonToolBar.ButtonClick += new ToolBarButtonClickEventHandler( OnClickSmallButtonToolBar );
				
				// Key events DONT'T work
				fileNameComboBox.KeyUp += new KeyEventHandler( OnKeyUpFileNameComboBox );
				
				fileTypeComboBox.SelectedIndexChanged += new EventHandler( OnSelectedIndexChangedFileTypeComboBox );
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
			
			public bool MultiSelect
			{
				set
				{
					multiSelect = value;
					fileListView.MultiSelect = value;
				}
				
				get
				{
					return multiSelect;
				}
			}
			
			void OnClickOpenSaveButton( object sender, EventArgs e )
			{
				if ( !multiSelect )
				{
					string fileFromComboBox = fileNameComboBox.Text.Trim( );
					
					if ( fileFromComboBox.Length > 0 )
						fileFromComboBox = Path.Combine( currentDirectoryName, fileFromComboBox );
					
					if ( currentFileName != fileFromComboBox )
						currentFileName = fileFromComboBox;
					
					if ( currentFileName.Length == 0 )
						return;
					
					
					if ( fileDialog.fileDialogType == FileDialogType.OpenFileDialog )
					{
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
					}
					else // FileDialogType == SaveFileDialog
					{
						if ( fileDialog.SaveDialogOverwritePrompt )
						{
							if ( File.Exists( currentFileName ) )
							{
								string message = currentFileName + " exists. Overwrite ?";
								DialogResult dr = MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning );
								
								if ( dr == DialogResult.Cancel )
								{
									currentFileName = "";
									
									return;
								}
							}
						}
						
						if ( fileDialog.SaveDialogCreatePrompt )
						{
							if ( !File.Exists( currentFileName ) )
							{
								string message = currentFileName + " doesn't exist. Create ?";
								DialogResult dr = MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning );
								
								if ( dr == DialogResult.Cancel )
								{
									currentFileName = "";
									
									return;
								}
							}
						}
					}
					
					if ( fileDialog.fileDialogType == FileDialogType.SaveFileDialog )
					{
						if ( fileDialog.AddExtension && fileDialog.DefaultExt.Length > 0 )
						{
							if ( !currentFileName.EndsWith( fileDialog.DefaultExt ) )
							{
								currentFileName += "." + fileDialog.DefaultExt;
							}
						}
					}
					
					fileDialog.FileName = currentFileName;
				}
				else // multiSelect = true
				if ( fileDialog.fileDialogType != FileDialogType.SaveFileDialog )
				{
					if ( fileListView.SelectedItems.Count > 0 )
					{
						// first remove all selected directories
						ArrayList al = new ArrayList( );
						
						foreach ( ListViewItem lvi in fileListView.SelectedItems )
						{
							FileStruct fileStruct = (FileStruct)fileHashtable[ lvi.Text ];
							
							if ( fileStruct.attributes != FileAttributes.Directory )
							{
								al.Add( fileStruct );
							}
						}
						
						fileDialog.FileName = ( (FileStruct)al[ 0 ] ).fullname;
						
						string[] filenames = new string[ al.Count ];
						
						for ( int i = 0; i < al.Count; i++ )
						{
							filenames[ i ] = ( (FileStruct)al[ i ] ).fullname;
						}
						
						fileDialog.SetFilenames( filenames );
					}
				}
				
				if ( fileDialog.CheckPathExists )
				{
					if ( !Directory.Exists( currentDirectoryName ) )
					{
						string message = currentDirectoryName + " doesn't exist. Please verify that you have entered the correct directory name.";
						MessageBox.Show( message, fileDialog.OpenSaveButtonText, MessageBoxButtons.OK, MessageBoxIcon.Warning );
						
						if ( fileDialog.InitialDirectory == String.Empty )
							currentDirectoryName = Environment.CurrentDirectory;
						else
							currentDirectoryName = fileDialog.InitialDirectory;
						
						return;
					}
				}
				
				if ( fileDialog.RestoreDirectory )
					currentDirectoryName = restoreDirectory;
				
				CancelEventArgs cancelEventArgs = new CancelEventArgs( );
				
				cancelEventArgs.Cancel = false;
				
				fileDialog.OnFileOk( cancelEventArgs );
				
				fileDialog.form.Controls.Remove( this );
				fileDialog.form.DialogResult = DialogResult.OK;
			}
			
			void OnClickCancelButton( object sender, EventArgs e )
			{
				if ( fileDialog.RestoreDirectory )
					currentDirectoryName = restoreDirectory;
				
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
						
						dirComboBox.CurrentPath = currentDirectoryName;
						
						fileListView.UpdateFileListView( );
					}
				}
				else
				if ( e.Button == backToolBarButton )
				{
					if ( directoryStack.Count != 0 )
					{
						PopDirectory( );
						
						dirComboBox.CurrentPath = currentDirectoryName;
						
						fileListView.UpdateFileListView( );
					}
				}
				else
				if ( e.Button == newdirToolBarButton )
				{
					
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
				if ( e.KeyCode == Keys.Enter )
				{
					currentFileName = currentDirectoryName + fileNameComboBox.Text;
					ForceDialogEnd( );
				}
			}
			
			void OnSelectedIndexChangedFileTypeComboBox( object sender, EventArgs e )
			{
				fileDialog.FilterIndex = fileTypeComboBox.SelectedIndex + 1;
				
				fileListView.UpdateFileListView( );
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
				
				fileListView.UpdateFileListView( );
			}
			
			public void ChangeDirectory( object sender, string path )
			{
				currentDirectoryName = path;
				
				PushDirectory( directoryInfo );
				
				directoryInfo = new DirectoryInfo( path );
				
				if ( sender != dirComboBox )
					dirComboBox.CurrentPath = path;
				
				fileListView.UpdateFileListView( );
			}
			
			public void ForceDialogEnd( )
			{
				OnClickOpenSaveButton( this, EventArgs.Empty );
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
				
				private ArrayList GetFileInfoArrayList( DirectoryInfo directoryInfo )
				{
					ArrayList arrayList = new ArrayList( );
					
					if ( fileDialogPanel.fileDialog.FilterArrayList.Count != 0 )
					{
						FilterStruct fs = (FilterStruct)fileDialogPanel.fileDialog.FilterArrayList[ fileDialogPanel.fileDialog.FilterIndex - 1 ];
						
						foreach ( string s in fs.filters )
							arrayList.AddRange( directoryInfo.GetFiles( s ) );
					}
					else
						arrayList.AddRange( directoryInfo.GetFiles( ) );
					
					return arrayList;
				}
				
				public void UpdateFileListView( )
				{
					DirectoryInfo directoryInfo = fileDialogPanel.DirectoryInfo;
					
					DirectoryInfo[] directoryInfoArray = directoryInfo.GetDirectories( );
					
					ArrayList fileInfoArrayList = GetFileInfoArrayList( directoryInfo );
					
					fileDialogPanel.fileHashtable.Clear( );
					
					BeginUpdate( );
					
					Items.Clear( );
					
					foreach ( DirectoryInfo directoryInfoi in directoryInfoArray )
					{
						if ( !fileDialogPanel.fileDialog.ShowHiddenFiles )
							if ( directoryInfoi.Name.StartsWith( "." ) || directoryInfoi.Attributes == FileAttributes.Hidden )
								continue;
						
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
					
					foreach ( FileInfo fileInfo in fileInfoArrayList )
					{
						if ( !fileDialogPanel.fileDialog.ShowHiddenFiles )
							if ( fileInfo.Name.StartsWith( "." )  || fileInfo.Attributes == FileAttributes.Hidden )
								continue;
						
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
					
					EndUpdate( );
				}
				
				protected override void OnClick( EventArgs e )
				{
					if ( !MultiSelect )
					{
						if ( SelectedItems.Count > 0 )
						{
							ListViewItem listViewItem = SelectedItems[ 0 ];
							
							FileStruct fileStruct = (FileStruct)fileDialogPanel.fileHashtable[ listViewItem.Text ];
							
							if ( fileStruct.attributes != FileAttributes.Directory )
							{
								fileDialogPanel.FileNameComboBox.Text = listViewItem.Text;
								fileDialogPanel.CurrentFileName = fileStruct.fullname;
							}
						}
					}
					
					base.OnClick( e );
				}
				
				protected override void OnDoubleClick( EventArgs e )
				{
					if ( SelectedItems.Count > 0 )
					{
						ListViewItem listViewItem = SelectedItems[ 0 ];
						
						FileStruct fileStruct = (FileStruct)fileDialogPanel.fileHashtable[ listViewItem.Text ];
						
						if ( fileStruct.attributes == FileAttributes.Directory )
						{
							fileDialogPanel.ChangeDirectory( this, fileStruct.fullname );
						}
						else
						{
							fileDialogPanel.FileNameComboBox.Text =  listViewItem.Text;
							fileDialogPanel.CurrentFileName = fileStruct.fullname;
							fileDialogPanel.ForceDialogEnd( );
							return;
						}
					}
					
					base.OnDoubleClick( e );
				}
				
				protected override void OnSelectedIndexChanged( EventArgs e )
				{
					if ( MultiSelect )
					{
						if ( SelectedItems.Count > 0 )
						{
							string combotext = "";
							
							if ( SelectedItems.Count == 1 )
							{
								FileStruct fileStruct = (FileStruct)fileDialogPanel.fileHashtable[ SelectedItems[ 0 ].Text ];
								
								if ( fileStruct.attributes != FileAttributes.Directory )
									combotext = SelectedItems[ 0 ].Text;
							}
							else
							{
								foreach ( ListViewItem lvi in SelectedItems )
								{
									FileStruct fileStruct = (FileStruct)fileDialogPanel.fileHashtable[ lvi.Text ];
									
									if ( fileStruct.attributes != FileAttributes.Directory )
										combotext += "\"" + lvi.Text + "\" ";
								}
							}
							
							fileDialogPanel.FileNameComboBox.Text = combotext;
						}
					}
					
					base.OnSelectedIndexChanged( e );
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
						fileDialogPanel.ChangeDirectory( this, Environment.GetFolderPath( Environment.SpecialFolder.Desktop ) );
					}
					else
					if ( sender == homeButton )
					{
						fileDialogPanel.ChangeDirectory( this, Environment.GetFolderPath( Environment.SpecialFolder.Personal ) );
					}
					else
					if ( sender == workplaceButton )
					{
//						fileDialogPanel.ChangeDirectory( this, Environment.GetFolderPath( Environment.SpecialFolder.MyComputer ) );
					}
					else
					if ( sender == networkButton )
					{
						
					}
				}
			}
			
			internal class DirComboBox : ComboBox
			{
				internal class DirComboBoxItem
				{
					private int imageIndex;
					private string name;
					private string path;
					private int xPos;
					
					public DirComboBoxItem( int imageIndex, string name, string path, int xPos )
					{
						this.imageIndex = imageIndex;
						this.name = name;
						this.path = path;
						this.XPos = xPos;
					}
					
					public int ImageIndex
					{
						set
						{
							imageIndex = value;
						}
						
						get
						{
							return imageIndex;
						}
					}
					
					public string Name
					{
						set
						{
							name = value;
						}
						
						get
						{
							return name;
						}
					}
					
					public string Path
					{
						set
						{
							path = value;
						}
						
						get
						{
							return path;
						}
					}
					
					public int XPos
					{
						set
						{
							xPos = value;
						}
						
						get
						{
							return xPos;
						}
					}
				}
				
				private FileDialogPanel fileDialogPanel;
				
				private ImageList imageList = new ImageList();
				
				private string currentPath;
				
				private bool firstTime = true;
				
				public DirComboBox( FileDialogPanel fileDialogPanel )
				{
					this.fileDialogPanel = fileDialogPanel;
					
					DrawMode = DrawMode.OwnerDrawFixed;
					
					imageList.ColorDepth = ColorDepth.Depth32Bit;
					imageList.ImageSize = new Size( 16, 16 );
					imageList.Images.Add( (Image)Locale.GetResource( "last_open" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "desktop" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "folder_with_paper" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "folder" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "monitor-computer" ) );
					imageList.Images.Add( (Image)Locale.GetResource( "monitor-planet" ) );
					imageList.TransparentColor = Color.Transparent;
					
					Items.AddRange( new object[] {
									   new DirComboBoxItem( 1, "Desktop", Environment.GetFolderPath( Environment.SpecialFolder.Desktop ), 0 ),
									   new DirComboBoxItem( 2, "Home", Environment.GetFolderPath( Environment.SpecialFolder.Personal ), 0 )
								   }
								   );
				}
				
				public string CurrentPath
				{
					set
					{
						currentPath = value;
						
						ShowPath( );
					}
					get
					{
						return currentPath;
					}
				}
				
				private void ShowPath( )
				{
					DirectoryInfo di = new DirectoryInfo( currentPath );
					
					Stack dirStack = new Stack( );
					
					dirStack.Push( di );
					
					while ( di.Parent != null )
					{
						di = di.Parent;
						dirStack.Push( di );
					}
					
					BeginUpdate( );
					
					Items.Clear( );
					
					Items.AddRange( new object[] {
									   new DirComboBoxItem( 1, "Desktop", Environment.GetFolderPath( Environment.SpecialFolder.Desktop ), 0 ),
									   new DirComboBoxItem( 2, "Home", Environment.GetFolderPath( Environment.SpecialFolder.Personal ), 0 )
								   }
								   );
					
					int sel = -1;
					
					int xPos = -4;
					
					while ( dirStack.Count != 0 )
					{
						DirectoryInfo dii = (DirectoryInfo)dirStack.Pop( );
						sel = Items.Add( new DirComboBoxItem( 3, dii.Name, dii.FullName, xPos + 4 ) );
						xPos += 4;
					}
					
					if ( sel != -1 )
						SelectedIndex = sel;
					
					EndUpdate( );
				}
				
				protected override void OnDrawItem( DrawItemEventArgs e )
				{
					if ( e.Index == -1 )
						return;
					
					Bitmap bmp = new Bitmap( e.Bounds.Width, e.Bounds.Height, e.Graphics );
					Graphics gr = Graphics.FromImage( bmp );
					
					DirComboBoxItem dcbi = Items[ e.Index ] as DirComboBoxItem;
					
					Color backColor = e.BackColor;
					Color foreColor = e.ForeColor;
					
					int xPos = dcbi.XPos;
					
					// Bug in ComboBox !!!!!
					// we never receive DrawItemState.ComboBoxEdit
					if ( ( e.State & DrawItemState.ComboBoxEdit ) != 0 )
						xPos = 0;
					else
					if ( ( e.State & DrawItemState.Selected ) == DrawItemState.Selected )
					{
						backColor = Color.Blue;
						foreColor = Color.White;
					}
					
					gr.FillRectangle( new SolidBrush( backColor ), new Rectangle( 0, 0, bmp.Width, bmp.Height ) );
					
					gr.DrawString( dcbi.Name, e.Font , new SolidBrush( foreColor ), new Point( 24 + xPos, ( bmp.Height - e.Font.Height ) / 2 ) );
					gr.DrawImage( imageList.Images[ dcbi.ImageIndex ], new Rectangle( new Point( xPos + 2, 0 ), new Size( 16, 16 ) ) );
					
					e.Graphics.DrawImage( bmp, e.Bounds.X, e.Bounds.Y );
				}
				
				protected override void OnSelectedIndexChanged( EventArgs e )
				{
					// do not call ChangeDirectory when invoked from FileDialogPanel ctor...
					if ( firstTime )
					{
						firstTime = false;
						return;
					}
					
					if ( Items.Count > 0 )
					{
						DirComboBoxItem dcbi = Items[ SelectedIndex ] as DirComboBoxItem;
						
						fileDialogPanel.ChangeDirectory( this, dcbi.Path );
					}
				}
			}
		}
	}
}


