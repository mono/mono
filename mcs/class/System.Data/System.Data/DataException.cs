//
// System.Data.DataException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class DataException : SystemException
	{
		public DataException ()
			: base (Locale.GetText ("A Data exception has occurred"))
		{
		}

		public DataException (string message)
			: base (message)
		{
		}

		protected DataException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public DataException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
