//
// System.Data.OleDb.OleDbInfoMessageEventHandler
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	[Serializable]
	public delegate void OleDbInfoMessageEventHandler (object sender, OleDbInfoMessageEventArgs e);
}
