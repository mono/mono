//
// BtlsX509Error.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
namespace Mono.Btls.Interface
{
	// Keep in sync with NativeBoringX509Error
	public enum BtlsX509Error
	{
		OK = 0,
		/* illegal error (for uninitialized values, to avoid X509_V_OK): 1 */

		UNABLE_TO_GET_ISSUER_CERT = 2,
		UNABLE_TO_GET_CRL = 3,
		UNABLE_TO_DECRYPT_CERT_SIGNATURE = 4,
		UNABLE_TO_DECRYPT_CRL_SIGNATURE = 5,
		UNABLE_TO_DECODE_ISSUER_PUBLIC_KEY = 6,
		CERT_SIGNATURE_FAILURE = 7,
		CRL_SIGNATURE_FAILURE = 8,
		CERT_NOT_YET_VALID = 9,
		CERT_HAS_EXPIRED = 10,
		CRL_NOT_YET_VALID = 11,
		CRL_HAS_EXPIRED = 12,
		ERROR_IN_CERT_NOT_BEFORE_FIELD = 13,
		ERROR_IN_CERT_NOT_AFTER_FIELD = 14,
		ERROR_IN_CRL_LAST_UPDATE_FIELD = 15,
		ERROR_IN_CRL_NEXT_UPDATE_FIELD = 16,
		OUT_OF_MEM = 17,
		DEPTH_ZERO_SELF_SIGNED_CERT = 18,
		SELF_SIGNED_CERT_IN_CHAIN = 19,
		UNABLE_TO_GET_ISSUER_CERT_LOCALLY = 20,
		UNABLE_TO_VERIFY_LEAF_SIGNATURE = 21,
		CERT_CHAIN_TOO_LONG = 22,
		CERT_REVOKED = 23,
		INVALID_CA = 24,
		PATH_LENGTH_EXCEEDED = 25,
		INVALID_PURPOSE = 26,
		CERT_UNTRUSTED = 27,
		CERT_REJECTED = 28,
		/* These are 'informational' when looking for issuer cert */
		SUBJECT_ISSUER_MISMATCH = 29,
		AKID_SKID_MISMATCH = 30,
		AKID_ISSUER_SERIAL_MISMATCH = 31,
		KEYUSAGE_NO_CERTSIGN = 32,

		UNABLE_TO_GET_CRL_ISSUER = 33,
		UNHANDLED_CRITICAL_EXTENSION = 34,
		KEYUSAGE_NO_CRL_SIGN = 35,
		UNHANDLED_CRITICAL_CRL_EXTENSION = 36,
		INVALID_NON_CA = 37,
		PROXY_PATH_LENGTH_EXCEEDED = 38,
		KEYUSAGE_NO_DIGITAL_SIGNATURE = 39,
		PROXY_CERTIFICATES_NOT_ALLOWED = 40,

		INVALID_EXTENSION = 41,
		INVALID_POLICY_EXTENSION = 42,
		NO_EXPLICIT_POLICY = 43,
		DIFFERENT_CRL_SCOPE = 44,
		UNSUPPORTED_EXTENSION_FEATURE = 45,

		UNNESTED_RESOURCE = 46,

		PERMITTED_VIOLATION = 47,
		EXCLUDED_VIOLATION = 48,
		SUBTREE_MINMAX = 49,
		UNSUPPORTED_CONSTRAINT_TYPE = 51,
		UNSUPPORTED_CONSTRAINT_SYNTAX = 52,
		UNSUPPORTED_NAME_SYNTAX = 53,
		CRL_PATH_VALIDATION_ERROR = 54,

		/* Suite B mode algorithm violation */
		SUITE_B_INVALID_VERSION = 56,
		SUITE_B_INVALID_ALGORITHM = 57,
		SUITE_B_INVALID_CURVE = 58,
		SUITE_B_INVALID_SIGNATURE_ALGORITHM = 59,
		SUITE_B_LOS_NOT_ALLOWED = 60,
		SUITE_B_CANNOT_SIGN_P_384_WITH_P_256 = 61,

		/* Host, email and IP check errors */
		HOSTNAME_MISMATCH = 62,
		EMAIL_MISMATCH = 63,
		IP_ADDRESS_MISMATCH = 64,

		/* The application is not happy */
		APPLICATION_VERIFICATION = 50
	}
}

