//
// System.Data.OleDb.OleDbTransaction
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
	public sealed class OleDbTransaction : MarshalByRefObject,
		IDbTransaction, IDisposable
	{
	}
}
