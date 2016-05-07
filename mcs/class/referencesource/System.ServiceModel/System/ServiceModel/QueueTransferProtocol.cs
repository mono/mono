//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum QueueTransferProtocol
    {
        Native,
        Srmp,
        SrmpSecure
    }

    static class QueueTransferProtocolHelper
    {
        public static bool IsDefined(QueueTransferProtocol mode)
        {
            return mode >= QueueTransferProtocol.Native && mode <= QueueTransferProtocol.SrmpSecure;
        }
    }
}
