
namespace System.Activities.Presentation {

    using System;

    /// <summary>
    /// A delegate that is a callback for service subscriptions.
    /// </summary>
    /// <param name="serviceType">The type of service that has just been published.</param>
    /// <param name="serviceInstance">The instance of the service.</param>
    public delegate void SubscribeServiceCallback(Type serviceType, object serviceInstance);

    /// <summary>
    /// A generic delegate that is a callback for service subscriptions
    /// </summary>
    /// <typeparam name="TServiceType">The type of service to listen to.</typeparam>
    /// <param name="serviceInstance">The instance of the service.</param>
    public delegate void SubscribeServiceCallback<TServiceType>(TServiceType serviceInstance);
}
