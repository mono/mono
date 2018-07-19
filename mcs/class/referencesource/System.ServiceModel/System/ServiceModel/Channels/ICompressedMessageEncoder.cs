// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System.ServiceModel.Channels
{
    internal interface ICompressedMessageEncoder
    {
        bool CompressionEnabled { get; }

        void SetSessionContentType(string contentType);

        void AddCompressedMessageProperties(Message message, string supportedCompressionTypes);
    }
}
