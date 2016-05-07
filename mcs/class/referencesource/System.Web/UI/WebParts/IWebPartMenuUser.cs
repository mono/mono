//------------------------------------------------------------------------------
// <copyright file="IWebPartMenuUser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <devdoc>
    /// </devdoc>
    internal interface IWebPartMenuUser {

        Style CheckImageStyle { get; }

        string CheckImageUrl { get; }

        string ClientID { get; }

        Style ItemHoverStyle { get; }

        Style ItemStyle { get; }

        Style LabelHoverStyle { get; }

        string LabelImageUrl { get; }

        Style LabelStyle { get; }

        string LabelText { get; }

        WebPartMenuStyle MenuPopupStyle { get; }

        Page Page { get; }

        string PopupImageUrl { get; }

        string PostBackTarget { get; }

        IUrlResolutionService UrlResolver { get; }

        void OnBeginRender(HtmlTextWriter writer);

        void OnEndRender(HtmlTextWriter writer);
    }
}
