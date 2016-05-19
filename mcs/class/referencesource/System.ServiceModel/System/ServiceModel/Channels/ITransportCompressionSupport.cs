// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System.ServiceModel.Channels
{
    internal interface ITransportCompressionSupport
    {
        bool IsCompressionFormatSupported(CompressionFormat compressionFormat);
    }
}
