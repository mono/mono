//
// System.Data.InvalidExpressionException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class InvalidExpressionException : DataException
	{
		public InvalidExpressionException ()
			: base (Locale.GetText ("This Expression is invalid"))
		{
		}

		public InvalidExpressionException (string message)
			: base (message)
		{
		}

		protected InvalidExpressionException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
