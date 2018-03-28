//------------------------------------------------------------------------------
// <copyright file="HexParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Util {
    using System;
    using System.Globalization;
    using System.Text;

    internal static class HexParser {
        public static byte[] Parse(string token) {
            byte[] tokenBytes = new byte[token.Length / 2];
            for (int i = 0; i < tokenBytes.Length; i++) {
                tokenBytes[i] = Byte.Parse(token.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return tokenBytes;
        }

        public static string ToString(byte[] tokenBytes) {
            StringBuilder tokenBuilder = new StringBuilder(tokenBytes.Length * 2);
            for (int i = 0; i < tokenBytes.Length; i++) {
                tokenBuilder.Append(tokenBytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return tokenBuilder.ToString();
        }
    }
}
