//
// System.Data.NoNullAllowedException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class NoNullAllowedException : DataException
	{
		public NoNullAllowedException ()
			: base (Locale.GetText ("Cannot insert a NULL value"))
		{
		}

		public NoNullAllowedException (string message)
			: base (message)
		{
		}

		protected NoNullAllowedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
