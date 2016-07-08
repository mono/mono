//
// Extra Mono-specific API for ServicePointManager
//
// Authors
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013-2014 Xamarin Inc.
//

#if MOBILE

using System;
using System.Collections.Generic;

namespace System.Net {

	/*
	 * The idea behind this API was to let the application filter the set of cipher suites received / send to
	 * the remote side.  This concept does not any longer work with the new native implementations.
	 */

	[Obsolete ("This API is no longer supported.")]
	public delegate IEnumerable<string> CipherSuitesCallback (SecurityProtocolType protocol, IEnumerable<string> allCiphers);

	public partial class ServicePointManager {

		[Obsolete ("This API is no longer supported.", true)]
		public static CipherSuitesCallback ClientCipherSuitesCallback { get; set; }

		[Obsolete ("This API is no longer supported.", true)]
		public static CipherSuitesCallback ServerCipherSuitesCallback { get; set; }
	}
}

#endif
