//
// System.Data.TypedDataSetGeneratorException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data {

	[Serializable]
	public class TypedDataSetGeneratorException : DataException
	{
		public TypedDataSetGeneratorException ()
			: base (Locale.GetText ("There is a name conflict"))
		{
		}

		[MonoTODO]
		public TypedDataSetGeneratorException (ArrayList list)
			: base (Locale.GetText ("There is a name conflict"))
		{
		}

		protected TypedDataSetGeneratorException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
