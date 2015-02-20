//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.ViewState
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Markup;

    /// <summary>
    /// This class is used to hold ViewStateData for all activities in the workflow as an attached
    /// property on the root of the xaml document.
    /// </summary>
    [ContentProperty("ViewStateData")]
    public sealed class ViewStateManager
    {
        Collection<ViewStateData> viewStateData;

        /// <summary>
        /// Creates a new instance of ViewStateManager
        /// </summary>
        public ViewStateManager()
        {
        }

        /// <summary>
        /// Gets a collection of ViewStateData for all activities in the workflow
        /// </summary>
        public Collection<ViewStateData> ViewStateData
        {
            get
            {
                if (this.viewStateData == null)
                {
                    this.viewStateData = new Collection<ViewStateData>();
                }
                return this.viewStateData;
            }
        }
    }
}
