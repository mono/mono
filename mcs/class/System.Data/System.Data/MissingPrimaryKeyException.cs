//
// System.Data.MissingPrimaryKeyException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {


	[Serializable]
	public class MissingPrimaryKeyException : DataException
	{
		public MissingPrimaryKeyException ()
			: base (Locale.GetText ("This table has no primary key"))
		{
		}

		public MissingPrimaryKeyException (string message)
			: base (message)
		{
		}

		protected MissingPrimaryKeyException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
