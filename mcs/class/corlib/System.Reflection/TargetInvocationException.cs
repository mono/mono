// System.Reflection.TargetInvocationException
//
// Sean MacIsaac (macisaac@ximian.com)
// Duncan Mak  (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.

namespace System.Reflection
{
	[Serializable]
	public sealed class TargetInvocationException : ApplicationException
	{
		public TargetInvocationException (Exception inner)
			: base (inner)
		{			
		}

		public TargetInvocationException (string message, Exception inner)
			: base (message, inner)
		{
		}		
	}	
}
