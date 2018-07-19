//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ServiceModel.Configuration;
    using System.ComponentModel;

    [TypeConverter(typeof(TransactionProtocolConverter))]
    public abstract class TransactionProtocol
    {
        public static TransactionProtocol Default
        {
            get { return OleTransactions; }
        }

        public static TransactionProtocol OleTransactions
        {
            get { return OleTransactionsProtocol.Instance; }
        }

        public static TransactionProtocol WSAtomicTransactionOctober2004
        {
            get { return WSAtomicTransactionOctober2004Protocol.Instance; }
        }

        public static TransactionProtocol WSAtomicTransaction11
        {
            get { return WSAtomicTransaction11Protocol.Instance; }
        }

        internal abstract string Name
        {
            get;
        }

        internal static bool IsDefined(TransactionProtocol transactionProtocol)
        {
            return transactionProtocol == TransactionProtocol.OleTransactions ||
                   transactionProtocol == TransactionProtocol.WSAtomicTransactionOctober2004 ||
                   transactionProtocol == TransactionProtocol.WSAtomicTransaction11;
        }
    }

    class OleTransactionsProtocol : TransactionProtocol
    {
        static TransactionProtocol instance = new OleTransactionsProtocol();

        internal static TransactionProtocol Instance
        {
            get { return instance; }
        }

        internal override string Name
        {
            get { return ConfigurationStrings.OleTransactions; }
        }
    }

    class WSAtomicTransactionOctober2004Protocol : TransactionProtocol
    {
        static TransactionProtocol instance = new WSAtomicTransactionOctober2004Protocol();

        internal static TransactionProtocol Instance
        {
            get { return instance; }
        }

        internal override string Name
        {
            get { return ConfigurationStrings.WSAtomicTransactionOctober2004; }
        }
    }

    class WSAtomicTransaction11Protocol : TransactionProtocol
    {
        static TransactionProtocol instance = new WSAtomicTransaction11Protocol();

        internal static TransactionProtocol Instance
        {
            get { return instance; }
        }

        internal override string Name
        {
            get { return ConfigurationStrings.WSAtomicTransaction11; }
        }
    }
}
