//
// Mono.Data.SybaseClient.SybaseRowUpdatingEventHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;

namespace Mono.Data.SybaseClient {
	public delegate void SybaseRowUpdatingEventHandler (object sender, SybaseRowUpdatingEventArgs e);
}
