//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication.ExtendedProtection;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    interface ISspiNegotiation : IDisposable
    {

        DateTime ExpirationTimeUtc
        {
            get;
        }
        /// <summary>
        /// This indicates if the handshake is complete or not. 
        /// Note that the IsValidContext flag indicates if the handshake ended in
        /// success or failure
        /// </summary>
        bool IsCompleted
        {
            get;
        }

        bool IsValidContext
        {
            get;
        }

        string KeyEncryptionAlgorithm
        {
            get;
        }

        byte[] Decrypt(byte[] encryptedData);

        byte[] Encrypt(byte[] data);

        byte[] GetOutgoingBlob(byte[] incomingBlob, ChannelBinding channelbinding, ExtendedProtectionPolicy protectionPolicy);

        string GetRemoteIdentityName();
    }

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    interface ISspiNegotiationInfo
    {
        ISspiNegotiation SspiNegotiation { get; }
    }
}
