//
// System.Data.SqlClient.SqlInfoMessageEventHandler.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;

namespace System.Data.SqlClient {
	public delegate void SqlInfoMessageEventHandler (object sender, SqlInfoMessageEventArgs e);
}
