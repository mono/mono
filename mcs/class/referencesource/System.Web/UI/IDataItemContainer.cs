//------------------------------------------------------------------------------
// <copyright file="IDataItemContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// </devdoc>
    public interface IDataItemContainer : INamingContainer {


        object DataItem {
            get;
        }

        int DataItemIndex {
            get;
        }

        int DisplayIndex {
            get;
        }
    }
}

