//	
// System.ContextMarshalException.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ContextMarshalException : SystemException
	{
		const int Result = unchecked ((int)0x80131504);

		// Constructors
		public ContextMarshalException ()
			: base (Locale.GetText ("Attempt to marshal and object across a context failed."))
		{
			HResult = Result;
		}

		public ContextMarshalException (string message)
			: base (message)
		{
			HResult = Result;
		}

		protected ContextMarshalException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public ContextMarshalException (string message, Exception innerException)
			:base (message, innerException)
		{
			HResult = Result;
		}
	}
}
