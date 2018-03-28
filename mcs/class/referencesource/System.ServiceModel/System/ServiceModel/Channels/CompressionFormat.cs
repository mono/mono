// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System.ServiceModel.Channels
{
    /// <summary>
    /// Supported compression formats
    /// </summary>
    public enum CompressionFormat
    {
        /// <summary>
        /// Default to compression off
        /// </summary>
        None,

        /// <summary>
        /// GZip compression
        /// </summary>
        GZip,

        /// <summary>
        /// Deflate compression
        /// </summary>
        Deflate,
    }
}
