//
// Mono.Data.Tds.Protocol.TdsInternalErrorMessageEventHandler.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.Tds.Protocol {
	public delegate void TdsInternalErrorMessageEventHandler (object sender, TdsInternalErrorMessageEventArgs e);
}
