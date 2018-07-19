//------------------------------------------------------------------------------
// <copyright file="PropertyGridEditorPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    using Debug = System.Diagnostics.Debug;
    using AttributeCollection = System.ComponentModel.AttributeCollection;

    public sealed class PropertyGridEditorPart : EditorPart {

        // Controls that accept user input to set property values (textboxes, dropdownlists, etc.)
        // Should use this in addition to Controls collection, since Controls collection
        // can be modified by user code.
        private ArrayList _editorControls;

        // Array of error messages associated with each editor control
        private string[] _errorMessages;

        private static readonly Attribute[] FilterAttributes =
            new Attribute[] { WebBrowsableAttribute.Yes };

        private static readonly WebPart designModeWebPart = new DesignModeWebPart();

        private static readonly UrlPropertyAttribute urlPropertyAttribute = new UrlPropertyAttribute();

        private const int TextBoxColumns = 30;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string DefaultButton {
            get { return base.DefaultButton; }
            set { base.DefaultButton = value; }
        }

        public override bool Display {
            get {
                if (base.Display == false) {
                    return false;
                }

                object editableObject = GetEditableObject();
                if (editableObject != null) {
                    if (GetEditableProperties(editableObject, false).Count > 0) {
                        return true;
                    }
                }

                return false;
            }
        }

        private ArrayList EditorControls {
            get {
                if (_editorControls == null) {
                    _editorControls = new ArrayList();
                }
                return _editorControls;
            }
        }

        private bool HasError {
            get {
                foreach (string errorMessage in _errorMessages) {
                    if (errorMessage != null) {
                        return true;
                    }
                }
                return false;
            }
        }

        [
        WebSysDefaultValue(SR.PropertyGridEditorPart_PartTitle),
        ]
        public override string Title {
            get {
                string s = (string)ViewState["Title"];
                return (s != null) ? s : SR.GetString(SR.PropertyGridEditorPart_PartTitle);
            }
            set {
                ViewState["Title"] = value;
            }
        }

        public override bool ApplyChanges() {
            object editableObject = GetEditableObject();

            Debug.Assert(editableObject != null);
            if (editableObject == null) {
                return true;
            }

            EnsureChildControls();

            int count = Controls.Count;
            Debug.Assert(count > 0);
            if (count == 0) {
                return true;
            }

            PropertyDescriptorCollection properties = GetEditableProperties(editableObject, true);
            for (int i=0; i < properties.Count; i++) {
                PropertyDescriptor pd = properties[i];
                Control editorControl = (Control)EditorControls[i];
                try {
                    object value = GetEditorControlValue(editorControl, pd);
                    // If the property is a url, validate protocol (VSWhidbey 290418)
                    if (pd.Attributes.Matches(urlPropertyAttribute) &&
                        CrossSiteScriptingValidation.IsDangerousUrl(value.ToString())) {

                        _errorMessages[i] = SR.GetString(SR.EditorPart_ErrorBadUrl);
                    }
                    else {
                        try {
                            pd.SetValue(editableObject, value);
                        }
                        catch (Exception e) {
                            _errorMessages[i] = CreateErrorMessage(e.Message);
                        }
                    }
                }
                catch {
                    // If custom errors are enabled, we do not want to render the property type to the browser.
                    // (VSWhidbey 381646)
                    if (Context != null && Context.IsCustomErrorEnabled) {
                        _errorMessages[i] = SR.GetString(SR.EditorPart_ErrorConvertingProperty);
                    }
                    else {
                        _errorMessages[i] = SR.GetString(SR.EditorPart_ErrorConvertingPropertyWithType, pd.PropertyType.FullName);
                    }
                }
            }

            return !HasError;
        }

        // In the future, we may want to add Color, Date, etc.
        private bool CanEditProperty(PropertyDescriptor property) {
            // Don't show readonly properties
            if (property.IsReadOnly) {
                return false;
            }

            // Don't show Shared personalizable properties in User mode
            if (WebPartManager != null &&
                WebPartManager.Personalization != null &&
                WebPartManager.Personalization.Scope == PersonalizationScope.User) {

                AttributeCollection attributes = property.Attributes;
                if (attributes.Contains(PersonalizableAttribute.SharedPersonalizable)) {
                    return false;
                }
            }

            // Only show properties that can be converted to/from string
            return Util.CanConvertToFrom(property.Converter, typeof(string));
        }

        protected internal override void CreateChildControls() {
            ControlCollection controls = Controls;
            controls.Clear();
            EditorControls.Clear();

            object editableObject = GetEditableObject();
            if (editableObject != null) {
                foreach (PropertyDescriptor pd in GetEditableProperties(editableObject, true)) {
                    Control editorControl = CreateEditorControl(pd);
                    EditorControls.Add(editorControl);
                    Controls.Add(editorControl);
                }
                _errorMessages = new string[EditorControls.Count];
            }

            // We don't need viewstate enabled on our child controls.  Disable for perf.
            foreach (Control c in controls) {
                c.EnableViewState = false;
            }
        }

        private Control CreateEditorControl(PropertyDescriptor pd) {
            Type propertyType = pd.PropertyType;
            if (propertyType == typeof(bool)) {
                return new CheckBox();
            }
            else if (typeof(Enum).IsAssignableFrom(propertyType)) {
                DropDownList dropDownList = new DropDownList();
                ICollection standardValues = pd.Converter.GetStandardValues();
                foreach (object o in standardValues) {
                    string text = pd.Converter.ConvertToString(o);
                    dropDownList.Items.Add(new ListItem(text));
                }
                return dropDownList;
            }
            else {
                TextBox textBox = new TextBox();
                textBox.Columns = TextBoxColumns;
                return textBox;
            }
        }

        private string GetDescription(PropertyDescriptor pd) {
            WebDescriptionAttribute attribute = (WebDescriptionAttribute)pd.Attributes[typeof(WebDescriptionAttribute)];
            if (attribute != null) {
                return attribute.Description;
            }
            else {
                return null;
            }
        }

        private string GetDisplayName(PropertyDescriptor pd) {
            WebDisplayNameAttribute attribute = (WebDisplayNameAttribute)pd.Attributes[typeof(WebDisplayNameAttribute)];
            if (attribute != null && !String.IsNullOrEmpty(attribute.DisplayName)) {
                return attribute.DisplayName;
            }
            else {
                return pd.Name;
            }
        }

        private object GetEditableObject() {
            if (DesignMode) {
                return designModeWebPart;
            }

            WebPart webPartToEdit = WebPartToEdit;
            IWebEditable editable = webPartToEdit as IWebEditable;

            if (editable != null) {
                return editable.WebBrowsableObject;
            }
            return webPartToEdit;
        }

        private PropertyDescriptorCollection GetEditableProperties(object editableObject, bool sort) {
            Debug.Assert(editableObject != null);

            PropertyDescriptorCollection propDescs = TypeDescriptor.GetProperties(editableObject, FilterAttributes);
            if (sort) {
                propDescs = propDescs.Sort();
            }

            PropertyDescriptorCollection filteredPropDescs = new PropertyDescriptorCollection(null);
            foreach (PropertyDescriptor pd in propDescs) {
                if (CanEditProperty(pd)) {
                    filteredPropDescs.Add(pd);
                }
            }

            return filteredPropDescs;
        }

        private object GetEditorControlValue(Control editorControl, PropertyDescriptor pd) {
            CheckBox checkBox = editorControl as CheckBox;
            if (checkBox != null) {
                return checkBox.Checked;
            }

            DropDownList dropDownList = editorControl as DropDownList;
            if (dropDownList != null) {
                string value = dropDownList.SelectedValue;
                return pd.Converter.ConvertFromString(value);
            }

            TextBox textBox = (TextBox)editorControl;
            return pd.Converter.ConvertFromString(textBox.Text);
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // We want to synchronize the EditorPart to the state of the WebPart on every page load,
            // so we stay current if the WebPart changes in the background.
            if (Display && Visible && !HasError) {
                SyncChanges();
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // HACK: Need this for child controls to be created at design-time when control is inside template
            EnsureChildControls();

            string[] propertyDisplayNames = null;
            string[] propertyDescriptions = null;
            object editableObject = GetEditableObject();
            if (editableObject != null) {
                PropertyDescriptorCollection propDescs = GetEditableProperties(editableObject, true);
                propertyDisplayNames = new string[propDescs.Count];
                propertyDescriptions = new string[propDescs.Count];
                for (int i=0; i < propDescs.Count; i++) {
                    propertyDisplayNames[i] = GetDisplayName(propDescs[i]);
                    propertyDescriptions[i] = GetDescription(propDescs[i]);
                }
            }

            if (propertyDisplayNames != null) {
                WebControl[] editorControls = (WebControl[])EditorControls.ToArray(typeof(WebControl));
                Debug.Assert(propertyDisplayNames.Length == editorControls.Length && propertyDisplayNames.Length == _errorMessages.Length);
                RenderPropertyEditors(writer, propertyDisplayNames, propertyDescriptions, editorControls, _errorMessages);
            }
        }

        public override void SyncChanges() {
            object editableObject = GetEditableObject();

            Debug.Assert(editableObject != null);
            if (editableObject != null) {
                EnsureChildControls();
                int count = 0;
                foreach (PropertyDescriptor pd in GetEditableProperties(editableObject, true)) {
                    if (CanEditProperty(pd)) {
                        Control editorControl = (Control)EditorControls[count];
                        SyncChanges(editorControl, pd, editableObject);
                        count++;
                    }
                }
            }
        }

        private void SyncChanges(Control control, PropertyDescriptor pd, object instance) {
            Type propertyType = pd.PropertyType;
            if (propertyType == typeof(bool)) {
                CheckBox checkBox = (CheckBox)control;
                checkBox.Checked = (bool)pd.GetValue(instance);
            }
            else if (typeof(Enum).IsAssignableFrom(propertyType)) {
                DropDownList dropDownList = (DropDownList)control;
                dropDownList.SelectedValue = pd.Converter.ConvertToString(pd.GetValue(instance));
            }
            else {
                TextBox textBox = (TextBox)control;
                textBox.Text = pd.Converter.ConvertToString(pd.GetValue(instance));
            }
        }

        private sealed class DesignModeWebPart : WebPart {
            [
            WebBrowsable(),
            WebSysWebDisplayName(SR.PropertyGridEditorPart_DesignModeWebPart_BoolProperty)
            ]
            public bool BoolProperty {
                get {
                    return false;
                }
                set {
                }
            }

            [
            WebBrowsable(),
            WebSysWebDisplayName(SR.PropertyGridEditorPart_DesignModeWebPart_EnumProperty)
            ]
            public SampleEnum EnumProperty {
                get {
                    return SampleEnum.EnumValue;
                }
                set {
                }
            }

            [
            WebBrowsable(),
            WebSysWebDisplayName(SR.PropertyGridEditorPart_DesignModeWebPart_StringProperty)
            ]
            public string StringProperty {
                get {
                    return String.Empty;
                }
                set {
                }
            }

            public enum SampleEnum {
                EnumValue
            }

            /// <devdoc>
            ///     WebDisplayNameAttribute marks a property, event, or extender with a
            ///     DisplayName for the PropertyGridEditorPart. 
            /// </devdoc>
            private sealed class WebSysWebDisplayNameAttribute : WebDisplayNameAttribute {

                private bool replaced;


                /// <devdoc>
                ///    <para>Constructs a new sys DisplayName.</para>
                /// </devdoc>
                internal WebSysWebDisplayNameAttribute(string DisplayName) : base(DisplayName) {
                }


                /// <devdoc>
                ///    <para>Retrieves the DisplayName text.</para>
                /// </devdoc>
                public override string DisplayName {
                    get {
                        if (!replaced) {
                            replaced = true;
                            DisplayNameValue = SR.GetString(base.DisplayName);                
                        }
                        return base.DisplayName;
                    }
                }

                public override object TypeId {
                    get {
                        return typeof(WebDisplayNameAttribute);
                    }
                }
            }
        }
    }
}
