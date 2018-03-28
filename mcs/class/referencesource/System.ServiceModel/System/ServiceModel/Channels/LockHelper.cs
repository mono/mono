// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Threading;

    // IMPORTANT: Only meant to be used within a using statement.
    struct LockHelper : IDisposable
    {
        ReaderWriterLockSlim readerWriterLock;
        bool isReaderLock;
        bool isLockHeld;

        LockHelper(ReaderWriterLockSlim readerWriterLock, bool isReaderLock)
        {
            this.readerWriterLock = readerWriterLock;
            this.isReaderLock = isReaderLock;

            if (isReaderLock)
            {
                this.readerWriterLock.EnterReadLock();
            }
            else
            {
                this.readerWriterLock.EnterWriteLock();
            }

            this.isLockHeld = true;
        }

        public void Dispose()
        {
            if (this.isLockHeld)
            {
                this.isLockHeld = false;
                if (this.isReaderLock)
                {
                    this.readerWriterLock.ExitReadLock();
                }
                else
                {
                    this.readerWriterLock.ExitWriteLock();
                }
            }
        }

        internal static IDisposable TakeWriterLock(ReaderWriterLockSlim readerWriterLock)
        {
            Fx.Assert(readerWriterLock != null, "The readerWriterLock cannot be null.");
            return new LockHelper(readerWriterLock, false);
        }

        internal static IDisposable TakeReaderLock(ReaderWriterLockSlim readerWriterLock)
        {
            Fx.Assert(readerWriterLock != null, "The readerWriterLock cannot be null.");
            return new LockHelper(readerWriterLock, true);
        }
    }
}
