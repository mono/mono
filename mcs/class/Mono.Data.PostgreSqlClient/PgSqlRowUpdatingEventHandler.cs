//
// Mono.Data.PostgreSqlClient.PgSqlRowUpdatingEventHandler.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;

namespace Mono.Data.PostgreSqlClient
{
	public delegate void PgSqlRowUpdatingEventHandler(object sender,
			PgSqlRowUpdatingEventArgs e);
}
