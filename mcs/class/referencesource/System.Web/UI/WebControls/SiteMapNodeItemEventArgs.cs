//------------------------------------------------------------------------------
// <copyright file="SiteMapNodeItemEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;    
    using System.ComponentModel;  
    using System.Web;
    using System.Web.UI;

    public class SiteMapNodeItemEventArgs : EventArgs {

        private SiteMapNodeItem _item;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.SiteMapNodeItemEventArgs'/> class.</para>
        /// </devdoc>
        public SiteMapNodeItemEventArgs(SiteMapNodeItem item) {
            this._item = item;
        }


        /// <devdoc>
        /// <para> Gets the <see cref='System.Web.UI.WebControls.SiteMapNodeItem'/> associated with the event.</para>
        /// </devdoc>
        public SiteMapNodeItem Item {
            get {
                return _item;
            }
        }
    }
}
