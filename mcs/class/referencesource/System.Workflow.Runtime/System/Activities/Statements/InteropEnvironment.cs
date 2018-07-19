//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Transactions;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Tracking;
    using System.Runtime;
    using System.Globalization;

    class InteropEnvironment : IDisposable, IServiceProvider
    {
        static readonly ReadOnlyCollection<IComparable> emptyList = new ReadOnlyCollection<IComparable>(new IComparable[] { });

        static MethodInfo getServiceMethod = typeof(NativeActivityContext).GetMethod("GetExtension");

        NativeActivityContext nativeActivityContext;

        BookmarkCallback bookmarkCallback;

        bool disposed;
        bool completed;
        bool canceled;

        InteropExecutor executor;
        IEnumerable<IComparable> initialBookmarks;
        Exception uncaughtException;
        Transaction transaction;

        public InteropEnvironment(InteropExecutor interopExecutor, NativeActivityContext nativeActivityContext,
            BookmarkCallback bookmarkCallback, Interop activity, Transaction transaction)
        {
            //setup environment;
            this.executor = interopExecutor;
            this.nativeActivityContext = nativeActivityContext;
            this.Activity = activity;
            this.executor.ServiceProvider = this;
            this.bookmarkCallback = bookmarkCallback;
            this.transaction = transaction;
            OnEnter();
        }

        public Interop Activity { get; set; }

        void IDisposable.Dispose()
        {
            if (!this.disposed)
            {
                OnExit();
                this.disposed = true;
            }
        }

        public void Execute(System.Workflow.ComponentModel.Activity definition, NativeActivityContext context)
        {
            Debug.Assert(!disposed, "Cannot access disposed object");
            try
            {
                this.executor.Initialize(definition, this.Activity.GetInputArgumentValues(context), this.Activity.HasNameCollision);
                ProcessExecutionStatus(this.executor.Execute());
            }
            catch (Exception e)
            {
                this.uncaughtException = e;
                throw;
            }
        }

        public void Cancel()
        {
            Debug.Assert(!disposed, "Cannot access disposed object");
            try
            {
                ProcessExecutionStatus(this.executor.Cancel());
                this.canceled = true;
            }
            catch (Exception e)
            {
                this.uncaughtException = e;
                throw;
            }
        }

        public void EnqueueEvent(IComparable queueName, object item)
        {
            Debug.Assert(!disposed, "Cannot access disposed object");
            try
            {
                ProcessExecutionStatus(this.executor.EnqueueEvent(queueName, item));
            }
            catch (Exception e)
            {
                this.uncaughtException = e;
                throw;
            }
        }

        public void TrackActivityStatusChange(System.Workflow.ComponentModel.Activity activity, int eventCounter)
        {
            this.nativeActivityContext.Track(
                new InteropTrackingRecord(this.Activity.DisplayName,
                    new ActivityTrackingRecord(
                        activity.GetType(),
                        activity.QualifiedName,
                        activity.ContextGuid,
                        activity.Parent == null ? Guid.Empty : activity.Parent.ContextGuid,
                        activity.ExecutionStatus,
                        DateTime.UtcNow,
                        eventCounter,
                        null
                    )
                )
            );
        }

        public void TrackData(System.Workflow.ComponentModel.Activity activity, int eventCounter, string key, object data)
        {
            this.nativeActivityContext.Track(
                new InteropTrackingRecord(this.Activity.DisplayName,
                    new UserTrackingRecord(
                        activity.GetType(),
                        activity.QualifiedName,
                        activity.ContextGuid,
                        activity.Parent == null ? Guid.Empty : activity.Parent.ContextGuid,
                        DateTime.UtcNow,
                        eventCounter,
                        key,
                        data
                    )
                )
            );
        }

        public void Resume()
        {
            Debug.Assert(!disposed, "Cannot access disposed object");
            try
            {
                ProcessExecutionStatus(this.executor.Resume());
            }
            catch (Exception e)
            {
                this.uncaughtException = e;
                throw;
            }
        }

        //
        object IServiceProvider.GetService(Type serviceType)
        {
            Debug.Assert(!disposed, "Cannot access disposed object");
            MethodInfo genericMethodInfo = getServiceMethod.MakeGenericMethod(serviceType);
            return genericMethodInfo.Invoke(this.nativeActivityContext, null);
        }

        public void Persist()
        {
            this.Activity.Persist(this.nativeActivityContext);
        }

        public void CreateTransaction(TransactionOptions transactionOptions)
        {
            this.Activity.CreateTransaction(this.nativeActivityContext, transactionOptions);
        }

        public void CommitTransaction()
        {
            this.Activity.CommitTransaction(this.nativeActivityContext);
        }

        public void AddResourceManager(VolatileResourceManager resourceManager)
        {
            this.Activity.AddResourceManager(this.nativeActivityContext, resourceManager);
        }

        //Called everytime on enter of interopenvironment.
        void OnEnter()
        {
            //Capture Current state of Queues in InteropEnvironment.
            this.initialBookmarks = this.executor.Queues;

            // This method sets up the ambient transaction for the current thread.
            // Since the runtime can execute us on different threads, 
            // we have to set up the transaction scope everytime we enter the interop environment and clear it when we leave the environment.
            this.executor.SetAmbientTransactionAndServiceEnvironment(this.transaction);
        }

        //Called everytime we leave InteropEnvironment.
        void OnExit()
        {
            if (this.uncaughtException != null)
            {
                if (WorkflowExecutor.IsIrrecoverableException(this.uncaughtException))
                {
                    return;
                }
            }

            // This method clears the ambient transaction for the current thread.
            // Since the runtime can execute us on different threads, 
            // we have to set up the transaction scope everytime we enter the interop environment and clear it when we leave the environment.
            this.executor.ClearAmbientTransactionAndServiceEnvironment();

            //Capture Current state of Queues in InteropEnvironment.
            IEnumerable<IComparable> currentBookmarks = this.executor.Queues;

            //Set outparameters when completed or faulted.
            if (this.completed || this.uncaughtException != null)
            {
                this.Activity.OnClose(this.nativeActivityContext, this.uncaughtException);

                this.Activity.SetOutputArgumentValues(
                    this.executor.Outputs, this.nativeActivityContext);

                this.nativeActivityContext.RemoveAllBookmarks();
                this.executor.BookmarkQueueMap.Clear();

                if (this.canceled)
                {
                    this.nativeActivityContext.MarkCanceled();
                }
            }
            else
            {
                //Find Differentials
                IList<IComparable> deletedBookmarks = new List<IComparable>();
                foreach (IComparable value in this.initialBookmarks)
                {
                    deletedBookmarks.Add(value);
                }

                IList<IComparable> newBookmarks = null;
                foreach (IComparable value in currentBookmarks)
                {
                    if (!deletedBookmarks.Remove(value))
                    {
                        if (newBookmarks == null)
                        {
                            newBookmarks = new List<IComparable>();
                        }
                        newBookmarks.Add(value);
                    }
                }

                if (newBookmarks != null)
                {
                    // Create new Queues as Bookmark.
                    foreach (IComparable bookmark in newBookmarks)
                    {
                        //
                        Bookmark v2Bookmark = this.nativeActivityContext.CreateBookmark(bookmark.ToString(),
                            this.bookmarkCallback, BookmarkOptions.MultipleResume);
                        this.executor.BookmarkQueueMap.Add(v2Bookmark, bookmark);
                    }
                }

                // Delete removed queues.    
                foreach (IComparable bookmark in deletedBookmarks)
                {
                    this.nativeActivityContext.RemoveBookmark(bookmark.ToString());
                    List<Bookmark> bookmarksToRemove = new List<Bookmark>();
                    foreach (KeyValuePair<Bookmark, IComparable> entry in this.executor.BookmarkQueueMap)
                    {
                        if (entry.Value == bookmark)
                        {
                            bookmarksToRemove.Add(entry.Key);
                        }
                    }

                    foreach (Bookmark bookmarkToRemove in bookmarksToRemove)
                    {
                        this.executor.BookmarkQueueMap.Remove(bookmarkToRemove);
                    }
                }
            }
        }

        void ProcessExecutionStatus(System.Workflow.ComponentModel.ActivityExecutionStatus executionStatus)
        {
            this.completed = (executionStatus == System.Workflow.ComponentModel.ActivityExecutionStatus.Closed);
        }

        public static class ParameterHelper
        {
            static readonly Type activityType = typeof(System.Workflow.ComponentModel.Activity);
            static readonly Type compositeActivityType = typeof(System.Workflow.ComponentModel.CompositeActivity);
            static readonly Type dependencyObjectType = typeof(System.Workflow.ComponentModel.DependencyObject);
            static readonly Type activityConditionType = typeof(System.Workflow.ComponentModel.ActivityCondition);
            // Interop Property Names
            internal const string interopPropertyActivityType = "ActivityType";
            internal const string interopPropertyActivityProperties = "ActivityProperties";
            internal const string interopPropertyActivityMetaProperties = "ActivityMetaProperties";
            // Allowed Meta-Properties
            internal const string activityNameMetaProperty = "Name";


            //Check property names for any Property/PropertyOut pairs that would conflict with our naming scheme 
            public static bool HasPropertyNameCollision(IList<PropertyInfo> properties)
            {
                bool hasNameCollision = false;
                HashSet<string> propertyNames = new HashSet<string>();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    propertyNames.Add(propertyInfo.Name);
                }

                if (propertyNames.Contains(interopPropertyActivityType) ||
                    propertyNames.Contains(interopPropertyActivityProperties) ||
                    propertyNames.Contains(interopPropertyActivityMetaProperties))
                {
                    hasNameCollision = true;
                }
                else
                {
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        if (propertyNames.Contains(propertyInfo.Name + Interop.OutArgumentSuffix))
                        {
                            hasNameCollision = true;
                            break;
                        }
                    }
                }
                return hasNameCollision;
            }

            public static bool IsBindable(PropertyInfo propertyInfo)
            {
                bool isMetaProperty;
                if (!IsBindableOrMetaProperty(propertyInfo, out isMetaProperty))
                {
                    return false;
                }
                return !isMetaProperty;
            }

            public static bool IsBindableOrMetaProperty(PropertyInfo propertyInfo, out bool isMetaProperty)
            {
                isMetaProperty = false;

                // Validate the declaring type: CompositeActivity and DependencyObject
                if (propertyInfo.DeclaringType.Equals(compositeActivityType) ||
                    propertyInfo.DeclaringType.Equals(dependencyObjectType))
                {
                    return false;
                }

                // Validate the declaring type: Activity
                // We allow certain meta-properties on System.Workflow.ComponentModel.Activity to be visible
                if (propertyInfo.DeclaringType.Equals(activityType) &&
                    !String.Equals(propertyInfo.Name, activityNameMetaProperty, StringComparison.Ordinal))
                {
                    return false;
                }

                //Validate the data type
                if (activityConditionType.IsAssignableFrom(propertyInfo.PropertyType))
                {
                    return false;
                }

                //Validate whether there is DP(Meta) backup
                string dependencyPropertyName = propertyInfo.Name;

                System.Workflow.ComponentModel.DependencyProperty dependencyProperty = System.Workflow.ComponentModel.DependencyProperty.FromName(dependencyPropertyName,
                    propertyInfo.DeclaringType);

                if (dependencyProperty != null && dependencyProperty.DefaultMetadata.IsMetaProperty)
                {
                    isMetaProperty = true;
                }
                return true;
            }
        }
    }
}
