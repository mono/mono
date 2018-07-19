//------------------------------------------------------------------------------
// <copyright file="StateMachineExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Text;

    /// <summary>
    /// StateMachineExtension is used to resume a bookmark outside StateMachine.
    /// </summary>
    class StateMachineExtension : IWorkflowInstanceExtension
    {
        private WorkflowInstanceProxy instance;

        /// <summary>
        /// Used to get additional extensions.
        /// </summary>
        /// <returns>Returns a IEnumerable of extensions</returns>
        public IEnumerable<object> GetAdditionalExtensions()
        {
            return null;
        }

        /// <summary>
        /// called with the targe instance under WorkflowInstance.Initialize
        /// </summary>
        /// <param name="instance">The value of WorkflowInstanceProxy</param>
        public void SetInstance(WorkflowInstanceProxy instance)
        {
            this.instance = instance;
        }

        /// <summary>
        /// Used to resume bookmark outside workflow.
        /// </summary>
        /// <param name="bookmark">The value of Bookmark to be resumed</param>
        public void ResumeBookmark(Bookmark bookmark)
        {
            // This method is necessary due to CSDMain 223257.
            IAsyncResult asyncResult = this.instance.BeginResumeBookmark(bookmark, null, Fx.ThunkCallback(new AsyncCallback(StateMachineExtension.OnResumeBookmarkCompleted)), this.instance);
            if (asyncResult.CompletedSynchronously)
            {
                this.instance.EndResumeBookmark(asyncResult);
            }
        }

        static void OnResumeBookmarkCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                WorkflowInstanceProxy instance = result.AsyncState as WorkflowInstanceProxy;
                Fx.Assert(instance != null, "BeginResumeBookmark should pass a WorkflowInstanceProxy object as the async state object.");
                instance.EndResumeBookmark(result);
            }
        }
    }
}
