//
// System.Data.InvalidConstraintException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]			
	public class InvalidConstraintException : DataException
	{
		public InvalidConstraintException ()
			: base (Locale.GetText ("Cannot access or create this relation"))
		{
		}

		public InvalidConstraintException (string message)
			: base (message)
		{
		}

		protected InvalidConstraintException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
