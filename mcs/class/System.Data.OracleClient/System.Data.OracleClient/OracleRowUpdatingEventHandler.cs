//
// OracleRowUpdatingEventHandler.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Parts derived from System.Data.SqlClient.SqlRowUpdatingEventHandler
// Authors:
//      Rodrigo Moya (rodrigo@ximian.com)
//      Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;

namespace System.Data.OracleClient {
	public delegate void OracleRowUpdatingEventHandler(object sender, OracleRowUpdatingEventArgs e);
}
