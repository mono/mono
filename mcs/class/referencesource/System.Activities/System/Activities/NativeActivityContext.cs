//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class NativeActivityContext : ActivityContext
    {
        BookmarkManager bookmarkManager;
        ActivityExecutor executor;

        // This is called by the Pool.
        internal NativeActivityContext()
        {
        }

        // This is only used by base classes which do not take
        // part in pooling.
        internal NativeActivityContext(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
            : base(instance, executor)
        {
            this.executor = executor;
            this.bookmarkManager = bookmarkManager;
        }

        public BookmarkScope DefaultBookmarkScope
        {
            get
            {
                ThrowIfDisposed();
                return this.executor.BookmarkScopeManager.Default;
            }
        }

        public bool IsCancellationRequested
        {
            get
            {
                ThrowIfDisposed();
                return this.CurrentInstance.IsCancellationRequested;
            }
        }

        public ExecutionProperties Properties
        {
            get
            {
                ThrowIfDisposed();
                return new ExecutionProperties(this, this.CurrentInstance, this.CurrentInstance.PropertyManager);
            }
        }

        internal bool HasRuntimeTransaction
        {
            get
            {
                return this.executor.HasRuntimeTransaction;
            }
        }

        internal bool RequiresTransactionContextWaiterExists
        {
            get
            {
                return this.executor.RequiresTransactionContextWaiterExists;
            }
        }

        internal bool IsInNoPersistScope
        {
            get
            {
                if ((this.Properties.Find(NoPersistProperty.Name) != null) || (this.executor.HasRuntimeTransaction))
                {
                    return true;
                }
                return false;
            }
        }

        internal void Initialize(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            base.Reinitialize(instance, executor);
            this.executor = executor;
            this.bookmarkManager = bookmarkManager;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public T GetValue<T>(Variable<T> variable)
        {
            ThrowIfDisposed();

            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }

            return GetValueCore<T>(variable);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "We explicitly provide a Variable overload to avoid requiring the object type parameter.")]
        public object GetValue(Variable variable)
        {
            ThrowIfDisposed();

            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }

            return GetValueCore<object>(variable);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public void SetValue<T>(Variable<T> variable, T value)
        {
            ThrowIfDisposed();

            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }

            SetValueCore(variable, value);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "We explicitly provide a Variable overload to avoid requiring the object type parameter.")]
        public void SetValue(Variable variable, object value)
        {
            ThrowIfDisposed();

            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }

            SetValueCore(variable, value);
        }

        public void CancelChildren()
        {
            ThrowIfDisposed();

            this.CurrentInstance.CancelChildren(this);
        }

        public ReadOnlyCollection<ActivityInstance> GetChildren()
        {
            ThrowIfDisposed();

            return this.CurrentInstance.GetChildren();
        }

        public void AbortChildInstance(ActivityInstance activity)
        {
            AbortChildInstance(activity, null);
        }

        public void AbortChildInstance(ActivityInstance activity, Exception reason)
        {
            ThrowIfDisposed();

            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }

            if (activity.IsCompleted)
            {
                // We shortcut since we might not actually have
                // a reference to the parent for an already
                // completed child.
                return;
            }

            if (!object.ReferenceEquals(activity.Parent, this.CurrentInstance))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CanOnlyAbortDirectChildren));
            }

            this.executor.AbortActivityInstance(activity, reason);
        }

        public void Abort()
        {
            Abort(null);
        }

        public void Abort(Exception reason)
        {
            ThrowIfDisposed();
            this.executor.AbortWorkflowInstance(reason);
        }

        internal void Terminate(Exception reason)
        {
            this.executor.ScheduleTerminate(reason);
        }

        public void Track(CustomTrackingRecord record)
        {
            ThrowIfDisposed();

            if (record == null)
            {
                throw FxTrace.Exception.ArgumentNull("record");
            }

            base.TrackCore(record);
        }

        public void CancelChild(ActivityInstance activityInstance)
        {
            ThrowIfDisposed();
            if (activityInstance == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityInstance");
            }

            if (activityInstance.IsCompleted)
            {
                // We shortcut since we might not actually have
                // a reference to the parent for an already
                // completed child.
                return;
            }

            if (!object.ReferenceEquals(activityInstance.Parent, this.CurrentInstance))
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.CanOnlyCancelDirectChildren));
            }

            this.executor.CancelActivity(activityInstance);
        }

        internal void Cancel()
        {
            ThrowIfDisposed();
            this.CurrentInstance.BaseCancel(this);
        }

        public Bookmark CreateBookmark(string name)
        {
            // We don't allow BookmarkOptions to be specified for bookmarks without callbacks
            // because it must be Blocking and SingleFire to be of any value

            ThrowIfDisposed();
            ThrowIfCanInduceIdleNotSet();

            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            return this.bookmarkManager.CreateBookmark(name, null, this.CurrentInstance, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback)
        {
            return CreateBookmark(name, callback, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkOptions options)
        {
            ThrowIfDisposed();
            ThrowIfCanInduceIdleNotSet();
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            if (callback == null)
            {
                throw FxTrace.Exception.ArgumentNull("callback");
            }

            if (!CallbackWrapper.IsValidCallback(callback, this.CurrentInstance))
            {
                throw FxTrace.Exception.Argument("callback", SR.InvalidExecutionCallback(callback, this.Activity.ToString()));
            }

            BookmarkOptionsHelper.Validate(options, "options");

            return this.bookmarkManager.CreateBookmark(name, callback, this.CurrentInstance, options);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkScope scope)
        {
            return CreateBookmark(name, callback, scope, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, BookmarkScope scope, BookmarkOptions options)
        {
            ThrowIfDisposed();
            ThrowIfCanInduceIdleNotSet();

            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            if (!CallbackWrapper.IsValidCallback(callback, this.CurrentInstance))
            {
                throw FxTrace.Exception.Argument("callback", SR.InvalidExecutionCallback(callback, this.Activity.ToString()));
            }

            if (scope == null)
            {
                throw FxTrace.Exception.ArgumentNull("scope");
            }

            BookmarkOptionsHelper.Validate(options, "options");

            return this.executor.BookmarkScopeManager.CreateBookmark(name, scope, callback, this.CurrentInstance, options);
        }

        // we don't just do CreateBookmark(BookmarkCallback callback = null, BookmarkOptions options = BookmarkOptions.None) below
        // since there would be overload resolution issues between it and CreateBookmark(string)
        public Bookmark CreateBookmark()
        {
            return CreateBookmark((BookmarkCallback)null);
        }

        public Bookmark CreateBookmark(BookmarkCallback callback)
        {
            return CreateBookmark(callback, BookmarkOptions.None);
        }

        public Bookmark CreateBookmark(BookmarkCallback callback, BookmarkOptions options)
        {
            ThrowIfDisposed();
            ThrowIfCanInduceIdleNotSet();

            if (callback != null && !CallbackWrapper.IsValidCallback(callback, this.CurrentInstance))
            {
                throw FxTrace.Exception.Argument("callback", SR.InvalidExecutionCallback(callback, this.Activity.ToString()));
            }

            BookmarkOptionsHelper.Validate(options, "options");

            return this.bookmarkManager.CreateBookmark(callback, this.CurrentInstance, options);
        }

        internal BookmarkScope CreateBookmarkScope()
        {
            return CreateBookmarkScope(Guid.Empty);
        }

        internal BookmarkScope CreateBookmarkScope(Guid scopeId)
        {
            return this.CreateBookmarkScope(scopeId, null);
        }

        internal BookmarkScope CreateBookmarkScope(Guid scopeId, BookmarkScopeHandle scopeHandle)
        {
            Fx.Assert(!IsDisposed, "This should not be disposed.");

            if (scopeId != Guid.Empty && !this.executor.KeysAllowed)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BookmarkScopesRequireKeys));
            }

            return this.executor.BookmarkScopeManager.CreateAndRegisterScope(scopeId, scopeHandle);
        }

        internal void UnregisterBookmarkScope(BookmarkScope scope)
        {
            Fx.Assert(!IsDisposed, "This should not be disposed.");
            Fx.Assert(scope != null, "The scope should not equal null.");

            this.executor.BookmarkScopeManager.UnregisterScope(scope);
        }

        internal void InitializeBookmarkScope(BookmarkScope scope, Guid id)
        {
            Fx.Assert(scope != null, "The scope should not be null.");
            Fx.Assert(id != Guid.Empty, "The caller should make sure this isn't empty.");

            ThrowIfDisposed();
            if (!this.executor.KeysAllowed)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BookmarkScopesRequireKeys));
            }

            this.executor.BookmarkScopeManager.InitializeScope(scope, id);
        }

        internal void RethrowException(FaultContext context)
        {
            Fx.Assert(!this.IsDisposed, "Must not be disposed.");

            this.executor.RethrowException(this.CurrentInstance, context);
        }

        public void RemoveAllBookmarks()
        {
            ThrowIfDisposed();

            this.CurrentInstance.RemoveAllBookmarks(this.executor.RawBookmarkScopeManager, this.bookmarkManager);
        }

        public void MarkCanceled()
        {
            ThrowIfDisposed();

            if (!this.CurrentInstance.IsCancellationRequested)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MarkCanceledOnlyCallableIfCancelRequested));
            }

            this.CurrentInstance.MarkCanceled();
        }

        public bool RemoveBookmark(string name)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNull("name");
            }

            return RemoveBookmark(new Bookmark(name));
        }

        public bool RemoveBookmark(Bookmark bookmark)
        {
            ThrowIfDisposed();
            if (bookmark == null)
            {
                throw FxTrace.Exception.ArgumentNull("bookmark");
            }
            return this.bookmarkManager.Remove(bookmark, this.CurrentInstance);
        }

        public bool RemoveBookmark(string name, BookmarkScope scope)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            if (scope == null)
            {
                throw FxTrace.Exception.ArgumentNull("scope");
            }

            return this.executor.BookmarkScopeManager.RemoveBookmark(new Bookmark(name), scope, this.CurrentInstance);
        }

        public BookmarkResumptionResult ResumeBookmark(Bookmark bookmark, object value)
        {
            ThrowIfDisposed();
            if (bookmark == null)
            {
                throw FxTrace.Exception.ArgumentNull("bookmark");
            }
            return this.executor.TryResumeUserBookmark(bookmark, value, false);
        }

        internal void RegisterMainRootCompleteCallback(Bookmark bookmark)
        {
            Fx.Assert(!this.IsDisposed, "Shouldn't call this on a disposed object.");
            Fx.Assert(bookmark != null, "Must have a bookmark.");

            this.executor.RegisterMainRootCompleteCallback(bookmark);
        }

        internal ActivityInstance ScheduleSecondaryRoot(Activity activity, LocationEnvironment environment)
        {
            Fx.Assert(!IsDisposed, "Shouldn't call this on a disposed object.");
            Fx.Assert(activity != null, "Activity must not be null.");

            return this.executor.ScheduleSecondaryRootActivity(activity, environment);
        }

        public ActivityInstance ScheduleActivity(Activity activity)
        {
            return ScheduleActivity(activity, null, null);
        }

        public ActivityInstance ScheduleActivity(Activity activity, CompletionCallback onCompleted)
        {
            return ScheduleActivity(activity, onCompleted, null);
        }

        public ActivityInstance ScheduleActivity(Activity activity, FaultCallback onFaulted)
        {
            return ScheduleActivity(activity, null, onFaulted);
        }

        public ActivityInstance ScheduleActivity(Activity activity, CompletionCallback onCompleted, FaultCallback onFaulted)
        {
            ThrowIfDisposed();

            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }
            CompletionBookmark completionBookmark = null;
            FaultBookmark faultBookmark = null;

            if (onCompleted != null)
            {
                if (CallbackWrapper.IsValidCallback(onCompleted, this.CurrentInstance))
                {
                    completionBookmark = ActivityUtilities.CreateCompletionBookmark(onCompleted, this.CurrentInstance);
                }
                else
                {
                    throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, this.Activity.ToString()));
                }
            }

            if (onFaulted != null)
            {
                if (CallbackWrapper.IsValidCallback(onFaulted, this.CurrentInstance))
                {
                    faultBookmark = ActivityUtilities.CreateFaultBookmark(onFaulted, this.CurrentInstance);
                }
                else
                {
                    throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, this.Activity.ToString()));
                }
            }

            return InternalScheduleActivity(activity, completionBookmark, faultBookmark);
        }

        ActivityInstance InternalScheduleActivity(Activity activity, CompletionBookmark onCompleted, FaultBookmark onFaulted)
        {
            ActivityInstance parent = this.CurrentInstance;

            if (!activity.IsMetadataCached || activity.CacheId != parent.Activity.CacheId)
            {
                throw FxTrace.Exception.Argument("activity", SR.ActivityNotPartOfThisTree(activity.DisplayName, parent.Activity.DisplayName));
            }

            if (!activity.CanBeScheduledBy(parent.Activity))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CanOnlyScheduleDirectChildren(parent.Activity.DisplayName, activity.DisplayName, activity.Parent.DisplayName)));
            }

            if (activity.HandlerOf != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DelegateHandlersCannotBeScheduledDirectly(parent.Activity.DisplayName, activity.DisplayName)));
            }

            if (parent.WaitingForTransactionContext)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotScheduleChildrenWhileEnteringIsolation));
            }

            if (parent.IsPerformingDefaultCancelation)
            {
                parent.MarkCanceled();
                return ActivityInstance.CreateCanceledInstance(activity);
            }

            return this.executor.ScheduleActivity(activity, parent, onCompleted,
                onFaulted, null);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction(ActivityAction activityAction, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            return InternalScheduleDelegate(activityAction, ActivityUtilities.EmptyParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T>(ActivityAction<T> activityAction, T argument, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(1)
            {
                { ActivityDelegate.ArgumentName, argument },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2>(ActivityAction<T1, T2> activityAction, T1 argument1, T2 argument2, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(2)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3>(ActivityAction<T1, T2, T3> activityAction, T1 argument1, T2 argument2, T3 argument3, CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(3)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4>(ActivityAction<T1, T2, T3, T4> activityAction, T1 argument1, T2 argument2, T3 argument3, T4 argument4,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(4)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5>(
            ActivityAction<T1, T2, T3, T4, T5> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(5)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6>(
            ActivityAction<T1, T2, T3, T4, T5, T6> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(6)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(7)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(8)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(9)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(10)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(11)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(12)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(13)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
                { ActivityDelegate.Argument13Name, argument13 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(14)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
                { ActivityDelegate.Argument13Name, argument13 },
                { ActivityDelegate.Argument14Name, argument14 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(15)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
                { ActivityDelegate.Argument13Name, argument13 },
                { ActivityDelegate.Argument14Name, argument14 },
                { ActivityDelegate.Argument15Name, argument15 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            ActivityAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> activityAction,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, T16 argument16,
            CompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityAction == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityAction");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(16)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
                { ActivityDelegate.Argument13Name, argument13 },
                { ActivityDelegate.Argument14Name, argument14 },
                { ActivityDelegate.Argument15Name, argument15 },
                { ActivityDelegate.Argument16Name, argument16 },
            };

            return InternalScheduleDelegate(activityAction, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleActivity<TResult>(Activity<TResult> activity, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            return InternalScheduleActivity(activity, ActivityUtilities.CreateCompletionBookmark(onCompleted, parent), ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<TResult>(ActivityFunc<TResult> activityFunc, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            return InternalScheduleDelegate(activityFunc, ActivityUtilities.EmptyParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T, TResult>(ActivityFunc<T, TResult> activityFunc, T argument, CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(1)
            {
               { ActivityDelegate.ArgumentName, argument }
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, TResult>(ActivityFunc<T1, T2, TResult> activityFunc, T1 argument1, T2 argument2,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(2)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, TResult>(ActivityFunc<T1, T2, T3, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(3)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, TResult>(ActivityFunc<T1, T2, T3, T4, TResult> activityFunc, T1 argument1, T2 argument2, T3 argument3, T4 argument4,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(4)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
           Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(5)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(6)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(7)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(8)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(9)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(10)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(11)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(12)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(13)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
                { ActivityDelegate.Argument13Name, argument13 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(14)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
                { ActivityDelegate.Argument13Name, argument13 },
                { ActivityDelegate.Argument14Name, argument14 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(15)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
                { ActivityDelegate.Argument13Name, argument13 },
                { ActivityDelegate.Argument14Name, argument14 },
                { ActivityDelegate.Argument15Name, argument15 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(
            ActivityFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> activityFunc,
            T1 argument1, T2 argument2, T3 argument3, T4 argument4, T5 argument5, T6 argument6, T7 argument7, T8 argument8,
            T9 argument9, T10 argument10, T11 argument11, T12 argument12, T13 argument13, T14 argument14, T15 argument15, T16 argument16,
            CompletionCallback<TResult> onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityFunc == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityFunc");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            Dictionary<string, object> inputParameters = new Dictionary<string, object>(16)
            {
                { ActivityDelegate.Argument1Name, argument1 },
                { ActivityDelegate.Argument2Name, argument2 },
                { ActivityDelegate.Argument3Name, argument3 },
                { ActivityDelegate.Argument4Name, argument4 },
                { ActivityDelegate.Argument5Name, argument5 },
                { ActivityDelegate.Argument6Name, argument6 },
                { ActivityDelegate.Argument7Name, argument7 },
                { ActivityDelegate.Argument8Name, argument8 },
                { ActivityDelegate.Argument9Name, argument9 },
                { ActivityDelegate.Argument10Name, argument10 },
                { ActivityDelegate.Argument11Name, argument11 },
                { ActivityDelegate.Argument12Name, argument12 },
                { ActivityDelegate.Argument13Name, argument13 },
                { ActivityDelegate.Argument14Name, argument14 },
                { ActivityDelegate.Argument15Name, argument15 },
                { ActivityDelegate.Argument16Name, argument16 },
            };

            return InternalScheduleDelegate(activityFunc, inputParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DefaultParametersShouldNotBeUsed, Justification = "Temporary suppression - to be addressed by DCR 127467")]
        public ActivityInstance ScheduleDelegate(ActivityDelegate activityDelegate, IDictionary<string, object> inputParameters,
            DelegateCompletionCallback onCompleted = null, FaultCallback onFaulted = null)
        {
            ThrowIfDisposed();

            ActivityInstance parent = this.CurrentInstance;

            if (activityDelegate == null)
            {
                throw FxTrace.Exception.ArgumentNull("activityDelegate");
            }

            if (onCompleted != null && !CallbackWrapper.IsValidCallback(onCompleted, parent))
            {
                throw FxTrace.Exception.Argument("onCompleted", SR.InvalidExecutionCallback(onCompleted, parent.Activity.ToString()));
            }

            if (onFaulted != null && !CallbackWrapper.IsValidCallback(onFaulted, parent))
            {
                throw FxTrace.Exception.Argument("onFaulted", SR.InvalidExecutionCallback(onFaulted, parent.Activity.ToString()));
            }

            // Check if the inputParameters collection matches the expected inputs for activityDelegate
            IEnumerable<RuntimeDelegateArgument> expectedParameters = activityDelegate.RuntimeDelegateArguments.Where(p => ArgumentDirectionHelper.IsIn(p.Direction));
            int expectedParameterCount = expectedParameters.Count();
            if ((inputParameters == null && expectedParameterCount > 0) ||
                (inputParameters != null && inputParameters.Count != expectedParameterCount))
            {
                throw FxTrace.Exception.Argument("inputParameters", SR.InputParametersCountMismatch(inputParameters == null ? 0 : inputParameters.Count, expectedParameterCount));
            }
            else if (expectedParameterCount > 0)
            {
                foreach (RuntimeDelegateArgument expectedParameter in expectedParameters)
                {
                    object inputParameterValue = null;
                    string parameterName = expectedParameter.Name;
                    if (inputParameters.TryGetValue(parameterName, out inputParameterValue))
                    {
                        if (!TypeHelper.AreTypesCompatible(inputParameterValue, expectedParameter.Type))
                        {
                            throw FxTrace.Exception.Argument("inputParameters", SR.InputParametersTypeMismatch(expectedParameter.Type, parameterName));
                        }
                    }
                    else
                    {
                        throw FxTrace.Exception.Argument("inputParameters", SR.InputParametersMissing(expectedParameter.Name));
                    }
                }
            }

            return InternalScheduleDelegate(activityDelegate, inputParameters ?? ActivityUtilities.EmptyParameters,
                ActivityUtilities.CreateCompletionBookmark(onCompleted, parent),
                ActivityUtilities.CreateFaultBookmark(onFaulted, parent));
        }

        ActivityInstance InternalScheduleDelegate(ActivityDelegate activityDelegate, IDictionary<string, object> inputParameters, CompletionBookmark completionBookmark, FaultBookmark faultBookmark)
        {
            ActivityInstance parent = this.CurrentInstance;

            if (activityDelegate.Handler != null)
            {
                Activity activity = activityDelegate.Handler;

                if (!activity.IsMetadataCached || activity.CacheId != parent.Activity.CacheId)
                {
                    throw FxTrace.Exception.Argument("activity", SR.ActivityNotPartOfThisTree(activity.DisplayName, parent.Activity.DisplayName));
                }
            }

            if (activityDelegate.Owner == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ActivityDelegateOwnerMissing(activityDelegate)));
            }

            if (!activityDelegate.CanBeScheduledBy(parent.Activity))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CanOnlyScheduleDirectChildren(parent.Activity.DisplayName, activityDelegate.DisplayName, activityDelegate.Owner.DisplayName)));
            }

            if (parent.WaitingForTransactionContext)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotScheduleChildrenWhileEnteringIsolation));
            }

            ActivityInstance declaringActivityInstance = this.FindDeclaringActivityInstance(this.CurrentInstance, activityDelegate.Owner);

            if (parent.IsPerformingDefaultCancelation)
            {
                parent.MarkCanceled();
                return ActivityInstance.CreateCanceledInstance(activityDelegate.Handler);
            }

            // Activity delegates execute in the environment of the declaring actvity and not the invoking activity.
            return this.executor.ScheduleDelegate(activityDelegate, inputParameters, parent, declaringActivityInstance.Environment, completionBookmark, faultBookmark);
        }

        internal void EnterNoPersist(NoPersistHandle handle)
        {
            ThrowIfDisposed();

            ExecutionProperties properties = GetExecutionProperties(handle);

            NoPersistProperty property = (NoPersistProperty)properties.FindAtCurrentScope(NoPersistProperty.Name);

            if (property == null)
            {
                property = this.executor.CreateNoPersistProperty();
                properties.Add(NoPersistProperty.Name, property, true, false);
            }

            property.Enter();
        }

        ExecutionProperties GetExecutionProperties(Handle handle)
        {
            Fx.Assert(handle != null, "caller must verify non-null handle");
            if (handle.Owner == this.CurrentInstance)
            {
                return this.Properties;
            }
            else
            {
                if (handle.Owner == null)
                {
                    Fx.Assert(this.executor.RootPropertyManager != null, "should only have a null owner for host-declared properties");
                    // null owner means we have a root property. Use the propertyManager from the ActivityExecutor
                    return new ExecutionProperties(this, null, this.executor.RootPropertyManager);
                }
                else
                {
                    return new ExecutionProperties(this, handle.Owner, handle.Owner.PropertyManager);
                }
            }
        }

        internal void ExitNoPersist(NoPersistHandle handle)
        {
            ThrowIfDisposed();

            ExecutionProperties properties = GetExecutionProperties(handle);

            NoPersistProperty property = (NoPersistProperty)properties.FindAtCurrentScope(NoPersistProperty.Name);

            if (property == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnmatchedNoPersistExit));
            }

            if (property.Exit())
            {
                properties.Remove(NoPersistProperty.Name, true);
            }
        }

        internal void RequestTransactionContext(bool isRequires, RuntimeTransactionHandle handle, Action<NativeActivityTransactionContext, object> callback, object state)
        {
            this.executor.RequestTransactionContext(this.CurrentInstance, isRequires, handle, callback, state);
        }

        internal void CompleteTransaction(RuntimeTransactionHandle handle, BookmarkCallback callback)
        {
            if (callback != null)
            {
                ThrowIfCanInduceIdleNotSet();
            }
            this.executor.CompleteTransaction(handle, callback, this.CurrentInstance);
        }

        internal void RequestPersist(BookmarkCallback onPersistComplete)
        {
            Fx.Assert(!IsDisposed, "We shouldn't call this on a disposed object.");
            Fx.Assert(onPersistComplete != null, "We must have a persist complete callback.");

            Bookmark onPersistBookmark = CreateBookmark(onPersistComplete);
            this.executor.RequestPersist(onPersistBookmark, this.CurrentInstance);
        }

        ActivityInstance FindDeclaringActivityInstance(ActivityInstance startingInstance, Activity activityToMatch)
        {
            Fx.Assert(startingInstance != null, "Starting instance should not be null.");

            ActivityInstance currentActivityInstance = startingInstance;
            while (currentActivityInstance != null)
            {
                if (object.ReferenceEquals(currentActivityInstance.Activity, activityToMatch))
                {
                    return currentActivityInstance;
                }
                else
                {
                    currentActivityInstance = currentActivityInstance.Parent;
                }
            }

            return null;
        }

        void ThrowIfCanInduceIdleNotSet()
        {
            Activity associatedActivity = this.Activity;
            if (!associatedActivity.InternalCanInduceIdle)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CanInduceIdleNotSpecified(associatedActivity.GetType().FullName)));
            }
        }
    }
}
