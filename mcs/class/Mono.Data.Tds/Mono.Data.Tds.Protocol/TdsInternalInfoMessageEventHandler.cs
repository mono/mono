//
// Mono.Data.TdsClient.Internal.TdsInternalInfoMessageEventHandler.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.TdsClient.Internal {
	internal delegate void TdsInternalInfoMessageEventHandler (object sender, TdsInternalInfoMessageEventArgs e);
}
