//-----------------------------------------------------------------------
// <copyright file="UriUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel
{
    using System;

    internal static class UriUtil
    {
        /// <summary>
        /// Determines whether a URI is valid and can be created using the specified UriKind.
        /// Uri.TryCreate is used here, which is more lax than Uri.IsWellFormedUriString.
        /// The reason we use this function is because IsWellFormedUriString will reject valid URIs if they are IPv6 or require escaping.
        /// </summary>
        /// <param name="uriString">The string to check.</param>
        /// <param name="uriKind">The type of URI (usually UriKind.Absolute)</param>
        /// <returns>True if the URI is valid, false otherwise.</returns>
        public static bool CanCreateValidUri(string uriString, UriKind uriKind)
        {
            Uri tempUri;

            return TryCreateValidUri(uriString, uriKind, out tempUri);
        }

        /// <summary>
        /// Determines whether a URI is valid and can be created using the specified UriKind.
        /// Uri.TryCreate is used here, which is more lax than Uri.IsWellFormedUriString.
        /// The reason we use this function is because IsWellFormedUriString will reject valid URIs if they are IPv6 or require escaping.
        /// </summary>
        /// <param name="uriString">The string to check.</param>
        /// <param name="uriKind">The type of URI (usually UriKind.Absolute)</param>
        /// <param name="result">An out param representing the created URI</param>
        /// <returns>True if the URI is valid, false otherwise.</returns>
        public static bool TryCreateValidUri(string uriString, UriKind uriKind, out Uri result)
        {
            return Uri.TryCreate(uriString, uriKind, out result);
        }
    }
}
