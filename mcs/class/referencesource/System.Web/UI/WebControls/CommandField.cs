//------------------------------------------------------------------------------
// <copyright file="CommandField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;

    /// <devdoc>
    /// <para>Creates a special field with buttons for <see langword='Edit'/>,
    /// <see langword='Update'/>, and <see langword='Cancel'/> commands to edit items
    ///    within the selected row.</para>
    /// </devdoc>
    public class CommandField : ButtonFieldBase {


        /// <devdoc>
        /// <para>Initializes a new instance of an <see cref='System.Web.UI.WebControls.CommandField'/> class.</para>
        /// </devdoc>
        public CommandField() {
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.CommandField_CancelImageUrl),
        UrlProperty()
        ]
        public virtual string CancelImageUrl {
            get {
                object o = ViewState["CancelImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["CancelImageUrl"])) {
                    ViewState["CancelImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Indicates the text to display for the <see langword='Cancel'/> command button
        ///    in the field.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        WebSysDefaultValue(SR.CommandField_DefaultCancelCaption),
        WebSysDescription(SR.CommandField_CancelText)
        ]
        public virtual string CancelText {
            get {
                object text = ViewState["CancelText"];
                return text == null ? SR.GetString(SR.CommandField_DefaultCancelCaption) : (string)text;
            }
            set {
                if (!String.Equals(value, ViewState["CancelText"])) {
                    ViewState["CancelText"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.ButtonFieldBase_CausesValidation)
        ]
        public override bool CausesValidation {
            get {
                object o = ViewState["CausesValidation"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                base.CausesValidation = value;
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.CommandField_DeleteImageUrl),
        UrlProperty()
        ]
        public virtual string DeleteImageUrl {
            get {
                object o = ViewState["DeleteImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["DeleteImageUrl"])) {
                    ViewState["DeleteImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Indicates the text to display for the <see langword='Delete'/> command button in
        ///    the field.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        WebSysDefaultValue(SR.CommandField_DefaultDeleteCaption),
        WebSysDescription(SR.CommandField_DeleteText)
        ]
        public virtual string DeleteText {
            get {
                object text = ViewState["DeleteText"];
                return text == null ? SR.GetString(SR.CommandField_DefaultDeleteCaption) : (string)text;
            }
            set {
                if (!String.Equals(value, ViewState["DeleteText"])) {
                    ViewState["DeleteText"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.CommandField_EditImageUrl),
        UrlProperty()
        ]
        public virtual string EditImageUrl {
            get {
                object o = ViewState["EditImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["EditImageUrl"])) {
                    ViewState["EditImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Indicates the text to display for the <see langword='Edit'/> command button in
        ///    the field.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        WebSysDefaultValue(SR.CommandField_DefaultEditCaption),
        WebSysDescription(SR.CommandField_EditText)
        ]
        public virtual string EditText {
            get {
                object text = ViewState["EditText"];
                return text == null ? SR.GetString(SR.CommandField_DefaultEditCaption) : (string)text;
            }
            set {
                if (!String.Equals(value, ViewState["EditText"])) {
                    ViewState["EditText"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.CommandField_InsertImageUrl),
        UrlProperty()
        ]
        public virtual string InsertImageUrl {
            get {
                object o = ViewState["InsertImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["InsertImageUrl"])) {
                    ViewState["InsertImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Indicates the text to display for the <see langword='Insert'/> command button
        ///    in the field.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        WebSysDefaultValue(SR.CommandField_DefaultInsertCaption),
        WebSysDescription(SR.CommandField_InsertText)
        ]
        public virtual string InsertText {
            get {
                object text = ViewState["InsertText"];
                return text == null ? SR.GetString(SR.CommandField_DefaultInsertCaption) : (string)text;
            }
            set {
                if (!String.Equals(value, ViewState["InsertText"])) {
                    ViewState["InsertText"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.CommandField_NewImageUrl),
        UrlProperty()
        ]
        public virtual string NewImageUrl {
            get {
                object o = ViewState["NewImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["NewImageUrl"])) {
                    ViewState["NewImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Indicates the text to display for the <see langword='New'/> command button
        ///    in the field.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        WebSysDefaultValue(SR.CommandField_DefaultNewCaption),
        WebSysDescription(SR.CommandField_NewText)
        ]
        public virtual string NewText {
            get {
                object text = ViewState["NewText"];
                return text == null ? SR.GetString(SR.CommandField_DefaultNewCaption) : (string)text;
            }
            set {
                if (!String.Equals(value, ViewState["NewText"])) {
                    ViewState["NewText"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.CommandField_SelectImageUrl),
        UrlProperty()
        ]
        public virtual string SelectImageUrl {
            get {
                object o = ViewState["SelectImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["SelectImageUrl"])) {
                    ViewState["SelectImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Indicates the text to display for the <see langword='Select'/> command button
        ///    in the field.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        WebSysDefaultValue(SR.CommandField_DefaultSelectCaption),
        WebSysDescription(SR.CommandField_SelectText)
        ]
        public virtual string SelectText {
            get {
                object text = ViewState["SelectText"];
                return text == null ? SR.GetString(SR.CommandField_DefaultSelectCaption) : (string)text;
            }
            set {
                if (!String.Equals(value, ViewState["SelectText"])) {
                    ViewState["SelectText"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.CommandField_ShowCancelButton)
        ]
        public virtual bool ShowCancelButton {
            get {
                object o = ViewState["ShowCancelButton"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                object oldValue = ViewState["ShowCancelButton"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["ShowCancelButton"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.CommandField_ShowDeleteButton)
        ]
        public virtual bool ShowDeleteButton {
            get {
                object o = ViewState["ShowDeleteButton"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                object oldValue = ViewState["ShowDeleteButton"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["ShowDeleteButton"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.CommandField_ShowEditButton)
        ]
        public virtual bool ShowEditButton {
            get {
                object o = ViewState["ShowEditButton"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                object oldValue = ViewState["ShowEditButton"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["ShowEditButton"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.CommandField_ShowSelectButton)
        ]
        public virtual bool ShowSelectButton {
            get {
                object o = ViewState["ShowSelectButton"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                object oldValue = ViewState["ShowSelectButton"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["ShowSelectButton"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.CommandField_ShowInsertButton)
        ]
        public virtual bool ShowInsertButton {
            get {
                object o = ViewState["ShowInsertButton"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                object oldValue = ViewState["ShowInsertButton"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["ShowInsertButton"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.CommandField_UpdateImageUrl),
        UrlProperty()
        ]
        public virtual string UpdateImageUrl {
            get {
                object o = ViewState["UpdateImageUrl"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["UpdateImageUrl"])) {
                    ViewState["UpdateImageUrl"] = value;
                    OnFieldChanged();
                }
            }
        }


        /// <devdoc>
        /// <para>Indicates the text to display for the <see langword='Update'/> command button
        ///    in the field.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Localizable(true),
        WebSysDefaultValue(SR.CommandField_DefaultUpdateCaption),
        WebSysDescription(SR.CommandField_UpdateText)
        ]
        public virtual string UpdateText {
            get {
                object text = ViewState["UpdateText"];
                return text == null ? SR.GetString(SR.CommandField_DefaultUpdateCaption) : (string)text;
            }
            set {
                if (!String.Equals(value, ViewState["UpdateText"])) {
                    ViewState["UpdateText"] = value;
                    OnFieldChanged();
                }
            }
        }

        private void AddButtonToCell(DataControlFieldCell cell,  string commandName, string buttonText, bool causesValidation, string validationGroup, int rowIndex, string imageUrl) {
            IButtonControl button;
            IPostBackContainer container = Control as IPostBackContainer;
            bool setCausesValidation = true;    // the setter on the DataControlButtons throw if there's a container for security

            switch (ButtonType) {
                case ButtonType.Link: {
                    if (container != null && !causesValidation) {
                        button = new DataControlLinkButton(container);
                        setCausesValidation = false;
                    }
                    else {
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

                    ((ImageButton)button).ImageUrl = imageUrl;
                    break;
                }
            }
            
            button.Text = buttonText;
            button.CommandName = commandName;
            button.CommandArgument = rowIndex.ToString(CultureInfo.InvariantCulture);
            if (setCausesValidation) {
                button.CausesValidation = causesValidation;
            }
            button.ValidationGroup = validationGroup;

            cell.Controls.Add((WebControl)button);

        }

        protected override void CopyProperties(DataControlField newField) {
            ((CommandField)newField).CancelImageUrl = CancelImageUrl;
            ((CommandField)newField).CancelText = CancelText;
            ((CommandField)newField).DeleteImageUrl = DeleteImageUrl;
            ((CommandField)newField).DeleteText = DeleteText;
            ((CommandField)newField).EditImageUrl = EditImageUrl;
            ((CommandField)newField).EditText = EditText;
            ((CommandField)newField).InsertImageUrl = InsertImageUrl;
            ((CommandField)newField).InsertText = InsertText;
            ((CommandField)newField).NewImageUrl = NewImageUrl;
            ((CommandField)newField).NewText = NewText;
            ((CommandField)newField).SelectImageUrl = SelectImageUrl;
            ((CommandField)newField).SelectText = SelectText;
            ((CommandField)newField).UpdateImageUrl = UpdateImageUrl;
            ((CommandField)newField).UpdateText = UpdateText;
            ((CommandField)newField).ShowCancelButton = ShowCancelButton;
            ((CommandField)newField).ShowDeleteButton = ShowDeleteButton;
            ((CommandField)newField).ShowEditButton = ShowEditButton;
            ((CommandField)newField).ShowSelectButton = ShowSelectButton;
            ((CommandField)newField).ShowInsertButton = ShowInsertButton;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField() {
            return new CommandField();
        }


        /// <devdoc>
        ///    <para>Initializes a cell within the field.</para>
        /// </devdoc>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            bool showEditButton = ShowEditButton;
            bool showDeleteButton = ShowDeleteButton;
            bool showInsertButton = ShowInsertButton;
            bool showSelectButton = ShowSelectButton;
            bool showCancelButton = ShowCancelButton;
            bool isFirstButton = true;
            bool causesValidation = CausesValidation;
            string validationGroup = ValidationGroup;
            LiteralControl spaceControl;

            if (cellType == DataControlCellType.DataCell) {
                if ((rowState & (DataControlRowState.Edit | DataControlRowState.Insert)) != 0) {
                    if ((rowState & DataControlRowState.Edit) != 0 && showEditButton) {
                        AddButtonToCell(cell,  DataControlCommands.UpdateCommandName, UpdateText, causesValidation, validationGroup, rowIndex, UpdateImageUrl);
                        if (showCancelButton) {
                            spaceControl = new LiteralControl("&nbsp;");
                            cell.Controls.Add(spaceControl);
                            AddButtonToCell(cell,  DataControlCommands.CancelCommandName, CancelText, false, String.Empty, rowIndex, CancelImageUrl);
                        }

                    }
                    if ((rowState & DataControlRowState.Insert) != 0 && showInsertButton) {
                        AddButtonToCell(cell,  DataControlCommands.InsertCommandName, InsertText, causesValidation, validationGroup, rowIndex, InsertImageUrl);
                        if (showCancelButton) {
                            spaceControl = new LiteralControl("&nbsp;");
                            cell.Controls.Add(spaceControl);
                            AddButtonToCell(cell,  DataControlCommands.CancelCommandName, CancelText, false, String.Empty, rowIndex, CancelImageUrl);
                        }
                    }
                }
                else {
                    if (showEditButton) {
                        AddButtonToCell(cell,  DataControlCommands.EditCommandName, EditText, false, String.Empty, rowIndex, EditImageUrl);
                        isFirstButton = false;
                    }
                    if (showDeleteButton) {
                        if (isFirstButton == false) {
                            spaceControl = new LiteralControl("&nbsp;");
                            cell.Controls.Add(spaceControl);
                        }
                        AddButtonToCell(cell,  DataControlCommands.DeleteCommandName, DeleteText, false, String.Empty, rowIndex, DeleteImageUrl);
                        isFirstButton = false;
                    }
                    if (showInsertButton) {
                        if (isFirstButton == false) {
                            spaceControl = new LiteralControl("&nbsp;");
                            cell.Controls.Add(spaceControl);
                        }
                        AddButtonToCell(cell,  DataControlCommands.NewCommandName, NewText, false, String.Empty, rowIndex, NewImageUrl);
                        isFirstButton = false;
                    }
                    if (showSelectButton) {
                        if (isFirstButton == false) {
                            spaceControl = new LiteralControl("&nbsp;");
                            cell.Controls.Add(spaceControl);
                        }
                        AddButtonToCell(cell,  DataControlCommands.SelectCommandName, SelectText, false, String.Empty, rowIndex, SelectImageUrl);
                        isFirstButton = false;
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>Override with an empty body if the field's controls all support callback.
        ///  Otherwise, override and throw a useful error message about why the field can't support callbacks.</para>
        /// </devdoc>
        public override void ValidateSupportsCallback() {
            if (ShowSelectButton) {
                throw new NotSupportedException(SR.GetString(SR.CommandField_CallbacksNotSupported, Control.ID));
            }
        }
    }
}

