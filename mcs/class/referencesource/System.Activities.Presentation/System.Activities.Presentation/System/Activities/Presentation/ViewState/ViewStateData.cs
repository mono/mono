//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.ViewState
{
    /// <summary>
    /// This class acts as a surrogate for holding view state properties (VirtualizedContainerService.HintSize and WorkflowViewStateService.ViewState)
    /// as attached properties when view state separation is done.
    /// </summary>
    public sealed class ViewStateData
    {
        /// <summary>
        /// Creates a new instance of ViewStateData
        /// </summary>
        public ViewStateData() 
        { 
        }

        /// <summary>
        /// Gets or sets an identifier that associates an object of this class
        /// with an activity that has matching WorkflowViewState.RefId value.
        /// </summary>
        public string Id { get; set; }
    }
}
