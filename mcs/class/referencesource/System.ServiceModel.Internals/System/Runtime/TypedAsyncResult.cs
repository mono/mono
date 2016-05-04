//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.Runtime
{
    abstract class TypedAsyncResult<T> : AsyncResult
    {
        T data;

        public TypedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
        }

        public T Data
        {
            get { return data; }
        }

        protected void Complete(T data, bool completedSynchronously)
        {
            this.data = data;
            Complete(completedSynchronously);
        }

        public static T End(IAsyncResult result)
        {
            TypedAsyncResult<T> completedResult = AsyncResult.End<TypedAsyncResult<T>>(result);
            return completedResult.Data;
        }
    }
}
