//------------------------------------------------------------------------------
// <copyright file="ListItemsPage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
//    using System.Web.UI.Design.Util;

    using System.Web.UI.MobileControls;
    using System.Web.UI.Design.MobileControls.Util;

    using Button     = System.Windows.Forms.Button;
    using Label      = System.Windows.Forms.Label;
    using TextBox    = System.Windows.Forms.TextBox;
    using CheckBox   = System.Windows.Forms.CheckBox;
    using TreeView   = System.Windows.Forms.TreeView;

    /// <summary>
    ///   The Items page for the List control.
    /// </summary>
    /// <internalonly/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class ListItemsPage : ListComponentEditorPage
    {
        private IListDesigner _listDesigner;
        private CheckBox      _itemsAsLinksCheckBox;
        private TextBox       _txtValue;
        private CheckBox      _ckbSelected;
        private bool          _isBaseControlList;

        public ListItemsPage()
        {
            TreeViewTitle  = SR.GetString(SR.ListItemsPage_ItemCaption);
            AddButtonTitle = SR.GetString(SR.ListItemsPage_NewItemCaption);
            DefaultName    = SR.GetString(SR.ListItemsPage_DefaultItemText);
        }

        protected override String HelpKeyword 
        {
            get 
            {
                if (_isBaseControlList)
                {
                    return "net.Mobile.ListProperties.Items";
                }
                else
                {
                    return "net.Mobile.SelectionListProperties.Items";
                }
            }
        }

        protected override bool FilterIllegalName()
        {
            return false;
        }

        protected override String GetNewName()
        {
            return SR.GetString(SR.ListItemsPage_DefaultItemText);
        }

        protected override void InitForm()
        {
            Debug.Assert(GetBaseControl() != null);
            _isBaseControlList = (GetBaseControl() is List);
            this._listDesigner = (IListDesigner)GetBaseDesigner();

            Y = (_isBaseControlList ? 52 : 24);

            base.InitForm();

            this.Text = SR.GetString(SR.ListItemsPage_Title);
            this.CommitOnDeactivate = true;
            this.Icon = new Icon(
                typeof(System.Web.UI.Design.MobileControls.MobileControlDesigner),
                "Items.ico"
            );
            this.Size = new Size(382, 220);

            if (_isBaseControlList)
            {
                _itemsAsLinksCheckBox = new CheckBox();
                _itemsAsLinksCheckBox.SetBounds(4, 4, 370, 16);
                _itemsAsLinksCheckBox.Text = SR.GetString(SR.ListItemsPage_ItemsAsLinksCaption);
                _itemsAsLinksCheckBox.FlatStyle = FlatStyle.System;
                _itemsAsLinksCheckBox.CheckedChanged += new EventHandler(this.OnSetPageDirty);
                _itemsAsLinksCheckBox.TabIndex = 0;
            }

            GroupLabel grplblItemList = new GroupLabel();
            grplblItemList.SetBounds(4, _isBaseControlList ? 32 : 4, 372, LabelHeight);
            grplblItemList.Text = SR.GetString(SR.ListItemsPage_ItemListGroupLabel);
            grplblItemList.TabIndex = 1;
            grplblItemList.TabStop = false;

            TreeList.TabIndex = 2;

            Label lblValue = new Label();
            lblValue.SetBounds(X, Y, 134, LabelHeight);
            lblValue.Text = SR.GetString(SR.ListItemsPage_ItemValueCaption);
            lblValue.TabStop = false;
            lblValue.TabIndex = Index;

            Y += LabelHeight;
            _txtValue = new TextBox();
            _txtValue.SetBounds(X, Y, 134, CmbHeight);
            _txtValue.TextChanged += new EventHandler(this.OnPropertyChanged);
            _txtValue.TabIndex = Index + 1;

            this.Controls.AddRange(new Control[] 
                                    {
                                        grplblItemList,
                                        lblValue,
                                        _txtValue
                                    });

            if (_isBaseControlList)
            {
                this.Controls.Add(_itemsAsLinksCheckBox);
            }
            else
            {
                Y += CellSpace;
                _ckbSelected = new CheckBox();
                _ckbSelected.SetBounds(X, Y, 134, LabelHeight);
                _ckbSelected.FlatStyle = System.Windows.Forms.FlatStyle.System;
                _ckbSelected.Text = SR.GetString(SR.ListItemsPage_ItemSelectedCaption); 
                _ckbSelected.CheckedChanged += new EventHandler(this.OnPropertyChanged);
                _ckbSelected.TabIndex = Index + 2;
                this.Controls.Add(_ckbSelected);
            }
        }

        protected override void InitPage() 
        {
            base.InitPage();

            if (_isBaseControlList)
            {
                List list = (List)GetBaseControl();
                _itemsAsLinksCheckBox.Checked = list.ItemsAsLinks;
            }
            else
            {
                _ckbSelected.Checked = false;
            }
            _txtValue.Text = String.Empty;
        }

        protected override void LoadItems()
        {
            using (new LoadingModeResource(this))
            {
                foreach (MobileListItem item in _listDesigner.Items)
                {
                    ItemTreeNode newNode = new ItemTreeNode(item);
                    TreeList.TvList.Nodes.Add(newNode);
                }
            }
        }

        protected override void LoadItemProperties() 
        {
            using (new LoadingModeResource(this))
            {
                if (CurrentNode != null)
                {
                    ItemTreeNode currentItemNode = (ItemTreeNode)CurrentNode;
                    _txtValue.Text = currentItemNode.Value;
                    if (!_isBaseControlList)
                    {
                        _ckbSelected.Checked = currentItemNode.Selected;
                    }
                }
                else
                {
                    _txtValue.Text = String.Empty;
                    if (!_isBaseControlList)
                    {
                        _ckbSelected.Checked = false;
                    }
                }
            }
        }

        protected override void OnAfterLabelEdit(Object source, NodeLabelEditEventArgs e)
        {
            base.OnAfterLabelEdit(source, e);

            if (!((ItemTreeNode)CurrentNode).ValueSet)
            {
                _txtValue.Text = ((ItemTreeNode)CurrentNode).Value = CurrentNode.Name;
            }
        }

        protected override void OnClickAddButton(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            ItemTreeNode newNode = new ItemTreeNode(GetNewName());
            TreeList.TvList.Nodes.Add(newNode);

            TreeList.TvList.SelectedNode = newNode;
            CurrentNode = newNode;
            newNode.Dirty = true;
            newNode.BeginEdit();

            LoadItemProperties();

            SetDirty();
        }

        private void OnSetPageDirty(Object source, EventArgs e) 
        {
            if (IsLoading())
            {
                return;
            }
            SetDirty();
        }

        protected override void OnPropertyChanged(Object source, EventArgs e)
        {
            // This means there are no fields yet. Do nothing
            if (CurrentNode == null || IsLoading())
            {
                return;
            }

            if (source is TextBox)
            {
                ((ItemTreeNode)CurrentNode).Value = _txtValue.Text;
            }
            else
            {
                Debug.Assert(!_isBaseControlList);
                ((ItemTreeNode)CurrentNode).Selected = _ckbSelected.Checked;
            }

            SetDirty();
            CurrentNode.Dirty = true;
        }

        protected override void SaveComponent()
        {
            // Delegate to base implementation first!
            // This will properly close ListTreeNode editing mode.
            base.SaveComponent();

            _listDesigner.Items.Clear();

            foreach (ItemTreeNode itemNode in TreeList.TvList.Nodes)
            {
                if (itemNode.Dirty)
                {
                    itemNode.RuntimeItem.Text  = itemNode.Text;
                    itemNode.RuntimeItem.Value = itemNode.Value;
                    if (!_isBaseControlList)
                    {
                        itemNode.RuntimeItem.Selected = itemNode.Selected;
                    }
                }

                _listDesigner.Items.Add(itemNode.RuntimeItem);
            }

            if (_isBaseControlList)
            {
                List list = (List)GetBaseControl();
                list.ItemsAsLinks = _itemsAsLinksCheckBox.Checked;
                TypeDescriptor.Refresh(list);
            }
            else
            {
                SelectionList selectionList = (SelectionList)GetBaseControl();
                TypeDescriptor.Refresh(selectionList);
            }
        }

        protected override void UpdateControlsEnabling()
        {
            if (TreeList.TvList.SelectedNode == null)
            {
                TreeList.TvList.Enabled = _txtValue.Enabled = false;
                _txtValue.Text = String.Empty;
            }
            else
            {
                TreeList.TvList.Enabled = _txtValue.Enabled = true;
            }

            if (!_isBaseControlList)
            {
                SelectionList selectionListControl = (SelectionList) GetBaseControl();
                if (TreeList.TvList.SelectedNode == null)
                {
                    _ckbSelected.Enabled = false;
                    _ckbSelected.Checked = false;
                }
                else
                {
                    _ckbSelected.Enabled = true;
                }
            }
        }

        /// <summary>
        ///    Internal object used to store all items properties
        /// </summary>
        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class ItemTreeNode : ListTreeNode
        {
            private MobileListItem _runtimeItem;
            private String _value;
            private bool _selected;
            private bool _valueSet = false;

            /// <summary>
            /// </summary>
            internal ItemTreeNode(String itemText) : base(itemText)
            {
                this._runtimeItem = new MobileListItem();
                this._value = null;
                this._selected = false;
            }

            /// <summary>
            /// </summary>
            internal ItemTreeNode(MobileListItem runtimeItem) : base(runtimeItem.Text)
            {
                Debug.Assert(runtimeItem != null, "runtimeItem is null");

                _valueSet = true;
                this._runtimeItem = runtimeItem;
                this._value = _runtimeItem.Value;
                this._selected = _runtimeItem.Selected;
            }

            internal MobileListItem RuntimeItem
            {
                get
                {
                    return _runtimeItem;
                }
            }

            internal String Value
            {
                get
                {
                    return _value;
                }

                set
                {
                    _value = value;
                    _valueSet = true;
                }
            }

            internal bool Selected
            {
                get
                {
                    return _selected;
                }

                set
                {
                    _selected = value;
                }
            }

            internal bool ValueSet
            {
                get
                {
                    return _valueSet;
                }
            }
        }
    }
}
