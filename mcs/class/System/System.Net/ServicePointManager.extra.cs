//
// Extra Mono-specific API for ServicePointManager
//
// Authors
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013-2014 Xamarin Inc.
//

using System;
using System.Collections.Generic;

namespace System.Net {

	[Obsolete]
	public delegate IEnumerable<string> CipherSuitesCallback (SecurityProtocolType protocol, IEnumerable<string> allCiphers);

	public partial class ServicePointManager {

		[Obsolete ("", true)]
		public static CipherSuitesCallback ClientCipherSuitesCallback { get; set; }

		[Obsolete ("", true)]
		public static CipherSuitesCallback ServerCipherSuitesCallback { get; set; }
	}
}
