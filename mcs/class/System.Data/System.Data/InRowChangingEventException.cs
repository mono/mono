//
// System.Data.InRowChangingEventException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class InRowChangingEventException : DataException
	{
		public InRowChangingEventException ()
			: base (Locale.GetText ("Cannot EndEdit within a RowChanging event"))
		{
		}

		public InRowChangingEventException (string message)
			: base (message)
		{
		}

		protected InRowChangingEventException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
