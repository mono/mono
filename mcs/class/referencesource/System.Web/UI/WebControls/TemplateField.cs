//------------------------------------------------------------------------------
// <copyright file="TemplateField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;


    /// <devdoc>
    ///    <para>Defines the template for controls layout within a 
    ///    <see cref='System.Web.UI.WebControls.DataBoundControl'/> 
    ///    field.</para>
    /// </devdoc>
    // 


    public class TemplateField : DataControlField {

        private ITemplate headerTemplate;
        private ITemplate footerTemplate;
        private ITemplate itemTemplate;
        private ITemplate editItemTemplate;
        private ITemplate alternatingItemTemplate;
        private ITemplate insertItemTemplate;

        

        /// <devdoc>
        ///    Initializes a new instance of the <see cref='System.Web.UI.WebControls.TemplateField'/> class.
        /// </devdoc>
        public TemplateField() {
        }


        /// <devdoc>
        /// <para>Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how alternating items are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateField_AlternatingItemTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(IDataItemContainer), BindingDirection.TwoWay)
        ]
        public virtual ITemplate AlternatingItemTemplate {
            get {
                return alternatingItemTemplate;
            }
            set {
                alternatingItemTemplate = value;
                OnFieldChanged();
            }
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
        

        /// <devdoc>
        /// <para>Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how rows in edit mode are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateField_EditItemTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(IDataItemContainer), BindingDirection.TwoWay)
        ]
        public virtual ITemplate EditItemTemplate {
            get {
                return editItemTemplate;
            }
            set {
                editItemTemplate = value;
                OnFieldChanged();
            }
        }
        

        /// <devdoc>
        /// <para>Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how the control footer is rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateField_FooterTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(IDataItemContainer))
        ]
        public virtual ITemplate FooterTemplate {
            get {
                return footerTemplate;
            }
            set {
                footerTemplate = value;
                OnFieldChanged();
            }
        }
        

        /// <devdoc>
        /// <para>Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/>
        /// that defines how the control header is rendered.</para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateField_HeaderTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(IDataItemContainer))
        ]
        public virtual ITemplate HeaderTemplate {
            get {
                return headerTemplate;
            }
            set {
                headerTemplate = value;
                OnFieldChanged();
            }
        }


        /// <devdoc>
        /// <para>Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how rows in insert mode are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateField_InsertItemTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(IDataItemContainer), BindingDirection.TwoWay)
        ]
        public virtual ITemplate InsertItemTemplate {
            get {
                return insertItemTemplate;
            }
            set {
                insertItemTemplate = value;
                OnFieldChanged();
            }
        }


        /// <devdoc>
        /// <para> Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how items are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateField_ItemTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(IDataItemContainer), BindingDirection.TwoWay)
        ]
        public virtual ITemplate ItemTemplate {
            get {
                return itemTemplate;
            }
            set {
                itemTemplate = value;
                OnFieldChanged();
            }
        }

        protected override void CopyProperties(DataControlField newField) {
            ((TemplateField)newField).ConvertEmptyStringToNull = ConvertEmptyStringToNull;
            ((TemplateField)newField).AlternatingItemTemplate = AlternatingItemTemplate;
            ((TemplateField)newField).ItemTemplate = ItemTemplate;
            ((TemplateField)newField).FooterTemplate = FooterTemplate;
            ((TemplateField)newField).EditItemTemplate = EditItemTemplate;
            ((TemplateField)newField).HeaderTemplate = HeaderTemplate;
            ((TemplateField)newField).InsertItemTemplate = InsertItemTemplate;
            base.CopyProperties(newField);
        }

        protected override DataControlField CreateField() {
            return new TemplateField();
        }
        

        /// <devdoc>
        /// Extracts the value(s) from the given cell and puts the value(s) into a dictionary.  Indicate includeReadOnly
        /// to have readonly fields' values inserted into the dictionary.
        /// </devdoc>
        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly) {
            DataBoundControlHelper.ExtractValuesFromBindableControls(dictionary, cell);

            IBindableTemplate bindableTemplate = ItemTemplate as IBindableTemplate;

            if (((rowState & DataControlRowState.Alternate) != 0) && (AlternatingItemTemplate != null)) {
                bindableTemplate = AlternatingItemTemplate as IBindableTemplate;
            }
            if (((rowState & DataControlRowState.Edit) != 0) && EditItemTemplate != null) {
                bindableTemplate = EditItemTemplate as IBindableTemplate;
            }
            else if ((rowState & DataControlRowState.Insert) != 0 && InsertVisible) {
                if (InsertItemTemplate != null) {
                    bindableTemplate = InsertItemTemplate as IBindableTemplate;
                }
                else {
                    if (EditItemTemplate != null) {
                        bindableTemplate = EditItemTemplate as IBindableTemplate;
                    }
                }
            }
            
            if (bindableTemplate != null) {
                bool convertEmptyStringToNull = ConvertEmptyStringToNull;
                foreach (DictionaryEntry entry in bindableTemplate.ExtractValues(cell.BindingContainer)) {
                    object value = entry.Value;
                    if (convertEmptyStringToNull && value is string && ((string)value).Length == 0) {
                        dictionary[entry.Key] = null;
                    }
                    else {
                        dictionary[entry.Key] = value;
                    }
                }
            }
            return;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex) {
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            ITemplate contentTemplate = null;
            switch (cellType) {
                case DataControlCellType.Header:
                    contentTemplate = headerTemplate;
                    break;
                    
                case DataControlCellType.Footer:
                    contentTemplate = footerTemplate;
                    break;
                    
                case DataControlCellType.DataCell:
                    contentTemplate = itemTemplate;
                    if ((rowState & DataControlRowState.Edit) != 0) {
                        if (editItemTemplate != null) {
                            contentTemplate = editItemTemplate;
                        }
                    }
                    else if ((rowState & DataControlRowState.Insert) != 0) {
                        if (insertItemTemplate != null) {
                            contentTemplate = insertItemTemplate;
                        }
                        else {
                            if (editItemTemplate != null) {
                                contentTemplate = editItemTemplate;
                            }
                        }
                    }
                    else if ((rowState & DataControlRowState.Alternate) != 0) {
                        if (alternatingItemTemplate != null) {
                            contentTemplate = alternatingItemTemplate;
                        }
                    }
                    break;
            }
            
            if (contentTemplate != null) {
                // The base class might have added a control or some text for some cases
                // such as header text which need to be removed before
                // the corresponding template is used.
                // Note that setting text also has the effect of clearing out any controls.
                cell.Text = String.Empty;
                
                contentTemplate.InstantiateIn(cell);
            }
            else {
                if (cellType == DataControlCellType.DataCell) {
                    cell.Text = "&nbsp;";
                }
            }
        }

        /// <devdoc>
        /// <para>Override with an empty body if the field's controls all support callback.
        ///  Otherwise, override and throw a useful error message about why the field can't support callbacks.</para>
        /// </devdoc>
        public override void ValidateSupportsCallback() {
            throw new NotSupportedException(SR.GetString(SR.TemplateField_CallbacksNotSupported, Control.ID));
        }
    }
}

