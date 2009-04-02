/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Security.Cryptography;
    using System.Text;

    internal sealed class AntiForgeryData {

        private const string AntiForgeryTokenFieldName = "__RequestVerificationToken";

        private const int TokenLength = 128 / 8;
        private static RNGCryptoServiceProvider _prng = new RNGCryptoServiceProvider();

        private string _salt;
        private string _value;

        public AntiForgeryData() {
        }

        // copy constructor
        public AntiForgeryData(AntiForgeryData token) {
            if (token == null) {
                throw new ArgumentNullException("token");
            }

            CreationDate = token.CreationDate;
            Salt = token.Salt;
            Value = token.Value;
        }

        public DateTime CreationDate {
            get;
            set;
        }

        public string Salt {
            get {
                return _salt ?? String.Empty;
            }
            set {
                _salt = value;
            }
        }

        public string Value {
            get {
                return _value ?? String.Empty;
            }
            set {
                _value = value;
            }
        }

        private static string Base64EncodeForCookieName(string s) {
            byte[] rawBytes = Encoding.UTF8.GetBytes(s);
            string base64String = Convert.ToBase64String(rawBytes);

            // replace base64-specific characters with characters that are safe for a cookie name
            return base64String.Replace('+', '.').Replace('/', '-').Replace('=', '_');
        }

        private static string GenerateRandomTokenString() {
            byte[] tokenBytes = new byte[TokenLength];
            _prng.GetBytes(tokenBytes);

            string token = Convert.ToBase64String(tokenBytes);
            return token;
        }

        // If the app path is provided, we're generating a cookie name rather than a field name, and the cookie names should
        // be unique so that a development server cookie and an IIS cookie - both running on localhost - don't stomp on
        // each other.
        internal static string GetAntiForgeryTokenName(string appPath) {
            if (String.IsNullOrEmpty(appPath)) {
                return AntiForgeryTokenFieldName;
            }
            else {
                return AntiForgeryTokenFieldName + "_" + Base64EncodeForCookieName(appPath);
            }
        }

        public static AntiForgeryData NewToken() {
            string tokenString = GenerateRandomTokenString();
            return new AntiForgeryData() {
                CreationDate = DateTime.Now,
                Value = tokenString
            };
        }

    }
}
