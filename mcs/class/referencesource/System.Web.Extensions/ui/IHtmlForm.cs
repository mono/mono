//------------------------------------------------------------------------------
// <copyright file="IHtmlForm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
using System;
using System.Web.UI;

namespace System.Web.UI {
    internal interface IHtmlForm {
        string ClientID {
            get;
        }

        string Method {
            get;
        }

        void RenderControl(HtmlTextWriter writer);

        void SetRenderMethodDelegate(RenderMethod renderMethod);
    }
}
