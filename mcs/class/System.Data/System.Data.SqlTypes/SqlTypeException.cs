//
// System.Data.SqlTypeException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data.SqlTypes {

	[Serializable]			
	public class SqlTypeException : SystemException
	{
		
		public SqlTypeException()
			: base (Locale.GetText ("A sql exception has occured."))
		{
		}

		public SqlTypeException (string message)
			: base (message)
		{
		}

		protected SqlTypeException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
