//------------------------------------------------------------------------------
// <copyright file="IWizardSideBarListControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Collections;

    internal interface IWizardSideBarListControl {
        object DataSource { get; set; }

        IEnumerable Items { get; }

        ITemplate ItemTemplate { get; set; }

        int SelectedIndex { get; set; }

        event CommandEventHandler ItemCommand;

        event EventHandler<WizardSideBarListControlItemEventArgs> ItemDataBound;

        void DataBind();
    }
}
