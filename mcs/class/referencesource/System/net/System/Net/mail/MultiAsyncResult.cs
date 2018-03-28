namespace System.Net.Mime
{
    using System;

    internal class MultiAsyncResult : LazyAsyncResult
    {
        int outstanding;
        object context;

        internal MultiAsyncResult(object context, AsyncCallback callback, object state) : base(context,state,callback)
        {
            this.context = context;
        }

        internal object Context
        {
            get
            {
                return this.context;
            }
        }

        internal void Enter()
        {
            Increment();
        }

        internal void Leave()
        {
            Decrement();
        }

        internal void Leave(object result)
        {
            this.Result = result;
            Decrement();
        }

        void Decrement()
        {
            if (System.Threading.Interlocked.Decrement(ref this.outstanding) == -1)
            {
                base.InvokeCallback(Result);
            }
        }

        void Increment()
        {
            System.Threading.Interlocked.Increment(ref this.outstanding);
        }

        internal void CompleteSequence()
        {
            Decrement();
        }

        internal static object End(IAsyncResult result)
        {
            MultiAsyncResult thisPtr = (MultiAsyncResult)result;
            thisPtr.InternalWaitForCompletion();
            return thisPtr.Result;
        }
    }
}
