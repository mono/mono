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
//	Alexander Olk (alex.olk@googlemail.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
//

using System;
using System.Drawing;
using System.ComponentModel;
using System.Resources;
using System.IO;
using System.Collections;

namespace System.Windows.Forms {
	[DefaultEvent ("HelpRequest")]
	[DefaultProperty ("SelectedPath")]
	[Designer ("System.Windows.Forms.Design.FolderBrowserDialogDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public sealed class FolderBrowserDialog : CommonDialog
	{
		#region Local Variables
		private Environment.SpecialFolder rootFolder = Environment.SpecialFolder.Desktop;
		private string selectedPath = string.Empty;
		private bool showNewFolderButton = true;
		
		private Label descriptionLabel;
		private Button cancelButton;
		private Button okButton;
		private FolderBrowserTreeView folderBrowserTreeView;
		private Button newFolderButton;
		private ContextMenu folderBrowserTreeViewContextMenu;
		private MenuItem newFolderMenuItem;
		
		private string old_selectedPath = string.Empty;
		
		private readonly string folderbrowserdialog_string = "FolderBrowserDialog";
		private readonly string width_string = "Width";
		private readonly string height_string = "Height";
		private readonly string x_string = "X";
		private readonly string y_string = "Y";
		#endregion	// Local Variables
		
		#region Public Constructors
		public FolderBrowserDialog ()
		{
			form = new DialogForm (this);
			Size formConfigSize = Size.Empty;
			Point formConfigLocation = Point.Empty;
			
			object formWidth = MWFConfig.GetValue (folderbrowserdialog_string, width_string);
			
			object formHeight = MWFConfig.GetValue (folderbrowserdialog_string, height_string);
			
			if (formHeight != null && formWidth != null)
				formConfigSize = new Size ((int)formWidth, (int)formHeight);
			
			object formLocationX = MWFConfig.GetValue (folderbrowserdialog_string, x_string);
			object formLocationY = MWFConfig.GetValue (folderbrowserdialog_string, y_string);
			
			if (formLocationX != null && formLocationY != null)
				formConfigLocation = new Point ((int)formLocationX, (int)formLocationY);
			
			newFolderButton = new Button ();
			folderBrowserTreeView = new FolderBrowserTreeView (this);
			okButton = new Button ();
			cancelButton = new Button ();
			descriptionLabel = new Label ();
			folderBrowserTreeViewContextMenu = new ContextMenu ();
			
			form.AcceptButton = okButton;
			form.CancelButton = cancelButton;
			
			form.SuspendLayout ();
			form.ClientSize = new Size (322, 324);
			form.MinimumSize = new Size (310, 254);
			form.Text = "Browse For Folder";
			form.SizeGripStyle = SizeGripStyle.Show;

			newFolderMenuItem = new MenuItem("New Folder", new EventHandler (OnClickNewFolderButton));
			folderBrowserTreeViewContextMenu.MenuItems.Add(newFolderMenuItem);
			
			// descriptionLabel
			descriptionLabel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
				| AnchorStyles.Right)));
			descriptionLabel.Location = new Point (15, 14);
			descriptionLabel.Size = new Size (292, 40);
			descriptionLabel.TabIndex = 0;
			descriptionLabel.Text = string.Empty;
			
			// folderBrowserTreeView
			folderBrowserTreeView.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
				| AnchorStyles.Left)
				| AnchorStyles.Right)));
			folderBrowserTreeView.ImageIndex = -1;
			folderBrowserTreeView.Location = new Point (15, 60);
			folderBrowserTreeView.SelectedImageIndex = -1;
			folderBrowserTreeView.Size = new Size (292, 212);
			folderBrowserTreeView.TabIndex = 3;
			folderBrowserTreeView.ShowLines = false;
			folderBrowserTreeView.ShowPlusMinus = true;
			folderBrowserTreeView.HotTracking = true;
			folderBrowserTreeView.BorderStyle = BorderStyle.Fixed3D;
			folderBrowserTreeView.ContextMenu = folderBrowserTreeViewContextMenu;
			//folderBrowserTreeView.Indent = 2;
			
			// newFolderButton
			newFolderButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
			newFolderButton.FlatStyle = FlatStyle.System;
			newFolderButton.Location = new Point (15, 285);
			newFolderButton.Size = new Size (105, 23);
			newFolderButton.TabIndex = 4;
			newFolderButton.Text = "Make New Folder";
			newFolderButton.Enabled = true;
			
			// okButton
			okButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			okButton.FlatStyle = FlatStyle.System;
			okButton.Location = new Point (135, 285);
			okButton.Size = new Size (80, 23);
			okButton.TabIndex = 1;
			okButton.Text = "OK";
			
			// cancelButton
			cancelButton.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.FlatStyle = FlatStyle.System;
			cancelButton.Location = new Point (227, 285);
			cancelButton.Size = new Size (80, 23);
			cancelButton.TabIndex = 2;
			cancelButton.Text = "Cancel";
			
			form.Controls.Add (cancelButton);
			form.Controls.Add (okButton);
			form.Controls.Add (newFolderButton);
			form.Controls.Add (folderBrowserTreeView);
			form.Controls.Add (descriptionLabel);
			
			form.ResumeLayout (false);
			
			if (formConfigSize != Size.Empty) {
				form.Size = formConfigSize;
			}
			
			if (formConfigLocation != Point.Empty) {
				form.Location = formConfigLocation;
			}
			
			okButton.Click += new EventHandler (OnClickOKButton);
			cancelButton.Click += new EventHandler (OnClickCancelButton);
			newFolderButton.Click += new EventHandler (OnClickNewFolderButton);

			form.VisibleChanged += new EventHandler (OnFormVisibleChanged);
			
			RootFolder = rootFolder;
		}
		
		#endregion	// Public Constructors
		
		#region Public Instance Properties
		[Browsable(true)]
		[DefaultValue("")]
		[Localizable(true)]
		public string Description {
			set {
				descriptionLabel.Text = value;
			}
			
			get {
				return descriptionLabel.Text;
			}
		}
		
		[Browsable(true)]
		[DefaultValue(Environment.SpecialFolder.Desktop)]
		[Localizable(false)]
		[TypeConverter (typeof (SpecialFolderEnumConverter))]
		public Environment.SpecialFolder RootFolder {
			set {
				int v = (int)value;

				Type enumType = typeof (Environment.SpecialFolder);
				if (!Enum.IsDefined (enumType, v))
					throw new InvalidEnumArgumentException (
						"value", v, enumType);
				
				rootFolder = value;
			}
			get {
				return rootFolder;
			}
		}
		
		[Browsable(true)]
		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.SelectedPathEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		public string SelectedPath {
			set {
				if (value == null)
					value = string.Empty;
				selectedPath = value;
				old_selectedPath = value;
			}
			get {
				return selectedPath;
			}
		}
		
		[Browsable(true)]
		[DefaultValue(true)]
		[Localizable(false)]
		public bool ShowNewFolderButton {
			set {
				if (value != showNewFolderButton) {
					newFolderButton.Visible = value;
					showNewFolderButton = value;
				}
			}
			
			get {
				return showNewFolderButton;
			}
		}
		#endregion	// Public Instance Properties
		
		#region Public Instance Methods
		public override void Reset ()
		{
			Description = string.Empty;
			RootFolder = Environment.SpecialFolder.Desktop;
			selectedPath = string.Empty;
			ShowNewFolderButton = true;
		}
		
		protected override bool RunDialog (IntPtr hWndOwner)
		{
			folderBrowserTreeView.RootFolder = RootFolder;
			folderBrowserTreeView.SelectedPath = SelectedPath;

			form.Refresh ();
			
			return true;
		}
		#endregion	// Public Instance Methods
		
		#region Internal Methods
		void OnClickOKButton (object sender, EventArgs e)
		{
			WriteConfigValues ();
			
			form.DialogResult = DialogResult.OK;
		}
		
		void OnClickCancelButton (object sender, EventArgs e)
		{
			WriteConfigValues ();
			
			selectedPath = old_selectedPath;
			form.DialogResult = DialogResult.Cancel;
		}
		
		void OnClickNewFolderButton (object sender, EventArgs e)
		{
			folderBrowserTreeView.CreateNewFolder ();
		}

		void OnFormVisibleChanged (object sender, EventArgs e)
		{
			if (form.Visible && okButton.Enabled)
				okButton.Select ();
		}
		
		private void WriteConfigValues ()
		{
			MWFConfig.SetValue (folderbrowserdialog_string, width_string, form.Width);
			MWFConfig.SetValue (folderbrowserdialog_string, height_string, form.Height);
			MWFConfig.SetValue (folderbrowserdialog_string, x_string, form.Location.X);
			MWFConfig.SetValue (folderbrowserdialog_string, y_string, form.Location.Y);
		}
		#endregion	// Internal Methods
		
		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler HelpRequest {
			add { base.HelpRequest += value; }
			remove { base.HelpRequest -= value; }
		}
		#endregion
		
		internal class FolderBrowserTreeView : TreeView
		{
			private MWFVFS vfs = new MWFVFS ();
			new private FBTreeNode root_node;
			private FolderBrowserDialog parentDialog;
			private ImageList imageList = new ImageList ();
			private Environment.SpecialFolder rootFolder;
			private bool dont_enable = false;
			private TreeNode node_under_mouse;
			
			public FolderBrowserTreeView (FolderBrowserDialog parent_dialog)
			{
				parentDialog = parent_dialog;
				HideSelection = false;
				ImageList = imageList;
				SetupImageList ();
			}
			
			public Environment.SpecialFolder RootFolder {
				set {
					rootFolder = value;
					
					string root_path = string.Empty;
					
					switch (rootFolder) {
						case Environment.SpecialFolder.Desktop:
							root_node = new FBTreeNode ("Desktop");
							root_node.RealPath = ThemeEngine.Current.Places (UIIcon.PlacesDesktop);
							root_path = MWFVFS.DesktopPrefix;
							break;
						case Environment.SpecialFolder.Recent:
							root_node = new FBTreeNode ("My Recent Documents");
							root_node.RealPath = ThemeEngine.Current.Places (UIIcon.PlacesRecentDocuments);
							root_path = MWFVFS.RecentlyUsedPrefix;
							break;
						case Environment.SpecialFolder.MyComputer:
							root_node = new FBTreeNode ("My Computer");
							root_path = MWFVFS.MyComputerPrefix;
							break;
						case Environment.SpecialFolder.Personal:
							root_node = new FBTreeNode ("Personal");
							root_path = MWFVFS.PersonalPrefix;
							root_node.RealPath = ThemeEngine.Current.Places (UIIcon.PlacesPersonal);
							break;
						default:
							root_node = new FBTreeNode (rootFolder.ToString ());
							root_node.RealPath = Environment.GetFolderPath (rootFolder);
							root_path = root_node.RealPath;
							break;
					}
					
					root_node.Tag = root_path;
					root_node.ImageIndex = NodeImageIndex (root_path);

					BeginUpdate ();
					Nodes.Clear ();
					EndUpdate ();

					FillNode (root_node);
					
					root_node.Expand ();
					
					Nodes.Add (root_node);
				}
			}
			
			public string SelectedPath {
				set {
					if (value.Length == 0)
						return;

					if (!Path.IsPathRooted (value))
						return;

					try {
						if (Check_if_path_is_child_of_RootFolder (value))
							SetSelectedPath (Path.GetFullPath (value));
					} catch (Exception) {
						// If we can't set the user's requested path, the
						// best we can do is not crash and reset to the default
						EndUpdate ();
						RootFolder = rootFolder;
					}
				}
			}
			
			private string parent_real_path;
			private bool dont_do_onbeforeexpand;
			
			public void CreateNewFolder ()
			{
				FBTreeNode fbnode = node_under_mouse == null ? SelectedNode as FBTreeNode : node_under_mouse as FBTreeNode;
				
				if (fbnode == null || fbnode.RealPath == null)
					return;
				
				string tmp_filename = "New Folder";
				
				if (Directory.Exists (Path.Combine (fbnode.RealPath, tmp_filename))) {
					int i = 1;

					if (XplatUI.RunningOnUnix) {
						tmp_filename = tmp_filename + "-" + i;
					} else {
						tmp_filename = tmp_filename + " (" + i + ")";
					}
					
					while (Directory.Exists (Path.Combine (fbnode.RealPath, tmp_filename))) {
						i++;
						if (XplatUI.RunningOnUnix) {
							tmp_filename = "New Folder" + "-" + i;
						} else {
							tmp_filename = "New Folder" + " (" + i + ")";
						}
					}
				}

				parent_real_path = fbnode.RealPath;
				
				FillNode (fbnode);
				dont_do_onbeforeexpand = true;
				fbnode.Expand ();
				dont_do_onbeforeexpand = false;

				// to match MS, immediately create the new folder
				// and rename it once the label edit completes
				string fullPath = Path.Combine (fbnode.RealPath, tmp_filename);
				if (!vfs.CreateFolder (fullPath))
					return;

				FBTreeNode new_node = new FBTreeNode (tmp_filename);
				new_node.ImageIndex = NodeImageIndex (tmp_filename);
				new_node.Tag = new_node.RealPath = fullPath;
				fbnode.Nodes.Add (new_node);

				LabelEdit = true;
				new_node.BeginEdit();
			}
			
			protected override void OnAfterLabelEdit (NodeLabelEditEventArgs e)
			{
				if (e.Label != null) {
					if (e.Label.Length > 0) {
						FBTreeNode fbnode = e.Node as FBTreeNode;

						string originalPath = fbnode.RealPath;
						string newPath = Path.Combine (parent_real_path, e.Label);

						if (vfs.MoveFolder (originalPath, newPath)) {
							fbnode.Tag = fbnode.RealPath = newPath;
						} else {
							e.CancelEdit = true;
							e.Node.BeginEdit ();
							return;
						}
					} else {
						e.CancelEdit = true;
						e.Node.BeginEdit ();
						return;
					}
				}

				// select new folder only if both the curren node under
				// mouse pointer and SelectedNode are the same (match .Net)
				if (node_under_mouse == SelectedNode)
					SelectedNode = e.Node;

				// disable LabelEdit when either edit has finished
				// or has been cancelled, to prevent the user from
				// editing label of existing folders
				LabelEdit = false;
			}
			
			private void SetSelectedPath (string path)
			{
				BeginUpdate ();
				
				FBTreeNode node = FindPathInNodes (path, Nodes);
				
				if (node == null) {
					Stack stack = new Stack ();
					
					string path_cut = path.Substring (0, path.LastIndexOf (Path.DirectorySeparatorChar));
					if (!XplatUI.RunningOnUnix && path_cut.Length == 2)
						path_cut += Path.DirectorySeparatorChar;
					
					while (node == null && path_cut.Length > 0) {
						node = FindPathInNodes (path_cut, Nodes);
						
						if (node == null) {
							string path_cut_new = path_cut.Substring (0, path_cut.LastIndexOf (Path.DirectorySeparatorChar));
							string leftover = path_cut.Replace (path_cut_new, string.Empty);
							
							stack.Push (leftover);
							
							path_cut = path_cut_new;
						}
					}

					// If we didn't find anything, just display the full, unselected tree
					if (node == null) {
						EndUpdate ();
						RootFolder = rootFolder;
						return;
					}
					
					FillNode (node);
					node.Expand ();

					// walk through the subdirs and fill the nodes
					while (stack.Count > 0) {
						string part_name = stack.Pop () as string;

						foreach (TreeNode treeNode in node.Nodes) {
							FBTreeNode fbnode = treeNode as FBTreeNode;

							if (path_cut + part_name == fbnode.RealPath) {
								node = fbnode;
								path_cut += part_name;

								FillNode (node);
								node.Expand ();
								break;
							}
						}
					}

					// finally find the node for the complete path
					foreach (TreeNode treeNode in node.Nodes) {
						FBTreeNode fbnode = treeNode as FBTreeNode;

						if (path == fbnode.RealPath) {
							node = fbnode;
							break;
						}
					}
				}
				
				if (node != null) {
					SelectedNode = node;
					node.EnsureVisible ();
				}
				
				EndUpdate ();
			}
			
			private FBTreeNode FindPathInNodes (string path, TreeNodeCollection nodes)
			{
				// On Windows the devices can be passed as C: yet match
				// the C:\ form
				//
				// Hackish, but works
				if (!XplatUI.RunningOnUnix && path.Length == 2)
					path += Path.DirectorySeparatorChar;

				foreach (TreeNode node in nodes) {
					FBTreeNode fbnode = node as FBTreeNode;
					
					if (fbnode != null && fbnode.RealPath != null) {
						if (fbnode.RealPath == path)
							return fbnode;
					}
					
					FBTreeNode n = FindPathInNodes (path, node.Nodes);
					if (n != null)
						return n;
				}
				
				return null;
			}
			
			private bool Check_if_path_is_child_of_RootFolder (string path)
			{
				string root_path = (string) root_node.RealPath;
				
				if (root_path != null || rootFolder == Environment.SpecialFolder.MyComputer) {
					try {
						if (!Directory.Exists (path))
							return false;
						
						switch (rootFolder) {
							case Environment.SpecialFolder.Desktop:
							case Environment.SpecialFolder.MyComputer:
								return true;
							case Environment.SpecialFolder.Personal:
								if (!path.StartsWith (root_path))
									return false;
								else
									return true;
							default:
								return false;
						}
					} catch {}
				}
				
				return false;
			}
			
			private void FillNode (TreeNode node)
			{
				BeginUpdate ();
				
				node.Nodes.Clear ();
				vfs.ChangeDirectory ((string)node.Tag);
				ArrayList folders = vfs.GetFoldersOnly ();
				
				foreach (FSEntry fsentry in folders) {
					if (fsentry.Name.StartsWith ("."))
						continue;
					
					FBTreeNode child = new FBTreeNode (fsentry.Name);
					child.Tag = fsentry.FullName;
					child.RealPath = fsentry.RealName == null ? fsentry.FullName : fsentry.RealName;
					child.ImageIndex = NodeImageIndex (fsentry.FullName);
					
					vfs.ChangeDirectory (fsentry.FullName);
					ArrayList sub_folders = vfs.GetFoldersOnly ();
					
					foreach (FSEntry fsentry_sub in sub_folders) {
						if (!fsentry_sub.Name.StartsWith (".")) {
							child.Nodes.Add (new TreeNode (String.Empty));
							break;
						}
					}
					
					node.Nodes.Add (child);
				}
				
				EndUpdate ();
			}
			
			private void SetupImageList ()
			{
				imageList.ColorDepth = ColorDepth.Depth32Bit;
				imageList.ImageSize = new Size (16, 16);
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesRecentDocuments, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesDesktop, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesPersonal, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesMyComputer, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.PlacesMyNetwork, 16));
				imageList.Images.Add (ThemeEngine.Current.Images (UIIcon.NormalFolder, 16));
				imageList.TransparentColor = Color.Transparent;
			}
			
			private int NodeImageIndex (string path)
			{
				int index = 5;
				
				if (path == MWFVFS.DesktopPrefix)
					index = 1;
				else if (path == MWFVFS.RecentlyUsedPrefix)
					index = 0;
				else if (path == MWFVFS.PersonalPrefix)
					index = 2;
				else if (path == MWFVFS.MyComputerPrefix)
					index = 3;
				else if (path == MWFVFS.MyNetworkPrefix)
					index = 4;
				
				return index;
			}
			
			protected override void OnAfterSelect (TreeViewEventArgs e)
			{
				if (e.Node == null)
					return;
				
				FBTreeNode fbnode = e.Node as FBTreeNode;
				
				if (fbnode.RealPath == null || fbnode.RealPath.IndexOf ("://") != -1) {
					parentDialog.okButton.Enabled = false;
					parentDialog.newFolderButton.Enabled = false;
					parentDialog.newFolderMenuItem.Enabled = false;
					dont_enable = true;
				} else {
					parentDialog.okButton.Enabled = true;
					parentDialog.newFolderButton.Enabled = true;
					parentDialog.newFolderMenuItem.Enabled = true;
					parentDialog.selectedPath = fbnode.RealPath;
					dont_enable = false;
				}
				
				base.OnAfterSelect (e);
			}
			
			protected internal override void OnBeforeExpand (TreeViewCancelEventArgs e)
			{
				if (!dont_do_onbeforeexpand) {
					if (e.Node == root_node)
						return;
					FillNode (e.Node);
				}
				
				base.OnBeforeExpand (e);
			}

			protected override void OnMouseDown (MouseEventArgs e)
			{
				node_under_mouse = GetNodeAt (e.X, e.Y);
				base.OnMouseDown (e);
			}
			
			protected override void OnMouseUp (MouseEventArgs e)
			{
				if (SelectedNode == null) {
					parentDialog.okButton.Enabled = false;
					parentDialog.newFolderButton.Enabled = false;
					parentDialog.newFolderMenuItem.Enabled = false;
				} else
				if (!dont_enable) {
					parentDialog.okButton.Enabled = true;
					parentDialog.newFolderButton.Enabled = true;
					parentDialog.newFolderMenuItem.Enabled = true;
				}

				node_under_mouse = null;
				
				base.OnMouseUp (e);
			}
		}
		
		internal class FBTreeNode : TreeNode
		{
			private string realPath = null;
			
			public FBTreeNode (string text)
			{
				Text = text;
			}
			
			public string RealPath {
				set {
					realPath = value;
				}
				
				get {
					return realPath;
				}
			}
		}
	}
	
	internal class SpecialFolderEnumConverter : TypeConverter
	{
		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if ((value == null) || !(value is String))
				return base.ConvertFrom (context, culture, value);

			return Enum.Parse (typeof (Environment.SpecialFolder), (string)value, true);
		}

		public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if ((value == null) || !(value is Environment.SpecialFolder) || (destinationType != typeof (string)))
				return base.ConvertTo (context, culture, value, destinationType);
				
			return ((Environment.SpecialFolder)value).ToString ();
		}
	}
}
