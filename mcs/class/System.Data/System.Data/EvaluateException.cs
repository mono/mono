//
// System.Data.EvaluateException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Runtime.Serialization;

namespace System.Data {
	public class EvaluateException : InvalidExpressionException
	{
		[Serializable]
		public EvaluateException ()
			: base (Locale.GetText ("This expression cannot be evaluated"))
		{
		}

		public EvaluateException (string message)
			: base (message)
		{
		}

		protected EvaluateException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
