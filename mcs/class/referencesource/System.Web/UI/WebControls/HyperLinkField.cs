//------------------------------------------------------------------------------
// <copyright file="HyperLinkField.cs" company="Microsoft">
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
    /// <para>Creates a field within the <see cref='System.Web.UI.WebControls.DataBoundControl'/> containing hyperlinks that
    ///    navigate to specified URLs.</para>
    /// </devdoc>
    public class HyperLinkField : DataControlField {

        private PropertyDescriptor textFieldDesc;
        private PropertyDescriptor[] urlFieldDescs;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.HyperLinkField'/> class.</para>
        /// </devdoc>
        public HyperLinkField() {
        }


        /// <devdoc>
        /// <para>Gets or sets the fields in the DataSource that provides the URL of the page to navigate to.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.DataFieldEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        TypeConverterAttribute(typeof(StringArrayConverter)),
        WebSysDescription(SR.HyperLinkField_DataNavigateUrlFields)
        ]
        public virtual string[] DataNavigateUrlFields {
            get {
                object o = ViewState["DataNavigateUrlFields"];
                if (o != null) {
                    return(string[])((string[])o).Clone();
                }

                return new string[0];
            }
            set {
                string[] oldValue = ViewState["DataNavigateUrlFields"] as string[];
                if (!StringArraysEqual(oldValue, value)) {
                    if (value != null) {
                        ViewState["DataNavigateUrlFields"] = (string[])value.Clone();
                    }
                    else {
                        ViewState["DataNavigateUrlFields"] = null;
                    }
                    OnFieldChanged();
                }
            }
        }

        /// <devdoc>
        /// <para>Gets or sets the formatting applied to the <see cref='System.Web.UI.WebControls.HyperLinkField.NavigateUrl'/>
        /// property.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescription(SR.HyperLinkField_DataNavigateUrlFormatString)
        ]
        public virtual string DataNavigateUrlFormatString {
            get {
                object o = ViewState["DataNavigateUrlFormatString"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["DataNavigateUrlFormatString"])) {
                    ViewState["DataNavigateUrlFormatString"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the field in the DataSource that will be used as the source of
        /// data for the <see cref='System.Web.UI.WebControls.HyperLinkField.Text'/> property.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebSysDescription(SR.HyperLinkField_DataTextField)
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
        /// <para>Gets or sets the formatting applied to the <see cref='System.Web.UI.WebControls.HyperLinkField.Text'/>
        /// property.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescription(SR.HyperLinkField_DataTextFormatString)
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


        /// <devdoc>
        ///    <para>Gets or sets the URL to navigate to when the hyperlink is clicked.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.HyperLinkField_NavigateUrl)
        ]
        public virtual string NavigateUrl {
            get {
                object o = ViewState["NavigateUrl"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["NavigateUrl"])) {
                    ViewState["NavigateUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the window or target frame that is
        ///       used to display the contents resulting from the hyperlink.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        TypeConverter(typeof(TargetConverter)),
        WebSysDescription(SR.HyperLink_Target)
        ]
        public virtual string Target {
            get {
                object o = ViewState["Target"];
                if (o != null)
                    return(string)o;
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["Target"])) {
                    ViewState["Target"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text to display for the hyperlink.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.HyperLinkField_Text)
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
            ((HyperLinkField)newField).DataNavigateUrlFields = DataNavigateUrlFields;   //the getter and setter both call Clone
            ((HyperLinkField)newField).DataNavigateUrlFormatString = DataNavigateUrlFormatString;
            ((HyperLinkField)newField).DataTextField = DataTextField;
            ((HyperLinkField)newField).DataTextFormatString = DataTextFormatString;
            ((HyperLinkField)newField).NavigateUrl = NavigateUrl;
            ((HyperLinkField)newField).Target = Target;
            ((HyperLinkField)newField).Text = Text;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField() {
            return new HyperLinkField();
        }


        /// <devdoc>
        /// </devdoc>
        protected virtual string FormatDataNavigateUrlValue(object[] dataUrlValues) {
            string formattedUrlValue = String.Empty;

            if ((dataUrlValues != null)) {
                string formatting = DataNavigateUrlFormatString;
                if (formatting.Length == 0) {
                    if (dataUrlValues.Length > 0 && !DataBinder.IsNull(dataUrlValues[0])) {
                        formattedUrlValue = dataUrlValues[0].ToString();
                    }
                }
                else {
                    formattedUrlValue = String.Format(CultureInfo.CurrentCulture, formatting, dataUrlValues);
                }
            }

            return formattedUrlValue;
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
        public override bool Initialize(bool enableSorting, Control control) {
            base.Initialize(enableSorting, control);
            textFieldDesc = null;
            urlFieldDescs = null;
            return false;
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes the cell representing this field with the
        ///       contained hyperlink.</para>
        /// </devdoc>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) {
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            if (cellType == DataControlCellType.DataCell) {
                HyperLink hyperLink = new HyperLink();

                hyperLink.Text = Text;
                hyperLink.NavigateUrl = NavigateUrl;
                hyperLink.Target = Target;

                if (((rowState & DataControlRowState.Insert) == 0) && Visible) {
                    if ((DataNavigateUrlFields.Length != 0) ||
                        (DataTextField.Length != 0)) {
                        hyperLink.DataBinding += new EventHandler(this.OnDataBindField);
                    }
    
                    cell.Controls.Add(hyperLink);
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        private void OnDataBindField(object sender, EventArgs e) {
            Debug.Assert((DataTextField.Length != 0) || (DataNavigateUrlFields.Length != 0),
                         "Shouldn't be DataBinding without a DataTextField and DataNavigateUrlField");

            HyperLink boundControl = (HyperLink)sender;
            Control controlContainer = boundControl.NamingContainer;
            object dataItem = null;


            if (controlContainer == null) {
                throw new HttpException(SR.GetString(SR.DataControlField_NoContainer));
            }

            // Get the DataItem from the container
            dataItem = DataBinder.GetDataItem(controlContainer);

            if (dataItem == null && !DesignMode) {
                throw new HttpException(SR.GetString(SR.DataItem_Not_Found));
            }

            if ((textFieldDesc == null) && (urlFieldDescs == null)) {

                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(dataItem);
                string fieldName;

                fieldName = DataTextField;
                if (fieldName.Length != 0) {
                    textFieldDesc = props.Find(fieldName, true);
                    if ((textFieldDesc == null) && !DesignMode) {
                        throw new HttpException(SR.GetString(SR.Field_Not_Found, fieldName));
                    }
                }

                string[] dataNavigateUrlFields = DataNavigateUrlFields;
                int dataNavigateUrlFieldsLength = dataNavigateUrlFields.Length;
                urlFieldDescs = new PropertyDescriptor[dataNavigateUrlFieldsLength];

                for (int i = 0; i < dataNavigateUrlFieldsLength; i++) {
                    fieldName = dataNavigateUrlFields[i];
                    if (fieldName.Length != 0) {
                        urlFieldDescs[i] = props.Find(fieldName, true);
                        if ((urlFieldDescs[i] == null) && !DesignMode) {
                            throw new HttpException(SR.GetString(SR.Field_Not_Found, fieldName));
                        }
                    }
                }
            }

            string dataTextValue = String.Empty;
            if (textFieldDesc != null && dataItem != null) {
                object data = textFieldDesc.GetValue(dataItem);
                dataTextValue = FormatDataTextValue(data);
            }
            if (DesignMode && (DataTextField.Length != 0) && dataTextValue.Length == 0) {
                dataTextValue = SR.GetString(SR.Sample_Databound_Text);
            }

            if (dataTextValue.Length > 0) {
                boundControl.Text = dataTextValue;
            }

            int urlFieldDescsLength = urlFieldDescs.Length;
            string dataNavValue = String.Empty;
            if (urlFieldDescs != null && urlFieldDescsLength > 0 && dataItem != null) {
                object[] data = new object[urlFieldDescsLength];

                for (int i = 0; i < urlFieldDescsLength; i++) {
                    if (urlFieldDescs[i] != null) {
                        data[i] = urlFieldDescs[i].GetValue(dataItem);
                    }
                }
                string urlValue = FormatDataNavigateUrlValue(data);
                if (!CrossSiteScriptingValidation.IsDangerousUrl(urlValue)) {
                    dataNavValue = urlValue;
                }
            }
            if (DesignMode && (DataNavigateUrlFields.Length != 0) && dataNavValue.Length == 0) {
                dataNavValue = "url";
            }

            if (dataNavValue.Length > 0) {
                boundControl.NavigateUrl = dataNavValue;
            }
        }

        private bool StringArraysEqual(string[] arr1, string[] arr2) {
            if (arr1 == null && arr2 == null) {
                return true;
            }
            if (arr1 == null || arr2 == null) {
                return false;
            }
            if (arr1.Length != arr2.Length) {
                return false;
            }
            for (int i = 0; i < arr1.Length; i++) {
                if (!String.Equals(arr1[i], arr2[i])) {
                    return false;
                }
            }
            return true;
        }
        
        /// <devdoc>
        /// <para>Override with an empty body if the field's controls all support callback.
        ///  Otherwise, override and throw a useful error message about why the field can't support callbacks.</para>
        /// </devdoc>
        public override void ValidateSupportsCallback() {
        }
    }
}

