//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    public enum PeerMessagePropagation
    {
        None = 0x0000,
        Local = 0x0001,
        Remote = 0x0002,
        LocalAndRemote = Local | Remote
    }
}
