//
// System.Data.SyntaxErrorException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {
	[Serializable]
	public class SyntaxErrorException : InvalidExpressionException
	{
		public SyntaxErrorException ()
			: base (Locale.GetText ("There is a syntax error in this Expression"))
		{
		}

		public SyntaxErrorException (string message)
			: base (message)
		{
		}

		protected SyntaxErrorException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

	}
}
