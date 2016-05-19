using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Workflow.Runtime
{
    internal class DbRetry
    {
        private const short _defaultMaxRetries = 20;
        private const int _defaultRetrySleep = 2000;
        private const short _spinCount = 3;

        private short _maxRetries = _defaultMaxRetries;
        private int _retrySleep = _defaultRetrySleep;
        private bool _enableRetries = false;

        protected DbRetry()
        {
        }

        internal DbRetry(bool enableRetries)
        {
            _enableRetries = enableRetries;
        }

        internal short MaxRetries
        {
            get { return _maxRetries; }
        }

        internal bool TryDoRetry(ref short retryCount)
        {
            if (CanRetry(retryCount++))
            {
                RetrySleep(retryCount);
                return true;
            }
            else
                return false;
        }

        internal bool CanRetry(short retryCount)
        {
            if (!_enableRetries)
                return false;

            if (retryCount < _maxRetries)
                return true;
            else
                return false;
        }

        internal void RetrySleep(short retryCount)
        {
            //
            // For the first couple of retries just spin
            // If we fail _spinCount times start then introduce a sleep
            if (retryCount <= _spinCount)
                return;

            int sleep = _retrySleep * retryCount;
            Thread.Sleep(sleep);
        }
    }
}
