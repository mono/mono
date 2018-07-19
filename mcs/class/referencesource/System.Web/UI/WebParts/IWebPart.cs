//------------------------------------------------------------------------------
// <copyright file="IWebPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    public interface IWebPart {

        string CatalogIconImageUrl { get; set; }

        string Description { get; set; }

        string Subtitle { get; }

        string Title { get; set; }

        string TitleIconImageUrl { get; set; }

        string TitleUrl { get; set; }
    }
}
