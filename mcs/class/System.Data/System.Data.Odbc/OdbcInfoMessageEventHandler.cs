//
// System.Data.Odbc.OdbcInfoMessageEventHandler
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc, 2004
//

using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	[Serializable]
	public delegate void OdbcInfoMessageEventHandler (object sender, OdbcInfoMessageEventArgs e);
}
