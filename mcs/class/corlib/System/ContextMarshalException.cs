//	
// System.ContextMarshalException.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class ContextMarshalException : SystemException
	{
		// Constructors
		public ContextMarshalException ()
			: base (Locale.GetText ("Attempt to marshal and object across a context failed."))
		{
		}

		public ContextMarshalException (string message)
			: base (message)
		{
		}
		
		protected ContextMarshalException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		public ContextMarshalException (string message, Exception innerException)
			:base (message, innerException)
		{
		}
	}
}
