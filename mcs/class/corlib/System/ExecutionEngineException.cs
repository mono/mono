//
// System.ExecutionEngineException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	[Serializable]
	public sealed class ExecutionEngineException : SystemException
	{
		// Constructors
		public ExecutionEngineException ()
			: base (Locale.GetText ("Internal error occurred.")) // Haha. Nice.
		{
		}

		public ExecutionEngineException (string message)
			: base (message)
		{
		}

		public ExecutionEngineException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
