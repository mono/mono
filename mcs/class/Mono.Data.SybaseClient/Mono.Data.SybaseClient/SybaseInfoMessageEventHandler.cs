//
// Mono.Data.SybaseClient.SybaseInfoMessageEventHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;

namespace Mono.Data.SybaseClient {
	public delegate void SybaseInfoMessageEventHandler (object sender, SybaseInfoMessageEventArgs e);
}
