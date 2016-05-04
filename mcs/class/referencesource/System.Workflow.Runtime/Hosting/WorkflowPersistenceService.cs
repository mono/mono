//------------------------------------------------------------------------------
// <copyright file="StatePersistenceService.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Diagnostics;

namespace System.Workflow.Runtime.Hosting
{
    /// <summary> Service for saving engine state. </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class WorkflowPersistenceService : WorkflowRuntimeService
    {
        /// <summary> Saves the state of a workflow instance. </summary>
        /// <param name="state"> The workflow instance state to save </param>
        internal protected abstract void SaveWorkflowInstanceState(Activity rootActivity, bool unlock);

        /// <summary></summary>
        /// <param name="state"></param>
        internal protected abstract void UnlockWorkflowInstanceState(Activity rootActivity);

        /// <summary> Loads the state of a workflow instance. </summary>
        /// <param name="instanceId"> The unique ID of the instance to load </param>
        /// <returns> The workflow instance state</returns>
        internal protected abstract Activity LoadWorkflowInstanceState(Guid instanceId);

        /// <summary> Saves the state of a completed scope. </summary>
        /// <param name="completedScopeState"> The completed scope to save </param>
        internal protected abstract void SaveCompletedContextActivity(Activity activity);

        /// <summary> Loads the state of a completed scope </summary>
        /// <param name="scopeId"> The unique identifier of the completed scope </param>
        /// <returns> The completed scope or null </returns>
        internal protected abstract Activity LoadCompletedContextActivity(Guid scopeId, Activity outerActivity);

        /// <summary></summary>
        /// <param name="activity"></param>
        /// <returns>The value of the "UnloadOnIdle" flag</returns>
        internal protected abstract bool UnloadOnIdle(Activity activity);

        static protected byte[] GetDefaultSerializedForm(Activity activity)
        {
            DateTime startTime = DateTime.Now;
            Byte[] result;

            Debug.Assert(activity != null, "Null activity");
            using (MemoryStream stream = new MemoryStream(10240))
            {
                stream.Position = 0;
                activity.Save(stream);
                using (MemoryStream compressedStream = new MemoryStream((int)stream.Length))
                {
                    using (GZipStream gzs = new GZipStream(compressedStream, CompressionMode.Compress, true))
                    {
                        gzs.Write(stream.GetBuffer(), 0, (int)stream.Length);
                    }

                    ActivityExecutionContextInfo executionContextInfo = (ActivityExecutionContextInfo)activity.GetValue(Activity.ActivityExecutionContextInfoProperty);
                    TimeSpan timeElapsed = DateTime.Now - startTime;
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0,
                        "Serialized a {0} with id {1} to length {2}. Took {3}.",
                        executionContextInfo, executionContextInfo.ContextGuid, compressedStream.Length, timeElapsed);

                    result = compressedStream.GetBuffer();
                    Array.Resize<Byte>(ref result, Convert.ToInt32(compressedStream.Length));
                }
            }
            return result;
        }

        static protected Activity RestoreFromDefaultSerializedForm(Byte[] activityBytes, Activity outerActivity)
        {
            DateTime startTime = DateTime.Now;
            Activity state;

            MemoryStream stream = new MemoryStream(activityBytes);
            stream.Position = 0;

            using (GZipStream gzs = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                state = Activity.Load(gzs, outerActivity);
            }
            Debug.Assert(state != null, "invalid state recovered");
            TimeSpan timeElapsed = DateTime.Now - startTime;
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0,
                "Deserialized a {0} to length {1}. Took {2}.",
                state, stream.Length, timeElapsed);

            return state;
        }
        static protected internal bool GetIsBlocked(Activity rootActivity)
        {
            return (bool)rootActivity.GetValue(WorkflowExecutor.IsBlockedProperty);
        }
        static protected internal string GetSuspendOrTerminateInfo(Activity rootActivity)
        {
            return (string)rootActivity.GetValue(WorkflowExecutor.SuspendOrTerminateInfoProperty);
        }
        static protected internal WorkflowStatus GetWorkflowStatus(Activity rootActivity)
        {
            return (WorkflowStatus)rootActivity.GetValue(WorkflowExecutor.WorkflowStatusProperty);
        }
    }
}
