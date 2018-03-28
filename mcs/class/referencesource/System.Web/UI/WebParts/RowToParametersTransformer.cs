//------------------------------------------------------------------------------
// <copyright file="RowToParametersTransformer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI.WebControls;

    [WebPartTransformer(typeof(IWebPartRow), typeof(IWebPartParameters))]
    public sealed class RowToParametersTransformer : WebPartTransformer, IWebPartParameters {

        private IWebPartRow _provider;
        private string[] _consumerFieldNames;
        private string[] _providerFieldNames;
        private PropertyDescriptorCollection _consumerSchema;
        private ParametersCallback _callback;

        public override Control CreateConfigurationControl() {
            return new RowToParametersConfigurationWizard(this);
        }

        [
        TypeConverterAttribute(typeof(StringArrayConverter)),
        ]
        public string[] ConsumerFieldNames {
            get {
                return (_consumerFieldNames != null) ? (string[])_consumerFieldNames.Clone() : new string[0];
            }
            set {
                _consumerFieldNames = (value != null) ? (string[])value.Clone() : null;
            }
        }

        private PropertyDescriptorCollection ConsumerSchema {
            get {
                return _consumerSchema;
            }
        }

        [
        TypeConverterAttribute(typeof(StringArrayConverter)),
        ]
        public string[] ProviderFieldNames {
            get {
                return (_providerFieldNames != null) ? (string[])_providerFieldNames.Clone() : new string[0];
            }
            set {
                _providerFieldNames = (value != null) ? (string[])value.Clone() : null;
            }
        }

        private PropertyDescriptorCollection ProviderSchema {
            get {
                return (_provider != null) ? _provider.Schema : null;
            }
        }

        // Schema containing property descriptors associated with each name in ProviderFieldNames
        private PropertyDescriptorCollection SelectedProviderSchema {
            get {
                PropertyDescriptorCollection props = new PropertyDescriptorCollection(null);

                PropertyDescriptorCollection providerSchema = ProviderSchema;
                if (providerSchema != null && _providerFieldNames != null && _providerFieldNames.Length > 0) {
                    foreach (string fieldName in _providerFieldNames) {
                        PropertyDescriptor prop = providerSchema.Find(fieldName, /* ignoreCase */ true);
                        if (prop == null) {
                            // If any provider field name is not in the schema, return an empty schema
                            return new PropertyDescriptorCollection(null);
                        }
                        else {
                            props.Add(prop);
                        }
                    }
                }

                return props;
            }
        }

        // Throws an exception if the ConsumerFieldNames and ProviderFieldNames have different length
        private void CheckFieldNamesLength() {
            int consumerFieldNamesLength = (_consumerFieldNames != null) ? _consumerFieldNames.Length : 0;
            int providerFieldNamesLength = (_providerFieldNames != null) ? _providerFieldNames.Length : 0;

            if (consumerFieldNamesLength != providerFieldNamesLength) {
                throw new InvalidOperationException(SR.GetString(SR.RowToParametersTransformer_DifferentFieldNamesLength));
            }
        }

        private void GetRowData(object rowData) {
            Debug.Assert(_callback != null);

            // Only return null if rowData is null.  Else, return an empty dictionary. (VSWhidbey 381264)
            IDictionary parametersData = null;

            // For perf, check in decreasing order of likeliness that an object is null
            if (rowData != null) {
                PropertyDescriptorCollection consumerSchema = ((IWebPartParameters)this).Schema;
                parametersData = new HybridDictionary(consumerSchema.Count);
                if (consumerSchema.Count > 0) {
                    PropertyDescriptorCollection providerSchema = SelectedProviderSchema;
                    if (providerSchema != null && providerSchema.Count > 0) {
                        if (providerSchema.Count == consumerSchema.Count) {
                            for (int i=0; i < providerSchema.Count; i++) {
                                PropertyDescriptor providerProp = providerSchema[i];
                                PropertyDescriptor consumerProp = consumerSchema[i];
                                parametersData[consumerProp.Name] = providerProp.GetValue(rowData);
                            }
                        }
                    }
                }
            }

            _callback(parametersData);
        }

        protected internal override void LoadConfigurationState(object savedState) {
            if (savedState != null) {
                string[] fieldNames = (string[])savedState;
                int fieldNamesLength = fieldNames.Length;

                if (fieldNamesLength % 2 != 0) {
                    throw new InvalidOperationException(SR.GetString(SR.RowToParametersTransformer_DifferentFieldNamesLength));
                }

                int length = fieldNamesLength / 2;
                _consumerFieldNames = new string[length];
                _providerFieldNames = new string[length];
                for (int i=0; i < length; i++) {
                    _consumerFieldNames[i] = fieldNames[2*i];
                    _providerFieldNames[i] = fieldNames[2*i + 1];
                }
            }
        }

        protected internal override object SaveConfigurationState() {
            CheckFieldNamesLength();

            int consumerFieldNamesLength = (_consumerFieldNames != null) ? _consumerFieldNames.Length : 0;
            if (consumerFieldNamesLength > 0) {
                string[] fieldNames = new string[consumerFieldNamesLength * 2];
                for (int i=0; i < consumerFieldNamesLength; i++) {
                    fieldNames[2*i] = _consumerFieldNames[i];
                    fieldNames[2*i + 1] = _providerFieldNames[i];
                }
                return fieldNames;
            }

            return null;
        }

        public override object Transform(object providerData) {
            _provider = (IWebPartRow)providerData;
            return this;
        }

        #region Implementation of IWebPartParameters
        void IWebPartParameters.GetParametersData(ParametersCallback callback) {
            if (callback == null) {
                throw new ArgumentNullException("callback");
            }

            CheckFieldNamesLength();

            if (_provider != null) {
                _callback = callback;
                _provider.GetRowData(new RowCallback(GetRowData));
            }
            else {
                callback(null);
            }
        }

        PropertyDescriptorCollection IWebPartParameters.Schema {
            get {
                CheckFieldNamesLength();

                PropertyDescriptorCollection props = new PropertyDescriptorCollection(null);

                if (_consumerSchema != null && _consumerFieldNames != null && _consumerFieldNames.Length > 0) {
                    foreach (string fieldName in _consumerFieldNames) {
                        PropertyDescriptor prop = _consumerSchema.Find(fieldName, true);
                        if (prop == null) {
                            // If any consumer field name is not in the schema, return an empty schema
                            return new PropertyDescriptorCollection(null);
                        }
                        else {
                            props.Add(prop);
                        }
                    }
                }

                return props;
            }
        }

        void IWebPartParameters.SetConsumerSchema(PropertyDescriptorCollection schema) {
            _consumerSchema = schema;
        }
        #endregion

        private sealed class RowToParametersConfigurationWizard : TransformerConfigurationWizardBase {
            private DropDownList[] _consumerFieldNames;
            private RowToParametersTransformer _owner;

            private const string consumerFieldNameID = "ConsumerFieldName";

            public RowToParametersConfigurationWizard(RowToParametersTransformer owner) {
                Debug.Assert(owner != null);
                _owner = owner;
            }

            protected override PropertyDescriptorCollection ConsumerSchema {
                get {
                    return _owner.ConsumerSchema;
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

                int oldProviderNamesLength = (OldProviderNames != null) ? OldProviderNames.Length : 0;
                if (oldProviderNamesLength > 0) {
                    _consumerFieldNames = new DropDownList[oldProviderNamesLength / 2];

                    ListItem[] consumerItems = null;
                    int oldConsumerNamesLength = (OldConsumerNames != null) ? OldConsumerNames.Length : 0;
                    if (oldConsumerNamesLength > 0) {
                        consumerItems = new ListItem[oldConsumerNamesLength / 2];
                        for (int i=0; i < oldConsumerNamesLength / 2; i++) {
                            consumerItems[i] = new ListItem(OldConsumerNames[2*i], OldConsumerNames[2*i + 1]);
                        }
                    }

                    for (int i=0; i < oldProviderNamesLength / 2; i++) {
                        WizardStep s = new WizardStep();

                        s.Controls.Add(new LiteralControl(
                            SR.GetString(SR.RowToParametersTransformer_ProviderFieldName) + " "));
                        Label label = new Label();

                        // HtmlEncode the string, since it comes from the provider schema and it may contain
                        // unsafe characters.
                        label.Text = HttpUtility.HtmlEncode(OldProviderNames[2*i]);

                        label.Font.Bold = true;
                        s.Controls.Add(label);

                        s.Controls.Add(new LiteralControl("<br />"));

                        DropDownList consumerFieldName = new DropDownList();
                        consumerFieldName.ID = consumerFieldNameID + i;
                        if (consumerItems != null) {
                            consumerFieldName.Items.Add(new ListItem());

                            // Calculate consumerFieldValue based on the current providerFieldValue,
                            // and the ProviderFieldNames and ConsumerFieldNames on the Transformer
                            string[] providerFieldNames = _owner._providerFieldNames;
                            string[] consumerFieldNames = _owner._consumerFieldNames;
                            string providerFieldValue = OldProviderNames[2*i + 1];
                            string consumerFieldValue = null;
                            if (providerFieldNames != null) {
                                for (int j=0; j < providerFieldNames.Length; j++) {
                                    // Ignore case when getting the value, since we ignore case when
                                    // returing the connection data. (VSWhidbey 434566)
                                    if (String.Equals(providerFieldNames[j], providerFieldValue,
                                                      StringComparison.OrdinalIgnoreCase) &&
                                        consumerFieldNames != null && consumerFieldNames.Length > j) {
                                        consumerFieldValue = consumerFieldNames[j];
                                        break;
                                    }
                                }
                            }

                            foreach (ListItem consumerItem in consumerItems) {
                                ListItem item = new ListItem(consumerItem.Text, consumerItem.Value);
                                // Ignore case when setting selected value, since we ignore case when
                                // returing the connection data. (VSWhidbey 434566)
                                if (String.Equals(item.Value, consumerFieldValue, StringComparison.OrdinalIgnoreCase)) {
                                    item.Selected = true;
                                }
                                consumerFieldName.Items.Add(item);
                            }
                        }
                        else {
                            consumerFieldName.Items.Add(new ListItem(
                                SR.GetString(SR.RowToParametersTransformer_NoConsumerSchema)));
                            consumerFieldName.Enabled = false;
                        }
                        _consumerFieldNames[i] = consumerFieldName;

                        Label consumerFieldNameLabel = new Label();
                        consumerFieldNameLabel.Text = SR.GetString(SR.RowToParametersTransformer_ConsumerFieldName);
                        consumerFieldNameLabel.AssociatedControlID = consumerFieldName.ID;

                        s.Controls.Add(consumerFieldNameLabel);
                        s.Controls.Add(new LiteralControl(" "));
                        s.Controls.Add(consumerFieldName);

                        WizardSteps.Add(s);
                    }
                }
                else {
                    WizardStep s = new WizardStep();
                    s.Controls.Add(new LiteralControl(SR.GetString(SR.RowToParametersTransformer_NoProviderSchema)));
                    WizardSteps.Add(s);
                }

                // We should always have at least 1 WizardStep when we return
                Debug.Assert(WizardSteps.Count > 0);
            }

            protected override void OnFinishButtonClick(WizardNavigationEventArgs e) {
                ArrayList providerFieldNames = new ArrayList();
                ArrayList consumerFieldNames = new ArrayList();

                int oldProviderNamesLength = (OldProviderNames != null) ? OldProviderNames.Length : 0;
                if (oldProviderNamesLength > 0) {
                    Debug.Assert(_consumerFieldNames != null);
                    Debug.Assert(oldProviderNamesLength == 2 * _consumerFieldNames.Length);
                    for (int i=0; i < _consumerFieldNames.Length; i++) {
                        DropDownList consumerFieldName = _consumerFieldNames[i];
                        if (consumerFieldName.Enabled) {
                            string selectedConsumerFieldName = consumerFieldName.SelectedValue;
                            if (!String.IsNullOrEmpty(selectedConsumerFieldName)) {
                                providerFieldNames.Add(OldProviderNames[2*i + 1]);
                                consumerFieldNames.Add(selectedConsumerFieldName);
                            }
                        }
                    }
                }

                _owner.ConsumerFieldNames = (string[])consumerFieldNames.ToArray(typeof(string));
                _owner.ProviderFieldNames = (string[])providerFieldNames.ToArray(typeof(string));

                base.OnFinishButtonClick(e);
            }
        }
    }
}

