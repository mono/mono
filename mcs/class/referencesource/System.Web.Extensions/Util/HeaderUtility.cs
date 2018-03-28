//------------------------------------------------------------------------------
// <copyright file="HeaderUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Util {
    using System;

    internal static class HeaderUtility {
        public static bool IsEncodingInAcceptList(string acceptEncodingHeader, string expectedEncoding) {
            if (String.IsNullOrEmpty(acceptEncodingHeader)) {
                return false;
            }

            foreach (string encoding in acceptEncodingHeader.Split(',')) {
                string e = encoding.Trim();

                // This code will typically handle all existing browsers, which
                // use "encoding1, encoding2" for this header.
                // IE, Firefox and Safari are sending "gzip, deflate"
                // Opera is sending "deflate, gzip, x-gzip, identity, *;q=0"
                // There is a currently hypothetical case where a browser would use the quantified syntax
                // on specific encodings ("encoding1;q=0.8, encoding2 ;q=0.2") which we don't handle here.
                // For those situations, the browser would get the uncompressed version.
                // See RFC 2068 for details.
                if (String.Equals(e, expectedEncoding, StringComparison.Ordinal)) {
                    return true;
                }
            }

            // no match found
            return false;
        }

    }
}
