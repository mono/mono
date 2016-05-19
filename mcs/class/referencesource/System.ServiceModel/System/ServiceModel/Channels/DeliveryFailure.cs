//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    public enum DeliveryFailure
    {
        Unknown = 0,
        AccessDenied = (1 << 15) | 0x04,
        BadDestinationQueue = (1 << 15),
        BadEncryption = (1 << 15) | 0x07,
        BadSignature = (1 << 15) | 0x06,
        CouldNotEncrypt = (1 << 15) | 0x08,
        HopCountExceeded = (1 << 15) | 0x05,
        NotTransactionalQueue = (1 << 15) | 0x09,
        NotTransactionalMessage = (1 << 15) | 0x0A,
        Purged = (1 << 15) | 0x01,
        QueueDeleted = (1 << 15) | (1 << 14),
        QueueExceedMaximumSize = (1 << 15) | 0x03,
        QueuePurged = (1 << 15) | (1 << 14) | 0x01,
        ReachQueueTimeout = (1 << 15) | 0x02,
        ReceiveTimeout = (1 << 15) | (1 << 14) | 0x02,
    }
}
