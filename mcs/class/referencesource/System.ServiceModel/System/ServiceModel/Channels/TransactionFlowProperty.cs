//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    using System.Transactions;
    using System.Runtime.Remoting.Messaging;
    using System.ServiceModel.Transactions;
    using System.ServiceModel.Diagnostics;

    sealed public class TransactionMessageProperty
    {
        TransactionInfo flowedTransactionInfo;
        Transaction flowedTransaction;
        const string PropertyName = "TransactionMessageProperty";

        private TransactionMessageProperty()
        {
        }

        public Transaction Transaction
        {
            get
            {
                if (this.flowedTransaction == null && this.flowedTransactionInfo != null)
                {
                    try
                    {
                        this.flowedTransaction = this.flowedTransactionInfo.UnmarshalTransaction();
                    }
                    catch (TransactionException e)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e);
                    }
                }
                return this.flowedTransaction;
            }
        }

        static internal TransactionMessageProperty TryGet(Message message)
        {
            if (message.Properties.ContainsKey(PropertyName))
                return message.Properties[PropertyName] as TransactionMessageProperty;
            else
                return null;
        }

        static internal Transaction TryGetTransaction(Message message)
        {
            if (!message.Properties.ContainsKey(PropertyName))
                return null;

            return ((TransactionMessageProperty)message.Properties[PropertyName]).Transaction;

        }

        static TransactionMessageProperty GetPropertyAndThrowIfAlreadySet(Message message)
        {
            if (message.Properties.ContainsKey(PropertyName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new FaultException(SR.GetString(SR.SFxTryAddMultipleTransactionsOnMessage)));
            }

            return new TransactionMessageProperty();
        }

        static public void Set(Transaction transaction, Message message)
        {
            TransactionMessageProperty property = GetPropertyAndThrowIfAlreadySet(message);
            property.flowedTransaction = transaction;
            message.Properties.Add(PropertyName, property);
        }

        static internal void Set(TransactionInfo transactionInfo, Message message)
        {
            TransactionMessageProperty property = GetPropertyAndThrowIfAlreadySet(message);
            property.flowedTransactionInfo = transactionInfo;
            message.Properties.Add(PropertyName, property);
        }
    }



    class TransactionFlowProperty
    {
        Transaction flowedTransaction;
        List<RequestSecurityTokenResponse> issuedTokens;
        const string PropertyName = "TransactionFlowProperty";

        private TransactionFlowProperty()
        {
        }

        internal ICollection<RequestSecurityTokenResponse> IssuedTokens
        {
            get
            {
                if (this.issuedTokens == null)
                {
                    this.issuedTokens = new List<RequestSecurityTokenResponse>();
                }

                return this.issuedTokens;
            }
        }

        internal Transaction Transaction
        {
            get { return this.flowedTransaction; }
        }

        static internal TransactionFlowProperty Ensure(Message message)
        {
            if (message.Properties.ContainsKey(PropertyName))
                return (TransactionFlowProperty)message.Properties[PropertyName];

            TransactionFlowProperty property = new TransactionFlowProperty();
            message.Properties.Add(PropertyName, property);
            return property;
        }

        static internal TransactionFlowProperty TryGet(Message message)
        {
            if (message.Properties.ContainsKey(PropertyName))
                return message.Properties[PropertyName] as TransactionFlowProperty;
            else
                return null;
        }

        static internal ICollection<RequestSecurityTokenResponse> TryGetIssuedTokens(Message message)
        {
            TransactionFlowProperty property = TransactionFlowProperty.TryGet(message);
            if (property == null)
                return null;

            // use this when reading only, consistently return null if no tokens.
            if (property.issuedTokens == null || property.issuedTokens.Count == 0)
                return null;

            return property.issuedTokens;
        }

        static internal Transaction TryGetTransaction(Message message)
        {
            if (!message.Properties.ContainsKey(PropertyName))
                return null;

            return ((TransactionFlowProperty)message.Properties[PropertyName]).Transaction;

        }

        static TransactionFlowProperty GetPropertyAndThrowIfAlreadySet(Message message)
        {
            TransactionFlowProperty property = TryGet(message);

            if (property != null)
            {
                if (property.flowedTransaction != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FaultException(SR.GetString(SR.SFxTryAddMultipleTransactionsOnMessage)));
                }
            }
            else
            {
                property = new TransactionFlowProperty();
            }

            return property;
        }

        static internal void Set(Transaction transaction, Message message)
        {
            TransactionFlowProperty property = GetPropertyAndThrowIfAlreadySet(message);
            property.flowedTransaction = transaction;
            message.Properties.Add(PropertyName, property);
        }
    }
}
