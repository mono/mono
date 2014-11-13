//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.ServiceModel;

    [ComImport,
     Guid("181b448c-c17c-4b17-ac6d-06699b93198f"),
     InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IChannelCredentials
    {
        void SetWindowsCredential(string domain, string userName, string password, int impersonationLevel, bool allowNtlm);
        void SetUserNameCredential(string userName, string password);
        void SetClientCertificateFromStore(string storeLocation, string storeName, string findType, object findValue);
        void SetClientCertificateFromStoreByName(string subjectName, string storeLocation, string storeName);
        void SetClientCertificateFromFile(string fileName, string password, string keyStorageFlags);
        void SetDefaultServiceCertificateFromStore(string storeLocation, string storeName, string findType, object findValue);
        void SetDefaultServiceCertificateFromStoreByName(string subjectName, string storeLocation, string storeName);
        void SetDefaultServiceCertificateFromFile(string fileName, string password, string keyStorageFlags);
        void SetServiceCertificateAuthentication(string storeLocation, string revocationMode, string certificationValidationMode);
        void SetIssuedToken(string localIssuerAddres, string localIssuerBindingType, string localIssuerBinding);
    }
}
