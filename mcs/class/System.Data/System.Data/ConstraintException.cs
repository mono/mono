//
// System.Data.ConstraintException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class ConstraintException : DataException
	{		
		public ConstraintException ()
			: base (Locale.GetText ("This operation violates a constraint"))
		{
		}

		public ConstraintException (string message)
			: base (message)
		{
		}

		protected ConstraintException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
