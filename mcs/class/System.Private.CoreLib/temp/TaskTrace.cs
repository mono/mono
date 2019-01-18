// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Internal.Threading.Tasks.Tracing
{
    /// <summary>
    /// Helper class for reporting <see cref="System.Threading.Tasks.Task"/>-related events.
    /// Calls are forwarded to an instance of <see cref="TaskTraceCallbacks"/>, if one has been
    /// provided.
    /// </summary>
    public static class TaskTrace
    {
        public static bool Enabled
        {
            get
            {
                    return false;
            }
        }

//        public static void Initialize(TaskTraceCallbacks callbacks)
//        {
//        }

        public static void TaskWaitBegin_Asynchronous(
            int OriginatingTaskSchedulerID,
            int OriginatingTaskID,
            int TaskID)
        {
        }

        public static void TaskWaitBegin_Synchronous(
            int OriginatingTaskSchedulerID,
            int OriginatingTaskID,
            int TaskID)
        {
        }

        public static void TaskWaitEnd(
            int OriginatingTaskSchedulerID,
            int OriginatingTaskID,
            int TaskID)
        {
        }

        public static void TaskScheduled(
            int OriginatingTaskSchedulerID,
            int OriginatingTaskID,
            int TaskID,
            int CreatingTaskID,
            int TaskCreationOptions)
        {
        }

        public static void TaskStarted(
            int OriginatingTaskSchedulerID,
            int OriginatingTaskID,
            int TaskID)
        {
        }

        public static void TaskCompleted(
            int OriginatingTaskSchedulerID,
            int OriginatingTaskID,
            int TaskID,
            bool IsExceptional)
        {
        }
    }
}