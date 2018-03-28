// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime
{
    using System;
    using System.Threading;

    class IOThreadCancellationTokenSource : IDisposable
    {
        static readonly Action<object> onCancel = Fx.ThunkCallback<object>(OnCancel);
        readonly TimeSpan timeout;
        CancellationTokenSource source;
        CancellationToken? token;
        IOThreadTimer timer;

        public IOThreadCancellationTokenSource(TimeSpan timeout)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            this.timeout = timeout;
        }

        public IOThreadCancellationTokenSource(int timeout)
            : this(TimeSpan.FromMilliseconds(timeout))
        {
        }

        // NOTE: this property is NOT threadsafe. Potential race condition could be caused if you are accessing it
        // via different threads.
        public CancellationToken Token
        {
            get
            {
                if (this.token == null)
                {
                    if (this.timeout >= TimeoutHelper.MaxWait)
                    {
                        this.token = CancellationToken.None;
                    }
                    else
                    {
                        this.timer = new IOThreadTimer(onCancel, this, true);
                        this.source = new CancellationTokenSource();
                        this.timer.Set(this.timeout);
                        this.token = this.source.Token;
                    }
                }

                return this.token.Value;
            }
        }

        public void Dispose()
        {
            if (this.source != null)
            {
                Fx.Assert(this.timer != null, "timer should not be null.");
                if (this.timer.Cancel())
                {
                    this.source.Dispose();
                    this.source = null;
                }
            }
        }

        static void OnCancel(object obj)
        {
            Fx.Assert(obj != null, "obj should not be null.");
            IOThreadCancellationTokenSource thisPtr = (IOThreadCancellationTokenSource)obj;
            thisPtr.Cancel();
        }

        void Cancel()
        {
            Fx.Assert(this.source != null, "source should not be null.");
            this.source.Cancel();
            this.source.Dispose();
            this.source = null;
        }
    }
}
