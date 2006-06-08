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
// Copyright (c) 2004-2006 Novell, Inc. (http://www.novell.com)
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
using System.Xml;
using Microsoft.Win32;

namespace System.Windows.Forms {
	#region FileDialog
	[DefaultProperty ("FileName")]
	[DefaultEvent ("FileOk")]
	public abstract class FileDialog : CommonDialog
	{
		protected static readonly object EventFileOk = new object ();
		
		internal enum FileDialogType
		{
			OpenFileDialog,
			SaveFileDialog
		}
		
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
		
		private Button cancelButton;
		private ToolBarButton upToolBarButton;
		private PopupButtonPanel popupButtonPanel;
		private Button openSaveButton;
		private Button helpButton;
		private Label fileTypeLabel;
		private ToolBarButton menueToolBarButton;
		private ContextMenu menueToolBarButtonContextMenu;
		private ToolBar smallButtonToolBar;
		private DirComboBox dirComboBox;
		private ComboBox fileNameComboBox;
		private Label fileNameLabel;
		private MWFFileView mwfFileView;
		private Label searchSaveLabel;
		private ToolBarButton newdirToolBarButton;
		private ToolBarButton backToolBarButton;
		private ComboBox fileTypeComboBox;
		private ImageList imageListTopToolbar;
		private CheckBox readonlyCheckBox;
		
		private bool multiSelect = false;
		
		private string restoreDirectoryString = "";
		
		internal FileDialogType fileDialogType;
		
		private bool do_not_call_OnSelectedIndexChangedFileTypeComboBox = false;
		
		private bool showReadOnly = false;
		private bool readOnlyChecked = false;
		internal bool createPrompt = false;
		internal bool overwritePrompt = true;
		
		internal FileFilter fileFilter;
		
		private string lastFolder = "";
		
		private MWFVFS vfs;
		
		private RegistryKey rootRegistryKey;
		private RegistryKey filedialogRegistryKey;
		private string registryKeyName = @"SOFTWARE\Mono.MWF\FileDialog";
		
		private int platform = (int) Environment.OSVersion.Platform;
		private bool running_windows = false;
		
		internal FileDialog ()
		{
			vfs = new MWFVFS ();
			
			if ((platform != 4) && (platform != 128))
				running_windows = true;
			
			Size formRegistrySize = Size.Empty;
			Point formRegistryLocation = Point.Empty;
			string[] registryFileNames = null;
			
			if (!running_windows) {
				rootRegistryKey = Microsoft.Win32.Registry.CurrentUser;
				filedialogRegistryKey = rootRegistryKey.OpenSubKey (registryKeyName);
				
				if (filedialogRegistryKey != null) {
					object formWidth = filedialogRegistryKey.GetValue ("Width");
					
					object formHeight = filedialogRegistryKey.GetValue ("Height");
					
					if (formHeight != null && formWidth != null)
						formRegistrySize = new Size ((int)formWidth, (int)formHeight);
					
					object formLocationX = filedialogRegistryKey.GetValue ("X");
					object formLocationY = filedialogRegistryKey.GetValue ("Y");
					
					if (formLocationX != null && formLocationY != null)
						formRegistryLocation = new Point ((int)formLocationX, (int)formLocationY);
					
					registryFileNames = (string[])filedialogRegistryKey.GetValue ("FileNames");
				}
			}
			
			fileTypeComboBox = new ComboBox ();
			backToolBarButton = new ToolBarButton ();
			newdirToolBarButton = new ToolBarButton ();
			searchSaveLabel = new Label ();
			mwfFileView = new MWFFileView (vfs);
			fileNameLabel = new Label ();
			fileNameComboBox = new ComboBox ();
			dirComboBox = new DirComboBox (vfs);
			smallButtonToolBar = new ToolBar ();
			menueToolBarButton = new ToolBarButton ();
			fileTypeLabel = new Label ();
			openSaveButton = new Button ();
			form.AcceptButton = openSaveButton;
			helpButton = new Button ();
			popupButtonPanel = new PopupButtonPanel ();
			upToolBarButton = new ToolBarButton ();
			cancelButton = new Button ();
			form.CancelButton = cancelButton;
			imageListTopToolbar = new ImageList ();
			menueToolBarButtonContextMenu = new ContextMenu ();
			readonlyCheckBox = new CheckBox ();
			
			form.SuspendLayout ();
			
			//imageListTopToolbar
			imageListTopToolbar.ColorDepth = ColorDepth.Depth32Bit;
			imageListTopToolbar.ImageSize = new Size (16, 16); // 16, 16
			imageListTopToolbar.Images.Add (ResourceImageLoader.Get ("go-previous.png"));
			imageListTopToolbar.Images.Add (ResourceImageLoader.Get ("go-top.png"));
			imageListTopToolbar.Images.Add (ResourceImageLoader.Get ("folder-new.png"));
			imageListTopToolbar.Images.Add (ResourceImageLoader.Get ("preferences-system-windows.png"));
			imageListTopToolbar.TransparentColor = Color.Transparent;
			
			// searchLabel
			searchSaveLabel.FlatStyle = FlatStyle.System;
			searchSaveLabel.Location = new Point (7, 8);
			searchSaveLabel.Size = new Size (72, 21);
			searchSaveLabel.TextAlign = ContentAlignment.MiddleRight;
			
			// dirComboBox
			dirComboBox.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
			dirComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			dirComboBox.Location = new Point (99, 8);
			dirComboBox.Size = new Size (260, 21);
			dirComboBox.TabIndex = 0;
			
			// smallButtonToolBar
			smallButtonToolBar.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
			smallButtonToolBar.Appearance = ToolBarAppearance.Flat;
			smallButtonToolBar.AutoSize = false;
			smallButtonToolBar.Buttons.AddRange (new ToolBarButton [] {
								     backToolBarButton,
								     upToolBarButton,
								     newdirToolBarButton,
								     menueToolBarButton});
			smallButtonToolBar.ButtonSize = new Size (24, 24); // 21, 16
			smallButtonToolBar.Divider = false;
			smallButtonToolBar.Dock = DockStyle.None;
			smallButtonToolBar.DropDownArrows = true;
			smallButtonToolBar.ImageList = imageListTopToolbar;
			smallButtonToolBar.Location = new Point (372, 6);
			smallButtonToolBar.ShowToolTips = true;
			smallButtonToolBar.Size = new Size (140, 28);
			smallButtonToolBar.TabIndex = 1;
			smallButtonToolBar.TextAlign = ToolBarTextAlign.Right;
			
			// buttonPanel
			popupButtonPanel.Dock = DockStyle.None;
			popupButtonPanel.Location = new Point (7, 37);
			popupButtonPanel.TabIndex = 2;
			
			// mwfFileView
			mwfFileView.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
			mwfFileView.Location = new Point (99, 37);
			mwfFileView.Size = new Size (449, 282);
			mwfFileView.Columns.Add (" Name", 170, HorizontalAlignment.Left);
			mwfFileView.Columns.Add ("Size ", 80, HorizontalAlignment.Right);
			mwfFileView.Columns.Add (" Type", 100, HorizontalAlignment.Left);
			mwfFileView.Columns.Add (" Last Access", 150, HorizontalAlignment.Left);
			mwfFileView.AllowColumnReorder = true;
			mwfFileView.MultiSelect = false;
			mwfFileView.TabIndex = 3;
			mwfFileView.RegisterSender (dirComboBox);
			mwfFileView.RegisterSender (popupButtonPanel);
			
			// fileNameLabel
			fileNameLabel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
			fileNameLabel.FlatStyle = FlatStyle.System;
			fileNameLabel.Location = new Point (102, 330);
			fileNameLabel.Size = new Size (70, 21);
			fileNameLabel.Text = "Filename:";
			fileNameLabel.TextAlign = ContentAlignment.MiddleLeft;
			
			// fileNameComboBox
			fileNameComboBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
			fileNameComboBox.Location = new Point (195, 330);
			fileNameComboBox.Size = new Size (245, 21);
			fileNameComboBox.TabIndex = 4;
			fileNameComboBox.MaxDropDownItems = 10;
			fileNameComboBox.Items.Add (" ");
			
			if (registryFileNames != null) {
				fileNameComboBox.Items.Clear ();
				
				foreach (string registryFileName in registryFileNames) {
					if (registryFileName != null)
						if (registryFileName.Trim ().Length > 0)
							fileNameComboBox.Items.Add (registryFileName);
				}
			}
			
			
			// fileTypeLabel
			fileTypeLabel.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
			fileTypeLabel.FlatStyle = FlatStyle.System;
			fileTypeLabel.Location = new Point (102, 356);
			fileTypeLabel.Size = new Size (70, 21);
			fileTypeLabel.Text = "Filetype:";
			fileTypeLabel.TextAlign = ContentAlignment.MiddleLeft;
			
			// fileTypeComboBox
			fileTypeComboBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
			fileTypeComboBox.Location = new Point (195, 356);
			fileTypeComboBox.Size = new Size (245, 21);
			fileTypeComboBox.TabIndex = 5;
			
			// backToolBarButton
			backToolBarButton.ImageIndex = 0;
			backToolBarButton.Enabled = false;
			backToolBarButton.Style = ToolBarButtonStyle.PushButton;
			mwfFileView.AddControlToEnableDisableByDirStack (backToolBarButton);
			
			// upToolBarButton
			upToolBarButton.ImageIndex = 1;
			upToolBarButton.Style = ToolBarButtonStyle.PushButton;
			mwfFileView.SetFolderUpToolBarButton (upToolBarButton);
			
			// newdirToolBarButton
			newdirToolBarButton.ImageIndex = 2;
			newdirToolBarButton.Style = ToolBarButtonStyle.PushButton;
			
			// menueToolBarButton
			menueToolBarButton.ImageIndex = 3;
			menueToolBarButton.DropDownMenu = menueToolBarButtonContextMenu;
			menueToolBarButton.Style = ToolBarButtonStyle.DropDownButton;
			
			// menueToolBarButtonContextMenu
			menueToolBarButtonContextMenu.MenuItems.AddRange (mwfFileView.ViewMenuItems);
			
			// openSaveButton
			openSaveButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			openSaveButton.FlatStyle = FlatStyle.System;
			openSaveButton.Location = new Point (475, 330);
			openSaveButton.Size = new Size (72, 21);
			openSaveButton.TabIndex = 7;
			openSaveButton.FlatStyle = FlatStyle.System;
			
			// cancelButton
			cancelButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			cancelButton.FlatStyle = FlatStyle.System;
			cancelButton.Location = new Point (475, 356);
			cancelButton.Size = new Size (72, 21);
			cancelButton.TabIndex = 8;
			cancelButton.Text = "Cancel";
			cancelButton.FlatStyle = FlatStyle.System;
			
			// helpButton
			helpButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			helpButton.FlatStyle = FlatStyle.System;
			helpButton.Location = new Point (475, 350);
			helpButton.Size = new Size (72, 21);
			helpButton.TabIndex = 9;
			helpButton.Text = "Help";
			helpButton.FlatStyle = FlatStyle.System;
			
			// checkBox
			readonlyCheckBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
			readonlyCheckBox.Text = "Open Readonly";
			readonlyCheckBox.Location = new Point (195, 350);
			readonlyCheckBox.Size = new Size (245, 21);
			readonlyCheckBox.TabIndex = 6;
			readonlyCheckBox.FlatStyle = FlatStyle.System;
			
			form.SizeGripStyle = SizeGripStyle.Show;
			
			form.MaximizeBox = true;
			form.FormBorderStyle = FormBorderStyle.Sizable;
			form.MinimumSize = new Size (554, 405);
			
			form.Size =  new Size (554, 405); // 384
			
			form.Controls.Add (smallButtonToolBar);
			form.Controls.Add (cancelButton);
			form.Controls.Add (openSaveButton);
			form.Controls.Add (mwfFileView);
			form.Controls.Add (fileTypeLabel);
			form.Controls.Add (fileNameLabel);
			form.Controls.Add (fileTypeComboBox);
			form.Controls.Add (fileNameComboBox);
			form.Controls.Add (dirComboBox);
			form.Controls.Add (searchSaveLabel);
			form.Controls.Add (popupButtonPanel);
			
			form.ResumeLayout (false);
			
			if (formRegistrySize != Size.Empty) {
				form.Size = formRegistrySize;
			}
			
			if (formRegistryLocation != Point.Empty) {
				form.Location = formRegistryLocation;
			}
			
			openSaveButton.Click += new EventHandler (OnClickOpenSaveButton);
			cancelButton.Click += new EventHandler (OnClickCancelButton);
			helpButton.Click += new EventHandler (OnClickHelpButton);
			
			smallButtonToolBar.ButtonClick += new ToolBarButtonClickEventHandler (OnClickSmallButtonToolBar);
			
			fileTypeComboBox.SelectedIndexChanged += new EventHandler (OnSelectedIndexChangedFileTypeComboBox);
			
			mwfFileView.SelectedFileChanged += new EventHandler (OnSelectedFileChangedFileView);
			mwfFileView.ForceDialogEnd += new EventHandler (OnForceDialogEndFileView);
			mwfFileView.SelectedFilesChanged += new EventHandler (OnSelectedFilesChangedFileView);
			
			dirComboBox.DirectoryChanged += new EventHandler (OnDirectoryChangedDirComboBox);
			popupButtonPanel.DirectoryChanged += new EventHandler (OnDirectoryChangedPopupButtonPanel);
			
			readonlyCheckBox.CheckedChanged += new EventHandler (OnCheckCheckChanged);
		}
		
		[DefaultValue(true)]
		public bool AddExtension {
			get {
				return addExtension;
			}
			
			set {
				addExtension = value;
			}
		}
		
		[DefaultValue(false)]
		public virtual bool CheckFileExists {
			get {
				return checkFileExists;
			}
			
			set {
				checkFileExists = value;
			}
		}
		
		[DefaultValue(true)]
		public bool CheckPathExists {
			get {
				return checkPathExists;
			}
			
			set {
				checkPathExists = value;
			}
		}
		
		[DefaultValue("")]
		public string DefaultExt {
			get {
				return defaultExt;
			}
			
			set {
				defaultExt = value;
				
				// if there is a dot remove it and everything before it
				if (defaultExt.LastIndexOf ('.') != - 1) {
					string[] split = defaultExt.Split (new char [] { '.' });
					defaultExt = split [split.Length - 1];
				}
			}
		}
		
		// in MS.NET it doesn't make a difference if
		// DerefenceLinks is true or false
		// if the selected file is a link FileDialog
		// always returns the link
		[DefaultValue(true)]
		public bool DereferenceLinks {
			get {
				return dereferenceLinks;
			}
			
			set {
				dereferenceLinks = value;
			}
		}
		
		[DefaultValue("")]
		public string FileName {
			get {
				return fileName;
			}
			
			set {
				if (value != null) {
					if (SetFileName (value))
						fileName = value;
				}
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string[] FileNames {
			get {
				if (fileNames == null || fileNames.Length == 0) {
					string[] null_nada_nothing_filenames = new string [0];
					return null_nada_nothing_filenames;
				}
				
				string[] new_filenames = new string [fileNames.Length];
				fileNames.CopyTo (new_filenames, 0);
				return new_filenames;
			}
		}
		
		[DefaultValue("")]
		[Localizable(true)]
		public string Filter {
			get {
				return filter;
			}
			
			set {
				if (value == null)
					throw new NullReferenceException ("Filter");
				
				filter = value;
				
				fileFilter = new FileFilter (filter);
				
				UpdateFilters ();
			}
		}
		
		[DefaultValue(1)]
		public int FilterIndex {
			get {
				return filterIndex;
			}
			
			set {
				if (fileFilter != null && fileFilter.FilterArrayList.Count > value)
					return;  // FIXME: throw an exception ?
				
				filterIndex = value;
				
				SelectFilter ();
			}
		}
		
		[DefaultValue("")]
		public string InitialDirectory {
			get {
				return initialDirectory;
			}
			
			set {
				if (Directory.Exists (value)) {
					initialDirectory = value;
					
					mwfFileView.ChangeDirectory (null, initialDirectory);
				}
			}
		}
		
		[DefaultValue(false)]
		public bool RestoreDirectory {
			get {
				return restoreDirectory;
			}
			
			set {
				restoreDirectory = value;
			}
		}
		
		[DefaultValue(false)]
		public bool ShowHelp {
			get {
				return showHelp;
			}
			
			set {
				showHelp = value;
				ResizeAndRelocateForHelpOrReadOnly ();
			}
		}
		
		[DefaultValue("")]
		[Localizable(true)]
		public string Title {
			get {
				return title;
			}
			
			set {
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
		[DefaultValue(true)]
		public bool ValidateNames {
			get {
				return validateNames;
			}
			
			set {
				validateNames = value;
			}
		}
		
		public override void Reset ()
		{
			addExtension = true;
			checkFileExists = false;
			checkPathExists = true;
			defaultExt = "";
			dereferenceLinks = true;
			fileName = "";
			fileNames = null;
			Filter = "";
			filterIndex = 1;
			initialDirectory = "";
			restoreDirectory = false;
			ShowHelp = false;
			Title = "";
			validateNames = true;
			
			UpdateFilters ();
		}
		
		public override string ToString ()
		{
			return base.ToString ();
		}
		
		public event CancelEventHandler FileOk {
			add { Events.AddHandler (EventFileOk, value); }
			remove { Events.RemoveHandler (EventFileOk, value); }
		}
		
		protected virtual IntPtr Instance {
			get {
				if (form == null)
					return IntPtr.Zero;
				return form.Handle;
			}
		}
		
		// This is just for internal use with MSs version, so it doesn't need to be implemented
		// as it can't really be accessed anyways
		protected int Options {
			get { return -1; }
		}
		
		[MonoTODO]
		protected  override IntPtr HookProc (IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			throw new NotImplementedException ();
		}
		
		protected void OnFileOk (CancelEventArgs e)
		{
			if (!running_windows)
				WriteRegistryValues (e);
			
			CancelEventHandler fo = (CancelEventHandler) Events [EventFileOk];
			if (fo != null)
				fo (this, e);
		}
		
		protected  override bool RunDialog (IntPtr hWndOwner)
		{
			ReadRegistryValues ();
			
			form.Refresh ();
			
			mwfFileView.ChangeDirectory (null, lastFolder);
			
			fileNameComboBox.Select ();
			
			return true;
		}
		
		internal virtual bool ShowReadOnly {
			set {
				showReadOnly = value;
				ResizeAndRelocateForHelpOrReadOnly ();
			}
			
			get {
				return showReadOnly;
			}
		}
		
		internal virtual bool ReadOnlyChecked {
			set {
				readOnlyChecked = value;
				readonlyCheckBox.Checked = value;
			}
			
			get {
				return readOnlyChecked;
			}
		}
		
		internal bool BMultiSelect {
			set {
				multiSelect = value;
				mwfFileView.MultiSelect = value;
			}
			
			get {
				return multiSelect;
			}
		}
		
		internal string OpenSaveButtonText {
			set {
				openSaveButton.Text = value;
			}
		}
		
		internal string SearchSaveLabel {
			set {
				searchSaveLabel.Text = value;
			}
		}
		
		private void SelectFilter ()
		{
			if (FilterIndex > mwfFileView.FilterArrayList.Count)
				return;
			
			do_not_call_OnSelectedIndexChangedFileTypeComboBox = true;
			fileTypeComboBox.BeginUpdate ();
			fileTypeComboBox.SelectedIndex = FilterIndex - 1;
			fileTypeComboBox.EndUpdate ();
			
			mwfFileView.FilterIndex = FilterIndex;
		}
		
		private bool SetFileName (string fname)
		{
			bool rooted = Path.IsPathRooted (fname);
			
			if (!rooted) {
				string dir = mwfFileView.CurrentRealFolder ;
				if (dir == null) {
					dir = Environment.CurrentDirectory;
				}
				if (File.Exists (Path.Combine (dir, fname))) {
					fileNameComboBox.Text = fname;
					mwfFileView.SetSelectedIndexTo (fname);
					
					return true;
				}
			} else {
				if (File.Exists (fname)) {
					fileNameComboBox.Text = Path.GetFileName (fname);
					mwfFileView.ChangeDirectory (null, Path.GetDirectoryName (fname));
					mwfFileView.SetSelectedIndexTo (fname);
					
					return true;
				}
			}
			
			return false;
		}
		
		void OnClickOpenSaveButton (object sender, EventArgs e)
		{
			if (fileDialogType == FileDialogType.OpenFileDialog) {
				ListView.SelectedListViewItemCollection sl = mwfFileView.SelectedItems;
				if (sl.Count > 0 && sl [0] != null) {
					if (sl.Count == 1) {
						FileViewListViewItem item = sl [0] as FileViewListViewItem;
						FSEntry fsEntry = item.FSEntry;
						
						if (fsEntry.Attributes == FileAttributes.Directory) {
							mwfFileView.ChangeDirectory (null, fsEntry.FullName);
							return;
						}
					} else {
						foreach (FileViewListViewItem item in sl) {
							FSEntry fsEntry = item.FSEntry;
							if (fsEntry.Attributes == FileAttributes.Directory) {
								mwfFileView.ChangeDirectory (null, fsEntry.FullName);
								return;
							}
						}
					}
				}
			}
			
			string internalfullfilename = "";
			
			if (!multiSelect) {
				string fileFromComboBox = fileNameComboBox.Text.Trim ();
				
				if (fileFromComboBox.Length > 0) {
					if (!Path.IsPathRooted (fileFromComboBox)) {
						// on unix currentRealFolder for "Recently used files" is null,
						// because recently used files don't get saved as links in a directory
						// recently used files get saved in a xml file
						if (mwfFileView.CurrentRealFolder != null)
							fileFromComboBox = Path.Combine (mwfFileView.CurrentRealFolder, fileFromComboBox);
						else
						if (mwfFileView.CurrentFSEntry != null) {
							fileFromComboBox = mwfFileView.CurrentFSEntry.FullName;
						}
					}
					
					FileInfo fileInfo = new FileInfo (fileFromComboBox);
					
					if (fileInfo.Exists || fileDialogType == FileDialogType.SaveFileDialog) {
						internalfullfilename = fileFromComboBox;
					} else {
						DirectoryInfo dirInfo = new DirectoryInfo (fileFromComboBox);
						if (dirInfo.Exists) {
							mwfFileView.ChangeDirectory (null, dirInfo.FullName);
							
							fileNameComboBox.Text = " ";
							return;
						} else {
							internalfullfilename = fileFromComboBox;
						}							
					}
				} else
					return;
				
				if (fileDialogType == FileDialogType.OpenFileDialog) {
					if (checkFileExists) {
						if (!File.Exists (internalfullfilename)) {
							string message = "\"" + internalfullfilename + "\" doesn't exist. Please verify that you have entered the correct file name.";
							MessageBox.Show (message, openSaveButton.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
							
							return;
						}
					}
				} else {
					if (overwritePrompt) {
						if (File.Exists (internalfullfilename)) {
							string message = "\"" + internalfullfilename + "\" exists. Overwrite ?";
							DialogResult dr = MessageBox.Show (message, openSaveButton.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
							
							if (dr == DialogResult.Cancel)
								return;
						}
					}
					
					if (createPrompt) {
						if (!File.Exists (internalfullfilename)) {
							string message = "\"" + internalfullfilename + "\" doesn't exist. Create ?";
							DialogResult dr = MessageBox.Show (message, openSaveButton.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
							
							if (dr == DialogResult.Cancel)
								return;
						}
					}
				}
				
				if (fileDialogType == FileDialogType.SaveFileDialog) {
					if (addExtension) {
						string extension_to_use = "";
						string filter_exentsion = "";
						
						if (fileFilter != null) {
							FilterStruct filterstruct = (FilterStruct)fileFilter.FilterArrayList [filterIndex - 1];
							
							for (int i = 0; i < filterstruct.filters.Count; i++) {
								string extension = filterstruct.filters [i];
								
								if (extension.StartsWith ("*"))
									extension = extension.Remove (0, 1);
								
								if (extension.IndexOf ('*') != -1)
									continue;
								
								filter_exentsion = extension;
								break;
							}
						}
						
						if (filter_exentsion != "")
							extension_to_use = filter_exentsion;
						else
						if (defaultExt != "")
							extension_to_use = "." + defaultExt;
						
						internalfullfilename += extension_to_use;
					}
				}
				
				fileNames = new string [1];
				
				fileNames [0] = internalfullfilename;
				
				fileName = internalfullfilename;
				
				mwfFileView.WriteRecentlyUsed (internalfullfilename);
			} else // multiSelect = true
			if (fileDialogType != FileDialogType.SaveFileDialog) {
				if (mwfFileView.SelectedItems.Count > 0) {
					// first remove all selected directories
					ArrayList al = new ArrayList ();
					
					foreach (FileViewListViewItem lvi in mwfFileView.SelectedItems) {
						FSEntry fsEntry = lvi.FSEntry;
						
						if (fsEntry.Attributes != FileAttributes.Directory) {
							al.Add (fsEntry);
						}
					}
					
					fileName = ((FSEntry)al [0]).FullName;
					
					fileNames = new string [al.Count];
					
					for (int i = 0; i < al.Count; i++) {
						fileNames [i] = ((FSEntry)al [i]).FullName;
					}
				}
			}
			
			if (checkPathExists && mwfFileView.CurrentRealFolder != null) {
				if (!Directory.Exists (mwfFileView.CurrentRealFolder)) {
					string message = "\"" + mwfFileView.CurrentRealFolder + "\" doesn't exist. Please verify that you have entered the correct directory name.";
					MessageBox.Show (message, openSaveButton.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					
					if (initialDirectory == String.Empty)
						mwfFileView.ChangeDirectory (null, lastFolder);
					else
						mwfFileView.ChangeDirectory (null, initialDirectory);
					
					return;
				}
			}
			
			if (restoreDirectory) {
				lastFolder  = restoreDirectoryString;
			} else {
				lastFolder = mwfFileView.CurrentFolder;
			}
			
			if (fileNameComboBox.Items.Count > 0) {
				if (fileNameComboBox.Items.IndexOf (fileName) == -1) {
					fileNameComboBox.Items.Insert (0, fileName);
				}
			} else
				fileNameComboBox.Items.Add (fileName);
			
			if (fileNameComboBox.Items.Count == 11)
				fileNameComboBox.Items.RemoveAt (10);
			
			CancelEventArgs cancelEventArgs = new CancelEventArgs ();
			
			cancelEventArgs.Cancel = false;
			
			OnFileOk (cancelEventArgs);
			
			form.DialogResult = DialogResult.OK;
		}
		
		void OnClickCancelButton (object sender, EventArgs e)
		{
			if (restoreDirectory)
				mwfFileView.CurrentFolder = restoreDirectoryString;
			
			CancelEventArgs cancelEventArgs = new CancelEventArgs ();
			
			cancelEventArgs.Cancel = true;
			
			OnFileOk (cancelEventArgs);
			
			form.DialogResult = DialogResult.Cancel;
		}
		
		void OnClickHelpButton (object sender, EventArgs e)
		{
			OnHelpRequest (e);
		}
		
		void OnClickSmallButtonToolBar (object sender, ToolBarButtonClickEventArgs e)
		{
			if (e.Button == upToolBarButton) {
				mwfFileView.OneDirUp ();
			} else
			if (e.Button == backToolBarButton) {
				mwfFileView.PopDir ();
			} else
			if (e.Button == newdirToolBarButton) {
				mwfFileView.CreateNewFolder ();
			}
		}
		
		void OnSelectedIndexChangedFileTypeComboBox (object sender, EventArgs e)
		{
			if (do_not_call_OnSelectedIndexChangedFileTypeComboBox) {
				do_not_call_OnSelectedIndexChangedFileTypeComboBox = false;
				return;
			}
			
			filterIndex = fileTypeComboBox.SelectedIndex + 1;
			
			mwfFileView.FilterIndex = filterIndex;
		}
		
		void OnSelectedFileChangedFileView (object sender, EventArgs e)
		{
			fileNameComboBox.Text = mwfFileView.CurrentFSEntry.Name;
		}
		
		void OnSelectedFilesChangedFileView (object sender, EventArgs e)
		{
			fileNameComboBox.Text = mwfFileView.SelectedFilesString;
		}
		
		void OnForceDialogEndFileView (object sender, EventArgs e)
		{
			OnClickOpenSaveButton (this, EventArgs.Empty);
		}
		
		void OnDirectoryChangedDirComboBox (object sender, EventArgs e)
		{
			mwfFileView.ChangeDirectory (sender, dirComboBox.CurrentFolder);
		}
		
		void OnDirectoryChangedPopupButtonPanel (object sender, EventArgs e)
		{
			mwfFileView.ChangeDirectory (sender, popupButtonPanel.CurrentFolder);
		}
		
		void OnCheckCheckChanged (object sender, EventArgs e)
		{
			ReadOnlyChecked = readonlyCheckBox.Checked;
		}
		
		private void UpdateFilters ()
		{
			ArrayList filters = fileFilter.FilterArrayList;
			
			fileTypeComboBox.BeginUpdate ();
			
			fileTypeComboBox.Items.Clear ();
			
			foreach (FilterStruct fs in filters) {
				fileTypeComboBox.Items.Add (fs.filterName);
			}
			
			fileTypeComboBox.SelectedIndex = FilterIndex - 1;
			
			fileTypeComboBox.EndUpdate ();
			
			mwfFileView.FilterArrayList = filters;
			
			mwfFileView.FilterIndex = FilterIndex;
		}
		
		private void ResizeAndRelocateForHelpOrReadOnly ()
		{
			form.SuspendLayout ();
			if (ShowHelp || ShowReadOnly) {
				mwfFileView.Size = new Size (449, 250); 
				fileNameLabel.Location = new Point (102, 298);
				fileNameComboBox.Location = new Point (195, 298);
				fileTypeLabel.Location = new Point (102, 324);
				fileTypeComboBox.Location = new Point (195, 324);
				openSaveButton.Location = new Point (475, 298);
				cancelButton.Location = new Point (475, 324);
			} else {
				mwfFileView.Size = new Size (449, 282);
				fileNameLabel.Location = new Point (102, 330);
				fileNameComboBox.Location = new Point (195, 330);
				fileTypeLabel.Location = new Point (102, 356);
				fileTypeComboBox.Location = new Point (195, 356);
				openSaveButton.Location = new Point (475, 330);
				cancelButton.Location = new Point (475, 356);
			}
			
			if (ShowHelp)
				form.Controls.Add (helpButton);
			else
				form.Controls.Remove (helpButton);
			
			if (ShowReadOnly)
				form.Controls.Add (readonlyCheckBox);
			else
				form.Controls.Remove (readonlyCheckBox);
			form.ResumeLayout ();
		}
		
		private void WriteRegistryValues (CancelEventArgs ce)
		{
			try {
				filedialogRegistryKey = rootRegistryKey.OpenSubKey (registryKeyName);
				
				if (filedialogRegistryKey == null)
					filedialogRegistryKey = rootRegistryKey.CreateSubKey (registryKeyName);
				
				filedialogRegistryKey.SetValue ("Width", form.Width);
				filedialogRegistryKey.SetValue ("Height", form.Height);
				filedialogRegistryKey.SetValue ("X", form.Location.X);
				filedialogRegistryKey.SetValue ("Y", form.Location.Y);
				
				if (!ce.Cancel) {
					filedialogRegistryKey.SetValue ("LastFolder", lastFolder);
					
					string[] fileNameCBItems = new string [fileNameComboBox.Items.Count];
					
					fileNameComboBox.Items.CopyTo (fileNameCBItems, 0);
					
					filedialogRegistryKey.SetValue ("FileNames", fileNameCBItems);
				}
			} catch (Exception) {}
		}
		
		private void ReadRegistryValues ()
		{
			rootRegistryKey = Microsoft.Win32.Registry.CurrentUser;
			filedialogRegistryKey = rootRegistryKey.OpenSubKey (registryKeyName);
			
			if (!running_windows)
				if (filedialogRegistryKey != null) {
					lastFolder = (string)filedialogRegistryKey.GetValue ("LastFolder");
				}
			
			if (initialDirectory != "")
				lastFolder = initialDirectory;
			else
			if (lastFolder == null || lastFolder == "")
				lastFolder = Environment.CurrentDirectory;
			
			if (RestoreDirectory)
				restoreDirectoryString = lastFolder;
		}
	}
	#endregion
	
	#region PopupButtonPanel
	internal class PopupButtonPanel : Control, IUpdateFolder
	{
		#region PopupButton
		internal class PopupButton : Control
		{
			internal enum PopupButtonState
			{ Normal, Down, Up}
			
			private Image image = null;
			private PopupButtonState popupButtonState = PopupButtonState.Normal;
			private StringFormat text_format = new StringFormat();
			private Rectangle text_rect = Rectangle.Empty;
			
			public PopupButton ()
			{
				text_format.Alignment = StringAlignment.Center;
				text_format.LineAlignment = StringAlignment.Near;
				
				SetStyle (ControlStyles.DoubleBuffer, true);
				SetStyle (ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.UserPaint, true);
				SetStyle (ControlStyles.Selectable, false);
			}
			
			public Image Image {
				set {
					image = value;
					Refresh ();
				}
				
				get {
					return image;
				}
			}
			
			public PopupButtonState ButtonState {
				set {
					popupButtonState = value;
					Refresh ();
				}
				
				get {
					return popupButtonState;
				}
			}
			
			protected override void OnPaint (PaintEventArgs pe)
			{
				Draw (pe);
				
				base.OnPaint (pe);
			}
			
			private void Draw (PaintEventArgs pe)
			{
				Graphics gr = pe.Graphics;
				
				gr.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle);
				
				// draw image
				if (image != null) {
					int i_x = (ClientSize.Width - image.Width) / 2;
					int i_y = 4;
					gr.DrawImage (image, i_x, i_y);
				}
				
				if (Text != String.Empty) {
					if (text_rect == Rectangle.Empty)
						text_rect = new Rectangle (0, Height - 30, Width, Height - 30); 
					
					gr.DrawString (Text, Font, ThemeEngine.Current.ResPool.GetSolidBrush (ForeColor), text_rect, text_format);
				}
				
				switch (popupButtonState) {
					case PopupButtonState.Up:
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.White), 0, 0, ClientSize.Width - 1, 0);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.White), 0, 0, 0, ClientSize.Height - 1);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.Black), ClientSize.Width - 1, 0, ClientSize.Width - 1, ClientSize.Height - 1);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.Black), 0, ClientSize.Height - 1, ClientSize.Width - 1, ClientSize.Height - 1);
						break;
						
					case PopupButtonState.Down:
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.Black), 0, 0, ClientSize.Width - 1, 0);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.Black), 0, 0, 0, ClientSize.Height - 1);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.White), ClientSize.Width - 1, 0, ClientSize.Width - 1, ClientSize.Height - 1);
						gr.DrawLine (ThemeEngine.Current.ResPool.GetPen (Color.White), 0, ClientSize.Height - 1, ClientSize.Width - 1, ClientSize.Height - 1);
						break;
				}
			}
			
			protected override void OnMouseEnter (EventArgs e)
			{
				if (popupButtonState != PopupButtonState.Down)
					popupButtonState = PopupButtonState.Up;
				
				PopupButtonPanel panel = Parent as PopupButtonPanel;
				
				if (panel.focusButton != null && panel.focusButton.ButtonState == PopupButtonState.Up) {
					panel.focusButton.ButtonState = PopupButtonState.Normal;
					panel.focusButton = null;
				}
				Refresh ();
				base.OnMouseEnter (e);
			}
			
			protected override void OnMouseLeave (EventArgs e)
			{
				if (popupButtonState == PopupButtonState.Up)
					popupButtonState = PopupButtonState.Normal;
				Refresh ();
				base.OnMouseLeave (e);
			}
			
			protected override void OnClick (EventArgs e)
			{
				popupButtonState = PopupButtonState.Down;
				Refresh ();
				base.OnClick (e);
			}
		}
		#endregion
		
		private EventHandler on_directory_changed;
		
		private PopupButton recentlyusedButton;
		private PopupButton desktopButton;
		private PopupButton personalButton;
		private PopupButton mycomputerButton;
		private PopupButton networkButton;
		
		private PopupButton lastPopupButton = null;
		private PopupButton focusButton = null;
		
		private string currentPath;
		
		private int currentFocusIndex;
		
		public PopupButtonPanel ()
		{
			SuspendLayout ();
			
			//BackColor = Color.FromArgb (128, 128, 128);
			Size = new Size (85, 336);
			
			recentlyusedButton = new PopupButton ();
			desktopButton = new PopupButton ();
			personalButton = new PopupButton ();
			mycomputerButton = new PopupButton ();
			networkButton = new PopupButton ();
			
			recentlyusedButton.Size = new Size (81, 64);
			recentlyusedButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesRecentDocuments, 32);
			recentlyusedButton.BackColor = BackColor;
			recentlyusedButton.ForeColor = Color.Black;
			recentlyusedButton.Location = new Point (2, 2);
			recentlyusedButton.Text = "Recently\nused";
			recentlyusedButton.Click += new EventHandler (OnClickButton);
			
			desktopButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesDesktop, 32);
			desktopButton.BackColor = BackColor;
			desktopButton.ForeColor = Color.Black;
			desktopButton.Size = new Size (81, 64);
			desktopButton.Location = new Point (2, 66);
			desktopButton.Text = "Desktop";
			desktopButton.Click += new EventHandler (OnClickButton);
			
			personalButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesPersonal, 32);
			personalButton.BackColor = BackColor;
			personalButton.ForeColor = Color.Black;
			personalButton.Size = new Size (81, 64);
			personalButton.Location = new Point (2, 130);
			personalButton.Text = "Personal";
			personalButton.Click += new EventHandler (OnClickButton);
			
			mycomputerButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesMyComputer, 32);
			mycomputerButton.BackColor = BackColor;
			mycomputerButton.ForeColor = Color.Black;
			mycomputerButton.Size = new Size (81, 64);
			mycomputerButton.Location = new Point (2, 194);
			mycomputerButton.Text = "My Computer";
			mycomputerButton.Click += new EventHandler (OnClickButton);
			
			networkButton.Image = ThemeEngine.Current.Images (UIIcon.PlacesMyNetwork, 32);
			networkButton.BackColor = BackColor;
			networkButton.ForeColor = Color.Black;
			networkButton.Size = new Size (81, 64);
			networkButton.Location = new Point (2, 258);
			networkButton.Text = "My Network";
			networkButton.Click += new EventHandler (OnClickButton);
			
			Controls.Add (recentlyusedButton);
			Controls.Add (desktopButton);
			Controls.Add (personalButton);
			Controls.Add (mycomputerButton);
			Controls.Add (networkButton);
			
			ResumeLayout (false);
			
			KeyDown += new KeyEventHandler (Key_Down);
			
			SetStyle (ControlStyles.StandardClick, false);
		}
		
		void OnClickButton (object sender, EventArgs e)
		{
			if (lastPopupButton != null && lastPopupButton != sender as PopupButton)
				lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
			lastPopupButton = sender as PopupButton;
			
			if (sender == recentlyusedButton) {
				currentPath = MWFVFS.RecentlyUsedPrefix;
			} else
			if (sender == desktopButton) {
				currentPath = MWFVFS.DesktopPrefix;
			} else
			if (sender == personalButton) {
				currentPath = MWFVFS.PersonalPrefix;
			} else
			if (sender == mycomputerButton) {
				currentPath = MWFVFS.MyComputerPrefix;
			} else
			if (sender == networkButton) {
				currentPath = MWFVFS.MyNetworkPrefix;
			}
			
			if (on_directory_changed != null)
				on_directory_changed (this, EventArgs.Empty);
		}
		
		public string CurrentFolder {
			set {
				string currentPath = value;
				if (currentPath == MWFVFS.RecentlyUsedPrefix) {
					if (lastPopupButton != recentlyusedButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						recentlyusedButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = recentlyusedButton;
					}
				} else
				if (currentPath == MWFVFS.DesktopPrefix) {
					if (lastPopupButton != desktopButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						desktopButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = desktopButton;
					}
				} else
				if (currentPath == MWFVFS.PersonalPrefix) {
					if (lastPopupButton != personalButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						personalButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = personalButton;
					}
				} else
				if (currentPath == MWFVFS.MyComputerPrefix) {
					if (lastPopupButton != mycomputerButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						mycomputerButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = mycomputerButton;
					}
				} else
				if (currentPath == MWFVFS.MyNetworkPrefix) {
					if (lastPopupButton != networkButton) {
						if (lastPopupButton != null)
							lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						networkButton.ButtonState = PopupButton.PopupButtonState.Down;
						lastPopupButton = networkButton;
					}
				} else {
					if (lastPopupButton != null) {
						lastPopupButton.ButtonState = PopupButton.PopupButtonState.Normal;
						lastPopupButton = null;
					}
				}
			}
			get {
				return currentPath;
			}
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			ControlPaint.DrawBorder3D (e.Graphics, ClientRectangle, Border3DStyle.Sunken);
			base.OnPaint (e);
		}
		
		protected override void OnGotFocus (EventArgs e)
		{
			if (lastPopupButton != recentlyusedButton) {
				recentlyusedButton.ButtonState = PopupButton.PopupButtonState.Up;
				focusButton = recentlyusedButton;
			}
			currentFocusIndex = 0;
			
			base.OnGotFocus (e);
		}
		
		protected override void OnLostFocus (EventArgs e)
		{
			if (focusButton != null && focusButton.ButtonState != PopupButton.PopupButtonState.Down)
				focusButton.ButtonState = PopupButton.PopupButtonState.Normal;
			base.OnLostFocus (e);
		}
		
		protected override bool IsInputKey (Keys key)
		{
			switch (key) {
				case Keys.Up:
				case Keys.Down:
				case Keys.Right:
				case Keys.Left:
				case Keys.Enter:
					return true;
			}
			return base.IsInputKey (key);
		}
		
		private void Key_Down (object sender, KeyEventArgs e)
		{
			bool update_focus = false;
			
			if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up) {
				currentFocusIndex --;
				
				if (currentFocusIndex < 0)
					currentFocusIndex = Controls.Count - 1;
				
				update_focus = true;
			} else
			if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Right) {
				currentFocusIndex++;
				
				if (currentFocusIndex == Controls.Count)
					currentFocusIndex = 0;
				
				update_focus = true;
			} else
			if (e.KeyCode == Keys.Enter) {
				focusButton.ButtonState = PopupButton.PopupButtonState.Down;
				OnClickButton (focusButton, EventArgs.Empty);
			}
			
			if (update_focus) {
				PopupButton newfocusButton = Controls [currentFocusIndex] as PopupButton;
				if (focusButton != null && focusButton.ButtonState != PopupButton.PopupButtonState.Down)
					focusButton.ButtonState = PopupButton.PopupButtonState.Normal;
				if (newfocusButton.ButtonState != PopupButton.PopupButtonState.Down)
					newfocusButton.ButtonState = PopupButton.PopupButtonState.Up;
				focusButton = newfocusButton;
			}
			
			e.Handled = true;
		}
		
		public event EventHandler DirectoryChanged {
			add { on_directory_changed += value; }
			remove { on_directory_changed -= value; }
		}
	}
	#endregion
	
	#region DirComboBox
	internal class DirComboBox : ComboBox, IUpdateFolder
	{
		#region DirComboBoxItem
		internal class DirComboBoxItem
		{
			private int imageIndex;
			private string name;
			private string path;
			private int xPos;
			private ImageList imageList;
			
			public DirComboBoxItem (ImageList imageList, int imageIndex, string name, string path, int xPos)
			{
				this.imageList = imageList;
				this.imageIndex = imageIndex;
				this.name = name;
				this.path = path;
				this.xPos = xPos;
			}
			
			public int ImageIndex {
				get {
					return imageIndex;
				}
			}
			
			public string Name {
				get {
					return name;
				}
			}
			
			public string Path {
				get {
					return path;
				}
			}
			
			public int XPos {
				get {
					return xPos;
				}
			}
			
			public ImageList ImageList {
				set {
					imageList = value;
				}
				
				get {
					return imageList;
				}
			}
		}
		#endregion
		
		private ImageList imageList = new ImageList();
		
		private string currentPath;
		
		private EventHandler on_directory_changed;
		
		private bool currentpath_internal_change = false;
		
		private Stack folderStack = new Stack();
		
		private static readonly int indent = 6;
		
		private DirComboBoxItem recentlyUsedDirComboboxItem;
		private DirComboBoxItem desktopDirComboboxItem;
		private DirComboBoxItem personalDirComboboxItem;
		private DirComboBoxItem myComputerDirComboboxItem;
		private DirComboBoxItem networkDirComboboxItem;
		
		private ArrayList myComputerItems = new ArrayList ();
		
		private DirComboBoxItem mainParentDirComboBoxItem = null;
		private DirComboBoxItem real_parent = null;
		
		private MWFVFS vfs;
		
		public DirComboBox (MWFVFS vfs)
		{
			SuspendLayout ();
			
			DrawMode = DrawMode.OwnerDrawFixed;
			
			imageList.ColorDepth = ColorDepth.Depth32Bit;
			imageList.ImageSize = new Size (16, 16);
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesRecentDocuments, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesDesktop, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesPersonal, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesMyComputer, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesMyNetwork, 16));
			imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.NormalFolder, 16));
			imageList.TransparentColor = Color.Transparent;
			
			recentlyUsedDirComboboxItem = new DirComboBoxItem (imageList, 0, "Recently used", MWFVFS.RecentlyUsedPrefix, 0);
			desktopDirComboboxItem = new DirComboBoxItem (imageList, 1, "Desktop", MWFVFS.DesktopPrefix, 0);
			personalDirComboboxItem = new DirComboBoxItem (imageList, 2, "Personal folder", MWFVFS.PersonalPrefix, indent);
			myComputerDirComboboxItem = new DirComboBoxItem (imageList, 3, "My Computer", MWFVFS.MyComputerPrefix, indent);
			networkDirComboboxItem = new DirComboBoxItem (imageList, 4, "My Network", MWFVFS.MyNetworkPrefix, indent);
			
			ArrayList al = vfs.GetMyComputerContent ();
			
			foreach (FSEntry fsEntry in al) {
				myComputerItems.Add (new DirComboBoxItem (MimeIconEngine.LargeIcons, fsEntry.IconIndex, fsEntry.Name, fsEntry.FullName, indent * 2));
			}
			
			mainParentDirComboBoxItem = myComputerDirComboboxItem;
			
			ResumeLayout (false);
		}
		
		public string CurrentFolder {
			set {
				currentPath = value;
				
				currentpath_internal_change = true;
				
				CreateComboList ();
			}
			get {
				return currentPath;
			}
		}
		
		private void CreateComboList ()
		{
			real_parent = null;
			DirComboBoxItem selection = null;
			
			if (currentPath == MWFVFS.RecentlyUsedPrefix) {
				mainParentDirComboBoxItem = recentlyUsedDirComboboxItem;
				selection = recentlyUsedDirComboboxItem;
			} else
			if (currentPath == MWFVFS.DesktopPrefix) {
				selection = desktopDirComboboxItem;
				mainParentDirComboBoxItem = desktopDirComboboxItem;
			} else
			if (currentPath == MWFVFS.PersonalPrefix) {
				selection = personalDirComboboxItem;
				mainParentDirComboBoxItem = personalDirComboboxItem;
			} else
			if (currentPath == MWFVFS.MyComputerPrefix) {
				selection = myComputerDirComboboxItem;
				mainParentDirComboBoxItem = myComputerDirComboboxItem;
			} else
			if (currentPath == MWFVFS.MyNetworkPrefix) {
				selection = networkDirComboboxItem;
				mainParentDirComboBoxItem = networkDirComboboxItem;
			} else {
				foreach (DirComboBoxItem dci in myComputerItems) {
					if (dci.Path == currentPath) {
						mainParentDirComboBoxItem = selection = dci;
						break;
					}
				}
			}
			
			BeginUpdate ();
			
			Items.Clear ();
			
			Items.Add (recentlyUsedDirComboboxItem);
			Items.Add (desktopDirComboboxItem);
			Items.Add (personalDirComboboxItem);
			Items.Add (myComputerDirComboboxItem);
			Items.AddRange (myComputerItems);
			Items.Add (networkDirComboboxItem);
			
			if (selection == null) {
				real_parent = CreateFolderStack ();
			}
			
			if (real_parent != null) {
				int local_indent = 0;
				
				if (real_parent == desktopDirComboboxItem)
					local_indent = 1;
				else
				if (real_parent == personalDirComboboxItem || real_parent == networkDirComboboxItem)
					local_indent = 2;
				else
					local_indent = 3;
				
				selection = AppendToParent (local_indent, real_parent);
			}
			
			EndUpdate ();
			
			if (selection != null)
				SelectedItem = selection;
		}
		
		private DirComboBoxItem CreateFolderStack ()
		{
			folderStack.Clear ();
			
			DirectoryInfo di = new DirectoryInfo (currentPath);
			
			folderStack.Push (di);
			
			while (di.Parent != null) {
				di = di.Parent;
				
				if (mainParentDirComboBoxItem != personalDirComboboxItem && di.FullName == ThemeEngine.Current.Places (UIIcon.PlacesDesktop))
					return desktopDirComboboxItem;
				else
				if (mainParentDirComboBoxItem == personalDirComboboxItem) {
					if (di.FullName == ThemeEngine.Current.Places (UIIcon.PlacesPersonal))
						return personalDirComboboxItem;
				} else
					foreach (DirComboBoxItem dci in myComputerItems) {
						if (dci.Path == di.FullName) {
							return dci;
						}
					}
				
				
				folderStack.Push (di);
			}
			
			return null;
		}
		
		private DirComboBoxItem AppendToParent (int nr_indents, DirComboBoxItem parentDirComboBoxItem)
		{
			DirComboBoxItem selection = null;
			
			int index = Items.IndexOf (parentDirComboBoxItem) + 1;
			
			int xPos = indent * nr_indents;
			
			while (folderStack.Count != 0) {
				DirectoryInfo dii = folderStack.Pop () as DirectoryInfo;
				
				DirComboBoxItem dci = new DirComboBoxItem (imageList, 5, dii.Name, dii.FullName, xPos);
				
				Items.Insert (index, dci);
				index++;
				selection = dci;
				xPos += indent;
			}
			
			return selection;
		}
		
		protected override void OnDrawItem (DrawItemEventArgs e)
		{
			if (e.Index == -1)
				return;
			
			DirComboBoxItem dcbi = Items [e.Index] as DirComboBoxItem;
			
			Bitmap bmp = new Bitmap (e.Bounds.Width, e.Bounds.Height, e.Graphics);
			Graphics gr = Graphics.FromImage (bmp);
			
			Color backColor = e.BackColor;
			Color foreColor = e.ForeColor;
			
			int xPos = dcbi.XPos;
			
			if ((e.State & DrawItemState.ComboBoxEdit) != 0)
				xPos = 0;
			else
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				backColor = ThemeEngine.Current.ColorHighlight;
				foreColor = ThemeEngine.Current.ColorHighlightText;
			}
			
			gr.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (backColor), new Rectangle (0, 0, bmp.Width, bmp.Height));
			
			gr.DrawString (dcbi.Name, e.Font , ThemeEngine.Current.ResPool.GetSolidBrush (foreColor), new Point (24 + xPos, (bmp.Height - e.Font.Height) / 2));
			gr.DrawImage (dcbi.ImageList.Images [dcbi.ImageIndex], new Rectangle (new Point (xPos + 2, 0), new Size (16, 16)));
			
			e.Graphics.DrawImage (bmp, e.Bounds.X, e.Bounds.Y);
			gr.Dispose ();
			bmp.Dispose ();
		}
		
		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			if (Items.Count > 0) {
				DirComboBoxItem dcbi = Items [SelectedIndex] as DirComboBoxItem;
				
				currentPath = dcbi.Path;
				// call DirectoryChange event only if the user changes the index with the ComboBox
				
				if (!currentpath_internal_change) {
					if (on_directory_changed != null)
						on_directory_changed (this, EventArgs.Empty);
				}
			}
			
			currentpath_internal_change = false;
		}
		
		public event EventHandler DirectoryChanged {
			add { on_directory_changed += value; }
			remove { on_directory_changed -= value; }
		}
	}
	#endregion
	
	#region FilterStruct
	internal struct FilterStruct
	{
		public string filterName;
		public StringCollection filters;
		
		public FilterStruct (string filterName, string filter)
		{
			this.filterName = filterName;
			
			filters =  new StringCollection ();
			
			SplitFilters (filter);
		}
		
		private void SplitFilters (string filter)
		{
			string[] split = filter.Split (new char [] {';'});
			
			filters.AddRange (split);
		}
	}
	#endregion
	
	#region FileFilter
	internal class FileFilter
	{
		private ArrayList filterArrayList = new ArrayList();
		
		private string filter;
		
		public FileFilter ()
		{}
		
		public FileFilter (string filter)
		{
			this.filter = filter;
			
			SplitFilter ();
		}
		
		public ArrayList FilterArrayList {
			set {
				filterArrayList = value;
			}
			
			get {
				return filterArrayList;
			}
		}
		
		public string Filter {
			set {
				filter = value;
				
				SplitFilter ();
			}
			
			get {
				return filter;
			}
		}
		
		private void SplitFilter ()
		{
			filterArrayList.Clear ();
			
			if (filter == null)
				throw new NullReferenceException ("Filter");
			
			if (filter.Length == 0)
				return;
			
			string[] filters = filter.Split (new char [] {'|'});
			
			if ((filters.Length % 2) != 0)
				throw new ArgumentException ("Filter");
			
			for (int i = 0; i < filters.Length; i += 2) {
				FilterStruct filterStruct = new FilterStruct (filters [i], filters [i + 1]);
				
				filterArrayList.Add (filterStruct);
			}
		}
	}
	#endregion
	
	#region MWFFileView		
	// MWFFileView
	internal class MWFFileView : ListView
	{
		private ArrayList filterArrayList;
		
		private bool showHiddenFiles = false;
		
		private EventHandler on_selected_file_changed;
		private EventHandler on_selected_files_changed;
		private EventHandler on_directory_changed;
		private EventHandler on_force_dialog_end;
		
		private string selectedFilesString;
		
		private int filterIndex = 1;
		
		private ToolTip toolTip;
		private int oldItemIndexForToolTip = -1;
		
		private ContextMenu contextMenu;
		
		private MenuItem menuItemView;
		private MenuItem menuItemNew;
		
		private MenuItem smallIconMenutItem;
		private MenuItem tilesMenutItem;
		private MenuItem largeIconMenutItem;
		private MenuItem listMenutItem;
		private MenuItem detailsMenutItem;
		
		private MenuItem newFolderMenuItem; 
		private MenuItem showHiddenFilesMenuItem;
		
		private int previousCheckedMenuItemIndex;
		
		private ArrayList viewMenuItemClones = new ArrayList ();
		
		private FSEntry currentFSEntry = null;
		
		private string currentFolder;
		private string currentRealFolder;
		private FSEntry currentFolderFSEntry;
		
		// store DirectoryInfo for a back button for example
		private Stack directoryStack = new Stack();
		
		// list of controls(components to enable or disable depending on current directoryStack.Count
		private ArrayList dirStackControlsOrComponents = new ArrayList ();
		
		private ToolBarButton folderUpToolBarButton;
		
		private ArrayList registered_senders = new ArrayList ();
		
		private bool should_push = true;
		
		private MWFVFS vfs;
		
		private View old_view;
		
		private int old_menuitem_index;
		private bool do_update_view = false;
		
		public MWFFileView (MWFVFS vfs)
		{
			this.vfs = vfs;
			
			SuspendLayout ();
			
			contextMenu = new ContextMenu ();
			
			toolTip = new ToolTip ();
			toolTip.InitialDelay = 300;
			toolTip.ReshowDelay = 0; 
			
			// contextMenu
			
			// View menu item
			menuItemView = new MenuItem ("View");
			
			smallIconMenutItem = new MenuItem ("Small Icon", new EventHandler (OnClickViewMenuSubItem));
			smallIconMenutItem.RadioCheck = true;
			menuItemView.MenuItems.Add (smallIconMenutItem);
			
			tilesMenutItem = new MenuItem ("Tiles", new EventHandler (OnClickViewMenuSubItem));
			tilesMenutItem.RadioCheck = true;
			menuItemView.MenuItems.Add (tilesMenutItem);
			
			largeIconMenutItem = new MenuItem ("Large Icon", new EventHandler (OnClickViewMenuSubItem));
			largeIconMenutItem.RadioCheck = true;
			menuItemView.MenuItems.Add (largeIconMenutItem);
			
			listMenutItem = new MenuItem ("List", new EventHandler (OnClickViewMenuSubItem));
			listMenutItem.RadioCheck = true;
			listMenutItem.Checked = true;
			menuItemView.MenuItems.Add (listMenutItem);
			previousCheckedMenuItemIndex = listMenutItem.Index;
			
			detailsMenutItem = new MenuItem ("Details", new EventHandler (OnClickViewMenuSubItem));
			detailsMenutItem.RadioCheck = true;
			menuItemView.MenuItems.Add (detailsMenutItem);
			
			contextMenu.MenuItems.Add (menuItemView);
			
			contextMenu.MenuItems.Add (new MenuItem ("-"));
			
			// New menu item
			menuItemNew = new MenuItem ("New");
			
			newFolderMenuItem = new MenuItem ("New Folder", new EventHandler (OnClickNewFolderMenuItem));
			menuItemNew.MenuItems.Add (newFolderMenuItem);
			
			contextMenu.MenuItems.Add (menuItemNew);
			
			contextMenu.MenuItems.Add (new MenuItem ("-"));
			
			// Show hidden files menu item
			showHiddenFilesMenuItem = new MenuItem ("Show hidden files", new EventHandler (OnClickContextMenu));
			showHiddenFilesMenuItem.Checked = showHiddenFiles;
			contextMenu.MenuItems.Add (showHiddenFilesMenuItem);
			
			LabelWrap = true;
			
			SmallImageList = MimeIconEngine.SmallIcons;
			LargeImageList = MimeIconEngine.LargeIcons;
			
			View = old_view = View.List;
			LabelEdit = true;
			
			ContextMenu = contextMenu;
			
			ResumeLayout (false);
			
//			currentFolder = Environment.CurrentDirectory;
			
			KeyDown += new KeyEventHandler (MWF_KeyDown);
		}
		
		public string CurrentFolder {
			get {
				return currentFolder;
			}
			set {
				currentFolder = value;
			}
		}
		
		public string CurrentRealFolder {
			get {
				return currentRealFolder;
			}
		}
		
		public FSEntry CurrentFSEntry {
			get {
				return currentFSEntry;
			}
		}
		
		public MenuItem[] ViewMenuItems {
			get {
				MenuItem[] menuItemClones = new MenuItem [] {
					smallIconMenutItem.CloneMenu (),
					tilesMenutItem.CloneMenu (),
					largeIconMenutItem.CloneMenu (),
					listMenutItem.CloneMenu (),
					detailsMenutItem.CloneMenu ()
				};
				
				viewMenuItemClones.Add (menuItemClones);
				
				return menuItemClones;
			}
		}
		
		public ArrayList FilterArrayList {
			set {
				filterArrayList = value;
			}
			
			get {
				return filterArrayList;
			}
		}
		
		public bool ShowHiddenFiles {
			set {
				showHiddenFiles = value;
			}
			
			get {
				return showHiddenFiles;
			}
		}
		
		public int FilterIndex {
			set {
				filterIndex = value;
				if (Visible)
					UpdateFileView (currentFolder);
			}
			
			get {
				return filterIndex;
			}
		}
		
		public string SelectedFilesString {
			set {
				selectedFilesString = value;
			}
			
			get {
				return selectedFilesString;
			}
		}
		
		public void PushDir ()
		{
			if (currentFolder != null)
				directoryStack.Push (currentFolder);
			
			EnableOrDisableDirstackObjects ();
		}
		
		public void PopDir ()
		{
			if (directoryStack.Count == 0)
				return;
			
			string new_folder = directoryStack.Pop () as string;
			
			EnableOrDisableDirstackObjects ();
			
			should_push = false;
			
			ChangeDirectory (null, new_folder);
		}
		
		public void RegisterSender (IUpdateFolder iud)
		{
			registered_senders.Add (iud);
		}
		
		public void CreateNewFolder ()
		{
			if (currentFolder == MWFVFS.RecentlyUsedPrefix)
				return;
			
			FSEntry fsEntry = new FSEntry ();
			fsEntry.Attributes = FileAttributes.Directory;
			fsEntry.FileType = FSEntry.FSEntryType.Directory;
			fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("inode/directory");
			fsEntry.LastAccessTime = DateTime.Now;
			
			// FIXME: when ListView.LabelEdit is available use it
//			listViewItem.BeginEdit();
			
			TextEntryDialog ted = new TextEntryDialog ();
			ted.IconPictureBoxImage = MimeIconEngine.LargeIcons.Images.GetImage (fsEntry.IconIndex);
			
			string folder = "";
			
			if (currentFolderFSEntry.RealName != null)
				folder = currentFolderFSEntry.RealName;
			else
				folder = currentFolder;
			
			string tmp_filename = "New Folder";
			
			if (Directory.Exists (Path.Combine (folder, tmp_filename))) {
				int i = 1;
				
				tmp_filename = tmp_filename + " (" + i + ")";
				
				while (Directory.Exists (Path.Combine (folder, tmp_filename))) {
					i++;
					tmp_filename = "New Folder" + " (" + i + ")";
				}
			}
			
			ted.FileName = tmp_filename;
			
			if (ted.ShowDialog () == DialogResult.OK) {
				string new_folder = Path.Combine (folder, ted.FileName);
				
				if (vfs.CreateFolder (new_folder)) {
					fsEntry.FullName = new_folder;
					fsEntry.Name = ted.FileName;
					
					FileViewListViewItem listViewItem = new FileViewListViewItem (fsEntry);
					
					BeginUpdate ();
					Items.Add (listViewItem);
					EndUpdate ();
					
					listViewItem.EnsureVisible ();
				}
			}
		}
		
		public void SetSelectedIndexTo (string fname)
		{
			foreach (FileViewListViewItem item in Items) {
				if (item.Text == fname) {
					BeginUpdate ();
					SelectedItems.Clear ();
					item.Selected = true;
					EndUpdate ();
					break;
				}
			}
		}
		
		public void OneDirUp ()
		{
			string parent_folder = vfs.GetParent ();
			if (parent_folder != null)
				ChangeDirectory (null, parent_folder);
		}
		
		public void ChangeDirectory (object sender, string folder)
		{
			if (folder == MWFVFS.DesktopPrefix || folder == MWFVFS.RecentlyUsedPrefix)
				folderUpToolBarButton.Enabled = false;
			else
				folderUpToolBarButton.Enabled = true;
			
			foreach (IUpdateFolder iuf in registered_senders) {
				iuf.CurrentFolder = folder;
			}
			
			if (should_push)
				PushDir ();
			else
				should_push = true;
			
			currentFolderFSEntry = vfs.ChangeDirectory (folder);
			
			currentFolder = folder;
			
			if (currentFolder.IndexOf ("://") != -1)
				currentRealFolder = currentFolderFSEntry.RealName;
			else
				currentRealFolder = currentFolder;
			
			BeginUpdate ();
			
			Items.Clear ();
			SelectedItems.Clear ();
			
			if (folder == MWFVFS.RecentlyUsedPrefix) {
				old_view = View;
				View = View.Details;
				old_menuitem_index = previousCheckedMenuItemIndex;
				UpdateMenuItems (detailsMenutItem);
				do_update_view = true;
			} else
			if (View != old_view && do_update_view) {
				UpdateMenuItems (menuItemView.MenuItems [old_menuitem_index]);
				View = old_view;
				do_update_view = false;
			}
			EndUpdate ();
			
			UpdateFileView (folder);
		}
		
		public void UpdateFileView (string folder)
		{
			ArrayList directoriesArrayList;
			ArrayList fileArrayList;
			
			if (filterArrayList != null && filterArrayList.Count != 0) {
				FilterStruct fs = (FilterStruct)filterArrayList [filterIndex - 1];
				
				vfs.GetFolderContent (fs.filters, out directoriesArrayList, out fileArrayList);
			} else
				vfs.GetFolderContent (out directoriesArrayList, out fileArrayList);
			
			BeginUpdate ();
			
			Items.Clear ();
			SelectedItems.Clear ();
			
			foreach (FSEntry directoryFSEntry in directoriesArrayList) {
				if (!ShowHiddenFiles)
					if (directoryFSEntry.Name.StartsWith (".") || directoryFSEntry.Attributes == FileAttributes.Hidden)
						continue;
				
				FileViewListViewItem listViewItem = new FileViewListViewItem (directoryFSEntry);
				
				Items.Add (listViewItem);
			}
			
			StringCollection collection = new StringCollection ();
			
			foreach (FSEntry fsEntry in fileArrayList) {
				
				// remove duplicates. that can happen when you read recently used files for example
				if (collection.Contains (fsEntry.Name)) {
					
					string fileName = fsEntry.Name;
					
					if (collection.Contains (fileName)) {
						int i = 1;
						
						while (collection.Contains (fileName + "(" + i + ")")) {
							i++;
						}
						
						fileName = fileName + "(" + i + ")";
					}
					
					fsEntry.Name = fileName;
				}
				
				collection.Add (fsEntry.Name);
				
				DoOneFSEntry (fsEntry);
			}
			
			EndUpdate ();
		}
		
		public void AddControlToEnableDisableByDirStack (object control)
		{
			dirStackControlsOrComponents.Add (control);
		}
		
		public void SetFolderUpToolBarButton (ToolBarButton tb)
		{
			folderUpToolBarButton = tb;
		}
		
		public void WriteRecentlyUsed (string fullfilename)
		{
			vfs.WriteRecentlyUsedFiles (fullfilename);
		}
		
		private void EnableOrDisableDirstackObjects ()
		{
			foreach (object o in dirStackControlsOrComponents) {
				if (o is Control) {
					Control c = o as Control;
					c.Enabled = (directoryStack.Count > 1);
				} else
				if (o is ToolBarButton) {
					ToolBarButton t = o as ToolBarButton;
					t.Enabled = (directoryStack.Count > 1);
				}
			}
		}
		
		private void DoOneFSEntry (FSEntry fsEntry) 
		{
			if (!ShowHiddenFiles)
				if (fsEntry.Name.StartsWith (".")  || fsEntry.Attributes == FileAttributes.Hidden)
					return;
			
			FileViewListViewItem listViewItem = new FileViewListViewItem (fsEntry);
			
			Items.Add (listViewItem);
		}
		
		private void MWF_KeyDown (object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Back) {
				OneDirUp ();
			}
		}
		
		protected override void OnClick (EventArgs e)
		{
			if (!MultiSelect) {
				if (SelectedItems.Count > 0) {
					FileViewListViewItem listViewItem = SelectedItems [0] as FileViewListViewItem;
					
					FSEntry fsEntry = listViewItem.FSEntry;
					
					if (fsEntry.FileType == FSEntry.FSEntryType.File) {
						currentFSEntry = fsEntry;
						
						if (on_selected_file_changed != null)
							on_selected_file_changed (this, EventArgs.Empty);
					}
				}
			}
			
			base.OnClick (e);
		}
		
		protected override void OnDoubleClick (EventArgs e)
		{
			if (SelectedItems.Count > 0) {
				FileViewListViewItem listViewItem = SelectedItems [0] as FileViewListViewItem;
				
				FSEntry fsEntry = listViewItem.FSEntry;
				
				if (fsEntry.Attributes == FileAttributes.Directory) {
					
					ChangeDirectory (null, fsEntry.FullName);
					
					if (on_directory_changed != null)
						on_directory_changed (this, EventArgs.Empty);
				} else {
					currentFSEntry = fsEntry;
					
					if (on_selected_file_changed != null)
						on_selected_file_changed (this, EventArgs.Empty);
					
					if (on_force_dialog_end != null)
						on_force_dialog_end (this, EventArgs.Empty);
					
					return;
				}
			}
			
			base.OnDoubleClick (e);
		}
		
		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			if (SelectedItems.Count > 0) {
				selectedFilesString = "";
				
				if (SelectedItems.Count == 1) {
					FileViewListViewItem listViewItem = SelectedItems [0] as FileViewListViewItem;
					
					FSEntry fsEntry = listViewItem.FSEntry;
					
					if (fsEntry.Attributes != FileAttributes.Directory)
						selectedFilesString = SelectedItems [0].Text;
				} else {
					foreach (FileViewListViewItem lvi in SelectedItems) {
						FSEntry fsEntry = lvi.FSEntry;
						
						if (fsEntry.Attributes != FileAttributes.Directory)
							selectedFilesString = selectedFilesString + "\"" + lvi.Text + " ";
					}
				}
				
				if (on_selected_files_changed != null)
					on_selected_files_changed (this, EventArgs.Empty);
			}
			
			base.OnSelectedIndexChanged (e);
		}
		
		protected override void OnMouseMove (MouseEventArgs e)
		{
			FileViewListViewItem item = GetItemAt (e.X, e.Y) as FileViewListViewItem;
			
			if (item != null) {
				int currentItemIndex = item.Index;
				
				if (currentItemIndex != oldItemIndexForToolTip) {
					oldItemIndexForToolTip = currentItemIndex;
					
					if (toolTip != null && toolTip.Active)
						toolTip.Active = false;
					
					FSEntry fsEntry = item.FSEntry;
					
					string output = String.Empty;
					
					if (fsEntry.FileType == FSEntry.FSEntryType.Directory)
						output = "Directory: " + fsEntry.FullName;
					else if (fsEntry.FileType == FSEntry.FSEntryType.Device)
						output = "Device: "+ fsEntry.FullName;
					else if (fsEntry.FileType == FSEntry.FSEntryType.Network)
						output = "Network: " + fsEntry.FullName;
					else
						output = "File: " + fsEntry.FullName;
					
					toolTip.SetToolTip (this, output);	
					
					toolTip.Active = true;
				}
			}
			
			base.OnMouseMove (e);
		}
		
		void OnClickContextMenu (object sender, EventArgs e)
		{
			MenuItem senderMenuItem = sender as MenuItem;
			
			if (senderMenuItem == showHiddenFilesMenuItem) {
				senderMenuItem.Checked = !senderMenuItem.Checked;
				showHiddenFiles = senderMenuItem.Checked;
				UpdateFileView (currentFolder);
			}
		}
		
		void OnClickViewMenuSubItem (object sender, EventArgs e)
		{
			MenuItem senderMenuItem = (MenuItem)sender;
			
			UpdateMenuItems (senderMenuItem);
			
			// update me
			
			switch (senderMenuItem.Index) {
				case 0:
					View = View.SmallIcon;
					break;
				case 1:
					View = View.LargeIcon;
					break;
				case 2:
					View = View.LargeIcon;
					break;
				case 3:
					View = View.List;
					break;
				case 4:
					View = View.Details;
					break;
				default:
					break;
			}
		}
		
		private void UpdateMenuItems (MenuItem senderMenuItem)
		{
			menuItemView.MenuItems [previousCheckedMenuItemIndex].Checked = false;
			menuItemView.MenuItems [senderMenuItem.Index].Checked = true;
			
			foreach (MenuItem[] items in viewMenuItemClones) {
				items [previousCheckedMenuItemIndex].Checked = false;
				items [senderMenuItem.Index].Checked = true;
			}
			
			previousCheckedMenuItemIndex = senderMenuItem.Index;
		}
		
		void OnClickNewFolderMenuItem (object sender, EventArgs e)
		{
			CreateNewFolder ();
		}
		
		public event EventHandler SelectedFileChanged {
			add { on_selected_file_changed += value; }
			remove { on_selected_file_changed -= value; }
		}
		
		public event EventHandler SelectedFilesChanged {
			add { on_selected_files_changed += value; }
			remove { on_selected_files_changed -= value; }
		}
		
		public event EventHandler DirectoryChanged {
			add { on_directory_changed += value; }
			remove { on_directory_changed -= value; }
		}
		
		public event EventHandler ForceDialogEnd {
			add { on_force_dialog_end += value; }
			remove { on_force_dialog_end -= value; }
		}
	}
	#endregion
	
	#region FileListViewItem
	internal class FileViewListViewItem : ListViewItem
	{
		private FSEntry fsEntry;
		
		public FileViewListViewItem (FSEntry fsEntry)
		{
			this.fsEntry = fsEntry;
			
			ImageIndex = fsEntry.IconIndex;
			
			Text = fsEntry.Name;
			
			switch (fsEntry.FileType) {
				case FSEntry.FSEntryType.Directory:
					SubItems.Add ("");
					SubItems.Add ("Directory");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				case FSEntry.FSEntryType.File:
					long fileLen = 1;
					try {
						if (fsEntry.FileSize > 1024)
							fileLen = fsEntry.FileSize / 1024;
					} catch (Exception) {
						fileLen = 1;
					}
					
					SubItems.Add (fileLen.ToString () + " KB");
					SubItems.Add ("File");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				case FSEntry.FSEntryType.Device:
					SubItems.Add ("");
					SubItems.Add ("Device");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				case FSEntry.FSEntryType.RemovableDevice:
					SubItems.Add ("");
					SubItems.Add ("RemovableDevice");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				default:
					break;
			}
		}
		
		public FSEntry FSEntry {
			set {
				fsEntry = value;
			}
			
			get {
				return fsEntry;
			}
		}
	}
	#endregion
	
	#region IUpdateFolder
	internal interface IUpdateFolder
	{
		string CurrentFolder {get; set;}
	}
	#endregion
	
	#region TextEntryDialog
	// FIXME: When ListView.LabelEdit is implemented remove me
	internal class TextEntryDialog : Form
	{
		private Label label1;
		private Button okButton;
		private TextBox newNameTextBox;
		private PictureBox iconPictureBox;
		private Button cancelButton;
		private GroupBox groupBox1;
		
		public TextEntryDialog ()
		{
			groupBox1 = new GroupBox ();
			cancelButton = new Button ();
			iconPictureBox = new PictureBox ();
			newNameTextBox = new TextBox ();
			okButton = new Button ();
			label1 = new Label ();
			groupBox1.SuspendLayout ();
			SuspendLayout ();
			
			// groupBox1
			groupBox1.Controls.Add (newNameTextBox);
			groupBox1.Controls.Add (label1);
			groupBox1.Controls.Add (iconPictureBox);
			groupBox1.Location = new Point (8, 8);
			groupBox1.Size = new Size (232, 160);
			groupBox1.TabIndex = 5;
			groupBox1.TabStop = false;
			groupBox1.Text = "New Name";
			
			// cancelButton
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.Location = new Point (168, 176);
			cancelButton.TabIndex = 4;
			cancelButton.Text = "Cancel";
			
			// iconPictureBox
			iconPictureBox.BorderStyle = BorderStyle.Fixed3D;
			iconPictureBox.Location = new Point (86, 24);
			iconPictureBox.Size = new Size (60, 60);
			iconPictureBox.TabIndex = 3;
			iconPictureBox.TabStop = false;
			iconPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
			
			// newNameTextBox
			newNameTextBox.Location = new Point (16, 128);
			newNameTextBox.Size = new Size (200, 20);
			newNameTextBox.TabIndex = 5;
			newNameTextBox.Text = "";
			
			// okButton
			okButton.DialogResult = DialogResult.OK;
			okButton.Location = new Point (80, 176);
			okButton.TabIndex = 3;
			okButton.Text = "OK";
			
			// label1
			label1.Location = new Point (16, 96);
			label1.Size = new Size (200, 23);
			label1.TabIndex = 4;
			label1.Text = "Enter Name:";
			label1.TextAlign = ContentAlignment.MiddleCenter;
			
			// MainForm
			AcceptButton = okButton;
			AutoScaleBaseSize = new Size (5, 13);
			CancelButton = cancelButton;
			ClientSize = new Size (248, 205);
			Controls.Add (groupBox1);
			Controls.Add (cancelButton);
			Controls.Add (okButton);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Text = "New Folder or File";
			groupBox1.ResumeLayout (false);
			ResumeLayout (false);
			
			newNameTextBox.Select ();
		}
		
		public Image IconPictureBoxImage {
			set {
				iconPictureBox.Image = value;
			}
		}
		
		public string FileName {
			get {
				return newNameTextBox.Text;
			}
			set {
				newNameTextBox.Text = value;
			}
		}
	}
	#endregion
	
	#region MWFVFS	
	internal class MWFVFS
	{
		private FileSystem fileSystem;
		
		private int platform = (int) Environment.OSVersion.Platform;
		
		public static readonly string DesktopPrefix = "Desktop://";
		public static readonly string PersonalPrefix = "Personal://";
		public static readonly string MyComputerPrefix = "MyComputer://";
		public static readonly string RecentlyUsedPrefix = "RecentlyUsed://";
		public static readonly string MyNetworkPrefix = "MyNetwork://";
		public static readonly string MyComputerPersonalPrefix = "MyComputerPersonal://";
		
		public static Hashtable MyComputerDevicesPrefix = new Hashtable ();
		
		public MWFVFS ()
		{
			if ((platform == 4) || (platform == 128)) {
				fileSystem = new UnixFileSystem ();
			} else {
				fileSystem = new WinFileSystem ();
			}
		}
		
		public FSEntry ChangeDirectory (string folder)
		{
			return fileSystem.ChangeDirectory (folder);
		}
		
		public void GetFolderContent (out ArrayList folders_out, out ArrayList files_out)
		{
			fileSystem.GetFolderContent (null, out folders_out, out files_out);
		}
		
		public void GetFolderContent (StringCollection filters, out ArrayList folders_out, out ArrayList files_out)
		{
			fileSystem.GetFolderContent (filters, out folders_out, out files_out);
		}
		
		public void WriteRecentlyUsedFiles (string filename)
		{
			fileSystem.WriteRecentlyUsedFiles (filename);
		}
		
		public ArrayList GetRecentlyUsedFiles ()
		{
			return fileSystem.GetRecentlyUsedFiles ();
		}
		
		public ArrayList GetMyComputerContent ()
		{
			return fileSystem.GetMyComputerContent ();
		}
		
		public ArrayList GetMyNetworkContent ()
		{
			return fileSystem.GetMyNetworkContent ();
		}
		
		public bool CreateFolder (string new_folder)
		{
			try {
				if (Directory.Exists (new_folder)) {
					string message = "Folder \"" + new_folder + "\" already exists.";
					MessageBox.Show (message, new_folder, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				} else
					Directory.CreateDirectory (new_folder);
			} catch (Exception) {
				return false;
			}
			
			return true;
		}
		
		public string GetParent ()
		{
			return fileSystem.GetParent ();
		}
	}
	#endregion
	
	#region FileSystem
	internal abstract class FileSystem
	{
		protected string currentTopFolder = "";
		protected FSEntry currentFolderFSEntry = null;
		protected FSEntry currentTopFolderFSEntry = null;
		
		public FSEntry ChangeDirectory (string folder)
		{
			if (folder == MWFVFS.DesktopPrefix) {
				currentTopFolder = MWFVFS.DesktopPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetDesktopFSEntry ();
			} else
			if (folder == MWFVFS.PersonalPrefix) {
				currentTopFolder = MWFVFS.PersonalPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetPersonalFSEntry ();
			} else
			if (folder == MWFVFS.MyComputerPersonalPrefix) {
				currentTopFolder = MWFVFS.MyComputerPersonalPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetMyComputerPersonalFSEntry ();
			} else
			if (folder == MWFVFS.RecentlyUsedPrefix) {
				currentTopFolder = MWFVFS.RecentlyUsedPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetRecentlyUsedFSEntry ();
			} else
			if (folder == MWFVFS.MyComputerPrefix) {
				currentTopFolder = MWFVFS.MyComputerPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetMyComputerFSEntry ();
			} else
			if (folder == MWFVFS.MyNetworkPrefix) {
				currentTopFolder = MWFVFS.MyNetworkPrefix;
				currentTopFolderFSEntry = currentFolderFSEntry = GetMyNetworkFSEntry ();
			} else {
				bool found = false;
				
				foreach (DictionaryEntry entry in MWFVFS.MyComputerDevicesPrefix) {
					FSEntry fsEntry = entry.Value as FSEntry;
					if (folder == fsEntry.FullName) {
						currentTopFolder = entry.Key as string;
						currentTopFolderFSEntry = currentFolderFSEntry = fsEntry;
						found = true;
						break;
					}
				}
				
				if (!found) {
					currentFolderFSEntry = GetDirectoryFSEntry (new DirectoryInfo (folder), currentTopFolderFSEntry);
				}
			}
			
			return currentFolderFSEntry;
		}
		
		public string GetParent ()
		{
			return currentFolderFSEntry.Parent;
		}
		
		// directories_out and files_out contain FSEntry objects
		public void GetFolderContent (StringCollection filters, out ArrayList directories_out, out ArrayList files_out)
		{
			directories_out = new ArrayList ();
			files_out = new ArrayList ();
			
			if (currentFolderFSEntry.FullName == MWFVFS.DesktopPrefix) {
				FSEntry personalFSEntry = GetPersonalFSEntry ();
				
				directories_out.Add (personalFSEntry);
				
				FSEntry myComputerFSEntry = GetMyComputerFSEntry ();
				
				directories_out.Add (myComputerFSEntry);
				
				FSEntry myNetworkFSEntry = GetMyNetworkFSEntry ();
				
				directories_out.Add (myNetworkFSEntry);
				
				ArrayList d_out = new ArrayList ();
				ArrayList f_out = new ArrayList ();
				GetNormalFolderContent (ThemeEngine.Current.Places (UIIcon.PlacesDesktop), filters, out d_out, out f_out);
				directories_out.AddRange (d_out);
				files_out.AddRange (f_out);
				
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.RecentlyUsedPrefix) {
				files_out = GetRecentlyUsedFiles ();
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.MyComputerPrefix) {
				directories_out.AddRange (GetMyComputerContent ());
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.PersonalPrefix || currentFolderFSEntry.FullName == MWFVFS.MyComputerPersonalPrefix) {
				ArrayList d_out = new ArrayList ();
				ArrayList f_out = new ArrayList ();
				GetNormalFolderContent (ThemeEngine.Current.Places (UIIcon.PlacesPersonal), filters, out d_out, out f_out);
				directories_out.AddRange (d_out);
				files_out.AddRange (f_out);
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.MyNetworkPrefix) {
				directories_out.AddRange (GetMyNetworkContent ());
			} else {
				GetNormalFolderContent (currentFolderFSEntry.FullName, filters, out directories_out, out files_out);
			}
		}
		
		protected void GetNormalFolderContent (string from_folder, StringCollection filters, out ArrayList directories_out, out ArrayList files_out)
		{
			DirectoryInfo dirinfo = new DirectoryInfo (from_folder);
			
			directories_out = new ArrayList ();
			
			DirectoryInfo[] dirs = dirinfo.GetDirectories ();
			
			for (int i = 0; i < dirs.Length; i++) {
				directories_out.Add (GetDirectoryFSEntry (dirs [i], currentTopFolderFSEntry));
			}
			
			files_out = new ArrayList ();
			
			ArrayList files = new ArrayList ();
			
			if (filters == null) {
				files.AddRange (dirinfo.GetFiles ());
			} else {
				foreach (string s in filters)
					files.AddRange (dirinfo.GetFiles (s));
			}
			
			for (int i = 0; i < files.Count; i++) {
				files_out.Add (GetFileFSEntry (files [i] as FileInfo));
			}
		}
		
		protected virtual FSEntry GetDirectoryFSEntry (DirectoryInfo dirinfo, FSEntry topFolderFSEntry)
		{
			FSEntry fs = new FSEntry ();
			
			fs.Attributes = dirinfo.Attributes;
			fs.FullName = dirinfo.FullName;
			fs.Name = dirinfo.Name;
			fs.MainTopNode = topFolderFSEntry;
			fs.FileType = FSEntry.FSEntryType.Directory;
			fs.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("inode/directory");
			fs.LastAccessTime = dirinfo.LastAccessTime;
			
			return fs;
		}
		
		protected virtual FSEntry GetFileFSEntry (FileInfo fileinfo)
		{
			FSEntry fs = new FSEntry ();
			
			fs.Attributes = fileinfo.Attributes;
			fs.FullName = fileinfo.FullName;
			fs.Name = fileinfo.Name;
			fs.FileType = FSEntry.FSEntryType.File;
			fs.IconIndex = MimeIconEngine.GetIconIndexForFile (fileinfo.FullName);
			fs.LastAccessTime = fileinfo.LastAccessTime;
			// the following catches broken symbolic links
			if ((int)fs.Attributes != 0)
				fs.FileSize = fileinfo.Length;
			
			return fs;
		}
		
		
		protected abstract FSEntry GetDesktopFSEntry ();
		
		protected abstract FSEntry GetRecentlyUsedFSEntry ();
		
		protected abstract FSEntry GetPersonalFSEntry ();
		
		protected abstract FSEntry GetMyComputerPersonalFSEntry ();
		
		protected abstract FSEntry GetMyComputerFSEntry ();
		
		protected abstract FSEntry GetMyNetworkFSEntry ();
		
		public abstract void WriteRecentlyUsedFiles (string fileToAdd);
		
		public abstract ArrayList GetRecentlyUsedFiles ();
		
		public abstract ArrayList GetMyComputerContent ();
		
		public abstract ArrayList GetMyNetworkContent ();
	}
	#endregion
	
	#region UnixFileSystem
	internal class UnixFileSystem : FileSystem
	{
		private MasterMount masterMount = new MasterMount ();
		private FSEntry desktopFSEntry = null;
		private FSEntry recentlyusedFSEntry = null;
		private FSEntry personalFSEntry = null;
		private FSEntry mycomputerpersonalFSEntry = null;
		private FSEntry mycomputerFSEntry = null;
		private FSEntry mynetworkFSEntry = null;
		
		public UnixFileSystem ()
		{
			desktopFSEntry = new FSEntry ();
			
			desktopFSEntry.Attributes = FileAttributes.Directory;
			desktopFSEntry.FullName = MWFVFS.DesktopPrefix;
			desktopFSEntry.Name = "Desktop";
			desktopFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesDesktop);
			desktopFSEntry.FileType = FSEntry.FSEntryType.Directory;
			desktopFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("deskop/desktop");
			desktopFSEntry.LastAccessTime = DateTime.Now;
			
			recentlyusedFSEntry = new FSEntry ();
			
			recentlyusedFSEntry.Attributes = FileAttributes.Directory;
			recentlyusedFSEntry.FullName = MWFVFS.RecentlyUsedPrefix;
			recentlyusedFSEntry.Name = "Recently Used";
			recentlyusedFSEntry.FileType = FSEntry.FSEntryType.Directory;
			recentlyusedFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("recently/recently");
			recentlyusedFSEntry.LastAccessTime = DateTime.Now;
			
			personalFSEntry = new FSEntry ();
			
			personalFSEntry.Attributes = FileAttributes.Directory;
			personalFSEntry.FullName = MWFVFS.PersonalPrefix;
			personalFSEntry.Name = "Personal";
			personalFSEntry.MainTopNode = GetDesktopFSEntry ();
			personalFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			personalFSEntry.FileType = FSEntry.FSEntryType.Directory;
			personalFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("directory/home");
			personalFSEntry.LastAccessTime = DateTime.Now;
			
			mycomputerpersonalFSEntry = new FSEntry ();
			
			mycomputerpersonalFSEntry.Attributes = FileAttributes.Directory;
			mycomputerpersonalFSEntry.FullName = MWFVFS.MyComputerPersonalPrefix;
			mycomputerpersonalFSEntry.Name = "Personal";
			mycomputerpersonalFSEntry.MainTopNode = GetMyComputerFSEntry ();
			mycomputerpersonalFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			mycomputerpersonalFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mycomputerpersonalFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("directory/home");
			mycomputerpersonalFSEntry.LastAccessTime = DateTime.Now;
			
			mycomputerFSEntry = new FSEntry ();
			
			mycomputerFSEntry.Attributes = FileAttributes.Directory;
			mycomputerFSEntry.FullName = MWFVFS.MyComputerPrefix;
			mycomputerFSEntry.Name = "My Computer";
			mycomputerFSEntry.MainTopNode = GetDesktopFSEntry ();
			mycomputerFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mycomputerFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("workplace/workplace");
			mycomputerFSEntry.LastAccessTime = DateTime.Now;
			
			mynetworkFSEntry = new FSEntry ();
			
			mynetworkFSEntry.Attributes = FileAttributes.Directory;
			mynetworkFSEntry.FullName = MWFVFS.MyNetworkPrefix;
			mynetworkFSEntry.Name = "My Network";
			mynetworkFSEntry.MainTopNode = GetDesktopFSEntry ();
			mynetworkFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mynetworkFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("network/network");
			mynetworkFSEntry.LastAccessTime = DateTime.Now;
		}
		
		public override void WriteRecentlyUsedFiles (string fileToAdd)
		{
			string personal_folder = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			string recently_used_path = Path.Combine (personal_folder, ".recently-used");
			
			if (File.Exists (recently_used_path) && new FileInfo (recently_used_path).Length > 0) {
				XmlDocument xml_doc = new XmlDocument ();
				xml_doc.Load (recently_used_path);
				
				XmlNode grand_parent_node = xml_doc.SelectSingleNode ("RecentFiles");
				
				if (grand_parent_node != null) {
					// create a new element
					XmlElement new_recent_item_node = xml_doc.CreateElement ("RecentItem");
					
					XmlElement new_child = xml_doc.CreateElement ("URI");
					UriBuilder ub = new UriBuilder ();
					ub.Path = fileToAdd;
					ub.Host = null;
					ub.Scheme = "file";
					XmlText new_text_child = xml_doc.CreateTextNode (ub.ToString ());
					new_child.AppendChild (new_text_child);
					
					new_recent_item_node.AppendChild (new_child);
					
					new_child = xml_doc.CreateElement ("Mime-Type");
					new_text_child = xml_doc.CreateTextNode (Mime.GetMimeTypeForFile (fileToAdd));
					new_child.AppendChild (new_text_child);
					
					new_recent_item_node.AppendChild (new_child);
					
					new_child = xml_doc.CreateElement ("Timestamp");
					long seconds = (long)(DateTime.UtcNow - new DateTime (1970, 1, 1)).TotalSeconds;
					new_text_child = xml_doc.CreateTextNode (seconds.ToString ());
					new_child.AppendChild (new_text_child);
					
					new_recent_item_node.AppendChild (new_child);
					
					new_child = xml_doc.CreateElement ("Groups");
					
					new_recent_item_node.AppendChild (new_child);
					
					// now search the nodes in grand_parent_node for another instance of the new uri and if found remove it
					// so that the new node is the first one
					foreach (XmlNode n in grand_parent_node.ChildNodes) {
						XmlNode uri_node = n.SelectSingleNode ("URI");
						if (uri_node != null) {
							XmlNode uri_node_child = uri_node.FirstChild;
							if (uri_node_child is XmlText)
								if (ub.ToString () == ((XmlText)uri_node_child).Data) {
									grand_parent_node.RemoveChild (n);
									break;
								}
						}
					}
					
					// prepend the new recent item to the grand parent node list
					grand_parent_node.PrependChild (new_recent_item_node);
					
					// limit the # of RecentItems to 10
					if (grand_parent_node.ChildNodes.Count > 10) {
						while (grand_parent_node.ChildNodes.Count > 10)
							grand_parent_node.RemoveChild (grand_parent_node.LastChild);
					}
					
					try {
						xml_doc.Save (recently_used_path);
					} catch (Exception) {
					}
				}
			} else {
				XmlDocument xml_doc = new XmlDocument ();
				xml_doc.AppendChild (xml_doc.CreateXmlDeclaration ("1.0", "", ""));
				
				XmlElement recentFiles_element = xml_doc.CreateElement ("RecentFiles");
				
				XmlElement new_recent_item_node = xml_doc.CreateElement ("RecentItem");
				
				XmlElement new_child = xml_doc.CreateElement ("URI");
				UriBuilder ub = new UriBuilder ();
				ub.Path = fileToAdd;
				ub.Host = null;
				ub.Scheme = "file";
				XmlText new_text_child = xml_doc.CreateTextNode (ub.ToString ());
				new_child.AppendChild (new_text_child);
				
				new_recent_item_node.AppendChild (new_child);
				
				new_child = xml_doc.CreateElement ("Mime-Type");
				new_text_child = xml_doc.CreateTextNode (Mime.GetMimeTypeForFile (fileToAdd));
				new_child.AppendChild (new_text_child);
				
				new_recent_item_node.AppendChild (new_child);
				
				new_child = xml_doc.CreateElement ("Timestamp");
				long seconds = (long)(DateTime.UtcNow - new DateTime (1970, 1, 1)).TotalSeconds;
				new_text_child = xml_doc.CreateTextNode (seconds.ToString ());
				new_child.AppendChild (new_text_child);
				
				new_recent_item_node.AppendChild (new_child);
				
				new_child = xml_doc.CreateElement ("Groups");
				
				new_recent_item_node.AppendChild (new_child);
				
				recentFiles_element.AppendChild (new_recent_item_node);
				
				xml_doc.AppendChild (recentFiles_element);
				
				try {
					xml_doc.Save (recently_used_path);
				} catch (Exception) {
				}
			}
		}
		
		// return an ArrayList with FSEntry objects
		public override ArrayList GetRecentlyUsedFiles ()
		{
			// check for GNOME and KDE
			string personal_folder = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			string recently_used_path = Path.Combine (personal_folder, ".recently-used");
			
			ArrayList files_al = new ArrayList ();
			
			// GNOME
			if (File.Exists (recently_used_path)) {
				try {
					XmlTextReader xtr = new XmlTextReader (recently_used_path);
					while (xtr.Read ()) {
						if (xtr.NodeType == XmlNodeType.Element && xtr.Name.ToUpper () == "URI") {
							xtr.Read ();
							Uri uri = new Uri (xtr.Value);
							if (!files_al.Contains (uri.LocalPath))
								if (File.Exists (uri.LocalPath))
									files_al.Add (GetFileFSEntry (new FileInfo (uri.LocalPath)));
						}
					}
					xtr.Close ();
				} catch (Exception) {
					
				}
			}
			
			// KDE
			string full_kde_recent_document_dir = personal_folder + "/.kde/share/apps/RecentDocuments";
			
			if (Directory.Exists (full_kde_recent_document_dir)) {
				string[] files = Directory.GetFiles (full_kde_recent_document_dir, "*.desktop");
				
				foreach (string file_name in files) {
					StreamReader sr = new StreamReader (file_name);
					
					string line = sr.ReadLine ();
					
					while (line != null) {
						line = line.Trim ();
						
						if (line.StartsWith ("URL=")) {
							line = line.Replace ("URL=", "");
							line = line.Replace ("$HOME", personal_folder);
							
							Uri uri = new Uri (line);
							if (!files_al.Contains (uri.LocalPath))
								if (File.Exists (uri.LocalPath))
									files_al.Add (GetFileFSEntry (new FileInfo (uri.LocalPath)));
							break;
						}
						
						line = sr.ReadLine ();
					}
					
					sr.Close ();
				}
			}
			
			
			return files_al;
		}
		
		// return an ArrayList with FSEntry objects
		public override ArrayList GetMyComputerContent ()
		{
			ArrayList my_computer_content_arraylist = new ArrayList ();
			
			if (masterMount.ProcMountAvailable) {
				masterMount.GetMounts ();
				
				foreach (MasterMount.Mount mount in masterMount.Block_devices) {
					FSEntry fsEntry = new FSEntry ();
					fsEntry.FileType = FSEntry.FSEntryType.Device;
					
					fsEntry.FullName = mount.mount_point;
					
					fsEntry.Name = "HDD (" +  mount.fsType + ", " + mount.device_short + ")";
					
					fsEntry.FsType = mount.fsType;
					fsEntry.DeviceShort = mount.device_short;
					
					fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("harddisk/harddisk");
					
					fsEntry.Attributes = FileAttributes.Directory;
					
					fsEntry.MainTopNode = GetMyComputerFSEntry ();
					
					my_computer_content_arraylist.Add (fsEntry);
					
					if (!MWFVFS.MyComputerDevicesPrefix.Contains (fsEntry.FullName + "://"))
						MWFVFS.MyComputerDevicesPrefix.Add (fsEntry.FullName + "://", fsEntry);
				}
				
				foreach (MasterMount.Mount mount in masterMount.Removable_devices) {
					FSEntry fsEntry = new FSEntry ();
					fsEntry.FileType = FSEntry.FSEntryType.RemovableDevice;
					
					fsEntry.FullName = mount.mount_point;
					
					bool is_dvd_cdrom = mount.fsType == MasterMount.FsTypes.usbfs ? false : true;
					string type_name = is_dvd_cdrom ? "DVD/CD-Rom" : "USB";
					string mime_type = is_dvd_cdrom ? "cdrom/cdrom" : "removable/removable";
					
					fsEntry.Name = type_name +" (" + mount.device_short + ")";
					
					fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType (mime_type);
					
					fsEntry.FsType = mount.fsType;
					fsEntry.DeviceShort = mount.device_short;
					
					fsEntry.Attributes = FileAttributes.Directory;
					
					fsEntry.MainTopNode = GetMyComputerFSEntry ();
					
					my_computer_content_arraylist.Add (fsEntry);
					
					string contain_string = fsEntry.FullName + "://";
					if (!MWFVFS.MyComputerDevicesPrefix.Contains (contain_string))
						MWFVFS.MyComputerDevicesPrefix.Add (contain_string, fsEntry);
				}
			}
			
			my_computer_content_arraylist.Add (GetMyComputerPersonalFSEntry ());
			
			return my_computer_content_arraylist;
		}
		
		public override ArrayList GetMyNetworkContent ()
		{
			ArrayList fsEntries = new ArrayList ();
			
			foreach (MasterMount.Mount mount in masterMount.Network_devices) {
				FSEntry fsEntry = new FSEntry ();
				fsEntry.FileType = FSEntry.FSEntryType.Network;
				
				fsEntry.FullName = mount.mount_point;
				
				fsEntry.FsType = mount.fsType;
				fsEntry.DeviceShort = mount.device_short;
				
				fsEntry.Name = "Network (" + mount.fsType + ", " + mount.device_short + ")";
				
				switch (mount.fsType) {
					case MasterMount.FsTypes.nfs:
						fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("nfs/nfs");
						break;
					case MasterMount.FsTypes.smbfs:
						fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("smb/smb");
						break;
					case MasterMount.FsTypes.ncpfs:
						fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("network/network");
						break;
					case MasterMount.FsTypes.cifs:
						fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("network/network");
						break;
					default:
						break;
				}
				
				fsEntry.Attributes = FileAttributes.Directory;
				
				fsEntry.MainTopNode = GetMyNetworkFSEntry ();
				
				fsEntries.Add (fsEntry);
			}
			return fsEntries;
		}
		
		protected override FSEntry GetDesktopFSEntry ()
		{
			return desktopFSEntry;
		}
		
		protected override FSEntry GetRecentlyUsedFSEntry ()
		{
			return recentlyusedFSEntry;
		}
		
		protected override FSEntry GetPersonalFSEntry ()
		{
			return personalFSEntry;
		}
		
		protected override FSEntry GetMyComputerPersonalFSEntry ()
		{
			return mycomputerpersonalFSEntry;
		}
		
		protected override FSEntry GetMyComputerFSEntry ()
		{
			return mycomputerFSEntry;
		}
		
		protected override FSEntry GetMyNetworkFSEntry ()
		{
			return mynetworkFSEntry;
		}
	}
	#endregion
	
	#region WinFileSystem
	internal class WinFileSystem : FileSystem
	{
		private FSEntry desktopFSEntry = null;
		private FSEntry recentlyusedFSEntry = null;
		private FSEntry personalFSEntry = null;
		private FSEntry mycomputerpersonalFSEntry = null;
		private FSEntry mycomputerFSEntry = null;
		private FSEntry mynetworkFSEntry = null;
		
		public WinFileSystem ()
		{
			desktopFSEntry = new FSEntry ();
			
			desktopFSEntry.Attributes = FileAttributes.Directory;
			desktopFSEntry.FullName = MWFVFS.DesktopPrefix;
			desktopFSEntry.Name = "Desktop";
			desktopFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesDesktop);
			desktopFSEntry.FileType = FSEntry.FSEntryType.Directory;
			desktopFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("deskop/desktop");
			desktopFSEntry.LastAccessTime = DateTime.Now;
			
			recentlyusedFSEntry = new FSEntry ();
			
			recentlyusedFSEntry.Attributes = FileAttributes.Directory;
			recentlyusedFSEntry.FullName = MWFVFS.RecentlyUsedPrefix;
			recentlyusedFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesRecentDocuments);
			recentlyusedFSEntry.Name = "Recently Used";
			recentlyusedFSEntry.FileType = FSEntry.FSEntryType.Directory;
			recentlyusedFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("recently/recently");
			recentlyusedFSEntry.LastAccessTime = DateTime.Now;
			
			personalFSEntry = new FSEntry ();
			
			personalFSEntry.Attributes = FileAttributes.Directory;
			personalFSEntry.FullName = MWFVFS.PersonalPrefix;
			personalFSEntry.Name = "Personal";
			personalFSEntry.MainTopNode = GetDesktopFSEntry ();
			personalFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			personalFSEntry.FileType = FSEntry.FSEntryType.Directory;
			personalFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("directory/home");
			personalFSEntry.LastAccessTime = DateTime.Now;
			
			mycomputerpersonalFSEntry = new FSEntry ();
			
			mycomputerpersonalFSEntry.Attributes = FileAttributes.Directory;
			mycomputerpersonalFSEntry.FullName = MWFVFS.MyComputerPersonalPrefix;
			mycomputerpersonalFSEntry.Name = "Personal";
			mycomputerpersonalFSEntry.MainTopNode = GetMyComputerFSEntry ();
			mycomputerpersonalFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			mycomputerpersonalFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mycomputerpersonalFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("directory/home");
			mycomputerpersonalFSEntry.LastAccessTime = DateTime.Now;
			
			mycomputerFSEntry = new FSEntry ();
			
			mycomputerFSEntry.Attributes = FileAttributes.Directory;
			mycomputerFSEntry.FullName = MWFVFS.MyComputerPrefix;
			mycomputerFSEntry.Name = "My Computer";
			mycomputerFSEntry.MainTopNode = GetDesktopFSEntry ();
			mycomputerFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mycomputerFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("workplace/workplace");
			mycomputerFSEntry.LastAccessTime = DateTime.Now;
			
			mynetworkFSEntry = new FSEntry ();
			
			mynetworkFSEntry.Attributes = FileAttributes.Directory;
			mynetworkFSEntry.FullName = MWFVFS.MyNetworkPrefix;
			mynetworkFSEntry.Name = "My Network";
			mynetworkFSEntry.MainTopNode = GetDesktopFSEntry ();
			mynetworkFSEntry.FileType = FSEntry.FSEntryType.Directory;
			mynetworkFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("network/network");
			mynetworkFSEntry.LastAccessTime = DateTime.Now;
		}
		
		public override void WriteRecentlyUsedFiles (string fileToAdd)
		{
			// TODO: Implement this method
			// use SHAddToRecentDocs ?
		}
		
		public override ArrayList GetRecentlyUsedFiles ()
		{
			ArrayList al = new ArrayList ();
			
			DirectoryInfo di = new DirectoryInfo (recentlyusedFSEntry.RealName);
			
			FileInfo[] fileinfos = di.GetFiles ();
			
			foreach (FileInfo fi in fileinfos) {
				al.Add (GetFileFSEntry (fi));
			}
			
			return al;
		}
		
		public override ArrayList GetMyComputerContent ()
		{
			string[] logical_drives = Directory.GetLogicalDrives ();
			
			ArrayList my_computer_content_arraylist = new ArrayList ();
			
			foreach (string drive in logical_drives) {
				FSEntry fsEntry = new FSEntry ();
				fsEntry.FileType = FSEntry.FSEntryType.Device;
				
				fsEntry.FullName = drive;
				
				fsEntry.Name = drive;
				
				fsEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("harddisk/harddisk");
				
				fsEntry.Attributes = FileAttributes.Directory;
				
				fsEntry.MainTopNode = GetMyComputerFSEntry ();
				
				my_computer_content_arraylist.Add (fsEntry);
				
				string contain_string = fsEntry.FullName + "://";
				if (!MWFVFS.MyComputerDevicesPrefix.Contains (contain_string))
					MWFVFS.MyComputerDevicesPrefix.Add (contain_string, fsEntry);
			}
			
			my_computer_content_arraylist.Add (GetMyComputerPersonalFSEntry ());
			
			return my_computer_content_arraylist;
		}
		
		public override ArrayList GetMyNetworkContent ()
		{
			// TODO: Implement this method
			return new ArrayList ();
		}
		protected override FSEntry GetDesktopFSEntry ()
		{
			return desktopFSEntry;
		}
		
		protected override FSEntry GetRecentlyUsedFSEntry ()
		{
			return recentlyusedFSEntry;
		}
		
		protected override FSEntry GetPersonalFSEntry ()
		{
			return personalFSEntry;
		}
		
		protected override FSEntry GetMyComputerPersonalFSEntry ()
		{
			return mycomputerpersonalFSEntry;
		}
		
		protected override FSEntry GetMyComputerFSEntry ()
		{
			return mycomputerFSEntry;
		}
		
		protected override FSEntry GetMyNetworkFSEntry ()
		{
			return mynetworkFSEntry;
		}
	}
	#endregion
	
	#region FSEntry
	internal class FSEntry
	{
		public enum FSEntryType
		{
			Desktop,
			RecentlyUsed,
			MyComputer,
			File,
			Directory,
			Device,
			RemovableDevice,
			Network
		}
		
		private MasterMount.FsTypes fsType;
		private string device_short;
		private string fullName;
		private string name;
		private string realName = null;
		private FileAttributes attributes = FileAttributes.Normal;
		private long fileSize;
		private FSEntryType fileType;
		private DateTime lastAccessTime;
		private FSEntry mainTopNode = null;
		
		private int iconIndex;
		
		private string parent;
		
		public MasterMount.FsTypes FsType {
			set {
				fsType = value;
			}
			
			get {
				return fsType;
			}
		}
		
		public string DeviceShort {
			set {
				device_short = value;
			}
			
			get {
				return device_short;
			}
		}
		
		public string FullName {
			set {
				fullName = value;
			}
			
			get {
				return fullName;
			}
		}
		
		public string Name {
			set {
				name = value;
			}
			
			get {
				return name;
			}
		}
		
		public string RealName {
			set {
				realName = value;
			}
			
			get {
				return realName;
			}
		}
		
		public FileAttributes Attributes {
			set {
				attributes = value;
			}
			
			get {
				return attributes;
			}
		}
		
		public long FileSize {
			set {
				fileSize = value;
			}
			
			get {
				return fileSize;
			}
		}
		
		public FSEntryType FileType {
			set {
				fileType = value;
			}
			
			get {
				return fileType;
			}
		}
		
		public DateTime LastAccessTime {
			set {
				lastAccessTime = value;
			}
			
			get {
				return lastAccessTime;
			}
		}
		
		public int IconIndex {
			set {
				iconIndex = value;
			}
			
			get {
				return iconIndex;
			}
		}
		
		public FSEntry MainTopNode {
			set {
				mainTopNode = value;
			}
			
			get {
				return mainTopNode;
			}
		}
		
		public string Parent {
			set {
				parent = value;
			}
			
			get {
				parent = GetParent ();
				
				return parent;
			}
		}
		
		private string GetParent ()
		{
			if (fullName == MWFVFS.PersonalPrefix) {
				return MWFVFS.DesktopPrefix;
			} else
			if (fullName == MWFVFS.MyComputerPersonalPrefix) {
				return MWFVFS.MyComputerPrefix;
			} else
			if (fullName == MWFVFS.MyComputerPrefix) {
				return MWFVFS.DesktopPrefix;
			} else
			if (fullName == MWFVFS.MyNetworkPrefix) {
				return MWFVFS.DesktopPrefix;
			} else
			if (fullName == MWFVFS.DesktopPrefix) {
				return null;
			} else
			if (fullName == MWFVFS.RecentlyUsedPrefix) {
				return null;
			} else {
				foreach (DictionaryEntry entry in MWFVFS.MyComputerDevicesPrefix) {
					FSEntry fsEntry = entry.Value as FSEntry;
					if (fullName == fsEntry.FullName) {
						return fsEntry.MainTopNode.FullName;
					}
				}
				
				DirectoryInfo dirInfo = new DirectoryInfo (fullName);
				
				DirectoryInfo dirInfoParent = dirInfo.Parent;
				
				if (dirInfoParent != null) {
					FSEntry fsEntry = MWFVFS.MyComputerDevicesPrefix [dirInfoParent.FullName + "://"] as FSEntry;
					
					if (fsEntry != null) {
						return fsEntry.FullName;
					}
					
					if (mainTopNode != null) {
						if (dirInfoParent.FullName == ThemeEngine.Current.Places (UIIcon.PlacesDesktop) &&
						    mainTopNode.FullName == MWFVFS.DesktopPrefix) {
							return mainTopNode.FullName;
						} else
						if (dirInfoParent.FullName == ThemeEngine.Current.Places (UIIcon.PlacesPersonal) &&
						    mainTopNode.FullName == MWFVFS.PersonalPrefix) {
							return mainTopNode.FullName;
						} else
						if (dirInfoParent.FullName == ThemeEngine.Current.Places (UIIcon.PlacesPersonal) &&
						    mainTopNode.FullName == MWFVFS.MyComputerPersonalPrefix) {
							return mainTopNode.FullName;
						}
					}
					
					return dirInfoParent.FullName;
				}
			}
			
			return null;
		}
	}
	#endregion
	
	#region MasterMount
	// Alexsantas little *nix helper
	internal class MasterMount
	{
		// add more...
		internal enum FsTypes
		{
			none,
			ext2,
			ext3,
			hpfs,
			iso9660,
			jfs,
			minix,
			msdos,
			ntfs,
			reiserfs,
			ufs,
			umsdos,
			vfat,
			sysv,
			xfs,
			ncpfs,
			nfs,
			smbfs,
			usbfs,
			cifs
		}
		
		internal struct Mount
		{
			public string device_or_filesystem;
			public string device_short;
			public string mount_point;
			public FsTypes fsType;
		}
		
		bool proc_mount_available = false;
		
		ArrayList block_devices = new ArrayList ();
		ArrayList network_devices = new ArrayList ();
		ArrayList removable_devices = new ArrayList ();
		
		private int platform = (int) Environment.OSVersion.Platform;
		private MountComparer mountComparer = new MountComparer ();
		
		public MasterMount ()
		{
			// maybe check if the current user can access /proc/mounts
			if ((platform == 4) || (platform == 128))
				if (File.Exists ("/proc/mounts"))
					proc_mount_available = true;
		}
		
		public ArrayList Block_devices {
			get {
				return block_devices;
			}
		}
		
		public ArrayList Network_devices {
			get {
				return network_devices;
			}
		}
		
		public ArrayList Removable_devices {
			get {
				return removable_devices;
			}
		}
		
		public bool ProcMountAvailable {
			get {
				return proc_mount_available;
			}
		}
		
		public void GetMounts ()
		{
			if (!proc_mount_available)
				return;
			
			block_devices.Clear ();
			network_devices.Clear ();
			removable_devices.Clear ();
			
			try {
				StreamReader sr = new StreamReader ("/proc/mounts");
				
				string line = sr.ReadLine ();
				
				ArrayList lines = new ArrayList ();
 				while (line != null) {
					if (lines.IndexOf (line) == -1) { // Avoid duplicates
						ProcessProcMountLine (line);
						lines.Add (line);
					}
 					line = sr.ReadLine ();
 				}
				
				sr.Close ();
				
				block_devices.Sort (mountComparer);
				network_devices.Sort (mountComparer);
				removable_devices.Sort (mountComparer);
			} catch {
				// bla
			}
		}
		
		private void ProcessProcMountLine (string line)
		{
			string[] split = line.Split (new char [] {' '});
			
			if (split != null && split.Length > 0) {
				Mount mount = new Mount ();
				
				if (split [0].StartsWith ("/dev/"))
					mount.device_short = split [0].Replace ("/dev/", "");
				else 
					mount.device_short = split [0];
				
				mount.device_or_filesystem = split [0];
				mount.mount_point = split [1];
				
				// TODO: other removable devices, floppy
				// ssh
				
				// network mount
				if (split [2] == "nfs") {
					mount.fsType = FsTypes.nfs;
					network_devices.Add (mount);
				} else if (split [2] == "smbfs") {
					mount.fsType = FsTypes.smbfs;
					network_devices.Add (mount);
				} else if (split [2] == "cifs") {
					mount.fsType = FsTypes.cifs;
					network_devices.Add (mount);
				} else if (split [2] == "ncpfs") {
					mount.fsType = FsTypes.ncpfs;
					network_devices.Add (mount);
					
				} else if (split [2] == "iso9660") { //cdrom
					mount.fsType = FsTypes.iso9660;
					removable_devices.Add (mount);
				} else if (split [2] == "usbfs") { //usb ? not tested
					mount.fsType = FsTypes.usbfs;
					removable_devices.Add (mount);
					
				} else if (split [0].StartsWith ("/")) { //block devices
					if (split [1].StartsWith ("/dev/"))  // root static, do not add
						return;
					
					if (split [2] == "ext2")
						mount.fsType = FsTypes.ext2;
					else if (split [2] == "ext3")
						mount.fsType = FsTypes.ext3;
					else if (split [2] == "reiserfs")
						mount.fsType = FsTypes.reiserfs;
					else if (split [2] == "xfs")
						mount.fsType = FsTypes.xfs;
					else if (split [2] == "vfat")
						mount.fsType = FsTypes.vfat;
					else if (split [2] == "ntfs")
						mount.fsType = FsTypes.ntfs;
					else if (split [2] == "msdos")
						mount.fsType = FsTypes.msdos;
					else if (split [2] == "umsdos")
						mount.fsType = FsTypes.umsdos;
					else if (split [2] == "hpfs")
						mount.fsType = FsTypes.hpfs;
					else if (split [2] == "minix")
						mount.fsType = FsTypes.minix;
					else if (split [2] == "jfs")
						mount.fsType = FsTypes.jfs;
					
					block_devices.Add (mount);
				}
			}
		}
		
		public class MountComparer : IComparer
		{
			public int Compare (object mount1, object mount2)
			{
				return String.Compare (((Mount)mount1).device_short, ((Mount)mount2).device_short);
			}
		}
	}
	#endregion
}


