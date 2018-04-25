//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System;
    using System.ServiceModel.Channels;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Transactions;

    abstract class TransactionFormatter
    {
        static TransactionFormatter oleTxFormatter = new OleTxTransactionFormatter();
        static object syncRoot = new object();

        public static TransactionFormatter OleTxFormatter
        {
            get { return oleTxFormatter; }
        }

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile TransactionFormatter wsatFormatter10;
        public static TransactionFormatter WsatFormatter10
        {
            get
            {
                if (wsatFormatter10 == null)
                {
                    lock (syncRoot)
                    {
                        if (wsatFormatter10 == null)
                        {
                            wsatFormatter10 = new WsatTransactionFormatter10();
                        }
                    }
                }
                return wsatFormatter10;
            }
        }

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile TransactionFormatter wsatFormatter11;
        public static TransactionFormatter WsatFormatter11
        {
            get
            {
                if (wsatFormatter11 == null)
                {
                    lock (syncRoot)
                    {
                        if (wsatFormatter11 == null)
                        {
                            wsatFormatter11 = new WsatTransactionFormatter11();
                        }
                    }
                }
                return wsatFormatter11;
            }
        }

        public abstract MessageHeader EmptyTransactionHeader
        {
            get;
        }

        // Write transaction information to a message
        //
        // Return the transaction protocols that were successfully written to the message
        // Throw TransactionException if something goes wrong (e.g., TM comms failure)
        public abstract void WriteTransaction(Transaction transaction, Message message);

        // Read transaction information from a message
        // 
        // Return a TransactionInfo instance if transaction headers are present in the message
        // Return null if no transaction headers are present in the message
        // Throw TransactionException if something goes wrong (e.g., malformed XML)
        public abstract TransactionInfo ReadTransaction(Message message);
    }

    abstract class TransactionInfo
    {
        // Convert transaction information from a message into an actual transaction
        //
        // Return a transaction instance if successful (fallback down the list of protocols as needed)
        // Throw TransactionException if a could not be unmarshaled.
        //
        // Should not throw an exception
        public abstract Transaction UnmarshalTransaction();
    }
}
