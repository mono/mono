//
// System.Data.TypedDataSetGeneratorException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class TypedDataSetGeneratorException : DataException
	{
		public TypedDataSetGeneratorException ()
			: base (Locale.GetText ("There is a name conflict"))
		{
		}

		public TypedDataSetGeneratorException (string message)
			: base (message)
		{
		}

		protected TypedDataSetGeneratorException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
