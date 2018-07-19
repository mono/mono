//------------------------------------------------------------------------------
// <copyright file="EditableTreeList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Web.UI.Design.MobileControls;

    [
        ToolboxItem(false),
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class EditableTreeList : Panel
    {
        
        private const String _assertMsgNullNodeSelected =
            "Caller is responsible for ensuring a TreeNode is selected. "
            + "Modified TreeViewNode without calling UpdateButtonsEnabling()?";

        private const String _assertMsgOutOfBounds =
            "Caller is responsible for ensuring this action does not move the "
            + "selected TreeViewNode out of bounds. "
            + "Modified TvList without calling UpdateButtonsEnabling()?";

        internal TreeNode LastNodeChanged = null;
        internal TreeNode EditCandidateNode = null;
        internal EventHandler RemoveHandler;
        private bool _caseSensitive;

        internal System.Windows.Forms.Button BtnAdd;
        internal System.Windows.Forms.Button BtnRemove;
        internal System.Windows.Forms.Button BtnDown;
        internal System.Windows.Forms.Button BtnUp;
        internal System.Windows.Forms.TreeView TvList;
        internal System.Windows.Forms.Label LblTitle;
        internal System.Windows.Forms.ContextMenu CntxtMenu;
        internal System.Windows.Forms.MenuItem CntxtMenuItem;

        internal EditableTreeList() : this(true, true, 16)
        {
        }
        
        internal EditableTreeList(bool showAddButton, bool caseSensitive, int Y)
        {
            this.TvList = new System.Windows.Forms.TreeView();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.BtnDown = new System.Windows.Forms.Button();
            this.LblTitle = new System.Windows.Forms.Label();
            this.BtnUp = new System.Windows.Forms.Button();
            this.BtnRemove = new System.Windows.Forms.Button();
            this.CntxtMenuItem = new System.Windows.Forms.MenuItem();
            this.CntxtMenu = new System.Windows.Forms.ContextMenu();
            
            LblTitle.Size = new System.Drawing.Size(210, 16);
            LblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;;

            TvList.Location = new System.Drawing.Point(0, 16);
            TvList.Size = new System.Drawing.Size(178, 148);
            TvList.ForeColor = System.Drawing.SystemColors.WindowText;
            TvList.Anchor = AnchorStyles.Top
                | AnchorStyles.Bottom
                | AnchorStyles.Left
                | AnchorStyles.Right;
            TvList.LabelEdit = true;
            TvList.ShowPlusMinus = false;
            TvList.HideSelection = false;
            TvList.Indent = 15;
            TvList.ShowRootLines = false;
            TvList.ShowLines = false;
            TvList.ContextMenu = CntxtMenu;
            
            BtnUp.AccessibleName = SR.GetString(SR.EditableTreeList_MoveUpName);
            BtnUp.AccessibleDescription = SR.GetString(SR.EditableTreeList_MoveUpDescription);
            BtnUp.Name = SR.GetString(SR.EditableTreeList_MoveUpName);
            BtnUp.Location = new System.Drawing.Point(182, 16);
            BtnUp.Size = new System.Drawing.Size(28, 27);
            BtnUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;;
            
            BtnDown.AccessibleName = SR.GetString(SR.EditableTreeList_MoveDownName);
            BtnDown.AccessibleDescription = SR.GetString(SR.EditableTreeList_MoveDownDescription);
            BtnDown.Name = SR.GetString(SR.EditableTreeList_MoveDownName);
            BtnDown.Location = new System.Drawing.Point(182, 48);
            BtnDown.Size = new System.Drawing.Size(28, 27);
            BtnDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            
            BtnRemove.AccessibleName = SR.GetString(SR.EditableTreeList_DeleteName);
            BtnRemove.AccessibleDescription = SR.GetString(SR.EditableTreeList_DeleteDescription);
            BtnRemove.Name = SR.GetString(SR.EditableTreeList_DeleteName);
            BtnRemove.Location = new System.Drawing.Point(182, 136);
            BtnRemove.Size = new System.Drawing.Size(28, 27);
            BtnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            BtnAdd.AccessibleName = SR.GetString(SR.EditableTreeList_AddName);
            BtnAdd.AccessibleDescription = SR.GetString(SR.EditableTreeList_AddDescription);
            BtnAdd.Name = SR.GetString(SR.EditableTreeList_AddName);
            BtnAdd.Location = new System.Drawing.Point(0, 168);
            BtnAdd.Size = new System.Drawing.Size(178, 25);
            BtnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            CntxtMenuItem.Text = SR.GetString(SR.EditableTreeList_Rename);
            CntxtMenu.MenuItems.Add(CntxtMenuItem);

            this.Location = new System.Drawing.Point(8, Y);
            this.Size = new System.Drawing.Size(210, 196);
            
            this.Controls.Add(LblTitle);
            this.Controls.Add(TvList);
            this.Controls.Add(BtnUp);
            this.Controls.Add(BtnDown);
            this.Controls.Add(BtnRemove);
            this.Controls.Add(BtnAdd);

            BtnDown.Image = GenericUI.SortDownIcon;
            BtnUp.Image = GenericUI.SortUpIcon;
            BtnRemove.Image = GenericUI.DeleteIcon;

            BtnUp.Click += new EventHandler(MoveSelectedItemUp);
            BtnDown.Click += new EventHandler(MoveSelectedItemDown);
            RemoveHandler = new EventHandler(OnRemove);
            BtnRemove.Click += RemoveHandler;
            TvList.AfterSelect += new TreeViewEventHandler(OnListSelect);
            TvList.KeyDown += new KeyEventHandler(OnKeyDown);
            TvList.MouseUp += new MouseEventHandler(OnListMouseUp);
            TvList.MouseDown += new MouseEventHandler(OnListMouseDown);
            CntxtMenu.Popup += new EventHandler(OnPopup);
            CntxtMenuItem.Click += new EventHandler(OnContextMenuItemClick);
            
            UpdateButtonsEnabling();

            if(!showAddButton)
            {
                // stretch UI to occupy space where add button was.
                BtnAdd.Visible = false;
                int offset = 4 + BtnAdd.Height;
                TvList.Height += offset;
                BtnRemove.Top += offset;
            }
            _caseSensitive = caseSensitive;
        }

        ////////////////////////////////////////////////////////////////////////
        //  End Windes Generated
        ////////////////////////////////////////////////////////////////////////

        internal int SelectedIndex
        {
            get
            {
                TreeNode selectedNode = TvList.SelectedNode;
                if(selectedNode != null)
                {
                    return selectedNode.Index;
                }
                else
                {
                    return -1;
                }
            }
        }

        internal TreeNode SelectedNode
        {
            get
            {
                return TvList.SelectedNode;
            }
        }

        private TreeNode SelectedNodeChecked
        {
            get
            {
                TreeNode node = TvList.SelectedNode;
                Debug.Assert(
                    node != null,
                    _assertMsgNullNodeSelected
                );
                return node;
            }
        }

        private void MoveSelectedNode(int direction)
        {
            Debug.Assert(direction == 1 || direction == -1);
            LastNodeChanged = TvList.SelectedNode;
            Debug.Assert(
                LastNodeChanged != null,
                _assertMsgNullNodeSelected
            );
            int index = LastNodeChanged.Index;
            Debug.Assert(
                (index + direction >= 0)
                && ((index + direction) < TvList.Nodes.Count),
                _assertMsgOutOfBounds
            );
            TvList.Nodes.RemoveAt(index);
            TvList.Nodes.Insert(index + direction, LastNodeChanged);
            TvList.SelectedNode = LastNodeChanged;
        }
        
        internal void MoveSelectedItemUp(Object sender, EventArgs e)
        {
            MoveSelectedNode(-1);
            UpdateButtonsEnabling();
        }

        internal void MoveSelectedItemDown(Object sender, EventArgs e)
        {
            MoveSelectedNode(1);
            UpdateButtonsEnabling();
        }

        internal void RemoveSelectedItem()
        {
            LastNodeChanged = SelectedNodeChecked;
            TvList.Nodes.Remove(LastNodeChanged);
            UpdateButtonsEnabling();
        }

        private void OnKeyDown(Object sender, KeyEventArgs e)
        {
            switch(e.KeyData)
            {
                case Keys.F2:
                {
                    TreeNode selectedNode = TvList.SelectedNode;
                    if(selectedNode != null)
                    {
                        selectedNode.BeginEdit();
                    }
                    break;
                }
                case (Keys.Control | Keys.Home):
                {
                    if(TvList.Nodes.Count > 0)
                    {
                        TvList.SelectedNode = TvList.Nodes[0];
                    }
                    break;
                }
                case (Keys.Control | Keys.End):
                {
                    int numNodes = TvList.Nodes.Count;
                    if(numNodes > 0)
                    {
                        TvList.SelectedNode = TvList.Nodes[numNodes - 1];
                    }
                    break;
                }
            }
        }

        private void OnRemove(Object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        private void OnListSelect(Object sender, TreeViewEventArgs e)
        {
            UpdateButtonsEnabling();
        }

        private void OnListMouseUp(Object sender, MouseEventArgs e)
        {
            EditCandidateNode= null;
            if (e.Button == MouseButtons.Right)
            {
                EditCandidateNode = (TreeNode)TvList.GetNodeAt (e.X, e.Y);
            }
        }

        private void OnListMouseDown(Object sender, MouseEventArgs e)
        {
            EditCandidateNode = null;
            if (e.Button == MouseButtons.Right)
            {
                EditCandidateNode = (TreeNode)TvList.GetNodeAt (e.X, e.Y);
            }
        }
        
        private void OnPopup(Object sender, EventArgs e)
        {
            CntxtMenuItem.Enabled = (EditCandidateNode != null || 
                                                    TvList.SelectedNode != null);
        }

        private void OnContextMenuItemClick(Object sender, EventArgs e)
        {
            if(EditCandidateNode == null)
            {
               // context menu key pressed
               if (TvList.SelectedNode!=null)
               {
                    TvList.SelectedNode.BeginEdit();
               }
            }
            else
            {
                // right mouse-click
                EditCandidateNode.BeginEdit();
            }
            EditCandidateNode = null;
        }
        
        internal String GetUniqueLabel(String label)
        {
            int index = 1;
            String uniqueLabel = label + index;
            while(LabelExists(uniqueLabel))
            {
                uniqueLabel = label + (++index);
            }
            return uniqueLabel;
        }

        internal bool LabelExists(String label)
        {
            foreach(TreeNode node in TvList.Nodes)
            {
                if(String.Compare(node.Text, label, ((!_caseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal void UpdateButtonsEnabling()
        {
            int selectedIndex = SelectedIndex;
            bool anItemIsSelected = (selectedIndex >= 0);

            BtnRemove.Enabled = anItemIsSelected;
            if (anItemIsSelected)
            {
                BtnUp.Enabled = (selectedIndex > 0);
                BtnDown.Enabled = (selectedIndex < TvList.Nodes.Count - 1);
            }
            else
            {
                BtnUp.Enabled = false;
                BtnDown.Enabled = false;
            }
        }
    }
}
