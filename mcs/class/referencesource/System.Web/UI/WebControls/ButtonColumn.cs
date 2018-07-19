//------------------------------------------------------------------------------
// <copyright file="ButtonColumn.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    /// <para>Creates a column with a set of <see cref='System.Web.UI.WebControls.Button'/>
    /// controls.</para>
    /// </devdoc>
    public class ButtonColumn : DataGridColumn {

        private PropertyDescriptor textFieldDesc;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ButtonColumn'/> class.</para>
        /// </devdoc>
        public ButtonColumn() {
        }



        /// <devdoc>
        ///    <para>Gets or sets the type of button to render in the
        ///       column.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ButtonColumnType.LinkButton),
        WebSysDescriptionAttribute(SR.ButtonColumn_ButtonType)
        ]
        public virtual ButtonColumnType ButtonType {
            get {
                object o = ViewState["ButtonType"];
                if (o != null)
                    return(ButtonColumnType)o;
                return ButtonColumnType.LinkButton;
            }
            set {
                if (value < ButtonColumnType.LinkButton || value > ButtonColumnType.PushButton) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ButtonType"] = value;
                OnColumnChanged();
            }
        }


        [
        DefaultValue(false),
        WebSysDescriptionAttribute(SR.ButtonColumn_CausesValidation)
        ]
        public virtual bool CausesValidation {
            get {
                object o = ViewState["CausesValidation"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                ViewState["CausesValidation"] = value;
                OnColumnChanged();
            }
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
                ViewState["CommandName"] = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the field name from the data model that is
        ///       bound to the <see cref='System.Web.UI.WebControls.ButtonColumn.Text'/> property of the button in this column.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescriptionAttribute(SR.ButtonColumn_DataTextField)
        ]
        public virtual string DataTextField {
            get {
                object o = ViewState["DataTextField"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                ViewState["DataTextField"] = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the string used to format the data bound to
        ///       the <see cref='System.Web.UI.WebControls.ButtonColumn.Text'/> property of the button.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescriptionAttribute(SR.ButtonColumn_DataTextFormatString)
        ]
        public virtual string DataTextFormatString {
            get {
                object o = ViewState["DataTextFormatString"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                ViewState["DataTextFormatString"] = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the caption text displayed on the <see cref='System.Web.UI.WebControls.Button'/>
        /// in this column.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescriptionAttribute(SR.ButtonColumn_Text)
        ]
        public virtual string Text {
            get {
                object o = ViewState["Text"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                ViewState["Text"] = value;
                OnColumnChanged();
            }
        }


        [
        DefaultValue(""),
        WebSysDescriptionAttribute(SR.ButtonColumn_ValidationGroup)
        ]
        public virtual string ValidationGroup {
            get {
                object o = ViewState["ValidationGroup"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["ValidationGroup"] = value;
                OnColumnChanged();
            }
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
        public override void Initialize() {
            base.Initialize();
            textFieldDesc = null;
        }


        /// <devdoc>
        /// <para>Initializes a cell in the <see cref='System.Web.UI.WebControls.ButtonColumn'/> .</para>
        /// </devdoc>
        public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType) {
            base.InitializeCell(cell, columnIndex, itemType);

            if ((itemType != ListItemType.Header) &&
                (itemType != ListItemType.Footer)) {
                WebControl buttonControl = null;

                if (ButtonType == ButtonColumnType.LinkButton) {
                    LinkButton button = new DataGridLinkButton();

                    button.Text = Text;
                    button.CommandName = CommandName;
                    button.CausesValidation = CausesValidation;
                    button.ValidationGroup = ValidationGroup;
                    buttonControl = button;
                }
                else {
                    Button button = new Button();

                    button.Text = Text;
                    button.CommandName = CommandName;
                    button.CausesValidation = CausesValidation;
                    button.ValidationGroup = ValidationGroup;
                    buttonControl = button;
                }

                if (DataTextField.Length != 0) {
                    buttonControl.DataBinding += new EventHandler(this.OnDataBindColumn);
                }

                cell.Controls.Add(buttonControl);
            }
        }


        /// <devdoc>
        /// </devdoc>
        private void OnDataBindColumn(object sender, EventArgs e) {
            Debug.Assert(DataTextField.Length != 0, "Shouldn't be DataBinding without a DataTextField");

            Control boundControl = (Control)sender;
            DataGridItem item = (DataGridItem)boundControl.NamingContainer;
            object dataItem = item.DataItem;

            if (textFieldDesc == null) {
                string dataField = DataTextField;

                textFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(dataField, true);
                if ((textFieldDesc == null) && !DesignMode) {
                    throw new HttpException(SR.GetString(SR.Field_Not_Found, dataField));
                }
            }

            string dataValue;

            if (textFieldDesc != null) {
                object data = textFieldDesc.GetValue(dataItem);
                dataValue = FormatDataTextValue(data);
            }
            else {
                Debug.Assert(DesignMode == true);
                dataValue = SR.GetString(SR.Sample_Databound_Text);
            }

            if (boundControl is LinkButton) {
                ((LinkButton)boundControl).Text = dataValue;
            }
            else {
                Debug.Assert(boundControl is Button, "Expected the bound control to be a Button");
                ((Button)boundControl).Text = dataValue;
            }
        }
    }
}

