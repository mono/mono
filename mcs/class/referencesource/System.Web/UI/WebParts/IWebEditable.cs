//------------------------------------------------------------------------------
// <copyright file="IWebEditable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    public interface IWebEditable {

        object WebBrowsableObject { get; }

        EditorPartCollection CreateEditorParts();
    }
}
