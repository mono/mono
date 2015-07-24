//
// OracleInfoMessageEventHandler.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.IO;
using System.Data.SqlTypes;

namespace System.Data.OracleClient {
	public delegate void OracleInfoMessageEventHandler (object sender, OracleInfoMessageEventArgs e);
}
