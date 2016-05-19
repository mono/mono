//------------------------------------------------------------------------------
// <copyright file="ObjectListFieldsPage.cs" company="Microsoft">
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
    using System.Web.UI.MobileControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Web.UI.Design.MobileControls.Util;

    using DesignTimeData = System.Web.UI.Design.DesignTimeData;
    using Button = System.Windows.Forms.Button;
    using Label = System.Windows.Forms.Label;
    using ComboBox = System.Windows.Forms.ComboBox;
    using TextBox = System.Windows.Forms.TextBox;

    /// <summary>
    ///   The Choices page for the StyleSheet control.
    /// </summary>
    /// <internalonly/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class ObjectListFieldsPage : ListComponentEditorPage
    {
        private CheckBox _ckbAutoGenerateFields;
        private CheckBox _ckbVisible;
        private TextBox _txtDataFormatString;
        private TextBox _txtTitle;
        private UnsettableComboBox _cmbDataField;
        private ObjectList _objectList;

        public ObjectListFieldsPage()
        {
            Y = 52;
            CaseSensitive = false;
            TreeViewTitle           = SR.GetString(SR.ObjectListFieldsPage_FieldNameCaption);
            AddButtonTitle          = SR.GetString(SR.ObjectListFieldsPage_NewFieldBtnCaption);
            DefaultName             = SR.GetString(SR.ObjectListFieldsPage_DefaultFieldName);
            MessageTitle            = SR.GetString(SR.ObjectListFieldsPage_ErrorMessageTitle);
            EmptyNameMessage        = SR.GetString(SR.ObjectListFieldsPage_EmptyNameError);
            // DuplicateNameMessage = SR.GetString(SR.ObjectListFieldsPage_DuplicateNameError);
        }

        protected override String HelpKeyword 
        {
            get 
            {
                return "net.Mobile.ObjectListProperties.Fields";
            }
        }

        protected override void InitForm()
        {
            base.InitForm();

            this._objectList = (ObjectList)Component;

            this.CommitOnDeactivate = true;
            this.Icon = new Icon(
                typeof(System.Web.UI.Design.MobileControls.MobileControlDesigner),
                "Fields.ico"
            );
            this.Size = new Size(402, 300);
            this.Text = SR.GetString(SR.ObjectListFieldsPage_Title);
            
            _ckbAutoGenerateFields = new CheckBox();
            _cmbDataField          = new UnsettableComboBox();
            _ckbVisible            = new CheckBox();
            _txtDataFormatString   = new TextBox();
            _txtTitle              = new TextBox();

            _ckbAutoGenerateFields.SetBounds(4, 4, 396, LabelHeight);
            _ckbAutoGenerateFields.Text = SR.GetString(SR.ObjectListFieldsPage_AutoGenerateFieldsCaption);
            _ckbAutoGenerateFields.FlatStyle = FlatStyle.System;
            _ckbAutoGenerateFields.CheckedChanged += new EventHandler(this.OnSetPageDirty);
            _ckbAutoGenerateFields.TabIndex = 0;

            GroupLabel grplblFieldList = new GroupLabel();
            grplblFieldList.SetBounds(4, 32, 392, LabelHeight);
            grplblFieldList.Text = SR.GetString(SR.ObjectListFieldsPage_FieldListGroupLabel);
            grplblFieldList.TabIndex = 1;
            grplblFieldList.TabStop = false;

            TreeList.TabIndex = 2;

            Label lblDataField = new Label();
            lblDataField.SetBounds(X, Y, ControlWidth, LabelHeight);
            lblDataField.Text = SR.GetString(SR.ObjectListFieldsPage_DataFieldCaption);
            lblDataField.TabStop = false;
            lblDataField.TabIndex = Index;

            Y += LabelHeight;
            _cmbDataField.SetBounds(X, Y, ControlWidth, CmbHeight);
            _cmbDataField.DropDownStyle = ComboBoxStyle.DropDown;
            _cmbDataField.Sorted        = true;
            _cmbDataField.NotSetText    = SR.GetString(SR.ObjectListFieldsPage_NoneComboEntry);
            _cmbDataField.TextChanged   += new EventHandler(this.OnPropertyChanged);
            _cmbDataField.SelectedIndexChanged += new EventHandler(this.OnPropertyChanged);
            _cmbDataField.TabIndex = Index + 1;

            Y += CellSpace;
            Label lblDataFormatString = new Label();
            lblDataFormatString.SetBounds(X, Y, ControlWidth, LabelHeight);
            lblDataFormatString.Text = SR.GetString(SR.ObjectListFieldsPage_DataFormatStringCaption);
            lblDataFormatString.TabStop = false;
            lblDataFormatString.TabIndex = Index + 2;

            Y += LabelHeight;
            _txtDataFormatString.SetBounds(X, Y, ControlWidth, CmbHeight);
            _txtDataFormatString.TextChanged += new EventHandler(this.OnPropertyChanged);
            _txtDataFormatString.TabIndex = Index + 3;

            Y += CellSpace;
            Label lblTitle = new Label();
            lblTitle.SetBounds(X, Y, ControlWidth, LabelHeight);
            lblTitle.Text = SR.GetString(SR.ObjectListFieldsPage_TitleCaption);
            lblTitle.TabStop = false;
            lblTitle.TabIndex = Index + 4;

            Y += LabelHeight;
            _txtTitle.SetBounds(X, Y, ControlWidth, CmbHeight);
            _txtTitle.TextChanged += new EventHandler(this.OnPropertyChanged);
            _txtTitle.TabIndex = Index + 5;

            Y += CellSpace;
            _ckbVisible.SetBounds(X, Y, ControlWidth, CmbHeight);
            _ckbVisible.FlatStyle = System.Windows.Forms.FlatStyle.System;
            _ckbVisible.Text = SR.GetString(SR.ObjectListFieldsPage_VisibleCaption); 
            _ckbVisible.CheckedChanged += new EventHandler(this.OnPropertyChanged);
            _ckbVisible.TabIndex = Index + 6;

            this.Controls.AddRange(new Control[] {
                                                     _ckbAutoGenerateFields,
                                                     grplblFieldList,
                                                     lblDataField,
                                                     _cmbDataField,
                                                     lblDataFormatString,
                                                     _txtDataFormatString,
                                                     lblTitle,
                                                     _txtTitle,
                                                     _ckbVisible
                                                 });
        }

        protected override void InitPage() 
        {
            base.InitPage();

            _cmbDataField.Items.Clear();
            _cmbDataField.SelectedIndex = -1;
            _cmbDataField.EnsureNotSetItem();
            _txtDataFormatString.Text = String.Empty;
            _txtTitle.Text = String.Empty;
            _ckbVisible.Checked = true;
            _ckbAutoGenerateFields.Checked = _objectList.AutoGenerateFields;

            LoadDataSourceFields();
        }

        private void LoadDataSourceFields() 
        {
            using (new LoadingModeResource(this))
            {
                PropertyDescriptorCollection props = null;
                ObjectListDesigner objectListDesigner = (ObjectListDesigner)GetBaseDesigner();

                IEnumerable dataSource = ((IDataSourceProvider)objectListDesigner).GetResolvedSelectedDataSource();
                if (dataSource != null)
                {
                    props = DesignTimeData.GetDataFields(dataSource);
                }

                if (props != null)
                {
                    foreach (PropertyDescriptor propDesc in props)
                    {
                        _cmbDataField.Items.Add(propDesc.Name);
                    }
                }
            }
        }

        protected override void LoadItems()
        {
            using (new LoadingModeResource(this))
            {
                foreach (ObjectListField field in _objectList.Fields)
                {
                    FieldTreeNode newNode = new FieldTreeNode(field.Name, field);
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
                    FieldTreeNode currentFieldNode = (FieldTreeNode)CurrentNode;

                    _cmbDataField.Text          = currentFieldNode.DataField;
                    _txtDataFormatString.Text   = currentFieldNode.DataFormatString;
                    _txtTitle.Text              = currentFieldNode.Title;
                    _ckbVisible.Checked         = currentFieldNode.Visible;
                }
                else
                {
                    _cmbDataField.Text          = String.Empty;
                    _txtDataFormatString.Text   = String.Empty;
                    _txtTitle.Text              = String.Empty;
                    _ckbVisible.Checked         = false;
                }
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

        protected override void OnClickAddButton(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            FieldTreeNode newNode = new FieldTreeNode(GetNewName());
            TreeList.TvList.Nodes.Add(newNode);

            TreeList.TvList.SelectedNode = newNode;
            CurrentNode = newNode;
            newNode.Dirty = true;
            newNode.BeginEdit();

            LoadItemProperties();

            SetDirty();
        }

        protected override void OnPropertyChanged(Object source, EventArgs e)
        {
            if (IsLoading())
            {
                return;
            }

            // This means there are no fields yet. Do nothing
            if (CurrentNode == null)
            {
                return;
            }

            FieldTreeNode currentFieldNode = (FieldTreeNode)CurrentNode;

            if (source == _cmbDataField)
            {
                currentFieldNode.DataField = _cmbDataField.Text;
            }
            else if (source == _txtDataFormatString)
            {
                currentFieldNode.DataFormatString = _txtDataFormatString.Text;
            }
            else if (source == _txtTitle)
            {
                currentFieldNode.Title = _txtTitle.Text;
            }
            else if (source == _ckbVisible)
            {
                currentFieldNode.Visible = _ckbVisible.Checked;
            }

            SetDirty();
            CurrentNode.Dirty = true;
        }

        protected override void SaveComponent()
        {
            // Delegate to base implementation first!
            // This will properly close ListTreeNode editing mode.
            base.SaveComponent();

            _objectList.Fields.Clear();

            foreach (FieldTreeNode fieldNode in TreeList.TvList.Nodes)
            {
                if (fieldNode.Dirty)
                {
                    fieldNode.RuntimeField.Name             = fieldNode.Name;
                    fieldNode.RuntimeField.DataField        = fieldNode.DataField;
                    fieldNode.RuntimeField.DataFormatString = fieldNode.DataFormatString;
                    fieldNode.RuntimeField.Title            = fieldNode.Title;
                    fieldNode.RuntimeField.Visible          = fieldNode.Visible;
                }

                Debug.Assert(fieldNode.RuntimeField != null);
                _objectList.Fields.AddAt(-1, fieldNode.RuntimeField);
            }

            _objectList.AutoGenerateFields = _ckbAutoGenerateFields.Checked;

            TypeDescriptor.Refresh(_objectList);
        }

        protected override void UpdateControlsEnabling()
        {
            TreeList.TvList.Enabled = 
            _cmbDataField.Enabled = 
            _txtDataFormatString.Enabled = 
            _txtTitle.Enabled = 
            _ckbVisible.Enabled = (TreeList.TvList.SelectedNode != null);
        }

        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class FieldTreeNode : ListTreeNode
        {
            private ObjectListField _runtimeField;

            private String _dataField;
            private String _dataFormatString;
            private String _title;
            private bool _visible;

            internal FieldTreeNode(String fieldID) : this(fieldID, new ObjectListField())
            {
            }

            /// <summary>
            /// </summary>
            internal FieldTreeNode(String fieldID, ObjectListField runtimeField) : base(fieldID)
            {
                Debug.Assert(fieldID != null, "invalid ID for ObjectListField");
                Debug.Assert(runtimeField != null, "null ObjectListField");

                this._runtimeField  = runtimeField;

                LoadAttributes();
            }

            private void LoadAttributes()
            {
                this.DataField          = RuntimeField.DataField;
                this.DataFormatString   = RuntimeField.DataFormatString;
                this.Title              = RuntimeField.Title;
                this.Visible            = RuntimeField.Visible;
            }

            internal ObjectListField RuntimeField 
            {
                get
                {
                    return _runtimeField;
                }
            }

            internal String DataField
            {
                get
                {
                    return _dataField;
                }

                set
                {
                    _dataField = value;
                }
            }

            internal String DataFormatString
            {
                get
                {
                    return _dataFormatString;
                }

                set
                {
                    _dataFormatString = value;
                }
            }

            internal String Title
            {
                get
                {
                    return _title;
                }

                set
                {
                    _title = value;
                }
            }

            internal bool Visible
            {
                get
                {
                    return _visible;
                }

                set
                {
                    _visible = value;
                }
            }
        }
    }
}
