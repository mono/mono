//
// ICertificateStore.cs: Interface for certificate stores.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Mono.Security.X509;

namespace Mono.Security.X509.Stores {

	public interface ICertificateStore {

		// properties

		X509CertificateCollection Certificates { get; }
		IntPtr Handle { get; }

		// methods

		void Open (string name, string location, bool readOnly, bool createIfNonExisting, bool includeArchives);
		void Close ();
		
		void Add (X509Certificate certificate);
		void Remove (X509Certificate certificate);
	}
}
