//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities
{
    using System;
    using System.Transactions;
    using System.Runtime.Serialization;

    [DataContract]
    class TransactedReceiveData
    {
        const string propertyName = "System.ServiceModel.Activities.TransactedReceiveDataExecutionPropertyName";

        public TransactedReceiveData()
        {
        }

        public static string TransactedReceiveDataExecutionPropertyName
        {
            get
            {
                return propertyName;
            }
        }

        public Transaction InitiatingTransaction
        {
            get;
            set;
        }
    }
}

