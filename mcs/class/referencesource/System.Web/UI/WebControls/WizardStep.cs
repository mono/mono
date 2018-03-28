//------------------------------------------------------------------------------
// <copyright file="WizardStep.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [
    Bindable(false),
    ControlBuilderAttribute(typeof(WizardStepControlBuilder)),
    ToolboxItem(false)
    ]

    public sealed class WizardStep : WizardStepBase {
    }
}
