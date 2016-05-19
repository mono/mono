//------------------------------------------------------------------------------
// <copyright file="TemplateColumn.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>Defines the template for controls layout within a 
    ///    <see cref='System.Web.UI.WebControls.DataGrid'/> 
    ///    column.</para>
    /// </devdoc>
    // 


    public class TemplateColumn : DataGridColumn {

        private ITemplate headerTemplate;
        private ITemplate footerTemplate;
        private ITemplate itemTemplate;
        private ITemplate editItemTemplate;

        

        /// <devdoc>
        ///    Initializes a new instance of the <see cref='System.Web.UI.WebControls.TemplateColumn'/> class.
        /// </devdoc>
        public TemplateColumn() {
        }

        

        /// <devdoc>
        /// <para>Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how items in edit mode are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateColumn_EditItemTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(DataGridItem))
        ]
        public virtual ITemplate EditItemTemplate {
            get {
                return editItemTemplate;
            }
            set {
                editItemTemplate = value;
                OnColumnChanged();
            }
        }
        

        /// <devdoc>
        /// <para>Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how the control footer is rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateColumn_FooterTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(DataGridItem))
        ]
        public virtual ITemplate FooterTemplate {
            get {
                return footerTemplate;
            }
            set {
                footerTemplate = value;
                OnColumnChanged();
            }
        }
        

        /// <devdoc>
        /// <para>Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/>
        /// that defines how the control header is rendered.</para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateColumn_HeaderTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(DataGridItem))
        ]
        public virtual ITemplate HeaderTemplate {
            get {
                return headerTemplate;
            }
            set {
                headerTemplate = value;
                OnColumnChanged();
            }
        }


        /// <devdoc>
        /// <para> Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how items are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            WebSysDescription(SR.TemplateColumn_ItemTemplate),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(DataGridItem))
        ]
        public virtual ITemplate ItemTemplate {
            get {
                return itemTemplate;
            }
            set {
                itemTemplate = value;
                OnColumnChanged();
            }
        }
        

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType) {
            base.InitializeCell(cell, columnIndex, itemType);

            ITemplate contentTemplate = null;
            switch (itemType) {
                case ListItemType.Header:
                    contentTemplate = headerTemplate;
                    break;
                    
                case ListItemType.Footer:
                    contentTemplate = footerTemplate;
                    break;
                    
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                    contentTemplate = itemTemplate;
                    break;
                    
                case ListItemType.EditItem:
                    if (editItemTemplate != null)
                        contentTemplate = editItemTemplate;
                    else
                        goto case ListItemType.Item;
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
                if (itemType == ListItemType.Item || 
                    itemType == ListItemType.AlternatingItem || 
                    itemType == ListItemType.SelectedItem || 
                    itemType == ListItemType.EditItem) {
                    cell.Text = "&nbsp;";
                }
            }
        }
    }
}

