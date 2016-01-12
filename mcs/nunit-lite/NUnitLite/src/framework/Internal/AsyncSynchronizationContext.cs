using System;
using System.Collections;
using System.Threading;

namespace NUnit.Framework.Internal
{
    internal class AsyncSynchronizationContext : SynchronizationContext
    {
        private int _operationCount;
        private readonly AsyncOperationQueue _operations = new AsyncOperationQueue();

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new InvalidOperationException("Sending to this synchronization context is not supported");
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _operations.Enqueue(new AsyncOperation(d, state));
        }

        public override void OperationStarted()
        {
            Interlocked.Increment(ref _operationCount);
            base.OperationStarted();
        }

        public override void OperationCompleted()
        {
            if (Interlocked.Decrement(ref _operationCount) == 0)
                _operations.MarkAsComplete();

            base.OperationCompleted();
        }

        public void WaitForPendingOperationsToComplete()
        {
            _operations.InvokeAll();
        }

        private class AsyncOperationQueue
        {
            private bool _run = true;
            private readonly Queue _operations = Queue.Synchronized(new Queue());
            private readonly AutoResetEvent _operationsAvailable = new AutoResetEvent(false);

            public void Enqueue(AsyncOperation asyncOperation)
            {
                _operations.Enqueue(asyncOperation);
                _operationsAvailable.Set();
            }

            public void MarkAsComplete()
            {
                _run = false;
                _operationsAvailable.Set();
            }

            public void InvokeAll()
            {
                while (_run)
                {
                    InvokePendingOperations();
                    _operationsAvailable.WaitOne();
                }

                InvokePendingOperations();
            }

            private void InvokePendingOperations()
            {
                while (_operations.Count > 0)
                {
                    AsyncOperation operation = (AsyncOperation)_operations.Dequeue();
                    operation.Invoke();
                }
            }
        }

        private class AsyncOperation
        {
            private readonly SendOrPostCallback _action;
            private readonly object _state;

            public AsyncOperation(SendOrPostCallback action, object state)
            {
                _action = action;
                _state = state;
            }

            public void Invoke()
            {
                _action(_state);
            }
        }
    }
}