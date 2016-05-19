//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Configuration;
using System.Workflow.Runtime.Hosting;
using System.Runtime.Serialization;
using System.Globalization;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;

namespace System.Workflow.Activities
{
    internal interface IDeliverMessage
    {
        object[] PrepareEventArgsArray(object sender, ExternalDataEventArgs eventArgs, out object workItem, out IPendingWork workHandler);
        void DeliverMessage(ExternalDataEventArgs eventArgs, IComparable queueName, object message, object workItem, IPendingWork workHandler);
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ExternalDataExchangeServiceSection : ConfigurationSection
    {
        private const string _services = "Services";

        /// <summary> The providers to be instantiated by the service container. </summary>
        [ConfigurationProperty(_services, DefaultValue = null)]
        public WorkflowRuntimeServiceElementCollection Services
        {
            get
            {
                return (WorkflowRuntimeServiceElementCollection)base[_services];
            }
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ExternalDataExchangeService : WorkflowRuntimeService
    {
        Dictionary<int, WorkflowMessageEventHandler> eventHandlers = null;
        object handlersLock = new object();
        private const string configurationSectionAttributeName = "ConfigurationSection";
        ExternalDataExchangeServiceSection settings = null;
        IDeliverMessage enqueueMessageWrapper = null;
        List<object> services;
        object servicesLock = new object();

        public ExternalDataExchangeService()
        {
            this.eventHandlers = new Dictionary<int, WorkflowMessageEventHandler>();
            this.services = new List<object>();
            this.enqueueMessageWrapper = new EnqueueMessageWrapper(this);
        }

        public ExternalDataExchangeService(string configSectionName)
            : this()
        {
            if (configSectionName == null)
                throw new ArgumentNullException("configSectionName");

            settings = ConfigurationManager.GetSection(configSectionName) as ExternalDataExchangeServiceSection;
            if (settings == null)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                    SR.GetString(SR.Error_ConfigurationSectionNotFound), configSectionName));
        }

        public ExternalDataExchangeService(NameValueCollection parameters)
            : this()
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            string configurationSectionName = null;

            foreach (string key in parameters.Keys)
            {
                if (key.Equals(configurationSectionAttributeName, StringComparison.OrdinalIgnoreCase))
                {
                    configurationSectionName = parameters[key];
                }
                else
                {
                    throw new ArgumentException(
                        String.Format(Thread.CurrentThread.CurrentCulture, SR.GetString(SR.Error_UnknownConfigurationParameter), key), "parameters");
                }
            }
            if (configurationSectionName != null)
            {
                settings = ConfigurationManager.GetSection(configurationSectionName) as ExternalDataExchangeServiceSection;
                if (settings == null)
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                        SR.GetString(SR.Error_ConfigurationSectionNotFound), configurationSectionName));
            }

        }

        public ExternalDataExchangeService(ExternalDataExchangeServiceSection settings)
            : this()
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.settings = settings;
        }

        internal ReadOnlyCollection<object> GetAllServices()
        {
            ReadOnlyCollection<object> collection;
            lock (this.servicesLock)
            {
                collection = this.services.AsReadOnly();
            }

            return collection;
        }

        protected override void Start()
        {
            if (settings != null)
            {
                foreach (WorkflowRuntimeServiceElement service in settings.Services)
                {
                    AddService(ServiceFromSettings(service));
                }
            }

            if (this.Runtime != null)
            {
                base.Start();
            }
        }

        internal void SetEnqueueMessageWrapper(IDeliverMessage wrapper)
        {
            this.enqueueMessageWrapper = wrapper;
            foreach (WorkflowMessageEventHandler eventHandler in this.eventHandlers.Values)
            {
                eventHandler.EnqueueWrapper = wrapper;
            }
        }

        // Todo: This is duplicate of code in WorkflowRuntime
        internal object ServiceFromSettings(WorkflowRuntimeServiceElement serviceSettings)
        {
            object service = null;

            Type t = Type.GetType(serviceSettings.Type, true);

            ConstructorInfo serviceProviderAndSettingsConstructor = null;
            ConstructorInfo serviceProviderConstructor = null;
            ConstructorInfo settingsConstructor = null;

            foreach (ConstructorInfo ci in t.GetConstructors())
            {
                ParameterInfo[] pi = ci.GetParameters();
                if (pi.Length == 1)
                {
                    if (typeof(IServiceProvider).IsAssignableFrom(pi[0].ParameterType))
                    {
                        serviceProviderConstructor = ci;
                    }
                    else if (typeof(NameValueCollection).IsAssignableFrom(pi[0].ParameterType))
                    {
                        settingsConstructor = ci;
                    }
                }
                else if (pi.Length == 2)
                {
                    if (typeof(IServiceProvider).IsAssignableFrom(pi[0].ParameterType)
                        && typeof(NameValueCollection).IsAssignableFrom(pi[1].ParameterType))
                    {
                        serviceProviderAndSettingsConstructor = ci;
                        break;
                    }
                }
            }

            if (serviceProviderAndSettingsConstructor != null)
            {
                service = serviceProviderAndSettingsConstructor.Invoke(
                    new object[] { Runtime, serviceSettings.Parameters });
            }
            else if (serviceProviderConstructor != null)
            {
                service = serviceProviderConstructor.Invoke(new object[] { Runtime });
            }
            else if (settingsConstructor != null)
            {
                service = settingsConstructor.Invoke(new object[] { serviceSettings.Parameters });
            }
            else
            {
                service = Activator.CreateInstance(t);
            }
            return service;
        }

        public virtual void AddService(object service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            InterceptService(service, true);
            if (this.Runtime != null)
            {
                this.Runtime.AddService(service);
            }
            else
            {
                lock (this.servicesLock)
                {
                    this.services.Add(service);
                }
            }
        }

        public virtual void RemoveService(object service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            InterceptService(service, false);
            if (this.Runtime != null)
            {
                this.Runtime.RemoveService(service);
            }
            else
            {
                lock (this.servicesLock)
                {
                    this.services.Remove(service);
                }
            }
        }

        public virtual object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            if (this.Runtime != null)
            {
                return this.Runtime.GetService(serviceType);
            }
            else
            {
                lock (this.servicesLock)
                {
                    foreach (object service in this.services)
                    {
                        if (serviceType.IsAssignableFrom(service.GetType()))
                        {
                            return service;
                        }
                    }

                    return null;
                }
            }
        }

        internal void InterceptService(object service, bool add)
        {
            bool isDataExchangeService = false;
            Type[] interfaceTypes = service.GetType().GetInterfaces();
            foreach (Type type in interfaceTypes)
            {
                object[] attributes = type.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false);
                if (attributes.Length == 0)
                    continue;

                if (this.Runtime != null && this.Runtime.GetService(type) != null && add)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_ExternalDataExchangeServiceExists), type));

                isDataExchangeService = true;
                EventInfo[] events = type.GetEvents();
                if (events == null)
                    continue;

                foreach (EventInfo e in events)
                {
                    WorkflowMessageEventHandler handler = null;
                    int hash = type.GetHashCode() ^ e.Name.GetHashCode();
                    lock (handlersLock)
                    {
                        if (!this.eventHandlers.ContainsKey(hash))
                        {
                            handler = new WorkflowMessageEventHandler(type, e, this.enqueueMessageWrapper);
                            this.eventHandlers.Add(hash, handler);
                        }
                        else
                        {
                            handler = this.eventHandlers[hash];
                        }
                    }
                    AddRemove(service, handler.Delegate, add, e.Name);
                }
            }
            if (!isDataExchangeService)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_ServiceMissingExternalDataExchangeInterface)));
        }

        private void AddRemove(object addedservice, Delegate delg, bool add, string eventName)
        {
            try
            {
                string eventAction;
                if (add)
                    eventAction = "add_" + eventName;
                else
                    eventAction = "remove_" + eventName;

                Type serviceType = addedservice.GetType();
                if (delg != null)
                {
                    // add or remove interception handler
                    object[] del = { delg };
                    serviceType.InvokeMember(eventAction, BindingFlags.InvokeMethod, null, addedservice, del, null);
                }
            }
            catch (Exception e)
            {
                if (IsIrrecoverableException(e))
                {
                    throw;
                }
                // cannot intercept this event
            }
        }

        internal static bool IsIrrecoverableException(Exception e)
        {
            return ((e is OutOfMemoryException) ||
                    (e is StackOverflowException) ||
                    (e is ThreadInterruptedException) ||
                    (e is ThreadAbortException));
        }

        class EnqueueMessageWrapper : IDeliverMessage
        {
            ExternalDataExchangeService eds;

            public EnqueueMessageWrapper(ExternalDataExchangeService eds)
            {
                this.eds = eds;
            }

            public object[] PrepareEventArgsArray(object sender, ExternalDataEventArgs eventArgs, out object workItem, out IPendingWork workHandler)
            {
                // remove the batch items from the event args, only the runtime needs this data
                // and it is not necessarily serializable.
                workItem = eventArgs.WorkItem;
                eventArgs.WorkItem = null;
                workHandler = eventArgs.WorkHandler;
                eventArgs.WorkHandler = null;

                return new object[] { sender, eventArgs };
            }
            public void DeliverMessage(ExternalDataEventArgs eventArgs, IComparable queueName, object message, object workItem, IPendingWork workHandler)
            {
                WorkflowInstance workflowInstance = this.eds.Runtime.GetWorkflow(eventArgs.InstanceId);
                if (eventArgs.WaitForIdle)
                {
                    workflowInstance.EnqueueItemOnIdle(queueName, message, workHandler, workItem);
                }
                else
                {
                    workflowInstance.EnqueueItem(queueName, message, workHandler, workItem);
                }
            }
        }
    }
}
