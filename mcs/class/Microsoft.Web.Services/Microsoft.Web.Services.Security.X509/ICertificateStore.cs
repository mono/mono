using System;

namespace Microsoft.Web.Services.Security.X509 {

	public interface ICertificateStore {

		void Close ();

		IntPtr Handle {
			get;
		}

		X509CertificateCollection GetCollection ();
	}
}
