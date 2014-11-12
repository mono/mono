// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// TplEtwProvider.cs
//
// <OWNER>[....]</OWNER>
//
// EventSource for TPL.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Security.Permissions;

namespace System.Threading.Tasks
{
#if !FEATURE_PAL    // PAL doesn't support  eventing
    using System.Diagnostics.Tracing;

    /// <summary>Provides an event source for tracing TPL information.</summary>
    [EventSource(
        Name = "System.Threading.Tasks.TplEventSource",
        Guid = "2e5dba47-a3d2-4d16-8ee0-6671ffdcd7b5", 
        LocalizationResources = "mscorlib")]
    internal sealed class TplEtwProvider : EventSource
    {
        /// <summary>
        /// Defines the singleton instance for the TPL ETW provider.
        /// The TPL Event provider GUID is {2e5dba47-a3d2-4d16-8ee0-6671ffdcd7b5}.
        /// </summary>
        public static TplEtwProvider Log = new TplEtwProvider();
        /// <summary>Prevent external instantiation.  All logging should go through the Log instance.</summary>
        private TplEtwProvider() { }

        /// <summary>Type of a fork/join operation.</summary>
        public enum ForkJoinOperationType
        {
            /// <summary>Parallel.Invoke.</summary>
            ParallelInvoke=1,
            /// <summary>Parallel.For.</summary>
            ParallelFor=2,
            /// <summary>Parallel.ForEach.</summary>
            ParallelForEach=3
        }

        /// <summary>Configured behavior of a task wait operation.</summary>
        public enum TaskWaitBehavior : int
        {
            /// <summary>A synchronous wait.</summary>
            Synchronous = 1,
            /// <summary>An asynchronous await.</summary>
            Asynchronous = 2
        }

        /// <summary>ETW tasks that have start/stop events.</summary>
        public class Tasks // this name is important for EventSource
        {
            /// <summary>A parallel loop.</summary>
            public const EventTask Loop = (EventTask)1;
            /// <summary>A parallel invoke.</summary>
            public const EventTask Invoke = (EventTask)2;
            /// <summary>Executing a Task.</summary>
            public const EventTask TaskExecute = (EventTask)3;
            /// <summary>Waiting on a Task.</summary>
            public const EventTask TaskWait = (EventTask)4;
            /// <summary>A fork/join task within a loop or invoke.</summary>
            public const EventTask ForkJoin = (EventTask)5;
            /// <summary>A task is scheduled to execute.</summary>
            public const EventTask TaskScheduled = (EventTask)6;
            /// <summary>An await task continuation is scheduled to execute.</summary>
            public const EventTask AwaitTaskContinuationScheduled = (EventTask)7;
        }

        public class Keywords // thisname is important for EventSource
        {
            /// <summary>
            /// Only the most basic information about the workings of the task library
            /// This sets activity IDS and logs when tasks are schedules (or waits begin)
            /// But are otherwise silent
            /// </summary>
            public const EventKeywords TaskTransfer = (EventKeywords) 1;
            /// <summary>
            /// TaskTranser events plus events when tasks start and stop 
            /// </summary>
            public const EventKeywords Tasks = (EventKeywords) 2;
            /// <summary>
            /// Events associted with the higher level parallel APIs
            /// </summary>
            public const EventKeywords Parallel = (EventKeywords) 4;
        }

        /// <summary>Enabled for all keywords.</summary>
        private const EventKeywords ALL_KEYWORDS = (EventKeywords)(-1);

        //-----------------------------------------------------------------------------------
        //        
        // TPL Event IDs (must be unique)
        //

        /// <summary>The beginning of a parallel loop.</summary>
        private const int PARALLELLOOPBEGIN_ID = 1;
        /// <summary>The ending of a parallel loop.</summary>
        private const int PARALLELLOOPEND_ID = 2;
        /// <summary>The beginning of a parallel invoke.</summary>
        private const int PARALLELINVOKEBEGIN_ID = 3;
        /// <summary>The ending of a parallel invoke.</summary>
        private const int PARALLELINVOKEEND_ID = 4;
        /// <summary>A task entering a fork/join construct.</summary>
        private const int PARALLELFORK_ID = 5;
        /// <summary>A task leaving a fork/join construct.</summary>
        private const int PARALLELJOIN_ID = 6;

        /// <summary>A task is scheduled to a task scheduler.</summary>
        private const int TASKSCHEDULED_ID = 7;
        /// <summary>A task is about to execute.</summary>
        private const int TASKSTARTED_ID = 8;
        /// <summary>A task has finished executing.</summary>
        private const int TASKCOMPLETED_ID = 9;
        /// <summary>A wait on a task is beginning.</summary>
        private const int TASKWAITBEGIN_ID = 10;
        /// <summary>A wait on a task is ending.</summary>
        private const int TASKWAITEND_ID = 11;
        /// <summary>A continuation of a task is scheduled.</summary>
        private const int AWAITTASKCONTINUATIONSCHEDULED_ID = 12;
        //-----------------------------------------------------------------------------------
        //        
        // Parallel Events
        //

        #region ParallelLoopBegin
        /// <summary>
        /// Denotes the entry point for a Parallel.For or Parallel.ForEach loop
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="ForkJoinContextID">The loop ID.</param>
        /// <param name="OperationType">The kind of fork/join operation.</param>
        /// <param name="InclusiveFrom">The lower bound of the loop.</param>
        /// <param name="ExclusiveTo">The upper bound of the loop.</param>
        [SecuritySafeCritical]
        [Event(PARALLELLOOPBEGIN_ID, Level = EventLevel.Informational, Task = TplEtwProvider.Tasks.Loop, Opcode = EventOpcode.Start)]        
        public void ParallelLoopBegin(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,      // PFX_COMMON_EVENT_HEADER
            int ForkJoinContextID, ForkJoinOperationType OperationType, // PFX_FORKJOIN_COMMON_EVENT_HEADER
            long InclusiveFrom, long ExclusiveTo)
        {
            if (IsEnabled() && IsEnabled(EventLevel.Informational, Keywords.Parallel))
            {
                // There is no explicit WriteEvent() overload matching this event's fields. Therefore calling
                // WriteEvent() would hit the "params" overload, which leads to an object allocation every time 
                // this event is fired. To prevent that problem we will call WriteEventCore(), which works with 
                // a stack based EventData array populated with the event fields.
                unsafe
                {
                    EventData* eventPayload = stackalloc EventData[6];

                    eventPayload[0].Size = sizeof(int);
                    eventPayload[0].DataPointer = ((IntPtr) (&OriginatingTaskSchedulerID));
                    eventPayload[1].Size = sizeof(int);
                    eventPayload[1].DataPointer = ((IntPtr) (&OriginatingTaskID));
                    eventPayload[2].Size = sizeof(int);
                    eventPayload[2].DataPointer = ((IntPtr) (&ForkJoinContextID));
                    eventPayload[3].Size = sizeof(int);
                    eventPayload[3].DataPointer = ((IntPtr) (&OperationType));
                    eventPayload[4].Size = sizeof(long);
                    eventPayload[4].DataPointer = ((IntPtr) (&InclusiveFrom));
                    eventPayload[5].Size = sizeof(long);
                    eventPayload[5].DataPointer = ((IntPtr) (&ExclusiveTo));

                    WriteEventCore(PARALLELLOOPBEGIN_ID, 6, eventPayload);
                }
            }
        }
        #endregion

        #region ParallelLoopEnd
        /// <summary>
        /// Denotes the end of a Parallel.For or Parallel.ForEach loop.
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="ForkJoinContextID">The loop ID.</param>
        /// <param name="TotalIterations">the total number of iterations processed.</param>
        [SecuritySafeCritical]
        [Event(PARALLELLOOPEND_ID, Level = EventLevel.Informational, Task = TplEtwProvider.Tasks.Loop, Opcode = EventOpcode.Stop)]
        public void ParallelLoopEnd(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int ForkJoinContextID, long TotalIterations)
        {
            if (IsEnabled() && IsEnabled(EventLevel.Informational, Keywords.Parallel))
            {
                // There is no explicit WriteEvent() overload matching this event's fields.
                // Therefore calling WriteEvent() would hit the "params" overload, which leads to an object allocation every time this event is fired.
                // To prevent that problem we will call WriteEventCore(), which works with a stack based EventData array populated with the event fields
                unsafe
                {
                    EventData* eventPayload = stackalloc EventData[4];

                    eventPayload[0].Size = sizeof(int);
                    eventPayload[0].DataPointer = ((IntPtr) (&OriginatingTaskSchedulerID));
                    eventPayload[1].Size = sizeof(int);
                    eventPayload[1].DataPointer = ((IntPtr) (&OriginatingTaskID));
                    eventPayload[2].Size = sizeof(int);
                    eventPayload[2].DataPointer = ((IntPtr) (&ForkJoinContextID));
                    eventPayload[3].Size = sizeof(long);
                    eventPayload[3].DataPointer = ((IntPtr) (&TotalIterations));

                    WriteEventCore(PARALLELLOOPEND_ID, 4, eventPayload);
                }
            }
        }
        #endregion

        #region ParallelInvokeBegin
        /// <summary>Denotes the entry point for a Parallel.Invoke call.</summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="ForkJoinContextID">The invoke ID.</param>
        /// <param name="OperationType">The kind of fork/join operation.</param>
        /// <param name="ActionCount">The number of actions being invoked.</param>
        [SecuritySafeCritical]
        [Event(PARALLELINVOKEBEGIN_ID, Level = EventLevel.Informational, Task = TplEtwProvider.Tasks.Invoke, Opcode = EventOpcode.Start)]
        public void ParallelInvokeBegin(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,      // PFX_COMMON_EVENT_HEADER
            int ForkJoinContextID, ForkJoinOperationType OperationType, // PFX_FORKJOIN_COMMON_EVENT_HEADER
            int ActionCount)
        {
            if (IsEnabled() && IsEnabled(EventLevel.Informational, Keywords.Parallel))
            {
                // There is no explicit WriteEvent() overload matching this event's fields.
                // Therefore calling WriteEvent() would hit the "params" overload, which leads to an object allocation every time this event is fired.
                // To prevent that problem we will call WriteEventCore(), which works with a stack based EventData array populated with the event fields
                unsafe
                {
                    EventData* eventPayload = stackalloc EventData[5];

                    eventPayload[0].Size = sizeof(int);
                    eventPayload[0].DataPointer = ((IntPtr) (&OriginatingTaskSchedulerID));
                    eventPayload[1].Size = sizeof(int);
                    eventPayload[1].DataPointer = ((IntPtr) (&OriginatingTaskID));
                    eventPayload[2].Size = sizeof(int);
                    eventPayload[2].DataPointer = ((IntPtr) (&ForkJoinContextID));
                    eventPayload[3].Size = sizeof(int);
                    eventPayload[3].DataPointer = ((IntPtr) (&OperationType));
                    eventPayload[4].Size = sizeof(int);
                    eventPayload[4].DataPointer = ((IntPtr) (&ActionCount));

                    WriteEventCore(PARALLELINVOKEBEGIN_ID, 5, eventPayload);
                }
            }            
        }
        #endregion

        #region ParallelInvokeEnd
        /// <summary>
        /// Denotes the exit point for a Parallel.Invoke call. 
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="ForkJoinContextID">The invoke ID.</param>
        [Event(PARALLELINVOKEEND_ID, Level = EventLevel.Informational, Task = TplEtwProvider.Tasks.Invoke, Opcode = EventOpcode.Stop)]
        public void ParallelInvokeEnd(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int ForkJoinContextID)
        {
            if (IsEnabled() && IsEnabled(EventLevel.Informational, Keywords.Parallel))
            {
                WriteEvent(PARALLELINVOKEEND_ID, OriginatingTaskSchedulerID, OriginatingTaskID, ForkJoinContextID);
            }
        }
        #endregion

        #region ParallelFork
        /// <summary>
        /// Denotes the start of an individual task that's part of a fork/join context. 
        /// Before this event is fired, the start of the new fork/join context will be marked 
        /// with another event that declares a unique context ID. 
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="ForkJoinContextID">The invoke ID.</param>
        [Event(PARALLELFORK_ID, Level = EventLevel.Verbose, Task = TplEtwProvider.Tasks.ForkJoin, Opcode = EventOpcode.Start)]
        public void ParallelFork(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int ForkJoinContextID)
        {
            if (IsEnabled() && IsEnabled(EventLevel.Verbose, Keywords.Parallel))
            {
                WriteEvent(PARALLELFORK_ID, OriginatingTaskSchedulerID, OriginatingTaskID, ForkJoinContextID);
            }
        }
        #endregion

        #region ParallelJoin
        /// <summary>
        /// Denotes the end of an individual task that's part of a fork/join context. 
        /// This should match a previous ParallelFork event with a matching "OriginatingTaskID"
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="ForkJoinContextID">The invoke ID.</param>
        [Event(PARALLELJOIN_ID, Level = EventLevel.Verbose, Task = TplEtwProvider.Tasks.ForkJoin, Opcode = EventOpcode.Stop)]
        public void ParallelJoin(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int ForkJoinContextID)
        {
            if (IsEnabled() && IsEnabled(EventLevel.Verbose, Keywords.Parallel))
            {
                WriteEvent(PARALLELJOIN_ID, OriginatingTaskSchedulerID, OriginatingTaskID, ForkJoinContextID);
            }
        }
        #endregion

        //-----------------------------------------------------------------------------------
        //        
        // Task Events
        //
        
        // These are all verbose events, so we need to call IsEnabled(EventLevel.Verbose, ALL_KEYWORDS) 
        // call. However since the IsEnabled(l,k) call is more expensive than IsEnabled(), we only want 
        // to incur this cost when instrumentation is enabled. So the Task codepaths that call these
        // event functions still do the check for IsEnabled()

        #region TaskScheduled
        /// <summary>
        /// Fired when a task is queued to a TaskScheduler.
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="TaskID">The task ID.</param>
        /// <param name="CreatingTaskID">The task ID</param>
        /// <param name="TaskCreationOptions">The options used to create the task.</param>
        [SecuritySafeCritical]
        [Event(TASKSCHEDULED_ID, Task = Tasks.TaskScheduled, Opcode = EventOpcode.Send, 
         Level = EventLevel.Informational, Keywords = Keywords.TaskTransfer|Keywords.Tasks)]
        public void TaskScheduled(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int TaskID, int CreatingTaskID, int TaskCreationOptions)
        {
            // IsEnabled() call is an inlined quick check that makes this very fast when provider is off 
            if (IsEnabled() && IsEnabled(EventLevel.Informational, Keywords.TaskTransfer|Keywords.Tasks))
            {
                unsafe
                {
                    EventData* eventPayload = stackalloc EventData[5];
                    eventPayload[0].Size = sizeof(int);
                    eventPayload[0].DataPointer = ((IntPtr) (&OriginatingTaskSchedulerID));
                    eventPayload[1].Size = sizeof(int);
                    eventPayload[1].DataPointer = ((IntPtr) (&OriginatingTaskID));
                    eventPayload[2].Size = sizeof(int);
                    eventPayload[2].DataPointer = ((IntPtr) (&TaskID));
                    eventPayload[3].Size = sizeof(int);
                    eventPayload[3].DataPointer = ((IntPtr) (&CreatingTaskID));
                    eventPayload[4].Size = sizeof(int);
                    eventPayload[4].DataPointer = ((IntPtr) (&TaskCreationOptions));
                    Guid childActivityId = CreateGuidForTaskID(TaskID);
                    WriteEventWithRelatedActivityIdCore(TASKSCHEDULED_ID, &childActivityId, 5, eventPayload);
                }
            }
        }
        #endregion

        #region TaskStarted
        /// <summary>
        /// Fired just before a task actually starts executing.
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="TaskID">The task ID.</param>
        [Event(TASKSTARTED_ID, Task = TplEtwProvider.Tasks.TaskExecute, Opcode = EventOpcode.Start,
         Level = EventLevel.Informational, Keywords = Keywords.Tasks)]
        public void TaskStarted(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int TaskID)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Tasks)) 
                WriteEvent(TASKSTARTED_ID, OriginatingTaskSchedulerID, OriginatingTaskID, TaskID);
        }
        #endregion

        #region TaskCompleted
        /// <summary>
        /// Fired right after a task finished executing.
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="TaskID">The task ID.</param>
        /// <param name="IsExceptional">Whether the task completed due to an error.</param>
        [SecuritySafeCritical]
        [Event(TASKCOMPLETED_ID, Version=1, Task = TplEtwProvider.Tasks.TaskExecute, Opcode = EventOpcode.Stop, 
         Level = EventLevel.Informational, Keywords = Keywords.Tasks)]
        public void TaskCompleted(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int TaskID, bool IsExceptional)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.Tasks)) 
            {
                unsafe
                {
                    EventData* eventPayload = stackalloc EventData[4];
                    Int32 isExceptionalInt = IsExceptional ? 1 : 0;
                    eventPayload[0].Size = sizeof(int);
                    eventPayload[0].DataPointer = ((IntPtr) (&OriginatingTaskSchedulerID));
                    eventPayload[1].Size = sizeof(int);
                    eventPayload[1].DataPointer = ((IntPtr) (&OriginatingTaskID));
                    eventPayload[2].Size = sizeof(int);
                    eventPayload[2].DataPointer = ((IntPtr) (&TaskID));
                    eventPayload[3].Size = sizeof(int);
                    eventPayload[3].DataPointer = ((IntPtr) (&isExceptionalInt));
                    WriteEventCore(TASKCOMPLETED_ID, 4, eventPayload);
                }
            }                
        }
        #endregion

        #region TaskWaitBegin
        /// <summary>
        /// Fired when starting to wait for a taks's completion explicitly or implicitly.
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="TaskID">The task ID.</param>
        /// <param name="Behavior">Configured behavior for the wait.</param>
        [SecuritySafeCritical]
        [Event(TASKWAITBEGIN_ID, Version=1, Task = TplEtwProvider.Tasks.TaskWait, Opcode = EventOpcode.Send, 
         Level = EventLevel.Informational, Keywords = Keywords.TaskTransfer|Keywords.Tasks)]
        public void TaskWaitBegin(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int TaskID, TaskWaitBehavior Behavior)
        {
            if (IsEnabled() && IsEnabled(EventLevel.Informational, Keywords.TaskTransfer|Keywords.Tasks))
            {
                unsafe
                {
                    EventData* eventPayload = stackalloc EventData[4];
                    eventPayload[0].Size = sizeof(int);
                    eventPayload[0].DataPointer = ((IntPtr)(&OriginatingTaskSchedulerID));
                    eventPayload[1].Size = sizeof(int);
                    eventPayload[1].DataPointer = ((IntPtr)(&OriginatingTaskID));
                    eventPayload[2].Size = sizeof(int);
                    eventPayload[2].DataPointer = ((IntPtr)(&TaskID));
                    eventPayload[3].Size = sizeof(int);
                    eventPayload[3].DataPointer = ((IntPtr)(&Behavior));
                    Guid childActivityId = CreateGuidForTaskID(TaskID);
                    WriteEventWithRelatedActivityIdCore(TASKWAITBEGIN_ID, &childActivityId, 4, eventPayload);
                }
            }
        }
        #endregion

        #region TaskWaitEnd
        /// <summary>
        /// Fired when the wait for a tasks completion returns.
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="TaskID">The task ID.</param>
        [Event(TASKWAITEND_ID, Task = TplEtwProvider.Tasks.TaskWait, Opcode = EventOpcode.Stop,
         Level = EventLevel.Verbose, Keywords = Keywords.Tasks)]
        public void TaskWaitEnd(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int TaskID)
        {
            // Log an event if indicated.  
            if (IsEnabled() && IsEnabled(EventLevel.Verbose, Keywords.Tasks))
                WriteEvent(TASKWAITEND_ID, OriginatingTaskSchedulerID, OriginatingTaskID, TaskID);
        }

        /// <summary>
        /// Fired when the an asynchronous continuation for a task is scheduled
        /// </summary>
        /// <param name="OriginatingTaskSchedulerID">The scheduler ID.</param>
        /// <param name="OriginatingTaskID">The task ID.</param>
        /// <param name="TaskID">The activityId for the continuation.</param>
        [SecuritySafeCritical]
        [Event(AWAITTASKCONTINUATIONSCHEDULED_ID, Task = Tasks.AwaitTaskContinuationScheduled, Opcode = EventOpcode.Send, 
         Level = EventLevel.Informational, Keywords = Keywords.TaskTransfer|Keywords.Tasks)]
        public void AwaitTaskContinuationScheduled(
            int OriginatingTaskSchedulerID, int OriginatingTaskID,  // PFX_COMMON_EVENT_HEADER
            int continuationId)
        {
            if (IsEnabled() && IsEnabled(EventLevel.Informational, Keywords.TaskTransfer|Keywords.Tasks))
            { 
                unsafe
                {
                    EventData* eventPayload = stackalloc EventData[3];
                    eventPayload[0].Size = sizeof(int);
                    eventPayload[0].DataPointer = ((IntPtr) (&OriginatingTaskSchedulerID));
                    eventPayload[1].Size = sizeof(int);
                    eventPayload[1].DataPointer = ((IntPtr) (&OriginatingTaskID));
                    eventPayload[2].Size = sizeof(int);
                    eventPayload[2].DataPointer = ((IntPtr) (&continuationId));
                    Guid continuationActivityId = CreateGuidForTaskID(continuationId);
                    WriteEventWithRelatedActivityIdCore(AWAITTASKCONTINUATIONSCHEDULED_ID, &continuationActivityId, 3, eventPayload);
                }
            }
        }

        /// <summary>
        /// Activity IDs are GUIDS but task IDS are integers (and are not unique across appdomains
        /// This routine creates a process wide unique GUID given a task ID
        /// </summary>
        internal static Guid CreateGuidForTaskID(int taskID) 
        {
            // The thread pool generated a process wide unique GUID from a task GUID by
            // using the taskGuid, the appdomain ID, and 8 bytes of 'randomization' chosen by
            // using the last 8 bytes  as the provider GUID for this provider.  
            // These were generated by CreateGuid, and are reasonably random (and thus unlikley to collide
            uint pid = EventSource.s_currentPid;
            int appDomainID = System.Threading.Thread.GetDomainID();
                return new Guid(taskID, 
                                (short) appDomainID , (short) (appDomainID >> 16), 
                                (byte)pid, (byte)(pid >> 8), (byte)(pid >> 16), (byte)(pid >> 24), 
                                0xff, 0xdc, 0xd7, 0xb5);
        }
        #endregion
    }
#endif // !FEATURE_PAL
}
