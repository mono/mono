//------------------------------------------------------------------------------
// <copyright file="ImageField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Web.Util;

    /// <devdoc>
    /// <para>Creates a field bounded to a data field in a <see cref='System.Web.UI.WebControls.DataBoundControl'/>.</para>
    /// </devdoc>
    public class ImageField : DataControlField {

        /// <devdoc>
        ///    <para>Specifies a string that represents "this". This field is read-only. </para>
        /// </devdoc>
        public static readonly string ThisExpression = "!";

        private PropertyDescriptor _imageFieldDesc;
        private PropertyDescriptor _altTextFieldDesc;
        string _dataField;

        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.WebControls.ImageField'/> class.</para>
        /// </devdoc>
        public ImageField() {
        }

        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.ImageField_AlternateText)
        ]
        public virtual string AlternateText {
            get {
                object o = ViewState["AlternateText"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["AlternateText"])) {
                    ViewState["AlternateText"] = value;
                    OnFieldChanged();
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the property that determines whether the field treats empty string as
        ///    null when the field values are extracted.</para>
        /// </devdoc>
        [
            WebCategory("Behavior"),
            DefaultValue(true),
            WebSysDescription(SR.ImageField_ConvertEmptyStringToNull)
        ]
        public virtual bool ConvertEmptyStringToNull {
            get {
                object o = ViewState["ConvertEmptyStringToNull"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                ViewState["ConvertEmptyStringToNull"] = value;
            }
        }
        
        [
        WebCategory("Data"),
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebSysDescription(SR.ImageField_DataAlternateTextField)
        ]
        public virtual string DataAlternateTextField {
            get {
                object o = ViewState["DataAlternateTextField"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["DataAlternateTextField"])) {
                    ViewState["DataAlternateTextField"] = value;
                    OnFieldChanged();
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the display format of alternate text in this
        ///       field.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescription(SR.ImageField_DataAlternateTextFormatString)
        ]
        public virtual string DataAlternateTextFormatString {
            get {
                object o = ViewState["DataAlternateTextFormatString"];
                if (o != null)
                    return (string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["DataAlternateTextFormatString"])) {
                    ViewState["DataAlternateTextFormatString"] = value;
                    OnFieldChanged();
                }
            }
        }
        
        /// <devdoc>
        ///    <para> Gets or sets the field name from the data model bound to this field.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebSysDescription(SR.ImageField_ImageUrlField)
        ]
        public virtual string DataImageUrlField {
            get {
                if (_dataField == null) {
                    object o = ViewState["DataImageUrlField"];
                    if (o != null)
                        _dataField = (string)o;
                    else
                        _dataField = String.Empty;
                    }
                return _dataField;
            }
            set {
                if (!String.Equals(value, ViewState["DataImageUrlField"])) {
                    ViewState["DataImageUrlField"] = value;
                    _dataField = value;
                    OnFieldChanged();
                }
            }
        }

        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescription(SR.ImageField_ImageUrlFormatString)
        ]
        public virtual string DataImageUrlFormatString {
            get {
                object o = ViewState["DataImageUrlFormatString"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["DataImageUrlFormatString"])) {
                    ViewState["DataImageUrlFormatString"] = value;
                    OnFieldChanged();
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the property that determines what text is displayed if the value
        ///    of the field is null.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.BoundField_NullDisplayText)
        ]
        public virtual string NullDisplayText {
            get {
                object o = ViewState["NullDisplayText"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["NullDisplayText"])) {
                    ViewState["NullDisplayText"] = value;
                    OnFieldChanged();
                }
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.ImageField_NullImageUrl)
        ]
        public virtual string NullImageUrl {
            get {
                object o = ViewState["NullImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["NullImageUrl"])) {
                    ViewState["NullImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the property that prevents modification to data
        ///       in this field.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.ImageField_ReadOnly)
        ]
        public virtual bool ReadOnly {
            get {
                object o = ViewState["ReadOnly"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                object oldValue = ViewState["ReadOnly"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["ReadOnly"] = value;
                    OnFieldChanged();
                }
            }
        }

        protected override void CopyProperties(DataControlField newField) {
            ((ImageField)newField).AlternateText = AlternateText;
            ((ImageField)newField).ConvertEmptyStringToNull = ConvertEmptyStringToNull;
            ((ImageField)newField).DataAlternateTextField = DataAlternateTextField;
            ((ImageField)newField).DataAlternateTextFormatString = DataAlternateTextFormatString;
            ((ImageField)newField).DataImageUrlField = DataImageUrlField;
            ((ImageField)newField).DataImageUrlFormatString = DataImageUrlFormatString;
            ((ImageField)newField).NullDisplayText = NullDisplayText;
            ((ImageField)newField).NullImageUrl = NullImageUrl;
            ((ImageField)newField).ReadOnly = ReadOnly;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField() {
            return new ImageField();
        }
        
        /// <devdoc>
        /// Extracts the value(s) from the given cell and puts the value(s) into a dictionary.  Indicate includeReadOnly
        /// to have readonly fields' values inserted into the dictionary.
        /// </devdoc>
        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly) {
            Control childControl = null;
            string dataField = DataImageUrlField;
            object value = null;
            bool includeNullValue = false;

            if (((rowState & DataControlRowState.Insert) != 0) && !InsertVisible) {
                return;
            }

            if (cell.Controls.Count == 0) { // this should happen only in design mode
                Debug.Assert(DesignMode, "Unless you're in designmode, there should be a control in the cell.");
                return;
            }

            childControl = cell.Controls[0];
            
            Image image = childControl as Image;
            if (image != null) {
                if (includeReadOnly) {
                    includeNullValue = true;
                    if (image.Visible) {
                        value = image.ImageUrl;
                    }
                }
            }
            else {
                TextBox editBox = childControl as TextBox;
                if (editBox != null) {
                    value = editBox.Text;
                    includeNullValue = true;    // just in case someone wrote a derived textbox that returns null for Text.
                }
            }

            if (value != null || includeNullValue) {
                if (ConvertEmptyStringToNull && value is string && ((string)value).Length == 0) {
                    value = null;
                }

                if (dictionary.Contains(dataField)) {
                    dictionary[dataField] = value;
                }
                else {
                    dictionary.Add(dataField, value);
                }
            }
        }

        /// <devdoc>
        /// Returns the formatted value of the image url
        /// </devdoc>
        protected virtual string FormatImageUrlValue(object dataValue) {
            string formattedValue = String.Empty;
            string formatting = DataImageUrlFormatString;

            if (!DataBinder.IsNull(dataValue)) {
                string dataValueString = dataValue.ToString();

                if (dataValueString.Length > 0) {
                    if (formatting.Length == 0) {
                        formattedValue = dataValueString;
                    }
                    else {
                        formattedValue = String.Format(CultureInfo.CurrentCulture, formatting, dataValue);
                    }
                }
            }
            else {
                return null;
            }
            return formattedValue;
        }

        /// <devdoc>
        /// Returns the alternate text to be used for accessibility
        /// </devdoc>
        protected virtual string GetFormattedAlternateText(Control controlContainer) {
            string altTextField = DataAlternateTextField;
            string altTextFormatString = DataAlternateTextFormatString;
            string formattedAlternateText;


            if (altTextField.Length > 0) {
                object value = GetValue(controlContainer, altTextField, ref _altTextFieldDesc);
                string strValue = String.Empty;
                if (!DataBinder.IsNull(value)) {
                    strValue = value.ToString();
                }

                if (altTextFormatString.Length > 0) {
                    formattedAlternateText = String.Format(CultureInfo.CurrentCulture, altTextFormatString, value);
                }
                else {
                    formattedAlternateText = strValue;
                }
            }
            else {
                formattedAlternateText = AlternateText;
            }
            return formattedAlternateText;
        }

        /// <devdoc>
        /// Returns a value to be used for design-time rendering
        /// </devdoc>
        protected virtual string GetDesignTimeValue() {
            return SR.GetString(SR.Sample_Databound_Text);
        }

        /// <devdoc>
        /// Retrieves the value of the field to be databound to the ImageField.
        /// </devdoc>
        protected virtual object GetValue(Control controlContainer, string fieldName, ref PropertyDescriptor cachedDescriptor) {
            Debug.Assert(DataImageUrlField.Length != 0, "Shouldn't be DataBinding without an DataImageUrlField");

            object data = null;
            object dataItem = null;

            if (controlContainer == null) {
                throw new HttpException(SR.GetString(SR.DataControlField_NoContainer));
            }

            // Get the DataItem from the container
            dataItem = DataBinder.GetDataItem(controlContainer);

            if (dataItem == null && !DesignMode) {
                throw new HttpException(SR.GetString(SR.DataItem_Not_Found));
            }
            // Get value of field in data item
            if (cachedDescriptor == null) {
                if (!fieldName.Equals(ThisExpression)) {
                    cachedDescriptor = TypeDescriptor.GetProperties(dataItem).Find(fieldName, true);
                    if ((cachedDescriptor == null) && !DesignMode) {
                        throw new HttpException(SR.GetString(SR.Field_Not_Found, fieldName));
                    }
                }
            }

            if (cachedDescriptor != null && dataItem != null) {
                data = cachedDescriptor.GetValue(dataItem);
            }
            else {
                if (!DesignMode) {
                    data = dataItem;
                }
            }

            return data;
        }

        /// <devdoc>
        /// Initializes the field and resets member variables.
        /// </devdoc>
        public override bool Initialize(bool enableSorting, Control control) {
            base.Initialize(enableSorting, control);
            _imageFieldDesc = null;
            _altTextFieldDesc = null;

            return false;
        }

        /// <devdoc>
        /// <para>Initializes a cell in the DataControlField.</para>
        /// </devdoc>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) {
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            switch (cellType) {
                case DataControlCellType.DataCell:
                    InitializeDataCell(cell, rowState);
                    break;
            }
        }

        protected virtual void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState) {
            Control boundControl = null;

            if (((rowState & DataControlRowState.Edit) != 0 && ReadOnly == false) || (rowState & DataControlRowState.Insert) != 0) {
                TextBox editor = new TextBox();
                cell.Controls.Add(editor);

                if (DataImageUrlField.Length != 0 && (rowState & DataControlRowState.Edit) != 0) {
                    boundControl = editor;
                }
            }
            else if (DataImageUrlField.Length != 0) {
                boundControl = cell;
                Image image = new Image();
                Label label = new Label();
                cell.Controls.Add(image);
                cell.Controls.Add(label);
            }

            if (boundControl != null && Visible) {
                boundControl.DataBinding += new EventHandler(this.OnDataBindField);
            }
        }

        /// <devdoc>
        /// Handles databinding the field and its controls.
        /// </devdoc>
        protected virtual void OnDataBindField(object sender, EventArgs e) {
            Control boundControl = (Control)sender;
            Control controlContainer = boundControl.NamingContainer;
            string urlValue = null;
            string nullImageUrl = NullImageUrl;
            string altText = GetFormattedAlternateText(controlContainer);

            if (DesignMode && (boundControl is TableCell)) {
                if (boundControl.Controls.Count == 0 || !(boundControl.Controls[0] is Image)) {
                    throw new HttpException(SR.GetString(SR.ImageField_WrongControlType, DataImageUrlField));
                }
                ((Image)boundControl.Controls[0]).Visible = false;
                ((TableCell)boundControl).Text = GetDesignTimeValue();
                return;
            }

            object data = GetValue(controlContainer, DataImageUrlField, ref _imageFieldDesc);

            urlValue = FormatImageUrlValue(data);
            if (boundControl is TableCell) {    // read-only
                TableCell cell = (TableCell)boundControl;
                if (cell.Controls.Count < 2 || !(cell.Controls[0] is Image) || !(cell.Controls[1] is Label)) {
                    throw new HttpException(SR.GetString(SR.ImageField_WrongControlType, DataImageUrlField));
                }
                Image image = (Image)cell.Controls[0];
                Label label = (Label)cell.Controls[1];

                label.Visible = false;
                if (urlValue == null || (ConvertEmptyStringToNull && urlValue.Length == 0)) {
                    if (nullImageUrl.Length > 0) {
                        urlValue = nullImageUrl;
                    }
                    else {
                        image.Visible = false;
                        label.Text = NullDisplayText;
                        label.Visible = true;
                    }
                }
                if (!CrossSiteScriptingValidation.IsDangerousUrl(urlValue)) {
                    image.ImageUrl = urlValue;
                }
                image.AlternateText = altText;
            }
            else {  // edit/insert
                if (!(boundControl is TextBox)) {
                    throw new HttpException(SR.GetString(SR.ImageField_WrongControlType, DataImageUrlField));
                }
                ((TextBox)boundControl).Text = data.ToString();
                ((TextBox)boundControl).ToolTip = altText;

                if (data != null) {
                    // size down the textbox for certain types
                    if (data.GetType().IsPrimitive) {
                        ((TextBox)boundControl).Columns = 5;
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>Override with an empty body if the field's controls all support callback.
        ///  Otherwise, override and throw a useful error message about why the field can't support callbacks.</para>
        /// </devdoc>
        public override void ValidateSupportsCallback() {
        }
    }
}

