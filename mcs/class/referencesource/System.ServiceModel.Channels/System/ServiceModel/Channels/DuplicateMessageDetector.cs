//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Security.Cryptography;

    sealed class DuplicateMessageDetector : IDisposable
    {
        HashAlgorithm hashAlgorithm;

        [Fx.Tag.Cache(typeof(string), Fx.Tag.CacheAttrition.PartialPurgeOnEachAccess, SizeLimit = "maxListLength parameter to constructor")]
        DuplicateDetector<string> duplicateDetector;
        [Fx.Tag.SynchronizationObject()]
        object thisLock;
        bool disposed;

        public DuplicateMessageDetector(int maxListLength)
        {
            Fx.Assert(maxListLength > 0, "maxListLength must be > 0");

            this.disposed = false;
            this.hashAlgorithm = HashAlgorithm.Create();
            this.thisLock = new object();

            this.duplicateDetector = new DuplicateDetector<string>(maxListLength);
        }

        public bool IsDuplicate(ArraySegment<byte> msgBytes, out string hashString)
        {
            Fx.Assert(msgBytes != null, "messageBytes can't be null");
            Fx.Assert(msgBytes.Count > 0, "messageBytes.Count must be > 0");

            byte[] hash;

            bool notDuplicate = true;
            lock (this.thisLock)
            {
                if (disposed)
                {
                    throw FxTrace.Exception.AsError(new ObjectDisposedException(this.GetType().ToString()));
                }

                hash = this.hashAlgorithm.ComputeHash(msgBytes.Array, msgBytes.Offset, msgBytes.Count);
            }

            hashString = Convert.ToBase64String(hash);

            Fx.Assert(string.IsNullOrEmpty(hashString) == false, "computed hashstring is null or empty");

            lock (this.thisLock)
            {
                //the act of retreiving an existing item pushes it to the front of the MRU list, ensuring
                //that the oldest hashes are trimmed first when we hit the max length.
                notDuplicate = this.duplicateDetector.AddIfNotDuplicate(hashString);
            }

            return !notDuplicate;
        }

        public void RemoveEntry(string msgHash)
        {
            Fx.Assert(!string.IsNullOrEmpty(msgHash), "Message hash should never be null or empty");

            lock (this.thisLock)
            {
                if (this.disposed)
                {
                    throw FxTrace.Exception.AsError(new ObjectDisposedException(this.GetType().ToString()));
                }

                this.duplicateDetector.Remove(msgHash);
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            lock (this.thisLock)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    if (this.duplicateDetector != null)
                    {
                        this.duplicateDetector.Clear();
                    }

                    this.hashAlgorithm.Clear();
                    this.hashAlgorithm = null;
                }
            }
        }
    }
}
