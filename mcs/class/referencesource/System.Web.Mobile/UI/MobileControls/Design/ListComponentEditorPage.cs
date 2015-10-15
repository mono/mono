//------------------------------------------------------------------------------
// <copyright file="ListComponentEditorPage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Diagnostics;
    using System.Web.UI.MobileControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    using System.Web.UI.Design.MobileControls.Util;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal abstract class ListComponentEditorPage : MobileComponentEditorPage
    {
        protected bool CaseSensitive;
        protected EditableTreeList TreeList = null;
        protected ListTreeNode CurrentNode = null;
        protected String TreeViewTitle = String.Empty;
        protected String AddButtonTitle = String.Empty;
        protected String DefaultName = String.Empty;
        protected String MessageTitle = String.Empty;
        protected String EmptyNameMessage = String.Empty;
        // protected String DuplicateNameMessage = String.Empty; // AUI 2292
        // protected String InvalidNameMessage = String.Empty; // AUI 4240
        // private bool _newLabelSetDirty = true;  // AUI 4452

        protected int Y = 16;
        protected static readonly int X = 238;
        protected static readonly int ControlWidth = 152;
        protected static readonly int LabelHeight  = 16;
        protected static readonly int CellSpace    = 27;
        protected static readonly int Index        = 200;
        protected static readonly int CmbHeight    = 20;

        protected virtual bool FilterIllegalName()
        {
            return true;
        }

        protected virtual void InitForm()
        {
            TreeList = new EditableTreeList(true, CaseSensitive, Y);
            TreeList.TabIndex = 0;
            TreeList.LblTitle.Text = TreeViewTitle;
            TreeList.BtnAdd.Text = AddButtonTitle;

            TreeList.TvList.AfterLabelEdit += new NodeLabelEditEventHandler(OnAfterLabelEdit);
            TreeList.TvList.BeforeLabelEdit += new NodeLabelEditEventHandler(OnBeforeLabelEdit);
            TreeList.TvList.AfterSelect += new TreeViewEventHandler(OnNodeSelected);
            TreeList.BtnAdd.Click += new EventHandler(OnClickAddButton);
            TreeList.BtnRemove.Click += new EventHandler(OnClickRemoveButton);
            TreeList.BtnUp.Click += new EventHandler(OnClickUpButton);
            TreeList.BtnDown.Click += new EventHandler(OnClickDownButton);

            this.Controls.AddRange(new Control[] {TreeList /*, grplblProperties*/});
        }

        protected virtual void InitPage()
        {
            TreeList.TvList.Nodes.Clear();
            TreeList.TvList.SelectedNode = null;
        }

        private void InitTree()
        {
            LoadItems();
            if (TreeList.TvList.Nodes.Count > 0)
            {
                CurrentNode = (ListTreeNode)TreeList.TvList.Nodes[0];
                TreeList.TvList.SelectedNode = CurrentNode;
                LoadItemProperties();
            }
        }

        /// <summary>
        ///   Loads the component into the page.
        /// </summary>
        /// <seealso class="System.ComponentModel.ComponentEditorPage"/>
        protected override sealed void LoadComponent() 
        {
            InitPage();
            InitTree();

            UpdateControlsEnabling();
        }

        protected abstract void LoadItems();
        protected abstract void LoadItemProperties();

        protected override void SaveComponent()
        {
            foreach (ListTreeNode node in TreeList.TvList.Nodes)
            {
                if (node.IsEditing)
                {
                    // commit changes if still in editing mode.
                    node.EndEdit(false);
                }
            }
        }

        public override sealed void SetComponent(IComponent component)
        {
            base.SetComponent(component);

            Debug.Assert (component is ObjectList | component is List | 
                component is SelectionList, "Invalid Component");

            InitForm();
        }

        protected virtual String GetNewName()
        {
            int i = 1;
            while (NameExists(DefaultName + i.ToString(CultureInfo.InvariantCulture)))
            {
                i++;
            }
            return DefaultName + i.ToString(CultureInfo.InvariantCulture);
        }

        protected bool NameExists(String name)
        {
            foreach (ListTreeNode node in TreeList.TvList.Nodes)
            {
                if (String.Compare(node.Name, name, ((!CaseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void OnAfterLabelEdit(Object source, NodeLabelEditEventArgs e)
        {
            Debug.Assert(null != e);
            Debug.Assert(e.CancelEdit == false);

            // this happens when the label is unchanged after entering and exiting
            // label editing mode - bizarre behavior. this may be a 
            if (null == e.Label)
            {
                return;
            }

            if (FilterIllegalName())
            {
                bool cancel = true;

/* AUI 2292
                if (String.Compare(e.Node.Text, e.Label, true) != 0 && NameExists(e.Label))
                {
                    MessageBox.Show(
                        String.Format(DuplicateNameMessage,  e.Label),
                        MessageTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
*/
                // can't accept an empty node name
                if (e.Label.Length == 0)
                {
                    MessageBox.Show(
                        EmptyNameMessage, 
                        MessageTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
                /* Removed for DCR 4240
                // can't accept an illegal node name
                else if (!IsValidName(e.Label))
                {
                    MessageBox.Show(
                        InvalidNameMessage,
                        MessageTitle, 
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
                */
                else
                {
                    cancel = false;
                }

                if (cancel)
                {
                    e.CancelEdit = true;
                    return;
                }
            }

            CurrentNode.Name = e.Label;
            CurrentNode.Dirty = true;

            SetDirty();

            /* pulled out because of 4452
            if (_newLabelSetDirty)
            {
                SetDirty();
            }
            */

            OnNodeRenamed();
        }

        private void OnBeforeLabelEdit(Object source, NodeLabelEditEventArgs e)
        {
            SetDirty();
        }

        private void OnClickDownButton(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            SetDirty();
        }

        private void OnClickUpButton(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            SetDirty();
        }

        protected virtual void OnNodeSelected(Object source, TreeViewEventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            CurrentNode = (ListTreeNode) TreeList.TvList.SelectedNode;

            LoadItemProperties();
            UpdateControlsEnabling();
        }

        protected virtual void OnPropertyChanged(Object source, EventArgs e)
        {
        }

        protected virtual void OnClickAddButton(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            SetDirty();
        }

        protected virtual void OnClickRemoveButton(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            if (TreeList.TvList.Nodes.Count == 0)
            {
                CurrentNode = null;
                LoadItemProperties();
            }

            SetDirty();
            UpdateControlsEnabling();
        }

        protected virtual void UpdateControlsEnabling()
        {
        }

        protected virtual void OnNodeRenamed()
        {
        }

        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        protected class ListTreeNode : TreeNode
        {
            private bool    _dirty;
            private String  _name;

            internal ListTreeNode(String text) : base(text) 
            {
                this._name  = text;
            }

            internal bool Dirty
            {
                get
                {
                    return _dirty;
                }
                set
                {
                    _dirty = value;
                }
            }

            internal new String Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }
        }
    }
}
