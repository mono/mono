// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System.ServiceModel.Channels
{
    using System.ComponentModel;

    internal static class CompressionFormatHelper
    {
        public static void Validate(CompressionFormat value)
        {
            if (!IsDefined(value))
            {
                throw FxTrace.Exception.AsError(new InvalidEnumArgumentException("value", (int)value, typeof(CompressionFormat)));
            }
        }

        internal static bool IsDefined(CompressionFormat value)
        {
            return
                value == CompressionFormat.None
                || value == CompressionFormat.Deflate
                || value == CompressionFormat.GZip;
        }
    }
}
