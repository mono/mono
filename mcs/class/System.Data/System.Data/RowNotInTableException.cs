//
// System.Data.RowNotInTableException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class RowNotInTableException : DataException
	{
		public RowNotInTableException ()
			: base (Locale.GetText ("This DataRow is not in this DataTable"))
		{
		}

		public RowNotInTableException (string message)
			: base (message)
		{
		}

		protected RowNotInTableException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
