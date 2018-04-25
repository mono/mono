//------------------------------------------------------------------------------
// <copyright file="DataListItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    /// <para>Represents an item in the <see cref='System.Web.UI.WebControls.DataList'/>. </para>
    /// </devdoc>
    [
    ToolboxItem(false)
    ]
    public class DataListItem : WebControl, IDataItemContainer {

        private int itemIndex;
        private ListItemType itemType;
        private object dataItem;

        
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataListItem'/> class.</para>
        /// </devdoc>
        public DataListItem(int itemIndex, ListItemType itemType) {
            this.itemIndex = itemIndex;
            this.itemType = itemType;
        }



        /// <devdoc>
        /// <para>Represents an item in the <see cref='System.Web.UI.WebControls.DataList'/>. </para>
        /// </devdoc>
        public virtual object DataItem {
            get {
                return dataItem;
            }
            set {
                dataItem = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates the index of the item in the <see cref='System.Web.UI.WebControls.DataList'/>. This property is 
        ///    read-only.</para>
        /// </devdoc>
        public virtual int ItemIndex {
            get {
                return itemIndex;
            }
        }


        /// <devdoc>
        /// <para>Indicates the type of the item in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        public virtual ListItemType ItemType {
            get {
                return itemType;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            return new TableItemStyle();
        }
        

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            if (e is CommandEventArgs) {
                DataListCommandEventArgs args = new DataListCommandEventArgs(this, source, (CommandEventArgs)e);

                RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }


        /// <devdoc>
        /// <para>Displays a <see cref='System.Web.UI.WebControls.DataListItem'/> on the client.</para>
        /// </devdoc>
        public virtual void RenderItem(HtmlTextWriter writer, bool extractRows, bool tableLayout) {
            HttpContext con = Context;
            if ((con != null) && con.TraceIsEnabled) {
                int presize = con.Response.GetBufferedLength();
            
                RenderItemInternal(writer, extractRows, tableLayout);
                
                int postsize = con.Response.GetBufferedLength();
                con.Trace.AddControlSize(UniqueID, postsize - presize);
            }
            else
                RenderItemInternal(writer, extractRows, tableLayout);
        }

        private void RenderItemInternal(HtmlTextWriter writer, bool extractRows, bool tableLayout) {
            if (extractRows == false) {
                if (tableLayout) {
                    // in table mode, style information has gone on the containing TD
                    RenderContents(writer);
                }
                else {
                    // in non-table mode, the item itself is responsible for putting
                    // out the style information
                    RenderControl(writer);
                }
            }
            else {
                IEnumerator controlEnum = this.Controls.GetEnumerator();
                Table templateTable = null;
                bool hasControls = false;

                while (controlEnum.MoveNext()) {
                    hasControls = true;
                    Control c = (Control)controlEnum.Current;
                    if (c is Table) {
                        templateTable = (Table)c;
                        break;
                    }
                }

                if (templateTable != null) {
                    IEnumerator rowEnum = templateTable.Rows.GetEnumerator();
                    while (rowEnum.MoveNext()) {
                        TableRow r = (TableRow)rowEnum.Current;
                        r.RenderControl(writer);
                    }
                }
                else if (hasControls) {
                    // there was a template, since there were controls but
                    // none of them was a table... so throw an exception here
                    throw new HttpException(SR.GetString(SR.DataList_TemplateTableNotFound,
                                                                         Parent.ID, itemType.ToString()));
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal virtual void SetItemType(ListItemType itemType) {
            this.itemType = itemType;
        }



        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        object IDataItemContainer.DataItem {
            get {
                return DataItem;
            }
        }
        
        int IDataItemContainer.DataItemIndex {
            get {
                return ItemIndex;
            }
        }

        int IDataItemContainer.DisplayIndex {
            get {
                return ItemIndex;
            }
        }
    }
}

