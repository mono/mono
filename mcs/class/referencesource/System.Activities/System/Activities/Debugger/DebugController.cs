//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Debugger
{
    using System;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Activities.Validation;

    // DebugController, one is needed per ActivityExecutor.
    [DebuggerNonUserCode]
    class DebugController
    {
        WorkflowInstance host;
        DebugManager debugManager;  // Instantiated after first instrumentation is successful.

        public DebugController(WorkflowInstance host)
        {
            this.host = host;
        }
       
        public void WorkflowStarted()
        {   
        }
      
        public void WorkflowCompleted()
        {
            if (this.debugManager != null)  
            {
                this.debugManager.Exit();
                this.debugManager = null;
            }
        }

        public void ActivityStarted(ActivityInstance activityInstance)
        {
            if (!(activityInstance.Activity.RootActivity is Constraint))  // Don't debug an activity in a Constraint
            {
                EnsureActivityInstrumented(activityInstance, false);
                this.debugManager.OnEnterState(activityInstance);
            }
        }

        public void ActivityCompleted(ActivityInstance activityInstance)
        {
            if (!(activityInstance.Activity.RootActivity is Constraint)) // Don't debug an activity in a Constraint
            {
                EnsureActivityInstrumented(activityInstance, true);
                this.debugManager.OnLeaveState(activityInstance);
            }
        }

        // Lazy instrumentation.
        // Parameter primeCurrentInstance specify whether priming (if needed) is done
        // up to the current instance.  Set this to true when calling this from an "...Completed" 
        // (exit state).
        void EnsureActivityInstrumented(ActivityInstance instance, bool primeCurrentInstance)
        {          
            if (this.debugManager == null)
            {   // Workflow has not been instrumented yet.

                // Finding rootInstance and check all referred sources.
                Stack<ActivityInstance> ancestors = new Stack<ActivityInstance>();
                while (instance.Parent != null)
                {
                    ancestors.Push(instance);
                    instance = instance.Parent;
                }

                Activity rootActivity = instance.Activity;

                // Do breakOnStartup only if debugger is attached from the beginning, i.e. no priming needed.
                // This specified by change the last parameter below to: "(ancestors.Count == 0)".
                this.debugManager = new DebugManager(rootActivity, "Workflow", "Workflow", "DebuggerThread", false, this.host, ancestors.Count == 0);

                if (ancestors.Count > 0)
                {
                    // Priming the background thread 
                    this.debugManager.IsPriming = true;
                    while (ancestors.Count > 0)
                    {
                        ActivityInstance ancestorInstance = ancestors.Pop();
                        this.debugManager.OnEnterState(ancestorInstance);
                    }
                    if (primeCurrentInstance)
                    {
                        this.debugManager.OnEnterState(instance);
                    }
                    this.debugManager.IsPriming = false;
                }
            }
        }
    }
}
