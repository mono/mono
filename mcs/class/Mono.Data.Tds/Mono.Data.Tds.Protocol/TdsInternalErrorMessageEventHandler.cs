//
// Mono.Data.TdsClient.Internal.TdsInternalErrorMessageEventHandler.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.TdsClient.Internal {
	internal delegate void TdsInternalErrorMessageEventHandler (object sender, TdsInternalErrorMessageEventArgs e);
}
