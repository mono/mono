//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation 
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    using System.Runtime;

    // <summary>
    // The ContextItemManager class maintains a set of context items.  A context
    // item represents a piece of transient state in a designer.
    //
    // ContextItems must define an empty constructor.  This empty constructor
    // version of a context item represents its default value, and will be the
    // value returned from GetItem if the context item manager does not contain
    // a context item of the requested type.
    //
    // The ContextItemManager supports context layers.  A context layer is a
    // separation in the set of context items and is useful when providing modal
    // functions.  For example, when switching modes in the designer to show the
    // tab order layout it may be desirable to disable adding items from the
    // toolbox and change the user mouse and keyboard gestures to focus on setting
    // the tab order.  Rather than grabbing and storing context items before
    // replacing them with new values, a developer can simply call CreateLayer.
    // Once the layer is created, all subsequent context changes go to that layer.
    //
    // When the developer is done with the layer, as would be the case when a user
    // switches out of tab order mode, she simply calls Remove on the layer. This
    // removes all context items that were added to the layer and restores the context
    // to its previous set of values before the layer was created.
    // </summary>
    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldHaveCorrectSuffix)]
    public abstract class ContextItemManager : IEnumerable<ContextItem>
    {

        // <summary>
        // Creates a new ContextItemManager object.
        // </summary>
        protected ContextItemManager() 
        {
        }

        // <summary>
        // Returns true if the item manager contains an item of the given type.
        // </summary>
        // <param name="itemType">The type of item to check.</param>
        // <returns>True if the context contains an instance of this item type.</returns>
        // <exception cref="ArgumentNullException">if itemType is null.</exception>
        public abstract bool Contains(Type itemType);

        // <summary>
        // Returns true if the item manager contains an item of the given type.
        // </summary>
        // <typeparam name="TItemType">The type of item to check.</typeparam>
        // <returns>True if the context contains an instance of this item type.</returns>
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.GenericMethodsShouldProvideTypeParameter)]
        public bool Contains<TItemType>() where TItemType : ContextItem
        {
            return Contains(typeof(TItemType));
        }

        // <summary>
        // Enumerates the context items in the editing context.  This enumeration
        // includes prior layers unless the enumerator hits an isolated layer.
        // Enumeration is typically not useful in most scenarios but it is provided so
        // that developers can search in the context and learn what is placed in it.
        // </summary>
        // <returns>An enumeration of context items.</returns>
        public abstract IEnumerator<ContextItem> GetEnumerator();

        // <summary>
        // Returns an instance of the requested item type.  If there is no context
        // item with the given type, an empty item will be created.
        // </summary>
        // <param name="itemType">The type of item to return.</param>
        // <returns>A context item of the requested type.  If there is no item in the context of this type a default one will be created.</returns>
        // <exception cref="ArgumentNullException">if itemType is null.</exception>
        public abstract ContextItem GetValue(Type itemType);

        // <summary>
        // Returns an instance of the requested item type.  If there is no context
        // item with the given type, an empty item will be created.
        // </summary>
        // <typeparam name="TItemType">The type of item to return.</typeparam>
        // <returns>A context item of the requested type.  If there is no item in the context of this type a default one will be created.</returns>
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.GenericMethodsShouldProvideTypeParameter)]
        public TItemType GetValue<TItemType>() where TItemType : ContextItem
        {
            return (TItemType)GetValue(typeof(TItemType));
        }

        // <summary>
        // This is a helper method that invokes the protected OnItemChanged
        // method on ContextItem.
        // </summary>
        // <param name="context">The editing context in use.</param>
        // <param name="item">The new context item.</param>
        // <param name="previousItem">The previous context item.</param>
        // <exception cref="ArgumentNullException">if context, item or previousItem is null.</exception>
        protected static void NotifyItemChanged(EditingContext context, ContextItem item, ContextItem previousItem) 
        {
            if (context == null) 
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            if (item == null) 
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            if (previousItem == null) 
            {
                throw FxTrace.Exception.ArgumentNull("previousItem");
            }
            item.InvokeOnItemChanged(context, previousItem);
        }

        // <summary>
        // This sets a context item to the given value.  It is illegal to pass
        // null here.  If you want to set a context item to its empty value create
        // an instance of the item using a default constructor.
        // </summary>
        // <param name="value">The value to set into the context item manager.</param>
        public abstract void SetValue(ContextItem value);

        // <summary>
        // Adds an event callback that will be invoked with a context item of the given item type changes.
        // </summary>
        // <param name="contextItemType">The type of item you wish to subscribe to.</param>
        // <param name="callback">A callback that will be invoked when contextItemType changes.</param>
        // <exception cref="ArgumentNullException">if contextItemType or callback is null.</exception>
        public abstract void Subscribe(Type contextItemType, SubscribeContextCallback callback);

        // <summary>
        // Adds an event callback that will be invoked with a context item of the given item type changes.
        // </summary>
        // <typeparam name="TContextItemType">The type of item you wish to subscribe to.</typeparam>
        // <param name="callback">A callback that will be invoked when contextItemType changes.</param>
        // <exception cref="ArgumentNullException">if callback is null.</exception>
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.GenericMethodsShouldProvideTypeParameter)]
        public void Subscribe<TContextItemType>(SubscribeContextCallback<TContextItemType> callback) where TContextItemType : ContextItem
        {
            if (callback == null) 
            {
                throw FxTrace.Exception.ArgumentNull("callback");
            }
            SubscribeProxy<TContextItemType> proxy = new SubscribeProxy<TContextItemType>(callback);
            Subscribe(typeof(TContextItemType), proxy.Callback);
        }

        // <summary>
        //     Removes a subscription.
        // </summary>
        // <typeparam name="TContextItemType">The type of context item to remove the callback from.</typeparam>
        // <param name="callback">The callback to remove.</param>
        // <exception cref="ArgumentNullException">if callback is null.</exception>
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.GenericMethodsShouldProvideTypeParameter)]
        public void Unsubscribe<TContextItemType>(SubscribeContextCallback<TContextItemType> callback) where TContextItemType : ContextItem
        {
            if (callback == null) 
            {
                throw FxTrace.Exception.ArgumentNull("callback");
            }
            SubscribeProxy<TContextItemType> proxy = new SubscribeProxy<TContextItemType>(callback);
            Unsubscribe(typeof(TContextItemType), proxy.Callback);
        }

        // <summary>
        //     Removes a subscription.
        // </summary>
        // <param name="contextItemType">The type of context item to remove the callback from.</param>
        // <param name="callback">The callback to remove.</param>
        // <exception cref="ArgumentNullException">if contextItemType or callback is null.</exception>
        public abstract void Unsubscribe(Type contextItemType, SubscribeContextCallback callback);

        // <summary>
        //     This is a helper method that returns the target object for a delegate.
        //     If the delegate was created to proxy a generic delegate, this will correctly
        //     return the original object, not the proxy.
        // </summary>
        // <param name="callback">The callback whose target you want.</param>
        // <exception cref="ArgumentNullException">if callback is null.</exception>
        // <returns>The target object of the callback.</returns>
        protected static object GetTarget(Delegate callback) 
        {
            if (callback == null) 
            {
                throw FxTrace.Exception.ArgumentNull("callback");
            }

            ICallbackProxy proxy = callback.Target as ICallbackProxy;
            if (proxy != null) 
            {
                return proxy.OriginalTarget;
            }

            return callback.Target;
        }

        // <summary>
        //     This is a helper method that performs a Delegate.Remove, but knows
        //     how to unwrap delegates that are proxies to generic callbacks.  Use
        //     this in your Unsubscribe implementations.
        // </summary>
        // <param name="existing">The existing delegate.</param>
        // <param name="toRemove">The delegate to be removed from existing.</param>
        // <returns>The new delegate that should be assigned to existing.</returns>
        protected static Delegate RemoveCallback(Delegate existing, Delegate toRemove) 
        {
            if (existing == null) 
            {
                return null;
            }
            if (toRemove == null) 
            {
                return existing;
            }

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

        // <summary>
        // Implementation of default IEnumerable.
        // </summary>
        IEnumerator IEnumerable.GetEnumerator() 
        {
            return GetEnumerator();
        }

        private interface ICallbackProxy 
        {
            Delegate OriginalDelegate 
            { get; }
            object OriginalTarget 
            { get; }
        }

        // <summary>
        // This is a simple proxy that converts a non-generic subscribe callback to a generic
        // one.
        // </summary>
        // <typeparam name="TContextItemType"></typeparam>
        private class SubscribeProxy<TContextItemType> : ICallbackProxy where TContextItemType : ContextItem 
        {
            private SubscribeContextCallback<TContextItemType> _genericCallback;

            internal SubscribeProxy(SubscribeContextCallback<TContextItemType> callback) 
            {
                _genericCallback = callback;
            }

            internal SubscribeContextCallback Callback 
            {
                get {
                    return new SubscribeContextCallback(SubscribeContext);
                }
            }

            Delegate ICallbackProxy.OriginalDelegate 
            {
                get { return _genericCallback; }
            }

            object ICallbackProxy.OriginalTarget 
            {
                get {
                    return _genericCallback.Target;
                }
            }

            private void SubscribeContext(ContextItem item) 
            {
                if (item == null) 
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }
                _genericCallback((TContextItemType)item);
            }
        }
    }
}
