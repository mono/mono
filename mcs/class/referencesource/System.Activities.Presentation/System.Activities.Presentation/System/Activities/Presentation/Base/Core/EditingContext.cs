//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation 
{
    using System.Activities.Presentation.Internal.Properties;
    using System;
    using System.Runtime;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Activities.Presentation;

    // <summary>
    // The EditingContext class contains contextual state about a designer.  This includes permanent
    // state such as list of services running in the designer.
    // It also includes transient state consisting of context items.  Examples of transient
    // context item state include the set of currently selected objects as well as the editing tool
    // being used to manipulate objects on the design surface.
    //
    // The editing context is designed to be a concrete class for ease of use.  It does have a protected
    // API that can be used to replace its implementation.
    // </summary>
    public class EditingContext : IDisposable 
    {

        private ContextItemManager _contextItems;
        private ServiceManager _services;

        // <summary>
        // Creates a new editing context.
        // </summary>
        public EditingContext() 
        {
        }

       
        // <summary>
        // The Disposing event gets fired just before the context gets disposed.
        // </summary>
        public event EventHandler Disposing;

        // <summary>
        // Returns the local collection of context items offered by this editing context.
        // </summary>
        // <value></value>
        public ContextItemManager Items 
        {
            get {
                if (_contextItems == null) 
                {
                    _contextItems = CreateContextItemManager();
                    if (_contextItems == null) 
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(
                            string.Format(CultureInfo.CurrentCulture, Resources.Error_NullImplementation, "CreateContextItemManager")));
                    }
                }

                return _contextItems;
            }
        }

        // <summary>
        // Returns the service manager for this editing context.
        // </summary>
        // <value></value>
        public ServiceManager Services 
        {
            get {
                if (_services == null) 
                {
                    _services = CreateServiceManager();
                    if (_services == null) 
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(
                            string.Format(CultureInfo.CurrentCulture, Resources.Error_NullImplementation, "CreateServiceManager")));
                    }
                }

                return _services;
            }
        }

        // <summary>
        // Creates an instance of the context item manager to be returned from
        // the ContextItems property.  The default implementation creates a
        // ContextItemManager that supports delayed activation of design editor
        // managers through the declaration of a SubscribeContext attribute on
        // the design editor manager.
        // </summary>
        // <returns>Returns an implementation of the ContextItemManager class.</returns>
        protected virtual ContextItemManager CreateContextItemManager() 
        {
            return new DefaultContextItemManager(this);
        }

        // <summary>
        // Creates an instance of the service manager to be returned from the
        // Services property. The default implementation creates a ServiceManager
        // that supports delayed activation of design editor managers through the
        // declaration of a SubscribeService attribute on the design editor manager.
        // </summary>
        // <returns>Returns an implemetation of the ServiceManager class.</returns>
        protected virtual ServiceManager CreateServiceManager() 
        {
            return new DefaultServiceManager();
        }

        // <summary>
        // Disposes this editing context.
        // </summary>
        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // <summary>
        // Disposes this editing context.
        // <param name="disposing">True if this object is being disposed, or false if it is finalizing.</param>
        // </summary>
        protected virtual void Dispose(bool disposing) 
        {
            if (disposing) 
            {
                // Let any interested parties know the context is being disposed
                if (Disposing != null)
                {
                    Disposing(this, EventArgs.Empty);
                }

                IDisposable d = _services as IDisposable;
                if (d != null) 
                {
                    d.Dispose();
                }

                d = _contextItems as IDisposable;
                if (d != null) 
                {
                    d.Dispose();
                }
            }
        }

        // <summary>
        // This is the default context item manager for our editing context.
        // </summary>
        private sealed class DefaultContextItemManager : ContextItemManager 
        {
            private EditingContext _context;
            private DefaultContextLayer _currentLayer;
            private Dictionary<Type, SubscribeContextCallback> _subscriptions;

            internal DefaultContextItemManager(EditingContext context) 
            {
                _context = context;
                _currentLayer = new DefaultContextLayer(null);
            }

            // <summary>
            // This changes a context item to the given value.  It is illegal to pass
            // null here.  If you want to set a context item to its empty value create
            // an instance of the item using a default constructor.
            // </summary>
            // <param name="value"></param>
            public override void SetValue(ContextItem value) 
            {
                if (value == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                // The rule for change is that we store the new value,
                // raise a change on the item, and then raise a change
                // to everyone else.  If changing the item fails, we recover
                // the previous item.
                ContextItem existing, existingRawValue;
                existing = existingRawValue = GetValueNull(value.ItemType);

                if (existing == null) 
                {
                    existing = GetValue(value.ItemType);
                }

                bool success = false;

                try 
                {
                    _currentLayer.Items[value.ItemType] = value;
                    NotifyItemChanged(_context, value, existing);
                    success = true;
                }
                finally 
                {
                    if (success) 
                    {
                        OnItemChanged(value);
                    }
                    else 
                    {
                        // The item threw during its transition to 
                        // becoming active.  Put the old one back.
                        // We must put the old one back by re-activating
                        // it.  This could throw a second time, so we
                        // cover this case by removing the value first.
                        // Should it throw again, we won't recurse because
                        // the existing raw value would be null.

                        _currentLayer.Items.Remove(value.ItemType);
                        if (existingRawValue != null) 
                        {
                            SetValue(existingRawValue);
                        }
                    }
                }
            }

            // <summary>
            // Returns true if the item manager contains an item of the given type.
            // This only looks in the current layer.
            // </summary>
            // <param name="itemType"></param>
            // <returns></returns>
            public override bool Contains(Type itemType) 
            {
                if (itemType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("itemType");
                }
                if (!typeof(ContextItem).IsAssignableFrom(itemType)) 
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "itemType", typeof(ContextItem).FullName)));
                }

                return _currentLayer.Items.ContainsKey(itemType);
            }

            // <summary>
            // Returns an instance of the requested item type.  If there is no context
            // item with the given type, an empty item will be created.
            // </summary>
            // <param name="itemType"></param>
            // <returns></returns>
            public override ContextItem GetValue(Type itemType) 
            {

                ContextItem item = GetValueNull(itemType);

                if (item == null) 
                {

                    // Check the default item table and add a new
                    // instance there if we need to
                    if (!_currentLayer.DefaultItems.TryGetValue(itemType, out item)) 
                    {
                        item = (ContextItem)Activator.CreateInstance(itemType);

                        // Verify that the resulting item has the correct item type
                        // If it doesn't, it means that the user provided a derived
                        // item type
                        if (item.ItemType != itemType) 
                        {
                            throw FxTrace.Exception.AsError(new ArgumentException(string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Error_DerivedContextItem,
                                itemType.FullName,
                                item.ItemType.FullName)));
                        }

                        // Now push the item in the context so we have
                        // a consistent reference
                        _currentLayer.DefaultItems.Add(item.ItemType, item);
                    }
                }

                return item;
            }

            // <summary>
            // Similar to GetValue, but returns NULL if the item isn't found instead of
            // creating an empty item.
            // </summary>
            // <param name="itemType"></param>
            // <returns></returns>
            private ContextItem GetValueNull(Type itemType) 
            {

                if (itemType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("itemType");
                }
                if (!typeof(ContextItem).IsAssignableFrom(itemType)) 
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "itemType", typeof(ContextItem).FullName)));
                }

                ContextItem item = null;
                DefaultContextLayer layer = _currentLayer;
                while (layer != null && !layer.Items.TryGetValue(itemType, out item)) 
                {
                    layer = layer.ParentLayer;
                }

                return item;
            }

            // <summary>
            // Enumerates the context items in the editing context.  This enumeration
            // includes prior layers unless the enumerator hits an isolated layer.
            // Enumeration is typically not useful in most scenarios but it is provided so
            // that developers can search in the context and learn what is placed in it.
            // </summary>
            // <returns></returns>
            public override IEnumerator<ContextItem> GetEnumerator() 
            {
                return _currentLayer.Items.Values.GetEnumerator();
            }

            // <summary>
            // Called when an item changes value.  This happens in one of two ways:
            // either the user has called Change, or the user has removed a layer.
            // </summary>
            // <param name="item"></param>
            private void OnItemChanged(ContextItem item) 
            {
                SubscribeContextCallback callback;

                Fx.Assert(item != null, "You cannot pass a null item here.");

                if (_subscriptions != null && _subscriptions.TryGetValue(item.ItemType, out callback)) 
                {
                    callback(item);
                }
            }

            // <summary>
            // Adds an event callback that will be invoked with a context item of the given item type changes.
            // </summary>
            // <param name="contextItemType"></param>
            // <param name="callback"></param>
            public override void Subscribe(Type contextItemType, SubscribeContextCallback callback) 
            {
                if (contextItemType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("contextItemType");
                }
                if (callback == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("callback");
                }
                if (!typeof(ContextItem).IsAssignableFrom(contextItemType)) 
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "contextItemType", typeof(ContextItem).FullName)));
                }

                if (_subscriptions == null) 
                {
                    _subscriptions = new Dictionary<Type, SubscribeContextCallback>();
                }

                SubscribeContextCallback existing = null;

                _subscriptions.TryGetValue(contextItemType, out existing);

                existing = (SubscribeContextCallback)Delegate.Combine(existing, callback);
                _subscriptions[contextItemType] = existing;

                // If the context is already present, invoke the callback.
                ContextItem item = GetValueNull(contextItemType);

                if (item != null) 
                {
                    callback(item);
                }
            }

            // <summary>
            //     Removes a subscription.
            // </summary>
            public override void Unsubscribe(Type contextItemType, SubscribeContextCallback callback) 
            {

                if (contextItemType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("contextItemType");
                }
                if (callback == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("callback");
                }
                if (!typeof(ContextItem).IsAssignableFrom(contextItemType)) 
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_ArgIncorrectType,
                        "contextItemType", typeof(ContextItem).FullName)));
                }
                if (_subscriptions != null) 
                {
                    SubscribeContextCallback existing;
                    if (_subscriptions.TryGetValue(contextItemType, out existing)) 
                    {
                        existing = (SubscribeContextCallback)RemoveCallback(existing, callback);
                        if (existing == null) 
                        {
                            _subscriptions.Remove(contextItemType);
                        }
                        else 
                        {
                            _subscriptions[contextItemType] = existing;
                        }
                    }
                }
            }

            // <summary>
            // This context layer contains our context items.
            // </summary>
            private class DefaultContextLayer 
            {
                private DefaultContextLayer _parentLayer;
                private Dictionary<Type, ContextItem> _items;
                private Dictionary<Type, ContextItem> _defaultItems;

                internal DefaultContextLayer(DefaultContextLayer parentLayer) 
                {
                    _parentLayer = parentLayer; // can be null
                }

                internal Dictionary<Type, ContextItem> DefaultItems 
                {
                    get {
                        if (_defaultItems == null) 
                        {
                            _defaultItems = new Dictionary<Type, ContextItem>();
                        }
                        return _defaultItems;
                    }
                }

                internal Dictionary<Type, ContextItem> Items 
                {
                    get {
                        if (_items == null) 
                        {
                            _items = new Dictionary<Type, ContextItem>();
                        }
                        return _items;
                    }
                }

                internal DefaultContextLayer ParentLayer 
                {
                    get { return _parentLayer; }
                }
            }
        }

        // <summary>
        // This is the default service manager for our editing context.
        // </summary>
        private sealed class DefaultServiceManager : ServiceManager, IDisposable 
        {
            private static readonly object _recursionSentinel = new object();

            private Dictionary<Type, object> _services;
            private Dictionary<Type, SubscribeServiceCallback> _subscriptions;

            internal DefaultServiceManager() 
            {
            }

            // <summary>
            // Returns true if the service manager contains a service of the given type.
            // </summary>
            // <param name="serviceType"></param>
            // <returns></returns>
            public override bool Contains(Type serviceType) 
            {
                if (serviceType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("serviceType");
                }
                return (_services != null && _services.ContainsKey(serviceType));
            }

            // <summary>
            // Retrieves the requested service.  This method returns null if the service could not be located.
            // </summary>
            // <param name="serviceType"></param>
            // <returns></returns>
            public override object GetService(Type serviceType)
            {
                object result = this.GetPublishedService(serviceType);
                if (result == null)
                {
                    if (this.Contains(typeof(IServiceProvider)))
                    {
                        result = this.GetRequiredService<IServiceProvider>().GetService(serviceType);
                        if (result != null)
                        {
                            this.Publish(serviceType, result);
                        }
                    }
                }
                return result;
            }

            object GetPublishedService(Type serviceType)
            {
                object service = null;

                if (serviceType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("serviceType");
                }

                if (_services != null && _services.TryGetValue(serviceType, out service)) 
                {

                    // If this service is our recursion sentinel, it means that someone is recursing
                    // while resolving a service callback.  Throw to break out of the recursion
                    // cycle.
                    if (service == _recursionSentinel) 
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Error_RecursionResolvingService, serviceType.FullName)));
                    }

                    // See if this service is a callback.  If it is, invoke it and store
                    // the resulting service back in the dictionary.
                    PublishServiceCallback callback = service as PublishServiceCallback;
                    if (callback != null) 
                    {

                        // Store a recursion sentinel in the dictionary so we can easily
                        // tell if someone is recursing
                        _services[serviceType] = _recursionSentinel;
                        try 
                        {
                            service = callback(serviceType);
                            if (service == null) 
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(
                                    string.Format(CultureInfo.CurrentCulture,
                                    Resources.Error_NullService,
                                    callback.Method.DeclaringType.FullName,
                                    serviceType.FullName)));
                            }

                            if (!serviceType.IsInstanceOfType(service)) 
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(
                                    string.Format(CultureInfo.CurrentCulture,
                                    Resources.Error_IncorrectServiceType,
                                    callback.Method.DeclaringType.FullName,
                                    serviceType.FullName,
                                    service.GetType().FullName)));
                            }
                        }
                        finally 
                        {
                            // Note, this puts the callback back in place if it threw.
                            _services[serviceType] = service;
                        }
                    }
                }

                // If the service is not found locally, do not walk up the parent chain.  
                // This was a major source of unreliability with the component model
                // design.  For a service to be accessible from the editing context, it
                // must be added.

                return service;
            }

            // <summary>
            // Retrieves an enumerator that can be used to enumerate all of the services that this
            // service manager publishes.
            // </summary>
            // <returns></returns>
            public override IEnumerator<Type> GetEnumerator() 
            {
                if (_services == null) 
                {
                    _services = new Dictionary<Type, object>();
                }

                return _services.Keys.GetEnumerator();
            }

            // <summary>
            // Calls back on the provided callback when someone has published the requested service.
            // If the service was already available, this method invokes the callback immediately.
            //
            // A generic version of this method is provided for convience, and calls the non-generic
            // method with appropriate casts.
            // </summary>
            // <param name="serviceType"></param>
            // <param name="callback"></param>
            public override void Subscribe(Type serviceType, SubscribeServiceCallback callback) 
            {
                if (serviceType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("serviceType");
                }
                if (callback == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("callback");
                }

                object service = GetService(serviceType);
                if (service != null) 
                {

                    // If the service is already available, callback immediately
                    callback(serviceType, service);
                }
                else 
                {

                    // Otherwise, store this for later
                    if (_subscriptions == null) 
                    {
                        _subscriptions = new Dictionary<Type, SubscribeServiceCallback>();
                    }
                    SubscribeServiceCallback existing = null;
                    _subscriptions.TryGetValue(serviceType, out existing);
                    existing = (SubscribeServiceCallback)Delegate.Combine(existing, callback);
                    _subscriptions[serviceType] = existing;
                }
            }

            // <summary>
            // Calls back on the provided callback when someone has published the requested service.
            // If the service was already available, this method invokes the callback immediately.
            //
            // A generic version of this method is provided for convience, and calls the non-generic
            // method with appropriate casts.
            // </summary>
            // <param name="serviceType"></param>
            // <param name="callback"></param>
            public override void Publish(Type serviceType, PublishServiceCallback callback) 
            {
                if (serviceType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("serviceType");
                }
                if (callback == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("callback");
                }

                Publish(serviceType, (object)callback);
            }

            // <summary>
            //     If you already have an instance to a service, you can publish it here.
            // </summary>
            // <param name="serviceType"></param>
            // <param name="serviceInstance"></param>
            public override void Publish(Type serviceType, object serviceInstance) 
            {
                if (serviceType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("serviceType");
                }
                if (serviceInstance == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("serviceInstance");
                }

                if (!(serviceInstance is PublishServiceCallback) && !serviceType.IsInstanceOfType(serviceInstance)) 
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Error_IncorrectServiceType,
                        typeof(ServiceManager).Name,
                        serviceType.FullName,
                        serviceInstance.GetType().FullName)));
                }

                if (_services == null) 
                {
                    _services = new Dictionary<Type, object>();
                }

                try 
                {
                    _services.Add(serviceType, serviceInstance);
                }
                catch (ArgumentException e) 
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_DuplicateService, serviceType.FullName), e));
                }

                // Now see if there were any subscriptions that required this service
                SubscribeServiceCallback subscribeCallback;
                if (_subscriptions != null && _subscriptions.TryGetValue(serviceType, out subscribeCallback)) 
                {
                    subscribeCallback(serviceType, GetService(serviceType));
                    _subscriptions.Remove(serviceType);
                }
            }

            // <summary>
            //     Removes a subscription.
            // </summary>
            public override void Unsubscribe(Type serviceType, SubscribeServiceCallback callback) 
            {

                if (serviceType == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("serviceType");
                }
                if (callback == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("callback");
                }

                if (_subscriptions != null) 
                {
                    SubscribeServiceCallback existing;
                    if (_subscriptions.TryGetValue(serviceType, out existing)) 
                    {
                        existing = (SubscribeServiceCallback)RemoveCallback(existing, callback);
                        if (existing == null) 
                        {
                            _subscriptions.Remove(serviceType);
                        }
                        else 
                        {
                            _subscriptions[serviceType] = existing;
                        }
                    }
                }
            }

            // <summary>
            // We implement IDisposable so that the editing context can destroy us when it
            // shuts down.
            // </summary>
            void IDisposable.Dispose() 
            {
                if (_services != null) 
                {
                    Dictionary<Type, object> services = _services;

                    try 
                    {
                        foreach (object value in services.Values) 
                        {
                            IDisposable d = value as IDisposable;
                            if (d != null) 
                            {
                                d.Dispose();
                            }
                        }
                    }
                    finally 
                    {
                        _services = null;
                    }
                }
            }
        }
    }
}
