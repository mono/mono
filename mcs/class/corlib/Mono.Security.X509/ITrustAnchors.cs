//
// ITrustAnchors.cs: Trust Anchors Interface
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Mono.Security.X509 {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	interface ITrustAnchors
	{
		X509CertificateCollection Anchors { get; }
	}
}
