#region Using directives

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using System.Globalization;

#endregion

namespace System.Workflow.Activities
{
    internal interface ICorrelationProvider
    {
        ICollection<CorrelationProperty> ResolveCorrelationPropertyValues(Type interfaceType, string memberName, object[] methodArgs, bool provideInitializerTokens);
        bool IsInitializingMember(Type interfaceType, string memberName, object[] methodArgs);
    }

    [AttributeUsageAttribute(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal sealed class CorrelationProviderAttribute : Attribute
    {
        private Type correlationProviderType;

        internal CorrelationProviderAttribute(Type correlationProviderType)
        {
            this.correlationProviderType = correlationProviderType;
        }

        internal Type CorrelationProviderType
        {
            get
            {
                return this.correlationProviderType;
            }
        }
    }

    internal static class CorrelationService
    {
        internal static void Initialize(IServiceProvider context, Activity activity, Type interfaceType, string methodName, Guid instanceId)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            Subscribe(context, activity, interfaceType, methodName, null, instanceId);
            InitializeFollowers(context, interfaceType, methodName);
        }

        internal static bool Subscribe(IServiceProvider context, Activity activity, Type interfaceType, string methodName, IActivityEventListener<QueueEventArgs> eventListener, Guid instanceId)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            WorkflowQueuingService queueService = (WorkflowQueuingService)context.GetService(typeof(WorkflowQueuingService));
            IComparable queueName = ResolveQueueName(activity, interfaceType, methodName);
            if (queueName != null)
            {
                // initializer
                WorkflowQueue queue = null;
                if (queueService.Exists(queueName))
                {
                    queue = queueService.GetWorkflowQueue(queueName);
                    queue.Enabled = true;
                }
                else
                {
                    queue = queueService.CreateWorkflowQueue(queueName, true);
                }

                if (eventListener != null)
                {
                    queue.RegisterForQueueItemAvailable(eventListener, activity.QualifiedName);
                    WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "CorrelationService: activity '{0}' subscribing to QueueItemAvailable", activity.QualifiedName);
                    return true;
                }
                return false;
            }

            SubscribeForCorrelationTokenInvalidation(activity, interfaceType, methodName, eventListener, instanceId);
            return false;
        }

        internal static bool Unsubscribe(IServiceProvider context, Activity activity, Type interfaceType, string methodName, IActivityEventListener<QueueEventArgs> eventListener)
        {
            if (activity == null)
                throw new ArgumentException("activity");
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            WorkflowQueuingService queueService = (WorkflowQueuingService)context.GetService(typeof(WorkflowQueuingService));
            IComparable queueName = ResolveQueueName(activity, interfaceType, methodName);
            if (queueName != null)
            {
                if (queueService.Exists(queueName))
                {
                    queueService.GetWorkflowQueue(queueName).UnregisterForQueueItemAvailable(eventListener);
                    return true;
                }
            }
            return false;
        }

        internal static IComparable ResolveQueueName(Activity activity, Type interfaceType, string methodName)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            // resolver will check for an explicit correlation provider, 
            // if none present this will return an uncorrelated provider.
            // note, an uncorrelated methodName will always be an initializer
            if (CorrelationResolver.IsInitializingMember(interfaceType, methodName, null))
            {
                ICollection<CorrelationProperty> corrvalues = CorrelationResolver.ResolveCorrelationValues(interfaceType, methodName, null, true);
                return new EventQueueName(interfaceType, methodName, corrvalues);
            }

            CorrelationToken reference = GetCorrelationToken(activity);
            if (!reference.Initialized)
                return null;

            return new EventQueueName(interfaceType, methodName, reference.Properties);
        }

        internal static void InvalidateCorrelationToken(Activity activity, Type interfaceType, string methodName, object[] messageArgs)
        {
            object correlationProvider = CorrelationResolver.GetCorrelationProvider(interfaceType);
            if (correlationProvider is NonCorrelatedProvider)
                return;

            CorrelationToken reference = GetCorrelationToken(activity);
            ICollection<CorrelationProperty> correlationvalues = CorrelationResolver.ResolveCorrelationValues(interfaceType, methodName, messageArgs, false);

            if (!CorrelationResolver.IsInitializingMember(interfaceType, methodName, messageArgs))
            {
                if (!reference.Initialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationNotInitialized, reference.Name, activity.QualifiedName));
                ValidateCorrelation(reference.Properties, correlationvalues, reference.Name, activity);
                return;
            }

            // invalidate correlation token if methodName is an initializer
            reference.Initialize(activity, correlationvalues);
        }

        private static CorrelationToken GetCorrelationToken(Activity activity)
        {
            DependencyProperty dependencyProperty = DependencyProperty.FromName("CorrelationToken", activity.GetType());
            if (dependencyProperty == null)
                dependencyProperty = DependencyProperty.FromName("CorrelationToken", activity.GetType().BaseType);
            CorrelationToken reference = activity.GetValue(dependencyProperty) as CorrelationToken;
            if (reference == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationTokenMissing, activity.Name));

            CorrelationToken correlator = CorrelationTokenCollection.GetCorrelationToken(activity, reference.Name, reference.OwnerActivityName);
            if (correlator == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationTokenMissing, activity.Name));

            return correlator;
        }

        private static void ValidateCorrelation(ICollection<CorrelationProperty> initializerProperties, ICollection<CorrelationProperty> followerProperties, string memberName, Activity activity)
        {
            if (followerProperties == null && initializerProperties == null)
                return;

            if (followerProperties == null || initializerProperties == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationViolationException, memberName, activity.QualifiedName));

            if (initializerProperties.Count != followerProperties.Count)
                throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationViolationException, memberName, activity.QualifiedName));

            IEnumerator<CorrelationProperty> initializerValues = initializerProperties.GetEnumerator();
            IEnumerator<CorrelationProperty> followerValues = followerProperties.GetEnumerator();
            while (initializerValues.MoveNext() && followerValues.MoveNext())
            {
                IComparable initializerValue = initializerValues.Current.Value as IComparable;
                object followerValue = followerValues.Current.Value;

                // 






                if (!initializerValues.Current.Name.Equals(followerValues.Current.Name, StringComparison.OrdinalIgnoreCase))
                {
                    CorrelationProperty followerProperty = null;
                    IEnumerator<CorrelationProperty> followerEnumerator = followerProperties.GetEnumerator();
                    while (followerEnumerator.MoveNext())
                    {
                        // We don't need to be concerned with culture here because the names we are comparing
                        // are parameter names on methods in an interface.
                        if (initializerValues.Current.Name.Equals(followerEnumerator.Current.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            // We found a matching Name in the follower collection.
                            // Saving the followerProperty rather than followerEnumerator.Current.Value here
                            // because the latter could be null and still be correct. I need something
                            // that indicates that we actually found a matching CorrelationProperty in the
                            // collection. So instead of having a separate bool, I just have a reference
                            // to the matching CorrelationProperty.
                            followerProperty = followerEnumerator.Current;
                            break;
                        }
                        // If we get here, the name of the parameter doesn't match, so just move to the next element in the 
                        // followerEnumerator.
                    }
                    // If we found a followerProperty with a matching name, use it.
                    // In the highly, possibly impossible, event that we didn't find an element in the
                    // followerProperties collection with a matching name, we fall thru with
                    // followerValue = followerValues.Current.Value, which is exactly what the previous
                    // code had, and we act just like we did before.
                    if (followerProperty != null)
                    {
                        followerValue = followerProperty.Value;
                    }
                }

                if (initializerValue != null && (initializerValue.CompareTo(followerValue) != 0))
                    throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationViolationException, memberName, activity.QualifiedName));
                else if (initializerValues.Current.Value == null && followerValue == null)
                    return;
                else if (initializerValue == null && followerValue != null && !followerValue.Equals(initializerValues.Current.Value))
                    throw new InvalidOperationException(SR.GetString(SR.Error_CorrelationViolationException, memberName, activity.QualifiedName));
            }
        }

        private static void SubscribeForCorrelationTokenInvalidation(Activity activity, Type interfaceType, string followermethodName, IActivityEventListener<QueueEventArgs> eventListener, Guid instanceId)
        {
            CorrelationToken reference = GetCorrelationToken(activity);
            CorrelationTokenInvalidatedHandler dataChangeEventListener = new CorrelationTokenInvalidatedHandler(interfaceType, followermethodName, eventListener, instanceId);
            reference.SubscribeForCorrelationTokenInitializedEvent(activity, dataChangeEventListener);
        }

        private static void InitializeFollowers(IServiceProvider context, Type interfaceType, string followermethodName)
        {
            if (CorrelationResolver.IsInitializingMember(interfaceType, followermethodName, null))
                return;

            EventInfo[] events = interfaceType.GetEvents();
            foreach (EventInfo e in events)
            {
                CreateFollowerEntry(context, interfaceType, followermethodName, e.Name);
            }
        }

        private static void CreateFollowerEntry(IServiceProvider context, Type interfaceType, string followermethodName, string initializermethodName)
        {
            if (!CorrelationResolver.IsInitializingMember(interfaceType, initializermethodName, null))
                return;

            WorkflowQueuingService queueSvcs = (WorkflowQueuingService)context.GetService(typeof(WorkflowQueuingService));
            FollowerQueueCreator follower = new FollowerQueueCreator(followermethodName);
            WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "Creating follower {0} on initializer {1}", interfaceType.Name + followermethodName, interfaceType.Name + initializermethodName);

            ICollection<CorrelationProperty> corrValues = CorrelationResolver.ResolveCorrelationValues(interfaceType, initializermethodName, null, true);
            EventQueueName key = new EventQueueName(interfaceType, initializermethodName, corrValues);
            WorkflowQueue initializerQueue = null;
            if (queueSvcs.Exists(key))
            {
                initializerQueue = queueSvcs.GetWorkflowQueue(key);
            }
            else
            {
                // traversed follower before initializer
                initializerQueue = queueSvcs.CreateWorkflowQueue(key, true);
                initializerQueue.Enabled = false;
            }

            initializerQueue.RegisterForQueueItemArrived(follower);
        }

        internal static void UninitializeFollowers(Type interfaceType, string initializer, WorkflowQueue initializerQueue)
        {
            if (!CorrelationResolver.IsInitializingMember(interfaceType, initializer, null))
                return;

            EventInfo[] events = interfaceType.GetEvents();
            foreach (EventInfo e in events)
            {
                string follower = e.Name;
                if (!CorrelationResolver.IsInitializingMember(interfaceType, e.Name, null))
                    initializerQueue.UnregisterForQueueItemArrived(new FollowerQueueCreator(follower));
            }
        }
    }
}
