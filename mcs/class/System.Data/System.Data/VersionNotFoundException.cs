//
// System.Data.VersionNotFoundException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]			
	public class VersionNotFoundException : DataException
	{
		public VersionNotFoundException ()
			: base (Locale.GetText ("This DataRow has been deleted"))
		{
		}

		public VersionNotFoundException (string message)
			: base (message)
		{
		}

		protected VersionNotFoundException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
