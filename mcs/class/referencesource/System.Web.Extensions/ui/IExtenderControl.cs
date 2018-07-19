//------------------------------------------------------------------------------
// <copyright file="IExtenderControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.UI;

    public interface IExtenderControl {
        IEnumerable<ScriptDescriptor> GetScriptDescriptors(Control targetControl);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely return a new collection, which is too slow for a property")]
        IEnumerable<ScriptReference> GetScriptReferences();
    }
}
