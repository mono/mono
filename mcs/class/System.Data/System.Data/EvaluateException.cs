//
// System.Data.EvaluateException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {
	[Serializable]
	public class EvaluateException : InvalidExpressionException
	{
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
