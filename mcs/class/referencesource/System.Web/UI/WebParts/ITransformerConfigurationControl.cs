//------------------------------------------------------------------------------
// <copyright file="ITransformerConfigurationControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    public interface ITransformerConfigurationControl {

        event EventHandler Cancelled;

        event EventHandler Succeeded;
    }
}
