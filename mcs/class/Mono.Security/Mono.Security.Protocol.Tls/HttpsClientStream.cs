//
// HttpsClientStream.cs: Glue between HttpWebRequest and SslClientStream to
//	reduce reflection usage.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Security.Protocol.Tls {

	// Note: DO NOT REUSE this class - instead use SslClientStream

	internal class HttpsClientStream : SslClientStream {

		private string _host;
		private WebRequest _request;


		public HttpsClientStream (Stream stream, string targetHost, X509CertificateCollection clientCertificates, WebRequest request)
			: base (stream, targetHost, false, SecurityProtocolType.Default, clientCertificates)
		{
			_host = targetHost;
			// this constructor permit access to the WebRequest to call
			// ICertificatePolicy.CheckValidationResult
			_request = request;
#if !NET_1_0
			// also saved from reflection
			base.CheckCertRevocationStatus = ServicePointManager.CheckCertificateRevocationList;
#endif
		}

		internal override bool RaiseServerCertificateValidation (X509Certificate certificate, int[] certificateErrors)
		{
			bool failed = (certificateErrors.Length > 0);
			if (ServicePointManager.CertificatePolicy != null) {
				Uri target = new Uri ("https://" + _host);
				ServicePoint sp = ServicePointManager.FindServicePoint (target);

				// only one problem can be reported by this interface
				int problem = ((failed) ? certificateErrors [0] : 0);

				return ServicePointManager.CertificatePolicy.CheckValidationResult (sp, certificate, _request, problem);
			}
			return failed;
		}
	}
}
