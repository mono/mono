//------------------------------------------------------------------------------
// <copyright file="SubscriptionQueue.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Collections.Generic;
    using System.Web;

    // Similar to a Queue<T>, but allows unsubscribing from the underlying queue.
    //
    // !! WARNING !!
    // Mutable struct for performance reasons; optimized for case where Enqueue is never called.
    // Be careful with usage, e.g. no readonly declarations of this type.
    //
    // Type is not thread safe.

    internal struct SubscriptionQueue<T> {

        private LinkedList<T> _list;

        public bool IsEmpty {
            get { return (_list == null || _list.Count == 0); }
        }

        public ISubscriptionToken Enqueue(T value) {
            if (_list == null) {
                // lazily instantiate the list
                _list = new LinkedList<T>();
            }

            LinkedListNode<T> node = _list.AddLast(value);
            return new SubscriptionToken(node);
        }

        public void FireAndComplete(Action<T> action) {
            try {
                T value;
                // Use a while loop instead of a foreach since the list might be changing
                while (TryDequeue(out value)) {
                    action(value);
                }
            }
            finally {
                _list = null;
            }
        }

        private bool TryDequeue(out T result) {
            if (_list != null && _list.First != null) {
                LinkedListNode<T> theNode = _list.First;
                _list.RemoveFirst(); // also marks the SubscriptionToken as inactive
                result = theNode.Value;
                theNode.Value = default(T); // unroot the value in case it's large
                return true;
            }
            else {
                result = default(T); // unroot the value in case it's large
                return false;
            }
        }

        private sealed class SubscriptionToken : ISubscriptionToken {
            private readonly LinkedListNode<T> _node;

            public SubscriptionToken(LinkedListNode<T> node) {
                _node = node;
            }

            public bool IsActive {
                get { return (_node.List != null); }
            }

            public void Unsubscribe() {
                if (IsActive) {
                    _node.List.Remove(_node);
                    _node.Value = default(T); // unroot the value in case it's large
                }
            }
        }

    }
}
