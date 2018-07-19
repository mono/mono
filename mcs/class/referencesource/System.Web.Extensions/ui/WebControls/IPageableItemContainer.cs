//------------------------------------------------------------------------------
// <copyright file="IPageableItemContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI.WebControls {

    public interface IPageableItemContainer {
        int StartRowIndex { get; }
        int MaximumRows { get; }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "databind",
            Justification = "Cannot change to 'dataBind' as would break binary compatibility with legacy code.")]
        void SetPageProperties(int startRowIndex, int maximumRows, bool databind);

        event EventHandler<PageEventArgs> TotalRowCountAvailable;
    }
}
