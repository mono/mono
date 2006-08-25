//
// EventViewer.cs
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
// 
//
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

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Mono.Tools.EventViewer {
	public class EventViewer : System.Windows.Forms.Form {
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.TreeView logTree;
		private System.Windows.Forms.ImageList imageList1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ToolBarButton logPropertiesButton;
		private System.Windows.Forms.ToolBar mainToolbar;
		private System.Windows.Forms.ToolBarButton refreshEntriesButton;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.ContextMenu treeContextMenu;
		private System.Windows.Forms.MenuItem refreshEntriesMenuItem;
		private System.Windows.Forms.MenuItem logPropertiesMenuItem;
		private System.Windows.Forms.MenuItem clearEventsMenuItem;
		private System.Windows.Forms.MenuItem treeContextMenuSeparator2;
		private System.Windows.Forms.MenuItem treeContextMenuSeparator1;
		private System.Windows.Forms.MenuItem treeContextMenuSeparator3;
		private System.Windows.Forms.MenuItem connectToComputerMenuItem;
		private System.Windows.Forms.MenuItem openLogFileMenuItem;
		private System.Windows.Forms.MenuItem helpMenuItem;

		private int _entryListSortColumn = -1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ListView entryList;
		private System.Windows.Forms.StatusBar mainStatusBar;
		private System.Windows.Forms.StatusBar listStatusBar;
		private System.Windows.Forms.StatusBarPanel listStatusPanel;
		private TreeNode _rightClickedNode;
		private EventEntryProperties _eventProperties;

		public EventViewer () {
			InitializeComponent ();
			LoadIcons (imageList1);
			RetrieveLogs (".");
			logTree.ExpandAll ();
			RefreshMainToolbar ();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose (bool disposing) {
			if (disposing) {
				if (components != null)
					components.Dispose ();
				if (_eventProperties != null)
					_eventProperties.Dispose ();
			}
			base.Dispose (disposing);
		}

		private void LoadIcons (ImageList imageList) {
			Assembly assembly = typeof (EventViewer).Assembly;
			imageList.Images.Add (LoadIconResource (assembly, "error.ico"));
			imageList.Images.Add (LoadIconResource (assembly, "info.ico"));
			imageList.Images.Add (LoadIconResource (assembly, "warning.ico"));
			imageList.Images.Add (LoadIconResource (assembly, "eventlog.ico"));
			imageList.Images.Add (LoadIconResource (assembly, "computer.ico"));
			imageList.Images.Add (LoadIconResource (assembly, "successaudit.ico"));
			imageList.Images.Add (LoadIconResource (assembly, "failureaudit.ico"));
			imageList.Images.Add (LoadBitmapResource (assembly, "refresh.png"));
			imageList.Images.Add (LoadBitmapResource (assembly, "properties.png"));
		}

		private void InitializeComponent () {
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(EventViewer));
			this.mainStatusBar = new System.Windows.Forms.StatusBar();
			this.mainToolbar = new System.Windows.Forms.ToolBar();
			this.logPropertiesButton = new System.Windows.Forms.ToolBarButton();
			this.refreshEntriesButton = new System.Windows.Forms.ToolBarButton();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.entryList = new System.Windows.Forms.ListView();
			this.listStatusBar = new System.Windows.Forms.StatusBar();
			this.listStatusPanel = new System.Windows.Forms.StatusBarPanel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.logTree = new System.Windows.Forms.TreeView();
			this.treeContextMenu = new System.Windows.Forms.ContextMenu();
			this.connectToComputerMenuItem = new System.Windows.Forms.MenuItem();
			this.openLogFileMenuItem = new System.Windows.Forms.MenuItem();
			this.treeContextMenuSeparator1 = new System.Windows.Forms.MenuItem();
			this.clearEventsMenuItem = new System.Windows.Forms.MenuItem();
			this.refreshEntriesMenuItem = new System.Windows.Forms.MenuItem();
			this.treeContextMenuSeparator2 = new System.Windows.Forms.MenuItem();
			this.logPropertiesMenuItem = new System.Windows.Forms.MenuItem();
			this.treeContextMenuSeparator3 = new System.Windows.Forms.MenuItem();
			this.helpMenuItem = new System.Windows.Forms.MenuItem();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.listStatusPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// mainStatusBar
			// 
			this.mainStatusBar.Location = new System.Drawing.Point(0, 355);
			this.mainStatusBar.Name = "mainStatusBar";
			this.mainStatusBar.Size = new System.Drawing.Size(472, 22);
			this.mainStatusBar.TabIndex = 0;
			// 
			// mainToolbar
			// 
			this.mainToolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.mainToolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						   this.logPropertiesButton,
																						   this.refreshEntriesButton});
			this.mainToolbar.ButtonSize = new System.Drawing.Size(30, 25);
			this.mainToolbar.DropDownArrows = true;
			this.mainToolbar.ImageList = this.imageList1;
			this.mainToolbar.Location = new System.Drawing.Point(0, 0);
			this.mainToolbar.Name = "mainToolbar";
			this.mainToolbar.ShowToolTips = true;
			this.mainToolbar.Size = new System.Drawing.Size(472, 28);
			this.mainToolbar.TabIndex = 1;
			this.mainToolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.mainToolbar_ButtonClick);
			// 
			// logPropertiesButton
			// 
			this.logPropertiesButton.ImageIndex = 8;
			this.logPropertiesButton.ToolTipText = "Properties";
			// 
			// refreshEntriesButton
			// 
			this.refreshEntriesButton.ImageIndex = 7;
			this.refreshEntriesButton.ToolTipText = "Refresh";
			// 
			// imageList1
			// 
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.entryList);
			this.panel1.Controls.Add(this.listStatusBar);
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.logTree);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 28);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(472, 327);
			this.panel1.TabIndex = 2;
			// 
			// entryList
			// 
			this.entryList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.entryList.FullRowSelect = true;
			this.entryList.HideSelection = false;
			this.entryList.Location = new System.Drawing.Point(179, 22);
			this.entryList.MultiSelect = false;
			this.entryList.Name = "entryList";
			this.entryList.Size = new System.Drawing.Size(293, 305);
			this.entryList.SmallImageList = this.imageList1;
			this.entryList.TabIndex = 3;
			this.entryList.TabStop = false;
			this.entryList.View = System.Windows.Forms.View.Details;
			this.entryList.DoubleClick += new System.EventHandler(this.entryList_DoubleClick);
			this.entryList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.entryList_ColumnClick);
			// 
			// listStatusBar
			// 
			this.listStatusBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.listStatusBar.Location = new System.Drawing.Point(179, 0);
			this.listStatusBar.Name = "listStatusBar";
			this.listStatusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																							 this.listStatusPanel});
			this.listStatusBar.ShowPanels = true;
			this.listStatusBar.Size = new System.Drawing.Size(293, 22);
			this.listStatusBar.SizingGrip = false;
			this.listStatusBar.TabIndex = 2;
			this.listStatusBar.Text = "statusBar2";
			// 
			// listStatusPanel
			// 
			this.listStatusPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.listStatusPanel.Width = 293;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(176, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 327);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// logTree
			// 
			this.logTree.Dock = System.Windows.Forms.DockStyle.Left;
			this.logTree.HideSelection = false;
			this.logTree.ImageList = this.imageList1;
			this.logTree.Location = new System.Drawing.Point(0, 0);
			this.logTree.Name = "logTree";
			this.logTree.Scrollable = false;
			this.logTree.Size = new System.Drawing.Size(176, 327);
			this.logTree.TabIndex = 0;
			this.logTree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.logTree_MouseUp);
			this.logTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.logTree_AfterSelect);
			// 
			// treeContextMenu
			// 
			this.treeContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.connectToComputerMenuItem,
																							this.openLogFileMenuItem,
																							this.treeContextMenuSeparator1,
																							this.clearEventsMenuItem,
																							this.refreshEntriesMenuItem,
																							this.treeContextMenuSeparator2,
																							this.logPropertiesMenuItem,
																							this.treeContextMenuSeparator3,
																							this.helpMenuItem});
			// 
			// connectToComputerMenuItem
			// 
			this.connectToComputerMenuItem.Index = 0;
			this.connectToComputerMenuItem.Text = "&Connect to another computer...";
			// 
			// openLogFileMenuItem
			// 
			this.openLogFileMenuItem.Index = 1;
			this.openLogFileMenuItem.Text = "&Open Log File...";
			// 
			// treeContextMenuSeparator1
			// 
			this.treeContextMenuSeparator1.Index = 2;
			this.treeContextMenuSeparator1.Text = "-";
			// 
			// clearEventsMenuItem
			// 
			this.clearEventsMenuItem.Index = 3;
			this.clearEventsMenuItem.Text = "&Clear all events";
			this.clearEventsMenuItem.Click += new System.EventHandler(this.clearEventsMenuItem_Click);
			// 
			// refreshEntriesMenuItem
			// 
			this.refreshEntriesMenuItem.Index = 4;
			this.refreshEntriesMenuItem.Text = "Re&fresh";
			this.refreshEntriesMenuItem.Click += new System.EventHandler(this.refreshEntriesMenuItem_Click);
			// 
			// treeContextMenuSeparator2
			// 
			this.treeContextMenuSeparator2.Index = 5;
			this.treeContextMenuSeparator2.Text = "-";
			// 
			// logPropertiesMenuItem
			// 
			this.logPropertiesMenuItem.Index = 6;
			this.logPropertiesMenuItem.Text = "P&roperties";
			// 
			// treeContextMenuSeparator3
			// 
			this.treeContextMenuSeparator3.Index = 7;
			this.treeContextMenuSeparator3.Text = "-";
			// 
			// helpMenuItem
			// 
			this.helpMenuItem.Index = 8;
			this.helpMenuItem.Text = "&Help";
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuItem1,
																					 this.menuItem3});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem2});
			this.menuItem1.Text = "&File";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 0;
			this.menuItem2.Text = "Exit";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem4});
			this.menuItem3.Text = "&Help";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 0;
			this.menuItem4.Text = "About Event Viewer...";
			// 
			// EventViewer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 377);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.mainToolbar);
			this.Controls.Add(this.mainStatusBar);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu;
			this.Name = "EventViewer";
			this.Text = "Event Viewer";
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.listStatusPanel)).EndInit();
			this.ResumeLayout(false);
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main () {
			try {
				Application.Run (new EventViewer ());
			} catch (Exception ex) {
				MessageBox.Show (ex.ToString ());
			}
		}

		private void RetrieveLogs (string machineName) {
			EventLog [] eventLogs = EventLog.GetEventLogs (machineName);
			string machineDisplayName = (machineName == ".") ? "local" : machineName;
			TreeNode machineNode = new TreeNode ("Event Viewer (" + machineDisplayName + ")", 4, 4);
			machineNode.Tag = machineName;
			logTree.Nodes.Add (machineNode);
			foreach (EventLog eventLog in eventLogs) {
				TreeNode logNode = new TreeNode (eventLog.LogDisplayName, 3, 3);
				logNode.Tag = eventLog.Log;
				machineNode.Nodes.Add (logNode);
			}
			logTree.SelectedNode = machineNode;
		}

		private void DisplayEventEntries (string logName, string machineName) {
			entryList.BeginUpdate ();
			try {
				this.Cursor = Cursors.WaitCursor;
				// remove current items
				entryList.Items.Clear ();
				// remove current columns
				entryList.Columns.Clear ();
				// create necessary columns
				entryList.Columns.Add ("Type", 100, HorizontalAlignment.Left);
				entryList.Columns.Add ("Date", 100, HorizontalAlignment.Left);
				entryList.Columns.Add ("Time", 100, HorizontalAlignment.Left);
				entryList.Columns.Add ("Source", 200, HorizontalAlignment.Left);
				entryList.Columns.Add ("Category", 100, HorizontalAlignment.Left);
				entryList.Columns.Add ("Event", 100, HorizontalAlignment.Left);
				entryList.Columns.Add ("User", 100, HorizontalAlignment.Left);
				entryList.Columns.Add ("Computer", 100, HorizontalAlignment.Left);
				// add eventlog entries
				using (EventLog eventLog = new EventLog (logName, machineName)) {
					EventLogEntryCollection entries = eventLog.Entries;
					foreach (EventLogEntry entry in entries) {
                        EventEntryView view = new EventEntryView (entry);
						// most recent event log entries are at the top
						entryList.Items.Insert (0, view.ListViewItem);
					}
				}

			} finally {
				this.Cursor = Cursors.Default;
				entryList.EndUpdate ();
			}
		}

		private void logTree_AfterSelect (object sender, System.Windows.Forms.TreeViewEventArgs e) {
			TreeNode selectedNode = logTree.SelectedNode;
			RefreshMainToolbar ();
			if (selectedNode.Parent == null) {
				RefreshEventLogList ();
			} else {
				RefreshEventEntryList ();
			}
		}

		private void menuItem2_Click (object sender, System.EventArgs e) {
			this.Close ();
		}

		private static Icon LoadIconResource (Assembly assembly, string name) {
			string manifestResourceName = typeof (EventViewer).Namespace
				+ ".Resources." + name;
			Stream s = assembly.GetManifestResourceStream (manifestResourceName);
			if (s == null) {
				throw new ArgumentException (string.Format (CultureInfo.InvariantCulture,
					"Icon '{0}' does not exist.", name));
			}
			return new Icon (s);
		}

		private static Bitmap LoadBitmapResource (Assembly assembly, string name) {
			string manifestResourceName = typeof (EventViewer).Namespace
				+ ".Resources." + name;
			Stream s = assembly.GetManifestResourceStream (manifestResourceName);
			if (s == null) {
				throw new ArgumentException (string.Format (CultureInfo.InvariantCulture,
					"Icon '{0}' does not exist.", name));
			}
			return new Bitmap (s);
		}

		private void entryList_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e) {
			// The listview will be re-sorted every time a new 
			// ListViewItemSorter is set, and everytime a new
			// sort order is set.
			//
			// To avoid sorting twice we avoid setting a new ListItemSorter
			// unless necessary.

			SortOrder newSortOrder = entryList.Sorting;

			// check whether clicked column is the same as last clicked column
			if (e.Column == _entryListSortColumn) {
				switch (entryList.Sorting) {
					case SortOrder.None:
						newSortOrder = SortOrder.Ascending;
						break;
					case SortOrder.Ascending:
						newSortOrder = SortOrder.Descending;
						break;
					case SortOrder.Descending:
						newSortOrder = SortOrder.Ascending;
						break;
				}
			} else {
				_entryListSortColumn = e.Column;
				if (entryList.Sorting == SortOrder.None)
					newSortOrder = SortOrder.Ascending;
			}

			entryList.BeginUpdate ();
			try {
				EventLogEntryComparer comparer = entryList.ListViewItemSorter as EventLogEntryComparer;
				if (comparer != null) {
					comparer.Column = e.Column;
					if (entryList.Sorting != newSortOrder) {
						// sorting will be automatically trigger by setting the
						// new sort order
						entryList.Sorting = newSortOrder;
					} else {
						// manually trigger sort
						entryList.Sort ();
					}
				} else {
					// here, we're forced to sort twice
					entryList.ListViewItemSorter = new EventLogEntryComparer (e.Column);
					entryList.Sorting = newSortOrder;
				}
			} finally {
				entryList.EndUpdate ();
			}
		}

		private void RefreshMainToolbar () {
			TreeNode selectedNode = logTree.SelectedNode;
			bool isMachineNode = (selectedNode.Parent == null);
			logPropertiesButton.Visible = !isMachineNode;
			refreshEntriesButton.Visible = !isMachineNode;
		}

		private void RefreshEventEntryList () {
			TreeNode selectedLogNode = logTree.SelectedNode;
			if (selectedLogNode == null)
				return;

			string logName = (string) selectedLogNode.Tag;
			string machineName = (string) selectedLogNode.Parent.Tag;
			DisplayEventEntries (logName, machineName);
			listStatusPanel.Text = " " + logName + "    " + entryList.Items.Count 
				+ " event(s)";
		}

		private void RefreshEventLogList () {
			entryList.BeginUpdate ();
			try {
				this.Cursor = Cursors.WaitCursor;
				// remove current items
				entryList.Items.Clear ();
				// remove current columns
				entryList.Columns.Clear ();
				// create necessary columns
				entryList.Columns.Add ("Name", 150, HorizontalAlignment.Left);
				entryList.Columns.Add ("Type", 80, HorizontalAlignment.Left);
				entryList.Columns.Add ("Description", 100, HorizontalAlignment.Left);
				entryList.Columns.Add ("Size", 200, HorizontalAlignment.Left);
				// add event logs
				TreeNode machineNode = logTree.Nodes [0];
				foreach (TreeNode logNode in machineNode.Nodes) {
					ListViewItem item = new ListViewItem (new string [] {
						logNode.Text, "Log" }, 3);
					item.Tag = logNode.Tag;
					entryList.Items.Add (item);
				}
			} finally {
				this.Cursor = Cursors.Default;
				entryList.EndUpdate ();
			}
		}

		private void mainToolbar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e) {
			if (e.Button == refreshEntriesButton)
				RefreshEventEntryList ();
		}

		private void logTree_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
			if (e.Button == MouseButtons.Right) {
				TreeNode clickedNode = logTree.GetNodeAt(new Point(e.X,e.Y));
				if (clickedNode != null) {
					// store the right-clicked node (this sucks!!)
					_rightClickedNode = clickedNode;

					bool isMachineNode = (clickedNode.Parent == null);
					connectToComputerMenuItem.Visible = isMachineNode;
					openLogFileMenuItem.Visible = true;
					treeContextMenuSeparator1.Visible = !isMachineNode;
					clearEventsMenuItem.Visible = !isMachineNode;
					refreshEntriesMenuItem.Visible = !isMachineNode;
					treeContextMenuSeparator2.Visible = !isMachineNode;
					logPropertiesMenuItem.Visible = !isMachineNode;
					treeContextMenuSeparator3.Visible = true;
					helpMenuItem.Visible = true;
					treeContextMenu.Show (logTree, logTree.PointToClient (TreeView.MousePosition));
				} else {
					foreach (MenuItem menuItem in treeContextMenu.MenuItems)
						menuItem.Visible = false;
				}
			}
		}

		private void clearEventsMenuItem_Click(object sender, System.EventArgs e) {
			if (_rightClickedNode == null)
				return;

			string logName = (string) _rightClickedNode.Tag;
			string machineName = (string) _rightClickedNode.Parent.Tag;
			using (EventLog eventLog = new EventLog (logName, machineName)) {
				eventLog.Clear ();
			}

			if (logTree.SelectedNode == _rightClickedNode)
				// only perform an actual refresh if the right-clicked node is
				// also the selected node
				RefreshEventEntryList ();
		}

		private void refreshEntriesMenuItem_Click(object sender, System.EventArgs e) {
			if (_rightClickedNode == null)
				return;
			if (logTree.SelectedNode == _rightClickedNode)
				// only perform an actual refresh if the right-clicked node is
				// also the selected node
				RefreshEventEntryList ();
		}

		private void entryList_DoubleClick(object sender, System.EventArgs e) {
			if (entryList.SelectedItems.Count == 0)
				return;

			ListViewItem selectedItem = entryList.SelectedItems [0];

			if (logTree.SelectedNode.Parent == null) {
				string logName = (string) selectedItem.Tag;
				// locate corresponding tree node
				foreach (TreeNode node in logTree.Nodes [0].Nodes) {
					if ((string) node.Tag == logName) {
						logTree.SelectedNode = node;
						break;
					}
				}
			} else {
				EventEntryView selectedEntry = (EventEntryView) selectedItem.Tag;
				if (_eventProperties == null || _eventProperties.IsDisposed)
					_eventProperties = new EventEntryProperties ();
				_eventProperties.DisplayEventEntry (selectedEntry);
				_eventProperties.Activate ();
				_eventProperties.Show ();
			}
		}
	}
}
