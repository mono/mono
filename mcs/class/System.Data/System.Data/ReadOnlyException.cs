//
// System.Data.ReadOnlyException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {


	[Serializable]
	public class ReadOnlyException : DataException
	{
		public ReadOnlyException ()
			: base (Locale.GetText ("Cannot change a value in a read-only column"))
		{
		}

		public ReadOnlyException (string message)
			: base (message)
		{
		}

		protected ReadOnlyException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
