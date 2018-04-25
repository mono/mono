//------------------------------------------------------------------------------
// <copyright file="CheckBoxField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Creates a field bounded to a data field in a <see cref='System.Web.UI.WebControls.DataBoundControl'/>.</para>
    /// </devdoc>
    public class CheckBoxField : BoundField {

        private bool _suppressPropertyThrows = false;


        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.WebControls.CheckBoxField'/> class.</para>
        /// </devdoc>
        public CheckBoxField() {
        }

        /// <devdoc>
        ///    <para> Indicates whether to apply the DataFormatString in edit mode</para>
        /// </devdoc>
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool ApplyFormatInEditMode {
            get {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "ApplyFormatInEditMode"));
                }
                return false;
            }
            set {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "ApplyFormatInEditMode"));
                }
            }
        }


        /// <devdoc>
        /// <para> Gets or sets the field name from the data model bound to this field.
        /// Overridden to change the type converter attribute.</para>
        /// </devdoc>
        [
            TypeConverter("System.Web.UI.Design.DataSourceBooleanViewSchemaConverter, " + AssemblyRef.SystemDesign),
        ]
        public override string DataField {
            get {
                return base.DataField;
            }
            set {
                base.DataField = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the display format of data in this
        /// field.</para>
        /// </devdoc>
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string DataFormatString {
            get {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "DataFormatString"));
                }
                return String.Empty;
            }
            set {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "DataFormatString"));
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets a property indicating whether data should be HtmlEncoded when it is displayed to the user.</para>
        /// </devdoc>
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool HtmlEncode {
            get {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "HtmlEncode"));
                }
                return false;
            }
            set {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "HtmlEncode"));
                }
            }
        }


        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool HtmlEncodeFormatString {
            get {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "HtmlEncodeFormatString"));
                }
                return false;
            }
            set {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "HtmlEncodeFormatString"));
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the property that determines what text is displayed if the value
        /// of the field is null.</para>
        /// </devdoc>
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string NullDisplayText {
            get {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "NullDisplayText"));
                }
                return String.Empty;
            }
            set {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "NullDisplayText"));
                }
            }
        }

        protected override bool SupportsHtmlEncode {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the CheckBox's Text property in this
        /// field.</para>
        /// </devdoc>
        [
            Localizable(true),
            WebCategory("Appearance"),
            DefaultValue(""),
            WebSysDescription(SR.CheckBoxField_Text)
        ]
        public virtual string Text {
            get {
                object o = ViewState["Text"];
                if (o != null)
                    return (string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["Text"])) {
                    ViewState["Text"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the property that determines whether the BoundField treats empty string as
        /// null when the field values are extracted.</para>
        /// </devdoc>
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool ConvertEmptyStringToNull {
            get {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "ConvertEmptyStringToNull"));
                }
                return false;
            }
            set {
                if (!_suppressPropertyThrows) {
                    throw new NotSupportedException(SR.GetString(SR.CheckBoxField_NotSupported, "ConvertEmptyStringToNull"));
                }
            }
        }

        protected override void CopyProperties(DataControlField newField) {
            ((CheckBoxField)newField).Text = Text;
            _suppressPropertyThrows = true;
            ((CheckBoxField)newField)._suppressPropertyThrows = true;
            base.CopyProperties(newField);
            _suppressPropertyThrows = false;
            ((CheckBoxField)newField)._suppressPropertyThrows = false;
        }

        protected override DataControlField CreateField() {
            return new CheckBoxField();
        }
        

        /// <devdoc>
        /// Extracts the value(s) from the given cell and puts the value(s) into a dictionary.  Indicate includeReadOnly
        /// to have readonly fields' values inserted into the dictionary.
        /// </devdoc>
        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly) {
            Control childControl = null;
            string dataField = DataField;
            object value = null;

            if (cell.Controls.Count > 0) {
                childControl = cell.Controls[0];

                CheckBox checkBox = childControl as CheckBox;
                if (checkBox != null) {
                    if (includeReadOnly || checkBox.Enabled) {
                        value = checkBox.Checked;
                    }
                }
            }

            if (value != null) {
                if (dictionary.Contains(dataField)) {
                    dictionary[dataField] = value;
                }
                else {
                    dictionary.Add(dataField, value);
                }
            }
        }


        /// <devdoc>
        /// Returns a value to be used for design-time rendering
        /// </devdoc>
        protected override object GetDesignTimeValue() {
            return true;
        }



        /// <devdoc>
        /// <para>Initializes a cell in the DataControlField.</para>
        /// </devdoc>
        protected override void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState) {
            CheckBox childControl = null;
            CheckBox boundControl = null;

            if (((rowState & DataControlRowState.Edit) != 0 && ReadOnly == false) || (rowState & DataControlRowState.Insert) != 0) {
                // 
                CheckBox editor = new CheckBox();
                editor.ToolTip = HeaderText;
                childControl = editor;

                if (DataField.Length != 0 && (rowState & DataControlRowState.Edit) != 0) {
                    boundControl = editor;
                }
            }
            else if (DataField.Length != 0) {
                CheckBox editor = new CheckBox();
                editor.Text = Text;
                editor.Enabled = false;
                childControl = editor;
                boundControl = editor;
            }

            if (childControl != null) {
                cell.Controls.Add(childControl);
            }

            if (boundControl != null && Visible) {
                boundControl.DataBinding += new EventHandler(this.OnDataBindField);
            }
        }


        /// <devdoc>
        /// Performs databinding the given field with data from the data source.
        /// </devdoc>
        protected override void OnDataBindField(object sender, EventArgs e) {
            Control boundControl = (Control)sender;
            Control controlContainer = boundControl.NamingContainer;


            object data = GetValue(controlContainer);

            if (!(boundControl is CheckBox)) {
                throw new HttpException(SR.GetString(SR.CheckBoxField_WrongControlType, DataField));
            }
            if (DataBinder.IsNull(data)) {
                ((CheckBox)boundControl).Checked = false;
            }
            else {
                if (data is Boolean) {
                    ((CheckBox)boundControl).Checked = (Boolean)data;
                }
                else {
                    try {
                        ((CheckBox)boundControl).Checked = Boolean.Parse(data.ToString());
                    }
                    catch (FormatException fe) {
                        throw new HttpException(SR.GetString(SR.CheckBoxField_CouldntParseAsBoolean, DataField), fe);
                    }
                }
            }
            ((CheckBox)boundControl).Text = Text;
        }

        /// <devdoc>
        /// <para>Override with an empty body if the field's controls all support callback.
        ///  Otherwise, override and throw a useful error message about why the field can't support callbacks.</para>
        /// </devdoc>
        public override void ValidateSupportsCallback() {
        }
    }
}

