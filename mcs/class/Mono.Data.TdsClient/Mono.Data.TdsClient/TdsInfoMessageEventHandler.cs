//
// Mono.Data.TdsClient.TdsInfoMessageEventHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;

namespace Mono.Data.TdsClient {
	public delegate void TdsInfoMessageEventHandler (object sender, TdsInfoMessageEventArgs e);
}
