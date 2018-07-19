//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    interface IPoisonHandlingStrategy : IDisposable
    {
        bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty);
        void FinalDisposition(MsmqMessageProperty messageProperty);
        void Open();
    }
}
