// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Mono.Security.Protocol.Tls
{
	internal class CipherSuiteFactory
	{
		public static CipherSuiteCollection GetSupportedCiphers(SecurityProtocolType protocol)
		{
			switch (protocol)
			{
				case SecurityProtocolType.Default:
				case SecurityProtocolType.Tls:				
					return CipherSuiteFactory.GetTls1SupportedCiphers();

				case SecurityProtocolType.Ssl3:
					return CipherSuiteFactory.GetSsl3SupportedCiphers();

				case SecurityProtocolType.Ssl2:
				default:
					throw new NotSupportedException("Unsupported security protocol type");
			}
		}

		#region Private Static Methods

		private static CipherSuiteCollection GetTls1SupportedCiphers()
		{
			CipherSuiteCollection scs = new CipherSuiteCollection(SecurityProtocolType.Tls);

			// Supported ciphers
			scs.Add((0x00 << 0x08) | 0x35, "TLS_RSA_WITH_AES_256_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, true, 32, 32, 256, 16, 16);
			scs.Add((0x00 << 0x08) | 0x2F, "TLS_RSA_WITH_AES_128_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, true, 16, 16, 128, 16, 16);
			scs.Add((0x00 << 0x08) | 0x0A, "TLS_RSA_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, true, 24, 24, 168, 8, 8);
			scs.Add((0x00 << 0x08) | 0x05, "TLS_RSA_WITH_RC4_128_SHA", CipherAlgorithmType.Rc4, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, false, 16, 16, 128, 0, 0);
			scs.Add((0x00 << 0x08) | 0x04, "TLS_RSA_WITH_RC4_128_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, false, false, 16, 16, 128, 0, 0);
			scs.Add((0x00 << 0x08) | 0x09, "TLS_RSA_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, true, 8, 8, 56, 8, 8);
			
			// Supported exportable ciphers
			scs.Add((0x00 << 0x08) | 0x03, "TLS_RSA_EXPORT_WITH_RC4_40_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, false, 5, 16, 40, 0, 0);
			scs.Add((0x00 << 0x08) | 0x06, "TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5", CipherAlgorithmType.Rc2, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, true, 5, 16, 40, 8, 8);
			scs.Add((0x00 << 0x08) | 0x08, "TLS_RSA_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, true, true, 5, 8, 40, 8, 8);
			scs.Add((0x00 << 0x08) | 0x60, "TLS_RSA_EXPORT_WITH_RC4_56_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, false, 7, 16, 56, 0, 0);
			scs.Add((0x00 << 0x08) | 0x61, "TLS_RSA_EXPORT_WITH_RC2_CBC_56_MD5", CipherAlgorithmType.Rc2, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, true, 7, 16, 56, 8, 8);
			// 56 bits but we use 64 bits because of parity (DES is really 56 bits)
			scs.Add((0x00 << 0x08) | 0x62, "TLS_RSA_EXPORT_WITH_DES_CBC_56_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, true, true, 8, 8, 64, 8, 8); 
			scs.Add((0x00 << 0x08) | 0x64, "TLS_RSA_EXPORT_WITH_RC4_56_SHA", CipherAlgorithmType.Rc4, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, true, false, 7, 16, 56, 0, 0);
			
			// Default CipherSuite
			// scs.Add(0, "TLS_NULL_WITH_NULL_NULL", CipherAlgorithmType.None, HashAlgorithmType.None, ExchangeAlgorithmType.None, true, false, 0, 0, 0, 0, 0);
			
			// RSA Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x01, "TLS_RSA_WITH_NULL_MD5", CipherAlgorithmType.None, HashAlgorithmType.Md5, ExchangeAlgorithmType.None, true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x02, "TLS_RSA_WITH_NULL_SHA", CipherAlgorithmType.None, HashAlgorithmType.Sha1, ExchangeAlgorithmType.None, true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x03, "TLS_RSA_EXPORT_WITH_RC4_40_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x05, "TLS_RSA_WITH_RC4_128_SHA", CipherAlgorithmType.Rc4, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x04, "TLS_RSA_WITH_RC4_128_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaSign, false, false, 16, 16, 128, 0, 0);			
			// scs.Add((0x00 << 0x08) | 0x06, "TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5", CipherAlgorithmType.Rc2, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, true, 5, 16, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x07, "TLS_RSA_WITH_IDEA_CBC_SHA", "IDEA", HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x08, "TLS_RSA_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x09, "TLS_RSA_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0A, "TLS_RSA_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, true, 24, 24, 168, 8, 8);

			// Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x0B, "TLS_DH_DSS_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0C, "TLS_DH_DSS_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, false, ExchangeAlgorithmType.DiffieHellman, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0D, "TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0E, "TLS_DH_RSA_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0F, "TLS_DH_RSA_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, false, ExchangeAlgorithmType.DiffieHellman, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x10, "TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x11, "TLS_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x12, "TLS_DHE_DSS_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x13, "TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x14, "TLS_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x15, "TLS_DHE_RSA_WITH_DES_CBC_SHA", HashAlgorithmType.Sha1, CipherAlgorithmType.Des, false, ExchangeAlgorithmType.DiffieHellman, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x16, "TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);

			// Anonymous Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x17, "TLS_DH_anon_EXPORT_WITH_RC4_40_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.DiffieHellman, true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x18, "TLS_DH_anon_WITH_RC4_128_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, false, ExchangeAlgorithmType.DiffieHellman, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x19, "TLS_DH_anon_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1A, "TLS_DH_anon_WITH_DES_CBC_SHA", "DES4", HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1B, "TLS_DH_anon_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);

			// AES CipherSuites
			//
			// Ref: RFC3268 - (http://www.ietf.org/rfc/rfc3268.txt)
		
			// scs.Add((0x00 << 0x08) | 0x2F, "TLS_RSA_WITH_AES_128_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, true, 16, 16, 128, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x30, "TLS_DH_DSS_WITH_AES_128_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x31, "TLS_DH_RSA_WITH_AES_128_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x32, "TLS_DHE_DSS_WITH_AES_128_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x33, "TLS_DHE_RSA_WITH_AES_128_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x34, "TLS_DH_anon_WITH_AES_128_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 16, 16, 128, 8, 8);

			// scs.Add((0x00 << 0x08) | 0x35, "TLS_RSA_WITH_AES_256_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x36, "TLS_DH_DSS_WITH_AES_256_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x37, "TLS_DH_RSA_WITH_AES_256_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x38, "TLS_DHE_DSS_WITH_AES_256_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x39, "TLS_DHE_RSA_WITH_AES_256_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x3A, "TLS_DH_anon_WITH_AES_256_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 32, 32, 256, 16, 16);

			return scs;
		}

		private static CipherSuiteCollection GetSsl3SupportedCiphers()
		{
			CipherSuiteCollection scs = new CipherSuiteCollection(SecurityProtocolType.Ssl3);

			// Supported ciphers
			scs.Add((0x00 << 0x08) | 0x35, "SSL_RSA_WITH_AES_256_CBC_SHA", CipherAlgorithmType.Rijndael, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, true, 32, 32, 256, 16, 16);
			scs.Add((0x00 << 0x08) | 0x0A, "SSL_RSA_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, true, 24, 24, 168, 8, 8);
			scs.Add((0x00 << 0x08) | 0x05, "SSL_RSA_WITH_RC4_128_SHA", CipherAlgorithmType.Rc4, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, false, 16, 16, 128, 0, 0);
			scs.Add((0x00 << 0x08) | 0x04, "SSL_RSA_WITH_RC4_128_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, false, false, 16, 16, 128, 0, 0);
			scs.Add((0x00 << 0x08) | 0x09, "SSL_RSA_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, false, true, 8, 8, 56, 8, 8);

			// Supported exportable ciphers
			scs.Add((0x00 << 0x08) | 0x03, "SSL_RSA_EXPORT_WITH_RC4_40_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, false, 5, 16, 40, 0, 0);
			scs.Add((0x00 << 0x08) | 0x06, "SSL_RSA_EXPORT_WITH_RC2_CBC_40_MD5", CipherAlgorithmType.Rc2, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, true, 5, 16, 40, 8, 8);
			scs.Add((0x00 << 0x08) | 0x08, "SSL_RSA_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, true, true, 5, 8, 40, 8, 8);
			scs.Add((0x00 << 0x08) | 0x60, "SSL_RSA_EXPORT_WITH_RC4_56_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, false, 7, 16, 56, 0, 0);
			scs.Add((0x00 << 0x08) | 0x61, "SSL_RSA_EXPORT_WITH_RC2_CBC_56_MD5", CipherAlgorithmType.Rc2, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, true, 7, 16, 56, 8, 8);
			// 56 bits but we use 64 bits because of parity (DES is really 56 bits)
			scs.Add((0x00 << 0x08) | 0x62, "SSL_RSA_EXPORT_WITH_DES_CBC_56_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, true, true, 8, 8, 64, 8, 8); 
			scs.Add((0x00 << 0x08) | 0x64, "SSL_RSA_EXPORT_WITH_RC4_56_SHA", CipherAlgorithmType.Rc4, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyX, true, false, 7, 16, 56, 0, 0);

			// Default CipherSuite
			// scs.Add(0, "SSL_NULL_WITH_NULL_NULL", CipherAlgorithmType.None, HashAlgorithmType.None, true, false, 0, 0, 0, 0, 0);
			
			// RSA Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x01, "SSL_RSA_WITH_NULL_MD5", CipherAlgorithmType.None, HashAlgorithmType.Md5, ExchangeAlgorithmType.None, true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x02, "SSL_RSA_WITH_NULL_SHA", CipherAlgorithmType.None, HashAlgorithmType.Sha1, true, ExchangeAlgorithmType.None, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x03, "SSL_RSA_EXPORT_WITH_RC4_40_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x05, "SSL_RSA_WITH_RC4_128_SHA", CipherAlgorithmType.Rc4, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x04, "SSL_RSA_WITH_RC4_128_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaSign, false, false, 16, 16, 128, 0, 0);			
			// scs.Add((0x00 << 0x08) | 0x06, "SSL_RSA_EXPORT_WITH_RC2_CBC_40_MD5", CipherAlgorithmType.Rc2, HashAlgorithmType.Md5, ExchangeAlgorithmType.RsaKeyX, true, true, 5, 16, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x07, "SSL_RSA_WITH_IDEA_CBC_SHA", "IDEA", HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x08, "SSL_RSA_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaKeyEx, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x09, "SSL_RSA_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0A, "SSL_RSA_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.RsaSign, false, true, 24, 24, 168, 8, 8);
						
			// Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x0B, "SSL_DH_DSS_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0C, "SSL_DH_DSS_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0D, "SSL_DH_DSS_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0E, "SSL_DH_RSA_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0F, "SSL_DH_RSA_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x10, "SSL_DH_RSA_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x11, "SSL_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x12, "SSL_DHE_DSS_WITH_DES_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x13, "SSL_DHE_DSS_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x14, "SSL_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x15, "SSL_DHE_RSA_WITH_DES_CBC_SHA", HashAlgorithmType.Sha1, CipherAlgorithmType.Des, ExchangeAlgorithmType.DiffieHellman, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x16, "SSL_DHE_RSA_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);

			// Anonymous Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x17, "SSL_DH_anon_EXPORT_WITH_RC4_40_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, ExchangeAlgorithmType.DiffieHellman, true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x18, "SSL_DH_anon_WITH_RC4_128_MD5", CipherAlgorithmType.Rc4, HashAlgorithmType.Md5, false, ExchangeAlgorithmType.DiffieHellman, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x19, "SSL_DH_anon_EXPORT_WITH_DES40_CBC_SHA", CipherAlgorithmType.Des, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1A, "SSL_DH_anon_WITH_DES_CBC_SHA", "DES4", HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1B, "SSL_DH_anon_WITH_3DES_EDE_CBC_SHA", CipherAlgorithmType.TripleDes, HashAlgorithmType.Sha1, ExchangeAlgorithmType.DiffieHellman, false, true, 24, 24, 168, 8, 8);

			return scs;
		}

		#endregion
	}
}
