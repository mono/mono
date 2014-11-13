//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    abstract class IteratorAsyncResult<TIteratorState> : AsyncResult
    {
        TIteratorState iterState;
        TimeoutHelper timeoutHelper;
        IEnumerator<AsyncStep> steps;
        bool completedSynchronously;
        int completedCalled;
        int numPendingSteps;
        bool shouldComplete;
        object thisLock;
        AsyncCallback onStepCompletedCallback;

        protected IteratorAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.onStepCompletedCallback = Fx.ThunkCallback(new AsyncCallback(this.OnStepCompleted));
            this.thisLock = new object();
        }

        protected TimeSpan OriginalTimeout
        {
            get { return this.timeoutHelper.OriginalTimeout; }
        }

        public static AsyncStep CallAsync(BeginCall begin, EndCall end)
        {
            return new AsyncStep(begin, end, false);
        }

        public static AsyncStep CallAsync(BeginCall begin, EndCall end, IAsyncCatch[] catches)
        {
            return new AsyncStep(begin, end, false, catches);
        }

        public static AsyncStep CallParallel(BeginCall begin, EndCall end)
        {
            return new AsyncStep(begin, end, true);
        }

        public static AsyncStep CallParallel(BeginCall begin, EndCall end, IAsyncCatch[] catches)
        {
            return new AsyncStep(begin, end, true, catches);
        }

        protected void Start(TIteratorState iterState, TimeSpan timeout)
        {
            this.iterState = iterState;
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.completedSynchronously = true;
            this.steps = this.GetAsyncSteps();
            this.ExecuteSteps();
        }

        protected TimeSpan RemainingTime()
        {
            return this.timeoutHelper.RemainingTime();
        }

        protected abstract IEnumerator<AsyncStep> GetAsyncSteps();

        protected void CompleteOnce()
        {
            this.CompleteOnce(null);
        }

        protected void CompleteOnce(Exception error)
        {
            if (Interlocked.CompareExchange(ref this.completedCalled, 1, 0) == 0)
            {
                base.Complete(this.completedSynchronously, error);
            }
        }

        void ExecuteSteps()
        {
            IAsyncResult result;
            AsyncStep currentStep;

            while (!this.IsCompleted)
            {
                if (!this.steps.MoveNext())
                {
                    this.CompleteIfNoPendingSteps();
                    break;
                }
                else
                {
                    currentStep = this.steps.Current;
                    result = this.StartStep(currentStep);
                    if (result != null)
                    {
                        if (result.CompletedSynchronously)
                        {
                            this.FinishStep(currentStep, result);
                        }
                        else if (!currentStep.IsParallel)
                        {
                            break;
                        }
                    }
                }
            }
        }

        IAsyncResult StartStep(AsyncStep step)
        {
            IAsyncResult result = null;
            Exception error = null;

            try
            {
                this.OnStepStart();

                result = step.Begin(
                    this.iterState,
                    this.timeoutHelper.RemainingTime(),
                    this.onStepCompletedCallback,
                    step);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                error = e;
            }

            if (error != null)
            {
                this.HandleException(error, step);
            }

            return result;
        }

        void OnStepStart()
        {
            lock (this.thisLock)
            {
                this.numPendingSteps++;
            }
        }

        void OnStepCompletion()
        {
            bool doComplete = false;

            lock (this.thisLock)
            {
                this.numPendingSteps--;
                if ((this.numPendingSteps == 0) && this.shouldComplete)
                {
                    doComplete = true;
                }
            }

            if (doComplete)
            {
                this.CompleteOnce();
            }
        }

        void CompleteIfNoPendingSteps()
        {
            bool doComplete = false;

            lock (this.thisLock)
            {
                if (this.numPendingSteps == 0)
                {
                    doComplete = true;
                }
                else
                {
                    this.shouldComplete = true;
                }
            }

            if (doComplete)
            {
                this.CompleteOnce();
            }
        }

        void OnStepCompleted(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            this.completedSynchronously = false;
            AsyncStep step = (AsyncStep)result.AsyncState;
            this.FinishStep(step, result);

            if (!step.IsParallel)
            {
                this.ExecuteSteps();
            }
        }

        void FinishStep(AsyncStep step, IAsyncResult result)
        {
            Exception error = null;

            try
            {
                step.End(this.iterState, result);             
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                error = e;
            }

            if (error != null)
            {
                this.HandleException(error, step);
            }

            this.OnStepCompletion();
        }

        void HandleException(Exception e, AsyncStep step)
        {
            if (step.Catches != null)
            {
                Exception outException;
                for (int i = 0; i < step.Catches.Length; i++)
                {
                    if (step.Catches[i].HandleException(iterState, e, out outException))
                    {
                        if (outException != null)
                        {
                            this.CompleteOnce(outException);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }

            // The exception wasn't handled
            this.CompleteOnce(e);
        }

        public delegate IAsyncResult BeginCall(
            TIteratorState iterState,
            TimeSpan timeout,
            AsyncCallback asyncCallback,
            object state);

        public delegate void EndCall(TIteratorState iterState, IAsyncResult result);

        public delegate Exception ExceptionHandler<TException>(TIteratorState iterState, TException exception)
            where TException : Exception;

        public class AsyncStep
        {
            public AsyncStep(BeginCall begin, EndCall end, bool isParallel)
            {
                this.Begin = begin;
                this.End = end;
                this.IsParallel = isParallel;
            }

            public AsyncStep(BeginCall begin, EndCall end, bool isParallel, IAsyncCatch[] catches)
                : this(begin, end, isParallel)
            {
                this.Catches = catches;
            }

            public IAsyncCatch[] Catches { get; private set; }

            public BeginCall Begin { get; private set; }

            public EndCall End { get; private set; }

            public bool IsParallel { get; private set; }
        }

        public interface IAsyncCatch
        {
            bool HandleException(TIteratorState iterState, Exception ex, out Exception outEx);
        }

        public class AsyncCatch<TException> : IAsyncCatch
            where TException : Exception
        {
            readonly ExceptionHandler<TException> handler;            

            public AsyncCatch(ExceptionHandler<TException> handler)
            {
                this.handler = handler;                
            }

            public bool HandleException(TIteratorState state, Exception ex, out Exception outEx)
            {
                outEx = null;

                TException casted = ex as TException;
                if (casted != null)
                {
                    outEx = this.handler(state, casted);
                    return true;
                }
                else
                {
                    // The exception wasn't matched, try next handler
                    return false;
                }
            }
        }
    }
}
