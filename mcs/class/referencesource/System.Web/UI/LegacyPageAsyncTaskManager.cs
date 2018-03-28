//------------------------------------------------------------------------------
// <copyright file="LegacyPageAsyncTaskManager.cs" company="Microsoft">
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

internal class LegacyPageAsyncTaskManager {
    private Page _page;
    private HttpApplication _app;
    private HttpAsyncResult _asyncResult;
    private bool _failedToStart;
    private ArrayList _tasks;
    private DateTime  _timeoutEnd;
    private volatile bool _timeoutEndReached;
    private volatile bool _inProgress;
    private int _tasksStarted;
    private int _tasksCompleted;
    private WaitCallback _resumeTasksCallback;
    private Timer _timeoutTimer;

    internal LegacyPageAsyncTaskManager(Page page) {
        _page = page;
        _app = page.Context.ApplicationInstance;
        _tasks = new ArrayList();
        _resumeTasksCallback = new WaitCallback(this.ResumeTasksThreadpoolThread);
    }

    internal HttpApplication Application {
        get { return _app; }
    }

    internal void AddTask(LegacyPageAsyncTask task) {
        _tasks.Add(task);
    }

    internal bool AnyTasksRemain {
        get {
            for (int i = 0; i < _tasks.Count; i++) {
                LegacyPageAsyncTask task = (LegacyPageAsyncTask)_tasks[i];
                if (!task.Started) {
                    return true;
                }
            }
            return false;
        }
    }

    internal bool FailedToStartTasks {
        get { return _failedToStart; }
    }

    internal bool TaskExecutionInProgress {
        get { return _inProgress; }
    }

    private Exception AnyTaskError {
        get {
            for (int i = 0; i < _tasks.Count; i++) {
                LegacyPageAsyncTask task = (LegacyPageAsyncTask)_tasks[i];
                if (task.Error != null) {
                    return task.Error;
                }
            }
            return null;
        }
    }

    private bool TimeoutEndReached {
        get {
            if (!_timeoutEndReached && (DateTime.UtcNow >= _timeoutEnd)) {
                _timeoutEndReached = true;
            }

            return _timeoutEndReached;
        }
    }

    private void WaitForAllStartedTasks(bool syncCaller, bool forceTimeout) {
        // don't foreach because the ArrayList could be modified by tasks' end methods
        for (int i = 0; i < _tasks.Count; i++) {
            LegacyPageAsyncTask task = (LegacyPageAsyncTask)_tasks[i];

            if (!task.Started || task.Completed) {
                continue;
            }

            // need to wait, but no longer than timeout.
            if (!forceTimeout && !TimeoutEndReached) {
                DateTime utcNow = DateTime.UtcNow;
               
                if (utcNow < _timeoutEnd) { // re-check not to wait negative time span
                    WaitHandle waitHandle = task.AsyncResult.AsyncWaitHandle;

                    if (waitHandle != null) {
                        bool signaled = waitHandle.WaitOne(_timeoutEnd - utcNow, false);

                        if (signaled && task.Completed) {
                            // a task could complete before timeout expires
                            // in this case go to the next task
                            continue;
                        }
                    }
                }
            }

            // start polling after timeout reached (or if there is no handle to wait on)

            bool taskTimeoutForced = false;

            while (!task.Completed) {
                if (forceTimeout || (!taskTimeoutForced && TimeoutEndReached)) {
                    task.ForceTimeout(syncCaller);
                    taskTimeoutForced = true;
                }
                else {
                    Thread.Sleep(50);
                }
            }
        }
    }

    internal void RegisterHandlersForPagePreRenderCompleteAsync() {
        _page.AddOnPreRenderCompleteAsync(
            new BeginEventHandler(this.BeginExecuteAsyncTasks),
            new EndEventHandler(this.EndExecuteAsyncTasks));
    }

    private IAsyncResult BeginExecuteAsyncTasks(object sender, EventArgs e, AsyncCallback cb, object extraData) {
        return ExecuteTasks(cb, extraData);
    }

    private void EndExecuteAsyncTasks(IAsyncResult ar) {
        _asyncResult.End();
    }

    internal HttpAsyncResult ExecuteTasks(AsyncCallback callback, Object extraData) {
        _failedToStart = false;
        _timeoutEnd = DateTime.UtcNow + _page.AsyncTimeout;
        _timeoutEndReached = false;
        _tasksStarted = 0;
        _tasksCompleted = 0;

        _asyncResult = new HttpAsyncResult(callback, extraData);

        bool waitUntilDone = (callback == null);

        if (waitUntilDone) {
            // when requested to wait for tasks, before starting tasks
            // make sure that the lock can be suspended.
            try {} finally {
                try {
                    // disassociating allows other pending work to take place, and associating will block until that work is complete
                    _app.Context.SyncContext.DisassociateFromCurrentThread();
                    _app.Context.SyncContext.AssociateWithCurrentThread();
                }
                catch (SynchronizationLockException) {
                    _failedToStart = true;
                    throw new InvalidOperationException(SR.GetString(SR.Async_tasks_wrong_thread));
                }
            }
        }

        _inProgress = true;

        try {
            // all work done here:
            ResumeTasks(waitUntilDone, true /*onCallerThread*/);
        }
        finally {
            if (waitUntilDone) {
                _inProgress = false;
            }
        }

        return _asyncResult;
    }

    private void ResumeTasks(bool waitUntilDone, bool onCallerThread) {

#if DBG
        Debug.Trace("Async", "TaskManager.ResumeTasks: onCallerThread=" + onCallerThread + 
            ", _tasksCompleted=" + _tasksCompleted + ", _tasksStarted=" + _tasksStarted);

        if (waitUntilDone) {
            // must be on caller thread to wait
            Debug.Assert(onCallerThread);
        }
#endif

        // artifically increment the task count by one
        // to make sure _tasksCompleted doesn't become equal to _tasksStarted during this method
        Interlocked.Increment(ref _tasksStarted);

        try {
            if (onCallerThread) {
                ResumeTasksPossiblyUnderLock(waitUntilDone);
            }
            else {
                using (_app.Context.SyncContext.AcquireThreadLock()) {
                    ThreadContext threadContext = null;
                    try {
                        threadContext = _app.OnThreadEnter();
                        ResumeTasksPossiblyUnderLock(waitUntilDone);
                    }
                    finally {
                        if (threadContext != null) {
                            threadContext.DisassociateFromCurrentThread();
                        }
                    }
                }
            }
        }
        finally {
            // complete the bogus task introduced with incrementing _tasksStarted at the beginning
            TaskCompleted(onCallerThread);
        }
    }

    private void ResumeTasksPossiblyUnderLock(bool waitUntilDone) {

        while (AnyTasksRemain) {
            bool someTasksStarted = false;
            bool realAsyncTaskStarted = false;
            bool parallelTaskStarted = false;

            // start the tasks

            for (int i = 0; i < _tasks.Count; i++) {
                LegacyPageAsyncTask task = (LegacyPageAsyncTask)_tasks[i];

                if (task.Started) {
                    continue; // ignore already started tasks
                }

                if (parallelTaskStarted && !task.ExecuteInParallel) {
                    // already started a parallel task, so need to ignore sequential ones
                    continue;
                }

                someTasksStarted = true;
                Interlocked.Increment(ref _tasksStarted);

                task.Start(this, _page, EventArgs.Empty);

                if (task.CompletedSynchronously) {
                    continue; // ignore the ones completed synchornously
                }

                // at this point a truly async task has been started
                realAsyncTaskStarted = true;

                if (task.ExecuteInParallel) {
                    parallelTaskStarted = true;
                }
                else {
                    // only one sequential task at a time
                    break;
                }
            }

            if (!someTasksStarted) {
                // no tasks to start, all done
                break;
            }

            if (!TimeoutEndReached && realAsyncTaskStarted && !waitUntilDone) {
                // make sure we have a timer going
                StartTimerIfNeeeded();

                // unwind the stack for async callers
                break;
            }

            // need to wait until tasks comlete, but the wait
            // must be outside of the lock (deadlock otherwise)

            // this code is always already under lock
            bool locked = true;

            try {
                // outer code has lock(_app) { ... }
                // the assumption here is that Disassociate undoes the lock
                try {} finally {
                    _app.Context.SyncContext.DisassociateFromCurrentThread();
                    locked = false;
                }

                WaitForAllStartedTasks(true /*syncCaller*/, false /*forceTimeout*/);
            }
            finally {
                if (!locked) {
                    _app.Context.SyncContext.AssociateWithCurrentThread();
                }
            }
        }
    }

    private void ResumeTasksThreadpoolThread(Object data) {
        ResumeTasks(false /*waitUntilDone*/, false /*onCallerThread*/);
    }

    internal void TaskCompleted(bool onCallerThread) {
        int newTasksCompleted = Interlocked.Increment(ref _tasksCompleted);

        Debug.Trace("Async", "TaskManager.TaskCompleted: onCallerThread=" + onCallerThread + 
            ", _tasksCompleted=" + newTasksCompleted + ", _tasksStarted=" + _tasksStarted);
  
        if (newTasksCompleted < _tasksStarted) {
            // need to wait for more completions
            return;
        }

        // check if any tasks remain not started
        if (!AnyTasksRemain) {
            // can complete the caller - all done
            _inProgress = false;
            _asyncResult.Complete(onCallerThread, null /*result*/, AnyTaskError);
            return;
        }

        // need to resume executing tasks
        if (Thread.CurrentThread.IsThreadPoolThread) {
            // if on thread pool thread, use the current thread
            ResumeTasks(false /*waitUntilDone*/, onCallerThread);
        }
        else {
            // if on a non-threadpool thread, requeue
            ThreadPool.QueueUserWorkItem(_resumeTasksCallback);
        }
    }

    private void StartTimerIfNeeeded() {
        if (_timeoutTimer != null) {
            return;
        }

        // calculate the wait time
        DateTime utcNow = DateTime.UtcNow;

        if (utcNow >= _timeoutEnd) {
            return;
        }

        double timerPeriod = (_timeoutEnd - utcNow).TotalMilliseconds;

        if (timerPeriod >= (double)Int32.MaxValue) {
            // timeout too big to launch timer (> ~25 days, plenty enough for an async page task)
            return;
        }

        // start the timer
        Debug.Trace("Async", "Starting timeout timer for " + timerPeriod + " ms");
        _timeoutTimer = new Timer(new TimerCallback(this.TimeoutTimerCallback), null, (int)timerPeriod, -1);
    }

    internal void DisposeTimer() {
        Timer timer = _timeoutTimer;
        if (timer != null && Interlocked.CompareExchange(ref _timeoutTimer, null, timer) == timer) {
            timer.Dispose();
        }
    }

    private void TimeoutTimerCallback(Object state) {
        DisposeTimer();

        // timeout everything that's left
        WaitForAllStartedTasks(false /*syncCaller*/, false /*forceTimeout*/);
    }

    internal void CompleteAllTasksNow(bool syncCaller) {
        WaitForAllStartedTasks(syncCaller, true /*forceTimeout*/);   
    }
}

}
