//------------------------------------------------------------------------------
// <copyright file="BackgroundWorker.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.ComponentModel 
{
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Threading;
    
    [
        SRDescription(SR.BackgroundWorker_Desc),
        DefaultEvent("DoWork"),
        HostProtection(SharedState = true)
    ]
    public class BackgroundWorker : Component 
    {
        // Private statics
        private static readonly object doWorkKey = new object();
        private static readonly object runWorkerCompletedKey = new object();
        private static readonly object progressChangedKey = new object();

        // Private instance members
        private bool                                canCancelWorker = false;
        private bool                                workerReportsProgress = false;
        private bool                                cancellationPending = false;
        private bool                                isRunning = false;
        private AsyncOperation                      asyncOperation = null;
        private readonly WorkerThreadStartDelegate  threadStart;
        private readonly SendOrPostCallback operationCompleted;
        private readonly SendOrPostCallback progressReporter;

        public BackgroundWorker()
        {
            threadStart        = new WorkerThreadStartDelegate(WorkerThreadStart);
            operationCompleted = new SendOrPostCallback(AsyncOperationCompleted);
            progressReporter   = new SendOrPostCallback(ProgressReporter);
        }

        private void AsyncOperationCompleted(object arg)
        {
            isRunning = false;
            cancellationPending = false;
            OnRunWorkerCompleted((RunWorkerCompletedEventArgs)arg);
        }
        
        [
          Browsable(false), 
          SRDescription(SR.BackgroundWorker_CancellationPending)
        ]
        public bool CancellationPending
        {
            get { return cancellationPending; }
        }

        public void CancelAsync()
        {
            if (!WorkerSupportsCancellation)
            {
                throw new InvalidOperationException(SR.GetString(SR.BackgroundWorker_WorkerDoesntSupportCancellation));
            }

            cancellationPending = true;
        }

        [
          SRCategory(SR.PropertyCategoryAsynchronous),
          SRDescription(SR.BackgroundWorker_DoWork)
        ]
        public event DoWorkEventHandler DoWork
        {
            add
            {
                this.Events.AddHandler(doWorkKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(doWorkKey, value);
            }
        }

        /// <include file='doc\BackgroundWorker.uex' path='docs/doc[@for="BackgroundWorker.IsBusy"]/*' />
        [
          Browsable(false),
          SRDescription(SR.BackgroundWorker_IsBusy)
        ]
        public bool IsBusy
        {
            get
            {
                return isRunning;
            }
        }

        /// <include file='doc\BackgroundWorker.uex' path='docs/doc[@for="BackgroundWorker.OnDoWork"]/*' />
        protected virtual void OnDoWork(DoWorkEventArgs e)
        {
            DoWorkEventHandler handler = (DoWorkEventHandler)(Events[doWorkKey]);
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <include file='doc\BackgroundWorker.uex' path='docs/doc[@for="BackgroundWorker.OnRunWorkerCompleted"]/*' />
        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            RunWorkerCompletedEventHandler handler = (RunWorkerCompletedEventHandler)(Events[runWorkerCompletedKey]);
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChangedEventHandler handler = (ProgressChangedEventHandler)(Events[progressChangedKey]);
            if (handler != null)
            {
                handler(this, e);
            }
        }
        
        [
          SRCategory(SR.PropertyCategoryAsynchronous),
          SRDescription(SR.BackgroundWorker_ProgressChanged)
        ]
        public event ProgressChangedEventHandler ProgressChanged
        {
            add
            {
                this.Events.AddHandler(progressChangedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(progressChangedKey, value);
            }
        }

        // Gets invoked through the AsyncOperation on the proper thread. 
        private void ProgressReporter(object arg)
        {
            OnProgressChanged((ProgressChangedEventArgs)arg);
        }

        // Cause progress update to be posted through current AsyncOperation.
        public void ReportProgress(int percentProgress)
        {
            ReportProgress(percentProgress, null);
        }

        // Cause progress update to be posted through current AsyncOperation.
        public void ReportProgress(int percentProgress, object userState)
        {
            if (!WorkerReportsProgress)
            {
                throw new InvalidOperationException(SR.GetString(SR.BackgroundWorker_WorkerDoesntReportProgress));
            }
            
            ProgressChangedEventArgs args = new ProgressChangedEventArgs(percentProgress, userState);

            if (asyncOperation != null)
            {
                asyncOperation.Post(progressReporter, args);
            }
            else
            {
                progressReporter(args);
            }
        }

        public void RunWorkerAsync()
        {
            RunWorkerAsync(null);
        }

        public void RunWorkerAsync(object argument)
        {
            if (isRunning)
            {
                throw new InvalidOperationException(SR.GetString(SR.BackgroundWorker_WorkerAlreadyRunning));
            }

            isRunning = true;
            cancellationPending = false;
            
            asyncOperation = AsyncOperationManager.CreateOperation(null);
            threadStart.BeginInvoke(argument,
                                    null,
                                    null);
        }

        [
          SRCategory(SR.PropertyCategoryAsynchronous),
          SRDescription(SR.BackgroundWorker_RunWorkerCompleted)
        ]
        public event RunWorkerCompletedEventHandler RunWorkerCompleted
        {
            add
            {
                this.Events.AddHandler(runWorkerCompletedKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(runWorkerCompletedKey, value);
            }
        }

        [ SRCategory(SR.PropertyCategoryAsynchronous),
          SRDescription(SR.BackgroundWorker_WorkerReportsProgress),
          DefaultValue(false)
        ]
        public bool WorkerReportsProgress
        {
            get { return workerReportsProgress; }
            set { workerReportsProgress = value; }
        }

        [ 
          SRCategory(SR.PropertyCategoryAsynchronous),
          SRDescription(SR.BackgroundWorker_WorkerSupportsCancellation),
          DefaultValue(false)
        ]
        public bool WorkerSupportsCancellation
        {
            get { return canCancelWorker; }
            set { canCancelWorker = value; }
        }

        private delegate void WorkerThreadStartDelegate(object argument);

        private void WorkerThreadStart(object argument)
        {
            object workerResult = null;
            Exception error = null;
            bool cancelled = false;
            
            try
            {
                DoWorkEventArgs doWorkArgs = new DoWorkEventArgs(argument);
                OnDoWork(doWorkArgs);
                if (doWorkArgs.Cancel)
                {
                    cancelled = true;
                }
                else
                {
                    workerResult = doWorkArgs.Result;
                }
            }
            catch (Exception exception)
            {
                error = exception;
            }

            RunWorkerCompletedEventArgs e = 
                new RunWorkerCompletedEventArgs(workerResult, error, cancelled); 

            asyncOperation.PostOperationCompleted(operationCompleted, e);
        }

    }
}
