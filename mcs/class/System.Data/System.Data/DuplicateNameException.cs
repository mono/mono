//
// System.Data.DuplicateNameException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class DuplicateNameException : DataException
	{
		public DuplicateNameException ()
			: base (Locale.GetText ("There is a database object with the same name"))
		{
		}

		public DuplicateNameException (string message)
			: base (message)
		{
		}

		protected DuplicateNameException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
