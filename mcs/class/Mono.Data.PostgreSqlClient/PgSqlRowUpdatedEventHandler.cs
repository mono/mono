//
// Mono.Data.PostgreSqlClient.PgSqlRowUpdatedEventHandler.cs
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
	public delegate void PgSqlRowUpdatedEventHandler(object sender,
					PgSqlRowUpdatedEventArgs e);
}
