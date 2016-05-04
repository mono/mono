//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;

    /// <summary>
    /// Arguments for view changed event
    /// </summary>
    public class ViewCreatedEventArgs : EventArgs
    {
        private WorkflowViewElement view;

        /// <summary>
        /// Contruct a ViewChangedEventArgs object
        /// </summary>
        /// <param name="view">the workflow view element that is created</param>
        public ViewCreatedEventArgs(WorkflowViewElement view)
        {
            if (view == null)
            {
                throw FxTrace.Exception.ArgumentNull("view");
            }

            this.view = view;
        }

        /// <summary>
        /// Gets the workflow view element that is created
        /// </summary>
        public WorkflowViewElement View
        {
            get { return this.view; }
        }
    }
}
