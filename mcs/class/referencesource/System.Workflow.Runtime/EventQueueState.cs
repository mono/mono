//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.     All rights    reserved.
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Workflow.ComponentModel;
using System.Runtime.Serialization;

namespace System.Workflow.Runtime
{
    [Serializable]
    internal sealed class EventQueueState
    {
        Queue deliveredMessages;
        List<ActivityExecutorDelegateInfo<QueueEventArgs>> synchronousListeners;
        List<ActivityExecutorDelegateInfo<QueueEventArgs>> asynchronousListeners;
        bool enabled = true;
        bool transactional = true;

        [NonSerialized]
        internal IComparable queueName;

        [NonSerialized]
        bool dirty = false; // dirty flag set to true until a transaction completes

        internal EventQueueState()
        {
            this.deliveredMessages = new Queue();
            this.synchronousListeners = new List<ActivityExecutorDelegateInfo<QueueEventArgs>>();
            this.asynchronousListeners = new List<ActivityExecutorDelegateInfo<QueueEventArgs>>();
        }

        internal Queue Messages
        {
            get { return this.deliveredMessages; }
        }
        internal List<ActivityExecutorDelegateInfo<QueueEventArgs>> AsynchronousListeners
        {
            get { return this.asynchronousListeners; }
        }
        internal List<ActivityExecutorDelegateInfo<QueueEventArgs>> SynchronousListeners
        {
            get { return this.synchronousListeners; }
        }
        internal bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }
        internal bool Transactional
        {
            get { return this.transactional; }
            set { this.transactional = value; }
        }

        internal bool Dirty
        {
            get { return this.dirty; }
            set { this.dirty = value; }
        }

        internal void CopyFrom(EventQueueState copyFromState)
        {
            this.deliveredMessages = new Queue(copyFromState.Messages);

            // don't copy Subscribers since this gets fixed 
            // up at access time based on these tracking context ints
            this.asynchronousListeners.AddRange(copyFromState.AsynchronousListeners.ToArray());
            this.synchronousListeners.AddRange(copyFromState.SynchronousListeners.ToArray());

            this.enabled = copyFromState.Enabled;
            this.transactional = copyFromState.Transactional;
            this.dirty = false;
        }
    }

}
