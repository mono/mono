//
// System.Data.DeletedRowInaccessibleException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class DeletedRowInaccessibleException : DataException
	{
		public DeletedRowInaccessibleException ()
			: base (Locale.GetText ("This DataRow has been deleted"))
		{
		}

		public DeletedRowInaccessibleException (string message)
			: base (message)
		{
		}

		protected DeletedRowInaccessibleException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
