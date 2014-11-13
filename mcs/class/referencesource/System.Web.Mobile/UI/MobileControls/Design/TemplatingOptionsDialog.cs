//------------------------------------------------------------------------------
// <copyright file="TemplatingOptionsDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
//    using System.Web.UI.Design.Util;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.MobileControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    using Panel = System.Windows.Forms.Panel;
    using Button = System.Windows.Forms.Button;
    using Label = System.Windows.Forms.Label;
    using ComboBox = System.Windows.Forms.ComboBox;
    using Form = System.Windows.Forms.Form;
    using UnsettableComboBox = System.Web.UI.Design.MobileControls.Util.UnsettableComboBox;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class TemplatingOptionsDialog : DesignerForm, IRefreshableDeviceSpecificEditor, IDeviceSpecificDesigner
    {
        private System.Windows.Forms.Control _header;
        private MobileTemplatedControlDesigner _designer;
        private IDeviceSpecificDesigner _dsd;
        private DeviceSpecific _ds;
        private ISite _site;
        private ComboBox _cmbChoices;
        private UnsettableComboBox _cmbSchemas;
        private Button _btnEditChoices;
        private Button _btnClose;
        private int _mergingContext;
        private StringCollection _strCollSchemas;
        private Label _lblChoices = new Label();
        private Label _lblSchemas = new Label();
        private Panel _pnlMain = new Panel();
        private String[] _schemasFriendly;
        private String[] _schemasUrl;

        private const int _standardSchemaNumber = 2;

        internal TemplatingOptionsDialog(MobileTemplatedControlDesigner designer, 
                                       ISite site, 
                                       int mergingContext) : base(site)
        {
            _strCollSchemas = new StringCollection();
            _mergingContext = mergingContext;
            _designer = designer;
            _site = site;
            _dsd = (IDeviceSpecificDesigner) designer;
            _dsd.SetDeviceSpecificEditor(this);

            InitializeComponent();

            this.Text = SR.GetString(SR.TemplatingOptionsDialog_Title);
            _btnClose.Text = SR.GetString(SR.GenericDialog_CloseBtnCaption);
            _lblSchemas.Text = SR.GetString(SR.TemplatingOptionsDialog_SchemaCaption);
            _btnEditChoices.Text = SR.GetString(SR.TemplatingOptionsDialog_EditBtnCaption);
            _lblChoices.Text = SR.GetString(SR.TemplatingOptionsDialog_FilterCaption);
            _schemasFriendly = new String[] { SR.GetString(SR.TemplatingOptionsDialog_HTMLSchemaFriendly),
                                              SR.GetString(SR.TemplatingOptionsDialog_CHTMLSchemaFriendly) };
            _schemasUrl = new String[] { SR.GetString(SR.MarkupSchema_HTML32),
                                         SR.GetString(SR.MarkupSchema_cHTML10) };
            
            int tabOffset = GenericUI.InitDialog(
                this,
                _dsd,
                _mergingContext
            );

            SetTabIndexes(tabOffset);
            _dsd.RefreshHeader(_mergingContext);
            String currentDeviceSpecificID = _dsd.CurrentDeviceSpecificID;
            if (null != currentDeviceSpecificID && currentDeviceSpecificID.Length > 0)
            {
                DeviceSpecific ds;
                _dsd.GetDeviceSpecific(currentDeviceSpecificID, out ds);
                ((IRefreshableDeviceSpecificEditor) this).Refresh(currentDeviceSpecificID, ds);
            }
            UpdateControlEnabling();
        }

        protected override string HelpTopic {
            get {
                return "net.Mobile.TemplatingOptionsDialog";
            }
        }

        private void SetTabIndexes(int tabIndexOffset)
        {
            _pnlMain.TabIndex = ++tabIndexOffset;
            _lblChoices.TabIndex = ++tabIndexOffset;
            _cmbChoices.TabIndex = ++tabIndexOffset;
            _btnEditChoices.TabIndex = ++tabIndexOffset;
            _lblSchemas.TabIndex = ++tabIndexOffset;
            _cmbSchemas.TabIndex = ++tabIndexOffset;
            _btnClose.TabIndex = ++tabIndexOffset;
        }

        private void InitializeComponent()
        {
            _cmbChoices = new ComboBox();
            _cmbSchemas = new UnsettableComboBox();

            _btnEditChoices = new Button();
            _btnClose = new Button();
            
            _lblChoices.Location = new System.Drawing.Point(0, 0);
            _lblChoices.Size = new System.Drawing.Size(276, 16);
            _lblChoices.TabStop = false;

            _cmbChoices.Location = new System.Drawing.Point(0, 16);
            _cmbChoices.Size = new System.Drawing.Size(195, 21);
            _cmbChoices.TabStop = true;
            _cmbChoices.Enabled = false;
            _cmbChoices.Sorted = true;
            _cmbChoices.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbChoices.SelectedIndexChanged += new EventHandler(this.OnSelectedIndexChangedChoicesComboBox);

            _btnEditChoices.Location = new System.Drawing.Point(201, 15);
            _btnEditChoices.Size = new System.Drawing.Size(75, 23);
            _btnEditChoices.TabStop = true;
            _btnEditChoices.Click += new EventHandler(this.OnClickEditChoicesButton);

            _lblSchemas.Location = new System.Drawing.Point(0, 48);
            _lblSchemas.Size = new System.Drawing.Size(276, 16);
            _lblSchemas.TabStop = false;

            _cmbSchemas.Location = new System.Drawing.Point(0, 64);
            _cmbSchemas.Size = new System.Drawing.Size(276, 21);
            _cmbSchemas.TabStop = true;
            _cmbSchemas.Sorted = true;
            _cmbSchemas.DropDownStyle = ComboBoxStyle.DropDown;
            _cmbSchemas.LostFocus += new EventHandler(this.OnLostFocusSchemasComboBox);

            _btnClose.Location = new System.Drawing.Point(201, 104);
            _btnClose.Size = new System.Drawing.Size(75, 23);
            _btnClose.TabStop = true;
            _btnClose.Click += new EventHandler(this.OnClickCloseButton);

            this._pnlMain.Controls.AddRange(new System.Windows.Forms.Control[] {
                this._btnClose,
                this._cmbSchemas,
                this._lblSchemas,
                this._btnEditChoices,
                this._lblChoices,
                this._cmbChoices
            });
            this._pnlMain.Location = new System.Drawing.Point(6, 5);
            this._pnlMain.Size = new System.Drawing.Size(276, 128);
            this._pnlMain.TabIndex = 0;
            this._pnlMain.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);

            this.ClientSize = new Size(285, 139);
            this.AcceptButton = _btnClose;
            this.CancelButton = _btnClose;
            this.Controls.Add(_pnlMain);
        }

        private void FillChoicesComboBox()
        {
            Debug.Assert(_dsd != null);

            _cmbChoices.Items.Clear();

            if (null != _ds || null != _dsd.UnderlyingObject)
            {
                _cmbChoices.Items.Add(SR.GetString(SR.DeviceFilter_NoChoice));
            }

            if (null == _ds)
            {
                if (_cmbChoices.Items.Count > 0)
                {
                    _cmbChoices.SelectedIndex = 0;
                }
            }
            else
            {
                bool addedDefault = false;
                foreach (DeviceSpecificChoice choice in _ds.Choices)
                {
                    if (choice.Filter.Length == 0)
                    {
                        if (!addedDefault)
                        {
                            _cmbChoices.Items.Add(SR.GetString(SR.DeviceFilter_DefaultChoice));
                            addedDefault = true;
                        }
                    }
                    else
                    {
                        if (!choice.Filter.Equals(SR.GetString(SR.DeviceFilter_NoChoice)))
                        {
                            _cmbChoices.Items.Add(DesignerUtility.ChoiceToUniqueIdentifier(choice));
                        }
                    }
                }
                if (null != _designer.CurrentChoice && _designer.CurrentDeviceSpecific == _ds)
                {
                    if (_designer.CurrentChoice.Filter.Length == 0)
                    {
                        _cmbChoices.SelectedItem = SR.GetString(SR.DeviceFilter_DefaultChoice);
                    }
                    else
                    {
                        _cmbChoices.SelectedItem = DesignerUtility.ChoiceToUniqueIdentifier(_designer.CurrentChoice);
                    }
                }
                else 
                {
                    Debug.Assert(_cmbChoices.Items.Count > 0);
                    _cmbChoices.SelectedItem = SR.GetString(SR.DeviceFilter_NoChoice);
                }
            }
        }

        private void FillSchemasComboBox()
        {
            String friendlySchema;

            _cmbSchemas.Items.Clear();
            _cmbSchemas.Text = String.Empty;

            if (null != _ds)
            {
                // Add the standard HTML 3.2 and cHTML1.0 schemas
                for (int i = 0; i < _standardSchemaNumber; i++)
                {
                    _cmbSchemas.AddItem(_schemasFriendly[i]);
                }

                // Add the Xmlns entries existing in the applied device filters of the page
                IContainer container = _site.Container;
                Debug.Assert(null != container, "container is null");
                ComponentCollection allComponents = container.Components;
                _strCollSchemas.Clear();

                foreach (IComponent component in allComponents)
                {
                    ExtractDeviceFilterSchemas(component as System.Web.UI.Control);
                }

                foreach (String strSchema in _strCollSchemas)
                {
                    friendlySchema = UrlToFriendlySchema(strSchema);
                    if (!CaseSensitiveComboSearch(_cmbSchemas, friendlySchema))
                    {
                        _cmbSchemas.AddItem(friendlySchema);
                    }
                }

                // Add the Xmlns entries existing in the currently selected device filter
                foreach (DeviceSpecificChoice choice in _ds.Choices)
                {
                    friendlySchema = UrlToFriendlySchema(choice.Xmlns);
                    if (friendlySchema != null && friendlySchema.Length > 0 &&
                        !CaseSensitiveComboSearch(_cmbSchemas, friendlySchema))
                    {
                        _cmbSchemas.AddItem(friendlySchema);
                    }
                }
            }
        }

        private String FriendlyToUrlSchema(String friendlySchema)
        {
            for (int i = 0; i < _standardSchemaNumber; i++)
            {
                if (0 == String.Compare(_schemasFriendly[i], friendlySchema, StringComparison.OrdinalIgnoreCase))
                {
                    return _schemasUrl[i];
                }
            }
            return friendlySchema;
        }

        private String UrlToFriendlySchema(String urlSchema)
        {
            for (int i = 0; i < _standardSchemaNumber; i++)
            {
                if (0 == String.Compare(_schemasUrl[i], urlSchema, StringComparison.Ordinal))
                {
                    return _schemasFriendly[i];
                }
            }
            return urlSchema;
        }

        private void SetSchemaValue()
        {
            if (_ds != null &&
                _cmbChoices.SelectedIndex >= 0)
            {
                String currentChoiceIdentifier = _cmbChoices.SelectedItem as String;
                if (currentChoiceIdentifier != null && !currentChoiceIdentifier.Equals(SR.GetString(SR.DeviceFilter_NoChoice)))
                {
                    DeviceSpecificChoice dsc = GetChoiceFromIdentifier((String) currentChoiceIdentifier, _ds);
                    _cmbSchemas.Text = UrlToFriendlySchema(dsc.Xmlns);
                }
            }
        }

        private void ExtractDeviceFilterSchemas(System.Web.UI.Control control)
        {
            if (null == control)
            {
                return;
            }

            MobileControl mobileControl = control as MobileControl;
            if (null != mobileControl)
            {
                DeviceSpecific deviceSpecific;
                DeviceSpecificChoiceCollection choices;
                if (mobileControl is StyleSheet)
                {
                    StyleSheet styleSheet = (StyleSheet) mobileControl;
                    ICollection styleKeys = styleSheet.Styles;
                    foreach (String key in styleKeys)
                    {
                        Style style = styleSheet[key];
                        deviceSpecific = style.DeviceSpecific;
                        if (null != deviceSpecific && _ds != deviceSpecific)
                        {
                            choices = deviceSpecific.Choices;

                            foreach (DeviceSpecificChoice choice in choices)
                            {
                                if (choice.Xmlns != null && choice.Xmlns.Length > 0 &&
                                    !_strCollSchemas.Contains(choice.Xmlns))
                                {
                                    _strCollSchemas.Add(choice.Xmlns);
                                }
                            }
                        }
                    }
                }
                else
                {
                    deviceSpecific = mobileControl.DeviceSpecific;
                    if (null != deviceSpecific && _ds != deviceSpecific)
                    {
                        choices = deviceSpecific.Choices;

                        foreach (DeviceSpecificChoice choice in choices)
                        {
                            if (choice.Xmlns != null && choice.Xmlns.Length > 0 &&
                                !_strCollSchemas.Contains(choice.Xmlns))
                            {
                                _strCollSchemas.Add(choice.Xmlns);
                            }
                        }
                    }
                }
            }

            if (control.HasControls())
            {
                foreach (System.Web.UI.Control child in control.Controls)
                {
                    ExtractDeviceFilterSchemas(child);
                }
            }
        }

        private bool CaseSensitiveComboSearch(ComboBox cmb, String str)
        {
            foreach (Object obj in cmb.Items)
            {
                if (String.Compare(str, (String) obj, StringComparison.Ordinal) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateControlEnabling()
        {
            _btnEditChoices.Enabled = (_dsd.UnderlyingObject != null);
            _cmbChoices.Enabled = (_cmbChoices.Items.Count > 0);
            _cmbSchemas.Enabled = (_cmbChoices.Items.Count > 1) &&
                                  (!((String)_cmbChoices.SelectedItem).Equals(SR.GetString(SR.DeviceFilter_NoChoice)));
        }

        private DeviceSpecificChoice GetChoiceFromIdentifier(String choiceIdentifier, DeviceSpecific ds)
        {
            if (null == ds)
            {
                return null;
            }

            Debug.Assert(ds.Choices != null);

            foreach (DeviceSpecificChoice choice in ds.Choices)
            {
                if (DesignerUtility.ChoiceToUniqueIdentifier(choice).Equals(choiceIdentifier) ||
                    (choice.Filter.Length == 0 && 
                     choiceIdentifier.Equals(SR.GetString(SR.DeviceFilter_DefaultChoice))))
                {
                    return choice;
                }
            }

            return null;
        }

        bool IRefreshableDeviceSpecificEditor.RequestRefresh()
        {
            return true;
        }
        
        void IRefreshableDeviceSpecificEditor.Refresh(String deviceSpecificID, DeviceSpecific ds)
        {
            _ds = ds;
            FillChoicesComboBox();
            FillSchemasComboBox();
            SetSchemaValue();
            UpdateControlEnabling();
        }

        void IRefreshableDeviceSpecificEditor.UnderlyingObjectsChanged()
        {
        }

        void IRefreshableDeviceSpecificEditor.BeginExternalDeviceSpecificEdit() {}
        void IRefreshableDeviceSpecificEditor.EndExternalDeviceSpecificEdit(
            bool commitChanges) {}
        void IRefreshableDeviceSpecificEditor.DeviceSpecificRenamed(
            String oldDeviceSpecificID, String newDeviceSpecificID) {}
        void IRefreshableDeviceSpecificEditor.DeviceSpecificDeleted(
            String deviceSpecificID) {}
        
        private void OnClickCloseButton(Object sender, EventArgs e)
        {
            _dsd.UseCurrentDeviceSpecificID();

            if (0 <= _cmbChoices.SelectedIndex &&
                !_cmbChoices.Text.Equals(SR.GetString(SR.DeviceFilter_NoChoice)))
            {
                _designer.CurrentChoice = GetChoiceFromIdentifier((String) _cmbChoices.SelectedItem, _ds);
            }
            else
            {
                _designer.CurrentChoice = null;
            }

            Close();
            DialogResult = DialogResult.OK;
        }

        private void OnSelectedIndexChangedChoicesComboBox(Object sender, EventArgs e) 
        {
            if (_cmbChoices.Text.Equals(SR.GetString(SR.DeviceFilter_NoChoice)))
            {
                _cmbSchemas.Enabled = false;
                _cmbSchemas.Text = String.Empty;
            }
            else
            {
                _cmbSchemas.Enabled = true;
                SetSchemaValue();
            }

            _designer.SetTemplateVerbsDirty();
        }

        private void OnLostFocusSchemasComboBox(Object sender, EventArgs e) 
        {
            Debug.Assert(_ds != null);
            Debug.Assert(_cmbChoices.SelectedIndex >= 0);
            DeviceSpecificChoice choice = GetChoiceFromIdentifier((String) _cmbChoices.SelectedItem, _ds);
            String urlSchema = FriendlyToUrlSchema(_cmbSchemas.Text);
            if (0 != String.Compare(choice.Xmlns, urlSchema, StringComparison.Ordinal))
            {
                String previousUrlSchema = choice.Xmlns;
                if (!_strCollSchemas.Contains(previousUrlSchema))
                {
                    int previousSchemaOccurrences = 0;
                    foreach (DeviceSpecificChoice choiceTmp in _ds.Choices)
                    {
                        if (0 == String.Compare(choiceTmp.Xmlns, previousUrlSchema, StringComparison.Ordinal))
                        {
                            previousSchemaOccurrences++;
                        }
                    }
                    Debug.Assert(previousSchemaOccurrences > 0);
                    if (previousSchemaOccurrences == 1)
                    {
                        bool standardSchema = false;
                        for (int i = 0; i < _standardSchemaNumber; i++)
                        {
                            if (0 == String.Compare(_schemasUrl[i], previousUrlSchema, StringComparison.Ordinal))
                            {
                                standardSchema = true;
                                break;
                            }
                        }
                        if (!standardSchema)
                        {
                            _cmbSchemas.Items.Remove(UrlToFriendlySchema(previousUrlSchema));
                        }
                    }
                }
                choice.Xmlns = urlSchema;
                String friendlySchema = UrlToFriendlySchema(urlSchema);
                if (friendlySchema == null || friendlySchema.Length > 0 &&
                    !CaseSensitiveComboSearch(_cmbSchemas, friendlySchema))
                {
                    _cmbSchemas.AddItem(friendlySchema);
                }
            }
        }

        private void OnClickEditChoicesButton(Object source, EventArgs e)
        {
            AppliedDeviceFiltersDialog dialog = new AppliedDeviceFiltersDialog(this, _mergingContext);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _designer.UpdateRendering();
                FillChoicesComboBox();
                FillSchemasComboBox();
                SetSchemaValue();
                UpdateControlEnabling();
            }
        }

        ////////////////////////////////////////////////////////////////////////
        //  Begin IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////

        void IDeviceSpecificDesigner.SetDeviceSpecificEditor
            (IRefreshableDeviceSpecificEditor editor)
        {
        }

        String IDeviceSpecificDesigner.CurrentDeviceSpecificID
        {
            get
            {
                return _dsd.CurrentDeviceSpecificID;
            }
        }

        System.Windows.Forms.Control IDeviceSpecificDesigner.Header
        {
            get
            {
                return _header;
            }
        }

        System.Web.UI.Control IDeviceSpecificDesigner.UnderlyingControl
        {
            get
            {
                return _dsd.UnderlyingControl;
            }
        }

        Object IDeviceSpecificDesigner.UnderlyingObject
        {
            get
            {
                return _dsd.UnderlyingObject;
            }
        }

        bool IDeviceSpecificDesigner.GetDeviceSpecific(String deviceSpecificParentID, out DeviceSpecific ds)
        {
            return _dsd.GetDeviceSpecific(deviceSpecificParentID, out ds);
        }

        void IDeviceSpecificDesigner.SetDeviceSpecific(String deviceSpecificParentID, DeviceSpecific ds)
        {
            _ds = ds;
            _dsd.SetDeviceSpecific(deviceSpecificParentID, ds);
        }

        void IDeviceSpecificDesigner.InitHeader(int mergingContext)
        {
            HeaderPanel panel = new HeaderPanel();
            HeaderLabel lblDescription = new HeaderLabel();

            lblDescription.TabIndex = 0;
            lblDescription.Text = SR.GetString(SR.MobileControl_SettingGenericChoiceDescription);
            
            panel.Height = lblDescription.Height;
            panel.Width = lblDescription.Width;
            panel.Controls.Add(lblDescription);
            _header = panel;
        }

        void IDeviceSpecificDesigner.RefreshHeader(int mergingContext)
        {
        }

        void IDeviceSpecificDesigner.UseCurrentDeviceSpecificID()
        {
        }

        /////////////////////////////////////////////////////////////////////////
        //  End IDeviceSpecificDesigner Implementation
        /////////////////////////////////////////////////////////////////////////
    }
}
