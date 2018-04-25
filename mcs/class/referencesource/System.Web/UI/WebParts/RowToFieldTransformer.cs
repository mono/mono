//------------------------------------------------------------------------------
// <copyright file="RowToFieldTransformer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI.WebControls;

    [WebPartTransformer(typeof(IWebPartRow), typeof(IWebPartField))]
    public sealed class RowToFieldTransformer : WebPartTransformer, IWebPartField {

        private IWebPartRow _provider;
        private string _fieldName;

        // We know there is only 1 RowCallback, since transformers can only have 1 consumer
        private FieldCallback _callback;

        public override Control CreateConfigurationControl() {
            return new RowToFieldConfigurationWizard(this);
        }

        public string FieldName {
            get {
                return (_fieldName != null) ? _fieldName : String.Empty;
            }
            set {
                _fieldName = value;
            }
        }

        private PropertyDescriptorCollection ProviderSchema {
            get {
                return (_provider != null) ? _provider.Schema : null;
            }
        }

        private void GetRowData(object rowData) {
            Debug.Assert(_callback != null);

            object fieldValue = null;

            if (rowData != null) {
                PropertyDescriptor prop = ((IWebPartField)this).Schema;
                if (prop != null) {
                    fieldValue = prop.GetValue(rowData);
                }
            }

            _callback(fieldValue);
        }

        protected internal override void LoadConfigurationState(object savedState) {
            _fieldName = (string)savedState;
        }

        protected internal override object SaveConfigurationState() {
            return _fieldName;
        }

        public override object Transform(object providerData) {
            _provider = (IWebPartRow)providerData;
            return this;
        }

        #region Implementation of IWebPartField
        void IWebPartField.GetFieldValue(FieldCallback callback) {
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }

            if (_provider != null) {
                _callback = callback;
                _provider.GetRowData(new RowCallback(GetRowData));
            }
            else {
                callback(null);
            }
        }

        PropertyDescriptor IWebPartField.Schema {
            get {
                PropertyDescriptorCollection schema = ProviderSchema;
                return (schema != null) ? schema.Find(FieldName, /* ignoreCase */ true) : null;
            }
        }
        #endregion

        private sealed class RowToFieldConfigurationWizard : TransformerConfigurationWizardBase {
            private DropDownList _fieldName;
            private RowToFieldTransformer _owner;

            private const string fieldNameID = "FieldName";

            public RowToFieldConfigurationWizard(RowToFieldTransformer owner) {
                Debug.Assert(owner != null);
                _owner = owner;
            }

            // Dummy consumer schema
            protected override PropertyDescriptorCollection ConsumerSchema {
                get {
                    return null;
                }
            }

            protected override PropertyDescriptorCollection ProviderSchema {
                get {
                    return _owner.ProviderSchema;
                }
            }

            protected override void CreateWizardSteps() {
                // The WizardSteps should be empty when this is called
                Debug.Assert(WizardSteps.Count == 0);

                WizardStep s = new WizardStep();

                _fieldName = new DropDownList();
                _fieldName.ID = fieldNameID;
                if (OldProviderNames != null) {
                    for (int i=0; i < OldProviderNames.Length / 2; i++) {
                        ListItem item = new ListItem(OldProviderNames[2*i], OldProviderNames[2*i + 1]);
                        // Ignore case when setting selected value, since we ignore case when
                        // returing the connection data. (VSWhidbey 434566)
                        if (String.Equals(item.Value, _owner.FieldName, StringComparison.OrdinalIgnoreCase)) {
                            item.Selected = true;
                        }
                        _fieldName.Items.Add(item);
                    }
                }
                else {
                    _fieldName.Items.Add(new ListItem(SR.GetString(SR.RowToFieldTransformer_NoProviderSchema)));
                    _fieldName.Enabled = false;
                }

                Label fieldNameLabel = new Label();
                fieldNameLabel.Text = SR.GetString(SR.RowToFieldTransformer_FieldName);
                fieldNameLabel.AssociatedControlID = _fieldName.ID;

                s.Controls.Add(fieldNameLabel);
                s.Controls.Add(new LiteralControl(" "));
                s.Controls.Add(_fieldName);

                WizardSteps.Add(s);
            }

            protected override void OnFinishButtonClick(WizardNavigationEventArgs e) {
                Debug.Assert(_fieldName != null);

                string selectedFieldName = null;
                if (_fieldName.Enabled) {
                    selectedFieldName = _fieldName.SelectedValue;
                }
                _owner.FieldName = selectedFieldName;

                base.OnFinishButtonClick(e);
            }
        }
    }
}

