//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Activities.Debugger
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.Hosting;
    using System.Activities.Runtime;
    using System.Activities.Statements;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime;

    [DebuggerNonUserCode]
    class DebugManager
    {
        static StateManager.DynamicModuleManager dynamicModuleManager;
        WorkflowInstance host;
        StateManager stateManager;
        Dictionary<object, State> states;
        Dictionary<int, Stack<Activity>> runningThreads;
        InstrumentationTracker instrumentationTracker;
        List<string> temporaryFiles;

        public DebugManager(Activity root, string moduleNamePrefix, string typeNamePrefix, string auxiliaryThreadName, bool breakOnStartup,
            WorkflowInstance host, bool debugStartedAtRoot) :
            this(root, moduleNamePrefix, typeNamePrefix, auxiliaryThreadName, breakOnStartup, host, debugStartedAtRoot, false)
        {
        }

        internal DebugManager(Activity root, string moduleNamePrefix, string typeNamePrefix, string auxiliaryThreadName, bool breakOnStartup, 
            WorkflowInstance host, bool debugStartedAtRoot, bool resetDynamicModule)
        {
            if (resetDynamicModule)
            {
                dynamicModuleManager = null;
            }

            if (dynamicModuleManager == null)
            {
                dynamicModuleManager = new StateManager.DynamicModuleManager(moduleNamePrefix);
            }

            this.stateManager = new StateManager(
                new StateManager.Properties
                    {
                        ModuleNamePrefix = moduleNamePrefix,
                        TypeNamePrefix = typeNamePrefix,
                        AuxiliaryThreadName = auxiliaryThreadName,
                        BreakOnStartup = breakOnStartup
                    },
                    debugStartedAtRoot, dynamicModuleManager);
            
            this.states = new Dictionary<object, State>();
            this.runningThreads = new Dictionary<int, Stack<Activity>>();
            this.instrumentationTracker = new InstrumentationTracker(root);
            this.host = host;
        }

        // Whether we're priming the background thread (in Attach To Process case).
        public bool IsPriming
        {
            set { this.stateManager.IsPriming = value; }
        }

        // Whether debugging is done from the start of the root workflow,
        // contrast to attaching into the middle of a running workflow.
        bool DebugStartedAtRoot
        {
            get
            {
                return this.stateManager.DebugStartedAtRoot;
            }
        }

        internal void Instrument(Activity activity)
        {
            bool isTemporaryFile = false;
            string sourcePath = null;
            bool instrumentationFailed = false;
            Dictionary<string, byte[]> checksumCache = null;
            try
            {
                byte[] checksum;
                Dictionary<object, SourceLocation> sourceLocations = SourceLocationProvider.GetSourceLocations(activity, out sourcePath, out isTemporaryFile, out checksum);
                if (checksum != null)
                {
                    checksumCache = new Dictionary<string, byte[]>();
                    checksumCache.Add(sourcePath.ToUpperInvariant(), checksum);
                }
                Instrument(activity, sourceLocations, Path.GetFileNameWithoutExtension(sourcePath), checksumCache);
            }
            catch (Exception ex)
            {
                instrumentationFailed = true;
                Trace.WriteLine(SR.DebugInstrumentationFailed(ex.Message));

                if (Fx.IsFatal(ex))
                {
                    throw;
                }
            }

            List<Activity> sameSourceActivities = this.instrumentationTracker.GetSameSourceSubRoots(activity);
            this.instrumentationTracker.MarkInstrumented(activity);

            foreach (Activity sameSourceActivity in sameSourceActivities)
            {
                if (!instrumentationFailed)
                {
                    MapInstrumentationStates(activity, sameSourceActivity);
                }
                // Mark it as instrumentated, even though it fails so it won't be
                // retried.
                this.instrumentationTracker.MarkInstrumented(sameSourceActivity);
            }

            if (isTemporaryFile)
            {
                if (this.temporaryFiles == null)
                {
                    this.temporaryFiles = new List<string>();
                }
                Fx.Assert(!string.IsNullOrEmpty(sourcePath), "SourcePath cannot be null for temporary file");
                this.temporaryFiles.Add(sourcePath);
            }
        }

        // Workflow rooted at rootActivity1 and rootActivity2 have same source file, but they
        // are two different instantiation.
        // rootActivity1 has been instrumented and its instrumentation states can be
        // re-used by rootActivity2.
        //
        // MapInstrumentationStates will walk both Workflow trees in parallel and map every 
        // state for activities in rootActivity1 to corresponding activities in rootActivity2.
        void MapInstrumentationStates(Activity rootActivity1, Activity rootActivity2)
        {
            Queue<KeyValuePair<Activity, Activity>> pairsRemaining = new Queue<KeyValuePair<Activity, Activity>>();

            pairsRemaining.Enqueue(new KeyValuePair<Activity, Activity>(rootActivity1, rootActivity2));
            HashSet<Activity> visited = new HashSet<Activity>();
            KeyValuePair<Activity, Activity> currentPair;
            State state;

            while (pairsRemaining.Count > 0)
            {
                currentPair = pairsRemaining.Dequeue();
                Activity activity1 = currentPair.Key;
                Activity activity2 = currentPair.Value;

                if (this.states.TryGetValue(activity1, out state))
                {
                    if (this.states.ContainsKey(activity2))
                    {
                        Trace.WriteLine("Workflow", SR.DuplicateInstrumentation(activity2.DisplayName));
                    }
                    else
                    {
                        // Map activity2 to the same state.
                        this.states.Add(activity2, state);
                    }
                }
                //Some activities may not have corresponding Xaml node, e.g. ActivityFaultedOutput.

                visited.Add(activity1);

                // This to avoid comparing any value expression with DesignTimeValueExpression (in designer case).
                IEnumerator<Activity> enumerator1 = WorkflowInspectionServices.GetActivities(activity1).GetEnumerator();
                IEnumerator<Activity> enumerator2 = WorkflowInspectionServices.GetActivities(activity2).GetEnumerator();

                bool hasNextItem1 = enumerator1.MoveNext();
                bool hasNextItem2 = enumerator2.MoveNext();

                while (hasNextItem1 && hasNextItem2)
                {
                    if (!visited.Contains(enumerator1.Current))  // avoid adding the same activity (e.g. some default implementation).
                    {
                        if (enumerator1.Current.GetType() != enumerator2.Current.GetType())
                        {
                            // Give debugger log instead of just asserting; to help user find out mismatch problem.
                            Trace.WriteLine(
                                "Unmatched type: " + enumerator1.Current.GetType().FullName +
                                " vs " + enumerator2.Current.GetType().FullName + "\n");
                        }
                        pairsRemaining.Enqueue(new KeyValuePair<Activity, Activity>(enumerator1.Current, enumerator2.Current));
                    }

                    hasNextItem1 = enumerator1.MoveNext();
                    hasNextItem2 = enumerator2.MoveNext();
                }

                // If enumerators do not finish at the same time, then they have unmatched number of activities.
                // Give debugger log instead of just asserting; to help user find out mismatch problem.
                if (hasNextItem1 || hasNextItem2)
                {
                    Trace.WriteLine("Workflow", "Unmatched number of children\n");
                }
            }
        }
        

        // Main instrumentation.
        // Currently the typeNamePrefix is used to notify the Designer of which file to show.
        // This will no longer necessary when the callstack API can give us the source line 
        // information.
        public void Instrument(Activity rootActivity, Dictionary<object, SourceLocation> sourceLocations, string typeNamePrefix, Dictionary<string, byte[]> checksumCache)
        {
            Queue<KeyValuePair<Activity, string>> pairsRemaining = new Queue<KeyValuePair<Activity, string>>();

            string name;
            Activity activity = rootActivity;
            KeyValuePair<Activity, string> pair = new KeyValuePair<Activity, string>(activity, string.Empty);
            pairsRemaining.Enqueue(pair);
            HashSet<string> existingNames = new HashSet<string>();
            HashSet<Activity> visited = new HashSet<Activity>();
            SourceLocation sourceLocation;

            while (pairsRemaining.Count > 0)
            {
                pair = pairsRemaining.Dequeue();
                activity = pair.Key;
                string parentName = pair.Value;
                string displayName = activity.DisplayName;
                
                // If no DisplayName, then use the type name.
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = activity.GetType().Name;
                }

                if (parentName == string.Empty)
                {   // the root
                    name = displayName;
                }
                else
                {
                    name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", parentName, displayName);
                }

                int i = 0;
                while (existingNames.Contains(name))
                {
                    ++i;
                    name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}{2}", parentName, displayName, i.ToString(CultureInfo.InvariantCulture));
                }

                existingNames.Add(name);

                visited.Add(activity);

                if (sourceLocations.TryGetValue(activity, out sourceLocation))
                {
                    object[] objects = activity.GetType().GetCustomAttributes(typeof(DebuggerStepThroughAttribute), false);
                    if ((objects == null || objects.Length == 0))
                    {
                        Instrument(activity, sourceLocation, name);
                    }
                }

                foreach (Activity childActivity in WorkflowInspectionServices.GetActivities(activity))
                {
                    if (!visited.Contains(childActivity))
                    {
                        pairsRemaining.Enqueue(new KeyValuePair<Activity, string>(childActivity, name));
                    }
                }
            }
            this.stateManager.Bake(typeNamePrefix, checksumCache);
        }

        // Exiting the DebugManager.
        // Delete all temporary files 
        public void Exit()
        {
            if (this.temporaryFiles != null)
            {
                foreach (string temporaryFile in this.temporaryFiles)
                {
                    // Clean up published source.
                    try
                    {
                        File.Delete(temporaryFile);
                    }
                    catch (IOException)
                    {
                        // ---- IOException silently.
                    }
                    this.temporaryFiles = null;
                }
            }
            this.stateManager.ExitThreads(); // State manager is still keep for the session in SessionStateManager
            this.stateManager = null;
        }

        void Instrument(Activity activity, SourceLocation sourceLocation, string name)
        {
            Fx.Assert(activity != null, "activity can't be null");
            Fx.Assert(sourceLocation != null, "sourceLocation can't be null");
            if (this.states.ContainsKey(activity))
            {
                Trace.WriteLine(SR.DuplicateInstrumentation(activity.DisplayName));
            }
            else
            {
                State activityState = this.stateManager.DefineStateWithDebugInfo(sourceLocation, name);
                this.states.Add(activity, activityState);
            }
        }

        // Test whether activity has been instrumented.
        // If not, try to instrument it.
        // It will return true if instrumentation is already done or
        // instrumentation is succesful.  False otherwise.
        bool EnsureInstrumented(Activity activity)
        {
            // This is the most common case, we will find the instrumentation.
            if (this.states.ContainsKey(activity))
            {
                return true;
            }
            
            // No states correspond to this yet.
            if (this.instrumentationTracker.IsUninstrumentedSubRoot(activity))
            {
                Instrument(activity);
                return this.states.ContainsKey(activity);
            }
            else
            {
                return false;
            }
        }

        // Primitive EnterState
        void EnterState(int threadId, Activity activity, Dictionary<string, object> locals)
        {
            Fx.Assert(activity != null, "activity cannot be null");
            this.Push(threadId, activity);

            State activityState;
            if (this.states.TryGetValue(activity, out activityState))
            {
                this.stateManager.EnterState(threadId, activityState, locals);
            }
            else
            {
                Fx.Assert(false, "Uninstrumented activity is disallowed: " + activity.DisplayName);
            }
        }
        public void OnEnterState(ActivityInstance instance)
        {
            Fx.Assert(instance != null, "ActivityInstance cannot be null");
            Activity activity = instance.Activity;

            if (this.EnsureInstrumented(activity))
            {
                this.EnterState(GetOrCreateThreadId(activity, instance), activity, GenerateLocals(instance));
            }
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters)]
        public void OnEnterState(Activity expression, ActivityInstance instance, LocationEnvironment environment)
        {
            if (this.EnsureInstrumented(expression))
            {
                this.EnterState(GetOrCreateThreadId(expression, instance), expression, GenerateLocals(instance));
            }
        }

        void LeaveState(Activity activity)
        {
            Fx.Assert(activity != null, "Activity cannot be null");
            int threadId = GetExecutingThreadId(activity, true);

            // If debugging was not started from the root, then threadId should not be < 0.
            Fx.Assert(!this.DebugStartedAtRoot || threadId >= 0, "Leaving from an unknown state");

            if (threadId >= 0) 
            {
                State activityState;
                if (this.states.TryGetValue(activity, out activityState))
                {
                    this.stateManager.LeaveState(threadId, activityState);
                }
                else
                {
                    Fx.Assert(false, "Uninstrumented activity is disallowed: " + activity.DisplayName);
                }
                this.Pop(threadId);
            }
        }

        public void OnLeaveState(ActivityInstance activityInstance)
        {
            Fx.Assert(activityInstance != null, "ActivityInstance cannot be null");
            if (this.EnsureInstrumented(activityInstance.Activity))
            {
                this.LeaveState(activityInstance.Activity);
            }
        }

        static Dictionary<string, object> GenerateLocals(ActivityInstance instance)
        {
            Dictionary<string, object> locals = new Dictionary<string, object>();
            locals.Add("debugInfo", new DebugInfo(instance));
            return locals;
        }

        void Push(int threadId, Activity activity)
        {
            ((Stack<Activity>)this.runningThreads[threadId]).Push(activity);
        }

        void Pop(int threadId)
        {
            Stack<Activity> stack = this.runningThreads[threadId];
            stack.Pop();
            if (stack.Count == 0)
            {
                this.stateManager.Exit(threadId);
                this.runningThreads.Remove(threadId);
            }
        }

        // Given an activity, return the thread id where it is currently
        // executed (on the top of the callstack).
        // Boolean "strict" parameter determine whether the activity itself should
        // be on top of the stack.
        // Strict checking is needed in the case of "Leave"-ing a state.
        // Non-strict checking is needed for "Enter"-ing a state, since the direct parent of 
        // the activity may not yet be executed (e.g. the activity is an argument of another activity,
        // the activity is "enter"-ed even though the direct parent is not yet "enter"-ed.
        int GetExecutingThreadId(Activity activity, bool strict)
        {
            int threadId = -1;

            foreach (KeyValuePair<int, Stack<Activity>> entry in this.runningThreads)
            {
                Stack<Activity> threadStack = entry.Value;
                if (threadStack.Peek() == activity)
                {
                    threadId = entry.Key;
                    break;
                }
            }

            if (threadId < 0 && !strict)
            {
                foreach (KeyValuePair<int, Stack<Activity>> entry in this.runningThreads)
                {
                    Stack<Activity> threadStack = entry.Value;
                    Activity topActivity = threadStack.Peek();
                    if (!IsParallelActivity(topActivity) && IsAncestorOf(threadStack.Peek(), activity))
                    {
                        threadId = entry.Key;
                        break;
                    }
                }
            }
            return threadId;
        }

        static bool IsAncestorOf(Activity ancestorActivity, Activity activity)
        {
            Fx.Assert(activity != null, "IsAncestorOf: Cannot pass null as activity");
            Fx.Assert(ancestorActivity != null, "IsAncestorOf: Cannot pass null as ancestorActivity");
            activity = activity.Parent;
            while (activity != null && activity != ancestorActivity && !IsParallelActivity(activity))
            {
                activity = activity.Parent;
            }
            return (activity == ancestorActivity);
        }

        static bool IsParallelActivity(Activity activity)
        {
            Fx.Assert(activity != null, "IsParallel: Cannot pass null as activity");
            return activity is Parallel ||
                    (activity.GetType().IsGenericType && activity.GetType().GetGenericTypeDefinition() == typeof(ParallelForEach<>));
        }

        // Get threads currently executing the parent of the given activity, 
        // if none then create a new one and prep the call stack to current state.
        int GetOrCreateThreadId(Activity activity, ActivityInstance instance)
        {
            int threadId = -1;
            if (activity.Parent != null && !IsParallelActivity(activity.Parent))
            {
                threadId = GetExecutingThreadId(activity.Parent, false);
            }
            if (threadId < 0)
            {
                threadId = CreateLogicalThread(activity, instance, false);
            }
            return threadId;
        }

        // Create logical thread and bring its call stack to reflect call from
        // the root up to (but not including) the instance.
        // If the activity is an expression though, then the call stack will also include the instance
        // (since it is the parent of the expression).
        int CreateLogicalThread(Activity activity, ActivityInstance instance, bool primeCurrentInstance)
        {
            Stack<ActivityInstance> ancestors = null;

            if (!this.DebugStartedAtRoot)
            {
                ancestors = new Stack<ActivityInstance>();

                if (activity != instance.Activity || primeCurrentInstance)
                {   // This mean that activity is an expression and 
                    // instance is the parent of this expression.
                   
                    Fx.Assert(primeCurrentInstance || (activity is ActivityWithResult), "Expect an ActivityWithResult");
                    Fx.Assert(primeCurrentInstance || (activity.Parent == instance.Activity), "Argument Expression is not given correct parent instance");
                    if (primeCurrentInstance || !IsParallelActivity(instance.Activity))
                    {
                        ancestors.Push(instance);
                    }
                }

                ActivityInstance instanceParent = instance.Parent;
                while (instanceParent != null && !IsParallelActivity(instanceParent.Activity))
                {
                    ancestors.Push(instanceParent);
                    instanceParent = instanceParent.Parent;
                }

                if (instanceParent != null && IsParallelActivity(instanceParent.Activity))
                {
                    // Ensure thread is created for the parent (a Parallel activity).
                    int parentThreadId = GetExecutingThreadId(instanceParent.Activity, false);
                    if (parentThreadId < 0)
                    {
                        parentThreadId = CreateLogicalThread(instanceParent.Activity, instanceParent, true);
                        Fx.Assert(parentThreadId > 0, "Parallel main thread can't be created");
                    }
                }
            }

            string threadName = "DebuggerThread:";
            if (activity.Parent != null)
            {
                threadName += activity.Parent.DisplayName;
            }
            else // Special case for the root of WorklowService that does not have a parent.
            {   
                threadName += activity.DisplayName;
            }

            int newThreadId = this.stateManager.CreateLogicalThread(threadName);
            Stack<Activity> newStack = new Stack<Activity>();
            this.runningThreads.Add(newThreadId, newStack);

            if (!this.DebugStartedAtRoot && ancestors != null)
            { // Need to create callstack to current activity.                        
                PrimeCallStack(newThreadId, ancestors);
            }
            
            return newThreadId;
        }

        // Prime the call stack to contains all the ancestors of this instance.
        // Note: the call stack will not include the current instance.
        void PrimeCallStack(int threadId, Stack<ActivityInstance> ancestors)
        {
            Fx.Assert(!this.DebugStartedAtRoot, "Priming should not be called if the debugging is attached from the start of the workflow");
            bool currentIsPrimingValue = this.stateManager.IsPriming;
            this.stateManager.IsPriming = true;
            while (ancestors.Count > 0)
            {
                ActivityInstance currentInstance = ancestors.Pop();
                if (EnsureInstrumented(currentInstance.Activity))
                {
                    this.EnterState(threadId, currentInstance.Activity, GenerateLocals(currentInstance));
                }
            }
            this.stateManager.IsPriming = currentIsPrimingValue;
        }
    }
}
