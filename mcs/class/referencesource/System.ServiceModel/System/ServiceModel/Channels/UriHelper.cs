// <copyright>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    internal static class UriHelper
    {
        internal static string NormalizedHost(this Uri uri)
        {
            return uri.GetComponents(UriComponents.NormalizedHost, UriFormat.UriEscaped);
        }

        internal static string NormalizedAbsoluteUri(this Uri uri)
        {
            return uri.GetComponents(UriComponents.AbsoluteUri | UriComponents.NormalizedHost, UriFormat.UriEscaped);
        }
    }
}
