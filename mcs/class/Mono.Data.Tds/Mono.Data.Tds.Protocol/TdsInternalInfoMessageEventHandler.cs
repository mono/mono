//
// Mono.Data.Tds.Protocol.TdsInternalInfoMessageEventHandler.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.Tds.Protocol {
	public delegate void TdsInternalInfoMessageEventHandler (object sender, TdsInternalInfoMessageEventArgs e);
}
