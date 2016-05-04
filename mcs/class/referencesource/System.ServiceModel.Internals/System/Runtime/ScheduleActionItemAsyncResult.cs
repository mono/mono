//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Threading;

    // An AsyncResult that schedules work for later on the IOThreadScheduler
    abstract class ScheduleActionItemAsyncResult : AsyncResult
    {
        static Action<object> doWork = new Action<object>(DoWork);

        // Implement your own constructor taking in necessary parameters
        // Constructor needs to call "Schedule()" to schedule work 
        // Cache all parameters
        // Implement OnDoWork to do work! 
        
        protected ScheduleActionItemAsyncResult(AsyncCallback callback, object state) : base(callback, state) { } 

        protected void Schedule()
        {
            ActionItem.Schedule(doWork, this);
        }

        static void DoWork(object state)
        {
            ScheduleActionItemAsyncResult thisPtr = (ScheduleActionItemAsyncResult)state;
            Exception completionException = null; 
            try
            {
                thisPtr.OnDoWork(); 
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw; 
                }
                completionException = ex; 
            }

            thisPtr.Complete(false, completionException);
        }

        protected abstract void OnDoWork();

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ScheduleActionItemAsyncResult>(result); 
        }
    }
}
