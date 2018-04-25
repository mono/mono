// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Activities;
    using System.Activities.Runtime;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;

    public class NativeActivityUpdateContext 
    {
        private bool isDisposed;
        private ActivityInstanceMap instanceMap;
        private ActivityExecutor activityExecutor;
        private ActivityInstance currentInstance;
        private NativeActivityContext innerContext;
        private DynamicUpdateMap updateMap;
        private DynamicUpdateMapEntry mapEntry;
        private DynamicUpdateMap rootMap;

        internal NativeActivityUpdateContext(ActivityInstanceMap instanceMap, ActivityExecutor activityExecutor, ActivityInstance currentInstance, DynamicUpdateMap updateMap, DynamicUpdateMapEntry mapEntry, DynamicUpdateMap rootMap)
        {
            this.isDisposed = false;
            this.instanceMap = instanceMap;
            this.activityExecutor = activityExecutor;           
            this.currentInstance = currentInstance;
            this.updateMap = updateMap;
            this.mapEntry = mapEntry;
            this.rootMap = rootMap;
            this.innerContext = new NativeActivityContext(this.currentInstance, this.activityExecutor, this.activityExecutor.RawBookmarkManager);

            Fx.Assert(
                this.instanceMap != null &&
                this.activityExecutor != null &&
                this.currentInstance != null &&
                this.currentInstance.Activity != null,
                "instanceMap, execturo, updateMap, targetDefinition, currentActivity and currentInstnace all must not be null.");
        }

        public string ActivityInstanceId 
        {
            get
            {
                ThrowIfDisposed();
                return this.currentInstance.Id;
            }
        }

        public bool IsCancellationRequested
        {
            get
            {
                ThrowIfDisposed();
                return this.currentInstance.IsCancellationRequested;
            }
        }

        public BookmarkScope DefaultBookmarkScope
        {
            get
            {
                ThrowIfDisposed();
                return this.activityExecutor.BookmarkScopeManager.Default;
            }
        }

        internal Activity CurrentActivity
        {
            get { return this.currentInstance.Activity; }
        }

        internal bool IsUpdateDisallowed
        {
            get;
            private set;
        }

        internal string DisallowedReason
        {
            get;
            private set;
        }

        public object FindExecutionProperty(string name)
        {
            ThrowIfDisposed();
            ExecutionProperties exeProperties = new ExecutionProperties(this.innerContext, this.currentInstance, this.currentInstance.PropertyManager);
            return exeProperties.Find(name);
        }

        public void DisallowUpdate(string reason)
        {
            ThrowIfDisposed();
            this.DisallowedReason = reason;
            this.IsUpdateDisallowed = true;
        }

        public object GetSavedOriginalValue(Activity childActivity)
        {
            ThrowIfDisposed();
            bool isReferencedChild;
            NativeActivityUpdateMapMetadata.ValidateOriginalValueAccess(this.CurrentActivity, childActivity, "childActivity", out isReferencedChild);
            if (!isReferencedChild && !this.updateMap.IsNoChanges)
            {
                // 
                // if the map is a NoChanges map, it is gauranteed that no original value has ever been saved
                // 
                DynamicUpdateMapEntry entry;
                if (this.updateMap.TryGetUpdateEntryByNewId(childActivity.InternalId, out entry))
                {
                    return entry.SavedOriginalValueFromParent;
                }
            }

            return null;
        }

        public bool IsNewlyAdded(Activity childActivity)
        {
            ThrowIfDisposed();
            bool isReferencedChild;
            NativeActivityUpdateMapMetadata.ValidateOriginalValueAccess(this.CurrentActivity, childActivity, "childActivity", out isReferencedChild);
            DynamicUpdateMap mapContainingChild;
            DynamicUpdateMapEntry entry;
            if (isReferencedChild)
            {
                mapContainingChild = this.rootMap;
                int[] idArray = childActivity.QualifiedId.AsIDArray();
                for (int i = 0; i < idArray.Length - 1; i++)
                {
                    mapContainingChild.TryGetUpdateEntryByNewId(idArray[i], out entry);
                    Fx.Assert(entry != null, "entry must not be null here.");

                    if (entry.ImplementationUpdateMap == null)
                    {
                        // if a reference childActivity were newly added,
                        // then we must have had a complete map chain from rootMap to the implementation map for the IdSpace the childActivity is declared in. 
                        return false;
                    }

                    mapContainingChild = entry.ImplementationUpdateMap;
                }
            }
            else
            {
                if (this.updateMap == DynamicUpdateMap.DummyMap)
                {
                    // if a non-reference childActivity were newly added,
                    // then we must have had a valid map, not DummyMap.
                    return false;
                }

                mapContainingChild = this.updateMap;                
            }

            return !mapContainingChild.TryGetUpdateEntryByNewId(childActivity.InternalId, out entry);
        }

        public object GetSavedOriginalValue(string propertyName)
        {
            ThrowIfDisposed();
            if (propertyName == null)
            {
                throw FxTrace.Exception.ArgumentNull("propertyName");
            }

            object result = null;
            if (this.mapEntry.SavedOriginalValues != null)
            {
                this.mapEntry.SavedOriginalValues.TryGetValue(propertyName, out result);
            }
            return result;
        }

        public object GetValue(Argument argument)
        {
            ThrowIfDisposed();
            return this.innerContext.GetValue(argument);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "We explicitly provide a RuntimeArgument overload to avoid requiring the object type parameter.")]
        public object GetValue(RuntimeArgument runtimeArgument)
        {
            ThrowIfDisposed();
            return this.innerContext.GetValue(runtimeArgument);
        }

        public void SetValue(Argument argument, object value)
        {
            ThrowIfDisposed();
            this.innerContext.SetValue(argument, value);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]      
        public T GetValue<T>(Variable<T> variable)
        {
            ThrowIfDisposed();
            return this.innerContext.GetValue(variable);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "We explicitly provide a Variable overload to avoid requiring the object type parameter.")]
        public object GetValue(Variable variable)
        {
            ThrowIfDisposed();
            return this.innerContext.GetValue(variable);
        }       

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public void SetValue<T>(Variable<T> variable, T value)
        {
            ThrowIfDisposed();
            this.innerContext.SetValue(variable, value);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "We explicitly provide a Variable overload to avoid requiring the object type parameter.")]
        public void SetValue(Variable variable, object value)
        {
            ThrowIfDisposed();
            this.innerContext.SetValue(variable, value);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters, Justification = "Generic needed for type inference")]
        public Location<T> GetLocation<T>(Variable variable)
        {
            ThrowIfDisposed();
            return this.innerContext.GetLocation<T>(variable);
        }        

        public Bookmark CreateBookmark(string name)
        {
            ThrowIfDisposed();
            return this.innerContext.CreateBookmark(name);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback)
        {
            return this.CreateBookmark(name, callback, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkOptions options)
        {
            ThrowIfDisposed();
            return this.innerContext.CreateBookmark(name, callback, options);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkScope scope)
        {
            return this.CreateBookmark(name, callback, scope, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkScope scope, BookmarkOptions options)
        {
            ThrowIfDisposed();
            return this.innerContext.CreateBookmark(name, callback, scope, options);
        }

        public Bookmark CreateBookmark()
        {
            return this.CreateBookmark((BookmarkCallback)null);
        }

        public Bookmark CreateBookmark(BookmarkCallback callback)
        {            
            return this.CreateBookmark(callback, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(BookmarkCallback callback, BookmarkOptions options)
        {
            ThrowIfDisposed();
            return this.innerContext.CreateBookmark(callback, options);
        }
        
        public void RemoveAllBookmarks()
        {
            ThrowIfDisposed();
            this.innerContext.RemoveAllBookmarks();
        }

        public bool RemoveBookmark(string name)
        {
            ThrowIfDisposed();
            return this.innerContext.RemoveBookmark(name);
        }

        public bool RemoveBookmark(Bookmark bookmark)
        {
            ThrowIfDisposed();
            return this.innerContext.RemoveBookmark(bookmark);
        }

        public bool RemoveBookmark(string name, BookmarkScope scope)
        {
            ThrowIfDisposed();
            return this.innerContext.RemoveBookmark(name, scope);
        }
        
        public void ScheduleActivity(Activity activity)
        {
            this.ScheduleActivity(activity, null, null);
        }

        public void ScheduleActivity(Activity activity, CompletionCallback onCompleted)
        {
            this.ScheduleActivity(activity, onCompleted, null);
        }

        public void ScheduleActivity(Activity activity, FaultCallback onFaulted)
        {
            this.ScheduleActivity(activity, null, onFaulted);
        }

        public void ScheduleActivity(Activity activity, CompletionCallback onCompleted, FaultCallback onFaulted)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleActivity(activity, onCompleted, onFaulted);
        }
        
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleActivity<TResult>(Activity<TResult> activity, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleActivity<TResult>(activity, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction(ActivityAction activityAction, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T>(ActivityAction<T> activityAction, T argument, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction<T>(activityAction, argument, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2>(ActivityAction<T1, T2> activityAction, T1 argument1, T2 argument2, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, argument1, argument2, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3>(ActivityAction<T1, T2, T3> activityAction, T1 argument1, T2 argument2, T3 argument3, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, argument1, argument2, argument3, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4>(
            ActivityAction<T1, T2, T3, T4> activityAction, 
            T1 argument1, 
            T2 argument2, 
            T3 argument3, 
            T4 argument4,
            CompletionCallback onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, argument1, argument2, argument3, argument4, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5>(
            ActivityAction<T1, T2, T3, T4, T5> activityAction, 
            T1 argument1, 
            T2 argument2, 
            T3 argument3, 
            T4 argument4, 
            T5 argument5,
            CompletionCallback onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, argument1, argument2, argument3, argument4, argument5, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6>(
            ActivityAction<T1, T2, T3, T4, T5, T6> activityAction,
            T1 argument1, 
            T2 argument2, 
            T3 argument3, 
            T4 argument4, 
            T5 argument5, 
            T6 argument6,
            CompletionCallback onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, argument1, argument2, argument3, argument4, argument5, argument6, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7> activityAction,
            T1 argument1, 
            T2 argument2, 
            T3 argument3, 
            T4 argument4, 
            T5 argument5,
            T6 argument6,
            T7 argument7,
            CompletionCallback onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, argument1, argument2, argument3, argument4, argument5, argument6, argument7, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8> activityAction,
            T1 argument1, 
            T2 argument2, 
            T3 argument3,
            T4 argument4, 
            T5 argument5,
            T6 argument6, 
            T7 argument7, 
            T8 argument8,
            CompletionCallback onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, argument1, argument2, argument3, argument4, argument5, argument6, argument7, argument8, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> activityAction,
            T1 argument1,
            T2 argument2, 
            T3 argument3, 
            T4 argument4,
            T5 argument5, 
            T6 argument6,
            T7 argument7, 
            T8 argument8,
            T9 argument9,
            CompletionCallback onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(activityAction, argument1, argument2, argument3, argument4, argument5, argument6, argument7, argument8, argument9, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> activityAction,
            T1 argument1,
            T2 argument2, 
            T3 argument3, 
            T4 argument4, 
            T5 argument5,
            T6 argument6,
            T7 argument7, 
            T8 argument8,
            T9 argument9, 
            T10 argument10,
            CompletionCallback onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(
                activityAction, 
                argument1, 
                argument2, 
                argument3, 
                argument4, 
                argument5, 
                argument6, 
                argument7, 
                argument8, 
                argument9,
                argument10, 
                onCompleted, 
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> activityAction,
            T1 argument1, 
            T2 argument2, 
            T3 argument3, 
            T4 argument4, 
            T5 argument5,
            T6 argument6,
            T7 argument7, 
            T8 argument8,
            T9 argument9, 
            T10 argument10,
            T11 argument11,
            CompletionCallback onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(
                activityAction,
                argument1,
                argument2,
                argument3, 
                argument4, 
                argument5, 
                argument6,
                argument7,
                argument8,
                argument9,
                argument10, 
                argument11,
                onCompleted,
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> activityAction,
            T1 argument1,
            T2 argument2, 
            T3 argument3,
            T4 argument4, 
            T5 argument5, 
            T6 argument6, 
            T7 argument7, 
            T8 argument8,
            T9 argument9,
            T10 argument10,
            T11 argument11,
            T12 argument12,
            CompletionCallback onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(
                activityAction,
                argument1,
                argument2,
                argument3, 
                argument4,
                argument5,
                argument6,
                argument7, 
                argument8, 
                argument9,
                argument10,
                argument11,
                argument12, 
                onCompleted, 
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> activityAction,
            T1 argument1,
            T2 argument2,
            T3 argument3, 
            T4 argument4, 
            T5 argument5, 
            T6 argument6, 
            T7 argument7, 
            T8 argument8,
            T9 argument9,
            T10 argument10, 
            T11 argument11, 
            T12 argument12, 
            T13 argument13,
            CompletionCallback onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(
                activityAction,
                argument1,
                argument2,
                argument3,
                argument4,
                argument5,
                argument6, 
                argument7, 
                argument8,
                argument9, 
                argument10, 
                argument11,
                argument12,
                argument13, 
                onCompleted, 
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> activityAction,
            T1 argument1,
            T2 argument2, 
            T3 argument3,
            T4 argument4,
            T5 argument5, 
            T6 argument6,
            T7 argument7,
            T8 argument8,
            T9 argument9,
            T10 argument10,
            T11 argument11,
            T12 argument12, 
            T13 argument13,
            T14 argument14,
            CompletionCallback onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(
                activityAction,
                argument1, 
                argument2, 
                argument3, 
                argument4, 
                argument5,
                argument6,
                argument7,
                argument8,
                argument9,
                argument10,
                argument11,
                argument12, 
                argument13, 
                argument14,
                onCompleted,
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> activityAction,
            T1 argument1,
            T2 argument2,
            T3 argument3,
            T4 argument4,
            T5 argument5,
            T6 argument6,
            T7 argument7,
            T8 argument8,
            T9 argument9,
            T10 argument10,
            T11 argument11,
            T12 argument12, 
            T13 argument13, 
            T14 argument14, 
            T15 argument15,
            CompletionCallback onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(
                activityAction,
                argument1, 
                argument2,
                argument3,
                argument4,
                argument5,
                argument6,
                argument7,
                argument8,
                argument9,
                argument10,
                argument11, 
                argument12, 
                argument13, 
                argument14,
                argument15,
                onCompleted, 
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> activityAction,
            T1 argument1, 
            T2 argument2,
            T3 argument3, 
            T4 argument4, 
            T5 argument5, 
            T6 argument6, 
            T7 argument7, 
            T8 argument8,
            T9 argument9,
            T10 argument10,
            T11 argument11,
            T12 argument12, 
            T13 argument13, 
            T14 argument14,
            T15 argument15,
            T16 argument16,
            CompletionCallback onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleAction(
                activityAction,
                argument1,
                argument2,
                argument3,
                argument4, 
                argument5,
                argument6,
                argument7,
                argument8, 
                argument9, 
                argument10, 
                argument11,
                argument12,
                argument13, 
                argument14, 
                argument15,
                argument16, 
                onCompleted,
                onFaulted);
        }
        
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<TResult>(ActivityFunc<TResult> activityFunc, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T, TResult>(ActivityFunc<T, TResult> activityFunc, T argument, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, TResult>(ActivityFunc<T1, T2, TResult> activityFunc, T1 argument1, T2 argument2, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, TResult>(
            ActivityFunc<T1, T2, T3, TResult> activityFunc, 
            T1 argument1, 
            T2 argument2, 
            T3 argument3,
            CompletionCallback<TResult> onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, argument3, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, TResult>(
            ActivityFunc<T1, T2, T3, T4, TResult> activityFunc, 
            T1 argument1,
            T2 argument2, 
            T3 argument3, 
            T4 argument4,
            CompletionCallback<TResult> onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, argument3, argument4, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
           Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, TResult> activityFunc,
            T1 argument1, 
            T2 argument2, 
            T3 argument3, 
            T4 argument4, 
            T5 argument5,
            CompletionCallback<TResult> onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, argument3, argument4, argument5, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, TResult> activityFunc,
            T1 argument1, 
            T2 argument2,
            T3 argument3,
            T4 argument4,
            T5 argument5,
            T6 argument6,
            CompletionCallback<TResult> onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, argument3, argument4, argument5, argument6, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, TResult> activityFunc,
            T1 argument1, 
            T2 argument2,
            T3 argument3,
            T4 argument4,
            T5 argument5, 
            T6 argument6, 
            T7 argument7,
            CompletionCallback<TResult> onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, argument3, argument4, argument5, argument6, argument7, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> activityFunc,
            T1 argument1, 
            T2 argument2,
            T3 argument3,
            T4 argument4,
            T5 argument5,
            T6 argument6, 
            T7 argument7, 
            T8 argument8,
            CompletionCallback<TResult> onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, argument3, argument4, argument5, argument6, argument7, argument8, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> activityFunc,
            T1 argument1, 
            T2 argument2,
            T3 argument3, 
            T4 argument4,
            T5 argument5, 
            T6 argument6, 
            T7 argument7,
            T8 argument8,
            T9 argument9,
            CompletionCallback<TResult> onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, argument3, argument4, argument5, argument6, argument7, argument8, argument9, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> activityFunc,
            T1 argument1,
            T2 argument2,
            T3 argument3, 
            T4 argument4,
            T5 argument5, 
            T6 argument6,
            T7 argument7, 
            T8 argument8,
            T9 argument9,
            T10 argument10,
            CompletionCallback<TResult> onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(activityFunc, argument1, argument2, argument3, argument4, argument5, argument6, argument7, argument8, argument9, argument10, onCompleted, onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> activityFunc,
            T1 argument1,
            T2 argument2, 
            T3 argument3,
            T4 argument4,
            T5 argument5, 
            T6 argument6,
            T7 argument7, 
            T8 argument8,
            T9 argument9, 
            T10 argument10, 
            T11 argument11,
            CompletionCallback<TResult> onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(
                activityFunc, 
                argument1, 
                argument2, 
                argument3, 
                argument4, 
                argument5, 
                argument6,
                argument7, 
                argument8, 
                argument9,
                argument10,
                argument11,
                onCompleted, 
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> activityFunc,
            T1 argument1, 
            T2 argument2,
            T3 argument3,
            T4 argument4,
            T5 argument5,
            T6 argument6,
            T7 argument7, 
            T8 argument8,
            T9 argument9, 
            T10 argument10,
            T11 argument11, 
            T12 argument12,
            CompletionCallback<TResult> onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(
                activityFunc,
                argument1,
                argument2,
                argument3,
                argument4,
                argument5,
                argument6,
                argument7,
                argument8, 
                argument9, 
                argument10,
                argument11,
                argument12,
                onCompleted, 
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> activityFunc,
            T1 argument1,
            T2 argument2, 
            T3 argument3,
            T4 argument4,
            T5 argument5, 
            T6 argument6,
            T7 argument7,
            T8 argument8,
            T9 argument9,
            T10 argument10,
            T11 argument11, 
            T12 argument12,
            T13 argument13,
            CompletionCallback<TResult> onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(
                activityFunc,
                argument1, 
                argument2,
                argument3,
                argument4, 
                argument5, 
                argument6,
                argument7,
                argument8,
                argument9, 
                argument10, 
                argument11,
                argument12, 
                argument13,
                onCompleted, 
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> activityFunc,
            T1 argument1,
            T2 argument2,
            T3 argument3,
            T4 argument4,
            T5 argument5,
            T6 argument6,
            T7 argument7,
            T8 argument8,
            T9 argument9,
            T10 argument10,
            T11 argument11, 
            T12 argument12, 
            T13 argument13,
            T14 argument14,
            CompletionCallback<TResult> onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(
                activityFunc,
                argument1,
                argument2, 
                argument3,
                argument4, 
                argument5,
                argument6,
                argument7,
                argument8,
                argument9,
                argument10,
                argument11,
                argument12, 
                argument13, 
                argument14,
                onCompleted,
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> activityFunc,
            T1 argument1,
            T2 argument2,
            T3 argument3, 
            T4 argument4,
            T5 argument5,
            T6 argument6,
            T7 argument7,
            T8 argument8,
            T9 argument9, 
            T10 argument10,
            T11 argument11, 
            T12 argument12,
            T13 argument13,
            T14 argument14, 
            T15 argument15,
            CompletionCallback<TResult> onCompleted = null,
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(
                activityFunc,
                argument1,
                argument2,
                argument3, 
                argument4,
                argument5, 
                argument6,
                argument7,
                argument8,
                argument9,
                argument10,
                argument11,
                argument12,
                argument13, 
                argument14,
                argument15,
                onCompleted,
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> activityFunc,
            T1 argument1,
            T2 argument2,
            T3 argument3,
            T4 argument4,
            T5 argument5,
            T6 argument6, 
            T7 argument7,
            T8 argument8,
            T9 argument9,
            T10 argument10, 
            T11 argument11, 
            T12 argument12,
            T13 argument13, 
            T14 argument14,
            T15 argument15,
            T16 argument16,
            CompletionCallback<TResult> onCompleted = null, 
            FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleFunc(
                activityFunc, 
                argument1,
                argument2,
                argument3, 
                argument4,
                argument5, 
                argument6,
                argument7,
                argument8, 
                argument9, 
                argument10, 
                argument11, 
                argument12, 
                argument13,
                argument14,
                argument15,
                argument16,
                onCompleted,
                onFaulted);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public void ScheduleDelegate(ActivityDelegate activityDelegate, IDictionary<string, object> inputParameters, DelegateCompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();
            this.innerContext.ScheduleDelegate(activityDelegate, inputParameters, onCompleted, onFaulted);
        }

        // extra insurance against misuse (if someone stashes away the execution context to use later)
        internal void Dispose()
        {
            this.isDisposed = true;
            this.instanceMap = null;
            this.activityExecutor = null;
            this.currentInstance = null;
            if (this.innerContext != null)
            {
                this.innerContext.Dispose();
                this.innerContext = null;
            }
        }

        internal void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw FxTrace.Exception.AsError(
                    new ObjectDisposedException(this.GetType().FullName, SR.NAUCDisposed));
            }
        }
    }
}
