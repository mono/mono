//
// HttpsClientStream.cs: Glue between HttpWebRequest and SslClientStream to
//      reduce reflection usage.
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

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
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls {

        // Note: DO NOT REUSE this class - instead use SslClientStream

        internal class HttpsClientStream : SslClientStream {

                private HttpWebRequest _request;


                public HttpsClientStream (Stream stream, X509CertificateCollection clientCertificates,
					HttpWebRequest request, byte [] buffer)
                        : base (stream, request.RequestUri.Host, false, SecurityProtocolType.Default, clientCertificates)
                {
                        // this constructor permit access to the WebRequest to call
                        // ICertificatePolicy.CheckValidationResult
                        _request = request;
			if (buffer != null)
				InputBuffer.Write (buffer, 0, buffer.Length);
#if !NET_1_0
                        // also saved from reflection
                        base.CheckCertRevocationStatus = ServicePointManager.CheckCertificateRevocationList;
#endif
                }

                internal override bool RaiseServerCertificateValidation (X509Certificate certificate, int[] certificateErrors)
                {
                        bool failed = (certificateErrors.Length > 0);
                        if (ServicePointManager.CertificatePolicy != null) {
                                ServicePoint sp = _request.ServicePoint;

                                // only one problem can be reported by this interface
                                int problem = ((failed) ? certificateErrors [0] : 0);

                                return ServicePointManager.CertificatePolicy.CheckValidationResult (sp, certificate, _request, problem);
                        }
                        return failed;
                }
        }
}

