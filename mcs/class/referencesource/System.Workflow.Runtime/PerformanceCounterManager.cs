// ****************************************************************************
// Copyright (C)  Microsoft Corporation.  All rights reserved.
//
// CONTENTS
//     Performance counters used by default host
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    internal enum PerformanceCounterOperation
    {
        Increment,
        Decrement
    }

    internal enum PerformanceCounterAction
    {
        Aborted,
        Completion,
        Creation,
        Unloading,
        Executing,
        Idle,
        NotExecuting,
        Persisted,
        Loading,
        Runnable,
        Suspension,
        Resumption,
        Termination,
        Starting,
    }

    internal sealed class PerformanceCounterManager
    {
        private static String c_PerformanceCounterCategoryName = ExecutionStringManager.PerformanceCounterCategory;

        // Create a declarative model for specifying performance counter behavior.

        private static PerformanceCounterData[] s_DefaultPerformanceCounters = new PerformanceCounterData[]
        {
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesCreatedName,
                                        ExecutionStringManager.PerformanceCounterSchedulesCreatedDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Creation, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesCreatedRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesCreatedRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Creation, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesUnloadedName,
                                        ExecutionStringManager.PerformanceCounterSchedulesUnloadedDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Unloading, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesUnloadedRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesUnloadedRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Unloading, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesLoadedName,
                                        ExecutionStringManager.PerformanceCounterSchedulesLoadedDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Loading, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesLoadedRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesLoadedRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Loading, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesCompletedName,
                                        ExecutionStringManager.PerformanceCounterSchedulesCompletedDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Completion, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesCompletedRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesCompletedRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Completion, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesSuspendedName,
                                        ExecutionStringManager.PerformanceCounterSchedulesSuspendedDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Suspension, PerformanceCounterOperation.Increment ),
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Resumption, PerformanceCounterOperation.Decrement ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesSuspendedRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesSuspendedRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Suspension, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesTerminatedName,
                                        ExecutionStringManager.PerformanceCounterSchedulesTerminatedDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Termination, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesTerminatedRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesTerminatedRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Termination, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesInMemoryName,
                                        ExecutionStringManager.PerformanceCounterSchedulesInMemoryDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Creation, PerformanceCounterOperation.Increment ),
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Loading, PerformanceCounterOperation.Increment ),
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Unloading, PerformanceCounterOperation.Decrement ),
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Completion, PerformanceCounterOperation.Decrement ),
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Termination, PerformanceCounterOperation.Decrement ),
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Aborted, PerformanceCounterOperation.Decrement ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesExecutingName,
                                        ExecutionStringManager.PerformanceCounterSchedulesExecutingDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Executing, PerformanceCounterOperation.Increment ),
                        new PerformanceCounterActionMapping( PerformanceCounterAction.NotExecuting, PerformanceCounterOperation.Decrement ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesIdleRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesIdleRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Idle, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesRunnableName,
                                        ExecutionStringManager.PerformanceCounterSchedulesRunnableDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Runnable, PerformanceCounterOperation.Increment ),
                        new PerformanceCounterActionMapping( PerformanceCounterAction.NotExecuting, PerformanceCounterOperation.Decrement ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesAbortedName,
                                        ExecutionStringManager.PerformanceCounterSchedulesAbortedDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Aborted, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesAbortedRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesAbortedRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Aborted, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesPersistedName,
                                        ExecutionStringManager.PerformanceCounterSchedulesPersistedDescription,
                                        PerformanceCounterType.NumberOfItems64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Persisted, PerformanceCounterOperation.Increment ),
                    }),
            new PerformanceCounterData( ExecutionStringManager.PerformanceCounterSchedulesPersistedRateName,
                                        ExecutionStringManager.PerformanceCounterSchedulesPersistedRateDescription,
                                        PerformanceCounterType.RateOfCountsPerSecond64,
                    new PerformanceCounterActionMapping[]
                    {
                        new PerformanceCounterActionMapping( PerformanceCounterAction.Persisted, PerformanceCounterOperation.Increment ),
                    }),
        };

        private String m_instanceName;
        private Dictionary<PerformanceCounterAction, List<PerformanceCounterStatement>> m_actionStatements;

        internal PerformanceCounterManager()
        {
        }

        internal void Initialize(WorkflowRuntime runtime)
        {
            runtime.WorkflowExecutorInitializing += WorkflowExecutorInitializing;
        }

        internal void Uninitialize(WorkflowRuntime runtime)
        {
            runtime.WorkflowExecutorInitializing -= WorkflowExecutorInitializing;
        }

        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", 
            Justification = "Design has been approved.")]
        internal void SetInstanceName(String instanceName)
        {
            PerformanceCounterData[] data = s_DefaultPerformanceCounters;

            if (String.IsNullOrEmpty(instanceName))
            {
                try
                {
                    // The assert is safe here as we never give out the instance name.
                    new System.Security.Permissions.SecurityPermission(System.Security.Permissions.PermissionState.Unrestricted).Assert();
                    Process process = Process.GetCurrentProcess();
                    ProcessModule mainModule = process.MainModule;
                    instanceName = mainModule.ModuleName;
                }
                finally
                {
                    System.Security.CodeAccessPermission.RevertAssert();
                }
            }

            this.m_instanceName = instanceName;

            // Build a mapping of PerformanceCounterActions to the actual actions that need
            // to be performed.  If this become a perf issue, we could build the default mapping
            // at build time.

            Dictionary<PerformanceCounterAction, List<PerformanceCounterStatement>> actionStatements = new Dictionary<PerformanceCounterAction, List<PerformanceCounterStatement>>();

            if (PerformanceCounterCategory.Exists(c_PerformanceCounterCategoryName))
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    PerformanceCounterData currentData = data[i];
                    for (int j = 0; j < currentData.Mappings.Length; ++j)
                    {
                        PerformanceCounterActionMapping currentMapping = currentData.Mappings[j];
                        if (!actionStatements.ContainsKey(currentMapping.Action))
                        {
                            actionStatements.Add(currentMapping.Action, new List<PerformanceCounterStatement>());
                        }
                        List<PerformanceCounterStatement> lStatements = actionStatements[currentMapping.Action];
                        PerformanceCounterStatement newStatement = new PerformanceCounterStatement(CreateCounters(currentData.Name), currentMapping.Operation);
                        lStatements.Add(newStatement);
                    }
                }
            }

            this.m_actionStatements = actionStatements;
        }

        private void Notify(PerformanceCounterAction action, WorkflowExecutor executor)
        {
            System.Diagnostics.Debug.Assert(this.m_actionStatements != null);

            List<PerformanceCounterStatement> lStatements;

            if (this.m_actionStatements.TryGetValue(action, out lStatements))
            {
                foreach (PerformanceCounterStatement statement in lStatements)
                {
                    NotifyCounter(action, statement, executor);
                }
            }
        }

        internal List<PerformanceCounter> CreateCounters(String name)
        {
            List<PerformanceCounter> counters = new List<PerformanceCounter>();

            counters.Add(
                new PerformanceCounter(
                        c_PerformanceCounterCategoryName,
                        name,
                        "_Global_",
                        false));

            if (!String.IsNullOrEmpty(this.m_instanceName))
            {
                counters.Add(
                    new PerformanceCounter(
                            c_PerformanceCounterCategoryName,
                            name,
                            this.m_instanceName,
                            false));
            }

            return counters;
        }

        private void NotifyCounter(PerformanceCounterAction action, PerformanceCounterStatement statement, WorkflowExecutor executor)
        {
            foreach (PerformanceCounter counter in statement.Counters)
            {
                switch (statement.Operation)
                {
                    case PerformanceCounterOperation.Increment:
                        counter.Increment();
                        break;

                    case PerformanceCounterOperation.Decrement:
                        counter.Decrement();
                        break;

                    default:
                        System.Diagnostics.Debug.Assert(false, "Unknown performance counter operation.");
                        break;
                }
            }
        }

        private void WorkflowExecutorInitializing(object sender, WorkflowRuntime.WorkflowExecutorInitializingEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (null == e)
                throw new ArgumentNullException("e");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException("sender");

            WorkflowExecutor exec = (WorkflowExecutor)sender;

            exec.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(WorkflowExecutionEvent);
        }

        private void WorkflowExecutionEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidArgumentType, "sender", typeof(WorkflowExecutor).ToString()));

            WorkflowExecutor exec = (WorkflowExecutor)sender;

            PerformanceCounterAction action;

            switch (e.EventType)
            {
                case WorkflowEventInternal.Created:
                    action = PerformanceCounterAction.Creation;
                    break;
                case WorkflowEventInternal.Started:
                    action = PerformanceCounterAction.Starting;
                    break;
                case WorkflowEventInternal.Runnable:
                    action = PerformanceCounterAction.Runnable;
                    break;
                case WorkflowEventInternal.Executing:
                    action = PerformanceCounterAction.Executing;
                    break;
                case WorkflowEventInternal.NotExecuting:
                    action = PerformanceCounterAction.NotExecuting;
                    break;
                case WorkflowEventInternal.Resumed:
                    action = PerformanceCounterAction.Resumption;
                    break;
                case WorkflowEventInternal.SchedulerEmpty:
                    //
                    // SchedulerEmpty signals that are about to persist
                    // after which we will be idle.  We need to do the idle
                    // work now so that it is included in the state for the idle persist.
                    action = PerformanceCounterAction.Idle;
                    break;
                case WorkflowEventInternal.Completed:
                    action = PerformanceCounterAction.Completion;
                    break;
                case WorkflowEventInternal.Suspended:
                    action = PerformanceCounterAction.Suspension;
                    break;
                case WorkflowEventInternal.Terminated:
                    action = PerformanceCounterAction.Termination;
                    break;
                case WorkflowEventInternal.Loaded:
                    action = PerformanceCounterAction.Loading;
                    break;
                case WorkflowEventInternal.Aborted:
                    action = PerformanceCounterAction.Aborted;
                    break;
                case WorkflowEventInternal.Unloaded:
                    action = PerformanceCounterAction.Unloading;
                    break;
                case WorkflowEventInternal.Persisted:
                    action = PerformanceCounterAction.Persisted;
                    break;
                default:
                    return;
            }
            Notify(action, exec);
        }
    }

    internal struct PerformanceCounterData
    {
        internal String Name;
        internal String Description;
        internal PerformanceCounterType CounterType;
        internal PerformanceCounterActionMapping[] Mappings;

        internal PerformanceCounterData(
            String name,
            String description,
            PerformanceCounterType counterType,
            PerformanceCounterActionMapping[] mappings)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(name));
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(description));
            System.Diagnostics.Debug.Assert(mappings != null && mappings.Length != 0);

            this.Name = name;
            this.Description = description;
            this.CounterType = counterType;
            this.Mappings = mappings;
        }
    };

    internal struct PerformanceCounterActionMapping
    {
        internal PerformanceCounterOperation Operation;
        internal PerformanceCounterAction Action;

        internal PerformanceCounterActionMapping(PerformanceCounterAction action, PerformanceCounterOperation operation)
        {
            this.Operation = operation;
            this.Action = action;
        }
    }

    internal struct PerformanceCounterStatement
    {
        internal List<PerformanceCounter> Counters;
        internal PerformanceCounterOperation Operation;


        internal PerformanceCounterStatement(List<PerformanceCounter> counters,
            PerformanceCounterOperation operation)
        {
            System.Diagnostics.Debug.Assert(counters != null);

            this.Counters = counters;
            this.Operation = operation;
        }
    }
}
