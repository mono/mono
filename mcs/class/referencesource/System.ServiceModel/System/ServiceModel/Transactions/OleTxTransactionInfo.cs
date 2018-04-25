//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System.Transactions;

    class OleTxTransactionInfo : TransactionInfo
    {
        OleTxTransactionHeader header;

        public OleTxTransactionInfo(OleTxTransactionHeader header)
        {
            this.header = header;
        }

        public override Transaction UnmarshalTransaction()
        {
            Transaction tx = UnmarshalPropagationToken(header.PropagationToken);

            if (this.header.WsatExtendedInformation != null)
                this.header.WsatExtendedInformation.TryCache(tx);

            return tx;
        }

        public static Transaction UnmarshalPropagationToken(byte[] propToken)
        {
            try
            {
                return TransactionInterop.GetTransactionFromTransmitterPropagationToken(propToken);
            }
            catch (ArgumentException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TransactionException(SR.GetString(SR.InvalidPropagationToken), e));
            }
        }
    }
}
