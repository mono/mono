//------------------------------------------------------------------------------
// <copyright file="ScriptDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    public abstract class ScriptDescriptor {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Implementation will likely be too slow for a property")]
        protected internal abstract string GetScript();

        internal virtual void RegisterDisposeForDescriptor(ScriptManager scriptManager, Control owner) {
        }
    }
}
