//------------------------------------------------------------------------------
// <copyright file="IRepeatInfoUser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.Web.UI;

    /// <devdoc>
    /// <para>Specifies a contract for implementing <see cref='System.Web.UI.WebControls.Repeater'/> objects in list controls.</para>
    /// </devdoc>
    public interface IRepeatInfoUser {


        /// <devdoc>
        /// <para>Indicates whether the <see cref='System.Web.UI.WebControls.Repeater'/> contains a
        ///    header item.</para>
        /// </devdoc>
        bool HasHeader {
            get;
        }


        /// <devdoc>
        ///    <para>Indicates whether the Repeater contains
        ///       a footer item.</para>
        /// </devdoc>
        bool HasFooter {
            get;
        }
        

        /// <devdoc>
        ///    <para>Indicates whether the Repeater contains
        ///       separator items.</para>
        /// </devdoc>
        bool HasSeparators {
            get;
        }


        /// <devdoc>
        ///    Specifies the item count of the Repeater.
        /// </devdoc>
        int RepeatedItemCount {
            get;
        }


        /// <devdoc>
        ///    <para>Retrieves the item style with the specified item type 
        ///       and location within the <see cref='System.Web.UI.WebControls.Repeater'/> .</para>
        /// </devdoc>
        Style GetItemStyle(ListItemType itemType, int repeatIndex);


        /// <devdoc>
        /// <para>Renders the <see cref='System.Web.UI.WebControls.Repeater'/> item with the specified information.</para>
        /// </devdoc>
        void RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer);
    }
}

