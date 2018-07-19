//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Security.Principal;
using System.Runtime.Serialization;

namespace System.Workflow.Activities
{
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class EventDeliveryFailedException : SystemException
    {
        public EventDeliveryFailedException()
        {

        }

        public EventDeliveryFailedException(String message)
            : base(message)
        {

        }

        public EventDeliveryFailedException(String message, Exception innerException)
            : base(message, innerException)
        {

        }

        private EventDeliveryFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    internal sealed class WorkflowMessageEventHandler
    {
        Type proxiedType;
        string eventName;
        [NonSerialized]
        Type eventHandlerType;
        [NonSerialized]
        IDeliverMessage enqueueWrapper;

        internal WorkflowMessageEventHandler(Type proxiedType, EventInfo eventInfo, IDeliverMessage enqueueWrapper)
        {
            this.proxiedType = proxiedType;
            this.eventName = eventInfo.Name;
            this.eventHandlerType = eventInfo.EventHandlerType;
            this.enqueueWrapper = enqueueWrapper;
        }

        internal IDeliverMessage EnqueueWrapper
        {
            get
            {
                return this.enqueueWrapper;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.enqueueWrapper = value;
            }
        }

        internal Delegate Delegate
        {
            get
            {
                MethodInfo interceptedHandler = this.eventHandlerType.GetMethod("Invoke");
                ParameterInfo[] parameters = interceptedHandler.GetParameters();
                bool isValidParameter = false;
                if (parameters.Length == 2)
                {
                    if (parameters[1].ParameterType.IsSubclassOf(typeof(ExternalDataEventArgs))
                        || parameters[1].ParameterType == (typeof(ExternalDataEventArgs)))
                        isValidParameter = true;
                }

                if (isValidParameter)
                {
                    MethodInfo mHandler = typeof(WorkflowMessageEventHandler).GetMethod("EventHandler");
                    return (Delegate)Activator.CreateInstance(eventHandlerType, new object[] { this, mHandler.MethodHandle.GetFunctionPointer() });
                }

                return null;
            }
        }

        public void EventHandler(object sender, ExternalDataEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException("eventArgs");
            }

            try
            {
                object workItem;
                IPendingWork workHandler;
                object[] args = this.enqueueWrapper.PrepareEventArgsArray(sender, eventArgs, out workItem, out workHandler);
                EventQueueName key = GetKey(args);

                String securityIdentifier = null;
                if (eventArgs.Identity == null)
                {
                    IIdentity identity = System.Threading.Thread.CurrentPrincipal.Identity;
                    WindowsIdentity windowsIdentity = identity as WindowsIdentity;
                    if (windowsIdentity != null && windowsIdentity.User != null)
                        securityIdentifier = windowsIdentity.User.Translate(typeof(NTAccount)).ToString();
                    else if (identity != null)
                        securityIdentifier = identity.Name;

                    eventArgs.Identity = securityIdentifier;
                }
                else
                {
                    securityIdentifier = eventArgs.Identity;
                }

                MethodMessage message = new MethodMessage(this.proxiedType, this.eventName, args, securityIdentifier);

                WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "Firing event {0} for instance {1}", this.eventName, eventArgs.InstanceId);

                this.enqueueWrapper.DeliverMessage(eventArgs, key, message, workItem, workHandler);
            }
            catch (Exception e)
            {
                if (ExternalDataExchangeService.IsIrrecoverableException(e))
                {
                    throw;
                }
                else
                {
                    throw new EventDeliveryFailedException(SR.GetString(SR.Error_EventDeliveryFailedException, this.proxiedType, this.eventName, eventArgs.InstanceId), e);
                }
            }
        }

        private EventQueueName GetKey(object[] eventArgs)
        {
            bool provideInitializerTokens = CorrelationResolver.IsInitializingMember(this.proxiedType, this.eventName, eventArgs);

            ICollection<CorrelationProperty> predicates = CorrelationResolver.ResolveCorrelationValues(this.proxiedType, this.eventName, eventArgs, provideInitializerTokens);
            return new EventQueueName(this.proxiedType, this.eventName, predicates);
        }
    }
}
