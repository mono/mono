//
// Mono.Data.SybaseClient.SybaseRowUpdatedEventHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;

namespace Mono.Data.SybaseClient {
	public delegate void SybaseRowUpdatedEventHandler (object sender, SybaseRowUpdatedEventArgs e);
}
