//
// System.Data.SqlClient.SqlRowUpdatingEventHandler.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;

namespace System.Data.SqlClient
{
	public delegate void SqlRowUpdatingEventHandler(object sender,
			SqlRowUpdatingEventArgs e);
}
