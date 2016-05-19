//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Windows.Controls;

    internal class NoContextMenuGrid : Grid
    {
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}
