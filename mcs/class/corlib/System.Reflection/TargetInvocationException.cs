// System.Reflection.TargetInvocationException
//
// Sean MacIsaac (macisaac@ximian.com)
// Duncan Mak  (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.

using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	public sealed class TargetInvocationException : ApplicationException
	{
		public TargetInvocationException (Exception inner)
			: base ("Exception has been thrown by the target of an invocation.", inner)
		{			
		}

		public TargetInvocationException (string message, Exception inner)
			: base (message, inner)
		{
		}		

		protected TargetInvocationException (SerializationInfo info, StreamingContext sc)
			: base (info, sc)
		{
		}
	}	
}
