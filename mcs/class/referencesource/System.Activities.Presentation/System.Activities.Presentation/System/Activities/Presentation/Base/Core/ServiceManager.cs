namespace System.Activities.Presentation
{

    using System.Activities.Presentation.Internal.Properties;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Activities.Presentation;

    /// <summary>
    /// The service manager implements IServiceProvider and provides access 
    /// to services offered by the editing context. 
    /// </summary>
    /// Suppressing FxCop from complaining about our use of naming, since it has been approved
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class ServiceManager : IServiceProvider, IEnumerable<Type>
    {

        /// <summary>
        /// Creates a new ServiceManager.
        /// </summary>
        protected ServiceManager() { }

        /// <summary>
        /// Returns true if the service manager contains a service of the given type.
        /// </summary>
        /// <param name="serviceType">The type of service to check.</param>
        /// <returns>True if a service of type serviceType has been published.</returns>
        /// <exception cref="ArgumentNullException">if serviceType is null.</exception>
        public abstract bool Contains(Type serviceType);

        /// <summary>
        /// Returns true if the service manager contains a service of the given type.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to check.</typeparam>
        /// <returns>True if a service of type TServiceType has been published.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public bool Contains<TServiceType>()
        {
            return Contains(typeof(TServiceType));
        }

        /// <summary>
        /// Retrives the requested service.  Unlike GetService, GetRequiredService 
        /// throws a NotSupportedException if the service isn’t available.  The reason 
        /// we provide this method, and not a normal GetService method is our wish to 
        /// move services to a more reliable contract.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to retrieve.</typeparam>
        /// <returns>An instance of the service.  This never returns null.</returns>
        /// <exception cref="NotSupportedException">if there is no service of the given type.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public TServiceType GetRequiredService<TServiceType>()
        {
            TServiceType service = GetService<TServiceType>();
            if (service == null)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(
                    string.Format(CultureInfo.CurrentCulture,
                    Resources.Error_RequiredService, typeof(TServiceType).FullName)));
            }
            return service;
        }

        /// <summary>
        /// Retrives the requested service.  This method returns null if the service could not be located.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to retrieve.</typeparam>
        /// <returns>An instance of the service, or null if the service has not been published.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public TServiceType GetService<TServiceType>()
        {
            object service = GetService(typeof(TServiceType));
            return (TServiceType)service;
        }

        /// <summary>
        /// Retrives the requested service.  This method returns null if the service could not be located.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve.</param>
        /// <returns>An instance of the service, or null if the service has not been published.</returns>
        /// <exception cref="ArgumentNullException">If serviceType is null.</exception>
        public abstract object GetService(Type serviceType);

        /// <summary>
        /// Retrives an enumerator that can be used to enumerate all of the services that this 
        /// service manager publishes.
        /// </summary>
        /// <returns>An enumeration of published services.</returns>
        public abstract IEnumerator<Type> GetEnumerator();

        /// <summary>
        /// Calls back on the provided callback when someone has published the requested service.  
        /// If the service was already available, this method invokes the callback immediately.
        /// 
        /// A generic version of this method is provided for convenience, and calls the non-generic 
        /// method with appropriate casts.
        /// </summary>
        /// <param name="serviceType">The type of service to subscribe to.</param>
        /// <param name="callback">A callback that will be notified when the service is available.</param>
        /// <exception cref="ArgumentNullException">If serviceType or callback is null.</exception>
        public abstract void Subscribe(Type serviceType, SubscribeServiceCallback callback);

        /// <summary>
        /// Calls back on the provided callback when someone has published the requested service.  
        /// If the service was already available, this method invokes the callback immediately.
        /// 
        /// A generic version of this method is provided for convenience, and calls the non-generic 
        /// method with appropriate casts.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to subscribe.</typeparam>
        /// <param name="callback">A callback that will be invoked when the service is available.</param>
        /// <exception cref="ArgumentNullException">If callback is null.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Subscribe<TServiceType>(SubscribeServiceCallback<TServiceType> callback)
        {
            if (callback == null) throw FxTrace.Exception.ArgumentNull("callback");

            // Call the standard Subscribe method and use a generic proxy
            SubscribeProxy<TServiceType> proxy = new SubscribeProxy<TServiceType>(callback);
            Subscribe(typeof(TServiceType), proxy.Callback);
        }

        /// <summary>
        /// Publishes the given service type, but does not declare an instance yet.  When someone
        /// requests the service the PublishServiceCallback will be invoked to create the instance.
        /// The callback is only invoked once and after that the instance it returned is cached.
        /// 
        /// A generic version of this method is provided for convenience, and calls the non-generic 
        /// method with appropriate casts.
        /// </summary>
        /// <param name="serviceType">The type of service to publish.</param>
        /// <param name="callback">A callback that will be invoked when an instance of the service is needed.</param>
        /// <exception cref="ArgumentNullException">If serviceType or callback is null.</exception>
        /// <exception cref="ArgumentException">If serviceType has already been published.</exception>
        public abstract void Publish(Type serviceType, PublishServiceCallback callback);

        /// <summary>
        /// Publishes the given service.  Once published, the service instance remains in the
        /// service manager until the editing context is disposed.
        /// </summary>
        /// <param name="serviceType">The type of service to publish.</param>
        /// <param name="serviceInstance">An instance of the service.</param>
        /// <exception cref="ArgumentNullException">If serviceType or serviceInstance is null.</exception>
        /// <exception cref="ArgumentException">If serviceInstance does not derive from or implement serviceType, or if serviceType has already been published.</exception>
        public abstract void Publish(Type serviceType, object serviceInstance);

        /// <summary>
        /// Publishes the given service type, but does not declare an instance yet.  When someone
        /// requests the service the PublishServiceCallback will be invoked to create the instance.
        /// The callback is only invoked once and after that the instance it returned is cached.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to publish.</typeparam>
        /// <param name="callback">A callback to be invoked when the service is required.</param>
        /// <exception cref="ArgumentNullException">If callback is null.</exception>
        /// <exception cref="ArgumentException">If TServiceType has already been published.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Publish<TServiceType>(PublishServiceCallback<TServiceType> callback)
        {
            if (callback == null) throw FxTrace.Exception.ArgumentNull("callback");

            // Call the standard Subscribe method and use a generic proxy
            PublishProxy<TServiceType> proxy = new PublishProxy<TServiceType>(callback);
            Publish(typeof(TServiceType), proxy.Callback);
        }

        /// <summary>
        /// Publishes the given service.  Once published, the service instance remains in the
        /// service manager until the editing context is disposed.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to publish.</typeparam>
        /// <param name="serviceInstance">The instance of the service to publish.</param>
        /// <exception cref="ArgumentException">If TServiceType has already been published.</exception>
        public void Publish<TServiceType>(TServiceType serviceInstance)
        {
            if (serviceInstance == null) throw FxTrace.Exception.ArgumentNull("serviceInstance");
            Publish(typeof(TServiceType), serviceInstance);
        }

        /// <summary>
        /// Removes a subscription for the TServiceType.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to remove the subscription from.</typeparam>
        /// <param name="callback">The callback object to remove from the subscription.</param>
        /// <exception cref="ArgumentNullException">If callback is null.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void Unsubscribe<TServiceType>(SubscribeServiceCallback<TServiceType> callback)
        {
            if (callback == null) throw FxTrace.Exception.ArgumentNull("callback");
            SubscribeProxy<TServiceType> proxy = new SubscribeProxy<TServiceType>(callback);
            Unsubscribe(typeof(TServiceType), proxy.Callback);
        }

        /// <summary>
        /// Removes a subscription for the serviceType.
        /// </summary>
        /// <param name="serviceType">The type of service to remove the subscription from.</param>
        /// <param name="callback">The callback object to remove from the subscription.</param>
        /// <exception cref="ArgumentNullException">If serviceType or callback is null.</exception>
        public abstract void Unsubscribe(Type serviceType, SubscribeServiceCallback callback);

        /// <summary>
        ///     This is a helper method that returns the target object for a delegate.
        ///     If the delegate was created to proxy a generic delegate, this will correctly
        ///     return the original object, not the proxy.
        /// </summary>
        /// <param name="callback">The delegate to get the target for.</param>
        /// <returns>The object that is the callback target.  This can return null if the callback represents a static object.</returns>
        /// <exception cref="ArgumentNullException">If callback is null.</exception>
        protected static object GetTarget(Delegate callback)
        {
            if (callback == null) throw FxTrace.Exception.ArgumentNull("callback");
            ICallbackProxy proxy = callback.Target as ICallbackProxy;
            if (proxy != null)
            {
                return proxy.OriginalTarget;
            }

            return callback.Target;
        }

        /// <summary>
        ///     This is a helper method that performs a Delegate.Remove, but knows
        ///     how to unwrap delegates that are proxies to generic callbacks.  Use
        ///     this in your Unsubscribe implementations.
        /// </summary>
        /// <param name="existing">The existing delegate to remove the callback from.</param>
        /// <param name="toRemove">The callback to remove.</param>
        /// <returns>A new value to assign to the existing delegate.</returns>
        protected static Delegate RemoveCallback(Delegate existing, Delegate toRemove)
        {
            if (existing == null) return null;
            if (toRemove == null) return existing;

            ICallbackProxy toRemoveProxy = toRemove.Target as ICallbackProxy;
            if (toRemoveProxy == null)
            {
                // The item to be removed is a normal delegate.  Just call
                // Delegate.Remove
                return Delegate.Remove(existing, toRemove);
            }

            toRemove = toRemoveProxy.OriginalDelegate;

            Delegate[] invocationList = existing.GetInvocationList();
            bool removedItems = false;

            for (int idx = 0; idx < invocationList.Length; idx++)
            {
                Delegate item = invocationList[idx];
                ICallbackProxy itemProxy = item.Target as ICallbackProxy;
                if (itemProxy != null)
                {
                    item = itemProxy.OriginalDelegate;
                }

                if (item.Equals(toRemove))
                {
                    invocationList[idx] = null;
                    removedItems = true;
                }
            }

            if (removedItems)
            {
                // We must create a new delegate containing the 
                // invocation list that is is left
                existing = null;
                foreach (Delegate d in invocationList)
                {
                    if (d != null)
                    {
                        if (existing == null)
                        {
                            existing = d;
                        }
                        else
                        {
                            existing = Delegate.Combine(existing, d);
                        }
                    }
                }
            }

            return existing;
        }

        /// <summary>
        /// Implementation of default IEnumerable.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// This is a simple proxy that converts a non-generic publish callback to a generic
        /// one.
        /// </summary>
        /// <typeparam name="TServiceType"></typeparam>
        private class PublishProxy<TServiceType>
        {
            private PublishServiceCallback<TServiceType> _genericCallback;

            internal PublishProxy(PublishServiceCallback<TServiceType> callback)
            {
                _genericCallback = callback;
            }

            internal PublishServiceCallback Callback
            {
                get
                {
                    return new PublishServiceCallback(PublishService);
                }
            }

            private object PublishService(Type serviceType)
            {

                if (serviceType == null) throw FxTrace.Exception.ArgumentNull("serviceType");

                if (serviceType != typeof(TServiceType))
                {
                    // This is an invalid publisher
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_IncorrectServiceType,
                        typeof(ServiceManager).FullName,
                        typeof(TServiceType).FullName,
                        serviceType.FullName)));
                }

                object service = _genericCallback();

                if (service == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(
                        string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_NullService,
                        _genericCallback.Method.DeclaringType.FullName,
                        serviceType.FullName)));
                }

                if (!serviceType.IsInstanceOfType(service))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(
                        string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_IncorrectServiceType,
                        _genericCallback.Method.DeclaringType.FullName,
                        serviceType.FullName,
                        service.GetType().FullName)));
                }

                return service;
            }
        }

        /// <summary>
        /// This is a simple proxy that converts a non-generic subscribe callback to a generic
        /// one.
        /// </summary>
        /// <typeparam name="TServiceType"></typeparam>
        private class SubscribeProxy<TServiceType> : ICallbackProxy
        {
            private SubscribeServiceCallback<TServiceType> _genericCallback;

            internal SubscribeProxy(SubscribeServiceCallback<TServiceType> callback)
            {
                _genericCallback = callback;
            }

            internal SubscribeServiceCallback Callback
            {
                get
                {
                    return new SubscribeServiceCallback(SubscribeService);
                }
            }


            private void SubscribeService(Type serviceType, object service)
            {

                if (serviceType == null) throw FxTrace.Exception.ArgumentNull("serviceType");
                if (service == null) throw FxTrace.Exception.ArgumentNull("service");

                if (!typeof(TServiceType).IsInstanceOfType(service))
                {
                    // This is an invalid subscriber
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Error_IncorrectServiceType,
                        typeof(TServiceType).FullName,
                        serviceType.FullName)));
                }

                _genericCallback((TServiceType)service);
            }

            Delegate ICallbackProxy.OriginalDelegate
            {
                get { return _genericCallback; }
            }

            object ICallbackProxy.OriginalTarget
            {
                get
                {
                    return _genericCallback.Target;
                }
            }
        }

        private interface ICallbackProxy
        {
            Delegate OriginalDelegate { get; }
            object OriginalTarget { get; }
        }
    }
}
