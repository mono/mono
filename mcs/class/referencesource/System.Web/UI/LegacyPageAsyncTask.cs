//------------------------------------------------------------------------------
// <copyright file="LegacyPageAsyncTask.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.Util;

// Represents an asynchronous task that uses the old asynchronous patterns and the legacy synchronization systems

internal sealed class LegacyPageAsyncTask {
    private BeginEventHandler   _beginHandler;
    private EndEventHandler     _endHandler;
    private EndEventHandler     _timeoutHandler;
    private Object              _state;
    private bool                _executeInParallel;

    private LegacyPageAsyncTaskManager _taskManager;
    private int                 _completionMethodLock;
    private bool                _started;
    private bool                _completed;
    private bool                _completedSynchronously;
    private AsyncCallback       _completionCallback;
    private IAsyncResult        _asyncResult;
    private Exception           _error;

    internal LegacyPageAsyncTask(BeginEventHandler beginHandler, EndEventHandler endHandler, EndEventHandler timeoutHandler, Object state, bool executeInParallel) {
        // Parameter checking is done by the public PageAsyncTask constructor

        _beginHandler = beginHandler;
        _endHandler = endHandler;
        _timeoutHandler = timeoutHandler;
        _state = state;
        _executeInParallel = executeInParallel;
    }

    public BeginEventHandler BeginHandler {
        get { return _beginHandler; }
    }

    public EndEventHandler EndHandler {
        get { return _endHandler; }
    }

    public EndEventHandler TimeoutHandler {
        get { return _timeoutHandler; }
    }

    public Object State {
        get { return _state; }
    }

    public bool ExecuteInParallel {
        get { return _executeInParallel; }
    }

    internal bool Started {
        get { return _started; }
    }

    internal bool CompletedSynchronously {
        get { return _completedSynchronously; }
    }

    internal bool Completed {
        get { return _completed; }
    }

    internal IAsyncResult AsyncResult {
        get { return _asyncResult; }
    }

    internal Exception Error {
        get { return _error; }
    }

    internal void Start(LegacyPageAsyncTaskManager manager, Object source, EventArgs args) {
        Debug.Assert(!_started);

        _taskManager = manager;
        _completionCallback = new AsyncCallback(this.OnAsyncTaskCompletion);
        _started = true;

        Debug.Trace("Async", "Start task");

        try {
            IAsyncResult ar = _beginHandler(source, args, _completionCallback, _state);

            if (ar == null) {
                throw new InvalidOperationException(SR.GetString(SR.Async_null_asyncresult));
            }

            if (_asyncResult == null) {
                // _asyncResult could be not null if already completed
                _asyncResult = ar;
            }
        }
        catch (Exception e) {
            Debug.Trace("Async", "Task failed to start");

            _error = e;
            _completed = true;
            _completedSynchronously = true;
            _taskManager.TaskCompleted(true /*onCallerThread*/); // notify TaskManager
            // it is ok to say false (onCallerThread) above because this kind of
            // error completion will never be the last in ResumeTasks()
        }
    }

    private void OnAsyncTaskCompletion(IAsyncResult ar) {
        Debug.Trace("Async", "Task completed, CompletedSynchronously=" + ar.CompletedSynchronously);

        if (_asyncResult == null) {
            // _asyncResult could be null if the code not yet returned from begin method
            _asyncResult = ar;
        }

        CompleteTask(false /*timedOut*/);
    }

    internal void ForceTimeout(bool syncCaller) {
        Debug.Trace("Async", "Task timed out");
        CompleteTask(true /*timedOut*/, syncCaller /*syncTimeoutCaller*/);
    }

    private void CompleteTask(bool timedOut) {
        CompleteTask(timedOut, false /*syncTimeoutCaller*/);
    }

    private void CompleteTask(bool timedOut, bool syncTimeoutCaller) {
        if (Interlocked.Exchange(ref _completionMethodLock, 1) != 0) {
            return;
        }

        bool needSetupThreadContext;
        bool responseEnded = false;

        if (timedOut) {
            needSetupThreadContext = !syncTimeoutCaller;
        }
        else {
            _completedSynchronously = _asyncResult.CompletedSynchronously;
            needSetupThreadContext = !_completedSynchronously;
        }

        // call the completion or timeout handler
        //  when neeeded setup the thread context and lock
        //  catch and remember all exceptions

        HttpApplication app = _taskManager.Application;

        try {
            if (needSetupThreadContext) {

                using (app.Context.SyncContext.AcquireThreadLock()) {
                    ThreadContext threadContext = null;
                    try {
                        threadContext = app.OnThreadEnter();
                        if (timedOut) {
                            if (_timeoutHandler != null) {
                                _timeoutHandler(_asyncResult);
                            }
                        }
                        else {
                            _endHandler(_asyncResult);
                        }
                    }
                    finally {
                        if (threadContext != null) {
                            threadContext.DisassociateFromCurrentThread();
                        }
                    }
                }
            }
            else {
                if (timedOut) {
                    if (_timeoutHandler != null) {
                        _timeoutHandler(_asyncResult);
                    }
                }
                else {
                    _endHandler(_asyncResult);
                }
            }
        }
        catch (ThreadAbortException e) {
           _error = e;

           HttpApplication.CancelModuleException exceptionState = e.ExceptionState as HttpApplication.CancelModuleException;

           // Is this from Response.End()
           if (exceptionState != null && !exceptionState.Timeout) {
               // Mark the request as completed
               using (app.Context.SyncContext.AcquireThreadLock()) {
                   // Handle response end once. Skip if already initiated (previous AsyncTask)
                   if (!app.IsRequestCompleted) {   
                       responseEnded = true;
                       app.CompleteRequest();
                   }
               }

               // Clear the error for Response.End
               _error = null;
           }

           // ---- the exception. Async completion required (DDB 140655)
           Thread.ResetAbort();
        } 
        catch (Exception e) {
            _error = e;
        }


        // Complete the current async task
        _completed = true;
        _taskManager.TaskCompleted(_completedSynchronously /*onCallerThread*/); // notify TaskManager

        // Wait for pending AsyncTasks (DDB 140655)
        if (responseEnded) {
            _taskManager.CompleteAllTasksNow(false /*syncCaller*/);
        } 
    }
}

}
