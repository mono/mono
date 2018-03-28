//------------------------------------------------------------------------------
// <copyright file="ListGeneralPage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Globalization;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.MobileControls;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    using System.Web.UI.Design.MobileControls.Util;

    using DataBinding = System.Web.UI.DataBinding;    
    using DataList = System.Web.UI.WebControls.DataList;

    using TextBox = System.Windows.Forms.TextBox;
    using CheckBox = System.Windows.Forms.CheckBox;
    using ComboBox = System.Windows.Forms.ComboBox;
    using Control = System.Windows.Forms.Control;
    using Label = System.Windows.Forms.Label;
    using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

    /// <summary>
    ///   The General page for the DataList control.
    /// </summary>
    /// <internalonly/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ListGeneralPage : MobileComponentEditorPage 
    {
        private const int IDX_DECORATION_NONE = 0;
        private const int IDX_DECORATION_BULLETED = 1;
        private const int IDX_DECORATION_NUMBERED = 2;

        private const int IDX_SELECTTYPE_DROPDOWN = 0;
        private const int IDX_SELECTTYPE_LISTBOX = 1;
        private const int IDX_SELECTTYPE_RADIO = 2;
        private const int IDX_SELECTTYPE_MULTISELECTLISTBOX = 3;
        private const int IDX_SELECTTYPE_CHECKBOX = 4;

        private ComboBox _decorationCombo;
        private ComboBox _selectTypeCombo;
        private TextBox _itemCountTextBox;
        private TextBox _itemsPerPageTextBox;
        private TextBox _rowsTextBox;

        private bool _isBaseControlList;

        protected override String HelpKeyword 
        {
            get 
            {
                if (_isBaseControlList)
                {
                    return "net.Mobile.ListProperties.General";
                }
                else
                {
                    return "net.Mobile.SelectionListProperties.General";
                }
            }
        }

        /// <summary>
        ///   Initializes the UI of the form.
        /// </summary>
        private void InitForm() 
        {
            Debug.Assert(GetBaseControl() != null);
            _isBaseControlList = (GetBaseControl() is List);   // SelectionList otherwise.

            GroupLabel appearanceGroup = new GroupLabel();
            GroupLabel pagingGroup = null;
            Label itemCountLabel = null;
            Label itemsPerPageLabel = null;
            Label rowsLabel = null;
            Label decorationLabel = null;
            Label selectTypeLabel = null;

            if (_isBaseControlList)
            {
                pagingGroup = new GroupLabel();
                itemCountLabel = new Label();
                _itemCountTextBox = new TextBox();
                itemsPerPageLabel = new Label();
                _itemsPerPageTextBox = new TextBox();
                decorationLabel = new Label();
                _decorationCombo = new ComboBox();
            }
            else
            {
                rowsLabel = new Label();
                _rowsTextBox = new TextBox();
                selectTypeLabel = new Label();
                _selectTypeCombo = new ComboBox();
            }

            appearanceGroup.SetBounds(4, 4, 372, 16);
            appearanceGroup.Text = SR.GetString(SR.ListGeneralPage_AppearanceGroupLabel);
            appearanceGroup.TabIndex = 0;
            appearanceGroup.TabStop = false;
            
            if (_isBaseControlList)
            {
                decorationLabel.SetBounds(8, 24, 200, 16);
                decorationLabel.Text = SR.GetString(SR.ListGeneralPage_DecorationCaption);
                decorationLabel.TabStop = false;
                decorationLabel.TabIndex = 1;

                _decorationCombo.SetBounds(8, 40, 161, 21);
                _decorationCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                _decorationCombo.SelectedIndexChanged += new EventHandler(this.OnSetPageDirty);
                _decorationCombo.Items.AddRange(new object[] {
                                                               SR.GetString(SR.ListGeneralPage_DecorationNone),
                                                               SR.GetString(SR.ListGeneralPage_DecorationBulleted),
                                                               SR.GetString(SR.ListGeneralPage_DecorationNumbered)
                                                             });
                _decorationCombo.TabIndex = 2;

                pagingGroup.SetBounds(4, 77, 372, 16);
                pagingGroup.Text = SR.GetString(SR.ListGeneralPage_PagingGroupLabel);
                pagingGroup.TabIndex = 3;
                pagingGroup.TabStop = false;
            
                itemCountLabel.SetBounds(8, 97, 161, 16);
                itemCountLabel.Text = SR.GetString(SR.ListGeneralPage_ItemCountCaption);
                itemCountLabel.TabStop = false;
                itemCountLabel.TabIndex = 4;

                _itemCountTextBox.SetBounds(8, 113, 161, 20);
                _itemCountTextBox.TextChanged += new EventHandler(this.OnSetPageDirty);
                _itemCountTextBox.KeyPress += new KeyPressEventHandler(this.OnKeyPressNumberTextBox);
                _itemCountTextBox.TabIndex = 5;
            
                itemsPerPageLabel.SetBounds(211, 97, 161, 16);
                itemsPerPageLabel.Text = SR.GetString(SR.ListGeneralPage_ItemsPerPageCaption);
                itemsPerPageLabel.TabStop = false;
                itemsPerPageLabel.TabIndex = 6;

                _itemsPerPageTextBox.SetBounds(211, 113, 161, 20);
                _itemsPerPageTextBox.TextChanged += new EventHandler(this.OnSetPageDirty);
                _itemsPerPageTextBox.KeyPress += new KeyPressEventHandler(this.OnKeyPressNumberTextBox);
                _itemsPerPageTextBox.TabIndex = 7;
            }
            else
            {
                selectTypeLabel.SetBounds(8, 24, 161, 16);
                selectTypeLabel.Text = SR.GetString(SR.ListGeneralPage_SelectTypeCaption);
                selectTypeLabel.TabStop = false;
                selectTypeLabel.TabIndex = 1;

                _selectTypeCombo.SetBounds(8, 40, 161, 21);
                _selectTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                _selectTypeCombo.SelectedIndexChanged += new EventHandler(this.OnSetPageDirty);
                _selectTypeCombo.Items.AddRange(new object[] {
                                                                SR.GetString(SR.ListGeneralPage_SelectTypeDropDown),
                                                                SR.GetString(SR.ListGeneralPage_SelectTypeListBox),
                                                                SR.GetString(SR.ListGeneralPage_SelectTypeRadio),
                                                                SR.GetString(SR.ListGeneralPage_SelectTypeMultiSelectListBox),
                                                                SR.GetString(SR.ListGeneralPage_SelectTypeCheckBox)
                                                             });
                _selectTypeCombo.TabIndex = 2;

                rowsLabel.SetBounds(211, 24, 161, 16);
                rowsLabel.Text = SR.GetString(SR.ListGeneralPage_RowsCaption);
                rowsLabel.TabStop = false;
                rowsLabel.TabIndex = 3;

                _rowsTextBox.SetBounds(211, 40, 161, 20);
                _rowsTextBox.TextChanged += new EventHandler(this.OnSetPageDirty);
                _rowsTextBox.KeyPress += new KeyPressEventHandler(this.OnKeyPressNumberTextBox);
                _rowsTextBox.TabIndex = 4;
            }

            this.Text = SR.GetString(SR.ListGeneralPage_Title);
            this.Size = new Size(382, 270);
            this.CommitOnDeactivate = true;
            this.Icon = new Icon(
                typeof(System.Web.UI.Design.MobileControls.MobileControlDesigner),
                "General.ico"
            );

            this.Controls.AddRange(new Control[] 
                           {
                               appearanceGroup
                           });

            if (_isBaseControlList)
            {
                this.Controls.AddRange(new Control[] 
                           {
                               _itemsPerPageTextBox,
                               itemsPerPageLabel,
                               _itemCountTextBox,
                               itemCountLabel,
                               pagingGroup,
                               decorationLabel,
                               _decorationCombo
                           });
            }
            else
            {
                this.Controls.AddRange(new Control[] 
                           {
                               _rowsTextBox,
                               rowsLabel,
                               selectTypeLabel,
                               _selectTypeCombo
                           });
            }
        }

        protected override void LoadComponent() 
        {
            IListDesigner listDesigner = (IListDesigner)GetBaseDesigner();

            if (_isBaseControlList)
            {
                List list = (List)GetBaseControl();
                _itemCountTextBox.Text = list.ItemCount.ToString(CultureInfo.InvariantCulture);
                _itemsPerPageTextBox.Text = list.ItemsPerPage.ToString(CultureInfo.InvariantCulture);

                switch (list.Decoration) 
                {
                    case ListDecoration.None:
                        _decorationCombo.SelectedIndex = IDX_DECORATION_NONE;
                        break;
                    case ListDecoration.Bulleted:
                        _decorationCombo.SelectedIndex = IDX_DECORATION_BULLETED;
                        break;
                    case ListDecoration.Numbered:
                        _decorationCombo.SelectedIndex = IDX_DECORATION_NUMBERED;
                        break;
                }
            }
            else
            {
                SelectionList selectionList = (SelectionList)GetBaseControl();

                switch (selectionList.SelectType) 
                {
                    case ListSelectType.DropDown:
                        _selectTypeCombo.SelectedIndex = IDX_SELECTTYPE_DROPDOWN;
                        break;
                    case ListSelectType.ListBox:
                        _selectTypeCombo.SelectedIndex = IDX_SELECTTYPE_LISTBOX;
                        break;
                    case ListSelectType.Radio:
                        _selectTypeCombo.SelectedIndex = IDX_SELECTTYPE_RADIO;
                        break;
                    case ListSelectType.MultiSelectListBox:
                        _selectTypeCombo.SelectedIndex = IDX_SELECTTYPE_MULTISELECTLISTBOX;
                        break;
                    case ListSelectType.CheckBox:
                        _selectTypeCombo.SelectedIndex = IDX_SELECTTYPE_CHECKBOX;
                        break;
                }

                _rowsTextBox.Text = selectionList.Rows.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void OnSetPageDirty(Object source, EventArgs e) 
        {
            if (IsLoading())
            {
                return;
            }
            SetDirty();
        }

        private void OnKeyPressNumberTextBox(Object source, KeyPressEventArgs e)
        {
            if (!((e.KeyChar >='0' && e.KeyChar <= '9') ||
                  e.KeyChar == 8))
            {
                e.Handled = true;
                SafeNativeMethods.MessageBeep(unchecked((int)0xFFFFFFFF));
            }
        }

        /// <summary>
        ///   Saves the component loaded into the page.
        /// </summary>
        /// <seealso cref="System.Windows.Forms.Design.ComponentEditorPage"/>
        protected override void SaveComponent() 
        {
            IListDesigner listDesigner = (IListDesigner)GetBaseDesigner();

            if (_isBaseControlList)
            {
                List list = (List)GetBaseControl();

                switch (_decorationCombo.SelectedIndex) 
                {
                    case IDX_DECORATION_NONE:
                        list.Decoration = ListDecoration.None;
                        break;
                    case IDX_DECORATION_BULLETED:
                        list.Decoration = ListDecoration.Bulleted;
                        break;
                    case IDX_DECORATION_NUMBERED:
                        list.Decoration = ListDecoration.Numbered;
                        break;
                }

                try
                {
                    int itemCount = 0;

                    if (_itemCountTextBox.Text.Length != 0)
                    {
                        itemCount = Int32.Parse(_itemCountTextBox.Text, CultureInfo.InvariantCulture);
                    }
                    list.ItemCount = itemCount;
                }
                catch (Exception)
                {
                    _itemCountTextBox.Text = list.ItemCount.ToString(CultureInfo.InvariantCulture);
                }

                try
                {
                    int itemsPerPage = 0;

                    if (_itemsPerPageTextBox.Text.Length != 0)
                    {
                        itemsPerPage = Int32.Parse(_itemsPerPageTextBox.Text, CultureInfo.InvariantCulture);
                    }
                    list.ItemsPerPage = itemsPerPage;
                }
                catch (Exception)
                {
                    _itemsPerPageTextBox.Text = list.ItemsPerPage.ToString(CultureInfo.InvariantCulture);
                }

                TypeDescriptor.Refresh(list);
            }
            else
            {
                // 
                SelectionList selectionList = (SelectionList)GetBaseControl();

                switch (_selectTypeCombo.SelectedIndex) 
                {
                    case IDX_SELECTTYPE_DROPDOWN:
                        selectionList.SelectType = ListSelectType.DropDown;
                        break;
                    case IDX_SELECTTYPE_LISTBOX:
                        selectionList.SelectType = ListSelectType.ListBox;
                        break;
                    case IDX_SELECTTYPE_RADIO:
                        selectionList.SelectType = ListSelectType.Radio;
                        break;
                    case IDX_SELECTTYPE_MULTISELECTLISTBOX:
                        selectionList.SelectType = ListSelectType.MultiSelectListBox;
                        break;
                    case IDX_SELECTTYPE_CHECKBOX:
                        selectionList.SelectType = ListSelectType.CheckBox;
                        break;
                }

                try
                {
                    int rows = 4;

                    if (_rowsTextBox.Text.Length != 0)
                    {
                        rows = Int32.Parse(_rowsTextBox.Text, CultureInfo.InvariantCulture);
                    }
                    selectionList.Rows = rows;
                }
                catch (Exception)
                {
                    _rowsTextBox.Text = selectionList.Rows.ToString(CultureInfo.InvariantCulture);
                }

                TypeDescriptor.Refresh(selectionList);
            }
        }

        /// <summary>
        ///   Sets the component that is to be edited in the page.
        /// </summary>
        /// <seealso cref="System.Windows.Forms.Design.ComponentEditorPage"/>
        public override void SetComponent(IComponent component) 
        {
            base.SetComponent(component);
            InitForm();
        }
    }
}
