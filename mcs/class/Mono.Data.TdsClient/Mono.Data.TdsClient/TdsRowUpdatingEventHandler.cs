//
// Mono.Data.TdsClient.TdsRowUpdatingEventHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;

namespace Mono.Data.TdsClient {
	public delegate void TdsRowUpdatingEventHandler(object sender, TdsRowUpdatingEventArgs e);
}
