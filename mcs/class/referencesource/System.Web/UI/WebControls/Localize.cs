//------------------------------------------------------------------------------
// <copyright file="Localize.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Web.UI;

    // Identical to the 'literal' control, but used for localization

    [
    Designer("System.Web.UI.Design.WebControls.LocalizeDesigner, " + AssemblyRef.SystemDesign),
    ToolboxBitmap(typeof(Localize)),
    ]
    public class Localize : Literal {
    }
}
