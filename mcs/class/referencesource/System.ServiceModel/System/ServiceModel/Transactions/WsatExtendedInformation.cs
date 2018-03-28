//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System.Transactions;

    class WsatExtendedInformation
    {
        string identifier;
        uint timeout;

        public WsatExtendedInformation(string identifier, uint timeout)
        {
            this.identifier = identifier;
            this.timeout = timeout;
        }

        public string Identifier
        {
            get { return this.identifier; }
        }

        public uint Timeout
        {
            get { return this.timeout; }
        }

        public void TryCache(Transaction tx)
        {
            Guid transactionId = tx.TransactionInformation.DistributedIdentifier;
            bool nativeId = IsNativeIdentifier(this.identifier, transactionId);
            string cacheIdentifier = nativeId ? null : this.identifier;

            if (!string.IsNullOrEmpty(cacheIdentifier) || this.timeout != 0)
            {
                // Cache extended information for subsequent marshal operations
                WsatExtendedInformationCache.Cache(tx, new WsatExtendedInformation(cacheIdentifier,
                                                                                   this.timeout));
            }
        }

        // Copied Helper method from CoordinationContext so we don't have to have this type
        public const string UuidScheme = "urn:uuid:";

        public static string CreateNativeIdentifier(Guid transactionId)
        {
            return UuidScheme + transactionId.ToString("D");
        }

        public static bool IsNativeIdentifier(string identifier, Guid transactionId)
        {
            return string.Compare(identifier,
                                  CreateNativeIdentifier(transactionId),
                                  StringComparison.Ordinal) == 0;
        }


    }
}
