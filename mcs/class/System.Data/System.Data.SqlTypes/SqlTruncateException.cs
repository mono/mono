//
// System.Data.SqlTruncateException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Runtime.Serialization;

namespace System.Data.SqlTypes {

	[Serializable]
	public class SqlTruncateException : SqlTypeException
	{
		public SqlTruncateException ()
			: base (Locale.GetText ("This value is being truncated"))
		{
		}

		public SqlTruncateException (string message)
			: base (message)
		{
		}
	}
}
