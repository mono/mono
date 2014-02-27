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

	public delegate IEnumerable<string> CipherSuitesCallback (SecurityProtocolType protocol, IEnumerable<string> allCiphers);

	public partial class ServicePointManager {

		public static CipherSuitesCallback ClientCipherSuitesCallback { get; set; }

		public static CipherSuitesCallback ServerCipherSuitesCallback { get; set; }
	}
}