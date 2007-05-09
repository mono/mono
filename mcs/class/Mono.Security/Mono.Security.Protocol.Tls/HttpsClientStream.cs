//
// HttpsClientStream.cs: Glue between HttpWebRequest and SslClientStream to
//      reduce reflection usage.
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2007 Novell, Inc. (http://www.novell.com)
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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls {

        // Note: DO NOT REUSE this class - instead use SslClientStream

        internal class HttpsClientStream : SslClientStream {

                private HttpWebRequest _request;
		private int _status;

                public HttpsClientStream (Stream stream, X509CertificateCollection clientCertificates,
					HttpWebRequest request, byte [] buffer)
                        : base (stream, request.RequestUri.Host, false, (Mono.Security.Protocol.Tls.SecurityProtocolType)
				ServicePointManager.SecurityProtocol, clientCertificates)
                {
                        // this constructor permit access to the WebRequest to call
                        // ICertificatePolicy.CheckValidationResult
                        _request = request;
			_status = 0;
			if (buffer != null)
				InputBuffer.Write (buffer, 0, buffer.Length);
#if !NET_1_0
                        // also saved from reflection
                        base.CheckCertRevocationStatus = ServicePointManager.CheckCertificateRevocationList;
#endif
#if NET_2_0
			ClientCertSelection += delegate (X509CertificateCollection clientCerts, X509Certificate serverCertificate,
				string targetHost, X509CertificateCollection serverRequestedCertificates) {
				return ((clientCerts == null) || (clientCerts.Count == 0)) ? null : clientCerts [0];
			};
			PrivateKeySelection += delegate (X509Certificate certificate, string targetHost) {
				X509Certificate2 cert = (certificate as X509Certificate2);
				return (cert == null) ? null : cert.PrivateKey;
			};
#endif
               }

		public bool TrustFailure {
			get { 
				switch (_status) {
				case -2146762486: // CERT_E_CHAINING		0x800B010A
				case -2146762487: // CERT_E_UNTRUSTEDROOT	0x800B0109
					return true;
				default:
					return false;
				}
			}
		}

                internal override bool RaiseServerCertificateValidation (X509Certificate certificate, int[] certificateErrors)
                {
			bool failed = (certificateErrors.Length > 0);
			// only one problem can be reported by this interface
			_status = ((failed) ? certificateErrors [0] : 0);

			if (ServicePointManager.CertificatePolicy != null) {
				ServicePoint sp = _request.ServicePoint;
				return ServicePointManager.CertificatePolicy.CheckValidationResult (sp, certificate, _request, _status);
			}
			return failed;
		}
        }
}
