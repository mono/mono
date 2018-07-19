//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Threading;

    partial class PeerNodeImplementation
    {
        // A simple state manager for the PeerNode. Unlike the state managers used for channels and other
        // classes, a PeerNode's Open/Close is counted, a PeerNode is re-openable, and Abort only
        // takes effect if the outstanding number of Opens is 1.
        // The PeerNode defers to this object for all state related operations.
        //
        // Whenever a call is made that may change the state of the object (openCount transitions between 0 and 1),
        // an operation is queued. When an operation is removed from the queue, if the target state is still the
        // same as the operation (e.g. openCount > 0 and operation == Open) and the object is not already in that
        // state, the operation is performed by calling back into the PeerNode
        //
        // Because each operation is pulled form the queue one at a time, the open and close of the
        // PeerNode is serialized
        class SimpleStateManager
        {
            internal enum State { NotOpened, Opening, Opened, Closing };
            State currentState = State.NotOpened;
            object thisLock = new object();
            Queue<IOperation> queue = new Queue<IOperation>();
            bool queueRunning;
            int openCount;
            PeerNodeImplementation peerNode;

            public SimpleStateManager(PeerNodeImplementation peerNode)
            {
                this.peerNode = peerNode;
            }

            object ThisLock
            {
                get { return thisLock; }
            }

            public void Abort()
            {
                lock (ThisLock)
                {
                    bool runAbort = false;
                    if (openCount <= 1 && currentState != State.NotOpened)
                    {
                        runAbort = true;
                    }
                    if (openCount > 0)
                    {
                        --openCount;
                    }
                    if (runAbort)
                    {
                        try
                        {
                            peerNode.OnAbort();
                        }
                        finally
                        {
                            currentState = State.NotOpened;
                        }
                    }
                }
            }

            public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                CloseOperation op = null;
                lock (ThisLock)
                {
                    if (openCount > 0)
                    {
                        --openCount;
                    }
                    if (openCount > 0)
                    {
                        return new CompletedAsyncResult(callback, state);
                    }
                    else
                    {
                        op = new CloseOperation(this, peerNode, timeout, callback, state);
                        queue.Enqueue(op);
                        RunQueue();
                    }
                }
                return op;
            }


            public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state, bool waitForOnline)
            {
                bool completedSynchronously = false;
                OpenOperation op = null;
                lock (ThisLock)
                {
                    openCount++;
                    if (openCount > 1 && currentState == State.Opened)
                    {
                        completedSynchronously = true;
                    }
                    else
                    {
                        op = new OpenOperation(this, peerNode, timeout, callback, state, waitForOnline);
                        queue.Enqueue(op);
                        RunQueue();
                    }
                }
                if (completedSynchronously)
                {
                    return new CompletedAsyncResult(callback, state);
                }

                return op;
            }

            public void Close(TimeSpan timeout)
            {
                EndClose(BeginClose(timeout, null, null));
            }

            public static void EndOpen(IAsyncResult result)
            {
                // result can be either an OpenOperation or a CompletedAsyncResult
                if (result is CompletedAsyncResult)
                    CompletedAsyncResult.End(result);
                else
                    OpenOperation.End(result);
            }

            public static void EndClose(IAsyncResult result)
            {
                // result can be either an CloseOperation or a CompletedAsyncResult
                if (result is CompletedAsyncResult)
                    CompletedAsyncResult.End(result);
                else
                    CloseOperation.End(result);
            }

            // Process IP Address change event from IP helper
            public void OnIPAddressesChanged(object sender, EventArgs e)
            {
                IPAddressChangeOperation op = null;
                lock (ThisLock)
                {
                    op = new IPAddressChangeOperation(peerNode);
                    queue.Enqueue(op);
                    RunQueue();
                }
            }

            public void Open(TimeSpan timeout, bool waitForOnline)
            {
                EndOpen(BeginOpen(timeout, null, null, waitForOnline));
            }

            // Start running operations from the queue (must be called within lock)
            void RunQueue()
            {
                if (queueRunning)
                    return;

                queueRunning = true;
                ActionItem.Schedule(new Action<object>(RunQueueCallback), null);
            }

            void RunQueueCallback(object state)
            {
                IOperation op;

                // remove an operation from the queue
                lock (ThisLock)
                {
                    Fx.Assert(queue.Count > 0, "queue should not be empty");
                    op = queue.Dequeue();
                }
                try
                {
                    // execute the operation
                    op.Run();
                }
                finally
                {
                    lock (ThisLock)
                    {
                        // if there are still pending operations, schedule another thread
                        if (queue.Count > 0)
                        {
                            try
                            {
                                ActionItem.Schedule(new Action<object>(RunQueueCallback), null);
                            }
                            catch (Exception e)
                            {
                                if (Fx.IsFatal(e)) throw;
                                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                            }
                        }
                        else
                        {
                            queueRunning = false;
                        }
                    }
                }
            }

            interface IOperation
            {
                void Run();
            }

            class CloseOperation : OperationBase
            {
                PeerNodeImplementation peerNode;

                public CloseOperation(SimpleStateManager stateManager,
                    PeerNodeImplementation peerNode, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(stateManager, timeout, callback, state)
                {
                    this.peerNode = peerNode;
                }

                protected override void Run()
                {
                    Exception lclException = null;
                    try
                    {
                        lock (ThisLock)
                        {
                            if (stateManager.openCount > 0)
                            {
                                // the current target state is no longer Closed
                                invokeOperation = false;
                            }
                            else if (stateManager.currentState == State.NotOpened)
                            {
                                // the state is already Closed
                                invokeOperation = false;
                            }
                            else if (timeoutHelper.RemainingTime() <= TimeSpan.Zero)
                            {
                                // Time out has already happened complete will be taken care of in the 
                                // OperationBase class
                                invokeOperation = false;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                            }
                            else
                            {
                                // the PeerNode needs to be closed
                                if (!(stateManager.currentState != State.Opening && stateManager.currentState != State.Closing))
                                {
                                    throw Fx.AssertAndThrow("Open and close are serialized by queue We should not be either in Closing or Opening state at this point");
                                }
                                if (stateManager.currentState != State.NotOpened)
                                {
                                    stateManager.currentState = State.Closing;
                                    invokeOperation = true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        lclException = e;
                    }

                    if (invokeOperation)
                    {
                        try
                        {
                            peerNode.OnClose(timeoutHelper.RemainingTime());
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e)) throw;
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                            lclException = e;
                        }
                        lock (ThisLock)
                        {
                            stateManager.currentState = State.NotOpened;
                        }
                    }
                    Complete(lclException);
                }
            }

            class OpenOperation : OperationBase
            {
                PeerNodeImplementation peerNode;
                bool waitForOnline;

                public OpenOperation(SimpleStateManager stateManager, PeerNodeImplementation peerNode, TimeSpan timeout,
                    AsyncCallback callback, object state, bool waitForOnline)
                    : base(stateManager, timeout, callback, state)
                {
                    this.peerNode = peerNode;
                    this.waitForOnline = waitForOnline;
                }

                protected override void Run()
                {
                    Exception lclException = null;
                    try
                    {
                        lock (ThisLock)
                        {
                            if (stateManager.openCount < 1)
                            {
                                // the current target state is no longer Opened
                                invokeOperation = false;
                            }
                            else if (stateManager.currentState == State.Opened)
                            {
                                // the state is already Opened
                                invokeOperation = false;
                            }
                            else if (timeoutHelper.RemainingTime() <= TimeSpan.Zero)
                            {
                                // Time out has already happened complete will be taken care of in the 
                                // OperationBase class
                                invokeOperation = false;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                            }
                            else
                            {
                                // the PeerNode needs to be opened
                                if (!(stateManager.currentState != State.Opening && stateManager.currentState != State.Closing))
                                {
                                    throw Fx.AssertAndThrow("Open and close are serialized by queue We should not be either in Closing or Opening state at this point");
                                }
                                if (stateManager.currentState != State.Opened)
                                {
                                    stateManager.currentState = State.Opening;
                                    invokeOperation = true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        lclException = e;
                    }

                    if (invokeOperation)
                    {
                        try
                        {
                            peerNode.OnOpen(timeoutHelper.RemainingTime(), waitForOnline);
                            lock (ThisLock)
                            {
                                stateManager.currentState = State.Opened;
                            }
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e)) throw;
                            lock (ThisLock)
                            {
                                stateManager.currentState = State.NotOpened;
                                // since Open is throwing, we roll back the openCount because a matching Close is not
                                // expected
                                stateManager.openCount--;
                            }
                            lclException = e;
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                    }
                    Complete(lclException);
                }
            }

            // Base class for Open and Cose
            abstract class OperationBase : AsyncResult, IOperation
            {
                protected SimpleStateManager stateManager;
                protected TimeoutHelper timeoutHelper;
                AsyncCallback callback;
                protected bool invokeOperation;
                
                // Double-checked locking pattern requires volatile for read/write synchronization
                volatile bool completed;

                public OperationBase(SimpleStateManager stateManager, TimeSpan timeout,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.stateManager = stateManager;
                    timeoutHelper = new TimeoutHelper(timeout);
                    this.callback = callback;
                    invokeOperation = false;
                    completed = false;
                }

                void AsyncComplete(object o)
                {
                    try
                    {
                        base.Complete(false, (Exception)o);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(SR.GetString(SR.AsyncCallbackException), e);
                    }
                }

                protected abstract void Run();

                void IOperation.Run()
                {
                    Run();
                }

                protected void Complete(Exception exception)
                {
                    if (completed)
                    {
                        return;
                    }
                    lock (ThisLock)
                    {
                        if (completed)
                        {
                            return;
                        }
                        completed = true;
                    }
                    try
                    {
                        if (callback != null)
                        {
                            // complete the AsyncResult on a separate thread so that the queue can progress.
                            // this prevents a deadlock when the callback attempts to call Close.
                            // this may cause the callbacks to be called in a differnet order in which they completed, but that
                            // is ok because each callback is associated with a different object (channel or listener factory)
                            ActionItem.Schedule(new Action<object>(AsyncComplete), exception);
                        }
                        else
                        {
                            AsyncComplete(exception);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(SR.GetString(SR.MessagePropagationException), e);
                    }
                }

                protected object ThisLock
                {
                    get { return stateManager.thisLock; }
                }

                static public void End(IAsyncResult result)
                {
                    AsyncResult.End<OperationBase>(result);
                }
            }

            // To serialize IP address change processing
            class IPAddressChangeOperation : IOperation
            {
                PeerNodeImplementation peerNode;

                public IPAddressChangeOperation(PeerNodeImplementation peerNode)
                {
                    this.peerNode = peerNode;
                }

                void IOperation.Run()
                {
                    peerNode.OnIPAddressChange();
                }
            }
        }
    }
}
