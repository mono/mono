//------------------------------------------------------------------------------
// <copyright file="IRemoteWebConfigurationHostServer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("A99B591A-23C6-4238-8452-C7B0E895063D")]
    public interface IRemoteWebConfigurationHostServer
    {
        byte []  GetData                 (string fileName, bool getReadTimeOnly, out long readTime);
        void     WriteData               (string fileName, string templateFileName, byte[] data, ref long  readTime);
        string   GetFilePaths            (int webLevel, string path, string site, string locationSubPath);
        string   DoEncryptOrDecrypt      (bool doEncrypt, string xmlString, string protectionProviderName, string protectionProviderType, string[] parameterKeys, string [] parameterValues);
        void     GetFileDetails          (string name, out bool exists, out long size, out long createDate, out long lastWriteDate);
    }
}
