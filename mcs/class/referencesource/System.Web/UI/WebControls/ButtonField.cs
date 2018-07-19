//------------------------------------------------------------------------------
// <copyright file="yyd.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web.Util;


    /// <devdoc>
    /// <para>Creates a field with a set of <see cref='System.Web.UI.WebControls.Button'/>
    /// controls.</para>
    /// </devdoc>
    public class ButtonField : ButtonFieldBase {

        private PropertyDescriptor textFieldDesc;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ButtonField'/> class.</para>
        /// </devdoc>
        public ButtonField() {
        }


        /// <devdoc>
        /// <para>Gets or sets the command to perform when this <see cref='System.Web.UI.WebControls.Button'/>
        /// is clicked.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescriptionAttribute(SR.WebControl_CommandName)
        ]
        public virtual string CommandName {
            get {
                object o = ViewState["CommandName"];
                if (o != null)
                    return(string)o;
                return string.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["CommandName"])) {
                    ViewState["CommandName"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the field name from the data model that is
        ///       bound to the <see cref='System.Web.UI.WebControls.ButtonField.Text'/> property of the button in this field.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescriptionAttribute(SR.ButtonField_DataTextField),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign)
        ]
        public virtual string DataTextField {
            get {
                object o = ViewState["DataTextField"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["DataTextField"])) {
                    ViewState["DataTextField"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the string used to format the data bound to
        ///       the <see cref='System.Web.UI.WebControls.ButtonField.Text'/> property of the button.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescriptionAttribute(SR.ButtonField_DataTextFormatString)
        ]
        public virtual string DataTextFormatString {
            get {
                object o = ViewState["DataTextFormatString"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["DataTextFormatString"])) {
                    ViewState["DataTextFormatString"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescriptionAttribute(SR.ButtonField_ImageUrl),
        UrlProperty()
        ]
        public virtual string ImageUrl {
            get {
                object o = ViewState["ImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["ImageUrl"])) {
                    ViewState["ImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the caption text displayed on the <see cref='System.Web.UI.WebControls.Button'/>
        /// in this field.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescriptionAttribute(SR.ButtonField_Text)
        ]
        public virtual string Text {
            get {
                object o = ViewState["Text"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["Text"])) {
                    ViewState["Text"] = value;
                    OnFieldChanged();
                }
            }
        }

        protected override void CopyProperties(DataControlField newField) {
            ((ButtonField)newField).CommandName = CommandName;
            ((ButtonField)newField).DataTextField = DataTextField;
            ((ButtonField)newField).DataTextFormatString = DataTextFormatString;
            ((ButtonField)newField).ImageUrl = ImageUrl;
            ((ButtonField)newField).Text = Text;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField() {
            return new ButtonField();
        }
        

        /// <devdoc>
        /// </devdoc>
        protected virtual string FormatDataTextValue(object dataTextValue) {
            string formattedTextValue = String.Empty;

            if (!DataBinder.IsNull(dataTextValue)) {
                string formatting = DataTextFormatString;
                if (formatting.Length == 0) {
                    formattedTextValue = dataTextValue.ToString();
                }
                else {
                    formattedTextValue = String.Format(CultureInfo.CurrentCulture, formatting, dataTextValue);
                }
            }

            return formattedTextValue;
        }


        /// <devdoc>
        /// </devdoc>
        public override bool Initialize(bool sortingEnabled, Control control) {
            base.Initialize(sortingEnabled, control);
            textFieldDesc = null;
            return false;
        }


        /// <devdoc>
        /// <para>Initializes a cell in the <see cref='System.Web.UI.WebControls.ButtonField'/> .</para>
        /// </devdoc>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) {
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            if ((cellType != DataControlCellType.Header) &&
                (cellType != DataControlCellType.Footer)) {
                IButtonControl button;
                IPostBackContainer container = Control as IPostBackContainer;
                bool causesValidation = CausesValidation;
                bool setCausesValidation = true;    // the setter on the DataControlButtons throw if there's a container for security

                switch(ButtonType) {
                    case ButtonType.Link: {
                        if (container != null && !causesValidation) {
                            button = new DataControlLinkButton(container);
                            setCausesValidation = false;
                        }
                        else {
                            // use DataControlLinkButton because it uses the right forecolor
                            button = new DataControlLinkButton(null);
                        }
                        break;
                    }
                    case ButtonType.Button: {
                        if (container != null && !causesValidation) {
                            button = new DataControlButton(container);
                            setCausesValidation = false;
                        }
                        else {
                            button = new Button();
                        }
                        break;
                    }
                    case ButtonType.Image:
                    default: {
                        if (container != null && !causesValidation) {
                            button = new DataControlImageButton(container);
                            setCausesValidation = false;
                        }
                        else {
                            button = new ImageButton();
                        }

                        ((ImageButton)button).ImageUrl = ImageUrl;
                        break;
                    }
                }

                button.Text = Text;
                button.CommandName = CommandName;
                button.CommandArgument = rowIndex.ToString(CultureInfo.InvariantCulture);
                if (setCausesValidation) {
                    button.CausesValidation = causesValidation;
                }
                button.ValidationGroup = ValidationGroup;
                
                if (DataTextField.Length != 0 && Visible) {
                    ((WebControl)button).DataBinding += new EventHandler(this.OnDataBindField);
                }

                cell.Controls.Add((WebControl)button);
            }
        }


        /// <devdoc>
        /// </devdoc>
        private void OnDataBindField(object sender, EventArgs e) {
            Debug.Assert(DataTextField.Length != 0, "Shouldn't be DataBinding without a DataTextField");

            Control boundControl = (Control)sender;
            Control controlContainer = boundControl.NamingContainer;
            string dataValue;
            object dataItem = null;

            if (controlContainer == null) {
                throw new HttpException(SR.GetString(SR.DataControlField_NoContainer));
            }

            // Get the DataItem from the container
            dataItem = DataBinder.GetDataItem(controlContainer);

            if (dataItem == null && !DesignMode) {
                throw new HttpException(SR.GetString(SR.DataItem_Not_Found));
            }

           if (textFieldDesc == null && dataItem != null) {
                string dataField = DataTextField;

                textFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(dataField, true);
                if ((textFieldDesc == null) && !DesignMode) {
                    throw new HttpException(SR.GetString(SR.Field_Not_Found, dataField));
                }
            }

            if (textFieldDesc != null && dataItem != null) {
                object data = textFieldDesc.GetValue(dataItem);
                dataValue = FormatDataTextValue(data);
            }
            else {
                Debug.Assert(DesignMode == true);
                dataValue = SR.GetString(SR.Sample_Databound_Text);
            }

            Debug.Assert(boundControl is IButtonControl, "Expected the bound control to be an IButtonControl");
            ((IButtonControl)boundControl).Text = dataValue;
        }

        /// <devdoc>
        /// <para>Override with an empty body if the field's controls all support callback.
        ///  Otherwise, override and throw a useful error message about why the field can't support callbacks.</para>
        /// </devdoc>
        public override void ValidateSupportsCallback() {
        }
    }
}

