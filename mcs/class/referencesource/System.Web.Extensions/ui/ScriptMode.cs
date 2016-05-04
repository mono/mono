//------------------------------------------------------------------------------
// <copyright file="ScriptMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System.Security.Permissions;
    using System.Web;

    public enum ScriptMode {
        Auto = 0,
        Inherit = 1,
        Debug = 2,
        Release = 3,
    }
}
