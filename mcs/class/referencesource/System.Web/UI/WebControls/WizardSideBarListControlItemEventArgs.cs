//------------------------------------------------------------------------------
// <copyright file="WizardSideBarListControlItemEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    internal sealed class WizardSideBarListControlItemEventArgs : EventArgs {

        public WizardSideBarListControlItem Item {
            get;
            private set;
        }

        public WizardSideBarListControlItemEventArgs(WizardSideBarListControlItem item) {
            Item = item;
        }
    }
}
