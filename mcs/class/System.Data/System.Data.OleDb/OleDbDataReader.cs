//
// System.Data.OleDb.OleDbDataReader
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbDataReader : MarshalByRefObject,
		IDataReader, IDisposable, IDataRecord, IEnumerable
	{
	}
}
