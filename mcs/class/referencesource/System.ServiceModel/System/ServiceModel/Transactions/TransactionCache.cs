//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Transactions;
    using System.Xml;

    //------------------------------------------------------------------------------------------
    //                          Transaction caches
    //------------------------------------------------------------------------------------------
    class WsatExtendedInformationCache : TransactionCache<Transaction, WsatExtendedInformation>
    {
        public static void Cache(Transaction tx, WsatExtendedInformation info)
        {
            WsatExtendedInformationCache entry = new WsatExtendedInformationCache();
            entry.AddEntry(tx, tx, info);
        }
    }

    class WsatIncomingTransactionCache : TransactionCache<string, Transaction>
    {
        public static void Cache(string identifier, Transaction tx)
        {
            WsatIncomingTransactionCache entry = new WsatIncomingTransactionCache();
            entry.AddEntry(tx, identifier, tx);
        }
    }
    
    abstract class TransactionCache<T, S>
    {
        static Dictionary<T, S> cache = new Dictionary<T, S>();
        static ReaderWriterLock cacheLock = new ReaderWriterLock();

        T key;

        protected void AddEntry(Transaction transaction, T key, S value)
        {
            this.key = key;

            if (Add(key, value))
            {
                transaction.TransactionCompleted += new TransactionCompletedEventHandler(OnTransactionCompleted);
            }
        }

        void OnTransactionCompleted(object sender, TransactionEventArgs e)
        {
            Remove(this.key);
        }

        static bool Add(T key, S value)
        {
            bool lockHeld = false;
            try
            {
                try { }
                finally
                {
                    cacheLock.AcquireWriterLock(Timeout.Infinite);
                    lockHeld = true;
                }

                if (!cache.ContainsKey(key))
                {
                    cache.Add(key, value);
                    return true;
                }
            }
            finally
            {
                if (lockHeld)
                {
                    cacheLock.ReleaseWriterLock();
                }
            }

            return false;
        }

        static void Remove(T key)
        {
            bool lockHeld = false;
            try
            {
                try { }
                finally
                {
                    cacheLock.AcquireWriterLock(Timeout.Infinite);
                    lockHeld = true;
                }

                bool remove = cache.Remove(key);
                if (!(remove))
                {
                    // tx processing requires failfast when state is inconsistent
                    DiagnosticUtility.FailFast("TransactionCache: key must be present in transaction cache");
                }
            }
            finally
            {
                if (lockHeld)
                {
                    cacheLock.ReleaseWriterLock();
                }
            }

        }

        public static bool Find(T key, out S value)
        {
            bool lockHeld = false;
            try
            {
                try { }
                finally
                {
                    cacheLock.AcquireReaderLock(Timeout.Infinite);
                    lockHeld = true;
                }

                if (cache.TryGetValue(key, out value))
                {
                    return true;
                }
            }
            finally
            {
                if (lockHeld)
                {
                    cacheLock.ReleaseReaderLock();
                }
            }

            return false;
        }
    }
}
