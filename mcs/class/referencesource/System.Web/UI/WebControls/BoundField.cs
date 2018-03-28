//------------------------------------------------------------------------------
// <copyright file="BoundField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.Util;


    /// <devdoc>
    /// <para>Creates a field bounded to a data field in a <see cref='System.Web.UI.WebControls.DataBoundControl'/>.</para>
    /// </devdoc>
    public class BoundField : DataControlField {


        /// <devdoc>
        ///    <para>Specifies a string that represents "this". This field is read-only. </para>
        /// </devdoc>
        public static readonly string ThisExpression = "!";
        private static readonly string _expressionPartSeparator = ".";

        private PropertyDescriptor _boundFieldDesc;
        private bool _boundFieldDescInitialized;
        string _dataField;
        string _dataFormatString;
        bool _htmlEncode;
        bool _htmlEncodeSet = false;
        bool _suppressHeaderTextFieldChange;
        private bool _htmlEncodeFormatString;
        private bool _htmlEncodeFormatStringSet;


        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.WebControls.BoundField'/> class.</para>
        /// </devdoc>
        public BoundField() {
        }

        /// <summary>
        /// Determines whether the control validates client input or not, defaults to inherit from parent.
        /// </summary>
        [
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_ValidateRequestMode),
        DefaultValue(ValidateRequestMode.Inherit)
        ]
        public new ValidateRequestMode ValidateRequestMode {
            get {
                return base.ValidateRequestMode;
            }
            set {
                base.ValidateRequestMode = value;
            }
        }

        /// <devdoc>
        ///    <para> Indicates whether to apply the DataFormatString in edit mode</para>
        /// </devdoc>
        [
            WebCategory("Behavior"),
            DefaultValue(false),
            WebSysDescription(SR.BoundField_ApplyFormatInEditMode)
        ]
        public virtual bool ApplyFormatInEditMode {
            get {
                object o = ViewState["ApplyFormatInEditMode"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                ViewState["ApplyFormatInEditMode"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the property that determines whether the BoundField treats empty string as
        ///    null when the field values are extracted.</para>
        /// </devdoc>
        [
            WebCategory("Behavior"),
            DefaultValue(true),
            WebSysDescription(SR.BoundField_ConvertEmptyStringToNull)
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
        

        /// <devdoc>
        ///    <para> Gets or sets the field name from the data model bound to this field.</para>
        /// </devdoc>
        [
            WebCategory("Data"),
            DefaultValue(""),
            TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
            WebSysDescription(SR.BoundField_DataField)
        ]
        public virtual string DataField {
            get {
                if (_dataField == null) {
                    object o = ViewState["DataField"];
                    if (o != null)
                        _dataField = (string)o;
                    else
                        _dataField = String.Empty;
                    }
                return _dataField;
            }
            set {
                if (!String.Equals(value, ViewState["DataField"])) {
                    ViewState["DataField"] = value;
                    _dataField = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the display format of data in this
        ///       field.</para>
        /// </devdoc>
        [
            WebCategory("Data"),
            DefaultValue(""),
            WebSysDescription(SR.BoundField_DataFormatString)
        ]
        public virtual string DataFormatString {
            get {
                if (_dataFormatString == null) {
                    object o = ViewState["DataFormatString"];
                    if (o != null)
                        _dataFormatString = (string)o;
                    else 
                        _dataFormatString = String.Empty;
                }
                return _dataFormatString;
            }
            set {
                if (!String.Equals(value, ViewState["DataFormatString"])) {
                    ViewState["DataFormatString"] = value;
                    _dataFormatString = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the text displayed in the header of the
        /// System.Web.UI.WebControls.Field.</para>
        /// </devdoc>
        public override string HeaderText {
            get {
                return base.HeaderText;
            }
            set {
                if (!String.Equals(value, ViewState["HeaderText"])) {
                    ViewState["HeaderText"] = value;
                    if (!_suppressHeaderTextFieldChange) {
                        OnFieldChanged();
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>Gets or sets a property indicating whether data should be HtmlEncoded when it is displayed to the user.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.BoundField_HtmlEncode)
        ]
        public virtual bool HtmlEncode {
            get {
                if (!_htmlEncodeSet) {
                    object o = ViewState["HtmlEncode"];
                    if (o != null) {
                        _htmlEncode = (bool)o;
                    }
                    else {
                        _htmlEncode = true;
                    }
                    _htmlEncodeSet = true;
                }
                return _htmlEncode;
            }
            set {
                object oldValue = ViewState["HtmlEncode"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["HtmlEncode"] = value;
                    _htmlEncode = value;
                    _htmlEncodeSet = true;
                    OnFieldChanged();
                }
            }
        }

        /// <devdoc>
        /// <para>Gets or sets a property indicating whether the format string should be HtmlEncoded
        /// when it is displayed to the user.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        ]
        public virtual bool HtmlEncodeFormatString {
            get {
                if (!_htmlEncodeFormatStringSet) {
                    object o = ViewState["HtmlEncodeFormatString"];
                    if (o != null) {
                        _htmlEncodeFormatString = (bool)o;
                    }
                    else {
                        _htmlEncodeFormatString = true;
                    }
                    _htmlEncodeFormatStringSet = true;
                }
                return _htmlEncodeFormatString;
            }
            set {
                object oldValue = ViewState["HtmlEncodeFormatString"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["HtmlEncodeFormatString"] = value;
                    _htmlEncodeFormatString = value;
                    _htmlEncodeFormatStringSet = true;
                    OnFieldChanged();
                }
            }
        }
        /// <devdoc>
        ///    <para>Gets or sets the property that determines what text is displayed if the value
        ///    of the field is null.</para>
        /// </devdoc>
        [
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


        /// <devdoc>
        ///    <para>Gets or sets the property that prevents modification to data
        ///       in this field.</para>
        /// </devdoc>
        [
            WebCategory("Behavior"),
            DefaultValue(false),
            WebSysDescription(SR.BoundField_ReadOnly)
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

        protected virtual bool SupportsHtmlEncode {
            get {
                return true;
            }
        }

        protected override void CopyProperties(DataControlField newField) {
            ((BoundField)newField).ApplyFormatInEditMode = ApplyFormatInEditMode;
            ((BoundField)newField).ConvertEmptyStringToNull = ConvertEmptyStringToNull;
            ((BoundField)newField).DataField = DataField;
            ((BoundField)newField).DataFormatString = DataFormatString;
            ((BoundField)newField).HtmlEncode = HtmlEncode;
            ((BoundField)newField).HtmlEncodeFormatString = HtmlEncodeFormatString;
            ((BoundField)newField).NullDisplayText = NullDisplayText;
            ((BoundField)newField).ReadOnly = ReadOnly;

            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField() {
            return new BoundField();
        }
        

        /// <devdoc>
        /// Extracts the value(s) from the given cell and puts the value(s) into a dictionary.  Indicate includeReadOnly
        /// to have readonly fields' values inserted into the dictionary.
        /// </devdoc>
        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly) {
            Control childControl = null;
            string dataField = DataField;
            object value = null;
            string nullDisplayText = NullDisplayText;

            if (((rowState & DataControlRowState.Insert) != 0) && !InsertVisible) {
                return;
            }

            if (cell.Controls.Count > 0) {
                childControl = cell.Controls[0];

                TextBox editBox = childControl as TextBox;
                if (editBox != null) {
                    value = editBox.Text;
                }
            }
            else {
                if (includeReadOnly == true) {
                    string cellText = cell.Text;
                    if (cellText == "&nbsp;") { // nothing HtmlEncodes to &nbsp;, so we know that this means it was empty.
                        value = String.Empty;
                    }
                    else {
                        if (SupportsHtmlEncode && HtmlEncode) {
                            value = HttpUtility.HtmlDecode(cellText);
                        }
                        else {
                            value = cellText;
                        }
                    }
                }
            }

            if (value != null) {
                if ((value is string) && (((string)value).Length == 0) && ConvertEmptyStringToNull) {
                    value = null;
                }

                if (value is string && (string)value == nullDisplayText && nullDisplayText.Length > 0) {
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
        /// Returns the value of the field formatted as text for the cell.
        /// </devdoc>
        protected virtual string FormatDataValue(object dataValue, bool encode) {
            string formattedValue = String.Empty;

            if (!DataBinder.IsNull(dataValue)) {
                string dataValueString = dataValue.ToString();
                string formatting = DataFormatString;
                int dataValueStringLength = dataValueString.Length;

                if (!HtmlEncodeFormatString) {
                    // Back-compat (Whidbey) behavior when HtmlEncodeFormatString=false
                    if (dataValueStringLength > 0 && encode) {
                        dataValueString = HttpUtility.HtmlEncode(dataValueString);
                    }

                    if (dataValueStringLength == 0 && ConvertEmptyStringToNull) {
                        formattedValue = NullDisplayText;
                    }
                    else if (formatting.Length == 0) {
                        formattedValue = dataValueString;
                    }
                    else {
                        if (encode) {
                            formattedValue = String.Format(CultureInfo.CurrentCulture, formatting, dataValueString);
                        }
                        else {
                            formattedValue = String.Format(CultureInfo.CurrentCulture, formatting, dataValue);
                        }
                    }
                }
                else {
                    // New default behavior (Orcas) when HtmlEncodeFormatString=true

                    // If the result is still empty and ConvertEmptyStringToNull=true, replace the value with the NullDisplayText
                    if (dataValueStringLength == 0 && ConvertEmptyStringToNull) {
                        dataValueString = NullDisplayText;
                    }
                    else {
                        // If there's a format string, apply it to the raw data value
                        // If there's no format string, then dataValueString already has the right value
                        if (!String.IsNullOrEmpty(formatting)) {
                            dataValueString = String.Format(CultureInfo.CurrentCulture, formatting, dataValue);
                        }

                        // Optionally HTML encode the value (including the format string, if any was applied)
                        if (!String.IsNullOrEmpty(dataValueString) && encode) {
                            dataValueString = HttpUtility.HtmlEncode(dataValueString);
                        }
                    }

                    formattedValue = dataValueString;
                }
            }
            else {
                formattedValue = NullDisplayText;
            }

            return formattedValue;
        }


        /// <devdoc>
        /// Returns a value to be used for design-time rendering
        /// </devdoc>
        protected virtual object GetDesignTimeValue() {
            return SR.GetString(SR.Sample_Databound_Text);
        }


        /// <devdoc>
        /// Retrieves the value of the field to be databound to the BoundField.
        /// </devdoc>
        protected virtual object GetValue(Control controlContainer) {
            Debug.Assert(DataField.Length != 0, "Shouldn't be DataBinding without a DataField");

            object data = null;
            object dataItem = null;
            string boundField = DataField;

            if (controlContainer == null) {
                throw new HttpException(SR.GetString(SR.DataControlField_NoContainer));
            }

            dataItem = DataBinder.GetDataItem(controlContainer);

            if (dataItem == null) {
                if (DesignMode) {
                    return GetDesignTimeValue();
                }
                else {
                    throw new HttpException(SR.GetString(SR.DataItem_Not_Found));
                }
            }

            if (boundField.Equals(ThisExpression)) {
                if (DesignMode) {
                    return GetDesignTimeValue();
                }
                else {
                    return dataItem;
                }
            }

            if (!TryGetSimplePropertyValue(dataItem, out data)) {
                data = DataBinder.Eval(dataItem, boundField);
            }

            return data;
        }

        private bool TryGetSimplePropertyValue(object dataItem, out object data) {
            string boundField = DataField;
            data = null;

            if (!_boundFieldDescInitialized) {
                //For simple properties , we cache the property descriptor for performance reasons.
                _boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(boundField, true);
                _boundFieldDescInitialized = true;
            }

            if (_boundFieldDesc != null) {
                // simple property case
                data = _boundFieldDesc.GetValue(dataItem);
                return true;
            }
            else if (DesignMode) {
                data = GetDesignTimeValue();
                return true;
            }
            else if (!boundField.Contains(_expressionPartSeparator)) {
                throw new HttpException(SR.GetString(SR.Field_Not_Found, boundField));
            }
            else {
                // complex property case
                return false;
            }
        }

        /// <devdoc>
        /// Initializes the field and resets member variables.
        /// </devdoc>
        public override bool Initialize(bool enableSorting, Control control) {
            base.Initialize(enableSorting, control);
            _boundFieldDesc = null;
            _boundFieldDescInitialized = false;
            return false;
        }


        /// <devdoc>
        /// <para>Initializes a cell in the DataControlField.</para>
        /// </devdoc>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) {
            string unencodedHeaderText = null;
            bool changedHeaderText = false;
            bool encode = false;
            // if this is a header cell and we're htmlEncoding, htmlEncode the HeaderText before the base class tries to render it
            if (cellType == DataControlCellType.Header && SupportsHtmlEncode && HtmlEncode) {
                unencodedHeaderText = HeaderText;
                encode = true;
            }

            if (encode && !String.IsNullOrEmpty(unencodedHeaderText)) {
                _suppressHeaderTextFieldChange = true;
                HeaderText = HttpUtility.HtmlEncode(unencodedHeaderText);
                changedHeaderText = true;
            }
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            
            // restore the HeaderText property
            if (changedHeaderText) {
                HeaderText = unencodedHeaderText;
                _suppressHeaderTextFieldChange = false;
            }
            switch (cellType) {
                case DataControlCellType.DataCell:
                    InitializeDataCell(cell, rowState);
                    break;
            }
        }


        protected virtual void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState) {
            Control childControl = null;
            Control boundControl = null;

            if (((rowState & DataControlRowState.Edit) != 0 && ReadOnly == false) || (rowState & DataControlRowState.Insert) != 0) {
                // 
                TextBox editor = new TextBox();
                editor.ToolTip = HeaderText;
                childControl = editor;

                if (DataField.Length != 0 && (rowState & DataControlRowState.Edit) != 0) {
                    boundControl = editor;
                }
            }
            else if (DataField.Length != 0) {
                boundControl = cell;
            }

            if (childControl != null) {
                cell.Controls.Add(childControl);
            }

            if (boundControl != null && Visible) {
                boundControl.DataBinding += new EventHandler(this.OnDataBindField);
            }
        }


        /// <devdoc>
        /// </devdoc>
        protected virtual void OnDataBindField(object sender, EventArgs e) {
            Control boundControl = (Control)sender;
            Control controlContainer = boundControl.NamingContainer;

            object data = GetValue(controlContainer);
            bool encodeValue = SupportsHtmlEncode && HtmlEncode && boundControl is TableCell;
            string dataValue = FormatDataValue(data, encodeValue);

            if (boundControl is TableCell) {
                if (dataValue.Length == 0) {
                    dataValue = "&nbsp;";
                }
                ((TableCell)boundControl).Text = dataValue;
            }
            else {
                if (!(boundControl is TextBox)) {
                    throw new HttpException(SR.GetString(SR.BoundField_WrongControlType, DataField));
                }
                
                if (ApplyFormatInEditMode) {
                    ((TextBox)boundControl).Text = dataValue;
                }
                else {
                    if (data != null) {
                        ((TextBox)boundControl).Text = data.ToString();
                    }
                }
                
                if (data != null) {
                    // size down the textbox for certain types
                    if (data.GetType().IsPrimitive) {
                        ((TextBox)boundControl).Columns = 5;
                    }
                }
            }
        }

        protected override void LoadViewState(object state) {
             // DevDiv Bugs 188902: Clear cached values as they may be stale after ViewState loads.
             _dataField = null;
             _dataFormatString = null;
             _htmlEncodeSet = false;
             _htmlEncodeFormatStringSet = false;

             base.LoadViewState(state);
        }

        /// <devdoc>
        /// <para>Override with an empty body if the field's controls all support callback.
        ///  Otherwise, override and throw a useful error message about why the field can't support callbacks.</para>
        /// </devdoc>
        public override void ValidateSupportsCallback() {
        }
    }
}

