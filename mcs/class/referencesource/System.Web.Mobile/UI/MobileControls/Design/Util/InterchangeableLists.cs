//------------------------------------------------------------------------------
// <copyright file="InterchangeableLists.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#if UNUSED_CODE

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Diagnostics;
    using System.Web.UI.Design.MobileControls;
    using System.Windows.Forms;

    [
        ToolboxItem(false),
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    internal sealed class InterchangeableLists : System.Windows.Forms.Panel
    {
        private System.Windows.Forms.Button _removeButton;
        private System.Windows.Forms.Button _addButton;
        private System.Windows.Forms.Button _upButton;
        private System.Windows.Forms.TreeView _availableList;
        private System.Windows.Forms.Label _availableFieldLabel;
        private System.Windows.Forms.TreeView _selectedList;
        private System.Windows.Forms.Button _downButton;
        private System.Windows.Forms.Label _selectedFieldLabel;

        private Hashtable _eventTable;

        /// <summary>
        ///    Required designer variable.
        /// </summary>
        private static readonly Object _componentChangedEvent = new Object();

        internal InterchangeableLists()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitForm call
            _downButton.Image = GenericUI.SortDownIcon;
            _upButton.Image = GenericUI.SortUpIcon;
            UpdateButtonEnabling();

            this._eventTable = new Hashtable();
        }

        internal void SetTitles(
            String availableListTitle,
            String selectedListTitle)
        {
            this._selectedFieldLabel.Text = selectedListTitle;
            this._availableFieldLabel.Text = availableListTitle;
        }

        internal void AddToAvailableList(Object obj)
        {
            AddItem(_availableList, new TreeNode(obj.ToString()));
        }

        internal void AddToSelectedList(Object obj)
        {
            AddItem(_selectedList, new TreeNode(obj.ToString()));
        }

        internal void Initialize()
        {
            if (_availableList.Nodes.Count > 0)
            {
                _availableList.SelectedNode = _availableList.Nodes[0];
            }
            if (_selectedList.Nodes.Count > 0)
            {
                _selectedList.SelectedNode = _selectedList.Nodes[0];
            }
        }

        internal EventHandler OnComponentChanged
        {
            get 
            {
                return (EventHandler)_eventTable[_componentChangedEvent];
            }
            set 
            {
                _eventTable[_componentChangedEvent] = value;
            }
        }

        private void NotifyChangeEvent()
        {
            EventHandler handler = (EventHandler)_eventTable[_componentChangedEvent];
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        internal void Clear()
        {
            _availableList.Nodes.Clear();
            _selectedList.Nodes.Clear();
        }

        internal ICollection GetSelectedItems()
        {
            ArrayList list = new ArrayList();

            foreach (TreeNode node in _selectedList.Nodes)
            {
                list.Add(node.Text);
            }
            return list;
        }

        /// <summary>
        ///    Required method for Designer support - do not modify 
        ///    the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._removeButton = new System.Windows.Forms.Button();
            this._selectedFieldLabel = new System.Windows.Forms.Label();
            this._addButton = new System.Windows.Forms.Button();
            this._selectedList = new System.Windows.Forms.TreeView();
            this._availableList = new System.Windows.Forms.TreeView();
            this._availableFieldLabel = new System.Windows.Forms.Label();
            this._upButton = new System.Windows.Forms.Button();
            this._downButton = new System.Windows.Forms.Button();
            this._removeButton.Location = new System.Drawing.Point(166, 69);
            this._removeButton.Size = new System.Drawing.Size(32, 25);
            this._removeButton.TabIndex = 4;
            this._removeButton.Text = "<";
            this._removeButton.Click += new System.EventHandler(this.RemoveNode);
            this._removeButton.AccessibleName = SR.GetString(SR.EditableTreeList_DeleteName);
            this._removeButton.AccessibleDescription = SR.GetString(SR.EditableTreeList_DeleteDescription);
            this._removeButton.Name = SR.GetString(SR.EditableTreeList_DeleteName);

            this._selectedFieldLabel.Location = new System.Drawing.Point(202, 8);
            this._selectedFieldLabel.Size = new System.Drawing.Size(164, 16);
            this._selectedFieldLabel.TabIndex = 5;

            this._addButton.AccessibleName = SR.GetString(SR.EditableTreeList_AddName);
            this._addButton.AccessibleDescription = SR.GetString(SR.EditableTreeList_AddDescription);
            this._addButton.Name = SR.GetString(SR.EditableTreeList_AddName);
            this._addButton.Location = new System.Drawing.Point(166, 40);
            this._addButton.Size = new System.Drawing.Size(32, 25);
            this._addButton.TabIndex = 3;
            this._addButton.Text = ">";
            this._addButton.Click += new System.EventHandler(this.AddNode);

            this._selectedList.HideSelection = false;
            this._selectedList.Indent = 15;
            this._selectedList.Location = new System.Drawing.Point(202, 24);
            this._selectedList.ShowLines = false;
            this._selectedList.ShowPlusMinus = false;
            this._selectedList.ShowRootLines = false;
            this._selectedList.Size = new System.Drawing.Size(154, 89);
            this._selectedList.TabIndex = 6;
            this._selectedList.DoubleClick += new System.EventHandler(this.RemoveNode);
            this._selectedList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SelectedList_AfterSelect);
            this._availableList.HideSelection = false;
            this._availableList.Indent = 15;
            this._availableList.Location = new System.Drawing.Point(8, 24);
            this._availableList.ShowLines = false;
            this._availableList.ShowPlusMinus = false;
            this._availableList.ShowRootLines = false;
            this._availableList.Size = new System.Drawing.Size(154, 89);
            this._availableList.TabIndex = 2;
            this._availableList.DoubleClick += new System.EventHandler(this.AddNode);
            this._availableList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.AvailableList_AfterSelect);
            this._availableFieldLabel.Location = new System.Drawing.Point(8, 8);
            this._availableFieldLabel.Size = new System.Drawing.Size(164, 16);
            this._availableFieldLabel.TabIndex = 1;

            this._upButton.AccessibleName = SR.GetString(SR.EditableTreeList_MoveUpName);
            this._upButton.AccessibleDescription = SR.GetString(SR.EditableTreeList_MoveUpDescription);
            this._upButton.Name = SR.GetString(SR.EditableTreeList_MoveUpName);
            this._upButton.Location = new System.Drawing.Point(360, 24);
            this._upButton.Size = new System.Drawing.Size(28, 27);
            this._upButton.TabIndex = 7;
            this._upButton.Click += new System.EventHandler(this.Up_Click);

            this._downButton.AccessibleName = SR.GetString(SR.EditableTreeList_MoveDownName);
            this._downButton.AccessibleDescription = SR.GetString(SR.EditableTreeList_MoveDownDescription);
            this._downButton.Name = SR.GetString(SR.EditableTreeList_MoveDownName);
            this._downButton.Location = new System.Drawing.Point(360, 55);
            this._downButton.Size = new System.Drawing.Size(28, 27);
            this._downButton.TabIndex = 8;
            this._downButton.Click += new System.EventHandler(this.Down_Click);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {this._availableFieldLabel,
                                                                          this._selectedFieldLabel,
                                                                          this._upButton,
                                                                          this._downButton,
                                                                          this._removeButton,
                                                                          this._selectedList,
                                                                          this._addButton,
                                                                          this._availableList});
            this.Size = new System.Drawing.Size(396, 119);

        }

        private void UpdateButtonEnabling()
        {
            bool anAvailableItemIsSelected = 
                (_availableList.SelectedNode != null);

            bool anSelectedItemIsSelected = 
                (_selectedList.SelectedNode != null);

            _addButton.Enabled = anAvailableItemIsSelected;
            _removeButton.Enabled = anSelectedItemIsSelected;

            if (anSelectedItemIsSelected)
            {
                int selectedIndex = _selectedList.SelectedNode.Index;
                _upButton.Enabled = (selectedIndex > 0);
                _downButton.Enabled = 
                    (selectedIndex < _selectedList.Nodes.Count - 1);
            }
            else
            {
                _downButton.Enabled = false;
                _upButton.Enabled = false;
            }
        }

        private void AddNode(object sender, System.EventArgs e)
        {
            TreeNode selectedNode = _availableList.SelectedNode;
            if (selectedNode != null)
            {
                RemoveItem(_availableList, selectedNode);
                AddItem(_selectedList, selectedNode);

                UpdateButtonEnabling();
                NotifyChangeEvent();
            }
        }

        private void RemoveItem(TreeView list, TreeNode node)
        {
            Debug.Assert (list.Nodes.Contains(node));

            int itemCount = list.Nodes.Count;
            int selectedIndex = list.SelectedNode.Index;

            list.Nodes.Remove(node);

            if (selectedIndex < itemCount - 1)
            {
                list.SelectedNode = list.Nodes[selectedIndex];
            }
            else if (selectedIndex >= 1)
            {
                list.SelectedNode = list.Nodes[selectedIndex-1];
            }
            else
            {
                Debug.Assert(itemCount == 1);
                list.SelectedNode = null;
            }
        }

        private void AddItem(TreeView list, TreeNode node)
        {
            Debug.Assert(node != null);

            list.Nodes.Add(node);
            list.SelectedNode = node;
            //_selectedList.Select();
        }

        private void RemoveNode(object sender, System.EventArgs e)
        {
            TreeNode selectedNode = _selectedList.SelectedNode;
            if (selectedNode != null)
            {
                RemoveItem(_selectedList, selectedNode);
                AddItem(_availableList, selectedNode);

                UpdateButtonEnabling();
            }

            //_availableList.Select();
            NotifyChangeEvent();
        }

        private void MoveItem(
            int direction /* 1 is up, -1 is down */)
        {
            Debug.Assert(direction == -1 || direction == 1);
            
            int selectedIndex = _selectedList.SelectedNode.Index;
            int newIndex = selectedIndex + direction;

            TreeNode node = _selectedList.SelectedNode;
            _selectedList.Nodes.RemoveAt(selectedIndex);
            _selectedList.Nodes.Insert(newIndex, node);
            _selectedList.SelectedNode = node;
        }

        private void Up_Click(object sender, System.EventArgs e)
        {
            MoveItem(-1);

            UpdateButtonEnabling();
            //_selectedList.Select();
            NotifyChangeEvent();
        }

        private void Down_Click(object sender, System.EventArgs e)
        {
            MoveItem(+1);

            UpdateButtonEnabling();
            //_selectedList.Select();
            NotifyChangeEvent();
        }

        private void AvailableList_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            UpdateButtonEnabling();
        }

        private void SelectedList_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            UpdateButtonEnabling();
        }
    }
}
#endif