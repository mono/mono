//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Activities.XamlIntegration;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract(Name = XD.ActivityInstance.Name, Namespace = XD.Runtime.Namespace)]
    [Fx.Tag.XamlVisible(false)]
    public sealed class ActivityInstance : ActivityInstanceMap.IActivityReferenceWithEnvironment
    {
        Activity activity;

        ChildList childList;
        ReadOnlyCollection<ActivityInstance> childCache;

        CompletionBookmark completionBookmark;

        ActivityInstanceMap instanceMap;
        ActivityInstance parent;

        string ownerName;
        int busyCount;
        ExtendedData extendedData;

        // most activities will have a symbol (either variable or argument, so optimize for that case)
        bool noSymbols;

        ActivityInstanceState state;
        bool isCancellationRequested;
        bool performingDefaultCancelation;
        Substate substate;

        long id;

        bool initializationIncomplete;

        // This is serialized through the SerializedEnvironment property
        LocationEnvironment environment;

        ExecutionPropertyManager propertyManager;

        internal ActivityInstance(Activity activity)
        {
            this.activity = activity;
            this.state = ActivityInstanceState.Executing;
            this.substate = Substate.Created;

            this.ImplementationVersion = activity.ImplementationVersion;
        }

        public Activity Activity
        {
            get
            {
                return this.activity;
            }

            internal set
            {
                Fx.Assert(value != null || this.state == ActivityInstanceState.Closed, "");
                this.activity = value;
            }
        }

        Activity ActivityInstanceMap.IActivityReference.Activity
        {
            get
            {
                return this.Activity;
            }
        }

        internal Substate SubState
        {
            get
            {
                return this.substate;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        internal LocationEnvironment SerializedEnvironment
        {
            get
            {
                if (this.IsCompleted)
                {
                    return null;
                }
                else
                {
                    return this.environment;
                }
            }
            set
            {
                Fx.Assert(value != null, "We should never get null here.");

                this.environment = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "busyCount")]
        internal int SerializedBusyCount
        {
            get { return this.busyCount; }
            set { this.busyCount = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "extendedData")]
        internal ExtendedData SerializedExtendedData
        {
            get { return this.extendedData; }
            set { this.extendedData = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "noSymbols")]
        internal bool SerializedNoSymbols
        {
            get { return this.noSymbols; }
            set { this.noSymbols = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "state")]
        internal ActivityInstanceState SerializedState
        {
            get { return this.state; }
            set { this.state = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "isCancellationRequested")]
        internal bool SerializedIsCancellationRequested
        {
            get { return this.isCancellationRequested; }
            set { this.isCancellationRequested = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "performingDefaultCancelation")]
        internal bool SerializedPerformingDefaultCancelation
        {
            get { return this.performingDefaultCancelation; }
            set { this.performingDefaultCancelation = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "substate")]
        internal Substate SerializedSubstate
        {
            get { return this.substate; }
            set { this.substate = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "id")]
        internal long SerializedId
        {
            get { return this.id; }
            set { this.id = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "initializationIncomplete")]
        internal bool SerializedInitializationIncomplete
        {
            get { return this.initializationIncomplete; }
            set { this.initializationIncomplete = value; }
        }

        internal LocationEnvironment Environment
        {
            get
            {
                Fx.Assert(this.environment != null, "There should always be an environment");
                return this.environment;
            }
        }

        internal ActivityInstanceMap InstanceMap
        {
            get
            {
                return this.instanceMap;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return ActivityUtilities.IsCompletedState(this.State);
            }
        }

        public ActivityInstanceState State
        {
            get
            {
                return this.state;
            }
        }

        internal bool IsCancellationRequested
        {
            get
            {
                return this.isCancellationRequested;
            }
            set
            {
                // This is set at the time of scheduling the cancelation work item

                Fx.Assert(!this.isCancellationRequested, "We should not set this if we have already requested cancel.");
                Fx.Assert(value != false, "We should only set this to true.");

                this.isCancellationRequested = value;
            }
        }

        internal bool IsPerformingDefaultCancelation
        {
            get
            {
                return this.performingDefaultCancelation;
            }
        }

        public string Id
        {
            get
            {
                return this.id.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal long InternalId
        {
            get
            {
                return this.id;
            }
        }

        internal bool IsEnvironmentOwner
        {
            get
            {
                return !this.noSymbols;
            }
        }

        internal bool IsResolvingArguments
        {
            get
            {
                return this.substate == Substate.ResolvingArguments;
            }
        }

        internal bool HasNotExecuted
        {
            get
            {
                return (this.substate & Substate.PreExecuting) != 0;
            }
        }

        internal bool HasPendingWork
        {
            get
            {
                if (this.HasChildren)
                {
                    return true;
                }

                // check if we have pending bookmarks or outstanding OperationControlContexts/WorkItems
                if (this.busyCount > 0)
                {
                    return true;
                }

                return false;
            }
        }

        internal bool OnlyHasOutstandingBookmarks
        {
            get
            {
                // If our whole busy count is because of blocking bookmarks then
                // we should return true
                return !this.HasChildren && this.extendedData != null && (this.extendedData.BlockingBookmarkCount == this.busyCount);
            }
        }

        internal ActivityInstance Parent
        {
            get
            {
                return this.parent;
            }
        }

        internal bool WaitingForTransactionContext
        {
            get
            {
                if (this.extendedData == null)
                {
                    return false;
                }
                else
                {
                    return this.extendedData.WaitingForTransactionContext;
                }
            }
            set
            {
                EnsureExtendedData();

                this.extendedData.WaitingForTransactionContext = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        internal CompletionBookmark CompletionBookmark
        {
            get
            {
                return this.completionBookmark;
            }

            set
            {
                this.completionBookmark = value;
            }
        }

        internal FaultBookmark FaultBookmark
        {
            get
            {
                if (this.extendedData == null)
                {
                    return null;
                }

                return this.extendedData.FaultBookmark;
            }

            set
            {
                Fx.Assert(value != null || (this.extendedData == null || this.extendedData.FaultBookmark == null), "cannot go from non-null to null");
                if (value != null)
                {
                    EnsureExtendedData();
                    this.extendedData.FaultBookmark = value;
                }
            }
        }

        internal bool HasChildren
        {
            get
            {
                return (this.childList != null && this.childList.Count > 0);
            }
        }

        internal ExecutionPropertyManager PropertyManager
        {
            get
            {
                return this.propertyManager;
            }
            set
            {
                this.propertyManager = value;
            }
        }

        internal WorkflowDataContext DataContext
        {
            get
            {
                if (this.extendedData != null)
                {
                    return this.extendedData.DataContext;
                }
                return null;
            }
            set
            {
                EnsureExtendedData();
                this.extendedData.DataContext = value;
            }
        }

        internal object CompiledDataContexts
        {
            get;
            set;
        }

        internal object CompiledDataContextsForImplementation
        {
            get;
            set;
        }

        internal bool HasActivityReferences
        {
            get
            {
                return this.extendedData != null && this.extendedData.HasActivityReferences;
            }
        }

        [DataMember(Name = XD.ActivityInstance.PropertyManager, EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal ExecutionPropertyManager SerializedPropertyManager
        {
            get
            {
                if (this.propertyManager == null || !this.propertyManager.ShouldSerialize(this))
                {
                    return null;
                }
                else
                {
                    return this.propertyManager;
                }
            }
            set
            {
                Fx.Assert(value != null, "We don't emit the default value so this should never be null.");
                this.propertyManager = value;
            }
        }

        [DataMember(Name = XD.ActivityInstance.Children, EmitDefaultValue = false)]
        internal ChildList SerializedChildren
        {
            get
            {
                if (this.HasChildren)
                {
                    this.childList.Compress();
                    return this.childList;
                }

                return null;
            }

            set
            {
                Fx.Assert(value != null, "value from Serialization should not be null");
                this.childList = value;
            }
        }

        [DataMember(Name = XD.ActivityInstance.Owner, EmitDefaultValue = false)]
        internal string OwnerName
        {
            get
            {
                if (this.ownerName == null)
                {
                    this.ownerName = this.Activity.GetType().Name;
                }
                return this.ownerName;
            }
            set
            {
                Fx.Assert(value != null, "value from Serialization should not be null");
                this.ownerName = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public Version ImplementationVersion
        {
            get;
            internal set;
        }

        internal static ActivityInstance CreateCompletedInstance(Activity activity)
        {
            ActivityInstance instance = new ActivityInstance(activity);
            instance.state = ActivityInstanceState.Closed;

            return instance;
        }

        internal static ActivityInstance CreateCanceledInstance(Activity activity)
        {
            ActivityInstance instance = new ActivityInstance(activity);
            instance.state = ActivityInstanceState.Canceled;

            return instance;
        }

        internal ReadOnlyCollection<ActivityInstance> GetChildren()
        {
            if (!this.HasChildren)
            {
                return ChildList.Empty;
            }

            if (this.childCache == null)
            {
                this.childCache = this.childList.AsReadOnly();
            }
            return this.childCache;
        }

        internal HybridCollection<ActivityInstance> GetRawChildren()
        {
            return this.childList;
        }

        void EnsureExtendedData()
        {
            if (this.extendedData == null)
            {
                this.extendedData = new ExtendedData();
            }
        }

        // Busy Count includes the following:
        //   1. Active OperationControlContexts.
        //   2. Active work items.
        //   3. Blocking bookmarks.
        internal void IncrementBusyCount()
        {
            this.busyCount++;
        }

        internal void DecrementBusyCount()
        {
            Fx.Assert(this.busyCount > 0, "something went wrong with our bookkeeping");
            this.busyCount--;
        }

        internal void DecrementBusyCount(int amount)
        {
            Fx.Assert(this.busyCount >= amount, "something went wrong with our bookkeeping");
            this.busyCount -= amount;
        }

        internal void AddActivityReference(ActivityInstanceReference reference)
        {
            EnsureExtendedData();
            this.extendedData.AddActivityReference(reference);
        }

        internal void AddBookmark(Bookmark bookmark, BookmarkOptions options)
        {
            bool affectsBusyCount = false;

            if (!BookmarkOptionsHelper.IsNonBlocking(options))
            {
                IncrementBusyCount();
                affectsBusyCount = true;
            }

            EnsureExtendedData();
            this.extendedData.AddBookmark(bookmark, affectsBusyCount);
        }

        internal void RemoveBookmark(Bookmark bookmark, BookmarkOptions options)
        {
            bool affectsBusyCount = false;

            if (!BookmarkOptionsHelper.IsNonBlocking(options))
            {
                DecrementBusyCount();
                affectsBusyCount = true;
            }

            Fx.Assert(this.extendedData != null, "something went wrong with our bookkeeping");
            this.extendedData.RemoveBookmark(bookmark, affectsBusyCount);
        }

        internal void RemoveAllBookmarks(BookmarkScopeManager bookmarkScopeManager, BookmarkManager bookmarkManager)
        {
            if (this.extendedData != null)
            {
                this.extendedData.PurgeBookmarks(bookmarkScopeManager, bookmarkManager, this);
            }
        }

        internal void SetInitializationIncomplete()
        {
            this.initializationIncomplete = true;
        }

        internal void MarkCanceled()
        {
            Fx.Assert(this.substate == Substate.Executing || this.substate == Substate.Canceling, "called from an unexpected state");
            this.substate = Substate.Canceling;
        }

        void MarkExecuted()
        {
            this.substate = Substate.Executing;
        }

        internal void MarkAsComplete(BookmarkScopeManager bookmarkScopeManager, BookmarkManager bookmarkManager)
        {
            if (this.extendedData != null)
            {
                this.extendedData.PurgeBookmarks(bookmarkScopeManager, bookmarkManager, this);

                if (this.extendedData.DataContext != null)
                {
                    this.extendedData.DataContext.Dispose();
                }
            }

            if (this.instanceMap != null)
            {
                this.instanceMap.RemoveEntry(this);

                if (this.HasActivityReferences)
                {
                    this.extendedData.PurgeActivityReferences(this.instanceMap);
                }
            }

            if (this.Parent != null)
            {
                this.Parent.RemoveChild(this);
            }
        }

        internal void Abort(ActivityExecutor executor, BookmarkManager bookmarkManager, Exception terminationReason, bool isTerminate)
        {
            // This is a gentle abort where we try to keep the runtime in a
            // usable state.
            AbortEnumerator abortEnumerator = new AbortEnumerator(this);

            while (abortEnumerator.MoveNext())
            {
                ActivityInstance currentInstance = abortEnumerator.Current;

                if (!currentInstance.HasNotExecuted)
                {
                    currentInstance.Activity.InternalAbort(currentInstance, executor, terminationReason);
                    executor.DebugActivityCompleted(currentInstance);
                }

                if (currentInstance.PropertyManager != null)
                {
                    currentInstance.PropertyManager.UnregisterProperties(currentInstance, currentInstance.Activity.MemberOf, true);
                }

                executor.TerminateSpecialExecutionBlocks(currentInstance, terminationReason);

                executor.CancelPendingOperation(currentInstance);

                executor.HandleRootCompletion(currentInstance);

                currentInstance.MarkAsComplete(executor.RawBookmarkScopeManager, bookmarkManager);

                currentInstance.state = ActivityInstanceState.Faulted;

                currentInstance.FinalizeState(executor, false, !isTerminate);
            }
        }

        internal void BaseCancel(NativeActivityContext context)
        {
            // Default cancelation logic starts here, but is also performed in
            // UpdateState and through special completion work items

            Fx.Assert(this.IsCancellationRequested, "This should be marked to true at this point.");

            this.performingDefaultCancelation = true;

            CancelChildren(context);
        }

        internal void CancelChildren(NativeActivityContext context)
        {
            if (this.HasChildren)
            {
                foreach (ActivityInstance child in this.GetChildren())
                {
                    context.CancelChild(child);
                }
            }
        }

        internal void Cancel(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            this.Activity.InternalCancel(this, executor, bookmarkManager);
        }

        internal void Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            if (this.initializationIncomplete)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InitializationIncomplete));
            }

            MarkExecuted();
            this.Activity.InternalExecute(this, executor, bookmarkManager);
        }

        internal void AddChild(ActivityInstance item)
        {
            if (this.childList == null)
            {
                this.childList = new ChildList();
            }

            this.childList.Add(item);
            this.childCache = null;
        }

        internal void RemoveChild(ActivityInstance item)
        {
            Fx.Assert(this.childList != null, "");
            this.childList.Remove(item, true);
            this.childCache = null;
        }

        // called by ActivityUtilities tree-walk
        internal void AppendChildren(ActivityUtilities.TreeProcessingList nextInstanceList, ref Queue<IList<ActivityInstance>> instancesRemaining)
        {
            Fx.Assert(this.HasChildren, "AppendChildren is tuned to only be called when HasChildren is true");
            this.childList.AppendChildren(nextInstanceList, ref instancesRemaining);
        }

        // called after deserialization of the workflow instance
        internal void FixupInstance(ActivityInstance parent, ActivityInstanceMap instanceMap, ActivityExecutor executor)
        {
            if (this.IsCompleted)
            {
                // We hang onto the root instance even after is it complete.  We skip the fixups
                // for a completed root.
                Fx.Assert(parent == null, "This should only happen to root instances.");

                return;
            }

            if (this.Activity == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ActivityInstanceFixupFailed));
            }

            this.parent = parent;
            this.instanceMap = instanceMap;

            if (this.PropertyManager != null)
            {
                this.PropertyManager.OnDeserialized(this, parent, this.Activity.MemberOf, executor);
            }
            else if (this.parent != null)
            {
                // The current property manager is null here
                this.PropertyManager = this.parent.PropertyManager;
            }
            else
            {
                this.PropertyManager = executor.RootPropertyManager;
            }

            if (!this.noSymbols)
            {
                this.environment.OnDeserialized(executor, this);
            }
        }

        internal bool TryFixupChildren(ActivityInstanceMap instanceMap, ActivityExecutor executor)
        {
            if (!this.HasChildren)
            {
                return false;
            }

            this.childList.FixupList(this, instanceMap, executor);
            return true;
        }

        internal void FillInstanceMap(ActivityInstanceMap instanceMap)
        {
            if (this.IsCompleted)
            {
                // We don't bother adding completed roots to the map
                return;
            }

            Fx.Assert(this.instanceMap == null, "We should never call this unless the current map is null.");
            Fx.Assert(this.Parent == null, "Can only generate a map from a root instance.");

            this.instanceMap = instanceMap;
            ActivityUtilities.ProcessActivityInstanceTree(this, null, new Func<ActivityInstance, ActivityExecutor, bool>(GenerateInstanceMapCallback));
        }

        bool GenerateInstanceMapCallback(ActivityInstance instance, ActivityExecutor executor)
        {
            this.instanceMap.AddEntry(instance);
            instance.instanceMap = this.instanceMap;

            if (instance.HasActivityReferences)
            {
                instance.extendedData.FillInstanceMap(instance.instanceMap);
            }
         
            return true;
        }

        internal bool Initialize(ActivityInstance parent, ActivityInstanceMap instanceMap, LocationEnvironment parentEnvironment, long instanceId, ActivityExecutor executor)
        {
            return this.Initialize(parent, instanceMap, parentEnvironment, instanceId, executor, 0);
        }

        internal bool Initialize(ActivityInstance parent, ActivityInstanceMap instanceMap, LocationEnvironment parentEnvironment, long instanceId, ActivityExecutor executor, int delegateParameterCount)
        {
            this.parent = parent;
            this.instanceMap = instanceMap;
            this.id = instanceId;

            if (this.instanceMap != null)
            {
                this.instanceMap.AddEntry(this);
            }

            // propagate necessary information from our parent
            if (this.parent != null)
            {
                if (this.parent.PropertyManager != null)
                {
                    this.PropertyManager = this.parent.PropertyManager;
                }

                if (parentEnvironment == null)
                {
                    parentEnvironment = this.parent.Environment;
                }
            }

            int symbolCount = this.Activity.SymbolCount + delegateParameterCount;

            if (symbolCount == 0)
            {
                if (parentEnvironment == null)
                {
                    // We create an environment for a root activity that otherwise would not have one
                    // to simplify environment management.
                    this.environment = new LocationEnvironment(executor, this.Activity);
                }
                else
                {
                    this.noSymbols = true;
                    this.environment = parentEnvironment;
                }

                // We don't set Initialized here since the tracking/tracing would be too early
                return false;
            }
            else
            {
                this.environment = new LocationEnvironment(executor, this.Activity, parentEnvironment, symbolCount);
                this.substate = Substate.ResolvingArguments;
                return true;
            }
        }

        internal void ResolveNewArgumentsDuringDynamicUpdate(ActivityExecutor executor, IList<int> dynamicUpdateArgumentIndexes)
        {
            Fx.Assert(!this.noSymbols, "Can only resolve arguments if we created an environment");
            Fx.Assert(this.substate == Substate.Executing, "Dynamically added arguments are to be resolved only in Substate.Executing.");

            if (this.Activity.SkipArgumentResolution)
            {
                return;
            }

            IList<RuntimeArgument> runtimeArguments = this.Activity.RuntimeArguments;

            for (int i = 0; i < dynamicUpdateArgumentIndexes.Count; i++)
            {
                RuntimeArgument argument = runtimeArguments[dynamicUpdateArgumentIndexes[i]];
                Fx.Assert(this.Environment.GetSpecificLocation(argument.Id) == null, "This is a newly added argument so the location should be null");

                this.InternalTryPopulateArgumentValueOrScheduleExpression(argument, -1, executor, null, null, true);
            }
        }

        private bool InternalTryPopulateArgumentValueOrScheduleExpression(RuntimeArgument argument, int nextArgumentIndex, ActivityExecutor executor, IDictionary<string, object> argumentValueOverrides, Location resultLocation, bool isDynamicUpdate)
        {
            object overrideValue = null;
            if (argumentValueOverrides != null)
            {
                argumentValueOverrides.TryGetValue(argument.Name, out overrideValue);
            }

            if (argument.TryPopulateValue(this.environment, this, executor, overrideValue, resultLocation, isDynamicUpdate))
            {
                return true;
            }

            ResolveNextArgumentWorkItem workItem = null;
            Location location = this.environment.GetSpecificLocation(argument.Id);

            if (isDynamicUpdate)
            {
                //1. Check if this argument has a temporary location that needs to be collapsed
                if (location.TemporaryResolutionEnvironment != null)
                {
                    // 2. Add a workitem to collapse the temporary location
                    executor.ScheduleItem(new CollapseTemporaryResolutionLocationWorkItem(location, this));
                }
            }
            else
            {
                //1. Check if there are more arguments to process
                nextArgumentIndex = nextArgumentIndex + 1;

                // 2. Add a workitem to resume argument resolution when
                // work related to 3 below either completes or it hits an async point.           
                int totalArgumentCount = this.Activity.RuntimeArguments.Count;

                if (nextArgumentIndex < totalArgumentCount)
                {
                    workItem = executor.ResolveNextArgumentWorkItemPool.Acquire();
                    workItem.Initialize(this, nextArgumentIndex, argumentValueOverrides, resultLocation);
                }
            }

            // 3. Schedule the argument expression.
            executor.ScheduleExpression(argument.BoundArgument.Expression, this, this.Environment, location, workItem);

            return false;
        }

        // return true if arguments were resolved synchronously
        internal bool ResolveArguments(ActivityExecutor executor, IDictionary<string, object> argumentValueOverrides, Location resultLocation, int startIndex = 0)
        {
            Fx.Assert(!this.noSymbols, "Can only resolve arguments if we created an environment");
            Fx.Assert(this.substate == Substate.ResolvingArguments, "Invalid sub-state machine");

            bool completedSynchronously = true;

            if (this.Activity.IsFastPath)
            {
                // We still need to resolve the result argument
                Fx.Assert(argumentValueOverrides == null, "We shouldn't have any overrides.");
                Fx.Assert(((ActivityWithResult)this.Activity).ResultRuntimeArgument != null, "We should have a result argument");

                RuntimeArgument argument = ((ActivityWithResult)this.Activity).ResultRuntimeArgument;

                if (!argument.TryPopulateValue(this.environment, this, executor, null, resultLocation, false))
                {
                    completedSynchronously = false;

                    Location location = this.environment.GetSpecificLocation(argument.Id);
                    executor.ScheduleExpression(argument.BoundArgument.Expression, this, this.Environment, location, null);
                }
            }
            else if (!this.Activity.SkipArgumentResolution)
            {
                IList<RuntimeArgument> runtimeArguments = this.Activity.RuntimeArguments;

                int argumentCount = runtimeArguments.Count;

                if (argumentCount > 0)
                {
                    for (int i = startIndex; i < argumentCount; i++)
                    {
                        RuntimeArgument argument = runtimeArguments[i];

                        if (!this.InternalTryPopulateArgumentValueOrScheduleExpression(argument, i, executor, argumentValueOverrides, resultLocation, false))
                        {
                            completedSynchronously = false;
                            break;
                        }
                    }
                }
            }

            if (completedSynchronously && startIndex == 0)
            {
                // We only move our state machine forward if this
                // is the first call to ResolveArguments (startIndex
                // == 0).  Otherwise, a call to UpdateState will
                // cause the substate switch (as well as a call to
                // CollapseTemporaryResolutionLocations).
                this.substate = Substate.ResolvingVariables;
            }

            return completedSynchronously;
        }

        internal void ResolveNewVariableDefaultsDuringDynamicUpdate(ActivityExecutor executor, IList<int> dynamicUpdateVariableIndexes, bool forImplementation)
        {
            Fx.Assert(!this.noSymbols, "Can only resolve variable default if we created an environment");
            Fx.Assert(this.substate == Substate.Executing, "Dynamically added variable default expressions are to be resolved only in Substate.Executing.");

            IList<Variable> runtimeVariables;
            if (forImplementation)
            {
                runtimeVariables = this.Activity.ImplementationVariables;
            }
            else
            {
                runtimeVariables = this.Activity.RuntimeVariables;
            }

            for (int i = 0; i < dynamicUpdateVariableIndexes.Count; i++)
            {
                Variable newVariable = runtimeVariables[dynamicUpdateVariableIndexes[i]];
                if (newVariable.Default != null)
                {
                    EnqueueVariableDefault(executor, newVariable, null);
                }
            }
        }

        internal bool ResolveVariables(ActivityExecutor executor)
        {
            Fx.Assert(!this.noSymbols, "can only resolve variables if we created an environment");
            Fx.Assert(this.substate == Substate.ResolvingVariables, "invalid sub-state machine");

            this.substate = Substate.ResolvingVariables;
            bool completedSynchronously = true;

            IList<Variable> implementationVariables = this.Activity.ImplementationVariables;
            IList<Variable> runtimeVariables = this.Activity.RuntimeVariables;

            int implementationVariableCount = implementationVariables.Count;
            int runtimeVariableCount = runtimeVariables.Count;

            if (implementationVariableCount > 0 || runtimeVariableCount > 0)
            {
                for (int i = 0; i < implementationVariableCount; i++)
                {
                    implementationVariables[i].DeclareLocation(executor, this);
                }

                for (int i = 0; i < runtimeVariableCount; i++)
                {
                    runtimeVariables[i].DeclareLocation(executor, this);
                }

                for (int i = 0; i < implementationVariableCount; i++)
                {
                    completedSynchronously &= ResolveVariable(implementationVariables[i], executor);
                }

                for (int i = 0; i < runtimeVariableCount; i++)
                {
                    completedSynchronously &= ResolveVariable(runtimeVariables[i], executor);
                }
            }

            return completedSynchronously;
        }

        // returns true if completed synchronously
        bool ResolveVariable(Variable variable, ActivityExecutor executor)
        {
            bool completedSynchronously = true;
            if (variable.Default != null)
            {
                Location variableLocation = this.Environment.GetSpecificLocation(variable.Id);

                if (variable.Default.UseOldFastPath)
                {
                    variable.PopulateDefault(executor, this, variableLocation);
                }
                else
                {
                    EnqueueVariableDefault(executor, variable, variableLocation);
                    completedSynchronously = false;
                }
            }

            return completedSynchronously;
        }

        void EnqueueVariableDefault(ActivityExecutor executor, Variable variable, Location variableLocation)
        {
            // Incomplete initialization detection logic relies on the fact that we
            // don't specify a completion callback.  If this changes we need to modify
            // callers of SetInitializationIncomplete().
            Fx.Assert(variable.Default != null, "If we've gone async we must have a default");
            if (variableLocation == null)
            {
                variableLocation = this.environment.GetSpecificLocation(variable.Id);
            }
            variable.SetIsWaitingOnDefaultValue(variableLocation);
            executor.ScheduleExpression(variable.Default, this, this.environment, variableLocation, null);
        }

        void ActivityInstanceMap.IActivityReference.Load(Activity activity, ActivityInstanceMap instanceMap)
        {
            if (activity.GetType().Name != this.OwnerName)
            {
                throw FxTrace.Exception.AsError(
                    new ValidationException(SR.ActivityTypeMismatch(activity.DisplayName, this.OwnerName)));
            }

            if (activity.ImplementationVersion != this.ImplementationVersion)
            {
                throw FxTrace.Exception.AsError(new VersionMismatchException(SR.ImplementationVersionMismatch(this.ImplementationVersion, activity.ImplementationVersion, activity)));
            }

            this.Activity = activity;
        }

        // Returns true if the activity completed
        internal bool UpdateState(ActivityExecutor executor)
        {
            bool activityCompleted = false;

            if (this.HasNotExecuted)
            {
                if (this.IsCancellationRequested) // need to cancel any in-flight resolutions and bail
                {
                    if (this.HasChildren)
                    {
                        foreach (ActivityInstance child in this.GetChildren())
                        {
                            Fx.Assert(child.State == ActivityInstanceState.Executing, "should only have children if they're still executing");
                            executor.CancelActivity(child);
                        }
                    }
                    else
                    {
                        SetCanceled();
                        activityCompleted = true;
                    }
                }
                else if (!this.HasPendingWork)
                {
                    bool scheduleBody = false;

                    if (this.substate == Substate.ResolvingArguments)
                    {
                        // if we've had asynchronous resolution of Locations (Out/InOut Arguments), resolve them now
                        this.Environment.CollapseTemporaryResolutionLocations();

                        this.substate = Substate.ResolvingVariables;
                        scheduleBody = ResolveVariables(executor);
                    }
                    else if (this.substate == Substate.ResolvingVariables)
                    {
                        scheduleBody = true;
                    }

                    if (scheduleBody)
                    {
                        executor.ScheduleBody(this, false, null, null);
                    }
                }

                Fx.Assert(this.HasPendingWork || activityCompleted, "should have scheduled work pending if we're not complete");
            }
            else if (!this.HasPendingWork)
            {
                if (!executor.IsCompletingTransaction(this))
                {
                    activityCompleted = true;
                    if (this.substate == Substate.Canceling)
                    {
                        SetCanceled();
                    }
                    else
                    {
                        SetClosed();
                    }
                } 
            }
            else if (this.performingDefaultCancelation)
            {
                if (this.OnlyHasOutstandingBookmarks)
                {
                    RemoveAllBookmarks(executor.RawBookmarkScopeManager, executor.RawBookmarkManager);
                    MarkCanceled();

                    Fx.Assert(!this.HasPendingWork, "Shouldn't have pending work here.");

                    SetCanceled();
                    activityCompleted = true;
                }
            }

            return activityCompleted;
        }

        void TryCancelParent()
        {
            if (this.parent != null && this.parent.IsPerformingDefaultCancelation)
            {
                this.parent.MarkCanceled();
            }
        }

        internal void SetInitializedSubstate(ActivityExecutor executor)
        {
            Fx.Assert(this.substate != Substate.Initialized, "SetInitializedSubstate called when substate is already Initialized.");
            this.substate = Substate.Initialized;
            if (executor.ShouldTrackActivityStateRecordsExecutingState)
            {
                if (executor.ShouldTrackActivity(this.Activity.DisplayName))
                {
                    executor.AddTrackingRecord(new ActivityStateRecord(executor.WorkflowInstanceId, this, this.state));
                }
            }

            if (TD.InArgumentBoundIsEnabled())
            {
                int runtimeArgumentsCount = this.Activity.RuntimeArguments.Count;
                if (runtimeArgumentsCount > 0)
                {
                    for (int i = 0; i < runtimeArgumentsCount; i++)
                    {
                        RuntimeArgument argument = this.Activity.RuntimeArguments[i];

                        if (ArgumentDirectionHelper.IsIn(argument.Direction))
                        {
                            Location location;
                            if (this.environment.TryGetLocation(argument.Id, this.Activity, out location))
                            {
                                string argumentValue = null;

                                if (location.Value == null)
                                {
                                    argumentValue = "<Null>";
                                }
                                else
                                {
                                    argumentValue = "'" + location.Value.ToString() + "'";
                                }

                                TD.InArgumentBound(argument.Name, this.Activity.GetType().ToString(), this.Activity.DisplayName, this.Id, argumentValue);
                            }
                        }
                    }
                }
            }
        }

        internal void FinalizeState(ActivityExecutor executor, bool faultActivity)
        {
            FinalizeState(executor, faultActivity, false);
        }

        internal void FinalizeState(ActivityExecutor executor, bool faultActivity, bool skipTracking)
        {
            if (faultActivity)
            {
                TryCancelParent();

                // We can override previous completion states with this
                this.state = ActivityInstanceState.Faulted;
            }

            Fx.Assert(this.state != ActivityInstanceState.Executing, "We must be in a completed state at this point.");

            if (this.state == ActivityInstanceState.Closed)
            {
                if (executor.ShouldTrackActivityStateRecordsClosedState && !skipTracking)
                {
                    if (executor.ShouldTrackActivity(this.Activity.DisplayName))
                    {
                        executor.AddTrackingRecord(new ActivityStateRecord(executor.WorkflowInstanceId, this, this.state));
                    }
                }
            }
            else
            {
                if (executor.ShouldTrackActivityStateRecords && !skipTracking)
                {
                    executor.AddTrackingRecord(new ActivityStateRecord(executor.WorkflowInstanceId, this, this.state));
                }
            }

            if (TD.ActivityCompletedIsEnabled())
            {
                TD.ActivityCompleted(this.Activity.GetType().ToString(), this.Activity.DisplayName, this.Id, this.State.ToString());
            }

        }

        void SetCanceled()
        {
            Fx.Assert(!this.IsCompleted, "Should not be completed if we are changing the state.");

            TryCancelParent();

            this.state = ActivityInstanceState.Canceled;
        }

        void SetClosed()
        {
            Fx.Assert(!this.IsCompleted, "Should not be completed if we are changing the state.");

            this.state = ActivityInstanceState.Closed;
        }

        static void UpdateLocationEnvironmentHierarchy(LocationEnvironment oldParentEnvironment, LocationEnvironment newEnvironment, ActivityInstance currentInstance)
        {
            Func<ActivityInstance, ActivityExecutor, bool> processInstanceCallback = delegate(ActivityInstance instance, ActivityExecutor executor)
            {
                if (instance == currentInstance)
                {
                    return true;
                }

                if (instance.IsEnvironmentOwner)
                {
                    if (instance.environment.Parent == oldParentEnvironment)
                    {
                        // overwrite its parent with newEnvironment
                        instance.environment.Parent = newEnvironment;
                    }

                    // We do not need to process children instances beyond this point.
                    return false;
                }

                if (instance.environment == oldParentEnvironment)
                {
                    // this instance now points to newEnvironment
                    instance.environment = newEnvironment;
                }

                return true;
            };

            ActivityUtilities.ProcessActivityInstanceTree(currentInstance, null, processInstanceCallback);
        }

        void ActivityInstanceMap.IActivityReferenceWithEnvironment.UpdateEnvironment(EnvironmentUpdateMap map, Activity activity)
        {            
            Fx.Assert(this.substate != Substate.ResolvingVariables, "We must have already performed the same validations in advance.");
            Fx.Assert(this.substate != Substate.ResolvingArguments, "We must have already performed the same validations in advance.");

            if (this.noSymbols)
            {
                // create a new LocationReference and this ActivityInstance becomes the owner of the created environment.
                LocationEnvironment oldParentEnvironment = this.environment;

                Fx.Assert(oldParentEnvironment != null, "environment must never be null.");

                this.environment = new LocationEnvironment(oldParentEnvironment, map.NewArgumentCount + map.NewVariableCount + map.NewPrivateVariableCount + map.RuntimeDelegateArgumentCount);
                this.noSymbols = false;

                // traverse the activity instance chain.
                // Update all its non-environment-owning decedent instances to point to the newly created enviroment,
                // and, update all its environment-owning decendent instances to have their environment's parent to point to the newly created environment.
                UpdateLocationEnvironmentHierarchy(oldParentEnvironment, this.environment, this);
            }

            this.Environment.Update(map, activity);
        }

        internal enum Substate : byte
        {
            Executing = 0, // choose the most common persist-time state for the default
            PreExecuting = 0x80, // used for all states prior to "core execution"
            Created = 1 | Substate.PreExecuting,
            ResolvingArguments = 2 | Substate.PreExecuting,
            // ResolvedArguments = 2,
            ResolvingVariables = 3 | Substate.PreExecuting,
            // ResolvedVariables = 3,
            Initialized = 4 | Substate.PreExecuting,
            Canceling = 5,
        }

        // data necessary to support non-mainline usage of instances (i.e. creating bookmarks, using transactions)
        [DataContract]
        internal class ExtendedData
        {
            BookmarkList bookmarks;
            ActivityReferenceList activityReferences;
            int blockingBookmarkCount;

            public ExtendedData()
            {
            }
                        
            public int BlockingBookmarkCount
            {
                get
                {
                    return blockingBookmarkCount;
                }
                private set
                {
                    blockingBookmarkCount = value;
                }
            }

            [DataMember(Name = XD.ActivityInstance.WaitingForTransactionContext, EmitDefaultValue = false)]
            public bool WaitingForTransactionContext
            {
                get;
                set;
            }

            [DataMember(Name = XD.ActivityInstance.FaultBookmark, EmitDefaultValue = false)]
            public FaultBookmark FaultBookmark
            {
                get;
                set;
            }

            public WorkflowDataContext DataContext
            {
                get;
                set;
            }

            [DataMember(Name = XD.ActivityInstance.BlockingBookmarkCount, EmitDefaultValue = false)]
            internal int SerializedBlockingBookmarkCount
            {
                get { return this.BlockingBookmarkCount; }
                set { this.BlockingBookmarkCount = value; }
            }

            [DataMember(Name = XD.ActivityInstance.Bookmarks, EmitDefaultValue = false)]
            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
            internal BookmarkList Bookmarks
            {
                get
                {
                    if (this.bookmarks == null || this.bookmarks.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return this.bookmarks;
                    }
                }
                set
                {
                    Fx.Assert(value != null, "We don't emit the default value so this should never be null.");
                    this.bookmarks = value;
                }
            }

            [DataMember(Name = XD.ActivityInstance.ActivityReferences, EmitDefaultValue = false)]
            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
            internal ActivityReferenceList ActivityReferences
            {
                get
                {
                    if (this.activityReferences == null || this.activityReferences.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return this.activityReferences;
                    }
                }
                set
                {
                    Fx.Assert(value != null && value.Count > 0, "We shouldn't emit the default value or empty lists");
                    this.activityReferences = value;
                }
            }

            public bool HasActivityReferences
            {
                get
                {
                    return this.activityReferences != null && this.activityReferences.Count > 0;
                }
            }

            public void AddBookmark(Bookmark bookmark, bool affectsBusyCount)
            {
                if (this.bookmarks == null)
                {
                    this.bookmarks = new BookmarkList();
                }

                if (affectsBusyCount)
                {
                    this.BlockingBookmarkCount = this.BlockingBookmarkCount + 1;
                }

                this.bookmarks.Add(bookmark);
            }

            public void RemoveBookmark(Bookmark bookmark, bool affectsBusyCount)
            {
                Fx.Assert(this.bookmarks != null, "The bookmark list should have been initialized if we are trying to remove one.");

                if (affectsBusyCount)
                {
                    Fx.Assert(this.BlockingBookmarkCount > 0, "We should never decrement below zero.");

                    this.BlockingBookmarkCount = this.BlockingBookmarkCount - 1;
                }

                this.bookmarks.Remove(bookmark);
            }

            public void PurgeBookmarks(BookmarkScopeManager bookmarkScopeManager, BookmarkManager bookmarkManager, ActivityInstance owningInstance)
            {
                if (this.bookmarks != null)
                {
                    if (this.bookmarks.Count > 0)
                    {
                        Bookmark singleBookmark;
                        IList<Bookmark> multipleBookmarks;
                        this.bookmarks.TransferBookmarks(out singleBookmark, out multipleBookmarks);
                        this.bookmarks = null;

                        if (bookmarkScopeManager != null)
                        {
                            bookmarkScopeManager.PurgeBookmarks(bookmarkManager, singleBookmark, multipleBookmarks);
                        }
                        else
                        {
                            bookmarkManager.PurgeBookmarks(singleBookmark, multipleBookmarks);
                        }

                        // Clean up the busy count
                        owningInstance.DecrementBusyCount(this.BlockingBookmarkCount);
                        this.BlockingBookmarkCount = 0;
                    }
                }
            }

            public void AddActivityReference(ActivityInstanceReference reference)
            {
                if (this.activityReferences == null)
                {
                    this.activityReferences = new ActivityReferenceList();
                }

                this.activityReferences.Add(reference);
            }

            public void FillInstanceMap(ActivityInstanceMap instanceMap)
            {
                Fx.Assert(this.HasActivityReferences, "Must have references to have called this.");

                this.activityReferences.FillInstanceMap(instanceMap);
            }

            public void PurgeActivityReferences(ActivityInstanceMap instanceMap)
            {
                Fx.Assert(this.HasActivityReferences, "Must have references to have called this.");

                this.activityReferences.PurgeActivityReferences(instanceMap);
            }

            [DataContract]
            internal class ActivityReferenceList : HybridCollection<ActivityInstanceReference>
            {
                public ActivityReferenceList()
                    : base()
                {
                }

                public void FillInstanceMap(ActivityInstanceMap instanceMap)
                {
                    Fx.Assert(this.Count > 0, "Should only call this when we have items");

                    if (this.SingleItem != null)
                    {
                        instanceMap.AddEntry(this.SingleItem);
                    }
                    else
                    {
                        for (int i = 0; i < this.MultipleItems.Count; i++)
                        {
                            ActivityInstanceReference reference = this.MultipleItems[i];

                            instanceMap.AddEntry(reference);
                        }
                    }
                }

                public void PurgeActivityReferences(ActivityInstanceMap instanceMap)
                {
                    Fx.Assert(this.Count > 0, "Should only call this when we have items");

                    if (this.SingleItem != null)
                    {
                        instanceMap.RemoveEntry(this.SingleItem);
                    }
                    else
                    {
                        for (int i = 0; i < this.MultipleItems.Count; i++)
                        {
                            instanceMap.RemoveEntry(this.MultipleItems[i]);
                        }
                    }
                }
            }
        }

        [DataContract]
        internal class ChildList : HybridCollection<ActivityInstance>
        {
            static ReadOnlyCollection<ActivityInstance> emptyChildren;

            public ChildList()
                : base()
            {
            }

            public static ReadOnlyCollection<ActivityInstance> Empty
            {
                get
                {
                    if (emptyChildren == null)
                    {
                        emptyChildren = new ReadOnlyCollection<ActivityInstance>(new ActivityInstance[0]);
                    }

                    return emptyChildren;
                }
            }

            public void AppendChildren(ActivityUtilities.TreeProcessingList nextInstanceList, ref Queue<IList<ActivityInstance>> instancesRemaining)
            {
                // This is only called if there is at least one item in the list.

                if (base.SingleItem != null)
                {
                    nextInstanceList.Add(base.SingleItem);
                }
                else if (nextInstanceList.Count == 0)
                {
                    nextInstanceList.Set(base.MultipleItems);
                }
                else
                {
                    // Next instance list already has some stuff and we have multiple
                    // items.  Let's enqueue them for later processing.

                    if (instancesRemaining == null)
                    {
                        instancesRemaining = new Queue<IList<ActivityInstance>>();
                    }

                    instancesRemaining.Enqueue(base.MultipleItems);
                }
            }

            public void FixupList(ActivityInstance parent, ActivityInstanceMap instanceMap, ActivityExecutor executor)
            {
                if (base.SingleItem != null)
                {
                    base.SingleItem.FixupInstance(parent, instanceMap, executor);
                }
                else
                {
                    for (int i = 0; i < base.MultipleItems.Count; i++)
                    {
                        base.MultipleItems[i].FixupInstance(parent, instanceMap, executor);
                    }
                }
            }
        }

        // Does a depth first walk and uses some knowledge of
        // the abort process to determine which child to visit next
        class AbortEnumerator : IEnumerator<ActivityInstance>
        {
            ActivityInstance root;
            ActivityInstance current;

            bool initialized;

            public AbortEnumerator(ActivityInstance root)
            {
                this.root = root;
            }

            public ActivityInstance Current
            {
                get
                {
                    return this.current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                if (!this.initialized)
                {
                    this.current = root;

                    // We start by diving down the tree along the
                    // "first child" path
                    while (this.current.HasChildren)
                    {
                        this.current = this.current.GetChildren()[0];
                    }

                    this.initialized = true;

                    return true;
                }
                else
                {
                    if (this.current == this.root)
                    {
                        // We're done if we returned all the way to the root last time
                        return false;
                    }
                    else
                    {
                        Fx.Assert(!this.current.Parent.GetChildren().Contains(this.current), "We should always have removed the current one from the parent's list by now.");

                        this.current = this.current.Parent;

                        // Dive down the tree of remaining first children
                        while (this.current.HasChildren)
                        {
                            this.current = this.current.GetChildren()[0];
                        }

                        return true;
                    }
                }
            }

            public void Reset()
            {
                this.current = null;
                this.initialized = false;
            }

            public void Dispose()
            {
                // no op
            }
        }
    }    
}
