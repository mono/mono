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
// Copyright (c) 2006 Alexander Olk
//
// Authors:
//
//  Alexander Olk	alex.olk@googlemail.com
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
using System.Threading;

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
		private string defaultExt = String.Empty;
		private bool dereferenceLinks = true;
		private string fileName = String.Empty;
		private string[] fileNames;
		private string filter = "";
		private int filterIndex = 1;
		private int setFilterIndex = 1;
		private string initialDirectory = String.Empty;
		private bool restoreDirectory = false;
		private bool showHelp = false;
		private string title = String.Empty;
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
		
		private string restoreDirectoryString = String.Empty;
		
		internal FileDialogType fileDialogType;
		
		private bool do_not_call_OnSelectedIndexChangedFileTypeComboBox = false;
		
		private bool showReadOnly = false;
		private bool readOnlyChecked = false;
		internal bool createPrompt = false;
		internal bool overwritePrompt = true;
		
		internal FileFilter fileFilter;
		
		private string lastFolder = String.Empty;
		
		private MWFVFS vfs;
		
		private readonly string filedialog_string = "FileDialog";
		private readonly string lastfolder_string = "LastFolder";
		private readonly string width_string = "Width";
		private readonly string height_string = "Height";
		private readonly string filenames_string = "FileNames";
		private readonly string x_string = "X";
		private readonly string y_string = "Y";
		
		internal FileDialog ()
		{
			vfs = new MWFVFS ();
			
			Size formConfigSize = Size.Empty;
			Point formConfigLocation = Point.Empty;
			string[] configFileNames = null;
			
			object formWidth = MWFConfig.GetValue (filedialog_string, width_string);
			
			object formHeight = MWFConfig.GetValue (filedialog_string, height_string);
			
			if (formHeight != null && formWidth != null)
				formConfigSize = new Size ((int)formWidth, (int)formHeight);
			
			object formLocationX = MWFConfig.GetValue (filedialog_string, x_string);
			object formLocationY = MWFConfig.GetValue (filedialog_string, y_string);
			
			if (formLocationX != null && formLocationY != null)
				formConfigLocation = new Point ((int)formLocationX, (int)formLocationY);
			
			configFileNames = (string[])MWFConfig.GetValue (filedialog_string, filenames_string);
			
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
			dirComboBox.TabIndex = 7;
			
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
			smallButtonToolBar.TabIndex = 8;
			smallButtonToolBar.TextAlign = ToolBarTextAlign.Right;
			
			// buttonPanel
			popupButtonPanel.Dock = DockStyle.None;
			popupButtonPanel.Location = new Point (7, 37);
			popupButtonPanel.TabIndex = 9;
			
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
			mwfFileView.TabIndex = 10;
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
			fileNameComboBox.TabIndex = 1;
			fileNameComboBox.MaxDropDownItems = 10;
			fileNameComboBox.Items.Add (" ");
			
			if (configFileNames != null) {
				fileNameComboBox.Items.Clear ();
				
				foreach (string configFileName in configFileNames) {
					if (configFileName != null)
						if (configFileName.Trim ().Length > 0)
							fileNameComboBox.Items.Add (configFileName);
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
			fileTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			fileTypeComboBox.Location = new Point (195, 356);
			fileTypeComboBox.Size = new Size (245, 21);
			fileTypeComboBox.TabIndex = 2;
			
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
			openSaveButton.TabIndex = 4;
			openSaveButton.FlatStyle = FlatStyle.System;
			
			// cancelButton
			cancelButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			cancelButton.FlatStyle = FlatStyle.System;
			cancelButton.Location = new Point (475, 356);
			cancelButton.Size = new Size (72, 21);
			cancelButton.TabIndex = 5;
			cancelButton.Text = "Cancel";
			cancelButton.FlatStyle = FlatStyle.System;
			
			// helpButton
			helpButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			helpButton.FlatStyle = FlatStyle.System;
			helpButton.Location = new Point (475, 350);
			helpButton.Size = new Size (72, 21);
			helpButton.TabIndex = 6;
			helpButton.Text = "Help";
			helpButton.FlatStyle = FlatStyle.System;
			
			// checkBox
			readonlyCheckBox.Anchor = ((AnchorStyles)(((AnchorStyles.Bottom | AnchorStyles.Left) | AnchorStyles.Right)));
			readonlyCheckBox.Text = "Open Readonly";
			readonlyCheckBox.Location = new Point (195, 350);
			readonlyCheckBox.Size = new Size (245, 21);
			readonlyCheckBox.TabIndex = 3;
			readonlyCheckBox.FlatStyle = FlatStyle.System;
			
			form.SizeGripStyle = SizeGripStyle.Show;
			
			form.MaximizeBox = true;
			form.MinimizeBox = true;
			form.FormBorderStyle = FormBorderStyle.Sizable;
			form.MinimumSize = new Size (554, 405);
			
			form.ClientSize =  new Size (554, 405); // 384
			
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
			
			if (formConfigSize != Size.Empty) {
				form.Size = formConfigSize;
			}
			
			if (formConfigLocation != Point.Empty) {
				form.Location = formConfigLocation;
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
#if NET_2_0
			form.FormClosed += new FormClosedEventHandler (OnFileDialogFormClosed);
#else
			form.Closed += new EventHandler (OnClickCancelButton);
#endif
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
					if (SetFileName (value)) {
						fileName = value;
						if (fileNames == null) {
							fileNames = new string [1];
							fileNames [0] = value;
						}
					}
				} else {
					fileName = String.Empty;
					fileNames = new string [0];
					// this does not match ms exactly,
					// but there is no other way to clear that %#&?§ combobox
					fileNameComboBox.Text = " ";
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
				if (value == null) {
					filter = "";
					if (fileFilter != null)
						fileFilter.FilterArrayList.Clear ();
				} else {
					if (FileFilter.CheckFilter (value)) {
						filter = value;
						
						fileFilter = new FileFilter (filter);
					} else
						throw new ArgumentException ();
				}
				
				UpdateFilters ();
			}
		}
		
		[DefaultValue(1)]
		public int FilterIndex {
			get {
				return setFilterIndex;
			}
			
			set {
				setFilterIndex = value;
				
				if (value < 1)
					filterIndex = 1;
				else
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
			defaultExt = String.Empty;
			dereferenceLinks = true;
			FileName = String.Empty;
			Filter = String.Empty;
			FilterIndex = 1;
			initialDirectory = String.Empty;
			restoreDirectory = false;
			ShowHelp = false;
			Title = String.Empty;
			validateNames = true;
			
			UpdateFilters ();
		}
		
		public override string ToString ()
		{
			return String.Format("{0}: Title: {1}, FileName: {2}", base.ToString (), Title, fileName);
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
			WriteConfigValues (e);
			
			CancelEventHandler fo = (CancelEventHandler) Events [EventFileOk];
			if (fo != null)
				fo (this, e);
			
			Mime.CleanFileCache ();
		}
		
		protected  override bool RunDialog (IntPtr hWndOwner)
		{
			ReadConfigValues ();
			
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
			if (mwfFileView.FilterArrayList == null || filterIndex > mwfFileView.FilterArrayList.Count)
				return;
			
			do_not_call_OnSelectedIndexChangedFileTypeComboBox = true;
			fileTypeComboBox.BeginUpdate ();
			fileTypeComboBox.SelectedIndex = filterIndex - 1;
			fileTypeComboBox.EndUpdate ();
			
			mwfFileView.FilterIndex = filterIndex;
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
			
			string internalfullfilename = String.Empty;
			
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
						string extension_to_use = String.Empty;
						string filter_exentsion = String.Empty;
						
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
						
						if (filter_exentsion != String.Empty)
							extension_to_use = filter_exentsion;
						else
						if (defaultExt != String.Empty)
							extension_to_use = "." + defaultExt;
						
						if (!internalfullfilename.EndsWith (extension_to_use))
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
			//
			// This method gets called whenever the dialog is closed, so we need
			// to make sure that we aren't changing the dialog result when the
			// OK (Open or Save) button has been clicked and we are closing the 
			// window ourselves.
			//
			if (form.DialogResult == DialogResult.OK)
				return;

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

#if NET_2_0		
		void OnFileDialogFormClosed (object sender, EventArgs e)
		{
			OnClickCancelButton (sender, EventArgs.Empty);
		}
#endif
		
		private void UpdateFilters ()
		{
			if (fileFilter == null)
				fileFilter = new FileFilter ();
			
			ArrayList filters = fileFilter.FilterArrayList;
			
			fileTypeComboBox.BeginUpdate ();
			
			fileTypeComboBox.Items.Clear ();
			
			foreach (FilterStruct fs in filters) {
				fileTypeComboBox.Items.Add (fs.filterName);
			}
			
			if (filters.Count > 0 && FilterIndex <= filters.Count)
				fileTypeComboBox.SelectedIndex = filterIndex - 1;
			
			fileTypeComboBox.EndUpdate ();
			
			mwfFileView.FilterArrayList = filters;
			
			mwfFileView.FilterIndex = filterIndex;
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
		
		private void WriteConfigValues (CancelEventArgs ce)
		{
			MWFConfig.SetValue (filedialog_string, width_string, form.Width);
			MWFConfig.SetValue (filedialog_string, height_string, form.Height);
			MWFConfig.SetValue (filedialog_string, x_string, form.Location.X);
			MWFConfig.SetValue (filedialog_string, y_string, form.Location.Y);
			
			if (!ce.Cancel) {
				MWFConfig.SetValue (filedialog_string, lastfolder_string, lastFolder);
				
				string[] fileNameCBItems = new string [fileNameComboBox.Items.Count];
				
				fileNameComboBox.Items.CopyTo (fileNameCBItems, 0);
				
				MWFConfig.SetValue (filedialog_string, filenames_string, fileNameCBItems);
			}
		}
		
		private void ReadConfigValues ()
		{
			lastFolder = (string)MWFConfig.GetValue (filedialog_string, lastfolder_string);
			
			if (lastFolder != null && lastFolder.IndexOf ("://") == -1) {
				if (!Directory.Exists (lastFolder)) {
					lastFolder = MWFVFS.DesktopPrefix;
				}
			}
			
			if (initialDirectory != String.Empty)
				lastFolder = initialDirectory;
			else
			if (lastFolder == null || lastFolder == String.Empty)
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
					Invalidate ();
				}
				
				get {
					return image;
				}
			}
			
			public PopupButtonState ButtonState {
				set {
					popupButtonState = value;
					Invalidate ();
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
					
					gr.DrawString (Text, Font, Brushes.White, text_rect, text_format);
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
				Invalidate ();
				base.OnMouseEnter (e);
			}
			
			protected override void OnMouseLeave (EventArgs e)
			{
				if (popupButtonState == PopupButtonState.Up)
					popupButtonState = PopupButtonState.Normal;
				Invalidate ();
				base.OnMouseLeave (e);
			}
			
			protected override void OnClick (EventArgs e)
			{
				popupButtonState = PopupButtonState.Down;
				Invalidate ();
				base.OnClick (e);
			}
		}
		#endregion
		
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
			
			BackColor = Color.FromArgb (128, 128, 128);
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
			
			EventHandler eh = (EventHandler)(Events [PDirectoryChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
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
		
		static object PDirectoryChangedEvent = new object ();
		
		public event EventHandler DirectoryChanged {
			add { Events.AddHandler (PDirectoryChangedEvent, value); }
			remove { Events.RemoveHandler (PDirectoryChangedEvent, value); }
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
			this.vfs = vfs;

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
			
			ArrayList al = this.vfs.GetMyComputerContent ();
			
			foreach (FSEntry fsEntry in al) {
				myComputerItems.Add (new DirComboBoxItem (MimeIconEngine.LargeIcons, fsEntry.IconIndex, fsEntry.Name, fsEntry.FullName, indent * 2));
			}
			
			al.Clear ();
			al = null;
			
			mainParentDirComboBoxItem = myComputerDirComboboxItem;
			
			ResumeLayout (false);
		}
		
		public string CurrentFolder {
			set {
				currentPath = value;
				
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

			gr.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (backColor),
					new Rectangle (0, 0, bmp.Width, bmp.Height));
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected &&
					(!DroppedDown || (e.State & DrawItemState.ComboBoxEdit) != DrawItemState.ComboBoxEdit)) {
				foreColor = ThemeEngine.Current.ColorHighlightText;

				int w = (int) gr.MeasureString (dcbi.Name, e.Font).Width;

				gr.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHighlight),
						new Rectangle (xPos + 23, 1, w + 3, e.Bounds.Height - 2));
				if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
					ControlPaint.DrawFocusRectangle (gr, new Rectangle (xPos + 22, 0, w + 5,
							e.Bounds.Height), foreColor, ThemeEngine.Current.ColorHighlight);
				}
			}

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
			}
		}
		
		protected override void OnSelectionChangeCommitted (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CDirectoryChangedEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		
		static object CDirectoryChangedEvent = new object ();
		
		public event EventHandler DirectoryChanged {
			add { Events.AddHandler (CDirectoryChangedEvent, value); }
			remove { Events.RemoveHandler (CDirectoryChangedEvent, value); }
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
			foreach (string s in split) {
				filters.Add (s.Trim ());
			}
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
		
		public FileFilter (string filter) : base ()
		{
			this.filter = filter;
			
			SplitFilter ();
		}
		
		public static bool CheckFilter (string val)
		{
			if (val.Length == 0)
				return true;
			
			string[] filters = val.Split (new char [] {'|'});
			
			if ((filters.Length % 2) != 0)
				return false;
			
			return true;
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
			
			if (filter.Length == 0)
				return;
			
			string[] filters = filter.Split (new char [] {'|'});
			
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
		
		private int platform = (int) Environment.OSVersion.Platform;
		
		public MWFFileView (MWFVFS vfs)
		{
			this.vfs = vfs;
			this.vfs.RegisterUpdateDelegate (new MWFVFS.UpdateDelegate (RealFileViewUpdate), this);
			
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
					UpdateFileView ();
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
			if (currentFolder == MWFVFS.MyComputerPrefix ||
			    currentFolder == MWFVFS.RecentlyUsedPrefix)
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
			
			string folder = String.Empty;
			
			if (currentFolderFSEntry.RealName != null)
				folder = currentFolderFSEntry.RealName;
			else
				folder = currentFolder;
			
			string tmp_filename = "New Folder";
			
			if (Directory.Exists (Path.Combine (folder, tmp_filename))) {
				int i = 1;
				
				if ((platform == 4) || (platform == 128)) {
					tmp_filename = tmp_filename + "-" + i;
				} else {
					tmp_filename = tmp_filename + " (" + i + ")";
				}
				
				while (Directory.Exists (Path.Combine (folder, tmp_filename))) {
					i++;
					if ((platform == 4) || (platform == 128)) {
						tmp_filename = "New Folder" + "-" + i;
					} else {
						tmp_filename = "New Folder" + " (" + i + ")";
					}
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

			try {
				UpdateFileView ();
			} catch (Exception e) {
				if (should_push)
					PopDir ();
				MessageBox.Show (e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		public void UpdateFileView ()
		{
			if (filterArrayList != null && filterArrayList.Count != 0) {
				FilterStruct fs = (FilterStruct)filterArrayList [filterIndex - 1];
				
				vfs.GetFolderContent (fs.filters);
			} else
				vfs.GetFolderContent ();
		}
		
		public void RealFileViewUpdate (ArrayList directoriesArrayList, ArrayList fileArrayList)
		{
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
			
			collection.Clear ();
			collection = null;
			
			directoriesArrayList.Clear ();
			fileArrayList.Clear ();
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
					t.Enabled = (directoryStack.Count > 0);
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
						
						EventHandler eh = (EventHandler)(Events [MSelectedFileChangedEvent]);
						if (eh != null)
							eh (this, EventArgs.Empty);
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
					
					EventHandler eh = (EventHandler)(Events [MDirectoryChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				} else {
					currentFSEntry = fsEntry;
					
					EventHandler eh = (EventHandler)(Events [MSelectedFileChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
					
					eh = (EventHandler)(Events [MForceDialogEndEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
					
					return;
				}
			}
			
			base.OnDoubleClick (e);
		}
		
		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			if (SelectedItems.Count > 0) {
				selectedFilesString = String.Empty;
				
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
				
				EventHandler eh = (EventHandler)(Events [MSelectedFilesChangedEvent]);
				if (eh != null)
					eh (this, EventArgs.Empty);
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
			} else
				toolTip.Active = false;
			
			base.OnMouseMove (e);
		}
		
		void OnClickContextMenu (object sender, EventArgs e)
		{
			MenuItem senderMenuItem = sender as MenuItem;
			
			if (senderMenuItem == showHiddenFilesMenuItem) {
				senderMenuItem.Checked = !senderMenuItem.Checked;
				showHiddenFiles = senderMenuItem.Checked;
				UpdateFileView ();
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
		
		static object MSelectedFileChangedEvent = new object ();
		static object MSelectedFilesChangedEvent = new object ();
		static object MDirectoryChangedEvent = new object ();
		static object MForceDialogEndEvent = new object ();
		
		public event EventHandler SelectedFileChanged {
			add { Events.AddHandler (MSelectedFileChangedEvent, value); }
			remove { Events.RemoveHandler (MSelectedFileChangedEvent, value); }
		}
		
		public event EventHandler SelectedFilesChanged {
			add { Events.AddHandler (MSelectedFilesChangedEvent, value); }
			remove { Events.RemoveHandler (MSelectedFilesChangedEvent, value); }
		}
		
		public event EventHandler DirectoryChanged {
			add { Events.AddHandler (MDirectoryChangedEvent, value); }
			remove { Events.RemoveHandler (MDirectoryChangedEvent, value); }
		}
		
		public event EventHandler ForceDialogEnd {
			add { Events.AddHandler (MForceDialogEndEvent, value); }
			remove { Events.RemoveHandler (MForceDialogEndEvent, value); }
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
					SubItems.Add (String.Empty);
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
					SubItems.Add (String.Empty);
					SubItems.Add ("Device");
					SubItems.Add (fsEntry.LastAccessTime.ToShortDateString () + " " + fsEntry.LastAccessTime.ToShortTimeString ());	
					break;
				case FSEntry.FSEntryType.RemovableDevice:
					SubItems.Add (String.Empty);
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
			newNameTextBox.Text = String.Empty;
			
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
		
		public delegate void UpdateDelegate (ArrayList folders, ArrayList files);
		private UpdateDelegate updateDelegate;
		private Control calling_control;
		
		private ThreadStart get_folder_content_thread_start;
		private Thread worker;
		private WorkerThread workerThread = null;
		private StringCollection the_filters;
		
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
		
		public void GetFolderContent ()
		{
			GetFolderContent (null);
		}
		
		public void GetFolderContent (StringCollection filters)
		{
			the_filters = filters;

			if (workerThread != null) {
				workerThread.Stop ();
				workerThread = null;
			}

			// Added next line to ensure the control is created before BeginInvoke is called on it
			calling_control.CreateControl();
			workerThread = new WorkerThread (fileSystem, the_filters, updateDelegate, calling_control);
			
			get_folder_content_thread_start = new ThreadStart (workerThread.GetFolderContentThread);
			worker = new Thread (get_folder_content_thread_start);
			worker.IsBackground = true;
			worker.Start();
		}
		
		internal class WorkerThread
		{
			private FileSystem fileSystem;
			private StringCollection the_filters;
			private UpdateDelegate updateDelegate;
			private Control calling_control;
			private readonly object lockobject = new object ();
			private bool stopped = false;
			
			public WorkerThread (FileSystem fileSystem, StringCollection the_filters, UpdateDelegate updateDelegate, Control calling_control)
			{
				this.fileSystem = fileSystem;
				this.the_filters = the_filters;
				this.updateDelegate = updateDelegate;
				this.calling_control = calling_control;
			}
			
			public void GetFolderContentThread()
			{
				ArrayList folders;
				ArrayList files;
				
				fileSystem.GetFolderContent (the_filters, out folders, out files);
				
				if (stopped)
					return;
				
				if (updateDelegate != null) {
					lock (this) {
						object[] objectArray = new object[2];
						
						objectArray[0] = folders;
						objectArray[1] = files;
						
						calling_control.BeginInvoke (updateDelegate, objectArray);
					}
				}
			}
			
			public void Stop ()
			{
				lock (lockobject) {
					stopped = true;
				}
			}
		}
		
		public ArrayList GetFoldersOnly ()
		{
			return fileSystem.GetFoldersOnly ();
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
			} catch (Exception e) {
				MessageBox.Show (e.Message, new_folder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			
			return true;
		}
		
		public string GetParent ()
		{
			return fileSystem.GetParent ();
		}
		
		public void RegisterUpdateDelegate(UpdateDelegate updateDelegate, Control control)
		{
			this.updateDelegate = updateDelegate;
			calling_control = control;
		}
	}
	#endregion
	
	#region FileSystem
	internal abstract class FileSystem
	{
		protected string currentTopFolder = String.Empty;
		protected FSEntry currentFolderFSEntry = null;
		protected FSEntry currentTopFolderFSEntry = null;
		private FileInfoComparer fileInfoComparer = new FileInfoComparer ();
		
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
				
				ArrayList d_out = null;
				ArrayList f_out = null;
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
				ArrayList d_out = null;
				ArrayList f_out = null;
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
		
		public ArrayList GetFoldersOnly ()
		{
			ArrayList directories_out = new ArrayList ();
			
			if (currentFolderFSEntry.FullName == MWFVFS.DesktopPrefix) {
				FSEntry personalFSEntry = GetPersonalFSEntry ();
				
				directories_out.Add (personalFSEntry);
				
				FSEntry myComputerFSEntry = GetMyComputerFSEntry ();
				
				directories_out.Add (myComputerFSEntry);
				
				FSEntry myNetworkFSEntry = GetMyNetworkFSEntry ();
				
				directories_out.Add (myNetworkFSEntry);
				
				ArrayList d_out = GetNormalFolders (ThemeEngine.Current.Places (UIIcon.PlacesDesktop));
				directories_out.AddRange (d_out);
				
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.RecentlyUsedPrefix) {
				//files_out = GetRecentlyUsedFiles ();
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.MyComputerPrefix) {
				directories_out.AddRange (GetMyComputerContent ());
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.PersonalPrefix || currentFolderFSEntry.FullName == MWFVFS.MyComputerPersonalPrefix) {
				ArrayList d_out = GetNormalFolders (ThemeEngine.Current.Places (UIIcon.PlacesPersonal));
				directories_out.AddRange (d_out);
			} else
			if (currentFolderFSEntry.FullName == MWFVFS.MyNetworkPrefix) {
				directories_out.AddRange (GetMyNetworkContent ());
			} else {
				directories_out = GetNormalFolders (currentFolderFSEntry.FullName);
			}
			return directories_out;
		}
		
		protected void GetNormalFolderContent (string from_folder, StringCollection filters, out ArrayList directories_out, out ArrayList files_out)
		{
			DirectoryInfo dirinfo = new DirectoryInfo (from_folder);
			
			directories_out = new ArrayList ();
			
			DirectoryInfo[] dirs = null;
			
			try {
				dirs = dirinfo.GetDirectories ();
			} catch (Exception) {}
			
			if (dirs != null)
				for (int i = 0; i < dirs.Length; i++) {
					directories_out.Add (GetDirectoryFSEntry (dirs [i], currentTopFolderFSEntry));
				}
			
			files_out = new ArrayList ();
			
			ArrayList files = new ArrayList ();
			
			try {
				if (filters == null) {
					files.AddRange (dirinfo.GetFiles ());
				} else {
					foreach (string s in filters)
						files.AddRange (dirinfo.GetFiles (s));
					
					files.Sort (fileInfoComparer);
				}
			} catch (Exception) {}
			
			for (int i = 0; i < files.Count; i++) {
				FSEntry fs = GetFileFSEntry (files [i] as FileInfo);
				if (fs != null)
					files_out.Add (fs);
			}
		}
		
		protected ArrayList GetNormalFolders (string from_folder)
		{
			DirectoryInfo dirinfo = new DirectoryInfo (from_folder);
			
			ArrayList directories_out = new ArrayList ();
			
			DirectoryInfo[] dirs = null;
			
			try {
				dirs = dirinfo.GetDirectories ();
			} catch (Exception) {}
			
			if (dirs != null)
				for (int i = 0; i < dirs.Length; i++) {
					directories_out.Add (GetDirectoryFSEntry (dirs [i], currentTopFolderFSEntry));
				}
			
			return directories_out;
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
			// *sigh* FileInfo gives us no usable information for links to directories
			// so, return null
			if ((fileinfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
				return null;
			
			FSEntry fs = new FSEntry ();
			
			fs.Attributes = fileinfo.Attributes;
			fs.FullName = fileinfo.FullName;
			fs.Name = fileinfo.Name;
			fs.FileType = FSEntry.FSEntryType.File;
			fs.IconIndex = MimeIconEngine.GetIconIndexForFile (fileinfo.FullName);
			fs.FileSize = fileinfo.Length;
			fs.LastAccessTime = fileinfo.LastAccessTime;
			
			return fs;
		}
		
		internal class FileInfoComparer : IComparer
		{
			public int Compare (object fileInfo1, object fileInfo2)
			{
				return String.Compare (((FileInfo)fileInfo1).Name, ((FileInfo)fileInfo2).Name);
			}
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
		
		private string personal_folder;
		private string recently_used_path;
		private string full_kde_recent_document_dir;
		
		public UnixFileSystem ()
		{
			personal_folder = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
			recently_used_path = Path.Combine (personal_folder, ".recently-used");
			
			full_kde_recent_document_dir = personal_folder + "/.kde/share/apps/RecentDocuments";
			
			desktopFSEntry = new FSEntry ();
			
			desktopFSEntry.Attributes = FileAttributes.Directory;
			desktopFSEntry.FullName = MWFVFS.DesktopPrefix;
			desktopFSEntry.Name = "Desktop";
			desktopFSEntry.RealName = ThemeEngine.Current.Places (UIIcon.PlacesDesktop);
			desktopFSEntry.FileType = FSEntry.FSEntryType.Directory;
			desktopFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("desktop/desktop");
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
				xml_doc.AppendChild (xml_doc.CreateXmlDeclaration ("1.0", String.Empty, String.Empty));
				
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
								if (File.Exists (uri.LocalPath)) {
									FSEntry fs = GetFileFSEntry (new FileInfo (uri.LocalPath));
									if (fs != null)
										files_al.Add (fs);
								}
						}
					}
					xtr.Close ();
				} catch (Exception) {
					
				}
			}
			
			// KDE
			if (Directory.Exists (full_kde_recent_document_dir)) {
				string[] files = Directory.GetFiles (full_kde_recent_document_dir, "*.desktop");
				
				foreach (string file_name in files) {
					StreamReader sr = new StreamReader (file_name);
					
					string line = sr.ReadLine ();
					
					while (line != null) {
						line = line.Trim ();
						
						if (line.StartsWith ("URL=")) {
							line = line.Replace ("URL=", String.Empty);
							line = line.Replace ("$HOME", personal_folder);
							
							Uri uri = new Uri (line);
							if (!files_al.Contains (uri.LocalPath))
								if (File.Exists (uri.LocalPath)) {
									FSEntry fs = GetFileFSEntry (new FileInfo (uri.LocalPath));
									if (fs != null)
										files_al.Add (fs);
								}
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
			desktopFSEntry.IconIndex = MimeIconEngine.GetIconIndexForMimeType ("desktop/desktop");
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
				FSEntry fs = GetFileFSEntry (fi);
				if (fs != null)
					al.Add (fs);
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
					mount.device_short = split [0].Replace ("/dev/", String.Empty);
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
		
	#region MWFConfig
	// easy to use class to store and read internal MWF config settings.
	// the config values are stored in the users home dir as a hidden xml file called "mwf_config".
	// currently supports int, string, byte, color and arrays (including byte arrays)
	// don't forget, when you read a value you still have to cast this value.
	//
	// usage:
	// MWFConfig.SetValue ("SomeClass", "What", value);
	// object o = MWFConfig.GetValue ("SomeClass", "What");
	//
	// example:
	// 
	// string[] configFileNames = (string[])MWFConfig.GetValue ("FileDialog", "FileNames");
	// MWFConfig.SetValue ("FileDialog", "LastFolder", "/home/user");
	
	internal class MWFConfig
	{
		private static MWFConfigInstance Instance = new MWFConfigInstance ();
		
		private static object lock_object = new object();
		
		public static object GetValue (string class_name, string value_name)
		{
			lock (lock_object) {
				return Instance.GetValue (class_name, value_name);
			}
		}
		
		public static void SetValue (string class_name, string value_name, object value)
		{
			lock (lock_object) {
				Instance.SetValue (class_name, value_name, value);
			}
		}
		
		public static void Flush ()
		{
			lock (lock_object) {
				Instance.Flush ();
			}
		}
		
		public static void RemoveClass (string class_name)
		{
			lock (lock_object) {
				Instance.RemoveClass (class_name);
			}
		}
		
		public static void RemoveClassValue (string class_name, string value_name)
		{
			lock (lock_object) {
				Instance.RemoveClassValue (class_name, value_name);
			}
		}
		
		public static void RemoveAllClassValues (string class_name)
		{
			lock (lock_object) {
				Instance.RemoveAllClassValues (class_name);
			}
		}
	
		internal class MWFConfigInstance
		{
			Hashtable classes_hashtable = new Hashtable ();
			static string full_file_name;
			static string default_file_name;
			readonly string configName = "MWFConfig";
			static int platform = (int) Environment.OSVersion.Platform;

			static bool IsUnix ()
			{
				return (platform == 4 || platform == 128);
			}

			static MWFConfigInstance ()
			{
				string b = "mwf_config";
				string dir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
					
				if (IsUnix ()){
					dir = Path.Combine (dir, ".mono");
					try {
						Directory.CreateDirectory (dir);
					} catch {}
				} 

				default_file_name = Path.Combine (dir, b);
				full_file_name = default_file_name;
			}
			
			public MWFConfigInstance ()
			{
				Open (default_file_name);
			}
			
			// only for testing
			public MWFConfigInstance (string filename)
			{
				string path = Path.GetDirectoryName (filename);
				if (path == null || path == String.Empty) {
					path = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
					
					full_file_name = Path.Combine (path, filename);
				}  else 
					full_file_name = filename;

				Open (full_file_name);
			}				
			
			~MWFConfigInstance ()
			{
				Flush ();
			}
			
			public object GetValue (string class_name, string value_name)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry != null)
					return class_entry.GetValue (value_name);
				
				return null;
			}
			
			public void SetValue (string class_name, string value_name, object value)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry == null) {
					class_entry = new ClassEntry ();
					class_entry.ClassName = class_name;
					classes_hashtable [class_name] = class_entry;
				}
				
				class_entry.SetValue (value_name, value);
			}
			
			private void Open (string filename)
			{
				try {
					XmlTextReader xtr = new XmlTextReader (filename);
					
					ReadConfig (xtr);
					
					xtr.Close ();
				} catch (Exception) {
				}
			}
			
			public void Flush ()
			{
				try {
					XmlTextWriter xtw = new XmlTextWriter (full_file_name, null);
					xtw.Formatting = Formatting.Indented;
					
					WriteConfig (xtw);
					
					xtw.Close ();

					if (!IsUnix ())
						File.SetAttributes (full_file_name, FileAttributes.Hidden);
				} catch (Exception){
				}
			}
			
			public void RemoveClass (string class_name)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry != null) {
					class_entry.RemoveAllClassValues ();
					
					classes_hashtable.Remove (class_name);
				}
			}
			
			public void RemoveClassValue (string class_name, string value_name)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry != null) {
					class_entry.RemoveClassValue (value_name);
				}
			}
			
			public void RemoveAllClassValues (string class_name)
			{
				ClassEntry class_entry = classes_hashtable [class_name] as ClassEntry;
				
				if (class_entry != null) {
					class_entry.RemoveAllClassValues ();
				}
			}
			
			private void ReadConfig (XmlTextReader xtr)
			{
				if (!CheckForMWFConfig (xtr))
					return;
				
				while (xtr.Read ()) {
					switch (xtr.NodeType) {
						case XmlNodeType.Element:
							ClassEntry class_entry = classes_hashtable [xtr.Name] as ClassEntry;
							
							if (class_entry == null) {
								class_entry = new ClassEntry ();
								class_entry.ClassName = xtr.Name;
								classes_hashtable [xtr.Name] = class_entry;
							}
							
							class_entry.ReadXml (xtr);
							break;
					}
				}
			}
			
			private bool CheckForMWFConfig (XmlTextReader xtr)
			{
				if (xtr.Read ()) {
					if (xtr.NodeType == XmlNodeType.Element) {
						if (xtr.Name == configName)
							return true;
					}
				}
				
				return false;
			}
			
			private void WriteConfig (XmlTextWriter xtw)
			{
				if (classes_hashtable.Count == 0)
					return;
				
				xtw.WriteStartElement (configName);
				foreach (DictionaryEntry entry in classes_hashtable) {
					ClassEntry class_entry = entry.Value as ClassEntry;
					
					class_entry.WriteXml (xtw);
				}
				xtw.WriteEndElement ();
			}
			
			internal class ClassEntry
			{
				private Hashtable classvalues_hashtable = new Hashtable ();
				private string className;
				
				public string ClassName {
					set {
						className = value;
					}
					
					get {
						return className;
					}
				}
				
				public void SetValue (string value_name, object value)
				{
					ClassValue class_value = classvalues_hashtable [value_name] as ClassValue;
					
					if (class_value == null) {
						class_value = new ClassValue ();
						class_value.Name = value_name;
						classvalues_hashtable [value_name] = class_value;
					}
					
					class_value.SetValue (value);
				}
				
				public object GetValue (string value_name)
				{
					ClassValue class_value = classvalues_hashtable [value_name] as ClassValue;
					
					if (class_value == null) {
						return null;
					}
					
					return class_value.GetValue ();
				}
				
				public void RemoveAllClassValues ()
				{
					classvalues_hashtable.Clear ();
				}
				
				public void RemoveClassValue (string value_name)
				{
					ClassValue class_value = classvalues_hashtable [value_name] as ClassValue;
					
					if (class_value != null) {
						classvalues_hashtable.Remove (value_name);
					}
				}
				
				public void ReadXml (XmlTextReader xtr)
				{
					while (xtr.Read ()) {
						switch (xtr.NodeType) {
							case XmlNodeType.Element:
								string name = xtr.GetAttribute ("name");
								
								ClassValue class_value = classvalues_hashtable [name] as ClassValue;
								
								if (class_value == null) {
									class_value = new ClassValue ();
									class_value.Name = name;
									classvalues_hashtable [name] = class_value;
								}
								
								class_value.ReadXml (xtr);
								break;
								
							case XmlNodeType.EndElement:
								return;
						}
					}
				}
				
				public void WriteXml (XmlTextWriter xtw)
				{
					if (classvalues_hashtable.Count == 0)
						return;
					
					xtw.WriteStartElement (className);
					
					foreach (DictionaryEntry entry in classvalues_hashtable) {
						ClassValue class_value = entry.Value as ClassValue;
						
						class_value.WriteXml (xtw);
					}
					
					xtw.WriteEndElement ();
				}
			}
			
			internal class ClassValue
			{
				private object value;
				private string name;
				
				public string Name {
					set {
						name = value;
					}
					
					get {
						return name;
					}
				}
				
				public void SetValue (object value)
				{
					this.value = value;
				}
				public object GetValue ()
				{
					return value;
				}
				
				public void ReadXml (XmlTextReader xtr)
				{
					string type;
					string single_value;
					
					type = xtr.GetAttribute ("type");
					
					if (type == "byte_array" || type.IndexOf ("-array") == -1) {
						single_value = xtr.ReadString ();
						
						if (type == "string") {
							value = single_value;
						} else
						if (type == "int") {
							value = Int32.Parse (single_value);
						} else
						if (type == "byte") {
							value = Byte.Parse (single_value);
						} else
						if (type == "color") {
							int color = Int32.Parse (single_value);
							value = Color.FromArgb (color);
						} else
						if (type == "byte-array") {
							byte[] b_array = Convert.FromBase64String (single_value);
							value = b_array;
						}
					} else {
						ReadXmlArrayValues (xtr, type);
					}
				}
				
				private void ReadXmlArrayValues (XmlTextReader xtr, string type)
				{
					ArrayList al = new ArrayList ();
					
					while (xtr.Read ()) {
						switch (xtr.NodeType) {
							case XmlNodeType.Element:
								string single_value = xtr.ReadString ();
								
								if (type == "int-array") {
									int int_val = Int32.Parse (single_value);
									al.Add (int_val);
								} else
								if (type == "string-array") {
									string str_val = single_value;
									al.Add (str_val);
								}
								break;
								
							case XmlNodeType.EndElement:
								if (xtr.Name == "value") {
									if (type == "int-array") {
										value = al.ToArray (typeof(int));
									} else
									if (type == "string-array") {
										value = al.ToArray (typeof(string));
									} 
									return;
								}
								break;
						}
					}
				}
				
				public void WriteXml (XmlTextWriter xtw)
				{
					xtw.WriteStartElement ("value");
					xtw.WriteAttributeString ("name", name);
					if (value is Array) {
						WriteArrayContent (xtw);
					} else {
						WriteSingleContent (xtw);
					}
					xtw.WriteEndElement ();
				}
				
				private void WriteSingleContent (XmlTextWriter xtw)
				{
					string type_string = String.Empty;
					
					if (value is string)
						type_string = "string";
					else
					if (value is int)
						type_string = "int";
					else
					if (value is byte)
						type_string = "byte";
					else
					if (value is Color)
						type_string = "color";
					
					xtw.WriteAttributeString ("type", type_string);
					
					if (value is Color)
						xtw.WriteString (((Color)value).ToArgb ().ToString ());
					else
						xtw.WriteString (value.ToString ());
				}
				
				private void WriteArrayContent (XmlTextWriter xtw)
				{
					string type_string = String.Empty;
					string type_name = String.Empty;
					
					if (value is string[]) {
						type_string = "string-array";
						type_name = "string";
					} else
					if (value is int[]) {
						type_string = "int-array";
						type_name = "int";
					} else
					if (value is byte[]) {
						type_string = "byte-array";
						type_name = "byte";
					}
					
					xtw.WriteAttributeString ("type", type_string);
					
					if (type_string != "byte-array") {
						Array array = value as Array;
						
						foreach (object o in array) {
							xtw.WriteStartElement (type_name);
							xtw.WriteString (o.ToString ());
							xtw.WriteEndElement ();
						}
					} else {
						byte[] b_array = value as byte [];
						
						xtw.WriteString (Convert.ToBase64String (b_array, 0, b_array.Length));
					}
				}
			}
		}
	}
	#endregion
}


