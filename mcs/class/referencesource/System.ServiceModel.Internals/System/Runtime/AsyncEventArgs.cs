// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime
{
    using System.Diagnostics;

    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.NonBlocking, SupportsAsync = true, ReleaseMethod = "Complete")]
    abstract class AsyncEventArgs : IAsyncEventArgs
    {
#if DEBUG
        StackTrace startStack;
        StackTrace completeStack;
#endif
        OperationState state;
        object asyncState;
        AsyncEventArgsCallback callback;
        Exception exception;

        public Exception Exception
        {
            get { return this.exception; }
        }

        public object AsyncState
        {
            get { return this.asyncState; }
        }

        OperationState State
        {
            set
            {
                switch (value)
                {
                    case OperationState.PendingCompletion:
                        if (this.state == OperationState.PendingCompletion)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.AsyncEventArgsCompletionPending(GetType())));
                        }
#if DEBUG
                        if (!Fx.FastDebug)
                        {
                            this.startStack = new StackTrace();
                        }
#endif
                        break;
                    case OperationState.CompletedAsynchronously:
                    case OperationState.CompletedSynchronously:
                        if (this.state != OperationState.PendingCompletion)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.AsyncEventArgsCompletedTwice(GetType())));
                        }
#if DEBUG
                        if (!Fx.FastDebug)
                        {
                            this.completeStack = new StackTrace();
                        }
#endif
                        break;
                }

                this.state = value;
            }
        }

        public void Complete(bool completedSynchronously)
        {
            this.Complete(completedSynchronously, null);
        }

        public virtual void Complete(bool completedSynchronously, Exception exception)
        {
            // The callback will be invoked only if completedSynchronously is false.
            // It is the responsibility of the caller or callback to throw the exception.
            this.exception = exception;
            if (completedSynchronously)
            {
                this.State = OperationState.CompletedSynchronously;
            }
            else
            {
                this.State = OperationState.CompletedAsynchronously;
                this.callback(this);
            }
        }

        protected void SetAsyncState(AsyncEventArgsCallback callback, object state)
        {
            if (callback == null)
            {
                throw Fx.Exception.ArgumentNull("callback");
            }

            this.State = OperationState.PendingCompletion;
            this.asyncState = state;
            this.callback = callback;
        }

        enum OperationState
        {
            Created,
            PendingCompletion,
            CompletedSynchronously,
            CompletedAsynchronously,
        }
    }

    class AsyncEventArgs<TArgument> : AsyncEventArgs
    {
        public TArgument Arguments
        {
            get;
            private set;
        }

        public virtual void Set(AsyncEventArgsCallback callback, TArgument arguments, object state)
        {
            this.SetAsyncState(callback, state);
            this.Arguments = arguments;
        }
    }

    class AsyncEventArgs<TArgument, TResult> : AsyncEventArgs<TArgument>
    {
        public TResult Result { get; set; }
    }
}
