// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel
{
    static class NetHttpMessageEncodingHelper
    {
        internal static bool IsDefined(NetHttpMessageEncoding value)
        {
            return
                value == NetHttpMessageEncoding.Binary
                || value == NetHttpMessageEncoding.Text
                || value == NetHttpMessageEncoding.Mtom;
        }
    }
}
